using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class BasketballHUD:MonoBehaviour {
  Text totals,feedback,hint,roamLabel;RectTransform meter,marker,greenBand;Image fill;Button roamControl;
  public static void Create(Transform parent,Font font){
   var root=new GameObject("Basketball score and power",typeof(RectTransform)).GetComponent<RectTransform>();root.SetParent(parent,false);
   root.anchorMin=root.anchorMax=new Vector2(.5f,1);root.pivot=new Vector2(.5f,1);root.anchoredPosition=new Vector2(0,-14);root.sizeDelta=new Vector2(500,214);
   var hud=root.gameObject.AddComponent<BasketballHUD>();hud.Build(font);
  }
  static RectTransform Rect(Transform parent,string name,Vector2 size,Vector2 point){
   var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=new Vector2(.5f,1);r.sizeDelta=size;r.anchoredPosition=point;return r;
  }
  static Image Panel(Transform parent,string name,Vector2 size,Vector2 point,Color color){var r=Rect(parent,name,size,point);var image=r.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;return image;}
  static Text Label(Transform parent,string name,int size,Vector2 point,Font font){var t=Rect(parent,name,new Vector2(480,35),point).gameObject.AddComponent<Text>();t.font=font;t.fontSize=size;t.alignment=TextAnchor.MiddleCenter;t.color=Color.white;t.raycastTarget=false;t.supportRichText=false;return t;}
  void Build(Font font){
   var bar=Panel(transform,"Shared practice score",new Vector2(500,78),Vector2.zero,new Color(.025f,.13f,.18f,.92f));
   totals=Label(bar.transform,"Points and baskets",26,new Vector2(0,-4),font);
   feedback=Label(bar.transform,"Shot result",19,new Vector2(0,-37),font);
   meter=Panel(transform,"Shot power",new Vector2(360,15),new Vector2(0,-94),new Color(.02f,.08f,.12f,.9f)).rectTransform;
   fill=Panel(meter,"Power fill",new Vector2(1,15),Vector2.zero,new Color(1,.73f,.28f));fill.rectTransform.pivot=new Vector2(0,1);fill.rectTransform.anchoredPosition=new Vector2(-180,0);
   var band=Panel(meter,"Green release window",new Vector2(360*BasketballBall.SweetWindow*2,15),new Vector2(360*(BasketballBall.SweetSpot-.5f),0),new Color(.3f,1,.63f,.85f));
   greenBand=band.rectTransform;
   marker=Panel(meter,"Release marker",new Vector2(3,23),new Vector2(-180,4),Color.white).rectTransform;
   hint=Label(transform,"Power hint",17,new Vector2(0,-118),font);
   var roam=Panel(transform,"Court or free roam",new Vector2(200,44),new Vector2(0,-158),new Color(.025f,.13f,.18f,.92f));roam.raycastTarget=true;
   roamControl=roam.gameObject.AddComponent<Button>();roamControl.targetGraphic=roam;roamControl.onClick.AddListener(ToggleCourt);
   roamLabel=Label(roam.transform,"Court mode",19,Vector2.zero,font);roamLabel.rectTransform.sizeDelta=new Vector2(200,44);
  }
  public void ToggleCourt(){
   var app=AppRoot.Instance;var ball=BasketballBall.Active;var actor=app?app.LocalAthlete:null;
   if(!ball||!ball.Playing||!actor||actor.inTransit)return;
   app.view.ClearMatchInput();var net=actor.GetComponent<NetworkAthlete>();
   if(net&&net.IsSpawned)net.BasketballFreeRoamRpc(!actor.BasketballFreeRoam);else ball.SetFreeRoam(actor,!actor.BasketballFreeRoam);
  }
  void Update(){
   var ball=BasketballBall.Active;var view=PlayerView.Instance;if(!ball||!view)return;var score=ball.Score;
   totals.text=score.points+" PTS    ·    "+score.made+" / "+score.attempts+" MADE";
   bool charging=view.ShotCharging,timing=view.ShotNeedsTiming;bool fresh=BasketballMotion.Clock-score.changed<2.5;
   bool roaming=view.target&&view.target.BasketballFreeRoam;
   roamLabel.text=roaming?"Play basketball":"Free roam";roamControl.interactable=ball.Playing&&view.target&&!view.target.inTransit;
   feedback.text=roaming?"FREE ROAM  ·  WALK TO SKY-SAIL":ball.StealCount>0&&BasketballMotion.Clock-ball.StealTime<1.4?"BALL LOOSE  ·  CHASE IT":charging?"RELEASE IN THE GREEN":score.result==BasketballResult.Flying?"SHOT IN FLIGHT":fresh&&score.result==BasketballResult.Scored?"BASKET!  +"+score.lastPoints:fresh&&score.result==BasketballResult.Missed?"MISSED  ·  GET THE REBOUND":"SHARED COURT PRACTICE";
   if(ball.FinishNoticeFor(view.target)&&!charging)feedback.text=BasketballFinishRules.Hint(ball.FinishNotice.reason);
   if(charging&&view.ShotFinish!=BasketballFinish.Shot)feedback.text=view.ShotFinish.ToString().ToUpperInvariant()+" - "+(timing?BasketballFinishRules.Hint(view.FinishAvailability):"RELEASE NOW");
   if(view.target&&view.target.BasketballMotion.Finishing)feedback.text=view.target.BasketballMotion.Action.ToString().ToUpperInvariant()+" - FINISHING";
   feedback.color=fresh&&score.result==BasketballResult.Scored?new Color(.4f,1,.7f):Color.white;
   meter.gameObject.SetActive(timing);hint.gameObject.SetActive(charging);
   if(!charging)return;
   if(!timing){hint.text=view.ShotFinish==BasketballFinish.Dunk?"No timing - guaranteed when unblocked":"No timing - 85% make chance";return;}
   float power=view.ShotPower;bool sweet=Mathf.Abs(power-BasketballBall.SweetSpot)<=view.ShotWindow;
   greenBand.sizeDelta=new Vector2(720*view.ShotWindow,15);
   fill.rectTransform.sizeDelta=new Vector2(360*power,15);marker.anchoredPosition=new Vector2(360*(power-.5f),4);
   fill.color=sweet?new Color(.3f,1,.63f):power>BasketballBall.SweetSpot?new Color(1,.43f,.3f):new Color(1,.73f,.28f);
   hint.text=(sweet?"Release!":"Wait for green")+" - "+view.ShotDistance.ToString("F1")+" m - "+(Application.isMobilePlatform?"Slide up / down; left to Cancel":"E + Up: dunk / Down: layup; X: cancel");
  }
 }
}
