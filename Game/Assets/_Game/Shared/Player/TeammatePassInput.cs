using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 public sealed partial class PlayerView {
  public const float TeammateTapSeconds=.25f,TeammateDoubleTapSeconds=.34f;
  readonly List<RaycastResult> teammateUiHits=new();
  Athlete tapDownTarget,firstTapTarget;int tapPointer=int.MinValue;
  Vector2 tapOrigin;float tapDownAt,firstTapAt,lastTouchAt=-1;
  uint tapPossession;SportId tapSport;bool tapDragged,pendingTargetPass;
  ulong targetPassReceiver;uint targetPassPossession;
  float TapSlop=>Mathf.Clamp(Screen.height*.018f,10,32);
  uint PassPossession=>AppRoot.Instance.SelectedSport==SportId.Basketball?BasketballBall.Active?BasketballBall.Active.Defense.play:0:FootballBall.Instance?FootballBall.Instance.PassPossession:0;
  bool CanTargetPass=>active&&target&&!target.IsSpectator&&!target.inTransit&&!InputBlocked&&AppRoot.Instance&&AppRoot.Instance.Exploring&&!RoomService.Paused&&!TeleportSequence.OwnsCamera&&!FootballReplay.Presenting&&!FootballMVP.Presenting&&!BasketballMVP.Presenting&&!pendingTargetPass&&!ShotCharging&&!PassCharging&&!FootballPassAiming&&!Charging&&!pendingShoot&&!pendingShotBegin&&!pendingPass&&!pendingPassBegin&&!pendingFootballPass&&!pendingKick&&(AppRoot.Instance.SelectedSport==SportId.Basketball?BasketballBall.Active&&BasketballBall.Active.CanPass(target)&&!BasketballMatch.BlocksActions:AppRoot.Instance.SelectedSport==SportId.Football&&FootballBall.Instance&&FootballBall.Instance.CanPass(target)&&!FootballMatch.BlocksActions);
  bool ValidTapReceiver(Athlete receiver)=>AppRoot.Instance.SelectedSport==SportId.Basketball?BasketballBall.Active&&BasketballBall.Active.ValidDirectReceiver(target,receiver):FootballBall.Instance&&FootballBall.Instance.ValidPassReceiver(target,receiver);
  bool PassUiBlocks(Vector2 screen){
   if(!EventSystem.current)return false;
   teammateUiHits.Clear();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=screen},teammateUiHits);
   foreach(var hit in teammateUiHits){var pad=hit.gameObject.GetComponentInParent<TouchPad>();if(pad&&pad.look)continue;return true;}
   return false;
  }
  // Use model bounds, not Physics.Raycast against CharacterControllers: remote
  // characters deliberately disable their colliders on clients.
  public Athlete TeammateAtScreen(Vector2 screen){
   if(!cam||!cam.pixelRect.Contains(screen)||PassUiBlocks(screen))return null;
   var ray=cam.ScreenPointToRay(screen);Athlete nearest=null;float depth=float.PositiveInfinity;
   foreach(var actor in Athlete.Active){
    if(!actor||actor==target||!actor.isActiveAndEnabled||actor.inTransit||actor.IsSpectator||!actor.visual)continue;
    var renderers=actor.visual.GetComponentsInChildren<SkinnedMeshRenderer>();bool found=false;Bounds bounds=default;
    foreach(var renderer in renderers){if(!renderer.enabled||!renderer.gameObject.activeInHierarchy)continue;if(!found){bounds=renderer.bounds;found=true;}else bounds.Encapsulate(renderer.bounds);}
    if(!found)continue;
    bounds.Expand(.12f);
    if(bounds.IntersectRay(ray,out float distance)&&distance>=cam.nearClipPlane&&distance<depth){nearest=actor;depth=distance;}
   }
   if(!nearest||Physics.Raycast(ray,depth,1<<8,QueryTriggerInteraction.Ignore))return null;
   return ValidTapReceiver(nearest)?nearest:null;
  }
  public void TargetPassPointerDown(int pointer,Vector2 screen){
   if(!CanTargetPass){ResetTargetTap();return;}
   // A joystick finger may remain held while another finger taps a teammate.
   if(PassUiBlocks(screen))return;
   if(tapPointer!=int.MinValue){ResetTargetTap();return;}
   if(firstTapTarget&&(Time.unscaledTime-firstTapAt>TeammateDoubleTapSeconds||tapSport!=AppRoot.Instance.SelectedSport||tapPossession!=PassPossession))ResetTargetTap();
   var receiver=TeammateAtScreen(screen);if(!receiver){ResetTargetTap();return;}
   tapPointer=pointer;tapDownTarget=receiver;tapOrigin=screen;tapDownAt=Time.unscaledTime;tapDragged=false;
   tapSport=AppRoot.Instance.SelectedSport;tapPossession=PassPossession;
  }
  public void TargetPassPointerMove(int pointer,Vector2 screen){if(pointer==tapPointer&&(screen-tapOrigin).sqrMagnitude>TapSlop*TapSlop)tapDragged=true;}
  public bool TargetPassPointerUp(int pointer,Vector2 screen){
   if(pointer!=tapPointer)return false;
   TargetPassPointerMove(pointer,screen);var receiver=tapDownTarget;tapPointer=int.MinValue;tapDownTarget=null;
   if(!CanTargetPass||tapDragged||Time.unscaledTime-tapDownAt>TeammateTapSeconds||tapSport!=AppRoot.Instance.SelectedSport||tapPossession!=PassPossession||TeammateAtScreen(screen)!=receiver){ResetTargetTap();return false;}
   if(firstTapTarget==receiver&&Time.unscaledTime-firstTapAt<=TeammateDoubleTapSeconds){ResetTargetTap();return RequestTargetPass(receiver);}
   firstTapTarget=receiver;firstTapAt=Time.unscaledTime;return false;
  }
  public bool RequestTargetPass(Athlete receiver){
   if(!CanTargetPass||!ValidTapReceiver(receiver))return false;
   pendingTargetPass=true;targetPassReceiver=TeammatePassing.Id(receiver);targetPassPossession=PassPossession;return true;
  }
  void ResetTargetTap(){tapPointer=int.MinValue;tapDownTarget=firstTapTarget=null;tapDragged=false;}
  void ClearTargetPass(){ResetTargetTap();pendingTargetPass=false;}
  void AddTargetPassInput(ref PlayerCommand command){command.targetedPass=pendingTargetPass;command.passReceiver=targetPassReceiver;command.passPossession=targetPassPossession;pendingTargetPass=false;}
  void UpdateTargetPassInput(){
   if(!CanTargetPass){ResetTargetTap();return;}
   if(firstTapTarget&&(tapPossession!=PassPossession||tapSport!=AppRoot.Instance.SelectedSport||Time.unscaledTime-firstTapAt>TeammateDoubleTapSeconds))ResetTargetTap();
   if(Input.touchCount>0){
    lastTouchAt=Time.unscaledTime;
    for(int i=0;i<Input.touchCount;i++){var touch=Input.GetTouch(i);switch(touch.phase){case TouchPhase.Began:TargetPassPointerDown(touch.fingerId,touch.position);break;case TouchPhase.Moved:case TouchPhase.Stationary:TargetPassPointerMove(touch.fingerId,touch.position);break;case TouchPhase.Ended:TargetPassPointerUp(touch.fingerId,touch.position);break;case TouchPhase.Canceled:if(tapPointer==touch.fingerId)ResetTargetTap();break;}}
   }else if(Time.unscaledTime-lastTouchAt>.5f){
    if(Input.GetMouseButton(1)){ResetTargetTap();return;}
    if(Input.GetMouseButtonDown(0))TargetPassPointerDown(-1,Input.mousePosition);
    if(Input.GetMouseButton(0))TargetPassPointerMove(-1,Input.mousePosition);
    if(Input.GetMouseButtonUp(0))TargetPassPointerUp(-1,Input.mousePosition);
   }
  }
 }
}
