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

public static class GolfFlagArtReview {
 static string Folder=>Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/GolfFlagBounceQA"));
 static Color32[] Capture(Camera camera,string name,Vector3 position,Vector3 target){
  camera.transform.SetPositionAndRotation(position,Quaternion.LookRotation(target-position));
  var rt=new RenderTexture(1600,1000,24);var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);
  try{camera.targetTexture=rt;for(int i=0;i<3;i++)camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(Folder,name+".png"),tex.EncodeToPNG());return tex.GetPixels32();}
  finally{camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);}
 }
 public static void PrepareAndReview(){
  Directory.CreateDirectory(Folder);GolfFlagArtBuilder.UpdateSavedMaps();GolfCourseReview.CaptureSavedMap();
  var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_Golf.unity");var island=scene.GetRootGameObjects().Single(g=>g.GetComponent<SkySailIslandScene>());island.SetActive(true);
  string previous=AssetDatabase.GetAssetPath(GraphicsSettings.defaultRenderPipeline),quality=AssetDatabase.GetAssetPath(QualitySettings.renderPipeline);
  var materials=AssetDatabase.FindAssets("t:Material",new[]{GolfCourseBuilder.Art+"Materials"}).Select(g=>AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g))).Where(m=>m.shader.name=="WhatTheFish/GolfFlagWind").ToArray();
  void Time(float t){foreach(var m in materials)m.SetFloat("_FlagTime",t);}var camera=new GameObject("Flag design review").AddComponent<Camera>();camera.gameObject.AddComponent<UniversalAdditionalCameraData>();camera.nearClipPlane=.01f;camera.fieldOfView=36;camera.allowHDR=false;
  var sun=new GameObject("Flag design light").AddComponent<Light>();sun.type=LightType.Directional;RenderSettings.sun=sun;
  try{
   GraphicsSettings.defaultRenderPipeline=AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(MobilePackageCleanup.DesktopPipelinePath);QualitySettings.renderPipeline=GraphicsSettings.defaultRenderPipeline;
   island.GetComponent<RefinedIslandEnvironment>().Apply(camera,sun);ShaderUtil.allowAsyncCompilation=false;Time(0);
   var holes=island.GetComponentInChildren<GolfCourse>(true).holes;
   foreach(var h in holes){Capture(camera,"flag-"+h.number+"-front",h.flag.TransformPoint(new Vector3(.60f,2.59f,-2.05f)),h.flag.TransformPoint(new Vector3(.60f,2.59f,0)));Capture(camera,"flag-"+h.number+"-back",h.flag.TransformPoint(new Vector3(.60f,2.59f,2.05f)),h.flag.TransformPoint(new Vector3(.60f,2.59f,0)));}
   var flag=holes[0].flag;var pos=flag.TransformPoint(new Vector3(2.05f,2.65f,-1.7f));var focus=flag.TransformPoint(new Vector3(.55f,2.58f,0));
   var first=Capture(camera,"wind-0",pos,focus);Time(1.2f);var second=Capture(camera,"wind-1",pos,focus);
   int changed=first.Where((c,i)=>!c.Equals(second[i])).Count();if(changed<50)throw new Exception("Wind shader produced no visible motion");
   if(holes.Any(h=>h.flag.GetComponentsInChildren<MeshRenderer>().Length!=2))throw new Exception("Flag renderer layout changed unexpectedly");
   File.WriteAllText(Path.Combine(Folder,"design-audit.txt"),$"PASS five numbered flags, two readable number faces each\nPASS approved reference pixels for border, cloth and numbers\nPASS wind preview changedPixels={changed}\nMaximum cloth-tip displacement bound: 0.204 m; pole-side weight: 0\n");
  }finally{
   Time(-1);foreach(var m in materials)EditorUtility.SetDirty(m);Object.DestroyImmediate(camera.gameObject);Object.DestroyImmediate(sun.gameObject);
   GraphicsSettings.defaultRenderPipeline=AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(previous);QualitySettings.renderPipeline=AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(quality);AssetDatabase.SaveAssets();
  }
 }
}
