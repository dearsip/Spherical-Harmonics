using NUnit.Framework;
using UnityEngine;
using SphericalHarmonics.Flow;
using SphericalHarmonics.Math;
using SphericalHarmonics.Rotation;
using SphericalHarmonics.State;

namespace SphericalHarmonics.Tests
{
    public sealed class FlowFieldTests
    {
        [Test]
        public void InitialNormalVelocityEqualsRealField()
        {
            var bank = new RealCoefficientBank();
            bank[0,0]=.2; bank[1,1]=-.7; bank[2,-2]=.45; bank[3,1]=.31;
            for (int i=0;i<64;i++)
            {
                Vector3 n=RotationService.FibonacciDirection(i,64);
                Assert.That(FlowFieldBuilder.InitialNormalVelocity(bank,n), Is.EqualTo(SphericalHarmonicEvaluator.EvaluateReal(bank,n)).Within(8e-4));
            }
        }

        [Test]
        public void PureL1IsTranslation()
        {
            var bank = new RealCoefficientBank(); bank[1,1]=1;
            Vector3 deltaA=FlowFieldBuilder.Integrate(bank,Vector3.up,.4f,16)-Vector3.up;
            Vector3 deltaB=FlowFieldBuilder.Integrate(bank,Vector3.forward,.4f,16)-Vector3.forward;
            Assert.That((deltaA-deltaB).magnitude, Is.LessThan(2e-4));
        }
    }
}
