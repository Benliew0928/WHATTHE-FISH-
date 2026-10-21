using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 // Independent opponents for the two individual sports. Room play keeps its
 // human roster; these actors exist only for the explicit offline versus mode.
 public sealed class IslandBots:MonoBehaviour {
  public static IslandBots Instance {get;private set;}
  public bool Solo {get;private set;}
  public SportId Sport {get;private set;}
  public static bool Supports(SportId sport)=>sport==SportId.Fishing||sport==SportId.Golf;
  public static bool ValidCount(int count)=>count>=1&&count<=4;
  public static int PreferredCount {get=>Mathf.Clamp(PlayerPrefs.GetInt("islandBotOpponents",2),1,4);set{if(ValidCount(value)){PlayerPrefs.SetInt("islandBotOpponents",value);PlayerPrefs.Save();}}}
  public static FishingMode PreferredFishingMode {get=>PlayerPrefs.GetInt("fishingBotCup",0)==1?FishingMode.Cup:FishingMode.Round;set{PlayerPrefs.SetInt("fishingBotCup",value==FishingMode.Cup?1:0);PlayerPrefs.Save();}}
  public static bool IsBot(Athlete a)=>a&&(a.GetComponent<FishingBot>()||a.GetComponent<GolfBot>());
  public static ulong Id(Athlete a){var f=a?a.GetComponent<FishingBot>():null;var g=a?a.GetComponent<GolfBot>():null;return (1UL<<63)+(ulong)(f?f.Slot:g?g.Slot:0);}
  public static string Name(Athlete a)=>"BOT "+(Id(a)-(1UL<<63));
  public Athlete[] Roster=>Athlete.Active.Where(a=>a&&a.isActiveAndEnabled&&(a==AppRoot.Instance.LocalAthlete||IsBot(a))).OrderBy(a=>IsBot(a)?Id(a):0).ToArray();
  public int Count=>Athlete.Active.Count(a=>a&&a.isActiveAndEnabled&&IsBot(a));
  void Awake(){Instance=this;}
  void OnDestroy(){if(Instance==this)Instance=null;}
  public bool StartSolo(int count){
   var app=AppRoot.Instance;
   if(!app||app.rooms.Connected||app.PendingSelection.HasValue||RoomMatchStart.Travelling||!Supports(app.SelectedSport)||!ValidCount(count))return false;
   app.EnterOffline();Solo=true;Sport=app.SelectedSport;
   for(int slot=1;slot<=count;slot++){
    var go=Instantiate(app.athletePrefab,app.environments.Current.Spawn(slot),Quaternion.identity);go.name=Sport+" bot "+slot;
    go.GetComponent<Athlete>().Setup();
    if(Sport==SportId.Fishing)go.AddComponent<FishingBot>().Initialize(slot);else go.AddComponent<GolfBot>().Initialize(slot);
   }
   return true;
  }
  public void Clear(){
   Solo=false;
   foreach(var a in Athlete.Active.Where(IsBot).ToArray()){a.gameObject.SetActive(false);Destroy(a.gameObject);}
  }
  void Update(){var app=AppRoot.Instance;if(Solo&&(!app||!app.Exploring||app.rooms.Connected||app.SelectedSport!=Sport||RoomMatchStart.Travelling))Clear();}
 }
}
