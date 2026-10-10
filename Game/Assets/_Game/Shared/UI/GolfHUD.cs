using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Presentation only. Existing Golf buttons retain their listeners, visibility,
 // charge timing, keyboard bindings and authority-checked input handlers.
 public sealed class GolfHUD:MonoBehaviour {
  public static GolfHUD Instance {get;private set;}
  sealed class Skin {
   public Button button;public RectTransform visual;public FishingStickerGraphic panel;public CanvasGroup opacity;
  }
  RectTransform page,course,hint,moveControl,lookControl,cameraControl,jumpControl,returnControl;
  GolfHUDLayout layout;GolfSwingButton swing;GolfCartButton[] carts;Font fallbackFont;
  Text station,hintText;FishingStickerGraphic aimIcon;Skin aimSkin;
  readonly List<Skin> skins=new();readonly Dictionary<RectTransform,RectTransform> frames=new();
  public RectTransform ActionRect=>swing?(RectTransform)swing.button.transform:null;
  public RectTransform CourseRect=>course;
  public RectTransform FrameRect=>page;
  public GolfHUDLayout LayoutConfig=>layout;
  public Button CameraButton=>cameraControl?cameraControl.GetComponent<Button>():null;
  public Button JumpButton=>jumpControl?jumpControl.GetComponent<Button>():null;
  public static void Create(RectTransform parent,Font font,RectTransform movement,RectTransform look,RectTransform camera,RectTransform jump,RectTransform back,GolfSwingButton swing,GolfCartButton[] cartControls){
   var frame=GolfHUDStyle.Rect("Golf sticker HUD frame",parent,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);frame.anchorMin=Vector2.zero;frame.anchorMax=Vector2.one;frame.offsetMin=frame.offsetMax=Vector2.zero;
   var hud=frame.gameObject.AddComponent<GolfHUD>();Instance=hud;hud.page=frame;hud.fallbackFont=font;hud.layout=Resources.Load<GolfHUDLayout>("GolfUILayout");if(!hud.layout)hud.layout=ScriptableObject.CreateInstance<GolfHUDLayout>();
   hud.moveControl=movement;hud.lookControl=look;hud.cameraControl=camera;hud.jumpControl=jump;hud.returnControl=back;hud.swing=swing;hud.carts=cartControls??new GolfCartButton[0];
   foreach(var control in new[]{movement,look,camera,jump,back,(RectTransform)swing.button.transform,(RectTransform)swing.aimButton.transform,(RectTransform)swing.startButton.transform})control.SetParent(frame,false);
   foreach(var cart in hud.carts)cart.transform.SetParent(frame,false);
   look.SetAsFirstSibling();
   camera.name="Golf camera button";jump.name="Golf jump button";hud.Build();hud.Layout();
  }
  Text Label(string name,Transform parent,string value,Vector2 size,int points,Color? color=null){var text=GolfHUDStyle.Text(name,parent,value,size,points,TextAnchor.MiddleCenter,color);if(!text.font)text.font=fallbackFont;return text;}
  static void Stretch(RectTransform rect){rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;}
  Skin SkinButton(Button button,Color color,bool solid=false){
   var control=(RectTransform)button.transform;var image=button.GetComponent<Image>();if(image){image.enabled=true;image.color=Color.clear;image.raycastTarget=true;}
   // Animate the contents, never the touch target or responsive frame.
   var visual=GolfHUDStyle.Rect("Golf button visual",control,new Vector2(.5f,.5f),Vector2.zero,control.sizeDelta);Stretch(visual);
   var plateRect=GolfHUDStyle.Rect("Golf button sticker",visual,new Vector2(.5f,.5f),Vector2.zero,control.sizeDelta);Stretch(plateRect);var panel=GolfHUDStyle.Panel(plateRect,color,solid);panel.cornerRadius=layout.utilityCornerRadius;panel.raycastTarget=false;
   foreach(var label in control.GetComponentsInChildren<Text>(true)){
    label.transform.SetParent(visual,false);label.fontStyle=FontStyle.Normal;label.color=GolfHUDStyle.Ink;label.raycastTarget=false;
    if(!label.font||label.font.name!="GolfCartLabels")label.font=CoveUI.DisplayFont??fallbackFont;
    foreach(var shadow in label.GetComponents<Shadow>())shadow.enabled=false;
   }
   button.targetGraphic=panel;button.transition=Selectable.Transition.None;
   var feedback=button.GetComponent<CoveFeedback>()??button.gameObject.AddComponent<CoveFeedback>();feedback.pressScale=.91f;feedback.releaseBounce=.045f;feedback.UseVisual(visual);
   var skin=new Skin{button=button,visual=visual,panel=panel,opacity=visual.gameObject.AddComponent<CanvasGroup>()};skin.opacity.interactable=false;skin.opacity.blocksRaycasts=false;skins.Add(skin);return skin;
  }
  static void PlaceLabel(Text text,Vector2 position,Vector2 size,int points){var rect=text.rectTransform;rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=position;rect.sizeDelta=size;text.fontSize=points;text.alignment=TextAnchor.MiddleCenter;text.resizeTextForBestFit=false;}
  FishingStickerGraphic Icon(Transform parent,string name,FishingStickerGraphic.Kind kind,Vector2 position,Vector2 size,Color color){var graphic=GolfHUDStyle.Rect(name,parent,new Vector2(.5f,.5f),position,size).gameObject.AddComponent<FishingStickerGraphic>();graphic.kind=kind;graphic.tint=color;graphic.accent=GolfHUDStyle.Teal;graphic.raycastTarget=false;return graphic;}
  void IconButton(RectTransform control,FishingStickerGraphic.Kind kind,string name,Vector2 iconSize){
   var skin=SkinButton(control.GetComponent<Button>(),GolfHUDStyle.Cream);foreach(var label in control.GetComponentsInChildren<Text>(true))label.enabled=false;Icon(skin.visual,name,kind,Vector2.zero,iconSize,GolfHUDStyle.Ink);
  }
  void Build(){
   course=GolfHUDStyle.Rect("Golf course sticker",page,new Vector2(0,1),Vector2.zero,layout.courseSize);GolfHUDStyle.Panel(course,GolfHUDStyle.Cream);GolfHUDStyle.Art(course,"Golf course illustration","GolfUI/GolfCourseBadge",layout.courseArtSize,layout.courseArtPosition);
   var title=Label("Golf course title",course,"ISLAND GREENS",new Vector2(290,48),layout.courseTitleFontSize);title.rectTransform.anchoredPosition=layout.courseTitlePosition;title.horizontalOverflow=HorizontalWrapMode.Overflow;
   station=Label("Golf station distance",course,"SKY-SAIL STATION",new Vector2(290,35),layout.courseStationFontSize);station.rectTransform.anchoredPosition=layout.courseStationPosition;station.horizontalOverflow=HorizontalWrapMode.Overflow;
   var action=SkinButton(swing.button,GolfHUDStyle.Gold,true);action.panel.cornerRadius=layout.actionSize.x*.5f;var shotArt=GolfHUDStyle.Art(action.visual,"Golf swing illustration","GolfUI/GolfShotBadge",layout.actionSize,Vector2.zero);if(shotArt.sprite)action.panel.enabled=false;
   swing.label.transform.SetAsLastSibling();PlaceLabel(swing.label,layout.shotLabelPosition,layout.shotLabelSize,layout.shotFontSize);swing.label.color=Color.white;
   var outline=swing.label.GetComponent<Outline>()??swing.label.gameObject.AddComponent<Outline>();outline.enabled=true;outline.effectColor=GolfHUDStyle.Ink;outline.effectDistance=new Vector2(1.2f,-1.2f);
   aimSkin=SkinButton(swing.aimButton,GolfHUDStyle.Teal,true);aimIcon=Icon(aimSkin.visual,"Golf aim or cancel icon",FishingStickerGraphic.Kind.Aim,new Vector2(-layout.aimSize.x*.34f,0),new Vector2(34,34),GolfHUDStyle.Ink);
   PlaceLabel(swing.aimLabel,new Vector2(19,0),new Vector2(layout.aimSize.x-68,layout.aimSize.y-14),layout.controlFontSize);
   var start=SkinButton(swing.startButton,GolfHUDStyle.Gold);PlaceLabel(swing.startLabel,Vector2.zero,layout.startSize-new Vector2(24,10),layout.controlFontSize);
   IconButton(cameraControl,FishingStickerGraphic.Kind.Camera,"Golf Camera icon",new Vector2(55,44));IconButton(jumpControl,FishingStickerGraphic.Kind.Jump,"Golf Jump icon",new Vector2(48,49));
   var back=SkinButton(returnControl.GetComponent<Button>(),GolfHUDStyle.Cream);var backLabel=returnControl.GetComponentInChildren<Text>(true);PlaceLabel(backLabel,new Vector2(16,0),new Vector2(layout.returnSize.x-57,layout.returnSize.y-10),23);
   var exit=GolfHUDStyle.Rect("Golf leave icon",back.visual,new Vector2(.5f,.5f),new Vector2(-layout.returnSize.x*.36f,0),new Vector2(30,30)).gameObject.AddComponent<CoveIcon>();exit.symbol=CoveSymbol.Join;exit.color=GolfHUDStyle.Ink;exit.raycastTarget=false;
   foreach(var cart in carts){var cartFont=cart.label.font;SkinButton(cart.button,cart.summon?GolfHUDStyle.Teal:GolfHUDStyle.Gold);cart.label.font=cartFont;PlaceLabel(cart.label,Vector2.zero,layout.cartSize-new Vector2(16,10),25);}
   hint=GolfHUDStyle.Rect("Golf framed control hint",page,new Vector2(.5f,0),Vector2.zero,layout.hintSize);GolfHUDStyle.Panel(hint,GolfHUDStyle.Cream);
   var dot=GolfHUDStyle.Rect("Golf hint attention circle",hint,new Vector2(.5f,.5f),new Vector2(-layout.hintSize.x*.36f,0),new Vector2(34,34));var circle=dot.gameObject.AddComponent<CovePlate>();circle.color=GolfHUDStyle.Ink;circle.border=0;circle.radius=17;circle.raycastTarget=false;Label("Golf hint !",dot,"!",new Vector2(30,40),22,Color.white);
   hintText=Label("Golf hold and release hint",hint,"Hold, then release",new Vector2(layout.hintSize.x-92,layout.hintSize.y-12),layout.hintFontSize);hintText.rectTransform.anchoredPosition=new Vector2(22,0);
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
   ActionRect.sizeDelta=layout.actionSize;cameraControl.sizeDelta=layout.cameraSize;jumpControl.sizeDelta=layout.jumpSize;returnControl.sizeDelta=layout.returnSize;moveControl.sizeDelta=layout.joystickSize;
   var aim=(RectTransform)swing.aimButton.transform;var start=(RectTransform)swing.startButton.transform;aim.sizeDelta=layout.aimSize;start.sizeDelta=layout.startSize;
   PlaceFrame(ActionRect,new Vector2(1,0),new Vector2(1,0),new Vector2(-m,m),scale);
   float side=m+(layout.actionSize.x+gap)*scale;
   PlaceFrame(aim,new Vector2(1,0),new Vector2(1,0),new Vector2(-side,m+(layout.returnSize.y+gap+24)*scale),scale);
   PlaceFrame(returnControl,new Vector2(1,0),new Vector2(1,0),new Vector2(-side,m),scale);
   bool crowded=GolfMatchManager.Instance&&GolfMatchManager.Instance.State.Players.Count>6;
   float utilities=m+(layout.actionSize.y+gap)*scale;
   if(crowded){
    PlaceFrame(cameraControl,new Vector2(1,0),new Vector2(1,0),new Vector2(-m,utilities),scale);
    PlaceFrame(jumpControl,new Vector2(1,0),new Vector2(1,0),new Vector2(-m-(layout.cameraSize.x+gap)*scale,utilities),scale);
   }else{
    PlaceFrame(cameraControl,new Vector2(1,0),new Vector2(.5f,0),new Vector2(-m-layout.actionSize.x*scale*.5f,utilities),scale);
    PlaceFrame(jumpControl,new Vector2(1,0),new Vector2(.5f,0),new Vector2(-m-layout.actionSize.x*scale*.5f,utilities+(layout.cameraSize.y+gap)*scale),scale);
   }
   PlaceFrame(moveControl,Vector2.zero,new Vector2(0,0),new Vector2(m,m),scale);
   hint.sizeDelta=layout.hintSize;PlaceFrame(hint,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,m),scale);
   PlaceFrame(start,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-m),scale);
   for(int i=0;i<carts.Length;i++){var cart=(RectTransform)carts[i].transform;cart.sizeDelta=layout.cartSize;PlaceFrame(cart,new Vector2(1,0),new Vector2(1,0),new Vector2(-side,m+(layout.returnSize.y+gap+layout.aimSize.y+gap+24+i*(layout.cartSize.y+gap))*scale),scale);}
  }
  void LateUpdate(){
   if(!page)return;Layout();var view=PlayerView.Instance;bool aiming=view&&view.GolfAimRequested;aimSkin.panel.Refresh(1,aiming?GolfHUDStyle.Red:GolfHUDStyle.Teal);aimIcon.kind=aiming?FishingStickerGraphic.Kind.Close:FishingStickerGraphic.Kind.Aim;aimIcon.SetVerticesDirty();if(aiming)swing.aimLabel.text="Cancel";
   foreach(var skin in skins)skin.opacity.alpha=skin.button.IsInteractable()?1:.52f;
   var app=AppRoot.Instance;var world=SkySailWorld.Instance;
   station.text=app&&app.LocalAthlete?"SKY-SAIL STATION  •  "+Mathf.RoundToInt(Vector3.Distance(app.LocalAthlete.transform.position,SkySailMap.Port(SportId.Golf)))+" m":"SKY-SAIL STATION";
   hintText.text=world&&world.Travelling?"Look around during the journey":aiming?"Hold, then release":view&&GolfCartWorld.Driving(view.target)?"Left stick: drive & steer":"Walk near your ball to aim";
   hintText.fontSize=aiming?layout.hintFontSize:20;
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
