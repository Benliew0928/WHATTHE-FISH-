using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public static partial class ControlDesigns {
  static readonly Color ButtonInk=CoveUI.Hex("073365");
  static Image Picture(Transform parent,string name,string sprite,Vector2 size,Vector2 position=default){
   var image=Center(parent,name,size,position).gameObject.AddComponent<Image>();
   image.sprite=ControlArt.Get(sprite);image.preserveAspect=true;image.raycastTarget=false;return image;
  }
  static Text GlossLabel(Transform parent,string value,Vector2 size,Vector2 position,float points){
   var text=Center(parent,"Control caption",size*2,position).gameObject.AddComponent<LocalizedText>();
   text.rectTransform.localScale=Vector3.one*.5f;text.font=CoveUI.DisplayFont;text.fontSize=Mathf.RoundToInt(points*2);
   text.fontStyle=FontStyle.Normal;text.color=Color.white;text.alignment=TextAnchor.MiddleCenter;
   text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;
   text.resizeTextForBestFit=true;text.resizeTextMinSize=Mathf.RoundToInt(points*1.5f);text.resizeTextMaxSize=text.fontSize;
   text.raycastTarget=false;text.supportRichText=false;text.text=value;
   var edge=text.gameObject.AddComponent<Outline>();edge.effectColor=ButtonInk;edge.effectDistance=new(2,-2);
   return text;
  }
  static void Shell(View v,string style){
   v.Glossy=true;v.SkinStyle=style;
   v.Visual=Center(v.Root,"Shared button visual",v.Root.sizeDelta);CoveUI.Stretch(v.Visual);v.PressVisual=v.Visual;
   var image=Picture(v.Visual,"Glossy "+style,style,v.Root.sizeDelta);CoveUI.Stretch(image.rectTransform);
   image.preserveAspect=false;v.Face=image;
  }
  static void ActionArt(View v,string icon,string label,Vector2 artSize,Vector2 artPosition,float points,Vector2 labelPosition,float labelHeight=26){
   v.Art=Picture(v.Visual,"Action illustration",icon,artSize,artPosition);
   v.Label=GlossLabel(v.Visual,label,new(v.Root.rect.width*.87f,labelHeight),labelPosition,points);
  }
  static Text Hint(View v,string value,Vector2 position,float width,float points=8){
   var label=GlossLabel(v.Visual,value,new(width,14),position,points);label.name="Action hint";return label;
  }
  static void SharedStick(View v,SportId sport){
   v.Glossy=true;v.SkinStyle="StickRing";var root=v.Root;float size=Mathf.Min(root.rect.width,root.rect.height);
   var face=root.GetComponent<Image>()??root.gameObject.AddComponent<Image>();face.enabled=true;
   face.sprite=circle?circle:circle=AppRoot.MakeCircle();face.color=new Color(.02f,.12f,.22f,.13f);face.raycastTarget=false;v.Face=face;
   var ring=Picture(root,"Glossy joystick rim","StickRing",Vector2.one*size);ring.color=new Color(1,1,1,.82f);
   for(int i=0;i<4;i++){
    float a=i*Mathf.PI*.5f;var mark=Picture(root,"Direction "+i,"Direction",new(size*.09f,size*.065f),new(-Mathf.Sin(a)*size*.36f,Mathf.Cos(a)*size*.36f));
    mark.rectTransform.localRotation=Quaternion.Euler(0,0,i*90);mark.color=sport==SportId.Football?new Color(.72f,1,.4f,.9f):new Color(1,1,1,.9f);
   }
   float thumb=sport==SportId.Golf||sport==SportId.Fishing?82:49;
   v.Thumb=Picture(root,"Joystick thumb","StickThumb",Vector2.one*thumb).rectTransform;
  }
  public static void CourtState(Button button,bool defending=false,bool ready=false){
   if(!button)return;var source=button.GetComponent<ControlDesignSource>();if(!source)return;var v=source.View;
   if(!v.Glossy)return;
   string style=source.Id=="shoot"?(defending?(source.Sport==SportId.Basketball?"RoundCyan":"TileCyan"):v.SkinStyle):v.SkinStyle;
   if(v.Face is Image image)image.sprite=ControlArt.Get(style);
   Availability(button,button.interactable);
   if(ready&&v.Face)v.Face.color=new Color(.8f,1f,.78f,1);
  }
  public static void Availability(Button button,bool available,bool active=false){
   if(!button)return;var source=button.GetComponentInParent<ControlDesignSource>();if(!source||!source.View.Glossy)return;
   var v=source.View;v.Face.color=available||active?Color.white:new Color(.62f,.69f,.77f,.9f);
   if(v.Art)v.Art.color=available||active?Color.white:new Color(.78f,.83f,.88f,.85f);
   if(v.Label)v.Label.color=available||active?Color.white:new Color(.82f,.87f,.92f,1);
   if(v.IconRoot)foreach(var icon in v.IconRoot.GetComponentsInChildren<Image>())icon.color=available||active?Color.white:new Color(.65f,.73f,.8f,.85f);
  }
  public static View CancelDesign(RectTransform root){
   var v=new View{Root=root};Shell(v,"Cancel");float h=root.rect.height;
   v.Art=Picture(v.Visual,"Cancel X","Close",Vector2.one*h*.48f,new(-root.rect.width*.33f,0));
   v.Label=GlossLabel(v.Visual,"CANCEL",new(root.rect.width*.63f,h*.65f),new(root.rect.width*.1f,0),h*.32f);return v;
  }
  public static void SkinWallCancel(Button button){
   var menu=button.GetComponent<MenuButtonState>();if(menu)menu.enabled=false;var root=(RectTransform)button.transform;var old=button.GetComponent<BasketballPlate>();if(old){old.enabled=false;old.raycastTarget=false;}
   foreach(var label in button.GetComponentsInChildren<Text>())label.gameObject.SetActive(false);
   var v=CancelDesign(root);button.targetGraphic=v.Face;v.Face.raycastTarget=true;
  }
 }
}
