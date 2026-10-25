#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Opt-in authoritative snapshot fixtures; all normal player builds keep this dormant.
 public sealed partial class ScoreboardReview:MonoBehaviour {
  string folder;bool failed,guest,detailed;AppRoot app;SportId sport;
  SportsScoreboard[] Boards=>FindObjectsByType<SportsScoreboard>(FindObjectsSortMode.None).Where(v=>v.Sport==sport&&v.gameObject.activeInHierarchy).ToArray();
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-scoreboardReview");if(i<0||i+1>=a.Length)return;var r=new GameObject("Scoreboard review").AddComponent<ScoreboardReview>();r.folder=a[i+1];r.guest=a.Contains("-localClient");r.detailed=a.Contains("-scoreboardDetails");}
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Wait(Func<bool> ready,string label,float seconds=45){float until=Time.realtimeSinceStartup+seconds;while(!ready()&&Time.realtimeSinceStartup<until)yield return null;Check(ready(),label);}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");Application.targetFrameRate=60;
   yield return Wait(()=>DevelopmentProbe.ReadyToReviewSport,"sport initialized",100);
   app=AppRoot.Instance;if(!app){Finish();yield break;}sport=app.SelectedSport;
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   yield return Wait(()=>Boards.Count(v=>v.Venue)==(sport==SportId.Basketball?4:2),"all physical venue boards attached");
   yield return new WaitForSecondsRealtime(.5f);Check(Boards.All(v=>v.CelebrationCount==0),"initial display does not celebrate old score");
   if(sport==SportId.Basketball)foreach(var board in Boards.Where(v=>v.Venue)){
    var r=(RectTransform)board.transform;
    foreach(float x in new[]{.16f,.5f,.84f}){var point=r.TransformPoint(new Vector3(Mathf.Lerp(r.rect.xMin,r.rect.xMax,x),r.rect.center.y,0));bool blocked=Physics.Linecast(point-r.forward*5,point,out var hit,~0,QueryTriggerInteraction.Ignore);Check(!blocked,"court sightline to scoreboard "+x+(blocked?" blocked by "+hit.collider.name:""));}
   }
   if(guest){if(detailed)yield return DetailsGuest();else yield return Guest();Finish();yield break;}
   if(detailed&&!app.rooms.Connected&&sport==SportId.Basketball){var practice=Boards.First(v=>!v.Venue);practice.Details.SetExpanded(true);yield return new WaitForSecondsRealtime(.3f);Check(practice.Details.Expanded&&practice.Details.Players.Count==0,"shared practice has no fabricated individual statistics");practice.Details.SetExpanded(false);}
   if(app.rooms.Connected){yield return Wait(()=>Athlete.Active.Count(a=>a.GetComponent<NetworkAthlete>()&&a.GetComponent<NetworkAthlete>().IsSpawned)>=2,"guest connected");yield return new WaitForSecondsRealtime(3);}
   if(sport==SportId.Football){
    var match=FootballMatch.Instance;if(!app.rooms.Connected)Check(FootballBots.Instance.StartSolo(detailed?5:2),"football solo roster");else if(detailed)Check(FootballBots.Instance.FillRoom(2),"football network roster");
    Check(match.StartMatch(),"football match starts");match.RequestTeam(FootballTeam.A);
    if(!app.rooms.Connected)Check(match.ConfirmSoloTeam(),"football teams confirmed");
    foreach(var bot in FindObjectsByType<FootballBot>(FindObjectsSortMode.None))bot.enabled=false;
    yield return Wait(()=>match.KickoffReady||match.Running,"football valid kickoff roster");
   }else{
    if(app.rooms.Connected)Check(BasketballBots.Instance.FillRoom(2),"basketball network roster");else Check(BasketballBots.Instance.StartSolo(detailed?5:2),"basketball solo roster");
    var match=BasketballMatch.Instance;Check(match.StartMatch(),"basketball match starts");match.RequestTeam(FootballTeam.A);
    if(!app.rooms.Connected)Check(match.ConfirmSoloTeam(),"basketball teams confirmed");
    foreach(var bot in FindObjectsByType<BasketballBot>(FindObjectsSortMode.None))bot.enabled=false;
    yield return Wait(()=>match.Running,"basketball valid playing roster");
   }
   SetScore(0,0);yield return new WaitForSecondsRealtime(.5f);
   if(detailed){yield return DetailsHost();Finish();yield break;}
   Check(Boards.Length==(sport==SportId.Basketball?5:3),"HUD and physical display count");CheckScores(0,0);
   int points=sport==SportId.Basketball?2:1;SetScore(points,0);yield return new WaitForSecondsRealtime(.28f);CheckScores(points,0);
   bool scoringVisible=Boards.All(v=>v.Animating&&v.LastPoints==points&&v.CelebrationCount==1);Check(scoringVisible,"one synchronized scoring animation per display");
   if(!scoringVisible)File.AppendAllText(Path.Combine(folder,"results.txt"),"DETAIL "+string.Join("; ",Boards.Select(v=>$"venue={v.Venue} animating={v.Animating} events={v.CelebrationCount} points={v.LastPoints}"))+" frameSeconds="+Time.unscaledDeltaTime+"\n");
   yield return Capture("first-score");yield return CaptureBoard("venue-first-score");
   yield return new WaitForSecondsRealtime(2);Check(Boards.All(v=>!v.Animating&&v.CelebrationCount==1),"animations settle without replay");
   int red=sport==SportId.Basketball?3:1;SetScore(points,red);yield return new WaitForSecondsRealtime(.3f);CheckScores(points,red);
   Check(Boards.All(v=>v.LastPoints==red&&v.CelebrationCount==2),"second team score animates once");
   yield return Capture("second-score");yield return CaptureBoard("venue-second-score");
   if(sport==SportId.Basketball)Check(Boards.All(v=>v.GetComponentsInChildren<Text>().Any(t=>GameLocalization.Source(t)=="THREE POINTER")),"three pointer has dedicated headline");
   yield return new WaitForSecondsRealtime(2);var venue=Boards.First(v=>v.Venue);int count=venue.CelebrationCount;venue.gameObject.SetActive(false);yield return null;venue.gameObject.SetActive(true);yield return null;yield return null;Check(venue.CelebrationCount==count&&!venue.Animating,"reenable does not replay score");
   foreach(var size in new[]{new Vector2Int(844,390),new Vector2Int(1600,900),new Vector2Int(1024,768)}){
    Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.4f);Canvas.ForceUpdateCanvases();
    foreach(var board in Boards.Where(v=>!v.Venue)){var corners=new Vector3[4];((RectTransform)board.transform).GetWorldCorners(corners);Check(corners[0].x>=0&&corners[0].y>=0&&corners[2].x<=Screen.width&&corners[2].y<=Screen.height,"HUD fits "+size);}
    yield return Capture("layout-"+size.x+"x"+size.y);
   }
   Check(Boards.All(v=>v.GetComponentsInChildren<Graphic>().Where(g=>!g.GetComponentInParent<Button>()).All(g=>!g.raycastTarget)),"active score visuals do not intercept touch input");
   if(sport==SportId.Basketball){var signs=app.stadium.GetComponentsInChildren<StadiumSign>();string original=signs.First().label.text;var appearance=LocalProfile.Basketball;var saved=appearance.title;appearance.title="RALLY SCORE REVIEW";app.stadium.Apply(appearance);yield return null;Check(signs.All(s=>s.label.text=="RALLY SCORE REVIEW"),"custom venue title survives live scoreboard");CheckScores(points,red);appearance.title=saved;app.stadium.Apply(appearance);}
   SetScore(0,0);yield return new WaitForSecondsRealtime(.5f);CheckScores(0,0);Check(Boards.All(v=>!v.Animating),"new match clears without scoring animation");
   if(!app.rooms.Connected)yield return WinningScore();
   yield return new WaitForSecondsRealtime(3);Finish();
  }
  IEnumerator WinningScore(){
   if(sport==SportId.Basketball){
    var m=BasketballMatch.Instance;var s=m.Snapshot;s.phase=BasketballMatchPhase.Finished;s.result=FootballMatchResult.TeamA;s.scoreA=3;s.revision=m.State.Revision+1;m.State.Restore(s);m.AfterBallStep();
    yield return new WaitForSecondsRealtime(.2f);
    var hud=FindObjectsByType<BasketballMatchHUD>(FindObjectsInactive.Include,FindObjectsSortMode.None).First();var button=hud.GetComponentsInChildren<Button>(true).First(b=>b.name=="Start basketball match");
    Check(!button.gameObject.activeInHierarchy||((RectTransform)button.transform).anchoredPosition.y<=-138,"winning three pointer leaves rematch control below banner");
   }else{
    var m=FootballMatch.Instance;var s=m.Snapshot;s.phase=FootballMatchPhase.Finished;s.result=FootballMatchResult.TeamA;s.scoreA=1;s.revision=m.State.Revision+1;m.State.Restore(s,300,60,0);
    typeof(FootballMatch).GetField("goalAt",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(m,m.Now);typeof(FootballMatch).GetField("goalId",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(m,(uint)1);typeof(FootballMatch).GetMethod("Publish",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(m,null);
    yield return new WaitForSecondsRealtime(.2f);
    var hud=FindObjectsByType<FootballMatchHUD>(FindObjectsInactive.Include,FindObjectsSortMode.None).First();var button=hud.GetComponentsInChildren<Button>(true).First(b=>b.name=="Start match");
    Check(m.GoalSequence&&!button.gameObject.activeInHierarchy,"winning goal keeps rematch control hidden during celebration");
   }
   Check(Boards.Any(v=>v.Venue&&v.Animating&&v.LastPoints==(sport==SportId.Basketball?3:1)),"winning score still animates on venue displays");yield return Capture("winning-score");
  }
  IEnumerator Guest(){
   yield return Wait(()=>Boards.All(v=>v.Frame.a==(sport==SportId.Basketball?2:1)),"replicated first score",70);yield return new WaitForSecondsRealtime(.1f);
   CheckScores(sport==SportId.Basketball?2:1,0);Check(Boards.All(v=>v.CelebrationCount==1),"guest receives one first-score event");yield return Capture("guest-first-score");
   yield return Wait(()=>Boards.All(v=>v.Frame.b==(sport==SportId.Basketball?3:1)),"replicated second score");yield return new WaitForSecondsRealtime(.1f);
   CheckScores(sport==SportId.Basketball?2:1,sport==SportId.Basketball?3:1);Check(Boards.All(v=>v.CelebrationCount==2),"guest receives one second-score event");yield return CaptureBoard("guest-venue-second-score");
   yield return Wait(()=>Boards.All(v=>v.Frame.a==0&&v.Frame.b==0),"replicated rematch reset");Check(Boards.All(v=>!v.Animating),"guest reset has no stale celebration");
  }
  void CheckScores(int a,int b){var views=Boards;Check(views.Length>0&&views.All(v=>v.ScoreA.text==a.ToString()&&v.ScoreB.text==b.ToString()),"HUD and every venue score = "+a+":"+b);Check(views.Select(v=>v.Clock.text).Distinct().Count()==1,"all clocks agree");if(sport==SportId.Basketball)Check(views.Select(v=>v.Shot).Distinct().Count()==1,"all shot clocks agree");}
  void SetScore(int a,int b){
   if(sport==SportId.Basketball){var m=BasketballMatch.Instance;var s=m.Snapshot;s.phase=BasketballMatchPhase.Regulation;s.scoreA=a;s.scoreB=b;s.deadline=m.Now+300;s.shotDeadline=m.Now+20;s.possession=FootballTeam.A;s.revision=m.State.Revision+1;m.State.Restore(s);m.AfterBallStep();}
   else {var m=FootballMatch.Instance;var s=m.Snapshot;s.phase=FootballMatchPhase.Regulation;s.scoreA=a;s.scoreB=b;s.deadline=m.Now+300;s.revision=m.State.Revision+1;m.State.Restore(s,300,60,0);typeof(FootballMatch).GetMethod("Publish",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(m,null);}
  }
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();var camera=Camera.main;var canvas=app.HudFrame.GetComponentInParent<Canvas>();var mode=canvas.renderMode;var old=canvas.worldCamera;float distance=canvas.planeDistance;
   canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();Save(camera,name,Screen.width,Screen.height);canvas.renderMode=mode;canvas.worldCamera=old;canvas.planeDistance=distance;
  }
  IEnumerator CaptureBoard(string name){
   yield return new WaitForEndOfFrame();var board=Boards.First(v=>v.Venue);var rect=(RectTransform)board.transform;var corners=new Vector3[4];rect.GetWorldCorners(corners);Vector3 centre=(corners[0]+corners[2])*.5f;float width=Vector3.Distance(corners[0],corners[3]),height=Vector3.Distance(corners[0],corners[1]);
   var camera=new GameObject("Scoreboard evidence camera").AddComponent<Camera>();camera.CopyFrom(Camera.main);camera.enabled=false;camera.cullingMask=~0;camera.orthographic=true;camera.orthographicSize=Mathf.Max(height*.65f,width/4);camera.aspect=2;camera.transform.position=centre-rect.forward*2;camera.transform.LookAt(centre,rect.up);Save(camera,name,1024,512);Destroy(camera.gameObject);
  }
  void Save(Camera camera,string name,int width,int height){var rt=RenderTexture.GetTemporary(width,height,24);var previous=camera.targetTexture;var active=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);camera.targetTexture=previous;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);}
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"SCOREBOARD_REVIEW_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
