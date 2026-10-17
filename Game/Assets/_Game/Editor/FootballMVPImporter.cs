#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
public sealed class FootballMVPImporter:AssetPostprocessor {
 void OnPreprocessTexture(){
  if(!assetPath.StartsWith("Assets/Resources/Football/MVP/mvp-"))return;
  var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Sprite;t.spriteImportMode=SpriteImportMode.Single;t.spritePixelsPerUnit=100;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.isReadable=false;t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;t.maxTextureSize=1024;t.textureCompression=TextureImporterCompression.CompressedHQ;
  t.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=512,format=TextureImporterFormat.ASTC_6x6,compressionQuality=100});
 }
}
#endif
