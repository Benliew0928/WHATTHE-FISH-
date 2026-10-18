#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class FootballStadiumShowReview:MonoBehaviour {
  string folder,shared,role;bool failed,watcher;AppRoot app;FootballMatch match;FootballStadiumCelebration show;NetworkAthlete own;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-stadiumShowReview");if(i>=0&&i+1<a.Length)new GameObject("Shared stadium show review").AddComponent<FootballStadiumShowReview>().folder=a[i+1];}
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Until(Func<bool> condition,string label,float seconds=65){float end=Time.realtimeSinceStartup+seconds;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Check(condition(),label);}
  IEnumerator Sync(string name){File.WriteAllText(Path.Combine(shared,role+"-"+name),"ready");yield return Until(()=>new[]{"host","guest","watcher"}.All(r=>File.Exists(Path.Combine(shared,r+"-"+name))),"peers "+name);}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");shared=Path.Combine(Directory.GetParent(folder).FullName,"Sync");Directory.CreateDirectory(shared);watcher=Environment.GetCommandLineArgs().Contains("-stadiumWatcher");
   Application.targetFrameRate=45;yield return Until(()=>AppRoot.Instance&&AppRoot.Instance.rooms.Connected&&RoomTeleport.Players().Length==3,"three room members");app=AppRoot.Instance;if(!app){Finish();yield break;}
   role=app.rooms.Host?"host":watcher?"watcher":"guest";own=RoomTeleport.Players().First(n=>n.IsOwner);DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   if(app.rooms.Host)NetworkAthlete.HostPlayer.AllowSpectatorsRpc(true);yield return Sync("policy");
   own.ChooseRoomSeatRpc((byte)(watcher?10:app.rooms.Host?0:1));yield return Until(()=>RoomTeleport.Players().Count(n=>n.IsSpectator)==1,"one spectator and two players");own.ReadyRpc(true);yield return Sync("ready");
   if(app.rooms.Host){var entering=app.rooms.SetExploring(true);while(!entering.IsCompleted)yield return null;}
   yield return Until(()=>DevelopmentProbe.ReadyToReviewSport&&FootballMatch.Instance,"shared arrival completes");match=FootballMatch.Instance;if(!match){Finish();yield break;}
   Check(app.LocalAthlete.IsSpectator==watcher,"correct player or spectator role");yield return Sync("arrived");
   if(app.rooms.Host){match.rules=Instantiate(match.rules);match.rules.teamSelectionSeconds=.5f;match.rules.kickoffSeconds=.2f;Check(match.StartMatch(),"two player match starts");foreach(var n in RoomTeleport.Players().Where(n=>!n.IsSpectator))match.ChooseTeam(n.GetComponent<Athlete>(),n.IsOwner?FootballTeam.A:FootballTeam.B,match.Snapshot.revision);}
   yield return Until(()=>match.KickoffReady,"kickoff ready");yield return Sync("kickoff");if(app.rooms.Host)Check(match.TryKickoff(match.KickoffActor,0,.2f,match.Snapshot.revision),"host kickoff");yield return Until(()=>match.Running,"match running");yield return Sync("running");
   show=FootballStadiumCelebration.Instance;yield return LiveBroadcast();
   if(app.rooms.Host){var star=match.PlayerRecords.First(r=>r.team==FootballTeam.A);star.goals=2;star.assists=1;star.passes=7;star.interceptions=3;match.Statistics.Players[star.id]=star;var state=match.Snapshot;state.phase=FootballMatchPhase.Finished;state.deadline=match.Now;state.result=FootballMatchResult.TeamA;state.scoreA=3;state.scoreB=1;state.revision=match.State.Revision+1;match.State.Restore(state,180,60,0);}
   yield return Until(()=>FootballMVP.Presenting&&FootballStadiumCelebration.Instance&&FootballStadiumCelebration.Instance.BroadcastActive,"shared celebration visible");show=FootballStadiumCelebration.Instance;
   File.WriteAllText(Path.Combine(folder,"clock.json"),JsonUtility.ToJson(new Clock{finishedAt=match.Snapshot.deadline,mvp=match.MVP.id,revision=match.Snapshot.revision,watcher=watcher}));
   yield return Until(()=>show.Age>=2.5f,"opening fireworks");Check(show.FireworksActive&&show.ScreenCount==2&&show.FeedFrames>0,"world screens footage and fireworks visible");yield return Capture("stadium-opening");
   yield return Until(()=>show.Age>=5,"individual dismiss test");if(role=="guest"){FootballMVP.Instance.SkipCelebration();yield return new WaitForSeconds(1.5f);FootballMVP.Instance.Dismiss();yield return null;PostMatchFlow.Instance.Close();Check(show.BroadcastActive&&show.FireworksActive,"dismissed player still sees room show");}
   yield return Until(()=>show.Age>=9,"spectator stand view");if(watcher){FootballMVP.Instance.Dismiss();yield return null;PostMatchFlow.Instance.Close();Check(show.BroadcastActive&&show.FireworksActive,"spectator can watch world independently");yield return Capture("spectator-screen-and-fireworks");show.enabled=false;yield return new WaitForSeconds(1);show.enabled=true;yield return null;yield return new WaitForEndOfFrame();Check(show.Age>9&&show.BroadcastActive,"re-enabled viewer catches up to shared clock");}
   yield return Until(()=>show.Age>=14.3f,"team holds 15 seconds");Check(show.Headline=="BLUE TAKES THE WIN"&&show.FireworksActive,"team fireworks remain before boundary");
   yield return Until(()=>show.Age>=15.2f,"MVP stage boundary");Check(!show.FireworksActive&&show.Headline=="MOST VALUABLE PLAYER","fireworks stop and MVP begins together");
   yield return Until(()=>show.Age>=20,"MVP spotlight");Check(show.BroadcastActive&&show.FeedFrames>0,"MVP feed continues for ten seconds");if(watcher)StandCamera();yield return Capture(watcher?"spectator-MVP-screen":"MVP-stage");
   yield return Until(()=>show.Age>=25.3f,"25 second show ends");Check(!show.BroadcastActive&&!show.FireworksActive&&show.Headline=="MOST VALUABLE PLAYER","room show duration exactly 15 plus 10");
   yield return Sync("complete");if(app.rooms.Host)match.ClearMatch();yield return Until(()=>match.Snapshot.phase==FootballMatchPhase.Idle,"new round clears presentation");yield return null;yield return new WaitForEndOfFrame();Check(!show.BroadcastActive&&!show.FireworksActive&&show.Footage&&show.Mode==FootballStadiumFeedMode.Live,"new round releases MVP and resumes live pitch");yield return Sync("clean");Finish();
  }
  IEnumerator LiveBroadcast(){
   yield return new WaitForSeconds(5);Check(show.Mode==FootballStadiumFeedMode.Live&&show.FeedFrames>20,"room live footage before goal");Check(Mathf.Abs(show.LiveHeaderFraction-.3f)<.001f&&show.ScoreHeader=="0  :  0","room 30 percent score header");yield return Sync("live");
   if(app.rooms.Host){var ball=FootballBall.Instance;int index=Enumerable.Range(0,ball.GoalCount).First(i=>ball.GoalSign(i)==1);var g=ball.GoalBounds(index);ball.ReviewFootballBall(ball.Pitch.TransformPoint(new Vector3(g.center.x+.6f,ball.PitchBounds.max.y+ball.WorldRadius+.05f,ball.GoalFront(index)-5)),ball.Pitch.forward*12);match.ResetAbilityGoalTracking();}
   yield return Until(()=>match.GoalSequence,"room goal received");yield return Until(()=>match.Now-match.Snapshot.goalAt>1.7,"room goal wipe");Check(show.TransitionActive&&show.ScoreHeader=="1  :  0","room wipe and score synchronized");
   yield return Until(()=>match.Now-match.Snapshot.goalAt>3.2,"room replay begins");Check(show.Mode==FootballStadiumFeedMode.Replay&&FootballReplay.Instance.HasFootage&&!show.TransitionActive,"room stadium shows recorded replay");Check(show.Footage&&show.Footage.height==480,"room full-screen replay texture");if(watcher)yield return Capture("spectator-goal-replay");
   yield return Until(()=>!match.GoalSequence,"room replay finishes");yield return new WaitForEndOfFrame();Check(show.Mode==FootballStadiumFeedMode.Live&&show.ScoreHeader=="1  :  0","room returns to live and keeps score");yield return Sync("broadcast");
  }
  void StandCamera(){var pitch=FootballBall.Instance.Pitch;Camera.main.transform.position=pitch.TransformPoint(new Vector3(16,16,-40));Camera.main.transform.LookAt(pitch.TransformPoint(new Vector3(0,27,-73.4f)));Camera.main.fieldOfView=58;}
  [Serializable]class Clock {public double finishedAt;public ulong mvp;public uint revision;public bool watcher;}
  IEnumerator Capture(string name){yield return new WaitForEndOfFrame();if(watcher&&name.StartsWith("spectator-"))StandCamera();var cam=Camera.main;var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=cam;c.planeDistance=1;}Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;var old=cam.targetTexture;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);cam.targetTexture=old;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}}
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"STADIUM_SHOW_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
