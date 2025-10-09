using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Denoising;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Unity.Collections;

namespace Rendering.RayTrace
{
    public partial class RayTracingRenderPipeline
    {
        public void RenderDenoise(ScriptableRenderContext context, CommandBuffer cmd, RTHandle src, ref Texture2D dstTexture)
        {

            using (new ProfilingScope(cmd, new ProfilingSampler("Denoising")))
            {
                //denoiser = new CommandBufferDenoiser();
                //result = denoiser.Init(DenoiserType.Optix, 1366, 768);
                //// Create a new denoise request for a color image stored in a Render Texture
                denoiser.DenoiseRequest(cmd, "color", src);

                //// Wait until the denoising request is done executing
                denoiserState = denoiser.WaitForCompletion(context, cmd);

                // var dst = new RenderTexture(denoiseSrc.descriptor);
                denoiserState = denoiser.GetResults(cmd, ref dstTexture);
                // Graphics.Blit(dstTexture, dst);

            }
        }

        void InitDenoiser(int fixWidth, int fixHeight)
        {
            denoiser = new CommandBufferDenoiser();
            //denoiserState = denoiser.Init(DenoiserType.Optix, Screen.width, Screen.height);
            denoiserState = denoiser.Init(DenoiserType.Optix, 1366, 768);
        }
    }
}