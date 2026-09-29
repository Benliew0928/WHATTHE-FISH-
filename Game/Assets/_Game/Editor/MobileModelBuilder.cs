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

public static class MobileModelBuilder {
 const string Root="Assets/_Game/Art/MobileModels/";
 [Serializable] sealed class Manifest {public Model[] models;}
 [Serializable] sealed class Model {public string source,sourceHash,delivery;public Change[] meshes;}
 [Serializable] sealed class Change {public string mesh;public int before,after;public float maxSourceVertexDistance;public bool accepted;}
 static Dictionary<string,Mesh> replacement;
 public static void PrepareAndBuild(){Prepare();ProjectBuilder.BuildAndroidRelease();}
 public static void ValidateSources(){
  var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Root+"model-map.json"));
  using var sha=SHA256.Create();
  foreach(var model in manifest.models){
   using var stream=File.OpenRead(model.source);string hash=BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();
   if(hash!=model.sourceHash)throw new BuildFailedException("Delivery model is stale: "+model.source+". Regenerate Tools/Blender/build_mobile_models.py, then MobileModelBuilder.Prepare.");
  }
 }
 public static void Prepare(){
  AssetDatabase.Refresh();Directory.CreateDirectory(Root+"Meshes");AssetDatabase.Refresh();
  var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Root+"model-map.json"));int count=0;
  foreach(var model in manifest.models){
   if(!model.meshes.Any(c=>c.accepted&&c.after<c.before))continue;
   var importer=(ModelImporter)AssetImporter.GetAtPath(model.delivery);
   importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;importer.importNormals=ModelImporterNormals.Import;
   importer.importTangents=ModelImporterTangents.CalculateMikk;importer.meshCompression=ModelImporterMeshCompression.Off;importer.SaveAndReimport();
   var original=AssetDatabase.LoadAssetAtPath<GameObject>(model.source).GetComponentsInChildren<MeshFilter>(true).ToDictionary(f=>f.name);
   var changed=AssetDatabase.LoadAssetAtPath<GameObject>(model.delivery).GetComponentsInChildren<MeshFilter>(true).ToDictionary(f=>f.name);
   foreach(var entry in model.meshes.Where(c=>c.accepted&&c.after<c.before)){
    if(entry.maxSourceVertexDistance>.02501f)throw new BuildFailedException("Mobile surface error exceeds 2.5 cm");
    if(!original.TryGetValue(entry.mesh,out var source)||!changed.TryGetValue(entry.mesh,out var target))throw new BuildFailedException("Delivery mesh name mismatch "+entry.mesh);
    var matrix=source.transform.worldToLocalMatrix*target.transform.localToWorldMatrix;
    var mesh=UnityEngine.Object.Instantiate(target.sharedMesh);mesh.name=source.sharedMesh.name;
    mesh.vertices=mesh.vertices.Select(matrix.MultiplyPoint3x4).ToArray();var normals=matrix.inverse.transpose;
    mesh.normals=mesh.normals.Select(n=>normals.MultiplyVector(n).normalized).ToArray();
    mesh.tangents=mesh.tangents.Select(t=>{var v=matrix.MultiplyVector(new Vector3(t.x,t.y,t.z)).normalized;return new Vector4(v.x,v.y,v.z,t.w);}).ToArray();mesh.RecalculateBounds();
    float scale=source.transform.lossyScale.magnitude;
    if((mesh.bounds.center-source.sharedMesh.bounds.center).magnitude*scale>.1f||(mesh.bounds.size-source.sharedMesh.bounds.size).magnitude*scale>.2f)throw new BuildFailedException("Delivery mesh axis/unit/bounds mismatch "+entry.mesh);
    if(mesh.subMeshCount!=source.sharedMesh.subMeshCount)throw new BuildFailedException("Delivery mesh material slots changed "+entry.mesh);
    MeshUtility.SetMeshCompression(mesh,ModelImporterMeshCompression.Medium);
    string path=Root+"Meshes/"+Key(source.sharedMesh)+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
    if(old){EditorUtility.CopySerialized(mesh,old);UnityEngine.Object.DestroyImmediate(mesh);EditorUtility.SetDirty(old);}else AssetDatabase.CreateAsset(mesh,path);count++;
   }
  }
  foreach(string path in new[]{"Assets/_Game/Art/Football/Sunvale/Stadium.fbx","Assets/_Game/Art/Basketball/Rally/LowerSeating.fbx","Assets/_Game/Art/Basketball/Rally/UpperSeating.fbx"}){
   foreach(var filter in AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponentsInChildren<MeshFilter>(true)){
    if(!filter.name.Contains("Seats"))continue;
    var size=Vector3.Scale(filter.sharedMesh.bounds.size,filter.transform.lossyScale);
    // Ten-bit packing is limited to these small, distant seating groups.
    // No seat is deleted; palette UVs stay inside their broad painted tiles.
    if(Mathf.Max(Mathf.Abs(size.x),Mathf.Abs(size.y),Mathf.Abs(size.z))>20)continue;
    var mesh=UnityEngine.Object.Instantiate(filter.sharedMesh);MeshUtility.SetMeshCompression(mesh,ModelImporterMeshCompression.High);
    string output=Root+"Meshes/"+Key(filter.sharedMesh)+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(output);
    if(old){EditorUtility.CopySerialized(mesh,old);UnityEngine.Object.DestroyImmediate(mesh);EditorUtility.SetDirty(old);}else AssetDatabase.CreateAsset(mesh,output);count++;
   }
  }
  // These silhouettes are only displayed hundreds of metres away.
  foreach(string guid in AssetDatabase.FindAssets("t:Mesh",new[]{"Assets/_Game/Art/SkySail/Distant"})){
   var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(guid));MeshUtility.SetMeshCompression(mesh,ModelImporterMeshCompression.High);EditorUtility.SetDirty(mesh);
  }
  AssetDatabase.SaveAssets();replacement=null;Debug.Log("MOBILE_MODELS_PREPARED meshes="+count);
 }
 static string Key(Mesh mesh){AssetDatabase.TryGetGUIDAndLocalFileIdentifier(mesh,out string guid,out long id);return guid+"_"+id;}
 public static void Apply(Scene scene){
  replacement??=AssetDatabase.FindAssets("t:Mesh",new[]{Root+"Meshes"}).Select(AssetDatabase.GUIDToAssetPath).ToDictionary(p=>Path.GetFileNameWithoutExtension(p),AssetDatabase.LoadAssetAtPath<Mesh>);
  var collision=new HashSet<Mesh>(scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<MeshCollider>(true)).Select(c=>c.sharedMesh));int count=0;
  foreach(var root in scene.GetRootGameObjects())foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true)){
   if(!filter.sharedMesh||collision.Contains(filter.sharedMesh))continue;
   if(replacement.TryGetValue(Key(filter.sharedMesh),out var mesh)){filter.sharedMesh=mesh;count++;}
  }
  Debug.Log("MOBILE_MODELS_APPLIED "+scene.name+" render meshes="+count+"; authored collision retained");
 }
}
public sealed class MobileModelBuildProcessor:IProcessSceneWithReport,IPreprocessBuildWithReport {
 public int callbackOrder=>-8;
 public void OnPreprocessBuild(BuildReport report){if(report.summary.platform==BuildTarget.Android||Environment.GetEnvironmentVariable("WTF_MOBILE_PREVIEW")=="1")MobileModelBuilder.ValidateSources();}
 public void OnProcessScene(Scene scene,BuildReport report){
  if(report!=null&&(report.summary.platform==BuildTarget.Android||Environment.GetEnvironmentVariable("WTF_MOBILE_PREVIEW")=="1"))MobileModelBuilder.Apply(scene);
 }
}
