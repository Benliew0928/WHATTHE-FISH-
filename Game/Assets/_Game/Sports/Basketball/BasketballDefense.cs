using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public enum BasketballRole:byte { Inactive,Loose,Attack,Defense }
 public struct BasketballDefenseSnapshot:INetworkSerializable,IEquatable<BasketballDefenseSnapshot> {
  public uint blocks,play;public double time;public ulong blocker,originator;public bool passing;public Vector3 contact;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref blocks);s.SerializeValue(ref play);s.SerializeValue(ref time);s.SerializeValue(ref blocker);s.SerializeValue(ref originator);s.SerializeValue(ref passing);s.SerializeValue(ref contact);}
  public bool Equals(BasketballDefenseSnapshot b)=>blocks==b.blocks&&play==b.play&&time==b.time&&blocker==b.blocker&&originator==b.originator&&passing==b.passing&&contact==b.contact;
 }
 public static class BasketballDefenseRules {
  public const float GuardSpeed=3,GuardRange=3.5f,HandRadius=.12f,BlockStart=.12f,BlockEnd=.30f,BlockDuration=.70f;
  public const float JumpLoad=.16f,JumpHeight=2.20f,JumpStart=.22f,JumpEnd=.88f,JumpDuration=1.30f,Recovery=.58f;
  public static bool IsBlock(BasketballAction action)=>action==BasketballAction.Block||action==BasketballAction.JumpBlock;
  public static bool Active(BasketballAction action,float time)=>IsBlock(action)&&time>=(action==BasketballAction.JumpBlock?JumpStart:BlockStart)&&time<=(action==BasketballAction.JumpBlock?JumpEnd:BlockEnd);
  public static float Window(float pressure)=>Mathf.Lerp(BasketballBall.SweetWindow,.025f,Mathf.Clamp01(pressure));
  // Solve relative sphere motion. Endpoint overlap alone misses fast releases.
  public static bool Sweep(Vector3 hand0,Vector3 hand1,Vector3 ball0,Vector3 ball1,float radius,out float fraction){
   fraction=0;if(!Finite(hand0)||!Finite(hand1)||!Finite(ball0)||!Finite(ball1)||!float.IsFinite(radius)||radius<=0)return false;
   var p=ball0-hand0;var v=ball1-ball0-(hand1-hand0);float c=p.sqrMagnitude-radius*radius;
   if(c<=0)return true;float a=v.sqrMagnitude;if(a<.000001f)return false;
   float b=Vector3.Dot(p,v),d=b*b-a*c;if(d<0)return false;
   fraction=(-b-Mathf.Sqrt(d))/a;return fraction>=0&&fraction<=1;
  }
  public static float Pressure(Vector3 defender,Vector3 forward,Vector3 shooter,Vector3 hoop){
   var to=Vector3.ProjectOnPlane(shooter-defender,Vector3.up);float distance=to.magnitude;
   if(distance>2.4f||distance<.01f||Mathf.Abs(shooter.y-defender.y)>.8f)return 0;
   float facing=Mathf.InverseLerp(.45f,.95f,Vector3.Dot(forward,to/distance));
   var lane=Vector3.ProjectOnPlane(hoop-shooter,Vector3.up).normalized;
   float position=Mathf.InverseLerp(.15f,.85f,Vector3.Dot(-to/distance,lane));
   return Mathf.InverseLerp(2.4f,.75f,distance)*facing*position;
  }
  public static Vector3 Deflect(Vector3 velocity,Vector3 handVelocity,Vector3 normal,Vector3 forward){
   if(normal.sqrMagnitude<.001f)normal=forward;normal.Normalize();
   var relative=velocity-handVelocity*.32f;float into=Vector3.Dot(relative,normal);
   if(into<0)relative-=normal*(1.45f*into);
   var result=relative*.58f+handVelocity*.32f+normal*2.4f+forward*.65f;
   result=Vector3.ClampMagnitude(result,13);result.y=Mathf.Clamp(result.y,-8,5);return result;
  }
  public static bool ProtectedRim(Vector3 previous,Vector3 current,Vector3 hoopLocalContact){
   return current.y<previous.y&&hoopLocalContact.y>BasketballHoop.RimHeight-BasketballBall.Radius&&new Vector2(hoopLocalContact.x,hoopLocalContact.z).magnitude<.65f;
  }
  public static bool Finite(Vector3 p)=>float.IsFinite(p.x)&&float.IsFinite(p.y)&&float.IsFinite(p.z);
 }

 public sealed partial class BasketballBall {
  sealed class BlockAttempt {public Athlete actor;public uint sequence,play;public Vector3 hand,ball;public float elapsed;}
  readonly List<BlockAttempt> blockAttempts=new();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public Action<string> DefenseTrace;
#endif
  uint defensePlay,blocks;double blockTime=-100;ulong blocker=ulong.MaxValue;Vector3 blockContact;
  public BasketballDefenseSnapshot Defense=>Authority?new BasketballDefenseSnapshot{blocks=blocks,play=defensePlay,time=blockTime,blocker=blocker,originator=lastShooter?PlayerId(lastShooter):ulong.MaxValue,passing=passing,contact=blockContact}:haveTarget?target.defense:default;
  public BasketballRole Role(Athlete actor){
   if(!Playing||!actor||actor.inTransit||actor.BasketballFreeRoam)return BasketballRole.Inactive;
   if(Held)return Holder==actor?BasketballRole.Attack:BasketballRole.Defense;
   bool flight=Score.result==BasketballResult.Flying||Defense.passing;
   bool own=Authority?lastShooter==actor:Defense.originator==PlayerId(actor);
   return flight&&!own?BasketballRole.Defense:BasketballRole.Loose;
  }
  public bool CanBlock(Athlete actor)=>Role(actor)==BasketballRole.Defense&&actor.BasketballMotion&&!actor.BasketballMotion.Busy&&!actor.Airborne&&!actor.LoadingJump;
  public void SetGuard(Athlete actor,bool held,float heading){
   if(!Authority||!actor||!actor.BasketballMotion)return;var motion=actor.BasketballMotion;
   bool allowed=held&&Role(actor)==BasketballRole.Defense&&Eligible(actor)&&actor.Grounded&&!actor.Airborne&&!actor.LoadingJump&&float.IsFinite(heading);
   if(!allowed){motion.EndGuard();return;}
   if(motion.Busy)return;
   var point=Holder?Holder.transform.position:Body.position;var delta=Vector3.ProjectOnPlane(point-actor.transform.position,Vector3.up);
   if(delta.sqrMagnitude>.01f&&delta.sqrMagnitude<BasketballDefenseRules.GuardRange*BasketballDefenseRules.GuardRange&&Vector3.Dot(actor.transform.forward,delta.normalized)>-.3f)heading=Quaternion.LookRotation(delta).eulerAngles.y;
   motion.Guard(heading);
  }
  public float ShotPressure(Athlete shooter,Transform hoop){
   if(!shooter||!hoop)return 0;float pressure=0;
   foreach(var defender in Athlete.Active){
    if(defender==shooter||!defender.gameObject.activeInHierarchy||defender.inTransit||defender.BasketballFreeRoam||!defender.BasketballMotion||!defender.BasketballMotion.Guarding||defender.Airborne)continue;
    float candidate=BasketballDefenseRules.Pressure(defender.transform.position,defender.transform.forward,shooter.transform.position,hoop.position);
    if(candidate<=pressure||Physics.Linecast(defender.transform.position+Vector3.up,shooter.transform.position+Vector3.up,1<<8,QueryTriggerInteraction.Ignore))continue;
    pressure=candidate;
   }
   return Mathf.Clamp01(pressure);
  }
  public bool TryBlock(Athlete actor,float heading,bool jumping,uint play=uint.MaxValue){
   if(!Authority||!CanBlock(actor)||!Eligible(actor)||!actor.Grounded||Outside(actor.transform.position)||!float.IsFinite(heading)||play!=uint.MaxValue&&play!=defensePlay)return false;
   var motion=actor.BasketballMotion;var point=Held?CarryPosition(holder):Body.position+Body.linearVelocity*.18f+Physics.gravity*(.5f*.18f*.18f);
   if(Held&&(holder.BasketballMotion.Action==BasketballAction.Shoot||holder.BasketballMotion.Action==BasketballAction.Pass)){
    var release=holder.BasketballMotion;
    point=holder.transform.position+release.Facing*release.BallOffset(Mathf.Min(release.Elapsed+.18f,BasketballMotion.ReleaseTime(release.Action)));
   }
   if(Held&&!holder.BasketballMotion.CarryingPose&&!holder.Airborne){var bounce=BasketballMotion.DribbleOffset(DribblePhase+dribbleCadence*.20f);bounce.y=Mathf.Max(Radius+.004f,bounce.y);point=holder.transform.TransformPoint(bounce);}
   // Guard turns the body first. A block commits that facing; it never homes
   // toward a moving ball or changes height after a shot fake.
   float committed=motion.Guarding?actor.transform.eulerAngles.y:Mathf.MoveTowardsAngle(actor.transform.eulerAngles.y,heading,55);
   motion.BeginDefense(jumping?BasketballAction.JumpBlock:BasketballAction.Block,committed,point);
   blockAttempts.Add(new BlockAttempt{actor=actor,sequence=motion.State.sequence,play=defensePlay,hand=motion.DefensePalm(0),ball=Held?CarryPosition(holder):Body.position});
   return true;
  }
  void NewDefensePlay(){defensePlay++;blockAttempts.Clear();}
  void ClearDefense(){NewDefensePlay();foreach(var actor in Athlete.Active)if(actor.BasketballMotion)actor.BasketballMotion.EndGuard();}
  void StepBlocks(){
   if(blockAttempts.Count==0)return;
   Vector3 current=Held?CarryPosition(holder):Body.position;
   BlockAttempt winner=null;float earliest=float.PositiveInfinity;Vector3 hit=default,handHit=default,handVelocity=default;
   for(int i=blockAttempts.Count-1;i>=0;i--){
    var attempt=blockAttempts[i];var actor=attempt.actor;var motion=actor?actor.BasketballMotion:null;
    if(!actor||!Eligible(actor)||!motion||motion.State.sequence!=attempt.sequence||!motion.Blocking||attempt.play!=defensePlay||Holder==actor||actor.BasketballFreeRoam){blockAttempts.RemoveAt(i);continue;}
    float now=motion.Elapsed,start=motion.JumpBlocking?BasketballDefenseRules.JumpStart:BasketballDefenseRules.BlockStart,end=motion.JumpBlocking?BasketballDefenseRules.JumpEnd:BasketballDefenseRules.BlockEnd;
    var palm=motion.DefensePalm(now);float dt=Mathf.Max(.0001f,now-attempt.elapsed);
    if(now>=start&&attempt.elapsed<=end&&(Held||Score.result==BasketballResult.Flying||passing)){
     float a=Mathf.Clamp01((start-attempt.elapsed)/dt),b=Mathf.Clamp01((end-attempt.elapsed)/dt);
     var h0=Vector3.Lerp(attempt.hand,palm,a);var h1=Vector3.Lerp(attempt.hand,palm,b);var p0=Vector3.Lerp(attempt.ball,current,a);var p1=Vector3.Lerp(attempt.ball,current,b);
     bool touching=BasketballDefenseRules.Sweep(h0,h1,p0,p1,Radius+BasketballDefenseRules.HandRadius,out float fraction);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
     DefenseTrace?.Invoke($"{motion.Action},{now:F4},{attempt.elapsed:F4},{Vector3.Distance(h0,p0):F4},{Vector3.Distance(h1,p1):F4},{touching}");
#endif
     if(touching){
      float contactFraction=Mathf.Lerp(a,b,fraction);var point=Vector3.Lerp(attempt.ball,current,contactFraction);var hand=Vector3.Lerp(attempt.hand,palm,contactFraction);
      var shoulder=motion.DefenseShoulder;var toward=Vector3.ProjectOnPlane(point-actor.transform.position,Vector3.up);
      bool clear=toward.sqrMagnitude<.001f||Vector3.Dot(motion.Facing*Vector3.forward,toward.normalized)>.35f;
      clear&=!Physics.Linecast(shoulder,point,1<<8,QueryTriggerInteraction.Ignore)&&!Physics.CheckSphere(point,Radius+.004f,1<<8,QueryTriggerInteraction.Ignore);
      if(Held){
       clear&=!BasketballStealRules.Shielded(shoulder,point,holder.transform.position,.255f);
       // A low ball still secured at the hip cannot be torn through both hands.
       if(holder.BasketballMotion.Charging&&point.y-holder.transform.position.y<.90f)clear=false;
      }else if(attemptedHoop&&BasketballDefenseRules.ProtectedRim(attempt.ball,current,attemptedHoop.transform.InverseTransformPoint(point)))clear=false;
      foreach(var other in Athlete.Active)if(other!=actor&&other!=Holder&&other.capsule&&other.gameObject.activeInHierarchy&&BasketballStealRules.Shielded(shoulder,point,other.transform.position,other.capsule.radius))clear=false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
      DefenseTrace?.Invoke($"contact,{clear},facing={Vector3.Dot(motion.Facing*Vector3.forward,toward.normalized):F3},held={Held},holderShield={(Held&&BasketballStealRules.Shielded(shoulder,point,holder.transform.position,.255f))}");
#endif
      // Stable tie breaking, independent of HashSet/RPC iteration order.
      if(clear&&(contactFraction<earliest-.0001f||Mathf.Abs(contactFraction-earliest)<.0001f&&winner!=null&&PlayerId(actor)<PlayerId(winner.actor))){winner=attempt;earliest=contactFraction;hit=point;handHit=hand;handVelocity=Vector3.ClampMagnitude((palm-attempt.hand)/dt,12);}
     }
    }
    attempt.hand=palm;attempt.ball=current;attempt.elapsed=now;
    if(now>end)blockAttempts.RemoveAt(i);
   }
   if(winner!=null){
    // Resolve the flight up to contact before changing its path. A basket
    // completed earlier in this same physics step cannot be erased by a hand.
    if(!Held&&samplingFlight){
     bool made=false;
     foreach(var hoop in baskets)if(hoop&&hoop.StepBall(flightPrevious,hit,Body,Radius,score.result==BasketballResult.Flying&&hoop==attemptedHoop)){ResolveAttempt(true);made=true;}
     flightPrevious=hit;if(made)return;
    }
    ApplyBlock(winner.actor,hit,handHit,handVelocity);NewDefensePlay();
   }
  }
  void ApplyBlock(Athlete defender,Vector3 point,Vector3 hand,Vector3 handVelocity){
   var victim=holder;var forward=defender.BasketballMotion.Facing*Vector3.forward;
   var incoming=victim?victim.Motor.Velocity+Vector3.up*victim.Jump.Velocity:Body.linearVelocity;
   var velocity=BasketballDefenseRules.Deflect(incoming,handVelocity,point-hand,forward);
   if(victim){
    CancelShotCharge(victim);pendingAction=BasketballAction.None;ResetShotTracking();passing=false;
    victim.BasketballMotion.BeginDefense(BasketballAction.Blocked,victim.transform.eulerAngles.y,point);
    holder=null;lastShooter=victim;shooterPickupAt=Time.time+.70f;
   }
   // Preserve a released attempt: a touched shot may still ricochet in.
   RestoreShooterCollision();RestoreStealerCollision();RestoreFloorContacts();
   Body.position=point;transform.position=point;Body.isKinematic=false;Body.detectCollisions=true;Body.linearDamping=restDamping;
   if(!victim){flightPrevious=point;samplingFlight=true;foreach(var hoop in baskets)if(hoop)hoop.ResetCrossing();}
   pickupAt=Time.time+.34f;looseFor=outsideFor=0;
   var sphere=GetComponent<SphereCollider>();
   bool Overlaps(Collider c)=>c&&(c.ClosestPoint(point)-point).sqrMagnitude<Radius*Radius;
   ignoredShooter=victim&&Overlaps(victim.capsule)?victim.capsule:null;ignoredStealer=Overlaps(defender.capsule)?defender.capsule:null;
   if(ignoredShooter)Physics.IgnoreCollision(sphere,ignoredShooter,true);if(ignoredStealer)Physics.IgnoreCollision(sphere,ignoredStealer,true);
   Body.linearVelocity=velocity;Body.angularVelocity=Vector3.ClampMagnitude(Vector3.Cross(point-hand,velocity)*35,28);Body.WakeUp();
   blocks++;blockTime=BasketballMotion.Clock;blocker=PlayerId(defender);blockContact=point;reset++;sendAt=0;
  }
 }
}
