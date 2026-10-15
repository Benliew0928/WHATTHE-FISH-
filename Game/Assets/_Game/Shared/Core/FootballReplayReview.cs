#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
namespace WhatTheFish {
 public sealed class FootballReplayReview:MonoBehaviour {
  string folder;bool failed,network;AppRoot app;FootballMatch match;FootballBall ball;int checks,originalTrail;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-footballReplayReview");if(i>=0&&i+1<args.Length)new GameObject("Football replay checks").AddComponent<FootballReplayReview>().folder=args[i+1];}
  void Check(bool value,string name){checks++;failed|=!value;File.AppendAllText(Path.Combine(folder,"results.txt"),(value?"PASS ":"FAIL ")+name+"\n");}
  IEnumerator Until(Func<bool> predicate,string name,float timeout=25){float limit=Time.realtimeSinceStartup+timeout;while(!predicate()&&Time.realtimeSinceStartup<limit)yield return null;Check(predicate(),name);}
  void Bots(bool enabled){foreach(var a in FootballBots.Roster){var bot=a.GetComponent<FootballBot>();if(bot)bot.enabled=enabled;}}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   yield return Until(()=>AppRoot.Instance&&AppRoot.Instance.Exploring&&FootballMatch.Instance&&AppRoot.Instance.LocalAthlete,"football ready",80);
   app=AppRoot.Instance;match=FootballMatch.Instance;ball=FootballBall.Instance;if(!app||!match||!ball){Finish();yield break;}
   DevelopmentProbe.Driving=false;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;network=app.rooms.Connected;originalTrail=FootballTrailStyle.Selected;FootballTrailStyle.Select(network&&!match.Authority?4:2);
   if(network&&!match.Authority){yield return Guest();Finish();yield break;}
   var original=match.rules;match.rules=Instantiate(original);match.rules.teamSelectionSeconds=network?3:.3f;match.rules.kickoffSeconds=.3f;
   int start=network?1:2,end=network?2:5;
   for(int size=start;size<=end;size++){
    if(network){if(size==2)Check(FootballBots.Instance.FillRoom(2),"room fills bots");}
    else Check(FootballBots.Instance.StartSolo(size),"VS AI setup "+size+"v"+size);
    yield return null;Check(match.StartMatch(),"match begins "+size);
    if(network){foreach(var actor in FootballBots.Roster)match.ChooseTeam(actor,FootballBots.Id(actor)%2==0?FootballTeam.A:FootballTeam.B,match.Snapshot.revision);}
    Bots(false);yield return Until(()=>match.KickoffReady,"centre ready "+size);
    var taker=match.KickoffActor;Check(taker&&match.TeamOf(taker)==match.Snapshot.kickoffTeam,"assigned kickoff team "+size);
    if(!taker){Finish();yield break;}
    var other=FootballBots.Roster.First(a=>a!=taker);uint revision=match.Snapshot.revision;
    Check(!match.TryKickoff(other,0,.5f,revision)&&!match.TryKickoff(taker,0,.5f,revision-1)&&!match.TryKickoff(taker,float.NaN,.5f,revision),"wrong player stale round and invalid heading rejected "+size);
    var stationary=other.transform.position;other.Simulate(new PlayerCommand{move=Vector2.up,kick=true,jump=true,tackle=true},.1f);Check(Vector3.Distance(stationary,other.transform.position)<.001f,"opponents held until contact "+size);
    float heading=(size-2)*91+37;
    Check(match.TryKickoff(taker,heading,.2f,revision),"arbitrary angle centre kick "+size);
    Check(Vector3.Dot(Vector3.ProjectOnPlane(ball.Body.linearVelocity,Vector3.up).normalized,Quaternion.Euler(0,heading,0)*Vector3.forward)>.999f,"exact requested kick direction "+size);
    yield return new WaitForSeconds(.2f);app.view.mode=(size-1)%3;int mode=app.view.mode;float yaw=app.view.yaw;
    yield return Goal(size%2==0?1:-1,"size"+size);
    Check(app.view.mode==mode&&Mathf.Abs(Mathf.DeltaAngle(app.view.yaw,yaw))<.01f,"restores camera choice and heading "+size);
    Check(match.AwaitingKickoff&&match.TeamOf(match.KickoffActor)==match.Snapshot.kickoffTeam,"conceding side restarts "+size);
    yield return Until(()=>match.KickoffReady,"post-replay kickoff ready "+size);
    if(!network&&match.TeamOf(match.KickoffActor)==FootballTeam.B){Bots(true);yield return Until(()=>match.Running,"AI opens centre ball "+size,4);Bots(false);}
    else {var actor=match.KickoffActor;if(actor==app.LocalAthlete){app.view.yaw=271;Check(FootballHUD.Instance.Kick.button.interactable,"kickoff touch action enabled");Check(app.view.BeginKick(),"human can charge at kickoff");yield return new WaitForSeconds(.2f);app.view.EndKick();var command=app.view.ReadCommand();Check(command.kick&&Mathf.Abs(command.heading-271)<.01f,"real input produces charged angled release");actor.Simulate(command,.02f);}else if(network){yield return Until(()=>match.Running,"guest reliable kick starts play",6);Check(Vector3.Dot(Vector3.ProjectOnPlane(ball.Body.linearVelocity,Vector3.up).normalized,Quaternion.Euler(0,271,0)*Vector3.forward)>.99f,"guest release heading reaches host");}else Check(match.TryKickoff(actor,271,.4f,match.Snapshot.revision),"room taker restarts");}
    Check(match.Running,"resumed live play "+size);
    if(!network&&size==2){yield return Goal(-1,"rapid-second-goal",false);yield return Until(()=>match.KickoffReady,"rapid restart ready");Check(match.TryKickoff(match.KickoffActor,90,.2f,match.Snapshot.revision),"rapid restart contact");}
    // Keep the accepted restart observable to the guest before clearing this fixture.
    if(network)yield return new WaitForSeconds(1);
    match.ClearMatch();if(network&&size==1)yield return new WaitForSeconds(1);
   }
   if(!network){
    Check(match.StartMatch(),"golden goal fixture");Bots(false);yield return Until(()=>match.KickoffReady,"golden fixture ready");match.TryKickoff(match.KickoffActor,90,.2f,match.Snapshot.revision);
    match.State.Advance(match.State.Deadline);Check(match.Snapshot.phase==FootballMatchPhase.Overtime,"overtime fixture active");yield return Goal(1,"golden");Check(match.Snapshot.phase==FootballMatchPhase.Finished&&!FootballReplay.Presenting,"winning replay returns to result");
    match.ClearMatch();Check(match.StartMatch(),"cancellation fixture");yield return Until(()=>match.KickoffReady,"cancellation ready");match.TryKickoff(match.KickoffActor,90,.2f,match.Snapshot.revision);yield return new WaitForSeconds(4.3f);yield return Shot(1);yield return Until(()=>FootballReplay.Presenting,"replay before cancellation");match.ClearMatch();yield return null;yield return null;Check(!FootballReplay.Presenting&&!FindObjectsByType<Renderer>(FindObjectsSortMode.None).Any(r=>r.forceRenderingOff),"cancel restores all live renderers");
   }
   match.rules=original;match.ClearMatch();if(network)yield return new WaitForSeconds(2);Finish();
  }
  IEnumerator Shot(int sign){
   int index=Enumerable.Range(0,ball.GoalCount).First(i=>ball.GoalSign(i)==sign);var g=ball.GoalBounds(index);var p=new Vector3(g.center.x+.6f,ball.PitchBounds.max.y+ball.WorldRadius+.05f,ball.GoalFront(index)-sign*5);
   ball.ReviewFootballBall(ball.Pitch.TransformPoint(p),ball.Pitch.TransformDirection(Vector3.forward*sign*12));match.ResetAbilityGoalTracking();
   yield return Until(()=>match.GoalSequence,"physical ball crosses goal",3);
  }
  IEnumerator Goal(int sign,string label,bool warmup=true){
   var keeper=app.LocalAthlete;ball.ReviewGiveFootballProp(keeper,SportsProp.Portal);var held=ball.PropSlot(keeper);
   if(warmup)yield return new WaitForSeconds(4.4f);int before=match.Snapshot.scoreA+match.Snapshot.scoreB;yield return Shot(sign);var snapshot=match.Snapshot;double remaining=match.Remaining;
   Check(snapshot.scoreA+snapshot.scoreB==before+1&&snapshot.kickoffTeam==(sign>0?FootballTeam.B:FootballTeam.A),"one score and conceding team "+label);
   yield return Transition(snapshot,label,false);
   yield return Until(()=>match.Now>=snapshot.goalAt+FootballMatch.ReplayStartOffset+.1,"recap starts "+label);
   var replay=FootballReplay.Instance;Check(FootballReplay.Presenting&&replay.HasFootage,"real recorded footage "+label);
   Check(Math.Abs(replay.FootageStart-(snapshot.goalAt-4))<.001&&Math.Abs(replay.FootageEnd-(snapshot.goalAt+1))<.001,"exact four before one after "+label);
   var stage=GameObject.Find("Goal replay · visual copies only");Check(stage&&stage.GetComponentsInChildren<Collider>().Length==0&&stage.GetComponentsInChildren<Animator>().Length==0&&stage.GetComponentsInChildren<NetworkObject>().Length==0,"visual copies cannot simulate or score "+label);
   if(stage){var copy=stage.transform.Cast<Transform>().First(t=>t.name==ball.name);var screen=Camera.main.WorldToViewportPoint(copy.position);Check(screen.z>0&&screen.x>.04f&&screen.x<.96f&&screen.y>.04f&&screen.y<.96f,"recorded ball framed at replay entry "+label);}
   var replayTrail=stage?stage.GetComponentInChildren<FootballTrail>():null;Check(replayTrail&&replayTrail.IsReplay&&replayTrail.Style==FootballTrailStyle.Selected,"replay uses viewer trail "+label);Check(!ball.GetComponent<FootballTrail>().Ribbon.enabled,"live trail hidden in replay "+label);
   yield return Capture(label+"-replay");yield return Until(()=>match.Now>=snapshot.goalAt+FootballMatch.ReplayStartOffset+4.3,"goal impact replay "+label);yield return Capture(label+"-net");
   Check(snapshot.scoreA==match.Snapshot.scoreA&&snapshot.scoreB==match.Snapshot.scoreB&&Math.Abs(remaining-match.Remaining)<.001,"score and match clock protected during playback "+label);
   yield return Transition(snapshot,label,true);
   yield return Until(()=>!match.GoalSequence,"recap finishes "+label);yield return null;Check(!FootballReplay.Presenting,"replay releases camera "+label);yield return Capture(label+"-return");
   if(match.AwaitingKickoff)Check(ball.PropSlot(keeper).Equals(held),"held ability survives recap "+label);
  }
  IEnumerator Transition(FootballMatchSnapshot snapshot,string label,bool returning){
   var hud=FindFirstObjectByType<FootballReplayHUD>();var card=hud.transform.Find("Full-screen diagonal wipe") as RectTransform;
   double start=snapshot.goalAt+(returning?FootballMatch.ReplayEndOffset:FootballMatch.ReplayCaptureSeconds);
   // Independent expected durations catch a shortened hold or a proportion-based easing regression.
   double duration=returning?.8:1.6,hold=returning?.3:1;
   double first=-1,last=-1,heldFirst=-1,heldLast=-1;bool captured=false,covered=true,switched=false;
   bool previousCamera=FootballReplay.Presenting,previousKickoff=match.AwaitingKickoff;
   var trace=new System.Text.StringBuilder("elapsed,cardVisible,cardX,replayCamera,kickoff\n");double recorded=-1;
   while(match.Now<start+duration+.04){
    yield return new WaitForEndOfFrame();double now=match.Now;bool visible=card.gameObject.activeInHierarchy;
    bool stationary=visible&&Mathf.Abs(card.anchoredPosition.x)<.001f;
    if(now>=start&&visible){if(first<0)first=now;last=now;if(stationary){if(heldFirst<0)heldFirst=now;heldLast=now;}}
    if(previousCamera!=FootballReplay.Presenting||previousKickoff!=match.AwaitingKickoff){covered&=stationary;switched=true;}
    previousCamera=FootballReplay.Presenting;previousKickoff=match.AwaitingKickoff;
    if(now>=start&&now!=recorded){trace.AppendLine(FormattableString.Invariant($"{now-start:F3},{visible},{card.anchoredPosition.x:F3},{FootballReplay.Presenting},{match.AwaitingKickoff}"));recorded=now;}
    if(stationary&&!captured){captured=true;yield return Capture(label+(returning?"-return-slash":"-slash"));}
   }
   string kind=returning?"return":"entry";
   File.WriteAllText(Path.Combine(folder,label+"-"+kind+"-timing.csv"),trace.ToString());
   Check(first>=0&&Math.Abs((last-first)-duration)<.08,"visible "+kind+" duration "+label);
   Check(heldFirst>=0&&Math.Abs((heldLast-heldFirst)-hold)<.08,"stationary "+kind+" hold "+label);
   Check(switched&&covered,"camera and reset covered during "+kind+" "+label);
   Check(!card.gameObject.activeInHierarchy,"card clears after "+kind+" "+label);
  }
  IEnumerator Guest(){
   uint prior=0;int seen=0;float limit=Time.realtimeSinceStartup+100;
   while(seen<2&&Time.realtimeSinceStartup<limit){
    if(match.Snapshot.goalId>0&&match.GoalSequence&&(!FootballReplay.Presenting)&&match.Now-match.Snapshot.goalAt<1){prior=match.Snapshot.goalId;yield return Until(()=>FootballReplay.Presenting,"guest receives goal replay");Check(FootballReplay.Instance.HasFootage,"guest recorded gameplay available");yield return null;var replayTrail=GameObject.Find("Goal replay \u00b7 visual copies only").GetComponentInChildren<FootballTrail>();Check(replayTrail&&replayTrail.Style==4&&FootballTrailStyle.Selected==4,"guest replay retains its own aurora trail");Check(!match.TryKickoff(app.LocalAthlete,0,1,match.Snapshot.revision),"guest cannot mutate host state");yield return Capture("guest-replay-"+seen);yield return Until(()=>!match.GoalSequence,"guest synchronized return");Check(match.Snapshot.kickoffTeam!=FootballTeam.None&&match.Snapshot.goalId==prior,"guest conceding team and goal event retained");seen++;if(seen==2){yield return Until(()=>FootballMatch.CanAimKickoff(app.LocalAthlete),"guest owns conceding kickoff");app.view.yaw=271;Check(app.view.BeginKick(),"guest can charge centre kick");yield return new WaitForSeconds(.2f);app.view.EndKick();yield return Until(()=>match.Running,"guest reliable release accepted by host");}}
    yield return null;
   }
   Check(seen==2,"human room and filled bot room both replay on guest");
   yield return new WaitForSeconds(2);
  }
  IEnumerator Capture(string name){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)yield break;
   yield return new WaitForEndOfFrame();var camera=Camera.main;var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
   foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;}
   Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var prior=RenderTexture.active;var target=camera.targetTexture;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
   var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);camera.targetTexture=target;RenderTexture.active=prior;RenderTexture.ReleaseTemporary(rt);
   foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;}Canvas.ForceUpdateCanvases();
  }
  void Finish(){FootballTrailStyle.Select(originalTrail);File.AppendAllText(Path.Combine(folder,"results.txt"),"FOOTBALL_REPLAY_COMPLETE success="+!failed+" checks="+checks+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
