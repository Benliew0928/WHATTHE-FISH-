#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
public static class RoomTeleportBuild {
 public static void Prepare(){
  TeleportBuild.Prepare();var lamp=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/SkySail/Materials/SS_Lamp.mat");
  if(!lamp)throw new System.Exception("Cabin lamp material missing.");
  SkySailBuilder.ConfigureLamp(lamp);AssetDatabase.SaveAssets();
 }
 public static void Build(){Prepare();ProjectBuilder.BuildWindowsPlayer("../Builds/RoomTeleportQA/Player/WhatTheFish.exe");}
}
#endif
