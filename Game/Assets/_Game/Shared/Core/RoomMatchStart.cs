using System;
using System.Linq;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Validate the entire relocation before a controller commits its new round.
 public static class RoomMatchStart {
  public static bool Travelling=>RoomService.Paused||RoomTeleport.Busy||TeleportSequence.Busy||SkySailWorld.Instance&&SkySailWorld.Instance.Travelling;
  public static string Message {get;private set;}="";
  public static float MessageUntil {get;private set;}
  public sealed class Placement {
   internal Athlete[] actors=Array.Empty<Athlete>();internal TeleportLanding[] landings=Array.Empty<TeleportLanding>();
   public void Apply(){
    var enabled=actors.Select(a=>a.capsule.enabled).ToArray();foreach(var actor in actors)actor.capsule.enabled=false;
    for(int i=0;i<actors.Length;i++){
     var actor=actors[i];var pose=landings[i];actor.transform.SetPositionAndRotation(pose.position,pose.rotation);actor.ResetLocomotion();
     var net=actor.GetComponent<NetworkAthlete>();if(net&&net.IsSpawned){net.ClearMatchInput();actor.GetComponent<NetworkTransform>().Teleport(pose.position,pose.rotation,actor.transform.localScale);net.MatchArrivalRpc(pose.rotation.eulerAngles.y);}
     else if(AppRoot.Instance.LocalAthlete==actor){AppRoot.Instance.view.ClearMatchInput();AppRoot.Instance.view.yaw=pose.rotation.eulerAngles.y;}
    }
    Physics.SyncTransforms();for(int i=0;i<actors.Length;i++)actors[i].capsule.enabled=enabled[i];
   }
  }
  public static bool Prepare(SportId sport,out Placement plan){
   plan=new Placement();Message="";var app=AppRoot.Instance;
   if(!app||!app.Exploring||app.SelectedSport!=sport||Travelling||app.PendingSelection.HasValue||app.environments.GetComponent<SkySailStreaming>().Busy)return Reject("Wait for everyone to arrive before starting.");
   if(app.rooms.Connected&&!app.rooms.Host)return Reject("Your host starts the match.");
   var people=app.rooms.Connected?RoomTeleport.Players().Select(n=>n.GetComponent<Athlete>()).ToArray():IslandBots.Instance&&IslandBots.Instance.Solo?IslandBots.Instance.Roster:new[]{app.LocalAthlete};
   var actors=people.Where(a=>a&&(a.IsSpectator||sport==SportId.Fishing)).ToArray();
   if(actors.Any(a=>a.inTransit||!a.isActiveAndEnabled))return Reject("Wait for everyone to arrive before starting.");
   if(actors.Length==0)return true;
   if(!TeleportDestinations.Resolve(sport,TeleportDestination.Game,actors,Vector3.zero,out var landings,out var reason)){Debug.LogWarning("Match arrival rejected: "+reason);return Reject(sport==SportId.Fishing?"A pier or viewing position is blocked. Clear the arrival area and try again.":"A viewing position is blocked. Clear the arrival area and try again.");}
   plan.actors=actors;plan.landings=landings;return true;
  }
  static bool Reject(string reason){Message=reason;MessageUntil=Time.unscaledTime+8;return false;}
  public static bool CanStart(SportId sport)=>sport switch{
   SportId.Football=>FootballMatch.Instance&&FootballMatch.Instance.CanStart,
   SportId.Basketball=>BasketballMatch.Instance&&BasketballMatch.Instance.CanStart,
   SportId.Golf=>GolfMatchManager.Instance&&GolfMatchManager.Instance.CanStart,
   _=>FishingGame.Instance&&FishingGame.Instance.CanStart(FishingMode.Round)
  };
  public static string Hint(SportId sport){
   if(Travelling)return "Wait for everyone to arrive.";
   if(PostMatchFlow.MatchActive)return "Match in progress";
   if(CanStart(sport))return "Spectators move to the viewing area when the match starts.";
   return sport switch{SportId.Football=>"Needs even teams: 2–10 players",SportId.Basketball=>"Needs even teams: 4–10 players",SportId.Fishing=>"Needs 1–5 players on the island",_=>"Waiting for a playing participant"};
  }
 }
 public sealed class MatchStartNotice:MonoBehaviour {
  Text label;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){new GameObject("Match start notice",typeof(Canvas),typeof(CanvasScaler)).AddComponent<MatchStartNotice>();}
  void Awake(){var canvas=GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=1050;var scale=GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1600,900);scale.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;var plate=CoveUI.Plate(transform,"Start status",0,0,1000,100,WorldUI.Surface,20,1.5f,false);plate.rectTransform.anchorMin=plate.rectTransform.anchorMax=new Vector2(.5f,1);plate.rectTransform.pivot=new Vector2(.5f,1);plate.rectTransform.anchoredPosition=new Vector2(0,-140);label=CoveUI.Text(plate.transform,"",24,10,952,80,22,true,WorldUI.Ink,TextAnchor.MiddleCenter);}
  void Update(){label.transform.parent.gameObject.SetActive(AppRoot.Instance&&AppRoot.Instance.Exploring&&Time.unscaledTime<RoomMatchStart.MessageUntil&&RoomMatchStart.Message.Length>0);label.text=RoomMatchStart.Message;}
 }
}
