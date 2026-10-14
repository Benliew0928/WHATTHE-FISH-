using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public struct BasketballPropsState:INetworkSerializable,IEquatable<BasketballPropsState> {
  public SportsPropSpawn pickup;public double turboA,turboB,aimUntil,portalAt;
  public bool aiming,transferring,guided;public ulong aimer,receiver;public uint portalSequence;
  public Vector3 source,destination;public byte portalOutcome;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {
   s.SerializeValue(ref guided);s.SerializeValue(ref pickup);s.SerializeValue(ref turboA);s.SerializeValue(ref turboB);
   s.SerializeValue(ref aimUntil);s.SerializeValue(ref aiming);s.SerializeValue(ref aimer);s.SerializeValue(ref transferring);
   s.SerializeValue(ref receiver);s.SerializeValue(ref portalAt);s.SerializeValue(ref portalSequence);s.SerializeValue(ref source);s.SerializeValue(ref destination);s.SerializeValue(ref portalOutcome);
  }
  public bool Equals(BasketballPropsState b)=>guided==b.guided&&pickup.Equals(b.pickup)&&turboA==b.turboA&&turboB==b.turboB&&aimUntil==b.aimUntil&&aiming==b.aiming&&aimer==b.aimer&&transferring==b.transferring&&receiver==b.receiver&&portalAt==b.portalAt&&portalSequence==b.portalSequence&&source==b.source&&destination==b.destination&&portalOutcome==b.portalOutcome;
 }
 public static class BasketballPropRules {
  public const float AimSeconds=2,TurboSeconds=SportsAbilityRules.TurboSeconds,TurboMultiplier=SportsAbilityRules.TurboMultiplier,PortalSeconds=SportsAbilityRules.PortalSeconds,RespawnSeconds=SportsAbilityRules.RespawnSeconds,CountdownSeconds=SportsAbilityRules.CountdownSeconds,PickupRadius=SportsAbilityRules.PickupRadius;
  public static string Name(SportsProp kind)=>kind==SportsProp.LockOn?"LOCK-ON SHOT":kind==SportsProp.Turbo?"TEAM TURBO":kind==SportsProp.Portal?"PORTAL STEAL":"FIND A PROP";
  // The two sideline ends of the centre line; no advantage to either half.
  public static Vector3 Anchor(int index)=>new Vector3(index==0?-7f:7f,.08f,0);
  // Directly sample the two alternatives; no reroll loop or repeat fallback.
  public static SportsProp NextKind(SportsProp previous,int choice)=>SportsAbilityRules.NextKind(previous,choice,1);
  public static bool OwnHalf(Vector3 point,FootballTeam team)=>team==FootballTeam.B?point.z>.35f:point.z<-.35f;
  // A deliberately assisted shot with a steep, centred descent. Physics contacts
  // and defender sweeps still interrupt it; scoring uses the real hoop crossing.
  public static Vector3 ShotPoint(Vector3 start,Vector3 end,float t){
   t=Mathf.Clamp01(t);float lateral=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.20f,.72f,t));
   var p=Vector3.Lerp(start,end,lateral);float distance=Vector3.ProjectOnPlane(end-start,Vector3.up).magnitude;
   p.y=Mathf.Lerp(start.y,end.y,t)+4*(5+distance*.10f)*t*(1-t);return p;
  }
 }
 public sealed partial class BasketballBall {
  BasketballPropsState propState;SportsProp lastSpawnedProp;static uint propRevision;
  readonly Dictionary<Athlete,SportsPropSlot> propSlots=new();readonly List<Athlete> expiredPropActors=new();
  readonly System.Random propRandom=new();Athlete propAimer,portalReceiver;Transform propHoop;
  int audioCountdown;bool audioPortalIn,audioPortalOut;bool propShotQueued,guidedPropShot;Vector3 propShotStart,propShotEnd,propShotDetour;double propShotAt;float propShotDuration;
  public BasketballPropsState Props=>Authority?propState:haveTarget?target.props:default;
  public bool PortalBusy=>Props.transferring;
  public Athlete PropActor(ulong id){if(!Connected&&!(BasketballMatch.Instance&&BasketballMatch.Instance.Competitive))return AppRoot.Instance?AppRoot.Instance.LocalAthlete:null;foreach(var a in Athlete.Active)if(PlayerId(a)==id)return a;return null;}
  public FootballTeam PropTeam(Athlete actor){var team=SportsPossession.TeamOf(actor,SportId.Basketball);return team==FootballTeam.None?FootballTeam.A:team;}
  public SportsPropSlot PropSlot(Athlete actor){
   if(!actor)return default;var net=actor.GetComponent<NetworkAthlete>();
   if(!Authority&&net&&net.IsSpawned)return net.BasketballProp.Value;
   return propSlots.TryGetValue(actor,out var slot)?slot:default;
  }
  void GiveProp(Athlete actor,SportsProp kind){
   var slot=new SportsPropSlot{kind=kind,revision=++propRevision};propSlots[actor]=slot;
   var net=actor.GetComponent<NetworkAthlete>();if(net&&net.IsSpawned&&net.IsServer)net.BasketballProp.Value=slot;
  }
  public float TurboRemaining(Athlete actor){if(!Playing||!actor||actor.inTransit||actor.BasketballFreeRoam)return 0;return Mathf.Max(0,(float)((PropTeam(actor)==FootballTeam.B?Props.turboB:Props.turboA)-BasketballMotion.Clock));}
  public bool PropLocked(Athlete actor)=>Playing&&actor&&(Authority?(propAimer==actor||portalReceiver==actor):(Props.aiming&&PropActor(Props.aimer)==actor||Props.transferring&&PropActor(Props.receiver)==actor));
  public bool InsidePaintedCourt(Athlete actor){
   if(!actor)return false;var p=transform.parent.InverseTransformPoint(actor.transform.position);
   return Finite(p)&&Mathf.Abs(p.x)<=PaintedCourtLimits.x-.30f&&Mathf.Abs(p.z)<=PaintedCourtLimits.y-.30f&&p.y>-.15f&&p.y<.5f;
  }
  public bool InOwnHalf(Athlete actor)=>InsidePaintedCourt(actor)&&BasketballPropRules.OwnHalf(transform.parent.InverseTransformPoint(actor.transform.position),PropTeam(actor));
  Transform PropTargetHoop(Athlete actor){
   if(BasketballMatch.Instance&&BasketballMatch.Instance.Competitive)return BasketballMatch.Instance.AttackingHoop(actor);
   // Imported North/South names reflect Blender axes, which are reversed in
   // Unity. Select the enemy end in the same arena frame as the own-half rule.
   float direction=PropTeam(actor)==FootballTeam.B?-1:1,best=0;Transform target=null;
   foreach(var basket in baskets){if(!basket)continue;float end=transform.parent.InverseTransformPoint(basket.transform.position).z*direction;if(end>best){best=end;target=basket.transform;}}
   return target;
  }
  public string PropUnavailable(Athlete actor){
   if(BasketballMatch.BlocksActions||InboundPending)return "WAIT FOR PLAY";
   if(!Playing||!actor||actor.inTransit||actor.BasketballFreeRoam)return "JOIN THE COURT";
   var kind=PropSlot(actor).kind;if(kind==SportsProp.None)return "FIND A COURTSIDE PROP";
   if(IsInbound(actor))return "PASS TO RESTART PLAY";
   if(PropLocked(actor))return "ABILITY IN PROGRESS";
   if(!InsidePaintedCourt(actor))return "RETURN INSIDE THE COURT";
   if(actor.Airborne||actor.LoadingJump||Authority&&!actor.Grounded)return "LAND FIRST";
   if(!actor.BasketballMotion||actor.BasketballMotion.Busy||IsCharging(actor)||IsPassAiming(actor)||ActionQueued&&Holder==actor)return "FINISH YOUR ACTION";
   if(kind==SportsProp.LockOn&&Holder!=actor)return "NEED THE BALL";
   if(kind==SportsProp.Turbo&&TurboRemaining(actor)>0)return "TEAM TURBO ALREADY ACTIVE";
   if(kind==SportsProp.Portal){if(!InOwnHalf(actor))return "RETURN TO YOUR OWN HALF";if(Holder==actor)return "YOU ALREADY HAVE THE BALL";if(PortalBusy)return "BALL IN TRANSIT";if(Authority&&Physics.CheckSphere(actor.transform.TransformPoint(new Vector3(.1f,.97f,.39f)),Radius+.015f,1<<8,QueryTriggerInteraction.Ignore))return "MOVE AWAY FROM OBSTACLES";}
   return "";
  }
  public void RequestProp(Athlete actor){if(!actor)return;var net=actor.GetComponent<NetworkAthlete>();var slot=PropSlot(actor);if(net&&net.IsSpawned)net.UseBasketballPropRpc(slot.revision);else if(!TryUseProp(actor,slot.revision))BasketballAudio.PlayMenu(BasketballSound.UIError);}
  public bool TryUseProp(Athlete actor,uint revision){
   if(!Authority||!Eligible(actor)||PropSlot(actor).revision!=revision||PropUnavailable(actor)!="")return false;
   var kind=PropSlot(actor).kind;
   if(kind==SportsProp.LockOn){
    var hoop=PropTargetHoop(actor);if(!hoop)return false;
    propHoop=hoop;propAimer=actor;propShotQueued=false;
    propState.aiming=true;propState.aimer=PlayerId(actor);propState.aimUntil=BasketballMotion.Clock+BasketballPropRules.AimSeconds;
    chargingAthlete=actor;chargingHoop=propHoop;chargeStarted=chargeSampled=BasketballMotion.Clock;chargePhase=0;chargePeriod=ChargePeriod(4);chargeHistory.Clear();
    actor.BasketballMotion.EndGuard();actor.Motor.Reset(actor.transform.eulerAngles.y);
   }else if(kind==SportsProp.Turbo){
    if(PropTeam(actor)==FootballTeam.B)propState.turboB=BasketballMotion.Clock+BasketballPropRules.TurboSeconds;
    else propState.turboA=BasketballMotion.Clock+BasketballPropRules.TurboSeconds;
   BasketballAudio.Emit(BasketballSound.TurboStart,actor.transform.position);
   }else if(kind==SportsProp.Portal){
    // Resolve an already completed physical crossing before reserving possession.
    StepShotPhysics();if(BasketballMatch.BlocksActions)return false;var source=Held?CarryPosition(holder):Body.position;
    ClearPossession();portalReceiver=actor;actor.Motor.Reset(actor.transform.eulerAngles.y);
    propState.transferring=true;propState.receiver=PlayerId(actor);propState.portalAt=BasketballMotion.Clock;propState.portalSequence++;propState.portalOutcome=0;
    propState.source=source;propState.destination=actor.transform.TransformPoint(new Vector3(0,1.05f,.5f));
    actor.BasketballMotion.Begin(BasketballAction.Charge,actor.transform.eulerAngles.y,Quaternion.Inverse(actor.transform.rotation)*(propState.destination-actor.transform.position));
    Body.isKinematic=true;Body.detectCollisions=false;SetPortalBall(source);reset++;
    audioPortalIn=audioPortalOut=false;BasketballAudio.Emit(BasketballSound.PortalOpen,source,.8f);BasketballAudio.Emit(BasketballSound.PortalOpen,propState.destination,.65f);
   }else return false;
   GiveProp(actor,SportsProp.None);sendAt=0;return true;
  }
  void SetPortalBall(Vector3 point){Body.position=point;transform.position=point;}
  void EndPropAim(){if(propState.aiming&&propAimer)BasketballAudio.Emit(BasketballSound.Interrupt,propAimer.transform.position,.7f);var actor=propAimer;propAimer=null;propHoop=null;propShotQueued=false;propState.aiming=false;if(actor){pendingAction=BasketballAction.None;CancelShotCharge(actor);}sendAt=0;}
  void CancelPropBall(){EndPropAim();guidedPropShot=false;propState.guided=false;if(portalReceiver&&portalReceiver.BasketballMotion)portalReceiver.BasketballMotion.ResetPose();portalReceiver=null;propState.transferring=false;sendAt=0;}
  void ResetProps(bool spawn=true,bool clearInventory=true){
   CancelPropBall();
   if(clearInventory){foreach(var pair in propSlots)if(pair.Key){var net=pair.Key.GetComponent<NetworkAthlete>();if(net&&net.IsSpawned&&net.IsServer)net.BasketballProp.Value=new SportsPropSlot{revision=++propRevision};}propSlots.Clear();}
   var pickup=propState.pickup;propState=default;if(!clearInventory)propState.pickup=pickup;
   if(clearInventory){lastSpawnedProp=SportsProp.None;audioCountdown=0;}audioPortalIn=audioPortalOut=false;
   if(clearInventory&&spawn&&Authority&&Playing&&(!BasketballMatch.Instance||!BasketballMatch.Instance.Competitive))SchedulePickup();
  }
  void SchedulePickup(){
   propState.pickup=SportsAbilityRules.SchedulePickup(BasketballMotion.Clock,propRandom.Next(2));sendAt=0;
  }
  void StepPickup(){
   var match=BasketballMatch.Instance;
   if(match&&match.Competitive&&!match.Active)return;
   if(propState.pickup.respawn==0&&propState.pickup.kind==SportsProp.None){if(match&&match.Competitive&&!match.Running)return;SchedulePickup();}
   ref var spawn=ref propState.pickup;
   if(spawn.kind==SportsProp.None){
    if(SportsAbilityRules.WarnPickup(ref spawn,BasketballMotion.Clock))sendAt=0;
    if(!spawn.countingDown)return;
    int number=spawn.CountdownNumber(BasketballMotion.Clock);if(number!=audioCountdown){audioCountdown=number;BasketballAudio.Emit(BasketballSound.Countdown,transform.parent.TransformPoint(BasketballPropRules.Anchor(spawn.anchor)),.65f);}
    if(BasketballMotion.Clock<spawn.respawn)return;
    spawn.kind=BasketballPropRules.NextKind(lastSpawnedProp,propRandom.Next(lastSpawnedProp==SportsProp.None?3:2));lastSpawnedProp=spawn.kind;spawn.countingDown=false;audioCountdown=0;BasketballAudio.Emit(BasketballSound.Spawn,transform.parent.TransformPoint(BasketballPropRules.Anchor(spawn.anchor)));sendAt=0;
    // Leave a materialization frame before collecting, even if a player waited here.
    return;
   }
   if(BasketballMatch.BlocksActions||InboundPending)return;
   var point=transform.parent.TransformPoint(BasketballPropRules.Anchor(spawn.anchor));Athlete winner=null;float best=BasketballPropRules.PickupRadius*BasketballPropRules.PickupRadius;
   foreach(var actor in Athlete.Active){
    if(!Eligible(actor)||actor.Airborne||actor.LoadingJump||PropLocked(actor)||!actor.Grounded)continue;
    var delta=actor.transform.position-point;float distance=new Vector2(delta.x,delta.z).sqrMagnitude;
    if(Mathf.Abs(delta.y)>.5f||distance>best+.00001f||Physics.Linecast(actor.transform.position+Vector3.up*.5f,point+Vector3.up*.5f,1<<8,QueryTriggerInteraction.Ignore))continue;
    if(winner&&Mathf.Abs(distance-best)<.00001f&&PlayerId(actor)>=PlayerId(winner))continue;
    winner=actor;best=distance;
   }
   if(winner){BasketballAudio.Emit(BasketballSound.Pickup,winner.transform.position);GiveProp(winner,spawn.kind);SchedulePickup();}
  }
  void StepProps(){
   expiredPropActors.Clear();foreach(var pair in propSlots)if(!Eligible(pair.Key))expiredPropActors.Add(pair.Key);
   foreach(var actor in expiredPropActors){if(actor)GiveProp(actor,SportsProp.None);propSlots.Remove(actor);}
   if(propState.aiming){
    if(!propAimer||!Eligible(propAimer)||holder!=propAimer||!InsidePaintedCourt(propAimer)||!propHoop){EndPropAim();}
    else if(!propShotQueued&&BasketballMotion.Clock>=propState.aimUntil-BasketballMotion.ReleaseTime(BasketballAction.Shoot)){
     propShotQueued=true;pendingAction=BasketballAction.Shoot;pendingShotHoop=propHoop;pendingHeading=Quaternion.LookRotation(Vector3.ProjectOnPlane(propHoop.position-propAimer.transform.position,Vector3.up)).eulerAngles.y;pendingPower=SweetSpot;pendingWindow=SweetWindow;chargingAthlete=null;chargingHoop=null;
    }
   }
   if(!propState.transferring)return;
   if(!portalReceiver||!Eligible(portalReceiver)||!InOwnHalf(portalReceiver)){
    var drop=propState.destination;BasketballAudio.Emit(BasketballSound.Interrupt,drop,.7f);BasketballAudio.Emit(BasketballSound.PortalClose,drop);CancelPropBall();propState.portalOutcome=2;Body.isKinematic=false;Body.detectCollisions=true;Body.linearVelocity=Vector3.zero;SetPortalBall(drop);pickupAt=Time.time+.35f;reset++;return;
   }
   float phase=(float)(BasketballMotion.Clock-propState.portalAt)/BasketballPropRules.PortalSeconds;
   propState.destination=CarryPosition(portalReceiver);
   if(phase>=.15f&&!audioPortalIn){audioPortalIn=true;BasketballAudio.Emit(BasketballSound.PortalIn,propState.source);}
   if(phase>=.52f&&!audioPortalOut){audioPortalOut=true;BasketballAudio.Emit(BasketballSound.PortalOut,propState.destination);}
   SetPortalBall(phase<.5f?propState.source:propState.destination);
   if(phase<1)return;
   var receiver=portalReceiver;portalReceiver=null;propState.transferring=false;propState.portalOutcome=1;
   holder=receiver;BasketballMatch.Instance?.Control(receiver);receiver.BasketballMotion.Begin(BasketballAction.Cancel,receiver.transform.eulerAngles.y,Quaternion.Inverse(receiver.transform.rotation)*(propState.destination-receiver.transform.position));dribblePhase=0;pendingAction=BasketballAction.None;Body.isKinematic=true;Body.detectCollisions=false;looseFor=outsideFor=0;NewDefensePlay();Follow(receiver);BasketballAudio.Emit(BasketballSound.Catch,Body.position);BasketballAudio.Emit(BasketballSound.PortalClose,propState.destination);BasketballAudio.Emit(BasketballSound.PortalClose,propState.source,.7f);reset++;sendAt=0;
  }
  bool EasyPropBlock(Athlete defender){
   if(!propState.aiming||!propAimer||SportsPossession.SameTeam(defender,propAimer,SportId.Basketball))return false;
   var delta=propAimer.transform.position-defender.transform.position;delta.y=0;
   if(delta.sqrMagnitude>2.2f*2.2f||delta.sqrMagnitude<.001f||Vector3.Dot(defender.transform.forward,delta.normalized)<.35f)return false;
   var point=CarryPosition(propAimer);if(Physics.Linecast(defender.transform.position+Vector3.up,point,1<<8,QueryTriggerInteraction.Ignore))return false;
   ApplyBlock(defender,point,point-defender.transform.forward*.2f,defender.transform.forward*3);NewDefensePlay();return true;
  }
  void StartPropFlight(Vector3 start,Transform hoop){
   BasketballAudio.Emit(BasketballSound.LockFire,start,.85f);guidedPropShot=true;propState.guided=true;propShotStart=start;propShotEnd=hoop.TransformPoint(new Vector3(0,BasketballHoop.RimHeight-.4f,0));propShotAt=BasketballMotion.Clock;
   propShotDuration=1.4f+Vector3.ProjectOnPlane(propShotEnd-start,Vector3.up).magnitude/11;
   propShotDetour=Vector3.zero;
   // A launch directly behind the backboard needs lateral clearance before it
   // rises through board height. Sweep candidate arcs against actual architecture.
   var routes=new[]{Vector3.zero,hoop.right*2,hoop.right*-2,hoop.right*3,hoop.right*-3,hoop.forward*2,hoop.forward*-2};
   foreach(var route in routes){propShotDetour=route;if(PropRouteClear())break;}
   propAimer=null;propHoop=null;propShotQueued=false;propState.aiming=false;
  }
  Vector3 PropFlightPoint(float t){float bulge=Mathf.Sin(Mathf.PI*Mathf.Clamp01(t/.55f));return BasketballPropRules.ShotPoint(propShotStart,propShotEnd,t)+propShotDetour*bulge;}
  bool PropRouteClear(){var previous=propShotStart;int steps=Mathf.CeilToInt(propShotDuration/Time.fixedDeltaTime);for(int i=1;i<=steps;i++){var next=PropFlightPoint(i/(float)steps);var d=next-previous;if(d.sqrMagnitude>0&&Physics.SphereCast(previous,Radius+.006f,d.normalized,out _,d.magnitude,1<<8,QueryTriggerInteraction.Ignore))return false;previous=next;}return true;}
  void StepPropFlight(){
   propState.guided=guidedPropShot;if(!guidedPropShot)return;if(holder||score.result!=BasketballResult.Flying){guidedPropShot=false;propState.guided=false;return;}
   float t=(float)(BasketballMotion.Clock-propShotAt+Time.fixedDeltaTime)/propShotDuration;
   if(t>1.01f){guidedPropShot=false;propState.guided=false;return;}
   var next=PropFlightPoint(t);
   Body.linearDamping=0;Body.linearVelocity=(next-Body.position)/Time.fixedDeltaTime-Physics.gravity*Time.fixedDeltaTime;
  }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public void ReviewGiveProp(Athlete actor,SportsProp kind){if(Authority&&actor)GiveProp(actor,kind);}
  public void ReviewResetProps(){if(Authority)ResetProps();}
  public void ReviewExpireTurbo(){propState.turboA=propState.turboB=0;}
  public void ReviewSpawnProps(SportsProp kind,int side=0){if(!Authority)return;propState.pickup=new SportsPropSpawn{kind=kind,anchor=(byte)side};lastSpawnedProp=kind;sendAt=0;}
  public void ReviewAdvanceSpawn(double seconds){if(!Authority)return;propState.pickup.respawn-=seconds;sendAt=0;}
#endif
 }
}
