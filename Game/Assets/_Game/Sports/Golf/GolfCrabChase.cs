using UnityEngine;
namespace WhatTheFish {
 public sealed partial class GolfHazardWorld {
  bool crabAuthority;double nextCrabPublish;
  bool CrabCooling(ulong owner)=>bumped.TryGetValue(owner,out double at)&&match.Now-at<GolfHazardRules.CrabPushCooldown;
  GolfBall CrabTarget(Lane lane,ulong owner){
   var ball=match.Ball(owner);if(!ball||!ball.Live||ball.Sinking||ball.HazardHeld||CrabCooling(owner)||!ClearCup(ball.transform.position)||Vector3.ProjectOnPlane(ball.transform.position-lane.centre,Vector3.up).sqrMagnitude>=256)return null;
   foreach(var p in match.State.Players)if(p.PlayerId==owner&&!p.IsFinished&&!p.IsDNF&&p.CurrentHole==lane.hole)return ball;return null;
  }
  void UpdateCrabs(GolfEventSnapshot e){
   var host=NetworkAthlete.HostPlayer;bool visible=e.kind==GolfEventKind.CrabCrossing&&(match.Events.WarningActive(match.Now)||match.Events.Active(match.Now)==GolfEventKind.CrabCrossing);
   if(!visible){crabAuthority=false;if(host&&host.IsServer){if(host.GolfCrabs.Count>0)host.GolfCrabs.Clear();if(host.GolfCrabCooldowns.Count>0)host.GolfCrabCooldowns.Clear();}return;}
   if(!crabAuthority){crabAuthority=true;if(host)foreach(var c in host.GolfCrabCooldowns)if(c.round==match.Round&&c.sequence==e.sequence)bumped[c.owner]=c.until-GolfHazardRules.CrabPushCooldown;}
   foreach(var lane in lanes){
    var s=lane.crab;
    if(s.round!=match.Round||s.sequence!=e.sequence){s=new GolfCrabSnapshot{lane=lane.id,round=match.Round,sequence=e.sequence,position=lane.centre+GolfHazardRules.Offset(e.kind,e,lane.id,match.Now,lane.forward),time=match.Now};if(Ground(s.position,out var spawn))s.position=spawn;else s.position=lane.centre;if(host)foreach(var saved in host.GolfCrabs)if(saved.lane==lane.id&&saved.round==match.Round&&saved.sequence==e.sequence){s=saved;break;}}
    var patrol=lane.centre+GolfHazardRules.Offset(e.kind,e,lane.id,match.Now,lane.forward);bool active=match.Now>=e.start;GolfBall target=null;
    if(active&&match.Now>=s.retreatUntil){
     if(s.hasTarget)target=CrabTarget(lane,s.target);
     if(!target){s.hasTarget=false;float best=float.PositiveInfinity;ulong tie=ulong.MaxValue;foreach(var player in match.State.Players){var candidate=CrabTarget(lane,player.PlayerId);if(!candidate)continue;float d=(candidate.transform.position-s.position).sqrMagnitude;if(d<best||d==best&&player.PlayerId<tie){target=candidate;best=d;tie=player.PlayerId;}}if(target){s.hasTarget=true;s.target=target.Owner;}}
    }else s.hasTarget=false;
    var desired=match.Now<s.retreatUntil?s.retreatGoal:target?target.transform.position+new Vector3(Mathf.Cos(lane.id*2.399963f),0,Mathf.Sin(lane.id*2.399963f))*.6f:patrol;desired.y=lane.centre.y;
    var before=s.position;var next=Vector3.MoveTowards(before,desired,5*Time.fixedDeltaTime);if(ClearCup(next)&&Ground(next,out var dry))s.position=dry;
    s.velocity=(s.position-before)/Mathf.Max(.0001f,Time.fixedDeltaTime);s.time=match.Now;lane.crab=s;
   }
   if(host&&host.IsServer&&match.Now>=nextCrabPublish){nextCrabPublish=match.Now+.1;while(host.GolfCrabs.Count>lanes.Count)host.GolfCrabs.RemoveAt(host.GolfCrabs.Count-1);for(int i=0;i<lanes.Count;i++){if(i>=host.GolfCrabs.Count)host.GolfCrabs.Add(lanes[i].crab);else host.GolfCrabs[i]=lanes[i].crab;}}
  }
  void RetreatCrab(Lane lane,GolfBall ball,Vector3 push){
   var s=lane.crab;s.hasTarget=false;s.velocity=Vector3.zero;s.retreatUntil=match.Now+.5;s.retreatGoal=lane.centre;
   foreach(float angle in new[]{0f,45f,-45f,90f,-90f}){var at=s.position+Quaternion.AngleAxis(angle,Vector3.up)*(-push)*2.5f;if(ClearCup(at)&&Ground(at,out var dry)){s.retreatGoal=dry;break;}}lane.crab=s;
   var host=NetworkAthlete.HostPlayer;if(!host||!host.IsServer)return;var cooldown=new GolfCrabCooldown{owner=ball.Owner,round=match.Round,sequence=match.Events.Snapshot.sequence,until=match.Now+GolfHazardRules.CrabPushCooldown};for(int i=0;i<host.GolfCrabCooldowns.Count;i++)if(host.GolfCrabCooldowns[i].owner==ball.Owner){host.GolfCrabCooldowns[i]=cooldown;return;}host.GolfCrabCooldowns.Add(cooldown);
  }
  void ReceiveCrabs(GolfEventSnapshot e){
   crabAuthority=false;var host=NetworkAthlete.HostPlayer;if(!host||e.kind!=GolfEventKind.CrabCrossing)return;
   foreach(var state in host.GolfCrabs)if(state.round==match.Round&&state.sequence==e.sequence)foreach(var lane in lanes)if(lane.id==state.lane){lane.crab=state;break;}
  }
  Vector3 RenderCrab(Lane lane,GolfEventSnapshot e){
   var s=lane.crab;if(s.sequence!=e.sequence||s.round!=match.Round)return Position(lane,e,match.Now);
   if(lane.crabRenderSequence!=e.sequence){lane.crabRender=s.position;lane.crabRenderVelocity=Vector3.zero;lane.crabRenderSequence=e.sequence;}
   var desired=s.position+s.velocity*Mathf.Clamp((float)(match.Now-s.time),0,.1f);lane.crabRender=Vector3.SmoothDamp(lane.crabRender,desired,ref lane.crabRenderVelocity,.08f,float.PositiveInfinity,Time.unscaledDeltaTime);return lane.crabRender;
  }
 }
}
