using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace WhatTheFish.Editor {
 public static class PlayerPortraitBuild {
  public static void Windows(){AssetDatabase.Refresh();Validate();ProjectBuilder.BuildWindowsPlayer("../Builds/ProfileStyleQA/Player/WhatTheFish.exe");}
  // Use only with a complete current Windows player, unchanged assets and serialized layouts.
  public static void Scripts(){
   var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,
    ProjectBuilder.ResolveBuildOutputPath("../Builds/ProfileStyleQA/Player/WhatTheFish.exe"),
    BuildOptions.Development|BuildOptions.CompressWithLz4HC);
   if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new BuildFailedException("Player hub scripts build failed.");
  }
  public static void Validate(){
   for(int i=0;i<PlayerProfiles.AvatarCount;i++){
    string path="Assets/_Game/Resources/PlayerPortraits/Character-"+(i+1).ToString("00")+".png";
    // Reimport also applies the portrait settings to resources restored before this importer existed.
    AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
    var t=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    if(!t||t.width!=1024||t.height!=1536)throw new BuildFailedException("Missing or resized character portrait: "+path);
   }
   if(!PlayerHubStyle.Display)throw new BuildFailedException("Player hub display font missing.");
   Debug.Log("PLAYER_PORTRAITS_VALIDATED count=10 size=1024x1536");
  }
 }
 public sealed class PlayerPortraitImport:AssetPostprocessor {
  void OnPreprocessTexture(){
   if(!assetPath.StartsWith("Assets/_Game/Resources/PlayerPortraits/"))return;
   var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.alphaIsTransparency=true;
   t.mipmapEnabled=false;t.npotScale=TextureImporterNPOTScale.None;t.wrapMode=TextureWrapMode.Clamp;
   t.filterMode=FilterMode.Bilinear;t.maxTextureSize=2048;t.isReadable=false;t.sRGBTexture=true;
   t.textureCompression=TextureImporterCompression.Uncompressed;
   t.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=2048,format=TextureImporterFormat.ASTC_4x4,compressionQuality=100});
  }
 }
}
