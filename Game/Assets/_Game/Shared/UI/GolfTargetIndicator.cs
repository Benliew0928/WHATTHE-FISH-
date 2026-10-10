using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Each device reads only its local player's target and owned ball.
 [DefaultExecutionOrder(150)]
 public sealed class GolfTargetIndicator:MonoBehaviour {
  RectTransform page,targetArrow,targetCard,ballMarker,ballArrow;Text targetLabel,ballLabel;GolfHUDLayout layout;GolfLeaderboardUI scorecard;readonly Vector3[] corners=new Vector3[4];
  public int TargetHole {get;private set;}
  public ulong BallOwner {get;private set;}
  public void Build(RectTransform parent,Font font){
   page=parent;scorecard=parent.GetComponentInChildren<GolfLeaderboardUI>();layout=Resources.Load<GolfHUDLayout>("GolfUILayout");if(!layout)layout=ScriptableObject.CreateInstance<GolfHUDLayout>();
   targetArrow=Arrow("Your target hole",parent,new Vector2(.5f,1),layout.targetArrowPosition,layout.targetArrowSize,GolfHUDStyle.Teal);
   targetCard=GolfHUDStyle.Rect("Golf target card",parent,new Vector2(.5f,1),layout.targetPosition,layout.targetSize);GolfHUDStyle.Panel(targetCard,GolfHUDStyle.Cream).raycastTarget=false;
   var flag=GolfHUDStyle.Rect("Golf target flag",targetCard,new Vector2(.5f,.5f),new Vector2(-layout.targetSize.x*.39f,0),new Vector2(31,31)).gameObject.AddComponent<CoveIcon>();flag.symbol=CoveSymbol.Flag;flag.color=GolfHUDStyle.Ink;flag.raycastTarget=false;
   targetLabel=GolfHUDStyle.Text("Your target",targetCard,"",new Vector2(layout.targetSize.x-65,layout.targetSize.y-12),layout.targetFontSize);targetLabel.rectTransform.anchoredPosition=new Vector2(18,0);
   ballMarker=GolfLeaderboardUI.Rect("Your ball indicator",parent,new Vector2(.5f,.5f),Vector2.zero,new Vector2(180,62));
   ballArrow=Arrow("Ball arrow",ballMarker,new Vector2(.5f,.5f),new Vector2(0,-10),new Vector2(20,25),LocalProfile.Hex("F0B956"));
   ballLabel=GolfHUDStyle.Text("Your ball",ballMarker,"",new Vector2(250,30),layout.ballFontSize,TextAnchor.MiddleCenter,GolfHUDStyle.Gold);ballLabel.rectTransform.anchoredPosition=new Vector2(0,22);var outline=ballLabel.gameObject.AddComponent<Outline>();outline.effectColor=GolfHUDStyle.Ink;outline.effectDistance=new Vector2(1,-1);
  }
  static RectTransform Arrow(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size,Color color){var r=GolfLeaderboardUI.Rect(name,parent,anchor,position,size);var arrow=r.gameObject.AddComponent<GolfArrowGraphic>();arrow.color=color;arrow.raycastTarget=false;return r;}
  bool LabelOverlaps(RectTransform obstacle){
   if(!obstacle||!obstacle.gameObject.activeInHierarchy)return false;ballLabel.rectTransform.GetWorldCorners(corners);var labelBounds=new Rect(corners[0].x,corners[0].y,corners[2].x-corners[0].x,corners[2].y-corners[0].y);
   obstacle.GetWorldCorners(corners);return labelBounds.Overlaps(new Rect(corners[0].x,corners[0].y,corners[2].x-corners[0].x,corners[2].y-corners[0].y));
  }
  public static float Bearing(Vector3 from,Vector3 to,float yaw){var direction=Quaternion.Euler(0,-yaw,0)*(to-from);return -Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg;}
  void LateUpdate(){
   var app=AppRoot.Instance;var match=GolfMatchManager.Instance;var actor=app?app.LocalAthlete:null;
   var hole=match&&match.Context?match.Target(actor):null;var ball=match&&match.Context?match.Ball(actor):null;
   var position=layout.targetPosition;var hud=GolfHUD.Instance;
   if(hud&&hud.CourseRect&&scorecard&&scorecard.BoardRect){
    hud.CourseRect.GetWorldCorners(corners);float left=page.InverseTransformPoint(corners[2]).x+layout.gap+layout.targetSize.x*.5f;
    scorecard.BoardRect.GetWorldCorners(corners);float right=page.InverseTransformPoint(corners[0]).x-layout.gap-layout.targetSize.x*.5f;
    if(left<=right)position.x=Mathf.Clamp(position.x,left,right);
   }
   targetCard.anchoredPosition=position;targetCard.sizeDelta=layout.targetSize;targetArrow.anchoredPosition=layout.targetArrowPosition+Vector2.right*(position.x-layout.targetPosition.x);targetArrow.sizeDelta=layout.targetArrowSize;
   TargetHole=hole?.number??0;targetArrow.gameObject.SetActive(hole!=null&&match.State.Running);targetCard.gameObject.SetActive(hole!=null&&match.State.Running);
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
   // Keep the world arrow fixed; only move its caption clear of fixed HUD cards.
   ballLabel.rectTransform.anchoredPosition=new Vector2(0,22);
   if(LabelOverlaps(targetCard)||hud&&LabelOverlaps(hud.CourseRect)||scorecard&&(LabelOverlaps(scorecard.BoardRect)||LabelOverlaps(scorecard.CountdownRect)||LabelOverlaps(scorecard.NoticeRect)))ballLabel.rectTransform.anchoredPosition=new Vector2(0,-68);
  }
 }
}
