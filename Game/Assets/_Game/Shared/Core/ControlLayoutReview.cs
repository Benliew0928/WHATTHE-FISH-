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
 public sealed class ControlLayoutReview:MonoBehaviour {
  string folder;int checks;AppRoot app;ControlLayoutEditor layout;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){
   var args=Environment.GetCommandLineArgs();
   if(args.Contains("-layoutReview")&&args.Contains("-guestProfile"))new GameObject("Control layout review").AddComponent<ControlLayoutReview>();
  }
  string Arg(string flag){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,flag);return i>=0&&i+1<a.Length?a[i+1]:"";}
  void Check(bool pass,string name){File.AppendAllText(Path.Combine(folder,"results.txt"),(pass?"PASS ":"FAIL ")+name+"\n");if(!pass)throw new Exception(name);checks++;}
  IEnumerator Until(Func<bool> ready,string name,float seconds=100){float end=Time.realtimeSinceStartup+seconds;while(!ready()&&Time.realtimeSinceStartup<end)yield return null;Check(ready(),name);}
  Button Find(string name)=>FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.name==name&&b.gameObject.activeInHierarchy);
  Vector2 ScreenPoint(RectTransform rect)=>RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
  GameObject Top(Vector2 point){var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},hits);return hits.Count>0?hits[0].gameObject:null;}
  void Click(string name){
   var button=Find(name);Canvas.ForceUpdateCanvases();var point=ScreenPoint((RectTransform)button.transform);var top=Top(point);
   bool hit=button.IsInteractable()&&top&&top.GetComponentInParent<Button>()==button;
   if(!hit){File.AppendAllText(Path.Combine(folder,"results.txt"),"RAYCAST "+name+" hit="+(top?top.name:"none")+"\n");Capture("failed-click");}
   Check(hit,"CLICK_"+name);
   var data=new PointerEventData(EventSystem.current){pointerId=73,position=point,button=PointerEventData.InputButton.Left};
   ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerDownHandler);
   ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerUpHandler);
   ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerClickHandler);
  }
  void OpenSettings(){
   if(app.SelectedSport==SportId.Basketball)BasketballHUD.Instance.ShowSettings();
   else if(app.SelectedSport==SportId.Football)FootballHUD.Instance.ShowSettings();
   else if(app.SelectedSport==SportId.Golf)GolfHUD.Instance.IslandMenu.Open();
   else FishingHUD.Instance.IslandMenu.Open();
  }
  void CloseSettings(){
   if(app.SelectedSport==SportId.Basketball)BasketballHUD.Instance.CloseMenu();
   else if(app.SelectedSport==SportId.Football)FootballHUD.Instance.CloseMenu();
   else if(app.SelectedSport==SportId.Golf)GolfHUD.Instance.IslandMenu.Close();
   else FishingHUD.Instance.IslandMenu.Close();
  }
  IEnumerator Start(){
   folder=Arg("-layoutReview");Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   yield return Until(()=>DevelopmentProbe.ReadyToReviewSport&&AppRoot.Instance.view.active,"ACTUAL_SPORT_READY");
   app=AppRoot.Instance;
   yield return new WaitForSecondsRealtime(3);
   layout=FindObjectsByType<ControlLayoutEditor>(FindObjectsSortMode.None).First(l=>!l.Preview&&l.Sport==app.SelectedSport);
   Check(layout.Controls.Count==(app.SelectedSport==SportId.Basketball?5:app.SelectedSport==SportId.Football?6:app.SelectedSport==SportId.Golf?7:6),"ALL_SPORT_ACTION_BINDINGS");
   string profile=PlayerIdentity.ProfileStorageKey;
   if(Environment.GetCommandLineArgs().Contains("-layoutRestart")){
    Check((layout.NormalizedCenter(0)-new Vector2(.25f,.28f)).magnitude<.003f&&Mathf.Abs(layout.Size(0)-1.3f)<.001f,"RESTART_RESTORES_SAVED_LAYOUT");
    foreach(SportId sport in Enum.GetValues(typeof(SportId)))PlayerPrefs.DeleteKey(ControlLayouts.Key(profile,sport));PlayerPrefs.Save();
    Complete();yield break;
   }
   Check(ControlLayouts.Load(profile,app.SelectedSport).Count==0,"NEW_IDENTITY_STARTS_DEFAULT");
   app.view.stick.value=Vector2.one;OpenSettings();yield return null;Click("Layout");yield return null;
   Check(layout.Editing&&layout.Sport==app.SelectedSport,"IN_GAME_LAYOUT_AUTOMATIC_SPORT");
   Check(app.view.InputBlocked&&app.view.ReadCommand().move==Vector2.zero&&app.view.stick.value==Vector2.zero,"EDITOR_BLOCKS_GAMEPLAY");
   Check(layout.Controls.All(c=>c.proxy),"CONTEXTUAL_CONTROLS_ALL_EDITABLE");
   if(layout.Sport==SportId.Fishing){
    Check(layout.Controls.Any(c=>c.id=="ability")&&!layout.Controls.Any(c=>c.id=="supply"||c.id=="salvage"),"ROUND_SKILL_LAYOUT_REPLACES_RETIRED_PICKUPS");
    int left=layout.Controls.ToList().FindIndex(c=>c.id=="left");layout.Select(left);
    Check(layout.Controls[left].proxy.gameObject.activeSelf&&!layout.Controls[0].proxy.gameObject.activeSelf,"REEL_CONTEXT_HAS_NO_STACKED_JOYSTICK");layout.Select(0);
    Check(layout.Controls[0].proxy.gameObject.activeSelf&&!layout.Controls[left].proxy.gameObject.activeSelf,"MOVE_CONTEXT_HAS_NO_STACKED_REEL");
   }
   VerifyArtwork(layout);Capture("actual-button-art");
   Click("Choose control");yield return null;Click("Choose "+layout.Controls[layout.Controls.Count-1].id);
   Check(layout.Selected==layout.Controls.Count-1,"CONTROL_PICKER_REACHES_CONTEXTUAL_ACTIONS");
   layout.Select(0);
   var defaultPosition=layout.NormalizedCenter(0);
   var drag=layout.Controls[0].proxy.GetComponent<ControlLayoutDrag>();
   var point=ScreenPoint(layout.Controls[0].proxy);var data=new PointerEventData(EventSystem.current){pointerId=41,button=PointerEventData.InputButton.Left,position=point};
   Check(Top(point).GetComponentInParent<ControlLayoutDrag>()==drag,"MOVE_PROXY_RAYCAST");
   drag.OnPointerDown(data);data.position+=new Vector2(90,75);drag.OnDrag(data);drag.OnPointerUp(data);
   Check(Vector2.Distance(defaultPosition,layout.NormalizedCenter(0))>.04f,"POINTER_DRAG_CHANGES_POSITION");
   layout.ResizeSelected(1.4f);yield return null;Check(Mathf.Abs(layout.Size(0)-1.4f)<.001f,"SIZE_SLIDER_MODEL");VerifyArtworkGeometry(layout);
   var corner=layout.Controls[0].proxy.GetComponentsInChildren<ControlLayoutDrag>().First(d=>d.resize);
   var cornerPoint=ScreenPoint((RectTransform)corner.transform);Check(Top(cornerPoint)==corner.gameObject,"RESIZE_CORNER_RAYCAST");
   data=new PointerEventData(EventSystem.current){pointerId=43,button=PointerEventData.InputButton.Left,position=cornerPoint};
   corner.OnPointerDown(data);data.position+=new Vector2(48,-48);corner.OnDrag(data);corner.OnPointerUp(data);
   Check(layout.Size(0)>1.4f,"POINTER_CORNER_RESIZES");layout.ResizeSelected(1.4f);
   Capture("live-editor");
   Click("Cancel layout");yield return null;
   Check(!layout.Editing&&app.view.InputBlocked&&(layout.NormalizedCenter(0)-defaultPosition).magnitude<.002f,"CANCEL_ROLLS_BACK_AND_RETURNS_TO_SETTINGS");
   Click("Layout");yield return null;layout.Select(0);layout.MoveSelected(new Vector2(.2f,.3f));layout.ResizeSelected(1.2f);
   Click("Save layout");yield return null;
   Check(ControlLayouts.Load(profile,app.SelectedSport)["move"].size==1.2f,"SAVE_PERSISTS");
   Check(ControlLayouts.Load(profile+"different-account",app.SelectedSport).Count==0,"OTHER_ACCOUNT_ISOLATED");
   foreach(SportId sport in Enum.GetValues(typeof(SportId)))if(sport!=app.SelectedSport)Check(ControlLayouts.Load(profile,sport).Count==0,"OTHER_SPORT_ISOLATED_"+sport);
   Click("Layout");yield return null;Click("Reset layout");Check(Mathf.Abs(layout.Size(0)-1)<.001f,"RESET_DRAFT_DEFAULTS");app.HandleBack();yield return null;
   Check(!layout.Editing&&Mathf.Abs(layout.Size(0)-1.2f)<.001f,"BACK_DISCARDS_RESET");
   Click("Layout");yield return null;
   layout.Select(0);layout.MoveSelected(new Vector2(-10,10));layout.ResizeSelected(99);yield return null;
   Check(layout.Size(0)==ControlLayouts.MaxSize,"SIZE_BOUNDED");
   var b=layout.Bounds(0);Check(layout.Frame.rect.Contains(b.min)&&layout.Frame.rect.Contains(b.max),"TOUCH_RECT_CLAMPED_INSIDE_SAFE_FRAME");
   // A second finger cannot take ownership of an existing drag.
   drag=layout.Controls[0].proxy.GetComponent<ControlLayoutDrag>();data.position=ScreenPoint(layout.Controls[0].proxy);drag.OnPointerDown(data);
   var before=layout.NormalizedCenter(0);drag.OnDrag(new PointerEventData(EventSystem.current){pointerId=42,position=Vector2.zero});
   Check((before-layout.NormalizedCenter(0)).sqrMagnitude<.000001f,"UNRELATED_POINTER_IGNORED");drag.OnPointerUp(data);
   Click("Move tools");yield return null;Capture("tools-below");Click("Move tools");
   layout.Cancel();CloseSettings();yield return null;
   Check(!app.view.InputBlocked,"CLOSING_SETTINGS_RESTORES_PLAY");
   var stick=app.view.stick;point=ScreenPoint((RectTransform)stick.transform);
   Check(Top(point)&&Top(point).GetComponentInParent<TouchPad>()==stick,"SAVED_JOYSTICK_HITBOX_MOVED_WITH_ART");
   data=new PointerEventData(EventSystem.current){pointerId=84,button=PointerEventData.InputButton.Left,position=point};
   stick.OnPointerDown(data);data.position+=Vector2.right*45;stick.OnDrag(data);Check(stick.value.x>.1f,"CUSTOM_JOYSTICK_STILL_RESPONDS");stick.OnPointerUp(data);Check(stick.value==Vector2.zero,"JOYSTICK_RELEASE_CLEARS_INPUT");
   if(app.SelectedSport==SportId.Basketball||app.SelectedSport==SportId.Football){
    OpenSettings();yield return null;Click("Layout");yield return null;layout.Select(1);layout.MoveSelected(new Vector2(.55f,.35f));layout.ResizeSelected(1.25f);Click("Save layout");CloseSettings();yield return null;
    var guide=app.SelectedSport==SportId.Basketball?(RectTransform)BasketballHUD.Instance.ShotGuide.transform:(RectTransform)FootballHUD.Instance.Kick.guide.transform;
    Check(Vector3.Distance(guide.TransformPoint(Vector3.zero),layout.Controls[1].target.TransformPoint(Vector3.zero))<.1f,"SHOT_GUIDE_FOLLOWS_BUTTON");
   }
   foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,900),new Vector2Int(1024,768)}){
    Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.5f);OpenSettings();yield return null;Click("Layout");yield return null;
    layout.Select(0);layout.MoveSelected(new Vector2(.23f,.32f));yield return null;b=layout.Bounds(0);
    Check(layout.Frame.rect.Contains(b.min)&&layout.Frame.rect.Contains(b.max),"RESPONSIVE_"+size);
    Capture("layout-"+size.x+"x"+size.y);layout.Cancel();CloseSettings();
   }
   Screen.SetResolution(1600,900,false);yield return new WaitForSecondsRealtime(.5f);
   app.LeaveCourt();yield return Until(()=>!app.Exploring&&app.Menu,"RETURN_TO_MENU");
   app.Show("settings");yield return Until(()=>!app.Menu.IsTransitioning,"SETTINGS_READY");yield return null;Click("Layout");
   yield return Until(()=>app.Menu.Page=="layouts"&&!app.Menu.IsTransitioning,"MAIN_MENU_LAYOUT_CHOOSER");yield return null;Capture("main-chooser");
   foreach(SportId sport in Enum.GetValues(typeof(SportId))){
    Click("Layout "+sport);yield return null;var preview=ControlLayoutEditor.Active;
    Check(preview&&preview.Preview&&preview.Sport==sport,"MENU_PREVIEW_"+sport);
    Check(preview.Controls.Count==(sport==SportId.Basketball?5:sport==SportId.Football?6:sport==SportId.Golf?7:6),"PREVIEW_BINDINGS_"+sport);
    VerifyArtwork(preview);
    preview.Select(0);preview.MoveSelected(new Vector2(.25f,.28f));preview.ResizeSelected(1.3f);yield return null;Capture("menu-"+sport);Click("Save layout");yield return null;
    Check(ControlLayouts.Load(profile,sport)["move"].size==1.3f,"MENU_SAVES_"+sport);
   }
   string linkProfile=profile+"linked";
   ControlLayouts.IdentityChanging(profile,linkProfile,true);Check(ControlLayouts.Load(linkProfile,app.SelectedSport)["move"].size==1.3f,"LINK_MIGRATES_LOCAL_LAYOUT");
   foreach(SportId sport in Enum.GetValues(typeof(SportId)))PlayerPrefs.DeleteKey(ControlLayouts.Key(linkProfile,sport));PlayerPrefs.Save();
   string bad=ControlLayouts.Key(profile+"malformed",app.SelectedSport);PlayerPrefs.SetString(bad,"{broken");
   Check(ControlLayouts.Load(profile+"malformed",app.SelectedSport).Count==0,"CORRUPT_PREFERENCE_RECOVERS");
   PlayerPrefs.DeleteKey(bad);PlayerPrefs.Save();Complete();
  }
  void Complete(){File.AppendAllText(Path.Combine(folder,"results.txt"),"CONTROL_LAYOUT_COMPLETE checks="+checks+"\n");Application.Quit();}
  void VerifyArtwork(ControlLayoutEditor editor){
   foreach(var c in editor.Controls){
    Check(c.Artwork!=null&&c.Artwork.Graphics.Any(p=>p.copy.enabled),"ACTUAL_ART_EXISTS_"+editor.Sport+"_"+c.id);
    Check(c.Artwork.Root.GetComponentsInChildren<Selectable>(true).Length==0&&c.Artwork.Root.GetComponentsInChildren<TouchPad>(true).Length==0,"ART_HAS_NO_GAMEPLAY_INPUT_"+editor.Sport+"_"+c.id);
    Check(c.Artwork.Graphics.All(pair=>pair.source is not Text a||pair.copy is Text b&&a.font==b.font&&a.fontSize==b.fontSize&&a.alignment==b.alignment&&a.text==b.text),"ORIGINAL_TYPOGRAPHY_"+editor.Sport+"_"+c.id);
    Check(c.proxy.GetComponentsInChildren<Text>(true).Length==c.target.GetComponentsInChildren<Text>(true).Length,"NO_ADDED_BUTTON_CAPTIONS_"+editor.Sport+"_"+c.id);
   }
   Check(editor.Dock.rect.height<=88&&editor.Dock.rect.width<=490,"COMPACT_EDITING_BAR_"+editor.Sport);
   VerifyArtworkGeometry(editor);
  }
  void VerifyArtworkGeometry(ControlLayoutEditor editor){
   var a=new Vector3[4];var b=new Vector3[4];
   foreach(var c in editor.Controls){
    bool same=true;
    foreach(var pair in c.Artwork.Rects){
     pair.source.GetWorldCorners(a);pair.copy.GetWorldCorners(b);
     for(int i=0;i<4;i++)same&=Vector3.Distance(a[i],b[i])<.15f;
    }
    Check(same,"ART_GEOMETRY_FOLLOWS_HITBOX_"+editor.Sport+"_"+c.id);
   }
  }
  void Capture(string name){
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
   var camera=Camera.main;var previousTarget=camera.targetTexture;var previousActive=RenderTexture.active;
   var target=new RenderTexture(Screen.width,Screen.height,24);var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
   try{
    foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=.5f;}camera.targetTexture=target;
    // Changing canvas render mode can rebuild the shared dynamic font atlas.
    // Warm it and rebuild labels before reading the final frame.
    for(int pass=0;pass<3;pass++){foreach(var text in FindObjectsByType<Text>(FindObjectsSortMode.None))text.SetAllDirty();Canvas.ForceUpdateCanvases();camera.Render();}
    RenderTexture.active=target;image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());
   }
   finally{camera.targetTexture=previousTarget;RenderTexture.active=previousActive;Destroy(target);Destroy(image);for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=RenderMode.ScreenSpaceOverlay;canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}}
  }
 }
}
#endif
