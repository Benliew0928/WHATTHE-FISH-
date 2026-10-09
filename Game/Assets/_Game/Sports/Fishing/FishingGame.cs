using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 [DefaultExecutionOrder(60)]
 public sealed class FishingGame:MonoBehaviour {
  public static FishingGame Instance {get;private set;}
  static uint nextRound;
  public readonly FishingState State=new();
  public FishingLagoonView Lagoon {get;private set;}
  public FishingPresentation Presentation {get;private set;}
  bool wasContext;float publishAt;uint recordedRound;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public bool ReviewSnapshot;
#endif
  public bool Authority=>!NetworkManager.Singleton||!NetworkManager.Singleton.IsListening||NetworkManager.Singleton.IsServer;
  public double Now=>NetworkManager.Singleton&&NetworkManager.Singleton.IsListening?NetworkManager.Singleton.ServerTime.Time:Time.timeAsDouble;
  public bool Context=>AppRoot.Instance&&AppRoot.Instance.Exploring&&AppRoot.Instance.SelectedSport==SportId.Fishing&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling)&&(!NetworkManager.Singleton||!NetworkManager.Singleton.IsListening||NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.Exploring.Value&&NetworkAthlete.HostPlayer.WorldSport.Value==SportId.Fishing);
  public static ulong Key(Athlete actor){var n=actor?actor.GetComponent<NetworkAthlete>():null;return n&&n.IsSpawned?n.OwnerClientId:0;}
  public string PlayerName(ulong id){var actor=Athlete.Active.FirstOrDefault(a=>a&&Key(a)==id);var net=actor?actor.GetComponent<NetworkAthlete>():null;string name=net&&net.Nickname.Value.Length>0?net.Nickname.Value.ToString():!AppRoot.Instance.rooms.Connected&&id==0?NetworkAthlete.CleanNickname(PlayerPrefs.GetString("fishing.nickname","YOU")):id==0?"HOST":"FRIEND "+id;return name.Length>0?name:"YOU";}
  Athlete[] Eligible()=>Athlete.Active.Where(a=>a&&a.isActiveAndEnabled&&!a.inTransit&&(AppRoot.Instance.rooms.Connected?a.GetComponent<NetworkAthlete>()&&a.GetComponent<NetworkAthlete>().IsSpawned:a==AppRoot.Instance.LocalAthlete)).OrderBy(Key).ToArray();
  public int ParticipantCount=>Eligible().Length;
  public bool CanStart(FishingMode mode)=>Authority&&Context&&State.Match.phase!=FishingRoundPhase.Playing&&State.Match.phase!=FishingRoundPhase.Countdown&&ParticipantCount<=5&&ParticipantCount>0&&(mode==FishingMode.Round&&!AppRoot.Instance.rooms.Connected||ParticipantCount>=2)&&State.AllReady;
  void Awake(){Lagoon=GetComponent<FishingLagoonView>();Presentation=gameObject.AddComponent<FishingPresentation>();Presentation.Bind(this);}
  void OnEnable(){Instance=this;}
  void OnDisable(){State.Clear();Presentation?.Clear();wasContext=false;if(Authority)Publish();if(Instance==this)Instance=null;}
  public FishingPlayerRecord? Player(Athlete actor)=>actor?State.Player(Key(actor)):null;
  public Vector3 Pier(int index)=>transform.TransformPoint(Lagoon.standingPositions[index]);
  public int NearestPier(Athlete actor){
   if(!actor||actor.inTransit||actor.Airborne||actor.LoadingJump||!actor.Grounded)return -1;
   int nearest=-1;float best=3.6f*3.6f;
   for(int i=0;i<Lagoon.standingPositions.Length;i++){var d=actor.transform.position-Pier(i);float sq=d.x*d.x+d.z*d.z;if(sq<best&&Mathf.Abs(d.y)<2.5f){nearest=i;best=sq;}}
   return nearest;
  }
  public bool Busy(Athlete actor)=>Context&&Player(actor).HasValue&&Player(actor).Value.Busy;
  public bool Available(Athlete actor,int fish){int pier=NearestPier(actor);return Context&&State.Running&&fish>=0&&fish<State.Fish.Count&&FishingState.PierForFish(fish)==pier&&!State.Fish[fish].claimed&&State.Fish[fish].respawnAt<=Now&&Vector3.ProjectOnPlane(FishPosition(fish,Now)-actor.transform.position,Vector3.up).magnitude<=12&&!Physics.Linecast(actor.transform.position+Vector3.up*1.4f,FishPosition(fish,Now),1<<8,QueryTriggerInteraction.Ignore);}
  public bool CanCast(Athlete actor){var p=Player(actor);return Context&&State.Running&&p.HasValue&&p.Value.phase==FishingPhase.Ready&&State.Fish.Any(f=>Available(actor,f.id));}
  public bool CanCast(Athlete actor,int fish)=>CanCast(actor)&&Available(actor,fish);
  public int PickFish(Athlete actor,Camera camera,Vector2 screenPoint,bool touch){
   if(!camera||!CanCast(actor))return -1;int selected=-1;float scale=Screen.height/900f,radius=(touch?48:25)*Mathf.Max(.65f,scale),best=radius*radius;
   for(int i=0;i<State.Fish.Count;i++){
    if(!Available(actor,i))continue;var point=Presentation.TargetPosition(i);var screen=camera.WorldToScreenPoint(point);
    if(screen.z<camera.nearClipPlane||screen.x<0||screen.y<0||screen.x>Screen.width||screen.y>Screen.height)continue;
    if(Physics.Linecast(camera.transform.position,point,1<<8,QueryTriggerInteraction.Ignore))continue;
    float distance=Vector2.SqrMagnitude(screenPoint-(Vector2)screen);if(distance<best){best=distance;selected=i;}
   }
   return selected;
  }
  public bool VisibleTarget(Athlete actor,Camera camera,int fish){
   if(!camera||!Available(actor,fish))return false;var point=Presentation.TargetPosition(fish);var screen=camera.WorldToViewportPoint(point);
   return screen.z>camera.nearClipPlane&&screen.x>=0&&screen.x<=1&&screen.y>=0&&screen.y<=1&&!Physics.Linecast(camera.transform.position,point,1<<8,QueryTriggerInteraction.Ignore);
  }
  // Angular targeting has the same tolerance on a phone and desktop. Keep a
  // valid target until another fish is clearly closer, avoiding swimming flicker.
  public int PickAimedFish(Athlete actor,Camera camera,Vector2 screenPoint,int current,float acquireDegrees,float releaseDegrees){
   if(!camera||!CanCast(actor))return -1;var ray=camera.ScreenPointToRay(screenPoint);int selected=-1;float best=acquireDegrees,currentAngle=float.MaxValue;
   for(int i=0;i<State.Fish.Count;i++){
    if(!VisibleTarget(actor,camera,i))continue;float angle=Vector3.Angle(ray.direction,Presentation.TargetPosition(i)-ray.origin);
    if(i==current)currentAngle=angle;if(angle<best){best=angle;selected=i;}
   }
   if(currentAngle<=releaseDegrees&&(selected<0||currentAngle<=best+.8f))return current;return selected;
  }
  public float Heading(Athlete actor){var p=Player(actor);if(!p.HasValue||p.Value.pier<0)return actor.transform.eulerAngles.y;var d=FishPosition(p.Value.fish,Now)-actor.transform.position;return Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg;}
  public float WaterY {get;private set;}
  public float WaterHeight(Vector3 world){var p=transform.InverseTransformPoint(world);float t=(float)Now;float fade=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(24,27.8f,new Vector2(p.x,p.z).magnitude));return WaterY+(Mathf.Sin(p.x*.92f+p.z*.46f+t*1.1f)*.023f+Mathf.Sin(p.z*1.27f-p.x*.34f-t*.85f)*.013f)*fade;}
  public Vector3 FishPosition(int fish,double now){
   int pier=Mathf.Clamp(FishingState.PierForFish(fish),0,Lagoon.standingPositions.Length-1),size=FishingState.SizeForFish(fish),row=fish%FishingState.FishPerPier/FishingState.SizeCount;var point=Pier(pier);var centre=transform.position;var radial=Vector3.ProjectOnPlane(point-centre,Vector3.up).normalized;var side=Vector3.Cross(Vector3.up,radial);
   float t=(float)(now*.55+fish*2.7);var p=point-radial*(5.8f+row*1.45f+size*.55f+Mathf.Cos(t)*.32f)+side*((size-1)*2.1f+(row==0?-.35f:.35f)+Mathf.Sin(t)*.42f);
   p.y=WaterY-.58f-size*.14f+Mathf.Sin(t*1.4f)*.08f;return p;
  }
  public bool StartMatch(FishingMode mode=FishingMode.Round){if(!CanStart(mode))return false;if(!State.Start(Eligible().Select(Key),++nextRound,Now,mode))return false;PlayerView.Instance?.ClearMatchInput();Publish();return true;}
  void BeginPractice(){var actors=Eligible();State.Begin(actors.Length<=5?actors.Select(Key):Array.Empty<ulong>(),++nextRound,Now,false);PlayerView.Instance?.ClearMatchInput();Publish();}
  public bool Execute(Athlete actor,FishingAction action,uint round,uint sequence,int fish=-1){
   if(!Authority||!Context||!actor||actor.inTransit||!Eligible().Contains(actor))return false;
   var p=Player(actor);if(!p.HasValue)return false;int pier=action==FishingAction.Cast?NearestPier(actor):p.Value.pier;
   if(action==FishingAction.Cast&&(!CanCast(actor,fish)||Physics.Linecast(actor.transform.position+Vector3.up*1.4f,FishPosition(fish,Now),1<<8,QueryTriggerInteraction.Ignore)))return false;
   if(action!=FishingAction.Cancel&&action!=FishingAction.ReelStop&&action!=FishingAction.Cast&&action!=FishingAction.Ready&&action!=FishingAction.Unready&&pier!=NearestPier(actor))return false;
   float distance=action==FishingAction.Cast?Vector3.ProjectOnPlane(FishPosition(fish,Now)-actor.transform.position,Vector3.up).magnitude:7;
   bool accepted=State.Act(Key(actor),round,sequence,action,pier,actor.transform.position,Now,fish,distance);if(accepted)Publish();return accepted;
  }
  public void Input(Athlete actor,PlayerCommand command){if(command.fishingAction!=FishingAction.None)Execute(actor,command.fishingAction,command.fishingRound,command.fishingSequence,command.fishingFish);}
  void FixedUpdate(){
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(ReviewSnapshot)return;
#endif
   if(!Context){if(wasContext){State.Clear();Presentation.Clear();PlayerView.Instance?.ClearMatchInput();if(Authority)Publish();}wasContext=false;return;}
   var surface=transform.Find("Lagoon presentation/Transparent moving lagoon");WaterY=surface?surface.position.y:transform.position.y-.43f;
   if(!wasContext&&Authority)BeginPractice();wasContext=true;
   if(!Authority){var host=NetworkAthlete.HostPlayer;if(host){bool changed=State.Match.round!=host.FishingMatch.Value.round;State.Receive(host.FishingMatch.Value,host.FishingPlayers,host.FishingFish);if(changed)PlayerView.Instance?.ClearMatchInput();}RecordResult();return;}
   var actors=Eligible();
   if(State.Match.phase==FishingRoundPhase.Countdown&&actors.Any(a=>!State.Player(Key(a)).HasValue))State.Abandon(Now);
   if(actors.Length<=5&&(State.Match.phase==FishingRoundPhase.Practice||State.Match.phase==FishingRoundPhase.Ended))foreach(var actor in actors)State.Join(Key(actor));
   foreach(var p in State.Players.ToArray()){
    var actor=actors.FirstOrDefault(a=>Key(a)==p.owner);
    if(!actor){if(p.connected)State.Disconnect(p.owner,Now);continue;}
    if(p.Busy&&(NearestPier(actor)!=p.pier||Vector3.Distance(actor.transform.position,p.origin)>.85f))State.Act(p.owner,State.Match.round,p.sequence,FishingAction.Cancel,p.pier,p.origin,Now);
   }
   State.Advance(Now,Time.fixedDeltaTime);RecordResult();
   if(Time.unscaledTime>=publishAt){publishAt=Time.unscaledTime+.05f;Publish();}
  }
  void RecordResult(){
   if(State.Match.phase!=FishingRoundPhase.Ended||recordedRound==State.Match.round)return;recordedRound=State.Match.round;
   var p=Player(AppRoot.Instance.LocalAthlete);if(!p.HasValue||State.Match.outcome==FishingOutcome.Abandoned)return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(DevelopmentProbe.TurnCommandActive)return;
#endif
   int count=State.Players.Count;string key="fishing.v2.best."+State.Match.mode+"."+count;
   if(State.Match.mode!=FishingMode.Crew)PlayerPrefs.SetInt(key,Math.Max(PlayerPrefs.GetInt(key),p.Value.score));
   else if(State.Match.outcome==FishingOutcome.Success){float elapsed=(float)(State.Match.ended-State.Match.started);PlayerPrefs.SetFloat(key,Mathf.Min(PlayerPrefs.GetFloat(key,float.MaxValue),elapsed));}
   PlayerPrefs.Save();
  }
  public void ReturnToPractice(){if(!Authority||!Context||State.Match.phase==FishingRoundPhase.Playing||State.Match.phase==FishingRoundPhase.Countdown)return;BeginPractice();}
  void Publish(){var host=NetworkAthlete.HostPlayer;if(!Authority||!host||!host.IsSpawned)return;host.FishingMatch.Value=State.Match;Sync(host.FishingPlayers,State.Players);Sync(host.FishingFish,State.Fish);}
  static void Sync<T>(NetworkList<T> target,List<T> source)where T:unmanaged,IEquatable<T>{while(target.Count>source.Count)target.RemoveAt(target.Count-1);for(int i=0;i<source.Count;i++){if(i>=target.Count)target.Add(source[i]);else if(!target[i].Equals(source[i]))target[i]=source[i];}}
 }
}
