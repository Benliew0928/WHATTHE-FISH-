using System;
using UnityEngine;

namespace WhatTheFish {
 // The motor and ball remain authoritative. This component owns only the
 // football presentation. One coordinated gait drives all four limbs. Animator
 // is restored before evaluation, keeping the other sports' clips untouched.
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
  public float GaitPhase=>gait.Phase;
  public bool LeftPlanted=>gait.Planted[0]&&contactBlend>.99f;
  public bool RightPlanted=>gait.Planted[1]&&contactBlend>.99f;
  public Vector3 LeftFoot=>legs[0].end.position;
  public Vector3 RightFoot=>legs[1].end.position;
  public static double Clock=>BasketballMotion.Clock;
  public FootballMotionLibrary library;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public string ReviewTake;public float ReviewTime;public bool ReviewOriginal;
  public double LastPoseMilliseconds {get;private set;}
  public long LastPoseAllocatedBytes {get;private set;}
#endif
  Athlete athlete;Transform hips,spine,chest,head;Limb[] legs,arms;
  Transform[] bones;Vector3[] beforePosition,referencePosition,lastPosition,entryPosition;Quaternion[] beforeRotation,referenceRotation,lastRotation,entryRotation;
  Vector3[] feet;Quaternion[] footRotation;Vector3 hipReference;
  readonly Vector3[] footTarget=new Vector3[2],footVelocity=new Vector3[2];bool feetInitialized;
  readonly Vector3[] displayedFeet=new Vector3[2],settleFrom=new Vector3[2];readonly float[] touchWeights=new float[2];float settleAt=-10,wallFreedom=1,groundOffset,jumpTime;int receiveFoot,settleLead;
  bool applied,heldBefore,received,haveLast;uint receivedSequence;float weight,phase,previousSpeed,previousYaw,brake,startAccent,turn,bank,receiveAt=-10;
  readonly FootballGait gait=new();float effortBlend,transitionAge=1,contactBlend,actionAge=10,actionDuration,armActionWeight;int mode,lastMode=-1,fallVariant;FootballAction lastAction,previousAction;
  Vector3 smoothBody,smoothPelvis,bodyVelocity,pelvisVelocity;Quaternion chestReference;float smoothedYaw,yawVelocity;
  double resultAt;FootballMatchPhase lastMatchPhase;Vector3 previousPosition;FootballPose pose;
  sealed class Limb {public Transform upper,lower,end,toe;public float upperLength,lowerLength;public Quaternion wristRest,palmRest,upperBasis,lowerBasis;public Vector3 toeOffset;}
  public static int FallVariant(Vector3 push,Quaternion facing){var v=Quaternion.Inverse(facing)*push;return Mathf.Abs(v.x)>Mathf.Abs(v.z)?(v.x<0?2:3):(v.z<0?0:1);}
  public void Bind(Athlete owner){
   if(RigReady)return;athlete=owner;if(!owner.visual)return;
   var all=owner.visual.GetComponentsInChildren<Transform>(true);bones=Array.FindAll(all,t=>t.name.StartsWith("mixamorig:"));
   Transform Bone(string n)=>Array.Find(bones,t=>t.name=="mixamorig:"+n);
   Limb Make(string side,bool arm){var l=new Limb{upper=Bone(side+(arm?"Arm":"UpLeg")),lower=Bone(side+(arm?"ForeArm":"Leg")),end=Bone(side+(arm?"Hand":"Foot")),toe=Bone(side+(arm?"HandMiddle4":"Toe_End"))};if(l.upper&&l.lower&&l.end){l.upperLength=Vector3.Distance(l.upper.position,l.lower.position);l.lowerLength=Vector3.Distance(l.lower.position,l.end.position);}return l;}
   hips=Bone("Hips");spine=Bone("Spine");chest=Bone("Spine2");head=Bone("Head");legs=new[]{Make("Left",false),Make("Right",false)};arms=new[]{Make("Left",true),Make("Right",true)};
   if(!hips||!spine||!chest||!head)return;foreach(var limb in legs)if(!limb.end)return;foreach(var limb in arms)if(!limb.end)return;
   foreach(var limb in arms){limb.wristRest=Quaternion.Inverse(limb.lower.rotation)*limb.end.rotation;var finger=limb.toe?limb.toe.position-limb.end.position:Vector3.down;limb.palmRest=Quaternion.Inverse(transform.rotation)*Quaternion.FromToRotation(finger,transform.forward)*limb.end.rotation;}
   foreach(var group in new[]{legs,arms})foreach(var limb in group){var u=limb.lower.position-limb.upper.position;var l=limb.end.position-limb.lower.position;var normal=Vector3.Cross(u,l).normalized;if(normal.sqrMagnitude<.01f)normal=transform.right;limb.upperBasis=Quaternion.Inverse(Quaternion.LookRotation(u,normal))*limb.upper.rotation;limb.lowerBasis=Quaternion.Inverse(Quaternion.LookRotation(l,normal))*limb.lower.rotation;}
   foreach(var group in new[]{legs,arms})foreach(var limb in group)limb.toeOffset=limb.toe?Quaternion.Inverse(limb.end.rotation)*(limb.toe.position-limb.end.position):Vector3.zero;
   var placement=owner.GetComponent<TurnFootPlacement>();feet=new Vector3[2];footRotation=new Quaternion[2];
   for(int i=0;i<2;i++){var stored=placement?(i==0?placement.left:placement.right):null;feet[i]=stored!=null?stored.initialPosition:transform.InverseTransformPoint(legs[i].end.position);footRotation[i]=stored!=null?stored.initialRotation:Quaternion.Inverse(transform.rotation)*legs[i].end.rotation;}
   hipReference=transform.InverseTransformPoint(hips.position);
   beforePosition=new Vector3[bones.Length];referencePosition=new Vector3[bones.Length];beforeRotation=new Quaternion[bones.Length];referenceRotation=new Quaternion[bones.Length];
   lastPosition=new Vector3[bones.Length];entryPosition=new Vector3[bones.Length];lastRotation=new Quaternion[bones.Length];entryRotation=new Quaternion[bones.Length];
   for(int i=0;i<bones.Length;i++){referencePosition[i]=bones[i].localPosition;referenceRotation[i]=bones[i].localRotation;}
   library=Resources.Load<FootballMotionLibrary>("FootballMotionLibrary");
   if(!library){Debug.LogError("Missing FootballMotionLibrary; run FootballMotionBuilder.Prepare before building.");return;}
   RigReady=true;previousPosition=transform.position;smoothedYaw=previousYaw=transform.eulerAngles.y;chestReference=Quaternion.Inverse(transform.rotation)*chest.rotation;
  }
  void ApplyFrame(){
   if(!RigReady)return;
   float dt=Mathf.Clamp(Time.deltaTime,.0001f,.1f);
   if(!Active){weight=0;haveLast=false;gait.Reset();return;}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(ReviewOriginal){weight=0;haveLast=false;return;}
#endif
   var displacement=Vector3.ProjectOnPlane(transform.position-previousPosition,Vector3.up);previousPosition=transform.position;
   if(displacement.magnitude>2){gait.Reset();haveLast=false;displacement=Vector3.zero;}
   bool jump=athlete.PresentingJump;
   jumpTime=athlete.JumpPose;
   // Controller skin clearance belongs to collision, not the rendered sole.
   // Retain this offset in flight so takeoff/landing have the same origin.
   if(!athlete.Airborne&&Physics.Raycast(transform.position+Vector3.up*.2f,Vector3.down,out var ground,.55f,1<<8,QueryTriggerInteraction.Ignore))groundOffset=Mathf.Clamp(transform.position.y-ground.point.y,0,.12f);
   float speed=athlete.Action==FootballAction.None?displacement.magnitude/dt:0;
   float accel=(speed-previousSpeed)/dt;
   if(previousSpeed>.10f&&speed<=.10f&&haveLast){settleAt=Time.time;settleLead=displayedFeet[0].y<displayedFeet[1].y?0:1;for(int i=0;i<2;i++)settleFrom[i]=displayedFeet[i];}
   if(speed>.15f)settleAt=-10;previousSpeed=speed;
   brake=Smooth(brake,Mathf.Clamp01(-accel/45),14,dt);startAccent=Smooth(startAccent,Mathf.Clamp01(accel/45),14,dt);
   float yaw=transform.eulerAngles.y,yawRate=Mathf.DeltaAngle(previousYaw,yaw)/dt;previousYaw=yaw;
   smoothedYaw=Mathf.SmoothDampAngle(smoothedYaw,yaw,ref yawVelocity,.065f,1440,dt);
   turn=Smooth(turn,Mathf.Clamp(yawRate/500,-1,1),12,dt);bank=Smooth(bank,turn*Mathf.Clamp01(speed/4),10,dt);
   bool held=athlete.ControlsFootball;if(held&&!heldBefore){receiveAt=Time.time;receiveFoot=gait.Swing(0)>gait.Swing(1)?0:1;}heldBefore=held;
   gait.Advance(displacement,transform.rotation,dt,jump||athlete.Action!=FootballAction.None);phase=gait.Phase;
   var action=athlete.Action;
   if(action!=previousAction&&action!=FootballAction.None){actionAge=athlete.ActionProgress*FootballTackle.Duration(action);actionDuration=action==FootballAction.Hit?1.10f:.68f;fallVariant=athlete.HitVariant;}
   else actionAge+=dt;
   previousAction=action;
   if(action!=FootballAction.None)lastAction=action;
   bool reacting=actionAge<actionDuration;
   if(jump&&action==FootballAction.None){reacting=false;actionAge=actionDuration;}
   bool hit=reacting&&lastAction==FootballAction.Hit;
   bool slide=reacting&&lastAction==FootballAction.Slide;
   if(!reacting)lastAction=FootballAction.None;
   float elapsed=(float)(Clock-State.started);
   float gestureDuration=State.gesture==FootballGesture.Kick?.35f:State.gesture==FootballGesture.Cancel?.24f:.28f;
   bool gesture=State.gesture!=FootballGesture.None&&elapsed>=0&&elapsed<gestureDuration&&!FootballMatch.BlocksActions&&!reacting;
   var match=FootballMatch.Instance;var matchPhase=match&&match.Context?match.Snapshot.phase:FootballMatchPhase.Idle;
   if(matchPhase!=lastMatchPhase){lastMatchPhase=matchPhase;resultAt=Clock;}
   pose=Take(held?(speed>3.3f?"Dribble_Fast":"Dribble_Control"):speed>.15f?"Walk":"Ready",speed>.15f?phase:(float)(Clock/2.8));
   // A coordinated pelvis/chest counter-rotation. The support side takes the
   // weight; vertical flight is deliberately small on this oversized-head rig.
   float wave=Mathf.Sin(phase*Mathf.PI*2),doubleWave=Mathf.Cos(phase*Mathf.PI*4);
   pose.pelvis+=new Vector3(-wave*.009f*gait.Amount,-.020f*gait.Amount+.010f*gait.Run*doubleWave,.018f*gait.Run);
   if(!held)pose.pelvis.y-=.025f*gait.Run;
   pose.body+=new Vector3(startAccent*5-brake*8+6*gait.Run,Mathf.DeltaAngle(yaw,smoothedYaw)+wave*3*gait.Amount,-bank*6-wave*2*gait.Amount);
   pose.chest+=new Vector3(-2*gait.Run,-wave*6*gait.Amount+turn*5,bank*2);
   pose.head+=new Vector3(-2*gait.Run,turn*6-wave*1.5f*gait.Amount,bank*2);
   pose.pelvis.y-=brake*.025f;
   if(athlete.WhiffRemaining>0){pose.body.x+=5;pose.pelvis.y-=.012f;Performance="WhiffRecover";}
   effortBlend=Smooth(effortBlend,athlete.FootballEffort.Pressuring?1:0,12,dt);
   pose.pelvis.y-=.045f*effortBlend;pose.body.x+=8*effortBlend;pose.chest.x-=4*effortBlend;
   if(effortBlend>.5f)Performance="Pressure";
   if(athlete.FootballEffort.Dashing){pose.body.x+=4*gait.Amount;Performance="Dash";}
   float receive=1-Ease((Time.time-receiveAt)/.24f);
   if(receive>0){pose.chest.y-=5*receive;Performance="Receive";}
   if(State.charging&&!reacting){
    float charge=Ease((float)(Clock-State.chargeStarted));pose.body.x-=charge*4;pose.chest.y+=(State.left?-1:1)*charge*9;
    if(speed<.3f){if(State.left){pose.leftFoot.y+=.025f*charge;pose.leftFoot.z-=.045f*charge;}else{pose.rightFoot.y+=.025f*charge;pose.rightFoot.z-=.045f*charge;}}Performance="Charge";
   }
   armActionWeight=0;mode=jump?1:0;
   if(jump){pose=Take("Jump",athlete.JumpPose);armActionWeight=1;}
   if(gesture){
    string name=State.gesture==FootballGesture.Kick?(State.left?"Kick_L":"Kick_R"):State.gesture==FootballGesture.Cancel?(State.left?"FakeCancel_L":"FakeCancel_R"):"Bump";
    var p=Take(name,elapsed/gestureDuration);if(name=="Bump"&&State.left)p=p.Mirror();
    armActionWeight=1-Ease((elapsed-(gestureDuration-.12f))/.12f);
    pose=FootballPose.Lerp(pose,p,armActionWeight);mode=10+(int)State.gesture+(State.left?10:0);
   }
   var locomotionPose=pose;
   if(hit){pose=Take(fallVariant==0?"Fall_Back":fallVariant==1?"Fall_Forward":fallVariant==2?"Fall_Left":"Fall_Right",actionAge/actionDuration);mode=30+fallVariant;armActionWeight=1;}
   if(slide){pose=Take("Slide_Tackle",actionAge/actionDuration);mode=35;armActionWeight=1;}
   float recoveryMove=reacting&&action==FootballAction.None?FootballGait.Ease((actionAge-FootballTackle.Duration(lastAction))/.24f)*gait.Amount:0;
   if(recoveryMove>0){pose=FootballPose.Lerp(pose,locomotionPose,recoveryMove);armActionWeight*=1-recoveryMove;}
   var headReach=transform.TransformPoint(hipReference+pose.pelvis)+transform.rotation*Quaternion.Euler(pose.body)*Vector3.up*.60f;
   bool obstructed=reacting&&Physics.Linecast(transform.position+Vector3.up*.45f,headReach,out var wall,1<<8,QueryTriggerInteraction.Ignore)&&Mathf.Abs(wall.normal.y)<.5f;
   wallFreedom=Smooth(wallFreedom,obstructed?.45f:1,14,dt);if(reacting){pose.body*=wallFreedom;pose.pelvis.y*=wallFreedom;}
   if(matchPhase==FootballMatchPhase.Finished){var team=match.TeamOf(athlete);bool won=match.Snapshot.result==FootballMatchResult.TeamA&&team==FootballTeam.A||match.Snapshot.result==FootballMatchResult.TeamB&&team==FootballTeam.B;pose=Take(won?"Result_Win":"Result_Disappointed",(float)(Clock-resultAt)/1.7f);mode=40;armActionWeight=1;}
   bool review=false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(!string.IsNullOrEmpty(ReviewTake)){pose=Take(ReviewTake,ReviewTime);mode=ReviewTake=="Jump"?1:50;jumpTime=ReviewTime;armActionWeight=1;review=true;}
#endif
   for(int i=0;i<bones.Length;i++){beforePosition[i]=bones[i].localPosition;beforeRotation[i]=bones[i].localRotation;}applied=true;
   weight=1;
   {
    for(int i=0;i<bones.Length;i++){bones[i].localPosition=referencePosition[i];bones[i].localRotation=referenceRotation[i];}
    hips.position=transform.TransformPoint(hipReference)-Vector3.up*groundOffset;
    // Damping applies to locomotion posture only; it never smears a planted
    // foot or adds a delay to the motor, ball release or tackle contacts.
    smoothBody=Vector3.SmoothDamp(smoothBody,pose.body,ref bodyVelocity,.075f,1200,dt);
    smoothPelvis=Vector3.SmoothDamp(smoothPelvis,pose.pelvis,ref pelvisVelocity,.075f,4,dt);
    var body=mode==0?smoothBody:pose.body;var pelvis=mode==0?smoothPelvis:pose.pelvis;
    hips.position+=transform.TransformVector(pelvis);
    Rotate(hips,body);Rotate(spine,pose.chest*.4f);Rotate(chest,pose.chest*.6f);Rotate(head,pose.head-body*.22f);
    MaximumReachError=BallContactError=ContactWeight=0;
    for(int i=0;i<2;i++){
     var offset=i==0?pose.leftFoot:pose.rightFoot;
     gait.Target(i,transform,feet[i]+offset,footRotation[i],groundOffset,dt,out var target,out var rotation);
     if(jump||reacting||review||mode==40) {target=Vector3.Lerp(transform.TransformPoint(feet[i]+offset)-Vector3.up*groundOffset,target,recoveryMove);rotation=Quaternion.Slerp(transform.rotation*footRotation[i],rotation,recoveryMove);}
     rotation=Quaternion.AngleAxis(i==0?pose.leftToe:pose.rightToe,transform.right)*rotation;
     float contact=0;touchWeights[i]=0;
     if(held&&!reacting&&!State.charging&&!gesture&&speed>.2f){
      // Touch only in the forward part of THIS foot's swing, never on its
      // supporting step. Arms use precisely the same phase below.
      float u=gait.Swing(i);contact=FootballGait.Ease((u-.28f)/.36f)*(1-FootballGait.Ease((u-.72f)/.28f));
      var ball=FootballBall.Instance;
      if(ball&&contact>0){var touchRotation=transform.rotation*footRotation[i];target=Vector3.Lerp(target,ContactAnkle(i,ball.FootPosition(athlete),touchRotation),contact);rotation=Quaternion.Slerp(rotation,touchRotation,contact);}
     }
     float receiving=(Time.time-receiveAt)/.34f,settling=(Time.time-settleAt)/.34f;
     if(held&&!reacting&&!gesture&&!State.charging&&i==receiveFoot&&((receiving>=0&&receiving<1)||(settling>=0&&settling<1))){
      float t=receiving<1?receiving:settling;contact=Mathf.Pow(Mathf.Sin(Mathf.PI*t),2);var touchRotation=transform.rotation*footRotation[i];var ball=FootballBall.Instance;
      if(ball){target=Vector3.Lerp(target,ContactAnkle(i,ball.FootPosition(athlete),touchRotation),contact);rotation=Quaternion.Slerp(rotation,touchRotation,contact);}
     }
     if(gesture&&State.gesture==FootballGesture.Kick&&i==(State.left?0:1)){
      float strike=(1-Ease(elapsed/.10f));var touchRotation=transform.rotation*footRotation[i];target=Vector3.Lerp(target,ContactAnkle(i,transform.TransformPoint(State.contact),touchRotation),strike);rotation=Quaternion.Slerp(rotation,touchRotation,strike);
     }
     if(!jump&&!reacting&&!review&&mode!=40){
      var local=transform.InverseTransformPoint(target);
      if(!feetInitialized){footTarget[i]=feet[i];footVelocity[i]=Vector3.zero;}
      if(gait.Planted[i]){footTarget[i]=local;footVelocity[i]=-transform.InverseTransformVector(displacement/dt);}
      else {
       float landing=FootballGait.Ease((gait.Swing(i)-.70f)/.30f);
       float damping=held?Mathf.Lerp(.0105f,.045f,Mathf.Abs(turn)*(1-contact)):Mathf.Lerp(.038f,.008f,Mathf.Max(Mathf.InverseLerp(7,10,gait.Speed),Mathf.Max(contact,landing)));
       // SmoothDamp's speed clamp is multiplied by smoothTime, not deltaTime.
       // With an 8ms contact response it truncated legitimate 50ms frames,
       // leaving the shoe behind the ball. Damping and IK bound the motion.
       footTarget[i]=Vector3.SmoothDamp(footTarget[i],local,ref footVelocity[i],damping,Mathf.Infinity,dt);target=transform.TransformPoint(footTarget[i]);
      }
     }
     if(!held&&!jump&&!reacting&&!review&&Time.time-settleAt<.34f){
      float t=Mathf.Clamp01((Time.time-settleAt-(i==settleLead?.10f:0))/.24f);var neutral=transform.TransformPoint(feet[i]+offset)-Vector3.up*groundOffset;
      float step=Mathf.Clamp01(Vector3.ProjectOnPlane(neutral-settleFrom[i],Vector3.up).magnitude/.04f);
      target=Vector3.Lerp(settleFrom[i],neutral,FootballGait.Ease(t))+Vector3.up*(.028f*step*Mathf.Pow(Mathf.Sin(Mathf.PI*t),2));
      footTarget[i]=transform.InverseTransformPoint(target);footVelocity[i]=Vector3.zero;
     }
     FloorFoot(i,ref target,rotation);
     var knee=transform.rotation*Quaternion.Euler(0,body.y,0)*new Vector3(i==0?-.10f:.10f,Mathf.Clamp01(-pelvis.y/.20f)*.85f,1);
     Solve(legs[i],target,knee,rotation);
     touchWeights[i]=contact;
    }
    feetInitialized=true;
    for(int i=0;i<2;i++)Arm(i,hit||slide,review,dt);
   }
   // Inertialize complete skeletons on entry/exit, including jump and slide.
   // Offsets are measured from the last displayed pose, not the bind pose.
   if(mode!=lastMode){
    transitionAge=0;lastMode=mode;
    for(int i=0;i<bones.Length;i++){entryPosition[i]=haveLast?lastPosition[i]-bones[i].localPosition:Vector3.zero;entryRotation[i]=haveLast?Quaternion.Inverse(bones[i].localRotation)*lastRotation[i]:Quaternion.identity;}
   }
   transitionAge+=dt;float remain=1-FootballGait.Ease(transitionAge/(jump?.16f:reacting?.14f:.16f));
   for(int i=0;i<bones.Length;i++){
    bones[i].localPosition+=entryPosition[i]*remain;
    bones[i].localRotation*=Quaternion.Slerp(Quaternion.identity,entryRotation[i],remain);
   }
   // Final contact correction also covers inertialized entry poses, which can
   // otherwise put a heel below the surface even when the target pose is safe.
   for(int i=0;i<2;i++){
    var target=legs[i].end.position;FloorFoot(i,ref target,legs[i].end.rotation);
    if(target.y>legs[i].end.position.y+.0005f){var leg=legs[i];var axis=target-leg.upper.position;var bend=Vector3.ProjectOnPlane(leg.lower.position-leg.upper.position,axis);Solve(leg,target,bend,leg.end.rotation);}
    if(reacting||review&&(Performance.StartsWith("Fall")||Performance=="Slide_Tackle")){var arm=arms[i];var tip=arm.end.position+arm.end.rotation*arm.toeOffset;float min=Mathf.Min(arm.end.position.y-.05f,tip.y-.05f);
     if(Physics.Raycast(arm.end.position+Vector3.up*.3f,Vector3.down,out var floor,.8f,1<<8,QueryTriggerInteraction.Ignore)&&min<floor.point.y+.005f){var axis=arm.end.position-arm.upper.position;var bend=Vector3.ProjectOnPlane(arm.lower.position-arm.upper.position,axis);Solve(arm,arm.end.position+Vector3.up*(floor.point.y+.005f-min),bend,arm.end.rotation);}
    }
   }
   for(int i=0;i<bones.Length;i++){lastPosition[i]=bones[i].localPosition;lastRotation[i]=bones[i].localRotation;}
   haveLast=true;contactBlend=mode==0&&!gesture&&!review?1-remain:0;
   // Contact is measured after the final visible blend, not on an intermediate
   // IK result. This is the same skeleton the renderer and the review see.
   BallContactError=ContactWeight=0;
   for(int i=0;i<2;i++){displayedFeet[i]=legs[i].end.position;if(touchWeights[i]>.90f&&FootballBall.Instance&&legs[i].toe){ContactWeight=Mathf.Max(ContactWeight,touchWeights[i]);BallContactError=Mathf.Max(BallContactError,Mathf.Abs(Vector3.Distance(legs[i].toe.position,FootballBall.Instance.transform.position)-FootballBall.Instance.WorldRadius));}}
  }
  void Arm(int side,bool ground,bool review,float dt){
   var arm=arms[side];float sign=side==0?-1:1;
   float f=gait.FootPhase(side),wave=Mathf.Cos(f*Mathf.PI*2);
   float shoulder=-wave*Mathf.Lerp(16,32,gait.Run)*gait.Amount+3+sign*bank*8;
   float flex=Mathf.Lerp(18,82,gait.Run*gait.Amount)+wave*8*gait.Amount;
   if(mode==1){float t=jumpTime,lift=FootballGait.Ease(t/.40f)*(1-FootballGait.Ease((t-.48f)/.45f));shoulder=-12*(1-FootballGait.Ease(t/.18f))+65*lift;flex=18+35*lift;}
   var frame=chest.rotation*Quaternion.Inverse(chestReference);
   Vector3 Direction(float degrees)=>new Vector3(sign*(.17f+Mathf.Abs(bank)*.16f),-Mathf.Cos(degrees*Mathf.Deg2Rad),Mathf.Sin(degrees*Mathf.Deg2Rad)).normalized;
   arm.upper.rotation=Quaternion.FromToRotation(arm.lower.position-arm.upper.position,frame*Direction(shoulder))*arm.upper.rotation;
   arm.lower.rotation=Quaternion.FromToRotation(arm.end.position-arm.lower.position,frame*Direction(shoulder+flex))*arm.lower.rotation;
   var wrist=Quaternion.AngleAxis(-5-wave*3*gait.Amount,frame*Vector3.right)*arm.lower.rotation*arm.wristRest;
   arm.end.rotation=wrist;
   if(armActionWeight<=0||mode==1)return;
   var target=Vector3.Lerp(arm.end.position,transform.TransformPoint(side==0?pose.leftHand:pose.rightHand)-Vector3.up*groundOffset,armActionWeight);
   // Preserve the authored FK bend plane. Ground bracing opens continuously,
   // instead of changing the elbow guide abruptly at timeline thresholds.
   var axis=(arm.end.position-arm.upper.position).normalized;
   var pole=Vector3.ProjectOnPlane(arm.lower.position-arm.upper.position,axis).normalized;
   float brace=ground?FootballGait.Ease(actionAge/.16f)*(1-FootballGait.Ease((actionAge-.62f)/.30f)):0;
   if(review)brace=Performance.StartsWith("Fall")||Performance=="Slide_Tackle"?1:0;
   pole=Vector3.Slerp(pole,(transform.right*sign*.8f+transform.forward*.2f-Vector3.up*.15f).normalized,brace);
   // Rotate the large hand toward its supporting surface before lowering it.
   var finger=arm.toe?arm.toe.position-arm.end.position:arm.end.rotation*Vector3.down*.14f;
   var palm=transform.rotation*arm.palmRest;
   wrist=Quaternion.Slerp(wrist,palm,brace);
   if(ground||review){
    float down=arm.toe?(Quaternion.Inverse(arm.end.rotation)*finger).magnitude:.14f;
    if(Physics.Raycast(target+Vector3.up*.5f,Vector3.down,out var floor,1.2f,1<<8,QueryTriggerInteraction.Ignore))target.y=Mathf.Max(target.y,floor.point.y+.045f+down*(1-brace));
   }
   Solve(arm,target,pole,wrist);
   arm.end.rotation=Quaternion.RotateTowards(arm.lower.rotation*arm.wristRest,wrist,50);
  }
  void FloorFoot(int side,ref Vector3 target,Quaternion rotation){
   if(!Physics.Raycast(target+Vector3.up*.5f,Vector3.down,out var floor,1,1<<8,QueryTriggerInteraction.Ignore))return;
   var delta=rotation*Quaternion.Inverse(transform.rotation*footRotation[side]);
   // Conservative sole corners, calibrated to this shoe's rest-plane height.
   float min=0;
   for(int x=-1;x<=1;x+=2)for(int z=0;z<2;z++)min=Mathf.Min(min,(delta*new Vector3(x*.055f,-feet[side].y+.012f,z==0?-.055f:.19f)).y);
   // The heel is partly weighted to the shin. Deep crouches deform it below
   // the rigid sole plane; this measured clearance also covers that skin.
   var leg=legs[side];float folded=Vector3.Distance(leg.upper.position,target)/(leg.upperLength+leg.lowerLength);
   float heel=.030f*FootballGait.Ease((.85f-folded)/.45f);
   target.y=Mathf.Max(target.y,floor.point.y-min+heel);
  }
  void Rotate(Transform bone,Vector3 angles){bone.rotation=transform.rotation*Quaternion.Euler(angles)*Quaternion.Inverse(transform.rotation)*bone.rotation;}
  Vector3 ContactAnkle(int side,Vector3 center,Quaternion rotation){
   float radius=FootballBall.Instance?FootballBall.Instance.WorldRadius:.11f;
   var normal=transform.TransformDirection(new Vector3(side==0?-.55f:.55f,-.45f,-.704f).normalized);
   return center+normal*radius-rotation*legs[side].toeOffset;
  }
  void Solve(Limb l,Vector3 target,Vector3 pole,Quaternion endRotation){
   if(Physics.Linecast(l.upper.position,target,out var wall,1<<8,QueryTriggerInteraction.Ignore)&&Mathf.Abs(wall.normal.y)<.5f)target=wall.point+wall.normal*.025f;
   Vector3 a=l.upper.position,b=l.lower.position,c=l.end.position,delta=target-a;float reach=delta.magnitude;if(reach<.001f)return;
   float upper=l.upperLength,lower=l.lowerLength;
   // Keep a small knee/elbow bend; the fully straight singularity can flip
   // the bend plane even though the previous solver preserved bone lengths.
   float length=Mathf.Clamp(reach,Mathf.Abs(upper-lower)+.012f,upper+lower-.009f);
   Vector3 axis=delta/reach,bend=Vector3.ProjectOnPlane(pole,axis).normalized;
   if(bend.sqrMagnitude<.01f)bend=Vector3.ProjectOnPlane(b-a,axis).normalized;
   if(bend.sqrMagnitude<.01f)bend=Vector3.ProjectOnPlane(transform.right,axis).normalized;
   float along=(upper*upper-lower*lower+length*length)/(2*length);var joint=a+axis*along+bend*Mathf.Sqrt(Mathf.Max(0,upper*upper-along*along));
   // Reconstruct a stable hinge frame. Shortest-arc FromToRotation alone
   // loses twist near a 180-degree swing and can flip an otherwise valid knee.
   var normal=Vector3.Cross(bend,axis).normalized;
   l.upper.rotation=Quaternion.LookRotation(joint-a,normal)*l.upperBasis;
   l.lower.rotation=Quaternion.LookRotation(a+axis*length-joint,normal)*l.lowerBasis;l.end.rotation=endRotation;
   MaximumReachError=Mathf.Max(MaximumReachError,Mathf.Max(0,reach-length));
  }
  public void ResetPose(){Restore();State=new(){sequence=State.sequence+1};effortBlend=weight=phase=previousSpeed=brake=startAccent=turn=bank=0;receiveAt=settleAt=-10;wallFreedom=1;heldBefore=haveLast=feetInitialized=false;gait.Reset();lastMode=-1;actionAge=10;lastAction=previousAction=FootballAction.None;smoothBody=smoothPelvis=bodyVelocity=pelvisVelocity=Vector3.zero;yawVelocity=0;previousPosition=transform.position;smoothedYaw=previousYaw=transform.eulerAngles.y;lastMatchPhase=FootballMatchPhase.Idle;}
  public void Receive(FootballMotionState value){if(received&&unchecked((int)(value.sequence-receivedSequence))<0)return;received=true;receivedSequence=value.sequence;State=value;}
  public void Charging(bool active){
   if(active==State.charging)return;var s=State;s.sequence++;s.charging=active;
   if(active){s.chargeStarted=Clock;s.left=gait.Swing(0)>gait.Swing(1);s.gesture=FootballGesture.None;}
   else if(State.gesture!=FootballGesture.Kick||Clock-State.started>.35){s.gesture=FootballGesture.Cancel;s.started=Clock;}
   State=s;
  }
  public void Kick(float power,Vector3 ball){State=new(){sequence=State.sequence+1,gesture=FootballGesture.Kick,started=Clock,left=State.charging?State.left:gait.Swing(0)>gait.Swing(1),power=Mathf.Clamp01(power),contact=transform.InverseTransformPoint(ball)};}
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
 }
}
