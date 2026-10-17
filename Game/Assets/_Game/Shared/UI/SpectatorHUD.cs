using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class SpectatorHUD:MonoBehaviour {
  Text score;
  void Start(){var plate=CoveUI.Plate(transform,"Spectator scoreboard",500,96,600,58,CoveUI.Cream,20,0,true);score=CoveUI.Text(plate.transform,"",12,0,576,58,25,true,CoveUI.Ink,TextAnchor.MiddleCenter);}
  void Update(){
   if(!score)return;var app=AppRoot.Instance;
   if(app.SelectedSport==SportId.Football&&FootballMatch.Instance){var s=FootballMatch.Instance.Snapshot;score.text=s.phase==FootballMatchPhase.Idle?"Football · Waiting for the match":"BLUE  "+s.scoreA+"  :  "+s.scoreB+"  RED";}
   else if(app.SelectedSport==SportId.Basketball&&BasketballMatch.Instance){var s=BasketballMatch.Instance.Snapshot;score.text=s.phase==BasketballMatchPhase.Idle?"Basketball · Waiting for the match":"BLUE  "+s.scoreA+"  :  "+s.scoreB+"  RED";}
   else score.text=app.SelectedSport+" · Watching your room";
  }
 }
}
