using UnityEngine;

namespace WhatTheFish {
 public sealed partial class PlayerView {
  Transform stadiumFrame;Athlete stadiumAthlete;Vector3 stadiumFocus,stadiumPreviousPlayer,stadiumVelocity;
  bool stadiumProjection,previousOrthographic;float previousOrthographicSize,previousFieldOfView,stadiumDistance,stadiumAngle;
  BasketballHoop[] stadiumHoops;

  // Use the playable surface, never the seating bowl or island bounds.
  bool StadiumField(out Transform frame,out Bounds bounds,out bool football){
   frame=null;bounds=default;football=false;
   var app=AppRoot.Instance;
   if(mode!=2||!active||!target||target.inTransit||!app||SkySailWorld.Instance&&SkySailWorld.Instance.Travelling)return false;
   football=app.SelectedSport==SportId.Football;
   if(football){var ball=FootballBall.Instance;if(!ball||!ball.Pitch)return false;frame=ball.Pitch;bounds=ball.PitchBounds;}
   else if(app.SelectedSport==SportId.Basketball){var ball=BasketballBall.Active;if(!ball||!ball.transform.parent)return false;frame=ball.transform.parent;var half=BasketballBall.PlayerCourtLimits;bounds=new Bounds(Vector3.zero,new Vector3(half.x*2,0,half.y*2));}
   else return false;
   return true;
  }

  // Movement follows the current broadcast pan while look input still
  // controls shot/pass/guard aim. Retain the existing command/network format.
  Vector2 StadiumMovement(Vector2 input){
   if(!StadiumField(out _,out _,out _))return input;
   var forward=Vector3.ProjectOnPlane(transform.forward,Vector3.up).normalized;
   var right=Vector3.Cross(Vector3.up,forward);
   var world=right*input.x+forward*input.y;
   var relative=Quaternion.Euler(0,-yaw,0)*world;
   return new Vector2(relative.x,relative.z);
  }

  bool UpdateStadiumCamera(){
   if(!StadiumField(out var frame,out var field,out bool football))return false;
   if(!stadiumProjection){previousOrthographic=cam.orthographic;previousOrthographicSize=cam.orthographicSize;previousFieldOfView=cam.fieldOfView;stadiumProjection=true;}
   var player=frame.InverseTransformPoint(target.transform.position);
   player.y=field.max.y;
   bool snap=stadiumFrame!=frame||stadiumAthlete!=target||Vector3.Distance(player,stadiumPreviousPlayer)>(football?12:5);
   if(stadiumFrame!=frame)stadiumHoops=football?null:frame.GetComponentsInChildren<BasketballHoop>();
   float dt=Mathf.Max(.0001f,Time.deltaTime);
   var velocity=Vector3.ClampMagnitude((player-stadiumPreviousPlayer)/dt,football?10:7);
   stadiumVelocity=snap?Vector3.zero:Vector3.Lerp(stadiumVelocity,velocity,1-Mathf.Exp(-dt/.35f));
   var lead=stadiumVelocity*(football?.32f:.22f);
   // A nearby loose ball opens the composition without dragging an off-ball
   // player across the entire venue. Ignore dribble/jump height entirely.
   var ball=football?FootballBall.Instance.transform:BasketballBall.Active.transform;
   var ballDelta=frame.InverseTransformPoint(ball.position)-player;ballDelta.y=0;
   float influence=(1-Mathf.SmoothStep(0,1,ballDelta.magnitude/(football?24:9)))*.22f;
   var desired=player+lead+Vector3.ClampMagnitude(ballDelta*influence,football?3:1.2f);
   float end=Mathf.Clamp((desired.z-field.center.z)/field.extents.z,-1,1);
   // A sideline gantry pans toward each goal. Basketball swings farther into
   // the half-court; neither view rolls or rotates with the athlete's aim.
   float angle=90-(football?9:10)-end*(football?19:32);
   float tilt=football?40:36;
   float fov=football?37:42;
   float speed=Mathf.Clamp01(stadiumVelocity.magnitude/(football?9:6));
   float distance=(football?41.5f:11)*(1+speed*.055f-Mathf.Abs(end)*(football?.035f:.085f));
   if(stadiumHoops!=null)foreach(var hoop in stadiumHoops){
    if(!hoop)continue;
    float gap=Vector3.ProjectOnPlane(hoop.transform.position-target.transform.position,frame.up).magnitude;
    float basket=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(9,13,gap));
    float crossCourt=Mathf.Abs(frame.InverseTransformPoint(hoop.transform.position).x-player.x);
    distance=Mathf.Max(distance,Mathf.Lerp(11,13.75f+crossCourt*.45f,basket));
   }
   // Narrow windows pull back enough to retain passing lanes and both rims.
   distance*=Mathf.Sqrt((16f/9)/Mathf.Clamp(cam.aspect,1.1f,2.4f));
   stadiumAngle=snap?angle:Mathf.LerpAngle(stadiumAngle,angle,1-Mathf.Exp(-dt/.55f));
   stadiumDistance=snap?distance:Mathf.Lerp(stadiumDistance,distance,1-Mathf.Exp(-dt/.7f));
   var rotation=frame.rotation*Quaternion.Euler(tilt,stadiumAngle,0);
   float lookHeight=football?.35f:.8f;
   cam.orthographic=false;cam.fieldOfView=fov;cam.nearClipPlane=.15f;
   // Constrain the useful ground footprint to the authored playing surface.
   // Perspective makes this footprint asymmetric, unlike a top-down rectangle.
   GroundExtents(frame,rotation,stadiumDistance,lookHeight,out var min,out var max);
   desired.x=FieldFocus(desired.x,field.min.x,field.max.x,min.x,max.x,football?2:1);
   desired.z=FieldFocus(desired.z,field.min.z,field.max.z,min.z,max.z,football?3:1.5f);
   desired.y=field.max.y;
   var previousFocus=stadiumFocus;stadiumFocus=desired;
   SetStadiumPose(frame,rotation,lookHeight);
   float side=Mathf.Clamp((player.x-field.center.x)/field.extents.x,-1,1);
   float top=football?.76f:side<0?Mathf.Lerp(.66f,.29f,-side):Mathf.Lerp(.66f,.72f,side);
   var feetSafe=new Rect(.15f,.24f,.65f,top-.24f);
   // Let edge players leave centre, but keep their feet clear of the bottom
   // controls and their head inside the picture. This also permits exploration.
   KeepStadiumPoint(frame,rotation,lookHeight,frame.TransformPoint(player),feetSafe);
   KeepStadiumPoint(frame,rotation,lookHeight,frame.TransformPoint(player)+frame.up*1.75f,new Rect(.12f,.25f,.68f,.57f));
   if(stadiumHoops!=null){
    foreach(var hoop in stadiumHoops){
     if(!hoop)continue;
     var rim=hoop.transform.TransformPoint(0,BasketballHoop.RimHeight,0);
     float gap=Vector3.ProjectOnPlane(rim-target.transform.position,frame.up).magnitude;
     float weight=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(9,13,gap));
     if(weight>0)KeepStadiumPoint(frame,rotation,lookHeight,rim,new Rect(.12f,.2f,.68f,.6f),weight);
    }
   }
   // Compose the basket and athlete before smoothing. Apply only the player's
   // safety limits immediately, so approaching a rim never causes a camera cut.
   stadiumFocus=snap?stadiumFocus:Vector3.Lerp(previousFocus,stadiumFocus,1-Mathf.Exp(-dt/.28f));
   SetStadiumPose(frame,rotation,lookHeight);
   KeepStadiumPoint(frame,rotation,lookHeight,frame.TransformPoint(player),feetSafe);
   KeepStadiumPoint(frame,rotation,lookHeight,frame.TransformPoint(player)+frame.up*1.75f,new Rect(.12f,.25f,.68f,.57f));
   stadiumFrame=frame;stadiumAthlete=target;stadiumPreviousPlayer=player;
   target.HideHead(false);return true;
  }

  void SetStadiumPose(Transform frame,Quaternion rotation,float lookHeight){
   transform.SetPositionAndRotation(frame.TransformPoint(stadiumFocus)+frame.up*lookHeight-rotation*Vector3.forward*stadiumDistance,rotation);
  }
  void KeepStadiumPoint(Transform frame,Quaternion rotation,float lookHeight,Vector3 world,Rect safe,float weight=1){
   var screen=cam.WorldToViewportPoint(world);
   var clamped=new Vector2(Mathf.Clamp(screen.x,safe.xMin,safe.xMax),Mathf.Clamp(screen.y,safe.yMin,safe.yMax));
   if(Mathf.Abs(clamped.x-screen.x)+Mathf.Abs(clamped.y-screen.y)<.0001f)return;
   var ray=cam.ViewportPointToRay(clamped);var plane=new Plane(frame.up,world);
   if(plane.Raycast(ray,out float hit)){var correction=frame.InverseTransformVector(world-ray.GetPoint(hit));correction.y=0;stadiumFocus+=correction*weight;SetStadiumPose(frame,rotation,lookHeight);}
  }
  void GroundExtents(Transform frame,Quaternion rotation,float distance,float lookHeight,out Vector3 min,out Vector3 max){
   min=Vector3.one*float.PositiveInfinity;max=Vector3.one*float.NegativeInfinity;
   float tangent=Mathf.Tan(cam.fieldOfView*.5f*Mathf.Deg2Rad);
   var origin=frame.up*lookHeight-rotation*Vector3.forward*distance;
   for(int i=0;i<4;i++){
    var ray=rotation*new Vector3((i%2==0?-.88f:.88f)*tangent*cam.aspect,(i<2?-.84f:.68f)*tangent,1);
    var point=frame.InverseTransformVector(origin-ray*(Vector3.Dot(origin,frame.up)/Vector3.Dot(ray,frame.up)));
    min=Vector3.Min(min,point);max=Vector3.Max(max,point);
   }
  }
  static float FieldFocus(float focus,float min,float max,float near,float far,float apron){
   float low=min-near-apron,high=max-far+apron;
   float outside=focus-Mathf.Clamp(focus,min,max);
   // When the perspective footprint exceeds the court, anchor to the court
   // centre. Centring its asymmetric bounds would bias the camera into stands.
   float center=(min+max)*.5f;
   float travel=Mathf.SmoothStep(0,1,Mathf.Max(0,high-low)/Mathf.Max(.01f,(max-min)*.25f));
   return Mathf.Lerp(center,low<=high?Mathf.Clamp(focus,low,high):center,travel)+outside;
  }

  void ResetStadiumCamera(){
   if(stadiumProjection&&cam){cam.orthographic=previousOrthographic;cam.orthographicSize=previousOrthographicSize;cam.fieldOfView=previousFieldOfView;}
   stadiumProjection=false;stadiumFrame=null;stadiumAthlete=null;stadiumHoops=null;
  }
 }
}
