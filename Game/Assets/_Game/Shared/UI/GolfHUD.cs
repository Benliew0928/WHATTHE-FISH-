using System.Collections.Generic;

using System.Linq;

using UnityEngine;

using UnityEngine.UI;



namespace WhatTheFish {

 // Presentation only. Existing Golf buttons retain their listeners, visibility,

 // authority-checked input handlers and existing aim/preview rules.

 public sealed class GolfHUD:MonoBehaviour {

  public static GolfHUD Instance {get;private set;}

  sealed class Skin {

   public Button button;public RectTransform visual;public FishingStickerGraphic panel;public CanvasGroup opacity;public Image art,caption;

  }

  RectTransform page,course,hint,moveControl,lookControl,cameraControl,jumpControl,returnControl;

  GolfHUDLayout layout;GolfSwingButton swing;GolfCartButton[] carts;Font fallbackFont;GolfLeaderboardUI scorecard;

  Text station,hintText,countdownBanner;FishingStickerGraphic aimIcon;Skin aimSkin;Image aimCaption,hintCaption;readonly List<(GolfCartButton cart,Skin skin)> cartSkins=new();Sprite cartDrive,cartBuggy,cartRecall,cartFallback,cartExit;
  Image countdownImage;RectTransform countdownAnchor;Sprite[] countdownSprites;Sprite goSprite;
  static readonly Vector2 PowerSize=new(210,350);
  int lastCountdownSec=-1;float countdownAnimTimer,goDisplayTimer;bool wasStarting;

  readonly List<Skin> skins=new();readonly Dictionary<RectTransform,RectTransform> frames=new();
  readonly Dictionary<RectTransform,CanvasGroup> focusGroups=new();
  public float AuxiliaryOpacity {get;private set;}=1;
  public bool AimFocused {get;private set;}

  // Separate parent groups preserve each control's own availability and tint.
  void FocusGroup(RectTransform root){
   if(!root)return;
   if(!focusGroups.TryGetValue(root,out var group))focusGroups[root]=group=root.gameObject.AddComponent<CanvasGroup>();
   group.alpha=AuxiliaryOpacity;group.interactable=group.blocksRaycasts=!AimFocused;
  }
  void FocusWidget(RectTransform widget){if(frames.TryGetValue(widget,out var frame))FocusGroup(frame);}
  void UpdateFocus(bool aiming){
   AimFocused=aiming;
   float duration=layout.aimFocusFadeSeconds;
   AuxiliaryOpacity=Mathf.MoveTowards(AuxiliaryOpacity,aiming?0:1,duration<=0||MenuPreferences.ReducedMotion?1:Time.unscaledDeltaTime/duration);
   FocusWidget(course);FocusWidget(moveControl);FocusWidget(jumpControl);FocusWidget(returnControl);FocusWidget((RectTransform)swing.startButton.transform);
   foreach(var cart in carts)if(frames.TryGetValue((RectTransform)cart.transform,out var frame))FocusGroup(frame);
   if(scorecard)FocusGroup(scorecard.BoardRect);
  }

  public RectTransform ActionRect=>swing?(RectTransform)swing.button.transform:null;

  public RectTransform CourseRect=>course;

  public RectTransform FrameRect=>page;

  public GolfHUDLayout LayoutConfig=>layout;

  public Button CameraButton=>cameraControl?cameraControl.GetComponent<Button>():null;

  public Button JumpButton=>jumpControl?jumpControl.GetComponent<Button>():null;
  readonly Vector3[] captionCorners=new Vector3[4],controlCorners=new Vector3[4];
  public bool CaptionObscured(RectTransform caption){
   caption.GetWorldCorners(captionCorners);var bounds=new UnityEngine.Rect(captionCorners[0].x,captionCorners[0].y,captionCorners[2].x-captionCorners[0].x,captionCorners[2].y-captionCorners[0].y);
   foreach(var pair in frames){if(!Visible(pair.Key))continue;pair.Value.GetWorldCorners(controlCorners);if(bounds.Overlaps(new UnityEngine.Rect(controlCorners[0].x,controlCorners[0].y,controlCorners[2].x-controlCorners[0].x,controlCorners[2].y-controlCorners[0].y)))return true;}return false;
  }

  public static void Create(RectTransform parent,Font font,RectTransform movement,RectTransform look,RectTransform camera,RectTransform jump,RectTransform back,GolfSwingButton swing,GolfCartButton[] cartControls){

   var frame=GolfHUDStyle.Rect("Golf sticker HUD frame",parent,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);frame.anchorMin=Vector2.zero;frame.anchorMax=Vector2.one;frame.offsetMin=frame.offsetMax=Vector2.zero;

   var hud=frame.gameObject.AddComponent<GolfHUD>();Instance=hud;hud.page=frame;hud.fallbackFont=font;hud.layout=Resources.Load<GolfHUDLayout>("GolfUILayout");if(!hud.layout)hud.layout=ScriptableObject.CreateInstance<GolfHUDLayout>();

   hud.moveControl=movement;hud.lookControl=look;hud.cameraControl=camera;hud.jumpControl=jump;hud.returnControl=back;hud.swing=swing;hud.carts=cartControls??new GolfCartButton[0];

   foreach(var control in new[]{movement,look,camera,jump,back,(RectTransform)swing.button.transform,(RectTransform)swing.aimButton.transform,(RectTransform)swing.startButton.transform})control.SetParent(frame,false);

   foreach(var cart in hud.carts)cart.transform.SetParent(frame,false);

   look.SetAsFirstSibling();

   back.gameObject.SetActive(false);camera.gameObject.SetActive(false);camera.GetComponent<Button>().onClick.RemoveAllListeners();camera.name="Golf camera button";jump.name="Golf jump button";hud.Build();hud.Layout();

  }

  Text Label(string name,Transform parent,string value,Vector2 size,int points,Color? color=null){var text=GolfHUDStyle.Text(name,parent,value,size,points,TextAnchor.MiddleCenter,color);if(!text.font)text.font=fallbackFont;return text;}

  static void Stretch(RectTransform rect){rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;}

  Skin SkinButton(Button button,Color color,bool solid=false,string reference="Utility"){
   if(!button.GetComponent<GolfFishingButtonSound>())button.gameObject.AddComponent<GolfFishingButtonSound>();

   var control=(RectTransform)button.transform;var image=button.GetComponent<Image>();if(image){image.enabled=true;image.color=Color.clear;image.raycastTarget=true;}

   // Animate the contents, never the touch target or responsive frame.

   var visual=GolfHUDStyle.Rect("Golf button visual",control,new Vector2(.5f,.5f),Vector2.zero,control.sizeDelta);Stretch(visual);

   var plateRect=GolfHUDStyle.Rect("Golf button sticker",visual,new Vector2(.5f,.5f),Vector2.zero,control.sizeDelta);Stretch(plateRect);var panel=GolfHUDStyle.Panel(plateRect,color,solid);panel.cornerRadius=layout.utilityCornerRadius;panel.raycastTarget=false;

   foreach(var label in control.GetComponentsInChildren<Text>(true)){

    label.transform.SetParent(visual,false);label.fontStyle=FontStyle.Normal;label.color=GolfHUDStyle.Ink;label.raycastTarget=false;

    if(!label.font||label.font.name!="GolfCartLabels")label.font=CoveUI.DisplayFont??fallbackFont;

    foreach(var shadow in label.GetComponents<Shadow>())shadow.enabled=false;

   }

   panel.enabled=false;var art=GolfReferenceUI.Plate(plateRect,reference,reference=="Utility");
   button.targetGraphic=art;button.transition=Selectable.Transition.None;

   var feedback=button.GetComponent<CoveFeedback>()??button.gameObject.AddComponent<CoveFeedback>();feedback.pressScale=.91f;feedback.releaseBounce=.045f;feedback.UseVisual(visual);

   var skin=new Skin{button=button,visual=visual,panel=panel,art=art,opacity=visual.gameObject.AddComponent<CanvasGroup>()};skin.opacity.interactable=false;skin.opacity.blocksRaycasts=false;skins.Add(skin);return skin;

  }

  static void PlaceLabel(Text text,Vector2 position,Vector2 size,int points){var rect=text.rectTransform;rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=position;rect.sizeDelta=size;text.fontSize=points;text.alignment=TextAnchor.MiddleCenter;text.resizeTextForBestFit=false;}

  FishingStickerGraphic Icon(Transform parent,string name,FishingStickerGraphic.Kind kind,Vector2 position,Vector2 size,Color color){var graphic=GolfHUDStyle.Rect(name,parent,new Vector2(.5f,.5f),position,size).gameObject.AddComponent<FishingStickerGraphic>();graphic.kind=kind;graphic.tint=color;graphic.accent=GolfHUDStyle.Teal;graphic.raycastTarget=false;return graphic;}

  void IconButton(RectTransform control,FishingStickerGraphic.Kind kind,string name,Vector2 iconSize){

   var skin=SkinButton(control.GetComponent<Button>(),GolfHUDStyle.Cream);skin.art.sprite=Resources.Load<Sprite>("GolfUI/Badges/Jump");skin.art.preserveAspect=true;foreach(var label in control.GetComponentsInChildren<Text>(true))label.enabled=false;

  }

  void Build(){

   course=GolfHUDStyle.Rect("Golf course sticker",page,new Vector2(0,1),Vector2.zero,layout.courseSize);GolfReferenceUI.Plate(course,"Course",false);

   var title=Label("Golf course title",course,"ISLAND GREENS",new Vector2(290,48),layout.courseTitleFontSize);title.enabled=false;title.rectTransform.anchoredPosition=layout.courseTitlePosition;title.horizontalOverflow=HorizontalWrapMode.Overflow;

   station=Label("Golf station distance",course,"SKY-SAIL STATION",new Vector2(290,35),layout.courseStationFontSize,LocalProfile.Hex("879491"));station.rectTransform.anchoredPosition=layout.courseStationPosition;station.horizontalOverflow=HorizontalWrapMode.Overflow;

   course.gameObject.SetActive(false);

   swing.BuildSlider();
   cartExit=Resources.Load<Sprite>("GolfUI/Badges/Exit");

   aimSkin=SkinButton(swing.aimButton,GolfHUDStyle.Red,true,"Aim");aimIcon=Icon(aimSkin.visual,"Golf aim or cancel icon",FishingStickerGraphic.Kind.Aim,new Vector2(-layout.aimSize.x*.34f,0),new Vector2(34,34),GolfHUDStyle.Ink);aimIcon.enabled=false;

   PlaceLabel(swing.aimLabel,new Vector2(31,0),new Vector2(layout.aimSize.x-108,layout.aimSize.y-28),32);swing.aimLabel.color=Color.white;swing.aimLabel.supportRichText=true;var aimOutline=swing.aimLabel.gameObject.AddComponent<Outline>();aimOutline.effectColor=LocalProfile.Hex("B34B50");aimOutline.effectDistance=new Vector2(1,-1);
   aimCaption=GolfReferenceUI.Caption(aimSkin.art.rectTransform,"Aim",new Vector2(257,123),new Vector2(104,83),new Vector2(35.5f,-1));aimSkin.caption=aimCaption;

   var start=SkinButton(swing.startButton,GolfHUDStyle.Gold);start.art.sprite=Resources.Load<Sprite>("GolfUI/StartButton/StartGame");start.art.preserveAspect=true;PlaceLabel(swing.startLabel,Vector2.zero,layout.startSize-new Vector2(32,20),30);

   IconButton(jumpControl,FishingStickerGraphic.Kind.Jump,"Golf Jump icon",new Vector2(48,49));

   var back=SkinButton(returnControl.GetComponent<Button>(),GolfHUDStyle.Cream,false,"Leave");var backLabel=returnControl.GetComponentInChildren<Text>(true);PlaceLabel(backLabel,new Vector2(28,0),new Vector2(layout.returnSize.x-92,layout.returnSize.y-20),26);
   backLabel.enabled=false;back.caption=GolfReferenceUI.Caption(back.art.rectTransform,"Leave",new Vector2(274,81),new Vector2(168,37),new Vector2(35,-1));

   // Door artwork is already part of the reference plate.

   cartDrive=Resources.Load<Sprite>("GolfUI/CartButtons/Drive");cartBuggy=Resources.Load<Sprite>("GolfUI/CartButtons/Buggy");cartRecall=Resources.Load<Sprite>("GolfUI/CartButtons/Recall");cartFallback=Resources.Load<Sprite>("GolfUI/Reference/Buggy");
   foreach(var cart in carts){
    var skin=SkinButton(cart.button,GolfHUDStyle.Gold,false,"Buggy");
    PlaceLabel(cart.label,new Vector2(35,0),layout.cartSize-new Vector2(105,20),27);
    cartSkins.Add((cart,skin));
   }

   hint=GolfHUDStyle.Rect("Golf framed control hint",page,new Vector2(.5f,0),Vector2.zero,layout.hintSize);GolfReferenceUI.Plate(hint,"Hint",false);hintCaption=GolfReferenceUI.Caption(hint,"Hint",new Vector2(390,84),new Vector2(282,35),new Vector2(36,-1.5f));

   // The exact attention badge is retained in the reference art.

   hintText=Label("Golf drag and release hint",hint,"Drag down, release to hit",new Vector2(layout.hintSize.x-105,layout.hintSize.y-24),layout.hintFontSize);hintText.rectTransform.anchoredPosition=new Vector2(32,0);
   countdownAnchor=GolfHUDStyle.Rect("Golf countdown root",page,new Vector2(.5f,.5f),new Vector2(0,40),Vector2.zero);
   countdownImage=GolfHUDStyle.Rect("Golf countdown badge",countdownAnchor,new Vector2(.5f,.5f),Vector2.zero,new Vector2(200,200)).gameObject.AddComponent<Image>();
   countdownImage.preserveAspect=true;countdownImage.raycastTarget=false;countdownImage.gameObject.SetActive(false);
   countdownSprites=new Sprite[5];
   for(int i=1;i<=5;i++)countdownSprites[i-1]=Resources.Load<Sprite>("GolfUI/Countdown_"+i);
   goSprite=Resources.Load<Sprite>("GolfUI/Countdown_GO");
   countdownBanner=Label("Golf starting countdown banner",countdownAnchor,"",new Vector2(600,240),200,GolfHUDStyle.Gold);countdownBanner.horizontalOverflow=HorizontalWrapMode.Overflow;countdownBanner.verticalOverflow=VerticalWrapMode.Overflow;countdownBanner.rectTransform.anchoredPosition=Vector2.zero;
   var bannerOutline=countdownBanner.gameObject.AddComponent<Outline>();bannerOutline.effectColor=GolfHUDStyle.Ink;bannerOutline.effectDistance=new Vector2(1.5f,-1.5f);countdownBanner.gameObject.SetActive(false);
   countdownAnchor.gameObject.SetActive(false);

   var movementImage=moveControl.GetComponent<Image>();if(movementImage)movementImage.color=new Color(.44f,.82f,.74f,.28f);

   var pad=moveControl.GetComponent<TouchPad>();if(pad&&pad.knob){var knobImage=pad.knob.GetComponent<Image>();if(knobImage)knobImage.color=GolfHUDStyle.Cream;}

  }

  void PlaceFrame(RectTransform widget,Vector2 anchor,Vector2 pivot,Vector2 position,float scale){

   if(!frames.TryGetValue(widget,out var frame)){frame=GolfHUDStyle.Rect("Golf layout "+widget.name,page,anchor,position,widget.sizeDelta);frames[widget]=frame;widget.SetParent(frame,false);widget.anchorMin=widget.anchorMax=widget.pivot=new Vector2(.5f,.5f);widget.anchoredPosition=Vector2.zero;}

   frame.anchorMin=frame.anchorMax=anchor;frame.pivot=pivot;frame.anchoredPosition=position;frame.sizeDelta=widget.sizeDelta;frame.localScale=Vector3.one*scale;

  }

  void Layout(){

   float width=page.rect.width,height=page.rect.height;if(width<=0||height<=0)return;float m=layout.margin,gap=layout.gap;

   lookControl.anchorMin=layout.lookAnchorMin;lookControl.anchorMax=layout.lookAnchorMax;lookControl.offsetMin=lookControl.offsetMax=Vector2.zero;

   float bottomWidth=layout.joystickSize.x+layout.hintSize.x+layout.returnSize.x+layout.actionSize.x+gap*4;

   float scale=Mathf.Clamp(Mathf.Min(1,(width-m*2)/bottomWidth),.5f,1);

   course.sizeDelta=layout.courseSize;float courseScale=Mathf.Min(1,Mathf.Max(300,width*.5f-230)/layout.courseSize.x);

   PlaceFrame(course,new Vector2(0,1),new Vector2(0,1),new Vector2(m,-m),courseScale);

   ActionRect.sizeDelta=PowerSize;cameraControl.sizeDelta=layout.cameraSize;jumpControl.sizeDelta=layout.jumpSize;returnControl.sizeDelta=layout.returnSize;moveControl.sizeDelta=layout.joystickSize;

   var aim=(RectTransform)swing.aimButton.transform;var start=(RectTransform)swing.startButton.transform;aim.sizeDelta=layout.aimSize;start.sizeDelta=layout.startSize;

   PlaceFrame(ActionRect,new Vector2(1,0),new Vector2(1,0),new Vector2(-m,m+8*scale),scale);

   float side=m+(layout.actionSize.x+2)*scale;

   PlaceFrame(aim,new Vector2(1,0),new Vector2(1,0),new Vector2(-side,m+(layout.returnSize.y+gap)*scale),scale);

   PlaceFrame(returnControl,new Vector2(1,0),new Vector2(1,0),new Vector2(-side,m),scale);

   bool crowded=GolfMatchManager.Instance&&GolfMatchManager.Instance.State.Players.Count>6;

   float utilities=m+(PowerSize.y+gap+20)*scale;
   if(!scorecard)scorecard=page.parent.GetComponentInChildren<GolfLeaderboardUI>();
   if(scorecard&&scorecard.BoardRect&&!scorecard.Expanded){scorecard.BoardRect.GetWorldCorners(controlCorners);float boardBottom=page.InverseTransformPoint(controlCorners[0]).y-page.rect.yMin;crowded|=boardBottom<utilities+(layout.jumpSize.y+gap)*scale;}


   if(crowded){


    PlaceFrame(jumpControl,Vector2.zero,Vector2.zero,new Vector2(m,m+(layout.joystickSize.y+gap)*scale),scale);

   }else{


    PlaceFrame(jumpControl,new Vector2(1,0),new Vector2(.5f,0),new Vector2(-m-PowerSize.x*scale*.5f,utilities),scale);

   }

   PlaceFrame(moveControl,Vector2.zero,new Vector2(0,0),new Vector2(m,m),scale);

   float hintX=Mathf.Min(-16*scale,width*.5f-side-(layout.returnSize.x+gap+layout.hintSize.x*.5f)*scale);
   hint.sizeDelta=layout.hintSize;PlaceFrame(hint,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(hintX,m),scale);

   PlaceFrame(start,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-m),scale);

   for(int i=0;i<carts.Length;i++){var cart=(RectTransform)carts[i].transform;cart.sizeDelta=layout.cartSize;PlaceFrame(cart,new Vector2(1,0),new Vector2(1,0),new Vector2(-side,m+(layout.returnSize.y+gap+layout.aimSize.y+gap+i*(layout.cartSize.y+gap))*scale),scale);}

  }

  void LateUpdate(){

   if(!page)return;Layout();var view=PlayerView.Instance;bool aiming=view&&view.GolfAimRequested;UpdateFocus(aiming);
   swing.aimLabel.text=aiming?"Cancel":"Aim";
   aimCaption.enabled=false;swing.aimLabel.enabled=true;
   swing.startLabel.enabled=false;
   swing.label.enabled=false;
   foreach(var item in cartSkins){
    // These approved images already contain their exact icon and lettering.
    var sprite=item.cart.label.text switch {"Drive"=>cartDrive,"Buggy"=>cartBuggy,"Recall"=>cartRecall,"Exit"=>cartExit,_=>null};
    item.skin.art.sprite=sprite?sprite:cartFallback;item.skin.art.preserveAspect=true;
    item.cart.label.enabled=!sprite;
   }

   // Keep layered plates opaque: per-graphic alpha would composite caption
   // patches twice and expose their rectangles on disabled controls.
   foreach(var skin in skins){skin.opacity.alpha=1;var tint=skin.button.IsInteractable()||skin.button==swing.button?Color.white:new Color(.78f,.82f,.84f,1);skin.art.color=tint;if(skin.caption)skin.caption.color=tint;}

   var app=AppRoot.Instance;var world=SkySailWorld.Instance;

   station.text=app&&app.LocalAthlete?"SKY-SAIL STATION  •  "+Mathf.RoundToInt(Vector3.Distance(app.LocalAthlete.transform.position,SkySailMap.Port(SportId.Golf)))+" m":"SKY-SAIL STATION";

   var match=GolfMatchManager.Instance;hintText.text=world&&world.Travelling?"Look around during the journey":match&&match.Starting?$"MATCH STARTS IN {Mathf.CeilToInt((float)match.StartRemaining)}s":aiming?"Drag down, release to hit":view&&GolfCartWorld.Driving(view.target)?"Left stick: drive & steer":"Walk near your ball to aim";
   if(aiming&&view.GolfCharging)hintText.text=$"Power {Mathf.RoundToInt(view.GolfDisplayedCharge*100)}%  •  Release";
   if(match&&app&&match.WaitingForTee(app.LocalAthlete))hintText.text="Waiting for a free tee";

   hintText.fontSize=aiming?layout.hintFontSize:20;
   hintCaption.enabled=hintText.text=="Walk near your ball to aim";hintText.enabled=!hintCaption.enabled;
   hint.gameObject.SetActive(!hintCaption.enabled);
   if(countdownAnchor){
    if(match&&match.Starting){
     wasStarting=true;
     int sec=Mathf.CeilToInt((float)match.StartRemaining);
     if(sec!=lastCountdownSec){
      lastCountdownSec=sec;
      countdownAnimTimer=0;
      if(sec>=1&&sec<=5&&countdownSprites!=null&&countdownSprites[sec-1]){
       countdownImage.sprite=countdownSprites[sec-1];
       countdownImage.rectTransform.sizeDelta=new Vector2(200,200);
       countdownImage.gameObject.SetActive(true);
       countdownBanner.gameObject.SetActive(false);
      }else{
       countdownImage.gameObject.SetActive(false);
       countdownBanner.text=sec.ToString();
       countdownBanner.gameObject.SetActive(true);
      }
      if(!countdownAnchor.gameObject.activeSelf)countdownAnchor.gameObject.SetActive(true);
     }
     countdownAnimTimer+=Time.unscaledDeltaTime;
     float t=Mathf.Clamp01(countdownAnimTimer/.6f);
     float ease=1f-Mathf.Pow(1f-t,3f);
     countdownAnchor.localScale=Vector3.one*Mathf.Lerp(3f,1f,ease);
    }else if(wasStarting){
     wasStarting=false;
     lastCountdownSec=-1;
     goDisplayTimer=1f;
     countdownAnimTimer=0;
     if(goSprite){
      countdownImage.sprite=goSprite;
      countdownImage.rectTransform.sizeDelta=new Vector2(316,200);
      countdownImage.gameObject.SetActive(true);
      countdownBanner.gameObject.SetActive(false);
     }else{
      countdownImage.gameObject.SetActive(false);
      countdownBanner.text="GO!";
      countdownBanner.gameObject.SetActive(true);
     }
     if(!countdownAnchor.gameObject.activeSelf)countdownAnchor.gameObject.SetActive(true);
     countdownAnchor.localScale=Vector3.one*3f;
    }else if(goDisplayTimer>0){
     goDisplayTimer-=Time.unscaledDeltaTime;
     countdownAnimTimer+=Time.unscaledDeltaTime;
     float t=Mathf.Clamp01(countdownAnimTimer/.6f);
     float ease=1f-Mathf.Pow(1f-t,3f);
     countdownAnchor.localScale=Vector3.one*Mathf.Lerp(3f,1f,ease);
     if(goDisplayTimer<=0){
      countdownAnchor.gameObject.SetActive(false);
      countdownAnchor.localScale=Vector3.one;
     }
    }else if(countdownAnchor.gameObject.activeSelf){
     countdownAnchor.gameObject.SetActive(false);
     countdownAnchor.localScale=Vector3.one;
    }
   }

  }

  bool Visible(RectTransform widget){if(!widget.gameObject.activeInHierarchy)return false;var visibility=widget.GetComponent<CanvasGroup>();return !visibility||visibility.alpha>.01f;}

  UnityEngine.Rect FrameBounds(RectTransform widget){var corners=new Vector3[4];frames[widget].GetWorldCorners(corners);var min=page.InverseTransformPoint(corners[0]);var max=page.InverseTransformPoint(corners[2]);return UnityEngine.Rect.MinMaxRect(min.x,min.y,max.x,max.y);}

  public bool FitsSafeFrame(){

   if(!page)return false;var bounds=page.rect;foreach(var pair in frames){if(!Visible(pair.Key))continue;var frame=FrameBounds(pair.Key);if(frame.xMin<bounds.xMin-1||frame.xMax>bounds.xMax+1||frame.yMin<bounds.yMin-1||frame.yMax>bounds.yMax+1)return false;}return true;

  }

  public bool RegionsSeparate(){var widgets=frames.Keys.Where(Visible).ToArray();for(int i=0;i<widgets.Length;i++)for(int j=i+1;j<widgets.Length;j++)if(FrameBounds(widgets[i]).Overlaps(FrameBounds(widgets[j])))return false;return true;}

#if UNITY_EDITOR || DEVELOPMENT_BUILD

  public void ReviewFrameInsets(Vector2 min,Vector2 max){page.parent.GetComponent<RectTransform>().offsetMin=min;page.parent.GetComponent<RectTransform>().offsetMax=-max;}

  public bool TypographyClean()=>page.GetComponentsInChildren<Text>(true).All(t=>t.fontStyle==FontStyle.Normal&&t.GetComponents<Shadow>().All(s=>!s.enabled||s is Outline&&Mathf.Abs(s.effectDistance.x)<=1.5f&&Mathf.Abs(s.effectDistance.y)<=1.5f));

#endif

  void OnDestroy(){if(Instance==this)Instance=null;}

 }

}
