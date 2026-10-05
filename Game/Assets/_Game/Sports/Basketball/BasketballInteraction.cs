using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public sealed partial class BasketballBall {
  public static BasketballBall Active {get;private set;}
  [Header("Pickup and carry")]
  public bool autoPickup=true;
  [Min(.2f)] public float pickupRadius=1.05f;
  [Min(.2f)] public float pickupHeight=1.65f;
  [Min(1)] public float pickupMaxSpeed=10;
  public Vector3 carryOffset=new(0,.80f,.34f);
  [Min(.1f)] public float releaseGrace=.3f,shooterGrace=.9f;
  [Header("Assisted shot")]
  [Min(.5f)] public float arcHeight=1.6f;
  [Min(0)] public float arcPerMetre=.18f;
  [Min(1)] public float maxShotSpeed=24;
  [Min(0)] public float backspin=18;
  [Header("Recovery (arena local metres)")]
  public Vector2 courtLimits=new(10,17);
  [Min(.1f)] public float boundaryDelay=.75f;
  [Min(2)] public float strandedTimeout=12;

  Athlete holder,lastShooter;Collider ignoredShooter;float pickupAt,shooterPickupAt,outsideFor,looseFor,restDamping,dribblePhase,dribbleCadence=1.65f;uint shotCount,passCount;bool shotFlying;
  BasketballAction pendingAction;float pendingHeading;bool passing;
  readonly List<Transform> hoops=new();
  public bool Held=>Authority?holder:haveTarget&&target.held;
  public ulong HolderId=>Authority?PlayerId(holder):haveTarget&&target.held?target.holder:ulong.MaxValue;
  public uint ShotCount=>Authority?shotCount:haveTarget?target.shots:0;
  public bool ActionQueued=>Authority?pendingAction!=BasketballAction.None:haveTarget&&target.queued;
  public uint PassCount=>Authority?passCount:haveTarget?target.passes:0;
  public float DribblePhase=>Mathf.Repeat(Authority?dribblePhase+Mathf.Max(0,Time.time-Time.fixedTime)*dribbleCadence:haveTarget?target.dribble+(float)Math.Max(0,BasketballMotion.Clock-target.time)*target.cadence:0,1);
  public Athlete Holder=>Authority?holder:ResolveHolder();
  public bool Playing=>AppRoot.Instance&&AppRoot.Instance.Exploring&&AppRoot.Instance.SelectedSport==SportId.Basketball&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling);

  static ulong PlayerId(Athlete athlete){var net=athlete?athlete.GetComponent<NetworkAthlete>():null;return net&&net.IsSpawned?net.OwnerClientId:ulong.MaxValue;}
  bool Connected=>NetworkManager.Singleton&&NetworkManager.Singleton.IsListening;
  bool Eligible(Athlete athlete){
   if(!athlete||!athlete.gameObject.activeInHierarchy||athlete.inTransit||athlete.BasketballFreeRoam)return false;
   if(!Connected)return AppRoot.Instance&&AppRoot.Instance.LocalAthlete==athlete;
   var net=athlete.GetComponent<NetworkAthlete>();return net&&net.IsSpawned&&NetworkManager.Singleton.ConnectedClients.ContainsKey(net.OwnerClientId);
  }
  Athlete ResolveHolder(){
   if(!haveTarget||!target.held||!Connected)return null;
   foreach(var player in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
    if(player.IsPlayerObject&&player.OwnerClientId==target.holder)return player.GetComponent<Athlete>();
   return null;
  }
  public bool CanShoot(Athlete athlete)=>Playing&&athlete&&!athlete.inTransit&&!athlete.BasketballFreeRoam&&!ActionQueued&&(!athlete.BasketballMotion||!athlete.BasketballMotion.Busy)&&Held&&(Authority?holder==athlete:PlayerId(athlete)==target.holder);
  public void CancelAction(Athlete athlete){if(Authority&&holder==athlete){pendingAction=BasketballAction.None;CancelShotCharge(athlete);}}
  void ClearPossession(){approachActor=null;ClearSteals();ResetShotTracking();passing=false;if(holder&&holder.BasketballMotion)holder.BasketballMotion.ResetPose();pendingAction=BasketballAction.None;holder=null;lastShooter=null;RestoreShooterCollision();if(Body){Body.linearDamping=restDamping;RestoreFloorContacts();}}
  void RestoreFloorContacts(){shotFlying=false;Body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;}
  void RestoreShooterCollision(){if(ignoredShooter){Physics.IgnoreCollision(GetComponent<SphereCollider>(),ignoredShooter,false);ignoredShooter=null;}}
  bool Outside(Vector3 world){var p=transform.parent?transform.parent.InverseTransformPoint(world):world;return Mathf.Abs(p.x)>courtLimits.x||Mathf.Abs(p.z)>courtLimits.y;}
  public Vector3 CarryPosition(Athlete athlete){
   var motion=athlete.BasketballMotion;bool action=motion&&motion.CarryingPose;
   Vector3 offset=action?motion.BallOffset(motion.Charging||motion.Action==BasketballAction.Cancel?motion.Elapsed:Mathf.Min(motion.Elapsed,BasketballMotion.ReleaseTime(motion.Action))):BasketballMotion.DribbleOffset(DribblePhase);
   if(!action&&(athlete.Airborne||athlete.LoadingJump))offset=carryOffset;
   var origin=athlete.transform.position+Vector3.up*offset.y;
   var desired=athlete.transform.position+(action?motion.PoseFacing:athlete.transform.rotation)*offset;
   if(Physics.Raycast(desired+Vector3.up*.3f,Vector3.down,out var floor,2,1<<8,QueryTriggerInteraction.Ignore))desired.y=Mathf.Max(desired.y,floor.point.y+Radius+.004f);
   var delta=desired-origin;
   if(Physics.SphereCast(origin,Radius,delta.normalized,out var hit,delta.magnitude,1<<8,QueryTriggerInteraction.Ignore))return origin+delta.normalized*Mathf.Max(0,hit.distance-.02f);
   return desired;
  }
  void Follow(Athlete athlete){
   var motion=athlete.BasketballMotion;float spin=motion&&(motion.Busy||motion.Charging)?motion.State.ballSpin:DribblePhase;
   var position=CarryPosition(athlete);var rotation=athlete.transform.rotation*Quaternion.Euler(spin*360,0,0)*homeRotation;
   Body.position=position;Body.rotation=rotation;transform.SetPositionAndRotation(position,rotation);
  }
  void LateUpdate(){if(!Playing||!Held)return;var actor=Holder;if(actor)Follow(actor);}
  void StepInteraction(){
   // Sweep the airborne sphere against thin rim segments. Speculative CCD
   // can invent an upward contact on their inflated bounds at shot speed.
   // Return to the proven speculative mode before the first floor bounce.
   if(shotFlying&&Body.linearVelocity.y<=0&&transform.localPosition.y<1.8f)RestoreFloorContacts();
   if(ignoredShooter&&Time.time>=pickupAt&&(ignoredShooter.ClosestPoint(Body.position)-Body.position).sqrMagnitude>Radius*Radius*2)RestoreShooterCollision();
   if(ignoredStealer&&Time.time>=pickupAt&&(ignoredStealer.ClosestPoint(Body.position)-Body.position).sqrMagnitude>Radius*Radius*2)RestoreStealerCollision();
   if(holder){
    if(!Eligible(holder)||Outside(holder.transform.position)||!Finite(holder.transform.position)){ResetHome();return;}
    dribbleCadence=Mathf.Lerp(1.65f,2.15f,Mathf.Clamp01(holder.speed/7));
    if((pendingAction==BasketballAction.Shoot||chargingAthlete==holder)&&holder.BasketballMotion&&!holder.BasketballMotion.CarryingPose)dribbleCadence/=BasketballMotion.ShotTimeScale;
    if(!holder.BasketballMotion.CarryingPose)dribblePhase=Mathf.Repeat(dribblePhase+dribbleCadence*Time.fixedDeltaTime,1);
    if(chargingAthlete==holder&&pendingAction==BasketballAction.None){
     var motion=holder.BasketballMotion;float heading=Quaternion.LookRotation(Vector3.ProjectOnPlane(chargingHoop.position-holder.transform.position,Vector3.up)).eulerAngles.y;
     if(motion.Charging)motion.AimHeading(heading);
     else if(!motion.Busy&&(DribblePhase>.90f||DribblePhase<.10f||holder.Airborne||holder.LoadingJump)){var start=CarryPosition(holder);motion.Begin(BasketballAction.Charge,heading,Quaternion.Inverse(Quaternion.Euler(0,heading,0))*(start-holder.transform.position));}
    }
    if(pendingAction!=BasketballAction.None){
     var motion=holder.BasketballMotion;
     if(!motion)pendingAction=BasketballAction.None;
     else if(!motion.Busy){
      // Finish the free bounce before gathering. Never pull a ball from the
      // floor into unreachable hands halfway through a shot windup.
      if(motion.Charging||DribblePhase>.90f||DribblePhase<.10f||holder.Airborne||holder.LoadingJump){
       if(BasketballFinishRules.IsFinish(pendingAction))StartFinish(holder);
       else {var start=CarryPosition(holder);motion.Begin(pendingAction,pendingHeading,Quaternion.Inverse(Quaternion.Euler(0,pendingHeading,0))*(start-holder.transform.position));}
      }
     }
     else if((motion.Finishing?motion.FinishMotorTime:motion.Elapsed)>=BasketballMotion.ReleaseTime(pendingAction)){ReleaseAction();return;}
    }
    Follow(holder);return;
   }
   // A destroyed holder (disconnect) must clear the stale possession before
   // it can leave a kinematic ball suspended at its last carried position.
   if(!ReferenceEquals(holder,null)){ResetHome();return;}
   outsideFor=Outside(Body.position)||OutsideCourtNearFloor(Body.position)?outsideFor+Time.fixedDeltaTime:0;
   looseFor+=Time.fixedDeltaTime;
   if(outsideFor>=boundaryDelay||(looseFor>=strandedTimeout&&Body.linearVelocity.sqrMagnitude<.25f&&transform.localPosition.y>pickupHeight)){ResetHome();return;}
   if(!autoPickup||Time.time<pickupAt||Body.linearVelocity.magnitude>(passing?14:pickupMaxSpeed)||Body.linearVelocity.y>1)return;
   Athlete nearest=null;float best=float.PositiveInfinity;ulong bestId=ulong.MaxValue;
   void Consider(Athlete athlete){
    if(!Eligible(athlete)||athlete==lastShooter&&Time.time<shooterPickupAt||Outside(athlete.transform.position)||athlete.BasketballMotion&&(athlete.BasketballMotion.Challenging||athlete.BasketballMotion.Finishing))return;
    var delta=Body.position-athlete.transform.position;
    if(delta.y<-.2f||delta.y>pickupHeight)return;
    float distance=new Vector2(delta.x,delta.z).sqrMagnitude;ulong id=PlayerId(athlete);
    if(distance>pickupRadius*pickupRadius||distance>best+.0001f||Mathf.Abs(distance-best)<=.0001f&&id>=bestId&&nearest)return;
    if(Physics.Linecast(athlete.transform.position+Vector3.up*.55f,Body.position,1<<8,QueryTriggerInteraction.Ignore))return;
    nearest=athlete;best=distance;bestId=id;
   }
   if(Connected){foreach(var client in NetworkManager.Singleton.ConnectedClientsList)if(client.PlayerObject)Consider(client.PlayerObject.GetComponent<Athlete>());}
   else Consider(AppRoot.Instance.LocalAthlete);
   if(!nearest)return;
   ResetShotTracking();
   RestoreShooterCollision();RestoreStealerCollision();RestoreFloorContacts();holder=nearest;dribblePhase=0;pendingAction=BasketballAction.None;Body.linearDamping=restDamping;Body.isKinematic=false;Body.linearVelocity=Vector3.zero;Body.angularVelocity=Vector3.zero;Body.isKinematic=true;Body.detectCollisions=false;reset++;looseFor=outsideFor=0;Follow(holder);sendAt=0;
  }
  public Transform SelectHoop(Vector3 origin,float heading){
   if(!float.IsFinite(heading))return null;
   if(hoops.Count==0&&transform.parent)foreach(var t in transform.parent.GetComponentsInChildren<Transform>())if(t.name=="Hoop_North"||t.name=="Hoop_South")hoops.Add(t);
   var forward=Quaternion.Euler(0,heading,0)*Vector3.forward;Transform best=null;float score=float.PositiveInfinity;
   foreach(var hoop in hoops){if(!hoop)continue;var delta=hoop.position-origin;delta.y=0;float value=delta.sqrMagnitude+(Vector3.Dot(delta,forward)<0?10000:0);if(value<score){score=value;best=hoop;}}
   return best;
  }
  public static bool SolveShot(Vector3 start,Vector3 target,float clearance,float speedLimit,out Vector3 velocity){
   velocity=Vector3.zero;float gravity=-Physics.gravity.y;
   if(!Finite(start)||!Finite(target)||!float.IsFinite(clearance)||!float.IsFinite(speedLimit)||gravity<=0||clearance<=0||speedLimit<=0)return false;
   float apex=Mathf.Max(start.y,target.y)+clearance;
   float up=Mathf.Sqrt(2*gravity*(apex-start.y));float time=up/gravity+Mathf.Sqrt(2*(apex-target.y)/gravity);
   velocity=(target-start)/time;velocity.y=up+gravity*Time.fixedDeltaTime*.5f;
   // Preserve the arc or reject it; clamping a valid trajectory misses the hoop.
   return Finite(velocity)&&velocity.magnitude<=speedLimit;
  }
  public bool TryShoot(Athlete athlete,float heading,float power=SweetSpot){
   if(!float.IsFinite(power)||power<0||power>1)return false;
   if(!TryAction(athlete,heading,BasketballAction.Shoot))return false;
   pendingPower=power;return true;
  }
  public bool TryPass(Athlete athlete,float heading){return TryAction(athlete,heading,BasketballAction.Pass);}
  bool TryAction(Athlete athlete,float heading,BasketballAction action){
   if(!Authority||!CanShoot(athlete)||!Eligible(athlete)||!float.IsFinite(heading))return false;
   var motion=athlete.BasketballMotion;if(!motion)return false;
   var start=CarryPosition(athlete);
   if(action==BasketballAction.Shoot){var hoop=SelectHoop(start,heading);if(!hoop)return false;heading=Quaternion.LookRotation(Vector3.ProjectOnPlane(hoop.position-athlete.transform.position,Vector3.up)).eulerAngles.y;}
   pendingAction=action;pendingHeading=heading;pendingShotHoop=action==BasketballAction.Shoot?SelectHoop(start,heading):null;
   chargingAthlete=null;chargingHoop=null;
   sendAt=0;return true;
  }
  void ReleaseAction(){
   var athlete=holder;var action=pendingAction;pendingAction=BasketballAction.None;
   Vector3 start=athlete.BasketballMotion.ReleasePosition;Vector3 velocity;Transform shotHoop=null;
   if(action==BasketballAction.Shoot||BasketballFinishRules.IsFinish(action)){
    var hoop=pendingShotHoop?pendingShotHoop:SelectHoop(start,pendingHeading);if(!hoop){athlete.BasketballMotion.ResetPose();return;}
    var goal=hoop.TransformPoint(new Vector3(0,3.048f,0));float distance=Vector3.ProjectOnPlane(start-goal,Vector3.up).magnitude;
    if(BasketballFinishRules.IsFinish(action)){
     if(!FinishVelocity(athlete,action,start,hoop,out velocity)){InterruptFinish(athlete);return;}
    }else if(!SolveShot(start,goal,arcHeight+distance*arcPerMetre,maxShotSpeed,out velocity)){athlete.BasketballMotion.ResetPose();return;}
    if(!BasketballFinishRules.IsFinish(action))velocity=ApplyShotPower(velocity,pendingPower);shotHoop=hoop;
    if(velocity.magnitude>maxShotSpeed){athlete.BasketballMotion.ResetPose();return;}
   }else velocity=PassVelocity(athlete,start,pendingHeading);
   // Reject an obstructed release instead of materializing inside architecture.
   if(Physics.CheckSphere(start,Radius+.015f,1<<8,QueryTriggerInteraction.Ignore)||Physics.Linecast(athlete.transform.position+Vector3.up*1.1f,start,1<<8,QueryTriggerInteraction.Ignore)){if(BasketballFinishRules.IsFinish(action))InterruptFinish(athlete);else athlete.BasketballMotion.ResetPose();return;}
   Body.position=start;transform.position=start;holder=null;lastShooter=athlete;pickupAt=Time.time+(action==BasketballAction.Pass?.12f:releaseGrace);shooterPickupAt=Time.time+shooterGrace;looseFor=outsideFor=0;
   Body.isKinematic=false;Body.detectCollisions=true;Body.linearDamping=0;shotFlying=true;Body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
   ignoredShooter=athlete.capsule;if(ignoredShooter)Physics.IgnoreCollision(GetComponent<SphereCollider>(),ignoredShooter,true);
   Body.linearVelocity=velocity;Body.angularVelocity=Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(velocity,Vector3.up).normalized)*-backspin;
   LastLaunchVelocity=velocity;LastReleasePower=pendingPower;
   if(shotHoop)BeginAttempt(shotHoop,athlete,start);
   passing=action==BasketballAction.Pass;Body.WakeUp();reset++;if(shotHoop)shotCount++;else passCount++;sendAt=0;
  }
  Vector3 PassVelocity(Athlete passer,Vector3 start,float heading){
   var forward=Quaternion.Euler(0,heading,0)*Vector3.forward;Athlete receiver=null;float best=float.PositiveInfinity;
   if(Connected)foreach(var client in NetworkManager.Singleton.ConnectedClientsList){
    var candidate=client.PlayerObject?client.PlayerObject.GetComponent<Athlete>():null;if(candidate==passer||!Eligible(candidate)||Outside(candidate.transform.position))continue;
    var delta=Vector3.ProjectOnPlane(candidate.transform.position-start,Vector3.up);float distance=delta.magnitude;
    if(distance<1.2f||distance>12||Vector3.Dot(forward,delta.normalized)<.78f)continue;
    var point=candidate.transform.position+Vector3.up*1.05f;if(Physics.Linecast(start,point,1<<8,QueryTriggerInteraction.Ignore))continue;
    float score=distance+8*(1-Vector3.Dot(forward,delta.normalized));if(score<best){best=score;receiver=candidate;}
   }
   Vector3 target=receiver?receiver.transform.position+Vector3.up*1.05f:start+forward*7-Vector3.up*.2f;
   float duration=Mathf.Clamp(Vector3.ProjectOnPlane(target-start,Vector3.up).magnitude/8.5f,.24f,1.3f);
   if(receiver)target+=Vector3.ClampMagnitude(receiver.Motor.Velocity*duration,1.5f);
   return (target-start)/duration-Physics.gravity*(duration*.5f+Time.fixedDeltaTime*.5f);
  }
  void OnCollisionEnter(Collision collision){if(Authority){ShotContact(collision);Body.linearDamping=restDamping;passing=false;}}
 }
}
