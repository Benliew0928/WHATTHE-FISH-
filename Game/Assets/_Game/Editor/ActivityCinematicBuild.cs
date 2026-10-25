#if UNITY_EDITOR
using UnityEditor;
namespace WhatTheFish.Editor {
 public static class ActivityCinematicBuild {
  const string Output="../Builds/ActivityCinematicsQA/Player/WhatTheFish.exe";
  public static void Build()=>ProjectBuilder.BuildWindowsPlayer(Output);
  public static void Scripts(){var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath(Output),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Activity cinematic scripts build failed");}
 }
}
#endif
