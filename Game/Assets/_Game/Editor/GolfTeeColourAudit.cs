using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using WhatTheFish;
using Object=UnityEngine.Object;

public static partial class GolfMultiplayerTeeBuild {
 static void AuditTeeColours(){
  var lines=new List<string>();void Check(bool pass,string label){lines.Add((pass?"PASS ":"FAIL ")+label);File.WriteAllLines(Path.Combine(ColourEvidence,"surface-audit.txt"),lines);if(!pass)throw new Exception(label);}
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
  Check(OriginalEntryCorridor(-38.5f,-150)==1&&OriginalEntryCorridor(-30,-151)==1,"annotated corridor restores original terrain colour");Check(OriginalEntryCorridor(0,-145)==0&&OriginalEntryCorridor(-62,-90)==0,"restoration mask excludes bridge and cup");
  var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/RefinedIslands/Golf/Structures.fbx").GetComponentsInChildren<MeshFilter>(true).ToDictionary(f=>f.name,f=>f.sharedMesh);int pathCount=0;
  foreach(var filter in island.GetComponentsInChildren<MeshFilter>(true).Where(f=>source.ContainsKey(f.name)&&f.name.Contains("CoastalPath_"))){var master=source[filter.name];var matrix=island.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;var points=master.vertices;Check(filter.sharedMesh.vertexCount>=master.vertexCount*.01f,"original path mesh retained "+filter.name);var live=filter.sharedMesh;var lv=live.vertices.Select(matrix.MultiplyPoint3x4).ToArray();var lt=live.triangles;bool Close(Vector3 a,Vector3 b)=>Vector2.Distance(new Vector2(a.x,a.z),new Vector2(b.x,b.z))<.005f;bool Has(Vector3 a,Vector3 b,Vector3 c){for(int j=0;j<lt.Length;j+=3)if(Close(a,lv[lt[j]])&&Close(b,lv[lt[j+1]])&&Close(c,lv[lt[j+2]]))return true;return false;}var mt=master.triangles;int required=0,found=0;for(int i=0;i<mt.Length;i+=3){var a=points[mt[i]];var b=points[mt[i+1]];var c=points[mt[i+2]];var centre=matrix.MultiplyPoint3x4((a+b+c)/3);if(OriginalEntryCorridor(centre.x,centre.z)<=.001f)continue;required++;if(Has(matrix.MultiplyPoint3x4(a),matrix.MultiplyPoint3x4(b),matrix.MultiplyPoint3x4(c)))found++;}Check(required>0&&found==required,"all requested original triangles restored "+filter.name+" "+found+"/"+required);if(points.Any(v=>{var p=matrix.MultiplyPoint3x4(v);return OriginalEntryCorridor(p.x,p.z)>.5f;}))pathCount++;}
  Check(pathCount==2,"original paving and border-stone sources overlap requested corridor");
  Check(!ShaderUtil.ShaderHasError(turf.shader)&&!ShaderUtil.ShaderHasError(Shader.Find("WhatTheFish/GolfTeeIsland")),"surface shaders compile");
  lines.Add("COMPLETE checks="+lines.Count);File.WriteAllLines(Path.Combine(ColourEvidence,"surface-audit.txt"),lines);
 }
}
