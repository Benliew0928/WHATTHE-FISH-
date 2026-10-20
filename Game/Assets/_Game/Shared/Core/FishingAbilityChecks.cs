#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
namespace WhatTheFish {
 public static class FishingAbilityChecks {
  public static void CompleteDraft(FishingState s,double now){foreach(var q in s.Players.ToArray())s.AbilityAct(q.owner,s.Match.round,q.sequence,FishingAction.ChooseSkill,(int)s.Match.offer0,0,Vector3.zero,now);s.Advance(now,0);}
  public static void Run(Action<bool,string> check){
   var s=new FishingState();
   bool A(ulong id,FishingAction action,int choice,double now)=>s.AbilityAct(id,s.Match.round,s.Player(id).Value.sequence,action,choice,0,Vector3.zero,now,null,new Vector3(3,0,4),7);
   check(FishingAbilityRules.Roster.Length==6&&FishingAbilityRules.Roster.Distinct().Count()==6&&new[]{2,3,4,5,6,9,255}.All(i=>!FishingAbilityRules.Valid((FishingAbility)i)),"SIX_SKILLS_AND_RETIRED_IDS_STAY_RESERVED");
   for(uint seed=1;seed<=80;seed++){
    s.Begin(new ulong[]{0,1},seed,0,false);var peer=new FishingState();peer.Begin(new ulong[]{1,0},seed,0,false);var offers=Enumerable.Range(0,3).Select(s.Offer).ToArray();
    check(offers.Distinct().Count()==3&&offers.All(FishingAbilityRules.Valid)&&offers.Any(FishingAbilityRules.BeforeCast)&&offers.Any(a=>!FishingAbilityRules.BeforeCast(a))&&offers.SequenceEqual(Enumerable.Range(0,3).Select(peer.Offer)),"SHARED_DISTINCT_BALANCED_DRAFT_"+seed);
   }
   s.Begin(new ulong[]{0,1},91,0,false);foreach(var q in s.Players.ToArray())s.Act(q.owner,91,q.sequence,FishingAction.Ready,0,Vector3.zero,0);
   check(s.Start(new ulong[]{0,1},92,0,FishingMode.Round)&&s.Preparing&&s.Match.phase==FishingRoundPhase.Drafting&&!s.Running&&s.Match.deadline==0,"DRAFT_PRECEDES_COUNTDOWN_AND_MATCH_CLOCK");
   check(!A(0,FishingAction.ChooseSkill,257,0)&&!A(0,FishingAction.ChooseSkill,-1,0)&&!A(0,FishingAction.ChooseSkill,0,0)&&!A(0,FishingAction.UseAbility,-1,0),"DRAFT_REJECTS_WRAPPED_INVALID_AND_EARLY_USE");
   var excluded=FishingAbilityRules.Roster.First(a=>!s.Offered(a));check(!A(0,FishingAction.ChooseSkill,(int)excluded,0),"UNOFFFERED_SKILL_CANNOT_BE_FORGED");
   check(A(0,FishingAction.ChooseSkill,(int)s.Match.offer1,1)&&!A(0,FishingAction.ChooseSkill,(int)s.Match.offer0,1)&&!s.Act(0,92,0,FishingAction.Cast,0,Vector3.zero,1,0),"PICK_LOCKS_ONCE_AND_DRAFT_BLOCKS_FISHING");
   s.Advance(11.9,0);check(s.Match.phase==FishingRoundPhase.Drafting,"DRAFT_WAITS_FOR_OTHER_PLAYER");s.Advance(12,0);check(s.Match.phase==FishingRoundPhase.Countdown&&s.Players.All(q=>q.skillPicked)&&s.Player(1).Value.item==s.Match.offer0&&s.Match.started==15&&s.Match.deadline==195,"TIMEOUT_PICKS_FIRST_CARD_WITHOUT_SHORTENING_ROUND");
   check(!A(1,FishingAction.ChooseSkill,(int)s.Match.offer2,12),"LATE_PICK_CANNOT_REPLACE_AUTOMATIC_CHOICE");
   s.Advance(15,0);check(s.Running&&s.Match.phase==FishingRoundPhase.Playing,"NORMAL_COUNTDOWN_STARTS_FULL_THREE_MINUTES");s.Disconnect(1,16);check(!s.Player(1).Value.connected,"DISCONNECTED_SKILL_OWNER_REMOVED");
   s.Begin(new ulong[]{0,1},93,0,false);foreach(var q in s.Players.ToArray())s.Act(q.owner,93,q.sequence,FishingAction.Ready,0,Vector3.zero,0);s.Start(new ulong[]{0,1},94,0,FishingMode.Crew);s.Disconnect(1,1);check(s.Match.outcome==FishingOutcome.Abandoned&&!s.Preparing,"DISCONNECT_ABANDONS_UNFINISHED_DRAFT");
   foreach(var ability in FishingAbilityRules.Roster){
    s.Begin(new ulong[]{0,1},95,0,false);var p=s.Players[0];p.item=ability;p.skillPicked=true;s.Players[0]=p;
    check(!A(0,FishingAction.Supply,(int)ability,0)&&!A(0,FishingAction.Salvage,(int)ability,0)&&!A(0,FishingAction.Favourites,(int)ability,0),"RETIRED_ACQUISITION_CANNOT_GRANT_SKILLS_"+ability);
    if(!FishingAbilityRules.BeforeCast(ability)){s.Act(0,95,0,FishingAction.Cast,0,Vector3.zero,0,2);s.Advance(3,0);s.Act(0,95,1,FishingAction.Hook,0,Vector3.zero,3);}
    p=s.Players[0];p.tension=.65f;p.strain=.3f;s.Players[0]=p;
    check(!s.AbilityAct(0,94,p.sequence,FishingAction.UseAbility,-1,0,Vector3.zero,3.1)&&!s.AbilityAct(0,95,p.sequence+1,FishingAction.UseAbility,-1,0,Vector3.zero,3.1),"STALE_SKILL_ROUND_AND_SEQUENCE_REJECTED_"+ability);
    check(A(0,FishingAction.UseAbility,-1,3.2)&&s.Players[0].item==ability&&s.Players[0].active==ability&&s.Players[0].cooldownUntil==3.2+FishingAbilityRules.Cooldown(ability),"PERSISTENT_SKILL_ACTIVATES_AND_STARTS_COOLDOWN_"+ability);
    check(!A(0,FishingAction.UseAbility,-1,3.2),"REPLAY_CANNOT_STACK_OR_REFRESH_SKILL_"+ability);
    if(ability==FishingAbility.Sonar)check(s.Players[0].sonarUntil==11.2&&s.Players[1].sonarUntil==0,"COMPETITIVE_SONAR_IS_OWNER_ONLY");
    if(ability==FishingAbility.SchoolCall)check(s.Players[0].schoolFishMask==7&&s.Players[0].schoolPoint==new Vector3(3,0,4)&&s.Fish.All(f=>!f.claimed),"SCHOOL_CALL_TARGETS_BUT_NEVER_RESERVES_OR_SCORES_FISH");
    if(ability==FishingAbility.SteelWire){float limit=s.Players[0].steelTension;for(int i=0;i<100;i++){var q=s.Players[0];q.held=true;q.steer=(sbyte)-FishingState.CounterSide(q,3.21+i*.02);q.reelLease=10;s.Players[0]=q;s.Advance(3.21+i*.02,.02f);check(s.Players[0].tension<=limit+.0001f&&s.Players[0].result!=FishingResult.Snapped,"STEEL_PROTECTS_ACTUAL_LOAD_"+i);}}
    double later=3.2+FishingAbilityRules.Duration(ability)+.1;s.Advance(later,0);check(s.Players[0].active==FishingAbility.None&&!A(0,FishingAction.UseAbility,-1,later),"EFFECT_EXPIRY_DOES_NOT_SKIP_COOLDOWN_"+ability);
    p=s.Players[0];p.phase=FishingAbilityRules.BeforeCast(ability)?FishingPhase.Ready:FishingPhase.Reeling;p.phaseAt=50;p.item=ability;s.Players[0]=p;check(A(0,FishingAction.UseAbility,-1,50)&&s.Players[0].itemSequence==2,"SAME_CHOSEN_SKILL_REUSABLE_AFTER_COOLDOWN_"+ability);
   }
   s.Begin(new ulong[]{0,1,2},96,0,false);s.Match.mode=FishingMode.Crew;var sonar=s.Players[0];sonar.item=FishingAbility.Sonar;s.Players[0]=sonar;A(0,FishingAction.UseAbility,-1,1);check(s.Players.All(q=>q.sonarUntil==9),"CREW_SONAR_SHARES_WITH_ALL_STARTING_PLAYERS");
   s.Begin(new ulong[]{0},95,0,false);s.Act(0,95,0,FishingAction.Cast,0,Vector3.zero,0,2);s.Advance(3,0);s.Act(0,95,1,FishingAction.Hook,0,Vector3.zero,3);float maxStep=0;Vector2 previous=s.Player(0).Value.fightOffset;bool rising=false,falling=false;
   for(int i=0;i<800;i++){double now=3+(i+1)*.01;s.Advance(now,.01f);var p=s.Player(0).Value;if(p.phase!=FishingPhase.Reeling)break;maxStep=Mathf.Max(maxStep,Vector2.Distance(previous,p.fightOffset));previous=p.fightOffset;check(float.IsFinite(p.fightMotion.x)&&float.IsFinite(p.fightMotion.y)&&Mathf.Abs(p.fightOffset.x)<4.5f&&p.fightOffset.y>-.9f&&p.fightOffset.y<1.5f,"FINITE_BOUNDED_FISH_FORCES_"+i);rising|=p.fightMotion.y<-.1f;falling|=p.fightMotion.y>.1f;}
   check(maxStep<.08f&&rising&&falling,"CONTINUOUS_INERTIAL_SWIM_WITH_RISING_AND_FALLING_MOTION");
   var copy=s.Player(0).Value;var other=copy;other.fightMotion.x+=.1f;check(!copy.Equals(other),"PHYSICAL_VELOCITY_REPLICATED_EQUALITY");
   copy.item=FishingAbility.FishingBuddy;copy.active=FishingAbility.SteelWire;copy.skillPicked=true;copy.itemSequence=17;copy.activeAt=3;copy.activeUntil=7;copy.sonarUntil=11;copy.cooldownUntil=27;copy.schoolAt=2;copy.schoolPoint=new Vector3(1,2,3);copy.schoolFishMask=1u<<29;copy.steelTension=.73f;copy.bubbleGuard=true;copy.fightHeading=new Vector3(.6f,0,.8f);copy.fightOffset=new Vector2(.8f,.4f);copy.fightMotion=new Vector2(1.2f,-.7f);copy.tapGrade=FishingTapGrade.Miss;
   using(var writer=new FastBufferWriter(1024,Allocator.Temp)){writer.WriteNetworkSerializable(copy);using(var reader=new FastBufferReader(writer,Allocator.Temp)){reader.ReadNetworkSerializable(out FishingPlayerRecord decoded);check(copy.Equals(decoded),"DRAFT_COOLDOWN_SCHOOL_PHYSICS_AND_SIGNED_GRADE_WIRE_ROUNDTRIP");}}
   var match=s.Match;match.phase=FishingRoundPhase.Drafting;match.offer0=FishingAbility.Sonar;match.offer1=FishingAbility.FishingBuddy;match.offer2=FishingAbility.PowerReel;match.draftUntil=23;
   using(var writer=new FastBufferWriter(512,Allocator.Temp)){writer.WriteNetworkSerializable(match);using(var reader=new FastBufferReader(writer,Allocator.Temp)){reader.ReadNetworkSerializable(out FishingMatchSnapshot decoded);check(match.Equals(decoded),"SHARED_OFFER_AND_DRAFT_DEADLINE_WIRE_ROUNDTRIP");}}
  }
 }
}
#endif
