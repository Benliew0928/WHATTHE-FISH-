using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 // The same light energy fabric as football. Presentation only: the player
 // motor owns containment, and a flying ball passes through without collision.
 [DefaultExecutionOrder(60)]
 public sealed class BasketballBoundaryVisual:MonoBehaviour {
  public const float Height=1.8f;
  Mesh mesh;MeshRenderer wall;MaterialPropertyBlock properties;BasketballBall ball;
  readonly Dictionary<Athlete,float> contactTimes=new();
  readonly Vector4[] impacts=new Vector4[4];
  static readonly int[] ImpactIds={Shader.PropertyToID("_Impact"),Shader.PropertyToID("_Impact1"),Shader.PropertyToID("_Impact2"),Shader.PropertyToID("_Impact3")};
  Vector3 previousBall;uint previousReset;bool sampled;int nextImpact;float visibility;
  public uint BallPulses {get;private set;}public uint PlayerPulses {get;private set;}
  public float LastImpactTime {get;private set;}=-10;
  public static void Attach(BasketballBall ball){
   var parent=ball.transform.parent;if(!parent||parent.Find("Basketball energy boundary"))return;
   var material=Resources.Load<Material>("FootballBoundary");if(!material){Debug.LogError("Missing court boundary material.",ball);return;}
   var go=new GameObject("Basketball energy boundary");go.layer=2;go.transform.SetParent(parent,false);
   go.AddComponent<BasketballBoundaryVisual>().Build(ball,material);
  }
  void Build(BasketballBall owner,Material material){
   ball=owner;properties=new MaterialPropertyBlock();var limits=BasketballBall.PlayerCourtLimits;
   var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
   void Panel(Vector3 a,Vector3 b){int start=vertices.Count;float length=Vector3.Distance(a,b);a.y=b.y=.018f;vertices.Add(a);vertices.Add(b);vertices.Add(b+Vector3.up*Height);vertices.Add(a+Vector3.up*Height);uv.Add(Vector2.zero);uv.Add(new Vector2(length,0));uv.Add(new Vector2(length,Height));uv.Add(new Vector2(0,Height));triangles.AddRange(new[]{start,start+2,start+1,start,start+3,start+2});}
   Panel(new Vector3(-limits.x,0,-limits.y),new Vector3(-limits.x,0,limits.y));Panel(new Vector3(limits.x,0,limits.y),new Vector3(limits.x,0,-limits.y));
   Panel(new Vector3(-limits.x,0,limits.y),new Vector3(limits.x,0,limits.y));Panel(new Vector3(limits.x,0,-limits.y),new Vector3(-limits.x,0,-limits.y));
   mesh=new Mesh{name="Basketball energy perimeter (16 vertices)"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
   wall=gameObject.AddComponent<MeshRenderer>();wall.sharedMaterial=material;wall.shadowCastingMode=ShadowCastingMode.Off;wall.receiveShadows=false;wall.lightProbeUsage=LightProbeUsage.Off;wall.reflectionProbeUsage=ReflectionProbeUsage.Off;
   properties.SetColor("_BaseColor",new Color(.07f,.8f,.73f,.28f));properties.SetColor("_AccentColor",new Color(1,.65f,.18f));properties.SetColor("_ContactColor",new Color(1,.86f,.48f));
   ResetContacts();
  }
  void ResetContacts(){sampled=false;contactTimes.Clear();for(int i=0;i<4;i++)impacts[i]=new Vector4(0,0,0,-10);}
  public static bool Crossed(Vector3 from,Vector3 to,out Vector3 hit){
   var bounds=BasketballBall.PlayerCourtLimits;float first=2;hit=default;
   void Plane(float a,float b,float limit,bool x){if(Mathf.Abs(b-a)<.00001f)return;float t=(limit-a)/(b-a);if(t<0||t>1||t>=first)return;var p=Vector3.LerpUnclamped(from,to,t);float cross=x?p.z:p.x;float edge=x?bounds.y:bounds.x;if(Mathf.Abs(cross)>edge+BasketballBall.Radius||p.y<-.15f||p.y>Height+BasketballBall.Radius)return;first=t;}
   Plane(from.x,to.x,-bounds.x,true);Plane(from.x,to.x,bounds.x,true);Plane(from.z,to.z,-bounds.y,false);Plane(from.z,to.z,bounds.y,false);
   if(first>1)return false;hit=Vector3.LerpUnclamped(from,to,first);return true;
  }
  void Pulse(Vector3 local,bool player){var world=transform.TransformPoint(local);LastImpactTime=Time.time;impacts[nextImpact]=new Vector4(world.x,world.y,world.z,Time.time);nextImpact=(nextImpact+1)%4;if(player)PlayerPulses++;else BallPulses++;}
  void LateUpdate(){
   if(!ball||!wall)return;bool playing=ball.Playing;visibility=Mathf.MoveTowards(visibility,playing?1:0,Time.deltaTime*4);wall.enabled=visibility>.001f;
   if(!playing){if(sampled)ResetContacts();properties.SetFloat("_Visibility",visibility);wall.SetPropertyBlock(properties);return;}
   var current=transform.InverseTransformPoint(ball.transform.position);uint sequence=ball.PresentationReset;
   if(!ball.Held&&sampled&&sequence==previousReset&&Crossed(previousBall,current,out var hit))Pulse(hit,false);
   previousBall=current;previousReset=sequence;sampled=true;
   var limits=BasketballBall.PlayerCourtLimits;
   foreach(var actor in Athlete.Active){
    if(!actor||!actor.isActiveAndEnabled||actor.inTransit||actor.BasketballFreeRoam||!actor.capsule)continue;
    var p=transform.InverseTransformPoint(actor.transform.position);if(Mathf.Abs(p.x)>limits.x+.05f||Mathf.Abs(p.z)>limits.y+.05f||p.y>Height||p.y<-.3f)continue;
    float x=limits.x-Mathf.Abs(p.x),z=limits.y-Mathf.Abs(p.z),reach=actor.capsule.radius+actor.capsule.skinWidth+.065f;
    bool touching=Mathf.Min(x,z)<=reach;
    if(!touching){contactTimes.Remove(actor);continue;}
    if(contactTimes.TryGetValue(actor,out float last)&&Time.time-last<.85f)continue;
    if(x<z)p.x=Mathf.Sign(p.x)*limits.x;else p.z=Mathf.Sign(p.z)*limits.y;p.y+=.85f;
    contactTimes[actor]=Time.time;Pulse(p,true);
   }
   var localPlayer=AppRoot.Instance?AppRoot.Instance.LocalAthlete:null;var pos=localPlayer?localPlayer.transform.position:Vector3.zero;
   properties.SetVector("_PlayerPosition",new Vector4(pos.x,pos.y,pos.z,localPlayer&&!localPlayer.BasketballFreeRoam?1:0));pos=ball.transform.position;
   properties.SetVector("_BallPosition",new Vector4(pos.x,pos.y,pos.z,1));properties.SetFloat("_Visibility",visibility);
   for(int i=0;i<4;i++)properties.SetVector(ImpactIds[i],impacts[i]);wall.SetPropertyBlock(properties);
  }
  void OnDisable(){ResetContacts();visibility=0;if(wall)wall.enabled=false;}
  void OnDestroy(){if(mesh)Destroy(mesh);}
 }
}
