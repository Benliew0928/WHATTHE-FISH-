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
    var target=game.FishPosition(0,game.Now);actor.capsule.enabled=false;actor.transform.position=target+Vector3.right*15;actor.capsule.enabled=true;Physics.SyncTransforms();
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
 }
}
#endif
