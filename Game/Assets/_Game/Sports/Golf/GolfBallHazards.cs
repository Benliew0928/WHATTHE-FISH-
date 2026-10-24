using UnityEngine;
namespace WhatTheFish {
 public sealed partial class GolfBall {
  bool hazardHeld,hazardReplay;
  public bool HazardReplay=>hazardReplay;
  ulong tornadoStamp;uint hazardLaunch;
  public uint HazardLaunch=>hazardLaunch;
  public bool HazardHeld=>hazardHeld;
  public ulong TornadoStamp=>tornadoStamp;
  internal bool BeginHazardHold(ulong stamp){
   if(!bound||!match||!match.Authority||!Live||Sinking||hazardHeld)return false;
   ResetMotion();hazardHeld=true;tornadoStamp=stamp;Body.linearVelocity=Body.angularVelocity=Vector3.zero;
   Body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;Body.isKinematic=true;Body.useGravity=false;Body.detectCollisions=false;Trail.Clear();return true;
  }
  internal void HoldForHazard(Vector3 position){if(hazardHeld){Body.MovePosition(position);previous=position;}}
  void CancelHazardHold(){
   if(!hazardHeld)return;hazardHeld=false;Body.isKinematic=false;Body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;Body.detectCollisions=Live&&match&&match.Authority;
  }
  internal void ReleaseHazard(Vector3 velocity){
   if(!hazardHeld)return;CancelHazardHold();ResetMotion();hazardReplay=true;if(velocity.sqrMagnitude>1)hazardLaunch++;Conditions=default;Body.linearVelocity=velocity;Body.angularVelocity=Vector3.Cross(Vector3.up,velocity)/Radius;previous=Body.position;Body.useGravity=Live;Body.WakeUp();Trail.Begin(StrikeSequence);
  }
  internal bool NudgeByHazard(Vector3 impulse){
   if(!bound||!match||!match.Authority||!Live||Sinking||hazardHeld||!float.IsFinite(impulse.sqrMagnitude))return false;
   var velocity=Velocity;ResetMotion();hazardReplay=false;hazardLaunch++;Body.useGravity=true;Body.isKinematic=false;Body.linearVelocity=velocity+impulse;Body.angularVelocity=Vector3.Cross(Vector3.up,Body.linearVelocity)/Radius;previous=Body.position;Body.WakeUp();Trail.Begin(StrikeSequence);return true;
  }
 }
}
