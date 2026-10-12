using UnityEditor;
using UnityEngine;

public static class SportsUIBuild {
 public static void Prepare(){
  BasketballUIBuild.Prepare();FootballUIBuild.Prepare();
  var t=(TextureImporter)AssetImporter.GetAtPath("Assets/_Game/Resources/SportsUI/Props.png");
  t.textureType=TextureImporterType.Default;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.isReadable=false;t.npotScale=TextureImporterNPOTScale.None;t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;t.maxTextureSize=512;t.textureCompression=TextureImporterCompression.Compressed;
  t.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=512,format=TextureImporterFormat.ASTC_6x6,compressionQuality=70});t.SaveAndReimport();
  foreach(var label in new[]{"Text","Display"}){
   var font=(TrueTypeFontImporter)AssetImporter.GetAtPath("Assets/_Game/Resources/SportsUI/"+label+".ttf");font.fontNames=new[]{"Cove Sport "+label};font.fontSize=16;font.fontRenderingMode=FontRenderingMode.Smooth;font.SaveAndReimport();
  }
  Debug.Log("SPORTS_UI_IMPORT_OK");
 }
 public static void BuildColourReview(){Prepare();ProjectBuilder.BuildWindowsPlayer("../Builds/ColorIconQA/Player/WhatTheFish.exe");}
 public static void BuildReview(){Prepare();ProjectBuilder.BuildWindowsPlayer("../Builds/SportsIconQA/Player/WhatTheFish.exe");}
}
