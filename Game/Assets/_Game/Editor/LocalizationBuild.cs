using UnityEditor;
public static class LocalizationBuild {
 [MenuItem("WHATTHE FISH?/Verify localization player")]
 public static void Windows()=>ProjectBuilder.BuildWindowsPlayer("../Builds/LocalizationQA/Player/WhatTheFish.exe");
 public static void Polish()=>ProjectBuilder.BuildWindowsPlayer("../Builds/LocalizationPolishQA/Player/WhatTheFish.exe");
 public static void PolishScripts(){
  var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/LocalizationPolishQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);
  if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new UnityEditor.Build.BuildFailedException("Localization code refresh failed.");
 }
 public static void Events()=>ProjectBuilder.BuildWindowsPlayer("../Builds/EventPresentationQA/Player/WhatTheFish.exe");
 public static void EventScripts(){var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/EventPresentationQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new UnityEditor.Build.BuildFailedException("Event presentation refresh failed.");}
 public static void Layout()=>ProjectBuilder.BuildWindowsPlayer("../Builds/LocalizationLayoutQA/Player/WhatTheFish.exe");
 public static void LayoutScripts(){var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/LocalizationLayoutQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new UnityEditor.Build.BuildFailedException("Layout code refresh failed.");}
 public static void Final()=>ProjectBuilder.BuildWindowsPlayer("../Builds/LocalizationQA/FinalPlayer/WhatTheFish.exe");
}
