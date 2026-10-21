using System;
using UnityEngine;
using UnityEngine.Scripting;
using Net.Codecrete.QrCodeGenerator;

namespace WhatTheFish {
 // Android uses Huawei for both encoding and scanning. Desktop uses the same invitation with a portable encoder.
 public sealed class HuaweiRoomScan:MonoBehaviour {
  const string Bridge="com.umpsa.whatthefish.scan.RoomScanBridge";
  static HuaweiRoomScan instance;
  Action<string,string> completion;float pollAt;int request;
  public static bool Supported=>Application.platform==RuntimePlatform.Android;
  public static bool Busy=>instance&&instance.completion!=null;
  public static string LastGenerationProvider {get;private set;}
  [Serializable,Preserve]sealed class Result {public int request;public string status;public string value;}
  static HuaweiRoomScan Instance {
   get {if(!instance){var go=new GameObject("Huawei room scanner");DontDestroyOnLoad(go);instance=go.AddComponent<HuaweiRoomScan>();}return instance;}
  }
  public static Texture2D Generate(string roomCode) {
   string payload=RoomInvitation.Encode(roomCode);
#if UNITY_ANDROID && !UNITY_EDITOR
   using(var bridge=new AndroidJavaClass(Bridge)) {
    byte[] png=bridge.CallStatic<byte[]>("generate",payload);
    var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);
    if(!ImageConversion.LoadImage(texture,png)){Destroy(texture);throw new InvalidOperationException("The QR image could not be created.");}
    texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;
    LastGenerationProvider="Huawei Scan Kit 2.15.0.301";return texture;
   }
#else
   var qr=QrCode.EncodeText(payload,QrCode.Ecc.Medium);const int border=4,scale=8;
   int width=(qr.Size+border*2)*scale;var pixels=new Color32[width*width];
   for(int y=0;y<width;y++)for(int x=0;x<width;x++)pixels[y*width+x]=qr.GetModule(x/scale-border,(width-1-y)/scale-border)?new Color32(0,0,0,255):new Color32(255,255,255,255);
   var texture=new Texture2D(width,width,TextureFormat.RGBA32,false);texture.SetPixels32(pixels);texture.Apply();texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;
   LastGenerationProvider="Codecrete desktop encoder";return texture;
#endif
  }
  public static void Scan(Action<string,string> completed,bool fromImage=false) {
   if(Busy)return;
   if(!Supported){completed("unavailable","Scanning is available in the Android game. Enter or paste the room code here.");return;}
#if UNITY_ANDROID && !UNITY_EDITOR
   var self=Instance;self.completion=completed;self.request++;
   try {
    using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))
    using(var bridge=new AndroidJavaClass(Bridge))bridge.CallStatic("start",activity,self.request,fromImage);
   }catch(Exception e){Debug.LogWarning("Scan Kit launch failed: "+e.GetType().Name);self.Complete("error","The scanner couldn't open. You can still enter the room code.");}
#endif
  }
  public static void Cancel() {
   if(!Busy)return;instance.completion=null;
#if UNITY_ANDROID && !UNITY_EDITOR
   try{using(var bridge=new AndroidJavaClass(Bridge))bridge.CallStatic("cancel");}catch(Exception e){Debug.LogWarning("Scanner close failed: "+e.GetType().Name);}
#endif
  }
  void Complete(string status,string value){var callback=completion;completion=null;callback?.Invoke(status,value);}
  void Update() {
#if UNITY_ANDROID && !UNITY_EDITOR
   if(completion==null||Time.unscaledTime<pollAt)return;pollAt=Time.unscaledTime+.15f;
   try {
    using(var bridge=new AndroidJavaClass(Bridge)) {
     string json=bridge.CallStatic<string>("takeResult");if(string.IsNullOrEmpty(json))return;
     var result=JsonUtility.FromJson<Result>(json);if(result!=null&&result.request==request)Complete(result.status,result.value);
    }
   }catch(Exception e){Debug.LogWarning("Scan Kit result failed: "+e.GetType().Name);var callback=completion;Cancel();callback?.Invoke("error","The scanner stopped. Try again or enter the room code.");}
#endif
  }
  void OnDestroy(){if(instance==this){Cancel();instance=null;}}
 }
}
