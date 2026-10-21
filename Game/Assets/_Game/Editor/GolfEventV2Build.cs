using System;
using UnityEditor;
using UnityEngine;
public static class GolfEventV2Build {
 public static void Windows()=>ProjectBuilder.BuildWindowsPlayer("../Builds/GolfEventV2QA/Player/WhatTheFish.exe");
 public static void Refresh(){var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/GolfEventV2QA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Event V2 refresh failed");}
}
