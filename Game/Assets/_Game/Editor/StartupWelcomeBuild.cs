using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WhatTheFish.Editor {
 public static class StartupWelcomeBuild {
  public const string Background="Assets/_Game/Resources/StartupWelcome/Background.png";
  public const string BlankSign="Assets/_Game/Resources/StartupWelcome/BlankSign.png";
  public static void Windows(){
   AssetDatabase.Refresh();Validate();EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");
   ProjectBuilder.BuildWindowsPlayer("../Builds/StartupWelcomeQA/Player/WhatTheFish.exe");
  }
  public static void Validate(){
   foreach(var path in new[]{Background,BlankSign}){
    var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    int expectedWidth=path==Background?1672:1671;
    if(!texture||texture.width!=expectedWidth||texture.height!=941)throw new BuildFailedException("Startup artwork must retain its recorded master dimensions: "+path);
    var importer=(TextureImporter)AssetImporter.GetAtPath(path);
    if(importer.mipmapEnabled||importer.npotScale!=TextureImporterNPOTScale.None||importer.wrapMode!=TextureWrapMode.Clamp)throw new BuildFailedException("Startup artwork import must preserve full composition and UI edges: "+path);
   }
   if(!Resources.Load<Font>("Menu/CoveDisplay")||!Resources.Load<Font>("Menu/CoveText"))throw new BuildFailedException("Startup text fonts are missing.");
   Debug.Log("STARTUP_WELCOME_ASSETS_VALIDATED approved scene + isolated blank sign, 1672x941");
  }
 }
 public sealed class StartupWelcomeTextureImport:AssetPostprocessor {
  void OnPreprocessTexture(){
   if(assetPath!=StartupWelcomeBuild.Background&&assetPath!=StartupWelcomeBuild.BlankSign)return;
   var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.alphaSource=TextureImporterAlphaSource.None;t.mipmapEnabled=false;t.npotScale=TextureImporterNPOTScale.None;t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;t.maxTextureSize=2048;t.isReadable=false;t.sRGBTexture=true;t.textureCompression=TextureImporterCompression.CompressedHQ;
   t.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=2048,format=TextureImporterFormat.ASTC_4x4,compressionQuality=100});
  }
 }
}
