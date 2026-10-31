using UnityEngine;

namespace WhatTheFish {
 public sealed partial class PlayerView {
  enum GolfCameraPhase { None,Contact,Flight,Return }
  GolfCameraPhase golfCameraPhase;
  GolfBall followedGolfBall;Athlete golfCameraActor;
  uint golfCameraRound,golfCameraReset,golfCameraStrike,golfCameraSwing;
  float golfCameraAt,golfCameraStill;Vector3 golfCameraFrom,golfCameraFocus,golfCameraPrevious,golfCameraVelocity;
  Quaternion golfCameraRotation;
  public bool GolfShotCameraActive=>golfCameraPhase!=GolfCameraPhase.None;
  public bool GolfFollowingBall=>golfCameraPhase==GolfCameraPhase.Flight;
  uint observedHazardLaunch,observedHazardRound;GolfBall observedHazardBall;
  Vector2 replayPress;float replayPressedAt=-1;int replayPointer=int.MinValue;
  public bool SkipGolfReplay(){if(!GolfFollowingBall||Time.time-golfCameraAt<.45f)return false;ReturnFromGolfShot();return true;}
  bool UpdateGolfReplayInput(){
   if(!GolfFollowingBall||InputBlocked){ResetReplayPress();return false;}
   int count=Input.touchCount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(ReviewMouseLookInput)count=ReviewMouseLookInput.touchCount;
#endif
   if(count>0){
    // Never mix emulated mouse taps with touch, and never let another finger
    // replace the owner or release its replay tap.
    if(replayPointer==-1)ResetReplayPress();
    for(int i=0;i<count;i++){
     Touch touch;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
     if(ReviewMouseLookInput)touch=ReviewMouseLookInput.GetTouch(i);else
#endif
     touch=Input.GetTouch(i);
     if(ReplayTouch(touch))return true;
    }
    return false;
   }
   if(replayPointer>=0)ResetReplayPress();
   if(Input.GetMouseButtonDown(0)&&replayPointer==int.MinValue){replayPointer=-1;replayPress=Input.mousePosition;replayPressedAt=Time.unscaledTime;}
   if(Input.GetMouseButtonUp(0)&&replayPointer==-1){bool tap=ReplayTap(Input.mousePosition);ResetReplayPress();if(tap)return SkipGolfReplay();}
   return false;
  }
  bool ReplayTap(Vector2 at)=>replayPressedAt>=0&&Time.unscaledTime-replayPressedAt<.4f&&Vector2.Distance(replayPress,at)<24;
  void ResetReplayPress(){replayPointer=int.MinValue;replayPressedAt=-1;}
  bool ReplayTouch(Touch touch){
   if(touch.phase==TouchPhase.Began&&replayPointer==int.MinValue){replayPointer=touch.fingerId;replayPress=touch.position;replayPressedAt=Time.unscaledTime;}
   else if(touch.fingerId==replayPointer){
    if(touch.phase==TouchPhase.Moved&&Vector2.Distance(replayPress,touch.position)>=24)replayPressedAt=-1;
    if(touch.phase==TouchPhase.Canceled){ResetReplayPress();return false;}
    if(touch.phase==TouchPhase.Ended){bool tap=ReplayTap(touch.position);ResetReplayPress();if(tap)return SkipGolfReplay();}
   }
   return false;
  }
  void ObserveGolfHazard(){
   var match=GolfMatchManager.Instance;var ball=match&&match.Context?match.Ball(target):null;
   if(!ball){observedHazardBall=null;return;}
   if(observedHazardBall!=ball||observedHazardRound!=match.Round){observedHazardBall=ball;observedHazardRound=match.Round;observedHazardLaunch=ball.HazardLaunch;return;}
   if(observedHazardLaunch==ball.HazardLaunch)return;observedHazardLaunch=ball.HazardLaunch;
   if(!ball.HazardReplay){if(followedGolfBall==ball)ReturnFromGolfShot();return;}
   if(InputBlocked||!active||!match.State.Running)return;
   followedGolfBall=ball;golfCameraActor=target;golfCameraRound=match.Round;golfCameraReset=ball.ResetSequence;
   golfCameraPhase=GolfCameraPhase.Flight;golfCameraAt=Time.time;golfCameraStill=0;
   golfCameraFrom=transform.position;golfCameraRotation=transform.rotation;golfCameraFocus=golfCameraPrevious=ball.transform.position;golfCameraVelocity=Vector3.zero;
  }

  void WatchGolfShot(GolfBall ball){
   followedGolfBall=ball;golfCameraActor=target;golfCameraRound=GolfMatchManager.Instance.Round;
   golfCameraReset=ball.ResetSequence;golfCameraStrike=ball.StrikeSequence;golfCameraSwing=target.GolfClubMotion.State.sequence;
   golfCameraPhase=GolfCameraPhase.Contact;golfCameraAt=Time.time;golfCameraStill=0;
   golfCameraFrom=transform.position;golfCameraRotation=transform.rotation;
  }
  void ResetGolfShotCamera(){golfCameraPhase=GolfCameraPhase.None;followedGolfBall=null;golfCameraActor=null;ResetReplayPress();}
  void ReturnFromGolfShot(){
   if(golfCameraPhase==GolfCameraPhase.None||golfCameraPhase==GolfCameraPhase.Return)return;
   golfCameraPhase=GolfCameraPhase.Return;golfCameraAt=Time.time;
   ResetReplayPress();
   golfCameraFrom=transform.position;golfCameraRotation=transform.rotation;followedGolfBall=null;
  }
  static float GolfCameraEase(float t){t=Mathf.Clamp01(t);return t*t*t*(t*(t*6-15)+10);}
  void PresentGolfShotCamera(ref Vector3 position,ref Quaternion rotation){
   ObserveGolfHazard();
   if(golfCameraPhase==GolfCameraPhase.None)return;
   var match=GolfMatchManager.Instance;
   if(!match||!match.Context||match.Round!=golfCameraRound||target!=golfCameraActor||target.inTransit){ResetGolfShotCamera();return;}
   if(InputBlocked||!match.State.Running)ReturnFromGolfShot();
   if(golfCameraPhase==GolfCameraPhase.Contact||golfCameraPhase==GolfCameraPhase.Flight){
    if(!followedGolfBall||!followedGolfBall.Live||followedGolfBall.ResetSequence!=golfCameraReset)ReturnFromGolfShot();
   }
   if(golfCameraPhase==GolfCameraPhase.Contact){
    // A release is only a request. Wait for replicated ball contact and the
    // local golfer's accepted swing, including when hitting somebody else's ball.
    var swing=target.GolfClubMotion.State;
    if(followedGolfBall.StrikeSequence!=golfCameraStrike&&swing.sequence!=golfCameraSwing&&swing.action==GolfClubAction.Swing){
     golfCameraPhase=GolfCameraPhase.Flight;golfCameraAt=Time.time;
     golfCameraFocus=golfCameraPrevious=followedGolfBall.transform.position;golfCameraVelocity=Vector3.zero;
     if(golfPreview)golfPreview.Hide();
    }else if(Time.time-golfCameraAt>2.5f)ReturnFromGolfShot();
    else {position=golfCameraFrom;rotation=golfCameraRotation;return;}
   }
   if(golfCameraPhase==GolfCameraPhase.Flight){
    float dt=Mathf.Max(.0001f,Time.deltaTime),age=Time.time-golfCameraAt;
    // Use the presented ball, not a guest's kinematic body velocity. This
    // follows bounces/run-out without chasing a predicted landing point.
    var point=followedGolfBall.transform.position;var delta=point-golfCameraPrevious;
    if(!FiniteCameraPoint(point)||delta.magnitude>Mathf.Max(8,65*dt)){ReturnFromGolfShot();}
    else {
     var velocity=Vector3.ClampMagnitude(delta/dt,45);golfCameraPrevious=point;
     golfCameraVelocity=Vector3.Lerp(golfCameraVelocity,velocity,1-Mathf.Exp(-dt/.12f));
     golfCameraFocus=Vector3.Lerp(golfCameraFocus,point,1-Mathf.Exp(-dt/.055f));
     bool grounded=Physics.Raycast(point+Vector3.up*.08f,Vector3.down,.3f,(1<<8)|(1<<GolfTee.Layer),QueryTriggerInteraction.Ignore);
     golfCameraStill=age>.65f&&grounded&&golfCameraVelocity.magnitude<.3f?golfCameraStill+dt:0;
     if(golfCameraStill>.85f||age>22)ReturnFromGolfShot();
     else {
      var look=golfCameraFocus+Vector3.up*.3f+Vector3.ClampMagnitude(golfCameraVelocity*.08f,.8f);
      bool course=mode==2;
      var followRotation=Quaternion.Euler(course?34:Mathf.Clamp(pitch,22,48),yaw,0);
      float distance=Mathf.Lerp(course?10:5,course?16:8,Mathf.Clamp01(golfCameraVelocity.magnitude/24));
      if(course&&Physics.Raycast(point+Vector3.up*.1f,Vector3.down,out var terrain,100,(1<<8)|(1<<GolfTee.Layer),QueryTriggerInteraction.Ignore)){
       float height=Mathf.Max(0,point.y-terrain.point.y);look-=Vector3.up*(height*.3f);distance=Mathf.Max(distance,height*1.8f);
      }
      var desired=look-followRotation*Vector3.forward*distance;
      var path=desired-look;
      if(Physics.SphereCast(look,.2f,path.normalized,out var hit,path.magnitude,1<<8,QueryTriggerInteraction.Ignore))desired=look+path.normalized*Mathf.Max(.4f,hit.distance-.15f);
      float blend=GolfCameraEase(age/.65f);
      position=Vector3.Lerp(golfCameraFrom,desired,blend);rotation=Quaternion.Slerp(golfCameraRotation,followRotation,blend);return;
     }
    }
   }
   if(golfCameraPhase==GolfCameraPhase.Return){
    float blend=GolfCameraEase((Time.time-golfCameraAt)/.9f);
    position=Vector3.Lerp(golfCameraFrom,position,blend);rotation=Quaternion.Slerp(golfCameraRotation,rotation,blend);
    if(blend>=1)ResetGolfShotCamera();
   }
  }
 }
}
