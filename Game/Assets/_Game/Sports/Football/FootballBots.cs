using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 // Bots reuse the athlete prefab, but never impersonate a connected human owner.
 public sealed class FootballBots:MonoBehaviour {
  public static FootballBots Instance {get;private set;}
  public bool Solo {get;private set;}
  public int Seed {get;private set;}=1729;
  public const int MinTeamSize=2,MaxTeamSize=5;
  public static bool ValidTeamSize(int size)=>size>=MinTeamSize&&size<=MaxTeamSize;
  public static int PreferredTeamSize {get=>Mathf.Clamp(PlayerPrefs.GetInt("footballAITeamSize",2),MinTeamSize,MaxTeamSize);set{if(ValidTeamSize(value)){PlayerPrefs.SetInt("footballAITeamSize",value);PlayerPrefs.Save();}}}
  public static bool IsBot(Athlete actor){var n=actor?actor.GetComponent<NetworkAthlete>():null;return n&&n.IsSpawned?n.BotSlot.Value>0&&n.BotSport.Value==SportId.Football:actor&&actor!=AppRoot.Instance.LocalAthlete&&actor.GetComponent<FootballBot>();}
  public static ulong Id(Athlete actor){
   var n=actor?actor.GetComponent<NetworkAthlete>():null;
   if(n&&n.IsSpawned)return n.BotSlot.Value>0?(1UL<<63)+n.BotSlot.Value:n.StableId;
   var bot=actor?actor.GetComponent<FootballBot>():null;return bot&&actor!=AppRoot.Instance.LocalAthlete?(1UL<<63)+(ulong)bot.Slot:ulong.MaxValue;
  }
  public static Athlete[] Roster=>Athlete.Active.Where(a=>a&&a.isActiveAndEnabled&&!a.IsSpectator&&!a.inTransit).OrderBy(Id).ToArray();
  public int Count=>Athlete.Active.Count(IsBot);
  public bool CanFill=>CanFillSize(PreferredTeamSize);
  public bool CanFillSize(int size)=>ValidTeamSize(size)&&AppRoot.Instance&&AppRoot.Instance.Exploring&&AppRoot.Instance.SelectedSport==SportId.Football&&AppRoot.Instance.rooms.Host&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling)&&FootballMatch.Instance&&!FootballMatch.Instance.Active&&Count==0&&Roster.Length<size*2;
  void Awake(){Instance=this;}
  void OnDestroy(){if(Instance==this)Instance=null;}
  public bool FillRoom()=>FillRoom(PreferredTeamSize);
  public bool FillRoom(int teamSize){if(!CanFillSize(teamSize))return false;Spawn(teamSize*2-Roster.Length);return true;}
  public bool StartSolo()=>StartSolo(PreferredTeamSize);
  public bool StartSolo(int teamSize){
   var app=AppRoot.Instance;if(!app||app.rooms.Connected||app.SelectedSport!=SportId.Football||app.PendingSelection.HasValue)return false;
   if(!ValidTeamSize(teamSize))return false;
   FootballMatch.Instance?.ClearMatch();Clear();app.EnterOffline();Solo=true;Spawn(teamSize*2-1);return true;
  }
  void Spawn(int count){
   var app=AppRoot.Instance;bool online=app.rooms.Connected;
   for(int i=1;i<=count;i++){
    var prefab=online?NetworkManager.Singleton.NetworkConfig.PlayerPrefab:app.athletePrefab;
    var go=Instantiate(prefab,app.environments.Current.Spawn(i),Quaternion.identity);go.name="Football bot "+i;
    var n=go.GetComponent<NetworkAthlete>();
    if(online){n.ConfigureBot((byte)i);go.GetComponent<NetworkObject>().Spawn();}
    else {var actor=go.GetComponent<Athlete>();actor.Setup();go.AddComponent<FootballBot>().Initialize(i,Seed);}
   }
  }
  public void Clear(){
   if(AppRoot.Instance&&AppRoot.Instance.rooms.Connected&&!AppRoot.Instance.rooms.Host)return;
   Solo=false;
   foreach(var actor in Athlete.Active.Where(IsBot).ToArray()){
    var n=actor.GetComponent<NetworkObject>();if(n&&n.IsSpawned)n.Despawn();else {actor.gameObject.SetActive(false);Destroy(actor.gameObject);}
   }
  }
  public void ResetAgents(int seed){Seed=seed;foreach(var actor in Roster){var bot=actor.GetComponent<FootballBot>();if(bot)bot.Initialize(bot.Slot,seed);}}
  void Update(){
   var app=AppRoot.Instance;if(!app)return;
   if(!app.Exploring||app.SelectedSport!=SportId.Football){if(Solo||Count>0)Clear();}
  }
 }
}
