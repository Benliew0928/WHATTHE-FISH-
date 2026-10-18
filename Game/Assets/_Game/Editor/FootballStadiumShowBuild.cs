using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public sealed class FootballStadiumShowBuild:IPreprocessBuildWithReport {
 public int callbackOrder=>-110;
 public void OnPreprocessBuild(BuildReport report){Prepare();}
 public static void Prepare(){
  const string path="Assets/_Game/Resources/FootballStadiumFireworks.mat";
  var shader=Shader.Find("WhatTheFish/StadiumFireworks");if(!shader)throw new Exception("Missing stadium fireworks shader.");
  if(!AssetDatabase.LoadAssetAtPath<Material>(path)){AssetDatabase.CreateAsset(new Material(shader){name="FootballStadiumFireworks"},path);AssetDatabase.SaveAssets();}
  const string framePath="Assets/_Game/Resources/FootballStadiumScreenFrame.mat";
  if(!AssetDatabase.LoadAssetAtPath<Material>(framePath)){var frame=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="FootballStadiumScreenFrame"};frame.SetColor("_BaseColor",new Color(.12f,.28f,.29f));frame.SetFloat("_Metallic",.25f);frame.SetFloat("_Smoothness",.4f);AssetDatabase.CreateAsset(frame,framePath);AssetDatabase.SaveAssets();}
 }
 public static void Windows(){Prepare();FootballMotionBuilder.Prepare();ProjectBuilder.BuildWindowsPlayer("../Builds/FootballStadiumShowQA/Player/WhatTheFish.exe");}
 public static void WindowsBroadcast(){Prepare();FootballMotionBuilder.Prepare();ProjectBuilder.BuildWindowsPlayer("../Builds/FootballBroadcastQA/Player/WhatTheFish.exe");}
 public static void Scripts()=>ScriptsAt("../Builds/FootballStadiumShowQA");
 public static void ScriptsBroadcast()=>ScriptsAt("../Builds/FootballBroadcastQA");
 public static void WindowsEffects(){Prepare();FootballMotionBuilder.Prepare();ProjectBuilder.BuildWindowsPlayer("../Builds/FootballStadiumEffectsQA/Player/WhatTheFish.exe");}
 public static void ScriptsEffects()=>ScriptsAt("../Builds/FootballStadiumEffectsQA");
 static void ScriptsAt(string folder){
  var root=ProjectBuilder.ResolveBuildOutputPath(folder);var player=Path.Combine(root,"Player/WhatTheFish.exe");if(!File.Exists(player))throw new Exception("Build the complete stadium-show player first.");
  var content=Path.Combine(root,"Player/WhatTheFish_Data/StreamingAssets/PackagedContent");var backup=Path.Combine(root,"ScriptPayload");Directory.CreateDirectory(backup);
  foreach(var file in Directory.GetFiles(content))File.Copy(file,Path.Combine(backup,Path.GetFileName(file)),true);
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/_Game/GeneratedPackaging/PackagedBoot.unity"},assetBundleManifestPath=ProjectBuilder.ResolveBuildOutputPath("../Builds/PackagedContent/StandaloneWindows64/StandaloneWindows64.manifest"),locationPathName=player,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development|BuildOptions.BuildScriptsOnly|BuildOptions.CompressWithLz4HC});
  Directory.CreateDirectory(content);foreach(var file in Directory.GetFiles(backup))File.Copy(file,Path.Combine(content,Path.GetFileName(file)),true);
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Stadium show scripts failed: "+report.summary.result);
 }
}
