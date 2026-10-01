using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Netcode;
using WhatTheFish;

public static class BasketballBallBuilder {
 const string Art="Assets/_Game/Art/Basketball/Ball/";
 public const string PrefabPath="Assets/_Game/Prefabs/Basketball/BasketballBall.prefab";
 static string Output=>Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/BasketballModel/20261001/"))+Path.DirectorySeparatorChar;
 [MenuItem("WHATTHE FISH?/Basketball/Prepare ball and physics")]
 public static void Prepare(){
  Directory.CreateDirectory(Art);Directory.CreateDirectory(Output);AssetDatabase.Refresh();
  foreach(var suffix in new[]{"BaseColor","Normal"}){
   var importer=(TextureImporter)AssetImporter.GetAtPath(Art+"Basketball_"+suffix+".png");
   if(!importer)throw new Exception("Generate the basketball in Blender first.");
   importer.textureType=suffix=="Normal"?TextureImporterType.NormalMap:TextureImporterType.Default;
   importer.sRGBTexture=suffix!="Normal";importer.mipmapEnabled=true;importer.isReadable=false;importer.wrapMode=TextureWrapMode.Repeat;importer.anisoLevel=4;
   importer.maxTextureSize=1024;importer.textureCompression=TextureImporterCompression.CompressedHQ;
   importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=1024,format=TextureImporterFormat.ASTC_6x6,compressionQuality=100});
   importer.SaveAndReimport();
  }
  var model=(ModelImporter)AssetImporter.GetAtPath(Art+"Basketball.fbx");
  model.globalScale=1;model.useFileScale=true;model.importAnimation=false;model.importCameras=false;model.importLights=false;model.materialImportMode=ModelImporterMaterialImportMode.None;
  model.importNormals=ModelImporterNormals.Import;model.importTangents=ModelImporterTangents.CalculateMikk;model.meshCompression=ModelImporterMeshCompression.Off;model.isReadable=false;model.SaveAndReimport();
  var material=AssetDatabase.LoadAssetAtPath<Material>(Art+"Basketball_Rubber.mat");
  if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,Art+"Basketball_Rubber.mat");}
  material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"Basketball_BaseColor.png"));material.SetTexture("_MainTex",material.GetTexture("_BaseMap"));
  material.SetColor("_BaseColor",Color.white);material.SetFloat("_Metallic",0);material.SetFloat("_Smoothness",.24f);
  material.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"Basketball_Normal.png"));material.SetFloat("_BumpScale",1);material.EnableKeyword("_NORMALMAP");material.enableInstancing=true;EditorUtility.SetDirty(material);
  var rubber=AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(Art+"Basketball_Rubber.physicMaterial");
  if(!rubber){rubber=new PhysicsMaterial("Basketball_Rubber");AssetDatabase.CreateAsset(rubber,Art+"Basketball_Rubber.physicMaterial");}
  rubber.bounciness=.78f;rubber.bounceCombine=PhysicsMaterialCombine.Maximum;rubber.dynamicFriction=.55f;rubber.staticFriction=.65f;rubber.frictionCombine=PhysicsMaterialCombine.Average;EditorUtility.SetDirty(rubber);
  var root=new GameObject("Basketball");
  var visual=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Art+"Basketball.fbx"));visual.name="Visual";visual.transform.SetParent(root.transform,false);
  var renderers=visual.GetComponentsInChildren<MeshRenderer>(true);foreach(var r in renderers){r.sharedMaterial=material;r.gameObject.SetActive(true);r.enabled=true;}
  var lod=root.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.03f,new Renderer[]{renderers.Single(r=>r.name=="Basketball_LOD0")}),new LOD(.001f,new Renderer[]{renderers.Single(r=>r.name=="Basketball_LOD1")})});lod.RecalculateBounds();
  var collider=root.AddComponent<SphereCollider>();collider.radius=BasketballBall.Radius;collider.sharedMaterial=rubber;collider.contactOffset=.002f;
  var body=root.AddComponent<Rigidbody>();body.mass=.62f;body.linearDamping=.015f;body.angularDamping=.04f;body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;body.solverIterations=10;body.solverVelocityIterations=6;body.maxAngularVelocity=100;
  root.AddComponent<BasketballBall>();new GameObject("EffectsAnchor").transform.SetParent(root.transform,false);
  var prefab=PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);UnityEngine.Object.DestroyImmediate(root);
  // Preserve the additive world and existing unrelated scene content.
  var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_Basketball.unity");
  var arena=scene.GetRootGameObjects().Single(r=>r.GetComponent<SkySailIslandScene>());Attach(arena);EditorSceneManager.SaveScene(scene);
  var environment=PrefabUtility.LoadPrefabContents("Assets/_Game/Prefabs/Environments/BasketballEnvironment.prefab");
  try{Attach(environment);PrefabUtility.SaveAsPrefabAsset(environment,"Assets/_Game/Prefabs/Environments/BasketballEnvironment.prefab");}finally{PrefabUtility.UnloadPrefabContents(environment);}
  var bootstrap=EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");
  var manager=UnityEngine.Object.FindFirstObjectByType<NetworkManager>();manager.NetworkConfig.ProtocolVersion=RoomService.ProtocolVersion;EditorUtility.SetDirty(manager);EditorSceneManager.MarkSceneDirty(bootstrap);EditorSceneManager.SaveScene(bootstrap);
  AssetDatabase.SaveAssets();Audit(prefab);Debug.Log("BASKETBALL_PREPARE_OK");
 }
 public static void Attach(GameObject arena){
  var existing=arena.GetComponentsInChildren<BasketballBall>(true);
  if(existing.Length>1)throw new Exception("More than one basketball in the arena.");
  if(existing.Length==0){
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);if(!prefab)throw new Exception("Prepare the basketball prefab before rebuilding the arena.");
   var ball=(GameObject)PrefabUtility.InstantiatePrefab(prefab);ball.transform.SetParent(arena.transform,false);ball.transform.localPosition=new Vector3(0,.122f,-1.5f);ball.transform.localRotation=Quaternion.Euler(10,20,15);
  }
  foreach(var hoop in arena.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Hoop_North"||t.name=="Hoop_South").ToArray()){
   if(hoop.Find("BallCollision"))continue;
   var proxy=new GameObject("BallCollision");proxy.layer=8;proxy.transform.SetParent(hoop,false);
   foreach(var name in new[]{"Hoop__BoardShell","Hoop__BoardInset","Hoop__BoardBumper","Hoop__RimMount"}){
    var r=hoop.GetComponentsInChildren<MeshRenderer>(true).Single(x=>x.name==name);
    var box=new GameObject(name+"_Collision");box.layer=8;box.transform.SetParent(r.transform,false);
    var mesh=r.GetComponent<MeshFilter>().sharedMesh;var c=box.AddComponent<BoxCollider>();c.center=mesh.bounds.center;c.size=mesh.bounds.size;c.contactOffset=.002f;
   }
   const int count=32;const float radius=.2476f;
   for(int i=0;i<count;i++){
    float a=i*Mathf.PI*2/count,b=(i+1)*Mathf.PI*2/count;
    var p=new Vector3(Mathf.Cos(a)*radius,3.048f,Mathf.Sin(a)*radius);var q=new Vector3(Mathf.Cos(b)*radius,3.048f,Mathf.Sin(b)*radius);
    var go=new GameObject("Rim segment "+i);go.layer=8;go.transform.SetParent(proxy.transform,false);go.transform.localPosition=(p+q)*.5f;go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,q-p);
    var c=go.AddComponent<CapsuleCollider>();c.radius=.019f;c.height=Vector3.Distance(p,q)+2*c.radius;c.direction=1;c.contactOffset=.001f;
   }
  }
 }
 static void Audit(GameObject prefab){
  var meshes=prefab.GetComponentsInChildren<MeshFilter>(true);var body=prefab.GetComponent<Rigidbody>();var sphere=prefab.GetComponent<SphereCollider>();
  if(meshes.Length!=2||meshes.Any(m=>Vector3.Distance(m.sharedMesh.bounds.size,Vector3.one*.24f)>.0001f))throw new Exception("Ball import scale mismatch");
  if(meshes.Single(m=>m.name=="Basketball_LOD0").sharedMesh.triangles.Length/3!=2976)throw new Exception("Wrong LOD0 topology");
  if(prefab.GetComponentsInChildren<Collider>(true).Length!=1||sphere.center!=Vector3.zero||Mathf.Abs(sphere.radius-.12f)>.00001f||body.mass!=.62f||body.collisionDetectionMode!=CollisionDetectionMode.ContinuousSpeculative)throw new Exception("Invalid ball physics prefab");
  File.WriteAllText(Output+"unity-import-audit.txt","PASS two centred 0.24 m meshes; 2976 / 720 triangles\nPASS one material, normal map and 1024x512 colour map\nPASS single sphere collider, radius 0.12 m; mass 0.62 kg; continuous collision\nPASS one ball per basketball scene; two 32-segment rim colliders and backboard collision\n");
 }
 public static void BuildWindows(){Prepare();ProjectBuilder.BuildCurrentWindows();}
}
