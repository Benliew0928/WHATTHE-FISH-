#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace WhatTheFish {
 public sealed class StadiumTeardownReview:MonoBehaviour {
  string folder;bool failed;int checks;AppRoot app;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-stadiumTeardownReview");if(i>=0&&i+1<args.Length)new GameObject("Stadium lifecycle regression").AddComponent<StadiumTeardownReview>().folder=args[i+1];}
  void Check(bool ok,string label){checks++;failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Until(Func<bool> condition,string label,float seconds=60){float end=Time.realtimeSinceStartup+seconds;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Check(condition(),label);}
  IEnumerator FinishMatch(){
   var match=FootballMatch.Instance;match.rules=Instantiate(match.rules);match.rules.teamSelectionSeconds=.2f;match.rules.kickoffSeconds=.1f;
   Check(FootballBots.Instance.StartSolo(2),"prepare live stadium fixture");yield return null;
   Check(match.StartMatch(),"start live fixture");match.RequestTeam(FootballTeam.A);Check(match.ConfirmSoloTeam(),"confirm live fixture");
   foreach(var a in FootballBots.Roster){var bot=a.GetComponent<FootballBot>();if(bot)bot.enabled=false;}
   yield return Until(()=>match.KickoffReady,"kickoff ready");Check(match.TryKickoff(match.KickoffActor,0,.2f,match.Snapshot.revision),"release kickoff");yield return null;
   var star=match.PlayerRecords.First(r=>r.team==FootballTeam.A);star.goals=2;match.Statistics.Players[star.id]=star;
   var state=match.Snapshot;state.phase=FootballMatchPhase.Finished;state.deadline=match.Now;state.result=FootballMatchResult.TeamA;state.scoreA=2;state.scoreB=0;state.revision=match.State.Revision+1;match.State.Restore(state,180,60,0);
   yield return Until(()=>FootballStadiumCelebration.Instance&&FootballStadiumCelebration.Instance.Mode==FootballStadiumFeedMode.Mvp&&FootballStadiumCelebration.Instance.Footage&&FootballStadiumCelebration.Instance.FeedFrames>0,"live MVP feed created");
  }
  IEnumerator Select(SportId sport){
   app.LeaveCourt();yield return null;app.SelectSport(sport);app.EnterOffline();
   yield return Until(()=>app.SelectedSport==sport&&DevelopmentProbe.ReadyToReviewSport,"enter "+sport);
   yield return new WaitForEndOfFrame();
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   yield return Until(()=>DevelopmentProbe.ReadyToReviewSport&&FootballBall.Instance&&FootballStadiumCelebration.Instance,"football ready",100);
   app=AppRoot.Instance;if(!app||!FootballBall.Instance){Finish();yield break;}
   DevelopmentProbe.Driving=false;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   var show=FootballStadiumCelebration.Instance;yield return Until(()=>show.ScreenCount==2,"replacement screens built");
   var original=FootballBall.Instance.Pitch.Find("Stadium screen");Check(original&&!original.gameObject.activeSelf,"original screen stays replaced");
   yield return FinishMatch();var feed=show.Footage;
   show.enabled=false;yield return null;Check(!show.Footage&&!feed,"disable releases live feed");
   show.enabled=true;yield return Until(()=>show.Footage&&show.FeedFrames>0,"re-enable resumes live broadcast");
   Check(original&&!original.gameObject.activeSelf,"disable does not reactivate original screen");
   FootballMatch.Instance.ClearMatch();yield return new WaitForEndOfFrame();Check(show.Footage&&show.Mode==FootballStadiumFeedMode.Live&&!show.BroadcastActive,"new round replaces MVP with live coverage");
   yield return Select(SportId.Basketball);
   var streaming=app.environments.GetComponent<SkySailStreaming>();Check(streaming,"island streaming available");
   if(streaming)yield return streaming.Release(SportId.Football);
   Check(!show&&!SceneManager.GetSceneByName(SkySailStreaming.SceneName(SportId.Football)).isLoaded,"inactive football arena unloaded");
   Check(BasketballBall.Active&&BasketballBall.Active.Playing,"basketball remains playable after unload");
   yield return Select(SportId.Football);show=FootballStadiumCelebration.Instance;
   yield return Until(()=>show&&show.ScreenCount==2,"football screens rebuild after reload");
   yield return FinishMatch();Check(show.BroadcastActive&&show.Footage,"quit while stadium and feed are active");
   // The launcher scans the complete log after process exit, including errors
   // emitted after this coroutine completes in OnDisable/OnDestroy callbacks.
   Finish();
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"STADIUM_TEARDOWN_COMPLETE success="+!failed+" checks="+checks+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
