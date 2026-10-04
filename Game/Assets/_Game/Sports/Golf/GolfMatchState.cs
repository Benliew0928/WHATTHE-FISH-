using System;
using System.Collections.Generic;
using System.Linq;

namespace WhatTheFish {
 public enum GolfMatchPhase:byte { Idle, Playing, FinalCountdown, Ended }
 public enum GolfHoleResult:byte { Ignored, Invalid, Completed, Finished }
 public readonly struct GolfEntrant {
  public readonly ulong id;public readonly string name;
  public GolfEntrant(ulong id,string name){this.id=id;this.name=name;}
 }
 public readonly struct GolfResult {
  public readonly GolfPlayerState player;public readonly int rank;public readonly bool tie;
  public GolfResult(GolfPlayerState player,int rank,bool tie){this.player=player;this.rank=rank;this.tie=tie;}
 }
 // Pure match rules. There is deliberately no deadline before the first finisher.
 public sealed class GolfMatchState {
  public const double FinalCountdownSeconds=30;
  readonly Dictionary<ulong,GolfPlayerState> players=new();
  public IReadOnlyCollection<GolfPlayerState> Players=>players.Values;
  public GolfMatchPhase Phase {get;private set;}
  public double StartedAt {get;private set;}
  public double CountdownDeadline {get;private set;}
  public ulong FirstFinisher {get;private set;}=ulong.MaxValue;
  public bool Running=>Phase==GolfMatchPhase.Playing||Phase==GolfMatchPhase.FinalCountdown;
  public bool CountdownStarted=>FirstFinisher!=ulong.MaxValue;
  public GolfPlayerState Player(ulong id)=>players.TryGetValue(id,out var player)?player:null;
  public bool Start(IEnumerable<GolfEntrant> roster,double now){
   if(Running||!double.IsFinite(now))return false;
   var entrants=roster?.ToArray();if(entrants==null||entrants.Length==0||entrants.Any(p=>p.id==ulong.MaxValue)||entrants.Select(p=>p.id).Distinct().Count()!=entrants.Length)return false;
   Clear();foreach(var p in entrants)players.Add(p.id,new GolfPlayerState(p.id,p.name));StartedAt=now;Phase=GolfMatchPhase.Playing;return true;
  }
  public bool RecordSwing(ulong hitter,double now){Advance(now);return Running&&double.IsFinite(now)&&now>=StartedAt&&Player(hitter)!=null&&Player(hitter).Stroke();}
  public GolfHoleResult EnterHole(ulong owner,int hole,double now){
   Advance(now);var player=Player(owner);
   if(!Running||!double.IsFinite(now)||now<StartedAt||player==null||player.IsFinished||player.IsDNF||hole<1||hole>GolfPlayerState.HoleCount)return GolfHoleResult.Ignored;
   if(hole!=player.CurrentHole)return GolfHoleResult.Invalid;
   player.Complete(hole,now-StartedAt);
   if(!player.IsFinished)return GolfHoleResult.Completed;
   if(!CountdownStarted){FirstFinisher=owner;CountdownDeadline=now+FinalCountdownSeconds;Phase=GolfMatchPhase.FinalCountdown;}
   if(players.Values.All(p=>p.IsFinished))Phase=GolfMatchPhase.Ended;
   return GolfHoleResult.Finished;
  }
  public void Advance(double now){
   if(Phase!=GolfMatchPhase.FinalCountdown||!double.IsFinite(now)||now<CountdownDeadline)return;
   foreach(var p in players.Values)p.DidNotFinish();Phase=GolfMatchPhase.Ended;
  }
  // Bound display time as well as the deadline: floating-point subtraction
  // must never turn the initial 30 seconds into a visible 00:31.
  public double Remaining(double now)=>CountdownStarted?Math.Min(FinalCountdownSeconds,Math.Max(0,CountdownDeadline-now)):0;
  public GolfResult[] Results(){
   // DNF stays in roster order; only finishers are ranked by strokes, then time.
   var ordered=players.Values.Where(p=>p.IsFinished).OrderBy(p=>p.TotalStroke).ThenBy(p=>p.FinishTime).Concat(players.Values.Where(p=>!p.IsFinished)).ToArray();
   var results=new GolfResult[ordered.Length];int rank=0;
   for(int i=0;i<ordered.Length;i++){
    var p=ordered[i];bool same=i>0&&SameScore(p,ordered[i-1]);if(!same)rank=i+1;
    bool tie=p.IsFinished&&(same||i+1<ordered.Length&&SameScore(p,ordered[i+1]));
    results[i]=new GolfResult(p,p.IsFinished?rank:0,tie);
   }
   return results;
  }
  static bool SameScore(GolfPlayerState a,GolfPlayerState b)=>a.IsFinished&&b.IsFinished&&a.TotalStroke==b.TotalStroke&&a.FinishTime==b.FinishTime;
  public void Clear(){players.Clear();Phase=GolfMatchPhase.Idle;StartedAt=CountdownDeadline=0;FirstFinisher=ulong.MaxValue;}
  internal void Receive(GolfMatchSnapshot match,IEnumerable<(ulong id,GolfPlayerSnapshot state)> roster){
   Clear();Phase=match.phase;StartedAt=match.started;CountdownDeadline=match.deadline;FirstFinisher=match.firstFinisher;
   foreach(var item in roster){if(!item.state.valid||item.state.round!=match.round)continue;var p=new GolfPlayerState(item.id,item.state.name.ToString());p.Receive(item.state);players.Add(item.id,p);}
  }
 }
}
