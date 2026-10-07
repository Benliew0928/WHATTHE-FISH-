using System;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public struct BasketballPassAim:INetworkSerializable,IEquatable<BasketballPassAim> {
  // 1: held aim, 2: committed gather, 3: brief release flash.
  public byte phase;public uint play;public ulong owner;public double started,released;public float heading,power,bend;public Vector3 origin;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref phase);s.SerializeValue(ref play);s.SerializeValue(ref owner);s.SerializeValue(ref started);s.SerializeValue(ref released);s.SerializeValue(ref heading);s.SerializeValue(ref power);s.SerializeValue(ref bend);s.SerializeValue(ref origin);}
  public bool Equals(BasketballPassAim b)=>bend==b.bend&&phase==b.phase&&play==b.play&&owner==b.owner&&started==b.started&&released==b.released&&heading==b.heading&&power==b.power&&origin==b.origin;
 }
 public static class BasketballPassRules {
  public const float ChargeSeconds=.8f,FlashSeconds=.26f;
  public static float Power(double seconds)=>Mathf.Clamp01((float)(seconds/ChargeSeconds));
  public static float Range(float power,float bend=0)=>bend<-.01f?Mathf.Lerp(3,7.5f,Mathf.Clamp01(power)):Mathf.Lerp(4,10,Mathf.Clamp01(power));
  public static string Name(float bend)=>bend>.01f?"Loft":bend<-.01f?"Bounce":"Chest";
  public static Vector3 Velocity(float heading,float power){
   float duration=Range(power)/Mathf.Lerp(8,13,Mathf.Clamp01(power));
   return Quaternion.Euler(0,heading,0)*Vector3.forward*(Range(power)/duration)-Physics.gravity*(duration*.5f+Time.fixedDeltaTime*.5f);
  }
 }
 public sealed partial class BasketballBall {
  BasketballPassAim passAim;float pendingPassPower;
  public BasketballPassAim PassAim=>Authority?passAim:haveTarget?target.passAim:default;
  public double PassClock=>Authority?BasketballMotion.Clock:Math.Max(BasketballMotion.Clock,haveTarget?target.time:0);
  public float PassPower=>PassAim.phase==1?BasketballPassRules.Power(PassClock-PassAim.started):PassAim.power;
  public bool IsPassAiming(Athlete actor)=>actor&&PassAim.phase==1&&Held&&Holder==actor;
  public bool BeginPassCharge(Athlete actor,float heading,uint play){
   if(!Authority||play!=Defense.play||!float.IsFinite(heading)||!CanShoot(actor)||!Eligible(actor)||chargingAthlete||passAim.phase==1)return false;
   passAim=new BasketballPassAim{phase=1,play=play,owner=PlayerId(actor),heading=heading%360,started=BasketballMotion.Clock};sendAt=0;return true;
  }
  public void AimPass(Athlete actor,float heading,float bend=0){
   if(!Authority||!IsPassAiming(actor)||!float.IsFinite(heading)||!float.IsFinite(bend))return;
   passAim.heading=heading%360;passAim.bend=Mathf.MoveTowards(passAim.bend,Mathf.Clamp(bend,-1,1),Time.fixedDeltaTime*5);actor.BasketballMotion.AimHeading(passAim.heading);
  }
  public void CancelPassCharge(Athlete actor,uint play){
   if(!Authority||!actor||holder!=actor||passAim.phase!=1||passAim.play!=play)return;
   passAim=default;sendAt=0;
   if(actor.BasketballMotion.PassCharging){var start=CarryPosition(actor);actor.BasketballMotion.Begin(BasketballAction.PassCancel,actor.transform.eulerAngles.y,Quaternion.Inverse(actor.transform.rotation)*(start-actor.transform.position));dribblePhase=0;}
  }
  public bool ReleasePassCharge(Athlete actor,float heading,uint play,double releasedAt,float bend=0){
   if(!Authority||!IsPassAiming(actor)||play!=Defense.play||passAim.play!=play||!float.IsFinite(heading)||!double.IsFinite(releasedAt)||!float.IsFinite(bend))return false;
   double now=BasketballMotion.Clock;
   float power=BasketballPassRules.Power(Math.Clamp(releasedAt,Math.Max(passAim.started,now-.25),now)-passAim.started);
   return CommitPass(actor,heading,power,Mathf.Clamp(bend,-1,1));
  }
  bool CommitPass(Athlete actor,float heading,float power,float bend=0){
   if(!TryAction(actor,heading,BasketballAction.Pass))return false;
   pendingPassPower=power;
   passAim=new BasketballPassAim{phase=2,play=Defense.play,owner=PlayerId(actor),heading=heading%360,power=power,bend=bend,started=BasketballMotion.Clock};sendAt=0;return true;
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
