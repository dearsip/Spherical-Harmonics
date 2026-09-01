using UnityEngine;

namespace SphericalHarmonics.Fourier
{
    public static class FourierCurves
    {
        public static Vector3 BridgeBasePoint(float phi,float circleAmount,float lineHalfLength,bool orbital)
        {
            circleAmount=Mathf.Clamp01(circleAmount);
            Vector3 lineBase=new Vector3(lineHalfLength*(phi/Mathf.PI-1f),0,0);
            float circleAngle=-phi-Mathf.PI*.5f;
            Vector3 circleBase=orbital?Vector3.zero:new Vector3(Mathf.Cos(circleAngle),Mathf.Sin(circleAngle),0);
            return Vector3.Lerp(lineBase,circleBase,circleAmount);
        }

        public static Vector3 BridgePoint(float phi,float circleAmount,float lineHalfLength,float realDisplacement,float imaginaryDisplacement=0,bool orbital=false)
        {
            circleAmount=Mathf.Clamp01(circleAmount);
            // This is the same final circle direction as -phi-PI/2, but this
            // unwrapped branch makes the line midpoint stationary while the
            // two ends turn by +PI and -PI respectively during the morph.
            float unwrappedCircleAngle=Mathf.PI*1.5f-phi;
            float normalAngle=Mathf.Lerp(Mathf.PI*.5f,unwrappedCircleAngle,circleAmount);
            Vector3 normal=new Vector3(Mathf.Cos(normalAngle),Mathf.Sin(normalAngle),0);
            return BridgeBasePoint(phi,circleAmount,lineHalfLength,orbital)+normal*realDisplacement+Vector3.forward*imaginaryDisplacement;
        }

        public static Vector3 ComplexHelix(float phi, int m, float amplitude, float phase) =>
            new Vector3(phi, amplitude * Mathf.Cos(m * phi + phase), amplitude * Mathf.Sin(m * phi + phase));

        public static float RealValue(float phi, int m, float amplitude)
        {
            int n = Mathf.Abs(m);
            return amplitude * (m < 0 ? Mathf.Sin(n * phi) : Mathf.Cos(n * phi));
        }
    }
}
