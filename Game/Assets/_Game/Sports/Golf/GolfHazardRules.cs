using UnityEngine;
namespace WhatTheFish {
 public static class GolfHazardRules {
  public const float TornadoGrowthSeconds=8,TornadoRoamRadius=28;
  public static float TornadoGrowth(double age)=>Mathf.Lerp(.18f,1,Mathf.SmoothStep(0,1,Mathf.Clamp01((float)age/TornadoGrowthSeconds)));
  public const float CrabPushCooldown=5;
  public const float TornadoRadius=3.5f,CrabRadius=.68f,CupClearance=3.5f,CarrySeconds=1.1f;
  public static bool IsHazard(GolfEventKind kind)=>kind is GolfEventKind.Tornado or GolfEventKind.CrabCrossing;
  public static ulong Stamp(uint round,uint sequence)=>((ulong)round<<32)|sequence;
  static uint Hash(uint x){unchecked{x^=x>>16;x*=0x7feb352d;x^=x>>15;x*=0x846ca68b;return x^(x>>16);}}
  public static float Phase(GolfEventSnapshot e,int lane)=>(Hash(unchecked(e.seed+e.sequence*7919+(uint)lane*3989))&65535)*(Mathf.PI*2/65536);
  // Same seed and room clock on host and clients; no per-client random walk.
  // Preserve the 8 m / 2 m patrol amplitudes; peak speeds are 6 m/s and 5 m/s.
  public static Vector3 Offset(GolfEventKind kind,GolfEventSnapshot e,int lane,double now,Vector3 forward){
   var right=Vector3.Cross(Vector3.up,forward).normalized;float age=(float)(now-e.start),phase=Phase(e,lane);
   return kind==GolfEventKind.Tornado?right*Mathf.Sin(age*.75f+phase)*8+forward*Mathf.Sin(age*2.5f+phase)*2:right*Mathf.Sin(age*.62f+phase)*7+forward*Mathf.Sin(age*1.24f+phase)*.35f;
  }
  public static Vector3 CrabVelocity(GolfEventSnapshot e,int lane,double now,Vector3 forward){float t=(float)(now-e.start);return Vector3.Cross(Vector3.up,forward).normalized*(4.34f*Mathf.Cos(t*.62f+Phase(e,lane)));}
  public static Vector3 TossVelocity(Vector3 from,Vector3 to,float seconds=1.35f)=>(to-from-Physics.gravity*(.5f*seconds*seconds))/seconds;
  public static float TossDistance(GolfEventSnapshot e,ulong owner)=>14+(Hash(unchecked(e.seed+e.sequence+(uint)owner))&65535)*(6f/65535);
 }
}
