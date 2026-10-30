using UnityEngine;

namespace WhatTheFish {
 // All speech is packaged locally. Generation credentials never enter the player.
 public sealed class FirstVisitNarration : MonoBehaviour {
  AudioSource voice;int step=-1;
  void Awake(){voice=gameObject.AddComponent<AudioSource>();voice.playOnAwake=false;voice.loop=false;voice.spatialBlend=0;voice.ignoreListenerPause=true;voice.volume=MenuPreferences.UIVolume;}
  void OnEnable(){GameLocalization.Changed+=LanguageChanged;}
  void OnDisable(){GameLocalization.Changed-=LanguageChanged;Stop();}
  void Update(){if(voice)voice.volume=MenuPreferences.UIVolume;}
  void LanguageChanged(){if(step>=0)PlayStep(step);}
  public void PlayStep(int value){
   step=value;Stop();if(!voice)return;
   voice.clip=Resources.Load<AudioClip>("FirstVisitNarration/"+GameLocalization.Code+"/"+(step+1).ToString("00"));
   voice.volume=MenuPreferences.UIVolume;if(voice.clip)voice.Play();
  }
  public void Stop(){if(voice)voice.Stop();}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public AudioSource Source=>voice;
#endif
 }
}
