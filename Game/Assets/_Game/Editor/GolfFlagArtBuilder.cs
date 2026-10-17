using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using WhatTheFish;
using Object=UnityEngine.Object;

// Native, editable geometry matching the approved teal/cream swallowtail design.
public static class GolfFlagArtBuilder {
 const string Folder=GolfCourseBuilder.Art;
 const string Prefab="Assets/_Game/Prefabs/Golf/HoleFlag.prefab";
 static readonly Dictionary<int,(DateTime sourceTime,Material material)> referenceCache=new();
 static readonly Color Cream=new Color(.96f,.88f,.71f),Teal=new Color(.015f,.49f,.56f);
 sealed class Shape {
  public readonly List<Vector3> p=new();public readonly List<int> t=new();public readonly List<Color> c=new();
  public void Triangle(Vector3 a,Vector3 b,Vector3 d,Color color){int n=p.Count;p.AddRange(new[]{a,b,d});c.AddRange(new[]{color,color,color});t.AddRange(new[]{n,n+1,n+2});}
  public Mesh Mesh(string name,bool weld=true){
   var positions=new List<Vector3>();var colors=new List<Color32>();var indices=new List<int>();
   var vertices=new Dictionary<(Vector3Int,Color32),int>();
   foreach(int i in t){var point=p[i];Color32 color=c[i];var key=(new Vector3Int(Mathf.RoundToInt(point.x*1000000),Mathf.RoundToInt(point.y*1000000),Mathf.RoundToInt(point.z*1000000)),color);
    if(!weld||!vertices.TryGetValue(key,out int mapped)){mapped=positions.Count;positions.Add(point);colors.Add(color);if(weld)vertices.Add(key,mapped);}indices.Add(mapped);
   }
   var m=new Mesh{name=name,indexFormat=positions.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};m.SetVertices(positions);m.SetTriangles(indices,0);m.SetColors(colors);m.RecalculateNormals();m.RecalculateBounds();var bounds=m.bounds;bounds.Expand(new Vector3(0,0,.48f));m.bounds=bounds;return m;
  }
 }
 static T Save<T>(T value,string path) where T:Object {
  Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(Application.dataPath,path.Substring(7))));AssetDatabase.Refresh();
  var existing=AssetDatabase.LoadAssetAtPath<T>(path);if(existing){if(existing is Mesh oldMesh)oldMesh.Clear(false);EditorUtility.CopySerialized(value,existing);if(existing is Mesh mesh)mesh.UploadMeshData(false);Object.DestroyImmediate(value);EditorUtility.SetDirty(existing);return existing;}
  AssetDatabase.CreateAsset(value,path);return value;
 }
 static Material Mat(string name,string shader,Color color,float metallic=0){
  string path=Folder+"Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(!m){m=new Material(Shader.Find(shader));m.name=name;AssetDatabase.CreateAsset(m,path);}
  m.shader=Shader.Find(shader);if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",color);if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metallic);if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.45f);
  m.enableInstancing=true;EditorUtility.SetDirty(m);return m;
 }
 static GameObject Node(string name,Transform root,Mesh mesh,params Material[] materials){var g=new GameObject(name);g.layer=10;g.transform.SetParent(root,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterials=materials;return g;}
 static void Cylinder(Shape s,float low,float high,float radius,Color color,int sides=32){
  for(int i=0;i<sides;i++){float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;var x=new Vector3(Mathf.Cos(a)*radius,low,Mathf.Sin(a)*radius);var y=new Vector3(Mathf.Cos(b)*radius,low,Mathf.Sin(b)*radius);var z=y+Vector3.up*(high-low);var w=x+Vector3.up*(high-low);s.Triangle(x,w,z,color);s.Triangle(x,z,y,color);s.Triangle(new Vector3(0,high,0),z,w,color);}
 }
 static void Sphere(Shape s,float height,float radius,Color color){
  Vector3 At(int row,int col){float a=row*Mathf.PI/20,b=col*Mathf.PI*2/32;return new Vector3(Mathf.Sin(a)*Mathf.Cos(b),Mathf.Cos(a),Mathf.Sin(a)*Mathf.Sin(b))*radius+Vector3.up*height;}
  for(int row=0;row<20;row++)for(int col=0;col<32;col++){s.Triangle(At(row,col),At(row+1,col+1),At(row+1,col),color);s.Triangle(At(row,col),At(row,col+1),At(row+1,col+1),color);}
 }
 public static void Prepare(){
  Directory.CreateDirectory(Path.Combine(Application.dataPath,(Folder+"Materials/").Substring(7)));AssetDatabase.Refresh();
  var silver=Mat("Flag satin silver","Universal Render Pipeline/Lit",new Color(.90f,.90f,.86f),.20f);var bands=Mat("Flag teal bindings","Universal Render Pipeline/Lit",Teal);
  var pole=new Shape();Cylinder(pole,-.15f,2.99f,.039f,Color.white);int sphereStart=pole.p.Count;Sphere(pole,3.03f,.075f,Color.white);
  var wraps=new Shape();Cylinder(wraps,2.20f,2.36f,.057f,Teal);Cylinder(wraps,2.77f,2.93f,.057f,Teal);
  var a=pole.Mesh("Silver flagpole and ball finial",false);var normals=a.normals;var polePoints=a.vertices;for(int i=0;i<sphereStart;i++)if(Mathf.Abs(normals[i].y)<.5f)normals[i]=new Vector3(polePoints[i].x,0,polePoints[i].z).normalized;for(int i=sphereStart;i<normals.Length;i++)normals[i]=(polePoints[i]-Vector3.up*3.03f).normalized;a.normals=normals;
  var b=wraps.Mesh("Teal fabric bindings",false);var bandNormals=b.normals;var bandPoints=b.vertices;for(int i=0;i<bandNormals.Length;i++)if(Mathf.Abs(bandNormals[i].y)<.5f)bandNormals[i]=new Vector3(bandPoints[i].x,0,bandPoints[i].z).normalized;b.normals=bandNormals;var combined=new Mesh{name="Silver ball-top pole and teal bindings"};combined.vertices=a.vertices.Concat(b.vertices).ToArray();combined.normals=a.normals.Concat(b.normals).ToArray();combined.subMeshCount=2;combined.SetTriangles(a.triangles,0);combined.SetTriangles(b.triangles.Select(i=>i+a.vertexCount).ToArray(),1);combined.RecalculateBounds();Object.DestroyImmediate(a);Object.DestroyImmediate(b);
  var root=new GameObject("Numbered hole flag");Node("Flagpole",root.transform,Save(combined,Folder+"Meshes/Shared/Flagpole.asset"),silver,bands);
  Node("Aqua pennant",root.transform,Save(ReferencePanel(),Folder+"Meshes/Shared/Pennant.asset"),ReferenceFabric(1));
  PrefabUtility.SaveAsPrefabAsset(root,Prefab);Object.DestroyImmediate(root);AssetDatabase.SaveAssets();
 }
 // Derive delivery pixels from the approved masters, preserving their actual
 // silhouette, colour, numeral and painted fabric highlights. No redrawing.
 static Material ReferenceFabric(int number){
  string sourcePath=Path.GetFullPath(Path.Combine(Application.dataPath,"../../ArtSource/Golf/Flags/References/hole-"+number+".png"));var sourceTime=File.GetLastWriteTimeUtc(sourcePath);
  if(referenceCache.TryGetValue(number,out var cached)&&cached.sourceTime==sourceTime&&cached.material&&AssetDatabase.Contains(cached.material))return cached.material;
  string path=Folder+"Textures/HoleFlag"+number+".png";
  Directory.CreateDirectory(Path.GetDirectoryName(path));
  var source=new Texture2D(2,2,TextureFormat.RGBA32,false);
  source.LoadImage(File.ReadAllBytes(sourcePath));
  const int width=512,height=384;var texture=new Texture2D(width*2,height,TextureFormat.RGBA32,false);
  var pixels=new Color32[width*2*height];
  for(int y=0;y<height;y++)for(int x=0;x<width;x++){
   // Image coordinates are measured in the unmodified 1086 x 1448 references.
   float sx=448+(x+.5f)/width*468,sy=120+(1-(y+.5f)/height)*340;
   Color Sample(float px)=>source.GetPixelBilinear(px/source.width,1-sy/source.height);
   pixels[y*width*2+x]=Sample(sx);
   // Reverse only the numeral region; the back keeps the same physical edge.
   var back=Sample(sx);
   if(sx>=540&&sx<=760&&sy>=205&&sy<=400){
    var mirrored=Sample(1300-sx);var cloth=Color.Lerp(Sample(540),Sample(730),Mathf.Clamp01((sx-540)/190));
    float Ink(Color c)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(.05f,.15f,c.r));
    float edge=Mathf.Min(sx-540,760-sx,sy-205,400-sy);float repair=Mathf.SmoothStep(0,1,Mathf.Clamp01(edge/25));
    // Include the painted numeral's shadow and antialias fringe in removal.
    for(int dx=-8;dx<=8;dx+=8)for(int dy=-8;dy<=8;dy+=8)if(source.GetPixelBilinear((sx+dx)/source.width,1-(sy+dy)/source.height).r>.4f)repair=1;
    back=Color.Lerp(back,cloth,repair);back=Color.Lerp(back,mirrored,Ink(mirrored));
   }
   pixels[y*width*2+x+width]=back;
  }
  texture.SetPixels32(pixels);texture.Apply();var delivery=texture.EncodeToPNG();if(!File.Exists(path)||!File.ReadAllBytes(path).SequenceEqual(delivery))File.WriteAllBytes(path,delivery);Object.DestroyImmediate(source);Object.DestroyImmediate(texture);
  AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
  importer.textureType=TextureImporterType.Default;importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=true;
  importer.mipmapEnabled=true;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Trilinear;importer.maxTextureSize=1024;
  var android=importer.GetPlatformTextureSettings("Android");android.overridden=true;android.maxTextureSize=1024;android.format=TextureImporterFormat.ASTC_6x6;importer.SetPlatformTextureSettings(android);importer.SaveAndReimport();
  var material=Mat("Reference hole flag "+number,"WhatTheFish/GolfFlagWind",Color.white);
  material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(path));EditorUtility.SetDirty(material);referenceCache[number]=(sourceTime,material);return material;
 }
 static Mesh ReferencePanel(){
  const int columns=40,rows=28;var p=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();
  for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++){float u=x/(float)columns,v=y/(float)rows;p.Add(new Vector3(.055f+1.065f*u,2.18f+.775f*v,0));uv.Add(new Vector2(u,v));}
  for(int y=0;y<rows;y++)for(int x=0;x<columns;x++){int a=y*(columns+1)+x,b=a+columns+1;t.AddRange(new[]{a,b,a+1,a+1,b,b+1});}
  var m=new Mesh{name="Reference silhouette wind cloth"};m.SetVertices(p);m.SetUVs(0,uv);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();var bounds=m.bounds;bounds.Expand(new Vector3(0,0,.48f));m.bounds=bounds;return m;
 }
 public static void AddNumbers(Transform flag,int number){
  foreach(var child in flag.Cast<Transform>().Where(t=>t.name.StartsWith("Flag number ")||t.name=="Aqua pennant").ToArray())Object.DestroyImmediate(child.gameObject);
  Node("Aqua pennant",flag,AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"Meshes/Shared/Pennant.asset"),ReferenceFabric(number));
 }
 static void Update(GameObject island){foreach(var h in island.GetComponentInChildren<GolfCourse>(true).holes)AddNumbers(h.flag,h.number);}
 public static void UpdateSavedMaps(){
  Prepare();var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_Golf.unity");Update(scene.GetRootGameObjects().Single(r=>r.GetComponent<SkySailIslandScene>()));EditorSceneManager.SaveScene(scene);
  const string path="Assets/_Game/Prefabs/Environments/GolfEnvironment.prefab";var root=PrefabUtility.LoadPrefabContents(path);try{Update(root);PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}AssetDatabase.SaveAssets();
 }
}
