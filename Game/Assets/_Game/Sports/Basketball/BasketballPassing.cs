using System;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public struct BasketballPassAim:INetworkSerializable,IEquatable<BasketballPassAim> {
  // 1: held aim, 2: committed gather, 3: brief release flash.
  public bool locked;public ulong receiver;public Vector3 destination;
  public byte phase;public uint play;public ulong owner;public double started,released;public float heading,power,bend;public Vector3 origin;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref locked);s.SerializeValue(ref receiver);s.SerializeValue(ref destination);s.SerializeValue(ref phase);s.SerializeValue(ref play);s.SerializeValue(ref owner);s.SerializeValue(ref started);s.SerializeValue(ref released);s.SerializeValue(ref heading);s.SerializeValue(ref power);s.SerializeValue(ref bend);s.SerializeValue(ref origin);}
  public bool Equals(BasketballPassAim b)=>locked==b.locked&&receiver==b.receiver&&destination==b.destination&&bend==b.bend&&phase==b.phase&&play==b.play&&owner==b.owner&&started==b.started&&released==b.released&&heading==b.heading&&power==b.power&&origin==b.origin;
 }
 public static class BasketballPassRules {
  public const float ChargeSeconds=.4f,FlashSeconds=.26f,SpeedMultiplier=1.5f;
  public static float Power(double seconds)=>Mathf.Clamp01((float)(seconds/ChargeSeconds));
  public static float Range(float power,float bend=0)=>SpeedMultiplier*(bend<-.01f?Mathf.Lerp(3,7.5f,Mathf.Clamp01(power)):Mathf.Lerp(4,10,Mathf.Clamp01(power)));
  public static string Name(float bend)=>bend>.01f?"Loft":bend<-.01f?"Bounce":"Chest";
  public static Vector3 Velocity(float heading,float power){
   float duration=Range(power)/(SpeedMultiplier*Mathf.Lerp(8,13,Mathf.Clamp01(power)));
   return Quaternion.Euler(0,heading,0)*Vector3.forward*(Range(power)/duration)-Physics.gravity*(duration*.5f+Time.fixedDeltaTime*.5f);
  }
 }
 public sealed partial class BasketballBall {
  BasketballPassAim passAim;float pendingPassPower;
  public Athlete PassReceiver {get {var aim=PassAim;if(!aim.locked)return null;foreach(var actor in Athlete.Active)if(BasketballMatch.PlayerId(actor)==aim.receiver)return actor;return null;}}
  public bool PassTeammate(Athlete passer,Athlete candidate)=>candidate&&candidate!=passer&&candidate.isActiveAndEnabled&&!candidate.inTransit&&!candidate.BasketballFreeRoam&&SportsPossession.SameTeam(passer,candidate,SportId.Basketball)&&(!BasketballMatch.Instance||!BasketballMatch.Instance.Competitive||BasketballMatch.Instance.Contains(candidate));
  public static float PassLockRadius(Athlete candidate)=>candidate&&candidate.capsule?candidate.capsule.radius*4*Mathf.Max(candidate.transform.lossyScale.x,candidate.transform.lossyScale.z):1.2f;
  public Athlete SelectPassReceiver(Athlete passer,float cameraHeading,Athlete retained=null){
   if(!passer||!float.IsFinite(cameraHeading))return null;
   var forward=Quaternion.Euler(0,cameraHeading,0)*Vector3.forward;
   bool InLane(Athlete candidate,out float distance){
    distance=float.PositiveInfinity;if(!PassTeammate(passer,candidate))return false;
    var delta=Vector3.ProjectOnPlane(candidate.transform.position-passer.transform.position,Vector3.up);float along=Vector3.Dot(delta,forward);distance=delta.sqrMagnitude;
    return along>.5f&&along<30&&(delta-forward*along).sqrMagnitude<=PassLockRadius(candidate)*PassLockRadius(candidate)&&!OutsidePaint(CourtPoint(candidate.transform.position))&&!Physics.Linecast(passer.transform.position+Vector3.up,candidate.transform.position+Vector3.up,1<<8,QueryTriggerInteraction.Ignore);
   }
   if(InLane(retained,out _))return retained;
   Athlete best=null;float nearest=float.PositiveInfinity;
   foreach(var candidate in Athlete.Active)if(InLane(candidate,out float distance)&&(distance<nearest-.001f||Mathf.Abs(distance-nearest)<.001f&&best&&BasketballMatch.PlayerId(candidate)<BasketballMatch.PlayerId(best))){best=candidate;nearest=distance;}
   return best;
  }
  void SetPassReceiver(Athlete actor,float heading){
   var receiver=SelectPassReceiver(actor,heading,PassReceiver);passAim.locked=receiver;passAim.receiver=receiver?BasketballMatch.PlayerId(receiver):ulong.MaxValue;
   passAim.destination=receiver?receiver.transform.position+Vector3.up*.9f:Vector3.zero;
   var delta=receiver?Vector3.ProjectOnPlane(receiver.transform.position-actor.transform.position,Vector3.up):Vector3.zero;
   passAim.heading=receiver?Quaternion.LookRotation(delta).eulerAngles.y:heading%360;
  }
  public BasketballPassPath PassPath(Vector3 origin,BasketballPassAim aim,float power)=>aim.locked?BasketballPassPath.ToReceiver(origin,aim.destination,power,aim.bend,PassFloor(origin)):BasketballPassPath.Create(origin,aim.heading,power,aim.bend,PassFloor(origin));
  public BasketballPassAim PassAim=>Authority?passAim:haveTarget?target.passAim:default;
  public double PassClock=>Authority?BasketballMotion.Clock:Math.Max(BasketballMotion.Clock,haveTarget?target.time:0);
  public float PassPower=>PassAim.phase==1?BasketballPassRules.Power(PassClock-PassAim.started):PassAim.power;
  public bool IsPassAiming(Athlete actor)=>actor&&PassAim.phase==1&&Held&&Holder==actor;
  public bool BeginPassCharge(Athlete actor,float heading,uint play){
   if(!Authority||play!=Defense.play||!float.IsFinite(heading)||!CanPass(actor)||!Eligible(actor)||chargingAthlete||passAim.phase==1)return false;
   passAim=new BasketballPassAim{phase=1,play=play,owner=PlayerId(actor),heading=heading%360,started=BasketballMotion.Clock};SetPassReceiver(actor,heading);sendAt=0;return true;
  }
  public void AimPass(Athlete actor,float heading,float bend=0){
   if(!Authority||!IsPassAiming(actor)||!float.IsFinite(heading)||!float.IsFinite(bend))return;
   SetPassReceiver(actor,heading);passAim.bend=Mathf.MoveTowards(passAim.bend,Mathf.Clamp(bend,-1,1),Time.fixedDeltaTime*5);actor.BasketballMotion.AimHeading(passAim.heading);
  }
  public void CancelPassCharge(Athlete actor,uint play){
   if(!Authority||!actor||holder!=actor||passAim.phase!=1||passAim.play!=play)return;
   passAim=default;sendAt=0;
   if(actor.BasketballMotion.PassCharging&&!IsInbound(actor)){var start=CarryPosition(actor);actor.BasketballMotion.Begin(BasketballAction.PassCancel,actor.transform.eulerAngles.y,Quaternion.Inverse(actor.transform.rotation)*(start-actor.transform.position));dribblePhase=0;}
  }
  public bool ReleasePassCharge(Athlete actor,float heading,uint play,double releasedAt,float bend=0){
   if(!Authority||!IsPassAiming(actor)||play!=Defense.play||passAim.play!=play||!float.IsFinite(heading)||!double.IsFinite(releasedAt)||!float.IsFinite(bend))return false;
   double now=BasketballMotion.Clock;
   float power=BasketballPassRules.Power(Math.Clamp(releasedAt,Math.Max(passAim.started,now-.25),now)-passAim.started);
   return CommitPass(actor,heading,power,Mathf.Clamp(bend,-1,1));
  }
  bool CommitPass(Athlete actor,float heading,float power,float bend=0){
   if(!Authority||!float.IsFinite(heading)||!CanPass(actor))return false;
   SetPassReceiver(actor,heading);
   if(IsInbound(actor)&&BasketballMatch.Instance&&BasketballMatch.Instance.Competitive&&!passAim.locked)return false;
   if(!TryAction(actor,passAim.heading,BasketballAction.Pass))return false;
   pendingPassPower=power;passAim.phase=2;passAim.play=Defense.play;passAim.owner=PlayerId(actor);passAim.power=power;passAim.bend=bend;passAim.started=BasketballMotion.Clock;sendAt=0;return true;
  }
  void StepPassCharge(){
   if(passAim.phase!=1)return;
   if(!holder||!Eligible(holder)||passAim.play!=Defense.play){passAim=default;sendAt=0;return;}
   var motion=holder.BasketballMotion;
   if(!motion.PassCharging&&!motion.Busy&&(DribblePhase>.90f||DribblePhase<.10f||holder.Airborne||holder.LoadingJump)){
    var start=CarryPosition(holder);motion.Begin(BasketballAction.PassCharge,passAim.heading,Quaternion.Inverse(Quaternion.Euler(0,passAim.heading,0))*(start-holder.transform.position));
   }
  }
 }
}
