#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed partial class GolfMatchManager {
  bool reviewRushBeforeFinish;uint reviewRushRound;
  void Update(){
   if(!Authority||!Context)return;
   // A manually tested Rush must not suppress the real first-finisher Rush.
   if(reviewRushBeforeFinish){
    if(reviewRushRound!=Round)reviewRushBeforeFinish=false;
    else if(State.CountdownStarted||Now>=Events.Snapshot.rushAt+GolfEventSchedule.RushDuration){var state=Events.Snapshot;state.rushAt=0;Events.Receive(state);reviewRushBeforeFinish=false;Publish();}
   }
   if(!Application.isFocused)return;
   var selected=EventSystem.current?EventSystem.current.currentSelectedGameObject:null;if(selected&&selected.GetComponent<InputField>())return;
   for(int i=0;i<9;i++){var key=(KeyCode)((int)KeyCode.F1+i);if(Input.GetKeyDown(key))ReviewEventHotkey(key);}
  }
  // Shared dispatch for the keyboard and focused development checks. There is
  // no client-to-server debug RPC: only the host/offline authority can trigger.
  public bool ReviewEventHotkey(KeyCode key){
   if(!Authority||!Context||Starting||State.Phase==GolfMatchPhase.Idle||!State.Running&&key!=KeyCode.F7)return false;
   if(key<KeyCode.F1||key>KeyCode.F9||key==KeyCode.F2||key==KeyCode.F5)return false;

   var state=Events.Snapshot;state.nextCycle=Now+GolfEventSchedule.Cycle;Events.Receive(state);
   if(key<=KeyCode.F4)ReviewEvent((GolfEventKind)(1+(int)key-(int)KeyCode.F1));
   else if(key==KeyCode.F8)ReviewEvent(GolfEventKind.Tornado);
   else if(key==KeyCode.F9)ReviewEvent(GolfEventKind.CrabCrossing);
   else if(key==KeyCode.F6){state=Events.Snapshot;state.rushAt=Now;Events.Receive(state);reviewRushBeforeFinish=!State.CountdownStarted;reviewRushRound=Round;Publish();}
   else {state=Events.Snapshot;state.kind=GolfEventKind.None;state.start=state.end=0;state.wind=Vector3.zero;state.rushAt=State.CountdownStarted?-1:0;state.sequence++;Events.Receive(state);reviewRushBeforeFinish=false;Publish();}
   Debug.Log("Golf test hotkey: "+key);return true;
  }
 }
}
#endif
