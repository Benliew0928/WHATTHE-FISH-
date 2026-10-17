using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using WhatTheFish;
using Object=UnityEngine.Object;

public static class GolfCircuitReview {
 static string Folder=>Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/GolfCircuitQA"));
 static readonly List<string> report=new();
 static void Check(bool pass,string label){report.Add((pass?"PASS ":"FAIL ")+label);File.WriteAllLines(Path.Combine(Folder,"circuit-editor.txt"),report);if(!pass)throw new Exception(label);}
 static Vector3 Ground(GameObject island,MeshCollider[] terrain,Vector3 local){var ray=new Ray(island.transform.TransformPoint(local+Vector3.up*100),Vector3.down);foreach(var c in terrain)if(c.Raycast(ray,out var hit,200))return hit.point;throw new Exception("No ground "+local);}
 static void Audit(GameObject island,string scope){
  bool active=island.activeSelf;island.SetActive(true);Physics.SyncTransforms();var course=island.GetComponentInChildren<GolfCourse>(true);
  try{
   var terrain=island.GetComponentsInChildren<MeshCollider>(true).Where(c=>c.name.StartsWith("Terrain__")).ToArray();var obstacle=course.transform.Find("Circuit obstacles");
   Check(course.holes.Length==5&&course.fairways.Length==6&&course.fairways.Count(f=>f.alternative)==1&&course.greens.Length==10,scope+" five holes, five routes, gate bypass, five integrated greens and five level tees");
   Check(obstacle&&obstacle.childCount==225&&obstacle.GetComponentsInChildren<Collider>(true).All(c=>c.gameObject.layer==8),scope+" 225 shared-kit obstacles with physical ball collision");
   Check(course.sandPatches.Length==20&&course.roughPatches.Length==20&&course.waterPatches.Length==6&&course.sandPatches[0].Contains(-61,-7),scope+" shallow sand footprint serialized");
   var settings=AssetDatabase.LoadAssetAtPath<GolfBallPhysicsSettings>("Assets/_Game/Resources/GolfBallPhysics.asset");var sand=Ground(island,terrain,new Vector3(-61,0,-7));
   Check(Mathf.Abs(course.RollingResistance(sand,settings)-settings.sandResistance)<.001f,scope+" visible sand also applies sand rolling resistance");
   for(int i=0;i<5;i++){
    var h=course.holes[i];var route=course.fairways[i];float distance=0;for(int j=1;j<route.points.Length;j++)distance+=Vector3.Distance(route.points[j-1],route.points[j]);
    report.Add($"MEASURE {scope} hole={h.number} tee={island.transform.InverseTransformPoint(h.tee.position)} cup={island.transform.InverseTransformPoint(h.cup.position)} route={distance:F2}m par={h.par}");
    Check(distance>60&&distance<210&&Vector3.Distance(h.tee.position,h.cup.position)>60,scope+" hole "+h.number+" bounded challenging route");
    Check(Vector3.Angle(Vector3.ProjectOnPlane(island.transform.TransformDirection(route.points[1]-route.points[0]),Vector3.up),h.tee.forward)<1,scope+" tee "+h.number+" faces safe first landing area");
    float maximumSlope=0;foreach(float x in new[]{-1.2f,0,1.2f})foreach(float z in new[]{-1.4f,0,1.4f}){
     var p=h.tee.position+h.tee.right*x+h.tee.forward*z;if(Physics.Raycast(p+Vector3.up*2,Vector3.down,out var hit,4,1<<8))maximumSlope=Mathf.Max(maximumSlope,Vector3.Angle(hit.normal,Vector3.up));
    }
    Check(maximumSlope<2,scope+" tee "+h.number+" level stance and ball support");
    if(i<4)Check(Vector3.Distance(h.cup.position,course.holes[i+1].tee.position)<32,scope+" short transition "+h.number+" to "+(h.number+1));
    // Independent route samples check physical clearance. These are not a
    // claim of a perfect shot: a full route includes shorter controlled shots.
    bool clear=true;
    for(int j=1;j<route.points.Length;j++){
     int count=Mathf.CeilToInt(Vector3.Distance(route.points[j-1],route.points[j])/2);
     for(int k=1;k<count;k++){var local=Vector3.Lerp(route.points[j-1],route.points[j],k/(float)count);var ground=Ground(island,terrain,local);var sphere=new Vector3(ground.x,ground.y+.5f,ground.z);foreach(var collision in Physics.OverlapSphere(sphere,.35f,1<<8,QueryTriggerInteraction.Ignore).Where(c=>c.transform.IsChildOf(obstacle))){clear=false;report.Add($"BLOCKED hole={h.number} point={local} obstacle={collision.transform.parent?.parent?.name} bounds={collision.bounds}");}}
    }
    Check(clear,scope+" hole "+h.number+" safe route avoids solid obstacle footprints");
   }
   var forward=new Vector3(2,0,-3).normalized;var gate=Ground(island,terrain,new Vector3(82,0,78));
   bool gap=Physics.SphereCast(gate-forward*10+Vector3.up*.7f,GolfBall.Radius,forward,out var hitGate,20,1<<8,QueryTriggerInteraction.Ignore);
   Check(!gap,scope+" stone gate has unobstructed ball-height opening");
   var bypass=course.fairways.Single(f=>f.alternative);bool bypassClear=true;
   for(int i=1;i<bypass.points.Length;i++){int samples=Mathf.CeilToInt(Vector3.Distance(bypass.points[i-1],bypass.points[i])/2);for(int k=1;k<samples;k++){var p=Ground(island,terrain,Vector3.Lerp(bypass.points[i-1],bypass.points[i],k/(float)samples))+Vector3.up*.5f;bypassClear&=!Physics.OverlapSphere(p,.35f,1<<8,QueryTriggerInteraction.Ignore).Any(c=>c.transform.IsChildOf(obstacle));}}
   Check(bypassClear,scope+" southern gate bypass physically clear");
   var post=obstacle.Find("Gate left pillar");var collider=post.GetComponentInChildren<MeshCollider>();
   Check(collider&&collider.bounds.size.y>8,scope+" gate pillars block lofted shortcuts");
   Check(Vector3.Distance(course.holes[4].cup.position,course.holes[0].tee.position)<40,scope+" final green returns near start");
  }finally{island.SetActive(active);}
 }
 static void Capture(Camera camera,string name,Vector3 position,Vector3 target,bool top=false){
  camera.orthographic=top;camera.orthographicSize=220;camera.fieldOfView=48;camera.transform.SetPositionAndRotation(position,Quaternion.LookRotation(target-position,top?Vector3.forward:Vector3.up));
  var rt=new RenderTexture(1600,1100,24);var tex=new Texture2D(1600,1100,TextureFormat.RGB24,false);
  try{camera.targetTexture=rt;for(int i=0;i<3;i++)camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,1100),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(Folder,name+".png"),tex.EncodeToPNG());}
  finally{camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);}
 }
 public static void PrepareAndReview(){
  Directory.CreateDirectory(Folder);report.Clear();GolfCourseReview.PrepareAndCapture();
  var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_Golf.unity");var island=scene.GetRootGameObjects().Single(r=>r.GetComponent<SkySailIslandScene>());Audit(island,"scene");
  const string path="Assets/_Game/Prefabs/Environments/GolfEnvironment.prefab";var prefab=PrefabUtility.LoadPrefabContents(path);try{Audit(prefab,"prefab");}finally{PrefabUtility.UnloadPrefabContents(prefab);}
  CaptureSavedViews();
 }
 public static void PrepareAndBuild(){PrepareAndReview();GolfSwingReview.BuildVerification();}
 public static void RefreshProbeScripts(){
  string player=Environment.GetEnvironmentVariable("WTF_GOLF_SWING_PLAYER")??"../Builds/GolfCircuitQA/DeliveryPlayer/WhatTheFish.exe";
  var build=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=SkySailBuilder.BuildScenes(),target=BuildTarget.StandaloneWindows64,locationPathName=player,options=BuildOptions.Development|BuildOptions.BuildScriptsOnly|BuildOptions.CompressWithLz4HC});
  if(build.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Circuit probe refresh failed");
 }
 public static void CaptureSavedViews(){
  Directory.CreateDirectory(Folder);var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_Golf.unity");var island=scene.GetRootGameObjects().Single(r=>r.GetComponent<SkySailIslandScene>());
  island.SetActive(true);Physics.SyncTransforms();var course=island.GetComponentInChildren<GolfCourse>();File.WriteAllLines(Path.Combine(Folder,"obstacle-bounds.txt"),course.transform.Find("Circuit obstacles").GetComponentsInChildren<Collider>().Select(c=>c.transform.parent?.parent?.name+" "+c.bounds));var pipeline=GraphicsSettings.defaultRenderPipeline;var quality=QualitySettings.renderPipeline;
  var camera=new GameObject("Circuit review camera").AddComponent<Camera>();camera.gameObject.AddComponent<UniversalAdditionalCameraData>();camera.nearClipPlane=.03f;camera.allowHDR=false;
  var sun=new GameObject("Circuit review light").AddComponent<Light>();sun.type=LightType.Directional;
  try{
   GraphicsSettings.defaultRenderPipeline=AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(MobilePackageCleanup.DesktopPipelinePath);QualitySettings.renderPipeline=GraphicsSettings.defaultRenderPipeline;island.GetComponent<RefinedIslandEnvironment>().Apply(camera,sun);ShaderUtil.allowAsyncCompilation=false;
   Capture(camera,"top-down",island.transform.TransformPoint(new Vector3(0,400,0)),island.transform.position,true);
   Capture(camera,"overview",island.transform.TransformPoint(new Vector3(270,285,-340)),island.transform.TransformPoint(new Vector3(0,4,5)));
   foreach(var h in island.GetComponentInChildren<GolfCourse>().holes){Capture(camera,"tee-"+h.number,h.tee.position-h.tee.forward*9+Vector3.up*5,h.tee.position+h.tee.forward*22);Capture(camera,"green-"+h.number,h.cup.position+new Vector3(-9,6,-12),h.cup.position);}
   foreach(var pond in course.waterPatches){var centre=course.transform.TransformPoint(pond.centre);Capture(camera,pond.name.Replace(" ","-"),centre+new Vector3(-9,5,-11),centre);}
   var at=island.transform.TransformPoint(new Vector3(82,4,78));Capture(camera,"stone-gate",at+new Vector3(-19,8,24),at+Vector3.up*4);
  }finally{GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=quality;Object.DestroyImmediate(camera.gameObject);Object.DestroyImmediate(sun.gameObject);}
  Debug.Log("GOLF_CIRCUIT_REVIEW_OK");
 }
}
