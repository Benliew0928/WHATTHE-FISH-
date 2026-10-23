using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 // Retired IDs remain reserved so malformed/old actions cannot become another skill.
 public enum FishingAbility:byte {None=0,Sonar=1,CoolSpool=2,Flippers=3,PowerReel=7,BubbleGuard=8,QuickLure=9,SteelWire=10,FishingBuddy=11,SchoolCall=12}
 public static class FishingAbilityRules {
  public const double DraftSeconds=12;
  public static readonly FishingAbility[] Roster={FishingAbility.SteelWire,FishingAbility.PowerReel,FishingAbility.FishingBuddy,FishingAbility.BubbleGuard,FishingAbility.Sonar,FishingAbility.SchoolCall};
  public static bool Valid(FishingAbility a)=>Array.IndexOf(Roster,a)>=0;
  public static bool BeforeCast(FishingAbility a)=>a is FishingAbility.Sonar or FishingAbility.SchoolCall;
  public static bool PhaseAllows(FishingAbility a,FishingPhase phase)=>Valid(a)&&(BeforeCast(a)?phase==FishingPhase.Ready:phase==FishingPhase.Reeling);
  public static bool CanUse(FishingPlayerRecord p,double now)=>p.connected&&Valid(p.item)&&now>=p.cooldownUntil&&now>=p.activeUntil&&PhaseAllows(p.item,p.phase);
  public static double Duration(FishingAbility a)=>a switch {FishingAbility.SteelWire=>7,FishingAbility.PowerReel or FishingAbility.FishingBuddy=>8,FishingAbility.Sonar=>16,FishingAbility.SchoolCall=>15,_=>12};
  public const float SchoolReleaseSeconds=2;
  public static double Cooldown(FishingAbility a)=>a switch {FishingAbility.SteelWire=>20,FishingAbility.PowerReel=>18,FishingAbility.FishingBuddy=>22,FishingAbility.BubbleGuard=>16,_=>18};
  public static string Name(FishingAbility a)=>a switch {FishingAbility.Sonar=>"SONAR",FishingAbility.SteelWire=>"STEEL WIRE",FishingAbility.FishingBuddy=>"FISHING BUDDY",FishingAbility.PowerReel=>"POWER REEL",FishingAbility.BubbleGuard=>"BUBBLE GUARD",FishingAbility.SchoolCall=>"SCHOOL CALL",_=>"SKILL"};
  public static string Description(FishingAbility a)=>a switch {FishingAbility.Sonar=>$"Reveal Wanted fish for {Duration(a)}s. Shared in Crew.",FishingAbility.SteelWire=>$"{Duration(a)}s protection: tension cannot rise.",FishingAbility.FishingBuddy=>$"Reel 45% faster with 40% less strain for {Duration(a)}s.",FishingAbility.PowerReel=>$"Reel 55% faster for {Duration(a)}s. Normal tension.",FishingAbility.BubbleGuard=>$"Absorb one strong surge within {Duration(a)}s.",FishingAbility.SchoolCall=>$"Bring 5 free fish within easy casting range for {Duration(a)}s.",_=>"Choose one skill for this round."};
  public static Color Color(FishingAbility a)=>a switch {FishingAbility.SteelWire=>LocalProfile.Hex("C1F4FF"),FishingAbility.FishingBuddy=>LocalProfile.Hex("DDD9EB"),FishingAbility.PowerReel=>LocalProfile.Hex("FFCB53"),FishingAbility.BubbleGuard=>LocalProfile.Hex("69C8F4"),FishingAbility.SchoolCall=>LocalProfile.Hex("FFAF68"),_=>LocalProfile.Hex("4CDED6")};
 }
 public sealed partial class FishingState {
  public bool Offered(FishingAbility skill)=>FishingAbilityRules.Valid(skill)&&(Match.offer0==skill||Match.offer1==skill||Match.offer2==skill);
  public FishingAbility Offer(int index)=>index==0?Match.offer0:index==1?Match.offer1:Match.offer2;
  void DealSkills(uint seed){
   // A shared server-owned menu with both before-cast and fight choices.
   uint Mix(uint n){unchecked{n^=n>>16;n*=0x7feb352d;n^=n>>15;return n*0x846ca68b;}}
   var before=new[]{FishingAbility.Sonar,FishingAbility.SchoolCall};
   var fight=FishingAbilityRules.Roster.Where(a=>!FishingAbilityRules.BeforeCast(a)).ToArray();
   Match.offer0=before[Mix(seed)%2];Match.offer1=fight[Mix(seed^0x68bc21eb)%4];
   var remaining=FishingAbilityRules.Roster.Where(a=>a!=Match.offer0&&a!=Match.offer1).ToArray();Match.offer2=remaining[Mix(seed^0x9e3779b9)%4];
   int rotate=(int)(Mix(seed^0x85ebca6b)%3);while(rotate-->0){var a=Match.offer0;Match.offer0=Match.offer1;Match.offer1=Match.offer2;Match.offer2=a;}
  }
  public bool AbilityAct(ulong owner,uint round,uint sequence,FishingAction action,int choice,int pier,Vector3 origin,double now,IEnumerable<ulong> nearby=null,Vector3 schoolPoint=default,uint schoolMask=0){
   if(!double.IsFinite(now)||!float.IsFinite(origin.x)||!float.IsFinite(origin.y)||!float.IsFinite(origin.z))return false;
   Advance(now,0);int index=Players.FindIndex(p=>p.owner==owner);if(index<0||round!=Match.round)return false;var p=Players[index];if(!p.connected||p.sequence!=sequence)return false;
   if(action==FishingAction.ChooseSkill){
    if(p.skillPicked||p.Busy||Match.phase!=FishingRoundPhase.Drafting&&Match.phase!=FishingRoundPhase.Practice||choice<0||choice>byte.MaxValue||!Offered((FishingAbility)choice))return false;
    p.item=(FishingAbility)choice;p.skillPicked=true;p.cooldownUntil=0;Players[index]=p;return true;
   }
   // Bags, treasure, favourites and one-use inventory actions are retired.
   if(action!=FishingAction.UseAbility||!Running||!FishingAbilityRules.CanUse(p,now))return false;
   var ability=p.item;
   if(ability==FishingAbility.SchoolCall&&(schoolMask==0||!float.IsFinite(schoolPoint.x)||!float.IsFinite(schoolPoint.y)||!float.IsFinite(schoolPoint.z)))return false;
   p.active=ability;p.itemSequence++;p.activeAt=now;p.activeUntil=now+FishingAbilityRules.Duration(ability);p.cooldownUntil=now+FishingAbilityRules.Cooldown(ability);
   p.steelTension=p.tension;p.bubbleGuard=ability==FishingAbility.BubbleGuard;
   if(ability==FishingAbility.SchoolCall){p.schoolAt=now;p.schoolPoint=schoolPoint;p.schoolFishMask=schoolMask;}
   Players[index]=p;
   if(ability==FishingAbility.Sonar){for(int i=0;i<Players.Count;i++){var q=Players[i];if(q.connected&&(q.owner==owner||Match.mode==FishingMode.Crew)){q.sonarUntil=Math.Max(q.sonarUntil,now+FishingAbilityRules.Duration(ability));Players[i]=q;}}}
   return true;
  }
  static void FightMotion(ref FishingPlayerRecord p,float dt,float mass){
   // Semi-implicit integration: thrust, tether restoring force, water drag,
   // neutral buoyancy below the surface and gravity during a breach.
   int steps=Mathf.Max(1,Mathf.CeilToInt(dt/.01f));float h=dt/steps,size=SizeForFish(p.fish);
   for(int i=0;i<steps;i++){
    float span=1.35f+.45f*size,thrust=p.pull*(3.2f+.7f*size+p.surge*4.8f);
    float stretch=Mathf.Max(0,p.distance-p.lineLength);
    float lateralForce=thrust+(p.held?p.steer*1.8f:0)-p.fightOffset.x*(1.8f+2.4f*p.progress)-9*stretch*p.fightOffset.x/Mathf.Max(1,p.distance);
    p.fightMotion.x=(p.fightMotion.x+lateralForce/mass*h)/(1+2.5f/mass*h);
    p.fightOffset.x+=p.fightMotion.x*h;
    bool wet=p.fightOffset.y>0;float neutral=.54f+.10f*size;
    float verticalForce=wet?(neutral-p.fightOffset.y)*5-p.surge*(3.8f+1.4f*size):mass*9.81f;
    p.fightMotion.y=(p.fightMotion.y+verticalForce/mass*h)/(1+(wet?2.2f:.08f)/mass*h);
    p.fightOffset.y+=p.fightMotion.y*h;
    // A safety envelope is well outside the normal trajectory; no per-frame teleporting.
    if(Mathf.Abs(p.fightOffset.x)>span*3){p.fightOffset.x=Mathf.Sign(p.fightOffset.x)*span*3;p.fightMotion.x=0;}
   }
  }
 }
}
