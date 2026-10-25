#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed partial class LocalizationReview:MonoBehaviour {
  string folder;bool failed;AppRoot app;int checks;List<string> labels=new();
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-localizationReview");if(i>=0)new GameObject("Localization review").AddComponent<LocalizationReview>().folder=args[i+1];}
  void Check(bool ok,string message){checks++;failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+message+"\n");}
  IEnumerator Wait(Func<bool> ready,string message,float seconds=150){float end=Time.realtimeSinceStartup+seconds;while(!ready()&&Time.realtimeSinceStartup<end)yield return null;Check(ready(),message);}
  IEnumerator Settle(){yield return Wait(()=>!app.PendingSelection.HasValue&&!app.Menu.IsTransitioning,"view settled");yield return new WaitForSecondsRealtime(.5f);Canvas.ForceUpdateCanvases();}
  Button Button(string name)=>FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(b=>b.name==name&&b.gameObject.activeInHierarchy);
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   var args=Environment.GetCommandLineArgs();int expected=Array.IndexOf(args,"-localizationExpected");if(expected>=0){Check(GameLocalization.Code==args[expected+1],"saved language survives process restart");Finish();yield break;}
   yield return Wait(()=>StartupWelcome.Instance&&StartupWelcome.Instance.IsReady&&AppRoot.Instance&&AppRoot.Instance.Menu,"welcome and packaged content ready");app=AppRoot.Instance;var welcome=StartupWelcome.Instance;if(!app||!welcome){Finish();yield break;}
   foreach(GameLanguage language in Enum.GetValues(typeof(GameLanguage))){
    welcome.LanguageButton.onClick.Invoke();yield return null;var option=Button("Language option "+(int)language);Check(Button("Language option 2")?.GetComponentInChildren<Text>().text=="Bahasa Melayu","native Malay choice label");Check(option,"language choice exists "+language);if(option)option.onClick.Invoke();
    yield return new WaitForSecondsRealtime(.25f);Check(GameLocalization.Current==language&&!welcome.ModalOpen,"choice applies and closes "+language);yield return Capture(GameLocalization.Code+"-welcome");
    welcome.SettingsButton.onClick.Invoke();yield return null;
    Check(!Button("Language settings"),"welcome settings has no duplicate language picker");yield return Capture(GameLocalization.Code+"-welcome-settings");welcome.CloseModal();
    welcome.ContinueAsGuest();yield return null;welcome.GuestNameField.text="jh";welcome.GuestNameField.ActivateInputField();yield return null;
    welcome.GuestNameField.selectionAnchorPosition=0;welcome.GuestNameField.selectionFocusPosition=2;yield return new WaitForSecondsRealtime(.1f);
    yield return Capture(GameLocalization.Code+"-name-selection");CheckNameSelection(welcome.GuestNameField);welcome.CloseModal();
   }
   welcome.ContinueAsGuest();yield return null;welcome.GuestNameField.text="Island Buddy";welcome.GuestContinueButton.onClick.Invoke();yield return Settle();
   foreach(GameLanguage language in Enum.GetValues(typeof(GameLanguage))){
    GameLocalization.Select(language);string code=GameLocalization.Code;app.Show("home");yield return Settle();yield return Capture(code+"-home");
    foreach(string page in new[]{"create","join","settings","layouts","tutorials","rules","custom"}){app.Show(page);yield return Settle();yield return Capture(code+"-"+page);if(page=="settings")yield return SettingsLanguage(code+"-main");}
    foreach(SportId sport in Enum.GetValues(typeof(SportId))){
     app.SelectSport(sport);yield return Settle();app.Show("sport");yield return Settle();yield return Capture(code+"-"+sport+"-menu");
     app.EnterOffline();yield return new WaitForSecondsRealtime(1);while(IslandTutorialUI.DismissTop())yield return null;
     yield return Capture(code+"-"+sport+"-hud");
     if(sport==SportId.Golf){
      var golf=GolfMatchManager.Instance;Check(golf&&golf.StartMatch(),"golf starts with translated controls "+code);yield return new WaitForSecondsRealtime(.5f);
      var ranking=FindFirstObjectByType<GolfLeaderboardUI>();ranking.ToggleButton.onClick.Invoke();yield return Capture(code+"-golf-ranking");ranking.BackdropButton.onClick.Invoke();
      foreach(var kind in new[]{GolfEventKind.TropicalWind,GolfEventKind.StickyGreen,GolfEventKind.BounceParty,GolfEventKind.Tornado,GolfEventKind.CrabCrossing}){golf.ReviewEvent(kind);yield return new WaitForSecondsRealtime(.15f);yield return Capture(code+"-golf-"+kind);}
      var state=golf.Events.Snapshot;state.start=golf.Now+3;state.end=state.start+30;state.nextCycle=golf.Now+90;golf.Events.Receive(state);yield return Capture(code+"-golf-warning");yield return GolfSignLayouts(code);
     }
     yield return TutorialLayouts(sport,code);
     app.HandleBack();yield return new WaitForSecondsRealtime(.3f);yield return Capture(code+"-"+sport+"-settings");yield return SettingsLanguage(code+"-"+sport);
     app.LeaveCourt();yield return Settle();
    }
   }
   yield return ArtworkGallery();
   // A visible label survives a locale round trip, including its original face.
   app.Show("home");GameLocalization.Select(GameLanguage.English);yield return Settle();
   var original=app.Menu.Content.GetComponentsInChildren<LocalizedText>().Select(t=>(t,t.Source,t.text,t.font,t.fontSize)).ToArray();
   GameLocalization.Select(GameLanguage.SimplifiedChinese);yield return null;GameLocalization.Select(GameLanguage.English);yield return null;
   Check(original.All(x=>x.t.text==x.text&&x.t.Source==x.Source&&x.t.font==x.font&&x.t.fontSize==x.fontSize),"English text font and size restored exactly");
   var raw=CoveUI.Text(app.Menu.Content,"Ready",0,0,100,30);GameLocalization.UserContent(raw);GameLocalization.Select(GameLanguage.SimplifiedChinese);Check(raw.text=="Ready","user content preserved on locale change");Destroy(raw.gameObject);
   Check(GameLocalization.Translate("HOLE 3  •  12 m")=="第 3 洞 • 12 米","dynamic hole and distance template");
   Check(GameLocalization.Translate("Fill 5v5 with bots")=="用机器人补齐 5 对 5","dynamic team-size template");
   Check(GameLocalization.Translate("SAVED FOR ISLAND BUDDY")=="已为 ISLAND BUDDY 保存","embedded player name is preserved");
   Check(GameLocalization.Translate("Uncatalogued words")=="Uncatalogued words","seconds template does not consume ordinary words");
   Check(GameLocalization.Translate("HOLE 1 PAR 3\n<size=26>Bunker Bend</size>")=="第 1 洞 标准杆 3\n<size=26>沙坑弯道</size>","world signs preserve rich text and translate each line");
   Finish();
  }
  IEnumerator SettingsLanguage(string context){
   var entry=Button("Language settings");Check(entry,"settings language entry "+context);if(!entry)yield break;
   var parent=entry.transform.parent;var start=GameLocalization.Current;
   var art=parent.GetComponentsInChildren<Graphic>().Where(g=>!(g is Text)).Select(g=>(g,g.color,sprite:g is Image image?image.sprite:null)).ToArray();
   var labels=parent.GetComponentsInChildren<Text>().Select(t=>(t,t.color,t.fontStyle,t.fontSize,t.alignment)).ToArray();
   var volumes=new[]{MenuPreferences.MasterVolume,MenuPreferences.SFXVolume,MenuPreferences.UIVolume,MenuPreferences.AmbienceVolume};
   bool blocked=PlayerView.Instance&&PlayerView.Instance.ExplicitInputBlocked;
   entry.onClick.Invoke();yield return null;yield return Capture(context+"-language-picker-current");app.HandleBack();yield return null;
   Check(!Button("Close settings language")&&parent.gameObject.activeInHierarchy,"Back closes only the language picker "+context);
   foreach(GameLanguage language in Enum.GetValues(typeof(GameLanguage))){
    entry.onClick.Invoke();yield return null;var option=Button("Settings language option "+(int)language);Check(option,"settings offers "+language+" "+context);
    if(language==GameLanguage.SimplifiedChinese)yield return Capture(context+"-language-picker");
    if(option)option.onClick.Invoke();yield return new WaitForSecondsRealtime(.15f);
    Check(GameLocalization.Current==language&&!Button("Close settings language")&&parent.gameObject.activeInHierarchy,"settings selection stays in settings "+context+" "+language);
    Check(art.All(v=>v.g&&v.g.color==v.color&&(!(v.g is Image img)||img.sprite==v.sprite)),"settings colors and original icons unchanged "+context+" "+language);
    Check(labels.All(v=>v.t&&v.t.color==v.color&&v.t.fontStyle==v.fontStyle&&v.t.fontSize==v.fontSize&&v.t.alignment==v.alignment),"text color weight size alignment unchanged "+context+" "+language);
    Check(blocked==(PlayerView.Instance&&PlayerView.Instance.ExplicitInputBlocked),"language keeps gameplay input ownership "+context);
   }
   entry.onClick.Invoke();yield return null;Button("Settings language option "+(int)start)?.onClick.Invoke();yield return null;
   Check(volumes.SequenceEqual(new[]{MenuPreferences.MasterVolume,MenuPreferences.SFXVolume,MenuPreferences.UIVolume,MenuPreferences.AmbienceVolume}),"language preserves audio values "+context);
  }
  IEnumerator Capture(string name){
   yield return null;Canvas.ForceUpdateCanvases();
   foreach(var t in FindObjectsByType<Text>(FindObjectsSortMode.None)){
    if(!t.enabled||!t.gameObject.activeInHierarchy||string.IsNullOrEmpty(t.text))continue;
    var raw=t is LocalizedText localized&&localized.UserContent;string source=GameLocalization.Source(t);
    labels.Add(name+"\t"+t.name+"\t"+source.Replace("\n","\\n")+"\t"+t.text.Replace("\n","\\n")+"\t"+raw);
    if(t.text.Any(c=>c>=0x4e00&&c<=0x9fff))Check(t.font&&t.text.Where(c=>c>=0x4e00&&c<=0x9fff).All(c=>t.font.HasCharacter(c)),"Chinese glyphs "+name+" "+t.name);
   }
   var native=FindObjectsByType<LocalizedText>(FindObjectsSortMode.None).Where(t=>t.isActiveAndEnabled&&!string.IsNullOrWhiteSpace(t.text)&&!t.GetComponents<BaseMeshEffect>().Any(e=>e.IsActive())).ToArray();
   Check(native.All(t=>t.canvasRenderer.GetMesh()&&t.canvasRenderer.GetMesh().vertexCount==t.cachedTextGenerator.verts.Count),"each text mesh contains its complete own glyphs "+name);
   Check(!FindObjectsByType<Transform>(FindObjectsSortMode.None).Any(t=>t.name=="Translated sign-in"||t.name=="Localized artwork caption"||t.name=="Translated utility caption"),"no replacement plates or icon overlays "+name);
   foreach(var a in FindObjectsByType<LocalizedArtwork>(FindObjectsSortMode.None))if(a.Caption&&a.Caption.isActiveAndEnabled)Check(((LocalizedText)a.Caption).HasUnclippedText(),"complete artwork caption "+name+" "+GameLocalization.Source(a.Caption));
   AuditLayout(name);
   yield return new WaitForEndOfFrame();var camera=Camera.main;if(!camera)yield break;
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).Select(c=>(c,c.worldCamera,c.planeDistance)).ToArray();
   foreach(var v in canvases){v.c.renderMode=RenderMode.ScreenSpaceCamera;v.c.worldCamera=camera;v.c.planeDistance=.5f;}
   var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var before=camera.targetTexture;var active=RenderTexture.active;camera.targetTexture=target;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
   var pic=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);pic.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);pic.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),pic.EncodeToPNG());Destroy(pic);
   camera.targetTexture=before;RenderTexture.active=active;RenderTexture.ReleaseTemporary(target);foreach(var v in canvases){v.c.renderMode=RenderMode.ScreenSpaceOverlay;v.c.worldCamera=v.worldCamera;v.c.planeDistance=v.planeDistance;}
  }
  void Finish(){File.WriteAllLines(Path.Combine(folder,"labels.tsv"),labels);File.AppendAllText(Path.Combine(folder,"results.txt"),"LOCALIZATION_COMPLETE success="+!failed+" checks="+checks+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
