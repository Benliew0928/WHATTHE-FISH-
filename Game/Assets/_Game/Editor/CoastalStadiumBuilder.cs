using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WhatTheFish;

public static class CoastalStadiumBuilder {
 const string Art="Assets/_Game/Art/CoastalStadiums/";
 [Serializable] class Library{public Mat[] materials;}
 [Serializable] class Mat{public string name,base_map,normal,mask;public float roughness;}
 [Serializable] public class Layout{public string sport;public Route[] routes;public Aisle[] aisles;public Screen[] scoreboards;public float concourse_height;}
 [Serializable] public class Aisle{public float[] points;}
 [Serializable] public class Route{public float[] entry,inside,stair_bottom,stair_top,concourse,stair_path;}
 [Serializable] public class Screen{public float[] position,inward;}
 static Dictionary<string,Material> materials;
 public static Vector3 V(float[] p)=>new Vector3(p[0],p[1],p[2]);
 public static Vector3 FromSource(float[] p)=>new Vector3(-p[0],p[2],-p[1]);
 public static Layout ReadLayout(string sport)=>JsonUtility.FromJson<Layout>(File.ReadAllText(Art+sport+"-layout.json"));
 public static void Prepare(){
  if(materials!=null)return;
  AssetDatabase.Refresh();materials=new Dictionary<string,Material>();
  foreach(var file in Directory.GetFiles(Art,"*.png")){
   var ti=(TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));bool normal=file.Contains("_Normal"),mask=file.Contains("_Mask");
   ti.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;ti.sRGBTexture=!normal&&!mask;
   ti.alphaSource=TextureImporterAlphaSource.FromInput;ti.maxTextureSize=2048;ti.mipmapEnabled=true;ti.anisoLevel=8;ti.wrapMode=TextureWrapMode.Repeat;
   ti.textureCompression=TextureImporterCompression.CompressedHQ;ti.SaveAndReimport();
  }
  var lib=JsonUtility.FromJson<Library>(File.ReadAllText(Art+"materials.json"));
  foreach(var spec in lib.materials){
   var path=Art+spec.name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
   mat.SetColor("_BaseColor",Color.white);mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+spec.base_map));
   mat.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+spec.normal));mat.SetFloat("_BumpScale",.45f);mat.EnableKeyword("_NORMALMAP");
   mat.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+spec.mask));mat.EnableKeyword("_METALLICSPECGLOSSMAP");mat.SetFloat("_Smoothness",1);mat.enableInstancing=true;
   if(spec.name.EndsWith("Lamp")){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",new Color(.55f,.36f,.14f));}
   EditorUtility.SetDirty(mat);materials[spec.name]=mat;
  }
  foreach(var sport in new[]{"Football","Basketball"})foreach(var suffix in new[]{"Architecture","Collision"}){
   var importer=(ModelImporter)AssetImporter.GetAtPath(Art+sport+"_"+suffix+".fbx");
   importer.globalScale=1;importer.useFileScale=true;importer.importCameras=false;importer.importLights=false;importer.importAnimation=false;importer.animationType=ModelImporterAnimationType.None;
   importer.importNormals=ModelImporterNormals.Import;importer.importTangents=ModelImporterTangents.CalculateMikk;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.SaveAndReimport();
  }
  AssetDatabase.SaveAssets();
 }
 public static void DetailMaterial(Material material,string name){
  if(!name.StartsWith("Rally_Stadium_"))return;
  Prepare();string kind=name.EndsWith("Terraces")?"Paving":name.EndsWith("Bands")?"Bronze":"Stone";
  var detail=materials["Coastal_"+kind];
  foreach(var prop in new[]{"_BaseMap","_BumpMap","_MetallicGlossMap"})material.SetTexture(prop,detail.GetTexture(prop));
  material.SetColor("_BaseColor",Color.white);material.EnableKeyword("_NORMALMAP");material.EnableKeyword("_METALLICSPECGLOSSMAP");material.SetFloat("_BumpScale",.45f);material.SetFloat("_Smoothness",1);
 }
 public static void Attach(GameObject venue,string sport){
  Prepare();var layout=ReadLayout(sport);
  var art=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Art+sport+"_Architecture.fbx"));art.name="Coastal stadium architecture";art.transform.SetParent(venue.transform,false);
  foreach(var r in art.GetComponentsInChildren<MeshRenderer>()){
   r.sharedMaterials=r.sharedMaterials.Select(m=>materials[m.name]).ToArray();r.gameObject.layer=8;
  }
  var physics=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Art+sport+"_Collision.fbx"));physics.name="Coastal route collision";physics.transform.SetParent(venue.transform,false);
  foreach(var f in physics.GetComponentsInChildren<MeshFilter>()){
   f.gameObject.layer=8;f.gameObject.AddComponent<MeshCollider>().sharedMesh=f.sharedMesh;UnityEngine.Object.DestroyImmediate(f.GetComponent<MeshRenderer>());
  }
  // Existing terrace slabs and arches provide the remainder of the walkable shell.
  foreach(var f in venue.transform.Find("Stadium").GetComponentsInChildren<MeshFilter>()){
   if(f.name.Contains("Seats")||f.name.Contains("Canopy"))continue;
   if(!f.GetComponent<Collider>())f.gameObject.AddComponent<MeshCollider>().sharedMesh=f.sharedMesh;
  }
  var routes=venue.AddComponent<CoastalVenueRoutes>();routes.sport=sport;
  routes.entries=layout.routes.Select(r=>FromSource(r.entry)).ToArray();routes.inside=layout.routes.Select(r=>FromSource(r.inside)).ToArray();
  routes.stairBottom=layout.routes.Select(r=>FromSource(r.stair_bottom)).ToArray();routes.stairTop=layout.routes.Select(r=>FromSource(r.stair_top)).ToArray();routes.concourse=layout.routes.Select(r=>FromSource(r.concourse)).ToArray();
  routes.stairs=layout.routes.Select(r=>new CoastalVenueRoutes.Stair{points=Enumerable.Range(0,r.stair_path.Length/3).Select(i=>FromSource(new[]{r.stair_path[i*3],r.stair_path[i*3+1],r.stair_path[i*3+2]})).ToArray()}).ToArray();
  routes.aisles=layout.aisles.Select(r=>new CoastalVenueRoutes.Stair{points=Enumerable.Range(0,r.points.Length/3).Select(i=>FromSource(new[]{r.points[i*3],r.points[i*3+1],r.points[i*3+2]})).ToArray()}).ToArray();
  routes.safeReturn=routes.entries.OrderBy(p=>p.z).First()+Vector3.up*.2f;
  for(int i=0;i<routes.entries.Length;i++){
   var direction=(routes.inside[i]-routes.entries[i]).normalized;
   var sign=new GameObject("Entrance wayfinding "+(i+1),typeof(RectTransform),typeof(Canvas));sign.transform.SetParent(venue.transform,false);
   sign.transform.localPosition=routes.entries[i]+direction*6.4f+Vector3.up*(sport=="Football"?6.47f:5.14f);sign.transform.localRotation=Quaternion.LookRotation(direction);sign.transform.localScale=Vector3.one*.006f;
   sign.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;sign.GetComponent<RectTransform>().sizeDelta=new Vector2(512,80);
   var label=new GameObject("Entrance label",typeof(RectTransform),typeof(UnityEngine.UI.Text));label.transform.SetParent(sign.transform,false);
   var text=label.GetComponent<UnityEngine.UI.Text>();text.rectTransform.sizeDelta=new Vector2(512,80);text.text="ENTRANCE "+(i+1).ToString("00");text.alignment=TextAnchor.MiddleCenter;text.fontSize=64;text.color=new Color(1,.91f,.69f);text.raycastTarget=false;
   text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
  }
 }
 public static void RebuildAll(){SunvaleStadiumBuilder.Build();RallyArenaBuilder.Build();ProjectBuilder.BuildWindows();}
 public static void RebuildAndAudit(){RebuildAll();CoastalLegacyAudit.Audit();}
}
