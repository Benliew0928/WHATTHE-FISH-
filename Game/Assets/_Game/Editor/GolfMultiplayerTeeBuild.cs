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

// Localized derived meshes; source island FBX and existing course masters remain editable.
public static partial class GolfMultiplayerTeeBuild {
 const string Folder=GolfCourseBuilder.Art+"Meshes/MultiplayerTees/";
 static string Evidence=>Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/GolfT1SeamQA"));
 static Mesh Save(Mesh mesh,string name){var path=Folder+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);EditorUtility.SetDirty(old);return old;}AssetDatabase.CreateAsset(mesh,path);return mesh;}
 static Vector3 Floor(GameObject island,MeshCollider[] terrain,Vector3 p){var ray=new Ray(island.transform.TransformPoint(new Vector3(p.x,100,p.z)),Vector3.down);foreach(var c in terrain)if(c.Raycast(ray,out var hit,200))return island.transform.InverseTransformPoint(hit.point);foreach(float offset in new[]{-.025f,.025f,-.3f,.3f,-.6f,.6f})foreach(var c in terrain)if(c.Raycast(new Ray(ray.origin+Vector3.right*offset,Vector3.down),out var hit,200)){var at=island.transform.InverseTransformPoint(hit.point);return new Vector3(p.x,at.y,p.z);}throw new Exception("No tee ground at "+p);}
 public static void Attach(GameObject island){
  var course=island.GetComponentInChildren<GolfCourse>(true);if(!course)return;
  if(course.transform.Find("Multiplayer tee grounds")){UpgradeFiveTees(island);FinishPaths(island);return;}
  Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
  var terrain=island.GetComponentsInChildren<MeshCollider>(true).Where(c=>c.name.StartsWith("Terrain__")).ToArray();
  var sites=new List<GolfTeeSite>();
  foreach(var h in course.holes){var cup=island.transform.InverseTransformPoint(h.cup.position);var at=island.transform.InverseTransformPoint(h.tee.position);var delta=Vector3.ProjectOnPlane(at-cup,Vector3.up);float radius=h.number==1?64:delta.magnitude;at=cup+delta.normalized*radius;var site=new GolfTeeSite{hole=h.number,count=h.number==1?10:5,spacing=h.number==1?3:4,cup=cup,centre=at,radius=radius};
   // Keep the entire plateau above existing support, with a soft five-metre join.
   float top=Floor(island,terrain,at).y;for(float a=-site.HalfWidth;a<=site.HalfWidth+.001f;a+=.025f)for(float r=radius-4;r<=radius+5;r+=1.5f){float angle=site.Angle+a;top=Mathf.Max(top,Floor(island,terrain,cup+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*r).y);}
   site.height=top+.035f;site.centre=new Vector3(at.x,site.height,at.z);sites.Add(site);h.tee.position=island.transform.TransformPoint(site.centre);h.tee.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(h.cup.position-h.tee.position,Vector3.up));
   foreach(Transform marker in h.tee)marker.gameObject.SetActive(false);
  }
  course.teeSites=sites.ToArray();var root=new GameObject("Multiplayer tee grounds");root.transform.SetParent(course.transform,false);
  bool Cleared(Vector3 p)=>sites.Any(s=>s.Clears(p.x,p.z)||s.Opening(p.x,p.z));
  // Local circuit props are separate prefab instances; move only intersecting ones.
  var props=course.transform.Find("Circuit obstacles");if(props){int side=0;foreach(Transform prop in props){var p=island.transform.InverseTransformPoint(prop.position);if(!Cleared(p))continue;var s=sites.OrderBy(t=>Vector3.SqrMagnitude(t.centre-p)).First();float a=s.Angle+(++side%2==0?1:-1)*(s.HalfWidth+9/s.radius);float r=Mathf.Clamp(s.Distance(p.x,p.z),16,s.radius);var target=s.cup+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*r;if(course.SafeDryPosition(island.transform.TransformPoint(target),3))prop.position=island.transform.TransformPoint(Floor(island,terrain,target));}}
  // Trim only grass/scenery triangles whose complete small footprint is in the
  // rebuilt turf. Large remote scenery triangles stay untouched.
  foreach(var f in island.GetComponentsInChildren<MeshFilter>(true).ToArray()){
   if(!f.sharedMesh||f.name.StartsWith("Terrain__")||f.transform.IsChildOf(course.transform))continue;
   var path=AssetDatabase.GetAssetPath(f.sharedMesh);if(!(path.Contains("/Grass/")||path.EndsWith("Grass.fbx")||path.Contains("Structures")||path.Contains("CoursePropsWithoutDecorativeFlags")))continue;
   var source=f.sharedMesh;var v=source.vertices;var matrix=island.transform.worldToLocalMatrix*f.transform.localToWorldMatrix;var positions=v.Select(p=>matrix.MultiplyPoint3x4(p)).ToArray();bool changed=false;var mesh=Object.Instantiate(source);
   for(int sub=0;sub<source.subMeshCount;sub++){var t=source.GetTriangles(sub);var keep=new List<int>();for(int i=0;i<t.Length;i+=3){var a=positions[t[i]];var b=positions[t[i+1]];var c=positions[t[i+2]];var centre=(a+b+c)/3;bool trim=Cleared(centre)&&Vector3.Distance(a,b)<16&&Vector3.Distance(a,c)<16;if(trim)changed=true;else keep.AddRange(new[]{t[i],t[i+1],t[i+2]});}mesh.SetTriangles(keep,sub);}
   if(!changed){Object.DestroyImmediate(mesh);continue;}mesh.RecalculateBounds();f.sharedMesh=Save(mesh,"cleared-"+f.name);foreach(var collider in f.GetComponents<MeshCollider>())collider.sharedMesh=f.sharedMesh;
  }
  var material=ConfigureTeeTurf();
  foreach(var s in sites){
   var pads=Enumerable.Range(0,s.count).Select(s.Pad).ToArray();if(s.hole!=1){float extent=s.hole==1?23:20;var centre=s.centre;int n=Mathf.CeilToInt(extent*2);var vertices=new List<Vector3>();var uv=new List<Vector2>();var colors=new List<Color>();var triangles=new List<int>();
   for(int iz=0;iz<=n;iz++)for(int ix=0;ix<=n;ix++){float x=centre.x-extent+ix,z=centre.z-extent+iz;var p=Floor(island,terrain,new Vector3(x,0,z));float weight=s.GroundWeight(x,z);p.y=Mathf.Lerp(p.y,s.height,weight)+.007f;vertices.Add(p);uv.Add(new Vector2(x,z)/4);colors.Add(new Color(1,1,1,weight>0?1:0));}
   for(int iz=0;iz<n;iz++)for(int ix=0;ix<n;ix++){int a=iz*(n+1)+ix,b=a+n+1;if(new[]{a,b,a+1,b+1}.All(i=>colors[i].a==0))continue;triangles.AddRange(new[]{a,b,b+1,a,b+1,a+1});}
   var mesh=new Mesh{name="Tee terrace "+s.hole};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh=Save(mesh,"terrace-"+s.hole);
   var node=new GameObject("Terrain__Tee terrace "+s.hole);node.layer=8;node.transform.SetParent(root.transform,false);node.AddComponent<MeshFilter>().sharedMesh=mesh;node.AddComponent<MeshRenderer>().sharedMaterial=material;node.AddComponent<MeshCollider>().sharedMesh=mesh;
   }
   foreach(var p in pads){var marker=GameObject.CreatePrimitive(PrimitiveType.Cylinder);marker.name="Tee pad marker";Object.DestroyImmediate(marker.GetComponent<Collider>());marker.transform.SetParent(root.transform,false);marker.transform.localPosition=p+Vector3.up*.022f;marker.transform.localScale=new Vector3(.22f,.008f,.22f);marker.GetComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/RefinedIslands/Shared/Materials/RI_Teal.mat");}
  }
  Physics.SyncTransforms();
  EditorUtility.SetDirty(course);FinishPaths(island);AssetDatabase.SaveAssets();
 }
 public static void Prepare(){
  Directory.CreateDirectory(Evidence);var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_Golf.unity");var island=scene.GetRootGameObjects().Single(r=>r.GetComponent<SkySailIslandScene>());bool active=island.activeSelf;island.SetActive(true);Attach(island);FinishPaths(island);island.SetActive(active);EditorSceneManager.SaveScene(scene);
  const string path="Assets/_Game/Prefabs/Environments/GolfEnvironment.prefab";var root=PrefabUtility.LoadPrefabContents(path);try{root.SetActive(true);Attach(root);FinishPaths(root);root.SetActive(false);PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}AssetDatabase.SaveAssets();Debug.Log("GOLF_MULTIPLAYER_TEES_PREPARED");
 }
 public static void Review(){Prepare();AuditAndCapture();}
 public static void Build(){Prepare();AuditAndCapture();ProjectBuilder.BuildWindowsPlayer("../Builds/GolfT1SeamQA/Player/WhatTheFish.exe");}
 public static void Refresh(){var report=PackagedContentBuild.BuildScripts(UnityEditor.BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/GolfT1SeamQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Tee probe refresh failed");}
}
