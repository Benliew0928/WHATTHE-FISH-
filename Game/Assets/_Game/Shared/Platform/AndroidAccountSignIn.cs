using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace WhatTheFish {
 public enum AccountProvider { Huawei,Google }
 [Serializable,Preserve] public sealed class AccountCapabilities {
  public bool huaweiDevice,huaweiAvailable,googleAvailable;
  public string googleClientId="",huaweiEndpoint="";
  public AccountProvider Preferred=>huaweiDevice||!googleAvailable?AccountProvider.Huawei:AccountProvider.Google;
 }
 public sealed class AndroidAccountSignIn:MonoBehaviour {
  const string Bridge="com.umpsa.whatthefish.accounts.AccountBridge";
  static AndroidAccountSignIn instance;static AccountCapabilities config;
  TaskCompletionSource<string> pending;int request;float nextPoll,deadline;
  [Serializable,Preserve]sealed class Result {public int request;public string status,token,message;}
  public static bool Pending=>instance&&instance.pending!=null;
  public static bool Supported=>Application.platform==RuntimePlatform.Android;
  public static AccountCapabilities Capabilities {
   get {
    if(config!=null)return config;
    config=new AccountCapabilities();
#if UNITY_ANDROID && !UNITY_EDITOR
    try{WithActivity((bridge,activity)=>{config=JsonUtility.FromJson<AccountCapabilities>(bridge.CallStatic<string>("configuration",activity))??config;});}
    catch(Exception){/* Missing plugin/configuration is shown as unavailable, never as a fake sign-in. */}
#endif
    return config;
   }
  }
  static AndroidAccountSignIn Instance {
   get {if(!instance){var go=new GameObject("Android account sign-in");DontDestroyOnLoad(go);instance=go.AddComponent<AndroidAccountSignIn>();}return instance;}
  }
  public static string Unavailable(AccountProvider provider) {
   if(!Supported)return "Account sign-in is unavailable on this device. You can continue as a guest.";
   if(provider==AccountProvider.Google) {
    if(!Capabilities.googleAvailable)return "Google Play services are unavailable on this phone. Use Huawei or continue as a guest.";
    if(string.IsNullOrEmpty(Capabilities.googleClientId))return "Google sign-in setup is not finished for this build. You can continue as a guest.";
   }else if(string.IsNullOrEmpty(Capabilities.huaweiEndpoint))return "Huawei sign-in setup is not finished for this build. You can continue as a guest.";
   return "";
  }
  public static Task<string> SignIn(AccountProvider provider) {
   string unavailable=Unavailable(provider);if(unavailable.Length>0)throw new InvalidOperationException(unavailable);
   var self=Instance;if(self.pending!=null)throw new InvalidOperationException("An account screen is already open.");
   self.pending=new TaskCompletionSource<string>();var task=self.pending.Task;self.request++;self.deadline=Time.realtimeSinceStartup+190;
#if UNITY_ANDROID && !UNITY_EDITOR
   try{WithActivity((bridge,activity)=>bridge.CallStatic("start",activity,self.request,provider==AccountProvider.Huawei?"huawei":"google",Capabilities.googleClientId));}
   catch(Exception){self.Fail(new InvalidOperationException("The phone's account screen could not open."));}
#endif
   return task;
  }
  public static void SignOutNative() {
#if UNITY_ANDROID && !UNITY_EDITOR
   try{WithActivity((bridge,activity)=>bridge.CallStatic("signOut",activity));}catch(Exception){ }
#endif
  }
  public static void Cancel() {
   if(!instance||instance.pending==null)return;
#if UNITY_ANDROID && !UNITY_EDITOR
   try{using(var bridge=new AndroidJavaClass(Bridge))bridge.CallStatic("cancel",instance.request);}catch(Exception){ }
#endif
   instance.Fail(new OperationCanceledException());
  }
  void Fail(Exception error){var promise=pending;pending=null;promise?.TrySetException(error);}
  void Update() {
   if(pending==null)return;
   if(Time.realtimeSinceStartup>deadline){Cancel();return;}
#if UNITY_ANDROID && !UNITY_EDITOR
   if(Time.unscaledTime<nextPoll)return;nextPoll=Time.unscaledTime+.15f;
   try{
    using(var bridge=new AndroidJavaClass(Bridge)){
     string json=bridge.CallStatic<string>("takeResult");if(string.IsNullOrEmpty(json))return;
     var result=JsonUtility.FromJson<Result>(json);if(result==null||result.request!=request)return;
     if(result.status=="ok"&&!string.IsNullOrEmpty(result.token)){var promise=pending;pending=null;promise.TrySetResult(result.token);}
     else if(result.status=="cancel")Fail(new OperationCanceledException());
     else Fail(new InvalidOperationException(string.IsNullOrEmpty(result.message)?"Sign-in failed. Try again.":result.message));
    }
   }catch(Exception){Fail(new InvalidOperationException("The account screen stopped. Please try again."));}
#endif
  }
#if UNITY_ANDROID && !UNITY_EDITOR
  static void WithActivity(Action<AndroidJavaClass,AndroidJavaObject> action) {
   using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
   using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))
   using(var bridge=new AndroidJavaClass(Bridge))action(bridge,activity);
  }
#endif
  void OnDestroy(){if(instance==this){Cancel();instance=null;}}
 }
}
