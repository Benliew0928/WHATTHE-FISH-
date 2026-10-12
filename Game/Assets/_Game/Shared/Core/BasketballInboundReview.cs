#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 public sealed class BasketballInboundReview:MonoBehaviour {
  string folder;bool failed;AppRoot app;BasketballBall ball;Athlete actor;Transform arena;BasketballShootButton pass;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-basketballInboundReview");if(i>=0&&i+1<args.Length)new GameObject("Basketball inbound review").AddComponent<BasketballInboundReview>().folder=args[i+1];}
  void Check(bool ok,string message){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+message+"\n");}
  void PlaceActor(Vector3 local){actor.capsule.enabled=false;actor.transform.position=arena.TransformPoint(local);actor.capsule.enabled=true;actor.ResetLocomotion();Physics.SyncTransforms();}
  void PlaceBall(Vector3 local,Vector3 velocity=default){ball.Place(local,Quaternion.identity,arena.TransformDirection(velocity),Vector3.zero);}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");float end=Time.realtimeSinceStartup+90;
   while((!(app=AppRoot.Instance)||!app.Exploring||app.SelectedSport!=SportId.Basketball||!BasketballHUD.Instance)&&Time.realtimeSinceStartup<end)yield return null;
   ball=BasketballBall.Active;actor=app?app.LocalAthlete:null;if(!ball||!actor){Check(false,"practice initialized");Finish();yield break;}
   arena=ball.transform.parent;pass=FindObjectsByType<BasketballShootButton>(FindObjectsSortMode.None).Single(b=>b.pass);
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;app.view.mode=2;yield return new WaitForSeconds(.5f);
   ball.autoPickup=false;ball.Body.useGravity=false;
   var bounds=BasketballBall.PlayerCourtLimits;var wood=app.stadium.GetComponentsInChildren<MeshFilter>().Single(f=>f.name=="Court__Maple").GetComponent<Renderer>().bounds;
   Check(bounds==BasketballBall.PaintedCourtLimits&&Mathf.Abs(wood.size.x-bounds.x*2)<.01f&&Mathf.Abs(wood.size.z-bounds.y*2)<.01f,"actual painted artwork, wall and rules dimensions agree");
   var mesh=arena.GetComponentInChildren<BasketballBoundaryVisual>().GetComponent<MeshFilter>().sharedMesh;
   Check(Mathf.Abs(mesh.bounds.extents.x-bounds.x)<.001f&&Mathf.Abs(mesh.bounds.extents.z-bounds.y)<.001f,"translucent wall mesh follows both touchlines");
   Check(Mathf.Abs(BasketballPassRules.Power(.2)-.5f)<.001f&&BasketballPassRules.Power(.4)==1,"half/full charge at 0.2 / 0.4 seconds");
   foreach(float power in new[]{0f,.5f,1f})foreach(float bend in new[]{-1f,0f,1f}){
    var path=BasketballPassPath.Create(new Vector3(0,.83f,0),0,power,bend,0);float oldRange=bend<0?Mathf.Lerp(3,7.5f,power):Mathf.Lerp(4,10,power);
    float oldTime=bend<0?.30f+.28f*.85f:bend>0?2*Mathf.Sqrt(2*Mathf.Lerp(1.25f,2.8f,power)/-Physics.gravity.y):oldRange/Mathf.Lerp(8,13,power);
    Check(Mathf.Abs(path.velocity.z/(oldRange/oldTime)-1.5f)<.001f,"150 percent horizontal pass speed: power="+power+" bend="+bend);
   }
   foreach(var direction in new[]{Vector3.right,Vector3.left,Vector3.forward,Vector3.back}){
    ball.ResetHome();app.view.ClearMatchInput();PlaceActor(new Vector3(0,.07f,-3));
    var start=Mathf.Abs(direction.x)>0?new Vector3(direction.x*(bounds.x-.15f),3,2):new Vector3(3,3,direction.z*(bounds.y-.15f));
    PlaceBall(start,direction*3);yield return new WaitForSeconds(1.6f);Check(!ball.InboundPending&&!ball.Held,"no restart before two continuous seconds outside "+direction);
    yield return new WaitForSeconds(.7f);Check(ball.IsInbound(actor)&&ball.Held,"high out-of-bounds ball becomes held inbound "+direction);
    var p=arena.InverseTransformPoint(actor.transform.position);var exit=arena.InverseTransformPoint(ball.InboundExit);
    Check(Mathf.Abs(direction.x)>0?Mathf.Abs(exit.x-direction.x*bounds.x)<.01f&&Mathf.Abs(exit.z-2)<.02f:Mathf.Abs(exit.z-direction.z*bounds.y)<.01f&&Mathf.Abs(exit.x-3)<.02f,"restart remembers crossing, not later rolling position "+direction);
    Check(Mathf.Abs(direction.x)>0?Mathf.Abs(p.x)>bounds.x&&Mathf.Abs(p.z-2)<.1f:Mathf.Abs(p.z)>bounds.y&&Mathf.Abs(p.x-3)<.1f,"player stands just outside original exit "+direction);
    Check(!ball.CanShoot(actor)&&ball.CanPass(actor)&&!ball.BeginShotCharge(actor,0)&&!ball.TryShoot(actor,0)&&!actor.CanRequestJump,"inbound permits passing and rejects shooting/jumping");
    ball.ReviewGiveProp(actor,SportsProp.Turbo);Check(!ball.TryUseProp(actor,ball.PropSlot(actor).revision),"inbound rejects props");
    Vector3 before=actor.transform.position;DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.one,sprint=true,jump=true,heading=app.view.yaw};yield return new WaitForSeconds(.4f);DevelopmentProbe.TurnCommand=new PlayerCommand{heading=app.view.yaw};
    Check(Vector3.ProjectOnPlane(actor.transform.position-before,Vector3.up).magnitude<.01f&&!actor.Airborne,"standing restriction survives movement, sprint and jump input");
    if(direction==Vector3.right){
     yield return Capture("inbound-ready");Check(app.view.BeginPass(81),"inbound pass begins");end=Time.time+1;while(!ball.IsPassAiming(actor)&&Time.time<end)yield return null;Check(ball.IsPassAiming(actor),"host observes inbound charge begin");double chargeStart=ball.PassAim.started;while(ball.PassClock-chargeStart<.4&&Time.time<end)yield return null;Check(ball.PassPower>.99f,"host reaches full charge at 0.4 seconds from accepted input: power="+ball.PassPower+" elapsed="+(ball.PassClock-chargeStart));app.view.CancelPass(81);yield return new WaitForSeconds(.15f);
     Check(ball.IsInbound(actor)&&ball.Held&&actor.BasketballMotion.PassCharging,"cancel keeps standing two-hand inbound hold");
     var hud=BasketballHUD.Instance;hud.ShowSettings();yield return null;Check(ball.IsInbound(actor)&&app.view.InputBlocked,"settings preserves inbound restriction");yield return Capture("settings-gear");hud.CloseMenu();
    }
    uint passes=ball.PassCount;Check(app.view.BeginPass(82),"restart accepts fresh held pass");yield return new WaitForSeconds(.1f);app.view.EndPass(82);Check(ball.IsInbound(actor),"queued pass does not unlock feet early");
    end=Time.time+2;while(ball.PassCount==passes&&Time.time<end)yield return null;
    Check(ball.PassCount==passes+1&&!ball.InboundPending&&!ball.Held,"physical pass releases the inbound lock exactly once");
    before=actor.transform.position;DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,sprint=true,heading=app.view.yaw};yield return new WaitForSeconds(.65f);DevelopmentProbe.TurnCommand=default;
    p=arena.InverseTransformPoint(actor.transform.position);Check(Vector3.Distance(before,actor.transform.position)>.6f&&Mathf.Abs(p.x)<bounds.x&&Mathf.Abs(p.z)<bounds.y,"player can re-enter after successful pass");
   }
   // The original stand bug: an elevated, stationary ball beyond the baseline.
   foreach(float sign in new[]{-1f,1f}){
    ball.ResetHome();PlaceActor(new Vector3(0,.07f,0));PlaceBall(new Vector3(0,2.8f,sign*(bounds.y+1)));yield return new WaitForSeconds(2.2f);
    Check(ball.IsInbound(actor),"stuck stand ball recovers at baseline "+sign);
    var p=actor.transform.position;float r=actor.capsule.radius+.02f,h=actor.capsule.height;var c=p+actor.capsule.center;
    Check(!Physics.CheckCapsule(c+Vector3.up*(h*.5f-r),c-Vector3.up*(h*.5f-r),r,1<<8,QueryTriggerInteraction.Ignore),"inbound standing spot clears the basket support "+sign);
    Check(Mathf.Abs(arena.InverseTransformPoint(p).z)>bounds.y,"stand recovery stays outside same baseline");
    if(sign>0)yield return Capture("basket-stand-restart");
    uint count=ball.PassCount;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=app.view.yaw};Check(ball.TryPass(actor,app.view.yaw),"support-side inbound can queue a pass");end=Time.time+2;while(ball.PassCount==count&&Time.time<end)yield return null;Check(ball.PassCount==count+1,"support does not obstruct inward release");
   }
   ball.ResetHome();PlaceBall(new Vector3(bounds.x+.3f,3,0));yield return new WaitForSeconds(1.3f);
   // Move the same physical ball, preserving its timer/history (Place deliberately resets both).
   ball.Body.position=arena.TransformPoint(new Vector3(bounds.x-.2f,3,0));yield return new WaitForSeconds(.1f);ball.Body.position=arena.TransformPoint(new Vector3(bounds.x+.3f,3,0));yield return new WaitForSeconds(1.2f);Check(!ball.InboundPending,"return inside resets the continuous outside timer");yield return new WaitForSeconds(1);Check(ball.IsInbound(actor),"second continuous excursion restarts after its own two seconds");
   ball.ResetHome();PlaceBall(new Vector3(bounds.x+.3f,-4,2));yield return new WaitForSeconds(.3f);Check(!ball.Held&&ball.transform.localPosition.y<0,"out-of-court fall does not bypass the two-second rule");yield return new WaitForSeconds(1.9f);Check(ball.IsInbound(actor),"out-of-court fall recovers at its exit");
   ball.ResetHome();PlaceBall(new Vector3(bounds.x+.3f,3,bounds.y+.3f));yield return new WaitForSeconds(2.2f);Check(ball.IsInbound(actor),"corner exit has a clear inbound standing position");
   ball.SetFreeRoam(actor,true);Check(!ball.InboundPending&&!ball.Held,"leaving practice clears inbound restriction");ball.SetFreeRoam(actor,false);
   Finish();
  }
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();var camera=Camera.main;var canvas=BasketballHUD.Instance.GetComponentInParent<Canvas>();var mode=canvas.renderMode;var oldCamera=canvas.worldCamera;float plane=canvas.planeDistance;
   canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var old=RenderTexture.active;var target=camera.targetTexture;
   camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);camera.targetTexture=target;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);canvas.renderMode=mode;canvas.worldCamera=oldCamera;canvas.planeDistance=plane;Canvas.ForceUpdateCanvases();
  }
  void Finish(){if(ball){ball.Body.useGravity=true;ball.autoPickup=true;}DevelopmentProbe.TurnCommandActive=false;DevelopmentProbe.TurnCommand=default;File.AppendAllText(Path.Combine(folder,"results.txt"),"BASKETBALL_INBOUND_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
