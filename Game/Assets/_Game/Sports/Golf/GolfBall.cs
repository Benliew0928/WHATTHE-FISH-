using UnityEngine;

namespace WhatTheFish {
 public enum GolfBallMotion { Flying,FastRolling,SlowRolling,Resting }
 [RequireComponent(typeof(Rigidbody),typeof(SphereCollider))]
 [DefaultExecutionOrder(50)]
 public sealed class GolfBall:MonoBehaviour {
  public const float ModelRadius=.0215f,VisualScale=3f,Radius=ModelRadius*VisualScale;
  public ulong Owner {get;private set;}
  public Rigidbody Body {get;private set;}
  public Vector3 LastShotPosition {get;private set;}
  public Vector3 LastDryPosition {get;private set;}
  public uint WaterRecoverySequence {get;private set;}
  bool hasDryPosition;
  public uint ResetSequence {get;private set;}
  public uint StrikeSequence {get;private set;}
  public GolfBallTrail Trail {get;private set;}
  public bool Live {get;private set;}
  public GolfBallMotion Motion {get;private set;}=GolfBallMotion.Flying;
  public bool IsGrounded {get;private set;}
  public bool ReadyToAim=>Live&&shot==null&&(match.Authority?Body.linearVelocity.sqrMagnitude<.04f:!received||Vector3.Distance(before.position,target.position)<.015f);
  public bool FollowingShot=>shot!=null;
  public Vector3 Velocity=>shot!=null?shotVelocity:Body.linearVelocity;
  public Vector3 LastGuideContact {get;private set;}
  public uint GuideContactSequence {get;private set;}
  GolfShotPlan shot;float shotTime;int shotIndex;Vector3 shotVelocity;
  readonly RaycastHit[] movingHits=new RaycastHit[16];
  public float StopTimer {get;private set;}
  public GolfBallPhysicsSettings PhysicsSettings {get;private set;}
  float feedbackLandingAt=-100;
  float freeUntil,settlingCeiling;Vector3 expectedVelocity,preStepVelocity;bool hasExpectedVelocity;
  GolfMatchManager match;SphereCollider shape;Vector3 previous;bool bound,received;
  GolfBallSnapshot before,target;float receivedAt;
  void Awake(){
   PhysicsSettings=Resources.Load<GolfBallPhysicsSettings>("GolfBallPhysics");
   if(!PhysicsSettings)PhysicsSettings=ScriptableObject.CreateInstance<GolfBallPhysicsSettings>();
   Body=GetComponent<Rigidbody>();shape=GetComponent<SphereCollider>();shape.radius=Radius;shape.contactOffset=.001f;
   Body.mass=.0459f;Body.maxAngularVelocity=2000;Body.linearDamping=.05f;Body.angularDamping=.1f;Body.useGravity=true;
   Body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;Body.interpolation=RigidbodyInterpolation.Interpolate;Body.isKinematic=true;
   // Character shoreline walls stay in place; only golf balls can leave the course.
   shape.excludeLayers=(1<<9)|(1<<GolfCartMotor.Layer);
   Trail=gameObject.AddComponent<GolfBallTrail>();
  }
  internal void Bind(GolfMatchManager manager,ulong owner){
   if(bound)return;bound=true;match=manager;Owner=owner;
   IgnorePlayers();
  }
  void IgnorePlayers(){foreach(var actor in Athlete.Active)if(actor&&actor.capsule&&actor.capsule.enabled)Physics.IgnoreCollision(shape,actor.capsule);}
  internal void SetLive(bool live){Live=live;bool dynamic=live&&match.Authority;if(dynamic&&shot==null){Body.isKinematic=false;Body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;}else if(!dynamic){ResetMotion();Body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;Body.isKinematic=true;}Body.detectCollisions=dynamic;}
  internal void Place(Vector3 position,bool newAnchor){
   if(!match.Authority)return;
   ResetMotion();Body.isKinematic=false;Body.linearVelocity=Body.angularVelocity=Vector3.zero;
   var interpolation=Body.interpolation;Body.interpolation=RigidbodyInterpolation.None;
   Body.position=position;Body.rotation=Quaternion.identity;transform.SetPositionAndRotation(position,Quaternion.identity);Body.interpolation=interpolation;
   previous=position;feedbackLandingAt=Time.time;if(newAnchor){LastShotPosition=position;LastDryPosition=position;hasDryPosition=true;}ResetSequence++;SetLive(Live);Trail.Clear();
  }
  void ResetMotion(){
   if(shot!=null){shot=null;Body.isKinematic=false;Body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;Body.linearVelocity=shotVelocity;Body.angularVelocity=Vector3.Cross(Vector3.up,shotVelocity)/Radius;}
   Motion=GolfBallMotion.Flying;StopTimer=0;IsGrounded=false;hasExpectedVelocity=false;freeUntil=Time.fixedTime+PhysicsSettings.newForceGracePeriod;Body.useGravity=true;
  }
  internal void Strike(Vector3 velocity){ResetMotion();LastShotPosition=Body.position;previous=Body.position;Body.isKinematic=false;Body.linearVelocity=velocity;Body.angularVelocity=Vector3.Cross(Vector3.up,velocity)/Radius;Body.WakeUp();Trail.Begin(++StrikeSequence);}
  internal void Strike(GolfShotPlan plan){
   ResetMotion();LastShotPosition=Body.position;previous=Body.position;shot=plan;shotTime=0;shotIndex=0;shotVelocity=plan.LaunchVelocity;
   Body.linearVelocity=Body.angularVelocity=Vector3.zero;Body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;Body.isKinematic=true;Body.useGravity=false;
   Trail.Begin(++StrikeSequence);
  }
  void FollowShot(){
   shotTime=Mathf.Min(shotTime+Time.fixedDeltaTime,shot.Duration);shot.Sample(shotTime,ref shotIndex,out var next,out var velocity);
   if(CheckWater(Body.position,next))return;
   var delta=next-Body.position;
   // Moving balls are intentionally not baked into the guide. Hand them back
   // to PhysX before impact so opponents can still knock one another's balls.
   if(delta.sqrMagnitude>1e-9f){
    int count=Physics.SphereCastNonAlloc(Body.position,Radius,delta.normalized,movingHits,delta.magnitude+.015f,1,QueryTriggerInteraction.Ignore);
    for(int i=0;i<count;i++)if(movingHits[i].rigidbody&&movingHits[i].rigidbody!=Body&&movingHits[i].rigidbody.GetComponent<GolfBall>()){ResetMotion();return;}
   }
   shotVelocity=velocity;IsGrounded=shot.Mode==GolfShotMode.Putt&&Mathf.Abs(velocity.y)<.5f;
   Motion=IsGrounded?(velocity.magnitude<PhysicsSettings.slowRollingThreshold?GolfBallMotion.SlowRolling:GolfBallMotion.FastRolling):GolfBallMotion.Flying;
   var rotation=Quaternion.AngleAxis(delta.magnitude/Radius*Mathf.Rad2Deg,Vector3.Cross(Vector3.up,delta).normalized)*Body.rotation;
   if(shotTime<shot.Duration){Body.MovePosition(next);Body.MoveRotation(rotation);return;}
   bool rest=shot.Resting;var normal=shot.Normal;bool contact=shot.HasTarget;
   if(contact&&!rest)FeedbackLanding(next-normal*Radius,normal,Mathf.Abs(Vector3.Dot(shot.Velocities[Mathf.Max(0,shot.Count-2)],normal)),shot.ContactCollider);
   // Resolve the contact at the exact swept point, then let the normal turf
   // rolling code handle a swing's run-out. The target is touchdown, not run-out.
   ResetMotion();Body.position=next;Body.rotation=rotation;Body.linearVelocity=velocity;Body.angularVelocity=Vector3.Cross(normal,velocity)/Radius;
   if(contact){LastGuideContact=next;GuideContactSequence++;}
   if(rest){Motion=GolfBallMotion.Resting;IsGrounded=true;StopTimer=PhysicsSettings.stopDelay;Body.linearVelocity=Body.angularVelocity=Vector3.zero;Body.useGravity=false;Body.Sleep();}
  }
  // Gameplay integrations should use this entry point so even a small impulse
  // clears a nearly completed stop timer before physics evaluates it.
  public void ApplyGameplayImpulse(Vector3 impulse){if(!bound||!match.Authority||!Live||!float.IsFinite(impulse.sqrMagnitude))return;ResetMotion();Body.WakeUp();Body.AddForce(impulse,ForceMode.Impulse);}
  void OnCollisionEnter(Collision collision){
   RecordDryContact(collision);
   AudioCollision(collision);
   if(bound&&match.Authority&&Live&&!collision.rigidbody&&((1<<collision.gameObject.layer)&((1<<8)|(1<<GolfTee.Layer)))!=0){for(int i=0;i<collision.contactCount;i++){var c=collision.GetContact(i);if(c.normal.y>.35f){FeedbackLanding(c.point,c.normal,Mathf.Max(0,-Vector3.Dot(preStepVelocity,c.normal)));break;}}}
   if(bound&&match.Authority&&Live&&shot==null&&Motion==GolfBallMotion.Flying&&!collision.rigidbody){
    for(int i=0;i<collision.contactCount;i++){
     var normal=collision.GetContact(i).normal;
     if(normal.y<=.35f||((1<<collision.gameObject.layer)&((1<<8)|(1<<GolfTee.Layer)))==0)continue;
     // Use velocity from before PhysX removes the incoming normal component.
     // No material restitution is added: this is the single bounce response.
     if(-Vector3.Dot(preStepVelocity,normal)>PhysicsSettings.minimumBounceSpeed){
      var rebound=PhysicsSettings.LandingVelocity(preStepVelocity,normal,false);
      ResetMotion();Body.linearVelocity=rebound;Body.angularVelocity=Vector3.Cross(normal,Vector3.ProjectOnPlane(rebound,normal))/Radius;Body.WakeUp();
     }
     break;
    }
   }
   NewContactForce(collision,true);
  }
  void AudioCollision(Collision collision){
   if(!bound||!match.Authority||!Live||collision.contactCount==0)return;
   var other=collision.rigidbody?collision.rigidbody.GetComponent<GolfBall>():null;
   if(other&&Owner>other.Owner)return; // One resolved ball pair, not two sounds.
   var c=collision.GetContact(0);string name=collision.collider.name.ToLowerInvariant();
   bool wood=name.Contains("wood")||name.Contains("timber")||name.Contains("trunk")||name.Contains("tree")||name.Contains("palm")||name.Contains("bridge")||name.Contains("plank");
   bool stone=name.Contains("stone")||name.Contains("rock")||name.Contains("path")||name.Contains("paving")||name.Contains("wall")||c.normal.y<=.35f;
   if(!other&&!wood&&!stone)return;
   float speed=collision.relativeVelocity.magnitude;if(speed<.45f||Time.time-feedbackLandingAt<.12f)return;feedbackLandingAt=Time.time;
   GolfFeedback.Emit(other?GolfFeedbackCue.Ball:wood?GolfFeedbackCue.Wood:GolfFeedbackCue.Stone,c.point,c.normal,Mathf.InverseLerp(.45f,10,speed),Owner,Owner);
  }
  void FeedbackLanding(Vector3 point,Vector3 normal,float speed,Collider support=null){
   string surface=support?support.name.ToLowerInvariant():"";
   bool wood=surface.Contains("wood")||surface.Contains("timber")||surface.Contains("trunk")||surface.Contains("tree")||surface.Contains("palm")||surface.Contains("bridge")||surface.Contains("plank");
   bool hard=surface.Contains("stone")||surface.Contains("rock")||surface.Contains("path")||surface.Contains("paving")||surface.Contains("wall")||support&&normal.y<=.35f;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-golfShotAudit")>=0)Debug.Log($"GOLF_AUDIO_CONTACT surface={surface} normal={normal} speed={speed:F3} cooldown={Time.time-feedbackLandingAt:F3} wood={wood} hard={hard}");
#endif
   var feedback=GolfFeedback.Instance;if(!feedback||!bound||!match.Authority||!Live||StrikeSequence==0||normal.y<=.35f&&!wood&&!hard||speed<feedback.Settings.minimumLandingSpeed||Time.time-feedbackLandingAt<feedback.Settings.landingCooldown)return;
   feedbackLandingAt=Time.time;
   bool sand=match.Course.RollingResistance(point,PhysicsSettings)>Mathf.Lerp(PhysicsSettings.fairwayResistance,PhysicsSettings.sandResistance,.5f);
   GolfFeedback.Emit(wood?GolfFeedbackCue.Wood:hard?GolfFeedbackCue.Stone:sand?GolfFeedbackCue.Sand:GolfFeedbackCue.Turf,point,normal,Mathf.InverseLerp(feedback.Settings.minimumLandingSpeed,10,speed),Owner,Owner);
  }
  void OnCollisionStay(Collision collision){RecordDryContact(collision);NewContactForce(collision,false);}
  void RecordDryContact(Collision collision){
   if(!bound||!match.Authority||!Live||collision.rigidbody||!(collision.collider.name.StartsWith("Terrain__")||collision.collider.name=="Pond bank"||collision.collider.gameObject.layer==GolfTee.Layer))return;
   for(int i=0;i<collision.contactCount;i++){var c=collision.GetContact(i);var anchor=c.point+c.normal*(Radius+.002f);if(c.normal.y>.65f&&match.Course.SafeDryPosition(anchor)){LastDryPosition=anchor;hasDryPosition=true;break;}}
  }
  void NewContactForce(Collision collision,bool newContact){
   if(!bound||!match.Authority||!Live||!collision.rigidbody)return;
   // Static terrain support is not a new hit. Dynamic balls and moving
   // gameplay bodies retain ordinary collision response, including at rest.
   if(newContact||collision.impulse.magnitude/Body.mass>=PhysicsSettings.externalVelocityChange){ResetMotion();Body.WakeUp();}
  }
  void TrackDryGround(Vector3 position){
   if(!match.Course.SafeDryPosition(position))return;
   if(!Physics.Raycast(position+Vector3.up*.01f,Vector3.down,out var hit,Radius+.04f,(1<<8)|(1<<GolfTee.Layer),QueryTriggerInteraction.Ignore)||hit.normal.y<.65f)return;
   if(hit.rigidbody||!(hit.collider.name.StartsWith("Terrain__")||hit.collider.name=="Pond bank"||hit.collider.gameObject.layer==GolfTee.Layer))return;
   if(Vector3.Dot(position-hit.point,hit.normal)>Radius+.02f)return;
   LastDryPosition=hit.point+hit.normal*(Radius+.002f);hasDryPosition=true;
  }
  bool CheckWater(Vector3 from,Vector3 to){
   if(!match.Course.TouchesWater(from,to,Radius))return false;
   GolfFeedback.Emit(GolfFeedbackCue.Water,to,Vector3.up,.7f,Owner,Owner);
   var anchor=hasDryPosition?LastDryPosition:LastShotPosition;
   Place(anchor,false);
   // A water reset finishes at a supported dry point. Discard the old flight
   // velocity so the next dry contact cannot reuse it as a landing rebound.
   preStepVelocity=expectedVelocity=shotVelocity=Vector3.zero;
   Motion=GolfBallMotion.Resting;IsGrounded=true;StopTimer=PhysicsSettings.stopDelay;Body.useGravity=false;Body.Sleep();
   WaterRecoverySequence++;return true;
  }
  public void Recover(){if(match&&match.Authority&&Live){GolfFeedback.Emit(GolfFeedbackCue.Recovery,Body.position,Vector3.up,.6f,Owner,Owner);Place(LastShotPosition,false);}}
  void FixedUpdate(){
   if(!bound||!match.Authority||!Live||!match.Context||!match.State.Running)return;
   // Reapply for late arrivals and controllers re-enabled after a cart ride.
   // Players and carts stay excluded; other golf balls can still collide.
   IgnorePlayers();
   var position=Body.position;
   if(match.OutsideCourse(position)){Recover();return;}
   if(CheckWater(previous,position))return;
   TrackDryGround(position);
   foreach(var hole in match.HoleTriggers)if(hole.Contains(position)||hole.Crossed(previous,position)){match.EnterHole(this,hole.Hole.number);previous=Body.position;return;}
   previous=position;
   if(shot!=null)FollowShot();else GroundMotion(position);
   preStepVelocity=Body.linearVelocity;
  }
  void GroundMotion(Vector3 position){
   var settings=PhysicsSettings;float dt=Time.fixedDeltaTime;
   // Opening supports stay outside the character/cart ground-query layer.
   IsGrounded=Physics.Raycast(position+Vector3.up*.01f,Vector3.down,out var hit,Radius*2+.03f,(1<<8)|(1<<GolfTee.Layer),QueryTriggerInteraction.Ignore)
    &&hit.normal.y>.35f&&Vector3.Dot(position-hit.point,hit.normal)<=Radius+.012f
    &&Vector3.Dot(Body.linearVelocity,hit.normal)<=.15f&&Vector3.Dot(Body.linearVelocity,hit.normal)>-settings.minimumBounceSpeed;
   var velocity=Body.linearVelocity;
   if(Motion==GolfBallMotion.Resting){
    if(!IsGrounded||velocity.sqrMagnitude>1e-6f){ResetMotion();Body.WakeUp();}
    else return; // No gravity, but a dynamic body: collisions can wake it.
   }
   if(!IsGrounded){Motion=GolfBallMotion.Flying;StopTimer=0;Body.useGravity=true;hasExpectedVelocity=false;return;}
   var normal=hit.normal;var tangent=Vector3.ProjectOnPlane(velocity,normal);
   if(Motion==GolfBallMotion.SlowRolling&&hasExpectedVelocity&&Vector3.ProjectOnPlane(velocity-expectedVelocity,normal).magnitude>settings.externalVelocityChange){ResetMotion();Body.WakeUp();}
   if(Time.fixedTime<freeUntil){StopTimer=0;return;}
   float speed=velocity.magnitude;
   if(Motion!=GolfBallMotion.SlowRolling){
    Motion=speed<settings.slowRollingThreshold?GolfBallMotion.SlowRolling:GolfBallMotion.FastRolling;
    if(Motion==GolfBallMotion.SlowRolling)settlingCeiling=tangent.magnitude;
   }
   float surfaceResistance=match.Course.RollingResistance(hit.point,settings);
   if(Motion==GolfBallMotion.FastRolling){
    StopTimer=0;
    // Turf resistance acts on every supported slope. Airborne shots are untouched.
    tangent=Vector3.MoveTowards(tangent,Vector3.zero,surfaceResistance*dt);
    Body.linearVelocity=Vector3.Project(velocity,normal)+tangent;
    // Do not leave stored rolling spin to accelerate the ball again at contact.
    Body.angularVelocity=Vector3.Cross(normal,tangent)/Radius+Vector3.Project(Body.angularVelocity,normal);
    return;
   }
   float progress=1-Mathf.Clamp01(tangent.magnitude/settings.slowRollingThreshold);
   float resistance=surfaceResistance+settings.rollingResistanceStrength*progress;
   // Cancel only downhill gravity during settling; leave the normal component
   // for contact support. Never pull an airborne ball toward the ground.
   settlingCeiling=Mathf.Max(settings.settlingMaxSpeed,settlingCeiling-resistance*dt);
   // Entering at nearly 2 m/s must not snap to 1 m/s. The ceiling decreases
   // continuously to that band. Gravity cancellation and resistance keep
   // draining motion below it; fresh gameplay impulses reset this ceiling.
   tangent=Vector3.ClampMagnitude(Vector3.MoveTowards(tangent,Vector3.zero,resistance*dt),settlingCeiling);
   Body.linearVelocity=Vector3.Project(velocity,normal)+tangent;
   Body.angularVelocity=Vector3.Cross(normal,tangent)/Radius+Vector3.MoveTowards(Vector3.Project(Body.angularVelocity,normal),Vector3.zero,resistance/Radius*dt);
   expectedVelocity=Body.linearVelocity;hasExpectedVelocity=true;
   StopTimer=Body.linearVelocity.magnitude<settings.stopSpeedThreshold?StopTimer+dt:0;
   if(StopTimer>=settings.stopDelay){Motion=GolfBallMotion.Resting;Body.linearVelocity=Body.angularVelocity=Vector3.zero;Body.useGravity=false;Body.Sleep();hasExpectedVelocity=false;}
   // Compensate in velocity, not a queued AddForce: the match manager may
   // Strike later in this very FixedUpdate. Its new launch velocity must
   // replace every part of settling, with no residual force in the solver.
   else Body.linearVelocity-=Vector3.ProjectOnPlane(Physics.gravity,normal)*dt;
  }
  internal GolfBallSnapshot Snapshot(uint round,double now)=>new GolfBallSnapshot{valid=true,active=Live,round=round,reset=ResetSequence,strike=StrikeSequence,position=match.transform.InverseTransformPoint(Body.position),rotation=Quaternion.Inverse(match.transform.rotation)*Body.rotation,time=now};
  internal void RestoreRoom(GolfBallSnapshot s){ResetMotion();ResetSequence=s.reset;StrikeSequence=s.strike;Body.position=match.transform.TransformPoint(s.position);Body.rotation=match.transform.rotation*s.rotation;transform.SetPositionAndRotation(Body.position,Body.rotation);}
  internal void Receive(GolfBallSnapshot snapshot){
   if(match.Authority||!snapshot.valid||snapshot.round!=match.Round)return;
   bool snap=!received||snapshot.reset!=target.reset||snapshot.round!=target.round;
   bool hit=received&&!snap&&snapshot.strike!=target.strike;
   if(!received||snapshot.time!=target.time){before=received?target:snapshot;target=snapshot;receivedAt=Time.unscaledTime;received=true;}
   ResetSequence=snapshot.reset;StrikeSequence=snapshot.strike;
   SetLive(snapshot.active);if(snap){before=target;Present(1);}
   if(snap||!snapshot.active)Trail.Clear();else if(hit)Trail.Begin(snapshot.strike);
  }
  void Update(){if(!received||!match||match.Authority)return;Present(Mathf.Clamp01((Time.unscaledTime-receivedAt)/Mathf.Clamp((float)(target.time-before.time),.02f,.2f)));}
  void Present(float alpha){var p=match.transform.TransformPoint(Vector3.Lerp(before.position,target.position,alpha));var q=match.transform.rotation*Quaternion.Slerp(before.rotation,target.rotation,alpha);Body.position=p;Body.rotation=q;transform.SetPositionAndRotation(p,q);}
 }
}
