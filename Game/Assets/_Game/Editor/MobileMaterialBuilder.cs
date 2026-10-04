using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

// Delivery variants share measured pigment patterns. Authoring materials remain intact.
public static class MobileMaterialBuilder {
 const string Root="Assets/_Game/Art/MobileMaterials/";
 [Serializable] sealed class Mapping {public Entry[] materials;}
 [Serializable] sealed class Entry {public string material,source,sourceHash,pattern,normal;public float[] tint;public float maxPixelError,meanPixelError;}
 static Dictionary<string,Material> variants;
 public static void PrepareAndBuild(){Prepare();ProjectBuilder.BuildAndroidRelease();}
 public static void Prepare(){
  var entries=JsonUtility.FromJson<Mapping>(File.ReadAllText(Root+"material-map.json")).materials;
  using(var sha=SHA256.Create())foreach(var entry in entries){using var stream=File.OpenRead(entry.source);if(BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant()!=entry.sourceHash)throw new BuildFailedException("Shared material is stale: "+entry.source+". Run Tools/Blender/build_shared_mobile_materials.py.");}
  if(entries.Any(e=>e.maxPixelError>2.6f))throw new BuildFailedException("Shared colour pattern exceeds source error tolerance");
  Directory.CreateDirectory(Root+"Materials");AssetDatabase.Refresh();
  foreach(string pattern in entries.Select(e=>e.pattern).Distinct()){
   var importer=(TextureImporter)AssetImporter.GetAtPath(pattern);importer.sRGBTexture=true;importer.mipmapEnabled=true;importer.anisoLevel=8;
   var settings=importer.GetPlatformTextureSettings("Android");settings.overridden=true;
   settings.maxTextureSize=pattern.Contains("RI_Timber")?2048:1024;
   settings.format=pattern.Contains("RI_Timber")?TextureImporterFormat.ASTC_6x6:TextureImporterFormat.ASTC_8x8;settings.compressionQuality=100;settings.textureCompression=TextureImporterCompression.CompressedHQ;
   importer.SetPlatformTextureSettings(settings);importer.SaveAndReimport();
  }
  foreach(string normal in entries.Select(e=>e.normal).Distinct()){
   var importer=(TextureImporter)AssetImporter.GetAtPath(normal);var settings=importer.GetPlatformTextureSettings("Android");
   settings.overridden=true;settings.maxTextureSize=normal.Contains("RI_Timber")||normal.Contains("RI_Stone")||normal.Contains("RI_Soil")?1024:512;
   settings.format=TextureImporterFormat.ASTC_6x6;settings.compressionQuality=100;settings.textureCompression=TextureImporterCompression.CompressedHQ;
   importer.SetPlatformTextureSettings(settings);importer.SaveAndReimport();
  }
  var paths=AssetDatabase.GetDependencies(SkySailBuilder.BuildScenes().Concat(AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/_Game/Art/MobileVegetation"}).Select(AssetDatabase.GUIDToAssetPath)).ToArray(),true);
  foreach(string path in paths.Where(p=>p.EndsWith(".mat")).Distinct()){
   var original=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(!original.HasProperty("_BaseMap")||string.Equals(original.GetTag("KeepSourceTexture",false),"True",StringComparison.OrdinalIgnoreCase))continue;
   var entry=entries.FirstOrDefault(e=>e.source==AssetDatabase.GetAssetPath(original.GetTexture("_BaseMap")));
   // Accent materials can be recoloured through a property block at runtime.
   if(entry==null||original.name=="LG_Accent")continue;
   var clone=new Material(original){name=original.name};var pattern=AssetDatabase.LoadAssetAtPath<Texture>(entry.pattern);clone.SetTexture("_BaseMap",pattern);
   // URP retains a hidden legacy alias; keeping its old texture would ship both.
   if(clone.HasProperty("_MainTex"))clone.SetTexture("_MainTex",pattern);
   var tint=new Color(entry.tint[0],entry.tint[1],entry.tint[2],1);
   var color=(original.GetColor("_BaseColor").linear*tint.linear).gamma;color.a=original.GetColor("_BaseColor").a;clone.SetColor("_BaseColor",color);
   if(clone.HasProperty("_Color"))clone.SetColor("_Color",color);
   if(clone.HasProperty("_BumpMap")&&clone.GetTexture("_BumpMap"))clone.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture>(entry.normal));
   string target=Root+"Materials/"+AssetDatabase.AssetPathToGUID(path)+".mat";var old=AssetDatabase.LoadAssetAtPath<Material>(target);
   if(old){EditorUtility.CopySerialized(clone,old);UnityEngine.Object.DestroyImmediate(clone);EditorUtility.SetDirty(old);}else AssetDatabase.CreateAsset(clone,target);
  }
  variants=null;AssetDatabase.SaveAssets();Debug.Log("MOBILE_MATERIALS_PREPARED 24 pigments / 11 shared patterns; source error below 2/255");
 }
 public static void Apply(Scene scene){
  variants??=AssetDatabase.FindAssets("t:Material",new[]{Root+"Materials"}).Select(AssetDatabase.GUIDToAssetPath).ToDictionary(p=>Path.GetFileNameWithoutExtension(p),AssetDatabase.LoadAssetAtPath<Material>);
  int count=0;
  foreach(var root in scene.GetRootGameObjects())foreach(var component in root.GetComponentsInChildren<Component>(true)){
   if(!component)continue;
   var serialized=new SerializedObject(component);var property=serialized.GetIterator();bool changed=false;
   while(property.Next(true))if(property.propertyType==SerializedPropertyType.ObjectReference&&property.objectReferenceValue is Material original){
    // Explicit sharing can be cheaper when another shader already requires the
    // original image; also ignore an older generated variant for such materials.
    if(string.Equals(original.GetTag("KeepSourceTexture",false),"True",StringComparison.OrdinalIgnoreCase))continue;
    string guid=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(original));
    if(variants.TryGetValue(guid,out var replacement)){property.objectReferenceValue=replacement;changed=true;count++;}
   }
   if(changed)serialized.ApplyModifiedPropertiesWithoutUndo();
  }
  Debug.Log("MOBILE_MATERIALS_SHARED "+scene.name+" references="+count);
 }
}
public sealed class MobileMaterialBuildProcessor:IProcessSceneWithReport {
 public int callbackOrder=>-5;
 public void OnProcessScene(Scene scene,BuildReport report){
  if(report!=null&&(report.summary.platform==BuildTarget.Android||Environment.GetEnvironmentVariable("WTF_MOBILE_PREVIEW")=="1"))MobileMaterialBuilder.Apply(scene);
 }
}
