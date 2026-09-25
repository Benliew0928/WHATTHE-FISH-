using UnityEngine;

namespace WhatTheFish {
 public enum LocomotionPhase : byte { Idle, Moving, Braking, Turning, Launching }

 // Input owns travel. Facing and animation can never hold or queue movement.
 public sealed class LocomotionMotor {
  public LocomotionPhase Phase {get;private set;}
  public Vector3 Velocity {get;private set;}
  public float Yaw {get;private set;}
  public float TurnAngle {get;private set;}
  public float TurnStartYaw {get;private set;}
  public float Duration {get;private set;}
  public float Elapsed {get;private set;}
  public uint Sequence {get;private set;}
  public float TurnProgress=>PoseProgress(TurnStartYaw,TurnAngle,Yaw);
  float lastSide=1,yawVelocity,turnTarget;
  public static float Ease(float t){t=Mathf.Clamp01(t);return t*t*(3-2*t);}
  public static float FacingProgress(float t)=>Ease((t-.15f)/.67f);
  public static float PoseProgress(float start,float angle,float yaw){
   if(Mathf.Abs(angle)<.01f)return 1;
   float fraction=Mathf.Clamp01(Mathf.DeltaAngle(start,yaw)/angle);
   if(fraction<=0)return 0;if(fraction>=1)return 1;
   float lo=0,hi=1;for(int i=0;i<12;i++){float mid=(lo+hi)*.5f;if(Ease(mid)<fraction)lo=mid;else hi=mid;}
   return .15f+.67f*(lo+hi)*.5f;
  }
  public void Reset(float yaw){Yaw=yaw;yawVelocity=0;lastSide=1;Velocity=Vector3.zero;TurnAngle=0;Enter(LocomotionPhase.Idle,0);}
  void Enter(LocomotionPhase phase,float duration){Phase=phase;Duration=duration;Elapsed=0;Sequence++;}
  float Angle(Vector3 direction){
   float angle=Mathf.DeltaAngle(Yaw,Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg);
   if(Mathf.Abs(angle)>178)angle=180*lastSide;
   else if(Mathf.Abs(angle)>15)lastSide=Mathf.Sign(angle);
   return angle;
  }
  public Vector3 Step(Vector3 input,float requestedSpeed,bool grounded,float dt){
   if(dt<=0)return Vector3.zero;
   input=Vector3.ClampMagnitude(input,1);input.y=0;
   Vector3 displacement=Vector3.zero;
   int count=Mathf.Max(1,Mathf.CeilToInt(dt/(1f/120)));
   for(int i=0;i<count;i++)displacement+=Tick(input,requestedSpeed,grounded,dt/count);
   return displacement;
  }
  Vector3 Tick(Vector3 input,float requestedSpeed,bool grounded,float dt){
   float previousSpeed=Velocity.magnitude;
   if(input.sqrMagnitude<.15f*.15f){
    // Release cancels facing immediately; only the short stop easing remains.
    yawVelocity=0;
    if(previousSpeed>.001f){
     if(Phase!=LocomotionPhase.Braking)Enter(LocomotionPhase.Braking,.08f);
     float next=Mathf.MoveTowards(previousSpeed,0,requestedSpeed/.08f*dt);
     var direction=Velocity.normalized;Velocity=direction*next;Elapsed+=dt;
     if(next==0)Enter(LocomotionPhase.Idle,0);
     return direction*((previousSpeed+next)*.5f*dt);
    }
    Velocity=Vector3.zero;if(Phase!=LocomotionPhase.Idle)Enter(LocomotionPhase.Idle,0);
    return Vector3.zero;
   }
   var wanted=input.normalized;float angle=Angle(wanted);
   float targetYaw=Yaw+angle;
   bool turning=grounded&&(Mathf.Abs(angle)>60||(Phase==LocomotionPhase.Turning&&Mathf.Abs(angle)>8));
   if(turning){
    if(Phase!=LocomotionPhase.Turning||Mathf.Abs(Mathf.DeltaAngle(turnTarget,targetYaw))>20){
     TurnStartYaw=Yaw;TurnAngle=angle;turnTarget=targetYaw;Enter(LocomotionPhase.Turning,0);
    }
   }else if(Phase==LocomotionPhase.Turning||Phase==LocomotionPhase.Idle||Phase==LocomotionPhase.Braking){
    Enter(previousSpeed>.1f?LocomotionPhase.Moving:LocomotionPhase.Launching,.1f);
   }
   // Immediately discard rotational momentum that points away from fresh input.
   if(yawVelocity*angle<0)yawVelocity=0;
   Yaw=Mathf.SmoothDampAngle(Yaw,targetYaw,ref yawVelocity,.09f,1620,dt);
   float nextSpeed=Mathf.MoveTowards(previousSpeed,requestedSpeed*input.magnitude,requestedSpeed/.1f*dt);
   Velocity=wanted*nextSpeed;Elapsed+=dt;
   if(Phase==LocomotionPhase.Launching&&Elapsed>=Duration)Enter(LocomotionPhase.Moving,0);
   // Preserve speed through reversals. Averaging opposing velocity vectors would
   // erase rapid up/down input, so ease speed only and use the latest direction.
   return wanted*((previousSpeed+nextSpeed)*.5f*dt);
  }
 }
}
