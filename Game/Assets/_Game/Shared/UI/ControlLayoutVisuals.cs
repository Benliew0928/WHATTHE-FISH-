using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Copy presentation only. Never instantiate a live control's input scripts,
 // listeners, singleton owners or animations in the layout editor.
 public static class ControlLayoutVisuals {
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
    if(sourceGraphic is Text t&&graphic is Text u){
     u.font=t.font;
     // JsonUtility copies Text's rendered value, but not LocalizedText's source
     // property. Restore that source before a hidden control is reactivated.
     if(t is LocalizedText from&&u is LocalizedText to){to.UserContent=from.UserContent;to.Artwork=from.Artwork;to.Basketball=from.Basketball;to.ManualFit=from.ManualFit;to.text=from.Source;}
     else u.text=t.text;
     u.fontSize=t.fontSize;u.fontStyle=t.fontStyle;u.alignment=t.alignment;u.lineSpacing=t.lineSpacing;u.supportRichText=t.supportRichText;u.resizeTextForBestFit=t.resizeTextForBestFit;u.resizeTextMinSize=t.resizeTextMinSize;u.resizeTextMaxSize=t.resizeTextMaxSize;u.horizontalOverflow=t.horizontalOverflow;u.verticalOverflow=t.verticalOverflow;
    }
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
    foreach(var image in copy.Root.GetComponentsInChildren<Image>(true))if(!image.sprite&&(image.name=="Action illustration"||image.name=="Prop illustration"||image.name=="Skill illustration")){image.sprite=sport==SportId.Basketball?ControlArt.Prop(SportsProp.Portal):sport==SportId.Football?ControlArt.Prop(SportsProp.Wall):ControlArt.Skill(FishingAbility.Sonar);image.color=Color.white;}
   }
   if(sport==SportId.Fishing&&id=="left"){
    foreach(var sticker in copy.Root.GetComponentsInChildren<FishingStickerGraphic>(true))if(sticker.kind==FishingStickerGraphic.Kind.Panel&&!sticker.Illustration)sticker.UseIllustration("SteerIdle");
   }
  }

  // Menu controls are passive instances of the exact gameplay design factory.
  public static void Preview(RectTransform parent,SportId sport,string id){
   var native=ControlDesigns.Size(sport,id);
   var root=GolfHUDStyle.Rect("Button artwork",parent,Vector2.one*.5f,Vector2.zero,native);
   ControlDesigns.Build(root,sport,id);
   root.localScale=new Vector3(parent.rect.width/native.x,parent.rect.height/native.y,1);
  }
 }
}
