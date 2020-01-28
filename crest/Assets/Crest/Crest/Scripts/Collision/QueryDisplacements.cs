﻿// Crest Ocean System

// This file is subject to the MIT License as seen in the root of this folder structure (LICENSE)

using Unity.Collections;
using UnityEngine;

namespace Crest
{
    /// <summary>
    /// Samples water surface shape - displacement, height, normal, velocity.
    /// </summary>
    public class QueryDisplacements : QueryBase, ICollProvider
    {
        readonly static int sp_LD_TexArray_AnimatedWaves = Shader.PropertyToID("_LD_TexArray_AnimatedWaves");
        readonly static int sp_ResultDisplacements = Shader.PropertyToID("_ResultDisplacements");

        protected override string QueryShaderName => "QueryDisplacements";
        protected override string QueryKernelName => "CSMain";

        public static QueryDisplacements Instance { get; private set; }

        protected override void OnEnable()
        {
            Debug.Assert(Instance == null);
            Instance = this;

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            Instance = null;

            base.OnDisable();
        }

        protected override void BindInputsAndOutputs(PropertyWrapperComputeStandalone wrapper, ComputeBuffer resultsBuffer)
        {
            OceanRenderer.Instance._lodDataAnimWaves.BindResultData(wrapper);
            ShaderProcessQueries.SetTexture(_kernelHandle, sp_LD_TexArray_AnimatedWaves, OceanRenderer.Instance._lodDataAnimWaves.DataTexture);
            ShaderProcessQueries.SetBuffer(_kernelHandle, sp_ResultDisplacements, resultsBuffer);
        }

        public int Query(int i_ownerHash, float i_minSpatialLength, NativeSlice<Vector3> i_queryPoints, NativeSlice<float> o_resultHeights, NativeSlice<Vector3> o_resultNorms, NativeSlice<Vector3> o_resultVels)
        {
            var result = (int)QueryStatus.OK;

            if (!UpdateQueryPoints(i_ownerHash, i_minSpatialLength, i_queryPoints, o_resultNorms.Length != 0 ? i_queryPoints : o_resultNorms))
            {
                result |= (int)QueryStatus.PostFailed;
            }

            if (!RetrieveResults(i_ownerHash, default, o_resultHeights, o_resultNorms))
            {
                result |= (int)QueryStatus.RetrieveFailed;
            }

            if (o_resultVels.Length != 0)
            {
                result |= CalculateVelocities(i_ownerHash, i_minSpatialLength, i_queryPoints, o_resultVels);
            }

            return result;
        }
    }
}
