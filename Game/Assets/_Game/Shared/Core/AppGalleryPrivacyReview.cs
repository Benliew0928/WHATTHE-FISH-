#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class AppGalleryPrivacyReview:MonoBehaviour {
  string folder;bool failed;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-privacyReview");
   if(i>=0&&i+1<args.Length&&args.Contains("-guestProfile")&&args.Contains("-authProfile"))new GameObject("Privacy review").AddComponent<AppGalleryPrivacyReview>().folder=args[i+1];
  }
  void Check(bool value,string label){failed|=!value;File.AppendAllText(Path.Combine(folder,"results.txt"),(value?"PASS ":"FAIL ")+label+"\n");}
  Button Find(string name)=>FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name==name&&b.gameObject.activeInHierarchy);
  IEnumerator Start(){
   Directory.CreateDirectory(folder);double until=Time.realtimeSinceStartupAsDouble+150;
   while((!AppRoot.Instance||!AppRoot.Instance.Menu||!PackagedContentBoot.Ready)&&Time.realtimeSinceStartupAsDouble<until)yield return null;
   Check(PackagedContentBoot.Ready,"game ready");if(!PackagedContentBoot.Ready){Finish();yield break;}
   PlayerIdentity.ClearAccount();PlayerIdentity.SaveName("Privacy Fixture",out _);
   AccountPanel.Open();yield return null;Find("Privacy and account data").onClick.Invoke();yield return null;
   Check(PrivacyDataPanel.Visible,"privacy reachable from accounts without sign-in");
   Check(!Find("Read published privacy policy").interactable,"missing policy URL is not a fake link");
   Check(!PlayerAccount.DeletionPending,"opening privacy does not initiate deletion");
   yield return Capture("privacy-overview.png");
   Find("Delete game account").onClick.Invoke();yield return null;
   var field=FindObjectsByType<InputField>(FindObjectsSortMode.None).Single(f=>f.name=="Deletion confirmation");
   Check(field.gameObject.activeInHierarchy,"deletion requires typed confirmation");
   Find("Delete game account").onClick.Invoke();yield return null;
   Check(PlayerIdentity.HasProfile&&!PlayerAccount.DeletionPending,"empty confirmation cannot delete");
   yield return Capture("privacy-confirm.png");
   Find("Close privacy").onClick.Invoke();yield return null;
   Check(!field.gameObject.activeSelf&&PlayerIdentity.HasProfile,"back cancels confirmation and preserves profile");
   // No production request or cloud identity is created/deleted by this review.
   Find("Close privacy").onClick.Invoke();yield return null;Check(!PrivacyDataPanel.Visible,"privacy closes");
   Find("Close account choices").onClick.Invoke();yield return null;
   AppRoot.Instance.Show("settings");yield return null;Find("Privacy and data").onClick.Invoke();yield return null;
   Check(PrivacyDataPanel.Visible,"privacy reachable from settings");
   Check(!PlayerAccount.Deleting&&!PlayerAccount.DeletionPending,"review never deletes or signs into cloud accounts");
   PlayerProfiles.SaveLocalName("Privacy Fixture");
   var localDelete=PlayerAccount.DeleteGameAccount();until=Time.realtimeSinceStartupAsDouble+45;
   while(!localDelete.IsCompleted&&Time.realtimeSinceStartupAsDouble<until)yield return null;
   Check(localDelete.IsCompleted&&!localDelete.IsFaulted,"isolated offline guest deletion completes");
   if(localDelete.IsFaulted)File.AppendAllText(Path.Combine(folder,"results.txt"),localDelete.Exception.ToString());
   Check(!PlayerIdentity.HasProfile&&!PlayerAccount.DeletionPending,"offline guest deletion clears identity and journal");
   Check(!Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn,"offline deletion never creates a cloud identity");
   Finish();
  }
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();var camera=Camera.main;
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
   foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}
   Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var old=RenderTexture.active;var original=camera.targetTexture;var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
   try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name),image.EncodeToPNG());}
   finally{Destroy(image);camera.targetTexture=original;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}}
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"PRIVACY_REVIEW_COMPLETE success="+!failed+"\n");Application.Quit(failed?2:0);}
 }
}
#endif
