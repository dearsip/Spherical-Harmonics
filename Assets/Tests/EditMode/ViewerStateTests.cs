using NUnit.Framework;
using UnityEngine;
using SphericalHarmonics.Math;
using SphericalHarmonics.State;

namespace SphericalHarmonics.Tests
{
    public sealed class ViewerStateTests
    {
        [Test]
        public void SelectionAndValueTypeSwitchesRetainIndependentBanks()
        {
            var state=new ViewerState();state.SetPureMode(false);state.Real[3,-2]=.42;state.Complex[2,1]=ComplexValue.FromPolar(.8,.7);
            state.Select(0,0);Assert.IsTrue(state.TrySetValueType(ValueType.Complex));state.Select(3,-3);Assert.IsTrue(state.TrySetValueType(ValueType.Real));
            Assert.That(state.Real[3,-2],Is.EqualTo(.42));Assert.That(state.Complex[2,1].Magnitude,Is.EqualTo(.8).Within(1e-12));
        }

        [Test]
        public void ComplexFlowAutomaticallySwitchesToReal()
        {
            var state=new ViewerState();Assert.IsTrue(state.TrySetValueType(ValueType.Complex));Assert.IsTrue(state.TrySetDisplay(DisplayMode.Flow));Assert.AreEqual(DisplayMode.Flow,state.Display);Assert.AreEqual(ValueType.Real,state.Value);
            Assert.IsFalse(state.TrySetValueType(ValueType.Complex));Assert.AreEqual(ValueType.Real,state.Value);
        }

        [Test]
        public void PureModeFollowsSelectionAndZeroSelectedOnlyClearsCurrentMode()
        {
            var state=new ViewerState();state.Select(2,-1);Assert.That(state.Real[2,-1],Is.EqualTo(1));Assert.That(state.Real[1,0],Is.EqualTo(0));
            state.ZeroSelected();Assert.That(state.Real[2,-1],Is.EqualTo(0));
            state.SetPureMode(false);state.Real[1,1]=.3;state.Select(3,2);Assert.That(state.Real[1,1],Is.EqualTo(.3));
        }

        [Test]
        public void MathematicalZAxisMapsToUnityUp()
        {
            Assert.That(Vector3.Distance(CoordinateSpace.AxisInWorld(Quaternion.identity,Vector3.forward),Vector3.up),Is.LessThan(1e-6));
            Assert.That(Vector3.Distance(CoordinateSpace.AxisInWorld(Quaternion.identity,Vector3.up),Vector3.forward),Is.LessThan(1e-6));
            Assert.That(Vector3.Distance(CoordinateSpace.AxisInWorld(Quaternion.identity,Vector3.right),Vector3.right),Is.LessThan(1e-6));
        }
    }
}
