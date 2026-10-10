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
 // Explicit opt-in presentation review. Synthetic score rows never run in normal play.
 public sealed class GolfHUDReview:MonoBehaviour {
  string report;int checks;AppRoot app;GolfMatchManager match;GolfHUD hud;GolfLeaderboardUI table;GolfSwingButton swing;Athlete actor;GolfBall ball;RectTransform safe;
  bool insetActive,savedAppEnabled;Vector2 savedMin,savedMax,savedOffsetMin,savedOffsetMax;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){if(Environment.GetCommandLineArgs().Contains("-golfHudAudit"))new GameObject("Golf HUD presentation review").AddComponent<GolfHUDReview>();}
  void Record(string value){File.AppendAllText(report,value+Environment.NewLine);Debug.Log("GOLF_HUD "+value);}
  void Check(bool value,string name){Record((value?"PASS ":"FAIL ")+name);if(!value)throw new Exception(name);checks++;}
  static IEnumerator Frame(){if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)yield return null;else yield return new WaitForEndOfFrame();}
  Text FindText(string name)=>table.GetComponentInParent<Canvas>().GetComponentsInChildren<Text>(true).FirstOrDefault(t=>t.name==name);
  static bool Overlaps(RectTransform a,RectTransform b){var ac=new Vector3[4];var bc=new Vector3[4];a.GetWorldCorners(ac);b.GetWorldCorners(bc);var ar=new Rect(ac[0].x,ac[0].y,ac[2].x-ac[0].x,ac[2].y-ac[0].y);var br=new Rect(bc[0].x,bc[0].y,bc[2].x-bc[0].x,bc[2].y-bc[0].y);return ar.Overlaps(br);}
  void RoutedPress(Button button,PointerEventData pointer,string tag){
   pointer.position=RectTransformUtility.WorldToScreenPoint(null,button.transform.position);var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
   Check(hits.Count>0&&hits[0].gameObject==button.gameObject,"ACTUAL_RAYCAST_REACHES_FIXED_BUTTON_"+tag);
   var image=button.GetComponent<Image>();Check(image&&image.enabled&&image.raycastTarget&&image.color.a==0,"TRANSPARENT_FIXED_TOUCH_TARGET_"+tag);
   ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerDownHandler);
  }
  void CheckLayout(string tag){
   Canvas.ForceUpdateCanvases();Check(hud.FitsSafeFrame()&&table.FitsSafeFrame(),"ALL_GOLF_FRAMES_INSIDE_SAFE_AREA_"+tag);
   Check(hud.RegionsSeparate()&&!Overlaps(table.BoardRect,(RectTransform)hud.CameraButton.transform)&&!Overlaps(table.BoardRect,(RectTransform)hud.JumpButton.transform),"SCORECARD_AND_TOUCH_CONTROLS_SEPARATE_"+tag);
   Check(hud.TypographyClean()&&table.RowsReadable(),"LIVE_NATIVE_TEXT_HAS_READABLE_GEOMETRY_"+tag);Check(table.DividersVisible(),"ALL_FOUR_COLUMN_DIVIDERS_VISIBLE_"+tag);Check(FindText("Golf hint !").cachedTextGenerator.vertexCount>0,"NATIVE_HINT_ATTENTION_MARK_RENDERS_"+tag);
   var cells=table.BoardRect.GetComponentsInChildren<Text>().Where(t=>t.name.StartsWith("Player ")).ToArray();
   Check(cells.Length==50&&cells.All(t=>t.fontStyle==FontStyle.Normal&&(t.font==CoveUI.TextFont||t.font==CoveUI.DisplayFont)&&t.cachedTextGenerator.vertexCount>0),"TEN_PLAYER_FIFTY_LIVE_CELLS_RENDER_SHARP_"+tag);
  }
  Vector3 Floor(Vector3 at){if(Physics.Raycast(at+Vector3.up*4,Vector3.down,out var hit,8,1<<8,QueryTriggerInteraction.Ignore))return hit.point;throw new Exception("Golf presentation fixture has no floor");}
  void Place(Vector3 at){actor.capsule.enabled=false;actor.transform.position=Floor(at)+Vector3.up*.035f;actor.ResetLocomotion();actor.capsule.enabled=true;Physics.SyncTransforms();}
  void Insets(Vector2 lower,Vector2 upper){
   if(insetActive)RestoreInsets();savedAppEnabled=app.enabled;app.enabled=false;savedMin=safe.anchorMin;savedMax=safe.anchorMax;savedOffsetMin=safe.offsetMin;savedOffsetMax=safe.offsetMax;
   safe.anchorMin=savedMin+new Vector2(lower.x/Screen.width,lower.y/Screen.height);safe.anchorMax=savedMax-new Vector2(upper.x/Screen.width,upper.y/Screen.height);safe.offsetMin=safe.offsetMax=Vector2.zero;insetActive=true;
  }
  void RestoreInsets(){if(!insetActive)return;if(safe){safe.anchorMin=savedMin;safe.anchorMax=savedMax;safe.offsetMin=savedOffsetMin;safe.offsetMax=savedOffsetMax;}if(app)app.enabled=savedAppEnabled;insetActive=false;}
  void Capture(string label){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
   string path=Path.ChangeExtension(report,label+".png");var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var camera=Camera.main;
   try{
    foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=.5f;}
    // The shared helper fixes 1600x900. Other aspect ratios need their actual tested dimensions.
    if(Screen.width==1600&&Screen.height==900)FindFirstObjectByType<DevelopmentProbe>()?.CaptureFrame(path);
    else {
     var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=camera.targetTexture;var active=RenderTexture.active;var texture=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
     try{camera.targetTexture=target;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());}
     finally{camera.targetTexture=previous;RenderTexture.active=active;Destroy(texture);RenderTexture.ReleaseTemporary(target);}
    }
   }finally{for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=RenderMode.ScreenSpaceOverlay;canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}Canvas.ForceUpdateCanvases();}
   Record("CAPTURE "+Path.GetFileName(path)+" dimensions="+Screen.width+"x"+Screen.height);
  }
  IEnumerator Start(){
   var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-report");report=index>=0&&index+1<args.Length?args[index+1]:Path.Combine(Application.persistentDataPath,"golf-hud.txt");Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(report)));File.WriteAllText(report,"");
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   float until=Time.realtimeSinceStartup+65;while((!AppRoot.Instance||!AppRoot.Instance.Exploring||AppRoot.Instance.SelectedSport!=SportId.Golf||!GolfMatchManager.Instance||!GolfHUD.Instance||!FindFirstObjectByType<GolfLeaderboardUI>())&&Time.realtimeSinceStartup<until)yield return null;
   app=AppRoot.Instance;match=GolfMatchManager.Instance;hud=GolfHUD.Instance;table=FindFirstObjectByType<GolfLeaderboardUI>();swing=FindFirstObjectByType<GolfSwingButton>();actor=app?app.LocalAthlete:null;
   Check(app&&match&&hud&&table&&swing&&actor&&match.Context&&match.Authority&&!app.rooms.Connected,"OFFLINE_GOLF_HUD_READY");Check(SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null,"REVIEW_USES_RENDERED_PLAYER");
   safe=table.GetComponentInParent<Canvas>().GetComponentsInChildren<RectTransform>(true).First(r=>r.name=="Safe area");yield return new WaitForSeconds(.6f);yield return Frame();
   Check(match.State.Phase==GolfMatchPhase.Idle&&swing.startButton.gameObject.activeInHierarchy&&swing.startButton.interactable&&!table.CountdownVisible,"IDLE_SHOWS_START_WITHOUT_ORDINARY_TIMER");
   var carts=FindObjectsByType<GolfCartButton>(FindObjectsSortMode.None);Check(carts.Length==2&&carts.First(c=>c.summon).button.interactable,"ROAMING_SUMMON_CONTROL_AVAILABLE");Capture("idle-course");
   swing.startButton.onClick.Invoke();yield return new WaitForSeconds(.2f);Check(match.State.Running&&match.Player(actor)!=null&&match.Ball(actor),"EXISTING_START_BUTTON_STARTS_REAL_MATCH");ball=match.Ball(actor);ulong localId=match.Player(actor).PlayerId;
   var tee=match.Course.holes[(int)(ball.Owner%5)].tee;ball.Place(tee.position+Vector3.up*(GolfBall.Radius+.01f),true);Place(tee.position-tee.forward*2.6f);app.view.mode=1;app.view.yaw=tee.eulerAngles.y;app.view.pitch=24;yield return new WaitForSeconds(.7f);
   // Only score presentation is synthetic. The local player keeps the real actor/ball mapping.
   var roster=new[]{new GolfEntrant(localId,"YOU")}.Concat(Enumerable.Range(1,9).Select(i=>new GolfEntrant((ulong)i,i==9?"Long Teammate Display Name":"Friend "+i))).ToArray();match.State.Clear();Check(match.State.Start(roster,match.Now),"SYNTHETIC_TEN_PLAYER_SCORECARD_STARTED");
   for(int i=1;i<10;i++){ulong id=(ulong)i;for(int shot=0;shot<i*2;shot++)match.State.RecordSwing(id,match.Now);for(int hole=1;hole<=i%4;hole++)match.State.EnterHole(id,hole,match.Now);}
   Record("FIXTURE scorecard_only=10 local_actor_and_ball=real additional_world_players=0");yield return null;yield return Frame();Check(match.State.Players.Count==10&&!table.CountdownVisible,"TEN_INDEPENDENT_PROGRESS_ROWS_HAVE_NO_TIMER_BEFORE_FINISH");CheckLayout("1600x900-roaming");Capture("ten-player-scorecard");
   Check(swing.aimButton.interactable&&!swing.button.interactable,"EXISTING_AIM_ENTRY_AND_SHOT_ELIGIBILITY_PRESERVED");swing.aimButton.onClick.Invoke();until=Time.realtimeSinceStartup+3;while(!app.view.GolfAiming&&Time.realtimeSinceStartup<until)yield return null;yield return null;yield return Frame();
   Check(app.view.GolfAiming&&swing.button.interactable&&swing.aimButton.interactable&&swing.aimLabel.text.StartsWith("Cancel",StringComparison.OrdinalIgnoreCase),"AIM_SWITCHES_TO_EXISTING_CANCEL_AND_ENABLES_SWING");
   Check(!hud.CameraButton.interactable&&!hud.JumpButton.interactable&&carts.All(c=>c.GetComponent<CanvasGroup>().alpha==0&&!c.GetComponent<CanvasGroup>().blocksRaycasts),"AIM_DISABLES_CAMERA_JUMP_AND_HIDES_CART_INPUT");
   var pointer=new PointerEventData(EventSystem.current){pointerId=63,button=PointerEventData.InputButton.Left};int before=match.Player(actor).TotalStroke;RoutedPress(swing.button,pointer,"SWING");yield return new WaitForSeconds(.9f);yield return Frame();
   Check(app.view.GolfCharging&&app.view.GolfDisplayedCharge>0&&swing.label.text.Contains("%"),"REAL_POINTER_HOLD_UPDATES_LIVE_POWER_CAPTION");Capture("hold-and-release");ExecuteEvents.Execute(swing.gameObject,pointer,ExecuteEvents.pointerExitHandler);yield return null;Check(!app.view.GolfCharging&&app.view.GolfAiming&&match.Player(actor).TotalStroke==before,"POINTER_EXIT_CANCELS_CHARGE_WITHOUT_STROKE");
   foreach(var size in new[]{new[]{1600,900},new[]{1280,960},new[]{1920,900}}){
    Screen.SetResolution(size[0],size[1],false);yield return new WaitForSeconds(.6f);yield return Frame();Check(Screen.width==size[0]&&Screen.height==size[1],"REQUESTED_LANDSCAPE_RESOLUTION_APPLIED_"+size[0]+"x"+size[1]);
    Insets(new Vector2(48,24),new Vector2(24,16));yield return null;yield return null;yield return Frame();CheckLayout(size[0]+"x"+size[1]+"-insets");Capture("layout-"+size[0]+"x"+size[1]+"-insets");RestoreInsets();yield return null;
   }
   Screen.SetResolution(1600,900,false);yield return new WaitForSeconds(.6f);yield return Frame();Check(app.view.GolfAiming&&swing.button.interactable,"RESPONSIVE_LAYOUT_PRESERVES_AIM_INPUT");RoutedPress(swing.button,pointer,"RELEASE");yield return new WaitForSeconds(.6f);ExecuteEvents.Execute(swing.gameObject,pointer,ExecuteEvents.pointerUpHandler);yield return new WaitForSeconds(.4f);Check(match.Player(actor).TotalStroke==before+1&&!app.view.GolfAimRequested,"REAL_RELEASE_COUNTS_EXACTLY_ONE_STROKE");
   double finishAt=match.Now;Finish(1,finishAt,5);Finish(2,finishAt,5);yield return null;yield return Frame();
   Check(match.State.Phase==GolfMatchPhase.FinalCountdown&&table.CountdownVisible&&FindText("First finisher notice").text=="A player has finished!"&&match.State.CountdownDeadline==finishAt+GolfMatchState.FinalCountdownSeconds,"FIRST_FINISH_SHOWS_EXISTING_NOTICE_AND_SHARED_THIRTY_SECONDS");Capture("final-countdown");
   match.State.Advance(match.State.CountdownDeadline);yield return null;yield return Frame();var results=match.State.Results();
   Check(results[0].rank==1&&results[1].rank==1&&results[0].tie&&results[1].tie&&results.Skip(2).All(r=>r.player.IsDNF&&r.rank==0),"RESULTS_RETAIN_SHARED_RANK_AND_UNRANKED_DNF");
   Check(FindText("Player 0 Player").text.StartsWith("1= ")&&FindText("Player 1 Player").text.StartsWith("1= ")&&FindText("Player 2 Status").text=="DNF"&&table.CountdownText=="00:00"&&FindText("First finisher notice").text=="Match finished","RESULT_ROWS_AND_EXPIRED_TIMER_MATCH_GAMEPLAY_STATE");CheckLayout("results-ten-players");Capture("results-ties-dnf");
   match.ClearMatch();yield return null;Check(match.StartMatch(),"REVIEW_RESTORES_REAL_SINGLE_PLAYER_MATCH");yield return null;yield return Frame();Check(match.State.Players.Count==1&&!table.CountdownVisible,"RESTART_REMOVES_SYNTHETIC_ROWS_AND_OLD_COUNTDOWN");
   Record("GOLF_HUD_COMPLETE checks="+checks);Application.Quit();
  }
  void Finish(ulong id,double now,int strokes){var player=match.State.Player(id);for(int i=player.TotalStroke;i<strokes;i++)match.State.RecordSwing(id,now);for(int hole=player.CurrentHole;hole<=5;hole++)match.State.EnterHole(id,hole,now);}
  void OnDestroy(){RestoreInsets();}
 }
}
#endif
