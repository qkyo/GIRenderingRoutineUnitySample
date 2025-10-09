using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Denoising;
using q_common;
using Unity.Mathematics;

namespace Rendering.RayTrace
{
    public partial class RayTracingRenderPipeline : RenderPipeline
    {
        #region Ray-Tracing-Declaration
        private readonly string cmdName = "Ray Trace Render Graph";

        private RayTracingRenderPipelineAsset asset;
        public CubeMapSetting cubeMapSetting;
        public TemporalVisibility temporalVisibility;

        private RayTracingAccelerationStructure accelerationStructure;
        public RayTracingShader rayGenAndMissShader;

        CommandBufferDenoiser denoiser;
        Denoiser.State denoiserState;

        Texture2D dstTexture;
        CommandBuffer cmd;

        private readonly Dictionary<int, ComputeBuffer> PRNGStates = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> weld_v_allPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> vtIdxPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> weld_vtIdxBufferPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> weld_vtIdx_mapBufferPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> deseiBufferPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> desesBufferPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> desVibilityBufferPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> vfEdgeMapBufferPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> ls_vBufferPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> ls_vnBufferPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> satTexBufferPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, RTHandle> outputTargets = new Dictionary<int, RTHandle>();
        private readonly Dictionary<int, Vector4> outputTargetSizes = new Dictionary<int, Vector4>();

        public int frameIndex = 0;
        #endregion

        public RayTracingRenderPipeline(RayTracingRenderPipelineAsset asset)
        {
            // Config Setting through scriptable object 
            this.asset = asset;
            this.cubeMapSetting = asset.cubeMapSetting;
            this.rayGenAndMissShader = asset.rtShader;

            InitResource();
        }

        // Only run in the first frame
        public void InitResource()
        {
            InitEnvmap();
            InitDenoiser(1366, 768);
            temporalVisibility = new TemporalVisibility(this);
            accelerationStructure = new RayTracingAccelerationStructure();
            InitAccelerationStructure();
            dstTexture = new Texture2D(1366, 768, GraphicsFormat.R32G32B32A32_SFloat, TextureCreationFlags.None);
        }

        // Only run in every frames
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {

            BeginFrameRendering(context, cameras);

            System.Array.Sort(cameras, (lhs, rhs) => (int)(lhs.depth - rhs.depth));
            temporalVisibility.RegenVisibility();
            BuildAccelerationStructure(ref temporalVisibility.q_obj);

            foreach (var camera in cameras)
            {
                // Only render game and scene view camera.
                //if (camera.cameraType != CameraType.Game && camera.cameraType != CameraType.SceneView)
                if (camera.cameraType != CameraType.Game)
                    continue;

                BeginCameraRendering(context, camera);
                SetupCamera(camera);
                cmd = CommandBufferPool.Get(cmdName);
                RTHandle rtOutput = RenderPathTrace(camera, cmd, asset);

                if (RayTracingRenderPipelineAsset.EnableDenoise)
                {
                    RenderDenoise(context, cmd, rtOutput, ref dstTexture);
                    FinalBlitPass(cmd, dstTexture);
                }
                else
                {
                    FinalBlitPass(cmd, rtOutput);
                }
#if UNITY_EDITOR
                if (camera.cameraType == UnityEngine.CameraType.SceneView)
                {
                    ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
                }
#endif

                if (camera.cameraType == CameraType.Game)
                    frameIndex++;

                context.ExecuteCommandBuffer(cmd);
                context.Submit();
                EndCameraRendering(context, camera);
            }

            EndFrameRendering(context, cameras);
        }

        public void DisposeVisbility()
        {
            ls_vBuffer?.Release();
            ls_vnBuffer?.Release();
            vtIdxBuffer?.Release();
            weld_vtIdxBuffer?.Release();
            weld_vtIdx_mapBuffer?.Release();
            vfEdgeMapBuffer?.Release();
            deseiBuffer?.Release();
            desesBuffer?.Release();
            desVibilityBuffer?.Release();

            foreach (var pair in deseiBufferPairs)
            {
                pair.Value.Release();
            }
            deseiBufferPairs.Clear();

            foreach (var pair in desesBufferPairs)
            {
                pair.Value.Release();
            }
            desesBufferPairs.Clear();

            foreach (var pair in desVibilityBufferPairs)
            {
                pair.Value.Release();
            }
            desVibilityBufferPairs.Clear();

            foreach (var pair in vfEdgeMapBufferPairs)
            {
                pair.Value.Release();
            }
            vfEdgeMapBufferPairs.Clear();

            foreach (var pair in weld_vtIdxBufferPairs)
            {
                pair.Value.Release();
            }
            weld_vtIdxBufferPairs.Clear();

            foreach (var pair in ls_vBufferPairs)
            {
                pair.Value.Release();
            }
            ls_vBufferPairs.Clear();

            foreach (var pair in ls_vnBufferPairs)
            {
                pair.Value.Release();
            }
            ls_vnBufferPairs.Clear();

            foreach (var pair in weld_v_allPairs)
            {
                pair.Value.Release();
            }
            weld_v_allPairs.Clear();

            foreach (var pair in vtIdxPairs)
            {
                pair.Value.Release();
            }
            vtIdxPairs.Clear();

            foreach (var pair in weld_vtIdx_mapBufferPairs)
            {
                pair.Value.Release();
            }
            weld_vtIdx_mapBufferPairs.Clear();
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var pair in PRNGStates)
            {
                pair.Value.Release();
            }
            PRNGStates.Clear();

            if (null != accelerationStructure)
            {
                accelerationStructure.Dispose();
                accelerationStructure = null;
            }

            foreach (var pair in outputTargets)
            {
                RTHandles.Release(pair.Value);
            }
            outputTargets.Clear();

            foreach (var pair in deseiBufferPairs)
            {
                pair.Value.Release();
            }
            deseiBufferPairs.Clear();

            foreach (var pair in desesBufferPairs)
            {
                pair.Value.Release();
            }
            desesBufferPairs.Clear();

            foreach (var pair in desVibilityBufferPairs)
            {
                pair.Value.Release();
            }
            desVibilityBufferPairs.Clear();

            foreach (var pair in vfEdgeMapBufferPairs)
            {
                pair.Value.Release();
            }
            vfEdgeMapBufferPairs.Clear();

            foreach (var pair in weld_vtIdxBufferPairs)
            {
                pair.Value.Release();
            }
            weld_vtIdxBufferPairs.Clear();

            foreach (var pair in ls_vBufferPairs)
            {
                pair.Value.Release();
            }
            ls_vBufferPairs.Clear();

            foreach (var pair in ls_vnBufferPairs)
            {
                pair.Value.Release();
            }
            ls_vnBufferPairs.Clear();

            foreach (var pair in weld_v_allPairs)
            {
                pair.Value.Release();
            }
            weld_v_allPairs.Clear();

            foreach (var pair in vtIdxPairs)
            {
                pair.Value.Release();
            }
            vtIdxPairs.Clear();

            foreach (var pair in weld_vtIdx_mapBufferPairs)
            {
                pair.Value.Release();
            }
            weld_vtIdx_mapBufferPairs.Clear();

        }
    }
}