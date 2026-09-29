using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WhatTheFish;

public static class MobilePackageCleanup {
 public const string EnvironmentFolder="Assets/_Game/Prefabs/Environments/";
 public const string DesktopPipelinePath="Assets/_Game/Settings/DesktopCoastURP.asset";
 public static void RemoveUnusedMaterialInputAndBuildAndroid(){
  const string path="Assets/_Game/Art/RefinedIslands/Shared/Materials/RI_Stone.mat";
  var material=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(!material||material.shader.name!="WhatTheFish/RefinedStone")throw new Exception("Unexpected stone material/shader; re-audit its texture usage.");
  // RefinedStone samples only base color and normals; its smoothness is .18.
  // The old generic material builder assigned this mask without a shader read.
  material.SetTexture("_MetallicGlossMap",null);EditorUtility.SetDirty(material);AssetDatabase.SaveAssets();
  Directory.CreateDirectory(BuildSizeAudit.Output);
  File.AppendAllText(BuildSizeAudit.Output+"cleanup-manifest.txt","REMOVE unused RefinedStone _MetallicGlossMap reference; source mask retained for authoring.\n");
  ProjectBuilder.BuildAndroidRelease();
 }
 public static void PackTerrainControlsAndBuildAndroid(){
  if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android)throw new Exception("Run with -buildTarget Android.");
  var lines=new List<string>();
  foreach(string sport in new[]{"Golf","Fishing"}){
   string path="Assets/_Game/Art/RefinedIslands/"+sport+"/"+sport+"_TerrainControl.png";
   var importer=(TextureImporter)AssetImporter.GetAtPath(path);
   var original=importer.GetPlatformTextureSettings("Android");
   bool readable=importer.isReadable;bool validated=false;
   try {
    importer.isReadable=true;importer.SaveAndReimport();
    var before=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    int width=before.width,height=before.height,mips=before.mipmapCount;
    var red=new byte[mips][];
    for(int mip=0;mip<mips;mip++)red[mip]=before.GetPixels32(mip).Select(p=>p.r).ToArray();
    string oldFormat=before.format.ToString();
    var settings=importer.GetPlatformTextureSettings("Android");settings.overridden=true;
    settings.format=TextureImporterFormat.R8;settings.maxTextureSize=Math.Max(width,height);
    settings.textureCompression=TextureImporterCompression.Uncompressed;settings.crunchedCompression=false;
    importer.SetPlatformTextureSettings(settings);importer.SaveAndReimport();
    var after=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    if(after.format!=TextureFormat.R8||after.width!=width||after.height!=height||after.mipmapCount!=mips)throw new Exception("Terrain control import changed resolution/mips or did not use R8: "+path);
    for(int mip=0;mip<mips;mip++){
     var data=after.GetPixelData<byte>(mip);
     if(data.Length!=red[mip].Length)throw new Exception("Terrain control pixel count changed: "+path);
     for(int i=0;i<data.Length;i++)if(data[i]!=red[mip][i])throw new Exception("Terrain red channel changed: "+path+" mip="+mip+" pixel="+i);
    }
    validated=true;lines.Add($"PASS {sport}: {oldFormat} -> R8; {width}x{height}; {mips} mips; EVERY red-channel sample is byte-identical; source PNG and desktop import unchanged.");
   } finally {
    importer=(TextureImporter)AssetImporter.GetAtPath(path);
    if(!validated)importer.SetPlatformTextureSettings(original);
    importer.isReadable=readable;importer.SaveAndReimport();
   }
  }
  Directory.CreateDirectory(BuildSizeAudit.Output);File.WriteAllLines(BuildSizeAudit.Output+"terrain-control-validation.txt",lines);
  ProjectBuilder.BuildAndroidRelease();
 }
 public static void Apply(){
  Directory.CreateDirectory(EnvironmentFolder);Directory.CreateDirectory("Assets/_Game/Prefabs/Characters");AssetDatabase.Refresh();
  var changes=new List<string>();
  foreach(string sport in new[]{"Football","Basketball","Golf","Fishing"})Move("Assets/_Game/Resources/"+sport+"Environment.prefab",EnvironmentFolder+sport+"Environment.prefab",changes);
  foreach(string name in new[]{"OfflineAthlete","NetworkAthlete"})Move("Assets/_Game/Resources/"+name+".prefab","Assets/_Game/Prefabs/Characters/"+name+".prefab",changes);
  Move("Assets/_Game/Resources/DesktopCoastURP.asset",DesktopPipelinePath,changes);
  foreach(string path in SkySailBuilder.BuildScenes()){
   var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
   foreach(var root in scene.GetRootGameObjects().Where(r=>r.GetComponent<SkySailIslandScene>())){
    // Sky-Sail owns the single ocean/cloud field. These old island objects were
    // permanently disabled by SkySailBuilder and are never enabled at runtime.
    var retired=root.GetComponentsInChildren<Transform>(true).Where(t=>!t.gameObject.activeSelf&&IsRetiredScenery(t.name)).ToArray();
    foreach(var t in retired){changes.Add("REMOVE disabled scenery "+path+" :: "+t.name);UnityEngine.Object.DestroyImmediate(t.gameObject);}
   }
   Validate(scene);EditorSceneManager.SaveScene(scene);
  }
  EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");AssetDatabase.SaveAssets();
  Directory.CreateDirectory(BuildSizeAudit.Output);File.WriteAllLines(BuildSizeAudit.Output+"cleanup-manifest.txt",changes);
  Debug.Log("MOBILE_PACKAGE_CLEANUP_COMPLETE changes="+changes.Count);
 }
 public static bool IsRetiredScenery(string name)=>name=="Sculpted cumulus cloud field"||name=="Sculpted cloud field"||name=="Sculpted clouds"||name=="Animated ocean and shore wash"||name=="Coastal sea with shore wash";
 static void Move(string source,string destination,List<string> changes){
  if(!File.Exists(source)){if(!File.Exists(destination))throw new Exception("Missing source and destination: "+source);return;}
  string error=AssetDatabase.MoveAsset(source,destination);if(!string.IsNullOrEmpty(error))throw new Exception(error);
  changes.Add("MOVE (GUID preserved) "+source+" -> "+destination);
 }
 static void Validate(Scene scene){
  foreach(var root in scene.GetRootGameObjects()){
   foreach(var t in root.GetComponentsInChildren<Transform>(true))if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0)throw new Exception("Missing script: "+scene.path+" / "+t.name);
   foreach(var r in root.GetComponentsInChildren<Renderer>(true))if(r.sharedMaterials.Any(m=>!m))throw new Exception("Missing material: "+scene.path+" / "+r.name);
   foreach(var f in root.GetComponentsInChildren<MeshFilter>(true))if(!f.sharedMesh)throw new Exception("Missing mesh: "+scene.path+" / "+f.name);
  }
 }
}

// Changes only build copies of scenes, preserving desktop contact shading.
public sealed class AndroidSceneDependencies : IProcessSceneWithReport {
 public int callbackOrder=>0;
 public void OnProcessScene(Scene scene,BuildReport report){
  if(report==null||report.summary.platform!=BuildTarget.Android)return;
  foreach(var root in scene.GetRootGameObjects()){
   foreach(var coast in root.GetComponentsInChildren<CoastalEnvironment>(true))coast.exteriorContactShading=null;
   foreach(var island in root.GetComponentsInChildren<RefinedIslandEnvironment>(true))island.contactShading=null;
  }
 }
}
