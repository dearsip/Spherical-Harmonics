using System;
using SphericalHarmonics.Math;

namespace SphericalHarmonics.State
{
    [Serializable]
    public sealed class RealCoefficientBank
    {
        private readonly double[] values = new double[16];
        public double this[int l, int m] { get { BasisDefinitionTable.Validate(l,m); return values[Index(l,m)]; } set { BasisDefinitionTable.Validate(l,m); values[Index(l,m)] = value; } }
        public void Clear() => Array.Clear(values, 0, values.Length);
        public void Pure(int l, int m, double amplitude = 1.0) { Clear(); this[l, m] = amplitude; }
        public RealCoefficientBank Clone() { var copy = new RealCoefficientBank(); Array.Copy(values, copy.values, 16); return copy; }
        public void CopyFrom(RealCoefficientBank source) => Array.Copy(source.values, values, 16);
        internal static int Index(int l, int m) => l * l + m + l;
    }

    [Serializable]
    public sealed class ComplexCoefficientBank
    {
        private readonly ComplexValue[] values = new ComplexValue[16];
        public ComplexValue this[int l, int m] { get { BasisDefinitionTable.Validate(l,m); return values[RealCoefficientBank.Index(l,m)]; } set { BasisDefinitionTable.Validate(l,m); values[RealCoefficientBank.Index(l,m)] = value; } }
        public void Clear() => Array.Clear(values, 0, values.Length);
        public void Pure(int l, int m, double magnitude = 1.0, double phase = 0.0) { Clear(); this[l, m] = ComplexValue.FromPolar(magnitude, phase); }
        public ComplexCoefficientBank Clone() { var copy = new ComplexCoefficientBank(); Array.Copy(values, copy.values, 16); return copy; }
        public void CopyFrom(ComplexCoefficientBank source) => Array.Copy(source.values, values, 16);
    }
}
