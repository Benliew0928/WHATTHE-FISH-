using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WhatTheFish.Editor {
 public static class StartupWelcomeBuild {
  public static void LoadingUtilities()=>ProjectBuilder.BuildWindowsPlayer("../Builds/WelcomeLoadingQA/Player/WhatTheFish.exe");
  public static void Windows(){
   AssetDatabase.Refresh();Validate();EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");
   ProjectBuilder.BuildWindowsPlayer("../Builds/StartupWelcomeQA/Player/WhatTheFish.exe");
  }
  public static void Validate(){
   foreach(var name in new[]{"Scene","Mascot"}){
    var path="Assets/_Game/Resources/StartupWelcome/"+name+".png";
    var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
    if(!texture||texture.width<1000||texture.height<700||!importer||importer.mipmapEnabled||importer.npotScale!=TextureImporterNPOTScale.None||importer.wrapMode!=TextureWrapMode.Clamp)throw new BuildFailedException("Sharp welcome layer is missing or resampled: "+path);
    if(name=="Mascot"&&!importer.DoesSourceTextureHaveAlpha())throw new BuildFailedException("Welcome mascot must retain transparent alpha.");
   }
   if(!Resources.Load<Font>("Menu/CoveDisplay")||!Resources.Load<Font>("Menu/CoveText"))throw new BuildFailedException("Startup text fonts are missing.");
   Debug.Log("STARTUP_WELCOME_ASSETS_VALIDATED sharp scenery + transparent mascot + original logo");
  }
 }
 public sealed class StartupWelcomeTextureImport:AssetPostprocessor {
  void OnPreprocessTexture(){
   bool mascot=assetPath=="Assets/_Game/Resources/StartupWelcome/Mascot.png";
   if(assetPath!="Assets/_Game/Resources/StartupWelcome/Scene.png"&&!mascot)return;
   var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.alphaSource=mascot?TextureImporterAlphaSource.FromInput:TextureImporterAlphaSource.None;t.alphaIsTransparency=mascot;t.mipmapEnabled=false;t.npotScale=TextureImporterNPOTScale.None;t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;t.maxTextureSize=2048;t.isReadable=false;t.sRGBTexture=true;t.textureCompression=TextureImporterCompression.CompressedHQ;
   t.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=2048,format=TextureImporterFormat.ASTC_4x4,compressionQuality=100});
  }
 }
}
