#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace WhatTheFish {
 [DefaultExecutionOrder(810)]
 public sealed partial class GolfPolishProbe:MonoBehaviour {
  string[] args;string report;GolfMatchManager match;int checks;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){if(Environment.GetCommandLineArgs().Contains("-golfPolishAudit"))new GameObject("Golf polish review").AddComponent<GolfPolishProbe>();}
  void Check(bool pass,string name){File.AppendAllText(report,(pass?"PASS ":"FAIL ")+name+"\n");if(!pass)throw new Exception(name);checks++;}
  void Update(){if(args!=null)DevelopmentProbe.Driving=false;}
  IEnumerator Start(){
   args=Environment.GetCommandLineArgs();report=args[Array.IndexOf(args,"-report")+1];Directory.CreateDirectory(Path.GetDirectoryName(report));
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   float until=Time.realtimeSinceStartup+100,nextEntry=0;
   while((!DevelopmentProbe.ReadyToReviewSport||!GolfMatchManager.Instance||!GolfMatchManager.Instance.Context)&&Time.realtimeSinceStartup<until){
    DevelopmentProbe.Driving=false;var app=AppRoot.Instance;
    if(args.Contains("-polishNetwork")&&app&&app.rooms.Host&&!app.Exploring&&!RoomTeleport.Busy&&!TeleportSequence.Busy&&Time.unscaledTime>=nextEntry&&RoomTeleport.Players().Length==2&&RoomTeleport.Players().All(n=>n.Ready.Value)){
     var entry=app.rooms.SetExploring(true);while(!entry.IsCompleted)yield return null;nextEntry=Time.unscaledTime+2;
    }
    yield return null;
   }
   match=GolfMatchManager.Instance;Check(match&&match.Context,"GOLF_CONTEXT_READY");
   if(args.Contains("-golfTeeAudit")){yield return TeePresentation();File.AppendAllText(report,"GOLF_POLISH_COMPLETE checks="+checks+"\n");yield break;}
   if(args.Contains("-golfPersonalRanking")){yield return PersonalRankingPresentation();File.AppendAllText(report,"GOLF_POLISH_COMPLETE checks="+checks+"\n");yield break;}
   if(args.Contains("-golfRankingRollback")){yield return RankingRollbackPresentation();File.AppendAllText(report,"GOLF_POLISH_COMPLETE checks="+checks+"\n");yield break;}
   Check(!match.GetComponent<GolfFollowCamera>(),"SHOT_INSET_NOT_CREATED");
   Check(!match.transform.Find("Moving golf mechanisms"),"MOVING_MECHANISMS_REMOVED");
   Check(match.Course.waterPatches.Length==6&&match.Course.sandPatches.Length==20&&match.Course.roughPatches.Length==20,"EXISTING_STATIC_COURSE_RETAINED");
   var hud=GolfHUD.Instance;Check(hud&&!hud.CourseRect.gameObject.activeInHierarchy,"ISLAND_GREENS_BANNER_HIDDEN");
   var leave=hud.FrameRect.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="Golf button sticker"&&t.parent.parent.GetComponentInChildren<UnityEngine.UI.Text>(true)?.text=="Leave island");
   Check(!leave||!leave.gameObject.activeInHierarchy,"LEAVE_ISLAND_HIDDEN");
   foreach(var h in match.HoleTriggers){
    Vector3 centre=h.transform.position;float r=h.CaptureRadius;
    Check(h.Contains(h.transform.TransformPoint(new Vector3(r-.001f,GolfBall.Radius,0))),"RIM_TOUCH_COUNTS_"+h.Hole.number);
    Check(!h.Contains(h.transform.TransformPoint(new Vector3(r+.01f,GolfBall.Radius,0))),"OUTSIDE_RIM_NOT_SCORED_"+h.Hole.number);
    Check(!h.Contains(centre+Vector3.up*.6f),"AIRBORNE_OVER_CUP_REJECTED_"+h.Hole.number);
    Check(h.Crossed(centre+new Vector3(-2,GolfBall.Radius,0),centre+new Vector3(2,GolfBall.Radius,0)),"FAST_HORIZONTAL_RIM_SWEEP_"+h.Hole.number);
    Check(h.Crossed(centre+Vector3.up*2,centre-Vector3.up*.1f),"VERTICAL_DROP_STILL_SCORES_"+h.Hole.number);
    Check(!h.Crossed(centre+new Vector3(-2,.5f,0),centre+new Vector3(2,.5f,0)),"HIGH_SWEEP_NOT_SCORED_"+h.Hole.number);
   }
   if(args.Contains("-polishNetwork"))yield return Network();else yield return Offline();
   File.AppendAllText(report,"GOLF_POLISH_COMPLETE checks="+checks+"\n");
  }
  IEnumerator StartMatch(){bool started=false;float until=Time.time+18;while(!started&&Time.time<until){started=match.StartMatch();if(!started)yield return null;}Check(started,"HOST_MATCH_STARTED");}
  IEnumerator Offline(){
   yield return StartMatch();var actor=AppRoot.Instance.LocalAthlete;var ball=match.Ball(actor);Check(ball,"LOCAL_BALL_EXISTS");
   if(args.Contains("-golfRefine"))yield return RefineOffline(actor,ball);
   // Exercise actual cart sweeps through every pool in both directions, allowing
   // a collision-free chord if an existing rock/palm intersects a particular one.
   foreach(var p in match.Course.waterPatches){
    var centre=match.transform.TransformPoint(p.centre);
    if(args.Contains("-golfRefine")){Check(GolfCartMotor.Surface(centre,0,out var waterSupported,out _),"POND_HAS_WHEEL_SUPPORT_"+p.name);Check(waterSupported.y>=centre.y-.005f&&waterSupported.y<=centre.y+.04f,"CART_RIDES_ON_WATER_NOT_UNDER_IT_"+p.name);}
    bool crossed=false;float best=0;
    for(int trial=0;trial<8&&!crossed;trial++){
     float angle=trial*Mathf.PI/4;var direction=new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle));float extent=Mathf.Max(p.radii.x,p.radii.y)*p.bankScale+2;
     var start=centre-direction*extent;float yaw=Quaternion.LookRotation(direction).eulerAngles.y;
     if(!GolfCartMotor.Surface(start,yaw,out var pos,out var rotation))continue;
     var state=new GolfCartState{position=pos,rotation=rotation,summoned=true,driver=GolfCartState.NoDriver};
     for(int tick=0;tick<240;tick++){GolfCartMotor.Step(null,ref state,Vector2.up,1f/60);if(Vector3.Dot(state.position-start,direction)>extent*2+.5f)break;}
     float distance=Vector3.Dot(state.position-start,direction);best=Mathf.Max(best,distance);
     crossed=distance>extent*2;
     if(crossed){state.rotation=Quaternion.Euler(0,yaw+180,0);state.speed=0;for(int tick=0;tick<240;tick++){GolfCartMotor.Step(null,ref state,Vector2.up,1f/60);if(Vector3.Dot(state.position-start,direction)<.5f)break;}crossed=Vector3.Dot(state.position-start,direction)<extent*.5f;}
    }
    File.AppendAllText(report,$"MEASURE {p.name} bestTravel={best:F3}\n");Check(crossed,"CART_CROSSES_POND_BOTH_WAYS_"+p.name);
   }
   var far=match.transform.position+new Vector3(500,0,500);Check(!GolfCartMotor.Surface(far,0,out _,out _),"SEA_HAS_NO_CART_SUPPORT");
   int cupCount=GolfFeedback.Instance.CueCount(GolfFeedbackCue.Cup);
   for(int n=1;n<=5;n++){
    var hole=match.HoleTriggers.First(h=>h.Hole.number==n);ball.Place(hole.transform.position+Vector3.right*(hole.CaptureRadius-.002f)+Vector3.up*GolfBall.Radius,true);
    yield return new WaitForFixedUpdate();yield return null;
    Check(match.Player(actor).CompletedHoleCount==n,"REAL_BALL_RIM_ADVANCES_HOLE_"+n);
    Check(GolfCupCelebration.ActiveCount>0,"CELEBRATION_ON_SCORE_"+n);
    if(n==1&&args.Contains("-polishPictures")){yield return new WaitForSeconds(.35f);CaptureWorld("cup-effect",hole.transform.position);}
   }
   Check(GolfFeedback.Instance.CueCount(GolfFeedbackCue.Cup)==cupCount+4,"ONE_CUP_EVENT_PER_HOLE");
   Check(match.Player(actor).TotalStroke==0,"CONTACT_RULE_DOES_NOT_ADD_STROKES");
   var badge=GolfFinalCountdown.Create(hudRoot());double now=match.Now,end=now+30;
   foreach(float age in new[]{0f,.4f,.9f,1.3f,2f,20f,29f,30f}){
    badge.Show(true,30-age,end,now+age);badge.ReviewAge(age);badge.Show(true,30-age,end,now+age);yield return new WaitForEndOfFrame();Check(badge.Seconds==Mathf.CeilToInt(30-age),"COUNTDOWN_DIGITS_"+age);
    Check(age<.5f?badge.Card.anchoredPosition.sqrMagnitude<.01f:age<1.3f||badge.Card.anchoredPosition.x<0&&badge.Card.anchoredPosition.y>0,"COUNTDOWN_POSITION_"+age);
    if(args.Contains("-polishPictures")){CaptureUI("countdown-"+age.ToString("0.0",System.Globalization.CultureInfo.InvariantCulture),badge,30-age,end,now+age);yield return null;}
   }
   if(args.Contains("-golfRefine"))yield return RefineCountdown(badge,end,now);
   badge.Show(true,28,end,now+2);yield return new WaitForSecondsRealtime(.06f);badge.Show(true,28,end,now+2.06);Check(Mathf.Abs(badge.ClockAngle)>.01f,"CLOCK_WIGGLES_AFTER_SECOND_CHANGES");
   yield return new WaitForSecondsRealtime(.36f);badge.Show(true,28,end,now+2.42);Check(badge.ClockAngle==0,"CLOCK_WIGGLE_SETTLES");
   badge.Show(false,0,end,now+30);Check(!badge.Visible,"COUNTDOWN_CAN_HIDE_ON_NEW_ROUND");Destroy(badge.gameObject);
   yield return new WaitForSeconds(2.7f);Check(GolfCupCelebration.ActiveCount==0,"CELEBRATION_POOL_EXPIRES");
  }
  RectTransform hudRoot()=>GolfHUD.Instance.FrameRect;
  IEnumerator Network(){
   if(match.Authority){
    yield return StartMatch();Check(match.State.Players.Count==2,"TWO_PLAYER_ROSTER");string ready=Path.Combine(Path.GetDirectoryName(report),"client-ready");float until=Time.time+20;while(!File.Exists(ready)&&Time.time<until)yield return null;Check(File.Exists(ready),"REMOTE_READY");
    if(args.Contains("-golfRefine"))yield return RefineHost();
    var ball=match.Ball(AppRoot.Instance.LocalAthlete);
    for(int n=1;n<=5;n++){
     var h=match.HoleTriggers.First(h=>h.Hole.number==n);ball.Place(h.transform.position+Vector3.right*(h.CaptureRadius-.002f)+Vector3.up*GolfBall.Radius,true);yield return new WaitForFixedUpdate();yield return new WaitForSeconds(.25f);
    }
    Check(match.State.Phase==GolfMatchPhase.FinalCountdown,"FIRST_FINISHER_STARTS_30_SECONDS");Check(match.Remaining>27&&match.Remaining<=30,"HOST_COUNTDOWN_DEADLINE");
    yield return new WaitForSeconds(2);var ui=FindFirstObjectByType<GolfLeaderboardUI>();Check(ui.CountdownVisible,"HOST_COUNTDOWN_VISIBLE");
   }else{
    Check(!match.StartMatch(),"REMOTE_CANNOT_START_MATCH");float until=Time.time+30;while((!match.State.Running||!match.Ball(AppRoot.Instance.LocalAthlete))&&Time.time<until)yield return null;
    Check(match.Ball(AppRoot.Instance.LocalAthlete),"REMOTE_OWN_BALL_EXISTS");int before=GolfFeedback.Instance.CueCount(GolfFeedbackCue.Cup);File.WriteAllText(Path.Combine(Path.GetDirectoryName(report),"client-ready"),"ready");
    if(args.Contains("-golfRefine"))yield return RefineClient();
    until=Time.time+15;while(match.State.Phase!=GolfMatchPhase.FinalCountdown&&Time.time<until)yield return null;
    Check(match.State.Phase==GolfMatchPhase.FinalCountdown,"REMOTE_RECEIVES_FIRST_FINISHER_DEADLINE");yield return new WaitForSeconds(.15f);
    Check(GolfFeedback.Instance.CueCount(GolfFeedbackCue.Cup)>=before+4&&GolfCupCelebration.ActiveCount>0,"REMOTE_PLAYS_HOST_CONFIRMED_CUP_EFFECTS");
    var ui=FindFirstObjectByType<GolfLeaderboardUI>();Check(ui.CountdownVisible,"REMOTE_COUNTDOWN_VISIBLE");Check(match.Player(AppRoot.Instance.LocalAthlete).CurrentHole==1,"OTHER_PLAYER_PROGRESS_UNCHANGED");yield return new WaitForSeconds(2);
   }
  }
  void CaptureUI(string name,GolfFinalCountdown badge,double remaining,double deadline,double now,bool showCountdown=true){
   var obj=new GameObject("Golf HUD evidence camera");var cam=obj.AddComponent<Camera>();cam.CopyFrom(Camera.main);cam.enabled=false;obj.AddComponent<UniversalAdditionalCameraData>();cam.nearClipPlane=.03f;
   var canvas=hudRoot().GetComponentInParent<Canvas>();var oldMode=canvas.renderMode;var oldCamera=canvas.worldCamera;float oldDistance=canvas.planeDistance;
   var rt=new RenderTexture(1600,900,24);var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);var old=RenderTexture.active;
   try{cam.targetTexture=rt;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=cam;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();if(showCountdown){badge.Show(true,remaining,deadline,now);Canvas.ForceUpdateCanvases();var corners=new Vector3[4];badge.Card.GetWorldCorners(corners);Check(corners.All(c=>{var v=hudRoot().InverseTransformPoint(c);var area=hudRoot().rect;return v.x>=area.xMin-1&&v.x<=area.xMax+1&&v.y>=area.yMin-1&&v.y<=area.yMax+1;}),"CAPTURE_COUNTDOWN_WITHIN_FRAME_"+name);}cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(report),name+".png"),tex.EncodeToPNG());}
   finally{canvas.renderMode=oldMode;canvas.worldCamera=oldCamera;canvas.planeDistance=oldDistance;cam.targetTexture=null;RenderTexture.active=old;Destroy(rt);Destroy(tex);Destroy(obj);Canvas.ForceUpdateCanvases();}
  }
  void CaptureWorld(string name,Vector3 focus){
   var obj=new GameObject("Golf polish evidence camera");var cam=obj.AddComponent<Camera>();cam.CopyFrom(Camera.main);cam.enabled=false;obj.AddComponent<UniversalAdditionalCameraData>();cam.fieldOfView=42;
   var direction=Camera.main.transform.forward;direction.y=0;direction.Normalize();cam.transform.SetPositionAndRotation(focus-direction*4+Vector3.up*2.2f,Quaternion.LookRotation(direction*4-Vector3.up*1.2f));
   var rt=new RenderTexture(1280,900,24);var tex=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);var old=RenderTexture.active;
   try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(report),name+".png"),tex.EncodeToPNG());}
   finally{cam.targetTexture=null;RenderTexture.active=old;Destroy(rt);Destroy(tex);Destroy(obj);}
  }
 }
}
#endif
