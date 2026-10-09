using UnityEngine;

namespace WhatTheFish {
 [CreateAssetMenu(menuName="WHATTHE FISH?/Fishing HUD layout")]
 public sealed class FishingHUDLayout:ScriptableObject {
  [Min(0)]public float margin=20,gap=16;
  [Min(.5f)]public float compactAspect=1.45f;
  [Range(1,2)]public float portraitScale=1.6f;
  public Vector2 actionSize=new(235,235),cancelSize=new(150,76),clockSize=new(390,166),gaugeSize=new(574,144),cameraSize=new(104,66),jumpSize=new(120,72),returnSize=new(250,62),joystickSize=new(176,176);
  public Vector2 clockBadgePosition=new(-139,33),clockTextPosition=new(34,29),wantedCardPosition=new(0,-44);
  public Vector2 aimAnchor=new(.5f,.5f),aimSize=new(54,54);
  [Range(2,15)]public float aimAcquireDegrees=8,aimReleaseDegrees=10;
  [Header("Readability and utility controls")]
  [Range(16,48)]public int wantedFontSize=28,wantedBonusFontSize=34,leaderboardNameFontSize=25,scorePopFontSize=29;
  [Min(0)]public float utilityCornerRadius=36;
  [Range(.8f,1.6f)]public float cameraIconWidthScale=1.1f;
  [Range(.5f,4)]public float labelOutlineWidth=2;
  [Header("Hook bite cue")]
  [Range(.1f,.6f)]public float hookCuePopDuration=.28f;
  [Min(16)]public float hookCueMarkSize=58,hookCueRippleSize=86;
  [Range(0,.15f)]public float hookCuePulse=.1f;
  [Min(0)]public float hookCuePadding=8;
  public bool hookCueReducedMotion;
 }
}
