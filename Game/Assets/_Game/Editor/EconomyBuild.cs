using UnityEditor;
public static class EconomyBuild {
 [MenuItem("WHATTHE FISH?/Build coin verification player")]
 public static void Windows()=>ProjectBuilder.BuildWindowsPlayer("../Builds/StoreQA/Player/WhatTheFish.exe");
 public static void Scripts(){var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/StoreQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new UnityEditor.Build.BuildFailedException("Coin verification compilation failed.");}
}
