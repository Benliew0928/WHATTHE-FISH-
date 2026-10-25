using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Reproducible studio captures of the actual gameplay prefabs, not substitute art.
public static class WelcomeSportIconBuild {
 const string Output="Assets/_Game/Resources/WelcomeSportIcons";
 static readonly string[] Names={"Football","Basketball","Golf","Fishing"};
 static readonly string[] Prefabs={"Football/SoccerBall","Basketball/BasketballBall","Golf/Ball","Fishing/MediumLagoonFish"};
 static string DiskPath(string assetPath)=>Path.Combine(Application.dataPath,assetPath.Substring("Assets/".Length));
 [MenuItem("WHATTHE FISH?/Welcome/Render sport icons")]
 public static void Capture(){
  var setup=EditorSceneManager.GetSceneManagerSetup();var oldPipeline=GraphicsSettings.defaultRenderPipeline;var oldQuality=QualitySettings.renderPipeline;
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var pipeline=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/_Game/Settings/MobileURP.asset"));
  pipeline.hideFlags=HideFlags.HideAndDontSave;
  pipeline.renderScale=1;pipeline.msaaSampleCount=4;GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;
  try{
   // Batch editor entry runs before the first render loop. Initialize its SRP
   // explicitly; this editor-only bridge is pinned to the project's Unity version.
   var prepare=typeof(RenderPipelineManager).GetMethod("TryPrepareRenderPipeline",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public);
   if(prepare==null)throw new Exception("Unity SRP preview initialization changed; update this editor capture bridge.");
   prepare.Invoke(null,new object[]{pipeline});
   Directory.CreateDirectory(DiskPath(Output));
   RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.52f,.56f,.62f);
   Light key=new GameObject("Icon key").AddComponent<Light>();key.type=LightType.Directional;key.intensity=1.25f;key.transform.rotation=Quaternion.Euler(35,-35,0);key.shadows=LightShadows.None;
   Light fill=new GameObject("Icon fill").AddComponent<Light>();fill.type=LightType.Directional;fill.intensity=.4f;fill.transform.rotation=Quaternion.Euler(20,135,0);fill.shadows=LightShadows.None;
   var camera=new GameObject("Icon camera").AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.orthographic=true;camera.nearClipPlane=.01f;camera.farClipPlane=100;camera.allowHDR=false;camera.allowMSAA=true;
   camera.GetUniversalAdditionalCameraData().renderPostProcessing=false;
   ShaderUtil.allowAsyncCompilation=false;
   for(int i=0;i<Names.Length;i++){
    key.intensity=i==2?.85f:1.25f;fill.intensity=i==2?.25f:.4f;
    string source="Assets/_Game/Prefabs/"+Prefabs[i]+".prefab";
    var instance=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(source));
    var copies=new System.Collections.Generic.List<Material>();
    try{
     foreach(var body in instance.GetComponentsInChildren<Rigidbody>())body.isKinematic=true;
     var lods=instance.GetComponentsInChildren<LODGroup>();
     var high=lods.SelectMany(l=>l.GetLODs().Take(1)).SelectMany(l=>l.renderers).ToArray();
     var low=lods.SelectMany(l=>l.GetLODs().Skip(1)).SelectMany(l=>l.renderers).Except(high).ToArray();
     foreach(var lod in lods)UnityEngine.Object.DestroyImmediate(lod);
     foreach(var r in low)if(r)r.enabled=false;
     foreach(var r in instance.GetComponentsInChildren<Renderer>())if(!(r is MeshRenderer)&&!(r is SkinnedMeshRenderer))r.enabled=false;
     foreach(var r in instance.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>{if(!m||!m.HasProperty("_TailMotion"))return m;var copy=new Material(m);copy.SetFloat("_TailMotion",0);copies.Add(copy);return copy;}).ToArray();
     // Fish swim along Z: a broadside, slightly frontal view exposes the real silhouette.
     camera.transform.rotation=Quaternion.LookRotation(i==3?new Vector3(-1,-.18f,.25f):new Vector3(-.3f,-.2f,1));
     var renderers=instance.GetComponentsInChildren<Renderer>().Where(r=>r.enabled&&(r is MeshRenderer||r is SkinnedMeshRenderer)).ToArray();
     Bounds bounds=renderers[0].bounds;foreach(var r in renderers.Skip(1))bounds.Encapsulate(r.bounds);
     var center=bounds.center;float extent=0;
     for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2){var p=camera.transform.InverseTransformDirection(Vector3.Scale(bounds.extents,new Vector3(x,y,z)));extent=Mathf.Max(extent,Mathf.Abs(p.x),Mathf.Abs(p.y));}
     camera.orthographicSize=extent*1.10f;camera.transform.position=center-camera.transform.forward*(bounds.size.magnitude+2);
     var rt=new RenderTexture(1024,1024,24,RenderTextureFormat.ARGB32){antiAliasing=4};rt.Create();var previous=RenderTexture.active;var pixels=new Texture2D(1024,1024,TextureFormat.RGBA32,false);
     try{camera.targetTexture=rt;for(int n=0;n<3;n++)camera.Render();RenderTexture.active=rt;pixels.ReadPixels(new Rect(0,0,1024,1024),0,0);pixels.Apply();
      var colors=pixels.GetPixels32();if(colors.Count(p=>p.a>128)<10000||colors.Count(p=>p.r>240&&p.b>240&&p.g<10)>1000)throw new Exception("Invalid or unshaded icon capture: "+Names[i]);
      File.WriteAllBytes(DiskPath(Output+"/"+Names[i]+".png"),pixels.EncodeToPNG());}
     finally{camera.targetTexture=null;RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(pixels);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
     Debug.Log("WELCOME_ICON_CAPTURE "+Names[i]+" source="+source+" renderers="+renderers.Length);
    }finally{UnityEngine.Object.DestroyImmediate(instance);foreach(var copy in copies)UnityEngine.Object.DestroyImmediate(copy);}
   }
   AssetDatabase.Refresh();Validate();
  }finally{GraphicsSettings.defaultRenderPipeline=oldPipeline;QualitySettings.renderPipeline=oldQuality;UnityEngine.Object.DestroyImmediate(pipeline);if(setup.Any(s=>s.isLoaded&&s.isActive))EditorSceneManager.RestoreSceneManagerSetup(setup);else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);}
 }
 public static void Validate(){
  foreach(string name in Names){var image=AssetDatabase.LoadAssetAtPath<Texture2D>(Output+"/"+name+".png");if(!image||image.width!=1024||image.height!=1024)throw new Exception("Missing full-resolution welcome icon: "+name);}
 }
 public static void Windows(){Validate();ProjectBuilder.BuildWindowsPlayer("../Builds/WelcomeIconsQA/Player/WhatTheFish.exe");}
 public static void CaptureAndBuild(){Capture();Windows();}
}

public sealed class WelcomeSportIconImport:AssetPostprocessor {
 void OnPreprocessTexture(){
  if(!assetPath.StartsWith("Assets/_Game/Resources/WelcomeSportIcons/"))return;
  var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.alphaIsTransparency=true;t.sRGBTexture=true;
  t.mipmapEnabled=false;t.npotScale=TextureImporterNPOTScale.None;t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;t.maxTextureSize=1024;t.textureCompression=TextureImporterCompression.Uncompressed;
  t.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=1024,format=TextureImporterFormat.ASTC_4x4,compressionQuality=100});
 }
}
