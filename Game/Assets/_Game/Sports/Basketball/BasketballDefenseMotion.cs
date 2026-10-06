using UnityEngine;

namespace WhatTheFish {
 public sealed partial class BasketballMotion {
  float defenseMotorTime,guardBlend,shufflePhase;bool defenseLaunched;Vector3 defenseLeftFoot,defenseRightFoot;
  Vector3 leftShoulder,rightShoulder;float leftReach,rightReach;
  public bool Guarding=>Action==BasketballAction.Guard;
  public bool Blocking=>BasketballDefenseRules.IsBlock(Action);
  public bool JumpBlocking=>Action==BasketballAction.JumpBlock;
  public bool Defensive=>Guarding||Blocking||Action==BasketballAction.GuardRecover||Action==BasketballAction.Blocked;
  void CalibrateDefense(){
   leftShoulder=transform.InverseTransformPoint(left.upper.position);rightShoulder=transform.InverseTransformPoint(right.upper.position);
   leftReach=Vector3.Distance(left.upper.position,left.lower.position)+Vector3.Distance(left.lower.position,left.end.position)-.035f;
   rightReach=Vector3.Distance(right.upper.position,right.lower.position)+Vector3.Distance(right.lower.position,right.end.position)-.035f;
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
   value.gather=local;value.finishJump=Mathf.Sqrt(2*JumpMotor.Gravity*BasketballDefenseRules.JumpHeight);State=value;
   defenseMotorTime=0;defenseLaunched=false;
   defenseLeftFoot=RigReady?leftLeg.end.position:transform.TransformPoint(leftFoot);defenseRightFoot=RigReady?rightLeg.end.position:transform.TransformPoint(rightFoot);
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
  Vector3 ClampPalm(Vector3 local,bool leftSide,Vector3 hip){
   var shoulder=(leftSide?leftShoulder:rightShoulder)+hip;
   float reach=leftSide?leftReach:rightReach;
   return shoulder+Vector3.ClampMagnitude(local-shoulder,Mathf.Max(.2f,reach));
  }
  Vector3 DefenseHandLocal(float time,bool leftSide){
   float sign=leftSide?-1:1;var rest=new Vector3(sign*.27f,.84f,.25f);bool strike=leftSide==State.leftHand;
   var action=State.action;Vector3 target;
   if(action==BasketballAction.JumpBlock){
    var low=new Vector3(sign*.23f,.69f,.13f);
    var high=new Vector3(sign*(strike?.23f:.30f),strike?1.58f:1.02f,strike?.44f:.26f);
    target=Vector3.Lerp(low,high,Phase(time,.07f,.30f));
    target+=new Vector3(strike?-sign*.10f:0,strike?-.10f:0,strike?.14f:0)*Phase(time,.47f,.70f);
    target=Vector3.Lerp(target,rest,Phase(time,.83f,1.10f));
   }else if(action==BasketballAction.Block){
    var cock=new Vector3(sign*.31f,.88f,.18f);var goal=strike?State.gather:new Vector3(sign*.36f,.98f,.19f);
    target=Vector3.Lerp(cock,goal,Phase(time,.04f,.18f));
    target+=new Vector3(strike?-sign*.12f:0,strike?-.045f:0,strike?.045f:0)*Phase(time,.18f,.30f);
    target=Vector3.Lerp(target,rest,Phase(time,.31f,.56f));
   }else if(action==BasketballAction.Blocked){
    var start=leftSide?State.leftStart:State.rightStart;
    var recoil=new Vector3(sign*.31f,Mathf.Clamp(start.y-.10f,.68f,1.23f),.20f);
    target=Vector3.Lerp(start,recoil,Phase(time,0,.19f));target=Vector3.Lerp(target,rest,Phase(time,.24f,.48f));
   }else target=new Vector3(sign*.40f,.98f,.23f);
   var initial=leftSide?State.leftStart:State.rightStart;
   target=Vector3.Lerp(initial,target,Phase(time,0,action==BasketballAction.JumpBlock?.09f:.10f));
   return ClampPalm(target,leftSide,DefenseHip(action,time));
  }
  public Vector3 DefensePalm(float time)=>transform.position+Facing*DefenseHandLocal(time,State.leftHand);
  public Vector3 DefenseShoulder=>transform.position+Facing*((State.leftHand?leftShoulder:rightShoulder)+DefenseHip(State.action,Elapsed));
  void DefensePose(float time,float ground){
   var action=Action;bool guarding=Guarding,returning=action==BasketballAction.GuardRecover,jumping=JumpBlocking;
   float duration=Duration(action);float fade=guarding?1:1-Phase(time,duration-.22f,duration);
   float blend=fade*weight;guardBlend=Damp(guardBlend,guarding?1:0,18);
   var facing=guarding?transform.rotation:Facing;var forward=facing*Vector3.forward;var side=facing*Vector3.right;
   var lp=leftLeg.end.position;var rp=rightLeg.end.position;var lr=leftLeg.end.rotation;var rr=rightLeg.end.rotation;
   var hip=DefenseHip(action,time);hip.y-=.065f*guardBlend;
   float strike=action==BasketballAction.Block?Pulse(time,.02f,.17f,.25f,.56f):0;
   float recoil=action==BasketballAction.Blocked?Pulse(time,0,.12f,.18f,.48f):0;
   hips.position+=facing*hip*blend-forward*(.035f*recoil*ground*blend);
   float pitch=8*guardBlend+5*strike-7*recoil;
   Rotate(spine,side,pitch*.45f*blend);Rotate(chest,side,pitch*.55f*blend);Rotate(head,side,-pitch*.6f*blend);
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
     var initial=athlete.capsule.enabled?(leftSide?defenseLeftFoot:defenseRightFoot):State.finishOrigin+Quaternion.Euler(0,State.startYaw,0)*rest;
     var target=time<BasketballDefenseRules.JumpLoad?initial:transform.position+facing*(rest+new Vector3(leftSide?-.025f:.025f,.08f*airborne,-.07f*airborne));
     var rotation=facing*(leftSide?leftFootRotation:rightFootRotation);Solve(leg,target,forward,rotation,blend);
    }
   }else if(ground>0){
    float planted=(1-Ease(athlete.speed/1.4f))*blend;
    Solve(leftLeg,Vector3.Lerp(lp,defenseLeftFoot,planted),forward,lr,blend);Solve(rightLeg,Vector3.Lerp(rp,defenseRightFoot,planted),forward,rr,blend);
   }
   for(int i=0;i<2;i++){
    bool leftSide=i==0;var arm=leftSide?left:right;float sign=leftSide?-1:1;
    var palm=transform.position+facing*DefenseHandLocal(time,leftSide);
    var normal=forward+(jumping?Vector3.down*.22f:Vector3.down*.05f);var fingers=Vector3.up+forward*.10f;
    float take=Phase(time,0,.10f);var original=transform.rotation*(leftSide?State.leftRotation:State.rightRotation);
    normal=Vector3.Slerp(original*arm.palmAxis,normal.normalized,take);fingers=Vector3.Slerp(original*arm.fingerAxis,fingers.normalized,take);
    Arm(arm,palm,normal,fingers,side*(sign*.65f)-forward*.08f-Vector3.up*.18f,blend);
    if(Blocking&&leftSide==State.leftHand&&BasketballDefenseRules.Active(action,time))ContactError=Vector3.Distance(Palm(arm),DefensePalm(time));
   }
  }
  Vector3 guardPrevious;
 }
}
