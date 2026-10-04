using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
namespace WhatTheFish {
 [DefaultExecutionOrder(-200)]
 public sealed class FootballMatch:MonoBehaviour {
  public static FootballMatch Instance {get;private set;}
  public FootballMatchRules rules;
  public readonly FootballMatchState State=new();
  public readonly FootballTeamSelection Selection=new();
  FootballBall ball;FootballGoalDetector[] goals;bool wasContext;uint presentedRevision=uint.MaxValue;
  readonly Dictionary<ulong,Athlete> participants=new();
  readonly System.Random kickoffRandom=new();
  static uint nextRoundRevision;uint publishedRevision,observedRuleRevision=uint.MaxValue;
  FootballMatchPhase publishedPhase;
  public const double GoalFollowThroughSeconds=.65;
  double goalFollowThroughUntil;
  public double Now=>NetworkManager.Singleton&&NetworkManager.Singleton.IsListening?NetworkManager.Singleton.ServerTime.FixedTime:Time.fixedTimeAsDouble;
  public bool Authority=>ball&&ball.HasAuthority;
  public bool Context=>AppRoot.Instance&&AppRoot.Instance.Exploring&&AppRoot.Instance.SelectedSport==SportId.Football&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling)&&(!NetworkManager.Singleton||!NetworkManager.Singleton.IsListening||NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.Exploring.Value&&NetworkAthlete.HostPlayer.WorldSport.Value==SportId.Football);
  public FootballMatchSnapshot Snapshot=>Authority?Capture():NetworkAthlete.HostPlayer?NetworkAthlete.HostPlayer.Match.Value:default;
  public static bool BlocksMovement=>Instance&&Instance.Context&&(Instance.Snapshot.phase==FootballMatchPhase.Kickoff||Instance.Snapshot.phase==FootballMatchPhase.GoalReset);
  public static bool BlocksActions=>Instance&&Instance.Context&&Instance.Snapshot.phase!=FootballMatchPhase.Idle&&!Instance.Running;
  public static bool GoalFollowThrough {get{if(!Instance||!Instance.Context)return false;var s=Instance.Snapshot;return (s.phase==FootballMatchPhase.GoalReset||s.phase==FootballMatchPhase.Finished)&&s.deadline>Instance.Now;}}
  public bool Running=>Snapshot.phase==FootballMatchPhase.Regulation||Snapshot.phase==FootballMatchPhase.Overtime;
  public double Remaining {get{var s=Snapshot;return s.phase==FootballMatchPhase.Regulation||s.phase==FootballMatchPhase.Overtime||s.phase==FootballMatchPhase.Kickoff||s.phase==FootballMatchPhase.TeamSelection?Math.Max(0,s.deadline-Now):s.remaining;}}
  public int EligiblePlayers=>Eligible().Count();
  public bool CanStart=>Authority&&Context&&EligiblePlayers>=2&&EligiblePlayers<=10&&EligiblePlayers%2==0;
  static ulong PlayerId(Athlete actor){var net=actor.GetComponent<NetworkAthlete>();return net&&net.IsSpawned?net.OwnerClientId:ulong.MaxValue;}
  static IEnumerable<Athlete> Eligible()=>Athlete.Active.Where(a=>a&&a.isActiveAndEnabled&&!a.inTransit);
  void Awake(){ball=GetComponent<FootballBall>();if(!rules)rules=Resources.Load<FootballMatchRules>("FootballMatchRules");if(!rules)throw new InvalidOperationException("Missing FootballMatchRules resource.");}
  void OnEnable(){Instance=this;}
  void OnDisable(){if(Authority)ClearMatch();participants.Clear();Selection.Clear();goals=null;if(Instance==this)Instance=null;}
  FootballMatchSnapshot Capture(){
   if(observedRuleRevision!=State.Revision){
    // Regulation -> overtime is the same live round: in-flight requests stay valid.
    bool continuous=publishedPhase==FootballMatchPhase.Regulation&&State.Phase==FootballMatchPhase.Overtime;
    observedRuleRevision=State.Revision;publishedPhase=State.Phase;if(!continuous)publishedRevision=++nextRoundRevision;
   }
   return new FootballMatchSnapshot{phase=State.Phase,result=State.Result,scoreA=State.ScoreA,scoreB=State.ScoreB,deadline=goalFollowThroughUntil>0?goalFollowThroughUntil:State.Deadline,remaining=State.Running||State.Phase==FootballMatchPhase.Kickoff||State.Phase==FootballMatchPhase.TeamSelection?0:State.Remaining(Now),revision=publishedRevision,teamA=Selection.Count(FootballTeam.A),teamB=Selection.Count(FootballTeam.B),teamCapacity=Selection.Capacity};
  }
  void Publish(){if(Authority&&NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.IsSpawned)NetworkAthlete.HostPlayer.Match.Value=Capture();}
  public FootballTeam TeamOf(Athlete actor){if(!actor)return FootballTeam.None;var net=actor.GetComponent<NetworkAthlete>();return net&&net.IsSpawned?net.Team.Value:Selection.TeamOf(PlayerId(actor));}
  void SyncTeams(){foreach(var pair in participants){if(!pair.Value)continue;var net=pair.Value.GetComponent<NetworkAthlete>();if(net&&net.IsSpawned)net.Team.Value=Selection.TeamOf(pair.Key);}}
  public bool StartMatch()=>BeginMatch(false);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  // Explicit local-only exception; normal Start Match always enforces an even roster.
  public bool StartTestMatch()=>!NetworkManager.Singleton||!NetworkManager.Singleton.IsListening?BeginMatch(true):false;
#endif
  bool BeginMatch(bool singlePlayerTest){
   if(!Authority||!Context||State.Phase!=FootballMatchPhase.Idle&&State.Phase!=FootballMatchPhase.Finished||!ball.Pitch||ball.GoalCount!=2)return false;
   var roster=Eligible().OrderBy(PlayerId).ToArray();if(!Selection.Begin(roster.Select(PlayerId),singlePlayerTest))return false;
   participants.Clear();foreach(var actor in roster)participants.Add(PlayerId(actor),actor);SyncTeams();
   goals=new FootballGoalDetector[ball.GoalCount];
   for(int i=0;i<goals.Length;i++){int sign=ball.GoalSign(i);float paintedEdge=(sign>0?ball.PitchBounds.max.z:ball.PitchBounds.min.z)+sign*rules.goalLineWidth*.5f;float line=sign*Mathf.Max(sign*paintedEdge,sign*ball.GoalFront(i));goals[i]=new FootballGoalDetector(ball.GoalBounds(i),line,sign);}
   goalFollowThroughUntil=0;State.SelectTeams(Now,rules.teamSelectionSeconds,rules.regulationSeconds,rules.overtimeSeconds,rules.kickoffSeconds);ClearInputs();wasContext=true;Publish();return true;
  }
  public bool ChooseTeam(Athlete actor,FootballTeam team,uint selectionRevision){
   if(!Authority||!Context||!actor||State.Phase!=FootballMatchPhase.TeamSelection||Now>=State.Deadline||selectionRevision!=Snapshot.revision)return false;
   if(!Selection.Choose(PlayerId(actor),team))return false;SyncTeams();Publish();return true;
  }
  public void RequestTeam(FootballTeam team){
   var actor=AppRoot.Instance.LocalAthlete;if(!actor)return;var net=actor.GetComponent<NetworkAthlete>();
   if(net&&net.IsSpawned){if(net.IsOwner)net.ChooseFootballTeamRpc(team,Snapshot.revision);}else ChooseTeam(actor,team,Snapshot.revision);
  }
  public void ClearMatch(){goalFollowThroughUntil=0;State.Clear();goals=null;participants.Clear();Selection.Clear();foreach(var actor in Athlete.Active){var net=actor.GetComponent<NetworkAthlete>();if(net&&net.IsSpawned&&net.IsServer)net.Team.Value=FootballTeam.None;}ClearInputs();ball.ResetBall();Publish();}
  void ClearInputs(){PlayerView.Instance?.ClearMatchInput();foreach(var actor in Athlete.Active){actor.ResetLocomotion();actor.GetComponent<NetworkAthlete>()?.ClearMatchInput();}}
  public bool TryKickoffPose(FootballTeam team,int teamSize,int slot,out Vector3 position,out Quaternion rotation){
   position=default;rotation=Quaternion.identity;var formation=rules.Formation(teamSize);
   if(!ball.Pitch||formation==null||formation.Length!=teamSize||slot<0||slot>=teamSize||(team!=FootballTeam.A&&team!=FootballTeam.B))return false;
   // Goal signs retain existing team ownership; use their pitch-space centres for the axis.
   int aGoal=Enumerable.Range(0,ball.GoalCount).First(i=>ball.GoalSign(i)<0),bGoal=Enumerable.Range(0,ball.GoalCount).First(i=>ball.GoalSign(i)>0);
   var axis=Vector3.ProjectOnPlane(ball.GoalBounds(bGoal).center-ball.GoalBounds(aGoal).center,Vector3.up).normalized;
   var side=Vector3.Cross(Vector3.up,axis);var offset=formation[slot];
   if(float.IsNaN(offset.x)||float.IsInfinity(offset.x)||float.IsNaN(offset.y)||float.IsInfinity(offset.y)||offset.y<=0||axis.sqrMagnitude<.9f)return false;
   var centre=ball.Pitch.InverseTransformPoint(ball.KickoffPosition);
   var local=centre+(side*offset.x+axis*offset.y)*(team==FootballTeam.A?-1:1);
   local.y=ball.PitchBounds.max.y;
   if(local.x<ball.PitchBounds.min.x+1||local.x>ball.PitchBounds.max.x-1||local.z<ball.PitchBounds.min.z+1||local.z>ball.PitchBounds.max.z-1)return false;
   var point=ball.Pitch.TransformPoint(local);
   if(!Physics.Raycast(point+Vector3.up*3,Vector3.down,out var ground,6,1<<8,QueryTriggerInteraction.Ignore)||ground.normal.y<.9f)return false;
   if(Mathf.Abs(ball.Pitch.InverseTransformPoint(ground.point).y-ball.PitchBounds.max.y)>.2f)return false;
   position=ground.point+Vector3.up*.02f;
   var facing=Vector3.ProjectOnPlane(ball.KickoffPosition-position,Vector3.up);
   if(facing.magnitude<=ball.controlDistance+ball.WorldRadius+1)return false;
   rotation=Quaternion.LookRotation(facing,Vector3.up);return true;
  }
  bool ResetRound(){
   var placements=new List<(Athlete actor,Vector3 position,Quaternion rotation)>();
   foreach(var team in new[]{FootballTeam.A,FootballTeam.B}){
    var members=Selection.AllocateKickoffSlots(team,kickoffRandom);
    for(int slot=0;slot<members.Length;slot++){
     if(!participants.TryGetValue(members[slot],out var actor)||!actor||!actor.capsule||!TryKickoffPose(team,Selection.Capacity,slot,out var position,out var rotation))return InvalidFormation();
     // Validate the entire plan before teleporting anyone. Participants will all move together.
     float radius=actor.capsule.radius*Mathf.Max(Mathf.Abs(actor.transform.lossyScale.x),Mathf.Abs(actor.transform.lossyScale.z));
     float height=Mathf.Max(radius*2,actor.capsule.height*Mathf.Abs(actor.transform.lossyScale.y));
     if(placements.Any(p=>Vector3.Distance(p.position,position)<radius+p.actor.capsule.radius+.2f))return InvalidFormation();
     foreach(var contact in Physics.OverlapCapsule(position+Vector3.up*(radius+.01f),position+Vector3.up*(height-radius),radius,~0,QueryTriggerInteraction.Ignore)){
      if(contact.GetComponentInParent<FootballBall>()==ball)continue; // This ball also returns to its existing anchor.
      var player=contact.GetComponentInParent<Athlete>();if(player&&participants.Values.Contains(player))continue;
      return InvalidFormation();
     }
     placements.Add((actor,position,rotation));
    }
   }
   ClearInputs();ball.ResetBall();
   foreach(var placement in placements){
    var (actor,position,rotation)=placement;
    bool enabled=actor.capsule.enabled;actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(position,rotation);actor.ResetLocomotion();
    var net=actor.GetComponent<NetworkAthlete>();if(net&&net.IsSpawned&&net.IsServer)actor.GetComponent<NetworkTransform>().Teleport(position,rotation,actor.transform.localScale);
    actor.capsule.enabled=enabled;
   }
   Physics.SyncTransforms();ResetDetection();return true;
  }
  bool InvalidFormation(){Debug.LogError("Football kickoff formation is invalid or obstructed. Check FootballMatchRules formation offsets and pitch ground; match cancelled without relocating slots.",this);return false;}
  void ResetDetection(){if(goals!=null)foreach(var goal in goals)goal.Reset(ball.Pitch.InverseTransformPoint(ball.Body.position),Now);}
  void FixedUpdate(){
   if(!Authority){if(Snapshot.revision!=presentedRevision){presentedRevision=Snapshot.revision;if(BlocksActions)PlayerView.Instance?.ClearMatchInput();}return;}
   if(!Context){if(wasContext||State.Phase!=FootballMatchPhase.Idle)ClearMatch();wasContext=false;return;}wasContext=true;
   if(State.Phase==FootballMatchPhase.Idle)return;
   double now=Now;
   if(State.Phase==FootballMatchPhase.TeamSelection){
    // A changed roster requires a fresh selection, rather than silently changing capacities.
    if(participants.Any(p=>!p.Value||!p.Value.isActiveAndEnabled)||!Eligible().Select(PlayerId).OrderBy(id=>id).SequenceEqual(participants.Keys.OrderBy(id=>id))){ClearMatch();return;}
    if(now>=State.Deadline){Selection.Complete(kickoffRandom);SyncTeams();if(!ResetRound()){ClearMatch();return;}State.TeamsConfirmed(now);}
    Publish();return;
   }
   // Goals are ordered before timeout, using the swept whole-ball crossing time.
   if(State.Running&&goals!=null){
    var point=ball.Pitch.InverseTransformPoint(ball.Body.position);var scale=ball.Pitch.lossyScale;float radius=ball.WorldRadius;
    var r=new Vector3(radius/Mathf.Abs(scale.x),radius/Mathf.Abs(scale.y),radius/Mathf.Abs(scale.z));
    for(int i=0;i<goals.Length;i++)if(goals[i].Sample(point,r,now,out double crossedAt)){
     if(crossedAt>=State.Deadline)State.Advance(crossedAt);
     if(State.Goal(goals[i].Sign>0?FootballTeam.A:FootballTeam.B,crossedAt)){goalFollowThroughUntil=now+GoalFollowThroughSeconds;ClearInputs();break;}
    }
   }
   if(State.Phase==FootballMatchPhase.GoalReset&&now>=goalFollowThroughUntil){goalFollowThroughUntil=0;if(!ResetRound()){ClearMatch();return;}State.ResetCompleted(now);}
   if(State.Phase==FootballMatchPhase.Finished&&now>=goalFollowThroughUntil)goalFollowThroughUntil=0;
   var before=State.Phase;State.Advance(now);
   if(before==FootballMatchPhase.Kickoff&&State.Running){ClearInputs();ResetDetection();}
   // Regulation -> overtime remains continuous; a winning goal gets the same net follow-through.
   Publish();
  }
 }
}
