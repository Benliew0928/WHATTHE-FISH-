using System;
using UnityEngine;

namespace WhatTheFish {
 // Serialized map data remains compatible with the existing five-hole assets.
 public sealed class GolfCourse : MonoBehaviour {
  public GolfHole[] holes;
  public GolfGreen[] greens=Array.Empty<GolfGreen>();
  public float RollingResistance(Vector3 world,GolfBallPhysicsSettings settings){
   var p=transform.InverseTransformPoint(world);
   float fairway=24*Mathf.Sin((p.z+135)/55)-5;
   float width=24+5*Mathf.Cos(p.z/38)+6*Mathf.Exp(-Mathf.Pow((p.z+110)/40,2));
   float rough=Mathf.SmoothStep(0,1,Mathf.InverseLerp(width-1,width+3,Mathf.Abs(p.x-fairway)));
   float resistance=Mathf.Lerp(settings.fairwayResistance,settings.roughResistance,rough);
   resistance=Mathf.Lerp(resistance,settings.sandResistance,MobileIslandGrass.GolfSand(p.x,p.z));
   foreach(var green in greens){
    float distance=Vector2.Distance(new Vector2(p.x,p.z),new Vector2(green.centre.x,green.centre.z));
    float fringe=Mathf.SmoothStep(0,1,Mathf.InverseLerp(green.radius,green.radius+2,distance));
    resistance=Mathf.Lerp(settings.greenResistance,resistance,fringe);
   }
   return resistance;
  }
  void OnEnable(){if(Application.isPlaying&&!GetComponent<GolfMatchManager>())gameObject.AddComponent<GolfMatchManager>();}
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
