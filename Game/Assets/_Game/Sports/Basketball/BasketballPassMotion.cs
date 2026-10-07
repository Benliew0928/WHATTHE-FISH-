using System;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public struct BasketballPassArms:INetworkSerializable,IEquatable<BasketballPassArms> {
  public Quaternion rightUpper,rightLower,rightHand,leftUpper,leftLower,leftHand;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref rightUpper);s.SerializeValue(ref rightLower);s.SerializeValue(ref rightHand);s.SerializeValue(ref leftUpper);s.SerializeValue(ref leftLower);s.SerializeValue(ref leftHand);}
  public bool Equals(BasketballPassArms b)=>rightUpper.Equals(b.rightUpper)&&rightLower.Equals(b.rightLower)&&rightHand.Equals(b.rightHand)&&leftUpper.Equals(b.leftUpper)&&leftLower.Equals(b.leftLower)&&leftHand.Equals(b.leftHand);
 }
 public sealed partial class BasketballMotion {
  public static readonly Vector3 PassPocket=new(0,.78f,.36f),PassPoint=new(0,.83f,.48f);
  public static Vector3 PassReleasePoint(float bend)=>PassPoint+Vector3.up*(bend>0?bend*.08f:bend*.12f);
  bool PassingPose=>Action==BasketballAction.Pass||PassCharging;
  uint passRecoverySequence;int passRecoveryMask;readonly Quaternion[] passRecovery=new Quaternion[6];
  BasketballPassArms CapturePassArms()=>RigReady?new BasketballPassArms{rightUpper=right.upper.localRotation,rightLower=right.lower.localRotation,rightHand=right.end.localRotation,leftUpper=left.upper.localRotation,leftLower=left.lower.localRotation,leftHand=left.end.localRotation}:default;
  void BlendPassCancel(){
   var pose=State.passArms;float u=Ease(Elapsed/.32f);
   right.upper.localRotation=Quaternion.Slerp(pose.rightUpper,right.upper.localRotation,u);right.lower.localRotation=Quaternion.Slerp(pose.rightLower,right.lower.localRotation,u);right.end.localRotation=Quaternion.Slerp(pose.rightHand,right.end.localRotation,u);
   left.upper.localRotation=Quaternion.Slerp(pose.leftUpper,left.upper.localRotation,u);left.lower.localRotation=Quaternion.Slerp(pose.leftLower,left.lower.localRotation,u);left.end.localRotation=Quaternion.Slerp(pose.leftHand,left.end.localRotation,u);
  }
  void PassArm(Limb arm,Vector3 palm,Vector3 normal,Vector3 fingers,Vector3 pole){
   // Solve a complete arm, then blend local joint poses. Independent world
   // rotation blends can make the elbow choose a different bend side mid-gather.
   var baseUpper=arm.upper.localRotation;var baseLower=arm.lower.localRotation;var baseHand=arm.end.localRotation;
   normal.Normalize();fingers=Vector3.ProjectOnPlane(fingers,normal).normalized;
   var rotation=Quaternion.LookRotation(fingers,normal)*Quaternion.Inverse(Quaternion.LookRotation(arm.fingerAxis,arm.palmAxis));
   var wrist=palm-rotation*Vector3.Scale(arm.palmOffset,arm.end.lossyScale);
   MaximumReachError=Mathf.Max(MaximumReachError,Solve(arm,wrist,pole,rotation,1));
   bool r=arm==right;var start=State.passArms;float gather=Ease(Elapsed/.22f),fade=PassCharging?1:1-Ease((Elapsed-(PassDuration-.36f))/.36f);
   if(fade<1){
    // Freeze the follow-through's offsets once recovery starts. Re-solving
    // and blending a nearly 180-degree wrist each frame can switch quaternion
    // hemisphere and visibly snap halfway back to the running animation.
    if(passRecoverySequence!=State.sequence){passRecoverySequence=State.sequence;passRecoveryMask=0;}
    int index=r?0:3,bit=r?1:2;
    if((passRecoveryMask&bit)==0){passRecovery[index]=Quaternion.Inverse(baseUpper)*arm.upper.localRotation;passRecovery[index+1]=Quaternion.Inverse(baseLower)*arm.lower.localRotation;passRecovery[index+2]=Quaternion.Inverse(baseHand)*arm.end.localRotation;passRecoveryMask|=bit;}
    arm.upper.localRotation=baseUpper*Quaternion.Slerp(Quaternion.identity,passRecovery[index],fade*weight);
    arm.lower.localRotation=baseLower*Quaternion.Slerp(Quaternion.identity,passRecovery[index+1],fade*weight);
    arm.end.localRotation=baseHand*Quaternion.Slerp(Quaternion.identity,passRecovery[index+2],fade*weight);
   }else{
    arm.upper.localRotation=Quaternion.Slerp(r?start.rightUpper:start.leftUpper,arm.upper.localRotation,gather*weight);
    arm.lower.localRotation=Quaternion.Slerp(r?start.rightLower:start.leftLower,arm.lower.localRotation,gather*weight);
    arm.end.localRotation=Quaternion.Slerp(r?start.rightHand:start.leftHand,arm.end.localRotation,gather*weight);
   }
   foreach(var finger in arm.fingers){if(finger.name.Contains("Thumb"))continue;var axis=Vector3.Cross(finger.GetChild(0).position-finger.position,normal).normalized;Rotate(finger,axis,(finger.name.EndsWith("1")?9:17)*gather*fade*weight);}
  }
 }
}
