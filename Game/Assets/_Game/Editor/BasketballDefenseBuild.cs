using UnityEditor;
using UnityEditor.SceneManagement;
namespace WhatTheFish.Editor {
 public static class BasketballDefenseBuild {
  public static void Windows(){AssetDatabase.Refresh();EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");ProjectBuilder.BuildWindowsPlayer("../Builds/BasketballDefenseQA/Player/WhatTheFish.exe");}
  public static void Scripts(){
   AssetDatabase.Refresh();
   var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=SkySailBuilder.BuildScenes(),locationPathName="../Builds/BasketballDefenseQA/Player/WhatTheFish.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development|BuildOptions.BuildScriptsOnly|BuildOptions.CompressWithLz4HC});
   if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Defense script build failed: "+report.summary.result);
  }
 }
}
