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
 public sealed class StorybookMenuReview : MonoBehaviour {
  AppRoot app;string folder,role;bool failed;string clipboard;float master,ui;bool motion;int cameraMode;StadiumAppearance football,basketball;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-coveReview");if(index<0||index+1>=args.Length)return;
   var p=new GameObject("Storybook interaction review").AddComponent<StorybookMenuReview>();p.folder=args[index+1];int n=Array.IndexOf(args,"-coveRole");p.role=n>=0&&n+1<args.Length?args[n+1]:"offline";
  }
  void Check(bool value,string label){failed|=!value;File.AppendAllText(Path.Combine(folder,"results.txt"),(value?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Wait(Func<bool> condition,string label,float timeout=20){float until=Time.realtimeSinceStartup+timeout;while(!condition()&&Time.realtimeSinceStartup<until)yield return null;Check(condition(),label);}
  IEnumerator Settled(){yield return null;yield return Wait(()=>app.Menu&&!app.Menu.IsTransitioning&&!app.PendingSelection.HasValue,"page settled",30);yield return new WaitForSecondsRealtime(.2f);Canvas.ForceUpdateCanvases();}
  CoveButton Button(string name)=>app.Menu.Content.GetComponentsInChildren<CoveButton>().FirstOrDefault(b=>b.name==name);
  InputField Field(string name)=>app.Menu.Content.GetComponentsInChildren<InputField>().FirstOrDefault(b=>b.name==name);
  PointerEventData Pointer(GameObject target,int id=31){var r=(RectTransform)target.transform;var center=r.TransformPoint(r.rect.center);return new PointerEventData(EventSystem.current){pointerId=id,button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,center)};}
  void Down(CoveButton b,PointerEventData p){ExecuteEvents.Execute(b.gameObject,p,ExecuteEvents.pointerEnterHandler);ExecuteEvents.Execute(b.gameObject,p,ExecuteEvents.pointerDownHandler);}
  void Up(CoveButton b,PointerEventData p){ExecuteEvents.Execute(b.gameObject,p,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(b.gameObject,p,ExecuteEvents.pointerClickHandler);}
  void Click(string name){var b=Button(name);Check(b&&b.IsInteractable(),"action available: "+name);if(!b||!b.IsInteractable())return;var p=Pointer(b.gameObject);Down(b,p);Up(b,p);}
  void Layout(){
   var canvas=app.Menu.Content.GetComponentInParent<Canvas>();var corners=new Vector3[4];
   foreach(var b in app.Menu.Content.GetComponentsInChildren<CoveButton>()){
    var r=(RectTransform)b.transform;r.GetWorldCorners(corners);var min=RectTransformUtility.WorldToScreenPoint(null,corners[0]);var max=RectTransformUtility.WorldToScreenPoint(null,corners[2]);
    Check(min.x>=-1&&min.y>=-1&&max.x<=Screen.width+1&&max.y<=Screen.height+1,"button in safe viewport: "+b.name);
    if(!b.IsInteractable())continue;var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(Pointer(b.gameObject),hits);
    Check(hits.Count>0&&hits[0].gameObject.GetComponentInParent<CoveButton>()==b,"button receives pointer: "+b.name);
   }
   foreach(var text in app.Menu.Content.GetComponentsInChildren<Text>()){
    if(!text.font||string.IsNullOrEmpty(text.text))continue;
    // Actual preferred dimensions catch truncation without mirroring layout code.
    Check(text.preferredHeight<=text.rectTransform.rect.height+3,"text fits: "+text.name+" ("+text.preferredHeight.ToString("F0")+"/"+text.rectTransform.rect.height.ToString("F0")+")");
    if(text.GetComponentInParent<CoveButton>()&&!text.text.Contains("\n"))Check(text.preferredWidth<=text.rectTransform.rect.width+3,"button label width: "+text.text);
   }
  }
  IEnumerator Capture(string name){yield return new WaitForEndOfFrame();if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)yield break;
   // Hidden Windows batch players do not present a back buffer. Render the real
   // Canvas through the camera instead of accepting an all-black screenshot.
   var camera=app.view.GetComponent<Camera>();var canvas=app.Menu.Content.GetComponentInParent<Canvas>();
   var oldMode=canvas.renderMode;var oldCamera=canvas.worldCamera;float distance=canvas.planeDistance;
   canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
   var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=camera.targetTexture;var active=RenderTexture.active;
   camera.targetTexture=target;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
   var screenshot=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);screenshot.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);screenshot.Apply();
   var samples=screenshot.GetPixels32();Check(samples.Where((pixel,index)=>index%1000==0).Select(pixel=>pixel.r+pixel.g*256+pixel.b*65536).Distinct().Count()>12,"capture contains rendered colour: "+name);
   File.WriteAllBytes(Path.Combine(folder,name+".png"),screenshot.EncodeToPNG());Destroy(screenshot);
   camera.targetTexture=previous;RenderTexture.active=active;RenderTexture.ReleaseTemporary(target);
   canvas.renderMode=oldMode;canvas.worldCamera=oldCamera;canvas.planeDistance=distance;Canvas.ForceUpdateCanvases();
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   yield return Wait(()=>AppRoot.Instance&&AppRoot.Instance.Menu,"menu initialized",80);app=AppRoot.Instance;if(!app||!app.Menu){Finish();yield break;}
   if(role!="offline"){yield return Network();Finish();yield break;}
   clipboard=GUIUtility.systemCopyBuffer;football=LocalProfile.Stadium;basketball=LocalProfile.Basketball;master=MenuPreferences.MasterVolume;ui=MenuPreferences.UIVolume;motion=MenuPreferences.ReducedMotion;cameraMode=app.view.mode;
   MenuPreferences.SetMotion(false);app.Show("home");yield return Settled();Layout();yield return Capture("01-home");
   var create=Button("Create room");var p=Pointer(create.gameObject);Down(create,p);yield return new WaitForSecondsRealtime(.7f);
   Check(create.Activations==0&&app.Menu.Page=="home","holding never activates or auto-repeats");Check(create.transform.localScale.x<.99f,"held button visibly compresses");
   ExecuteEvents.Execute(create.gameObject,new PointerEventData(EventSystem.current){pointerId=99},ExecuteEvents.pointerUpHandler);yield return null;
   Check(create.transform.localScale.x<.99f,"second finger cannot release a held button");
   ExecuteEvents.Execute(create.gameObject,p,ExecuteEvents.pointerExitHandler);p.position=Vector2.zero;Up(create,p);yield return new WaitForSecondsRealtime(.25f);
   Check(create.Activations==0&&app.Menu.Page=="home","drag outside cancels action");Check(Mathf.Abs(create.transform.localScale.x-1)<.02f,"cancel restores resting scale");
   p=Pointer(create.gameObject);Down(create,p);Up(create,p);Check(create.Activations==1,"release inside activates exactly once");
   Check(app.Menu.IsTransitioning&&!Button("Back").IsInteractable(),"transition blocks incoming input");yield return Settled();Check(app.Menu.Page=="create","create screen reached");Layout();yield return Capture("02-create-room");
   Click("Edit conditions");yield return Settled();Click("Duration 5");yield return Settled();Click("Save conditions");yield return Settled();
   Check(MenuMatchRules.CurrentMinutes==5&&MenuMatchRules.RegulationSeconds(180)==300,"saved five-minute condition drives actual match clock");
   app.Show("rules");yield return Settled();Layout();yield return Capture("05-conditions");MenuMatchRules.SetMinutes(3);
   app.Show("join");yield return Settled();Layout();Check(!Button("Join with code").IsInteractable(),"empty room code cannot join");
   var field=Field("Room code");field.text="AB";yield return new WaitForSecondsRealtime(.2f);Check(!Button("Join with code").IsInteractable(),"short room code cannot join");
   Check(!StorybookMenu.ValidCode(null)&&!StorybookMenu.ValidCode("<b>ABCD</b>")&&!StorybookMenu.ValidCode("ＡＢＣＤ"),"code validation rejects empty, markup and non-ASCII codes");
   Check(StorybookMenu.NormalizeCode(" ab12 ")=="AB12"&&StorybookMenu.ValidCode(" ab12 "),"codes normalize case and surrounding spaces");
   bool copied=false;yield return CoveClipboard.Copy("  wave24  ",ok=>copied=ok);Click("Paste code");yield return new WaitForSecondsRealtime(.2f);
   if(copied)Check(field.text=="WAVE24"&&Button("Join with code").IsInteractable(),"OS clipboard paste fills a usable uppercase code");
   else {
    File.AppendAllText(Path.Combine(folder,"results.txt"),"LIMIT OS clipboard roundtrip unavailable; verify copy/paste in an interactive desktop and on Android.\n");
    Check(field.text=="AB"&&!Button("Join with code").IsInteractable(),"unavailable clipboard does not submit an invalid invitation");
    Check(app.Menu.Content.GetComponentsInChildren<Text>().Any(t=>t.text.StartsWith("Copy just the")),"unavailable paste explains how to enter the host code");
   }
   field.text="";field.ActivateInputField();yield return null;
   foreach(char c in "wave24")field.ProcessEvent(new Event{type=EventType.KeyDown,character=c});
   yield return new WaitForSecondsRealtime(.2f);Check(field.text=="WAVE24"&&Button("Join with code").IsInteractable(),"typing normalizes case and enables join");yield return Capture("03-join-room");
   app.rooms.busy=true;yield return new WaitForSecondsRealtime(.25f);Check(!Button("Join with code").IsInteractable()&&!Button("Back").IsInteractable(),"connection locks duplicate requests and navigation");app.rooms.busy=false;
   app.Show("settings");yield return Settled();Layout();var volume=app.Menu.Content.GetComponentsInChildren<Slider>().First();volume.value=.27f;
   Check(Mathf.Abs(AudioListener.volume-.27f)<.001f,"volume control changes real output");Check(volume.handleRect.rect.height<=52,"slider thumb remains inside its track");Click("Camera Elevated");yield return Settled();Check(app.view.mode==2&&PlayerPrefs.GetInt("camera")==2,"camera choice persists");
   Click("Reduced motion");yield return Settled();Check(MenuPreferences.ReducedMotion,"reduced motion enabled");var done=Button("Done settings");p=Pointer(done.gameObject);Down(done,p);yield return new WaitForSecondsRealtime(.2f);Check(Mathf.Abs(done.transform.localScale.x-1)<.001f,"reduced motion removes scaling");ExecuteEvents.Execute(done.gameObject,p,ExecuteEvents.pointerExitHandler);p.position=Vector2.zero;Up(done,p);yield return Capture("08-settings");
   MenuPreferences.SetMotion(false);
   foreach(var sport in new[]{SportId.Football,SportId.Basketball,SportId.Golf,SportId.Fishing}){
    app.SelectSport(sport);yield return Settled();app.Show("help");yield return Settled();Layout();yield return Capture("09-help-"+sport);
    if(sport==SportId.Football||sport==SportId.Basketball){
     app.Show("custom");yield return Settled();Layout();Click("Reset venue");yield return Settled();Click("Palette Rose");yield return Settled();Check(app.Menu.Content.GetComponentsInChildren<RawImage>().Any(v=>v.texture),"real venue preview renders for "+sport);
     yield return Capture("06-venue-"+sport);Click("Preview detail");yield return Settled();
     if(sport==SportId.Football){Click("Board design");yield return Settled();Check(app.stadium.subtitle.text.Contains("GOOD ENERGY"),"screen message selector changes the actual venue sign");}
     else{Click("Logo Star");yield return Settled();var arena=app.stadium as BasketballArenaView;Check(arena&&arena.logoCatalog[1].activeSelf&&!arena.logoCatalog[0].activeSelf,"logo selector changes the actual court");}
     yield return Capture("07-venue-detail-"+sport);app.Menu.Back();yield return null;Check(Button("Discard changes"),"unsaved venue changes ask before discard");
     Check(!Button("Save venue").IsInteractable()&&Button("Discard changes").IsInteractable(),"confirmation isolates keyboard and pointer interaction");Click("Discard changes");yield return Settled();
     Check(LocalProfile.ForSport(sport).palette==(sport==SportId.Football?football.palette:basketball.palette),"discard preserves stored "+sport+" venue");
     app.Show("custom");yield return Settled();var title=Field("Venue name");title.text="COVE REVIEW";title.onEndEdit.Invoke(title.text);Click("Save venue");yield return Settled();Check(LocalProfile.ForSport(sport).title=="COVE REVIEW","venue save persists "+sport);
     LocalProfile.SaveSport(sport,sport==SportId.Football?football:basketball);app.stadium?.Apply(app.CurrentAppearance);
    }else{app.Show("home");yield return Settled();Check(!Button("Customize venue").IsInteractable(),"unsupported venue customization is visibly disabled for "+sport);}
   }
   app.SelectSport(SportId.Football);yield return Settled();app.Show("home");yield return Settled();
   foreach(var resolution in new[]{new Vector2Int(1280,800),new Vector2Int(1920,1080),new Vector2Int(2340,1080)}){
    Screen.SetResolution(resolution.x,resolution.y,false);yield return new WaitForSecondsRealtime(.6f);Canvas.ForceUpdateCanvases();Layout();yield return Capture("10-home-"+Screen.width+"x"+Screen.height+"-requested-"+resolution.x);
   }
   Click("Explore offline");Check(app.Menu.IsTransitioning,"island arrival fades and gates gameplay input");yield return new WaitForSecondsRealtime(.6f);Check(app.Exploring&&app.view.active&&!app.Menu.Content.gameObject.activeInHierarchy,"offline launch hands control to gameplay");
   Check(FindObjectsByType<Button>(FindObjectsSortMode.None).Where(b=>b.gameObject.activeInHierarchy).All(b=>b.GetComponent<CoveFeedback>()),"gameplay buttons have press feedback");
   Finish();
  }
  IEnumerator Network(){
   yield return Wait(()=>app.rooms.Connected&&NetworkAthlete.HostPlayer&&FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).Count(p=>p.IsSpawned)==2,"two matching protocol players connected",55);yield return Settled();
   if(!app.rooms.Connected)yield break;
   Check(RoomService.ProtocolVersion==25,"basketball steal/golf/menu protocol 25");
   var own=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).FirstOrDefault(p=>p.IsOwner);
   if(role=="host"){
    Check(MenuMatchRules.SetMinutes(5),"host can set conditions");
    yield return new WaitForSecondsRealtime(2);Layout();yield return Capture("04-lobby-host");
    Check(!Button("Travel together").IsInteractable(),"host cannot start with unready crew");Click("Ready toggle");
    yield return Wait(()=>FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).All(n=>n.Ready.Value),"both players acknowledge setup");
    Check(MenuMatchRules.SetMinutes(10),"host updates conditions");Check(FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).All(n=>!n.Ready.Value),"changing conditions resets every ready flag");
    yield return new WaitForSecondsRealtime(2);Click("Ready toggle");yield return Wait(()=>FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).All(n=>n.Ready.Value),"crew readies again");yield return new WaitForSecondsRealtime(.3f);
    Check(Button("Travel together").IsInteractable(),"all-ready enables departure");Click("Travel together");yield return Wait(()=>app.Exploring,"host enters gameplay");
    yield return new WaitForSecondsRealtime(3);_=app.rooms.SetExploring(false);yield return Wait(()=>!app.Exploring&&app.Menu.Page=="room","host returns entire crew to lobby");yield return new WaitForSecondsRealtime(3);
   }else{
    yield return Wait(()=>MenuMatchRules.CurrentMinutes==5,"guest receives five-minute rule");Check(!MenuMatchRules.SetMinutes(10)&&MenuMatchRules.CurrentMinutes==5,"guest cannot alter room conditions");
    app.Show("custom");yield return Settled();Check(app.Menu.Page=="room","guest cannot open host venue editor");Layout();yield return Capture("04-lobby-guest");
    Click("Ready toggle");yield return Wait(()=>MenuMatchRules.CurrentMinutes==10,"guest receives changed condition");Check(!own.Ready.Value,"guest is unready after rule change");yield return new WaitForSecondsRealtime(.5f);Click("Ready toggle");
    yield return Wait(()=>app.Exploring,"guest follows host into gameplay");Check(MenuMatchRules.RegulationSeconds(180)==600,"replicated rule supplies ten-minute clock");
    yield return Wait(()=>!app.Exploring&&app.Menu.Page=="room","guest follows host back to lobby");
   }
  }
  void Finish(){
   if(role=="offline"&&app){LocalProfile.Stadium=football;LocalProfile.Basketball=basketball;MenuPreferences.SetMaster(master);MenuPreferences.SetUI(ui);MenuPreferences.SetMotion(motion);app.view.mode=cameraMode;PlayerPrefs.SetInt("camera",cameraMode);MenuPreferences.Save();GUIUtility.systemCopyBuffer=clipboard;}
   File.AppendAllText(Path.Combine(folder,"results.txt"),"COVE_REVIEW_COMPLETE success="+!failed+"\n");
  }
 }
}
#endif
