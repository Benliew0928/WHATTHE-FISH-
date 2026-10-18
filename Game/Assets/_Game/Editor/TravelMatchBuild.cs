#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
public static class TravelMatchBuild {
 public static void Build(){EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");ProjectBuilder.BuildWindowsPlayer("../Builds/TravelMatchQA/Player/WhatTheFish.exe");}
 public static void Scripts(){var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/TravelMatchQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Travel match script build failed.");}
}
#endif
