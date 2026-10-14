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
   Check(NetworkManager.Singleton.NetworkConfig.ProtocolVersion==RoomService.ProtocolVersion,"matching room protocol");
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
    effort.Step(true,true,true,true,1f/rate);Check(effort.Pressuring&&effort.Dashing,"dash can run while pressure stays held at "+rate+" FPS");
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
    Check(Mathf.Abs(actor.speed-(run?7.35f:4.2f))<.12f,(run?"run":"walk")+" is 70 percent of previous speed: "+actor.speed);
   }
   DevelopmentProbe.TurnCommand=default;Loose();Place(actor,home);DevelopmentProbe.TurnCommand=new(){move=Vector2.up,dash=true};yield return new WaitForSeconds(.7f);
   Check(actor.FootballEffort.Dashing&&Mathf.Abs(actor.speed-10.6575f)<.15f,"dash boosts to 10.6575 metres per second");
   Check(actor.FootballEffort.Stamina<85,"moving dash consumes host stamina");DevelopmentProbe.TurnCommand=default;
   Place(actor,home);Pickup(actor);DevelopmentProbe.TurnCommand=new(){move=Vector2.up,dash=true};yield return new WaitForSeconds(.7f);
   Check(actor.ControlsFootball&&Mathf.Abs(actor.speed-7.993125f)<.15f,"dash dribble retains ball and possession multiplier");
   Check(Vector3.Distance(ball.transform.position,ball.FootPosition(actor))<.03f,"ball stays on foot during fast dribble");
   DevelopmentProbe.TurnCommand=new(){move=Vector2.up,dash=true,charging=true};yield return new WaitForSeconds(.3f);
   Check(!actor.FootballEffort.Dashing&&Mathf.Abs(actor.speed-2.52f)<.15f,"charging cancels dash and moves at scaled charge speed");DevelopmentProbe.TurnCommand=default;
   foreach(float charge in new[]{0f,.5f,1f}){Place(actor,home);Pickup(actor);Check(actor.TryKick(charge),"kick accepted at "+charge);Check(Mathf.Abs(Vector3.ProjectOnPlane(ball.Body.linearVelocity,Vector3.up).magnitude-Mathf.Lerp(13.44f,24.64f,charge))<.02f,"football launch is twenty percent slower at charge "+charge);Check(Mathf.Abs(ball.Body.linearVelocity.y-.28f)<.001f,"shot vertical launch scales with speed at charge "+charge);}
   var rival=Instantiate(app.athletePrefab).GetComponent<Athlete>();rival.Setup();Place(actor,home);Place(rival,home+Vector3.forward*1.4f,180);Pickup(actor);
   rival.Simulate(new(){pressure=true,heading=180},.1f);
   Check(rival.FootballEffort.Pressuring&&!actor.FootballEffort.Pressuring,"defender can pressure ball carrier");
   var forward=FootballPressure.Constrain(actor,Vector3.forward);
   Check(Mathf.Abs(forward.z-.75f)<.001f,"pressure reduces total carrier movement by exactly 25 percent");
   Check(FootballPressure.Constrain(actor,Vector3.right)==Vector3.right*.75f&&FootballPressure.Constrain(actor,Vector3.back)==Vector3.back*.75f,"same slowdown applies sideways and backwards");
   Check(FootballPressure.Constrain(actor,new Vector3(15,2,-20))==new Vector3(11.25f,2,-15),"boosted horizontal motion is reduced without changing vertical motion");
   var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=8;wall.transform.position=home+new Vector3(0,.8f,.7f);wall.transform.localScale=new Vector3(3,2,.08f);Physics.SyncTransforms();
   Check(FootballPressure.Constrain(actor,Vector3.forward)==Vector3.forward,"world obstruction prevents pressure through wall");Destroy(wall);yield return null;
   Place(rival,home+Vector3.forward*1.4f,180);rival.Simulate(new(){pressure=true,heading=180},.1f);
   var other=Instantiate(app.athletePrefab).GetComponent<Athlete>();other.Setup();Place(other,home+new Vector3(.7f,0,1.4f),205);other.Simulate(new(){pressure=true,heading=205},.1f);
   Check(Mathf.Abs(FootballPressure.Constrain(actor,Vector3.forward).z-.75f)<.001f,"multiple pressure defenders cannot stack slowdown");other.gameObject.SetActive(false);Destroy(other.gameObject);
   DefenseChecks(rival);
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
  void DefenseChecks(Athlete rival){
   foreach(int rate in new[]{30,60,120}){
    Place(actor,home);Pickup(actor);Place(rival,home+Vector3.forward*7,180);
    for(int i=0;i<rate/2;i++)rival.Simulate(new(){pressure=true},1f/rate);
    Check(Mathf.Abs(rival.speed-5.145f)<.12f,"automatic chase uses 70 percent run speed at "+rate+" Hz: "+rival.speed);
    for(int i=0;i<rate*2;i++)rival.Simulate(new(){pressure=true},1f/rate);
    float gap=Vector3.Distance(actor.transform.position,rival.transform.position);
    Check(gap>1&&gap<1.6f&&Vector3.Dot(rival.transform.forward,(actor.transform.position-rival.transform.position).normalized)>.95f,"no-stick pressure closes and holds a facing marking gap at "+rate+" Hz: "+gap);
    var stopped=rival.transform.position;for(int i=0;i<rate/2;i++)rival.Simulate(default,1f/rate);
    Check(!rival.FootballEffort.Pressuring&&Vector3.Distance(stopped,rival.transform.position)<.2f,"release cancels automatic guard at "+rate+" Hz");
    foreach(bool dash in new[]{false,true}){
     Place(actor,home,90);Pickup(actor);
     for(int i=0;i<rate/2;i++){
      Place(rival,actor.transform.position+Vector3.forward*1.8f,180);rival.Simulate(new(){pressure=true},1f/rate);
      actor.Simulate(new(){move=Vector2.right,sprint=true,dash=dash},1f/rate);
     }
     float expected=5.5125f*.75f*(dash?1.45f:1);
     Check(actor.ControlsFootball&&Mathf.Abs(actor.speed-expected)<.12f,"25 percent total "+(dash?"dash":"run")+" dribble slowdown at "+rate+" Hz: "+actor.speed);
    }
   }
   Place(actor,home,90);Pickup(actor);ball.ReviewGiveFootballProp(actor,SportsProp.Turbo);
   Check(ball.TryUseProp(actor,ball.PropSlot(actor).revision,FootballMatch.Instance.Snapshot.revision,90),"Turbo pressure fixture activated legally");
   for(int i=0;i<30;i++){Place(rival,actor.transform.position+Vector3.forward*1.8f,180);rival.Simulate(new(){pressure=true},1f/60);actor.Simulate(new(){move=Vector2.right,dash=true},1f/60);}
   Check(Mathf.Abs(actor.speed-7.993125f*SportsAbilityRules.TurboMultiplier*.75f)<.15f,"25 percent penalty includes dash plus Team Turbo: "+actor.speed);
   Place(rival,home);Pickup(rival);Place(actor,home+Vector3.forward*7,180);
   ball.ReviewGiveFootballProp(actor,SportsProp.Turbo);Check(ball.TryUseProp(actor,ball.PropSlot(actor).revision,FootballMatch.Instance.Snapshot.revision,180),"defender Turbo fixture activated legally");
   for(int i=0;i<42;i++){Place(rival,actor.transform.position+Vector3.back*7);ball.FollowController(rival);actor.Simulate(new(){pressure=true,dash=true},1f/60);}
   Check(actor.FootballEffort.Pressuring&&actor.FootballEffort.Dashing&&Mathf.Abs(actor.speed-7.35f*SportsAbilityRules.TurboMultiplier*.7f*FootballEffort.DashMultiplier)<.15f,"defender Turbo and Dash retain 30 percent slowdown: "+actor.speed);ball.ReviewExpireFootballEffects();
   Place(rival,home+Vector3.forward*10,180);Check(!FootballPressure.CarrierFor(rival),"guard does not acquire across pitch");
   Place(actor,home);Pickup(actor);Place(rival,home+Vector3.forward*1.8f,180);rival.Simulate(new(){pressure=true},.02f);
   var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=8;wall.transform.position=home+new Vector3(0,.8f,.8f);wall.transform.localScale=new Vector3(4,2,.1f);Physics.SyncTransforms();
   rival.Simulate(new(){pressure=true},.02f);Check(!rival.FootballEffort.Pressuring,"wall cancels automatic marking");wall.SetActive(false);Destroy(wall);
   var slide=rival.GetComponent<FootballTackle>();
   foreach(int rate in new[]{30,60,120}){
    Loose();Place(actor,home+Vector3.right*8);Place(rival,home);
    Check(rival.TryTackle()&&Mathf.Abs(slide.CooldownRemaining-1.5f)<.001f,"1.5 second tackle cooldown starts at "+rate+" Hz");
    for(int i=0;i<rate*1.4f;i++)slide.Step(1f/rate,out _,out _);
    Check(!rival.TryTackle(),"tackle blocked before 1.5 seconds at "+rate+" Hz");
    for(int i=0;i<rate*.12f;i++)slide.Step(1f/rate,out _,out _);
    Check(rival.TryTackle(),"tackle re-arms after 1.5 seconds at "+rate+" Hz");
   }
   Place(rival,home);Place(actor,home+new Vector3(.95f,0,1),0);Pickup(actor);rival.TryTackle();slide.ResolveContacts(home,home+Vector3.forward*1.5f);
   Check(actor.Action==FootballAction.Hit&&!ball.CurrentController,"wider sweep dislodges off-centre carrier");
   Place(rival,home);Place(actor,home+new Vector3(1.7f,0,1),0);Pickup(actor);rival.TryTackle();slide.ResolveContacts(home,home+Vector3.forward*1.5f);
   Check(actor.Action==FootballAction.None&&ball.CurrentController==actor,"wider tackle still rejects outside reach");
   Place(rival,home);Place(actor,home+Vector3.forward*1.5f,0);Pickup(actor);wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=8;wall.transform.position=home+new Vector3(0,1,.75f);wall.transform.localScale=new Vector3(4,2,.1f);Physics.SyncTransforms();rival.TryTackle();slide.ResolveContacts(home,home+Vector3.forward*.3f);
   Check(actor.Action==FootballAction.None&&ball.CurrentController==actor,"enlarged tackle cannot steal through a wall");wall.SetActive(false);Destroy(wall);
   foreach(int rate in new[]{30,60,120}){
    Place(actor,home,180);Place(rival,home+Vector3.back*2.8f);Pickup(actor);rival.TryTackle();
    for(int i=0;i<rate*.3f;i++)rival.Simulate(default,1f/rate);
    Check(ball.CurrentController!=actor&&!ball.Body.isKinematic&&ball.Body.linearVelocity.z>5,"frontal ball-first tackle defeats attached-ball collision shield at "+rate+" Hz");
   }
   Loose();Place(actor,home+Vector3.right*8);Place(rival,home);ball.Body.position=home+new Vector3(.85f,ball.WorldRadius,1);Physics.SyncTransforms();rival.TryTackle();slide.ResolveContacts(home,home+Vector3.forward*1.5f);
   Check(ball.Body.linearVelocity.z>10&&!slide.ClaimBallContact(),"wider sweep contacts loose ball exactly once");
   Place(actor,home);Place(rival,home+Vector3.forward*1.4f,180);Pickup(actor);
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
   var guest=Athlete.Active.First(a=>a.GetComponent<NetworkAthlete>()&&a!=actor);
   Loose();Place(actor,home+Vector3.right*5);Place(guest,home,180);ball.Body.position=home+Vector3.forward*.9f+Vector3.up*ball.WorldRadius;Physics.SyncTransforms();ball.RefreshControl(guest);
   Check(ball.CurrentController==guest&&ball.Body.isKinematic,"host acquires ball behind guest without facing it");Signal("rear-pickup-ready");yield return Await("rear-pickup-seen");
   Loose();Place(guest,home);Signal("dash-ready");
   yield return Await("dash-started");yield return new WaitForSeconds(.8f);Check(guest.FootballEffort.Dashing&&guest.FootballEffort.Stamina<85&&Mathf.Abs(guest.speed-10.6575f)<.15f,"host validates guest reduced dash speed and stamina");Signal("dash-seen");
   yield return Await("input-paused");yield return new WaitForSeconds(.5f);Check(!guest.FootballEffort.Dashing,"expired guest input cancels dash");Signal("expiry-seen");
   yield return Await("dash-stopped");yield return new WaitForSeconds(.4f);Check(!guest.FootballEffort.Dashing,"guest release cancels host dash");
   Place(actor,home);Place(guest,home+Vector3.forward*4,180);Pickup(actor);Signal("pressure-ready");yield return Await("pressure-started");yield return new WaitForSeconds(.9f);
   Check(guest.FootballEffort.Pressuring&&Mathf.Abs(FootballPressure.Constrain(actor,Vector3.forward).z-.75f)<.001f&&Vector3.Distance(actor.transform.position,guest.transform.position)<2,"guest no-stick pressure follows and slows host carrier by 25 percent");Signal("pressure-seen");
   yield return Await("pressure-paused");yield return new WaitForSeconds(.5f);Check(!guest.FootballEffort.Pressuring&&guest.speed<.2f,"expired guest input cancels automatic following");Signal("pressure-expired");yield return Await("pressure-stopped");yield return new WaitForSeconds(.4f);
   Check(!guest.FootballEffort.Pressuring,"guest release removes pressure");
   var match=FootballMatch.Instance;Check(match.StartMatch(),"team selection starts for teammate pressure check");yield return null;
   actor.GetComponent<NetworkAthlete>().Team.Value=FootballTeam.A;guest.GetComponent<NetworkAthlete>().Team.Value=FootballTeam.A;
   Check(!FootballPressure.Opponents(actor,guest),"same-team pressure rejected");Check(SportsPossession.SameTeam(actor,guest,SportId.Football),"football HUD and minimap use confirmed teammate identity");guest.GetComponent<NetworkAthlete>().Team.Value=FootballTeam.B;Check(FootballPressure.Opponents(actor,guest),"opposing-team pressure accepted");
   Signal("host-done");yield return Await("guest-done");
  }
  IEnumerator Guest(){
   yield return Await("rear-pickup-ready");yield return new WaitForSeconds(.4f);
   Check(actor.ControlsFootball&&Vector3.Distance(ball.transform.position,ball.FootPosition(actor))<.15f,"guest sees host-authoritative rear pickup attached at feet");Signal("rear-pickup-seen");
   yield return Await("dash-ready");DevelopmentProbe.TurnCommand=new(){dash=true,move=Vector2.up};Signal("dash-started");yield return Await("dash-seen");yield return new WaitForSeconds(.15f);
   Check(actor.FootballEffort.Dashing&&actor.FootballEffort.Stamina<85,"guest displays host dash and depleted stamina");actor.GetComponent<NetworkAthlete>().enabled=false;Signal("input-paused");yield return Await("expiry-seen");
   DevelopmentProbe.TurnCommand=default;actor.GetComponent<NetworkAthlete>().enabled=true;Signal("dash-stopped");
   yield return Await("pressure-ready");yield return new WaitForSeconds(.4f);DevelopmentProbe.TurnCommand=new(){pressure=true,heading=180};Signal("pressure-started");yield return Await("pressure-seen");yield return new WaitForSeconds(.15f);
   Check(actor.FootballEffort.Pressuring&&!actor.FootballEffort.Dashing,"guest displays replicated pressure");actor.GetComponent<NetworkAthlete>().enabled=false;Signal("pressure-paused");yield return Await("pressure-expired");
   DevelopmentProbe.TurnCommand=default;actor.GetComponent<NetworkAthlete>().enabled=true;Signal("pressure-stopped");yield return Await("host-done");Signal("guest-done");
  }
  void Finish(){DevelopmentProbe.TurnCommand=default;File.AppendAllText(Path.Combine(folder,"results.txt"),"FOOTBALL_EFFORT_COMPLETE success="+!failed+"\n");}
 }
}
#endif
