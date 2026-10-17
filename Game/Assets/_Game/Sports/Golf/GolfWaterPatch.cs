using System;
using UnityEngine;

namespace WhatTheFish {
 // Local horizontal water plane; bank art and detection share the same footprint.
 [Serializable] public struct GolfWaterPatch {
  public string name;public int hole;public Vector3 centre;public Vector2 radii;public float bankScale;
  public bool Contains(float x,float z,float margin=0){
   float dx=(x-centre.x)/(radii.x+margin),dz=(z-centre.z)/(radii.y+margin);return dx*dx+dz*dz<=1;
  }
  // Swept sphere/cylinder intersection also covers kinematic guided shots and
  // fast rolling between physics frames. Flying above the plane remains safe.
  public bool Touches(Vector3 from,Vector3 to,float radius){
   float lo=0,hi=1,dy=to.y-from.y,maxY=centre.y+.04f+radius;
   // The existing ground closes the pool bottom. Any sphere inside the wet
   // footprint below its surface is wet, including the lower side of a slope.
   if(Mathf.Abs(dy)<1e-7f){if(from.y>maxY)return false;}
   else{float crossing=(maxY-from.y)/dy;if(dy>0)hi=Mathf.Min(hi,crossing);else lo=Mathf.Max(lo,crossing);if(lo>hi)return false;}
   float rx=radii.x+radius,rz=radii.y+radius;
   var a2=new Vector2((from.x-centre.x)/rx,(from.z-centre.z)/rz);var delta=new Vector2((to.x-from.x)/rx,(to.z-from.z)/rz);
   float t=delta.sqrMagnitude>1e-12f?Mathf.Clamp(-Vector2.Dot(a2,delta)/delta.sqrMagnitude,lo,hi):lo;
   return (a2+delta*t).sqrMagnitude<=1;
  }
 }
}
