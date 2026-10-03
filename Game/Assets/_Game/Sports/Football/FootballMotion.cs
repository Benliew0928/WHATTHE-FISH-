using System;
using UnityEngine;

namespace WhatTheFish {
 // The motor and ball remain authoritative. This component owns only the
 // football presentation, restores its previous pose before Animator evaluation,
 // then composes saved control-space takes with the existing run/jump/slide.
 [DefaultExecutionOrder(50)]
 public sealed class FootballMotion:MonoBehaviour {
  public FootballMotionState State {get;private set;}
  public bool RigReady {get;private set;}
  public bool Active=>RigReady&&FootballTackle.EnvironmentAllowed&&athlete&&!athlete.inTransit;
  public float Weight=>weight;
  public float MaximumReachError {get;private set;}
  public float BallContactError {get;private set;}
  public float ContactWeight {get;private set;}
  public string Performance {get;private set;}="Ready";
  public float GaitPhase=>phase;
  public Vector3 LeftFoot=>legs[0].end.position;
  public Vector3 RightFoot=>legs[1].end.position;
  public static double Clock=>BasketballMotion.Clock;
  public FootballMotionLibrary library;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public string ReviewTake;public float ReviewTime;
  public double LastPoseMilliseconds {get;private set;}
  public long LastPoseAllocatedBytes {get;private set;}
#endif
  Athlete athlete;Transform hips,spine,chest,head;Limb[] legs,arms;
  Transform[] bones;Vector3[] beforePosition,referencePosition;Quaternion[] beforeRotation,referenceRotation;
  Vector3[] feet;Quaternion[] footRotation;Vector3 hipReference;readonly Vector3[] stopPins=new Vector3[2];
  bool applied,heldBefore,received;uint receivedSequence;float weight,phase,previousSpeed,previousYaw,brake,startAccent,turn,bank,receiveAt=-10,stopAt=-10,cutAt=-10,cutSign=1;
  double resultAt;FootballMatchPhase lastMatchPhase;Vector3 previousPosition;bool stopLeft;FootballPose pose;
  sealed class Limb {public Transform upper,lower,end,toe;public float upperLength,lowerLength;public Quaternion wristRest;}
  public static int FallVariant(Vector3 push,Quaternion facing){var v=Quaternion.Inverse(facing)*push;return Mathf.Abs(v.x)>Mathf.Abs(v.z)?(v.x<0?2:3):(v.z<0?0:1);}
  public void Bind(Athlete owner){
   if(RigReady)return;athlete=owner;if(!owner.visual)return;
   var all=owner.visual.GetComponentsInChildren<Transform>(true);bones=Array.FindAll(all,t=>t.name.StartsWith("mixamorig:"));
   Transform Bone(string n)=>Array.Find(bones,t=>t.name=="mixamorig:"+n);
   Limb Make(string side,bool arm){var l=new Limb{upper=Bone(side+(arm?"Arm":"UpLeg")),lower=Bone(side+(arm?"ForeArm":"Leg")),end=Bone(side+(arm?"Hand":"Foot")),toe=Bone(side+(arm?"HandMiddle4":"Toe_End"))};if(l.upper&&l.lower&&l.end){l.upperLength=Vector3.Distance(l.upper.position,l.lower.position);l.lowerLength=Vector3.Distance(l.lower.position,l.end.position);}return l;}
   hips=Bone("Hips");spine=Bone("Spine");chest=Bone("Spine2");head=Bone("Head");legs=new[]{Make("Left",false),Make("Right",false)};arms=new[]{Make("Left",true),Make("Right",true)};
   if(!hips||!spine||!chest||!head)return;foreach(var limb in legs)if(!limb.end)return;foreach(var limb in arms)if(!limb.end)return;
   foreach(var limb in arms)limb.wristRest=Quaternion.Inverse(limb.lower.rotation)*limb.end.rotation;
   var placement=owner.GetComponent<TurnFootPlacement>();feet=new Vector3[2];footRotation=new Quaternion[2];
   for(int i=0;i<2;i++){var stored=placement?(i==0?placement.left:placement.right):null;feet[i]=stored!=null?stored.initialPosition:transform.InverseTransformPoint(legs[i].end.position);footRotation[i]=stored!=null?stored.initialRotation:Quaternion.Inverse(transform.rotation)*legs[i].end.rotation;}
   hipReference=transform.InverseTransformPoint(hips.position);
   beforePosition=new Vector3[bones.Length];referencePosition=new Vector3[bones.Length];beforeRotation=new Quaternion[bones.Length];referenceRotation=new Quaternion[bones.Length];
   for(int i=0;i<bones.Length;i++){referencePosition[i]=bones[i].localPosition;referenceRotation[i]=bones[i].localRotation;}
   library=Resources.Load<FootballMotionLibrary>("FootballMotionLibrary");
   if(!library){Debug.LogError("Missing FootballMotionLibrary; run FootballMotionBuilder.Prepare before building.");return;}
   RigReady=true;previousPosition=transform.position;previousYaw=transform.eulerAngles.y;
  }
  public void ResetPose(){Restore();State=new(){sequence=State.sequence+1};weight=phase=previousSpeed=brake=startAccent=turn=bank=0;receiveAt=stopAt=cutAt=-10;heldBefore=false;previousPosition=transform.position;previousYaw=transform.eulerAngles.y;lastMatchPhase=FootballMatchPhase.Idle;}
  public void Receive(FootballMotionState value){if(received&&unchecked((int)(value.sequence-receivedSequence))<0)return;received=true;receivedSequence=value.sequence;State=value;}
  public void Charging(bool active){
   if(active==State.charging)return;var s=State;s.sequence++;s.charging=active;
   if(active){s.chargeStarted=Clock;s.left=phase<.5f;s.gesture=FootballGesture.None;}
   else if(State.gesture!=FootballGesture.Kick||Clock-State.started>.35){s.gesture=FootballGesture.Cancel;s.started=Clock;}
   State=s;
  }
  public void Kick(float power,Vector3 ball){State=new(){sequence=State.sequence+1,gesture=FootballGesture.Kick,started=Clock,left=State.charging?State.left:phase<.5f,power=Mathf.Clamp01(power),contact=transform.InverseTransformPoint(ball)};}
  public void Bump(Vector3 normal){if(!Active||athlete.Action!=FootballAction.None||athlete.speed<1||Mathf.Abs(normal.y)>.3f||Clock-State.started<.3||State.charging)return;State=new(){sequence=State.sequence+1,gesture=FootballGesture.Bump,started=Clock,left=Vector3.Dot(normal,transform.right)<0};}
  void OnDisable(){Restore();}
  void Restore(){if(!applied)return;for(int i=0;i<bones.Length;i++)if(bones[i]){bones[i].localPosition=beforePosition[i];bones[i].localRotation=beforeRotation[i];}applied=false;}
  void Update(){Restore();}
  static float Ease(float t)=>BasketballMotion.Ease(t);
  static float Smooth(float a,float b,float rate,float dt)=>Mathf.Lerp(a,b,1-Mathf.Exp(-rate*dt));
  FootballPose Take(string name,float time){Performance=name;var take=library.Find(name);return take!=null?take.Sample(time):FootballMotionLibrary.Ready;}
  public float BoneLengthError(){float error=0;foreach(var l in legs)error=Mathf.Max(error,Mathf.Abs(Vector3.Distance(l.upper.position,l.lower.position)-l.upperLength),Mathf.Abs(Vector3.Distance(l.lower.position,l.end.position)-l.lowerLength));foreach(var l in arms)error=Mathf.Max(error,Mathf.Abs(Vector3.Distance(l.upper.position,l.lower.position)-l.upperLength),Mathf.Abs(Vector3.Distance(l.lower.position,l.end.position)-l.lowerLength));return error;}
  void LateUpdate(){
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   long start=System.Diagnostics.Stopwatch.GetTimestamp(),allocated=GC.GetAllocatedBytesForCurrentThread();
#endif
   ApplyFrame();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   LastPoseMilliseconds=(System.Diagnostics.Stopwatch.GetTimestamp()-start)*1000.0/System.Diagnostics.Stopwatch.Frequency;LastPoseAllocatedBytes=GC.GetAllocatedBytesForCurrentThread()-allocated;
#endif
  }
  void ApplyFrame(){
   if(!RigReady)return;float dt=Mathf.Clamp(Time.deltaTime,.0001f,.1f);
   bool active=Active,air=athlete.Airborne||athlete.LoadingJump;float targetWeight=active&&!air?1:0;
   weight=Smooth(weight,targetWeight,air?28:16,dt);if(!active){weight=0;heldBefore=false;return;}if(weight<.001f)return;
   var displacement=Vector3.ProjectOnPlane(transform.position-previousPosition,Vector3.up);previousPosition=transform.position;
   if(displacement.magnitude>2){previousSpeed=0;stopAt=receiveAt=cutAt=-10;displacement=Vector3.zero;}
   float speed=athlete.Action==FootballAction.None?athlete.speed:0;
   bool held=athlete.ControlsFootball;float accel=(speed-previousSpeed)/dt;
   brake=Smooth(brake,Mathf.Clamp01(-accel/35),16,dt);startAccent=Smooth(startAccent,Mathf.Clamp01(accel/35),18,dt);
   if(previousSpeed>.35f&&speed<.35f){stopAt=Time.time;stopLeft=legs[0].end.position.y<legs[1].end.position.y;for(int i=0;i<2;i++)stopPins[i]=legs[i].end.position;}
   if(speed>.5f&&accel>1)stopAt=-10;
   float yaw=transform.eulerAngles.y,yawRate=Mathf.DeltaAngle(previousYaw,yaw)/dt;previousYaw=yaw;
   turn=Smooth(turn,Mathf.Clamp(yawRate/320,-1,1),12,dt);bank=Smooth(bank,turn*Mathf.Clamp01(speed/4),10,dt);
   if(Mathf.Abs(yawRate)>170&&Time.time-cutAt>.25f&&speed>.8f){cutAt=Time.time;cutSign=Mathf.Sign(yawRate);}
   if(held&&!heldBefore)receiveAt=Time.time;heldBefore=held;
   // Distance and current speed drive a continuous phase, not a restarted clip.
   phase=Mathf.Repeat(phase+dt*Mathf.Lerp(held?.8f:.9f,held?1.4f:2.4f,Mathf.Clamp01(speed/(held?5.25f:7f)))*Mathf.Clamp01(speed/.6f),1);
   previousSpeed=speed;float gait=Ease(speed/1.2f),elapsed=(float)(Clock-State.started),stopTime=Time.time-stopAt;
   bool hit=athlete.Action==FootballAction.Hit,slide=athlete.Action==FootballAction.Slide;
   float gestureDuration=State.gesture==FootballGesture.Kick?.35f:State.gesture==FootballGesture.Cancel?.18f:.22f;
   bool gesture=State.gesture!=FootballGesture.None&&elapsed<gestureDuration&&!FootballMatch.BlocksActions;
   pose=Take(held?(speed>3.3f?"Dribble_Fast":"Dribble_Control"):speed>.2f?"Walk":"Ready",phase);
   if(!held&&speed<.2f)pose=Take("Ready",(float)(Clock/2.8));
   bool authored=speed>1.25f&&!hit;float contact=0;int foot=phase<.5f?0:1;
   if(stopTime<.30f){pose=Take(held?(stopLeft?"BallSettle_L":"BallSettle_R"):(stopLeft?"Stop_L":"Stop_R"),stopTime/(held?.28f:.30f));}
   else if(Time.time-receiveAt<.22f)pose=Take(foot==0?"Receive_L":"Receive_R",(Time.time-receiveAt)/.22f);
   else if(startAccent>.08f){pose=FootballPose.Lerp(pose,Take(foot==0?"Start_L":"Start_R",.36f),startAccent*.65f);}
   if(Time.time-cutAt<.25f&&!gesture)pose=FootballPose.Lerp(pose,Take(cutSign<0?"Cut_L":"Cut_R",(Time.time-cutAt)/.25f),Mathf.Abs(bank)*.65f);
   pose.body.z-=bank*7;pose.chest.y+=turn*6;pose.head.y+=turn*7;pose.pelvis.y-=brake*.025f;
   if(athlete.WhiffRemaining>0)pose=FootballPose.Lerp(pose,Take("WhiffRecover",1-athlete.WhiffRemaining/.8f),.75f);
   if(State.charging&&!gesture&&!FootballMatch.BlocksActions){
    float charge=Ease((float)(Clock-State.chargeStarted));pose.body.x-=charge*5;pose.chest.y+=(State.left?-1:1)*charge*8;
    if(speed<.3f){if(State.left){pose.leftFoot.y+=.03f*charge;pose.leftFoot.z-=.065f*charge;}else{pose.rightFoot.y+=.03f*charge;pose.rightFoot.z-=.065f*charge;}}
    Performance="Charge";
   }
   if(gesture){var name=State.gesture==FootballGesture.Kick?(State.left?"Kick_L":"Kick_R"):State.gesture==FootballGesture.Cancel?(State.left?"FakeCancel_L":"FakeCancel_R"):"Bump";var p=Take(name,elapsed/gestureDuration);if(name=="Bump"&&State.left)p=p.Mirror();float power=State.gesture==FootballGesture.Kick?Mathf.Lerp(.65f,1,State.power):1;pose=FootballPose.Lerp(pose,p,power*(1-Ease((elapsed-(gestureDuration-.10f))/.10f)));foot=State.left?0:1;}
   var match=FootballMatch.Instance;var matchPhase=match&&match.Context?match.Snapshot.phase:FootballMatchPhase.Idle;
   if(matchPhase!=lastMatchPhase){lastMatchPhase=matchPhase;resultAt=Clock;}
   if(matchPhase==FootballMatchPhase.Finished){var team=match.TeamOf(athlete);bool won=match.Snapshot.result==FootballMatchResult.TeamA&&team==FootballTeam.A||match.Snapshot.result==FootballMatchResult.TeamB&&team==FootballTeam.B;pose=Take(won?"Result_Win":"Result_Disappointed",(float)(Clock-resultAt)/1.7f);authored=false;}
   if(hit){var variant=athlete.HitVariant;pose=Take(variant==0?"Fall_Back":variant==1?"Fall_Forward":variant==2?"Fall_Left":"Fall_Right",athlete.ActionProgress);authored=false;}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(!string.IsNullOrEmpty(ReviewTake)){pose=Take(ReviewTake,ReviewTime);authored=false;}
#endif
   if(slide){Performance="Slide_Tackle";return;} // Retain the saved authored slide and its hand/leg recovery.
   for(int i=0;i<bones.Length;i++){beforePosition[i]=bones[i].localPosition;beforeRotation[i]=bones[i].localRotation;}applied=true;
   var leftPos=legs[0].end.position;var rightPos=legs[1].end.position;var leftRot=legs[0].end.rotation;var rightRot=legs[1].end.rotation;
   if(!authored){for(int i=0;i<bones.Length;i++){bones[i].localPosition=referencePosition[i];bones[i].localRotation=referenceRotation[i];}hips.position=transform.TransformPoint(hipReference);}
   // A wall-shortened fall stays compact. It may change the visible lean but
   // never the authoritative capsule or hit duration.
   var reachHead=hips.position+transform.TransformVector(pose.pelvis)+transform.rotation*Quaternion.Euler(pose.body)*Vector3.up*.60f;
   if(Physics.Linecast(transform.position+Vector3.up*.45f,reachHead,out var obstruction,1<<8,QueryTriggerInteraction.Ignore)&&Mathf.Abs(obstruction.normal.y)<.5f){pose.body*=.42f;pose.pelvis.x*=.4f;pose.pelvis.z*=.4f;}
   hips.position+=transform.TransformVector(pose.pelvis);
   Rotate(hips,pose.body);Rotate(spine,pose.chest*.4f);Rotate(chest,pose.chest*.6f);Rotate(head,pose.head-pose.body*.25f);
   MaximumReachError=BallContactError=ContactWeight=0;
   for(int i=0;i<2;i++){
    var offset=i==0?pose.leftFoot:pose.rightFoot;var target=transform.TransformPoint(feet[i]+offset);var rotation=transform.rotation*footRotation[i]*Quaternion.AngleAxis(i==0?pose.leftToe:pose.rightToe,Vector3.right);
    if(authored){target=(i==0?leftPos:rightPos)+transform.TransformVector(offset*.35f);rotation=i==0?leftRot:rightRot;}
    else if(speed>.15f&&!hit&&stopTime>.3f){
     float f=Mathf.Repeat(phase+i*.5f,1);float z=f<.6f?Mathf.Lerp(.09f,-.09f,f/.6f):Mathf.Lerp(-.09f,.09f,Ease((f-.6f)/.4f));float lift=f>.6f?.065f*Mathf.Sin((f-.6f)/.4f*Mathf.PI):0;
     target+=transform.forward*z*gait+Vector3.up*lift*gait;rotation=Quaternion.AngleAxis(f>.6f?-12*Mathf.Sin((f-.6f)/.4f*Mathf.PI):6*Mathf.Sin(f/.6f*Mathf.PI),transform.right)*rotation;
    }
    if(held&&!hit&&speed>.2f&&!State.charging&&!gesture){float f=Mathf.Repeat(phase*2,1);contact=Ease(f/.16f)*(1-Ease((f-.32f)/.20f));if(i==foot)target=BallTarget(i,target,contact,rotation,out rotation);}
    if(held&&Time.time-receiveAt<.22f&&!hit&&i==foot){contact=Mathf.Sin(Mathf.PI*Mathf.Clamp01((Time.time-receiveAt)/.22f));target=BallTarget(i,target,contact,rotation,out rotation);}
    if(gesture&&State.gesture==FootballGesture.Kick&&i==foot&&!hit){float strike=1-Ease(elapsed/.09f);Vector3 center=transform.TransformPoint(State.contact);var touch=ContactAnkle(i,center);target=Vector3.Lerp(target,touch,strike);rotation=Quaternion.Slerp(rotation,transform.rotation*footRotation[i],strike);}
    if(stopTime<.18f&&speed<.35f&&!hit&&!gesture&&i==(stopLeft?0:1)&&Mathf.Abs(yawRate)<90)target=Vector3.Lerp(stopPins[i],target,Ease(stopTime/.18f));
    // The world floor has final say. Feet are never pulled below it to meet a
    // decorative target. World walls also shorten a reach without moving roots.
    if(Physics.Raycast(target+Vector3.up*.5f,Vector3.down,out var floor,.9f,1<<8,QueryTriggerInteraction.Ignore))target.y=Mathf.Max(target.y,floor.point.y+feet[i].y-.018f);
    Solve(legs[i],target,transform.forward,rotation);
    if(i==foot&&contact>.9f&&!hit){ContactWeight=contact;var ball=FootballBall.Instance;if(ball){var toe=legs[i].toe?legs[i].toe.position:legs[i].end.position;BallContactError=Mathf.Abs(Vector3.Distance(toe,ball.transform.position)-ball.WorldRadius);}}
   }
   for(int i=0;i<2;i++){
    var local=i==0?pose.leftHand:pose.rightHand;
    if(!hit&&!gesture){float swing=Mathf.Sin((phase+i*.5f)*Mathf.PI*2)*Mathf.Lerp(.025f,.10f,Mathf.Clamp01(speed/7));local.z+=swing*gait;local.y+=Mathf.Max(0,swing)*.3f;}
    var arm=arms[i];var rotation=arm.end.rotation;var target=transform.TransformPoint(local);
    Vector3 pole=transform.right*(i==0?-1:1)*.7f-transform.forward*.25f-Vector3.up*.3f;
    if(hit&&athlete.ActionProgress>.25f&&athlete.ActionProgress<.72f)pole=transform.right*(i==0?-1:1)*.7f+transform.forward*.2f;
    Solve(arm,target,pole,rotation);arm.end.rotation=arm.lower.rotation*arm.wristRest;
    if(hit&&arm.toe&&Physics.Raycast(arm.toe.position+Vector3.up*.5f,Vector3.down,out var handFloor,1,1<<8,QueryTriggerInteraction.Ignore)){
     for(int correction=0;correction<2;correction++){float lift=handFloor.point.y+.025f-arm.toe.position.y;if(lift<=0)break;target+=Vector3.up*Mathf.Min(lift,.09f);Solve(arm,target,pole,rotation);arm.end.rotation=arm.lower.rotation*arm.wristRest;}
    }
   }
   if(weight<.999f)for(int i=0;i<bones.Length;i++){bones[i].localPosition=Vector3.Lerp(beforePosition[i],bones[i].localPosition,weight);bones[i].localRotation=Quaternion.Slerp(beforeRotation[i],bones[i].localRotation,weight);}
  }
  void Rotate(Transform bone,Vector3 angles){bone.rotation=transform.rotation*Quaternion.Euler(angles)*Quaternion.Inverse(transform.rotation)*bone.rotation;}
  Vector3 ContactAnkle(int side,Vector3 center){
   // Toe landmark measures this mesh's actual shoe length. Work back from the
   // ball's near surface and use the inside edge; do not target its centre.
   var leg=legs[side];float toe=leg.toe?Mathf.Clamp(Vector3.Distance(leg.end.position,leg.toe.position),.1f,.25f):.18f;
   float radius=FootballBall.Instance?FootballBall.Instance.WorldRadius:.11f;
   return center-transform.forward*(radius+toe*.8f)+transform.right*(side==0?-.045f:.045f)+Vector3.up*.015f;
  }
  Vector3 BallTarget(int side,Vector3 from,float blend,Quaternion source,out Quaternion rotation){rotation=Quaternion.Slerp(source,transform.rotation*footRotation[side],Ease(blend/.8f));var ball=FootballBall.Instance;return ball?Vector3.Lerp(from,ContactAnkle(side,ball.FootPosition(athlete)),blend):from;}
  void Solve(Limb l,Vector3 target,Vector3 pole,Quaternion endRotation){
   if(Physics.Linecast(l.upper.position,target,out var wall,1<<8,QueryTriggerInteraction.Ignore)&&Mathf.Abs(wall.normal.y)<.5f)target=wall.point+wall.normal*.025f;
   Vector3 a=l.upper.position,b=l.lower.position,c=l.end.position;float upper=Vector3.Distance(a,b),lower=Vector3.Distance(b,c);Vector3 delta=target-a;float reach=delta.magnitude;if(reach<.001f)return;
   float length=Mathf.Clamp(reach,Mathf.Abs(upper-lower)+.003f,upper+lower-.006f);Vector3 axis=delta/reach;
   Vector3 bend=Vector3.ProjectOnPlane(pole,axis).normalized;if(bend.sqrMagnitude<.01f)bend=Vector3.ProjectOnPlane(transform.right,axis).normalized;
   float along=(upper*upper-lower*lower+length*length)/(2*length);var joint=a+axis*along+bend*Mathf.Sqrt(Mathf.Max(0,upper*upper-along*along));
   l.upper.rotation=Quaternion.FromToRotation(b-a,joint-a)*l.upper.rotation;l.lower.rotation=Quaternion.FromToRotation(l.end.position-l.lower.position,a+axis*length-l.lower.position)*l.lower.rotation;l.end.rotation=endRotation;
   MaximumReachError=Mathf.Max(MaximumReachError,Mathf.Max(0,reach-length));
  }
 }
}
