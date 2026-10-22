using UnityEngine;

namespace WhatTheFish {
 // Catch ordinary ground-level crossings of the visible opening. Very hard
 // shots, airborne flyovers and upward rebounds retain ordinary physics.
 public static class GolfCupRules {
  public static float Clearance(float radius,float forgiveness=1)=>Mathf.Max(0,radius-GolfBall.Radius*(1-Mathf.Clamp01(forgiveness)));
  public static float EntryHeight(float ratio=1.3f)=>GolfBall.Radius*Mathf.Clamp(ratio,.5f,1.5f);
  public static bool CanSink(Vector3 local,Vector3 velocity,float radius,float depth,float captureSpeed,float rimForgiveness=1f,float entryHeightRatio=1.3f){
   if(!float.IsFinite(local.sqrMagnitude)||!float.IsFinite(velocity.sqrMagnitude))return false;
   float clearance=Clearance(radius,rimForgiveness);
   if(clearance<=0||new Vector2(local.x,local.z).sqrMagnitude>clearance*clearance||local.y>EntryHeight(entryHeightRatio)||local.y<-depth-GolfBall.Radius||velocity.y>.15f)return false;
   // A sphere completely below the lip cannot roll across it. A shallow,
   // fast contact remains free to overshoot or rebound off the far lining.
   return local.y<-GolfBall.Radius||new Vector2(velocity.x,velocity.z).sqrMagnitude<=captureSpeed*captureSpeed;
  }
 }
 public sealed partial class GolfBall {
  GolfHoleTrigger sinkingCup;Vector3 sinkFrom;float sinkAt;
  public bool Sinking=>sinkingCup;
  internal bool TryCupCapture(GolfHoleTrigger cup){
   if(!bound||!match||!match.Authority||!Live||!match.Context||!match.State.Running||Sinking||!cup)return false;
   var entry=Body.position;
   if(!cup.Contains(entry)&&!cup.CrossingPoint(previous,entry,out entry))return false;
   var local=cup.transform.InverseTransformPoint(entry);
   if(!GolfCupRules.CanSink(local,cup.transform.InverseTransformDirection(Velocity),cup.Hole.cupRadius,cup.Hole.cupDepth,PhysicsSettings.cupCaptureSpeed,PhysicsSettings.cupRimForgiveness,PhysicsSettings.cupEntryHeightRatio))return false;
   ResetMotion();sinkingCup=cup;sinkFrom=entry;sinkAt=Time.fixedTime;
   Body.position=entry;
   Body.linearVelocity=Body.angularVelocity=Vector3.zero;Body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;
   Body.isKinematic=true;Body.useGravity=false;Body.detectCollisions=false;Trail.Clear();return true;
  }
  void CancelCupSink(){
   if(!sinkingCup)return;sinkingCup=null;
   Body.isKinematic=false;Body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
   Body.detectCollisions=bound&&match&&match.Authority&&Live;
  }
  bool AdvanceCupSink(){
   if(!sinkingCup)return false;
   float t=Mathf.Clamp01((Time.fixedTime-sinkAt)/Mathf.Max(.12f,PhysicsSettings.cupSinkSeconds));
   var floor=sinkingCup.transform.TransformPoint(Vector3.down*(sinkingCup.Hole.cupDepth-Radius-.002f));
   var position=Vector3.Lerp(sinkFrom,floor,t*t);Body.MovePosition(position);previous=position;
   if(t>=1){int number=sinkingCup.Hole.number;CancelCupSink();match.EnterHole(this,number);previous=Body.position;}
   return true;
  }
 }
}
