#if UNITY_EDITOR
namespace WhatTheFish.Editor {
 public static class VenueCustomizationBuild {
  public static void Build()=>ProjectBuilder.BuildWindowsPlayer("../Builds/VenueCustomizationQA/Player/WhatTheFish.exe");
  public static void Scripts(){
   var report=PackagedContentBuild.BuildScripts(UnityEditor.BuildTarget.StandaloneWindows64,
    ProjectBuilder.ResolveBuildOutputPath("../Builds/VenueCustomizationQA/Player/WhatTheFish.exe"),
    UnityEditor.BuildOptions.Development|UnityEditor.BuildOptions.CompressWithLz4HC);
   if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Venue script build failed.");
  }
 }
}
#endif
