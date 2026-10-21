using System;
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
 static string ColourEvidence=>Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/GolfTeeColourQA"));
 public static void CaptureColourBefore()=>CaptureTeeColours("before");
 static void CaptureTeeColours(string phase){
  string output=Path.Combine(ColourEvidence,phase);Directory.CreateDirectory(output);
  var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_Golf.unity");var island=scene.GetRootGameObjects().Single(r=>r.GetComponent<SkySailIslandScene>());island.SetActive(true);var course=island.GetComponentInChildren<GolfCourse>();
  File.WriteAllText(Path.Combine(output,"tee-sites.json"),JsonUtility.ToJson(course,true));
  var graphics=GraphicsSettings.defaultRenderPipeline;var quality=QualitySettings.renderPipeline;
  try{
   var desktop=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(MobilePackageCleanup.DesktopPipelinePath);GraphicsSettings.defaultRenderPipeline=desktop;QualitySettings.renderPipeline=desktop;ShaderUtil.allowAsyncCompilation=false;
   var camera=new GameObject("Tee colour review camera").AddComponent<Camera>();camera.gameObject.AddComponent<UniversalAdditionalCameraData>();camera.nearClipPlane=.03f;camera.farClipPlane=1000;camera.allowHDR=false;
   var sun=new GameObject("Tee colour review sun").AddComponent<Light>();sun.type=LightType.Directional;RenderSettings.sun=sun;island.GetComponent<RefinedIslandEnvironment>().Apply(camera,sun);
   void Capture(string name,Vector3 at,Vector3 target,bool ortho=false,float size=24){
    camera.orthographic=ortho;camera.orthographicSize=size;camera.fieldOfView=52;camera.transform.position=island.transform.TransformPoint(at);camera.transform.LookAt(island.transform.TransformPoint(target));
    var rt=new RenderTexture(1600,1100,24);var tex=new Texture2D(1600,1100,TextureFormat.RGB24,false);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,1100),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);
   }
   foreach(var s in course.teeSites.Where(t=>t.hole>=2)){Capture("tee-"+s.hole+"-top",s.centre+Vector3.up*90,s.centre,true,23);Capture("tee-"+s.hole+"-near",s.centre+new Vector3(-16,5,-19),s.centre);}
   var first=course.teeSites.First(s=>s.hole==1);Capture("tee-1-top",first.centre+Vector3.up*90,first.centre,true,40);
   var corridor=new Vector3(-38.5f,first.height,-150);Capture("entry-corridor-top",corridor+Vector3.up*90,corridor,true,15);Capture("entry-corridor-near",corridor+new Vector3(-12,4,-12),corridor);
   Object.DestroyImmediate(camera.gameObject);Object.DestroyImmediate(sun.gameObject);
  }finally{GraphicsSettings.defaultRenderPipeline=graphics;QualitySettings.renderPipeline=quality;}
  Debug.Log("GOLF_TEE_COLOUR_CAPTURED "+phase);
 }
}
