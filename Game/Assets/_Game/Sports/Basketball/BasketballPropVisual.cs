using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace WhatTheFish {
 // Presentation follows snapshots; it never awards inventory or changes possession.
 public sealed class BasketballPropVisual:MonoBehaviour {
  BasketballBall ball;Transform pickup;SportsProp kind;
  GameObject[] models;Mesh quad;Mesh[] digits;Material effect;MeshRenderer[] portals=new MeshRenderer[2];MeshRenderer shadow,aim,hologram;MeshFilter digitFilter;int countdownNumber;
  MaterialPropertyBlock block;Transform[] visualBalls;Vector3[] ballScales;uint sequence;TrailRenderer trail;Material trailMaterial;
  readonly Dictionary<Athlete,TrailRenderer> turboTrails=new();readonly List<Athlete> removedActors=new();
  void Start(){
   ball=GetComponent<BasketballBall>();block=new MaterialPropertyBlock();models=new[]{Resources.Load<GameObject>("BasketballProps/LockOn"),Resources.Load<GameObject>("BasketballProps/Turbo"),Resources.Load<GameObject>("BasketballProps/Portal")};
   effect=Resources.Load<Material>("BasketballProps/PropEffect");
   quad=new Mesh{name="Shared ability effect quad"};quad.vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.5f,0),new Vector3(-.5f,.5f,0)};quad.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};quad.triangles=new[]{0,2,1,0,3,2};quad.RecalculateBounds();quad.RecalculateNormals();
   for(int i=0;i<2;i++)portals[i]=Plane("Portal",0);shadow=Plane("Pickup projection pad",2);aim=Plane("Lock-on countdown halo",3);
   hologram=Plane("Blue holographic spawn countdown",4);digitFilter=hologram.GetComponent<MeshFilter>();digits=new Mesh[5];for(int i=0;i<5;i++)digits[i]=Digit(i+1);
   var renderers=GetComponentsInChildren<MeshRenderer>();visualBalls=new Transform[renderers.Length];ballScales=new Vector3[renderers.Length];for(int i=0;i<renderers.Length;i++){visualBalls[i]=renderers[i].transform;ballScales[i]=renderers[i].transform.localScale;}
   trailMaterial=Resources.Load<Material>("BasketballProps/PropTrail");trail=gameObject.AddComponent<TrailRenderer>();trail.sharedMaterial=trailMaterial;trail.time=.15f;trail.startWidth=.07f;trail.endWidth=0;trail.startColor=new Color(1,.7f,.1f,.8f);trail.endColor=new Color(1,.7f,.1f,0);trail.emitting=false;trail.shadowCastingMode=ShadowCastingMode.Off;
  }
  MeshRenderer Plane(string name,int mode){var go=new GameObject(name);go.transform.SetParent(transform.parent,false);go.AddComponent<MeshFilter>().sharedMesh=quad;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=effect;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;r.enabled=false;return r;}
  void Style(MeshRenderer r,Color color,int mode,float progress=1){block.Clear();block.SetColor("_Tint",color);block.SetFloat("_Mode",mode);block.SetFloat("_Progress",progress);r.SetPropertyBlock(block);}
  public static Color ColorOf(SportsProp k)=>k==SportsProp.LockOn?new Color(1,.65f,.12f):k==SportsProp.Turbo?new Color(.10f,1,.63f):new Color(.08f,.6f,1);
  void Pickup(SportsPropSpawn spawn){
   if(kind!=spawn.kind){if(pickup)Destroy(pickup.gameObject);pickup=null;kind=spawn.kind;
    if(kind!=SportsProp.None&&models[(int)kind-1]){pickup=Instantiate(models[(int)kind-1],transform.parent).transform;pickup.name="Floating "+BasketballPropRules.Name(kind);}}
   var p=BasketballPropRules.Anchor(spawn.anchor);bool countdown=ball.Playing&&spawn.countingDown&&spawn.kind==SportsProp.None;
   shadow.enabled=ball.Playing&&(spawn.kind!=SportsProp.None||countdown);
   shadow.transform.localPosition=p+Vector3.up*.012f;shadow.transform.localRotation=Quaternion.Euler(90,0,0);shadow.transform.localScale=Vector3.one*(countdown?1.4f:1.1f);Style(shadow,countdown?new Color(.12f,.65f,1,.6f):ColorOf(spawn.kind),countdown?5:2);
   countdownNumber=countdown?spawn.CountdownNumber(BasketballMotion.Clock):0;hologram.enabled=countdown;
   if(countdown){
    digitFilter.sharedMesh=digits[countdownNumber-1];hologram.transform.localPosition=p+Vector3.up*(1.1f+.025f*Mathf.Sin(Time.time*2));
    var facing=Camera.main?Vector3.ProjectOnPlane(hologram.transform.position-Camera.main.transform.position,Vector3.up):transform.parent.forward;
    if(facing.sqrMagnitude>.001f)hologram.transform.rotation=Quaternion.LookRotation(facing.normalized)*Quaternion.Euler(0,12*Mathf.Sin(Time.time*.9f),0);
    float phase=1-Mathf.Repeat((float)(spawn.respawn-BasketballMotion.Clock),1);hologram.transform.localScale=Vector3.one*(1+.08f*Mathf.Exp(-phase*8));Style(hologram,new Color(.12f,.65f,1,.55f),4);
   }
   if(!pickup)return;var model=models[(int)spawn.kind-1].transform;
   // FBX unit conversion is stored on its root. Preserve it when animating the
   // pickup; replacing it with unit scale shrinks these models by 100 times.
   pickup.gameObject.SetActive(ball.Playing);pickup.localPosition=p+Vector3.up*(.60f+Mathf.Sin(Time.time*2)*.07f);pickup.localRotation=Quaternion.Euler(0,Time.time*30,0)*model.localRotation;pickup.localScale=model.localScale*.95f;
  }
  void LateUpdate(){
   if(!ball||!effect)return;var state=ball.Props;Pickup(state.pickup);
   var actor=state.aiming?ball.PropActor(state.aimer):null;aim.enabled=ball.Playing&&actor;
   if(actor){aim.transform.position=actor.transform.position+Vector3.up*.025f;aim.transform.rotation=Quaternion.Euler(90,0,0);aim.transform.localScale=Vector3.one*1.8f;Style(aim,ColorOf(SportsProp.LockOn),3,Mathf.Clamp01(1-(float)(state.aimUntil-BasketballMotion.Clock)/BasketballPropRules.AimSeconds));}
   float phase=(float)(BasketballMotion.Clock-state.portalAt)/BasketballPropRules.PortalSeconds;
   bool visible=ball.Playing&&state.portalSequence>0&&phase>=0&&phase<1.28f&&state.portalOutcome!=2;
   if(state.portalSequence!=sequence){sequence=state.portalSequence;trail.Clear();}
   float size=Mathf.SmoothStep(0,1,phase/.15f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(1,1.28f,phase)));
   for(int i=0;i<2;i++){
    var r=portals[i];r.enabled=visible;r.transform.position=(i==0?state.source:state.destination)+Vector3.forward*.04f;
    if(Camera.main)r.transform.rotation=Quaternion.LookRotation(r.transform.position-Camera.main.transform.position);
    r.transform.localScale=Vector3.one*(.72f*Mathf.Max(.001f,size));Style(r,ColorOf(SportsProp.Portal),0);
   }
   float scale=1;if(state.transferring)scale=phase<.5f?1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.15f,.48f,phase)):Mathf.SmoothStep(0,1,Mathf.InverseLerp(.52f,.88f,phase));ScaleBall(Mathf.Max(.001f,scale));
   trail.emitting=ball.Playing&&state.guided&&ball.Score.result==BasketballResult.Flying&&!state.transferring&&!ball.Held;
   foreach(var a in Athlete.Active){
    bool boosted=ball.TurboRemaining(a)>0;
    if(boosted&&!turboTrails.ContainsKey(a)){var go=new GameObject("Team turbo foot trail");go.transform.SetParent(a.transform,false);go.transform.localPosition=new Vector3(0,.12f,-.12f);var t=go.AddComponent<TrailRenderer>();t.sharedMaterial=trailMaterial;t.time=.28f;t.startWidth=.22f;t.endWidth=0;t.startColor=new Color(.15f,1,.62f,.8f);t.endColor=new Color(.15f,1,.62f,0);t.shadowCastingMode=ShadowCastingMode.Off;turboTrails[a]=t;}
    if(turboTrails.TryGetValue(a,out var renderer))renderer.emitting=boosted&&a.speed>.5f;
   }
   removedActors.Clear();foreach(var pair in turboTrails)if(!pair.Key||!Athlete.Active.Contains(pair.Key))removedActors.Add(pair.Key);foreach(var a in removedActors){if(turboTrails[a])Destroy(turboTrails[a].gameObject);turboTrails.Remove(a);}
  }
  void ScaleBall(float scale){if(visualBalls==null)return;for(int i=0;i<visualBalls.Length;i++)if(visualBalls[i]&&visualBalls[i]!=transform)visualBalls[i].localScale=ballScales[i]*scale;}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public Transform ReviewPickup()=>pickup;
  public int ReviewCountdownNumber=>countdownNumber;
  public MeshRenderer ReviewHologram=>hologram;
#endif
  void OnDisable(){ScaleBall(1);if(shadow)shadow.enabled=false;if(hologram)hologram.enabled=false;countdownNumber=0;foreach(var r in portals)if(r)r.enabled=false;if(aim)aim.enabled=false;if(trail)trail.Clear();if(pickup)pickup.gameObject.SetActive(false);foreach(var t in turboTrails.Values)if(t){t.emitting=false;t.Clear();}}
  void OnDestroy(){if(shadow)Destroy(shadow.gameObject);if(hologram)Destroy(hologram.gameObject);foreach(var r in portals)if(r)Destroy(r.gameObject);if(aim)Destroy(aim.gameObject);if(pickup)Destroy(pickup.gameObject);if(quad)Destroy(quad);if(digits!=null)foreach(var m in digits)Destroy(m);foreach(var t in turboTrails.Values)if(t)Destroy(t.gameObject);}
  // Five tiny extruded seven-segment meshes. No textures, fonts or per-tick allocations.
  public static Mesh Digit(int value){
   int[] masks={0,6,91,79,102,109};int mask=masks[value];var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
   Vector2[] centres={new(0,.44f),new(.26f,.22f),new(.26f,-.22f),new(0,-.44f),new(-.26f,-.22f),new(-.26f,.22f),Vector2.zero};
   for(int segment=0;segment<7;segment++){
    if((mask&(1<<segment))==0)continue;bool vertical=segment==1||segment==2||segment==4||segment==5;float length=vertical?.35f:.46f,width=.105f;
    Vector2[] outline={new(-length/2,0),new(-length/2+width/2,-width/2),new(length/2-width/2,-width/2),new(length/2,0),new(length/2-width/2,width/2),new(-length/2+width/2,width/2)};
    Vector3[] front=new Vector3[6],back=new Vector3[6];for(int i=0;i<6;i++){var p=outline[i];if(vertical)p=new Vector2(-p.y,p.x);p+=centres[segment];front[i]=new Vector3(p.x,p.y,-.07f);back[i]=new Vector3(p.x,p.y,.07f);}
    for(int i=1;i<5;i++){Face(front[0],front[i+1],front[i]);Face(back[0],back[i],back[i+1]);}
    for(int i=0;i<6;i++){int next=(i+1)%6;Face(front[i],front[next],back[next]);Face(front[i],back[next],back[i]);}
   }
   float min=float.MaxValue,max=float.MinValue;foreach(var v in vertices){min=Mathf.Min(min,v.x);max=Mathf.Max(max,v.x);}float centre=(min+max)*.5f;
   for(int i=0;i<vertices.Count;i++)vertices[i]-=Vector3.right*centre;
   var mesh=new Mesh{name="Holographic digit "+value};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
   void Face(Vector3 a,Vector3 b,Vector3 c){int start=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);uv.Add(new Vector2(a.x,a.y));uv.Add(new Vector2(b.x,b.y));uv.Add(new Vector2(c.x,c.y));triangles.Add(start);triangles.Add(start+1);triangles.Add(start+2);}
  }
 }
}
