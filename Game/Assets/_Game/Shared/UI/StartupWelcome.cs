using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 // A lightweight, fully local entry screen. The painted scene is independent of
 // the island bundle, so it is visible while that bundle is being prepared.
 public sealed partial class StartupWelcome:MonoBehaviour {
  public static StartupWelcome Instance {get;private set;}
  public static bool Blocking=>Instance&&Instance.Visible;
  public bool Visible {get;private set;}=true;
  public bool IsReady {get;private set;}
  public bool HasError {get;private set;}
  public float Progress {get;private set;}
  public Canvas Canvas {get;private set;}
  public RectTransform Design {get;private set;}
  public RectTransform LoadingPanel {get;private set;}
  public Button GuestButton {get;private set;}
  public Button HuaweiButton {get;private set;}
  public Button RetryButton {get;private set;}
  public Button LanguageButton {get;private set;}
  public Button SettingsButton {get;private set;}
  public Button InfoButton {get;private set;}
  public InputField GuestNameField {get;private set;}
  public Button GuestContinueButton {get;private set;}
  public bool ModalOpen=>modal&&modal.gameObject.activeSelf;
  public bool ArtworkLoaded {get;private set;}
  public Rect SafeArea {get;private set;}
  public const float Width=1672,Height=941;
  RectTransform cover,actions,modal;CanvasGroup actionGroup;GameObject blankSign;
  Text heading,subtitle,status,percentage;WelcomePlate fill;
  GameObject modalRoot;Button returnFocus;Action retry;
  bool driven,leaving;float hideProgressAt=-1;
  static readonly Color Ink=LocalProfile.Hex("064B55"),Cream=LocalProfile.Hex("FFF7E3");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public Rect? ReviewSafeArea;
#endif
  public static StartupWelcome Ensure(bool externallyDriven=false){
   if(Instance)return Instance;
   var view=new GameObject("Startup welcome").AddComponent<StartupWelcome>();view.driven=externallyDriven;return view;
  }
  void Awake(){Instance=this;MenuPreferences.Load();Create();CreateLanguageArtwork();GameLocalization.Changed+=RefreshLanguageArtwork;RefreshLanguageArtwork();}
  IEnumerator Start(){
   if(driven)yield break;
   SetProgress(.03f,"Preparing your islands...");
   while(!AppRoot.Instance||!AppRoot.Instance.Menu)yield return null;
   SetReady();
  }
  public void SetRetry(Action action)=>retry=action;
  public void BeginLoading(){
   HasError=false;IsReady=false;Progress=0;hideProgressAt=-1;CloseModal();
   SetBaseInteractable(!ModalOpen);
   actionGroup.alpha=0;actionGroup.interactable=actionGroup.blocksRaycasts=false;
   GuestButton.interactable=HuaweiButton.interactable=false;RetryButton.gameObject.SetActive(false);
   blankSign.SetActive(true);heading.gameObject.SetActive(true);
   heading.text="Welcome aboard!";subtitle.text="Your island adventure is getting ready.";subtitle.gameObject.SetActive(true);
   LoadingPanel.gameObject.SetActive(true);percentage.gameObject.SetActive(true);SetProgress(0,"Preparing your islands...");
  }
  public void SetProgress(float value,string label){
   if(IsReady||HasError)return;
   Progress=Mathf.Clamp(value,0,.99f);status.text=label;percentage.text=Mathf.FloorToInt(Progress*100)+"%";
   fill.rectTransform.sizeDelta=new Vector2(258*Progress,22);fill.SetVerticesDirty();
  }
  public void ShowError(string cause=""){
   HasError=true;IsReady=false;hideProgressAt=-1;
   SetBaseInteractable(!ModalOpen);
   actionGroup.alpha=0;actionGroup.interactable=actionGroup.blocksRaycasts=false;
   GuestButton.interactable=HuaweiButton.interactable=false;
   blankSign.SetActive(true);heading.gameObject.SetActive(true);
   heading.text="Let’s try again";subtitle.text="Your adventure will be here when you’re ready.";subtitle.gameObject.SetActive(true);
   status.text=cause.IndexOf("space",StringComparison.OrdinalIgnoreCase)>=0||cause.IndexOf("disk",StringComparison.OrdinalIgnoreCase)>=0?"Free some storage, then retry.":cause.IndexOf("hash",StringComparison.OrdinalIgnoreCase)>=0||cause.IndexOf("integrity",StringComparison.OrdinalIgnoreCase)>=0?"Content needs repair. Retry loading.":"Could not load islands. Try again.";percentage.gameObject.SetActive(false);RetryButton.gameObject.SetActive(true);LoadingPanel.gameObject.SetActive(true);
  }
  public void SetReady(bool allowBypass=true){
   if(IsReady)return;IsReady=true;HasError=false;Progress=1;
   blankSign.SetActive(false);heading.gameObject.SetActive(false);subtitle.gameObject.SetActive(false);
   actionGroup.alpha=1;actionGroup.interactable=actionGroup.blocksRaycasts=true;
   GuestButton.interactable=HuaweiButton.interactable=true;RetryButton.gameObject.SetActive(false);
   status.text="Ready to play";percentage.text="100%";fill.rectTransform.sizeDelta=new Vector2(258,22);fill.SetVerticesDirty();hideProgressAt=Time.unscaledTime+.8f;
   SetBaseInteractable(!ModalOpen);if(allowBypass&&BypassRequested())Dismiss(false);
  }
  static bool BypassRequested(){
   var args=Environment.GetCommandLineArgs();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(args.Contains("-startupWelcomeReview")||args.Contains("-profilesReview")||args.Contains("-localizationReview"))return false;
   if(args.Contains("-skipWelcome")||args.Contains("-probe")||args.Any(a=>a.StartsWith("-")&&(a.EndsWith("Review",StringComparison.OrdinalIgnoreCase)||a.EndsWith("Audit",StringComparison.OrdinalIgnoreCase))))return true;
#endif
   // Existing explicit Explore launchers continue to launch the selected sport.
   return args.Any(a=>a=="-offline"||a=="-localHost"||a=="-localClient"||a=="-onlineHost"||a=="-onlineJoin");
  }
  public void ContinueAsGuest(){
   if(!IsReady||HasError||leaving||ModalOpen)return;
   returnFocus=GuestButton;CreateModal();
   var card=Plate(modal,"Guest profile",476,191,720,540,"FFFAEB","F5E9CF","76AA9C",32,true);
   Label(card.transform,PlayerIdentity.HasProfile?"Welcome back!":"What should we call you?",34,22,652,65,35,true,Ink,TextAnchor.MiddleCenter);
   Label(card.transform,"Your name appears to other players.",34,92,652,45,24,false,Ink,TextAnchor.MiddleCenter);
   GuestNameField=CoveUI.Field(card.transform,"Guest name","Your name",PlayerIdentity.SavedName,44,159,632,PlayerIdentity.NameLimit);
   var hint=Label(card.transform,"",44,240,632,60,22,false,Ink,TextAnchor.MiddleCenter);
   Label(card.transform,PlayerIdentity.AccountId.Length>0?"Your account is linked. Name edits sync when you go online.":"Guest saved on this device. Link a sign-in account to recover it on another phone.",44,312,632,69,23,false,Ink,TextAnchor.MiddleCenter);
   ActionButton(card.transform,"Cancel guest profile","Back",44,423,230,70,"FFFAEB","F5E9CF","76AA9C",CloseModal,WelcomeSymbol.None,26);
   GuestContinueButton=ActionButton(card.transform,"Confirm guest profile","Let's play",306,423,370,70,"C9FFF0","82DDC4","57A996",()=>{
    string previous=PlayerIdentity.SavedName;
    if(!PlayerIdentity.SaveName(GuestNameField.text,out var error)){hint.text=error;return;}
    if(previous!=PlayerIdentity.SavedName)PlayerProfiles.SaveLocalName(GuestNameField.text);
    CloseModal();Dismiss(true);
   },WelcomeSymbol.None,27);
   void Validate(string value){GuestContinueButton.interactable=PlayerIdentity.ValidateName(value,out _,out var error);hint.text=error.Length>0?error:"Up to 16 characters. You can change this later.";}
   GuestNameField.onValueChanged.AddListener(Validate);Validate(GuestNameField.text);SetBaseInteractable(false);GuestNameField.Select();
  }
  void Dismiss(bool goHome){
   if(leaving)return;leaving=true;Visible=false;
   if(EventSystem.current)EventSystem.current.SetSelectedGameObject(null);
   if(AppRoot.Instance){AppRoot.Instance.view.InputBlocked=false;if(goHome)AppRoot.Instance.Show("home");}
   Canvas.gameObject.SetActive(false);Destroy(gameObject);
  }
  void Update(){
   if(!Visible)return;Layout();RefreshLanguageArtwork();
   if(hideProgressAt>=0&&Time.unscaledTime>=hideProgressAt){LoadingPanel.gameObject.SetActive(false);hideProgressAt=-1;}
   if(Input.GetKeyDown(KeyCode.Escape)&&ModalOpen&&!AccountPanel.Visible&&!LanguageSettings.DismissTop())CloseModal();
   if(Input.GetKeyDown(KeyCode.Tab)&&EventSystem.current&&!EventSystem.current.currentSelectedGameObject)(IsReady?GuestButton:LanguageButton).Select();
  }
  void OnDestroy(){GameLocalization.Changed-=RefreshLanguageArtwork;if(Instance==this)Instance=null;}
  void Create(){
   Canvas=new GameObject("Welcome canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).GetComponent<Canvas>();
   Canvas.transform.SetParent(transform,false);Canvas.renderMode=RenderMode.ScreenSpaceOverlay;Canvas.sortingOrder=30000;
   // Pixel-space layout keeps every overlay locked to the approved 16:9 artwork.
   Canvas.GetComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
   var texture=Resources.Load<Texture2D>("StartupWelcome/Background");
   var signTexture=Resources.Load<Texture2D>("StartupWelcome/BlankSign");ArtworkLoaded=texture&&signTexture;
   cover=Rect(Canvas.transform,"Safe frame backdrop",0,0,Width,Height);var backdrop=cover.gameObject.AddComponent<WelcomePlate>();backdrop.top=LocalProfile.Hex("236C86");backdrop.bottom=LocalProfile.Hex("103F50");backdrop.edge=backdrop.bottom;backdrop.radius=0;backdrop.sheen=false;backdrop.raycastTarget=true;
   Design=Rect(Canvas.transform,"Approved artwork frame",0,0,Width,Height);
   var art=Rect(Design,"Cable-car welcome artwork",0,0,Width,Height).gameObject.AddComponent<RawImage>();art.texture=texture;art.raycastTarget=true;
   // Keep the approved scene byte-for-byte. Only this small sign interior uses
   // the blank companion plate; generated changes elsewhere are never shown.
   var sign=Rect(Design,"Blank sign during preparation",1138,340,484,320).gameObject.AddComponent<WelcomeSignPatch>();
   sign.texture=signTexture;sign.source=new UnityEngine.Rect(1138/Width,1-660/Height,484/Width,320/Height);sign.raycastTarget=false;blankSign=sign.gameObject;
   heading=Label(Design,"Welcome aboard!",1162,337,436,72,38,true,Ink,TextAnchor.MiddleCenter);
   subtitle=Label(Design,"Your island adventure is getting ready.",1170,455,416,104,26,false,Ink,TextAnchor.MiddleCenter);
   actions=Rect(Design,"Sign-in choices",1153,444,457,212);actionGroup=actions.gameObject.AddComponent<CanvasGroup>();
   HuaweiButton=ArtworkButton(actions,"Huawei sign-in",0,0,457,100,new UnityEngine.Rect(2,1,453,90),45,()=>{
    if(!AndroidAccountSignIn.Supported)ShowPlaceholder("Account sign-in","Huawei and Google sign-in use the Android phone's account screen.\nGuest play is available here.",HuaweiButton);
    else AccountPanel.Open(()=>{CloseModal();ContinueAsGuest();});
   });
   if(AndroidAccountSignIn.Supported&&AndroidAccountSignIn.Capabilities.Preferred==AccountProvider.Google){
    Plate(HuaweiButton.transform,"Google sign-in plate",2,1,453,90,"FFFFFF","FFF7E3","76AA9C",45,false);
    Label(HuaweiButton.transform,"Sign in with Google",22,1,413,90,28,true,Ink,TextAnchor.MiddleCenter);
   }
   GuestButton=ArtworkButton(actions,"Continue as guest",0,112,457,100,new UnityEngine.Rect(2,1,453,90),45,ContinueAsGuest);
   LanguageButton=Utility("English",1258,OpenLanguagePicker);
   SettingsButton=Utility("Settings",1398,OpenWelcomeSettings);
   InfoButton=Utility("Info",1533,OpenWelcomeInfo);
   // Immediately right of the character's shins, above the boardwalk edge.
   LoadingPanel=Rect(Design,"Loading beside character",750,708,350,118);
   var plate=LoadingPanel.gameObject.AddComponent<WelcomePlate>();plate.top=Cream;plate.bottom=LocalProfile.Hex("F9ECCE");plate.edge=LocalProfile.Hex("8AAEA0");plate.radius=23;plate.raycastTarget=false;
   status=Label(LoadingPanel,"Preparing your islands...",20,12,312,50,22,true,Ink);
   Plate(LoadingPanel,"Progress track",20,69,258,22,"CAD5D0","E0E4D8","C5CFC7",11,false);
   fill=Plate(LoadingPanel,"Actual load progress",20,69,0,22,"4BD9FC","0CA7E2","178FB7",11,false);
   percentage=Label(LoadingPanel,"0%",283,61,59,38,24,true,Ink,TextAnchor.MiddleCenter);percentage.resizeTextForBestFit=true;percentage.resizeTextMinSize=14;percentage.resizeTextMaxSize=24;
   RetryButton=ActionButton(Design,"Retry content loading","Try again",1216,570,320,88,"C9FFF0","82DDC4","57A996",()=>{if(HasError){BeginLoading();retry?.Invoke();}},WelcomeSymbol.None,30);
   if(!EventSystem.current)new GameObject("Welcome input",typeof(EventSystem),typeof(StandaloneInputModule));
   BeginLoading();Layout();
  }
  public void Layout(){
   SafeArea=Screen.safeArea;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(ReviewSafeArea.HasValue)SafeArea=ReviewSafeArea.Value;
#endif
   if(SafeArea.width<=0||SafeArea.height<=0)SafeArea=new Rect(0,0,Screen.width,Screen.height);
   float scale=Mathf.Min(SafeArea.width/Width,SafeArea.height/Height);
   // Scale the canvas, not a descendant transform, so dynamic fonts rasterize
   // at the actual display pixel density (including high-resolution monitors).
   Canvas.GetComponent<CanvasScaler>().scaleFactor=scale;Canvas.scaleFactor=scale;
   Design.localScale=Vector3.one;Design.anchoredPosition=new Vector2(SafeArea.center.x-Width*scale/2,-(Screen.height-SafeArea.center.y-Height*scale/2))/scale;
   float coverScale=Mathf.Max(Screen.width/Width,Screen.height/Height);
   cover.localScale=Vector3.one*(coverScale/scale);cover.anchoredPosition=new Vector2((Screen.width-Width*coverScale)/2,-(Screen.height-Height*coverScale)/2)/scale;
  }
  // Both the circle and its illustrated label belong to the touch target.
  Button Utility(string label,float x,Action click)=>ArtworkButton(Design,label,x,34,114,124,new UnityEngine.Rect(20,6,74,74),37,click);
  static Button ArtworkButton(Transform parent,string name,float x,float y,float w,float h,UnityEngine.Rect visual,float radius,Action click){
   var feedback=Rect(parent,name,x,y,w,h).gameObject.AddComponent<WelcomeArtworkFeedback>();feedback.visual=visual;feedback.radius=radius;
   var button=feedback.gameObject.AddComponent<WelcomeButton>();button.targetGraphic=feedback;button.transition=Selectable.Transition.None;
   button.onClick.AddListener(()=>{CoveSound.Play();click?.Invoke();});return button;
  }
  public void ShowPlaceholder(string title,string message,Button source){
   if(ModalOpen)return;returnFocus=source;CreateModal();
   var card=Plate(modal,"Dialog",536,299,600,343,"FFFAEB","F5E9CF","76AA9C",32,true);
   Label(card.transform,title,34,25,532,63,35,true,Ink,TextAnchor.MiddleCenter);
   Label(card.transform,message,38,103,524,123,24,false,Ink,TextAnchor.MiddleCenter);
   var close=ActionButton(card.transform,"Close welcome dialog","Got it",162,255,276,60,"C9FFF0","82DDC4","57A996",CloseModal,WelcomeSymbol.None,25);
   SetBaseInteractable(false);close.Select();
  }
  void CreateModal(){
   modalRoot=new GameObject("Welcome dialog",typeof(RectTransform));modal=modalRoot.GetComponent<RectTransform>();modal.SetParent(Design,false);modal.anchorMin=modal.anchorMax=modal.pivot=new Vector2(0,1);modal.sizeDelta=new Vector2(Width,Height);
   var shade=modal.gameObject.AddComponent<Image>();shade.color=new Color(.015f,.10f,.12f,.68f);shade.raycastTarget=true;
  }
  void SetBaseInteractable(bool enabled){
   LanguageButton.interactable=enabled;
   // These local panels do not depend on the island bundle or account readiness.
   SettingsButton.interactable=InfoButton.interactable=enabled;
   GuestButton.interactable=HuaweiButton.interactable=enabled&&IsReady&&!HasError;RetryButton.interactable=enabled;
  }
  public void CloseModal(){
   if(!modalRoot)return;MenuPreferences.Save();modalRoot.SetActive(false);Destroy(modalRoot);modalRoot=null;modal=null;SetBaseInteractable(true);
   if(returnFocus&&returnFocus.IsActive()&&returnFocus.IsInteractable())returnFocus.Select();else if(EventSystem.current)EventSystem.current.SetSelectedGameObject(null);returnFocus=null;
  }
  static RectTransform Rect(Transform parent,string name,float x,float y,float w,float h)=>CoveUI.Rect(parent,name,x,y,w,h);
  static Text Label(Transform parent,string value,float x,float y,float w,float h,int size,bool bold,Color color,TextAnchor align=TextAnchor.MiddleLeft)=>CoveUI.Text(parent,value,x,y,w,h,size,bold,color,align);
  static WelcomePlate Plate(Transform parent,string name,float x,float y,float w,float h,string top,string bottom,string edge,float radius,bool shadow){
   var p=Rect(parent,name,x,y,w,h).gameObject.AddComponent<WelcomePlate>();p.top=LocalProfile.Hex(top);p.bottom=LocalProfile.Hex(bottom);p.edge=LocalProfile.Hex(edge);p.radius=radius;p.shadow=shadow;p.raycastTarget=false;return p;
  }
  static Button ActionButton(Transform parent,string name,string label,float x,float y,float w,float h,string top,string bottom,string edge,Action click,WelcomeSymbol symbol,int size=30){
   var plate=Plate(parent,name,x,y,w,h,top,bottom,edge,h/2,true);plate.raycastTarget=true;
   var button=plate.gameObject.AddComponent<WelcomeButton>();button.targetGraphic=plate;button.transition=Selectable.Transition.None;button.onClick.AddListener(()=>{CoveSound.Play();click?.Invoke();});
   bool hasIcon=symbol!=WelcomeSymbol.None;
   if(hasIcon){var icon=Rect(plate.transform,name+" symbol",38,(h-48)/2,48,48).gameObject.AddComponent<WelcomeIcon>();icon.symbol=symbol;icon.color=symbol==WelcomeSymbol.Huawei?LocalProfile.Hex("D82630"):Ink;icon.raycastTarget=false;}
   Label(plate.transform,label,hasIcon?100:10,1,w-(hasIcon?116:20),h-4,size,true,symbol==WelcomeSymbol.Huawei?LocalProfile.Hex("593300"):Ink,TextAnchor.MiddleCenter);
   return button;
  }
 }

 public sealed class WelcomeButton:Button {
  protected override void DoStateTransition(SelectionState state,bool instant){
   if(targetGraphic is WelcomePlate p){p.pressed=state==SelectionState.Pressed;p.focused=state==SelectionState.Selected;p.dimmed=state==SelectionState.Disabled;p.SetVerticesDirty();}
   if(targetGraphic is WelcomeArtworkFeedback art){art.pressed=state==SelectionState.Pressed;art.focused=state==SelectionState.Selected;art.highlighted=state==SelectionState.Highlighted;art.SetVerticesDirty();}
  }
 }
 // The normal state uses the exact approved raster design. Native feedback
 // supplies pointer press, hover and keyboard focus without redrawing the art.
 public sealed class WelcomeArtworkFeedback:MaskableGraphic {
  public Rect visual;public float radius;public bool pressed,focused,highlighted;
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();if(!pressed&&!focused&&!highlighted)return;
   var r=new Rect(rectTransform.rect.xMin+visual.x,rectTransform.rect.yMax-visual.y-visual.height,visual.width,visual.height);
   if(pressed)CovePlate.Round(v,r,radius,new Color(.015f,.12f,.15f,.17f));
   var tint=focused?LocalProfile.Hex("FFF2AE"):new Color(1,1,1,.8f);float thickness=focused?3:2;
   int start=v.currentVertCount;float rad=Mathf.Min(radius,Mathf.Min(r.width,r.height)/2);
   for(int corner=0;corner<4;corner++){
    var c=new Vector2(corner==0||corner==3?r.xMax-rad:r.xMin+rad,corner<2?r.yMax-rad:r.yMin+rad);
    for(int i=0;i<=16;i++){float a=(corner*90+i*90f/16)*Mathf.Deg2Rad;var direction=new Vector2(Mathf.Cos(a),Mathf.Sin(a));v.AddVert(c+direction*(rad+thickness),tint,Vector2.zero);v.AddVert(c+direction*(rad-1),tint,Vector2.zero);}
   }
   for(int i=0;i<68;i++){int a=start+i*2,b=start+((i+1)%68)*2;v.AddTriangle(a,b,a+1);v.AddTriangle(a+1,b,b+1);}
  }
 }
 // A six-pixel blend removes a hard rectangular seam on the cream sign.
 public sealed class WelcomeSignPatch:MaskableGraphic {
  public Texture texture;public Rect source;public override Texture mainTexture=>texture?texture:base.mainTexture;
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();var r=rectTransform.rect;float[] xs={0,6/r.width,1-6/r.width,1},ys={0,6/r.height,1-6/r.height,1};
   for(int y=0;y<4;y++)for(int x=0;x<4;x++)v.AddVert(new Vector2(r.xMin+xs[x]*r.width,r.yMin+ys[y]*r.height),new Color(1,1,1,x==0||x==3||y==0||y==3?0:1),new Vector2(source.x+xs[x]*source.width,source.y+ys[y]*source.height));
   for(int y=0;y<3;y++)for(int x=0;x<3;x++){int i=y*4+x;v.AddTriangle(i,i+4,i+1);v.AddTriangle(i+1,i+4,i+5);}
  }
 }
 public sealed class WelcomePlate:MaskableGraphic {
  public Color top=Color.white,bottom=Color.white,edge=Color.white;public float radius=24;public bool shadow,pressed,focused,dimmed;public bool sheen=true;
  protected override void OnPopulateMesh(VertexHelper vh){
   vh.Clear();Rect r=rectTransform.rect;if(r.width<=0||r.height<=0)return;
   if(focused){var f=r;f.xMin-=5;f.xMax+=5;f.yMin-=5;f.yMax+=5;Gradient(vh,f,radius+5,LocalProfile.Hex("FFF2AE"),LocalProfile.Hex("FFF2AE"));}
   if(shadow){var sh=r;sh.y-=pressed?2:6;Gradient(vh,sh,radius,new Color(.10f,.18f,.11f,.26f),new Color(.10f,.18f,.11f,.26f));}
   if(pressed)r.y-=3;Gradient(vh,r,radius,edge,edge);
   r.xMin+=2;r.xMax-=2;r.yMin+=3;r.yMax-=2;
   Color high=dimmed?Color.Lerp(top,Color.gray,.35f):top,low=dimmed?Color.Lerp(bottom,Color.gray,.35f):bottom;
   Gradient(vh,r,radius-2,high,low);
   if(sheen&&r.height>40){var shine=r;shine.yMin=shine.yMax-3;shine.xMin+=radius*.65f;shine.xMax-=radius*.65f;CovePlate.Round(vh,shine,1.5f,new Color(1,1,1,.6f));}
  }
  void Gradient(VertexHelper v,Rect r,float rad,Color top,Color bottom){
   if(r.width<=0||r.height<=0)return;rad=Mathf.Clamp(rad,0,Mathf.Min(r.width,r.height)/2);int start=v.currentVertCount;v.AddVert(r.center,Color.Lerp(bottom,top,.5f),Vector2.zero);
   // One display-pixel alpha fringe smooths overlay UI, which does not receive
   // the world camera's MSAA. Keep interiors and the narrow border fully crisp.
   float feather=1/Mathf.Max(.1f,canvas?canvas.scaleFactor:1);const int segments=24,count=4*(segments+1);
   for(int corner=0;corner<4;corner++){var c=new Vector2(corner==0||corner==3?r.xMax-rad:r.xMin+rad,corner<2?r.yMax-rad:r.yMin+rad);for(int i=0;i<=segments;i++){float a=(corner*90+i*90f/segments)*Mathf.Deg2Rad;var dir=new Vector2(Mathf.Cos(a),Mathf.Sin(a));var p=c+dir*rad;var tint=Color.Lerp(bottom,top,Mathf.InverseLerp(r.yMin,r.yMax,p.y));v.AddVert(p,tint,Vector2.zero);tint.a=0;v.AddVert(p+dir*feather,tint,Vector2.zero);}}
   for(int i=0;i<count;i++){int a=start+1+i*2,b=start+1+(i+1)%count*2;v.AddTriangle(start,a,b);v.AddTriangle(a,a+1,b);v.AddTriangle(a+1,b+1,b);}
  }
 }
 public enum WelcomeSymbol {None,Huawei,Person,Globe,Gear,Info}
 public sealed class WelcomeIcon:MaskableGraphic {
  public WelcomeSymbol symbol;
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();
   switch(symbol){
    case WelcomeSymbol.Person:Circle(v,.5f,.72f,.22f,.22f);Circle(v,.5f,.25f,.36f,.26f);break;
    case WelcomeSymbol.Info:Circle(v,.5f,.83f,.08f,.08f);Line(v,.5f,.17f,.5f,.63f,.14f);break;
    case WelcomeSymbol.Globe:Ring(v,.5f,.5f,.45f,.45f,.045f);Ring(v,.5f,.5f,.2f,.45f,.04f);Line(v,.08f,.5f,.92f,.5f,.045f);Line(v,.16f,.3f,.84f,.3f,.04f);Line(v,.16f,.7f,.84f,.7f,.04f);break;
    case WelcomeSymbol.Gear:Ring(v,.5f,.5f,.27f,.27f,.12f);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Line(v,.5f+Mathf.Cos(a)*.25f,.5f+Mathf.Sin(a)*.25f,.5f+Mathf.Cos(a)*.43f,.5f+Mathf.Sin(a)*.43f,.18f);}break;
    case WelcomeSymbol.Huawei:
     for(int i=0;i<8;i++){float a=(14+i*21.7f)*Mathf.Deg2Rad;var root=new Vector2(.5f,.12f);var tip=root+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.83f;Petal(v,root,tip,.075f);}break;
   }
  }
  Vector2 P(float x,float y)=>new Vector2(rectTransform.rect.xMin+x*rectTransform.rect.width,rectTransform.rect.yMin+y*rectTransform.rect.height);
  void Circle(VertexHelper v,float x,float y,float rx,float ry){int s=v.currentVertCount;v.AddVert(P(x,y),color,Vector2.zero);for(int i=0;i<=32;i++){float a=i*Mathf.PI*2/32;v.AddVert(P(x+Mathf.Cos(a)*rx,y+Mathf.Sin(a)*ry),color,Vector2.zero);if(i>0)v.AddTriangle(s,s+i,s+i+1);}}
  void Ring(VertexHelper v,float x,float y,float rx,float ry,float width){for(int i=0;i<48;i++){float a=i*Mathf.PI*2/48,b=(i+1)*Mathf.PI*2/48;Line(v,x+Mathf.Cos(a)*rx,y+Mathf.Sin(a)*ry,x+Mathf.Cos(b)*rx,y+Mathf.Sin(b)*ry,width);}}
  void Line(VertexHelper v,float x,float y,float xx,float yy,float width){var a=P(x,y);var b=P(xx,yy);var n=new Vector2(-(b-a).y,(b-a).x).normalized*width*rectTransform.rect.width/2;int s=v.currentVertCount;foreach(var p in new[]{a-n,a+n,b+n,b-n})v.AddVert(p,color,Vector2.zero);v.AddTriangle(s,s+1,s+2);v.AddTriangle(s,s+2,s+3);}
  void Petal(VertexHelper v,Vector2 root,Vector2 tip,float width){var axis=tip-root;var normal=new Vector2(-axis.y,axis.x).normalized;int s=v.currentVertCount;v.AddVert(P(root.x,root.y),color,Vector2.zero);for(int i=0;i<=24;i++){float t=i/24f;var center=root+axis*Mathf.Sin(t*Mathf.PI/2);var p=center+normal*Mathf.Sin(t*Mathf.PI)*width;v.AddVert(P(p.x,p.y),color,Vector2.zero);}for(int i=24;i>=0;i--){float t=i/24f;var center=root+axis*Mathf.Sin(t*Mathf.PI/2);var p=center-normal*Mathf.Sin(t*Mathf.PI)*width;v.AddVert(P(p.x,p.y),color,Vector2.zero);}for(int i=0;i<49;i++)v.AddTriangle(s,s+i+1,s+i+2);}
 }
}
