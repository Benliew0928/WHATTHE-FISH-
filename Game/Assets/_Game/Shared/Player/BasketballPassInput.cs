using UnityEngine;

namespace WhatTheFish {
 public sealed partial class PlayerView {
  bool pendingPassBegin,pendingPassCancel;int passSource;uint passPlay;double passStarted,passReleasedAt;
  public bool PassCharging {get;private set;}
  public float PassBend {get;private set;}
  public void SelectPass(float bend,int source=int.MinValue){if(PassCharging&&source==passSource&&float.IsFinite(bend))PassBend=Mathf.Clamp(bend,-1,1);}
  public float PassPower=>!PassCharging?0:BasketballBall.Active&&BasketballBall.Active.IsPassAiming(target)?BasketballBall.Active.PassPower:BasketballPassRules.Power(BasketballMotion.Clock-passStarted);
  public bool BeginPass(int source=int.MinValue){
   var ball=BasketballBall.Active;
   if(InputBlocked||pendingTargetPass||PassCharging||ShotCharging||pendingShoot||pendingShotBegin||pendingPass||!active||!target||!ball||!ball.CanPass(target))return false;
   var receiver=ball.SelectPassReceiver(target,yaw);
   if(receiver)return RequestTargetPass(receiver);
   // Practice without a teammate still permits an immediate forward pass.
   PassBend=0;passSource=source;passPlay=ball.Defense.play;passStarted=BasketballMotion.Clock;pendingPassBegin=true;pendingPass=true;passReleasedAt=ball.PassClock;return true;
  }
  public void EndPass(int source=int.MinValue){
   // Single-tap passes commit on the down edge; lifting never fires again.
  }
  public void CancelPass(int source=int.MinValue){
   if(!PassCharging||source!=int.MinValue&&source!=passSource)return;
   PassCharging=false;pendingPassCancel=true;
  }
  void AddPassInput(ref PlayerCommand command){
   command.passBend=PassBend;command.passBegin|=pendingPassBegin;command.passCancel|=pendingPassCancel;command.passPlay=passPlay;command.passReleasedAt=passReleasedAt;
   pendingPassBegin=pendingPassCancel=false;
  }
 }
}
