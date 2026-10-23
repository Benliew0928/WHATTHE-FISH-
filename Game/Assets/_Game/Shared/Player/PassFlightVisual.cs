using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 // Lives on the ball rather than the local HUD: participants and spectators
 // render the same authoritative flight, including a guest-initiated pass.
 public sealed class PassFlightVisual:MonoBehaviour {
  const int Steps=40;
  BasketballBall basketball;FootballBall football;Mesh mesh;MeshRenderer lane;Material material;
  readonly Vector3[] vertices=new Vector3[(Steps+1)*2];readonly Vector2[] uv=new Vector2[(Steps+1)*2];
  public bool Visible=>lane&&lane.enabled;
  public Vector3 Endpoint {get;private set;}
  void Awake(){basketball=GetComponent<BasketballBall>();football=GetComponent<FootballBall>();}
  void Create(){
   var source=Resources.Load<Material>(basketball?"BasketballPassAim":"FootballKickAim");if(!source)return;
   material=new Material(source){name="Shared pass flight"};mesh=new Mesh{name="Shared pass flight ribbon"};mesh.MarkDynamic();
   var indices=new int[Steps*6];for(int i=0;i<Steps;i++){int v=i*2,t=i*6;indices[t]=v;indices[t+1]=v+2;indices[t+2]=v+1;indices[t+3]=v+1;indices[t+4]=v+2;indices[t+5]=v+3;}
   mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=indices;
   var go=new GameObject("Shared pass flight");go.layer=2;go.AddComponent<MeshFilter>().sharedMesh=mesh;lane=go.AddComponent<MeshRenderer>();lane.sharedMaterial=material;lane.shadowCastingMode=ShadowCastingMode.Off;lane.receiveShadows=false;
  }
  void LateUpdate(){
   var flight=basketball?basketball.PassFlight:football?football.PassFlight:default;
   bool playing=basketball?basketball.Playing:FootballBall.Allowed;
   float remaining=flight.remaining-Mathf.Max(0,(float)(BasketballMotion.Clock-flight.sampled));
   if(!flight.active||!playing||remaining<=0||FootballReplay.Presenting){if(lane)lane.enabled=false;return;}
   if(!lane)Create();if(!lane)return;
   var start=transform.position;Endpoint=flight.destination;var camera=Camera.main;float length=Mathf.Max(.1f,Vector3.Distance(start,Endpoint));
   for(int i=0;i<=Steps;i++){
    float t=i/(float)Steps;var point=Vector3.Lerp(start,Endpoint,t);
    if(basketball)point.y+=-Physics.gravity.y*remaining*remaining*.5f*t*(1-t);else point.y=start.y+.035f;
    var tangent=Endpoint-start;if(basketball)tangent.y+=-Physics.gravity.y*remaining*remaining*.5f*(1-2*t);
    var side=basketball&&camera?Vector3.Cross(tangent,camera.transform.position-point).normalized:Vector3.Cross(Vector3.up,tangent).normalized;
    vertices[i*2]=point-side*.28f;vertices[i*2+1]=point+side*.28f;uv[i*2]=new Vector2(-.7f,t);uv[i*2+1]=new Vector2(.7f,t);
   }
   mesh.vertices=vertices;mesh.uv=uv;mesh.RecalculateBounds();material.SetFloat("_Length",length);material.SetFloat("_Opacity",.7f);
   if(basketball){material.SetFloat("_Charge",1);material.SetFloat("_Bend",0);material.SetFloat("_Release",0);}else material.SetFloat("_Lock",1);lane.enabled=true;
  }
  void OnDisable(){if(lane)lane.enabled=false;}
  void OnDestroy(){if(lane)Destroy(lane.gameObject);if(mesh)Destroy(mesh);if(material)Destroy(material);}
 }
}
