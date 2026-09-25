using System;
using UnityEngine;

namespace WhatTheFish {
 // Turn-only two-bone IK. Targets use the same footprint schedule as Blender.
 public sealed class TurnFootPlacement:MonoBehaviour {
  [Serializable] public class Leg {
   public Transform thigh,knee,ankle;
   public Vector3 initialPosition;
   public Quaternion initialRotation;
  }
  public Transform hips;public Vector3 hipOffset;
  public Leg left=new Leg(),right=new Leg();
  public float MaximumTargetError {get;private set;}
  public static void FootStep(float t,bool leading,out float fraction,out float height){
   fraction=0;height=0;
   if(leading){
    if(t<.12f)return;
    if(t<.35f){float u=(t-.12f)/.23f;fraction=.6f*LocomotionMotor.Ease(u);height=.065f*Mathf.Pow(Mathf.Sin(Mathf.PI*u),2);return;}
    if(t<.70f){fraction=.6f;return;}
    if(t<.96f){float u=(t-.70f)/.26f;fraction=.6f+.4f*LocomotionMotor.Ease(u);height=.045f*Mathf.Pow(Mathf.Sin(Mathf.PI*u),2);return;}
    fraction=1;return;
   }
   if(t<.35f)return;
   if(t<.70f){float u=(t-.35f)/.35f;fraction=LocomotionMotor.Ease(u);height=.075f*Mathf.Pow(Mathf.Sin(Mathf.PI*u),2);return;}
   fraction=1;
  }
  Vector3 Target(Leg leg,float t,float angle,float start,bool leading,out Quaternion rotation){
   FootStep(t,leading,out float fraction,out float height);
   var yaw=Quaternion.Euler(0,start+angle*fraction,0);
   rotation=yaw*leg.initialRotation;
   return transform.position+yaw*leg.initialPosition+Vector3.up*height;
  }
  public void Apply(float progress,float angle,float startYaw){
   if(!hips||!left.ankle||!right.ankle)return;
   float weight=LocomotionMotor.Ease(progress/.15f);
   var l=Target(left,progress,angle,startYaw,angle<0,out var lr);
   var r=Target(right,progress,angle,startYaw,angle>0,out var rr);
   var center=(l+r)*.5f+transform.rotation*hipOffset;
   var hip=hips.position;center.y=hip.y;hips.position=Vector3.Lerp(hip,center,weight);
   Solve(left,Vector3.Lerp(left.ankle.position,l,weight),Quaternion.Slerp(left.ankle.rotation,lr,weight));
   Solve(right,Vector3.Lerp(right.ankle.position,r,weight),Quaternion.Slerp(right.ankle.rotation,rr,weight));
   MaximumTargetError=Mathf.Max(Vector3.Distance(left.ankle.position,l),Vector3.Distance(right.ankle.position,r));
  }
  void Solve(Leg leg,Vector3 target,Quaternion footRotation){
   Vector3 a=leg.thigh.position,b=leg.knee.position,c=leg.ankle.position;
   float upper=Vector3.Distance(a,b),lower=Vector3.Distance(b,c);
   Vector3 direction=target-a;float distance=Mathf.Clamp(direction.magnitude,.001f,upper+lower-.00001f);direction.Normalize();
   Vector3 bend=Vector3.ProjectOnPlane(transform.forward,direction).normalized;
   if(bend.sqrMagnitude<.01f)bend=Vector3.ProjectOnPlane(transform.right,direction).normalized;
   float along=(upper*upper-lower*lower+distance*distance)/(2*distance);
   var knee=a+direction*along+bend*Mathf.Sqrt(Mathf.Max(0,upper*upper-along*along));
   leg.thigh.rotation=Quaternion.FromToRotation(b-a,knee-a)*leg.thigh.rotation;
   leg.knee.rotation=Quaternion.FromToRotation(leg.ankle.position-leg.knee.position,target-leg.knee.position)*leg.knee.rotation;
   leg.ankle.rotation=footRotation;
  }
 }
}
