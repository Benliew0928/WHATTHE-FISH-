using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace WhatTheFish {
 [DefaultExecutionOrder(-200)]
 public sealed partial class BasketballMatch:MonoBehaviour {
  public static BasketballMatch Instance {get;private set;}
  public readonly BasketballMatchState State=new();
  public readonly FootballTeamSelection Selection=new();
  readonly Dictionary<ulong,Athlete> participants=new();
  readonly System.Random random=new();
  BasketballBall ball;bool wasContext;uint observed=uint.MaxValue,published;static uint nextRevision;
  BasketballMatchPhase presented;Vector3 restartPoint;FootballTeam lastTouch;
  public double Now=>BasketballMotion.Clock;
  public bool Authority=>ball&&ball.Authority;
  public bool Room=>NetworkManager.Singleton&&NetworkManager.Singleton.IsListening;
  public bool Context=>ball&&ball.Playing&&(!Room||NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.Exploring.Value&&NetworkAthlete.HostPlayer.WorldSport.Value==SportId.Basketball);
  public BasketballMatchSnapshot Snapshot=>Authority?Capture():NetworkAthlete.HostPlayer?NetworkAthlete.HostPlayer.BasketballMatch.Value:default;
  public bool Competitive=>Context&&Snapshot.phase!=BasketballMatchPhase.Idle;
  public bool Active=>Context&&Snapshot.phase!=BasketballMatchPhase.Idle&&Snapshot.phase!=BasketballMatchPhase.Finished&&Snapshot.phase!=BasketballMatchPhase.Cancelled;
  public bool Running=>Snapshot.phase==BasketballMatchPhase.Regulation||Snapshot.phase==BasketballMatchPhase.Overtime;
  public bool BoundaryReleased=>Context&&(Snapshot.phase==BasketballMatchPhase.Finished||Snapshot.phase==BasketballMatchPhase.Cancelled);
  public bool RestartReady=>Snapshot.phase==BasketballMatchPhase.Restart&&Now>=Snapshot.deadline;
  public bool Live=>!Competitive||Running&&Now<Snapshot.deadline||RestartReady;
  public bool BallLive=>Live||Competitive&&Snapshot.phase==BasketballMatchPhase.Buzzer;
  public static bool BlocksActions=>Instance&&Instance.Context&&!Instance.Live;
  public static bool BlocksMovement=>Instance&&Instance.Context&&(Instance.Snapshot.phase==BasketballMatchPhase.Countdown||Instance.Snapshot.phase==BasketballMatchPhase.Restart&&!Instance.RestartReady||Instance.Snapshot.phase==BasketballMatchPhase.Buzzer);
  public double Remaining=>Running?Math.Max(0,Snapshot.deadline-Now):Snapshot.remaining;
  public bool MatchMode=>Room||BasketballBots.Instance&&BasketballBots.Instance.Solo;
  public bool CanStart=>Authority&&MatchMode&&Context&&Roster().Count()>=4&&Roster().Count()<=10&&Roster().Count()%2==0&&!Active;
  public static ulong PlayerId(Athlete actor){var n=actor?actor.GetComponent<NetworkAthlete>():null;if(n&&n.IsSpawned)return n.BotSlot.Value>0?(1UL<<63)+n.BotSlot.Value:n.StableId;var bot=actor?actor.GetComponent<BasketballBot>():null;return bot?(1UL<<63)+(ulong)bot.Slot:actor?(ulong)(uint)actor.GetInstanceID():ulong.MaxValue;}
  IEnumerable<Athlete> Roster()=>Athlete.Active.Where(a=>a&&a.isActiveAndEnabled&&!a.IsSpectator&&!a.inTransit&&(!Room||a.GetComponent<NetworkAthlete>() is NetworkAthlete n&&n.IsSpawned&&NetworkManager.Singleton.ConnectedClients.ContainsKey(n.OwnerClientId)));
  public bool Contains(Athlete actor)=>actor&&participants.TryGetValue(PlayerId(actor),out var entry)&&entry==actor;
  public FootballTeam TeamOf(Athlete actor){if(!actor)return FootballTeam.None;var n=actor.GetComponent<NetworkAthlete>();return n&&n.IsSpawned?n.BasketballTeam.Value:Selection.TeamOf(PlayerId(actor));}
  public FootballTeam TeamOf(ulong id){if(Authority)return Selection.TeamOf(id);foreach(var actor in Athlete.Active)if(PlayerId(actor)==id)return TeamOf(actor);return FootballTeam.None;}
  void Awake(){ball=GetComponent<BasketballBall>();BasketballMVP.Create();}
  void OnDestroy(){if(BasketballMVP.Instance)Destroy(BasketballMVP.Instance.gameObject);}
  void OnEnable(){Instance=this;}
  void OnDisable(){if(Authority)Clear();if(Instance==this)Instance=null;}
  BasketballMatchSnapshot Capture()=>new BasketballMatchSnapshot{phase=State.Phase,result=State.Result,possession=State.Possession,restart=State.RestartReason,scoreA=State.ScoreA,scoreB=State.ScoreB,deadline=State.Deadline,remaining=State.Remaining(Now),shotDeadline=State.ShotDeadline,revision=published,teamA=Selection.Count(FootballTeam.A),teamB=Selection.Count(FootballTeam.B),capacity=Selection.Capacity};
  void Publish(){PublishStatistics();if(Authority&&NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.IsSpawned)NetworkAthlete.HostPlayer.BasketballMatch.Value=Capture();}
  public bool StartMatch(){if(!CanStart)return false;return Begin(Roster().OrderBy(PlayerId).ToArray(),MenuMatchRules.BasketballSeconds,10);}
  bool Begin(Athlete[] roster,double seconds,double selection){
   if(!Authority||!Context||Active||roster.Length<4||!Selection.Begin(roster.Select(PlayerId)))return false;
   Statistics.Clear();participants.Clear();foreach(var actor in roster)participants.Add(PlayerId(actor),actor);
   if(BasketballBots.Instance&&BasketballBots.Instance.Solo){Selection.Choose(PlayerId(AppRoot.Instance.LocalAthlete),FootballTeam.A);foreach(var mate in roster.Where(BasketballBots.IsBot).Take(Selection.Capacity-1))Selection.Choose(PlayerId(mate),FootballTeam.A);}
   SyncTeams();lastTouch=FootballTeam.None;State.Start(Now,seconds,selection);ball.ResetMatchBall(true);Synchronize();wasContext=true;return true;
  }
  public bool ChooseTeam(Athlete actor,FootballTeam team,uint revision){
   if(!Authority||!Context||State.Phase!=BasketballMatchPhase.TeamSelection||Now>=State.Deadline||revision!=published||!Contains(actor)||!Selection.Choose(PlayerId(actor),team))return false;
   SyncTeams();Publish();return true;
  }
  public void RequestTeam(FootballTeam team){var actor=AppRoot.Instance.LocalAthlete;if(!actor)return;var n=actor.GetComponent<NetworkAthlete>();if(n&&n.IsSpawned){if(n.IsOwner)n.ChooseBasketballTeamRpc(team,Snapshot.revision);}else ChooseTeam(actor,team,Snapshot.revision);}
  void SyncTeams(){RefreshStatisticsRoster();foreach(var pair in participants){var n=pair.Value?pair.Value.GetComponent<NetworkAthlete>():null;if(n&&n.IsSpawned&&n.IsServer)n.BasketballTeam.Value=Selection.TeamOf(pair.Key);}}
  void ClearInputs(){PlayerView.Instance?.ClearMatchInput();foreach(var actor in Athlete.Active){actor.ResetLocomotion();actor.GetComponent<NetworkAthlete>()?.ClearMatchInput();}}
  public void Clear(){Statistics.Clear();State.Clear();Selection.Clear();participants.Clear();lastTouch=FootballTeam.None;foreach(var actor in Athlete.Active){var n=actor.GetComponent<NetworkAthlete>();if(n&&n.IsSpawned&&n.IsServer)n.BasketballTeam.Value=FootballTeam.None;}ball.ResetMatchBall(false);Synchronize();}
  bool RosterValid()=>participants.All(p=>p.Value&&p.Value.isActiveAndEnabled&&!p.Value.inTransit&&(!Room||p.Value.GetComponent<NetworkAthlete>() is NetworkAthlete n&&n.IsSpawned&&NetworkManager.Singleton.ConnectedClients.ContainsKey(n.OwnerClientId)));
  public void Cancel(){if(!Authority||!State.Active)return;State.Cancel();Synchronize();}
  void FixedUpdate(){
   if(!Authority){if(observed!=Snapshot.revision){observed=Snapshot.revision;if(BlocksActions)PlayerView.Instance?.ClearMatchInput();}return;}
   if(!Context){if(wasContext||State.Phase!=BasketballMatchPhase.Idle)Clear();wasContext=false;return;}wasContext=true;
   if(!State.Active)return;
   if(!RosterValid()){Cancel();return;}
   if(State.Phase==BasketballMatchPhase.TeamSelection&&Now>=State.Deadline){Selection.Complete(random);SyncTeams();if(!PlaceFormation()){Cancel();return;}ball.Place(Vector3.up*.25f,Quaternion.identity,Vector3.zero,Vector3.zero);State.Confirm(Now);Synchronize();}
  }
  public void AfterBallStep(){if(!Authority||!Context)return;State.Advance(Now);Synchronize();}
  void Synchronize(){
   if(observed!=State.Revision){
    bool continuous=presented==BasketballMatchPhase.Regulation&&State.Phase==BasketballMatchPhase.Overtime||presented==BasketballMatchPhase.Restart&&State.Running;
    observed=State.Revision;presented=State.Phase;if(!continuous)published=++nextRevision;
    if(!continuous)ClearInputs();
    if(State.Phase==BasketballMatchPhase.Restart){Statistics.ResetPlay();if(!PrepareRestart()){State.Cancel();Synchronize();return;}}
    else if(State.Phase==BasketballMatchPhase.Cancelled||State.Phase==BasketballMatchPhase.Finished){Statistics.ResetPlay();ball.ResetMatchBall(false);}
   }
   Publish();
  }
  public Transform AttackingHoop(Athlete actor)=>Hoop(TeamOf(actor));
  public Transform Hoop(FootballTeam attack){
   if(attack==FootballTeam.None)return null;float sign=attack==FootballTeam.A?1:-1;
   Transform best=null;float distance=0;
   foreach(var h in ball.transform.parent.GetComponentsInChildren<BasketballHoop>()){float z=ball.transform.parent.InverseTransformPoint(h.transform.position).z*sign;if(z>distance){distance=z;best=h.transform;}}
   return best;
  }
  public void Touch(Athlete actor){if(!Authority||(!State.Running&&State.Phase!=BasketballMatchPhase.Buzzer)||!Contains(actor))return;lastTouch=TeamOf(actor);Statistics.Touch(PlayerId(actor),Now,false);}
  public void Control(Athlete actor){if(!Competitive||!Authority||!Contains(actor))return;Touch(actor);if(State.CanAct(Now))Statistics.Touch(PlayerId(actor),Now,true);State.Control(TeamOf(actor),Now);}
  public bool Release(Athlete actor){if(!Competitive)return true;if(!Authority||!Contains(actor))return false;Touch(actor);return State.Release(Now);}
  public void Miss(){if(Competitive&&Authority){State.Miss();Statistics.Miss();}}
  public void Rim(Transform hoop){if(Competitive&&Authority&&hoop==Hoop(State.Possession))State.Rim(Now);}
  public void Basket(Transform hoop,int points,ulong? shooter=null){
   if(!Competitive||!Authority||!hoop)return;
   FootballTeam scoring=ball.transform.parent.InverseTransformPoint(hoop.position).z>0?FootballTeam.A:FootballTeam.B;
   if(State.Basket(scoring,points,Now))Statistics.Basket(scoring,points,Now,shooter);
  }
  public bool Boundary(Vector3 exit,bool recovery=false){
   if(!Competitive||!Authority)return false;
   if(State.Phase==BasketballMatchPhase.Buzzer){State.Miss();return true;}
   if(!State.CanAct(Now))return true;
   restartPoint=exit;var team=lastTouch==FootballTeam.None?BasketballMatchState.Other(State.Possession):BasketballMatchState.Other(lastTouch);
   State.Restart(team,recovery?BasketballRestart.Recovery:BasketballRestart.Boundary,Now);return true;
  }
  bool Ground(Vector3 local,out Vector3 world){
   world=ball.transform.parent.TransformPoint(local);
   if(!Physics.Raycast(world+Vector3.up*2,Vector3.down,out var hit,5,1<<8,QueryTriggerInteraction.Ignore)||hit.normal.y<.9f)return false;
   world=hit.point+Vector3.up*.02f;return true;
  }
  void Place(Athlete actor,Vector3 world){
   var facing=Hoop(TeamOf(actor));var direction=facing?Vector3.ProjectOnPlane(facing.position-world,Vector3.up):Vector3.forward;
   var rotation=direction.sqrMagnitude>.01f?Quaternion.LookRotation(direction):Quaternion.identity;
   bool enabled=actor.capsule.enabled;actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(world,rotation);actor.ResetLocomotion();
   var n=actor.GetComponent<NetworkTransform>();if(n&&n.IsSpawned)n.Teleport(world,rotation,actor.transform.localScale);actor.capsule.enabled=enabled;
  }
  bool PlaceFormation(){
   var slots=new[]{new Vector2(-2.5f,4),new Vector2(2.5f,4),new Vector2(0,7),new Vector2(-4,10),new Vector2(4,10)};
   var poses=new List<(Athlete actor,Vector3 world)>();
   foreach(var team in new[]{FootballTeam.A,FootballTeam.B}){
    var ids=Selection.AllocateKickoffSlots(team,random);
    for(int i=0;i<ids.Length;i++){if(!participants.TryGetValue(ids[i],out var a)||!a||!Ground(new Vector3(slots[i].x,0,slots[i].y*(team==FootballTeam.A?-1:1)),out var world))return false;poses.Add((a,world));}
   }
   foreach(var pose in poses)Place(pose.actor,pose.world);Physics.SyncTransforms();return true;
  }
  bool PrepareRestart(){
   ball.ResetMatchBall(false,preserveInventory:true);if(!PlaceFormation())return false;
   var limit=BasketballBall.PaintedCourtLimits;
   // Right from the court facing the conceding hoop, closer to the stand
   // than the midpoint between hoop centre and baseline corner.
   float sign=State.Possession==FootballTeam.A?-1:1;
   var point=State.RestartReason==BasketballRestart.Basket?new Vector3(sign*2,0,sign*limit.y):State.RestartReason==BasketballRestart.ShotClock?new Vector3(limit.x,0,0):restartPoint;
   var recipient=participants.Values.Where(a=>TeamOf(a)==State.Possession).OrderBy(PlayerId).FirstOrDefault();
   if(!recipient||!ball.BeginInbound(recipient,point))return false;
   lastTouch=State.Possession;return true;
  }
  public bool OpenInbound(Athlete actor){
   if(!Authority||!ball.IsInbound(actor)||!State.OpenInbound(TeamOf(actor),Now))return false;
   lastTouch=TeamOf(actor);Synchronize();return true;
  }
  public bool RestrictedHalf(Athlete actor)=>Competitive&&Snapshot.phase==BasketballMatchPhase.Restart&&TeamOf(actor)!=FootballTeam.None&&TeamOf(actor)!=Snapshot.possession;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public bool ReviewStart(Athlete[] actors,double seconds=180,double selection=0)=>!Room&&Begin(actors,seconds,selection);
#endif
 }
}
