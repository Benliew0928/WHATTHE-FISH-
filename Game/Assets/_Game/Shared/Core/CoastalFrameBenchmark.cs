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
 // Normal rendered-player timing. Never interpret batch-mode update rate as FPS.
 public sealed class CoastalFrameBenchmark:MonoBehaviour {
  string report;int renderedFrames;
  void Rendered(ScriptableRenderContext context,Camera camera){if(camera==Camera.main)renderedFrames++;}
  void OnEnable(){RenderPipelineManager.endCameraRendering+=Rendered;}
  void OnDisable(){RenderPipelineManager.endCameraRendering-=Rendered;}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-coastalFrameBenchmark");if(i<0)return;
   if(Application.isBatchMode)throw new InvalidOperationException("Frame benchmark requires a normally rendered player, without -batchmode.");
   new GameObject("Coastal frame benchmark").AddComponent<CoastalFrameBenchmark>().report=args[i+1];
  }
  IEnumerator Start(){
   yield return new WaitForSeconds(3);Screen.SetResolution(1920,1080,FullScreenMode.Windowed);
   QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;Application.runInBackground=true;
   File.WriteAllText(report,$"Normal Windows player; {SystemInfo.graphicsDeviceName}; 1920x1080; vSync off; uncapped; native development={Debug.isDebugBuild}. 120 warm-up + 600 measured frames per view.\n");
   var app=AppRoot.Instance;var camera=Camera.main;
   foreach(var sport in new[]{SportId.Football,SportId.Basketball}){
    app.SelectSport(sport);app.EnterOffline();app.view.enabled=false;
    var routes=app.stadium.GetComponent<CoastalVenueRoutes>();var main=Enumerable.Range(0,4).OrderBy(i=>routes.entries[i].z).First();
    var entry=routes.entries[main];var inward=(routes.inside[main]-entry).normalized;bool football=sport==SportId.Football;
    var positions=new[]{football?new Vector3(140,86,-174):new Vector3(72,43,-86),entry-inward*4+Vector3.up*1.6f,football?new Vector3(12,1.7f,-20):new Vector3(5,1.65f,-9)};
    var targets=new[]{new Vector3(0,football?7:4,0),entry+inward*9+Vector3.up*3,new Vector3(0,football?12:8,football?45:20)};
    for(int view=0;view<3;view++){
     camera.transform.position=positions[view];camera.transform.LookAt(targets[view]);camera.fieldOfView=view==0?48:68;
     var times=new List<float>();int before=renderedFrames;
     for(int n=0;n<720;n++){yield return null;if(n>=120)times.Add(Time.unscaledDeltaTime*1000);}
     if(renderedFrames-before<600){File.AppendAllText(report,"INVALID: player window did not render enough frames; ensure it is visible.\n");Application.Quit(1);yield break;}
     times.Sort();
     File.AppendAllText(report,$"{sport} {new[]{"exterior","entrance","playing area"}[view]}: mean_ms={times.Average():F2}; p95_ms={times[(int)(times.Count*.95)]:F2}; FPS={1000/times.Average():F1}; rendered_frames={renderedFrames-before}; Unity_reserved_MB={Profiler.GetTotalReservedMemoryLong()/1048576}; graphics_MB={Profiler.GetAllocatedMemoryForGraphicsDriver()/1048576}; resolution={Screen.width}x{Screen.height}\n");
    }
   }
   File.AppendAllText(report,"FRAME_BENCHMARK_COMPLETE\n");
   // Detach the diagnostic hook and drain rendering before player teardown.
   RenderPipelineManager.endCameraRendering-=Rendered;camera.enabled=false;
   yield return new WaitForSeconds(1);Application.Quit();
  }
 }
}
#endif
