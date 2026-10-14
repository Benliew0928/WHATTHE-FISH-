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
  string hash=Hash(File.ReadAllBytes("Assets/_Game/Editor/CharacterIdentityBuild.cs"))+AssetDatabase.GetAssetDependencyHash(Model);
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
     var scalp=new HairMesh(head,lod==0?32:16,lod==0?16:8);scalp.Scalp();var mesh=scalp.Mesh("Shared scalp LOD"+lod);MeshUtility.SetMeshCompression(mesh,ModelImporterMeshCompression.Low);
     string path=Folder+"Scalp_LOD"+lod+".asset";Save(mesh,path);if(lod==0)library.nearScalp=AssetDatabase.LoadAssetAtPath<Mesh>(path);else library.farScalp=AssetDatabase.LoadAssetAtPath<Mesh>(path);
    }
    for(int preset=1;preset<10;preset++)for(int lod=0;lod<2;lod++){
     var builder=new HairMesh(head,lod==0?24:12,lod==0?10:5);builder.Make(preset);
     var mesh=builder.Mesh("Identity_"+(preset+1).ToString("00")+"_LOD"+lod);MeshUtility.SetMeshCompression(mesh,ModelImporterMeshCompression.Low);
     string path=Folder+mesh.name+".asset";Save(mesh,path);if(lod==0)library.nearHair[preset]=AssetDatabase.LoadAssetAtPath<Mesh>(path);else library.farHair[preset]=AssetDatabase.LoadAssetAtPath<Mesh>(path);
    }
    var digits=Digits();Save(digits,Folder+"Numbers.asset");
    var material=new Material(Shader.Find("WhatTheFish/CharacterIdentity")){name="Shared identity body"};
    material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/RainbowSprinter/albedo.png"));
    material.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/RainbowSprinter/normal.png"));
    material.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/RainbowSprinter/metallic_smoothness.png"));
    material.SetTexture("_Numbers",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"Numbers.asset"));
    Save(material,Folder+"Body.mat");library.bodyMaterial=AssetDatabase.LoadAssetAtPath<Material>(Folder+"Body.mat");
    var hair=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Shared sculpted hair"};hair.SetFloat("_Smoothness",.3f);hair.SetFloat("_Metallic",0);Save(hair,Folder+"Hair.mat");library.hairMaterial=AssetDatabase.LoadAssetAtPath<Material>(Folder+"Hair.mat");
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
 sealed class HairMesh {
  readonly Transform head;readonly int sectors,rings;readonly List<Vector3> positions=new(),normals=new();readonly List<int> triangles=new();
  public HairMesh(Transform head,int sectors,int rings){this.head=head;this.sectors=sectors;this.rings=rings;}
  void Triangle(int a,int b,int c){if(Vector3.Dot(Vector3.Cross(positions[b]-positions[a],positions[c]-positions[a]),normals[a]+normals[b]+normals[c])<0)(b,c)=(c,b);triangles.Add(a);triangles.Add(b);triangles.Add(c);}
  void Ellipsoid(Vector3 center,Vector3 scale,bool cap=false,bool flat=false,int detail=0){
   int s=detail>0?Mathf.Max(6,sectors/2):sectors,r=detail>0?Mathf.Max(4,rings/2):rings,start=positions.Count;
   for(int row=0;row<=r;row++)for(int col=0;col<=s;col++){
    float phi=col*Mathf.PI*2/s,front=(Mathf.Sin(phi)+1)*.5f;
    float maximum=cap?Mathf.Acos(Mathf.Lerp(-.95f,-.15f,front)):Mathf.PI;
    float theta=row*maximum/r;var unit=new Vector3(Mathf.Sin(theta)*Mathf.Cos(phi),Mathf.Cos(theta),Mathf.Sin(theta)*Mathf.Sin(phi));
    var point=Vector3.Scale(unit,scale);if(flat&&unit.y>0)point.y=scale.y*Mathf.Min(unit.y*1.12f,.86f);
    positions.Add(head.InverseTransformPoint(center+point));var normal=new Vector3(unit.x/scale.x,unit.y/scale.y,unit.z/scale.z);if(flat&&unit.y>.77f)normal=Vector3.up;normals.Add(head.InverseTransformDirection(normal.normalized));
   }
   for(int row=0;row<r;row++)for(int col=0;col<s;col++){int a=start+row*(s+1)+col,b=a+s+1;Triangle(a,b,a+1);Triangle(a+1,b,b+1);}
  }
  public void Make(int preset){
   var center=new Vector3(0,1.325f,.062f);var radius=new Vector3(.285f,.285f,.285f);
   if(preset==3)radius.y=.32f;if(preset==4)radius.y=.23f;if(preset==8)radius.y=.265f;
   Ellipsoid(center,radius,true,preset==3);
   switch(preset){
    case 1: // Cropped scalloped fringe.
     for(int i=-2;i<=2;i++)Ellipsoid(new Vector3(i*.088f,1.405f,.285f),new Vector3(.065f,.065f,.045f),detail:1);break;
    case 2:
     for(int ring=0;ring<3;ring++)for(int j=0;j<8;j++){float a=(j+ring*.5f)*Mathf.PI/4,t=(ring+1)*.43f;Ellipsoid(center+new Vector3(Mathf.Cos(a)*.26f*Mathf.Sin(t),.29f*Mathf.Cos(t),Mathf.Sin(a)*.25f*Mathf.Sin(t)),new Vector3(.085f,.085f,.085f),detail:1);}break;
    case 4:
     for(int i=0;i<4;i++)Ellipsoid(new Vector3(0,1.58f+Mathf.Sin(i*.8f)*.04f,.23f-i*.12f),new Vector3(.07f,.16f,.1f),detail:1);break;
    case 5:
     Ellipsoid(new Vector3(0,1.26f,-.265f),new Vector3(.10f,.10f,.14f));Ellipsoid(new Vector3(0,1.13f,-.365f),new Vector3(.115f,.21f,.115f));break;
    case 6:Ellipsoid(new Vector3(0,1.69f,.02f),new Vector3(.16f,.16f,.15f));break;
    case 7:
     foreach(int side in new[]{-1,1})Ellipsoid(new Vector3(side*.29f,1.49f,.02f),new Vector3(.17f,.17f,.165f));break;
    case 8:Ellipsoid(new Vector3(0,1.39f,.32f),new Vector3(.29f,.034f,.20f));break;
    case 9:
     Ellipsoid(new Vector3(0,1.385f,.06f),new Vector3(.36f,.038f,.335f));Ellipsoid(new Vector3(0,1.45f,.06f),new Vector3(.27f,.19f,.255f),true,true);break;
   }
  }
  public void Scalp()=>Ellipsoid(new Vector3(0,1.27f,.02f),new Vector3(.263f,.28f,.18f));
  public Mesh Mesh(string name){var m=new Mesh{name=name};m.SetVertices(positions);m.SetNormals(normals);m.SetTriangles(triangles,0);m.RecalculateBounds();return m;}
 }
}
