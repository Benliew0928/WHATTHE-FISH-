#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace WhatTheFish {
 // Small explicit opt-in review of this feature, excluded from release APKs.
 [DefaultExecutionOrder(140)]public sealed class GolfCupEffectsReview:MonoBehaviour {
  string report;int checks;bool done;
  bool overrideView;Vector3 viewPosition;Quaternion viewRotation;
  void View(Vector3 position,Vector3 target){overrideView=true;viewPosition=position;viewRotation=Quaternion.LookRotation(target-position,Vector3.up);Camera.main.transform.SetPositionAndRotation(viewPosition,viewRotation);}
  void LateUpdate(){if(overrideView&&Camera.main)Camera.main.transform.SetPositionAndRotation(viewPosition,viewRotation);}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var args=Environment.GetCommandLineArgs();if(args.Contains("-golfCupReview")||args.Contains("-golfEaseReview")||args.Contains("-golfClarityReview"))new GameObject("Golf cup and event review").AddComponent<GolfCupEffectsReview>();}
  void Check(bool ok,string label){File.AppendAllText(report,(ok?"PASS ":"FAIL ")+label+"\n");if(!ok)throw new Exception(label);checks++;}
  void OnLog(string text,string stack,LogType type){if(!done&&(type==LogType.Exception||type==LogType.Error)){done=true;File.AppendAllText(report,"RUNTIME_FAILURE "+text+"\n"+stack);Application.Quit(2);}}
  IEnumerator Start(){
   var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-reviewReport");report=at>=0&&at+1<args.Length?args[at+1]:Path.Combine(Application.persistentDataPath,"golf-cup-review.txt");Directory.CreateDirectory(Path.GetDirectoryName(report));File.WriteAllText(report,"Golf cup/effect focused review\n");Application.logMessageReceived+=OnLog;
   float timeout=Time.realtimeSinceStartup+120;while(!DevelopmentProbe.ReadyToReviewSport||!GolfMatchManager.Instance||!GolfMatchManager.Instance.Context||!GolfMatchManager.Instance.CanStart||!GolfHUD.Instance||AppRoot.Instance.PendingSelection.HasValue||AppRoot.Instance.environments.GetComponent<SkySailStreaming>().Busy){if(Time.realtimeSinceStartup>timeout){Check(false,"startup timeout");yield break;}yield return null;}
   var app=AppRoot.Instance;var match=GolfMatchManager.Instance;var actor=app.LocalAthlete;
   yield return new WaitForSeconds(.6f);GolfFeedback.Instance.ReviewFocus(true);
   Check(match.StartMatch(),"round starts");yield return new WaitForSeconds(.6f);var ball=match.Ball(actor);var hole=match.Target(actor);var cup=hole.cup;
   Check(ball&&hole.number==1,"owned ball and first target exist");
   // A real guided Putt must hand back to physical collision near the opening.
   var start=cup.TransformPoint(new Vector3(-.45f,GolfBall.Radius+.008f,0));ball.Place(start,true);
   var plan=new GolfShotPlan();plan.Build(start,cup.right,.2f,GolfShotMode.Putt,match.Course,ball.PhysicsSettings);ball.Strike(plan);
   yield return new WaitForSeconds(.28f);
   Check(!ball.FollowingShot&&!ball.Sinking&&match.Target(actor).number==1,"fast guided putt crosses without instant scoring");
   Check(cup.InverseTransformPoint(ball.Body.position).x>hole.cupRadius,"powerful shot emerges beyond far lip");
   if(args.Contains("-golfEaseReview")||args.Contains("-golfClarityReview")){
    yield return EaseReview(app,match,actor,ball,hole);
    if(args.Contains("-golfClarityReview"))yield return ClarityReview(app,match,actor,ball);
    done=true;File.AppendAllText(report,"GOLF_EASE_REVIEW_PASS checks="+checks+"\n");Debug.Log("GOLF_EASE_REVIEW_PASS checks="+checks);Application.Quit(0);yield break;
   }
   ball.Place(hole.tee.position+Vector3.up*(GolfBall.Radius+.01f),true);
   actor.capsule.enabled=false;actor.transform.position=ball.Body.position-hole.tee.forward*.9f;actor.ResetLocomotion();actor.capsule.enabled=true;Physics.SyncTransforms();yield return new WaitForSeconds(.5f);
   Check(app.view.BeginGolfAim(),"normal aim control accepts player");yield return new WaitForSeconds(.4f);
   var indicator=FindFirstObjectByType<GolfTargetIndicator>();var camera=Camera.main;
   View(cup.position+new Vector3(-6,3,-8),cup.position+Vector3.up*.6f);yield return new WaitForSeconds(.25f);
   Check(indicator.HoleGuideVisible&&!indicator.EdgeArrowVisible&&!indicator.LargeTargetArrowVisible,"one numbered marker over actual visible target");Capture("numbered-hole");
   viewRotation=Quaternion.LookRotation(-camera.transform.forward,Vector3.up);yield return new WaitForSeconds(.15f);
   Check(indicator.EdgeArrowVisible&&Mathf.Abs(indicator.EdgeArrow.anchoredPosition.x)>((RectTransform)indicator.EdgeArrow.parent).rect.width*.35f,"same number gives sideways turn cue for target behind camera");Capture("offscreen-hole");
   View(viewPosition,cup.position+Vector3.up*.6f);
   Screen.SetResolution(1600,720,false);yield return new WaitForSeconds(.25f);Check(GolfHUD.Instance.FitsSafeFrame(),"wide landscape HUD fits safe frame");Capture("wide-hole");Screen.SetResolution(1600,900,false);yield return new WaitForSeconds(.2f);
   match.ReviewEvent(GolfEventKind.TropicalWind);yield return new WaitForSeconds(.3f);
   Check(GolfEventHUD.Instance.WindVisible&&match.GetComponent<GolfWindWorld>().VisibleStreaks>0,"wind arrow and world streaks use active weather");Capture("wind");
   app.view.CancelGolfAim();yield return new WaitForSeconds(.25f);Check(!indicator.HoleGuideVisible,"target marker fades after leaving aim");
   overrideView=false;
   foreach(var kind in new[]{GolfEventKind.StickyGreen,GolfEventKind.BounceParty}){
    match.ReviewEvent(kind);var origin=hole.tee.position+hole.tee.right*3+Vector3.up*2;ball.Place(origin,true);var shot=new GolfShotPlan();shot.Build(origin,hole.tee.right,.05f,GolfShotMode.Swing,match.Course,ball.PhysicsSettings,match.ShotConditions(GolfShotMode.Swing));var fx=ball.GetComponent<GolfBallEventFX>();int before=fx.Contacts;ball.Strike(shot);
    float deadline=Time.realtimeSinceStartup+4;while(fx.Contacts==before&&Time.realtimeSinceStartup<deadline)yield return null;
    Check(fx.Contacts>before,kind+" reacts to actual landing");yield return new WaitForSeconds(.08f);
    View(ball.transform.position+new Vector3(1.2f,1,-1.5f),ball.transform.position);Capture(kind.ToString());overrideView=false;
   }
   // A slow centred approach falls first, animates in the cup, then advances.
   start=cup.TransformPoint(new Vector3(-.31f,GolfBall.Radius+.004f,0));ball.Place(start,true);ball.Strike(cup.right*1.15f);
   float slowDeadline=Time.realtimeSinceStartup+4;bool sawSink=false;while(match.Target(actor)?.number==1&&Time.realtimeSinceStartup<slowDeadline){sawSink|=ball.Sinking;yield return null;}
   Check(sawSink&&match.Target(actor)?.number==2,"slow ball visibly sinks before next tee");
   float teeDeadline=Time.realtimeSinceStartup+2;while(!ball.ReadyToAim&&Time.realtimeSinceStartup<teeDeadline)yield return null;
   Check(!ball.Sinking&&ball.ReadyToAim,"next tee clears sink lock");
   done=true;File.AppendAllText(report,"GOLF_CUP_REVIEW_PASS checks="+checks+"\n");Debug.Log("GOLF_CUP_REVIEW_PASS checks="+checks);Application.Quit(0);
  }
  IEnumerator EaseReview(AppRoot app,GolfMatchManager match,Athlete actor,GolfBall ball,GolfHole hole){
   foreach(var kind in GolfEventSchedule.RandomKinds){match.ReviewEvent(kind);yield return null;var e=match.Events.Snapshot;Check(Math.Abs(e.end-e.start-45)<.001&&match.Events.Active(match.Now+40)==kind,"full forty-five-second event "+kind);}
   match.ReviewEvent(GolfEventKind.TropicalWind);yield return new WaitForSeconds(1.6f);
   var edge=GolfEventHUD.Instance.Card.parent.Find("Wind arrival screen edges").GetComponent<GolfWindEdgeGraphic>();
   Check(GolfEventHUD.Instance.WindVisible&&match.GetComponent<GolfWindWorld>().VisibleStreaks>0&&edge.color.a>.08f,"strong wind remains visible after its arrival sweep");
   Check(GolfEventHUD.Instance.Card.GetComponentsInChildren<GolfArrowGraphic>().Any(a=>a.rectTransform.rect.width>=40),"large wind direction arrow stays visible");Capture("sustained-wind");
   match.ReviewEvent(GolfEventKind.StickyGreen);var origin=hole.tee.position+hole.tee.right*3+Vector3.up*2;ball.Place(origin,true);var shot=new GolfShotPlan();shot.Build(origin,hole.tee.right,.05f,GolfShotMode.Swing,match.Course,ball.PhysicsSettings,match.ShotConditions(GolfShotMode.Swing));var fx=ball.GetComponent<GolfBallEventFX>();int contacts=fx.Contacts;ball.Strike(shot);
   float until=Time.realtimeSinceStartup+4;while(fx.Contacts==contacts&&Time.realtimeSinceStartup<until)yield return null;
   Check(fx.Contacts>contacts,"actual sticky touchdown produces contact feedback");yield return new WaitForSeconds(.62f);
   Check(FindObjectsByType<LineRenderer>(FindObjectsSortMode.None).Any(r=>r.name=="Golf event contact ring"&&r.enabled),"landing ripple stays visible beyond half a second");
   View(ball.transform.position+new Vector3(1.2f,1,-1.5f),ball.transform.position);Capture("longer-contact");overrideView=false;
   var cup=hole.cup;View(cup.position+new Vector3(-1.2f,1.05f,-1.2f),cup.position);yield return null;Capture("larger-cup");overrideView=false;
   var start=cup.TransformPoint(new Vector3(-.31f,GolfBall.Radius+.004f,.11f));ball.Place(start,true);ball.Strike(cup.right*1.5f);
   until=Time.realtimeSinceStartup+4;bool sink=false;while(match.Target(actor)?.number==1&&Time.realtimeSinceStartup<until){sink|=ball.Sinking;yield return null;}
   Check(sink&&match.Target(actor)?.number==2,"gentle off-centre approach drops in and advances the hole");
   until=Time.realtimeSinceStartup+2;while(!ball.ReadyToAim&&Time.realtimeSinceStartup<until)yield return null;Check(!ball.Sinking&&ball.ReadyToAim,"next tee is playable after easier entry");
  }
  IEnumerator ClarityReview(AppRoot app,GolfMatchManager match,Athlete actor,GolfBall ball){
   var hole=match.Target(actor);var cup=hole.cup;
   ball.Place(cup.TransformPoint(new Vector3(-.46f,GolfBall.Radius+.004f,0)),true);ball.Strike(cup.right*4.5f);
   float until=Time.realtimeSinceStartup+3;bool sink=false;int number=hole.number;
   while(match.Target(actor)?.number==number&&Time.realtimeSinceStartup<until){sink|=ball.Sinking;yield return null;}
   Check(sink&&match.Target(actor)?.number==number+1,"ordinary fast crossing drops in without requiring a slow physical fall");
   match.ReviewEvent(GolfEventKind.BounceParty);yield return new WaitForSeconds(.35f);
   var edge=GolfEventHUD.Instance.Card.parent.Find("Wind arrival screen edges").GetComponent<GolfWindEdgeGraphic>();
   Check(edge.color.a>.04f&&edge.color.r>edge.color.b,"bounce event has clear orange edge feedback");Capture("bounce-event");
   app.Show("home");yield return new WaitForSeconds(.5f);
   var menu=app.Menu;
   string[] phrases={"solo adventures","cast & catch","shoot & score","pass & score","aim & putt","home ground","controls, rules"};
   Check(!menu.Content.GetComponentsInChildren<UnityEngine.UI.Text>().Any(t=>phrases.Any(p=>t.text.ToLowerInvariant().Contains(p))),"home omits promotional subtitles");Capture("clean-home");
   app.Show("settings");yield return new WaitForSeconds(.4f);
   Check(!menu.Content.GetComponentsInChildren<UnityEngine.UI.Text>().Any(t=>t.text.Contains("A little tuning")),"settings omits promotional subtitle");
   app.Show("sport");yield return new WaitForSeconds(.4f);
   var labels=menu.Content.GetComponentsInChildren<UnityEngine.UI.Text>().Select(t=>t.text).Where(t=>!string.IsNullOrEmpty(t)).ToArray();
   Check(labels.Contains("LET'S PLAY.")&&labels.Contains("EXPLORE OFFLINE")&&labels.Contains("VERSUS BOTS")&&!labels.Any(t=>t.Contains("Choose how")||t.Contains("Your own pace")),"sport page retains modes and removes subtitles");Capture("clean-sport");
  }
  void Capture(string name){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
   var camera=Camera.main;var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distance=canvases.Select(c=>c.planeDistance).ToArray();var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=camera.targetTexture;var active=RenderTexture.active;var pixels=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
   try{foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=.5f;}camera.targetTexture=rt;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;pixels.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);pixels.Apply();File.WriteAllBytes(Path.ChangeExtension(report,name+".png"),pixels.EncodeToPNG());}
   finally{for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=RenderMode.ScreenSpaceOverlay;canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distance[i];}camera.targetTexture=previous;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);Destroy(pixels);}
  }
  void OnDestroy(){Application.logMessageReceived-=OnLog;}
 }
}
#endif
