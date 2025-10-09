using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Rendering.RayTrace
{
    public partial class RayTracingRenderPipeline
    {
        const int maxNumSubMeshes = 32;
        private bool[] subMeshFlagArray = new bool[maxNumSubMeshes];
        private bool[] subMeshCutoffArray = new bool[maxNumSubMeshes];

        private void InitAccelerationStructure()
        {
            for (var i = 0; i < maxNumSubMeshes; ++i)
            {
                subMeshFlagArray[i] = true;
                subMeshCutoffArray[i] = false;
            }
        }

        private void BuildAccelerationStructure(ref GameObject mergeGO)
        {
            accelerationStructure.Dispose();
            accelerationStructure = new RayTracingAccelerationStructure();

            FillAccelerationStructure(ref accelerationStructure, ref mergeGO);

            accelerationStructure.Build();
        }

        private void FillAccelerationStructure(ref RayTracingAccelerationStructure accelerationStructure, ref GameObject mergeGO) 
        {
            Renderer m_renderer = mergeGO.GetComponent<Renderer>();
            if (m_renderer)
            {
                accelerationStructure.AddInstance(m_renderer, subMeshFlagArray, subMeshCutoffArray);
            }
        } 
    }
}