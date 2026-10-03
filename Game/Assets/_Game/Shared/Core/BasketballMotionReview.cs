#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 // Actual player frames and joint/ball telemetry; never included in release.
 public sealed class BasketballMotionReview:MonoBehaviour {
  string folder,stage;bool failed,capture;int rate=30;bool noCapture;AppRoot app;Athlete actor;BasketballBall ball;Camera reviewCamera;
  int frame;float contactError,reachError,minBall=100,maxBall;StreamWriter metrics;Transform[] joints;Vector3[] previous;float maximumStep,gatherSpin;bool havePrevious,haveGather;Quaternion previousGather;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-basketballMotionReview");if(index<0||index+1>=args.Length)return;
   var review=new GameObject("Basketball motion review").AddComponent<BasketballMotionReview>();review.folder=args[index+1];
   int fps=Array.IndexOf(args,"-motionRate");if(fps>=0&&fps+1<args.Length&&int.TryParse(args[fps+1],out int value))review.rate=Mathf.Clamp(value,20,120);review.noCapture=args.Contains("-motionNoCapture");
  }
  void Check(bool ok,string message){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+message+"\n");}
  void Place(Vector3 position,float yaw=0){actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));actor.capsule.enabled=true;actor.ResetLocomotion();Physics.SyncTransforms();havePrevious=false;}
  IEnumerator Pickup(){
   ball.Place(actor.transform.position+actor.transform.forward*.6f+Vector3.up*.1f,Quaternion.identity,Vector3.zero,Vector3.zero);
   float end=Time.time+2;while(ball.Holder!=actor&&Time.time<end)yield return null;
   Check(ball.Holder==actor,"proximity pickup");yield return new WaitForSeconds(.25f);
  }
  IEnumerator Segment(string label,float duration,PlayerCommand command){
   stage=label;contactError=reachError=maximumStep=0;minBall=100;maxBall=0;havePrevious=haveGather=false;gatherSpin=0;
   DevelopmentProbe.TurnCommand=command;float end=Time.time+duration;
   while(Time.time<end){yield return new WaitForEndOfFrame();Sample();}
   Check(float.IsFinite(maximumStep)&&maximumStep<.5f,label+" bounded joint displacement "+maximumStep.ToString("F3")+"m/frame");
   Check(reachError<.065f,label+" hand reach error "+reachError.ToString("F3")+"m");
   if(label.Contains("shoot")||label=="pass")Check(gatherSpin<1,label+" ball stays gripped during gather; spin="+gatherSpin.ToString("F3"));
   if(label.Contains("dribble")){Check(contactError<.025f,label+" palm contact "+contactError.ToString("F3")+"m");Check(maxBall-minBall>.55f,label+" ball completes a bounce");}
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);Directory.CreateDirectory(Path.Combine(folder,"frames"));File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   Check(default(BasketballMotionState).Equals(default),"unchanged idle pose equals itself for network replication");
   float deadline=Time.realtimeSinceStartup+70;
   while((!(app=AppRoot.Instance)||!app.Exploring||app.SelectedSport!=SportId.Basketball)&&Time.realtimeSinceStartup<deadline)yield return null;
   actor=app?app.LocalAthlete:null;ball=BasketballBall.Active;if(!actor||!ball){Check(false,"player and basketball ready");Finish();yield break;}
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;Time.captureFramerate=rate;
   app.view.mode=1;app.view.yaw=0;actor.HideHead(false);
   Check(actor.BasketballMotion&&actor.BasketballMotion.RigReady,"both arms, legs, spine and palm landmarks bound");
   joints=actor.visual.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("mixamorig:")&&(!t.name.Contains("Hand")||t.name.EndsWith(":LeftHand")||t.name.EndsWith(":RightHand"))&&!t.name.Contains("Toe")).ToArray();previous=new Vector3[joints.Length];
   File.WriteAllLines(Path.Combine(folder,"rig.txt"),actor.visual.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("mixamorig:")).Select(t=>t.name+" "+actor.transform.InverseTransformPoint(t.position).ToString("F4")));
   metrics=new StreamWriter(Path.Combine(folder,"motion.csv"));metrics.WriteLine("frame,stage,elapsed,speed,held,phase,ballY,contactError,reachError,jointStep");
   reviewCamera=new GameObject("Motion review camera").AddComponent<Camera>();reviewCamera.CopyFrom(Camera.main);reviewCamera.enabled=false;capture=!noCapture&&SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null;
   Place(new Vector3(-3,.07f,-5));ball.autoPickup=false;
   yield return Segment("ready",.7f,default);
   yield return Segment("run",.85f,new PlayerCommand{move=Vector2.up,sprint=true});
   yield return Segment("stop",.8f,default);
   ball.autoPickup=true;yield return Pickup();
   yield return Segment("stationary-dribble",1.3f,default);
   yield return Segment("running-dribble",1f,new PlayerCommand{move=Vector2.up,sprint=true});
   yield return Segment("stop-dribble",.9f,default);
   yield return Segment("turn-dribble",.8f,new PlayerCommand{move=Vector2.down,sprint=true});
   yield return Segment("settle-dribble",.8f,default);
   uint shots=ball.ShotCount;app.view.yaw=0;app.view.RequestShoot();
   yield return Segment("shoot",1.55f,default);
   Check(ball.ShotCount==shots+1&&!ball.Held,"one shot releases on animation timeline");
   Check(!actor.BasketballMotion.Busy,"shoot follow-through recovers");
   Place(new Vector3(-3,.07f,-3));yield return Pickup();uint passes=ball.PassCount;app.view.RequestPass();
   yield return Segment("pass",1.25f,default);
   Check(ball.PassCount==passes+1&&!ball.Held,"one chest pass releases on animation timeline");
   Check(!actor.BasketballMotion.Busy,"pass follow-through recovers");
   Place(new Vector3(0,.07f,-5));yield return Pickup();app.view.RequestJump();
   yield return Segment("jump-with-ball",1.3f,new PlayerCommand{move=Vector2.up});
   Check(!actor.Airborne&&ball.Holder==actor,"jump keeps possession and lands");
   DevelopmentProbe.TurnCommand=default;Place(new Vector3(0,.07f,-4));ball.ResetHome();yield return Pickup();
   var lod=actor.visual.GetComponent<LODGroup>();lod.ForceLOD(1);
   yield return Segment("lod1-side-dribble",.9f,default);app.view.RequestShoot();
   yield return Segment("lod1-side-shoot",1.55f,default);lod.ForceLOD(-1);
   shots=ball.ShotCount-1;
   Place(new Vector3(0,.07f,-4));yield return Pickup();
   // A blocked release keeps possession; travel/session resets cancel windup.
   yield return new WaitForSeconds(.2f);ball.TryPass(actor,0);
   var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=8;wall.transform.position=actor.transform.position+Vector3.forward*.52f+Vector3.up*1.2f;wall.transform.localScale=new Vector3(2,2,.08f);Physics.SyncTransforms();
   yield return new WaitForSeconds(1);Check(ball.Held,"blocked hand release retains possession");Destroy(wall);yield return null;
   ball.TryPass(actor,0);actor.ResetLocomotion();yield return new WaitForSeconds(.8f);Check(!actor.BasketballMotion.Busy&&!ball.ActionQueued&&ball.Held,"teleport cancels active and queued release");
   ball.TryShoot(actor,0);app.SendMessage("Return");yield return new WaitForSeconds(.6f);Check(!ball.Held&&ball.ShotCount==shots+1,"session return cancels pending release");
   Finish();
  }
  void Sample(){
   var motion=actor.BasketballMotion;
   if(ball.Held&&motion.Busy){var rotation=Quaternion.Inverse(actor.transform.rotation)*ball.transform.rotation;if(haveGather)gatherSpin=Mathf.Max(gatherSpin,Quaternion.Angle(previousGather,rotation));previousGather=rotation;haveGather=true;}else haveGather=false;
   contactError=Mathf.Max(contactError,motion.ContactError);reachError=Mathf.Max(reachError,motion.MaximumReachError);
   float y=ball.transform.position.y-actor.transform.position.y;minBall=Mathf.Min(minBall,y);maxBall=Mathf.Max(maxBall,y);
   float step=0;for(int i=0;i<joints.Length;i++){var p=actor.transform.InverseTransformPoint(joints[i].position);if(havePrevious)step=Mathf.Max(step,Vector3.Distance(p,previous[i]));previous[i]=p;}havePrevious=true;maximumStep=Mathf.Max(maximumStep,step);
   metrics.WriteLine($"{frame},{stage},{motion.Elapsed:F4},{actor.speed:F4},{ball.Held},{ball.DribblePhase:F4},{y:F4},{motion.ContactError:F4},{motion.MaximumReachError:F4},{step:F4}");
   if(capture){
    var focus=actor.transform.position+Vector3.up*.98f;reviewCamera.transform.position=focus+(stage.Contains("side")?new Vector3(4.5f,.6f,.4f):new Vector3(2.8f,1.0f,3.8f));reviewCamera.transform.LookAt(focus);reviewCamera.fieldOfView=34;
    var rt=RenderTexture.GetTemporary(960,720,24);var old=RenderTexture.active;reviewCamera.targetTexture=rt;reviewCamera.Render();RenderTexture.active=rt;
    var image=new Texture2D(960,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,960,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,"frames",frame.ToString("D4")+".png"),image.EncodeToPNG());Destroy(image);reviewCamera.targetTexture=null;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);
   }
   frame++;
  }
  void Finish(){metrics?.Dispose();Time.captureFramerate=0;DevelopmentProbe.TurnCommandActive=false;if(reviewCamera)Destroy(reviewCamera.gameObject);File.AppendAllText(Path.Combine(folder,"results.txt"),"BASKETBALL_MOTION_COMPLETE success="+!failed+"\n");}
 }
}
#endif
