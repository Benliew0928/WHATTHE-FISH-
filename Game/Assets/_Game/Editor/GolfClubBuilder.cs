using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WhatTheFish;

public static class GolfClubBuilder {
 const string Art="Assets/_Game/Art/Golf/Club/",Prefab="Assets/_Game/Resources/GolfClub.prefab";
 [MenuItem("WHATTHE FISH?/Golf/Prepare handheld club")]
 public static void Prepare(){
  AssetDatabase.Refresh();
  foreach(var file in Directory.GetFiles(Art,"*.png")){
   var path=file.Replace('\\','/');var importer=(TextureImporter)AssetImporter.GetAtPath(path);bool normal=path.Contains("Normal"),mask=path.Contains("Mask");
   importer.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;importer.sRGBTexture=!normal&&!mask;
   importer.textureShape=TextureImporterShape.Texture2D;importer.npotScale=TextureImporterNPOTScale.None;
   importer.mipmapEnabled=true;importer.isReadable=false;importer.wrapMode=TextureWrapMode.Clamp;importer.anisoLevel=4;
   importer.alphaSource=mask?TextureImporterAlphaSource.FromInput:TextureImporterAlphaSource.None;importer.maxTextureSize=mask?256:512;
   importer.textureCompression=TextureImporterCompression.CompressedHQ;
   importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=importer.maxTextureSize,format=normal?TextureImporterFormat.ASTC_4x4:mask?TextureImporterFormat.ASTC_8x8:TextureImporterFormat.ASTC_6x6,compressionQuality=100});importer.SaveAndReimport();
  }
  var modelImporter=(ModelImporter)AssetImporter.GetAtPath(Art+"MidnightIron.fbx");if(!modelImporter)throw new Exception("Run prepare_golf_club.py first.");
  modelImporter.globalScale=1;modelImporter.useFileScale=true;modelImporter.importAnimation=false;modelImporter.importCameras=false;modelImporter.importLights=false;modelImporter.materialImportMode=ModelImporterMaterialImportMode.None;
  modelImporter.importNormals=ModelImporterNormals.Import;modelImporter.importTangents=ModelImporterTangents.CalculateMikk;modelImporter.meshCompression=ModelImporterMeshCompression.Low;modelImporter.isReadable=false;modelImporter.SaveAndReimport();
  var material=AssetDatabase.LoadAssetAtPath<Material>(Art+"MidnightIron.mat");
  if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,Art+"MidnightIron.mat");}
  Texture2D Map(string name){var image=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"MidnightIron_"+name+".png");if(!image)throw new Exception("Missing 2D club texture: "+name);return image;}
  var color=Map("BaseColor");material.SetTexture("_BaseMap",color);material.SetTexture("_MainTex",color);material.SetColor("_BaseColor",Color.white);
  material.SetTexture("_BumpMap",Map("Normal"));material.EnableKeyword("_NORMALMAP");material.SetFloat("_BumpScale",.7f);
  material.SetTexture("_MetallicGlossMap",Map("Mask"));material.EnableKeyword("_METALLICSPECGLOSSMAP");material.SetFloat("_Metallic",1);material.SetFloat("_Smoothness",1);material.enableInstancing=true;EditorUtility.SetDirty(material);
  var root=new GameObject("GolfClub");
  try {
   var visual=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Art+"MidnightIron.fbx"));PrefabUtility.UnpackPrefabInstance(visual,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);visual.name="Visual";visual.transform.SetParent(root.transform,false);
   var markers=visual.GetComponentsInChildren<Transform>(true);var grip=markers.Single(t=>t.name=="ClubGrip");var top=markers.Single(t=>t.name=="ClubTop");var face=markers.Single(t=>t.name=="ClubFace");
   visual.transform.rotation=Quaternion.FromToRotation(top.position-grip.position,Vector3.up)*visual.transform.rotation;
   var normal=Vector3.ProjectOnPlane(face.position-grip.position,Vector3.up).normalized;
   visual.transform.rotation=Quaternion.FromToRotation(normal,Vector3.forward)*visual.transform.rotation;visual.transform.position-=grip.position;
   var renderers=visual.GetComponentsInChildren<MeshRenderer>(true);foreach(var r in renderers)r.sharedMaterial=material;
   foreach(var imported in visual.GetComponentsInChildren<LODGroup>(true))UnityEngine.Object.DestroyImmediate(imported);
   var lod=root.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.15f,renderers.Where(r=>r.name.EndsWith("LOD0")).Cast<Renderer>().ToArray()),new LOD(.065f,renderers.Where(r=>r.name.EndsWith("LOD1")).Cast<Renderer>().ToArray()),new LOD(.008f,renderers.Where(r=>r.name.EndsWith("LOD2")).Cast<Renderer>().ToArray())});lod.RecalculateBounds();
   Transform Marker(string name,Vector3 point){var t=new GameObject(name).transform;t.SetParent(root.transform,false);t.localPosition=point;return t;}
   var club=root.AddComponent<GolfClubModel>();club.head=markers.Single(t=>t.name=="ClubHead");club.leftGrip=Marker("Left hand grip",new Vector3(0,.034f,-.010f));club.rightGrip=Marker("Right hand grip",new Vector3(0,-.034f,0));
   foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=2;
   PrefabUtility.SaveAsPrefabAsset(root,Prefab);
   Directory.CreateDirectory("../Builds/GolfClubQA/Art");File.WriteAllLines("../Builds/GolfClubQA/Art/unity-import.txt",new[]{"PASS grip="+grip.position,"PASS head="+club.head.position,"PASS one renderer per LOD"}.Concat(root.GetComponentsInChildren<MeshFilter>(true).Select(f=>"PASS "+f.name+" triangles="+f.sharedMesh.triangles.Length/3)));
   if(Vector3.Distance(grip.position,Vector3.zero)>.001f||club.head.position.y>-.7f||club.head.position.y< -1.05f)throw new Exception("Incorrect handheld club axis/scale.");
  } finally {UnityEngine.Object.DestroyImmediate(root);}
  AssetDatabase.SaveAssets();Debug.Log("GOLF_CLUB_PREPARE_OK");
 }
}
