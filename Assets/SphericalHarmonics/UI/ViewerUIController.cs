using UnityEngine;
using UnityEngine.UI;
using SphericalHarmonics.Fourier;
using SphericalHarmonics.Input;
using SphericalHarmonics.Math;
using SphericalHarmonics.Rotation;
using SphericalHarmonics.State;

namespace SphericalHarmonics.UI
{
    public sealed class ViewerUIController : MonoBehaviour
    {
        private static readonly Color TextColor=new Color(.10f,.14f,.19f);
        private static readonly Color Accent=new Color(.12f,.48f,.82f);
        private ViewerState state;
        private FourierBridgeController bridge;
        private RotationController rotation;
        private CameraOrbitController cameraOrbit;
        private RectTransform panel;
        private Text selection,formula,lLabel,mLabel,coefficientLabel,phaseLabel,flowLabel,axesLabel,hideButtonText;
        private Slider lSlider,mSlider,coefficientSlider,phaseSlider,flowSlider;
        private Toggle pureToggle;
        private Button sphereButton,orbitalButton,flowButton,rotateButton;
        private RectTransform[] lMarkers,mMarkers;
        private GameObject phaseRow,flowRow,rotationPanel,bridgePanel;
        private Slider rotX,rotY,rotZ;
        private bool updating;
        private Vector2Int lastScreen;

        public void Initialize(ViewerState viewerState,FourierBridgeController bridgeController,RotationController rotationController,CameraOrbitController orbit)
        {
            state=viewerState;bridge=bridgeController;rotation=rotationController;cameraOrbit=orbit;
            Build();state.Changed+=Refresh;bridge.Changed+=Refresh;Refresh();ApplyResponsiveLayout();
        }

        private void Build()
        {
            Canvas canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=20;
            CanvasScaler scaler=gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            panel=Rect("Control Panel",transform);panel.gameObject.AddComponent<Image>().color=new Color(.98f,.985f,.995f,.97f);
            ScrollRect scroll=panel.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;scroll.scrollSensitivity=24;
            RectTransform viewport=Rect("Viewport",panel);Stretch(viewport,10,10,10,10);Image viewportImage=viewport.gameObject.AddComponent<Image>();viewportImage.color=new Color(1,1,1,.01f);viewport.gameObject.AddComponent<Mask>().showMaskGraphic=false;scroll.viewport=viewport;
            RectTransform content=Rect("Content",viewport);content.anchorMin=new Vector2(0,1);content.anchorMax=new Vector2(1,1);content.pivot=new Vector2(.5f,1);content.offsetMin=Vector2.zero;content.offsetMax=Vector2.zero;
            VerticalLayoutGroup layout=content.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=8;layout.padding=new RectOffset(8,8,8,18);layout.childControlHeight=true;layout.childForceExpandHeight=false;layout.childControlWidth=true;
            ContentSizeFitter fitter=content.gameObject.AddComponent<ContentSizeFitter>();fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;scroll.content=content;

            Text title=Label(content,"Spherical Harmonics",22,FontStyle.Bold,34);title.color=new Color(.07f,.27f,.47f);
            selection=Label(content,"",14,FontStyle.Bold,24);
            Button[] displayButtons=AddButtonRow(content,new[]{("Sphere",(UnityEngine.Events.UnityAction)(()=>SetDisplay(DisplayMode.Sphere))),("Orbital",()=>SetDisplay(DisplayMode.Orbital)),("Flow",()=>SetDisplay(DisplayMode.Flow))});sphereButton=displayButtons[0];orbitalButton=displayButtons[1];flowButton=displayButtons[2];
            AddButtonRow(content,new[]{("Real",(UnityEngine.Events.UnityAction)(()=>SetValueType(ValueType.Real))),("Complex",()=>SetValueType(ValueType.Complex))});

            Text formulaHeading=Label(content,"Formula",14,FontStyle.Bold,20);formulaHeading.color=new Color(.07f,.27f,.47f);
            formula=Label(content,"",16,FontStyle.Italic,60);
            Font mathFont=Resources.Load<Font>("Fonts/NotoSansMath-Regular");
            if(mathFont!=null)formula.font=mathFont;
            formula.horizontalOverflow=HorizontalWrapMode.Wrap;formula.verticalOverflow=VerticalWrapMode.Overflow;
            lSlider=AddSlider(content,"Degree l",0,3,true,v=>{if(updating||bridge.Active)return;int l=Mathf.RoundToInt(v);state.Select(l,Mathf.Clamp(state.M,-l,l));},out lLabel);lMarkers=CreateDiscreteMarkers(lSlider,4);
            mSlider=AddSlider(content,"Order m",-3,3,true,v=>{if(updating)return;if(bridge.Active)bridge.SetM(Mathf.RoundToInt(v));else state.Select(state.L,Mathf.RoundToInt(v));},out mLabel);mMarkers=CreateDiscreteMarkers(mSlider,7);
            coefficientSlider=AddSlider(content,"Coefficient",-2,2,false,OnCoefficient,out coefficientLabel);
            phaseSlider=AddSlider(content,"Phase",-180,180,false,OnPhase,out phaseLabel);phaseRow=phaseSlider.transform.parent.gameObject;
            pureToggle=AddToggle(content,"Pure Mode — set selected (l,m) to 1",state.PureMode,v=>{if(!updating)state.SetPureMode(v);});
            AddButtonRow(content,new[]{("Zero Selected",(UnityEngine.Events.UnityAction)state.ZeroSelected),("Clear All",Clear)});
            flowSlider=AddSlider(content,"Flow Time",-.65f,.65f,false,v=>{if(!updating){state.FlowTime=v;state.NotifyChanged();}},out flowLabel);flowRow=flowSlider.transform.parent.gameObject;

            Text displayHeading=Label(content,"Display",14,FontStyle.Bold,22);displayHeading.color=new Color(.07f,.27f,.47f);
            AddToggle(content,"Function Surface",state.ShowFunctionSurface,v=>{state.ShowFunctionSurface=v;state.NotifyChanged();});
            AddToggle(content,"Function Wireframe (colored)",state.ShowFunctionWireframe,v=>{state.ShowFunctionWireframe=v;state.NotifyChanged();});
            AddToggle(content,"Unit Sphere Surface",state.ShowUnitSphereSurface,v=>{state.ShowUnitSphereSurface=v;state.NotifyChanged();});
            AddToggle(content,"Unit Sphere Wireframe",state.ShowUnitSphereWireframe,v=>{state.ShowUnitSphereWireframe=v;state.NotifyChanged();});
            AddToggle(content,"Normal / Radial Vectors",state.ShowNormalVectors,v=>{state.ShowNormalVectors=v;state.NotifyChanged();});
            AddToggle(content,"Coordinate Axes",state.ShowAxes,v=>{state.ShowAxes=v;state.NotifyChanged();});
            AddToggle(content,"Directional Light",state.DirectionalLight,v=>{state.DirectionalLight=v;state.NotifyChanged();});
            axesLabel=Label(content,"",12,FontStyle.Normal,72);axesLabel.verticalOverflow=VerticalWrapMode.Overflow;

            rotateButton=AddButtonRow(content,new[]{("Rotate",(UnityEngine.Events.UnityAction)OpenRotation),("Reset Axes",rotation.ResetAxes),("Reset Camera",cameraOrbit.ResetView)})[0];
            rotationPanel=Section(content,"Rotation");BuildRotationPanel(rotationPanel.transform);
            AddButtonRow(content,new[]{("Fourier Bridge",(UnityEngine.Events.UnityAction)ToggleBridge)});
            bridgePanel=Section(content,"Fourier Bridge — m controls l=|m|");BuildBridgePanel(bridgePanel.transform);
            rotationPanel.SetActive(false);bridgePanel.SetActive(false);

            Button hide=Button(transform,"Hide UI");RectTransform hideRect=(RectTransform)hide.transform;hideRect.anchorMin=hideRect.anchorMax=new Vector2(1,1);hideRect.pivot=new Vector2(1,1);hideRect.sizeDelta=new Vector2(92,34);hideRect.anchoredPosition=new Vector2(-12,-12);hideButtonText=hide.GetComponentInChildren<Text>();hide.onClick.AddListener(ToggleUiVisibility);
        }

        private void BuildRotationPanel(Transform parent)
        {
            AddButtonRow(parent,new[]{("Function",(UnityEngine.Events.UnityAction)(()=>rotation.SetMode(true))),("Coordinates",()=>rotation.SetMode(false))});
            rotX=AddSlider(parent,"X",-180,180,false,v=>RotationPreview());rotY=AddSlider(parent,"Y",-180,180,false,v=>RotationPreview());rotZ=AddSlider(parent,"Z",-180,180,false,v=>RotationPreview());
            AddButtonRow(parent,new[]{("Apply",(UnityEngine.Events.UnityAction)(()=>{rotation.Apply();rotationPanel.SetActive(false);})),("Cancel",()=>{rotation.Cancel();rotationPanel.SetActive(false);ResetRotationSliders();})});
        }

        private void BuildBridgePanel(Transform parent)
        {
            AddButtonRow(parent,new[]{("Sphere",(UnityEngine.Events.UnityAction)(()=>SetBridgeStage(BridgeStage.Sphere))),("Circle",()=>SetBridgeStage(BridgeStage.Circle)),("Line",()=>SetBridgeStage(BridgeStage.Line))});
            Label(parent,"Order m controls frequency; Degree follows l=|m|.",12,FontStyle.Normal,38);
            AddButtonRow(parent,new[]{("Exit Bridge",(UnityEngine.Events.UnityAction)(()=>{bridge.Exit();bridgePanel.SetActive(false);}))});
        }

        private void Refresh()
        {
            if(state==null)return;updating=true;
            selection.text=bridge.Active?$"Bridge {bridge.Stage} / {state.Value}":$"{state.Display} / {state.Value}";
            lSlider.SetValueWithoutNotify(state.L);lSlider.interactable=!bridge.Active;
            mSlider.minValue=bridge.Active?-3:-state.L;mSlider.maxValue=bridge.Active?3:state.L;mSlider.SetValueWithoutNotify(state.M);
            UpdateDiscreteMarkers(mMarkers,bridge.Active?7:2*state.L+1);
            lLabel.text=$"Degree l = {state.L}";mLabel.text=$"Order m = {state.M}";
            bool complex=state.Value==ValueType.Complex;phaseRow.SetActive(complex);flowRow.SetActive(state.Display==DisplayMode.Flow);
            if(complex)
            {
                ComplexValue c=state.Complex[state.L,state.M];coefficientSlider.minValue=0;coefficientSlider.SetValueWithoutNotify((float)c.Magnitude);phaseSlider.SetValueWithoutNotify((float)(c.Phase*Mathf.Rad2Deg));coefficientLabel.text=$"Magnitude = {c.Magnitude:0.000}";phaseLabel.text=$"Phase = {c.Phase*Mathf.Rad2Deg:0}°";
            }
            else
            {
                coefficientSlider.minValue=-2;coefficientSlider.SetValueWithoutNotify((float)state.Real[state.L,state.M]);coefficientLabel.text=$"Coefficient = {state.Real[state.L,state.M]:0.000}";
            }
            pureToggle.SetIsOnWithoutNotify(state.PureMode);pureToggle.interactable=true;
            rotateButton.interactable=!bridge.Active;sphereButton.interactable=true;orbitalButton.interactable=!bridge.Active||bridge.Stage==BridgeStage.Circle;flowButton.interactable=!bridge.Active;
            flowSlider.SetValueWithoutNotify(state.FlowTime);flowLabel.text=$"Flow Time = {state.FlowTime:0.00}";
            formula.text=BasisDefinitionTable.FormulaCard(state.L,state.M,complex,false)+"\n"+BasisDefinitionTable.Get(state.L,state.M).Label;
            axesLabel.text="Normalized axes (display coordinate order: x, z, y)\n"+AxisLine("X",Vector3.right)+"\n"+AxisLine("Z",Vector3.forward)+"\n"+AxisLine("Y",Vector3.up);
            updating=false;
        }

        private string AxisLine(string name,Vector3 axis)
        {
            Vector3 world=CoordinateSpace.AxisInWorld(state.CoordinateFrame,axis).normalized;
            return $"{name}=({world.x:+0.00;-0.00;0.00}, {world.z:+0.00;-0.00;0.00}, {world.y:+0.00;-0.00;0.00})";
        }

        private void OnCoefficient(float value)
        {
            if(updating)return;
            if(state.Value==ValueType.Real)state.Real[state.L,state.M]=value;
            else{ComplexValue old=state.Complex[state.L,state.M];state.Complex[state.L,state.M]=ComplexValue.FromPolar(value,old.Phase);}
            state.NotifyChanged();
        }
        private void OnPhase(float degrees){if(updating)return;ComplexValue old=state.Complex[state.L,state.M];state.Complex[state.L,state.M]=ComplexValue.FromPolar(old.Magnitude,degrees*Mathf.Deg2Rad);state.NotifyChanged();}
        private void Clear(){if(state.Value==ValueType.Real)state.Real.Clear();else state.Complex.Clear();state.NotifyChanged();}
        private void OpenRotation(){if(bridge.Active)return;rotationPanel.SetActive(true);rotation.Begin();ResetRotationSliders();}
        private void RotationPreview(){if(!updating)rotation.SetEuler(new Vector3(rotX.value,rotY.value,rotZ.value));}
        private void ResetRotationSliders(){updating=true;rotX.SetValueWithoutNotify(0);rotY.SetValueWithoutNotify(0);rotZ.SetValueWithoutNotify(0);updating=false;}
        private void ToggleBridge(){if(bridge.Active){bridge.Exit();bridgePanel.SetActive(false);}else{if(rotation.IsOpen)rotation.Cancel();rotation.ResetAxes();rotationPanel.SetActive(false);bridge.Enter(state.Value,state.M);bridgePanel.SetActive(true);}}
        private void SetDisplay(DisplayMode mode){if(bridge.Active&&(mode==DisplayMode.Flow||(mode==DisplayMode.Orbital&&bridge.Stage!=BridgeStage.Circle)))return;state.TrySetDisplay(mode);}
        private void SetValueType(ValueType type){if(bridge.Active)bridge.SetValueType(type);else state.TrySetValueType(type);}
        private void SetBridgeStage(BridgeStage stage){bridge.SetStage(stage);}
        private void ToggleUiVisibility(){bool visible=!panel.gameObject.activeSelf;panel.gameObject.SetActive(visible);hideButtonText.text=visible?"Hide UI":"Show UI";}

        private void Update(){if(lastScreen.x!=Screen.width||lastScreen.y!=Screen.height)ApplyResponsiveLayout();}
        private void ApplyResponsiveLayout()
        {
            lastScreen=new Vector2Int(Screen.width,Screen.height);bool portrait=Screen.height>Screen.width;
            if(portrait){panel.anchorMin=new Vector2(0,0);panel.anchorMax=new Vector2(1,.52f);panel.pivot=new Vector2(.5f,.5f);panel.offsetMin=new Vector2(8,8);panel.offsetMax=new Vector2(-8,-8);}
            else{panel.anchorMin=new Vector2(1,0);panel.anchorMax=new Vector2(1,1);panel.pivot=new Vector2(1,.5f);panel.sizeDelta=new Vector2(370,0);panel.anchoredPosition=new Vector2(-10,0);panel.offsetMin=new Vector2(panel.offsetMin.x,10);panel.offsetMax=new Vector2(panel.offsetMax.x,-10);}
        }

        private static GameObject Section(Transform parent,string name){RectTransform r=Rect(name,parent);VerticalLayoutGroup l=r.gameObject.AddComponent<VerticalLayoutGroup>();l.spacing=6;l.padding=new RectOffset(7,7,7,7);l.childControlHeight=true;l.childForceExpandHeight=false;Image i=r.gameObject.AddComponent<Image>();i.color=new Color(.92f,.95f,.98f,.98f);ContentSizeFitter f=r.gameObject.AddComponent<ContentSizeFitter>();f.verticalFit=ContentSizeFitter.FitMode.PreferredSize;return r.gameObject;}
        private static Button[] AddButtonRow(Transform parent,(string,UnityEngine.Events.UnityAction)[] items){RectTransform row=Rect("Buttons",parent);HorizontalLayoutGroup l=row.gameObject.AddComponent<HorizontalLayoutGroup>();l.spacing=5;l.childControlWidth=true;l.childForceExpandWidth=true;l.childControlHeight=true;row.gameObject.AddComponent<LayoutElement>().preferredHeight=34;Button[] buttons=new Button[items.Length];for(int i=0;i<items.Length;i++){Button b=Button(row,items[i].Item1);b.onClick.AddListener(items[i].Item2);buttons[i]=b;}return buttons;}
        private static Button Button(Transform parent,string value){RectTransform r=Rect(value,parent);Image image=r.gameObject.AddComponent<Image>();image.color=new Color(.78f,.88f,.97f,1);Button b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;ColorBlock colors=b.colors;colors.highlightedColor=new Color(.66f,.82f,.96f);colors.pressedColor=new Color(.52f,.74f,.92f);b.colors=colors;Text t=Label(r,value,13,FontStyle.Bold,30);Stretch(t.rectTransform,4,4,2,2);t.alignment=TextAnchor.MiddleCenter;return b;}
        private static Toggle AddToggle(Transform parent,string value,bool initial,UnityEngine.Events.UnityAction<bool> action){RectTransform row=Rect(value,parent);row.gameObject.AddComponent<LayoutElement>().preferredHeight=28;Toggle t=row.gameObject.AddComponent<Toggle>();Image bg=Rect("Box",row).gameObject.AddComponent<Image>();bg.rectTransform.anchorMin=new Vector2(0,.5f);bg.rectTransform.anchorMax=new Vector2(0,.5f);bg.rectTransform.sizeDelta=new Vector2(21,21);bg.rectTransform.anchoredPosition=new Vector2(11,0);bg.color=new Color(.78f,.82f,.87f);Image check=Rect("Check",bg.transform).gameObject.AddComponent<Image>();Stretch(check.rectTransform,4,4,4,4);check.color=Accent;t.targetGraphic=bg;t.graphic=check;t.isOn=initial;Text label=Label(row,value,13,FontStyle.Normal,26);label.rectTransform.offsetMin=new Vector2(36,0);label.alignment=TextAnchor.MiddleLeft;t.onValueChanged.AddListener(action);return t;}
        private static Slider AddSlider(Transform parent,string name,float min,float max,bool whole,UnityEngine.Events.UnityAction<float> action)=>AddSlider(parent,name,min,max,whole,action,out _);
        private static Slider AddSlider(Transform parent,string name,float min,float max,bool whole,UnityEngine.Events.UnityAction<float> action,out Text label)
        {
            RectTransform group=Rect(name,parent);VerticalLayoutGroup layout=group.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=3;layout.childForceExpandHeight=false;ContentSizeFitter fit=group.gameObject.AddComponent<ContentSizeFitter>();fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            label=Label(group,name,13,FontStyle.Normal,20);RectTransform track=Rect("Track",group);track.gameObject.AddComponent<LayoutElement>().preferredHeight=36;
            Slider s=track.gameObject.AddComponent<Slider>();s.minValue=min;s.maxValue=max;s.wholeNumbers=whole;
            Image background=Rect("Background",track).gameObject.AddComponent<Image>();Stretch(background.rectTransform,0,0,7,7);background.color=new Color(.75f,.81f,.87f);
            Image fill=Rect("Fill",track).gameObject.AddComponent<Image>();Stretch(fill.rectTransform,0,0,7,7);fill.color=Accent;s.fillRect=fill.rectTransform;
            RectTransform handleArea=Rect("Handle Slide Area",track);Stretch(handleArea,12,12,0,0);
            Image handle=Rect("Handle",handleArea).gameObject.AddComponent<Image>();handle.rectTransform.anchorMin=handle.rectTransform.anchorMax=new Vector2(.5f,.5f);handle.rectTransform.sizeDelta=new Vector2(24,30);handle.gameObject.AddComponent<SliderHandleConstraint>();handle.color=new Color(.08f,.27f,.46f);s.handleRect=handle.rectTransform;s.targetGraphic=handle;s.onValueChanged.AddListener(action);return s;
        }

        private static RectTransform[] CreateDiscreteMarkers(Slider slider,int maximumCount)
        {
            RectTransform[] markers=new RectTransform[maximumCount];RectTransform track=(RectTransform)slider.transform;
            RectTransform markerArea=Rect("Discrete Marker Area",track);Stretch(markerArea,12,12,0,0);markerArea.SetSiblingIndex(slider.fillRect.GetSiblingIndex());
            for(int i=0;i<maximumCount;i++){Image marker=Rect("Discrete Marker",markerArea).gameObject.AddComponent<Image>();marker.color=new Color(.35f,.43f,.51f,.82f);RectTransform r=marker.rectTransform;r.anchorMin=r.anchorMax=new Vector2(0,.5f);r.sizeDelta=new Vector2(4,27);markers[i]=r;}
            UpdateDiscreteMarkers(markers,maximumCount);return markers;
        }

        private static void UpdateDiscreteMarkers(RectTransform[] markers,int activeCount)
        {
            if(markers==null)return;activeCount=Mathf.Clamp(activeCount,1,markers.Length);
            for(int i=0;i<markers.Length;i++){bool active=i<activeCount;markers[i].gameObject.SetActive(active);if(active){float t=activeCount==1?.5f:(float)i/(activeCount-1);markers[i].anchorMin=markers[i].anchorMax=new Vector2(t,.5f);markers[i].anchoredPosition=Vector2.zero;}}
        }

        private static Text Label(Transform parent,string value,int size,FontStyle style,float height){RectTransform r=Rect("Label",parent);Text t=r.gameObject.AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;t.fontSize=size;t.fontStyle=style;t.color=TextColor;t.alignment=TextAnchor.MiddleLeft;LayoutElement e=r.gameObject.AddComponent<LayoutElement>();e.preferredHeight=height;return t;}
        private static RectTransform Rect(string name,Transform parent){GameObject go=new GameObject(name,typeof(RectTransform));RectTransform r=(RectTransform)go.transform;r.SetParent(parent,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;return r;}
        private static void Stretch(RectTransform r,float left,float right,float bottom,float top){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(left,bottom);r.offsetMax=new Vector2(-right,-top);}
        private void OnDestroy(){if(state!=null)state.Changed-=Refresh;if(bridge!=null)bridge.Changed-=Refresh;}
    }
}
