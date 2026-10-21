using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace WhatTheFish {
 public static class ProfilePhoto {
  [Serializable,Preserve] sealed class Result {public string error,photo;}
  public static bool Busy {get;private set;}
  public static async Task<string> Choose(bool camera) {
#if UNITY_ANDROID && !UNITY_EDITOR
   if(Busy)throw new InvalidOperationException("The photo picker is already open.");
   Busy=true;string id=Guid.NewGuid().ToString("N");
   using var bridge=new AndroidJavaClass("com.umpsa.whatthefish.profiles.PhotoBridge");
   try {
    using var unity=new AndroidJavaClass("com.unity3d.player.UnityPlayer");using var activity=unity.GetStatic<AndroidJavaObject>("currentActivity");
    bridge.CallStatic("start",activity,id,camera);float end=Time.realtimeSinceStartup+300;
    while(Time.realtimeSinceStartup<end){
     await Task.Delay(150);string text=bridge.CallStatic<string>("poll");if(string.IsNullOrEmpty(text))continue;
     var result=JsonUtility.FromJson<Result>(text);
     if(result.error=="cancelled")throw new OperationCanceledException();
     if(!string.IsNullOrEmpty(result.error))throw new InvalidOperationException(result.error=="permission"?"Allow camera access in Android Settings to take a picture.":"Could not open this picture. Choose a JPEG or PNG under 20 MB, or try another camera app.");
     if(string.IsNullOrEmpty(result.photo)||result.photo.Length!=65536)throw new InvalidOperationException("Invalid picture.");
     return result.photo;
    }
    throw new OperationCanceledException();
   }finally{bridge.CallStatic("cancel",id);Busy=false;}
#else
   await Task.Yield();throw new InvalidOperationException("Camera and photo library are available in the Android game. You can choose a character avatar here.");
#endif
  }
 }
}
