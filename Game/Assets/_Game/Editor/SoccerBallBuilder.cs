using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Shared authoring path for the scene, environment prefab and regenerators.
public static class SoccerBallBuilder {
 public const string ModelPath="Assets/_Game/Art/Football/Ball/soccer-glossy-unity.fbx";
 public const string PrefabPath="Assets/_Game/Prefabs/Football/SoccerBall.prefab";
 const string ScenePath="Assets/_Game/Scenes/SkySail_Football.unity";
 const string Name="Soccer ball · centre spot";
 const float Diameter=.44f;
 const float GroundClearance=.0005f;

 [MenuItem("WHATTHE FISH?/Football/Install centre-spot soccer ball")]
 public static void Install(){
  if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play mode before installing the centre ball.");
  var scene=SceneManager.GetSceneByPath(ScenePath);
  bool opened=!scene.isLoaded;
  if(!opened&&scene.isDirty)throw new InvalidOperationException("Save the football scene before installation.");
  var original=SceneManager.GetActiveScene();
  if(opened)scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Additive);
  try{
   var stadium=scene.GetRootGameObjects().Single(r=>r.GetComponent<WhatTheFish.SkySailIslandScene>());
   Attach(stadium);
   EditorSceneManager.SaveScene(scene);
   const string environment="Assets/_Game/Prefabs/Environments/FootballEnvironment.prefab";
   var content=PrefabUtility.LoadPrefabContents(environment);
   try{Attach(content);PrefabUtility.SaveAsPrefabAsset(content,environment);}finally{PrefabUtility.UnloadPrefabContents(content);}
   AssetDatabase.SaveAssets();
   Audit(scene);
   Capture(scene);
   Debug.Log($"SOCCER_BALL_INSTALLED centre=(0,0) diameter={Diameter:F2}m; dynamic physics, two URP materials");
  }finally{if(opened)EditorSceneManager.CloseScene(scene,true);if(original.IsValid()&&original.isLoaded)EditorSceneManager.SetActiveScene(original);}
 }

 public static void Attach(GameObject stadium){
  var prefab=PreparePrefab();
  var old=stadium.transform.Find(Name);if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
  var ball=(GameObject)PrefabUtility.InstantiatePrefab(prefab,stadium.transform);
  ball.name=Name;
  var pitch=stadium.GetComponentsInChildren<MeshFilter>(true).Single(f=>f.name=="Lawn__Pitch");
  // Use the authored pitch mesh, excluding the decorative grass/paint above it.
  var bounds=pitch.GetComponent<Renderer>().bounds;
  float top=stadium.transform.InverseTransformPoint(new Vector3(bounds.center.x,bounds.max.y,bounds.center.z)).y;
  ball.transform.localPosition=new Vector3(0,top+Diameter*.5f+GroundClearance,0);
 }

 static GameObject PreparePrefab(){
  Directory.CreateDirectory("Assets/_Game/Art/Football/Ball/Materials");AssetDatabase.Refresh();
  var importer=AssetImporter.GetAtPath(ModelPath) as ModelImporter;
  if(!importer)throw new InvalidOperationException("Missing supplied football FBX: "+ModelPath);
  importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;
  importer.globalScale=1;importer.useFileScale=true;importer.importNormals=ModelImporterNormals.Import;
  importer.importTangents=ModelImporterTangents.CalculateMikk;
  importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
  importer.meshCompression=ModelImporterMeshCompression.Low;importer.isReadable=false;importer.SaveAndReimport();
  var root=new GameObject("SoccerBall");
  try{
   var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath));model.transform.SetParent(root.transform,false);
   foreach(var renderer in model.GetComponentsInChildren<MeshRenderer>(true))renderer.sharedMaterials=renderer.sharedMaterials.Select(Material).ToArray();
   var renderers=model.GetComponentsInChildren<Renderer>(true);var bounds=renderers[0].bounds;foreach(var renderer in renderers.Skip(1))bounds.Encapsulate(renderer.bounds);
   float diameter=Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);
   if(diameter<.001f||diameter>30)throw new InvalidOperationException("Invalid football model bounds.");
   float scale=Diameter/diameter;model.transform.localScale*=scale;model.transform.localPosition=-bounds.center*scale;
   var collider=root.AddComponent<SphereCollider>();collider.radius=Diameter*.5f;collider.contactOffset=.002f;
   const string physicsPath="Assets/_Game/Art/Football/Ball/Football.asset";
   var physics=AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(physicsPath);
   if(!physics){physics=new PhysicsMaterial("Football");AssetDatabase.CreateAsset(physics,physicsPath);}
   physics.dynamicFriction=.6f;physics.staticFriction=.6f;physics.bounciness=.15f;physics.frictionCombine=PhysicsMaterialCombine.Average;physics.bounceCombine=PhysicsMaterialCombine.Minimum;EditorUtility.SetDirty(physics);collider.sharedMaterial=physics;
   var body=root.AddComponent<Rigidbody>();body.mass=.43f;body.linearDamping=.05f;body.angularDamping=.05f;body.useGravity=true;body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;body.solverIterations=12;body.solverVelocityIterations=4;body.maxAngularVelocity=120;
   root.AddComponent<WhatTheFish.FootballBall>();
   // Default layer: the ball must not be mistaken for walkable stadium floor.
   return PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
  }finally{UnityEngine.Object.DestroyImmediate(root);}
 }

 static Material Material(Material source){
  bool ivory=source&&source.name.Contains("ivory");
  if(!source||!ivory&&!source.name.Contains("charcoal"))throw new InvalidOperationException("Unexpected football pigment.");
  string path="Assets/_Game/Art/Football/Ball/Materials/"+(ivory?"SatinIvory":"SatinCharcoal")+".mat";
  var material=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
  // FBX values are linear pigments; Unity's material colour uses an sRGB value.
  var color=ivory?new Color(.9f,.81f,.65f,1):new Color(.028f,.033f,.042f,1);
  material.SetColor("_BaseColor",color.gamma);material.SetFloat("_Metallic",0);
  material.SetFloat("_Smoothness",ivory?.66f:.70f);material.enableInstancing=true;EditorUtility.SetDirty(material);return material;
 }

 static void Audit(Scene scene){
  var root=scene.GetRootGameObjects().Single(r=>r.GetComponent<WhatTheFish.SkySailIslandScene>());
  var ball=root.transform.Find(Name);var renderers=ball.GetComponentsInChildren<Renderer>(true);
  var bounds=renderers[0].bounds;foreach(var renderer in renderers.Skip(1))bounds.Encapsulate(renderer.bounds);
  var pitch=root.GetComponentsInChildren<MeshFilter>(true).Single(f=>f.name=="Lawn__Pitch").GetComponent<Renderer>().bounds;
  int triangles=ball.GetComponentsInChildren<MeshFilter>(true).Sum(f=>(int)f.sharedMesh.GetIndexCount(0)/3+(f.sharedMesh.subMeshCount>1?(int)f.sharedMesh.GetIndexCount(1)/3:0));
  if(root.GetComponentsInChildren<Transform>(true).Count(t=>t.name==Name)!=1||Mathf.Abs(ball.localPosition.x)>.0001f||Mathf.Abs(ball.localPosition.z)>.0001f)throw new Exception("Football centre placement is invalid.");
  if(Mathf.Abs(bounds.size.x-Diameter)>.001f||Mathf.Abs(bounds.size.y-Diameter)>.001f||Mathf.Abs(bounds.size.z-Diameter)>.001f||Mathf.Abs(bounds.min.y-pitch.max.y-GroundClearance)>.002f)throw new Exception("Football scale or pitch contact is invalid.");
  var collider=ball.GetComponent<SphereCollider>();
  var body=ball.GetComponent<Rigidbody>();
  if(!body||body.isKinematic||!body.useGravity||body.collisionDetectionMode!=CollisionDetectionMode.ContinuousDynamic||!ball.GetComponent<WhatTheFish.FootballBall>()||!collider||!collider.sharedMaterial||Mathf.Abs(collider.radius-Diameter*.5f)>.0001f||renderers.Any(r=>r.sharedMaterials.Any(m=>!m||m.shader.name!="Universal Render Pipeline/Lit")))throw new Exception("Football collider/material audit failed.");
  Directory.CreateDirectory("../Builds/SoccerBall");
  File.WriteAllText("../Builds/SoccerBall/unity-audit.txt",$"PASS one ball at pitch centre\nPASS diameter {bounds.size}\nPASS ground clearance {bounds.min.y-pitch.max.y:F6} m\nPASS dynamic sphere, gravity, CCD and football behavior\nPASS URP pigments and smoothness\nTriangles {triangles}\n");
 }

 static void Capture(Scene scene){
  var roots=scene.GetRootGameObjects();var root=roots.Single(r=>r.GetComponent<WhatTheFish.SkySailIslandScene>());
  bool active=root.activeSelf;bool asyncCompilation=ShaderUtil.allowAsyncCompilation;var cam=new GameObject("Soccer ball review camera").AddComponent<Camera>();SceneManager.MoveGameObjectToScene(cam.gameObject,scene);
  try{
   root.SetActive(true);var centre=root.transform.Find(Name).position;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.65f,.78f,.85f);cam.nearClipPlane=.01f;cam.farClipPlane=500;cam.fieldOfView=42;
   ShaderUtil.allowAsyncCompilation=false;
   foreach(var shot in new[]{("close",new Vector3(.29f,.19f,-.4f)*(Diameter/.22f)),("centre",new Vector3(2,1.7f,-3))}){
    cam.transform.position=centre+shot.Item2;cam.transform.LookAt(centre);
    var rt=RenderTexture.GetTemporary(1280,720,24);var previous=RenderTexture.active;
    var pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);
    try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();File.WriteAllBytes("../Builds/SoccerBall/"+shot.Item1+".png",pixels.EncodeToPNG());}
    finally{cam.targetTexture=null;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(pixels);}
   }
  }finally{ShaderUtil.allowAsyncCompilation=asyncCompilation;root.SetActive(active);UnityEngine.Object.DestroyImmediate(cam.gameObject);}
 }
}
