using NUnit.Framework;
using UnityEngine;
using SphericalHarmonics.Fourier;

namespace SphericalHarmonics.Tests
{
    public sealed class FourierCurveTests
    {
        [Test]
        public void ComplexMThreeHasThreeTurnsAndOppositeHandedness()
        {
            Vector3 plusQuarter=FourierCurves.ComplexHelix(Mathf.PI/6,3,1,0);
            Vector3 minusQuarter=FourierCurves.ComplexHelix(Mathf.PI/6,-3,1,0);
            Assert.That(plusQuarter.z, Is.EqualTo(1).Within(1e-6));
            Assert.That(minusQuarter.z, Is.EqualTo(-1).Within(1e-6));
            Vector3 end=FourierCurves.ComplexHelix(2*Mathf.PI,3,1,0);
            Assert.That(end.y, Is.EqualTo(1).Within(1e-5));
            Assert.That(end.z, Is.EqualTo(0).Within(1e-5));
        }

        [Test]
        public void BridgeMorphHasMatchingLineAndCircleDisplacement()
        {
            const float displacement=.3f,halfLength=2.05f;
            Vector3 line=FourierCurves.BridgePoint(0,0,halfLength,displacement);
            Assert.That(line.x,Is.EqualTo(-halfLength).Within(1e-6));
            Assert.That(line.y,Is.EqualTo(displacement).Within(1e-6));
            Assert.That(line.z,Is.EqualTo(0).Within(1e-6));

            Vector3 circle=FourierCurves.BridgePoint(0,1,halfLength,displacement);
            Assert.That(circle.x,Is.EqualTo(0).Within(1e-6));
            Assert.That(circle.y,Is.EqualTo(-1-displacement).Within(1e-6));
            Assert.That(circle.z,Is.EqualTo(0).Within(1e-6));
            Vector3 clockwiseQuarter=FourierCurves.BridgeBasePoint(Mathf.PI*.5f,1,halfLength,false);
            Assert.That(clockwiseQuarter.x,Is.EqualTo(-1).Within(1e-6));Assert.That(clockwiseQuarter.y,Is.EqualTo(0).Within(1e-6));

            Vector3 complex=FourierCurves.BridgePoint(0,0,halfLength,displacement,.4f);
            Assert.That(complex.z,Is.EqualTo(.4f).Within(1e-6));
            Assert.That(FourierCurves.BridgeBasePoint(Mathf.PI,1,halfLength,true),Is.EqualTo(Vector3.zero));

            Vector3 leftHalf=FourierCurves.BridgePoint(0,.5f,halfLength,1)-FourierCurves.BridgeBasePoint(0,.5f,halfLength,false);
            Vector3 centerHalf=FourierCurves.BridgePoint(Mathf.PI,.5f,halfLength,1)-FourierCurves.BridgeBasePoint(Mathf.PI,.5f,halfLength,false);
            Vector3 rightHalf=FourierCurves.BridgePoint(2*Mathf.PI,.5f,halfLength,1)-FourierCurves.BridgeBasePoint(2*Mathf.PI,.5f,halfLength,false);
            Assert.That(leftHalf.x,Is.EqualTo(-1).Within(1e-5));Assert.That(leftHalf.y,Is.EqualTo(0).Within(1e-5));
            Assert.That(centerHalf.x,Is.EqualTo(0).Within(1e-5));Assert.That(centerHalf.y,Is.EqualTo(1).Within(1e-5));
            Assert.That(rightHalf.x,Is.EqualTo(1).Within(1e-5));Assert.That(rightHalf.y,Is.EqualTo(0).Within(1e-5));
        }
    }
}
