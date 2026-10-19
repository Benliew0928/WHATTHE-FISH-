using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public static partial class GolfMultiplayerTeeBuild {
 struct DrapeVertex {public Vector3 p,n;public Vector2 uv;public Color color;public static DrapeVertex Lerp(DrapeVertex a,DrapeVertex b,float t)=>new DrapeVertex{p=Vector3.LerpUnclamped(a.p,b.p,t),n=Vector3.LerpUnclamped(a.n,b.n,t),uv=Vector2.LerpUnclamped(a.uv,b.uv,t),color=Color.LerpUnclamped(a.color,b.color,t)};}
 struct GroundTriangle {public Vector3 a,b,c,na,nb,nc;}
 sealed class T1GroundIndex {
  public readonly List<GroundTriangle> triangles=new();readonly Dictionary<Vector2Int,List<int>> cells=new();
  public T1GroundIndex(GameObject island,MeshCollider[] colliders){foreach(var collider in colliders){var mesh=collider.sharedMesh;var matrix=island.transform.worldToLocalMatrix*collider.transform.localToWorldMatrix;var v=mesh.vertices.Select(matrix.MultiplyPoint3x4).ToArray();var n=mesh.normals.Select(p=>matrix.MultiplyVector(p).normalized).ToArray();var t=mesh.triangles;for(int i=0;i<t.Length;i+=3){var face=new GroundTriangle{a=v[t[i]],b=v[t[i+1]],c=v[t[i+2]],na=n[t[i]],nb=n[t[i+1]],nc=n[t[i+2]]};if(Mathf.Abs(Cross(face.b-face.a,face.c-face.a))<1e-8f)continue;int id=triangles.Count;triangles.Add(face);foreach(var key in Cells(face.a,face.b,face.c)){if(!cells.TryGetValue(key,out var list)){list=new List<int>();cells[key]=list;}list.Add(id);}}}}
  static IEnumerable<Vector2Int> Cells(Vector3 a,Vector3 b,Vector3 c){int minX=Mathf.FloorToInt(Mathf.Min(a.x,Mathf.Min(b.x,c.x))/4),maxX=Mathf.FloorToInt(Mathf.Max(a.x,Mathf.Max(b.x,c.x))/4),minZ=Mathf.FloorToInt(Mathf.Min(a.z,Mathf.Min(b.z,c.z))/4),maxZ=Mathf.FloorToInt(Mathf.Max(a.z,Mathf.Max(b.z,c.z))/4);for(int z=minZ;z<=maxZ;z++)for(int x=minX;x<=maxX;x++)yield return new Vector2Int(x,z);}
  public IEnumerable<GroundTriangle> Candidates(Vector3 a,Vector3 b,Vector3 c){var used=new HashSet<int>();foreach(var key in Cells(a,b,c))if(cells.TryGetValue(key,out var ids))foreach(var id in ids)if(used.Add(id))yield return triangles[id];}
 }
 static float Cross(Vector3 a,Vector3 b)=>a.x*b.z-a.z*b.x;
 static Mesh DrapeT1Course(Mesh source,Matrix4x4 matrix,WhatTheFish.GolfTeeSite site,T1GroundIndex ground){
  var inverse=matrix.inverse;var v=source.vertices;var uv=source.uv;var normals=source.normals;var colors=source.colors;var output=new List<DrapeVertex>();var indices=new List<int>();var used=new Dictionary<(Vector3,Vector2,Color),int>();
  int Add(DrapeVertex vertex){vertex.p=inverse.MultiplyPoint3x4(vertex.p);vertex.n=inverse.MultiplyVector(vertex.n).normalized;var key=(vertex.p,vertex.uv,vertex.color);if(used.TryGetValue(key,out int id))return id;id=output.Count;used[key]=id;output.Add(vertex);return id;}
  void Emit(List<DrapeVertex> polygon){for(int j=1;j+1<polygon.Count;j++){if(Mathf.Abs(Cross(polygon[j].p-polygon[0].p,polygon[j+1].p-polygon[0].p))<1e-8f)continue;indices.Add(Add(polygon[0]));indices.Add(Add(polygon[j]));indices.Add(Add(polygon[j+1]));}}
  var t=source.triangles;
  for(int i=0;i<t.Length;i+=3){var polygon=new List<DrapeVertex>();foreach(int id in new[]{t[i],t[i+1],t[i+2]})polygon.Add(new DrapeVertex{p=matrix.MultiplyPoint3x4(v[id]),n=matrix.MultiplyVector(normals[id]).normalized,uv=uv[id],color=colors[id]});
   if(!polygon.Any(p=>T1GradingNeighborhood(site,p.p))){Emit(polygon);continue;}
   int before=indices.Count;
   foreach(var face in ground.Candidates(polygon[0].p,polygon[1].p,polygon[2].p)){
    float area=Cross(face.b-face.a,face.c-face.a),orientation=Mathf.Sign(area);var clipped=polygon;
    var corners=new[]{face.a,face.b,face.c};
    for(int edge=0;edge<3&&clipped.Count>0;edge++){
     var a=corners[edge];var b=corners[(edge+1)%3];var next=new List<DrapeVertex>();
     for(int j=0;j<clipped.Count;j++){var start=clipped[j];var end=clipped[(j+1)%clipped.Count];float ds=Cross(b-a,start.p-a)*orientation,de=Cross(b-a,end.p-a)*orientation;bool inside=ds>=0,endInside=de>=0;if(inside)next.Add(start);if(inside!=endInside)next.Add(DrapeVertex.Lerp(start,end,ds/(ds-de)));}
     clipped=next;
    }
    for(int j=0;j<clipped.Count;j++){var point=clipped[j];var delta=point.p-face.a;float b=Cross(delta,face.c-face.a)/area,c=Cross(face.b-face.a,delta)/area;point.n=(face.na*(1-b-c)+face.nb*b+face.nc*c).normalized;point.p.y=face.a.y+(face.b.y-face.a.y)*b+(face.c.y-face.a.y)*c+.025f;clipped[j]=point;}
    Emit(clipped);
   }
   if(indices.Count==before)throw new InvalidOperationException("Dressing has no native ground support at "+polygon[0].p);
  }
  var mesh=new Mesh{name=source.name,indexFormat=output.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};mesh.SetVertices(output.Select(p=>p.p).ToList());mesh.SetUVs(0,output.Select(p=>p.uv).ToList());mesh.SetColors(output.Select(p=>p.color).ToList());mesh.SetTriangles(indices,0);mesh.SetNormals(output.Select(p=>p.n).ToList());mesh.RecalculateBounds();return mesh;
 }
}
