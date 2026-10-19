using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public sealed class BasketballAudioImporter:AssetPostprocessor {
 void OnPreprocessAudio(){if(assetPath.StartsWith("Assets/_Game/Resources/BasketballAudio/",StringComparison.Ordinal))BasketballAudioBuild.Configure((AudioImporter)assetImporter);}
}
public sealed class BasketballAudioBuild:IPreprocessBuildWithReport {
 public int callbackOrder=>-100;
 public static void Configure(AudioImporter importer){
  importer.forceToMono=true;importer.ambisonic=false;importer.loadInBackground=false;
  // Preserve the approved relative levels, especially the deliberately quiet aim.
  var serialized=new SerializedObject(importer);var normalize=serialized.FindProperty("m_Normalize");
  if(normalize==null)throw new BuildFailedException("Cannot verify audio normalization setting");normalize.boolValue=false;serialized.ApplyModifiedPropertiesWithoutUndo();
  bool loop=Path.GetFileName(importer.assetPath).StartsWith("air-")||Path.GetFileName(importer.assetPath).StartsWith("turbo-loop-")||Path.GetFileName(importer.assetPath).StartsWith("roll-");
  var settings=importer.defaultSampleSettings;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.65f;settings.sampleRateSetting=AudioSampleRateSetting.OverrideSampleRate;settings.sampleRateOverride=32000;settings.loadType=loop?AudioClipLoadType.CompressedInMemory:AudioClipLoadType.DecompressOnLoad;settings.preloadAudioData=true;importer.defaultSampleSettings=settings;
 }
 public static void Prepare(){
  AssetDatabase.Refresh();var guids=AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/_Game/Resources/BasketballAudio"});
  if(guids.Length!=58)throw new BuildFailedException("Expected 58 approved basketball clips; run Tools/Audio/publish_basketball_audio.py");
  foreach(var guid in guids){var path=AssetDatabase.GUIDToAssetPath(guid);var importer=(AudioImporter)AssetImporter.GetAtPath(path);Configure(importer);importer.SaveAndReimport();}
  var aim=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Resources/BasketballAudio/lock-aim-01.wav");if(!aim||Mathf.Abs(aim.length-2)>.005f)throw new BuildFailedException("Lock-on aim must remain two seconds");
  Debug.Log("BASKETBALL_AUDIO_IMPORT_OK clips="+guids.Length);
 }
 public void OnPreprocessBuild(BuildReport report){Prepare();}
 public static void BuildReview(){Prepare();ProjectBuilder.BuildWindowsPlayer("../Builds/BasketballAudioQA/Player/WhatTheFish.exe");}
 // Only for code changes after a full review build; retain its scene payload.
 public static void BuildReviewScripts(){var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/BasketballAudioQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=BuildResult.Succeeded)throw new BuildFailedException("Audio review script refresh failed");}
}
