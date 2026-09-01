using UnityEngine;
using SphericalHarmonics.Flow;
using SphericalHarmonics.Math;
using SphericalHarmonics.Rotation;
using SphericalHarmonics.State;

namespace SphericalHarmonics.Rendering
{
    public sealed class OverlayController : MonoBehaviour
    {
        private ViewerState state;
        private SphericalHarmonics.Fourier.FourierBridgeController bridge;
        private GameObject axesRoot;
        private MeshFilter referenceFilter, referenceWireFilter, wireFilter;
        private MeshRenderer referenceRenderer, referenceWireRenderer, wireRenderer;
        private LineRenderer[] vectorLines;
        private SphereMeshData topology;
        private Vector3[] lastVertices;
        private bool functionGeometryVisible=true;

        public void Initialize(ViewerState viewerState, SphereMeshData meshData,SphericalHarmonics.Fourier.FourierBridgeController bridgeController)
        {
            state = viewerState; topology = meshData;bridge=bridgeController;
            axesRoot = new GameObject("Coordinate Axes"); axesRoot.transform.SetParent(transform, false);
            CreateAxis(Vector3.right,Vector3.forward,new Color(.92f,.2f,.2f), "X");
            CreateAxis(Vector3.forward,Vector3.right,new Color(.15f,.68f,.28f), "Y");
            CreateAxis(Vector3.up,Vector3.right,new Color(.18f,.4f,.95f), "Z");
            referenceFilter = CreateMeshObject("Reference Unit Sphere", out referenceRenderer);
            Mesh refMesh = new Mesh { name = "Reference Sphere" };
            Vector3[] referenceVertices=new Vector3[meshData.Directions.Length];for(int i=0;i<referenceVertices.Length;i++)referenceVertices[i]=CoordinateSpace.ToWorld(meshData.Directions[i],Quaternion.identity);
            refMesh.vertices = referenceVertices; refMesh.triangles = meshData.Triangles; refMesh.RecalculateNormals();
            referenceFilter.sharedMesh = refMesh;
            Material translucent = new Material(Shader.Find("SphericalHarmonics/UnlitColor"));
            translucent.color = new Color(.62f,.72f,.82f,.12f);
            referenceRenderer.sharedMaterial = translucent;
            referenceWireFilter=CreateMeshObject("Unit Sphere Wireframe",out referenceWireRenderer);Mesh referenceWire=new Mesh{name="Unit Sphere Wireframe",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};referenceWire.vertices=referenceVertices;referenceWire.SetIndices(meshData.LineIndices,MeshTopology.Lines,0);referenceWireFilter.sharedMesh=referenceWire;referenceWireRenderer.sharedMaterial=LineMaterial(new Color(.36f,.46f,.56f,.42f));
            wireFilter = CreateMeshObject("Function Wireframe", out wireRenderer);
            wireRenderer.sharedMaterial = new Material(Shader.Find("SphericalHarmonics/FunctionWireVertexColor"));
            CreateVectorLines();
            state.Changed += RefreshVisibility;
            RefreshVisibility();
        }

        public void UpdateDynamic(Vector3[] vertices,Color[] colors,bool visible)
        {
            lastVertices=vertices;functionGeometryVisible=visible;
            Mesh wire = wireFilter.sharedMesh;
            if (wire == null) { wire = new Mesh { name="Wireframe Lines", indexFormat=UnityEngine.Rendering.IndexFormat.UInt32 }; wireFilter.sharedMesh=wire; }
            wire.Clear(); wire.vertices=vertices;wire.colors=colors; wire.SetIndices(topology.LineIndices,MeshTopology.Lines,0);
            if (state.ShowNormalVectors&&visible) UpdateVectors();
            axesRoot.transform.rotation = CoordinateSpace.FrameToWorld(state.CoordinateFrame);
            RefreshVisibility();
        }

        public void PreviewAxes(Quaternion worldRotation) => axesRoot.transform.rotation = worldRotation;

        private void UpdateVectors()
        {
            const int count=48;
            for(int i=0;i<count;i++)
            {
                float signedValue;
                if((state.Display==DisplayMode.Sphere||state.Display==DisplayMode.Orbital)&&lastVertices!=null)
                {
                    int index=Mathf.RoundToInt((float)i/(count-1)*(lastVertices.Length-1));Vector3 direction=lastVertices[index].normalized;
                    Vector3 local=topology.Directions[index];
                    if(bridge.Active&&bridge.Stage==BridgeStage.Sphere)signedValue=(float)bridge.EvaluateDirection(local).Real;
                    else signedValue=state.Value==ValueType.Real?(float)SphericalHarmonicEvaluator.EvaluateReal(state.Real,local):(float)SphericalHarmonicEvaluator.EvaluateComplex(state.Complex,local).Real;
                    vectorLines[i].SetPosition(0,state.Display==DisplayMode.Orbital?Vector3.zero:direction);
                    vectorLines[i].SetPosition(1,lastVertices[index]);
                }
                else
                {
                    Vector3 local=RotationService.FibonacciDirection(i,count);Vector3 world=CoordinateSpace.ToWorld(local,state.CoordinateFrame);double vn=FlowFieldBuilder.InitialNormalVelocity(state.Real,local);
                    signedValue=(float)vn;Vector3 start=world.normalized*1.015f;vectorLines[i].SetPosition(0,start);vectorLines[i].SetPosition(1,start+world.normalized*signedValue*.35f);
                }
                vectorLines[i].startColor=vectorLines[i].endColor=VectorColor(signedValue);
            }
        }

        public void SetReferenceScale(float scale){referenceFilter.transform.localScale=referenceWireFilter.transform.localScale=Vector3.one*scale;}

        private void RefreshVisibility()
        {
            axesRoot.SetActive(state.ShowAxes);
            referenceRenderer.gameObject.SetActive(state.ShowUnitSphereSurface);
            referenceWireRenderer.gameObject.SetActive(state.ShowUnitSphereWireframe);
            wireRenderer.gameObject.SetActive(state.ShowFunctionWireframe&&functionGeometryVisible);
            bool vectorsVisible=state.ShowNormalVectors&&functionGeometryVisible;
            if(vectorLines!=null)foreach(LineRenderer line in vectorLines)line.gameObject.SetActive(vectorsVisible);
        }

        private void CreateAxis(Vector3 direction,Vector3 side,Color color,string name)
        {
            const float length=2.25f,head=.18f;
            GameObject go=new GameObject(name+" Axis");go.transform.SetParent(axesRoot.transform,false);
            LineRenderer line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=2;line.SetPosition(0,-direction*length);line.SetPosition(1,direction*length);line.widthMultiplier=.022f;line.sharedMaterial=LineMaterial(color);
            CreateArrowSide(go.transform,direction*length,direction,side,head,color);CreateArrowSide(go.transform,direction*length,direction,-side,head,color);
        }

        private void CreateVectorLines()
        {
            const int count=48;vectorLines=new LineRenderer[count];GameObject root=new GameObject("Normal / Radial Vectors");root.transform.SetParent(transform,false);
            for(int i=0;i<count;i++){GameObject go=new GameObject("Vector "+i);go.transform.SetParent(root.transform,false);LineRenderer line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=2;line.widthMultiplier=.011f;line.sharedMaterial=new Material(Shader.Find("SphericalHarmonics/UnlitVertexColor"));vectorLines[i]=line;}
        }

        private static Color VectorColor(float value)=>value>=0?new Color(.18f,.55f,1f,1f):new Color(1f,.3f,.18f,1f);

        private void CreateArrowSide(Transform parent,Vector3 tip,Vector3 direction,Vector3 side,float size,Color color){GameObject arrow=new GameObject("Arrow");arrow.transform.SetParent(parent,false);LineRenderer line=arrow.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=2;line.SetPosition(0,tip);line.SetPosition(1,tip-direction*size+side*size*.55f);line.widthMultiplier=.022f;line.sharedMaterial=LineMaterial(color);}

        private MeshFilter CreateMeshObject(string name,out MeshRenderer renderer)
        {
            GameObject go=new GameObject(name);go.transform.SetParent(transform,false);MeshFilter filter=go.AddComponent<MeshFilter>();renderer=go.AddComponent<MeshRenderer>();return filter;
        }

        private static Material LineMaterial(Color color){Material m=new Material(Shader.Find("SphericalHarmonics/UnlitColor"));m.color=color;return m;}
        private void OnDestroy(){if(state!=null)state.Changed-=RefreshVisibility;}
    }
}
