using NUnit.Framework;
using UnityEngine;
using SphericalHarmonics.Math;
using SphericalHarmonics.Rendering;

namespace SphericalHarmonics.Tests
{
    public sealed class HarmonicColorMapTests
    {
        [Test]
        public void RealCurveRaisesSaturationWithoutReducingBrightness()
        {
            Color zero=HarmonicColorMap.RealCurve(0);Color small=HarmonicColorMap.RealCurve(.02);
            Color.RGBToHSV(small,out _,out float saturation,out float value);
            Assert.That(zero,Is.EqualTo(Color.white));Assert.That(saturation,Is.GreaterThan(0));Assert.That(value,Is.EqualTo(1).Within(1e-6));
        }

        [Test]
        public void ComplexSmallMagnitudeRemainsMostlyWhite()
        {
            Color color=HarmonicColorMap.Complex(new ComplexValue(.01,0));Color.RGBToHSV(color,out _,out float saturation,out _);
            Assert.That(saturation,Is.InRange(.15f,.3f));Assert.That(HarmonicColorMap.Complex(new ComplexValue()),Is.EqualTo(Color.white));
        }

        [Test]
        public void BridgeShaderRendersAfterOrdinaryVertexColorLines()
        {
            Shader ordinary=Shader.Find("SphericalHarmonics/UnlitVertexColor");Shader functionWire=Shader.Find("SphericalHarmonics/FunctionWireVertexColor");Shader bridge=Shader.Find("SphericalHarmonics/BridgeVertexColor");
            Assert.NotNull(ordinary);Assert.NotNull(functionWire);Assert.NotNull(bridge);
            Assert.That(functionWire.renderQueue,Is.GreaterThan(ordinary.renderQueue));Assert.That(bridge.renderQueue,Is.GreaterThan(functionWire.renderQueue));
        }
    }
}
