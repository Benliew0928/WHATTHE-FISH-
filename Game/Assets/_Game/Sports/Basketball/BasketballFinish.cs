using System;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public enum BasketballFinish:byte { Shot,Layup,Dunk }
 public enum BasketballFinishReason:byte { Ready,Possession,Grounded,Closer,TooClose,Front,Drive,Lane,Recovery,Invalid }
 public struct BasketballFinishNotice:INetworkSerializable,IEquatable<BasketballFinishNotice> {
  public BasketballFinishReason reason;public ulong player;public double time;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref reason);s.SerializeValue(ref player);s.SerializeValue(ref time);}
  public bool Equals(BasketballFinishNotice b)=>reason==b.reason&&player==b.player&&time==b.time;
 }
 public static class BasketballFinishRules {
  public const float GatherGrace=.20f,MaximumDistance=4f,StandingLayupDistance=2f,LayupMakeChance=.85f;
  public static bool IsFinish(BasketballAction a)=>a==BasketballAction.Layup||a==BasketballAction.Dunk;
  public static float Takeoff(BasketballAction action)=>action==BasketballAction.Dunk?.26f:.24f;
  public static float Release(BasketballAction action)=>action==BasketballAction.Dunk?.76f:.60f;
  public static float Duration(BasketballAction action)=>action==BasketballAction.Dunk?1.55f:1.30f;
  public static Vector3 ReleaseOffset(BasketballAction action,bool left)=>new((left?-1:1)*(action==BasketballAction.Dunk?.12f:.20f),action==BasketballAction.Dunk?1.03f:1.11f,action==BasketballAction.Dunk?.58f:.44f);
  public static string Hint(BasketballFinishReason reason)=>reason switch {
   BasketballFinishReason.Possession=>"Get the ball first",BasketballFinishReason.Grounded=>"Land before finishing",
   BasketballFinishReason.Closer=>"Move closer to the basket",BasketballFinishReason.TooClose=>"Step back from under the rim",
   BasketballFinishReason.Front=>"Approach the front of the basket",BasketballFinishReason.Drive=>"Drive toward the basket",
   BasketballFinishReason.Lane=>"Finishing path is blocked",BasketballFinishReason.Recovery=>"Finish landing first",
   BasketballFinishReason.Invalid=>"Choose a valid finish",_=>"Release in green"
  };
  public static string ShortHint(BasketballFinishReason reason)=>reason switch {
   BasketballFinishReason.Ready=>"READY",BasketballFinishReason.Closer=>"Move closer",BasketballFinishReason.TooClose=>"Step back",
   BasketballFinishReason.Drive=>"Keep moving",BasketballFinishReason.Front=>"Face the rim",BasketballFinishReason.Lane=>"Path blocked",
   BasketballFinishReason.Grounded=>"Land first",BasketballFinishReason.Recovery=>"Recover first",_=>"Unavailable"
  };
  // All measurements come from the host's current body and velocity. Camera
  // direction and an input request cannot manufacture a run-up or extra reach.
  public static BasketballFinishReason Evaluate(BasketballFinish kind,Vector3 hoopLocal,Vector3 toward,Vector3 forward,Vector3 velocity,bool grounded){
   if(kind!=BasketballFinish.Layup&&kind!=BasketballFinish.Dunk)return BasketballFinishReason.Invalid;
   if(!grounded)return BasketballFinishReason.Grounded;
   float distance=new Vector2(hoopLocal.x,hoopLocal.z).magnitude;
   if(distance>MaximumDistance)return BasketballFinishReason.Closer;
   if(hoopLocal.z<0)return BasketballFinishReason.Front;
   if(distance<(kind==BasketballFinish.Dunk?1.05f:1.25f))return BasketballFinishReason.TooClose;
   if(hoopLocal.z<.45f||hoopLocal.z/distance<.35f||Vector3.Dot(forward,toward)<.6f)return BasketballFinishReason.Front;
   float speed=Vector3.ProjectOnPlane(velocity,Vector3.up).magnitude;
   if((kind==BasketballFinish.Dunk||distance>StandingLayupDistance)&&(speed<(kind==BasketballFinish.Dunk?1.6f:1.0f)||Vector3.Dot(velocity.normalized,toward)<.68f))return BasketballFinishReason.Drive;
   return BasketballFinishReason.Ready;
  }
 }

 public sealed partial class BasketballBall {
  BasketballFinishNotice finishNotice;
  Athlete approachActor;Vector3 approachVelocity,approachPosition;double approachTime;
  Vector3 acceptedApproachVelocity;bool finishMake;
  public bool GatheringFinish(Athlete actor)=>Authority&&holder==actor&&BasketballFinishRules.IsFinish(pendingAction)&&!actor.BasketballMotion.Finishing;
  // A real approach may become a planted gather just before finger release.
  // Record host-observed capsule displacement, never a client-supplied speed.
  public void RecordFinishApproach(Athlete actor,Vector3 velocity){
   if(!Authority||!actor||holder!=actor)return;
   if(actor.Airborne||actor.LoadingJump||!actor.Grounded||actor.BasketballFreeRoam||actor.BasketballMotion.Busy){ForgetFinishApproach(actor);return;}
   velocity.y=0;
   if(velocity.sqrMagnitude>=1&&Vector3.Dot(velocity.normalized,actor.transform.forward)>=.68f){
    // Preserve the stronger recent run-up while braking into the plant. A
    // slower sample must not erase it or extend its original expiry time.
    if(approachActor!=actor||BasketballMotion.Clock-approachTime>BasketballFinishRules.GatherGrace||velocity.sqrMagnitude>=approachVelocity.sqrMagnitude-.01f){approachActor=actor;approachVelocity=velocity;approachPosition=actor.transform.position;approachTime=BasketballMotion.Clock;}
   }
   else if(velocity.sqrMagnitude>.04f&&Vector3.Dot(velocity.normalized,actor.transform.forward)<.68f)ForgetFinishApproach(actor);
  }
  public void ForgetFinishApproach(Athlete actor){if(approachActor==actor)approachActor=null;}
  Vector3 FinishApproachVelocity(Athlete actor,Vector3 toward,bool queued){
   if(Authority&&queued)return acceptedApproachVelocity;
   var velocity=Authority?actor.Motor.Velocity:actor.transform.forward*actor.speed;
   if(Authority&&approachActor==actor&&BasketballMotion.Clock-approachTime<=BasketballFinishRules.GatherGrace&&Vector3.Distance(actor.transform.position,approachPosition)<.35f&&(velocity.sqrMagnitude<.04f||Vector3.Dot(velocity.normalized,toward)>=.68f)&&approachVelocity.sqrMagnitude>velocity.sqrMagnitude)return approachVelocity;
   return velocity;
  }
  public BasketballFinishNotice FinishNotice=>Authority?finishNotice:haveTarget?target.finish:default;
  public bool FinishNoticeFor(Athlete actor)=>actor&&FinishNotice.reason!=BasketballFinishReason.Ready&&FinishNotice.player==PlayerId(actor)&&BasketballMotion.Clock-FinishNotice.time<2;
  public BasketballFinishReason FinishAvailability(Athlete actor,BasketballFinish kind,Transform hoop=null,bool checkPath=false,bool queued=false){
   if(!actor||!Held||Holder!=actor||!Playing||actor.BasketballFreeRoam||actor.inTransit)return BasketballFinishReason.Possession;
   if(ActionQueued&&!queued||actor.BasketballMotion&&actor.BasketballMotion.Busy)return BasketballFinishReason.Recovery;
   if(!hoop)hoop=ChargeHoop(actor,actor.transform.eulerAngles.y);
   if(!hoop)return BasketballFinishReason.Invalid;
   var toward=Vector3.ProjectOnPlane(hoop.position-actor.transform.position,Vector3.up).normalized;
   // Remote motor velocity is not simulated. Use its replicated speed and
   // presented heading for the advisory HUD; the host always uses actual velocity.
   var velocity=FinishApproachVelocity(actor,toward,queued);
   var reason=BasketballFinishRules.Evaluate(kind,hoop.InverseTransformPoint(actor.transform.position),toward,actor.transform.forward,velocity,actor.Grounded&&!actor.Airborne&&!actor.LoadingJump);
   if(reason==BasketballFinishReason.Ready&&checkPath){var plan=FinishPlan(actor,hoop,kind);if(!FinishPathClear(actor,plan))reason=BasketballFinishReason.Lane;}
   return reason;
  }
  BasketballMotionState FinishPlan(Athlete actor,Transform hoop,BasketballFinish kind){
   var action=kind==BasketballFinish.Dunk?BasketballAction.Dunk:BasketballAction.Layup;
   var origin=actor.transform.position;var toward=Vector3.ProjectOnPlane(hoop.position-origin,Vector3.up).normalized;
   var facing=Quaternion.LookRotation(toward);bool left=hoop.InverseTransformPoint(origin).x<-.15f;
   float distance=Vector3.ProjectOnPlane(hoop.position-origin,Vector3.up).magnitude;
   var offset=BasketballFinishRules.ReleaseOffset(action,left);
   float releaseDistance=kind==BasketballFinish.Dunk?0:Mathf.Clamp(distance-.45f,.85f,1.15f);
   var destination=hoop.position-toward*releaseDistance-facing*new Vector3(offset.x,0,offset.z);destination.y=origin.y;
   float lift=kind==BasketballFinish.Dunk?hoop.position.y+BasketballHoop.RimHeight+Radius+.12f-origin.y-offset.y:1.08f;
   float launch=kind==BasketballFinish.Dunk?Mathf.Sqrt(2*JumpMotor.Gravity*(lift+.30f)):Mathf.Sqrt(2*JumpMotor.Gravity*lift);
   return new BasketballMotionState{action=action,leftHand=left,heading=facing.eulerAngles.y,finishOrigin=origin,finishTarget=destination,finishJump=launch,gather=Quaternion.Inverse(facing)*(CarryPosition(actor)-origin)};
  }
  bool FinishPathClear(Athlete actor,BasketballMotionState plan){
   var controller=actor.capsule;float radius=controller.radius+.025f;float half=Mathf.Max(0,controller.height*.5f-radius);
   Vector3 previous=plan.finishOrigin,previousBall=CarryPosition(actor);var facing=Quaternion.Euler(0,plan.heading,0);
   // Swept capsules cover the entire planned jump, not just the line to the rim.
   for(int i=1;i<=24;i++){
    float t=(BasketballFinishRules.Duration(plan.action)-.22f)*i/24;
    var point=BasketballMotion.FinishRoot(plan,t);point.y+=BasketballMotion.FinishHeight(plan,t);
    var delta=point-previous;var center=previous+controller.center+Vector3.up*.06f;
    if(delta.sqrMagnitude>.000001f&&Physics.CapsuleCast(center+Vector3.up*half,center-Vector3.up*half,radius,delta.normalized,delta.magnitude,1<<8,QueryTriggerInteraction.Ignore))return false;
    foreach(var other in Athlete.Active)if(other!=actor&&other.gameObject.activeInHierarchy&&!other.inTransit&&other.capsule){
     var separation=point-other.transform.position;
     if(Mathf.Abs(separation.y)<controller.height&&Vector3.ProjectOnPlane(separation,Vector3.up).magnitude<radius+other.capsule.radius+.06f)return false;
    }
    if(t<=BasketballFinishRules.Release(plan.action)){
     var ballPoint=point+facing*BasketballMotion.FinishBall(plan,t);var ballDelta=ballPoint-previousBall;
     if(ballDelta.sqrMagnitude>.000001f&&Physics.SphereCast(previousBall,Radius+.005f,ballDelta.normalized,out _,ballDelta.magnitude,1<<8,QueryTriggerInteraction.Ignore))return false;
     previousBall=ballPoint;
    }
    previous=point;
   }
   return Physics.Raycast(plan.finishTarget+Vector3.up*.2f,Vector3.down,.45f,1<<8,QueryTriggerInteraction.Ignore);
  }
  void RejectFinish(Athlete actor,BasketballFinishReason reason){finishNotice=new BasketballFinishNotice{reason=reason,player=PlayerId(actor),time=BasketballMotion.Clock};CancelShotCharge(actor);sendAt=0;}
  bool FallbackShot(Athlete actor,BasketballFinishReason reason,Transform hoop,float power){
   // Releasing Shoot remains a shot. Invalid positioning cannot grant a finish
   // window or bypass the normal shot's power, release-clearance and scoring.
   finishNotice=new BasketballFinishNotice{reason=reason,player=PlayerId(actor),time=BasketballMotion.Clock};sendAt=0;
   if(reason!=BasketballFinishReason.Invalid&&hoop&&TryShoot(actor,Quaternion.LookRotation(Vector3.ProjectOnPlane(hoop.position-actor.transform.position,Vector3.up)).eulerAngles.y,power)){pendingShotHoop=hoop;return true;}
   CancelShotCharge(actor);return false;
  }
  bool TryFinish(Athlete actor,BasketballFinish kind,Transform hoop,float power){
   var reason=FinishAvailability(actor,kind,hoop,true);
   if(reason!=BasketballFinishReason.Ready)return FallbackShot(actor,reason,hoop,power);
   if(!Authority||!Eligible(actor)||!float.IsFinite(power))return false;
   var plan=FinishPlan(actor,hoop,kind);pendingAction=plan.action;pendingHeading=plan.heading;pendingPower=power;pendingShotHoop=hoop;
   acceptedApproachVelocity=FinishApproachVelocity(actor,Vector3.ProjectOnPlane(hoop.position-actor.transform.position,Vector3.up).normalized,false);
   chargingAthlete=null;chargingHoop=null;finishNotice=default;
   if(actor.BasketballMotion.Charging)StartFinish(actor);sendAt=0;return true;
  }
  void StartFinish(Athlete actor){
   var kind=pendingAction==BasketballAction.Dunk?BasketballFinish.Dunk:BasketballFinish.Layup;
   var reason=FinishAvailability(actor,kind,pendingShotHoop,true,true);
   if(reason!=BasketballFinishReason.Ready){pendingAction=BasketballAction.None;FallbackShot(actor,reason,pendingShotHoop,pendingPower);return;}
   // Exactly one host roll per committed layup. Timing never changes either
   // finish's outcome, and repeated requests cannot reroll a committed action.
   finishMake=kind==BasketballFinish.Dunk||UnityEngine.Random.value<BasketballFinishRules.LayupMakeChance;pendingPower=SweetSpot;
   var plan=FinishPlan(actor,pendingShotHoop,kind);var start=CarryPosition(actor);
   actor.BasketballMotion.BeginFinish(plan,Quaternion.Inverse(Quaternion.Euler(0,plan.heading,0))*(start-actor.transform.position));
  }
  public void InterruptFinish(Athlete actor){
   if(!Authority||!actor||!actor.BasketballMotion.Finishing)return;
   if(holder==actor){pendingAction=BasketballAction.None;var start=CarryPosition(actor);actor.BasketballMotion.Begin(BasketballAction.Cancel,actor.transform.eulerAngles.y,Quaternion.Inverse(actor.transform.rotation)*(start-actor.transform.position));dribblePhase=0;RejectFinish(actor,BasketballFinishReason.Lane);}
   else actor.BasketballMotion.ResetPose();
  }
  bool FinishVelocity(Athlete actor,BasketballAction action,Vector3 start,Transform hoop,out Vector3 velocity){
   var kind=action==BasketballAction.Dunk?BasketballFinish.Dunk:BasketballFinish.Layup;
   var goal=hoop.TransformPoint(new Vector3(0,BasketballHoop.RimHeight,0));
   if(kind==BasketballFinish.Layup){
    if(!finishMake)goal=hoop.TransformPoint(new Vector3(actor.BasketballMotion.State.leftHand?-.68f:.68f,BasketballHoop.RimHeight,.20f));
    return SolveShot(start,goal,.48f,maxShotSpeed,out velocity);
   }
   if(start.y<goal.y+Radius+.025f||Vector3.ProjectOnPlane(start-goal,Vector3.up).magnitude>.24f){velocity=Vector3.zero;return false;}
   // Perfect unblocked dunk, with no timing roll. The ball stays physical so
   // later defensive contacts can still stop it before the scoring crossing.
   const float time=.14f;velocity=(goal-start)/time-Physics.gravity*(time*.5f+Time.fixedDeltaTime*.5f);return true;
  }
 }
}
