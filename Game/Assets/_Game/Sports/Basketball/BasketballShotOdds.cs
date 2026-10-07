using UnityEngine;

namespace WhatTheFish {
 public static class BasketballShotOdds {
  public const float CloseDistance=4,ThreeDistance=7.2644f,HalfDistance=13,FullDistance=26;
  static float DistanceCurve(float distance,float close,float three,float half,float full){
   if(distance<=CloseDistance)return close;
   if(distance<=ThreeDistance)return Mathf.Lerp(close,three,Mathf.InverseLerp(CloseDistance,ThreeDistance,distance));
   if(distance<=HalfDistance)return Mathf.Lerp(three,half,Mathf.InverseLerp(ThreeDistance,HalfDistance,distance));
   return Mathf.Lerp(half,full,Mathf.InverseLerp(HalfDistance,FullDistance,distance));
  }
  public static float Quality(float power,float window=BasketballBall.SweetWindow){
   if(!float.IsFinite(power)||!float.IsFinite(window))return 0;
   power=Mathf.Clamp01(power);window=Mathf.Clamp(window,.025f,BasketballBall.SweetWindow);
   float error=Mathf.Max(0,Mathf.Abs(power-BasketballBall.SweetSpot)-window);
   float span=(power<BasketballBall.SweetSpot?BasketballBall.SweetSpot:1-BasketballBall.SweetSpot)-window;
   return Mathf.Clamp01(1-error/span);
  }
  public static float Chance(float distance,float power,float window=BasketballBall.SweetWindow){
   if(!float.IsFinite(distance)||!float.IsFinite(power)||!float.IsFinite(window))return 0;
   float best=DistanceCurve(distance,.95f,.80f,.30f,.05f);
   float floor=DistanceCurve(distance,.08f,.04f,.01f,.002f);
   float quality=Quality(power,window),pressure=Mathf.InverseLerp(BasketballBall.SweetWindow,.025f,window);
   return Mathf.Lerp(floor,best,quality*quality)*Mathf.Lerp(1,.75f,pressure);
  }
 }
 public sealed partial class BasketballBall {
  public float LastShotChance {get;private set;}
  public uint ShotRolls {get;private set;}
  readonly System.Random shotRandom=new();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  // Test injection is absent from the release build and never received by RPC.
  public System.Func<float> ReviewShotRoll;
#endif
  float RollShot(){
   ShotRolls++;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(ReviewShotRoll!=null)return Mathf.Clamp01(ReviewShotRoll());
#endif
   return (float)shotRandom.NextDouble();
  }
  bool ShotVelocity(Athlete athlete,Vector3 start,Transform hoop,out Vector3 velocity){
   float distance=Vector3.ProjectOnPlane(hoop.position-athlete.transform.position,Vector3.up).magnitude;
   LastShotChance=BasketballShotOdds.Chance(distance,pendingPower,pendingWindow);
   bool make=RollShot()<LastShotChance;
   float variation=(float)shotRandom.NextDouble();
   var goal=hoop.TransformPoint(new Vector3(0,BasketballHoop.RimHeight,0));
   if(!make){
    // Select a physical miss at release. The ball never changes course in flight,
    // and a later deflection can still score if it really crosses the hoop.
    float quality=BasketballShotOdds.Quality(pendingPower,pendingWindow);
    float side=variation<.5f?-1:1;
    goal+=hoop.right*(side*Mathf.Lerp(.85f,.48f,quality));
    goal+=hoop.forward*(Mathf.Sign(BasketballBall.SweetSpot-pendingPower)*Mathf.Lerp(1.7f,.12f,quality));
   }
   // Longer heaves need a steeper descent so a selected make clears the
   // finite-radius ball over the front rim instead of grazing it every time.
   float clearance=arcHeight+distance*arcPerMetre+Mathf.Max(0,distance-13)*.12f;
   return SolveShot(start,goal,clearance,maxShotSpeed,out velocity);
  }
 }
}
