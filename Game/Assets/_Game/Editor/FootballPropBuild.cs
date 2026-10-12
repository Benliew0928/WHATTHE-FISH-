using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using WhatTheFish;

// Editable, deterministic mesh masters. No external FBX/Blender dependencies,
// runtime primitive creation, textures on models, or per-pickup materials.
public static class FootballPropBuild {
 const string Root="Assets/_Game/Resources/FootballProps/";
 static Material ink,mint,gold,blue,violet,black,ivory,orange,glass;
 static readonly List<(Mesh mesh,Matrix4x4 matrix,Material material)> parts=new();
 public static void Prepare(){
  if(!File.Exists(Root+"Icons.png"))throw new Exception("Run Tools/Build/Generate-FootballProps.cjs first.");
  Directory.CreateDirectory(Root);AssetDatabase.Refresh();
  ink=Material("Ink",new(.025f,.09f,.15f));mint=Material("Mint",new(.10f,.85f,.63f));gold=Material("Gold",new(1,.62f,.08f));blue=Material("Blue",new(.16f,.72f,1));violet=Material("Violet",new(.24f,.19f,.53f));black=Material("Black",new(.004f,.008f,.025f));ivory=Material("Ivory",new(.98f,.97f,.86f));orange=Material("Orange",new(1,.36f,.065f));
  glass=AssetDatabase.LoadAssetAtPath<Material>(Root+"Glass.mat");if(!glass){glass=new Material(Shader.Find("WhatTheFish/Basketball Prop Effect"));AssetDatabase.CreateAsset(glass,Root+"Glass.mat");}glass.SetColor("_Tint",new Color(.17f,.72f,1,.38f));glass.SetFloat("_Mode",6);glass.enableInstancing=true;EditorUtility.SetDirty(glass);
  var countdown=Material("Countdown",new Color(.025f,.12f,.72f,1));countdown.SetFloat("_Smoothness",.12f);countdown.SetFloat("_Metallic",0);countdown.SetFloat("_Surface",0);countdown.SetFloat("_ZWrite",1);EditorUtility.SetDirty(countdown);
  Armband();Save("Turbo");Portal();Save("Portal");Wall(.98f,.68f,.13f,false);Save("Wall");Wall(FootballPropRules.WallWidth,FootballPropRules.WallHeight,FootballPropRules.WallThickness,true);Save("DeployedWall");
  var t=(TextureImporter)AssetImporter.GetAtPath(Root+"Icons.png");t.textureType=TextureImporterType.Default;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.isReadable=false;t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;t.maxTextureSize=512;t.textureCompression=TextureImporterCompression.Compressed;t.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=512,format=TextureImporterFormat.ASTC_6x6,compressionQuality=70});t.SaveAndReimport();AssetDatabase.SaveAssets();Debug.Log("FOOTBALL_PROPS_ASSETS_OK");
 }
 static Material Material(string name,Color color){var m=AssetDatabase.LoadAssetAtPath<Material>(Root+name+".mat");if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,Root+name+".mat");}m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",.38f);m.SetFloat("_Metallic",name=="Gold"||name=="Orange"?.22f:.08f);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;}
 static void Add(Mesh mesh,Vector3 p,Quaternion q,Vector3 size,Material m)=>parts.Add((mesh,Matrix4x4.TRS(p,q,size),m));
 static void Cube(Vector3 p,Vector3 size,Material m,float bevel=.09f){Add(BevelCube(bevel),p,Quaternion.identity,size,m);}
 static void Armband(){
  var q=Quaternion.Euler(90,0,0);Add(Ring(32,.41f,.32f,.31f),Vector3.zero,q,Vector3.one,ink);
  foreach(float y in new[]{-.16f,.16f})Add(Ring(32,.424f,.313f,.055f),Vector3.up*y,q,Vector3.one,mint);
  foreach(float x in new[]{-.19f,0,.19f})Add(Extrude(new[]{new Vector2(-.08f,-.11f),new Vector2(.01f,-.11f),new Vector2(.10f,0),new Vector2(.01f,.11f),new Vector2(-.08f,.11f),new Vector2(.015f,0)},.035f),new Vector3(x,0,-.418f),Quaternion.identity,Vector3.one,gold);
  // Sculpted speed ribbons retain their silhouette from the stadium camera.
  foreach(float y in new[]{-.25f,.25f})Add(Ring(24,.55f,.528f,.025f,0,275),new Vector3(0,y,0),Quaternion.Euler(80,y>0?25:205,-12),new Vector3(1,.78f,1),mint);
 }
 static void Portal(){
  Add(Ring(6,.55f,.40f,.17f),Vector3.zero,Quaternion.Euler(0,0,30),Vector3.one,ink);
  Add(Ring(6,.53f,.435f,.19f),Vector3.zero,Quaternion.Euler(0,0,30),Vector3.one,violet);
  Add(Ring(6,.435f,.408f,.205f),Vector3.zero,Quaternion.Euler(0,0,30),Vector3.one,blue);
  Add(Extrude(Polygon(6,.405f,30),.035f),Vector3.zero,Quaternion.identity,Vector3.one,black);
  var centre=new Vector3(.24f,-.08f,-.18f);Add(Sphere(16,10),centre,Quaternion.identity,Vector3.one*.19f,ivory);
  foreach(var n in new[]{Vector3.back,Vector3.up,Vector3.right,Vector3.left,Vector3.down,Vector3.forward})Add(Extrude(Polygon(5,.065f),.005f),centre+n*.181f,Quaternion.FromToRotation(Vector3.forward,n),Vector3.one,ink);
  for(int i=0;i<3;i++){float a=(i*120+25)*Mathf.Deg2Rad;Add(BevelCube(.15f),new Vector3(Mathf.Cos(a)*.66f,Mathf.Sin(a)*.66f,0),Quaternion.Euler(20,30,i*120+30),new Vector3(.09f,.21f,.12f),blue);}
 }
 static void Wall(float width,float height,float depth,bool deployed){
  float post=deployed?.24f:.14f,foot=deployed?.4f:.22f,bottom=deployed?0:-height*.5f;float x=(width-post)*.5f;
  foreach(float side in new[]{-1f,1f}){
   Cube(new Vector3(x*side,bottom+height*.49f,0),new Vector3(post,height*.98f,depth),orange);
   Cube(new Vector3(x*side,bottom+height*.49f,-depth*.52f),new Vector3(post*.50f,height*.60f,depth*.16f),ink);
   Cube(new Vector3(x*side,bottom+height*.49f,-depth*.62f),new Vector3(post*.20f,height*.46f,depth*.06f),blue);
   Cube(new Vector3(x*side,bottom+height*.04f,0),new Vector3(foot,height*.08f,depth*1.55f),ink);
   Cube(new Vector3(x*side,bottom+height*.13f,0),new Vector3(post*1.3f,height*.16f,depth*1.15f),orange);
  }
  float inner=width-post*2;for(int i=0;i<3;i++){float centre=(i-1)*inner/3;Add(Panel(),new Vector3(centre,bottom+height*.52f,0),Quaternion.identity,new Vector3(inner/3-.018f,height*.94f,1),glass);}
 }
 static Mesh Panel(){var m=new Mesh{name="Energy pane"};m.vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.5f,0),new Vector3(-.5f,.5f,0)};m.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};m.triangles=new[]{0,2,1,0,3,2};m.RecalculateNormals();m.RecalculateBounds();return m;}
 static Vector2[] Polygon(int sides,float radius,float angle=0){var p=new Vector2[sides];for(int i=0;i<sides;i++){float a=(angle+360f*i/sides)*Mathf.Deg2Rad;p[i]=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;}return p;}
 static Mesh Extrude(Vector2[] points,float depth){var v=new List<Vector3>();var t=new List<int>();int count=points.Length;Vector2 centre=Vector2.zero;foreach(var p in points)centre+=p;centre/=count;
  for(int i=0;i<count;i++){var a=points[i];var b=points[(i+1)%count];Face(new Vector3(centre.x,centre.y,-depth*.5f),new Vector3(b.x,b.y,-depth*.5f),new Vector3(a.x,a.y,-depth*.5f));Face(new Vector3(centre.x,centre.y,depth*.5f),new Vector3(a.x,a.y,depth*.5f),new Vector3(b.x,b.y,depth*.5f));Face(new Vector3(a.x,a.y,-depth*.5f),new Vector3(b.x,b.y,-depth*.5f),new Vector3(b.x,b.y,depth*.5f));Face(new Vector3(a.x,a.y,-depth*.5f),new Vector3(b.x,b.y,depth*.5f),new Vector3(a.x,a.y,depth*.5f));}
  return Mesh(v,t);void Face(Vector3 a,Vector3 b,Vector3 c){int n=v.Count;v.Add(a);v.Add(b);v.Add(c);t.Add(n);t.Add(n+1);t.Add(n+2);}
 }
 static Mesh Ring(int segments,float outer,float inner,float depth,float start=0,float degrees=360){var v=new List<Vector3>();var t=new List<int>();for(int i=0;i<segments;i++){float a=(start+degrees*i/segments)*Mathf.Deg2Rad,b=(start+degrees*(i+1)/segments)*Mathf.Deg2Rad;Vector3 P(float angle,float r,float z)=>new(Mathf.Cos(angle)*r,Mathf.Sin(angle)*r,z);float f=-depth*.5f,k=depth*.5f;Quad(P(a,outer,f),P(b,outer,f),P(b,inner,f),P(a,inner,f));Quad(P(a,inner,k),P(b,inner,k),P(b,outer,k),P(a,outer,k));Quad(P(a,outer,k),P(b,outer,k),P(b,outer,f),P(a,outer,f));Quad(P(a,inner,f),P(b,inner,f),P(b,inner,k),P(a,inner,k));}return Mesh(v,t);void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){int n=v.Count;v.AddRange(new[]{a,b,c,d});t.AddRange(new[]{n,n+2,n+1,n,n+3,n+2});}}
 static Mesh BevelCube(float bevel){
  // Six chamfered face outlines with bevel strips; vertices are shared only
  // within each face so highlights remain crisp at this deliberately low count.
  var v=new List<Vector3>();var t=new List<int>();float b=Mathf.Clamp(bevel,.01f,.2f),h=.5f-b;
  foreach(var normal in new[]{Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back}){
   var q=Quaternion.FromToRotation(Vector3.forward,normal);Vector2[] p={new(-h,-.5f),new(h,-.5f),new(.5f,-h),new(.5f,h),new(h,.5f),new(-h,.5f),new(-.5f,h),new(-.5f,-h)};
   for(int i=0;i<8;i++){var a=p[i]*(1-b*2);var c=p[(i+1)%8]*(1-b*2);Face(q*new Vector3(0,0,.5f),q*new Vector3(a.x,a.y,.5f),q*new Vector3(c.x,c.y,.5f));Vector3 outerA=q*new Vector3(p[i].x,p[i].y,h),outerB=q*new Vector3(p[(i+1)%8].x,p[(i+1)%8].y,h);Face(q*new Vector3(a.x,a.y,.5f),outerA,outerB);Face(q*new Vector3(a.x,a.y,.5f),outerB,q*new Vector3(c.x,c.y,.5f));}
  }return Mesh(v,t);void Face(Vector3 a,Vector3 b,Vector3 c){int n=v.Count;v.AddRange(new[]{a,b,c});t.AddRange(new[]{n,n+1,n+2});}
 }
 static Mesh Sphere(int around,int rows){var v=new List<Vector3>();var t=new List<int>();for(int y=0;y<=rows;y++){float p=Mathf.PI*y/rows;for(int x=0;x<=around;x++){float a=Mathf.PI*2*x/around;v.Add(new Vector3(Mathf.Sin(p)*Mathf.Cos(a),Mathf.Cos(p),Mathf.Sin(p)*Mathf.Sin(a)));}}for(int y=0;y<rows;y++)for(int x=0;x<around;x++){int n=y*(around+1)+x;t.AddRange(new[]{n,n+1,n+around+1,n+1,n+around+2,n+around+1});}var m=Mesh(v,t);var normals=m.vertices;for(int i=0;i<normals.Length;i++)normals[i]=normals[i].normalized;m.normals=normals;return m;}
 static Mesh Mesh(List<Vector3> v,List<int> t){var m=new Mesh();m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
 static void Save(string name){
  var materials=parts.Select(p=>p.material).Distinct().ToArray();var combined=new List<Mesh>();
  foreach(var material in materials){var m=new Mesh();m.CombineMeshes(parts.Where(p=>p.material==material).Select(p=>new CombineInstance{mesh=p.mesh,transform=p.matrix}).ToArray(),true,true);combined.Add(m);}
  var result=new Mesh{name="Football "+name};result.CombineMeshes(combined.Select(m=>new CombineInstance{mesh=m,transform=Matrix4x4.identity}).ToArray(),false,false);result.RecalculateBounds();
  string path=Root+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing){EditorUtility.CopySerialized(result,existing);UnityEngine.Object.DestroyImmediate(result);result=existing;EditorUtility.SetDirty(result);}else AssetDatabase.CreateAsset(result,path);
  var go=new GameObject(name);go.AddComponent<MeshFilter>().sharedMesh=result;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterials=materials;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
  PrefabUtility.SaveAsPrefabAsset(go,Root+name+".prefab");UnityEngine.Object.DestroyImmediate(go);foreach(var p in parts)UnityEngine.Object.DestroyImmediate(p.mesh);foreach(var m in combined)UnityEngine.Object.DestroyImmediate(m);parts.Clear();
 }
 public static void BuildReview(){ProjectBuilder.BuildWindowsPlayer("../Builds/FootballPropsQA/Player/WhatTheFish.exe");}
}
