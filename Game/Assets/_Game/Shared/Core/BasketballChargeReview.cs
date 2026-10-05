#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 public sealed class BasketballChargeReview:MonoBehaviour {
  string folder;bool failed;AppRoot app;BasketballBall ball;Athlete actor;Transform hoop;Camera camera;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-basketballChargeReview");if(i>=0&&i+1<args.Length)new GameObject("Basketball charge review").AddComponent<BasketballChargeReview>().folder=args[i+1];}
  void Check(bool ok,string message){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+message+"\n");}
  void Place(Vector3 position,float heading){actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(position,Quaternion.Euler(0,heading,0));actor.capsule.enabled=true;actor.ResetLocomotion();Physics.SyncTransforms();app.view.yaw=heading;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=heading};}
  IEnumerator Pickup(float distance){
   app.view.ClearMatchInput();var p=hoop.TransformPoint(new Vector3(0,.07f,distance));float heading=Quaternion.LookRotation(hoop.position-p).eulerAngles.y;Place(p,heading);ball.ResetHome();ball.autoPickup=true;ball.Place(ball.transform.parent.InverseTransformPoint(p+actor.transform.forward*.6f+Vector3.up*.18f),Quaternion.identity,Vector3.zero,Vector3.zero);
   float end=Time.time+2;while(ball.Holder!=actor&&Time.time<end)yield return null;yield return new WaitForSeconds(.1f);Check(ball.Holder==actor,"pickup at "+distance+" m");
  }
  IEnumerator Green(){float end=Time.time+4;while(Time.time<end){if(Mathf.Abs(app.view.ShotPower-BasketballBall.SweetSpot)<=BasketballBall.SweetWindow)yield break;yield return null;}Check(false,"green window reached");}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");float end=Time.realtimeSinceStartup+80;
   while((!(app=AppRoot.Instance)||!app.Exploring||app.SelectedSport!=SportId.Basketball)&&Time.realtimeSinceStartup<end)yield return null;
   ball=BasketballBall.Active;actor=app?app.LocalAthlete:null;if(!ball||!actor){Check(false,"initialized");Finish();yield break;}
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.4f);
   hoop=app.stadium.GetComponentsInChildren<Transform>().Single(t=>t.name=="Hoop_North");
   camera=new GameObject("Charge review camera").AddComponent<Camera>();camera.CopyFrom(Camera.main);camera.enabled=false;
   yield return Pickup(4);float heading=actor.transform.eulerAngles.y;
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.right,heading=heading};yield return new WaitForSeconds(.6f);float walk=actor.speed;
   Check(app.view.BeginShot(11),"begin charging while walking");yield return new WaitForSeconds(.7f);float charged=actor.speed;
   Check(walk>3.7f&&charged>1.7f&&charged<2.2f,"charging walk speed halves: "+walk.ToString("F2")+" -> "+charged.ToString("F2"));
   Check(actor.BasketballMotion.Charging&&ball.Held,"held ball visibly gathers while charge stays active");
   Check(ball.CarryPosition(actor).y-actor.transform.position.y>.94f,"two-hand aim holds ball above dribble height");
   float maxReach=0,maxJump=0,maxPalmError=0;Vector3 previous=ball.transform.position;end=Time.time+.7f;
   while(Time.time<end){yield return new WaitForEndOfFrame();maxReach=Mathf.Max(maxReach,actor.BasketballMotion.MaximumReachError);maxPalmError=Mathf.Max(maxPalmError,Vector3.Distance(actor.BasketballMotion.RightPalm,ball.CarryPosition(actor)-Vector3.up*.13125f-actor.BasketballMotion.PoseFacing*Vector3.forward*.01875f));maxJump=Mathf.Max(maxJump,Vector3.Distance(previous,ball.transform.position));previous=ball.transform.position;}
   Check(maxReach<.055f&&maxJump<.20f,"walking aim stays reachable and continuous: reach="+maxReach.ToString("F3")+" step="+maxJump.ToString("F3"));
   Check(maxPalmError<.04f,"shooting palm stays on held ball throughout moving aim: "+maxPalmError.ToString("F3"));
   DevelopmentProbe.TurnCommand=new PlayerCommand{heading=heading};yield return new WaitForSeconds(.3f);yield return new WaitForEndOfFrame();Capture("aim-close",actor.transform.position+actor.transform.forward*2.7f+actor.transform.right*2+Vector3.up*1.55f,actor.transform.position+Vector3.up*.9f);
   app.view.EndShot(12);Check(app.view.ShotCharging,"foreign pointer cannot release");uint shots=ball.ShotCount;app.view.CancelShot(11);yield return new WaitForSeconds(.5f);
   Check(ball.Held&&!ball.IsCharging(actor)&&!actor.BasketballMotion.Charging&&ball.ShotCount==shots,"cancel lowers hands to dribble and preserves possession");
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.left,heading=heading};yield return new WaitForSeconds(.5f);Check(actor.speed>3.7f,"normal walking speed returns after cancel");
   yield return Pickup(4);DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.left,heading=heading,sprint=true};app.view.BeginShot();yield return new WaitForSeconds(.65f);Check(actor.speed>3.1f&&actor.speed<3.8f,"sprinting charge capped near half speed");app.view.CancelShot();
   float nearPeriod=0,farPeriod=0;
   foreach(float distance in new[]{4f,12f,22f}){
    yield return Pickup(distance);app.view.BeginShot();yield return new WaitForSeconds(.1f);float period=ball.Charge.period;if(distance==4)nearPeriod=period;if(distance==22)farPeriod=period;
    float min=1,max=0;int greenPasses=0;bool green=false;end=Time.time+period*4.2f;
    while(Time.time<end){float power=app.view.ShotPower;min=Mathf.Min(min,power);max=Mathf.Max(max,power);bool next=Mathf.Abs(power-BasketballBall.SweetSpot)<=BasketballBall.SweetWindow;if(next&&!green)greenPasses++;green=next;yield return null;}
    Check(min<.09f&&max>.91f&&greenPasses>=3,"needle loops with repeated green chances at "+distance+"m: "+greenPasses+" windows");
    Check(ball.Held&&ball.ShotCount==shots,"holding across cycles never auto-fires at "+distance+"m");app.view.CancelShot();yield return new WaitForSeconds(.3f);
   }
   Check(farPeriod<nearPeriod*.55f&&farPeriod>=.419f,"far-court needle moves twice as fast without narrowing normalized band");
   yield return Pickup(12);app.view.BeginShot();yield return new WaitForSeconds(.4f);var locked=ball.ChargeHoop(actor,app.view.yaw);double phase=ball.PresentedChargePhase;float oldPeriod=ball.Charge.period;
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,heading=actor.transform.eulerAngles.y,sprint=true};app.view.yaw+=180;yield return new WaitForSeconds(.6f);DevelopmentProbe.TurnCommand=new PlayerCommand{heading=actor.transform.eulerAngles.y};
   Check(ball.ChargeHoop(actor,app.view.yaw)==locked,"turning camera during a hold cannot switch to a slower basket");
   Check(ball.Charge.period>oldPeriod&&ball.PresentedChargePhase>phase,"moving closer smoothly integrates a slower needle without resetting phase");app.view.CancelShot();yield return new WaitForSeconds(.3f);
   yield return Pickup(5);var button=FindObjectsByType<BasketballShootButton>(FindObjectsSortMode.None).First(b=>!b.pass);yield return null;
   var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=41};button.OnPointerDown(pointer);yield return new WaitForSeconds(.5f);
   Check(button.cancelArea.gameObject.activeSelf,"explicit cancel target appears during touch hold");
   pointer.position=RectTransformUtility.WorldToScreenPoint(null,button.cancelArea.position)+new Vector2(0,200);button.OnPointerExit(pointer);Check(app.view.ShotCharging,"dragging outside Shoot keeps original finger tracking");
   pointer.position=RectTransformUtility.WorldToScreenPoint(null,button.cancelArea.position);button.OnDrag(new PointerEventData(EventSystem.current){pointerId=42,position=pointer.position});Check(app.view.ShotCharging,"other finger cannot trigger drag cancel");
   button.OnDrag(pointer);button.OnPointerUp(pointer);yield return new WaitForSeconds(.4f);Check(!app.view.ShotCharging&&ball.Held&&ball.ShotCount==shots,"drag to Cancel then lift cannot queue a shot");
   yield return Pickup(5);app.view.BeginShot();yield return new WaitForSeconds(5.5f);Check(app.view.ShotCharging&&ball.IsCharging(actor)&&actor.BasketballMotion.Charging,"holding beyond five seconds retains repeat chances and aiming pose");
   yield return Green();float displayed=app.view.ShotPower;app.view.EndShot();end=Time.time+2;while(ball.ShotCount==shots&&Time.time<end)yield return null;
   Check(ball.ShotCount==shots+1&&Mathf.Abs(ball.LastReleasePower-displayed)<.018f,"release uses displayed needle phase without an extra input-frame penalty");
   end=Time.time+6;while(ball.Score.result==BasketballResult.Flying&&Time.time<end)yield return null;Check(ball.Score.result==BasketballResult.Scored,"later-cycle green release scores through physical hoop");
   yield return Pickup(5);app.view.BeginShot();yield return new WaitForSeconds(.7f);uint passes=ball.PassCount;app.view.RequestPass();end=Time.time+2;while(ball.PassCount==passes&&Time.time<end)yield return null;Check(ball.PassCount==passes+1&&!ball.IsCharging(actor),"pass cleanly replaces a charged aim");
   yield return Boundary();Finish();
  }
  IEnumerator Boundary(){
   app.view.ClearMatchInput();ball.autoPickup=false;ball.ResetHome();var arena=ball.transform.parent;var limits=BasketballBall.PlayerCourtLimits;Place(arena.TransformPoint(new Vector3(5,.07f,0)),90);yield return new WaitForSeconds(.3f);
   var wall=arena.GetComponentInChildren<BasketballBoundaryVisual>();Check(wall&&wall.GetComponent<MeshFilter>().sharedMesh.vertexCount==16&&wall.GetComponents<Collider>().Length==0,"lightweight four-panel translucent boundary has no physics collider");
   Check(wall.GetComponent<MeshRenderer>().sharedMaterial==Resources.Load<Material>("FootballBoundary"),"basketball shares football energy material and shader");
   foreach(float side in new[]{-1f,1f}){
    Check(BasketballBoundaryVisual.Crossed(new Vector3(side*6,1,0),new Vector3(side*9,1,0),out var hit)&&Mathf.Abs(hit.x-side*limits.x)<.001f,"fast sideline crossing sampled "+side);
    Check(BasketballBoundaryVisual.Crossed(new Vector3(0,1,side*13),new Vector3(0,1,side*16),out hit)&&Mathf.Abs(hit.z-side*limits.y)<.001f,"fast baseline crossing sampled "+side);
   }
   Check(!BasketballBoundaryVisual.Crossed(new Vector3(6,3,0),new Vector3(9,3,0),out _),"ball over top does not flash untouched wall");
   uint contacts=wall.PlayerPulses;DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,heading=90,sprint=true};yield return new WaitForSeconds(.65f);DevelopmentProbe.TurnCommand=new PlayerCommand{heading=90};
   Check(wall.PlayerPulses>contacts&&actor.transform.position.x<limits.x,"player contact creates an expanding pulse while motor holds boundary");
   yield return new WaitForEndOfFrame();Capture("barrier-player-contact",arena.TransformPoint(new Vector3(3.8f,2.3f,4.2f)),arena.TransformPoint(new Vector3(7.4f,.8f,0)));
   Place(arena.TransformPoint(new Vector3(4,.07f,-5)),90);yield return new WaitForSeconds(.3f);uint crossings=wall.BallPulses;
   ball.Body.useGravity=false;ball.Place(new Vector3(6.4f,1.1f,0),Quaternion.identity,arena.TransformDirection(Vector3.right*9),Vector3.zero);
   yield return new WaitForSeconds(.22f);Check(wall.BallPulses>crossings&&ball.transform.localPosition.x>limits.x,"flying basketball passes through and leaves an animated ripple");
   yield return new WaitForEndOfFrame();Capture("barrier-ball-ripple",arena.TransformPoint(new Vector3(3.8f,2.3f,4.2f)),arena.TransformPoint(new Vector3(7.4f,.8f,0)));
   crossings=wall.BallPulses;ball.ResetHome();yield return new WaitForSeconds(.15f);Check(wall.BallPulses==crossings,"ball recovery teleport does not create a false crossing");ball.Body.useGravity=true;
   for(int i=0;i<12;i++){yield return new WaitForSeconds(.1f);yield return new WaitForEndOfFrame();Capture("barrier-fade-"+i.ToString("D2"),arena.TransformPoint(new Vector3(3.8f,2.3f,4.2f)),arena.TransformPoint(new Vector3(7.4f,.8f,0)));}
   yield return new WaitForEndOfFrame();Capture("court-wide",arena.TransformPoint(new Vector3(4,7,-12)),arena.TransformPoint(new Vector3(0,.5f,2)));
  }
  void Capture(string name,Vector3 position,Vector3 focus){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
   camera.transform.position=position;camera.transform.LookAt(focus);camera.fieldOfView=52;var rt=RenderTexture.GetTemporary(1280,720,24);var old=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);camera.targetTexture=null;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);
  }
  void Finish(){DevelopmentProbe.TurnCommand=default;File.AppendAllText(Path.Combine(folder,"results.txt"),"BASKETBALL_CHARGE_COMPLETE success="+!failed+"\n");}
 }
}
#endif
