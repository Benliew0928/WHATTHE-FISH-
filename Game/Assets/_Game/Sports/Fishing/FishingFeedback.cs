using System;
using UnityEngine;

namespace WhatTheFish {
 public enum FishingFeedbackCue:byte {None,Cast,Splash,Bite,Hook,Landed,Missed,Snapped,Strain}

 // Approved recordings and designed cues follow this device's accepted record.
 // Initial, resumed and late-join snapshots remain a silent baseline.
 [DefaultExecutionOrder(95)]
 public sealed class FishingFeedback:MonoBehaviour {
  const int SampleRate=32000;
  static readonly string[] Keys={null,"fish-cast","float-splash","fish-bite","fish-hook","fish-land","fish-missed","fish-snap","fish-line"};
  GolfFishingAudio audio;FishingPlayerRecord previous;bool cooling;int side;double sonarAt,expiredAt;float surfaceAt,slipUntil;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  readonly System.Collections.Generic.Dictionary<string,int> extraCounts=new();
  public int ReviewExtraCount(string cue)=>extraCounts.TryGetValue(cue,out int n)?n:0;
#endif
  FishingGame game;
  AudioSource effects,reel;
  readonly AudioClip[] clips=new AudioClip[9];
  AudioClip reelClip;
  bool focused,paused,observing,loopWanted,highStrain;
  uint round,sequence;
  ulong owner;
  int seen;
  float strainAgainAt;
  volatile int hapticEpoch;
  volatile bool hapticUnavailable=false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  readonly int[] cueCounts=new int[9];
  public int ReviewCueCount(FishingFeedbackCue cue)=>cueCounts[(int)cue];
  public bool ReviewLoopWanted=>loopWanted;
  public int RuntimeSampleBytes {get;private set;}
#endif

  public static FishingFeedback Ensure(FishingGame owner) {
   if(!owner)return null;
   var feedback=owner.GetComponent<FishingFeedback>();
   if(!feedback)feedback=owner.gameObject.AddComponent<FishingFeedback>();
   feedback.game=owner;return feedback;
  }
  void Awake() {
   game=GetComponent<FishingGame>();audio=GolfFishingAudio.Ensure(this);focused=Application.isFocused;
   effects=Source("Fishing action sounds");reel=Source("Fishing reel sound");
   reel.loop=true;reelClip=GolfFishingAudio.Choose("fish-reel",0);reel.clip=reelClip;
   for(int i=1;i<clips.Length;i++)clips[i]=GolfFishingAudio.Choose(Keys[i],0);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   RuntimeSampleBytes=(reelClip.samples+System.Linq.Enumerable.Sum(clips,c=>c?c.samples:0))*sizeof(float);
#endif
  }
  AudioSource Source(string name) {
   var child=new GameObject(name);child.transform.SetParent(transform,false);
   var source=child.AddComponent<AudioSource>();source.playOnAwake=false;
   source.spatialBlend=0;source.volume=1;source.priority=80;
   source.ignoreListenerPause=false;source.dopplerLevel=0;return source;
  }
  void LateUpdate() {
   effects.volume=MenuPreferences.SFXVolume;
   var app=AppRoot.Instance;var actor=app?app.LocalAthlete:null;
   bool active=focused&&!paused&&Time.timeScale>0&&!AudioListener.pause&&game&&game.isActiveAndEnabled&&game.Context&&actor&&actor.isActiveAndEnabled&&!actor.inTransit&&app.view&&app.view.active&&(!app.Menu||!app.Menu.IsTransitioning);
   var player=active?game.Player(actor):null;
   if(!active||!player.HasValue||!player.Value.connected){Quiet();return;}
   Consume(game.State.Match.round,player.Value,game.Now,app.view.FishingHeld,Time.unscaledTime);
  }

  // Masks cover earlier stages too: a guest that skips a snapshot hears the
  // current actionable cue, never old casts or splashes replayed afterwards.
  static int Through(FishingPhase phase) {
   return phase switch {
    FishingPhase.Casting=>1<<1,
    FishingPhase.Waiting=>(1<<1)|(1<<2),
    FishingPhase.Bite=>(1<<1)|(1<<2)|(1<<3),
    FishingPhase.Reeling=>(1<<1)|(1<<2)|(1<<3)|(1<<4),
    FishingPhase.Caught=>255,
    FishingPhase.Escaped=>255,
    _=>0
   };
  }
  void Consume(uint currentRound,FishingPlayerRecord player,double now,bool locallyHeld,float clock) {
   if(!observing||round!=currentRound||owner!=player.owner) {
    StopSources();observing=true;round=currentRound;owner=player.owner;
    sequence=player.sequence;seen=Through(player.phase);highStrain=player.tension>=.74f;previous=player;cooling=player.tension>.82f;side=FishingState.CounterSide(player,now);sonarAt=now+2;expiredAt=player.activeUntil<=now?player.activeUntil:0;slipUntil=0;
    return; // Initial/late-join and resumed snapshots are a silent baseline.
   }
   if(sequence!=player.sequence) {
    if(unchecked((int)(player.sequence-sequence))<0)return;
    bool sameResult=(player.phase==FishingPhase.Caught||player.phase==FishingPhase.Escaped)&&player.phase==previous.phase&&player.phaseAt==previous.phaseAt&&player.result==previous.result;
    sequence=player.sequence;if(!sameResult)seen=0;highStrain=false;
   }
   FishingFeedbackCue cue=player.phase switch {
    FishingPhase.Casting=>FishingFeedbackCue.Cast,
    FishingPhase.Waiting=>FishingFeedbackCue.Splash,
    FishingPhase.Bite=>FishingFeedbackCue.Bite,
    FishingPhase.Reeling=>FishingFeedbackCue.Hook,
    FishingPhase.Caught=>FishingFeedbackCue.Landed,
    FishingPhase.Escaped when player.result==FishingResult.Snapped=>FishingFeedbackCue.Snapped,
    FishingPhase.Escaped when player.result==FishingResult.Missed||player.result==FishingResult.TimedOut=>FishingFeedbackCue.Missed,
    _=>FishingFeedbackCue.None
   };
   double cueAt=player.phase==FishingPhase.Bite?player.biteAt:player.phase==FishingPhase.Waiting?player.phaseAt+.65:player.phaseAt;
   bool fresh=double.IsFinite(now)&&double.IsFinite(cueAt)&&now>=cueAt-.15&&now-cueAt<=1.1&&(player.phase!=FishingPhase.Bite||now<player.hookUntil);
   if(cue!=FishingFeedbackCue.None&&(seen&(1<<(int)cue))==0&&fresh)Play(cue);
   seen|=Through(player.phase);
   PresentExtras(player,now,clock);
   bool held=player.held&&player.phase==FishingPhase.Reeling&&player.steer==FishingState.CounterSide(player,now);
   UpdateLoop(held,player.tension);
   reel.pitch=player.active==FishingAbility.PowerReel&&now<player.activeUntil?1.18f:1;
   if(player.tension<.56f)highStrain=false;
   if(held&&player.tension>=.74f&&!highStrain&&clock>=strainAgainAt) {
    highStrain=true;strainAgainAt=clock+1.25f;Play(FishingFeedbackCue.Strain);
   }
   previous=player;
  }
  void Extra(string cue,float gain=.75f,int variant=-1){
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   extraCounts.TryGetValue(cue,out int n);extraCounts[cue]=n+1;
#endif
   audio?.Play(cue,previous.origin,gain,true,variant);
  }
  void PresentExtras(FishingPlayerRecord p,double now,float clock){
   bool fighting=p.phase==FishingPhase.Reeling,wasFighting=previous.phase==FishingPhase.Reeling;
   bool fresh=now>=p.phaseAt-.15&&now-p.phaseAt<1.1;
   bool activated=p.activeAt!=previous.activeAt&&p.active!=FishingAbility.None&&now-p.activeAt<=1.1&&now>=p.activeAt-.15;
   if(activated){
    string cue=p.active switch {FishingAbility.Sonar=>"sonar",FishingAbility.SteelWire=>"cool-spool",FishingAbility.FishingBuddy=>"gear-pickup",FishingAbility.PowerReel=>"power-reel",FishingAbility.BubbleGuard=>"bubble-guard",_=>"quick-lure"};
    Extra(cue);sonarAt=now+2;
   }
   if(p.sonarUntil>previous.sonarUntil&&p.sonarUntil>now&&!activated){Extra("sonar",.6f);sonarAt=now+2;}
   if(p.sonarUntil>now&&now>=sonarAt){Extra("sonar",.28f);sonarAt=now+2;}
   bool absorbed=wasFighting&&fighting&&previous.bubbleGuard&&!p.bubbleGuard&&previous.activeUntil>now;
   if(absorbed)Extra("bubble-block");

   if(!absorbed&&previous.active!=FishingAbility.None&&previous.activeUntil>0&&previous.activeUntil!=expiredAt&&now>=previous.activeUntil&&now-previous.activeUntil<.3&&p.activeUntil<=previous.activeUntil&&p.result!=FishingResult.Cancelled){Extra("gear-expire",.45f);expiredAt=previous.activeUntil;}
   if(p.awardSequence!=previous.awardSequence&&p.phase==FishingPhase.Caught&&fresh){Extra("catch-score",.65f);if(p.lastBonus>0)Extra("wanted-bonus",.7f);}
   int required=fighting?FishingState.CounterSide(p,now):0;
   bool nextCooling=fighting&&(cooling?p.tension>.58f:p.tension>.82f);
   if(wasFighting&&fighting){
    if(nextCooling&&!cooling)Extra("fish-warning",.65f);
    if(cooling&&!nextCooling)Extra("fish-cooled",.6f);
    if(required!=side&&!nextCooling)Extra("fish-switch",.45f,required<0?0:1);
    if(previous.fightOffset.y>0&&p.fightOffset.y<=0&&clock>=surfaceAt){Extra("fish-surge",.65f,FishingState.SizeForFish(p.fish));surfaceAt=clock+.7f;}
   }
   cooling=nextCooling;side=required;
   // Slip follows increasing paid-out line, not simply a high tension meter.
   if(fighting&&wasFighting&&p.lineLength>previous.lineLength+.0005f)slipUntil=clock+.14f;
   audio?.Loop("drag","fish-drag",fighting&&clock<slipUntil,p.origin,.42f,1,true);
   audio?.Loop("salvage","fish-reel",p.phase==FishingPhase.Salvaging,p.origin,.4f,1,true);
  }
  void UpdateLoop(bool held,float tension) {
   loopWanted=held&&MenuPreferences.MasterVolume>.001f&&MenuPreferences.SFXVolume>.001f;
   if(!loopWanted){if(reel.isPlaying)reel.Stop();return;}
   reel.volume=(.45f+Mathf.Clamp01(tension)*.15f)*MenuPreferences.SFXVolume;
   reel.pitch=MenuPreferences.ReducedMotion?1:1+Mathf.Clamp01(tension)*.13f;
   if(!reel.isPlaying)reel.Play();
  }
  void Play(FishingFeedbackCue cue) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   cueCounts[(int)cue]++;
#endif
   if(MenuPreferences.MasterVolume>.001f){effects.volume=MenuPreferences.SFXVolume;effects.PlayOneShot(GolfFishingAudio.Choose(Keys[(int)cue]),cue==FishingFeedbackCue.Strain?.4f:.85f);}
   if(cue==FishingFeedbackCue.Bite||cue==FishingFeedbackCue.Hook||cue==FishingFeedbackCue.Landed||cue==FishingFeedbackCue.Snapped||cue==FishingFeedbackCue.Strain)Haptic();
  }
  void StopSources() {
   if(effects)effects.Stop();if(reel)reel.Stop();if(audio){audio.StopLoop("drag");audio.StopLoop("salvage");}loopWanted=false;
   hapticEpoch++;
  }
  void Quiet(){if(observing||loopWanted||effects&&effects.isPlaying)StopSources();observing=false;highStrain=false;}
  void OnApplicationFocus(bool value){focused=value;if(!value){StopSources();observing=false;}}
  void OnApplicationPause(bool value){paused=value;if(value){StopSources();observing=false;}}
  void OnDisable(){StopSources();observing=false;}
  void OnDestroy() {
   StopSources(); // Imported clips remain owned by the shared Resources bank.
   if(effects)Destroy(effects.gameObject);if(reel)Destroy(reel.gameObject);
  }

  void Haptic() {
   if(!MenuPreferences.FishingVibration||MenuPreferences.ReducedMotion||hapticUnavailable)return;
#if UNITY_ANDROID && !UNITY_EDITOR
   // Native touch feedback supplies a short device-appropriate click, respects
   // Android's own touch-feedback setting, and needs no VIBRATE permission.
   int token=hapticEpoch;AndroidJavaObject activity=null;
   try {
    using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))activity=player.GetStatic<AndroidJavaObject>("currentActivity");
    var current=activity;
    current.Call("runOnUiThread",new AndroidJavaRunnable(()=>{
     try {
      if(hapticEpoch!=token)return;
      using(var window=current.Call<AndroidJavaObject>("getWindow"))
      using(var view=window.Call<AndroidJavaObject>("getDecorView"))
      using(var constants=new AndroidJavaClass("android.view.HapticFeedbackConstants"))
       view.Call<bool>("performHapticFeedback",constants.GetStatic<int>("CONTEXT_CLICK"));
     }catch(Exception){hapticUnavailable=true;}
     finally{current.Dispose();}
    }));
   }catch(Exception){hapticUnavailable=true;activity?.Dispose();}
#endif
  }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
  // Exercises the production cursor and audio sources without fabricating a
  // network identity. Fixtures must supply only the chosen local record.
  public void ReviewConsume(uint currentRound,FishingPlayerRecord player,double now,bool held=true,bool active=true) {
   if(!active){Quiet();return;}
   Consume(currentRound,player,now,held,Time.unscaledTime);
  }
  public void ReviewCheckWaveforms(Action<bool,string> check) {
   foreach(var clip in clips)if(clip)CheckClip(clip,check);
   CheckClip(reelClip,check);
   check(RuntimeSampleBytes<4000000,"FISHING_AUDIO_IMPORTED_SAMPLE_MEMORY_BUDGET");
  }
  static void CheckClip(AudioClip clip,Action<bool,string> check) {
   var data=new float[clip.samples];bool read=clip.GetData(data,0),valid=read,nonzero=false;
   foreach(float sample in data){valid&=float.IsFinite(sample)&&Mathf.Abs(sample)<=.8f;nonzero|=Mathf.Abs(sample)>.001f;}
   check(valid&&nonzero&&clip.channels==1&&clip.frequency==SampleRate,"FISHING_AUDIO_VALID_"+clip.name.Replace(' ','_'));
   check((clip.name.StartsWith("fish-reel")?Mathf.Abs(data[0]-data[data.Length-1])<.035f:Mathf.Abs(data[0])<.02f&&Mathf.Abs(data[data.Length-1])<.02f),"FISHING_AUDIO_FADED_BOUNDARIES_"+clip.name.Replace(' ','_'));
  }
#endif
 }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
 public static class FishingFeedbackChecks {
  public static void Run(Action<bool,string> check) {
   var host=new GameObject("Development fishing feedback review");
   var feedback=host.AddComponent<FishingFeedback>();
   bool motion=MenuPreferences.ReducedMotion,vibration=MenuPreferences.FishingVibration;
   float volume=MenuPreferences.MasterVolume;
   try {
    // Keep this fixture tactile-silent and respect the review runner's volume.
    MenuPreferences.SetFishingVibration(false);
    feedback.ReviewCheckWaveforms(check);
    var p=new FishingPlayerRecord{owner=7,connected=true,phase=FishingPhase.Ready};
    feedback.ReviewConsume(1,p,10);
    p.sequence=1;p.phase=FishingPhase.Casting;p.phaseAt=10;
    feedback.ReviewConsume(1,p,10);feedback.ReviewConsume(1,p,10.1);
    check(feedback.ReviewCueCount(FishingFeedbackCue.Cast)==1,"FISHING_AUDIO_CAST_ONCE_PER_ACCEPTED_SEQUENCE");
    p.phase=FishingPhase.Waiting;feedback.ReviewConsume(1,p,10.7);
    check(feedback.ReviewCueCount(FishingFeedbackCue.Splash)==1,"FISHING_AUDIO_SPLASH_AT_FLOAT_ENTRY");
    p.phase=FishingPhase.Bite;p.biteAt=12;p.hookUntil=13.8;
    feedback.ReviewConsume(1,p,12);feedback.ReviewConsume(1,p,12.05);
    check(feedback.ReviewCueCount(FishingFeedbackCue.Bite)==1,"FISHING_AUDIO_BITE_DOES_NOT_REPEAT_SNAPSHOTS");
    p.phase=FishingPhase.Reeling;p.phaseAt=12.3;p.held=true;p.tension=.2f;p.steer=(sbyte)FishingState.CounterSide(p,12.3);
    feedback.ReviewConsume(1,p,12.3);
    check(feedback.ReviewCueCount(FishingFeedbackCue.Hook)==1,"FISHING_AUDIO_HOOK_ACKNOWLEDGES_ACCEPTANCE");
    check(feedback.ReviewLoopWanted==(volume>.001f&&MenuPreferences.SFXVolume>.001f),"FISHING_AUDIO_REEL_OBEYS_GAME_VOLUME");
    feedback.ReviewConsume(1,p,12.4,false);
    check(feedback.ReviewLoopWanted==(volume>.001f&&MenuPreferences.SFXVolume>.001f),"FISHING_AUDIO_TAP_RELEASE_PRESERVES_ACCEPTED_SHORT_STROKE");
    p.held=false;feedback.ReviewConsume(1,p,12.45,false);check(!feedback.ReviewLoopWanted,"FISHING_AUDIO_FINISHED_STROKE_STOPS_LOOP");p.held=true;
    feedback.ReviewConsume(1,p,12.5,true,false);
    check(!feedback.ReviewLoopWanted,"FISHING_AUDIO_CONTEXT_EXIT_STOPS_LOOP");
    feedback.ReviewConsume(1,p,12.6);feedback.ReviewConsume(1,p,12.7);
    check(feedback.ReviewCueCount(FishingFeedbackCue.Hook)==1,"FISHING_AUDIO_RESUMED_REEL_HAS_NO_STALE_HOOK_PROMPT");
    feedback.SendMessage("OnApplicationFocus",false);check(!feedback.ReviewLoopWanted,"FISHING_AUDIO_FOCUS_LOSS_STOPS_LOOP");
    feedback.ReviewConsume(1,p,12.8);feedback.SendMessage("OnApplicationPause",true);
    check(!feedback.ReviewLoopWanted,"FISHING_AUDIO_PAUSE_STOPS_LOOP");
    feedback.ReviewConsume(2,p,12.9);
    check(!feedback.ReviewLoopWanted&&feedback.ReviewCueCount(FishingFeedbackCue.Hook)==1,"FISHING_AUDIO_ROUND_CHANGE_SILENT_BASELINE");
    p.sequence=2;p.phase=FishingPhase.Bite;p.biteAt=15;p.hookUntil=16.8;
    feedback.ReviewConsume(2,p,17);
    check(feedback.ReviewCueCount(FishingFeedbackCue.Bite)==1,"FISHING_AUDIO_EXPIRED_BITE_SUPPRESSED");
    p.sequence=3;p.phase=FishingPhase.Casting;p.phaseAt=10;feedback.ReviewConsume(2,p,18);
    check(feedback.ReviewCueCount(FishingFeedbackCue.Cast)==1,"FISHING_AUDIO_OLD_CAST_SUPPRESSED");
    p.sequence=2;p.phaseAt=18;feedback.ReviewConsume(2,p,18);
    check(feedback.ReviewCueCount(FishingFeedbackCue.Cast)==1,"FISHING_AUDIO_REGRESSIVE_SEQUENCE_SUPPRESSED");
    p.owner=8;p.sequence=4;p.phase=FishingPhase.Bite;p.biteAt=18;p.hookUntil=19.8;
    feedback.ReviewConsume(2,p,18);
    check(feedback.ReviewCueCount(FishingFeedbackCue.Bite)==1,"FISHING_AUDIO_NEW_IDENTITY_HAS_SILENT_BASELINE");
    p.sequence=5;p.phase=FishingPhase.Caught;p.phaseAt=19;p.result=FishingResult.Landed;
    feedback.ReviewConsume(2,p,19);feedback.ReviewConsume(2,p,19.1);
    check(feedback.ReviewCueCount(FishingFeedbackCue.Landed)==1&&!feedback.ReviewLoopWanted,"FISHING_AUDIO_LANDING_ONCE_NO_REEL_LOOP");
    p.sequence=6;p.phase=FishingPhase.Escaped;p.phaseAt=20;p.result=FishingResult.Snapped;
    feedback.ReviewConsume(2,p,20);feedback.ReviewConsume(2,p,20.1);
    check(feedback.ReviewCueCount(FishingFeedbackCue.Snapped)==1,"FISHING_AUDIO_SNAP_ONCE");
    p.sequence=7;p.phaseAt=21;p.result=FishingResult.Missed;feedback.ReviewConsume(2,p,21);
    check(feedback.ReviewCueCount(FishingFeedbackCue.Missed)==1,"FISHING_AUDIO_MISSED_HOOK_FEEDBACK");
    p.sequence=8;p.phaseAt=22;p.result=FishingResult.Cancelled;feedback.ReviewConsume(2,p,22);
    check(feedback.ReviewCueCount(FishingFeedbackCue.Missed)==1&&feedback.ReviewCueCount(FishingFeedbackCue.Snapped)==1,"FISHING_AUDIO_CANCEL_HAS_NO_FALSE_FAILURE_CUE");
    MenuPreferences.SetMaster(0);p.sequence=9;p.phase=FishingPhase.Reeling;p.phaseAt=23;p.held=true;feedback.ReviewConsume(2,p,23);
    check(!feedback.ReviewLoopWanted,"FISHING_AUDIO_MUTED_LOOP_STAYS_STOPPED");
    MenuPreferences.SetMotion(true);feedback.ReviewConsume(2,p,23.1);
    check(!feedback.ReviewLoopWanted,"FISHING_AUDIO_REDUCED_MOTION_PRESERVES_MUTE");
    feedback.enabled=false;check(!feedback.ReviewLoopWanted,"FISHING_AUDIO_DISABLE_STOPS_LOOP");
   }finally {
    MenuPreferences.SetMaster(volume);MenuPreferences.SetMotion(motion);MenuPreferences.SetFishingVibration(vibration);
    UnityEngine.Object.Destroy(host);
   }
  }
 }
#endif
}
