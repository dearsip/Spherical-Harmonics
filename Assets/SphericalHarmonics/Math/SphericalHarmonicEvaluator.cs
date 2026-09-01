using System;
using UnityEngine;
using SphericalHarmonics.State;

namespace SphericalHarmonics.Math
{
    public static class SphericalHarmonicEvaluator
    {
        private const double FourPi = 4.0 * System.Math.PI;

        public static ComplexValue ComplexBasis(int l, int m, Vector3 direction)
        {
            BasisDefinitionTable.Validate(l, m);
            Vector3 n = direction.normalized;
            if (n.sqrMagnitude < 0.5f) return new ComplexValue();
            int k = System.Math.Abs(m);
            double p = AssociatedLegendre(l, k, n.z);
            double normalization = System.Math.Sqrt((2.0 * l + 1.0) / FourPi * Factorial(l - k) / Factorial(l + k));
            double phi = System.Math.Atan2(n.y, n.x);
            var positive = ComplexValue.FromPolar(normalization * p, k * phi);
            if (m >= 0) return positive;
            return ((k & 1) == 0 ? 1.0 : -1.0) * positive.Conjugate;
        }

        public static double RealBasis(int l, int m, Vector3 direction)
        {
            if (m == 0) return ComplexBasis(l, 0, direction).Real;
            int k = System.Math.Abs(m);
            ComplexValue y = ComplexBasis(l, k, direction);
            double sign = (k & 1) == 0 ? 1.0 : -1.0;
            return System.Math.Sqrt(2.0) * sign * (m > 0 ? y.Real : y.Imaginary);
        }

        public static double EvaluateReal(RealCoefficientBank bank, Vector3 direction)
        {
            double sum = 0;
            for (int l = 0; l <= 3; l++)
                for (int m = -l; m <= l; m++) sum += bank[l, m] * RealBasis(l, m, direction);
            return sum;
        }

        public static double EvaluateRealDegree(RealCoefficientBank bank, int l, Vector3 direction)
        {
            double sum = 0;
            for (int m = -l; m <= l; m++) sum += bank[l, m] * RealBasis(l, m, direction);
            return sum;
        }

        public static ComplexValue EvaluateComplex(ComplexCoefficientBank bank, Vector3 direction)
        {
            var sum = new ComplexValue();
            for (int l = 0; l <= 3; l++)
                for (int m = -l; m <= l; m++) sum += bank[l, m] * ComplexBasis(l, m, direction);
            return sum;
        }

        private static double AssociatedLegendre(int l, int m, double x)
        {
            double pmm = 1.0;
            if (m > 0)
            {
                double root = System.Math.Sqrt(System.Math.Max(0.0, 1.0 - x * x));
                double factor = 1.0;
                for (int i = 1; i <= m; i++) { pmm *= -factor * root; factor += 2.0; }
            }
            if (l == m) return pmm;
            double pmmp1 = x * (2.0 * m + 1.0) * pmm;
            if (l == m + 1) return pmmp1;
            double previous = pmm, current = pmmp1;
            for (int degree = m + 2; degree <= l; degree++)
            {
                double next = ((2.0 * degree - 1.0) * x * current - (degree + m - 1.0) * previous) / (degree - m);
                previous = current; current = next;
            }
            return current;
        }

        private static double Factorial(int n)
        {
            double value = 1;
            for (int i = 2; i <= n; i++) value *= i;
            return value;
        }
    }
}
