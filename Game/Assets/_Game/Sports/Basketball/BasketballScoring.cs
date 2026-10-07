using System;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public enum BasketballResult:byte { None,Flying,Scored,Missed }
 public struct BasketballScore:INetworkSerializable,IEquatable<BasketballScore> {
  public uint attempts,made,missed,points,sequence;
  public BasketballResult result;
  public byte lastPoints;
  public ulong shooter;
  public double changed;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {
   s.SerializeValue(ref attempts);s.SerializeValue(ref made);s.SerializeValue(ref missed);s.SerializeValue(ref points);s.SerializeValue(ref sequence);
   s.SerializeValue(ref result);s.SerializeValue(ref lastPoints);s.SerializeValue(ref shooter);s.SerializeValue(ref changed);
  }
  public bool Equals(BasketballScore b)=>attempts==b.attempts&&made==b.made&&missed==b.missed&&points==b.points&&sequence==b.sequence&&result==b.result&&lastPoints==b.lastPoints&&shooter==b.shooter&&changed==b.changed;
 }

 public sealed partial class BasketballBall {
  public const float ChargeDuration=1.2f*BasketballMotion.ShotTimeScale,SweetSpot=.65f,SweetWindow=.045f;
  public BasketballScore Score=>Authority?score:haveTarget?target.score:default;
  public Vector3 LastLaunchVelocity {get;private set;}
  public Vector3 LastLaunchImpulse=>LastLaunchVelocity*Body.mass;
  public float LastReleasePower {get;private set;}
  BasketballScore score;
  readonly BasketballHoop[] baskets=new BasketballHoop[2];
  BasketballHoop attemptedHoop;
  Vector3 flightPrevious;
  bool samplingFlight;
  byte attemptedPoints;
  Athlete chargingAthlete;
  double chargeStarted;
  double chargePhase,chargeSampled;
  float chargePeriod=ChargeDuration;
  Transform chargingHoop,pendingShotHoop;
  readonly System.Collections.Generic.List<(double time,double phase,float pressure)> chargeHistory=new();
  float pendingPower=SweetSpot,pendingWindow=SweetWindow;

  // One-way sweeps get progressively faster from close shots to the far court.
  // Integrate phase when moving, rather than rescaling elapsed time and jumping
  // the needle. The chosen hoop stays locked until release or cancellation.
  public static float ChargePeriod(float distance)=>ChargeDuration/Mathf.Lerp(1,2,Mathf.SmoothStep(0,1,Mathf.InverseLerp(4,22,distance)));
  public static float PowerAtPhase(double phase)=>Mathf.PingPong((float)(Math.Max(0,phase)%2),1);
  public bool IsCharging(Athlete actor)=>actor&&Held&&Holder==actor&&(Authority?chargingAthlete==actor:haveTarget&&target.charge.active);
  public BasketballCharge Charge=>Authority?new BasketballCharge{active=chargingAthlete,phase=chargePhase,time=chargeSampled,period=chargePeriod,pressure=ShotPressure(chargingAthlete,chargingHoop),hoop=chargingHoop&&chargingHoop.name=="Hoop_South"?(byte)1:(byte)0}:haveTarget?target.charge:default;
  public Transform ChargeHoop(Athlete actor,float heading){
   if(IsCharging(actor)){var c=Charge;if(Authority)return chargingHoop;int index=c.hoop<2?c.hoop:0;return baskets[index]?baskets[index].transform:null;}
   return actor?SelectHoop(actor.transform.position,heading):null;
  }
  public float ChargeDistance(Athlete actor,float heading){var hoop=ChargeHoop(actor,heading);return hoop&&actor?Vector3.ProjectOnPlane(hoop.position-actor.transform.position,Vector3.up).magnitude:4;}
  // A freshly received sample can be ahead of a client's buffered server
  // clock. Its visible phase and release timestamp must describe the same
  // instant; otherwise an edge-of-green release is scored at an older phase.
  public double PresentedChargeTime=>Math.Max(BasketballMotion.Clock,Charge.time);
  public double PresentedChargePhase=>Charge.phase+(PresentedChargeTime-Charge.time)/Mathf.Max(.1f,Charge.period);
  void AdvanceCharge(){
   if(!chargingAthlete)return;
   double now=BasketballMotion.Clock;chargePhase+=Math.Max(0,now-chargeSampled)/chargePeriod;chargeSampled=now;
   chargePeriod=ChargePeriod(ChargeDistance(chargingAthlete,chargingAthlete.transform.eulerAngles.y));
   if(chargeHistory.Count==0||now>chargeHistory[^1].time){chargeHistory.Add((now,chargePhase,ShotPressure(chargingAthlete,chargingHoop)));while(chargeHistory.Count>2&&chargeHistory[1].time<now-.5)chargeHistory.RemoveAt(0);}
  }
  double ReleasePhase(double time){
   // Timestamp is an input edge, never a supplied power/velocity. Bound rewind
   // to 250 ms of host history to remove frame/RPC delay without arbitrary picks.
   double now=BasketballMotion.Clock;if(double.IsNaN(time)||double.IsInfinity(time))return chargePhase;
   time=Math.Clamp(time,Math.Max(chargeStarted,now-.25),now);
   for(int i=1;i<chargeHistory.Count;i++)if(time<=chargeHistory[i].time){var a=chargeHistory[i-1];var b=chargeHistory[i];return a.phase+(b.phase-a.phase)*Math.Clamp((time-a.time)/Math.Max(.000001,b.time-a.time),0,1);}
   return chargePhase;
  }

  float ReleaseWindow(double time){
   double now=BasketballMotion.Clock;
   if(double.IsNaN(time)||double.IsInfinity(time))time=now;
   time=Math.Clamp(time,Math.Max(chargeStarted,now-.25),now);
   for(int i=chargeHistory.Count-1;i>=0;i--)if(chargeHistory[i].time<=time)return BasketballDefenseRules.Window(chargeHistory[i].pressure);
   return BasketballDefenseRules.Window(ShotPressure(chargingAthlete,chargingHoop));
  }
  void PrepareHoops(){
   if(!transform.parent)return;
   foreach(var t in transform.parent.GetComponentsInChildren<Transform>(true)){
    int index=t.name=="Hoop_North"?0:t.name=="Hoop_South"?1:-1;if(index<0)continue;
    baskets[index]=t.GetComponent<BasketballHoop>()??t.gameObject.AddComponent<BasketballHoop>();
   }
  }
  public bool BeginShotCharge(Athlete athlete,float heading=float.NaN){
   if(!Authority||!CanShoot(athlete)||!Eligible(athlete)||chargingAthlete||IsPassAiming(athlete))return false;
   if(float.IsNaN(heading))heading=athlete.transform.eulerAngles.y;if(!float.IsFinite(heading))return false;
   chargingHoop=SelectHoop(athlete.transform.position,heading);if(!chargingHoop)return false;
   chargingAthlete=athlete;chargeStarted=chargeSampled=BasketballMotion.Clock;chargePhase=0;chargePeriod=ChargePeriod(ChargeDistance(athlete,heading));chargeHistory.Clear();chargeHistory.Add((chargeStarted,0,ShotPressure(athlete,chargingHoop)));sendAt=0;return true;
  }
  public void CancelShotCharge(Athlete athlete){if(Authority&&chargingAthlete==athlete&&athlete){chargingAthlete=null;chargingHoop=null;sendAt=0;if(athlete.BasketballMotion&&athlete.BasketballMotion.Charging){var start=CarryPosition(athlete);athlete.BasketballMotion.Begin(BasketballAction.Cancel,athlete.transform.eulerAngles.y,Quaternion.Inverse(athlete.transform.rotation)*(start-athlete.transform.position));dribblePhase=0;}}}
  public bool ReleaseShotCharge(Athlete athlete,float heading,double releasedAt=double.NaN,BasketballFinish kind=BasketballFinish.Shot){
   if(!Authority||!athlete||chargingAthlete!=athlete)return false;
   if(!float.IsFinite(heading)||!chargingHoop){CancelShotCharge(athlete);return false;}
   AdvanceCharge();float power=PowerAtPhase(ReleasePhase(releasedAt));var hoop=chargingHoop;
   heading=Quaternion.LookRotation(Vector3.ProjectOnPlane(hoop.position-athlete.transform.position,Vector3.up)).eulerAngles.y;
   if(kind!=BasketballFinish.Shot){float finishWindow=ReleaseWindow(releasedAt);bool accepted=TryFinish(athlete,kind,hoop,power);if(accepted)pendingWindow=finishWindow;return accepted;}
   float window=ReleaseWindow(releasedAt);bool fired=TryShoot(athlete,heading,power);if(fired){pendingShotHoop=hoop;pendingWindow=window;}else CancelShotCharge(athlete);return fired;
  }
  // Match the authored 7.239 m arc and 6.7056 m corner lines. The line
  // belongs to the two-point region; use the shooter's feet at release.
  public static byte ShotValue(Vector3 feetInHoopSpace){
   float x=Mathf.Abs(feetInHoopSpace.x),z=feetInHoopSpace.z;
   return (x>6.731f||z>0&&new Vector2(x,z).magnitude>7.2644f)?(byte)3:(byte)2;
  }
  void BeginAttempt(Transform hoop,Athlete shooter,Vector3 start){
   attemptedHoop=hoop.GetComponent<BasketballHoop>();
   attemptedPoints=ShotValue(hoop.InverseTransformPoint(shooter.transform.position));
   score.attempts++;score.result=BasketballResult.Flying;score.lastPoints=0;score.shooter=PlayerId(shooter);score.changed=BasketballMotion.Clock;score.sequence++;
   flightPrevious=start;samplingFlight=true;
   foreach(var basket in baskets)if(basket)basket.ResetCrossing();
  }
  void ResolveAttempt(bool made){
   if(score.result!=BasketballResult.Flying)return;
   score.result=made?BasketballResult.Scored:BasketballResult.Missed;score.lastPoints=made?attemptedPoints:(byte)0;
   if(made){score.made++;score.points+=attemptedPoints;}else score.missed++;
   score.changed=BasketballMotion.Clock;score.sequence++;sendAt=0;attemptedHoop=null;
  }
  void ResetShotTracking(){
   ResolveAttempt(false);chargingAthlete=null;samplingFlight=false;
   foreach(var basket in baskets)if(basket)basket.ResetCrossing();
  }
  void ResetSessionScore(){
   blocks=0;blockTime=-100;score=default;finishNotice=default;attemptedHoop=null;chargingAthlete=null;samplingFlight=false;
   foreach(var basket in baskets)if(basket)basket.ResetNet();
  }
  void StepShotPhysics(){
   if(chargingAthlete){if(!Eligible(chargingAthlete)||holder!=chargingAthlete||!Playing)CancelShotCharge(chargingAthlete);else AdvanceCharge();}
   var current=Body.position;
   if(Held){samplingFlight=false;return;}
   if(!samplingFlight){flightPrevious=current;samplingFlight=true;}
   foreach(var basket in baskets){
    if(!basket)continue;
    bool made=basket.StepBall(flightPrevious,current,Body,Radius,score.result==BasketballResult.Flying&&basket==attemptedHoop);
    if(made)ResolveAttempt(true);
   }
   flightPrevious=current;
   if(score.result==BasketballResult.Flying&&BasketballMotion.Clock-score.changed>8)ResolveAttempt(false);
  }
  void ShotContact(Collision collision){
   if(!Authority||Held)return;
   if(passing)for(int i=0;i<collision.contactCount;i++)if(collision.GetContact(i).normal.y>.45f){passing=false;sendAt=0;break;}
   var basket=collision.collider.GetComponentInParent<BasketballHoop>();
   if(basket&&collision.contactCount>0)basket.RimContact(collision.GetContact(0).point,collision.relativeVelocity);
   if(score.result!=BasketballResult.Flying)return;
   for(int i=0;i<collision.contactCount;i++){
    var c=collision.GetContact(i);var p=transform.parent?transform.parent.InverseTransformPoint(c.point):c.point;
    if(p.y<.3f&&c.normal.y>.45f){ResolveAttempt(false);break;}
   }
  }
 }
}
