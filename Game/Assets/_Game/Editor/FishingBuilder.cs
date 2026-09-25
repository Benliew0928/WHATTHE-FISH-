using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using WhatTheFish;

public static partial class ProjectBuilder {
 const string FishingArt=Root+"Art/Fishing/Lagoon/";
 const string FishingPrefabs=Root+"Prefabs/Fishing/Lagoon/";
 [Serializable] public class FishingImport {
  public string name;public int capacity,unique_triangles;
  public FishingAsset[] modules;public GolfMaterial[] materials;public FishingPlacement[] placements;
  public Vector3[] spawns,stands;public FishingBoundary[] boundaries;
 }
 [Serializable] public class FishingAsset {public string name,collision;public FishingBox[] boxes;public int meshes,triangles;}
 [Serializable] public class FishingBox {public float[] center,size;}
 [Serializable] public class FishingPlacement {public string module,id,accent;public Vector3 position;public float yaw,scale;}
 [Serializable] public class FishingBoundary {public Vector3 a,b;}
 static FishingImport ReadFishing()=>JsonUtility.FromJson<FishingImport>(File.ReadAllText(FishingArt+"unity-import.json"));

 static GameObject BuildFishing(){
  var data=ReadFishing();Directory.CreateDirectory(FishingPrefabs);Directory.CreateDirectory(FishingArt+"Materials");AssetDatabase.Refresh();
  foreach(var path in Directory.GetFiles(FishingArt,"*.png")){
   var ti=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));ti.textureType=TextureImporterType.Default;
   ti.sRGBTexture=true;ti.mipmapEnabled=true;ti.alphaSource=TextureImporterAlphaSource.None;
   ti.wrapMode=path.Contains("Terrain")||path.Contains("Gradient")||path.Contains("WaterPainted")?TextureWrapMode.Clamp:TextureWrapMode.Repeat;
   ti.maxTextureSize=path.Contains("Terrain")||path.Contains("WaterPainted")?2048:512;ti.anisoLevel=4;ti.textureCompression=TextureImporterCompression.CompressedHQ;ti.SaveAndReimport();
  }
  var materials=new Dictionary<string,Material>();
  foreach(var spec in data.materials){
   string path=FishingArt+"Materials/"+spec.name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
   bool isWater=spec.name=="LG_Ocean"||spec.name=="LG_LagoonWater"||spec.name=="LG_WaterGradient";
   mat.shader=Shader.Find(isWater?"WhatTheFish/LagoonWater":"Universal Render Pipeline/Lit");
   var color=new Color(spec.color_srgb[0],spec.color_srgb[1],spec.color_srgb[2],1);
   mat.SetColor("_BaseColor",color);mat.SetFloat("_Smoothness",1-spec.roughness);mat.SetFloat("_Metallic",0);
   mat.SetTexture("_BaseMap",string.IsNullOrEmpty(spec.base_map)?null:AssetDatabase.LoadAssetAtPath<Texture2D>(FishingArt+spec.base_map));
   if(spec.emission>0){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",color*spec.emission);}
   mat.enableInstancing=true;EditorUtility.SetDirty(mat);materials.Add(spec.name,mat);
  }
  var prefabs=new Dictionary<string,GameObject>();
  foreach(var spec in data.modules){
   string path=FishingArt+spec.name+".fbx";var importer=AssetImporter.GetAtPath(path) as ModelImporter;
   if(!importer)throw new Exception("Missing fishing module: "+path);
   importer.globalScale=1;importer.useFileScale=true;importer.preserveHierarchy=true;
   importer.importCameras=false;importer.importLights=false;importer.importAnimation=false;importer.animationType=ModelImporterAnimationType.None;
   importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.importNormals=ModelImporterNormals.Import;importer.isReadable=false;importer.SaveAndReimport();
   var root=new GameObject(spec.name);var marker=root.AddComponent<FishingModule>();marker.moduleType=spec.name;marker.slotId=spec.name;
   var art=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));art.transform.SetParent(root.transform,false);
   art.transform.localRotation=Quaternion.Euler(0,180,0);
   bool water=spec.name=="Ocean"||spec.name=="LagoonWater"||spec.name=="Shallows"||spec.name=="ShoreFoam";
   foreach(var filter in art.GetComponentsInChildren<MeshFilter>(true)){
    filter.gameObject.layer=8;var renderer=filter.GetComponent<MeshRenderer>();
    renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>materials[m.name]).ToArray();
    renderer.shadowCastingMode=water?ShadowCastingMode.Off:ShadowCastingMode.On;
    if(water)renderer.receiveShadows=false;
    if(spec.collision=="mesh"){var collider=filter.gameObject.AddComponent<MeshCollider>();collider.sharedMesh=filter.sharedMesh;}
   }
   foreach(var box in spec.boxes){
    var go=new GameObject("Collision");go.layer=8;go.transform.SetParent(root.transform,false);var c=go.AddComponent<BoxCollider>();
    c.center=new Vector3(box.center[0],box.center[1],box.center[2]);c.size=new Vector3(box.size[0],box.size[1],box.size[2]);
   }
   if(spec.name.StartsWith("Flowers")||spec.name=="Bush"){
    var lod=root.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.009f,root.GetComponentsInChildren<Renderer>())});lod.RecalculateBounds();
   }
   prefabs[spec.name]=PrefabUtility.SaveAsPrefabAsset(root,FishingPrefabs+spec.name+".prefab");UnityEngine.Object.DestroyImmediate(root);
  }
  var lagoon=new GameObject("Lagoon Island - Modular Fishing Environment");var view=lagoon.AddComponent<FishingLagoonView>();var stands=new List<FishingModule>();
  foreach(var p in data.placements){
   var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefabs[p.module]);instance.name=p.id;instance.transform.SetParent(lagoon.transform,false);
   instance.transform.localPosition=p.position;instance.transform.localRotation=Quaternion.Euler(0,p.yaw,0);instance.transform.localScale=Vector3.one*p.scale;
   var marker=instance.GetComponent<FishingModule>();marker.slotId=p.id;
   if(!string.IsNullOrEmpty(p.accent)){marker.accent=LocalProfile.Hex(p.accent);marker.SetAccent(marker.accent);}
   if(p.module=="PlayerStand")stands.Add(marker);
  }
  view.playerStands=stands.ToArray();view.standingPositions=data.stands;view.waterMaterials=new[]{materials["LG_Ocean"],materials["LG_LagoonWater"]};
  var boundaryRoot=new GameObject("Player water boundaries");boundaryRoot.transform.SetParent(lagoon.transform,false);
  foreach(var edge in data.boundaries){
   var d=edge.b-edge.a;if(d.sqrMagnitude<.0001f)continue;
   var segment=new GameObject("Shore boundary");segment.layer=9;segment.transform.SetParent(boundaryRoot.transform,false);
   segment.transform.localPosition=(edge.a+edge.b)*.5f;segment.transform.localRotation=Quaternion.LookRotation(d);
   segment.AddComponent<BoxCollider>().size=new Vector3(.14f,6,d.magnitude+.035f);
  }
  var anchors=new GameObject("Gameplay sockets - no fishing logic");anchors.transform.SetParent(lagoon.transform,false);
  for(int i=0;i<5;i++){
   var anchor=new GameObject("Spawn_"+(i+1).ToString("00"));anchor.transform.SetParent(anchors.transform,false);anchor.transform.localPosition=data.spawns[i];
   var stand=new GameObject("Standing_"+(i+1).ToString("00"));stand.transform.SetParent(anchors.transform,false);stand.transform.localPosition=data.stands[i];
   stand.transform.localRotation=Quaternion.LookRotation(new Vector3(-data.stands[i].x,0,-data.stands[i].z));
  }
  Physics.SyncTransforms();
  if(stands.Count!=5||data.spawns.Length!=5)throw new Exception("Fishing requires five independent stands and spawns.");
  foreach(var p in data.stands)if(!Physics.Raycast(p+Vector3.up,Vector3.down,out var hit,3,1<<8)||Mathf.Abs(hit.point.y-1.2f)>.05f)throw new Exception("Fishing module scale/axis/collision check failed at "+p);
  PrefabUtility.SaveAsPrefabAsset(lagoon,FishingPrefabs+"LagoonIsland.prefab");
  Directory.CreateDirectory("../Builds");File.WriteAllText("../Builds/fishing-unity-import-audit.txt","Modules: "+data.modules.Length+"\nStands: "+stands.Count+"\nSpawns: "+data.spawns.Length+"\nUnique source triangles: "+data.unique_triangles+"\nFive deck floor/axis checks passed\n");
  return lagoon;
 }
}
