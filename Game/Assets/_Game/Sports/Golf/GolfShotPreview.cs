using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 public sealed class GolfShotPreview:MonoBehaviour {
  readonly GolfShotPlan plan=new();
  readonly List<Vector3> vertices=new(2200);readonly List<Vector2> uvs=new(2200),styles=new(2200);readonly List<int> indices=new(6600);
  Mesh mesh;MeshRenderer lane;Material material;
  Vector3 previousOrigin;float previousHeading,previousCharge;GolfShotMode previousMode;bool sampled;
  public bool Visible=>lane&&lane.enabled;
  public Vector3 EndPoint=>plan.EndPoint;
  public Vector3 LaunchVelocity=>plan.LaunchVelocity;
  public int PointCount=>plan.Count;
  public GolfShotMode Mode=>plan.Mode;
  public bool HasTarget=>plan.HasTarget;
  public void Hide(){if(lane)lane.enabled=false;sampled=false;}
  void Create(){
   var source=Resources.Load<Material>("GolfShotGuide");if(!source)return;
   material=new Material(source);mesh=new Mesh{name="Golf trajectory and landing glow"};mesh.MarkDynamic();
   gameObject.layer=2;gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;lane=gameObject.AddComponent<MeshRenderer>();lane.sharedMaterial=material;lane.shadowCastingMode=ShadowCastingMode.Off;lane.receiveShadows=false;
  }
  public void Show(GolfBall ball,GolfCourse course,Vector3 origin,float heading,float charge,GolfShotMode mode,Camera camera){
   if(!lane)Create();if(!lane)return;
   if(!sampled||origin!=previousOrigin||heading!=previousHeading||charge!=previousCharge||mode!=previousMode){
    plan.Build(origin,Quaternion.Euler(0,heading,0)*Vector3.forward,charge,mode,course,ball.PhysicsSettings);
    previousOrigin=origin;previousHeading=heading;previousCharge=charge;previousMode=mode;sampled=true;
   }
   Render(camera);lane.enabled=true;
  }
  void Vertex(Vector3 p,Vector2 uv,Vector2 style){vertices.Add(p);uvs.Add(uv);styles.Add(style);}
  void Quad(int a,int b,int c,int d){indices.Add(a);indices.Add(c);indices.Add(b);indices.Add(b);indices.Add(c);indices.Add(d);}
  void Render(Camera camera){
   vertices.Clear();uvs.Clear();styles.Clear();indices.Clear();float distance=0;
   for(int i=0;i<plan.Count;i++){
    var p=plan.Points[i]+Vector3.up*.035f;var before=plan.Points[Mathf.Max(0,i-1)];var after=plan.Points[Mathf.Min(plan.Count-1,i+1)];
    var side=Vector3.Cross(after-before,camera.transform.position-p).normalized;
    float width=.13f+Mathf.Min(.13f,Vector3.Distance(camera.transform.position,p)*.0025f);
    if(i>0)distance+=Vector3.Distance(plan.Points[i-1],plan.Points[i]);
    var style=new Vector2(0,plan.Mode==GolfShotMode.Putt?1:0);
    Vertex(p-side*width,new Vector2(-1,distance),style);Vertex(p+side*width,new Vector2(1,distance),style);
    if(i>0){int n=vertices.Count;Quad(n-4,n-3,n-2,n-1);}
   }
   if(plan.HasTarget){
    var normal=plan.Normal;var point=EndPoint-normal*(GolfBall.Radius-.025f);
    var right=Vector3.Cross(normal,Mathf.Abs(normal.z)>.9f?Vector3.right:Vector3.forward).normalized;var forward=Vector3.Cross(right,normal);
    float radius=.85f+Mathf.Min(.5f,Vector3.Distance(camera.transform.position,point)*.01f);int n=vertices.Count;var style=new Vector2(1,0);
    Vertex(point+(-right-forward)*radius,new Vector2(-1,-1),style);Vertex(point+(right-forward)*radius,new Vector2(1,-1),style);
    Vertex(point+(-right+forward)*radius,new Vector2(-1,1),style);Vertex(point+(right+forward)*radius,new Vector2(1,1),style);Quad(n,n+1,n+2,n+3);
   }
   mesh.Clear();mesh.SetVertices(vertices);mesh.SetUVs(0,uvs);mesh.SetUVs(1,styles);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();
  }
  void OnDestroy(){if(mesh)Destroy(mesh);if(material)Destroy(material);}
 }
}
