using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace WhatTheFish.Editor {
 public static class SportyMenuBuild {
  public static void Windows(){AssetDatabase.Refresh();Validate();ProjectBuilder.BuildWindowsPlayer("../Builds/SportyMenuQA/Player/WhatTheFish.exe");}
  public static void Scripts(){
   // Retain the full Windows build's validated assets while iterating only C#.
   // A normal Windows() build is required first; this is not a standalone delivery.
   var path=ProjectBuilder.ResolveBuildOutputPath("../Builds/SportyMenuQA/Player/WhatTheFish.exe");
   if(!System.IO.File.Exists(path))throw new BuildFailedException("Build the complete sports menu player first.");
   var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/_Game/GeneratedPackaging/PackagedBoot.unity"},assetBundleManifestPath=ProjectBuilder.ResolveBuildOutputPath("../Builds/PackagedContent/StandaloneWindows64/StandaloneWindows64.manifest"),locationPathName=path,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development|BuildOptions.BuildScriptsOnly|BuildOptions.CompressWithLz4HC});
   if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new BuildFailedException("Sports menu script build failed.");
   // Unity reconstructs StreamingAssets even for scripts-only builds.
   var source=ProjectBuilder.ResolveBuildOutputPath("../Builds/PackagedContent/StandaloneWindows64/Payload");
   var destination=System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path),"WhatTheFish_Data/StreamingAssets/PackagedContent");System.IO.Directory.CreateDirectory(destination);
   foreach(var file in System.IO.Directory.GetFiles(source))System.IO.File.Copy(file,System.IO.Path.Combine(destination,System.IO.Path.GetFileName(file)),true);
  }
  public static void Validate(){
   if(!Resources.Load<Sprite>("Menu/WhatTheFishLogo")||!Resources.Load<Sprite>("SportyMenu/Mascot")||!Resources.Load<Font>("SportyMenu/BarlowDisplay"))throw new BuildFailedException("Sports menu logo, mascot or typeface missing.");
   var cards=Resources.Load<Texture2D>("SportyMenu/SportCards");if(!cards||cards.width!=1536||cards.height!=1024)throw new BuildFailedException("Sports card atlas must preserve its approved 2 by 2 composition.");
   Debug.Log("SPORTY_MENU_ASSETS_VALIDATED");
  }
 }
 public sealed class SportyMenuTextureImport:AssetPostprocessor {
  void OnPreprocessTexture(){
   if(!assetPath.StartsWith("Assets/_Game/Resources/SportyMenu/"))return;
   var t=(TextureImporter)assetImporter;t.textureType=assetPath.EndsWith("Mascot.png")?TextureImporterType.Sprite:TextureImporterType.Default;
   t.spriteImportMode=SpriteImportMode.Single;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.npotScale=TextureImporterNPOTScale.None;t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;t.maxTextureSize=2048;t.isReadable=false;t.sRGBTexture=true;t.textureCompression=TextureImporterCompression.Uncompressed;
   t.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=2048,format=TextureImporterFormat.ASTC_4x4,compressionQuality=100});
  }
 }
}
