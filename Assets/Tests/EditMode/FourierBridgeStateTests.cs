using NUnit.Framework;
using UnityEngine;
using SphericalHarmonics.Fourier;
using SphericalHarmonics.State;

namespace SphericalHarmonics.Tests
{
    public sealed class FourierBridgeStateTests
    {
        [Test]
        public void BridgePureOffSelectsStoredCoefficientAndRendersTheSectoralSum()
        {
            var state=new ViewerState();state.SetPureMode(false);state.Real[1,1]=.25;state.Real[2,-2]=-.4;state.Real[3,0]=.7;state.Select(1,1);
            GameObject go=new GameObject("bridge-test");
            try
            {
                var bridge=go.AddComponent<FourierBridgeController>();bridge.Initialize(state);bridge.Enter(ValueType.Real,1);
                Assert.AreEqual(1,state.L);Assert.AreEqual(1,state.M);Assert.IsFalse(state.PureMode);Assert.That(bridge.SignedAmplitude,Is.EqualTo(.25));
                float phi=.37f;Vector3 equator=new Vector3(Mathf.Cos(-phi-Mathf.PI*.5f),Mathf.Sin(-phi-Mathf.PI*.5f),0);
                double expected=.25*SphericalHarmonics.Math.SphericalHarmonicEvaluator.RealBasis(1,1,equator)-.4*SphericalHarmonics.Math.SphericalHarmonicEvaluator.RealBasis(2,-2,equator);
                Assert.That(bridge.Sample(phi).Real,Is.EqualTo(expected).Within(1e-9));
                bridge.SetM(-2);Assert.AreEqual(2,state.L);Assert.AreEqual(-2,state.M);Assert.That(bridge.SignedAmplitude,Is.EqualTo(-.4).Within(1e-6));
                Assert.That(bridge.Sample(phi).Real,Is.EqualTo(expected).Within(1e-9));
                Assert.That(state.Real[1,1],Is.EqualTo(.25));Assert.That(state.Real[2,-2],Is.EqualTo(-.4));Assert.That(state.Real[3,0],Is.EqualTo(.7));
                bridge.Exit();Assert.That(state.Real[1,1],Is.EqualTo(.25));Assert.That(state.Real[2,-2],Is.EqualTo(-.4));Assert.That(state.Real[3,0],Is.EqualTo(.7));
            }
            finally{Object.DestroyImmediate(go);}
        }

        [Test]
        public void BridgePureOnUsesTheNormalPureSelectionBehavior()
        {
            var state=new ViewerState();GameObject go=new GameObject("bridge-pure-test");
            try
            {
                var bridge=go.AddComponent<FourierBridgeController>();bridge.Initialize(state);bridge.Enter(ValueType.Real,-2);
                Assert.IsTrue(state.PureMode);Assert.That(state.Real[2,-2],Is.EqualTo(1));Assert.That(state.Real[1,0],Is.EqualTo(0));
                bridge.SetM(3);Assert.That(state.Real[3,3],Is.EqualTo(1));Assert.That(state.Real[2,-2],Is.EqualTo(0));
            }
            finally{Object.DestroyImmediate(go);}
        }

        [Test]
        public void LineAndCircleStagesAnimateBetweenTheirParameterizations()
        {
            var state=new ViewerState();GameObject go=new GameObject("bridge-animation-test");
            try
            {
                var bridge=go.AddComponent<FourierBridgeController>();bridge.Initialize(state);bridge.Enter(ValueType.Real,1);
                bridge.SetStage(BridgeStage.Line);
                Assert.That(bridge.CircleAmount,Is.EqualTo(1).Within(1e-6));
                bridge.AdvanceAnimation(.35f);
                Assert.That(bridge.CircleAmount,Is.InRange(.45f,.55f));
                bridge.AdvanceAnimation(.35f);
                Assert.That(bridge.CircleAmount,Is.EqualTo(0).Within(1e-6));
                bridge.SetStage(BridgeStage.Circle);bridge.AdvanceAnimation(.7f);
                Assert.That(bridge.CircleAmount,Is.EqualTo(1).Within(1e-6));
            }
            finally{Object.DestroyImmediate(go);}
        }

        [Test]
        public void BridgeValueTypeSwitchDoesNotApplyExistingPureToggle()
        {
            var state=new ViewerState();state.SetPureMode(false);state.Complex[2,1]=new SphericalHarmonics.Math.ComplexValue(.3,.2);GameObject go=new GameObject("bridge-bank-test");
            try
            {
                var bridge=go.AddComponent<FourierBridgeController>();bridge.Initialize(state);bridge.Enter(ValueType.Real,0);bridge.SetValueType(ValueType.Complex);
                Assert.That(state.Complex[2,1].Real,Is.EqualTo(.3));Assert.That(state.Complex[2,1].Imaginary,Is.EqualTo(.2));
            }
            finally{Object.DestroyImmediate(go);}
        }

        [Test]
        public void BridgeComplexSelectionUsesEachStoredMagnitudeAndPhase()
        {
            var state=new ViewerState();state.SetPureMode(false);state.Complex[1,-1]=new SphericalHarmonics.Math.ComplexValue(.3,-.4);state.Complex[2,2]=new SphericalHarmonics.Math.ComplexValue(-.2,.6);state.Select(1,-1);
            GameObject go=new GameObject("bridge-complex-parameter-test");
            try
            {
                var bridge=go.AddComponent<FourierBridgeController>();bridge.Initialize(state);bridge.Enter(ValueType.Complex,-1);
                Assert.That(state.Complex[1,-1].Real,Is.EqualTo(.3));Assert.That(state.Complex[1,-1].Imaginary,Is.EqualTo(-.4));
                bridge.SetM(2);Assert.AreEqual(2,state.L);Assert.AreEqual(2,state.M);
                Assert.That(state.Complex[2,2].Real,Is.EqualTo(-.2));Assert.That(state.Complex[2,2].Imaginary,Is.EqualTo(.6));
                Assert.That(state.Complex[1,-1].Real,Is.EqualTo(.3));Assert.That(state.Complex[1,-1].Imaginary,Is.EqualTo(-.4));
            }
            finally{Object.DestroyImmediate(go);}
        }
    }
}
