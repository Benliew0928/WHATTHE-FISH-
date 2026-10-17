#if UNITY_EDITOR
using System;
using UnityEngine;
using WhatTheFish;
public static class FootballPlaymakerBuild {
 public static void BuildReview(){CheckStats();ProjectBuilder.BuildWindowsPlayer("../Builds/FootballPlaymakerQA/Player/WhatTheFish.exe");}
 public static void CheckStats(){
  int checks=0;void Check(bool ok,string name){if(!ok)throw new Exception("Playmaker rule: "+name);checks++;Debug.Log("PASS PLAYMAKER "+name);}
  FootballStatistics Make(){var s=new FootballStatistics();for(ulong i=1;i<=4;i++)s.Players.Add(i,new FootballPlayerRecord{id=i,team=i<=2?FootballTeam.A:FootballTeam.B});return s;}
  var stats=Make();stats.Shot(1,0);stats.Touch(2,1,true);stats.Shot(2,2);stats.Goal(FootballTeam.A,3);Check(stats.Players[1].passes==0&&stats.Players[1].assists==0&&stats.Players[2].goals==1,"shots never become passes or assists");
  stats=Make();stats.Pass(1,0);stats.Touch(2,1,false);stats.Touch(2,1.02,true);stats.Touch(2,2,true);stats.Shot(2,4);stats.Goal(FootballTeam.A,5);Check(stats.Players[1].passes==1&&stats.Players[1].assists==1&&stats.Players[2].goals==1,"direct pass counts once and exact five second assist allowed");
  stats=Make();stats.Pass(1,0);stats.Touch(2,1,true);stats.Shot(2,4);stats.Goal(FootballTeam.A,5.001);Check(stats.Players[1].passes==1&&stats.Players[1].assists==0,"assist older than five seconds rejected");
  stats=Make();stats.Pass(1,0);stats.Touch(3,.5,false);stats.Touch(3,.6,true);stats.Touch(2,1,true);stats.Shot(2,2);stats.Goal(FootballTeam.A,3);Check(stats.Players[3].interceptions==1&&stats.Players[1].passes==0&&stats.Players[1].assists==0,"opponent block counted once and breaks assist");
  stats=Make();stats.Pass(1,0);stats.Touch(2,1,true);stats.Pass(2,2);stats.Touch(1,3,true);stats.Shot(1,3.2);stats.Goal(FootballTeam.A,4);Check(stats.Players[1].passes==1&&stats.Players[2].passes==1&&stats.Players[2].assists==1&&stats.Players[1].assists==0,"only final direct passer earns assist");
  stats=Make();stats.Pass(1,0);stats.ResetPlay();stats.Touch(2,1,true);stats.Goal(FootballTeam.A,2);Check(stats.Players[1].passes==0&&stats.Players[1].assists==0,"reset clears pending attribution");
  stats=Make();stats.Shot(3,0);stats.Goal(FootballTeam.A,1);Check(stats.Players[3].goals==0,"own goal never boosts opponent player's goals");
  stats=Make();stats.Pass(1,0);stats.Touch(1,.1,true);stats.Touch(2,1,true);Check(stats.Players[1].passes==0,"self recovery not successful pass");
  stats=Make();stats.Pass(1,0);stats.Touch(2,5.01,true);Check(stats.Players[1].passes==0,"expired loose ball not successful pass");
  stats=Make();stats.Players[ulong.MaxValue]=new FootballPlayerRecord{id=ulong.MaxValue,team=FootballTeam.A};stats.Pass(ulong.MaxValue,0);stats.Touch(2,1,true);stats.Shot(2,2);stats.Goal(FootballTeam.A,3);Check(stats.Players[ulong.MaxValue].passes==1&&stats.Players[ulong.MaxValue].assists==1,"offline local identity is a valid passer");
  stats.ResetPlay();stats.Goal(FootballTeam.A,4);Check(stats.Players[ulong.MaxValue].goals==0,"unattributed goals do not credit offline local identity");
  var a=new FootballPlayerRecord{id=1,goals=2};var b=new FootballPlayerRecord{id=2,goals=1,assists=3};Check(FootballStatistics.MVP(new[]{a,b}).id==2,"one fewer goal and three more assists wins MVP");
  b.assists=2;Check(FootballStatistics.MVP(new[]{a,b}).id==1,"equal weighted points prefers goals");
  b.goals=2;b.assists=0;b.passes=1;Check(FootballStatistics.MVP(new[]{a,b}).id==2,"equal goals and assists uses completed passes");
  b.passes=0;Check(FootballStatistics.MVP(new[]{b,a}).id==1,"exact tie resolves deterministically");
  Debug.Log("PLAYMAKER_RULES_COMPLETE checks="+checks);
 }
}
#endif
