using System.Collections.Generic;
using UnityEngine;

namespace WhatTheFish {
 // A separate multiplier preserves every widget's own disabled tint and aim fade.
 [DefaultExecutionOrder(200)]
 public sealed class GolfCinematicHUD:MonoBehaviour {
  readonly Dictionary<Canvas,CanvasGroup> groups=new();
  public float Opacity {get;private set;}=1;
  bool InGolf=>AppRoot.Instance&&AppRoot.Instance.Exploring&&AppRoot.Instance.SelectedSport==SportId.Golf;
  void Apply(){foreach(var pair in groups)if(pair.Value){pair.Value.alpha=Opacity;pair.Value.interactable=pair.Value.blocksRaycasts=Opacity>=.99f;}}
  void Update(){
   // Restore before awards snapshot the UI's normal visibility.
   if(!InGolf||!GolfMatchManager.Instance||!GolfMatchManager.Instance.State.Running){if(Opacity<1){Opacity=1;Apply();}}
  }
  void LateUpdate(){
   if(FishingCeremony.Presenting)return;
   bool watching=InGolf&&ActivityCinematic.Instance&&ActivityCinematic.Instance.CameraActive;
   if(!watching&&Opacity>=1)return;
   foreach(var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None)){
    if(!canvas.isRootCanvas||canvas.renderMode==RenderMode.WorldSpace||groups.ContainsKey(canvas))continue;
    groups[canvas]=canvas.gameObject.AddComponent<CanvasGroup>();
   }
   Opacity=Mathf.MoveTowards(Opacity,watching?0:1,Time.unscaledDeltaTime/.4f);Apply();
  }
  void OnDisable(){Opacity=1;Apply();}
  void OnDestroy(){foreach(var group in groups.Values)if(group)Destroy(group);}
 }
}
