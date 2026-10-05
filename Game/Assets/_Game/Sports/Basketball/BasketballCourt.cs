using UnityEngine;

namespace WhatTheFish {
 public sealed partial class BasketballBall {
  // Rally's painted 15.24 x 28.6512 m court, in the ball's arena frame.
  // The wider courtLimits apron belongs only to loose-ball recovery.
  public static readonly Vector2 PlayerCourtLimits=new(7.62f,14.3256f);
  bool OutsideCourtNearFloor(Vector3 world){
   var point=transform.parent?transform.parent.InverseTransformPoint(world):world;
   // An apron rebound must not strand the ball beyond the fenced player's
   // reach. Preserve airborne excursions until the ball comes back down.
   return point.y<=pickupHeight&&(Mathf.Abs(point.x)>PlayerCourtLimits.x+Radius||Mathf.Abs(point.z)>PlayerCourtLimits.y+Radius);
  }
  public bool SetFreeRoam(Athlete actor,bool enabled){
   if(!Authority||!Playing||!actor||actor.inTransit)return false;
   actor.BasketballFreeRoam=enabled;
   if(enabled){CancelShotCharge(actor);if(holder==actor)ResetHome();}
   return true;
  }
  void ResetCourtPlayers(){if(Authority)foreach(var actor in Athlete.Active)actor.BasketballFreeRoam=false;}
  bool TryPlayerCourt(Athlete actor,out Transform arena,out Vector3 point,out Vector2 limits){
   arena=transform.parent;point=Vector3.zero;limits=PlayerCourtLimits;
   if(!Playing||!actor||actor.inTransit||actor.BasketballFreeRoam||!actor.capsule||!actor.capsule.enabled)return false;
   var controller=actor.capsule;
   var center=actor.transform.TransformPoint(controller.center);
   point=arena?arena.InverseTransformPoint(center):center;
   // Arrivals at the station remain free to approach the court. Once inside,
   // walking, sprinting and jumping all meet the same player-only boundary.
   if(Mathf.Abs(point.x)>PlayerCourtLimits.x||Mathf.Abs(point.z)>PlayerCourtLimits.y)return false;
   var scale=arena?arena.lossyScale:Vector3.one;
   float radius=controller.radius*Mathf.Max(Mathf.Abs(actor.transform.lossyScale.x),Mathf.Abs(actor.transform.lossyScale.z))+controller.skinWidth;
   limits-=new Vector2(radius/Mathf.Abs(scale.x),radius/Mathf.Abs(scale.z));return true;
  }
  public Vector3 ConstrainPlayerInput(Athlete actor,Vector3 direction){
   if(!TryPlayerCourt(actor,out var arena,out var point,out var limits))return direction;
   var local=arena?arena.InverseTransformDirection(direction):direction;
   // Stop accelerating into a contacted wall. Projecting velocity alone makes
   // the motor repeatedly accelerate and lose speed, slowing tangential travel.
   if(point.x>=limits.x-.002f&&local.x>0||point.x<=-limits.x+.002f&&local.x<0)local.x=0;
   if(point.z>=limits.y-.002f&&local.z>0||point.z<=-limits.y+.002f&&local.z<0)local.z=0;
   return arena?arena.TransformDirection(local):local;
  }
  public Vector3 ConstrainPlayerMotion(Athlete actor,Vector3 displacement){
   if(!TryPlayerCourt(actor,out var arena,out var point,out var limits))return displacement;
   var target=point+(arena?arena.InverseTransformVector(displacement):displacement);
   float x=Mathf.Clamp(target.x,-limits.x,limits.x),z=Mathf.Clamp(target.z,-limits.y,limits.y);
   if(x!=target.x)actor.Motor.Constrain((arena?arena.right:Vector3.right)*-Mathf.Sign(target.x));
   if(z!=target.z)actor.Motor.Constrain((arena?arena.forward:Vector3.forward)*-Mathf.Sign(target.z));
   target.x=x;target.z=z;
   // No collider is created: the ball and camera can pass through this wall.
   return arena?arena.TransformVector(target-point):target-point;
  }
 }
}
