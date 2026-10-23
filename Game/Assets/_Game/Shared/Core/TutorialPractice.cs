using System;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public sealed partial class BasketballBall {
  internal void PrepareTutorial(Athlete actor,bool possession,Vector3 loose){
   if(!TutorialSession.Is(SportId.Basketball)||!TutorialSession.Owns(actor)||!Authority)return;
   ResetMatchBall(false);propState.pickup=new SportsPropSpawn{respawn=double.MaxValue};
   if(possession){ClearPossession();holder=actor;Body.isKinematic=true;Body.detectCollisions=false;NewDefensePlay();Follow(actor);}
   else {Body.isKinematic=false;Body.detectCollisions=true;Body.position=loose;transform.position=loose;Body.linearVelocity=Vector3.zero;Body.angularVelocity=Vector3.zero;pickupAt=Time.time+.2f;}
  }
  internal Vector3 TutorialProp(Athlete actor,bool inventory){
   if(!TutorialSession.Is(SportId.Basketball)||!TutorialSession.Owns(actor))return Vector3.zero;
   if(inventory)GiveProp(actor,SportsProp.Turbo);else propState.pickup=new SportsPropSpawn{kind=SportsProp.Turbo,anchor=0};
   return transform.parent.TransformPoint(BasketballPropRules.Anchor(0));
  }
 }
 public sealed partial class FootballBall {
  internal void PrepareTutorial(Athlete actor,bool possession,Vector3 loose){
   if(!TutorialSession.Is(SportId.Football)||!TutorialSession.Owns(actor)||!HasAuthority)return;
   ResetBall();footballProps.live=true;footballProps.pickup=new SportsPropSpawn{respawn=double.MaxValue};
   Body.position=possession?actor.transform.position+Vector3.up*WorldRadius:loose;transform.position=Body.position;Body.linearVelocity=Vector3.zero;Body.angularVelocity=Vector3.zero;propPickupDelay=0;mustApproach.Clear();
   if(possession){controller=actor;Physics.IgnoreCollision(sphere,actor.capsule,true);SetPhysicsMode(false);FollowController(actor);}
  }
  internal Vector3 TutorialProp(Athlete actor,bool inventory){
   if(!TutorialSession.Is(SportId.Football)||!TutorialSession.Owns(actor))return Vector3.zero;
   if(inventory)GiveFootballProp(actor,SportsProp.Turbo);else footballProps.pickup=new SportsPropSpawn{kind=SportsProp.Turbo,anchor=0};return PropAnchor(0);
  }
 }
 public sealed partial class FishingGame {
  // Resume/retry gives a fresh practice fish. Cast, Hook, reel, ability and
  // landing still use the ordinary state machine and input handlers.
  internal void PrepareTutorial(Athlete actor,string step){
   if(!TutorialSession.Is(SportId.Fishing)||!TutorialSession.Owns(actor)||!Authority)return;
   var previous=Player(actor);ResetOfflinePractice();int index=State.Players.FindIndex(p=>p.owner==Key(actor));if(index<0)return;
   var p=State.Players[index];bool picked=step!="skillChoice"&&step!="pier";
   p.skillPicked=picked;p.item=previous.HasValue&&previous.Value.skillPicked?previous.Value.item:FishingAbility.SteelWire;State.Players[index]=p;
   if(step is "forward" or "backward" or "sideways" or "look" or "jump" or "pier" or "skillChoice" or "cast" or "cancelCast"||step=="fishSkill"&&FishingAbilityRules.BeforeCast(p.item))return;
   int fish=State.Fish.Where(f=>SizeForTutorial(f.id)&&CanCast(actor,f.id)).Select(f=>f.id).DefaultIfEmpty(-1).First();
   if(fish<0)return;
   Execute(actor,FishingAction.Cast,State.Match.round,p.sequence,fish);p=State.Players[index];
   p.phase=FishingPhase.Bite;p.biteAt=Now;p.hookUntil=Now+8;p.phaseAt=Now;State.Players[index]=p;
   if(step=="hook")return;
   Execute(actor,FishingAction.Hook,State.Match.round,p.sequence,fish,Now);p=State.Players[index];
   p.tension=step=="cool"?.55f:.2f;p.progress=0;State.Players[index]=p;
  }
  static bool SizeForTutorial(int fish)=>FishingState.SizeForFish(fish)==0;
 }
}
