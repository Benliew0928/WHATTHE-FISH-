#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class VenueCollisionBuild {
 const string ProxyName="Venue solid collision";
 const string Generated="Assets/_Game/Art/VenueCollision/";
 // Bake only authored solid structures. Foliage, sea, distant islands and gameplay
 // equipment are deliberately excluded; a venue-sized box would seal the routes.
 public static int Apply(GameObject root){
  int changes=0;
  foreach(var f in root.GetComponentsInChildren<MeshFilter>(true)){
   var source=PrefabUtility.GetCorrespondingObjectFromSource(f);var mesh=source?source.sharedMesh:f.sharedMesh;var renderer=f.GetComponent<MeshRenderer>();if(!mesh||!renderer)continue;
   var path=AssetDatabase.GetAssetPath(mesh);var name=mesh.name;
   Func<string,bool> materials=null;
   if(path.Contains("/CoastalStadiums/")&&path.EndsWith("_Architecture.fbx")){
    if(name.StartsWith("Aisle_")){
     // These steps already have a continuous ramp. Stacked visual step boxes
     // conflict with the controller at the ramp toe; keep the authored ramp.
     var redundant=f.transform.Find(ProxyName);if(redundant){UnityEngine.Object.DestroyImmediate(redundant.gameObject);changes++;}continue;
    }
    if(path.EndsWith("Basketball_Architecture.fbx")&&name.StartsWith("Bay_")&&name.EndsWith("_Stone")){
     var walkable=WalkwayCaps(mesh,f.transform,root.transform);
     if(f.sharedMesh!=walkable){f.sharedMesh=walkable;PrefabUtility.RecordPrefabInstancePropertyModifications(f);changes++;}mesh=walkable;
    }
    materials=m=>m=="Coastal_Stone"||m=="Coastal_Timber"||m=="Coastal_Teal"||m=="Coastal_Screen";
   }else if(path.Contains("/Football/Sunvale/")){
    if(name.Contains("Seats_LOD1")||name.StartsWith("Floodlights__")||name.StartsWith("Landscape__")||name.StartsWith("Perimeter__")||name.StartsWith("Dugouts__")||name.StartsWith("Scoreboard__")||name.StartsWith("Banners__")||name.Contains("_Canopy"))materials=m=>true;
   }else if(path.Contains("/Basketball/Rally/")){
    if(name.Contains("Seats_LOD1")||name.StartsWith("Railings__")||name.StartsWith("PerimeterPads__")||name.StartsWith("CourtsideFurniture__")||name=="Hoop__Support"||name=="Hoop__Base"||name=="Hoop__Padding"||name=="Hoop__Wheels")materials=m=>true;
   }else if(path.Contains("/CoastalIslands/")&&path.EndsWith("_Coast.fbx")){
    if(name.Contains("CoastRocks_")||name.Contains("GardenOutcrop")||name.Contains("DockRailing"))materials=m=>true;
    else if(name.Contains("PalmGarden_"))materials=m=>m=="Coast_PalmBark";
   }else if(path.Contains("/RefinedIslands/Shared/Palm_")&&name.StartsWith("LOD1__Palm_Bark")){
    materials=m=>true;
    // The old straight box missed the bent trunk and blocked empty space beside it.
    var box=f.transform.parent.parent.Find("Support collision");
    if(box){UnityEngine.Object.DestroyImmediate(box.gameObject);changes++;}
   }else if(path.Contains("/SkySail/Meshes/SailStation_")&&!path.EndsWith("_Far.asset"))materials=m=>m=="SS_Timber"||m=="SS_Teal"||m=="SS_Cream";
   else if(path.Contains("/SkySail/Meshes/CableTower_")&&!path.EndsWith("_Far.asset"))materials=m=>m=="SS_Steel"||m=="SS_Cream"||m=="SS_Teal";
   if(materials==null)continue;
   var indices=Enumerable.Range(0,mesh.subMeshCount).Where(i=>i<renderer.sharedMaterials.Length&&renderer.sharedMaterials[i]&&materials(renderer.sharedMaterials[i].name)).ToArray();
   if(indices.Length==0)continue;
   var collision=indices.Length==mesh.subMeshCount?mesh:SolidMesh(mesh,indices);
   var child=f.transform.Find(ProxyName);
   if(!child){var go=new GameObject(ProxyName);child=go.transform;child.SetParent(f.transform,false);changes++;}
   if(child.gameObject.layer!=8){child.gameObject.layer=8;changes++;}
   var collider=child.GetComponent<MeshCollider>();if(!collider){collider=child.gameObject.AddComponent<MeshCollider>();changes++;}
   if(collider.sharedMesh!=collision){collider.sharedMesh=collision;changes++;}
   // Separate child survives renderer LOD swaps without changing collision quality.
   if(collider.convex||collider.isTrigger||!collider.enabled){collider.convex=false;collider.isTrigger=false;collider.enabled=true;changes++;}
  }
  return changes;
 }
 static Mesh WalkwayCaps(Mesh source,Transform model,Transform venue){
  var v=source.vertices;var parent=Enumerable.Range(0,v.Length).ToArray();
  int Find(int i){while(parent[i]!=i){parent[i]=parent[parent[i]];i=parent[i];}return i;}
  void Join(int a,int b){parent[Find(a)]=Find(b);}
  var weld=new Dictionary<Vector3,int>();for(int i=0;i<v.Length;i++){if(weld.TryGetValue(v[i],out var j))Join(i,j);else weld.Add(v[i],i);}
  var triangles=source.triangles;for(int i=0;i<triangles.Length;i+=3){Join(triangles[i],triangles[i+1]);Join(triangles[i],triangles[i+2]);}
  bool changed=false;
  foreach(var part in Enumerable.Range(0,v.Length).GroupBy(Find)){
   var points=part.Select(i=>venue.InverseTransformPoint(model.TransformPoint(v[i]))).ToArray();
   // Only disconnected column-cap blocks above the 6.1m balcony. Lowering
   // the complete block retains its bevels and joins; columns/steps stay intact.
   if(points.Min(p=>p.y)<6.12f||points.Max(p=>p.y)>6.5f)continue;
   foreach(int i in part)v[i]=model.InverseTransformPoint(model.TransformPoint(v[i])-venue.up*.2f);changed=true;
  }
  if(!changed)return source;
  Directory.CreateDirectory(Generated);AssetDatabase.TryGetGUIDAndLocalFileIdentifier(source,out string guid,out long id);
  string path=Generated+"Walkway_"+guid+"_"+id+".asset";var prior=AssetDatabase.LoadAssetAtPath<Mesh>(path);
  if(prior&&prior.vertices.SequenceEqual(v)&&prior.normals.SequenceEqual(source.normals)&&prior.tangents.SequenceEqual(source.tangents)&&prior.colors32.SequenceEqual(source.colors32)&&prior.uv.SequenceEqual(source.uv)&&prior.triangles.SequenceEqual(source.triangles))return prior;
  var mesh=UnityEngine.Object.Instantiate(source);mesh.name=source.name;mesh.vertices=v;mesh.RecalculateBounds();
  if(prior){EditorUtility.CopySerialized(mesh,prior);UnityEngine.Object.DestroyImmediate(mesh);EditorUtility.SetDirty(prior);return prior;}
  AssetDatabase.CreateAsset(mesh,path);return mesh;
 }
 static Mesh SolidMesh(Mesh source,int[] submeshes){
  Directory.CreateDirectory(Generated);
  AssetDatabase.TryGetGUIDAndLocalFileIdentifier(source,out string guid,out long id);
  string path=Generated+guid+"_"+id+"_"+string.Join("_",submeshes)+".asset";
  var vertices=source.vertices;var output=new List<Vector3>();var triangles=new List<int>();var map=new Dictionary<int,int>();
  foreach(var sub in submeshes)foreach(int index in source.GetTriangles(sub)){
   if(!map.TryGetValue(index,out int compact)){compact=output.Count;map.Add(index,compact);output.Add(vertices[index]);}triangles.Add(compact);
  }
  var prior=AssetDatabase.LoadAssetAtPath<Mesh>(path);
  if(prior&&prior.vertices.SequenceEqual(output)&&prior.triangles.SequenceEqual(triangles))return prior;
  var mesh=new Mesh{name=source.name+" solid trunk"};if(output.Count>65535)mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
  mesh.SetVertices(output);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();
  if(prior){EditorUtility.CopySerialized(mesh,prior);UnityEngine.Object.DestroyImmediate(mesh);EditorUtility.SetDirty(prior);return prior;}
  AssetDatabase.CreateAsset(mesh,path);return mesh;
 }
 [MenuItem("WHATTHE FISH?/World/Repair venue collisions")]
 public static void Prepare(){
  if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play mode before baking venue collisions.");
  for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save open scenes before baking venue collisions.");
  var setup=EditorSceneManager.GetSceneManagerSetup();int changed=0;
  try{
   foreach(var name in Scenes){
    var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/"+name+".unity");int count=0;
    foreach(var root in scene.GetRootGameObjects())count+=Apply(root);
    if(count>0){EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}changed+=count;
   }
   AssetDatabase.SaveAssets();
  }finally{if(setup.Length>0&&setup.Any(s=>s.isLoaded&&s.isActive))EditorSceneManager.RestoreSceneManagerSetup(setup);else EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");}
  Debug.Log("VENUE_COLLISION_PREPARE changes="+changed);
 }
 public static void Diagnose(){
  var lines=new List<string>();
  foreach(var sport in new[]{WhatTheFish.SportId.Football,WhatTheFish.SportId.Basketball,WhatTheFish.SportId.Golf,WhatTheFish.SportId.Fishing}){
   EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");
   var world=UnityEngine.Object.FindFirstObjectByType<WhatTheFish.SkySailWorld>();world.scenery.localPosition=-WhatTheFish.SkySailMap.Center(sport);
   for(int i=0;i<4;i++)foreach(var c in world.stations[i].GetComponentsInChildren<Collider>(true))c.enabled=i==(int)sport;
   var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_"+sport+".unity",OpenSceneMode.Additive);foreach(var root in scene.GetRootGameObjects())root.SetActive(true);Physics.SyncTransforms();
   var points=sport switch{
    WhatTheFish.SportId.Football=>new[]{new Vector3(3.83f,.18f,-55.94f)},
    WhatTheFish.SportId.Basketball=>new[]{new Vector3(-17.16f,6.34f,-25.19f),new Vector3(22.38f,6.26f,-10.91f),new Vector3(6.12f,.05f,-18.94f)},
    WhatTheFish.SportId.Golf=>new[]{new Vector3(2.87f,4.67f,-155.96f),new Vector3(-28.88f,8.63f,121.14f)},
    _=>new[]{new Vector3(35.88f,1.33f,2.87f)}};
   foreach(var p in points){lines.Add(sport+" "+p);foreach(var c in Physics.OverlapCapsule(p+Vector3.up*.35f,p+Vector3.up*1.4f,.65f,~0,QueryTriggerInteraction.Ignore))lines.Add("  "+PathOf(c.transform)+" "+c.GetType().Name+" "+c.bounds);}
   EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetSceneByPath("Assets/_Game/Scenes/Bootstrap.unity"));
  }
  File.WriteAllLines(Path.Combine(Folder,"route-obstacles.txt"),lines);
 }
 public static void PrepareAndAudit(){Prepare();Audit();ExportSamples();}
 public static void ExportSamples(){
  Directory.CreateDirectory(Folder);
  foreach(var name in Scenes){
   var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/"+name+".unity");var samples=new List<WhatTheFish.VenueCollisionReview.Sample>();
   foreach(var c in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<MeshCollider>(true)).Where(c=>c.name==ProxyName)){
    var v=c.sharedMesh.vertices;var t=c.sharedMesh.triangles;
    // Largest vertical triangles, distributed through the structure's height,
    // exercise collision surfaces independently of collider/component existence.
    var picks=new List<(float area,Vector3 p,Vector3 n)>();
    for(int i=0;i<t.Length;i+=3){
     var a=v[t[i]];var b=v[t[i+1]];var d=v[t[i+2]];var cross=Vector3.Cross(c.transform.TransformVector(b-a),c.transform.TransformVector(d-a));float area=cross.magnitude;
     if(area<.008f)continue;var worldNormal=cross.normalized;var normal=c.transform.InverseTransformDirection(worldNormal);
     if(Mathf.Abs(worldNormal.y)>.35f)continue;picks.Add((area,(a+b+d)/3,normal));
    }
    foreach(var pick in picks.OrderByDescending(p=>p.area).Take(6))samples.Add(new WhatTheFish.VenueCollisionReview.Sample{path=PathOf(c.transform),point=pick.p,normal=pick.n});
   }
   File.WriteAllText(Path.Combine(Folder,name+"-samples.json"),JsonUtility.ToJson(new WhatTheFish.VenueCollisionReview.Samples{items=samples.ToArray()},true));
   Debug.Log("VENUE_COLLISION_SAMPLES "+name+" count="+samples.Count);
  }
  EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");
 }

 public static void BuildVerification(){Prepare();ExportSamples();ProjectBuilder.BuildWindowsPlayer("../Builds/VenueCollisionQA/Player/WhatTheFish.exe");}
 public static readonly string[] Scenes={"Bootstrap","SkySail_Football","SkySail_Basketball","SkySail_Golf","SkySail_Fishing"};
 static string Folder=>ProjectBuilder.ResolveBuildOutputPath("../Builds/VenueCollisionQA");
 static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
 public static void Audit(){
  Directory.CreateDirectory(Folder);var rows=new List<string>{"scene\tobject\tmesh\tvertices\ttriangles\tcolliders\tlayer\tcenter\tsize\tasset\tmaterials"};
  foreach(var name in Scenes){var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/"+name+".unity");
   foreach(var f in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<MeshFilter>(true))){if(!f.sharedMesh)continue;var r=f.GetComponent<Renderer>();var b=r?r.bounds:new Bounds();var cs=f.GetComponents<Collider>();
    rows.Add(string.Join("\t",name,PathOf(f.transform),f.sharedMesh.name,f.sharedMesh.vertexCount,f.sharedMesh.triangles.Length/3,string.Join(",",cs.Select(c=>c.GetType().Name+":"+c.enabled+":"+c.isTrigger)),f.gameObject.layer,b.center.ToString("F2"),b.size.ToString("F2"),AssetDatabase.GetAssetPath(f.sharedMesh),r?string.Join(",",r.sharedMaterials.Select((m,i)=>m.name+":"+f.sharedMesh.GetIndexCount(i)/3)):"physics"));
   }
  }
  File.WriteAllLines(System.IO.Path.Combine(Folder,"detailed-inventory.tsv"),rows);Debug.Log("VENUE_COLLISION_INVENTORY "+(rows.Count-1));
 }
}
#endif
