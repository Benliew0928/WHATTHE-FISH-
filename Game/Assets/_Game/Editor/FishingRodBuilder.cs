using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WhatTheFish;

public static class FishingRodBuilder {
 const string Art="Assets/_Game/Art/Fishing/Rods/",Prefabs="Assets/_Game/Prefabs/Fishing/";
 public const string GroupName="Fishing Rods";
 static readonly string[] Names={"BlueLime","TealOrange"};
 [MenuItem("WHATTHE FISH?/Fishing/Prepare Meshy rods")]
 public static void Prepare(){
  Directory.CreateDirectory(Prefabs);Directory.CreateDirectory("../Builds/FishingRods/20261001");AssetDatabase.Refresh();
  foreach(var file in Directory.GetFiles(Art,"*.png")){
   var path=file.Replace('\\','/');var importer=(TextureImporter)AssetImporter.GetAtPath(path);bool normal=path.Contains("Normal");
   importer.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;importer.sRGBTexture=!normal;
   importer.mipmapEnabled=true;importer.isReadable=false;importer.wrapMode=TextureWrapMode.Clamp;importer.anisoLevel=4;
   importer.alphaSource=TextureImporterAlphaSource.None;importer.maxTextureSize=normal?256:512;importer.textureCompression=TextureImporterCompression.CompressedHQ;
   importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=importer.maxTextureSize,format=TextureImporterFormat.ASTC_6x6,compressionQuality=100});importer.SaveAndReimport();
  }
  foreach(var name in Names){
   var importer=(ModelImporter)AssetImporter.GetAtPath(Art+name+".fbx");if(!importer)throw new Exception("Run build_fishing_rods.py first.");
   importer.globalScale=1;importer.useFileScale=true;importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;importer.materialImportMode=ModelImporterMaterialImportMode.None;
   importer.importNormals=ModelImporterNormals.Import;importer.importTangents=ModelImporterTangents.CalculateMikk;importer.meshCompression=ModelImporterMeshCompression.Low;importer.isReadable=false;importer.SaveAndReimport();
   var mat=AssetDatabase.LoadAssetAtPath<Material>(Art+name+".mat");if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,Art+name+".mat");}
   var color=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+name+"_BaseColor.png");mat.SetTexture("_BaseMap",color);mat.SetTexture("_MainTex",color);mat.SetColor("_BaseColor",Color.white);
   mat.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+name+"_Normal.png"));mat.EnableKeyword("_NORMALMAP");mat.SetFloat("_BumpScale",.5f);
   mat.SetFloat("_Metallic",0);mat.SetFloat("_Smoothness",.45f);mat.DisableKeyword("_METALLICSPECGLOSSMAP");mat.enableInstancing=true;EditorUtility.SetDirty(mat);
   var root=new GameObject(name);var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Art+name+".fbx"));model.name="Visual";model.transform.SetParent(root.transform,false);
   var renderers=model.GetComponentsInChildren<MeshRenderer>(true);foreach(var renderer in renderers){renderer.enabled=true;renderer.gameObject.SetActive(true);renderer.sharedMaterial=mat;renderer.gameObject.layer=10;}
   var lod=root.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.12f,renderers.Where(r=>r.name.EndsWith("_LOD0")).Cast<Renderer>().ToArray()),new LOD(.007f,renderers.Where(r=>r.name.EndsWith("_LOD1")).Cast<Renderer>().ToArray())});lod.RecalculateBounds();
   Socket(root,"Grip",new Vector3(0,.23f,0));
   var nearFilter=model.GetComponentsInChildren<MeshFilter>().Single(f=>f.name.EndsWith("_LOD0"));var points=nearFilter.sharedMesh.vertices.Select(v=>root.transform.InverseTransformPoint(nearFilter.transform.TransformPoint(v))).ToArray();float tipHeight=points.Max(v=>v.y);var tipPoints=points.Where(v=>v.y>tipHeight-.008f).ToArray();Socket(root,"Tip",tipPoints.Aggregate(Vector3.zero,(a,b)=>a+b)/tipPoints.Length);
   PrefabUtility.SaveAsPrefabAsset(root,Prefabs+name+"Rod.prefab");UnityEngine.Object.DestroyImmediate(root);
  }
  var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_Fishing.unity");Attach(scene.GetRootGameObjects().Single(r=>r.GetComponent<SkySailIslandScene>()));EditorSceneManager.SaveScene(scene);
  var environment=PrefabUtility.LoadPrefabContents("Assets/_Game/Prefabs/Environments/FishingEnvironment.prefab");
  try{Attach(environment);PrefabUtility.SaveAsPrefabAsset(environment,"Assets/_Game/Prefabs/Environments/FishingEnvironment.prefab");}finally{PrefabUtility.UnloadPrefabContents(environment);}
  AssetDatabase.SaveAssets();Audit();EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");Debug.Log("FISHING_RODS_PREPARE_OK");
 }
 static void Socket(GameObject root,string name,Vector3 pos){var socket=new GameObject(name);socket.transform.SetParent(root.transform,false);socket.transform.localPosition=pos;}
 public static void Attach(GameObject island){
  var old=island.transform.Find(GroupName);if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
  var stand=island.GetComponentsInChildren<FishingModule>(true).Single(s=>s.slotId=="Stand_01");
  var group=new GameObject(GroupName);group.transform.SetParent(island.transform,false);
  group.transform.position=stand.transform.TransformPoint(new Vector3(-1.65f,.02f,2.5f));group.transform.rotation=stand.transform.rotation;
  var timber=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/RefinedIslands/Shared/Materials/RI_Timber.mat");
  if(!timber)throw new Exception("Missing shared timber material.");
  for(int i=0;i<Names.Length;i++){
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+Names[i]+"Rod.prefab");if(!prefab)throw new Exception("Missing fishing rod "+Names[i]);
   var rod=(GameObject)PrefabUtility.InstantiatePrefab(prefab);rod.name=Names[i];rod.transform.SetParent(group.transform,false);
   rod.transform.localPosition=new Vector3((i-.5f)*.58f,.09f,-.08f);rod.transform.localRotation=Quaternion.Euler(8,i==0?155:205,0);
  }
  // Small timber rack at the pier edge leaves the central walking/fishing lane clear.
  foreach(float x in new[]{-.61f,.61f}){
   Box(group,"Rack upright",new Vector3(x,.45f,.12f),new Vector3(.055f,.9f,.065f),timber);
   Box(group,"Rack foot",new Vector3(x,.035f,.05f),new Vector3(.14f,.07f,.42f),timber);
  }
  Box(group,"Lower rest",new Vector3(0,.05f,-.08f),new Vector3(1.27f,.08f,.13f),timber);
  Box(group,"Support rail",new Vector3(0,.74f,.06f),new Vector3(1.27f,.07f,.07f),timber);
 }
 static void Box(GameObject parent,string name,Vector3 position,Vector3 scale,Material mat){var box=GameObject.CreatePrimitive(PrimitiveType.Cube);box.name=name;box.layer=8;box.transform.SetParent(parent.transform,false);box.transform.localPosition=position;box.transform.localScale=scale;box.GetComponent<Renderer>().sharedMaterial=mat;}
 public static void Audit(){
  var lines=new System.Collections.Generic.List<string>();
  foreach(var name in Names){
   var instance=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+name+"Rod.prefab"));
   try{
    var filters=instance.GetComponentsInChildren<MeshFilter>(true);if(filters.Length!=2)throw new Exception("Expected two LODs for "+name);
    var lod=instance.GetComponent<LODGroup>();if(lod.GetLODs().Any(l=>l.renderers.Length!=1))throw new Exception("LOD renderer assignment");
    var near=filters.Single(f=>f.name.EndsWith("_LOD0"));var bounds=near.GetComponent<Renderer>().bounds;
    if(Mathf.Abs(bounds.size.y-1.8f)>.008f||Mathf.Abs(bounds.min.y)>.008f)throw new Exception("Rod scale/pivot: "+bounds);
    foreach(var filter in filters){if(filter.sharedMesh.triangles.Length/3>14000)throw new Exception("Triangle budget");lines.Add($"PASS {name} {filter.name} triangles={filter.sharedMesh.triangles.Length/3}");}
    lines.Add($"PASS {name} Unity dimensions={bounds.size}; butt pivot; two LODs; grip and tip sockets");
   }finally{UnityEngine.Object.DestroyImmediate(instance);}
  }
  File.WriteAllLines("../Builds/FishingRods/20261001/unity-import-audit.txt",lines);
 }
 public static void BuildWindows(){Prepare();ProjectBuilder.BuildCurrentWindows();File.WriteAllText("../Builds/WindowsFinal/Review-Fishing-Rods.cmd","@echo off\r\nstart \"\" \"%~dp0WhatTheFish.exe\" -offline -sport Fishing -fishingRodPreview -screen-fullscreen 0 -screen-width 1600 -screen-height 1000\r\n");}
}
