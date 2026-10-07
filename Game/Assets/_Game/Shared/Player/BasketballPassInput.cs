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
   if(PassCharging||ShotCharging||pendingShoot||pendingShotBegin||pendingPass||!active||!target||!ball||!ball.CanShoot(target))return false;
   PassBend=0;PassCharging=true;passSource=source;passPlay=ball.Defense.play;passStarted=BasketballMotion.Clock;pendingPassBegin=true;return true;
  }
  public void EndPass(int source=int.MinValue){
   if(!PassCharging||source!=passSource)return;
   PassCharging=false;pendingPass=true;passReleasedAt=BasketballBall.Active?BasketballBall.Active.PassClock:BasketballMotion.Clock;
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
