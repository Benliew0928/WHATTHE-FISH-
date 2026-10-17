using UnityEngine;

namespace WhatTheFish {
 [CreateAssetMenu(menuName="WHATTHE FISH?/Golf HUD layout")]
 public sealed class GolfHUDLayout:ScriptableObject {
  [Header("Safe-area frames")]
  [Min(0)]public float margin=20,gap=16;
  [Header("Aim focus")]
  [Min(0)]public float aimFocusFadeSeconds=.25f;
  [Min(.5f)]public float compactAspect=1.45f;
  public Vector2 lookAnchorMin=new(.475f,.1333333f),lookAnchorMax=new(.9875f,.9333333f);
  public Vector2 courseSize=new(500,144),actionSize=new(266,250),aimSize=new(280,134),cameraSize=new(142,99),jumpSize=new(148,117),returnSize=new(300,90),cartSize=new(272,104),joystickSize=new(258,258),hintSize=new(430,92),startSize=new(320,86);
  [Header("Course badge and live captions")]
  public Vector2 courseArtSize=new(108,108),courseArtPosition=new(-155,0),courseTitlePosition=new(59,23),courseStationPosition=new(59,-22),shotLabelPosition=new(0,-62);
  public Vector2 shotLabelSize=new(188,78);
  [Range(16,48)]public int courseTitleFontSize=33,courseStationFontSize=20,shotFontSize=30,hintFontSize=25,controlFontSize=37;
  [Min(0)]public float utilityCornerRadius=36;
  [Header("Scorecard")]
  [Min(100)]public float boardWidth=570;
  [Min(16)]public float boardHeaderHeight=66,boardColumnHeight=40,boardRowHeight=54,boardCompactRowHeight=32,boardFooterHeight=58,boardPadding=20;
  public Vector2 boardPosition=new(-20,-20);
  [Range(12,40)]public int tableFontSize=20,tableNameFontSize=21,tableHeaderFontSize=17;
  [Range(.5f,4)]public float rowSeparatorWidth=1.6f;
  [Header("Collapsible leaderboard")]
  [Range(.2f,.45f)]public float boardCompactHeightFraction=.44f;
  [Range(.5f,.9f)]public float boardExpandedHeightFraction=.78f;
  [Min(100)]public float boardExpandedWidth=720,boardCompactTitleWidth=320;
  [Min(24)]public float boardPreviewRowHeight=64,boardFullRowHeight=82,boardToggleHeight=36;
  [Min(40)]public float boardAimSummaryHeight=58;
  [Range(.75f,1)]public float boardExpandedTitleFraction=.9f;
  [Range(.65f,.9f)]public float boardHeaderJoin=.8f;
  [Range(1,1.05f)]public float boardHoverScale=1.018f;
  [Header("Final countdown")]
  public Vector2 countdownSize=new(330,94),countdownPosition=new(0,-230);
  [Range(20,60)]public int countdownFontSize=36;
  [Header("Existing target and ball indicators")]
  [Range(.5f,1)]public float aimTargetScale=.72f;
  [Min(0)]public float aimTargetTopMargin=12;
  public Vector2 aimEdgeArrowSize=new(42,44);
  public Vector2 targetPosition=new(0,-152),targetSize=new(330,118),targetArrowPosition=new(0,-93),targetArrowSize=new(74,77);
  [Min(0)]public float targetTextLeftPadding=100,targetTextRightPadding=18,targetCaptionGap=12;
  [Range(12,40)]public int targetFontSize=26,ballFontSize=24;
 }
}
