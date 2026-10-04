using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 [DefaultExecutionOrder(60)]
 public sealed class GolfMatchManager:MonoBehaviour {
  public static GolfMatchManager Instance {get;private set;}
  public GolfMatchState State {get;}=new GolfMatchState();
  public GolfCourse Course {get;private set;}
  public GolfHoleTrigger[] HoleTriggers {get;private set;}=Array.Empty<GolfHoleTrigger>();
  public uint Round {get;private set;}
  static uint nextRound;
  readonly Dictionary<ulong,Athlete> actors=new();readonly Dictionary<ulong,GolfBall> balls=new();
  readonly Dictionary<ulong,GolfTee> tees=new();
  readonly List<PendingContact> contacts=new();
  struct PendingContact {public Athlete actor;public GolfBall ball;public uint round,sequence,reset;public double at;public Vector3 velocity;}
  GameObject ballTemplate;float publishAt;bool wasContext;
  public bool Authority=>!NetworkManager.Singleton||!NetworkManager.Singleton.IsListening||NetworkManager.Singleton.IsServer;
  public double Now=>NetworkManager.Singleton&&NetworkManager.Singleton.IsListening?NetworkManager.Singleton.ServerTime.FixedTime:Time.fixedTimeAsDouble;
  public bool Context=>AppRoot.Instance&&AppRoot.Instance.Exploring&&AppRoot.Instance.SelectedSport==SportId.Golf&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling)&&(!NetworkManager.Singleton||!NetworkManager.Singleton.IsListening||NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.Exploring.Value&&NetworkAthlete.HostPlayer.WorldSport.Value==SportId.Golf);
  public bool CanStart=>Authority&&Context&&!State.Running&&Course.holes!=null&&Course.holes.Length==GolfPlayerState.HoleCount&&Eligible().Any();
  public double Remaining=>State.Remaining(Now);
  public GolfMatchSnapshot Snapshot=>new GolfMatchSnapshot{phase=State.Phase,round=Round,started=State.StartedAt,deadline=State.CountdownDeadline,firstFinisher=State.FirstFinisher};
  void Awake(){
   Course=GetComponent<GolfCourse>();
   if(Course.holes==null)return;
   HoleTriggers=Course.holes.Select(h=>{
    var existing=h.cup.Find("Golf ball capture");var node=existing?existing.gameObject:new GameObject("Golf ball capture");
    node.layer=2;node.transform.SetParent(h.cup,false);var trigger=node.GetComponent<GolfHoleTrigger>();if(!trigger)trigger=node.AddComponent<GolfHoleTrigger>();trigger.Bind(this,h);return trigger;
   }).ToArray();
  }
  void OnEnable(){Instance=this;}
  void OnDisable(){if(Authority)ClearMatch();else ClearViews();if(Instance==this)Instance=null;}
  IEnumerable<Athlete> Eligible(){
   if(!AppRoot.Instance)return Array.Empty<Athlete>();
   if(AppRoot.Instance.rooms.Connected)return Athlete.Active.Where(a=>a&&a.isActiveAndEnabled&&!a.inTransit&&a.GetComponent<NetworkAthlete>()&&a.GetComponent<NetworkAthlete>().IsSpawned).OrderBy(a=>a.GetComponent<NetworkAthlete>().OwnerClientId);
   var local=AppRoot.Instance.LocalAthlete;return local&&local.isActiveAndEnabled&&!local.inTransit?new[]{local}:Array.Empty<Athlete>();
  }
  public GolfPlayerState Player(Athlete actor){if(!actor)return null;foreach(var p in actors)if(p.Value==actor)return State.Player(p.Key);return null;}
  public GolfBall Ball(ulong owner)=>balls.TryGetValue(owner,out var ball)?ball:null;
  public GolfTee Tee(ulong owner)=>tees.TryGetValue(owner,out var tee)?tee:null;
  public GolfBall Ball(Athlete actor){var player=Player(actor);return player!=null?Ball(player.PlayerId):null;}
  public GolfHole Target(Athlete actor){var p=Player(actor);return p==null||p.IsFinished||p.IsDNF?null:Course.holes.FirstOrDefault(h=>h.number==p.CurrentHole);}
  public bool StartMatch(){
   if(!CanStart)return false;
   return Begin(Eligible().Select(a=>(GolfCartWorld.Key(a),a,a.GetComponent<NetworkAthlete>()?(GolfCartWorld.Key(a)==0?"Host":"Player "+GolfCartWorld.Key(a)):"You")).ToArray());
  }
  bool Begin((ulong id,Athlete actor,string name)[] roster){
   if(!Authority||!Context||State.Running||Course.holes.Length!=5||Course.holes.Select(h=>h.number).OrderBy(n=>n).SequenceEqual(new[]{1,2,3,4,5})==false||Course.holes.Any(h=>!h.cup||!h.tee))return false;
   var template=AppRoot.Instance.stadium.transform.Find("Golf Equipment/Ball");if(!template)return false;ballTemplate=template.gameObject;
   if(!State.Start(roster.Select(p=>new GolfEntrant(p.id,p.name)),Now))return false;
   ClearViews();Round=++nextRound;
   foreach(var p in roster){actors[p.id]=p.actor;p.actor.Setup();p.actor.GolfClubMotion.Equip(Round);var ball=CreateBall(p.id);ball.SetLive(true);ball.Place(TeePosition(1,p.id),true);}
   ClearInputs();wasContext=true;Publish();return true;
  }
  GolfBall CreateBall(ulong id){
   if(balls.TryGetValue(id,out var found)&&found)return found;
   if(!ballTemplate){var template=AppRoot.Instance.stadium.transform.Find("Golf Equipment/Ball");if(!template)return null;ballTemplate=template.gameObject;}
   // Reuse the authored meshes/materials without cloning the FBX's nested LOD
   // groups, which register the same renderers twice in the equipment showcase.
   var go=new GameObject("Golf ball · "+id);go.transform.SetParent(transform,false);
   var renderers=new Dictionary<Renderer,Renderer>();
   foreach(var source in ballTemplate.GetComponentsInChildren<MeshRenderer>(true)){
    var visual=new GameObject(source.name);visual.transform.SetParent(go.transform,false);
    visual.transform.localPosition=ballTemplate.transform.InverseTransformPoint(source.transform.position);
    visual.transform.localRotation=Quaternion.Inverse(ballTemplate.transform.rotation)*source.transform.rotation;
    var a=source.transform.lossyScale;var b=ballTemplate.transform.lossyScale;visual.transform.localScale=new Vector3(a.x/b.x,a.y/b.y,a.z/b.z);
    visual.AddComponent<MeshFilter>().sharedMesh=source.GetComponent<MeshFilter>().sharedMesh;var renderer=visual.AddComponent<MeshRenderer>();
    renderer.sharedMaterials=source.sharedMaterials;renderer.shadowCastingMode=source.shadowCastingMode;renderer.receiveShadows=source.receiveShadows;renderers.Add(source,renderer);
   }
   var lod=go.AddComponent<LODGroup>();lod.SetLODs(ballTemplate.GetComponent<LODGroup>().GetLODs().Select(l=>new LOD(l.screenRelativeTransitionHeight,l.renderers.Select(r=>renderers[r]).ToArray())).ToArray());lod.RecalculateBounds();
   var ball=go.AddComponent<GolfBall>();ball.Bind(this,id);balls[id]=ball;
   var tee=Instantiate(Resources.Load<GolfTee>("GolfTee"),transform);tee.name="Wooden opening tee · "+id;tee.transform.SetPositionAndRotation(TeeGroundPosition(1,id),Quaternion.identity);tees[id]=tee;
   return ball;
  }
  Vector3 TeeGroundPosition(int hole,ulong owner){
   var tee=Course.holes.First(h=>h.number==hole).tee;int slot=State.Players.Select(p=>p.PlayerId).OrderBy(id=>id).ToList().IndexOf(owner);
   var p=tee.position+tee.right*(.18f*(slot-(State.Players.Count-1)*.5f));
   if(Physics.Raycast(p+Vector3.up*2,Vector3.down,out var ground,4,1<<8,QueryTriggerInteraction.Ignore))p=ground.point;
   return p;
  }
  public Vector3 TeePosition(int hole,ulong owner)=>TeeGroundPosition(hole,owner)+Vector3.up*(GolfBall.Radius+.002f+(hole==1?GolfTee.SeatHeight:0));
  public bool CanSwing(Athlete actor){var p=Player(actor);return Context&&State.Running&&p!=null&&!p.IsFinished&&!p.IsDNF&&actor&&actor.GolfClubMotion&&actor.GolfClubMotion.RigReady&&!actor.GolfClubMotion.Busy&&!actor.inTransit&&!actor.Airborne&&!actor.LoadingJump&&actor.Grounded&&!GolfCartWorld.Driving(actor);}
  public GolfBall Strikeable(Athlete actor){
   if(!CanSwing(actor))return null;
   return balls.Values.Where(b=>CanStrike(actor,b)).OrderBy(b=>Vector3.SqrMagnitude(b.Body.position-actor.transform.position)).FirstOrDefault();
  }
  public bool CanStrike(Athlete actor,GolfBall ball)=>CanSwing(actor)&&ball&&Ball(ball.Owner)==ball&&ball.Live&&State.Player(ball.Owner)!=null&&!State.Player(ball.Owner).IsFinished&&!State.Player(ball.Owner).IsDNF&&InRange(actor,ball);
  bool InRange(Athlete actor,GolfBall ball)=>actor.GolfClubMotion.CanAddress(ball.Body.position)&&(!Physics.Linecast(actor.transform.position+Vector3.up*.5f,ball.Body.position+Vector3.up*.04f,out var hit,1<<8,QueryTriggerInteraction.Ignore)||hit.distance>Vector3.Distance(actor.transform.position+Vector3.up*.5f,ball.Body.position)-.07f);
  public void SetCharging(Athlete actor,bool value,ulong owner,float heading,uint round){
   if(!Authority||!actor||!actor.GolfClubMotion||round!=Round||!float.IsFinite(heading))return;
   var ball=Ball(owner);if(value&&!CanStrike(actor,ball))value=false;
   if(value)actor.GolfClubMotion.Address(ball.Body.position);
   actor.GolfClubMotion.Charge(value,heading,ball?ball.Body.position:Vector3.zero,round);
  }
  public bool TrySwing(Athlete hitter,ulong owner,float heading,float charge,uint round){
   if(!Authority||round!=Round||!float.IsFinite(heading)||!float.IsFinite(charge)||charge<0||charge>1)return false;
   State.Advance(Now);var ball=Ball(owner);if(!CanStrike(hitter,ball)){Publish();return false;}
   if(!State.RecordSwing(Player(hitter).PlayerId,Now))return false;
   var direction=Quaternion.Euler(0,heading%360,0)*Vector3.forward;
   // Charge controls a putt or lofted shot with Inspector-configured speed.
   var velocity=ball.PhysicsSettings.SwingVelocity(direction,charge);
   hitter.GolfClubMotion.Address(ball.Body.position);
   hitter.GolfClubMotion.Swing(heading%360,charge,ball.Body.position,Round);
   contacts.Add(new PendingContact{actor=hitter,ball=ball,round=Round,sequence=hitter.GolfClubMotion.State.sequence,reset=ball.ResetSequence,at=hitter.GolfClubMotion.State.started+GolfClubMotion.ContactTime,velocity=velocity});Publish();return true;
  }
  public GolfHoleResult EnterHole(GolfBall ball,int hole){
   if(!Authority||!Context||!ball||Ball(ball.Owner)!=ball||!ball.Live)return GolfHoleResult.Ignored;
   var result=State.EnterHole(ball.Owner,hole,Now);
   if(result==GolfHoleResult.Invalid)ball.Recover();
   else if(result==GolfHoleResult.Completed)ball.Place(TeePosition(State.Player(ball.Owner).CurrentHole,ball.Owner),true);
   else if(result==GolfHoleResult.Finished){ball.SetLive(false);if(AppRoot.Instance.LocalAthlete&&Player(AppRoot.Instance.LocalAthlete)?.IsFinished==true)PlayerView.Instance?.ClearMatchInput();}
   ApplyLive();Publish();return result;
  }
  public bool OutsideCourse(Vector3 world){
   if(!float.IsFinite(world.x)||!float.IsFinite(world.y)||!float.IsFinite(world.z))return true;
   var island=Course.GetComponentInParent<RefinedIslandEnvironment>();if(!island)return false;
   var p=island.transform.InverseTransformPoint(world);if(p.y<island.layout.sea_level-.5f)return true;
   var edges=island.layout.boundaries;if(edges==null||edges.Length<3)return false;bool inside=false;
   foreach(var e in edges)if((e.a.z>p.z)!=(e.b.z>p.z)&&p.x<(e.b.x-e.a.x)*(p.z-e.a.z)/(e.b.z-e.a.z)+e.a.x)inside=!inside;
   return !inside;
  }
  public void ClearMatch(){State.Clear();Round=++nextRound;ClearViews();ClearInputs();Publish();}
  void ClearViews(){contacts.Clear();foreach(var actor in actors.Values)if(actor&&actor.GolfClubMotion)actor.GolfClubMotion.ResetPose();foreach(var ball in balls.Values)if(ball){ball.SetLive(false);Destroy(ball.gameObject);}foreach(var tee in tees.Values)if(tee)Destroy(tee.gameObject);tees.Clear();balls.Clear();actors.Clear();}
  void ClearInputs(){PlayerView.Instance?.ClearMatchInput();foreach(var a in actors.Values)if(a)a.GetComponent<NetworkAthlete>()?.ClearMatchInput();}
  void ApplyLive(){foreach(var item in balls){var p=State.Player(item.Key);item.Value.SetLive(Context&&State.Running&&p!=null&&!p.IsFinished&&!p.IsDNF);}}
  void Publish(){
   if(!Authority)return;var host=NetworkAthlete.HostPlayer;if(!host||!host.IsSpawned||!host.IsServer)return;
   host.GolfMatch.Value=Snapshot;
   var roster=State.Players.ToArray();while(host.GolfPlayers.Count>roster.Length)host.GolfPlayers.RemoveAt(host.GolfPlayers.Count-1);
   for(int i=0;i<roster.Length;i++){var record=new GolfPlayerRecord{owner=roster[i].PlayerId,progress=GolfPlayerSnapshot.Capture(roster[i],Round)};if(i>=host.GolfPlayers.Count)host.GolfPlayers.Add(record);else if(!host.GolfPlayers[i].Equals(record))host.GolfPlayers[i]=record;}
   foreach(var a in actors){if(!a.Value)continue;var net=a.Value.GetComponent<NetworkAthlete>();if(net&&net.IsSpawned){net.GolfBall.Value=Ball(a.Key)?Ball(a.Key).Snapshot(Round,Now):default;net.GolfPose.Value=a.Value.GolfClubMotion.State;}}
  }
  void Receive(){
   var host=NetworkAthlete.HostPlayer;if(!host||!host.IsSpawned)return;var snapshot=host.GolfMatch.Value;
   if(snapshot.round!=Round){ClearViews();Round=snapshot.round;PlayerView.Instance?.ClearMatchInput();}
   var roster=new List<(ulong,GolfPlayerSnapshot)>();foreach(var p in host.GolfPlayers)roster.Add((p.owner,p.progress));State.Receive(snapshot,roster);
   foreach(var net in FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None)){
    if(!net.IsSpawned||State.Player(net.OwnerClientId)==null)continue;var actor=net.GetComponent<Athlete>();actors[net.OwnerClientId]=actor;actor.GolfClubMotion.Equip(Round);
    if(net.GolfBall.Value.valid&&net.GolfBall.Value.round==Round){var ball=CreateBall(net.OwnerClientId);if(ball)ball.Receive(net.GolfBall.Value);}
   }
  }
  void FixedUpdate(){
   if(!Authority){if(Context)Receive();return;}
   if(!Context){if(wasContext||State.Phase!=GolfMatchPhase.Idle)ClearMatch();wasContext=false;return;}wasContext=true;
   var phase=State.Phase;State.Advance(Now);ApplyLive();if(phase!=State.Phase)ClearInputs();
   for(int i=contacts.Count-1;i>=0;i--){var c=contacts[i];var p=Player(c.actor);
    bool valid=State.Running&&c.round==Round&&c.actor&&c.actor.isActiveAndEnabled&&!c.actor.inTransit&&!GolfCartWorld.Driving(c.actor)&&c.actor.GolfClubMotion.State.sequence==c.sequence&&p!=null&&!p.IsFinished&&!p.IsDNF&&c.ball&&c.ball.Live&&c.ball.ResetSequence==c.reset;
    if(!valid){contacts.RemoveAt(i);continue;}
    if(GolfClubMotion.Clock>=c.at){c.ball.Strike(c.velocity);contacts.RemoveAt(i);}
   }
   foreach(var actor in actors.Values)if(actor&&actor.GolfClubMotion.State.action==GolfClubAction.Charge){var ball=Strikeable(actor);if(!ball)actor.GolfClubMotion.Charge(false,0,Vector3.zero,Round);}
   if(Time.unscaledTime>=publishAt){publishAt=Time.unscaledTime+.05f;Publish();}
  }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public bool StartTestMatch((ulong id,Athlete actor,string name)[] roster)=>Begin(roster);
#endif
 }
}
