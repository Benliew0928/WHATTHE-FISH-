#if UNITY_EDITOR
using UnityEditor;
namespace WhatTheFish.Editor {
 public static class PlayerIdentityCameraBuild {
  const string Output="../Builds/PlayerIdentityCameraQA/Player/WhatTheFish.exe";
  public static void Build()=>ProjectBuilder.BuildWindowsPlayer(Output);
  public static void Scripts(){var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath(Output),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Identity/camera build failed");}
 }
}
#endif
