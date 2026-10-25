using UnityEngine;

namespace WhatTheFish {
 public sealed partial class PlayerView {
  Transform stadiumFrame,stadiumBall;Athlete stadiumAthlete;
  Vector3 stadiumFocus,stadiumPreviousBall,stadiumVelocity;
  bool stadiumProjection,previousOrthographic;float previousOrthographicSize,previousFieldOfView,stadiumDistance,stadiumAngle;
  BasketballHoop[] stadiumHoops;
  public bool StadiumFollowing=>stadiumProjection&&isActiveAndEnabled&&StadiumField(out _,out _,out _);

  bool StadiumField(out Transform frame,out Bounds bounds,out bool football){
   frame=null;bounds=default;football=false;var app=AppRoot.Instance;
   if(mode!=2||!active||!target||!target.isActiveAndEnabled||target.IsSpectator||target.inTransit||!app||SkySailWorld.Instance&&SkySailWorld.Instance.Travelling)return false;
   football=app.SelectedSport==SportId.Football;
   if(football){var ball=FootballBall.Instance;if(!ball||!ball.Pitch)return false;frame=ball.Pitch;bounds=ball.PitchBounds;}
   else if(app.SelectedSport==SportId.Basketball){var ball=BasketballBall.Active;if(!ball||!ball.transform.parent)return false;frame=ball.transform.parent;var half=BasketballBall.PlayerCourtLimits;bounds=new Bounds(Vector3.zero,new Vector3(half.x*2,0,half.y*2));}
   else return false;
   return true;
  }

  // Screen-relative movement remains independent of shot/pass aim.
  Vector2 StadiumMovement(Vector2 input){
   if(!StadiumField(out _,out _,out _))return input;
   var forward=Vector3.ProjectOnPlane(PropMovementForward,Vector3.up).normalized;
   var right=Vector3.Cross(Vector3.up,forward);
   var relative=Quaternion.Euler(0,-yaw,0)*(right*input.x+forward*input.y);
   return new Vector2(relative.x,relative.z);
  }

  // Invoked after ball presentation, including guest interpolation and held
  // animation. PlayerView retains its early update order for input sampling.
  internal void PresentStadiumCamera(){
   if(FootballReplay.Presenting)return;
   if(!isActiveAndEnabled||!StadiumField(out var frame,out var field,out bool football)){ResetStadiumCamera();return;}
   var ball=football?FootballBall.Instance.transform:BasketballBall.Active.transform;
   var point=frame.InverseTransformPoint(ball.position);
   var player=frame.InverseTransformPoint(target.transform.position);player.y=field.max.y;
   if(!FiniteCameraPoint(point)||!FiniteCameraPoint(player))return;
   if(!stadiumProjection){previousOrthographic=cam.orthographic;previousOrthographicSize=cam.orthographicSize;previousFieldOfView=cam.fieldOfView;stadiumProjection=true;}
   bool snap=stadiumFrame!=frame||stadiumAthlete!=target||stadiumBall!=ball;
   if(stadiumFrame!=frame)stadiumHoops=football?null:frame.GetComponentsInChildren<BasketballHoop>();
   float dt=Mathf.Max(.0001f,Time.deltaTime);
   bool held=football?FootballBall.Instance.CurrentController!=null:BasketballBall.Active.Held;
   // Presented positions work on guests as well as the host. Never read a
   // guest's kinematic Rigidbody velocity. Discontinuities are not shot leads.
   var delta=point-stadiumPreviousBall;
   bool relocated=snap||delta.magnitude>Mathf.Max(football?8:4,45*dt);
   var velocity=relocated?Vector3.zero:Vector3.ClampMagnitude(delta/dt,45);
   if(held)velocity.y=0;
   stadiumVelocity=relocated?Vector3.zero:Vector3.Lerp(stadiumVelocity,velocity,1-Mathf.Exp(-dt/.12f));
   float height=Mathf.Max(0,point.y-field.max.y);
   bool flight=!held&&height>(football?.65f:1.8f);
   var ground=point;ground.y=field.max.y;
   var lead=Vector3.ClampMagnitude(Vector3.ProjectOnPlane(stadiumVelocity,Vector3.up)*(flight?.32f:.18f),football?6:2.5f);
   var desired=ground+lead;
   var playerDelta=player-ground;
   float nearby=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(football?10:4,football?28:12,playerDelta.magnitude));
   desired+=Vector3.ClampMagnitude(playerDelta,football?10:5)*nearby*(flight?.08f:.22f);
   desired.x=Mathf.Clamp(desired.x,field.min.x-2,field.max.x+2);
   desired.z=Mathf.Clamp(desired.z,field.min.z-3,field.max.z+3);
   // Ignore dribble height but retain ground context beneath airborne shots.
   desired.y=field.max.y+(football?.6f:1.15f)+(flight?Mathf.Max(0,height-1.5f)*.42f:0);
   float end=Mathf.Clamp((desired.z-field.center.z)/field.extents.z,-1,1);
   float angle=80-end*(football?19:30);
   stadiumAngle=snap?angle:Mathf.LerpAngle(stadiumAngle,angle,1-Mathf.Exp(-dt/.65f));
   var rotation=frame.rotation*Quaternion.Euler(football?40:38,stadiumAngle,0);
   cam.orthographic=false;cam.fieldOfView=football?37:42;cam.nearClipPlane=.15f;
   stadiumFocus=snap?desired:Vector3.Lerp(stadiumFocus,desired,1-Mathf.Exp(-dt/(relocated?.12f:flight?.18f:.32f)));
   float baseline=(football?42:15)*Mathf.Sqrt((16f/9)/Mathf.Clamp(cam.aspect,.65f,2.6f));
   float distance=baseline;
   FitStadiumPoint(frame,rotation,point,ref distance);
   FitStadiumPoint(frame,rotation,ground,ref distance);
   // Short flight forecast opens space before an apex or pass arrives. This is
   // composition only; collisions, steals and catches remain authoritative.
   if(flight){
    float ahead=.55f;var future=point+stadiumVelocity*ahead+frame.InverseTransformVector(Physics.gravity)*(.5f*ahead*ahead);
    future.y=Mathf.Max(field.max.y+.2f,future.y);FitStadiumPoint(frame,rotation,future,ref distance);
   }
   if(nearby>.25f){FitStadiumPoint(frame,rotation,player,ref distance);FitStadiumPoint(frame,rotation,player+Vector3.up*1.8f,ref distance);}
   if(stadiumHoops!=null)foreach(var hoop in stadiumHoops){
    if(!hoop)continue;var rim=frame.InverseTransformPoint(hoop.transform.TransformPoint(0,BasketballHoop.RimHeight,0));
    if(Vector3.ProjectOnPlane(rim-ground,Vector3.up).magnitude<12)FitStadiumPoint(frame,rotation,rim,ref distance);
   }
   distance=Mathf.Clamp(distance,baseline,baseline*(football?1.55f:1.85f));
   stadiumDistance=snap?distance:Mathf.Lerp(stadiumDistance,distance,1-Mathf.Exp(-dt/(distance>stadiumDistance?.22f:.85f)));
   SetStadiumPose(frame,rotation);
   // A broad dead zone allows off-centre action. Only the ball has a hard
   // visibility guard; a distant athlete uses the edge indicator instead.
   KeepStadiumBall(frame,rotation,ball.position,new Rect(.12f,.25f,.71f,.53f));
   stadiumFrame=frame;stadiumAthlete=target;stadiumBall=ball;stadiumPreviousBall=point;
   target.HideHead(false);UpdateKickAim();
  }
  static bool FiniteCameraPoint(Vector3 p)=>float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z);
  void FitStadiumPoint(Transform frame,Quaternion rotation,Vector3 point,ref float distance){
   var offset=Quaternion.Inverse(rotation)*frame.TransformVector(point-stadiumFocus);
   float tangent=Mathf.Tan(cam.fieldOfView*.5f*Mathf.Deg2Rad);
   distance=Mathf.Max(distance,Mathf.Max(Mathf.Abs(offset.x)/(tangent*cam.aspect*.62f)-offset.z,Mathf.Abs(offset.y)/(tangent*.48f)-offset.z));
  }
  void SetStadiumPose(Transform frame,Quaternion rotation)=>transform.SetPositionAndRotation(frame.TransformPoint(stadiumFocus)-rotation*Vector3.forward*stadiumDistance,rotation);
  void KeepStadiumBall(Transform frame,Quaternion rotation,Vector3 world,Rect safe){
   var screen=cam.WorldToViewportPoint(world);
   if(screen.z<=cam.nearClipPlane){stadiumFocus=frame.InverseTransformPoint(world);SetStadiumPose(frame,rotation);return;}
   var clamped=new Vector3(Mathf.Clamp(screen.x,safe.xMin,safe.xMax),Mathf.Clamp(screen.y,safe.yMin,safe.yMax),screen.z);
   // Image-plane translation also handles high lobs above the horizon.
   stadiumFocus+=frame.InverseTransformVector(world-cam.ViewportToWorldPoint(clamped));SetStadiumPose(frame,rotation);
  }
  void ResetStadiumCamera(){
   if(stadiumProjection&&cam){cam.orthographic=previousOrthographic;cam.orthographicSize=previousOrthographicSize;cam.fieldOfView=previousFieldOfView;}
   stadiumProjection=false;stadiumFrame=stadiumBall=null;stadiumAthlete=null;stadiumHoops=null;stadiumVelocity=Vector3.zero;
  }
 }
}
