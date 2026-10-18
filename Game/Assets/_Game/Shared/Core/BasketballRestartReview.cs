#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Netcode.Components;
using UnityEngine;

namespace WhatTheFish {
 public sealed class BasketballRestartReview:MonoBehaviour {
  string folder;bool failed,sampling;int checks;AppRoot app;BasketballMatch match;BasketballBall ball;Athlete[] roster;
  readonly Dictionary<Athlete,Vector3> previous=new();float largestStep;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-basketballRestartReview");if(i>=0&&i+1<args.Length)new GameObject("Basketball continuous restart review").AddComponent<BasketballRestartReview>().folder=args[i+1];}
  void Check(bool ok,string message){checks++;failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+message+"\n");}
  IEnumerator Until(Func<bool> test,string message,float seconds=12){float end=Time.realtimeSinceStartup+seconds;while(!test()&&Time.realtimeSinceStartup<end)yield return null;Check(test(),message);}
  void FixedUpdate(){
   if(!match||!ball||!ball.Authority||roster==null)return;
   if(!app.rooms.Connected)foreach(var a in roster)if(a&&a!=app.LocalAthlete)a.Simulate(default,Time.fixedDeltaTime);
   if(sampling)foreach(var a in roster){if(previous.TryGetValue(a,out var p))largestStep=Mathf.Max(largestStep,Vector3.ProjectOnPlane(a.transform.position-p,Vector3.up).magnitude);previous[a]=a.transform.position;}
  }
  void Place(Athlete a,Vector3 local){var p=ball.transform.parent.TransformPoint(local);a.capsule.enabled=false;a.transform.position=p;a.capsule.enabled=true;a.ResetLocomotion();var n=a.GetComponent<NetworkTransform>();if(n&&n.IsSpawned)n.Teleport(p,a.transform.rotation,a.transform.localScale);Physics.SyncTransforms();}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   yield return Until(()=>DevelopmentProbe.ReadyToReviewSport&&BasketballMatch.Instance,"court ready",90);
   app=AppRoot.Instance;match=BasketballMatch.Instance;ball=BasketballBall.Active;if(!app||!match||!ball){Finish();yield break;}
   DevelopmentProbe.Driving=false;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;app.view.mode=2;
   if(!ball.Authority){yield return Guest();Finish();yield break;}
   RoomMatchRuleChecks.Run(Check);
   Check(app.rooms.Connected?BasketballBots.Instance.FillRoom(3):BasketballBots.Instance.StartSolo(3),"six-player fixture");yield return null;
   roster=BasketballBots.Roster.OrderBy(BasketballMatch.PlayerId).ToArray();foreach(var a in roster){var bot=a.GetComponent<BasketballBot>();if(bot)bot.enabled=false;}
   Check(roster.Length==6&&roster.Contains(app.LocalAthlete),"fixture includes the actual local player and exactly six participants");
   Check(app.rooms.Connected?match.StartMatch():match.ReviewStart(roster,600,.1),"match starts");
   for(int i=0;i<roster.Length;i++)Check(match.ChooseTeam(roster[i],i<3?FootballTeam.A:FootballTeam.B,match.Snapshot.revision),"team slot "+i);
   if(!app.rooms.Connected)Check(match.ConfirmSoloTeam(),"solo confirmation");yield return Until(()=>match.Running,"regulation begins",20);
   ball.autoPickup=false;
   yield return BlockedInbounds();
   if(failed){Finish();yield break;}
   // Keep the scoring rotation checks independent of the boundary fixtures.
   match.Clear();Check(app.rooms.Connected?match.StartMatch():match.ReviewStart(roster,600,.1),"scoring fixture restarts");
   for(int i=0;i<roster.Length;i++)Check(match.ChooseTeam(roster[i],i<3?FootballTeam.A:FootballTeam.B,match.Snapshot.revision),"scoring team slot "+i);
   if(!app.rooms.Connected)Check(match.ConfirmSoloTeam(),"scoring solo confirmation");yield return Until(()=>match.Running,"scoring regulation begins",20);
   var next=new Dictionary<FootballTeam,int>();
   for(int trial=0;trial<6;trial++){
    var scoring=trial<3?FootballTeam.A:FootballTeam.B;var receiving=BasketballMatchState.Other(scoring);float attack=scoring==FootballTeam.A?1:-1;
    var scorers=roster.Where(a=>match.TeamOf(a)==scoring).ToArray();var receivers=roster.Where(a=>match.TeamOf(a)==receiving).ToArray();
    sampling=false;ball.ResetHome();DevelopmentProbe.TurnCommand=default;
    for(int i=0;i<scorers.Length;i++)Place(scorers[i],new Vector3(-4+i*4,.07f,attack*(4+i*3)));
    for(int i=0;i<receivers.Length;i++)Place(receivers[i],new Vector3(-4+i*4,.07f,-attack*4));
    if(trial==0)Place(receivers[2],new Vector3(-4,.07f,attack*1.7f)); // A stationary teammate blocks the scorer's direct return path.
    bool quick=trial==2;
    if(quick){for(int i=0;i<scorers.Length;i++)Place(scorers[i],new Vector3(-4+i*4,.07f,attack*12));Place(receivers[2],new Vector3(attack*2,.07f,attack*13));}
    var before=roster.ToDictionary(a=>a,a=>a.transform.position);uint revision=match.Snapshot.revision;
    var hoop=match.Hoop(scoring);var start=hoop.TransformPoint(new Vector3(0,3.65f,0));ball.Place(ball.transform.parent.InverseTransformPoint(start),Quaternion.identity,Vector3.down*3,Vector3.zero);
    match.Control(scorers[0]);Check(match.Release(scorers[0]),"legal scoring release "+trial);
    typeof(BasketballBall).GetMethod("BeginAttempt",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(ball,new object[]{hoop,scorers[0],start});
    largestStep=0;previous.Clear();foreach(var a in roster)previous[a]=a.transform.position;sampling=true;
    yield return Until(()=>match.Snapshot.phase==BasketballMatchPhase.Restart,"physical basket starts inbound "+trial,4);
    next.TryGetValue(receiving,out int index);var inbound=receivers[index];next[receiving]=index+1;
    Check(match.Snapshot.inbounder==BasketballMatch.PlayerId(inbound),"sequential inbound assignment "+receiving+" #"+index);
    Check(match.Snapshot.revision==revision&&!BasketballMatch.BlocksMovement,"no global input reset or movement freeze");
    Check(!ball.Held&&match.RetrievingInbound(inbound),"inbounder must run to collect before possession");
    if(app.rooms.Connected&&trial==1){match.RestoreRoom();Check(match.Snapshot.inbounder==BasketballMatch.PlayerId(inbound)&&match.RetrievingInbound(inbound),"room checkpoint restores collection without advancing turn");}
    Check(scorers.All(match.AutomaticMovement),"only scoring team returns automatically");
    Check(receivers.Where(a=>a!=inbound).All(a=>!match.AutomaticMovement(a)&&Vector3.Distance(before[a],a.transform.position)<.08f),"other receiving players keep their positions and control");
    Check(match.MovementHint(inbound).Contains("INBOUND")&&scorers.All(a=>match.MovementHint(a).Contains("YOUR HALF")),"distinct movement explanations");
    Check(!inbound.CanRequestJump&&scorers.All(a=>!a.CanRequestJump),"automatic runners cannot jump out of their run");
    var free=receivers.First(a=>a!=inbound);var freeBefore=free.transform.position;free.Simulate(new PlayerCommand{move=Vector2.right,heading=0},.12f);
    Check(free.CanRequestJump&&Vector3.Distance(freeBefore,free.transform.position)>.1f,"receiving teammate can move and jump during collection");
    var controlled=scorers[0];var p=controlled.transform.position;controlled.Simulate(new PlayerCommand{move=Vector2.up,heading=attack>0?0:180,jump=true,shoot=true,block=true},.1f);
    Check((controlled.transform.position.z-p.z)*attack<0&&!controlled.Airborne,"opposing player input cannot override return run");
    if(!quick)yield return Capture("automatic-"+trial);
    double clock=match.Remaining;yield return Until(()=>match.RestartReady,"runner collects outside baseline "+trial);
    if(!match.RestartReady){Check(false,"collector="+inbound.transform.position+" ball="+ball.transform.position+" auto="+match.AutomaticMovement(inbound));Finish();yield break;}
    Check(ball.IsInbound(inbound)&&!match.AutomaticMovement(inbound),"collector becomes the standing passer");
    Check(Math.Abs(match.Remaining-clock)<.01,"playing clock preserved through collection");
    if(quick)Check(scorers.Any(match.AutomaticMovement),"inbound can become ready before return runs finish");
    else {yield return Until(()=>scorers.All(a=>!match.AutomaticMovement(a)),"each scorer completes their own return");Check(scorers.All(a=>ball.transform.parent.InverseTransformPoint(a.transform.position).z*attack<=-a.capsule.radius-a.capsule.skinWidth+.015f),"scoring players regain controls individually inside their half's body clearance");}
    Check(largestStep<2.1f,"no reset teleport beyond the capped 0.25s motor step maxStep="+largestStep.ToString("F3"));sampling=false;
    var local=ball.transform.parent.InverseTransformPoint(inbound.transform.position);
    Check(local.z*attack>BasketballBall.PaintedCourtLimits.y,"collector is outside the scored baseline");
    var locked=inbound.transform.position;inbound.Simulate(new PlayerCommand{move=Vector2.one,jump=true,sprint=true},.1f);Check(Vector3.Distance(locked,inbound.transform.position)<.02f&&!ball.CanShoot(inbound),"inbound feet locked until physical pass");
    var mate=receivers.First(a=>a!=inbound);Place(mate,new Vector3(local.x,.07f,local.z-attack*3));
    float heading=Quaternion.LookRotation(Vector3.ProjectOnPlane(mate.transform.position-inbound.transform.position,Vector3.up)).eulerAngles.y;
    Check(ball.BeginPassCharge(inbound,heading,ball.Defense.play),"pass aim starts");yield return new WaitForSeconds(.1f);
    ball.autoPickup=true;Check(ball.ReleasePassCharge(inbound,heading,ball.Defense.play,ball.PassClock),"inbound pass accepted");
    yield return Until(()=>match.Running&&!ball.InboundPending,"physical release resumes live ball");
    if(quick){Check(scorers.Any(match.AutomaticMovement),"early inbound does not cancel unfinished return runs");yield return Until(()=>scorers.All(a=>!match.AutomaticMovement(a)),"return runs finish during live play");}
    yield return Until(()=>ball.Holder==mate,"teammate catches inbound",5);ball.autoPickup=false;
    Check(match.State.ShotRemaining(match.Now)>17,"new possession has full shot clock");
   }
   // Departure while returning must release every control lock.
   ball.ResetHome();match.Control(roster[0]);match.Basket(match.Hoop(FootballTeam.A),2);match.AfterBallStep();Check(match.Snapshot.automaticPlayers.Length>0,"cancellation fixture has a running collector");
   match.Cancel();Check(roster.All(a=>!match.AutomaticMovement(a))&&match.Snapshot.automaticPlayers.Length==0,"cancellation clears all automatic control");
   Check(PlayerCollisionsRestored(),"cancellation leaves player collisions restored");
   File.WriteAllText(Path.Combine(Directory.GetParent(folder).FullName,"host-done"),"done");yield return new WaitForSeconds(2);Finish();
  }
  bool PlayerCollisionsRestored()=>roster.All(a=>roster.All(b=>a==b||!Physics.GetIgnoreCollision(a.capsule,b.capsule)));
  IEnumerator BlockedInbounds(){
   var limits=BasketballBall.PaintedCourtLimits;
   var exits=new[]{new Vector3(limits.x,.07f,2),new Vector3(-limits.x,.07f,-2),new Vector3(3,.07f,limits.y),new Vector3(-limits.x,.07f,-limits.y+.6f)};
   for(int trial=0;trial<exits.Length;trial++){
    sampling=false;ball.ResetHome();DevelopmentProbe.TurnCommand=default;
    for(int i=0;i<roster.Length;i++)Place(roster[i],new Vector3(-3+i%3*3,.07f,i<3?-4:4));
    match.Control(roster[0]);Check(match.Boundary(exits[trial]),"blocked boundary accepted "+trial);match.AfterBallStep();
    var inbound=roster.Single(a=>BasketballMatch.PlayerId(a)==match.Snapshot.inbounder);
    Check(ball.TryInboundSpot(inbound,exits[trial],out var spot,out var inward),"blocked boundary has legal spot "+trial);
    var approach=ball.transform.parent.InverseTransformPoint(spot);approach.x=Mathf.Clamp(approach.x,-limits.x+1.2f,limits.x-1.2f);approach.z=Mathf.Clamp(approach.z,-limits.y+1.2f,limits.y-1.2f);
    var localInward=ball.transform.parent.InverseTransformDirection(inward);var side=Vector3.Cross(Vector3.up,localInward);
    Place(inbound,approach+localInward*3);
    // Occupy the required waypoint: going around cannot reach its arrival
    // radius. Exercise both teams, a crowded baseline and a boundary corner.
    var blocker=roster.First(a=>a!=inbound&&(trial==0?match.TeamOf(a)==match.TeamOf(inbound):match.TeamOf(a)!=match.TeamOf(inbound)));
    Place(blocker,approach);
    if(trial>=2){var crowd=roster.Where(a=>a!=inbound&&a!=blocker).Take(2).ToArray();Place(crowd[0],approach+side*.8f);Place(crowd[1],approach-side*.8f);}
    double clock=match.Remaining;var possession=match.State.Possession;var blockerStart=blocker.transform.position;
    var preserved=roster.First(a=>a!=inbound&&a!=blocker&&Vector3.Distance(a.transform.position,blockerStart)>2);
    Physics.IgnoreCollision(inbound.capsule,preserved.capsule,true);
    largestStep=0;previous.Clear();sampling=true;
    yield return Until(()=>Physics.GetIgnoreCollision(inbound.capsule,blocker.capsule),"stalled collector gets body-only clearance "+trial,8);
    Check(!ball.Held&&match.RetrievingInbound(inbound),"clearance still requires physical collection "+trial);
    var floor=ball.transform.parent.GetComponentsInChildren<Collider>().FirstOrDefault(c=>c.gameObject.layer==8&&!c.isTrigger);
    Check(floor&&!Physics.GetIgnoreCollision(inbound.capsule,floor)&&inbound.capsule.enabled,"collector retains scenery collisions "+trial);
    if(app.rooms.Connected&&trial==2){
     match.RestoreRoom();Check(!Physics.GetIgnoreCollision(inbound.capsule,blocker.capsule),"room restore cleans previous clearance");
     Check(match.Snapshot.inbounder==BasketballMatch.PlayerId(inbound)&&match.RetrievingInbound(inbound),"room restore retains blocked collector duty");
    }
    if(trial==3){
     match.Cancel();Check(!Physics.GetIgnoreCollision(inbound.capsule,blocker.capsule)&&roster.All(a=>!match.AutomaticMovement(a)),"cancel during clearance restores collisions and controls");
    }else{
     yield return Until(()=>match.RestartReady,"blocked collector reaches ball "+trial,8);
     Check(ball.IsInbound(inbound)&&!match.AutomaticMovement(inbound),"blocked collector becomes inbound passer "+trial);
     Check(match.State.Possession==possession&&Math.Abs(match.Remaining-clock)<.01,"blocked retrieval preserves possession and clock "+trial);
     Check(!Physics.GetIgnoreCollision(inbound.capsule,blocker.capsule),"pickup restores opponent and teammate collisions "+trial);
     Check(Vector3.ProjectOnPlane(blocker.transform.position-blockerStart,Vector3.up).magnitude<.1f&&!match.AutomaticMovement(blocker),"blocker retains position and control "+trial);
     if(trial==0)yield return Capture("blocked-inbound-recovered");
     // The full scoring suite below covers the physical pass itself.
     Check(match.OpenInbound(inbound),"recovered inbound can resume play "+trial);
    }
    Check(Physics.GetIgnoreCollision(inbound.capsule,preserved.capsule),"pre-existing collision exception preserved "+trial);
    Physics.IgnoreCollision(inbound.capsule,preserved.capsule,false);
    Check(PlayerCollisionsRestored(),"no retrieval collision exceptions leak "+trial);
    Check(largestStep<2.1f,"blocked retrieval uses continuous motor movement "+trial+" maxStep="+largestStep.ToString("F3"));sampling=false;
    if(failed)yield break;
   }
  }
  IEnumerator Guest(){
   bool returnSeen=false,inboundSeen=false,unlocked=false;var own=app.LocalAthlete;string done=Path.Combine(Directory.GetParent(folder).FullName,"host-done");float end=Time.realtimeSinceStartup+160;
   while(!File.Exists(done)&&Time.realtimeSinceStartup<end){
    if(match.AutomaticMovement(own)){
     DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.one,sprint=true,jump=true,block=true};
     if(match.RetrievingInbound(own)&&!inboundSeen){inboundSeen=true;yield return Capture("guest-inbound-hint");}
     else if(!match.RetrievingInbound(own)&&!returnSeen){returnSeen=true;yield return Capture("guest-return-hint");}
    }else {DevelopmentProbe.TurnCommand=default;if(returnSeen&&match.Active)unlocked=true;}
    yield return null;
   }
   Check(File.Exists(done),"host fixture completed");Check(returnSeen,"guest receives automatic return");Check(inboundSeen,"guest receives automatic inbound duty");Check(unlocked,"guest receives individual control return");
   Check(!ball.BeginInbound(own,Vector3.zero)&&!match.OpenInbound(own),"guest cannot author restart possession");
  }
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();var camera=Camera.main;var canvas=BasketballHUD.Instance.GetComponentInParent<Canvas>();var mode=canvas.renderMode;var oldCamera=canvas.worldCamera;float plane=canvas.planeDistance;
   canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var old=RenderTexture.active;var target=camera.targetTexture;
   camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var texture=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);texture.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),texture.EncodeToPNG());Destroy(texture);camera.targetTexture=target;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);canvas.renderMode=mode;canvas.worldCamera=oldCamera;canvas.planeDistance=plane;
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"BASKETBALL_RESTART_COMPLETE success="+!failed+" checks="+checks+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
