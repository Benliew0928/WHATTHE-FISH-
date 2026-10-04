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
  public uint ResetSequence {get;private set;}
  public bool Live {get;private set;}
  public GolfBallMotion Motion {get;private set;}=GolfBallMotion.Flying;
  public bool IsGrounded {get;private set;}
  public float StopTimer {get;private set;}
  public GolfBallPhysicsSettings PhysicsSettings {get;private set;}
  float freeUntil,settlingCeiling;Vector3 expectedVelocity;bool hasExpectedVelocity;
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
  }
  internal void Bind(GolfMatchManager manager,ulong owner){
   if(bound)return;bound=true;match=manager;Owner=owner;
   IgnorePlayers();
  }
  void IgnorePlayers(){foreach(var actor in Athlete.Active)if(actor&&actor.capsule&&actor.capsule.enabled)Physics.IgnoreCollision(shape,actor.capsule);}
  internal void SetLive(bool live){Live=live;bool dynamic=live&&match.Authority;if(dynamic){Body.isKinematic=false;Body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;}else{Body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;Body.isKinematic=true;ResetMotion();}Body.detectCollisions=dynamic;}
  internal void Place(Vector3 position,bool newAnchor){
   if(!match.Authority)return;
   ResetMotion();Body.isKinematic=false;Body.linearVelocity=Body.angularVelocity=Vector3.zero;
   var interpolation=Body.interpolation;Body.interpolation=RigidbodyInterpolation.None;
   Body.position=position;Body.rotation=Quaternion.identity;transform.SetPositionAndRotation(position,Quaternion.identity);Body.interpolation=interpolation;
   previous=position;if(newAnchor)LastShotPosition=position;ResetSequence++;SetLive(Live);
  }
  void ResetMotion(){Motion=GolfBallMotion.Flying;StopTimer=0;IsGrounded=false;hasExpectedVelocity=false;freeUntil=Time.fixedTime+PhysicsSettings.newForceGracePeriod;Body.useGravity=true;}
  internal void Strike(Vector3 velocity){ResetMotion();LastShotPosition=Body.position;previous=Body.position;Body.isKinematic=false;Body.linearVelocity=velocity;Body.angularVelocity=Vector3.Cross(Vector3.up,velocity)/Radius;Body.WakeUp();}
  // Gameplay integrations should use this entry point so even a small impulse
  // clears a nearly completed stop timer before physics evaluates it.
  public void ApplyGameplayImpulse(Vector3 impulse){if(!bound||!match.Authority||!Live||!float.IsFinite(impulse.sqrMagnitude))return;ResetMotion();Body.WakeUp();Body.AddForce(impulse,ForceMode.Impulse);}
  void OnCollisionEnter(Collision collision){NewContactForce(collision,true);}
  void OnCollisionStay(Collision collision){NewContactForce(collision,false);}
  void NewContactForce(Collision collision,bool newContact){
   if(!bound||!match.Authority||!Live||!collision.rigidbody)return;
   // Static terrain support is not a new hit. Dynamic balls and moving
   // gameplay bodies retain ordinary collision response, including at rest.
   if(newContact||collision.impulse.magnitude/Body.mass>=PhysicsSettings.externalVelocityChange){ResetMotion();Body.WakeUp();}
  }
  public void Recover(){if(match&&match.Authority&&Live)Place(LastShotPosition,false);}
  void FixedUpdate(){
   if(!bound||!match.Authority||!Live||!match.Context||!match.State.Running)return;
   // Reapply for late arrivals and controllers re-enabled after a cart ride.
   // Players and carts stay excluded; other golf balls can still collide.
   IgnorePlayers();
   var position=Body.position;
   if(match.OutsideCourse(position)){Recover();return;}
   foreach(var hole in match.HoleTriggers)if(hole.Contains(position)||hole.Crossed(previous,position)){match.EnterHole(this,hole.Hole.number);previous=Body.position;return;}
   previous=position;
   GroundMotion(position);
  }
  void GroundMotion(Vector3 position){
   var settings=PhysicsSettings;float dt=Time.fixedDeltaTime;
   // Opening supports stay outside the character/cart ground-query layer.
   IsGrounded=Physics.Raycast(position+Vector3.up*.01f,Vector3.down,out var hit,Radius*2+.03f,(1<<8)|(1<<GolfTee.Layer),QueryTriggerInteraction.Ignore)
    &&hit.normal.y>.35f&&Vector3.Dot(position-hit.point,hit.normal)<=Radius+.012f
    &&Vector3.Dot(Body.linearVelocity,hit.normal)<=.15f&&Vector3.Dot(Body.linearVelocity,hit.normal)>-1f;
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
   if(Motion==GolfBallMotion.FastRolling){
    StopTimer=0;
    // Preserve the existing level-ground roll; high-speed slopes and flight
    // keep their ordinary damping, gravity and collision response.
    if(normal.y>=Mathf.Cos(settings.levelGroundDegrees*Mathf.Deg2Rad))Body.linearVelocity=Vector3.Project(velocity,normal)+Vector3.MoveTowards(tangent,Vector3.zero,settings.levelRollingResistance*dt);
    return;
   }
   float progress=1-Mathf.Clamp01(tangent.magnitude/settings.slowRollingThreshold);
   float resistance=settings.rollingResistanceStrength*Mathf.Lerp(.35f,1,progress);
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
  internal GolfBallSnapshot Snapshot(uint round,double now)=>new GolfBallSnapshot{valid=true,active=Live,round=round,reset=ResetSequence,position=match.transform.InverseTransformPoint(Body.position),rotation=Quaternion.Inverse(match.transform.rotation)*Body.rotation,time=now};
  internal void Receive(GolfBallSnapshot snapshot){
   if(match.Authority||!snapshot.valid||snapshot.round!=match.Round)return;
   bool snap=!received||snapshot.reset!=target.reset||snapshot.round!=target.round;
   if(!received||snapshot.time!=target.time){before=received?target:snapshot;target=snapshot;receivedAt=Time.unscaledTime;received=true;}
   ResetSequence=snapshot.reset;
   SetLive(snapshot.active);if(snap){before=target;Present(1);}
  }
  void Update(){if(!received||!match||match.Authority)return;Present(Mathf.Clamp01((Time.unscaledTime-receivedAt)/Mathf.Clamp((float)(target.time-before.time),.02f,.2f)));}
  void Present(float alpha){var p=match.transform.TransformPoint(Vector3.Lerp(before.position,target.position,alpha));var q=match.transform.rotation*Quaternion.Slerp(before.rotation,target.rotation,alpha);Body.position=p;Body.rotation=q;transform.SetPositionAndRotation(p,q);}
 }
}
