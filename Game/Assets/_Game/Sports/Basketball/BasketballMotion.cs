using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public enum BasketballAction:byte { None,Shoot,Pass }
 public struct BasketballMotionState:INetworkSerializable,IEquatable<BasketballMotionState> {
  public BasketballAction action;public uint sequence;public double started;public float heading,startYaw,ballSpin;public Vector3 gather,rightStart,leftStart,rightPole,leftPole;public Quaternion rightRotation,leftRotation;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref action);s.SerializeValue(ref sequence);s.SerializeValue(ref started);s.SerializeValue(ref heading);s.SerializeValue(ref startYaw);s.SerializeValue(ref ballSpin);s.SerializeValue(ref gather);s.SerializeValue(ref rightStart);s.SerializeValue(ref leftStart);s.SerializeValue(ref rightPole);s.SerializeValue(ref leftPole);s.SerializeValue(ref rightRotation);s.SerializeValue(ref leftRotation);}
  public bool Equals(BasketballMotionState b)=>action==b.action&&sequence==b.sequence&&started==b.started&&heading==b.heading&&startYaw==b.startYaw&&ballSpin==b.ballSpin&&gather==b.gather&&rightStart==b.rightStart&&leftStart==b.leftStart&&rightPole==b.rightPole&&leftPole==b.leftPole&&rightRotation.Equals(b.rightRotation)&&leftRotation.Equals(b.leftRotation);
 }

 // A compact basketball layer over the existing in-place gait and jump. The
 // same timeline drives the hands, the held ball and authoritative release.
 [DefaultExecutionOrder(40)]
 public sealed class BasketballMotion:MonoBehaviour {
  public const float ShotRelease=.44f,PassRelease=.30f,ShotDuration=.98f,PassDuration=.70f;
  public static double Clock=>NetworkManager.Singleton&&NetworkManager.Singleton.IsListening?NetworkManager.Singleton.ServerTime.Time:Time.timeAsDouble;
  public BasketballMotionState State {get;private set;}
  public BasketballAction Action=>Elapsed<Duration(State.action)?State.action:BasketballAction.None;
  public float Elapsed=>Mathf.Max(0,(float)(Clock-State.started));
  public bool Busy=>Action!=BasketballAction.None;
  public bool BeforeRelease=>Busy&&Elapsed<ReleaseTime(Action);
  public bool RigReady {get;private set;}
  public float MaximumReachError {get;private set;}
  public float ContactError {get;private set;}
  public Vector3 RightPalm=>Palm(right);
  public Vector3 LeftPalm=>Palm(left);
  public float Weight=>weight;
  public static float Duration(BasketballAction a)=>a==BasketballAction.Shoot?ShotDuration:a==BasketballAction.Pass?PassDuration:0;
  public static float ReleaseTime(BasketballAction a)=>a==BasketballAction.Shoot?ShotRelease:PassRelease;
  public static float Ease(float x){x=Mathf.Clamp01(x);return x*x*(3-2*x);}
  static float Damp(float current,float target,float rate)=>Mathf.Lerp(current,target,1-Mathf.Exp(-rate*Mathf.Min(Time.deltaTime,.1f)));
  Athlete athlete;Transform hips,spine,chest,head;Limb left,right,leftLeg,rightLeg;
  Transform[] bones;Vector3[] sourcePositions;Quaternion[] sourceRotations;bool applied;
  Vector3 leftFoot,rightFoot;Quaternion leftFootRotation,rightFootRotation;
  float weight,run,lean,previousSpeed,brake,actionBlend,possessionBlend,stanceBlend;Vector3 previousPosition;
  sealed class Limb {public Transform upper,lower,end;public Vector3 palmOffset,palmAxis,fingerAxis;public readonly List<Transform> fingers=new();}
  public void Bind(Athlete owner){
   if(RigReady)return;athlete=owner;if(!owner.visual)return;
   bones=owner.visual.GetComponentsInChildren<Transform>(true);
   Transform Bone(string name)=>Array.Find(bones,t=>t.name=="mixamorig:"+name);
   Limb Make(string side,bool arm){return new Limb{upper=Bone(side+(arm?"Arm":"UpLeg")),lower=Bone(side+(arm?"ForeArm":"Leg")),end=Bone(side+(arm?"Hand":"Foot"))};}
   hips=Bone("Hips");spine=Bone("Spine");chest=Bone("Spine2");head=Bone("Head");
   left=Make("Left",true);right=Make("Right",true);leftLeg=Make("Left",false);rightLeg=Make("Right",false);
   foreach(var limb in new[]{left,right,leftLeg,rightLeg})if(!limb.upper||!limb.lower||!limb.end)return;
   if(!hips||!spine||!chest||!head)return;
   foreach(var side in new[]{"Left","Right"}){
    var arm=side=="Left"?left:right;var middle=Bone(side+"HandMiddle1")??Bone(side+"HandMiddle4");var index=Bone(side+"HandIndex1");var pinky=Bone(side+"HandPinky1");
    Vector3 along=middle?middle.position-arm.end.position:(arm.end.position-arm.lower.position).normalized*.08f;
    // Calibrate the palm frame from actual rig landmarks, never from FBX Euler axes.
    Vector3 normal=index&&pinky?Vector3.Cross(index.position-arm.end.position,pinky.position-arm.end.position).normalized:-transform.right;
    if(side=="Left")normal=-normal;
    arm.palmOffset=arm.end.InverseTransformVector(along*.52f);
    arm.fingerAxis=arm.end.InverseTransformDirection(along.normalized);arm.palmAxis=arm.end.InverseTransformDirection(normal);
    foreach(var bone in bones)if(bone.name.StartsWith("mixamorig:"+side+"Hand")&&bone!=arm.end&&bone.childCount>0&&!bone.name.Contains("Middle4")&&!bone.name.EndsWith("_end"))arm.fingers.Add(bone);
   }
   var placement=owner.GetComponent<TurnFootPlacement>();
   leftFoot=placement?placement.left.initialPosition:transform.InverseTransformPoint(leftLeg.end.position);
   rightFoot=placement?placement.right.initialPosition:transform.InverseTransformPoint(rightLeg.end.position);
   leftFootRotation=placement?placement.left.initialRotation:Quaternion.Inverse(transform.rotation)*leftLeg.end.rotation;
   rightFootRotation=placement?placement.right.initialRotation:Quaternion.Inverse(transform.rotation)*rightLeg.end.rotation;
   sourcePositions=new Vector3[bones.Length];sourceRotations=new Quaternion[bones.Length];RigReady=true;previousPosition=transform.position;
  }
  public void Begin(BasketballAction action,float heading,Vector3 gather){
   // Carry the gather around with the turning body. Interpolating between two
   // world-space hand positions across a 180-degree turn cuts through the torso.
   gather=Quaternion.Inverse(transform.rotation)*Quaternion.Euler(0,heading,0)*gather;
   var inverse=Quaternion.Inverse(transform.rotation);
   State=new BasketballMotionState{action=action,heading=heading,startYaw=transform.eulerAngles.y,ballSpin=BasketballBall.Active?BasketballBall.Active.DribblePhase:0,started=Clock,gather=gather,sequence=State.sequence+1,rightStart=inverse*(RightPalm-transform.position),leftStart=inverse*(LeftPalm-transform.position),rightPole=RigReady?inverse*(right.lower.position-right.upper.position):Vector3.right,leftPole=RigReady?inverse*(left.lower.position-left.upper.position):Vector3.left,rightRotation=RigReady?inverse*right.end.rotation:Quaternion.identity,leftRotation=RigReady?inverse*left.end.rotation:Quaternion.identity};
  }
  public void Receive(BasketballMotionState value){State=value;}
  public void ResetPose(){Restore();if(BasketballBall.Active)BasketballBall.Active.CancelAction(athlete);State=new BasketballMotionState{sequence=State.sequence+1};weight=run=lean=brake=actionBlend=possessionBlend=stanceBlend=previousSpeed=0;previousPosition=transform.position;}
  void OnDisable(){Restore();}
  void Restore(){if(!applied)return;for(int i=0;i<bones.Length;i++)if(bones[i]){bones[i].localPosition=sourcePositions[i];bones[i].localRotation=sourceRotations[i];}applied=false;}
  void Update(){Restore();}
  public static Vector3 DribbleOffset(float phase){
   phase=Mathf.Repeat(phase,1);float u=phase<.46f?phase/.46f:(1-phase)/.54f;
   return new Vector3(.31f,.065f+.64f*(1-u*u),.29f+.045f*Mathf.Sin(phase*Mathf.PI));
  }
  public Vector3 BallOffset(float elapsed){
   Vector3 pocket=new(.07f,.70f,.32f);
   if(State.action==BasketballAction.Shoot){
    if(elapsed<.14f)return Vector3.Lerp(State.gather,pocket,Ease(elapsed/.14f));
    if(elapsed<.29f)return Vector3.Lerp(pocket,new Vector3(.10f,.97f,.39f),Ease((elapsed-.14f)/.15f));
    return Vector3.Lerp(new Vector3(.10f,.97f,.39f),new Vector3(.09f,1.19f,.43f),Ease((elapsed-.29f)/.15f));
   }
   if(elapsed<.14f)return Vector3.Lerp(State.gather,new Vector3(0,.80f,.30f),Ease(elapsed/.14f));
   return Vector3.Lerp(new Vector3(0,.80f,.30f),new Vector3(0,.88f,.46f),Ease((elapsed-.14f)/.16f));
  }
  public Quaternion Facing=>Quaternion.Euler(0,State.heading,0);
  public Quaternion PoseFacing=>Quaternion.Slerp(transform.rotation,Facing,Ease((Elapsed-.16f)/.20f));
  public Vector3 ReleasePosition=>transform.position+Facing*BallOffset(ReleaseTime(State.action));
  Vector3 Palm(Limb arm)=>RigReady?arm.end.TransformPoint(arm.palmOffset):transform.position;
  void LateUpdate(){
   if(!RigReady)return;
   var ball=BasketballBall.Active;bool playing=ball&&ball.Playing&&!athlete.inTransit;
   bool held=playing&&ball.Holder==athlete;bool acting=playing&&Busy;
   weight=Damp(weight,playing?1:0,14);if(weight<.001f)return;
   for(int i=0;i<bones.Length;i++){sourcePositions[i]=bones[i].localPosition;sourceRotations[i]=bones[i].localRotation;}applied=true;
   float dt=Mathf.Max(Time.deltaTime,.001f);float speed=athlete.speed;
   if(Vector3.Distance(previousPosition,transform.position)>2){brake=0;previousSpeed=speed;}
   previousPosition=transform.position;
   brake=Damp(brake,Mathf.Clamp01((previousSpeed-speed)/(dt*35)),9);previousSpeed=speed;
   run=Damp(run,Mathf.Clamp01(speed/7),12);
   float t=Elapsed;float envelope=acting?Ease(t/.12f)*(1-Ease((t-(Duration(Action)-.22f))/.22f)):0;
   actionBlend=Damp(actionBlend,envelope,25);
   float ground=athlete.Airborne||athlete.LoadingJump?0:1;
   stanceBlend=Damp(stanceBlend,1-Ease(speed/1.4f),14);
   float stand=stanceBlend*ground;
   Vector3 lp=leftLeg.end.position,rp=rightLeg.end.position;Quaternion lr=leftLeg.end.rotation,rr=rightLeg.end.rotation;
   float dip=acting?(Action==BasketballAction.Shoot?.10f*Mathf.Sin(Mathf.PI*Mathf.Clamp01(t/.36f)):.05f*Mathf.Sin(Mathf.PI*Mathf.Clamp01(t/.30f))):0;
   float rise=acting&&Action==BasketballAction.Shoot?.035f*Ease((t-.29f)/.15f)*(1-Ease((t-.55f)/.22f)):0;
   hips.position+=transform.up*((-.035f-.025f*run-.05f*brake-dip+rise)*ground*weight);
   lean=Damp(lean,held?8+run*5:3+run*4,12);
   float pitch=(lean-brake*11)*(1-actionBlend)+(-4*actionBlend);
   Rotate(spine,transform.right,pitch*.45f*weight);Rotate(chest,transform.right,pitch*.55f*weight);Rotate(head,transform.right,-pitch*.65f*weight);
   if(acting){
    float yaw=Mathf.Clamp(Mathf.DeltaAngle(transform.eulerAngles.y,State.heading),-65,65)*actionBlend*weight;
    Rotate(spine,Vector3.up,yaw*.4f);Rotate(chest,Vector3.up,yaw*.6f);
   }
   // Retain the authored running contacts; settle into a balanced split stance
   // at rest. Knee poles remain forward when the body lowers for a stop or shot.
   if(ground>0){
    var l=transform.TransformPoint(leftFoot+new Vector3(-.025f,0,-.025f));var r=transform.TransformPoint(rightFoot+new Vector3(.025f,0,.025f));
    var lrot=transform.rotation*leftFootRotation;var rrot=transform.rotation*rightFootRotation;
    if(acting&&Mathf.Abs(Mathf.DeltaAngle(State.startYaw,State.heading))>20){
     ActionFoot(leftFoot+new Vector3(-.025f,0,-.025f),leftFootRotation,true,t,out l,out lrot);
     ActionFoot(rightFoot+new Vector3(.025f,0,.025f),rightFootRotation,false,t,out r,out rrot);
    }
    Solve(leftLeg,Vector3.Lerp(lp,l,stand*weight),transform.forward,Quaternion.Slerp(lr,lrot,stand*weight),weight);
    Solve(rightLeg,Vector3.Lerp(rp,r,stand*weight),transform.forward,Quaternion.Slerp(rr,rrot,stand*weight),weight);
   }
   MaximumReachError=ContactError=0;
   possessionBlend=Damp(possessionBlend,held?1:0,20);
   float arms=held?possessionBlend*weight:actionBlend*weight;
   if(arms<.001f||!held&&!acting)return;
   Quaternion facing=acting?PoseFacing:transform.rotation;
   Vector3 forward=facing*Vector3.forward,side=facing*Vector3.right;
   Vector3 center=held?ball.CarryPosition(athlete):transform.position+Facing*BallOffset(ReleaseTime(State.action));
   if(!acting&&held&&(athlete.Airborne||athlete.LoadingJump)){
    Arm(right,center+side*.10f-forward*.06f,-side,forward,side*.6f,arms);
    Arm(left,center-side*.10f-forward*.06f,side,forward,-side*.6f,arms);
   }else if(acting){
    bool shooting=Action==BasketballAction.Shoot;
    float release=Ease((t-ReleaseTime(Action))/.12f);
    Vector3 rightContact=center+(shooting?-Vector3.up*.105f-forward*.015f:(-forward*.055f+side*.095f));
    Vector3 leftContact=center+(shooting?-side*.096f-Vector3.up*.072f:-forward*.055f-side*.095f);
    if(t>=ReleaseTime(Action)){
     rightContact+=shooting?-forward*(.08f*release)-Vector3.up*.12f*release:side*.035f*release;
     leftContact+=shooting?-side*.05f*release-Vector3.up*.12f*release:-side*.035f*release;
    }
    float push=Ease((t-.14f)/.16f);
    Arm(right,rightContact,shooting?Vector3.Lerp(Vector3.up,forward,release):Vector3.Slerp(-side,forward,push),shooting?Vector3.Lerp(forward,Vector3.down,release):Vector3.up,side*.45f-forward*.1f-Vector3.up*.7f,arms);
    Arm(left,leftContact,shooting?side:Vector3.Slerp(side,forward,push),shooting?Vector3.up*.8f+forward*.6f:Vector3.up,-side*.45f-forward*.1f-Vector3.up*.7f,arms);
   }else{
    float phase=ball.DribblePhase;float contact=phase<.30f?1-Ease((phase-.14f)/.16f):phase>.84f?Ease((phase-.84f)/.16f):0;
    float handY=phase<.30f?Mathf.Lerp(.82f,.62f,Ease(phase/.30f)):Mathf.Lerp(.62f,.82f,Ease((phase-.40f)/.60f));
    Vector3 palm=transform.TransformPoint(new Vector3(.31f,handY,.30f));palm=Vector3.Lerp(palm,center+Vector3.up*.115f,contact);
    Arm(right,palm,Vector3.down,forward,side*.6f-forward*.12f,arms);
    Vector3 guard=transform.TransformPoint(new Vector3(-.23f,.77f+.025f*Mathf.Sin(phase*Mathf.PI*2),.29f));
    Arm(left,guard,side,forward,-side*.6f-forward*.1f,arms*.82f);
    if(contact>.99f)ContactError=Vector3.Distance(RightPalm,center+Vector3.up*.115f);
   }
  }
  void ActionFoot(Vector3 foot,Quaternion rest,bool leftSide,float time,out Vector3 target,out Quaternion rotation){
   float angle=Mathf.DeltaAngle(State.startYaw,State.heading);
   TurnFootPlacement.FootStep(Mathf.Clamp01(time/.38f),leftSide?angle<0:angle>0,out float fraction,out float height);
   var yaw=Quaternion.Euler(0,State.startYaw+angle*fraction,0);target=transform.position+yaw*foot+Vector3.up*height;rotation=yaw*rest;
  }
  static void Rotate(Transform bone,Vector3 axis,float angle){bone.rotation=Quaternion.AngleAxis(angle,axis)*bone.rotation;}
  void Arm(Limb arm,Vector3 palm,Vector3 normal,Vector3 fingers,Vector3 pole,float blend){
   normal.Normalize();
   fingers=Vector3.ProjectOnPlane(fingers,normal).normalized;
   Quaternion rotation=Quaternion.LookRotation(fingers,normal)*Quaternion.Inverse(Quaternion.LookRotation(arm.fingerAxis,arm.palmAxis));
   if(Busy&&Elapsed<.14f){float gather=Ease(Elapsed/.14f);palm=Vector3.Lerp(transform.position+PoseFacing*(arm==right?State.rightStart:State.leftStart),palm,gather);rotation=Quaternion.Slerp(PoseFacing*(arm==right?State.rightRotation:State.leftRotation),rotation,gather);}
   if(Busy&&Elapsed<.20f)pole=Vector3.Slerp(PoseFacing*(arm==right?State.rightPole:State.leftPole),pole,Ease(Elapsed/.20f));
   Vector3 wrist=palm-rotation*Vector3.Scale(arm.palmOffset,arm.end.lossyScale);
   MaximumReachError=Mathf.Max(MaximumReachError,Solve(arm,wrist,pole,rotation,blend));
   // Relax and spread the imported fingers around the surface instead of a
   // rigid open paddle. The thumb retains its opposed rest orientation.
   foreach(var finger in arm.fingers){
    if(finger.name.Contains("Thumb"))continue;
    Vector3 direction=finger.GetChild(0).position-finger.position;
    Vector3 axis=Vector3.Cross(direction,normal).normalized;
    Rotate(finger,axis,(finger.name.EndsWith("1")?9:17)*blend);
   }
  }
  static float Solve(Limb limb,Vector3 target,Vector3 pole,Quaternion endRotation,float blend){
   var a=limb.upper.position;var b=limb.lower.position;var c=limb.end.position;
   float upper=Vector3.Distance(a,b),lower=Vector3.Distance(b,c);
   Vector3 delta=target-a;float reach=delta.magnitude;
   if(reach<.001f||upper<.001f||lower<.001f)return 0;
   float distance=Mathf.Clamp(reach,Mathf.Abs(upper-lower)+.002f,upper+lower-.012f);var direction=delta/reach;
   var bend=Vector3.ProjectOnPlane(pole,direction).normalized;if(bend.sqrMagnitude<.01f)bend=Vector3.ProjectOnPlane(Vector3.forward,direction).normalized;
   float along=(upper*upper-lower*lower+distance*distance)/(2*distance);
   var elbow=a+direction*along+bend*Mathf.Sqrt(Mathf.Max(0,upper*upper-along*along));
   target=a+direction*distance;
   var upperRotation=Quaternion.FromToRotation(b-a,elbow-a)*limb.upper.rotation;
   limb.upper.rotation=Quaternion.Slerp(limb.upper.rotation,upperRotation,blend);
   var lowerRotation=Quaternion.FromToRotation(limb.end.position-limb.lower.position,target-limb.lower.position)*limb.lower.rotation;
   limb.lower.rotation=Quaternion.Slerp(limb.lower.rotation,lowerRotation,blend);
   limb.end.rotation=Quaternion.Slerp(limb.end.rotation,endRotation,blend);
   return Mathf.Max(0,reach-distance);
  }
 }
}
