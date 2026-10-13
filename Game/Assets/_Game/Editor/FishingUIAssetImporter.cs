using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// The full-resolution PNGs remain editable; delivery uses small alpha sprites.
public sealed class FishingUIAssetImporter:AssetPostprocessor {
 void OnPreprocessTexture(){
  if(!assetPath.StartsWith("Assets/_Game/Resources/FishingUI/")||!assetPath.EndsWith(".png"))return;
  var importer=(TextureImporter)assetImporter;
  int size=assetPath.EndsWith("TutorialPanels.png")||assetPath.EndsWith("GearIcons.png")?512:assetPath.EndsWith("Flame.png")?64:assetPath.EndsWith("FishBadge.png")||assetPath.EndsWith("FisherBadge.png")||assetPath.EndsWith("ClockBadge.png")?128:assetPath.EndsWith("CounterFrame.png")?512:256;
  importer.textureType=TextureImporterType.Sprite;importer.textureShape=TextureImporterShape.Texture2D;importer.spriteImportMode=SpriteImportMode.Single;
  importer.spritePixelsPerUnit=100;importer.spriteBorder=assetPath.EndsWith("CounterFrame.png")?new Vector4(44,22,44,22):Vector4.zero;
  var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
  importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.isReadable=false;importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;
  importer.maxTextureSize=size;importer.textureCompression=TextureImporterCompression.CompressedHQ;
  importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Standalone",overridden=true,maxTextureSize=size,format=TextureImporterFormat.RGBA32});
  importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=size,format=TextureImporterFormat.ASTC_6x6,compressionQuality=100});
 }
}

// A fresh clone can import a picture before its new postprocessor is compiled.
// Reimport these small delivery files before collecting either player's content.
public sealed class FishingUIBuildCheck:IPreprocessBuildWithReport {
 public int callbackOrder=>-10;
 public void OnPreprocessBuild(BuildReport report){
  foreach(string name in new[]{"ReelButton","SteerIdle","SteerLit","CounterFrame","GearIcons","TutorialPanels"}){
   string path="Assets/_Game/Resources/FishingUI/"+name+".png";
   AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
   var importer=AssetImporter.GetAtPath(path) as TextureImporter;
   var android=importer?importer.GetPlatformTextureSettings("Android"):null;
   if(!importer||importer.textureType!=TextureImporterType.Sprite||importer.mipmapEnabled||!AssetDatabase.LoadAssetAtPath<Sprite>(path)||android==null||!android.overridden||android.format!=TextureImporterFormat.ASTC_6x6)
    throw new BuildFailedException("Fishing delivery sprite/import settings invalid: "+path);
  }
  Debug.Log("FISHING_ILLUSTRATED_IMPORTS_VERIFIED");
 }
}
