using UnityEngine;

namespace WhatTheFish {
 public sealed partial class BasketballMatch {
  // Radius is one quarter of the full painted court length, plus each
  // defender's body clearance so their capsule also stays outside the zone.
  public float InboundClearanceRadius=>BasketballBall.PaintedCourtLimits.y*.5f*ball.transform.parent.TransformVector(Vector3.forward).magnitude;
  float DefenderClearance(Athlete actor)=>InboundClearanceRadius+actor.capsule.radius+actor.capsule.skinWidth;
  public bool TryInboundClearance(out Vector3 center){
   center=default;if(!Context||Snapshot.phase!=BasketballMatchPhase.Restart)return false;
   var state=Snapshot;var passer=TeammatePassing.Find(state.inbounder);if(!passer)return false;
   if(ball.IsInbound(passer)){center=passer.transform.position;return true;}
   if(Authority&&automaticMoves.TryGetValue(state.inbounder,out var move)){center=move.destination;return true;}
   return ball.TryInboundSpot(passer,state.inboundExit,out center,out _);
  }
  bool ClearanceOpponent(Athlete actor)=>actor&&actor.isActiveAndEnabled&&!actor.inTransit&&!actor.IsSpectator&&actor.capsule&&TeamOf(actor)!=FootballTeam.None&&TeamOf(actor)!=Snapshot.possession;
  public bool InboundSpaceClear {
   get {if(!TryInboundClearance(out var center))return true;foreach(var actor in Athlete.Active)if(ClearanceOpponent(actor)&&Vector3.ProjectOnPlane(actor.transform.position-center,Vector3.up).magnitude<DefenderClearance(actor)-.02f)return false;return true;}
  }
  bool ClearanceDestination(Athlete actor,Vector3 center,out Vector3 destination){
   destination=default;float best=float.PositiveInfinity,radius=DefenderClearance(actor)+.35f;
   var arena=ball.transform.parent;var limits=BasketballBall.PlayerCourtLimits-Vector2.one*(actor.capsule.radius+actor.capsule.skinWidth+.1f);
   // Choose the nearest legal point on the in-court part of the circle.
   // Sampling also handles corners and hoop supports without pushing a player
   // toward the out-of-court wall.
   for(int i=0;i<64;i++){
    float angle=i*Mathf.PI/32;var point=center+new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle))*radius;var local=arena.InverseTransformPoint(point);
    if(Mathf.Abs(local.x)>limits.x||Mathf.Abs(local.z)>limits.y||!Ground(local,out point))continue;
    var middle=point+actor.capsule.center;float bodyRadius=actor.capsule.radius+.03f,half=actor.capsule.height*.5f-bodyRadius;
    if(Physics.CheckCapsule(middle+Vector3.up*half,middle-Vector3.up*half,bodyRadius,1<<8,QueryTriggerInteraction.Ignore))continue;
    float distance=(point-actor.transform.position).sqrMagnitude;if(distance>=best)continue;best=distance;destination=point;
   }
   return !float.IsPositiveInfinity(best);
  }
  void EnsureInboundClearance(){
   if(!TryInboundClearance(out var center))return;
   foreach(var actor in participants.Values){
    if(!ClearanceOpponent(actor)||AutomaticMovement(actor)||Vector3.ProjectOnPlane(actor.transform.position-center,Vector3.up).magnitude>=DefenderClearance(actor))continue;
    if(ClearanceDestination(actor,center,out var destination))StartAutomaticMove(actor,destination,false,true);
   }
  }
  public Vector3 ConstrainInboundInput(Athlete actor,Vector3 input){
   if(!ClearanceOpponent(actor)||!TryInboundClearance(out var center))return input;
   var offset=Vector3.ProjectOnPlane(actor.transform.position-center,Vector3.up);
   if(offset.magnitude>DefenderClearance(actor)+.03f||offset.sqrMagnitude<.001f)return input;
   var normal=offset.normalized;float inward=Vector3.Dot(input,normal);return inward<0?input-normal*inward:input;
  }
  public Vector3 ConstrainInboundMotion(Athlete actor,Vector3 displacement){
   if(!ClearanceOpponent(actor)||!TryInboundClearance(out var center))return displacement;
   var offset=Vector3.ProjectOnPlane(actor.transform.position-center,Vector3.up);var flat=Vector3.ProjectOnPlane(displacement,Vector3.up);
   float radius=DefenderClearance(actor),length=flat.sqrMagnitude,c=offset.sqrMagnitude-radius*radius,b=Vector3.Dot(offset,flat);
   // Existing occupants leave via normal automatic movement, never a teleport.
   if(c<0)return displacement;
   if(length<.000001f||b>=0||b*b-length*c<0)return displacement;
   float contact=(-b-Mathf.Sqrt(Mathf.Max(0,b*b-length*c)))/length;
   if(contact<0||contact>1)return displacement;
   var normal=(offset+flat*contact).normalized;actor.Motor.Constrain(normal);
   flat*=Mathf.Max(0,contact-.001f);return flat+Vector3.up*displacement.y;
  }
 }
}
