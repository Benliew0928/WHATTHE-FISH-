#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public sealed class ActivityCinematicReview:MonoBehaviour {
  string folder;bool failed,film;AppRoot app;PlayerView view;Athlete actor;Camera camera;RenderTexture render;Texture2D pixels;int checks;
  static string Arg(string key){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:"";}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var path=Arg("-activityCinematicReview");if(path.Length>0)new GameObject("Activity cinematic review").AddComponent<ActivityCinematicReview>().folder=path;}
  void Check(bool ok,string label){checks++;failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Until(Func<bool> done,string label,float seconds=80){float end=Time.realtimeSinceStartup+seconds;while(!done()&&Time.realtimeSinceStartup<end)yield return null;Check(done(),label);}
  void Place(Athlete a,Vector3 p,float yaw){a.capsule.enabled=false;a.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));a.ResetLocomotion();a.capsule.enabled=true;Physics.SyncTransforms();}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");film=Environment.GetCommandLineArgs().Contains("-cinematicFilm");
   yield return Until(()=>DevelopmentProbe.ReadyToReviewSport,"sport ready");if(!DevelopmentProbe.ReadyToReviewSport){Finish();yield break;}
   app=AppRoot.Instance;view=app.view;actor=app.LocalAthlete;camera=Camera.main;view.mode=1;
   while(IslandTutorialUI.DismissTop()){}DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   QualitySettings.vSyncCount=0;Application.targetFrameRate=30;Time.captureFramerate=30;
   render=new RenderTexture(Screen.width,Screen.height,24);pixels=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
   yield return new WaitForSeconds(.5f);
   if(app.SelectedSport==SportId.Fishing)yield return Fishing();else if(app.SelectedSport==SportId.Golf)yield return Golf();else yield return Court();
   Finish();
  }
  IEnumerator Clip(string name,Action activate,Action activeCheck=null,Action endedCheck=null){
   string dir=Path.Combine(folder,name);if(film)Directory.CreateDirectory(dir);var start=camera.transform.rotation;float heading=view.yaw,clock=Time.time;bool moved=false;
   for(int i=0;i<150;i++){
    if(i==24)activate();
    yield return new WaitForEndOfFrame();
    if(i>=28&&i<90)moved|=Quaternion.Angle(start,camera.transform.rotation)>3;
    if(i==40){activeCheck?.Invoke();Check(Time.timeScale==1&&!view.InputBlocked,name+" gameplay remains available");Check(Mathf.Abs(Mathf.DeltaAngle(heading,view.yaw))<.01f,name+" gameplay heading unchanged");}
    if(i==135)endedCheck?.Invoke();
    if(film)Capture(Path.Combine(dir,i.ToString("D5")+".jpg"));else if(i==40)Capture(Path.Combine(folder,name+".jpg"));
   }
   Check(Time.time-clock>4.9f,name+" simulation advances");File.AppendAllText(Path.Combine(folder,"clips.tsv"),name+"\t150\t30\t"+moved+"\n");
  }
  void Capture(string path){
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var oldCameras=canvases.Select(c=>c.worldCamera).ToArray();var planes=canvases.Select(c=>c.planeDistance).ToArray();var previous=camera.targetTexture;var active=RenderTexture.active;
   try{foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=.5f;}camera.targetTexture=render;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=render;pixels.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);pixels.Apply();File.WriteAllBytes(path,pixels.EncodeToJPG(92));}
   finally{camera.targetTexture=previous;RenderTexture.active=active;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=RenderMode.ScreenSpaceOverlay;canvases[i].worldCamera=oldCameras[i];canvases[i].planeDistance=planes[i];}Canvas.ForceUpdateCanvases();}
  }
  IEnumerator Fishing(){
   FishingAbilityChecks.Run(Check);var game=FishingGame.Instance;var origin=game.Pier(0);var toward=Vector3.ProjectOnPlane(game.transform.position-origin,Vector3.up).normalized;float heading=Quaternion.LookRotation(toward).eulerAngles.y;
   Place(actor,origin,heading);view.yaw=heading;view.pitch=18;yield return new WaitForSeconds(.6f);
   foreach(var skill in FishingAbilityRules.Roster){
    view.mode=1;view.ClearMatchInput();game.ResetOfflinePractice();while(IslandTutorialUI.DismissTop()){}FishingHUD.Instance.CloseRoundOverlays();
    var p=game.Player(actor).Value;p.item=skill;p.skillPicked=true;p.active=FishingAbility.None;p.itemSequence=0;p.activeUntil=p.cooldownUntil=0;p.pier=0;p.phase=FishingAbilityRules.BeforeCast(skill)?FishingPhase.Ready:FishingPhase.Reeling;p.phaseAt=game.Now;p.origin=actor.transform.position;p.fish=FishingAbilityRules.BeforeCast(skill)?-1:2;p.distance=p.lineLength=p.castDistance=7;p.fightHeading=toward;p.fightOffset=new Vector2(0,.5f);p.tension=.25f;p.progress=.1f;
    game.State.Players[game.State.Players.FindIndex(q=>q.owner==p.owner)]=p;
    if(p.fish>=0){var fish=game.State.Fish[p.fish];fish.claimed=true;fish.owner=p.owner;game.State.Fish[p.fish]=fish;}
    yield return new WaitForSeconds(.25f);
    double deadline=0;yield return Clip("Fishing-"+skill,()=>{var current=game.Player(actor).Value;Check(game.Execute(actor,FishingAction.UseAbility,game.State.Match.round,current.sequence),skill+" accepted by gameplay");deadline=game.Player(actor).Value.activeUntil;},()=>{
     Check(ActivityCinematic.Instance.Visible&&ActivityCinematic.Instance.Title==FishingAbilityRules.Name(skill),skill+" named announcement");Check(ActivityCinematic.Instance.CameraActive,skill+" own cinematic camera");
     Check(game.Player(actor).Value.activeUntil==deadline,skill+" duration unchanged");if(skill==FishingAbility.FishingBuddy)Check(ActivityCinematic.Instance.BuddySpotlight&&game.GetComponent<FishingAbilityPresentation>().VisibleBuddies>0,"buddy UI spotlight and real helper visible");
     var current=game.Player(actor).Value;Check(!game.Execute(actor,FishingAction.UseAbility,game.State.Match.round,current.sequence),skill+" duplicate use rejected");
    },()=>Check(!ActivityCinematic.Instance.Visible&&!ActivityCinematic.Instance.CameraActive,skill+" exits to normal camera"));
   }
   game.ResetOfflinePractice();yield return null;Check(!ActivityCinematic.Instance.Visible,"fishing round reset clears cue");
   var future=game.Player(actor).Value;future.active=FishingAbility.Sonar;future.activeAt=game.Now+.3;future.activeUntil=game.Now+16;future.itemSequence++;
   int index=game.State.Players.FindIndex(p=>p.owner==future.owner);game.State.Players[index]=future;
   yield return new WaitForSeconds(.1f);Check(!ActivityCinematic.Instance.Visible,"future fishing packet waits for activation timestamp");
   yield return new WaitForSeconds(.3f);Check(ActivityCinematic.Instance.Visible&&ActivityCinematic.Instance.Skill==FishingAbility.Sonar,"future fishing packet announces when clock catches up");
   game.ResetOfflinePractice();yield return null;var stale=game.Player(actor).Value;stale.active=FishingAbility.Sonar;stale.activeAt=game.Now-8;stale.activeUntil=game.Now+8;stale.itemSequence++;
   game.State.Players[game.State.Players.FindIndex(p=>p.owner==stale.owner)]=stale;
   yield return new WaitForSeconds(.1f);Check(!ActivityCinematic.Instance.Visible,"late fishing packet does not replay cinematic");
   game.ResetOfflinePractice();
  }
  IEnumerator Golf(){
   var game=GolfMatchManager.Instance;bool started=false;yield return Until(()=>started||(started=game.StartTestMatch(new[]{(GolfCartWorld.Key(actor),actor,"You")})),"golf test match starts",20);yield return new WaitForSeconds(.5f);
   var hole=game.Course.holes.OrderBy(h=>h.number).First();var direction=hole.cup.position-hole.tee.position;Place(actor,hole.tee.position+Vector3.up*.05f,Quaternion.LookRotation(Vector3.ProjectOnPlane(direction,Vector3.up)).eulerAngles.y);view.yaw=actor.transform.eulerAngles.y;view.pitch=20;
   foreach(var kind in GolfEventSchedule.RandomKinds){
    game.Events.Clear();game.Ball(actor).Place(hole.tee.position+Vector3.up*.05f,true);view.SkipGolfReplay();yield return new WaitForSeconds(.7f);double deadline=0;
    yield return Clip("Golf-"+kind,()=>{game.ReviewEvent(kind);deadline=game.Events.Snapshot.end;},()=>{Check(ActivityCinematic.Instance.Visible&&ActivityCinematic.Instance.Title==GolfEventHUD.Name(kind),kind+" named event animation");Check(ActivityCinematic.Instance.CameraActive||view.GolfShotCameraActive,kind+" fairway camera or live shot priority");Check(game.Events.Snapshot.end==deadline,kind+" event deadline unchanged");},()=>Check(!ActivityCinematic.Instance.Visible&&!ActivityCinematic.Instance.CameraActive,kind+" presentation ends while event remains"));
    Check(game.Events.Active(game.Now)==kind,kind+" effect persists after announcement");
   }
   game.Events.Clear();yield return new WaitForSeconds(.2f);
   yield return Clip("Golf-FinalRush",()=>{var e=game.Events.Snapshot;e.rushAt=game.Now;game.Events.Receive(e);},()=>Check(ActivityCinematic.Instance.Visible&&ActivityCinematic.Instance.Title=="FINAL RUSH","final rush announced"));
   // A packet received after its short presentation window must not replay it.
   game.Events.ReviewEvent(GolfEventKind.TropicalWind,game.Now-8);yield return null;yield return new WaitForEndOfFrame();Check(!ActivityCinematic.Instance.Visible,"late golf event does not replay cinematic");
   bool reduced=MenuPreferences.ReducedMotion;MenuPreferences.SetMotion(true);game.ReviewEvent(GolfEventKind.BounceParty);yield return new WaitForSeconds(.4f);Check(ActivityCinematic.Instance.Visible&&!ActivityCinematic.Instance.CameraActive,"reduced motion retains event notice without camera travel");MenuPreferences.SetMotion(reduced);
   foreach(int mode in new[]{0,1,2}){view.mode=mode;game.ReviewEvent(GolfEventKind.StickyGreen);yield return new WaitForSeconds(.4f);Check(ActivityCinematic.Instance.CameraActive,"golf cinematic works in camera mode "+mode);view.mode=(mode+1)%3;yield return new WaitForSeconds(.1f);Check(!ActivityCinematic.Instance.CameraActive,"changing camera cancels cinematic "+mode);}
   view.mode=1;game.Events.Clear();var ball=game.Ball(actor);ball.Place(hole.tee.position+Vector3.up*.05f,true);Place(actor,ball.Body.position+Vector3.back*.75f+Vector3.up*.1f,0);view.yaw=0;yield return new WaitForSeconds(.4f);Check(view.BeginGolfAim(),"aim begins before event");game.ReviewEvent(GolfEventKind.TropicalWind);yield return new WaitForSeconds(.4f);Check(view.GolfAiming&&ActivityCinematic.Instance.Visible&&!ActivityCinematic.Instance.CameraActive,"event announces without taking aiming camera");view.CancelGolfAim();
   game.Events.Clear();game.ClearMatch();yield return null;yield return new WaitForEndOfFrame();Check(!ActivityCinematic.Instance.Visible,"golf exit clears cue");
  }
  IEnumerator Court(){
   bool basket=app.SelectedSport==SportId.Basketball;var bb=BasketballBall.Active;var fb=FootballBall.Instance;
   if(basket){Check(BasketballBots.Instance.StartSolo(2),"basketball review roster");yield return null;Check(BasketballMatch.Instance.StartMatch(),"basketball match starts");BasketballMatch.Instance.Selection.Complete(new System.Random(7));BasketballMatch.Instance.State.Confirm(BasketballMatch.Instance.Now-4);BasketballMatch.Instance.State.Advance(BasketballMatch.Instance.Now);}
   else {Check(FootballBots.Instance.StartSolo(2),"football review roster");yield return null;Check(FootballMatch.Instance.StartTestMatch(),"football match starts");FootballMatch.Instance.Selection.Complete(new System.Random(7));FootballMatch.Instance.State.TeamsConfirmed(FootballMatch.Instance.Now-4);FootballMatch.Instance.State.Advance(FootballMatch.Instance.Now);}
   foreach(var bot in FindObjectsByType<BasketballBot>(FindObjectsSortMode.None))bot.enabled=false;foreach(var bot in FindObjectsByType<FootballBot>(FindObjectsSortMode.None))bot.enabled=false;
   yield return new WaitForSeconds(.5f);
   var others=Athlete.Active.Where(a=>a&&a!=actor&&a.isActiveAndEnabled&&(basket?BasketballBots.IsBot(a):FootballBots.IsBot(a))).ToArray();Check(others.Length>0,"prop actor available");if(others.Length==0)yield break;var user=others[0];
   Vector3 Point(float x,float z)=>basket?bb.transform.parent.TransformPoint(x,.08f,z):fb.Pitch.TransformPoint(fb.PitchBounds.center.x+x,fb.PitchBounds.max.y+.03f,fb.PitchBounds.center.z+z);
   foreach(var prop in basket?new[]{SportsProp.Turbo,SportsProp.Portal,SportsProp.LockOn}:new[]{SportsProp.Turbo,SportsProp.Portal,SportsProp.Wall}){
    view.enabled=true;view.mode=1;view.pitch=16;view.yaw=basket?0:fb.Pitch.eulerAngles.y;
    if(basket){bb.ReviewResetProps();bb.autoPickup=false;bb.Place(new Vector3(5,.35f,1),Quaternion.identity,Vector3.zero,Vector3.zero);}else{fb.ResetFootballProps();fb.ReviewFootballBall(Point(8,8),Vector3.zero);}
    float z=(basket?bb.PropTeam(user):fb.PropTeam(user))==FootballTeam.B?4:-4;
    Place(actor,Point(-3,z-4),view.yaw);Place(user,Point(1,z),view.yaw+(z>0?180:0));for(int i=1;i<others.Length;i++)Place(others[i],Point(7,i*3-4),view.yaw);
    // Show the actual inventory badge before use, then keep the player's normal
    // camera enabled for the automatic turn and the post-activation direction.
    view.yaw-=8;
    if(prop==SportsProp.LockOn){view.mode=0;view.yaw=Quaternion.LookRotation(user.transform.position-actor.transform.position).eulerAngles.y-10;view.pitch=5;}
    if(basket&&prop==SportsProp.LockOn){bb.autoPickup=true;bb.Place(bb.transform.parent.InverseTransformPoint(user.transform.position)+new Vector3(0,.18f,z>0?-.6f:.6f),Quaternion.identity,Vector3.zero,Vector3.zero);yield return Until(()=>bb.Holder==user,"Lock-on player picks up ball",4);}
    if(basket)bb.ReviewGiveProp(user,prop);else fb.ReviewGiveFootballProp(user,prop);yield return new WaitForSeconds(.5f);
    Check(CharacterIdentityBadges.TryGetHeldProp(user,out var held,out _)&&held==prop,prop+" icon beside remote player name");
    yield return Clip((basket?"Basketball-":"Football-")+prop,()=>{bool accepted=basket?bb.TryUseProp(user,bb.PropSlot(user).revision):fb.TryUseProp(user,fb.PropSlot(user).revision,FootballMatch.Instance.Snapshot.revision,user.transform.eulerAngles.y);Check(accepted,prop+" actual activation accepted");},()=>{
     Check(!CharacterIdentityBadges.TryGetHeldProp(user,out _,out _),prop+" consumed icon disappears");
     if(prop==SportsProp.LockOn)Check(!view.PropFocusActive&&bb.GetComponent<SportsPropPresentation>().WarningVisible,"Lock-on aura without camera takeover");else Check(view.PropFocusActive&&view.PropFocusSubject==user,prop+" camera focuses other player");
    },()=>{if(prop!=SportsProp.LockOn)Check(!view.PropFocusActive&&view.PropHeadingHeld,prop+" camera keeps final heading");});
    PlayerView.LookDelta=new Vector2(20,0);yield return new WaitForSeconds(.15f);
   }
  }
  void Finish(){Time.captureFramerate=0;DevelopmentProbe.TurnCommand=default;DevelopmentProbe.TurnCommandActive=false;if(render){render.Release();Destroy(render);}if(pixels)Destroy(pixels);File.AppendAllText(Path.Combine(folder,"results.txt"),"ACTIVITY_CINEMATIC_COMPLETE success="+!failed+" checks="+checks+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
