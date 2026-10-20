#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class GolfFishingAudioReview:MonoBehaviour {
  string folder;bool failed;readonly Dictionary<string,int> events=new();
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-golfFishingAudioReview");if(i>=0&&i+1<a.Length)new GameObject("Golf Fishing audio review").AddComponent<GolfFishingAudioReview>().folder=a[i+1];}
  void Check(bool ok,string message){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+message+"\n");}
  int Count(string cue)=>events.TryGetValue(cue,out int n)?n:0;
  void Heard(string cue,Vector3 point){events[cue]=Count(cue)+1;File.AppendAllText(Path.Combine(folder,"events.tsv"),$"{Time.unscaledTime:F3}\t{cue}\t{point}\n");}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");GolfFishingAudio.Trace+=Heard;
   float deadline=Time.realtimeSinceStartup+90;while((!AppRoot.Instance||!AppRoot.Instance.Exploring||!GolfFishingAudio.Current)&&Time.realtimeSinceStartup<deadline)yield return null;
   var app=AppRoot.Instance;var audio=GolfFishingAudio.Current;
   if(!app||!audio){Check(false,"audio island initialized");Finish();yield break;}
   DevelopmentProbe.Driving=false;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;audio.ReviewFocus(true);
   yield return new WaitForSeconds(.7f);
   int clips=0;foreach(var cue in GolfFishingAudio.Definitions){foreach(var clip in GolfFishingAudio.Clips(cue.cue)){Check(clip&&clip.channels==1&&clip.frequency==32000&&clip.length>0,"approved resource "+cue.cue+" / "+clip.name);clips++;}}
   Check(clips==115,"115 clips resolve through 81 approved cues");
   Check(GolfFishingAudio.Choose("gear-pickup",0)==Resources.Load<AudioClip>("BasketballAudio/pickup-01"),"pickup shares approved Basketball asset");
   Check(GolfFishingAudio.Choose("golf-go",0)==Resources.Load<AudioClip>("GolfAudio/go"),"recorded Golf GO retains original resource");
   Check(GolfFishingAudio.Clips("golf-walkway").Length==3,"all three recorded walkway takes retained");
   Check(FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(x=>x.enabled)==1,"one enabled listener");
   var ui=FindObjectsByType<GolfFishingButtonSound>(FindObjectsSortMode.None).FirstOrDefault(b=>b.GetComponent<Button>().IsInteractable());
   Check(ui,"active buttons have approval-bank audio");
   if(ui){var fixture=new GameObject("Audio review button",typeof(RectTransform),typeof(Button));var sound=fixture.AddComponent<GolfFishingButtonSound>();int n=events.Values.Sum();var e=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left};sound.OnPointerDown(e);sound.OnPointerUp(e);fixture.GetComponent<Button>().onClick.Invoke();Check(events.Values.Sum()==n+1,"button pointer and click produce exactly one sound");Destroy(fixture);}
   audio.Loop("review","fish-reel",true,app.LocalAthlete.transform.position,.5f,1,true);Check(audio.PlayingLoops>0,"approved loop starts");audio.ReviewFocus(false);Check(audio.PlayingLoops==0,"focus loss stops all loops immediately");audio.ReviewFocus(true);
   if(app.SelectedSport==SportId.Fishing){FishingFeedbackChecks.Run(Check);yield return FishingActions(app,audio);}else yield return GolfActions(app,audio);
   yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"gameplay.png"));yield return null;yield return null;
   audio.StopAll();Finish();
  }
  IEnumerator GolfActions(AppRoot app,GolfFishingAudio audio){
   var match=GolfMatchManager.Instance;var actor=app.LocalAthlete;var feedback=match.GetComponent<GolfFeedback>();feedback.ReviewFocus(true);
   if(!match.State.Running)Check(match.StartTestMatch(new[]{(GolfCartWorld.Key(actor),actor,"Audio review")}),"Golf match starts");yield return null;
   GolfFeedbackChecks.Run(feedback,match,Check);
   foreach(var cue in new[]{GolfFeedbackCue.Stone,GolfFeedbackCue.Wood,GolfFeedbackCue.Ball,GolfFeedbackCue.Water,GolfFeedbackCue.Recovery}){int n=feedback.CueCount(cue);GolfFeedback.Emit(cue,actor.transform.position,Vector3.up,.7f,GolfCartWorld.Key(actor),GolfCartWorld.Key(actor));Check(feedback.CueCount(cue)==n+1,"new authoritative contact "+cue);}
   var ball=match.Ball(actor);Check(ball,"live owned ball exists");
   if(ball){int recovery=feedback.CueCount(GolfFeedbackCue.Recovery);ball.Recover();Check(feedback.CueCount(GolfFeedbackCue.Recovery)==recovery+1,"real recovery sends one sound");
    int cup=feedback.CueCount(GolfFeedbackCue.Cup);match.EnterHole(ball,1);Check(feedback.CueCount(GolfFeedbackCue.Cup)==cup+1&&Count("golf-hole")>0,"accepted cup has physical and owner acknowledgement");
   }
   yield return null;
   int summons=Count("cart-summon");Check(GolfCartWorld.Execute(actor,GolfCartAction.Summon,GolfCartWorld.Key(actor)),"buggy summoned through production rules");yield return null;yield return null;
   Check(Count("cart-summon")==summons+1,"buggy summon sound follows accepted state once");int recall=Count("cart-recall");GolfCartWorld.Execute(actor,GolfCartAction.Recall,GolfCartWorld.Key(actor));yield return null;yield return null;Check(Count("cart-recall")==recall+1,"buggy recall follows disappearance once");
  }
  IEnumerator FishingActions(AppRoot app,GolfFishingAudio audio){
   var game=FishingGame.Instance;var feedback=game.GetComponent<FishingFeedback>();feedback.SendMessage("OnApplicationFocus",true);game.ReviewSnapshot=true;
   game.State.Begin(new[]{FishingGame.Key(app.LocalAthlete)},game.State.Match.round+1000,game.Now,false);yield return null;yield return null;
   var p=game.State.Players[0];p.origin=app.LocalAthlete.transform.position;p.fish=0;p.pier=0;
   p.sequence++;p.phase=FishingPhase.Casting;p.phaseAt=game.Now;game.State.Players[0]=p;yield return null;yield return null;
   int casts=feedback.ReviewCueCount(FishingFeedbackCue.Cast);Check(casts>0,"live local cast phase is heard");yield return null;Check(feedback.ReviewCueCount(FishingFeedbackCue.Cast)==casts,"unchanged live snapshot is silent");
   p.phase=FishingPhase.Reeling;p.phaseAt=game.Now;p.held=true;p.steer=(sbyte)FishingState.CounterSide(p,game.Now);p.tension=.2f;p.lineLength=6;game.State.Players[0]=p;yield return null;yield return null;
   Check(feedback.ReviewLoopWanted,"accepted correct-side hold winds the reel");
   p.steer=(sbyte)-FishingState.CounterSide(p,game.Now);game.State.Players[0]=p;yield return null;Check(!feedback.ReviewLoopWanted,"wrong-side hold does not wind reel");
   p.held=false;p.tension=.9f;game.State.Players[0]=p;yield return null;Check(Count("fish-warning")==1,"high tension warns once at cooling threshold");yield return null;Check(Count("fish-warning")==1,"cooling warning does not repeat");
   p.tension=.55f;game.State.Players[0]=p;yield return null;Check(Count("fish-cooled")==1,"cooled cue matches HUD hysteresis");
   foreach(var ability in FishingAbilityRules.Roster){p.active=ability;p.activeAt=game.Now;p.activeUntil=game.Now+8;p.itemSequence++;game.State.Players[0]=p;yield return null;yield return null;yield return new WaitForSeconds(.03f);}
   foreach(string cue in new[]{"sonar","cool-spool","gear-pickup","power-reel","bubble-guard","quick-lure"})Check(Count(cue)==1,"accepted skill plays once "+cue);
   p.active=FishingAbility.BubbleGuard;p.bubbleGuard=true;p.activeUntil=game.Now+8;game.State.Players[0]=p;yield return null;
   p.bubbleGuard=false;p.activeUntil=game.Now;game.State.Players[0]=p;yield return null;Check(Count("bubble-block")==1,"guard consumption has separate one-shot");
   p.phase=FishingPhase.Caught;p.phaseAt=game.Now;p.awardSequence++;p.lastBonus=3;game.State.Players[0]=p;yield return null;yield return null;
   Check(Count("catch-score")==1&&Count("wanted-bonus")==1,"one confirmed catch reward and wanted bonus");
   p.phase=FishingPhase.Reeling;p.phaseAt=game.Now;p.sequence++;game.State.Players[0]=p;yield return null;yield return null;
   int landed=feedback.ReviewCueCount(FishingFeedbackCue.Landed);
   p.phase=FishingPhase.Caught;p.phaseAt=game.Now;p.awardSequence++;p.sequence++;game.State.Players[0]=p;game.State.Match.phase=FishingRoundPhase.Ended;yield return null;yield return null;
   Check(feedback.ReviewCueCount(FishingFeedbackCue.Landed)==landed+1,"final catch remains audible when match ends in same snapshot");
   p.sequence++;game.State.Players[0]=p;yield return null;yield return null;
   Check(feedback.ReviewCueCount(FishingFeedbackCue.Landed)==landed+1,"terminal sequence increment does not repeat catch sound");
   feedback.SendMessage("OnApplicationPause",true);Check(!feedback.ReviewLoopWanted,"pause stops fishing winding");feedback.SendMessage("OnApplicationPause",false);
   game.ReviewSnapshot=false;
  }
  void Finish(){GolfFishingAudio.Trace-=Heard;File.AppendAllText(Path.Combine(folder,"results.txt"),"GOLF_FISHING_AUDIO_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
