using UnityEditor;
using UnityEngine;

// Editable high-resolution masters live outside Assets; only tiny sprites ship.
public sealed class GolfUIAssetImporter : AssetPostprocessor {
 void OnPreprocessTexture() {
  if (!assetPath.StartsWith("Assets/_Game/Resources/GolfUI/") || !assetPath.EndsWith(".png")) return;
  var importer = (TextureImporter)assetImporter;
  if(assetPath.Contains("/Events/")){
   importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.isReadable=false;importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;
   foreach(var platform in new[]{"Standalone","Android"})importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name=platform,overridden=true,maxTextureSize=4096,format=TextureImporterFormat.RGBA32});
   return;
  }
  bool reference=assetPath.Contains("/Reference/");
  bool leaderboard=assetPath.Contains("/Leaderboard/");
  bool finalCountdown=assetPath.Contains("/FinalCountdown/");
  bool countdownPlate=finalCountdown&&!System.IO.Path.GetFileName(assetPath).StartsWith("Glyph");
  bool cartButton=(assetPath.Contains("/CartButtons/")||assetPath.Contains("/Badges/")||assetPath.Contains("/StartButton/")||assetPath.Contains("/PowerSlider/"));
  int size = assetPath.Contains("/PowerSlider/Rail")?1024:countdownPlate?1024:cartButton?512:reference||leaderboard?1024:256;
  importer.textureType = TextureImporterType.Sprite;
  importer.spriteImportMode = SpriteImportMode.Single;
  importer.spritePixelsPerUnit = 100;
  if(!reference)importer.spriteBorder = Vector4.zero;
  if(leaderboard)importer.spriteBorder=System.IO.Path.GetFileNameWithoutExtension(assetPath) switch {
   "Frame"=>new Vector4(52,52,52,52),
   "Row" or "LocalRow"=>new Vector4(28,28,28,28),
   "Cell" or "LocalCell"=>new Vector4(20,20,20,20),
   "Pending"=>new Vector4(40,40,40,40),
   _=>Vector4.zero
  };
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
   format = reference||cartButton||leaderboard||finalCountdown?TextureImporterFormat.ASTC_4x4:TextureImporterFormat.ASTC_6x6, compressionQuality = 100
  });
 }
}
