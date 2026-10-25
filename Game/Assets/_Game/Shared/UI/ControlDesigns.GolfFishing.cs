using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public static partial class ControlDesigns {
  static readonly System.Collections.Generic.Dictionary<string,Sprite> cartArt=new();
  public static float PowerTop(RectTransform root)=>root.rect.height*.385f;
  public static float PowerBottom(RectTransform root)=>-root.rect.height*.34f;
  static void Golf(View v,string id){
   if(id=="move"){SharedStick(v,SportId.Golf);return;}
   var root=v.Root;
   if(id=="power"){
    if(v.Label)v.Label.enabled=false;
    v.Art=GolfHUDStyle.Art(root,"Golf power rail","GolfUI/PowerSlider/Rail",root.sizeDelta,Vector2.zero);CoveUI.Stretch(v.Art.rectTransform);v.Face=v.Art;
    v.Arrows=new Graphic[5];
    for(int i=0;i<v.Arrows.Length;i++){var r=Center(root,"Downward drag hint "+i,new(22,13),new(0,Mathf.Lerp(PowerTop(root)-35,PowerBottom(root),i/4f)));v.Arrows[i]=r.gameObject.AddComponent<GolfPowerChevron>();v.Arrows[i].color=new Color(1,1,1,.4f);v.Arrows[i].raycastTarget=false;}
    v.Thumb=GolfHUDStyle.Art(root,"Golf ball power handle","GolfUI/PowerSlider/Knob",new(138,70),new(0,PowerTop(root))).rectTransform;return;
   }
   v.Visual=Center(root,"Golf button visual",root.sizeDelta);CoveUI.Stretch(v.Visual);
   string reference=id=="aim"?"Aim":id=="buggy"||id=="drive"?"Buggy":"Utility";
   v.Art=GolfReferenceUI.Plate(v.Visual,reference,reference=="Utility");v.Face=v.Art;
   if(!v.Label)v.Label=GolfHUDStyle.Text("Control caption",v.Visual,"",root.sizeDelta,32);
   else v.Label.transform.SetParent(v.Visual,false);
   var label=v.Label;label.font=CoveUI.DisplayFont;label.fontStyle=FontStyle.Normal;label.color=GolfHUDStyle.Ink;label.raycastTarget=false;label.resizeTextForBestFit=false;label.alignment=TextAnchor.MiddleCenter;
   label.rectTransform.anchorMin=label.rectTransform.anchorMax=label.rectTransform.pivot=Vector2.one*.5f;
   foreach(var shadow in label.GetComponents<Shadow>())shadow.enabled=false;
   label.enabled=id=="aim";label.supportRichText=id=="aim";
   label.rectTransform.anchoredPosition=id=="aim"?new(31,0):id=="start"?Vector2.zero:new(35,0);
   label.rectTransform.sizeDelta=root.sizeDelta-(id=="aim"?new Vector2(108,28):id=="start"?new Vector2(32,20):new Vector2(105,20));
   label.fontSize=id=="aim"?32:id=="start"?30:27;label.text=id=="aim"?"Aim":id=="start"?"Start game":id=="buggy"?"Buggy":id=="drive"?"Drive":"Jump";
   if(id=="aim"){
    label.color=Color.white;var outline=label.GetComponent<Outline>()??label.gameObject.AddComponent<Outline>();outline.enabled=true;outline.effectColor=CoveUI.Hex("B34B50");outline.effectDistance=new(1,-1);
   }else{
    v.Art.sprite=id=="jump"?Resources.Load<Sprite>("GolfUI/Badges/Jump"):id=="start"?Resources.Load<Sprite>("GolfUI/StartButton/StartGame"):GolfCartArt(GameLocalization.Source(label));v.Art.preserveAspect=true;
    if(id!="jump")LocalizedArtwork.Add(v.Art,id=="start"?new Rect(.15f,.16f,.78f,.63f):new Rect(.48f,.26f,.42f,.52f),GameLocalization.Source(label),id=="start"?30:27,id=="start"?CoveUI.Hex("FFF2C9"):CoveUI.Hex("FFE78A"),id=="start"?Color.white:GolfHUDStyle.Ink,label);
   }
  }
  public static Sprite GolfCartArt(string caption){
   if(!cartArt.TryGetValue(caption,out var art))cartArt[caption]=art=Resources.Load<Sprite>(caption=="Exit"?"GolfUI/Badges/Exit":"GolfUI/CartButtons/"+caption)??Resources.Load<Sprite>("GolfUI/Reference/Buggy");return art;
  }
  public static void SkillIcon(RectTransform parent,FishingAbility ability){
   foreach(Transform child in parent){child.gameObject.SetActive(false);Object.Destroy(child.gameObject);}
   FishingSkillGraphic.Icon(Center(parent,"Skill illustration",parent.sizeDelta),ability);
  }
  public static void FishingActionCaption(Text label,bool directional){
   var view=label.GetComponentInParent<ControlDesignSource>().View;
   label.rectTransform.anchoredPosition=directional?FishingLayout.fightLabelPosition:view.LabelPosition;
   label.rectTransform.sizeDelta=directional?FishingLayout.fightLabelSize:view.LabelSize;
   label.color=directional?GolfHUDStyle.Ink:view.LabelColor;
   var outline=label.GetComponent<Outline>();if(outline)outline.enabled=!directional;
  }
  static void Fishing(View v,string id){
   var root=v.Root;
   if(id=="move"){SharedStick(v,SportId.Fishing);return;}
   if(id=="jump"){
    var native=Center(root,"Sports jump button",Size(SportId.Basketball,"jump"));native.localScale=Vector3.one*2;
    var jump=new View{Root=native,Visual=native};Court(jump,SportId.Basketball,"jump");v.Visual=native;v.PressVisual=jump.PressVisual;v.Glossy=true;v.SkinStyle=jump.SkinStyle;v.Face=jump.Face;v.Label=jump.Label;v.Art=jump.Art;return;
   }
   if(id=="ability"){
    Shell(v,"Ability");
    v.Ring=Center(v.Visual,"Skill clock border",Vector2.one*70,new(0,9)).gameObject.AddComponent<FishingSkillGraphic>();v.Ring.ring=true;v.Ring.raycastTarget=false;v.Ring.tint=CoveUI.Hex("35E8F4");
    v.IconRoot=Center(v.Visual,"Chosen skill icon",new(63,57),new(0,10));SkillIcon(v.IconRoot,FishingAbility.Sonar);
    v.Label=GlossLabel(v.Visual,"USE",new(84,26),new(0,-29),20);return;
   }
   if(id=="cancel"){var cancel=CancelDesign(root);v.Glossy=true;v.SkinStyle=cancel.SkinStyle;v.Visual=cancel.Visual;v.PressVisual=cancel.PressVisual;v.Face=cancel.Face;v.Art=cancel.Art;v.Label=cancel.Label;return;}
   var p=root.gameObject.AddComponent<FishingStickerGraphic>();p.kind=FishingStickerGraphic.Kind.Panel;p.tint=id=="cancel"?GolfHUDStyle.Red:id=="left"?GolfHUDStyle.Teal:GolfHUDStyle.Gold;p.raycastTarget=false;p.Decorate();p.UseSolidPanel();v.Face=p;
   if(id=="cast"){
    p.UseIllustration("ReelButton");v.Label=Label(root,"CAST",new(0,-58),new(164,64),27,Color.white);
    var outline=v.Label.gameObject.AddComponent<Outline>();outline.effectColor=GolfHUDStyle.Ink;outline.effectDistance=new(1,-1);
    v.Label.resizeTextForBestFit=true;v.Label.resizeTextMinSize=18;v.Label.resizeTextMaxSize=28;return;
   }
   if(id=="left"){
    p.UseIllustration("SteerIdle");v.Label=Label(root,"LEFT",FishingLayout.fightLabelPosition,FishingLayout.fightLabelSize,24);v.Label.resizeTextForBestFit=true;v.Label.resizeTextMinSize=18;v.Label.resizeTextMaxSize=24;return;
   }

  }
 }
}
