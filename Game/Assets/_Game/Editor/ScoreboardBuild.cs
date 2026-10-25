#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using WhatTheFish;

public static class ScoreboardBuild {
 public static void Build(){Check();ProjectBuilder.BuildWindowsPlayer("../Builds/ScoreboardQA/Player/WhatTheFish.exe");}
 public static void Details(){Check();ProjectBuilder.BuildWindowsPlayer("../Builds/ScoreboardDetailsQA/Player/WhatTheFish.exe");}
 public static void DetailsScripts(){Check();var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/ScoreboardDetailsQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Scoreboard details build failed");}
 public static void Scripts(){Check();var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/ScoreboardQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Scoreboard script build failed");}
 public static void Check(){
  int count=0;void Assert(bool ok,string label){if(!ok)throw new Exception("SCOREBOARD FAIL: "+label);count++;}
  var tracker=new ScoreboardScoreTracker();var s=new BasketballMatchSnapshot{phase=BasketballMatchPhase.Regulation,scoreA=18,scoreB=16,possession=FootballTeam.A,shotDeadline=114};
  var f=SportsScoreboardFrame.Basketball(s,102,100,false,default);f.context=1;
  Assert(f.clock=="01:42"&&f.shot=="14"&&f.detail=="BLUE BALL","basketball snapshot display");
  Assert(tracker.Observe(f,out _)==0,"join seeds existing score silently");s.scoreA+=2;f=SportsScoreboardFrame.Basketball(s,102,100,false,default);f.context=1;
  Assert(tracker.Observe(f,out var team)==2&&team==FootballTeam.A,"two points");Assert(tracker.Observe(f,out _)==0,"duplicate snapshot silent");
  s.scoreB+=3;f=SportsScoreboardFrame.Basketball(s,102,100,false,default);f.context=1;Assert(tracker.Observe(f,out team)==3&&team==FootballTeam.B,"three points");
  s.scoreA+=5;f=SportsScoreboardFrame.Basketball(s,102,100,false,default);f.context=1;Assert(tracker.Observe(f,out _)==0,"aggregate catch-up silent");
  f.a+=3;f.context=2;Assert(tracker.Observe(f,out _)==0,"reconnected context baselines");tracker.Clear();f.a+=3;Assert(tracker.Observe(f,out _)==0,"reactivated display baselines");
  f.a=0;f.b=0;Assert(tracker.Observe(f,out _)==0,"score reset silent");
  foreach(BasketballMatchPhase phase in Enum.GetValues(typeof(BasketballMatchPhase))){s.phase=phase;var v=SportsScoreboardFrame.Basketball(s,0,100,false,default);Assert(!string.IsNullOrEmpty(v.clock)&&!string.IsNullOrEmpty(v.phase),"basketball phase "+phase);}
  var practice=SportsScoreboardFrame.Basketball(default,0,0,true,new BasketballScore{points=18,made=8,attempts=11});Assert(practice.a==18&&practice.made==8&&practice.attempts==11&&practice.clock=="PRACTICE","shared practice values");Assert(tracker.Observe(practice,out _)==0,"practice switch baselines");practice.a+=3;practice.made++;practice.attempts++;Assert(tracker.Observe(practice,out _)==3,"shared practice score event");practice.attempts++;Assert(tracker.Observe(practice,out _)==0,"miss does not celebrate");
  foreach(FootballMatchPhase phase in Enum.GetValues(typeof(FootballMatchPhase))){var v=SportsScoreboardFrame.Football(new FootballMatchSnapshot{phase=phase},0);Assert(!string.IsNullOrEmpty(v.clock)&&!string.IsNullOrEmpty(v.phase),"football phase "+phase);}
  tracker.Clear();var g=SportsScoreboardFrame.Football(new FootballMatchSnapshot{phase=FootballMatchPhase.Regulation},73);Assert(g.clock=="01:13"&&tracker.Observe(g,out _)==0,"football baseline");g.a++;Assert(tracker.Observe(g,out team)==1&&team==FootballTeam.A,"football goal");Assert(tracker.Observe(g,out _)==0,"goal duplicate silent");g.reset=true;g.a=0;Assert(tracker.Observe(g,out _)==0,"cancel clears celebration");
  Assert(SportsScoreboardFrame.Clock(-1)=="00:00"&&SportsScoreboardFrame.Clock(.1)=="00:01","deadline rounding and clamp");
  Debug.Log("SCOREBOARD_STATE_CHECKS_PASS "+count);
 }
}
#endif
