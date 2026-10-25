using System;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public struct FootballWallState:INetworkSerializable,IEquatable<FootballWallState> {
  public bool active;public Vector3 position;public float heading;public double until;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref active);s.SerializeValue(ref position);s.SerializeValue(ref heading);s.SerializeValue(ref until);}
  public bool Equals(FootballWallState b)=>active==b.active&&position==b.position&&heading==b.heading&&until==b.until;
  public Quaternion Rotation=>Quaternion.Euler(0,heading,0);
  public Vector3 Centre=>position+Vector3.up*(FootballPropRules.WallHeight*.5f);
 }
 public struct FootballPropsState:INetworkSerializable,IEquatable<FootballPropsState> {
  public SportsPropSpawn pickup;public SportsPropActivation activation;public double turboA,turboB,portalAt;public FootballWallState wallA,wallB;
  public bool live,transferring;public ulong receiver;public uint portalSequence;public Vector3 source,destination;public byte portalOutcome;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {
   s.SerializeValue(ref activation);s.SerializeValue(ref live);s.SerializeValue(ref pickup);s.SerializeValue(ref turboA);s.SerializeValue(ref turboB);s.SerializeValue(ref wallA);s.SerializeValue(ref wallB);
   s.SerializeValue(ref transferring);s.SerializeValue(ref receiver);s.SerializeValue(ref portalAt);s.SerializeValue(ref portalSequence);s.SerializeValue(ref source);s.SerializeValue(ref destination);s.SerializeValue(ref portalOutcome);
  }
  public bool Equals(FootballPropsState b)=>activation.Equals(b.activation)&&live==b.live&&pickup.Equals(b.pickup)&&turboA==b.turboA&&turboB==b.turboB&&wallA.Equals(b.wallA)&&wallB.Equals(b.wallB)&&transferring==b.transferring&&receiver==b.receiver&&portalAt==b.portalAt&&portalSequence==b.portalSequence&&source==b.source&&destination==b.destination&&portalOutcome==b.portalOutcome;
 }
 public static class FootballPropRules {
  public const float WallWidth=6,WallHeight=5.25f,WallThickness=.3f,WallSeconds=15,PlacementDistance=2;
  public static Vector3 WallHalfExtents=>new(WallWidth*.5f,WallHeight*.5f,WallThickness*.5f);
  public static bool Kind(SportsProp k)=>k==SportsProp.Turbo||k==SportsProp.Portal||k==SportsProp.Wall;
  public static string Name(SportsProp k)=>k==SportsProp.Turbo?"Team Turbo":k==SportsProp.Portal?"Portal Steal":k==SportsProp.Wall?"Wall":"Find a prop";
  public static SportsProp NextKind(SportsProp previous,int choice)=>SportsAbilityRules.NextKind(previous,choice,2);
  public static bool OwnHalf(float z,float centre,FootballTeam team)=>team==FootballTeam.A?z<centre-.35f:team==FootballTeam.B&&z>centre+.35f;
  public static bool Contains(FootballWallState w,Vector3 point,float radius){var p=Quaternion.Inverse(w.Rotation)*(point-w.Centre);var e=WallHalfExtents;var q=new Vector3(Mathf.Clamp(p.x,-e.x,e.x),Mathf.Clamp(p.y,-e.y,e.y),Mathf.Clamp(p.z,-e.z,e.z));return (p-q).sqrMagnitude<=radius*radius;}
  // Continuous ball-centre sweep against a radius-expanded OBB. Conservative at
  // the tiny rounded corners; it cannot tunnel at high speed or a render hitch.
  public static bool Sweep(FootballWallState wall,Vector3 from,Vector3 to,float radius,out float fraction,out Vector3 normal){
   fraction=0;normal=Vector3.zero;if(!SportsAbilityRules.Finite(from)||!SportsAbilityRules.Finite(to)||!float.IsFinite(radius)||radius<0)return false;
   var rotation=wall.Rotation;var inv=Quaternion.Inverse(rotation);var p=inv*(from-wall.Centre);var d=inv*(to-from);var e=WallHalfExtents+Vector3.one*radius;
   float enter=0,exit=1;Vector3 n=Vector3.zero;bool inside=true;
   for(int axis=0;axis<3;axis++){
    if(Mathf.Abs(p[axis])>e[axis]+.00001f)inside=false;
    if(Mathf.Abs(d[axis])<.0000001f){if(Mathf.Abs(p[axis])>e[axis])return false;continue;}
    float a=(-e[axis]-p[axis])/d[axis],b=(e[axis]-p[axis])/d[axis];float sign=-1;
    if(a>b){(a,b)=(b,a);sign=1;}if(a>enter){enter=a;n=Vector3.zero;n[axis]=sign;}exit=Mathf.Min(exit,b);if(enter>exit)return false;
   }
   if(inside){
    int axis=0;float depth=e.x-Mathf.Abs(p.x);for(int i=1;i<3;i++)if(e[i]-Mathf.Abs(p[i])<depth){axis=i;depth=e[i]-Mathf.Abs(p[i]);}
    n=Vector3.zero;n[axis]=p[axis]>=0?1:-1;
    // A touching ball moving away must be able to leave the wall.
    if(Vector3.Dot(d,n)>=0)return false;
   }
   if(enter<0||enter>1||exit<0||n==Vector3.zero)return false;fraction=enter;normal=rotation*n;return true;
  }
  public static bool Overlap(FootballWallState a,FootballWallState b,float padding=.05f){
   var ar=a.Rotation*Vector3.right;var af=a.Rotation*Vector3.forward;var br=b.Rotation*Vector3.right;var bf=b.Rotation*Vector3.forward;var delta=b.position-a.position;
   return Axis(ar)&&Axis(af)&&Axis(br)&&Axis(bf);
   bool Axis(Vector3 axis){float ra=Mathf.Abs(Vector3.Dot(axis,ar))*WallWidth*.5f+Mathf.Abs(Vector3.Dot(axis,af))*WallThickness*.5f;float rb=Mathf.Abs(Vector3.Dot(axis,br))*WallWidth*.5f+Mathf.Abs(Vector3.Dot(axis,bf))*WallThickness*.5f;return Mathf.Abs(Vector3.Dot(delta,axis))<ra+rb+padding;}
  }
 }
}
