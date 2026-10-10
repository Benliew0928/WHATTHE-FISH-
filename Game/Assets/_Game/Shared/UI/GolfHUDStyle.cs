using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Shared Golf presentation; the match and input components remain authoritative.
 public static class GolfHUDStyle {
  public static readonly Color Ink=LocalProfile.Hex("143B50"),Cream=LocalProfile.Hex("FFF3D7"),Teal=LocalProfile.Hex("4CDED6"),Gold=LocalProfile.Hex("FFCB53"),LocalBlue=LocalProfile.Hex("69C8F4"),Red=LocalProfile.Hex("F7656A");
  public static RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size){
   var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);rect.anchorMin=rect.anchorMax=anchor;rect.anchoredPosition=position;rect.sizeDelta=size;return rect;
  }
  public static Text Text(string name,Transform parent,string content,Vector2 size,int points,TextAnchor alignment=TextAnchor.MiddleCenter,Color? color=null){
   var text=Rect(name,parent,new Vector2(.5f,.5f),Vector2.zero,size).gameObject.AddComponent<Text>();
   text.font=points<=23?CoveUI.TextFont??CoveUI.DisplayFont:CoveUI.DisplayFont??CoveUI.TextFont;text.fontSize=points;text.fontStyle=FontStyle.Normal;text.text=content;text.color=color??Ink;text.alignment=alignment;text.raycastTarget=false;text.supportRichText=false;text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;return text;
  }
  public static FishingStickerGraphic Panel(RectTransform rect,Color color,bool solid=false){
   var graphic=rect.gameObject.AddComponent<FishingStickerGraphic>();graphic.kind=FishingStickerGraphic.Kind.Panel;graphic.tint=color;graphic.raycastTarget=false;graphic.Decorate();if(solid)graphic.UseSolidPanel();return graphic;
  }
  public static Image Art(Transform parent,string name,string resource,Vector2 size,Vector2 position){
   var image=Rect(name,parent,new Vector2(.5f,.5f),position,size).gameObject.AddComponent<Image>();image.sprite=Resources.Load<Sprite>(resource);image.preserveAspect=true;image.raycastTarget=false;image.enabled=image.sprite;return image;
  }
 }
}
