using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class SpectatorHUD:MonoBehaviour {
  Text score,startHint;RectTransform strip;SportsScoreboard sportsScore;public Button startButton;
  void Start(){
   strip=CoveUI.Rect(transform,"Spectator scoreboard",0,0,640,96);strip.anchorMin=strip.anchorMax=strip.pivot=new Vector2(.5f,1);strip.anchoredPosition=new Vector2(0,-122);
   var sport=AppRoot.Instance.SelectedSport;bool teams=sport==SportId.Basketball||sport==SportId.Football;
   if(teams){sportsScore=SportsScoreboard.Create(strip,sport);sportsScore.transform.localScale=Vector3.one*2;((RectTransform)sportsScore.transform).anchoredPosition=new Vector2(16,0);}
   else WorldUI.Card(strip,"Spectator surface",0,0,640,96);
   score=WorldUI.Label(strip,"",18,8,604,36,27,true,null,TextAnchor.MiddleCenter);score.gameObject.SetActive(!teams);
   startHint=WorldUI.Label(strip,"",18,teams?(sport==SportId.Basketball?168:124):50,604,32,20,false,WorldUI.Muted,TextAnchor.MiddleCenter);
  }
  void Update(){if(!score)return;var app=AppRoot.Instance;strip.localScale=Vector3.one*Mathf.Min(1,(((RectTransform)transform).rect.width-40)/640);
   strip.gameObject.SetActive(!RoomMatchStart.Travelling&&!(PostMatchFlow.Instance&&PostMatchFlow.Instance.Visible));
   if(startButton){startButton.interactable=RoomMatchStart.CanStart(app.SelectedSport);startButton.gameObject.SetActive(!PostMatchFlow.MatchActive&&!RoomMatchStart.Travelling);}
   bool active=PostMatchFlow.MatchActive;startHint.text=active?"Spectating · walk around and follow the action":app.rooms.Host?RoomMatchStart.Hint(app.SelectedSport):"Waiting for "+WorldUI.HostName+" · you are spectating";
   if(sportsScore){startHint.gameObject.SetActive(!sportsScore.DetailsExpanded);startHint.rectTransform.anchoredPosition=new Vector2(18,-((app.SelectedSport==SportId.Basketball?196:152)+(sportsScore.Animating?104:0)));}
   score.text=app.SelectedSport+" · "+(active?"Match in progress":"Between matches");
   if(app.SelectedSport==SportId.Football&&FootballMatch.Instance&&active){var s=FootballMatch.Instance.Snapshot;score.text="BLUE  "+s.scoreA+"  :  "+s.scoreB+"  RED";}
   else if(app.SelectedSport==SportId.Basketball&&BasketballMatch.Instance&&active){var s=BasketballMatch.Instance.Snapshot;score.text="BLUE  "+s.scoreA+"  :  "+s.scoreB+"  RED";}
  }
 }
}
