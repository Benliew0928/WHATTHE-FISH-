using UnityEngine;

namespace WhatTheFish {
 public static class SpectatorMovement {
  public static bool Court(out Transform frame,out Bounds bounds){
   frame=null;bounds=default;var app=AppRoot.Instance;if(!app)return false;
   if(app.SelectedSport==SportId.Football&&FootballBall.Instance&&FootballBall.Instance.Pitch){frame=FootballBall.Instance.Pitch;bounds=FootballBall.Instance.PitchBounds;}
   else if(app.SelectedSport==SportId.Basketball&&BasketballBall.Active){frame=BasketballBall.Active.transform.parent;var half=BasketballBall.PlayerCourtLimits;bounds=new Bounds(Vector3.zero,new Vector3(half.x*2,0,half.y*2));}
   else return false;
   return frame;
  }
  // Sweep against the expanded playing rectangle, then slide along its outside.
  // Height is deliberately ignored: steps, ramps and falling cannot bypass it.
  public static Vector3 Constrain(Vector3 position,Vector3 motion,Bounds bounds,float margin){
   bounds.Expand(new Vector3(margin*2,0,margin*2));bounds.center=new Vector3(bounds.center.x,0,bounds.center.z);bounds.size=new Vector3(bounds.size.x,10000,bounds.size.z);
   float y=motion.y;position.y=0;motion.y=0;
   if(bounds.Contains(position)){
    float[] d={Mathf.Abs(position.x-bounds.min.x),Mathf.Abs(bounds.max.x-position.x),Mathf.Abs(position.z-bounds.min.z),Mathf.Abs(bounds.max.z-position.z)};
    int side=0;for(int i=1;i<4;i++)if(d[i]<d[side])side=i;
    var corrected=position;if(side==0)corrected.x=bounds.min.x-.01f;else if(side==1)corrected.x=bounds.max.x+.01f;else if(side==2)corrected.z=bounds.min.z-.01f;else corrected.z=bounds.max.z+.01f;
    motion=corrected-position;
   }else if(motion.sqrMagnitude>.000001f&&bounds.IntersectRay(new Ray(position,motion.normalized),out float distance)&&distance<=motion.magnitude){
    Vector3 contact=position+motion.normalized*Mathf.Max(0,distance-.003f);Vector3 remainder=motion-(contact-position);
    if(Mathf.Abs(contact.x-bounds.min.x)<.01f||Mathf.Abs(contact.x-bounds.max.x)<.01f)remainder.x=0;else remainder.z=0;
    motion=contact-position+remainder;
   }
   motion.y=y;return motion;
  }
  public static void Simulate(Athlete actor,PlayerCommand command,float dt){
   if(!actor.capsule.enabled)return;
   actor.capsule.excludeLayers=~(1<<8); // Ground/scenery only; no ball or participant collisions.
   int steps=Mathf.Max(1,Mathf.CeilToInt(Mathf.Min(dt,.25f)*60));float step=Mathf.Min(dt,.25f)/steps;
   for(int i=0;i<steps;i++){
    Vector3 direction=Quaternion.Euler(0,command.heading,0)*new Vector3(command.move.x,0,command.move.y);
    Vector3 movement=actor.Motor.Step(direction,command.sprint?7:4,actor.Grounded,step);
    if(Court(out var frame,out var bounds))movement=frame.TransformVector(Constrain(frame.InverseTransformPoint(actor.transform.position),frame.InverseTransformVector(movement),bounds,actor.capsule.radius+.6f));
    Vector3 before=actor.transform.position;actor.transform.rotation=Quaternion.Euler(0,actor.Motor.Yaw,0);
    actor.capsule.Move(movement+Vector3.up*actor.Jump.Step(actor.Grounded,false,step));actor.speed=Vector3.ProjectOnPlane(actor.transform.position-before,Vector3.up).magnitude/step;
   }
   if(actor.transform.position.y<-3){
    var net=actor.GetComponent<NetworkAthlete>();var landing=RoomSeats.SpectatorLanding(AppRoot.Instance.SelectedSport,net?net.RoomSeat.Value%10:0);
    if(Physics.Raycast(landing.position+Vector3.up*4,Vector3.down,out var floor,12,1<<8,QueryTriggerInteraction.Ignore)){
     actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(floor.point+Vector3.up*.035f,landing.rotation);actor.ResetLocomotion();actor.capsule.enabled=true;
    }
   }
  }
 }
}
