#if UNITY_EDITOR
namespace WhatTheFish.Editor {
 public static class FirstVisitBuild {
  public static void Scripts(){var report=PackagedContentBuild.BuildScripts(UnityEditor.BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/FirstVisitQA/Player/WhatTheFish.exe"),UnityEditor.BuildOptions.Development|UnityEditor.BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Guide scripts build failed.");}
  public static void Audio()=>ProjectBuilder.BuildWindowsPlayer("../Builds/FirstVisitAudioQA/Player/WhatTheFish.exe");
  public static void Build()=>ProjectBuilder.BuildWindowsPlayer("../Builds/FirstVisitQA/Player/WhatTheFish.exe");
 }
}
#endif
