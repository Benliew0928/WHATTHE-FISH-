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
  // Kept serialized for older prefabs; the rules timer is OutOfBoundsSeconds.
  [HideInInspector] public float boundaryDelay=OutOfBoundsSeconds;
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
  public bool CanShoot(Athlete athlete)=>!IsInbound(athlete)&&CanPass(athlete);
  public bool CanPass(Athlete athlete)=>!PropLocked(athlete)&&!PortalBusy&&Playing&&athlete&&!athlete.inTransit&&!athlete.BasketballFreeRoam&&!ActionQueued&&(!athlete.BasketballMotion||!athlete.BasketballMotion.Busy)&&Held&&(Authority?holder==athlete:PlayerId(athlete)==target.holder);
  public void CancelAction(Athlete athlete){if(Authority&&holder==athlete){pendingAction=BasketballAction.None;passAim=default;CancelShotCharge(athlete);}}
  void ClearPossession(){ResetBoundaryTracking();CancelPropBall();passBounceArmed=false;passAim=default;ClearDefense();approachActor=null;ClearSteals();ResetShotTracking();passing=false;if(holder&&holder.BasketballMotion)holder.BasketballMotion.ResetPose();pendingAction=BasketballAction.None;holder=null;lastShooter=null;RestoreShooterCollision();if(Body){Body.linearDamping=restDamping;RestoreFloorContacts();}}
  void RestoreFloorContacts(){shotFlying=false;Body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;}
  void RestoreShooterCollision(){if(ignoredShooter){Physics.IgnoreCollision(GetComponent<SphereCollider>(),ignoredShooter,false);ignoredShooter=null;}}
  bool Outside(Vector3 world){var p=transform.parent?transform.parent.InverseTransformPoint(world):world;var recovery=Vector2.Max(courtLimits,PlayerCourtLimits+new Vector2(2.4f,2.7f));return Mathf.Abs(p.x)>recovery.x||Mathf.Abs(p.z)>recovery.y;}
  public Vector3 CarryPosition(Athlete athlete){
   var motion=athlete.BasketballMotion;bool action=motion&&motion.CarryingPose;
   Vector3 offset=action?motion.BallOffset(motion.Charging||motion.PassCharging||(motion.Action==BasketballAction.Cancel||motion.Action==BasketballAction.PassCancel)?motion.Elapsed:Mathf.Min(motion.Elapsed,BasketballMotion.ReleaseTime(motion.Action))):BasketballMotion.DribbleOffset(DribblePhase);
   if(!action&&(athlete.Airborne||athlete.LoadingJump))offset=carryOffset;
   var origin=athlete.transform.position+Vector3.up*offset.y;
   var desired=athlete.transform.position+(action?motion.PoseFacing:athlete.transform.rotation)*offset;
   if(Physics.Raycast(desired+Vector3.up*.3f,Vector3.down,out var floor,2,1<<8,QueryTriggerInteraction.Ignore))desired.y=Mathf.Max(desired.y,floor.point.y+Radius+.004f);
   var delta=desired-origin;
   if(Physics.SphereCast(origin,Radius,delta.normalized,out var hit,delta.magnitude,1<<8,QueryTriggerInteraction.Ignore))return origin+delta.normalized*Mathf.Max(0,hit.distance-.02f);
   return desired;
  }
  void Follow(Athlete athlete){
   var motion=athlete.BasketballMotion;float spin=motion&&(motion.Busy||motion.Charging||motion.PassCharging)?motion.State.ballSpin:DribblePhase;
   var position=CarryPosition(athlete);var rotation=athlete.transform.rotation*Quaternion.Euler(spin*360,0,0)*homeRotation;
   Body.position=position;Body.rotation=rotation;transform.SetPositionAndRotation(position,rotation);
  }
  void LateUpdate(){if(!Playing||!Held)return;var actor=Holder;if(actor)Follow(actor);}
  void StepInteraction(){
   if(inboundActor&&holder!=inboundActor)inboundActor=null;
   // Sweep the airborne sphere against thin rim segments. Speculative CCD
   // can invent an upward contact on their inflated bounds at shot speed.
   // Return to the proven speculative mode before the first floor bounce.
   if(shotFlying&&!passBounceArmed&&Body.linearVelocity.y<=0&&transform.localPosition.y<1.8f)RestoreFloorContacts();
   if(ignoredShooter&&Time.time>=pickupAt&&(ignoredShooter.ClosestPoint(Body.position)-Body.position).sqrMagnitude>Radius*Radius*2)RestoreShooterCollision();
   if(ignoredStealer&&Time.time>=pickupAt&&(ignoredStealer.ClosestPoint(Body.position)-Body.position).sqrMagnitude>Radius*Radius*2)RestoreStealerCollision();
   if(holder){
    if(!Eligible(holder)||!IsInbound(holder)&&Outside(holder.transform.position)||!Finite(holder.transform.position)){ResetHome();return;}
    StepInboundHold();StepPassCharge();
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
      if(motion.Charging||motion.PassCharging||DribblePhase>.90f||DribblePhase<.10f||holder.Airborne||holder.LoadingJump){
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
   if(StepBoundaryRecovery())return;
   if(!autoPickup||Time.time<pickupAt||Body.linearVelocity.magnitude>(passing?14*BasketballPassRules.SpeedMultiplier:pickupMaxSpeed)||Body.linearVelocity.y>1)return;
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
   NewDefensePlay();passBounceArmed=false;passAim=default;passing=false;RestoreShooterCollision();RestoreStealerCollision();RestoreFloorContacts();holder=nearest;dribblePhase=0;pendingAction=BasketballAction.None;Body.linearDamping=restDamping;Body.isKinematic=false;Body.linearVelocity=Vector3.zero;Body.angularVelocity=Vector3.zero;Body.isKinematic=true;Body.detectCollisions=false;reset++;looseFor=outsideFor=0;Follow(holder);BasketballAudio.Emit(BasketballSound.Catch,Body.position);sendAt=0;
  }
  public Transform SelectHoop(Vector3 origin,float heading){
   if(!float.IsFinite(heading))return null;
   if(hoops.Count==0&&transform.parent)foreach(var t in transform.parent.GetComponentsInChildren<Transform>())if(t.name=="Hoop_North"||t.name=="Hoop_South")hoops.Add(t);
   var forward=Quaternion.Euler(0,heading,0)*Vector3.forward;Transform best=null;float score=float.PositiveInfinity;
   foreach(var hoop in hoops){if(!hoop)continue;var delta=hoop.position-origin;delta.y=0;float value=delta.sqrMagnitude+(Vector3.Dot(delta,forward)<0?10000:0)+(hoop.InverseTransformPoint(origin).z<0?10000:0);if(value<score){score=value;best=hoop;}}
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
  public bool TryPass(Athlete athlete,float heading){return CommitPass(athlete,heading,0);}
  bool TryAction(Athlete athlete,float heading,BasketballAction action){
   if(!Authority||!(action==BasketballAction.Pass?CanPass(athlete):CanShoot(athlete))||!Eligible(athlete)||!float.IsFinite(heading))return false;
   var motion=athlete.BasketballMotion;if(!motion)return false;
   var start=CarryPosition(athlete);
   if(action==BasketballAction.Shoot){var hoop=SelectHoop(start,heading);if(!hoop)return false;heading=Quaternion.LookRotation(Vector3.ProjectOnPlane(hoop.position-athlete.transform.position,Vector3.up)).eulerAngles.y;}
   pendingWindow=BasketballDefenseRules.Window(ShotPressure(athlete,SelectHoop(start,heading)));pendingAction=action;pendingHeading=heading;pendingShotHoop=action==BasketballAction.Shoot?SelectHoop(start,heading):null;
   chargingAthlete=null;chargingHoop=null;
   sendAt=0;return true;
  }
  void ReleaseAction(){
   var athlete=holder;var action=pendingAction;pendingAction=BasketballAction.None;
   Vector3 start=athlete.BasketballMotion.ReleasePosition;Vector3 velocity;Transform shotHoop=null;
   if(action==BasketballAction.Shoot||BasketballFinishRules.IsFinish(action)){
    var hoop=pendingShotHoop?pendingShotHoop:SelectHoop(start,pendingHeading);if(!hoop){EndPropAim();athlete.BasketballMotion.ResetPose();return;}
    var goal=hoop.TransformPoint(new Vector3(0,3.048f,0));float distance=Vector3.ProjectOnPlane(start-goal,Vector3.up).magnitude;
    if(BasketballFinishRules.IsFinish(action)){
     if(!FinishVelocity(athlete,action,start,hoop,out velocity)){InterruptFinish(athlete);return;}
    }else if(!ShotVelocity(athlete,start,hoop,out velocity)){EndPropAim();athlete.BasketballMotion.ResetPose();return;}
    shotHoop=hoop;
    if(velocity.magnitude>maxShotSpeed){EndPropAim();athlete.BasketballMotion.ResetPose();return;}
   }else {launchedPass=BasketballPassPath.Create(start,pendingHeading,pendingPassPower,passAim.bend,PassFloor(start));velocity=launchedPass.velocity;}
   // Reject an obstructed release instead of materializing inside architecture.
   if(Physics.CheckSphere(start,Radius+.015f,1<<8,QueryTriggerInteraction.Ignore)||Physics.Linecast(athlete.transform.position+Vector3.up*1.1f,start,1<<8,QueryTriggerInteraction.Ignore)){if(BasketballFinishRules.IsFinish(action))InterruptFinish(athlete);else {EndPropAim();athlete.BasketballMotion.ResetPose();}return;}
   inboundActor=null;boundaryPrevious=CourtPoint(start);boundarySampled=true;
   Body.position=start;transform.position=start;holder=null;lastShooter=athlete;pickupAt=Time.time+(action==BasketballAction.Pass?.12f:releaseGrace);shooterPickupAt=Time.time+shooterGrace;looseFor=outsideFor=0;
   Body.isKinematic=false;Body.detectCollisions=true;Body.linearDamping=0;shotFlying=true;Body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
   ignoredShooter=athlete.capsule;if(ignoredShooter)Physics.IgnoreCollision(GetComponent<SphereCollider>(),ignoredShooter,true);
   Body.linearVelocity=velocity;Body.angularVelocity=Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(velocity,Vector3.up).normalized)*-backspin;
   LastLaunchVelocity=velocity;LastReleasePower=pendingPower;BasketballAudio.Emit(BasketballSound.Release,start);
   if(shotHoop)BeginAttempt(shotHoop,athlete,start);if(propShotQueued)StartPropFlight(start,shotHoop);
   passing=action==BasketballAction.Pass;passBounceArmed=passing&&launchedPass.Bounces;passLaunchedAt=BasketballMotion.Clock;if(passing){passAim.phase=3;passAim.origin=start;passAim.released=BasketballMotion.Clock;}else passAim=default;Body.WakeUp();reset++;if(shotHoop)shotCount++;else passCount++;sendAt=0;
  }
  void OnCollisionEnter(Collision collision){if(Authority){AudioContact(collision);guidedPropShot=false;propState.guided=false;if(BouncePassContact(collision))return;ShotContact(collision);Body.linearDamping=restDamping;passing=false;}}
 }
}
