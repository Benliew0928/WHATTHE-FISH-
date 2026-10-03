using System;
namespace WhatTheFish {
 public enum FootballTeam:byte { None, A, B }
 public enum FootballMatchPhase:byte { Idle=0, Kickoff=1, Regulation=2, GoalReset=3, Overtime=5, Finished=6, TeamSelection=7 }
 public enum FootballMatchResult:byte { None, TeamA, TeamB, Draw }
 // Pure rules and deadline clock. No scene, UI, animation or network dependency.
 public sealed class FootballMatchState {
  public FootballMatchPhase Phase {get;private set;}
  public FootballMatchResult Result {get;private set;}
  public int ScoreA {get;private set;} public int ScoreB {get;private set;}
  public double Deadline {get;private set;}
  double remaining,regular,overtime,kickoff;
  public uint Revision {get;private set;}
  public bool Running=>Phase==FootballMatchPhase.Regulation||Phase==FootballMatchPhase.Overtime;
  public double Remaining(double now)=>Running||Phase==FootballMatchPhase.Kickoff||Phase==FootballMatchPhase.TeamSelection?Math.Max(0,Deadline-now):remaining;
  public void Start(double now,double regulation,double extra,double countdown){
   regular=Math.Max(.01,regulation);overtime=Math.Max(.01,extra);kickoff=Math.Max(0,countdown);
   ScoreA=ScoreB=0;Result=FootballMatchResult.None;remaining=regular;Set(FootballMatchPhase.Kickoff,now+kickoff);
  }
  public void SelectTeams(double now,double seconds,double regulation,double extra,double countdown){Start(now,regulation,extra,countdown);Set(FootballMatchPhase.TeamSelection,now+Math.Max(0,seconds));}
  public void TeamsConfirmed(double now){if(Phase==FootballMatchPhase.TeamSelection)Set(FootballMatchPhase.Kickoff,now+kickoff);}
  void Set(FootballMatchPhase phase,double deadline){Phase=phase;Deadline=deadline;Revision++;}
  public void Clear(){ScoreA=ScoreB=0;Result=FootballMatchResult.None;remaining=0;Set(FootballMatchPhase.Idle,0);}
  public bool Goal(FootballTeam scoring,double crossedAt){
   if(!Running||crossedAt>=Deadline||scoring==FootballTeam.None)return false;
   if(scoring==FootballTeam.A)ScoreA++;else ScoreB++;
   remaining=Math.Max(0,Deadline-crossedAt);
   if(Phase==FootballMatchPhase.Overtime)Finish(scoring==FootballTeam.A?FootballMatchResult.TeamA:FootballMatchResult.TeamB);
   else Set(FootballMatchPhase.GoalReset,0);
   return true;
  }
  public void ResetCompleted(double now){if(Phase==FootballMatchPhase.GoalReset)Set(FootballMatchPhase.Kickoff,now+kickoff);}
  public void Advance(double now){
   if(now<Deadline)return;
   switch(Phase){
    case FootballMatchPhase.Kickoff:Set(FootballMatchPhase.Regulation,now+remaining);break;
    case FootballMatchPhase.Regulation:
     remaining=0;
     if(ScoreA!=ScoreB)Finish(ScoreA>ScoreB?FootballMatchResult.TeamA:FootballMatchResult.TeamB);
     else {remaining=overtime;Set(FootballMatchPhase.Overtime,Deadline+overtime);if(now>=Deadline){remaining=0;Finish(FootballMatchResult.Draw);}}
     break;
    case FootballMatchPhase.Overtime:remaining=0;Finish(FootballMatchResult.Draw);break;
   }
  }
  void Finish(FootballMatchResult result){Result=result;Set(FootballMatchPhase.Finished,0);}
 }
}
