using UnityEngine;

namespace WhatTheFish {
 // One distance-driven clock for the whole body. Support targets live in world
 // space; swing trajectories include the ground-speed tangent at each end.
 // Short legs need a short contact duty at sprint speed, not a sliding stance.
 public sealed class FootballGait {
  public float Phase {get;private set;}
  public float Frequency {get;private set;}
  public float Duty {get;private set;}
  public float Amount {get;private set;}
  public float Run {get;private set;}
  public float Speed {get;private set;}
  public Vector3 Direction {get;private set;}=Vector3.forward;
  public readonly bool[] Planted=new bool[2];
  public readonly Vector3[] Pins=new Vector3[2];
  readonly Quaternion[] pinRotation=new Quaternion[2];
  readonly Quaternion[] previousRotation=new Quaternion[2];
  readonly float[] previous=new float[2];
  readonly bool[] eligible=new bool[2];
  float amountVelocity,speedVelocity,runVelocity;bool initialized;
  public static float Ease(float t){t=Mathf.Clamp01(t);return t*t*t*(t*(t*6-15)+10);}
  public void Reset(){Phase=Amount=Speed=Run=amountVelocity=speedVelocity=runVelocity=0;initialized=false;Planted[0]=Planted[1]=eligible[0]=eligible[1]=false;}
  public void Advance(Vector3 displacement,Quaternion facing,float dt,bool suspended){
   float actual=displacement.magnitude/dt;
   Speed=Mathf.SmoothDamp(Speed,suspended?0:actual,ref speedVelocity,.055f,100,dt);
   Amount=Mathf.SmoothDamp(Amount,suspended?0:Ease(actual/.45f),ref amountVelocity,.10f,100,dt);
   Run=Mathf.SmoothDamp(Run,Ease((Speed-1.05f)/2.7f),ref runVelocity,.16f,10,dt);
   Frequency=Mathf.Lerp(1.35f,2.65f,Mathf.Sqrt(Mathf.Clamp01(Speed/7)));
   float stride=Mathf.Max(.22f,actual/Frequency);
   Phase=Mathf.Repeat(Phase+Mathf.Min(3.2f,actual/stride)*dt,1);
   float reach=Mathf.Lerp(.095f,.135f,Run);
   Duty=Mathf.Clamp(2*reach*Frequency/Mathf.Max(.05f,Speed),.10f,.60f);
   var wanted=actual>.05f?displacement.normalized:facing*Vector3.forward;
   Direction=Vector3.Slerp(Direction,wanted,1-Mathf.Exp(-22*dt));
   if(Direction.sqrMagnitude<.01f)Direction=facing*Vector3.forward;
   Direction=Vector3.ProjectOnPlane(Direction,Vector3.up).normalized;
  }
  public float FootPhase(int side)=>Mathf.Repeat(Phase+side*.5f,1);
  public float Swing(int side)=>Mathf.Clamp01((FootPhase(side)-Duty)/(1-Duty));
  public float Flight=>Mathf.Min(Mathf.Sin(Mathf.PI*Swing(0)),Mathf.Sin(Mathf.PI*Swing(1)));
  public void Target(int side,Transform actor,Vector3 rest,Quaternion restRotation,float groundOffset,float dt,out Vector3 target,out Quaternion rotation){
   float f=FootPhase(side),swing=Swing(side),span=Speed*Duty/Frequency,half=span*.5f;
   if(!initialized||f<previous[side])eligible[side]=Amount>.985f;
   bool contact=f<Duty&&Amount>.985f&&eligible[side];
   if(contact&&Planted[side]&&Quaternion.Angle(actor.rotation*restRotation,pinRotation[side])>25){eligible[side]=false;contact=false;}
   var neutral=actor.TransformPoint(rest)-Vector3.up*groundOffset;
   var facing=actor.rotation;
   float x=half-Speed/Frequency*f,lift=0,pitch=0;
   if(f>=Duty){
    float duration=(1-Duty)/Frequency,k=Mathf.Min(.10f,.010f/duration);
    // Local velocity at touchdown/liftoff is -ground velocity. Localized
    // endpoint tangents avoid the huge overshoot of an unconstrained cubic.
    float tangent=swing*Mathf.Exp(-swing/k)-(1-swing)*Mathf.Exp(-(1-swing)/k);
    x=-half+span*Ease(swing)-Speed*duration*tangent;
    lift=Mathf.Pow(Mathf.Sin(Mathf.PI*swing),2)*Mathf.Lerp(.045f,.075f,Run);
    pitch=6*Mathf.Sin(2*Mathf.PI*swing)-5*Mathf.Sin(Mathf.PI*swing);
   }
   target=neutral+(Direction*x+Vector3.up*lift)*Amount;
   rotation=Quaternion.AngleAxis(pitch*Amount,Vector3.Cross(Vector3.up,Direction))*facing*restRotation;
   if(contact){
    if(!initialized||!Planted[side]||f<previous[side]){Pins[side]=target;pinRotation[side]=initialized?previousRotation[side]:facing*restRotation;}
    target=Pins[side];rotation=pinRotation[side];
   }
   else if(initialized)rotation=Quaternion.Slerp(previousRotation[side],rotation,1-Mathf.Exp(-25*dt));
   previousRotation[side]=rotation;
   Planted[side]=contact;previous[side]=f;if(side==1)initialized=true;
  }
 }
}
