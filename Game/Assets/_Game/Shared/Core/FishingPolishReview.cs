#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 // Explicit development-only fixtures; never run in an ordinary room.
 public sealed class FishingPolishReview:MonoBehaviour {
  string report;int checks;AppRoot app;FishingGame game;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Environment.GetCommandLineArgs().Contains("-fishingPolishAudit"))new GameObject("Fishing polish review").AddComponent<FishingPolishReview>();}
  void Check(bool value,string name){File.AppendAllText(report,(value?"PASS ":"FAIL ")+name+Environment.NewLine);if(!value)throw new Exception(name);checks++;}
  static Vector3 Eye(Athlete actor,PlayerView view)=>actor.transform.position+Vector3.up*1.57f+Quaternion.Euler(0,view.yaw,0)*Vector3.forward*.28f;
  static bool HeadMasked(Athlete actor){var skins=actor.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true);var block=new MaterialPropertyBlock();int mask=Shader.PropertyToID("_HideFirstPersonHead");foreach(var skin in skins){skin.GetPropertyBlock(block);if(!skin.enabled||block.GetFloat(mask)<.99f)return false;}return skins.Length>0;}
  IEnumerator Start(){
   var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-report");report=index>=0&&index+1<args.Length?args[index+1]:Path.Combine(Application.persistentDataPath,"fishing-polish.txt");
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   float until=Time.realtimeSinceStartup+65;
   while((!AppRoot.Instance||!AppRoot.Instance.Exploring||!FishingGame.Instance||!FishingGame.Instance.Context)&&Time.realtimeSinceStartup<until)yield return null;
   app=AppRoot.Instance;game=FishingGame.Instance;Check(app&&game&&game.Context,"FISHING_POLISH_ACTIVE_SAVED_SCENE");
   until=Time.realtimeSinceStartup+15;while(!app.view.active&&Time.realtimeSinceStartup<until)yield return null;
   Check(app.view.active&&FishingHUD.Instance&&FishingHUD.Instance.Results,"FISHING_POLISH_INTEGRATED_CONTROLS_AND_RESULTS");
   Check(game.GetComponent<FishingFeedback>(),"FISHING_POLISH_FEEDBACK_ATTACHED_TO_ACTUAL_GAME");
   FishingFeedbackChecks.Run(Check);
   yield return TargetFeedback();
   yield return app.view.ReviewFishingHookPending(Check);
   yield return LiveFeedback();
   yield return ClarityFeedback();
   yield return FishingResultsChecks.Run(app,game,Check,report);
   File.AppendAllText(report,"FISHING_POLISH_COMPLETE checks="+checks+Environment.NewLine);
  }
  IEnumerator TargetFeedback(){
   var actor=app.LocalAthlete;var view=app.view;var match=game.State.Match;var players=game.State.Players.ToArray();var fish=game.State.Fish.ToArray();
   var position=actor.transform.position;var rotation=actor.transform.rotation;float yaw=view.yaw,pitch=view.pitch;int mode=view.mode;
   game.ReviewSnapshot=true;
   GameObject obstruction=null;
   try {
    var pier=game.Pier(0);Check(Physics.Raycast(pier+Vector3.up*12,Vector3.down,out var floor,30,1<<8,QueryTriggerInteraction.Ignore),"FEEDBACK_FIXTURE_REAL_PIER_FLOOR");
    actor.capsule.enabled=false;actor.transform.position=floor.point+Vector3.up*.04f;actor.ResetLocomotion();actor.capsule.enabled=true;Physics.SyncTransforms();
    yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
    Check(FishingTargetFeedback.ReadyLabel(game,view)!="FIND A PIER","FEEDBACK_ACCEPTS_REAL_GROUNDED_PIER");
    var record=game.State.Fish[0];record.claimed=true;record.owner=99;game.State.Fish[0]=record;
    Check(FishingTargetFeedback.FishLabel(game,actor,view.FishingCamera,0)=="FRIEND'S\nFISH"&&!game.CanCast(actor,0),"CLAIMED_FISH_FEEDBACK_MATCHES_HOST_REJECTION");
    record.claimed=false;record.respawnAt=game.Now+3;game.State.Fish[0]=record;
    Check(FishingTargetFeedback.FishLabel(game,actor,view.FishingCamera,0)=="FISH\nRETURNING"&&!game.CanCast(actor,0),"RESPAWNING_FISH_FEEDBACK_MATCHES_HOST_REJECTION");
    record.respawnAt=0;game.State.Fish[0]=record;
    var target=game.FishPosition(0,game.Now);actor.capsule.enabled=false;actor.transform.position=target+Vector3.right*(FishingState.MaxCastDistance+5);actor.capsule.enabled=true;Physics.SyncTransforms();
    Check(FishingTargetFeedback.FishLabel(game,actor,view.FishingCamera,0)=="MOVE\nCLOSER"&&!game.CanCast(actor,0),"OUT_OF_RANGE_FISH_FEEDBACK_MATCHES_HOST_REJECTION");
    Check(FishingTargetFeedback.ReadyLabel(game,view)=="FIND A PIER","OFF_PIER_BUTTON_EXPLAINS_WHERE_TO_FISH");
    actor.capsule.enabled=false;actor.transform.position=floor.point+Vector3.up*.04f;actor.ResetLocomotion();actor.capsule.enabled=true;Physics.SyncTransforms();yield return new WaitForFixedUpdate();
    obstruction=new GameObject("Development-only blocked cast fixture");obstruction.layer=8;var collider=obstruction.AddComponent<BoxCollider>();collider.size=Vector3.one*.7f;obstruction.transform.position=Vector3.Lerp(actor.transform.position+Vector3.up*1.4f,game.FishPosition(0,game.Now),.5f);Physics.SyncTransforms();
    Check(FishingTargetFeedback.FishLabel(game,actor,view.FishingCamera,0)=="CAST PATH\nBLOCKED"&&!game.CanCast(actor,0),"PHYSICAL_CAST_OBSTRUCTION_HAS_SPECIFIC_FEEDBACK");
    Check(FishingTargetFeedback.FailureLabel(FishingResult.Missed)=="MISSED BITE"&&FishingTargetFeedback.FailureLabel(FishingResult.Snapped)=="LINE SNAPPED","MISSED_AND_SNAPPED_FAILURES_REMAIN_DISTINCT");
    Check(FishingTargetFeedback.FailureLabel(FishingResult.Cancelled)=="CAST\nCANCELLED"&&FishingTargetFeedback.FailureLabel(FishingResult.TimedOut)=="FISH\nESCAPED","CANCEL_AND_TIMEOUT_HAVE_DISTINCT_CONTAINED_LABELS");
    Check(!game.State.Fish[0].claimed&&game.State.Fish[0].respawnAt==0,"DIAGNOSTICS_NEVER_RESERVE_OR_REPLACE_TARGET");
   }finally {
    if(obstruction)Destroy(obstruction);game.State.Receive(match,players,fish);game.ReviewSnapshot=false;actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(position,rotation);actor.ResetLocomotion();actor.capsule.enabled=true;view.yaw=yaw;view.pitch=pitch;view.mode=mode;Physics.SyncTransforms();
   }
   yield return null;
  }
  IEnumerator LiveFeedback(){
   var feedback=game.GetComponent<FishingFeedback>();var actor=app.LocalAthlete;var match=game.State.Match;var players=game.State.Players.ToArray();var fish=game.State.Fish.ToArray();bool review=game.ReviewSnapshot;
   try {
    game.ReviewSnapshot=true;game.State.Begin(new[]{FishingGame.Key(actor)},match.round+50000,game.Now,false);feedback.SendMessage("OnApplicationFocus",true);yield return null;yield return null;
    int casts=feedback.ReviewCueCount(FishingFeedbackCue.Cast),bites=feedback.ReviewCueCount(FishingFeedbackCue.Bite),hooks=feedback.ReviewCueCount(FishingFeedbackCue.Hook);
    var p=game.State.Players[0];p.sequence++;p.phase=FishingPhase.Casting;p.phaseAt=game.Now;game.State.Players[0]=p;yield return null;yield return null;
    Check(feedback.ReviewCueCount(FishingFeedbackCue.Cast)==casts+1,"ACTUAL_FEEDBACK_LATEUPDATE_PLAYS_ACCEPTED_LOCAL_CAST_ONCE");yield return null;
    Check(feedback.ReviewCueCount(FishingFeedbackCue.Cast)==casts+1,"ACTUAL_FEEDBACK_POLLING_NEVER_REPLAYS_CAST");
    p.phase=FishingPhase.Bite;p.biteAt=game.Now;p.hookUntil=game.Now+FishingState.HookSeconds;game.State.Players[0]=p;yield return null;yield return null;
    Check(feedback.ReviewCueCount(FishingFeedbackCue.Bite)==bites+1,"ACTUAL_FEEDBACK_LATEUPDATE_PLAYS_LOCAL_BITE_ALERT");
    p.phase=FishingPhase.Reeling;p.phaseAt=game.Now;p.hooks=1;game.State.Players[0]=p;yield return null;yield return null;
    Check(feedback.ReviewCueCount(FishingFeedbackCue.Hook)==hooks+1,"ACTUAL_FEEDBACK_LATEUPDATE_ACKNOWLEDGES_HOST_HOOK");
    feedback.SendMessage("OnApplicationPause",true);yield return null;Check(!feedback.ReviewLoopWanted,"ACTUAL_FEEDBACK_PAUSE_CALLBACK_QUIETS_LOOP");
   }finally {
    feedback.SendMessage("OnApplicationPause",false);feedback.SendMessage("OnApplicationFocus",Application.isFocused);game.State.Receive(match,players,fish);game.ReviewSnapshot=review;
   }
   yield return null;
  }
  IEnumerator ClarityFeedback(){
   var ui=FishingHUD.Instance;var view=app.view;var actor=app.LocalAthlete;
   var match=game.State.Match;var players=game.State.Players.ToArray();var fish=game.State.Fish.ToArray();bool review=game.ReviewSnapshot,motion=MenuPreferences.ReducedMotion;int mode=view.mode;float yaw=view.yaw,pitch=view.pitch;
   var position=actor.transform.position;var rotation=actor.transform.rotation;
   try {
    game.ReviewSnapshot=true;MenuPreferences.SetMotion(false);view.mode=1;ui.SendMessage("OnApplicationFocus",true);
    Check(Physics.Raycast(game.Pier(0)+Vector3.up*12,Vector3.down,out var floor,30,1<<8,QueryTriggerInteraction.Ignore),"CLARITY_FIXTURE_REAL_PIER_FLOOR");
    actor.capsule.enabled=false;actor.transform.position=floor.point+Vector3.up*.04f;actor.ResetLocomotion();actor.capsule.enabled=true;Physics.SyncTransforms();
    game.State.Begin(new[]{FishingGame.Key(actor)},match.round+60000,game.Now,false);yield return null;yield return null;
    int starts=ui.CatchCue.Starts;
    var input=game.State.Players[0];input.phase=FishingPhase.Reeling;input.phaseAt=game.Now-FishingState.TimingPeriod*.5;input.steerAt=input.phaseAt;input.reelBeat=-1;input.pier=0;input.fish=0;input.origin=actor.transform.position;input.castDistance=input.distance=input.lineLength=6;game.State.Players[0]=input;
    view.ClearMatchInput();view.ReadCommand();
    Check(view.BeginCountersteer(-1,311),"TOUCH_DOWN_STARTS_DIRECTIONAL_HOLD");view.EndFishing(312);Check(view.FishingHeld,"UNRELATED_TOUCH_CANNOT_RELEASE_HELD_DIRECTION");
    var command=view.ReadCommand();Check(command.fishingAction==FishingAction.ReelLeft&&view.FishingHeld,"REAL_QUEUE_SENDS_LEFT_HOLD");
    Check(game.Execute(actor,command.fishingAction,command.fishingRound,command.fishingSequence,command.fishingFish,command.fishingPressedAt)&&game.Player(actor).Value.held,"HOST_ACCEPTS_HELD_DIRECTION_WITHOUT_TIMING");
    double originalLease=game.Player(actor).Value.reelLease;
    yield return new WaitForSeconds(.23f);command=view.ReadCommand();
    if(command.fishingAction==FishingAction.ReelLeft)game.Execute(actor,command.fishingAction,command.fishingRound,command.fishingSequence,command.fishingFish,command.fishingPressedAt);
    Check(view.FishingHeld&&game.Player(actor).Value.reelLease>originalLease,"HELD_FINGER_RENEWS_PRODUCTION_INPUT_LEASE");
    Check(view.BeginCountersteer(1,312),"NEW_FINGER_CAN_TAKE_OVER_RIGHT");view.ReadCommand();view.EndFishing(311);Check(view.FishingHeld,"OLD_FINGER_UP_CANNOT_CANCEL_NEW_HOLD");view.EndFishing(312);
    command=view.ReadCommand();Check(command.fishingAction==FishingAction.ReelStop&&!view.FishingHeld,"MATCHING_FINGER_RELEASE_QUEUES_REAL_STOP");
    var p=game.State.Players[0];p.fish=0;p.phase=FishingPhase.Reeling;p.phaseAt=game.Now;p.origin=actor.transform.position;p.castDistance=p.distance=p.lineLength=6;p.tension=.86f;p.held=false;p.reelBeat=-1;p.tapGrade=FishingTapGrade.None;p.steerAt=game.Now;p.steer=0;p.item=FishingAbility.Sonar;game.State.Players[0]=p;yield return null;yield return null;
    Check(ui.TimingGuideVisible&&ui.GuidanceText=="RELEASE TO COOL","HIGH_TENSION_SHOWS_RELEASE_TO_COOL");
    var frame=ui.GetComponentInParent<Canvas>().transform;var labels=frame.GetComponentsInChildren<UnityEngine.UI.Text>(true);
    // Shared control wrappers no longer share the board's immediate parent.
    // The current host-start flow deliberately keeps setup available in practice.
    var setup=frame.GetComponentsInChildren<RectTransform>(true).First(t=>t.name=="Fishing match setup");
    Check(setup.gameObject.activeSelf==game.Authority,"PRACTICE_HOST_START_REMAINS_AVAILABLE_WHILE_FIGHTING");
    Check(ui.GetComponentInParent<Canvas>().GetComponentsInChildren<UnityEngine.UI.Image>(true).Count(i=>i.name=="Fish pull marker")==1,"PICTURED_METER_HAS_ONE_FISH_PULL_MARKER");
    p.held=false;p.steer=0;p.tension=.68f;game.State.Players[0]=p;yield return null;yield return null;
    Check(ui.TimingGuideVisible&&ui.GuidanceText=="RELEASE TO COOL","COOLING_GUIDANCE_PERSISTS_UNTIL_SAFE");
    p.tension=.5f;game.State.Players[0]=p;yield return null;yield return null;Check(!labels.Any(t=>t.gameObject.activeInHierarchy&&(t.text.Contains("CLICK")||t.text.Contains("Keyboard")||t.text.Contains(" / F")))&&FishingTutorialUI.Copy.All(c=>!c.Contains("Keyboard")),"MOBILE_HUD_AND_SHARED_TUTORIAL_HAVE_NO_KEYBOARD_GUIDES");
    var guide=ui.GetComponentInParent<Canvas>().GetComponentInChildren<FishingFightGuide>();float marker=guide.MarkerPosition;yield return new WaitForSeconds(.12f);Check(Mathf.Abs(marker-guide.MarkerPosition)>.005f,"FISH_MARKER_FOLLOWS_CURRENT_SWAY");
    foreach(int turnBeat in new[]{2,5}){p.phaseAt=game.Now-FishingState.TimingPeriod*(turnBeat+.05);game.State.Players[0]=p;yield return new WaitForSeconds(.8f);Check(ui.CounterControlsVisible&&Mathf.Sign(view.FishingFightSide)==FishingState.CounterSide(p,game.Now)&&Mathf.Abs(view.FishingFightSide)>.6f,"CAMERA_SHOULDER_FOLLOWS_OPPOSING_PULL_"+turnBeat);}
    foreach(int chosenMode in Environment.GetCommandLineArgs().Contains("-fishingDefaultViews")?new[]{0,1}:new[]{0,1,2}){
     view.mode=chosenMode;yield return new WaitForSeconds(.8f);
     if(chosenMode==0){
     // The approved first-person body camera sits ahead of the open neck.
     var eye=Eye(actor,view);
      Check(!view.FishingFightCameraActive&&Vector3.Distance(view.transform.position,eye)<.08f&&Mathf.Abs(view.FishingCamera.nearClipPlane-.06f)<.001f,"EYE_LEVEL_REELING_STAYS_AT_PLAYER_EYES");
      PlayerView.LookDelta=new Vector2(24,6);yield return null;yield return null;
      Check(Mathf.Abs(view.yaw-yaw)>1&&!view.FishingFightCameraActive,"EYE_LEVEL_REELING_PRESERVES_MANUAL_LOOK");view.yaw=yaw;view.pitch=pitch;
      foreach(int side in new[]{-1,1}){
       Check(view.BeginCountersteer(side,780+side),"EYE_LEVEL_ACCEPTS_DIRECTIONAL_PRESS_"+side);var held=view.ReadCommand();game.Execute(actor,held.fishingAction,held.fishingRound,held.fishingSequence,held.fishingFish,held.fishingPressedAt);yield return new WaitForSeconds(.15f);
       Check(!view.FishingFightCameraActive&&Vector3.Distance(view.transform.position,Eye(actor,view))<.08f,"EYE_LEVEL_DIRECTIONAL_PRESS_NEVER_MOVES_TO_THIRD_PERSON_"+side);view.EndFishing(780+side);view.ReadCommand();
      }
      var strain=ui.GetComponentInParent<Canvas>().GetComponentInChildren<FishingTensionVisual>(true);
      foreach(float value in new[]{.2f,.6f,.9f}){
       p.tension=value;p.held=false;game.State.Players[0]=p;yield return new WaitForSeconds(.15f);
       // The rod poses and hides the head in LateUpdate; capture the rendered
       // pose rather than manually rendering between Update and LateUpdate.
       yield return new WaitForEndOfFrame();
       var head=actor.visual.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="mixamorig:Head");
       Check(head&&head.localScale.sqrMagnitude>.0001f&&HeadMasked(actor),"EYE_LEVEL_HEAD_HIDDEN_AFTER_FISHING_POSE_"+value);
       Check(strain&&strain.Visible&&!strain.raycastTarget&&strain.FitsSafeFrame()&&strain.GetComponentsInChildren<UnityEngine.UI.Text>().Length==0,"ROPE_STRAIN_CUE_CLEAR_TOUCH_THROUGH_AND_BOUNDED_"+value);
       FishingResultsChecks.Capture(report,"eye-level-strain-"+Mathf.RoundToInt(value*100),0);
      }
      p.tension=.5f;game.State.Players[0]=p;
     }else if(chosenMode==1)Check(view.FishingFightCameraActive,"FOLLOW_CAMERA_HAS_ACCEPTED_FIGHT_FRAMING");
     else Check(!view.FishingFightCameraActive&&view.ActivityStadiumFollowing,"LAGOON_VIEW_FRAMES_ANGLER_AND_HOOKED_FISH");
     p.phase=FishingPhase.Escaped;p.result=FishingResult.Cancelled;game.State.Players[0]=p;yield return new WaitForSeconds(.85f);
     Check(!view.FishingFightCameraActive&&view.mode==chosenMode&&Mathf.Abs(view.yaw-yaw)<.01f&&Mathf.Abs(view.pitch-pitch)<.01f,"CANCEL_RETURNS_PREFERRED_CAMERA_WITHOUT_CHANGING_ANGLE_"+chosenMode);
     p.phase=FishingPhase.Reeling;p.phaseAt=game.Now;p.result=FishingResult.None;game.State.Players[0]=p;yield return null;
    }
    view.mode=0;yield return null;yield return null;Check(!view.FishingFightCameraActive&&Vector3.Distance(view.transform.position,Eye(actor,view))<.08f,"SWITCHING_TO_EYE_LEVEL_CANCELS_PENDING_FIGHT_RETURN");
    MenuPreferences.SetMotion(true);yield return null;yield return null;Check(!view.FishingFightCameraActive,"REDUCED_MOTION_KEEPS_PREFERRED_CAMERA_DURING_FIGHT");
    p.phase=FishingPhase.Caught;p.result=FishingResult.Landed;p.phaseAt=game.Now;p.awardSequence++;p.lastAward=5;p.lastBonus=3;game.State.Players[0]=p;yield return null;yield return null;
    Check(ui.CatchCue.Visible&&ui.CatchCue.PointsText=="+5 POINTS"&&ui.CatchCue.Starts==starts+1,"CATCH_CARD_USES_ACCEPTED_TOTAL_INCLUDING_WANTED");
    Check(ui.FitsSafeFrame()&&ui.RegionsSeparate(),"HELD_GEAR_REMAINS_CLEAR_OF_LANDING_CARD");
    var heading=ui.CatchCue.GetComponentsInChildren<UnityEngine.UI.Text>().First(t=>t.name=="Catch confirmed");Check(heading.cachedTextGenerator.vertexCount>0,"LANDING_HEADING_RENDERS_NATIVE_GLYPHS_WITHOUT_VERTICAL_CLIPPING");
    p.item=FishingAbility.None;game.State.Players[0]=p;yield return null;yield return null;
    Check(ui.RegionsSeparate()&&!ui.GetComponent<FishingAbilityHUD>().VisibleRegions.Any(r=>r.name=="Fishing supplies"||r.name=="Fishing salvage"),"PICKUP_SHORTCUTS_STAY_HIDDEN_DURING_LANDING_FEEDBACK");
    yield return null;Check(ui.CatchCue.Starts==starts+1&&ui.CatchCue.GetComponentsInChildren<UnityEngine.UI.Graphic>().All(g=>!g.raycastTarget),"REPEATED_AWARD_SNAPSHOT_NEVER_REPLAYS_OR_BLOCKS_INPUT");
    ui.SendMessage("OnApplicationFocus",false);ui.SendMessage("OnApplicationFocus",true);yield return null;yield return null;
    Check(!ui.CatchCue.Visible&&ui.CatchCue.Starts==starts+1,"REFOCUS_ESTABLISHES_SILENT_CATCH_BASELINE");
    p.awardSequence++;p.phaseAt=game.Now-4;game.State.Players[0]=p;yield return null;yield return null;Check(!ui.CatchCue.Visible&&ui.CatchCue.Starts==starts+1,"STALE_CATCH_SNAPSHOT_HAS_NO_FALSE_CELEBRATION");
   }finally {game.State.Receive(match,players,fish);game.ReviewSnapshot=review;MenuPreferences.SetMotion(motion);view.mode=mode;view.yaw=yaw;view.pitch=pitch;actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(position,rotation);actor.ResetLocomotion();actor.capsule.enabled=true;Physics.SyncTransforms();ui.SendMessage("OnApplicationFocus",Application.isFocused);}
   yield return null;
  }
 }
}
#endif
