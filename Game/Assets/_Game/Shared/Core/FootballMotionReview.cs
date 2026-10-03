#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public sealed class FootballMotionReview:MonoBehaviour {
  string folder,stage;bool failed,capture=true;int rate=30,frame;Athlete actor;FootballMotion motion;FootballBall ball;Camera camera;StreamWriter metrics;float reach,length,contact,penetration;Vector3 home;Mesh skinProbe;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-footballMotionReview");if(i<0||i+1>=args.Length)return;var r=new GameObject("Football motion review").AddComponent<FootballMotionReview>();r.folder=args[i+1];r.capture=!args.Contains("-motionNoCapture");int f=Array.IndexOf(args,"-motionRate");if(f>=0&&f+1<args.Length)int.TryParse(args[f+1],out r.rate);}
  void Check(bool ok,string text){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+text+"\n");}
  void Place(){actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(home,Quaternion.identity);actor.capsule.enabled=true;actor.ResetLocomotion();Physics.SyncTransforms();}
  IEnumerator Pickup(){ball.ResetBall();Place();yield return new WaitForSeconds(.2f);ball.Body.position=ball.FootPosition(actor);ball.Body.linearVelocity=Vector3.zero;ball.RefreshControl(actor);yield return new WaitForSeconds(.3f);Check(actor.ControlsFootball,"slow-ball acquisition");}
  IEnumerator Segment(string label,float seconds,PlayerCommand command){stage=label;reach=length=contact=0;DevelopmentProbe.TurnCommand=command;var until=Time.time+seconds;while(Time.time<until){yield return new WaitForEndOfFrame();Sample();}Check(length<.0001f&&reach<.065f,label+" preserves lengths and bounded reach="+reach.ToString("F3"));if(label.Contains("dribble"))Check(contact<.035f,label+" shoe/ball contact error="+contact.ToString("F3"));Check(motion.Weight>.9f||label.Contains("jump"),label+" pose active");}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);Directory.CreateDirectory(Path.Combine(folder,"frames"));File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   float deadline=Time.realtimeSinceStartup+70;while((!AppRoot.Instance||!AppRoot.Instance.LocalAthlete||!FootballBall.Instance)&&Time.realtimeSinceStartup<deadline)yield return null;
   if(!AppRoot.Instance||!FootballBall.Instance){Check(false,"football loaded");Finish();yield break;}
   AppRoot.Instance.EnterOffline();DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.7f);
   actor=AppRoot.Instance.LocalAthlete;motion=actor.FootballMotion;ball=FootballBall.Instance;home=ball.KickoffPosition+new Vector3(0,-ball.WorldRadius,-.70f);Place();
   Check(motion&&motion.RigReady,"real character skeleton and library bound");if(!motion||!motion.RigReady){Finish();yield break;}
   Check(motion.library.takes.Length==26,"all 24 core and two result takes present");Check(default(FootballMotionState).Equals(default),"idle network equality");
   for(int i=0;i<4;i++){var pushes=new[]{Vector3.back,Vector3.forward,Vector3.left,Vector3.right};Check(FootballMotion.FallVariant(pushes[i],Quaternion.identity)==i,"fall selection "+i);Check(FootballMotion.FallVariant(Quaternion.Euler(0,90,0)*pushes[i],Quaternion.Euler(0,90,0))==i,"rotated fall selection "+i);}
   metrics=new StreamWriter(Path.Combine(folder,"motion.csv"));metrics.WriteLine("frame,stage,performance,speed,reach,length,contact,phase");
   File.WriteAllLines(Path.Combine(folder,"rig.txt"),actor.visual.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("mixamorig:")).Select(t=>t.name+" "+actor.transform.InverseTransformPoint(t.position).ToString("F4")));
   camera=new GameObject("Football motion capture").AddComponent<Camera>();camera.CopyFrom(Camera.main);camera.enabled=false;camera.fieldOfView=35;skinProbe=new Mesh();
   AppRoot.Instance.view.mode=1;actor.HideHead(false);Time.captureFramerate=rate;
   yield return Segment("ready",.6f,default);ball.ResetBall();ball.Body.position+=Vector3.right*8;
   yield return Segment("walk",.6f,new(){move=Vector2.up*.2f});yield return Segment("run",.7f,new(){move=Vector2.up});yield return Segment("sprint",.7f,new(){move=Vector2.up,sprint=true});yield return Segment("stop",.5f,default);
   yield return Segment("reverse",.7f,new(){move=Vector2.down,sprint=true});yield return Segment("restart",.3f,new(){move=Vector2.right});
   DevelopmentProbe.TurnCommand=default;yield return Pickup();yield return Segment("receive-ready",.5f,default);yield return Segment("dribble-control",1.2f,new(){move=Vector2.up});yield return Segment("dribble-fast",1.2f,new(){move=Vector2.up,sprint=true});yield return Segment("dribble-cut",.5f,new(){move=Vector2.right,sprint=true});yield return Segment("ball-stop",.5f,default);
   Check(actor.ControlsFootball,"all dribbling retains attached possession");yield return Segment("charge",1.2f,new(){charging=true});Check(motion.State.charging,"charge timeline active");yield return Segment("fake-cancel",.4f,default);Check(motion.State.gesture==FootballGesture.Cancel&&actor.ControlsFootball,"fake cancels without release");
   AppRoot.Instance.view.RequestKick(.2f);yield return Segment("light-kick",.5f,default);Check(!actor.ControlsFootball&&motion.State.gesture==FootballGesture.Kick,"accepted light kick and presentation");yield return Pickup();yield return Segment("charge-full",1.1f,new(){charging=true});AppRoot.Instance.view.RequestKick(1);yield return Segment("full-kick",.5f,default);Check(!actor.ControlsFootball,"full kick releases");
   ball.ResetBall();ball.Body.position+=Vector3.right*8;Place();AppRoot.Instance.view.RequestJump();yield return Segment("jump-land",1.2f,new(){move=Vector2.up});Check(!actor.Airborne,"jump lands");
   Place();Check(actor.TryTackle(),"slide starts");yield return Segment("slide-and-whiff",1.4f,new(){move=Vector2.up});Check(actor.Action==FootballAction.None&&actor.WhiffRemaining==0,"slide and miss recovery finish");
   foreach(var push in new[]{Vector3.back,Vector3.forward,Vector3.left,Vector3.right}){Place();Check(actor.GetComponent<FootballTackle>().ReceiveHit(push),"accepted directional hit");yield return Segment("fall-"+actor.HitVariant,.9f,default);Check(actor.Action==FootballAction.None,"hit ends at authoritative deadline");}
   Place();DevelopmentProbe.TurnCommand=default;ball.ResetBall();ball.Body.position+=Vector3.right*8;
   var lod=actor.visual.GetComponent<LODGroup>();
   for(int level=0;level<2;level++){
    if(lod)lod.ForceLOD(level);
    foreach(var take in motion.library.takes){
     stage="take-"+take.name+"-lod"+level;motion.ReviewTake=take.name;reach=length=penetration=0;
     for(int sample=0;sample<=30;sample++){motion.ReviewTime=sample/30f;yield return new WaitForEndOfFrame();Sample(sample%5==0,true);}
     Check(length<.0001f&&reach<.065f,stage+" valid complete pose; maximum clamp="+reach.ToString("F3"));
     Check(penetration<.015f,stage+" skinned mesh floor penetration="+penetration.ToString("F3"));
    }
   }
   motion.ReviewTake=null;if(lod)lod.ForceLOD(-1);
   var crowd=new Athlete[9];for(int i=0;i<crowd.Length;i++){crowd[i]=Instantiate(actor.gameObject).GetComponent<Athlete>();crowd[i].controlled=false;crowd[i].capsule.enabled=false;crowd[i].transform.position=home+new Vector3(-3*(1+i%3),0,3*(1+i/3));crowd[i].capsule.enabled=true;crowd[i].Setup();crowd[i].ResetLocomotion();}
   double poseCost=0;long poseAllocations=0;int samples=0;var crowdUntil=Time.time+1.5f;
   while(Time.time<crowdUntil){for(int i=0;i<crowd.Length;i++)crowd[i].Simulate(new PlayerCommand{move=(i%2==0?Vector2.up:Vector2.down)*.4f},Time.deltaTime);yield return new WaitForEndOfFrame();if(samples++>10){poseCost=Math.Max(poseCost,motion.LastPoseMilliseconds+crowd.Sum(a=>a.FootballMotion.LastPoseMilliseconds));poseAllocations=Math.Max(poseAllocations,motion.LastPoseAllocatedBytes+crowd.Sum(a=>a.FootballMotion.LastPoseAllocatedBytes));}}
   Check(crowd.All(a=>a.FootballMotion.RigReady&&a.FootballMotion.BoneLengthError()<.0001f),"ten-athlete pose workload preserves rigs");Check(poseAllocations==0,"ten-athlete steady pose allocation bytes/frame="+poseAllocations);File.AppendAllText(Path.Combine(folder,"results.txt"),"MEASURE ten-athlete Windows pose layer maximum="+poseCost.ToString("F3")+"ms; excludes Animator/rendering and is not phone FPS\n");foreach(var other in crowd)Destroy(other.gameObject);
   motion.Charging(true);actor.ResetLocomotion();Check(!motion.State.charging&&motion.State.gesture==FootballGesture.None,"reset clears gesture and charge");
   var snapshot=new FootballMotionState{sequence=900,gesture=FootballGesture.Kick,started=FootballMotion.Clock-1};motion.Receive(snapshot);motion.Receive(new FootballMotionState{sequence=899,gesture=FootballGesture.Cancel});Check(motion.State.sequence==900,"stale presentation snapshots ignored");actor.ResetLocomotion();
   AppRoot.Instance.SendMessage("Return");yield return null;yield return new WaitForEndOfFrame();Check(!motion.Active&&motion.Weight<.001f,"session return clears football overlay");Finish();
  }
  void Sample(bool force=false,bool geometry=false){
   reach=Mathf.Max(reach,motion.MaximumReachError);length=Mathf.Max(length,motion.BoneLengthError());contact=Mathf.Max(contact,motion.BallContactError);
   metrics.WriteLine($"{frame},{stage},{motion.Performance},{actor.speed:F4},{motion.MaximumReachError:F4},{motion.BoneLengthError():F6},{motion.BallContactError:F4},{motion.GaitPhase:F4}");
   if(geometry&&Physics.Raycast(actor.transform.position+Vector3.up,Vector3.down,out var floor,2,1<<8,QueryTriggerInteraction.Ignore))foreach(var renderer in actor.visual.GetComponentsInChildren<SkinnedMeshRenderer>()){
    renderer.BakeMesh(skinProbe,true);float min=100,unscaled=100;foreach(var vertex in skinProbe.vertices){min=Mathf.Min(min,renderer.transform.TransformPoint(vertex).y);unscaled=Mathf.Min(unscaled,(renderer.transform.position+renderer.transform.rotation*vertex).y);}penetration=Mathf.Max(penetration,floor.point.y-min);
    if(stage.Contains("take-")&&motion.ReviewTime==.5f)File.AppendAllText(Path.Combine(folder,"skin-diagnostic.txt"),stage+" "+renderer.name+" scale="+renderer.transform.lossyScale.ToString("F3")+" baked="+skinProbe.bounds.size.ToString("F3")+" renderer="+renderer.bounds.size.ToString("F3")+" floor="+floor.point.y+" min="+min+" unscaled="+unscaled+"\n");
   }
   if(capture&&(force||frame%2==0)){
    var focus=actor.transform.position+Vector3.up*.80f;camera.transform.position=focus+(stage.Contains("lod1")?new Vector3(3.8f,.5f,.1f):new Vector3(2.6f,.75f,3.3f));camera.transform.LookAt(focus);
    var rt=RenderTexture.GetTemporary(800,640,24);var old=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var img=new Texture2D(800,640,TextureFormat.RGB24,false);img.ReadPixels(new Rect(0,0,800,640),0,0);img.Apply();File.WriteAllBytes(Path.Combine(folder,"frames",frame.ToString("D5")+"-"+stage+".png"),img.EncodeToPNG());Destroy(img);camera.targetTexture=null;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);
   }frame++;
  }
  void Finish(){metrics?.Dispose();Time.captureFramerate=0;DevelopmentProbe.TurnCommandActive=false;if(camera)Destroy(camera.gameObject);if(skinProbe)Destroy(skinProbe);File.AppendAllText(Path.Combine(folder,"results.txt"),"FOOTBALL_MOTION_COMPLETE success="+!failed+"\n");}
 }
}
#endif
