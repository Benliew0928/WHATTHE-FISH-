using System;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // The player hub shares the sports menu's surfaces, typography and interaction treatment.
 public static class PlayerHubStyle {
  public static readonly Color Ink=CoveUI.Hex("F3F8FA"),Muted=CoveUI.Hex("ADC2CE"),
   Panel=CoveUI.Hex("153449"),Raised=CoveUI.Hex("193D52"),Background=CoveUI.Hex("0C2434"),
   Lime=CoveUI.Hex("BBEF6E"),Cyan=CoveUI.Hex("53DED7"),Line=CoveUI.Hex("29495D"),Dark=CoveUI.Hex("102B3B");
  static Font display;
  public static Font Display=>display?display:display=Resources.Load<Font>("SportyMenu/BarlowDisplay");
  public static Text Label(Transform parent,string value,float x,float y,float w,float h,int size=24,bool bold=false,Color? color=null,TextAnchor align=TextAnchor.MiddleLeft){
   var t=CoveUI.Text(parent,value,x,y,w,h,size,bold,color??Ink,align);t.font=bold?Display:CoveUI.TextFont;return t;
  }
  public static CovePlate Card(Transform parent,string id,float x,float y,float w,float h,Color? color=null,float radius=14){
   var p=CoveUI.Plate(parent,id,x,y,w,h,color??Panel,radius,1.5f,false);p.borderColor=Line;return p;
  }
  public static CoveButton Action(Transform parent,string id,string label,float x,float y,float w,float h,Color color,System.Action action,CoveSymbol icon=CoveSymbol.None,int size=26){
   var b=CoveUI.Button(parent,id,label,x,y,w,h,color,action,icon,size);
   var p=b.GetComponent<CovePlate>();p.radius=12;p.border=1.5f;p.shadow=false;p.borderColor=Line;p.canvasRenderer.SetColor(Color.white);
   var f=b.GetComponent<CoveFeedback>();f.activeBorder=Line;f.disabledBorder=Line;f.disabledTint=Background;f.pressedTint=Dark;f.releaseBounce=.012f;
   Color ink=color==Lime||color==Cyan?Dark:Ink;
   foreach(var t in b.GetComponentsInChildren<Text>()){t.font=Display;t.color=ink;}
   foreach(var i in b.GetComponentsInChildren<CoveIcon>())i.color=ink;
   return b;
  }
  public static InputField Entry(Transform parent,string id,string hint,string value,float x,float y,float w,int limit=24){
   var f=CoveUI.Field(parent,id,hint,value,x,y,w,limit);var p=f.GetComponent<CovePlate>();
   p.color=Background;p.radius=12;p.borderColor=Line;f.textComponent.font=CoveUI.TextFont;f.textComponent.color=Ink;
   ((Text)f.placeholder).color=Muted;f.caretColor=Ink;f.selectionColor=new Color(.33f,.87f,.84f,.3f);
   f.GetComponent<CoveFieldFeedback>().enabled=false;f.gameObject.AddComponent<PlayerHubField>();return f;
  }
 }
 public sealed class PlayerHubField:MonoBehaviour {
  InputField field;CovePlate plate;
  void Awake(){field=GetComponent<InputField>();plate=GetComponent<CovePlate>();}
  void LateUpdate(){plate.focus=false;plate.borderColor=field.isFocused?PlayerHubStyle.Cyan:PlayerHubStyle.Line;plate.border=field.isFocused?2.5f:1.5f;plate.SetVerticesDirty();}
 }
}
