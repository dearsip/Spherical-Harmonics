using UnityEngine;
using UnityEngine.EventSystems;
using SphericalHarmonics.Fourier;
using SphericalHarmonics.Input;
using SphericalHarmonics.Rendering;
using SphericalHarmonics.Rotation;
using SphericalHarmonics.State;
using SphericalHarmonics.UI;

namespace SphericalHarmonics
{
    public sealed class ViewerBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            if(FindObjectOfType<ViewerBootstrap>()!=null)return;
            new GameObject("Spherical Harmonics Viewer").AddComponent<ViewerBootstrap>().Initialize();
        }

        private void Initialize()
        {
            Application.targetFrameRate=60;
            Camera camera=Camera.main;
            if(camera==null){GameObject c=new GameObject("Main Camera");camera=c.AddComponent<Camera>();c.tag="MainCamera";c.AddComponent<AudioListener>();}
            camera.backgroundColor=new Color(.94f,.955f,.975f);camera.clearFlags=CameraClearFlags.SolidColor;camera.nearClipPlane=.05f;
            CameraOrbitController orbit=camera.GetComponent<CameraOrbitController>()??camera.gameObject.AddComponent<CameraOrbitController>();
            if(FindObjectOfType<EventSystem>()==null){GameObject events=new GameObject("EventSystem");events.AddComponent<EventSystem>();events.AddComponent<StandaloneInputModule>();}

            ViewerState state=new ViewerState();
            FourierBridgeController bridge=gameObject.AddComponent<FourierBridgeController>();bridge.Initialize(state);
            VisualizationRenderer visualization=gameObject.AddComponent<VisualizationRenderer>();visualization.Initialize(state,bridge);
            RotationController rotation=new RotationController(state,visualization);
            GameObject ui=new GameObject("Viewer UI");ui.transform.SetParent(transform,false);ui.AddComponent<ViewerUIController>().Initialize(state,bridge,rotation,orbit);
        }
    }
}
