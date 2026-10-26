#if UNITY_EDITOR
namespace WhatTheFish.Editor {
 public static class PublicInfoBuild {
  public static void Build()=>ProjectBuilder.BuildWindowsPlayer("../Builds/PublicInfoQA/Player/WhatTheFish.exe");
  // Code-only refresh after this task's full build. Serialized assets and type
  // layouts must remain unchanged when using this entry point.
  public static void Scripts(){
   var report=PackagedContentBuild.BuildScripts(UnityEditor.BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/PublicInfoQA/Player/WhatTheFish.exe"),UnityEditor.BuildOptions.Development|UnityEditor.BuildOptions.CompressWithLz4HC);
   if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Public Info review build failed");
  }
 }
}
#endif
