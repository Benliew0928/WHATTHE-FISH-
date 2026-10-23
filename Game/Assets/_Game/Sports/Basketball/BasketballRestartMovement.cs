using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace WhatTheFish {
 public sealed partial class BasketballMatch {
  sealed class RestartMove {
   public Athlete actor;public Vector3 destination,approach,detour;public bool inbound,clearance,approaching,avoiding,passingPlayers;
   public double avoidUntil,progressAt;public float bestDistance;
   public readonly List<Collider> ignoredPlayers=new();
  }
  const double InboundStallSeconds=1.5;
  readonly Dictionary<ulong,RestartMove> automaticMoves=new();
  readonly Dictionary<FootballTeam,int> inboundTurns=new();
  ulong inbounder=ulong.MaxValue;Vector3 inboundExit;bool localAutomatic;
  int InboundTurn(FootballTeam team)=>inboundTurns.TryGetValue(team,out var turn)?turn:0;
  FixedList128Bytes<ulong> AutomaticPlayers(){var ids=new FixedList128Bytes<ulong>();foreach(var id in automaticMoves.Keys)ids.Add(id);return ids;}
  public bool AutomaticMovement(Athlete actor)=>Context&&Active&&actor&&(Authority?automaticMoves.ContainsKey(PlayerId(actor)):Snapshot.IsAutomatic(PlayerId(actor)));
  public bool RetrievingInbound(Athlete actor)=>AutomaticMovement(actor)&&PlayerId(actor)==Snapshot.inbounder;
  public string MovementHint(Athlete actor)=>!AutomaticMovement(actor)?"":RetrievingInbound(actor)?"AUTO-MOVING TO INBOUND\nPicking up the ball outside the court":Snapshot.phase==BasketballMatchPhase.Restart&&Snapshot.restart!=BasketballRestart.Basket?"GIVING INBOUND SPACE\nMoving outside the protected passing area":"AUTO-MOVING TO YOUR HALF\nControls return when you reach your half";
  void UpdateAutomaticInput(){
   if(!Authority&&automaticMoves.Count>0)ResetRestartMovement();
   bool current=AppRoot.Instance&&AutomaticMovement(AppRoot.Instance.LocalAthlete);
   if(current!=localAutomatic){PlayerView.Instance?.ClearMatchInput();localAutomatic=current;}
  }
  void RestoreRestartCollisions(RestartMove move){
   if(move.actor&&move.actor.capsule)foreach(var other in move.ignoredPlayers)if(other)Physics.IgnoreCollision(move.actor.capsule,other,false);
   move.ignoredPlayers.Clear();
  }
  void ResetRestartMovement(){foreach(var move in automaticMoves.Values){RestoreRestartCollisions(move);if(move.actor)move.actor.ResetLocomotion();}automaticMoves.Clear();inbounder=ulong.MaxValue;}
  void StartAutomaticMove(Athlete actor,Vector3 destination,bool inbound,bool clearance=false){
   actor.ResetLocomotion();actor.GetComponent<NetworkAthlete>()?.ClearMatchInput();
   if(PlayerView.Instance&&PlayerView.Instance.target==actor)PlayerView.Instance.ClearMatchInput();
   var approach=destination;
   if(inbound){var local=ball.transform.parent.InverseTransformPoint(destination);var limit=BasketballBall.PaintedCourtLimits;local.x=Mathf.Clamp(local.x,-limit.x+1.2f,limit.x-1.2f);local.z=Mathf.Clamp(local.z,-limit.y+1.2f,limit.y-1.2f);approach=ball.transform.parent.TransformPoint(local);}
   automaticMoves.Add(PlayerId(actor),new RestartMove{actor=actor,destination=destination,approach=approach,inbound=inbound,clearance=clearance,approaching=inbound,progressAt=Now,bestDistance=Vector3.ProjectOnPlane(approach-actor.transform.position,Vector3.up).magnitude});
  }
  void StepRestartMovement(){
   foreach(var pair in automaticMoves.ToArray()){
    var move=pair.Value;var actor=move.actor;if(!actor){RestoreRestartCollisions(move);automaticMoves.Remove(pair.Key);continue;}
    if(move.approaching&&Vector3.ProjectOnPlane(move.approach-actor.transform.position,Vector3.up).magnitude<.22f){move.approaching=false;move.bestDistance=float.PositiveInfinity;move.progressAt=Now;}
    bool arrived=Vector3.ProjectOnPlane(move.destination-actor.transform.position,Vector3.up).magnitude<.12f;
    if(move.clearance)arrived=!TryInboundClearance(out var center)||Vector3.ProjectOnPlane(actor.transform.position-center,Vector3.up).magnitude>=DefenderClearance(actor)+.08f;
    else if(!move.inbound){float home=TeamOf(actor)==FootballTeam.A?-1:1;arrived=ball.transform.parent.InverseTransformPoint(actor.transform.position).z*home>=actor.capsule.radius+actor.capsule.skinWidth+.15f;}
    if(!arrived)continue;
    if(move.inbound&&!ball.BeginInbound(actor,inboundExit,false))continue;
    RestoreRestartCollisions(move);actor.ResetLocomotion();actor.GetComponent<NetworkAthlete>()?.ClearMatchInput();automaticMoves.Remove(pair.Key);
   }
   Publish();
  }
  void RestoreRestartMovement(BasketballMatchSnapshot state){
   ResetRestartMovement();inboundTurns[FootballTeam.A]=state.inboundTurnA;inboundTurns[FootballTeam.B]=state.inboundTurnB;inbounder=state.inbounder;inboundExit=state.inboundExit;
   if(!State.Active)return;
   foreach(var actor in participants.Values.Where(a=>state.IsAutomatic(PlayerId(a))&&PlayerId(a)!=inbounder)){
    if(state.phase==BasketballMatchPhase.Restart&&state.restart!=BasketballRestart.Basket)continue; // Recomputed from the restored inbound below.
    var local=ball.transform.parent.InverseTransformPoint(actor.transform.position);local.z=TeamOf(actor)==FootballTeam.A?-2:2;
    if(Ground(local,out var destination))StartAutomaticMove(actor,destination,false);
   }
   if(State.Phase!=BasketballMatchPhase.Restart)return;
   if(!participants.TryGetValue(inbounder,out var inbound)){
    var team=participants.Values.Where(a=>TeamOf(a)==State.Possession).OrderBy(PlayerId).ToArray();if(team.Length==0){Cancel();return;}
    int turn=InboundTurn(State.Possession)%team.Length;inbound=team[turn];inbounder=PlayerId(inbound);inboundTurns[State.Possession]=(turn+1)%team.Length;
   }
   if(!ball.BeginInbound(inbound,inboundExit,false)){
    if(!ball.TryInboundSpot(inbound,inboundExit,out var spot,out _)){Cancel();return;}
    ball.Place(ball.transform.parent.InverseTransformPoint(spot)+Vector3.up*(BasketballBall.Radius+.02f),Quaternion.identity,Vector3.zero,Vector3.zero);StartAutomaticMove(inbound,spot,true);
   }
   EnsureInboundClearance();Publish();
  }
  // Replace all player/bot commands on the authority, so delayed RPCs and
  // held inputs cannot steer or interrupt these short individual runs.
  public bool AutomaticCommand(Athlete actor,out PlayerCommand command){
   command=default;if(!Authority||!AutomaticMovement(actor)||!automaticMoves.TryGetValue(PlayerId(actor),out var move))return false;
   var delta=Vector3.ProjectOnPlane((move.approaching?move.approach:move.destination)-actor.transform.position,Vector3.up);
   if(move.inbound||move.clearance){
    // Measure progress toward the actual waypoint, not distance travelled:
    // a detour can keep circling a player standing on the approach itself.
    if(delta.magnitude<move.bestDistance-.2f){move.bestDistance=delta.magnitude;move.progressAt=Now;}
    if(Now-move.progressAt>=InboundStallSeconds)move.passingPlayers=true;
    if(move.passingPlayers){
     move.avoiding=false;
     // Only this stalled automatic runner may pass through athlete bodies. Keep
     // ground, stands, hoop supports and every other collision unchanged.
     // Refresh for newly spawned/re-enabled bodies; preserve existing ignores.
     foreach(var other in Athlete.Active){
      if(!other||other==actor||!other.capsule||!other.capsule.enabled)continue;
      if(Physics.GetIgnoreCollision(actor.capsule,other.capsule))continue;
      if(!move.ignoredPlayers.Contains(other.capsule))move.ignoredPlayers.Add(other.capsule);
      Physics.IgnoreCollision(actor.capsule,other.capsule,true);
     }
    }
   }
   if(move.avoiding&&(Now>=move.avoidUntil||Vector3.ProjectOnPlane(move.detour-actor.transform.position,Vector3.up).magnitude<.25f))move.avoiding=false;
   if(!move.passingPlayers&&!move.avoiding&&delta.magnitude>.3f){
    var forward=delta.normalized;var side=Vector3.Cross(Vector3.up,forward);Athlete obstacle=null;float closest=float.PositiveInfinity;
    foreach(var other in participants.Values){
     if(other==actor||!other||!other.capsule||Mathf.Abs(other.transform.position.y-actor.transform.position.y)>1.2f)continue;
     var offset=Vector3.ProjectOnPlane(other.transform.position-actor.transform.position,Vector3.up);float along=Vector3.Dot(offset,forward),width=actor.capsule.radius+other.capsule.radius+.22f;
     if(along<0||along>Mathf.Min(delta.magnitude,1.6f)||Mathf.Abs(Vector3.Dot(offset,side))>width||along>=closest)continue;obstacle=other;closest=along;
    }
    if(obstacle){
     float offset=Vector3.Dot(obstacle.transform.position-actor.transform.position,side);float sign=Mathf.Abs(offset)<.08f?1:offset>0?-1:1;
     move.detour=obstacle.transform.position+side*sign*(actor.capsule.radius+obstacle.capsule.radius+.5f)+forward*.6f;
     var local=ball.transform.parent.InverseTransformPoint(move.detour);var limits=BasketballBall.PaintedCourtLimits-new Vector2(.8f,.8f);local.x=Mathf.Clamp(local.x,-limits.x,limits.x);local.z=Mathf.Clamp(local.z,-limits.y,limits.y);move.detour=ball.transform.parent.TransformPoint(local);
     move.avoiding=true;move.avoidUntil=Now+1.2;
    }
   }
   if(move.avoiding)delta=Vector3.ProjectOnPlane(move.detour-actor.transform.position,Vector3.up);
   command.heading=delta.sqrMagnitude>.0001f?Quaternion.LookRotation(delta).eulerAngles.y:actor.transform.eulerAngles.y;
   command.move=Vector2.up*Mathf.Clamp01(delta.magnitude/.65f);command.sprint=true;return true;
  }
 }
}
