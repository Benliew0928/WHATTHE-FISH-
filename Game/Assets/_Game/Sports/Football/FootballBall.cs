using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 [DefaultExecutionOrder(100)]
 [RequireComponent(typeof(Rigidbody),typeof(SphereCollider))]
 public sealed class FootballBall:MonoBehaviour {
  public static FootballBall Instance;
  [Header("Kick tuning (speed in metres/second)")]
  [Min(0)] public float minimumKickSpeed=12;
  [Min(0)] public float kickSpeed=22;
  [Min(.01f)] public float maximumChargeTime=1f;
  [Min(.01f)] public float maximumKickDistance=1.35f;
  [Header("Ball physics")]
  [Min(0)] public float linearDamping=.05f,angularDamping=.05f,rollingDeceleration=2f;
  [Header("Control (metres, degrees, metres/second)")]
  [Min(.01f)] public float controlDistance=1.2f,footControlDistance=.55f,footOffset=.65f;
  [Range(0,180)] public float controlAngle=55;
  [Min(0)] public float controlSpeed=6.2f;
  [Range(0,1)] public float dribbleMovementMultiplier=.75f,chargeMovementMultiplier=.6f;
  [Header("Slide contact")]
  [Min(0)] public float tackleBallSpeed=8;
  [Min(0)] public float tackleBallLift=4;
  [Range(0,1)] public float tackleIncomingRetention=.65f;
  Athlete controller;
  // Released participants must leave the acquisition area before approaching again.
  readonly HashSet<Athlete> mustApproach=new();
  RigidbodyInterpolation freeInterpolation;
  public Athlete CurrentController {
   get {
    if(HasAuthority)return controller;
    if(!NetworkAthlete.HostPlayer)return null;
    var snapshot=NetworkAthlete.HostPlayer.Ball.Value;if(!snapshot.valid)return null;ulong id=snapshot.controllerId;
    foreach(var actor in Athlete.Active){var net=actor.GetComponent<NetworkObject>();if(net&&net.IsSpawned&&net.NetworkObjectId==id)return actor;}
    return null;
   }
  }
  bool CanControl(Athlete actor){
   if(!actor||!actor.isActiveAndEnabled||actor.inTransit||!actor.capsule||!actor.capsule.enabled||!actor.Grounded||actor.Airborne||actor.LoadingJump||actor.Action!=FootballAction.None)return false;
   if(mustApproach.Contains(actor)){
    if(Vector3.ProjectOnPlane(Body.position-actor.transform.position,Vector3.up).magnitude<=controlDistance+.1f)return false;
    mustApproach.Remove(actor);
   }
   var offset=Body.position-(actor.transform.position+Vector3.up*WorldRadius);var horizontal=Vector3.ProjectOnPlane(offset,Vector3.up);
   return horizontal.magnitude<=controlDistance&&Vector3.ProjectOnPlane(Body.position-FootPosition(actor),Vector3.up).magnitude<=footControlDistance&&Mathf.Abs(offset.y)<=.5f&&Vector3.Angle(actor.transform.forward,horizontal)<=controlAngle&&Vector3.ProjectOnPlane(Body.linearVelocity,Vector3.up).magnitude<=controlSpeed&&!Physics.Linecast(actor.transform.position+Vector3.up*.35f,Body.position,1<<8,QueryTriggerInteraction.Ignore);
  }
  public void RefreshControl(Athlete candidate=null){
   if(!HasAuthority)return;
   if(!Allowed){ReleaseControl();return;}
   if(controller&&(!controller.isActiveAndEnabled||controller.inTransit||!controller.capsule.enabled))ReleaseControl();
   // Walking, sprinting, facing and distance never eject an attached ball.
   if(!controller&&!Body.isKinematic&&candidate&&CanControl(candidate)){
    controller=candidate;sequence++;
    Physics.IgnoreCollision(sphere,candidate.capsule,true);SetPhysicsMode(false);
    Body.interpolation=RigidbodyInterpolation.None;FollowController(candidate);
   }
  }
  public float MovementSpeed(Athlete actor,bool sprint,bool charging){
   RefreshControl(actor);
   // Charging replaces the possession penalty; both use the original base speed.
   return charging&&InKickRange(actor)?4*chargeMovementMultiplier:(sprint?7:4)*(CurrentController==actor?dribbleMovementMultiplier:1);
  }
  public void ForgetPlayer(Athlete actor){if(controller==actor)ReleaseControl();mustApproach.Remove(actor);}
  void ReleaseControl(Athlete excluded=null){
   if(controller){if(controller.capsule)Physics.IgnoreCollision(sphere,controller.capsule,false);controller=null;sequence++;Body.interpolation=freeInterpolation;SetPhysicsMode(Allowed);}
   if(excluded)mustApproach.Add(excluded);
  }
  public void ReleaseFromTackle(Athlete victim,Athlete tackler,Vector3 direction){
   if(!HasAuthority||controller!=victim||!Allowed)return;
   ReleaseControl(victim);if(tackler){mustApproach.Add(tackler);tackler.GetComponent<FootballTackle>()?.ClaimBallContact();}
   Body.WakeUp();Body.linearVelocity=Vector3.ProjectOnPlane(direction,Vector3.up).normalized*tackleBallSpeed+Vector3.up*tackleBallLift;Body.angularVelocity=Vector3.zero;impulsePending=true;
  }
  public Vector3 FootPosition(Athlete actor){
   var position=actor.transform.position+actor.transform.forward*Mathf.Max(footOffset,WorldRadius+(actor.capsule?actor.capsule.radius+actor.capsule.skinWidth:0)+.02f);
   float ground=actor.transform.position.y;
   if(Physics.Raycast(position+Vector3.up*.5f,Vector3.down,out var hit,1.5f,1<<8,QueryTriggerInteraction.Ignore))ground=hit.point.y;
   position.y=ground+WorldRadius;return position;
  }
  public void FollowController(Athlete actor){
   if(!actor||CurrentController!=actor)return;
   var position=FootPosition(actor);var travel=position-Body.position;
   if(travel.sqrMagnitude>.000001f)Body.rotation=Quaternion.AngleAxis(travel.magnitude/WorldRadius*Mathf.Rad2Deg,Vector3.Cross(Vector3.up,travel).normalized)*Body.rotation;
   Body.position=position;transform.SetPositionAndRotation(position,Body.rotation);
  }
  void SetPhysicsMode(bool dynamic){
   if(Body.isKinematic!=!dynamic){
    if(!dynamic){Body.linearVelocity=Vector3.zero;Body.angularVelocity=Vector3.zero;}
    Body.collisionDetectionMode=CollisionDetectionMode.Discrete;Body.isKinematic=!dynamic;
   }
   Body.collisionDetectionMode=dynamic?CollisionDetectionMode.ContinuousDynamic:CollisionDetectionMode.ContinuousSpeculative;
  }
  void OnValidate(){maximumChargeTime=Mathf.Max(.01f,maximumChargeTime);maximumKickDistance=Mathf.Max(.01f,maximumKickDistance);kickSpeed=Mathf.Max(0,kickSpeed);minimumKickSpeed=Mathf.Clamp(minimumKickSpeed,0,kickSpeed);ApplyDamping();}
  void ApplyDamping(){var body=GetComponent<Rigidbody>();if(body){body.linearDamping=Mathf.Max(0,linearDamping);body.angularDamping=Mathf.Max(0,angularDamping);}}
  public float ChargeFraction(float seconds)=>Mathf.Clamp01(seconds/Mathf.Max(.01f,maximumChargeTime));
  public Rigidbody Body {get;private set;}
  public Transform Pitch {get;private set;}
  public Bounds PitchBounds {get;private set;}
  struct GoalArea {public Bounds bounds;public float front;public int sign;}
  readonly GoalArea[] goals=new GoalArea[2];
  public int GoalCount {get;private set;}
  public Bounds GoalBounds(int index)=>goals[index].bounds;
  public float GoalFront(int index)=>goals[index].front;
  public int GoalSign(int index)=>goals[index].sign;
  public float WorldRadius=>sphere.radius*Mathf.Max(Mathf.Abs(transform.lossyScale.x),Mathf.Abs(transform.lossyScale.y),Mathf.Abs(transform.lossyScale.z));
  public void SetMatchDynamic(bool dynamic){if(!HasAuthority)return;if(!dynamic)ReleaseControl();SetPhysicsMode(dynamic&&!controller);}
  int enteredGoal=-1;
  SphereCollider sphere;Vector3 localOrigin;Quaternion localOriginRotation;bool playing,authority;float publishTimer,blend;
  Vector3 origin=>transform.parent?transform.parent.TransformPoint(localOrigin):localOrigin;
  public Vector3 KickoffPosition=>origin;
  Quaternion originRotation=>transform.parent?transform.parent.rotation*localOriginRotation:localOriginRotation;
  uint sequence,receivedSequence;FootballBallSnapshot received;Vector3 from;Quaternion fromRotation;bool impulsePending;
  readonly RaycastHit[] interceptionHits=new RaycastHit[32];
  public static bool Allowed=>!FootballMatch.BlocksActions&&FootballTackle.Allowed&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling);
  bool Online=>NetworkManager.Singleton&&NetworkManager.Singleton.IsListening;
  public bool HasAuthority=>!Online||NetworkManager.Singleton.IsServer;
  void Awake(){
   Body=GetComponent<Rigidbody>();freeInterpolation=Body.interpolation;ApplyDamping();sphere=GetComponent<SphereCollider>();localOrigin=transform.localPosition;localOriginRotation=transform.localRotation;Body.solverIterations=12;Body.solverVelocityIterations=4;Body.maxAngularVelocity=120;sphere.contactOffset=.002f;
   if(transform.parent)foreach(var mesh in transform.parent.GetComponentsInChildren<MeshFilter>(true))if(mesh.name=="Lawn__Pitch"&&mesh.sharedMesh){
    Pitch=transform.parent;var source=mesh.sharedMesh.bounds;var bounds=new Bounds(Pitch.InverseTransformPoint(mesh.transform.TransformPoint(source.min)),Vector3.zero);
    for(int corner=0;corner<8;corner++)bounds.Encapsulate(Pitch.InverseTransformPoint(mesh.transform.TransformPoint(new Vector3((corner&1)==0?source.min.x:source.max.x,(corner&2)==0?source.min.y:source.max.y,(corner&4)==0?source.min.z:source.max.z))));
    PitchBounds=bounds;break;
   }
   if(!Pitch)Debug.LogError("Football ball requires the authored Lawn__Pitch mesh for its field boundary.",this);
   if(Pitch)foreach(var goal in Pitch.GetComponentsInChildren<Transform>(true))if(goal.name=="Goal_North"||goal.name=="Goal_South"){
    BoxCollider left=null,right=null,bar=null;MeshFilter net=null;
    foreach(var collider in goal.GetComponentsInChildren<BoxCollider>(true)){
     if(collider.name=="Collision_Crossbar")bar=collider;
     else if(collider.name=="Collision_Post"){if(!left)left=collider;else right=collider;}
    }
    foreach(var mesh in goal.GetComponentsInChildren<MeshFilter>(true))if(mesh.name=="Goal__Net")net=mesh;
    if(!left||!right||!bar||!net||GoalCount==goals.Length)continue;
    var a=InPitch(left.transform,new Bounds(left.center,left.size));var b=InPitch(right.transform,new Bounds(right.center,right.size));
    if(a.center.x>b.center.x){var swap=a;a=b;b=swap;}
    var top=InPitch(bar.transform,new Bounds(bar.center,bar.size));var back=InPitch(net.transform,net.sharedMesh.bounds);
    float front=Pitch.InverseTransformPoint(goal.position).z;int sign=front>PitchBounds.center.z?1:-1;
    var bounds=new Bounds();bounds.SetMinMax(new Vector3(a.max.x,PitchBounds.max.y,sign>0?PitchBounds.max.z:back.min.z+.01f),new Vector3(b.min.x,top.min.y,sign>0?back.max.z-.01f:PitchBounds.min.z));
    goals[GoalCount++]=new GoalArea{bounds=bounds,front=front,sign=sign};
    FootballGoalNet.Attach(goal,Pitch,bounds,front,sign);
   }
   if(!GetComponent<FootballMatch>())gameObject.AddComponent<FootballMatch>();
   if(Pitch&&GoalCount!=2)Debug.LogError("Football boundary requires both authored goal posts, crossbars and nets.",this);
  }
  Bounds InPitch(Transform frame,Bounds source){
   var bounds=new Bounds(Pitch.InverseTransformPoint(frame.TransformPoint(source.min)),Vector3.zero);
   for(int corner=0;corner<8;corner++)bounds.Encapsulate(Pitch.InverseTransformPoint(frame.TransformPoint(new Vector3((corner&1)==0?source.min.x:source.max.x,(corner&2)==0?source.min.y:source.max.y,(corner&4)==0?source.min.z:source.max.z))));
   return bounds;
  }
  void OnEnable(){Instance=this;playing=false;received=default;}
  void OnDisable(){ReleaseControl();mustApproach.Clear();if(Instance==this)Instance=null;}
  void SnapPose(Vector3 position,Quaternion rotation){
   // A sleeping interpolated body otherwise keeps its old rendered pose until contact wakes it.
   var interpolation=Body.interpolation;Body.interpolation=RigidbodyInterpolation.None;
   Body.position=position;Body.rotation=rotation;transform.SetPositionAndRotation(position,rotation);
   Physics.SyncTransforms();Body.interpolation=interpolation;
  }
  public void ResetBall(){if(!HasAuthority)return;sequence++;impulsePending=false;enteredGoal=-1;ReleaseControl();mustApproach.Clear();SnapPose(origin,originRotation);if(!Body.isKinematic){Body.linearVelocity=Vector3.zero;Body.angularVelocity=Vector3.zero;}Body.Sleep();}
  void FixedUpdate(){
   authority=HasAuthority;
   bool active=FootballTackle.EnvironmentAllowed&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling)&&(!Online||NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.Exploring.Value&&NetworkAthlete.HostPlayer.WorldSport.Value==SportId.Football);
   bool dynamic=authority&&active&&!FootballMatch.BlocksActions;
   SetPhysicsMode(dynamic&&!controller);
   Body.detectCollisions=authority;
   if(authority){
    if(active&&!playing)ResetBall();playing=active;
    if(!dynamic)ReleaseControl();
    if(dynamic){
     RefreshControl();
     if(!controller){Athlete closest=null;float distance=float.MaxValue;foreach(var candidate in Athlete.Active)if(CanControl(candidate)){float d=(candidate.transform.position-Body.position).sqrMagnitude;if(d<distance){distance=d;closest=candidate;}}if(closest)RefreshControl(closest);}
     float sea=RefinedIslandEnvironment.Active?RefinedIslandEnvironment.Active.layout.sea_level:0;
     if(Body.position.y<sea-2||Vector3.Distance(Body.position,origin)>300)ResetBall();
     if(controller)FollowController(controller);
     else {KeepInsidePitch();ApplyRollingResistance(Time.fixedDeltaTime);SweepPlayers(Time.fixedDeltaTime);}
    }
    if(Online&&NetworkAthlete.HostPlayer&&(publishTimer-=Time.fixedDeltaTime)<=0){publishTimer=.05f;NetworkAthlete.HostPlayer.Ball.Value=new FootballBallSnapshot{valid=true,position=Body.position,rotation=Body.rotation,sequence=sequence,controllerId=ControllerId()};}
   }else if(NetworkAthlete.HostPlayer){
    var value=NetworkAthlete.HostPlayer.Ball.Value;if(!value.valid)return;
    if(!received.valid||receivedSequence!=value.sequence){SnapPose(value.position,value.rotation);from=value.position;fromRotation=value.rotation;blend=1;}
    else if(!value.Equals(received)){from=Body.position;fromRotation=Body.rotation;blend=0;}
    received=value;receivedSequence=value.sequence;blend=Mathf.Min(1,blend+Time.fixedDeltaTime/.05f);
    Body.MovePosition(Vector3.Lerp(from,value.position,blend));Body.MoveRotation(Quaternion.Slerp(fromRotation,value.rotation,blend));
   }
  }
  ulong ControllerId(){var net=controller?controller.GetComponent<NetworkObject>():null;return net&&net.IsSpawned?net.NetworkObjectId:ulong.MaxValue;}
  // Only the authoritative ball is bounded by pitch lines; nets have shared world collision.
  // LateUpdate catches the result of PhysX impulses, including a kick at the line.
  void LateUpdate(){var owner=CurrentController;Body.interpolation=owner?RigidbodyInterpolation.None:freeInterpolation;if(owner&&Allowed)FollowController(owner);else if(HasAuthority&&!Body.isKinematic&&Allowed)KeepInsidePitch();}
  void KeepInsidePitch(){
   if(!Pitch)return;
   var point=Pitch.InverseTransformPoint(Body.position);var original=point;
   float radius=sphere.radius*Mathf.Max(Mathf.Abs(transform.lossyScale.x),Mathf.Abs(transform.lossyScale.z));
   float marginX=radius/Mathf.Abs(Pitch.lossyScale.x),marginZ=radius/Mathf.Abs(Pitch.lossyScale.z);
   float marginY=radius/Mathf.Abs(Pitch.lossyScale.y);
   if(enteredGoal>=0&&point.z>=PitchBounds.min.z+marginZ&&point.z<=PitchBounds.max.z-marginZ)enteredGoal=-1;
   if(enteredGoal<0)for(int i=0;i<GoalCount;i++){
    var goal=goals[i];float line=goal.sign>0?PitchBounds.max.z-marginZ:PitchBounds.min.z+marginZ;
    if((point.z-line)*goal.sign>0&&point.x>=goal.bounds.min.x+marginX&&point.x<=goal.bounds.max.x-marginX&&point.y+marginY<=goal.bounds.max.y&&point.y-marginY>=goal.bounds.min.y-.04f){enteredGoal=i;break;}
   }
   float ceiling=float.PositiveInfinity;
   if(enteredGoal>=0){
    var goal=goals[enteredGoal];point.x=Mathf.Clamp(point.x,goal.bounds.min.x+marginX,goal.bounds.max.x-marginX);
    point.z=goal.sign>0?Mathf.Min(point.z,goal.bounds.max.z-marginZ):Mathf.Max(point.z,goal.bounds.min.z+marginZ);
    // The authored roof falls from 2.44 m at the mouth to 2.01 m at the back.
    ceiling=FootballGoalNet.RoofHeight(goal.bounds,goal.front,goal.sign,point.z)-marginY;
    point.y=Mathf.Min(point.y,ceiling);
   }else{
    point.x=Mathf.Clamp(point.x,PitchBounds.min.x+marginX,PitchBounds.max.x-marginX);
    point.z=Mathf.Clamp(point.z,PitchBounds.min.z+marginZ,PitchBounds.max.z-marginZ);
   }
   var velocity=Pitch.InverseTransformDirection(Body.linearVelocity);var spin=Pitch.InverseTransformDirection(Body.angularVelocity);bool hit=false;
   if(point.x!=original.x){if(velocity.x*(original.x-point.x)>0)velocity.x=0;spin.z=0;hit=true;}
   if(point.z!=original.z){if(velocity.z*(original.z-point.z)>0)velocity.z=0;spin.x=0;hit=true;}
   if(point.y!=original.y){if(velocity.y>0)velocity.y=0;hit=true;}
   if(!hit)return;
   Body.position=Pitch.TransformPoint(point);Body.linearVelocity=Pitch.TransformDirection(velocity);Body.angularVelocity=Pitch.TransformDirection(spin);impulsePending=false;sequence++;
   if(Vector3.ProjectOnPlane(Body.linearVelocity,Vector3.up).sqrMagnitude<.0064f)Body.angularVelocity=Vector3.zero;
  }
  // PhysX prevents penetration; interception also removes tangential speed/spin.
  void StopAtPlayer(Athlete actor){
   if(!HasAuthority||Body.isKinematic||!Allowed)return;
   if(SlideContact(actor))return;
   ReleaseControl();Body.linearVelocity=Vector3.zero;Body.angularVelocity=Vector3.zero;impulsePending=false;sequence++;
   // Grounded rolling can sleep, but an airborne contact must still fall.
   float radius=sphere.radius*transform.lossyScale.x;
   if(Physics.Raycast(Body.position,Vector3.down,radius+.04f,1<<8,QueryTriggerInteraction.Ignore))Body.Sleep();
  }
  void SweepPlayers(float dt){
   var velocity=Body.linearVelocity;float distance=velocity.magnitude*dt;if(distance<.001f)return;
   var direction=velocity.normalized;float radius=sphere.radius*transform.lossyScale.x;
   int count=Physics.SphereCastNonAlloc(Body.position,radius*.95f,direction,interceptionHits,distance+.002f,~0,QueryTriggerInteraction.Ignore);
   float nearest=float.MaxValue;Collider obstacle=null;
   for(int i=0;i<count;i++){var hit=interceptionHits[i];if(hit.collider.attachedRigidbody==Body||hit.distance>=nearest)continue;nearest=hit.distance;obstacle=hit.collider;}
   if(!obstacle||!obstacle.GetComponentInParent<Athlete>())return;
   Body.position+=direction*Mathf.Max(0,nearest-radius*.05f-.002f);StopAtPlayer(obstacle.GetComponentInParent<Athlete>());
  }
  void OnCollisionEnter(Collision collision){var actor=collision.collider.GetComponentInParent<Athlete>();if(actor)StopAtPlayer(actor);}
  public bool Intercept(Athlete actor){
   if(!actor||actor.inTransit||!HasAuthority||Body.isKinematic||!Allowed)return false;
   if(SlideContact(actor))return true;
   var toward=Vector3.ProjectOnPlane(actor.transform.position-Body.position,Vector3.up);
   if(Vector3.Dot(Body.linearVelocity,toward)<=.001f)return false;
   StopAtPlayer(actor);return true;
  }
  // The controller's rounded foot/step solver must not climb the football.
  // Sweep its horizontal footprint instead, keeping vertical terrain movement intact.
  public Vector3 ConstrainPlayerMotion(Athlete actor,Vector3 displacement,float dt){
   if(!Allowed||actor.inTransit||!actor.capsule||!actor.capsule.enabled)return displacement;
   if(CurrentController==actor)return ConstrainAttachedMotion(actor,displacement);
   var controller=actor.capsule;float ballRadius=sphere.radius*transform.lossyScale.x;
   var bounds=controller.bounds;
   if(bounds.min.y>Body.position.y+ballRadius+.1f||bounds.max.y<Body.position.y-ballRadius)return displacement;
   float radius=controller.radius*Mathf.Max(actor.transform.lossyScale.x,actor.transform.lossyScale.z)+ballRadius+controller.skinWidth+.01f;
   var offset=Vector3.ProjectOnPlane(bounds.center-Body.position,Vector3.up);
   var motion=Vector3.ProjectOnPlane(displacement,Vector3.up);var correction=Vector3.zero;bool contact=false;
   float distance=offset.magnitude;
   if(distance<radius){
    var normal=distance>.001f?offset/distance:(motion.sqrMagnitude>.000001f?-motion.normalized:-actor.transform.forward);
    correction=normal*(radius-distance);offset+=correction;contact=true;
   }
   float lengthSquared=motion.sqrMagnitude,approach=Vector3.Dot(offset,motion);
   if(lengthSquared>.000001f&&approach<0){
    float discriminant=approach*approach-lengthSquared*(offset.sqrMagnitude-radius*radius);
    if(discriminant>=0){
     float fraction=(-approach-Mathf.Sqrt(discriminant))/lengthSquared;
     if(fraction>=-.001f&&fraction<=1){
      fraction=Mathf.Clamp01(fraction);var reached=motion*fraction;var normal=(offset+reached).normalized;
      var remaining=motion*(1-fraction);remaining-=normal*Mathf.Min(0,Vector3.Dot(remaining,normal));
      motion=reached+remaining;contact=true;
     }
    }
   }
   if(contact)Intercept(actor);
   return correction+motion+Vector3.up*displacement.y;
  }
  Vector3 ConstrainAttachedMotion(Athlete actor,Vector3 displacement){
   if(!Pitch)return displacement;
   var anchor=FootPosition(actor);var target=anchor+Vector3.ProjectOnPlane(displacement,Vector3.up);
   var point=Pitch.InverseTransformPoint(target);float radius=WorldRadius;
   float mx=radius/Mathf.Abs(Pitch.lossyScale.x),mz=radius/Mathf.Abs(Pitch.lossyScale.z);
   point.x=Mathf.Clamp(point.x,PitchBounds.min.x+mx,PitchBounds.max.x-mx);
   bool mouth=false;
   for(int i=0;i<GoalCount;i++){
    var goal=goals[i];if(point.x<goal.bounds.min.x+mx||point.x>goal.bounds.max.x-mx)continue;
    if((point.z-goal.front)*goal.sign>=-1){point.z=goal.sign>0?Mathf.Min(point.z,goal.bounds.max.z-mz):Mathf.Max(point.z,goal.bounds.min.z+mz);mouth=true;break;}
   }
   if(!mouth)point.z=Mathf.Clamp(point.z,PitchBounds.min.z+mz,PitchBounds.max.z-mz);
   var motion=Vector3.ProjectOnPlane(Pitch.TransformPoint(point)-anchor,Vector3.up);
   if(motion.sqrMagnitude>.000001f&&Physics.SphereCast(anchor,radius*.95f,motion.normalized,out var hit,motion.magnitude,1<<8,QueryTriggerInteraction.Ignore))motion=motion.normalized*Mathf.Max(0,hit.distance-.002f);
   return motion+Vector3.up*displacement.y;
  }
  // Only grounded rolling loses speed; gravity and collision response stay with PhysX.
  public void ApplyRollingResistance(float dt){
   // Give a newly applied contact/kick one physics step before resistance or sleeping.
   if(impulsePending){impulsePending=false;return;}
   if(Body.isKinematic||Body.IsSleeping())return;
   // Free physics is not restricted by a low global speed cap.
   float radius=sphere.radius*transform.lossyScale.x;
   if(!Physics.SphereCast(Body.position,.9f*radius,Vector3.down,out var ground,.1f+radius*.1f,1<<8,QueryTriggerInteraction.Ignore)||ground.normal.y<.55f)return;
   var velocity=Vector3.ProjectOnPlane(Body.linearVelocity,ground.normal);
   if(velocity.magnitude<.08f&&Mathf.Abs(Body.linearVelocity.y)<.1f&&Body.angularVelocity.magnitude<.5f){Body.linearVelocity=Vector3.zero;Body.angularVelocity=Vector3.zero;Body.Sleep();return;}
   Body.AddForce(-velocity.normalized*Mathf.Min(rollingDeceleration,velocity.magnitude/dt),ForceMode.Acceleration);
   Body.AddTorque(-Body.angularVelocity.normalized*Mathf.Min(rollingDeceleration/radius,Body.angularVelocity.magnitude/dt),ForceMode.Acceleration);
  }
  public bool InKickRange(Athlete actor){
   if(!actor||!Allowed||actor.inTransit||!actor.Grounded||actor.Action!=FootballAction.None||CurrentController&&CurrentController!=actor)return false;
   var position=CurrentController==actor?FootPosition(actor):Body.position;
   var offset=position-(actor.transform.position+Vector3.up*WorldRadius);var horizontal=Vector3.ProjectOnPlane(offset,Vector3.up);
   if(horizontal.magnitude>maximumKickDistance||Mathf.Abs(offset.y)>.5f||Vector3.Dot(actor.transform.forward,horizontal.normalized)<.25f)return false;
   return !Physics.Linecast(actor.transform.position+Vector3.up*.35f,position,1<<8,QueryTriggerInteraction.Ignore);
  }
  public static Vector3 KickDirection(Athlete actor)=>Vector3.ProjectOnPlane(actor.transform.forward,Vector3.up).normalized;
  public bool TryKick(Athlete actor,float charge=1){
   if(float.IsNaN(charge)||float.IsInfinity(charge)||!HasAuthority||Body.isKinematic&&controller!=actor||!InKickRange(actor))return false;
   if(controller==actor)FollowController(actor);
   var direction=KickDirection(actor);
   float maximum=Mathf.Max(0,kickSpeed),speed=Mathf.Lerp(Mathf.Clamp(minimumKickSpeed,0,maximum),maximum,Mathf.Clamp01(charge));
   ReleaseControl(actor);impulsePending=true;Body.WakeUp();
   // Subsequent callbacks see this velocity immediately instead of stacking deferred AddForce.
   Body.linearVelocity=direction*speed+Vector3.up*.25f;Body.angularVelocity=Vector3.zero;return true;
  }
  bool SlideContact(Athlete actor){
   if(!HasAuthority||Body.isKinematic||!Allowed||!actor||actor.Action!=FootballAction.Slide)return false;
   var slide=actor.GetComponent<FootballTackle>();
   if(slide&&slide.ClaimBallContact()){
    ReleaseControl();
    var incoming=Vector3.ProjectOnPlane(Body.linearVelocity,Vector3.up);float speed=incoming.magnitude;
    var direction=slide.Direction;Vector3 result;
    if(speed<=tackleBallSpeed)result=Vector3.ClampMagnitude(direction*tackleBallSpeed+(incoming-direction*Vector3.Dot(incoming,direction))*.25f,tackleBallSpeed);
    else result=Vector3.ClampMagnitude(incoming*tackleIncomingRetention+direction*Mathf.Min(tackleBallSpeed,speed*.2f),speed*Mathf.Min(.95f,tackleIncomingRetention+.2f));
    Body.WakeUp();Body.linearVelocity=result+Vector3.up*tackleBallLift;
    Body.angularVelocity=Vector3.Cross(Vector3.up,result)/(sphere.radius*transform.lossyScale.x);impulsePending=true;
   }
   return true; // Subsequent callbacks from the same slide cannot hit or push again.
  }
 }
}
