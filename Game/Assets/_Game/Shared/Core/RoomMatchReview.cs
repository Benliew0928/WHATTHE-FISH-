#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class RoomMatchReview:MonoBehaviour {
  string folder;bool failed;AppRoot app;BasketballBall ball;BasketballMatch match;Athlete actor,partner,mateA,mateB;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-roomMatchReview");if(i>=0&&i+1<a.Length)new GameObject("Room match review").AddComponent<RoomMatchReview>().folder=a[i+1];}
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Until(Func<bool> condition,string name,float seconds=15){float end=Time.realtimeSinceStartup+seconds;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Check(condition(),name);}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");RoomMatchRuleChecks.Run(Check);
   yield return Until(()=>AppRoot.Instance&&AppRoot.Instance.Exploring&&AppRoot.Instance.LocalAthlete,"entered sport",80);
   app=AppRoot.Instance;if(!app||!app.Exploring){Finish();yield break;}actor=app.LocalAthlete;
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;app.view.mode=2;yield return new WaitForSeconds(.6f);
   if(app.SelectedSport==SportId.Football){yield return Football();Finish();yield break;}
   ball=BasketballBall.Active;match=BasketballMatch.Instance;Check(ball&&match,"basketball match component exists");if(!ball||!match){Finish();yield break;}
   if(app.rooms.Connected&&!app.rooms.Host){yield return Guest();Finish();yield break;}
   bool online=app.rooms.Connected;
   if(!online){
    Check(!match.StartMatch()&&match.State.Phase==BasketballMatchPhase.Idle,"Explore Offline cannot start a competitive match");
    var clone=Instantiate(app.athletePrefab);partner=clone.GetComponent<Athlete>();partner.Setup();
    mateA=Instantiate(app.athletePrefab).GetComponent<Athlete>();mateA.Setup();mateB=Instantiate(app.athletePrefab).GetComponent<Athlete>();mateB.Setup();
    Check(match.ReviewStart(new[]{actor,partner,mateA,mateB},180,1),"explicit development fixture starts");
   }else {partner=Athlete.Active.First(a=>a!=actor&&a.GetComponent<NetworkAthlete>()&&a.GetComponent<NetworkAthlete>().IsSpawned);Check(BasketballBots.Instance.FillRoom(2),"host supplies teammates for inbound fixture");var mates=BasketballBots.Roster.Where(BasketballBots.IsBot).ToArray();mateA=mates[0];mateB=mates[1];foreach(var mate in mates)mate.GetComponent<BasketballBot>().enabled=false;Check(match.StartMatch(),"host starts real room match");}
   uint selection=match.Snapshot.revision;Check(match.ChooseTeam(actor,FootballTeam.A,selection),"host selects A");
   Check(match.ChooseTeam(mateA,FootballTeam.A,selection)&&match.ChooseTeam(mateB,FootballTeam.B,selection),"fixture teammates selected");
   Check(!match.ChooseTeam(partner,FootballTeam.A,selection),"full team rejected");
   if(!online)Check(match.ChooseTeam(partner,FootballTeam.B,selection),"partner selects B");
   yield return Capture("selection");yield return Until(()=>match.Running,"regulation starts",18);
   Check(match.TeamOf(actor)==FootballTeam.A&&match.TeamOf(partner)==FootballTeam.B,"balanced locked teams");
   Check(Math.Abs(match.Remaining-180)<2,"room duration excludes setup");
   Check(!match.ChooseTeam(actor,FootballTeam.B,selection),"team changes rejected after selection");
   var aHoop=match.AttackingHoop(actor);var bHoop=match.AttackingHoop(partner);
   Check(aHoop&&bHoop&&aHoop!=bHoop,"teams attack opposite hoops");
   foreach(var a in new[]{actor,partner})foreach(float heading in new[]{0f,90f,180f,270f})Check(ball.SelectHoop(a.transform.position,heading,a)==match.AttackingHoop(a),"fixed attacking hoop heading="+heading+" team="+match.TeamOf(a));
   Check(!ball.SetFreeRoam(actor,true),"free roam rejected during match");
   if(SkySailWorld.Instance)Check(!SkySailWorld.Instance.CanTravel(SkySailMap.Neighbor(SportId.Basketball,1),out var why)&&why.Contains("match"),"shared travel blocked during match");
   ball.GiveMatchPossession(actor);match.Control(actor);double deadline=match.State.ShotDeadline;
   Check(ball.Holder==actor&&match.State.Possession==FootballTeam.A&&deadline>match.Now,"carrier and live shot clock initialized");
   ball.GiveMatchPossession(actor);match.Control(actor);Check(match.State.ShotDeadline==deadline,"same team control retains clock");
   ball.ReviewGiveProp(actor,SportsProp.Turbo);Check(ball.TryUseProp(actor,ball.PropSlot(actor).revision),"ability usable in regulation");
   yield return Capture("regulation");
   yield return Drop(actor,aHoop,3);Check(match.State.ScoreA==3&&match.State.ScoreB==0&&match.State.Phase==BasketballMatchPhase.Restart,"physical three pointer credited to A");
   Check(ball.Holder&&match.TeamOf(ball.Holder)==FootballTeam.B&&match.State.Possession==FootballTeam.B,"conceding team gets protected baseline ball");
   Check(ball.PropSlot(actor).kind==SportsProp.None&&ball.TurboRemaining(actor)==0,"score clears inventory and active effects");
   Check(!ball.BeginShotCharge(partner,0)&&!ball.TrySteal(actor,0)&&!ball.TryUseProp(actor,ball.PropSlot(actor).revision),"restart gates shots steals and abilities");
   double frozen=match.Remaining;Vector3 before=actor.transform.position;actor.Simulate(new PlayerCommand{move=Vector2.one,jump=true,heading=0},.1f);
   Check(Vector3.Distance(before,actor.transform.position)<.001f&&match.Remaining==frozen,"restart freezes movement and match clock");
   if(online){var n=actor.GetComponent<NetworkAthlete>();Check(!(bool)typeof(NetworkAthlete).GetMethod("AcceptBasketball",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(n,new object[]{selection}),"old network round rejected");}
   yield return Capture("basket-restart");yield return OpenRestart();
   Check(!ball.InboundPending&&match.Running,"legal pass opens restart");
   Check(match.State.ShotRemaining(match.Now)>17,"fresh possession clock");
   ball.autoPickup=false;ball.Body.isKinematic=false;ball.Place(new Vector3(7,3,0),Quaternion.identity,ball.transform.parent.right*8,Vector3.zero);match.Touch(partner);
   yield return Until(()=>match.State.Phase==BasketballMatchPhase.Restart,"out of bounds detected");
   Check(match.State.Possession==FootballTeam.A&&ball.Holder&&match.TeamOf(ball.Holder)==FootballTeam.A,"last-touch opponent owns sideline restart");
   yield return OpenRestart();
   yield return Until(()=>match.State.Phase==BasketballMatchPhase.Restart,"20-second shot clock expires",25);
   Check(match.State.RestartReason==BasketballRestart.ShotClock&&match.State.Possession==FootballTeam.B,"shot-clock turnover awards B");yield return Capture("shot-clock");
   yield return OpenRestart();
   yield return Drop(partner,bHoop,2);Check(match.State.ScoreB==2,"physical opposite-hoop basket credits B");
   yield return OpenRestart();
   if(online){yield return Until(()=>match.State.Phase==BasketballMatchPhase.Cancelled,"guest departure immediately voids match",20);Check(match.State.Result==FootballMatchResult.None&&match.State.ScoreA==3&&match.State.ScoreB==2,"void has no awarded winner and retains scores");yield return Capture("disconnected");}
   else {
    partner.gameObject.SetActive(false);yield return new WaitForFixedUpdate();Check(match.State.Phase==BasketballMatchPhase.Cancelled&&match.State.Result==FootballMatchResult.None,"departed participant voids match before another goal");partner.gameObject.SetActive(true);
    Check(match.ReviewStart(new[]{actor,partner,mateA,mateB},.5,1),"new match after void");uint rev=match.Snapshot.revision;Check(match.ChooseTeam(actor,FootballTeam.A,rev)&&match.ChooseTeam(partner,FootballTeam.B,rev)&&match.ChooseTeam(mateA,FootballTeam.A,rev)&&match.ChooseTeam(mateB,FootballTeam.B,rev),"overtime fixture teams locked");
    yield return Until(()=>match.Running,"short regulation starts");yield return Until(()=>match.State.Phase==BasketballMatchPhase.Overtime,"tied match enters overtime");
    yield return Drop(actor,match.AttackingHoop(actor),2);Check(match.State.Result==FootballMatchResult.TeamA&&match.State.Phase==BasketballMatchPhase.Finished,"golden basket ends match");yield return Capture("result");
    Check(!match.State.Basket(FootballTeam.B,3,match.Now)&&match.State.ScoreB==0,"finished score immutable");
    match.Clear();Check(match.State.Phase==BasketballMatchPhase.Idle&&ball.SetFreeRoam(actor,true),"clearing fixture restores offline sandbox");
    Destroy(partner.gameObject);Destroy(mateA.gameObject);Destroy(mateB.gameObject);
   }
   Finish();
  }
  IEnumerator OpenRestart(){
   yield return Until(()=>match.RestartReady,"inbound ready");ball.autoPickup=true;var holder=ball.Holder;var mate=new[]{actor,partner,mateA,mateB}.First(a=>a!=holder&&match.TeamOf(a)==match.TeamOf(holder));
   var exit=ball.InboundExit;var local=ball.transform.parent.InverseTransformPoint(exit);var inward=Mathf.Abs(local.x)>BasketballBall.PaintedCourtLimits.x-.01f?new Vector3(-Mathf.Sign(local.x),0,0):new Vector3(0,0,-Mathf.Sign(local.z));
   mate.capsule.enabled=false;mate.transform.position=exit+ball.transform.parent.TransformDirection(inward)*3;mate.capsule.enabled=true;mate.ResetLocomotion();Physics.SyncTransforms();
   float heading=Quaternion.LookRotation(Vector3.ProjectOnPlane(mate.transform.position-holder.transform.position,Vector3.up)).eulerAngles.y;
   Check(ball.BeginPassCharge(holder,heading,ball.Defense.play)&&ball.ReleasePassCharge(holder,heading,ball.Defense.play,ball.PassClock),"restart uses legal teammate pass");yield return Until(()=>match.Running,"inbound release resumes play");
   yield return Until(()=>ball.Held&&ball.Holder!=holder&&match.TeamOf(ball.Holder)==match.TeamOf(holder),"inbound teammate receives physical pass",4);
  }
  IEnumerator Drop(Athlete shooter,Transform hoop,int points){
   var court=ball.transform.parent;var start=hoop.TransformPoint(new Vector3(0,3.65f,0));
   var feet=hoop.TransformPoint(new Vector3(0,.07f,points==3?8:4));shooter.capsule.enabled=false;shooter.transform.position=feet;shooter.capsule.enabled=true;shooter.ResetLocomotion();
   ball.Place(court.InverseTransformPoint(start),Quaternion.identity,Vector3.down*3,Vector3.zero);match.Control(shooter);Check(match.Release(shooter),"physical shot registered before deadline");
   typeof(BasketballBall).GetMethod("BeginAttempt",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(ball,new object[]{hoop,shooter,start});
   yield return Until(()=>match.State.Phase==BasketballMatchPhase.Restart||match.State.Phase==BasketballMatchPhase.Finished,"physical hoop crossing resolved",3);
  }
  IEnumerator Guest(){
   Check(!match.StartMatch(),"guest cannot start match");
   yield return Until(()=>match.Snapshot.phase==BasketballMatchPhase.TeamSelection,"guest sees team selection",15);
   match.RequestTeam(FootballTeam.B);yield return Until(()=>match.TeamOf(actor)==FootballTeam.B,"guest team choice replicated");
   yield return Until(()=>match.Running,"guest sees regulation",15);
   Check(match.AttackingHoop(actor)==ball.SelectHoop(actor.transform.position,180,actor),"guest aims at assigned hoop");
   Check(!MenuMatchRules.SetMinutes(10),"guest cannot change duration");
   bool restart=false,clock=false;float end=Time.realtimeSinceStartup+50;
   while(Time.realtimeSinceStartup<end&&match.Snapshot.scoreB<2){var s=match.Snapshot;restart|=s.phase==BasketballMatchPhase.Restart&&s.scoreA==3;clock|=s.restart==BasketballRestart.ShotClock;yield return null;}
   Check(match.Snapshot.scoreA==3&&match.Snapshot.scoreB==2,"guest receives both team scores");Check(restart&&clock,"guest receives score restart and shot clock turnover");
   yield return Until(()=>match.Running,"guest sees resumed play");yield return Capture("guest-regulation");
   var task=app.rooms.Leave();while(!task.IsCompleted)yield return null;Check(!app.rooms.Connected,"guest leaves cleanly");
  }
  IEnumerator Football(){
   var m=FootballMatch.Instance;
   if(!app.rooms.Connected){Check(!m.StartMatch()&&m.State.Phase==FootballMatchPhase.Idle,"offline football stays sandbox");yield break;}
   if(!app.rooms.Host){Check(!m.StartMatch(),"football guest cannot start");yield return Until(()=>m.Running,"guest sees football regulation",20);yield return new WaitForSeconds(1);var leave=app.rooms.Leave();while(!leave.IsCompleted)yield return null;yield break;}
   m.rules=Instantiate(m.rules);m.rules.teamSelectionSeconds=1;m.rules.kickoffSeconds=.2f;
   Check(m.StartMatch(),"football room starts");yield return Until(()=>m.Running,"football regulation");
   yield return Until(()=>m.State.Phase==FootballMatchPhase.Cancelled,"football guest departure voids immediately",15);
   Check(m.State.Result==FootballMatchResult.None&&!m.State.Goal(FootballTeam.A,m.Now),"football void blocks scoring and awards no winner");yield return Capture("football-void");
  }
  IEnumerator Capture(string name){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)yield break;
   yield return null;yield return new WaitForEndOfFrame();var camera=Camera.main;var canvas=FindObjectsByType<Canvas>(FindObjectsSortMode.None).First(c=>c.name=="Mobile UI");var mode=canvas.renderMode;var oldCamera=canvas.worldCamera;float plane=canvas.planeDistance;
   canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var old=RenderTexture.active;var target=camera.targetTexture;
   camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);camera.targetTexture=target;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);canvas.renderMode=mode;canvas.worldCamera=oldCamera;canvas.planeDistance=plane;Canvas.ForceUpdateCanvases();
  }
  void Finish(){DevelopmentProbe.TurnCommandActive=false;DevelopmentProbe.TurnCommand=default;File.AppendAllText(Path.Combine(folder,"results.txt"),"ROOM_MATCH_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
