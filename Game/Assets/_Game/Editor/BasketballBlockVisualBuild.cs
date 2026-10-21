#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
namespace WhatTheFish.Editor {
 public static class BasketballBlockVisualBuild {
  public static void Build()=>ProjectBuilder.BuildWindowsPlayer("../Builds/BlockVisualQA/Player/WhatTheFish.exe");
  public static void Scripts(){
   var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/BlockVisualQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);
   if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Block visual script build failed.");
  }
 }
}
#endif
