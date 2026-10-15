using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public enum FootballSound:byte { KickLight,KickShot,Dribble,Receive,Bounce,Roll,Post,Net,NetBrush,Deflection,Boundary,Walk,Run,Cut,Slide,Body,Fall,Land,Cloth,Swipe,Dash,Empty,Ready,Countdown,Spawn,Pickup,TurboStart,TurboLoop,TurboEnd,PortalOpen,PortalIn,PortalOut,PortalClose,Interrupt,WallPreview,WallPlace,WallHit,WallExpire,Kickoff,Final,Score,Overtime,Win,Loss,Draw,Air,UIClick,UIConfirm,UIBack,UIToggle,UISelect,UIError,UIOpen,UIClose }
 public struct FootballAudioEvent:INetworkSerializable {
  public uint sequence;public double time;public FootballSound cue;public Vector3 point;public float gain;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref sequence);s.SerializeValue(ref time);s.SerializeValue(ref cue);s.SerializeValue(ref point);s.SerializeValue(ref gain);}
 }
 // Outcomes are live host events. Gait, prop phases and match UI follow the
 // presented state; initial snapshots establish a baseline without replaying it.
 [DefaultExecutionOrder(115)]
 public sealed class FootballAudio:MonoBehaviour {
  public static FootballAudio Instance {get;private set;}
  public static bool Context=>Instance&&Instance.ball&&Instance.ball.AudioActive;
  static readonly string[] Keys={"kick-light","kick-shot","dribble-touch","receive","turf-bounce","turf-roll","goal-post","goal-net","net-brush-football","deflection","boundary","grass-walk","grass-run","grass-cut","tackle-slide","body","turf-fall","turf-land","cloth","swipe","dash-start","stamina-empty","stamina-ready","countdown","spawn","pickup","turbo-start","turbo-loop","turbo-end","portal-open","portal-in","portal-out","portal-close","interrupt","wall-preview","wall-place","wall-hit","wall-expire","kickoff-whistle","final-whistle","score","overtime","result-win","result-loss","result-draw","air","ui-click","ui-confirm","ui-back","ui-toggle","ui-select","ui-error","ui-open","ui-close"};
  sealed class Voice {public AudioSource source;public FootballSound cue;public float gain;public int priority;public double began;}
  sealed class Feet {public Vector3 position;public float phase,touch,yaw,speed;public bool airborne,dash,empty,charging;public double step=-10,cut=-10,fallAt=-1;public FootballAction action;}
  readonly Dictionary<FootballSound,AudioClip[]> bank=new();readonly Dictionary<FootballSound,int> takes=new();
  readonly List<Voice> voices=new();readonly Queue<FootballAudioEvent> pending=new();
  readonly Dictionary<Athlete,Feet> feet=new();readonly List<Athlete> removed=new();
  FootballBall ball;Transform voiceRoot;Voice air,turbo,roll;bool presenting,suspended;uint serial,lastReceived;ulong sender=ulong.MaxValue;
  FootballMatchSnapshot previousMatch;int countdown,propCountdown;double resultAt=-1;FootballSound resultCue;float priorTurbo;Vector3 priorBall;float ballSpeed;
  uint portalSequence;float portalPhase;bool portalClosed;
  public int LoadedClips {get{int n=0;foreach(var clips in bank.Values)n+=clips.Length;return n;}}
  public int PlayingVoices {get{int n=0;foreach(var v in voices)if(v.source.isPlaying)n++;return n;}}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public static Action<FootballSound,Vector3,float,string,uint> Trace;public static int NetworkReceived,NetworkDropped;
  public AudioClip ReviewClip(FootballSound cue)=>bank[cue][0];
  public float ReviewTurboVolume=>turbo.source.volume;public bool ReviewLoopsPlaying=>air.source.isPlaying||turbo.source.isPlaying||roll.source.isPlaying;
#endif
  void Awake(){
   Instance=this;ball=GetComponent<FootballBall>();voiceRoot=new GameObject("Football sound voices").transform;voiceRoot.SetParent(transform.parent,false);
   // The shared resources remain at their approved paths. Unity references the
   // same AudioClip objects; no duplicated shared files or normalization.
   var clips=new List<AudioClip>(Resources.LoadAll<AudioClip>("FootballAudio"));clips.AddRange(Resources.LoadAll<AudioClip>("BasketballAudio"));
   for(int i=0;i<Keys.Length;i++){
    var list=new List<AudioClip>();foreach(var clip in clips)if(Matches(clip.name,Keys[i])||i==(int)FootballSound.KickShot&&Matches(clip.name,"kick-alt")||i==(int)FootballSound.Run&&Matches(clip.name,"trail-alt"))list.Add(clip);
    list.Sort((a,b)=>string.CompareOrdinal(a.name,b.name));bank[(FootballSound)i]=list.ToArray();if(list.Count==0)Debug.LogError("Missing football sound "+Keys[i]);
   }
   for(int i=0;i<20;i++)voices.Add(NewVoice("Pitch voice "+i));air=NewVoice("Coastal air");turbo=NewVoice("Team Turbo");roll=NewVoice("Turf roll");
  }
  static bool Matches(string name,string key)=>name.Length==key.Length+3&&name.StartsWith(key+"-",StringComparison.Ordinal);
  Voice NewVoice(string name){var go=new GameObject(name);go.transform.SetParent(voiceRoot,false);var s=go.AddComponent<AudioSource>();s.playOnAwake=false;s.dopplerLevel=0;s.reverbZoneMix=0;s.rolloffMode=AudioRolloffMode.Custom;s.SetCustomCurve(AudioSourceCurveType.CustomRolloff,AnimationCurve.Linear(0,1,1,1));s.minDistance=1;s.maxDistance=100;return new Voice{source=s};}
  void OnEnable(){Instance=this;presenting=false;}
  void OnDisable(){StopAll();if(Instance==this)Instance=null;}
  void OnDestroy(){if(voiceRoot)Destroy(voiceRoot.gameObject);}
  void OnApplicationPause(bool value){suspended=value;if(value)StopAll();}
  public void StopAll(){foreach(var v in voices)v.source.Stop();air?.source.Stop();turbo?.source.Stop();roll?.source.Stop();pending.Clear();feet.Clear();presenting=false;resultAt=-1;lastReceived=0;sender=ulong.MaxValue;}
  public void BeginReplay(){foreach(var v in voices)v.source.Stop();turbo.source.Stop();roll.source.Stop();feet.Clear();}
  public void EndReplay(){foreach(var v in voices)if(!UI(v.cue))v.source.Stop();feet.Clear();priorBall=ball.transform.position;ballSpeed=0;}
  static bool UI(FootballSound c)=>c>=FootballSound.UIClick;
  static bool Flat(FootballSound c)=>UI(c)||c==FootballSound.Air||c>=FootballSound.Kickoff&&c<=FootballSound.Draw;
  static int Priority(FootballSound c)=>Flat(c)&&c!=FootballSound.Air?3:c==FootballSound.Walk||c==FootballSound.Run||c==FootballSound.Cut||c==FootballSound.Cloth||c==FootballSound.Dribble?0:c==FootballSound.Bounce?1:2;
  float Gain(Voice v){
   float level=UI(v.cue)?MenuPreferences.UIVolume:v.cue==FootballSound.Air?MenuPreferences.AmbienceVolume:MenuPreferences.SFXVolume;
   var actor=AppRoot.Instance?AppRoot.Instance.LocalAthlete:null;float distance=actor&&!Flat(v.cue)?Vector3.ProjectOnPlane(v.source.transform.position-actor.transform.position,Vector3.up).magnitude:0;
   return v.gain*level*Mathf.Lerp(1,.10f,Mathf.InverseLerp(5,50,distance));
  }
  AudioClip Choose(FootballSound cue){if(!bank.TryGetValue(cue,out var clips)||clips.Length==0)return null;takes.TryGetValue(cue,out int take);takes[cue]=(take+1)%clips.Length;return clips[take];}
  void Begin(Voice v,FootballSound cue,AudioClip clip,Vector3 point,float gain,bool loop){var s=v.source;s.Stop();v.cue=cue;v.gain=Mathf.Clamp(gain,0,1.4f);v.priority=Priority(cue);v.began=Time.unscaledTimeAsDouble;s.transform.position=point;s.clip=clip;s.pitch=1;s.loop=loop;s.spatialBlend=Flat(cue)?0:.5f;s.priority=128-v.priority*24;s.volume=Gain(v);s.Play();}
  public void Play(FootballSound cue,Vector3 point,float gain=1,string reason="pose",uint eventId=0){
   if(reason!="replay")FootballReplay.Instance?.RecordSound(cue,point,gain);
   if(!Context||suspended||gain<=0)return;var clip=Choose(cue);if(!clip)return;Voice voice=null;int priority=Priority(cue);
   for(int i=UI(cue)?16:0;i<(UI(cue)?20:16);i++){var v=voices[i];if(!v.source.isPlaying){voice=v;break;}if(v.priority<=priority&&(voice==null||v.priority<voice.priority||v.priority==voice.priority&&v.began<voice.began))voice=v;}
   if(voice==null)return;Begin(voice,cue,clip,point,gain,false);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   Trace?.Invoke(cue,point,gain,reason,eventId);
#endif
  }
  void Loop(Voice v,FootballSound cue,bool active,Vector3 point,float gain){if(!active){v.source.Stop();return;}if(!v.source.isPlaying){var clip=Choose(cue);if(clip)Begin(v,cue,clip,point,gain,true);}v.gain=gain;v.source.transform.position=point;v.source.volume=Gain(v);}
  public static void Emit(FootballSound cue,Vector3 point,float gain=1){
   if(!Context||!Instance.ball.HasAuthority)return;var a=Instance;var e=new FootballAudioEvent{sequence=++a.serial,time=FootballMotion.Clock,cue=cue,point=point,gain=Mathf.Clamp(gain,0,1.4f)};a.Play(cue,point,e.gain,"authority",e.sequence);
   var host=NetworkAthlete.HostPlayer;if(host&&host.IsSpawned&&host.IsServer)host.FootballAudioRpc(e);
  }
  public static void Receive(FootballAudioEvent e,ulong host){
   if(!Context||Instance.ball.HasAuthority)return;var a=Instance;if(a.sender!=host){a.sender=host;a.lastReceived=0;a.pending.Clear();}
   if(e.sequence<=a.lastReceived)return;
   if(!double.IsFinite(e.time)||!float.IsFinite(e.gain)||!float.IsFinite(e.point.sqrMagnitude)||(int)e.cue>=Keys.Length)return;
   a.lastReceived=e.sequence;double age=FootballMotion.Clock-e.time;
   if(age>.8||age<-.5||a.pending.Count>=64){
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    NetworkDropped++;
#endif
    return;
   }
   a.pending.Enqueue(e);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   NetworkReceived++;
#endif
  }
  public static bool PlayMenu(FootballSound cue=FootballSound.UIClick){if(!Context)return false;Instance.Play(cue,AppRoot.Instance&&AppRoot.Instance.LocalAthlete?AppRoot.Instance.LocalAthlete.transform.position:Vector3.zero,1,"ui");return true;}
  public static FootballSound ButtonCue(string name){
   if(name=="Pitch settings")return FootballHUD.Instance&&FootballHUD.Instance.MenuOpen?FootballSound.UIClose:FootballSound.UIOpen;
   if(name.StartsWith("Close",StringComparison.Ordinal))return FootballSound.UIClose;
   if(name.StartsWith("Cancel",StringComparison.Ordinal)||name.StartsWith("Back",StringComparison.Ordinal)||name=="Keep playing")return FootballSound.UIBack;
   if(name.StartsWith("Confirm",StringComparison.Ordinal)||name.Contains("Start"))return FootballSound.UIConfirm;
   if(name.Contains("tab")||name.Contains("navigation")||name.Contains("Team")||name=="Eye level"||name=="Follow"||name=="Stadium")return FootballSound.UISelect;
   if(name.Contains("guide")||name.Contains("Leave"))return FootballSound.UIOpen;
   return FootballSound.UIClick;
  }
  void LateUpdate(){
   if(!Context||suspended){if(presenting)StopAll();return;}bool first=!presenting;presenting=true;
   while(pending.Count>0&&FootballMotion.Clock>=pending.Peek().time+.05){var e=pending.Dequeue();if(FootballMotion.Clock-e.time<.8)Play(e.cue,e.point,e.gain,"network",e.sequence);}
   foreach(var v in voices)if(v.source.isPlaying)v.source.volume=Gain(v);
   if(!FootballReplay.Presenting){PresentFeet();PresentProps(first);}else {turbo.source.Stop();roll.source.Stop();}PresentMatch(first);
   var actor=AppRoot.Instance.LocalAthlete;var point=actor?actor.transform.position:ball.transform.position;Loop(air,FootballSound.Air,true,point,.55f);
   float speed=first?0:Vector3.ProjectOnPlane(ball.transform.position-priorBall,Vector3.up).magnitude/Mathf.Max(.001f,Time.deltaTime);priorBall=ball.transform.position;ballSpeed=Mathf.Lerp(ballSpeed,Mathf.Min(32,speed),.4f);
   bool rolling=!ball.CurrentController&&!ball.PortalBusy&&ballSpeed>.4f&&ballSpeed<8&&FootballBall.Allowed&&Physics.Raycast(ball.transform.position,Vector3.down,ball.WorldRadius+.05f,1<<8,QueryTriggerInteraction.Ignore);
   Loop(roll,FootballSound.Roll,rolling,ball.transform.position,Mathf.Clamp01(ballSpeed/5)*.45f);
  }
  void PresentMatch(bool first){
   var match=FootballMatch.Instance;var state=match.Snapshot;var actor=AppRoot.Instance.LocalAthlete;var point=ball.KickoffPosition;
   if(state.phase==FootballMatchPhase.Finished&&match.GoalSequence)state.phase=FootballMatchPhase.GoalReset;
   if(!first){
    if(state.scoreA+state.scoreB>previousMatch.scoreA+previousMatch.scoreB)Play(FootballSound.Score,point,1,"goal-confirmed");
    if(state.phase!=previousMatch.phase){
     if(state.phase==FootballMatchPhase.TeamSelection)PlayMenu(FootballSound.UIOpen);
     if(previousMatch.phase==FootballMatchPhase.Kickoff&&state.phase==FootballMatchPhase.Regulation)Play(FootballSound.Kickoff,point,1,"kickoff");
     if(state.phase==FootballMatchPhase.Overtime)Play(FootballSound.Overtime,point,1,"overtime");
     if(state.phase==FootballMatchPhase.Finished){Play(FootballSound.Final,point,1,"full-time");var team=match.TeamOf(actor);bool won=state.result==FootballMatchResult.TeamA&&team==FootballTeam.A||state.result==FootballMatchResult.TeamB&&team==FootballTeam.B;resultCue=state.result==FootballMatchResult.Draw||team==FootballTeam.None?FootballSound.Draw:won?FootballSound.Win:FootballSound.Loss;resultAt=FootballMotion.Clock+1.25;}
    }
   }
   if(state.phase!=FootballMatchPhase.Finished)resultAt=-1;
   if(resultAt>=0&&FootballMotion.Clock>=resultAt){Play(resultCue,point,1,"result");resultAt=-1;}
   int tick=state.phase==FootballMatchPhase.Kickoff?Mathf.CeilToInt((float)Math.Max(0,state.deadline-match.Now)):0;
   if(!first&&tick>0&&tick<=3&&tick!=countdown)Play(FootballSound.Countdown,point,.8f,"kickoff-countdown");countdown=tick;previousMatch=state;
  }
  void PresentProps(bool first){
   var state=ball.Props;double now=FootballBall.PropClock;var actor=AppRoot.Instance.LocalAthlete;
   int tick=state.pickup.countingDown?state.pickup.CountdownNumber(now):0;
   if(!first&&ball.PropsLive&&tick>0&&tick!=propCountdown)Play(FootballSound.Countdown,ball.PropAnchor(state.pickup.anchor),.7f,"prop-countdown");propCountdown=tick;
   float remaining=actor?ball.TurboRemaining(actor):0;
   if(!first&&ball.PropsLive&&priorTurbo>0&&remaining<=0)Play(FootballSound.TurboEnd,actor.transform.position,.75f,"turbo-expired");priorTurbo=remaining;
   Loop(turbo,FootballSound.TurboLoop,remaining>0,actor?actor.transform.position:transform.position,.6f);
   float phase=(float)(now-state.portalAt)/SportsAbilityRules.PortalSeconds;
   bool changed=portalSequence!=state.portalSequence;
   if(first||changed){portalPhase=first||phase>.25f?phase:0;portalClosed=first&&phase>=1.26f;portalSequence=state.portalSequence;}
   if(state.portalSequence>0&&state.portalOutcome!=2&&(state.transferring||state.portalOutcome==1)){
    if(phase>=.14f&&portalPhase<.14f)Play(FootballSound.PortalIn,state.source,.9f,"portal-shrink");
    if(phase>=.54f&&portalPhase<.54f)Play(FootballSound.PortalOut,state.destination,.9f,"portal-grow");
    if(phase>=1.26f&&!portalClosed&&portalPhase<1.26f){Play(FootballSound.PortalClose,state.destination,.8f,"portal-close");portalClosed=true;}
   }
   portalPhase=phase;
  }
  void PresentFeet(){
   removed.Clear();foreach(var pair in feet)if(!pair.Key||!Athlete.Active.Contains(pair.Key))removed.Add(pair.Key);foreach(var a in removed)feet.Remove(a);
   foreach(var a in Athlete.Active){
    if(!a||!a.gameObject.activeInHierarchy||a.inTransit)continue;var m=a.GetComponent<FootballMotion>();if(!m||!m.Active)continue;
    bool airborne=a.Airborne;var effort=a.FootballEffort;
    if(!feet.TryGetValue(a,out var f)){f=new Feet{position=a.transform.position,phase=m.GaitPhase,touch=m.ContactWeight,yaw=a.transform.eulerAngles.y,airborne=airborne,action=a.Action,dash=effort.Dashing,empty=effort.Exhausted,charging=a.Charging};feet[a]=f;continue;}
    bool jump=Vector3.Distance(f.position,a.transform.position)>2||Time.deltaTime>.25f;bool moving=a.speed>.35f&&!airborne&&!a.LoadingJump&&a.Action==FootballAction.None&&!jump;
    if(!jump){
     if(f.airborne&&!airborne)Play(FootballSound.Land,a.transform.position,.8f,"landing");
     if(!f.airborne&&airborne)Play(FootballSound.Cloth,a.transform.position,.7f,"jump");
     // Gait wraps at the support contact, even when sprint stance is shorter
     // than one rendered frame. Both feet share this presentation clock.
     if(a==AppRoot.Instance.LocalAthlete&&moving&&Time.timeAsDouble-f.step>.09){if(BasketballAudio.Crossed(f.phase,m.GaitPhase,0)||m.GaitPhase<f.phase){Play(a.speed>4.8f?FootballSound.Run:FootballSound.Walk,m.LeftFoot,.72f,"foot-plant");f.step=Time.timeAsDouble;}else if(BasketballAudio.Crossed(f.phase,m.GaitPhase,.5f)){Play(a.speed>4.8f?FootballSound.Run:FootballSound.Walk,m.RightFoot,.72f,"foot-plant");f.step=Time.timeAsDouble;}}
     if(moving&&a.ControlsFootball&&!a.Charging&&m.ContactWeight>.9f&&f.touch<=.9f)Play(FootballSound.Dribble,ball.transform.position,.8f,"foot-ball");
     if(a==AppRoot.Instance.LocalAthlete&&moving&&(Mathf.Abs(Mathf.DeltaAngle(f.yaw,a.transform.eulerAngles.y))>4||f.speed-a.speed>1.2f||effort.Pressuring)&&Time.timeAsDouble-f.cut>.55){Play(FootballSound.Cut,a.transform.position,.7f,"grass-cut");f.cut=Time.timeAsDouble;}
     if(a.Action!=f.action){if(a.Action==FootballAction.Slide){Play(FootballSound.Slide,a.transform.position,1,"slide");Play(FootballSound.Swipe,a.transform.position,.35f,"slide-accent");}else if(a.Action==FootballAction.Hit){Play(FootballSound.Body,a.transform.position,.8f,"body-contact");f.fallAt=Time.timeAsDouble+.18;}}
     if(f.fallAt>=0&&Time.timeAsDouble>=f.fallAt){if(a.Action==FootballAction.Hit)Play(FootballSound.Fall,a.transform.position,.8f,"grounded-fall");f.fallAt=-1;}
     if(a.Charging&&!f.charging)Play(FootballSound.Cloth,a.transform.position,.6f,"gather");
     if(effort.Dashing&&!f.dash)Play(FootballSound.Dash,a.transform.position,.7f,"dash");
     if(a==AppRoot.Instance.LocalAthlete){if(effort.Exhausted&&!f.empty)Play(FootballSound.Empty,a.transform.position,.8f,"exhausted");if(!effort.Exhausted&&f.empty)Play(FootballSound.Ready,a.transform.position,.6f,"recovered");}
    }
    f.phase=m.GaitPhase;f.touch=m.ContactWeight;f.airborne=airborne;f.action=a.Action;f.position=a.transform.position;f.yaw=a.transform.eulerAngles.y;f.speed=a.speed;f.dash=effort.Dashing;f.empty=effort.Exhausted;f.charging=a.Charging;
   }
  }
 }
}
