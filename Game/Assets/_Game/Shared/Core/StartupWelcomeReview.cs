#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class StartupWelcomeReview:MonoBehaviour {
  string folder;bool failed;StartupWelcome welcome;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-startupWelcomeReview");if(index>=0&&index+1<args.Length)new GameObject("Startup welcome review").AddComponent<StartupWelcomeReview>().folder=args[index+1];}
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Until(Func<bool> condition,string label,float timeout=120){double end=Time.realtimeSinceStartupAsDouble+timeout;while(!condition()&&Time.realtimeSinceStartupAsDouble<end)yield return null;Check(condition(),label);}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   yield return Until(()=>StartupWelcome.Instance,"startup view created before game content");welcome=StartupWelcome.Instance;if(!welcome){Finish();yield break;}
   Check(welcome.ArtworkLoaded,"approved artwork available without scene bundle");
   bool fault=Environment.GetCommandLineArgs().Contains("-welcomeFailOnce");
   if(fault){
    yield return Until(()=>welcome.HasError,"preparation failure presents retry",20);if(!welcome.HasError){Finish();yield break;}
    Check(!welcome.IsReady&&!welcome.GuestButton.interactable&&!welcome.HuaweiButton.interactable,"failure cannot enter unloaded game");
    var retryBounds=Bounds((RectTransform)welcome.RetryButton.transform);Check(retryBounds.height>=44,"retry keeps a full touch target");
    foreach(var text in welcome.Design.GetComponentsInChildren<Text>().Where(t=>t.transform.parent==welcome.Design&&t.gameObject.activeInHierarchy))Check(!Bounds(text.rectTransform).Overlaps(retryBounds),"retry does not overlap sign text: "+text.text);
    yield return Capture("retry");Click(welcome.RetryButton);yield return null;
   }
   bool bounded=true,monotonic=true,blocked=true,captured=false;float last=0;double deadline=Time.realtimeSinceStartupAsDouble+150;
   while(!welcome.IsReady&&Time.realtimeSinceStartupAsDouble<deadline){
    bounded&=welcome.Progress<1;monotonic&=welcome.Progress+.00001f>=last;last=welcome.Progress;
    blocked&=!welcome.GuestButton.interactable&&!welcome.HuaweiButton.interactable;
    if(!captured&&welcome.Progress>.02f&&welcome.Progress<.95f){yield return Capture("loading");captured=true;}
    yield return null;
   }
   Check(welcome.IsReady&&PackagedContentBoot.Ready,"real packaged content and existing menu are ready");if(!welcome.IsReady){Finish();yield break;}
   Check(bounded&&monotonic,"progress is monotonic and stays below 100 until ready");Check(blocked,"login entry remains unavailable during preparation");
   Check(welcome.Progress==1&&StartupWelcome.Blocking,"loaded game waits on welcome screen for user entry");
   bool expectedWarm=Environment.GetCommandLineArgs().Contains("-expectWarmWelcome");Check(PackagedContentBoot.CacheHit==expectedWarm,expectedWarm?"warm content cache path":"cold content cache path");
   Check(welcome.GuestButton.interactable&&welcome.HuaweiButton.interactable,"entry choices become interactive after load");
   Check(!AppRoot.Instance.Menu.Content.GetComponentsInChildren<Button>().Any(b=>b.IsInteractable()),"underlying menu cannot receive keyboard or pointer input");
   yield return new WaitForSecondsRealtime(1);Check(!welcome.LoadingPanel.gameObject.activeSelf,"completed progress clears without changing page");
   LayoutChecks();yield return Capture("ready");
   Check(!welcome.Design.Find("Blank sign during preparation").gameObject.activeSelf,"ready state restores the exact approved sign artwork");
   foreach(var button in new[]{welcome.HuaweiButton,welcome.LanguageButton,welcome.SettingsButton,welcome.InfoButton}){
    Click(button,button!=welcome.HuaweiButton);yield return null;Check(welcome.ModalOpen,button.name+" opens its real UI placeholder");
    Check(!welcome.GuestButton.interactable&&!welcome.LanguageButton.interactable,"dialog blocks underlying controls");
    welcome.ContinueAsGuest();Check(StartupWelcome.Blocking,"guest cannot bypass an open modal");
    if(button==welcome.HuaweiButton)yield return Capture("huawei-placeholder");
    var close=welcome.Canvas.GetComponentsInChildren<Button>().First(b=>b.name=="Close welcome dialog");Click(close);yield return null;Check(!welcome.ModalOpen,"dialog closes without signing in");
   }
   // Device cutout simulation affects the production layout, not the art itself.
   welcome.ReviewSafeArea=new Rect(54,18,Screen.width-108,Screen.height-36);welcome.Layout();yield return null;LayoutChecks();yield return Capture("safe-area");welcome.ReviewSafeArea=null;welcome.Layout();yield return null;
   bool returning=PlayerIdentity.HasProfile;
   Check(returning==expectedWarm,"guest profile persists between separate player launches");
   Check(!PlayerIdentity.ValidateName("   ",out _,out _)&&!PlayerIdentity.ValidateName("---",out _,out _)&&!PlayerIdentity.ValidateName("<b>Name</b>",out _,out _)&&!PlayerIdentity.ValidateName(new string('A',17),out _,out _),"blank markup punctuation-only and overlength names rejected");
   Check(PlayerIdentity.ValidateName("  Island Buddy  ",out var trimmed,out _)&&trimmed=="Island Buddy"&&PlayerIdentity.ValidateName("A_2-B",out _,out _),"trimmed names and supported punctuation accepted");
   if(!returning){PlayerPrefs.SetString(PlayerIdentity.LegacyNameKey,"Old Angler");Check(PlayerIdentity.SavedName=="Old Angler","existing fishing name is available for migration");PlayerPrefs.DeleteKey(PlayerIdentity.LegacyNameKey);}
   string priorId=PlayerIdentity.GuestId;
   Click(welcome.GuestButton);yield return null;Check(welcome.ModalOpen&&welcome.GuestNameField&&StartupWelcome.Blocking,"guest choice opens name dialog and retains input gate");
   Check(welcome.GuestNameField.text==(returning?"Island Buddy":""),"returning guest name is prefilled");
   welcome.GuestNameField.text="   ";Check(!welcome.GuestContinueButton.interactable,"blank name cannot continue");
   welcome.GuestNameField.text="<b>Name";Check(!welcome.GuestContinueButton.interactable,"markup name cannot continue");
   welcome.GuestNameField.text="Unsaved";welcome.CloseModal();yield return null;Check(PlayerIdentity.GuestId==priorId&&PlayerIdentity.SavedName==(returning?"Island Buddy":""),"cancel does not create or rename a profile");
   Click(welcome.GuestButton);yield return null;welcome.GuestNameField.text="Island Buddy";
   LayoutChecks();yield return Capture("guest-name");
   Check(Contains(welcome.SafeArea,Bounds(welcome.GuestNameField.GetComponent<RectTransform>()))&&Contains(welcome.SafeArea,Bounds(welcome.GuestContinueButton.GetComponent<RectTransform>())),"guest name controls fit inside safe area");
   Click(welcome.GuestContinueButton);yield return Until(()=>!StartupWelcome.Blocking,"named guest entry dismisses startup gate",10);
   Check(PlayerIdentity.HasProfile&&PlayerIdentity.DisplayName=="Island Buddy"&&(!returning||PlayerIdentity.GuestId==priorId),"guest ID and display name persist without replacing returning identity");
   File.WriteAllText(Path.Combine(folder,"guest-id.txt"),PlayerIdentity.GuestId);
   var app=AppRoot.Instance;Check(app&&app.Menu&&app.Menu.Page=="home"&&!app.Exploring,"guest opens the existing home menu");
   Check(app&&!app.rooms.Connected,"guest creation works without an online session");yield return new WaitForSecondsRealtime(.6f);yield return Capture("main-menu");
   Click(app.Menu.Content.GetComponentsInChildren<Button>().First(b=>b.name=="Player profile"));yield return new WaitForSecondsRealtime(.4f);
   var profileName=app.Menu.Content.GetComponentsInChildren<InputField>().First(f=>f.name=="Profile name");profileName.text="  Island Buddy  ";
   Click(app.Menu.Content.GetComponentsInChildren<Button>().First(b=>b.name=="Save player name"));yield return null;
   Check(profileName.text=="Island Buddy"&&PlayerIdentity.GuestId==File.ReadAllText(Path.Combine(folder,"guest-id.txt")),"profile editor saves trimmed name without replacing guest ID");yield return Capture("profile");
   Click(app.Menu.Content.GetComponentsInChildren<Button>().First(b=>b.name=="Done profile"));yield return new WaitForSecondsRealtime(.4f);
   app.EnterOffline();yield return new WaitForSecondsRealtime(.8f);Check(app.Exploring&&app.view.active&&!app.view.InputBlocked,"normal gameplay input restored after guest entry");
   Finish();
  }
  void LayoutChecks(){
   Canvas.ForceUpdateCanvases();
   foreach(var button in new[]{welcome.GuestButton,welcome.HuaweiButton,welcome.LanguageButton,welcome.SettingsButton,welcome.InfoButton}){
    var bounds=Bounds((RectTransform)button.transform);Check(Contains(welcome.SafeArea,bounds),button.name+" inside safe area");Check(bounds.width>=44&&bounds.height>=44,button.name+" keeps touch target");
   }
   var progress=welcome.LoadingPanel;var p=progress.anchoredPosition;var artLegs=new Rect(385,570,340,230);var local=new Rect(p.x,-p.y,progress.rect.width,progress.rect.height);
   Check(local.xMin>artLegs.xMax&&local.yMin>HeightFraction(.7f)&&!local.Overlaps(artLegs),"progress is low beside the character legs without covering them");
   foreach(var t in welcome.Canvas.GetComponentsInChildren<Text>())if(t.isActiveAndEnabled&&t.text.Length>0){Check(t.preferredHeight<=t.rectTransform.rect.height+2,"text fits: "+t.text.Replace('\n',' '));Check(t.cachedTextGenerator.characterCountVisible>0,"text renders: "+t.text.Replace('\n',' '));}
  }
  static float HeightFraction(float fraction)=>StartupWelcome.Height*fraction;
  static Rect Bounds(RectTransform r){var corners=new Vector3[4];r.GetWorldCorners(corners);return Rect.MinMaxRect(corners.Min(p=>p.x),corners.Min(p=>p.y),corners.Max(p=>p.x),corners.Max(p=>p.y));}
  static bool Contains(Rect outer,Rect inner)=>inner.xMin>=outer.xMin-1&&inner.xMax<=outer.xMax+1&&inner.yMin>=outer.yMin-1&&inner.yMax<=outer.yMax+1;
  void Click(Button button,bool labelArea=false){
   Canvas.ForceUpdateCanvases();var bounds=Bounds((RectTransform)button.transform);var point=labelArea?new Vector2(bounds.center.x,bounds.yMin+10):bounds.center;
   var pointer=new PointerEventData(EventSystem.current){position=point,button=PointerEventData.InputButton.Left};
   var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);GameObject target=hits.Count>0?ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject):null;
   Check(button.IsInteractable()&&target==button.gameObject,"pointer reaches "+button.name+(labelArea?" label":""));if(target!=button.gameObject)return;
   ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
  }
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();var camera=Camera.main;GameObject temporary=null;
   if(!camera){temporary=new GameObject("Welcome capture camera");camera=temporary.AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;}
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
   foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}
   Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var old=RenderTexture.active;var original=camera.targetTexture;
   var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
   // A prime stride avoids repeatedly sampling the same six columns at 1200px.
   try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();Check(image.GetPixels32().Where((pixel,index)=>index%997==0).Select(pixel=>pixel.r+pixel.g*256+pixel.b*65536).Distinct().Count()>30,"capture contains rendered colour: "+name);File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());}
   finally{Destroy(image);camera.targetTexture=original;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}Canvas.ForceUpdateCanvases();if(temporary)Destroy(temporary);}
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"STARTUP_WELCOME_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
