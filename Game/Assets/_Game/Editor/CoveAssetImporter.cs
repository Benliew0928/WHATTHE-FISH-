using UnityEditor;

namespace WhatTheFish.Editor {
 // Full-quality masters stay in the repository; the player receives mobile blocks.
 public sealed class CoveAssetImporter : AssetPostprocessor {
  void OnPreprocessTexture() {
   if (!assetPath.StartsWith("Assets/_Game/Resources/Menu/")) return;
   var t = (TextureImporter)assetImporter;
   bool logo = assetPath.EndsWith("WhatTheFishLogo.png");
   t.textureType = logo ? TextureImporterType.Sprite : TextureImporterType.Default;
   t.spriteImportMode = SpriteImportMode.Single;
   t.alphaIsTransparency = true; t.mipmapEnabled = false; t.isReadable = false;
   t.npotScale = TextureImporterNPOTScale.None;
   t.maxTextureSize = logo ? 512 : 2048;
   t.textureCompression = TextureImporterCompression.CompressedHQ;
   t.SetPlatformTextureSettings(new TextureImporterPlatformSettings {
    name = "Android", overridden = true, maxTextureSize = logo ? 512 : 1024,
    format = TextureImporterFormat.ASTC_6x6, compressionQuality = 100
   });
  }
 }
}
