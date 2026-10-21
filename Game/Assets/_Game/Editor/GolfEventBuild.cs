using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class GolfEventBuild {
 public static void Windows()=>ProjectBuilder.BuildWindowsPlayer("../Builds/GolfEventsQA/Player/WhatTheFish.exe");
 public static void Refresh(){var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/GolfEventsQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Event probe refresh failed");}
 public static void Hotkeys(){var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/GolfEventHotkeyQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Golf hotkey player refresh failed");}
 public static void Check(){
  var output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/GolfEventsQA"));Directory.CreateDirectory(output);
  File.WriteAllText(Path.Combine(output,"compile.txt"),"Golf event runtime/editor compilation loaded successfully.\n");
  Debug.Log("GOLF_EVENTS_COMPILE_OK");
 }
}
