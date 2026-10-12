using System;
using UnityEditor;
using UnityEngine;
public static class FootballUIBuild {
 public static void Prepare(){
  AssetDatabase.Refresh();const string path="Assets/_Game/Resources/FootballUI/Icons.png";
  var t=AssetImporter.GetAtPath(path) as TextureImporter;if(!t)throw new Exception("Run Tools/Build/Generate-FootballUI.cjs first.");
  t.textureType=TextureImporterType.Default;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.isReadable=false;t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;t.maxTextureSize=512;t.textureCompression=TextureImporterCompression.Compressed;
  t.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=512,format=TextureImporterFormat.ASTC_6x6,compressionQuality=70});t.SaveAndReimport();Debug.Log("FOOTBALL_UI_IMPORT_OK");
 }
 public static void BuildReview(){Prepare();ProjectBuilder.BuildWindowsPlayer("../Builds/FootballUIQA/Player/WhatTheFish.exe");}
}
