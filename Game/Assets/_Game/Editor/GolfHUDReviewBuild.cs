using UnityEditor;
using UnityEngine;

// Full player using the saved scenes; UI changes do not regenerate island art.
public static class GolfHUDReviewBuild {
 [MenuItem("WHATTHE FISH?/Golf/Build HUD review player")]
 public static void BuildCandidate(){
  foreach(var id in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/_Game/Resources/GolfUI"})){
   var path=AssetDatabase.GUIDToAssetPath(id);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
   var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);Debug.Log("GOLF_UI_IMPORT "+path+" "+texture.width+"x"+texture.height+" "+texture.format);
  }
  ProjectBuilder.BuildWindowsPlayer("../Builds/GolfHUDQA/Player/WhatTheFish.exe");Debug.Log("GOLF_HUD_CANDIDATE_BUILT");
 }
}
