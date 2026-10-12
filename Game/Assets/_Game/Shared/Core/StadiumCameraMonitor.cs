#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using UnityEngine;

namespace WhatTheFish {
 // Opt-in observer for the existing three-peer ability regression. This never
 // moves the ball or players, so guest interpolation and real portals are tested.
 [DefaultExecutionOrder(250)]
 public sealed class StadiumCameraMonitor:MonoBehaviour {
  [Serializable]sealed class Report {public int frames,flightFrames,heldFrames,offscreenFrames,ballFailures,arrowFailures;public bool guest;}
  readonly Report report=new();string path;float saveAt;Camera camera;StadiumPlayerIndicator marker;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-stadiumCameraMonitor");
   if(i>=0&&i+1<args.Length)new GameObject("Network camera monitor").AddComponent<StadiumCameraMonitor>().path=args[i+1];
  }
  void Update(){var app=AppRoot.Instance;if(app&&app.Exploring&&app.SelectedSport==SportId.Basketball)app.view.mode=2;}
  void LateUpdate(){
   var app=AppRoot.Instance;var ball=BasketballBall.Active;
   if(!app||!ball||!app.view.StadiumFollowing||!app.LocalAthlete)return;
   if(!camera)camera=app.view.GetComponent<Camera>();if(!marker)marker=FindFirstObjectByType<StadiumPlayerIndicator>();
   report.guest=!ball.Authority;report.frames++;if(ball.Held)report.heldFrames++;else if(ball.transform.localPosition.y>2)report.flightFrames++;
   var p=camera.WorldToViewportPoint(ball.transform.position);
   if(p.z<=0||p.x<.115f||p.x>.835f||p.y<.245f||p.y>.785f)report.ballFailures++;
   var screen=camera.WorldToScreenPoint(app.LocalAthlete.transform.position+Vector3.up*.9f);
   bool outside=screen.z<=camera.nearClipPlane||!Screen.safeArea.Contains(new Vector2(screen.x,screen.y));
   if(outside){report.offscreenFrames++;if(!marker||!marker.Showing)report.arrowFailures++;}
   if(Time.unscaledTime>=saveAt){saveAt=Time.unscaledTime+.5f;File.WriteAllText(path,JsonUtility.ToJson(report,true));}
  }
 }
}
#endif
