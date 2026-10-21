using UnityEngine;
namespace WhatTheFish {
 public sealed partial class PlayerView {
  bool pendingFootballPass;int footballPassSource;
  public bool FootballPassAiming {get;private set;}
  public Athlete FootballPassTarget=>FootballPassAiming&&FootballBall.Instance?FootballBall.Instance.SelectPassReceiver(target,yaw):null;
  public bool BeginFootballPass(int source=int.MinValue){if(InputBlocked||pendingTargetPass||pendingFootballPass||pendingKick||FootballPassAiming||Charging||!active||!FootballBall.Instance||!FootballBall.Instance.CanPass(target))return false;footballPassSource=source;FootballPassAiming=true;return true;}
  public void EndFootballPass(int source=int.MinValue){if(!FootballPassAiming||source!=footballPassSource)return;FootballPassAiming=false;if(!InputBlocked&&FootballBall.Instance&&FootballBall.Instance.CanPass(target))pendingFootballPass=true;}
  public void CancelFootballPass(){FootballPassAiming=false;}
 }
}
