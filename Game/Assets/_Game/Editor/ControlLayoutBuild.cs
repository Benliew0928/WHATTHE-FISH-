#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
namespace WhatTheFish.Editor {
 public static class ControlLayoutBuild {
  public static void Build()=>ProjectBuilder.BuildWindowsPlayer("../Builds/ControlLayoutQA/Player/WhatTheFish.exe");
  public static void Scripts(){
   var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/ControlLayoutQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);
   if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Control layout script build failed.");
  }
 }
}
#endif
