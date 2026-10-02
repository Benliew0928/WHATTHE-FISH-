using System.Collections.Generic;
using UnityEngine;

namespace WhatTheFish {
 public sealed class Athlete : MonoBehaviour {
  public static readonly HashSet<Athlete> Active=new();
  void OnEnable(){Active.Add(this);}
  void OnDisable(){Active.Remove(this);FootballBall.Instance?.ForgetPlayer(this);}
  public CharacterController capsule; public Transform visual; public bool controlled,inTransit; public float speed;
  Animator animator; SkinnedMeshRenderer[] bodyRenderers;float turnWeight;int turnLayer=-1;FootballTackle football;int footballLayer=-1;float footballWeight;FootballSnapshot receivedFootball,cachedFootball;uint footballSequence=uint.MaxValue;double footballClock,footballReceivedAt;Vector3 visualRest;float groundSlideWeight;
  public JumpMotor Jump {get;}=new JumpMotor();
  public BasketballMotion BasketballMotion {get;private set;}
  JumpSnapshot receivedJump;int jumpLayer=-1;float jumpWeight;
  public bool LoadingJump=>remote?receivedJump.preparing:Jump.Preparing;
  public bool Airborne=>remote?receivedJump.airborne:Jump.Airborne;
  public bool CanRequestJump=>!inTransit&&Action==FootballAction.None&&AppRoot.Instance&&AppRoot.Instance.Exploring;
  public void RequestJump(){if(!initialized)Setup();if(CanRequestJump)Jump.Request();}
  public JumpSnapshot JumpState()=>new JumpSnapshot{airborne=Jump.Airborne,preparing=Jump.Preparing,load=Jump.LoadElapsed,velocity=Jump.Velocity,landing=Jump.Landing,sequence=Jump.Sequence};
  public void ApplyJump(JumpSnapshot value){receivedJump=value;}
  public LocomotionMotor Motor {get;}=new LocomotionMotor();
  public LocomotionPhase Phase=>remote?received.phase:Motor.Phase;
  public float PresentedTurnProgress {get;private set;}
  bool initialized,remote;LocomotionSnapshot received;Vector3 lastPosition;
  uint sentSequence=uint.MaxValue;double phaseStarted;
  public FootballAction Action=>remote?receivedFootball.action:football?football.State:FootballAction.None;
  public float ActionProgress=>remote?Mathf.Clamp01((float)((footballClock+Time.timeAsDouble-footballReceivedAt-receivedFootball.started)/FootballTackle.Duration(Action))):football?Mathf.Clamp01(football.Elapsed/FootballTackle.Duration(Action)):0;
  public float TackleCooldown=>remote?Mathf.Max(0,(float)(receivedFootball.cooldownUntil-footballClock-Time.timeAsDouble+footballReceivedAt)):football?football.CooldownRemaining:0;
  public bool TackleReady=>!inTransit&&FootballTackle.Allowed&&Action==FootballAction.None&&TackleCooldown<=0&&!Airborne&&!LoadingJump&&Grounded;
  public bool Grounded=>capsule&&Physics.Raycast(transform.position+Vector3.up*.1f,Vector3.down,.25f,1<<8,QueryTriggerInteraction.Ignore);
  public bool TryTackle(){if(inTransit||Airborne||LoadingJump)return false;if(!initialized)Setup();return football&&football.TryStart(Grounded);}
  float nextKick;
  public bool Charging {get;private set;}
  public bool KickReady=>!Airborne&&!LoadingJump&&Time.time>=nextKick&&FootballBall.Instance&&FootballBall.Instance.InKickRange(this);
  public bool TryKick(float charge=1){if(!KickReady||!FootballBall.Instance.TryKick(this,charge))return false;nextKick=Time.time+.35f;return true;}
  public FootballSnapshot FootballState(double now){
   if(footballSequence!=football.Sequence){footballSequence=football.Sequence;cachedFootball=new FootballSnapshot{action=football.State,sequence=football.Sequence,started=now-football.Elapsed,cooldownUntil=now+football.CooldownRemaining};}
   return cachedFootball;
  }
  public void ApplyFootball(FootballSnapshot value,double serverTime){receivedFootball=value;footballClock=serverTime;footballReceivedAt=Time.timeAsDouble;}
  public void Setup(){if(!BasketballMotion)BasketballMotion=GetComponent<BasketballMotion>()??gameObject.AddComponent<BasketballMotion>();BasketballMotion.Bind(this);capsule=GetComponent<CharacterController>();animator=GetComponentInChildren<Animator>();football=GetComponent<FootballTackle>();jumpLayer=animator?animator.GetLayerIndex("Jump"):-1;footballLayer=animator?animator.GetLayerIndex("Football action"):-1;turnLayer=animator?animator.GetLayerIndex("Turn expression"):-1;bodyRenderers=visual?visual.GetComponentsInChildren<SkinnedMeshRenderer>(true):GetComponentsInChildren<SkinnedMeshRenderer>(true);if(!initialized){Jump.Reset();Motor.Reset(transform.eulerAngles.y);lastPosition=transform.position;visualRest=visual?visual.localPosition:Vector3.zero;initialized=true;}}
  public void ResetLocomotion(){if(!initialized)Setup();Motor.Reset(transform.eulerAngles.y);speed=0;if(BasketballMotion)BasketballMotion.ResetPose();Jump.Reset();receivedJump=default;jumpWeight=0;if(animator&&jumpLayer>=0)animator.SetLayerWeight(jumpLayer,0);if(football)football.ResetAction();footballWeight=0;groundSlideWeight=0;if(visual)visual.localPosition=visualRest;if(animator&&footballLayer>=0)animator.SetLayerWeight(footballLayer,0);lastPosition=transform.position;turnWeight=0;if(animator&&turnLayer>=0)animator.SetLayerWeight(turnLayer,0);}
  public LocomotionSnapshot Snapshot(double now){
   // Keep the timestamp stable within a phase, including idle, so unchanged
   // athletes do not resend the entire snapshot on every network tick.
   if(sentSequence!=Motor.Sequence){sentSequence=Motor.Sequence;phaseStarted=now-Motor.Elapsed;}
   return new LocomotionSnapshot{phase=Motor.Phase,speed=speed,angle=Motor.TurnAngle,startYaw=Motor.TurnStartYaw,duration=Motor.Duration,started=phaseStarted,sequence=Motor.Sequence};
  }
  public void ApplySnapshot(LocomotionSnapshot value){remote=true;received=value;speed=value.speed;}
  public void Simulate(PlayerCommand command,float dt){
   if(!initialized)Setup();if(!capsule.enabled||inTransit||dt<=0)return;remote=false;
   if(command.jump)RequestJump();
   if(command.shoot&&BasketballBall.Active)BasketballBall.Active.TryShoot(this,command.heading);
   else if(command.pass&&BasketballBall.Active)BasketballBall.Active.TryPass(this,command.heading);
   // Bounded sweeps keep low frame rates and short hitches from skipping ceilings.
   int steps=Mathf.Max(1,Mathf.CeilToInt(Mathf.Min(dt,.25f)*60));
   float step=Mathf.Min(dt,.25f)/steps;
   for(int i=0;i<steps;i++){SimulateStep(command,step);command.tackle=false;command.kick=false;}
  }
  void SimulateStep(PlayerCommand command,float dt){
   if(!initialized)Setup(); if(!capsule.enabled)return;remote=false;
   if(Vector3.Distance(transform.position,lastPosition)>2)ResetLocomotion();
   var move=Vector2.ClampMagnitude(command.move,1);Vector3 direction=Quaternion.Euler(0,command.heading,0)*new Vector3(move.x,0,move.y);
   bool grounded=!Jump.Airborne&&Grounded;
   // Gather on balanced feet, then immediately return control during recovery.
   // Flight retains the shared jump motor's steering and momentum.
   if(grounded&&!Jump.Preparing&&BasketballMotion&&BasketballMotion.Busy&&BasketballMotion.Elapsed<WhatTheFish.BasketballMotion.ReleaseTime(BasketballMotion.Action)+.08f)direction*=1-WhatTheFish.BasketballMotion.Ease(BasketballMotion.Elapsed/.12f);
   if(command.tackle)TryTackle();
   if(command.kick)TryKick(command.kickCharge);
   float normalTime=dt;bool slideContact=false;
   var displacement=football?football.Step(dt,out normalTime,out slideContact):Vector3.zero;
   Charging=command.charging&&KickReady;
   float requestedSpeed=FootballBall.Instance&&FootballBall.Allowed?FootballBall.Instance.MovementSpeed(this,command.sprint,Charging):(command.sprint?7:4);
   float vertical=Jump.Step(grounded,CanRequestJump,dt);
   // Preserve the running impulse even if the stick is released during the
   // grounded jump preparation; air control also starts on the takeoff step.
   if(normalTime>0)displacement+=Motor.Step(direction,requestedSpeed,grounded&&!Jump.Preparing&&!Jump.Airborne,normalTime);
   if(BasketballMotion&&BasketballMotion.Busy)Motor.FaceBasketball(BasketballMotion.State.heading,dt);
   capsule.stepOffset=Jump.Airborne?0:.3f;
   var before=transform.position;
   if(FootballBall.Instance)displacement=FootballBall.Instance.ConstrainPlayerMotion(this,displacement,dt);
   var flags=capsule.Move(displacement+Vector3.up*vertical);
   Jump.Collide(flags);
   if(slideContact)football.ResolveContacts(before,transform.position);
   var actual=transform.position-before;actual.y=0;speed=dt>0?actual.magnitude/dt:0;
   transform.rotation=Quaternion.Euler(0,Motor.Yaw,0);
   var island=RefinedIslandEnvironment.Active;
   float fallLimit=island?island.layout.sea_level-2:(CoastalVenueRoutes.Active?-2.0f:-10f);
   if(transform.position.y<fallLimit){capsule.enabled=false;transform.position=island?island.layout.safe_return:(CoastalVenueRoutes.Active?CoastalVenueRoutes.Active.safeReturn:new Vector3(0,1,0));capsule.enabled=true;ResetLocomotion();}
   lastPosition=transform.position;
  }
  void OnControllerColliderHit(ControllerColliderHit hit){var ball=hit.collider.GetComponent<FootballBall>();if(ball&&!ball.Intercept(this))ball.Push(this,hit.moveDirection);Motor.Constrain(hit.normal);}
  void Update(){
   if(!animator)return;
   if(inTransit){if(jumpLayer>=0)animator.SetLayerWeight(jumpLayer,0);jumpWeight=0;animator.SetFloat("Speed",0);animator.SetBool("Turning",false);animator.SetBool("Launching",false);if(turnLayer>=0)animator.SetLayerWeight(turnLayer,0);if(footballLayer>=0)animator.SetLayerWeight(footballLayer,0);return;}
   bool turning=!Airborne&&Phase==LocomotionPhase.Turning;
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
   if(jumpLayer>=0){
    float pose=remote?JumpMotor.Pose(receivedJump.preparing,receivedJump.load,receivedJump.airborne,receivedJump.velocity,receivedJump.landing):Jump.PoseTime;
    bool presenting=remote?receivedJump.preparing||receivedJump.airborne||receivedJump.landing<JumpMotor.LandingDuration:Jump.Presenting;
    jumpWeight=Mathf.MoveTowards(jumpWeight,presenting&&Action==FootballAction.None?1:0,Time.deltaTime*18);
    animator.SetFloat("JumpTime",pose);animator.SetLayerWeight(jumpLayer,jumpWeight);
   }
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
