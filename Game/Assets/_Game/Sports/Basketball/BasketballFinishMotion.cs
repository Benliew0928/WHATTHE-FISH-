using UnityEngine;

namespace WhatTheFish {
 public sealed partial class BasketballMotion {
  float finishMotorTime;bool finishLaunched;Vector3 finishLeftFoot,finishRightFoot;
  public bool Finishing=>BasketballFinishRules.IsFinish(Action);
  public float FinishMotorTime=>finishMotorTime;
  public void BeginFinish(BasketballMotionState plan,Vector3 gather){
   Begin(plan.action,plan.heading,gather);var value=State;
   value.leftHand=plan.leftHand;value.finishOrigin=plan.finishOrigin;value.finishTarget=plan.finishTarget;value.finishJump=plan.finishJump;State=value;
   finishMotorTime=0;finishLaunched=false;
   finishLeftFoot=RigReady?leftLeg.end.position:transform.TransformPoint(leftFoot);finishRightFoot=RigReady?rightLeg.end.position:transform.TransformPoint(rightFoot);
   athlete.Motor.Reset(transform.eulerAngles.y);
  }
  public static float FinishHeight(BasketballMotionState plan,float time){float t=Mathf.Max(0,time-BasketballFinishRules.Takeoff(plan.action));return Mathf.Max(0,plan.finishJump*t-.5f*JumpMotor.Gravity*t*t);}
  public static Vector3 FinishRoot(BasketballMotionState plan,float time){
   float takeoff=BasketballFinishRules.Takeoff(plan.action),release=BasketballFinishRules.Release(plan.action);
   var delta=plan.finishTarget-plan.finishOrigin;
   float gather=Mathf.Min(.32f,delta.magnitude*.26f);var step=plan.finishOrigin+delta.normalized*gather;
   // Continuous position and velocity through the plant. The airborne part
   // decelerates into release without pulling a body forward after a collision.
   float initial=gather/takeoff;
   if(time<takeoff)return Vector3.Lerp(plan.finishOrigin,step,time/takeoff);
   float u=Mathf.Clamp01((time-takeoff)/(release-takeoff));
   float h00=2*u*u*u-3*u*u+1,h10=u*u*u-2*u*u+u,h01=-2*u*u*u+3*u*u;
   return h00*step+h10*(release-takeoff)*initial*delta.normalized+h01*plan.finishTarget;
  }
  public Vector3 StepFinish(float dt){
   float before=finishMotorTime;finishMotorTime+=dt;float takeoff=BasketballFinishRules.Takeoff(Action);
   var move=FinishRoot(State,finishMotorTime)-FinishRoot(State,before);float vertical;
   if(finishMotorTime<takeoff)vertical=-2*dt;
   else {
    float airDt=dt;
    if(!finishLaunched){finishLaunched=true;athlete.Jump.Launch(State.finishJump);airDt=finishMotorTime-takeoff;}
    vertical=athlete.Jump.Step(athlete.Grounded&&!athlete.Jump.Airborne,false,airDt);
   }
   move.y=vertical;return move;
  }
  Vector3 FinishBall(float time)=>FinishBall(State,time);
  public static Vector3 FinishBall(BasketballMotionState plan,float time){
   float takeoff=BasketballFinishRules.Takeoff(plan.action),release=BasketballFinishRules.Release(plan.action);
   float hand=plan.leftHand?-1:1;var pocket=new Vector3(hand*.10f,.73f,.32f);
   if(time<.14f)return Vector3.Lerp(plan.gather,pocket,Ease(time/.14f));
   if(plan.action==BasketballAction.Dunk){
    var crown=new Vector3(hand*.28f,1.22f,.40f);float peak=takeoff+.20f;
    if(time<peak)return Vector3.Lerp(pocket,crown,Ease((time-.14f)/(peak-.14f)));
    return Vector3.Lerp(crown,BasketballFinishRules.ReleaseOffset(plan.action,plan.leftHand),Ease((time-peak)/(release-peak)));
   }
   var lift=new Vector3(hand*.20f,.93f,.37f);
   if(time<takeoff+.10f)return Vector3.Lerp(pocket,lift,Ease((time-.14f)/(takeoff-.04f)));
   return Vector3.Lerp(lift,BasketballFinishRules.ReleaseOffset(plan.action,plan.leftHand),Ease((time-takeoff-.10f)/(release-takeoff-.10f)));
  }
  void FinishPose(float time){
   var facing=Facing;var forward=facing*Vector3.forward;var side=facing*Vector3.right;float sign=State.leftHand?-1:1;
   float takeoff=BasketballFinishRules.Takeoff(Action),release=ReleaseTime(Action);
   float land=takeoff+2*State.finishJump/JumpMotor.Gravity;
   float recovery=Ease((time-land)/.24f),blend=Ease(time/.10f)*(1-recovery)*weight;
   float load=Mathf.Sin(Mathf.PI*Mathf.Clamp01(time/takeoff));
   float flight=Ease((time-takeoff)/.12f)*(1-Ease((time-land+.18f)/.18f));
   float absorption=Mathf.Sin(Mathf.PI*Mathf.Clamp01((time-land)/.24f));
   hips.position+=(Vector3.up*(-.085f*load-.105f*absorption+(Action==BasketballAction.Dunk?.060f:.018f)*flight)+forward*(Action==BasketballAction.Dunk?.10f*flight:0))*blend;
   float pitch=Mathf.Lerp(12,Action==BasketballAction.Dunk?12:-5,Ease((time-takeoff)/.24f))+12*absorption;
   Rotate(spine,side,pitch*.45f*blend);Rotate(chest,side,pitch*.55f*blend);Rotate(head,side,-pitch*.6f*blend);
   Rotate(chest,Vector3.up,sign*(Action==BasketballAction.Layup?-9:4)*flight*blend);
   for(int i=0;i<2;i++){
    bool isLeft=i==0;var leg=isLeft?leftLeg:rightLeg;var rest=isLeft?leftFoot:rightFoot;var rotation=facing*(isLeft?leftFootRotation:rightFootRotation);
    bool shootingLeg=isLeft==State.leftHand;Vector3 target;
    if(time<takeoff){
     float start=shootingLeg?0:.10f;float u=Mathf.Clamp01((time-start)/.14f);
     var planted=FinishRoot(State,takeoff)+facing*(rest+new Vector3(0,0,shootingLeg?-.08f:.06f));
     var initial=isLeft?finishLeftFoot:finishRightFoot;
     // Remote actors derive a stable contact from the committed origin.
     if(!athlete.capsule.enabled)initial=State.finishOrigin+Quaternion.Euler(0,State.startYaw,0)*rest;
     target=Vector3.Lerp(initial,planted,Ease(u))+Vector3.up*(.07f*Mathf.Sin(Mathf.PI*u));
    }else {
     var offset=rest+new Vector3(0,shootingLeg?.24f*flight:.065f*flight,shootingLeg?.14f*flight:-.18f*flight);
     target=transform.position+facing*offset;
     rotation=Quaternion.AngleAxis((shootingLeg?-12:18)*flight,side)*rotation;
    }
    Solve(leg,target,forward,rotation,blend);
   }
   var ball=BasketballBall.Active;bool held=ball&&ball.Holder==athlete;
   var center=held?ball.CarryPosition(athlete):transform.position+facing*FinishBall(release);
   float guideAway=Ease((time-takeoff-.13f)/.13f),follow=Ease((time-release)/.18f),withdraw=Ease((time-release-.20f)/.27f);
   bool dunk=Action==BasketballAction.Dunk;
   var shooting=State.leftHand?left:right;var guide=State.leftHand?right:left;
   // Begin with two hands on the gather. Layups roll off the underside of the
   // shooting palm; dunks turn that palm over the crown before driving down.
   float over=dunk?Ease((time-takeoff-.20f)/(release-takeoff-.20f)):0;
   var normal=Quaternion.AngleAxis(sign*180*over,forward)*Vector3.up;
   var contact=center-normal*.135f-forward*.018f;
   contact+=dunk?-Vector3.up*(.26f*follow)-forward*.035f*follow:forward*.035f*follow+Vector3.up*.025f*follow;
   contact=Vector3.Lerp(contact,transform.position+facing*new Vector3(sign*.29f,.81f,.29f),withdraw);
   normal=Vector3.Slerp(normal,forward,follow);
   Arm(shooting,contact,normal,Vector3.Lerp(forward,Vector3.down,follow),side*sign*.6f-forward*.12f,blend);
   var guideContact=Vector3.Lerp(center-side*sign*.12f-Vector3.up*.07f,transform.position+facing*new Vector3(-sign*.29f,.88f,.25f),guideAway);
   Arm(guide,guideContact,side*sign,forward,-side*sign*.6f-forward*.15f,blend);
   if(held&&time>.15f&&time<release-.015f)ContactError=Vector3.Distance(Palm(shooting),contact);
  }
 }
}
