using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using Unity.Netcode;
using WhatTheFish;

public static class SkySailBuilder {
 const string Art="Assets/_Game/Art/SkySail/",Prefabs="Assets/_Game/Prefabs/SkySail/",Scenes="Assets/_Game/Scenes/";
 static Dictionary<string,Material> materials=new();
 [Serializable] class Spec {public Mat[] materials;}
 [Serializable] class Mat {public string name,color;public float roughness,metallic;}
 public static string[] BuildScenes()=>new[]{Scenes+"Bootstrap.unity"}.Concat(SkySailMap.Circuit.Select(s=>Scenes+SkySailStreaming.SceneName(s)+".unity")).Where(File.Exists).ToArray();
 [MenuItem("WHATTHE FISH?/Sky-Sail/Build world and Windows player")]
 public static void BuildWindows(){Prepare();ProjectBuilder.BuildCurrentWindows();File.WriteAllText("../Builds/WindowsFinal/Explore-SkySail.cmd","@echo off\r\nstart \"\" \"%~dp0WhatTheFish.exe\" -offline -skyStation\r\n");File.AppendAllText("../Builds/WindowsFinal/LATEST-BUILD.txt","Sky-Sail Circuit: four connected island stations, rideable group cabin and additive island loading. Quick launch: Explore-SkySail.cmd\n");}
 public static void RebuildPlayer(){EditorSceneManager.OpenScene(Scenes+"Bootstrap.unity");ProjectBuilder.BuildCurrentWindows();}
 public static void Prepare(){
  Directory.CreateDirectory(Art+"Materials");Directory.CreateDirectory(Art+"Distant");Directory.CreateDirectory(Art+"Meshes");Directory.CreateDirectory(Prefabs);AssetDatabase.Refresh();
  EditorSceneManager.OpenScene(Scenes+"Bootstrap.unity");var env=UnityEngine.Object.FindFirstObjectByType<SportEnvironmentController>();
  var worldOld=UnityEngine.Object.FindFirstObjectByType<SkySailWorld>();if(worldOld)UnityEngine.Object.DestroyImmediate(worldOld.gameObject);
  var bootstrap=EditorSceneManager.GetActiveScene();var full=new GameObject[4];
  foreach(SportId id in Enum.GetValues(typeof(SportId))){
   if(env.roots[(int)id])full[(int)id]=env.roots[(int)id];
   else {var scene=EditorSceneManager.OpenScene(Scenes+SkySailStreaming.SceneName(id)+".unity",OpenSceneMode.Additive);full[(int)id]=scene.GetRootGameObjects().Single(r=>r.GetComponent<SkySailIslandScene>());}
   full[(int)id].SetActive(true);full[(int)id].transform.position=Vector3.zero;
  }
  EditorSceneManager.SetActiveScene(bootstrap);PrepareModules();
  foreach(var island in full)island.SetActive(false);
  var root=new GameObject("Sky-Sail World");var world=root.AddComponent<SkySailWorld>();world.sun=env.mainLight;world.sky=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/CoastalIslands/Coast_DaylightSky.mat");
  world.scenery=new GameObject("World scenery and route modules").transform;world.scenery.SetParent(root.transform,false);world.proxies=new GameObject[4];world.stations=new GameObject[4];
  bool reuse=Environment.GetCommandLineArgs().Contains("-skyReuseProxies");
  foreach(SportId id in Enum.GetValues(typeof(SportId))){
   Debug.Log("SKY_SAIL_PROXY "+id);var island=full[(int)id];island.SetActive(true);ClearPortDecor(island,id);
   var proxy=new GameObject(id+" · approach and horizon");proxy.transform.SetParent(world.scenery,false);proxy.transform.localPosition=SkySailMap.Center(id);
   var lod=proxy.AddComponent<LODGroup>();var levels=new List<LOD>();
   for(int level=0;level<2;level++){
    string path=Art+"Distant/"+id+"_LOD"+level+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
    if(!reuse||!mesh){mesh=BuildProxy(island,level==0?1.65f:4.5f);SaveAsset(mesh,path);mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);}
    var g=new GameObject("LOD"+level);g.transform.SetParent(proxy.transform,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=Material("Distant","WhatTheFish/SkySailDistant");r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;
    levels.Add(new LOD(level==0?.16f:.001f,new Renderer[]{r}));
   }
   lod.SetLODs(levels.ToArray());lod.RecalculateBounds();world.proxies[(int)id]=proxy;
   var station=Module("SailStation",world.scenery);station.name=id+" · Sail Station";station.transform.localPosition=SkySailMap.Station(id);station.transform.localRotation=SkySailMap.Facing(id);world.stations[(int)id]=station;
   Box(station,"Deck collision",new Vector3(0,.55f,0),new Vector3(11.8f,.37f,15));
   foreach(float x in new[]{-5.6f,5.6f})Box(station,"Boardwalk railing",new Vector3(x,1.3f,0),new Vector3(.18f,1.2f,15));
   // Clear span over the boarding lane; side pillars remain solid.
   foreach(float x in new[]{-5.35f,5.35f})foreach(float z in new[]{-5.8f,4.8f})Box(station,"Timber pillar",new Vector3(x,4,z),new Vector3(.44f,7,.44f));
   Sign(station,id+"\nSKY-SAIL CIRCUIT",new Vector3(0,5.75f,-6.12f),6.2f);
   AddApproach(island,station,id);
   foreach(var renderer in island.GetComponentsInChildren<Renderer>(true))if(renderer.sharedMaterials.Any(m=>m&&(m.shader.name.Contains("CoastalWater")||m.shader.name.Contains("RefinedWater")))&&!renderer.name.Contains("Cascade")&&!renderer.name.Contains("SpringPool")){
    if(renderer.name.Contains("sea")||renderer.name.Contains("ocean"))renderer.gameObject.SetActive(false);
   }
   foreach(var t in island.GetComponentsInChildren<Transform>(true).Where(t=>t.name.Contains("cloud field")||t.name=="Sculpted clouds").ToArray())t.gameObject.SetActive(false);
   foreach(var t in island.GetComponentsInChildren<Transform>(true).Where(t=>!t.gameObject.activeSelf&&MobilePackageCleanup.IsRetiredScenery(t.name)).ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);
   var marker=island.GetComponent<SkySailIslandScene>();if(!marker)marker=island.AddComponent<SkySailIslandScene>();marker.sport=id;
   if(id==SportId.Fishing)LagoonPresentationBuilder.Attach(island);
   var destination=SceneManager.GetSceneByName(SkySailStreaming.SceneName(id));if(!destination.IsValid())destination=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
   SceneManager.MoveGameObjectToScene(island,destination);island.SetActive(false);EditorSceneManager.SaveScene(destination,Scenes+SkySailStreaming.SceneName(id)+".unity");
   env.definitions[(int)id].environmentPrefab=null;EditorUtility.SetDirty(env.definitions[(int)id]);
  }
  EditorSceneManager.SetActiveScene(bootstrap);
  for(int i=0;i<4;i++){
   var a=SkySailMap.Circuit[i];var b=SkySailMap.Circuit[(i+1)%4];
   var line=new GameObject(a+" — "+b+" · Wire line");line.transform.SetParent(world.scenery,false);
   var mesh=Wire(a,b);string path=Art+a+"_"+b+"_Wire.asset";SaveAsset(mesh,path);line.AddComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);var r=line.AddComponent<MeshRenderer>();r.sharedMaterial=Material("Cable","WhatTheFish/SkySailCable");r.shadowCastingMode=ShadowCastingMode.Off;
   PrefabUtility.SaveAsPrefabAsset(line,Prefabs+a+"_"+b+"_CableSpan.prefab");
   foreach(var pair in new[]{new[]{a,b},new[]{b,a}}){var tower=Module("CableTower",world.scenery);tower.name=pair[0]+" to "+pair[1]+" · Tower";tower.transform.localPosition=SkySailMap.Tower(pair[0],pair[1]);tower.transform.localRotation=Quaternion.LookRotation(SkySailMap.Berth(pair[1])-SkySailMap.Berth(pair[0]));}
  }
  var cabin=new GameObject("Cabin · ten passenger group");cabin.transform.SetParent(world.scenery,false);world.cabin=cabin.transform;
  foreach(string name in new[]{"CabinShell","CabinInterior","CabinHanger"})Module(name,cabin.transform);
  world.leftDoor=Module("SlidingDoor",cabin.transform).transform;world.leftDoor.localPosition=new Vector3(-.65f,0,-3.48f);
  world.rightDoor=Module("SlidingDoor",cabin.transform).transform;world.rightDoor.localPosition=new Vector3(.65f,0,-3.48f);
  PrefabUtility.SaveAsPrefabAsset(cabin,Prefabs+"GroupCabin.prefab");
  cabin.transform.localPosition=SkySailMap.Berth(SportId.Football);cabin.transform.localRotation=SkySailMap.Facing(SportId.Football);
  var ocean=GameObject.CreatePrimitive(PrimitiveType.Plane);ocean.name="Shared sea · four shoreline depth fields";UnityEngine.Object.DestroyImmediate(ocean.GetComponent<Collider>());ocean.transform.SetParent(world.scenery,false);ocean.transform.localPosition=Vector3.down*.43f;ocean.transform.localScale=new Vector3(1400,1,1400);
  var water=Material("WorldOcean","WhatTheFish/SkySailOcean");foreach(var pair in new[]{new[]{"Golf","Golf"},new[]{"Fish","Fishing"}}){water.SetTexture("_"+pair[0]+"Color",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/RefinedIslands/"+pair[1]+"/"+pair[1]+"_WaterColor.png"));water.SetTexture("_"+pair[0]+"Depth",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/RefinedIslands/"+pair[1]+"/"+pair[1]+"_WaterDepth.png"));}EditorUtility.SetDirty(water);ocean.GetComponent<Renderer>().sharedMaterial=water;ocean.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
  Clouds(world.scenery);
  env.roots=new GameObject[4];var streaming=env.GetComponent<SkySailStreaming>();if(!streaming)streaming=env.gameObject.AddComponent<SkySailStreaming>();streaming.enabledForWorld=true;
  UnityEngine.Object.FindFirstObjectByType<NetworkManager>().NetworkConfig.ProtocolVersion=RoomService.ProtocolVersion;
  world.scenery.localPosition=-SkySailMap.Center(SportId.Football);
  EditorSceneManager.MarkSceneDirty(bootstrap);EditorSceneManager.SaveScene(bootstrap);
  foreach(var island in full)EditorSceneManager.CloseScene(island.scene,true);
  EditorSceneManager.SetActiveScene(bootstrap);EditorBuildSettings.scenes=BuildScenes().Select(p=>new EditorBuildSettingsScene(p,true)).ToArray();AssetDatabase.SaveAssets();
  Debug.Log("SKY_SAIL_WORLD_COMPLETE scenes="+BuildScenes().Length);
 }
 static void PrepareModules(){
  materials.Clear();var specs=JsonUtility.FromJson<Spec>(File.ReadAllText(Art+"modules.json"));
  foreach(var s in specs.materials){var m=Material(s.name,"Universal Render Pipeline/Lit");ColorUtility.TryParseHtmlString("#"+s.color,out var color);m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",s.metallic);m.SetFloat("_Smoothness",1-s.roughness);m.enableInstancing=true;
   string texture=s.name switch {"SS_Timber"=>"Timber","SS_Teal"=>"Teal","SS_Cream"=>"Stone","SS_Bronze"=>"Bronze",_=>null};
   if(texture!=null){string prefix="Assets/_Game/Art/RefinedIslands/Shared/RI_"+texture;m.SetColor("_BaseColor",Color.white);m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(prefix+"_BaseColor.png"));m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(prefix+"_Normal.png"));m.SetFloat("_BumpScale",.35f);m.EnableKeyword("_NORMALMAP");}
   if(s.name=="SS_Glass"){color.a=.19f;m.SetColor("_BaseColor",color);m.SetFloat("_Surface",1);m.SetFloat("_ZWrite",0);m.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);m.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.SetOverrideTag("RenderType","Transparent");m.renderQueue=3000;m.SetFloat("_Cull",0);}
   if(s.name=="SS_Lamp"){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*.65f);}EditorUtility.SetDirty(m);materials[s.name]=m;
  }
  foreach(string file in Directory.GetFiles(Art,"*.fbx")){
   var importer=(ModelImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));importer.globalScale=1;importer.useFileScale=true;importer.importCameras=false;importer.importLights=false;importer.importAnimation=false;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.importNormals=ModelImporterNormals.Import;importer.isReadable=false;importer.SaveAndReimport();
   var model=PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(file.Replace('\\','/'))) as GameObject;var module=new GameObject(Path.GetFileNameWithoutExtension(file));model.transform.SetParent(module.transform,false);model.transform.localRotation=Quaternion.Euler(0,180,0);
   foreach(var r in model.GetComponentsInChildren<Renderer>()){r.sharedMaterials=r.sharedMaterials.Select(m=>materials[m.name]).ToArray();r.gameObject.layer=10;}
   var filters=model.GetComponentsInChildren<MeshFilter>();
   foreach(var group in filters.GroupBy(f=>f.GetComponent<Renderer>().sharedMaterial)){
    var merged=new Mesh{name=module.name+"_"+group.Key.name,indexFormat=IndexFormat.UInt32};merged.CombineMeshes(group.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=module.transform.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray(),true,true);
    string meshPath=Art+"Meshes/"+merged.name+".asset";SaveAsset(merged,meshPath);var node=new GameObject(group.Key.name);node.transform.SetParent(module.transform,false);node.AddComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);node.AddComponent<MeshRenderer>().sharedMaterial=group.Key;
   }
   UnityEngine.Object.DestroyImmediate(model);
   var detailed=module.GetComponentsInChildren<Renderer>();var distant=BuildProxy(module,module.name=="CableTower"?.40f:.20f);string distantPath=Art+"Meshes/"+module.name+"_Far.asset";SaveAsset(distant,distantPath);
   var far=new GameObject("LOD1 distant module");far.transform.SetParent(module.transform,false);far.AddComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(distantPath);var farRenderer=far.AddComponent<MeshRenderer>();farRenderer.sharedMaterial=Material("Distant","WhatTheFish/SkySailDistant");farRenderer.shadowCastingMode=ShadowCastingMode.Off;
   var lod=module.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.10f,detailed),new LOD(.002f,new Renderer[]{farRenderer})});lod.RecalculateBounds();
   PrefabUtility.SaveAsPrefabAsset(module,Prefabs+module.name+".prefab");UnityEngine.Object.DestroyImmediate(module);
  }
 }
 static GameObject Module(string name,Transform parent){var g=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+name+".prefab"));g.transform.SetParent(parent,false);return g;}
 static Material Material(string name,string shader){string path=Art+"Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,path);}m.shader=Shader.Find(shader);return m;}
 // Rebuilding the world must retain the mobile packing used by delivered meshes.
 static ModelImporterMeshCompression Packing(string path)=>path.Contains("/Distant/")?ModelImporterMeshCompression.High:ModelImporterMeshCompression.Low;
 static void SaveAsset(UnityEngine.Object value,string path){if(value is Mesh mesh)MeshUtility.SetMeshCompression(mesh,Packing(path));var old=AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);if(old){EditorUtility.CopySerialized(value,old);UnityEngine.Object.DestroyImmediate(value);}else AssetDatabase.CreateAsset(value,path);}
 public static void PrepareMobileMeshes(){
  foreach(string guid in AssetDatabase.FindAssets("t:Mesh",new[]{Art})){
   string path=AssetDatabase.GUIDToAssetPath(guid);if(!path.EndsWith(".asset"))continue;
   var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!mesh)continue;
   if((int)MeshUtility.GetMeshCompression(mesh)<(int)Packing(path)){MeshUtility.SetMeshCompression(mesh,Packing(path));EditorUtility.SetDirty(mesh);}
  }
  AssetDatabase.SaveAssets();
 }
 static void Box(GameObject root,string name,Vector3 p,Vector3 size){var g=new GameObject(name);g.layer=8;g.transform.SetParent(root.transform,false);g.transform.localPosition=p;g.AddComponent<BoxCollider>().size=size;}
 static void Sign(GameObject root,string words,Vector3 p,float width){
  for(int side=0;side<2;side++){
   var g=new GameObject("Station sign "+side,typeof(RectTransform),typeof(Canvas));g.transform.SetParent(root.transform,false);g.transform.localPosition=p+Vector3.forward*(side*.24f);g.transform.localRotation=Quaternion.Euler(0,side*180,0);g.transform.localScale=Vector3.one*.01f;g.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;g.GetComponent<RectTransform>().sizeDelta=new Vector2(width*100,100);
   var label=new GameObject("Station name",typeof(RectTransform),typeof(UnityEngine.UI.Text));label.transform.SetParent(g.transform,false);label.GetComponent<RectTransform>().sizeDelta=new Vector2(width*100,100);var t=label.GetComponent<UnityEngine.UI.Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.material=Material("Sign","WhatTheFish/SkySailSign");t.text=words;t.alignment=TextAnchor.MiddleCenter;t.fontSize=38;t.color=new Color(1,.93f,.72f);t.raycastTarget=false;
  }
 }
 static void ClearPortDecor(GameObject island,SportId id){
  var refined=island.GetComponent<RefinedIslandEnvironment>();if(!refined)return;
  var inverse=Quaternion.Inverse(SkySailMap.Facing(id));var port=SkySailMap.Port(id);
  foreach(var item in refined.layout.instances){
   if(!item.module.StartsWith("Rock_")&&!item.module.StartsWith("Palm_")&&!item.module.StartsWith("Plant_"))continue;
   var node=island.transform.Find(item.id);if(!node)continue;
   var renderers=node.GetComponentsInChildren<Renderer>(true);if(renderers.Length==0)continue;
   var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
   var p=inverse*(bounds.center-port);float radius=Mathf.Max(bounds.extents.x,bounds.extents.z);
   bool deck=Mathf.Abs(p.x)<6.1f+radius&&Mathf.Abs(p.z)<7.8f+radius;
   bool approach=Mathf.Abs(p.x)<2.9f+radius&&p.z>-30-radius&&p.z<-7+radius;
   if(deck||approach){node.gameObject.SetActive(false);Debug.Log("SKY_PORT_CLEAR "+id+" "+item.id);}
  }
 }
 static void AddApproach(GameObject island,GameObject station,SportId id){
  var old=island.transform.Find("Sky-Sail shore approach");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
  var root=new GameObject("Sky-Sail shore approach");root.transform.SetParent(island.transform,false);
  Vector3 end=SkySailMap.Port(id)+SkySailMap.Facing(id)*new Vector3(0,.72f,-7.4f),start=id==SportId.Fishing?new Vector3(36,1.4f,0):end-SkySailMap.Facing(id)*Vector3.forward*22;
  Physics.SyncTransforms();if(Physics.Raycast(start+Vector3.up*30,Vector3.down,out var hit,70,1<<8))start.y=hit.point.y+.14f;else start.y=1.2f;
  foreach(var point in new[]{("Inland arrival",start),("Platform arrival",end)}){var marker=new GameObject(point.Item1);marker.transform.SetParent(root.transform,false);marker.transform.localPosition=point.Item2;}
  int segments=5;for(int i=0;i<segments;i++){
   var a=Vector3.Lerp(start,end,i/(float)segments);var b=Vector3.Lerp(start,end,(i+1f)/segments);var deck=Module("Gangway",root.transform);deck.transform.localPosition=(a+b)*.5f;deck.transform.localRotation=Quaternion.LookRotation(b-a);deck.transform.localScale=new Vector3(1,1,Vector3.Distance(a,b)/6);Box(deck,"Ramp support",Vector3.down*.06f,new Vector3(4.9f,.15f,6));
   foreach(float x in new[]{-2.42f,2.42f})Box(deck,"Ramp guard",new Vector3(x,.6f,0),new Vector3(.15f,1.2f,6));
  }
  foreach(var c in island.GetComponentsInChildren<Collider>(true))if(c.gameObject.layer==9){var p=c.bounds.center;var d=end-start;float f=Mathf.Clamp01(Vector3.Dot(p-start,d)/d.sqrMagnitude);var nearest=start+d*f;if(Vector2.Distance(new Vector2(p.x,p.z),new Vector2(nearest.x,nearest.z))<3.4f)c.enabled=false;}
 }
 static Mesh Wire(SportId a,SportId b){var v=new List<Vector3>();var tris=new List<int>();const int count=400,sides=6;
  for(int line=0;line<2;line++){int offset=v.Count;for(int i=0;i<=count;i++){float t=i/(float)count;var p=SkySailMap.Path(a,b,t)+Vector3.up*SkySailMap.HangerHeight;var tangent=SkySailMap.Tangent(a,b,t);var right=Vector3.Cross(Vector3.up,tangent).normalized;var up=Vector3.Cross(tangent,right).normalized;p+=right*(line==0?0:.5f);for(int j=0;j<sides;j++){float angle=j*Mathf.PI*2/sides;v.Add(p+(right*Mathf.Cos(angle)+up*Mathf.Sin(angle))*.047f);}if(i==0)continue;for(int j=0;j<sides;j++){int x=offset+(i-1)*sides+j,y=offset+(i-1)*sides+(j+1)%sides;tris.AddRange(new[]{x,y,y+sides,x,y+sides,x+sides});}}}
  var mesh=new Mesh{name=a+"_"+b+"_Cable",indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
 }
 static void Clouds(Transform parent){var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/CoastalIslands/Cumulus.fbx");var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/CoastalIslands/Coast_Cloud.mat");for(int i=0;i<35;i++){float angle=i*Mathf.PI*2/35;var g=(GameObject)PrefabUtility.InstantiatePrefab(source);g.name="Shared cloud "+i;g.transform.SetParent(parent,false);g.transform.localPosition=new Vector3(Mathf.Cos(angle)*1900,190+i%5*24,Mathf.Sin(angle)*1900);g.transform.localScale=Vector3.Scale(g.transform.localScale,new Vector3(20,12,17));foreach(var r in g.GetComponentsInChildren<Renderer>()){r.sharedMaterial=mat;r.shadowCastingMode=ShadowCastingMode.Off;}}}
 struct Cell:IEquatable<Cell>{public int x,y,z;public Cell(Vector3 p,float step){x=Mathf.RoundToInt(p.x/step);y=Mathf.RoundToInt(p.y/step);z=Mathf.RoundToInt(p.z/step);}public bool Equals(Cell p)=>x==p.x&&y==p.y&&z==p.z;public override int GetHashCode()=>HashCode.Combine(x,y,z);}
 sealed class Vertex {public Vector3 p,n;public Color c;public int count;}
 static Mesh BuildProxy(GameObject root,float step){
  var skip=new HashSet<Renderer>();foreach(var group in root.GetComponentsInChildren<LODGroup>(true)){var lods=group.GetLODs();var selected=lods.LastOrDefault(l=>l.renderers.Length>0).renderers??Array.Empty<Renderer>();foreach(var r in lods.SelectMany(l=>l.renderers))if(!selected.Contains(r))skip.Add(r);}
  var cells=new Dictionary<Cell,int>();var vertices=new List<Vertex>();var triangles=new List<int>();var textures=new Dictionary<Texture,Color[]>();
  foreach(var f in root.GetComponentsInChildren<MeshFilter>(true)){
   var r=f.GetComponent<MeshRenderer>();if(!r||!r.gameObject.activeInHierarchy||!f.sharedMesh||skip.Contains(r))continue;string name=f.name.ToLowerInvariant();
   if(name.Contains("cloud")||name.Contains("grass")||name.Contains("flower")||name.Contains("sky-sail")||name.Contains("sea")&&!name.Contains("seat")&&!name.Contains("arch")||f.GetComponentInParent<LagoonPresentation>())continue;
   if(r.bounds.size.magnitude<1.2f)continue;
   var mesh=f.sharedMesh;Vector3[] positions=mesh.vertices,normals=mesh.normals;var uv=mesh.uv;var transform=root.transform.worldToLocalMatrix*f.transform.localToWorldMatrix;
   for(int sub=0;sub<mesh.subMeshCount;sub++){
    var material=r.sharedMaterials[Mathf.Min(sub,r.sharedMaterials.Length-1)];if(!material||material.shader.name.Contains("Water")||material.shader.name.Contains("Cloud"))continue;
    Color tint=material.HasProperty("_BaseColor")?material.GetColor("_BaseColor"):Color.white;Texture tex=material.HasProperty("_BaseMap")?material.GetTexture("_BaseMap"):null;
    Color[] pixels=null;if(tex&&!textures.TryGetValue(tex,out pixels)){string path=AssetDatabase.GetAssetPath(tex);if(File.Exists(path)&&path.EndsWith(".png")){var tmp=new Texture2D(2,2);ImageConversion.LoadImage(tmp,File.ReadAllBytes(path));pixels=new Color[4096];for(int y=0;y<64;y++)for(int x=0;x<64;x++)pixels[y*64+x]=tmp.GetPixelBilinear((x+.5f)/64,(y+.5f)/64);UnityEngine.Object.DestroyImmediate(tmp);}textures[tex]=pixels;}
    var remap=new Dictionary<int,int>();var indices=mesh.GetTriangles(sub);
    for(int j=0;j<indices.Length;j+=3){int[] tri=new int[3];for(int k=0;k<3;k++){int index=indices[j+k];if(!remap.TryGetValue(index,out int result)){
      var p=transform.MultiplyPoint3x4(positions[index]);var key=new Cell(p,step);if(!cells.TryGetValue(key,out result)){result=vertices.Count;cells.Add(key,result);vertices.Add(new Vertex());}
      Color c=tint;if(pixels!=null&&uv.Length>index){var coord=uv[index];coord=Vector2.Scale(coord,material.GetTextureScale("_BaseMap"))+material.GetTextureOffset("_BaseMap");int x=Mathf.Clamp((int)(Mathf.Repeat(coord.x,1)*64),0,63),y=Mathf.Clamp((int)(Mathf.Repeat(coord.y,1)*64),0,63);c*=pixels[y*64+x];}
      var entry=vertices[result];entry.p+=p;entry.n+=normals.Length>index?transform.MultiplyVector(normals[index]).normalized:Vector3.up;entry.c+=c.linear;entry.count++;remap[index]=result;
     }tri[k]=result;}
     if(tri[0]!=tri[1]&&tri[0]!=tri[2]&&tri[1]!=tri[2])triangles.AddRange(tri);
    }
   }
  }
  var output=new Mesh{name=root.name+" distant "+step,indexFormat=IndexFormat.UInt32};output.SetVertices(vertices.Select(v=>v.p/v.count).ToList());output.SetNormals(vertices.Select(v=>v.n.normalized).ToList());output.SetColors(vertices.Select(v=>v.c/v.count).ToList());output.SetTriangles(triangles,0);output.RecalculateBounds();
  Debug.Log("SKY_SAIL_PROXY_MESH "+root.name+" step="+step+" vertices="+output.vertexCount+" triangles="+triangles.Count/3);return output;
 }
}
