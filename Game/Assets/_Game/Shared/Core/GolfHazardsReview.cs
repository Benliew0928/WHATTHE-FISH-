#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 // Explicit feature review, excluded from the release APK.
 [DefaultExecutionOrder(140)]public sealed class GolfHazardsReview:MonoBehaviour {
  string report;int checks;bool done,overrideView;Vector3 viewPosition;Quaternion viewRotation;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Environment.GetCommandLineArgs().Contains("-golfHazardsReview"))new GameObject("Golf hazards focused review").AddComponent<GolfHazardsReview>();}
  void Check(bool ok,string label){File.AppendAllText(report,(ok?"PASS ":"FAIL ")+label+"\n");if(!ok)throw new Exception(label);checks++;}
  void OnLog(string text,string stack,LogType type){if(!done&&(type==LogType.Exception||type==LogType.Error)){done=true;File.AppendAllText(report,"RUNTIME_FAILURE "+text+"\n"+stack);Application.Quit(2);}}
  void View(Vector3 position,Vector3 target){overrideView=true;viewPosition=position;viewRotation=Quaternion.LookRotation(target-position);}
  void LateUpdate(){if(overrideView&&Camera.main)Camera.main.transform.SetPositionAndRotation(viewPosition,viewRotation);}
  Vector3 Ground(Vector3 point){Check(Physics.Raycast(point+Vector3.up*25,Vector3.down,out var hit,60,1<<8,QueryTriggerInteraction.Ignore),"hazard has real course support");return hit.point+Vector3.up*(GolfBall.Radius+.01f);}
  IEnumerator Start(){
   var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-reviewReport");report=at>=0&&at+1<args.Length?args[at+1]:Path.Combine(Application.persistentDataPath,"golf-hazards-review.txt");Directory.CreateDirectory(Path.GetDirectoryName(report));File.WriteAllText(report,"Golf hazards and Fishing host-start review\n");Application.logMessageReceived+=OnLog;
   float until=Time.realtimeSinceStartup+180;
   while(!DevelopmentProbe.ReadyToReviewSport||!GolfMatchManager.Instance||!GolfMatchManager.Instance.CanStart||AppRoot.Instance.PendingSelection.HasValue){if(Time.realtimeSinceStartup>until){Check(false,"startup timeout");yield break;}yield return null;}
   var app=AppRoot.Instance;var match=GolfMatchManager.Instance;var actor=app.LocalAthlete;
   bool started=false;until=Time.realtimeSinceStartup+18;while(!started&&Time.realtimeSinceStartup<until){started=match.StartMatch();if(!started)yield return null;}
   Check(started,"normal golf host-start succeeds after arrival settles");yield return new WaitForSeconds(.5f);
   var world=match.GetComponent<GolfHazardWorld>();var ball=match.Ball(actor);var player=match.Player(actor);var hole=match.Target(actor);
   Check(world&&ball&&world.Lanes.Count>0,"course creates playable hazard lanes");var lane=world.Lanes.First(l=>l.hole==1);int strokes=player.TotalStroke;uint strike=ball.StrikeSequence;
   Check(match.ReviewEventHotkey(KeyCode.F8),"host development shortcut triggers tornado");
   ball.Place(Ground(world.Position(lane,match.Events.Snapshot,match.Now)),true);
   until=Time.realtimeSinceStartup+2;while(!ball.HazardHeld&&Time.realtimeSinceStartup<until)yield return null;
   Check(ball.HazardHeld&&!ball.ReadyToAim,"real tornado catches ball and blocks another swing");yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();Check(ball.Body.isKinematic&&!ball.Body.detectCollisions&&!ball.Body.useGravity,"regular match updates preserve the carried ball physics lock");
   View(lane.centre+new Vector3(10,7,-13),world.Position(lane,match.Events.Snapshot,match.Now)+Vector3.up*4);yield return new WaitForSeconds(.25f);Capture("tornado");
   until=Time.realtimeSinceStartup+3;while(world.TornadoTosses==0&&Time.realtimeSinceStartup<until)yield return null;
   Check(world.TornadoTosses==1&&!ball.HazardHeld&&ball.Body.linearVelocity.magnitude>1,"held ball is released with physical flight");yield return new WaitForSeconds(.5f);Check(app.view.GolfFollowingBall,"hazard launch starts the owned-ball flight camera");Check(app.view.SkipGolfReplay(),"flight camera accepts tap-skip action");yield return new WaitForSeconds(1);Check(!app.view.GolfShotCameraActive,"skipped flight returns to gameplay while ball keeps simulating");
   yield return new WaitForSeconds(.5f);Check(match.Course.SafeDryPosition(ball.Body.position)&&!match.OutsideCourse(ball.Body.position),"tornado throw returns to dry course");Capture("tornado-mature");
   var cyclone=lane.visual.GetComponentInChildren<GolfTornadoFX>();Check(cyclone&&cyclone.GetComponent<MeshRenderer>().sharedMaterial.shader.name=="WhatTheFish/GolfStormVolume"&&cyclone.GetComponentsInChildren<ParticleSystem>().Sum(p=>p.particleCount)>0,"tornado renders a shaded body, cloud and dust particles");
   Check(GolfEventHUD.Instance.Card.GetComponentsInChildren<GolfEventBadgeGraphic>().Any(g=>g.Badge==6&&g.Sheet==Resources.Load<Texture2D>("GolfUI/Events/TornadoBadge")),"tornado uses the generated illustrated badge and native timer");
   Check(player.TotalStroke==strokes&&ball.StrikeSequence==strike,"tornado adds no strokes or artificial club hit");
   ball.Place(Ground(world.Position(lane,match.Events.Snapshot,match.Now)),true);yield return new WaitForSeconds(.18f);Check(!ball.HazardHeld&&world.TornadoTosses==1,"same tornado cannot repeatedly trap a recovered ball");
   Check(match.ReviewEventHotkey(KeyCode.F9),"host development shortcut triggers crabs");yield return null;
   var crabPoint=Ground(world.Position(lane,match.Events.Snapshot,match.Now));ball.Place(crabPoint,true);ball.Strike(lane.forward*1.2f);uint beforeBump=ball.StrikeSequence;
   until=Time.realtimeSinceStartup+2;while(world.CrabBumps==0&&Time.realtimeSinceStartup<until)yield return null;
   Check(world.CrabBumps>0&&ball.HazardLaunch>=2&&ball.StrikeSequence==beforeBump&&player.TotalStroke==strokes,"walking crab launches a ball without a penalty stroke");
   var crab=world.Position(lane,match.Events.Snapshot,match.Now);View(crab+new Vector3(2,1.3f,-2.4f),crab+Vector3.up*.3f);yield return new WaitForSeconds(.15f);Capture("crab-close");
   Check(lane.visual&&lane.visual.GetComponentsInChildren<SkinnedMeshRenderer>().Length>0,"visible crab uses the authored skinned model");
   var bones=lane.visual.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Leg_")).ToArray();var rotations=bones.Select(b=>b.localRotation).ToArray();float moved=0;double clockBefore=match.Now;until=Time.realtimeSinceStartup+.8f;
   while(moved<=.1f&&Time.realtimeSinceStartup<until){yield return null;for(int i=0;i<bones.Length;i++)moved=Mathf.Max(moved,Quaternion.Angle(rotations[i],bones[i].localRotation));}
   File.AppendAllText(report,$"CRAB_MOTION angle={moved} active={lane.visual.gameObject.activeInHierarchy} reduced={MenuPreferences.ReducedMotion} clockDelta={match.Now-clockBefore} bones={bones.Length}\n");Check(moved>.1f,"crab legs actually animate");
   ball.Place(hole.cup.position+hole.cup.right*2+Vector3.up*(GolfBall.Radius+.01f),true);yield return new WaitForSeconds(.15f);Check(!ball.HazardHeld&&match.Target(actor).number==1,"near-cup ball remains playable");
   Screen.SetResolution(844,390,false);yield return new WaitForSeconds(.2f);Capture("crab-phone-layout");Screen.SetResolution(1600,900,false);overrideView=false;
   var indicator=FindFirstObjectByType<GolfTargetIndicator>();var guide=indicator.EdgeArrow.GetComponent<GolfHoleGuideGraphic>();var aimDirection=typeof(GolfTargetIndicator).GetMethod("AimDirection",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);var camera=Camera.main;
   foreach(float depth in new[]{-.5f,.001f}){aimDirection.Invoke(indicator,new object[]{camera,camera.transform.position+camera.transform.forward*depth+camera.transform.right*.5f});Check(Mathf.Abs(Mathf.DeltaAngle(0,guide.direction))<=90&&float.IsFinite(indicator.EdgeArrow.anchoredPosition.sqrMagnitude),"near/behind cup pointer stays finite and cannot point skyward "+depth);}
   while(!player.IsFinished)match.State.EnterHole(player.PlayerId,player.CurrentHole,match.Now);
   yield return new WaitForSeconds(2);Check(FishingCeremony.Presenting&&FishingCeremony.Instance.CastCount>0,"Golf ends with the reused sports awards cast");Check(GolfHUD.Instance.FrameRect.GetComponentInParent<CanvasGroup>().alpha==0,"awards hide the gameplay UI root");Check(actor.GolfClubMotion.Club.GetComponentsInChildren<Renderer>().All(r=>r.forceRenderingOff),"awards hide the real golfer equipment");Capture("golf-awards");
   yield return new WaitForSeconds(6);Capture("golf-winner-focus");FishingCeremony.Instance.Skip();yield return new WaitForSeconds(.6f);Check(!FishingCeremony.Presenting&&PostMatchFlow.Instance.Visible,"Golf awards skip restores the next-adventure card");PostMatchFlow.Instance.Close();
   app.Show("home");app.SelectSport(SportId.Fishing);until=Time.realtimeSinceStartup+90;while(app.PendingSelection.HasValue||app.SelectedSport!=SportId.Fishing){if(Time.realtimeSinceStartup>until){Check(false,"fishing selection timeout");yield break;}yield return null;}app.EnterOffline();
   while(!DevelopmentProbe.ReadyToReviewSport||!FishingGame.Instance||!FishingGame.Instance.Context||!FishingHUD.Instance){if(Time.realtimeSinceStartup>until){Check(false,"fishing arrival timeout");yield break;}yield return null;}
   yield return new WaitForSeconds(.3f);var game=FishingGame.Instance;var hud=FishingHUD.Instance;var abilities=hud.GetComponent<FishingAbilityHUD>();hud.Tutorial.Close();yield return null;
   Check(!abilities.Modal,"practice does not automatically open skill selection");
   var sharedCloud=SkySailWorld.Instance.scenery.GetComponentsInChildren<Transform>().First(t=>t.name=="Shared cloud 0");Check(Mathf.Abs(Vector3.ProjectOnPlane(sharedCloud.localPosition,Vector3.up).magnitude-1900)<1,"Golf cloud adjustment restores shared scenery after leaving");
   var buttons=FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None);var start=buttons.FirstOrDefault(b=>b.name=="Start fishing round");Check(start,"host Start control exists in the safe-area leaderboard frame");
   Check(!buttons.Any(b=>b.name=="Fishing ready"||b.name=="Return to free fishing"||b.name.StartsWith("Mode "))&&!hud.GetComponentsInChildren<InputField>(true).Any(),"leaderboard has no readiness, mode, practice or rename controls");
   Check(game.State.Players.All(p=>!p.ready)&&start.interactable,"host Start is enabled while nobody is ready");start.onClick.Invoke();yield return new WaitForSeconds(.15f);
   Check(game.State.Match.phase==FishingRoundPhase.Drafting&&game.State.Match.mode==FishingMode.Round&&abilities.Modal,"Start opens the shared draft for the default round");Capture("fishing-start-draft");
   abilities.ChoiceButtons[0].onClick.Invoke();until=Time.realtimeSinceStartup+7;while(game.State.Match.phase!=FishingRoundPhase.Playing&&Time.realtimeSinceStartup<until)yield return null;
   Check(game.State.Match.phase==FishingRoundPhase.Playing&&game.Player(app.LocalAthlete).Value.skillPicked,"skill selection and countdown enter normal play without Ready");Capture("fishing-clean-board");
   Check(!FindObjectsByType<FishingTapCue>(FindObjectsInactive.Include,FindObjectsSortMode.None).Any(c=>c.gameObject.activeInHierarchy),"no misleading arrow above Jump before hooking");
   game.State.Advance(game.State.Match.deadline+.01,0);yield return new WaitForSeconds(2);Check(FishingCeremony.Presenting&&FishingCeremony.Instance.CastCount>0,"Fishing ends with shared sports awards");Capture("fishing-awards");
   yield return new WaitForSeconds(6);Capture("fishing-winner-focus");FishingCeremony.Instance.Skip();yield return new WaitForSeconds(.6f);Check(!FishingCeremony.Presenting&&hud.Results.Visible,"Fishing awards skip opens results and restores controls");
   done=true;File.AppendAllText(report,"GOLF_HAZARDS_REVIEW_PASS checks="+checks+"\n");Debug.Log("GOLF_HAZARDS_REVIEW_PASS checks="+checks);Application.Quit(0);
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
