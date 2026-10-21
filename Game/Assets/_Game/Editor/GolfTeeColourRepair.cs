using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WhatTheFish;
using Object=UnityEngine.Object;

public static partial class GolfMultiplayerTeeBuild {
 static float OriginalEntryCorridor(float x,float z)=>(1-T1Smooth(14.5f,16.5f,Mathf.Abs(x+38.5f)))*(1-T1Smooth(7,9,Mathf.Abs(z+150)));
 static Material ConfigureTeeTurf(){
  var original=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/RefinedIslands/Golf/Golf_Terrain.mat");
  var material=AssetDatabase.LoadAssetAtPath<Material>(Folder+"Tee turf.mat");if(!material){material=new Material(original);AssetDatabase.CreateAsset(material,Folder+"Tee turf.mat");}
  material.CopyPropertiesFromMaterial(original);material.shader=Shader.Find("WhatTheFish/GolfTeeSurface");material.SetFloat("_IslandExtent",RefinedIslandBuilder.Read("Golf").extent);material.SetFloat("_LocalUVScale",4);EditorUtility.SetDirty(material);return material;
 }
 static void RestoreOriginalEntryPath(GameObject island){
  var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/RefinedIslands/Golf/Structures.fbx").GetComponentsInChildren<MeshFilter>(true).ToDictionary(f=>f.name,f=>f.sharedMesh);
  var native=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/RefinedIslands/Golf/Terrain.fbx").GetComponentsInChildren<MeshFilter>(true).ToDictionary(f=>f.name,f=>f.sharedMesh);
  var current=island.GetComponentsInChildren<MeshCollider>(true).Where(c=>native.ContainsKey(c.name)).ToArray();var temporary=new List<GameObject>();var original=new List<MeshCollider>();
  try{
   foreach(var filter in island.GetComponentsInChildren<MeshFilter>(true).Where(f=>native.ContainsKey(f.name))){var obj=new GameObject("Original path support");temporary.Add(obj);obj.transform.SetPositionAndRotation(filter.transform.position,filter.transform.rotation);obj.transform.localScale=filter.transform.lossyScale;var collider=obj.AddComponent<MeshCollider>();collider.sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(GolfCourseBuilder.Art+"Meshes/Terrain/"+filter.name+".asset")??native[filter.name];original.Add(collider);}
   foreach(var filter in island.GetComponentsInChildren<MeshFilter>(true).Where(f=>source.ContainsKey(f.name)&&f.name.Contains("CoastalPath_")).ToArray()){
    if(!AssetDatabase.GetAssetPath(filter.sharedMesh).StartsWith(Folder+"cleared-"))continue;
    var matrix=island.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;var inverse=matrix.inverse;var originalGround=original.ToArray();
    Vector3 Warp(Vector3 v){var p=matrix.MultiplyPoint3x4(v);if(OriginalEntryCorridor(p.x,p.z)<=.001f)return v;float old=Floor(island,originalGround,p).y,newY=Floor(island,current,p).y;p.y+=newY-old;return inverse.MultiplyPoint3x4(p);}
    var restored=RestoreGrassTriangles(filter.sharedMesh,source[filter.name],matrix,p=>OriginalEntryCorridor(p.x,p.z)>.001f,Warp);
    if(restored){filter.sharedMesh=Save(restored,"cleared-"+filter.name);foreach(var collider in filter.GetComponents<MeshCollider>())collider.sharedMesh=filter.sharedMesh;}
   }
   RepairEntryPathSeams(island);
  }finally{foreach(var obj in temporary)Object.DestroyImmediate(obj);}
 }
 public static void RepairTeeColours(){
  ConfigureTeeTurf();var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_Golf.unity");var island=scene.GetRootGameObjects().Single(r=>r.GetComponent<SkySailIslandScene>());island.SetActive(true);Physics.SyncTransforms();RestoreOriginalEntryPath(island);
  // Referenced meshes/material keep their GUIDs: scene/prefab coordinates are untouched.
  AssetDatabase.SaveAssets();AuditTeeColours();CaptureTeeColours("after");
 }
 public static void BuildTeeColourPlayer()=>ProjectBuilder.BuildWindowsPlayer("../Builds/GolfTeeColourQA/Player/WhatTheFish.exe");
}
