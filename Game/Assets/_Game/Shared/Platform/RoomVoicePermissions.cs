using System;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace WhatTheFish {
 public static class RoomVoicePermissions {
  public static bool MicrophoneAllowed {
   get {
#if UNITY_ANDROID && !UNITY_EDITOR
    return Permission.HasUserAuthorizedPermission(Permission.Microphone);
#else
    return true;
#endif
   }
  }
  public static async Task<bool> Request(){
#if UNITY_ANDROID && !UNITY_EDITOR
   if(!await Ask(Permission.Microphone))return false;
   using(var version=new AndroidJavaClass("android.os.Build$VERSION")){
    // Denying nearby-device access still permits the built-in phone microphone.
    if(version.GetStatic<int>("SDK_INT")>=31)await Ask("android.permission.BLUETOOTH_CONNECT");
   }
#else
   await Task.CompletedTask;
#endif
   return MicrophoneAllowed;
  }
#if UNITY_ANDROID && !UNITY_EDITOR
  static async Task<bool> Ask(string permission){
   if(Permission.HasUserAuthorizedPermission(permission))return true;
   var completion=new TaskCompletionSource<bool>();var callbacks=new PermissionCallbacks();
   void Granted(string value){if(value==permission)completion.TrySetResult(true);}
   void Denied(string value){if(value==permission)completion.TrySetResult(false);}
   callbacks.PermissionGranted+=Granted;callbacks.PermissionDenied+=Denied;
#pragma warning disable CS0618
   callbacks.PermissionDeniedAndDontAskAgain+=Denied;
#pragma warning restore CS0618
   try{
    Permission.RequestUserPermission(permission,callbacks);
    await Task.WhenAny(completion.Task,Task.Delay(120000));
    return Permission.HasUserAuthorizedPermission(permission);
   }finally{
    callbacks.PermissionGranted-=Granted;callbacks.PermissionDenied-=Denied;
#pragma warning disable CS0618
    callbacks.PermissionDeniedAndDontAskAgain-=Denied;
#pragma warning restore CS0618
   }
  }
#endif
  public static void OpenSettings(){
#if UNITY_ANDROID && !UNITY_EDITOR
   using var unity=new AndroidJavaClass("com.unity3d.player.UnityPlayer");
   using var activity=unity.GetStatic<AndroidJavaObject>("currentActivity");
   using var intent=new AndroidJavaObject("android.content.Intent","android.settings.APPLICATION_DETAILS_SETTINGS");
   using var uri=new AndroidJavaClass("android.net.Uri");
   using var package=uri.CallStatic<AndroidJavaObject>("parse","package:"+Application.identifier);
   intent.Call<AndroidJavaObject>("setData",package);activity.Call("startActivity",intent);
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
   Application.OpenURL("ms-settings:privacy-microphone");
#endif
  }
 }
}
