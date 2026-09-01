using System;
using UnityEngine;

namespace SphericalHarmonics.State
{
    public enum DisplayMode { Sphere, Orbital, Flow }
    public enum ValueType { Real, Complex }
    public enum BridgeStage { Sphere, Circle, Line }

    public sealed class ViewerState
    {
        public readonly RealCoefficientBank Real = new RealCoefficientBank();
        public readonly ComplexCoefficientBank Complex = new ComplexCoefficientBank();
        public DisplayMode Display = DisplayMode.Sphere;
        public ValueType Value = ValueType.Real;
        public int L = 1;
        public int M = 0;
        public bool PureMode = true;
        public bool ShowAxes = true;
        public bool ShowFunctionSurface = true;
        public bool ShowFunctionWireframe;
        public bool ShowUnitSphereSurface;
        public bool ShowUnitSphereWireframe = true;
        public bool ShowNormalVectors;
        public bool DirectionalLight = true;
        public float FlowTime = .35f;
        public Quaternion CoordinateFrame = Quaternion.identity;
        public event Action Changed;

        public ViewerState() { Real.Pure(1, 0); Complex.Pure(1, 0); }

        public void NotifyChanged() => Changed?.Invoke();
        public void Select(int l, int m)
        {
            L = Mathf.Clamp(l, 0, 3); M = Mathf.Clamp(m, -L, L);
            if (PureMode) MakePureSelected();
            NotifyChanged();
        }
        public void SetPureMode(bool enabled)
        {
            PureMode = enabled;
            if (enabled) MakePureSelected();
            NotifyChanged();
        }
        public void ZeroSelected()
        {
            if (Value == ValueType.Real) Real[L, M] = 0;
            else Complex[L, M] = new SphericalHarmonics.Math.ComplexValue();
            NotifyChanged();
        }
        public void MakePureSelected()
        {
            if (Value == ValueType.Real) Real.Pure(L, M);
            else Complex.Pure(L, M);
        }
        public bool TrySetValueType(ValueType type)
        {
            if (Display == DisplayMode.Flow && type == ValueType.Complex) return false;
            Value = type;
            if (PureMode) MakePureSelected();
            NotifyChanged(); return true;
        }
        public bool TrySetDisplay(DisplayMode mode)
        {
            if (mode == DisplayMode.Flow && Value == ValueType.Complex) Value = ValueType.Real;
            Display = mode;
            NotifyChanged(); return true;
        }
    }
}
