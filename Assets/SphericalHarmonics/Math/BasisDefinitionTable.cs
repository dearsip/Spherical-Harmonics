using System;
using System.Collections.Generic;

namespace SphericalHarmonics.Math
{
    public enum RealBasisType { Sine, Zonal, Cosine }

    public sealed class BasisDefinition
    {
        public int L { get; }
        public int M { get; }
        public RealBasisType Type { get; }
        public string Label { get; }
        public string YCombination { get; }
        public double Normalization { get; }
        public string NormalizationExact { get; }
        public string SphericalShape { get; }
        public string CartesianShape { get; }
        public string ComplexFormula { get; }

        public BasisDefinition(int l, int m, RealBasisType type, string label, string combination,
            double normalization, string normalizationExact, string sphericalShape, string cartesianShape,
            string complexFormula)
        {
            L = l; M = m; Type = type; Label = label; YCombination = combination;
            Normalization = normalization; NormalizationExact = normalizationExact;
            SphericalShape = sphericalShape; CartesianShape = cartesianShape; ComplexFormula = complexFormula;
        }
    }

    /// <summary>
    /// Machine-readable constants from spherical_harmonics_constants_l0_l3.json, represented as an
    /// immutable runtime table so WebGL does not depend on filesystem access.
    /// </summary>
    public static class BasisDefinitionTable
    {
        private static readonly Dictionary<int, BasisDefinition> Definitions = Build();

        public static BasisDefinition Get(int l, int m)
        {
            Validate(l, m);
            return Definitions[Key(l, m)];
        }

        public static IEnumerable<BasisDefinition> All => Definitions.Values;

        public static string FormulaCard(int l, int m, bool complex, bool normalized)
        {
            BasisDefinition d = Get(l, m);
            if (complex) return normalized ? d.ComplexFormula : $"Y({l},{m}) ∝ {ComplexShape(l, m)}";
            if (normalized) return $"R({l},{m}) = {d.NormalizationExact} × ({d.SphericalShape})";
            return $"R({l},{m}) ∝ {d.YCombination} ∝ {d.SphericalShape} ∝ {d.CartesianShape}";
        }

        public static void Validate(int l, int m)
        {
            if (l < 0 || l > 3 || m < -l || m > l)
                throw new ArgumentOutOfRangeException($"Invalid spherical harmonic (l,m)=({l},{m}); expected 0≤l≤3 and -l≤m≤l.");
        }

        private static int Key(int l, int m) => l * 10 + m + 3;

        private static string ComplexShape(int l, int m)
        {
            string[] shapes = {
                "1",
                "sinθ e^{-iφ}", "cosθ", "sinθ e^{iφ}",
                "sin²θ e^{-2iφ}", "sinθ cosθ e^{-iφ}", "3cos²θ-1", "sinθ cosθ e^{iφ}", "sin²θ e^{2iφ}",
                "sin³θ e^{-3iφ}", "sin²θ cosθ e^{-2iφ}", "sinθ(5cos²θ-1)e^{-iφ}", "5cos³θ-3cosθ", "sinθ(5cos²θ-1)e^{iφ}", "sin²θ cosθ e^{2iφ}", "sin³θ e^{3iφ}"
            };
            int index = l * l + m + l;
            return shapes[index];
        }

        private static Dictionary<int, BasisDefinition> Build()
        {
            var d = new Dictionary<int, BasisDefinition>();
            Add(d,0,0,"s","Y₀⁰",0.28209479177387814,"1/(2√π)","1","1","Y₀⁰ = 1/(2√π)");
            Add(d,1,-1,"p_y","i(Y₁⁻¹ + Y₁¹)/√2",0.4886025119029199,"1/2√(3/π)","sinθ sinφ","y/r","Y₁⁻¹ = +1/2√(3/(2π)) e^{-iφ} sinθ");
            Add(d,1,0,"p_z","Y₁⁰",0.4886025119029199,"1/2√(3/π)","cosθ","z/r","Y₁⁰ = 1/2√(3/π) cosθ");
            Add(d,1,1,"p_x","(Y₁⁻¹ - Y₁¹)/√2",0.4886025119029199,"1/2√(3/π)","sinθ cosφ","x/r","Y₁¹ = -1/2√(3/(2π)) e^{iφ} sinθ");
            Add(d,2,-2,"d_xy","i(Y₂⁻² - Y₂²)/√2",0.5462742152960396,"1/4√(15/π)","sin²θ sin2φ","2xy/r²","Y₂⁻² = 1/4√(15/(2π)) e^{-2iφ} sin²θ");
            Add(d,2,-1,"d_yz","i(Y₂⁻¹ + Y₂¹)/√2",1.0925484305920792,"1/2√(15/π)","sinθ cosθ sinφ","yz/r²","Y₂⁻¹ = +1/2√(15/(2π)) e^{-iφ} sinθ cosθ");
            Add(d,2,0,"d_(3z²-r²)","Y₂⁰",0.31539156525252005,"1/4√(5/π)","3cos²θ-1","(3z²-r²)/r²","Y₂⁰ = 1/4√(5/π)(3cos²θ-1)");
            Add(d,2,1,"d_xz","(Y₂⁻¹ - Y₂¹)/√2",1.0925484305920792,"1/2√(15/π)","sinθ cosθ cosφ","xz/r²","Y₂¹ = -1/2√(15/(2π)) e^{iφ} sinθ cosθ");
            Add(d,2,2,"d_(x²-y²)","(Y₂⁻² + Y₂²)/√2",0.5462742152960396,"1/4√(15/π)","sin²θ cos2φ","(x²-y²)/r²","Y₂² = 1/4√(15/(2π)) e^{2iφ} sin²θ");
            Add(d,3,-3,"f_y(3x²-y²)","i(Y₃⁻³ + Y₃³)/√2",0.5900435899266435,"1/4√(35/(2π))","sin³θ sin3φ","y(3x²-y²)/r³","Y₃⁻³ = +1/8√(35/π) e^{-3iφ} sin³θ");
            Add(d,3,-2,"f_xyz","i(Y₃⁻² - Y₃²)/√2",1.445305721320277,"1/4√(105/π)","sin²θ cosθ sin2φ","2xyz/r³","Y₃⁻² = 1/4√(105/(2π)) e^{-2iφ} sin²θ cosθ");
            Add(d,3,-1,"f_y(5z²-r²)","i(Y₃⁻¹ + Y₃¹)/√2",0.4570457994644658,"1/4√(21/(2π))","sinθ(5cos²θ-1) sinφ","y(5z²-r²)/r³","Y₃⁻¹ = +1/8√(21/π) e^{-iφ} sinθ(5cos²θ-1)");
            Add(d,3,0,"f_z(5z²-3r²)","Y₃⁰",0.3731763325901154,"1/4√(7/π)","5cos³θ-3cosθ","z(5z²-3r²)/r³","Y₃⁰ = 1/4√(7/π)(5cos³θ-3cosθ)");
            Add(d,3,1,"f_x(5z²-r²)","(Y₃⁻¹ - Y₃¹)/√2",0.4570457994644658,"1/4√(21/(2π))","sinθ(5cos²θ-1) cosφ","x(5z²-r²)/r³","Y₃¹ = -1/8√(21/π) e^{iφ} sinθ(5cos²θ-1)");
            Add(d,3,2,"f_z(x²-y²)","(Y₃⁻² + Y₃²)/√2",1.445305721320277,"1/4√(105/π)","sin²θ cosθ cos2φ","z(x²-y²)/r³","Y₃² = 1/4√(105/(2π)) e^{2iφ} sin²θ cosθ");
            Add(d,3,3,"f_x(x²-3y²)","(Y₃⁻³ - Y₃³)/√2",0.5900435899266435,"1/4√(35/(2π))","sin³θ cos3φ","x(x²-3y²)/r³","Y₃³ = -1/8√(35/π) e^{3iφ} sin³θ");
            return d;
        }

        private static void Add(Dictionary<int, BasisDefinition> d, int l, int m, string label, string combination,
            double n, string nExact, string spherical, string cartesian, string complex)
        {
            RealBasisType type = m < 0 ? RealBasisType.Sine : m > 0 ? RealBasisType.Cosine : RealBasisType.Zonal;
            d.Add(Key(l, m), new BasisDefinition(l, m, type, label, combination, n, nExact, spherical, cartesian, complex));
        }
    }
}
