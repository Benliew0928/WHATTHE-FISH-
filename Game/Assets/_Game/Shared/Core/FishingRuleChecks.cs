#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
using UnityEngine;
namespace WhatTheFish {
 // Integration fixtures exercise authoritative scoring, deadlines and real line integration.
 public static class FishingRuleChecks {
  static FishingPlayerRecord P(FishingState s,ulong id=0)=>s.Player(id).Value;
  static bool Act(FishingState s,FishingAction a,double now,ulong id=0,int fish=-1,float distance=7)=>s.Act(id,s.Match.round,P(s,id).sequence,a,fish>=0?FishingState.PierForFish(fish):0,Vector3.zero,now,fish,distance);
  static void Ready(FishingState s,double now){foreach(var p in s.Players.ToArray())Act(s,FishingAction.Ready,now,p.owner);}
  static FishingState Round(int count,FishingMode mode,uint token=2){var s=new FishingState();s.Begin(Enumerable.Range(0,count).Select(i=>(ulong)i),1,0,false);Ready(s,0);if(!s.Start(s.Players.Select(p=>p.owner).ToArray(),token,0,mode))throw new Exception("Start fixture");s.Advance(3,0);return s;}
  static void Hook(FishingState s,ref double now,int fish,ulong id=0,float distance=7){if(!Act(s,FishingAction.Cast,now,id,fish,distance))throw new Exception("Cast fixture "+fish);now=P(s,id).biteAt;s.Advance(now,0);if(!Act(s,FishingAction.Hook,now,id))throw new Exception("Hook fixture");}
  static double Land(FishingState s,ref double now,int fish,ulong id=0,float distance=7){Hook(s,ref now,fish,id,distance);bool held=false;double hookedAt=now,limit=now+31;
   while(P(s,id).phase==FishingPhase.Reeling&&now<limit){var p=P(s,id);if(p.tension>.84f)held=false;else if(p.tension<.58f)held=true;Act(s,held?FishingAction.ReelStart:FishingAction.ReelStop,now,id);now+=.02;s.Advance(now,.02f);}
   return now-hookedAt;
  }
  static void Scores(FishingState s,params int[] scores){for(int i=0;i<scores.Length;i++){var p=s.Players[i];p.score=scores[i];s.Players[i]=p;}}
  static FishingState RemoteCast(){var s=new FishingState();s.Begin(new ulong[]{0,1},1,0,false);if(!s.Act(1,1,0,FishingAction.Cast,0,Vector3.zero,0,0,7,0,true))throw new Exception("Remote cast fixture");return s;}
  static bool RemoteHook(FishingState s,double receiveAt,double pressedAt,uint? sequence=null,ulong owner=1,uint? round=null)=>s.Act(owner,round??s.Match.round,sequence??P(s,1).sequence,FishingAction.Hook,0,Vector3.zero,receiveAt,-1,7,pressedAt,true);
  static void HookTiming(Action<bool,string> check){
   var s=RemoteCast();var p=P(s,1);double until=p.hookUntil;
   check(Math.Abs(until-p.biteAt-1.8)<.000001,"REMOTE_REACTION_WINDOW_REMAINS_1_POINT_8_SECONDS");
   check(!RemoteHook(s,p.biteAt+.01,p.biteAt-.001)&&P(s,1).hooks==0,"REMOTE_PRE_BITE_PRESS_CANNOT_BECOME_HOOK");
   s=RemoteCast();p=P(s,1);check(!RemoteHook(s,p.biteAt-.01,p.biteAt+.039),"FUTURE_TOLERANCE_CANNOT_MANUFACTURE_BITE");
   s=RemoteCast();p=P(s,1);check(RemoteHook(s,p.biteAt,p.biteAt)&&P(s,1).hooks==1,"REMOTE_EXACT_BITE_START_ACCEPTED");
   s=RemoteCast();p=P(s,1);check(RemoteHook(s,p.biteAt+.1,p.biteAt+.149)&&P(s,1).phaseAt==p.biteAt+.1,"SMALL_CLOCK_SKEW_CLAMPED_TO_HOST_NOW");
   s=RemoteCast();p=P(s,1);check(!RemoteHook(s,p.biteAt+.1,p.biteAt+.151),"REMOTE_FUTURE_OVER_50MS_REJECTED");
   s=RemoteCast();p=P(s,1);check(RemoteHook(s,p.biteAt+.499,p.biteAt+.25),"REMOTE_249MS_TRANSIT_ACCEPTED");
   s=RemoteCast();p=P(s,1);check(RemoteHook(s,p.biteAt+.5,p.biteAt+.25),"REMOTE_EXACT_250MS_TRANSIT_ACCEPTED");
   s=RemoteCast();p=P(s,1);check(!RemoteHook(s,p.biteAt+.501,p.biteAt+.25),"REMOTE_251MS_STALE_PRESS_REJECTED");
   s=RemoteCast();p=P(s,1);check(!RemoteHook(s,p.biteAt+.1,double.NaN)&&!RemoteHook(s,p.biteAt+.1,double.PositiveInfinity)&&!RemoteHook(s,p.biteAt+.1,double.NegativeInfinity),"REMOTE_NONFINITE_PRESS_REJECTED");
   s=RemoteCast();p=P(s,1);until=p.hookUntil;s.Advance(until+.2,0);
   check(P(s,1).phase==FishingPhase.Bite&&s.Fish[0].claimed,"REMOTE_TIMELY_RPC_RESERVATION_RETAINED_200MS");
   check(RemoteHook(s,until+.2,until-.049)&&P(s,1).hooks==1&&P(s,1).missedBites==0,"PRE_DEADLINE_PRESS_RECEIVED_LATE_ACCEPTED");
   check(!RemoteHook(s,until+.201,until-.049)&&P(s,1).hooks==1,"DUPLICATE_LATE_HOOK_CANNOT_COUNT_TWICE");
   s=RemoteCast();until=P(s,1).hookUntil;check(!RemoteHook(s,until,until)&&!RemoteHook(s,until+.01,until+.001),"EXACT_AND_AFTER_HOOK_PRESS_DEADLINE_REJECTED");
   s=RemoteCast();until=P(s,1).hookUntil;s.Advance(until+.25,0);
   check(P(s,1).result==FishingResult.Missed&&P(s,1).missedBites==1&&!s.Fish[0].claimed&&!RemoteHook(s,until+.25,until-.001),"EXACT_TRANSIT_GRACE_EXPIRES_RESERVATION_ONCE");
   s.Advance(until+.3,0);check(P(s,1).missedBites==1,"REPEATED_MISS_TICK_NEVER_DOUBLE_COUNTS");
   s=RemoteCast();p=P(s,1);check(!RemoteHook(s,p.biteAt+.1,p.biteAt+.05,p.sequence-1)&&!RemoteHook(s,p.biteAt+.1,p.biteAt+.05,p.sequence,1,s.Match.round-1)&&!RemoteHook(s,p.biteAt+.1,p.biteAt+.05,p.sequence,99),"REMOTE_STALE_CAST_ROUND_AND_UNKNOWN_OWNER_REJECTED");
   s=RemoteCast();p=P(s,1);Act(s,FishingAction.Cancel,p.biteAt+.1,1);check(!RemoteHook(s,p.biteAt+.2,p.biteAt+.05,p.sequence)&&!s.Fish[0].claimed&&P(s,1).cancels==1,"CANCEL_INVALIDATES_IN_TRANSIT_HOOK_AND_COUNTS_ONCE");
   s.Advance(p.biteAt+2.2,0);Act(s,FishingAction.Cast,p.biteAt+2.2,1,0);check(!RemoteHook(s,p.biteAt+2.3,p.biteAt+2.25,p.sequence),"PREVIOUS_CAST_RPC_CANNOT_HOOK_NEW_CAST");
   var newCast=P(s,1);s.Advance(newCast.hookUntil,0);check(newCast.sequence!=p.sequence&&P(s,1).result==FishingResult.Missed,"REMOTE_RESERVATION_GRACE_DOES_NOT_LEAK_TO_NEXT_HOST_CAST");
   s=RemoteCast();p=P(s,1);s.Disconnect(1,p.biteAt+.1);check(!RemoteHook(s,p.biteAt+.2,p.biteAt+.05,p.sequence)&&!s.Fish[0].claimed,"DISCONNECT_CLEARS_REMOTE_HOOK_RESERVATION");
   s=RemoteCast();p=P(s,1);s.Match.phase=FishingRoundPhase.Playing;s.Match.deadline=p.hookUntil+.1;
   check(!RemoteHook(s,s.Match.deadline,p.hookUntil-.05,p.sequence)&&s.Match.phase==FishingRoundPhase.Ended&&!s.Fish[0].claimed,"MATCH_DEADLINE_OVERRIDES_TRANSIT_GRACE");
   s=new FishingState();s.Begin(new ulong[]{0},1,0,false);Act(s,FishingAction.Cast,0,0,0);p=P(s);s.Advance(p.hookUntil,0);
   check(P(s).result==FishingResult.Missed&&P(s).missedBites==1&&!s.Fish[0].claimed,"HOST_OFFLINE_EXACT_DEADLINE_REMAINS_IMMEDIATE_MISS");
  }
  public static void Run(Action<bool,string> check){
   HookTiming(check);
   var s=new FishingState();double now=100;s.Begin(new ulong[]{0,1,2,3},1,now,false);
   check(s.Players.Count==4&&s.Fish.Count==30,"PRACTICE_FIVE_SHARED_SCHOOLS_SIX_FISH_EACH");
   for(int pier=0;pier<FishingState.PierCount;pier++)check(s.Fish.Count(f=>FishingState.PierForFish(f.id)==pier)==6&&Enumerable.Range(0,FishingState.SizeCount).All(size=>s.Fish.Count(f=>FishingState.PierForFish(f.id)==pier&&FishingState.SizeForFish(f.id)==size)==2),"TWO_OF_EACH_TIER_AT_PIER_"+pier);
   check(FishingState.PierForFish(-1)==-1&&FishingState.PierForFish(30)==-1&&FishingState.SizeForFish(-1)==-1,"INVALID_FISH_HAS_NO_PIER_OR_SIZE");
   check(!s.Act(0,99,0,FishingAction.Cast,0,Vector3.zero,now,0)&&!s.Act(99,1,0,FishingAction.Cast,0,Vector3.zero,now,0),"STALE_TOKEN_UNKNOWN_OWNER");
   check(!s.Act(0,1,0,FishingAction.Cast,-1,Vector3.zero,now,0)&&!s.Act(0,1,0,FishingAction.Cast,0,new Vector3(float.NaN,0,0),now,0),"INVALID_PIER_NONFINITE_ORIGIN");
   check(!Act(s,FishingAction.Cast,now),"MISSING_TARGET_NEVER_AUTO_SELECTS");
   check(Act(s,FishingAction.Cast,now,0,1)&&P(s).fish==1&&!Act(s,FishingAction.Hook,now),"EXPLICIT_MEDIUM_TARGET_NO_EARLY_HOOK");
   check(!Act(s,FishingAction.Cast,now,1,1)&&Act(s,FishingAction.Cast,now,1,0)&&Act(s,FishingAction.Cast,now,2,2)&&!Act(s,FishingAction.Cast,now,3,0),"EXCLUSIVE_FISH_CLAIMS");
   check(!s.Act(0,1,0,FishingAction.Cancel,0,Vector3.zero,now),"STALE_CAST_SEQUENCE");
   now=P(s).hookUntil;s.Advance(now,0);check(P(s).result==FishingResult.Missed&&P(s).score==0,"EXACT_HOOK_BOUNDARY_MISSES");
   // Recorded original reeling times at 20ms steps, using release above .84 / resume below .58.
   // Guard both directions: the slight easing must improve catches without removing their challenge.
   double[,] originalReelSeconds={{3.34,5.52,8.04,9.68},{5.68,9.60,13.14,15.30},{10.98,16.66,23.04,26.78}};
   float[] ranges={5.5f,8f,10.5f,12f};
   for(int fish=0;fish<FishingState.FishCount;fish++)for(int r=0;r<ranges.Length;r++){
    float range=ranges[r];s.Begin(new ulong[]{0},2,0,false);now=0;
    check(!s.Act(0,s.Match.round,P(s).sequence,FishingAction.Cast,(FishingState.PierForFish(fish)+1)%FishingState.PierCount,Vector3.zero,now,fish,range),"CROSS_PIER_TARGET_REJECTED_"+fish+"_"+range);
    double reelSeconds=Land(s,ref now,fish,0,range);
    check(P(s).phase==FishingPhase.Caught&&P(s).score==FishingState.Points(fish),"BALANCED_LINE_LANDS_"+fish+"_AT_"+range.ToString("0.0",System.Globalization.CultureInfo.InvariantCulture)+"m_"+now.ToString("0.0",System.Globalization.CultureInfo.InvariantCulture)+"s");
    var landed=P(s);check(landed.smallCatches+landed.mediumCatches+landed.largeCatches==1&&landed.catches==1&&landed.bonusTotal==0&&(FishingState.SizeForFish(fish)==0?landed.smallCatches==1:FishingState.SizeForFish(fish)==1?landed.mediumCatches==1:landed.largeCatches==1),"EXACT_SIZE_AND_PRACTICE_STAT_"+fish+"_"+range);
    if(fish<3){double saved=originalReelSeconds[fish,r]-reelSeconds;check(saved>.1&&saved<2.2,"MODEST_REEL_EASING_"+fish+"_"+range);}
    if(FishingState.SizeForFish(fish)==2)check(reelSeconds>=8&&reelSeconds<28,"LARGE_FISH_RETAINS_CONTROL_CHALLENGE_"+fish+"_"+range);
    int award=P(s).score;check(!Act(s,FishingAction.Hook,now)&&P(s).score==award&&s.Fish[fish].respawnAt>now,"ONCE_ONLY_AWARD_AND_RESPAWN_"+fish+"_"+range);
   }
   s.Begin(new ulong[]{0,1},2,0,false);check(Act(s,FishingAction.Cast,0,0,0)&&Act(s,FishingAction.Cast,0,1,3)&&s.Fish[0].claimed&&s.Fish[3].claimed,"SAME_TIER_FISH_HAVE_INDEPENDENT_RESERVATIONS");
   s.Begin(new ulong[]{0},3,0,false);now=0;Hook(s,ref now,2);Act(s,FishingAction.ReelStart,now);now+=1;s.Advance(now,.02f);check(!P(s).held,"LOST_RELEASE_LEASE");
   Act(s,FishingAction.Cancel,now);check(!s.Fish.Any(f=>f.claimed)&&P(s).score==0&&P(s).cancels==1&&!Act(s,FishingAction.Cancel,now)&&P(s).cancels==1,"CANCEL_RELEASE_NO_POINTS_ONCE_ONLY_STAT");
   for(int fish=2;fish<FishingState.FishCount;fish+=FishingState.SizeCount){s.Begin(new ulong[]{0},4,0,false);now=0;Hook(s,ref now,fish,0,5.5f);for(int i=0;i<1500&&P(s).phase==FishingPhase.Reeling;i++){Act(s,FishingAction.ReelStart,now);now+=.02;s.Advance(now,.02f);}
    check(P(s).result==FishingResult.Snapped&&P(s).score==0&&P(s).snappedLines==1,"CONTINUOUS_LARGE_OVERREEL_SNAPS_"+fish);
   }
   s.Begin(new ulong[]{0},5,0,false);now=0;Hook(s,ref now,0);s.Advance(now+30,.02f);check(P(s).result==FishingResult.TimedOut&&P(s).score==0&&P(s).timeouts==1,"NO_REEL_TIMEOUT");
   s.Begin(new ulong[]{0,1},6,0,false);Act(s,FishingAction.Ready,0);check(!s.Start(new ulong[]{0,1},7,0,FishingMode.Round),"EVERYONE_MUST_READY");
   Act(s,FishingAction.Ready,0,1);check(s.Start(new ulong[]{0,1},7,0,FishingMode.Round)&&s.Match.phase==FishingRoundPhase.Countdown&&!Act(s,FishingAction.Cast,2,0,0),"THREE_SECOND_COUNTDOWN_BLOCKS_CAST");
   s.Advance(3,0);check(s.Match.phase==FishingRoundPhase.Playing&&s.Match.deadline-s.Match.started==180,"SHARED_THREE_MINUTE_ROUND");
   now=3;Land(s,ref now,0);check(P(s).score==5&&P(s).lastBonus==3,"FIRST_WANTED_SMALL_BONUS");
   s.Advance(now+=7.1,0);Land(s,ref now,0);check(P(s).score==7&&P(s).lastBonus==0,"NO_REPEAT_WINDOW_BONUS");
   s.Advance(now=63,0);Land(s,ref now,1);check(P(s).score==15&&P(s).lastBonus==3,"MINUTE_TWO_MEDIUM_BONUS");
   s.Advance(now=123,0);Land(s,ref now,2);check(P(s).score==28&&P(s).lastBonus==3&&P(s).wantedMask==7&&s.Match.phase==FishingRoundPhase.Playing,"MAX_NINE_BONUS_POINTS_TWENTY_DOES_NOT_END_CONTEST");
   check(P(s).smallCatches==2&&P(s).mediumCatches==1&&P(s).largeCatches==1&&P(s).bonusTotal==9&&P(s).catches==4,"ROUND_RECAP_SIZE_AND_TOTAL_BONUS_COUNTS");
   Scores(s,28,28);check(s.Rank(0)==1&&s.Rank(1)==1,"SHARED_ROUND_RANK");
   s.Advance(183,0);check(s.Match.phase==FishingRoundPhase.Ended&&s.Match.outcome==FishingOutcome.Finished&&!Act(s,FishingAction.Cast,183,0,0),"EXACT_DEADLINE_PRESERVES_RESULTS");
   var clone=new FishingState();clone.Receive(s.Match,s.Players,s.Fish);check(clone.Match.Equals(s.Match)&&clone.Players.SequenceEqual(s.Players)&&clone.Fish.SequenceEqual(s.Fish),"COMPLETE_SNAPSHOT_ROUNDTRIP");
   Ready(s,184);check(s.Start(new ulong[]{0,1},s.Match.round+1,184,FishingMode.Round)&&s.Players.All(q=>q.catches==0&&q.smallCatches==0&&q.mediumCatches==0&&q.largeCatches==0&&q.bonusTotal==0&&q.missedBites==0&&q.snappedLines==0&&q.cancels==0&&q.timeouts==0),"READY_REMATCH_RESETS_ALL_RECAP_STATS");
   s=Round(1,FishingMode.Round);now=3;Land(s,ref now,0);s.Advance(now+=2.1,0);Land(s,ref now,3);check(P(s).score==7&&P(s).lastBonus==0&&P(s).wantedMask==1,"SECOND_FISH_OF_WANTED_TIER_DOES_NOT_REPEAT_BONUS");
   s=Round(3,FishingMode.Cup);Scores(s,20,20,5);s.Advance(s.Match.deadline,0);
   check(P(s).cupPoints==3&&P(s,1).cupPoints==3&&P(s,2).cupPoints==1,"CUP_SHARED_FIRST_SKIPS_SECOND");
   for(int r=2;r<=3;r++){Ready(s,s.Match.deadline+1);double start=s.Match.deadline+1;check(s.Start(new ulong[]{0,1,2},(uint)(r+1),start,FishingMode.Cup)&&s.Match.cupRound==r,"CUP_CONTINUES_ROUND_"+r);s.Advance(s.Match.started,0);Scores(s,r==2?5:10,r==2?4:9,3);s.Advance(s.Match.deadline,0);}
   check(s.Match.cupFinished&&s.CupRank(0)==1&&P(s).cupScore==35,"THREE_ROUND_CUP_ACCUMULATES_AND_FINISHES");
   s=Round(2,FishingMode.Cup);s.Disconnect(1,20);s.Advance(s.Match.deadline,0);check(!s.Match.cupValid&&P(s).cupPoints==0,"ROSTER_CHANGE_INVALIDATES_CUP");
   for(int count=2;count<=5;count++){s=Round(count,FishingMode.Crew);check(s.Match.crewTarget==count*20,"CREW_TARGET_20_EACH_"+count);}
   s=Round(2,FishingMode.Crew);Scores(s,25,15);s.Match.crewScore=40;s.Match.crewMask=3;for(int i=0;i<2;i++){var p=s.Players[i];p.catches=1;s.Players[i]=p;}s.Advance(100,0);check(s.Match.phase==FishingRoundPhase.Playing,"CREW_REQUIRES_ALL_THREE_SIZES");
   s.Match.crewMask=7;var missing=s.Players[1];missing.catches=0;s.Players[1]=missing;s.Advance(101,0);check(s.Match.phase==FishingRoundPhase.Playing,"CREW_REQUIRES_EVERY_PLAYER");
   missing.catches=1;s.Players[1]=missing;s.Advance(102,0);check(s.Match.outcome==FishingOutcome.Success&&s.Match.medal==3,"CREW_COMPLETES_EARLY_SHARED_GOLD");
   foreach(double finish in new[]{153d,173d}){s=Round(2,FishingMode.Crew);s.Match.crewScore=40;s.Match.crewMask=7;for(int i=0;i<2;i++){var p=s.Players[i];p.catches=1;s.Players[i]=p;}s.Advance(finish,0);check(s.Match.medal==(finish==153?2:1),"CREW_MEDAL_"+finish);}
   s=Round(2,FishingMode.Crew);s.Advance(183,0);check(s.Match.outcome==FishingOutcome.Failed,"CREW_INCOMPLETE_AT_DEADLINE");
   s=Round(2,FishingMode.Crew);Act(s,FishingAction.Cast,4,1,0);s.Disconnect(1,5);check(s.Match.outcome==FishingOutcome.Abandoned&&!s.Fish.Any(f=>f.claimed),"CREW_DISCONNECT_ABANDONS_AND_RELEASES");
   s=Round(2,FishingMode.Round);s.Join(8);check(!s.Player(8).HasValue,"LATE_PLAYER_WAITS_NEXT_ROUND");s.Disconnect(1,5);check(s.Match.phase==FishingRoundPhase.Playing&&!P(s,1).connected,"ROUND_CONTINUES_WITH_LEFT_ROW");
   s.Advance(183,0);s.Join(8);check(s.Player(8).HasValue&&!s.Player(1).HasValue,"NEXT_ROUND_REPLACES_LEFT_SLOT");
   s.Clear();check(s.Match.phase==FishingRoundPhase.Idle&&s.Fish.Count==0,"CLEAR_RELEASES_MATCH");
  }
 }
}
#endif
