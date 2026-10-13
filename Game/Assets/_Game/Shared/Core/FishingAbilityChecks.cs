#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
namespace WhatTheFish {
 public static class FishingAbilityChecks {
  public static void Run(Action<bool,string> check){
   var s=new FishingState();s.Begin(new ulong[]{0,1,2},91,0,false);
   bool A(ulong id,FishingAction action,int choice,double now,int pier=0)=>s.AbilityAct(id,s.Match.round,s.Player(id).Value.sequence,action,choice,pier,Vector3.zero,now,new ulong[]{0,1});
   check(!FishingAbilityRules.ValidFavourites(0)&&!FishingAbilityRules.ValidFavourites(3)&&!FishingAbilityRules.ValidFavourites(1023)&&FishingAbilityRules.ValidFavourites(FishingAbilityRules.DefaultFavourites),"FAVOURITES_EXACTLY_TWO_VALID_UNIQUE_ITEMS");
   check(new[]{4,5,6}.All(i=>!FishingAbilityRules.Valid((FishingAbility)i)&&!FishingAbilityRules.ValidFavourites((1<<i)|(1<<1)))&&FishingAbilityRules.Roster.Length==6,"REMOVED_ABILITIES_REJECTED_AND_SIX_ITEM_ROSTER");
   var splash=FishingSplash.PositionAt(Vector3.up*.1f,new Vector3(1,2,0),.2f);check(Mathf.Abs(splash.x-.2f)<.00001&&Mathf.Abs(splash.y-(.5f-.5f*9.81f*.04f))<.00001&&FishingSplash.PositionAt(Vector3.zero,Vector3.up*2,.6f).y<0,"DROPLETS_INHERIT_VELOCITY_AND_FALL_UNDER_GRAVITY");
   check(!A(0,FishingAction.Favourites,0,0)&&A(0,FishingAction.Favourites,(1<<1)|(1<<2),0),"HOST_VALIDATES_FAVOURITE_SELECTION");
   s.Act(0,91,0,FishingAction.Ready,0,Vector3.zero,0);
   check(!A(0,FishingAction.Favourites,FishingAbilityRules.DefaultFavourites,0),"READY_LOCKS_FAVOURITE_KIT");s.Act(0,91,0,FishingAction.Unready,0,Vector3.zero,0);
   check(!A(0,FishingAction.Supply,257,0)&&!A(0,FishingAction.Supply,-255,0)&&!A(0,FishingAction.Supply,9,0)&&A(0,FishingAction.Supply,1,0)&&!A(1,FishingAction.Supply,1,.1)&&!A(0,FishingAction.Supply,2,23),"SHARED_SUPPLY_COOLDOWN_ONE_SLOT_NO_WRAPPED_OR_UNCHOSEN_ITEMS");
   check(!s.AbilityAct(0,90,0,FishingAction.UseAbility,-1,0,Vector3.zero,0)&&!s.AbilityAct(0,91,999,FishingAction.UseAbility,-1,0,Vector3.zero,0),"STALE_ABILITY_ROUND_AND_CAST_REJECTED");
   check(A(0,FishingAction.UseAbility,-1,1)&&s.Player(0).Value.sonarUntil==9&&s.Player(1).Value.sonarUntil==9&&s.Player(2).Value.sonarUntil==0&&!A(0,FishingAction.UseAbility,-1,1),"SONAR_CONSUMED_ONCE_NEARBY_CREW_ONLY_NO_REPLAY");
   check(!A(0,FishingAction.Supply,2,5)&&A(0,FishingAction.Supply,2,23)&&!A(0,FishingAction.UseAbility,-1,23),"ACTIVE_EFFECT_BLOCKS_STACKING_COOL_SPOOL_REQUIRES_FIGHT");
   foreach(var ability in FishingAbilityRules.Roster){int i=(int)ability;
    s.Begin(new ulong[]{0,1,2},92,0,false);int mask=(1<<i)|(1<<(int)(ability==FishingAbility.Sonar?FishingAbility.QuickLure:FishingAbility.Sonar));check(A(0,FishingAction.Favourites,mask,0)&&A(0,FishingAction.Supply,i,0),"ROSTER_ITEM_COLLECTIBLE_"+i);
    if(i==2||i==7||i==8){s.Act(0,92,0,FishingAction.Cast,0,Vector3.zero,.1,2);s.Advance(3.1,0);s.Act(0,92,1,FishingAction.Hook,0,Vector3.zero,3.1);}
    check(A(0,FishingAction.UseAbility,-1,3.2)&&s.Player(0).Value.item==FishingAbility.None&&s.Player(0).Value.active==(FishingAbility)i,"ROSTER_ITEM_HOST_ACTIVATES_"+i);
    if(i==1)check(s.Player(0).Value.sonarUntil==11.2&&s.Player(1).Value.sonarUntil==11.2&&s.Player(2).Value.sonarUntil==0,"CREW_SONAR_ONLY_HOST_SPATIAL_RECIPIENTS");
    if(i==3)check(s.Player(0).Value.speedUntil==8.2&&s.Player(1).Value.speedUntil==0,"FLIPPERS_OWNER_ONLY");
    if(i==9){s.Act(0,92,0,FishingAction.Cast,0,Vector3.zero,3.3,0);check(Math.Abs(s.Player(0).Value.biteAt-4.2)<.00001&&!s.Player(0).Value.quickLure,"QUICK_LURE_SINGLE_NEXT_CAST_NO_AUTO_CATCH");}
    s.Advance(40,0);check(s.Player(0).Value.active==FishingAbility.None&&!s.Player(0).Value.quickLure&&!s.Player(0).Value.bubbleGuard,"ABILITY_EXPIRY_"+i);
   }
   s.Begin(new ulong[]{0,1},93,0,false);check(!A(0,FishingAction.Salvage,1,11)&&A(0,FishingAction.Salvage,1,12)&&!A(1,FishingAction.Salvage,1,12)&&s.Player(0).Value.Busy,"TREASURE_SHARED_RESERVATION_AND_SIX_SECOND_COST");
   s.Advance(17.99,0);check(s.Player(0).Value.item==FishingAbility.None,"SALVAGE_NOT_GRANTED_EARLY");s.Advance(18,0);check(s.Player(0).Value.item==FishingAbility.Sonar&&!s.Player(0).Value.Busy&&s.Player(0).Value.score==0&&!s.Fish.Any(f=>f.claimed),"SALVAGE_GRANTS_GEAR_NO_POINTS_OR_FISH_CLAIMS");
   s.Begin(new ulong[]{0},94,0,false);A(0,FishingAction.Salvage,1,12);s.Act(0,94,1,FishingAction.Cancel,0,Vector3.zero,13);s.Advance(20,0);check(s.Player(0).Value.item==FishingAbility.None,"CANCELLED_SALVAGE_NEVER_GRANTS_LOOT");
   s.Begin(new ulong[]{0},95,0,false);s.Act(0,95,0,FishingAction.Cast,0,Vector3.zero,0,2);s.Advance(3,0);s.Act(0,95,1,FishingAction.Hook,0,Vector3.zero,3);float maxStep=0;Vector2 previous=s.Player(0).Value.fightOffset;bool rising=false,falling=false;
   for(int i=0;i<800;i++){double now=3+(i+1)*.01;s.Advance(now,.01f);var p=s.Player(0).Value;if(p.phase!=FishingPhase.Reeling)break;maxStep=Mathf.Max(maxStep,Vector2.Distance(previous,p.fightOffset));previous=p.fightOffset;check(float.IsFinite(p.fightMotion.x)&&float.IsFinite(p.fightMotion.y)&&Mathf.Abs(p.fightOffset.x)<4.5f&&p.fightOffset.y>-.9f&&p.fightOffset.y<1.5f,"FINITE_BOUNDED_FISH_FORCES_"+i);rising|=p.fightMotion.y<-.1f;falling|=p.fightMotion.y>.1f;}
   check(maxStep<.08f&&rising&&falling,"CONTINUOUS_INERTIAL_SWIM_WITH_RISING_AND_FALLING_MOTION");
   var copy=s.Player(0).Value;var other=copy;other.fightMotion.x+=.1f;check(!copy.Equals(other),"PHYSICAL_VELOCITY_REPLICATED_EQUALITY");other=copy;other.item=FishingAbility.Flippers;check(!copy.Equals(other),"INVENTORY_REPLICATED_EQUALITY");var match=s.Match;var m=match;m.supply0+=1;check(!match.Equals(m),"SHARED_PICKUP_COOLDOWN_REPLICATED_EQUALITY");
   copy.favourites=514;copy.item=FishingAbility.CoolSpool;copy.active=FishingAbility.PowerReel;copy.salvageItem=FishingAbility.QuickLure;copy.itemSequence=17;copy.activeAt=3;copy.activeUntil=7;copy.sonarUntil=11;copy.speedUntil=8;copy.quickLure=copy.bubbleGuard=true;copy.fightHeading=new Vector3(.6f,0,.8f);copy.fightOffset=new Vector2(.8f,.4f);copy.fightMotion=new Vector2(1.2f,-.7f);
   using(var writer=new FastBufferWriter(1024,Allocator.Temp)){writer.WriteNetworkSerializable(copy);using(var reader=new FastBufferReader(writer,Allocator.Temp)){reader.ReadNetworkSerializable(out FishingPlayerRecord decoded);check(copy.Equals(decoded),"NONDEFAULT_GEAR_AND_PHYSICS_WIRE_ROUNDTRIP");}}
   match.supply0=1;match.supply1=2;match.supply2=3;match.supply3=4;match.supply4=5;match.salvageReadyAt=6;
   using(var writer=new FastBufferWriter(512,Allocator.Temp)){writer.WriteNetworkSerializable(match);using(var reader=new FastBufferReader(writer,Allocator.Temp)){reader.ReadNetworkSerializable(out FishingMatchSnapshot decoded);check(match.Equals(decoded),"ALL_SHARED_SUPPLY_AND_TREASURE_DEADLINES_WIRE_ROUNDTRIP");}}
  }
 }
}
#endif
