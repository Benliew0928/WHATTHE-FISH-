#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
namespace WhatTheFish {
 public static class RoomMatchRuleChecks {
  public static void Run(Action<bool,string> check){
   BasketballMatchState Start(double seconds=180){var s=new BasketballMatchState();s.Start(0,seconds,0);s.Confirm(0,0);s.Advance(0);return s;}
   foreach(int minutes in new[]{3,5,10}){
    var s=Start(minutes*60);check(s.Deadline==minutes*60,"basketball duration "+minutes);
    s.Control(FootballTeam.A,1);check(s.ShotDeadline==21,"new possession gets 20 seconds");
    s.Control(FootballTeam.A,8);check(s.ShotDeadline==21,"teammate receives without clock reset");
    s.Control(FootballTeam.B,9);check(s.ShotDeadline==29,"opponent possession resets clock");
    s.Advance(29);check(s.Phase==BasketballMatchPhase.Restart&&s.Possession==FootballTeam.A&&s.RestartReason==BasketballRestart.ShotClock,"shot clock turnover");
    double saved=s.Remaining(29);s.Advance(30.99);check(s.Phase==BasketballMatchPhase.Restart&&s.Remaining(30.99)==saved,"restart protected for two seconds");
    s.Advance(31);check(s.Phase==BasketballMatchPhase.Restart&&s.Remaining(31)==saved,"restart waits for a pass after setup");check(!s.OpenInbound(FootballTeam.B,31)&&s.OpenInbound(FootballTeam.A,31),"only inbound side opens play");check(s.Running&&s.ShotDeadline==51&&s.Deadline==minutes*60+2,"legal inbound resumes both clocks");
    check(s.Release(32),"legal physical release");check(s.Basket(FootballTeam.A,3,33),"three point basket accepted");
    check(s.ScoreA==3&&s.Possession==FootballTeam.B,"basket awards conceding team possession");
    check(!s.Basket(FootballTeam.A,3,33.1)&&s.ScoreA==3,"duplicate basket rejected in restart");
    s.Cancel();check(s.Phase==BasketballMatchPhase.Cancelled&&s.Result==FootballMatchResult.None&&s.ScoreA==3,"disconnect void preserves visible score without winner");
    s.Advance(1000);check(!s.Basket(FootballTeam.B,2,1000)&&s.Phase==BasketballMatchPhase.Cancelled,"void immutable");
    s.Start(1001,180);check(s.ScoreA==0&&s.ScoreB==0&&s.Possession==FootballTeam.None,"rematch clears prior state");
   }
   var b=Start(5);b.Control(FootballTeam.A,0);check(b.Release(4.99),"pre-buzzer physical release");b.Advance(5);check(b.Phase==BasketballMatchPhase.Buzzer&&!b.CanAct(5),"buzzer permits flight only");check(b.Basket(FootballTeam.A,2,5.2)&&b.Result==FootballMatchResult.TeamA,"buzzer basket decides winner");check(!b.Basket(FootballTeam.B,3,5.3),"finished result immutable");
   b=Start(5);b.Control(FootballTeam.A,0);check(!b.Release(5),"release exactly on buzzer rejected");b.Advance(5);check(b.Phase==BasketballMatchPhase.Overtime&&b.Deadline==65,"tied regulation enters 60 second overtime");b.Advance(65);check(b.Result==FootballMatchResult.Draw,"no overtime basket gives draw without shootout");
   b=Start(5);b.Control(FootballTeam.A,0);b.Release(4);b.Advance(5);b.Advance(12);check(b.Phase==BasketballMatchPhase.Overtime,"stuck buzzer ball resolves in at most eight flight seconds");
   b=Start(5);b.Advance(5);b.Control(FootballTeam.B,6);b.Release(6);check(b.Basket(FootballTeam.B,3,7)&&b.Result==FootballMatchResult.TeamB,"golden basket ends immediately");
   b=Start();b.Control(FootballTeam.A,0);b.Release(19.9);b.Advance(20);check(b.Running,"legal shot continues beyond shot clock");b.Rim(20.1);b.Control(FootballTeam.A,21);check(b.ShotDeadline==40.1,"offensive rim rebound receives refreshed clock");
   b=Start();b.Control(FootballTeam.A,0);b.Release(19.9);b.Miss();b.Advance(20);check(b.Phase==BasketballMatchPhase.Restart&&b.Possession==FootballTeam.B,"airball at expired shot clock turns over");
   b=Start();b.Control(FootballTeam.A,0);b.Restart(FootballTeam.B,BasketballRestart.Boundary,4);check(b.Possession==FootballTeam.B&&b.Deadline==6&&b.Remaining(5)==176,"boundary pause and ownership");
   b=Start();check(b.Basket(FootballTeam.B,2,1)&&b.ScoreB==2,"accidental own basket credits hoop owner");
   foreach(int count in Enumerable.Range(1,11)){
    var teams=new FootballTeamSelection();check(teams.Begin(Enumerable.Range(0,count).Select(i=>(ulong)i))==(count>=2&&count<=10&&count%2==0),"even roster gate "+count);
    if(count<2||count>10||count%2!=0)continue;
    teams.Choose(0,FootballTeam.A);teams.Complete(new Random(42));check(teams.Count(FootballTeam.A)==count/2&&teams.Count(FootballTeam.B)==count/2&&teams.TeamOf(0)==FootballTeam.A,"balanced selected roster "+count);
   }
   foreach(int phase in new[]{0,1,2,3}){
    var f=new FootballMatchState();f.SelectTeams(0,1,3,1,1);if(phase>0)f.TeamsConfirmed(1);if(phase>1)f.Advance(2);if(phase>2)f.Goal(FootballTeam.A,2.1);f.Cancel();check(f.Phase==FootballMatchPhase.Cancelled&&f.Result==FootballMatchResult.None,"football cancel in phase "+phase);f.Advance(100);check(!f.Goal(FootballTeam.B,100),"football void rejects scoring "+phase);
   }
  }
 }
}
#endif
