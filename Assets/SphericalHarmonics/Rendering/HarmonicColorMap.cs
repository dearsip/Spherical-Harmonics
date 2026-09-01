using UnityEngine;
using SphericalHarmonics.Math;

namespace SphericalHarmonics.Rendering
{
    public static class HarmonicColorMap
    {
        public static Color Real(double value)
        {
            float strength=1f-Mathf.Exp(-(float)System.Math.Abs(value)*2.5f);
            Color target=value>=0?new Color(.18f,.55f,1f):new Color(1f,.3f,.18f);
            return Color.Lerp(Color.white,target,strength);
        }

        public static Color RealCurve(double value)
        {
            float saturation=(1f-Mathf.Exp(-(float)System.Math.Abs(value)*2.5f))*.82f;
            float hue=value>=0?.59f:.015f;
            return Color.HSVToRGB(hue,saturation,1f);
        }

        public static Color Complex(ComplexValue value,float phaseEpsilon=.035f)
        {
            float saturation=Mathf.SmoothStep(0,1,(float)(value.Magnitude/phaseEpsilon));
            if(saturation<=0)return Color.white;
            float hue=(float)(value.Phase/(2*System.Math.PI));hue-=Mathf.Floor(hue);
            return Color.Lerp(Color.white,Color.HSVToRGB(hue,.92f,.98f),saturation);
        }
    }
}
