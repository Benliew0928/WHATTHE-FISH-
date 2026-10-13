using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public enum FishingAbility:byte {None=0,Sonar=1,CoolSpool=2,Flippers=3,PowerReel=7,BubbleGuard=8,QuickLure=9}
 public static class FishingAbilityRules {
  public const int FavouriteCount=2;
  public static readonly FishingAbility[] Roster={FishingAbility.Sonar,FishingAbility.CoolSpool,FishingAbility.Flippers,FishingAbility.PowerReel,FishingAbility.BubbleGuard,FishingAbility.QuickLure};
  public const ushort AllowedMask=(1<<1)|(1<<2)|(1<<3)|(1<<7)|(1<<8)|(1<<9),DefaultFavourites=(1<<1)|(1<<9);
  public const double SupplySeconds=22,SalvageSeconds=6,SalvageRespawn=28;
  public static bool Valid(FishingAbility a)=>Array.IndexOf(Roster,a)>=0;
  public static bool ValidFavourites(int mask)=>mask>0&&(mask&~AllowedMask)==0&&Roster.Count(a=>(mask&(1<<(int)a))!=0)==FavouriteCount;
  public static string Name(FishingAbility a)=>a switch {FishingAbility.Sonar=>"SONAR",FishingAbility.CoolSpool=>"COOL SPOOL",FishingAbility.Flippers=>"FLIPPERS",FishingAbility.PowerReel=>"POWER REEL",FishingAbility.BubbleGuard=>"BUBBLE GUARD",FishingAbility.QuickLure=>"QUICK LURE",_=>"GEAR"};
  public static string Description(FishingAbility a)=>a switch {FishingAbility.Sonar=>"Mark crew's Wanted fish: 8s",FishingAbility.CoolSpool=>"Less strain for 4s",FishingAbility.Flippers=>"Move 30% faster for 5s",FishingAbility.PowerReel=>"Faster reel, more strain for 4s",FishingAbility.BubbleGuard=>"Soften one surge within 12s",FishingAbility.QuickLure=>"Next bite 0.8s sooner; use in 12s",_=>"Carry one item at a time"};
  public static Color Color(FishingAbility a)=>a==FishingAbility.PowerReel?LocalProfile.Hex("FF7659"):a==FishingAbility.BubbleGuard||a==FishingAbility.CoolSpool?LocalProfile.Hex("69C8F4"):a==FishingAbility.QuickLure?LocalProfile.Hex("FFCB53"):LocalProfile.Hex("4CDED6");
 }
 public sealed partial class FishingState {
  public double SupplyReady(int pier)=>pier switch {0=>Match.supply0,1=>Match.supply1,2=>Match.supply2,3=>Match.supply3,4=>Match.supply4,_=>double.PositiveInfinity};
  void CoolSupply(int pier,double until){switch(pier){case 0:Match.supply0=until;break;case 1:Match.supply1=until;break;case 2:Match.supply2=until;break;case 3:Match.supply3=until;break;case 4:Match.supply4=until;break;}}
  // Nearby IDs come only from the host's spatial query, never from an RPC payload.
  public bool AbilityAct(ulong owner,uint round,uint sequence,FishingAction action,int choice,int pier,Vector3 origin,double now,IEnumerable<ulong> nearby=null){
   if(!double.IsFinite(now)||!float.IsFinite(origin.x)||!float.IsFinite(origin.y)||!float.IsFinite(origin.z))return false;
   Advance(now,0);int index=Players.FindIndex(p=>p.owner==owner);if(index<0||round!=Match.round)return false;var p=Players[index];if(!p.connected||p.sequence!=sequence)return false;
   if(action==FishingAction.Favourites){if(p.Busy||p.ready||Match.phase!=FishingRoundPhase.Practice&&Match.phase!=FishingRoundPhase.Ended||!FishingAbilityRules.ValidFavourites(choice))return false;p.favourites=(ushort)choice;Players[index]=p;return true;}
   if(!Running)return false;
   var ability=(FishingAbility)choice;
   if(action==FishingAction.Supply||action==FishingAction.Salvage){
    if(p.phase!=FishingPhase.Ready||p.item!=FishingAbility.None||p.activeUntil>now||pier<0||pier>=PierCount||choice<1||choice>9||!FishingAbilityRules.Valid(ability)||(p.favourites&(1<<choice))==0)return false;
    if(action==FishingAction.Supply){if(now<SupplyReady(pier))return false;CoolSupply(pier,now+FishingAbilityRules.SupplySeconds);p.item=ability;p.itemSequence++;}
    else {if(now<Match.salvageReadyAt)return false;Match.salvageReadyAt=now+FishingAbilityRules.SalvageRespawn;p.salvageItem=ability;p.phase=FishingPhase.Salvaging;p.phaseAt=now;p.origin=origin;p.pier=pier;p.fish=-1;p.sequence++;p.ready=false;}
    Players[index]=p;return true;
   }
   if(action!=FishingAction.UseAbility||p.item==FishingAbility.None||p.activeUntil>now||p.phase==FishingPhase.Salvaging)return false;
   ability=p.item;
   if((ability==FishingAbility.CoolSpool||ability==FishingAbility.PowerReel||ability==FishingAbility.BubbleGuard)&&p.phase!=FishingPhase.Reeling)return false;
   if((ability==FishingAbility.Flippers||ability==FishingAbility.QuickLure)&&p.Busy)return false;
   p.item=FishingAbility.None;p.active=ability;p.itemSequence++;p.activeAt=now;p.activeUntil=now+(ability==FishingAbility.Sonar?8:ability==FishingAbility.CoolSpool||ability==FishingAbility.PowerReel?4:ability==FishingAbility.BubbleGuard?12:ability==FishingAbility.QuickLure?12:5);
   if(ability==FishingAbility.QuickLure)p.quickLure=true;
   if(ability==FishingAbility.BubbleGuard)p.bubbleGuard=true;
   Players[index]=p;
   var ids=new HashSet<ulong>(nearby??Array.Empty<ulong>()){owner};
   for(int i=0;i<Players.Count;i++){var q=Players[i];if(!q.connected||!ids.Contains(q.owner))continue;
    if(ability==FishingAbility.Sonar)q.sonarUntil=Math.Max(q.sonarUntil,now+8);
    if(ability==FishingAbility.Flippers&&q.owner==owner)q.speedUntil=Math.Max(q.speedUntil,now+5);
    Players[i]=q;
   }
   return true;
  }
  static void FightMotion(ref FishingPlayerRecord p,float dt,float mass){
   // Semi-implicit integration: thrust, tether restoring force, water drag,
   // neutral buoyancy below the surface and gravity during a breach.
   int steps=Mathf.Max(1,Mathf.CeilToInt(dt/.01f));float h=dt/steps,size=SizeForFish(p.fish);
   for(int i=0;i<steps;i++){
    float span=1.35f+.45f*size,thrust=p.pull*(3.2f+.7f*size+p.surge*4.8f);
    float stretch=Mathf.Max(0,p.distance-p.lineLength);
    float lateralForce=thrust-p.fightOffset.x*(1.8f+2.4f*p.progress)-9*stretch*p.fightOffset.x/Mathf.Max(1,p.distance);
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
