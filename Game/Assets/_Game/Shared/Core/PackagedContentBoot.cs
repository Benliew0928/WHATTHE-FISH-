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
   if(Error!=null){status.text="Could not prepare islands.\nCheck free storage, then tap Retry.";retry.gameObject.SetActive(true);Debug.LogError("CONTENT_LOAD_FAILED "+Error);yield break;}
   Debug.Log($"CONTENT_PREPARED cacheHit={CacheHit} seconds={PreparationSeconds:F3} allocatedBytes={UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()}");
   var load=SceneManager.LoadSceneAsync("Bootstrap",LoadSceneMode.Additive);
   yield return load;
   SceneManager.SetActiveScene(SceneManager.GetSceneByName("Bootstrap"));
   while(!AppRoot.Instance||!AppRoot.Instance.Menu)yield return null;
   Ready=true;canvas.gameObject.SetActive(false);loadingCamera.gameObject.SetActive(false);
   Debug.Log($"CONTENT_BOOT_READY seconds={Time.realtimeSinceStartupAsDouble-started:F3}");
  }
  IEnumerator Prepare(){
   CacheHit=false;
   PackagedContentCatalog catalog=null;
   using(var request=UnityWebRequest.Get(StreamingUrl("catalog.json"))){
    yield return request.SendWebRequest();
    if(request.result!=UnityWebRequest.Result.Success){Error=request.error;yield break;}
    try{catalog=JsonUtility.FromJson<PackagedContentCatalog>(request.downloadHandler.text);}
    catch(Exception e){Error=e.Message;}
   }
   if(catalog==null||catalog.version!=1||catalog.bundle!="islands.bundle"||catalog.bytes<=0||catalog.sha256==null||!System.Text.RegularExpressions.Regex.IsMatch(catalog.sha256,"^[a-f0-9]{64}$")||catalog.scenes==null||catalog.scenes.Length!=5){Error="Invalid packaged content catalog";yield break;}
   string cacheRoot=Path.Combine(Application.persistentDataPath,"PackagedContent-v1");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   var args=Environment.GetCommandLineArgs();int cacheArg=Array.IndexOf(args,"-contentCache");
   if(cacheArg>=0&&cacheArg+1<args.Length)cacheRoot=Path.GetFullPath(args[cacheArg+1]);
#endif
   Debug.Log("CONTENT_CACHE_ROOT "+cacheRoot);
   try{Directory.CreateDirectory(cacheRoot);}catch(Exception e){Error=e.Message;}
   if(Error!=null)yield break;
   // A process-wide file lease prevents two launches from removing each other's
   // preparation files. A crashed process releases the handle automatically.
   FileStream lease=null;
   double deadline=Time.realtimeSinceStartupAsDouble+120;
   while(lease==null&&Error==null){
    try{lease=new FileStream(Path.Combine(cacheRoot,"prepare.lock"),FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);}
    catch(IOException){if(Time.realtimeSinceStartupAsDouble>=deadline)Error="Another game is still preparing islands. Close it, then retry.";}
    catch(Exception e){Error=e.Message;}
    if(lease==null&&Error==null)yield return null;
   }
   if(Error!=null)yield break;
   try{yield return PrepareLocked(catalog,cacheRoot);}
   finally{lease.Dispose();}
  }
  IEnumerator PrepareLocked(PackagedContentCatalog catalog,string cacheRoot){
   CleanCache(cacheRoot,catalog.sha256,false);
   string cached=Path.Combine(cacheRoot,catalog.sha256+".bundle");
   CacheHit=File.Exists(cached);
   if(CacheHit){
    var warm=AssetBundle.LoadFromFileAsync(cached);yield return warm;content=warm.assetBundle;
    if(content){CleanCache(cacheRoot,catalog.sha256,true);fraction=1;yield break;}
    try{File.Delete(cached);}catch(Exception e){Error=e.Message;}
    CacheHit=false;if(Error!=null)yield break;
   }
   string temporary=Path.Combine(cacheRoot,catalog.sha256+"-"+Guid.NewGuid().ToString("N"));
   string download=temporary+".download",converted=temporary+".partial";
   // Desktop StreamingAssets is already a file. Hash and convert it in place
   // instead of making a second compressed copy beside the expanded cache.
   string local=Path.Combine(Application.streamingAssetsPath,"PackagedContent",catalog.bundle);
   bool direct=!Application.streamingAssetsPath.Contains("://");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(Array.IndexOf(Environment.GetCommandLineArgs(),"-contentCopySource")>=0)direct=false;
#endif
   string source=direct?local:download;
   Debug.Log("CONTENT_SOURCE direct="+direct);
   try{
   if(!direct)using(var request=DownloadRequest(catalog.bundle,download)){
    if(request!=null){
     var operation=request.SendWebRequest();
     while(!operation.isDone){fraction=.3f*request.downloadProgress;yield return null;}
     if(request.result!=UnityWebRequest.Result.Success)Error=request.error;
    }}
   if(Error==null){
    var hash=Task.Run(()=>{using var stream=File.OpenRead(source);using var sha=SHA256.Create();return stream.Length==catalog.bytes&&BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant()==catalog.sha256;});
    while(!hash.IsCompleted)yield return null;
    if(hash.IsFaulted)Error=hash.Exception.GetBaseException().Message;
    else if(!hash.Result)Error="Packaged content checksum mismatch. Copy the complete game folder again.";
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
   if(Error==null&&!File.Exists(cached))try{
    try{File.Move(converted,cached);}catch(IOException) when(File.Exists(cached)){}
   }catch(Exception e){Error=e.Message;}
   }finally{
    // Unity may leave .partial.tmp after disk-full or interrupted conversion.
    // Never delete the original StreamingAssets file or another transaction.
    DeleteTemporary(download);DeleteTemporary(converted);DeleteTemporary(converted+".tmp");
   }
   if(Error!=null)yield break;
   var load=AssetBundle.LoadFromFileAsync(cached);yield return load;content=load.assetBundle;
   if(!content){Error="Could not open prepared content";yield break;}
   CleanCache(cacheRoot,catalog.sha256,true);
   fraction=1;
  }
  static UnityWebRequest DownloadRequest(string name,string path){
   UnityWebRequest request=null;
   try{request=UnityWebRequest.Get(StreamingUrl(name));request.downloadHandler=new DownloadHandlerFile(path){removeFileOnAbort=true};return request;}
   catch(Exception e){request?.Dispose();Error=e.Message;return null;}
  }
  static void DeleteTemporary(string path){try{if(File.Exists(path))File.Delete(path);}catch(Exception e){Debug.LogWarning("CONTENT_CACHE_CLEANUP "+e.Message);}}
  static void CleanCache(string root,string current,bool loaded){
   // Only files created by this loader belong to it. Preserve other files and
   // previous complete caches until the current content has actually loaded.
   try{foreach(string path in Directory.GetFiles(root)){
    string name=Path.GetFileName(path);
    bool transaction=System.Text.RegularExpressions.Regex.IsMatch(name,"^[a-f0-9]{64}-[a-f0-9]{32}\\.(download|partial|partial\\.tmp)$");
    bool obsolete=loaded&&name!=current+".bundle"&&System.Text.RegularExpressions.Regex.IsMatch(name,"^[a-f0-9]{64}\\.bundle$");
    if((transaction||obsolete)&&(File.GetAttributes(path)&FileAttributes.ReparsePoint)==0)DeleteTemporary(path);
   }}catch(Exception e){Debug.LogWarning("CONTENT_CACHE_CLEANUP "+e.Message);}
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
