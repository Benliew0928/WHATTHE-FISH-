using UnityEngine;

namespace WhatTheFish {
 // Stable on the host and replicas, including room bots. Local IDs never leave
 // the offline command path; zero is a valid network player ID.
 public static class TeammatePassing {
  public static ulong Id(Athlete actor)=>BasketballMatch.PlayerId(actor);
  public static Athlete Find(ulong id){foreach(var actor in Athlete.Active)if(actor&&Id(actor)==id)return actor;return null;}
  public static Vector3 Velocity(Athlete receiver){
   var velocity=receiver?Vector3.ProjectOnPlane(receiver.Motor.Velocity,Vector3.up):Vector3.zero;
   if(!float.IsFinite(velocity.x)||!float.IsFinite(velocity.z))return Vector3.zero;
   return Vector3.ClampMagnitude(velocity,16);
  }
  public static Vector3 Lead(Athlete receiver,float seconds)=>Velocity(receiver)*Mathf.Clamp(seconds,0,5);
  public static Athlete ClosestAngle(Athlete passer,float heading,System.Func<Athlete,bool> valid){
   if(!passer||!float.IsFinite(heading))return null;
   var forward=Quaternion.Euler(0,heading,0)*Vector3.forward;Athlete best=null;float angle=-2,distance=float.PositiveInfinity;
   foreach(var candidate in Athlete.Active){
    if(!valid(candidate))continue;
    var delta=Vector3.ProjectOnPlane(candidate.transform.position-passer.transform.position,Vector3.up);float dot=Vector3.Dot(delta.normalized,forward),d=delta.sqrMagnitude;
    if(dot<0)continue;
    if(dot>angle+.0001f||Mathf.Abs(dot-angle)<.0001f&&(d<distance-.0001f||Mathf.Abs(d-distance)<.0001f&&best&&Id(candidate)<Id(best))){best=candidate;angle=dot;distance=d;}
   }
   return best;
  }
 }
}
