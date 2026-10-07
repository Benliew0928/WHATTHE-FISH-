using System.Collections.Generic;
using UnityEngine;

namespace WhatTheFish {
 // Deterministic geometry shared by the host and development checks. No client
 // supplies a victim, success flag, probability roll or outgoing velocity.
 public static class BasketballStealRules {
  public const float Windup=.14f,ContactEnd=.26f,Duration=.68f,Repeat=.74f,Reaction=.60f;
  public const float Reach=.95f,BodyRange=1.55f,PickupDelay=.34f,VictimDelay=.70f;
  public static Vector3 Shoulder(Vector3 root,Quaternion facing,Vector3 ball,bool left){
   float crouch=Mathf.Clamp01((.65f-(ball.y-root.y))/.45f);
   return root+facing*new Vector3(left?-.22f:.22f,.94f-.20f*crouch,.06f);
  }
  public static bool Shielded(Vector3 start,Vector3 ball,Vector3 body,float radius){
   var path=Vector3.ProjectOnPlane(ball-start,Vector3.up);float length=path.sqrMagnitude;
   float t=length>.0001f?Mathf.Clamp01(Vector3.Dot(Vector3.ProjectOnPlane(body-start,Vector3.up),path)/length):0;
   var point=Vector3.Lerp(start,ball,t)-body;
   return point.y>.22f&&point.y<1.30f&&new Vector2(point.x,point.z).sqrMagnitude<radius*radius;
  }
  public static bool Contact(Vector3 attacker,Quaternion facing,Vector3 victim,Vector3 ball,bool left,float phase,bool secured,out float quality){
   quality=0;
   if(!Finite(attacker)||!Finite(victim)||!Finite(ball)||!float.IsFinite(phase))return false;
   if(Vector3.ProjectOnPlane(victim-attacker,Vector3.up).sqrMagnitude>BodyRange*BodyRange||Mathf.Abs(victim.y-attacker.y)>.65f)return false;
   var delta=Vector3.ProjectOnPlane(ball-attacker,Vector3.up);
   float alignment=delta.sqrMagnitude>.001f?Vector3.Dot(facing*Vector3.forward,delta.normalized):1;
   var shoulder=Shoulder(attacker,facing,ball,left);float distance=Vector3.Distance(shoulder,ball);
   if(alignment<.5f||distance>Reach||Shielded(shoulder,ball,victim,.255f))return false;
   float reach=Mathf.InverseLerp(Reach,.40f,distance);
   float exposure=secured?.05f:Mathf.Pow(Mathf.Sin(Mathf.Repeat(phase,1)*Mathf.PI),2);
   quality=.45f*reach+.25f*Mathf.InverseLerp(.5f,.98f,alignment)+.30f*exposure;
   return quality>=(secured?.64f:.60f);
  }
  public static Vector3 Deflection(Vector3 attacker,Vector3 ball,Vector3 carrierVelocity,Vector3 defenderVelocity,float phase,float cadence,bool secured,bool left,float quality,float height){
   Vector3 outward=Vector3.ProjectOnPlane(ball-attacker,Vector3.up).normalized;
   if(outward.sqrMagnitude<.01f)outward=Vector3.forward;
   Vector3 across=Vector3.Cross(Vector3.up,outward)*(left?1:-1);
   var swipe=(outward*.85f+across*.50f).normalized;
   float closing=Mathf.Clamp(Vector3.Dot(defenderVelocity-carrierVelocity,outward),0,5);
   var velocity=Vector3.ProjectOnPlane(carrierVelocity*.60f+defenderVelocity*.25f,Vector3.up)+swipe*(2.6f+quality*1.2f+closing*.20f);
   float bounce=secured?0:(BasketballMotion.DribbleOffset(phase+.01f).y-BasketballMotion.DribbleOffset(phase-.01f).y)*cadence/.02f;
   velocity=Vector3.ClampMagnitude(velocity,7.5f);
   velocity.y=Mathf.Clamp(.45f+Mathf.Clamp01(height)*1.15f+bounce*.28f,-.85f,2.5f);
   return velocity;
  }
  static bool Finite(Vector3 p)=>float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z);
 }

 public sealed partial class BasketballBall {
  struct StealAttempt {public Athlete actor,victim;public uint sequence,possession;}
  readonly List<StealAttempt> stealAttempts=new();
  Collider ignoredStealer;uint steals;double stealTime=-100;
  public uint StealCount=>Authority?steals:haveTarget?target.steals:0;
  public double StealTime=>Authority?stealTime:haveTarget?target.stealTime:-100;
  public bool CanSteal(Athlete actor)=>Held&&Role(actor)==BasketballRole.Defense&&Playing&&actor&&!actor.inTransit&&!actor.BasketballFreeRoam&&Holder!=actor&&!actor.Airborne&&!actor.LoadingJump;
  public bool TrySteal(Athlete actor,float heading){
   if(!Authority||!CanSteal(actor)||!Eligible(actor)||!float.IsFinite(heading)||!actor.Grounded||Outside(actor.transform.position)||!actor.BasketballMotion||actor.BasketballMotion.Busy)return false;
   heading%=360;var facing=Quaternion.Euler(0,heading,0);
   var point=holder?CarryPosition(holder):actor.transform.position+facing*new Vector3(0,.65f,.75f);
   if(holder&&!holder.Airborne&&!holder.LoadingJump&&!holder.BasketballMotion.CarryingPose){
    var bounce=BasketballMotion.DribbleOffset(DribblePhase+dribbleCadence*.20f);bounce.y=Mathf.Max(Radius+.004f,bounce.y);
    point=holder.transform.TransformPoint(bounce);
   }
   // Commit a direction and hand at the beginning; the victim can move out of
   // that cone during the windup. An empty swipe still spends its recovery.
   actor.BasketballMotion.BeginChallenge(BasketballAction.Steal,heading,point);
   stealAttempts.Add(new StealAttempt{actor=actor,victim=holder,sequence=actor.BasketballMotion.State.sequence,possession=reset});
   return true;
  }
  void StepSteals(){
   for(int i=stealAttempts.Count-1;i>=0;i--){
    var attempt=stealAttempts[i];var actor=attempt.actor;var motion=actor?actor.BasketballMotion:null;
    if(!motion||motion.State.sequence!=attempt.sequence||motion.Action!=BasketballAction.Steal||!CanSteal(actor)||!Eligible(actor)||!actor.Grounded||attempt.possession!=reset||!attempt.victim||holder!=attempt.victim||!Eligible(holder)) {stealAttempts.RemoveAt(i);continue;}
    if(motion.Elapsed<BasketballStealRules.Windup)continue;
    if(motion.Elapsed>BasketballStealRules.ContactEnd){stealAttempts.RemoveAt(i);continue;}
    // A pass/shot that has already reached its release key beats a late swipe.
    if(pendingAction!=BasketballAction.None&&holder.BasketballMotion.BeforeRelease==false&&holder.BasketballMotion.Busy)continue;
    var point=CarryPosition(holder);bool secured=holder.Airborne||holder.LoadingJump||holder.BasketballMotion.CarryingPose;
    if(!BasketballStealRules.Contact(actor.transform.position,motion.Facing,holder.transform.position,point,motion.State.leftHand,DribblePhase,secured,out float quality))continue;
    var shoulder=BasketballStealRules.Shoulder(actor.transform.position,motion.Facing,point,motion.State.leftHand);
    if(Vector3.Dot(actor.transform.forward,(point-actor.transform.position).normalized)<.35f||Physics.Linecast(shoulder,point,1<<8,QueryTriggerInteraction.Ignore)||Physics.CheckSphere(point,Radius+.005f,1<<8,QueryTriggerInteraction.Ignore))continue;
    bool blocked=false;
    foreach(var other in Athlete.Active)if(other!=actor&&other!=holder&&other.capsule&&other.capsule.enabled&&BasketballStealRules.Shielded(shoulder,point,other.transform.position,other.capsule.radius)){blocked=true;break;}
    if(blocked)continue;
    KnockLoose(actor,point,quality,secured);stealAttempts.RemoveAt(i);
   }
  }
  void KnockLoose(Athlete actor,Vector3 point,float quality,bool secured){
   var victim=holder;var motion=actor.BasketballMotion;
   var velocity=BasketballStealRules.Deflection(actor.transform.position,point,victim.Motor.Velocity,actor.Motor.Velocity,DribblePhase,dribbleCadence,secured,motion.State.leftHand,quality,point.y-victim.transform.position.y);
   NewDefensePlay();CancelShotCharge(victim);pendingAction=BasketballAction.None;ResetShotTracking();passAim=default;passing=false;
   motion.ContactPoint(point);
   victim.BasketballMotion.BeginChallenge(BasketballAction.Stripped,victim.transform.eulerAngles.y,point);
   holder=null;lastShooter=victim;pickupAt=Time.time+BasketballStealRules.PickupDelay;shooterPickupAt=Time.time+BasketballStealRules.VictimDelay;looseFor=outsideFor=0;
   RestoreShooterCollision();RestoreFloorContacts();Body.position=point;transform.position=point;
   Body.isKinematic=false;Body.detectCollisions=true;Body.linearDamping=restDamping;
   // Normal dribbles start outside both capsules: leave their collisions on,
   // so a swipe into the carrier's legs/body ricochets instead of tunnelling
   // through them. A tight gathered grip can overlap; reflect out of that
   // body and ignore only the initial overlap until the sphere separates.
   bool Overlaps(Collider body)=>body&&(body.ClosestPoint(point)-point).sqrMagnitude<Radius*Radius;
   ignoredShooter=Overlaps(victim.capsule)?victim.capsule:null;
   ignoredStealer=Overlaps(actor.capsule)?actor.capsule:null;
   if(ignoredShooter){var normal=Vector3.ProjectOnPlane(point-victim.transform.position,Vector3.up).normalized;float into=Vector3.Dot(velocity,normal);if(into<0)velocity-=normal*into*1.65f;}
   if(ignoredStealer){var normal=Vector3.ProjectOnPlane(point-actor.transform.position,Vector3.up).normalized;float into=Vector3.Dot(velocity,normal);if(into<0)velocity-=normal*into*1.65f;}
   var sphere=GetComponent<SphereCollider>();if(ignoredShooter)Physics.IgnoreCollision(sphere,ignoredShooter,true);if(ignoredStealer)Physics.IgnoreCollision(sphere,ignoredStealer,true);
   Body.linearVelocity=velocity;Body.angularVelocity=Vector3.ClampMagnitude(Vector3.Cross(Vector3.up,velocity)/Radius*.4f+Vector3.up*(motion.State.leftHand?3:-3),24);
   Body.WakeUp();steals++;stealTime=BasketballMotion.Clock;reset++;sendAt=0;
  }
  void ClearSteals(){foreach(var attempt in stealAttempts)if(attempt.actor&&attempt.actor.BasketballMotion)attempt.actor.BasketballMotion.ResetPose();stealAttempts.Clear();RestoreStealerCollision();}
  void RestoreStealerCollision(){if(ignoredStealer){Physics.IgnoreCollision(GetComponent<SphereCollider>(),ignoredStealer,false);ignoredStealer=null;}}
 }
}
