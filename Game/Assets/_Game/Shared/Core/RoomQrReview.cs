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
 public sealed class RoomQrReview:MonoBehaviour {
  string folder;bool failed;AppRoot app;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-roomQrReview");if(i>=0&&i+1<args.Length)new GameObject("Room QR review").AddComponent<RoomQrReview>().folder=args[i+1];}
  void Check(bool value,string message){failed|=!value;File.AppendAllText(Path.Combine(folder,"results.txt"),(value?"PASS ":"FAIL ")+message+"\n");}
  IEnumerator Wait(Func<bool> condition,float timeout=90){double end=Time.realtimeSinceStartupAsDouble+timeout;while(!condition()&&Time.realtimeSinceStartupAsDouble<end)yield return null;}
  CoveButton Button(string name)=>app.Menu.Content.GetComponentsInChildren<CoveButton>().FirstOrDefault(b=>b.name==name);
  void Click(string name){var button=Button(name);Check(button&&button.IsInteractable(),"available: "+name);if(!button||!button.IsInteractable())return;
   var rect=(RectTransform)button.transform;var point=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));var pointer=new PointerEventData(EventSystem.current){position=point,button=PointerEventData.InputButton.Left};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
   Check(hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject)==button.gameObject,"pointer reaches "+name);button.onClick.Invoke();}
  IEnumerator Settled(){yield return Wait(()=>!app.Menu.IsTransitioning&&!app.PendingSelection.HasValue,30);yield return new WaitForSecondsRealtime(.3f);Canvas.ForceUpdateCanvases();}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   foreach(string input in new[]{"abcd"," WAVE24 ","123456789012"}){string payload=RoomInvitation.Encode(input);Check(RoomInvitation.TryDecode(payload,out var code,out _)&&code==input.Trim().ToUpperInvariant(),"roundtrip "+input);var image=HuaweiRoomScan.Generate(code);File.WriteAllBytes(Path.Combine(folder,code+".png"),image.EncodeToPNG());Destroy(image);}
   foreach(string input in new[]{null,"","AB","LOCAL TEST","—","<b>ABCD</b>","ＡＢＣＤ","1234567890123"}){bool rejected=false;try{RoomInvitation.Encode(input);}catch(ArgumentException){rejected=true;}Check(rejected,"invalid room rejected: "+input);}
   foreach(string input in new[]{null,"","ABCD","https://example.com","WTFROOM:2:ABCD","WTFROOM:1:ab12","WTFROOM:1: ABCD","WTFROOM:1:ABCD\n","WTFROOM:1:ABCD:SECRET",new string('A',200)})Check(!RoomInvitation.TryDecode(input,out _,out _),"foreign or malformed QR rejected: "+(input??"null").Substring(0,Math.Min(40,(input??"null").Length)));
   Check(!HuaweiRoomScan.Supported,"Windows has a truthful scanner fallback");
   bool fallback=false;HuaweiRoomScan.Scan((status,value)=>fallback=status=="unavailable"&&value.Contains("room code"));Check(fallback&&!HuaweiRoomScan.Busy,"unsupported scanner completes without blocking menu");
   yield return Wait(()=>AppRoot.Instance&&AppRoot.Instance.Menu);app=AppRoot.Instance;Check(app&&app.Menu,"game ready");if(!app||!app.Menu){Finish();yield break;}
   if(StartupWelcome.Instance&&StartupWelcome.Blocking){StartupWelcome.Instance.ContinueAsGuest();yield return null;if(StartupWelcome.Instance.GuestNameField)StartupWelcome.Instance.GuestNameField.text="QR Reviewer";if(StartupWelcome.Instance.GuestContinueButton)StartupWelcome.Instance.GuestContinueButton.onClick.Invoke();}
   yield return Wait(()=>!StartupWelcome.Blocking,10);Check(!StartupWelcome.Blocking,"guest entry complete");
   app.Show("join");yield return Settled();
   var field=app.Menu.Content.GetComponentsInChildren<InputField>().First(f=>f.name=="Room code");field.text="ABCD";yield return null;
   Check(Button("Join with code").IsInteractable(),"manual joining remains enabled");Click("Scan room QR");yield return null;
   Check(field.text=="ABCD"&&Button("Join with code").IsInteractable(),"scanner fallback preserves typed code and join button");
   Check(app.Menu.Content.GetComponentsInChildren<Text>().Any(t=>t.text.Contains("Scanning is available in the Android game")),"desktop scanner limitation explained");
   Click("Choose room QR image");yield return null;
   Check(field.text=="ABCD"&&Button("Join with code").IsInteractable()&&!HuaweiRoomScan.Busy,"image fallback preserves typed code and manual joining");
   yield return Capture("join");
   if(Environment.GetCommandLineArgs().Contains("-qrOnline")){
    var create=app.rooms.Create();yield return Wait(()=>create.IsCompleted,100);
    if(create.IsCompleted&&app.rooms.Connected&&StorybookMenu.ValidCode(app.rooms.Code)){
     app.Show("room");yield return Settled();Check(Button("Show room QR").IsInteractable(),"live online room has QR");
     var image=app.Menu.Content.GetComponentsInChildren<RawImage>().First(r=>r.name=="Room QR image");File.WriteAllBytes(Path.Combine(folder,"live-room.png"),((Texture2D)image.texture).EncodeToPNG());File.WriteAllText(Path.Combine(folder,"live-room-code.txt"),app.rooms.Code);
     yield return Capture("lobby");Click("Show room QR");yield return null;yield return Capture("invitation");
     Check(!Button("Travel together").IsInteractable()&&Button("Close room QR").IsInteractable(),"QR modal blocks underlying lobby");Click("Close room QR");yield return null;
     var leave=app.rooms.Leave();yield return Wait(()=>leave.IsCompleted,15);Check(!app.rooms.Connected,"review online room closed");
    }else File.AppendAllText(Path.Combine(folder,"results.txt"),"LIMIT Online session unavailable: "+app.rooms.Error+"\n");
   }
   app.rooms.StartLocal(true,7796);yield return Wait(()=>app.rooms.Connected,10);app.Show("room");yield return Settled();Check(!Button("Show room QR").IsInteractable(),"local test code never becomes a false online invitation");
   var exit=app.rooms.Leave();yield return Wait(()=>exit.IsCompleted,10);Finish();
  }
  IEnumerator Capture(string name){yield return new WaitForEndOfFrame();
   var camera=app.view.GetComponent<Camera>();var canvas=app.Menu.Content.GetComponentInParent<Canvas>();var oldMode=canvas.renderMode;var oldCamera=canvas.worldCamera;float distance=canvas.planeDistance;
   canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=camera.targetTexture;var active=RenderTexture.active;
   camera.targetTexture=target;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;var screenshot=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);screenshot.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);screenshot.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),screenshot.EncodeToPNG());Destroy(screenshot);
   camera.targetTexture=previous;RenderTexture.active=active;RenderTexture.ReleaseTemporary(target);canvas.renderMode=oldMode;canvas.worldCamera=oldCamera;canvas.planeDistance=distance;
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"ROOM_QR_REVIEW_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
