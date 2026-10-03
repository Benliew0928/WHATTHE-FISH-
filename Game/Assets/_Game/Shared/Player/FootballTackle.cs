using System.Collections.Generic;
using UnityEngine;

namespace WhatTheFish {
 public enum FootballAction:byte { None, Slide, Hit }

 // Simulation is called by Athlete, on the server for network players.
 public sealed class FootballTackle:MonoBehaviour {
  public const float SlidePlayback=2f,SlideDuration=.85f/SlidePlayback,HitDuration=.8f,HitPlayback=.45f/HitDuration,SlideTravel=.55f/SlidePlayback,HitTravel=.24f;
  public const float WhiffSpeedMultiplier=.4f;
  [Min(0)] public float cooldown=4,whiffRecovery=.8f;
  public FootballAction State {get;private set;}
  public float Elapsed {get;private set;}
  public float CooldownRemaining {get;private set;}
  public float RecoveryRemaining {get;private set;}
  public float MovementMultiplier {get;private set;}=1;
  public uint Sequence {get;private set;}
  public byte HitVariant {get;private set;}
  public int HitsDealt {get;private set;}
  public int HitsReceived {get;private set;}
  Vector3 direction;float immunity;Athlete athlete;bool ballHit;
  public Vector3 Direction=>direction;
  public bool ClaimBallContact(){if(State!=FootballAction.Slide||Elapsed>SlideTravel||ballHit)return false;ballHit=true;return true;}
  Collider[] contacts=new Collider[32];readonly HashSet<FootballTackle> hitThisSlide=new();
  public static bool Allowed=>EnvironmentAllowed&&!FootballMatch.BlocksActions;
  public static bool EnvironmentAllowed=>AppRoot.Instance&&AppRoot.Instance.Exploring&&AppRoot.Instance.SelectedSport==SportId.Football;
  public static float Duration(FootballAction action)=>action==FootballAction.Slide?SlideDuration:HitDuration;
  void Awake(){athlete=GetComponent<Athlete>();}
  public void ResetAction(){State=FootballAction.None;Elapsed=0;CooldownRemaining=0;RecoveryRemaining=0;MovementMultiplier=1;immunity=0;ballHit=false;hitThisSlide.Clear();Sequence++;}
  public bool TryStart(bool grounded){
   if(!Allowed||athlete.ControlsFootball||!grounded||State!=FootballAction.None||RecoveryRemaining>0||CooldownRemaining>0)return false;
   direction=transform.forward;direction.y=0;direction.Normalize();
   State=FootballAction.Slide;Elapsed=0;CooldownRemaining=cooldown;Sequence++;ballHit=false;hitThisSlide.Clear();
   athlete.Motor.Reset(transform.eulerAngles.y);return true;
  }
  public bool ReceiveHit(Vector3 push,Athlete tackler=null){
   if(!Allowed||athlete.inTransit||athlete.Airborne||immunity>0)return false;
   direction=push;direction.y=0;direction.Normalize();
   HitVariant=(byte)FootballMotion.FallVariant(direction,transform.rotation);
   State=FootballAction.Hit;Elapsed=0;immunity=HitDuration;CooldownRemaining=Mathf.Max(CooldownRemaining,HitDuration);Sequence++;HitsReceived++;
   RecoveryRemaining=0;MovementMultiplier=1;
   athlete.Motor.Reset(transform.eulerAngles.y);FootballBall.Instance?.ReleaseFromTackle(athlete,tackler,direction);return true;
  }
  // Integral of a smooth speed falloff. Identical slide distance at any frame rate.
  public static float TravelAt(FootballAction action,float time){
   float duration=action==FootballAction.Slide?SlideTravel:HitTravel;
   float u=Mathf.Clamp01(time/duration);
   return action==FootballAction.Slide?14*SlidePlayback*duration*(u-.8f*(u*u*u-.5f*u*u*u*u)):9*duration*(u-u*u+.333333333f*u*u*u);
  }
  public Vector3 Step(float dt,out float normalTime,out bool slideContact){
   normalTime=dt;slideContact=false;MovementMultiplier=1;CooldownRemaining=Mathf.Max(0,CooldownRemaining-dt);immunity=Mathf.Max(0,immunity-dt);
   if(!Allowed){if(State!=FootballAction.None||RecoveryRemaining>0)ResetAction();return Vector3.zero;}
   if(State==FootballAction.None){StepRecovery(dt);return Vector3.zero;}
   float travel=State==FootballAction.Slide?SlideTravel:HitTravel;
   float used=Mathf.Clamp(travel-Elapsed,0,dt);normalTime=Mathf.Max(0,dt-Mathf.Clamp(Duration(State)-Elapsed,0,dt));
   var delta=direction*(TravelAt(State,Elapsed+dt)-TravelAt(State,Elapsed));
   slideContact=State==FootballAction.Slide&&used>0;
   Elapsed+=dt;
   if(Elapsed>=Duration(State)){
    if(State==FootballAction.Slide&&!ballHit&&hitThisSlide.Count==0)RecoveryRemaining=whiffRecovery;
    State=FootballAction.None;Elapsed=0;Sequence++;
    StepRecovery(normalTime);
   }
   return delta;
  }
  void StepRecovery(float dt){
   // Weight the final partial recovery step so its 0.8-second budget is frame-rate independent.
   float slowed=Mathf.Min(RecoveryRemaining,dt);
   MovementMultiplier=dt>0?1-(1-WhiffSpeedMultiplier)*slowed/dt:1;
   RecoveryRemaining=Mathf.Max(0,RecoveryRemaining-dt);
  }
  public void ResolveContacts(Vector3 before,Vector3 after){
   if(!Allowed||State!=FootballAction.Slide)return;
   // Sweep actual travel, including initial overlap, so fast slides cannot skip
   // an opponent. World obstruction is checked separately before applying a hit.
   if(State!=FootballAction.Slide)return;
   int count;
   // Grow only when crowded: a full non-alloc buffer may have omitted players.
   // There is no per-slide victim limit, and the sweep covers actual travel only.
   while((count=Physics.OverlapCapsuleNonAlloc(before+Vector3.up*.5f,after+Vector3.up*.5f,.48f,contacts,~0,QueryTriggerInteraction.Ignore))==contacts.Length)
    System.Array.Resize(ref contacts,contacts.Length*2);
   Vector3 slideDirection=direction;
   for(int i=0;i<count;i++){
    var opponent=contacts[i].GetComponentInParent<FootballTackle>();
    if(!opponent||opponent==this||hitThisSlide.Contains(opponent)||!opponent.gameObject.activeInHierarchy)continue;
    if(Physics.Linecast(before+Vector3.up*.55f,opponent.transform.position+Vector3.up*.55f,1<<8,QueryTriggerInteraction.Ignore))continue;
    bool mutual=opponent.State==FootballAction.Slide;Vector3 otherDirection=opponent.Direction;
    if(!opponent.ReceiveHit(slideDirection,athlete))continue;
    hitThisSlide.Add(opponent);HitsDealt++;
    if(mutual){
     // Resolve both hits now, before simulation order can erase the other slide.
     if(ReceiveHit(otherDirection,opponent.athlete))opponent.HitsDealt++;
     break;
    }
   }
  }
 }
}
