using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Approved resources are shared by path, including the existing sports banks.
 // This component presents state only; it never changes a match or inventory.
 [DefaultExecutionOrder(400)]
 public sealed class GolfFishingAudio:MonoBehaviour {
  [Serializable] public sealed class Cue {public string cue;public string[] paths;}
  [Serializable] public sealed class Bank {public Cue[] cues;}
  static Bank definition;
  static readonly Dictionary<string,AudioClip[]> clips=new();
  static readonly Dictionary<string,int> takes=new();
  public static GolfFishingAudio Golf {get;private set;}
  public static GolfFishingAudio Fishing {get;private set;}
  public static GolfFishingAudio Current=>Golf&&Golf.Context?Golf:Fishing&&Fishing.Context?Fishing:null;
  sealed class Voice {public AudioSource source;public string cue;public float gain;public bool flat;public int frame;}
  sealed class Walker {public Vector3 position;public float distance;public bool airborne;}
  sealed class Cart {public GolfCartState state;public Vector3 point;public float speed;}
  sealed class Ball {public Vector3 point;public uint reset;}
  readonly List<Voice> voices=new();readonly Dictionary<string,Voice> loops=new();
  readonly Dictionary<Athlete,Walker> walkers=new();readonly Dictionary<ulong,Cart> carts=new();readonly Dictionary<ulong,Ball> balls=new();
  GolfMatchManager golf;FishingGame fishing;GolfBallPhysicsSettings golfPhysics;bool focused=true,paused,presenting;uint round;
  GolfMatchPhase golfPhase;ulong firstFinisher;FishingMatchSnapshot fishMatch;int tick,wanted;
  readonly double[] respawns=new double[FishingState.FishCount];
  float buttonsAt,uiAt=-10;int uiFrame=-1;string resultCue;double resultAt=-1;
  public bool Context=>isActiveAndEnabled&&(golf&&golf.Context||fishing&&fishing.Context);
  public bool Audible=>Context&&focused&&!paused&&!AudioListener.pause&&Time.timeScale>0;
  public int PlayingLoops=>loops.Values.Count(v=>v.source&&v.source.isPlaying);
  public static IEnumerable<Cue> Definitions {get{Load();return definition.cues;}}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public static Action<string,Vector3> Trace;
#endif
  static void Load(){if(definition!=null)return;var text=Resources.Load<TextAsset>("GolfFishingAudioBank");if(!text)throw new InvalidOperationException("Approved Golf/Fishing audio bank is missing");definition=JsonUtility.FromJson<Bank>(text.text);}
  public static AudioClip[] Clips(string key){
   Load();if(clips.TryGetValue(key,out var found))return found;
   var entry=Array.Find(definition.cues,c=>c.cue==key);if(entry==null)throw new ArgumentException("Unknown Golf/Fishing cue: "+key);
   found=Array.ConvertAll(entry.paths,p=>Resources.Load<AudioClip>(p));
   if(found.Length==0||Array.Exists(found,c=>!c))throw new InvalidOperationException("Missing approved Golf/Fishing clip: "+key);
   clips.Add(key,found);return found;
  }
  public static AudioClip Choose(string key,int variant=-1){var bank=Clips(key);if(variant>=0)return bank[variant%bank.Length];takes.TryGetValue(key,out int i);takes[key]=(i+1)%bank.Length;return bank[i];}
  public static GolfFishingAudio Ensure(Component owner){var a=owner.GetComponent<GolfFishingAudio>();return a?a:owner.gameObject.AddComponent<GolfFishingAudio>();}
  void Awake(){golf=GetComponent<GolfMatchManager>();fishing=GetComponent<FishingGame>();if(golf){Golf=this;golfPhysics=Resources.Load<GolfBallPhysicsSettings>("GolfBallPhysics");}if(fishing)Fishing=this;focused=Application.isFocused;Load();for(int i=0;i<16;i++)voices.Add(NewVoice("Island sound "+i));}
  Voice NewVoice(string name){var child=new GameObject(name);child.transform.SetParent(transform,false);var s=child.AddComponent<AudioSource>();s.playOnAwake=false;s.dopplerLevel=0;s.reverbZoneMix=0;s.rolloffMode=AudioRolloffMode.Linear;s.minDistance=3;s.maxDistance=38;return new Voice{source=s};}
  float Gain(Voice v)=>v.gain*(v.cue.StartsWith("ui-",StringComparison.Ordinal)?MenuPreferences.UIVolume:v.cue=="air"||v.cue=="shore-waves"?MenuPreferences.AmbienceVolume:MenuPreferences.SFXVolume);
  public void Play(string cue,Vector3 point,float gain=1,bool flat=false,int variant=-1){
   if(!Audible||gain<=0)return;
   int start=cue.StartsWith("ui-",StringComparison.Ordinal)?12:0,end=start==12?16:12;
   Voice voice=null;for(int i=start;i<end;i++){if(!voices[i].source.isPlaying){voice=voices[i];break;}if(voice==null||voices[i].frame<voice.frame)voice=voices[i];}
   voice.source.Stop();voice.cue=cue;voice.gain=gain;voice.flat=flat;voice.frame=Time.frameCount;
   voice.source.transform.position=point;voice.source.spatialBlend=flat?0:.75f;voice.source.clip=Choose(cue,variant);voice.source.pitch=1;voice.source.volume=Gain(voice);voice.source.Play();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   Trace?.Invoke(cue,point);
#endif
  }
  public void Loop(string slot,string cue,bool active,Vector3 point,float gain=1,float pitch=1,bool flat=false,int variant=0){
   if(!active||!Audible||MenuPreferences.MasterVolume<=.001f){if(loops.TryGetValue(slot,out var old))old.source.Stop();return;}
   if(!loops.TryGetValue(slot,out var v)){if(loops.Count>=32)return;loops.Add(slot,v=NewVoice("Island loop "+slot));v.source.loop=true;}
   v.frame=Time.frameCount;v.gain=gain;v.flat=flat;v.source.transform.position=point;v.source.spatialBlend=flat?0:.8f;v.source.pitch=Mathf.Clamp(pitch,.5f,1.8f);
   var selected=Choose(cue,variant);if(v.source.clip!=selected){v.source.Stop();v.source.clip=selected;}v.cue=cue;v.source.volume=Gain(v);if(!v.source.isPlaying&&v.source.volume>.001f)v.source.Play();
  }
  public void StopLoop(string slot){if(loops.TryGetValue(slot,out var v))v.source.Stop();}
  public void StopAll(){foreach(var v in voices)if(v.source)v.source.Stop();foreach(var v in loops.Values)if(v.source){v.source.Stop();Destroy(v.source.gameObject);}loops.Clear();walkers.Clear();carts.Clear();balls.Clear();presenting=false;resultAt=-1;}
  public static bool PlayMenu(string cue="ui-click"){
   var a=Current;if(!a)return false;if(cue!="ui-error"&&(a.uiFrame==Time.frameCount||Time.unscaledTime-a.uiAt<.08f))return true;a.uiFrame=Time.frameCount;a.uiAt=Time.unscaledTime;
   a.Play(cue,Vector3.zero,1,true);return true;
  }
  public static void PointerReleased(){var a=Current;if(a)a.uiFrame=Time.frameCount;}
  public static string ButtonCue(string name){
   name=name.ToLowerInvariant();if(name.Contains("close"))return "ui-close";
   if(name.Contains("back")||name.Contains("cancel")||name.Contains("leave"))return "ui-back";
   if(name.Contains("start")||name.Contains("confirm")||name.Contains("ready"))return "ui-confirm";
   if(name.Contains("settings")||name.Contains("guide")||name.Contains("results")||name.Contains("ranking"))return "ui-open";
   if(name.Contains("toggle")||name.Contains("mute"))return "ui-toggle";
   if(name.Contains("select")||name.Contains("mode")||name.Contains("tab")||name.Contains("camera")||name.Contains("favourite"))return "ui-select";
   return "ui-click";
  }
  void LateUpdate(){
   if(!Audible){if(presenting)StopAll();return;}
   uint current=golf?golf.Round:fishing.State.Match.round;bool first=!presenting||round!=current;
   if(first){StopAll();round=current;presenting=true;}
   var actor=AppRoot.Instance?AppRoot.Instance.LocalAthlete:null;var point=actor?actor.transform.position:transform.position;
   Loop("air","air",true,point,.28f,1,true);Loop("shore","shore-waves",true,point,fishing ? .32f : .12f,1,true);
   if(golf)GolfState(first,actor);else FishingStateSounds(first,actor);
   Movement(actor,first);
   if(resultAt>=0&&Time.unscaledTimeAsDouble>=resultAt){if(!FishingCeremony.Presenting)Play(resultCue,point,.8f,true);resultAt=-1;}
   foreach(var v in voices)if(v.source.isPlaying)v.source.volume=Gain(v);
   foreach(var v in loops.Values){if(v.frame<Time.frameCount-1)v.source.Stop();else if(v.source.isPlaying)v.source.volume=Gain(v);}
   if(Time.unscaledTime>=buttonsAt){buttonsAt=Time.unscaledTime+.5f;foreach(var button in Resources.FindObjectsOfTypeAll<Button>())if(button&&button.gameObject.scene.IsValid()&&!button.GetComponent<GolfFishingButtonSound>())button.gameObject.AddComponent<GolfFishingButtonSound>();}
  }
  void GolfState(bool first,Athlete actor){
   var state=golf.State;var point=actor?actor.transform.position:transform.position;
   if(!first&&state.FirstFinisher!=firstFinisher&&state.CountdownStarted&&state.Phase==GolfMatchPhase.FinalCountdown)Play("first-finisher",point,.75f,true);
   if(!first&&state.Phase==GolfMatchPhase.Ended&&golfPhase!=state.Phase){var p=golf.Player(actor);var result=state.Results().FirstOrDefault(r=>r.player==p);resultCue=p!=null&&p.IsFinished&&result.rank==1?(result.tie?"result-draw":"result-win"):"result-loss";resultAt=Time.unscaledTimeAsDouble+1.1;}
   int seconds=state.Phase==GolfMatchPhase.FinalCountdown?Mathf.CeilToInt((float)golf.Remaining):0;
   if(!first&&seconds>0&&seconds<=5&&seconds!=tick)Play("countdown",point,.65f,true);
   tick=seconds;golfPhase=state.Phase;firstFinisher=state.FirstFinisher;
   foreach(var a in Athlete.Active){if(!a)continue;ulong id=GolfCartWorld.Key(a);var c=GolfCartWorld.State(a);var cart=GolfCartWorld.Owned(a);
    Vector3 pos=cart?cart.transform.position:c.position;
    if(!carts.TryGetValue(id,out var old)){carts[id]=new Cart{state=c,point=pos};continue;}
    if(c.summoned!=old.state.summoned)Play(c.summoned?"cart-summon":"cart-recall",c.summoned?pos:old.point,.75f);
    if(c.summoned&&old.state.summoned&&c.driver!=old.state.driver)Play("cart-seat",pos,.7f);
    float speed=c.summoned&&old.state.summoned?Vector3.ProjectOnPlane(pos-old.point,Vector3.up).magnitude/Mathf.Max(.001f,Time.deltaTime):0;
    bool moving=speed>.12f&&speed<20&&Time.deltaTime<.25f&&c.driver!=GolfCartState.NoDriver;
    if(c.summoned&&old.state.summoned){
     if(c.speed<-.35f&&old.state.speed>=-.35f&&moving)Play("cart-reverse",pos,.4f);
     if(old.speed>1&&speed<.15f)Play("cart-brake",pos,.55f);
    }
    bool sand=c.summoned&&golf.Course.RollingResistance(pos,golfPhysics)>5.7f;
    Loop("motor"+id,"cart-motor",moving,pos,.35f+Mathf.Clamp01(speed/8)*.25f,.78f+Mathf.Clamp01(speed/8)*.45f);
    Loop("tyres"+id,"cart-tyres",moving,pos,Mathf.Clamp01(speed/5)*.45f,1,false,sand?1:0);
    old.state=c;old.point=pos;old.speed=speed;
   }
   foreach(var p in state.Players){var b=golf.Ball(p.PlayerId);if(!b)continue;var pos=b.transform.position;
    if(!balls.TryGetValue(p.PlayerId,out var old)){balls[p.PlayerId]=new Ball{point=pos,reset=b.ResetSequence};continue;}
    float speed=Vector3.Distance(pos,old.point)/Mathf.Max(.001f,Time.deltaTime);bool ground=Physics.Raycast(pos,Vector3.down,GolfBall.Radius+.055f,(1<<8)|(1<<GolfTee.Layer),QueryTriggerInteraction.Ignore);
    bool rolling=b.Live&&state.Running&&b.ResetSequence==old.reset&&speed>.15f&&speed<9&&ground&&Time.deltaTime<.25f;
    Loop("ball"+p.PlayerId,"golf-roll",rolling,pos,Mathf.Clamp01(speed/4)*.45f);old.point=pos;old.reset=b.ResetSequence;
   }
  }
  void FishingStateSounds(bool first,Athlete actor){
   var m=fishing.State.Match;double now=fishing.Now;var point=actor?actor.transform.position:transform.position;
   if(!first&&m.phase!=fishMatch.phase){
    if(m.phase==FishingRoundPhase.Playing&&fishMatch.phase==FishingRoundPhase.Countdown)Play("round-start",point,.85f,true);
    if(m.phase==FishingRoundPhase.Ended){Play("round-end",point,.75f,true);var p=fishing.Player(actor);bool win=m.mode==FishingMode.Crew?m.outcome==FishingOutcome.Success:p.HasValue&&fishing.State.Rank(p.Value.owner)==1;bool tie=win&&m.mode!=FishingMode.Crew&&p.HasValue&&fishing.State.Players.Count(q=>q.score==p.Value.score)>1;
     resultCue=win?(tie?"result-draw":"result-win"):"result-loss";
     if(m.cupFinished&&m.cupValid&&p.HasValue&&fishing.State.CupRank(p.Value.owner)==1)resultCue="cup-champion";
     if(m.outcome==FishingOutcome.Abandoned)resultCue="recovery";resultAt=Time.unscaledTimeAsDouble+.8;
    }
   }
   int seconds=m.phase==FishingRoundPhase.Countdown?Mathf.CeilToInt((float)(m.started-now)):m.phase==FishingRoundPhase.Playing?Mathf.CeilToInt((float)(m.deadline-now)):0;
   if(!first&&seconds>0&&seconds<=5&&seconds!=tick)Play("countdown",point,.65f,true);
   int next=fishing.State.Window(now);if(!first&&m.phase==FishingRoundPhase.Playing&&next!=wanted)Play("wanted-change",point,.65f,true);wanted=next;tick=seconds;fishMatch=m;
   foreach(var fish in fishing.State.Fish){if(!first&&respawns[fish.id]>0&&now>=respawns[fish.id]&&now-Time.unscaledDeltaTime<respawns[fish.id]){var pos=fishing.FishPosition(fish.id,now);if(actor&&Vector3.Distance(pos,point)<8&&pos.y>=fishing.WaterY-.12f)Play("fish-bubbles",pos,.4f);}respawns[fish.id]=fish.respawnAt;}
  }
  void Movement(Athlete actor,bool first){
   if(!actor)return;var pos=actor.transform.position;
   if(first||!walkers.TryGetValue(actor,out var w)){walkers[actor]=new Walker{position=pos,airborne=actor.Airborne};return;}
   float distance=Vector3.ProjectOnPlane(pos-w.position,Vector3.up).magnitude;bool valid=!actor.inTransit&&distance<1.5f&&Time.deltaTime<.25f&&!(golf&&GolfCartWorld.Driving(actor));
   if(valid){if(actor.Airborne&&!w.airborne)Play("cloth",pos,.6f);if(!actor.Airborne&&w.airborne&&actor.Grounded)Play("grass-land",pos,.65f);}
   if(fishing&&valid&&actor.Grounded&&!actor.Airborne&&!actor.LoadingJump&&!fishing.Busy(actor)&&distance>.0001f){
    w.distance+=distance;bool run=distance/Mathf.Max(.001f,Time.deltaTime)>4.5f;float stride=run?1.5f:.9f;
    if(w.distance>=stride){w.distance%=stride;Physics.Raycast(pos+Vector3.up*.2f,Vector3.down,out var hit,.8f,1<<8,QueryTriggerInteraction.Ignore);string name=hit.collider?hit.collider.name.ToLowerInvariant():"";
     string cue=pos.y<=fishing.WaterY+.06f?"water-steps":name.Contains("pier")||name.Contains("wood")||name.Contains("deck")||name.Contains("plank")?"wood-steps":name.Contains("sand")?"sand-steps":name.Contains("stone")||name.Contains("path")||name.Contains("station")?"stone-steps":run?"grass-run":"grass-walk";Play(cue,pos,.65f);
    }
   }else w.distance=0;w.position=pos;w.airborne=actor.Airborne;
  }
  void OnApplicationFocus(bool value){focused=value;if(!value)StopAll();}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public void ReviewFocus(bool value)=>OnApplicationFocus(value);
#endif
  void OnApplicationPause(bool value){paused=value;if(value)StopAll();}
  void OnDisable()=>StopAll();
  void OnDestroy(){if(Golf==this)Golf=null;if(Fishing==this)Fishing=null;}
 }
}
