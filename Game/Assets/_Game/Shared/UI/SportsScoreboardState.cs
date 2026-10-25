using System;
using UnityEngine;
using Unity.Netcode;

namespace WhatTheFish {
 // Presentation values only. The match/ball snapshot remains the sole scoring authority.
 public struct SportsScoreboardFrame {
  public SportId sport;public int context,a,b,made,attempts;public bool ready,practice,canCelebrate,reset;
  public string clock,phase,detail,shot;public FootballTeam possession;
  public static string Clock(double seconds){int n=(int)Math.Min(35999,Math.Max(0,Math.Ceiling(seconds)));return (n/60).ToString("00")+":"+(n%60).ToString("00");}
  public static SportsScoreboardFrame Basketball(BasketballMatchSnapshot s,double remaining,double now,bool practice,BasketballScore score,bool solo=false){
   var f=new SportsScoreboardFrame{sport=SportId.Basketball,ready=true,a=s.scoreA,b=s.scoreB,practice=practice,possession=s.possession,clock=Clock(remaining),shot="—",detail="RALLY COURT"};
   if(practice){f.a=(int)score.points;f.made=(int)score.made;f.attempts=(int)score.attempts;f.clock="PRACTICE";f.phase="SHARED COURT";f.detail="SHARED POINTS";f.canCelebrate=true;return f;}
   f.reset=s.phase==BasketballMatchPhase.Idle||s.phase==BasketballMatchPhase.TeamSelection||s.phase==BasketballMatchPhase.Countdown||s.phase==BasketballMatchPhase.Cancelled;
   f.canCelebrate=!f.reset;
   f.phase=s.phase switch{BasketballMatchPhase.Idle=>"WARM-UP",BasketballMatchPhase.TeamSelection=>"CHOOSE TEAMS",BasketballMatchPhase.Countdown=>"TIP-OFF",BasketballMatchPhase.Regulation=>"MATCH",BasketballMatchPhase.Overtime=>"GOLDEN BASKET",BasketballMatchPhase.Buzzer=>"LAST SHOT",BasketballMatchPhase.Restart=>s.restart==BasketballRestart.Basket?"BASKET":s.restart==BasketballRestart.ShotClock?"SHOT CLOCK":"OUT OF BOUNDS",BasketballMatchPhase.Cancelled=>"PLAYER LEFT",_=>s.result==FootballMatchResult.Draw?"DRAW":s.result==FootballMatchResult.TeamA?"BLUE WINS":"RED WINS"};
   if(s.phase==BasketballMatchPhase.Idle)f.clock="READY";
   if(s.phase==BasketballMatchPhase.TeamSelection||s.phase==BasketballMatchPhase.Countdown)f.clock=solo&&s.phase==BasketballMatchPhase.TeamSelection?"READY":Math.Ceiling(Math.Max(0,s.deadline-now)).ToString();
   if(s.phase==BasketballMatchPhase.Buzzer)f.clock="00:00";
   if(s.phase==BasketballMatchPhase.Cancelled)f.clock="VOID";
   if(s.phase==BasketballMatchPhase.Finished)f.clock="FT";
   bool running=s.phase==BasketballMatchPhase.Regulation||s.phase==BasketballMatchPhase.Overtime;
   if(s.possession!=FootballTeam.None&&(running||s.phase==BasketballMatchPhase.Restart)){
    f.detail=(s.possession==FootballTeam.A?"BLUE":"RED")+(running?" BALL":" INBOUND");
    f.shot=running?Math.Ceiling(Math.Max(0,s.shotDeadline-now)).ToString():"20";
   }
   return f;
  }
  public static SportsScoreboardFrame Football(FootballMatchSnapshot s,double remaining,bool solo=false){
   var f=new SportsScoreboardFrame{sport=SportId.Football,ready=true,a=s.scoreA,b=s.scoreB,clock=Clock(remaining),shot="",detail="",reset=s.phase==FootballMatchPhase.Idle||s.phase==FootballMatchPhase.TeamSelection||s.phase==FootballMatchPhase.Kickoff||s.phase==FootballMatchPhase.Cancelled};
   f.canCelebrate=!f.reset;
   f.phase=s.phase switch{FootballMatchPhase.Idle=>"KICKABOUT",FootballMatchPhase.TeamSelection=>"CHOOSE TEAMS",FootballMatchPhase.Kickoff=>"KICKOFF",FootballMatchPhase.Regulation=>"MATCH",FootballMatchPhase.Overtime=>"GOLDEN GOAL",FootballMatchPhase.Cancelled=>"PLAYER LEFT",FootballMatchPhase.Finished=>s.result==FootballMatchResult.Draw?"DRAW":s.result==FootballMatchResult.TeamA?"BLUE WINS":"RED WINS",_=>"GOAL"};
   if(s.phase==FootballMatchPhase.Idle)f.clock="READY";
   if(s.phase==FootballMatchPhase.TeamSelection||s.phase==FootballMatchPhase.Kickoff)f.clock=solo&&s.phase==FootballMatchPhase.TeamSelection?"READY":remaining<=0?"READY":Math.Ceiling(Math.Max(0,remaining)).ToString();
   if(s.phase==FootballMatchPhase.Finished)f.clock="FT";
   if(s.phase==FootballMatchPhase.Cancelled)f.clock="VOID";
   return f;
  }
 }
 // Baselines on entry, reconnect, resets and view reactivation prevent old scores replaying.
 public sealed class ScoreboardScoreTracker {
  SportsScoreboardFrame previous;bool seeded;
  public void Clear(){seeded=false;}
  public int Observe(SportsScoreboardFrame next,out FootballTeam team){
   team=FootballTeam.None;int points=0;
   if(seeded&&previous.ready&&next.ready&&previous.context==next.context&&previous.practice==next.practice&&previous.sport==next.sport&&!previous.reset&&!next.reset&&next.canCelebrate){
    int da=next.a-previous.a,db=next.b-previous.b;
    if(da>0&&db==0){points=da;team=FootballTeam.A;}else if(db>0&&da==0&&!next.practice){points=db;team=FootballTeam.B;}
    // An aggregate catch-up is not a single scoring event. Update digits without inventing a shot.
    if(next.sport==SportId.Basketball?(points!=2&&points!=3):points!=1){points=0;team=FootballTeam.None;}
   }
   previous=next;seeded=next.ready;return points;
  }
 }
 public static class SportsScoreboardFeed {
  static int basketballFrame=-1,footballFrame=-1;static SportsScoreboardFrame basketball,football;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void Reset(){basketballFrame=footballFrame=-1;}
  public static SportsScoreboardFrame Read(SportId sport){
   if(sport==SportId.Basketball){
    if(basketballFrame==Time.frameCount)return basketball;basketballFrame=Time.frameCount;
    var match=BasketballMatch.Instance;var ball=BasketballBall.Active;basketball=default;
    if(match&&ball){bool practice=!match.Room&&!match.Competitive;var state=match.Snapshot;basketball=SportsScoreboardFrame.Basketball(state,match.Remaining,match.Now,practice,ball.Score,match.SoloTeamSelection);basketball.context=Context(ball);basketball.ready=!match.Room||NetworkAthlete.HostPlayer;
     if(!practice&&state.phase==BasketballMatchPhase.Restart)basketball.detail=(state.possession==FootballTeam.A?"BLUE":"RED")+(match.RestartReady?" · PASS TO OPEN":ball.InboundPending&&!match.InboundSpaceClear?" · CLEARING SPACE":" · COLLECTING BALL");
    }
    return basketball;
   }
   if(footballFrame==Time.frameCount)return football;footballFrame=Time.frameCount;football=default;
   var fm=FootballMatch.Instance;if(fm){var state=fm.Snapshot;if(state.phase==FootballMatchPhase.Finished&&fm.GoalSequence)state.phase=FootballMatchPhase.GoalReset;football=SportsScoreboardFrame.Football(state,fm.Remaining,fm.SoloTeamSelection);football.context=Context(fm);football.ready=!(NetworkManager.Singleton&&NetworkManager.Singleton.IsListening)||NetworkAthlete.HostPlayer;}
   return football;
  }
  static int Context(UnityEngine.Object owner){var host=NetworkAthlete.HostPlayer;return unchecked(owner.GetInstanceID()*397^(host?host.GetInstanceID():0));}
 }
}
