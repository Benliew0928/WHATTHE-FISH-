using UnityEngine;

namespace WhatTheFish {
 public sealed partial class PlayerView {
  bool aimWanted,aimAccepted;ulong aimOwner;uint aimRound;float aimRequestedAt,pendingGolfHeading;
  GolfShotPreview golfPreview;
  bool chargePresented,golfReleased;float presentedCharge,presentedHeading;
  public bool GolfAimRequested=>aimWanted;
  public ulong AimedGolfOwner=>aimOwner;
  public GolfShotPreview GolfPreview=>golfPreview;
  public GolfShotMode GolfMode=>GolfMatchManager.Instance?GolfMatchManager.Instance.Aim(target).mode:GolfShotMode.Swing;
  public float GolfDisplayedCharge=>GolfCharging&&chargePresented?presentedCharge:GolfCharge;
  public bool GolfAiming {get{var match=GolfMatchManager.Instance;var aim=match?match.Aim(target):default;return aimWanted&&aim.active&&aim.owner==aimOwner&&aim.round==aimRound;}}
  public bool CanAimGolf=>active&&target&&GolfMatchManager.Instance&&GolfMatchManager.Instance.AimCandidate(target);
  public void ToggleGolfAim(){if(aimWanted)CancelGolfAim();else BeginGolfAim();}
  public bool BeginGolfAim(){
   if(aimWanted)return GolfAiming;var match=GolfMatchManager.Instance;var ball=active&&match?match.AimCandidate(target):null;if(!ball)return false;
   aimWanted=true;aimAccepted=false;golfReleased=false;aimOwner=ball.Owner;aimRound=match.Round;aimRequestedAt=Time.unscaledTime;
   pendingJump=false;pendingCart=GolfCartAction.None;
   // Start with the direction the player was already looking. The shot camera
   // works from every roaming camera mode and restores it when aim ends.
   var net=target.GetComponent<NetworkAthlete>();if(net&&net.IsSpawned)net.GolfAimRpc(true,aimOwner,aimRound,yaw);else if(!match.SetAim(target,true,aimOwner,aimRound,yaw)){aimWanted=false;return false;}
   return true;
  }
  public void CancelGolfAim(){
   bool requested=aimWanted;aimWanted=aimAccepted=false;GolfCharging=false;pendingGolf=false;if(golfPreview)golfPreview.Hide();
   if(!requested||!target)return;var match=GolfMatchManager.Instance;var net=target.GetComponent<NetworkAthlete>();
   if(net&&net.IsSpawned)net.GolfAimRpc(false,aimOwner,aimRound,yaw);else if(match)match.SetAim(target,false,aimOwner,aimRound);
  }
  void UpdateGolfAim(){
   if(!aimWanted){if(golfPreview)golfPreview.Hide();return;}
   var match=GolfMatchManager.Instance;
   if(!active||!target||!match||!match.Context||!match.State.Running||match.Round!=aimRound||target.inTransit||GolfCartWorld.Driving(target)){CancelGolfAim();return;}
   if(GolfAiming)aimAccepted=true;else if(aimAccepted||Time.unscaledTime-aimRequestedAt>1.5f)CancelGolfAim();
  }
  void UpdateGolfPreview(){
   var match=GolfMatchManager.Instance;var ball=GolfAiming&&match?match.Ball(aimOwner):null;
   if(!ball||golfReleased){if(golfPreview)golfPreview.Hide();return;}
   if(!golfPreview){var go=new GameObject("Golf shot guide");golfPreview=go.AddComponent<GolfShotPreview>();}
   var aim=match.Aim(target);float charge=GolfCharging?GolfCharge:.3f;
   golfPreview.Show(ball,match.Course,aim.ballPosition,yaw,charge,aim.mode,cam);
   if(GolfCharging){presentedCharge=charge;presentedHeading=yaw;chargePresented=true;}
  }
 }
}
