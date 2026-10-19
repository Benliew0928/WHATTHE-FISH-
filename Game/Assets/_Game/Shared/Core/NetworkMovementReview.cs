#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace WhatTheFish {
 // Opt-in frame-cadence audit. Capture time separates simulation/render mismatch
 // from GPU load; real transport latency is tested by the existing room suites.
 public sealed class NetworkMovementReview:MonoBehaviour {
  string folder;bool failed;AppRoot app;Athlete actor;int oldMode;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-movementReview");if(i>=0&&i+1<args.Length)new GameObject("Network movement review").AddComponent<NetworkMovementReview>().folder=args[i+1];}
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");float end=Time.realtimeSinceStartup+75;
   // Exploration becomes true before the shared arrival finishes. Measuring
   // then records the intentional travel input lock as a movement failure.
   while(!DevelopmentProbe.ReadyToReviewSport&&Time.realtimeSinceStartup<end)yield return null;
   app=AppRoot.Instance;
   if(!DevelopmentProbe.ReadyToReviewSport||!app||!(actor=app.LocalAthlete)){Check(false,"entered sport after arrival");Finish();yield break;}
   oldMode=app.view.mode;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.3f);
   var net=actor.GetComponent<NetworkAthlete>();bool host=net&&net.IsSpawned&&net.IsServer;
   Check(!net||!net.IsSpawned||host,"audit owns authoritative player");
   var start=actor.transform.position;float heading=0;
   if(FootballBall.Instance){var b=FootballBall.Instance;var centre=b.PitchBounds.center;start=b.Pitch.TransformPoint(new Vector3(centre.x,b.PitchBounds.max.y+.02f,centre.z-18));heading=b.Pitch.eulerAngles.y;b.ReviewFootballBall(b.Pitch.TransformPoint(new Vector3(centre.x+12,b.PitchBounds.max.y+.2f,centre.z+15)),Vector3.zero);}
   else if(BasketballBall.Active){start=BasketballBall.Active.transform.parent.TransformPoint(new Vector3(-5,.08f,-11));heading=BasketballBall.Active.transform.parent.eulerAngles.y;BasketballBall.Active.SetFreeRoam(actor,true);}
   var summary=new List<string>{"sport,host,fps,camera,frames,stalls,mean_speed,speed_cv,max_speed"};
   foreach(int rate in new[]{30,60,120})foreach(int mode in new[]{1,2}){
    DevelopmentProbe.TurnCommand=default;actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(start,Quaternion.Euler(0,heading,0));if(host)actor.GetComponent<NetworkTransform>().Teleport(start,actor.transform.rotation,Vector3.one);actor.capsule.enabled=true;actor.ResetLocomotion();Physics.SyncTransforms();app.view.mode=mode;app.view.yaw=heading;Time.captureFramerate=rate;
    DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,heading=heading,sprint=true};for(int i=0;i<rate/2;i++)yield return new WaitForEndOfFrame();
    var previous=actor.transform.position;int stalls=0;float sum=0,sum2=0,max=0;var csv=new List<string>{"frame,dt,dx,dy,dz,speed,reported_speed"};
    for(int i=0;i<rate;i++){yield return new WaitForEndOfFrame();var delta=actor.transform.position-previous;previous=actor.transform.position;float speed=Vector3.ProjectOnPlane(delta,Vector3.up).magnitude/Time.deltaTime;sum+=speed;sum2+=speed*speed;max=Mathf.Max(max,speed);if(speed<.01f)stalls++;csv.Add(FormattableString.Invariant($"{i},{Time.deltaTime:R},{delta.x:R},{delta.y:R},{delta.z:R},{speed:R},{actor.speed:R}"));}
    float mean=sum/rate,cv=Mathf.Sqrt(Mathf.Max(0,sum2/rate-mean*mean))/Mathf.Max(.001f,mean);string label=$"{app.SelectedSport}-{(host?"host":"offline")}-{rate}fps-camera{mode}";File.WriteAllLines(Path.Combine(folder,label+".csv"),csv);summary.Add(FormattableString.Invariant($"{app.SelectedSport},{host},{rate},{mode},{rate},{stalls},{mean:R},{cv:R},{max:R}"));
    float expected=app.SelectedSport==SportId.Football?7.35f:7;
    Check(Mathf.Abs(mean-expected)<.02f,"running pace "+label+" mean="+mean.ToString("F3",CultureInfo.InvariantCulture));
    bool baseline=Array.IndexOf(Environment.GetCommandLineArgs(),"-movementBaseline")>=0;
    if(!baseline)Check(stalls==0&&cv<.04f,"even rendered movement "+label+" stalls="+stalls+" cv="+cv.ToString("F4",CultureInfo.InvariantCulture));
   }
   File.WriteAllLines(Path.Combine(folder,"cadence.csv"),summary);DevelopmentProbe.TurnCommand=default;Time.captureFramerate=0;yield return new WaitForSeconds(.3f);
   var stopped=actor.transform.position;yield return new WaitForSeconds(.4f);Check(Vector3.Distance(stopped,actor.transform.position)<.03f,"release stops movement");Finish();
  }
  void Finish(){Time.captureFramerate=0;DevelopmentProbe.TurnCommandActive=false;DevelopmentProbe.TurnCommand=default;if(app)app.view.mode=oldMode;File.AppendAllText(Path.Combine(folder,"results.txt"),"MOVEMENT_REVIEW_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
