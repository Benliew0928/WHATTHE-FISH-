#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 // Opt-in local presentation preview; never changes scores or finishes a room.
 public sealed class GolfAwardsPreview:MonoBehaviour {
  FishingCeremony ceremony;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  static void Boot(){if(Environment.GetCommandLineArgs().Contains("-golfAwardsPreview"))new GameObject("Golf awards preview launcher").AddComponent<GolfAwardsPreview>();}
  IEnumerator Start(){
   float deadline=Time.realtimeSinceStartup+90;
   while(Time.realtimeSinceStartup<deadline){
    var app=AppRoot.Instance;var match=GolfMatchManager.Instance;
    if(app&&app.Exploring&&app.SelectedSport==SportId.Golf&&app.LocalAthlete&&match&&match.Context&&!TeleportSequence.Busy&&!RoomTeleport.Busy){
     while(IslandTutorialUI.DismissTop()){}
     yield return new WaitForSecondsRealtime(1);
     if(Play())yield break;
    }
    yield return null;
   }
   Debug.LogWarning("Golf awards preview needs a loaded Golf island. Launch with -offline -sport Golf -golfAwardsPreview.");
  }
  bool Play(){if(FishingCeremony.Presenting)return false;if(!ceremony)ceremony=gameObject.AddComponent<FishingCeremony>();return ceremony.BeginGolfPreview(GolfMatchManager.Instance);}
  void Update(){if(Input.GetKeyDown(KeyCode.F10))Play();}
  void OnDisable(){if(ceremony)ceremony.Cancel();}
 }
}
#endif
