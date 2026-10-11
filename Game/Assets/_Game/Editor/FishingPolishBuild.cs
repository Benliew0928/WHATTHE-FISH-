using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System;
using System.IO;
using System.Linq;

public static class FishingPolishBuild {
 // Reuse the saved, integrated scenes without regenerating island authoring.
 public static void Windows() {
  string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/FishingPolishQA/Player/WhatTheFish.exe"));
  ProjectBuilder.BuildWindowsPlayer(output);
  Debug.Log("FISHING_POLISH_CANDIDATE_BUILT");
 }
 // A complete candidate must already exist. Only nonserialized code changed
 // since its full build; retain all player data and replace compiled scripts together.
 public static void WindowsScripts() {
  string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/FishingPolishQA/Player/WhatTheFish.exe"));
  if(!File.Exists(output)||!Directory.Exists(Path.Combine(Path.GetDirectoryName(output),"WhatTheFish_Data")))throw new InvalidOperationException("Build the complete candidate with Windows first.");
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development|BuildOptions.CompressWithLz4HC|BuildOptions.BuildScriptsOnly});
  if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Fishing script refresh failed: "+report.summary.result);
  Debug.Log("FISHING_POLISH_SCRIPTS_REFRESHED");
 }
}
