using System;
using UnityEngine;

namespace WhatTheFish {
 // Deterministic short foley layers and quiet musical cues, shared per island.
 // Runtime clips follow the existing Fishing feedback architecture.
 public sealed class GolfFeedbackSounds:IDisposable {
  public const int Rate=24000;
  static readonly float[] Notes={523.25f,659.25f,784f,1046.5f};
  public readonly AudioClip[] cues=new AudioClip[8];
  public readonly AudioClip charge;
  public GolfFeedbackSounds(){
   for(int k=0;k<cues.Length;k++)cues[k]=Create(k);
   var data=new float[Rate/2];
   for(int i=0;i<data.Length;i++){float t=(float)i/Rate;float envelope=Mathf.Min(1,i/120f)*Mathf.Min(1,(data.Length-1-i)/120f);data[i]=envelope*(Tone(220,t)*.15f+Tone(440,t)*.035f)*(1+.1f*Mathf.Sin(t*4*Mathf.PI));}
   charge=Clip("Golf soft charge loop",data);
  }
  static float Tone(float hz,float t)=>Mathf.Sin(2*Mathf.PI*hz*t);
  static AudioClip Clip(string name,float[] data){var clip=AudioClip.Create(name,data.Length,1,Rate,false);clip.SetData(data,0);return clip;}
  static AudioClip Create(int cue){
   float duration=cue switch {0=>.17f,1=>.15f,2=>.11f,3=>.18f,4=>.23f,5=>.48f,6=>.9f,7=>.16f,_=>.2f};
   var data=new float[Mathf.RoundToInt(duration*Rate)];uint seed=0xDA781347;float low=0;
   for(int i=0;i<data.Length;i++){
    float t=(float)i/Rate,u=t/duration;seed^=seed<<13;seed^=seed>>17;seed^=seed<<5;
    float n=(seed&65535)/32767.5f-1;low=Mathf.Lerp(low,n,.12f);
    float value=cue switch {
     0=>low*.7f*Mathf.Pow(Mathf.Sin(Mathf.PI*u),2), // swing air
     1=>(n*.24f+Tone(2350,t)*.24f+Tone(970,t)*.23f)*Mathf.Exp(-t*52)+low*.2f*Mathf.Exp(-t*25),
     2=>(Tone(1600,t)*.24f+Tone(740,t)*.35f+n*.13f)*Mathf.Exp(-t*66),
     3=>(low*.65f+Tone(165,t)*.2f)*Mathf.Exp(-t*27),
     4=>(low*.85f+n*.08f)*Mathf.Pow(1-u,2),
     5=>Cup(t),
     6=>Celebrate(t),
     7=>(Tone(880,t)*.27f+Tone(1320,t)*.13f)*Mathf.Exp(-t*21),
     _=>Tone(660,t)*.18f*Mathf.Exp(-t*20)
    };
    float edge=Mathf.Min(1,t/.002f)*Mathf.Min(1,(duration-t)/.008f);
    data[i]=Mathf.Clamp(value*edge,-.85f,.85f);
   }
   return Clip("Golf "+((GolfFeedbackCue)cue),data);
  }
  static float Cup(float t){
   float value=0;for(int i=0;i<3;i++){float start=i*.07f,d=t-start;if(d>=0)value+=(Tone(480,d)*.19f+Tone(1130,d)*.11f)*Mathf.Exp(-d*44)*(1-start*3);}
   if(t>.18f)value+=(Tone(1046.5f,t-.18f)*.19f+Tone(1568,t-.18f)*.08f)*Mathf.Exp(-(t-.18f)*13);return value;
  }
  static float Celebrate(float t){float value=0;var notes=Notes;for(int i=0;i<4;i++){float d=t-i*.12f;if(d>=0)value+=(Tone(notes[i],d)*.16f+Tone(notes[i]*2,d)*.035f)*Mathf.Min(1,d/.006f)*Mathf.Exp(-d*7);}return value;}
  public static AudioClip[] Footsteps(){
   var result=new AudioClip[5];
   for(int k=0;k<5;k++){
    float duration=k==2?.22f:k==1?.18f:.13f;var data=new float[Mathf.RoundToInt(duration*Rate)];uint seed=(uint)(47317+k*171);float low=0;
    for(int i=0;i<data.Length;i++){
     float t=(float)i/Rate,u=t/duration;seed^=seed<<13;seed^=seed>>17;seed^=seed<<5;float noise=(seed&65535)/32767.5f-1;low=Mathf.Lerp(low,noise,k==2?.05f:.18f);
     float value=k switch {
      0=>low*.46f*Mathf.Exp(-t*25)+Tone(110,t)*.13f*Mathf.Exp(-t*40),
      1=>low*.6f*Mathf.Pow(1-u,2)+noise*.05f*Mathf.Exp(-t*18),
      2=>low*.8f*Mathf.Sin(Mathf.PI*u)+Tone(260-500*t,t)*.16f*Mathf.Exp(-t*22),
      3=>(Tone(180,t)*.25f+Tone(420,t)*.12f+noise*.18f)*Mathf.Exp(-t*35),
      _=>(Tone(900,t)*.18f+noise*.34f)*Mathf.Exp(-t*55)
     };
     data[i]=Mathf.Clamp(value*Mathf.Min(1,t/.003f)*Mathf.Min(1,(duration-t)/.008f),-.8f,.8f);
    }
    result[k]=Clip("Golf footstep "+(GolfFootSurface)k,data);
   }
   return result;
  }
  public static AudioClip StartSignal(bool go){
   float duration=go?.62f:.16f;var data=new float[Mathf.RoundToInt(duration*Rate)];float phase=0;
   for(int i=0;i<data.Length;i++){
    float t=(float)i/Rate;
    float hz=go?2400:880;phase+=2*Mathf.PI*hz/Rate;
    float envelope=Mathf.Min(1,t/.008f)*Mathf.Min(1,(duration-t)/.06f);
    data[i]=go?envelope*Mathf.Sin(phase)*.37f:envelope*(Mathf.Sin(phase)*.3f+Mathf.Sin(phase*2)*.08f);
   }
   return Clip(go?"Golf GO whistle":"Golf countdown beep",data);
  }
  public void Dispose(){foreach(var clip in cues)if(clip)UnityEngine.Object.Destroy(clip);if(charge)UnityEngine.Object.Destroy(charge);}
 }
}
