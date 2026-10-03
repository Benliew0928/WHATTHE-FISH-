using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 // Cosmetic only: use the ball's authored bounds, never add player/ball collision.
 public sealed class FootballBoundaryVisual:MonoBehaviour {
  public const float Height=1.8f;
  Mesh mesh;FootballBall ball;MeshRenderer wall;MaterialPropertyBlock properties;
  Vector3[] endpoints;float previousDistance=float.PositiveInfinity,lastImpact=-10;
  public float LastImpactTime=>lastImpact;
  static readonly int PlayerPosition=Shader.PropertyToID("_PlayerPosition"),BallPosition=Shader.PropertyToID("_BallPosition"),Impact=Shader.PropertyToID("_Impact");
  public static void Attach(FootballBall ball){
   if(!ball.Pitch||ball.Pitch.Find("Football energy boundary"))return;
   var material=Resources.Load<Material>("FootballBoundary");
   if(!material){Debug.LogError("Missing football boundary material.",ball);return;}
   var go=new GameObject("Football energy boundary");go.layer=2;go.transform.SetParent(ball.Pitch,false);
   var visual=go.AddComponent<FootballBoundaryVisual>();visual.Build(ball,material);
  }
  void Build(FootballBall ball,Material material){
   this.ball=ball;properties=new MaterialPropertyBlock();
   var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
   var bounds=ball.PitchBounds;float floor=bounds.max.y+.012f;
   float height=Height/Mathf.Abs(ball.Pitch.lossyScale.y);
   void Panel(Vector3 a,Vector3 b){
    a.y=b.y=floor;int start=vertices.Count;
    float length=Vector3.Distance(ball.Pitch.TransformPoint(a),ball.Pitch.TransformPoint(b));
    vertices.Add(a);vertices.Add(b);vertices.Add(b+Vector3.up*height);vertices.Add(a+Vector3.up*height);
    uv.Add(Vector2.zero);uv.Add(new Vector2(length,0));uv.Add(new Vector2(length,Height));uv.Add(new Vector2(0,Height));
    triangles.Add(start);triangles.Add(start+2);triangles.Add(start+1);
    triangles.Add(start);triangles.Add(start+3);triangles.Add(start+2);
   }
   Panel(new Vector3(bounds.min.x,0,bounds.min.z),new Vector3(bounds.min.x,0,bounds.max.z));
   Panel(new Vector3(bounds.max.x,0,bounds.max.z),new Vector3(bounds.max.x,0,bounds.min.z));
   foreach(int sign in new[]{-1,1}){
    float z=sign<0?bounds.min.z:bounds.max.z;int goal=-1;
    for(int i=0;i<ball.GoalCount;i++)if(ball.GoalSign(i)==sign){goal=i;break;}
    if(goal<0)Panel(new Vector3(bounds.min.x,0,z),new Vector3(bounds.max.x,0,z));
    else {
     var opening=ball.GoalBounds(goal);
     Panel(new Vector3(bounds.min.x,0,z),new Vector3(opening.min.x,0,z));
     Panel(new Vector3(opening.max.x,0,z),new Vector3(bounds.max.x,0,z));
    }
   }
   mesh=new Mesh{name="Football boundary with open goal mouths"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();
   endpoints=vertices.ToArray();
   gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
   var renderer=gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;
   wall=renderer;properties.SetVector(Impact,new Vector4(0,0,0,-10));wall.SetPropertyBlock(properties);
   renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
   renderer.lightProbeUsage=LightProbeUsage.Off;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;
  }
  void LateUpdate(){
   if(!ball||!wall)return;
   var position=ball.transform.position;var local=transform.InverseTransformPoint(position);local.y=mesh.bounds.min.y;
   float distance=float.PositiveInfinity;Vector3 closest=default;
   for(int i=0;i<endpoints.Length;i+=4){
    var a=endpoints[i];var d=endpoints[i+1]-a;
    var point=transform.TransformPoint(a+d*Mathf.Clamp01(Vector3.Dot(local-a,d)/d.sqrMagnitude));
    float gap=Vector3.ProjectOnPlane(point-position,Vector3.up).magnitude;
    if(gap<distance){distance=gap;closest=point;}
   }
   // Evaluate the presented ball on every peer; no new network traffic or physics.
   float threshold=ball.WorldRadius+.1f;
   if(!ball.CurrentController&&distance<=threshold&&previousDistance>threshold&&Time.time-lastImpact>.25f){
    lastImpact=Time.time;closest.y=Mathf.Max(closest.y,position.y);
    properties.SetVector(Impact,new Vector4(closest.x,closest.y,closest.z,lastImpact));
   }
   previousDistance=distance;
   var actor=AppRoot.Instance?AppRoot.Instance.LocalAthlete:null;
   properties.SetVector(PlayerPosition,actor?new Vector4(actor.transform.position.x,actor.transform.position.y,actor.transform.position.z,1):Vector4.zero);
   properties.SetVector(BallPosition,new Vector4(position.x,position.y,position.z,1));
   wall.SetPropertyBlock(properties);
  }
  void OnDisable(){previousDistance=float.PositiveInfinity;lastImpact=-10;if(properties!=null){properties.SetVector(Impact,new Vector4(0,0,0,-10));if(wall)wall.SetPropertyBlock(properties);}}
  void OnDestroy(){if(mesh)Destroy(mesh);}
 }
}
