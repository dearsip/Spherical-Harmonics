using UnityEngine;

namespace SphericalHarmonics.Input
{
    public static class CameraGestureMath
    {
        public static Vector2 TouchOrbitDelta(Vector2 screenDelta,float speed)
            =>new Vector2(-screenDelta.x*speed,screenDelta.y*speed);
    }
}
