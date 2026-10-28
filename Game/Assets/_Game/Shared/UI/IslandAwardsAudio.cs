using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhatTheFish {
 // Local presentation voices belong to the ceremony, never to a real Athlete.
 public sealed class IslandAwardsAudio:MonoBehaviour {
  sealed class Walker {public Transform left,right;public float phase,age=-1;public bool leftPlant,rightPlant;}
  readonly List<Walker> walkers=new();readonly List<AudioSource> steps=new();
  AudioSource water,music,cheers;Camera camera;GolfLeaderboardUI golfResults;FishingResultsUI fishingResults;
  bool golf,finished,celebrated,focused=true,paused;float duck=1,waterGain;int voice;
  public int StepCount {get;private set;}public int CelebrationCount {get;private set;}
  public float CelebrationGain=>duck;public bool WaterPlaying=>water&&water.isPlaying;
  public string StepCue=>golf?"grass-walk":"wood-steps";
  public static IslandAwardsAudio Create(Transform parent,IEnumerable<Transform> cast,bool golf,Camera camera){
   var audio=new GameObject("Island awards sound").AddComponent<IslandAwardsAudio>();audio.transform.SetParent(parent,false);
   audio.golf=golf;audio.camera=camera;audio.focused=Application.isFocused;
   audio.golfResults=FindFirstObjectByType<GolfLeaderboardUI>();audio.fishingResults=FindFirstObjectByType<FishingResultsUI>();
   foreach(var actor in cast){var bones=actor.GetComponentsInChildren<Transform>();audio.walkers.Add(new Walker{left=Array.Find(bones,t=>t.name=="mixamorig:LeftFoot"),right=Array.Find(bones,t=>t.name=="mixamorig:RightFoot")});}
   for(int i=0;i<8;i++)audio.steps.Add(audio.Source("Awards planted step",true));
   audio.water=audio.Source("Podium gentle water");audio.water.clip=GolfFishingAudio.Choose("shore-waves",0);audio.water.loop=true;
   audio.music=audio.Source("Awards fanfare");audio.music.clip=GolfFishingAudio.Choose("cup-champion",0);
   audio.cheers=audio.Source("Awards cheers");audio.cheers.clip=Resources.Load<AudioClip>("IslandAwards/Cheers");return audio;
  }
  AudioSource Source(string name,bool spatial=false){var node=new GameObject(name);node.transform.SetParent(transform,false);var s=node.AddComponent<AudioSource>();s.playOnAwake=false;s.spatialBlend=spatial?.55f:0;s.dopplerLevel=0;s.reverbZoneMix=0;s.rolloffMode=AudioRolloffMode.Linear;s.minDistance=5;s.maxDistance=34;return s;}
  bool Audible=>focused&&!paused&&!AudioListener.pause&&MenuPreferences.MasterVolume>.001f&&MenuPreferences.SFXVolume>.001f;
  bool ResultsVisible=>finished||(golf?golfResults&&golfResults.Expanded:fishingResults&&fishingResults.Visible);
  // Called with the very phase used to pose each actor, after foot IK.
  public void ObserveWalk(int index,float phase,float age,bool walking){
   var w=walkers[index];bool continuous=w.age>=0&&age>=w.age&&age-w.age<.2f;
   w.leftPlant=continuous&&walking&&Mathf.FloorToInt(phase)>Mathf.FloorToInt(w.phase);
   w.rightPlant=continuous&&walking&&Mathf.FloorToInt(phase+.5f)>Mathf.FloorToInt(w.phase+.5f);
   w.phase=phase;w.age=age;
  }
  public void Present(float age,float settleAt){
   if(finished)return;
   // Camera has now been posed too. Emit only the plants reported by animation.
   foreach(var w in walkers){if(w.leftPlant)Step(w.left);if(w.rightPlant)Step(w.right);w.leftPlant=w.rightPlant=false;}
   waterGain=!golf&&!MenuPreferences.ReducedMotion?Mathf.SmoothStep(0,1,(age-1.7f)/.25f)*(1-Mathf.SmoothStep(0,1,(age-3.55f)/.3f)):0;
   if(Audible&&waterGain>.001f){if(!water.isPlaying)water.Play();}else water.Stop();
   // Consume once even when muted or unfocused; returning must not replay it.
   if(!celebrated&&age>=settleAt){celebrated=true;if(Audible){music.Play();if(cheers.clip)cheers.Play();CelebrationCount++;}}
   Mix();
  }
  void Step(Transform foot){
   if(!Audible||!foot||!camera)return;var p=camera.WorldToViewportPoint(foot.position);
   if(p.z<=0||p.x<.02f||p.x>.98f||p.y<.09f||p.y>.92f)return;
   var s=steps[voice++%steps.Count];s.transform.position=foot.position;s.clip=Resources.Load<AudioClip>("IslandAwards/"+(golf?"GrassPlant":"WoodPlant")+((StepCount%4)+1));s.volume=StepGain(foot.position)*MenuPreferences.SFXVolume;s.Play();StepCount++;
  }
  float StepGain(Vector3 point)=>.38f*Mathf.Lerp(.35f,1,Mathf.InverseLerp(18,6,Vector3.Distance(camera.transform.position,point)));
  void Mix(){
   duck=Mathf.MoveTowards(duck,ResultsVisible?.25f:1,Time.unscaledDeltaTime*3);
   float gain=Audible?MenuPreferences.SFXVolume:0;
   water.volume=.22f*waterGain*gain;music.volume=.58f*duck*gain;cheers.volume=.48f*duck*gain;
   foreach(var s in steps)s.volume=StepGain(s.transform.position)*gain;
  }
  void Update(){Mix();if(finished&&!music.isPlaying&&!cheers.isPlaying)Destroy(gameObject);}
  public void Complete(bool showResults){
   water.Stop();foreach(var step in steps)step.Stop();
   if(!showResults){StopSources();Destroy(gameObject);}else finished=true;
  }
  void StopSources(){if(water)water.Stop();if(music)music.Stop();if(cheers)cheers.Stop();foreach(var s in steps)if(s)s.Stop();}
  void OnApplicationFocus(bool value){focused=value;if(!value)StopSources();}
  void OnApplicationPause(bool value){paused=value;if(value)StopSources();}
  void OnDisable()=>StopSources();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public void ReviewFocus(bool value)=>OnApplicationFocus(value);
#endif
 }
}
