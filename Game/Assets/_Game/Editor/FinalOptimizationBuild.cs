#if UNITY_EDITOR
public static class FinalOptimizationBuild {
 public static void Windows(){WhatTheFish.Editor.StartupWelcomeBuild.Validate();ProjectBuilder.BuildWindowsPlayer("../Builds/FinalOptimizationQA/Player/WhatTheFish.exe");}
 public static void Scripts(){var report=PackagedContentBuild.BuildScripts(UnityEditor.BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/FinalOptimizationQA/Player/WhatTheFish.exe"),UnityEditor.BuildOptions.Development|UnityEditor.BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Optimization review script refresh failed.");}
}
#endif
