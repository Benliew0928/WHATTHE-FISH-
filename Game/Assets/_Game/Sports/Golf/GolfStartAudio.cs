using UnityEngine;

namespace WhatTheFish {
 public sealed class GolfStartAudio:MonoBehaviour {
  GolfMatchManager match;GolfFeedbackSettings settings;AudioSource voice;AudioClip beep,whistle;int previous;bool starting,focused=true,ownsWhistle;
  public int Beeps {get;private set;}public int Whistles {get;private set;}
  public AudioClip Beep=>beep;public AudioClip Whistle=>whistle;
  void Awake(){match=GetComponent<GolfMatchManager>();settings=Resources.Load<GolfFeedbackSettings>("GolfFeedback");voice=gameObject.AddComponent<AudioSource>();voice.playOnAwake=false;voice.spatialBlend=0;voice.dopplerLevel=0;beep=GolfFeedbackSounds.StartSignal(false);whistle=Resources.Load<AudioClip>("GolfAudio/go");ownsWhistle=!whistle;if(ownsWhistle)whistle=GolfFeedbackSounds.StartSignal(true);}
  void Update(){
   bool active=match.Context&&focused&&!AudioListener.pause&&Time.timeScale>0;
   if(!active){starting=false;previous=0;voice.Stop();return;}
   if(match.Starting){int second=Mathf.Clamp(Mathf.CeilToInt((float)match.StartRemaining),1,5);if(!starting||second!=previous){voice.clip=beep;voice.volume=settings.countdownVolume*MenuPreferences.SFXVolume;voice.Play();Beeps++;}previous=second;starting=true;}
   else {if(starting&&match.State.Running){voice.clip=whistle;voice.volume=settings.whistleVolume*MenuPreferences.SFXVolume;voice.Play();Whistles++;}starting=false;previous=0;}
  }
  void OnApplicationFocus(bool value){focused=value;if(!value){voice.Stop();starting=false;}}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public void ReviewFocus(bool value){focused=value;}
#endif
  void OnDisable(){if(voice)voice.Stop();starting=false;}
  void OnDestroy(){if(voice)Destroy(voice);}
 }
}
