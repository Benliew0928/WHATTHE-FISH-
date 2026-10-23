using UnityEngine;
namespace WhatTheFish {
 public sealed partial class PlayerView {
  bool pendingFootballPass;int footballPassSource;
  public bool FootballPassAiming {get;private set;}
  public Athlete FootballPassTarget=>FootballBall.Instance&&FootballBall.Instance.CanPass(target)?FootballBall.Instance.SelectPassReceiver(target,yaw):null;
  public bool BeginFootballPass(int source=int.MinValue){if(InputBlocked||pendingTargetPass||pendingFootballPass||pendingKick||Charging||!active||!FootballBall.Instance||!FootballBall.Instance.CanPass(target))return false;var receiver=FootballPassTarget;if(receiver)return RequestTargetPass(receiver);pendingFootballPass=true;return true;}
  public void EndFootballPass(int source=int.MinValue){}
  public void CancelFootballPass(){FootballPassAiming=false;}
 }
}
