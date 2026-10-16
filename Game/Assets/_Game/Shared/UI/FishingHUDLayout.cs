using UnityEngine;

namespace WhatTheFish {
 [CreateAssetMenu(menuName="WHATTHE FISH?/Fishing HUD layout")]
 public sealed class FishingHUDLayout:ScriptableObject {
  [Min(0)]public float margin=20,gap=16;
  [Min(.5f)]public float compactAspect=1.45f;
  [Range(1,2)]public float portraitScale=1.6f;
  public Vector2 actionSize=new(235,235),cancelSize=new(150,76),clockSize=new(390,154),gaugeSize=new(574,186),cameraSize=new(104,66),jumpSize=new(120,72),returnSize=new(250,62),joystickSize=new(176,176);
  public Vector2 fightControlSize=new(218,218);
  public Vector2 fightLabelPosition=new(0,-40),fightLabelSize=new(126,32);
  [Header("Illustrated fight card") ]
  public Vector2 guideSize=new(510,48),guidePosition=new(0,22),progressSize=new(242,28);
  public Vector2 catchPosition=new(-134,-56),tensionPosition=new(134,-56);
  public Vector2 fightTitlePosition=new(0,64);
  public Vector2 clockBadgePosition=new(-139,31),clockTextPosition=new(34,27),wantedCardPosition=new(0,-39);
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
  [Header("Action guidance and fight camera")]
  public Vector2 actionHintSize=new Vector2(64,64);
  public bool fightCamera=true;
  [Range(4,12)] public float fightCameraDistance=7.2f;
  [Range(2,8)] public float fightCameraHeight=4.2f;
  [Range(0,3)] public float fightCameraShoulder=1.1f;
  [Range(.3f,2)] public float fightCameraTransition=.7f;
 }
}
