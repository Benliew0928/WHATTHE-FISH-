using UnityEditor;
using UnityEngine;

public static class FishingGameplayBuild {
 public static void BuildAimCandidate(){
  CheckRules();ProjectBuilder.BuildWindowsPlayer("../Builds/FishingAimQA/Player/WhatTheFish.exe");Debug.Log("FISHING_AIM_CANDIDATE_BUILT");
 }
 public static void BuildAimCode(){
  CheckRules();var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=SkySailBuilder.BuildScenes(),locationPathName="../Builds/FishingAimQA/Player/WhatTheFish.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development|BuildOptions.BuildScriptsOnly|BuildOptions.CompressWithLz4HC});
  if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Fishing aim code build failed.");Debug.Log("FISHING_AIM_CODE_BUILT");
 }
 static void CheckRules()=>WhatTheFish.FishingRuleChecks.Run((pass,name)=>{if(!pass)throw new System.Exception("FISHING_RULE_FAIL "+name);Debug.Log("FISHING_RULE_PASS "+name);});
 public static void BuildUXCode(){
  WhatTheFish.FishingRuleChecks.Run((pass,name)=>{if(!pass)throw new System.Exception("FISHING_RULE_FAIL "+name);Debug.Log("FISHING_RULE_PASS "+name);});
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=SkySailBuilder.BuildScenes(),locationPathName="../Builds/FishingUXQA/Player/WhatTheFish.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development|BuildOptions.BuildScriptsOnly|BuildOptions.CompressWithLz4HC});
  if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Fishing UX code build failed.");Debug.Log("FISHING_UX_CODE_BUILT");
 }
 public static void BuildUXCandidate(){
  foreach(var id in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/_Game/Resources/FishingUI"})){var path=AssetDatabase.GUIDToAssetPath(id);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);Debug.Log("FISHING_UI_IMPORT "+path+" "+texture.width+"x"+texture.height+" "+texture.format);}
  WhatTheFish.FishingRuleChecks.Run((pass,name)=>{if(!pass)throw new System.Exception("FISHING_RULE_FAIL "+name);Debug.Log("FISHING_RULE_PASS "+name);});ProjectBuilder.BuildWindowsPlayer("../Builds/FishingUXQA/Player/WhatTheFish.exe");Debug.Log("FISHING_UX_CANDIDATE_BUILT");
 }
 [MenuItem("WHATTHE FISH?/Fishing/Build gameplay review player")]
 public static void BuildCandidate(){ProjectBuilder.BuildWindowsPlayer("../Builds/FishingGameplayQA/Player/WhatTheFish.exe");Debug.Log("FISHING_CANDIDATE_BUILT");}
 public static void BuildCandidateCode(){
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=SkySailBuilder.BuildScenes(),locationPathName="../Builds/FishingGameplayQA/Player/WhatTheFish.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development|BuildOptions.BuildScriptsOnly|BuildOptions.CompressWithLz4HC});
  if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Fishing code build failed.");Debug.Log("FISHING_CODE_BUILT");
 }
}
