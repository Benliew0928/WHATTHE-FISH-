#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace WhatTheFish {
 // Runs only when explicitly requested by the local rendered-player runner.
 public sealed class PlayerIdentityCameraReview:MonoBehaviour {
  string folder;AppRoot app;Athlete actor;PlayerView view;Camera cam;bool failed;int samples;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-identityCameraReview");if(at>=0&&at+1<args.Length)new GameObject("Open type and activity camera review").AddComponent<PlayerIdentityCameraReview>().folder=args[at+1];}
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Settle(float seconds=.7f){yield return new WaitForSecondsRealtime(seconds);yield return new WaitForEndOfFrame();}
  bool Visible(Vector3 point,Rect safe){var p=cam.WorldToViewportPoint(point);return p.z>cam.nearClipPlane&&safe.Contains(p);}
  void Place(Athlete a,Vector3 point){a.capsule.enabled=false;a.transform.position=point;a.ResetLocomotion();a.capsule.enabled=true;Physics.SyncTransforms();}
  Vector3 Floor(Vector3 point){return Physics.Raycast(point+Vector3.up*15,Vector3.down,out var hit,30,1<<8,QueryTriggerInteraction.Ignore)?hit.point+Vector3.up*.04f:point;}
  void Capture(string name)=>FishingResultsChecks.Capture(Path.Combine(folder,"view.txt"),name,0);
  bool Labels(bool requireSelf=false){
   var placed=new List<Rect>();bool good=true;float scale=Mathf.Clamp(Screen.height/720f,.8f,2);
   foreach(var a in Athlete.Active){if(!a||!CharacterIdentityBadges.TryGetScreenRect(a,out var rect))continue;samples++;
    var head=cam.WorldToScreenPoint(a.GetComponent<CharacterIdentity>().LabelAnchor);var top=new Vector2(head.x,Screen.height-head.y);
    good&=Mathf.Abs(rect.center.x-top.x)<=18*scale+.1f&&top.y-rect.yMax>=4*scale&&top.y-rect.yMax<=46*scale;
    good&=!placed.Any(r=>r.Overlaps(rect));placed.Add(rect);
    good&=CharacterIdentityBadges.TryGetPresentation(a,out var number,out var name,out _,out var plate,out _)&&plate.a==0;
    good&=a==actor?number==""&&name=="You":number==a.GetComponent<CharacterIdentity>().Number.ToString("00");
   }
   return good&&(!requireSelf||CharacterIdentityBadges.TryGetScreenRect(actor,out _));
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   float until=Time.realtimeSinceStartup+180;while(!DevelopmentProbe.ReadyToReviewSport&&Time.realtimeSinceStartup<until)yield return null;
   app=AppRoot.Instance;Check(app&&app.Exploring,"playable scene initialized");if(!app||!app.Exploring){Finish();yield break;}
   actor=app.LocalAthlete;view=app.view;cam=Camera.main;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   yield return Settle(2);view.mode=2;view.pitch=16;
   if(app.SelectedSport==SportId.Golf)yield return Golf();
   else if(app.SelectedSport==SportId.Fishing)yield return Fishing();
   else {
    bool basket=app.SelectedSport==SportId.Basketball;Check(basket?BasketballBots.Instance.StartSolo(5):FootballBots.Instance.StartSolo(5),"ten-player roster created");
    if(basket){BasketballMatch.Instance.StartMatch();BasketballMatch.Instance.RequestTeam(FootballTeam.A);BasketballMatch.Instance.ConfirmSoloTeam();}
    else {FootballMatch.Instance.StartMatch();FootballMatch.Instance.RequestTeam(FootballTeam.A);FootballMatch.Instance.ConfirmSoloTeam();}
    until=Time.realtimeSinceStartup+25;while(Time.realtimeSinceStartup<until&&!(basket?BasketballMatch.Instance.Running:FootballMatch.Instance.Running)){
     var match=FootballMatch.Instance;if(!basket&&match.KickoffReady&&match.KickoffActor)match.TryKickoff(match.KickoffActor,match.BotKickoffHeading(match.KickoffActor),.3f,match.Snapshot.revision);yield return null;
    }
    yield return Settle(2);Check(view.StadiumFollowing,"court broadcast retained");Capture("stadium");
   }
   // Static scene plus live camera pans/animated head poses isolates label ownership.
   var roster=Athlete.Active.Where(a=>a&&a.isActiveAndEnabled&&!a.inTransit).ToArray();
   if(roster.Length==1&&IslandBots.Supports(app.SelectedSport)){IslandBots.Instance.StartSolo(4);yield return Settle();roster=IslandBots.Instance.Roster;}
   foreach(var a in roster){foreach(var bot in a.GetComponents<MonoBehaviour>().Where(b=>b is FootballBot||b is BasketballBot||b is GolfBot||b is FishingBot))bot.enabled=false;}
   app.enabled=false;view.enabled=false;SkySailWorld.Instance.reviewCamera=true;Time.timeScale=0;
   var center=actor.transform.position;
   if(app.SelectedSport==SportId.Basketball){var frame=BasketballBall.Active.transform.parent;var local=frame.InverseTransformPoint(center);center=frame.TransformPoint(0,local.y,0);}
   foreach(var a in roster)a.capsule.enabled=false;
   for(int i=0;i<roster.Length;i++){roster[i].transform.position=center+Vector3.right*(i-(roster.Length-1)*.5f)*1.6f;roster[i].HideHead(false);}
   cam.fieldOfView=50;cam.transform.position=center+new Vector3(0,4,-13);cam.transform.LookAt(center+Vector3.up);yield return Settle();
   Check(Labels(true),"open type: You only, transparent name background, stable numbers, attached and separated");Check(roster.Count(a=>CharacterIdentityBadges.TryGetScreenRect(a,out _))>=Mathf.Min(3,roster.Length),"lineup includes visible peer identities");Capture("open-type-lineup");
   bool tracked=true;for(int frame=0;frame<90;frame++){
    cam.transform.position=center+new Vector3(Mathf.Sin(frame*.04f)*2,4,-13);cam.transform.LookAt(center+Vector3.up);
    yield return new WaitForEndOfFrame();tracked&=Labels();
   }Check(tracked&&samples>90,"moving camera keeps labels attached without overlap, samples="+samples);
   var blocker=new GameObject("Review-only label occlusion");blocker.layer=8;blocker.AddComponent<BoxCollider>().size=Vector3.one;
   blocker.transform.position=Vector3.Lerp(cam.transform.position,actor.GetComponent<CharacterIdentity>().LabelAnchor,.5f);Physics.SyncTransforms();yield return Settle();
   Check(!CharacterIdentityBadges.TryGetScreenRect(actor,out _),"world obstruction hides label");Destroy(blocker);yield return null;
   for(int i=0;i<roster.Length;i++)roster[i].transform.position=center+new Vector3((i%3-1)*.35f,0,(i/3)*.4f);
   yield return Settle();Check(Labels(true),"crowd compresses or hides labels without sideways detachment");Capture("crowd");
   view.mode=0;yield return Settle();Check(!CharacterIdentityBadges.TryGetScreenRect(actor,out _),"own label hidden in eye-level camera");
   view.mode=2;actor.inTransit=true;yield return Settle();Check(!CharacterIdentityBadges.TryGetScreenRect(actor,out _),"travelling actor has no stale label");actor.inTransit=false;
   Time.timeScale=1;Finish();
  }
  IEnumerator Golf(){
   var match=GolfMatchManager.Instance;float until=Time.realtimeSinceStartup+20;bool started=false;
   while(!started&&Time.realtimeSinceStartup<until){started=match.StartMatch();if(!started)yield return null;}
   Check(started,"Golf starts through normal match API");yield return Settle(1);
   var ball=match.Ball(actor);if(!ball){Check(false,"Golf ball exists");yield break;}
   var tee=match.Course.holes[0].tee;ball.Place(match.TeePosition(1,ball.Owner),true);Place(actor,Floor(tee.position-tee.forward*2.6f));view.yaw=tee.eulerAngles.y;yield return Settle(1);
   Check(view.ActivityStadiumFollowing&&cam.transform.eulerAngles.x>=28&&cam.transform.eulerAngles.x<=42,"Golf course angle replaces steep generic overhead");Capture("course-roam");
   Check(view.BeginGolfAim(),"Golf accepts aim from Course view");yield return Settle(1);
   var safe=new Rect(.06f,.12f,.88f,.8f);Check(view.GolfAiming&&view.GolfPreview&&view.GolfPreview.Visible,"Golf shot guide remains active");
   Check(Visible(ball.transform.position,safe)&&Visible(view.GolfPreview.EndPoint,safe)&&Visible(actor.transform.position+Vector3.up,safe),"Golf frames golfer, ball and landing point");Capture("course-aim");
   Check(view.BeginGolfSwing(831),"Golf power begins");view.SetGolfPower(831,.7f);yield return Settle(.5f);
   Check(Visible(ball.transform.position,safe)&&Visible(view.GolfPreview.EndPoint,safe),"charged shot landing area stays visible");Capture("course-power");view.EndGolfSwing(831);
   until=Time.realtimeSinceStartup+5;while(!view.GolfFollowingBall&&Time.realtimeSinceStartup<until)yield return null;
   Check(view.GolfFollowingBall,"confirmed physical swing starts flight camera");yield return Settle(.8f);Capture("course-flight");
   bool tracked=true;int frames=0;until=Time.realtimeSinceStartup+4;while(view.GolfFollowingBall&&Time.realtimeSinceStartup<until){yield return new WaitForEndOfFrame();tracked&=Visible(ball.transform.position,new Rect(.04f,.04f,.92f,.92f));frames++;}
   Check(tracked&&frames>5,"actual moving ball remains visible throughout sampled flight");view.SkipGolfReplay();yield return Settle(1.2f);Check(view.ActivityStadiumFollowing&&!view.GolfShotCameraActive,"skip returns to Course mode");
   view.mode=1;yield return Settle();Check(!view.ActivityStadiumFollowing&&Mathf.Abs(cam.fieldOfView-60)<.01f,"Follow restores original lens");view.mode=2;yield return Settle();
  }
  IEnumerator Fishing(){
   var game=FishingGame.Instance;var pier=game.Pier(0);Place(actor,Floor(pier));var direction=game.transform.position-pier;view.yaw=Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg;view.pitch=16;view.mode=2;yield return Settle(1);
   Check(view.ActivityStadiumFollowing&&!cam.orthographic,"Fishing Lagoon angle active");Check(Visible(actor.transform.position+Vector3.up,new Rect(.05f,.12f,.9f,.8f)),"Lagoon view retains angler");
   int fish=Enumerable.Range(0,game.State.Fish.Count).Where(i=>game.VisibleTarget(actor,cam,i)).DefaultIfEmpty(-1).First();Check(fish>=0,"castable fish visible from real pier");Capture("lagoon-cast");
   view.mode=1;view.Switch();Check(view.mode==2,"camera shortcut now reaches Lagoon");view.Switch();Check(view.mode==0,"Lagoon cycles to Eye level");view.mode=2;
   if(fish<0)yield break;
   Check(view.SelectFishingFish(fish)&&view.BeginFishing(832),"real cast input accepted in Lagoon");yield return Settle(.5f);
   Check(game.Busy(actor),"authoritative cast begins");Capture("lagoon-line");
   float until=Time.realtimeSinceStartup+8;while(game.Player(actor).Value.phase!=FishingPhase.Bite&&Time.realtimeSinceStartup<until)yield return null;
   Check(game.Player(actor).Value.phase==FishingPhase.Bite&&view.BeginFishing(833),"real bite can be hooked from Lagoon");yield return Settle(.5f);
   Check(game.Player(actor).Value.phase==FishingPhase.Reeling,"real hook enters reeling");Check(cam.WorldToViewportPoint(actor.transform.position).y>.32f,"angler feet clear the bottom fight panel");Capture("lagoon-reel");
   bool framed=true;int frames=0;until=Time.realtimeSinceStartup+2;
   while(game.Player(actor).Value.phase==FishingPhase.Reeling&&Time.realtimeSinceStartup<until){yield return new WaitForEndOfFrame();framed&=Visible(game.Presentation.HookPoint(game.Player(actor).Value),new Rect(.06f,.12f,.88f,.8f))&&Visible(actor.transform.position+Vector3.up,new Rect(.04f,.1f,.92f,.85f));frames++;}
   Check(framed&&frames>10,"angler and moving hooked fish remain visible");view.CancelFishing();yield return Settle(1);Check(view.ActivityStadiumFollowing&&!game.Busy(actor),"cancel retains Lagoon and returns casting view");
   view.mode=0;yield return Settle();Check(Vector3.Distance(cam.transform.position,actor.transform.position+Vector3.up*1.57f)<.1f&&Mathf.Abs(cam.fieldOfView-60)<.01f,"Fishing Eye level restores eyes and lens");view.mode=2;yield return Settle();
  }
  void Finish(){Time.timeScale=1;File.AppendAllText(Path.Combine(folder,"results.txt"),"IDENTITY_CAMERA_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
