using System.Collections.Generic;
using UnityEngine;

namespace WhatTheFish {
 // Continuous collision surfaces behind the authored opening, not the tiny net strands.
 // Parenting to each goal keeps collision aligned when its island is moved or disabled.
 public sealed class FootballGoalNet:MonoBehaviour {
  public const float RoofDrop=.43f,RoofDepth=2.2f;
  const float Thickness=.04f;
  readonly List<Mesh> meshes=new();
  public FootballNetSurface Surface {get;private set;}
  public static float RoofHeight(Bounds bounds,float front,int sign,float z)=>bounds.max.y-RoofDrop*Mathf.Clamp01((z-front)*sign/RoofDepth);
  public static void Attach(Transform goal,Transform pitch,Bounds bounds,float front,int sign){
   if(goal.Find("Goal net collision"))return;
   var root=new GameObject("Goal net collision");root.transform.SetParent(goal,false);
   var net=root.AddComponent<FootballGoalNet>();
   float back=sign>0?bounds.max.z:bounds.min.z,low=bounds.min.y,high=bounds.max.y;
   float backHigh=RoofHeight(bounds,front,sign,back),left=bounds.min.x,right=bounds.max.x;
   net.Panel(pitch,"Left",new Vector3(left,low,front),new Vector3(left,low,back),new Vector3(left,backHigh,back),new Vector3(left,high,front),Vector3.left);
   net.Panel(pitch,"Right",new Vector3(right,low,front),new Vector3(right,high,front),new Vector3(right,backHigh,back),new Vector3(right,low,back),Vector3.right);
   net.Panel(pitch,"Back",new Vector3(left,low,back),new Vector3(right,low,back),new Vector3(right,backHigh,back),new Vector3(left,backHigh,back),Vector3.forward*sign);
   net.Panel(pitch,"Roof",new Vector3(left,high,front),new Vector3(left,backHigh,back),new Vector3(right,backHigh,back),new Vector3(right,high,front),Vector3.up);
   net.Surface=FootballNetSurface.Create(goal,pitch,bounds,front,sign);
  }
  public void Contact(Vector3 point,Vector3 velocity,Vector3 inwardNormal){if(Surface)Surface.Contact(point,velocity,inwardNormal);}
  void Panel(Transform pitch,string name,Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector3 outward){
   var vertices=new[]{a,b,c,d,a+outward*Thickness,b+outward*Thickness,c+outward*Thickness,d+outward*Thickness};
   for(int i=0;i<vertices.Length;i++)vertices[i]=transform.InverseTransformPoint(pitch.TransformPoint(vertices[i]));
   var mesh=new Mesh{name="Goal net "+name+" collision"};
   mesh.vertices=vertices;mesh.triangles=new[]{0,2,1,0,3,2,4,5,6,4,6,7,0,1,5,0,5,4,1,2,6,1,6,5,2,3,7,2,7,6,3,0,4,3,4,7};mesh.RecalculateBounds();meshes.Add(mesh);
   var panel=new GameObject(name){layer=8};panel.transform.SetParent(transform,false);
   var collider=panel.AddComponent<MeshCollider>();collider.convex=true;collider.sharedMesh=mesh;collider.contactOffset=.002f;
  }
  void OnDestroy(){foreach(var mesh in meshes)if(mesh)Destroy(mesh);}
 }
}
