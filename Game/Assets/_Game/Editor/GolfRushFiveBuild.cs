using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WhatTheFish;
using Object=UnityEngine.Object;

public static partial class GolfMultiplayerTeeBuild {
 public static void UpgradeFiveTees(GameObject island){
  Physics.SyncTransforms();var course=island.GetComponentInChildren<GolfCourse>(true);var root=course.transform.Find("Multiplayer tee grounds");if(!root)return;
  var terrain=island.GetComponentsInChildren<MeshCollider>(true).Where(c=>c.name.StartsWith("Terrain__")&&!c.transform.IsChildOf(course.transform)).ToArray();
  for(int i=0;i<course.teeSites.Length;i++){
   var s=course.teeSites[i];if(s.hole==1||s.count==5)continue;
   var previous=s;
   foreach(Transform marker in root.Cast<Transform>().ToArray())if(marker.name=="Tee pad marker"&&Enumerable.Range(0,previous.count).Any(slot=>Vector3.Distance(marker.localPosition,previous.Pad(slot)+Vector3.up*.022f)<.1f))Object.DestroyImmediate(marker.gameObject);
   s.count=5;float top=s.height-.035f;
   for(float a=-s.HalfWidth;a<=s.HalfWidth+.001f;a+=.025f)for(float r=s.radius-4;r<=s.radius+5;r+=1.5f){float angle=s.Angle+a;top=Mathf.Max(top,Floor(island,terrain,s.cup+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*r).y);}
   s.height=top+.035f;s.centre.y=s.height;course.teeSites[i]=s;course.holes.Single(h=>h.number==s.hole).tee.position=island.transform.TransformPoint(s.centre);
   const int n=40;var vertices=new List<Vector3>();var uv=new List<Vector2>();var colors=new List<Color>();var triangles=new List<int>();
   for(int iz=0;iz<=n;iz++)for(int ix=0;ix<=n;ix++){float x=s.centre.x-20+ix,z=s.centre.z-20+iz;var p=Floor(island,terrain,new Vector3(x,0,z));float weight=s.GroundWeight(x,z);p.y=Mathf.Lerp(p.y,s.height,weight)+.007f;vertices.Add(p);uv.Add(new Vector2(x,z)/4);colors.Add(new Color(1,1,1,weight));}
   for(int iz=0;iz<n;iz++)for(int ix=0;ix<n;ix++){int a=iz*(n+1)+ix,b=a+n+1;if(new[]{a,b,a+1,b+1}.All(k=>colors[k].a==0))continue;triangles.AddRange(new[]{a,b,b+1,a,b+1,a+1});}
   var mesh=new Mesh{name="Tee terrace "+s.hole};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh=Save(mesh,"terrace-"+s.hole);
   var node=root.Find("Terrain__Tee terrace "+s.hole);node.GetComponent<MeshFilter>().sharedMesh=mesh;node.GetComponent<MeshCollider>().sharedMesh=mesh;
   foreach(var p in Enumerable.Range(0,5).Select(s.Pad)){var marker=GameObject.CreatePrimitive(PrimitiveType.Cylinder);marker.name="Tee pad marker";Object.DestroyImmediate(marker.GetComponent<Collider>());marker.transform.SetParent(root,false);marker.transform.localPosition=p+Vector3.up*.022f;marker.transform.localScale=new Vector3(.22f,.008f,.22f);marker.GetComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/RefinedIslands/Shared/Materials/RI_Teal.mat");}
  }
  Physics.SyncTransforms();EditorUtility.SetDirty(course);
 }
 public static void PrepareRushFive(){
  var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_Golf.unity");var island=scene.GetRootGameObjects().Single(r=>r.GetComponent<SkySailIslandScene>());bool active=island.activeSelf;island.SetActive(true);UpgradeFiveTees(island);island.SetActive(active);EditorSceneManager.SaveScene(scene);
  const string path="Assets/_Game/Prefabs/Environments/GolfEnvironment.prefab";var root=PrefabUtility.LoadPrefabContents(path);try{bool enabled=root.activeSelf;root.SetActive(true);UpgradeFiveTees(root);root.SetActive(enabled);PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}
  var shader=Shader.Find("WhatTheFish/GolfRushTrail");const string matPath="Assets/_Game/Resources/GolfRushTrail.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);if(!mat){mat=new Material(shader);AssetDatabase.CreateAsset(mat,matPath);}else mat.shader=shader;
  AssetDatabase.SaveAssets();Directory.CreateDirectory(ColourEvidence);AuditFiveTeeColours();CaptureTeeColours("five-tees");
 }
 public static void BuildRushFive(){PrepareRushFive();ProjectBuilder.BuildWindowsPlayer("../Builds/GolfRushFiveQA/Player/WhatTheFish.exe");}
 public static void RefreshRushFive(){var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/GolfRushFiveQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Rush five refresh failed");}
 static void AuditFiveTeeColours(){
  var lines=new List<string>();void Check(bool pass,string label){lines.Add((pass?"PASS ":"FAIL ")+label);File.WriteAllLines(Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/GolfRushFiveQA/surface-audit.txt")),lines);if(!pass)throw new Exception(label);}
  var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_Golf.unity");var island=scene.GetRootGameObjects().Single(r=>r.GetComponent<SkySailIslandScene>());island.SetActive(true);var course=island.GetComponentInChildren<GolfCourse>();
  var original=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/RefinedIslands/Golf/Golf_Terrain.mat");var turf=AssetDatabase.LoadAssetAtPath<Material>(Folder+"Tee turf.mat");Check(turf.renderQueue==2990,"tee layer precedes original transparent course dressings");Check(turf.shader.name=="WhatTheFish/GolfTeeSurface","native-lit tee shader assigned");
  foreach(string name in new[]{"_BaseMap","_Control","_Turf","_Sand","_TurfNormal","_SandNormal"})Check(turf.GetTexture(name)==original.GetTexture(name),"shared native terrain map "+name);
  Check(turf.GetColor("_BaseColor")==original.GetColor("_BaseColor")&&turf.GetFloat("_Tile")==original.GetFloat("_Tile"),"native colour and detail scale match");
  float extent=RefinedIslandBuilder.Read("Golf").extent;Check(turf.GetFloat("_IslandExtent")==extent&&turf.GetFloat("_LocalUVScale")==4,"native macro UV conversion configured");
  var native=island.GetComponentsInChildren<MeshCollider>(true).Where(c=>c.name.StartsWith("Terrain__")&&!c.transform.IsChildOf(course.transform)).ToArray();var grounds=course.transform.Find("Multiplayer tee grounds");
  foreach(var site in course.teeSites.Where(s=>s.hole>=2)){
   var filter=grounds.Find("Terrain__Tee terrace "+site.hole).GetComponent<MeshFilter>();var mesh=filter.sharedMesh;var matrix=island.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;var v=mesh.vertices;var uv=mesh.uv;var alpha=mesh.colors;bool coords=true,weights=true,macro=true;int samples=0;
   for(int i=0;i<v.Length;i++){var p=matrix.MultiplyPoint3x4(v[i]);coords&=Vector2.Distance(uv[i]*4,new Vector2(p.x,p.z))<.0001f;weights&=Mathf.Abs(alpha[i].a-site.GroundWeight(p.x,p.z))<.0001f;
    if(i%17!=0)continue;var ray=new Ray(island.transform.TransformPoint(p+Vector3.up*30),Vector3.down);foreach(var ground in native)if(ground.Raycast(ray,out var hit,60)){macro&=Vector2.Distance(uv[i]*4/(extent*2)+Vector2.one*.5f,hit.textureCoord)<.0001f;samples++;break;}
   }
   Check(coords,"T"+site.hole+" terrace coordinates preserve original texture phase");Check(macro&&samples>10,"T"+site.hole+" macro UV matches underlying native terrain");Check(weights&&alpha.Any(c=>c.a>0&&c.a<1),"T"+site.hole+" smooth five-metre boundary weights");Check(filter.GetComponent<MeshRenderer>().sharedMaterial==turf,"T"+site.hole+" uses shared native palette");
   foreach(var pad in Enumerable.Range(0,site.count).Select(site.Pad)){bool support=Physics.Raycast(island.transform.TransformPoint(pad)+Vector3.up*10,Vector3.down,out var hit,20,1<<8,QueryTriggerInteraction.Ignore);Check(support&&Mathf.Abs(hit.point.y-island.transform.TransformPoint(pad).y)<.04f,"T"+site.hole+" original pad support retained");}
  }
  Check(!ShaderUtil.ShaderHasError(turf.shader),"tee shader compiles");
 }
}
