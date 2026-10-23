using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public enum FishingAction:byte {None,Cast,Hook,ReelStart,ReelStop,Cancel,Ready,Unready,ReelLeft,ReelRight,Favourites,Supply,Salvage,UseAbility,ReelTap,ChooseSkill}
 public enum FishingPhase:byte {Ready,Casting,Waiting,Bite,Reeling,Caught,Escaped,Salvaging}
 public enum FishingRoundPhase:byte {Idle,Practice,Playing,Ended,Countdown,Drafting}
 public enum FishingResult:byte {None,Landed,Missed,Snapped,Cancelled,TimedOut}
 public enum FishingMode:byte {Round,Cup,Crew}
 public enum FishingOutcome:byte {None,Finished,Success,Failed,Abandoned}
 public struct FishingMatchSnapshot:INetworkSerializable,IEquatable<FishingMatchSnapshot> {
  public uint round;public FishingRoundPhase phase;public FishingMode mode;public FishingOutcome outcome;
  public double started,deadline,ended;public int cupRound,crewTarget,crewScore;public byte crewMask,medal,wantedOffset;public bool cupValid,cupFinished;public FishingAbility offer0,offer1,offer2;public double draftUntil;
  public void NetworkSerialize<T>(BufferSerializer<T> s)where T:IReaderWriter {
   s.SerializeValue(ref round);s.SerializeValue(ref phase);s.SerializeValue(ref mode);s.SerializeValue(ref outcome);
   s.SerializeValue(ref started);s.SerializeValue(ref deadline);s.SerializeValue(ref ended);s.SerializeValue(ref cupRound);
   s.SerializeValue(ref crewTarget);s.SerializeValue(ref crewScore);s.SerializeValue(ref crewMask);s.SerializeValue(ref medal);s.SerializeValue(ref wantedOffset);s.SerializeValue(ref cupValid);s.SerializeValue(ref cupFinished);s.SerializeValue(ref offer0);s.SerializeValue(ref offer1);s.SerializeValue(ref offer2);s.SerializeValue(ref draftUntil);
  }
  public bool Equals(FishingMatchSnapshot v)=>round==v.round&&phase==v.phase&&mode==v.mode&&outcome==v.outcome&&started==v.started&&deadline==v.deadline&&ended==v.ended&&cupRound==v.cupRound&&crewTarget==v.crewTarget&&crewScore==v.crewScore&&crewMask==v.crewMask&&medal==v.medal&&wantedOffset==v.wantedOffset&&cupValid==v.cupValid&&cupFinished==v.cupFinished&&offer0==v.offer0&&offer1==v.offer1&&offer2==v.offer2&&draftUntil==v.draftUntil;
 }
 public struct FishingPlayerRecord:INetworkSerializable,IEquatable<FishingPlayerRecord> {
  public ulong owner;public uint sequence,awardSequence;public FishingPhase phase;public FishingResult result;
  public int fish,pier,score,catches,casts,hooks,lastAward,lastBonus,cupPoints,cupScore;
  public int smallCatches,mediumCatches,largeCatches,bonusTotal,missedBites,snappedLines,cancels,timeouts;
  public byte wantedMask,catchMask;public bool held,connected,ready;public double phaseAt,biteAt,hookUntil,reelLease;
  public sbyte steer;public double steerAt;public float pull;public int reelBeat;public FishingTapGrade tapGrade;
  public float progress,tension,distance,lineLength,velocity,strain,castDistance,surge;public Vector3 origin;
  public Vector3 fightHeading;public Vector2 fightOffset,fightMotion;
  public FishingAbility item,active;public uint itemSequence;public double activeAt,activeUntil,sonarUntil,cooldownUntil,schoolAt;public bool skillPicked,bubbleGuard;public float steelTension;public Vector3 schoolPoint;public uint schoolFishMask;
  public bool Busy=>phase>=FishingPhase.Casting&&phase<=FishingPhase.Reeling;
  public void NetworkSerialize<T>(BufferSerializer<T> s)where T:IReaderWriter {
   s.SerializeValue(ref owner);s.SerializeValue(ref sequence);s.SerializeValue(ref awardSequence);s.SerializeValue(ref phase);s.SerializeValue(ref result);
   s.SerializeValue(ref fish);s.SerializeValue(ref pier);s.SerializeValue(ref score);s.SerializeValue(ref catches);s.SerializeValue(ref casts);s.SerializeValue(ref hooks);
   s.SerializeValue(ref smallCatches);s.SerializeValue(ref mediumCatches);s.SerializeValue(ref largeCatches);s.SerializeValue(ref bonusTotal);s.SerializeValue(ref missedBites);s.SerializeValue(ref snappedLines);s.SerializeValue(ref cancels);s.SerializeValue(ref timeouts);
   s.SerializeValue(ref lastAward);s.SerializeValue(ref lastBonus);s.SerializeValue(ref cupPoints);s.SerializeValue(ref cupScore);s.SerializeValue(ref wantedMask);s.SerializeValue(ref catchMask);
   s.SerializeValue(ref held);s.SerializeValue(ref connected);s.SerializeValue(ref ready);s.SerializeValue(ref phaseAt);s.SerializeValue(ref biteAt);s.SerializeValue(ref hookUntil);s.SerializeValue(ref reelLease);
   s.SerializeValue(ref steer);s.SerializeValue(ref steerAt);s.SerializeValue(ref pull);s.SerializeValue(ref reelBeat);s.SerializeValue(ref tapGrade);s.SerializeValue(ref progress);s.SerializeValue(ref tension);s.SerializeValue(ref distance);s.SerializeValue(ref lineLength);s.SerializeValue(ref velocity);s.SerializeValue(ref strain);s.SerializeValue(ref castDistance);s.SerializeValue(ref surge);s.SerializeValue(ref origin);s.SerializeValue(ref fightHeading);s.SerializeValue(ref fightOffset);s.SerializeValue(ref fightMotion);s.SerializeValue(ref item);s.SerializeValue(ref active);s.SerializeValue(ref itemSequence);s.SerializeValue(ref activeAt);s.SerializeValue(ref activeUntil);s.SerializeValue(ref sonarUntil);s.SerializeValue(ref cooldownUntil);s.SerializeValue(ref schoolAt);s.SerializeValue(ref skillPicked);s.SerializeValue(ref bubbleGuard);s.SerializeValue(ref steelTension);s.SerializeValue(ref schoolPoint);s.SerializeValue(ref schoolFishMask);
  }
  public bool Equals(FishingPlayerRecord v)=>owner==v.owner&&sequence==v.sequence&&awardSequence==v.awardSequence&&phase==v.phase&&result==v.result&&fish==v.fish&&pier==v.pier&&score==v.score&&catches==v.catches&&casts==v.casts&&hooks==v.hooks&&smallCatches==v.smallCatches&&mediumCatches==v.mediumCatches&&largeCatches==v.largeCatches&&bonusTotal==v.bonusTotal&&missedBites==v.missedBites&&snappedLines==v.snappedLines&&cancels==v.cancels&&timeouts==v.timeouts&&lastAward==v.lastAward&&lastBonus==v.lastBonus&&cupPoints==v.cupPoints&&cupScore==v.cupScore&&wantedMask==v.wantedMask&&catchMask==v.catchMask&&held==v.held&&connected==v.connected&&ready==v.ready&&phaseAt==v.phaseAt&&biteAt==v.biteAt&&hookUntil==v.hookUntil&&reelLease==v.reelLease&&steer==v.steer&&steerAt==v.steerAt&&pull==v.pull&&reelBeat==v.reelBeat&&tapGrade==v.tapGrade&&progress==v.progress&&tension==v.tension&&distance==v.distance&&lineLength==v.lineLength&&velocity==v.velocity&&strain==v.strain&&castDistance==v.castDistance&&surge==v.surge&&origin==v.origin&&fightHeading==v.fightHeading&&fightOffset==v.fightOffset&&fightMotion==v.fightMotion&&item==v.item&&active==v.active&&itemSequence==v.itemSequence&&activeAt==v.activeAt&&activeUntil==v.activeUntil&&sonarUntil==v.sonarUntil&&cooldownUntil==v.cooldownUntil&&schoolAt==v.schoolAt&&skillPicked==v.skillPicked&&bubbleGuard==v.bubbleGuard&&steelTension==v.steelTension&&schoolPoint==v.schoolPoint&&schoolFishMask==v.schoolFishMask;
 }
 public struct FishingFishRecord:INetworkSerializable,IEquatable<FishingFishRecord> {
  public int id;public bool claimed;public ulong owner;public double respawnAt;
  public void NetworkSerialize<T>(BufferSerializer<T> s)where T:IReaderWriter {s.SerializeValue(ref id);s.SerializeValue(ref claimed);s.SerializeValue(ref owner);s.SerializeValue(ref respawnAt);}
  public bool Equals(FishingFishRecord v)=>id==v.id&&claimed==v.claimed&&owner==v.owner&&respawnAt==v.respawnAt;
 }
 // Host-owned rules and a spring/drag line model; presentations never award points.
 public sealed partial class FishingState {
  public const double MatchSeconds=180,HookSeconds=1.8,HookTransitSeconds=.25,HookClockTolerance=.05,LeaseSeconds=.8,CountdownSeconds=3;
  public const int PierCount=5,FishPerPier=6,SizeCount=3,FishCount=PierCount*FishPerPier,CrewPointsPerPlayer=20;
  public const float MaxCastDistance=44;
  public FishingMatchSnapshot Match;
  public readonly List<FishingPlayerRecord> Players=new();
  public readonly List<FishingFishRecord> Fish=new();
  // Host-derived ownership only: keep a remote cast reserved while its timely Hook is in transit.
  readonly Dictionary<ulong,uint> remoteHookCasts=new();
  public bool Preparing=>Match.phase is FishingRoundPhase.Drafting or FishingRoundPhase.Countdown;
  public bool Running=>Match.phase==FishingRoundPhase.Practice||Match.phase==FishingRoundPhase.Playing;
  public FishingPlayerRecord? Player(ulong id){int i=Players.FindIndex(p=>p.owner==id);return i<0?null:Players[i];}
  public static int PierForFish(int fish)=>fish>=0&&fish<FishCount?fish/FishPerPier:-1;
  public static int SizeForFish(int fish)=>fish>=0&&fish<FishCount?fish%SizeCount:-1;
  public static int Points(int fish)=>SizeForFish(fish) switch {0=>2,1=>5,_=>10};
  public static string SizeName(int fish)=>SizeForFish(fish) switch {0=>"SMALL",1=>"MEDIUM",_=>"LARGE"};
  public int Window(double now)=>Mathf.Clamp((int)((now-Match.started)/60),0,2);
  public int Wanted(double now)=>(Window(now)+Match.wantedOffset)%SizeCount;
  public int Rank(ulong owner){var p=Player(owner);return p.HasValue?1+Players.Count(q=>q.score>p.Value.score):0;}
  public int CupRank(ulong owner){var p=Player(owner);return p.HasValue?1+Players.Count(q=>q.cupPoints>p.Value.cupPoints||q.cupPoints==p.Value.cupPoints&&q.cupScore>p.Value.cupScore):0;}
  public bool AllReady=>Players.Any(p=>p.connected)&&Players.Where(p=>p.connected).All(p=>p.ready&&!p.Busy);
  public void Begin(IEnumerable<ulong> owners,uint round,double now,bool competitive){
   var ids=owners.Distinct().ToArray();if(ids.Length>5)throw new ArgumentException("Fishing supports at most five participants.");
   Match=new FishingMatchSnapshot{round=round,phase=competitive?FishingRoundPhase.Playing:FishingRoundPhase.Practice,mode=FishingMode.Round,started=now,deadline=competitive?now+MatchSeconds:0};Players.Clear();Fish.Clear();remoteHookCasts.Clear();
   foreach(var id in ids)Players.Add(new FishingPlayerRecord{owner=id,fish=-1,pier=-1,connected=true});for(int i=0;i<FishCount;i++)Fish.Add(new FishingFishRecord{id=i});
   DealSkills(round);if(!competitive)for(int i=0;i<Players.Count;i++){var p=Players[i];p.item=Match.offer0;Players[i]=p;}
  }
  public bool Start(IEnumerable<ulong> owners,uint round,double now,FishingMode mode){
   var ids=owners.Distinct().OrderBy(i=>i).ToArray();if(ids.Length<1||ids.Length>5||mode==FishingMode.Crew&&ids.Length<2||!ids.SequenceEqual(Players.Where(p=>p.connected).Select(p=>p.owner).OrderBy(i=>i))||Match.phase==FishingRoundPhase.Playing||Preparing)return false;
   bool continuing=mode==FishingMode.Cup&&Match.mode==mode&&Match.phase==FishingRoundPhase.Ended&&Match.cupValid&&!Match.cupFinished&&ids.SequenceEqual(Players.Select(p=>p.owner).OrderBy(i=>i));
   var old=Players.ToArray();int cup=continuing?Match.cupRound+1:1;byte offset=(byte)(Match.phase==FishingRoundPhase.Ended?(Match.wantedOffset+1)%SizeCount:0);Begin(ids,round,now,false);
   for(int i=0;i<Players.Count;i++){var p=Players[i];p.ready=true;p.item=FishingAbility.None;if(continuing){var previous=old.First(q=>q.owner==p.owner);p.cupPoints=previous.cupPoints;p.cupScore=previous.cupScore;}Players[i]=p;}
   Match=new FishingMatchSnapshot{round=round,phase=FishingRoundPhase.Drafting,mode=mode,draftUntil=now+FishingAbilityRules.DraftSeconds,cupRound=cup,wantedOffset=offset,cupValid=mode==FishingMode.Cup,crewTarget=mode==FishingMode.Crew?CrewPointsPerPlayer*ids.Length:0};DealSkills(round);return true;
  }
  public void Join(ulong id){if(Match.phase!=FishingRoundPhase.Practice&&Match.phase!=FishingRoundPhase.Ended)return;if(Players.Any(p=>p.owner==id))return;Players.RemoveAll(p=>!p.connected);if(Players.Count<5){Players.Add(new FishingPlayerRecord{owner=id,fish=-1,pier=-1,connected=true});Match.cupValid=false;}}
  public void Disconnect(ulong id,double now){int i=Players.FindIndex(p=>p.owner==id);if(i<0)return;var p=Players[i];Release(p,false,now);remoteHookCasts.Remove(id);p.connected=p.held=p.ready=false;p.phase=FishingPhase.Ready;p.sequence++;Players[i]=p;Match.cupValid=false;
   if(Preparing||Match.mode==FishingMode.Crew&&Match.phase==FishingRoundPhase.Playing)End(now,FishingOutcome.Abandoned);
  }
  void Release(FishingPlayerRecord p,bool caught,double now){if(p.fish<0||p.fish>=Fish.Count)return;var f=Fish[p.fish];if(!f.claimed||f.owner!=p.owner)return;f.claimed=false;f.respawnAt=caught?now+7:0;Fish[p.fish]=f;}
  void Finish(ref FishingPlayerRecord p,FishingPhase phase,FishingResult reason,double now){
   Release(p,phase==FishingPhase.Caught,now);remoteHookCasts.Remove(p.owner);p.phase=phase;p.result=reason;p.phaseAt=now;p.held=false;
   switch(reason){case FishingResult.Missed:p.missedBites++;break;case FishingResult.Snapped:p.snappedLines++;break;case FishingResult.Cancelled:p.cancels++;break;case FishingResult.TimedOut:p.timeouts++;break;}
   if(phase!=FishingPhase.Caught)return;
   int window=Window(now);int size=SizeForFish(p.fish);int bonus=Match.phase==FishingRoundPhase.Playing&&size==Wanted(now)&&(p.wantedMask&(1<<window))==0?3:0;
   if(size==0)p.smallCatches++;else if(size==1)p.mediumCatches++;else p.largeCatches++;p.bonusTotal+=bonus;
   if(bonus>0)p.wantedMask|=(byte)(1<<window);p.lastBonus=bonus;p.lastAward=Points(p.fish)+bonus;p.score+=p.lastAward;p.catches++;p.awardSequence++;p.catchMask|=(byte)(1<<size);
   if(Match.mode==FishingMode.Crew&&Match.phase==FishingRoundPhase.Playing){Match.crewScore+=p.lastAward;Match.crewMask|=(byte)(1<<size);}
  }
  public bool Act(ulong owner,uint round,uint sequence,FishingAction action,int pier,Vector3 origin,double now,int chosenFish=-1,float castDistance=7,double pressedAt=double.NaN,bool remoteHook=false,Vector3? fishWorld=null,float waterSurface=0){
   if(!double.IsFinite(now))return false;Advance(now,0);int index=Players.FindIndex(p=>p.owner==owner);if(round!=Match.round||index<0)return false;var p=Players[index];if(!p.connected||sequence!=p.sequence)return false;
   if(action==FishingAction.Ready||action==FishingAction.Unready){if(Match.phase!=FishingRoundPhase.Practice&&Match.phase!=FishingRoundPhase.Ended||p.Busy)return false;p.ready=action==FishingAction.Ready;Players[index]=p;return true;}
   if(!Running)return false;
   switch(action){
    case FishingAction.Cast:
     if(p.phase!=FishingPhase.Ready||pier<0||pier>=PierCount||!float.IsFinite(origin.x)||!float.IsFinite(origin.y)||!float.IsFinite(origin.z)||!float.IsFinite(castDistance)||castDistance<1.2f||castDistance>MaxCastDistance)return false;
     int fish=chosenFish;
     if(fish<0||fish>=Fish.Count||Fish[fish].claimed||now<Fish[fish].respawnAt)return false;
     Fish[fish]=new FishingFishRecord{id=fish,claimed=true,owner=owner};p.fish=fish;p.pier=pier;p.origin=origin;p.sequence++;p.casts++;p.phase=FishingPhase.Casting;p.phaseAt=now;p.biteAt=now+1.7+SizeForFish(fish)*.65;p.hookUntil=p.biteAt+HookSeconds;p.result=FishingResult.None;p.progress=0;p.tension=.18f;p.held=p.ready=false;p.castDistance=p.distance=p.lineLength=castDistance;p.velocity=p.strain=p.surge=0;p.fightHeading=fishWorld.HasValue?Vector3.ProjectOnPlane(fishWorld.Value-origin,Vector3.up).normalized:Vector3.forward;p.fightOffset=new Vector2(0,fishWorld.HasValue?waterSurface-fishWorld.Value.y:.64f+SizeForFish(fish)*.12f);p.fightMotion=Vector2.zero;if(remoteHook)remoteHookCasts[owner]=p.sequence;else remoteHookCasts.Remove(owner);break;
    case FishingAction.Hook:
     double hookAt=remoteHook?pressedAt:now;
     if(!double.IsFinite(hookAt)||hookAt>now+HookClockTolerance||now-hookAt>HookTransitSeconds)return false;
     hookAt=Math.Min(hookAt,now);
     if(p.phase!=FishingPhase.Bite||hookAt<p.biteAt||hookAt>=p.hookUntil||remoteHook&&(!remoteHookCasts.TryGetValue(owner,out var castSequence)||castSequence!=sequence))return false;
     remoteHookCasts.Remove(owner);p.phase=FishingPhase.Reeling;p.phaseAt=now;p.held=false;p.steer=0;p.steerAt=now;p.reelBeat=-1;p.tapGrade=FishingTapGrade.None;p.pull=Pull(p,now);p.hooks++;if(fishWorld.HasValue){var reach=Vector3.ProjectOnPlane(fishWorld.Value-p.origin,Vector3.up);p.fightHeading=reach.normalized;p.castDistance=p.distance=p.lineLength=Mathf.Clamp(reach.magnitude,1.2f,MaxCastDistance);p.fightOffset.y=waterSurface-fishWorld.Value.y;}break;
    case FishingAction.ReelStart: return false; // Both directional holds remain explicit.
    case FishingAction.ReelTap: return false; // Retired timing edge cannot renew a hold.
    case FishingAction.ReelLeft:
    case FishingAction.ReelRight:
    case FishingAction.ReelStop:
     if(p.phase!=FishingPhase.Reeling)return false;
     double edge=double.IsNaN(pressedAt)&&!remoteHook?now:pressedAt;
     if(!double.IsFinite(edge)||edge<p.phaseAt||edge>now+HookClockTolerance||now-edge>LeaseSeconds||edge<p.steerAt)return false;
     p.steerAt=Math.Min(edge,now);p.steer=(sbyte)(action==FishingAction.ReelLeft?-1:action==FishingAction.ReelRight?1:0);
     p.held=p.steer!=0;p.reelLease=now+LeaseSeconds;break;
    case FishingAction.Cancel:if(!p.Busy)return false;Finish(ref p,FishingPhase.Escaped,FishingResult.Cancelled,now);p.sequence++;break;
    default:return false;
   }
   Players[index]=p;return true;
  }
  public void Abandon(double now){if(Preparing||Match.phase==FishingRoundPhase.Playing)End(now,FishingOutcome.Abandoned);}
  void End(double now,FishingOutcome outcome){
   if(Match.phase==FishingRoundPhase.Ended)return;bool scored=Match.phase==FishingRoundPhase.Playing;Match.phase=FishingRoundPhase.Ended;Match.outcome=outcome;Match.ended=now;remoteHookCasts.Clear();
   if(outcome==FishingOutcome.Success)Match.medal=(byte)(Match.deadline-now>=40?3:Match.deadline-now>=20?2:1);
   if(scored&&Match.mode==FishingMode.Cup&&Match.cupValid&&outcome==FishingOutcome.Finished){for(int i=0;i<Players.Count;i++){var p=Players[i];p.cupPoints+=Players.Count+1-Rank(p.owner);p.cupScore+=p.score;Players[i]=p;}Match.cupFinished=Match.cupRound==3;}
   for(int i=0;i<Players.Count;i++){var p=Players[i];Release(p,false,now);p.held=p.ready=false;if(p.Busy)p.phase=FishingPhase.Ready;p.sequence++;p.item=p.active=FishingAbility.None;p.activeUntil=p.sonarUntil=p.cooldownUntil=0;p.schoolFishMask=0;p.skillPicked=p.bubbleGuard=false;Players[i]=p;}
  }
  public void Advance(double now,float dt){
   if(!double.IsFinite(now)||!float.IsFinite(dt)||dt<0)return;
   if(Match.phase==FishingRoundPhase.Drafting){
    if(Players.Any(p=>!p.skillPicked)&&now<Match.draftUntil)return;
    for(int i=0;i<Players.Count;i++){var p=Players[i];if(!p.skillPicked){p.item=Match.offer0;p.skillPicked=true;}Players[i]=p;}
    Match.phase=FishingRoundPhase.Countdown;Match.started=now+CountdownSeconds;Match.deadline=Match.started+MatchSeconds;return;
   }
   if(Match.phase==FishingRoundPhase.Countdown){if(now<Match.started)return;Match.phase=FishingRoundPhase.Playing;}
   if(!Running)return;
   if(Match.phase==FishingRoundPhase.Playing&&now>=Match.deadline){End(now,Match.mode==FishingMode.Crew?FishingOutcome.Failed:FishingOutcome.Finished);return;}
   for(int i=0;i<Players.Count;i++){
    var p=Players[i];if(!p.connected)continue;
    if(now>=p.activeUntil){p.active=FishingAbility.None;p.bubbleGuard=false;}
    if(p.phase==FishingPhase.Casting&&now>=p.phaseAt+.65)p.phase=FishingPhase.Waiting;
    if((p.phase==FishingPhase.Casting||p.phase==FishingPhase.Waiting)&&now>=p.biteAt)p.phase=FishingPhase.Bite;
    double hookResolveAt=p.hookUntil+(remoteHookCasts.TryGetValue(p.owner,out var remoteSequence)&&remoteSequence==p.sequence?HookTransitSeconds:0);
    if(p.phase==FishingPhase.Bite&&now>=hookResolveAt)Finish(ref p,FishingPhase.Escaped,FishingResult.Missed,now);
    if(p.phase==FishingPhase.Reeling){
     if(now>=p.reelLease){p.held=false;p.steer=0;}
     // Long public-pool casts use the same spring/drag fight as a 12 m cast.
     // Convert only solver distances, then restore world units for rod/camera.
     // This keeps a distant target from multiplying catch time or strain.
     float worldCast=p.castDistance,rangeScale=Mathf.Max(1,(worldCast-1.5f)/10.5f);
     p.castDistance=1.5f+(worldCast-1.5f)/rangeScale;
     p.distance=1.5f+(p.distance-1.5f)/rangeScale;p.lineLength=1.5f+(p.lineLength-1.5f)/rangeScale;p.velocity/=rangeScale;
     p.pull=Pull(p,now);bool countering=p.held&&p.steer==CounterSide(p,now);
     float step=Mathf.Min(dt,.05f),size=SizeForFish(p.fish);
     p.surge=Mathf.Pow(Mathf.Max(0,Mathf.Sin((float)(now-p.phaseAt)*(1.7f-.2f*size)+p.fish*.8f)),6);
     float mass=.45f+.2f*size,pull=.8f+.65f*size+p.surge*(.36f+.73f*size),speed=(size==0?1.48f:size==1?1.05f:.73f)*1.06f*1.2f;
     bool boosted=p.active==FishingAbility.PowerReel&&now<p.activeUntil,steel=p.active==FishingAbility.SteelWire&&now<p.activeUntil,buddy=p.active==FishingAbility.FishingBuddy&&now<p.activeUntil;
     // Limited early winding assist: mainly trims longer fights, without automatic progress.
      if(now-p.phaseAt<5)speed+=.18f;
     if(boosted)speed*=1.55f;if(buddy)speed*=1.45f;
     // Restore continuous winding. Catch progress still comes from the real
     // spring/drag line simulation, rather than a timed stroke or score shortcut.
     p.lineLength=Mathf.Clamp(p.lineLength+(countering?-speed:p.held?.04f:.16f)*step,.9f,p.castDistance+1);
     // The reel drag slips under a sudden correct-side load. Keep the real
     // spring force and accumulated strain, but leave time to release and cool.
     if(countering)p.lineLength=Mathf.Clamp(Mathf.Max(p.lineLength,p.distance-Mathf.Max(.08f,.86f-p.strain)*6.85f/8),.9f,p.castDistance+1);
     float stretch=Mathf.Max(0,p.distance-p.lineLength);
     p.velocity=Mathf.Clamp((p.velocity+(pull-stretch*8)/mass*step)/(1+1.5f/mass*step),-2.5f,1.5f);
     p.distance=Mathf.Clamp(p.distance+p.velocity*step,.9f,p.castDistance+1.2f);
     // A little more recovery room, while large surges still punish uninterrupted reeling.
     float strainRate=countering?(.09f+.05f*size)*.94f:p.held?.15f+.075f*size:-.35f;
     if(strainRate>0){if(steel)strainRate=0;if(buddy)strainRate*=.6f;}
     if(p.bubbleGuard&&p.surge>.75f){p.strain=Mathf.Max(0,p.strain-.35f);p.bubbleGuard=false;p.activeUntil=now;}
     p.strain=Mathf.Clamp01(p.strain+strainRate*step);FightMotion(ref p,step,mass);
     p.tension=Mathf.Clamp01(Mathf.Max(0,p.distance-p.lineLength)*8/6.85f+p.strain);
     if(steel){
      // A slipping protected spool limits the physical load as well as the HUD.
      p.tension=Mathf.Min(p.tension,p.steelTension);p.strain=Mathf.Min(p.strain,p.tension);
      p.lineLength=Mathf.Max(p.lineLength,p.distance-Mathf.Max(0,p.tension-p.strain)*6.85f/8);
     }
     p.progress=Mathf.Clamp01((p.castDistance-p.distance)/Mathf.Max(.3f,p.castDistance-1.5f));
     p.castDistance=worldCast;p.distance=Mathf.Max(.9f,1.5f+(p.distance-1.5f)*rangeScale);p.lineLength=Mathf.Max(.9f,1.5f+(p.lineLength-1.5f)*rangeScale);p.velocity*=rangeScale;
     if(p.tension>=1)Finish(ref p,FishingPhase.Escaped,FishingResult.Snapped,now);
     else if(now-p.phaseAt>=30)Finish(ref p,FishingPhase.Escaped,FishingResult.TimedOut,now);
     else if(p.progress>=1)Finish(ref p,FishingPhase.Caught,FishingResult.Landed,now);
    }
    if((p.phase==FishingPhase.Caught||p.phase==FishingPhase.Escaped)&&now>=p.phaseAt+2){p.phase=FishingPhase.Ready;p.fish=-1;}
    Players[i]=p;
   }
   if(Match.phase==FishingRoundPhase.Playing&&Match.mode==FishingMode.Crew&&Match.crewScore>=Match.crewTarget&&Match.crewMask==7&&Players.All(p=>p.catches>0))End(now,FishingOutcome.Success);
  }
  public void Receive(FishingMatchSnapshot match,IEnumerable<FishingPlayerRecord> players,IEnumerable<FishingFishRecord> fish){Match=match;remoteHookCasts.Clear();Players.Clear();Players.AddRange(players);Fish.Clear();Fish.AddRange(fish);}
  public void Receive(FishingMatchSnapshot match,NetworkList<FishingPlayerRecord> players,NetworkList<FishingFishRecord> fish){Match=match;remoteHookCasts.Clear();Players.Clear();for(int i=0;i<players.Count;i++)Players.Add(players[i]);Fish.Clear();for(int i=0;i<fish.Count;i++)Fish.Add(fish[i]);}
  public void Clear(){Match=default;Players.Clear();Fish.Clear();remoteHookCasts.Clear();}
 }
}
