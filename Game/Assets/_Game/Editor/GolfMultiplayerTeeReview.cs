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

public static partial class GolfMultiplayerTeeBuild {
 public static void AuditAndCapture(){
  Directory.CreateDirectory(Evidence);var lines=new List<string>();
  void Check(bool ok,string message){lines.Add((ok?"PASS ":"FAIL ")+message);File.WriteAllLines(Path.Combine(Evidence,"editor-audit.txt"),lines);if(!ok)throw new Exception(message);}
  var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_Golf.unity");var island=scene.GetRootGameObjects().Single(r=>r.GetComponent<SkySailIslandScene>());island.SetActive(true);var course=island.GetComponentInChildren<GolfCourse>();Physics.SyncTransforms();
  Check(course.teeSites.Length==5,"five authored tee sites");Check(course.waterPatches.Length==6,"six ponds retained");Check(course.holes.Length==5,"five original holes retained");
  foreach(var s in course.teeSites){Check(s.count==(s.hole==1?10:3),"T"+s.hole+" pad count");var pads=Enumerable.Range(0,s.count).Select(s.Pad).ToArray();Check(pads.Select(p=>$"{p.x:F3}/{p.z:F3}").Distinct().Count()==s.count,"T"+s.hole+" unique pads");
   foreach(var p in pads){Check(Mathf.Abs(Vector3.ProjectOnPlane(p-s.cup,Vector3.up).magnitude-s.radius)<.001f,"T"+s.hole+" equal cup distance");var world=island.transform.TransformPoint(p);bool ground=Physics.Raycast(world+Vector3.up*20,Vector3.down,out var hit,40,1<<8,QueryTriggerInteraction.Ignore);Check(ground&&Mathf.Abs(hit.point.y-world.y)<.04f&&hit.normal.y>.99f,"T"+s.hole+" level supported pad "+p);}
   var alloc=new GolfTeeReservations(37);for(ulong p=101;p<104;p++)alloc.Request(s.hole,p);var assigned=alloc.Assign(_=>s.count,(_,i)=>s.Offset(i),(_,i)=>true);Check(assigned.Count==3&&assigned[0].slot==0,"T"+s.hole+" centre assigned first");Check(assigned.Skip(1).Select(p=>s.Offset(p.slot)).OrderBy(x=>x).SequenceEqual(new[]{-1,1}),"T"+s.hole+" second and third use opposite sides");
  }
  for(int seed=0;seed<100;seed++){var a=new GolfTeeReservations(seed);for(ulong id=1;id<=10;id++)a.Request(2,id,id<4?0:1);var first=a.Assign(_=>3,(_,i)=>i==0?0:i==1?-1:1,(_,i)=>true);Check(first.Count==3&&first.Select(v=>v.slot).Distinct().Count()==3,"reservation unique seed "+seed);Check(first.All(v=>v.owner<4),"earlier cohort precedes later cohort "+seed);a.Release(2,first[0].owner);var next=a.Assign(_=>3,(_,i)=>i==0?0:i==1?-1:1,(_,i)=>true);Check(next.Count==1&&next[0].owner>=4,"release fills one waiting pad "+seed);}
  var ten=new GolfTeeReservations(1);for(ulong p=1;p<=10;p++)ten.Request(1,p);var order=ten.Assign(_=>10,(_,i)=>course.teeSites[0].Offset(i),(_,i)=>true);Check(order.Count==10,"ten unique T1 reservations");Check(order.Select(v=>Mathf.Abs(course.teeSites[0].Offset(v.slot))).SequenceEqual(new[]{0,1,1,2,2,3,3,4,4,5}),"T1 fills centre then outward");
  var pathSite=course.teeSites.First(s=>s.hole==1);var root=course.transform.Find("Multiplayer tee grounds");Check(!root.Find("Terrain__Tee terrace 1")&&!root.Find("Terrain__Rear tee path"),"T1 has no floating opaque ground overlay");var sign=course.transform.Find("Tee sign 1");Check(Vector3.ProjectOnPlane(sign.position-island.transform.TransformPoint(pathSite.centre),Vector3.up).magnitude<8,"Hole 1 sign is beside T1");
  for(float a=-.28f;a<=.28f;a+=.04f){var p=pathSite.cup+new Vector3(Mathf.Cos(pathSite.Angle+a),0,Mathf.Sin(pathSite.Angle+a))*75.5f;var ray=new Ray(island.transform.TransformPoint(p)+Vector3.up*50,Vector3.down);Check(Physics.Raycast(ray,out var floor,100,1<<8,QueryTriggerInteraction.Ignore),"rear path supports walking");var blockers=Physics.OverlapCapsule(floor.point+Vector3.up*.5f,floor.point+Vector3.up*1.4f,.3f,1<<8,QueryTriggerInteraction.Ignore).Where(c=>!c.name.StartsWith("Terrain__")).ToArray();Check(blockers.Length==0,"rear path capsule clear "+string.Join(",",blockers.Select(c=>c.name)));}
  ValidateT1Ground(island,course,Check);
  var graphics=GraphicsSettings.defaultRenderPipeline;var quality=QualitySettings.renderPipeline;
  try{
   var desktop=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(MobilePackageCleanup.DesktopPipelinePath);GraphicsSettings.defaultRenderPipeline=desktop;QualitySettings.renderPipeline=desktop;ShaderUtil.allowAsyncCompilation=false;
   var camera=new GameObject("Tee audit camera").AddComponent<Camera>();camera.gameObject.AddComponent<UniversalAdditionalCameraData>();camera.nearClipPlane=.01f;camera.farClipPlane=1000;camera.allowHDR=false;
   var sun=new GameObject("Tee audit sun").AddComponent<Light>();sun.type=LightType.Directional;RenderSettings.sun=sun;island.GetComponent<RefinedIslandEnvironment>().Apply(camera,sun);
   void Capture(string name,Vector3 at,Vector3 target,bool ortho=false,float size=30){camera.orthographic=ortho;camera.orthographicSize=size;camera.fieldOfView=52;camera.transform.position=island.transform.TransformPoint(at);camera.transform.LookAt(island.transform.TransformPoint(target));var rt=new RenderTexture(1600,1100,24);var tex=new Texture2D(1600,1100,TextureFormat.RGB24,false);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,1100),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(Evidence,name+".png"),tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);}
   Capture("course-top",new Vector3(0,450,0),Vector3.zero,true,220);
   foreach(var s in course.teeSites){Capture("tee-"+s.hole+"-top",s.centre+Vector3.up*90,s.centre,true,s.hole==1?40:21);Capture("tee-"+s.hole+"-near",s.centre+new Vector3(-13,10,-17),s.centre);}
   var t1=course.teeSites[0];var radial=Vector3.ProjectOnPlane(t1.centre-t1.cup,Vector3.up).normalized;Capture("tee-1-entry-low",t1.centre+radial*15+Vector3.up*1.8f,t1.centre+Vector3.up*.2f);Capture("tee-1-entry-low-side",t1.centre+radial*16+new Vector3(5,1.6f,4),t1.centre+Vector3.up*.2f);
   Object.DestroyImmediate(camera.gameObject);Object.DestroyImmediate(sun.gameObject);
  }finally{GraphicsSettings.defaultRenderPipeline=graphics;QualitySettings.renderPipeline=quality;}
  lines.Add("COMPLETE checks="+lines.Count);File.WriteAllLines(Path.Combine(Evidence,"editor-audit.txt"),lines);Debug.Log("GOLF_TEE_EDITOR_AUDIT_COMPLETE "+lines.Count);
 }
}
