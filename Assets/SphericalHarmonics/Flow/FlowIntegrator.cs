using UnityEngine;
using SphericalHarmonics.State;

namespace SphericalHarmonics.Flow
{
    public sealed class FlowIntegrator
    {
        public int Steps { get; set; } = 32;

        public Vector3[] IntegrateFromReference(RealCoefficientBank bank, Vector3[] referenceVertices, float time)
        {
            var output = new Vector3[referenceVertices.Length];
            for (int i = 0; i < output.Length; i++) output[i] = FlowFieldBuilder.Integrate(bank, referenceVertices[i], time, Steps);
            return output;
        }
    }
}
