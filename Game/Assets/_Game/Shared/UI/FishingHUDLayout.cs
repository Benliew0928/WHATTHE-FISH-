using UnityEngine;

namespace WhatTheFish {
 [CreateAssetMenu(menuName="WHATTHE FISH?/Fishing HUD layout")]
 public sealed class FishingHUDLayout:ScriptableObject {
  [Min(0)]public float margin=20,gap=16;
  [Min(.5f)]public float compactAspect=1.45f;
  [Range(1,2)]public float portraitScale=1.6f;
  public Vector2 actionSize=new(235,235),cancelSize=new(150,76),clockSize=new(390,174),gaugeSize=new(574,144),cameraSize=new(148,66),jumpSize=new(172,72),returnSize=new(250,62),joystickSize=new(176,176);
  public Vector2 aimAnchor=new(.5f,.5f),aimSize=new(54,54);
  [Range(2,15)]public float aimAcquireDegrees=8,aimReleaseDegrees=10;
 }
}
