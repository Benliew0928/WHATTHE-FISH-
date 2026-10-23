using UnityEngine;

namespace WhatTheFish {
 public sealed partial class PlayerView {
  bool activityProjection,activityFishing,activityLine;Athlete activityActor;
  bool activityWasOrthographic;float activityOriginalFov,activityOriginalSize,activityDistance,activityYaw;
  Vector3 activityFocus,activityPreviousActor;
  public bool ActivityStadiumFollowing=>activityProjection&&ActivityStadiumContext;
  bool ActivityStadiumContext {
   get {
    var app=AppRoot.Instance;
    return isActiveAndEnabled&&mode==2&&active&&target&&target.isActiveAndEnabled&&!target.IsSpectator&&!target.inTransit&&app&&app.Exploring&&
     !(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling)&&!GolfCartWorld.Driving(target)&&
     (app.SelectedSport==SportId.Golf&&GolfMatchManager.Instance&&GolfMatchManager.Instance.Context||app.SelectedSport==SportId.Fishing&&FishingGame.Instance&&FishingGame.Instance.Context);
   }
  }
  void ResetActivityStadiumCamera(){
   if(activityProjection&&cam){cam.orthographic=activityWasOrthographic;cam.fieldOfView=activityOriginalFov;cam.orthographicSize=activityOriginalSize;}
   activityProjection=false;activityActor=null;activityLine=false;
  }
  // Presented after locomotion and fish/ball interpolation. Court broadcast is
  // deliberately separate: these sports need the player's own shot/casting lane.
  internal void PresentActivityStadiumCamera(){
   if(TeleportSequence.OwnsCamera||FootballReplay.Presenting||FootballMVP.Presenting||BasketballMVP.Presenting||FishingCeremony.Presenting)return;
   if(!ActivityStadiumContext){ResetActivityStadiumCamera();return;}
   bool fishing=AppRoot.Instance.SelectedSport==SportId.Fishing;
   if(fishing)ResetFishingFightCamera();
   bool snap=!activityProjection||activityActor!=target||activityFishing!=fishing||Vector3.Distance(activityPreviousActor,target.transform.position)>20;
   if(!activityProjection){activityWasOrthographic=cam.orthographic;activityOriginalFov=cam.fieldOfView;activityOriginalSize=cam.orthographicSize;activityProjection=true;}
   cam.orthographic=false;cam.fieldOfView=fishing?52:50;cam.nearClipPlane=.15f;
   var forward=Quaternion.Euler(0,yaw,0)*Vector3.forward;
   var player=target.transform.position;var first=player+Vector3.up*1.25f;
   var second=first+forward*(fishing?4.5f:3);var focus=Vector3.Lerp(first,second,.5f);
   float angle=yaw,tilt=fishing?Mathf.Clamp(pitch+12,24,40):Mathf.Clamp(pitch+16,28,42),distance=fishing?10.5f:12;
   if(fishing){
    var game=FishingGame.Instance;var state=game.Player(target);bool line=state.HasValue&&state.Value.Busy;
    if(line){
     second=game.Presentation.HookPoint(state.Value);
     if(!FiniteCameraPoint(second))second=first+forward*5;
     if(!activityLine||snap){var direction=Vector3.ProjectOnPlane(second-player,Vector3.up);activityYaw=direction.sqrMagnitude>.01f?Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg:yaw;}
     // Fixed side during a cast/fight: left/right rod controls stay meaningful.
     angle=activityYaw;tilt=30;focus=Vector3.Lerp(first,second,.44f)+Vector3.up*.45f;
    }
    activityLine=line;
   }else if(GolfAiming){
    UpdateGolfPreview();var ball=GolfMatchManager.Instance.Ball(aimOwner);
    if(ball){first=ball.transform.position;second=golfPreview&&golfPreview.Visible?golfPreview.EndPoint:first+forward*8;
     if(!FiniteCameraPoint(second))second=first+forward*8;
     focus=Vector3.Lerp(first,second,.42f)+Vector3.up*.4f;tilt=GolfMode==GolfShotMode.Putt?38:32;distance=GolfMode==GolfShotMode.Putt?8:11;
    }
   }
   float blend=MenuPreferences.ReducedMotion?1:1-Mathf.Exp(-Time.unscaledDeltaTime/.16f);
   activityFocus=snap?focus:Vector3.Lerp(activityFocus,focus,blend);
   var rotation=Quaternion.Euler(tilt,angle,0);
   FitActivityPoint(rotation,first,ref distance,fishing?.36f:.48f);FitActivityPoint(rotation,second,ref distance,fishing?.36f:.48f);
   if(fishing)FitActivityPoint(rotation,player,ref distance,.36f);
   if(!fishing&&GolfAiming)FitActivityPoint(rotation,player+Vector3.up*1.8f,ref distance);
   activityDistance=snap?distance:Mathf.Lerp(activityDistance,distance,1-Mathf.Exp(-Time.unscaledDeltaTime/(distance>activityDistance?.1f:.45f)));
   // Lift the fishing composition above the bottom fight panel. Fit the feet
   // as well as the hook, so a visible head cannot mask an obscured angler.
   var composedFocus=activityFocus-(fishing?rotation*Vector3.up*(activityDistance*Mathf.Tan(cam.fieldOfView*.5f*Mathf.Deg2Rad)*.16f):Vector3.zero);
   var desired=composedFocus-rotation*Vector3.forward*activityDistance;
   var path=desired-composedFocus;
   if(Physics.SphereCast(composedFocus,.2f,path.normalized,out var hit,path.magnitude,1<<8,QueryTriggerInteraction.Ignore))desired=composedFocus+path.normalized*Mathf.Max(.5f,hit.distance-.15f);
   if(!fishing)PresentGolfShotCamera(ref desired,ref rotation);
   transform.SetPositionAndRotation(desired,rotation);target.HideHead(Vector3.Distance(desired,player+Vector3.up*1.57f)<.85f);
   activityActor=target;activityFishing=fishing;activityPreviousActor=player;
   UpdateGolfPreview();UpdateFishingAim();
  }
  void FitActivityPoint(Quaternion rotation,Vector3 point,ref float distance,float vertical=.48f){
   var offset=Quaternion.Inverse(rotation)*(point-activityFocus);float tangent=Mathf.Tan(cam.fieldOfView*.5f*Mathf.Deg2Rad);
   distance=Mathf.Max(distance,Mathf.Abs(offset.x)/(tangent*cam.aspect*.56f)-offset.z,Mathf.Abs(offset.y)/(tangent*vertical)-offset.z);
  }
 }
}
