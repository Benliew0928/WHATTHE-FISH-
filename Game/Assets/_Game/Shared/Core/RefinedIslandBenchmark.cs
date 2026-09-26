#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
namespace WhatTheFish {
 public sealed class RefinedIslandBenchmark:MonoBehaviour {
  string report;int renderedFrames;
  void Rendered(ScriptableRenderContext context,Camera camera){if(camera==Camera.main)renderedFrames++;}
  void OnEnable(){RenderPipelineManager.endCameraRendering+=Rendered;}
  void OnDisable(){RenderPipelineManager.endCameraRendering-=Rendered;}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-islandBenchmark");if(i<0)return;if(Application.isBatchMode)throw new InvalidOperationException("Requires visible non-batch player");new GameObject("Island frame benchmark").AddComponent<RefinedIslandBenchmark>().report=a[i+1];}
  void Report(string label,List<float> times,int count){
   times.Sort();using(var p=System.Diagnostics.Process.GetCurrentProcess())File.AppendAllText(report,$"{label}: samples={times.Count}; rendered_frames={count}; mean_ms={times.Average():F3}; p95_ms={times[(int)(times.Count*.95)]:F3}; FPS={1000/times.Average():F1}; working_set_MiB={p.WorkingSet64/1048576}; private_MiB={p.PrivateMemorySize64/1048576}; Unity_reserved_MiB={Profiler.GetTotalReservedMemoryLong()/1048576}; graphics_MiB={Profiler.GetAllocatedMemoryForGraphicsDriver()/1048576}; resolution={Screen.width}x{Screen.height}\n");
  }
  IEnumerator Start(){
   yield return new WaitForSeconds(3);Screen.SetResolution(1920,1080,FullScreenMode.Windowed);QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;Application.runInBackground=true;
   File.WriteAllText(report,$"Visible Windows development player; GPU={SystemInfo.graphicsDeviceName}; driver={SystemInfo.graphicsDeviceVersion}; vSync off; 1080p; 120 warm-up + 900 frames/view, plus 60-second real-controller run.\n");
   var app=AppRoot.Instance;var camera=Camera.main;
   foreach(var sport in new[]{SportId.Golf,SportId.Fishing}){
    app.SelectSport(sport);app.EnterOffline();app.view.enabled=false;var layout=app.stadium.GetComponent<RefinedIslandEnvironment>().layout;
    foreach(var view in new[]{layout.views[0],layout.views[3],layout.views[6]}){
     camera.transform.position=view.position;camera.transform.LookAt(view.target);camera.fieldOfView=view.fov;var times=new List<float>();int before=renderedFrames;
     for(int n=0;n<1020;n++){yield return null;if(n>=120)times.Add(Time.unscaledDeltaTime*1000);}
     if(renderedFrames-before<900){File.AppendAllText(report,"INVALID: player did not render sufficient frames\n");Application.Quit(1);yield break;}Report(sport+" "+view.name,times,renderedFrames-before);
    }
    var route=layout.routes.Single(r=>r.name==(sport==SportId.Golf?"Main fairway":"Lagoon circuit"));var athlete=app.LocalAthlete;athlete.capsule.enabled=false;athlete.transform.position=route.points[0];athlete.capsule.enabled=true;athlete.ResetLocomotion();athlete.HideHead(true);
    DevelopmentProbe.TurnCommandActive=true;var samples=new List<float>();float elapsed=0,walked=0;int index=1,frames=renderedFrames;var start=athlete.transform.position;var previous=start;
    while(elapsed<60){
     var d=route.points[index]-athlete.transform.position;d.y=0;if(d.magnitude<.3f){index=(index+1)%route.points.Length;d=route.points[index]-athlete.transform.position;d.y=0;}
     float heading=Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg;DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,heading=heading};
     camera.transform.position=athlete.transform.position+Vector3.up*1.65f;camera.transform.rotation=Quaternion.Euler(-2,heading,0);camera.fieldOfView=70;
     yield return null;elapsed+=Time.unscaledDeltaTime;samples.Add(Time.unscaledDeltaTime*1000);var movement=athlete.transform.position-previous;movement.y=0;walked+=movement.magnitude;previous=athlete.transform.position;
    }
    DevelopmentProbe.TurnCommandActive=false;Report(sport+" sustained_walk_60s",samples,renderedFrames-frames);File.AppendAllText(report,$"{sport} walk_start={start} walk_end={athlete.transform.position} waypoint={index}/{route.points.Length} distance_metres={walked:F1}\n");
    if(walked<100){File.AppendAllText(report,"INVALID: sustained walking run covered less than 100 metres\n");Application.Quit(1);yield break;}
   }
   File.AppendAllText(report,"ISLAND_BENCHMARK_COMPLETE\n");RenderPipelineManager.endCameraRendering-=Rendered;camera.enabled=false;yield return new WaitForSeconds(1);Application.Quit();
  }
 }
}
#endif
