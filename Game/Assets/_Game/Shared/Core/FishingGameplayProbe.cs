#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 public sealed class FishingGameplayProbe:MonoBehaviour {
  string[] args;string report;int checks;AppRoot app;FishingGame game;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var a=Environment.GetCommandLineArgs();if(a.Contains("-fishingGameplayAudit")||a.Contains("-networkFishingAudit"))new GameObject("Fishing gameplay review").AddComponent<FishingGameplayProbe>();}
  string Value(string key,string fallback){int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:fallback;}
  void Check(bool pass,string name){File.AppendAllText(report,(pass?"PASS ":"FAIL ")+name+"\n");if(!pass)throw new Exception(name);checks++;}
  IEnumerator Start(){
   args=Environment.GetCommandLineArgs();report=Value("-report",Path.Combine(Application.persistentDataPath,"fishing-review.txt"));DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   float end=Time.realtimeSinceStartup+65;while((!AppRoot.Instance||!AppRoot.Instance.Exploring||!FishingGame.Instance||!FishingGame.Instance.Context)&&Time.realtimeSinceStartup<end)yield return null;
   app=AppRoot.Instance;game=FishingGame.Instance;Check(app&&game&&game.Context,"FISHING_CONTEXT");
   end=Time.realtimeSinceStartup+15;while(!app.view.active&&Time.realtimeSinceStartup<end)yield return null;
   Check(app.view.active,"ARRIVAL_HANDS_CONTROL_TO_PLAYER");
   if(args.Contains("-networkFishingAudit"))yield return Network();else{Rules();yield return Offline();}
   File.AppendAllText(report,"FISHING_GAMEPLAY_COMPLETE checks="+checks+"\n");
  }
  void Rules()=>FishingRuleChecks.Run(Check);
  void Place(Athlete actor,int pier){var pos=game.Pier(pier);Check(Physics.Raycast(pos+Vector3.up*12,Vector3.down,out var hit,30,1<<8,QueryTriggerInteraction.Ignore),"PIER_FLOOR_"+pier);pos=hit.point+Vector3.up*.04f;actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(pos,Quaternion.LookRotation(Vector3.ProjectOnPlane(game.transform.position-pos,Vector3.up)));actor.ResetLocomotion();var net=actor.GetComponent<NetworkTransform>();if(net&&net.IsSpawned&&net.IsServer)net.Teleport(pos,actor.transform.rotation,Vector3.one);actor.capsule.enabled=game.Authority;Physics.SyncTransforms();app.view.yaw=actor.transform.eulerAngles.y;}
  IEnumerator AimFish(int fish,int mode){
   var view=app.view;view.mode=mode;view.pitch=30;view.yaw=app.LocalAthlete.transform.eulerAngles.y;yield return null;
   float limit=Time.time+2;
   while(Time.time<limit){
    var point=game.Presentation.TargetPosition(fish);
    if(mode==2){var delta=Vector3.ProjectOnPlane(point-app.LocalAthlete.transform.position,Vector3.up);view.yaw=Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg;view.pitch=20+(6.5f-delta.magnitude)/.08f;}
    else{var ray=view.FishingCamera.ScreenPointToRay(view.FishingAimScreenPoint);var local=view.FishingCamera.transform.InverseTransformDirection((point-ray.origin).normalized);var aim=view.FishingCamera.transform.InverseTransformDirection(ray.direction);view.yaw+=(Mathf.Atan2(local.x,local.z)-Mathf.Atan2(aim.x,aim.z))*Mathf.Rad2Deg*.8f;view.pitch-=(Mathf.Atan2(local.y,local.z)-Mathf.Atan2(aim.y,aim.z))*Mathf.Rad2Deg*.8f;}
    yield return null;
    if(view.SelectedFishingFish==fish&&game.VisibleTarget(app.LocalAthlete,view.FishingCamera,fish))yield break;
   }
   bool blocked=Physics.Linecast(view.FishingCamera.transform.position,game.Presentation.TargetPosition(fish),out var blocker,1<<8,QueryTriggerInteraction.Ignore);
   File.AppendAllText(report,"AIM_CONTEXT fish="+fish+" mode="+mode+" selected="+view.SelectedFishingFish+" available="+game.Available(app.LocalAthlete,fish)+" pitch="+view.pitch+" camera="+view.transform.position+" target="+game.Presentation.TargetPosition(fish)+" blocker="+(blocked?blocker.collider.name:"none")+"\n");Capture("aim-blocked");
  }
  IEnumerator CatchWithInput(bool touch,int size=0){
   var actor=app.LocalAthlete;var view=app.view;var ui=FindFirstObjectByType<FishingHUD>();var e=new PointerEventData(EventSystem.current){pointerId=71,button=PointerEventData.InputButton.Left};
   float end=Time.time+40;while(!game.Player(actor).HasValue||game.Player(actor).Value.phase!=FishingPhase.Ready){if(Time.time>end){Check(false,"READY_TIMEOUT");yield break;}yield return null;}
   int fish=game.NearestPier(actor)*FishingState.FishPerPier+size;yield return AimFish(fish,1);
   Check(view.SelectedFishingFish==fish&&ui.AimLocked,"RETICLE_HIGHLIGHTS_VISIBLE_FISH_"+size);yield return null;Capture("selected-"+size);
   if(touch){ui.OnPointerDown(e);ui.OnPointerUp(e);}else Check(view.BeginFishing(),"CAST_COMMAND_QUEUED");
   float until=Time.time+8;while(game.Player(actor).Value.phase!=FishingPhase.Bite&&Time.time<until)yield return null;
   Check(game.Player(actor).Value.phase==FishingPhase.Bite,"HOST_BITE_CUE_REPLICATED");
   if(touch){ui.OnPointerDown(e);ui.OnPointerUp(e);}else Check(view.BeginFishing(),"HOOK_COMMAND_QUEUED");
   until=Time.time+2;while(game.Player(actor).Value.phase!=FishingPhase.Reeling&&Time.time<until)yield return null;
   Check(game.Player(actor).Value.phase==FishingPhase.Reeling,"HOOK_ENTERS_REELING");
   yield return new WaitForSeconds(.15f);yield return PoseFrame();Check(ui.FitsSafeFrame()&&ui.RegionsSeparate(),"LIVE_GAUGES_FIT_WITHOUT_CONTROL_OVERLAP");Capture("hooked");CaptureGrip();Check(!view.SelectFishingFish(game.NearestPier(actor)*FishingState.FishPerPier+1),"CANNOT_RETARGET_ACTIVE_LINE");bool holding=false,danger=false,pointerChecked=false;int pulse=ui.ScorePulseCount;
   while(game.Player(actor).Value.phase==FishingPhase.Reeling&&Time.time<end){var p=game.Player(actor).Value;if(p.tension<.58f&&!holding){if(touch)ui.OnPointerDown(e);else view.BeginFishing();holding=true;if(touch&&!pointerChecked){pointerChecked=true;view.EndFishing(72);Check(view.FishingHeld,"OTHER_FINGER_CANNOT_RELEASE");}}else if(p.tension>.84f&&holding){if(touch)ui.OnPointerExit(e);else view.EndFishing();holding=false;}if(!danger&&p.tension>.74f){danger=true;Capture("danger-"+size);}yield return null;}
   if(touch)ui.OnPointerUp(e);else view.EndFishing();Check(game.Player(actor).Value.phase==FishingPhase.Caught,"INPUT_LANDS_CATCH_"+size);yield return null;Check(ui.ScorePulseCount>pulse,"LIVE_SCORE_AWARD_ANIMATES_"+size);Capture("landed-"+size);yield return new WaitForSeconds(.2f);
  }
  void Capture(string name){
   if(!args.Contains("-fishingCapture"))return;
   // Batch players do not present a back buffer. Render the real overlays with the scene.
   var camera=Camera.main;var target=new RenderTexture(Screen.width,Screen.height,24);var active=RenderTexture.active;var previous=camera.targetTexture;
   var overlays=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var cameras=overlays.Select(c=>c.worldCamera).ToArray();var distances=overlays.Select(c=>c.planeDistance).ToArray();
   try{camera.targetTexture=target;foreach(var c in overlays){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
    var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.ChangeExtension(report,name+".png"),image.EncodeToPNG());Destroy(image);
   }finally{for(int i=0;i<overlays.Length;i++){overlays[i].renderMode=RenderMode.ScreenSpaceOverlay;overlays[i].worldCamera=cameras[i];overlays[i].planeDistance=distances[i];}camera.targetTexture=previous;RenderTexture.active=active;target.Release();Destroy(target);Canvas.ForceUpdateCanvases();}
  }
  static IEnumerator PoseFrame(){if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)yield return null;else yield return new WaitForEndOfFrame();}
  void CaptureGrip(){
   if(!args.Contains("-fishingCapture"))return;var socket=app.LocalAthlete.visual.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Golf grip socket");
   var camera=Camera.main;var oldPosition=camera.transform.position;var oldRotation=camera.transform.rotation;float fov=camera.fieldOfView;
   camera.transform.position=socket.position+app.LocalAthlete.transform.right*.45f-app.LocalAthlete.transform.forward*.3f+Vector3.up*.15f;camera.transform.LookAt(socket.position);camera.fieldOfView=42;FindFirstObjectByType<DevelopmentProbe>()?.CaptureFrame(Path.ChangeExtension(report,"hand-grip.png"));camera.transform.SetPositionAndRotation(oldPosition,oldRotation);camera.fieldOfView=fov;
  }
  IEnumerator Offline(){
   if(args.Contains("-fishingCapture")){
    foreach(var page in new[]{"home","rules","help"}){
     app.Show(page);while(app.Menu.IsTransitioning)yield return null;yield return PoseFrame();
     Check(app.Menu.Content.GetComponentsInChildren<UnityEngine.UI.Text>().All(t=>!t.text.Contains("mechanics will come later")&&!t.text.Contains("not available yet")&&!t.text.Contains("exploration island for now")),"CURRENT_FISHING_MENU_"+page.ToUpperInvariant());Capture("menu-"+page);
     if(page=="rules")Check(app.Menu.Content.GetComponentsInChildren<UnityEngine.UI.Text>().Where(t=>new[]{"Round & Cup","Fish & bonuses","Crew Catch","Ready together"}.Contains(t.text)).Count(t=>t.cachedTextGenerator.vertexCount>0)==4,"FISHING_RULE_HEADINGS_RENDER");
    }
    app.Show("stadium");yield return null;while(app.Menu.IsTransitioning||!app.view.active)yield return null;
   }
   var actor=app.LocalAthlete;Check(game.State.Match.phase==FishingRoundPhase.Practice,"OFFLINE_ENTERS_FREE_FISHING");
   Check(!game.Execute(actor,FishingAction.Cast,game.State.Match.round,game.Player(actor).Value.sequence),"SPAWN_AWAY_FROM_PIER_CANNOT_CAST");
   for(int pier=0;pier<5;pier++){Place(actor,pier);yield return new WaitForSeconds(.15f);Check(game.NearestPier(actor)==pier,"FIVE_SUPPORTED_CASTING_PIERS_"+pier);}
   Place(actor,0);yield return new WaitForSeconds(.2f);Check(game.Presentation.LiveFishCount==FishingState.FishCount,"THIRTY_SWIMMING_FISH_USE_EXISTING_LODS");
   bool safe=true,moving=false;for(int i=0;i<FishingState.FishCount;i++)for(int t=0;t<20;t++){var p=game.FishPosition(i,t);safe&=Vector3.ProjectOnPlane(p-game.transform.position,Vector3.up).magnitude<22&&p.y<game.WaterY-.25f;moving|=Vector3.Distance(p,game.FishPosition(i,t+1))>.1f;}
   Check(safe&&moving,"SWIMMING_STAYS_INSIDE_LAGOON_BELOW_SURFACE");
   Check(Mathf.Abs(game.WaterHeight(game.Pier(0))-game.WaterY)<.037f,"FLOAT_FOLLOWS_BOUNDED_SURFACE_RIPPLES");
   var hud=FindFirstObjectByType<FishingHUD>();Check(hud.TypographyClean(),"HUD_NATIVE_FONT_NO_SYNTHETIC_BOLD_OR_STACKED_SHADOWS");Capture("crisp-practice");
   for(int pier=0;pier<FishingState.PierCount;pier++){
    Place(actor,pier);yield return new WaitForSeconds(.12f);
    for(int mode=0;mode<3;mode++)for(int slot=0;slot<FishingState.FishPerPier;slot++){int fish=pier*FishingState.FishPerPier+slot;yield return AimFish(fish,mode);Check(app.view.SelectedFishingFish==fish,"AIM_SIX_TARGETS_PIER_"+pier+"_CAMERA_"+mode+"_SLOT_"+slot);}
   }
   Place(actor,0);yield return new WaitForSeconds(.15f);hud.ReviewFrameInsets(new Vector2(96,40),new Vector2(24,16));yield return null;yield return null;
   for(int mode=0;mode<3;mode++){yield return AimFish(4,mode);yield return PoseFrame();Check(hud.AimLocked&&app.view.SelectedFishingFish==4&&Vector2.Distance(hud.AimScreenPoint,app.view.FishingCamera.pixelRect.center)>20,"RETICLE_TARGETS_WITH_ASYMMETRIC_SAFE_AREA_"+mode);Capture("aim-safe-insets-"+mode);}
   hud.ReviewFrameInsets(Vector2.zero,Vector2.zero);yield return null;yield return AimFish(1,1);uint castSequence=game.Player(actor).Value.sequence;
   PlayerView.LookDelta=new Vector2(90,15);yield return null;yield return null;Check(game.Player(actor).Value.sequence==castSequence&&game.Player(actor).Value.phase==FishingPhase.Ready,"AIM_DRAG_NEVER_CASTS");
   var claimed=game.State.Fish[1];game.ReviewSnapshot=true;game.State.Fish[1]=new FishingFishRecord{id=1,claimed=true,owner=0};yield return null;yield return null;Check(app.view.SelectedFishingFish!=1,"RETICLE_REJECTS_CLAIMED_FISH");game.State.Fish[1]=new FishingFishRecord{id=1,respawnAt=game.Now+7};yield return null;yield return null;Check(app.view.SelectedFishingFish!=1,"RETICLE_REJECTS_RESPAWNING_FISH");game.State.Fish[1]=claimed;game.ReviewSnapshot=false;
   yield return AimFish(1,1);var blocker=new GameObject("Temporary aim occlusion fixture");blocker.layer=8;var point=game.Presentation.TargetPosition(1);blocker.transform.SetPositionAndRotation(Vector3.Lerp(app.view.transform.position,point,.6f),Quaternion.LookRotation(point-app.view.transform.position));blocker.AddComponent<BoxCollider>().size=new Vector3(.6f,.6f,.2f);Physics.SyncTransforms();yield return null;yield return null;
   Check(!game.VisibleTarget(actor,app.view.FishingCamera,1)&&app.view.SelectedFishingFish!=1,"RETICLE_REJECTS_TERRAIN_OCCLUDED_TARGET");Destroy(blocker);yield return null;Physics.SyncTransforms();
   app.view.yaw+=180;yield return null;yield return null;Check(app.view.SelectedFishingFish<0&&!app.view.BeginFishing(),"OFFSCREEN_AIM_CANNOT_CAST_OR_REPLACE_TARGET");
   yield return AimFish(1,1);Check(app.view.BeginFishing(),"RETICLE_CAST_CONFIRMS_EXPLICIT_ID");Check(!app.view.BeginFishing(),"PENDING_CAST_CANNOT_REPEAT_OR_RETARGET");app.view.yaw+=90;yield return new WaitForSeconds(.2f);Check(game.Player(actor).Value.fish==1&&app.view.SelectedFishingFish==1,"QUEUED_CAST_FREEZES_TARGET_WHILE_CAMERA_MOVES");app.view.CancelFishing();yield return new WaitForSeconds(2.2f);
   yield return CatchWithInput(true);Check(game.Player(actor).Value.score==2&&game.Player(actor).Value.catches==1,"TOUCH_CATCH_SCORES_TWO");
   var rod=actor.GetComponent<FishingRodMotion>();Check(rod&&rod.Equipped&&rod.GripError<.002f,"AUTHORED_ROD_ALIGNED_WITH_HAND_SOCKET");
   Check(!game.StartMatch(),"START_REQUIRES_READY");app.view.ReadyFishing();yield return new WaitForSeconds(.2f);uint old=game.State.Match.round;Check(game.StartMatch()&&game.State.Match.round!=old&&game.Player(actor).Value.score==0,"HOST_START_RESETS_PRACTICE_AND_ROUND_TOKEN");
   Check(!game.Execute(actor,FishingAction.Cast,old,0),"LATE_PRACTICE_CAST_REJECTED_BY_NEW_MATCH");
   Check(game.State.Match.phase==FishingRoundPhase.Countdown&&!app.view.BeginFishing(),"LIVE_COUNTDOWN_BLOCKS_INPUT");yield return new WaitForSeconds(3.1f);
   yield return CatchWithInput(false);Check(game.Player(actor).Value.score==5,"KEYBOARD_CATCH_WANTED_BONUS");Check(FindFirstObjectByType<FishingHUD>().WantedText.Contains("SMALL")&&FindFirstObjectByType<FishingHUD>().WantedBonusText=="OK","WANTED_BADGE_CONFIRMS_LOCAL_BONUS_CLAIM");
   yield return new WaitForSeconds(2.1f);yield return AimFish(1,1);Check(app.view.BeginFishing(),"EXPLICIT_SECOND_CAST_QUEUED");yield return new WaitForSeconds(.2f);app.view.CancelFishing();yield return new WaitForSeconds(.2f);Check(!game.Busy(actor)&&game.Player(actor).Value.score==5,"CANCEL_INPUT_KEEPS_SCORE_AND_RELEASES_LINE");
   yield return new WaitForSeconds(2.1f);
   for(int mode=0;mode<3;mode++){yield return AimFish(1,mode);yield return PoseFrame();Check(float.IsFinite(app.view.transform.position.x)&&float.IsFinite(app.view.transform.position.y),"CAMERA_FINITE_"+mode);Check(app.view.SelectedFishingFish==1,"FISH_SELECTABLE_CAMERA_"+mode);Capture("camera-"+mode);}
   if(args.Contains("-fishingCapture")){
    yield return new WaitForSeconds(.5f);Check(FindFirstObjectByType<LagoonReflection>().Ready,"ENVIRONMENT_REFLECTION_CAPTURE_READY");Check(FindObjectsByType<Light>(FindObjectsSortMode.None).Any(l=>l.enabled&&l.type==LightType.Directional),"DIRECTIONAL_LIGHT_ACTIVE");
    string[] assets={"Flame","FishBadge","FisherBadge","ClockBadge","LagoonFrame","ReelButton"};int[] limits={64,128,128,128,256,256};
    for(int i=0;i<assets.Length;i++){var sprite=Resources.Load<Sprite>("FishingUI/"+assets[i]);Check(sprite&&sprite.texture.width<=limits[i]&&sprite.texture.height<=limits[i],"COMPRESSED_UI_SIZE_"+assets[i]);}
   }
   yield return StickerFixtures();yield return GripSportTransition();
  }
  IEnumerator GripSportTransition(){
   var actor=app.LocalAthlete;var departing=game.State;var motion=actor.GetComponent<FishingRodMotion>();var streaming=app.environments.GetComponent<SkySailStreaming>();
   yield return streaming.Prepare(SportId.Golf);app.SelectSport(SportId.Golf);yield return new WaitForSeconds(.2f);
   Check(!motion.Equipped&&departing.Match.phase==FishingRoundPhase.Idle&&departing.Fish.Count==0,"FISHING_EXIT_RELEASES_ROD_AND_SHARED_ROUND");
   var golf=GolfMatchManager.Instance;Check(golf&&golf.StartMatch(),"GOLF_AFTER_FISHING_CAN_START");app.view.mode=0;yield return new WaitForSeconds(.3f);yield return PoseFrame();
   var skins=actor.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(s=>s.sharedMesh&&s.sharedMesh.GetBlendShapeIndex("GolfRightGrip")>=0).ToArray();
   Check(actor.GolfClubMotion.Equipped&&skins.Length>0&&skins.All(s=>s.GetBlendShapeWeight(s.sharedMesh.GetBlendShapeIndex("GolfRightGrip"))>99),"FISHING_STOW_PRESERVES_GOLF_HAND_GRIP");
   if(SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null){var head=actor.visual.GetComponentsInChildren<Transform>(true).First(t=>t.name=="mixamorig:Head");Check(head.localScale.magnitude<.01f&&skins.All(s=>s.enabled),"GOLF_FIRST_PERSON_HANDOFF_KEEPS_BODY_AND_HIDES_HEAD");}
   yield return streaming.Prepare(SportId.Fishing);app.SelectSport(SportId.Fishing);app.view.mode=1;yield return new WaitForSeconds(.3f);yield return PoseFrame();game=FishingGame.Instance;
   Check(game&&game.State.Match.phase==FishingRoundPhase.Practice&&motion.Equipped&&skins.All(s=>s.GetBlendShapeWeight(s.sharedMesh.GetBlendShapeIndex("GolfRightGrip"))>99),"RETURN_TO_FISHING_REEQUIPS_ROD");
  }
  IEnumerator StickerFixtures(){
   if(!args.Contains("-fishingCapture"))yield break;
   var savedMatch=game.State.Match;var savedPlayers=game.State.Players.ToArray();var savedFish=game.State.Fish.ToArray();
   var fixture=new FishingState();fixture.Begin(new ulong[]{0,1,2,3,4},100,game.Now,true);
   int[] scores={17,19,12,5,2};for(int i=0;i<5;i++){var p=fixture.Players[i];p.score=scores[i];fixture.Players[i]=p;}
   game.ReviewSnapshot=true;game.State.Receive(fixture.Match,fixture.Players,fixture.Fish);app.view.mode=1;yield return new WaitForSeconds(.35f);Canvas.ForceUpdateCanvases();Check(FindFirstObjectByType<FishingHUD>().VisibleScoreCount==5,"FIVE_PLAYER_SCORE_DIGITS_RENDER");Capture("five-player-rank");
   var ui=FindFirstObjectByType<FishingHUD>();int pulses=ui.ScorePulseCount;var leader=game.State.Players[0];leader.score=25;leader.catches++;leader.lastAward=8;leader.lastBonus=3;leader.awardSequence++;game.State.Players[0]=leader;
   yield return null;yield return null;Check(ui.RowsMoving&&ui.ScorePulseCount==pulses+1,"RANK_REORDER_AND_SCORE_POP");Capture("rank-overtake");yield return new WaitForSeconds(.75f);Capture("wanted-pop-three");
   leader=game.State.Players[0];leader.phase=FishingPhase.Reeling;leader.fish=0;leader.origin=app.LocalAthlete.transform.position;leader.castDistance=8;leader.distance=4;leader.lineLength=4;leader.tension=.85f;leader.progress=.65f;game.State.Players[0]=leader;
   int[][] resolutions={new[]{844,390},new[]{1024,768},new[]{1920,810},new[]{390,844}};
   foreach(var size in resolutions){Screen.SetResolution(size[0],size[1],false);yield return new WaitForSeconds(.6f);Canvas.ForceUpdateCanvases();Check(ui.FitsSafeFrame()&&ui.RegionsSeparate(),"SAFE_FRAME_NO_OVERLAP_"+size[0]+"x"+size[1]);Check(ui.VisibleScoreCount==5,"SCORE_DIGITS_"+size[0]+"x"+size[1]);Capture("layout-"+size[0]+"x"+size[1]);}
   Screen.SetResolution(844,390,false);ui.ReviewFrameInsets(new Vector2(96,40),new Vector2(24,16));yield return new WaitForSeconds(.6f);Check(ui.FitsSafeFrame()&&ui.RegionsSeparate(),"ASYMMETRIC_SAFE_FRAME_INSETS");Capture("safe-insets");ui.ReviewFrameInsets(Vector2.zero,Vector2.zero);
   var nextMatch=game.State.Match;nextMatch.started=game.Now-55;nextMatch.deadline=nextMatch.started+180;game.State.Receive(nextMatch,game.State.Players.ToArray(),game.State.Fish.ToArray());yield return new WaitForSeconds(.3f);Check(ui.WantedText.Contains("NEXT")&&ui.WantedText.Contains("MEDIUM")&&ui.WantedBonusText=="+3","WANTED_NEXT_TARGET_PREVIEW");Capture("wanted-next");
   nextMatch.started=game.Now-174;nextMatch.deadline=nextMatch.started+180;game.State.Receive(nextMatch,game.State.Players.ToArray(),game.State.Fish.ToArray());yield return new WaitForSeconds(.3f);Capture("timer-urgent");
   nextMatch.mode=FishingMode.Crew;nextMatch.crewTarget=100;game.State.Receive(nextMatch,game.State.Players.ToArray(),game.State.Fish.ToArray());yield return new WaitForSeconds(.3f);Check(ui.FitsSafeFrame()&&ui.RegionsSeparate(),"CREW_CHECKLIST_FITS_EXTENDED_CLOCK_FRAME");Capture("crew-card");
   Screen.SetResolution(1600,900,false);yield return new WaitForSeconds(.5f);game.State.Receive(savedMatch,savedPlayers,savedFish);game.ReviewSnapshot=false;
  }
  IEnumerator Network(){
   var actor=app.LocalAthlete;float end=Time.time+15;
   while((game.State.Players.Count!=2||!game.Player(actor).HasValue)&&Time.time<end)yield return null;
   Check(game.State.Players.Count==2,"BOTH_PEERS_JOIN_PRACTICE");app.view.ReadyFishing();yield return new WaitForSeconds(.25f);
   if(game.Authority){
    end=Time.time+10;while(!game.State.AllReady&&Time.time<end)yield return null;Check(game.State.AllReady,"BOTH_READY_REPLICATED");
    var guest=Athlete.Active.First(a=>a.GetComponent<NetworkAthlete>()&&a.GetComponent<NetworkAthlete>().OwnerClientId!=0);Place(actor,0);Place(guest,1);
    Check(game.StartMatch()&&game.State.Players.Count==2,"HOST_STARTS_TWO_PLAYER_CONTEST");yield return new WaitForSeconds(3.2f);
    end=Time.time+40;while(game.Player(guest).Value.catches==0&&Time.time<end)yield return null;
    Check(game.Player(guest).Value.catches==1&&game.Player(guest).Value.score==5&&game.Player(actor).Value.score==0,"GUEST_TARGET_RPC_HOST_ONLY_AWARDS");
    Check(game.State.Fish.All(f=>!f.claimed),"GUEST_LANDING_RELEASES_RESERVATION");
    yield return CatchWithInput(false,2);Check(game.Player(actor).Value.score==10&&game.State.Rank(0)==1&&game.State.Rank(FishingGame.Key(guest))==2,"HOST_LARGE_CATCH_OVERTAKES_GUEST");
    yield return new WaitForSeconds(1);uint old=game.State.Match.round;game.State.Advance(game.State.Match.deadline,0);yield return new WaitForSeconds(.6f);Check(game.State.Match.phase==FishingRoundPhase.Ended,"HOST_SHARED_RESULTS_DEADLINE");
    app.view.ReadyFishing();end=Time.time+10;while(!game.State.AllReady&&Time.time<end)yield return null;Check(game.StartMatch()&&game.State.Match.round!=old&&game.State.Players.All(p=>p.score==0),"READY_REMATCH_RESETS_BOTH_PEERS");
   }else{
    end=Time.time+15;while((game.State.Match.phase!=FishingRoundPhase.Playing||game.NearestPier(actor)<0)&&Time.time<end)yield return null;
    Check(game.State.Match.phase==FishingRoundPhase.Playing,"GUEST_RECEIVES_COUNTDOWN_AND_PLAYING");Check(!game.StartMatch(),"GUEST_CANNOT_START");
    var p=game.Player(actor).Value;actor.GetComponent<NetworkAthlete>().FishingRpc(FishingAction.Cast,game.State.Match.round-1,p.sequence,FishingState.FishPerPier);yield return new WaitForSeconds(.2f);Check(game.Player(actor).Value.phase==FishingPhase.Ready,"STALE_GUEST_ROUND_REJECTED");
    actor.GetComponent<NetworkAthlete>().FishingRpc(FishingAction.Cast,game.State.Match.round,p.sequence,0);yield return new WaitForSeconds(.2f);Check(game.Player(actor).Value.phase==FishingPhase.Ready,"GUEST_CANNOT_SELECT_OTHER_PIER_FISH");
    yield return CatchWithInput(true);Check(game.Player(actor).Value.score==5&&game.State.Player(0).Value.score==0,"GUEST_OBSERVES_AUTHORITATIVE_BONUS");
    end=Time.time+40;while(game.State.Player(0).Value.catches==0&&Time.time<end)yield return null;yield return new WaitForSeconds(.15f);
    Check(game.State.Player(0).Value.score==10&&game.State.Rank(FishingGame.Key(actor))==2,"GUEST_OBSERVES_OTHER_PLAYER_OVERTAKE");
    end=Time.time+8;while(game.State.Match.phase!=FishingRoundPhase.Ended&&Time.time<end)yield return null;Check(game.State.Match.phase==FishingRoundPhase.Ended,"GUEST_RECEIVES_FINAL_RESULTS");
    uint old=game.State.Match.round;app.view.ReadyFishing();end=Time.time+8;while(game.State.Match.round==old&&Time.time<end)yield return null;Check(game.State.Match.round!=old&&game.Player(actor).Value.score==0,"GUEST_RECEIVES_CLEAN_REMATCH");
   }
  }
 }
}
#endif
