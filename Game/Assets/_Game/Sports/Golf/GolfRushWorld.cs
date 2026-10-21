using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace WhatTheFish {
 public sealed class GolfRushWorld:MonoBehaviour {
  GolfMatchManager match;float nextScan;
  void Start(){match=GetComponent<GolfMatchManager>();}
  void Update(){if(!match||!match.Context||Time.unscaledTime<nextScan)return;nextScan=Time.unscaledTime+.5f;foreach(var actor in Athlete.Active)if(actor&&!actor.GetComponent<GolfRushTrail>())actor.gameObject.AddComponent<GolfRushTrail>();}
 }
 // World-space history stays behind the runner. Teleports clear history; turning
 // clips old samples against the current facing plane, including ribbon width.
 public sealed class GolfRushTrail:MonoBehaviour {
  struct Sample {public Vector3 p,right;public float born;}
  readonly List<Sample> samples=new(40);readonly List<Vector3> vertices=new(1024);readonly List<Vector3> uv=new(1024);readonly List<Color> colors=new(1024);readonly List<int> indices=new(1536);
  Athlete actor;Mesh mesh;MeshRenderer trailRenderer;GameObject node;MaterialPropertyBlock properties;Vector3 previous;float credit;
  public int VisibleVertices=>mesh?mesh.vertexCount:0;
  public bool Emitting {get;private set;}
  public int FrontVertexCount {get;private set;}
  const float Lifetime=.7f;
  void Awake(){actor=GetComponent<Athlete>();previous=transform.position;}
  void Build(){node=new GameObject("Island Rush · wind ribbons and leaves");mesh=new Mesh{name="Pooled Rush wind trail"};mesh.MarkDynamic();node.AddComponent<MeshFilter>().sharedMesh=mesh;trailRenderer=node.AddComponent<MeshRenderer>();trailRenderer.sharedMaterial=Resources.Load<Material>("GolfRushTrail");trailRenderer.shadowCastingMode=ShadowCastingMode.Off;trailRenderer.receiveShadows=false;properties=new MaterialPropertyBlock();}
  void LateUpdate(){
   var match=GolfMatchManager.Instance;float dt=Time.deltaTime;var delta=transform.position-previous;previous=transform.position;float distance=Vector3.ProjectOnPlane(delta,Vector3.up).magnitude;
   if(distance>4||!match||!match.Context){samples.Clear();if(mesh)mesh.Clear();Emitting=false;return;}
   Emitting=match.Events.RushActive(match.Now)&&match.Player(actor)!=null&&!actor.IsSpectator&&!actor.inTransit&&!GolfCartWorld.Driving(actor)&&actor.Grounded&&dt>0&&distance/dt>2;
   var forward=Vector3.ProjectOnPlane(transform.forward,Vector3.up).normalized;
   samples.RemoveAll(s=>Time.time-s.born>Lifetime||(Vector3.ProjectOnPlane(s.p-transform.position,Vector3.up)).sqrMagnitude>3.6f*3.6f||Vector3.Dot(s.p-transform.position,forward)>-.16f);
   if(Emitting){credit+=dt;if(credit>=.025f){credit%=.025f;samples.Add(new Sample{p=transform.position-forward*.42f,right=transform.right,born=Time.time});if(samples.Count>32)samples.RemoveAt(0);}}
   if(samples.Count==0){if(mesh)mesh.Clear();return;}if(!mesh)Build();vertices.Clear();uv.Clear();colors.Clear();indices.Clear();FrontVertexCount=0;
   var camera=Camera.main;Vector3 view=camera?camera.transform.forward:Vector3.forward;
   for(int lane=0;lane<2;lane++){
    int last=-1;for(int i=samples.Count-1;i>=0;i--){var s=samples[i];float age=Time.time-s.born,t=Mathf.Max(age/Lifetime,Vector3.Distance(s.p,transform.position)/3.6f);float wave=Mathf.Sin(t*6+lane*2.4f+Time.time*2);var p=s.p+Vector3.up*((lane==0?.15f:.85f)+Mathf.Sin(t*6+lane*2)*(.15f+t*.25f))+s.right*(wave*(.12f+t*.4f)+(lane-.5f)*.25f);
     Vector3 tangent=i>0?s.p-samples[i-1].p:forward;var side=Vector3.Cross(tangent,view).normalized;if(side.sqrMagnitude<.1f)side=Vector3.up;
     float head=Mathf.SmoothStep(0,1,Mathf.Clamp01((Vector3.Distance(s.p,transform.position)-.4f)/.55f));float width=(.09f+lane*.025f)*Mathf.Sin(Mathf.PI*Mathf.Clamp01(1-t)*.85f)*head;var a=p-side*width;var b=p+side*width;
     if(Vector3.Dot(a-transform.position,forward)>-.1f||Vector3.Dot(b-transform.position,forward)>-.1f){last=-1;continue;}
     int at=vertices.Count;var c=new Color(.15f,1,1,(1-t)*.9f);V(a,new Vector3(t,0,0),c);V(b,new Vector3(t,1,0),c);if(last>=0){T(last,last+1,at);T(at,last+1,at+1);}last=at;
    }
   }
   for(int i=0;i<samples.Count;i+=3){var s=samples[i];float age=Time.time-s.born,t=age/Lifetime;var p=s.p+s.right*(Mathf.Sin(s.born*11)*.6f)+Vector3.up*(.2f+(i%4)*.28f+age*.35f);if(Vector3.Dot(p-transform.position,forward)>-.3f)continue;bool leaf=i%2==0;float size=leaf?.14f:.075f;var right=camera?camera.transform.right:Vector3.right;var up=camera?camera.transform.up:Vector3.up;Quad(p,(right+up*.5f).normalized*size,(up-right*.5f).normalized*size*(leaf?1.7f:1),leaf?1:2,leaf?new Color(.3f,1,.04f,1-t):new Color(1,.91f,.2f,1-t));}
   for(int i=1;i<samples.Count;i+=4){var s=samples[i];float t=(Time.time-s.born)/Lifetime;var p=s.p+s.right*Mathf.Sin(s.born*13)*.5f+Vector3.up*(.12f+i%3*.38f);if(Vector3.Dot(p-transform.position,forward)>-.4f)continue;var right=camera?camera.transform.right:Vector3.right;var up=camera?camera.transform.up:Vector3.up;Quad(p,(right+up*.35f).normalized*.18f,up*.055f,1,new Color(1,.88f,.16f,(1-t)*.95f));}
   mesh.Clear();mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetColors(colors);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();properties.SetVector("_RunnerPlane",new Vector4(forward.x,forward.y,forward.z,-Vector3.Dot(forward,transform.position)+.1f));trailRenderer.SetPropertyBlock(properties);
  }
  void T(int a,int b,int c){indices.Add(a);indices.Add(b);indices.Add(c);}
  void V(Vector3 p,Vector3 tex,Color c){vertices.Add(p);uv.Add(tex);colors.Add(c);if(Vector3.Dot(p-transform.position,transform.forward)>-.09f)FrontVertexCount++;}
  void Quad(Vector3 p,Vector3 right,Vector3 up,int kind,Color c){int n=vertices.Count;V(p-right-up,new Vector3(0,0,kind),c);V(p+right-up,new Vector3(1,0,kind),c);V(p+right+up,new Vector3(1,1,kind),c);V(p-right+up,new Vector3(0,1,kind),c);T(n,n+1,n+2);T(n,n+2,n+3);}
  void OnDisable(){samples.Clear();if(mesh)mesh.Clear();Emitting=false;}
  void OnDestroy(){if(node)Destroy(node);if(mesh)Destroy(mesh);}
 }
}
