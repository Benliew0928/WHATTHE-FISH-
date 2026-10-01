using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WhatTheFish;

public static class GolfEquipmentBuilder {
 const string Art="Assets/_Game/Art/Golf/Equipment/", Prefabs="Assets/_Game/Prefabs/Golf/";
 public const string GroupName="Golf Equipment";
 static readonly string[] Names={"Ball","Driver","Iron","Putter"};
 [MenuItem("WHATTHE FISH?/Golf/Prepare Meshy equipment")]
 public static void Prepare(){
  Directory.CreateDirectory(Prefabs);Directory.CreateDirectory("../Builds/GolfEquipment/20261001");AssetDatabase.Refresh();
  foreach(var file in Directory.GetFiles(Art,"*.png")){
   var path=file.Replace('\\','/');var importer=(TextureImporter)AssetImporter.GetAtPath(path);bool normal=path.Contains("Normal"),mask=path.Contains("Mask");
   importer.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;importer.sRGBTexture=!normal&&!mask;
   importer.mipmapEnabled=true;importer.isReadable=false;importer.wrapMode=path.Contains("Ball_")?TextureWrapMode.Repeat:TextureWrapMode.Clamp;importer.anisoLevel=4;
   importer.alphaSource=mask?TextureImporterAlphaSource.FromInput:TextureImporterAlphaSource.None;importer.maxTextureSize=path.Contains("Ball_")?1024:mask?256:512;
   importer.textureCompression=TextureImporterCompression.CompressedHQ;
   importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=importer.maxTextureSize,format=normal?TextureImporterFormat.ASTC_4x4:TextureImporterFormat.ASTC_6x6,compressionQuality=100});importer.SaveAndReimport();
  }
  foreach(string name in Names){
   var importer=(ModelImporter)AssetImporter.GetAtPath(Art+name+".fbx");if(!importer)throw new Exception("Run build_golf_equipment.py first.");
   importer.globalScale=1;importer.useFileScale=true;importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;importer.materialImportMode=ModelImporterMaterialImportMode.None;
   importer.importNormals=ModelImporterNormals.Import;importer.importTangents=ModelImporterTangents.CalculateMikk;importer.meshCompression=ModelImporterMeshCompression.Off;importer.isReadable=false;importer.SaveAndReimport();
   string materialPath=Art+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
   if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,materialPath);}
   var color=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+name+"_BaseColor.png");mat.SetTexture("_BaseMap",color);mat.SetTexture("_MainTex",color);mat.SetColor("_BaseColor",name=="Ball"?new Color(.96f,.95f,.91f):Color.white);
   mat.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+name+"_Normal.png"));mat.EnableKeyword("_NORMALMAP");mat.SetFloat("_BumpScale",.6f);
   var mask=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+name+"_Mask.png");mat.SetTexture("_MetallicGlossMap",mask);mat.SetFloat("_Metallic",name=="Ball"?0:.7f);mat.SetFloat("_Smoothness",name=="Ball"?.38f:1);
   if(mask)mat.EnableKeyword("_METALLICSPECGLOSSMAP");else mat.DisableKeyword("_METALLICSPECGLOSSMAP");mat.enableInstancing=true;EditorUtility.SetDirty(mat);
   var root=new GameObject(name);var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Art+name+".fbx"));model.name="Visual";model.transform.SetParent(root.transform,false);
   var renderers=model.GetComponentsInChildren<MeshRenderer>(true);foreach(var r in renderers){r.enabled=true;r.gameObject.SetActive(true);r.sharedMaterial=mat;}
   var lod=root.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(name=="Ball"?.012f:.04f,renderers.Where(r=>r.name==name+"_LOD0").Cast<Renderer>().ToArray()),new LOD(name=="Ball"?.0004f:.002f,renderers.Where(r=>r.name==name+"_LOD1").Cast<Renderer>().ToArray())});lod.RecalculateBounds();
   if(name=="Ball"){var sphere=root.AddComponent<SphereCollider>();sphere.radius=.0215f;sphere.contactOffset=.001f;}
   PrefabUtility.SaveAsPrefabAsset(root,Prefabs+name+".prefab");UnityEngine.Object.DestroyImmediate(root);
  }
  var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_Golf.unity");var island=scene.GetRootGameObjects().Single(r=>r.GetComponent<SkySailIslandScene>());Attach(island);EditorSceneManager.SaveScene(scene);
  var environment=PrefabUtility.LoadPrefabContents("Assets/_Game/Prefabs/Environments/GolfEnvironment.prefab");
  try{Attach(environment);PrefabUtility.SaveAsPrefabAsset(environment,"Assets/_Game/Prefabs/Environments/GolfEnvironment.prefab");}finally{PrefabUtility.UnloadPrefabContents(environment);}
  AssetDatabase.SaveAssets();Audit();EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");Debug.Log("GOLF_EQUIPMENT_PREPARE_OK");
 }
 public static void Attach(GameObject island){
  bool wasActive=island.activeSelf;island.SetActive(true);var old=island.transform.Find(GroupName);if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
  var group=new GameObject(GroupName);group.transform.SetParent(island.transform,false);
  // Next to the practice green, offset from its walking route and pavilion access.
  var anchor=new Vector3(-84,0,-117);group.transform.localPosition=anchor;
  Physics.SyncTransforms();
  var timber=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/RefinedIslands/Shared/Materials/RI_Timber.mat");if(!timber)throw new Exception("Missing shared timber material.");
  for(int i=1;i<Names.Length;i++){
   var model=AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+Names[i]+".prefab");if(!model)throw new Exception("Missing golf prefab "+Names[i]);
   var obj=(GameObject)PrefabUtility.InstantiatePrefab(model);obj.name=Names[i];obj.transform.SetParent(group.transform,false);obj.transform.localPosition=new Vector3((i-2)*.34f,0,0);
   obj.transform.localRotation=Quaternion.Euler(-7,0,0);Physics.SyncTransforms();var floor=Floor(island,obj.transform.position);
   var filter=obj.GetComponentsInChildren<MeshFilter>(true).Single(f=>f.name.EndsWith("_LOD0"));var points=filter.sharedMesh.vertices.Select(p=>filter.transform.TransformPoint(p)).ToArray();float bottom=points.Min(p=>p.y);
   obj.transform.position+=Vector3.up*(floor.y-bottom);Physics.SyncTransforms();points=filter.sharedMesh.vertices.Select(p=>filter.transform.TransformPoint(p)).ToArray();bottom=points.Min(p=>p.y);
   float clearance=points.Where(p=>p.y<bottom+.08f).Min(p=>p.y-Floor(island,p).y);obj.transform.position+=Vector3.up*(.0005f-clearance);
  }
  var ball=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+"Ball.prefab"));ball.name="Ball";ball.transform.SetParent(group.transform,false);ball.transform.localPosition=new Vector3(.33f,0,.32f);ball.transform.position=Floor(island,ball.transform.position)+Vector3.up*.0215f;
  var middle=Floor(island,group.transform.position);float baseY=group.transform.InverseTransformPoint(middle).y;
  // A narrow reusable timber support holds the three clubs without blocking the route.
  foreach(float x in new[]{-.53f,.53f})Box(group,"Rack upright",new Vector3(x,baseY+.36f,-.12f),new Vector3(.045f,.72f,.05f),timber);
  Box(group,"Rack rail",new Vector3(0,baseY+.68f,-.12f),new Vector3(1.12f,.055f,.07f),timber);
  foreach(float x in new[]{-.53f,.53f}){var foot=Floor(island,group.transform.TransformPoint(new Vector3(x,0,-.12f)));Box(group,"Rack foot",group.transform.InverseTransformPoint(foot)+Vector3.up*.025f,new Vector3(.10f,.05f,.40f),timber);}
  island.SetActive(wasActive);
 }
 static Vector3 Floor(GameObject island,Vector3 at){
  var ray=new Ray(new Vector3(at.x,island.transform.position.y+100,at.z),Vector3.down);
  // Collider.Raycast also works while editing isolated prefab contents.
  var hits=island.GetComponentsInChildren<MeshCollider>(true).Where(c=>c.name.StartsWith("Terrain__")).Select(c=>c.Raycast(ray,out var hit,200)?(RaycastHit?)hit:null).Where(h=>h.HasValue).Select(h=>h.Value).OrderBy(h=>h.distance).ToArray();
  if(hits.Length==0)throw new Exception("No golf terrain below equipment at "+at);return hits[0].point;
 }
 static void Box(GameObject group,string name,Vector3 pos,Vector3 size,Material mat){var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name=name;obj.transform.SetParent(group.transform,false);obj.transform.localPosition=pos;obj.transform.localScale=size;obj.layer=8;obj.GetComponent<Renderer>().sharedMaterial=mat;}
 public static void Audit(){
  var lines=new System.Collections.Generic.List<string>();
  foreach(var name in Names){var root=AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+name+".prefab");var filters=root.GetComponentsInChildren<MeshFilter>(true);if(filters.Length!=2)throw new Exception("Expected two LODs: "+name);
   foreach(var f in filters){if(!f.sharedMesh||f.sharedMesh.triangles.Length==0)throw new Exception("Empty mesh "+name);lines.Add($"PASS {name} {f.name} triangles={f.sharedMesh.triangles.Length/3} bounds={f.sharedMesh.bounds}");}
   // FBX's standard Z-up to Y-up conversion lives on its imported mesh node.
   // Check the prefab in Unity space rather than interpreting raw mesh-local axes.
   var instance=(GameObject)PrefabUtility.InstantiatePrefab(root);try{
    var bounds=instance.GetComponentsInChildren<Renderer>(true).Single(r=>r.name.EndsWith("_LOD0")).bounds;float expected=name=="Ball"?.043f:name=="Driver"?1.14f:name=="Iron"?.94f:.89f;
    if(Mathf.Abs(bounds.size.y-expected)>.004f)throw new Exception("Equipment scale incorrect: "+name+" "+bounds.size);
    if(name=="Ball"&&bounds.center.magnitude>.00001f)throw new Exception("Ball must be centred");
    lines.Add($"PASS {name} Unity-space dimensions={bounds.size}; grip/centre pivot={instance.transform.position}");
   }finally{UnityEngine.Object.DestroyImmediate(instance);}
  }
  File.WriteAllLines("../Builds/GolfEquipment/20261001/unity-import-audit.txt",lines);
 }
 public static void BuildWindows(){Prepare();ProjectBuilder.BuildCurrentWindows();File.WriteAllText("../Builds/WindowsFinal/Review-Golf-Equipment.cmd","@echo off\r\nstart \"\" \"%~dp0WhatTheFish.exe\" -offline -sport Golf -golfEquipmentPreview -screen-fullscreen 0 -screen-width 1600 -screen-height 1000\r\n");}
}
