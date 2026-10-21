#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class AccountSignInReview:MonoBehaviour {
  string folder;bool failed;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-accountSignInReview");
   if(index>=0&&index+1<args.Length&&args.Contains("-guestProfile"))new GameObject("Account review").AddComponent<AccountSignInReview>().folder=args[index+1];
  }
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   double until=Time.realtimeSinceStartupAsDouble+120;
   while((!AppRoot.Instance||!AppRoot.Instance.Menu||!PackagedContentBoot.Ready)&&Time.realtimeSinceStartupAsDouble<until)yield return null;
   Check(PackagedContentBoot.Ready,"game content ready");if(!PackagedContentBoot.Ready){Finish();yield break;}
   PlayerIdentity.ClearAccount();PlayerIdentity.SaveName("Island Guest",out _);string guest=PlayerIdentity.GuestId;
   Check(PlayerIdentity.HasProfile,"guest works offline");
   PlayerIdentity.ActivateAccount("FixturePlayerA","Island Guest");
   Check(PlayerIdentity.DisplayName=="Island Guest"&&PlayerIdentity.AccountId=="FixturePlayerA","link preserves the guest name");
   PlayerIdentity.SaveName("Captain A",out _);
   PlayerIdentity.ActivateAccount("FixturePlayerB","");
   Check(PlayerIdentity.SavedName==""&&!PlayerIdentity.HasProfile,"new account cannot inherit another account's name");
   PlayerIdentity.SaveName("Captain B",out _);
   PlayerIdentity.ActivateAccount("FixturePlayerA","");
   Check(PlayerIdentity.SavedName=="Captain A","returning account loads only its own cached name");
   Check(PlayerIdentity.GuestId==guest,"provider switching does not reinterpret local guest ID as authentication");
   PlayerIdentity.ClearAccount();
   Check(PlayerIdentity.SavedName==""&&PlayerIdentity.GuestId==""&&PlayerIdentity.AccountId=="","sign-out removes active profile and guest copies");
   Check(new AccountCapabilities{huaweiDevice=true,googleAvailable=true}.Preferred==AccountProvider.Huawei,"Huawei device prefers Huawei even with GMS");
   Check(new AccountCapabilities{googleAvailable=true}.Preferred==AccountProvider.Google,"other Android with GMS prefers Google");
   Check(new AccountCapabilities{huaweiAvailable=true}.Preferred==AccountProvider.Huawei,"no GMS permits Huawei fallback");
   Check(AndroidAccountSignIn.Unavailable(AccountProvider.Google).Contains("Android"),"Windows cannot claim native sign-in");
   Check(PlayerAccount.Explain(new OperationCanceledException()).Contains("unchanged"),"cancel states profile preservation");
   AccountPanel.Open();yield return null;
   Check(AccountPanel.Visible,"account choices open in game");
   foreach(var provider in new[]{"Sign in with HUAWEI ID","Sign in with Google"}){
    var button=FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name==provider&&b.gameObject.activeInHierarchy);
    button.onClick.Invoke();yield return null;
    Check(!PlayerAccount.Busy&&PlayerIdentity.AccountId=="","unsupported "+provider+" keeps guest identity");
   }
   yield return Capture();
   FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name=="Close account choices").onClick.Invoke();yield return null;
   Check(!AccountPanel.Visible,"account dialog closes");
   PlayerIdentity.SaveName("Review Guest",out _);AppRoot.Instance.Show("profile");yield return new WaitForSeconds(.4f);
   Check(FindObjectsByType<Button>(FindObjectsSortMode.None).Any(b=>b.name=="Manage account"&&b.gameObject.activeInHierarchy),"profile page exposes account management");
   Finish();
  }
  IEnumerator Capture(){
   yield return new WaitForEndOfFrame();var camera=Camera.main;
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
   foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}
   Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var old=RenderTexture.active;var original=camera.targetTexture;
   var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
   try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();Check(image.GetPixels32().Where((pixel,index)=>index%997==0).Select(pixel=>pixel.r+pixel.g*256+pixel.b*65536).Distinct().Count()>30,"capture contains rendered colour");File.WriteAllBytes(Path.Combine(folder,"account-choices.png"),image.EncodeToPNG());}
   finally{Destroy(image);camera.targetTexture=original;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}Canvas.ForceUpdateCanvases();}
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"ACCOUNT_REVIEW_COMPLETE success="+!failed+"\n");Application.Quit(failed?2:0);}
 }
}
#endif
