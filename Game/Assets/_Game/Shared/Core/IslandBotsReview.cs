#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class IslandBotsReview:MonoBehaviour {
  string folder;bool failed;AppRoot app;FishingMode savedMode;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-islandBotsReview");if(i>=0&&i+1<args.Length)new GameObject("Island bots review").AddComponent<IslandBotsReview>().folder=args[i+1];}
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Until(Func<bool> predicate,string label,float seconds=30){float end=Time.realtimeSinceStartup+seconds;while(!predicate()&&Time.realtimeSinceStartup<end)yield return null;Check(predicate(),label);}
  IEnumerator Start(){
   savedMode=IslandBots.PreferredFishingMode;Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   yield return Until(()=>DevelopmentProbe.ReadyToReviewSport,"sport loaded",100);
   app=AppRoot.Instance;if(!app||!app.Exploring){Finish();yield break;}
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;app.view.mode=2;
   bool fishing=app.SelectedSport==SportId.Fishing;
   Check(IslandBots.Instance.Count==0,"exploration starts without bots");
   app.SendMessage("Return");app.Show("sport");yield return Until(()=>!app.Menu.IsTransitioning,"sport menu ready");
   var entry=app.Menu.Content.GetComponentsInChildren<Button>().FirstOrDefault(b=>b.name=="Play vs AI");
   Check(entry&&entry.interactable,"versus bots enabled");if(!entry){Finish();yield break;}entry.onClick.Invoke();
   yield return Until(()=>app.Menu.Page=="ai"&&!app.Menu.IsTransitioning,"individual setup opens");
   Check(app.Menu.Content.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("AI opponents "))==4,"one through four opponents");
   Check(!app.Menu.Content.GetComponentsInChildren<Button>().Any(b=>b.name.StartsWith("AI team size ")),"no team selection in individual sports");
   yield return Capture("setup");
   for(int count=1;count<=4;count++){
    Check(IslandBots.Instance.StartSolo(count),"spawn "+count);yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
    Check(IslandBots.Instance.Count==count&&IslandBots.Instance.Roster.Length==count+1,"roster "+count);
    Check(IslandBots.Instance.Roster.Select(a=>fishing?FishingGame.Key(a):GolfCartWorld.Key(a)).Distinct().Count()==count+1,"unique player ids "+count);
    Check(!IslandBots.Instance.StartSolo(5)&&IslandBots.Instance.Count==count,"invalid count preserves roster");
    app.EnterOffline();yield return null;Check(IslandBots.Instance.Count==0,"explore clears bots "+count);
   }
   IslandBots.PreferredFishingMode=FishingMode.Cup;app.PlayAgainstAI(4);
   yield return Until(()=>fishing?FishingGame.Instance.State.Match.phase==FishingRoundPhase.Playing:GolfMatchManager.Instance.State.Running,"AI match starts",20);
   Time.timeScale=6;
   if(fishing)yield return Fishing();else yield return Golf();
   Time.timeScale=1;
   app.EnterOffline();yield return new WaitForFixedUpdate();yield return null;
   Check(IslandBots.Instance.Count==0&&!IslandBots.Instance.Solo&&app.LocalAthlete,"explore clears match bots and keeps human");
   Check(fishing?FishingGame.Instance.ParticipantCount==1&&FishingGame.Instance.State.Match.phase==FishingRoundPhase.Practice:GolfMatchManager.Instance.State.Phase==GolfMatchPhase.Idle,"clean single-player practice");
   // A return to the menu also owns bot cleanup.
   IslandBots.Instance.StartSolo(1);app.SendMessage("Return");yield return null;yield return null;Check(IslandBots.Instance.Count==0,"leaving cleans bots");
   Finish();
  }
  IEnumerator Fishing(){
   var game=FishingGame.Instance;
   Check(game.ParticipantCount==5,"five fishing competitors");
   Check(game.State.Players.Where(p=>p.owner!=FishingGame.Key(app.LocalAthlete)).All(p=>p.skillPicked&&p.item!=FishingAbility.None),"bots choose offered round skills");
   Check(!game.CanStart(FishingMode.Crew),"versus mode rejects cooperative Crew");
   yield return new WaitForSeconds(12);yield return Capture("fishing-play");
   for(int round=1;round<=3;round++){
    yield return Until(()=>game.State.Match.phase==FishingRoundPhase.Ended,"autonomous fishing round "+round,100);
    Check(game.State.Match.outcome==FishingOutcome.Finished,"round ends normally "+round);
    var bots=game.State.Players.Where(p=>p.owner!=FishingGame.Key(app.LocalAthlete)).ToArray();
    foreach(var p in bots)Check(p.casts>0&&p.hooks>0&&p.catches>0&&p.score>0,"bot catches and scores "+p.owner+" round "+round);
    Check(game.State.Rank(FishingGame.Key(app.LocalAthlete))>1,"bots beat idle human "+round);
    Check(game.State.Match.cupRound==round&&game.State.Match.cupValid,"cup progression "+round);
    File.AppendAllText(Path.Combine(folder,"scores.txt"),JsonUtility.ToJson(game.State.Match)+"\n"+string.Join("\n",game.State.Players.Select(p=>JsonUtility.ToJson(p)))+"\n");
    if(round==1){yield return new WaitForSecondsRealtime(1.5f);Check(FishingCeremony.Instance&&FishingCeremony.Instance.CastCount==5,"all competitors appear in awards");yield return Capture("fishing-results");}
    if(round<3){
     yield return Until(()=>game.State.Players.Where(p=>p.owner!=0).All(p=>p.ready),"bots ready for next cup round");
     var p=game.Player(app.LocalAthlete).Value;game.Execute(app.LocalAthlete,FishingAction.Ready,game.State.Match.round,p.sequence);
     Check(game.StartMatch(FishingMode.Cup),"next cup round starts");
    }
   }
   Check(game.State.Match.cupFinished&&game.State.Players.Any(p=>p.cupPoints>0),"cup finishes with earned points");
   yield return Until(()=>game.State.Players.Where(p=>p.owner!=0).All(p=>p.ready),"bots ready after cup");
   var human=game.Player(app.LocalAthlete).Value;game.Execute(app.LocalAthlete,FishingAction.Ready,game.State.Match.round,human.sequence);
   Check(game.StartMatch(FishingMode.Round),"standalone round starts after cup");
   Check(game.State.Players.All(p=>p.cupPoints==0&&p.score==0),"standalone round resets cup and catch scores");
   yield return Until(()=>game.State.Match.phase==FishingRoundPhase.Ended,"autonomous standalone round ends",100);
   Check(game.State.Players.Where(p=>p.owner!=0).All(p=>p.catches>0&&p.score>0),"all bots score in standalone round");
  }
  IEnumerator Golf(){
   var game=GolfMatchManager.Instance;Check(game.State.Players.Count==5,"five golf competitors");
   var bots=IslandBots.Instance.Roster.Where(IslandBots.IsBot).ToArray();
   Check(bots.All(a=>game.Ball(a)&&game.Ball(a)!=game.Ball(app.LocalAthlete)),"each golfer owns a ball");
   Check(bots.All(a=>!game.TrySwing(a,game.Ball(app.LocalAthlete).Owner,0,.5f,game.Round)),"bots cannot strike human ball");
   double start=game.Now;float limit=Time.realtimeSinceStartup+720;float trace=0;bool captured=false;
   while(game.State.Running&&Time.realtimeSinceStartup<limit){
    if(Time.realtimeSinceStartup>=trace){
     trace=Time.realtimeSinceStartup+5;
     File.AppendAllText(Path.Combine(folder,"golf-trace.txt"),"time="+(game.Now-start).ToString("F1")+" "+string.Join(" | ",bots.Select(a=>{var p=game.Player(a);var b=game.Ball(a);var bot=a.GetComponent<GolfBot>();return a.name+" hole="+p.CurrentHole+" strokes="+p.TotalStroke+" at="+a.transform.position+" ball="+b.Body.position+" "+bot.Intent+" "+bot.NavigationStatus;}))+"\n");
    }
    if(!captured&&game.Now-start>30){captured=true;yield return Capture("golf-play");}
    yield return null;
   }
   Check(game.State.Phase==GolfMatchPhase.Ended,"autonomous five-hole match ends");
   foreach(var a in bots){var bot=a.GetComponent<GolfBot>();Check(bot.Decisions>0&&game.Player(a).TotalStroke>0,"golfer plans and strikes "+bot.Slot);}
   Check(game.State.Players.Any(p=>p.IsFinished&&p.PlayerId!=GolfCartWorld.Key(app.LocalAthlete)),"bot completes all five holes through physics");
   Check(game.State.Results().First().player.PlayerId!=GolfCartWorld.Key(app.LocalAthlete),"bot wins against idle human");
   PostMatchFlow.Instance?.Close();var leaderboard=FindFirstObjectByType<GolfLeaderboardUI>();if(leaderboard&&!leaderboard.Expanded)leaderboard.ToggleButton.onClick.Invoke();
   yield return Capture("golf-results");
   Check(game.StartMatch(),"golf rematch starts");yield return new WaitForSeconds(3);
   Check(game.State.Players.Count==5&&bots.All(a=>game.Player(a).CurrentHole==1),"rematch resets all golfers");
  }
  IEnumerator Capture(string name){
   yield return null;yield return new WaitForEndOfFrame();
   var camera=Camera.main;if(!camera)yield break;
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
   var cameras=canvases.Select(c=>c.worldCamera).ToArray();var planes=canvases.Select(c=>c.planeDistance).ToArray();
   var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var old=RenderTexture.active;var previous=camera.targetTexture;
   try{
    foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;}
    Canvas.ForceUpdateCanvases();camera.targetTexture=target;camera.Render();RenderTexture.active=target;
    var texture=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);texture.Apply();
    Check(texture.GetPixels32().Any(p=>p.r>30||p.g>30||p.b>30),"nonblank capture "+name);
    File.WriteAllBytes(Path.Combine(folder,name+".png"),texture.EncodeToPNG());Destroy(texture);
   }finally{
    camera.targetTexture=previous;RenderTexture.active=old;RenderTexture.ReleaseTemporary(target);
    for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=RenderMode.ScreenSpaceOverlay;canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=planes[i];}
    Canvas.ForceUpdateCanvases();
   }
  }
  void Finish(){Time.timeScale=1;IslandBots.PreferredFishingMode=savedMode;File.AppendAllText(Path.Combine(folder,"results.txt"),"ISLAND_BOTS_COMPLETE success="+!failed+"\n");Application.Quit(failed?2:0);}
 }
}
#endif
