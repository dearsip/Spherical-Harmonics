using System;

namespace SphericalHarmonics.Math
{
    [Serializable]
    public struct ComplexValue
    {
        public double Real;
        public double Imaginary;

        public ComplexValue(double real, double imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }

        public double Magnitude => System.Math.Sqrt(Real * Real + Imaginary * Imaginary);
        public double Phase => System.Math.Atan2(Imaginary, Real);
        public ComplexValue Conjugate => new ComplexValue(Real, -Imaginary);

        public static ComplexValue FromPolar(double magnitude, double phase) =>
            new ComplexValue(magnitude * System.Math.Cos(phase), magnitude * System.Math.Sin(phase));

        public static ComplexValue operator +(ComplexValue a, ComplexValue b) => new ComplexValue(a.Real + b.Real, a.Imaginary + b.Imaginary);
        public static ComplexValue operator -(ComplexValue a, ComplexValue b) => new ComplexValue(a.Real - b.Real, a.Imaginary - b.Imaginary);
        public static ComplexValue operator -(ComplexValue value) => new ComplexValue(-value.Real, -value.Imaginary);
        public static ComplexValue operator *(ComplexValue a, ComplexValue b) => new ComplexValue(a.Real * b.Real - a.Imaginary * b.Imaginary, a.Real * b.Imaginary + a.Imaginary * b.Real);
        public static ComplexValue operator *(ComplexValue value, double scalar) => new ComplexValue(value.Real * scalar, value.Imaginary * scalar);
        public static ComplexValue operator *(double scalar, ComplexValue value) => value * scalar;
        public static ComplexValue operator /(ComplexValue value, double scalar) => new ComplexValue(value.Real / scalar, value.Imaginary / scalar);
    }
}
