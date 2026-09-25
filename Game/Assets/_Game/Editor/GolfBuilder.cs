using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using WhatTheFish;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static partial class ProjectBuilder {
 const string GolfArt=Root+"Art/Golf/Tidebloom/";
 [Serializable] public class GolfImport {
  public GolfMaterial[] materials;public Vector3[] shoreline,spawns;public GolfObstacle[] obstacles;
  public int meshes,vertices,triangles,palms,bunkers;
 }
 [Serializable] public class GolfMaterial {public string name,base_map;public float[] color_srgb;public float roughness,emission;}
 [Serializable] public class GolfObstacle {public float x,y,z,rx,rz,height;}
 static GolfImport ReadGolf()=>JsonUtility.FromJson<GolfImport>(File.ReadAllText(GolfArt+"unity-import.json"));
 static GameObject BuildGolf(){
  var data=ReadGolf();Directory.CreateDirectory(GolfArt+"Materials");AssetDatabase.Refresh();
  foreach(var path in Directory.GetFiles(GolfArt,"*.png")){
   var ti=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));ti.textureType=TextureImporterType.Default;
   ti.sRGBTexture=true;ti.mipmapEnabled=true;ti.alphaSource=TextureImporterAlphaSource.None;
   ti.wrapMode=path.Contains("Course")?TextureWrapMode.Clamp:TextureWrapMode.Repeat;
   if(path.Contains("Gradient"))ti.wrapModeV=TextureWrapMode.Clamp;
   ti.maxTextureSize=path.Contains("Course")?2048:512;ti.anisoLevel=8;
   ti.textureCompression=TextureImporterCompression.CompressedHQ;ti.SaveAndReimport();
  }
  var mats=new Dictionary<string,Material>();
  foreach(var spec in data.materials){
   var path=GolfArt+"Materials/"+spec.name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
   var color=new Color(spec.color_srgb[0],spec.color_srgb[1],spec.color_srgb[2],1);
   m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",1-spec.roughness);m.SetFloat("_Metallic",0);
   m.SetTexture("_BaseMap",string.IsNullOrEmpty(spec.base_map)?null:AssetDatabase.LoadAssetAtPath<Texture2D>(GolfArt+spec.base_map));
   if(spec.emission>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*spec.emission);}
   m.enableInstancing=true;EditorUtility.SetDirty(m);mats[spec.name]=m;
  }
  var island=new GameObject("Tidebloom Island - Blender source");
  var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Art/GolfIsland.fbx"));
  model.transform.SetParent(island.transform,false);
  // Blender FBX -Z/Y arrives in Unity with both plan axes reversed.
  // Rotate the art only; manifest positions use Blender X/Y as Unity X/Z.
  model.transform.localRotation=Quaternion.Euler(0,180,0);
  var view=island.AddComponent<GolfIslandView>();
  view.waterMaterials=new[]{mats["TB_Ocean"],mats["TB_Lagoon"],mats["TB_Shallows"],mats["TB_Cascade"]};
  foreach(var filter in island.GetComponentsInChildren<MeshFilter>()){
   filter.gameObject.isStatic=true;filter.gameObject.layer=8;var renderer=filter.GetComponent<MeshRenderer>();
   renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>mats[m.name]).ToArray();
   bool water=filter.name.StartsWith("Water__"),background=filter.name.StartsWith("Backdrop__");
   renderer.shadowCastingMode=water||background?ShadowCastingMode.Off:ShadowCastingMode.On;
   if(filter.name.StartsWith("Terrain__Walkable")||filter.name.StartsWith("Rocks__Cluster")||filter.name=="Landmarks__PoolBasin"){
    var collider=filter.gameObject.AddComponent<MeshCollider>();collider.sharedMesh=filter.sharedMesh;
   }
   if(filter.name.StartsWith("Garden__")){var lod=filter.gameObject.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.012f,new Renderer[]{renderer})});lod.RecalculateBounds();}
  }
  var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
  tags.FindProperty("layers").GetArrayElementAtIndex(9).stringValue="PlayerBoundary";tags.ApplyModifiedProperties();Physics.IgnoreLayerCollision(0,9,false);
  var bounds=new GameObject("Shoreline boundary");bounds.transform.SetParent(island.transform,false);
  for(int i=0;i<data.shoreline.Length;i++){
   var a=data.shoreline[i];var b=data.shoreline[(i+1)%data.shoreline.Length];var direction=b-a;
   var segment=new GameObject("Shore boundary "+i);segment.layer=9;segment.transform.SetParent(bounds.transform,false);
   segment.transform.localPosition=(a+b)/2+Vector3.up*3;direction.y=0;segment.transform.localRotation=Quaternion.LookRotation(direction);
   segment.AddComponent<BoxCollider>().size=new Vector3(.5f,9,direction.magnitude+.3f);
  }
  var obstacles=new GameObject("Rock and palm collision");obstacles.transform.SetParent(island.transform,false);
  foreach(var o in data.obstacles){
   // Limestone uses its actual low-poly mesh. Thin trunks use cheap capsules.
   if(o.rx>2)continue;
   var go=new GameObject("Coastal obstacle");go.transform.SetParent(obstacles.transform,false);go.layer=8;go.transform.localPosition=new Vector3(o.x,o.y,o.z);
   var collider=go.AddComponent<CapsuleCollider>();collider.radius=Mathf.Min(o.rx,o.rz);collider.height=Mathf.Max(o.height,collider.radius*2);
  }
  File.WriteAllText("../Builds/golf-unity-import-audit.txt","Tidebloom import: "+data.meshes+" meshes, "+data.triangles+" source triangles; "+mats.Count+" mapped URP materials; "+data.shoreline.Length+" boundary segments; "+island.GetComponentsInChildren<MeshCollider>().Length+" terrain/rock mesh colliders; "+island.GetComponentsInChildren<CapsuleCollider>().Length+" trunk colliders.\n");
  return island;
 }
}
