using UnityEditor;
using UnityEngine;

// Full player using the saved scenes; UI changes do not regenerate island art.
public static class GolfHUDReviewBuild {
 public static void RefreshScripts(){
  var report=UnityEditor.BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=SkySailBuilder.BuildScenes(),target=BuildTarget.StandaloneWindows64,locationPathName=System.Environment.GetEnvironmentVariable("WTF_GOLF_HUD_PLAYER")??"../Builds/GolfHUDQA/Player/WhatTheFish.exe",options=BuildOptions.Development|BuildOptions.BuildScriptsOnly|BuildOptions.CompressWithLz4HC});
  if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("HUD probe script refresh failed");
 }
 [MenuItem("WHATTHE FISH?/Golf/Build HUD review player")]
 public static void BuildCandidate(){
  foreach(var id in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/_Game/Resources/GolfUI"})){
   var path=AssetDatabase.GUIDToAssetPath(id);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
   var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);Debug.Log("GOLF_UI_IMPORT "+path+" "+texture.width+"x"+texture.height+" "+texture.format);
  }
  ProjectBuilder.BuildWindowsPlayer(System.Environment.GetEnvironmentVariable("WTF_GOLF_HUD_PLAYER")??"../Builds/GolfHUDQA/Player/WhatTheFish.exe");Debug.Log("GOLF_HUD_CANDIDATE_BUILT");
 }
}
