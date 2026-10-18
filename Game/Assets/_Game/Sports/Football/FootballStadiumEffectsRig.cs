using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 // Twelve permanent mounts share one static mesh and two materials. All are
 // outside the playable pitch and contain no gameplay components or colliders.
 internal sealed class FootballStadiumEffectsRig {
  readonly Mesh mesh;readonly Material metal,trim;
  readonly List<Vector3> vertices=new();readonly List<int>[] indices={new(),new()};
  public static Vector3 FlameOrigin(int index)=>new(index%2==0?-39:39,6,index/2==0?-48:index/2==1?-22:index/2==2?22:48);
  public static Vector3 LaserOrigin(int index)=>new(index%2==0?-23:23,18,index<2?-72:72);
  public FootballStadiumEffectsRig(Transform parent){
   metal=new Material(Resources.Load<Material>("FootballStadiumScreenFrame")){name="Stadium emitter dark metal"};metal.SetColor("_BaseColor",new Color(.025f,.085f,.10f));
   trim=new Material(metal){name="Stadium emitter bronze trim"};trim.SetColor("_BaseColor",new Color(.83f,.40f,.075f));trim.SetFloat("_Metallic",.5f);
   for(int i=0;i<8;i++){
    var origin=FlameOrigin(i);var foot=origin-Vector3.up*5.7f;
    Cylinder(foot,.9f,.35f,0);Cylinder(foot+Vector3.up*.28f,.67f,.20f,1);
    Box(foot+Vector3.up*2.7f,new Vector3(.7f,4.9f,.7f),0);
    Box(foot+Vector3.up*2.7f,new Vector3(.76f,3.5f,.16f),1);
    Cylinder(origin-Vector3.up*.60f,.82f,.28f,1);Cylinder(origin-Vector3.up*.38f,.72f,.30f,0);Cylinder(origin-Vector3.up*.12f,.34f,.24f,1);
   }
   for(int i=0;i<4;i++){
    var origin=LaserOrigin(i);Cylinder(new Vector3(origin.x,.35f,origin.z),.85f,.4f,0);
    Box(new Vector3(origin.x,9,origin.z),new Vector3(.55f,17.4f,.55f),0);
    Cylinder(new Vector3(origin.x,6,origin.z),.36f,.25f,1);Cylinder(new Vector3(origin.x,12,origin.z),.36f,.25f,1);
    Box(origin-Vector3.up*.35f,new Vector3(2.1f,.4f,.75f),1);
    for(int head=0;head<3;head++)Cylinder(origin+Vector3.right*((head-1)*.6f)-Vector3.up*.08f,.22f,.3f,0);
   }
   mesh=new Mesh{name="Combined stadium flame and laser fixtures",subMeshCount=2};mesh.SetVertices(vertices);mesh.SetTriangles(indices[0],0);mesh.SetTriangles(indices[1],1);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh.UploadMeshData(true);
   var go=new GameObject("Eight flame pylons and four roof laser mounts",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;
   var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterials=new[]{metal,trim};renderer.shadowCastingMode=ShadowCastingMode.Off;
  }
  void Face(Vector3 a,Vector3 b,Vector3 c,Vector3 d,int material){int n=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);vertices.Add(d);indices[material].AddRange(new[]{n,n+1,n+2,n,n+2,n+3});}
  void Box(Vector3 p,Vector3 size,int material){
   var x=Vector3.right*size.x*.5f;var y=Vector3.up*size.y*.5f;var z=Vector3.forward*size.z*.5f;
   Face(p-x-y-z,p-x+y-z,p+x+y-z,p+x-y-z,material);Face(p+x-y+z,p+x+y+z,p-x+y+z,p-x-y+z,material);
   Face(p-x-y+z,p-x+y+z,p-x+y-z,p-x-y-z,material);Face(p+x-y-z,p+x+y-z,p+x+y+z,p+x-y+z,material);
   Face(p-x+y-z,p-x+y+z,p+x+y+z,p+x+y-z,material);Face(p-x-y+z,p-x-y-z,p+x-y-z,p+x-y+z,material);
  }
  void Cylinder(Vector3 p,float radius,float height,int material){
   for(int i=0;i<8;i++){
    float a=i*Mathf.PI/4,b=(i+1)*Mathf.PI/4;var u=new Vector3(Mathf.Sin(a)*radius,0,Mathf.Cos(a)*radius);var v=new Vector3(Mathf.Sin(b)*radius,0,Mathf.Cos(b)*radius);var y=Vector3.up*(height*.5f);
    Face(p+u-y,p+v-y,p+v+y,p+u+y,material);Face(p+y,p+u+y,p+v+y,p+y,material);Face(p-y,p+v-y,p+u-y,p-y,material);
   }
  }
  public void Dispose(){if(mesh)Object.Destroy(mesh);if(metal)Object.Destroy(metal);if(trim)Object.Destroy(trim);}
 }
}
