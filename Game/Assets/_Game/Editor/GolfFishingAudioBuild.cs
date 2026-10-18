using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public sealed class GolfFishingAudioImporter:AssetPostprocessor {
 void OnPreprocessAudio(){if(assetPath.StartsWith("Assets/_Game/Resources/GolfFishingAudio/",StringComparison.Ordinal))GolfFishingAudioBuild.Configure((AudioImporter)assetImporter);}
}
public sealed class GolfFishingAudioBuild:IPreprocessBuildWithReport {
 public int callbackOrder=>-98;
 public static void Configure(AudioImporter importer){
  BasketballAudioBuild.Configure(importer);
  // These short loops need sample-exact joins. PCM also permits runtime signal
  // verification and avoids decoding 32 concurrent tiny compressed streams.
  var s=importer.defaultSampleSettings;s.compressionFormat=AudioCompressionFormat.PCM;s.loadType=AudioClipLoadType.DecompressOnLoad;importer.defaultSampleSettings=s;
 }
 public static void Prepare(){
  AssetDatabase.Refresh();var guids=AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/_Game/Resources/GolfFishingAudio"});
  if(guids.Length!=81)throw new BuildFailedException("Expected 81 approved Golf/Fishing clips; run the audio publisher.");
  foreach(var guid in guids){var importer=(AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));Configure(importer);importer.SaveAndReimport();}
  int count=0;foreach(var cue in WhatTheFish.GolfFishingAudio.Definitions)count+=WhatTheFish.GolfFishingAudio.Clips(cue.cue).Length;
  if(count!=115)throw new BuildFailedException("Approved bank must resolve all 115 clips.");
  Debug.Log("GOLF_FISHING_AUDIO_IMPORT_OK new=81 reused=34 total=115");
 }
 public void OnPreprocessBuild(BuildReport report)=>Prepare();
 public static void BuildReview(){Prepare();ProjectBuilder.BuildWindowsPlayer("../Builds/GolfFishingAudioQA/Player/WhatTheFish.exe");}
 public static void BuildScripts(){
  var path=ProjectBuilder.ResolveBuildOutputPath("../Builds/GolfFishingAudioQA/Player/WhatTheFish.exe");
  if(!System.IO.File.Exists(path))throw new BuildFailedException("Build the complete audio player first.");
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/_Game/GeneratedPackaging/PackagedBoot.unity"},assetBundleManifestPath=ProjectBuilder.ResolveBuildOutputPath("../Builds/PackagedContent/StandaloneWindows64/StandaloneWindows64.manifest"),locationPathName=path,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development|BuildOptions.BuildScriptsOnly|BuildOptions.CompressWithLz4HC});
  if(report.summary.result!=BuildResult.Succeeded)throw new BuildFailedException("Audio script build failed.");
  // Scripts-only builds refresh StreamingAssets; retain the matching full build's payload.
  var source=ProjectBuilder.ResolveBuildOutputPath("../Builds/PackagedContent/StandaloneWindows64/Payload");
  var destination=System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path),"WhatTheFish_Data/StreamingAssets/PackagedContent");System.IO.Directory.CreateDirectory(destination);
  foreach(var file in System.IO.Directory.GetFiles(source))System.IO.File.Copy(file,System.IO.Path.Combine(destination,System.IO.Path.GetFileName(file)),true);
 }
}
