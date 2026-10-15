using System;

namespace WhatTheFish {
 public enum BasketballMatchPhase:byte { Idle,TeamSelection,Countdown,Regulation,Restart,Overtime,Buzzer,Finished,Cancelled }
 public enum BasketballRestart:byte { None,Basket,Boundary,ShotClock,Recovery }

 // Match deadlines use playing time. Every restart preserves the remaining period.
 public sealed class BasketballMatchState {
  public const double ShotSeconds=20,RestartSeconds=2,OvertimeSeconds=60,FlightSeconds=8;
  public BasketballMatchPhase Phase {get;private set;}
  public FootballMatchResult Result {get;private set;}
  public FootballTeam Possession {get;private set;}
  public BasketballRestart RestartReason {get;private set;}
  public int ScoreA {get;private set;}
  public int ScoreB {get;private set;}
  public double Deadline {get;private set;}
  public double ShotDeadline {get;private set;}
  public uint Revision {get;private set;}
  double remaining,shotAt;bool extra,flight;
  public bool Running=>Phase==BasketballMatchPhase.Regulation||Phase==BasketballMatchPhase.Overtime;
  public bool Active=>Phase!=BasketballMatchPhase.Idle&&Phase!=BasketballMatchPhase.Finished&&Phase!=BasketballMatchPhase.Cancelled;
  public bool CanAct(double now)=>Running&&now<Deadline;
  public double Remaining(double now)=>Running?Math.Max(0,Deadline-now):remaining;
  public double ShotRemaining(double now)=>Running&&Possession!=FootballTeam.None?Math.Max(0,ShotDeadline-now):Phase==BasketballMatchPhase.Restart?ShotSeconds:0;
  public static FootballTeam Other(FootballTeam team)=>team==FootballTeam.A?FootballTeam.B:FootballTeam.A;
  void Set(BasketballMatchPhase phase,double deadline){Phase=phase;Deadline=deadline;Revision++;}
  public void Restore(BasketballMatchSnapshot s){Phase=s.phase;Result=s.result;Possession=s.possession;RestartReason=s.restart;ScoreA=s.scoreA;ScoreB=s.scoreB;Deadline=s.deadline;ShotDeadline=s.shotDeadline;Revision=s.revision;remaining=s.remaining;extra=s.phase==BasketballMatchPhase.Overtime;flight=false;}
  public void Clear(){ScoreA=ScoreB=0;Result=FootballMatchResult.None;Possession=FootballTeam.None;RestartReason=BasketballRestart.None;remaining=ShotDeadline=0;flight=extra=false;Set(BasketballMatchPhase.Idle,0);}
  public void Start(double now,double seconds,double selection=10){Clear();remaining=Math.Max(.01,seconds);Set(BasketballMatchPhase.TeamSelection,now+selection);}
  public void Confirm(double now,double countdown=3){if(Phase==BasketballMatchPhase.TeamSelection)Set(BasketballMatchPhase.Countdown,now+countdown);}
  public void Cancel(){if(!Active)return;flight=false;Result=FootballMatchResult.None;Set(BasketballMatchPhase.Cancelled,0);}
  public void Control(FootballTeam team,double now){
   if(!CanAct(now)||team==FootballTeam.None)return;
   if(team!=Possession){Possession=team;ShotDeadline=now+ShotSeconds;}
   flight=false;
  }
  public bool Release(double now){
   if(!CanAct(now)||Possession==FootballTeam.None||now>=ShotDeadline)return false;
   shotAt=now;flight=true;return true;
  }
  public void Miss(){flight=false;}
  public void Rim(double now){if(Running&&flight&&now<Deadline){ShotDeadline=now+ShotSeconds;}}
  public bool Basket(FootballTeam team,int points,double now){
   if((!Running&&Phase!=BasketballMatchPhase.Buzzer)||team==FootballTeam.None||points<2||points>3)return false;
   if(Possession!=FootballTeam.None&&now>=ShotDeadline&&(!flight||now>=shotAt+FlightSeconds))return false;
   if((Phase==BasketballMatchPhase.Buzzer||now>=Deadline)&&(!flight||now>=shotAt+FlightSeconds))return false;
   if(team==FootballTeam.A)ScoreA+=points;else ScoreB+=points;
   flight=false;
   if(extra){Finish();return true;}
   if(Phase==BasketballMatchPhase.Buzzer||now>=Deadline){
    if(ScoreA!=ScoreB){Finish();return true;}
    extra=true;remaining=OvertimeSeconds;
   }else remaining=Math.Max(0,Deadline-now);
   RestartCore(Other(team),BasketballRestart.Basket,now);return true;
  }
  public void Restart(FootballTeam team,BasketballRestart reason,double now){
   if(!Running||now>=Deadline||team==FootballTeam.None)return;
   remaining=Math.Max(0,Deadline-now);RestartCore(team,reason,now);
  }
  void RestartCore(FootballTeam team,BasketballRestart reason,double now){Possession=team;RestartReason=reason;flight=false;ShotDeadline=0;Set(BasketballMatchPhase.Restart,now+RestartSeconds);}
  public bool OpenInbound(FootballTeam team,double now){
   if(Phase!=BasketballMatchPhase.Restart||team!=Possession||now<Deadline)return false;
   ShotDeadline=now+ShotSeconds;Set(extra?BasketballMatchPhase.Overtime:BasketballMatchPhase.Regulation,now+remaining);return true;
  }
  public void Advance(double now){
   if(!Active)return;
   if(Phase==BasketballMatchPhase.Countdown){
    if(now<Deadline)return;
    if(Possession!=FootballTeam.None)ShotDeadline=now+ShotSeconds;
    Set(extra?BasketballMatchPhase.Overtime:BasketballMatchPhase.Regulation,now+remaining);return;
   }
   if(Phase==BasketballMatchPhase.Buzzer){if(!flight||now>=Deadline){flight=false;EndPeriod(now);}return;}
   if(!Running)return;
   if(flight&&now>=shotAt+FlightSeconds)flight=false;
   if(now>=Deadline){
    remaining=0;
    if(flight){Set(BasketballMatchPhase.Buzzer,shotAt+FlightSeconds);return;}
    EndPeriod(now);return;
   }
   if(Possession!=FootballTeam.None&&now>=ShotDeadline&&!flight)Restart(Other(Possession),BasketballRestart.ShotClock,now);
  }
  void EndPeriod(double now){
   if(extra||ScoreA!=ScoreB){Finish();return;}
   extra=true;remaining=OvertimeSeconds;if(Possession!=FootballTeam.None)ShotDeadline=now+ShotSeconds;
   Set(BasketballMatchPhase.Overtime,now+OvertimeSeconds);
  }
  void Finish(){remaining=0;Result=ScoreA==ScoreB?FootballMatchResult.Draw:ScoreA>ScoreB?FootballMatchResult.TeamA:FootballMatchResult.TeamB;Set(BasketballMatchPhase.Finished,0);}
 }
}
