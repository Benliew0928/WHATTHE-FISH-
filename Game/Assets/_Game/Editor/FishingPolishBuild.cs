using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

public static class FishingPolishBuild {
 // Reuse the saved, integrated scenes without regenerating island authoring.
 public static void Windows() {
  Rules();
  string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/FishingIllustrated/Player/WhatTheFish.exe"));
  ProjectBuilder.BuildWindowsPlayer(output);
  Debug.Log("FISHING_POLISH_CANDIDATE_BUILT");
 }
 public static void Rules(){int failures=0;WhatTheFish.FishingRuleChecks.Run((pass,name)=>{if(!pass)failures++;Debug.Log((pass?"FISHING_RULE_PASS ":"FISHING_RULE_FAIL ")+name);});if(failures>0)throw new InvalidOperationException("Fishing rule failures: "+failures);}
 // A complete candidate must already exist. Only nonserialized code changed
 // since its full build; retain all player data and replace compiled scripts together.
 public static void WindowsScripts() {
  Rules();
  string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/FishingIllustrated/Player/WhatTheFish.exe"));
  // Refresh an explicitly selected complete review player. Relative overrides
  // resolve from the Unity project, independently of the caller's directory.
  var args=Environment.GetCommandLineArgs();int selected=Array.IndexOf(args,"-fishingReviewPlayer");
  if(selected>=0&&selected+1<args.Length)output=ProjectBuilder.ResolveBuildOutputPath(args[selected+1]);
  if(!File.Exists(output)||!Directory.Exists(Path.Combine(Path.GetDirectoryName(output),"WhatTheFish_Data")))throw new InvalidOperationException("Build the complete candidate with Windows first.");
  if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.StandaloneWindows64)throw new InvalidOperationException("A Windows full build must precede a code-only refresh.");
  string data=Path.Combine(Path.GetDirectoryName(output),"WhatTheFish_Data"),content=Path.Combine(data,"StreamingAssets/PackagedContent");
  bool packaged=File.Exists(Path.Combine(content,"catalog.json"));
  string boot="Assets/_Game/GeneratedPackaging/PackagedBoot.unity";
  string manifest=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/PackagedContent/StandaloneWindows64/StandaloneWindows64.manifest"));
  if(packaged&&(!AssetDatabase.LoadAssetAtPath<SceneAsset>(boot)||!File.Exists(manifest)))throw new InvalidOperationException("The full packaged build inputs are missing.");
  // BuildScriptsOnly must use the same entry scene and manifest as the full
  // player. The ordinary scene list describes the unpackaged development build.
  string[] protectedFiles=packaged?new[]{Path.Combine(data,"data.unity3d"),Path.Combine(content,"catalog.json"),Path.Combine(content,"islands.bundle")}:new[]{Path.Combine(data,"data.unity3d")};
  string[] hashes=protectedFiles.Select(Hash).ToArray();
  var options=BuildOptions.Development|BuildOptions.CompressWithLz4HC|BuildOptions.BuildScriptsOnly;
  var report=packaged?PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,output,options):BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName=output,target=BuildTarget.StandaloneWindows64,options=options});
  if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Fishing script refresh failed: "+report.summary.result);
  if(protectedFiles.Where((path,i)=>Hash(path)!=hashes[i]).Any())throw new InvalidOperationException("Code-only refresh changed native content. Rebuild the complete player before delivery.");
  File.AppendAllText(Path.Combine(Path.GetDirectoryName(output),"LATEST-BUILD.txt"),"\nScripts refreshed UTC: "+DateTime.UtcNow.ToString("o")+"; full native content/catalog/bundle hashes unchanged.\n");
  Debug.Log("FISHING_POLISH_SCRIPTS_REFRESHED");
 }
 static string Hash(string path){using var sha=SHA256.Create();using var stream=File.OpenRead(path);return BitConverter.ToString(sha.ComputeHash(stream));}
}
