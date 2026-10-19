#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WhatTheFish {
 public sealed class PackagingReview:MonoBehaviour {
  [Serializable] sealed class Measurement {public string island;public int cycle;public double loadSeconds;public long allocatedBytes;public int renderers,meshes;}
  [Serializable] sealed class Report {public bool success,packaged,cacheHit;public double startupSeconds,preparationSeconds,maxPreparationFrameSeconds;public long peakAllocatedBytes;public Measurement[] loads;}
  string folder;bool failed,measuringStartup=true;double lastFrame,maxFrame;long peak;readonly List<Measurement> loads=new();
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-packagingReview");if(i>=0&&i+1<args.Length)new GameObject("Packaging review").AddComponent<PackagingReview>().folder=args[i+1];}
  void Check(bool pass,string label){failed|=!pass;File.AppendAllText(Path.Combine(folder,"results.txt"),(pass?"PASS ":"FAIL ")+label+"\n");}
  void Update(){double now=Time.realtimeSinceStartupAsDouble;if(lastFrame>0&&measuringStartup)maxFrame=Math.Max(maxFrame,now-lastFrame);lastFrame=now;peak=Math.Max(peak,UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong());}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   bool packaged=FindFirstObjectByType<PackagedContentBoot>();
   bool retryReview=Array.IndexOf(Environment.GetCommandLineArgs(),"-packagingRetryReview")>=0;
   if(retryReview)yield return ReviewRetry();
   if(failed){Application.Quit(1);yield break;}
   double deadline=Time.realtimeSinceStartupAsDouble+120;
   while((!AppRoot.Instance||!AppRoot.Instance.Menu||!AppRoot.Instance.LocalAthlete||packaged&&!PackagedContentBoot.Ready)&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
   var app=AppRoot.Instance;Check(app&&app.Menu&&app.LocalAthlete,"World and home ready");if(failed){Application.Quit(1);yield break;}
   if(retryReview){
    string cache=ReviewCachePath();
    Check(File.Exists(Path.Combine(cache,"keep-me.txt")),"Cache cleanup preserves unrelated files");
    Check(!File.Exists(Path.Combine(cache,new string('a',64)+".bundle")),"Old complete cache removed only after successful load");
    Check(!Directory.GetFiles(cache).Any(p=>p.EndsWith(".download")||p.EndsWith(".partial")||p.EndsWith(".tmp")),"Interrupted and current preparation files cleaned");
   }
   double startup=Time.realtimeSinceStartupAsDouble;measuringStartup=false;
   Check(!packaged||PackagedContentBoot.Ready,"Packaged bootstrap completed");
   Check(app.environments.definitions.Length==4&&app.environments.definitions.All(d=>d&&d.available),"All four sports available");
   app.EnterOffline();app.view.enabled=false;SkySailWorld.Instance.reviewCamera=true;
   var streaming=app.environments.GetComponent<SkySailStreaming>();var camera=Camera.main;
   for(int cycle=0;cycle<2;cycle++)foreach(var sport in SkySailMap.Circuit){
    while(streaming.Busy)yield return null;
    double start=Time.realtimeSinceStartupAsDouble;yield return streaming.Prepare(sport);double seconds=Time.realtimeSinceStartupAsDouble-start;
    Check(streaming.Error==null&&streaming.Loaded(sport),sport+" load cycle "+cycle);
    if(failed)break;
    app.SelectSport(sport);yield return null;while(streaming.Busy)yield return null;
    var root=app.environments.roots[(int)sport];
    var renderers=root.GetComponentsInChildren<Renderer>(true);var filters=root.GetComponentsInChildren<MeshFilter>(true);
    // Pooled effects can reserve a disabled renderer before assigning its first
    // material. Validate every assigned shader, and require materials when visible.
    var materialIssues=renderers.Where(r=>{
     bool visible=r.enabled&&r.gameObject.activeInHierarchy;var materials=r.sharedMaterials;
     return visible&&materials.Length==0||materials.Any(m=>m?(!m.shader||!m.shader.isSupported):visible);
    }).ToArray();
    Check(renderers.Length>0&&materialIssues.Length==0,sport+" materials and shaders supported"+(materialIssues.Length>0?": "+string.Join(", ",materialIssues.Select(r=>r.name)):""));
    Check(filters.Any(f=>f.sharedMesh&&f.sharedMesh.vertexCount>0),sport+" detailed geometry present");
    Check(app.environments.roots.Count(r=>r&&r.activeSelf)==1,sport+" only active detailed island");
    Check(root.GetComponentsInChildren<Collider>(true).Length>0,sport+" collision retained");
    loads.Add(new Measurement{island=sport.ToString(),cycle=cycle,loadSeconds=seconds,allocatedBytes=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong(),renderers=renderers.Length,meshes=filters.Count(f=>f.sharedMesh)});
    if(cycle==0){camera.transform.position=app.environments.Current.menuCamera;camera.transform.LookAt(app.environments.Current.menuFocus);yield return null;Capture(camera,sport.ToString());}
   }
   yield return streaming.Prepare(SportId.Football);app.SelectSport(SportId.Football);yield return null;while(streaming.Busy)yield return null;
   Check(app.SelectedSport==SportId.Football&&streaming.Error==null,"Return from Fishing releases presentation cleanly");
   var report=new Report{success=!failed,packaged=packaged,cacheHit=PackagedContentBoot.CacheHit,startupSeconds=startup,preparationSeconds=PackagedContentBoot.PreparationSeconds,maxPreparationFrameSeconds=maxFrame,peakAllocatedBytes=peak,loads=loads.ToArray()};
   File.WriteAllText(Path.Combine(folder,"timings.json"),JsonUtility.ToJson(report,true));
   File.AppendAllText(Path.Combine(folder,"results.txt"),"PACKAGING_REVIEW_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);
  }
  string ReviewCachePath(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-contentCache");
   return i>=0&&i+1<args.Length?Path.GetFullPath(args[i+1]):"";
  }
  IEnumerator ReviewRetry(){
   double deadline=Time.realtimeSinceStartupAsDouble+15;
   while(PackagedContentBoot.Error==null&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
   Check(PackagedContentBoot.Error!=null,"Blocked cache produces a recoverable preparation error");
   Check(!PackagedContentBoot.Ready&&!AppRoot.Instance,"Failed preparation never starts a partial world");
   var welcome=StartupWelcome.Instance;
   var button=welcome?welcome.RetryButton:null;
   bool storageGuidance=welcome&&welcome.HasError&&welcome.LoadingPanel.GetComponentsInChildren<UnityEngine.UI.Text>().Any(t=>t.gameObject.activeInHierarchy&&t.text.Contains("storage"));
   Check(button&&button.gameObject.activeInHierarchy&&button.interactable,"Retry remains visible and usable");
   Check(storageGuidance,"Failure explains storage recovery in the loading screen");
   string cache=ReviewCachePath();
   bool owned=cache.StartsWith(Path.GetFullPath(folder)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)&&File.Exists(cache)&&new FileInfo(cache).Length==1;
   Check(owned,"Recovery fixture is confined to the explicit review output");
   if(failed||!owned)yield break;
   File.Delete(cache);Directory.CreateDirectory(cache);
   File.WriteAllText(Path.Combine(cache,"keep-me.txt"),"Unrelated file must survive");
   File.WriteAllBytes(Path.Combine(cache,new string('a',64)+".bundle"),Array.Empty<byte>());
   string temporary=Path.Combine(cache,new string('a',64)+"-"+new string('b',32));
   foreach(string suffix in new[]{".download",".partial",".partial.tmp"})File.WriteAllBytes(temporary+suffix,Array.Empty<byte>());
   button.onClick.Invoke();
   Check(PackagedContentBoot.Error==null&&!button.gameObject.activeSelf,"Retry resets the error and hides itself during preparation");
  }
  void Capture(Camera camera,string name){
   var target=RenderTexture.GetTemporary(1280,720,24);var previous=RenderTexture.active;var original=camera.targetTexture;
   var pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);
   try{camera.targetTexture=target;foreach(var instances in FindObjectsByType<MobileVegetationInstances>(FindObjectsSortMode.None))instances.RenderForCamera(camera);camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());
    var colors=pixels.GetPixels32();Check(colors.Where((_,i)=>i%100==0).Select(c=>(c.r/16,c.g/16,c.b/16)).Distinct().Count()>20,name+" nonblank rendered view");
    Check(colors.Count(c=>c.r>230&&c.b>230&&c.g<30)<colors.Length/100,name+" no missing-shader magenta");
   }finally{camera.targetTexture=original;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);Destroy(pixels);}
  }
 }
}
#endif
