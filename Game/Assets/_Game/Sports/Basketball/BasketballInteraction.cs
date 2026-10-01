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
  public Vector3 carryOffset=new(.34f,1.05f,.52f);
  [Min(.1f)] public float releaseGrace=.3f,shooterGrace=.9f;
  [Header("Assisted shot")]
  [Min(.5f)] public float arcHeight=1.6f;
  [Min(0)] public float arcPerMetre=.12f;
  [Min(1)] public float maxShotSpeed=24;
  [Min(0)] public float backspin=18;
  [Header("Recovery (arena local metres)")]
  public Vector2 courtLimits=new(10,17);
  [Min(.1f)] public float boundaryDelay=.75f;
  [Min(2)] public float strandedTimeout=12;

  Athlete holder,lastShooter;Collider ignoredShooter;float pickupAt,shooterPickupAt,outsideFor,looseFor,restDamping;uint shotCount;bool shotFlying;
  readonly List<Transform> hoops=new();
  public bool Held=>Authority?holder:haveTarget&&target.held;
  public ulong HolderId=>Authority?PlayerId(holder):haveTarget&&target.held?target.holder:ulong.MaxValue;
  public uint ShotCount=>Authority?shotCount:haveTarget?target.shots:0;
  public Athlete Holder=>Authority?holder:ResolveHolder();
  public bool Playing=>AppRoot.Instance&&AppRoot.Instance.Exploring&&AppRoot.Instance.SelectedSport==SportId.Basketball&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling);

  static ulong PlayerId(Athlete athlete){var net=athlete?athlete.GetComponent<NetworkAthlete>():null;return net&&net.IsSpawned?net.OwnerClientId:ulong.MaxValue;}
  bool Connected=>NetworkManager.Singleton&&NetworkManager.Singleton.IsListening;
  bool Eligible(Athlete athlete){
   if(!athlete||!athlete.gameObject.activeInHierarchy||athlete.inTransit)return false;
   if(!Connected)return AppRoot.Instance&&AppRoot.Instance.LocalAthlete==athlete;
   var net=athlete.GetComponent<NetworkAthlete>();return net&&net.IsSpawned&&NetworkManager.Singleton.ConnectedClients.ContainsKey(net.OwnerClientId);
  }
  Athlete ResolveHolder(){
   if(!haveTarget||!target.held||!Connected)return null;
   foreach(var player in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
    if(player.IsPlayerObject&&player.OwnerClientId==target.holder)return player.GetComponent<Athlete>();
   return null;
  }
  public bool CanShoot(Athlete athlete)=>Playing&&athlete&&!athlete.inTransit&&Held&&(Authority?holder==athlete:PlayerId(athlete)==target.holder);
  void ClearPossession(){holder=null;lastShooter=null;RestoreShooterCollision();if(Body){Body.linearDamping=restDamping;RestoreFloorContacts();}}
  void RestoreFloorContacts(){shotFlying=false;Body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;}
  void RestoreShooterCollision(){if(ignoredShooter){Physics.IgnoreCollision(GetComponent<SphereCollider>(),ignoredShooter,false);ignoredShooter=null;}}
  bool Outside(Vector3 world){var p=transform.parent?transform.parent.InverseTransformPoint(world):world;return Mathf.Abs(p.x)>courtLimits.x||Mathf.Abs(p.z)>courtLimits.y;}
  public Vector3 CarryPosition(Athlete athlete){
   var origin=athlete.transform.position+Vector3.up*carryOffset.y;
   var desired=athlete.transform.TransformPoint(carryOffset);var delta=desired-origin;
   if(Physics.SphereCast(origin,Radius,delta.normalized,out var hit,delta.magnitude,1<<8,QueryTriggerInteraction.Ignore))return origin+delta.normalized*Mathf.Max(0,hit.distance-.02f);
   return desired;
  }
  void Follow(Athlete athlete){var position=CarryPosition(athlete);var rotation=athlete.transform.rotation*homeRotation;Body.position=position;Body.rotation=rotation;transform.SetPositionAndRotation(position,rotation);}
  void LateUpdate(){if(!Playing||!Held)return;var actor=Holder;if(actor)Follow(actor);}
  void StepInteraction(){
   // Sweep the airborne sphere against thin rim segments. Speculative CCD
   // can invent an upward contact on their inflated bounds at shot speed.
   // Return to the proven speculative mode before the first floor bounce.
   if(shotFlying&&Body.linearVelocity.y<=0&&transform.localPosition.y<1.8f)RestoreFloorContacts();
   if(ignoredShooter&&Time.time>=pickupAt&&(ignoredShooter.ClosestPoint(Body.position)-Body.position).sqrMagnitude>Radius*Radius*2)RestoreShooterCollision();
   if(holder){
    if(!Eligible(holder)||Outside(holder.transform.position)||!Finite(holder.transform.position)){ResetHome();return;}
    Follow(holder);return;
   }
   // A destroyed holder (disconnect) must clear the stale possession before
   // it can leave a kinematic ball suspended at its last carried position.
   if(!ReferenceEquals(holder,null)){ResetHome();return;}
   outsideFor=Outside(Body.position)?outsideFor+Time.fixedDeltaTime:0;
   looseFor+=Time.fixedDeltaTime;
   if(outsideFor>=boundaryDelay||(looseFor>=strandedTimeout&&Body.linearVelocity.sqrMagnitude<.25f&&transform.localPosition.y>pickupHeight)){ResetHome();return;}
   if(!autoPickup||Time.time<pickupAt||Body.linearVelocity.magnitude>pickupMaxSpeed||Body.linearVelocity.y>1)return;
   Athlete nearest=null;float best=float.PositiveInfinity;ulong bestId=ulong.MaxValue;
   void Consider(Athlete athlete){
    if(!Eligible(athlete)||athlete==lastShooter&&Time.time<shooterPickupAt||Outside(athlete.transform.position))return;
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
   RestoreShooterCollision();RestoreFloorContacts();holder=nearest;Body.linearDamping=restDamping;Body.isKinematic=false;Body.linearVelocity=Vector3.zero;Body.angularVelocity=Vector3.zero;Body.isKinematic=true;Body.detectCollisions=false;reset++;looseFor=outsideFor=0;Follow(holder);sendAt=0;
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
  public bool TryShoot(Athlete athlete,float heading){
   if(!Authority||!CanShoot(athlete)||!Eligible(athlete)||!float.IsFinite(heading))return false;
   Vector3 start=CarryPosition(athlete);var hoop=SelectHoop(start,heading);if(!hoop)return false;
   var goal=hoop.TransformPoint(new Vector3(0,3.048f,0));float distance=Vector3.Distance(Vector3.ProjectOnPlane(start-goal,Vector3.up),Vector3.zero);
   if(!SolveShot(start,goal,arcHeight+distance*arcPerMetre,maxShotSpeed,out var velocity))return false;
   // Reject an obstructed release instead of materializing inside architecture.
   if(Physics.CheckSphere(start,Radius+.015f,1<<8,QueryTriggerInteraction.Ignore))return false;
   Follow(athlete);holder=null;lastShooter=athlete;pickupAt=Time.time+releaseGrace;shooterPickupAt=Time.time+shooterGrace;looseFor=outsideFor=0;
   Body.isKinematic=false;Body.detectCollisions=true;Body.linearDamping=0;shotFlying=true;Body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
   ignoredShooter=athlete.capsule;if(ignoredShooter)Physics.IgnoreCollision(GetComponent<SphereCollider>(),ignoredShooter,true);
   Body.linearVelocity=velocity;Body.angularVelocity=Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(velocity,Vector3.up).normalized)*-backspin;
   Body.WakeUp();reset++;shotCount++;sendAt=0;return true;
  }
  void OnCollisionEnter(Collision collision){if(Authority)Body.linearDamping=restDamping;}
 }
}
