using System;
using UnityEngine;

namespace WhatTheFish {
 // Imported approved clips; ownership remains with Unity's shared Resources bank.
 public sealed class GolfFeedbackSounds:IDisposable {
  public const int Rate=32000;
  static readonly string[] Keys={"golf-swish","golf-swing","golf-putt","golf-turf","golf-sand","golf-cup","golf-finish","golf-hole","golf-stone","golf-wood","golf-ball-hit","water-entry","recovery","cart-bump"};
  public readonly AudioClip[] cues=Array.ConvertAll(Keys,k=>GolfFishingAudio.Choose(k,0));
  public readonly AudioClip charge=GolfFishingAudio.Choose("golf-charge",0);
  public AudioClip Choose(GolfFeedbackCue cue)=>GolfFishingAudio.Choose(Keys[(int)cue]);
  public static AudioClip[] Footsteps()=>new[]{GolfFishingAudio.Choose("grass-walk",0),GolfFishingAudio.Choose("sand-steps",0),GolfFishingAudio.Choose("water-steps",0),GolfFishingAudio.Choose("wood-steps",0),GolfFishingAudio.Choose("stone-steps",0)};
  public static AudioClip Footstep(GolfFootSurface surface,bool running)=>GolfFishingAudio.Choose(surface switch {GolfFootSurface.Grass=>running?"grass-run":"grass-walk",GolfFootSurface.Sand=>"sand-steps",GolfFootSurface.Water=>"water-steps",GolfFootSurface.Wood=>"wood-steps",_=>"stone-steps"});
  public static AudioClip StartSignal(bool go)=>GolfFishingAudio.Choose(go?"golf-go":"golf-countdown",0);
  public void Dispose(){} // Never destroy imported AudioClip assets.
 }
}
