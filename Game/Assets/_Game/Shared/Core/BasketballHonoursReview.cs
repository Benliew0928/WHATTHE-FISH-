#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class BasketballHonoursReview:MonoBehaviour {
  string folder;bool failed;int checks;AppRoot app;BasketballMatch match;BasketballBall ball;Athlete passer,mate;Transform hoop,court;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-basketballHonoursReview");if(i>=0&&i+1<a.Length)new GameObject("Basketball honours review").AddComponent<BasketballHonoursReview>().folder=a[i+1];}
  void Check(bool ok,string name){checks++;failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+name+"\n");}
  IEnumerator Until(Func<bool> test,string name,float seconds=12){float end=Time.realtimeSinceStartup+seconds;while(!test()&&Time.realtimeSinceStartup<end)yield return null;Check(test(),name);}
  BasketballPlayerRecord Record(Athlete a)=>match.PlayerRecords.FirstOrDefault(p=>p.id==BasketballMatch.PlayerId(a));
  void Place(Athlete a,Vector3 p,float yaw=0){bool enabled=a.capsule.enabled;a.capsule.enabled=false;a.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));a.ResetLocomotion();var n=a.GetComponent<NetworkTransform>();if(n&&n.IsSpawned&&ball.Authority)n.Teleport(p,a.transform.rotation,a.transform.localScale);a.capsule.enabled=enabled;Physics.SyncTransforms();}
  float Heading(Athlete a,Athlete b)=>Quaternion.LookRotation(Vector3.ProjectOnPlane(b.transform.position-a.transform.position,Vector3.up)).eulerAngles.y;
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");yield return Until(()=>DevelopmentProbe.ReadyToReviewSport&&BasketballMatch.Instance&&BasketballMatch.Instance.Context,"basketball ready",90);
   app=AppRoot.Instance;match=BasketballMatch.Instance;ball=BasketballBall.Active;if(!app||!match||!ball){Finish();yield break;}court=ball.transform.parent;DevelopmentProbe.Driving=false;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   if(!ball.Authority){yield return Guest();Finish();yield break;}ball.ReviewShotRoll=()=>0;
   bool online=app.rooms.Connected;
   for(int size=2;size<=(online?2:5);size++){
    if(online)Check(BasketballBots.Instance.FillRoom(2),"room filled to 2v2");else Check(BasketballBots.Instance.StartSolo(size),"VS AI "+size+"v"+size);
    yield return null;foreach(var a in BasketballBots.Roster){var bot=a.GetComponent<BasketballBot>();if(bot)bot.enabled=false;}
    Check(online?match.StartMatch():match.ReviewStart(BasketballBots.Roster,180,.15),"start match "+size);
    if(online)foreach(var a in BasketballBots.Roster)match.ChooseTeam(a,BasketballBots.IsBot(a)?FootballTeam.B:FootballTeam.A,match.Snapshot.revision);
    else if(size==5){Check(match.ChooseTeam(app.LocalAthlete,FootballTeam.B,match.Snapshot.revision),"local selects red team");Check(match.ChooseTeam(BasketballBots.Roster.First(BasketballBots.IsBot),FootballTeam.B,match.Snapshot.revision),"teammate selects red team");}
    if(!online){if(size!=5)match.RequestTeam(FootballTeam.A);Check(match.ConfirmSoloTeam(),"explicit solo start");}
    yield return Until(()=>match.Running,"match live",20);Check(match.PlayerRecords.Count()==size*2&&match.PlayerRecords.Sum(p=>p.points+p.assists+p.passes+p.interceptions)==0,"full roster starts with zero stats "+size);
    foreach(var participant in BasketballBots.Roster)MatchBoundaryChecks.Verify(participant,true,true,Check);
    passer=app.LocalAthlete;mate=BasketballBots.Roster.First(a=>a!=passer&&match.TeamOf(a)==match.TeamOf(passer));hoop=match.AttackingHoop(passer);
    ball.ResetHome();int i=0;foreach(var a in BasketballBots.Roster)Place(a,court.TransformPoint(new Vector3(-6,.07f,-10+i++*2)));
    var receiver=hoop.TransformPoint(new Vector3(0,0,size%2==0?4:9));receiver.y=court.position.y+.07f;
    Place(mate,receiver);Place(passer,receiver+court.right*4);ball.Place(court.InverseTransformPoint(passer.transform.position+Vector3.up*.2f+passer.transform.forward*.5f),Quaternion.identity,Vector3.zero,Vector3.zero);
    yield return Until(()=>ball.Holder==passer,"passer picks up ball");yield return new WaitForSeconds(.3f);
    float heading=Heading(passer,mate);app.view.yaw=heading;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=heading};yield return null;
    Check(app.view.BeginPass(),"normal pass input begins");app.view.CancelPass();yield return new WaitForSeconds(.4f);Check(Record(passer).passes==0&&ball.Holder==passer,"cancelled pass earns no stats");
    if(!online&&size==3){
     var enemy=BasketballBots.Roster.First(a=>match.TeamOf(a)!=match.TeamOf(passer));var saved=enemy.transform.position;Place(enemy,Vector3.Lerp(passer.transform.position,mate.transform.position,.5f));
     Check(ball.TryPass(passer,heading),"physical intercepted pass released");yield return Until(()=>Record(enemy).interceptions==1,"opponent intercepts real pass",4);yield return new WaitForSeconds(.3f);Check(Record(enemy).interceptions==1&&Record(passer).passes==0,"interception counts once without completed pass");
     ball.ResetHome();Place(enemy,saved);ball.Place(court.InverseTransformPoint(passer.transform.position+Vector3.up*.2f+passer.transform.forward*.5f),Quaternion.identity,Vector3.zero,Vector3.zero);yield return Until(()=>ball.Holder==passer,"passer regains test possession");yield return new WaitForSeconds(.3f);
    }
    Check(app.view.BeginPass(),"intentional pass begins");app.view.EndPass();yield return Until(()=>ball.Holder==mate,"physical teammate reception",5);Check(Record(passer).passes==1,"one completed pass recorded "+size);
    if(online)yield return Until(()=>ball.Holder==passer&&Record(mate).passes==1,"guest RPC pass completed",8);
    var scorer=online?passer:mate;match.State.Advance(match.State.Deadline);Check(match.State.Phase==BasketballMatchPhase.Overtime,"golden basket period");
    int expected=BasketballBall.ShotValue(hoop.InverseTransformPoint(scorer.transform.position));Check(ball.TryShoot(scorer,scorer.transform.eulerAngles.y),"receiver shoots");
    yield return Until(()=>match.Snapshot.phase==BasketballMatchPhase.Finished,"physical basket ends match",6);
    Check(Record(scorer).points==expected,"actual "+expected+" point basket credited to shooter");Check(Record(online?mate:passer).assists==1,"direct pass within five seconds earns assist");
    Check(match.PlayerRecords.Sum(p=>p.points)==match.Snapshot.scoreA+match.Snapshot.scoreB,"player and team point totals agree");
    yield return Until(()=>BasketballMVP.Presenting,"basketball MVP opens");BasketballMVP.Instance.SkipCelebration();yield return Until(()=>BasketballMVP.Instance.ResultsReady,"results camera settled",5);
    Check(match.MVP.id==BasketballMatch.PlayerId(scorer),"MVP rank selects scorer");Check(app.view.InputBlocked,"MVP blocks gameplay input");
    Check(match.MVP.team==match.TeamOf(scorer),"MVP team matches authoritative selection");
    Check(Camera.main.fieldOfView==32&&BasketballMVP.Instance.CelebrationStage,"courtside 3D award camera active");
    var badges=FindAnyObjectByType<CharacterIdentityBadges>();Check(!badges||!badges.GetComponent<Canvas>().enabled,"live player number tags hidden during MVP");
    var art=BasketballMVP.Instance.GetComponentsInChildren<Image>().FirstOrDefault(t=>t.name=="MVP player illustration");Check(!art&&BasketballMVP.Instance.CelebratedMvp==match.MVP.id,"animated MVP replaces character illustration");
    foreach(var t in BasketballMVP.Instance.GetComponentsInChildren<Text>())if(t.text=="MVP"||t.text==expected.ToString())Check(t.cachedTextGenerator.vertexCount>=4,"large MVP glyphs render "+t.text);
    yield return Capture("mvp-"+size);BasketballMVP.Instance.GetComponentsInChildren<Button>().First(b=>b.name=="Toggle match stats").onClick.Invoke();yield return Capture("stats-"+size);
    BasketballMVP.Instance.GetComponentsInChildren<Button>().First(b=>b.name=="Close MVP").onClick.Invoke();PostMatchFlow.Instance.Close();foreach(var participant in BasketballBots.Roster)MatchBoundaryChecks.Verify(participant,true,false,Check);Check(!app.view.InputBlocked&&!BasketballMVP.Presenting,"MVP close restores controls");yield return null;
    if(online)yield return new WaitForSeconds(2);match.Clear();BasketballBots.Instance.Clear();yield return null;
   }
   Finish();
  }
  IEnumerator Guest(){
   yield return Until(()=>match.Running,"guest match live",30);yield return Until(()=>ball.Holder==app.LocalAthlete,"guest receives host pass",12);
   var host=BasketballBots.Roster.First(a=>a!=app.LocalAthlete&&!BasketballBots.IsBot(a));float heading=Heading(app.LocalAthlete,host);app.view.yaw=heading;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=heading};yield return new WaitForSeconds(.15f);
   Check(app.view.BeginPass(),"guest begins pass through input");app.view.EndPass();yield return Until(()=>Record(app.LocalAthlete).passes==1,"host publishes guest completed pass",7);
   yield return Until(()=>BasketballMVP.Presenting,"guest receives MVP",15);Check(match.PlayerRecords.Count()==4&&match.PlayerRecords.Sum(p=>p.points)>=2&&match.PlayerRecords.Sum(p=>p.assists)==1&&match.PlayerRecords.Sum(p=>p.passes)==2,"same complete player stats replicated");BasketballMVP.Instance.SkipCelebration();yield return Until(()=>BasketballMVP.Instance.ResultsReady,"results camera settled",5);yield return Capture("guest-mvp");BasketballMVP.Instance.GetComponentsInChildren<Button>().First(b=>b.name=="Close MVP").onClick.Invoke();PostMatchFlow.Instance.Close();Check(match.BoundaryReleased&&!app.view.InputBlocked,"guest Back to court releases boundary and input");
  }
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();var camera=Camera.main;var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var old=RenderTexture.active;var target=camera.targetTexture;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);camera.targetTexture=target;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"BASKETBALL_HONOURS_COMPLETE success="+!failed+" checks="+checks+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
