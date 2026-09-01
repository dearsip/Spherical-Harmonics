using UnityEngine;
using SphericalHarmonics.Math;
using SphericalHarmonics.State;

namespace SphericalHarmonics.Flow
{
    public static class FlowFieldBuilder
    {
        private const float GradientStep = 0.0005f;

        public static Vector3 Velocity(RealCoefficientBank bank, Vector3 position)
        {
            Vector3 velocity = Vector3.zero;
            double f0 = bank[0, 0] * BasisDefinitionTable.Get(0, 0).Normalization;
            velocity += (float)f0 * position;
            for (int l = 1; l <= 3; l++)
                for (int m = -l; m <= l; m++)
                {
                    double coefficient = bank[l, m];
                    if (System.Math.Abs(coefficient) < 1e-14) continue;
                    velocity += (float)(coefficient / l) * GradientSolidHarmonic(l, m, position);
                }
            return velocity;
        }

        public static Vector3 Integrate(RealCoefficientBank bank, Vector3 referencePosition, float time, int steps)
        {
            if (Mathf.Approximately(time, 0f)) return referencePosition;
            steps = Mathf.Max(1, steps);
            float h = time / steps;
            Vector3 p = referencePosition;
            for (int i = 0; i < steps; i++)
            {
                Vector3 k1 = Velocity(bank, p);
                Vector3 k2 = Velocity(bank, p + 0.5f * h * k1);
                Vector3 k3 = Velocity(bank, p + 0.5f * h * k2);
                Vector3 k4 = Velocity(bank, p + h * k3);
                p += h * (k1 + 2f * k2 + 2f * k3 + k4) / 6f;
            }
            return p;
        }

        public static double InitialNormalVelocity(RealCoefficientBank bank, Vector3 unitNormal) =>
            Vector3.Dot(unitNormal.normalized, Velocity(bank, unitNormal.normalized));

        public static double SolidHarmonic(int l, int m, Vector3 p)
        {
            BasisDefinition d = BasisDefinitionTable.Get(l, m);
            double x = p.x, y = p.y, z = p.z;
            double r2 = x * x + y * y + z * z;
            double shape;
            if (l == 0) shape = 1;
            else if (l == 1) shape = m == -1 ? y : m == 0 ? z : x;
            else if (l == 2)
            {
                switch (m) { case -2: shape = 2*x*y; break; case -1: shape = y*z; break; case 0: shape = 3*z*z-r2; break; case 1: shape = x*z; break; default: shape = x*x-y*y; break; }
            }
            else
            {
                switch (m)
                {
                    case -3: shape = y*(3*x*x-y*y); break;
                    case -2: shape = 2*x*y*z; break;
                    case -1: shape = y*(5*z*z-r2); break;
                    case 0: shape = z*(5*z*z-3*r2); break;
                    case 1: shape = x*(5*z*z-r2); break;
                    case 2: shape = z*(x*x-y*y); break;
                    default: shape = x*(x*x-3*y*y); break;
                }
            }
            return d.Normalization * shape;
        }

        private static Vector3 GradientSolidHarmonic(int l, int m, Vector3 p)
        {
            Vector3 dx = new Vector3(GradientStep, 0, 0), dy = new Vector3(0, GradientStep, 0), dz = new Vector3(0, 0, GradientStep);
            double denominator = 2.0 * GradientStep;
            return new Vector3(
                (float)((SolidHarmonic(l,m,p+dx)-SolidHarmonic(l,m,p-dx))/denominator),
                (float)((SolidHarmonic(l,m,p+dy)-SolidHarmonic(l,m,p-dy))/denominator),
                (float)((SolidHarmonic(l,m,p+dz)-SolidHarmonic(l,m,p-dz))/denominator));
        }
    }
}
