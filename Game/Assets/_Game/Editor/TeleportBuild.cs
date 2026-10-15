#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
public static class TeleportBuild {
 public static void Prepare(){
  foreach(string name in new[]{"TeleportCharacter","TeleportLight"}){
   string path="Assets/_Game/Resources/"+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(!material){var shader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Game/Shared/Player/"+name+".shader");if(!shader)throw new System.Exception("Missing teleport shader "+name);material=new Material(shader);AssetDatabase.CreateAsset(material,path);}
  }
  AssetDatabase.SaveAssets();AssetDatabase.Refresh();
 }
 public static void BuildReview(){Prepare();ProjectBuilder.BuildWindowsPlayer("../Builds/TeleportQA/Player/WhatTheFish.exe");foreach(string name in new[]{"TeleportCharacter","TeleportLight"})foreach(var message in ShaderUtil.GetShaderMessages(AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Game/Shared/Player/"+name+".shader")))if(message.severity.ToString()=="Error")throw new System.Exception(message.message);File.WriteAllText("../Builds/TeleportQA/Player/Preview-Teleport.cmd","@echo off\r\nstart \"\" \"%~dp0WhatTheFish.exe\" -offline -teleportPreview -screen-fullscreen 0 -screen-width 1600 -screen-height 900\r\n");}
}
#endif
