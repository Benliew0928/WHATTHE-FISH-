using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using WhatTheFish;
using Object=UnityEngine.Object;

public static partial class GolfMultiplayerTeeBuild {
 [Serializable] sealed class T1ObstacleLayout {public GolfCircuitArtBuilder.Obstacle[] obstacles;}
 static void RestoreT1BunkerScenery(GameObject island,GolfCourse course){
  // Only placements whose original centres lie in the annotated blue circle.
  foreach(var p in RefinedIslandBuilder.Read("Golf").instances){
   if(GolfTeeSite.RestoredBunker(p.position.x,p.position.z)<.999f||!(p.module.StartsWith("Palm")||p.module.StartsWith("Rock")))continue;
   var item=island.transform.Find(p.id);if(item)item.localPosition=p.position;
  }
  var props=course.transform.Find("Circuit obstacles");
  var layout=JsonUtility.FromJson<T1ObstacleLayout>(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Game/Sports/Golf/FiveHoleCourse.json").text);
  foreach(var item in layout.obstacles??Array.Empty<GolfCircuitArtBuilder.Obstacle>()){
   if(GolfTeeSite.RestoredBunker(item.position.x,item.position.z)<.999f)continue;
   var old=props.Find(item.name);if(old)Object.DestroyImmediate(old.gameObject);GolfCircuitArtBuilder.AddObstacle(island,props,item);
  }
  var imported=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/RefinedIslands/Golf/Grass.fbx").GetComponentsInChildren<MeshFilter>(true).ToDictionary(f=>f.name,f=>f.sharedMesh);
  foreach(var filter in island.GetComponentsInChildren<MeshFilter>(true)){
   if(!filter.sharedMesh||!imported.ContainsKey(filter.name)||!AssetDatabase.GetAssetPath(filter.sharedMesh).StartsWith(Folder))continue;
   var master=AssetDatabase.LoadAssetAtPath<Mesh>(GolfCourseBuilder.Art+"Meshes/Grass/"+filter.name+".asset")??imported[filter.name];
   var matrix=island.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;var v=master.vertices;
   if(!v.Any(p=>{p=matrix.MultiplyPoint3x4(p);return GolfTeeSite.RestoredBunker(p.x,p.z)>.999f;}))continue;
   var merged=RestoreGrassTriangles(filter.sharedMesh,master,matrix);if(merged)filter.sharedMesh=Save(merged,"cleared-"+filter.name);
  }
 }
 static Mesh RestoreGrassTriangles(Mesh current,Mesh original,Matrix4x4 matrix,Func<Vector3,bool> selection=null,Func<Vector3,Vector3> warp=null){
  var cv=current.vertices;var ov=original.vertices;if(warp!=null)ov=ov.Select(warp).ToArray();
  Vector3Int Key(Vector3 v)=>Vector3Int.RoundToInt(v*1000);
  var present=new HashSet<(Vector3Int,Vector3Int,Vector3Int)>();
  for(int sub=0;sub<current.subMeshCount;sub++){var t=current.GetTriangles(sub);for(int i=0;i<t.Length;i+=3)present.Add((Key(cv[t[i]]),Key(cv[t[i+1]]),Key(cv[t[i+2]])));}
  var outputs=new List<int[]>();bool changed=false;
  for(int sub=0;sub<current.subMeshCount;sub++){
   var output=current.GetTriangles(sub).ToList();var t=original.GetTriangles(sub);
   for(int i=0;i<t.Length;i+=3){var a=ov[t[i]];var b=ov[t[i+1]];var c=ov[t[i+2]];var p=matrix.MultiplyPoint3x4((a+b+c)/3);
    if(!(selection!=null?selection(p):GolfTeeSite.RestoredBunker(p.x,p.z)>=.999f)||!present.Add((Key(a),Key(b),Key(c))))continue;
    output.AddRange(new[]{cv.Length+t[i],cv.Length+t[i+1],cv.Length+t[i+2]});changed=true;
   }
   outputs.Add(output.ToArray());
  }
  if(!changed)return null;
  var used=outputs.SelectMany(t=>t).Distinct().OrderBy(i=>i).ToArray();var remap=new Dictionary<int,int>();for(int i=0;i<used.Length;i++)remap[used[i]]=i;
  var mesh=new Mesh{name=current.name,indexFormat=used.Length>65535?IndexFormat.UInt32:IndexFormat.UInt16};var vertices=cv.Concat(ov).ToArray();mesh.SetVertices(used.Select(i=>vertices[i]).ToList());
  var cn=current.normals;var on=original.normals;if(cn.Length==cv.Length&&on.Length==ov.Length){var n=cn.Concat(on).ToArray();mesh.SetNormals(used.Select(i=>n[i]).ToList());}
  var ct=current.tangents;var ot=original.tangents;if(ct.Length==cv.Length&&ot.Length==ov.Length){var n=ct.Concat(ot).ToArray();mesh.SetTangents(used.Select(i=>n[i]).ToList());}
  var cc=current.colors;var oc=original.colors;if(cc.Length==cv.Length&&oc.Length==ov.Length){var n=cc.Concat(oc).ToArray();mesh.SetColors(used.Select(i=>n[i]).ToList());}
  for(int channel=0;channel<8;channel++){var a=new List<Vector4>();var b=new List<Vector4>();current.GetUVs(channel,a);original.GetUVs(channel,b);if(a.Count!=cv.Length||b.Count!=ov.Length)continue;var uv=a.Concat(b).ToArray();int dimension=current.GetVertexAttributeDimension(VertexAttribute.TexCoord0+channel);if(dimension==2)mesh.SetUVs(channel,used.Select(i=>new Vector2(uv[i].x,uv[i].y)).ToList());else mesh.SetUVs(channel,used.Select(i=>uv[i]).ToList());}
  mesh.subMeshCount=outputs.Count;for(int sub=0;sub<outputs.Count;sub++)mesh.SetTriangles(outputs[sub].Select(i=>remap[i]).ToArray(),sub);mesh.RecalculateBounds();return mesh;
 }
}
