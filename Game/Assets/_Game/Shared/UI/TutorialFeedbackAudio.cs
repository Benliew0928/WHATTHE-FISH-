using UnityEngine;

namespace WhatTheFish {
 // A short, warm ascending chime, generated once per tutorial. No downloaded
 // audio or voice is required; UI and master volume retain their normal roles.
 public sealed class TutorialFeedbackAudio:MonoBehaviour {
  AudioSource source;AudioClip chime;
  public int PlayCount {get;private set;}
  public float Gain=>source?source.volume:0;
  void Awake(){
   source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;source.dopplerLevel=0;
   const int rate=22050;var samples=new float[(int)(rate*.78f)];float[] notes={523.25f,659.25f,783.99f};
   for(int i=0;i<samples.Length;i++){
    float time=(float)i/rate,value=0;
    for(int n=0;n<notes.Length;n++){float age=time-n*.12f;if(age<0)continue;
     float envelope=Mathf.Min(1,age/.012f)*Mathf.Exp(-age*8)*Mathf.Clamp01((.78f-time)/.08f);
     value+=(Mathf.Sin(2*Mathf.PI*notes[n]*age)+.16f*Mathf.Sin(4*Mathf.PI*notes[n]*age))*envelope*.22f;
    }samples[i]=value;
   }
   chime=AudioClip.Create("Tutorial success chime",samples.Length,1,rate,false);chime.SetData(samples,0);source.clip=chime;
  }
  public void PlaySuccess(){
   source.volume=MenuPreferences.UIVolume;
   if(MenuPreferences.MasterVolume<=.001f||source.volume<=.001f||AudioListener.pause)return;
   source.Play();PlayCount++;
  }
  void Update(){source.volume=MenuPreferences.UIVolume;}
  void OnApplicationPause(bool paused){if(paused)source.Stop();}
  void OnApplicationFocus(bool focused){if(!focused&&!Application.isBatchMode)source.Stop();}
  void OnDestroy(){if(chime)Destroy(chime);}
 }
}
