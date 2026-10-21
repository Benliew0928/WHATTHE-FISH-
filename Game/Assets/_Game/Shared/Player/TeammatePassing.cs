using UnityEngine;

namespace WhatTheFish {
 // Stable on the host and replicas, including room bots. Local IDs never leave
 // the offline command path; zero is a valid network player ID.
 public static class TeammatePassing {
  public static ulong Id(Athlete actor)=>BasketballMatch.PlayerId(actor);
  public static Athlete Find(ulong id){foreach(var actor in Athlete.Active)if(actor&&Id(actor)==id)return actor;return null;}
  public static Vector3 Lead(Athlete receiver,float seconds){
   var velocity=receiver?Vector3.ProjectOnPlane(receiver.Motor.Velocity,Vector3.up):Vector3.zero;
   if(!float.IsFinite(velocity.x)||!float.IsFinite(velocity.z))return Vector3.zero;
   return Vector3.ClampMagnitude(velocity*Mathf.Clamp(seconds,0,1.2f),6);
  }
 }
}
