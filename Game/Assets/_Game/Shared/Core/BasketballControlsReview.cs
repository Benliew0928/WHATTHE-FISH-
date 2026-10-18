#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 public sealed class BasketballControlsReview:MonoBehaviour {
  string folder;bool failed;AppRoot app;BasketballBall ball;BasketballMatch match;Athlete actor;Athlete[] roster;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-basketballControlsReview");if(i>=0&&i+1<a.Length)new GameObject("Basketball controls review").AddComponent<BasketballControlsReview>().folder=a[i+1];}
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  void FixedUpdate(){if(ball&&ball.Authority&&!app.rooms.Connected&&roster!=null)foreach(var a in roster)if(a&&a!=actor&&match.AutomaticMovement(a))a.Simulate(default,Time.fixedDeltaTime);}
  IEnumerator Until(Func<bool> condition,string label,float seconds=8){float end=Time.realtimeSinceStartup+seconds;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Check(condition(),label);}
  void Place(Athlete a,Vector3 point,float heading=0){var world=ball.transform.parent.TransformPoint(point);var rotation=ball.transform.parent.rotation*Quaternion.Euler(0,heading,0);a.capsule.enabled=false;a.transform.SetPositionAndRotation(world,rotation);a.ResetLocomotion();a.capsule.enabled=true;var net=a.GetComponent<NetworkTransform>();if(net&&net.IsSpawned)net.Teleport(world,rotation,a.transform.localScale);Physics.SyncTransforms();}
  Vector3 Local(Athlete a)=>ball.transform.parent.InverseTransformPoint(a.transform.position);
  float Heading(Athlete from,Athlete to)=>Quaternion.LookRotation(Vector3.ProjectOnPlane(to.transform.position-from.transform.position,Vector3.up)).eulerAngles.y;
  void Park(){app.view.ClearMatchInput();DevelopmentProbe.TurnCommand=default;ball.ResetHome();for(int i=0;i<roster.Length;i++)Place(roster[i],new Vector3(i%2==0?-5:5,.07f,-10+i*3));ball.autoPickup=false;}
  bool HoldGuard(bool block,int pointer=73){
   var button=BasketballHUD.Instance.GetComponentsInChildren<BasketballDefenseButton>(true).Single();button.Refresh();
   var touch=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=pointer,position=RectTransformUtility.WorldToScreenPoint(null,button.transform.position)};
   button.OnPointerDown(touch);if(block){touch.position=RectTransformUtility.WorldToScreenPoint(null,button.transform.TransformPoint(new Vector2(0,130)));button.OnDrag(touch);}
   return app.view.GuardHeld&&(!block||app.view.AutoBlockHeld);
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   yield return Until(()=>DevelopmentProbe.ReadyToReviewSport&&BasketballBall.Active,"court ready",85);
   app=AppRoot.Instance;ball=BasketballBall.Active;match=BasketballMatch.Instance;actor=app.LocalAthlete;
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;app.view.mode=1;yield return new WaitForSeconds(.5f);
   if(!ball.Authority){yield return Guest();Finish();yield break;}
   if(app.rooms.Connected){Check(BasketballBots.Instance.FillRoom(3),"host fills 3v3 fixture");yield return new WaitForSeconds(.5f);roster=BasketballBots.Roster;foreach(var a in roster){var bot=a.GetComponent<BasketballBot>();if(bot)bot.enabled=false;}Check(match.StartMatch(),"host starts fixture");}
   else {roster=new Athlete[6];roster[0]=actor;for(int i=1;i<6;i++){roster[i]=Instantiate(app.athletePrefab).GetComponent<Athlete>();roster[i].Setup();}Check(match.ReviewStart(roster,600,2),"offline 3v3 fixture starts");}
   roster=roster.OrderBy(a=>a==actor?0:1).ThenBy(BasketballMatch.PlayerId).ToArray();
   uint revision=match.Snapshot.revision;for(int i=0;i<roster.Length;i++)Check(match.ChooseTeam(roster[i],i<3?FootballTeam.A:FootballTeam.B,revision),"fixture team "+i);
   yield return Until(()=>match.Running,"regulation starts",16);
   yield return Passing();yield return Guard();yield return AutomaticBlocks();yield return RemoteGuard();yield return Inbounds();
   File.WriteAllText(Path.Combine(Directory.GetParent(folder).FullName,"host-done"),"done");yield return new WaitForSeconds(1);Finish();
  }
  IEnumerator Passing(){
   Park();Place(actor,new Vector3(0,.07f,0));Place(roster[1],new Vector3(0,.07f,6));Place(roster[2],new Vector3(.1f,.07f,10));Place(roster[3],new Vector3(0,.07f,3));ball.GiveMatchPossession(actor);match.Control(actor);
   Check(ball.SelectPassReceiver(actor,0)==roster[1],"camera ray selects nearest teammate and ignores closer opponent");
   float radius=BasketballBall.PassLockRadius(roster[1]);float inside=Mathf.Atan2(radius*.9f,6)*Mathf.Rad2Deg,outside=Mathf.Atan2(radius*1.15f,6)*Mathf.Rad2Deg;
   Check(ball.SelectPassReceiver(actor,inside,roster[1])==roster[1],"lock retained within two player widths");
   Place(roster[2],new Vector3(-5,.07f,10));Check(ball.SelectPassReceiver(actor,outside,roster[1])==null,"camera beyond two widths releases to free aim");
   Place(roster[3],new Vector3(5,.07f,3));
   Check(ball.BeginPassCharge(actor,0,ball.Defense.play),"camera pass charge starts");yield return new WaitForSeconds(.4f);
   Check(ball.PassAim.locked&&ball.PassReceiver==roster[1],"selected receiver is authoritative");yield return Capture("pass-lock");
   ball.AimPass(actor,80);Check(!ball.PassAim.locked&&Mathf.Abs(ball.PassAim.heading-80)<.001f,"free pass follows camera heading");ball.AimPass(actor,0);
   foreach(float bend in new[]{-1f,0f,1f}){var start=actor.transform.position+Vector3.up;var end=roster[1].transform.position+Vector3.up*.9f;var path=BasketballPassPath.ToReceiver(start,end,1,bend,.07f);Check(Vector3.Distance(path.Point(path.duration),end)<.01f,"physical pass endpoint meets selected receiver bend="+bend);}
   ball.autoPickup=true;Check(ball.ReleasePassCharge(actor,0,ball.Defense.play,ball.PassClock),"locked pass commits");yield return Until(()=>ball.Holder==roster[1],"selected teammate catches actual pass",4);
  }
  IEnumerator Guard(){
   Park();var carrier=roster[3];Place(carrier,new Vector3(0,.07f,0));Place(actor,new Vector3(0,.07f,1.1f),0);ball.GiveMatchPossession(carrier);match.Control(carrier);yield return new WaitForSeconds(.2f);
   var button=BasketballHUD.Instance.GetComponentsInChildren<BasketballDefenseButton>(true).Single();button.Refresh();
   Vector2 Point(Vector2 offset)=>RectTransformUtility.WorldToScreenPoint(null,button.transform.TransformPoint(offset));
   var touch=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=71,position=Point(Vector2.zero)};
   button.OnPointerDown(touch);yield return new WaitForSeconds(.65f);
   Check(app.view.GuardHeld&&actor.BasketballMotion.GuardActive&&Vector3.Dot(actor.transform.forward,(carrier.transform.position-actor.transform.position).normalized)>.98f,"guard turns from facing away to carrier");
   Check(!app.view.AutoBlockHeld&&!ball.GuardBlockReady(actor)&&!ball.GetComponent<BasketballPropVisual>().ReviewGuardLight(actor),"plain guard does not arm auto block");
   Check(Mathf.Abs(ball.GuardMovementMultiplier(actor)-.7f)<.0001f&&Mathf.Abs(ball.GuardMovementMultiplier(carrier)-.7f)<.0001f,"both players slowed exactly thirty percent");
   Check(ball.BeginShotCharge(carrier,0),"guarded shot begins");yield return new WaitForSeconds(.2f);
   float normal=BasketballBall.ChargePeriod(ball.ChargeDistance(carrier,0));Check(Mathf.Abs(normal/ball.Charge.period-1.3f)<.001f,"guarded meter spins thirty percent faster");
   Check(Mathf.Abs(BasketballShotOdds.Chance(4,.65f,pressure:1)-.75f)<.0001f&&BasketballShotOdds.Chance(26,.65f,pressure:1)==0,"twenty percentage point penalty clamps at zero");
   yield return Capture("guard-controls");ball.CancelShotCharge(carrier);yield return new WaitForSeconds(.3f);
   button.OnPointerUp(new PointerEventData(EventSystem.current){pointerId=72});Check(app.view.GuardHeld,"other finger cannot release guard");
   touch.position=Point(new Vector2(-109,-30));button.OnPointerExit(touch);button.OnDrag(touch);yield return new WaitForSeconds(.06f);Check(app.view.GuardHeld&&actor.BasketballMotion.GuardActive,"left swipe keeps guard through steal animation");
   yield return Until(()=>!ball.Held,"close guarded swipe definitely strips the ball",1);Check(ball.StealCount>0,"host recorded successful steal");button.OnPointerUp(touch);
   Park();Place(carrier,new Vector3(0,.07f,0));Place(actor,new Vector3(0,.07f,3));ball.GiveMatchPossession(carrier);yield return new WaitForSeconds(.3f);button.Refresh();touch.position=Point(Vector2.zero);button.OnPointerDown(touch);yield return new WaitForSeconds(.2f);
   for(int i=0;i<2;i++){uint before=actor.BasketballMotion.State.sequence;touch.position=Point(new Vector2(-109,-30));button.OnDrag(touch);yield return new WaitForSeconds(.2f);Check(actor.BasketballMotion.Action==BasketballAction.Steal&&actor.BasketballMotion.State.sequence>before&&app.view.GuardHeld,"repeat swipe spends steal and retains guard "+i);yield return new WaitForSeconds(.6f);touch.position=Point(Vector2.zero);button.OnDrag(touch);yield return null;}
   Check(ball.Holder==carrier,"out-of-range swipes miss");touch.position=Point(new Vector2(0,130));button.OnDrag(touch);yield return new WaitForSeconds(.15f);
   Check(app.view.AutoBlockHeld&&ball.GuardBlockReady(actor),"upward held swipe arms auto block");
   Check(!app.view.BeginSteal(72)&&!app.view.RequestGuardSteal(71)&&!ball.TrySteal(actor,0)&&!ball.CanSteal(actor),"second finger and host steal rejected while block armed");
   Check(!app.view.SetGuardBlock(false,72)&&app.view.AutoBlockHeld,"other finger cannot disarm block mode");
   yield return Capture("guard-block-ready");touch.position=Point(Vector2.zero);button.OnDrag(touch);yield return new WaitForSeconds(.15f);
   Check(!app.view.AutoBlockHeld&&!ball.GuardBlockReady(actor)&&ball.CanSteal(actor),"return to centre disarms block and restores steal");
   touch.position=Point(new Vector2(-109,-30));button.OnDrag(touch);yield return new WaitForSeconds(.06f);touch.position=Point(new Vector2(0,130));button.OnDrag(touch);yield return new WaitForSeconds(.06f);
   Check(actor.BasketballMotion.Action==BasketballAction.Steal&&!ball.GuardBlockReady(actor),"up swipe cannot interrupt steal recovery or block during steal");
   yield return new WaitForSeconds(.75f);Check(ball.GuardBlockReady(actor),"held block mode arms only after steal recovery ends");
   button.OnPointerUp(touch);yield return new WaitForSeconds(.1f);Check(!actor.BasketballMotion.Blocking&&!app.view.GuardHeld&&!app.view.AutoBlockHeld,"release clears selected mode without a timed block");yield return new WaitForSeconds(.8f);
   touch.position=Point(Vector2.zero);button.Refresh();button.OnPointerDown(touch);BasketballHUD.Instance.ShowSettings();yield return null;Check(!app.view.GuardHeld&&!button.Tracking,"menu cancels captured guard");BasketballHUD.Instance.CloseMenu();
  }
  IEnumerator AutomaticBlocks(){
   var carrier=roster[3];var visual=ball.GetComponent<BasketballPropVisual>();
   foreach(var offset in new[]{new Vector3(0,0,2f),new Vector3(2.49f,0,0),new Vector3(0,0,-2f)}){
    Park();Place(carrier,new Vector3(0,.07f,0));Place(actor,new Vector3(0,.07f,0)+offset);ball.GiveMatchPossession(carrier);match.Control(carrier);yield return new WaitForSeconds(.2f);
    Check(HoldGuard(true),"upward swipe selects auto block "+offset);yield return new WaitForSeconds(.3f);
    Check(ball.GuardBlockReady(actor)&&visual.ReviewGuardLight(actor),"ready stance and visible light agree "+offset);
    uint blocks=ball.Defense.blocks,shots=ball.ShotCount;var score=ball.Score;Check(ball.BeginShotCharge(carrier,0),"opponent can charge near ready guard");yield return new WaitForSeconds(.7f);
    Check(ball.Defense.blocks==blocks&&ball.Holder==carrier,"charging fake does not auto block");
    if(offset.z>0)yield return Capture("auto-block-ready");
    Check(ball.ReleaseShotCharge(carrier,0),"opponent commits guarded shot");yield return Until(()=>ball.Defense.blocks==blocks+1,"close shot automatically blocked while holding",1);
    Check(ball.Defense.blocker==BasketballMatch.PlayerId(actor)&&!ball.Held&&!ball.Body.isKinematic&&ball.Body.linearVelocity.magnitude>1,"correct defender produces physical loose ball");
    Check(ball.ShotCount==shots&&ball.Score.attempts==score.attempts&&ball.Score.points==score.points&&!ball.ActionQueued,"blocked attempt cannot launch or score");
    if(offset.z>0)yield return Capture("auto-block-contact");
    yield return new WaitForSeconds(.85f);Check(ball.Defense.blocks==blocks+1&&!ball.GuardBlockReady(actor)&&!visual.ReviewGuardLight(actor),"one block only and readiness clears with possession");
   }
   foreach(string scenario in new[]{"plain guard","outside range","released hold","wall","teammate","pass"}){
    Park();Place(carrier,new Vector3(0,.07f,0));Place(actor,new Vector3(scenario=="outside range"?2.6f:1.1f,.07f,0));ball.GiveMatchPossession(carrier);match.Control(actor);match.Control(carrier);yield return new WaitForSeconds(.2f);
    Check(HoldGuard(scenario!="plain guard"),"negative fixture hold "+scenario);yield return new WaitForSeconds(.25f);
    GameObject wall=null;
    if(scenario=="released hold"){app.view.EndGuard(73);yield return new WaitForSeconds(.3f);Check(!ball.GuardBlockReady(actor)&&!visual.ReviewGuardLight(actor),"release extinguishes readiness");}
    if(scenario=="wall"){wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=8;wall.transform.SetParent(ball.transform.parent,false);wall.transform.localPosition=new Vector3(.55f,.8f,0);wall.transform.localScale=new Vector3(.06f,1.6f,1.6f);Physics.SyncTransforms();}
    if(scenario=="teammate"){app.view.EndGuard(73);Place(actor,new Vector3(4,.07f,0));Place(roster[4],new Vector3(1.1f,.07f,0));roster[4].BasketballMotion.Guard(270,true);Check(!ball.GuardBlockReady(roster[4]),"teammate cannot arm an opposing block");}
    uint blocks=ball.Defense.blocks,shots=ball.ShotCount,passes=ball.PassCount;
    Check(scenario=="pass"?ball.TryPass(carrier,0):ball.TryShoot(carrier,0),"negative fixture commits "+scenario);
    yield return Until(()=>scenario=="pass"?ball.PassCount>passes:ball.ShotCount>shots,"unblocked release "+scenario,3);
    Check(ball.Defense.blocks==blocks,"no auto block for "+scenario);if(wall)Destroy(wall);
   }
  }
  IEnumerator Inbounds(){
   Park();match.Control(actor);var limits=BasketballBall.PaintedCourtLimits;
   foreach(var exit in new[]{new Vector3(limits.x,.07f,3),new Vector3(-limits.x,.07f,-4),new Vector3(3,.07f,limits.y),new Vector3(-3,.07f,-limits.y)}){
    // Continuous restarts retain everyone, so reset this test's receiving lane
    // explicitly instead of relying on the removed formation teleport.
    Park();match.Control(actor);
    match.Touch(actor);Check(match.Boundary(exit),"boundary event accepted "+exit);yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
    Check(match.Snapshot.phase==BasketballMatchPhase.Restart,"restart begins collection");yield return Until(()=>ball.InboundPending,"inbounder reaches crossing");var inbound=ball.Holder;if(!inbound)yield break;
    bool side=Mathf.Abs(exit.x)>limits.x-.01f;var local=Local(inbound);Check((side?Mathf.Abs(local.x)>limits.x:Mathf.Abs(local.z)>limits.y)&&Mathf.Abs(side?local.z-exit.z:local.x-exit.x)<.15f,"inbounder runs outside exact crossing");
    double remaining=match.Remaining;yield return Until(()=>match.RestartReady,"restart setup ready");yield return new WaitForSeconds(.25f);Check(match.Snapshot.phase==BasketballMatchPhase.Restart&&Math.Abs(match.Remaining-remaining)<.001,"clocks stay paused until pass");
    Vector3 before=inbound.transform.position;inbound.Simulate(new PlayerCommand{move=Vector2.one,jump=true,shoot=true},.1f);Check(Vector3.Distance(before,inbound.transform.position)<.01f&&!ball.CanShoot(inbound)&&!inbound.CanRequestJump,"inbounder cannot move jump or shoot");
    var opponent=roster.First(a=>match.TeamOf(a)!=match.TeamOf(inbound));float sign=match.TeamOf(opponent)==FootballTeam.A?-1:1;Place(opponent,new Vector3(0,.07f,sign*.5f));var move=ball.transform.parent.forward*(-sign*3);var constrained=ball.ConstrainPlayerMotion(opponent,move);var target=ball.transform.parent.InverseTransformPoint(opponent.transform.position+constrained);Check(target.z*sign<0,"boundary restart preserves opponent movement without a formation reset");
    var mate=roster.First(a=>a!=inbound&&match.TeamOf(a)==match.TeamOf(inbound));var inward=side?new Vector3(-Mathf.Sign(exit.x),0,0):new Vector3(0,0,-Mathf.Sign(exit.z));Place(mate,exit+inward*3);float heading=Heading(inbound,mate);
    int parked=0;foreach(var other in roster.Where(a=>a!=inbound&&a!=mate))Place(other,new Vector3(-5+parked++*3,.07f,0));
    Check(ball.BeginPassCharge(inbound,heading+180,ball.Defense.play),"inbound may aim");Check(!ball.ReleasePassCharge(inbound,heading+180,ball.Defense.play,ball.PassClock)&&ball.IsInbound(inbound),"outward free inbound rejected");ball.AimPass(inbound,heading);
    yield return Capture("inbound-"+(side?"side":"baseline")+Mathf.Sign(side?exit.x:exit.z));
    ball.autoPickup=true;Check(ball.ReleasePassCharge(inbound,heading,ball.Defense.play,ball.PassClock),"teammate inbound commits");yield return Until(()=>match.Running&&!ball.InboundPending,"pass release opens match");Check(!match.RestrictedHalf(opponent)&&match.State.ShotRemaining(match.Now)>18,"release unlocks half and starts shot clock");yield return Until(()=>ball.Holder==mate,"inbound enters court and teammate receives",4);ball.autoPickup=false;
   }
   Park();var hoop=match.AttackingHoop(actor);var start=hoop.TransformPoint(new Vector3(0,3.65f,0));ball.Place(ball.transform.parent.InverseTransformPoint(start),Quaternion.identity,Vector3.down*3,Vector3.zero);match.Control(actor);Check(match.Release(actor),"score fixture records legal shot");typeof(BasketballBall).GetMethod("BeginAttempt",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(ball,new object[]{hoop,actor,start});yield return Until(()=>match.Snapshot.phase==BasketballMatchPhase.Restart,"physical basket opens conceding baseline restart");
   yield return Until(()=>match.RestartReady,"scored inbounder collects ball");var spot=Local(ball.Holder);float baselineSign=match.Snapshot.possession==FootballTeam.A?-1:1;Check(Mathf.Abs(spot.x-baselineSign*2)<.15f&&spot.z*baselineSign>limits.y&&Mathf.Abs(spot.x)<limits.x*.5f,"score restart right of hoop closer to stand than midpoint");yield return Capture("scored-inbound");
  }
  IEnumerator RemoteGuard(){
   if(!app.rooms.Connected)yield break;
   var remote=roster.First(a=>{var n=a.GetComponent<NetworkAthlete>();return n&&n.IsSpawned&&!n.IsOwner&&n.BotSlot.Value==0;});var carrier=roster[3];
   Park();Place(carrier,new Vector3(0,.07f,0));Place(remote,new Vector3(2.25f,.07f,0));ball.GiveMatchPossession(carrier);match.Control(actor);match.Control(carrier);
   File.WriteAllText(Path.Combine(Directory.GetParent(folder).FullName,"remote-guard"),ball.Defense.play.ToString());
   yield return Until(()=>ball.GuardBlockReady(remote),"guest held input arms server readiness",8);
   Check(!ball.TrySteal(remote,0),"host rejects steal from guest with armed block mode");
   uint blocks=ball.Defense.blocks,shots=ball.ShotCount;Check(ball.TryShoot(carrier,0),"shoot against guest guard");
   yield return Until(()=>ball.Defense.blocks==blocks+1,"server automatically blocks for guest hold",2);
   Check(ball.Defense.blocker==BasketballMatch.PlayerId(remote)&&ball.ShotCount==shots&&!ball.Held,"guest receives sole block credit and shot is cancelled");yield return new WaitForSeconds(.7f);
  }
  IEnumerator Guest(){
   bool guard=false,stealGuard=false,locked=false,inbound=false,readyLight=false,blocked=false;float end=Time.realtimeSinceStartup+175;string done=Path.Combine(Directory.GetParent(folder).FullName,"host-done");
   bool remoteHeld=false;string remoteSignal=Path.Combine(Directory.GetParent(folder).FullName,"remote-guard");
   while(!File.Exists(done)&&Time.realtimeSinceStartup<end){if(!remoteHeld&&File.Exists(remoteSignal)&&uint.TryParse(File.ReadAllText(remoteSignal),out uint play)&&ball.Defense.play==play)remoteHeld=HoldGuard(true,74);foreach(var a in Athlete.Active){guard|=a.BasketballMotion.Guarding;stealGuard|=a.BasketballMotion.GuardActive&&a.BasketballMotion.Action==BasketballAction.Steal;readyLight|=ball.GetComponent<BasketballPropVisual>().ReviewGuardLight(a);if(a.BasketballMotion.Action==BasketballAction.Steal&&a.BasketballMotion.State.guardBlock)Check(false,"steal and block modes never overlap on peer");}blocked|=ball.Defense.blocks>0&&ball.Defense.blocker!=ulong.MaxValue;locked|=ball.PassAim.locked&&ball.PassReceiver;inbound|=ball.InboundPending&&match.Snapshot.phase==BasketballMatchPhase.Restart;yield return null;}
   Check(remoteHeld,"guest sends its own held guard input");
   Check(readyLight&&blocked,"peer receives ready light and authoritative automatic blocks");
   Check(File.Exists(done),"host completed shared fixture");Check(guard&&stealGuard,"peer receives guard and simultaneous steal");Check(locked,"peer resolves selected teammate snapshot");Check(inbound,"peer receives protected inbound state");Check(!ball.TrySteal(actor,0)&&!ball.BeginInbound(actor,Vector3.zero),"guest cannot author possession");
  }
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)yield break;
   var camera=Camera.main;var canvas=BasketballHUD.Instance.GetComponentInParent<Canvas>();var mode=canvas.renderMode;var previousCamera=canvas.worldCamera;float plane=canvas.planeDistance;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();
   var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var old=RenderTexture.active;var target=camera.targetTexture;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);camera.targetTexture=target;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);canvas.renderMode=mode;canvas.worldCamera=previousCamera;canvas.planeDistance=plane;
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"BASKETBALL_CONTROLS_COMPLETE success="+!failed+"\n");Application.Quit(failed?2:0);}
 }
}
#endif
