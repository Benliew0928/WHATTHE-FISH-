using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using WhatTheFish;

public static class MobileOptimizationValidation {
 [Serializable] sealed class References {public Sample[] samples;}
 [Serializable] sealed class Sample {public string sport;public double x,z,height;}
 public static void ValidateTerrain(){
  Directory.CreateDirectory("../Builds/MobileOptimization");
  var reference=JsonUtility.FromJson<References>(File.ReadAllText("../Tools/Blender/Fixtures/terrain-reference.json"));
  double maximum=0;
  foreach(var sample in reference.samples)maximum=Math.Max(maximum,Math.Abs(sample.height-MobileIslandGrass.Height(sample.x,sample.z,sample.sport=="Golf")));
  if(reference.samples.Length!=13122||maximum>1e-8)throw new Exception("Runtime grass terrain mismatch: "+maximum);
  File.WriteAllText("../Builds/MobileOptimization/terrain-validation.txt",$"PASS {reference.samples.Length} independent authoring samples; maximum error {maximum:R} metres\n");
  Debug.Log("MOBILE_TERRAIN_VALIDATED samples="+reference.samples.Length+" maxError="+maximum);
 }
 public static void BuildPreview(){
  ValidateTerrain();
  MobileMaterialBuilder.Prepare();
  string pipeline=AssetDatabase.GetAssetPath(GraphicsSettings.defaultRenderPipeline),quality=AssetDatabase.GetAssetPath(QualitySettings.renderPipeline);
  string previous=Environment.GetEnvironmentVariable("WTF_MOBILE_PREVIEW");
  try{
   Environment.SetEnvironmentVariable("WTF_MOBILE_PREVIEW","1");
   var desktop=AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(MobilePackageCleanup.DesktopPipelinePath);
   GraphicsSettings.defaultRenderPipeline=desktop;QualitySettings.renderPipeline=desktop;
   var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=SkySailBuilder.BuildScenes(),target=BuildTarget.StandaloneWindows64,locationPathName="../Builds/WindowsMobilePreview/WhatTheFish.exe",options=BuildOptions.Development|BuildOptions.CompressWithLz4HC});
   if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Mobile geometry preview build failed");
  }finally{
   Environment.SetEnvironmentVariable("WTF_MOBILE_PREVIEW",previous);
   GraphicsSettings.defaultRenderPipeline=AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(pipeline);QualitySettings.renderPipeline=AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(quality);AssetDatabase.SaveAssets();
  }
 }
}
