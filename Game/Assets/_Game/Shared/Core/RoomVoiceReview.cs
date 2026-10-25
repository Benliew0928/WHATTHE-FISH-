#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class RoomVoiceReview:MonoBehaviour {
  string folder;int checks;AppRoot app;RoomChatUI ui;RoomVoiceService service;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-voiceReview");if(i>=0&&i+1<args.Length)new GameObject("Voice review").AddComponent<RoomVoiceReview>().folder=args[i+1];}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);float end=Time.realtimeSinceStartup+80;
   while((!AppRoot.Instance||!RoomChatUI.Instance||!AppRoot.Instance.rooms.Connected||!NetworkAthlete.HostPlayer)&&Time.realtimeSinceStartup<end)yield return null;
   app=AppRoot.Instance;ui=RoomChatUI.Instance;Check(app&&ui&&app.rooms.Connected,"local review room ready");service=RoomVoiceService.Ensure(app.rooms);
   var stateChecks=RoomVoiceChecks.Run();while(!stateChecks.IsCompleted)yield return null;
   Check(!stateChecks.IsFaulted,"connection state regression");foreach(string result in stateChecks.Result){File.AppendAllText(Path.Combine(folder,"results.txt"),result+"\n");checks++;}
   ui.OpenVoice();yield return new WaitForSecondsRealtime(.3f);
   Check(!service.Available,"local transport does not masquerade as live voice");Check(!Button("Toggle microphone").interactable,"unavailable microphone stays disabled");
   var fake=new RoomVoiceChecks.Fake();fake.People.Add(new RoomVoicePeer{Id="self",Name="Harbor",IsSelf=true});
   for(int i=1;i<20;i++)fake.People.Add(new RoomVoicePeer{Id="crew"+i,Name=(i>=14?"Reef ":"Coral ")+i.ToString("00"),Speaking=i==1,Spectator=i>=14});
   var voice=new RoomVoiceSession(fake,()=>Time.realtimeSinceStartupAsDouble);voice.SetRoom("voice-ui-review","You");service.ReviewUseSession(voice);voice.Join();
   yield return new WaitForSecondsRealtime(.4f);Check(voice.Ready&&!voice.MicEnabled,"UI joins listening with mic off");
   Check(ui.VoiceOpen&&RoomChatUI.BlocksInput&&RoomChatUI.BlocksBack&&app.view.InputBlocked,"voice panel blocks gameplay and consumes back");
   Check(Button("Toggle microphone").interactable,"connected microphone action enabled");Button("Toggle microphone").onClick.Invoke();yield return new WaitForSecondsRealtime(.2f);Check(fake.Mic&&voice.MicEnabled,"mic action enables transport");
   Button("Toggle microphone").onClick.Invoke();Check(!fake.Mic,"mic action immediately silences transport");
   Button("Toggle voice speakers").onClick.Invoke();Check(!fake.Sound,"speaker action mutes playback");Button("Toggle voice speakers").onClick.Invoke();Check(fake.Sound,"speaker action restores playback");
   var slider=ui.GetComponentsInChildren<Slider>().Single(s=>s.name=="Voice volume slider");slider.value=.4f;Check(Mathf.Abs(fake.Volume-.4f)<.001f,"volume slider reaches audio layer");
   Check(ui.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Voice member "))==14,"players tab contains only its fourteen members");
   Check(ui.GetComponentsInChildren<Text>().Any(t=>t.text=="Harbor (You)")&&ui.GetComponentsInChildren<Text>().Any(t=>t.text=="Coral 01"),"roster uses in-game names including self");
   Button("Mute voice member 1").onClick.Invoke();Check(voice.IsMuted("crew1"),"speaker icon mutes chosen member");Button("Mute voice member 1").onClick.Invoke();Check(!voice.IsMuted("crew1"),"speaker icon unmutes chosen member");
   Button("Voice member 1").onClick.Invoke();yield return new WaitForSecondsRealtime(.2f);
   ui.GetComponentsInChildren<Slider>().Single(s=>s.name=="Member volume slider").value=.25f;Check(Mathf.Abs(fake.PeerVolumes["crew1"]-.25f)<.001f,"member slider controls only chosen participant");
   yield return Capture("member-volume");ui.SetVoiceAudience(true);yield return new WaitForSecondsRealtime(.3f);
   Check(ui.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Voice member "))==6,"spectator tab contains only its six members");
   Button("Mute voice member 0").onClick.Invoke();Button("Mute all spectators").onClick.Invoke();yield return new WaitForSecondsRealtime(.3f);
   Check(voice.SpectatorsMuted&&fake.People.Where(p=>p.Spectator).All(p=>fake.Muted[p.Id])&&!fake.Muted["crew1"],"spectator bulk mute leaves players audible");
   Check(!Button("Mute voice member 1").interactable,"individual actions cannot override spectator bulk mute");yield return Capture("spectators-muted");
   Button("Mute all spectators").onClick.Invoke();yield return new WaitForSecondsRealtime(.2f);Check(fake.Muted["crew14"]&&!fake.Muted["crew15"],"bulk unmute preserves earlier individual mute");
   Button("Voice member 1").onClick.Invoke();yield return new WaitForSecondsRealtime(.2f);ui.GetComponentsInChildren<Slider>().Single(s=>s.name=="Member volume slider").value=.6f;yield return Capture("spectator-volume");
   ui.SetVoiceAudience(false);yield return new WaitForSecondsRealtime(.2f);
   Check(!ui.GetComponentsInChildren<Text>().Any(t=>t.text.Contains("every island")||t.text.Contains("SPEAKING LIGHTS UP")||t.text.Contains("starts off")),"decorative subtitles removed");
   yield return new WaitForSecondsRealtime(.2f);Check(ui.GetComponentsInChildren<Text>().Any(t=>t.text=="● Speaking"),"speaking indicator follows live state");
   yield return Views("lobby");
   ui.Close();yield return new WaitForSecondsRealtime(.2f);Check(ui.Launchers.GetComponentsInChildren<Button>().Length==2,"launcher contains only chat and mic icons");
   yield return Capture("launcher-muted");yield return Gesture(false,false);Check(voice.MicEnabled&&!ui.VoiceOpen,"short tap unmutes without opening settings");yield return Capture("launcher-live");
   yield return Gesture(false,false);Check(!voice.MicEnabled&&!ui.VoiceOpen,"second short tap mutes without opening settings");
   yield return Gesture(false,true);Check(!voice.MicEnabled&&!ui.VoiceOpen,"dragging off cancels mic action");
   yield return Gesture(true,false);Check(ui.VoiceOpen&&!voice.MicEnabled,"long press opens settings without unmuting");
   service.Leave();yield return new WaitForSecondsRealtime(.2f);ui.Close();yield return Gesture(false,false);Check(voice.Ready&&voice.MicEnabled,"mic tap joins and speaks after explicit consent");
   service.Leave();yield return new WaitForSecondsRealtime(.2f);_=service.Join(true);service.SendMessage("OnApplicationPause",true);yield return new WaitForSecondsRealtime(.2f);Check(!service.PendingMicrophone&&!voice.MicEnabled,"background cancels pending mic intent");
   service.SendMessage("OnApplicationPause",false);yield return new WaitForSecondsRealtime(.3f);Check(voice.Ready&&!voice.MicEnabled,"resume never restores cancelled mic intent");
   ui.Close();yield return null;yield return null;Check(!RoomChatUI.BlocksInput&&!ui.VoiceOpen,"closing releases gameplay input");
   ui.SetOpen(true);Check(!ui.VoiceOpen&&ui.Open,"text chat remains available");ui.OpenVoice();Check(ui.VoiceOpen&&!ui.Open,"voice and text panels are mutually exclusive");ui.Close();
   ui.OpenVoice();Check(app.view.InputBlocked&&!app.view.ExplicitInputBlocked,"voice uses a transient input block");
   string channel=voice.Channel;voice.SetMicrophone(true);var explore=app.rooms.SetExploring(true);while(!explore.IsCompleted)yield return null;
   var travelMute=ui.GetComponentsInChildren<Button>(true).Single(b=>b.name=="Mute voice during travel");
   end=Time.realtimeSinceStartup+15;while(!travelMute.gameObject.activeInHierarchy&&Time.realtimeSinceStartup<end)yield return null;
   Check(travelMute.gameObject.activeInHierarchy&&fake.Mic,"travel keeps an accessible microphone mute control");travelMute.onClick.Invoke();Check(!fake.Mic&&!voice.MicEnabled,"travel mute immediately stops transmission");
   end=Time.realtimeSinceStartup+90;while((!app.Exploring||RoomTeleport.Busy||TeleportSequence.Busy||RoomMatchStart.Travelling||!app.view.active||!app.view.target||app.view.target.inTransit)&&Time.realtimeSinceStartup<end)yield return null;
   yield return new WaitForSecondsRealtime(.3f);
   IslandTutorialUI tutorial=app.SelectedSport==SportId.Fishing?FishingHUD.Instance.Tutorial:SportTutorialUI.Find(app.SelectedSport);
   Check(tutorial,"island rules exist for the fresh review profile");
   // Room exploration can acknowledge before its final travel coroutine starts.
   // Observe a stable visible tutorial instead of opening it into that transition.
   end=Time.realtimeSinceStartup+30;float rulesShownAt=-1;
   while(Time.realtimeSinceStartup<end){
    bool settled=app.Exploring&&!RoomTeleport.Busy&&!TeleportSequence.Busy&&!RoomMatchStart.Travelling&&app.view.active&&app.view.target&&!app.view.target.inTransit;
    if(settled&&!tutorial.Visible){rulesShownAt=-1;tutorial.Open();}
    else if(settled&&tutorial.Visible){if(rulesShownAt<0)rulesShownAt=Time.realtimeSinceStartup;if(Time.realtimeSinceStartup-rulesShownAt>.8f)break;}else rulesShownAt=-1;
    yield return null;
   }
   voice.SetMicrophone(true);yield return new WaitForSecondsRealtime(.3f);
   Check(tutorial.Visible&&!ui.Visible&&travelMute.gameObject.activeInHierarchy,$"island rules hide chat and keep microphone mute accessible (rules={tutorial.Visible}, chat={ui.Visible}, mic={voice.MicEnabled}, mute={travelMute.gameObject.activeInHierarchy}, active={app.view.active}, transit={app.view.target.inTransit}, roomTravel={RoomTeleport.Busy}, sequence={TeleportSequence.Busy}, matchTravel={RoomMatchStart.Travelling})");
   travelMute.onClick.Invoke();Check(!voice.MicEnabled,"island rules fallback mutes voice");tutorial.Close();yield return new WaitForSecondsRealtime(.3f);
   Check(ui.Visible&&app.Exploring,"gameplay voice controls are visible");Check(voice.Ready&&voice.Channel==channel,"lobby-to-island travel preserves channel");
   Check(!ui.VoiceOpen&&!app.view.InputBlocked,"travel restores movement after closing voice");
   if(app.SelectedSport==SportId.Golf)yield return GolfIntegration(voice,travelMute);
   ui.OpenVoice();yield return Views("gameplay");
   ui.Close();voice.Suspend(true);yield return new WaitForSecondsRealtime(.2f);Check(!fake.Connected&&!fake.Mic,"background releases audio");voice.Suspend(false);yield return new WaitForSecondsRealtime(.2f);Check(voice.Ready&&!fake.Mic,"foreground restores listening only");
   var leave=app.rooms.Leave();while(!leave.IsCompleted)yield return null;yield return new WaitForSecondsRealtime(.3f);Check(!fake.Connected&&!voice.Enabled,"leaving game room leaves voice");
   File.AppendAllText(Path.Combine(folder,"results.txt"),"VOICE_UI_PASS "+checks+" assertions\n");Application.Quit();
  }
  Button Button(string name)=>ui.GetComponentsInChildren<Button>().First(b=>b.name==name);
  IEnumerator Gesture(bool hold,bool cancel){
   var target=Button("Open room voice").gameObject;Canvas.ForceUpdateCanvases();var rect=target.GetComponent<RectTransform>();
   var e=new PointerEventData(EventSystem.current){pointerId=7,button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center))};
   ExecuteEvents.Execute(target,e,ExecuteEvents.pointerDownHandler);
   if(cancel)ExecuteEvents.Execute(target,e,ExecuteEvents.pointerExitHandler);
   // A short synthetic press releases in the same frame, so a slow build/CI
   // frame cannot accidentally turn the requested tap into a real long press.
   if(hold)yield return new WaitForSecondsRealtime(.65f);
   if(target){ExecuteEvents.Execute(target,e,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(target,e,ExecuteEvents.pointerClickHandler);}
   yield return new WaitForSecondsRealtime(.2f);
  }
  IEnumerator GolfIntegration(RoomVoiceSession voice,Button fallback){
   Check(GolfMatchManager.Instance.StartMatch(),"Golf match starts with combined network schema");
   float end=Time.realtimeSinceStartup+30;
   while((RoomMatchStart.Travelling||GolfMatchManager.Instance.Starting)&&Time.realtimeSinceStartup<end)yield return null;
   yield return new WaitForSecondsRealtime(.5f);
   var board=app.HudFrame.GetComponentInChildren<GolfLeaderboardUI>(true);
   Check(board&&board.BoardRect.gameObject.activeInHierarchy,"Golf standings visible in active round");
   var launcher=(RectTransform)Button("Open room voice").transform;var a=new Vector3[4];var b=new Vector3[4];launcher.GetWorldCorners(a);board.BoardRect.GetWorldCorners(b);
   Check(a[1].y<b[0].y,"voice launcher sits below compact Golf standings");
   voice.SetMicrophone(true);board.ToggleButton.onClick.Invoke();yield return new WaitForSecondsRealtime(.3f);
   Check(board.Expanded&&!ui.Visible&&fallback.gameObject.activeInHierarchy,"expanded standings keep microphone mute accessible");
   fallback.onClick.Invoke();Check(!voice.MicEnabled,"standings fallback mutes voice");board.BackdropButton.onClick.Invoke();yield return new WaitForSecondsRealtime(.3f);
   Check(ui.Visible,"voice controls return after closing standings");
   ui.OpenVoice();GolfHUD.Instance.IslandMenu.Open();yield return new WaitForSecondsRealtime(.3f);
   Check(!ui.VoiceOpen&&app.view.InputBlocked,"settings own input after closing voice");GolfHUD.Instance.IslandMenu.Close();yield return new WaitForSecondsRealtime(.3f);
   Check(!app.view.InputBlocked,"settings restore movement without retaining voice lock");
   var match=GolfMatchManager.Instance;var actor=app.LocalAthlete;var ball=match.Ball(actor);var tee=match.Target(actor).tee;
   ball.Place(match.TeePosition(match.Target(actor).number,ball.Owner),true);
   var feet=ball.Body.position-tee.forward*2.6f;
   Check(Physics.Raycast(feet+Vector3.up*5,Vector3.down,out var ground,20,1<<8,QueryTriggerInteraction.Ignore),"Golf aim fixture has ground support");
   actor.capsule.enabled=false;actor.transform.position=ground.point+Vector3.up*.035f;actor.ResetLocomotion();
   var net=actor.GetComponent<Unity.Netcode.Components.NetworkTransform>();if(net&&net.IsSpawned&&net.IsServer)net.Teleport(actor.transform.position,actor.transform.rotation,Vector3.one);
   actor.capsule.enabled=true;Physics.SyncTransforms();yield return new WaitForSecondsRealtime(.5f);
   end=Time.realtimeSinceStartup+5;while(!app.view.CanAimGolf&&Time.realtimeSinceStartup<end)yield return null;
   File.AppendAllText(Path.Combine(folder,"results.txt"),$"AIM_FIXTURE blocked={app.view.InputBlocked} active={app.view.active} grounded={actor.Grounded} transit={actor.inTransit} canSwing={match.CanSwing(actor)} ballReady={ball.ReadyToAim} distance={Vector3.Distance(actor.transform.position,ball.Body.position):F3} stance={match.TryStance(actor,ball,out _,app.view.yaw)}\n");
   voice.SetMicrophone(true);Check(app.view.BeginGolfAim(),"Golf aim begins for voice regression");yield return new WaitForSecondsRealtime(.6f);
   Check(GolfHUD.Instance.AimFocused&&!Button("Open room voice").IsInteractable()&&fallback.gameObject.activeInHierarchy,"aim hides chat launchers but retains microphone mute");
   fallback.onClick.Invoke();Check(!voice.MicEnabled,"aim fallback mutes voice");app.view.CancelGolfAim();yield return new WaitForSecondsRealtime(.5f);
   Check(Button("Open room voice").IsInteractable(),"voice launcher restores after aim");
  }
  void Check(bool value,string label){checks++;File.AppendAllText(Path.Combine(folder,"results.txt"),(value?"PASS ":"FAIL ")+label+"\n");if(!value)throw new InvalidOperationException(label);}
  IEnumerator Views(string context){
   foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1920,864),new Vector2Int(1024,768)}){
    Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.6f);if(!ui.VoiceOpen)ui.OpenVoice();yield return new WaitForSecondsRealtime(.2f);
    var corners=new Vector3[4];ui.VoicePanel.GetWorldCorners(corners);Check(corners.All(p=>p.x>=-1&&p.x<=Screen.width+1&&p.y>=-1&&p.y<=Screen.height+1),context+" panel fits "+size);
    var launcherCorners=new Vector3[4];ui.Launchers.GetWorldCorners(launcherCorners);var panelRect=Rect.MinMaxRect(corners[0].x,corners[0].y,corners[2].x,corners[2].y);var launchRect=Rect.MinMaxRect(launcherCorners[0].x,launcherCorners[0].y,launcherCorners[2].x,launcherCorners[2].y);Check(!panelRect.Overlaps(launchRect),"voice settings do not overlap launcher "+context+size);
    Check(ui.GetComponentsInChildren<Text>().All(t=>!t.supportRichText),"voice names render literally "+context+size);
    if(context=="gameplay"&&app.SelectedSport==SportId.Fishing){var board=app.HudFrame.GetComponentsInChildren<RectTransform>().First(t=>t.name=="Lagoon sticker leaderboard");var boardCorners=new Vector3[4];board.GetWorldCorners(boardCorners);Check(corners[1].y<boardCorners[0].y,"Fishing voice stays beneath leaderboard "+size);}
    if(context=="gameplay")DockCheck(ui.VoicePanel,"voice players "+size);
    yield return Capture(context+"-"+size.x+"x"+size.y);
    if(context=="gameplay")yield return DockViews(size);
   }
   Screen.SetResolution(1600,900,false);yield return new WaitForSecondsRealtime(.3f);
  }
  Rect ScreenRect(RectTransform rect){var c=new Vector3[4];rect.GetWorldCorners(c);return Rect.MinMaxRect(c[0].x,c[0].y,c[2].x,c[2].y);}
  void DockCheck(RectTransform panel,string label){
   var p=ScreenRect(panel);var launcher=ScreenRect(ui.Launchers);float scale=launcher.width/ui.Launchers.rect.width;
   Check(panel.rect.width<=580.5f&&panel.rect.height<=270.5f&&Mathf.Abs(p.xMin-launcher.xMin)<1&&p.yMax<=launcher.yMin-6*scale&&p.yMin>=launcher.yMin-280*scale,label+" stays in requested shallow dock");
   if(app.view.stick){var stick=ScreenRect((RectTransform)app.view.stick.transform);Check(!p.Overlaps(stick),label+" clears movement joystick");}
  }
  void ClickVisible(string name){
   var b=Button(name);var rect=ScreenRect((RectTransform)b.transform);
   var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=rect.center},hits);
   Check(hits.Count>0&&(hits[0].gameObject==b.gameObject||hits[0].gameObject.transform.IsChildOf(b.transform)),name+" receives the pointer inside its visible panel");b.onClick.Invoke();
  }
  IEnumerator DockViews(Vector2Int size){
   ui.SetVoiceAudience(true);yield return new WaitForSecondsRealtime(.2f);DockCheck(ui.VoicePanel,"voice spectators "+size);
   ClickVisible("Mute all spectators");yield return new WaitForSecondsRealtime(.2f);
   if(size.x==1600)yield return Capture("dock-spectators");ClickVisible("Mute all spectators");
   var roster=ui.GetComponentsInChildren<ScrollRect>().Single(s=>s.name=="Voice roster viewport");roster.verticalNormalizedPosition=0;yield return null;
   ClickVisible("Voice member 5");yield return new WaitForSecondsRealtime(.2f);
   var member=ui.GetComponentsInChildren<Slider>().Single(s=>s.name=="Member volume slider");member.value=.65f;yield return null;
   var viewport=ScreenRect(roster.viewport);var slider=ScreenRect((RectTransform)member.transform);
   Check(viewport.Contains(slider.min)&&viewport.Contains(slider.max),"last spectator volume stays reachable inside scrolled roster "+size);
   if(size.x==1600)yield return Capture("dock-member-volume");
   ui.SetOpen(true);yield return new WaitForSecondsRealtime(.2f);DockCheck(ui.Panel,"chat history "+size);
   if(size.x==1600)yield return Capture("dock-chat");
   ClickVisible("Chat options");yield return null;
   var preferences=ui.GetComponentsInChildren<RectTransform>().Single(r=>r.name=="Chat preferences");DockCheck(preferences,"chat preferences "+size);ClickVisible("Close preferences");yield return null;
   ClickVisible("Compose sticker");yield return null;
   var stickers=ui.GetComponentsInChildren<RectTransform>().Single(r=>r.name=="Choose a sticker");DockCheck(stickers,"sticker picker "+size);
   Check(RoomChatService.Stickers.All(id=>{var r=ScreenRect((RectTransform)Button("Send "+id).transform);var bounds=ScreenRect(stickers);return bounds.Contains(r.min)&&bounds.Contains(r.max);}),"every sticker stays inside the compact picker "+size);
   if(size.x==1600)yield return Capture("dock-stickers");
   ClickVisible("Close stickers");yield return null;ui.OpenVoice();ui.SetVoiceAudience(false);yield return new WaitForSecondsRealtime(.2f);
  }
  IEnumerator Capture(string name){
   yield return null;var camera=app.view.GetComponent<Camera>();var overlays=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
   foreach(var c in overlays){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}Canvas.ForceUpdateCanvases();
   var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=camera.targetTexture;var active=RenderTexture.active;camera.targetTexture=target;camera.Render();RenderTexture.active=target;
   var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);
   camera.targetTexture=previous;RenderTexture.active=active;RenderTexture.ReleaseTemporary(target);foreach(var c in overlays){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}
  }
 }
}
#endif
