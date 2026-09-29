using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Reproducible delivery settings. Source PNGs and Blender masters stay intact.
public static class MobileAssetOptimizer {
 public const string Output="../Builds/MobileOptimization/";
 public static void PrepareAndBuild(){Prepare();MobileGeometryBuilder.Prepare();MobileModelBuilder.Prepare();ProjectBuilder.BuildAndroidRelease();}
 public static void Prepare(){
  Directory.CreateDirectory(Output);
  var paths=AssetDatabase.GetDependencies(SkySailBuilder.BuildScenes(),true).Where(p=>p.StartsWith("Assets/_Game/Art/")).Distinct().OrderBy(p=>p).ToArray();
  var lines=new List<string>{"asset,setting"};
  foreach(string path in paths){
   var texture=AssetImporter.GetAtPath(path) as TextureImporter;
   if(texture){
    BackupMeta(path);
    var settings=texture.GetPlatformTextureSettings("Android");
    bool control=path.Contains("TerrainControl"),depth=path.Contains("WaterDepth");
    bool normal=texture.textureType==TextureImporterType.NormalMap;
    bool mask=path.Contains("_Mask")||path.Contains("MetallicSmoothness")||path.Contains("metallic_smoothness");
    bool character=path.Contains("RainbowSprinter/");
    bool close=path.Contains("RI_Timber_")||path.Contains("RI_Teal_")||path.Contains("RI_Stone_")||path.Contains("RI_Bronze_");
    bool macro=path.Contains("TerrainColor")||path.Contains("GroundColor");
    settings.overridden=true;settings.compressionQuality=100;settings.crunchedCompression=false;
    settings.textureCompression=TextureImporterCompression.CompressedHQ;
    if(control){settings.maxTextureSize=4096;settings.format=TextureImporterFormat.R8;}
    else if(depth){settings.maxTextureSize=2048;settings.format=TextureImporterFormat.ASTC_4x4;}
    else if(mask){settings.maxTextureSize=character?1024:256;settings.format=TextureImporterFormat.ASTC_6x6;}
    else if(normal){settings.maxTextureSize=character||close?1024:512;settings.format=TextureImporterFormat.ASTC_6x6;}
    else {settings.maxTextureSize=character||close||macro?2048:1024;settings.format=macro?TextureImporterFormat.ASTC_8x8:TextureImporterFormat.ASTC_6x6;}
    texture.SetPlatformTextureSettings(settings);texture.SaveAndReimport();
    lines.Add(path+","+settings.maxTextureSize+" "+settings.format);
   }
   var model=AssetImporter.GetAtPath(path) as ModelImporter;
   if(model){
    BackupMeta(path);
    // Keep greater UV precision for architecture, clothing and close modules.
    bool medium=path.Contains("CoastalIslands/")||path.Contains("CoastalStadiums/")||path.Contains("/Sunvale/")||path.Contains("/Rally/")||path.EndsWith("Grass.fbx")||path.Contains("/Palm_")||path.Contains("/Plant_");
    model.meshCompression=medium?ModelImporterMeshCompression.Medium:ModelImporterMeshCompression.Low;
    model.SaveAndReimport();lines.Add(path+",mesh "+model.meshCompression);
   }
   if(path.EndsWith(".asset")&&AssetDatabase.LoadAssetAtPath<Mesh>(path) is Mesh mesh){
    BackupAsset(path);MeshUtility.SetMeshCompression(mesh,ModelImporterMeshCompression.Low);EditorUtility.SetDirty(mesh);
    lines.Add(path+",mesh Low (generated)");
   }
  }
  AssetDatabase.SaveAssets();File.WriteAllLines(Output+"import-settings.csv",lines);
  Debug.Log("MOBILE_IMPORT_SETTINGS_COMPLETE assets="+lines.Count);
 }
 static void BackupMeta(string path){BackupAsset(path+".meta");}
 static void BackupAsset(string path){
  string backup=Output+"original/"+path;Directory.CreateDirectory(Path.GetDirectoryName(backup));
  if(!File.Exists(backup))File.Copy(path,backup);
 }
}
