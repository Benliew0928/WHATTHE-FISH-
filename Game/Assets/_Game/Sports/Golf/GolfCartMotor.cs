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
  static readonly Vector3[] wheels={new(-.54f,0,-.94f),new(.54f,0,-.94f),new(-.54f,0,.94f),new(.54f,0,.94f)};
  public static bool Terrain(Collider collider)=>collider&&collider.name.StartsWith("Terrain__");
  public static float Heading(Quaternion rotation)=>
   (Quaternion.Inverse(Quaternion.FromToRotation(Vector3.up,rotation*Vector3.up))*rotation).eulerAngles.y;
  public static bool Floor(Vector3 at,out RaycastHit floor) {
   floor=default;float best=float.MaxValue;
   int count=Physics.RaycastNonAlloc(at+Vector3.up*12,Vector3.down,casts,30,1<<8,QueryTriggerInteraction.Ignore);
   for(int i=0;i<count;i++)if(Terrain(casts[i].collider)&&casts[i].distance<best){floor=casts[i];best=floor.distance;}
   var island=RefinedIslandEnvironment.Active;
   return best<float.MaxValue&&floor.normal.y>.80f&&(!island||floor.point.y>island.layout.sea_level+.12f);
  }
  public static bool Surface(Vector3 position,float yaw,out Vector3 supported,out Quaternion rotation) {
   supported=position;rotation=Quaternion.Euler(0,yaw,0);var normal=Vector3.zero;float height=0;int contacts=0;
   foreach(var wheel in wheels){
    if(!Floor(position+rotation*wheel,out var hit))continue;
    normal+=hit.normal;height+=hit.point.y;contacts++;
   }
   if(contacts<3)return false;
   normal.Normalize();if(normal.y<.82f)return false;
   supported.y=height/contacts;rotation=Quaternion.FromToRotation(Vector3.up,normal)*rotation;return true;
  }
  public static bool Clear(Vector3 position,Quaternion rotation,GolfCart self,Athlete ignored=null) {
   var center=position+rotation*new Vector3(0,.66f,0);
   int count=Physics.OverlapBoxNonAlloc(center,HalfBody,overlaps,rotation,Obstacles,QueryTriggerInteraction.Ignore);
   for(int i=0;i<count;i++){
    var collider=overlaps[i];if(Terrain(collider)||self&&collider.transform.IsChildOf(self.transform)||ignored&&collider.transform.IsChildOf(ignored.transform))continue;
    return false;
   }
   return count<overlaps.Length;
  }
  static float Distance(GolfCart self,Vector3 position,Quaternion rotation,Vector3 delta) {
   if(delta.sqrMagnitude<.0000001f)return 0;
   float distance=delta.magnitude,allowed=distance;
   int count=Physics.BoxCastNonAlloc(position+rotation*new Vector3(0,.66f,0),HalfBody,delta/distance,casts,rotation,distance+.04f,Obstacles,QueryTriggerInteraction.Ignore);
   for(int i=0;i<count;i++){
    // Wheel support below validates terrain height, slope and shore clearance.
    // A level body sweep can start inside an uphill terrain triangle; PhysX
    // reports that overlap with a horizontal normal, falsely making it a wall.
    var hit=casts[i];if(Terrain(hit.collider)||self&&hit.collider.transform.IsChildOf(self.transform)||hit.normal.y>.55f)continue;
    allowed=Mathf.Min(allowed,Mathf.Max(0,hit.distance-.04f));
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
    float wanted=throttle*(throttle<0?ReverseSpeed:ForwardSpeed);
    float rate=throttle==0||state.speed*wanted<0?Brake:Acceleration;
    state.speed=Mathf.MoveTowards(state.speed,wanted,rate*step);
    // Remove terrain tilt before extracting steering yaw; Euler Y alone would
    // repeatedly add slope roll/pitch and turn an idle cart without stick input.
    float yaw=Heading(state.rotation);
    if(Mathf.Abs(state.speed)>.08f)yaw+=input.x*Steering*Mathf.Clamp01(Mathf.Abs(state.speed)/2)*Mathf.Sign(state.speed)*step;
    var facing=Quaternion.Euler(0,yaw,0);var delta=facing*Vector3.forward*(state.speed*step);
    float travel=Distance(cart,state.position,facing,delta);
    if(travel+.0001f<delta.magnitude)state.speed=0;
    var candidate=state.position+(delta.sqrMagnitude>.000001f?delta.normalized*travel:Vector3.zero);
    if(Surface(candidate,yaw,out var supported,out var rotation)&&Mathf.Abs(supported.y-state.position.y)<.38f&&Clear(supported,rotation,cart)){
     state.position=supported;state.rotation=rotation;
    }else state.speed=0;
   }
  }
  public static bool ExitClear(Vector3 position,Athlete athlete,GolfCart cart) {
   float radius=athlete.capsule?athlete.capsule.radius:.35f;
   float height=athlete.capsule?athlete.capsule.height:1.8f;
   int count=Physics.OverlapCapsuleNonAlloc(position+Vector3.up*(radius+.07f),position+Vector3.up*(height-radius+.07f),radius,overlaps,Obstacles,QueryTriggerInteraction.Ignore);
   for(int i=0;i<count;i++)if(!Terrain(overlaps[i])&&!overlaps[i].transform.IsChildOf(athlete.transform))return false;
   return count<overlaps.Length;
  }
 }
}
