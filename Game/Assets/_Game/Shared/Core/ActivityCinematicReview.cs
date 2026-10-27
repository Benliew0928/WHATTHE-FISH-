#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public sealed class ActivityCinematicReview:MonoBehaviour {
  string folder;bool failed,film,originalMotion;AppRoot app;PlayerView view;Athlete actor;Camera camera;RenderTexture render;Texture2D pixels;int checks;
  static string Arg(string key){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:"";}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var path=Arg("-activityCinematicReview");if(path.Length>0)new GameObject("Activity cinematic review").AddComponent<ActivityCinematicReview>().folder=path;}
  void Check(bool ok,string label){checks++;failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Until(Func<bool> done,string label,float seconds=80){float end=Time.realtimeSinceStartup+seconds;while(!done()&&Time.realtimeSinceStartup<end)yield return null;Check(done(),label);}
  void Place(Athlete a,Vector3 p,float yaw){a.capsule.enabled=false;a.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));a.ResetLocomotion();a.capsule.enabled=true;Physics.SyncTransforms();}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");film=Environment.GetCommandLineArgs().Contains("-cinematicFilm");
   yield return Until(()=>DevelopmentProbe.ReadyToReviewSport,"sport ready");if(!DevelopmentProbe.ReadyToReviewSport){Finish();yield break;}
   originalMotion=MenuPreferences.ReducedMotion;MenuPreferences.SetMotion(false);app=AppRoot.Instance;view=app.view;actor=app.LocalAthlete;camera=Camera.main;view.mode=1;
   while(IslandTutorialUI.DismissTop()){}DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   QualitySettings.vSyncCount=0;Application.targetFrameRate=30;Time.captureFramerate=30;
   render=new RenderTexture(Screen.width,Screen.height,24);pixels=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
   yield return new WaitForSeconds(.5f);
   if(app.SelectedSport==SportId.Fishing)yield return Fishing();else if(app.SelectedSport==SportId.Golf)yield return Golf();else yield return Court();
   Finish();
  }
  IEnumerator Clip(string name,Action activate,Action activeCheck=null,Action endedCheck=null){
   string dir=Path.Combine(folder,name);if(film)Directory.CreateDirectory(dir);var start=camera.transform.rotation;float heading=view.yaw,clock=Time.time;bool moved=false,fadeOut=false,fadeIn=false;Vector3 stickyAt=default;Quaternion stickyRotation=default;Vector3 rushLook=default,windCamera=default;Quaternion windRotation=default;Transform runningLeg=null;Quaternion earlierLeg=default;
   for(int i=0;i<150;i++){
    if(i==24)activate();
    yield return new WaitForEndOfFrame();
    if(i>=28&&i<90)moved|=Quaternion.Angle(start,camera.transform.rotation)>3;
    if(i==40){Check(!GameObject.Find("Activity announcement"),name+" has no black announcement card");activeCheck?.Invoke();Check(Time.timeScale==1&&!view.InputBlocked,name+" gameplay remains available");Check(Mathf.Abs(Mathf.DeltaAngle(heading,view.yaw))<.01f,name+" gameplay heading unchanged");}
    if(name.StartsWith("Golf-")&&i>=24){float alpha=view.GetComponent<GolfCinematicHUD>().Opacity;bool partial=alpha>0&&alpha<1;fadeOut|=partial&&ActivityCinematic.Instance.CameraActive;fadeIn|=partial&&!ActivityCinematic.Instance.CameraActive;if(i==30||i==136)Capture(Path.Combine(folder,name+(i==30?"-fade-out.jpg":"-fade-in.jpg")));}
    if(name=="Golf-StickyGreen"&&(i==75||i==100)){var example=GameObject.Find("Demonstration golf ball");Check(example,"sticky example remains visible during hold");if(example){if(i==75){stickyAt=example.transform.position;stickyRotation=example.transform.rotation;}else Check(Vector3.Distance(stickyAt,example.transform.position)<.0001f&&Quaternion.Angle(stickyRotation,example.transform.rotation)<.001f,"sticky ball holds both position and rotation after landing");}}
    if(name=="Golf-TropicalWind"&&i==54){windCamera=camera.transform.position;windRotation=camera.transform.rotation;}
    if(name=="Golf-TropicalWind"&&i==70)Check(Vector3.Distance(windCamera,camera.transform.position)<.001f&&Quaternion.Angle(windRotation,camera.transform.rotation)<.01f,"wind reference camera remains fixed as ball drifts");
    if(name=="Golf-BounceParty"&&(i==60||i==62||i==93||i==95||i==114||i==116)){
     var ball=GameObject.Find("Demonstration golf ball");var scale=ball.transform.localScale;
     Check(Mathf.Abs(scale.x-scale.y)<.000001f&&Mathf.Abs(scale.y-scale.z)<.000001f,"bounce demonstration keeps spherical scale through impact "+i);
    }
    if(i==108&&name.StartsWith("Golf-"))Check(ActivityCinematic.Instance.CameraActive,"golf camera remains active beyond old 2.6 second limit");
    if(name=="Golf-FinalRush"&&i==48){
     rushLook=camera.transform.forward;var demo=GameObject.Find("Rush demonstration cart");
     bool grounded=demo&&GolfCartMotor.Surface(demo.transform.position,GolfCartMotor.Heading(demo.transform.rotation),out var support,out var slope)&&Mathf.Abs(demo.transform.position.y-support.y-.025f)<.06f&&Quaternion.Angle(demo.transform.rotation,slope)<3;
     Check(grounded,"rush cart follows four-wheel ground support and slope");
    }
    if(name=="Golf-FinalRush"&&i==90){runningLeg=GameObject.Find("Rush chasing runner 1").GetComponentsInChildren<Transform>().First(t=>t.name=="mixamorig:LeftUpLeg");earlierLeg=runningLeg.localRotation;}
    if(name=="Golf-FinalRush"&&i==100){
     Check(runningLeg&&Quaternion.Angle(earlierLeg,runningLeg.localRotation)>3,"actual gameplay run clip animates the cloned leg bones");
     Check(Vector3.Angle(rushLook,camera.transform.forward)>120,"rush camera turns through the close pass to follow the pursuit");
     var trails=GameObject.Find("Island Rush demonstration (visual only)").GetComponentsInChildren<GolfRushTrail>();
     Check(trails.Length==3&&trails.All(t=>t.Emitting&&t.VisibleVertices>0&&t.FrontVertexCount==0),"all preview runners use the live Rush trail, wholly behind their facing plane");
    }
    if(i==135)endedCheck?.Invoke();
    if(i==148&&name.StartsWith("Golf-"))Check(view.GetComponent<GolfCinematicHUD>().Opacity>.99f,name+" all HUD opacity restored");
    if(film)Capture(Path.Combine(dir,i.ToString("D5")+".jpg"));else if(i==40||i==64||i==84||i==105)Capture(Path.Combine(folder,name+"-"+i+".jpg"));
   }
   if(name.StartsWith("Golf-")){Check(fadeOut,name+" uses gradual fade out");Check(fadeIn,name+" uses gradual fade in");}
   Check(Time.time-clock>4.9f,name+" simulation advances");File.AppendAllText(Path.Combine(folder,"clips.tsv"),name+"\t150\t30\t"+moved+"\n");
  }
  void Capture(string path){
   // Hidden/headless player windows do not provide a populated backbuffer.
   // Convert only root canvases; nested override canvases must keep their
   // inherited camera and independent sorting/CanvasGroup behaviour.
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var oldCameras=canvases.Select(c=>c.worldCamera).ToArray();var planes=canvases.Select(c=>c.planeDistance).ToArray();var previous=camera.targetTexture;var active=RenderTexture.active;
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
   Check(Mathf.Approximately(GolfEventShowcase.Radius,GolfBall.Radius),"event example radius matches gameplay ball");
   float landing=GolfEventShowcase.WindLanding;var physics=Resources.Load<GolfBallPhysicsSettings>("GolfBallPhysics");
   Check(Mathf.Abs(GolfEventShowcase.Sample(GolfEventKind.TropicalWind,landing).y-GolfBall.Radius)<.0001f&&GolfEventShowcase.Sample(GolfEventKind.TropicalWind,landing+.3f).y>GolfBall.Radius+.4f,"wind ball contacts turf and rebounds naturally");
   float bounceTime=(landing-.5f)*physics.turfRestitution;
   Check(Mathf.Abs(GolfEventShowcase.Sample(GolfEventKind.TropicalWind,landing+bounceTime).y-GolfBall.Radius)<.001f,"wind rebound duration uses gameplay turf restitution");
   var incoming=(GolfEventShowcase.Sample(GolfEventKind.TropicalWind,landing)-GolfEventShowcase.Sample(GolfEventKind.TropicalWind,landing-.001f))/.001f;
   var outgoing=(GolfEventShowcase.Sample(GolfEventKind.TropicalWind,landing+.001f)-GolfEventShowcase.Sample(GolfEventKind.TropicalWind,landing))/.001f;
   Check(Mathf.Abs(outgoing.x/incoming.x-physics.landingSpeedRetention)<.003f,"wind landing keeps the gameplay horizontal damping");
   Check(Mathf.Abs(GolfEventShowcase.Sample(GolfEventKind.StickyGreen,.35f).y-GolfBall.Radius-8)<.001f&&Mathf.Abs(GolfEventShowcase.Sample(GolfEventKind.BounceParty,.35f).y-GolfBall.Radius-8)<.001f,"both falling examples start eight metres above contact");
   Check(GolfRushShowcase.CartTravel(2.5f)-GolfRushShowcase.CartTravel(2)>GolfRushShowcase.CartTravel(1)-GolfRushShowcase.CartTravel(.5f),"rush cart visibly accelerates");
   Check(GolfEventShowcase.Sample(GolfEventKind.TropicalWind,2.2f).z>GolfEventShowcase.Sample(GolfEventKind.TropicalWind,1.3f).z*2,"wind bends the airborne path progressively");
   Check(GolfEventShowcase.Sample(GolfEventKind.StickyGreen,1.35f)==GolfEventShowcase.Sample(GolfEventKind.StickyGreen,3.4f),"sticky demonstration stops exactly at contact");
   Check(Mathf.Abs(GolfEventShowcase.Sample(GolfEventKind.BounceParty,1.15f).y-GolfEventShowcase.Radius)<.001f&&GolfEventShowcase.Sample(GolfEventKind.BounceParty,1.7f).y>2&&Mathf.Abs(GolfEventShowcase.Sample(GolfEventKind.BounceParty,2.25f).y-GolfEventShowcase.Radius)<.001f,"bounce demonstration lands then rises then lands again");
   var game=GolfMatchManager.Instance;bool started=false;yield return Until(()=>started||(started=game.StartTestMatch(new[]{(GolfCartWorld.Key(actor),actor,"You")})),"golf test match starts",20);yield return new WaitForSeconds(.5f);
   var hole=game.Course.holes.OrderBy(h=>h.number).First();var direction=hole.cup.position-hole.tee.position;Place(actor,hole.tee.position+Vector3.up*.05f,Quaternion.LookRotation(Vector3.ProjectOnPlane(direction,Vector3.up)).eulerAngles.y);view.yaw=actor.transform.eulerAngles.y;view.pitch=20;
   foreach(var kind in GolfEventSchedule.RandomKinds){
    game.Events.Clear();game.Ball(actor).Place(hole.tee.position+Vector3.up*.05f,true);view.SkipGolfReplay();yield return new WaitForSeconds(.7f);double deadline=0;
    yield return Clip("Golf-"+kind,()=>{game.ReviewEvent(kind);deadline=game.Events.Snapshot.end;},()=>{Check(ActivityCinematic.Instance.Visible&&ActivityCinematic.Instance.Title==GolfEventHUD.Name(kind),kind+" named event animation");bool watching=ActivityCinematic.WatchesGolfEvent(kind);Check(ActivityCinematic.Instance.CameraActive==watching,kind+" correct camera opt-in");Check(Mathf.Abs(view.GetComponent<GolfCinematicHUD>().Opacity-(watching?0:1))<.01f,kind+" synchronized HUD fade policy");if(watching)Check(GearHit(),"settings remains visible and pointer-reachable during hidden HUD");Check(game.Events.Snapshot.end==deadline,kind+" event deadline unchanged");
    if(GolfEventShowcase.Supports(kind)){var demo=GameObject.Find("Golf event demonstration (visual only)");Check(ActivityCinematic.Instance.DemonstrationVisible&&demo,kind+" has a visible example ball");Check(demo&&demo.GetComponentsInChildren<Collider>().Length==0&&demo.GetComponentsInChildren<Rigidbody>().Length==0&&demo.GetComponentsInChildren<GolfBall>().Length==0,kind+" demonstration cannot affect match physics");if(demo){
     var visual=demo.transform.Find("Demonstration golf ball");var filters=visual.GetComponentsInChildren<MeshFilter>();var size=new Bounds();bool first=true;
     foreach(var f in filters){var matrix=Matrix4x4.Scale(visual.lossyScale)*visual.worldToLocalMatrix*f.transform.localToWorldMatrix;var b=f.sharedMesh.bounds;for(int k=0;k<8;k++){var q=matrix.MultiplyPoint3x4(b.center+Vector3.Scale(b.extents,new Vector3((k&1)==0?-1:1,(k&2)==0?-1:1,(k&4)==0?-1:1)));if(first){size=new Bounds(q,Vector3.zero);first=false;}else size.Encapsulate(q);}}
     Check(Mathf.Abs(Mathf.Max(size.size.x,Mathf.Max(size.size.y,size.size.z))-GolfBall.Radius*2)<.002f,kind+" rendered mesh diameter equals actual gameplay ball");
     var trail=visual.GetComponent<GolfBallTrail>();Check(trail&&Mathf.Approximately(trail.Ribbon.time,GolfBallTrail.Lifetime)&&Mathf.Approximately(trail.Ribbon.widthMultiplier,game.Ball(actor).Trail.width),kind+" reuses gameplay trail lifetime and width");
    }}},()=>Check(!ActivityCinematic.Instance.Visible&&!ActivityCinematic.Instance.CameraActive,kind+" presentation ends while event remains"));
    Check(game.Events.Active(game.Now)==kind,kind+" effect persists after announcement");Check(!ActivityCinematic.Instance.DemonstrationVisible,kind+" demonstration cleaned up after return");
   }
   game.Events.Clear();yield return new WaitForSeconds(.2f);
   yield return Clip("Golf-FinalRush",()=>{var e=game.Events.Snapshot;e.rushAt=game.Now;game.Events.Receive(e);},()=>{Check(ActivityCinematic.Instance.CameraActive&&ActivityCinematic.Instance.RushDemonstrationVisible&&ActivityCinematic.Instance.RushRunnerCount==3,"Island Rush watches cart and three chasing runners");var cast=GameObject.Find("Island Rush demonstration (visual only)");Check(cast&&cast.GetComponentsInChildren<Collider>().Length==0&&cast.GetComponentsInChildren<Athlete>().Length==0&&cast.GetComponentsInChildren<GolfCart>().Length==0,"rush cast cannot move real players or carts");Check(GearHit(),"settings stays reachable during rush");},()=>Check(!ActivityCinematic.Instance.RushDemonstrationVisible,"rush cast cleans up after 3.5 seconds"));
   yield return BadgeCaptions(game);
   // A packet received after its short presentation window must not replay it.
   game.Events.ReviewEvent(GolfEventKind.TropicalWind,game.Now-8);yield return null;yield return new WaitForEndOfFrame();Check(!ActivityCinematic.Instance.Visible,"late golf event does not replay cinematic");
   bool reduced=MenuPreferences.ReducedMotion;MenuPreferences.SetMotion(true);game.ReviewEvent(GolfEventKind.BounceParty);yield return new WaitForSeconds(.4f);Check(ActivityCinematic.Instance.Visible&&!ActivityCinematic.Instance.CameraActive&&!ActivityCinematic.Instance.DemonstrationVisible,"reduced motion retains event notice without camera travel");MenuPreferences.SetMotion(reduced);
   foreach(int mode in new[]{0,1,2}){view.mode=mode;game.ReviewEvent(GolfEventKind.Tornado);yield return new WaitForSeconds(.4f);Check(ActivityCinematic.Instance.CameraActive,"golf cinematic works in camera mode "+mode);view.mode=(mode+1)%3;yield return new WaitForSeconds(.1f);Check(!ActivityCinematic.Instance.CameraActive,"changing camera cancels cinematic "+mode);}
   view.mode=1;game.Events.Clear();var ball=game.Ball(actor);ball.Place(hole.tee.position+Vector3.up*.05f,true);Place(actor,ball.Body.position+Vector3.back*.75f+Vector3.up*.1f,0);view.yaw=0;yield return new WaitForSeconds(.4f);Check(view.BeginGolfAim(),"aim begins before event");game.ReviewEvent(GolfEventKind.TropicalWind);yield return new WaitForSeconds(.4f);Check(view.GolfAiming&&ActivityCinematic.Instance.Visible&&!ActivityCinematic.Instance.CameraActive&&!ActivityCinematic.Instance.DemonstrationVisible,"event announces without taking aiming camera");view.CancelGolfAim();
   game.Events.Clear();game.ReviewEvent(GolfEventKind.Tornado);yield return new WaitForSeconds(.7f);Check(GearHit(),"settings hit target remains on top");GolfHUD.Instance.IslandMenu.Toggle();yield return new WaitForSeconds(.5f);Check(GolfHUD.Instance.IslandMenu.Visible&&!ActivityCinematic.Instance.CameraActive,"settings opens during cinematic and releases camera");GolfHUD.Instance.IslandMenu.Close();yield return new WaitForSeconds(.5f);
   game.Events.Clear();ulong id=game.Player(actor).PlayerId;for(int i=0;i<5;i++){var player=game.State.Player(id);game.State.RecordSwing(id,game.Now);game.State.EnterHole(id,player.CurrentHole,game.Now);}
   yield return Until(()=>FishingCeremony.Presenting,"golf awards opens",8);
   var closedRanking=FindFirstObjectByType<GolfLeaderboardUI>();yield return new WaitForSecondsRealtime(18);
   Check(FishingCeremony.Presenting&&!closedRanking.Expanded&&!closedRanking.BoardRect.gameObject.activeInHierarchy,"golf awards stays open past timer without automatically showing ranking");
   var show=FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).FirstOrDefault(b=>b.name=="Show island results");Check(show,"real Show results button exists");if(show)show.onClick.Invoke();yield return new WaitForSeconds(1);
   var ranking=FindFirstObjectByType<GolfLeaderboardUI>();Check(ranking&&ranking.Expanded&&!PostMatchFlow.Instance.Visible,"Show results opens golf leaderboard instead of next adventure");var resultArt=ranking.BoardRect.GetComponentsInChildren<UnityEngine.UI.Image>().First(i=>i.sprite&&i.name.Contains("Frame"));Check(resultArt.canvasRenderer.GetInheritedAlpha()>.99f,"ranking artwork visible above awards");
   var closeRect=(RectTransform)ranking.ToggleButton.transform;var resultPointer=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,closeRect.TransformPoint(closeRect.rect.center))};var resultHits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(resultPointer,resultHits);Check(resultHits.Count>0&&(resultHits[0].gameObject.transform==closeRect||resultHits[0].gameObject.transform.IsChildOf(closeRect)),"ranking close button reachable by pointer");
   Capture(Path.Combine(folder,"Golf-finished-results.jpg"));Check(ranking.BoardRect.pivot==new Vector2(.5f,.5f)&&ranking.BoardRect.anchoredPosition.sqrMagnitude<.01f,"results centered in awards safe area");
   ranking.ToggleButton.onClick.Invoke();yield return null;Check(!ranking.Expanded&&FishingCeremony.Presenting,"close ranking returns to awards");
   show.onClick.Invoke();yield return null;Check(ranking.Expanded,"results can reopen");ranking.BackdropButton.onClick.Invoke();yield return null;Check(!ranking.Expanded&&FishingCeremony.Presenting,"outside click closes ranking without dismissing awards");
   var next=FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).FirstOrDefault(b=>b.name=="Golf next adventure");Check(next,"golf awards has next adventure exit");next.onClick.Invoke();yield return null;Check(!FishingCeremony.Presenting&&PostMatchFlow.Instance.Visible,"next adventure exits awards and restores flow");
   game.ClearMatch();yield return null;yield return new WaitForEndOfFrame();Check(!ActivityCinematic.Instance.Visible,"golf exit clears cue");
  }
  IEnumerator BadgeCaptions(GolfMatchManager game){
   var language=GameLocalization.Current;bool motion=MenuPreferences.ReducedMotion;MenuPreferences.SetMotion(true);
   foreach(var selected in new[]{GameLanguage.Malay,GameLanguage.SimplifiedChinese}){
    GameLocalization.Select(selected,false);
    foreach(var kind in GolfEventSchedule.RandomKinds){
     game.Events.ReviewEvent(kind,game.Now-5);var state=game.Events.Snapshot;state.rushAt=game.Now-5;game.Events.Receive(state);
     yield return new WaitForSeconds(.1f);yield return new WaitForEndOfFrame();Canvas.ForceUpdateCanvases();
     foreach(var caption in GolfEventHUD.Instance.Card.GetComponentsInChildren<LocalizedArtwork>())if(caption.enabled&&caption.Caption.gameObject.activeInHierarchy){
      var text=caption.Caption;var mesh=text.canvasRenderer.GetMesh();bool fits=mesh&&mesh.vertexCount>0;var rect=text.rectTransform.rect;
      if(mesh)foreach(var vertex in mesh.vertices)fits&=vertex.x>=rect.xMin-.1f&&vertex.x<=rect.xMax+.1f&&vertex.y>=rect.yMin-.1f&&vertex.y<=rect.yMax+.1f;
      Check(fits&&((LocalizedText)text).HasUnclippedText(),selected+" "+kind+" caption including outline remains inside its plate");
      Check(text.lineSpacing<=.72f,selected+" event caption has compact line spacing");
     }
     Capture(Path.Combine(folder,"Badges-"+selected+"-"+kind+".jpg"));
    }
   }
   GameLocalization.Select(language,false);MenuPreferences.SetMotion(motion);game.Events.Clear();yield return null;
  }
  bool GearHit(){
   var gear=GolfHUD.Instance.IslandMenu.GearRect;var graphic=gear.GetComponent<UnityEngine.UI.Graphic>();
   var data=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,gear.TransformPoint(gear.rect.center))};var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(data,hits);
   return graphic.canvasRenderer.GetInheritedAlpha()>.99f&&hits.Count>0&&(hits[0].gameObject.transform==gear||hits[0].gameObject.transform.IsChildOf(gear));
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
  void Finish(){MenuPreferences.SetMotion(originalMotion);Time.captureFramerate=0;DevelopmentProbe.TurnCommand=default;DevelopmentProbe.TurnCommandActive=false;if(render){render.Release();Destroy(render);}if(pixels)Destroy(pixels);File.AppendAllText(Path.Combine(folder,"results.txt"),"ACTIVITY_CINEMATIC_COMPLETE success="+!failed+" checks="+checks+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
