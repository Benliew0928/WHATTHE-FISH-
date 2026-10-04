using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Each device reads only its local player's target and owned ball.
 [DefaultExecutionOrder(150)]
 public sealed class GolfTargetIndicator:MonoBehaviour {
  RectTransform page,targetArrow,ballMarker,ballArrow;Text targetLabel,ballLabel;
  public int TargetHole {get;private set;}
  public ulong BallOwner {get;private set;}
  public void Build(RectTransform parent,Font font){
   page=parent;targetArrow=Arrow("Your target hole",parent,new Vector2(.5f,1),new Vector2(0,-72),new Vector2(28,36),LocalProfile.Teams[0]);
   targetLabel=GolfLeaderboardUI.Text("Your target",parent,font,new Vector2(.5f,1),new Vector2(0,-116),new Vector2(350,32),21);targetLabel.alignment=TextAnchor.MiddleCenter;
   ballMarker=GolfLeaderboardUI.Rect("Your ball indicator",parent,new Vector2(.5f,.5f),Vector2.zero,new Vector2(180,62));
   ballArrow=Arrow("Ball arrow",ballMarker,new Vector2(.5f,.5f),new Vector2(0,-10),new Vector2(20,25),LocalProfile.Hex("F0B956"));
   ballLabel=GolfLeaderboardUI.Text("Your ball",ballMarker,font,new Vector2(.5f,.5f),new Vector2(0,22),new Vector2(230,28),17);ballLabel.alignment=TextAnchor.MiddleCenter;ballLabel.color=LocalProfile.Hex("F0B956");
  }
  static RectTransform Arrow(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size,Color color){var r=GolfLeaderboardUI.Rect(name,parent,anchor,position,size);var arrow=r.gameObject.AddComponent<GolfArrowGraphic>();arrow.color=color;arrow.raycastTarget=false;return r;}
  public static float Bearing(Vector3 from,Vector3 to,float yaw){var direction=Quaternion.Euler(0,-yaw,0)*(to-from);return -Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg;}
  void LateUpdate(){
   var app=AppRoot.Instance;var match=GolfMatchManager.Instance;var actor=app?app.LocalAthlete:null;
   var hole=match&&match.Context?match.Target(actor):null;var ball=match&&match.Context?match.Ball(actor):null;
   TargetHole=hole?.number??0;targetArrow.gameObject.SetActive(hole!=null&&match.State.Running);targetLabel.gameObject.SetActive(hole!=null&&match.State.Running);
   // Read the presented camera, including the cart's third-person reverse turn.
   if(hole!=null){targetArrow.localRotation=Quaternion.Euler(0,0,Bearing(actor.transform.position,hole.cup.position,app.view.transform.eulerAngles.y));targetLabel.text=$"HOLE {hole.number}  •  {Vector3.Distance(actor.transform.position,hole.cup.position):0} m";}
   bool show=ball&&match.State.Running&&match.Player(actor)?.IsFinished==false;ballMarker.gameObject.SetActive(show);if(!show)return;BallOwner=ball.Owner;
   var camera=app.view.GetComponent<Camera>();var screen=camera.WorldToScreenPoint(ball.Body.position);
   // Keep the arrow tip above the tiny ball at every camera distance.
   RectTransformUtility.ScreenPointToLocalPointInRectangle(page,screen,null,out var point);if(screen.z<0)point=-point;else point+=Vector2.up*34;
   var r=page.rect;var clamped=new Vector2(Mathf.Clamp(point.x,r.xMin+100,r.xMax-100),Mathf.Clamp(point.y,r.yMin+155,r.yMax-155));
   bool off=screen.z<0||Vector2.Distance(clamped,point)>1;ballMarker.anchoredPosition=clamped;
   ballArrow.localRotation=Quaternion.Euler(0,0,off?Bearing(actor.transform.position,ball.Body.position,camera.transform.eulerAngles.y):180);
   ballLabel.text=$"YOUR BALL  •  {Vector3.Distance(actor.transform.position,ball.Body.position):0} m";
  }
 }
}
