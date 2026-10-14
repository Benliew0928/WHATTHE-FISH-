using System;
using UnityEditor;
using UnityEngine;

public static class CharacterLightingBuild {
 public static void BuildReview(){
  foreach(string path in SkySailBuilder.BuildScenes())
   if(Array.Exists(AssetDatabase.GetDependencies(path),p=>p=="Assets/_Game/Settings/DesktopCoastRenderer.asset"))
    throw new InvalidOperationException("Scene still embeds desktop renderer features: "+path);
  ProjectBuilder.BuildWindowsPlayer("../Builds/CharacterLightingQA/Player/WhatTheFish.exe");
  Debug.Log("CHARACTER_LIGHTING_BUILD_COMPLETE");
 }
}
