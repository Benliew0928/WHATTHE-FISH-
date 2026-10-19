#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
public static class ResponsivenessBuild {
 public static void Build()=>ProjectBuilder.BuildWindowsPlayer("../Builds/ResponsivenessQA/Player/WhatTheFish.exe");
 public static void Scripts(){var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/ResponsivenessQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Responsiveness script build failed.");}
}
#endif
