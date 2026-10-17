using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 // Cosmetic local wheel contacts only. World-space marks never alter terrain.
 [DefaultExecutionOrder(180)]
 public sealed class GolfCartGroundFX:MonoBehaviour {
  const int Capacity=512;GolfCart cart;GolfFeedbackSettings settings;Mesh mesh;GameObject marks;ParticleSystem dust;
  readonly Vector3[] vertices=new Vector3[Capacity*4];readonly Color[] colours=new Color[Capacity*4];readonly float[] born=new float[Capacity],alphas=new float[Capacity];
  readonly Vector3[] previous=new Vector3[4];readonly bool[] contact=new bool[4];int cursor;Vector3 last;bool initialized;
  public int TotalMarks {get;private set;}public int TotalParticles {get;private set;}
  public int LiveMarks {get{int n=0;for(int i=0;i<Capacity;i++)if(alphas[i]>0&&Time.time-born[i]<settings.wheelTrackLifetime)n++;return n;}}
  void Awake(){cart=GetComponent<GolfCart>();settings=Resources.Load<GolfFeedbackSettings>("GolfFeedback");}
  void Build(){
   if(mesh)return;var material=Resources.Load<Material>("GolfFeedback");marks=new GameObject("Fading Golf wheel contacts");mesh=new Mesh{name="Pooled Golf wheel marks"};mesh.MarkDynamic();marks.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=marks.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
   var uv=new Vector2[vertices.Length];var triangles=new int[Capacity*6];
   for(int i=0;i<Capacity;i++){int v=i*4,t=i*6;uv[v]=new Vector2(0,0);uv[v+1]=new Vector2(1,0);uv[v+2]=new Vector2(1,1);uv[v+3]=new Vector2(0,1);triangles[t]=v;triangles[t+1]=v+1;triangles[t+2]=v+2;triangles[t+3]=v;triangles[t+4]=v+2;triangles[t+5]=v+3;}
   mesh.vertices=vertices;mesh.uv=uv;mesh.colors=colours;mesh.triangles=triangles;
   var go=new GameObject("Golf wheel soil flecks");go.transform.SetParent(transform,false);dust=go.AddComponent<ParticleSystem>();dust.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=dust.main;main.loop=false;main.playOnAwake=false;main.maxParticles=256;main.simulationSpace=ParticleSystemSimulationSpace.World;main.startSpeed=0;main.duration=1;main.gravityModifier=1;var emission=dust.emission;emission.enabled=false;var shape=dust.shape;shape.enabled=false;var fade=dust.colorOverLifetime;fade.enabled=true;fade.color=new Gradient{alphaKeys=new[]{new GradientAlphaKey(.7f,0),new GradientAlphaKey(0,1)},colorKeys=new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)}};var pr=go.GetComponent<ParticleSystemRenderer>();pr.sharedMaterial=material;pr.shadowCastingMode=ShadowCastingMode.Off;pr.receiveShadows=false;
  }
  void LateUpdate(){
   if(!GolfCartWorld.Allowed){Clear();return;}
   var delta=transform.position-last;last=transform.position;
   bool moving=initialized&&delta.sqrMagnitude>.000001f&&delta.sqrMagnitude<9&&cart.State.driver!=GolfCartState.NoDriver;initialized=true;
   if(moving&&cart.wheelPivots!=null&&cart.wheelPivots.Length==4){
    Build();for(int i=0;i<4;i++){
     var wheel=cart.wheelPivots[i];if(!wheel||!Physics.Raycast(wheel.position+Vector3.up*.4f,Vector3.down,out var ground,1.3f,1<<8,QueryTriggerInteraction.Ignore)||!GolfCartMotor.Terrain(ground.collider)){contact[i]=false;continue;}
     var point=ground.point+ground.normal*.012f;
     if(!contact[i]){previous[i]=point;contact[i]=true;continue;}
     var path=point-previous[i];if(path.magnitude<.45f)continue;if(path.magnitude>2){previous[i]=point;continue;}
     var side=Vector3.Cross(ground.normal,path.normalized)*.085f;int slot=cursor++%Capacity,v=slot*4;
     vertices[v]=previous[i]-side;vertices[v+1]=previous[i]+side;vertices[v+2]=point+side;vertices[v+3]=point-side;born[slot]=Time.time;
     var course=GolfMatchManager.Instance?GolfMatchManager.Instance.Course:null;bool sand=false;if(course){var local=course.transform.InverseTransformPoint(point);sand=MobileIslandGrass.GolfSand(local.x,local.z)>.5f;foreach(var patch in course.sandPatches)if(patch.Contains(local.x,local.z))sand=true;}
     alphas[slot]=settings.wheelTrackOpacity*(sand?1:.8f);TotalMarks++;
     for(int k=0;k<settings.wheelParticles;k++){var p=new ParticleSystem.EmitParams{position=point-path.normalized*Random.Range(0,.35f)+side*Random.Range(-1.5f,1.5f),velocity=-path.normalized*Random.Range(.5f,1.5f)+ground.normal*Random.Range(.7f,1.5f)+side*Random.Range(-5f,5f),startLifetime=Random.Range(.35f,.65f),startSize=Random.Range(.045f,.085f),startColor=sand?new Color(.55f,.4f,.22f,.6f):new Color(.28f,.24f,.12f,.5f)};dust.Emit(p,1);TotalParticles++;}previous[i]=point;
    }
    mesh.vertices=vertices;mesh.RecalculateBounds();
   }else if(delta.sqrMagnitude>=9||cart.State.driver==GolfCartState.NoDriver)for(int i=0;i<4;i++)contact[i]=false;
   if(mesh){for(int i=0;i<Capacity;i++){float age=Time.time-born[i];var colour=new Color(.24f,.2f,.1f,alphas[i]*Mathf.Clamp01(1-age/settings.wheelTrackLifetime));for(int k=0;k<4;k++)colours[i*4+k]=colour;}mesh.colors=colours;}
  }
  void Clear(){initialized=false;for(int i=0;i<4;i++)contact[i]=false;for(int i=0;i<Capacity;i++)alphas[i]=0;if(mesh){System.Array.Clear(colours,0,colours.Length);mesh.colors=colours;}if(dust)dust.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
  void OnDisable(){Clear();}
  void OnDestroy(){if(marks)Destroy(marks);if(mesh)Destroy(mesh);}
 }
}
