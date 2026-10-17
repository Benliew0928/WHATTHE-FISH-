using System;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public sealed partial class FootballMatch {
  public const double ReplayBefore=4,ReplayAfter=1,ReplayCaptureSeconds=1.3,ReplaySeconds=5;
  public const double ReplayWipeInSeconds=.3,ReplayWipeHoldSeconds=1,ReplayWipeOutSeconds=.3;
  public const double ReplayReturnInSeconds=.25,ReplayReturnHoldSeconds=.3,ReplayReturnOutSeconds=.25;
  public const double ReplayWipeSeconds=ReplayWipeInSeconds+ReplayWipeHoldSeconds+ReplayWipeOutSeconds,ReplayReturnSeconds=ReplayReturnInSeconds+ReplayReturnHoldSeconds+ReplayReturnOutSeconds;
  public const double ReplayStartOffset=ReplayCaptureSeconds+ReplayWipeSeconds,ReplayEndOffset=ReplayStartOffset+ReplaySeconds,GoalSequenceSeconds=ReplayEndOffset+ReplayReturnSeconds;
  // Camera switches and centre reset happen while the card fully covers the field.
  public const double ReplayEnterHandoff=ReplayCaptureSeconds+ReplayWipeInSeconds+ReplayWipeHoldSeconds*.5,ReplayReturnHandoff=ReplayEndOffset+ReplayReturnInSeconds+ReplayReturnHoldSeconds*.5;
  double goalAt;uint goalId;FootballTeam kickoffTeam;ulong kickoffPlayer=ulong.MaxValue;bool goalEffectsCleared;
  public bool GoalSequence=>Context&&Snapshot.goalId!=0&&Now<Snapshot.goalAt+GoalSequenceSeconds;
  public bool AwaitingKickoff=>Context&&Snapshot.phase==FootballMatchPhase.Kickoff;
  public bool KickoffReady=>AwaitingKickoff&&Now>=Snapshot.deadline&&!GoalSequence;
  public Athlete KickoffActor=>AwaitingKickoff?Athlete.Active.FirstOrDefault(a=>a&&PlayerId(a)==Snapshot.kickoffPlayer):null;
  public static bool CanAimKickoff(Athlete actor)=>actor&&actor.isActiveAndEnabled&&!actor.inTransit&&Instance&&Instance.KickoffReady&&Instance.TeamOf(actor)==Instance.Snapshot.kickoffTeam&&PlayerId(actor)==Instance.Snapshot.kickoffPlayer;
  public static bool KickoffInput(Athlete actor,PlayerCommand command){
   var m=Instance;if(!m||!m.AwaitingKickoff)return false;
   if(m.Authority&&CanAimKickoff(actor)){
    m.AimKickoff(actor,command.heading);
    if(command.kick)m.TryKickoff(actor,command.heading,command.kickCharge,m.Snapshot.revision);
   }
   return true;
  }
  public Vector3 AttackDirection(FootballTeam team){
   if(!ball.Pitch||ball.GoalCount<2)return Vector3.forward;
   int goal=Enumerable.Range(0,ball.GoalCount).First(i=>ball.GoalSign(i)==(team==FootballTeam.A?1:-1));
   return Vector3.ProjectOnPlane(ball.Pitch.TransformPoint(ball.GoalBounds(goal).center)-ball.KickoffPosition,Vector3.up).normalized;
  }
  void ChooseKickoff(){
   var members=participants.Values.Where(a=>a&&TeamOf(a)==kickoffTeam).OrderBy(PlayerId).ToArray();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   // The explicit one-person diagnostic has no opposing team to take a restart.
   if(members.Length==0&&participants.Count==1){members=participants.Values.ToArray();kickoffTeam=TeamOf(members[0]);}
#endif
   // Solo players always take their team's restart; room teammates rotate the duty.
   var human=members.Where(a=>!FootballBots.IsBot(a)).ToArray();
   var candidates=human.Length>0?human:members;
   kickoffPlayer=candidates.Length>0?PlayerId(candidates[(int)(goalId%(uint)candidates.Length)]):ulong.MaxValue;
  }
  void KickoffPose(float heading,out Vector3 position,out Quaternion rotation){
   rotation=Quaternion.Euler(0,heading,0);position=ball.KickoffPosition-rotation*Vector3.forward*.8f;
   position.y=ball.Pitch.TransformPoint(new Vector3(0,ball.PitchBounds.max.y,0)).y+.02f;
  }
  void AimKickoff(Athlete actor,float heading){
   if(!float.IsFinite(heading)||!actor||!actor.capsule)return;
   KickoffPose(heading,out var position,out var rotation);
   bool enabled=actor.capsule.enabled;actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(position,rotation);actor.capsule.enabled=enabled;
   actor.Motor.Reset(heading);actor.speed=0;
  }
  public bool TryKickoff(Athlete actor,float heading,float charge,uint revision){
   if(!Authority||!CanAimKickoff(actor)||revision!=Snapshot.revision||!float.IsFinite(heading)||!float.IsFinite(charge)||!actor.capsule||!actor.capsule.enabled||!actor.Grounded||ball.PortalBusy||Vector3.Distance(ball.Body.position,ball.KickoffPosition)>.05f)return false;
   AimKickoff(actor,heading);Physics.SyncTransforms();
   State.Advance(Now);ClearInputs();ball.SetMatchDynamic(true);ResetDetection();
   // Normal range/contact and shot-speed rules still own the actual kick.
   bool kicked=actor.TryKick(Mathf.Clamp01(charge));
   if(!kicked)Debug.LogError("Validated centre kickoff failed its contact check.",this);
   Publish();return kicked;
  }
  public float BotKickoffHeading(Athlete actor){var direction=AttackDirection(TeamOf(actor));return Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg;}
  void ClearGoalSequence(){goalAt=0;goalId=0;goalEffectsCleared=false;kickoffPlayer=ulong.MaxValue;kickoffTeam=FootballTeam.None;}
 }
}
