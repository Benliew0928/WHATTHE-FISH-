#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace WhatTheFish {
 public sealed class SceneLifecycleReview:MonoBehaviour {
  string folder,mode;bool failed;int checks;AppRoot app;SkySailStreaming streaming;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-sceneLifecycleReview");if(i<0||i+1>=a.Length)return;var r=new GameObject("All sports lifecycle regression").AddComponent<SceneLifecycleReview>();r.folder=a[i+1];i=Array.IndexOf(a,"-lifecycleMode");r.mode=i>=0&&i+1<a.Length?a[i+1]:"Tour";}
  void Check(bool ok,string label){checks++;failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Until(Func<bool> condition,string label,float seconds=70){float end=Time.realtimeSinceStartup+seconds;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Check(condition(),label);}
  IEnumerator Select(SportId sport){
   SportId previous=app.SelectedSport;var old=app.environments.roots[(int)previous];app.LeaveCourt();yield return null;app.SelectSport(sport);app.EnterOffline();
   yield return Until(()=>app.SelectedSport==sport&&DevelopmentProbe.ReadyToReviewSport,"enter "+sport);
   if(previous!=sport){yield return streaming.Release(previous);Check(!old&&!SceneManager.GetSceneByName(SkySailStreaming.SceneName(previous)).isLoaded,"unload "+previous+" while "+sport+" stays live");}
   Check(SceneLifetime.CanPresent(app.environments.roots[(int)sport]),"active "+sport+" accepts presentation");
  }
  IEnumerator Fishing(){
   yield return Until(()=>FishingGame.Instance&&FishingHUD.Instance&&FishingHUD.Instance.Results&&FishingGame.Instance.State.Players.Any(),"fishing presentation ready");
   var game=FishingGame.Instance;game.ReviewSnapshot=true;var comparison=game.transform.Find("Lagoon presentation/Fish size comparison");Check(comparison,"fish comparison exists");
   comparison.gameObject.SetActive(false);game.Presentation.enabled=false;Check(!comparison.gameObject.activeSelf,"presentation disable cannot revive comparison");game.Presentation.enabled=true;
   comparison.gameObject.SetActive(false);game.enabled=false;Check(!comparison.gameObject.activeSelf,"game disable cannot revive comparison");game.enabled=true;yield return null;
   game.Presentation.Clear();Check(comparison.gameObject.activeSelf,"normal fishing clear still restores comparison");comparison.gameObject.SetActive(false);
   var state=game.State;state.Begin(new[]{FishingGame.Key(app.LocalAthlete)},state.Match.round+1,game.Now,false);
   var ended=state.Match;ended.round++;ended.phase=FishingRoundPhase.Ended;ended.mode=FishingMode.Round;ended.outcome=FishingOutcome.Finished;ended.ended=game.Now;state.Match=ended;
   var results=FishingHUD.Instance.Results;results.Tick(game,app.LocalAthlete,true);Check(results.Visible,"completed fishing result opens");results.Close();Check(results.ReopenRect.gameObject.activeSelf,"normal close offers reopen");
   results.gameObject.SetActive(false);Check(!results.ReopenRect.gameObject.activeSelf,"results disable cannot revive reopen button");results.Tick(game,app.LocalAthlete,true);Check(!results.ReopenRect.gameObject.activeSelf,"late results tick cannot revive disabled UI");results.gameObject.SetActive(true);results.Tick(game,app.LocalAthlete,true);
   SceneLifetime.BeginUnload(game.gameObject.scene);
   try{game.Presentation.Clear();results.Close();Check(!comparison.gameObject.activeSelf&&!results.ReopenRect.gameObject.activeSelf,"unloading scene rejects normal presentation restoration");}
   finally{SceneLifetime.EndUnload(game.gameObject.scene);}
   results.Tick(game,app.LocalAthlete,true);Check(results.ReopenRect.gameObject.activeSelf,"normal result reopen resumes after guard clears");
  }
  IEnumerator Golf(){yield return Until(()=>GolfMatchManager.Instance&&GolfMatchManager.Instance.CanStart,"golf ready");Check(GolfMatchManager.Instance.StartMatch(),"live golf match and equipment created");yield return null;Check(GolfMatchManager.Instance.State.Running,"golf remains playable");}
  IEnumerator Basketball(){yield return Until(()=>BasketballBall.Active&&BasketballBall.Active.Playing,"basketball ready");BasketballBall.Active.GiveMatchPossession(app.LocalAthlete);yield return null;Check(BasketballBall.Active.Held,"live basketball possession created");}
  IEnumerator Teleport(bool quit){
   var service=TeleportSequence.Ensure();var original=app.SelectedSport;var position=app.LocalAthlete.transform.position;
   Check(service.StartPreview(SportId.Basketball,TeleportDestination.Game,new[]{app.LocalAthlete}),"shared teleport starts");
   yield return Until(()=>service.Phase==TeleportPhase.Arriving,"teleport reaches committed arrival");
   if(quit){Check(app.LocalAthlete.inTransit&&app.view.InputBlocked,"quit during active teleport");yield break;}
   service.enabled=false;yield return null;Check(!TeleportSequence.Busy&&app.SelectedSport==original,"live service disable still rolls back island");Check(!app.LocalAthlete.inTransit&&app.LocalAthlete.capsule.enabled&&!app.view.InputBlocked&&Vector3.Distance(position,app.LocalAthlete.transform.position)<.08f,"live cancellation restores player and controls");service.enabled=true;
   yield return Until(()=>!streaming.Busy,"cancellation releases streaming");
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");yield return Until(()=>DevelopmentProbe.ReadyToReviewSport,"initial island ready",100);
   app=AppRoot.Instance;if(!app){Finish();yield break;}streaming=app.environments.GetComponent<SkySailStreaming>();Check(streaming&&streaming.enabledForWorld,"production island streaming enabled");DevelopmentProbe.Driving=false;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   if(mode=="Tour"){
    yield return Select(SportId.Fishing);yield return Fishing();yield return Select(SportId.Golf);yield return Golf();yield return Select(SportId.Basketball);yield return Basketball();yield return Select(SportId.Football);yield return Teleport(false);
   }else if(mode=="Fishing")yield return Fishing();else if(mode=="Golf")yield return Golf();else if(mode=="Basketball")yield return Basketball();else if(mode=="Teleport")yield return Teleport(true);
   Finish();
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"SCENE_LIFECYCLE_COMPLETE success="+!failed+" checks="+checks+"\n");Application.Quit(failed?1:0);}
  void OnDestroy(){if(!string.IsNullOrEmpty(folder)&&Directory.Exists(folder))File.AppendAllText(Path.Combine(folder,"results.txt"),(SceneLifetime.Quitting?"PASS ":"FAIL ")+"shutdown guard armed before object destruction\n");}
 }
}
#endif
