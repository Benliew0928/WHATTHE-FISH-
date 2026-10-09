using UnityEngine;

namespace WhatTheFish {
 public enum GolfShotMode:byte { Swing,Putt }
 public static class GolfShotRules {
  public const float PuttDistance=12f,PowerSweepSeconds=1.3f;
  public static float Power(float elapsed)=>Mathf.PingPong(Mathf.Max(0,elapsed)/PowerSweepSeconds,1);
 }
 // One terrain-aware trajectory is used for presentation and authoritative
 // travel. Swings end at first contact; putts end at rest. Ordinary rigidbody
 // physics resumes on landing, impact with another ball, or a gameplay impulse.
 public sealed class GolfShotPlan {
  public const float Step=.02f;
  const int Capacity=1001,GroundMask=1<<8;
  public readonly Vector3[] Points=new Vector3[Capacity],Velocities=new Vector3[Capacity];
  public readonly float[] Times=new float[Capacity];
  public int Count {get;private set;}
  public GolfShotMode Mode {get;private set;}
  public bool Resting {get;private set;}
  public bool HasTarget {get;private set;}
  public Vector3 Normal {get;private set;}
  public Vector3 EndPoint=>Points[Count-1];
  public Vector3 LaunchVelocity {get;private set;}
  public float Duration=>Times[Count-1];
  void Add(Vector3 position,Vector3 velocity,float time){Points[Count]=position;Velocities[Count]=velocity;Times[Count++]=time;}
  static bool Ground(Vector3 p,out RaycastHit hit)=>Physics.Raycast(p+Vector3.up*.18f,Vector3.down,out hit,GolfBall.Radius+.36f,GroundMask,QueryTriggerInteraction.Ignore)&&hit.normal.y>.35f&&Mathf.Abs(Vector3.Dot(p-hit.point,hit.normal)-GolfBall.Radius)<.13f;
  public void Build(Vector3 origin,Vector3 direction,float charge,GolfShotMode mode,GolfCourse course,GolfBallPhysicsSettings settings){
   Mode=mode;Count=0;Resting=HasTarget=false;Normal=Vector3.up;
   Vector3 p=origin,v=settings.SwingVelocity(direction,charge,mode);LaunchVelocity=v;Add(p,v,0);
   bool slow=false;float time=0;
   for(int i=1;i<Capacity;i++){
    RaycastHit support=default;bool supported=mode==GolfShotMode.Putt&&Ground(p,out support)&&Vector3.Dot(v,support.normal)<.2f;
    Vector3 next,normal=Vector3.up;
    if(supported){
     // Project onto the authored turf, including its cross-slope break. Below
     // the settling threshold friction holds against gravity, as in GolfBall.
     var hit=support;normal=hit.normal;v=Vector3.ProjectOnPlane(v,normal);slow|=v.magnitude<settings.slowRollingThreshold;
     float resistance=course.RollingResistance(hit.point,settings)+(slow?settings.rollingResistanceStrength*(1-Mathf.Clamp01(v.magnitude/settings.slowRollingThreshold)):0);
     if(!slow)v+=Vector3.ProjectOnPlane(Physics.gravity,normal)*Step;
     v=Vector3.MoveTowards(v,Vector3.zero,resistance*Step);v*=1-.05f*Step;
     if(v.magnitude<settings.stopSpeedThreshold){v=Vector3.zero;Resting=HasTarget=true;Normal=normal;Add(hit.point+normal*GolfBall.Radius,v,time+Step);break;}
     next=p+v*Step;
     if(Ground(next,out var ground)){next=ground.point+ground.normal*(GolfBall.Radius+.001f);normal=ground.normal;}
    }else{
     v=(v+Physics.gravity*Step)*(1-.05f*Step);next=p+v*Step;
    }
    var delta=next-p;
    if(delta.sqrMagnitude>1e-9f&&Physics.SphereCast(p,GolfBall.Radius,delta.normalized,out var collision,delta.magnitude,GroundMask,QueryTriggerInteraction.Ignore)){
     // Supporting turf is already followed above. A wall or a flight's first
     // contact ends the guide exactly at the swept sphere's contact position.
     if(!supported||collision.normal.y<.35f){
      next=p+delta.normalized*collision.distance;Normal=collision.normal;
      if(mode==GolfShotMode.Swing||collision.normal.y<.35f){
       HasTarget=true;v=Vector3.ProjectOnPlane(v,Normal);
       if(mode==GolfShotMode.Swing&&Normal.y>.35f)v*=settings.landingSpeedRetention;
       Add(next,v,time+Step*Mathf.Clamp01(collision.distance/delta.magnitude));break;
      }
      v=Vector3.ProjectOnPlane(v,collision.normal);
     }
    }
    p=next;time+=Step;Add(p,v,time);Normal=normal;
    if((p-origin).sqrMagnitude>14400||p.y<origin.y-25)break;
   }
  }
  public void Sample(float time,ref int index,out Vector3 position,out Vector3 velocity){
   while(index<Count-2&&Times[index+1]<time)index++;
   int next=Mathf.Min(index+1,Count-1);float t=Mathf.InverseLerp(Times[index],Times[next],time);
   position=Vector3.Lerp(Points[index],Points[next],t);velocity=Vector3.Lerp(Velocities[index],Velocities[next],t);
  }
 }
}
