using UnityEngine;
namespace WhatTheFish {
 // Bounded shared-material droplets. Velocity is inherited from the rendered
 // fish; gravity and water contact determine each droplet's path and lifetime.
 // This is presentation only: fish forces, claims and scoring remain on the host.
 public sealed class FishingSplash:MonoBehaviour {
  const int Capacity=48;const float Gravity=9.81f;
  readonly LineRenderer[] drops=new LineRenderer[Capacity];readonly Vector3[] origins=new Vector3[Capacity],velocities=new Vector3[Capacity];readonly float[] ages=new float[Capacity];readonly bool[] live=new bool[Capacity];
  readonly Vector3[] previous=new Vector3[FishingState.FishCount];readonly float[] previousSurface=new float[FishingState.FishCount];readonly bool[] observed=new bool[FishingState.FishCount];readonly float[] nextBurst=new float[FishingState.FishCount];FishingGame game;FishingPresentation presentation;int cursor;
  public int BurstCount {get;private set;}
  public int LiveDrops {get{int n=0;foreach(bool active in live)if(active)n++;return n;}}
  public bool ArcVisible {get{for(int i=0;i<Capacity;i++)if(live[i]&&ages[i]>.07f)return true;return false;}}
  public void Bind(FishingGame owner,FishingPresentation source){game=owner;presentation=source;}
  public static Vector3 PositionAt(Vector3 origin,Vector3 velocity,float time)=>origin+velocity*time+Vector3.down*(.5f*Gravity*time*time);
  public void Observe(int id,Vector3 position,float bodySurface,bool hooked,float surge){
   if(id<0||id>=previous.Length)return;float delta=Time.deltaTime;
   if(!hooked||delta<=0||delta>.1f){previous[id]=position;previousSurface[id]=bodySurface;observed[id]=hooked;return;}
   if(!observed[id]){previous[id]=position;previousSurface[id]=bodySurface;observed[id]=true;return;}
   var velocity=(position-previous[id])/delta;float water=game.WaterHeight(position);bool crossing=(previousSurface[id]-water)*(bodySurface-water)<=0;
   // The visible body, including its pitched tail, displaces the surface even
   // while its centre is submerged. Never treat the model pivot as contact.
   bool thrashing=bodySurface>=water-.15f&&position.y<=water+.25f&&velocity.sqrMagnitude>1.2f&&surge>.35f;
   previous[id]=position;previousSurface[id]=bodySurface;if((crossing||thrashing)&&Time.time>=nextBurst[id]){nextBurst[id]=Time.time+.30f;Burst(new Vector3(position.x,water+.035f,position.z),velocity,id);}
  }
  void Burst(Vector3 origin,Vector3 velocity,int fish){
   BurstCount++;int count=MenuPreferences.ReducedMotion?4:12;
   for(int i=0;i<count;i++){int n=cursor++%Capacity;if(!drops[n]){drops[n]=presentation.Line("Fishing water droplet "+n,transform,.026f);drops[n].positionCount=2;drops[n].numCapVertices=2;}
    float angle=(i+.37f*fish)*Mathf.PI*2/count,speed=1.3f+.22f*(i%3);var radial=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));origins[n]=origin;velocities[n]=Vector3.ClampMagnitude(velocity,4)*.30f+radial*speed+Vector3.up*(2.4f+.20f*(i%4));ages[n]=0;live[n]=true;drops[n].SetPosition(0,origin);drops[n].SetPosition(1,origin+velocities[n]*.035f);drops[n].startWidth=.05f;drops[n].endWidth=.075f;drops[n].startColor=drops[n].endColor=new Color(.8f,1,1,.95f);drops[n].enabled=true;
   }
  }
  void LateUpdate(){
   if(!game||!game.Context){Clear();return;}
   for(int i=0;i<Capacity;i++){if(!live[i])continue;ages[i]+=Time.deltaTime;var p=PositionAt(origins[i],velocities[i],ages[i]);
    if(ages[i]>1||p.y<game.WaterHeight(p)){live[i]=false;drops[i].enabled=false;continue;}
    var v=velocities[i]+Vector3.down*Gravity*ages[i];drops[i].SetPosition(0,p);drops[i].SetPosition(1,p-Vector3.ClampMagnitude(v*.040f,.16f));float fade=1-Mathf.Clamp01(ages[i]);drops[i].startWidth=.05f*fade;drops[i].endWidth=.075f*fade;drops[i].startColor=drops[i].endColor=new Color(.8f,1,1,.95f*fade);
   }
  }
  public void Clear(){for(int i=0;i<Capacity;i++){live[i]=false;if(drops[i])drops[i].enabled=false;}for(int i=0;i<observed.Length;i++)observed[i]=false;}
  void OnDisable()=>Clear();void OnApplicationPause(bool pause){if(pause)Clear();}
 }
}
