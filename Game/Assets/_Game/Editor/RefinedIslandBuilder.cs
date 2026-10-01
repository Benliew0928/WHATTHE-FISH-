using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using WhatTheFish;

public static class RefinedIslandBuilder {
 const string Art="Assets/_Game/Art/RefinedIslands/",Shared=Art+"Shared/",Prefabs="Assets/_Game/Prefabs/RefinedIslands/";
 [Serializable] class Materials {public MaterialSpec[] materials;}
 [Serializable] class MaterialSpec {public string name,base_map,normal,mask;public float roughness,tile_metres;public bool foliage;}
 [Serializable] class Kit {public Module[] modules;}
 [Serializable] class Module {public string name,file,collision;public bool lods;public Box[] boxes;}
 [Serializable] class Box {public float[] center,size;}
 static Dictionary<string,Material> materials;static Dictionary<string,GameObject> modules;
 public static IslandLayout Read(string sport)=>JsonUtility.FromJson<IslandLayout>(File.ReadAllText(Art+sport+"/layout.json"));
 static Material Material(string path,string shader){
  var s=Shader.Find(shader);if(!s)throw new Exception("Missing shader "+shader);
  var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(s);AssetDatabase.CreateAsset(m,path);}m.shader=s;return m;
 }
 static void Prepare(){
  Directory.CreateDirectory(Shared+"Materials");Directory.CreateDirectory(Prefabs);AssetDatabase.Refresh();
  foreach(var file in Directory.GetFiles(Art,"*.png",SearchOption.AllDirectories)){
   string p=file.Replace('\\','/');var t=(TextureImporter)AssetImporter.GetAtPath(p);bool normal=p.Contains("_Normal"),linear=normal||p.Contains("_Mask")||p.Contains("Control")||p.Contains("Depth");
   t.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;t.sRGBTexture=!linear;t.mipmapEnabled=true;t.anisoLevel=16;
   t.maxTextureSize=p.Contains("Terrain")?4096:2048;t.wrapMode=p.Contains("/Shared/")?TextureWrapMode.Repeat:TextureWrapMode.Clamp;
   t.alphaSource=p.Contains("_Mask")?TextureImporterAlphaSource.FromInput:TextureImporterAlphaSource.None;
   t.textureCompression=p.Contains("Depth")||p.Contains("Control")?TextureImporterCompression.Uncompressed:TextureImporterCompression.CompressedHQ;t.SaveAndReimport();
  }
  materials=new Dictionary<string,Material>();
  foreach(var s in JsonUtility.FromJson<Materials>(File.ReadAllText(Shared+"materials.json")).materials){
   var m=Material(Shared+"Materials/"+s.name+".mat",s.foliage?"WhatTheFish/RefinedFoliage":s.name=="RI_Stone"?"WhatTheFish/RefinedStone":"Universal Render Pipeline/Lit");m.SetColor("_BaseColor",Color.white);
   m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Shared+s.base_map));m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Shared+s.normal));
   if(s.name=="LG_Accent")m.SetTexture("_BaseMap",Texture2D.whiteTexture);
   // RefinedStone uses fixed smoothness and never samples the metallic mask.
   m.SetTexture("_MetallicGlossMap",s.name=="RI_Stone"?null:AssetDatabase.LoadAssetAtPath<Texture2D>(Shared+s.mask));m.SetFloat("_Smoothness",1);m.SetFloat("_BumpScale",.5f);
   m.EnableKeyword("_NORMALMAP");m.EnableKeyword("_METALLICSPECGLOSSMAP");m.SetFloat("_Cull",s.foliage?0:2);m.doubleSidedGI=s.foliage;m.enableInstancing=true;
   EditorUtility.SetDirty(m);materials.Add(s.name,m);
  }
  foreach(string p in Directory.GetFiles(Art,"*.fbx",SearchOption.AllDirectories)){
   var i=(ModelImporter)AssetImporter.GetAtPath(p.Replace('\\','/'));i.globalScale=1;i.useFileScale=true;i.preserveHierarchy=true;
   i.importCameras=false;i.importLights=false;i.importAnimation=false;i.animationType=ModelImporterAnimationType.None;i.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
   i.importNormals=ModelImporterNormals.Import;i.importTangents=ModelImporterTangents.CalculateMikk;i.isReadable=false;i.SaveAndReimport();
  }
  modules=new Dictionary<string,GameObject>();
  foreach(var s in JsonUtility.FromJson<Kit>(File.ReadAllText(Shared+"kit.json")).modules){
   var root=new GameObject(s.name);var art=Model(Shared+s.file,root,materials,false);
   if(s.lods){
    var lod=root.AddComponent<LODGroup>();var renderers=art.GetComponentsInChildren<Renderer>();
    lod.SetLODs(new[]{new LOD(.16f,renderers.Where(r=>r.name.StartsWith("LOD0")).ToArray()),new LOD(.055f,renderers.Where(r=>r.name.StartsWith("LOD1")).ToArray()),new LOD(.008f,renderers.Where(r=>r.name.StartsWith("LOD2")).ToArray())});lod.RecalculateBounds();
   }
   if(s.collision=="rock")foreach(var f in art.GetComponentsInChildren<MeshFilter>().Where(f=>f.name.StartsWith("LOD1"))){f.gameObject.layer=8;f.gameObject.AddComponent<MeshCollider>().sharedMesh=f.sharedMesh;}
   foreach(var b in s.boxes)BoxCollider(root,"Support collision",new Vector3(b.center[0],b.center[1],b.center[2]),new Vector3(b.size[0],b.size[1],b.size[2]));
   if(s.name=="PlayerStand"){
    var marker=root.AddComponent<FishingModule>();marker.moduleType="PlayerStand";marker.slotId="PlayerStand";
    foreach(float x in new[]{-2.95f,2.95f})BoxCollider(root,"Pier railing",new Vector3(x,.5f,3.5f),new Vector3(.16f,1.1f,7));
   }
   if(s.name=="InletBridge")foreach(float z in new[]{-2.05f,2.05f})BoxCollider(root,"Bridge railing",new Vector3(0,.55f,z),new Vector3(13,1.1f,.18f));
   if(s.name.EndsWith("Pavilion")||s.name.EndsWith("Shelter")){
    bool golf=s.name=="GolfPavilion";float w=golf?10:7,d=golf?7:4;
    foreach(float x in new[]{-w*.45f,w*.45f})foreach(float z in new[]{-d*.42f,d*.42f})BoxCollider(root,"Timber column",new Vector3(x,1.55f,z),new Vector3(.32f,3.1f,.32f));
    // Actual joinery, benches and roof also block camera and player movement.
    foreach(var f in art.GetComponentsInChildren<MeshFilter>()){f.gameObject.layer=8;f.gameObject.AddComponent<MeshCollider>().sharedMesh=f.sharedMesh;}
   }
   modules.Add(s.name,PrefabUtility.SaveAsPrefabAsset(root,Prefabs+s.name+".prefab"));UnityEngine.Object.DestroyImmediate(root);
  }
  AssetDatabase.SaveAssets();
 }
 static void BoxCollider(GameObject root,string name,Vector3 center,Vector3 size){var g=new GameObject(name);g.transform.SetParent(root.transform,false);g.layer=8;var c=g.AddComponent<BoxCollider>();c.center=center;c.size=size;}
 static GameObject Model(string path,GameObject parent,Dictionary<string,Material> map,bool collision){
  var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!source)throw new Exception("Missing island art "+path);
  var g=(GameObject)PrefabUtility.InstantiatePrefab(source);g.transform.SetParent(parent.transform,false);g.transform.localRotation=Quaternion.Euler(0,180,0);
  foreach(var f in g.GetComponentsInChildren<MeshFilter>()){
   f.gameObject.layer=collision?8:10;var r=f.GetComponent<MeshRenderer>();r.sharedMaterials=r.sharedMaterials.Select(m=>map[m.name]).ToArray();
   if(collision)f.gameObject.AddComponent<MeshCollider>().sharedMesh=f.sharedMesh;
  }
  return g;
 }
 public static GameObject Build(string sport){
  if(materials==null)Prepare();var data=Read(sport);string dir=Art+sport+"/";var root=new GameObject(data.name+" - "+data.concept);
  var map=new Dictionary<string,Material>(materials);var terrain=Material(dir+sport+"_Terrain.mat","WhatTheFish/RefinedTerrain");
  terrain.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(dir+sport+"_TerrainColor.png"));terrain.SetTexture("_Control",AssetDatabase.LoadAssetAtPath<Texture2D>(dir+sport+"_TerrainControl.png"));
  foreach(string s in new[]{"Turf","Sand"}){terrain.SetTexture("_"+s,materials["RI_"+s].GetTexture("_BaseMap"));terrain.SetTexture("_"+s+"Normal",materials["RI_"+s].GetTexture("_BumpMap"));}
  terrain.SetFloat("_Tile",4);EditorUtility.SetDirty(terrain);map.Add("RI_"+sport+"_Terrain",terrain);
  Model(dir+data.terrain_file,root,map,true);Model(dir+data.structures_file,root,map,true);
  var grass=Model(dir+"Grass.fbx",root,map,false);
  foreach(var cell in grass.GetComponentsInChildren<Renderer>().GroupBy(r=>System.Text.RegularExpressions.Regex.Match(r.name,@"Cell_\d+_\d+").Value)){
   var batch=new GameObject(cell.Key);batch.transform.SetParent(grass.transform,false);foreach(var r in cell)r.transform.SetParent(batch.transform,true);
   var lod=batch.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.3f,cell.Where(r=>r.name.StartsWith("LOD0")).ToArray()),new LOD(.16f,cell.Where(r=>r.name.StartsWith("LOD1")).ToArray()),new LOD(.08f,cell.Where(r=>r.name.StartsWith("LOD2")).ToArray())});lod.RecalculateBounds();
  }
  var stands=new List<FishingModule>();var plants=new List<GameObject>();
  foreach(var p in data.instances){
   if(p.module.StartsWith("Grass"))continue;
   var g=(GameObject)PrefabUtility.InstantiatePrefab(modules[p.module]);g.name=p.id;g.transform.SetParent(root.transform,false);g.transform.localPosition=p.position;g.transform.localRotation=Quaternion.Euler(0,p.yaw,0);g.transform.localScale=Vector3.one*p.scale;
   if(p.module=="PlayerStand"){var m=g.GetComponent<FishingModule>();m.slotId=p.id;m.SetAccent(LocalProfile.Hex(p.accent));stands.Add(m);}
   if(p.id.StartsWith("TerracePlant")||p.id.StartsWith("TerraceGarden"))plants.Add(g);
  }
  // Snap planted rock ledges to the real support geometry, including irregular rock crowns.
  Physics.SyncTransforms();foreach(var p in plants)if(Physics.Raycast(p.transform.position+Vector3.up*35,Vector3.down,out var hit,100,1<<8))p.transform.position=hit.point-Vector3.up*.015f;
  if(sport=="Golf"){root.AddComponent<GolfIslandView>();GolfEquipmentBuilder.Attach(root);}else{var v=root.AddComponent<FishingLagoonView>();v.playerStands=stands.ToArray();v.standingPositions=data.stands;FishingRodBuilder.Attach(root);}
  if(data.signs!=null)foreach(var s in data.signs){
   var canvas=new GameObject("Wayfinding "+s.title,typeof(RectTransform),typeof(Canvas));canvas.transform.SetParent(root.transform,false);canvas.transform.localPosition=s.position;canvas.transform.localRotation=Quaternion.Euler(0,s.yaw,0);canvas.transform.localScale=Vector3.one*.004f;canvas.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;canvas.GetComponent<RectTransform>().sizeDelta=new Vector2(s.width/.004f,140);
   var label=new GameObject("Label",typeof(RectTransform),typeof(UnityEngine.UI.Text));label.transform.SetParent(canvas.transform,false);var t=label.GetComponent<UnityEngine.UI.Text>();t.rectTransform.sizeDelta=new Vector2(s.width/.004f,140);t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=44;t.alignment=TextAnchor.MiddleCenter;t.color=new Color(1,.94f,.79f);t.text=s.title+"\n<size=25>"+s.caption+"</size>";t.raycastTarget=false;
  }
  var boundary=new GameObject("Shoreline containment");boundary.transform.SetParent(root.transform,false);
  foreach(var e in data.boundaries){var d=e.b-e.a;d.y=0;if(d.sqrMagnitude<.0001f)continue;var g=new GameObject("Water boundary");g.layer=9;g.transform.SetParent(boundary.transform,false);g.transform.localPosition=(e.a+e.b)*.5f;g.transform.localRotation=Quaternion.LookRotation(d);g.AddComponent<BoxCollider>().size=new Vector3(.14f,6,d.magnitude+.035f);}
  var water=Material(dir+sport+"_Ocean.mat","WhatTheFish/RefinedWater");water.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(dir+sport+"_WaterColor.png"));water.SetTexture("_DepthMap",AssetDatabase.LoadAssetAtPath<Texture2D>(dir+sport+"_WaterDepth.png"));water.SetFloat("_Extent",data.water_extent);water.SetFloat("_Calm",sport=="Fishing"?.28f:.7f);EditorUtility.SetDirty(water);
  var sea=GameObject.CreatePrimitive(PrimitiveType.Plane);sea.name="Coastal sea with shore wash";UnityEngine.Object.DestroyImmediate(sea.GetComponent<Collider>());sea.transform.SetParent(root.transform,false);sea.transform.localPosition=Vector3.up*data.sea_level;sea.transform.localScale=new Vector3(600,1,600);sea.layer=10;sea.GetComponent<Renderer>().sharedMaterial=water;sea.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
  if(!string.IsNullOrEmpty(data.cascade_file)){
   var flow=Material(dir+"Golf_Cascade.mat","WhatTheFish/RefinedWater");flow.CopyPropertiesFromMaterial(water);flow.SetFloat("_Cascade",1);EditorUtility.SetDirty(flow);
   var cascade=Model(dir+data.cascade_file,root,new Dictionary<string,Material>{{"RI_Teal",flow}},false);
   foreach(var r in cascade.GetComponentsInChildren<Renderer>()){r.shadowCastingMode=ShadowCastingMode.Off;if(r.name.Contains("SpringPool"))r.sharedMaterial=water;}
  }
  var env=root.AddComponent<RefinedIslandEnvironment>();env.layout=data;env.lighting=new IslandLightingProfile();
  env.lighting.sky=Material(dir+sport+"_DaylightSky.mat","WhatTheFish/CoastalSky");env.lighting.intensity=sport=="Fishing"?1.12f:1.18f;
  env.contactShading=AssetDatabase.LoadAllAssetsAtPath("Assets/_Game/Settings/DesktopCoastRenderer.asset").OfType<ScreenSpaceAmbientOcclusion>().Single();
  var clouds=Material(dir+sport+"_Cloud.mat","WhatTheFish/CoastalCloud");clouds.CopyPropertiesFromMaterial(AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/CoastalIslands/Coast_Cloud.mat"));EditorUtility.SetDirty(clouds);
  var random=new System.Random(sport=="Golf"?927:928);var skyRoot=new GameObject("Sculpted cloud field");skyRoot.transform.SetParent(root.transform,false);
  for(int i=0;i<32;i++){
   var c=new GameObject("Cumulus_"+i);c.transform.SetParent(skyRoot.transform,false);
   var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/CoastalIslands/Cumulus.fbx"));model.transform.SetParent(c.transform,false);
   float a=i*Mathf.PI*2/32,r=650+(float)random.NextDouble()*600,s=11+(float)random.NextDouble()*12;c.transform.localPosition=new Vector3(Mathf.Cos(a)*r,120+(float)random.NextDouble()*100,Mathf.Sin(a)*r);c.transform.localRotation=Quaternion.Euler(0,i*71,0);c.transform.localScale=new Vector3(s,s*.75f,s*.88f);
   foreach(var renderer in c.GetComponentsInChildren<Renderer>()){renderer.sharedMaterial=clouds;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.gameObject.layer=10;}
  }
  AssetDatabase.SaveAssets();Debug.Log("REFINED_ISLAND_BUILT "+sport+" modules="+data.instances.Length+" routes="+data.routes.Length);return root;
 }
}
