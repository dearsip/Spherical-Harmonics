using UnityEngine;
using SphericalHarmonics.Rendering;
using SphericalHarmonics.State;
using SphericalHarmonics.Math;

namespace SphericalHarmonics.Rotation
{
    public sealed class RotationController
    {
        private readonly ViewerState state;
        private readonly VisualizationRenderer renderer;
        private Quaternion delta=Quaternion.identity;
        private Quaternion startingFrame;
        public bool FunctionMode { get; private set; }=true;
        public bool IsOpen { get; private set; }

        public RotationController(ViewerState viewerState,VisualizationRenderer visualization){state=viewerState;renderer=visualization;}
        public void Begin(){IsOpen=true;startingFrame=state.CoordinateFrame;delta=Quaternion.identity;Preview();}
        public void SetMode(bool function){FunctionMode=function;Preview();}
        public void SetEuler(Vector3 euler){delta=Quaternion.Euler(euler);Preview();}
        public void Cancel(){state.CoordinateFrame=startingFrame;renderer.ClearRotationPreview();delta=Quaternion.identity;IsOpen=false;state.NotifyChanged();}

        public void Apply()
        {
            if(FunctionMode)
            {
                if(state.Value==ValueType.Real)state.Real.CopyFrom(RotationService.RotateRealActive(state.Real,delta));
                else state.Complex.CopyFrom(RotationService.RotateComplexActive(state.Complex,delta));
            }
            else
            {
                if(state.Value==ValueType.Real)state.Real.CopyFrom(RotationService.ReexpressRealInRotatedCoordinates(state.Real,delta));
                else state.Complex.CopyFrom(RotationService.ReexpressComplexInRotatedCoordinates(state.Complex,delta));
                state.CoordinateFrame=startingFrame*delta;
            }
            delta=Quaternion.identity;IsOpen=false;renderer.ClearRotationPreview();state.NotifyChanged();
        }

        public void ResetAxes()
        {
            Quaternion frame=state.CoordinateFrame;
            if(state.Value==ValueType.Real)state.Real.CopyFrom(RotationService.RotateRealActive(state.Real,frame));
            else state.Complex.CopyFrom(RotationService.RotateComplexActive(state.Complex,frame));
            state.CoordinateFrame=Quaternion.identity;delta=Quaternion.identity;renderer.ClearRotationPreview();state.NotifyChanged();
        }

        private void Preview()
        {
            if(!IsOpen)return;
            if(FunctionMode){renderer.PreviewFunctionRotation(CoordinateSpace.ActiveRotationToWorld(startingFrame,delta));renderer.PreviewCoordinateRotation(CoordinateSpace.FrameToWorld(startingFrame));}
            else{renderer.PreviewFunctionRotation(Quaternion.identity);renderer.PreviewCoordinateRotation(CoordinateSpace.FrameToWorld(startingFrame*delta));}
        }
    }
}
