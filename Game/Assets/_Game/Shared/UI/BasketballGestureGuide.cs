using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public enum BasketballDragChoice { Center,Upper,Diagonal,Outside,Cancel }
 public sealed class BasketballGestureGuide:MaskableGraphic {
  public BasketballShootButton owner;
  public BasketballDragChoice Choice {get;private set;}
  public bool TimingVisible {get;private set;}
  public string Cue=>cue?cue.text:"";
  readonly Vector2[] targets={new(0,145),new(-105,100),new(-178,22)};
  BasketballPlate[] landings;Text[] names,labels;Text hint,cue;BasketballPlate cuePlate,hintPlate;float half,radius,power,window;bool timing,pass,ready;Vector2 contact;bool hasContact;
  public RectTransform CancelArea=>landings[2].rectTransform;
  public Vector2 Target(BasketballDragChoice choice)=>targets[choice==BasketballDragChoice.Upper?0:choice==BasketballDragChoice.Diagonal?1:2];
  public void Build(BasketballShootButton control){
   owner=control;pass=control.pass;half=((RectTransform)control.transform).rect.width*.5f;radius=half+12;raycastTarget=false;
   landings=new BasketballPlate[3];names=new Text[3];labels=new Text[3];
   for(int i=0;i<3;i++){
    var p=BasketballUIStyle.Plate(transform,"Drag destination "+i,0,0,92,49,i==2?CoveUI.Hex("FFE2D5"):CoveUI.Cream,24,1.5f);p.rectTransform.anchorMin=p.rectTransform.anchorMax=p.rectTransform.pivot=Vector2.one*.5f;p.rectTransform.anchoredPosition=targets[i];p.dashed=true;p.edge=new Color(.07f,.25f,.31f,.5f);landings[i]=p;
    names[i]=BasketballUIStyle.Text(p.transform,i==0?(pass?"Loft":"Dunk"):i==1?(pass?"Bounce":"Layup"):"× Cancel",3,3,86,24,16,true,TextAnchor.MiddleCenter,i==2?CoveUI.Hex("9C3335"):CoveUI.Ink);
    labels[i]=BasketballUIStyle.Text(p.transform,i==0?"Drag ↑":i==1?"Drag ↖":"Drag ←",3,28,86,15,11,false,TextAnchor.MiddleCenter,CoveUI.Muted);
   }
   cuePlate=BasketballUIStyle.Plate(transform,"Release cue",0,0,148,24,CoveUI.Cream,12,0);cuePlate.rectTransform.anchorMin=cuePlate.rectTransform.anchorMax=cuePlate.rectTransform.pivot=Vector2.one*.5f;cuePlate.rectTransform.anchoredPosition=new Vector2(0,pass?-56:-half-20);
   cue=BasketballUIStyle.Text(cuePlate.transform,"Release in green",3,0,142,24,13,true,TextAnchor.MiddleCenter);
   hintPlate=BasketballUIStyle.Plate(transform,"Gesture guidance",0,0,260,30,CoveUI.Cream,10,0);hintPlate.rectTransform.anchorMin=hintPlate.rectTransform.anchorMax=hintPlate.rectTransform.pivot=Vector2.one*.5f;hintPlate.rectTransform.anchoredPosition=new Vector2(-85,190);
   hint=BasketballUIStyle.Text(hintPlate.transform,"Keep holding · slide along a path",5,1,250,28,11,false,TextAnchor.MiddleCenter);
   control.cancelArea=CancelArea;control.cancelLabel=labels[2];gameObject.SetActive(false);
  }
  public void Begin(){Choice=BasketballDragChoice.Center;hasContact=false;gameObject.SetActive(true);Refresh();}
  public void End(){hasContact=false;gameObject.SetActive(false);}
  public BasketballDragChoice Track(Vector2 local){
   contact=local;hasContact=true;
   if(Mathf.Abs(local.x-targets[2].x)<53&&Mathf.Abs(local.y-targets[2].y)<31)return SetChoice(BasketballDragChoice.Cancel);
   float distance=local.magnitude;
   // Use a smaller return zone and wider retained corridor to prevent boundary chatter.
   if(distance<half+6)return SetChoice(BasketballDragChoice.Center);
   for(int i=0;i<2;i++){
    var choice=i==0?BasketballDragChoice.Upper:BasketballDragChoice.Diagonal;
    if(Mathf.Abs(local.x-targets[i].x)<53&&Mathf.Abs(local.y-targets[i].y)<31)return SetChoice(choice);
    float alignment=Vector2.Dot(local.normalized,targets[i].normalized);
    if(distance>half+(Choice==choice?6:22)&&distance<targets[i].magnitude+36&&alignment>(Choice==choice?.87f:.94f))return SetChoice(choice);
   }
   return SetChoice(distance<radius+20?BasketballDragChoice.Center:BasketballDragChoice.Outside);
  }
  BasketballDragChoice SetChoice(BasketballDragChoice choice){if(choice!=Choice&&choice!=BasketballDragChoice.Outside)BasketballAudio.PlayMenu(choice==BasketballDragChoice.Cancel?BasketballSound.UIBack:BasketballSound.UISelect);Choice=choice;SetVerticesDirty();return choice;}
  public void Refresh(){
   var view=PlayerView.Instance;if(!view||!owner)return;
   pass=owner.pass;power=pass?view.PassPower:view.ShotPower;window=view.ShotWindow;timing=!pass&&view.ShotNeedsTiming;TimingVisible=timing;
   if(!owner.Tracking)Choice=pass?(view.PassBend>.1f?BasketballDragChoice.Upper:view.PassBend<-.1f?BasketballDragChoice.Diagonal:BasketballDragChoice.Center):view.ShotFinish==BasketballFinish.Dunk?BasketballDragChoice.Upper:view.ShotFinish==BasketballFinish.Layup?BasketballDragChoice.Diagonal:BasketballDragChoice.Center;
   bool selected=Choice==BasketballDragChoice.Upper||Choice==BasketballDragChoice.Diagonal;ready=selected&&!pass&&!timing;
   var reason=selected&&!pass?view.FinishAvailability:BasketballFinishReason.Ready;
   for(int i=0;i<3;i++){
    bool on=i==0&&Choice==BasketballDragChoice.Upper||i==1&&Choice==BasketballDragChoice.Diagonal||i==2&&Choice==BasketballDragChoice.Cancel;
    bool unavailable=false;BasketballFinishReason availability=BasketballFinishReason.Ready;
    if(!pass&&i<2&&BasketballBall.Active){availability=BasketballBall.Active.FinishAvailability(view.target,i==0?BasketballFinish.Dunk:BasketballFinish.Layup,view.SelectedShotHoop);unavailable=availability!=BasketballFinishReason.Ready;}
    landings[i].color=on?(i==2?CoveUI.Hex("FFC5AE"):unavailable?BasketballUIStyle.Paper:CoveUI.Mint):i==2?CoveUI.Hex("FFE2D5"):unavailable?CoveUI.Hex("E8E9DF"):CoveUI.Cream;
    landings[i].dashed=!on;landings[i].border=on?2.5f:1.5f;landings[i].edge=on?CoveUI.Ink:new Color(.07f,.25f,.31f,.5f);landings[i].Refresh();
    labels[i].text=on?(i==2?"Lift to cancel":unavailable?BasketballFinishRules.ShortHint(availability):"Release"):i==0?"Drag ↑":i==1?"Drag ↖":"Drag ←";
   }
   bool sweet=timing&&Mathf.Abs(power-BasketballBall.SweetSpot)<=window;
   cuePlate.color=Choice==BasketballDragChoice.Cancel?CoveUI.Hex("FFC5AE"):ready||sweet?CoveUI.Mint:CoveUI.Cream;
   cue.text=Choice==BasketballDragChoice.Cancel?"Release to cancel":Choice==BasketballDragChoice.Outside?"Slide back to button":selected?(pass?(Choice==BasketballDragChoice.Upper?"Loft · release":"Bounce · release"):timing?"Timed shot · green":Choice==BasketballDragChoice.Upper?"Release to dunk":"Release for layup"):pass?(power<.35f?"Short pass · release":power<.85f?"Building distance…":"Full pass · release"):"Release in green";
   hint.text=Choice==BasketballDragChoice.Cancel?"Lift to cancel · slide back to keep aiming":Choice==BasketballDragChoice.Outside?"Outside the paths · slide back to the button":selected&&!pass&&timing?(reason==BasketballFinishReason.Closer?"Too far for "+(Choice==BasketballDragChoice.Upper?"Dunk":"Layup"):BasketballFinishRules.ShortHint(reason))+" · release a timed shot":selected?"Lift your finger to use the selected action":"Keep holding · slide along a path";
   SetVerticesDirty();
  }
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();for(int i=0;i<3;i++){
    var target=targets[i];var direction=target.normalized;var start=direction*(half+5);var end=target-direction*32;bool on=i==0&&Choice==BasketballDragChoice.Upper||i==1&&Choice==BasketballDragChoice.Diagonal||i==2&&Choice==BasketballDragChoice.Cancel;
    BasketballUIMesh.Line(v,start,end,on?new Color(.65f,.91f,.8f,.7f):new Color(1,.976f,.914f,.27f),on?20:18);
    if(on)BasketballUIMesh.Line(v,start,end,new Color(.07f,.25f,.31f,.8f),2.5f);
    else {float len=Vector2.Distance(start,end);for(float d=0;d<len;d+=11)BasketballUIMesh.Line(v,start+direction*d,start+direction*Mathf.Min(d+4,len),new Color(1,.976f,.914f,.9f),2.5f);}
    var middle=Vector2.Lerp(start,end,.65f);var side=new Vector2(-direction.y,direction.x)*4;BasketballUIMesh.Line(v,middle-direction*7+side,middle,CoveUI.Ink,2.5f);BasketballUIMesh.Line(v,middle,middle-direction*7-side,CoveUI.Ink,2.5f);
   }
   if(pass||timing){
    BasketballUIMesh.Arc(v,radius,150,390,CoveUI.Ink,13);BasketballUIMesh.Arc(v,radius,150,390,CoveUI.Cream,8);
    if(pass)BasketballUIMesh.Arc(v,radius,150,150+240*power,BasketballUIStyle.Teal,8);
    else {
     BasketballUIMesh.Arc(v,radius,150+240*(BasketballBall.SweetSpot-window),150+240*(BasketballBall.SweetSpot+window),BasketballUIStyle.Green,9);
     var check=BasketballUIMesh.Polar(radius+12,306);BasketballUIMesh.Line(v,check+new Vector2(-3,0),check+new Vector2(-1,-3),CoveUI.Ink,1.5f);BasketballUIMesh.Line(v,check+new Vector2(-1,-3),check+new Vector2(4,4),CoveUI.Ink,1.5f);
     var p=BasketballUIMesh.Polar(radius,150+240*power);for(int layer=0;layer<2;layer++){float size=layer==0?8:5;int n=v.currentVertCount;v.AddVert(p+Vector2.up*size,layer==0?CoveUI.Ink:CoveUI.Cream,Vector2.zero);v.AddVert(p+Vector2.right*size,layer==0?CoveUI.Ink:CoveUI.Cream,Vector2.zero);v.AddVert(p+Vector2.down*size,layer==0?CoveUI.Ink:CoveUI.Cream,Vector2.zero);v.AddVert(p+Vector2.left*size,layer==0?CoveUI.Ink:CoveUI.Cream,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}
    }
   }
   // Actual fingers obscure their contact point; no extra cursor is drawn on mobile.
   if(hasContact&&!Application.isMobilePlatform){BasketballUIMesh.Dot(v,contact,10,new Color(.07f,.25f,.31f,.8f));BasketballUIMesh.Dot(v,contact,8,new Color(1,.976f,.914f,.5f));}
  }
 }
}
