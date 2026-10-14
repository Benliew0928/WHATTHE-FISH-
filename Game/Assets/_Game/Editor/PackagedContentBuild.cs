using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WhatTheFish;

public static class PackagedContentBuild {
 const string Generated="Assets/_Game/GeneratedPackaging";
 public static string PayloadDirectory {get;private set;}
 public static bool IsPrepared(Scene scene)=>scene.path.StartsWith(Generated+"/",StringComparison.Ordinal);
 public static void CopyContentAudit(BuildTarget target){
  string audit=Path.Combine(BuildSizeAudit.Output,"content-"+target),latest=Path.Combine(BuildSizeAudit.Output,"latest");
  foreach(string file in new[]{"packed-assets.csv","packed-layout.csv","meshes.csv","summary.txt"})File.Copy(Path.Combine(audit,file),Path.Combine(latest,"bundle-"+file),true);
  string catalog=ProjectBuilder.ResolveBuildOutputPath("../Builds/PackagedContent/"+target+"/Payload/catalog.json");
  File.Copy(catalog,Path.Combine(latest,"bundle-catalog.json"),true);
  File.AppendAllText(Path.Combine(latest,"summary.txt"),"\nScene content is in the lossless StreamingAssets bundle. See bundle-packed-assets.csv and bundle-packed-layout.csv for its separate serialized asset inventory. Do not interpret player-only packed-assets.csv as the whole game.\n");
 }
 public static void BuildWindows(){ProjectBuilder.BuildWindowsPlayer("../Builds/PackagingQA/Player/WhatTheFish.exe");}
 public static void BuildBaselineWindows(){
  string previous=Environment.GetEnvironmentVariable("WTF_UNPACKAGED_REVIEW");
  try{Environment.SetEnvironmentVariable("WTF_UNPACKAGED_REVIEW","1");ProjectBuilder.BuildWindowsPlayer("../Builds/PackagingQA/BaselinePlayer/WhatTheFish.exe");}
  finally{Environment.SetEnvironmentVariable("WTF_UNPACKAGED_REVIEW",previous);}
 }
 public static BuildReport Build(BuildTarget target,string output,BuildOptions options){
  string bundleRoot=ProjectBuilder.ResolveBuildOutputPath("../Builds/PackagedContent/"+target);
  string payload=Path.Combine(bundleRoot,"Payload");Directory.CreateDirectory(payload);
  Directory.CreateDirectory(Generated);AssetDatabase.Refresh();
  var originalSetup=EditorSceneManager.GetSceneManagerSetup();
  string[] sources=SkySailBuilder.BuildScenes();
  if(sources.Length!=5)throw new BuildFailedException("Packaged delivery requires Bootstrap and all four islands");
  bool mobile=target==BuildTarget.Android||Environment.GetEnvironmentVariable("WTF_MOBILE_PREVIEW")=="1";
  try{
   var copies=sources.Select(path=>Generated+"/"+Path.GetFileName(path)).ToArray();
   for(int i=0;i<sources.Length;i++){
    var scene=EditorSceneManager.OpenScene(sources[i],OpenSceneMode.Single);
    if(mobile){MobileGeometryBuilder.Apply(scene);MobileModelBuilder.Apply(scene);MobileMaterialBuilder.Apply(scene);}
    EditorSceneManager.SaveScene(scene,copies[i],true);
   }
   var entry=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   new GameObject("Packaged content").AddComponent<PackagedContentBoot>();
   string boot=Generated+"/PackagedBoot.unity";EditorSceneManager.SaveScene(entry,boot);
   AssetDatabase.Refresh();
   var manifest=BuildPipeline.BuildAssetBundles(bundleRoot,new[]{new AssetBundleBuild{assetBundleName="islands.bundle",assetNames=copies}},BuildAssetBundleOptions.StrictMode|BuildAssetBundleOptions.AssetBundleStripUnityVersion,target);
   if(!manifest)throw new BuildFailedException("Lossless scene bundle build failed");
   string bundle=Path.Combine(bundleRoot,"islands.bundle");
   if(!BuildPipeline.GetCRCForAssetBundle(bundle,out uint crc))throw new BuildFailedException("Cannot read content CRC");
   using var sha=SHA256.Create();using var stream=File.OpenRead(bundle);
   var catalog=new PackagedContentCatalog{bundle="islands.bundle",sha256=BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant(),bytes=stream.Length,crc=crc,scenes=copies};
   File.Copy(bundle,Path.Combine(payload,"islands.bundle"),true);File.WriteAllText(Path.Combine(payload,"catalog.json"),JsonUtility.ToJson(catalog,true));
   var contentReport=BuildReport.GetLatestReport();
   if(contentReport!=null)BuildSizeAudit.Write(contentReport,"content-"+target);
   PayloadDirectory=payload;
   var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{boot},assetBundleManifestPath=Path.Combine(bundleRoot,Path.GetFileName(bundleRoot)+".manifest"),target=target,locationPathName=output,options=options|BuildOptions.DetailedBuildReport});
   Debug.Log("PACKAGED_CONTENT_BUILD bytes="+catalog.bytes+" sha256="+catalog.sha256);
   return report;
  }finally{PayloadDirectory=null;if(originalSetup.Any(s=>s.isLoaded&&s.isActive))EditorSceneManager.RestoreSceneManagerSetup(originalSetup);else EditorSceneManager.OpenScene(sources[0]);}
 }
}
public sealed class PackagedContentStreamingAssets:BuildPlayerProcessor {
 public override void PrepareForBuild(BuildPlayerContext context){if(PackagedContentBuild.PayloadDirectory!=null)context.AddAdditionalPathToStreamingAssets(PackagedContentBuild.PayloadDirectory,"PackagedContent");}
}
