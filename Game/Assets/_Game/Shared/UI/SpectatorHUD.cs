using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class SpectatorHUD:MonoBehaviour {
  Text score,startHint;public Button startButton;
  void Start(){var plate=CoveUI.Plate(transform,"Spectator scoreboard",500,120,600,58,CoveUI.Cream,20,0,true);score=CoveUI.Text(plate.transform,"",12,0,576,58,25,true,CoveUI.Ink,TextAnchor.MiddleCenter);if(startButton){var note=CoveUI.Plate(transform,"Host match guidance",450,190,700,48,CoveUI.Cream,14,0,true);startHint=CoveUI.Text(note.transform,"",12,0,676,48,19,true,CoveUI.Ink,TextAnchor.MiddleCenter);}}
  void Update(){
   if(!score)return;var app=AppRoot.Instance;
   if(startButton){startButton.interactable=RoomMatchStart.CanStart(app.SelectedSport);startHint.text=RoomMatchStart.Hint(app.SelectedSport);}
   if(app.SelectedSport==SportId.Football&&FootballMatch.Instance){var s=FootballMatch.Instance.Snapshot;score.text=s.phase==FootballMatchPhase.Idle?"Football · Waiting for the match":"BLUE  "+s.scoreA+"  :  "+s.scoreB+"  RED";}
   else if(app.SelectedSport==SportId.Basketball&&BasketballMatch.Instance){var s=BasketballMatch.Instance.Snapshot;score.text=s.phase==BasketballMatchPhase.Idle?"Basketball · Waiting for the match":"BLUE  "+s.scoreA+"  :  "+s.scoreB+"  RED";}
   else score.text=app.SelectedSport+" · Watching your room";
  }
 }
}
