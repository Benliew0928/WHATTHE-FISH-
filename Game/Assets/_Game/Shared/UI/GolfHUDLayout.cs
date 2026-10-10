using UnityEngine;

namespace WhatTheFish {
 [CreateAssetMenu(menuName="WHATTHE FISH?/Golf HUD layout")]
 public sealed class GolfHUDLayout:ScriptableObject {
  [Header("Safe-area frames")]
  [Min(0)]public float margin=20,gap=16;
  [Min(.5f)]public float compactAspect=1.45f;
  public Vector2 lookAnchorMin=new(.475f,.1333333f),lookAnchorMax=new(.9875f,.9333333f);
  public Vector2 courseSize=new(430,110),actionSize=new(235,235),aimSize=new(215,80),cameraSize=new(112,66),jumpSize=new(112,72),returnSize=new(250,62),cartSize=new(150,70),joystickSize=new(176,176),hintSize=new(370,64),startSize=new(280,64);
  [Header("Course badge and live captions")]
  public Vector2 courseArtSize=new(108,108),courseArtPosition=new(-155,0),courseTitlePosition=new(47,19),courseStationPosition=new(47,-23),shotLabelPosition=new(0,-65);
  public Vector2 shotLabelSize=new(188,78);
  [Range(16,48)]public int courseTitleFontSize=29,courseStationFontSize=19,shotFontSize=29,hintFontSize=24,controlFontSize=24;
  [Min(0)]public float utilityCornerRadius=36;
  [Header("Scorecard")]
  [Min(100)]public float boardWidth=570;
  [Min(16)]public float boardHeaderHeight=56,boardColumnHeight=32,boardRowHeight=46,boardCompactRowHeight=32,boardFooterHeight=46,boardPadding=14;
  public Vector2 boardPosition=new(-20,-20);
  [Range(12,40)]public int tableFontSize=20,tableNameFontSize=21,tableHeaderFontSize=17;
  [Range(.5f,4)]public float rowSeparatorWidth=1.6f;
  [Header("Final countdown")]
  public Vector2 countdownSize=new(330,94),countdownPosition=new(0,-230);
  [Range(20,60)]public int countdownFontSize=36;
  [Header("Existing target and ball indicators")]
  public Vector2 targetPosition=new(0,-116),targetSize=new(300,54),targetArrowPosition=new(0,-68),targetArrowSize=new(28,36);
  [Range(12,40)]public int targetFontSize=22,ballFontSize=18;
 }
}
