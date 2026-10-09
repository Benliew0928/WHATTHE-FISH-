using UnityEditor;
using UnityEngine;

// The full-resolution PNGs remain editable; delivery uses small alpha sprites.
public sealed class FishingUIAssetImporter:AssetPostprocessor {
 void OnPreprocessTexture(){
  if(!assetPath.StartsWith("Assets/_Game/Resources/FishingUI/")||!assetPath.EndsWith(".png"))return;
  var importer=(TextureImporter)assetImporter;bool frame=assetPath.EndsWith("LagoonFrame.png");
  int size=assetPath.EndsWith("Flame.png")?64:assetPath.EndsWith("FishBadge.png")||assetPath.EndsWith("FisherBadge.png")||assetPath.EndsWith("ClockBadge.png")?128:256;
  importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
  importer.spritePixelsPerUnit=100;importer.spriteBorder=frame?new Vector4(180,180,180,180):Vector4.zero;
  var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
  importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.isReadable=false;importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;
  importer.maxTextureSize=size;importer.textureCompression=TextureImporterCompression.CompressedHQ;
  importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Standalone",overridden=true,maxTextureSize=size,format=TextureImporterFormat.RGBA32});
  importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=size,format=TextureImporterFormat.ASTC_6x6,compressionQuality=100});
 }
}
