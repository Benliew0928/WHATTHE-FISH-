using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 // Local, render-only teaching shot. No GolfBall, collider, Rigidbody or RPC.
 public sealed class GolfEventShowcase:IDisposable {
  public const float Radius=GolfBall.Radius;
  Transform root,ball;GolfBallTrail trail;LineRenderer ring,aura,target;
  readonly LineRenderer[] reference=new LineRenderer[16];

  readonly LineRenderer[] accents=new LineRenderer[3];
  Vector3 ground,normal,along,wind;float scale;int contact;bool launched;GolfEventKind kind;
  static GolfBallPhysicsSettings turf;
  Color outlineColor;float outlineClock;
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
   contacts=0;lastImpact=WindLanding;float gravity=Mathf.Abs(Physics.gravity.y),flight=WindLanding-.5f;
   float t=Mathf.Clamp(age-.5f,0,flight),u=t/flight;
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
   // Measure the copied local geometry, not a rotated source world AABB.
   var localBounds=new Bounds();bool measured=false;
   foreach(var filter in ball.GetComponentsInChildren<MeshFilter>()){
    var matrix=ball.worldToLocalMatrix*filter.transform.localToWorldMatrix;var box=filter.sharedMesh.bounds;
    for(int i=0;i<8;i++){var corner=box.center+Vector3.Scale(box.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));var point=matrix.MultiplyPoint3x4(corner);if(!measured){localBounds=new Bounds(point,Vector3.zero);measured=true;}else localBounds.Encapsulate(point);}
   }
   scale=Radius*2/Mathf.Max(.001f,Mathf.Max(localBounds.size.x,Mathf.Max(localBounds.size.y,localBounds.size.z)));ball.localScale=Vector3.one*scale;
   trail=ball.gameObject.AddComponent<GolfBallTrail>();trail.BeginDemonstration(value);
   ring=Line("Event landing ripple",40,true);aura=Line("Golf event ball outline",32,true);
   for(int i=0;i<accents.Length;i++)accents[i]=Line("Event motion accent",3);
   if(value==GolfEventKind.TropicalWind){
    target=Line("Normal flight target",40,true);
    for(int i=0;i<reference.Length;i++)reference[i]=Line("Normal flight reference dash",2);
   }
   RenderPipelineManager.beginCameraRendering+=FaceOutline;contact=0;launched=false;return true;
  }
  Vector3 World(Vector3 point)=>ground+along*point.x+normal*point.y+wind*point.z;
  public bool Present(GolfEventKind value,float age,Athlete owner,GolfMatchManager match){
   if(!Visible&&!Create(value,owner,match))return false;
   var p=Sample(value,age);ball.position=World(p);float spinAge=Mathf.Min(age,value==GolfEventKind.StickyGreen?1.35f:value==GolfEventKind.TropicalWind?3.5f:2.95f);ball.rotation=Quaternion.AngleAxis(spinAge*240,wind);
   if(value==GolfEventKind.TropicalWind&&age>=.5f&&!launched){launched=true;GolfFishingAudio.Current?.Play("golf-swish",ball.position,.7f,true);}
   float fade=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.95f,3.5f,age));
   var config=GolfPresentationSettings.Current;Color color=value==GolfEventKind.TropicalWind?config.windColor:value==GolfEventKind.BounceParty?config.bounceColor:config.stickyColor;
   trail.PresentDemonstration(age>.5f&&age<2.95f&&(value!=GolfEventKind.StickyGreen||age<1.35f),fade);
   float impact=value==GolfEventKind.StickyGreen?1.35f:value==GolfEventKind.BounceParty?(age>=2.95f?2.95f:age>=2.25f?2.25f:1.15f):2.8f;
   float since=age-impact;int next=value==GolfEventKind.StickyGreen?(since>=0?1:0):value==GolfEventKind.BounceParty?(age>=2.95f?3:age>=2.25f?2:age>=1.15f?1:0):0;
   if(value==GolfEventKind.TropicalWind){WindSample(age,out next,out impact);since=age-impact;}
   if(next>contact){contact=next;GolfFeedback.Instance?.DemonstrationLanding(ball.position-normal*Radius,normal,value==GolfEventKind.StickyGreen?.55f:.8f);}
   float squash=value==GolfEventKind.StickyGreen?GolfBallEventFX.Squash(since):0;
   ball.localScale=Vector3.Scale(Vector3.one*scale,new Vector3(1+squash*.5f,1-squash,1+squash*.5f));
   var centre=World(Sample(value,impact))-normal*Radius+normal*.012f;
   if(value==GolfEventKind.TropicalWind)ring.enabled=false;
   else GolfBallEventFX.DrawContact(ring,centre,normal,color,since,fade);
   aura.enabled=value!=GolfEventKind.TropicalWind;var auraColor=color;auraColor.a*=fade;
   outlineColor=auraColor;outlineClock=age;
   for(int i=0;i<accents.Length;i++){
    var line=accents[i];line.enabled=value==GolfEventKind.TropicalWind;
    if(!line.enabled)continue;
    float phase=Mathf.Repeat(age*.9f+i*.37f,1);
    var point=ground+along*(-2+i*2.5f)+normal*(.5f+i*.6f)+wind*(-1+phase*11);
    GolfWindWorld.DrawStreak(line,point,wind,i,Mathf.Sin(phase*Mathf.PI)*fade*Mathf.SmoothStep(0,1,(age-.5f)/.5f));
   }
   if(value==GolfEventKind.TropicalWind){
    for(int i=0;i<reference.Length;i++){
     var line=reference[i];line.widthMultiplier=.025f;line.startColor=line.endColor=new Color(.88f,.93f,1,.38f*fade);
     for(int j=0;j<2;j++){float u=(i+j*.55f)/reference.Length;float t=u*(WindLanding-.5f);line.SetPosition(j,World(new Vector3(-3+6*u,Radius+Mathf.Abs(Physics.gravity.y)*t*(WindLanding-.5f-t)*.5f,0)));}
    }
    target.widthMultiplier=.035f;target.startColor=target.endColor=new Color(.88f,.93f,1,.55f*fade);
    for(int i=0;i<40;i++){float a=i*Mathf.PI/20;target.SetPosition(i,World(new Vector3(3+Mathf.Cos(a)*.35f,.025f,Mathf.Sin(a)*.35f)));}
    GolfFishingAudio.Current?.Loop("wind-demo","air",age>.5f,ball.position,fade*Mathf.Lerp(.15f,.65f,Mathf.Clamp01((age-.5f)/1.2f)),1,true);
   }
   return true;
  }
  public void Camera(float age,out Vector3 focus,out Vector3 position){
   if(kind==GolfEventKind.TropicalWind){
    focus=ground+along*1.5f+normal*1.5f+wind*4.5f;
    position=focus-along*9-wind*9+normal*7;
   }else{
    focus=ball.position+normal*.08f;position=focus-along*1.35f-wind*2.5f+normal*.7f;
   }
  }
  void FaceOutline(ScriptableRenderContext context,Camera camera){
   if(ball&&aura&&aura.enabled&&camera==UnityEngine.Camera.main)GolfBallEventFX.DrawAura(aura,ball.position,camera,outlineColor,outlineClock);
  }
  public void Dispose(){RenderPipelineManager.beginCameraRendering-=FaceOutline;GolfFishingAudio.Current?.StopLoop("wind-demo");if(root){root.gameObject.SetActive(false);UnityEngine.Object.Destroy(root.gameObject);}root=ball=null;}
 }
}
