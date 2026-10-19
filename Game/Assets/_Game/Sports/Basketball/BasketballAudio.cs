using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public enum BasketballSound:byte { Bounce,Board,Rim,Net,NetBrush,Catch,Hand,Release,Block,Score,Walk,Run,Squeak,Scuff,Land,Cloth,Swipe,Body,Countdown,Spawn,Pickup,LockAim,LockFire,Interrupt,TurboStart,TurboLoop,TurboEnd,PortalOpen,PortalIn,PortalOut,PortalClose,Boundary,Inbound,Air,Roll,UIClick,UIConfirm,UIBack,UIToggle,UISelect,UIError,UIOpen,UIClose }
 public struct BasketballAudioEvent:INetworkSerializable {
  public uint sequence;public double time;public BasketballSound cue;public Vector3 point;public float gain;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref sequence);s.SerializeValue(ref time);s.SerializeValue(ref cue);s.SerializeValue(ref point);s.SerializeValue(ref gain);}
 }

 // One bounded bank/pool per active court. Physical outcomes come from the host;
 // held dribbles follow the presented pose; only the local player's feet sound.
 [DefaultExecutionOrder(110)]
 public sealed class BasketballAudio:MonoBehaviour {
  public static BasketballAudio Instance {get;private set;}
  static readonly string[] Keys={"ball-bounce","board","rim","net","net-brush","catch","hand","release","block","score","walk","run","squeak-real","scuff","land","cloth","swipe","body","countdown","spawn","pickup","lock-aim","lock-fire","interrupt","turbo-start","turbo-loop","turbo-end","portal-open","portal-in","portal-out","portal-close","boundary","inbound","air","roll","ui-click","ui-confirm","ui-back","ui-toggle","ui-select","ui-error","ui-open","ui-close"};
  sealed class Voice {public AudioSource source;public BasketballSound cue;public float gain;public int priority;public double began;}
  readonly Dictionary<BasketballSound,AudioClip[]> bank=new();readonly Dictionary<BasketballSound,int> lastTake=new();
  readonly List<Voice> voices=new();readonly Queue<BasketballAudioEvent> pending=new();
  readonly Dictionary<Athlete,FootState> feet=new();readonly List<Athlete> removed=new();
  BasketballBall ball;Transform voiceRoot;Voice aim,turbo,air,roll;bool presenting,suspended;uint serial,lastReceived;ulong sender=ulong.MaxValue;
  Athlete carrier;float phase;bool dribbling;double aimDeadline;float previousTurbo;
  public bool AimPlaying=>aim!=null&&aim.source.isPlaying;
  public int PlayingVoices {get{int count=0;foreach(var v in voices)if(v.source.isPlaying)count++;return count;}}
  public int LoadedClips {get{int count=0;foreach(var p in bank)count+=p.Value.Length;return count;}}
  public static bool Context=>Instance&&Instance.ball&&Instance.ball.Playing;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public static Action<BasketballSound,Vector3,float,string,uint> Trace;
  public static int NetworkReceived,NetworkDropped;
  public AudioClip ReviewClip(BasketballSound cue)=>bank.TryGetValue(cue,out var clips)&&clips.Length>0?clips[0]:null;
  public float ReviewAimVolume=>aim?.source.volume??0;
  public float ReviewAimRemaining=>aim?.source.clip?Mathf.Max(0,aim.source.clip.length-aim.source.time):0;
  public float ReviewVolume(BasketballSound cue){float level=0;foreach(var v in voices)if(v.cue==cue&&v.source.isPlaying)level=Mathf.Max(level,v.source.volume);return level;}
#endif
  void Awake(){
   ball=GetComponent<BasketballBall>();Instance=this;voiceRoot=new GameObject("Basketball sound voices").transform;voiceRoot.SetParent(transform.parent,false);
   var clips=Resources.LoadAll<AudioClip>("BasketballAudio");
   for(int i=0;i<Keys.Length;i++){var list=new List<AudioClip>();foreach(var clip in clips)if(clip.name.Length==Keys[i].Length+3&&clip.name.StartsWith(Keys[i]+"-",StringComparison.Ordinal))list.Add(clip);list.Sort((a,b)=>string.CompareOrdinal(a.name,b.name));bank[(BasketballSound)i]=list.ToArray();}
   for(int i=0;i<20;i++)voices.Add(NewVoice("Court voice "+i));
   aim=NewVoice("Lock-on aim");turbo=NewVoice("Turbo air");air=NewVoice("Coastal air");roll=NewVoice("Rolling ball");
  }
  Voice NewVoice(string name){var go=new GameObject(name);go.transform.SetParent(voiceRoot,false);var source=go.AddComponent<AudioSource>();source.playOnAwake=false;source.dopplerLevel=0;source.reverbZoneMix=0;source.rolloffMode=AudioRolloffMode.Custom;source.SetCustomCurve(AudioSourceCurveType.CustomRolloff,AnimationCurve.Linear(0,1,1,1));source.minDistance=1;source.maxDistance=100;return new Voice{source=source};}
  void OnEnable(){Instance=this;presenting=false;}
  void OnDisable(){StopAll();if(Instance==this)Instance=null;}
  void OnDestroy(){if(voiceRoot)Destroy(voiceRoot.gameObject);}
  void OnApplicationPause(bool pause){suspended=pause;if(pause)StopAll();else presenting=false;}
  public void StopAll(){foreach(var v in voices)v.source.Stop();aim?.source.Stop();turbo?.source.Stop();air?.source.Stop();roll?.source.Stop();pending.Clear();feet.Clear();carrier=null;dribbling=false;presenting=false;lastReceived=0;sender=ulong.MaxValue;}
  public static bool PlayMenu(BasketballSound cue=BasketballSound.UIClick){if(!Context)return false;Instance.Play(cue,Vector3.zero,1,"ui");return true;}
  public static BasketballSound ButtonCue(string name){
   if(name=="Court settings"&&BasketballHUD.Instance&&BasketballHUD.Instance.MenuOpen)return BasketballSound.UIClose;
   if(name.StartsWith("Close",StringComparison.Ordinal))return BasketballSound.UIClose;
   if(name.StartsWith("Back",StringComparison.Ordinal)||name=="Stay on court")return BasketballSound.UIBack;
   if(name.StartsWith("Confirm",StringComparison.Ordinal))return BasketballSound.UIConfirm;
   if(name=="Free roam")return BasketballSound.UIToggle;
   if(name.Contains("tab")||name=="Eye level"||name=="Follow"||name=="Court view")return BasketballSound.UISelect;
   if(name=="Court guide"||name.Contains("settings")||name=="Leave court")return BasketballSound.UIOpen;
   return BasketballSound.UIClick;
  }
  public static void Emit(BasketballSound cue,Vector3 point,float gain=1){
   var audio=Instance;if(!audio||!audio.ball||!audio.ball.Authority||!audio.ball.Playing)return;
   var e=new BasketballAudioEvent{sequence=++audio.serial,time=BasketballMotion.Clock,cue=cue,point=point,gain=Mathf.Clamp(gain,0,1.4f)};
   audio.Play(cue,point,e.gain,"authority",e.sequence);
   var host=NetworkAthlete.HostPlayer;if(host&&host.IsSpawned&&host.IsServer)host.BasketballAudioRpc(e);
  }
  public static void Receive(BasketballAudioEvent e,ulong hostObject){
   var a=Instance;if(!a||!a.ball||a.ball.Authority||!a.ball.Playing)return;
   if(a.sender!=hostObject){a.sender=hostObject;a.lastReceived=0;a.pending.Clear();}
   if(e.sequence<=a.lastReceived)return;a.lastReceived=e.sequence;
   if(!double.IsFinite(e.time)||!float.IsFinite(e.gain)||!float.IsFinite(e.point.sqrMagnitude)||(int)e.cue>=Keys.Length)return;
   // Live reliable events, never stored snapshots: a join cannot replay a basket.
   double age=BasketballMotion.Clock-e.time;
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
  static bool UI(BasketballSound cue)=>cue>=BasketballSound.UIClick;
  static int Priority(BasketballSound cue)=>UI(cue)?3:cue==BasketballSound.Walk||cue==BasketballSound.Run||cue==BasketballSound.Scuff||cue==BasketballSound.Cloth||cue==BasketballSound.Hand?0:cue==BasketballSound.Squeak||cue==BasketballSound.Bounce?1:2;
  // Mix at playback so host events and guest presentation receive the same
  // reduction once, while existing user volume controls still apply.
  static float MixLevel(BasketballSound cue)=>cue switch {
   BasketballSound.Walk or BasketballSound.Run=>.28f,
   BasketballSound.Squeak or BasketballSound.Scuff or BasketballSound.Cloth=>.22f,
   BasketballSound.Land or BasketballSound.Swipe or BasketballSound.Body=>.35f,
   BasketballSound.Bounce or BasketballSound.Board or BasketballSound.Rim=>.4f,
   BasketballSound.Hand or BasketballSound.Catch or BasketballSound.Release or BasketballSound.Block or BasketballSound.Roll=>.45f,
   _=>1
  };
  float Gain(Voice v){
   if(UI(v.cue))return v.gain*MenuPreferences.UIVolume;
   float distance=0;var actor=AppRoot.Instance?AppRoot.Instance.LocalAthlete:null;if(actor)distance=Vector3.ProjectOnPlane(v.source.transform.position-actor.transform.position,Vector3.up).magnitude;
   float attenuation=Mathf.Lerp(1,.16f,Mathf.InverseLerp(4,32,distance));
   return v.gain*MixLevel(v.cue)*(v.cue==BasketballSound.Air?MenuPreferences.AmbienceVolume:MenuPreferences.SFXVolume)*attenuation;
  }
  AudioClip Choose(BasketballSound cue,out int take){take=0;if(!bank.TryGetValue(cue,out var clips)||clips.Length==0)return null;lastTake.TryGetValue(cue,out int last);take=(last+1)%clips.Length;lastTake[cue]=take;return clips[take];}
  public void Play(BasketballSound cue,Vector3 position,float gain=1,string reason="pose",uint eventId=0){
   if(suspended||!ball||!ball.Playing||gain<=0)return;var clip=Choose(cue,out int take);if(!clip)return;
   int priority=Priority(cue);Voice voice=null;
   int start=UI(cue)?16:0,end=UI(cue)?20:16;
   for(int i=start;i<end;i++){var v=voices[i];if(!v.source.isPlaying){voice=v;break;}if(v.priority<=priority&&(voice==null||v.priority<voice.priority||v.priority==voice.priority&&v.began<voice.began))voice=v;}
   if(voice==null)return;
   Begin(voice,cue,clip,position,gain,false,0,UI(cue)||cue==BasketballSound.Countdown?1:1+(take%3-1)*.015f);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   Trace?.Invoke(cue,position,gain,reason,eventId);
#endif
  }
  void Begin(Voice v,BasketballSound cue,AudioClip clip,Vector3 point,float gain,bool loop,float offset,float pitch=1){
   var s=v.source;s.Stop();v.cue=cue;v.gain=Mathf.Clamp(gain,0,1.4f);v.priority=Priority(cue);v.began=Time.unscaledTimeAsDouble;
   s.transform.position=point;s.clip=clip;s.pitch=pitch;s.loop=loop;s.spatialBlend=UI(cue)||cue==BasketballSound.Air?0:.5f;s.priority=128-v.priority*24;s.volume=Gain(v);s.time=Mathf.Clamp(offset,0,Mathf.Max(0,clip.length-.01f));s.Play();
  }
  void Loop(Voice voice,BasketballSound cue,bool active,Vector3 point,float gain){if(!active){voice.source.Stop();return;}if(!voice.source.isPlaying){var clip=Choose(cue,out _);if(clip)Begin(voice,cue,clip,point,gain,true,0);}voice.gain=gain;voice.source.transform.position=point;voice.source.volume=Gain(voice);}
  void LateUpdate(){
   if(!ball.Playing||suspended){if(presenting)StopAll();return;}
   bool first=!presenting;presenting=true;
   while(pending.Count>0&&BasketballMotion.Clock>=pending.Peek().time+.05){var e=pending.Dequeue();if(BasketballMotion.Clock-e.time<.8)Play(e.cue,e.point,e.gain,"network",e.sequence);}
   foreach(var v in voices)if(v.source.isPlaying)v.source.volume=Gain(v);
   PresentDribble(first);PresentFeet();PresentLoops(first);
  }
  void PresentDribble(bool first){
   var actor=ball.Holder;float next=ball.DribblePhase;
   bool active=actor&&!actor.Airborne&&!actor.LoadingJump&&!ball.InboundPending&&!ball.PortalBusy&&actor.BasketballMotion&&!actor.BasketballMotion.CarryingPose;
   if(active&&dribbling&&carrier==actor&&!first){float advance=Mathf.Repeat(next-phase,1);if(advance<.55f&&Time.deltaTime<.25f){
    if(Crossed(phase,next,.43f))Play(BasketballSound.Bounce,ball.transform.position,.85f,"dribble-floor");
    if(next<phase)Play(BasketballSound.Hand,ball.transform.position,.7f,"dribble-hand");
   }}carrier=actor;phase=next;dribbling=active;
  }
  public static bool Crossed(float from,float to,float key)=>to>=from?from<key&&to>=key:from<key||to>=key;
  void PresentLoops(bool first){
   var state=ball.Props;var actor=AppRoot.Instance?AppRoot.Instance.LocalAthlete:null;double now=BasketballMotion.Clock;
   if(state.aiming&&now<state.aimUntil){var aimer=ball.PropActor(state.aimer);if(aimer){
    // Keep the approved clip's natural pitch and quiet finish. Play its final
    // aim-duration seconds, seeking ahead for a delayed guest presentation.
    if(aimDeadline!=state.aimUntil||first){aimDeadline=state.aimUntil;var clip=Choose(BasketballSound.LockAim,out _);if(clip)Begin(aim,BasketballSound.LockAim,clip,aimer.transform.position,1,false,Mathf.Max(0,clip.length-BasketballPropRules.AimSeconds)+(float)Math.Max(0,now-(state.aimUntil-BasketballPropRules.AimSeconds)));}
    aim.source.transform.position=aimer.transform.position;aim.source.volume=Gain(aim);
   }}else {aim.source.Stop();aimDeadline=0;}
   float remaining=actor?ball.TurboRemaining(actor):0;
   if(!first&&previousTurbo>0&&remaining<=0)Play(BasketballSound.TurboEnd,actor?actor.transform.position:transform.position,.75f,"turbo-expired");previousTurbo=remaining;
   Loop(turbo,BasketballSound.TurboLoop,remaining>0,actor?actor.transform.position:transform.position,.6f);
   Loop(air,BasketballSound.Air,true,actor?actor.transform.position:transform.position,.55f);
   float speed=ball.Authority&&!ball.Body.isKinematic?Vector3.ProjectOnPlane(ball.Body.linearVelocity,Vector3.up).magnitude:0;
   bool rolling=!ball.Held&&!ball.PortalBusy&&speed>.4f&&speed<3&&Mathf.Abs(ball.Body.linearVelocity.y)<.3f&&Physics.Raycast(ball.transform.position,Vector3.down,BasketballBall.Radius+.06f,1<<8,QueryTriggerInteraction.Ignore);
   Loop(roll,BasketballSound.Roll,rolling,ball.transform.position,Mathf.Clamp01(speed/3)*.5f);
  }
  sealed class FootState {
   public Transform left,right;public float leftBase,rightBase,leftHeight,rightHeight;public bool leftLift,rightLift,airborne;public Vector3 position;public float speed,yaw;public double lastStep,lastSqueak;public uint action;public BasketballAction lastAction;
  }
  void PresentFeet(){
   removed.Clear();foreach(var pair in feet)if(!pair.Key||!Athlete.Active.Contains(pair.Key))removed.Add(pair.Key);foreach(var actor in removed)feet.Remove(actor);
   foreach(var actor in Athlete.Active){
    if(!actor||!actor.gameObject.activeInHierarchy||actor.inTransit||actor.BasketballFreeRoam)continue;
    if(!feet.TryGetValue(actor,out var f)){
     var placement=actor.GetComponent<TurnFootPlacement>();if(!placement||!placement.left.ankle||!placement.right.ankle)continue;
     f=new FootState{left=placement.left.ankle,right=placement.right.ankle,leftBase=placement.left.initialPosition.y,rightBase=placement.right.initialPosition.y,position=actor.transform.position,yaw=actor.transform.eulerAngles.y,airborne=actor.Airborne,action=actor.BasketballMotion?actor.BasketballMotion.State.sequence:0};feet[actor]=f;continue;
    }
    bool local=AppRoot.Instance&&actor==AppRoot.Instance.LocalAthlete;
    var motion=actor.BasketballMotion;bool airborne=actor.Airborne||(motion&&motion.Finishing&&BasketballMotion.FinishHeight(motion.State,motion.FinishMotorTime)>.025f);
    bool teleported=Vector3.Distance(f.position,actor.transform.position)>2||Time.deltaTime>.25f;
    if(local&&f.airborne&&!airborne&&!teleported)Play(BasketballSound.Land,actor.transform.position,.8f,"landing");
    float left=actor.transform.InverseTransformPoint(f.left.position).y,right=actor.transform.InverseTransformPoint(f.right.position).y;
    bool moving=local&&actor.speed>.35f&&!airborne&&!actor.LoadingJump&&!teleported;
    void Foot(float height,float rest,ref float previous,ref bool lifted,Vector3 point){
     if(!moving){lifted=false;previous=height;return;}
     if(height>rest+.028f)lifted=true;
     if(lifted&&height<=rest+.02f&&height<=previous&&Time.timeAsDouble-f.lastStep>.11){Play(actor.speed>4.8f?BasketballSound.Run:BasketballSound.Walk,point,.72f,"foot-plant");if(motion&&motion.Defensive)Play(BasketballSound.Scuff,point,.55f,"defensive-shuffle");f.lastStep=Time.timeAsDouble;lifted=false;}
     previous=height;
    }
    Foot(left,f.leftBase,ref f.leftHeight,ref f.leftLift,f.left.position);Foot(right,f.rightBase,ref f.rightHeight,ref f.rightLift,f.right.position);
    float yaw=actor.transform.eulerAngles.y;
    bool cut=moving&&(Mathf.Abs(Mathf.DeltaAngle(yaw,f.yaw))>4||f.speed-actor.speed>1.2f);
    if(cut&&Time.timeAsDouble-f.lastSqueak>.55){Play(BasketballSound.Squeak,actor.transform.position,.6f,"shoe-cut");Play(BasketballSound.Scuff,actor.transform.position,.6f,"shoe-drag");f.lastSqueak=Time.timeAsDouble;}
    if(motion&&motion.State.sequence!=f.action){
     f.action=motion.State.sequence;var action=motion.Action;
     if(!teleported&&BasketballMotion.Clock-motion.State.started<.3){
      if(action==BasketballAction.Steal||action==BasketballAction.Block||action==BasketballAction.JumpBlock)Play(BasketballSound.Swipe,actor.transform.position,.8f,"reach");
      else if(local&&(action==BasketballAction.Charge||action==BasketballAction.PassCharge||action==BasketballAction.Cancel||action==BasketballAction.PassCancel))Play(BasketballSound.Cloth,actor.transform.position,.7f,"gather");
     }f.lastAction=action;
    }
    f.airborne=airborne;f.speed=actor.speed;f.yaw=yaw;f.position=actor.transform.position;
   }
  }
 }
}
