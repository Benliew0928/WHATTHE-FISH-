using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
  Canvas canvas;
  Text status;
  Image progress;
  Button retry;
  Camera loadingCamera;
  float fraction;

  void Awake(){Ready=false;Error=null;CreateLoadingView();}
  IEnumerator Start(){yield return Boot();}
  IEnumerator Boot(){
   Error=null;fraction=0;retry.gameObject.SetActive(false);
   var started=Time.realtimeSinceStartupAsDouble;
   yield return Prepare();
   PreparationSeconds=Time.realtimeSinceStartupAsDouble-started;
   if(Error!=null){status.text="Could not prepare islands. Please retry.";retry.gameObject.SetActive(true);Debug.LogError("CONTENT_LOAD_FAILED "+Error);yield break;}
   Debug.Log($"CONTENT_PREPARED cacheHit={CacheHit} seconds={PreparationSeconds:F3} allocatedBytes={UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()}");
   var load=SceneManager.LoadSceneAsync("Bootstrap",LoadSceneMode.Additive);
   yield return load;
   SceneManager.SetActiveScene(SceneManager.GetSceneByName("Bootstrap"));
   while(!AppRoot.Instance||!AppRoot.Instance.Menu)yield return null;
   Ready=true;canvas.gameObject.SetActive(false);loadingCamera.gameObject.SetActive(false);
   Debug.Log($"CONTENT_BOOT_READY seconds={Time.realtimeSinceStartupAsDouble-started:F3}");
  }
  IEnumerator Prepare(){
   PackagedContentCatalog catalog=null;
   using(var request=UnityWebRequest.Get(StreamingUrl("catalog.json"))){
    yield return request.SendWebRequest();
    if(request.result!=UnityWebRequest.Result.Success){Error=request.error;yield break;}
    try{catalog=JsonUtility.FromJson<PackagedContentCatalog>(request.downloadHandler.text);}
    catch(Exception e){Error=e.Message;}
   }
   if(catalog==null||catalog.version!=1||catalog.bundle!="islands.bundle"||catalog.sha256==null||!System.Text.RegularExpressions.Regex.IsMatch(catalog.sha256,"^[a-f0-9]{64}$")||catalog.scenes==null||catalog.scenes.Length!=5){Error="Invalid packaged content catalog";yield break;}
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
    var warm=AssetBundle.LoadFromFileAsync(cached);yield return warm;content=warm.assetBundle;
    if(content){fraction=1;yield break;}
    try{File.Delete(cached);}catch(Exception e){Error=e.Message;}
    CacheHit=false;if(Error!=null)yield break;
   }
   string temporary=Path.Combine(cacheRoot,catalog.sha256+"-"+Guid.NewGuid().ToString("N"));
   string source=temporary+".download",converted=temporary+".partial";
   using(var request=UnityWebRequest.Get(StreamingUrl(catalog.bundle))){
    request.downloadHandler=new DownloadHandlerFile(source){removeFileOnAbort=true};
    var operation=request.SendWebRequest();
    while(!operation.isDone){fraction=.3f*request.downloadProgress;yield return null;}
    if(request.result!=UnityWebRequest.Result.Success)Error=request.error;
   }
   if(Error==null){
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
     while(!operation.isDone){fraction=.3f+.6f*operation.progress;yield return null;}
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
   var load=AssetBundle.LoadFromFileAsync(cached);yield return load;content=load.assetBundle;
   if(!content){Error="Could not open prepared content";yield break;}
   fraction=1;
  }
  static string StreamingUrl(string name){
   string path=Application.streamingAssetsPath+"/PackagedContent/"+name;
   return path.Contains("://")?path:new Uri(path).AbsoluteUri;
  }
  void Update(){if(progress)progress.rectTransform.anchorMax=new Vector2(fraction,1);if(status&&Error==null)status.text=CacheHit?"Loading islands...":"Preparing islands... "+Mathf.RoundToInt(fraction*100)+"%";}
  void CreateLoadingView(){
   loadingCamera=new GameObject("Loading camera").AddComponent<Camera>();loadingCamera.clearFlags=CameraClearFlags.SolidColor;loadingCamera.backgroundColor=new Color(.05f,.16f,.14f);loadingCamera.cullingMask=0;
   canvas=new GameObject("Island loading",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
   var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
   Text Label(string name,string value,float y,int size){var text=new GameObject(name,typeof(RectTransform),typeof(Text)).GetComponent<Text>();text.transform.SetParent(canvas.transform,false);text.rectTransform.sizeDelta=new Vector2(720,70);text.rectTransform.anchoredPosition=new Vector2(0,y);text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=size;text.alignment=TextAnchor.MiddleCenter;text.color=Color.white;text.text=value;return text;}
   Label("Title","WHATTHE FISH?",65,36);status=Label("Status","Preparing islands...",0,22);
   var track=new GameObject("Progress",typeof(RectTransform),typeof(Image)).GetComponent<Image>();track.transform.SetParent(canvas.transform,false);track.rectTransform.sizeDelta=new Vector2(360,4);track.rectTransform.anchoredPosition=new Vector2(0,-48);track.color=new Color(.15f,.32f,.29f);
   progress=new GameObject("Fill",typeof(RectTransform),typeof(Image)).GetComponent<Image>();progress.transform.SetParent(track.transform,false);progress.rectTransform.anchorMin=Vector2.zero;progress.rectTransform.anchorMax=Vector2.one;progress.rectTransform.offsetMin=progress.rectTransform.offsetMax=Vector2.zero;progress.color=new Color(.6f,.88f,.7f);
   progress.rectTransform.anchorMax=new Vector2(0,1);
   var retryText=Label("Retry","Retry",-100,24);retry=retryText.gameObject.AddComponent<Button>();retry.targetGraphic=retryText;retry.onClick.AddListener(()=>StartCoroutine(Boot()));
   if(!FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>())new GameObject("Loading input",typeof(UnityEngine.EventSystems.EventSystem),typeof(UnityEngine.EventSystems.StandaloneInputModule));
  }
 }
}
