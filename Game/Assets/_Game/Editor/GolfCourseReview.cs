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

// Actual scene/prefab collision checks and opt-in Unity renders; never changes gameplay cameras.
public static class GolfCourseReview {
 static readonly List<string> results=new();
 static void Check(bool condition,string text){results.Add((condition?"PASS ":"FAIL ")+text);if(!condition)throw new Exception(text);}
 static void Audit(GameObject island,string label){
  bool active=island.activeSelf;island.SetActive(true);Physics.SyncTransforms();
  try{
   var courses=island.GetComponentsInChildren<GolfCourse>(true);Check(courses.Length==1,label+" one course");var course=courses[0];
   Check(course.holes.Length==5&&course.holes.Select(h=>h.number).OrderBy(n=>n).SequenceEqual(new[]{1,2,3,4,5}),label+" five numbered holes");
   var terrain=island.GetComponentsInChildren<MeshCollider>(true).Where(c=>c.name.StartsWith("Terrain__")).ToArray();Check(terrain.Length==16,label+" sixteen terrain sectors retained");
   var terrainMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/RefinedIslands/Golf/Golf_Terrain.mat");
   var aqua=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/RefinedIslands/Shared/Materials/RI_Teal.mat");
   var originals=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/RefinedIslands/Golf/Terrain.fbx").GetComponentsInChildren<MeshFilter>(true).ToDictionary(f=>f.name,f=>f.sharedMesh);
   var reference=new GameObject("Original terrain collision reference");reference.transform.SetParent(island.transform,false);
   var sourceTerrain=terrain.Select(c=>{var node=new GameObject(c.name);node.transform.SetParent(c.transform,false);var collider=node.AddComponent<MeshCollider>();collider.sharedMesh=originals[c.name];return collider;}).ToArray();
   foreach(var c in sourceTerrain)c.transform.SetParent(reference.transform,true);Physics.SyncTransforms();
   try{
   for(int i=0;i<course.holes.Length;i++)for(int j=i+1;j<course.holes.Length;j++){
    var a=course.holes[i];var b=course.holes[j];float distance=Vector2.Distance(new Vector2(a.cup.position.x,a.cup.position.z),new Vector2(b.cup.position.x,b.cup.position.z));
    Check(distance>80&&(a.number>3||b.number>3||distance>150),$"{label} holes {a.number}/{b.number} separated by {distance:F1} m");
   }
   foreach(var h in course.holes){
    var p=h.cup.position;var ray=new Ray(p+Vector3.up,Vector3.down);
    Check(h.flag&&h.tee&&Vector3.Distance(h.flag.position,p)<.001f,label+" hole "+h.number+" independent flag and tee");
    Check(h.flag.GetComponentsInChildren<MeshRenderer>().Length==4,label+" hole "+h.number+" pole, pennant and two readable number faces");
    Check(h.flag.Find("Aqua pennant").GetComponent<MeshRenderer>().sharedMaterial==aqua,label+" hole "+h.number+" original map aqua material");
    Check(!terrain.Any(c=>c.Raycast(ray,out _,1.2f)),label+" hole "+h.number+" terrain opening");
    var floor=h.cup.Find("Cup floor").GetComponent<MeshCollider>();
    Check(floor.Raycast(ray,out var bottom,2)&&Mathf.Abs(bottom.point.y-(p.y-h.cupDepth))<.003f,label+" hole "+h.number+" recessed floor "+h.cupDepth+" m");
    var wall=h.cup.Find("Cup lining").GetComponent<MeshCollider>();
    Check(wall.Raycast(new Ray(p-Vector3.up*h.cupDepth*.5f,Vector3.right),out _,h.cupRadius*1.1f),label+" hole "+h.number+" collision on interior wall");
    Check(terrain.Any(c=>c.Raycast(new Ray(p+Vector3.right*(h.cupRadius+.065f)+Vector3.up,Vector3.down),out _,2)),label+" hole "+h.number+" solid ground at cup edge");
    Check(Mathf.Abs(h.cupRadius-.1425f)<.00001f&&Mathf.Abs(floor.sharedMesh.bounds.extents.x-h.cupRadius)<.00001f,label+" hole "+h.number+" 1.5x opening diameter, 0.285 m");
    Check(terrain.Any(c=>c.Raycast(new Ray(h.tee.position+Vector3.up,Vector3.down),out _,2)),label+" hole "+h.number+" grounded tee");
    Check(!h.cup.Find("Putting green")&&terrain.All(c=>c.GetComponent<MeshRenderer>().sharedMaterial==terrainMaterial),label+" hole "+h.number+" original grass surface without a colour overlay");
    float positionError=0,uvError=0,maximumSlope=0,heightError=0;int samples=0;
    foreach(float radius in new[]{h.cupRadius+.07f,h.greenRadius*.3f,h.greenRadius})for(int i=0;i<16;i++){
     float a=i*Mathf.PI*2/16;var sample=p+new Vector3(Mathf.Cos(a)*radius,10,Mathf.Sin(a)*radius);var sampleRay=new Ray(sample,Vector3.down);
     var current=terrain.Select(c=>c.Raycast(sampleRay,out var hit,20)?(RaycastHit?)hit:null).FirstOrDefault(hit=>hit.HasValue);
     var original=sourceTerrain.Select(c=>c.Raycast(sampleRay,out var hit,20)?(RaycastHit?)hit:null).FirstOrDefault(hit=>hit.HasValue);
     if(!current.HasValue||!original.HasValue)continue;samples++;
     positionError=Mathf.Max(positionError,Vector3.Distance(current.Value.point,original.Value.point));uvError=Mathf.Max(uvError,Vector2.Distance(current.Value.textureCoord,original.Value.textureCoord));
     var localPoint=island.transform.InverseTransformPoint(current.Value.point);var green=course.greens[h.number-1];
     float plane=green.centre.y+green.slope.x*(localPoint.x-green.centre.x)+green.slope.y*(localPoint.z-green.centre.z);
     if(radius<h.greenRadius*.8f){maximumSlope=Mathf.Max(maximumSlope,Vector3.Angle(current.Value.normal,Vector3.up));heightError=Mathf.Max(heightError,Mathf.Abs(localPoint.y-plane));}
    }
    results.Add($"INFO {label} hole {h.number} green samples={samples}/48 maximumHeightChange={positionError:R} uvError={uvError:R} innerSlope={maximumSlope:R} planeError={heightError:R}");
    Check(samples>=46&&positionError<.7f&&maximumSlope<1&&heightError<.005f,label+" hole "+h.number+" gentle integrated green with restrained height change");
    Check(uvError<.0001f,label+" hole "+h.number+" original terrain texture coordinates retained");
    Check(h.cup.GetComponentsInChildren<MeshRenderer>().All(r=>r.sharedMaterials.All(m=>m&&m.shader)),label+" hole "+h.number+" complete render dependencies");
    var local=island.transform.InverseTransformPoint(p);results.Add($"INFO hole {h.number} cup={local} tee={island.transform.InverseTransformPoint(h.tee.position)} greenRadius={h.greenRadius} par={h.par}");
    float joinSlope=0,originalJoinSlope=0,outerError=0;int outsideSamples=0;
    for(int ring=0;ring<4;ring++)for(int j=0;j<32;j++){
     float angle=j*Mathf.PI*2/32,r=h.greenRadius+2+ring*2.5f;
     var sampleRay=new Ray(p+new Vector3(Mathf.Cos(angle)*r,20,Mathf.Sin(angle)*r),Vector3.down);
     var current=terrain.Select(c=>c.Raycast(sampleRay,out var hit,40)?(RaycastHit?)hit:null).FirstOrDefault(hit=>hit.HasValue);
     var original=sourceTerrain.Select(c=>c.Raycast(sampleRay,out var hit,40)?(RaycastHit?)hit:null).FirstOrDefault(hit=>hit.HasValue);
     if(!current.HasValue||!original.HasValue)continue;
     joinSlope=Mathf.Max(joinSlope,Vector3.Angle(current.Value.normal,Vector3.up));
     originalJoinSlope=Mathf.Max(originalJoinSlope,Vector3.Angle(original.Value.normal,Vector3.up));
     if(ring==3){outerError=Mathf.Max(outerError,Vector3.Distance(current.Value.point,original.Value.point));outsideSamples++;}
    }
    Check(joinSlope<Mathf.Max(9,originalJoinSlope+.3f)&&outsideSamples==32&&outerError<.001f,$"{label} hole {h.number} seamless hillside blend slope={joinSlope:F3} originalSlope={originalJoinSlope:F3} outerError={outerError:R}");
   }
   }finally{Object.DestroyImmediate(reference);}
  }finally{island.SetActive(active);}
 }
 static void Capture(Camera camera,string name,Vector3 position,Vector3 target,float fov){
  camera.transform.position=position;camera.transform.LookAt(target);camera.fieldOfView=fov;
  var rt=new RenderTexture(1600,1000,24);var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);var previous=RenderTexture.active;
  try{camera.targetTexture=rt;for(int i=0;i<3;i++)camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(GolfCourseBuilder.Evidence,name+".png"),tex.EncodeToPNG());}
  finally{camera.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);}
 }
 [MenuItem("WHATTHE FISH?/Golf/Place, audit and capture five holes")]
 public static void PrepareAndCapture()=>Review(true);
 [MenuItem("WHATTHE FISH?/Golf/Audit and capture saved five holes")]
 public static void CaptureSavedMap()=>Review(false);
 public static void Windows()=>ProjectBuilder.BuildWindowsPlayer("../Builds/GolfPlayabilityQA/Player/WhatTheFish.exe");
 public static void PrepareAndBuild(){Review(true);Windows();}
 public static void Mobile(){Review(false);MobileOptimizationValidation.BuildPreview();}
 public static void Scripts(){
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=SkySailBuilder.BuildScenes(),locationPathName="../Builds/GolfPlayabilityQA/Player/WhatTheFish.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development|BuildOptions.BuildScriptsOnly|BuildOptions.CompressWithLz4HC});
  if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Golf scripts build failed");
 }
 static void Review(bool regenerate){
  Directory.CreateDirectory(GolfCourseBuilder.Evidence);results.Clear();
  string graphics=AssetDatabase.GetAssetPath(GraphicsSettings.defaultRenderPipeline),quality=AssetDatabase.GetAssetPath(QualitySettings.renderPipeline);
  try{
   if(regenerate)GolfCourseBuilder.UpdateMap();
   var scene=regenerate?UnityEngine.SceneManagement.SceneManager.GetActiveScene():EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_Golf.unity");var island=scene.GetRootGameObjects().Single(r=>r.GetComponent<SkySailIslandScene>());Audit(island,"streamed scene");
   if(regenerate){var contents=PrefabUtility.LoadPrefabContents("Assets/_Game/Prefabs/Environments/GolfEnvironment.prefab");
   try{
    Audit(contents,"environment prefab");var before=contents.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name.StartsWith("Terrain__")).Select(f=>f.sharedMesh.vertexCount+":"+f.sharedMesh.triangles.Length).ToArray();
    GolfCourseBuilder.Attach(contents);var after=contents.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name.StartsWith("Terrain__")).Select(f=>f.sharedMesh.vertexCount+":"+f.sharedMesh.triangles.Length).ToArray();Check(before.SequenceEqual(after),"repeated generation retains identical terrain counts");Audit(contents,"repeat generation");
   }finally{PrefabUtility.UnloadPrefabContents(contents);}}
   var desktop=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(MobilePackageCleanup.DesktopPipelinePath);GraphicsSettings.defaultRenderPipeline=desktop;QualitySettings.renderPipeline=desktop;ShaderUtil.allowAsyncCompilation=false;
   island.SetActive(true);var camera=new GameObject("Course review camera").AddComponent<Camera>();camera.gameObject.AddComponent<UniversalAdditionalCameraData>();camera.nearClipPlane=.01f;camera.allowHDR=false;
   var sun=new GameObject("Course review sun").AddComponent<Light>();sun.type=LightType.Directional;RenderSettings.sun=sun;island.GetComponent<RefinedIslandEnvironment>().Apply(camera,sun);
   Capture(camera,"overview",island.transform.TransformPoint(new Vector3(270,285,-340)),island.transform.TransformPoint(new Vector3(0,4,5)),50);
   foreach(var h in island.GetComponentInChildren<GolfCourse>().holes){
    var p=h.cup.position;Capture(camera,"hole-"+h.number,p+new Vector3(-8,5,-11),p+Vector3.up*.9f,42);
    Capture(camera,"approach-"+h.number,h.tee.position+new Vector3(-3,6,-8),p+Vector3.up,48);
   }
   var first=island.GetComponentInChildren<GolfCourse>().holes[0];Capture(camera,"cup-detail",first.cup.position+new Vector3(-.5f,.42f,-.65f),first.cup.position-Vector3.up*.04f,42);
   Object.DestroyImmediate(camera.gameObject);Object.DestroyImmediate(sun.gameObject);island.SetActive(false);Debug.Log("GOLF_FIVE_HOLES_REVIEW_OK");
  }finally{
   File.WriteAllLines(Path.Combine(GolfCourseBuilder.Evidence,regenerate?"editor-audit.txt":"saved-editor-audit.txt"),results);
   GraphicsSettings.defaultRenderPipeline=AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(graphics);QualitySettings.renderPipeline=AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(quality);AssetDatabase.SaveAssets();
  }
 }
}
