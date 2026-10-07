using UnityEngine;

namespace WhatTheFish {
 public sealed partial class BasketballMotion {
  float defenseMotorTime,guardBlend,shufflePhase;bool defenseLaunched;
  Vector3 leftShoulder,rightShoulder;float leftReach,rightReach;
  Vector3[] defenseBasePositions;Quaternion[] defenseBaseRotations;
  public bool Guarding=>Action==BasketballAction.Guard;
  public bool Blocking=>BasketballDefenseRules.IsBlock(Action);
  public bool JumpBlocking=>Action==BasketballAction.JumpBlock;
  public bool Defensive=>Guarding||Blocking||Action==BasketballAction.GuardRecover||Action==BasketballAction.Blocked;
  void CalibrateDefense(){
   defenseBasePositions=new Vector3[bones.Length];defenseBaseRotations=new Quaternion[bones.Length];
   leftShoulder=transform.InverseTransformPoint(left.upper.position);rightShoulder=transform.InverseTransformPoint(right.upper.position);
   leftReach=Vector3.Distance(left.upper.position,left.lower.position)+Vector3.Distance(left.lower.position,left.end.position)-.018f;
   rightReach=Vector3.Distance(right.upper.position,right.lower.position)+Vector3.Distance(right.lower.position,right.end.position)-.018f;
  }
  public void Guard(float heading){
   if(!Guarding)BeginDefense(BasketballAction.Guard,heading,transform.position+transform.forward);
   var value=State;value.heading=heading%360;State=value;
  }
  public void EndGuard(){if(Guarding)BeginDefense(BasketballAction.GuardRecover,State.heading,State.contact);}
  public void BeginDefense(BasketballAction action,float heading,Vector3 point){
   Begin(action,heading,Vector3.zero);var value=State;value.contact=point;value.finishOrigin=transform.position;
   value.leftHand=Vector3.Dot(point-transform.position,Quaternion.Euler(0,heading,0)*Vector3.right)<0;
   var local=Quaternion.Inverse(Quaternion.Euler(0,heading,0))*(point-transform.position);
   local.x=Mathf.Clamp(local.x,-.42f,.42f);local.y=Mathf.Clamp(local.y,.40f,1.35f);local.z=Mathf.Clamp(local.z,.35f,.70f);
   value.gather=local;value.finishJump=Mathf.Sqrt(2*JumpMotor.Gravity*BasketballDefenseRules.JumpHeight);
   var inverse=Quaternion.Inverse(transform.rotation);
   value.leftFootStart=RigReady?transform.InverseTransformPoint(leftLeg.end.position):leftFoot;value.rightFootStart=RigReady?transform.InverseTransformPoint(rightLeg.end.position):rightFoot;
   value.leftFootRotation=RigReady?inverse*leftLeg.end.rotation:leftFootRotation;value.rightFootRotation=RigReady?inverse*rightLeg.end.rotation:rightFootRotation;State=value;
   defenseMotorTime=0;defenseLaunched=false;
  }
  public float StepDefenseJump(float dt){
   defenseMotorTime+=dt;
   if(defenseMotorTime<BasketballDefenseRules.JumpLoad)return -2*dt;
   if(!defenseLaunched){defenseLaunched=true;athlete.Jump.Launch(State.finishJump);dt=defenseMotorTime-BasketballDefenseRules.JumpLoad;}
   return athlete.Jump.Step(athlete.Grounded&&!athlete.Jump.Airborne,false,dt);
  }
  static Vector3 DefenseHip(BasketballAction action,float t){
   if(action==BasketballAction.JumpBlock){
    float load=Mathf.Sin(Mathf.PI*Mathf.Clamp01(t/BasketballDefenseRules.JumpLoad));
    float land=BasketballDefenseRules.JumpLoad+2*Mathf.Sqrt(2*JumpMotor.Gravity*BasketballDefenseRules.JumpHeight)/JumpMotor.Gravity;
    float absorb=Mathf.Sin(Mathf.PI*Mathf.Clamp01((t-land)/.22f));
    return new Vector3(0,-.07f*load-.075f*absorb,.018f*load);
   }
   if(action==BasketballAction.Block)return new Vector3(0,-.035f*Pulse(t,0,.08f,.20f,.55f),.035f*Pulse(t,.03f,.17f,.25f,.53f));
   return Vector3.zero;
  }
  Vector3 DefenseFingers(bool leftSide){
   bool raised=leftSide==State.leftHand&&(State.action==BasketballAction.JumpBlock||State.action==BasketballAction.Block&&State.gather.y>.9f);
   return new Vector3((leftSide?-1:1)*(raised?.45f:.8f),raised?1:.4f,.08f).normalized;
  }
  Vector3 ClampPalm(Vector3 local,bool leftSide,Vector3 hip){
   var shoulder=(leftSide?leftShoulder:rightShoulder)+hip;
   float reach=leftSide?leftReach:rightReach;
   // Reach belongs to the wrist, not the palm. Clamping both used to pull
   // raised hands into the face on this short-arm, large-head character.
   var arm=leftSide?left:right;
   var offset=DefenseFingers(leftSide)*Vector3.Scale(arm.palmOffset,arm.end.lossyScale).magnitude;
   return shoulder+Vector3.ClampMagnitude(local-offset-shoulder,Mathf.Max(.2f,reach))+offset;
  }
  Vector3 DefenseHandLocal(float time,bool leftSide){
   float sign=leftSide?-1:1;var rest=new Vector3(sign*.34f,.72f,.20f);bool strike=leftSide==State.leftHand;
   var action=State.action;Vector3 target;
   if(action==BasketballAction.JumpBlock){
    var low=new Vector3(sign*.34f,.68f,.13f);
    var high=new Vector3(sign*(strike?.47f:.37f),strike?1.10f:.77f,strike?.19f:.20f);
    target=Vector3.Lerp(low,high,Phase(time,.08f,.34f));
    // Lift beside the head with the other arm balancing below the shoulder.
    // A short forward press replaces the old inward swipe across the eyes.
    target+=new Vector3(0,strike?-.035f:0,strike?.08f:0)*Phase(time,.47f,.70f);
    target=Vector3.Lerp(target,rest,Phase(time,.76f,1.10f));
   }else if(action==BasketballAction.Block){
    var cock=new Vector3(sign*.36f,.74f,.19f);var goal=State.gather;
    // Keep a low swat in front of the chest. Higher contests open to the
    // outside of the cheek instead of dragging an elbow through the head.
    float high=Phase(goal.y,.85f,1.15f);
    goal.x=sign*Mathf.Lerp(Mathf.Clamp(sign*goal.x,.12f,.40f),.47f,high);
    goal.y=Mathf.Lerp(Mathf.Min(goal.y,.85f),1.10f,high);goal.z=Mathf.Lerp(goal.z,.20f,high);
    target=Vector3.Lerp(cock,strike?goal:new Vector3(sign*.37f,.73f,.17f),Phase(time,.02f,.18f));
    target+=new Vector3(0,strike?-.035f:0,strike?.035f:0)*Phase(time,.18f,.30f);
    target=Vector3.Lerp(target,rest,Phase(time,.30f,.57f));
   }else if(action==BasketballAction.Blocked){
    var start=leftSide?State.leftStart:State.rightStart;
    var recoil=new Vector3(sign*.44f,.83f,.24f);
    target=Vector3.Lerp(start,recoil,Phase(time,0,.27f));target=Vector3.Lerp(target,rest,Phase(time,.27f,.50f));
   }else if(action==BasketballAction.GuardRecover)target=leftSide?State.leftStart:State.rightStart;
   else target=new Vector3(sign*.39f,leftSide?.69f:.75f,leftSide?.21f:.24f);
   var initial=leftSide?State.leftStart:State.rightStart;
   target=Vector3.Lerp(initial,target,Phase(time,0,action==BasketballAction.JumpBlock?.12f:.16f));
   return ClampPalm(target,leftSide,DefenseHip(action,time));
  }
  // Body and hands turn together. Starts were captured in the body's frame;
  // applying the final heading immediately teleported the arm across the face.
  public Vector3 DefensePalm(float time)=>transform.position+transform.rotation*DefenseHandLocal(time,State.leftHand);
  public Vector3 DefenseShoulder=>transform.position+transform.rotation*((State.leftHand?leftShoulder:rightShoulder)+DefenseHip(State.action,Elapsed));
  void DefenseArm(Limb arm,Vector3 palm,Vector3 normal,Vector3 fingers,Vector3 pole,float blend){
   normal.Normalize();fingers=Vector3.ProjectOnPlane(fingers,normal).normalized;
   var desired=Quaternion.LookRotation(fingers,normal)*Quaternion.Inverse(Quaternion.LookRotation(arm.fingerAxis,arm.palmAxis));
   var rotation=Quaternion.Slerp(arm.end.rotation,desired,blend);
   palm=Vector3.Lerp(Palm(arm),palm,blend);
   pole=Vector3.Slerp(arm.lower.position-arm.upper.position,pole,blend);
   var wrist=palm-rotation*Vector3.Scale(arm.palmOffset,arm.end.lossyScale);
   // Blend the hand's position/orientation before solving the whole chain.
   // Independently fading upper/lower world rotations caused an elbow/wrist
   // snap at the end of recovery as the target moved behind the bent arm.
   wrist=arm.upper.position+Vector3.ClampMagnitude(wrist-arm.upper.position,Vector3.Distance(arm.upper.position,arm.lower.position)+Vector3.Distance(arm.lower.position,arm.end.position)-.018f);
   MaximumReachError=Mathf.Max(MaximumReachError,Solve(arm,wrist,pole,rotation,1));
  }
  void DefensePose(float time,float ground){
   var action=Action;bool guarding=Guarding,returning=action==BasketballAction.GuardRecover,jumping=JumpBlocking;
   float duration=Duration(action);float fade=guarding?1:1-Phase(time,duration-.22f,duration);
   float blend=weight;guardBlend=Damp(guardBlend,guarding?1:0,18);lean=Damp(lean,3+run*4,12);
   if(fade<1)DefenseLocomotion(ground);
   var facing=transform.rotation;var forward=facing*Vector3.forward;var side=facing*Vector3.right;
   var lp=leftLeg.end.position;var rp=rightLeg.end.position;var lr=leftLeg.end.rotation;var rr=rightLeg.end.rotation;
   var hip=DefenseHip(action,time);hip.y-=.065f*guardBlend;
   float strike=action==BasketballAction.Block?Pulse(time,.02f,.17f,.25f,.56f):0;
   float recoil=action==BasketballAction.Blocked?Pulse(time,0,.12f,.18f,.48f):0;
   hips.position+=facing*hip*blend-forward*(.035f*recoil*ground*blend);
   float pitch=(8*guardBlend+5*strike-7*recoil)*blend;
   Rotate(spine,side,pitch*.45f);Rotate(chest,side,pitch*.55f);Rotate(head,side,-pitch*.6f);
   // Small shoulder rotation drives the reach. The opposite arm counterbalances.
   float twist=(State.leftHand?-1:1)*-6*strike*blend;Rotate(chest,Vector3.up,twist);Rotate(head,Vector3.up,-twist*.55f);
   if(guarding||returning){
    float moving=Mathf.Clamp01(athlete.speed/1.3f);shufflePhase+=Mathf.Min(Time.deltaTime,.05f)*Mathf.Lerp(1.8f,3.4f,Mathf.Clamp01(athlete.speed/3));
    var velocity=Quaternion.Inverse(facing)*(athlete.capsule.enabled?athlete.Motor.Velocity:(transform.position-guardPrevious)/Mathf.Max(.001f,Time.deltaTime));
    guardPrevious=transform.position;velocity.y=0;velocity=Vector3.ClampMagnitude(velocity,3);
    for(int i=0;i<2;i++){
     bool leftSide=i==0;var leg=leftSide?leftLeg:rightLeg;var rest=leftSide?leftFoot:rightFoot;
     float phase=Mathf.Repeat(shufflePhase+(leftSide?0:.5f),1);bool swing=phase<.38f;
     float u=swing?phase/.38f:(phase-.38f)/.62f;
     float travel=swing?Mathf.Lerp(-.5f,.5f,Ease(u)):Mathf.Lerp(.5f,-.5f,u);
     var stride=velocity/Mathf.Lerp(1.8f,3.4f,Mathf.Clamp01(athlete.speed/3));
     var local=rest+new Vector3(leftSide?-.045f:.045f,0,leftSide?-.025f:.025f)+stride*travel*.62f*moving;
     local.x=leftSide?Mathf.Min(-.075f,local.x):Mathf.Max(.075f,local.x);
     local.y+=swing?.045f*Mathf.Sin(Mathf.PI*u)*moving:0;
     var target=transform.position+facing*local;var rotation=facing*(leftSide?leftFootRotation:rightFootRotation);
     Solve(leg,target,forward,rotation,guardBlend*weight);
    }
   }else if(jumping){
    float airborne=Phase(time,BasketballDefenseRules.JumpLoad,.34f)*(1-Phase(time,.83f,1.07f));
    for(int i=0;i<2;i++){
     bool leftSide=i==0;var rest=leftSide?leftFoot:rightFoot;var leg=leftSide?leftLeg:rightLeg;
     var initial=State.finishOrigin+Quaternion.Euler(0,State.startYaw,0)*(leftSide?State.leftFootStart:State.rightFootStart);
     var flight=transform.position+facing*(rest+new Vector3(leftSide?-.025f:.025f,.08f*airborne,-.07f*airborne));
     var lifted=initial+Vector3.up*Mathf.Max(0,transform.position.y-State.finishOrigin.y);
     var target=Vector3.Lerp(lifted,flight,Phase(time,BasketballDefenseRules.JumpLoad,.38f));
     var rotation=facing*(leftSide?leftFootRotation:rightFootRotation);Solve(leg,target,forward,rotation,blend);
    }
   }else if(action==BasketballAction.Blocked&&time<.28f){
    // A blocked dunk retains its airborne feet before settling into the
    // shared jump/landing clip. Replicate the captured pose for guests too.
    float release=Phase(time,0,.28f);
    Solve(leftLeg,Vector3.Lerp(transform.position+facing*State.leftFootStart,lp,release),forward,Quaternion.Slerp(facing*State.leftFootRotation,lr,release),weight);
    Solve(rightLeg,Vector3.Lerp(transform.position+facing*State.rightFootStart,rp,release),forward,Quaternion.Slerp(facing*State.rightFootRotation,rr,release),weight);
   }else if(ground>0&&action!=BasketballAction.Blocked){
    float planted=(1-Ease(athlete.speed/1.4f))*blend;
    var start=Quaternion.Euler(0,State.startYaw,0);
    Solve(leftLeg,Vector3.Lerp(lp,State.finishOrigin+start*State.leftFootStart,planted),forward,lr,blend);Solve(rightLeg,Vector3.Lerp(rp,State.finishOrigin+start*State.rightFootStart,planted),forward,rr,blend);
   }
   for(int i=0;i<2;i++){
    bool leftSide=i==0;var arm=leftSide?left:right;float sign=leftSide?-1:1;
    var palm=transform.position+facing*DefenseHandLocal(time,leftSide);
    var palmNormal=forward;var fingers=facing*DefenseFingers(leftSide);
    float take=Phase(time,0,.18f);var original=transform.rotation*(leftSide?State.leftRotation:State.rightRotation);
    palmNormal=Vector3.Slerp(original*arm.palmAxis,palmNormal.normalized,take);fingers=Vector3.Slerp(original*arm.fingerAxis,fingers.normalized,take);
    // Keep the elbow guide behind the forward-reaching wrist. A guide almost
    // parallel to the forearm changes bend side as the palm rises, producing
    // a visible elbow flip even when the hand itself follows a smooth curve.
    var pole=side*(sign*.35f)-forward*.75f-Vector3.up*.45f;
    DefenseArm(arm,palm,palmNormal,fingers,pole,blend);
    if(Blocking&&leftSide==State.leftHand&&BasketballDefenseRules.Active(action,time))ContactError=Vector3.Distance(Palm(arm),DefensePalm(time));
   }
   // Crossfade completed local poses. Fading individual IK solves in world
   // space can flip a knee or elbow between two competing bend directions.
   if(fade<1)for(int i=0;i<bones.Length;i++){bones[i].localPosition=Vector3.Lerp(defenseBasePositions[i],bones[i].localPosition,fade);bones[i].localRotation=Quaternion.Slerp(defenseBaseRotations[i],bones[i].localRotation,fade);}
  }
  void DefenseLocomotion(float ground){
   var lp=leftLeg.end.position;var rp=rightLeg.end.position;var lr=leftLeg.end.rotation;var rr=rightLeg.end.rotation;
   hips.position+=transform.up*((-.035f-.025f*run-.05f*brake)*ground*weight);
   float pitch=(lean-brake*11)*weight;Rotate(spine,transform.right,pitch*.45f);Rotate(chest,transform.right,pitch*.55f);Rotate(head,transform.right,-pitch*.65f);
   if(ground>0){
    float stand=stanceBlend*weight;var l=transform.TransformPoint(leftFoot+new Vector3(-.025f,0,-.025f));var r=transform.TransformPoint(rightFoot+new Vector3(.025f,0,.025f));
    Solve(leftLeg,Vector3.Lerp(lp,l,stand),transform.forward,Quaternion.Slerp(lr,transform.rotation*leftFootRotation,stand),weight);
    Solve(rightLeg,Vector3.Lerp(rp,r,stand),transform.forward,Quaternion.Slerp(rr,transform.rotation*rightFootRotation,stand),weight);
   }
   for(int i=0;i<bones.Length;i++){defenseBasePositions[i]=bones[i].localPosition;defenseBaseRotations[i]=bones[i].localRotation;bones[i].localPosition=sourcePositions[i];bones[i].localRotation=sourceRotations[i];}
  }
  Vector3 guardPrevious;
 }
}
