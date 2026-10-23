#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Explicit opt-in smoke review of the shared rules view, not a gameplay bot.
 public sealed class IslandRulesReview:MonoBehaviour {
  AppRoot app;string folder;int checks;readonly Dictionary<string,(bool exists,int value)> saved=new();
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var args=Environment.GetCommandLineArgs();if(args.Contains("-islandRulesReview")&&args.Contains("-guestProfile")){DevelopmentProbe.TurnCommandActive=true;new GameObject("Island rules review").AddComponent<IslandRulesReview>();}}
  string Arg(string key){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:"";}
  void Check(bool ok,string what){File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+what+"\n");if(!ok){Restore();throw new InvalidOperationException(what);}checks++;}
  IEnumerator Until(Func<bool> condition,string what,float seconds=90){float end=Time.realtimeSinceStartup+seconds;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Check(condition(),what);}
  string Key(SportId sport)=>sport==SportId.Fishing?FishingTutorialUI.SeenKey:SportTutorialUI.SeenKey(sport);
  IslandTutorialUI Current(SportId sport)=>sport==SportId.Fishing?FishingHUD.Instance?.Tutorial:SportTutorialUI.Find(sport);
  Button Button(IslandTutorialUI ui,string suffix)=>ui.GetComponentsInChildren<Button>(true).Single(b=>b.name==ui.Sport+" tutorial "+suffix);
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
   var camera=Camera.main;var previousTarget=camera.targetTexture;var previousActive=RenderTexture.active;
   var target=new RenderTexture(Screen.width,Screen.height,24);var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
   try {
    foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=.5f;}camera.targetTexture=target;
    for(int pass=0;pass<3;pass++){foreach(var text in FindObjectsByType<Text>(FindObjectsSortMode.None))text.SetAllDirty();Canvas.ForceUpdateCanvases();camera.Render();}
    RenderTexture.active=target;image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();
    var colors=new HashSet<int>();var pixels=image.GetPixels32();for(int i=0;i<pixels.Length;i+=997){var c=pixels[i];colors.Add(c.r+c.g*256+c.b*65536);}
    Check(colors.Count>64,"rendered colour "+name);File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());
   } finally {camera.targetTexture=previousTarget;RenderTexture.active=previousActive;Destroy(target);Destroy(image);for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=RenderMode.ScreenSpaceOverlay;canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}Canvas.ForceUpdateCanvases();}
  }
  IEnumerator Start(){
   folder=Arg("-islandRulesReview");Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   foreach(var sport in new[]{SportId.Football,SportId.Basketball,SportId.Golf,SportId.Fishing}){string key=Key(sport);saved[key]=(PlayerPrefs.HasKey(key),PlayerPrefs.GetInt(key,0));PlayerPrefs.DeleteKey(key);}
   yield return Until(()=>AppRoot.Instance&&AppRoot.Instance.SafeFrame&&PlayerView.Instance&&PlayerView.Instance.active,"player ready",120);app=AppRoot.Instance;
   foreach(var sport in new[]{SportId.Football,SportId.Basketball,SportId.Golf,SportId.Fishing}){
    app.LeaveCourt();yield return null;app.SelectSport(sport);yield return Until(()=>!app.PendingSelection.HasValue&&app.SelectedSport==sport,"loaded "+sport);app.EnterOffline();
    yield return Until(()=>Current(sport),"created "+sport);var ui=Current(sport);if(ui is SportTutorialUI other)other.ReviewEntry=true;else ((FishingTutorialUI)ui).ReviewEntry=true;
    yield return Until(()=>ui.Visible,"automatic first visit "+sport,35);yield return new WaitForSecondsRealtime(.3f);
    Check(ui.Step==0&&PlayerView.Instance.InputBlocked,"page one blocks gameplay "+sport);Check(!Button(ui,"back").interactable,"back disabled on first page "+sport);
    for(int page=0;page<4;page++){
     Check(ui.Step==page&&!string.IsNullOrWhiteSpace(ui.Body),"correct page "+sport+" "+page);Canvas.ForceUpdateCanvases();Check(ui.FitsSafeFrame(),"safe frame "+sport+" "+page);
     Check(!ui.GetComponentsInChildren<Text>().Any(t=>t.enabled&&(t.text.Contains("KeyCode")||t.text.Contains("Press F")||t.text.Contains("Keyboard"))),"touch wording "+sport+" "+page);
     yield return Capture(sport+"-page-"+(page+1));
     if(page==1){Button(ui,"back").onClick.Invoke();Check(ui.Step==0,"back navigates "+sport);Button(ui,"next").onClick.Invoke();}
     Button(ui,"next").onClick.Invoke();yield return new WaitForSecondsRealtime(.2f);
    }
    Check(!ui.Visible&&PlayerPrefs.GetInt(Key(sport))==1&&!IslandTutorialUI.ModalOpen,"finish releases input and saves seen "+sport);
    yield return new WaitForSecondsRealtime(.6f);Check(!ui.Visible,"seen does not reopen "+sport);
    if(sport==SportId.Football)FootballHUD.Instance.OpenGuide();else if(sport==SportId.Basketball)BasketballHUD.Instance.OpenGuide();else if(sport==SportId.Fishing)ui.Open();else {GolfHUD.Instance.IslandMenu.Open();GolfHUD.Instance.IslandMenu.GetComponentsInChildren<Button>(true).Single(b=>b.name=="Golf guide").onClick.Invoke();}
    Check(ui.Visible&&ui.Step==0,"settings replays same view "+sport);Check(IslandTutorialUI.DismissTop()&&!ui.Visible,"back closes rules "+sport);
    string body=ui.Body;var embedded=IslandTutorialUI.CreateFor(app.SafeFrame,sport,true);yield return new WaitForSecondsRealtime(.25f);Check(embedded.Visible&&embedded.Body==body&&!IslandTutorialUI.ModalOpen,"same embedded rules without gameplay block "+sport);Destroy(embedded.gameObject);yield return null;
    ui.Open();foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,864)}){Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.3f);Canvas.ForceUpdateCanvases();Check(ui.FitsSafeFrame(),"landscape "+sport+" "+size);}
    Button(ui,"skip").onClick.Invoke();Check(!ui.Visible,"skip closes "+sport);Screen.SetResolution(1600,900,false);yield return new WaitForSecondsRealtime(.3f);
    // Rebuilding the HUD simulates leaving and returning without resetting seen.
    app.EnterOffline();yield return new WaitForSecondsRealtime(.7f);var again=Current(sport);if(again is SportTutorialUI s)s.ReviewEntry=true;else ((FishingTutorialUI)again).ReviewEntry=true;yield return new WaitForSecondsRealtime(.6f);Check(!again.Visible,"return visit stays closed "+sport);
   }
   Restore();File.AppendAllText(Path.Combine(folder,"results.txt"),"ISLAND_RULES_PASS checks="+checks+"\n");Application.Quit(0);
  }
  void Restore(){foreach(var pair in saved){if(pair.Value.exists)PlayerPrefs.SetInt(pair.Key,pair.Value.value);else PlayerPrefs.DeleteKey(pair.Key);}PlayerPrefs.Save();saved.Clear();}
  void OnDestroy(){Restore();}
 }
}
#endif
