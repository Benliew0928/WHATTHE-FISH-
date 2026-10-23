using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public static class RoomFeedback {
  public static string Explain(string error){
   if(string.IsNullOrEmpty(error))return "";string s=error.ToLowerInvariant();
   if(s.StartsWith("wait until")||s.StartsWith("the crew changed")||s.StartsWith("choose at least one player place"))return error;
   if(s.Contains("no open places")||s.Contains("room is full")||s.Contains("session is full"))return "This room has no open places. Ask the host to make space for a player or spectator.";
   if(s.Contains("protocol")||s.Contains("version mismatch")||s.Contains("networkconfig"))return "Your game build differs from the host's. Install the same build, then join again.";
   if(s.Contains("not found")||s.Contains("invalid join code")||s.Contains("invalid room code")||s.Contains("no longer available"))return "That room is unavailable. Check the invitation with your host and try again.";
   if(s.Contains("locked")||s.Contains("exploration has started"))return "The crew is already playing. Ask the host to return everyone to the waiting room, then join.";
   if(s.Contains("destination could not")||s.Contains("took too long to prepare")||s.Contains("host cancelled"))return "The journey stopped. Everyone is back in the waiting room. Ready up and try departing again.";
   if(s.Contains("reconnect"))return "The room could not reconnect. Check your connection, then ask your crew for the current room code.";
   if(s.Contains("same game build"))return "Check your connection and use the same game build as the host, then try again.";
   return "The connection could not finish. Check your internet connection and try again. Your room code is kept.";
  }
 }
 public sealed class RoomConnectionNotice:MonoBehaviour {
  RectTransform busyCard,errorCard;Text operation,error;Button cancel;float began;bool wasBusy;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){new GameObject("Room connection feedback",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).AddComponent<RoomConnectionNotice>();}
  void Awake(){WorldUI.Canvas(gameObject,1150);
   busyCard=CoveUI.Rect(transform,"Room operation",0,0,740,228);PlayerHubStyle.Card(busyCard,"Operation surface",0,0,740,228);
   PlayerHubStyle.Label(busyCard,"GETTING YOUR CREW TOGETHER",26,20,688,30,21,true,PlayerHubStyle.Cyan);
   operation=PlayerHubStyle.Label(busyCard,"",26,65,688,65,27,true);
   cancel=PlayerHubStyle.Action(busyCard,"Cancel room connection","Cancel",514,152,200,50,PlayerHubStyle.Raised,()=>AppRoot.Instance.rooms.CancelConnection(),size:22);
   PlayerHubStyle.Label(busyCard,"Your room code is kept if you cancel.",26,156,470,45,21,false,PlayerHubStyle.Muted);
   errorCard=CoveUI.Rect(transform,"Room error",0,0,1120,120);PlayerHubStyle.Card(errorCard,"Error surface",0,0,1120,120);
   error=PlayerHubStyle.Label(errorCard,"",24,12,848,96,23);
   PlayerHubStyle.Action(errorCard,"Dismiss room error","Dismiss",900,33,196,54,PlayerHubStyle.Raised,()=>AppRoot.Instance.rooms.DismissError(),size:22);
   busyCard.gameObject.SetActive(false);errorCard.gameObject.SetActive(false);
  }
  void Update(){var app=AppRoot.Instance;if(!app||!app.rooms)return;var room=app.rooms;bool busy=room.busy&&room.Operation.Length>0&&!room.Recovering;
   if(busy&&!wasBusy)began=Time.unscaledTime;wasBusy=busy;busyCard.gameObject.SetActive(busy&&!StartupWelcome.Blocking);
   if(busy){operation.text=room.Operation+(Time.unscaledTime-began>15?"\nThis is taking longer than usual.":"");cancel.interactable=room.CanCancelConnection;WorldUI.Fit(busyCard,(RectTransform)transform,740,228);}
   bool show=!busy&&!app.Exploring&&!StartupWelcome.Blocking&&!AccountPanel.Visible&&!SocialPanel.Visible&&!string.IsNullOrEmpty(room.Error);
   errorCard.gameObject.SetActive(show);if(show){error.text=RoomFeedback.Explain(room.Error);WorldUI.Fit(errorCard,(RectTransform)transform,1120,120,true,30);}
  }
 }
}
