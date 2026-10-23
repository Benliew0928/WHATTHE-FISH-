using UnityEngine;
using UnityEngine.Rendering;
namespace WhatTheFish {
 // A shaped vapor body, rotating condensation puffs and a dusty contact base.
 // Simulation remains in GolfHazardWorld; these pooled effects never apply force.
 public sealed class GolfTornadoFX:MonoBehaviour {
  const int Sides=32,Rows=18;
  Mesh mesh;MeshRenderer body;Vector3[] vertices;MaterialPropertyBlock properties;
  ParticleSystem vapor,cap,dust;float vaporCredit,capCredit,dustCredit;int emitted;bool reducedCap;
  public static GolfTornadoFX Create(Transform parent){var node=new GameObject("Tornado condensation and debris");node.transform.SetParent(parent,false);var fx=node.AddComponent<GolfTornadoFX>();fx.Build();return fx;}
  static Vector3 Bend(float t,float phase)=>new Vector3(Mathf.Sin(t*2.1f+phase*.12f)*.65f*t*t,0,Mathf.Cos(t*2.4f+phase*.1f)*.28f*t);
  void Build(){
   mesh=new Mesh{name="Shaped condensation funnel"};mesh.MarkDynamic();vertices=new Vector3[(Sides+1)*(Rows+1)];var uv=new Vector2[vertices.Length];var colors=new Color[vertices.Length];var triangles=new int[Sides*Rows*6];
   for(int y=0;y<=Rows;y++)for(int x=0;x<=Sides;x++){int i=y*(Sides+1)+x;float t=y/(float)Rows;uv[i]=new Vector2(x/(float)Sides,t);colors[i]=Color.Lerp(new Color(.38f,.29f,.2f,.96f),new Color(.39f,.45f,.51f,.94f),Mathf.SmoothStep(0,1,t*3));}
   int at=0;for(int y=0;y<Rows;y++)for(int x=0;x<Sides;x++){int i=y*(Sides+1)+x;triangles[at++]=i;triangles[at++]=i+Sides+1;triangles[at++]=i+1;triangles[at++]=i+1;triangles[at++]=i+Sides+1;triangles[at++]=i+Sides+2;}
   mesh.vertices=vertices;mesh.uv=uv;mesh.colors=colors;mesh.triangles=triangles;mesh.bounds=new Bounds(Vector3.up*4,new Vector3(8,10,8));gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
   body=gameObject.AddComponent<MeshRenderer>();body.sharedMaterial=Resources.Load<Material>("GolfEvents/StormVolume");body.shadowCastingMode=ShadowCastingMode.Off;body.receiveShadows=false;properties=new MaterialPropertyBlock();
   vapor=Particles("Rotating condensation",64,false);cap=Particles("Compact storm cloud",16,false);dust=Particles("Ground dust and debris",32,true);
  }
  ParticleSystem Particles(string name,int maximum,bool world){
   var node=new GameObject(name);node.transform.SetParent(transform,false);var ps=node.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
   var main=ps.main;main.loop=true;main.playOnAwake=false;main.maxParticles=maximum;main.simulationSpace=world?ParticleSystemSimulationSpace.World:ParticleSystemSimulationSpace.Local;main.startSpeed=0;
   var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.enabled=false;
   var fade=ps.colorOverLifetime;fade.enabled=true;fade.color=new Gradient{colorKeys=new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},alphaKeys=new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.9f,.12f),new GradientAlphaKey(.7f,.65f),new GradientAlphaKey(0,1)}};
   var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,.65f,1,1.25f));
   var renderer=node.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=Resources.Load<Material>("GolfEvents/StormParticle");renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;return ps;
  }
  public void Tick(double seconds){
   gameObject.SetActive(true);bool reduced=MenuPreferences.ReducedMotion;float phase=reduced?0:(float)seconds;
   properties.SetFloat("_Phase",phase);body.SetPropertyBlock(properties);
   for(int y=0;y<=Rows;y++)for(int x=0;x<=Sides;x++){float t=y/(float)Rows,a=x*Mathf.PI*2/Sides+phase*.8f,r=(.18f+1.9f*Mathf.Pow(t,1.15f)+.12f*Mathf.Sin(t*14-phase)*t)*(1+.14f*Mathf.Sin(a*3+t*11-phase*2));vertices[y*(Sides+1)+x]=Bend(t,phase)+new Vector3(Mathf.Cos(a)*r,t*8,Mathf.Sin(a)*r);}
   mesh.SetVertices(vertices);
   if(reduced){
    vapor.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);dust.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
    if(!reducedCap){cap.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);for(int i=0;i<9;i++){float a=i*Mathf.PI*2/9;cap.Emit(new ParticleSystem.EmitParams{position=Bend(1,0)+new Vector3(Mathf.Cos(a),7.7f,Mathf.Sin(a)),startLifetime=10,startSize=2.7f,startColor=new Color(.8f,.84f,.88f,.65f)},1);}cap.Simulate(2,true,false);cap.Pause();reducedCap=true;}return;
   }
   if(reducedCap){cap.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);reducedCap=false;}
   if(!vapor.isPlaying)vapor.Play();if(!cap.isPlaying)cap.Play();if(!dust.isPlaying)dust.Play();
   vaporCredit+=Time.deltaTime*28;capCredit+=Time.deltaTime*8;dustCredit+=Time.deltaTime*18;
   while(vaporCredit>=1){vaporCredit--;float t=Mathf.Repeat(++emitted*.618034f,1),a=phase*5+emitted*2.4f,r=.22f+1.85f*Mathf.Pow(t,1.65f);var radial=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));vapor.Emit(new ParticleSystem.EmitParams{position=Bend(t,phase)+radial*r+Vector3.up*(t*7.8f),velocity=Vector3.Cross(Vector3.up,radial)*(1.2f+t)+Vector3.up*1.2f,startLifetime=1.35f,startSize=.55f+t*1.2f,rotation=a*Mathf.Rad2Deg,startColor=new Color(.84f,.88f,.92f,.46f)},1);}
   while(capCredit>=1){capCredit--;float a=phase*1.4f+emitted;cap.Emit(new ParticleSystem.EmitParams{position=Bend(1,phase)+new Vector3(Mathf.Cos(a)*1.15f,7.7f+Mathf.Sin(a*2)*.18f,Mathf.Sin(a)*1.15f),velocity=new Vector3(-Mathf.Sin(a),.05f,Mathf.Cos(a))*.3f,startLifetime=1.7f,startSize=2.6f,rotation=a*Mathf.Rad2Deg,startColor=new Color(.78f,.83f,.88f,.58f)},1);}
   while(dustCredit>=1){dustCredit--;float a=phase*6+emitted;var radial=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));dust.Emit(new ParticleSystem.EmitParams{position=transform.TransformPoint(radial*1.5f+Vector3.up*.15f),velocity=transform.TransformDirection(Vector3.Cross(Vector3.up,radial)*2.2f+Vector3.up*.6f),startLifetime=1.3f,startSize=.8f,rotation=a*Mathf.Rad2Deg,startColor=new Color(.72f,.58f,.38f,.6f)},1);}
  }
  public void Hide(){vapor.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);cap.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);dust.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);reducedCap=false;vaporCredit=capCredit=dustCredit=0;gameObject.SetActive(false);}
  void OnDestroy(){if(mesh)Destroy(mesh);}
 }
}
