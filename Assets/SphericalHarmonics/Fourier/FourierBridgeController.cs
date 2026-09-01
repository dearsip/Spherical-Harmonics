using System.Collections.Generic;
using UnityEngine;
using SphericalHarmonics.State;
using SphericalHarmonics.Math;
using SphericalHarmonics.Rendering;

namespace SphericalHarmonics.Fourier
{
    public sealed class FourierBridgeController : MonoBehaviour
    {
        public bool Active { get; private set; }
        public BridgeStage Stage { get; private set; }=BridgeStage.Sphere;
        public ValueType ValueType => state.Value;
        public int M => state.M;
        public float Amplitude => state.Value==ValueType.Real?(float)System.Math.Abs(state.Real[state.L,state.M]):(float)state.Complex[state.L,state.M].Magnitude;
        public float SignedAmplitude => state.Value==ValueType.Real?(float)state.Real[state.L,state.M]:Amplitude;
        public float Phase => state.Value==ValueType.Complex?(float)state.Complex[state.L,state.M].Phase:0f;
        public float CircleAmount => circleAmount;
        public event System.Action Changed;

        [SerializeField] private float transitionDuration=.7f;
        [SerializeField] private float lineHalfLength=2.05f;
        [SerializeField] private float displacementScale=1.15f;
        private const int CurveSampleCount=257;
        private const int MaxPointsPerColorSegment=8;
        private readonly List<LineRenderer> curveSegments=new List<LineRenderer>();
        private Transform curveRoot;
        private Material curveMaterial;
        private LineRenderer[] vectorLines;
        private ViewerState state;
        private float circleAmount=1f,transitionStart=1f,transitionTarget=1f,transitionElapsed;
        private bool transitioning;

        public void Initialize(ViewerState viewerState)
        {
            state=viewerState;
            GameObject go=new GameObject("Fourier Curve");go.transform.SetParent(transform,false);curveRoot=go.transform;
            curveMaterial=new Material(Shader.Find("SphericalHarmonics/BridgeVertexColor"));CreateCurveSegment();
            CreateVectorLines();state.Changed+=OnStateChanged;
        }

        public void Enter(ValueType type,int m)
        {
            if(Active)return;
            Active=true;state.Value=type;state.Display=DisplayMode.Sphere;Stage=BridgeStage.Sphere;circleAmount=1f;transitioning=false;
            state.Select(Mathf.Abs(Mathf.Clamp(m,-3,3)),Mathf.Clamp(m,-3,3));
        }

        public void Exit()
        {
            if(!Active)return;
            Active=false;SetCurveVisibility(false);SetVectorVisibility(false);state.NotifyChanged();Changed?.Invoke();
        }

        public void SetStage(BridgeStage stage)
        {
            if(stage==Stage)return;
            Stage=stage;
            if(stage!=BridgeStage.Circle&&state.Display==DisplayMode.Orbital){state.Display=DisplayMode.Sphere;state.NotifyChanged();}
            if(stage==BridgeStage.Circle||stage==BridgeStage.Line)
            {
                transitionStart=circleAmount;transitionTarget=stage==BridgeStage.Circle?1f:0f;transitionElapsed=0;transitioning=!Mathf.Approximately(transitionStart,transitionTarget);
            }
            else transitioning=false;
            Rebuild();
        }

        public void SetM(int m)
        {
            m=Mathf.Clamp(m,-3,3);
            if(m==state.M&&state.L==Mathf.Abs(m))return;
            state.Select(Mathf.Abs(m),m);
        }

        public void SetValueType(ValueType type){if(Active)state.TrySetValueType(type);}

        public ComplexValue Sample(float phi)
        {
            Vector3 equator=new Vector3(Mathf.Cos(-phi-Mathf.PI*.5f),Mathf.Sin(-phi-Mathf.PI*.5f),0);
            return EvaluateDirection(equator);
        }

        public ComplexValue EvaluateDirection(Vector3 direction)
        {
            ComplexValue sum=new ComplexValue();
            for(int m=-3;m<=3;m++)
            {
                int l=Mathf.Abs(m);
                if(ValueType==ValueType.Real)sum+=new ComplexValue(state.Real[l,m]*SphericalHarmonicEvaluator.RealBasis(l,m,direction),0);
                else sum+=state.Complex[l,m]*SphericalHarmonicEvaluator.ComplexBasis(l,m,direction);
            }
            return sum;
        }

        private void Rebuild()
        {
            if(curveRoot==null)return;
            bool curveVisible=Active&&Stage!=BridgeStage.Sphere;SetCurveVisibility(curveVisible);
            SetVectorVisibility(curveVisible&&state.ShowNormalVectors);
            if(!curveVisible){Changed?.Invoke();return;}
            Vector3[] points=new Vector3[CurveSampleCount];Color[] pointColors=new Color[CurveSampleCount];bool orbital=Stage==BridgeStage.Circle&&state.Display==DisplayMode.Orbital;
            for(int i=0;i<CurveSampleCount;i++)
            {
                float phi=2*Mathf.PI*i/(CurveSampleCount-1);ComplexValue sample=Sample(phi);
                float radial=displacementScale*(float)(orbital?System.Math.Abs(sample.Real):sample.Real);
                float imaginary=ValueType==ValueType.Complex?displacementScale*(float)sample.Imaginary:0;
                Vector3 mathematical=FourierCurves.BridgePoint(phi,circleAmount,lineHalfLength,radial,imaginary,orbital);
                points[i]=CoordinateSpace.ToWorld(mathematical,Quaternion.identity);
                pointColors[i]=ValueType==ValueType.Complex?HarmonicColorMap.Complex(sample):HarmonicColorMap.RealCurve(sample.Real);
            }
            BuildColoredCurve(points,pointColors);UpdateBridgeVectors(orbital);Changed?.Invoke();
        }

        private void UpdateBridgeVectors(bool orbital)
        {
            if(vectorLines==null||!state.ShowNormalVectors)return;
            for(int i=0;i<vectorLines.Length;i++)
            {
                float phi=2*Mathf.PI*i/vectorLines.Length;ComplexValue sample=Sample(phi);
                float radial=displacementScale*(float)(orbital?System.Math.Abs(sample.Real):sample.Real);
                float imaginary=ValueType==ValueType.Complex?displacementScale*(float)sample.Imaginary:0;
                Vector3 start=FourierCurves.BridgeBasePoint(phi,circleAmount,lineHalfLength,orbital);
                Vector3 end=FourierCurves.BridgePoint(phi,circleAmount,lineHalfLength,radial,imaginary,orbital);
                vectorLines[i].SetPosition(0,CoordinateSpace.ToWorld(start,Quaternion.identity));vectorLines[i].SetPosition(1,CoordinateSpace.ToWorld(end,Quaternion.identity));
                Color color=ValueType==ValueType.Complex?HarmonicColorMap.Complex(sample):HarmonicColorMap.RealCurve(sample.Real);vectorLines[i].startColor=vectorLines[i].endColor=color;
            }
        }

        public void AdvanceAnimation(float deltaTime)
        {
            if(!transitioning)return;
            transitionElapsed+=Mathf.Max(0,deltaTime);float t=transitionDuration<=0?1:Mathf.Clamp01(transitionElapsed/transitionDuration);
            circleAmount=Mathf.Lerp(transitionStart,transitionTarget,t*t*(3f-2f*t));if(t>=1)transitioning=false;Rebuild();
        }

        private void Update(){AdvanceAnimation(Time.unscaledDeltaTime);}
        private void OnStateChanged(){if(Active)Rebuild();}
        private void OnDestroy(){if(state!=null)state.Changed-=OnStateChanged;}

        private void CreateVectorLines()
        {
            const int count=12;vectorLines=new LineRenderer[count];GameObject root=new GameObject("Bridge Normal / Radial Vectors");root.transform.SetParent(transform,false);
            for(int i=0;i<count;i++){GameObject go=new GameObject("Bridge Vector "+i);go.transform.SetParent(root.transform,false);LineRenderer line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=2;line.widthMultiplier=.011f;line.sharedMaterial=new Material(Shader.Find("SphericalHarmonics/UnlitVertexColor"));vectorLines[i]=line;}
        }

        private void SetVectorVisibility(bool visible){if(vectorLines!=null)foreach(LineRenderer line in vectorLines)line.gameObject.SetActive(visible);}

        private LineRenderer CreateCurveSegment()
        {
            GameObject go=new GameObject("Color Segment "+curveSegments.Count);go.transform.SetParent(curveRoot,false);
            LineRenderer line=go.AddComponent<LineRenderer>();line.sharedMaterial=curveMaterial;line.widthMultiplier=.025f;line.positionCount=0;line.useWorldSpace=false;curveSegments.Add(line);return line;
        }

        private void SetCurveVisibility(bool visible)
        {
            if(curveRoot!=null)curveRoot.gameObject.SetActive(visible);
        }

        private void BuildColoredCurve(Vector3[] points,Color[] pointColors)
        {
            int intervalsPerSegment=MaxPointsPerColorSegment-1;
            int needed=Mathf.CeilToInt((points.Length-1)/(float)intervalsPerSegment);
            while(curveSegments.Count<needed)CreateCurveSegment();
            for(int segmentIndex=0;segmentIndex<curveSegments.Count;segmentIndex++)
            {
                LineRenderer line=curveSegments[segmentIndex];bool active=segmentIndex<needed;line.gameObject.SetActive(active);if(!active)continue;
                int start=segmentIndex*intervalsPerSegment;int end=Mathf.Min(start+intervalsPerSegment,points.Length-1);int count=end-start+1;
                line.positionCount=count;for(int i=0;i<count;i++)line.SetPosition(i,points[start+i]);
                float[] distances=new float[count];for(int i=1;i<count;i++)distances[i]=distances[i-1]+Vector3.Distance(points[start+i-1],points[start+i]);
                float total=distances[count-1];GradientColorKey[] keys=new GradientColorKey[count];
                for(int i=0;i<count;i++){float time=total>1e-6f?distances[i]/total:(float)i/(count-1);keys[i]=new GradientColorKey(pointColors[start+i],time);}
                Gradient gradient=new Gradient();gradient.SetKeys(keys,new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(1,1)});line.colorGradient=gradient;
            }
        }
    }
}
