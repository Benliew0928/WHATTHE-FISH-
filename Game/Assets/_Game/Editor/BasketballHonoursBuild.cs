#if UNITY_EDITOR
using System;
using UnityEngine;
using WhatTheFish;
public static class BasketballHonoursBuild {
 public static void BuildReview(){CheckStats();ProjectBuilder.BuildWindowsPlayer("../Builds/BasketballHonoursQA/Player/WhatTheFish.exe");}
 public static void CheckStats(){
  int checks=0;void Check(bool ok,string name){if(!ok)throw new Exception("Basketball stats: "+name);checks++;Debug.Log("PASS HONOURS "+name);}
  BasketballStatistics Make(){var s=new BasketballStatistics();for(ulong i=1;i<=4;i++)s.Players.Add(i,new BasketballPlayerRecord{id=i,team=i<=2?FootballTeam.A:FootballTeam.B});return s;}
  var s=Make();s.Shot(1,0);s.Miss();s.Touch(2,1,true);s.Shot(2,2);s.Basket(FootballTeam.A,3,3,2);Check(s.Players[1].passes==0&&s.Players[1].assists==0&&s.Players[2].points==3,"shot rebound never pass or assist; three-point scorer");
  s=Make();s.Pass(1,0);s.Touch(2,1,false);s.Touch(2,1.01,true);s.Touch(2,2,true);s.Shot(2,3);s.Basket(FootballTeam.A,2,5,2);Check(s.Players[1].passes==1&&s.Players[1].assists==1&&s.Players[2].points==2,"one reception and exact five-second direct assist");
  s=Make();s.Pass(1,0);s.Touch(2,1,true);s.Shot(2,3);s.Basket(FootballTeam.A,3,5.001,2);Check(s.Players[1].assists==0&&s.Players[1].passes==1,"late basket has no assist");
  s=Make();s.Pass(1,0);s.Touch(3,.5,false);s.Touch(3,.6,true);s.Touch(2,1,true);s.Shot(2,2);s.Basket(FootballTeam.A,2,3,2);Check(s.Players[3].interceptions==1&&s.Players[1].passes==0&&s.Players[1].assists==0,"enemy pass block counted once");
  s=Make();s.Pass(1,0);s.Touch(2,1,true);s.Shot(2,2);s.Touch(3,2.5,false);s.Basket(FootballTeam.A,3,3,2);Check(s.Players[2].points==3&&s.Players[1].assists==0&&s.Players[3].interceptions==0,"deflected shot retains shooter but breaks assist; no pass interception");
  s=Make();s.Pass(1,0);s.Touch(2,1,true);s.Shot(2,2);s.Miss();s.Touch(2,3,true);s.Shot(2,3.5);s.Basket(FootballTeam.A,2,4,2);Check(s.Players[1].assists==0,"self rebound clears assist");
  s=Make();s.Pass(1,0);s.Touch(2,1,true);s.Pass(2,2);s.Touch(1,3,true);s.Shot(1,3.5);s.Basket(FootballTeam.A,2,4,1);Check(s.Players[2].assists==1&&s.Players[1].assists==0&&s.Players[1].passes==1&&s.Players[2].passes==1,"only final direct passer earns assist");
  s=Make();s.Pass(1,0);s.Touch(1,1,true);s.Touch(2,2,true);Check(s.Players[1].passes==0,"self recovery not pass");
  s=Make();s.Pass(1,0);s.Touch(2,5.01,true);Check(s.Players[1].passes==0,"expired pass not counted");
  s=Make();s.Pass(1,0);s.ResetPlay();s.Touch(2,1,true);s.Basket(FootballTeam.A,2,2);Check(s.Players[1].passes==0&&s.Players[1].assists==0,"restart clears pending pass");
  s=Make();s.Shot(3,0);s.Basket(FootballTeam.A,2,1,3);Check(s.Players[3].points==0,"own basket excluded from individual scoring");
  s=Make();s.Players[ulong.MaxValue]=new BasketballPlayerRecord{id=ulong.MaxValue,team=FootballTeam.A};s.Pass(ulong.MaxValue,0);s.Touch(2,1,true);s.Shot(2,2);s.Basket(FootballTeam.A,2,3,2);Check(s.Players[ulong.MaxValue].assists==1,"all ulong player IDs supported");
  s=Make();s.Players[5]=new BasketballPlayerRecord{id=5,team=FootballTeam.A};s.Pass(1,0);s.Touch(5,.5,false);s.Touch(2,1,false);s.Touch(2,1.1,true);s.Shot(2,2);s.Basket(FootballTeam.A,2,3,2);Check(s.Players[1].passes==1&&s.Players[1].assists==0,"intermediate teammate deflection is not a direct assist");
  s.Clear();Check(s.Players.Count==0,"new match clears statistics");
  var a=new BasketballPlayerRecord{id=1,points=6};var b=new BasketballPlayerRecord{id=2,points=4,assists=3};Check(BasketballStatistics.MVP(new[]{a,b}).id==2,"two fewer points and three more assists wins MVP");
  b.assists=2;Check(BasketballStatistics.MVP(new[]{a,b}).id==1,"equal rating prefers scoring points");
  b.points=6;b.assists=0;b.passes=1;Check(BasketballStatistics.MVP(new[]{a,b}).id==2,"equal scoring and assists prefers passes");b.passes=0;Check(BasketballStatistics.MVP(new[]{b,a}).id==1,"deterministic final tie");
  var state=new BasketballMatchState();state.Start(0,2,0);state.Confirm(0,0);state.Advance(0);state.Control(FootballTeam.A,0);Check(state.Release(1.9),"shot released before buzzer");state.Advance(2);Check(state.Phase==BasketballMatchPhase.Buzzer&&state.Basket(FootballTeam.A,3,2.5)&&state.Phase==BasketballMatchPhase.Finished,"buzzer basket accepted before match honours");Check(!state.Basket(FootballTeam.A,3,2.6),"finished match rejects duplicate basket");
  Debug.Log("BASKETBALL_HONOURS_RULES_COMPLETE checks="+checks);
 }
}
#endif
