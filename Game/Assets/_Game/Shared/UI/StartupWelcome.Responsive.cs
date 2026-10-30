using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed partial class StartupWelcome {
  Rect appliedWelcomeArea;Vector2 appliedWelcomeScreen;bool welcomeLayoutApplied;
  public RectTransform Mascot=>mascot;
  public RectTransform Brand=>brand;
  public RectTransform SignCard=>signCard;
  void Create(){
   Canvas=new GameObject("Welcome canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).GetComponent<Canvas>();
   Canvas.transform.SetParent(transform,false);Canvas.renderMode=RenderMode.ScreenSpaceOverlay;Canvas.sortingOrder=30000;
   Canvas.GetComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
   sceneTexture=Resources.Load<Texture2D>("StartupWelcome/Scene");var character=Resources.Load<Texture2D>("StartupWelcome/Mascot");
   var logo=Resources.Load<Sprite>("Menu/WhatTheFishLogo");ArtworkLoaded=sceneTexture&&character&&logo;
   // Full-resolution scenery is the only layer that crops. Characters, branding
   // and controls are independent safe-area elements, never stretched or blurred.
   cover=Rect(Canvas.transform,"Safe frame backdrop",0,0,Width,Height);
   var background=cover.gameObject.AddComponent<RawImage>();background.texture=sceneTexture;background.raycastTarget=true;
   Design=Rect(Canvas.transform,"Adaptive welcome composition",0,0,Width,Height);
   mascot=Rect(Design,"Welcome mascot",80,130,500,750);var portrait=mascot.gameObject.AddComponent<RawImage>();portrait.texture=character;portrait.raycastTarget=false;
   brand=CoveUI.Image(Design,"Official welcome logo",logo,1100,150,520,260).rectTransform;
   utilityRow=Rect(Design,"Welcome utilities",1150,24,452,140);
   LanguageButton=Utility("English",0,WelcomeSymbol.Globe,OpenLanguagePicker);
   SettingsButton=Utility("Settings",156,WelcomeSymbol.Gear,OpenWelcomeSettings);
   InfoButton=Utility("Info",312,WelcomeSymbol.Info,OpenWelcomeInfo);
   signCard=Plate(Design,"Welcome sign",1080,433,560,460,"259C99","0C6C73","105B65",42,true).rectTransform;
   Plate(signCard,"Cream sign interior",8,8,544,444,"FFF8E6","F6EBCF","8ABCAE",36,false);
   readyTitle=Label(signCard,"Ready to play",32,38,496,70,38,true,Ink,TextAnchor.MiddleCenter);
   blankSign=Rect(signCard,"Blank sign during preparation",0,0,560,460).gameObject;
   heading=Label(blankSign.transform,"Welcome aboard!",32,32,496,86,40,true,Ink,TextAnchor.MiddleCenter);
   subtitle=Label(blankSign.transform,"Your island adventure is getting ready.",36,118,488,82,26,false,Ink,TextAnchor.MiddleCenter);
   LoadingPanel=Rect(blankSign.transform,"Actual content loading",105,208,350,118);
   var loading=LoadingPanel.gameObject.AddComponent<WelcomePlate>();loading.top=Cream;loading.bottom=LocalProfile.Hex("F9ECCE");loading.edge=LocalProfile.Hex("8AAEA0");loading.radius=23;loading.raycastTarget=false;
   status=Label(LoadingPanel,"Preparing your islands...",20,12,312,50,22,true,Ink);
   Plate(LoadingPanel,"Progress track",20,69,258,22,"CAD5D0","E0E4D8","C5CFC7",11,false);
   fill=Plate(LoadingPanel,"Actual load progress",20,69,0,22,"4BD9FC","0CA7E2","178FB7",11,false);
   percentage=Label(LoadingPanel,"0%",283,61,59,38,24,true,Ink,TextAnchor.MiddleCenter);percentage.resizeTextForBestFit=true;percentage.resizeTextMinSize=14;percentage.resizeTextMaxSize=24;
   RetryButton=ActionButton(blankSign.transform,"Retry content loading","Try again",90,338,380,108,"C9FFF0","82DDC4","57A996",()=>{if(HasError){BeginLoading();retry?.Invoke();}},WelcomeSymbol.None,30);
   actions=Rect(signCard,"Sign-in choices",44,140,472,228);actionGroup=actions.gameObject.AddComponent<CanvasGroup>();
   bool google=AndroidAccountSignIn.Supported&&AndroidAccountSignIn.Capabilities.Preferred==AccountProvider.Google;
   HuaweiButton=ActionButton(actions,"Huawei sign-in",google?"Sign in with Google":"Sign in with Huawei",0,0,472,108,google?"FFFFFF":"FFF0A5",google?"FFF7E3":"FFC969",google?"76AA9C":"E3AD4D",()=>{
    if(!AndroidAccountSignIn.Supported)ShowPlaceholder("Account sign-in","Huawei and Google sign-in use the Android phone's account screen.\nGuest play is available here.",HuaweiButton);
    else AccountPanel.Open(()=>{CloseModal();ContinueAsGuest();});
   },google?WelcomeSymbol.None:WelcomeSymbol.Huawei,28);
   GuestButton=ActionButton(actions,"Continue as guest","Continue as guest",0,116,472,108,"C9FFF0","82DDC4","57A996",ContinueAsGuest,WelcomeSymbol.Person,28);
   if(!EventSystem.current)new GameObject("Welcome input",typeof(EventSystem),typeof(StandaloneInputModule));
   BeginLoading();Layout();
  }
  Button Utility(string label,float x,WelcomeSymbol symbol,UnityEngine.Events.UnityAction click){
   var button=Rect(utilityRow,label,x,0,140,140).gameObject.AddComponent<WelcomeButton>();button.transition=Selectable.Transition.None;
   var hit=button.gameObject.AddComponent<Image>();hit.color=Color.clear;button.targetGraphic=hit;
   var circle=Plate(button.transform,"Utility circle",23,0,94,94,"3299B4","126A83","B0E4E6",47,true);
   button.targetGraphic=circle;
   var icon=Rect(circle.transform,"Utility icon",23,23,48,48).gameObject.AddComponent<WelcomeIcon>();icon.symbol=symbol;icon.color=Color.white;icon.raycastTarget=false;
   var caption=Label(button.transform,label,0,99,140,37,24,true,Ink,TextAnchor.MiddleCenter);caption.resizeTextForBestFit=true;caption.resizeTextMinSize=18;caption.resizeTextMaxSize=24;
   // Feedback is native and keeps the circle/label inside one stable hit target.
   button.onClick.AddListener(()=>{CoveSound.Play();click();});return button;
  }
  static void Place(RectTransform r,float x,float y,float w,float h){r.sizeDelta=new Vector2(w,h);r.anchoredPosition=new Vector2(x+r.pivot.x*w,-y-(1-r.pivot.y)*h);}
  public void Layout(){
   SafeArea=ResponsiveScreen.SafeArea;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(ReviewSafeArea.HasValue)SafeArea=ReviewSafeArea.Value;
#endif
   SafeArea=ResponsiveScreen.Centered(SafeArea,new Vector2(Screen.width,Screen.height));
   var screen=new Vector2(Screen.width,Screen.height);
   if(welcomeLayoutApplied&&appliedWelcomeArea==SafeArea&&appliedWelcomeScreen==screen){if(modal)LayoutDialog();return;}
   welcomeLayoutApplied=true;appliedWelcomeArea=SafeArea;appliedWelcomeScreen=screen;
   bool vertical=SafeArea.width<SafeArea.height;
   float scale=vertical?SafeArea.width/900:Mathf.Min(SafeArea.width/1200,SafeArea.height/Height);
   Canvas.GetComponent<CanvasScaler>().scaleFactor=scale;Canvas.scaleFactor=scale;
   float w=SafeArea.width/scale,h=SafeArea.height/scale;
   Design.localScale=Vector3.one;Place(Design,SafeArea.x/scale,(Screen.height-SafeArea.yMax)/scale,w,h);
   CoveUI.Stretch(cover);cover.offsetMin=Vector2.one*(-2/scale);cover.offsetMax=Vector2.one*(2/scale);
   float sourceAspect=sceneTexture.width/(float)sceneTexture.height,targetAspect=(Screen.width+4f)/(Screen.height+4f);
   var uv=targetAspect<sourceAspect?new Rect(0,0,targetAspect/sourceAspect,1):new Rect(0,(1-sourceAspect/targetAspect)/2,1,sourceAspect/targetAspect);
   cover.GetComponent<RawImage>().uvRect=uv;
   float pad=Mathf.Max(48,(w-2200)/2),cardX=vertical?(w-560)/2:w-pad-560,cardY=h-508;
   Place(signCard,cardX,cardY,560,460);
   Place(utilityRow,vertical?(w-452)/2:cardX+54,24,452,140);
   if(!vertical){
    float available=cardX-pad-48,characterH=Mathf.Min(h-181,available*1.5f),characterW=characterH*2/3;
    Place(mascot,pad+(available-characterW)*.5f,h-48-characterH,characterW,characterH);
    Place(brand,cardX,172,560,Mathf.Max(120,cardY-190));
   }else if(w/h<.65f){
    Place(brand,(w-500)/2,174,500,230);
    float characterH=Mathf.Min(cardY-458,(w-96)*1.5f),characterW=characterH*2/3;
    Place(mascot,(w-characterW)/2,cardY-30-characterH,characterW,characterH);
   }else{
    float available=cardY-200,characterH=Mathf.Min(available,(w*.48f-48)*1.5f),characterW=characterH*2/3;
    Place(mascot,48,cardY-30-characterH,characterW,characterH);Place(brand,w*.48f,180,w*.52f-48,Mathf.Max(180,available));
   }
   if(modal)LayoutDialog();
  }
  void LayoutDialog(){
   float canvasScale=Canvas.scaleFactor,fit=Mathf.Min(SafeArea.width/Width,SafeArea.height/Height);
   modal.localScale=Vector3.one*(fit/canvasScale);
   modal.anchoredPosition=new Vector2(SafeArea.center.x-Width*fit/2,-(Screen.height-SafeArea.center.y-Height*fit/2))/canvasScale;
  }
 }
}
