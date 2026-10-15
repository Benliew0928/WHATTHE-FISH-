using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 internal static class RoomGameRecovery {
  public static void Restore(){
   var host=NetworkAthlete.HostPlayer;if(!host||!host.Exploring.Value)return;
   switch(host.WorldSport.Value){
    case SportId.Football:FootballMatch.Instance?.RestoreRoom();break;
    case SportId.Basketball:BasketballMatch.Instance?.RestoreRoom();break;
    case SportId.Golf:GolfMatchManager.Instance?.RestoreRoom();break;
    case SportId.Fishing:FishingGame.Instance?.RestoreRoom();break;
   }
  }
 }
 public sealed partial class FootballMatch {
  internal void RestoreRoom(){
   var host=NetworkAthlete.HostPlayer;var s=host.Match.Value;
   State.Restore(s,MenuMatchRules.RegulationSeconds(rules.regulationSeconds),rules.overtimeSeconds,rules.kickoffSeconds);
   participants.Clear();foreach(var a in Eligible())participants[PlayerId(a)]=a;
   Selection.Restore(participants.Select(p=>(p.Key,p.Value.GetComponent<NetworkAthlete>().Team.Value)),s.teamCapacity,s.phase!=FootballMatchPhase.TeamSelection);
   Statistics.Clear();foreach(var p in host.FootballPlayers)Statistics.Players[p.id]=p;
   publishedRevision=s.revision;nextRoundRevision=Math.Max(nextRoundRevision,s.revision);observedRuleRevision=State.Revision;publishedPhase=State.Phase;
   goalAt=s.goalAt;goalId=s.goalId;kickoffTeam=s.kickoffTeam;kickoffPlayer=s.kickoffPlayer;wasContext=true;
   goals=new FootballGoalDetector[ball.GoalCount];
   for(int i=0;i<goals.Length;i++){int sign=ball.GoalSign(i);float edge=(sign>0?ball.PitchBounds.max.z:ball.PitchBounds.min.z)+sign*rules.goalLineWidth*.5f;goals[i]=new FootballGoalDetector(ball.GoalBounds(i),sign*Mathf.Max(sign*edge,sign*ball.GoalFront(i)),sign);}
   ball.RestoreRoom(host.Ball.Value);
   if(State.Phase==FootballMatchPhase.GoalReset){ClearGoalSequence();State.ResetCompleted(Now);}
   if(State.Phase==FootballMatchPhase.Kickoff&&!participants.ContainsKey(kickoffPlayer))ChooseKickoff();
  }
 }
 public sealed partial class FootballBall {
  internal void RestoreRoom(FootballBallSnapshot s){playing=true;authority=true;received=s;sequence=s.sequence;footballProps=s.props;ReleaseControl();if(s.valid)SnapPose(s.position,s.rotation);}
 }
 public sealed partial class BasketballMatch {
  internal void RestoreRoom(){
   var host=NetworkAthlete.HostPlayer;var s=host.BasketballMatch.Value;State.Restore(s);
   participants.Clear();foreach(var a in Roster())participants[PlayerId(a)]=a;
   Selection.Restore(participants.Select(p=>(p.Key,p.Value.GetComponent<NetworkAthlete>().BasketballTeam.Value)),s.capacity,s.phase!=BasketballMatchPhase.TeamSelection);
   Statistics.Clear();foreach(var p in host.BasketballPlayers)Statistics.Players[p.id]=p;
   observed=State.Revision;published=s.revision;nextRevision=Math.Max(nextRevision,s.revision);presented=s.phase;wasContext=true;
   ball.RestoreRoom(host.Basketball.Value);
  }
 }
 public sealed partial class BasketballBall {
  internal void RestoreRoom(BasketballSnapshot s){
   wasConnected=wasAuthority=true;wasPlaying=Playing;ClearPossession();if(s.valid)Place(s.position,s.rotation,Vector3.zero,Vector3.zero);score=s.score;reset=s.reset+1;
  }
 }
 public sealed partial class GolfMatchManager {
  internal void RestoreRoom(){Receive();foreach(var n in RoomTeleport.Players())GolfCartWorld.Receive(n.GetComponent<Athlete>(),n.Cart.Value);nextRound=Math.Max(nextRound,Round);wasContext=true;foreach(var net in RoomTeleport.Players())if(balls.TryGetValue(net.StableId,out var ball)&&net.GolfBall.Value.valid)ball.RestoreRoom(net.GolfBall.Value);ApplyLive();}
 }
 public sealed partial class FishingGame {
  internal void RestoreRoom(){var host=NetworkAthlete.HostPlayer;State.Receive(host.FishingMatch.Value,host.FishingPlayers,host.FishingFish);nextRound=Math.Max(nextRound,State.Match.round);wasContext=true;}
 }
}
