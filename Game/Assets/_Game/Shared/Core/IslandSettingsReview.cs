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
 // Opt-in review of the actual HUD, command reader and real room transport.
 public sealed class IslandSettingsReview:MonoBehaviour {
  string report,sync,role,exit;int checks;bool online,fishing;AppRoot app;IslandSettingsMenu menu;PlayerView view;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Environment.GetCommandLineArgs().Contains("-islandSettingsAudit"))new GameObject("Island settings review").AddComponent<IslandSettingsReview>();}
  string Arg(string key,string fallback=""){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:fallback;}
  void Check(bool value,string name){File.AppendAllText(report,(value?"PASS ":"FAIL ")+name+"\n");if(!value)throw new Exception(name);checks++;}
  Button Button(string name)=>menu.GetComponentsInChildren<Button>(true).First(b=>b.name==name);
  void Click(Button button,string tag){
   Canvas.ForceUpdateCanvases();var data=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,button.transform.TransformPoint(((RectTransform)button.transform).rect.center))};
   var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
   if(!button.IsInteractable()||hits.Count==0||hits[0].gameObject.GetComponentInParent<Button>()!=button){File.AppendAllText(report,"HITS "+tag+" target="+button.name+" position="+data.position+" hits="+string.Join(",",hits.Take(8).Select(h=>h.gameObject.name))+"\n");Capture("failed-"+tag);}
   Check(button.IsInteractable()&&hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==button,"RAYCAST_"+tag);
   ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerClickHandler);
  }
  bool HasText(string text)=>menu.GetComponentsInChildren<Text>().Any(t=>t.text.Contains(text));
  bool Reachable(Button button){var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,button.transform.TransformPoint(((RectTransform)button.transform).rect.center))},hits);return hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==button;}
  IEnumerator Until(Func<bool> predicate,string tag,float seconds=35){float deadline=Time.realtimeSinceStartup+seconds;while(!predicate()&&Time.realtimeSinceStartup<deadline)yield return null;Check(predicate(),tag);}
  void Mark(string stage)=>File.WriteAllText(Path.Combine(sync,role+"-"+stage),"ready");
  bool Marked(string who,string stage)=>File.Exists(Path.Combine(sync,who+"-"+stage));
  IEnumerator Start(){
   report=Arg("-settingsReport",Path.Combine(Application.persistentDataPath,"island-settings.txt"));sync=Arg("-settingsSync",Path.GetDirectoryName(report));Directory.CreateDirectory(sync);File.WriteAllText(report,"");exit=Arg("-settingsExit","Offline");online=exit!="Offline";
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   yield return Until(()=>DevelopmentProbe.ReadyToReviewSport&&AppRoot.Instance.view.active,"SPORT_READY",100);
   app=AppRoot.Instance;view=app.view;fishing=app.SelectedSport==SportId.Fishing;role=online?(app.rooms.Host?"host":"guest"):"offline";
   menu=fishing?FishingHUD.Instance.IslandMenu:GolfHUD.Instance.IslandMenu;Check(menu&&menu.GearRect.GetComponent<BasketballPlate>(),"SHARED_SETTINGS_IN_ACTUAL_HUD");
   Check(app.rooms.Connected==online,"REQUESTED_OFFLINE_OR_REAL_NETWORK_CONTEXT");
   int camera=view.mode;float[] saved={MenuPreferences.MasterVolume,MenuPreferences.SFXVolume,MenuPreferences.UIVolume,MenuPreferences.AmbienceVolume};
   try{
    yield return Common();
    if(online){Mark("common");yield return Until(()=>Marked("host","common")&&Marked("guest","common"),"BOTH_PEERS_FINISHED_COMMON_UI",60);}
    yield return MatchChecks();
    if(online){Mark("match");yield return Until(()=>Marked("host","match")&&Marked("guest","match"),"BOTH_PEERS_CHECKED_LIVE_MATCH",60);yield return NetworkExit();}
    else {menu.Open();menu.OpenLeave();yield return null;Click(Button("Confirm leave island"),"OFFLINE_LEAVE");yield return Until(()=>!app.Exploring,"OFFLINE_LEAVE_REACHES_SPORT_SELECTION");Check(!app.rooms.Connected&&!view.InputBlocked,"OFFLINE_EXIT_RELEASES_INPUT");}
   }finally{
    if(menu)menu.Close();view.mode=camera;PlayerPrefs.SetInt("camera",camera);MenuPreferences.SetMaster(saved[0]);MenuPreferences.SetSFX(saved[1]);MenuPreferences.SetUI(saved[2]);MenuPreferences.SetAmbience(saved[3]);MenuPreferences.Save();
   }
   File.AppendAllText(report,"ISLAND_SETTINGS_COMPLETE checks="+checks+"\n");
  }
  IEnumerator Common(){
   yield return Until(()=>Reachable(Button(fishing?"Fishing settings":"Golf settings")),"ARRIVAL_REVEAL_RELEASES_GEAR_INPUT",25);Canvas.ForceUpdateCanvases();Check(menu.FitsSafeFrame(),"GEAR_INSIDE_SAFE_FRAME");
   Click(Button(fishing?"Fishing settings":"Golf settings"),"GEAR");Check(menu.Visible&&view.InputBlocked,"GEAR_OPENS_MODAL_AND_BLOCKS_LOCAL_INPUT");menu.Open();menu.Open();menu.Close();Check(!view.InputBlocked,"REPEATED_OPEN_DOES_NOT_CAPTURE_ITS_OWN_BLOCK");
   view.InputBlocked=true;menu.Open();menu.Close();Check(view.InputBlocked,"CLOSE_PRESERVES_EXISTING_INPUT_BLOCK");view.InputBlocked=false;
   view.stick.value=Vector2.one;PlayerView.LookDelta=Vector2.one*50;app.HandleBack();Check(menu.Visible&&view.InputBlocked&&view.stick.value==Vector2.zero&&PlayerView.LookDelta==Vector2.zero,"ESCAPE_OPENS_SETTINGS_AND_CLEARS_PAD_LOOK");
   var command=view.ReadCommand();Check(command.move==Vector2.zero&&!command.jump&&!command.golfSwing&&command.cartAction==GolfCartAction.None,"MODAL_HAS_NO_MOVEMENT_SHOT_OR_CART_COMMAND");
   Check(!view.BeginGolfAim()&&!view.BeginFishing()&&!view.BeginCountersteer(1,753),"MODAL_REJECTS_NEW_GOLF_AND_FISHING_ACTIONS");
   // Unity refreshes CanvasRenderer culling after a reactivated modal renders.
   // Repeated open/close fixtures above all ran in one update.
   yield return null;
   int savedCamera=PlayerPrefs.GetInt("camera",-1);
   for(int i=0;i<3;i++){Click(Button(new[]{"Eye level","Follow",app.SelectedSport==SportId.Fishing?"Lagoon":"Course"}[i]),"CAMERA_"+i);Check(view.mode==i&&PlayerPrefs.GetInt("camera",-1)==(app.SelectedSport==SportId.Fishing?savedCamera:i),"CAMERA_PREFERENCE_"+i);}
   var sliders=menu.GetComponentsInChildren<Slider>(true);Check(sliders.Length==4,"FOUR_AUDIO_CHANNELS");for(int i=0;i<4;i++)sliders[i].value=.35f+i*.1f;
   Check(Mathf.Abs(MenuPreferences.MasterVolume-.35f)<.001f&&Mathf.Abs(MenuPreferences.SFXVolume-.45f)<.001f&&Mathf.Abs(MenuPreferences.UIVolume-.55f)<.001f&&Mathf.Abs(MenuPreferences.AmbienceVolume-.65f)<.001f,"SLIDERS_CHANGE_SHARED_PREFERENCES");
   Check(HasText(online?"Room play continues":"Play continues"),"HONEST_MODE_SPECIFIC_PLAY_NOTE");Capture("settings");
   menu.OpenLeave();Check(menu.LeaveVisible&&HasText(online?(app.rooms.Host?"Everyone returns":"You leave the room"):"practice score will reset"),"ROLE_SPECIFIC_LEAVE_WARNING");
   Check(Button("Confirm leave island").GetComponentInChildren<Text>().text==(online?(app.rooms.Host?"Return to room":"Leave room"):"Leave island"),"ROLE_SPECIFIC_CONFIRM_ACTION");Capture("leave");app.HandleBack();Check(menu.Visible&&!menu.LeaveVisible&&view.InputBlocked,"BACK_FROM_CONFIRM_STAYS_IN_SETTINGS");
   yield return null;Click(Button(fishing?"Fishing guide":"Golf guide"),"GUIDE");Check((fishing?FishingHUD.Instance.Tutorial.Visible:SportTutorialUI.Find(SportId.Golf).Visible)&&!menu.Visible,"SPORT_SPECIFIC_GUIDE");Capture("guide");app.HandleBack();
   Check(!IslandTutorialUI.ModalOpen&&!view.InputBlocked,"GUIDE_CLOSE_RETURNS_CONTROL");app.HandleBack();Check(menu.Visible,"GUIDE_BACK_PATH_REACHES_SETTINGS");
   app.HandleBack();Check(!menu.Visible&&!view.InputBlocked,"ESCAPE_CLOSES_SETTINGS_WITHOUT_LEAVING_ISLAND");
   menu.Open();Check(sliders.Select((s,i)=>Mathf.Abs(s.value-(.35f+i*.1f))<.001f).All(v=>v),"REOPEN_RETAINS_AUDIO_VALUES");
   var frame=(RectTransform)menu.transform.parent;var oldMin=frame.offsetMin;var oldMax=frame.offsetMax;frame.offsetMin=new Vector2(116,30);frame.offsetMax=new Vector2(-90,-24);yield return null;Canvas.ForceUpdateCanvases();Check(menu.FitsSafeFrame(),"NOTCHED_SAFE_FRAME_FITS_MENU_AND_GEAR");Capture("safe-insets");frame.offsetMin=oldMin;frame.offsetMax=oldMax;yield return null;menu.Close();
   if(!online){
    foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,900),new Vector2Int(1024,768)}){
     Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.35f);menu.Open();yield return new WaitForEndOfFrame();Canvas.ForceUpdateCanvases();Check(menu.FitsSafeFrame(),"RESPONSIVE_MENU_"+size.x+"x"+size.y);Click(Button("Close Island settings"),"CLOSE_"+size.x);Check(!menu.Visible&&!view.InputBlocked,"CLOSE_RESTORES_INPUT_"+size.x);
    }
    Screen.SetResolution(1600,900,false);yield return new WaitForSecondsRealtime(.35f);
   }
  }
  IEnumerator MatchChecks(){
   if(!fishing){
    var match=GolfMatchManager.Instance;
    if(match.Authority)Check(match.StartMatch(),"HOST_OR_OFFLINE_STARTS_GOLF");
    yield return Until(()=>match.State.Running&&!match.Starting&&match.Player(app.LocalAthlete)!=null,"LIVE_GOLF_MATCH_REPLICATED",35);yield return new WaitForSeconds(.6f);
    if(!online){
     Check(view.BeginGolfAim(),"REAL_GOLF_AIM_BEFORE_SETTINGS");yield return Until(()=>view.GolfAiming,"AIM_ACCEPTED");var swing=FindFirstObjectByType<GolfSwingButton>();yield return null;
     var r=(RectTransform)swing.transform;PointerEventData Pointer(float power)=>new(EventSystem.current){pointerId=891,button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(new Vector3(0,Mathf.Lerp(r.rect.height*.385f,-r.rect.height*.34f,power))))};
     swing.OnPointerDown(Pointer(0));swing.OnDrag(Pointer(.7f));Check(swing.Dragging&&view.GolfCharging,"REAL_GOLF_POWER_DRAG_HELD");int strokes=match.Player(app.LocalAthlete).TotalStroke;
     menu.Open();swing.OnPointerUp(Pointer(.7f));yield return new WaitForSeconds(.2f);Check(!swing.Dragging&&!view.GolfAimRequested&&!view.GolfCharging&&match.Player(app.LocalAthlete).TotalStroke==strokes,"OPEN_SETTINGS_CANCELS_GESTURE_WITHOUT_SHOT");
    }else menu.Open();
    uint round=match.Round;double now=match.Now;yield return new WaitForSeconds(.5f);Check(menu.Visible&&match.State.Running&&match.Round==round&&match.Now>now+.2,"GOLF_CONTINUES_WITH_SETTINGS_OPEN");Capture("live-match");Click(Button("Close Island settings"),"CLOSE_ABOVE_LIVE_RANKING");
    var table=FindFirstObjectByType<GolfLeaderboardUI>();yield return null;Click(table.ToggleButton,"EXPAND_RANKING");yield return null;Check(table.Expanded,"RANKING_EXPANDED_BEFORE_SETTINGS");menu.Open();yield return null;Click(Button("Follow"),"CAMERA_ABOVE_EXPANDED_RANKING");Check(menu.Visible&&view.InputBlocked,"EXPANDED_RANKING_CANNOT_STEAL_MODAL_INPUT");menu.Close();
    if(!online){
     table.CloseResults();menu.Open();foreach(var participant in match.State.Players.ToArray())for(int hole=0;hole<GolfPlayerState.HoleCount;hole++)match.State.EnterHole(participant.PlayerId,participant.CurrentHole,match.Now);
     Check(match.State.Phase==GolfMatchPhase.Ended,"CURRENT_SHUFFLED_ROUTE_FINISHES_ALL_PARTICIPANTS");
     yield return null;Check(menu.Visible&&!PostMatchFlow.Instance.Visible&&!FishingCeremony.Presenting,"AWARDS_AND_NEXT_ADVENTURE_WAIT_FOR_EXISTING_SETTINGS");menu.Close();yield return Until(()=>FishingCeremony.Presenting,"FINISHED_GAME_PRESENTS_AWARDS_BEFORE_NEXT_ADVENTURE");
     if(FishingCeremony.Presenting){Check(!PostMatchFlow.Instance.Visible&&view.InputBlocked,"AWARDS_OWN_INPUT_BEFORE_NEXT_ADVENTURE");FishingCeremony.Instance.Cancel();}
     yield return Until(()=>PostMatchFlow.Instance.Visible,"FINISHED_GAME_OPENS_NEXT_ADVENTURE");Check(!menu.Visible&&view.InputBlocked,"RESULTS_TAKE_INPUT_AFTER_CLOSING_SETTINGS");PostMatchFlow.Instance.Close();Check(!view.InputBlocked,"CLOSING_RESULTS_RESTORES_ORIGINAL_INPUT_BLOCK");
    }
   }else{
    var game=FishingGame.Instance;
    view.ReadyFishing();yield return Until(()=>game.Player(app.LocalAthlete).HasValue&&game.Player(app.LocalAthlete).Value.ready,"FISHING_READY_REPLICATED");
    var mode=(FishingMode)Enum.Parse(typeof(FishingMode),Arg("-settingsFishingMode","Round"));
    if(game.Authority){yield return Until(()=>game.State.AllReady,"ALL_FISHING_PLAYERS_READY");Check(game.StartMatch(mode),"HOST_OR_OFFLINE_STARTS_FISHING_"+mode);}
    yield return Until(()=>game.State.Match.phase==FishingRoundPhase.Countdown||game.State.Match.phase==FishingRoundPhase.Playing,"FISHING_START_COUNTDOWN_REPLICATED");
    yield return Until(()=>!FishingHUD.Instance.GetComponent<FishingAbilityHUD>().Modal,"DRAFT_OVERLAY_CLOSED_BEFORE_SETTINGS");
    menu.Open();Check(menu.Visible&&view.InputBlocked,"SETTINGS_AVAILABLE_DURING_FISHING_COUNTDOWN");
    if(online&&!app.rooms.Host){menu.OpenLeave();Check(HasText("ends the match for everyone"),"COUNTDOWN_LEAVE_WARNING");}
    yield return Until(()=>game.State.Match.phase==FishingRoundPhase.Playing,"LIVE_FISHING_MATCH_REPLICATED",35);
    Check(menu.Visible,"MENU_SURVIVES_COUNTDOWN_WITHOUT_PAUSING");yield return null;
    if(online&&!app.rooms.Host){Check(HasText(mode==FishingMode.Crew?"ends Crew Catch":mode==FishingMode.Cup?"Cup result becomes invalid":"remaining players can continue"),"OPEN_CONFIRMATION_UPDATES_AFTER_COUNTDOWN_"+mode);menu.Back();}menu.Close();
    if(!online){
     game.ReviewSnapshot=true;var p=game.State.Players[0];p.phase=FishingPhase.Reeling;p.phaseAt=game.Now;p.sequence++;p.pier=game.NearestPier(app.LocalAthlete);p.fish=0;p.origin=app.LocalAthlete.transform.position;p.lineLength=p.distance=p.castDistance=6;p.tension=.3f;game.State.Players[0]=p;
     Check(view.BeginCountersteer(1,811),"REAL_FISHING_HOLD_QUEUED");view.ReadCommand();menu.Open();var command=view.ReadCommand();Check(!view.FishingHeld&&command.fishingAction==FishingAction.Cancel,"SETTINGS_DRAINS_REAL_FISHING_CANCEL_WHILE_BLOCKED");Check(game.Execute(app.LocalAthlete,command.fishingAction,command.fishingRound,command.fishingSequence,command.fishingFish,command.fishingPressedAt)&&!game.Player(app.LocalAthlete).Value.Busy,"AUTHORITY_ACCEPTS_MODAL_CANCEL");game.ReviewSnapshot=false;
    }else menu.Open();
    uint round=game.State.Match.round;double now=game.Now;yield return new WaitForSeconds(.5f);Check(menu.Visible&&game.State.Match.phase==FishingRoundPhase.Playing&&game.State.Match.round==round&&game.Now>now+.2,"FISHING_CONTINUES_WITH_SETTINGS_OPEN");Capture("live-match");
    if(online&&!app.rooms.Host){menu.OpenLeave();Check(HasText(mode==FishingMode.Crew?"ends Crew Catch":mode==FishingMode.Cup?"Cup result becomes invalid":"remaining players can continue"),"LIVE_MODE_SPECIFIC_GUEST_LEAVE_WARNING_"+mode);menu.Back();}menu.Close();
   }
  }
  IEnumerator NetworkExit(){
   Check(app.rooms.Connected&&FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).Count(n=>n.IsPlayerObject)==2,"BOTH_REAL_PEERS_STILL_CONNECTED_AFTER_SETTINGS");
   if(exit=="Host"){
    if(app.rooms.Host){yield return Until(()=>Marked("guest","exit-open"),"GUEST_MENU_OPEN_BEFORE_HOST_RETURN");menu.Open();menu.OpenLeave();yield return null;Click(Button("Confirm leave island"),"HOST_RETURN_TO_ROOM");}
    else {menu.Open();Mark("exit-open");}
    yield return Until(()=>!app.Exploring,"HOST_RETURN_REPLICATED_TO_BOTH_PEERS");Check(app.rooms.Connected&&!view.InputBlocked&&app.Menu,"HOST_EXIT_RETAINS_ROOM_AND_USABLE_MENU");
   }else{
    if(!app.rooms.Host){menu.Open();menu.OpenLeave();yield return null;Click(Button("Confirm leave island"),"GUEST_LEAVE_ROOM");yield return Until(()=>!app.rooms.Connected&&!app.Exploring,"GUEST_LEAVE_REACHES_SPORT_SELECTION");Check(!view.InputBlocked&&app.Menu,"GUEST_EXIT_RELEASES_MODAL_INPUT");}
    else {menu.Open();yield return Until(()=>FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).Count(n=>n.IsPlayerObject)==1,"HOST_OBSERVES_GUEST_DISCONNECT");Check(app.rooms.Connected&&app.Exploring,"GUEST_LEAVING_PRESERVES_HOST_ROOM");yield return null;menu.Close();app.LeaveCourt();yield return Until(()=>!app.Exploring,"HOST_CAN_RETURN_AFTER_GUEST_LEAVES");Check(app.rooms.Connected&&!view.InputBlocked,"HOST_ROOM_REMAINS_USABLE");}
   }
  }
  void Capture(string name){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
   try{foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=Camera.main;c.planeDistance=.5f;}Canvas.ForceUpdateCanvases();FindFirstObjectByType<DevelopmentProbe>()?.CaptureFrame(Path.Combine(Path.GetDirectoryName(report),name+".png"));}
   finally{for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=RenderMode.ScreenSpaceOverlay;canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}}
  }
 }
}
#endif
