#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
namespace WhatTheFish.Editor {
 public static class TutorialBuild {
  static string Output=>System.Environment.GetEnvironmentVariable("WTF_TUTORIAL_PLAYER")??"../Builds/TutorialQA/Player/WhatTheFish.exe";
  public static void Build()=>ProjectBuilder.BuildWindowsPlayer(Output);
  public static void Scripts(){
   var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath(Output),BuildOptions.Development|BuildOptions.CompressWithLz4HC);
   if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Tutorial script build failed.");
  }
 }
}
#endif
