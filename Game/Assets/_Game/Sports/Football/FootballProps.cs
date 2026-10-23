using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public sealed partial class FootballBall {
  FootballPropsState footballProps;SportsProp previousProp;static uint slotRevision,transferRevision;
  readonly Dictionary<Athlete,SportsPropSlot> footballSlots=new();readonly List<Athlete> retiredSlots=new();readonly System.Random abilityRandom=new();
  Athlete portalReceiver;Vector3 abilitySweepStart;double abilitySweepAt;bool abilitySweepReady;float propPickupDelay;
  Transform wallPhysicsRoot;readonly BoxCollider[] wallColliders=new BoxCollider[2];PhysicsMaterial wallMaterial;
  // Shared world collision, independent of rendering and of which team placed it.
  // Sync immediately on placement/reset and before movement so an expired wall
  // cannot block an athlete for one extra simulation step.
  public void SyncFootballWalls(){
   bool changed=false;
   for(int i=0;i<2;i++){
    var wall=i==0?Props.wallA:Props.wallB;bool active=WallActive(wall);var collider=wallColliders[i];
    if(active&&!collider){
     if(!wallPhysicsRoot){wallPhysicsRoot=new GameObject("Football wall collision").transform;wallPhysicsRoot.SetParent(Pitch,true);wallMaterial=new PhysicsMaterial("Football wall"){dynamicFriction=0,staticFriction=0,bounciness=.72f,frictionCombine=PhysicsMaterialCombine.Minimum,bounceCombine=PhysicsMaterialCombine.Maximum};}
     var go=new GameObject("Solid football wall "+i){layer=8};go.transform.SetParent(wallPhysicsRoot,false);collider=go.AddComponent<BoxCollider>();collider.enabled=false;collider.size=FootballPropRules.WallHalfExtents*2;collider.contactOffset=.002f;collider.sharedMaterial=wallMaterial;wallColliders[i]=collider;
    }
    if(!collider)continue;
    if(active&&(collider.transform.position!=wall.Centre||collider.transform.rotation!=wall.Rotation)){collider.transform.SetPositionAndRotation(wall.Centre,wall.Rotation);changed=true;}
    if(collider.enabled!=active){collider.enabled=active;changed=true;}
   }
   if(changed)Physics.SyncTransforms();
  }
  void DisableFootballWalls(){foreach(var c in wallColliders)if(c)c.enabled=false;}
  void DestroyFootballWalls(){if(wallPhysicsRoot)Destroy(wallPhysicsRoot.gameObject);if(wallMaterial)Destroy(wallMaterial);}
  public FootballPropsState Props=>HasAuthority?footballProps:NetworkAthlete.HostPlayer?NetworkAthlete.HostPlayer.Ball.Value.props:default;
  public static double PropClock=>FootballMatch.Instance?FootballMatch.Instance.Now:Time.fixedTimeAsDouble;
  // Solo Explore has no selected match teams. Give its local player Team A's
  // practice half without changing the match roster or multiplayer lobby rules.
  public bool OfflinePropPractice=>isActiveAndEnabled&&!Online&&FootballMatch.Instance&&FootballMatch.Instance.Context&&FootballMatch.Instance.Snapshot.phase==FootballMatchPhase.Idle;
  public bool PropsLive=>isActiveAndEnabled&&FootballMatch.Instance&&FootballMatch.Instance.Context&&(FootballMatch.Instance.Running||OfflinePropPractice);
  public bool PropsVisible=>isActiveAndEnabled&&FootballMatch.Instance&&FootballMatch.Instance.Context&&(OfflinePropPractice||FootballMatch.Instance.Active&&Props.live);
  public bool PortalBusy=>Props.transferring;
  public uint PropBallRevision=>Props.portalSequence;
  public FootballTeam PropTeam(Athlete actor)=>OfflinePropPractice&&actor&&AppRoot.Instance&&actor==AppRoot.Instance.LocalAthlete?FootballTeam.A:FootballMatch.Instance?FootballMatch.Instance.TeamOf(actor):FootballTeam.None;
  ulong PropPlayerId(Athlete actor)=>Online?SportsAbilityRules.PlayerId(actor):FootballBots.Id(actor);
  public Athlete PropActor(ulong id){foreach(var a in Athlete.Active)if(PropPlayerId(a)==id)return a;return null;}
  public bool PropLocked(Athlete actor)=>actor&&PortalBusy&&(HasAuthority?portalReceiver==actor:SportsAbilityRules.PlayerId(actor)==Props.receiver);
  bool PropEligible(Athlete actor)=>PropsLive&&actor&&!actor.IsSpectator&&actor.isActiveAndEnabled&&!actor.inTransit&&actor.capsule&&PropTeam(actor)!=FootballTeam.None;
  public SportsPropSlot PropSlot(Athlete actor){if(!actor)return default;var n=actor.GetComponent<NetworkAthlete>();if(!HasAuthority&&n&&n.IsSpawned)return n.FootballProp.Value;return footballSlots.TryGetValue(actor,out var s)?s:default;}
  void GiveFootballProp(Athlete actor,SportsProp kind){if(!actor)return;var slot=new SportsPropSlot{kind=kind,revision=++slotRevision};footballSlots[actor]=slot;var n=actor.GetComponent<NetworkAthlete>();if(n&&n.IsSpawned&&n.IsServer)n.FootballProp.Value=slot;}
  public float TurboRemaining(Athlete actor)=>PropEligible(actor)?Mathf.Max(0,(float)((PropTeam(actor)==FootballTeam.B?Props.turboB:Props.turboA)-PropClock)):0;
  public FootballWallState TeamWall(FootballTeam team)=>team==FootballTeam.B?Props.wallB:Props.wallA;
  public bool WallActive(FootballWallState wall)=>PropsLive&&wall.active&&PropClock<wall.until;
  public Vector3 PropAnchor(int side){var p=PitchBounds.center;p.x=side==0?PitchBounds.min.x+.75f/Mathf.Abs(Pitch.lossyScale.x):PitchBounds.max.x-.75f/Mathf.Abs(Pitch.lossyScale.x);p.y=PitchBounds.max.y+.025f;return Pitch.TransformPoint(p);}
  public bool InsidePitch(Athlete actor){if(!actor||!Pitch)return false;var p=Pitch.InverseTransformPoint(actor.transform.position);return SportsAbilityRules.Finite(p)&&p.x>PitchBounds.min.x+.3f&&p.x<PitchBounds.max.x-.3f&&p.z>PitchBounds.min.z+.3f&&p.z<PitchBounds.max.z-.3f&&Mathf.Abs(p.y-PitchBounds.max.y)<.4f;}
  public bool InOwnHalf(Athlete actor)=>InsidePitch(actor)&&FootballPropRules.OwnHalf(Pitch.InverseTransformPoint(actor.transform.position).z,PitchBounds.center.z,PropTeam(actor));
  public string PropUnavailable(Athlete actor){
   if(!PropsLive&&FootballMatch.Instance&&FootballMatch.Instance.Context&&FootballMatch.Instance.Active&&PropSlot(actor).kind!=SportsProp.None)return "Available after kickoff";
   if(!PropEligible(actor))return "Start a match or Explore Offline";var kind=PropSlot(actor).kind;if(!FootballPropRules.Kind(kind))return "Find a touchline prop";
   if(!InsidePitch(actor))return "Return inside the pitch";
   if(actor.Airborne||actor.LoadingJump||!actor.Grounded)return "Land first";
   if(actor.Action!=FootballAction.None||actor.WhiffRemaining>0||actor.Charging||PropLocked(actor))return "Finish your action";
   if(kind==SportsProp.Turbo&&TurboRemaining(actor)>0)return "Team Turbo already active";
   if(kind==SportsProp.Wall&&WallActive(TeamWall(PropTeam(actor))))return "Your team already has a wall";
   if(kind==SportsProp.Portal){if(PortalBusy)return "Ball in transit";if(CurrentController==actor)return "You already have the ball";if(!InOwnHalf(actor))return "Return to your own half";if(!PortalLandingClear(actor))return "Clear space at your feet";}
   return "";
  }
  bool PortalLandingClear(Athlete actor){var p=FootPosition(actor);var local=Pitch.InverseTransformPoint(p);float r=WorldRadius;
   if(local.x<PitchBounds.min.x+r||local.x>PitchBounds.max.x-r||local.z<PitchBounds.min.z+r||local.z>PitchBounds.max.z-r)return false;
   if(WallActive(Props.wallA)&&FootballPropRules.Contains(Props.wallA,p,r+.03f)||WallActive(Props.wallB)&&FootballPropRules.Contains(Props.wallB,p,r+.03f))return false;
   foreach(var other in Athlete.Active)if(other&&other!=actor&&!other.inTransit&&other.capsule&&other.capsule.enabled&&other.capsule.bounds.SqrDistance(p)<(r+.02f)*(r+.02f))return false;
   return AbilityReachClear(actor,p)&&!Physics.CheckSphere(p,r*.85f,1<<8,QueryTriggerInteraction.Ignore);
  }
  // Ball interactions also need an explicit line-of-reach check. This prevents a portal
  // from delivering to the far side of a wall through the receiver's feet.
  public bool AbilityReachClear(Athlete actor,Vector3 point){
   if(!actor)return false;var from=actor.transform.position+Vector3.up*.35f;
   var a=Props.wallA;var b=Props.wallB;
   return !(WallActive(a)&&FootballPropRules.Sweep(a,from,point,WorldRadius,out _,out _))&&!(WallActive(b)&&FootballPropRules.Sweep(b,from,point,WorldRadius,out _,out _));
  }
  public bool TryWallPlacement(Athlete actor,float heading,out FootballWallState wall,out string reason){
   wall=default;reason=PropUnavailable(actor);if(reason!=""||PropSlot(actor).kind!=SportsProp.Wall)return false;
   if(!float.IsFinite(heading)){reason="Invalid direction";return false;}
   var rotation=Quaternion.Euler(0,heading,0);var point=actor.transform.position+rotation*Vector3.forward*FootballPropRules.PlacementDistance;
   if(!Physics.Raycast(point+Vector3.up*1.5f,Vector3.down,out var ground,3,1<<8,QueryTriggerInteraction.Ignore)||ground.normal.y<.97f){reason="Place on level pitch";return false;}
   wall=new FootballWallState{active=true,position=ground.point,heading=heading%360,until=PropClock+FootballPropRules.WallSeconds};
   var e=FootballPropRules.WallHalfExtents;
   for(int i=0;i<4;i++){
    var corner=point+rotation*new Vector3((i&1)==0?-e.x:e.x,0,(i&2)==0?-e.z:e.z);var local=Pitch.InverseTransformPoint(corner);
    if(local.x<PitchBounds.min.x+.1f||local.x>PitchBounds.max.x-.1f||local.z<PitchBounds.min.z+.1f||local.z>PitchBounds.max.z-.1f){reason="Keep the whole wall in bounds";return false;}
    for(int g=0;g<GoalCount;g++)if(Mathf.Abs(local.z-goals[g].front)<5.5f&&local.x>goals[g].bounds.min.x-3&&local.x<goals[g].bounds.max.x+3){reason="Keep clear of goal areas";return false;}
   }
   if(WallActive(Props.wallA)&&FootballPropRules.Overlap(wall,Props.wallA)||WallActive(Props.wallB)&&FootballPropRules.Overlap(wall,Props.wallB)){reason="Another wall is in the way";return false;}
   foreach(var other in Athlete.Active)if(other&&other.isActiveAndEnabled&&!other.inTransit&&other.capsule&&other.capsule.enabled){var bounds=other.capsule.bounds;float radius=Mathf.Max(bounds.extents.x,bounds.extents.z);var centre=bounds.center;float segment=Mathf.Max(0,bounds.extents.y-radius);centre.y=Mathf.Clamp(wall.Centre.y,centre.y-segment,centre.y+segment);if(FootballPropRules.Contains(wall,centre,radius+.1f)){reason="Keep clear of players";return false;}}
   if(FootballPropRules.Contains(wall,Body.position,WorldRadius+.15f)){reason="Keep clear of the ball";return false;}
   if(PortalBusy&&(FootballPropRules.Contains(wall,Props.source,WorldRadius+.15f)||FootballPropRules.Contains(wall,Props.destination,WorldRadius+.15f))){reason="Keep clear of the portals";return false;}
   // Ground is deliberately outside this box; players and active barriers are
   // also checked explicitly to give a useful placement rejection.
   if(Physics.CheckBox(wall.Centre+Vector3.up*.06f,new Vector3(e.x,e.y-.06f,e.z),rotation,1<<8,QueryTriggerInteraction.Ignore)){reason="An obstacle is in the way";return false;}
   reason="";return true;
  }
  public bool RequestProp(Athlete actor,float heading){if(!TutorialSession.Allows("ability"))return false;
   if(!actor)return false;var n=actor.GetComponent<NetworkAthlete>();var slot=PropSlot(actor);uint round=FootballMatch.Instance?FootballMatch.Instance.Snapshot.revision:0;
   if(n&&n.IsSpawned){if(!n.IsOwner)return false;n.UseFootballPropRpc(slot.revision,round,heading);return true;}
   return TryUseProp(actor,slot.revision,round,heading);
  }
  public bool TryUseProp(Athlete actor,uint revision,uint round,float heading){
   if(!HasAuthority||!FootballMatch.Instance||round!=FootballMatch.Instance.Snapshot.revision||PropSlot(actor).revision!=revision||!float.IsFinite(heading))return false;
   // Resolve physical contacts and an already crossed goal before reserving a ball.
   FootballMatch.Instance.ResolvePendingGoals();if(!PropsLive||PropUnavailable(actor)!="")return false;
   var kind=PropSlot(actor).kind;
   if(kind==SportsProp.Turbo){if(PropTeam(actor)==FootballTeam.B)footballProps.turboB=PropClock+SportsAbilityRules.TurboSeconds;else footballProps.turboA=PropClock+SportsAbilityRules.TurboSeconds;}
   else if(kind==SportsProp.Wall){if(!TryWallPlacement(actor,heading,out var wall,out _))return false;if(PropTeam(actor)==FootballTeam.B)footballProps.wallB=wall;else footballProps.wallA=wall;}
   else if(kind==SportsProp.Portal){
    var source=controller?FootPosition(controller):Body.position;ReleaseControl();portalReceiver=actor;actor.Motor.Reset(actor.transform.eulerAngles.y);actor.FootballEffort.Stop();
    footballProps.transferring=true;footballProps.receiver=PropPlayerId(actor);footballProps.portalAt=PropClock;footballProps.portalSequence=++transferRevision;footballProps.portalOutcome=0;footballProps.source=source;footballProps.destination=FootPosition(actor);
    SetPhysicsMode(false);Body.detectCollisions=false;Body.position=source;transform.position=source;abilitySweepReady=false;sequence++;FootballMatch.Instance.ResetAbilityGoalTracking();
   }else return false;
   FootballAudio.Emit(kind==SportsProp.Turbo?FootballSound.TurboStart:kind==SportsProp.Wall?FootballSound.WallPlace:FootballSound.PortalOpen,kind==SportsProp.Portal?footballProps.source:actor.transform.position);SyncFootballWalls();GiveFootballProp(actor,SportsProp.None);publishTimer=0;return true;
  }
  public void ResetFootballProps(bool clearInventory=true){
   if(!HasAuthority)return;
   if(clearInventory){foreach(var pair in footballSlots)if(pair.Key){var n=pair.Key.GetComponent<NetworkAthlete>();if(n&&n.IsSpawned&&n.IsServer)n.FootballProp.Value=new SportsPropSlot{revision=++slotRevision};}footballSlots.Clear();}
   if(footballProps.transferring){portalReceiver=null;SetPhysicsMode(Allowed);Body.detectCollisions=true;sequence++;}
   var pickup=footballProps.pickup;bool live=footballProps.live;
   footballProps=new FootballPropsState{portalSequence=++transferRevision};
   if(!clearInventory){footballProps.pickup=pickup;footballProps.live=live;}
   if(clearInventory)previousProp=SportsProp.None;
   portalReceiver=null;abilitySweepReady=false;publishTimer=0;SyncFootballWalls();
  }
  void ScheduleFootballPickup(){footballProps.pickup=SportsAbilityRules.SchedulePickup(PropClock,abilityRandom.Next(2));publishTimer=0;}
  void StepFootballProps(){
   if(!HasAuthority)return;
   if(!PropsLive){if(footballProps.live){if(PropsVisible)StepFootballPickup();else ResetFootballProps();}return;}
   if(!footballProps.live){footballProps.live=true;ScheduleFootballPickup();}
   retiredSlots.Clear();foreach(var pair in footballSlots)if(!PropEligible(pair.Key))retiredSlots.Add(pair.Key);foreach(var a in retiredSlots){if(a)GiveFootballProp(a,SportsProp.None);footballSlots.Remove(a);}
   if(footballProps.wallA.active&&PropClock>=footballProps.wallA.until){FootballAudio.Emit(FootballSound.WallExpire,footballProps.wallA.Centre,.8f);footballProps.wallA.active=false;publishTimer=0;}if(footballProps.wallB.active&&PropClock>=footballProps.wallB.until){FootballAudio.Emit(FootballSound.WallExpire,footballProps.wallB.Centre,.8f);footballProps.wallB.active=false;publishTimer=0;}
   StepFootballPickup();if(!PortalBusy)return;
   if(!PropEligible(portalReceiver)||portalReceiver.Action!=FootballAction.None||!portalReceiver.Grounded||!InsidePitch(portalReceiver)||!PortalLandingClear(portalReceiver)){CancelFootballPortal();return;}
   float phase=(float)(PropClock-footballProps.portalAt)/SportsAbilityRules.PortalSeconds;
   footballProps.destination=FootPosition(portalReceiver);Body.position=phase<.5f?footballProps.source:footballProps.destination;transform.position=Body.position;
   if(phase<1)return;
   controller=portalReceiver;portalReceiver=null;footballProps.transferring=false;footballProps.portalOutcome=1;mustApproach.Remove(controller);Physics.IgnoreCollision(sphere,controller.capsule,true);
   SetPhysicsMode(false);Body.detectCollisions=true;Body.interpolation=RigidbodyInterpolation.None;sequence++;FollowController(controller);FootballAudio.Emit(FootballSound.Receive,Body.position,.8f);abilitySweepReady=false;FootballMatch.Instance.ResetAbilityGoalTracking();publishTimer=0;
  }
  void CancelFootballPortal(){
   FootballAudio.Emit(FootballSound.Interrupt,footballProps.source,.8f);
   var drop=footballProps.source;var local=Pitch.InverseTransformPoint(drop);local.x=Mathf.Clamp(local.x,PitchBounds.min.x+WorldRadius,PitchBounds.max.x-WorldRadius);local.z=Mathf.Clamp(local.z,PitchBounds.min.z+WorldRadius,PitchBounds.max.z-WorldRadius);local.y=Mathf.Max(local.y,PitchBounds.max.y+WorldRadius+.02f);
   portalReceiver=null;footballProps.transferring=false;footballProps.portalOutcome=2;SetPhysicsMode(Allowed);Body.detectCollisions=true;Body.position=Pitch.TransformPoint(local);transform.position=Body.position;if(!Body.isKinematic){Body.linearVelocity=Vector3.zero;Body.angularVelocity=Vector3.zero;}sequence++;abilitySweepReady=false;propPickupDelay=Time.time+.3f;FootballMatch.Instance?.ResetAbilityGoalTracking();publishTimer=0;
  }
  void StepFootballPickup(){
   ref var spawn=ref footballProps.pickup;
   if(spawn.kind==SportsProp.None){if(SportsAbilityRules.WarnPickup(ref spawn,PropClock))publishTimer=0;if(PropClock<spawn.respawn)return;spawn.kind=FootballPropRules.NextKind(previousProp,abilityRandom.Next(previousProp==SportsProp.None?3:2));previousProp=spawn.kind;spawn.countingDown=false;FootballAudio.Emit(FootballSound.Spawn,PropAnchor(spawn.anchor),.9f);publishTimer=0;}
   var point=PropAnchor(spawn.anchor);Athlete winner=null;float best=SportsAbilityRules.PickupRadius*SportsAbilityRules.PickupRadius;
   foreach(var actor in Athlete.Active){if(!PropEligible(actor)||!actor.Grounded||actor.Airborne||actor.LoadingJump||PropLocked(actor)||actor.Action!=FootballAction.None)continue;
    var delta=actor.transform.position-point;float distance=Vector3.ProjectOnPlane(delta,Vector3.up).sqrMagnitude;
    if(Mathf.Abs(delta.y)>.5f||distance>best+.00001f||Physics.Linecast(actor.transform.position+Vector3.up*.5f,point+Vector3.up*.5f,1<<8,QueryTriggerInteraction.Ignore))continue;
    if(winner&&Mathf.Abs(distance-best)<.00001f&&PropPlayerId(actor)>=PropPlayerId(winner))continue;winner=actor;best=distance;
   }
   if(winner){FootballAudio.Emit(FootballSound.Pickup,winner.transform.position,.9f);GiveFootballProp(winner,spawn.kind);ScheduleFootballPickup();}
  }
  public bool SweepAbilityWall(Vector3 from,Vector3 to,out Vector3 point,out Vector3 normal,double stepStarted=double.NaN){
   point=to;normal=Vector3.zero;if(!PropsLive||PortalBusy)return false;float best=2;Vector3 contact=to,hitNormal=Vector3.zero;Check(Props.wallA);Check(Props.wallB);point=contact;normal=hitNormal;return best<=1;
   void Check(FootballWallState wall){if(!wall.active||!FootballPropRules.Sweep(wall,from,to,WorldRadius,out var t,out var n)||t>=best)return;double hitAt=double.IsNaN(stepStarted)?PropClock:stepStarted+(PropClock-stepStarted)*t;if(hitAt>=wall.until)return;best=t;contact=Vector3.Lerp(from,to,t)+n*.008f;hitNormal=n;}
  }
  void WallRebound(Vector3 point,Vector3 normal,Vector3 incoming){
   SoundContact(FootballSound.WallHit,point,Mathf.Abs(Vector3.Dot(incoming,normal)),.10f);
   Body.position=point;transform.position=point;float toward=Vector3.Dot(incoming,normal);if(toward<0)incoming-=normal*(toward*1.72f);Body.linearVelocity=incoming;Body.angularVelocity*=.7f;Body.WakeUp();impulsePending=true;sequence++;publishTimer=0;abilitySweepStart=point;
  }
  // Called before goal sampling as well as before/after simulation. This orders
  // a swept wall contact ahead of a goal when multiple fixed steps share a frame.
  public void ResolveAbilityBallStep(){
   if(!HasAuthority||!Body||PortalBusy||controller||Body.isKinematic){abilitySweepReady=false;return;}
   if(abilitySweepReady&&SweepAbilityWall(abilitySweepStart,Body.position,out var point,out var normal,abilitySweepAt))WallRebound(point,normal,Body.linearVelocity);
   abilitySweepStart=Body.position;abilitySweepAt=PropClock;abilitySweepReady=true;
  }
  bool BlockAttachedBall(Athlete actor,Vector3 destination){
   if(!HasAuthority||!SweepAbilityWall(Body.position,destination,out var point,out var normal))return false;
   var incoming=(destination-Body.position)/Mathf.Max(.005f,Time.fixedDeltaTime);ReleaseControl(actor);WallRebound(point,normal,Vector3.ClampMagnitude(incoming,5));return true;
  }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public void ReviewGiveFootballProp(Athlete actor,SportsProp kind)=>GiveFootballProp(actor,kind);
  public void ReviewAdvanceFootballSpawn(double seconds){footballProps.pickup.respawn-=seconds;publishTimer=0;}
  public BoxCollider ReviewWallCollider(FootballTeam team)=>wallColliders[team==FootballTeam.B?1:0];
  public void ReviewExpireFootballEffects(){footballProps.turboA=footballProps.turboB=0;footballProps.wallA.until=footballProps.wallB.until=PropClock-.01;publishTimer=0;SyncFootballWalls();}
  public void ReviewFootballWall(FootballTeam team,Vector3 position,float heading=0,float seconds=15){var wall=new FootballWallState{active=true,position=position,heading=heading,until=PropClock+seconds};if(team==FootballTeam.B)footballProps.wallB=wall;else footballProps.wallA=wall;publishTimer=0;SyncFootballWalls();}
  public void ReviewFootballBall(Vector3 position,Vector3 velocity){ReleaseControl();SetPhysicsMode(true);Body.position=position;transform.position=position;Body.linearVelocity=velocity;Body.angularVelocity=Vector3.zero;Body.WakeUp();abilitySweepStart=position;abilitySweepAt=PropClock;abilitySweepReady=true;sequence++;publishTimer=0;}
#endif
 }
}
