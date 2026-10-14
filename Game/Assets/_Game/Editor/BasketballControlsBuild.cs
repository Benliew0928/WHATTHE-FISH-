public static class BasketballControlsBuild {
 public static void BuildReview(){RoomMatchBuild.CheckRules();ProjectBuilder.BuildWindowsPlayer("../Builds/BasketballControlQA/Player/WhatTheFish.exe");}
 public static void BuildDelivery(){RoomMatchBuild.CheckRules();ProjectBuilder.BuildWindowsPlayer("../Builds/BasketballControlQA/Release/WhatTheFish.exe");}
 public static void Scripts(){
  UnityEditor.AssetDatabase.Refresh();RoomMatchBuild.CheckRules();
  var result=UnityEditor.BuildPipeline.BuildPlayer(new UnityEditor.BuildPlayerOptions{scenes=SkySailBuilder.BuildScenes(),locationPathName="../Builds/BasketballControlQA/Player/WhatTheFish.exe",target=UnityEditor.BuildTarget.StandaloneWindows64,options=UnityEditor.BuildOptions.Development|UnityEditor.BuildOptions.BuildScriptsOnly|UnityEditor.BuildOptions.CompressWithLz4HC});
  if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Control script build failed");
 }
}
