using UnityEditor;

public static class BasketballRestartBuild {
 public static void BuildReview(){AssetDatabase.Refresh();ProjectBuilder.BuildWindowsPlayer("../Builds/BasketballRestartQA/Player/WhatTheFish.exe");}
 public static void Scripts(){
  // Use only after BuildReview: keep its complete, matching scene bundle and
  // runtime beside the recompiled scripts. The manifest retains bundle types.
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/_Game/GeneratedPackaging/PackagedBoot.unity"},assetBundleManifestPath=ProjectBuilder.ResolveBuildOutputPath("../Builds/PackagedContent/StandaloneWindows64/StandaloneWindows64.manifest"),locationPathName=ProjectBuilder.ResolveBuildOutputPath("../Builds/BasketballRestartQA/Player/WhatTheFish.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development|BuildOptions.BuildScriptsOnly|BuildOptions.CompressWithLz4HC});
  if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Basketball restart scripts build failed: "+report.summary.result);
  // Unity rebuilds StreamingAssets even with BuildScriptsOnly; restore the
  // existing full build's content instead of regenerating or omitting it.
  var payload=ProjectBuilder.ResolveBuildOutputPath("../Builds/PackagedContent/StandaloneWindows64/Payload");var destination=ProjectBuilder.ResolveBuildOutputPath("../Builds/BasketballRestartQA/Player/WhatTheFish_Data/StreamingAssets/PackagedContent");System.IO.Directory.CreateDirectory(destination);
  foreach(var file in System.IO.Directory.GetFiles(payload))System.IO.File.Copy(file,System.IO.Path.Combine(destination,System.IO.Path.GetFileName(file)),true);
 }
}
