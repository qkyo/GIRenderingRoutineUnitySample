﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Unity.Mathematics;
using System;

namespace Rendering.RayTrace
{
    public partial class RayTracingRenderPipeline
    {
        public ComputeBuffer RequirePRNGStates(Camera camera)
        {
            var id = camera.GetInstanceID();
            if (PRNGStates.TryGetValue(id, out var buffer))
                return buffer;

            buffer = new ComputeBuffer(Screen.width * Screen.height, 4 * 4, ComputeBufferType.Structured, ComputeBufferMode.Immutable);

            var _mt19937 = new MersenneTwister.MT.mt19937ar_cok_opt_t();
            _mt19937.init_genrand((uint)System.DateTime.Now.Ticks);

            var data = new uint[Screen.width * Screen.height * 4];
            for (var i = 0; i < Screen.width * Screen.height * 4; ++i)
                data[i] = _mt19937.genrand_int32();
            buffer.SetData(data);

            PRNGStates.Add(id, buffer);

            return buffer;
        }

        public ComputeBuffer RequireBufferData(Camera camera, List<int> intList, Dictionary<int, ComputeBuffer> pairs)
        {
            var id = camera.GetInstanceID();

            if (pairs.TryGetValue(id, out var m_buffer))
                return m_buffer;

            m_buffer = new ComputeBuffer(intList.Count, sizeof(int), ComputeBufferType.Raw, ComputeBufferMode.Immutable);
            m_buffer.SetData(intList);

            pairs.Add(id, m_buffer);

            return m_buffer;
        }

        public ComputeBuffer RequireBufferData(Camera camera, int[] intList, Dictionary<int, ComputeBuffer> pairs)
        {
            var id = camera.GetInstanceID();

            if (pairs.TryGetValue(id, out var m_buffer))
                return m_buffer;

            m_buffer = new ComputeBuffer(intList.Length, sizeof(int), ComputeBufferType.Raw, ComputeBufferMode.Immutable);
            m_buffer.SetData(intList);

            pairs.Add(id, m_buffer);

            return m_buffer;
        }

        public ComputeBuffer RequireBufferData(Camera camera, float[] floatList, Dictionary<int, ComputeBuffer> pairs)
        {
            var id = camera.GetInstanceID();

            if (pairs.TryGetValue(id, out var m_buffer))
                return m_buffer;

            m_buffer = new ComputeBuffer(floatList.Length, sizeof(int), ComputeBufferType.Raw, ComputeBufferMode.Immutable);
            m_buffer.SetData(floatList);

            pairs.Add(id, m_buffer);

            return m_buffer;
        }


        public ComputeBuffer RequireBufferData(Camera camera, List<float> floatList, Dictionary<int, ComputeBuffer> pairs)
        {
            var id = camera.GetInstanceID();

            if (pairs.TryGetValue(id, out var m_buffer))
                return m_buffer;

            m_buffer = new ComputeBuffer(floatList.Count, sizeof(float), ComputeBufferType.Raw, ComputeBufferMode.Immutable);
            m_buffer.SetData(floatList);
            //Debug.Log(m_buffer.count);

            pairs.Add(id, m_buffer);

            return m_buffer;
        }

        public ComputeBuffer RequireBufferData(Camera camera, List<Vector3> vecList, Dictionary<int, ComputeBuffer> pairs)
        {
            var id = camera.GetInstanceID();

            if (pairs.TryGetValue(id, out var m_buffer))
                return m_buffer;

            m_buffer = new ComputeBuffer(vecList.Count, sizeof(float) * 3);
            m_buffer.SetData(vecList);

            pairs.Add(id, m_buffer);

            return m_buffer;
        }

        public ComputeBuffer RequireBufferData(Camera camera, Vector3[] vecList, Dictionary<int, ComputeBuffer> pairs)
        {
            var id = camera.GetInstanceID();

            if (pairs.TryGetValue(id, out var m_buffer))
                return m_buffer;

            m_buffer = new ComputeBuffer(vecList.Length, sizeof(float) * 3);
            m_buffer.SetData(vecList);

            pairs.Add(id, m_buffer);

            return m_buffer;
        }

        public ComputeBuffer RequireBufferData(Camera camera, List<Vector4> vecList, Dictionary<int, ComputeBuffer> pairs)
        {
            var id = camera.GetInstanceID();

            if (pairs.TryGetValue(id, out var m_buffer))
                return m_buffer;

            m_buffer = new ComputeBuffer(vecList.Count, sizeof(float)*4);
            m_buffer.SetData(vecList);

            pairs.Add(id, m_buffer);

            return m_buffer;
        }

        /// <summary>
        /// Require a output target for camera.
        /// </summary>
        /// <param name="camera">the camera.</param>
        /// <returns>the output target.</returns>
        protected RTHandle RequireOutputTarget(Camera camera)
        {
            var id = camera.GetInstanceID();

            if (outputTargets.TryGetValue(id, out var outputTarget))
                return outputTarget;

            outputTarget = RTHandles.Alloc(
              camera.pixelWidth,
              camera.pixelHeight,
              1,
              DepthBits.None,
              GraphicsFormat.R32G32B32A32_SFloat,
              FilterMode.Point,
              TextureWrapMode.Clamp,
              TextureDimension.Tex2D,
              true,
              false,
              false,
              false,
              1,
              0f,
              MSAASamples.None,
              false,
              false,
              RenderTextureMemoryless.None,
              $"OutputTarget_{camera.name}");
            outputTargets.Add(id, outputTarget);

            return outputTarget;
        }

        /// <summary>
        /// require a output target size for camera.
        /// </summary>
        /// <param name="camera">the camera.</param>
        /// <returns>the output target size.</returns>
        protected Vector4 RequireOutputTargetSize(Camera camera)
        {

            var id = camera.GetInstanceID();

            if (outputTargetSizes.TryGetValue(id, out var outputTargetSize))
                return outputTargetSize;

            outputTargetSize = new Vector4(camera.pixelWidth, camera.pixelHeight, 1.0f / camera.pixelWidth, 1.0f / camera.pixelHeight);
            outputTargetSizes.Add(id, outputTargetSize);

            return outputTargetSize;
        }
    }
}