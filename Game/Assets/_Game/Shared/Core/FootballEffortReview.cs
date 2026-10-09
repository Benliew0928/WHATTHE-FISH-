#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 public sealed class FootballEffortReview:MonoBehaviour {
  string folder,shared;bool failed,capture;AppRoot app;Athlete actor;FootballBall ball;Vector3 home;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-footballEffortReview");if(i<0||i+1>=args.Length)return;var r=new GameObject("Football pace review").AddComponent<FootballEffortReview>();r.folder=args[i+1];r.capture=args.Contains("-effortCapture");}
  void Check(bool ok,string message){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+message+"\n");}
  void Place(Athlete a,Vector3 p,float heading=0){a.capsule.enabled=false;a.transform.SetPositionAndRotation(p,Quaternion.Euler(0,heading,0));a.capsule.enabled=ball.HasAuthority;a.ResetLocomotion();var net=a.GetComponent<NetworkTransform>();if(net&&net.IsSpawned&&ball.HasAuthority)net.Teleport(p,a.transform.rotation,Vector3.one);Physics.SyncTransforms();}
  void Loose(){ball.ResetBall();ball.Body.position=home+Vector3.right*12+Vector3.up*ball.WorldRadius;Physics.SyncTransforms();}
  void Pickup(Athlete a){ball.ResetBall();ball.Body.position=ball.FootPosition(a);ball.Body.linearVelocity=Vector3.zero;Physics.SyncTransforms();ball.RefreshControl(a);Check(ball.CurrentController==a,"fixture acquires football");}
  void Signal(string name){var p=Path.Combine(shared,name);File.WriteAllText(p+".tmp","ready");File.Move(p+".tmp",p);}
  IEnumerator Await(string name){float until=Time.realtimeSinceStartup+20;while(!File.Exists(Path.Combine(shared,name))&&Time.realtimeSinceStartup<until)yield return null;Check(File.Exists(Path.Combine(shared,name)),"peer checkpoint "+name);}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);shared=Directory.GetParent(folder).FullName;File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   float until=Time.realtimeSinceStartup+80;while((!(app=AppRoot.Instance)||!app.Exploring||!app.LocalAthlete||!FootballBall.Instance)&&Time.realtimeSinceStartup<until)yield return null;
   actor=app?app.LocalAthlete:null;ball=FootballBall.Instance;if(!actor||!ball){Check(false,"football initialized");Finish();yield break;}
   yield return new WaitForSeconds(.8f);home=ball.KickoffPosition-Vector3.up*ball.WorldRadius+Vector3.back*5;
   Check(RoomService.ProtocolVersion==38,"football protocol 38");
   if(app.rooms.Connected){if(ball.HasAuthority)yield return Host();else yield return Guest();}else {Rules();yield return Offline();}
   Finish();
  }
  void Rules(){
   foreach(int rate in new[]{20,30,60,120}){
    var effort=new FootballEffort();for(int i=0;i<rate*2;i++)effort.Step(true,true,true,false,1f/rate);
    Check(Mathf.Abs(effort.Stamina-36)<.01f&&effort.Dashing,"two seconds dash drains 64 at "+rate+" FPS");
    for(int i=0;i<rate*2;i++)effort.Step(true,true,true,false,1f/rate);
    Check(effort.Exhausted&&!effort.Dashing,"dash exhausts in short burst at "+rate+" FPS");
    for(int i=0;i<rate*8;i++)effort.Step(true,true,true,false,1f/rate);
    Check(effort.Stamina==100&&effort.Exhausted&&!effort.Dashing,"holding through recovery cannot auto dash at "+rate+" FPS");
    effort.Step(false,true,true,false,1f/rate);effort.Step(true,true,true,false,1f/rate);Check(effort.Dashing,"release then fresh dash at "+rate+" FPS");
    effort.Reset();for(int i=0;i<rate;i++)effort.Step(true,false,true,false,1f/rate);Check(effort.Stamina==100&&!effort.Dashing,"standing does not waste stamina at "+rate+" FPS");
    effort.Step(true,true,true,true,1f/rate);Check(effort.Pressuring&&!effort.Dashing,"pressure wins simultaneous inputs at "+rate+" FPS");
    effort.Reset();for(int i=0;i<rate;i++)effort.Step(true,true,true,false,1f/rate);for(int i=0;i<rate;i++)effort.Step(false,true,true,false,1f/rate);
    Check(Mathf.Abs(effort.Stamina-74)<.02f,"recovery begins after 0.6 seconds and restores 15 per second at "+rate+" FPS");
    effort.Step(true,true,false,false,1f/rate);Check(!effort.Dashing&&!effort.Pressuring,"invalid action state cancels effort at "+rate+" FPS");
   }
   Check(FootballPressure.Strength(Vector3.forward,Vector3.back,Vector3.zero)>.8f,"close facing guard applies pressure");
   Check(FootballPressure.Strength(Vector3.forward,Vector3.forward,Vector3.zero)==0,"facing away has no pressure");
   Check(FootballPressure.Strength(Vector3.forward*3,Vector3.back,Vector3.zero)==0,"distant defender has no pressure");
   Check(FootballPressure.Strength(Vector3.forward+Vector3.up,Vector3.back,Vector3.zero)==0,"different elevation has no pressure");
   var snapshot=new FootballEffortSnapshot{stamina=43,dashing=true};var copy=new FootballEffort();copy.Receive(snapshot);Check(copy.Stamina==43&&copy.Dashing&&copy.Snapshot.Equals(snapshot),"effort presentation reads replicated state");
  }
  IEnumerator Offline(){
   foreach(var run in new[]{false,true}){
    Loose();Place(actor,home);DevelopmentProbe.TurnCommand=new(){move=Vector2.up,sprint=run};yield return new WaitForSeconds(.7f);
    Check(Mathf.Abs(actor.speed-(run?10.5f:6))<.12f,(run?"run":"walk")+" is 150 percent of previous speed: "+actor.speed);
   }
   DevelopmentProbe.TurnCommand=default;Loose();Place(actor,home);DevelopmentProbe.TurnCommand=new(){move=Vector2.up,dash=true};yield return new WaitForSeconds(.7f);
   Check(actor.FootballEffort.Dashing&&Mathf.Abs(actor.speed-15.225f)<.15f,"dash boosts to 15.225 metres per second");
   Check(actor.FootballEffort.Stamina<85,"moving dash consumes host stamina");DevelopmentProbe.TurnCommand=default;
   Place(actor,home);Pickup(actor);DevelopmentProbe.TurnCommand=new(){move=Vector2.up,dash=true};yield return new WaitForSeconds(.7f);
   Check(actor.ControlsFootball&&Mathf.Abs(actor.speed-11.41875f)<.15f,"dash dribble retains ball and possession multiplier");
   Check(Vector3.Distance(ball.transform.position,ball.FootPosition(actor))<.03f,"ball stays on foot during fast dribble");
   DevelopmentProbe.TurnCommand=new(){move=Vector2.up,dash=true,charging=true};yield return new WaitForSeconds(.3f);
   Check(!actor.FootballEffort.Dashing&&Mathf.Abs(actor.speed-3.6f)<.15f,"charging cancels dash and moves at scaled charge speed");DevelopmentProbe.TurnCommand=default;
   foreach(float charge in new[]{0f,.5f,1f}){Place(actor,home);Pickup(actor);Check(actor.TryKick(charge),"kick accepted at "+charge);Check(Mathf.Abs(Vector3.ProjectOnPlane(ball.Body.linearVelocity,Vector3.up).magnitude-Mathf.Lerp(16.8f,30.8f,charge))<.02f,"football launch is 140 percent at charge "+charge);}
   var rival=Instantiate(app.athletePrefab).GetComponent<Athlete>();rival.Setup();Place(actor,home);Place(rival,home+Vector3.forward*1.4f,180);Pickup(actor);
   rival.Simulate(new(){pressure=true,heading=180},.1f);
   Check(rival.FootballEffort.Pressuring&&!actor.FootballEffort.Pressuring,"defender can pressure ball carrier");
   var forward=FootballPressure.Constrain(actor,Vector3.forward);
   Check(forward.z>.35f&&forward.z<.7f,"pressure contains forward dribble without immobilizing");
   Check(FootballPressure.Constrain(actor,Vector3.right)==Vector3.right&&FootballPressure.Constrain(actor,Vector3.back)==Vector3.back,"sideways escape and retreat preserved");
   var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=8;wall.transform.position=home+new Vector3(0,.8f,.7f);wall.transform.localScale=new Vector3(3,2,.08f);Physics.SyncTransforms();
   Check(FootballPressure.Constrain(actor,Vector3.forward)==Vector3.forward,"world obstruction prevents pressure through wall");Destroy(wall);yield return null;
   Place(rival,home+Vector3.forward*1.4f,180);rival.Simulate(new(){pressure=true,heading=180},.1f);
   var other=Instantiate(app.athletePrefab).GetComponent<Athlete>();other.Setup();Place(other,home+new Vector3(.7f,0,1.4f),205);other.Simulate(new(){pressure=true,heading=205},.1f);
   Check(FootballPressure.Constrain(actor,Vector3.forward).z>=forward.z-.01f,"multiple pressure defenders cannot stack slowdown");Destroy(other.gameObject);
   actor.Simulate(new(){pressure=true},.02f);Check(!actor.FootballEffort.Pressuring,"ball carrier cannot defend self");
   Place(rival,home+Vector3.forward*1.4f,180);rival.Simulate(new(){pressure=true,heading=180},.1f);Loose();rival.Simulate(new(){pressure=true},.02f);Check(!rival.FootballEffort.Pressuring,"loose ball ends pressure");
   Place(actor,home);Pickup(actor);rival.Simulate(new(){pressure=true,heading=180},.02f);
   app.view.target=actor;app.view.mode=1;app.view.yaw=0;app.view.pitch=16;yield return new WaitForSeconds(.5f);
   var buttons=FindObjectsByType<FootballEffortButton>(FindObjectsSortMode.None);var dash=buttons.Single(b=>!b.pressure);var pressure=buttons.Single(b=>b.pressure);
   Check(buttons.Length==2&&dash.button.interactable&&!pressure.enabled&&pressure.GetComponent<KickButton>().enabled,"dash and shared possession-aware action slot");
   var pointer=new PointerEventData(EventSystem.current){pointerId=17,button=PointerEventData.InputButton.Left};dash.OnPointerDown(pointer);Check(app.view.DashHeld,"touch hold starts dash input");
   dash.OnPointerUp(new PointerEventData(EventSystem.current){pointerId=18});Check(app.view.DashHeld,"unrelated finger cannot release dash");dash.OnPointerExit(pointer);Check(!app.view.DashHeld,"leaving button cancels captured dash");
   dash.OnPointerDown(pointer);app.view.ClearMatchInput();Check(!app.view.DashHeld&&!app.view.PressureHeld,"menu or round reset clears football holds");dash.OnPointerUp(pointer);
   var hud=FindFirstObjectByType<FootballEffortHUD>();yield return new WaitForSeconds(.2f);Check(hud&&hud.VisibleBars>=1,"stamina bars project above visible players");
   if(capture){yield return Capture("third-person");
    Place(rival,home+Vector3.right*3);DevelopmentProbe.TurnCommand=new(){dash=true,move=Vector2.up};yield return new WaitForSeconds(.8f);yield return Capture("dash");DevelopmentProbe.TurnCommand=default;Place(actor,home);Place(rival,home+Vector3.forward*1.4f,180);Pickup(actor);rival.Simulate(new(){pressure=true,heading=180},.1f);app.view.mode=2;yield return new WaitForSeconds(.8f);yield return Capture("stadium");app.view.mode=0;yield return new WaitForSeconds(.3f);yield return Capture("first-person");}
   app.view.mode=1;Destroy(rival.gameObject);actor.inTransit=true;actor.Simulate(new(){dash=true,pressure=true,move=Vector2.up},.02f);Check(!actor.FootballEffort.Dashing&&!actor.FootballEffort.Pressuring,"travel blocks effort");actor.inTransit=false;actor.ResetLocomotion();
   Check(actor.FootballEffort.Stamina==100,"new round or teleport resets stamina");
  }
  IEnumerator Capture(string name){yield return new WaitForEndOfFrame();SaveCapture(Path.Combine(folder,name+".png"));Check(File.Exists(Path.Combine(folder,name+".png")),"rendered capture "+name);yield return null;}
  void SaveCapture(string path){
   var cam=Camera.main;
   var target=new RenderTexture(Screen.width,Screen.height,24);var previous=RenderTexture.active;var cameraTarget=cam.targetTexture;
   var overlays=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
   var cameras=overlays.Select(c=>c.worldCamera).ToArray();var distances=overlays.Select(c=>c.planeDistance).ToArray();
   try {
    cam.targetTexture=target;for(int i=0;i<overlays.Length;i++){overlays[i].renderMode=RenderMode.ScreenSpaceCamera;overlays[i].worldCamera=cam;overlays[i].planeDistance=1;}
    Canvas.ForceUpdateCanvases();cam.Render();RenderTexture.active=target;
    var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());Destroy(image);
   }finally{for(int i=0;i<overlays.Length;i++){overlays[i].renderMode=RenderMode.ScreenSpaceOverlay;overlays[i].worldCamera=cameras[i];overlays[i].planeDistance=distances[i];}cam.targetTexture=cameraTarget;RenderTexture.active=previous;target.Release();Destroy(target);Canvas.ForceUpdateCanvases();}
  }
  IEnumerator Host(){
   var guest=Athlete.Active.First(a=>a.GetComponent<NetworkAthlete>()&&a!=actor);Loose();Place(actor,home+Vector3.right*5);Place(guest,home);Signal("dash-ready");
   yield return Await("dash-started");yield return new WaitForSeconds(.8f);Check(guest.FootballEffort.Dashing&&guest.FootballEffort.Stamina<85&&guest.speed>14,"host validates guest dash and stamina");Signal("dash-seen");
   yield return Await("input-paused");yield return new WaitForSeconds(.5f);Check(!guest.FootballEffort.Dashing,"expired guest input cancels dash");Signal("expiry-seen");
   yield return Await("dash-stopped");yield return new WaitForSeconds(.4f);Check(!guest.FootballEffort.Dashing,"guest release cancels host dash");
   Place(actor,home);Place(guest,home+Vector3.forward*1.4f,180);Pickup(actor);Signal("pressure-ready");yield return Await("pressure-started");yield return new WaitForSeconds(.5f);
   Check(guest.FootballEffort.Pressuring&&FootballPressure.Constrain(actor,Vector3.forward).z<.7f,"guest pressure contains host carrier");Signal("pressure-seen");yield return Await("pressure-stopped");yield return new WaitForSeconds(.4f);
   Check(!guest.FootballEffort.Pressuring,"guest release removes pressure");
   var match=FootballMatch.Instance;Check(match.StartMatch(),"team selection starts for teammate pressure check");yield return null;
   actor.GetComponent<NetworkAthlete>().Team.Value=FootballTeam.A;guest.GetComponent<NetworkAthlete>().Team.Value=FootballTeam.A;
   Check(!FootballPressure.Opponents(actor,guest),"same-team pressure rejected");Check(SportsPossession.SameTeam(actor,guest,SportId.Football),"football HUD and minimap use confirmed teammate identity");guest.GetComponent<NetworkAthlete>().Team.Value=FootballTeam.B;Check(FootballPressure.Opponents(actor,guest),"opposing-team pressure accepted");
   Signal("host-done");yield return Await("guest-done");
  }
  IEnumerator Guest(){
   yield return Await("dash-ready");DevelopmentProbe.TurnCommand=new(){dash=true,move=Vector2.up};Signal("dash-started");yield return Await("dash-seen");yield return new WaitForSeconds(.15f);
   Check(actor.FootballEffort.Dashing&&actor.FootballEffort.Stamina<85,"guest displays host dash and depleted stamina");actor.GetComponent<NetworkAthlete>().enabled=false;Signal("input-paused");yield return Await("expiry-seen");
   DevelopmentProbe.TurnCommand=default;actor.GetComponent<NetworkAthlete>().enabled=true;Signal("dash-stopped");
   yield return Await("pressure-ready");yield return new WaitForSeconds(.4f);DevelopmentProbe.TurnCommand=new(){pressure=true,heading=180};Signal("pressure-started");yield return Await("pressure-seen");yield return new WaitForSeconds(.15f);
   Check(actor.FootballEffort.Pressuring&&!actor.FootballEffort.Dashing,"guest displays replicated pressure");DevelopmentProbe.TurnCommand=default;Signal("pressure-stopped");yield return Await("host-done");Signal("guest-done");
  }
  void Finish(){DevelopmentProbe.TurnCommand=default;File.AppendAllText(Path.Combine(folder,"results.txt"),"FOOTBALL_EFFORT_COMPLETE success="+!failed+"\n");}
 }
}
#endif
