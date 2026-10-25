using UnityEngine;
namespace WhatTheFish {
 // Reuses the Fishing/court awards presenter, models, poses and responsive UI.
 [DefaultExecutionOrder(450)]
 public sealed class GolfAwards:MonoBehaviour {
  GolfMatchManager match;FishingCeremony ceremony;uint shown;bool observed;
  void Awake(){match=GetComponent<GolfMatchManager>();}
  void Update(){
   if(!match||!match.Context){ceremony?.Cancel();return;}
   if(match.State.Phase!=GolfMatchPhase.Ended||observed&&shown==match.Round)return;
   if(TeleportSequence.Busy||RoomTeleport.Busy)return;
   if(!ceremony)ceremony=gameObject.AddComponent<FishingCeremony>();
   if(ceremony.BeginGolf(match,null)){shown=match.Round;observed=true;}
  }
  void OnDisable()=>ceremony?.Cancel();
 }
}
