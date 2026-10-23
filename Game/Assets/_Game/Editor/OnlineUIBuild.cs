#if UNITY_EDITOR
namespace WhatTheFish.Editor {
 public static class OnlineUIBuild {
  public static void Build()=>ProjectBuilder.BuildWindowsPlayer("../Builds/OnlineUIQA/Player/WhatTheFish.exe");
  // Only after Build: retain its complete scene bundle, runtime and launchers.
  public static void Scripts(){
   var report=UnityEditor.BuildPipeline.BuildPlayer(new UnityEditor.BuildPlayerOptions{scenes=new[]{"Assets/_Game/GeneratedPackaging/PackagedBoot.unity"},assetBundleManifestPath=ProjectBuilder.ResolveBuildOutputPath("../Builds/PackagedContent/StandaloneWindows64/StandaloneWindows64.manifest"),locationPathName=ProjectBuilder.ResolveBuildOutputPath("../Builds/OnlineUIQA/Player/WhatTheFish.exe"),target=UnityEditor.BuildTarget.StandaloneWindows64,options=UnityEditor.BuildOptions.Development|UnityEditor.BuildOptions.BuildScriptsOnly|UnityEditor.BuildOptions.CompressWithLz4HC});
   if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Online UI scripts build failed: "+report.summary.result);
   var payload=ProjectBuilder.ResolveBuildOutputPath("../Builds/PackagedContent/StandaloneWindows64/Payload");var destination=ProjectBuilder.ResolveBuildOutputPath("../Builds/OnlineUIQA/Player/WhatTheFish_Data/StreamingAssets/PackagedContent");System.IO.Directory.CreateDirectory(destination);
   foreach(var file in System.IO.Directory.GetFiles(payload))System.IO.File.Copy(file,System.IO.Path.Combine(destination,System.IO.Path.GetFileName(file)),true);
  }
 }
}
#endif
