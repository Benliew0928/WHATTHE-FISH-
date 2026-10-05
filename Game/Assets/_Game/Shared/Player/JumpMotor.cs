using UnityEngine;

namespace WhatTheFish {
 // Vertical movement only: steering stays in LocomotionMotor, including landing.
 public sealed class JumpMotor {
  public const float Height=1.2f,Gravity=22,LoadDuration=.08f,LandingDuration=.22f,BufferDuration=.12f,CoyoteDuration=.10f;
  public static float LaunchSpeed=>Mathf.Sqrt(2*Gravity*Height);
  public float Velocity {get;private set;}
  public bool Airborne {get;private set;}
  public bool Preparing {get;private set;}
  public float LoadElapsed {get;private set;}
  public float Landing {get;private set;}
  public uint Sequence {get;private set;}
  // The grounded phase includes both compression and extension. Leave the floor
  // at the extended pose, with the arm swing already driving upward.
  public static float Pose(bool preparing,float load,bool airborne,float velocity,float landing)=>preparing?.24f*Mathf.Clamp01(load/LoadDuration):airborne?.24f+.52f*Mathf.Clamp01((LaunchSpeed-velocity)/(2*LaunchSpeed)):.76f+.24f*Mathf.Clamp01(landing/LandingDuration);
  public float PoseTime=>Pose(Preparing,LoadElapsed,Airborne,Velocity,Landing);
  public bool Presenting=>Preparing||Airborne||Landing<LandingDuration;
  float buffer,grace;bool launched;
  public void Reset(){Velocity=-2;Airborne=Preparing=false;LoadElapsed=0;Landing=LandingDuration;buffer=grace=0;launched=false;Sequence++;}
  public void Launch(float velocity){Preparing=false;Airborne=launched=true;Velocity=velocity;LoadElapsed=0;buffer=grace=0;Landing=LandingDuration;Sequence++;}
  public void Request(){buffer=BufferDuration;}
  public float Step(bool supported,bool allowed,float dt){
   Landing=Mathf.Min(LandingDuration,Landing+dt);
   if(!Airborne&&supported)grace=CoyoteDuration;else grace=Mathf.Max(0,grace-dt);
   if(!allowed){buffer=0;if(Preparing){Preparing=false;launched=false;}}
   if(buffer>0&&grace>0&&!launched&&allowed){
    launched=true;grace=buffer=0;Sequence++;
    // Grounded input is visible immediately as a short knee/arm load. Rebounds
    // and coyote jumps already have bent legs, so they do not load a second time.
    if(supported&&Landing>=LandingDuration){Preparing=true;LoadElapsed=0;}
    else {Velocity=LaunchSpeed;Airborne=true;}
   }
   else if(!Airborne&&!supported){Airborne=true;Velocity=Mathf.Min(0,Velocity);Sequence++;}
   buffer=Mathf.Max(0,buffer-dt);
   if(Preparing){
    float used=supported?Mathf.Min(dt,LoadDuration-LoadElapsed):0;LoadElapsed+=used;
    if(LoadElapsed<LoadDuration&&supported){Velocity=-2;return -2*dt;}
    Preparing=false;Airborne=true;Velocity=LaunchSpeed;dt-=used;Sequence++;
   }
   if(!Airborne){Velocity=-2;return Velocity*dt;}
   float next=Mathf.Max(-25,Velocity-Gravity*dt),distance=(Velocity+next)*.5f*dt;Velocity=next;return distance;
  }
  public void Collide(CollisionFlags flags){
   if((flags&CollisionFlags.Above)!=0&&Velocity>0)Velocity=0;
   if((flags&CollisionFlags.Below)==0||Velocity>0)return;
   if(Airborne){Airborne=false;Landing=0;Sequence++;}
   Velocity=-2;if(!Preparing)launched=false;grace=CoyoteDuration;
  }
 }
}
