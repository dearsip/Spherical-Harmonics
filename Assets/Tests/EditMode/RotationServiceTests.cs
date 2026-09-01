using NUnit.Framework;
using UnityEngine;
using SphericalHarmonics.Math;
using SphericalHarmonics.Rotation;
using SphericalHarmonics.State;

namespace SphericalHarmonics.Tests
{
    public sealed class RotationServiceTests
    {
        [Test]
        public void ActiveRotationMatchesRotatedSamplePointsAndDoesNotMixL()
        {
            var source = new RealCoefficientBank();
            source[2,-2] = 0.35; source[2,0] = -0.7; source[2,1] = 1.1;
            Quaternion q = Quaternion.Euler(31, -22, 47);
            RealCoefficientBank rotated = RotationService.RotateRealActive(source, q, 4096);
            Vector3[] points = { Vector3.right, Vector3.up, Vector3.forward, new Vector3(.3f,.5f,.8f).normalized };
            foreach (Vector3 n in points)
                Assert.That(SphericalHarmonicEvaluator.EvaluateReal(rotated,n), Is.EqualTo(SphericalHarmonicEvaluator.EvaluateReal(source,Quaternion.Inverse(q)*n)).Within(2e-3));
            for (int l = 0; l <= 3; l++) if (l != 2) for (int m = -l; m <= l; m++) Assert.That(rotated[l,m], Is.EqualTo(0).Within(2e-3));
        }

        [Test]
        public void PassiveReexpressionPreservesWorldFunctionWhenAxesRotate()
        {
            var source = new RealCoefficientBank(); source[1,1] = 1; source[3,-1] = .4;
            Quaternion axes = Quaternion.Euler(-13, 54, 9);
            RealCoefficientBank expressed = RotationService.ReexpressRealInRotatedCoordinates(source, axes, 4096);
            foreach (Vector3 world in new[] { Vector3.right, Vector3.up, new Vector3(.2f,.7f,-.4f).normalized })
            {
                Vector3 coordinates = Quaternion.Inverse(axes) * world;
                Assert.That(SphericalHarmonicEvaluator.EvaluateReal(expressed, coordinates), Is.EqualTo(SphericalHarmonicEvaluator.EvaluateReal(source, world)).Within(3e-3));
            }
        }

        [Test]
        public void ComplexActiveRotationMatchesRotatedSamplePoints()
        {
            var source=new ComplexCoefficientBank();source[2,-1]=ComplexValue.FromPolar(.7,.4);source[2,2]=ComplexValue.FromPolar(1.1,-.8);
            Quaternion q=Quaternion.Euler(17,-38,21);ComplexCoefficientBank rotated=RotationService.RotateComplexActive(source,q,4096);
            foreach(Vector3 n in new[]{Vector3.right,Vector3.up,new Vector3(.4f,-.2f,.7f).normalized})
            {
                ComplexValue actual=SphericalHarmonicEvaluator.EvaluateComplex(rotated,n);ComplexValue expected=SphericalHarmonicEvaluator.EvaluateComplex(source,Quaternion.Inverse(q)*n);
                Assert.That(actual.Real,Is.EqualTo(expected.Real).Within(3e-3));Assert.That(actual.Imaginary,Is.EqualTo(expected.Imaginary).Within(3e-3));
            }
        }
    }
}
