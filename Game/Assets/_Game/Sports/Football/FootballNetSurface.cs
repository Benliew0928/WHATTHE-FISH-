using System.Collections.Generic;
using UnityEngine;

namespace WhatTheFish {
 // A tessellated replacement for the authored two-endpoint strands. Masters and
 // the original collision panels stay intact; only the visible cloth deforms.
 public sealed class FootballNetSurface:MonoBehaviour {
  public const int Capacity=4;
  public const float Lifetime=1.8f,MaximumDeflection=.55f;
  public Vector4 Dimensions {get;private set;}
  public uint ImpactCount {get;private set;}
  public Vector3 LastContact {get;private set;}
  public Vector3 LastImpulse {get;private set;}
  public int Sign {get;private set;}
  public float LastImpactTime {get;private set;}=-10;
  public Mesh SurfaceMesh=>mesh;
  readonly Vector4[] contacts=new Vector4[Capacity],impulses=new Vector4[Capacity];
  Mesh mesh;MeshRenderer surface,authored;bool authoredEnabled;MaterialPropertyBlock properties;int next;
  static readonly int SizeId=Shader.PropertyToID("_NetSize"),ContactsId=Shader.PropertyToID("_Contacts"),ImpulsesId=Shader.PropertyToID("_Impulses");
  public static FootballNetSurface Create(Transform goal,Transform pitch,Bounds bounds,float front,int sign){
   var source=Resources.Load<Material>("FootballGoalNet");if(!source){Debug.LogError("Missing reactive football net material.",goal);return null;}
   var root=new GameObject("Reactive goal net");root.transform.SetParent(goal,false);
   root.transform.SetPositionAndRotation(pitch.TransformPoint(new Vector3(bounds.center.x,bounds.min.y,front)),pitch.rotation*Quaternion.Euler(0,sign>0?0:180,0));
   var net=root.AddComponent<FootballNetSurface>();net.Sign=sign;
   float depth=Mathf.Abs((sign>0?bounds.max.z:bounds.min.z)-front);
   net.Dimensions=new Vector4(bounds.extents.x,bounds.size.y,depth,FootballGoalNet.RoofDrop*Mathf.Clamp01(depth/FootballGoalNet.RoofDepth));
   net.Build(source);
   foreach(var candidate in goal.GetComponentsInChildren<MeshRenderer>(true))if(candidate.name=="Goal__Net"){
    net.authored=candidate;net.authoredEnabled=candidate.enabled;candidate.enabled=false;break;
   }
   return net;
  }
  void Build(Material material){
   var vertices=new List<Vector3>();var uv=new List<Vector2>();var normals=new List<Vector3>();var indices=new List<int>();
   float w=Dimensions.x,h=Dimensions.y,d=Dimensions.z,back=h-Dimensions.w;
   void Panel(Vector3 a,Vector3 b,Vector3 c,Vector3 e,int columns,int rows,Vector3 normal){
    int start=vertices.Count,nx=columns*2,ny=rows*2;
    for(int y=0;y<=ny;y++)for(int x=0;x<=nx;x++){
     float u=x/(float)nx,v=y/(float)ny;
     vertices.Add(Vector3.Lerp(Vector3.Lerp(a,b,u),Vector3.Lerp(e,c,u),v));uv.Add(new Vector2(u*columns,v*rows));normals.Add(normal);
    }
    for(int y=0;y<ny;y++)for(int x=0;x<nx;x++){
     int i=start+y*(nx+1)+x;indices.Add(i);indices.Add(i+nx+2);indices.Add(i+1);indices.Add(i);indices.Add(i+nx+1);indices.Add(i+nx+2);
    }
   }
   int across=Mathf.Max(2,Mathf.RoundToInt(w*2/.2f)),deep=Mathf.Max(2,Mathf.RoundToInt(d/.2f)),high=Mathf.Max(2,Mathf.RoundToInt(back/.2f));
   Panel(new Vector3(-w,0,d),new Vector3(w,0,d),new Vector3(w,back,d),new Vector3(-w,back,d),across,high,Vector3.forward);
   Panel(new Vector3(-w,0,0),new Vector3(-w,0,d),new Vector3(-w,back,d),new Vector3(-w,h,0),deep,high,Vector3.left);
   Panel(new Vector3(w,0,0),new Vector3(w,0,d),new Vector3(w,back,d),new Vector3(w,h,0),deep,high,Vector3.right);
   Panel(new Vector3(-w,h,0),new Vector3(w,h,0),new Vector3(w,back,d),new Vector3(-w,back,d),across,deep,new Vector3(0,1,Dimensions.w/d).normalized);
   mesh=new Mesh{name="Subdivided reactive goal net"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetNormals(normals);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();
   gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;surface=gameObject.AddComponent<MeshRenderer>();surface.sharedMaterial=material;
   var bounds=mesh.bounds;bounds.Expand(MaximumDeflection*2);surface.localBounds=bounds;
   properties=new MaterialPropertyBlock();properties.SetVector(SizeId,Dimensions);Clear();
  }
  public static float PinWeight(Vector3 p,Vector4 size){
   float side=Mathf.Abs(Mathf.Abs(p.x)-size.x),roof=size.y-size.w*Mathf.Clamp01(p.z/size.z);
   float support=Mathf.Min(Mathf.Min(Mathf.Abs(p.z),Mathf.Abs(p.y)),Mathf.Min(new Vector2(side,p.y-roof).magnitude,new Vector2(side,p.z-size.z).magnitude));
   return Mathf.SmoothStep(0,1,Mathf.Clamp01(support/.28f));
  }
  // Shared analytic response for validation and queries; the shader evaluates
  // the same response on the GPU. Supports remain fixed and seams share positions.
  public Vector3 Displacement(Vector3 p,float time){
   Vector3 offset=Vector3.zero;
   for(int i=0;i<Capacity;i++){
    var c=contacts[i];float distance=Vector3.Distance(p,new Vector3(c.x,c.y,c.z));float age=time-c.w-distance/12;
    if(age<=0||age>=Lifetime)continue;
    var force=impulses[i];float radius=Mathf.Max(.01f,force.w);
    float response=(1-Mathf.Exp(-age*35))*Mathf.Exp(-age*4.5f)*Mathf.Sin(age*12)*Mathf.Exp(-distance*distance/(radius*radius));
    offset+=new Vector3(force.x,force.y,force.z)*response;
   }
   return Vector3.ClampMagnitude(offset,MaximumDeflection)*PinWeight(p,Dimensions);
  }
  public void Receive(Vector3 point,Vector3 impulse,float radius,float started){
   if(!Finite(point)||!Finite(impulse)||!float.IsFinite(radius)||!float.IsFinite(started)||radius<.1f||radius>2||impulse.magnitude>1.5f)return;
   var bounds=mesh.bounds;bounds.Expand(.15f);if(!bounds.Contains(point))return;
   contacts[next]=new Vector4(point.x,point.y,point.z,started);impulses[next]=new Vector4(impulse.x,impulse.y,impulse.z,radius);next=(next+1)%Capacity;
   ImpactCount++;LastContact=point;LastImpulse=impulse;LastImpactTime=started;Upload();
  }
  public void Contact(Vector3 worldPoint,Vector3 incomingVelocity,Vector3 inwardNormal){
   if(!FootballBall.Instance||!FootballBall.Instance.HasAuthority||!Finite(incomingVelocity)||!Finite(worldPoint)||!Finite(inwardNormal)||inwardNormal.sqrMagnitude<.5f)return;
   float speed=Mathf.Max(0,-Vector3.Dot(incomingVelocity,inwardNormal.normalized));if(speed<.35f)return;
   var point=transform.InverseTransformPoint(worldPoint);var velocity=transform.InverseTransformDirection(incomingVelocity);
   // One PhysX impact can also reach the analytic containment fallback that frame.
   if(Time.time-LastImpactTime<.06f&&Vector3.Distance(point,LastContact)<.22f)return;
   float amplitude=Mathf.Clamp(speed*.055f,.06f,1.05f),radius=Mathf.Lerp(.55f,1.15f,Mathf.Clamp01(speed/22));
   var impulse=velocity.normalized*amplitude;
   Receive(point,impulse,radius,Time.time);FootballNetEvents.Send(this,point,impulse,radius);
  }
  static bool Finite(Vector3 v)=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z);
  void Upload(){properties.SetVectorArray(ContactsId,contacts);properties.SetVectorArray(ImpulsesId,impulses);surface.SetPropertyBlock(properties);}
  public void Clear(){for(int i=0;i<Capacity;i++){contacts[i]=new Vector4(0,0,0,-100);impulses[i]=Vector4.zero;}next=0;LastImpactTime=-10;if(properties!=null)Upload();}
  void OnEnable(){FootballNetEvents.Add(this);}
  void LateUpdate(){FootballNetEvents.EnsureConnected();}
  void OnDisable(){FootballNetEvents.Remove(this);Clear();}
  void OnDestroy(){if(authored)authored.enabled=authoredEnabled;if(mesh)Destroy(mesh);}
 }
}
