#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace WhatTheFish.Editor {
 public static class RoomChatBuild {
  public static void Build()=>ProjectBuilder.BuildWindowsPlayer("../Builds/RoomChatQA/Player/WhatTheFish.exe");
  public static void Scripts(){
   var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/RoomChatQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);
   if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Room chat scripts build failed: "+report.summary.result);
  }
 }
 // Approved full-resolution masters remain editable in Docs/ChatPrototype.
 // Unity derives a crisp 256px sprite; no mip chains or readable CPU copy.
 public sealed class RoomChatTextureImport:AssetPostprocessor {
  void OnPreprocessTexture(){
   if(!assetPath.StartsWith("Assets/_Game/Resources/RoomChat/"))return;
   var importer=(TextureImporter)assetImporter;importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
   importer.spritePixelsPerUnit=100;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.isReadable=false;importer.maxTextureSize=256;importer.npotScale=TextureImporterNPOTScale.None;
   importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.textureCompression=TextureImporterCompression.Uncompressed;
   var android=importer.GetPlatformTextureSettings("Android");android.overridden=true;android.maxTextureSize=256;android.format=TextureImporterFormat.ASTC_4x4;android.compressionQuality=100;importer.SetPlatformTextureSettings(android);
  }
 }
}
#endif
