using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using WhatTheFish;

public static class CharacterRefinementReview {
 const string Root="../Builds/CharacterRefinementQA/";
 public static void Build(){CharacterIdentityBuild.Prepare();ProjectBuilder.BuildWindowsPlayer(Root+"Player/WhatTheFish.exe");}
 public static void PreviewAndBuild(){Preview();Build();}
 public static void Preview(){
  // Explicit art review regenerates with the currently compiled generator even
  // if a concurrent source edit happened during an earlier build's hash read.
  var previousLibrary=AssetDatabase.LoadAssetAtPath<CharacterIdentityLibrary>("Assets/_Game/Art/CharacterIdentity/Library.asset");if(previousLibrary)previousLibrary.sourceHash=null;
  CharacterIdentityBuild.Prepare();var setup=EditorSceneManager.GetSceneManagerSetup();
  string previousPipeline=AssetDatabase.GetAssetPath(GraphicsSettings.defaultRenderPipeline),previousQuality=AssetDatabase.GetAssetPath(QualitySettings.renderPipeline);
  // Keep contact sheets deterministic when a preceding desktop build selected
  // its SSAO renderer. Gameplay cameras are checked separately in the player.
  var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/_Game/Settings/MobileURP.asset");
  GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;
  string output=ProjectBuilder.ResolveBuildOutputPath(Root+"ArtPreview");Directory.CreateDirectory(output);
  var report=new StringBuilder();
  try{
   EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.65f,.65f,.65f);
   var light=new GameObject("Soft key").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.2f;light.transform.rotation=Quaternion.Euler(35,-30,0);
   var fill=new GameObject("Fill").AddComponent<Light>();fill.type=LightType.Directional;fill.intensity=.65f;fill.transform.rotation=Quaternion.Euler(20,150,0);
   var camera=new GameObject("Head inspection").AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.36f,.42f,.47f);camera.orthographic=true;camera.orthographicSize=.50f;camera.nearClipPlane=.03f;
   var actor=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Characters/OfflineAthlete.prefab")).GetComponent<Athlete>();
   var identity=CharacterIdentity.Ensure(actor);var lodGroup=actor.visual.GetComponent<LODGroup>();
   var library=actor.identityLibrary;
   foreach(var mesh in library.nearHair.Concat(library.farHair).Concat(new[]{library.nearScalp,library.farScalp}).Where(m=>m)){
    if(mesh.vertexCount!=mesh.colors32.Length||mesh.vertexCount!=mesh.normals.Length)throw new Exception("Missing vertex data: "+mesh.name);
    if(mesh.vertices.Any(v=>float.IsNaN(v.x)||float.IsNaN(v.y)||float.IsNaN(v.z)))throw new Exception("Non-finite mesh: "+mesh.name);
    if(mesh.normals.Any(n=>n.sqrMagnitude<.8f||n.sqrMagnitude>1.2f))throw new Exception("Invalid normal: "+mesh.name);
    report.AppendLine($"PASS {mesh.name} vertices={mesh.vertexCount} triangles={mesh.triangles.Length/3}");
   }
   string[] views={"front","quarter","side","back"};float[] angles={0,45,90,180};
   for(int lod=0;lod<2;lod++)for(int view=0;view<views.Length;view++){
    const int tile=480;var sheet=new Texture2D(tile*5,tile*2,TextureFormat.RGB24,false);
    for(int i=0;i<10;i++){
     identity.SetNumber((byte)(i+1));lodGroup.ForceLOD(lod);
     var target=new Vector3(0,1.39f,.02f);float a=angles[view]*Mathf.Deg2Rad;
     camera.transform.position=target+new Vector3(Mathf.Sin(a)*3,.12f,Mathf.Cos(a)*3);camera.transform.LookAt(target);
     var rt=RenderTexture.GetTemporary(tile,tile,24);var previous=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
     var pixels=new Texture2D(tile,tile,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,tile,tile),0,0);pixels.Apply();sheet.SetPixels((i%5)*tile,(1-i/5)*tile,tile,tile,pixels.GetPixels());
     File.WriteAllBytes(Path.Combine(output,$"{i+1:00}-{views[view]}-LOD{lod}.png"),pixels.EncodeToPNG());
     camera.targetTexture=null;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(pixels);
    }
    sheet.Apply();File.WriteAllBytes(Path.Combine(output,$"all-{views[view]}-LOD{lod}.png"),sheet.EncodeToPNG());UnityEngine.Object.DestroyImmediate(sheet);
   }
   File.WriteAllText(Path.Combine(output,"mesh-validation.txt"),report.ToString());Debug.Log("CHARACTER_REFINEMENT_PREVIEW_PASS "+output);
  }finally{GraphicsSettings.defaultRenderPipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(previousPipeline);QualitySettings.renderPipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(previousQuality);if(setup.Any(s=>s.isLoaded&&s.isActive))EditorSceneManager.RestoreSceneManagerSetup(setup);else EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");}
 }
}
