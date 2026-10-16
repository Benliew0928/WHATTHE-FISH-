using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace WhatTheFish {
 [Serializable] public sealed class PackagedContentCatalog {
  public int version=1;
  public string bundle,sha256;
  public uint crc;
  public long bytes;
  public string[] scenes;
 }

 public sealed class PackagedContentBoot:MonoBehaviour {
  public static bool Ready {get;private set;}
  public static bool CacheHit {get;private set;}
  public static double PreparationSeconds {get;private set;}
  public static string Error {get;private set;}
  static AssetBundle content;
  StartupWelcome welcome;
  Camera loadingCamera;
  float fraction;bool busy;int attempts;
  static string contentHash;

  void Awake(){
   Ready=false;CacheHit=false;Error=null;
   loadingCamera=new GameObject("Loading camera").AddComponent<Camera>();loadingCamera.clearFlags=CameraClearFlags.SolidColor;loadingCamera.backgroundColor=new Color(.05f,.16f,.14f);loadingCamera.cullingMask=0;
   welcome=StartupWelcome.Ensure(true);welcome.SetRetry(()=>{if(!busy)StartCoroutine(Boot());});
  }
  IEnumerator Start(){yield return Boot();}
  IEnumerator Boot(){
   if(busy)yield break;busy=true;Error=null;fraction=0;welcome.BeginLoading();SetProgress(0,"Preparing your islands...");
   var started=Time.realtimeSinceStartupAsDouble;
   yield return Prepare();
   PreparationSeconds=Time.realtimeSinceStartupAsDouble-started;
   if(Error!=null){Fail();yield break;}
   Debug.Log($"CONTENT_PREPARED cacheHit={CacheHit} seconds={PreparationSeconds:F3} allocatedBytes={UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()}");
   AsyncOperation load=null;var scene=SceneManager.GetSceneByName("Bootstrap");
   if(!scene.isLoaded){
    try{load=SceneManager.LoadSceneAsync("Bootstrap",LoadSceneMode.Additive);}catch(Exception e){Error=e.Message;}
    if(Error!=null||load==null){Error??="Could not start game scene";Fail();yield break;}
    while(!load.isDone){SetProgress(.9f+.07f*Mathf.Clamp01(load.progress/.9f),"Loading your islands...");yield return null;}
    scene=SceneManager.GetSceneByName("Bootstrap");
   }
   if(!scene.IsValid()||!scene.isLoaded){Error="Game scene unavailable";Fail();yield break;}
   SceneManager.SetActiveScene(scene);SetProgress(.98f,"Getting your menu ready...");
   double deadline=Time.realtimeSinceStartupAsDouble+120;
   while((!AppRoot.Instance||!AppRoot.Instance.Menu)&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
   if(!AppRoot.Instance||!AppRoot.Instance.Menu){Error="Game menu did not become ready";Fail();yield break;}
   Ready=true;busy=false;loadingCamera.gameObject.SetActive(false);welcome.SetReady();
   Debug.Log($"CONTENT_BOOT_READY seconds={Time.realtimeSinceStartupAsDouble-started:F3}");
  }
  IEnumerator Prepare(){
   attempts++;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(attempts==1&&Array.IndexOf(Environment.GetCommandLineArgs(),"-welcomeFailOnce")>=0){Error="Development fixture: first preparation fails";yield break;}
#endif
   PackagedContentCatalog catalog=null;
   using(var request=UnityWebRequest.Get(StreamingUrl("catalog.json"))){
    yield return request.SendWebRequest();
    if(request.result!=UnityWebRequest.Result.Success){Error=request.error;yield break;}
    try{catalog=JsonUtility.FromJson<PackagedContentCatalog>(request.downloadHandler.text);}
    catch(Exception e){Error=e.Message;}
   }
   if(catalog==null||catalog.version!=1||catalog.bundle!="islands.bundle"||catalog.sha256==null||!System.Text.RegularExpressions.Regex.IsMatch(catalog.sha256,"^[a-f0-9]{64}$")||catalog.scenes==null||catalog.scenes.Length!=5){Error="Invalid packaged content catalog";yield break;}
   if(content&&contentHash==catalog.sha256){SetProgress(.9f,"Loading your islands...");yield break;}
   SetProgress(.03f,"Preparing your islands...");
   string cacheRoot=Path.Combine(Application.persistentDataPath,"PackagedContent-v1");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   var args=Environment.GetCommandLineArgs();int cacheArg=Array.IndexOf(args,"-contentCache");
   if(cacheArg>=0&&cacheArg+1<args.Length)cacheRoot=Path.GetFullPath(args[cacheArg+1]);
#endif
   string cached=Path.Combine(cacheRoot,catalog.sha256+".bundle");
   try{Directory.CreateDirectory(cacheRoot);}catch(Exception e){Error=e.Message;}
   if(Error!=null)yield break;
   CacheHit=File.Exists(cached);
   if(CacheHit){
    var warm=AssetBundle.LoadFromFileAsync(cached);while(!warm.isDone){SetProgress(.03f+.87f*warm.progress,"Loading your islands...");yield return null;}content=warm.assetBundle;
    if(content){contentHash=catalog.sha256;SetProgress(.9f,"Loading your islands...");yield break;}
    try{File.Delete(cached);}catch(Exception e){Error=e.Message;}
    CacheHit=false;if(Error!=null)yield break;
   }
   string temporary=Path.Combine(cacheRoot,catalog.sha256+"-"+Guid.NewGuid().ToString("N"));
   string source=temporary+".download",converted=temporary+".partial";
   using(var request=UnityWebRequest.Get(StreamingUrl(catalog.bundle))){
    request.downloadHandler=new DownloadHandlerFile(source){removeFileOnAbort=true};
    var operation=request.SendWebRequest();
    while(!operation.isDone){SetProgress(.03f+.27f*Mathf.Clamp01(request.downloadProgress),"Preparing your islands...");yield return null;}
    if(request.result!=UnityWebRequest.Result.Success)Error=request.error;
   }
   if(Error==null){
    SetProgress(.30f,"Checking game content...");
    var hash=Task.Run(()=>{using var stream=File.OpenRead(source);using var sha=SHA256.Create();return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();});
    while(!hash.IsCompleted)yield return null;
    if(hash.IsFaulted)Error=hash.Exception.GetBaseException().Message;
    else if(hash.Result!=catalog.sha256||new FileInfo(source).Length!=catalog.bytes)Error="Packaged content checksum mismatch";
   }
   if(Error==null){
    AssetBundleRecompressOperation operation=null;
    try{operation=AssetBundle.RecompressAssetBundleAsync(source,converted,BuildCompression.LZ4Runtime,catalog.crc,ThreadPriority.Low);}
    catch(Exception e){Error=e.Message;}
    if(operation!=null){
     while(!operation.isDone){SetProgress(.35f+.49f*operation.progress,"Preparing your islands...");yield return null;}
     if(!operation.success)Error=operation.humanReadableResult;
    }
   }
   try{
    if(Error==null&&!File.Exists(cached)){
     // Another local player may finish the same hash-keyed cache concurrently.
     try{File.Move(converted,cached);}catch(IOException) when(File.Exists(cached)){}
    }
    if(File.Exists(source))File.Delete(source);
    if(File.Exists(converted))File.Delete(converted);
   }catch(Exception e){Error=e.Message;}
   if(Error!=null)yield break;
   var load=AssetBundle.LoadFromFileAsync(cached);while(!load.isDone){SetProgress(.84f+.06f*load.progress,"Loading your islands...");yield return null;}content=load.assetBundle;
   if(!content){Error="Could not open prepared content";yield break;}
   contentHash=catalog.sha256;SetProgress(.9f,"Loading your islands...");
  }
  static string StreamingUrl(string name){
   string path=Application.streamingAssetsPath+"/PackagedContent/"+name;
   return path.Contains("://")?path:new Uri(path).AbsoluteUri;
  }
  void SetProgress(float value,string label){fraction=Mathf.Max(fraction,value);if(welcome)welcome.SetProgress(fraction,label);}
  void Fail(){busy=false;welcome.ShowError();Debug.LogError("CONTENT_LOAD_FAILED "+Error);}
 }
}
