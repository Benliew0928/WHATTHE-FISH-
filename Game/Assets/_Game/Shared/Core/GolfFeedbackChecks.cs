#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public static class GolfFeedbackChecks {
  public static IEnumerator Motion(GolfFeedback feedback,Athlete actor,Action<bool,string> check){
   bool motion=MenuPreferences.ReducedMotion,shake=feedback.Settings.cameraShake;var point=actor.transform.position+Vector3.up*.5f;ulong owner=GolfCartWorld.Key(actor);
   try {
    MenuPreferences.SetMotion(false);feedback.Settings.cameraShake=true;
    yield return new WaitForSecondsRealtime(.13f);
    GolfFeedback.Emit(GolfFeedbackCue.Swing,point,Vector3.up,1,owner,owner);yield return null;yield return new WaitForEndOfFrame();
    check(feedback.LastCameraKick>0&&feedback.LastCameraKick<=feedback.Settings.shakeDistance,"STRONG_LOCAL_SWING_HAS_BOUNDED_SHORT_CAMERA_KICK");
    yield return new WaitForSecondsRealtime(.14f);yield return new WaitForEndOfFrame();check(feedback.LastCameraKick==0,"CAMERA_KICK_EXPIRES_WITHOUT_DRIFT");
    GolfFeedback.Emit(GolfFeedbackCue.Putt,point,Vector3.up,1,owner,owner);yield return null;yield return new WaitForEndOfFrame();check(feedback.LastCameraKick==0,"PUTT_NEVER_KICKS_CAMERA");
    GolfFeedback.Emit(GolfFeedbackCue.Swing,point,Vector3.up,1,999,owner);yield return null;yield return new WaitForEndOfFrame();check(feedback.LastCameraKick==0,"OTHER_PLAYER_CONTACT_NEVER_KICKS_LOCAL_CAMERA");
    MenuPreferences.SetMotion(true);GolfFeedback.Emit(GolfFeedbackCue.Swing,point,Vector3.up,1,owner,owner);yield return null;yield return new WaitForEndOfFrame();check(feedback.LastCameraKick==0,"REDUCED_MOTION_SUPPRESSES_CAMERA_KICK");
    MenuPreferences.SetMotion(false);feedback.Settings.cameraShake=false;GolfFeedback.Emit(GolfFeedbackCue.Swing,point,Vector3.up,1,owner,owner);yield return null;yield return new WaitForEndOfFrame();check(feedback.LastCameraKick==0,"INSPECTOR_OPTION_SUPPRESSES_CAMERA_KICK");
    feedback.ReviewFocus(false);yield return null;check(!feedback.ChargeLoopPlaying&&feedback.ParticleCount==0&&!feedback.ChargeRingVisible,"FOCUS_LOSS_CLEARS_AUDIO_PARTICLES_AND_CHARGE_UI");feedback.ReviewFocus(true);
   }finally {MenuPreferences.SetMotion(motion);feedback.Settings.cameraShake=shake;feedback.ReviewFocus(true);}
  }
  public static void Run(GolfFeedback feedback,GolfMatchManager match,Action<bool,string> check){
   check(feedback&&feedback.Settings,"GOLF_FEEDBACK_AND_CENTRAL_SETTINGS_READY");
   foreach(GolfFeedbackCue cue in Enum.GetValues(typeof(GolfFeedbackCue))){
    var clip=feedback.CueClip(cue);var samples=new float[clip.samples];clip.GetData(samples,0);float peak=0,energy=0;
    foreach(float sample in samples){peak=Mathf.Max(peak,Mathf.Abs(sample));energy+=sample*sample;}
    check(clip.channels==1&&clip.frequency==32000&&peak>.05f&&peak<.9f&&energy/samples.Length>.00005f&&Mathf.Abs(samples[0])<.01f&&Mathf.Abs(samples[^1])<.02f,"AUDIBLE_UNCLIPPED_MONO_CUE_WITH_CLEAN_EDGES_"+cue);
   }
   var e=new GolfFeedbackEvent{sequence=100,round=match.Round,time=match.Now,actor=GolfCartWorld.OfflineOwner,owner=GolfCartWorld.OfflineOwner,cue=GolfFeedbackCue.Turf,point=match.transform.position,normal=Vector3.up,strength=.5f};
   using(var writer=new FastBufferWriter(96,Allocator.Temp)){
    writer.WriteNetworkSerializable(e);check(writer.Length==61,"GOLF_FEEDBACK_WIRE_SIZE_IS_BOUNDED");
    using var reader=new FastBufferReader(writer,Allocator.Temp);reader.ReadNetworkSerializable(out GolfFeedbackEvent copy);
    check(copy.sequence==e.sequence&&copy.round==e.round&&copy.actor==e.actor&&copy.owner==e.owner&&copy.normal==e.normal&&copy.point==e.point&&copy.cue==e.cue&&copy.strength==e.strength&&copy.time==e.time,"GOLF_EVENT_WIRE_ROUNDTRIP");
   }
   int before=feedback.CueCount(GolfFeedbackCue.Turf);
   e.time=match.Now-2;check(!feedback.Accept(e),"STALE_COSMETIC_EVENT_IS_NOT_REPLAYED");e.time=match.Now;e.round++;
   check(!feedback.Accept(e),"OTHER_ROUND_COSMETIC_EVENT_REJECTED");e.round=match.Round;e.strength=float.NaN;
   check(!feedback.Accept(e),"NONFINITE_COSMETIC_EVENT_REJECTED");e.strength=.5f;
   check(feedback.CueCount(GolfFeedbackCue.Turf)==before,"REJECTED_EVENTS_HAVE_NO_AUDIO_OR_EFFECT");
   check(feedback.Accept(e)&&!feedback.Accept(e)&&feedback.CueCount(GolfFeedbackCue.Turf)==before+1,"DUPLICATE_COSMETIC_EVENT_PLAYS_EXACTLY_ONCE");
   var renderer=feedback.GetComponentInChildren<ParticleSystemRenderer>();check(renderer&&renderer.sharedMaterial&&renderer.sharedMaterial.shader.isSupported,"GOLF_PARTICLE_SHADER_IS_INCLUDED_AND_SUPPORTED");
  }
 }
}
#endif
