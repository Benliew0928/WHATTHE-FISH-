using UnityEngine;

namespace WhatTheFish {
 public sealed partial class PlayerView {
  FishingAction pendingFishing;double pendingFishingPressedAt;uint pendingFishingRound,pendingFishingSequence;int pendingFishingFish;
  bool fishingHookPending;uint fishingHookRound,fishingHookSequence;float fishingHookQueuedAt;
  bool fishingHeld;int fishingSource;float fishingRenewAt;
  float fishingAimFrozenUntil,fishingCameraBlend,fishingPierCameraUntil;
  public bool FishingHeld=>fishingHeld;
  public Camera FishingCamera=>cam;
  public Vector2 FishingAimScreenPoint=>FishingHUD.Instance?FishingHUD.Instance.AimScreenPoint:cam.pixelRect.center;
  public bool FishingCastPending {get{var g=FishingGame.Instance;var p=g?g.Player(target):null;return p.HasValue&&p.Value.phase==FishingPhase.Ready&&g.State.Match.round==pendingFishingRound&&p.Value.sequence==pendingFishingSequence&&(pendingFishing==FishingAction.Cast||Time.unscaledTime<fishingAimFrozenUntil);}}
  public bool FishingHookPending {get{var g=FishingGame.Instance;var p=g?g.Player(target):null;if(!fishingHookPending)return false;if(!active||!g||!g.Context||!p.HasValue||!p.Value.connected||p.Value.phase!=FishingPhase.Bite||g.State.Match.round!=fishingHookRound||p.Value.sequence!=fishingHookSequence){fishingHookPending=false;return false;}return Time.unscaledTime-fishingHookQueuedAt<.5f;}}
  public int SelectedFishingFish {get;private set;}=-1;
  public uint SelectionRevision {get;private set;}
  public bool SelectFishingFish(int fish){
   var g=FishingGame.Instance;var p=g?g.Player(target):null;
   if(!active||!g||!g.Context||!p.HasValue||p.Value.phase!=FishingPhase.Ready||!g.Available(target,fish))return false;
   if(SelectedFishingFish!=fish){SelectedFishingFish=fish;SelectionRevision++;}return true;
  }
  public bool SelectFishingAt(Vector2 point,bool touch=false){var g=FishingGame.Instance;return g&&SelectFishingFish(g.PickFish(target,cam,point,touch));}
  void UpdateFishingTargetInput(){
   var game=FishingGame.Instance;if(!game||!game.Context){SelectedFishingFish=-1;fishingAimFrozenUntil=0;fishingHookPending=false;}
  }
  void UpdateFishingAim(){
   var g=FishingGame.Instance;var p=g?g.Player(target):null;if(!active||!g||!g.Context||!p.HasValue)return;
   if(p.Value.Busy||pendingFishing==FishingAction.Cast||Time.unscaledTime<fishingAimFrozenUntil&&p.Value.sequence==pendingFishingSequence&&g.State.Match.round==pendingFishingRound)return;
   int fish=p.Value.phase==FishingPhase.Ready?g.PickAimedFish(target,cam,FishingAimScreenPoint,SelectedFishingFish,FishingHUD.Instance?FishingHUD.Instance.AimAcquireDegrees:8,FishingHUD.Instance?FishingHUD.Instance.AimReleaseDegrees:10):-1;
   if(SelectedFishingFish!=fish){SelectedFishingFish=fish;SelectionRevision++;}
  }
  void FishingCameraFocus(ref Vector3 focus){
   var g=FishingGame.Instance;if(!g||!g.Context){fishingCameraBlend=0;fishingPierCameraUntil=0;return;}
   if(g.NearestPier(target)>=0)fishingPierCameraUntil=Time.unscaledTime+.25f;
   fishingCameraBlend=Mathf.MoveTowards(fishingCameraBlend,Time.unscaledTime<fishingPierCameraUntil?1:0,Time.unscaledDeltaTime*3);
   var forward=Quaternion.Euler(0,yaw,0)*Vector3.forward;
   if(mode==1){focus+=(forward*1.25f+Vector3.up*.55f)*fishingCameraBlend;}
   else if(mode==2){var pond=target.transform.position+forward*Mathf.Clamp(6.5f-(pitch-20)*.08f,3.5f,10);pond.y=g.WaterY-.6f;focus=Vector3.Lerp(focus,pond,fishingCameraBlend);}
  }
  void QueueFishing(FishingAction action){var g=FishingGame.Instance;var p=g?g.Player(target):null;if(!g||!p.HasValue)return;pendingFishing=action;pendingFishingPressedAt=g.Now;pendingFishingRound=g.State.Match.round;pendingFishingSequence=p.Value.sequence;pendingFishingFish=action==FishingAction.Cast?SelectedFishingFish:-1;}
  public void ReadyFishing(){var g=FishingGame.Instance;var p=g?g.Player(target):null;if(active&&g&&p.HasValue&&!p.Value.Busy)QueueFishing(p.Value.ready?FishingAction.Unready:FishingAction.Ready);}
  public bool BeginFishing(int source=int.MinValue){
   var g=FishingGame.Instance;var p=g?g.Player(target):null;if(!active||!g||!g.Context||!p.HasValue||fishingHeld||pendingFishing!=FishingAction.None)return false;
   if(p.Value.phase==FishingPhase.Ready){if(FishingCastPending||!g.CanCast(target,SelectedFishingFish)||!g.VisibleTarget(target,cam,SelectedFishingFish))return false;QueueFishing(FishingAction.Cast);fishingAimFrozenUntil=Time.unscaledTime+1.5f;}
   else if(p.Value.phase==FishingPhase.Bite){if(FishingHookPending||g.Now>=p.Value.hookUntil)return false;QueueFishing(FishingAction.Hook);fishingHookPending=true;fishingHookRound=pendingFishingRound;fishingHookSequence=pendingFishingSequence;fishingHookQueuedAt=Time.unscaledTime;}
   else if(p.Value.phase==FishingPhase.Reeling){fishingHeld=true;fishingSource=source;fishingRenewAt=Time.unscaledTime+.2f;QueueFishing(FishingAction.ReelStart);}
   else return false;
   return true;
  }
  public void EndFishing(int source=int.MinValue){if(!fishingHeld||source!=fishingSource)return;fishingHeld=false;QueueFishing(FishingAction.ReelStop);}
  public void CancelFishing(){pendingFishing=FishingAction.None;fishingHookPending=false;var g=FishingGame.Instance;if(g&&g.Busy(target))QueueFishing(FishingAction.Cancel);else if(fishingHeld)QueueFishing(FishingAction.ReelStop);fishingHeld=false;SelectedFishingFish=-1;fishingAimFrozenUntil=0;}
  void AddFishingInput(ref PlayerCommand command){
   var g=FishingGame.Instance;var p=g?g.Player(target):null;
   if(fishingHeld&&(!active||!g||!g.Context||!p.HasValue||p.Value.phase!=FishingPhase.Reeling)){fishingHeld=false;QueueFishing(FishingAction.ReelStop);}
   if(fishingHeld&&pendingFishing==FishingAction.None&&Time.unscaledTime>=fishingRenewAt){fishingRenewAt=Time.unscaledTime+.2f;QueueFishing(FishingAction.ReelStart);}
   if(pendingFishing!=FishingAction.None){command.fishingPressedAt=pendingFishingPressedAt;command.fishingAction=pendingFishing;command.fishingRound=pendingFishingRound;command.fishingSequence=pendingFishingSequence;command.fishingFish=pendingFishingFish;pendingFishing=FishingAction.None;}
  }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  // Uses the normal queue and command reader. No fabricated timestamp or direct
  // pending flag is used, so this covers the actual edge sent to FishingRpc.
  public System.Collections.IEnumerator ReviewFishingHookPending(System.Action<bool,string> check){
   var game=FishingGame.Instance;if(!game||!target){check(false,"HOOK_PENDING_FIXTURE_CONTEXT");yield break;}
   var savedMatch=game.State.Match;var savedPlayers=game.State.Players.ToArray();var savedFish=game.State.Fish.ToArray();
   var position=target.transform.position;var rotation=target.transform.rotation;bool reviewed=game.ReviewSnapshot;uint token=savedMatch.round+100;
   void Bite(){ClearMatchInput();ReadCommand();var fixture=new FishingState();double now=game.Now;fixture.Begin(new[]{FishingGame.Key(target)},++token,now,false);bool cast=fixture.Act(FishingGame.Key(target),token,0,FishingAction.Cast,0,target.transform.position,now-1.95,0,7);fixture.Advance(now,0);check(cast&&fixture.Player(FishingGame.Key(target)).Value.phase==FishingPhase.Bite,"HOOK_PENDING_FIXTURE_ACCEPTED_CAST_AND_CURRENT_BITE_"+token);game.State.Receive(fixture.Match,fixture.Players,fixture.Fish);}
   try {
    game.ReviewSnapshot=true;var pier=game.Pier(0);check(Physics.Raycast(pier+Vector3.up*12,Vector3.down,out var floor,30,1<<8,QueryTriggerInteraction.Ignore),"HOOK_PENDING_FIXTURE_REAL_PIER");
    target.capsule.enabled=false;target.transform.position=floor.point+Vector3.up*.04f;target.ResetLocomotion();target.capsule.enabled=true;Physics.SyncTransforms();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
    Debug.Log("HOOK_PENDING_FIXTURE_CONTEXT now="+game.Now+" active="+active+" context="+game.Context+" pending="+pendingFishing+" modal="+(FishingHUD.Instance&&FishingHUD.Instance.BlocksGameplay));
    // A fresh player can reach this fixture before two simulated seconds.
    // Wait for a real positive cast history; a negative cast precedes respawnAt=0.
    while(game.Now<2)yield return null;
    Bite();check(BeginFishing()&&FishingHookPending&&!BeginFishing(),"HOOK_PENDING_QUEUED_ONCE_DUPLICATE_PRESS_BLOCKED");
    var command=ReadCommand();check(command.fishingAction==FishingAction.Hook&&double.IsFinite(command.fishingPressedAt)&&command.fishingPressedAt>=game.Player(target).Value.biteAt&&command.fishingPressedAt<game.Player(target).Value.hookUntil,"HOOK_QUEUE_CARRIES_ORIGINAL_SERVER_CLOCK_PRESS");
    check(FishingHookPending&&ReadCommand().fishingAction==FishingAction.None&&!BeginFishing(),"SENT_HOOK_RETAINS_BOUNDED_ACK_STATUS_WITHOUT_DUPLICATE_COMMAND");
    check(game.Execute(target,command.fishingAction,command.fishingRound,command.fishingSequence,command.fishingFish,command.fishingPressedAt)&&!FishingHookPending,"HOST_ACKNOWLEDGED_HOOK_CLEARS_PENDING_STATUS");
    Bite();BeginFishing();ReadCommand();CancelFishing();check(!FishingHookPending&&ReadCommand().fishingAction==FishingAction.Cancel,"HOOK_CANCEL_CLEARS_PENDING_AND_QUEUES_REAL_CANCEL");
    Bite();BeginFishing();ReadCommand();SendMessage("OnApplicationFocus",false);check(!FishingHookPending,"HOOK_FOCUS_LOSS_CLEARS_PENDING");ReadCommand();SendMessage("OnApplicationFocus",true);
    Bite();BeginFishing();ReadCommand();SendMessage("OnApplicationPause",true);check(!FishingHookPending,"HOOK_PAUSE_CLEARS_PENDING");ReadCommand();SendMessage("OnApplicationPause",false);
    Bite();BeginFishing();ReadCommand();var changed=game.State.Match;changed.round++;game.State.Receive(changed,game.State.Players.ToArray(),game.State.Fish.ToArray());check(!FishingHookPending,"NEW_ROUND_INVALIDATES_PENDING_HOOK");
    Bite();BeginFishing();ReadCommand();game.State.Advance(game.Player(target).Value.hookUntil,0);check(!FishingHookPending&&!BeginFishing()&&game.Player(target).Value.result==FishingResult.Missed,"MISSED_HOOK_CLEARS_PENDING_AND_BLOCKS_LATE_INPUT");
    Bite();BeginFishing();ReadCommand();yield return new WaitForSecondsRealtime(.52f);check(!FishingHookPending&&game.Now<game.Player(target).Value.hookUntil&&BeginFishing(),"HALF_SECOND_ACK_CAP_ALLOWS_RETRY_INSIDE_ORIGINAL_WINDOW");ReadCommand();
    game.State.Advance(game.Player(target).Value.hookUntil,0);check(!FishingHookPending&&!BeginFishing(),"ACK_RETRY_NEVER_EXTENDS_REAL_HOOK_DEADLINE");
   }finally {
    CancelFishing();ReadCommand();game.State.Receive(savedMatch,savedPlayers,savedFish);game.ReviewSnapshot=reviewed;target.capsule.enabled=false;target.transform.SetPositionAndRotation(position,rotation);target.ResetLocomotion();target.capsule.enabled=true;Physics.SyncTransforms();
   }
  }
#endif
 }
}
