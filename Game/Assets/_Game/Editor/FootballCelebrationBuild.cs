using System;
using System.IO;
using UnityEditor;
using WhatTheFish;
public static class FootballCelebrationBuild {
 public static void AuthorAndBuild(){
  var library=AssetDatabase.LoadAssetAtPath<FootballMotionLibrary>("Assets/_Game/Resources/FootballMotionLibrary.asset");var takes=new System.Collections.Generic.List<FootballTake>(library.takes);
  foreach(var take in FootballCelebrationPoses.Defaults()){int index=takes.FindIndex(t=>t.name==take.name);if(index<0)takes.Add(take);else takes[index]=take;}library.takes=takes.ToArray();EditorUtility.SetDirty(library);AssetDatabase.SaveAssets();BuildReview();
 }
 public static void BuildReview(){FootballMotionBuilder.Prepare();ProjectBuilder.BuildWindowsPlayer("../Builds/FootballCelebrationQA/Player/WhatTheFish.exe");}
 public static void Scripts(){
  // C# iteration only after a complete BuildReview. Preserve this player's
  // exact payload rather than whatever another task most recently generated.
  var root=ProjectBuilder.ResolveBuildOutputPath("../Builds/FootballCelebrationQA");var player=Path.Combine(root,"Player/WhatTheFish.exe");
  if(!File.Exists(player))throw new Exception("Build the complete celebration review player first.");
  var content=Path.Combine(root,"Player/WhatTheFish_Data/StreamingAssets/PackagedContent");var backup=Path.Combine(root,"ScriptPayload");Directory.CreateDirectory(backup);
  foreach(var file in Directory.GetFiles(content))File.Copy(file,Path.Combine(backup,Path.GetFileName(file)),true);
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/_Game/GeneratedPackaging/PackagedBoot.unity"},assetBundleManifestPath=ProjectBuilder.ResolveBuildOutputPath("../Builds/PackagedContent/StandaloneWindows64/StandaloneWindows64.manifest"),locationPathName=player,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development|BuildOptions.BuildScriptsOnly|BuildOptions.CompressWithLz4HC});
  Directory.CreateDirectory(content);foreach(var file in Directory.GetFiles(backup))File.Copy(file,Path.Combine(content,Path.GetFileName(file)),true);
  if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Celebration scripts build failed: "+report.summary.result);
 }
}
