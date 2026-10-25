using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 // Local, render-only teaching shot. No GolfBall, collider, Rigidbody or RPC.
 public sealed class GolfEventShowcase:IDisposable {
  public const float Radius=GolfBall.Radius;
  Transform root,ball;TrailRenderer trail;LineRenderer ring;
  readonly LineRenderer[] accents=new LineRenderer[8];
  Vector3 ground,normal,along,wind;float scale;int contact;bool launched;GolfEventKind kind;
  static GolfBallPhysicsSettings turf;
  public const float WindLanding=2f;
  public bool Visible=>root&&root.gameObject.activeSelf;
  public Vector3 BallPosition=>ball?ball.position:Vector3.zero;
  public static bool Supports(GolfEventKind value)=>value is GolfEventKind.TropicalWind or GolfEventKind.BounceParty or GolfEventKind.StickyGreen;

  // Continuous, clock-sampled paths also let late/future packets share the cue age.
  public static Vector3 Sample(GolfEventKind value,float age){
   if(value==GolfEventKind.TropicalWind)return WindSample(age,out _,out _);
   if(value==GolfEventKind.StickyGreen){float u=Mathf.InverseLerp(.35f,1.35f,age);return new Vector3(-.65f*(1-u),Radius+8*(1-u*u),0);}
   if(age<1.15f){float u=Mathf.InverseLerp(.35f,1.15f,age);return new Vector3(-.7f*(1-u),Radius+8*(1-u*u),0);}
   if(age<2.25f){float u=Mathf.InverseLerp(1.15f,2.25f,age);return new Vector3(.7f*u,Radius+9.6f*u*(1-u),0);}
   float v=Mathf.InverseLerp(2.25f,2.95f,age);return new Vector3(.7f+.45f*v,Radius+2.8f*v*(1-v),0);
  }
  static Vector3 WindSample(float age,out int contacts,out float lastImpact){
   contacts=0;lastImpact=WindLanding;float gravity=Mathf.Abs(Physics.gravity.y),flight=WindLanding-.4f;
   float t=Mathf.Clamp(age-.4f,0,flight),u=t/flight;
   if(age<WindLanding)return new Vector3(-3+6*u,Radius+gravity*t*(flight-t)*.5f,6*u*u);
   if(!turf)turf=Resources.Load<GolfBallPhysicsSettings>("GolfBallPhysics");
   var point=new Vector3(3,Radius,6);var velocity=new Vector3(6/flight,-gravity*flight*.5f,12/flight);float remaining=age-WindLanding;
   // Use the actual turf response, including first-contact tangential loss.
   // No live ball or match state is changed by this render-only trajectory.
   for(int bounce=0;bounce<12;bounce++){
    contacts++;velocity=turf.LandingVelocity(velocity,Vector3.up,bounce==0);
    if(velocity.y<=0){float speed=velocity.magnitude,stop=speed/turf.fairwayResistance;float roll=Mathf.Min(remaining,stop);return point+velocity.normalized*(speed*roll-.5f*turf.fairwayResistance*roll*roll);}
    float duration=2*velocity.y/gravity;
    if(remaining<=duration)return point+velocity*remaining-Vector3.up*(gravity*remaining*remaining*.5f);
    point+=Vector3.ProjectOnPlane(velocity,Vector3.up)*duration;remaining-=duration;lastImpact+=duration;velocity.y=-velocity.y;
   }
   return point;
  }
  LineRenderer Line(string name,int points,bool loop=false){
   var go=new GameObject(name);go.layer=2;go.transform.SetParent(root,false);var line=go.AddComponent<LineRenderer>();
   line.sharedMaterial=Resources.Load<Material>("GolfBallTrail");line.useWorldSpace=true;line.positionCount=points;line.loop=loop;
   line.widthMultiplier=.045f;line.numCornerVertices=3;line.numCapVertices=3;line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;return line;
  }
  bool Create(GolfEventKind value,Athlete owner,GolfMatchManager match){
   var original=match.Ball(owner);if(!original)return false;
   kind=value;normal=Vector3.up;var facing=Vector3.ProjectOnPlane(owner.transform.forward,Vector3.up).normalized;
   wind=Vector3.ProjectOnPlane(match.Events.Snapshot.wind,Vector3.up).normalized;if(wind.sqrMagnitude<.5f)wind=owner.transform.right;
   along=value==GolfEventKind.TropicalWind?Vector3.Cross(wind,Vector3.up).normalized:facing;
   // Prefer a clear nearby patch; skip the view if terrain/obstacles leave no safe stage.
   bool found=false;
   for(int i=0;i<12;i++){
    var offset=Quaternion.Euler(0,(i%3-1)*55,0)*facing*(7+(i/3)*3);
    if(!Physics.Raycast(owner.transform.position+offset+Vector3.up*12,Vector3.down,out var hit,28,1<<8,QueryTriggerInteraction.Ignore)||hit.normal.y<.94f)continue;
    if(Physics.CheckBox(hit.point+Vector3.up*2.6f,new Vector3(4,2.1f,4),Quaternion.identity,1<<8,QueryTriggerInteraction.Ignore))continue;
    ground=hit.point;normal=hit.normal;found=true;break;
   }
   if(!found)return false;
   along=Vector3.ProjectOnPlane(along,normal).normalized;wind=value==GolfEventKind.TropicalWind?Vector3.ProjectOnPlane(wind,normal).normalized:Vector3.Cross(normal,along).normalized;
   root=new GameObject("Golf event demonstration (visual only)").transform;root.gameObject.layer=2;
   ball=new GameObject("Demonstration golf ball").transform;ball.SetParent(root,false);
   var lod=original.GetComponentInChildren<LODGroup>();var renderers=lod?lod.GetLODs()[0].renderers:original.GetComponentsInChildren<MeshRenderer>();
   Bounds bounds=default;bool first=true;
   foreach(var source in renderers){
    if(!source||!source.TryGetComponent<MeshFilter>(out var filter)||!filter.sharedMesh)continue;
    var mesh=new GameObject("Original golf ball mesh");mesh.layer=2;mesh.transform.SetParent(ball,false);
    var matrix=original.transform.worldToLocalMatrix*source.transform.localToWorldMatrix;
    mesh.transform.localPosition=matrix.GetColumn(3);mesh.transform.localRotation=matrix.rotation;mesh.transform.localScale=matrix.lossyScale;
    mesh.AddComponent<MeshFilter>().sharedMesh=filter.sharedMesh;mesh.AddComponent<MeshRenderer>().sharedMaterials=source.sharedMaterials;
    if(first){bounds=source.bounds;first=false;}else bounds.Encapsulate(source.bounds);
   }
   if(first){Dispose();return false;}
   scale=Radius*2/Mathf.Max(.001f,Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z)));ball.localScale=Vector3.one*scale;
   trail=ball.gameObject.AddComponent<TrailRenderer>();trail.sharedMaterial=Resources.Load<Material>("GolfBallTrail");trail.time=value==GolfEventKind.TropicalWind?1.1f:.72f;trail.minVertexDistance=.025f;trail.widthMultiplier=.065f;
   trail.widthCurve=AnimationCurve.Linear(0,1,1,0);trail.numCornerVertices=4;trail.shadowCastingMode=ShadowCastingMode.Off;trail.receiveShadows=false;trail.emitting=false;
   ring=Line("Event landing ripple",48,true);for(int i=0;i<accents.Length;i++)accents[i]=Line("Event motion accent",20);
   contact=0;launched=false;return true;
  }
  Vector3 World(Vector3 point)=>ground+along*point.x+normal*point.y+wind*point.z;
  public bool Present(GolfEventKind value,float age,Athlete owner,GolfMatchManager match){
   if(!Visible&&!Create(value,owner,match))return false;
   var p=Sample(value,age);ball.position=World(p);float spinAge=Mathf.Min(age,value==GolfEventKind.StickyGreen?1.35f:value==GolfEventKind.TropicalWind?3.5f:2.95f);ball.rotation=Quaternion.AngleAxis(spinAge*240,wind);
   if(value==GolfEventKind.TropicalWind&&age>=.4f&&!launched){launched=true;GolfFishingAudio.Current?.Play("golf-swish",ball.position,.7f,true);}
   float fade=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.95f,3.5f,age));
   var config=GolfPresentationSettings.Current;Color color=value==GolfEventKind.TropicalWind?config.windColor:value==GolfEventKind.BounceParty?config.bounceColor:config.stickyColor;
   trail.emitting=age>.4f&&age<2.95f&&(value!=GolfEventKind.StickyGreen||age<1.35f);trail.startColor=new Color(color.r,color.g,color.b,fade);trail.endColor=new Color(color.r,color.g,color.b,0);
   float impact=value==GolfEventKind.StickyGreen?1.35f:value==GolfEventKind.BounceParty?(age>=2.95f?2.95f:age>=2.25f?2.25f:1.15f):2.8f;
   float since=age-impact;int next=value==GolfEventKind.StickyGreen?(since>=0?1:0):value==GolfEventKind.BounceParty?(age>=2.95f?3:age>=2.25f?2:age>=1.15f?1:0):0;
   if(value==GolfEventKind.TropicalWind){WindSample(age,out next,out impact);since=age-impact;}
   if(next>contact){contact=next;GolfFishingAudio.Current?.Play("golf-turf",ball.position,value==GolfEventKind.StickyGreen?.55f:.8f,true);}
   ball.localScale=Vector3.one*scale;
   ring.enabled=since>=0;float radius=value==GolfEventKind.StickyGreen?Mathf.Lerp(.85f,.28f,Mathf.Clamp01(since/.4f)):.25f+Mathf.Clamp01(since/.6f)*1.1f;
   float alpha=fade*(value==GolfEventKind.StickyGreen?.85f:1-Mathf.Clamp01(since/.65f));ring.startColor=ring.endColor=new Color(color.r,color.g,color.b,alpha);
   var centre=World(Sample(value,impact));centre-=normal*Radius;centre+=normal*.035f;
   for(int i=0;i<48;i++){float a=i*Mathf.PI*2/48;ring.SetPosition(i,centre+(along*Mathf.Cos(a)+wind*Mathf.Sin(a))*radius);}
   for(int i=0;i<accents.Length;i++){
    var line=accents[i];line.enabled=value==GolfEventKind.TropicalWind||value==GolfEventKind.StickyGreen&&since>=0;
    line.startColor=new Color(color.r,color.g,color.b,fade*.5f);line.endColor=new Color(color.r,color.g,color.b,0);
    for(int j=0;j<20;j++){
     float u=j/19f;Vector3 point;
     if(value==GolfEventKind.TropicalWind){float phase=Mathf.Repeat(age*.65f+i*.137f,1);point=ground+along*((i%4-1.5f)*1.7f)+normal*(.6f+(i/4)*1.4f+Mathf.Sin(u*Mathf.PI)*.18f)+wind*((phase-.5f)*10-u*1.7f);}
     else {float a=i*Mathf.PI/4+u*.7f;point=centre+(along*Mathf.Cos(a)+wind*Mathf.Sin(a))*Mathf.Lerp(.65f,.19f,u)+normal*(Mathf.Sin(u*Mathf.PI)*.18f);}
     line.SetPosition(j,point);
    }
   }
   return true;
  }
  public void Camera(float age,out Vector3 focus,out Vector3 position){
   if(kind==GolfEventKind.TropicalWind){
    var centre=ground+normal*1.8f+wind*2.5f;focus=Vector3.Lerp(centre,ball.position,.7f);
    position=focus-along*1.7f-wind*3.8f+normal*1.05f;
   }else{
    focus=ball.position+normal*.08f;position=focus-along*1.35f-wind*2.5f+normal*.7f;
   }
  }
  public void Dispose(){if(root){root.gameObject.SetActive(false);UnityEngine.Object.Destroy(root.gameObject);}root=ball=null;}
 }
}
