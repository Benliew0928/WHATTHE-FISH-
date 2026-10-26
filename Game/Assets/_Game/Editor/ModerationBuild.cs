#if UNITY_EDITOR
namespace WhatTheFish.Editor {
 public static class ModerationBuild {
  public static void Build()=>ProjectBuilder.BuildWindowsPlayer("../Builds/ModerationQA/Player/WhatTheFish.exe");
  public static void Scripts(){
   var report=PackagedContentBuild.BuildScripts(UnityEditor.BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/ModerationQA/Player/WhatTheFish.exe"),UnityEditor.BuildOptions.Development|UnityEditor.BuildOptions.CompressWithLz4HC);
   if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Moderation player scripts build failed");
  }
 }
}
#endif
