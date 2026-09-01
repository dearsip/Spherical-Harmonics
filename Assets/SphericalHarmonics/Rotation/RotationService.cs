using System;
using UnityEngine;
using SphericalHarmonics.Math;
using SphericalHarmonics.State;

namespace SphericalHarmonics.Rotation
{
    /// <summary>
    /// Rotates each l-subspace independently by orthogonal projection on a deterministic Fibonacci grid.
    /// This is a compact, convention-safe l≤3 equivalent of applying Wigner-D matrices.
    /// </summary>
    public static class RotationService
    {
        public static RealCoefficientBank RotateRealActive(RealCoefficientBank source, Quaternion rotation, int sampleCount = 4096) =>
            ProjectReal(source, Quaternion.Inverse(rotation), sampleCount);

        public static RealCoefficientBank ReexpressRealInRotatedCoordinates(RealCoefficientBank source, Quaternion coordinatesRotation, int sampleCount = 4096) =>
            ProjectReal(source, coordinatesRotation, sampleCount);

        public static ComplexCoefficientBank RotateComplexActive(ComplexCoefficientBank source, Quaternion rotation, int sampleCount = 4096) =>
            ProjectComplex(source, Quaternion.Inverse(rotation), sampleCount);

        public static ComplexCoefficientBank ReexpressComplexInRotatedCoordinates(ComplexCoefficientBank source, Quaternion coordinatesRotation, int sampleCount = 4096) =>
            ProjectComplex(source, coordinatesRotation, sampleCount);

        private static RealCoefficientBank ProjectReal(RealCoefficientBank source, Quaternion argumentRotation, int sampleCount)
        {
            if (sampleCount < 64) throw new ArgumentOutOfRangeException(nameof(sampleCount));
            var result = new RealCoefficientBank();
            double weight = 4.0 * System.Math.PI / sampleCount;
            for (int l = 0; l <= 3; l++)
            {
                double[] sums = new double[2 * l + 1];
                for (int i = 0; i < sampleCount; i++)
                {
                    Vector3 n = FibonacciDirection(i, sampleCount);
                    double value = SphericalHarmonicEvaluator.EvaluateRealDegree(source, l, argumentRotation * n);
                    for (int m = -l; m <= l; m++) sums[m + l] += value * SphericalHarmonicEvaluator.RealBasis(l, m, n);
                }
                for (int m = -l; m <= l; m++) result[l, m] = weight * sums[m + l];
            }
            return result;
        }

        private static ComplexCoefficientBank ProjectComplex(ComplexCoefficientBank source, Quaternion argumentRotation, int sampleCount)
        {
            if (sampleCount < 64) throw new ArgumentOutOfRangeException(nameof(sampleCount));
            var result = new ComplexCoefficientBank();
            double weight = 4.0 * System.Math.PI / sampleCount;
            for (int l = 0; l <= 3; l++)
            {
                var sums = new ComplexValue[2 * l + 1];
                for (int i = 0; i < sampleCount; i++)
                {
                    Vector3 n = FibonacciDirection(i, sampleCount);
                    ComplexValue value = EvaluateComplexDegree(source, l, argumentRotation * n);
                    for (int m = -l; m <= l; m++) sums[m + l] += SphericalHarmonicEvaluator.ComplexBasis(l, m, n).Conjugate * value;
                }
                for (int m = -l; m <= l; m++) result[l, m] = sums[m + l] * weight;
            }
            return result;
        }

        private static ComplexValue EvaluateComplexDegree(ComplexCoefficientBank source, int l, Vector3 n)
        {
            var value = new ComplexValue();
            for (int m = -l; m <= l; m++) value += source[l, m] * SphericalHarmonicEvaluator.ComplexBasis(l, m, n);
            return value;
        }

        public static Vector3 FibonacciDirection(int i, int count)
        {
            double y = 1.0 - 2.0 * (i + 0.5) / count;
            double radius = System.Math.Sqrt(System.Math.Max(0.0, 1.0 - y * y));
            double phi = i * System.Math.PI * (3.0 - System.Math.Sqrt(5.0));
            return new Vector3((float)(radius * System.Math.Cos(phi)), (float)(radius * System.Math.Sin(phi)), (float)y);
        }
    }
}
