using System;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public struct FootballEffortSnapshot:INetworkSerializable,IEquatable<FootballEffortSnapshot> {
  public byte stamina;public bool dashing,pressuring,exhausted;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref stamina);s.SerializeValue(ref dashing);s.SerializeValue(ref pressuring);s.SerializeValue(ref exhausted);}
  public bool Equals(FootballEffortSnapshot v)=>stamina==v.stamina&&dashing==v.dashing&&pressuring==v.pressuring&&exhausted==v.exhausted;
 }
 // Advanced only by Athlete's authoritative, bounded simulation steps.
 public sealed class FootballEffort {
  public const float PlayerPace=1.5f,BallPace=1.4f,DashMultiplier=1.45f;
  public const float Drain=32,Recovery=15,RecoveryDelay=.6f,RestartStamina=25,PressureSpeed=6.6f;
  public float Stamina {get;private set;}=100;
  public bool Dashing {get;private set;}
  public bool Pressuring {get;private set;}
  public bool Exhausted {get;private set;}
  float recoveryWait;bool released=true;
  public FootballEffortSnapshot Snapshot=>new(){stamina=(byte)Mathf.RoundToInt(Stamina),dashing=Dashing,pressuring=Pressuring,exhausted=Exhausted};
  public void Receive(FootballEffortSnapshot state){Stamina=state.stamina;Dashing=state.dashing;Pressuring=state.pressuring;Exhausted=state.exhausted;}
  public void Reset(){Stamina=100;Dashing=Pressuring=Exhausted=false;recoveryWait=0;released=true;}
  public void Stop(){Dashing=Pressuring=false;}
  public void Step(bool dash,bool moving,bool eligible,bool pressure,float dt){
   if(!float.IsFinite(dt)||dt<=0)return;
   if(!dash)released=true;
   if(Exhausted&&released&&Stamina>=RestartStamina)Exhausted=false;
   Pressuring=eligible&&pressure;
   Dashing=eligible&&moving&&dash&&!Pressuring&&!Exhausted&&Stamina>0;
   if(Dashing){Stamina=Mathf.Max(0,Stamina-Drain*dt);recoveryWait=RecoveryDelay;if(Stamina<=0){Dashing=false;Exhausted=true;released=false;}}
   else {float resting=Mathf.Max(0,dt-recoveryWait);recoveryWait=Mathf.Max(0,recoveryWait-dt);Stamina=Mathf.Min(100,Stamina+Recovery*resting);}
  }
 }
 public static class FootballPressure {
  public const float Reach=2.4f,Containment=.6f;
  public static bool Eligible(Athlete a)=>a&&a.isActiveAndEnabled&&!a.inTransit&&a.Action==FootballAction.None&&!a.Airborne&&!a.LoadingJump&&a.WhiffRemaining<=0&&FootballBall.Allowed;
  public static bool Opponents(Athlete a,Athlete b){
   if(!a||!b||a==b)return false;var match=FootballMatch.Instance;
   if(!match||match.Snapshot.phase==FootballMatchPhase.Idle)return true;
   var first=match.TeamOf(a);var second=match.TeamOf(b);return first!=FootballTeam.None&&second!=FootballTeam.None&&first!=second;
  }
  public static Athlete CarrierFor(Athlete defender){var ball=FootballBall.Instance;var carrier=ball?ball.CurrentController:null;return Eligible(defender)&&!defender.Charging&&Opponents(defender,carrier)&&Eligible(carrier)?carrier:null;}
  public static bool Visible(Athlete a,Athlete b)=>Mathf.Abs(a.transform.position.y-b.transform.position.y)<.7f&&!Physics.Linecast(a.transform.position+Vector3.up*.6f,b.transform.position+Vector3.up*.6f,1<<8,QueryTriggerInteraction.Ignore);
  public static float Strength(Vector3 defender,Vector3 facing,Vector3 carrier){
   var offset=Vector3.ProjectOnPlane(carrier-defender,Vector3.up);float distance=offset.magnitude;
   if(distance<.01f||distance>=Reach||Mathf.Abs(carrier.y-defender.y)>.7f)return 0;
   return Mathf.InverseLerp(Reach,.8f,distance)*Mathf.InverseLerp(.35f,.85f,Vector3.Dot(facing,offset/distance));
  }
  public static Vector3 Constrain(Athlete carrier,Vector3 displacement){
   if(!carrier.ControlsFootball||!Eligible(carrier))return displacement;
   float strongest=0;Vector3 normal=Vector3.zero;
   foreach(var defender in Athlete.Active){
    if(!defender.FootballEffort.Pressuring||CarrierFor(defender)!=carrier||!Visible(defender,carrier))continue;
    var toward=Vector3.ProjectOnPlane(defender.transform.position-carrier.transform.position,Vector3.up).normalized;
    float forward=Mathf.Max(0,Vector3.Dot(displacement,toward));
    float amount=forward*Strength(defender.transform.position,defender.transform.forward,carrier.transform.position)*Containment;
    if(amount>strongest){strongest=amount;normal=toward;}
   }
   // Remove only the component into the strongest guard. Sideways escape and
   // retreat stay available; extra defenders cannot multiply a slow to zero.
   return displacement-normal*strongest;
  }
 }
}
