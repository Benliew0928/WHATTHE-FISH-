using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public sealed class BasketballBots:MonoBehaviour {
  public static BasketballBots Instance {get;private set;}
  public bool Solo {get;private set;}
  public static int PreferredTeamSize {get=>Mathf.Clamp(PlayerPrefs.GetInt("basketballBotTeamSize",2),2,5);set{if(FootballBots.ValidTeamSize(value)){PlayerPrefs.SetInt("basketballBotTeamSize",value);PlayerPrefs.Save();}}}
  public static bool IsBot(Athlete actor){var n=actor?actor.GetComponent<NetworkAthlete>():null;return n&&n.IsSpawned?n.BotSlot.Value>0&&n.BotSport.Value==SportId.Basketball:actor&&actor.GetComponent<BasketballBot>();}
  public static Athlete[] Roster=>Athlete.Active.Where(a=>a&&a.isActiveAndEnabled&&!a.inTransit&&!FootballBots.IsBot(a)).OrderBy(BasketballMatch.PlayerId).ToArray();
  public int Count=>Athlete.Active.Count(IsBot);
  public bool CanFillSize(int size)=>FootballBots.ValidTeamSize(size)&&AppRoot.Instance&&AppRoot.Instance.Exploring&&AppRoot.Instance.SelectedSport==SportId.Basketball&&AppRoot.Instance.rooms.Host&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling)&&BasketballMatch.Instance&&!BasketballMatch.Instance.Active&&Count==0&&Roster.Length<size*2;
  void Awake(){Instance=this;}
  void OnDestroy(){if(Instance==this)Instance=null;}
  public bool FillRoom(int size){if(!CanFillSize(size))return false;Spawn(size*2-Roster.Length);return true;}
  public bool StartSolo(int size){
   var app=AppRoot.Instance;if(!app||app.rooms.Connected||app.SelectedSport!=SportId.Basketball||app.PendingSelection.HasValue||!FootballBots.ValidTeamSize(size))return false;
   app.EnterOffline();Solo=true;Spawn(size*2-1);return true;
  }
  void Spawn(int count){
   var app=AppRoot.Instance;bool online=app.rooms.Connected;
   for(int i=1;i<=count;i++){
    var go=Instantiate(online?NetworkManager.Singleton.NetworkConfig.PlayerPrefab:app.athletePrefab,app.environments.Current.Spawn(i),Quaternion.identity);go.name="Basketball bot "+i;
    if(online){go.GetComponent<NetworkAthlete>().ConfigureBot((byte)i,SportId.Basketball);go.GetComponent<NetworkObject>().Spawn();}
    else {go.GetComponent<Athlete>().Setup();go.AddComponent<BasketballBot>().Initialize(i);}
   }
  }
  public void Clear(){
   if(AppRoot.Instance&&AppRoot.Instance.rooms.Connected&&!AppRoot.Instance.rooms.Host)return;
   Solo=false;
   foreach(var actor in Athlete.Active.Where(IsBot).ToArray()){
    var n=actor.GetComponent<NetworkObject>();if(n&&n.IsSpawned)n.Despawn();else {actor.gameObject.SetActive(false);Destroy(actor.gameObject);}
   }
  }
  void Update(){var app=AppRoot.Instance;if(app&&(!app.Exploring||app.SelectedSport!=SportId.Basketball)&&(Solo||Count>0))Clear();}
 }
}
