using UnityEngine;

namespace WhatTheFish {
 public sealed partial class PlayerView {
  FishingAction pendingFishing;uint pendingFishingRound,pendingFishingSequence;int pendingFishingFish;
  bool fishingHeld;int fishingSource;float fishingRenewAt;
  float fishingAimFrozenUntil,fishingCameraBlend,fishingPierCameraUntil;
  public bool FishingHeld=>fishingHeld;
  public Camera FishingCamera=>cam;
  public Vector2 FishingAimScreenPoint=>FishingHUD.Instance?FishingHUD.Instance.AimScreenPoint:cam.pixelRect.center;
  public bool FishingCastPending {get{var g=FishingGame.Instance;var p=g?g.Player(target):null;return p.HasValue&&p.Value.phase==FishingPhase.Ready&&g.State.Match.round==pendingFishingRound&&p.Value.sequence==pendingFishingSequence&&(pendingFishing==FishingAction.Cast||Time.unscaledTime<fishingAimFrozenUntil);}}
  public int SelectedFishingFish {get;private set;}=-1;
  public uint SelectionRevision {get;private set;}
  public bool SelectFishingFish(int fish){
   var g=FishingGame.Instance;var p=g?g.Player(target):null;
   if(!active||!g||!g.Context||!p.HasValue||p.Value.phase!=FishingPhase.Ready||!g.Available(target,fish))return false;
   if(SelectedFishingFish!=fish){SelectedFishingFish=fish;SelectionRevision++;}return true;
  }
  public bool SelectFishingAt(Vector2 point,bool touch=false){var g=FishingGame.Instance;return g&&SelectFishingFish(g.PickFish(target,cam,point,touch));}
  void UpdateFishingTargetInput(){
   var game=FishingGame.Instance;if(!game||!game.Context){SelectedFishingFish=-1;fishingAimFrozenUntil=0;}
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
  void QueueFishing(FishingAction action){var g=FishingGame.Instance;var p=g?g.Player(target):null;if(!g||!p.HasValue)return;pendingFishing=action;pendingFishingRound=g.State.Match.round;pendingFishingSequence=p.Value.sequence;pendingFishingFish=action==FishingAction.Cast?SelectedFishingFish:-1;}
  public void ReadyFishing(){var g=FishingGame.Instance;var p=g?g.Player(target):null;if(active&&g&&p.HasValue&&!p.Value.Busy)QueueFishing(p.Value.ready?FishingAction.Unready:FishingAction.Ready);}
  public bool BeginFishing(int source=int.MinValue){
   var g=FishingGame.Instance;var p=g?g.Player(target):null;if(!active||!g||!g.Context||!p.HasValue||fishingHeld||pendingFishing!=FishingAction.None)return false;
   if(p.Value.phase==FishingPhase.Ready){if(FishingCastPending||!g.CanCast(target,SelectedFishingFish)||!g.VisibleTarget(target,cam,SelectedFishingFish))return false;QueueFishing(FishingAction.Cast);fishingAimFrozenUntil=Time.unscaledTime+1.5f;}
   else if(p.Value.phase==FishingPhase.Bite){if(g.Now>=p.Value.hookUntil)return false;QueueFishing(FishingAction.Hook);}
   else if(p.Value.phase==FishingPhase.Reeling){fishingHeld=true;fishingSource=source;fishingRenewAt=Time.unscaledTime+.2f;QueueFishing(FishingAction.ReelStart);}
   else return false;
   return true;
  }
  public void EndFishing(int source=int.MinValue){if(!fishingHeld||source!=fishingSource)return;fishingHeld=false;QueueFishing(FishingAction.ReelStop);}
  public void CancelFishing(){pendingFishing=FishingAction.None;var g=FishingGame.Instance;if(g&&g.Busy(target))QueueFishing(FishingAction.Cancel);else if(fishingHeld)QueueFishing(FishingAction.ReelStop);fishingHeld=false;SelectedFishingFish=-1;fishingAimFrozenUntil=0;}
  void AddFishingInput(ref PlayerCommand command){
   var g=FishingGame.Instance;var p=g?g.Player(target):null;
   if(fishingHeld&&(!active||!g||!g.Context||!p.HasValue||p.Value.phase!=FishingPhase.Reeling)){fishingHeld=false;QueueFishing(FishingAction.ReelStop);}
   if(fishingHeld&&pendingFishing==FishingAction.None&&Time.unscaledTime>=fishingRenewAt){fishingRenewAt=Time.unscaledTime+.2f;QueueFishing(FishingAction.ReelStart);}
   if(pendingFishing!=FishingAction.None){command.fishingAction=pendingFishing;command.fishingRound=pendingFishingRound;command.fishingSequence=pendingFishingSequence;command.fishingFish=pendingFishingFish;pendingFishing=FishingAction.None;}
  }
 }
}
