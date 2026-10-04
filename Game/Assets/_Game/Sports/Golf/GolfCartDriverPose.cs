using System;
using UnityEngine;

namespace WhatTheFish {
 // Apply seating after the existing gait/foot/basketball presentation. Restoring
 // each frame prevents pose accumulation and immediately releases it on exit.
 [DefaultExecutionOrder(110)]
 public sealed class GolfCartDriverPose:MonoBehaviour {
  Athlete athlete;Transform[] bones;Quaternion[] rotations;Vector3[] positions;bool applied;
  Transform hips,leftThigh,leftShin,leftFoot,rightThigh,rightShin,rightFoot,leftArm,leftForearm,leftHand,rightArm,rightForearm,rightHand;
  void Awake(){
   athlete=GetComponent<Athlete>();if(!athlete||!athlete.visual)return;
   bones=athlete.visual.GetComponentsInChildren<Transform>(true);rotations=new Quaternion[bones.Length];positions=new Vector3[bones.Length];
   Transform Bone(string name)=>Array.Find(bones,t=>t.name=="mixamorig:"+name);
   hips=Bone("Hips");leftThigh=Bone("LeftUpLeg");leftShin=Bone("LeftLeg");leftFoot=Bone("LeftFoot");rightThigh=Bone("RightUpLeg");rightShin=Bone("RightLeg");rightFoot=Bone("RightFoot");
   leftArm=Bone("LeftArm");leftForearm=Bone("LeftForeArm");leftHand=Bone("LeftHand");rightArm=Bone("RightArm");rightForearm=Bone("RightForeArm");rightHand=Bone("RightHand");
  }
  void Restore(){if(!applied)return;for(int i=0;i<bones.Length;i++)if(bones[i]){bones[i].localPosition=positions[i];bones[i].localRotation=rotations[i];}applied=false;}
  void Update(){Restore();}
  void OnDisable(){Restore();}
  void LateUpdate(){
   var cart=GolfCartWorld.Driving(athlete);if(!cart||!GolfCartWorld.Allowed||!hips)return;
   for(int i=0;i<bones.Length;i++){positions[i]=bones[i].localPosition;rotations[i]=bones[i].localRotation;}applied=true;
   hips.position=cart.seat.position;
   var forward=cart.transform.forward;var right=cart.transform.right;var up=cart.transform.up;
   // Extend both legs forward into the footwell without bending the knees.
   SeatLeg(leftThigh,leftShin,leftFoot,forward,up);
   SeatLeg(rightThigh,rightShin,rightFoot,forward,up);
   var stance=athlete.GetComponent<TurnFootPlacement>();
   if(stance){leftFoot.rotation=cart.transform.rotation*stance.left.initialRotation;rightFoot.rotation=cart.transform.rotation*stance.right.initialRotation;}
   FaceFoot(leftFoot,forward);FaceFoot(rightFoot,forward);
   var wheel=cart.seat.position+forward*.53f+up*.20f;
   Solve(leftArm,leftForearm,leftHand,wheel-right*.12f,-right-up*.4f);
   Solve(rightArm,rightForearm,rightHand,wheel+right*.12f,right-up*.4f);
  }
  static void FaceFoot(Transform foot,Vector3 forward){
   if(!foot||foot.childCount==0)return;
   // Imported foot axes differ from the cart axes. Use the actual toe segment
   // so the toes point forward rather than dropping below the straight leg.
   var toe=foot.GetChild(0);var direction=toe.childCount>0?toe.GetChild(0).position-toe.position:toe.position-foot.position;
   if(direction.sqrMagnitude>.000001f)foot.rotation=Quaternion.FromToRotation(direction,forward)*foot.rotation;
  }
  static void SeatLeg(Transform thigh,Transform shin,Transform foot,Vector3 forward,Vector3 up){
   if(!thigh||!shin||!foot)return;
   // Align the actual bone segments; keep their lengths and the seated hip.
   // The short legs extend slightly upward, keeping the shoes above the cushion.
   var direction=(forward+up*.18f).normalized;
   thigh.rotation=Quaternion.FromToRotation(shin.position-thigh.position,direction)*thigh.rotation;
   shin.rotation=Quaternion.FromToRotation(foot.position-shin.position,direction)*shin.rotation;
  }
  static void Solve(Transform upper,Transform lower,Transform end,Vector3 target,Vector3 pole){
   if(!upper||!lower||!end)return;
   var a=upper.position;var b=lower.position;var c=end.position;float first=Vector3.Distance(a,b),second=Vector3.Distance(b,c);
   var delta=target-a;if(delta.sqrMagnitude<.000001f||first<.001f||second<.001f)return;
   float distance=Mathf.Clamp(delta.magnitude,Mathf.Abs(first-second)+.002f,(first+second)*.995f);var direction=delta.normalized;target=a+direction*distance;
   float along=(first*first-second*second+distance*distance)/(2*distance);
   var bend=Vector3.ProjectOnPlane(pole,direction).normalized;
   if(bend.sqrMagnitude<.01f)bend=Vector3.ProjectOnPlane(Vector3.up,direction).normalized;
   var knee=a+direction*along+bend*Mathf.Sqrt(Mathf.Max(0,first*first-along*along));
   upper.rotation=Quaternion.FromToRotation(b-a,knee-a)*upper.rotation;
   lower.rotation=Quaternion.FromToRotation(end.position-lower.position,target-lower.position)*lower.rotation;
  }
 }
}
