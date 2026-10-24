using UnityEngine;

namespace WhatTheFish {
 // One small shared procedural wind loop, prepared during scene loading.
 // Each visible funnel owns only a spatial voice; no gameplay/network changes.
 public sealed class GolfTornadoAudio:MonoBehaviour {
  static AudioClip loop;
  AudioSource voice;float level;
  public static void Prepare(){
   if(loop)return;
   const int rate=22050,count=rate*6,overlap=rate/8;
   var noise=new System.Random(873421);var raw=new float[count+overlap];float low=0,body=0;
   for(int i=-rate/2;i<raw.Length;i++){
    float white=(float)(noise.NextDouble()*2-1);
    low+=.045f*(white-low);body+=.22f*(white-body);
    if(i<0)continue;
    float t=i/(float)rate;
    float gust=.68f+.19f*Mathf.Sin(t*Mathf.PI*2*.5f)+.09f*Mathf.Sin(t*Mathf.PI*2*1.33f);
    float rumble=.055f*Mathf.Sin(t*Mathf.PI*2*47)+.025f*Mathf.Sin(t*Mathf.PI*2*73);
    raw[i]=Mathf.Clamp((low*2.1f+body*.48f+rumble)*gust,-.85f,.85f);
   }
   var samples=new float[count];
   for(int i=0;i<count;i++){
    // Crossfade the tail into the warm-up overlap; the next loop sample is
    // contiguous with that overlap, avoiding a click at the wrap point.
    float value=raw[i+overlap];int tail=i-(count-overlap);
    if(tail>=0)value=Mathf.Lerp(value,raw[tail],Mathf.SmoothStep(0,1,tail/(float)(overlap-1)));
    samples[i]=value;
   }
   loop=AudioClip.Create("Golf tornado turbulent wind loop",count,1,rate,false);loop.SetData(samples,0);
  }
  public void Tick(float growth,float opacity,int lane){
   var bus=GolfFishingAudio.Golf;
   if(!bus||!bus.Audible||opacity<=.001f||MenuPreferences.SFXVolume<=.001f||MenuPreferences.MasterVolume<=.001f){Silence();return;}
   if(!voice){
    Prepare();var node=new GameObject("Tornado spatial wind");node.transform.SetParent(transform,false);voice=node.AddComponent<AudioSource>();
    voice.playOnAwake=false;voice.loop=true;voice.clip=loop;voice.spatialBlend=1;voice.dopplerLevel=0;voice.reverbZoneMix=0;
    voice.rolloffMode=AudioRolloffMode.Linear;voice.minDistance=4;voice.maxDistance=42;voice.spread=110;voice.priority=180;voice.volume=0;
   }
   voice.transform.localPosition=Vector3.up*Mathf.Lerp(.25f,1.5f,growth);
   voice.pitch=Mathf.Lerp(1.15f,.88f,growth);
   float target=.32f*Mathf.Lerp(.18f,1,growth)*Mathf.Clamp01(opacity)*MenuPreferences.SFXVolume;
   level=Mathf.MoveTowards(level,target,Time.unscaledDeltaTime*.65f);voice.volume=level;
   if(!voice.isPlaying){voice.time=Mathf.Repeat(lane*1.731f,loop.length);voice.Play();}
  }
  public void Silence(){level=0;if(voice){voice.volume=0;voice.Stop();}}
  void OnApplicationFocus(bool focused){if(!focused)Silence();}
  void OnApplicationPause(bool paused){if(paused)Silence();}
  void OnDisable()=>Silence();
 }
}
