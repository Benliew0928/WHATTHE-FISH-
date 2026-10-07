using UnityEditor;
using UnityEditor.SceneManagement;
namespace WhatTheFish.Editor {
 public static class BasketballPassBuild {
  const string Player="../Builds/BasketballPassQA/Player/WhatTheFish.exe";
  public static void Windows(){AssetDatabase.Refresh();EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");ProjectBuilder.BuildWindowsPlayer(Player);}
  public static void Trajectory(){AssetDatabase.Refresh();EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");ProjectBuilder.BuildWindowsPlayer("../Builds/BasketballTrajectoryQA/Player/WhatTheFish.exe");}
  public static void TrajectoryScripts()=>BuildScripts("../Builds/BasketballTrajectoryQA/Player/WhatTheFish.exe");
  public static void Scripts()=>BuildScripts(Player);
  public static void DeliveryScripts()=>BuildScripts("../Builds/WindowsFinal/WhatTheFish.exe");
  static void BuildScripts(string player){
   AssetDatabase.Refresh();var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=SkySailBuilder.BuildScenes(),locationPathName=player,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development|BuildOptions.BuildScriptsOnly|BuildOptions.CompressWithLz4HC});
   if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Pass script build failed: "+report.summary.result);
  }
 }
}
