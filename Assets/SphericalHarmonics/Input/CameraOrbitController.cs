using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace SphericalHarmonics.Input
{
    public sealed class CameraOrbitController : MonoBehaviour
    {
        [SerializeField] private float orbitSpeed=.18f;
        [SerializeField] private float zoomSpeed=.8f;
        [SerializeField] private float minDistance=1.35f,maxDistance=20f;
        private float yaw=35,pitch=18,distance=4.6f;
        private Vector2 lastMouse;private float lastPinch;
        private bool mouseCapturedByUi,pinching;
        private readonly HashSet<int> uiTouches=new HashSet<int>();
        private readonly List<RaycastResult> uiRaycastResults=new List<RaycastResult>();

        private void Awake()
        {
            // A touch must be handled by exactly one path. In particular, WebGL/mobile
            // must not feed the same gesture through Unity's simulated mouse events.
            if(UnityEngine.Input.touchSupported)UnityEngine.Input.simulateMouseWithTouches=false;
        }

        private void LateUpdate()
        {
            if(UnityEngine.Input.touchCount>0){mouseCapturedByUi=false;HandleTouch();}
            else{pinching=false;uiTouches.Clear();HandleMouse();}
            pitch=Mathf.Clamp(pitch,-89.9f,89.9f);distance=Mathf.Clamp(distance,minDistance,maxDistance);
            Quaternion rotation=Quaternion.Euler(pitch,yaw,0);transform.position=rotation*new Vector3(0,0,-distance);transform.rotation=rotation;
        }

        public void ResetView(){yaw=35;pitch=18;distance=4.6f;}

        private void HandleMouse()
        {
            if(UnityEngine.Input.GetMouseButtonDown(0)){lastMouse=UnityEngine.Input.mousePosition;mouseCapturedByUi=PointerOverUi(-1);}
            if(UnityEngine.Input.GetMouseButton(0))
            {
                Vector2 current=UnityEngine.Input.mousePosition;
                if(!mouseCapturedByUi){Vector2 delta=current-lastMouse;yaw+=delta.x*orbitSpeed;pitch-=delta.y*orbitSpeed;}
                lastMouse=current;
            }
            if(UnityEngine.Input.GetMouseButtonUp(0))mouseCapturedByUi=false;
            if(!PointerOverUi(-1))distance-=UnityEngine.Input.mouseScrollDelta.y*zoomSpeed;
        }

        private void HandleTouch()
        {
            for(int i=0;i<UnityEngine.Input.touchCount;i++)
            {
                Touch t=UnityEngine.Input.GetTouch(i);
                if((t.phase==TouchPhase.Began||t.phase==TouchPhase.Moved)&&ScreenPointOverUi(t.position))uiTouches.Add(t.fingerId);
                if(t.phase==TouchPhase.Ended||t.phase==TouchPhase.Canceled)uiTouches.Remove(t.fingerId);
            }
            if(UnityEngine.Input.touchCount==1)
            {
                pinching=false;
                Touch t=UnityEngine.Input.GetTouch(0);
                if(t.phase==TouchPhase.Moved&&!uiTouches.Contains(t.fingerId))
                {
                    // Direct-manipulation convention: the model follows the finger.
                    Vector2 orbitDelta=CameraGestureMath.TouchOrbitDelta(t.deltaPosition,orbitSpeed);
                    yaw+=orbitDelta.x;pitch+=orbitDelta.y;
                }
            }
            else if(UnityEngine.Input.touchCount==2)
            {
                Touch a=UnityEngine.Input.GetTouch(0),b=UnityEngine.Input.GetTouch(1);float pinch=Vector2.Distance(a.position,b.position);
                if(pinching&&!uiTouches.Contains(a.fingerId)&&!uiTouches.Contains(b.fingerId))distance-=(pinch-lastPinch)*.012f;
                lastPinch=pinch;
                pinching=true;
            }
        }

        private bool ScreenPointOverUi(Vector2 position)
        {
            if(EventSystem.current==null)return false;
            PointerEventData pointer=new PointerEventData(EventSystem.current){position=position};
            uiRaycastResults.Clear();EventSystem.current.RaycastAll(pointer,uiRaycastResults);
            return uiRaycastResults.Count>0;
        }

        private static bool PointerOverUi(int id)=>EventSystem.current!=null&&EventSystem.current.IsPointerOverGameObject(id);
    }
}
