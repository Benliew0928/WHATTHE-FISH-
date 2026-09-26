using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using WhatTheFish;

public static class CoastalIslandBuilder {
 const string Art="Assets/_Game/Art/CoastalIslands/";
 [Serializable] class Manifest{public Mat[] materials;public Island[] islands;}
 [Serializable] class Mat{public string name,base_map,normal;public float[] color_srgb;public float roughness;}
 [Serializable] class Island{public string sport;public float rx,ry,extent,dockStart;public int meshes,triangles;}
 static Dictionary<string,Material> materials;
 static Manifest manifest;
 static Material GetMaterial(string name,string shader){
  string path=Art+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(!m){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,path);}
  m.shader=Shader.Find(shader);return m;
 }
 static void Prepare(){
  DesktopPipeline();
  manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Art+"coast-manifest.json"));materials=new Dictionary<string,Material>();
  foreach(string file in Directory.GetFiles(Art,"*.png")){
   var ti=(TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));bool normal=file.Contains("Normal");
   ti.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;
   ti.sRGBTexture=!normal;ti.maxTextureSize=file.Contains("GroundColor")?4096:1024;
   ti.mipmapEnabled=true;ti.anisoLevel=8;ti.wrapMode=file.Contains("GroundColor")?TextureWrapMode.Clamp:TextureWrapMode.Repeat;
   ti.textureCompression=TextureImporterCompression.CompressedHQ;ti.SaveAndReimport();
  }
  foreach(var spec in manifest.materials){
   var m=GetMaterial(spec.name,spec.name=="Coast_Cloud"?"WhatTheFish/CoastalCloud":"Universal Render Pipeline/Lit");
   m.SetColor("_BaseColor",new Color(spec.color_srgb[0],spec.color_srgb[1],spec.color_srgb[2]));
   m.SetFloat("_Smoothness",1-spec.roughness);m.SetFloat("_Metallic",0);m.enableInstancing=true;
   if(!string.IsNullOrEmpty(spec.base_map))m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+spec.base_map));
   if(!string.IsNullOrEmpty(spec.normal)){m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+spec.normal));m.SetFloat("_BumpScale",.22f);m.EnableKeyword("_NORMALMAP");}
   if(spec.name.EndsWith("_Terrain")){
    float extent=manifest.islands.Single(i=>spec.name.StartsWith(i.sport)).extent;
    m.SetTexture("_DetailAlbedoMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"Coast_GroundDetail.png"));
    m.SetTexture("_DetailNormalMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"Coast_GroundNormal.png"));
    m.SetTextureScale("_DetailAlbedoMap",Vector2.one*extent);m.SetFloat("_DetailNormalMapScale",.5f);m.SetFloat("_DetailAlbedoMapScale",.55f);
    m.EnableKeyword("_DETAIL_SCALED");
   }
   EditorUtility.SetDirty(m);materials.Add(spec.name,m);
  }
  foreach(string file in new[]{"Football_Coast.fbx","Basketball_Coast.fbx","Cumulus.fbx"}){
   var importer=(ModelImporter)AssetImporter.GetAtPath(Art+file);importer.globalScale=1;importer.useFileScale=true;
   importer.importCameras=false;importer.importLights=false;importer.importAnimation=false;importer.animationType=ModelImporterAnimationType.None;
   importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.importNormals=ModelImporterNormals.Import;
   importer.importTangents=ModelImporterTangents.CalculateMikk;importer.isReadable=false;importer.SaveAndReimport();
  }
 }
 static void DesktopPipeline(){
  const string rendererPath="Assets/_Game/Settings/DesktopCoastRenderer.asset";
  const string pipelinePath="Assets/_Game/Resources/DesktopCoastURP.asset";
  var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
  if(!renderer){renderer=ScriptableObject.CreateInstance<UniversalRendererData>();AssetDatabase.CreateAsset(renderer,rendererPath);}
  var rendererSettings=new SerializedObject(renderer);
  rendererSettings.FindProperty("postProcessData").objectReferenceValue=null;
  rendererSettings.ApplyModifiedPropertiesWithoutUndo();
  var feature=renderer.rendererFeatures.OfType<ScreenSpaceAmbientOcclusion>().FirstOrDefault();
  if(!feature){feature=ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();feature.name="Soft contact shading";AssetDatabase.AddObjectToAsset(feature,renderer);renderer.rendererFeatures.Add(feature);}
  var settings=new SerializedObject(feature);var s=settings.FindProperty("m_Settings");
  s.FindPropertyRelative("Intensity").floatValue=1.05f;s.FindPropertyRelative("Radius").floatValue=.55f;
  s.FindPropertyRelative("DirectLightingStrength").floatValue=.16f;s.FindPropertyRelative("Falloff").floatValue=100;
  s.FindPropertyRelative("Source").enumValueIndex=0;s.FindPropertyRelative("NormalSamples").enumValueIndex=2;
  s.FindPropertyRelative("AfterOpaque").boolValue=true;
  settings.ApplyModifiedPropertiesWithoutUndo();feature.Create();EditorUtility.SetDirty(feature);EditorUtility.SetDirty(renderer);
  var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
  if(!pipeline){pipeline=UniversalRenderPipelineAsset.Create(renderer);AssetDatabase.CreateAsset(pipeline,pipelinePath);}
  pipeline.renderScale=1;pipeline.msaaSampleCount=4;pipeline.shadowDistance=230;pipeline.mainLightShadowmapResolution=4096;
  pipeline.shadowCascadeCount=4;pipeline.supportsHDR=false;pipeline.supportsCameraDepthTexture=true;
  var pipeSettings=new SerializedObject(pipeline);pipeSettings.FindProperty("m_SoftShadowsSupported").boolValue=true;pipeSettings.ApplyModifiedPropertiesWithoutUndo();
  EditorUtility.SetDirty(pipeline);AssetDatabase.SaveAssets();
 }
 public static void Attach(GameObject venue,string sport){
  if(materials==null)Prepare();
  var spec=manifest.islands.Single(i=>i.sport==sport);
  var root=new GameObject("Palm Shore Island - Walkable Coast");root.transform.SetParent(venue.transform,false);
  var marker=root.AddComponent<CoastalEnvironment>();marker.basketball=sport=="Basketball";marker.islandRadii=new Vector2(spec.rx,spec.ry);
  marker.exteriorContactShading=AssetDatabase.LoadAllAssetsAtPath("Assets/_Game/Settings/DesktopCoastRenderer.asset").OfType<ScreenSpaceAmbientOcclusion>().Single();
  marker.sky=GetMaterial("Coast_DaylightSky","WhatTheFish/CoastalSky");
  var art=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Art+sport+"_Coast.fbx"));
  art.transform.SetParent(root.transform,false);art.transform.localRotation=Quaternion.Euler(0,180,0);
  foreach(var r in art.GetComponentsInChildren<MeshRenderer>()){
   r.sharedMaterials=r.sharedMaterials.Select(m=>materials[m.name]).ToArray();r.gameObject.layer=10;
   if(r.name.Contains("Terrain_")||r.name.Contains("DockPlanks")||r.name.Contains("ArrivalStairs")||r.name.Contains("GardenBridge")||r.name.Contains("DockPiles")){
    r.gameObject.layer=8;r.gameObject.AddComponent<MeshCollider>().sharedMesh=r.GetComponent<MeshFilter>().sharedMesh;
   }
  }
  // The coastal terrain dips before the original arrival steps. Bridge that
  // transition with a visible stone ramp, rather than an invisible high step.
  float approachZ=spec.dockStart+7;float approachY=-.42f;
  foreach(var ground in art.GetComponentsInChildren<MeshCollider>().Where(c=>c.name.Contains("Terrain_")))
   if(ground.Raycast(new Ray(new Vector3(0,5,approachZ),Vector3.down),out var hit,10))approachY=hit.point.y+.015f;
  var arrivalMesh=new Mesh{name=sport+" arrival terrain transition"};
  arrivalMesh.vertices=new[]{new Vector3(-3.2f,approachY,approachZ),new Vector3(3.2f,approachY,approachZ),new Vector3(3.2f,-.37f,spec.dockStart+3.2f),new Vector3(-3.2f,-.37f,spec.dockStart+3.2f)};
  arrivalMesh.uv=new[]{new Vector2(0,0),new Vector2(1.6f,0),new Vector2(1.6f,1.9f),new Vector2(0,1.9f)};
  arrivalMesh.triangles=new[]{0,1,2,0,2,3};arrivalMesh.RecalculateNormals();arrivalMesh.RecalculateBounds();
  string arrivalPath=Art+sport+"_ArrivalApproach.asset";var oldArrival=AssetDatabase.LoadAssetAtPath<Mesh>(arrivalPath);
  if(oldArrival){EditorUtility.CopySerialized(arrivalMesh,oldArrival);UnityEngine.Object.DestroyImmediate(arrivalMesh);arrivalMesh=oldArrival;}else AssetDatabase.CreateAsset(arrivalMesh,arrivalPath);
  var arrival=new GameObject("Arrival dock stone approach");arrival.transform.SetParent(root.transform,false);arrival.layer=8;
  arrival.AddComponent<MeshFilter>().sharedMesh=arrivalMesh;arrival.AddComponent<MeshRenderer>().sharedMaterial=materials["Coast_Limestone"];
  arrival.AddComponent<MeshCollider>().sharedMesh=arrivalMesh;
  foreach(float side in new[]{-1f,1f}){
   float x=spec.rx*.47f,z=-spec.ry*.66f;
   var mesh=new Mesh{name=sport+" garden bridge approach "+side};
   mesh.vertices=new[]{new Vector3(x+side*5.73f,.30f,z-1.48f),new Vector3(x+side*5.73f,.30f,z+1.48f),new Vector3(x+side*8.8f,-.39f,z+1.48f),new Vector3(x+side*8.8f,-.39f,z-1.48f)};
   mesh.uv=new[]{new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(1,0)};mesh.triangles=side>0?new[]{0,1,2,0,2,3}:new[]{2,1,0,3,2,0};mesh.RecalculateNormals();mesh.RecalculateBounds();
   string path=Art+sport+"_BridgeApproach_"+(side>0?"East":"West")+".asset";var prior=AssetDatabase.LoadAssetAtPath<Mesh>(path);
   if(prior){EditorUtility.CopySerialized(mesh,prior);UnityEngine.Object.DestroyImmediate(mesh);mesh=prior;}else AssetDatabase.CreateAsset(mesh,path);
   var ramp=new GameObject("Garden bridge ramp "+side);ramp.transform.SetParent(root.transform,false);ramp.layer=8;
   ramp.AddComponent<MeshFilter>().sharedMesh=mesh;ramp.AddComponent<MeshRenderer>().sharedMaterial=materials["Coast_HoneyTimber"];ramp.AddComponent<MeshCollider>().sharedMesh=mesh;
  }
  var sea=GameObject.CreatePrimitive(PrimitiveType.Plane);sea.name="Animated ocean and shore wash";UnityEngine.Object.DestroyImmediate(sea.GetComponent<Collider>());
  sea.transform.SetParent(root.transform,false);sea.transform.localPosition=new Vector3(0,-1.8f,0);sea.transform.localScale=new Vector3(600,1,600);
  var seaMat=GetMaterial(sport+"_Ocean","WhatTheFish/CoastalWater");seaMat.SetVector("_Radii",new Vector4(spec.rx,spec.ry,0,0));EditorUtility.SetDirty(seaMat);
  sea.GetComponent<Renderer>().sharedMaterial=seaMat;sea.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;sea.layer=10;
  var cloud=AssetDatabase.LoadAssetAtPath<GameObject>(Art+"Cumulus.fbx");var random=new System.Random(926);
  var skyRoot=new GameObject("Sculpted cumulus cloud field");skyRoot.transform.SetParent(root.transform,false);
  for(int i=0;i<32;i++){
   var c=new GameObject("Cumulus_"+i.ToString("00"));c.transform.SetParent(skyRoot.transform,false);
   var model=(GameObject)PrefabUtility.InstantiatePrefab(cloud);model.transform.SetParent(c.transform,false);
   float a=i*Mathf.PI*2/32;float radius=650+(float)random.NextDouble()*650;
   c.transform.localPosition=new Vector3(Mathf.Cos(a)*radius,110+(float)random.NextDouble()*110,Mathf.Sin(a)*radius);
   c.transform.localRotation=Quaternion.Euler(0,i*71,0);float s=11+(float)random.NextDouble()*12;c.transform.localScale=new Vector3(s,s*.75f,s*.88f);
   foreach(var r in c.GetComponentsInChildren<Renderer>()){r.sharedMaterial=materials["Coast_Cloud"];r.shadowCastingMode=ShadowCastingMode.Off;r.gameObject.layer=10;}
   if(i==0)Debug.Log("CLOUD_IMPORT_BOUNDS "+c.GetComponentInChildren<Renderer>().bounds);
  }
  AssetDatabase.SaveAssets();
  Debug.Log("COASTAL_ISLAND_ATTACHED "+sport+" meshes="+spec.meshes+" triangles="+spec.triangles+" walkable coastal terrain, dock and bridge");
 }
}
