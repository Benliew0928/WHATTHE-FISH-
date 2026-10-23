using System;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Quiet, scene-facing surfaces. No blur pass, new textures or opaque menu backdrop.
 public static class WorldUI {
  public static readonly Color Ink=CoveUI.Hex("F8F5EB"),Muted=CoveUI.Hex("C4CDC8"),
   Surface=new Color(.075f,.105f,.10f,.95f),Raised=new Color(.17f,.21f,.19f,.96f),
   Line=new Color(.72f,.80f,.74f,.27f);
  public static Color Accent(SportId sport)=>sport switch {SportId.Football=>CoveUI.Coral,SportId.Basketball=>CoveUI.Gold,SportId.Golf=>CoveUI.Mint,_=>CoveUI.Sky};
  public static string HostName=>NetworkAthlete.HostPlayer?NetworkAthlete.HostPlayer.Nickname.Value.ToString():"Your host";
  public static Text Label(Transform parent,string value,float x,float y,float w,float h,int size=24,bool bold=false,Color? color=null,TextAnchor align=TextAnchor.MiddleLeft)=>PlayerHubStyle.Label(parent,value,x,y,w,h,size,bold,color??Ink,align);
  public static CovePlate Card(Transform parent,string name,float x,float y,float w,float h){var p=CoveUI.Plate(parent,name,x,y,w,h,Surface,18,1.5f,false);p.borderColor=Line;return p;}
  public static CoveButton Button(Transform parent,string name,string label,float x,float y,float w,float h,Action action,Color? accent=null,int size=24){
   var b=PlayerHubStyle.Action(parent,name,label,x,y,w,h,accent??Raised,action,size:size);
   var f=b.GetComponent<CoveFeedback>();f.activeBorder=Line;f.disabledBorder=Line;f.disabledTint=Raised;f.pressedTint=Surface;
   foreach(var t in b.GetComponentsInChildren<Text>())t.color=accent.HasValue?CoveUI.Hex("152C27"):Ink;return b;
  }
  public static Canvas Canvas(GameObject owner,int order){var c=owner.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=order;var s=owner.GetComponent<CanvasScaler>();s.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;s.referenceResolution=new Vector2(1600,900);s.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;return c;}
  // Screen-safe placement for independent canvases; anchored at centre or bottom centre.
  public static void Fit(RectTransform panel,RectTransform root,float width,float height,bool bottom=false,float inset=24){
   var safe=Screen.safeArea;float w=safe.width/Screen.width*root.rect.width,h=safe.height/Screen.height*root.rect.height;
   panel.localScale=Vector3.one*Mathf.Min(1,(w-32)/width,(h-32)/height);
   panel.anchorMin=panel.anchorMax=panel.pivot=bottom?new Vector2(.5f,0):Vector2.one*.5f;
   panel.anchoredPosition=new Vector2((safe.center.x/Screen.width-.5f)*root.rect.width,bottom?safe.y/Screen.height*root.rect.height+inset:(safe.center.y/Screen.height-.5f)*root.rect.height);
  }
 }
}
