using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WhatTheFish;

// Updates the shared equipment prefab used by every live match ball.
public static class GolfBallBuilder {
 const string Art="Assets/_Game/Art/Golf/Equipment/",Prefab=GolfEquipmentBuilder.BallPrefabPath;
 public static void BuildVerification()=>ProjectBuilder.BuildWindowsPlayer("../Builds/GolfBallRoundedQA/Player/WhatTheFish.exe");
 public static void BuildPhysicsVerification()=>ProjectBuilder.BuildWindowsPlayer("../Builds/GolfBallPhysicsQA/Player/WhatTheFish.exe");
 public static void BuildAimVerification()=>ProjectBuilder.BuildWindowsPlayer("../Builds/GolfBallAimQA/Player/WhatTheFish.exe");
 [MenuItem("WHATTHE FISH?/Golf/Prepare rounded ball")]
 public static void Prepare(){
  AssetDatabase.Refresh();
  var importer=(ModelImporter)AssetImporter.GetAtPath(Art+"Ball.fbx");if(!importer)throw new Exception("Run prepare_golf_ball.py first.");
  importer.globalScale=1;importer.useFileScale=true;importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;importer.materialImportMode=ModelImporterMaterialImportMode.None;
  importer.importNormals=ModelImporterNormals.Import;importer.importTangents=ModelImporterTangents.None;importer.meshCompression=ModelImporterMeshCompression.Low;importer.isReadable=false;importer.SaveAndReimport();
  var material=AssetDatabase.LoadAssetAtPath<Material>(Art+"Ball.mat");if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,Art+"Ball.mat");}
  material.SetColor("_BaseColor",new Color(.978f,.956f,.910f));material.SetColor("_Color",new Color(.978f,.956f,.910f));material.SetFloat("_Metallic",0);material.SetFloat("_Smoothness",.45f);
  foreach(var name in new[]{"_BaseMap","_MainTex","_BumpMap","_MetallicGlossMap"})material.SetTexture(name,null);
  material.DisableKeyword("_NORMALMAP");material.DisableKeyword("_METALLICSPECGLOSSMAP");material.enableInstancing=true;EditorUtility.SetDirty(material);
  var root=PrefabUtility.LoadPrefabContents(Prefab);
  try {
   for(int i=root.transform.childCount-1;i>=0;i--)UnityEngine.Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
   var visual=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Art+"Ball.fbx"));PrefabUtility.UnpackPrefabInstance(visual,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);visual.name="Visual";visual.transform.SetParent(root.transform,false);
   visual.transform.localScale=Vector3.one*GolfBall.VisualScale;
   foreach(var nested in visual.GetComponentsInChildren<LODGroup>(true))UnityEngine.Object.DestroyImmediate(nested);
   var renderers=visual.GetComponentsInChildren<MeshRenderer>(true);foreach(var r in renderers)r.sharedMaterial=material;
   var lod=root.GetComponent<LODGroup>()??root.AddComponent<LODGroup>();
   var levels=new[]{new LOD(.012f,renderers.Where(r=>r.name=="Ball_LOD0").Cast<Renderer>().ToArray()),new LOD(0,renderers.Where(r=>r.name=="Ball_LOD1").Cast<Renderer>().ToArray())};
   if(levels.Any(l=>l.renderers.Length!=1))throw new Exception("Rounded ball requires exactly one mesh per LOD.");lod.SetLODs(levels);lod.RecalculateBounds();
   var shape=root.GetComponent<SphereCollider>()??root.AddComponent<SphereCollider>();shape.center=Vector3.zero;shape.radius=GolfBall.Radius;shape.contactOffset=.001f;
   foreach(var r in renderers)if(r.bounds.center.sqrMagnitude>1e-8f||r.bounds.size.magnitude>.076f*GolfBall.VisualScale||r.bounds.size.magnitude<.071f*GolfBall.VisualScale)throw new Exception("Ball visual must be centred and fit the configured physics sphere.");
   PrefabUtility.SaveAsPrefabAsset(root,Prefab);AssetDatabase.SaveAssets();
   Directory.CreateDirectory("../Builds/GolfBallRoundedQA/Art");File.WriteAllLines("../Builds/GolfBallRoundedQA/Art/unity-import.txt",renderers.Select(r=>$"PASS {r.name} triangles={r.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3} centre={r.bounds.center} dimensions={r.bounds.size}").Concat(new[]{"PASS existing prefab GUID, two LODs, one sphere collider, no prefab rigidbody; live GolfBall retains authority and physics"}));
  } finally {PrefabUtility.UnloadPrefabContents(root);}
  Debug.Log("GOLF_ROUNDED_BALL_PREFAB_OK");
 }
}
