using UnityEditor;
using UnityEngine;

// Editable high-resolution masters live outside Assets; only tiny sprites ship.
public sealed class GolfUIAssetImporter : AssetPostprocessor {
 void OnPreprocessTexture() {
  if (!assetPath.StartsWith("Assets/_Game/Resources/GolfUI/") || !assetPath.EndsWith(".png")) return;
  var importer = (TextureImporter)assetImporter;
  int size = assetPath.EndsWith("GolfCourseBadge.png") ? 128 : 256;
  importer.textureType = TextureImporterType.Sprite;
  importer.spriteImportMode = SpriteImportMode.Single;
  importer.spritePixelsPerUnit = 100;
  importer.spriteBorder = Vector4.zero;
  var settings = new TextureImporterSettings();
  importer.ReadTextureSettings(settings);
  settings.spriteMeshType = SpriteMeshType.FullRect;
  importer.SetTextureSettings(settings);
  importer.alphaIsTransparency = true;
  importer.mipmapEnabled = false;
  importer.isReadable = false;
  importer.npotScale = TextureImporterNPOTScale.None;
  importer.wrapMode = TextureWrapMode.Clamp;
  importer.filterMode = FilterMode.Bilinear;
  importer.maxTextureSize = size;
  importer.textureCompression = TextureImporterCompression.CompressedHQ;
  importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings {
   name = "Standalone", overridden = true, maxTextureSize = size,
   format = TextureImporterFormat.RGBA32
  });
  importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings {
   name = "Android", overridden = true, maxTextureSize = size,
   format = TextureImporterFormat.ASTC_6x6, compressionQuality = 100
  });
 }
}
