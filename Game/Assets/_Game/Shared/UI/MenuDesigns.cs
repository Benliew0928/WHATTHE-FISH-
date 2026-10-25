using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public enum MenuSurface { None, Button, Card, Dialog, Track, Thumb }

 // Shared presentation for the non-gameplay controls. No listeners, game rules,
 // texture copies or moving hit targets: existing views still own their actions.
 public static class MenuDesigns {
  public static readonly Color Navy=CoveUI.Hex("103451"), Cyan=CoveUI.Hex("35C8DC"), Gold=CoveUI.Hex("FFC85E"), Cream=CoveUI.Hex("FFF8E8");
  public static void Surface(Graphic graphic,MenuSurface kind){
   if(graphic is BasketballPlate b){b.menuSurface=kind;b.Refresh();}
   else if(graphic is FishingStickerGraphic f){f.menuSurface=kind;f.SetVerticesDirty();}
   else if(graphic is CovePlate c){c.menuSurface=kind;c.SetVerticesDirty();}
  }
  public static T Dialog<T>(T graphic) where T:Graphic {Surface(graphic,MenuSurface.Dialog);return graphic;}
  public static Button Apply(Button button,MenuSurface kind=MenuSurface.Button){
   if(button.targetGraphic is Image image){
    var face=BasketballUIStyle.Plate(button.transform,"Shared menu face",0,0,1,1,image.color);CoveUI.Stretch(face.rectTransform);face.transform.SetAsFirstSibling();image.color=Color.clear;image.raycastTarget=true;button.targetGraphic=face;
    foreach(var label in button.GetComponentsInChildren<Text>(true)){label.font=SportsUIStyle.DisplayFont;label.fontStyle=FontStyle.Normal;}
   }
   Surface(button.targetGraphic,kind);
   if(!button.GetComponent<MenuButtonState>())button.gameObject.AddComponent<MenuButtonState>();
   var old=button.GetComponent<BasketballButtonFeedback>();if(old)old.enabled=false;
   var feedback=button.GetComponent<CoveFeedback>();if(feedback)feedback.enabled=false;
   return button;
  }
  public static void Selected(Button button,bool value){var state=button?button.GetComponent<MenuButtonState>():null;if(state)state.Selected=value;}
  public static void Slider(Slider slider){
   Surface(slider.GetComponent<Graphic>(),MenuSurface.Track);
   if(slider.handleRect)Surface(slider.handleRect.GetComponent<Graphic>(),MenuSurface.Thumb);
  }
  public static void Sliders(Transform parent){foreach(var slider in parent.GetComponentsInChildren<Slider>(true))Slider(slider);}
  public static void Buttons(Transform parent){foreach(var button in parent.GetComponentsInChildren<Button>(true))Apply(button);}
  public static void TeamCard(Button button,bool blue){
   Apply(button,MenuSurface.Card);
   var badge=CoveUI.Rect(button.transform,"Team crest",18,17,66,70).gameObject.AddComponent<MenuTeamCrest>();badge.Blue=blue;badge.raycastTarget=false;
   var letter=BasketballUIStyle.Text(badge.transform,blue?"A":"B",0,15,66,40,25,true,TextAnchor.MiddleCenter,Color.white);
   letter.gameObject.AddComponent<Shadow>().effectColor=Navy;
   var caption=button.GetComponentInChildren<Text>();caption.rectTransform.anchoredPosition=new Vector2(94,-20);caption.rectTransform.sizeDelta=new Vector2(228,120);caption.alignment=TextAnchor.MiddleLeft;
   var rule=BasketballUIStyle.Plate(button.transform,"Team colour",19,101,184,3,blue?Cyan:CoveUI.Coral,1.5f,0);rule.raycastTarget=false;
  }
  internal static void Draw(VertexHelper mesh,Rect rect,Color tint,float radius,MenuSurface kind,float pressure=0,bool selected=false,bool disabled=false,float hover=0){
   mesh.Clear();if(rect.width<=0||rect.height<=0)return;
   float unit=Mathf.Clamp(Mathf.Min(rect.width,rect.height)/48f,.65f,1.6f);
   bool dialog=kind==MenuSurface.Dialog,track=kind==MenuSurface.Track,thumb=kind==MenuSurface.Thumb;
   radius=track||thumb?Mathf.Min(rect.width,rect.height)/2:Mathf.Min(radius,dialog?26:16);
   float rim=(dialog?3.2f:1.8f)*unit;
   Color ink=Navy,accent=selected?Gold:Cyan;
   if(disabled){tint=Color.Lerp(tint,CoveUI.Hex("AEBCC7"),.68f);accent=CoveUI.Hex("8AADB9");}
   bool dark=tint.r+tint.g+tint.b<1.5f;
   Color top=Color.Lerp(tint,Color.white,dialog?.18f:.29f+hover*.12f),bottom=Color.Lerp(tint,dark?Color.black:Navy,dialog?.035f:.12f);
   top.a=bottom.a=tint.a;
   if(track){Gradient(mesh,rect,radius,Navy,CoveUI.Hex("36556B"));Gradient(mesh,Inset(rect,2*unit),radius-2*unit,CoveUI.Hex("1C526C"),CoveUI.Hex("287D91"));return;}
   var shadow=rect;shadow.y-=unit*(dialog?5:3)*(1-pressure);Gradient(mesh,shadow,radius,new Color(.025f,.09f,.16f,.22f),new Color(.025f,.09f,.16f,.4f));
   rect.y-=pressure*1.5f*unit;
   Gradient(mesh,rect,radius,Color.Lerp(ink,Color.white,.12f),ink);
   var r=Inset(rect,rim);Gradient(mesh,r,Mathf.Max(0,radius-rim),Color.Lerp(accent,Color.white,.38f),Color.Lerp(accent,ink,.3f));
   r=Inset(r,unit*(selected?2.1f:1.1f));Gradient(mesh,r,Mathf.Max(0,radius-rim-unit),Color.Lerp(top,Color.white,.4f),bottom);
   r=Inset(r,unit);Gradient(mesh,r,Mathf.Max(0,radius-rim-2*unit),top,bottom);
   // A short upper highlight and lower bevel keep the treatment crisp at small sizes.
   float inset=Mathf.Min(radius+3*unit,rect.width*.18f);
   var shine=new Rect(rect.xMin+inset,rect.yMax-(dialog?5.5f:5)*unit,Mathf.Max(0,rect.width-inset*2),unit);
   Gradient(mesh,shine,unit,new Color(1,1,1,.65f),new Color(1,1,1,.4f));
   if(dialog){
    float width=Mathf.Min(rect.width*.18f,70*unit);var cap=new Rect(rect.center.x-width*.5f,rect.yMax-3.2f*unit,width,2.2f*unit);
    Gradient(mesh,cap,unit,Color.Lerp(Gold,Color.white,.4f),Gold);
   }
   if(selected&&kind==MenuSurface.Card){var bar=new Rect(rect.xMin+14*unit,rect.yMin+7*unit,rect.width-28*unit,2*unit);Gradient(mesh,bar,unit,Gold,Gold);}
  }
  static Rect Inset(Rect r,float amount)=>new(r.x+amount,r.y+amount,Mathf.Max(0,r.width-amount*2),Mathf.Max(0,r.height-amount*2));
  internal static void Gradient(VertexHelper mesh,Rect r,float radius,Color top,Color bottom){
   if(r.width<=0||r.height<=0)return;radius=Mathf.Clamp(radius,0,Mathf.Min(r.width,r.height)*.5f);int start=mesh.currentVertCount;
   Color At(float y)=>Color.Lerp(bottom,top,Mathf.InverseLerp(r.yMin,r.yMax,y));
   mesh.AddVert(r.center,At(r.center.y),Vector2.zero);
   const int segments=12,count=4*(segments+1);
   for(int corner=0;corner<4;corner++){
    var center=new Vector2(corner==0||corner==3?r.xMax-radius:r.xMin+radius,corner<2?r.yMax-radius:r.yMin+radius);
    for(int i=0;i<=segments;i++){float a=(corner*90+i*90f/segments)*Mathf.Deg2Rad;var p=center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;mesh.AddVert(p,At(p.y),Vector2.zero);}
   }
   for(int i=0;i<count;i++)mesh.AddTriangle(start,start+1+i,start+1+(i+1)%count);
  }
 }

 // Only the mesh responds to presses; the original RectTransform and raycast
 // rectangle remain stationary, even when a button contains dynamic text/art.
 public sealed class MenuButtonState:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler,ISelectHandler,IDeselectHandler,ICancelHandler {
  public bool Selected;Button button;BasketballPlate plate;FishingStickerGraphic fishing;CovePlate cove;bool hover,focus;int pointer=int.MinValue;float pressure,glow;bool lastSelected,lastDisabled;float lastPressure=-1,lastGlow=-1;
  void Awake(){button=GetComponent<Button>();plate=button.targetGraphic as BasketballPlate;fishing=button.targetGraphic as FishingStickerGraphic;cove=button.targetGraphic as CovePlate;}
  public void OnPointerEnter(PointerEventData e)=>hover=true;
  public void OnPointerExit(PointerEventData e){hover=false;if(pointer==e.pointerId)pointer=int.MinValue;}
  public void OnPointerDown(PointerEventData e){if(e.button==PointerEventData.InputButton.Left&&pointer==int.MinValue&&button.IsInteractable())pointer=e.pointerId;}
  public void OnPointerUp(PointerEventData e){if(pointer==e.pointerId)pointer=int.MinValue;}
  public void OnSelect(BaseEventData e)=>focus=true;
  public void OnDeselect(BaseEventData e)=>focus=false;
  public void OnCancel(BaseEventData e){pointer=int.MinValue;hover=focus=false;}
  void OnDisable(){pointer=int.MinValue;hover=focus=false;pressure=glow=0;lastPressure=-1;}
  void OnApplicationFocus(bool value){if(!value)OnCancel(null);}
  void OnApplicationPause(bool value){if(value)OnCancel(null);}
  void LateUpdate(){
   bool disabled=!button.IsInteractable();if(disabled)pointer=int.MinValue;
   pressure=Mathf.MoveTowards(pressure,pointer!=int.MinValue&&!MenuPreferences.ReducedMotion?1:0,Time.unscaledDeltaTime*14);
   glow=Mathf.MoveTowards(glow,!disabled&&(hover||focus)?1:0,Time.unscaledDeltaTime*10);
   bool selected=Selected||focus;
   if(selected==lastSelected&&disabled==lastDisabled&&Mathf.Approximately(pressure,lastPressure)&&Mathf.Approximately(glow,lastGlow))return;
   lastSelected=selected;lastDisabled=disabled;lastPressure=pressure;lastGlow=glow;
   if(plate){plate.menuSelected=selected;plate.menuDisabled=disabled;plate.menuHover=glow;plate.pressure=pressure;plate.Refresh();}
   if(fishing){fishing.menuSelected=selected;fishing.menuDisabled=disabled;fishing.menuHover=glow;fishing.pressure=pressure;fishing.SetVerticesDirty();}
   if(cove){cove.menuSelected=selected;cove.menuDisabled=disabled;cove.menuHover=glow;cove.pressure=pressure;cove.SetVerticesDirty();}
  }
 }

 public sealed class MenuTeamCrest:MaskableGraphic {
  public bool Blue;
  protected override void OnPopulateMesh(VertexHelper mesh){
   mesh.Clear();var r=rectTransform.rect;var points=new[]{new Vector2(.5f,1),new Vector2(.96f,.84f),new Vector2(.9f,.34f),new Vector2(.5f,0),new Vector2(.1f,.34f),new Vector2(.04f,.84f)};
   void Shape(float scale,Color top,Color bottom){int start=mesh.currentVertCount;mesh.AddVert(r.center,Color.Lerp(bottom,top,.5f),Vector2.zero);foreach(var p in points){var point=r.center+Vector2.Scale(p-Vector2.one*.5f,r.size)*scale;mesh.AddVert(point,Color.Lerp(bottom,top,p.y),Vector2.zero);}for(int i=0;i<points.Length;i++)mesh.AddTriangle(start,start+1+i,start+1+(i+1)%points.Length);}
   Shape(1,MenuDesigns.Navy,MenuDesigns.Navy);Shape(.91f,Color.white,MenuDesigns.Gold);Shape(.8f,Blue?CoveUI.Hex("50DEEF"):CoveUI.Hex("FF9477"),Blue?CoveUI.Hex("1671C4"):CoveUI.Hex("C43D54"));
  }
 }
}
