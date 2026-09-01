using UnityEngine;

namespace SphericalHarmonics.Math
{
    /// <summary>Maps mathematical (x,y,z) to displayed Unity (x,z,y), with mathematical +z visually upward.</summary>
    public static class CoordinateSpace
    {
        public static Vector3 ToWorld(Vector3 mathematical, Quaternion coordinateFrame) =>
            BaseToWorld(coordinateFrame * mathematical);

        public static Vector3 ToMathematical(Vector3 world, Quaternion coordinateFrame) =>
            Quaternion.Inverse(coordinateFrame) * BaseToWorld(world);

        public static Quaternion FrameToWorld(Quaternion coordinateFrame) => RotationToWorld(coordinateFrame);

        public static Quaternion ActiveRotationToWorld(Quaternion coordinateFrame, Quaternion localRotation)
        {
            return RotationToWorld(coordinateFrame*localRotation*Quaternion.Inverse(coordinateFrame));
        }

        public static Vector3 AxisInWorld(Quaternion coordinateFrame, Vector3 mathematicalAxis) =>
            ToWorld(mathematicalAxis,coordinateFrame);

        private static Vector3 BaseToWorld(Vector3 value)=>new Vector3(value.x,value.z,value.y);

        private static Quaternion RotationToWorld(Quaternion mathematicalRotation)
        {
            Vector3 forward=BaseToWorld(mathematicalRotation*Vector3.up);
            Vector3 up=BaseToWorld(mathematicalRotation*Vector3.forward);
            return Quaternion.LookRotation(forward,up);
        }
    }
}
