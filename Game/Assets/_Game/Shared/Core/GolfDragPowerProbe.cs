#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
namespace WhatTheFish {
 public sealed class GolfDragPowerProbe:MonoBehaviour {
  string report;int checks;AppRoot app;GolfMatchManager match;Athlete actor;GolfBall ball;GolfSwingButton ui;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){if(Environment.GetCommandLineArgs().Contains("-golfDragPowerAudit"))new GameObject("Golf drag power review").AddComponent<GolfDragPowerProbe>();}
  void Check(bool pass,string name){File.AppendAllText(report,(pass?"PASS ":"FAIL ")+name+"\n");if(!pass)throw new Exception(name);checks++;}
  void PlaceActor(Vector3 at){bool enabled=actor.capsule.enabled;actor.capsule.enabled=false;actor.transform.position=at;actor.ResetLocomotion();actor.capsule.enabled=enabled;Physics.SyncTransforms();}
  IEnumerator Aim(){Check(app.view.BeginGolfAim(),"AIM_REQUEST");float until=Time.time+3;while(!app.view.GolfAiming&&Time.time<until)yield return null;yield return new WaitForSeconds(.25f);Check(app.view.GolfAiming&&ui.button.interactable,"AIM_ENABLED_DRAG");}
  PointerEventData Pointer(int id,float power){var r=(RectTransform)ui.transform;var local=new Vector3(0,Mathf.Lerp(r.rect.height*.385f,-r.rect.height*.34f,power));return new PointerEventData(EventSystem.current){pointerId=id,button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(local))};}
  void Capture(string name){
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
   try{foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=Camera.main;c.planeDistance=.5f;}Canvas.ForceUpdateCanvases();FindFirstObjectByType<DevelopmentProbe>()?.CaptureFrame(Path.Combine(Path.GetDirectoryName(report),name+".png"));}
   finally{for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=RenderMode.ScreenSpaceOverlay;canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}}
  }
  IEnumerator Start(){
   var args=Environment.GetCommandLineArgs();report=args[Array.IndexOf(args,"-report")+1];Directory.CreateDirectory(Path.GetDirectoryName(report));DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   float until=Time.realtimeSinceStartup+100;while((!DevelopmentProbe.ReadyToReviewSport||!GolfMatchManager.Instance||!GolfMatchManager.Instance.Context)&&Time.realtimeSinceStartup<until){DevelopmentProbe.Driving=false;yield return null;}
   app=AppRoot.Instance;match=GolfMatchManager.Instance;actor=app.LocalAthlete;Check(match&&actor&&match.Context,"GOLF_CONTEXT");
   bool started=false;until=Time.time+18;while(!started&&Time.time<until){started=match.StartTestMatch(new[]{(GolfCartWorld.Key(actor),actor,"Drag player")});if(!started)yield return null;}Check(started,"FOCUSED_MATCH_STARTED");yield return new WaitForSeconds(.7f);
   ball=match.Ball(actor);ui=FindFirstObjectByType<GolfSwingButton>();Check(ui&&ui.Knob&&ui.Knob.GetComponent<UnityEngine.UI.Image>().sprite&&ui.transform.Find("Golf power rail").GetComponent<UnityEngine.UI.Image>().sprite,"SLIDER_SPRITES_LOADED");
   Check(ui.transform.Find("Golf button visual")==null,"OLD_HOLD_BUTTON_REMOVED");Check(!ui.button.interactable&&!app.view.BeginGolfSwing(88),"DRAG_REQUIRES_AIM");
   var origin=ball.Body.position;app.view.yaw=Quaternion.LookRotation(match.Course.holes[0].cup.position-origin).eulerAngles.y;app.view.pitch=24;
   yield return Aim();Check(app.view.GolfMode==GolfShotMode.Swing,"SWING_MODE_RETAINED");yield return new WaitForEndOfFrame();
   Check(ui.Power==0&&ui.Knob.anchoredPosition.y>100,"ZERO_POWER_BALL_AT_GREEN_TOP");Check(GolfHUD.Instance.FitsSafeFrame()&&GolfHUD.Instance.RegionsSeparate(),"SLIDER_LAYOUT_FITS_AND_SEPARATES");Check(app.view.GolfPreview&&app.view.GolfPreview.Visible,"EXISTING_PREVIEW_PRESENT");Check(Vector3.Distance(app.view.GolfPreview.LaunchVelocity,ball.PhysicsSettings.SwingVelocity(Quaternion.Euler(0,app.view.yaw,0)*Vector3.forward,.3f,app.view.GolfMode))<.002f,"IDLE_SWING_PREVIEW_THIRTY_PERCENT");Capture("swing-idle-thirty");
   int strokes=match.Player(actor).TotalStroke;
   ui.OnPointerDown(Pointer(10,0));yield return new WaitForSeconds(.7f);Check(ui.Dragging&&app.view.GolfCharge==0,"HOLDING_WITHOUT_DRAG_NEVER_CHARGES");Check(Vector3.Distance(app.view.GolfPreview.LaunchVelocity,ball.PhysicsSettings.SwingVelocity(Quaternion.Euler(0,app.view.yaw,0)*Vector3.forward,.3f,app.view.GolfMode))<.002f,"TOUCH_WITHOUT_DRAG_KEEPS_THIRTY_PREVIEW");ui.OnPointerUp(Pointer(10,0));yield return new WaitForSeconds(.2f);Check(match.Player(actor).TotalStroke==strokes,"TAP_WITHOUT_PULL_NEVER_FIRES");Check(Vector3.Distance(app.view.GolfPreview.LaunchVelocity,ball.PhysicsSettings.SwingVelocity(Quaternion.Euler(0,app.view.yaw,0)*Vector3.forward,.3f,app.view.GolfMode))<.002f,"TAP_RETURNS_TO_THIRTY_PREVIEW");
   ui.OnPointerDown(Pointer(11,0));ui.OnDrag(Pointer(11,.6f));yield return new WaitForEndOfFrame();Check(Mathf.Abs(app.view.GolfCharge-.6f)<.002f,"DRAG_POSITION_SETS_POWER");
   yield return new WaitForSeconds(1.2f);Check(Mathf.Abs(app.view.GolfCharge-.6f)<.002f,"POWER_DOES_NOT_CYCLE_OVER_TIME");
   ui.OnDrag(Pointer(12,1));ui.OnPointerUp(Pointer(12,1));Check(ui.Dragging&&Mathf.Abs(ui.Power-.6f)<.002f,"SECOND_POINTER_IGNORED");
   ui.OnDrag(Pointer(11,.2f));Check(Mathf.Abs(app.view.GolfCharge-.2f)<.002f,"DRAG_UP_REDUCES_POWER");ui.OnDrag(Pointer(11,.6f));yield return new WaitForEndOfFrame();
   var expected=ball.PhysicsSettings.SwingVelocity(Quaternion.Euler(0,app.view.yaw,0)*Vector3.forward,.6f,app.view.GolfMode);Check(Vector3.Distance(expected,app.view.GolfPreview.LaunchVelocity)<.002f,"SWING_PREVIEW_MATCHES_DRAG_POWER");Capture("swing-sixty");
   ui.OnPointerExit(Pointer(11,.6f));Check(ui.Dragging,"POINTER_CAPTURE_SURVIVES_LEAVING_RAIL");ui.OnPointerUp(Pointer(11,.6f));ui.OnPointerUp(Pointer(11,.6f));yield return new WaitForSeconds(.7f);
   Check(match.Player(actor).TotalStroke==strokes+1,"RELEASE_FIRES_EXACTLY_ONCE");Check(Mathf.Abs(actor.GolfClubMotion.State.charge-.6f)<.002f,"AUTHORITY_RECEIVES_SELECTED_POWER");Check(ui.Power==0&&!ui.Dragging,"SLIDER_RESETS_AFTER_SHOT");
   ball.Place(origin,true);PlaceActor(origin-Vector3.forward*1.8f);yield return new WaitForSeconds(.7f);yield return Aim();ui.OnPointerDown(Pointer(21,0));ui.OnDrag(Pointer(21,.7f));app.view.CancelGolfAim();yield return null;ui.OnPointerUp(Pointer(21,.7f));yield return new WaitForSeconds(.2f);Check(!ui.Dragging&&match.Player(actor).TotalStroke==strokes+1,"CANCEL_AIM_DOES_NOT_FIRE");
   var cup=match.Course.holes[0].cup.position;var at=cup+Vector3.back*4;if(Physics.Raycast(at+Vector3.up*8,Vector3.down,out var hit,20,1<<8,QueryTriggerInteraction.Ignore))at=hit.point+Vector3.up*GolfBall.Radius;ball.Place(at,true);PlaceActor(at+Vector3.back*1.8f);app.view.yaw=0;yield return new WaitForSeconds(.7f);yield return Aim();Check(app.view.GolfMode==GolfShotMode.Putt,"NEAR_HOLE_PUTT_RETAINED");Check(Vector3.Distance(app.view.GolfPreview.LaunchVelocity,ball.PhysicsSettings.SwingVelocity(Vector3.forward,.3f,GolfShotMode.Putt))<.002f,"IDLE_PUTT_PREVIEW_THIRTY_PERCENT");
   ui.OnPointerDown(Pointer(31,0));ui.OnDrag(Pointer(31,.08f));yield return new WaitForEndOfFrame();expected=ball.PhysicsSettings.SwingVelocity(Vector3.forward,.08f,GolfShotMode.Putt);Check(Mathf.Abs(ui.Power-.08f)<.002f&&Vector3.Distance(expected,app.view.GolfPreview.LaunchVelocity)<.002f,"FINE_LOW_POWER_PUTT_PREVIEW");Capture("putt-eight");ui.OnPointerUp(Pointer(31,.08f));yield return new WaitForSeconds(.7f);Check(match.Player(actor).TotalStroke==strokes+2,"PUTT_RELEASE_FIRES_ONCE");
   Check(GolfSwingButton.ArrowLifetime==.5f&&GolfSwingButton.HintAlpha(.25f,0)>.9f&&GolfSwingButton.HintAlpha(.51f,0)==0&&GolfSwingButton.HintAlpha(1.55f,0)>.9f,"HINT_HALF_SECOND_LIFETIME_AND_REPEAT");Check(GolfSwingButton.HintAlpha(.1f,0)>0&&GolfSwingButton.HintAlpha(.1f,1)==0&&GolfSwingButton.HintAlpha(.26f,1)>0,"HINTS_STAGGER_DOWNWARD");
   File.AppendAllText(report,"GOLF_DRAG_POWER_COMPLETE checks="+checks+"\n");
  }
 }
}
#endif
