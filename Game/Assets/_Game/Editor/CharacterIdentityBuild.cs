using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using WhatTheFish;

public static class CharacterIdentityBuild {
 const string Folder="Assets/_Game/Art/CharacterIdentity/",Model="Assets/_Game/Art/RainbowSprinter.fbx";
 [MenuItem("WHATTHE FISH?/Characters/Prepare ten identities")]
 public static void Prepare(){
  Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
  string hash=Hash(File.ReadAllBytes("Assets/_Game/Editor/CharacterIdentityBuild.cs"))+Hash(File.ReadAllBytes("Assets/_Game/Editor/CharacterHeadMesh.cs"))+AssetDatabase.GetAssetDependencyHash(Model);
  var library=AssetDatabase.LoadAssetAtPath<CharacterIdentityLibrary>(Folder+"Library.asset");
  if(!library){library=ScriptableObject.CreateInstance<CharacterIdentityLibrary>();AssetDatabase.CreateAsset(library,Folder+"Library.asset");}
  if(library.sourceHash!=hash||!library.bodyMaterial||library.nearHair==null||library.nearHair.Length!=10){
   var model=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Model));
   try{
    var head=model.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="mixamorig:Head");
    foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>())if(skin.sharedMesh.colors32.Length!=skin.sharedMesh.vertexCount||skin.sharedMesh.GetBlendShapeIndex("GolfRightGrip")<0)throw new Exception("Character import must preserve identity regions and both grips");
    library.headPosition=head.position;library.headRotation=head.rotation;
    library.nearHair=new Mesh[10];library.farHair=new Mesh[10];
    for(int lod=0;lod<2;lod++){
     var scalp=new CharacterHeadMesh(head,lod!=0);scalp.Scalp();var mesh=scalp.Mesh("Shared scalp LOD"+lod);MeshUtility.SetMeshCompression(mesh,ModelImporterMeshCompression.Medium);
     string path=Folder+"Scalp_LOD"+lod+".asset";Save(mesh,path);if(lod==0)library.nearScalp=AssetDatabase.LoadAssetAtPath<Mesh>(path);else library.farScalp=AssetDatabase.LoadAssetAtPath<Mesh>(path);
    }
    for(int preset=1;preset<10;preset++)for(int lod=0;lod<2;lod++){
     var builder=new CharacterHeadMesh(head,lod!=0);builder.Make(preset);
     var mesh=builder.Mesh("Identity_"+(preset+1).ToString("00")+"_LOD"+lod);MeshUtility.SetMeshCompression(mesh,ModelImporterMeshCompression.Medium);
     string path=Folder+mesh.name+".asset";Save(mesh,path);if(lod==0)library.nearHair[preset]=AssetDatabase.LoadAssetAtPath<Mesh>(path);else library.farHair[preset]=AssetDatabase.LoadAssetAtPath<Mesh>(path);
    }
    var digits=Digits();Save(digits,Folder+"Numbers.asset");
    var material=new Material(Shader.Find("WhatTheFish/CharacterIdentity")){name="Shared identity body"};
    material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/RainbowSprinter/albedo.png"));
    material.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/RainbowSprinter/normal.png"));
    material.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/RainbowSprinter/metallic_smoothness.png"));
    material.SetTexture("_Numbers",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"Numbers.asset"));
    Save(material,Folder+"Body.mat");library.bodyMaterial=AssetDatabase.LoadAssetAtPath<Material>(Folder+"Body.mat");
    var hair=new Material(Shader.Find("WhatTheFish/CharacterHead")){name="Shared sculpted hair"};hair.SetFloat("_Smoothness",.3f);Save(hair,Folder+"Hair.mat");library.hairMaterial=AssetDatabase.LoadAssetAtPath<Material>(Folder+"Hair.mat");
    library.sourceHash=hash;EditorUtility.SetDirty(library);
   }finally{UnityEngine.Object.DestroyImmediate(model);}
  }
  foreach(string path in new[]{"Assets/_Game/Prefabs/Characters/OfflineAthlete.prefab","Assets/_Game/Prefabs/Characters/NetworkAthlete.prefab"}){
   var prefab=PrefabUtility.LoadPrefabContents(path);try{var actor=prefab.GetComponent<Athlete>();if(actor.identityLibrary!=library){actor.identityLibrary=library;PrefabUtility.SaveAsPrefabAsset(prefab,path);}}finally{PrefabUtility.UnloadPrefabContents(prefab);}
  }
  AssetDatabase.SaveAssets();Debug.Log("CHARACTER_IDENTITIES_PREPARED ten presets, shared rig and textures");
 }
 public static void BuildReview(){Prepare();ProjectBuilder.BuildWindowsPlayer("../Builds/CharacterIdentityQA/Player/WhatTheFish.exe");}
 public static void PreviewAndBuildReview(){Preview();BuildReview();}
 public static void Preview(){
  Prepare();var setup=UnityEditor.SceneManagement.EditorSceneManager.GetSceneManagerSetup();
  string output=ProjectBuilder.ResolveBuildOutputPath("../Builds/CharacterIdentityQA/ArtPreview");Directory.CreateDirectory(output);
  try{
   UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,UnityEditor.SceneManagement.NewSceneMode.Single);
   RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=Color.gray;
   var light=new GameObject("Preview sun").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.5f;light.transform.rotation=Quaternion.Euler(40,-35,0);
   var camera=new GameObject("Preview camera").AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.65f,.75f,.8f);camera.fieldOfView=38;camera.nearClipPlane=.05f;
   var actors=new List<Athlete>();
   for(int i=0;i<10;i++){var actor=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Characters/OfflineAthlete.prefab")).GetComponent<Athlete>();actor.transform.position=Vector3.right*i*1.1f;var identity=CharacterIdentity.Ensure(actor);identity.SetNumber((byte)(i+1));foreach(var skin in actor.visual.GetComponentsInChildren<SkinnedMeshRenderer>())skin.sharedMaterial.SetColor("_JerseyColor",CharacterIdentity.Blue);actor.visual.GetComponent<LODGroup>()?.ForceLOD(0);actors.Add(actor);}
   void Capture(string name){var rt=RenderTexture.GetTemporary(1280,720,24);camera.targetTexture=rt;camera.Render();var previous=RenderTexture.active;RenderTexture.active=rt;var pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),pixels.EncodeToPNG());UnityEngine.Object.DestroyImmediate(pixels);camera.targetTexture=null;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);}
   camera.transform.position=new Vector3(4.95f,3.6f,13);camera.transform.LookAt(new Vector3(4.95f,1,0));camera.fieldOfView=48;Capture("all-front");
   camera.transform.position=new Vector3(4.95f,3.6f,-13);camera.transform.LookAt(new Vector3(4.95f,1,0));Capture("all-back");
   for(int i=0;i<10;i++){camera.fieldOfView=38;camera.transform.position=actors[i].transform.position+new Vector3(.4f,1.6f,2.9f);camera.transform.LookAt(actors[i].transform.position+Vector3.up*1.1f);Capture("identity-"+(i+1).ToString("00"));}
   foreach(int i in new[]{1,5,8}){camera.transform.position=actors[i].transform.position+new Vector3(.5f,1.6f,-2.9f);camera.transform.LookAt(actors[i].transform.position+Vector3.up*1.1f);Capture("back-"+(i+1).ToString("00"));}
   Debug.Log("CHARACTER_IDENTITY_ART_PREVIEW "+output);
  }finally{if(setup.Any(s=>s.isLoaded&&s.isActive))UnityEditor.SceneManagement.EditorSceneManager.RestoreSceneManagerSetup(setup);else UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");}
 }
 static string Hash(byte[] bytes)=>BitConverter.ToString(SHA256.Create().ComputeHash(bytes)).Replace("-","");
 static void Save(UnityEngine.Object value,string path){var old=AssetDatabase.LoadMainAssetAtPath(path);if(old){if(old is Mesh previous)previous.Clear(false);EditorUtility.CopySerialized(value,old);if(old is Mesh updated)updated.UploadMeshData(false);EditorUtility.SetDirty(old);UnityEngine.Object.DestroyImmediate(value);}else AssetDatabase.CreateAsset(value,path);}
 static Texture2D Digits(){
  int[] bits={63,6,91,79,102,109,125,7,127,111};var pixels=new byte[320*64];
  RectInt[] segments={new(8,54,16,6),new(23,34,6,20),new(23,10,6,20),new(8,4,16,6),new(2,10,6,20),new(2,34,6,20),new(8,29,16,6)};
  for(int digit=0;digit<10;digit++)for(int segment=0;segment<7;segment++)if((bits[digit]&(1<<segment))!=0){var r=segments[segment];for(int y=r.y;y<r.yMax;y++)for(int x=r.x;x<r.xMax;x++)pixels[y*320+digit*32+x]=255;}
  for(int digit=6;digit<=9;digit+=3)for(int x=9;x<23;x++)pixels[320+digit*32+x]=255;
  var texture=new Texture2D(320,64,TextureFormat.R8,false,true){name="Shared jersey digits",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};texture.SetPixelData(pixels,0);texture.Apply(false,true);return texture;
 }
}
