#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 public sealed class GolfAimProbe:MonoBehaviour {
  string report;int checks;AppRoot app;GolfMatchManager match;Athlete actor;GolfBall ball;GolfSwingButton ui;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){if(Environment.GetCommandLineArgs().Contains("-golfAimAudit"))new GameObject("Golf aim interaction checks").AddComponent<GolfAimProbe>();}
  void Check(bool value,string name){File.AppendAllText(report,(value?"PASS ":"FAIL ")+name+"\n");if(!value)throw new Exception(name);checks++;}
  static IEnumerator Frame(){if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)yield return null;else yield return new WaitForEndOfFrame();}
  Vector3 Ground(Vector3 p){if(Physics.Raycast(p+Vector3.up*3,Vector3.down,out var hit,6,1<<8,QueryTriggerInteraction.Ignore))return hit.point+Vector3.up*.035f;throw new Exception("Missing test ground");}
  void Place(Athlete a,Vector3 p){a.capsule.enabled=false;a.transform.position=Ground(p);a.ResetLocomotion();var net=a.GetComponent<NetworkTransform>();if(net&&net.IsSpawned&&net.IsServer)net.Teleport(a.transform.position,a.transform.rotation,Vector3.one);a.capsule.enabled=match.Authority;Physics.SyncTransforms();}
  IEnumerator Aim(){ui.aimButton.onClick.Invoke();float until=Time.time+3;while(!app.view.GolfAiming&&Time.time<until)yield return null;yield return new WaitForSeconds(.15f);Check(app.view.GolfAiming&&match.IsAiming(actor),"AIM_ACKNOWLEDGED_AND_AUTHORITY_LOCKED");}
  void Capture(string name){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
   try{foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=Camera.main;c.planeDistance=.5f;}FindFirstObjectByType<DevelopmentProbe>()?.CaptureFrame(Path.ChangeExtension(report,name+".png"));}
   finally{for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=RenderMode.ScreenSpaceOverlay;canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}}
  }
  IEnumerator Start(){
   var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-report");report=index>=0?args[index+1]:Path.Combine(Application.persistentDataPath,"golf-aim.txt");
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   float until=Time.time+65;while((!AppRoot.Instance||!AppRoot.Instance.Exploring||!GolfMatchManager.Instance)&&Time.time<until)yield return null;
   app=AppRoot.Instance;match=GolfMatchManager.Instance;actor=app.LocalAthlete;Check(match&&actor&&match.Context,"GOLF_CONTEXT");
   if(match.Authority){Check(match.StartMatch(),"MATCH_STARTED");foreach(var player in Athlete.Active.Where(a=>match.Player(a)!=null).ToArray()){var b=match.Ball(player);var tee=match.Course.holes[(int)(b.Owner%5)].tee;b.Place(tee.position+Vector3.up*(GolfBall.Radius+.01f),true);Place(player,tee.position-tee.forward*2.6f);}}
   until=Time.time+10;while((!match.State.Running||!match.Ball(actor)||Vector3.Distance(actor.transform.position,match.Ball(actor).Body.position)>3)&&Time.time<until)yield return null;
   ball=match.Ball(actor);yield return new WaitForSeconds(.7f);ui=FindFirstObjectByType<GolfSwingButton>();Check(ball&&ui&&ui.aimButton.interactable,"NEARBY_AIM_AVAILABLE_BEYOND_OLD_SWING_RANGE");
   Check(!ui.button.interactable&&!app.view.BeginGolfSwing(),"SWING_REQUIRES_EXPLICIT_AIM");
   app.view.mode=1;app.view.yaw=match.Course.holes[(int)(ball.Owner%5)].tee.eulerAngles.y;app.view.pitch=24;
   yield return Aim();Check(app.view.AimedGolfOwner==ball.Owner&&Vector3.ProjectOnPlane(actor.transform.position-ball.Body.position,Vector3.up).magnitude<.95f,"SAFE_STANCE_NEAR_SELECTED_BALL");
   var feet=actor.transform.position;DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.one,sprint=true,jump=true,heading=app.view.yaw};
   app.view.RequestJump();app.view.RequestCartToggle();yield return new WaitForSeconds(.6f);
   Check(Vector3.ProjectOnPlane(actor.transform.position-feet,Vector3.up).magnitude<.015f&&!actor.Airborne&&!actor.LoadingJump&&!GolfCartWorld.Driving(actor),"MOVEMENT_JUMP_AND_CART_INPUT_CANNOT_BREAK_AIM");
   if(match.Authority){actor.Simulate(new PlayerCommand{move=Vector2.one,jump=true,sprint=true},.2f);Check(Vector3.ProjectOnPlane(actor.transform.position-feet,Vector3.up).magnitude<.015f&&!actor.LoadingJump,"AUTHORITY_REJECTS_DIRECT_MOVEMENT_AND_JUMP_WHILE_LOCKED");}
   DevelopmentProbe.TurnCommand=default;yield return Frame();Check(app.view.GolfPreview&&app.view.GolfPreview.Visible&&app.view.GolfPreview.PointCount>2,"GOLF_PREVIEW_VISIBLE_BEFORE_CHARGE");Capture("aim-putt");
   float originalYaw=app.view.yaw;var end=app.view.GolfPreview.EndPoint;app.view.yaw+=35;yield return new WaitForSeconds(.2f);yield return Frame();Check(Vector3.Distance(end,app.view.GolfPreview.EndPoint)>.3f&&Vector3.ProjectOnPlane(actor.transform.position-feet,Vector3.up).magnitude<.015f,"ROTATING_AIM_CHANGES_PATH_WITHOUT_MOVING_FEET");app.view.yaw=originalYaw;
   int strokes=match.Player(actor).TotalStroke;Check(app.view.BeginGolfSwing(41),"CHARGE_FROM_LOCKED_STANCE");yield return new WaitForSeconds(.4f);app.view.CancelGolfAim();yield return new WaitForSeconds(.35f);
   Check(!match.IsAiming(actor)&&!app.view.GolfCharging&&!app.view.GolfPreview.Visible&&match.Player(actor).TotalStroke==strokes,"CANCEL_DURING_CHARGE_UNLOCKS_HIDES_GUIDE_AND_COSTS_NO_STROKE");
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,heading=originalYaw+90};yield return new WaitForSeconds(.35f);DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.2f);Check(Vector3.ProjectOnPlane(actor.transform.position-feet,Vector3.up).magnitude>.15f,"CANCEL_RESTORES_WALKING");
   yield return Aim();
   var pointer=new PointerEventData(EventSystem.current){pointerId=27,button=PointerEventData.InputButton.Left};ui.OnPointerDown(pointer);yield return new WaitForSeconds(.95f);yield return Frame();
   Check(app.view.GolfCharging&&app.view.GolfCharge>.65f&&app.view.GolfPreview.LaunchVelocity.y>1,"TOUCH_HOLD_SHOWS_LOFTED_SHOT");
   var expected=ball.PhysicsSettings.SwingVelocity(Quaternion.Euler(0,app.view.yaw,0)*Vector3.forward,app.view.GolfDisplayedCharge,app.view.GolfMode);Check(Vector3.Distance(expected,app.view.GolfPreview.LaunchVelocity)<.001f,"PREVIEW_USES_DISPLAYED_RELEASE_PROFILE");Capture("aim-chip");
   ui.OnPointerUp(new PointerEventData(EventSystem.current){pointerId=99,button=PointerEventData.InputButton.Left});Check(app.view.GolfCharging,"UNRELATED_POINTER_CANNOT_RELEASE_SWING");
   ui.OnPointerUp(pointer);yield return new WaitForSeconds(.4f);Check(match.Player(actor).TotalStroke==strokes+1&&!app.view.GolfAimRequested&&!match.IsAiming(actor),"TOUCH_RELEASE_SWINGS_ONCE_AND_RELEASES_STANCE");
   Check(Vector3.ProjectOnPlane(ball.Body.position-ball.LastShotPosition,Vector3.up).magnitude>.5f&&!app.view.GolfPreview.Visible,"SHOT_TRAVELS_AND_PREVIEW_CLEARS");
   if(!app.rooms.Connected)yield return OfflineEdges();else yield return NetworkPutt();
   File.AppendAllText(report,"GOLF_AIM_COMPLETE checks="+checks+"\n");
  }
  IEnumerator NetworkPutt(){
   if(match.Authority){
    yield return new WaitForSeconds(2);
    foreach(var player in Athlete.Active.Where(a=>match.Player(a)!=null).ToArray()){
     var b=match.Ball(player);var cup=match.Target(player).cup.position;var side=b.Owner%2==0?Vector3.right:Vector3.left;
     b.Place(Ground(cup+side*2)+Vector3.up*(GolfBall.Radius-.033f),true);Place(player,cup+side*2-Vector3.forward*2.5f);
    }
   }
   float until=Time.time+10;while((Vector3.ProjectOnPlane(ball.Body.position-match.Target(actor).cup.position,Vector3.up).magnitude>10||!app.view.CanAimGolf)&&Time.time<until)yield return null;
   app.view.yaw=ball.Owner%2==0?90:-90;yield return new WaitForSeconds(.5f);yield return Aim();
   Check(app.view.GolfMode==GolfShotMode.Putt&&ui.label.text.StartsWith("Putt"),"NETWORK_AUTHORITY_SELECTS_AND_REPLICATES_PUTT_MODE");
   int strokes=match.Player(actor).TotalStroke;var pointer=new PointerEventData(EventSystem.current){pointerId=49,button=PointerEventData.InputButton.Left};ui.OnPointerDown(pointer);yield return new WaitForSeconds(.5f);yield return Frame();
   var endpoint=app.view.GolfPreview.EndPoint;Check(Mathf.Abs(app.view.GolfPreview.LaunchVelocity.y)<.001f,"NETWORK_PUTT_GUIDE_STAYS_ON_GROUND");Capture("network-putt");ui.OnPointerUp(pointer);
   yield return new WaitForSeconds(4);float error=Vector3.Distance(endpoint,ball.Body.position);File.AppendAllText(report,$"NETWORK_PUTT_ENDPOINT_ERROR metres={error:F5}\n");
   Check(error<.08f&&match.Player(actor).TotalStroke==strokes+1,"NETWORK_PUTT_STOPS_AT_DISPLAYED_TARGET_AND_COUNTS_ONCE");
  }
  IEnumerator OfflineEdges(){
   yield return new WaitForSeconds(.7f);var tee=match.Course.holes[0].tee;ball.Place(tee.position+Vector3.up*.09f,true);Place(actor,tee.position-tee.forward*3.4f);yield return new WaitForSeconds(.4f);
   Check(!app.view.CanAimGolf&&!ui.aimButton.interactable,"DISTANT_BALL_DOES_NOT_ALLOW_TELEPORTING");
   for(int mode=0;mode<3;mode++){Place(actor,tee.position-tee.forward*2.6f);app.view.mode=mode;yield return new WaitForSeconds(.15f);yield return Aim();yield return Frame();Check(Camera.main.nearClipPlane>.1f,"AIM_CAMERA_AVAILABLE_FROM_ROAM_MODE_"+mode);app.view.CancelGolfAim();yield return null;Check(app.view.mode==mode,"CANCEL_PRESERVES_ROAM_CAMERA_"+mode);}
   Place(actor,tee.position-tee.forward*2.6f);yield return new WaitForSeconds(.15f);yield return Aim();ball.Recover();yield return new WaitForSeconds(.2f);Check(!app.view.GolfAimRequested&&!match.IsAiming(actor),"BALL_RESET_CANCELS_STALE_AIM");
   yield return new WaitForSeconds(.3f);yield return Aim();ball.ApplyGameplayImpulse(Vector3.right*.07f);yield return new WaitForSeconds(.2f);Check(!app.view.GolfAimRequested&&!match.IsAiming(actor),"MOVING_BALL_CANCELS_STALE_AIM");
   ball.Place(tee.position+Vector3.up*.09f,true);Place(actor,tee.position-tee.forward*2.6f);yield return new WaitForSeconds(.5f);
   var wall=new GameObject("Temporary golf aim barrier");wall.layer=8;wall.transform.SetPositionAndRotation(tee.position-tee.forward*1.3f+Vector3.up*.7f,tee.rotation);wall.AddComponent<BoxCollider>().size=new Vector3(3,2,.15f);Physics.SyncTransforms();Check(!app.view.CanAimGolf&&!app.view.BeginGolfAim(),"WALL_BLOCKS_AIM_ENTRY");Destroy(wall);yield return null;
   for(int h=0;h<5;h++){var t=match.Course.holes[h].tee;ball.Place(t.position+Vector3.up*.09f,true);Place(actor,t.position-t.forward*2.6f);yield return new WaitForSeconds(.5f);yield return Aim();Check(match.CanStrike(actor,ball),"AUTHORED_TEE_HAS_SAFE_STANCE_"+(h+1));app.view.CancelGolfAim();}
   yield return Aim();app.view.enabled=false;yield return new WaitForSeconds(.1f);app.view.enabled=true;yield return null;Check(!match.IsAiming(actor),"DISABLED_CAMERA_RELEASES_AIM_WITHOUT_HUD_ERROR");
   yield return Aim();app.view.ClearMatchInput();yield return null;Check(!match.IsAiming(actor)&&!app.view.GolfAimRequested,"MENU_OR_ROUND_CLEAR_UNLOCKS");
   yield return Aim();match.ClearMatch();yield return null;Check(!app.view.GolfAimRequested&&!app.view.GolfPreview.Visible,"MATCH_END_CLEARS_AIM_AND_PREVIEW");
  }
 }
}
#endif
