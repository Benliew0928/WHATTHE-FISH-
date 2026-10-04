using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using WhatTheFish;

public static class LagoonPresentationBuilder {
 const string Art="Assets/_Game/Art/Fishing/",FishArt=Art+"Fish/",Prefabs="Assets/_Game/Prefabs/Fishing/";
 public const string Group="Lagoon presentation";
 public static readonly string[] Sizes={"Small","Medium","Large"};
 public static readonly float[] Lengths={.4f,.9f,1.8f};
 [MenuItem("WHATTHE FISH?/Fishing/Prepare transparent lagoon and fish")]
 public static void Prepare(){
  Directory.CreateDirectory(FishArt);Directory.CreateDirectory(Prefabs);AssetDatabase.Refresh();
  var importer=AssetImporter.GetAtPath(FishArt+"LagoonFish.fbx") as ModelImporter;
  if(!importer)throw new Exception("Run Tools/Blender/build_lagoon_fish.py first.");
  importer.globalScale=1;importer.useFileScale=true;importer.importAnimation=false;
  importer.importCameras=false;importer.importLights=false;importer.materialImportMode=ModelImporterMaterialImportMode.None;
  importer.importNormals=ModelImporterNormals.Import;importer.importTangents=ModelImporterTangents.None;
  importer.meshCompression=ModelImporterMeshCompression.Low;importer.isReadable=false;importer.SaveAndReimport();
  var material=Material(FishArt+"LagoonFish.mat","WhatTheFish/LagoonFish");material.enableInstancing=true;
  var model=AssetDatabase.LoadAssetAtPath<GameObject>(FishArt+"LagoonFish.fbx");
  foreach(int index in Enumerable.Range(0,3)){
   var root=new GameObject(Sizes[index]+" lagoon fish");
   var visual=(GameObject)PrefabUtility.InstantiatePrefab(model);visual.name="Visual";visual.transform.SetParent(root.transform,false);
   var renderers=visual.GetComponentsInChildren<MeshRenderer>(true);
   foreach(var r in renderers){r.enabled=true;r.gameObject.SetActive(true);r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.gameObject.layer=10;}
   var near=renderers.Single(r=>r.name.EndsWith("LOD0"));
   float length=near.bounds.size.z;
   if(length<.9f||length>1.1f)throw new Exception("Expected a one-metre +Z fish. Imported bounds: "+near.bounds.size);
   visual.transform.localScale=Vector3.one*(Lengths[index]/length);
   var lod=root.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.035f,new Renderer[]{near}),new LOD(.002f,renderers.Where(r=>r.name.EndsWith("LOD1")).Cast<Renderer>().ToArray())});lod.RecalculateBounds();
   PrefabUtility.SaveAsPrefabAsset(root,Prefabs+Sizes[index]+"LagoonFish.prefab");UnityEngine.Object.DestroyImmediate(root);
  }
  SaveMesh(Disc("Lagoon surface",28,32,96,false),Art+"LagoonSurface.asset");
  SaveMesh(Disc("Lagoon sandy basin",30,12,64,true),Art+"LagoonBed.asset");
  var water=Material(Art+"LagoonWater.mat","WhatTheFish/LagoonWater");
  water.SetTexture("_ShoreMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/RefinedIslands/Fishing/Fishing_WaterColor.png"));EditorUtility.SetDirty(water);
  var sand=Material(Art+"LagoonBed.mat","WhatTheFish/LagoonBed");
  var sharedSand=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/RefinedIslands/Shared/Materials/RI_Sand.mat");
  sand.SetTexture("_BaseMap",sharedSand.GetTexture("_BaseMap"));sand.SetColor("_BaseColor",new Color(.32f,.74f,.71f,1));
  // RefinedTerrain already ships this exact sand map. A delivery colour-pattern
  // variant would add a second texture instead of sharing the existing one.
  sand.SetOverrideTag("KeepSourceTexture","True");EditorUtility.SetDirty(sand);
  var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_Fishing.unity");
  Attach(scene.GetRootGameObjects().Single(r=>r.GetComponent<SkySailIslandScene>()));EditorSceneManager.SaveScene(scene);
  const string environmentPath="Assets/_Game/Prefabs/Environments/FishingEnvironment.prefab";
  var environment=PrefabUtility.LoadPrefabContents(environmentPath);
  try{Attach(environment);PrefabUtility.SaveAsPrefabAsset(environment,environmentPath);}finally{PrefabUtility.UnloadPrefabContents(environment);}
  AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");
  Debug.Log("LAGOON_PRESENTATION_PREPARE_OK");
 }
 static Material Material(string path,string shader){
  var material=AssetDatabase.LoadAssetAtPath<Material>(path);var s=Shader.Find(shader);if(!s)throw new Exception("Missing shader: "+shader);
  if(!material){material=new Material(s);AssetDatabase.CreateAsset(material,path);}else material.shader=s;
  EditorUtility.SetDirty(material);return material;
 }
 static void SaveMesh(Mesh mesh,string path){
  var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
  if(existing){EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);}else AssetDatabase.CreateAsset(mesh,path);
 }
 static Mesh Disc(string name,float radius,int rings,int segments,bool bed){
  var vertices=new List<Vector3>{new Vector3(0,bed?-2.2f:0,0)};var triangles=new List<int>();
  for(int j=1;j<=rings;j++){
   float r=radius*j/rings;
   for(int i=0;i<segments;i++){
    float a=i*Mathf.PI*2/segments;float x=Mathf.Cos(a)*r,z=Mathf.Sin(a)*r;
    float y=bed?Mathf.Lerp(-2.2f,-.24f,Mathf.Pow(r/radius,3))+.035f*Mathf.Sin(x*.7f)*Mathf.Sin(z*.8f):0;
    vertices.Add(new Vector3(x,y,z));
   }
  }
  for(int i=0;i<segments;i++)triangles.AddRange(new[]{0,1+(i+1)%segments,1+i});
  for(int j=1;j<rings;j++)for(int i=0;i<segments;i++){
   int a=1+(j-1)*segments+i,b=1+(j-1)*segments+(i+1)%segments,c=a+segments,d=b+segments;
   triangles.AddRange(new[]{a,b,d,a,d,c});
  }
  var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
  var bounds=mesh.bounds;bounds.Expand(.12f);mesh.bounds=bounds;return mesh;
 }
 public static void Attach(GameObject island){
  var surface=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"LagoonSurface.asset");if(!surface)throw new Exception("Prepare lagoon presentation assets before rebuilding fishing.");
  var old=island.transform.Find(Group);if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
  var root=new GameObject(Group);root.transform.SetParent(island.transform,false);root.AddComponent<LagoonPresentation>();
  float waterLevel=island.GetComponent<SkySailIslandScene>()?-.43f:-.08f;
  AddMesh(root,"Transparent moving lagoon",surface,Art+"LagoonWater.mat",Vector3.up*(waterLevel+.004f));
  AddMesh(root,"Sandy lagoon bed",AssetDatabase.LoadAssetAtPath<Mesh>(Art+"LagoonBed.asset"),Art+"LagoonBed.mat",Vector3.up*waterLevel);
  // An art comparison group beside the first pier: identical asset, visibly different lengths.
  // Static placement and shader tail sway are visual only, with no gameplay state.
  var fish=new GameObject("Fish size comparison");fish.transform.SetParent(root.transform,false);
  for(int i=0;i<3;i++){
   var source=AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+Sizes[i]+"LagoonFish.prefab");
   if(!source)throw new Exception("Missing "+Sizes[i]+" fish prefab.");
   var instance=(GameObject)PrefabUtility.InstantiatePrefab(source);instance.name=Sizes[i]+" fish";instance.transform.SetParent(fish.transform,false);
   instance.transform.localPosition=new Vector3(4+(i-1)*1.6f,waterLevel-.7f,-15.5f);
   instance.transform.localRotation=Quaternion.Euler(0,75,0);
  }
 }
 static void AddMesh(GameObject root,string name,Mesh mesh,string mat,Vector3 position){
  var g=new GameObject(name);g.layer=10;g.transform.SetParent(root.transform,false);g.transform.localPosition=position;
  g.AddComponent<MeshFilter>().sharedMesh=mesh;var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(mat);r.shadowCastingMode=ShadowCastingMode.Off;
 }
 public static void BuildWindows(){Prepare();ProjectBuilder.BuildCurrentWindows();}
}
