using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 // Path, landing and ring share the design space of the fixed Kick hitbox.
 public sealed class FootballKickGuide:MaskableGraphic {
  KickButton owner;BasketballPlate landing,cue;Text landingHint,cueText,instruction;float power;
  public string Cue=>cueText.text;
  public void Build(KickButton button){
   owner=button;raycastTarget=false;
   landing=BasketballUIStyle.Plate(transform,"Kick cancel landing",0,0,102,61,FootballUIStyle.Cancel,23,2,true);var r=landing.rectTransform;r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(-165,114.5f);landing.inset=2;
   BasketballUIStyle.Art(r,BasketballIcon.Close,11,13,17,17);button.cancelLabel=BasketballUIStyle.Text(r,"Cancel",34,9,64,27,17,true);button.cancelArea=r;
   landingHint=BasketballUIStyle.Text(r,"SLIDE HERE",4,35,94,18,9,false,TextAnchor.MiddleCenter);
   var hint=BasketballUIStyle.Plate(transform,"Drag instruction",0,0,188,32,CoveUI.Cream,11,0);hint.rectTransform.anchorMin=hint.rectTransform.anchorMax=hint.rectTransform.pivot=Vector2.one*.5f;hint.rectTransform.anchoredPosition=new Vector2(-145,171);
   FootballUIStyle.Art(hint.transform,FootballIcon.Swipe,7,5,23,23);instruction=BasketballUIStyle.Text(hint.transform,"Keep holding · slide to cancel",36,0,146,32,10);
   var title=BasketballUIStyle.Text(transform,"KICK POWER",0,0,116,22,11,true,TextAnchor.MiddleCenter,CoveUI.Cream);title.rectTransform.anchorMin=title.rectTransform.anchorMax=new Vector2(.5f,.5f);title.rectTransform.anchoredPosition=new Vector2(-58,98);title.gameObject.AddComponent<Shadow>().effectDistance=new Vector2(0,-1);
   cue=BasketballUIStyle.Plate(transform,"Kick release cue",0,0,128,27,CoveUI.Cream,11,1.5f,true);cue.rectTransform.anchorMin=cue.rectTransform.anchorMax=cue.rectTransform.pivot=Vector2.one*.5f;cue.rectTransform.anchoredPosition=new Vector2(0,-77);
   cueText=BasketballUIStyle.Text(cue.transform,"Release to kick",3,0,122,27,11,true,TextAnchor.MiddleCenter);gameObject.SetActive(false);
  }
  public void Begin(){gameObject.SetActive(true);if(landing)landing.gameObject.SetActive(true);Refresh();}
  public void End(){if(landing)landing.gameObject.SetActive(false);gameObject.SetActive(false);}
  public void Refresh(){
   power=PlayerView.Instance?PlayerView.Instance.Charge:0;bool armed=owner.CancelArmed;
   landing.color=armed?FootballUIStyle.CancelArmed:FootballUIStyle.Cancel;landing.edge=armed?CoveUI.Ink:new Color(.6f,.65f,.6f,.8f);landing.Refresh();
   landingHint.text=armed?"LIFT TO CANCEL":"SLIDE HERE";instruction.text=armed?"Slide back to keep aiming":"Keep holding · slide to cancel";
   cue.color=armed?FootballUIStyle.CancelArmed:CoveUI.Cream;cueText.text=armed?"Release to cancel":power>=1?"Full power · release":"Release to kick";SetVerticesDirty();
  }
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();BasketballUIMesh.Arc(v,60,135,405,new Color(.07f,.25f,.31f,.7f),7);BasketballUIMesh.Arc(v,60,135,135+270*power,BasketballUIStyle.Shoot,7);
   Vector2 a=new(-44,44),b=new(-62,68),c=new(-84,76),d=new(-113,85);Vector2 previous=a;
   for(int i=1;i<=48;i++){float t=i/48f,u=1-t;var next=u*u*u*a+3*u*u*t*b+3*u*t*t*c+t*t*t*d;BasketballUIMesh.Line(v,previous,next,new Color(.07f,.25f,.31f,.17f),8);if(i%6<3)BasketballUIMesh.Line(v,previous,next,new Color(1,.976f,.914f,.58f),3);previous=next;}
  }
 }
}
