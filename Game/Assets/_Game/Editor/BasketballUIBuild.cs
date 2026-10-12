using System;
using UnityEditor;
using UnityEngine;

public static class BasketballUIBuild {
 public static void Prepare(){
  AssetDatabase.Refresh();const string path="Assets/_Game/Resources/BasketballUI/Icons.png";
  var t=AssetImporter.GetAtPath(path) as TextureImporter;if(!t)throw new Exception("Run Tools/Build/Generate-BasketballUI.cjs first.");
  t.textureType=TextureImporterType.Default;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.isReadable=false;t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;t.maxTextureSize=512;t.textureCompression=TextureImporterCompression.Compressed;
  t.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=512,format=TextureImporterFormat.ASTC_6x6,compressionQuality=70});t.SaveAndReimport();
  Debug.Log("BASKETBALL_UI_IMPORT_OK");
 }
 public static void BuildInboundReview(){Prepare();ProjectBuilder.BuildWindowsPlayer("../Builds/BasketballInboundQA/Player/WhatTheFish.exe");}
 public static void BuildReview(){Prepare();ProjectBuilder.BuildWindowsPlayer("../Builds/BasketballUIQA/Player/WhatTheFish.exe");}
}
