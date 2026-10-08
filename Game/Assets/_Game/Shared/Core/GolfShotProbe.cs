#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 // Measures live ball travel against the mesh actually shown to the player.
 public sealed class GolfShotProbe:MonoBehaviour {
  string report;int checks;AppRoot app;GolfMatchManager match;Athlete actor;GolfBall ball;GolfSwingButton ui;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){if(Environment.GetCommandLineArgs().Contains("-golfShotAudit"))new GameObject("Golf shot accuracy checks").AddComponent<GolfShotProbe>();}
  void Check(bool pass,string label){File.AppendAllText(report,(pass?"PASS ":"FAIL ")+label+"\n");if(!pass)throw new Exception(label);checks++;}
  Vector3 Ground(Vector3 p,float height){if(Physics.Raycast(p+Vector3.up*4,Vector3.down,out var hit,8,1<<8,QueryTriggerInteraction.Ignore))return hit.point+Vector3.up*height;throw new Exception("No shot test ground");}
  void PlaceActor(Vector3 p){actor.capsule.enabled=false;actor.transform.position=Ground(p,.035f);actor.ResetLocomotion();actor.capsule.enabled=true;Physics.SyncTransforms();}
  static IEnumerator Frame(){if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)yield return null;else yield return new WaitForEndOfFrame();}
  void Capture(string name){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
   try{foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=Camera.main;c.planeDistance=.5f;}FindFirstObjectByType<DevelopmentProbe>()?.CaptureFrame(Path.ChangeExtension(report,name+".png"));}
   finally{for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=RenderMode.ScreenSpaceOverlay;canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}}
  }
  IEnumerator Prepare(Vector3 origin,float heading){
   app.view.CancelGolfAim();ball.Place(origin,true);PlaceActor(origin-(Quaternion.Euler(0,heading,0)*Vector3.forward)*1.8f);app.view.yaw=heading;app.view.pitch=28;app.view.mode=1;
   yield return new WaitForSeconds(.8f);float until=Time.time+3;while(!ball.ReadyToAim&&Time.time<until)yield return new WaitForFixedUpdate();
   File.AppendAllText(report,$"AIM ball={ball.Body.position:F3} player={actor.transform.position:F3} ready={ball.ReadyToAim} grounded={actor.Grounded} canSwing={match.CanSwing(actor)} stance={match.TryStance(actor,ball,out _,heading)} mode={match.ShotMode(ball)}\n");
   until=Time.realtimeSinceStartup+3;while(!app.view.active&&Time.realtimeSinceStartup<until)yield return null;
   Check(app.view.BeginGolfAim(),"NEAR_BALL_AIM");yield return new WaitForSeconds(.25f);yield return Frame();
  }
  static float DistanceToGuide(Vector3 p,Vector3[] vertices,int count){
   float best=float.MaxValue;for(int i=1;i<count;i++){
    var a=(vertices[(i-1)*2]+vertices[(i-1)*2+1])*.5f-Vector3.up*.035f;var b=(vertices[i*2]+vertices[i*2+1])*.5f-Vector3.up*.035f;
    var d=b-a;float t=d.sqrMagnitude>1e-10f?Mathf.Clamp01(Vector3.Dot(p-a,d)/d.sqrMagnitude):0;best=Mathf.Min(best,Vector3.Distance(p,a+d*t));
   }return best;
  }
  IEnumerator Observe(string label,Vector3 target,Vector3[] vertices,int count,GolfShotMode mode,uint contact,int strokes){
   float until=Time.time+1;while(!ball.FollowingShot&&Time.time<until)yield return new WaitForFixedUpdate();Check(ball.FollowingShot,label+"_CONTACT_STARTS_TRAVEL");
   float error=0,peak=ball.Body.position.y;uint reset=ball.ResetSequence;until=Time.time+20;
   while(ball.FollowingShot&&Time.time<until){error=Mathf.Max(error,DistanceToGuide(ball.Body.position,vertices,count));peak=Mathf.Max(peak,ball.Body.position.y);yield return new WaitForFixedUpdate();}
   float endError=Vector3.Distance(target,ball.LastGuideContact);
   File.AppendAllText(report,$"MEASURE {label} pathError={error:F5} contactError={endError:F5} peak={peak:F3}\n");
   Check(ball.ResetSequence==reset&&ball.GuideContactSequence==contact+1,label+"_REACHES_REAL_CONTACT_WITHOUT_RESET");
   Check(error<.08f&&endError<.02f,label+"_VISIBLE_PATH_AND_CONTACT_MATCH_LIVE_BALL");
   Check(match.Player(actor).TotalStroke==strokes+1,label+"_ONE_RELEASE_ONE_STROKE");
   if(mode==GolfShotMode.Putt){yield return new WaitForSeconds(.8f);Check(ball.Motion==GolfBallMotion.Resting&&Vector3.Distance(ball.Body.position,target)<.03f,label+"_PUTT_RESTS_AT_GLOW");}
   else Check(peak-ball.LastShotPosition.y>.35f,label+"_SWING_HAS_CURVED_FLIGHT");
  }
  IEnumerator Start(){
   var args=Environment.GetCommandLineArgs();report=args[Array.IndexOf(args,"-report")+1];DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   float until=Time.time+60;while((!AppRoot.Instance||!AppRoot.Instance.Exploring||!GolfMatchManager.Instance)&&Time.time<until)yield return null;
   app=AppRoot.Instance;match=GolfMatchManager.Instance;actor=app.LocalAthlete;Check(match&&match.StartMatch(),"MATCH_STARTED");ball=match.Ball(actor);ui=FindFirstObjectByType<GolfSwingButton>();
   Check(!GameObject.Find("Golf shot readout"),"NO_PHYSICS_READOUT_PANEL");
   Time.timeScale=3;
   foreach(var hole in match.Course.holes){
    foreach(var mode in new[]{GolfShotMode.Swing,GolfShotMode.Putt})foreach(float power in new[]{.25f,.80f}){
     var direction=mode==GolfShotMode.Swing?Vector3.ProjectOnPlane(hole.cup.position-hole.tee.position,Vector3.up).normalized:Vector3.right;
     var origin=mode==GolfShotMode.Swing?match.TeePosition(hole.number,ball.Owner):Ground(hole.cup.position+Vector3.right*2,GolfBall.Radius+.002f);
     float heading=Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg;
     yield return Prepare(origin,heading);Check(app.view.GolfMode==mode&&ui.label.text.StartsWith(mode.ToString()),$"HOLE_{hole.number}_{mode}_AUTOMATIC_MODE_AND_BUTTON");
     var aim=match.Aim(actor);app.view.GolfPreview.Show(ball,match.Course,aim.ballPosition,heading,power,aim.mode,Camera.main);
     Check(app.view.GolfPreview.HasTarget,"TARGET_ON_REAL_COURSE_SURFACE");
     var target=app.view.GolfPreview.EndPoint;var vertices=app.view.GolfPreview.GetComponent<MeshFilter>().sharedMesh.vertices;int count=app.view.GolfPreview.PointCount;uint contact=ball.GuideContactSequence;int strokes=match.Player(actor).TotalStroke;
     app.view.RequestGolfSwing(power);yield return Observe($"HOLE_{hole.number}_{mode}_{power:F2}",target,vertices,count,mode,contact,strokes);
    }
    app.view.CancelGolfAim();if(hole.number<5)Check(match.EnterHole(ball,hole.number)==GolfHoleResult.Completed,"ADVANCE_TO_NEXT_TARGET_"+hole.number);
   }
   // Real pointer hold traverses two peaks and a trough without auto-releasing.
   Time.timeScale=1;var last=match.Course.holes[4];var dir=Vector3.ProjectOnPlane(last.cup.position-last.tee.position,Vector3.up).normalized;
   yield return Prepare(match.TeePosition(5,ball.Owner),Mathf.Atan2(dir.x,dir.z)*Mathf.Rad2Deg);
   var pointer=new PointerEventData(EventSystem.current){pointerId=72,button=PointerEventData.InputButton.Left};int before=match.Player(actor).TotalStroke;ui.OnPointerDown(pointer);
   yield return new WaitForSeconds(1.15f);Check(app.view.GolfDisplayedCharge>.8f,"POWER_FIRST_RISE");Capture("swing-glow");
   yield return new WaitForSeconds(.9f);Check(app.view.GolfDisplayedCharge>.25f&&app.view.GolfDisplayedCharge<.55f,"POWER_FALLS_WHILE_STILL_HELD");
   yield return new WaitForSeconds(.52f);Check(app.view.GolfDisplayedCharge<.12f,"POWER_RETURNS_TO_LOW");
   yield return new WaitForSeconds(1.1f);Check(app.view.GolfDisplayedCharge>.7f&&app.view.GolfCharging&&match.Player(actor).TotalStroke==before,"POWER_REPEATS_WITHOUT_AUTO_SWING");
   yield return new WaitForSeconds(.55f);yield return Frame();
   var end=app.view.GolfPreview.EndPoint;var mesh=app.view.GolfPreview.GetComponent<MeshFilter>().sharedMesh.vertices;int samples=app.view.GolfPreview.PointCount;uint sequence=ball.GuideContactSequence;
   // A direction update arriving after the rendered frame cannot move the shot.
   app.view.yaw+=10;ui.OnPointerUp(pointer);yield return Observe("DESCENDING_RELEASE",end,mesh,samples,GolfShotMode.Swing,sequence,before);
   yield return Prepare(Ground(last.cup.position+Vector3.left*2,GolfBall.Radius+.002f),-90);
   ui.OnPointerDown(pointer);yield return new WaitForSeconds(.9f);yield return Frame();Capture("putt-glow");ui.OnPointerExit(pointer);yield return null;
   Check(!app.view.GolfCharging&&app.view.GolfAiming,"POINTER_CANCEL_RETAINS_AIM_WITHOUT_SWING");app.view.CancelGolfAim();
   yield return Interactions();
   File.AppendAllText(report,"GOLF_SHOT_COMPLETE checks="+checks+"\n");
  }
  IEnumerator Interactions(){
   var cup=match.Course.holes[4].cup.position;
   ball.Place(Ground(cup+Vector3.left*11.9f,GolfBall.Radius+.002f),true);Check(match.ShotMode(ball)==GolfShotMode.Putt,"INSIDE_TWELVE_METRES_USES_PUTT");
   ball.Place(Ground(cup+Vector3.left*12.1f,GolfBall.Radius+.002f),true);Check(match.ShotMode(ball)==GolfShotMode.Swing,"OUTSIDE_TWELVE_METRES_USES_SWING");
   var plate=new GameObject("Temporary trajectory interaction support");plate.layer=8;plate.transform.position=cup+Vector3.up*5;plate.AddComponent<BoxCollider>().size=new Vector3(16,.2f,16);Physics.SyncTransforms();
   var start=plate.transform.position+Vector3.up*(.1f+GolfBall.Radius+.002f);ball.Place(start,true);
   var other=new GameObject("Temporary trajectory collision ball").AddComponent<GolfBall>();other.Bind(match,999999);other.SetLive(true);other.Place(start+Vector3.right*1.2f,true);yield return new WaitForSeconds(.6f);
   var plan=new GolfShotPlan();plan.Build(ball.Body.position,Vector3.right,.7f,GolfShotMode.Putt,match.Course,ball.PhysicsSettings);ball.Strike(plan);
   float until=Time.time+2;while(ball.FollowingShot&&Time.time<until)yield return new WaitForFixedUpdate();yield return new WaitForSeconds(.2f);
   Check(!ball.FollowingShot&&!ball.Body.isKinematic&&other.Body.position.x>start.x+1.23f,"PLANNED_PUTT_HANDS_OFF_TO_REAL_BALL_COLLISION");
   Destroy(other.gameObject);ball.Place(start,true);plan=new GolfShotPlan();plan.Build(start,Vector3.right,.7f,GolfShotMode.Putt,match.Course,ball.PhysicsSettings);ball.Strike(plan);yield return new WaitForFixedUpdate();ball.ApplyGameplayImpulse(Vector3.forward*.04f);
   Check(!ball.FollowingShot&&!ball.Body.isKinematic&&ball.Body.useGravity,"NEW_IMPULSE_INTERRUPTS_PLANNED_TRAVEL");
   ball.Place(start,true);Check(!ball.FollowingShot&&ball.ReadyToAim,"RESET_CLEARS_PLANNED_TRAVEL");Destroy(plate);yield return null;
   match.ClearMatch();Check(match.StartMatch(),"CUP_PUTT_MATCH_STARTED");ball=match.Ball(actor);cup=match.Course.holes[0].cup.position;
   yield return Prepare(Ground(cup+Vector3.right*.65f,GolfBall.Radius+.002f),-90);Check(app.view.GolfMode==GolfShotMode.Putt,"CUP_APPROACH_USES_PUTT");
   app.view.RequestGolfSwing(.35f);until=Time.time+4;while(match.Player(actor).CompletedHoleCount==0&&Time.time<until)yield return new WaitForFixedUpdate();
   File.AppendAllText(report,$"CUP_PUTT position={ball.Body.position:F4} completed={match.Player(actor).CompletedHoleCount}\n");
   Check(match.Player(actor).CompletedHoleCount==1&&match.Player(actor).TotalStroke==1,"ACTUAL_GROUND_PUTT_FALLS_INTO_CUP_AND_SCORES");
  }
 }
}
#endif
