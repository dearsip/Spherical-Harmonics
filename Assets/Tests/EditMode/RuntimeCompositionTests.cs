using NUnit.Framework;
using UnityEngine;
using SphericalHarmonics.Fourier;
using SphericalHarmonics.Input;
using SphericalHarmonics.Rendering;
using SphericalHarmonics.Rotation;
using SphericalHarmonics.State;
using SphericalHarmonics.UI;

namespace SphericalHarmonics.Tests
{
    public sealed class RuntimeCompositionTests
    {
        [Test]
        public void TouchOrbitUsesDirectManipulationDirection()
        {
            Vector2 orbit=CameraGestureMath.TouchOrbitDelta(new Vector2(4,-3),.5f);
            Assert.That(orbit.x,Is.EqualTo(-2).Within(1e-6));
            Assert.That(orbit.y,Is.EqualTo(-1.5f).Within(1e-6));
        }

        [Test]
        public void BundledMathFontContainsFormulaGlyphs()
        {
            Font font=Resources.Load<Font>("Fonts/NotoSansMath-Regular");
            Assert.IsNotNull(font);
            foreach(char glyph in "θφ∝√π⁰¹²³⁻₀₁₂₃")
                Assert.IsTrue(font.HasCharacter(glyph),$"Math font is missing U+{(int)glyph:X4} ({glyph}).");
        }

        [Test]
        public void RuntimeComponentsComposeWithoutSceneReferences()
        {
            GameObject root=new GameObject("test-root"),cameraObject=new GameObject("test-camera"),uiObject=new GameObject("test-ui");
            try
            {
                var state=new ViewerState();var bridge=root.AddComponent<FourierBridgeController>();bridge.Initialize(state);
                var visualization=root.AddComponent<VisualizationRenderer>();visualization.Initialize(state,bridge);
                var orbit=cameraObject.AddComponent<CameraOrbitController>();var rotation=new RotationController(state,visualization);
                uiObject.AddComponent<ViewerUIController>().Initialize(state,bridge,rotation,orbit);
                Assert.IsNotNull(root.transform.Find("Harmonic Surface"));
                Assert.IsNotNull(uiObject.GetComponent<Canvas>());
                Assert.That(visualization.ReferenceScale,Is.EqualTo(1).Within(1e-6));
                state.TrySetDisplay(DisplayMode.Orbital);Assert.That(visualization.DisplayBlend,Is.EqualTo(0).Within(1e-6));Assert.That(visualization.ReferenceScale,Is.EqualTo(1).Within(1e-6));
                visualization.AdvanceDisplayAnimation(.35f);Assert.That(visualization.DisplayBlend,Is.InRange(.45f,.55f));Assert.That(visualization.ReferenceScale,Is.EqualTo(0).Within(1e-6));
                visualization.AdvanceDisplayAnimation(.35f);Assert.That(visualization.DisplayBlend,Is.EqualTo(1).Within(1e-6));Assert.That(visualization.ReferenceScale,Is.EqualTo(1.5f).Within(1e-6));
                state.TrySetDisplay(DisplayMode.Sphere);Assert.That(visualization.DisplayBlend,Is.EqualTo(1).Within(1e-6));Assert.That(visualization.ReferenceScale,Is.EqualTo(1.5f).Within(1e-6));
                visualization.AdvanceDisplayAnimation(.35f);Assert.That(visualization.ReferenceScale,Is.EqualTo(0).Within(1e-6));
                visualization.AdvanceDisplayAnimation(.35f);Assert.That(visualization.DisplayBlend,Is.EqualTo(0).Within(1e-6));Assert.That(visualization.ReferenceScale,Is.EqualTo(1).Within(1e-6));
            }
            finally{Object.DestroyImmediate(uiObject);Object.DestroyImmediate(cameraObject);Object.DestroyImmediate(root);}
        }
    }
}
