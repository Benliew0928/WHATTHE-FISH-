using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Copy presentation only. Never instantiate a live control's input scripts,
 // listeners, singleton owners or animations in the layout editor.
 public static class ControlLayoutVisuals {
  static Sprite previewCircle;
  public sealed class Copy {
   public RectTransform Root;
   public readonly List<(RectTransform source,RectTransform copy)> Rects=new();
   public readonly List<(Graphic source,Graphic copy)> Graphics=new();
   public void Fit(RectTransform source,Vector2 size){
    foreach(var pair in Rects)if(pair.source&&pair.copy&&pair.copy!=Root)Geometry(pair.source,pair.copy);
    Root.anchorMin=Root.anchorMax=Vector2.one*.5f;Root.pivot=source.pivot;Root.sizeDelta=source.rect.size;
    Root.localScale=new Vector3(size.x/Mathf.Max(1,source.rect.width),size.y/Mathf.Max(1,source.rect.height),1);
    Root.anchoredPosition=Vector2.Scale(source.pivot-Vector2.one*.5f,size);
   }
  }
  public static Copy Clone(RectTransform source,RectTransform parent){
   var result=new Copy();result.Root=Node(source,parent,result,true);return result;
  }
  static RectTransform Node(RectTransform source,RectTransform parent,Copy result,bool reveal){
   var rect=new GameObject(source.name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);Geometry(source,rect);result.Rects.Add((source,rect));
   // A contextual button may be inactive; its artwork must still be editable.
   rect.gameObject.SetActive(reveal||source.gameObject.activeSelf);
   foreach(var sourceGraphic in source.GetComponents<Graphic>()){
    var graphic=(Graphic)rect.gameObject.AddComponent(sourceGraphic.GetType());
    JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(sourceGraphic),graphic);
    graphic.color=sourceGraphic.color;graphic.material=sourceGraphic.material;graphic.raycastTarget=false;graphic.enabled=sourceGraphic.enabled;
    if(sourceGraphic is Image a&&graphic is Image b){b.sprite=a.sprite;b.overrideSprite=a.overrideSprite;b.type=a.type;b.preserveAspect=a.preserveAspect;b.fillAmount=a.fillAmount;b.fillCenter=a.fillCenter;b.fillMethod=a.fillMethod;b.fillOrigin=a.fillOrigin;b.fillClockwise=a.fillClockwise;b.pixelsPerUnitMultiplier=a.pixelsPerUnitMultiplier;}
    if(sourceGraphic is Text t&&graphic is Text u){u.font=t.font;u.text=t.text;u.fontSize=t.fontSize;u.fontStyle=t.fontStyle;u.alignment=t.alignment;u.lineSpacing=t.lineSpacing;u.supportRichText=t.supportRichText;u.resizeTextForBestFit=t.resizeTextForBestFit;u.resizeTextMinSize=t.resizeTextMinSize;u.resizeTextMaxSize=t.resizeTextMaxSize;u.horizontalOverflow=t.horizontalOverflow;u.verticalOverflow=t.verticalOverflow;}
    result.Graphics.Add((sourceGraphic,graphic));
   }
   foreach(var effect in source.GetComponents<Shadow>()){
    var copy=(Shadow)rect.gameObject.AddComponent(effect.GetType());copy.effectColor=effect.effectColor;copy.effectDistance=effect.effectDistance;copy.useGraphicAlpha=effect.useGraphicAlpha;copy.enabled=effect.enabled;
   }
   foreach(Transform child in source)if(child is RectTransform r)Node(r,rect,result,reveal&&!source.GetComponent<Graphic>());
   var sticker=source.GetComponent<FishingStickerGraphic>();
   if(sticker)sticker.CopyAppearanceTo(rect.GetComponent<FishingStickerGraphic>());
   return rect;
  }
  static void Geometry(RectTransform source,RectTransform target){
   target.anchorMin=source.anchorMin;target.anchorMax=source.anchorMax;target.pivot=source.pivot;target.sizeDelta=source.sizeDelta;
   target.anchoredPosition3D=source.anchoredPosition3D;target.localRotation=source.localRotation;target.localScale=source.localScale;
  }
  public static void CompleteContext(Copy copy,SportId sport,string id){
   // Empty inventory slots still show their real button, with an example item.
   if(id=="ability"){
    foreach(var image in copy.Root.GetComponentsInChildren<Image>(true))if(!image.sprite){image.sprite=sport==SportId.Basketball?SportsUIStyle.PropIcon(SportsProp.Portal):sport==SportId.Football?FootballPropHUD.Icon(SportsProp.Wall):FishingGearArt.Icon(0);image.color=Color.white;}
   }
   if(sport==SportId.Fishing&&id=="left"){
    foreach(var sticker in copy.Root.GetComponentsInChildren<FishingStickerGraphic>(true))if(sticker.kind==FishingStickerGraphic.Kind.Panel&&!sticker.Illustration)sticker.UseIllustration("SteerIdle");
   }
  }

  // Main-menu samples use the same art, typefaces and native design dimensions
  // as the game's controls, never a replacement card with an icon and caption.
  public static void Preview(RectTransform parent,SportId sport,string id){
   Vector2 native=NativeSize(sport,id);
   var root=Center(parent,"Button artwork",native);root.localScale=new Vector3(parent.rect.width/native.x,parent.rect.height/native.y,1);
   if(sport==SportId.Basketball||sport==SportId.Football)Court(root,sport,id);
   else if(sport==SportId.Golf)Golf(root,id);
   else Fishing(root,id);
  }
  static Vector2 NativeSize(SportId sport,string id){
   if(sport==SportId.Basketball)return id switch{"move"=>new(108,108),"shoot"=>new(106,106),"pass"=>new(80,80),"jump"=>new(64,64),_=>new(80,80)};
   if(sport==SportId.Football)return id switch{"move" or "dash"=>new(108,108),"pass"=>new(66,72),"tackle"=>new(76,79),_=>new(80,80)};
   if(sport==SportId.Golf){var config=Resources.Load<GolfHUDLayout>("GolfUILayout");return id switch{"move"=>config.joystickSize,"jump"=>config.jumpSize,"power"=>new(210,350),"aim"=>config.aimSize,"start"=>config.startSize,_=>config.cartSize};}
   var fishing=Resources.Load<FishingHUDLayout>("FishingUILayout");
   return id switch{"move"=>fishing.joystickSize,"jump"=>new(128,128),"cast"=>fishing.actionSize,"left"=>fishing.fightControlSize,"cancel"=>fishing.cancelSize,_=>new(104,104)};
  }
  static RectTransform Center(Transform parent,string name,Vector2 size,Vector2 at=default)=>GolfHUDStyle.Rect(name,parent,Vector2.one*.5f,at,size);
  static Text Label(Transform parent,string text,Vector2 at,Vector2 size,int points,Color? color=null){
   var t=GolfHUDStyle.Text(text,parent,text,size,points,TextAnchor.MiddleCenter,color??CoveUI.Ink);t.rectTransform.anchoredPosition=at;t.font=points<=23?SportsUIStyle.TextFont:SportsUIStyle.DisplayFont;return t;
  }
  static void Art(Transform parent,string resource,Vector2 size,Vector2 at=default)=>GolfHUDStyle.Art(parent,resource,resource,size,at);
  static BasketballPlate Plate(RectTransform root,Color color,float radius,float border=2.5f){
   var p=root.gameObject.AddComponent<BasketballPlate>();p.color=color;p.radius=Vector2.one*radius;p.border=border;p.inset=3;p.shadow=5;p.raycastTarget=false;return p;
  }
  static void Court(RectTransform root,SportId sport,string id){
   bool football=sport==SportId.Football;
   if(id=="move"){
    if(football)root.gameObject.AddComponent<FootballJoystickFace>().raycastTarget=false;
    else{var p=Plate(root,new Color(1,.976f,.914f,.16f),54,3);p.edge=new Color(1,.976f,.914f,.6f);p.inset=p.shadow=0;}
    var thumb=Plate(Center(root,"Joystick thumb",Vector2.one*49),CoveUI.Cream,24.5f,2);thumb.inset=0;thumb.shadow=3;
    CoveUI.Rect(root,"Run direction",45,6,18,12).gameObject.AddComponent<BasketballJoystickMark>().raycastTarget=false;return;
   }
   if(id=="ability"){
    var p=Plate(root,CoveUI.Ink,27,2);p.edge=CoveUI.Cream;p.inset=2;p.insetColor=CoveUI.Gold;p.shadow=4;
    CoveUI.Image(root,"Prop illustration",football?FootballPropHUD.Icon(SportsProp.Wall):SportsUIStyle.PropIcon(SportsProp.Turbo),football?9:10,football?4:6,football?62:60,football?47:45);
    BasketballUIStyle.Text(root,football?"Wall":"Turbo",football?2:4,football?54:56,football?76:72,20,12,true,TextAnchor.MiddleCenter,CoveUI.Cream);
    var badge=BasketballUIStyle.Plate(root,"One use",58,-5,25,25,CoveUI.Gold,12.5f,2);BasketballUIStyle.Text(badge.transform,"1",0,0,25,25,13,true,TextAnchor.MiddleCenter);return;
   }
   if(!football){
    float size=root.rect.width;var p=Plate(root,id=="shoot"?BasketballUIStyle.Shoot:id=="pass"?CoveUI.Sky:CoveUI.Cream,id=="shoot"?53:id=="pass"?30.4f:24);
    if(id=="shoot")p.inset=4;
    BasketballUIStyle.Art(root,id=="shoot"?BasketballIcon.Shoot:id=="pass"?BasketballIcon.Pass:BasketballIcon.Jump,id=="shoot"?25.5f:id=="pass"?19:16,id=="shoot"?6:id=="pass"?9:3,id=="shoot"?55:id=="pass"?42:32,id=="shoot"?55:id=="pass"?42:32);
    BasketballUIStyle.Text(root,id=="shoot"?"Shoot":id=="pass"?"Pass":"Jump",4,id=="shoot"?56:id=="pass"?48:33,size-8,id=="shoot"?31:id=="pass"?25:28,id=="shoot"?21:id=="pass"?17:13,true,TextAnchor.MiddleCenter);
    if(id=="shoot")BasketballUIStyle.Text(root,"HOLD",6,81,94,17,11,false,TextAnchor.MiddleCenter);return;
   }
   Plate(root,id=="shoot"?BasketballUIStyle.Shoot:id=="dash"?CoveUI.Mint:id=="pass"?CoveUI.Cream:CoveUI.Sky,id=="shoot"?29:id=="dash"?54:id=="pass"?25:27,2);
   if(id=="shoot"){FootballUIStyle.Art(root,FootballIcon.Ball,23,3,34,34);BasketballUIStyle.Text(root,"Shoot",3,36,74,25,17,true,TextAnchor.MiddleCenter);BasketballUIStyle.Text(root,"HOLD",3,61,74,16,8,false,TextAnchor.MiddleCenter);}
   if(id=="pass"){FootballUIStyle.Art(root,FootballIcon.Target,16,2,34,31);BasketballUIStyle.Text(root,"Pass",3,34,60,23,13,true,TextAnchor.MiddleCenter);BasketballUIStyle.Text(root,"HOLD TO AIM",2,54,62,15,8,false,TextAnchor.MiddleCenter);}
   if(id=="tackle"){FootballUIStyle.Art(root,FootballIcon.Tackle,10,3,56,35);BasketballUIStyle.Text(root,"Tackle",3,38,70,24,15,true,TextAnchor.MiddleCenter);BasketballUIStyle.Text(root,"TAP",2,60,72,15,9,false,TextAnchor.MiddleCenter);}
   if(id=="dash"){
    FootballUIStyle.Art(root,FootballIcon.Dash,30,9,48,48);BasketballUIStyle.Text(root,"Dash",4,55,100,30,23,true,TextAnchor.MiddleCenter);
    var bar=BasketballUIStyle.Plate(root,"Stamina",19,89,72,7,CoveUI.Ink,3.5f,0);BasketballUIStyle.Plate(bar.transform,"Stamina fill",1.5f,1.5f,69,4,BasketballUIStyle.Teal,2,0);
    BasketballUIStyle.Text(root,"HOLD / SWIPE ACTIONS",-18,113,144,16,8,true,TextAnchor.MiddleCenter,CoveUI.Cream);
   }
  }
  static void Stick(RectTransform root,bool golf){
   var circle=previewCircle?previewCircle:previewCircle=AppRoot.MakeCircle();var p=root.gameObject.AddComponent<Image>();p.sprite=circle;p.color=golf?new Color(.44f,.82f,.74f,.28f):new Color(1,1,1,.25f);p.raycastTarget=false;
   var knob=Center(root,"Joystick thumb",Vector2.one*82).gameObject.AddComponent<Image>();knob.sprite=circle;knob.color=golf?GolfHUDStyle.Cream:CoveUI.Cream;knob.raycastTarget=false;
  }
  static void Golf(RectTransform root,string id){
   if(id=="move"){Stick(root,true);return;}
   if(id=="power"){
    Art(root,"GolfUI/PowerSlider/Rail",new(210,350));
    for(int i=0;i<5;i++){var arrow=Center(root,"Downward drag hint "+i,new(22,13),new(0,Mathf.Lerp(350*.385f-35,-350*.34f,i/4f))).gameObject.AddComponent<GolfPowerChevron>();arrow.color=new Color(1,1,1,.4f);arrow.raycastTarget=false;}
    Art(root,"GolfUI/PowerSlider/Knob",new(138,70),new(0,350*.385f));return;
   }
   if(id=="aim"){GolfReferenceUI.Plate(root,"Aim",false);var t=Label(root,"Aim",new(31,0),new(root.rect.width-108,root.rect.height-28),32,Color.white);t.font=CoveUI.DisplayFont;var outline=t.gameObject.AddComponent<Outline>();outline.effectColor=CoveUI.Hex("B34B50");outline.effectDistance=new(1,-1);return;}
   string resource=id=="jump"?"GolfUI/Badges/Jump":id=="start"?"GolfUI/StartButton/StartGame":"GolfUI/CartButtons/"+(id=="buggy"?"Buggy":"Drive");
   Art(root,resource,root.rect.size);
  }
  static void Fishing(RectTransform root,string id){
   if(id=="move"){Stick(root,false);return;}
   if(id=="jump"){var native=Center(root,"Sports jump button",Vector2.one*64);native.localScale=Vector3.one*2;Court(native,SportId.Basketball,"jump");return;}
   if(id=="ability"){
   var plate=root.gameObject.AddComponent<BasketballPlate>();plate.color=CoveUI.Cream;plate.radius=Vector2.one*35;plate.border=3;plate.raycastTarget=false;
   FishingSkillGraphic.Icon(Center(root,"Chosen skill icon",Vector2.one*62,new(0,8)),FishingAbility.Sonar);
   var ring=Center(root,"Skill clock border",Vector2.one*98).gameObject.AddComponent<FishingSkillGraphic>();ring.ring=true;ring.raycastTarget=false;
   Label(root,"USE",new(0,-32.5f),new(86,21),12);
   return;}
   var p=root.gameObject.AddComponent<FishingStickerGraphic>();p.kind=FishingStickerGraphic.Kind.Panel;p.tint=CoveUI.Hex("FFF3D7");p.raycastTarget=false;p.UseSolidPanel();
   if(id=="cast"){p.UseIllustration("ReelButton");var t=Label(root,"CAST",new(0,-58),new(164,64),27,Color.white);var outline=t.gameObject.AddComponent<Outline>();outline.effectColor=GolfHUDStyle.Ink;outline.effectDistance=new(1,-1);return;}
   if(id=="left"){p.UseIllustration("SteerIdle");Label(root,"LEFT",new(0,-40),new(126,32),24);return;}
   if(id=="cancel"){
    p.tint=CoveUI.Hex("F7656A");float cross=root.rect.height*.34f;
    Label(root,"CANCEL",new(cross*.5f,0),new(root.rect.width-cross-18,root.rect.height-12),23);
    var mark=Center(root,"Cancel X",Vector2.one*cross,new(-root.rect.width*.5f+cross*.8f,0)).gameObject.AddComponent<FishingStickerGraphic>();mark.kind=FishingStickerGraphic.Kind.Close;mark.tint=GolfHUDStyle.Ink;mark.raycastTarget=false;return;
   }

  }
 }
}
