#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace WhatTheFish {
 public sealed class FootballMatchNetworkProbe:MonoBehaviour {
  string report;int count;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){if(Environment.GetCommandLineArgs().Contains("-networkFootballMatchAudit"))new GameObject("Network football match checks").AddComponent<FootballMatchNetworkProbe>();}
  void Check(bool value,string name){File.AppendAllText(report,(value?"PASS ":"FAIL ")+name+"\n");if(!value)throw new Exception(name);count++;}
  IEnumerator Playing(){while(!FootballMatch.Instance.Running)yield return new WaitForFixedUpdate();}
  bool FixedSlotsReplicated(){
   var match=FootballMatch.Instance;var players=Athlete.Active.Where(a=>a&&a.GetComponent<NetworkAthlete>()&&a.GetComponent<NetworkAthlete>().IsSpawned).ToArray();
   if(players.Length!=2)return false;
   foreach(var actor in players){
    if(!match.TryKickoffPose(match.TeamOf(actor),1,0,out var position,out var rotation)||Vector3.Distance(actor.transform.position,position)>.15f||Vector3.Dot(actor.transform.forward,rotation*Vector3.forward)<.99f)return false;
   }
   return true;
  }
  bool BallRenderedAtCentre(){var ball=FootballBall.Instance;return Mathf.Abs(ball.Pitch.InverseTransformPoint(ball.Body.position).z)<.01f&&Vector3.Distance(ball.transform.position,ball.Body.position)<.01f&&ball.GetComponentsInChildren<Renderer>().Any(r=>r.enabled&&Vector3.Distance(r.bounds.center,ball.Body.position)<ball.WorldRadius);}
  bool NetImpactsCorrect(){var nets=FootballBall.Instance.Pitch.GetComponentsInChildren<FootballNetSurface>();return nets.Length==2&&nets.All(n=>n.ImpactCount>0&&Mathf.Abs(n.LastContact.x)<.3f&&Mathf.Abs(n.LastContact.z-n.Dimensions.z)<.08f&&n.LastImpulse.z>.1f);}
  IEnumerator Shoot(int sign){var ball=FootballBall.Instance;int index=Enumerable.Range(0,2).First(i=>ball.GoalSign(i)==sign);float line=ball.GoalFront(index);ball.Body.position=ball.Pitch.TransformPoint(new Vector3(0,ball.PitchBounds.max.y+.24f,line-sign));ball.Body.linearVelocity=Vector3.zero;yield return new WaitForFixedUpdate();ball.Body.linearVelocity=ball.Pitch.TransformDirection(new Vector3(0,0,sign*12));yield return new WaitForSeconds(.2f);while(FootballMatch.Instance.State.Phase==FootballMatchPhase.GoalReset)yield return new WaitForFixedUpdate();}
  IEnumerator Start(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-report");report=i>=0?args[i+1]:Path.Combine(Application.persistentDataPath,"network-match.txt");Directory.CreateDirectory(Path.GetDirectoryName(report));File.WriteAllText(report,"Network football match checks\n");
   while(!AppRoot.Instance||!AppRoot.Instance.Exploring||!FootballMatch.Instance||!NetworkAthlete.HostPlayer)yield return null;
   var app=AppRoot.Instance;var match=FootballMatch.Instance;
   if(app.rooms.Host){
    var original=match.rules;match.rules=Instantiate(original);match.rules.regulationSeconds=4;match.rules.overtimeSeconds=1;match.rules.kickoffSeconds=.5f;match.rules.teamSelectionSeconds=2;
    yield return new WaitForSeconds(.4f);Check(match.StartMatch(),"HOST_START");uint stale=match.Snapshot.revision;Check(match.Snapshot.teamA==0&&match.Snapshot.teamB==0,"HOST_START_UNSELECTED");
    Check(match.ChooseTeam(app.LocalAthlete,FootballTeam.A,stale),"HOST_CHOOSES_A");
    while(match.State.Phase==FootballMatchPhase.TeamSelection)yield return new WaitForFixedUpdate();Check(Athlete.Active.Count(a=>match.TeamOf(a)==FootballTeam.A)==1&&Athlete.Active.Count(a=>match.TeamOf(a)==FootballTeam.B)==1,"HOST_TWO_TEAMS");
    yield return Playing();Check(FixedSlotsReplicated(),"HOST_FIXED_KICKOFF_SLOTS_FACING_BALL");var net=NetworkAthlete.HostPlayer;bool accept=(bool)typeof(NetworkAthlete).GetMethod("AcceptRound",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(net,new object[]{stale});Check(!accept,"OLD_ROUND_INPUT_REJECTED");
    yield return Shoot(-1);Check(match.State.ScoreB==1&&match.State.ScoreA==0,"HOST_GOAL_B_ONCE");if(Application.isBatchMode)yield return null;else yield return new WaitForEndOfFrame();Check(BallRenderedAtCentre(),"HOST_RESET_B_RENDER_VISIBLE");yield return Playing();yield return Shoot(1);Check(match.State.ScoreA==1&&match.State.ScoreB==1,"HOST_GOAL_A_ONCE");if(Application.isBatchMode)yield return null;else yield return new WaitForEndOfFrame();Check(BallRenderedAtCentre(),"HOST_RESET_A_RENDER_VISIBLE");yield return Playing();
    Check(NetImpactsCorrect(),"HOST_BOTH_NET_CONTACTS");uint liveRevision=match.Snapshot.revision;while(match.State.Phase==FootballMatchPhase.Regulation)yield return new WaitForFixedUpdate();Check(match.State.Phase==FootballMatchPhase.Overtime&&match.Snapshot.revision==liveRevision,"HOST_CONTINUOUS_OVERTIME_REVISION");
    while(match.State.Phase!=FootballMatchPhase.Finished)yield return new WaitForFixedUpdate();Check(match.State.Result==FootballMatchResult.Draw,"HOST_OVERTIME_TIMEOUT_DRAW");var guest=Athlete.Active.First(a=>a.GetComponent<NetworkAthlete>().OwnerClientId!=0);var guestBefore=guest.transform.position;yield return new WaitForSeconds(.5f);Check(Vector3.Distance(guestBefore,guest.transform.position)>.03f,"CLIENT_CAN_MOVE_AFTER_FINISH");Check(match.StartMatch()&&match.State.ScoreA==0&&match.State.ScoreB==0,"HOST_NEW_MATCH_CLEAN");yield return new WaitForSeconds(.2f);match.rules=original;_=app.rooms.SetExploring(false);while(app.Exploring)yield return null;yield return new WaitForFixedUpdate();Check(match.State.Phase==FootballMatchPhase.Idle,"HOST_ROOM_RETURN_CLEAR");
   }else{
    Check(!match.StartMatch(),"CLIENT_CANNOT_START");bool countdown=false,regulation=false,goalB=false,goalA=false,extra=false,finished=false;bool selected=false,selectionSeen=false,fullSeen=false,resetB=false,resetA=false,slotsReplicated=false;float until=Time.time+20;
    while(Time.time<until&&!finished){var s=match.Snapshot;
     if(s.phase==FootballMatchPhase.Kickoff||s.phase==FootballMatchPhase.Regulation)slotsReplicated|=FixedSlotsReplicated();
     if(s.phase==FootballMatchPhase.TeamSelection){selectionSeen=true;fullSeen|=s.teamA==s.teamCapacity&&s.teamCapacity==1;if(fullSeen&&!selected){match.RequestTeam(FootballTeam.A);yield return new WaitForSeconds(.15f);Check(match.TeamOf(app.LocalAthlete)==FootballTeam.None,"CLIENT_FULL_TEAM_REJECTED");match.RequestTeam(FootballTeam.B);selected=true;}}
     if(s.phase==FootballMatchPhase.Kickoff&&s.scoreB==1&&Mathf.Abs(FootballBall.Instance.Pitch.InverseTransformPoint(FootballBall.Instance.Body.position).z)<.01f){if(Application.isBatchMode)yield return null;else yield return new WaitForEndOfFrame();if(s.scoreA==0&&!resetB){Check(BallRenderedAtCentre(),"CLIENT_RESET_B_RENDER_VISIBLE");resetB=true;}else if(s.scoreA==1&&!resetA){Check(BallRenderedAtCentre(),"CLIENT_RESET_A_RENDER_VISIBLE");resetA=true;}}
countdown|=s.phase==FootballMatchPhase.Kickoff;regulation|=s.phase==FootballMatchPhase.Regulation;goalB|=s.scoreB==1;goalA|=s.scoreA==1;extra|=s.phase==FootballMatchPhase.Overtime;finished=s.phase==FootballMatchPhase.Finished;if(finished)Check(s.result==FootballMatchResult.Draw&&s.scoreA==1&&s.scoreB==1,"CLIENT_FINAL_SCORES_RESULT");yield return null;}
    Check(selectionSeen&&fullSeen&&countdown&&regulation&&extra&&finished,"CLIENT_ALL_MATCH_PHASES");Check(goalA&&goalB,"CLIENT_BOTH_SCORES");Check(NetImpactsCorrect(),"CLIENT_RECEIVES_BOTH_AUTHORITATIVE_NET_CONTACTS");var targetNet=FootballBall.Instance.Pitch.GetComponentInChildren<FootballNetSurface>();uint impactCount=targetNet.ImpactCount;targetNet.Contact(targetNet.transform.position,Vector3.forward*22,Vector3.back);Check(targetNet.ImpactCount==impactCount,"CLIENT_CANNOT_AUTHOR_NET_COLLISION");Check(resetA&&resetB,"CLIENT_BOTH_RESTARTS_VISUALLY_CHECKED");Check(slotsReplicated,"CLIENT_RECEIVES_HOST_FIXED_SLOTS_AND_FACING");Check(match.TeamOf(app.LocalAthlete)==FootballTeam.B,"CLIENT_TEAM_B");DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,sprint=true};while(app.Exploring)yield return null;DevelopmentProbe.TurnCommandActive=false;Check(!FootballMatch.BlocksActions,"CLIENT_RETURN_UNBLOCKED");
   }
   File.AppendAllText(report,"NETWORK_MATCH_COMPLETE checks="+count+"\n");
  }
 }
}
#endif
