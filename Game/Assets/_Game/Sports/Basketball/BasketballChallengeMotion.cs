using UnityEngine;

namespace WhatTheFish {
 public sealed partial class BasketballMotion {
  // Time keys are seconds on the replicated action clock. The striking hand
  // passes THROUGH contact, then folds back; it never parks in a reaching pose.
  static float Phase(float time,float begin,float end)=>Ease((time-begin)/(end-begin));
  static float Pulse(float time,float begin,float peak,float hold,float end)=>Phase(time,begin,peak)*(1-Phase(time,hold,end));
  void ChallengePose(float time,float ground,float stand){
   bool stealing=Action==BasketballAction.Steal;float sign=State.leftHand?-1:1;
   var facing=Facing;var forward=facing*Vector3.forward;var side=facing*Vector3.right;
   float blend=Pulse(time,0,stealing?.065f:.055f,stealing?.46f:.40f,Duration(Action))*weight;
   float load=stealing?Pulse(time,0,.065f,.065f,.15f):0;
   float drive=stealing?Pulse(time,.045f,.17f,.27f,.61f):Pulse(time,0,.09f,.13f,.50f);
   float follow=stealing?Pulse(time,.15f,.29f,.31f,.56f):Pulse(time,.055f,.18f,.22f,.52f);
   float low=Mathf.Clamp01((.72f-(State.contact.y-transform.position.y))/.48f);
   Vector3 lp=leftLeg.end.position,rp=rightLeg.end.position;Quaternion lr=leftLeg.end.rotation,rr=rightLeg.end.rotation;

   // Fit the weight shift to this character's short shins. Grounded feet stay
   // on the court; a moving player retains the authored gait underneath it.
   float shift=stealing?.065f*drive-.022f*load:-.055f*drive+.070f*follow;
   float lower=stealing?(.035f+.055f*low)*drive+.018f*load:.035f*drive;
   hips.position+=(forward*shift+side*(sign*(stealing?.022f:-.025f)*drive))*ground*weight
                 -Vector3.up*(.035f+.025f*run+.05f*brake+lower)*ground*weight;
   lean=Damp(lean,3+run*4,12);
   float pitch=lean-brake*11+(stealing?(15+12*low)*drive-5*load:-9*drive+25*follow);
   float twist=sign*(stealing?12*load-18*drive-8*follow:16*drive-29*follow);
   float roll=sign*(stealing?-5*drive:-8*drive+13*follow);
   float align=Mathf.Clamp(Mathf.DeltaAngle(transform.eulerAngles.y,State.heading),-65,65)*blend;
   Rotate(spine,side,pitch*.42f*weight);Rotate(chest,side,pitch*.58f*weight);
   Rotate(spine,Vector3.up,(twist*.35f*weight+align*.4f));Rotate(chest,Vector3.up,(twist*.65f*weight+align*.6f));
   Rotate(chest,forward,roll*weight);
   // Eyes follow the low ball while the head counterbalances the shoulders.
   Rotate(head,side,-pitch*.38f*weight);Rotate(head,Vector3.up,-twist*.60f*weight);Rotate(head,forward,-roll*.65f*weight);

   if(ground>0){
    ChallengeFoot(true,time,stealing,sign,facing,out var l,out var lrot);
    ChallengeFoot(false,time,stealing,sign,facing,out var r,out var rrot);
    Solve(leftLeg,Vector3.Lerp(lp,l,stand*weight),forward,Quaternion.Slerp(lr,lrot,stand*weight),weight);
    Solve(rightLeg,Vector3.Lerp(rp,r,stand*weight),forward,Quaternion.Slerp(rr,rrot,stand*weight),weight);
   }
   var arm=State.leftHand?left:right;var guard=State.leftHand?right:left;
   Vector3 Local(float x,float y,float z)=>transform.position+facing*new Vector3(x,y,z);
   if(stealing){
    // A curved rake from outside the ball to the inside hip. The palm turns
    // across/down with the sweep, instead of showing a vertical "stop" hand.
    // A whiff still rakes in front of the body. Moving past a committed world
    // point must not pull the wrist backward through the chest on recovery.
    var localContact=Quaternion.Inverse(facing)*(State.contact-transform.position);
    localContact.x=Mathf.Clamp(localContact.x,-.45f,.45f);localContact.y=Mathf.Clamp(localContact.y,.20f,.98f);localContact.z=Mathf.Clamp(localContact.z,.27f,.72f);
    var ballPoint=transform.position+facing*localContact;
    var contact=ballPoint+(arm.upper.position-ballPoint).normalized*(BasketballBall.Radius-.012f);
    var cock=Local(sign*.32f,.70f,.21f);
    var through=contact-side*(sign*.19f)-Vector3.up*.065f+forward*.035f;
    var control=contact*2-(cock+through)*.5f;
    float strike=Phase(time,.065f,.30f);
    var palm=(1-strike)*(1-strike)*cock+2*(1-strike)*strike*control+strike*strike*through;
    palm=Vector3.Lerp(palm,Local(sign*.23f,.63f,.26f),Phase(time,.32f,.53f));
    var normal=(forward*.32f-side*(sign*.8f)-Vector3.up*.32f).normalized;
    ChallengeArm(arm,palm,normal,forward-Vector3.up*.35f,side*(sign*.55f)-Vector3.up*.55f-forward*.10f,time,stealing);
    // The free elbow balances behind the body, then returns below the chest.
    ChallengeArm(guard,Local(-sign*(.24f+.035f*drive),.68f-.055f*drive,.22f-.14f*drive),
        side*sign,forward-Vector3.up*.35f,-side*(sign*.6f)-Vector3.up*.2f,time,stealing);
   }else{
    // The dribbling hand is knocked down, reaches late after the escaped ball,
    // and recoils to the ribs. The other arm opens low to catch the balance.
    var start=transform.position+facing*(State.leftHand?State.leftStart:State.rightStart);
    var knocked=Local(sign*.28f,.52f,.26f);
    var chase=State.contact+forward*.10f+side*(sign*.035f)+Vector3.up*.035f;
    var palm=Vector3.Lerp(start,knocked,Phase(time,0,.08f));
    palm=Vector3.Lerp(palm,chase,Phase(time,.08f,.19f));
    palm=Vector3.Lerp(palm,Local(sign*.23f,.69f,.20f),Phase(time,.23f,.43f));
    ChallengeArm(arm,palm,Vector3.down*.8f-side*(sign*.3f),forward,side*(sign*.6f)-Vector3.up*.35f,time,stealing);
    ChallengeArm(guard,Local(-sign*(.25f+.10f*follow),.67f-.08f*follow,.20f-.13f*follow),
        side*sign-Vector3.up*.4f,forward,-side*(sign*.7f)-Vector3.up*.3f,time,stealing);
   }
  }
  void ChallengeArm(Limb arm,Vector3 palm,Vector3 normal,Vector3 fingers,Vector3 pole,float time,bool stealing){
   bool rightSide=arm==right;float take=Phase(time,0,stealing?.065f:.055f);
   var rotation=transform.rotation*(rightSide?State.rightRotation:State.leftRotation);
   // Start from the last visible hand, including a dribble. A fade from the
   // underlying idle arm would snap the victim's hand down on loss of control.
   bool guard=arm==(State.leftHand?right:left);
   if(stealing||guard)palm=Vector3.Lerp(transform.position+transform.rotation*(rightSide?State.rightStart:State.leftStart),palm,take);
   normal=Vector3.Slerp(rotation*arm.palmAxis,normal.normalized,take);
   fingers=Vector3.Slerp(rotation*arm.fingerAxis,fingers.normalized,take);
   pole=Vector3.Slerp(transform.rotation*(rightSide?State.rightPole:State.leftPole),pole,take);
   Arm(arm,palm,normal,fingers,pole,(1-Phase(time,stealing?.46f:.40f,Duration(Action)))*weight);
  }
  void ChallengeFoot(bool leftSide,float time,bool stealing,float sign,Quaternion facing,out Vector3 target,out Quaternion rotation){
   var rest=leftSide?leftFoot:rightFoot;var restRotation=leftSide?leftFootRotation:rightFootRotation;
   bool lead=leftSide==State.leftHand;
   float outward=Phase(time,stealing?.025f:.075f,stealing?.155f:.235f);
   float back=Phase(time,stealing?.38f:.32f,stealing?.63f:.57f);
   float step=outward*(1-back);
   float lift=lead?.034f*(Mathf.Sin(Mathf.PI*outward)+Mathf.Sin(Mathf.PI*back)):0;
   var offset=lead?new Vector3(sign*(stealing?.035f:.075f),lift,(stealing?.115f:.055f)*step):Vector3.zero;
   offset.x*=step;
   // Only the leading foot steps; the support foot anchors the weight shift.
   target=transform.TransformPoint(rest+new Vector3(leftSide?-.025f:.025f,0,leftSide?-.025f:.025f))+facing*offset;
   rotation=Quaternion.AngleAxis((lead?sign*7*step:0),Vector3.up)*transform.rotation*restRotation;
  }
 }
}
