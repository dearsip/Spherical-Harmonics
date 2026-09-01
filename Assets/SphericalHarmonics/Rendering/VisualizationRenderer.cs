using UnityEngine;
using SphericalHarmonics.Flow;
using SphericalHarmonics.Fourier;
using SphericalHarmonics.Math;
using SphericalHarmonics.State;

namespace SphericalHarmonics.Rendering
{
    public sealed class VisualizationRenderer : MonoBehaviour
    {
        public float DisplayBlend => displayBlend;
        public float ReferenceScale => referenceScale;
        [Header("Mesh")][SerializeField] private int longitudeSegments=64;
        [SerializeField] private int latitudeSegments=32;
        [Header("Sphere / Orbital")][SerializeField] private float displayScale=1.15f;
        [SerializeField] private float orbitalScaleMultiplier=1.5f;
        [SerializeField] private bool preventNegativeRadius=true;
        [SerializeField] private float minimumRadius=.08f;
        [Header("Complex color")][SerializeField] private float phaseEpsilon=.035f;
        [Header("Flow")][SerializeField] private int rk4Steps=32;
        [SerializeField] private float displayTransitionDuration=.7f;

        private ViewerState state;
        private FourierBridgeController bridge;
        private Mesh mesh;
        private SphereMeshData topology;
        private Vector3[] vertices;
        private Color[] colors;
        private OverlayController overlays;
        private MeshRenderer meshRenderer;
        private Material surfaceMaterial;
        private Transform surfaceTransform;
        private DisplayMode lastDisplay;
        private float displayBlend,displayBlendStart,displayBlendTarget,displayTransitionElapsed,referenceScale=1f;
        private bool displayTransitioning;

        public void Initialize(ViewerState viewerState, FourierBridgeController bridgeController)
        {
            state=viewerState;bridge=bridgeController;topology=SphereMeshGenerator.Generate(latitudeSegments,longitudeSegments);ReverseTriangleWinding(topology.Triangles);
            vertices=new Vector3[topology.Directions.Length];colors=new Color[vertices.Length];
            GameObject surface=new GameObject("Harmonic Surface");surface.transform.SetParent(transform,false);surfaceTransform=surface.transform;
            MeshFilter filter=surface.AddComponent<MeshFilter>();meshRenderer=surface.AddComponent<MeshRenderer>();
            mesh=new Mesh{name="Dynamic Harmonic Surface",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.MarkDynamic();mesh.vertices=topology.Directions;mesh.triangles=topology.Triangles;mesh.RecalculateNormals();SmoothLongitudeSeamNormals();filter.sharedMesh=mesh;
            surfaceMaterial=new Material(Shader.Find("SphericalHarmonics/VertexColor"));meshRenderer.sharedMaterial=surfaceMaterial;
            overlays=gameObject.AddComponent<OverlayController>();overlays.Initialize(state,topology,bridge);
            lastDisplay=state.Display;displayBlend=state.Display==DisplayMode.Orbital?1f:0f;
            state.Changed+=Rebuild;bridge.Changed+=Rebuild;Rebuild();
        }

        public void PreviewFunctionRotation(Quaternion worldRotation){if(surfaceTransform!=null)surfaceTransform.rotation=worldRotation;}
        public void PreviewCoordinateRotation(Quaternion worldRotation){if(overlays!=null)overlays.PreviewAxes(worldRotation);}
        public void ClearRotationPreview(){if(surfaceTransform!=null)surfaceTransform.rotation=Quaternion.identity;if(overlays!=null)overlays.PreviewAxes(CoordinateSpace.FrameToWorld(state.CoordinateFrame));}

        public void Rebuild()
        {
            if(mesh==null)return;DetectDisplayTransition();surfaceMaterial.SetFloat("_Directional",state.DirectionalLight?1f:0f);
            bool radialVisible=!bridge.Active||bridge.Stage==BridgeStage.Sphere;
            meshRenderer.enabled=radialVisible&&state.ShowFunctionSurface;
            UpdateReferenceScale();if(!radialVisible){overlays.UpdateDynamic(vertices,colors,false);return;}
            if(state.Display==DisplayMode.Flow&&!bridge.Active)BuildFlow();else BuildRadial();
            CopyLongitudeSeam();mesh.Clear();mesh.vertices=vertices;mesh.triangles=topology.Triangles;mesh.colors=colors;mesh.RecalculateNormals();SmoothLongitudeSeamNormals();mesh.RecalculateBounds();
            overlays.UpdateDynamic(vertices,colors,true);
        }

        private void BuildRadial()
        {
            double[] real=new double[vertices.Length];ComplexValue[] complex=new ComplexValue[vertices.Length];
            float scale=displayScale;
            for(int i=0;i<vertices.Length;i++)
            {
                Vector3 coordinates=topology.Directions[i];
                if(bridge.Active)
                {
                    ComplexValue bridgeValue=bridge.EvaluateDirection(coordinates);
                    if(bridge.ValueType==ValueType.Real)real[i]=bridgeValue.Real;else complex[i]=bridgeValue;
                }
                else if(state.Value==ValueType.Real)real[i]=SphericalHarmonicEvaluator.EvaluateReal(state.Real,coordinates);
                else complex[i]=SphericalHarmonicEvaluator.EvaluateComplex(state.Complex,coordinates);
            }
            bool isComplex=bridge.Active?bridge.ValueType==ValueType.Complex:state.Value==ValueType.Complex;
            if(state.Display==DisplayMode.Sphere&&preventNegativeRadius)
            {
                double min=0;for(int i=0;i<real.Length;i++)min=System.Math.Min(min,isComplex?complex[i].Real:real[i]);
                if(min<0)scale=Mathf.Min(scale,(1f-minimumRadius)/(float)-min);
            }
            for(int i=0;i<vertices.Length;i++)
            {
                double value=isComplex?complex[i].Real:real[i];double magnitude=isComplex?complex[i].Magnitude:System.Math.Abs(real[i]);
                float sphereRadius=1f+scale*(float)value,orbitalRadius=orbitalScaleMultiplier*scale*(float)magnitude;
                float blend=!bridge.Active&&displayTransitioning?displayBlend:(state.Display==DisplayMode.Orbital?1f:0f);
                float radius=Mathf.Lerp(sphereRadius,orbitalRadius,blend);
                vertices[i]=CoordinateSpace.ToWorld(topology.Directions[i]*Mathf.Max(.001f,radius),state.CoordinateFrame);
                colors[i]=isComplex?HarmonicColorMap.Complex(complex[i],phaseEpsilon):HarmonicColorMap.Real(real[i]);
            }
        }

        private void BuildFlow()
        {
            for(int i=0;i<vertices.Length;i++)
            {
                Vector3 local=topology.Directions[i];
                Vector3 flowed=FlowFieldBuilder.Integrate(state.Real,local,state.FlowTime,rk4Steps);
                vertices[i]=CoordinateSpace.ToWorld(flowed,state.CoordinateFrame);
                colors[i]=HarmonicColorMap.Real(SphericalHarmonicEvaluator.EvaluateReal(state.Real,local));
            }
        }

        public void AdvanceDisplayAnimation(float deltaTime)
        {
            if(!displayTransitioning)return;
            displayTransitionElapsed+=Mathf.Max(0,deltaTime);float t=displayTransitionDuration<=0?1:Mathf.Clamp01(displayTransitionElapsed/displayTransitionDuration);
            float smooth=t*t*(3f-2f*t);displayBlend=Mathf.Lerp(displayBlendStart,displayBlendTarget,smooth);if(t>=1)displayTransitioning=false;Rebuild();
        }

        private void Update(){AdvanceDisplayAnimation(Time.unscaledDeltaTime);}

        private void DetectDisplayTransition()
        {
            if(bridge.Active||state.Display==lastDisplay)return;
            bool radialPair=(lastDisplay==DisplayMode.Sphere&&state.Display==DisplayMode.Orbital)||(lastDisplay==DisplayMode.Orbital&&state.Display==DisplayMode.Sphere);
            if(radialPair){displayBlendStart=displayBlend;displayBlendTarget=state.Display==DisplayMode.Orbital?1f:0f;displayTransitionElapsed=0;displayTransitioning=true;}
            else{displayTransitioning=false;displayBlend=state.Display==DisplayMode.Orbital?1f:0f;}
            lastDisplay=state.Display;
        }

        private void UpdateReferenceScale()
        {
            float scale;
            if(displayTransitioning)
            {
                float t=displayTransitionDuration<=0?1:Mathf.Clamp01(displayTransitionElapsed/displayTransitionDuration);
                float from=Mathf.Lerp(1f,orbitalScaleMultiplier,displayBlendStart);float to=Mathf.Lerp(1f,orbitalScaleMultiplier,displayBlendTarget);
                if(t<=.5f){float u=Smooth01(t*2f);scale=Mathf.Lerp(from,0f,u);}
                else{float u=Smooth01((t-.5f)*2f);scale=Mathf.Lerp(0f,to,u);}
            }
            else scale=state.Display==DisplayMode.Orbital?orbitalScaleMultiplier:1f;
            referenceScale=scale;overlays.SetReferenceScale(scale);
        }

        private static float Smooth01(float t){t=Mathf.Clamp01(t);return t*t*(3f-2f*t);}

        private void CopyLongitudeSeam()
        {
            int columns=longitudeSegments+1;for(int lat=0;lat<=latitudeSegments;lat++){int first=lat*columns,last=first+longitudeSegments;vertices[last]=vertices[first];colors[last]=colors[first];}
        }

        private static void ReverseTriangleWinding(int[] triangles){for(int i=0;i<triangles.Length;i+=3){int swap=triangles[i+1];triangles[i+1]=triangles[i+2];triangles[i+2]=swap;}}

        private void SmoothLongitudeSeamNormals()
        {
            Vector3[] normals=mesh.normals;if(normals==null||normals.Length==0)return;int columns=longitudeSegments+1;
            for(int lat=0;lat<=latitudeSegments;lat++){int first=lat*columns,last=first+longitudeSegments;Vector3 normal=(normals[first]+normals[last]).normalized;normals[first]=normals[last]=normal;}mesh.normals=normals;
        }

        private void OnDestroy(){if(state!=null)state.Changed-=Rebuild;if(bridge!=null)bridge.Changed-=Rebuild;}
    }
}
