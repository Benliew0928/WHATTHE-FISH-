#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 // Fixed-clock rendered review plus joint/foot telemetry. This fixture never
 // changes the release build or substitutes an animation for a real strip.
 public sealed class BasketballChallengeReview:MonoBehaviour {
  string folder,stage;int rate=30,frame;bool failed,capture,havePrevious;AppRoot app;Athlete actor;BasketballBall ball;Camera cameraReview;StreamWriter metrics;
  Transform[] joints;Vector3[] previous;Transform support;Vector3 planted;float jointStep,reach,slide;bool havePlant;float minimumY;Vector3 minPalm,maxPalm;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-basketballChallengeReview");if(i<0||i+1>=args.Length)return;
   var review=new GameObject("Basketball challenge animation review").AddComponent<BasketballChallengeReview>();review.folder=args[i+1];
   i=Array.IndexOf(args,"-motionRate");if(i>=0&&i+1<args.Length&&int.TryParse(args[i+1],out int value))review.rate=Mathf.Clamp(value,20,120);
   review.capture=!args.Contains("-motionNoCapture");
  }
  void Check(bool ok,string message){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+message+"\n");}
  void Place(Athlete who,Vector3 point,float yaw=0){who.capsule.enabled=false;who.transform.SetPositionAndRotation(point,Quaternion.Euler(0,yaw,0));who.capsule.enabled=true;who.ResetLocomotion();Physics.SyncTransforms();}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);Directory.CreateDirectory(Path.Combine(folder,"frames"));File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   float end=Time.realtimeSinceStartup+70;while((!(app=AppRoot.Instance)||!app.Exploring||app.SelectedSport!=SportId.Basketball)&&Time.realtimeSinceStartup<end)yield return null;
   actor=app?app.LocalAthlete:null;ball=BasketballBall.Active;if(!actor||!ball){Check(false,"court ready");Finish();yield break;}
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;Time.captureFramerate=rate;app.view.mode=1;actor.HideHead(false);
   cameraReview=new GameObject("Challenge review camera").AddComponent<Camera>();cameraReview.CopyFrom(Camera.main);cameraReview.enabled=false;
   capture&=SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null;
   joints=actor.visual.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("mixamorig:")&&!t.name.Contains("_end")).ToArray();previous=new Vector3[joints.Length];
   metrics=new StreamWriter(Path.Combine(folder,"motion.csv"));metrics.WriteLine("frame,stage,action,elapsed,held,steals,palmX,palmY,palmZ,jointStep,reachError,supportSlide,minimumFootY");
   Check(actor.BasketballMotion.RigReady,"character rig ready");
   foreach(bool left in new[]{false,true})foreach(bool low in new[]{false,true}){
    ball.autoPickup=false;Place(actor,new Vector3(0,.07f,0));ball.ResetHome();yield return new WaitForSeconds(.6f);
    var point=actor.transform.position+new Vector3(left?-.24f:.24f,low?.23f:.70f,.55f);
    actor.BasketballMotion.BeginChallenge(BasketballAction.Steal,0,point);
    yield return Segment((left?"left":"right")+(low?"-low":"-high"),.95f,true);
    Check(!actor.BasketballMotion.Busy,"swipe returns fully to locomotion");
    Check(maxPalm.z-minPalm.z>.12f&&maxPalm.y-minPalm.y>.08f,"swipe has visible reach and height change");
   }
   foreach(bool left in new[]{false,true}){
    Place(actor,new Vector3(0,.07f,0));yield return new WaitForSeconds(.3f);
    actor.BasketballMotion.BeginChallenge(BasketballAction.Stripped,0,actor.transform.position+new Vector3(left?-.34f:.34f,.35f,.32f));
    yield return Segment(left?"left-reaction":"right-reaction",.85f,true);Check(!actor.BasketballMotion.Busy,"reaction recovers");
   }
   Place(actor,new Vector3(0,.07f,0));DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up};yield return new WaitForSeconds(.4f);
   actor.BasketballMotion.BeginChallenge(BasketballAction.Steal,0,actor.transform.position+new Vector3(.24f,.35f,.65f));yield return Segment("moving-swipe",1,false);DevelopmentProbe.TurnCommand=default;
   Place(actor,new Vector3(0,.07f,0));yield return new WaitForSeconds(.5f);actor.visual.GetComponent<LODGroup>().ForceLOD(1);
   uint before=actor.BasketballMotion.State.sequence;yield return Segment("lod1-repeat",2.4f,true);
   Check(actor.BasketballMotion.State.sequence>=before+3&&actor.BasketballMotion.State.sequence<=before+4,"repeat cadence preserves complete swipes");
   yield return Segment("lod1-release",.85f,true);Check(!actor.BasketballMotion.Busy,"release settles after final recovery");actor.visual.GetComponent<LODGroup>().ForceLOD(-1);
   Finish();
  }
  void ResetSamples(string label){stage=label;jointStep=reach=slide=0;minimumY=100;havePrevious=havePlant=false;minPalm=Vector3.one*100;maxPalm=Vector3.one*-100;
   support=actor.visual.GetComponentsInChildren<Transform>().First(t=>t.name=="mixamorig:"+(actor.BasketballMotion.State.leftHand?"Right":"Left")+"Foot");}
  IEnumerator Segment(string label,float duration,bool stationary){
   ResetSamples(label);float end=Time.time+duration,next=Time.time;
   while(Time.time<end){
    // captureFramerate changes simulation time, not the UI's unscaled clock.
    // Drive pose repetition on this fixed clock; Test-BasketballSteal covers
    // real held/released button input at normal wall-clock speed.
    if(label=="lod1-repeat"&&Time.time>=next){actor.BasketballMotion.BeginChallenge(BasketballAction.Steal,0,actor.transform.position+new Vector3(.24f,.35f,.65f));next=Time.time+BasketballStealRules.Repeat;}
    yield return new WaitForEndOfFrame();Sample(stationary);
   }
   Check(float.IsFinite(jointStep)&&jointStep<.34f,label+" bounded joint step "+jointStep.ToString("F3"));
   Check(reach<.025f,label+" no arm stretch "+reach.ToString("F4"));
   if(stationary){Check(slide<.022f,label+" planted support foot "+slide.ToString("F4"));Check(minimumY>.04f,label+" ankle remains above floor "+minimumY.ToString("F4"));}
  }
  void Sample(bool stationary){
   var motion=actor.BasketballMotion;float step=0;
   for(int i=0;i<joints.Length;i++){var p=actor.transform.InverseTransformPoint(joints[i].position);if(havePrevious)step=Mathf.Max(step,Vector3.Distance(previous[i],p));previous[i]=p;}havePrevious=true;jointStep=Mathf.Max(jointStep,step);reach=Mathf.Max(reach,motion.MaximumReachError);
   var palm=actor.transform.InverseTransformPoint(motion.State.leftHand?motion.LeftPalm:motion.RightPalm);minPalm=Vector3.Min(minPalm,palm);maxPalm=Vector3.Max(maxPalm,palm);
   if(stationary&&motion.Challenging){if(!havePlant){planted=support.position;havePlant=true;}slide=Mathf.Max(slide,Vector3.Distance(planted,support.position));}
   foreach(var joint in joints)if(joint.name.EndsWith("Foot"))minimumY=Mathf.Min(minimumY,joint.position.y);
   metrics.WriteLine($"{frame},{stage},{motion.Action},{motion.Elapsed:F4},{ball.Held},{ball.StealCount},{palm.x:F4},{palm.y:F4},{palm.z:F4},{step:F4},{motion.MaximumReachError:F4},{slide:F4},{minimumY:F4}");
   if(capture){
    var focus=actor.transform.position+Vector3.up*.83f;
    cameraReview.transform.position=focus+(stage.Contains("lod1")?new Vector3(3.5f,.8f,.1f):new Vector3(2.1f,.7f,3.4f));cameraReview.transform.LookAt(focus);cameraReview.fieldOfView=31;
    var rt=RenderTexture.GetTemporary(960,720,24);var old=RenderTexture.active;cameraReview.targetTexture=rt;cameraReview.Render();RenderTexture.active=rt;
    var image=new Texture2D(960,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,960,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,"frames",frame.ToString("D4")+".png"),image.EncodeToPNG());Destroy(image);cameraReview.targetTexture=null;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);
   }
   frame++;
  }
  void Finish(){metrics?.Dispose();app?.view.ClearMatchInput();Time.captureFramerate=0;DevelopmentProbe.TurnCommand=default;DevelopmentProbe.TurnCommandActive=false;if(cameraReview)Destroy(cameraReview.gameObject);File.AppendAllText(Path.Combine(folder,"results.txt"),"BASKETBALL_CHALLENGE_COMPLETE success="+!failed+"\n");}
 }
}
#endif
