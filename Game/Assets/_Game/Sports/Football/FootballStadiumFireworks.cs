using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 // Effects are evaluated from the shared match clock, including late arrivals.
 // There are no network particles, physics or per-frame objects.
 internal sealed class FootballStadiumFireworks {
  readonly Transform root;readonly Mesh mesh;readonly MeshRenderer renderer;readonly FootballStadiumEffectsRig rig;
  readonly List<Vector3> vertices=new(24000);readonly List<Vector2> uvs=new(24000);readonly List<Color> colors=new(24000);readonly List<int> indices=new(36000);
  public int VisibleParticles {get;private set;}
  public int FlameJets {get;private set;}
  public int LaserBeams {get;private set;}
  public int ElectricArcs {get;private set;}
  public int SkyShells {get;private set;}
  public Transform Root=>root;
  Vector3 right,up,view;uint seed;
  static readonly Color Gold=new(1,.48f,.015f),Cyan=new(0,.8f,1),Violet=new(.55f,.035f,1),Rose=new(1,.025f,.24f);
  public FootballStadiumFireworks(Transform pitch,Material material){
   root=new GameObject("Stadium perimeter fountains and sky fireworks").transform;root.SetParent(pitch,false);
   mesh=new Mesh{name="Shared-clock stadium effects"};mesh.MarkDynamic();root.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;renderer=root.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
   rig=new FootballStadiumEffectsRig(root);
  }
  public void SetVisible(bool visible){if(root&&root.gameObject.activeSelf!=visible)root.gameObject.SetActive(visible);}
  float Random(int n){uint x=(uint)n*747796405u+seed*2891336453u+277803737u;x=((x>>((int)(x>>28)+4))^x)*277803737u;return ((x>>22)^x)/(float)uint.MaxValue;}
  void Strip(Vector3 head,Vector3 tail,float headWidth,float tailWidth,Color color,float alpha,bool continuous=false,bool upright=false){
   if(alpha<=.005f)return;var axis=head-tail;var side=upright?right:Vector3.Cross(axis,view).normalized;if(side.sqrMagnitude<.000001f)side=right;
   int v=vertices.Count;vertices.Add(tail-side*tailWidth);vertices.Add(tail+side*tailWidth);vertices.Add(head-side*headWidth);vertices.Add(head+side*headWidth);
   // Joined flame segments retain opacity instead of resembling stacked beads.
   uvs.Add(new(0,continuous?.5f:0));uvs.Add(new(1,continuous?.5f:0));uvs.Add(new(0,1));uvs.Add(new(1,1));color.a=alpha;
   for(int i=0;i<4;i++)colors.Add(color);indices.Add(v);indices.Add(v+2);indices.Add(v+1);indices.Add(v+1);indices.Add(v+2);indices.Add(v+3);VisibleParticles++;
  }
  void Spark(Vector3 head,Vector3 tail,float width,Color color,float alpha)=>Strip(head,tail,width,width,color,alpha);
  void Beam(Vector3 a,Vector3 b,float width,Color color,float alpha){
   Strip(b,a,width*2.6f,width*2.6f,color,alpha*.12f,true);
   Strip(b,a,width,width,color,alpha,true);
   Strip(b,a,width*.19f,width*.19f,Color.Lerp(color,Color.white,.62f),alpha,true);
  }
  void Flames(float age,float fade){
   for(int jet=0;jet<8;jet++){
    var origin=FootballStadiumEffectsRig.FlameOrigin(jet);float pulse=.87f+.09f*Mathf.Sin(age*2.2f-jet*.6f)+.04f*Mathf.Sin(age*5.1f+jet);FlameJets++;
    for(int tongue=0;tongue<3;tongue++){
     float phase=age*3.2f+jet*1.7f+tongue*2.1f,height=(tongue==0?9.6f:6.9f)*pulse*fade;
     Vector3 Point(float t)=>origin+Vector3.up*(t*height)+new Vector3(Mathf.Sin(phase-t*5.5f)*t*t*.8f+(tongue-1)*t*.8f,0,Mathf.Cos(phase*.8f-t*4)*t*t*.65f);
     float Width(float t)=>Mathf.Max(.012f,(.29f+Mathf.Sin(t*Mathf.PI)*.67f)*(1-t));
     // Red silhouette, orange body and a smaller golden core retain contrast
     // in sunlight without depending on bloom or changing the arena lighting.
     for(int layer=0;layer<3;layer++)for(int s=0;s<10;s++){
      float t0=s/10f,t1=(s+1)/10f,scale=layer==0?1.38f:layer==1?.92f:.38f;
      float length=layer==2?.70f:1;Color color=layer==0?new Color(.92f,.035f,.003f):layer==1?new Color(1,.27f,.005f):new Color(1,.78f,.08f);
      Strip(Point(t1*length),Point(t0*length),Width(t1)*scale,Width(t0)*scale,color,fade*(layer==0?.85f:.96f),true,true);
     }
    }
    for(int i=0;i<12;i++){
     float flight=Mathf.Repeat(age*.85f+i*.137f+jet*.23f,1.65f);
     Vector3 P(float t)=>origin+new Vector3(Mathf.Sin(i*5.1f+jet)*t*2,11*t-2*t*t,Mathf.Cos(i*7.2f+jet)*t*2);
     Spark(P(flight),P(Mathf.Max(0,flight-.08f)),.10f,Gold,fade*(1-flight/1.65f));
    }
   }
  }
  void Lasers(float age,float fade){
   for(int tower=0;tower<4;tower++)for(int ray=0;ray<3;ray++){
    var origin=FootballStadiumEffectsRig.LaserOrigin(tower);float side=Mathf.Sign(origin.x),end=Mathf.Sign(origin.z);
    float sweep=Mathf.Sin(age*.72f+tower*.8f),spread=ray-1;
    var target=origin+new Vector3(side*(19+sweep*9+spread*6),32+spread*5,end*(14-sweep*5+spread*4));
    Beam(origin+Vector3.right*(spread*.6f),target,.15f,ray==1?Rose:Cyan,fade*.73f);LaserBeams++;
   }
  }
  void Lightning(float age,float fade){
   for(int side=0;side<2;side++)for(int span=0;span<3;span++){
    var start=FootballStadiumEffectsRig.FlameOrigin(span*2+side)+Vector3.up*.4f;
    var end=FootballStadiumEffectsRig.FlameOrigin((span+1)*2+side)+Vector3.up*.4f;
    Vector3 Point(int i){float t=i/16f,envelope=Mathf.Sin(t*Mathf.PI);return Vector3.Lerp(start,end,t)+new Vector3(Mathf.Sign(start.x)*envelope*(.8f+Mathf.Sin(i*2.7f+age*9)*.55f),envelope*(2.2f+Mathf.Sin(i*1.9f-age*11)*.9f),0);}
    for(int i=1;i<=16;i++)Beam(Point(i-1),Point(i),.13f,Violet,fade*(.76f+.12f*Mathf.Sin(age*5+span)));
    ElectricArcs++;
   }
  }
  void Shells(float age,float fade){
   // Overlapping shells and long coloured tails read against the daytime sky.
   for(int burst=0;burst<30;burst++){
    float launch=.1f+burst*.39f,t=age-launch;if(t<0||t>4.2f)continue;
    float angle=burst<8?(burst-3.5f)*.29f:burst*2.399963f;var origin=new Vector3(Mathf.Sin(angle)*48,5,-Mathf.Cos(angle)*72);
    var centre=origin+new Vector3(Mathf.Sin(angle)*5,33+Random(burst)*12,-Mathf.Cos(angle)*5);
    Color color=burst%4==0?Gold:burst%4==1?Cyan:burst%4==2?Violet:Rose;SkyShells++;
    if(t<1){var head=Vector3.Lerp(origin,centre,1-(1-t)*(1-t));Beam(head-Vector3.up*(3+4*(1-t)),head,.22f,Gold,fade);continue;}
    float life=t-1,envelope=Mathf.Pow(Mathf.Clamp01(1-life/3.2f),.65f)*fade;
    for(int i=0;i<48;i++){
     float y=1-2*(i+.5f)/48,rad=Mathf.Sqrt(1-y*y),spin=i*2.399963f+Random(burst+71)*6.28f;
     var direction=new Vector3(Mathf.Cos(spin)*rad,y,Mathf.Sin(spin)*rad);float speed=10+Random(burst*100+i)*5;
     Vector3 Position(float a)=>centre+direction*speed*(1-Mathf.Exp(-a*.85f))/.85f+Vector3.down*(a*a*1.15f);
     var point=Position(life);float width=.14f+envelope*.16f;
     // Tapered curved comet tails, rather than square confetti fragments.
     for(int segment=0;segment<3;segment++){
      float q0=segment/3f,q1=(segment+1)/3f;
      var tail=Position(Mathf.Max(0,life-.78f*(1-q0)));var head=Position(Mathf.Max(0,life-.78f*(1-q1)));
      float a=(.22f+.78f*q1)*envelope,w0=width*(.12f+.88f*q0),w1=width*(.12f+.88f*q1);
      Strip(head,tail,w1*1.65f,w0*1.65f,color*.50f,a*.85f,true);Strip(head,tail,w1,w0,color,a,true);
     }
     Spark(point+up*.14f,point-up*.14f,.13f,Color.Lerp(color,Color.white,.35f),envelope);
    }
   }
  }
  public void Present(float age,uint revision,Camera camera){
   VisibleParticles=FlameJets=LaserBeams=ElectricArcs=SkyShells=0;
   if(age<0||age>=FootballStadiumCelebration.TeamSeconds||!camera){renderer.enabled=false;return;}
   renderer.enabled=true;seed=revision;vertices.Clear();uvs.Clear();colors.Clear();indices.Clear();
   right=root.InverseTransformDirection(camera.transform.right);up=root.InverseTransformDirection(camera.transform.up);view=root.InverseTransformDirection(camera.transform.forward);
   float fade=Mathf.SmoothStep(0,1,Mathf.Min(age/.65f,(FootballStadiumCelebration.TeamSeconds-age)/1.1f));
   Lasers(age,fade);Lightning(age,fade);Flames(age,fade);Shells(age,fade);
   mesh.Clear();mesh.SetVertices(vertices);mesh.SetUVs(0,uvs);mesh.SetColors(colors);mesh.SetTriangles(indices,0);
   mesh.bounds=new Bounds(new Vector3(0,34,0),new Vector3(190,105,230));
  }
  public void Dispose(){rig.Dispose();if(root)Object.Destroy(root.gameObject);if(mesh)Object.Destroy(mesh);}
 }
}
