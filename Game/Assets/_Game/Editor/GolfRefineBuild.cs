using System;
using UnityEditor;
using UnityEngine;
public static class GolfRefineBuild {
 public static void Prepare(){var mesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/_Game/Art/Golf/FiveHoleCourse/Meshes/Shared/Pennant.asset");var bounds=mesh.bounds;var extents=bounds.extents;extents.z=Mathf.Max(extents.z,.36f);bounds.extents=extents;mesh.bounds=bounds;EditorUtility.SetDirty(mesh);AssetDatabase.SaveAssets();}
 public static void Build(){Prepare();ProjectBuilder.BuildWindowsPlayer("../Builds/GolfRefineQA/Player/WhatTheFish.exe");}
 public static void BuildRankingRollback(){ProjectBuilder.BuildWindowsPlayer("../Builds/GolfRankingRollbackQA/Player/WhatTheFish.exe");}
 public static void RefreshPersonalRanking(){var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/GolfPersonalRankingQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Personal ranking script refresh failed");}
 public static void RefreshRankingRollback(){var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/GolfRankingRollbackQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Rollback script refresh failed");}
 public static void RefreshScripts(){var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/GolfRefineQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Refinement script refresh failed");}
}
