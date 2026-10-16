using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WhatTheFish.Editor {
 public static class StartupWelcomeBuild {
  public const string Background="Assets/_Game/Resources/StartupWelcome/Background.png";
  public static void Windows(){
   AssetDatabase.Refresh();Validate();EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");
   ProjectBuilder.BuildWindowsPlayer("../Builds/StartupWelcomeQA/Player/WhatTheFish.exe");
  }
  public static void Validate(){
   var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Background);
   if(!texture||texture.width<1600||texture.height<900)throw new BuildFailedException("Startup background must retain the approved scene at delivery resolution.");
   var importer=(TextureImporter)AssetImporter.GetAtPath(Background);
   if(importer.mipmapEnabled||importer.npotScale!=TextureImporterNPOTScale.None||importer.wrapMode!=TextureWrapMode.Clamp)throw new BuildFailedException("Startup background import must preserve full composition and UI edges.");
   if(!Resources.Load<Font>("Menu/CoveDisplay")||!Resources.Load<Font>("Menu/CoveText"))throw new BuildFailedException("Startup text fonts are missing.");
   Debug.Log("STARTUP_WELCOME_ASSETS_VALIDATED "+texture.width+"x"+texture.height);
  }
 }
 public sealed class StartupWelcomeTextureImport:AssetPostprocessor {
  void OnPreprocessTexture(){
   if(assetPath!=StartupWelcomeBuild.Background)return;
   var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.alphaSource=TextureImporterAlphaSource.None;t.mipmapEnabled=false;t.npotScale=TextureImporterNPOTScale.None;t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;t.maxTextureSize=2048;t.isReadable=false;t.sRGBTexture=true;t.textureCompression=TextureImporterCompression.CompressedHQ;
   t.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=2048,format=TextureImporterFormat.ASTC_4x4,compressionQuality=100});
  }
 }
}
