using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace WhatTheFish {
 public sealed partial class BasketballBall {
  public const float OutOfBoundsSeconds=2;
  Athlete inboundActor;Vector3 boundaryPrevious,boundaryExit;bool boundarySampled,inboundFlight;double inboundReleased;
  public bool InboundPending=>Authority?inboundActor&&holder==inboundActor:haveTarget&&target.inbound&&target.held;
  public bool IsInbound(Athlete actor)=>Playing&&actor&&InboundPending&&Holder==actor;
  public Vector3 InboundExit=>transform.parent?transform.parent.TransformPoint(boundaryExit):boundaryExit;
  Vector3 CourtPoint(Vector3 world)=>transform.parent?transform.parent.InverseTransformPoint(world):world;
  bool OutsidePaint(Vector3 point)=>Mathf.Abs(point.x)>PaintedCourtLimits.x||Mathf.Abs(point.z)>PaintedCourtLimits.y;
  void ResetBoundaryTracking(){boundarySampled=inboundFlight=false;outsideFor=0;inboundActor=null;}
  // This is a planar rules boundary, independent of the visual fabric's height.
  // Preserve the first crossing even if the ball rolls far away or lands on a stand.
  Vector3 ExitPoint(Vector3 from,Vector3 to){
   var limit=PaintedCourtLimits;float first=2;
   void Plane(float a,float b,float edge,bool x){
    if(Mathf.Abs(b-a)<.00001f)return;float t=(edge-a)/(b-a);if(t<0||t>1||t>=first)return;
    var p=Vector3.LerpUnclamped(from,to,t);if(Mathf.Abs(x?p.z:p.x)>(x?limit.y:limit.x)+.001f)return;first=t;
   }
   if(!OutsidePaint(from)){Plane(from.x,to.x,-limit.x,true);Plane(from.x,to.x,limit.x,true);Plane(from.z,to.z,-limit.y,false);Plane(from.z,to.z,limit.y,false);}
   var point=first<=1?Vector3.LerpUnclamped(from,to,first):new Vector3(Mathf.Clamp(to.x,-limit.x,limit.x),0,Mathf.Clamp(to.z,-limit.y,limit.y));
   // Recovery of an in-court obstruction uses its closest touchline.
   if(Mathf.Abs(point.x)<limit.x-.001f&&Mathf.Abs(point.z)<limit.y-.001f){if(limit.x-Mathf.Abs(point.x)<limit.y-Mathf.Abs(point.z))point.x=(point.x<0?-1:1)*limit.x;else point.z=(point.z<0?-1:1)*limit.y;}
   point.y=.07f;return point;
  }
  bool StepBoundaryRecovery(){
   var point=CourtPoint(Body.position);bool outside=OutsidePaint(point);
   // The legal launch begins beyond the line; allow it to enter the court.
   if(inboundFlight){if(!outside)inboundFlight=false;else if(BasketballMotion.Clock-inboundReleased<.7)return true;else inboundFlight=false;}
   if(outside&&BasketballMatch.Instance&&BasketballMatch.Instance.Competitive){boundaryExit=ExitPoint(boundarySampled?boundaryPrevious:point,point);return BasketballMatch.Instance.Boundary(boundaryExit);}
   if(outside){if(outsideFor==0){boundaryExit=ExitPoint(boundarySampled?boundaryPrevious:point,point);BasketballAudio.Emit(BasketballSound.Boundary,InboundExit);}outsideFor+=Time.fixedDeltaTime;}
   else outsideFor=0;
   boundaryPrevious=point;boundarySampled=true;looseFor+=Time.fixedDeltaTime;
   if(outsideFor>=OutOfBoundsSeconds){RecoverAtBoundary();return true;}
   if(looseFor>=strandedTimeout&&Body.linearVelocity.sqrMagnitude<.25f&&point.y>pickupHeight){boundaryExit=ExitPoint(point,point);if(!BasketballMatch.Instance||!BasketballMatch.Instance.Boundary(boundaryExit,true))RecoverAtBoundary();return true;}
   return outside; // No collecting an out-of-bounds ball through the wall.
  }
  void RecoverAtBoundary(){
   // Practice restart only. Competitive restarts are handled by BasketballMatch.
   // A host testing alone uses the same rule as offline practice.
   var actor=AppRoot.Instance?AppRoot.Instance.LocalAthlete:null;
   if(Connected&&NetworkManager.Singleton.ConnectedClients.Count!=1||!Eligible(actor)){ResetHome();return;}
   BeginInbound(actor,boundaryExit);
  }
  public bool BeginInbound(Athlete actor,Vector3 exit){
   if(!Authority||!actor||!actor.capsule)return false;
   var limits=PaintedCourtLimits;
   bool side=Mathf.Abs(exit.x)>=limits.x-.001f;
   var outward=side?new Vector3(Mathf.Sign(exit.x),0,0):new Vector3(0,0,Mathf.Sign(exit.z));
   float clearance=actor.capsule.radius+actor.capsule.skinWidth+.12f;
   var spot=exit+outward*clearance;var tangent=side?Vector3.forward:Vector3.right;
   float span=side?limits.y:limits.x;bool found=false;Vector3 world=default;
   // Basket bases may occupy the crossing point. Use the nearest clear spot
   // along that same touchline, never warp into the support or across the court.
   for(int i=0;i<100;i++){
    float shift=i==0?0:((i+1)/2)*.35f*(i%2==1?1:-1);
    var candidate=spot+tangent*shift;if(Mathf.Abs(side?candidate.z:candidate.x)>span-.55f)continue;
    world=transform.parent?transform.parent.TransformPoint(candidate):candidate;
    float radius=actor.capsule.radius+.04f,height=actor.capsule.height;
    var center=world+actor.capsule.center;
    if(Physics.CheckCapsule(center+Vector3.up*(height*.5f-radius),center-Vector3.up*(height*.5f-radius),radius,1<<8,QueryTriggerInteraction.Ignore))continue;
    found=true;break;
   }
   if(!found)return false; // Retry while retaining the original exit; do not lose the ball.
   ClearPossession();boundaryExit=exit;NewDefensePlay();inboundActor=actor;
   if(PlayerView.Instance&&PlayerView.Instance.target==actor)PlayerView.Instance.ClearMatchInput();
   var inward=transform.parent?transform.parent.TransformDirection(-outward):-outward;float heading=Quaternion.LookRotation(inward).eulerAngles.y;
   bool enabled=actor.capsule.enabled;actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(world,Quaternion.Euler(0,heading,0));actor.capsule.enabled=enabled;actor.ResetLocomotion();
   var net=actor.GetComponent<NetworkTransform>();if(net&&net.IsSpawned)net.Teleport(world,actor.transform.rotation,actor.transform.localScale);
   if(PlayerView.Instance&&PlayerView.Instance.target==actor)PlayerView.Instance.yaw=heading;
   holder=actor;pendingAction=BasketballAction.None;dribblePhase=0;looseFor=outsideFor=0;
   Body.isKinematic=false;Body.linearVelocity=Body.angularVelocity=Vector3.zero;Body.isKinematic=true;Body.detectCollisions=false;
   actor.BasketballMotion.Begin(BasketballAction.PassCharge,heading,BasketballMotion.PassPocket);
   Follow(actor);BasketballAudio.Emit(BasketballSound.Inbound,Body.position);reset++;sendAt=0;return true;
  }
  void StepInboundHold(){
   if(!IsInbound(holder))return;
   var motion=holder.BasketballMotion;
   if(pendingAction==BasketballAction.None&&!motion.Busy&&!motion.PassCharging)motion.Begin(BasketballAction.PassCharge,holder.transform.eulerAngles.y,BasketballMotion.PassPocket);
  }
 }
}
