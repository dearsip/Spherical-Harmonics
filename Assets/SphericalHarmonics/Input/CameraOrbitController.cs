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
        private bool mouseCapturedByUi;
        private readonly HashSet<int> uiTouches=new HashSet<int>();

        private void LateUpdate()
        {
            HandleMouse();HandleTouch();pitch=Mathf.Clamp(pitch,-89.9f,89.9f);distance=Mathf.Clamp(distance,minDistance,maxDistance);
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
            for(int i=0;i<UnityEngine.Input.touchCount;i++){Touch t=UnityEngine.Input.GetTouch(i);if(t.phase==TouchPhase.Began&&PointerOverUi(t.fingerId))uiTouches.Add(t.fingerId);if(t.phase==TouchPhase.Ended||t.phase==TouchPhase.Canceled)uiTouches.Remove(t.fingerId);}
            if(UnityEngine.Input.touchCount==1)
            {
                Touch t=UnityEngine.Input.GetTouch(0);if(t.phase==TouchPhase.Moved&&!uiTouches.Contains(t.fingerId)){yaw+=t.deltaPosition.x*orbitSpeed;pitch-=t.deltaPosition.y*orbitSpeed;}
            }
            else if(UnityEngine.Input.touchCount==2)
            {
                Touch a=UnityEngine.Input.GetTouch(0),b=UnityEngine.Input.GetTouch(1);float pinch=Vector2.Distance(a.position,b.position);
                if(a.phase!=TouchPhase.Began&&b.phase!=TouchPhase.Began&&!uiTouches.Contains(a.fingerId)&&!uiTouches.Contains(b.fingerId))distance-=(pinch-lastPinch)*.012f;
                lastPinch=pinch;
            }
        }

        private static bool PointerOverUi(int id)=>EventSystem.current!=null&&EventSystem.current.IsPointerOverGameObject(id);
    }
}
