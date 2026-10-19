using System;
using UnityEngine;

namespace WhatTheFish {
 // Serialized map data remains compatible with the existing five-hole assets.
 public sealed class GolfCourse : MonoBehaviour {
  public GolfHole[] holes;
  public GolfGreen[] greens=Array.Empty<GolfGreen>();
  public GolfFairway[] fairways=Array.Empty<GolfFairway>();
  public GolfSandPatch[] sandPatches=Array.Empty<GolfSandPatch>();
  public GolfSandPatch[] roughPatches=Array.Empty<GolfSandPatch>();
  public GolfWaterPatch[] waterPatches=Array.Empty<GolfWaterPatch>();
  public GolfTeeSite[] teeSites=Array.Empty<GolfTeeSite>();
  public bool TouchesWater(Vector3 from,Vector3 to,float radius){
   var a=transform.InverseTransformPoint(from);var b=transform.InverseTransformPoint(to);
   float localRadius=radius/Mathf.Min(transform.lossyScale.x,transform.lossyScale.z);
   foreach(var water in waterPatches)if(water.Touches(a,b,localRadius))return true;return false;
  }
  public bool SafeDryPosition(Vector3 world,float margin=1){
   var p=transform.InverseTransformPoint(world);foreach(var water in waterPatches)if(water.Contains(p.x,p.z,margin))return false;return true;
  }
  public float RollingResistance(Vector3 world,GolfBallPhysicsSettings settings){
   var p=transform.InverseTransformPoint(world);
   float fairway=24*Mathf.Sin((p.z+135)/55)-5;
   float width=24+5*Mathf.Cos(p.z/38)+6*Mathf.Exp(-Mathf.Pow((p.z+110)/40,2));
   float rough=Mathf.SmoothStep(0,1,Mathf.InverseLerp(width-1,width+3,Mathf.Abs(p.x-fairway)));
   float resistance=Mathf.Lerp(settings.fairwayResistance,settings.roughResistance,rough);
   foreach(var fair in fairways)if(fair.Contains(p.x,p.z))resistance=settings.fairwayResistance;
   foreach(var roughPatch in roughPatches)if(roughPatch.Contains(p.x,p.z))resistance=settings.roughResistance;
   resistance=Mathf.Lerp(resistance,settings.sandResistance,MobileIslandGrass.GolfSand(p.x,p.z));
   foreach(var sand in sandPatches)if(sand.Contains(p.x,p.z))resistance=settings.sandResistance;
   foreach(var green in greens){
    float distance=Vector2.Distance(new Vector2(p.x,p.z),new Vector2(green.centre.x,green.centre.z));
    float fringe=Mathf.SmoothStep(0,1,Mathf.InverseLerp(green.radius,green.radius+2,distance));
    resistance=Mathf.Lerp(settings.greenResistance,resistance,fringe);
   }
   foreach(var site in teeSites){int surface=site.Surface(p.x,p.z);if(surface>0)resistance=surface==2?settings.sandResistance:surface==3?settings.roughResistance:settings.fairwayResistance;if(site.GroundWeight(p.x,p.z)>.95f)resistance=settings.fairwayResistance;}
   return resistance;
  }
  void OnEnable(){if(Application.isPlaying&&!GetComponent<GolfMatchManager>())gameObject.AddComponent<GolfMatchManager>();}
 }
 [Serializable] public struct GolfFairway {
  public int hole;public Vector3[] points;public float width;public bool alternative;
  public bool Contains(float x,float z){
   if(points==null)return false;var p=new Vector2(x,z);
   for(int i=1;i<points.Length;i++){var a=new Vector2(points[i-1].x,points[i-1].z);var b=new Vector2(points[i].x,points[i].z);var d=b-a;float t=d.sqrMagnitude>0?Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude):0;if(Vector2.Distance(p,a+d*t)<width*.5f)return true;}
   return false;
  }
 }
 [Serializable] public struct GolfSandPatch {
  public string name;public Vector3 centre;public Vector2 radii;public float yaw;
  public bool Contains(float x,float z){float a=yaw*Mathf.Deg2Rad,dx=x-centre.x,dz=z-centre.z;float u=(dx*Mathf.Cos(a)+dz*Mathf.Sin(a))/radii.x,v=(-dx*Mathf.Sin(a)+dz*Mathf.Cos(a))/radii.y;return u*u+v*v<1;}
 }
 // Island-local metres. Pure value data may be copied to the mobile grass worker.
 [Serializable] public struct GolfGreen {
  public Vector3 centre;
  public Vector2 slope;
  public float radius,blend;
  public float Height(float x,float z,float original){
   float dx=x-centre.x,dz=z-centre.z;
   float inner=radius*.5f;
   float t=Mathf.Clamp01((Mathf.Sqrt(dx*dx+dz*dz)-inner)/(radius+blend-inner));
   // Quintic falloff joins the original hillside with zero first/second derivative.
   float weight=1-t*t*t*(t*(t*6-15)+10);
   return Mathf.Lerp(original,centre.y+slope.x*dx+slope.y*dz,weight);
  }
  public bool ClearsGrass(float x,float z){
   float dx=x-centre.x,dz=z-centre.z;
   // A restrained irregular fringe avoids a conspicuous circular clearing.
   float edge=radius-.8f+.35f*Mathf.Sin(Mathf.Atan2(dz,dx)*3+.7f);
   return dx*dx+dz*dz<edge*edge;
  }
 }
 [Serializable] public sealed class GolfHole {
  public int number,par;
  public string title;
  public Transform cup,flag,tee;
  public float cupRadius,cupDepth,greenRadius;
 }
}
