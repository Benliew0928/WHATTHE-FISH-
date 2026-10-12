using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public sealed class FootballAudioImporter:AssetPostprocessor {
 void OnPreprocessAudio(){if(assetPath.StartsWith("Assets/_Game/Resources/FootballAudio/",StringComparison.Ordinal))FootballAudioBuild.Configure((AudioImporter)assetImporter);}
}
public sealed class FootballAudioBuild:IPreprocessBuildWithReport {
 public int callbackOrder=>-99;
 public static void Configure(AudioImporter importer){
  BasketballAudioBuild.Configure(importer);
  var s=importer.defaultSampleSettings;s.quality=.65f;s.loadType=Path.GetFileName(importer.assetPath).StartsWith("turf-roll-")?AudioClipLoadType.CompressedInMemory:AudioClipLoadType.DecompressOnLoad;importer.defaultSampleSettings=s;
 }
 public static void Prepare(){
  AssetDatabase.Refresh();var guids=AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/_Game/Resources/FootballAudio"});
  if(guids.Length!=55)throw new BuildFailedException("Expected 55 new approved football clips; run Tools/Audio/publish_football_audio.py");
  foreach(var guid in guids){var importer=(AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));Configure(importer);importer.SaveAndReimport();}
  Debug.Log("FOOTBALL_AUDIO_IMPORT_OK new=55 shared=25");
 }
 public void OnPreprocessBuild(BuildReport report)=>Prepare();
 public static void BuildReview(){Prepare();ProjectBuilder.BuildWindowsPlayer("../Builds/FootballAudioQA/Player/WhatTheFish.exe");}
}
