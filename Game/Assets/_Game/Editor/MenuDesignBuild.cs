#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
namespace WhatTheFish.Editor {
 public static class MenuDesignBuild {
  public static void Build()=>ProjectBuilder.BuildWindowsPlayer("../Builds/MenuDesignQA/Player/WhatTheFish.exe");
  public static void Scripts(){var r=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/MenuDesignQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(r.summary.result!=BuildResult.Succeeded)throw new System.Exception("Menu design scripts build failed.");}
 }
}
#endif
