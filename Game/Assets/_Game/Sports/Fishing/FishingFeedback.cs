using System;
using UnityEngine;

namespace WhatTheFish {
 public enum FishingFeedbackCue:byte {None,Cast,Splash,Bite,Hook,Landed,Missed,Snapped,Strain}

 // Short, shared mono waveforms are synthesized once per lagoon. No imported
 // audio is shipped, and only this device's accepted fishing record makes cues.
 [DefaultExecutionOrder(95)]
 public sealed class FishingFeedback:MonoBehaviour {
  const int SampleRate=22050;
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
   game=GetComponent<FishingGame>();focused=Application.isFocused;
   effects=Source("Fishing action sounds");reel=Source("Fishing reel sound");
   reel.loop=true;reelClip=MakeLoop();reel.clip=reelClip;
   for(int i=1;i<clips.Length;i++)clips[i]=MakeCue((FishingFeedbackCue)i);
  }
  AudioSource Source(string name) {
   var child=new GameObject(name);child.transform.SetParent(transform,false);
   var source=child.AddComponent<AudioSource>();source.playOnAwake=false;
   source.spatialBlend=0;source.volume=1;source.priority=80;
   source.ignoreListenerPause=false;source.dopplerLevel=0;return source;
  }
  void LateUpdate() {
   var app=AppRoot.Instance;var actor=app?app.LocalAthlete:null;
   bool active=focused&&!paused&&Time.timeScale>0&&!AudioListener.pause&&game&&game.isActiveAndEnabled&&game.Context&&game.State.Running&&actor&&actor.isActiveAndEnabled&&!actor.inTransit&&app.view&&app.view.active&&(!app.Menu||!app.Menu.IsTransitioning);
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
    sequence=player.sequence;seen=Through(player.phase);highStrain=player.tension>=.74f;
    return; // Initial/late-join and resumed snapshots are a silent baseline.
   }
   if(sequence!=player.sequence) {
    if(unchecked((int)(player.sequence-sequence))<0)return;
    sequence=player.sequence;seen=0;highStrain=false;
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
   bool held=locallyHeld&&player.held&&player.phase==FishingPhase.Reeling;
   UpdateLoop(held,player.tension);
   if(player.tension<.56f)highStrain=false;
   if(held&&player.tension>=.74f&&!highStrain&&clock>=strainAgainAt) {
    highStrain=true;strainAgainAt=clock+1.25f;Play(FishingFeedbackCue.Strain);
   }
  }
  void UpdateLoop(bool held,float tension) {
   loopWanted=held&&MenuPreferences.MasterVolume>.001f;
   if(!loopWanted){if(reel.isPlaying)reel.Stop();return;}
   reel.volume=.055f+Mathf.Clamp01(tension)*.035f;
   reel.pitch=MenuPreferences.ReducedMotion?1:1+Mathf.Clamp01(tension)*.13f;
   if(!reel.isPlaying)reel.Play();
  }
  void Play(FishingFeedbackCue cue) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   cueCounts[(int)cue]++;
#endif
   if(MenuPreferences.MasterVolume>.001f)effects.PlayOneShot(clips[(int)cue],cue==FishingFeedbackCue.Bite?.36f:cue==FishingFeedbackCue.Strain?.2f:.24f);
   if(cue==FishingFeedbackCue.Bite||cue==FishingFeedbackCue.Hook||cue==FishingFeedbackCue.Snapped||cue==FishingFeedbackCue.Strain)Haptic();
  }
  void StopSources() {
   if(effects)effects.Stop();if(reel)reel.Stop();loopWanted=false;
   hapticEpoch++;
  }
  void Quiet(){if(observing||loopWanted||effects&&effects.isPlaying)StopSources();observing=false;highStrain=false;}
  void OnApplicationFocus(bool value){focused=value;if(!value){StopSources();observing=false;}}
  void OnApplicationPause(bool value){paused=value;if(value){StopSources();observing=false;}}
  void OnDisable(){StopSources();observing=false;}
  void OnDestroy() {
   StopSources();foreach(var clip in clips)if(clip)Destroy(clip);
   if(reelClip)Destroy(reelClip);
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

  AudioClip Clip(string name,float[] samples) {
   var clip=AudioClip.Create(name,samples.Length,1,SampleRate,false);clip.SetData(samples,0);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   RuntimeSampleBytes+=samples.Length*sizeof(float);
#endif
   return clip;
  }
  static float Tone(float hz,float t)=>Mathf.Sin(2*Mathf.PI*hz*t);
  AudioClip MakeCue(FishingFeedbackCue cue) {
   float duration=cue switch {FishingFeedbackCue.Cast=>.24f,FishingFeedbackCue.Splash=>.22f,FishingFeedbackCue.Bite=>.22f,FishingFeedbackCue.Hook=>.12f,FishingFeedbackCue.Landed=>.2f,FishingFeedbackCue.Missed=>.16f,FishingFeedbackCue.Snapped=>.12f,_=>.12f};
   var samples=new float[Mathf.RoundToInt(duration*SampleRate)];uint noise=0x624FA83D;float filtered=0;
   for(int i=0;i<samples.Length;i++) {
    float t=(float)i/SampleRate,u=t/duration;
    noise^=noise<<13;noise^=noise>>17;noise^=noise<<5;
    float n=(noise&0xFFFF)/32767.5f-1;filtered=Mathf.Lerp(filtered,n,.17f);
    float attack=Mathf.Min(1,t/.006f),tail=Mathf.Pow(1-u,2),value;
    switch(cue) {
     case FishingFeedbackCue.Cast:value=(filtered*.65f+Tone(620-1600*t,t)*.14f)*Mathf.Sin(Mathf.PI*u);break;
     case FishingFeedbackCue.Splash:value=(filtered*.7f+Tone(270-480*t,t)*.18f)*tail;break;
     case FishingFeedbackCue.Bite:float segment=Mathf.Repeat(t,.11f);value=(Tone(t<.11f?740:990,segment)*.36f+Tone(1480,segment)*.06f)*Mathf.Sin(Mathf.PI*segment/.11f);break;
     case FishingFeedbackCue.Hook:value=(Tone(430+2100*t,t)*.34f+n*.07f)*tail;break;
     // A quiet water plop marks an ordinary landing; it is not a win fanfare.
     case FishingFeedbackCue.Landed:value=(Tone(190-350*t,t)*.3f+filtered*.4f)*tail;break;
     case FishingFeedbackCue.Missed:value=Tone(310-650*t,t)*.24f*tail;break;
     case FishingFeedbackCue.Snapped:value=(n*.42f+Tone(1200,t)*.17f)*Mathf.Exp(-t*35);break;
     default:value=(Tone(660,t)*.3f+Tone(880,t)*.1f)*tail;break;
    }
    samples[i]=Mathf.Clamp(value*attack*Mathf.Min(1,(duration-t)/.006f),-.8f,.8f);
   }
   return Clip("Fishing "+cue,samples);
  }
  AudioClip MakeLoop() {
   var samples=new float[3528]; // Four exactly repeated ratchet ticks, 160 ms.
   for(int i=0;i<samples.Length;i++) {
    float t=(float)(i%882)/SampleRate;
    float envelope=t<.014f?Mathf.Sin(Mathf.PI*t/.014f):0;
    samples[i]=(Tone(775,t)*.36f+Tone(1425,t)*.19f)*envelope;
   }
   return Clip("Fishing soft reel ratchet",samples);
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
   check(RuntimeSampleBytes<160000,"FISHING_AUDIO_GENERATED_MONO_SAMPLE_BUDGET");
  }
  static void CheckClip(AudioClip clip,Action<bool,string> check) {
   var data=new float[clip.samples];bool read=clip.GetData(data,0),valid=read,nonzero=false;
   foreach(float sample in data){valid&=float.IsFinite(sample)&&Mathf.Abs(sample)<=.8f;nonzero|=Mathf.Abs(sample)>.001f;}
   check(valid&&nonzero&&clip.channels==1&&clip.frequency==SampleRate,"FISHING_AUDIO_VALID_"+clip.name.Replace(' ','_'));
   check(Mathf.Abs(data[0])<.001f&&Mathf.Abs(data[data.Length-1])<.001f,"FISHING_AUDIO_FADED_BOUNDARIES_"+clip.name.Replace(' ','_'));
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
    p.phase=FishingPhase.Reeling;p.phaseAt=12.3;p.held=true;p.tension=.2f;
    feedback.ReviewConsume(1,p,12.3);
    check(feedback.ReviewCueCount(FishingFeedbackCue.Hook)==1,"FISHING_AUDIO_HOOK_ACKNOWLEDGES_ACCEPTANCE");
    check(feedback.ReviewLoopWanted==(volume>.001f),"FISHING_AUDIO_REEL_OBEYS_GAME_VOLUME");
    feedback.ReviewConsume(1,p,12.4,false);
    check(!feedback.ReviewLoopWanted,"FISHING_AUDIO_RELEASE_STOPS_LOOP_IMMEDIATELY");
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
