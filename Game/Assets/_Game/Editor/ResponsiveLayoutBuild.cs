#if UNITY_EDITOR
namespace WhatTheFish.Editor {
 public static class ResponsiveLayoutBuild {
  public static void Build(){UnityEditor.AssetDatabase.Refresh();StartupWelcomeBuild.Validate();ProjectBuilder.BuildWindowsPlayer("../Builds/ResponsiveLayoutQA/Player/WhatTheFish.exe");}
  public static void Scripts(){var report=PackagedContentBuild.BuildScripts(UnityEditor.BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/ResponsiveLayoutQA/Player/WhatTheFish.exe"),UnityEditor.BuildOptions.Development|UnityEditor.BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Responsive layout script build failed.");}
 }
}
#endif
