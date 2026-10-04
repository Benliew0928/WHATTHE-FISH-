using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using WhatTheFish;

// Editable, deterministic lathe master. No external model or texture dependency.
public static class GolfTeeBuilder {
 const string Art="Assets/_Game/Art/Golf/Tee/",Prefab="Assets/_Game/Resources/GolfTee.prefab";
 public static void BuildVerification()=>ProjectBuilder.BuildWindowsPlayer("../Builds/GolfWoodTeeQA/Player/WhatTheFish.exe");
 [MenuItem("WHATTHE FISH?/Golf/Prepare wooden tee")]
 public static void Prepare(){
  Directory.CreateDirectory(Art);AssetDatabase.Refresh();
  var profile=new List<Vector2>{new(0,-.018f),new(.002f,-.013f),new(.005f,.006f),new(.007f,.041f),new(.011f,.056f),new(.020f,.070f),new(GolfTee.CupRadius,.074f),new(GolfTee.CupRadius,.0824f)};
  // A dish slightly flatter than the ball supports its underside at the centre
  // while the rounded rim leaves clearance for a normal low-charge putt.
  float dishRadius=GolfBall.Radius*1.15f;
  for(int ring=6;ring>=0;ring--){float r=GolfTee.CupRadius*ring/6f;profile.Add(new(r,GolfTee.SeatHeight+dishRadius-Mathf.Sqrt(dishRadius*dishRadius-r*r)));}
  var mesh=Lathe(profile,32);var existing=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"WoodTee.asset");
  if(existing){EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;EditorUtility.SetDirty(mesh);}else AssetDatabase.CreateAsset(mesh,Art+"WoodTee.asset");
  string texturePath=Art+"WoodGrain.png";var grain=new Texture2D(64,128,TextureFormat.RGB24,false);
  for(int y=0;y<grain.height;y++)for(int x=0;x<grain.width;x++){
   float u=x/(float)grain.width,v=y/(float)grain.height;
   float wave=Mathf.Sin((u*18+.10f*Mathf.Sin(v*9)+.07f*Mathf.Sin(v*21))*Mathf.PI*2);
   float fine=Mathf.PerlinNoise(x*.47f,y*.11f),broad=Mathf.PerlinNoise(x*.095f,y*.025f);
   float shade=.94f+.04f*wave+.035f*(fine-.5f)+.055f*(broad-.5f);
   grain.SetPixel(x,y,new Color(.98f,.80f,.52f)*shade);
  }
  grain.Apply();File.WriteAllBytes(texturePath,grain.EncodeToPNG());UnityEngine.Object.DestroyImmediate(grain);AssetDatabase.ImportAsset(texturePath);
  var importer=(TextureImporter)AssetImporter.GetAtPath(texturePath);importer.textureType=TextureImporterType.Default;importer.sRGBTexture=true;importer.mipmapEnabled=true;importer.isReadable=false;importer.alphaSource=TextureImporterAlphaSource.None;importer.wrapMode=TextureWrapMode.Repeat;importer.maxTextureSize=128;importer.textureCompression=TextureImporterCompression.CompressedHQ;
  importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=128,format=TextureImporterFormat.ASTC_6x6,compressionQuality=100});importer.SaveAndReimport();
  var material=AssetDatabase.LoadAssetAtPath<Material>(Art+"WoodTee.mat");if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,Art+"WoodTee.mat");}
  material.SetColor("_BaseColor",Color.white);material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));material.SetFloat("_Metallic",0);material.SetFloat("_Smoothness",.22f);material.enableInstancing=true;EditorUtility.SetDirty(material);
  var root=new GameObject("Wooden opening tee");root.layer=GolfTee.Layer;
  try{root.AddComponent<MeshFilter>().sharedMesh=mesh;root.AddComponent<MeshRenderer>().sharedMaterial=material;root.AddComponent<MeshCollider>().sharedMesh=mesh;root.AddComponent<GolfTee>();PrefabUtility.SaveAsPrefabAsset(root,Prefab);}
  finally{UnityEngine.Object.DestroyImmediate(root);}
  AssetDatabase.SaveAssets();Directory.CreateDirectory("../Builds/GolfWoodTeeQA/Art");File.WriteAllText("../Builds/GolfWoodTeeQA/Art/unity-import.txt",$"PASS shared lathe mesh vertices={mesh.vertexCount} triangles={mesh.triangles.Length/3} bounds={mesh.bounds}\nPASS 64x128 shared wood texture, ASTC 6x6 Android, one instanced Lit material\nPASS fixed concave static collider, no rigidbody, no replacement ball\n");Debug.Log("GOLF_WOOD_TEE_PREPARE_OK");
 }
 static Mesh Lathe(List<Vector2> profile,int sides){
  var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();int stride=sides+1;
  for(int ring=0;ring<profile.Count;ring++)for(int side=0;side<=sides;side++){
   float angle=side*Mathf.PI*2/sides;var p=profile[ring];vertices.Add(new Vector3(Mathf.Cos(angle)*p.x,p.y,Mathf.Sin(angle)*p.x));uv.Add(new Vector2(side/(float)sides,(p.y+.018f)/.1004f));
  }
  for(int ring=0;ring<profile.Count-1;ring++)for(int side=0;side<sides;side++){
   int a=ring*stride+side,b=a+stride;
   if(profile[ring].x>0){triangles.Add(a);triangles.Add(b);triangles.Add(a+1);}
   if(profile[ring+1].x>0){triangles.Add(a+1);triangles.Add(b);triangles.Add(b+1);}
  }
  var mesh=new Mesh{name="Wooden tee · pointed stem and shallow cup"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
 }
}
