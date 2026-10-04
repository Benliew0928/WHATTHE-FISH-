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

// Local procedural authoring: shared flag art and derived terrain retain the FBX masters.
public static class GolfCourseBuilder {
 public const string Art="Assets/_Game/Art/Golf/FiveHoleCourse/";
 const string TerrainSource="Assets/_Game/Art/RefinedIslands/Golf/Terrain.fbx";
 const string Config="Assets/_Game/Sports/Golf/FiveHoleCourse.json",Group="Five-hole course";
 const string FlagPrefab="Assets/_Game/Prefabs/Golf/HoleFlag.prefab";
 [Serializable] sealed class Layout {public float cupRadius,cupDepth;public Hole[] holes;}
 [Serializable] sealed class Hole {public int number,par;public string title;public Vector3 cup,tee;public float greenRadius;}
 struct Vertex {
  public Vector3 p,n;public Vector4 tangent;public Vector2 uv,uv2;public Color color;
  public static Vertex Lerp(Vertex a,Vertex b,float t)=>new Vertex{p=Vector3.LerpUnclamped(a.p,b.p,t),n=Vector3.LerpUnclamped(a.n,b.n,t).normalized,tangent=Vector4.LerpUnclamped(a.tangent,b.tangent,t),uv=Vector2.LerpUnclamped(a.uv,b.uv,t),uv2=Vector2.LerpUnclamped(a.uv2,b.uv2,t),color=Color.LerpUnclamped(a.color,b.color,t)};
 }
 public static string Evidence=>Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/GolfCourseRefinementQA"));
 static string Disk(string assetPath)=>Path.Combine(Application.dataPath,assetPath.Substring("Assets/".Length));
 static T Save<T>(T asset,string path) where T:Object {
  Directory.CreateDirectory(Path.GetDirectoryName(Disk(path)));AssetDatabase.Refresh();
  var old=AssetDatabase.LoadAssetAtPath<T>(path);
  if(old){EditorUtility.CopySerialized(asset,old);if(old is Mesh mesh)mesh.UploadMeshData(false);Object.DestroyImmediate(asset);EditorUtility.SetDirty(old);return old;}
  AssetDatabase.CreateAsset(asset,path);return asset;
 }
 static Material Mat(string name,Color tint){
  var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.name=name;
  material.SetColor("_BaseColor",tint);material.SetFloat("_Smoothness",.12f);material.enableInstancing=true;
  if(material.HasProperty("_BaseMap"))material.SetTexture("_MainTex",material.GetTexture("_BaseMap"));
  return Save(material,Art+"Materials/"+name+".mat");
 }
 static GameObject Node(string name,Transform parent,Mesh mesh,Material material,bool collision=false){
  var node=new GameObject(name);node.layer=collision?8:10;node.transform.SetParent(parent,false);
  node.AddComponent<MeshFilter>().sharedMesh=mesh;node.AddComponent<MeshRenderer>().sharedMaterial=material;
  if(collision)node.AddComponent<MeshCollider>().sharedMesh=mesh;
  return node;
 }
 static Mesh Mesh(string name,List<Vector3> p,List<int> triangles,List<Vector2> uv=null){
  var mesh=new Mesh{name=name,indexFormat=p.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};mesh.SetVertices(p);mesh.SetTriangles(triangles,0);
  if(uv!=null)mesh.SetUVs(0,uv);mesh.RecalculateNormals();if(uv!=null)mesh.RecalculateTangents();mesh.RecalculateBounds();return mesh;
 }
 static void Quad(List<int> triangles,int a,int b,int c,int d){triangles.AddRange(new[]{a,b,c,a,c,d});}
 static Vector3 Floor(GameObject island,Vector3 local,MeshCollider[] terrain){
  var world=island.transform.TransformPoint(local);var ray=new Ray(new Vector3(world.x,island.transform.position.y+100,world.z),Vector3.down);
  foreach(var collider in terrain)if(collider.Raycast(ray,out var hit,200))return island.transform.InverseTransformPoint(hit.point);
  // Imported sector edges can have a millimetre seam. Sample either side while retaining the requested X/Z.
  foreach(var offset in new[]{Vector3.right*.025f,Vector3.left*.025f,Vector3.forward*.025f,Vector3.back*.025f})
   foreach(var collider in terrain)if(collider.Raycast(new Ray(ray.origin+island.transform.TransformVector(offset),Vector3.down),out var hit,200)){local.y=island.transform.InverseTransformPoint(hit.point).y;return local;}
  throw new Exception("No golf terrain at "+local);
 }
 static void RestoreTerrain(GameObject island){
  var original=AssetDatabase.LoadAssetAtPath<GameObject>(TerrainSource).GetComponentsInChildren<MeshFilter>(true).ToDictionary(f=>f.name,f=>f.sharedMesh);
  foreach(var filter in island.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name.StartsWith("Terrain__"))){
   filter.sharedMesh=original[filter.name];filter.GetComponent<MeshCollider>().sharedMesh=filter.sharedMesh;
  }
 }
 public static bool IsCourseGrass(Mesh mesh)=>AssetDatabase.GetAssetPath(mesh).StartsWith(Art+"Meshes/Grass/",StringComparison.Ordinal);
 public static void Attach(GameObject island){
  var layout=JsonUtility.FromJson<Layout>(AssetDatabase.LoadAssetAtPath<TextAsset>(Config).text);
  if(layout.holes.Length!=5||layout.holes.Select(h=>h.number).Distinct().Count()!=5)throw new Exception("Expected five distinct course holes.");
  bool active=island.activeSelf;island.SetActive(true);
  try{
   var old=island.transform.Find(Group);if(old)Object.DestroyImmediate(old.gameObject);
   RestoreTerrain(island);Physics.SyncTransforms();
   var terrain=island.GetComponentsInChildren<MeshCollider>(true).Where(c=>c.name.StartsWith("Terrain__")).ToArray();
   var root=new GameObject(Group);root.transform.SetParent(island.transform,false);var course=root.AddComponent<GolfCourse>();var holes=new List<GolfHole>();
   var aqua=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/RefinedIslands/Shared/Materials/RI_Teal.mat");
   var white=Mat("Ivory lining",new Color(.93f,.91f,.83f));var dark=Mat("Cup interior",new Color(.045f,.055f,.043f));
   PrepareFlag(white,aqua);
   foreach(var h in layout.holes){
    var at=Floor(island,h.cup,terrain);var hole=new GameObject("Hole "+h.number+" · "+h.title);hole.transform.SetParent(root.transform,false);hole.transform.localPosition=at;
    var lining=new List<Vector3>();var wallTriangles=new List<int>();var rim=new List<Vector3>();var rimTriangles=new List<int>();
    var putting=new List<Vector3>();const int sides=32;
    float[] rings={layout.cupRadius,layout.cupRadius+.035f};
    for(int r=0;r<rings.Length;r++)for(int i=0;i<sides;i++){
     float angle=i*Mathf.PI*2/sides;var p=h.cup+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*rings[r];p=Floor(island,p,terrain)-at;p.y+=.006f;
     putting.Add(p);
    }
    for(int i=0;i<sides;i++){
     var top=putting[i];lining.Add(top);lining.Add(new Vector3(top.x,-layout.cupDepth,top.z));
     int a=i*2,b=((i+1)%sides)*2;Quad(wallTriangles,a,a+1,b+1,b);
     rim.Add(top+Vector3.up*.003f);rim.Add(putting[sides+i]+Vector3.up*.003f);Quad(rimTriangles,a,b,b+1,a+1);
    }
    Node("Cup lining",hole.transform,Save(Mesh("Hole "+h.number+" lining",lining,wallTriangles),Art+"Meshes/Cups/"+h.number+"-lining.asset"),white,true);
    Node("Cup rim",hole.transform,Save(Mesh("Hole "+h.number+" rim",rim,rimTriangles),Art+"Meshes/Cups/"+h.number+"-rim.asset"),white);
    var floor=Disc("Cup floor",layout.cupRadius,32);var baseNode=Node("Cup floor",hole.transform,Save(floor,Art+"Meshes/Shared/CupFloor.asset"),dark,true);baseNode.transform.localPosition=Vector3.down*layout.cupDepth;
    // The original terrain and meadow remain the putting surface; clear only each cup opening.
    var flag=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(FlagPrefab));flag.transform.SetParent(hole.transform,false);flag.transform.localRotation=Quaternion.Euler(0,35,0);
    var number=Save(Digit(h.number),Art+"Meshes/Shared/Number"+h.number+".asset");
    foreach(float side in new[]{-.021f,.021f}){var label=Node("Flag number "+h.number,flag.transform,number,white);label.transform.localPosition=new Vector3(.48f,2.48f,side);if(side>0){label.transform.localRotation=Quaternion.Euler(0,180,0);label.transform.localPosition+=Vector3.right*.24f;}}
    var tee=new GameObject("Tee "+h.number);tee.transform.SetParent(root.transform,false);tee.transform.localPosition=Floor(island,h.tee,terrain);var direction=at-tee.transform.localPosition;direction.y=0;tee.transform.localRotation=Quaternion.LookRotation(direction);
    var teeMesh=Save(Disc("Tee marker",.20f,16),Art+"Meshes/Shared/TeeMarker.asset");
    foreach(float x in new[]{-1.2f,1.2f}){var marker=Node("Tee marker",tee.transform,teeMesh,aqua);var location=Floor(island,tee.transform.localPosition+tee.transform.localRotation*new Vector3(x,0,0),terrain);marker.transform.position=island.transform.TransformPoint(location+Vector3.up*.012f);}
    holes.Add(new GolfHole{number=h.number,par=h.par,title=h.title,cup=hole.transform,flag=flag.transform,tee=tee.transform,cupRadius=layout.cupRadius,cupDepth=layout.cupDepth,greenRadius=h.greenRadius});
   }
   course.holes=holes.ToArray();
   foreach(var filter in terrain.Select(c=>c.GetComponent<MeshFilter>())){
    var cut=Cut(filter.sharedMesh,island.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix,layout.holes,layout.cupRadius,false);
    if(!cut)continue;filter.sharedMesh=Save(cut,Art+"Meshes/Terrain/"+filter.name+".asset");filter.GetComponent<MeshCollider>().sharedMesh=filter.sharedMesh;
   }
   TrimGrass(island,layout);
   // Previous flags remain scenery and have no GolfHole record. New aqua numbered flags own the course.
   AssetDatabase.SaveAssets();Physics.SyncTransforms();
  }finally{island.SetActive(active);}
 }
 static Mesh Disc(string name,float radius,int sides){
  var p=new List<Vector3>{Vector3.zero};var t=new List<int>();for(int i=0;i<sides;i++){float a=i*Mathf.PI*2/sides;p.Add(new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius));}
  for(int i=0;i<sides;i++)t.AddRange(new[]{0,(i+1)%sides+1,i+1});return Mesh(name,p,t);
 }
 static void PrepareFlag(Material pole,Material cloth){
  var root=new GameObject("Numbered hole flag");
  var p=new List<Vector3>();var t=new List<int>();const int sides=12;
  for(int y=0;y<2;y++)for(int i=0;i<sides;i++){float a=i*Mathf.PI*2/sides;p.Add(new Vector3(Mathf.Cos(a)*.017f,y==0?-.15f:2.96f,Mathf.Sin(a)*.017f));}
  for(int i=0;i<sides;i++)Quad(t,i,i+sides,(i+1)%sides+sides,(i+1)%sides);
  Node("Flagpole",root.transform,Save(Mesh("Shared flagpole",p,t),Art+"Meshes/Shared/Flagpole.asset"),pole);
  var banner=new List<Vector3>{new Vector3(.025f,2.9f,-.012f),new Vector3(1.03f,2.9f,-.012f),new Vector3(.88f,2.3f,-.012f),new Vector3(.025f,2.3f,-.012f),new Vector3(.025f,2.9f,.012f),new Vector3(1.03f,2.9f,.012f),new Vector3(.88f,2.3f,.012f),new Vector3(.025f,2.3f,.012f)};
  var faces=new List<int>();Quad(faces,0,1,2,3);Quad(faces,4,7,6,5);for(int i=0;i<4;i++)Quad(faces,i,i+4,(i+1)%4+4,(i+1)%4);
  var uv=banner.Select(v=>new Vector2((v.x-.025f)/1.005f,(v.y-2.3f)/.6f)).ToList();
  Node("Aqua pennant",root.transform,Save(Mesh("Shared aqua pennant",banner,faces,uv),Art+"Meshes/Shared/Pennant.asset"),cloth);
  Directory.CreateDirectory(Path.GetDirectoryName(Disk(FlagPrefab)));AssetDatabase.Refresh();PrefabUtility.SaveAsPrefabAsset(root,FlagPrefab);Object.DestroyImmediate(root);
 }
 static Mesh Digit(int number){
  int[] masks={0,0x06,0x5b,0x4f,0x66,0x6d};var p=new List<Vector3>();var t=new List<int>();
  Vector4[] bars={new Vector4(.02f,.34f,.20f,.035f),new Vector4(.20f,.19f,.035f,.15f),new Vector4(.20f,.02f,.035f,.15f),new Vector4(.02f,0,.20f,.035f),new Vector4(0,.02f,.035f,.15f),new Vector4(0,.19f,.035f,.15f),new Vector4(.02f,.17f,.20f,.035f)};
  for(int i=0;i<bars.Length;i++)if((masks[number]&(1<<i))!=0){var b=bars[i];int start=p.Count;p.AddRange(new[]{new Vector3(b.x,b.y,0),new Vector3(b.x,b.y+b.w,0),new Vector3(b.x+b.z,b.y+b.w,0),new Vector3(b.x+b.z,b.y,0)});Quad(t,start,start+1,start+2,start+3);}
  return Mesh("Flag number "+number,p,t);
 }
 static void TrimGrass(GameObject island,Layout layout){
  var original=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/RefinedIslands/Golf/Grass.fbx").GetComponentsInChildren<MeshFilter>(true).ToDictionary(f=>f.name,f=>f.sharedMesh);
  foreach(var f in island.GetComponentsInChildren<MeshFilter>(true))if(original.TryGetValue(f.name,out var source)){
   f.sharedMesh=source;var trimmed=Cut(source,island.transform.worldToLocalMatrix*f.transform.localToWorldMatrix,layout.holes,layout.cupRadius+.1f,true);
   if(trimmed)f.sharedMesh=Save(trimmed,Art+"Meshes/Grass/"+f.name+".asset");
  }
 }
 static Mesh Cut(Mesh source,Matrix4x4 localToIsland,Hole[] holes,float radius,bool grass){
  var pos=source.vertices;var normal=source.normals;var uv=source.uv;var uv2=source.uv2;var tangent=source.tangents;var colors=source.colors;
  var vertices=new List<Vertex>();for(int i=0;i<pos.Length;i++)vertices.Add(new Vertex{p=pos[i],n=normal.Length==pos.Length?normal[i]:Vector3.up,uv=uv.Length==pos.Length?uv[i]:Vector2.zero,uv2=uv2.Length==pos.Length?uv2[i]:Vector2.zero,tangent=tangent.Length==pos.Length?tangent[i]:Vector4.zero,color=colors.Length==pos.Length?colors[i]:Color.white});
  var submeshes=new List<int>[source.subMeshCount];int changed=0;
  for(int sub=0;sub<submeshes.Length;sub++){
   var output=submeshes[sub]=new List<int>();var indices=source.GetTriangles(sub);
   for(int i=0;i<indices.Length;i+=3){
    var a=vertices[indices[i]];var b=vertices[indices[i+1]];var c=vertices[indices[i+2]];var pa=localToIsland.MultiplyPoint3x4(a.p);var pb=localToIsland.MultiplyPoint3x4(b.p);var pc=localToIsland.MultiplyPoint3x4(c.p);
    var nearby=holes.Where(h=>Mathf.Max(pa.x,Mathf.Max(pb.x,pc.x))>=h.cup.x-radius&&Mathf.Min(pa.x,Mathf.Min(pb.x,pc.x))<=h.cup.x+radius&&Mathf.Max(pa.z,Mathf.Max(pb.z,pc.z))>=h.cup.z-radius&&Mathf.Min(pa.z,Mathf.Min(pb.z,pc.z))<=h.cup.z+radius).ToArray();
    if(nearby.Length==0){output.AddRange(new[]{indices[i],indices[i+1],indices[i+2]});continue;}
    if(grass){
     bool remove=nearby.Any(h=>new[]{pa,pb,pc}.Any(p=>new Vector2(p.x-h.cup.x,p.z-h.cup.z).sqrMagnitude<radius*radius));
     if(remove)changed++;else output.AddRange(new[]{indices[i],indices[i+1],indices[i+2]});continue;
    }
    var pieces=new List<List<Vertex>>{new List<Vertex>{a,b,c}};
    foreach(var h in nearby){var next=new List<List<Vertex>>();foreach(var piece in pieces)next.AddRange(Outside(piece,h.cup,radius,localToIsland));pieces=next;}
    changed++;
    foreach(var piece in pieces){int start=vertices.Count;vertices.AddRange(piece);for(int k=1;k<piece.Count-1;k++)if(Vector3.Cross(piece[k].p-piece[0].p,piece[k+1].p-piece[0].p).sqrMagnitude>1e-16f)output.AddRange(new[]{start,start+k,start+k+1});}
   }
  }
  if(changed==0)return null;
  // Removed blades and replaced triangles must not leave their unused vertices in delivery meshes.
  var used=submeshes.SelectMany(s=>s).Distinct().ToArray();var remap=new int[vertices.Count];for(int i=0;i<used.Length;i++)remap[used[i]]=i;
  vertices=used.Select(i=>vertices[i]).ToList();foreach(var submesh in submeshes)for(int i=0;i<submesh.Count;i++)submesh[i]=remap[submesh[i]];
  var mesh=new Mesh{name=source.name+(grass?" · clear cup openings":" · five cup openings"),indexFormat=vertices.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};
  mesh.SetVertices(vertices.Select(v=>v.p).ToList());mesh.SetNormals(vertices.Select(v=>v.n).ToList());if(uv.Length==pos.Length)mesh.SetUVs(0,vertices.Select(v=>v.uv).ToList());if(uv2.Length==pos.Length)mesh.SetUVs(1,vertices.Select(v=>v.uv2).ToList());if(tangent.Length==pos.Length)mesh.SetTangents(vertices.Select(v=>v.tangent).ToList());if(colors.Length==pos.Length)mesh.SetColors(vertices.Select(v=>v.color).ToList());
  mesh.subMeshCount=submeshes.Length;for(int sub=0;sub<submeshes.Length;sub++)mesh.SetTriangles(submeshes[sub],sub,false);mesh.RecalculateBounds();MeshUtility.SetMeshCompression(mesh,grass?ModelImporterMeshCompression.Low:ModelImporterMeshCompression.Off);return mesh;
 }
 static IEnumerable<List<Vertex>> Outside(List<Vertex> polygon,Vector3 centre,float radius,Matrix4x4 matrix){
  var remaining=polygon;const int sides=32;
  for(int i=0;i<sides&&remaining.Count>2;i++){
   float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;var start=new Vector2(centre.x+radius*Mathf.Cos(a),centre.z+radius*Mathf.Sin(a));var end=new Vector2(centre.x+radius*Mathf.Cos(b),centre.z+radius*Mathf.Sin(b));
   var outside=Clip(remaining,start,end,matrix,false);if(outside.Count>2)yield return outside;remaining=Clip(remaining,start,end,matrix,true);
  }
 }
 static List<Vertex> Clip(List<Vertex> input,Vector2 start,Vector2 end,Matrix4x4 matrix,bool inside){
  var output=new List<Vertex>();var edge=end-start;
  float Distance(Vertex v){var p=matrix.MultiplyPoint3x4(v.p);return edge.x*(p.z-start.y)-edge.y*(p.x-start.x);}
  var previous=input[input.Count-1];float before=Distance(previous);bool keepBefore=inside?before>=0:before<=0;
  foreach(var current in input){float after=Distance(current);bool keepAfter=inside?after>=0:after<=0;if(keepAfter!=keepBefore)output.Add(Vertex.Lerp(previous,current,before/(before-after)));if(keepAfter)output.Add(current);previous=current;before=after;keepBefore=keepAfter;}
  return output;
 }
 [MenuItem("WHATTHE FISH?/Golf/Place five-hole course")]
 public static void UpdateMap(){
  Directory.CreateDirectory(Evidence);
  var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_Golf.unity");var island=scene.GetRootGameObjects().Single(r=>r.GetComponent<SkySailIslandScene>());Attach(island);EditorSceneManager.SaveScene(scene);
  const string prefab="Assets/_Game/Prefabs/Environments/GolfEnvironment.prefab";var contents=PrefabUtility.LoadPrefabContents(prefab);
  try{Attach(contents);PrefabUtility.SaveAsPrefabAsset(contents,prefab);}finally{PrefabUtility.UnloadPrefabContents(contents);}
  AssetDatabase.SaveAssets();Debug.Log("GOLF_FIVE_HOLES_MAP_UPDATED");
 }
 public static void RestoreMeadow(){
  var layout=JsonUtility.FromJson<Layout>(AssetDatabase.LoadAssetAtPath<TextAsset>(Config).text);
  var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_Golf.unity");
  TrimGrass(scene.GetRootGameObjects().Single(r=>r.GetComponent<SkySailIslandScene>()),layout);EditorSceneManager.SaveScene(scene);
  const string prefab="Assets/_Game/Prefabs/Environments/GolfEnvironment.prefab";var contents=PrefabUtility.LoadPrefabContents(prefab);
  try{TrimGrass(contents,layout);PrefabUtility.SaveAsPrefabAsset(contents,prefab);}finally{PrefabUtility.UnloadPrefabContents(contents);}
  AssetDatabase.SaveAssets();Debug.Log("GOLF_AUTHORED_MEADOW_RESTORED_FIVE_CUPS_RETAINED");
 }
}
