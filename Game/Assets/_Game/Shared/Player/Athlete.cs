using UnityEngine;

namespace WhatTheFish {
 public sealed class Athlete : MonoBehaviour {
  public CharacterController capsule; public Transform visual; public bool controlled; public float speed;
  float gravity; Animator animator; SkinnedMeshRenderer[] bodyRenderers;float turnWeight;int turnLayer=-1;FootballTackle football;int footballLayer=-1;float footballWeight;FootballSnapshot receivedFootball,cachedFootball;uint footballSequence=uint.MaxValue;double footballClock,footballReceivedAt;Vector3 visualRest;float groundSlideWeight;
  public LocomotionMotor Motor {get;}=new LocomotionMotor();
  public LocomotionPhase Phase=>remote?received.phase:Motor.Phase;
  public float PresentedTurnProgress {get;private set;}
  bool initialized,remote;LocomotionSnapshot received;Vector3 lastPosition;
  uint sentSequence=uint.MaxValue;double phaseStarted;
  public FootballAction Action=>remote?receivedFootball.action:football?football.State:FootballAction.None;
  public float ActionProgress=>remote?Mathf.Clamp01((float)((footballClock+Time.timeAsDouble-footballReceivedAt-receivedFootball.started)/FootballTackle.Duration(Action))):football?Mathf.Clamp01(football.Elapsed/FootballTackle.Duration(Action)):0;
  public float TackleCooldown=>remote?Mathf.Max(0,(float)(receivedFootball.cooldownUntil-footballClock-Time.timeAsDouble+footballReceivedAt)):football?football.CooldownRemaining:0;
  public bool TackleReady=>FootballTackle.Allowed&&Action==FootballAction.None&&TackleCooldown<=0&&Grounded;
  public bool Grounded=>capsule&&Physics.Raycast(transform.position+Vector3.up*.1f,Vector3.down,.25f,1<<8,QueryTriggerInteraction.Ignore);
  public bool TryTackle(){if(!initialized)Setup();return football&&football.TryStart(Grounded);}
  public FootballSnapshot FootballState(double now){
   if(footballSequence!=football.Sequence){footballSequence=football.Sequence;cachedFootball=new FootballSnapshot{action=football.State,sequence=football.Sequence,started=now-football.Elapsed,cooldownUntil=now+football.CooldownRemaining};}
   return cachedFootball;
  }
  public void ApplyFootball(FootballSnapshot value,double serverTime){receivedFootball=value;footballClock=serverTime;footballReceivedAt=Time.timeAsDouble;}
  public void Setup(){capsule=GetComponent<CharacterController>();animator=GetComponentInChildren<Animator>();football=GetComponent<FootballTackle>();footballLayer=animator?animator.GetLayerIndex("Football action"):-1;turnLayer=animator?animator.GetLayerIndex("Turn expression"):-1;bodyRenderers=visual?visual.GetComponentsInChildren<SkinnedMeshRenderer>(true):GetComponentsInChildren<SkinnedMeshRenderer>(true);if(!initialized){Motor.Reset(transform.eulerAngles.y);lastPosition=transform.position;visualRest=visual?visual.localPosition:Vector3.zero;initialized=true;}}
  public void ResetLocomotion(){if(!initialized)Setup();Motor.Reset(transform.eulerAngles.y);speed=0;gravity=0;if(football)football.ResetAction();footballWeight=0;groundSlideWeight=0;if(visual)visual.localPosition=visualRest;if(animator&&footballLayer>=0)animator.SetLayerWeight(footballLayer,0);lastPosition=transform.position;turnWeight=0;if(animator&&turnLayer>=0)animator.SetLayerWeight(turnLayer,0);}
  public LocomotionSnapshot Snapshot(double now){
   // Keep the timestamp stable within a phase, including idle, so unchanged
   // athletes do not resend the entire snapshot on every network tick.
   if(sentSequence!=Motor.Sequence){sentSequence=Motor.Sequence;phaseStarted=now-Motor.Elapsed;}
   return new LocomotionSnapshot{phase=Motor.Phase,speed=speed,angle=Motor.TurnAngle,startYaw=Motor.TurnStartYaw,duration=Motor.Duration,started=phaseStarted,sequence=Motor.Sequence};
  }
  public void ApplySnapshot(LocomotionSnapshot value){remote=true;received=value;speed=value.speed;}
  public void Simulate(PlayerCommand command,float dt){
   if(!initialized)Setup(); if(!capsule.enabled)return;remote=false;
   if(Vector3.Distance(transform.position,lastPosition)>2)ResetLocomotion();
   var move=Vector2.ClampMagnitude(command.move,1);Vector3 direction=Quaternion.Euler(0,command.heading,0)*new Vector3(move.x,0,move.y);
   bool grounded=Grounded;
   if(command.tackle)TryTackle();
   float normalTime=dt;bool slideContact=false;
   var displacement=football?football.Step(dt,out normalTime,out slideContact):Vector3.zero;
   if(normalTime>0)displacement+=Motor.Step(direction,command.sprint?7:4,grounded,normalTime);
   if(capsule.isGrounded)gravity=-2;else gravity=Mathf.Max(-25,gravity-22*dt);
   var before=transform.position;capsule.Move(displacement+Vector3.up*gravity*dt);
   if(slideContact)football.ResolveContacts(before,transform.position);
   var actual=transform.position-before;actual.y=0;speed=dt>0?actual.magnitude/dt:0;
   transform.rotation=Quaternion.Euler(0,Motor.Yaw,0);
   if(transform.position.y<-10){capsule.enabled=false;transform.position=new Vector3(0,1,0);capsule.enabled=true;ResetLocomotion();}
   lastPosition=transform.position;
  }
  void Update(){
   if(!animator)return;
   bool turning=Phase==LocomotionPhase.Turning;
   float angle=remote?received.angle:Motor.TurnAngle;
   float progress=LocomotionMotor.PoseProgress(remote?received.startYaw:Motor.TurnStartYaw,angle,transform.eulerAngles.y);
   PresentedTurnProgress=Mathf.Clamp01(progress);
   animator.SetBool("Turning",turning);animator.SetFloat("TurnAngle",Mathf.Sign(angle)*Mathf.Max(90,Mathf.Abs(angle)));animator.SetFloat("TurnTime",PresentedTurnProgress);
   animator.SetBool("Launching",Phase==LocomotionPhase.Launching||Phase==LocomotionPhase.Moving);
   animator.SetFloat("Speed",speed);animator.SetFloat("RunPlayback",Mathf.Clamp(speed/7f,.25f,1f));
   // The running legs stay active while the saved turn contributes torso/head
   // expression. Neither the animation nor planted-foot IK can gate travel.
   turnWeight=Mathf.MoveTowards(turnWeight,turning&&speed>.2f&&Action==FootballAction.None?.65f:0,Time.deltaTime*8);
   if(turnLayer>=0)animator.SetLayerWeight(turnLayer,turnWeight);
   if(footballLayer>=0){
    bool acting=Action!=FootballAction.None;
    if(acting){animator.SetInteger("FootballAction",(int)Action);animator.SetFloat("FootballTime",ActionProgress);}else animator.SetFloat("FootballTime",1);
    footballWeight=Mathf.MoveTowards(footballWeight,acting?1:0,Time.deltaTime*(acting?25:9));
    animator.SetLayerWeight(footballLayer,footballWeight);
   }
  }
  void LateUpdate(){
   if(!visual)return;
   groundSlideWeight=Mathf.MoveTowards(groundSlideWeight,Action==FootballAction.Slide?1:0,Time.deltaTime*10);
   float clearance=0;
   if(groundSlideWeight>0&&Physics.Raycast(transform.position+Vector3.up*.2f,Vector3.down,out var floor,.45f,1<<8,QueryTriggerInteraction.Ignore))clearance=Mathf.Clamp(transform.position.y-floor.point.y,0,.12f);
   visual.localPosition=visualRest-Vector3.up*(clearance*groundSlideWeight);
  }
  public void Appearance(CharacterAppearance data){
   // Preserve the profile/network call path while this baked Meshy look has no swap slots.
   data.Clamp();
  }
  public void HideHead(bool hidden){if(bodyRenderers==null)Setup();foreach(var renderer in bodyRenderers)renderer.enabled=!hidden;}
 }
}
