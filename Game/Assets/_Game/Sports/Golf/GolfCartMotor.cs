using UnityEngine;

namespace WhatTheFish {
 // Host-only kinematic driving: bounded sweeps, four ground samples and a
 // supported exit keep the cart on the island without adding a physics package.
 public static class GolfCartMotor {
  public const int Layer=11, Obstacles=(1<<0)|(1<<8)|(1<<9)|(1<<Layer);
  public const float SpeedMultiplier=2f;
  public const float ForwardSpeed=8.5f*SpeedMultiplier,ReverseSpeed=4*SpeedMultiplier,Acceleration=10*SpeedMultiplier,Brake=16*SpeedMultiplier,Steering=120;
  public const float WheelSteeringAngle=38;
  public static readonly Vector3 HalfBody=new(.72f,.37f,1.20f);
  static readonly Collider[] overlaps=new Collider[64];
  static readonly RaycastHit[] casts=new RaycastHit[64];
  static readonly Vector3[] supportPoints=new Vector3[4];
  static readonly Vector3[] wheels={new(-.54f,0,-.94f),new(.54f,0,-.94f),new(-.54f,0,.94f),new(.54f,0,.94f)};
  public static bool Terrain(Collider collider)=>collider&&collider.name.StartsWith("Terrain__");
  // The body sweep delegates authored pond rims to wheel support below.
  // Rocks, other carts and the sea retain their existing collision rules.
  public static bool PondBank(Collider collider)=>collider&&collider.name=="Pond bank"&&collider.GetComponentInParent<GolfCourse>();
  // Only the authored thin paving ribbons are drivable support. Border stones,
  // pavilion walls, rocks and bridge rails remain ordinary solid obstacles.
  public static bool Walkway(Collider collider){if(!collider||collider.gameObject.layer!=8)return false;string name=collider.name;return name.EndsWith("_Paving")&&(name.Contains("Path_")||name.Contains("Approach_"));}
  public static float Heading(Quaternion rotation)=>
   (Quaternion.Inverse(Quaternion.FromToRotation(Vector3.up,rotation*Vector3.up))*rotation).eulerAngles.y;
  public static bool Floor(Vector3 at,out RaycastHit floor) {
   floor=default;float best=float.MaxValue;
   int count=Physics.RaycastNonAlloc(at+Vector3.up*12,Vector3.down,casts,30,1<<8,QueryTriggerInteraction.Ignore);
   for(int i=0;i<count;i++)if((Terrain(casts[i].collider)||Walkway(casts[i].collider))&&casts[i].normal.y>.80f&&casts[i].distance<best){floor=casts[i];best=floor.distance;}
   var island=RefinedIslandEnvironment.Active;
   return best<float.MaxValue&&floor.normal.y>.80f&&(!island||floor.point.y>island.layout.sea_level+.12f);
  }
  static bool WheelFloor(Vector3 at,out Vector3 point,out Vector3 normal,out bool pond){
   point=normal=default;pond=false;float terrainDistance=float.MaxValue,bankHeight=float.NegativeInfinity;RaycastHit ground=default,bank=default;
   int count=Physics.RaycastNonAlloc(at+Vector3.up*12,Vector3.down,casts,30,1<<8,QueryTriggerInteraction.Ignore);
   for(int i=0;i<count;i++){
    var hit=casts[i];if((Terrain(hit.collider)||Walkway(hit.collider))&&hit.normal.y>.80f&&hit.distance<terrainDistance){ground=hit;terrainDistance=hit.distance;}
    if(PondBank(hit.collider)&&hit.point.y>bankHeight){bank=hit;bankHeight=hit.point.y;}
   }
   var island=RefinedIslandEnvironment.Active;
   if(terrainDistance==float.MaxValue||ground.normal.y<=.80f||island&&ground.point.y<=island.layout.sea_level+.12f)return false;
   point=ground.point;normal=ground.normal;
   if(bankHeight>point.y){point=bank.point;normal=bank.normal;pond=true;}
   var match=GolfMatchManager.Instance;var course=match?match.Course:null;
   if(course){var local=course.transform.InverseTransformPoint(at);
    foreach(var water in course.waterPatches){
     float u=(local.x-water.centre.x)/water.radii.x,v=(local.z-water.centre.z)/water.radii.y,r=Mathf.Sqrt(u*u+v*v);
     if(r>1.08f)continue;
     // Match the actual bank crest at 1.08, easing down to the water plane.
     // This is cart-only support; ball hazards and walking colliders stay intact.
     float crest=.10f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.9f,1.08f,r));
     float y=course.transform.TransformPoint(new Vector3(local.x,water.centre.y+crest,local.z)).y;
     if(y>point.y){point.y=y;normal=Vector3.up;pond=true;}
    }
   }
   return true;
  }
  public static bool Surface(Vector3 position,float yaw,out Vector3 supported,out Quaternion rotation) {
   supported=position;rotation=Quaternion.Euler(0,yaw,0);var normal=Vector3.zero;float height=0;int contacts=0;bool onPond=false;
   for(int i=0;i<wheels.Length;i++){
    if(!WheelFloor(position+rotation*wheels[i],out var point,out var up,out var pond))continue;
    supportPoints[i]=point;normal+=up;height+=point.y;contacts++;onPond|=pond;
   }
   if(contacts<3)return false;
   if(contacts==4){
    var forward=rotation*new Vector3(0,(supportPoints[2].y+supportPoints[3].y-supportPoints[0].y-supportPoints[1].y)*.5f,1.88f);
    var right=rotation*new Vector3(1.08f,(supportPoints[1].y+supportPoints[3].y-supportPoints[0].y-supportPoints[2].y)*.5f,0);
    normal=Vector3.Cross(forward,right);
   }
   normal.Normalize();if(normal.y<(onPond?.65f:.82f))return false;
   supported.y=height/contacts;rotation=Quaternion.FromToRotation(Vector3.up,normal)*rotation;return true;
  }
  public static bool Clear(Vector3 position,Quaternion rotation,GolfCart self,Athlete ignored=null) {
   var center=position+rotation*new Vector3(0,.66f,0);
   int count=Physics.OverlapBoxNonAlloc(center,HalfBody,overlaps,rotation,Obstacles,QueryTriggerInteraction.Ignore);
   for(int i=0;i<count;i++){
    var collider=overlaps[i];if(Terrain(collider)||Walkway(collider)||PondBank(collider)||self&&collider.transform.IsChildOf(self.transform)||ignored&&collider.transform.IsChildOf(ignored.transform))continue;
    return false;
   }
   return count<overlaps.Length;
  }
  static float Distance(GolfCart self,Vector3 position,Quaternion rotation,Vector3 delta,out GolfCart other,out Vector3 normal,out Athlete pedestrian) {
   other=null;normal=Vector3.zero;pedestrian=null;
   if(delta.sqrMagnitude<.0000001f)return 0;
   float distance=delta.magnitude,allowed=distance;
   int count=Physics.BoxCastNonAlloc(position+rotation*new Vector3(0,.66f,0),HalfBody,delta/distance,casts,rotation,distance+.04f,Obstacles,QueryTriggerInteraction.Ignore);
   for(int i=0;i<count;i++){
    // Wheel support below validates terrain height, slope and shore clearance.
    // A level body sweep can start inside an uphill terrain triangle; PhysX
    // reports that overlap with a horizontal normal, falsely making it a wall.
    var hit=casts[i];if(Terrain(hit.collider)||Walkway(hit.collider)||PondBank(hit.collider)||self&&hit.collider.transform.IsChildOf(self.transform)||hit.normal.y>.55f)continue;
    float next=Mathf.Max(0,hit.distance-.04f);
    if(next<=allowed){allowed=next;other=hit.collider.GetComponentInParent<GolfCart>();normal=hit.normal;pedestrian=hit.collider.GetComponentInParent<Athlete>();}
   }
   return count==casts.Length?0:allowed;
  }
  public static void Step(GolfCart cart,ref GolfCartState state,Vector2 input,float dt) {
   input=Vector2.ClampMagnitude(input,1);dt=Mathf.Clamp(dt,0,.25f);
   state.steering=input.x*WheelSteeringAngle;
   // Equal stick strength means equal throttle even when some of it steers.
   // Retain forward/reverse intent and stationary side-only steering.
   float throttle=Mathf.Abs(input.y)<.08f?0:input.magnitude*Mathf.Sign(input.y);
   int ticks=Mathf.Max(1,Mathf.CeilToInt(dt*60));float step=dt/ticks;
   for(int tick=0;tick<ticks;tick++){
    float boost=GolfMatchManager.Instance?GolfMatchManager.Instance.EventCartMultiplier:1;
    float wanted=throttle*(throttle<0?ReverseSpeed:ForwardSpeed)*boost;
    float rate=throttle==0||state.speed*wanted<0?Brake:Acceleration;
    state.speed=Mathf.MoveTowards(state.speed,wanted,rate*step);
    // Remove terrain tilt before extracting steering yaw; Euler Y alone would
    // repeatedly add slope roll/pitch and turn an idle cart without stick input.
    float yaw=Heading(state.rotation);
    if(Mathf.Abs(state.speed)>.08f)yaw+=input.x*Steering*Mathf.Clamp01(Mathf.Abs(state.speed)/2)*Mathf.Sign(state.speed)*step;
    var facing=Quaternion.Euler(0,yaw,0);var delta=facing*Vector3.forward*(state.speed*step);
    float travel=Distance(cart,state.position,facing,delta,out var other,out var hitNormal,out var pedestrian);
    if(travel+.0001f<delta.magnitude){if(cart){if(other)GolfCartWorld.Impact(cart,other,hitNormal);else if(pedestrian&&pedestrian.HitByGolfCart(delta.normalized,Mathf.Abs(state.speed))){GolfFeedback.Emit(GolfFeedbackCue.CartPlayer,pedestrian.transform.position+Vector3.up*.65f,delta.normalized,Mathf.Clamp01(Mathf.Abs(state.speed)/20),cart.Owner,GolfCartWorld.Key(pedestrian));}else if(!pedestrian)cart.AudioBump(Mathf.Abs(state.speed));}state.speed=0;}
    var candidate=state.position+(delta.sqrMagnitude>.000001f?delta.normalized*travel:Vector3.zero);
    if(Surface(candidate,yaw,out var supported,out var rotation)&&Mathf.Abs(supported.y-state.position.y)<.38f&&Clear(supported,rotation,cart)){
     state.position=supported;state.rotation=rotation;
    }else state.speed=0;
   }
  }
  // Recoil uses the same bounded sweep and support checks as ordinary driving.
  // It cannot push a cart through rocks, a second cart or off the island.
  public static bool Recoil(GolfCart cart,ref GolfCartState state,Vector3 delta){
   float yaw=Heading(state.rotation);int steps=Mathf.Max(1,Mathf.CeilToInt(delta.magnitude/.10f));
   var part=delta/steps;bool moved=false;
   for(int i=0;i<steps;i++){
    float travel=Distance(cart,state.position,state.rotation,part,out _,out _,out _);
    if(travel<.0001f)break;
    var candidate=state.position+part.normalized*travel;
    if(!Surface(candidate,yaw,out var at,out var rotation)||Mathf.Abs(at.y-state.position.y)>=.38f||!Clear(at,rotation,cart))break;
    state.position=at;state.rotation=rotation;moved=true;
   }
   return moved;
  }
  public static bool ExitClear(Vector3 position,Athlete athlete,GolfCart cart) {
   float radius=athlete.capsule?athlete.capsule.radius:.35f;
   float height=athlete.capsule?athlete.capsule.height:1.8f;
   int count=Physics.OverlapCapsuleNonAlloc(position+Vector3.up*(radius+.07f),position+Vector3.up*(height-radius+.07f),radius,overlaps,Obstacles,QueryTriggerInteraction.Ignore);
   for(int i=0;i<count;i++)if(!Terrain(overlaps[i])&&!Walkway(overlaps[i])&&!overlaps[i].transform.IsChildOf(athlete.transform))return false;
   return count<overlaps.Length;
  }
 }
}
