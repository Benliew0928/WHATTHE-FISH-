#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 // Opt-in rendered integration checks. All fixtures and captures stay out of
 // release players; assertions exercise the same host rules as normal play.
 public sealed class BasketballPhysicsReview:MonoBehaviour {
  string folder;bool failed;AppRoot app;BasketballBall ball;Athlete actor;Camera reviewCamera;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-basketballPhysicsReview");if(i<0||i+1>=args.Length)return;
   new GameObject("Basketball physics review").AddComponent<BasketballPhysicsReview>().folder=args[i+1];
  }
  void Check(bool ok,string message){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+message+"\n");}
  void PlaceActor(Vector3 position,float yaw){actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));actor.capsule.enabled=true;actor.ResetLocomotion();Physics.SyncTransforms();}
  IEnumerator Pickup(BasketballHoop hoop,float distance){
   var point=hoop.transform.TransformPoint(new Vector3(0,.07f,distance));float yaw=Quaternion.LookRotation(hoop.transform.position-point).eulerAngles.y;PlaceActor(point,yaw);
   DevelopmentProbe.TurnCommand=new PlayerCommand{heading=yaw};app.view.yaw=yaw;
   ball.autoPickup=true;ball.Place(ball.transform.parent.InverseTransformPoint(point+actor.transform.forward*.6f+Vector3.up*.15f),Quaternion.identity,Vector3.zero,Vector3.zero);
   float end=Time.time+2;while(ball.Holder!=actor&&Time.time<end)yield return null;
   Check(ball.Holder==actor,"pickup for "+hoop.name+" at "+distance+"m");
  }
  IEnumerator WaitGreen(){
   float end=Time.time+4;while(Time.time<end){float p=app.view.ShotPower;if(app.view.ShotCharging&&Mathf.Abs(p-BasketballBall.SweetSpot)<=BasketballBall.SweetWindow)yield break;yield return null;}
   Check(false,"live needle reaches a green window");
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   float end=Time.realtimeSinceStartup+80;
   while((!(app=AppRoot.Instance)||!app.Exploring||app.SelectedSport!=SportId.Basketball)&&Time.realtimeSinceStartup<end)yield return null;
   ball=BasketballBall.Active;actor=app?app.LocalAthlete:null;
   if(!ball||!actor){Check(false,"basketball initialized");Finish();yield break;}
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.4f);
   var hoops=app.stadium.GetComponentsInChildren<BasketballHoop>().OrderBy(h=>h.name).ToArray();
   Check(hoops.Length==2,"both modular hoops have responsive nets");if(hoops.Length!=2){Finish();yield break;}
   Check(Mathf.Abs(ball.GetComponent<SphereCollider>().radius-.15f)<.00001f,"30 cm physical diameter");
   var visual=ball.transform.Find("Visual");Check(visual&&Mathf.Abs(visual.localScale.x-1.25f)<.00001f,"visual enlarged 25 percent with original meshes");
   Check(FindFirstObjectByType<BasketballHUD>(),"scoreboard and hold/release meter present");
   Check(Mathf.Abs(BasketballBall.ChargeDuration-.84f)<.00001f&&Mathf.Abs(BasketballBall.ChargeDuration*BasketballBall.SweetSpot-.546f)<.00001f,"charge and ideal hold take 30 percent less time");
   foreach(var hoop in hoops){
    var responsive=hoop.transform.Find("Responsive net");Check(responsive&&responsive.GetComponent<MeshFilter>().sharedMesh.vertexCount==1152,hoop.name+" lightweight 1152-vertex cord mesh");
    var a=new Vector3(0,3.5f,0);var b=new Vector3(0,2.6f,0);float radius=BasketballBall.Radius;
    hoop.ResetCrossing();Check(hoop.Crossed(a,b,radius,true),hoop.name+" high-speed downward sweep counts");
    hoop.ResetCrossing();Check(!hoop.Crossed(b,a,radius,true),hoop.name+" upward entry rejected");
    hoop.ResetCrossing();Check(!hoop.Crossed(a+Vector3.right*.2f,b+Vector3.right*.2f,radius,true),hoop.name+" rim/side crossing rejected");
    hoop.ResetCrossing();Check(!hoop.Crossed(new Vector3(.4f,2.9f,0),b,radius,true),hoop.name+" net side entry rejected");
    hoop.ResetCrossing();Check(!hoop.Crossed(a,new Vector3(0,3,0),radius,true),hoop.name+" centre entry waits for whole ball");
    Check(!hoop.Crossed(new Vector3(0,3,0),a,radius,true),hoop.name+" rim bounce back upward cancels entry");
    Check(!hoop.Crossed(a,b,radius,false),hoop.name+" resolved or non-shot ball cannot score again");
    hoop.ResetCrossing();
   }
   Check(BasketballBall.ShotValue(new Vector3(0,0,7.239f))==2&&BasketballBall.ShotValue(new Vector3(0,0,7.4f))==3,"three-point arc matches authored line");
   Check(BasketballBall.ShotValue(new Vector3(6.7056f,0,0))==2&&BasketballBall.ShotValue(new Vector3(6.8f,0,0))==3,"corner line matches authored court");
   Check(!ball.ReleaseShotCharge(actor,0),"release without host-observed hold rejected");
   Check(!ball.TryShoot(actor,0,float.NaN)&&!ball.TryShoot(actor,0,1.1f),"invalid power rejected");
   yield return Pickup(hoops[0],5);
   var button=FindObjectsByType<BasketballShootButton>(FindObjectsSortMode.None).First(b=>!b.pass);yield return null;
   var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=7};
   uint count=ball.ShotCount;button.OnPointerDown(pointer);yield return new WaitForSeconds(.35f);
   button.OnPointerUp(new PointerEventData(EventSystem.current){pointerId=8});
   Check(app.view.ShotCharging&&ball.ShotCount==count,"unrelated touch cannot release charge");
   pointer.position=RectTransformUtility.WorldToScreenPoint(null,button.cancelArea.position);button.OnDrag(pointer);button.OnPointerUp(pointer);yield return new WaitForSeconds(.7f);
   Check(!app.view.ShotCharging&&ball.Held&&ball.ShotCount==count,"drag into Cancel keeps possession without a shot");
   reviewCamera=new GameObject("Basketball close review camera").AddComponent<Camera>();reviewCamera.CopyFrom(Camera.main);reviewCamera.enabled=false;
   // Capture the UI separately: synchronous GPU readback must not delay the
   // queued release edge and turn a deliberately perfect hold into an overshot.
   app.view.BeginShot();yield return WaitGreen();
   Check(Mathf.Abs(app.view.ShotPower-BasketballBall.SweetSpot)<BasketballBall.SweetWindow,"live distance-based meter reaches a green window");
   yield return new WaitForEndOfFrame();CapturePlayer("power-meter",true);app.view.CancelShot();yield return null;
   foreach(var trial in new[]{(hoops[0],5f,BasketballBall.SweetSpot,true),(hoops[0],10f,BasketballBall.SweetSpot,true),(hoops[1],5f,BasketballBall.SweetSpot,true),(hoops[0],5f,.10f,false),(hoops[0],5f,1f,false)}){
    yield return Trial(trial.Item1,trial.Item2,trial.Item3,trial.Item4);
   }
   ball.autoPickup=false;PlaceActor(new Vector3(-4,.07f,0),0);
   var testHoop=hoops[0];var before=ball.Score;var hitBefore=testHoop.Hit.sequence;
   ball.Place(ball.transform.parent.InverseTransformPoint(testHoop.transform.TransformPoint(new Vector3(.34f,2.80f,0))),Quaternion.identity,testHoop.transform.TransformDirection(Vector3.left*2),Vector3.zero);
   yield return new WaitForSeconds(.12f);yield return new WaitForEndOfFrame();
   Check(testHoop.Hit.sequence>hitBefore&&testHoop.Hit.kind==BasketballNetKind.Touch&&testHoop.Displacement>.001f,"outside net touch visibly ripples without scoring");
   Check(ball.Score.points==before.points&&ball.Score.made==before.made,"net touch alone awards no points");CaptureHoop(testHoop,"net-side-touch");
   ball.ResetHome();yield return new WaitForSeconds(2);yield return new WaitForEndOfFrame();
   Check(testHoop.Displacement<.0001f&&testHoop.AnchorDisplacement<.00001f,"net settles and anchors stay pinned");
   yield return Pickup(testHoop,5);count=ball.ShotCount;Check(ball.TryShoot(actor,actor.transform.eulerAngles.y),"timeout attempt starts");
   end=Time.time+2;while(ball.ShotCount==count&&Time.time<end)yield return null;
   ball.autoPickup=false;ball.Body.useGravity=false;ball.Body.position=new Vector3(0,6,0);ball.Body.linearVelocity=Vector3.zero;
   end=Time.time+9;while(ball.Score.result==BasketballResult.Flying&&Time.time<end)yield return null;
   Check(ball.Score.result==BasketballResult.Missed,"unresolved flight times out as a miss");ball.Body.useGravity=true;ball.ResetHome();
   var preserved=ball.Score;ball.ResetHome();yield return new WaitForFixedUpdate();Check(ball.Score.points==preserved.points&&ball.Score.missed==preserved.missed,"recovery preserves totals and does not duplicate a miss");
   Check(ball.Score.attempts==ball.Score.made+ball.Score.missed,"every completed attempt has exactly one outcome");
   app.SendMessage("Return");yield return new WaitForSeconds(.2f);Check(ball.Score.attempts==0&&ball.Score.points==0,"leaving practice resets session score");
   Finish();
  }
  IEnumerator Trial(BasketballHoop hoop,float distance,float power,bool shouldScore){
   yield return Pickup(hoop,distance);var before=ball.Score;uint shots=ball.ShotCount;
   if(shouldScore){
    Check(app.view.BeginShot(),"begin timed shot");yield return WaitGreen();
    app.view.EndShot();
   }else{Check(ball.BeginShotCharge(actor),"begin deliberately mistimed shot");yield return new WaitForSeconds(ball.Charge.period*power);Check(ball.ReleaseShotCharge(actor,actor.transform.eulerAngles.y),"release deliberately mistimed shot");}
   float end=Time.time+2;while(ball.ShotCount==shots&&Time.time<end)yield return null;
   Check(ball.ShotCount==shots+1&&!ball.Held,"one shot releases after gather; measured power="+ball.LastReleasePower.ToString("F3"));ball.autoPickup=false;
   Check(ball.LastLaunchVelocity.magnitude<=ball.maxShotSpeed&&Vector3.Distance(ball.LastLaunchImpulse,ball.LastLaunchVelocity*.62f)<.0001f,"bounded ballistic impulse from mass and velocity");
   float peak=0;bool captured=false;end=Time.time+7;
   while(Time.time<end){
    yield return new WaitForEndOfFrame();peak=Mathf.Max(peak,hoop.Displacement);
    if(shouldScore&&ball.Score.result==BasketballResult.Scored&&!captured&&hoop.Displacement>.035f){CaptureHoop(hoop,"swish-"+hoop.name+"-"+distance);captured=true;}
    if(ball.Score.result!=BasketballResult.Flying&&Time.time-(float)ball.Score.changed>.65f)break;
   }
   Check(ball.Score.attempts==before.attempts+1,"one registered attempt");
   Check(shouldScore?ball.Score.made==before.made+1&&ball.Score.missed==before.missed:ball.Score.missed==before.missed+1&&ball.Score.made==before.made,"timing outcome "+hoop.name+" "+distance+"m power="+power+" result="+ball.Score.result);
   if(shouldScore){Check(ball.Score.points==before.points+(distance>7.3f?3:2),"correct 2/3 point award");Check(peak>.035f&&peak<.3f&&hoop.AnchorDisplacement<.00001f,"scored net stretches and stays attached; peak="+peak.ToString("F3"));}
   yield return new WaitForSeconds(.8f);Check(ball.Score.attempts==ball.Score.made+ball.Score.missed,"rebound cannot add another outcome");
  }
  void CaptureHoop(BasketballHoop hoop,string name){
   var focus=hoop.transform.TransformPoint(new Vector3(0,2.95f,0));reviewCamera.transform.position=hoop.transform.TransformPoint(new Vector3(1.5f,3.35f,2.0f));reviewCamera.transform.LookAt(focus);reviewCamera.fieldOfView=32;Capture(reviewCamera,name,false);
  }
  void CapturePlayer(string name,bool ui){Capture(Camera.main,name,ui);}
  void Capture(Camera camera,string name,bool ui){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
   var canvas=ui?FindFirstObjectByType<BasketballHUD>().GetComponentInParent<Canvas>():null;
   if(canvas){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();}
   var rt=RenderTexture.GetTemporary(1280,720,24);var old=RenderTexture.active;var previous=camera.targetTexture;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
   var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);camera.targetTexture=previous;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);
   if(canvas){canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;}
  }
  void Finish(){if(reviewCamera)Destroy(reviewCamera.gameObject);DevelopmentProbe.TurnCommandActive=false;File.AppendAllText(Path.Combine(folder,"results.txt"),"BASKETBALL_PHYSICS_COMPLETE success="+!failed+"\n");}
 }
}
#endif
