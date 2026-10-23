using UnityEngine;

namespace WhatTheFish {
 public sealed partial class PlayerView {
  FishingAction pendingFishing;double pendingFishingPressedAt;uint pendingFishingRound,pendingFishingSequence;int pendingFishingFish;
  bool fishingHookPending;uint fishingHookRound,fishingHookSequence;float fishingHookQueuedAt;
  bool fishingHeld;int fishingSource,fishingSide;float fishingRenewAt;
  float fishingAimFrozenUntil,fishingCameraBlend,fishingPierCameraUntil;bool fishingCameraEntered,fishingCameraRestore;int fishingPreviousMode;
  bool fishingFightCamera,fishingFightReturning;Athlete fishingCameraActor;uint fishingCameraRound;
  float fishingFightAt,fishingFightYaw,fishingFightSide;Vector3 fishingFightFrom,fishingFightFocus;Quaternion fishingFightRotation;
  public float FishingFightSide=>fishingFightSide;
  public bool FishingFightCameraActive=>fishingFightCamera||fishingFightReturning;
  bool FishingFightLocksLook {get{var g=FishingGame.Instance;var p=g?g.Player(target):null;var ui=FishingHUD.Instance;return mode==1&&ui&&ui.Settings.fightCamera&&!MenuPreferences.ReducedMotion&&g&&g.Context&&p.HasValue&&p.Value.phase==FishingPhase.Reeling;}}
  void ResetFishingFightCamera(){fishingFightCamera=fishingFightReturning=false;fishingCameraActor=null;fishingFightSide=0;}
  void PresentFishingFightCamera(ref Vector3 position,ref Quaternion rotation){
   var g=FishingGame.Instance;var ui=FishingHUD.Instance;
   if(mode!=1||!g||!g.Context||!target||target.inTransit||!ui||!ui.Settings.fightCamera||MenuPreferences.ReducedMotion){ResetFishingFightCamera();return;}
   if(FishingFightCameraActive&&(fishingCameraActor!=target||fishingCameraRound!=g.State.Match.round))ResetFishingFightCamera();
   var p=g.Player(target);bool fight=!InputBlocked&&!ui.BlocksGameplay&&g.State.Running&&p.HasValue&&p.Value.connected&&p.Value.phase==FishingPhase.Reeling&&p.Value.fish>=0&&p.Value.fish<FishingState.FishCount;
   float duration=ui.Settings.fightCameraTransition;
   if(fight){
    var fish=g.Presentation.HookPoint(p.Value);if(!FiniteCameraPoint(fish)){ResetFishingFightCamera();return;}
    var focus=Vector3.Lerp(target.transform.position+Vector3.up*1.3f,fish,.43f)+Vector3.up*.5f;
    if(!fishingFightCamera){
     fishingFightCamera=true;fishingFightReturning=false;fishingCameraActor=target;fishingCameraRound=g.State.Match.round;fishingFightAt=Time.unscaledTime;
     fishingFightFrom=transform.position;fishingFightRotation=transform.rotation;fishingFightFocus=focus;
     var direction=Vector3.ProjectOnPlane(fish-target.transform.position,Vector3.up);fishingFightYaw=direction.sqrMagnitude>.01f?Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg:yaw;
    }
    // Only the occasional directional sweep moves the shoulder. Keep the
    // fish's screen-left/right consistent with the opposing rod control.
    fishingFightFocus=Vector3.Lerp(fishingFightFocus,focus,1-Mathf.Exp(-5*Time.unscaledDeltaTime));
    float side=FishingState.DirectionalBeat(p.Value,g.Now)?FishingState.CounterSide(p.Value,g.Now):0;
    fishingFightSide=Mathf.Lerp(fishingFightSide,side,1-Mathf.Exp(-3.5f*Time.unscaledDeltaTime));
    var basis=Quaternion.Euler(0,fishingFightYaw+fishingFightSide*7,0);float span=Vector3.ProjectOnPlane(fish-target.transform.position,Vector3.up).magnitude;
    var desired=fishingFightFocus-basis*Vector3.forward*Mathf.Max(ui.Settings.fightCameraDistance,span*.9f)+Vector3.up*(ui.Settings.fightCameraHeight-Mathf.Abs(fishingFightSide)*.35f)+basis*Vector3.right*ui.Settings.fightCameraShoulder*(.35f+fishingFightSide*1.45f);
    var path=desired-fishingFightFocus;
    if(Physics.SphereCast(fishingFightFocus,.2f,path.normalized,out var hit,path.magnitude,1<<8,QueryTriggerInteraction.Ignore))desired=fishingFightFocus+path.normalized*Mathf.Max(.4f,hit.distance-.15f);
    float blend=GolfCameraEase((Time.unscaledTime-fishingFightAt)/duration);
    position=Vector3.Lerp(fishingFightFrom,desired,blend);rotation=Quaternion.Slerp(fishingFightRotation,Quaternion.LookRotation(fishingFightFocus-desired),blend);return;
   }
   if(fishingFightCamera){fishingFightCamera=false;fishingFightReturning=true;fishingFightAt=Time.unscaledTime;fishingFightFrom=transform.position;fishingFightRotation=transform.rotation;}
   if(fishingFightReturning){float blend=GolfCameraEase((Time.unscaledTime-fishingFightAt)/duration);position=Vector3.Lerp(fishingFightFrom,position,blend);rotation=Quaternion.Slerp(fishingFightRotation,rotation,blend);if(blend>=1)ResetFishingFightCamera();}
  }
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
   if(SelectedFishingFish!=fish){SelectedFishingFish=fish;SelectionRevision++;GolfFishingAudio.PlayMenu("ui-select");}return true;
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
   if(mode==1||mode==2)focus+=(forward*1.25f+Vector3.up*.55f)*fishingCameraBlend;
  }
  void QueueFishing(FishingAction action){var g=FishingGame.Instance;var p=g?g.Player(target):null;if(!g||!p.HasValue)return;pendingFishing=action;pendingFishingPressedAt=g.Now;pendingFishingRound=g.State.Match.round;pendingFishingSequence=p.Value.sequence;pendingFishingFish=action==FishingAction.Cast?SelectedFishingFish:-1;}
  // Modal skill choices and skill taps use the same owner-checked RPC directly.
  // A held directional input cannot discard a skill tap or lose its renewal.
  public bool FishingAbilityInput(FishingAction action,int choice=-1){
   if(!TutorialSession.Allows(action==FishingAction.ChooseSkill?"skills":"ability"))return false;
   var g=FishingGame.Instance;var p=g?g.Player(target):null;
   if(!active||InputBlocked||!g||!g.Context||!target||target.inTransit||!p.HasValue||action!=FishingAction.ChooseSkill&&action!=FishingAction.UseAbility)return false;
   var net=target.GetComponent<NetworkAthlete>();
   if(net&&net.IsSpawned){if(!net.IsOwner)return false;if(!net.IsServer){net.FishingRpc(action,g.State.Match.round,p.Value.sequence,choice,g.Now);return true;}}
   return g.Execute(target,action,g.State.Match.round,p.Value.sequence,choice,g.Now);
  }
  void ObserveFishingArrival(){
   var g=FishingGame.Instance;bool on=g&&g.Context&&!RoomMatchStart.Travelling;
   if(!on){if(fishingCameraEntered){fishingCameraEntered=false;fishingCameraRestore=true;}if(fishingCameraRestore&&!RoomMatchStart.Travelling&&target&&!target.inTransit){mode=fishingPreviousMode;fishingCameraRestore=false;}return;}
   if(fishingCameraEntered||!active||!target||target.inTransit||TeleportSequence.OwnsCamera)return;
   fishingCameraEntered=true;fishingCameraRestore=false;fishingPreviousMode=mode;mode=1;fishingCameraBlend=0;ResetFishingFightCamera();
  }
  public void ReadyFishing(){var g=FishingGame.Instance;var p=g?g.Player(target):null;if(!InputBlocked&&active&&g&&p.HasValue&&!p.Value.Busy)QueueFishing(p.Value.ready?FishingAction.Unready:FishingAction.Ready);}
  public bool BeginFishing(int source=int.MinValue){
   var g=FishingGame.Instance;var p=g?g.Player(target):null;if(InputBlocked||!active||!g||!g.Context||!p.HasValue||fishingHeld||pendingFishing!=FishingAction.None)return false;
   if(p.Value.phase==FishingPhase.Ready){if(FishingCastPending||!g.CanCast(target,SelectedFishingFish)||!g.VisibleTarget(target,cam,SelectedFishingFish)){GolfFishingAudio.PlayMenu("ui-error");return false;}QueueFishing(FishingAction.Cast);fishingAimFrozenUntil=Time.unscaledTime+1.5f;}
   else if(p.Value.phase==FishingPhase.Bite){if(FishingHookPending||g.Now>=p.Value.hookUntil)return false;QueueFishing(FishingAction.Hook);fishingHookPending=true;fishingHookRound=pendingFishingRound;fishingHookSequence=pendingFishingSequence;fishingHookQueuedAt=Time.unscaledTime;}
   else if(p.Value.phase==FishingPhase.Reeling)return BeginCountersteer(1,source);
   else return false;
   return true;
  }
  public bool BeginCountersteer(int side,int source=int.MinValue){
   var g=FishingGame.Instance;var p=g?g.Player(target):null;
   if(InputBlocked||side!= -1&&side!=1||!active||!g||!g.Context||!p.HasValue||p.Value.phase!=FishingPhase.Reeling||pendingFishing!=FishingAction.None&&pendingFishing!=FishingAction.ReelStop||fishingHeld&&fishingSource==source)return false;
   fishingHeld=true;fishingSide=side;fishingSource=source;fishingRenewAt=Time.unscaledTime+.2f;
   QueueFishing(side<0?FishingAction.ReelLeft:FishingAction.ReelRight);return true;
  }
  void UpdateCountersteerKeyboard(){
   var g=FishingGame.Instance;var p=g?g.Player(target):null;if(!g||!g.Context||!p.HasValue||p.Value.phase!=FishingPhase.Reeling)return;
   if(Input.GetKeyUp(KeyCode.A)||Input.GetKeyUp(KeyCode.LeftArrow))EndFishing(-101);
   if(Input.GetKeyUp(KeyCode.D)||Input.GetKeyUp(KeyCode.RightArrow))EndFishing(-102);
   if(!fishingHeld&&pendingFishing==FishingAction.None){
    bool left=Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow),right=Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow);
    if(left!=right)BeginCountersteer(left?-1:1,left?-101:-102);
   }
  }
  public void EndFishing(int source=int.MinValue){if(!fishingHeld||source!=fishingSource)return;fishingHeld=false;QueueFishing(FishingAction.ReelStop);}
  public void CancelFishing(){pendingFishing=FishingAction.None;fishingHookPending=false;var g=FishingGame.Instance;if(g&&g.Busy(target))QueueFishing(FishingAction.Cancel);else if(fishingHeld)QueueFishing(FishingAction.ReelStop);fishingHeld=false;SelectedFishingFish=-1;fishingAimFrozenUntil=0;}
  void AddFishingInput(ref PlayerCommand command){
   var g=FishingGame.Instance;var p=g?g.Player(target):null;
   if(fishingHeld&&(!active||!g||!g.Context||!p.HasValue||p.Value.phase!=FishingPhase.Reeling)){fishingHeld=false;QueueFishing(FishingAction.ReelStop);}
   if(fishingHeld&&pendingFishing==FishingAction.None&&Time.unscaledTime>=fishingRenewAt){fishingRenewAt=Time.unscaledTime+.2f;QueueFishing(fishingSide<0?FishingAction.ReelLeft:FishingAction.ReelRight);}
   if(g&&g.Context&&g.State.Preparing){command.move=Vector2.zero;command.jump=command.sprint=false;}
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
