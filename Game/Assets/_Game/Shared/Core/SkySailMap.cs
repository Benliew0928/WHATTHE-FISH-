using System;
using UnityEngine;
using Unity.Netcode;

namespace WhatTheFish {
 public enum SkySailPhase:byte { Idle, Preparing, Boarding, Riding, Docking }
 public struct SkySailJourney:INetworkSerializable,IEquatable<SkySailJourney> {
  public uint sequence; public SkySailPhase phase; public SportId from,to; public double started; public float duration;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref sequence);s.SerializeValue(ref phase);s.SerializeValue(ref from);s.SerializeValue(ref to);s.SerializeValue(ref started);s.SerializeValue(ref duration);}
  public bool Equals(SkySailJourney b)=>sequence==b.sequence&&phase==b.phase&&from==b.from&&to==b.to&&started==b.started&&duration==b.duration;
 }
 public static class SkySailMap {
  public static readonly SportId[] Circuit={SportId.Football,SportId.Golf,SportId.Basketball,SportId.Fishing};
  public const float HangerHeight=6.5f;
  public const float StationLead=16;
  public static Vector3 Center(SportId s)=>s switch {SportId.Football=>new Vector3(-430,0,0),SportId.Golf=>new Vector3(0,0,500),SportId.Basketball=>new Vector3(430,0,0),_=>new Vector3(0,0,-340)};
  public static Vector3 Port(SportId s)=>s switch {SportId.Football=>new Vector3(118,2,0),SportId.Golf=>new Vector3(0,4.2f,-181),SportId.Basketball=>new Vector3(-68,2,0),_=>new Vector3(52,3.2f,0)};
  public static Quaternion Facing(SportId s)=>Quaternion.Euler(0,s switch {SportId.Football=>90,SportId.Golf=>180,SportId.Basketball=>270,_=>90},0);
  public static Vector3 Station(SportId s)=>Center(s)+Port(s);
  public static Vector3 Berth(SportId s)=>Station(s)+Facing(s)*new Vector3(0,.78f,1.4f);
  public static Vector3 Exit(SportId s,int index)=>Port(s)+Facing(s)*new Vector3((index%5-2)*.85f,.88f,-5.3f-index/5*1.15f);
  public static SportId Neighbor(SportId s,int direction)=>Circuit[(Array.IndexOf(Circuit,s)+direction+4)%4];
  public static bool Adjacent(SportId a,SportId b)=>a!=b&&(Neighbor(a,1)==b||Neighbor(a,-1)==b);
  public static Vector3 Tower(SportId a,SportId b){
   var berth=Berth(a);float side=Mathf.Sign(Vector3.Dot(Berth(b)-berth,Facing(a)*Vector3.right));
   // Both destinations lie north of Fishing's east-facing port. Give those
   // branches separate seaward supports instead of superimposing two towers.
   float spread=a==SportId.Fishing?(b==SportId.Football?80:30):60;
   if(a==SportId.Fishing)side=b==SportId.Football?-1:1;
   var p=berth+Facing(a)*new Vector3(side*spread,0,90);return new Vector3(p.x,0,p.z);
  }
  // All four stations are axis-aligned; this is also called for grass blades
  // on a background worker, so avoid repeated quaternion/native math calls.
  public static bool OnStationDeck(SportId sport,Vector3 islandPoint){var p=islandPoint-Port(sport);return sport==SportId.Golf?Mathf.Abs(p.x)<6.25f&&Mathf.Abs(p.z)<7.85f:Mathf.Abs(p.x)<7.85f&&Mathf.Abs(p.z)<6.25f;}
  static Vector3 Bezier(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float t){float u=1-t;return u*u*u*a+3*u*u*t*b+3*u*t*t*c+t*t*t*d;}
  // One route definition drives both the rendered wire and the cabin's wheel contact.
  public static Vector3 Path(SportId a,SportId b,float t){
   t=Mathf.Clamp01(t);var p=Berth(a);var q=Berth(b);var u=Tower(a,b)+Vector3.up*23;var v=Tower(b,a)+Vector3.up*23;
   var fa=Facing(a)*Vector3.forward;var fb=Facing(b)*Vector3.forward;
   var departure=p+fa*StationLead;var arrival=q+fb*StationLead;
   var d=(v-u).normalized;
   // Keep the entire cabin and hanger level until they have cleared the canopy.
   if(t<.04f)return Vector3.Lerp(p,departure,t/.04f);
   if(t<.16f)return Bezier(departure,departure+fa*24,u-d*43,u-d*20,(t-.04f)/.12f);
   if(t<.18f)return Vector3.Lerp(u-d*20,u,(t-.16f)/.02f);
   if(t>.96f)return Vector3.Lerp(arrival,q,(t-.96f)/.04f);
   if(t>.84f)return Bezier(v+d*20,v+d*43,arrival+fb*24,arrival,(t-.84f)/.12f);
   if(t>.82f)return Vector3.Lerp(v,v+d*20,(t-.82f)/.02f);
   float f=(t-.18f)/.64f;float sag=Mathf.Sin(Mathf.PI*f);return Vector3.Lerp(u,v,f)-Vector3.up*(6*sag*sag);
  }
  // The body turns over open water, then reverses into its berth. The bogie
  // swivels independently so its wheels stay aligned with the rendered cable.
  public static Quaternion CabinRotation(SportId a,SportId b,float t){
   if(t<=.04f)return Facing(a);if(t>=.96f)return Facing(b);
   var direction=Tangent(a,b,t);direction.y=0;
   return Quaternion.LookRotation(direction)*Quaternion.Euler(0,180*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.35f,.65f,t)),0);
  }
  public static Quaternion HangerRotation(SportId a,SportId b,float t){var d=Tangent(a,b,t);d.y=0;return Quaternion.LookRotation(d);}
  public static Vector3 Tangent(SportId a,SportId b,float t)=>(Path(a,b,Mathf.Min(1,t+.001f))-Path(a,b,Mathf.Max(0,t-.001f))).normalized;
  // Arc-length lookup makes cruise speed stable despite uneven spline parameterization.
  public static float[] Distances(SportId a,SportId b){var lengths=new float[513];var last=Path(a,b,0);for(int i=1;i<lengths.Length;i++){var p=Path(a,b,i/512f);lengths[i]=lengths[i-1]+Vector3.Distance(last,p);last=p;}return lengths;}
  public static float Parameter(float[] lengths,float progress){float distance=lengths[lengths.Length-1]*Mathf.SmoothStep(0,1,Mathf.Clamp01(progress));int i=Array.BinarySearch(lengths,distance);if(i<0)i=~i;if(i<=0)return 0;if(i>=lengths.Length)return 1;return (i-1+Mathf.InverseLerp(lengths[i-1],lengths[i],distance))/(lengths.Length-1);}
 }
}
