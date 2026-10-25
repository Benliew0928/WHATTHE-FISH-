using UnityEditor;

public sealed class LocalizationArtworkImport:AssetPostprocessor {
 void OnPreprocessTexture(){
  if(!assetPath.StartsWith("Assets/_Game/Resources/Localization/Artwork/"))return;
  var importer=(TextureImporter)assetImporter;
  importer.textureType=TextureImporterType.Default;importer.sRGBTexture=true;importer.alphaIsTransparency=true;
  importer.mipmapEnabled=false;importer.isReadable=false;importer.wrapMode=UnityEngine.TextureWrapMode.Clamp;importer.filterMode=UnityEngine.FilterMode.Bilinear;importer.npotScale=TextureImporterNPOTScale.None;
  string name=System.IO.Path.GetFileNameWithoutExtension(assetPath);
  int size=name=="Background"||name=="BadgeSheet"||name=="WarningSheet"?2048:name=="Playing"||name=="Finished"?256:name=="Title"||name=="Columns"||name=="Plate"?1024:512;
  importer.maxTextureSize=size;importer.textureCompression=TextureImporterCompression.CompressedHQ;
  importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=size,format=TextureImporterFormat.ASTC_4x4,compressionQuality=100});
  importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Standalone",overridden=true,maxTextureSize=size,format=TextureImporterFormat.RGBA32});
 }
}
