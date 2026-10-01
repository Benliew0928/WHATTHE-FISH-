using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
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
  [Min(.01f)] public float controlDistance=1.2f,releaseDistance=1.6f;
  [Range(0,180)] public float controlAngle=55,releaseAngle=80;
  [Min(0)] public float controlSpeed=6.2f,releaseSpeed=7;
  [Range(0,1)] public float dribbleMovementMultiplier=.85f,chargeMovementMultiplier=.6f;
  [Min(0)] public float recaptureDelay=.3f;
  [Header("Slide contact")]
  [Min(0)] public float tackleBallSpeed=8;
  [Range(0,1)] public float tackleIncomingRetention=.65f;
  Athlete controller;
  readonly Dictionary<Athlete,float> recaptureUntil=new();
  readonly Dictionary<Athlete,double> pushedAt=new();
  public Athlete CurrentController {
   get {
    if(HasAuthority)return controller;
    if(!NetworkAthlete.HostPlayer)return null;
    var snapshot=NetworkAthlete.HostPlayer.Ball.Value;if(!snapshot.valid)return null;ulong id=snapshot.controllerId;
    foreach(var actor in Athlete.Active){var net=actor.GetComponent<NetworkObject>();if(net&&net.IsSpawned&&net.NetworkObjectId==id)return actor;}
    return null;
   }
  }
  bool CanControl(Athlete actor,bool retaining){
   if(!actor||!actor.isActiveAndEnabled||actor.inTransit||!actor.capsule||!actor.capsule.enabled||!actor.Grounded||actor.Action!=FootballAction.None)return false;
   if(recaptureUntil.TryGetValue(actor,out float until)&&Time.time<until)return false;
   var offset=Body.position-(actor.transform.position+Vector3.up*sphere.radius);var horizontal=Vector3.ProjectOnPlane(offset,Vector3.up);
   float distance=retaining?Mathf.Max(controlDistance,releaseDistance):controlDistance;
   float angle=retaining?Mathf.Max(controlAngle,releaseAngle):controlAngle;
   float speed=retaining?Mathf.Max(controlSpeed,releaseSpeed):controlSpeed;
   return horizontal.magnitude<=distance&&Mathf.Abs(offset.y)<=.5f&&Vector3.Angle(actor.transform.forward,horizontal)<=angle&&Vector3.ProjectOnPlane(Body.linearVelocity,Vector3.up).magnitude<=speed&&!Physics.Linecast(actor.transform.position+Vector3.up*.35f,Body.position,1<<8,QueryTriggerInteraction.Ignore);
  }
  public void RefreshControl(Athlete candidate=null){
   if(!HasAuthority)return;
   if(!Allowed||Body.isKinematic){controller=null;return;}
   if(controller&&!CanControl(controller,true))controller=null;
   if(!controller&&candidate&&CanControl(candidate,false)){controller=candidate;blockedPusher=null;}
  }
  public float MovementSpeed(Athlete actor,bool sprint,bool charging){
   RefreshControl(actor);
   // Charging replaces dribbling; each penalty uses the original speed.
   return charging&&InKickRange(actor)?4*chargeMovementMultiplier:(sprint?7:4)*(CurrentController==actor?dribbleMovementMultiplier:1);
  }
  public void ForgetPlayer(Athlete actor){if(controller==actor)controller=null;recaptureUntil.Remove(actor);pushedAt.Remove(actor);}
  void ReleaseControl(Athlete excluded=null){controller=null;if(excluded)recaptureUntil[excluded]=Time.time+recaptureDelay;}
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
  int enteredGoal=-1;
  SphereCollider sphere;Vector3 localOrigin;Quaternion localOriginRotation;bool playing,authority;float publishTimer,blend;
  Vector3 origin=>transform.parent?transform.parent.TransformPoint(localOrigin):localOrigin;
  Quaternion originRotation=>transform.parent?transform.parent.rotation*localOriginRotation:localOriginRotation;
  uint sequence,receivedSequence;FootballBallSnapshot received;Vector3 from;Quaternion fromRotation;bool impulsePending;
  readonly RaycastHit[] interceptionHits=new RaycastHit[32];
  Athlete blockedPusher;
  public static bool Allowed=>FootballTackle.Allowed&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling);
  bool Online=>NetworkManager.Singleton&&NetworkManager.Singleton.IsListening;
  public bool HasAuthority=>!Online||NetworkManager.Singleton.IsServer;
  void Awake(){
   Body=GetComponent<Rigidbody>();ApplyDamping();sphere=GetComponent<SphereCollider>();localOrigin=transform.localPosition;localOriginRotation=transform.localRotation;Body.solverIterations=12;Body.solverVelocityIterations=4;Body.maxAngularVelocity=120;sphere.contactOffset=.002f;
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
   }
   if(Pitch&&GoalCount!=2)Debug.LogError("Football boundary requires both authored goal posts, crossbars and nets.",this);
  }
  Bounds InPitch(Transform frame,Bounds source){
   var bounds=new Bounds(Pitch.InverseTransformPoint(frame.TransformPoint(source.min)),Vector3.zero);
   for(int corner=0;corner<8;corner++)bounds.Encapsulate(Pitch.InverseTransformPoint(frame.TransformPoint(new Vector3((corner&1)==0?source.min.x:source.max.x,(corner&2)==0?source.min.y:source.max.y,(corner&4)==0?source.min.z:source.max.z))));
   return bounds;
  }
  void OnEnable(){Instance=this;playing=false;received=default;}
  void OnDisable(){if(Instance==this)Instance=null;}
  public void ResetBall(){if(!HasAuthority)return;sequence++;impulsePending=false;blockedPusher=null;enteredGoal=-1;controller=null;recaptureUntil.Clear();pushedAt.Clear();Body.position=origin;Body.rotation=originRotation;if(!Body.isKinematic){Body.linearVelocity=Vector3.zero;Body.angularVelocity=Vector3.zero;}Body.Sleep();}
  void FixedUpdate(){
   authority=HasAuthority;
   bool active=Allowed&&(!Online||NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.Exploring.Value&&NetworkAthlete.HostPlayer.WorldSport.Value==SportId.Football);
   bool dynamic=authority&&active;
   if(Body.isKinematic==dynamic){
    if(!dynamic){Body.linearVelocity=Vector3.zero;Body.angularVelocity=Vector3.zero;Body.collisionDetectionMode=CollisionDetectionMode.Discrete;Body.isKinematic=true;}
    else {Body.isKinematic=false;Body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;}
   }
   Body.detectCollisions=authority;
   if(authority){
    if(active&&!playing)ResetBall();playing=active;
    if(!dynamic)controller=null;
    if(dynamic){
     RefreshControl();
     if(!controller){Athlete closest=null;float distance=float.MaxValue;foreach(var candidate in Athlete.Active)if(CanControl(candidate,false)){float d=(candidate.transform.position-Body.position).sqrMagnitude;if(d<distance){distance=d;closest=candidate;}}if(closest)RefreshControl(closest);}
     if(blockedPusher&&(blockedPusher.inTransit||Vector3.ProjectOnPlane(blockedPusher.transform.position-Body.position,Vector3.up).magnitude>sphere.radius*transform.lossyScale.x+blockedPusher.capsule.radius+blockedPusher.capsule.skinWidth+.2f))blockedPusher=null;
     float sea=RefinedIslandEnvironment.Active?RefinedIslandEnvironment.Active.layout.sea_level:0;
     if(Body.position.y<sea-2||Vector3.Distance(Body.position,origin)>300)ResetBall();
     KeepInsidePitch();
     ApplyRollingResistance(Time.fixedDeltaTime);
     SweepPlayers(Time.fixedDeltaTime);
    }
    if(Online&&NetworkAthlete.HostPlayer&&(publishTimer-=Time.fixedDeltaTime)<=0){publishTimer=.05f;NetworkAthlete.HostPlayer.Ball.Value=new FootballBallSnapshot{valid=true,position=Body.position,rotation=Body.rotation,sequence=sequence,controllerId=ControllerId()};}
   }else if(NetworkAthlete.HostPlayer){
    var value=NetworkAthlete.HostPlayer.Ball.Value;if(!value.valid)return;
    if(!received.valid||receivedSequence!=value.sequence){Body.position=value.position;Body.rotation=value.rotation;from=value.position;fromRotation=value.rotation;blend=1;}
    else if(!value.Equals(received)){from=Body.position;fromRotation=Body.rotation;blend=0;}
    received=value;receivedSequence=value.sequence;blend=Mathf.Min(1,blend+Time.fixedDeltaTime/.05f);
    Body.MovePosition(Vector3.Lerp(from,value.position,blend));Body.MoveRotation(Quaternion.Slerp(fromRotation,value.rotation,blend));
   }
  }
  ulong ControllerId(){var net=controller?controller.GetComponent<NetworkObject>():null;return net&&net.IsSpawned?net.NetworkObjectId:ulong.MaxValue;}
  // Only the authoritative ball is bounded; no wall colliders restrict athletes.
  // LateUpdate catches the result of PhysX impulses, including a kick at the line.
  void LateUpdate(){if(HasAuthority&&!Body.isKinematic&&Allowed)KeepInsidePitch();}
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
    float depth=Mathf.Max(0,(point.z-goal.front)*goal.sign);ceiling=goal.bounds.max.y-.43f*Mathf.Clamp01(depth/2.2f)-marginY;
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
   ReleaseControl();Body.linearVelocity=Vector3.zero;Body.angularVelocity=Vector3.zero;impulsePending=false;blockedPusher=actor;sequence++;
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
   if(contact&&!Intercept(actor))Push(actor,displacement,dt>0?Vector3.ProjectOnPlane(displacement,Vector3.up).magnitude/dt:0);
   return correction+motion+Vector3.up*displacement.y;
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
   if(!actor||!Allowed||actor.inTransit||!actor.Grounded||actor.Action!=FootballAction.None)return false;
   var offset=Body.position-(actor.transform.position+Vector3.up*sphere.radius);var horizontal=Vector3.ProjectOnPlane(offset,Vector3.up);
   if(horizontal.magnitude>maximumKickDistance||Mathf.Abs(offset.y)>.5f||Vector3.Dot(actor.transform.forward,horizontal.normalized)<.25f)return false;
   return !Physics.Linecast(actor.transform.position+Vector3.up*.35f,Body.position,1<<8,QueryTriggerInteraction.Ignore);
  }
  public bool TryKick(Athlete actor,float charge=1){
   if(float.IsNaN(charge)||float.IsInfinity(charge)||!HasAuthority||Body.isKinematic||!InKickRange(actor))return false;
   var direction=Vector3.ProjectOnPlane(actor.transform.forward,Vector3.up).normalized;
   float maximum=Mathf.Max(0,kickSpeed),speed=Mathf.Lerp(Mathf.Clamp(minimumKickSpeed,0,maximum),maximum,Mathf.Clamp01(charge));
   ReleaseControl(actor);blockedPusher=null;impulsePending=true;Body.WakeUp();
   // Subsequent callbacks see this velocity immediately instead of stacking deferred AddForce.
   Body.linearVelocity=direction*speed+Vector3.up*.25f;Body.angularVelocity=Vector3.zero;return true;
  }
  bool SlideContact(Athlete actor){
   if(!HasAuthority||Body.isKinematic||!Allowed||!actor||actor.Action!=FootballAction.Slide)return false;
   var slide=actor.GetComponent<FootballTackle>();
   if(slide&&slide.ClaimBallContact()){
    ReleaseControl();blockedPusher=null;
    var incoming=Vector3.ProjectOnPlane(Body.linearVelocity,Vector3.up);float speed=incoming.magnitude;
    var direction=slide.Direction;Vector3 result;
    if(speed<=tackleBallSpeed)result=Vector3.ClampMagnitude(direction*tackleBallSpeed+(incoming-direction*Vector3.Dot(incoming,direction))*.25f,tackleBallSpeed);
    else result=Vector3.ClampMagnitude(incoming*tackleIncomingRetention+direction*Mathf.Min(tackleBallSpeed,speed*.2f),speed*Mathf.Min(.95f,tackleIncomingRetention+.2f));
    Body.WakeUp();Body.linearVelocity=result+Vector3.up*Mathf.Min(0,Body.linearVelocity.y);
    Body.angularVelocity=Vector3.Cross(Vector3.up,result)/(sphere.radius*transform.lossyScale.x);impulsePending=true;
   }
   return true; // Subsequent callbacks from the same slide cannot hit or push again.
  }
  public void Push(Athlete actor,Vector3 movement,float movementSpeed=-1){
   if(!actor||!HasAuthority||Body.isKinematic||!Allowed||actor.inTransit)return;
   if(SlideContact(actor))return;
   if(actor.Action!=FootballAction.None||actor==blockedPusher||recaptureUntil.TryGetValue(actor,out float until)&&Time.time<until)return;
   if(pushedAt.TryGetValue(actor,out double tick)&&tick==Time.fixedTimeAsDouble)return;
   pushedAt[actor]=Time.fixedTimeAsDouble;
   var direction=Vector3.ProjectOnPlane(movement,Vector3.up).normalized;
   float closing=Vector3.Dot(direction,Body.position-actor.transform.position);
   if(closing<=0)return;
   float target=Mathf.Min(movementSpeed>=0?movementSpeed:actor.speed,7*dribbleMovementMultiplier);
   float impulse=Mathf.Clamp(target-Vector3.Dot(Body.linearVelocity,direction),0,1.5f);
   if(impulse>0){
    if(controller&&controller!=actor)ReleaseControl();RefreshControl(actor);
    var horizontal=Vector3.ProjectOnPlane(Body.linearVelocity,Vector3.up);
    var pushed=Vector3.ClampMagnitude(horizontal+direction*impulse,Mathf.Max(horizontal.magnitude,target));
    impulsePending=true;Body.WakeUp();Body.linearVelocity=pushed+Vector3.up*Body.linearVelocity.y;
   }
  }
 }
}
