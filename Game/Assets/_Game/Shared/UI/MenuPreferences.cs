using UnityEngine;

namespace WhatTheFish {
 public static class MenuPreferences {
  public static bool ReducedMotion {get; private set;}
  public static float MasterVolume {get; private set;}
  public static float UIVolume {get; private set;}
  public static void Load() {
   ReducedMotion = PlayerPrefs.GetInt("cove.motion", 0) == 1;
   MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("cove.master", .8f));
   UIVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("cove.ui", .65f));
   AudioListener.volume = MasterVolume;
  }
  public static void SetMotion(bool reduced) {ReducedMotion = reduced; PlayerPrefs.SetInt("cove.motion", reduced ? 1 : 0);}
  public static void SetMaster(float value) {MasterVolume = Mathf.Clamp01(value); AudioListener.volume = MasterVolume; PlayerPrefs.SetFloat("cove.master", MasterVolume);}
  public static void SetUI(float value) {UIVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat("cove.ui", UIVolume);}
  public static void Save() => PlayerPrefs.Save();
 }

 // Only the host writes this replicated setting. It is read when a new round starts.
 public static class MenuMatchRules {
  public static int Minutes {get; private set;} = 3;
  public static int OverrideMinutes {get; private set;}
  public static int Sanitize(int value) => value == 5 || value == 10 ? value : 3;
  public static int CurrentMinutes => AppRoot.Instance && AppRoot.Instance.rooms.Connected && NetworkAthlete.HostPlayer
   ? Sanitize(NetworkAthlete.HostPlayer.FootballMinutes.Value) : Minutes;
  public static float RegulationSeconds(float authoredSeconds) {
   int value=AppRoot.Instance&&AppRoot.Instance.rooms.Connected&&NetworkAthlete.HostPlayer
    ? NetworkAthlete.HostPlayer.FootballMinutes.Value:OverrideMinutes;
   return value==0?authoredSeconds:Sanitize(value)*60;
  }
  public static bool SetMinutes(int value) {
   var app = AppRoot.Instance;
   if (app && app.rooms.Connected && (!app.rooms.Host || app.Exploring)) return false;
   Minutes = Sanitize(value); OverrideMinutes=Minutes;
   if (app && app.rooms.Host && NetworkAthlete.HostPlayer) {
    NetworkAthlete.HostPlayer.FootballMinutes.Value = Minutes;
    // A rule change requires everyone to acknowledge the new setup.
    foreach (var player in Object.FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None))
     if (player.IsSpawned && player.IsServer) player.Ready.Value = false;
   }
   return true;
  }
 }
}
