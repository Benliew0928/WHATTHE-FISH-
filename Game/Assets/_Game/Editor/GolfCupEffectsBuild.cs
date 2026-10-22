using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WhatTheFish;
public static class GolfCupEffectsBuild {
 [Serializable]sealed class CupLayout {public float cupRadius,cupDepth;}
 static string Argument(string key,string fallback){var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,key);return at>=0&&at+1<args.Length?args[at+1]:fallback;}
 public static void Prepare(){
  const string path="Assets/_Game/Resources/GolfPresentation.asset";
  if(!AssetDatabase.LoadAssetAtPath<GolfPresentationSettings>(path))AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<GolfPresentationSettings>(),path);
  GolfCourseBuilder.ResizeCurrentCups();
  var config=Resources.Load<GolfBallPhysicsSettings>("GolfBallPhysics");var layout=JsonUtility.FromJson<CupLayout>(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Game/Sports/Golf/FiveHoleCourse.json").text);float radius=layout.cupRadius;
  void Check(bool ok,string name){if(!ok)throw new Exception("Golf cup check: "+name);Debug.Log("GOLF_CUP_CHECK PASS "+name);}
  Check(GolfCupRules.CanSink(new Vector3(0,.02f,0),Vector3.forward*.7f,radius,.18f,config.cupCaptureSpeed),"slow centred ball sinks after entry");
  Check(!GolfCupRules.CanSink(new Vector3(0,.02f,0),Vector3.forward*9,radius,.18f,config.cupCaptureSpeed),"very powerful shallow contact remains free");
  Check(GolfCupRules.CanSink(new Vector3(radius-.02f,GolfBall.Radius,0),Vector3.forward*4,radius,.18f,config.cupCaptureSpeed),"ordinary crossing inside rim is assisted at turf height");
  Check(!GolfCupRules.CanSink(new Vector3(0,.3f,0),Vector3.down*8,radius,.18f,config.cupCaptureSpeed),"airborne flyover never scores");
  Check(GolfCupRules.CanSink(new Vector3(0,-.09f,0),Vector3.down*8,radius,.18f,config.cupCaptureSpeed),"vertical drop inside lip can sink");
  Check(!GolfCupRules.CanSink(new Vector3(0,.01f,0),Vector3.up*2,radius,.18f,config.cupCaptureSpeed),"upward rebound stays in physics");
  bool Sink(Vector3 p,Vector3 v)=>GolfCupRules.CanSink(p,v,radius,layout.cupDepth,config.cupCaptureSpeed,config.cupRimForgiveness,config.cupEntryHeightRatio);
  Check(Sink(new Vector3(0,GolfBall.Radius*.85f,0),Vector3.right*1.5f),"gentle entry begins sooner without waiting for deep drop");
  Check(Sink(new Vector3(radius-GolfBall.Radius+.003f,.02f,0),Vector3.forward),"small inside-rim allowance accepts a near-centred ball");
  Check(!Sink(new Vector3(radius+.005f,.02f,0),Vector3.zero),"ball outside the visible opening is never attracted");
  Check(Sink(new Vector3(0,GolfBall.Radius+.004f,0),Vector3.forward*5.5f),"normal crossing does not need to fall below the rim first");
  foreach(var kind in GolfEventSchedule.RandomKinds){
   var events=new GolfEventSchedule();events.ReviewEvent(kind,100,Vector3.right*GolfEventSchedule.WindAcceleration);
   Check(events.Active(140)==kind&&events.Conditions(140,GolfShotMode.Swing).kind==kind,"weather still affects new shots after forty seconds "+kind);
   Check(events.Active(144.99)==kind&&events.Active(145)==GolfEventKind.None,"weather expires at forty-five seconds "+kind);
  }
  var course=UnityEngine.Object.FindFirstObjectByType<GolfCourse>(FindObjectsInactive.Include);Physics.SyncTransforms();
  Check(course&&course.holes.Length==5,"five authored holes retained");
  foreach(var hole in course.holes){
   Check(Mathf.Abs(hole.cupRadius-radius)<.00001f,"matching cup metadata "+hole.number);
   Check(!Physics.Raycast(hole.cup.position+Vector3.up,Vector3.down,1.03f,1<<8,QueryTriggerInteraction.Ignore),"physical opening remains open "+hole.number);
   var lining=hole.cup.Find("Cup lining").GetComponent<MeshFilter>().sharedMesh;
   Check(Mathf.Abs(lining.bounds.extents.x-radius)<.001f,"lining matches enlarged opening "+hole.number);
  }
  int fishingChecks=0;FishingAbilityChecks.Run((ok,name)=>{if(!ok)throw new Exception("Fishing ability check: "+name);fishingChecks++;});
  Debug.Log("FISHING_ABILITY_CHECKS_OK checks="+fishingChecks);
  Debug.Log("GOLF_CUP_EFFECTS_PREPARE_OK");
 }
 public static void Windows()=>ProjectBuilder.BuildWindowsPlayer(Argument("-golfCupOutput","../Builds/GolfCupEffects/Player/WhatTheFish.exe"));
 public static void PrepareWindows(){Prepare();Windows();}
 // Only for code changes after this task's full content build, with unchanged
 // serialized assets/type layouts. Reuses that build's exact scene payload.
 public static void Refresh(){
  string output=ProjectBuilder.ResolveBuildOutputPath(Argument("-golfCupOutput","../Builds/GolfCupEffects/Player/WhatTheFish.exe"));
  var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,output,BuildOptions.Development|BuildOptions.CompressWithLz4HC);
  if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Golf code refresh failed");
 }
}
