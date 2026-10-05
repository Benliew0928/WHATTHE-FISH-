using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 public enum BasketballNetKind:byte { None,Touch,Rim,Score }
 public struct BasketballNetHit:INetworkSerializable,IEquatable<BasketballNetHit> {
  public uint sequence;public double time;public Vector3 point,velocity;public BasketballNetKind kind;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref sequence);s.SerializeValue(ref time);s.SerializeValue(ref point);s.SerializeValue(ref velocity);s.SerializeValue(ref kind);}
  public bool Equals(BasketballNetHit b)=>sequence==b.sequence&&time==b.time&&point==b.point&&velocity==b.velocity&&kind==b.kind;
 }

 // A pinned diamond cord mesh, shared with the authored hoop material. Local
 // damped impulses make both nets responsive without shipping a cloth package.
 public sealed class BasketballHoop:MonoBehaviour {
  public const float RimHeight=3.048f,InnerRadius=.2286f,NetTop=3.024f,NetBottom=2.564f;
  const int Around=16,Rows=5,Sides=4,Edges=144;
  readonly Vector3[] rest=new Vector3[Around*Rows],nodes=new Vector3[Around*Rows];
  readonly Vector3[] vertices=new Vector3[Edges*Sides*2],normals=new Vector3[Edges*Sides*2];
  readonly int[] edgeA=new int[Edges],edgeB=new int[Edges];
  Mesh mesh;MeshRenderer authored;bool entered,dirty;double enteredAt,nextTouch;
  public BasketballNetHit Hit {get;private set;}
  public float Displacement {get;private set;}
  public float AnchorDisplacement {get;private set;}
  void Awake(){
   foreach(var r in GetComponentsInChildren<MeshRenderer>(true))if(r.name=="Hoop__Net"){authored=r;break;}
   if(!authored)return;
   var go=new GameObject("Responsive net");go.transform.SetParent(transform,false);
   var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=authored.sharedMaterial;renderer.shadowCastingMode=ShadowCastingMode.Off;authored.enabled=false;
   mesh=new Mesh{name="Basketball diamond cords"};mesh.MarkDynamic();go.AddComponent<MeshFilter>().sharedMesh=mesh;
   for(int row=0;row<Rows;row++)for(int col=0;col<Around;col++){
    float a=col*2*Mathf.PI/Around+(row%2)*Mathf.PI/Around;
    rest[row*Around+col]=new Vector3(Mathf.Cos(a)*(.23f-row*.023f),NetTop-row*.115f,Mathf.Sin(a)*(.23f-row*.023f));
   }
   int edge=0;
   for(int row=0;row<Rows-1;row++)for(int col=0;col<Around;col++)for(int side=0;side<2;side++){
    edgeA[edge]=row*Around+col;edgeB[edge++]=(row+1)*Around+(col+(row%2==0?(side==0?0:15):side))%Around;
   }
   for(int col=0;col<Around;col++){edgeA[edge]=4*Around+col;edgeB[edge++]=4*Around+(col+1)%Around;}
   var triangles=new int[Edges*Sides*6];var uv=new Vector2[vertices.Length];
   for(int e=0;e<Edges;e++)for(int side=0;side<Sides;side++){
    int a=e*Sides*2+side,b=e*Sides*2+(side+1)%Sides,c=a+Sides,d=b+Sides,k=(e*Sides+side)*6;
    triangles[k]=a;triangles[k+1]=b;triangles[k+2]=c;triangles[k+3]=b;triangles[k+4]=d;triangles[k+5]=c;
    uv[a]=new Vector2(side/(float)Sides,0);uv[c]=new Vector2(side/(float)Sides,1);
   }
   Array.Copy(rest,nodes,rest.Length);WriteMesh();mesh.triangles=triangles;mesh.uv=uv;
  }
  public void ResetCrossing(){entered=false;}
  public void ResetNet(){entered=false;nextTouch=0;Hit=new BasketballNetHit{sequence=Hit.sequence+1};dirty=true;}
  public void Receive(BasketballNetHit hit){if(hit.sequence==Hit.sequence)return;Hit=hit;dirty=true;}
  void Impulse(BasketballNetKind kind,Vector3 point,Vector3 velocity){
   double now=BasketballMotion.Clock;
   if(kind!=BasketballNetKind.Score&&now<nextTouch)return;
   nextTouch=now+(kind==BasketballNetKind.Score?.35:.10);
   Hit=new BasketballNetHit{sequence=Hit.sequence+1,time=now,kind=kind,point=point,velocity=Vector3.ClampMagnitude(velocity,18)};dirty=true;
  }
  public void RimContact(Vector3 worldPoint,Vector3 velocity){
   Vector3 p=transform.InverseTransformPoint(worldPoint);
   if(Mathf.Abs(p.y-RimHeight)<.13f)Impulse(BasketballNetKind.Rim,p,transform.InverseTransformDirection(velocity));
  }
  // Swept centre-plane entry followed by full-ball clearance. Upward crossings,
  // side entry and a ball resting/jittering on the rim cannot award a basket.
  public bool Crossed(Vector3 previous,Vector3 current,float radius,bool canScore){
   if(!canScore){entered=false;return false;}
   float fall=previous.y-current.y;
   if(current.y>RimHeight+radius||BasketballMotion.Clock-enteredAt>1.2)entered=false;
   if(fall>0&&previous.y>RimHeight&&current.y<=RimHeight){
    var p=Vector3.Lerp(previous,current,(previous.y-RimHeight)/fall);
    entered=new Vector2(p.x,p.z).magnitude<=InnerRadius-radius+.003f;enteredAt=BasketballMotion.Clock;
   }
   float exit=RimHeight-radius;
   if(entered&&fall>0&&previous.y>exit&&current.y<=exit){
    var p=Vector3.Lerp(previous,current,(previous.y-exit)/fall);entered=false;
    return new Vector2(p.x,p.z).magnitude<=InnerRadius;
   }
   if(new Vector2(current.x,current.z).magnitude>InnerRadius+radius)entered=false;
   return false;
  }
  public bool StepBall(Vector3 previousWorld,Vector3 currentWorld,Rigidbody body,float radius,bool canScore){
   var previous=transform.InverseTransformPoint(previousWorld);var current=transform.InverseTransformPoint(currentWorld);
   bool scored=Crossed(previous,current,radius,canScore);
   if(scored){
    Impulse(BasketballNetKind.Score,current,transform.InverseTransformDirection(body.linearVelocity));
    // Net resistance acts only after the ball has cleared the rim. It never
    // steers a miss into a make, and the ball remains a free physical rebound.
    var velocity=body.linearVelocity;velocity.x*=.88f;velocity.z*=.88f;velocity.y*=.80f;body.linearVelocity=velocity;
   }else{
    for(int i=0;i<=4;i++){
     var p=Vector3.Lerp(previous,current,i*.25f);
     if(p.y<NetBottom-radius||p.y>NetTop)continue;
     float t=Mathf.InverseLerp(NetTop,NetBottom,p.y),netRadius=Mathf.Lerp(.23f,.138f,t);
     if(Mathf.Abs(new Vector2(p.x,p.z).magnitude-netRadius)>radius+.012f)continue;
     Impulse(BasketballNetKind.Touch,p,transform.InverseTransformDirection(body.linearVelocity));
     if(current.y<RimHeight-radius)body.linearVelocity*=Mathf.Exp(-1.2f*Time.fixedDeltaTime);
     break;
    }
   }
   return scored;
  }
  void LateUpdate(){
   if(!mesh)return;
   float age=(float)(BasketballMotion.Clock-Hit.time);bool active=Hit.kind!=BasketballNetKind.None&&age>=0&&age<1.8f;
   if(!active&&!dirty)return;
   float pulse=active?Mathf.Exp(-age*4.8f)*Mathf.Sin(age*15):0;
   float strength=Hit.kind==BasketballNetKind.Score?1:Hit.kind==BasketballNetKind.Rim?.14f:Mathf.Clamp(Hit.velocity.magnitude/10,.18f,.60f);
   var sway=new Vector3(Hit.velocity.x,0,Hit.velocity.z)*.018f;sway=Vector3.ClampMagnitude(sway,.13f);
   Vector3 contactDirection=new Vector3(Hit.point.x,0,Hit.point.z).normalized;
   Displacement=AnchorDisplacement=0;
   for(int i=0;i<nodes.Length;i++){
    float depth=(i/Around)/(float)(Rows-1);Vector3 radial=new Vector3(rest[i].x,0,rest[i].z).normalized;
    float local=Hit.kind==BasketballNetKind.Touch?Mathf.Pow(Mathf.Max(0,Vector3.Dot(radial,contactDirection)),3):1;
    Vector3 offset=depth*strength*pulse*(radial*(.09f*local)+Vector3.down*.19f+sway);
    nodes[i]=rest[i]+offset;Displacement=Mathf.Max(Displacement,offset.magnitude);if(i<Around)AnchorDisplacement=Mathf.Max(AnchorDisplacement,offset.magnitude);
   }
   WriteMesh();dirty=active;
  }
  void WriteMesh(){
   for(int e=0;e<Edges;e++){
    Vector3 a=nodes[edgeA[e]],b=nodes[edgeB[e]],axis=(b-a).normalized;
    var tangent=Vector3.Cross(axis,Vector3.forward).normalized;if(tangent.sqrMagnitude<.1f)tangent=Vector3.Cross(axis,Vector3.right).normalized;
    var bitangent=Vector3.Cross(axis,tangent);
    for(int side=0;side<Sides;side++){
     float angle=side*Mathf.PI*2/Sides;Vector3 normal=tangent*Mathf.Cos(angle)+bitangent*Mathf.Sin(angle);int v=e*Sides*2+side;
     vertices[v]=a+normal*.0055f;vertices[v+Sides]=b+normal*.0055f;normals[v]=normals[v+Sides]=normal;
    }
   }
   mesh.vertices=vertices;mesh.normals=normals;mesh.bounds=new Bounds(new Vector3(0,2.8f,0),new Vector3(1.2f,1.3f,1.2f));
  }
  void OnDestroy(){if(mesh)Destroy(mesh);if(authored)authored.enabled=true;}
 }
}
