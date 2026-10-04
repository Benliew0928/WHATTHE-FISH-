using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using WhatTheFish;

public static class MobileGeometryBuilder {
 const string Root="Assets/_Game/Art/MobileVegetation/";
 [Serializable] sealed class Placements {public Placement[] instances;}
 [Serializable] sealed class Placement {public string sport,cluster,kind;public int variant;public Vector3 position;public float scale,yaw;}
 [Serializable] sealed class GrassJson {public PatchJson[] prototypes;public MobileGrassLibrary.CoastalPatch[] coastal;}
 [Serializable] sealed class PatchJson {public BladeJson[] blades;}
 [Serializable] sealed class BladeJson {public Vector3[] p;public bool light;}
 public static void PrepareAndBuildAndroid(){Prepare();ProjectBuilder.BuildAndroidRelease();}
 public static void PrepareDeliveryAndBuild(){Prepare();MobileModelBuilder.Prepare();ProjectBuilder.BuildAndroidRelease();}
 public static void Prepare(){
  Directory.CreateDirectory(Root+"Meshes");Directory.CreateDirectory(Root+"Prefabs");AssetDatabase.Refresh();
  string path=Root+"CoastalVegetation.fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(path);
  if(!importer)throw new Exception("Run Tools/Blender/build_mobile_vegetation.py first.");
  importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;importer.isReadable=false;
  importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.meshCompression=ModelImporterMeshCompression.Medium;importer.SaveAndReimport();
  var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);
  var filters=model.GetComponentsInChildren<MeshFilter>(true).ToDictionary(f=>f.name);
  foreach(var kind in new[]{"Palm","Plant","Tuft"})for(int variant=0;variant<(kind=="Palm"?4:kind=="Plant"?3:2);variant++){
   string name=kind+"_"+variant;var root=new GameObject(name);var lods=new LOD[3];
   float[] heights=kind=="Palm"?new[]{.20f,.055f,.012f}:kind=="Plant"?new[]{.10f,.045f,.015f}:new[]{.08f,.025f,.009f};
   for(int level=0;level<3;level++){
    var filter=filters[name+"_LOD"+level];var mesh=UnityEngine.Object.Instantiate(filter.sharedMesh);mesh.name=name+"_LOD"+level;
    // Bake the same FBX axis/unit correction as CoastalIslandBuilder once.
    var matrix=Matrix4x4.Rotate(Quaternion.Euler(0,180,0))*filter.transform.localToWorldMatrix;
    mesh.vertices=mesh.vertices.Select(matrix.MultiplyPoint3x4).ToArray();mesh.normals=mesh.normals.Select(n=>matrix.MultiplyVector(n).normalized).ToArray();
    mesh.tangents=mesh.tangents.Select(t=>{Vector3 v=matrix.MultiplyVector(new Vector3(t.x,t.y,t.z)).normalized;return new Vector4(v.x,v.y,v.z,t.w);}).ToArray();
    mesh.RecalculateBounds();MeshUtility.SetMeshCompression(mesh,ModelImporterMeshCompression.Medium);
    string meshPath=Root+"Meshes/"+mesh.name+".asset";Save(mesh,meshPath);
    var node=new GameObject("LOD"+level);node.layer=10;node.transform.SetParent(root.transform,false);
    node.AddComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
    var renderer=node.AddComponent<MeshRenderer>();renderer.sharedMaterials=filter.GetComponent<MeshRenderer>().sharedMaterials.Select(m=>AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/CoastalIslands/"+m.name+".mat")).ToArray();
    if(renderer.sharedMaterials.Any(m=>!m))throw new Exception("Missing coast material for "+name);
    lods[level]=new LOD(heights[level],new Renderer[]{renderer});
   }
   var lod=root.AddComponent<LODGroup>();lod.SetLODs(lods);lod.RecalculateBounds();
   float height=root.GetComponentInChildren<Renderer>().bounds.size.y;
   if(kind=="Palm"&&(height<11||height>16))throw new Exception("Mobile palm unit/axis mismatch: "+height);
   PrefabUtility.SaveAsPrefabAsset(root,Root+"Prefabs/"+name+".prefab");UnityEngine.Object.DestroyImmediate(root);
  }
  var json=JsonUtility.FromJson<GrassJson>(File.ReadAllText(Root+"grass-prototypes.json"));
  var library=ScriptableObject.CreateInstance<MobileGrassLibrary>();
  library.patches=json.prototypes.Select(p=>new MobileGrassLibrary.Patch{blades=p.blades.Select(b=>new MobileGrassLibrary.Blade{a=b.p[0],b=b.p[1],c=b.p[2],d=b.p[3],e=b.p[4],light=b.light}).ToArray()}).ToArray();
  library.coastal=json.coastal;
  if(library.coastal==null||library.coastal.Length<1000)throw new Exception("Missing coastal meadow placements");
  foreach(var name in new[]{"Coast_Grass","Coast_LeafSun"}){
   var material=new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/CoastalIslands/"+name+".mat")){name=name+" meadow"};material.SetFloat("_Cull",0);Save(material,Root+name+".mat");
  }
  if(library.patches.Length!=3||library.patches.Any(p=>p.blades.Length!=1100))throw new Exception("Unexpected authored grass patch size");
  Save(library,Root+"GrassLibrary.asset");AssetDatabase.SaveAssets();
  Debug.Log("MOBILE_GEOMETRY_LIBRARY_COMPLETE 9 coast modules; 27 LOD meshes; 3300 shared grass blades");
 }
 static void Save(UnityEngine.Object asset,string path){var old=AssetDatabase.LoadMainAssetAtPath(path);if(old){EditorUtility.CopySerialized(asset,old);UnityEngine.Object.DestroyImmediate(asset);EditorUtility.SetDirty(old);}else AssetDatabase.CreateAsset(asset,path);}
 public static void Apply(Scene scene){
  foreach(var root in scene.GetRootGameObjects()){
   foreach(var coast in root.GetComponentsInChildren<CoastalEnvironment>(true)){
    var placements=JsonUtility.FromJson<Placements>(File.ReadAllText(Root+"coast-placements.json")).instances.Where(p=>p.sport==(coast.basketball?"Basketball":"Football")).ToArray();
    var source=coast.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name.Contains("PalmGarden_")).ToDictionary(f=>f.name);
    if(source.Count==0)continue;
    foreach(var placement in placements){
     if(!source.TryGetValue(placement.cluster,out var original))throw new BuildFailedException("Unknown original garden "+placement.cluster);
     var bounds=original.GetComponent<Renderer>().bounds;bounds.Expand(4);
     if(!bounds.Contains(coast.transform.TransformPoint(placement.position)))throw new BuildFailedException("Vegetation placement left its original garden: "+placement.cluster+" / "+placement.position+" bounds="+bounds);
    }
    var group=new GameObject("Shared coastal vegetation");group.transform.SetParent(coast.transform,false);
    var keys=placements.Select(p=>p.kind+"_"+p.variant).Distinct().ToArray();
    var instances=group.AddComponent<MobileVegetationInstances>();
    instances.prototypes=keys.Select(key=>{
     var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/"+key+".prefab");if(!prefab)throw new BuildFailedException("Mobile vegetation library not prepared");var lod=prefab.GetComponent<LODGroup>();
     return new MobileVegetationInstances.Prototype{size=lod.size,center=lod.localReferencePoint,levels=lod.GetLODs().Select(l=>{
      var renderer=l.renderers[0];if(renderer.sharedMaterials.Any(m=>!m.enableInstancing))throw new BuildFailedException("Vegetation material must enable instancing");
      return new MobileVegetationInstances.Level{mesh=renderer.GetComponent<MeshFilter>().sharedMesh,materials=renderer.sharedMaterials,threshold=l.screenRelativeTransitionHeight};
     }).ToArray()};
    }).ToArray();
    instances.placements=placements.Select(p=>new MobileVegetationInstances.Placement{prototype=Array.IndexOf(keys,p.kind+"_"+p.variant),matrix=Matrix4x4.TRS(p.position,Quaternion.Euler(0,p.yaw,0),Vector3.one*p.scale),scale=p.scale}).ToArray();
    foreach(var filter in source.Values){if(filter.GetComponent<Collider>())throw new BuildFailedException("Do not replace collision geometry: "+filter.name);filter.sharedMesh=null;var renderer=filter.GetComponent<MeshRenderer>();renderer.enabled=false;renderer.sharedMaterials=Array.Empty<Material>();}
    foreach(var filter in coast.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name.Contains("__Meadow_"))){filter.sharedMesh=null;var renderer=filter.GetComponent<MeshRenderer>();renderer.enabled=false;renderer.sharedMaterials=Array.Empty<Material>();}
    var meadow=coast.gameObject.AddComponent<MobileIslandGrass>();meadow.library=AssetDatabase.LoadAssetAtPath<MobileGrassLibrary>(Root+"GrassLibrary.asset");
    meadow.leaf=AssetDatabase.LoadAssetAtPath<Material>(Root+"Coast_Grass.mat");meadow.lightLeaf=AssetDatabase.LoadAssetAtPath<Material>(Root+"Coast_LeafSun.mat");
    Debug.Log("MOBILE_COAST_REUSED "+scene.name+" placements="+placements.Length+" retired baked clusters="+source.Count);
   }
   foreach(var refined in root.GetComponentsInChildren<RefinedIslandEnvironment>(true)){
    var old=refined.GetComponentsInChildren<MeshFilter>(true).Where(f=>AssetDatabase.GetAssetPath(f.sharedMesh).EndsWith("/Grass.fbx")||GolfCourseBuilder.IsCourseGrass(f.sharedMesh)).ToArray();
    if(old.Length==0)continue;
    foreach(var filter in old){filter.sharedMesh=null;var renderer=filter.GetComponent<MeshRenderer>();renderer.enabled=false;renderer.sharedMaterials=Array.Empty<Material>();}
    var grass=refined.gameObject.AddComponent<MobileIslandGrass>();grass.library=AssetDatabase.LoadAssetAtPath<MobileGrassLibrary>(Root+"GrassLibrary.asset");
    grass.leaf=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/RefinedIslands/Shared/Materials/RI_Leaf.mat");
    grass.lightLeaf=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/RefinedIslands/Shared/Materials/RI_LeafLight.mat");
    if(!grass.library||!grass.leaf||!grass.lightLeaf)throw new BuildFailedException("Missing mobile grass dependencies");
    Debug.Log("MOBILE_GRASS_REUSED "+scene.name+" retired baked meshes="+old.Length);
   }
  }
 }
}
public sealed class MobileGeometryBuildProcessor:IProcessSceneWithReport {
 public int callbackOrder=>-10;
 public void OnProcessScene(Scene scene,BuildReport report){
  if(report!=null&&(report.summary.platform==BuildTarget.Android||Environment.GetEnvironmentVariable("WTF_MOBILE_PREVIEW")=="1"))MobileGeometryBuilder.Apply(scene);
 }
}
