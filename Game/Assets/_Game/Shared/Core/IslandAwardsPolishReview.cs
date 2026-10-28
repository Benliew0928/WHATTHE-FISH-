#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class IslandAwardsPolishReview:MonoBehaviour {
  string folder,report;bool failed;FishingCeremony ceremony;
  static string Arg(string key){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:"";}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){string path=Arg("-awardsPolishReview");if(path.Length>0)new GameObject("Awards sound and results review").AddComponent<IslandAwardsPolishReview>().folder=path;}
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(report,(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Frame(){yield return null;yield return new WaitForEndOfFrame();Canvas.ForceUpdateCanvases();}
  void Capture(string name)=>FishingResultsChecks.Capture(report,name,AppRoot.Instance.SelectedSport==SportId.Golf?15:5);
  IEnumerator Start(){
   Directory.CreateDirectory(folder);report=Path.Combine(folder,"results.txt");File.WriteAllText(report,"");
   float until=Time.realtimeSinceStartup+100;while(!DevelopmentProbe.ReadyToReviewSport&&Time.realtimeSinceStartup<until)yield return null;
   if(!DevelopmentProbe.ReadyToReviewSport){Check(false,"island ready");Application.Quit(1);yield break;}
   while(IslandTutorialUI.DismissTop()){}yield return new WaitForSecondsRealtime(1);
   string language=Arg("-language");GameLocalization.Select(language=="zh-Hans"?GameLanguage.SimplifiedChinese:language=="ms"?GameLanguage.Malay:GameLanguage.English,false);
   Check(GameLocalization.Code==(language.Length>0?language:"en"),"requested language is active after profile load");
   var app=AppRoot.Instance;bool golf=app.SelectedSport==SportId.Golf;var actor=app.LocalAthlete;var match=GolfMatchManager.Instance;var fish=FishingGame.Instance;
   float master=MenuPreferences.MasterVolume,sfx=MenuPreferences.SFXVolume;bool reduced=MenuPreferences.ReducedMotion;
   MenuPreferences.SetMaster(.8f);MenuPreferences.SetSFX(.8f);MenuPreferences.SetMotion(false);
   ceremony=gameObject.AddComponent<FishingCeremony>();Check(golf?ceremony.BeginGolfPreview(match):ceremony.BeginFishingPreview(fish),"ceremony opens");
   var audio=FindFirstObjectByType<IslandAwardsAudio>();Check(audio,"ceremony owns sound presenter");audio.ReviewFocus(true);
   float walkAt=golf?2.5f:4.8f;
   ceremony.ReviewTime(2.2f);yield return Frame();Check(audio.WaterPlaying==!golf,"only Fishing podium rise plays gentle water");Check(audio.StepCount==0&&audio.CelebrationCount==0,"opening has no footsteps or premature fanfare");
   ceremony.ReviewTime(4.1f);yield return Frame();Check(!audio.WaterPlaying,"water ends with podium rise");
   ceremony.ReviewTime(walkAt+3);yield return Frame();int before=audio.StepCount;
   for(int i=1;i<=100;i++){ceremony.ReviewTime(walkAt+3+i/60f);yield return Frame();}
   Check(audio.StepCount-before>=2&&audio.StepCount-before<=18,"visible walk uses sparse planted steps rather than a repeating step loop");
   Check(audio.StepCue==(golf?"grass-walk":"wood-steps"),"correct grass or timber surface cue");
   int steps=audio.StepCount;MenuPreferences.SetSFX(0);
   for(int i=101;i<=130;i++){ceremony.ReviewTime(walkAt+3+i/60f);yield return Frame();}
   Check(audio.StepCount==steps,"muted footsteps are not emitted");MenuPreferences.SetSFX(.8f);
   ceremony.ReviewTime(ceremony.ReviewSettleAt+.1f);yield return Frame();Check(audio.CelebrationCount==1,"standing triggers fanfare and cheers once");
   ceremony.ReviewTime(ceremony.ReviewSettleAt+.2f);yield return Frame();Check(audio.CelebrationCount==1,"refresh never repeats celebration");
   audio.ReviewFocus(false);yield return Frame();Check(audio.GetComponentsInChildren<AudioSource>().All(s=>!s.isPlaying),"focus loss stops all ceremony sources");audio.ReviewFocus(true);yield return Frame();Check(audio.CelebrationCount==1,"focus return does not replay fanfare");
   Check(Resources.Load<AudioClip>("IslandAwards/Cheers").length>1,"original cheers clip is present");
   ceremony.Cancel();yield return Frame();Check(!FindFirstObjectByType<IslandAwardsAudio>(),"cancelling removes ceremony sound sources");
   // Exercise real score UIs with read-only synthetic result rosters.
   if(golf){
    if(!match.State.Running)match.StartMatch();yield return Frame();ulong id=match.Player(actor).PlayerId;
    var roster=Enumerable.Range(100,14).Select(i=>new GolfEntrant((ulong)i,"Long teammate display name "+i)).Append(new GolfEntrant(id,"My_Long_Display_Name_For_Review")).ToArray();
    match.State.Clear();Check(match.State.Start(roster,match.Now),"fifteen-player ranking fixture starts");
    foreach(var p in roster.Where(p=>p.id!=id))for(int h=1;h<=4;h++)match.State.EnterHole(p.id,h,match.Now);
    yield return Frame();var table=FindFirstObjectByType<GolfLeaderboardUI>();table.OpenResults();yield return Frame();yield return Frame();
    Check(table.Expanded&&table.VisiblePlayerCount==15&&table.RankingScroll.vertical,"full ranking retains every player and scrolling");
    var marks=table.BoardRect.GetComponentsInChildren<RectTransform>().Where(t=>t.name=="Local player marker"&&t.gameObject.activeInHierarchy).ToArray();
    Check(marks.Length==1,"exactly one stronger local-player marker");var row=(RectTransform)marks[0].parent;
    var corners=new Vector3[4];row.GetWorldCorners(corners);Check(corners.All(c=>{var p=table.RowViewport.InverseTransformPoint(c);return p.y>=table.RowViewport.rect.yMin-2&&p.y<=table.RowViewport.rect.yMax+2;}),"opening reveals entire local row near bottom of ranking");
    Check(table.DisplayedPlayerIds.Last()==id,"ranking order is preserved");Capture("golf-local-row");
    table.RankingScroll.content.anchoredPosition=Vector2.zero;yield return Frame();yield return Frame();Check(Mathf.Abs(table.RankingScroll.content.anchoredPosition.y)<1,"live refresh respects manual scroll");
    table.CloseResults();yield return Frame();table.OpenResults();yield return Frame();Check(table.RankingScroll.content.anchoredPosition.y>100,"reopening reveals local player again");
    Check(table.RowsReadable()&&table.FitsSafeFrame(),"native ranking text and frame remain readable");
    var name=row.GetComponentsInChildren<Text>().First(t=>t.name.EndsWith(" Player",StringComparison.Ordinal));
    Check(name.text.EndsWith(GameLocalization.Translate(" (you)"),StringComparison.Ordinal),"long local name retains translated local suffix");
    // A fresh ceremony must duck in place while the ranking is open.
    table.CloseResults();Check(ceremony.BeginGolfPreview(match),"second ceremony opens");audio=FindFirstObjectByType<IslandAwardsAudio>();audio.ReviewFocus(true);
    ceremony.ReviewTime(ceremony.ReviewSettleAt+.1f);yield return Frame();ceremony.Skip();
    for(int i=0;i<22;i++){ceremony.ReviewTime(ceremony.ReviewSettleAt+.12f+i/60f);yield return new WaitForSecondsRealtime(.02f);}
    Check(table.Expanded&&audio.CelebrationGain<.35f,"viewing Golf results ducks celebration to a quarter");Capture("golf-ceremony-results");table.CloseResults();
    for(int i=0;i<20;i++)yield return new WaitForSecondsRealtime(.02f);
    Check(audio.CelebrationGain>.9f&&audio.CelebrationCount==1,"closing results restores mix without restarting sound");ceremony.Cancel();
   }else{
    fish.ReviewSnapshot=true;ulong id=FishingGame.Key(actor);var records=Enumerable.Range(100,4).Select(i=>new FishingPlayerRecord{owner=(ulong)i,connected=true,phase=FishingPhase.Ready,fish=-1,pier=-1,score=i,cupPoints=i-99,catches=1}).Append(new FishingPlayerRecord{owner=id,connected=true,phase=FishingPhase.Ready,fish=-1,pier=-1,score=1,catches=1}).ToArray();
    var m=new FishingMatchSnapshot{round=fish.State.Match.round+1000,phase=FishingRoundPhase.Ended,mode=FishingMode.Cup,cupRound=3,cupValid=true,cupFinished=true,outcome=FishingOutcome.Finished,started=fish.Now-180,ended=fish.Now,deadline=fish.Now};
    fish.State.Receive(m,records,fish.State.Fish.ToArray());var results=FindFirstObjectByType<FishingResultsUI>();results.Tick(fish,actor,true);yield return Frame();
    var names=(string[])typeof(FishingResultsUI).GetField("frozenNames",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(results);names[4]="Long_Display_Name_For_Review";yield return Frame();
    Check(results.Visible&&results.FitsSafeFrame()&&results.Readable(),"Fishing result card retains all five readable rows");
    var marks=results.CardRect.GetComponentsInChildren<RectTransform>().Where(t=>t.name=="Local player marker"&&t.gameObject.activeInHierarchy).ToArray();Check(marks.Length==1,"Fishing marks the local row without hiding other standings");
    var name=marks[0].parent.GetComponentsInChildren<Text>().First(t=>t.name.StartsWith("Fishing final name"));Check(name.text.EndsWith(GameLocalization.Translate(" · YOU"),StringComparison.Ordinal),"Fishing long name retains translated local suffix");Capture("fishing-local-row");
    // End-of-ceremony tails remain quiet under the Fishing results card.
    results.Close();Check(ceremony.BeginFishingPreview(fish),"second Fishing ceremony opens");audio=FindFirstObjectByType<IslandAwardsAudio>();audio.ReviewFocus(true);ceremony.ReviewTime(ceremony.ReviewSettleAt+.1f);yield return Frame();audio.Complete(true);
    for(int i=0;i<20;i++)yield return new WaitForSecondsRealtime(.02f);Check(audio&&audio.CelebrationGain<.35f,"Fishing result transition ducks remaining cheers");ceremony.Cancel();results.Open();
   }
   MenuPreferences.SetMaster(master);MenuPreferences.SetSFX(sfx);MenuPreferences.SetMotion(reduced);
   File.AppendAllText(report,"AWARDS_POLISH_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);
  }
  void OnDisable(){if(ceremony)ceremony.Cancel();}
 }
}
#endif
