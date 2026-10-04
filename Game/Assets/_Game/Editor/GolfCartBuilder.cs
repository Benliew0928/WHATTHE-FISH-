using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WhatTheFish;

public static class GolfCartBuilder {
 const string Art="Assets/_Game/Art/Golf/Cart/",Prefab="Assets/_Game/Resources/GolfCart.prefab";
 public static void BuildSeatVerification()=>ProjectBuilder.BuildWindowsPlayer("../Builds/GolfCartSeatQA/Player/WhatTheFish.exe");
 public static void BuildFeetVerification()=>ProjectBuilder.BuildWindowsPlayer("../Builds/GolfCartFeetQA/Player/WhatTheFish.exe");
 [MenuItem("WHATTHE FISH?/Golf/Prepare golf cart")]
 public static void Prepare(){
  AssetDatabase.Refresh();
  var layerSettings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
  var layer=layerSettings.FindProperty("layers").GetArrayElementAtIndex(GolfCartMotor.Layer);
  if(!string.IsNullOrEmpty(layer.stringValue)&&layer.stringValue!="GolfCart")throw new Exception("Golf cart layer is already in use.");
  layer.stringValue="GolfCart";layerSettings.ApplyModifiedPropertiesWithoutUndo();
  foreach(var file in Directory.GetFiles(Art,"*.png")){
   var path=file.Replace('\\','/');var importer=(TextureImporter)AssetImporter.GetAtPath(path);bool normal=path.Contains("Normal"),mask=path.Contains("Mask");
   importer.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;importer.sRGBTexture=!normal&&!mask;
   importer.mipmapEnabled=true;importer.isReadable=false;importer.wrapMode=TextureWrapMode.Clamp;importer.anisoLevel=4;
   importer.alphaSource=mask?TextureImporterAlphaSource.FromInput:TextureImporterAlphaSource.None;importer.maxTextureSize=mask?512:1024;
   importer.textureCompression=TextureImporterCompression.CompressedHQ;
   importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=importer.maxTextureSize,format=normal?TextureImporterFormat.ASTC_4x4:mask?TextureImporterFormat.ASTC_8x8:TextureImporterFormat.ASTC_6x6,compressionQuality=100});importer.SaveAndReimport();
  }
  var modelImporter=(ModelImporter)AssetImporter.GetAtPath(Art+"GolfCart.fbx");if(!modelImporter)throw new Exception("Run prepare_golf_cart.py first.");
  modelImporter.globalScale=1;modelImporter.useFileScale=true;modelImporter.importAnimation=false;modelImporter.importCameras=false;modelImporter.importLights=false;modelImporter.materialImportMode=ModelImporterMaterialImportMode.None;
  modelImporter.importNormals=ModelImporterNormals.Import;modelImporter.importTangents=ModelImporterTangents.CalculateMikk;modelImporter.meshCompression=ModelImporterMeshCompression.Low;modelImporter.isReadable=false;modelImporter.SaveAndReimport();
  var font=(TrueTypeFontImporter)AssetImporter.GetAtPath("Assets/_Game/Resources/GolfCartLabels.ttf");
  if(!font)throw new Exception("Run Prepare-CartLabels.py first.");font.includeFontData=true;font.fontNames=new[]{"Golf Cart Controls"};font.fontSize=27;font.SaveAndReimport();
  var material=AssetDatabase.LoadAssetAtPath<Material>(Art+"GolfCart.mat");
  if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,Art+"GolfCart.mat");}
  var color=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"GolfCart_BaseColor.png");material.SetTexture("_BaseMap",color);material.SetTexture("_MainTex",color);material.SetColor("_BaseColor",Color.white);
  material.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"GolfCart_Normal.png"));material.EnableKeyword("_NORMALMAP");material.SetFloat("_BumpScale",.7f);
  material.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"GolfCart_Mask.png"));material.EnableKeyword("_METALLICSPECGLOSSMAP");material.SetFloat("_Smoothness",1);material.enableInstancing=true;EditorUtility.SetDirty(material);
  var root=new GameObject("GolfCart");
  try {
   var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Art+"GolfCart.fbx"));
   // Wheel meshes must be reparented to axle pivots; connected model instances forbid this.
   PrefabUtility.UnpackPrefabInstance(model,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
   model.name="Visual";model.transform.SetParent(root.transform,false);
   foreach(var imported in model.GetComponentsInChildren<LODGroup>(true))UnityEngine.Object.DestroyImmediate(imported);
   var markers=model.GetComponentsInChildren<Transform>(true);var front=markers.Single(t=>t.name=="GolfCart_Front");var seat=markers.Single(t=>t.name=="GolfCart_Seat");
   var forward=Vector3.ProjectOnPlane(front.position-model.transform.position,Vector3.up);model.transform.rotation=Quaternion.Euler(0,-Mathf.Atan2(forward.x,forward.z)*Mathf.Rad2Deg,0);
   var renderers=model.GetComponentsInChildren<MeshRenderer>(true);foreach(var renderer in renderers)renderer.sharedMaterial=material;
   var near=renderers.Where(r=>r.name.EndsWith("LOD0")).ToArray();var bounds=near[0].bounds;foreach(var renderer in near.Skip(1))bounds.Encapsulate(renderer.bounds);
   model.transform.position-=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
   var lod=root.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.26f,renderers.Where(r=>r.name.EndsWith("LOD0")).Cast<Renderer>().ToArray()),new LOD(.10f,renderers.Where(r=>r.name.EndsWith("LOD1")).Cast<Renderer>().ToArray()),new LOD(.012f,renderers.Where(r=>r.name.EndsWith("LOD2")).Cast<Renderer>().ToArray())});lod.RecalculateBounds();
   var cart=root.AddComponent<GolfCart>();cart.seat=seat;cart.hull=root.AddComponent<BoxCollider>();cart.hull.center=new Vector3(0,.66f,0);cart.hull.size=GolfCartMotor.HalfBody*2;
   cart.wheelPivots=new Transform[4];cart.wheelRadii=new float[]{.339f,.339f,.328f,.328f};
   var wheelNames=new[]{"FL","FR","RL","RR"};
   for(int wheel=0;wheel<4;wheel++){
    var parts=markers.Where(t=>t.name.StartsWith("GolfCart_Wheel_"+wheelNames[wheel]+"_LOD")).ToArray();
    if(parts.Length!=3)throw new Exception("Cart requires all three independently rotating wheel LODs: "+wheelNames[wheel]);
    var pivot=new GameObject("Wheel "+wheelNames[wheel]).transform;pivot.SetParent(root.transform,false);pivot.position=parts[0].position;
    foreach(var part in parts){part.SetParent(pivot,true);if(part.parent!=pivot)throw new Exception("Wheel reparent failed: "+part.name);}
    cart.wheelPivots[wheel]=pivot;
   }
   var body=root.AddComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;body.mass=350;
   var roof=new GameObject("Roof collision");roof.transform.SetParent(root.transform,false);var roofCollider=roof.AddComponent<BoxCollider>();roofCollider.center=new Vector3(0,2.43f,-.04f);roofCollider.size=new Vector3(1.65f,.20f,2.18f);
   foreach(var transform in root.GetComponentsInChildren<Transform>(true))transform.gameObject.layer=GolfCartMotor.Layer;
   PrefabUtility.SaveAsPrefabAsset(root,Prefab);
   Directory.CreateDirectory("../Builds/GolfCartQA/Art");File.WriteAllLines("../Builds/GolfCartQA/Art/unity-import.txt",new[]{"PASS front marker="+front.position,"PASS seat="+seat.position,"PASS dimensions="+bounds.size}.Concat(root.GetComponentsInChildren<MeshFilter>(true).Select(f=>"PASS "+f.name+" triangles="+f.sharedMesh.triangles.Length/3)));
   if(Mathf.Abs(bounds.size.z-2.85f)>.015f||seat.position.y<.9f||seat.position.y>1.3f)throw new Exception("Incorrect cart import scale/orientation: "+bounds.size+" seat="+seat.position);
  } finally {UnityEngine.Object.DestroyImmediate(root);}
  AssetDatabase.SaveAssets();Debug.Log("GOLF_CART_PREPARE_OK");
 }
}
