using System.Collections.Generic;
using UnityEngine;

namespace SportsPrototype {
 public enum FootballAction:byte { None, Slide, Hit }

 // Simulation is called by Athlete, on the server for network players.
 public sealed class FootballTackle:MonoBehaviour {
  public const float SlidePlayback=2f,SlideDuration=.85f/SlidePlayback,HitDuration=.45f,SlideTravel=.55f/SlidePlayback,HitTravel=.24f,Cooldown=1.25f;
  public FootballAction State {get;private set;}
  public float Elapsed {get;private set;}
  public float CooldownRemaining {get;private set;}
  public uint Sequence {get;private set;}
  public int HitsDealt {get;private set;}
  public int HitsReceived {get;private set;}
  Vector3 direction;float immunity;Athlete athlete;
  readonly Collider[] contacts=new Collider[32];readonly HashSet<FootballTackle> hitThisSlide=new();
  public static bool Allowed=>AppRoot.Instance&&AppRoot.Instance.Exploring&&AppRoot.Instance.SelectedSport==SportId.Football;
  public static float Duration(FootballAction action)=>action==FootballAction.Slide?SlideDuration:HitDuration;
  void Awake(){athlete=GetComponent<Athlete>();}
  public void ResetAction(){State=FootballAction.None;Elapsed=0;CooldownRemaining=0;immunity=0;hitThisSlide.Clear();Sequence++;}
  public bool TryStart(bool grounded){
   if(!Allowed||!grounded||State!=FootballAction.None||CooldownRemaining>0)return false;
   direction=transform.forward;direction.y=0;direction.Normalize();
   State=FootballAction.Slide;Elapsed=0;CooldownRemaining=Cooldown;Sequence++;hitThisSlide.Clear();
   athlete.Motor.Reset(transform.eulerAngles.y);return true;
  }
  public bool ReceiveHit(Vector3 push){
   if(!Allowed||immunity>0)return false;
   direction=push;direction.y=0;direction.Normalize();
   State=FootballAction.Hit;Elapsed=0;immunity=.65f;CooldownRemaining=Mathf.Max(CooldownRemaining,.45f);Sequence++;HitsReceived++;
   athlete.Motor.Reset(transform.eulerAngles.y);return true;
  }
  // Integral of a smooth speed falloff. Identical slide distance at any frame rate.
  public static float TravelAt(FootballAction action,float time){
   float duration=action==FootballAction.Slide?SlideTravel:HitTravel;
   float u=Mathf.Clamp01(time/duration);
   return action==FootballAction.Slide?14*SlidePlayback*duration*(u-.8f*(u*u*u-.5f*u*u*u*u)):9*duration*(u-u*u+.333333333f*u*u*u);
  }
  public Vector3 Step(float dt,out float normalTime,out bool slideContact){
   normalTime=dt;slideContact=false;CooldownRemaining=Mathf.Max(0,CooldownRemaining-dt);immunity=Mathf.Max(0,immunity-dt);
   if(!Allowed){if(State!=FootballAction.None)ResetAction();return Vector3.zero;}
   if(State==FootballAction.None)return Vector3.zero;
   float travel=State==FootballAction.Slide?SlideTravel:HitTravel;
   float used=Mathf.Clamp(travel-Elapsed,0,dt);normalTime=dt-used;
   var delta=direction*(TravelAt(State,Elapsed+dt)-TravelAt(State,Elapsed));
   slideContact=State==FootballAction.Slide&&used>0;
   Elapsed+=dt;
   if(Elapsed>=Duration(State)){State=FootballAction.None;Elapsed=0;Sequence++;}
   return delta;
  }
  public void ResolveContacts(Vector3 before,Vector3 after){
   // Sweep actual travel, including initial overlap, so fast slides cannot skip
   // an opponent. World obstruction is checked separately before applying a hit.
   int count=Physics.OverlapCapsuleNonAlloc(before+Vector3.up*.5f,after+Vector3.up*.5f,.48f,contacts,~0,QueryTriggerInteraction.Ignore);
   for(int i=0;i<count;i++){
    var opponent=contacts[i].GetComponentInParent<FootballTackle>();
    if(!opponent||opponent==this||hitThisSlide.Contains(opponent)||!opponent.gameObject.activeInHierarchy)continue;
    if(Physics.Linecast(before+Vector3.up*.55f,opponent.transform.position+Vector3.up*.55f,1<<8,QueryTriggerInteraction.Ignore))continue;
    hitThisSlide.Add(opponent);
    if(opponent.ReceiveHit(direction))HitsDealt++;
   }
  }
 }
}
