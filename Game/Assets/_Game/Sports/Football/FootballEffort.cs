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
  public const float PlayerPace=1.5f*.7f,BallPace=1.4f,DashMultiplier=1.45f;
  public const float Drain=32,Recovery=15,RecoveryDelay=.6f,RestartStamina=25;
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
   Dashing=eligible&&moving&&dash&&!Exhausted&&Stamina>0;
   if(Dashing){Stamina=Mathf.Max(0,Stamina-Drain*dt);recoveryWait=RecoveryDelay;if(Stamina<=0){Dashing=false;Exhausted=true;released=false;}}
   else {float resting=Mathf.Max(0,dt-recoveryWait);recoveryWait=Mathf.Max(0,recoveryWait-dt);Stamina=Mathf.Min(100,Stamina+Recovery*resting);}
  }
 }
 public static class FootballPressure {
  public const float Reach=2.4f,FollowReach=8,StandOff=1.35f,DefenderMultiplier=.7f,CarrierMultiplier=.75f;
  public static bool Eligible(Athlete a)=>!(FootballBall.Instance&&FootballBall.Instance.PropLocked(a))&&a&&a.isActiveAndEnabled&&!a.inTransit&&a.Action==FootballAction.None&&!a.Airborne&&!a.LoadingJump&&a.WhiffRemaining<=0&&FootballBall.Allowed;
  // A captured pressure gesture survives the defender's own jump or tackle.
  // The active target still requires a live opposing carrier and clear sight.
  public static bool CanHold(Athlete a)=>a&&a.isActiveAndEnabled&&!a.inTransit&&a.Action!=FootballAction.Hit&&!(FootballBall.Instance&&FootballBall.Instance.PropLocked(a))&&FootballBall.Allowed;
  public static bool Opponents(Athlete a,Athlete b){
   if(!a||!b||a==b)return false;var match=FootballMatch.Instance;
   if(!match||match.Snapshot.phase==FootballMatchPhase.Idle)return true;
   var first=match.TeamOf(a);var second=match.TeamOf(b);return first!=FootballTeam.None&&second!=FootballTeam.None&&first!=second;
  }
  public static Athlete CarrierFor(Athlete defender){var ball=FootballBall.Instance;var carrier=ball?ball.CurrentController:null;return CanHold(defender)&&!defender.Charging&&Opponents(defender,carrier)&&Eligible(carrier)&&(carrier.transform.position-defender.transform.position).sqrMagnitude<=FollowReach*FollowReach&&(defender.Airborne||defender.LoadingJump?!Physics.Linecast(defender.transform.position+Vector3.up*.6f,carrier.transform.position+Vector3.up*.6f,1<<8,QueryTriggerInteraction.Ignore):Visible(defender,carrier))?carrier:null;}
  public static bool Visible(Athlete a,Athlete b)=>Mathf.Abs(a.transform.position.y-b.transform.position.y)<.7f&&!Physics.Linecast(a.transform.position+Vector3.up*.6f,b.transform.position+Vector3.up*.6f,1<<8,QueryTriggerInteraction.Ignore);
  public static float Strength(Vector3 defender,Vector3 facing,Vector3 carrier){
   var offset=Vector3.ProjectOnPlane(carrier-defender,Vector3.up);float distance=offset.magnitude;
   if(distance<.01f||distance>=Reach||Mathf.Abs(carrier.y-defender.y)>.7f)return 0;
   return Vector3.Dot(facing,offset/distance)>.35f?1:0;
  }
  public static Vector3 Follow(Athlete defender,Athlete carrier,Vector3 input,out float heading){
   var toward=Vector3.ProjectOnPlane(carrier.transform.position-defender.transform.position,Vector3.up);
   heading=toward.sqrMagnitude>.0001f?Mathf.Atan2(toward.x,toward.z)*Mathf.Rad2Deg:defender.transform.eulerAngles.y;
   var side=-toward.normalized;var match=FootballMatch.Instance;var ball=FootballBall.Instance;
   var team=match?match.TeamOf(defender):FootballTeam.None;
   if(team!=FootballTeam.None&&ball&&ball.Pitch){
    var ownGoal=ball.Pitch.TransformPoint(new Vector3(ball.PitchBounds.center.x,ball.PitchBounds.max.y,team==FootballTeam.A?ball.PitchBounds.min.z:ball.PitchBounds.max.z));
    side=Vector3.ProjectOnPlane(ownGoal-carrier.transform.position,Vector3.up).normalized;
   }
   if(side.sqrMagnitude<.01f)side=-carrier.transform.forward;
   var radial=-toward.normalized;float angle=Vector3.SignedAngle(radial,side,Vector3.up);
   if(toward.magnitude<StandOff+1.2f&&Mathf.Abs(angle)>55)side=Quaternion.AngleAxis(Mathf.Clamp(angle,-55,55),Vector3.up)*radial;
   // Follow the goal-side shoulder. Stick input shifts the marking lane without
   // removing the automatic chase or changing the carrier-facing stance.
   var target=carrier.transform.position+side*StandOff+Vector3.ClampMagnitude(input,1)*.9f;
   return Vector3.ClampMagnitude(Vector3.ProjectOnPlane(target-defender.transform.position,Vector3.up)/.8f,1);
  }
  public static Vector3 Constrain(Athlete carrier,Vector3 displacement){
   if(!carrier.ControlsFootball||!Eligible(carrier))return displacement;
   foreach(var defender in Athlete.Active){
    if(!defender.FootballEffort.Pressuring||CarrierFor(defender)!=carrier||Strength(defender.transform.position,defender.transform.forward,carrier.transform.position)==0)continue;
    // Applied after the motor: dash, Turbo and existing momentum all receive the
    // same penalty. Return once so multiple defenders never stack the slowdown.
    return new Vector3(displacement.x*CarrierMultiplier,displacement.y,displacement.z*CarrierMultiplier);
   }
   return displacement;
  }
 }
}
