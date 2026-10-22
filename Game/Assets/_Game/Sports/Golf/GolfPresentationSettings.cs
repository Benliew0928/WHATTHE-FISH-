using UnityEngine;
namespace WhatTheFish {
 [CreateAssetMenu(menuName="WHATTHE FISH/Golf presentation")]
 public sealed class GolfPresentationSettings:ScriptableObject {
  static GolfPresentationSettings loaded;
  public static GolfPresentationSettings Current {get{if(!loaded){loaded=Resources.Load<GolfPresentationSettings>("GolfPresentation");if(!loaded)loaded=CreateInstance<GolfPresentationSettings>();}return loaded;}}
  [Header("Current hole guide, in safe-area HUD units")]
  public Vector2 holeGuideSize=new(64,82);
  [Min(0)] public float holeGuideWorldLift=1.8f;
  [Min(0)] public float guideFadeSeconds=.18f;
  [Header("Wind, world-space streaks")]
  [Range(4,28)] public int windStreakCount=28;
  [Range(.1f,1)] public float windStreakOpacity=.88f;
  [Range(.01f,.4f)] public float windEdgeOpacity=.3f;
  [Range(.1f,2)] public float windEdgeSeconds=1.2f;
  [Range(0,.2f)] public float windSustainOpacity=.12f;
  [Range(40,100)] public float windCompassSize=72;
  [Range(0,.15f)] public float weatherEdgeOpacity=.085f;
  [Header("Contact effects")]
  [Range(.2f,2)] public float impactSeconds=1.4f;
  [Range(.15f,2)] public float impactRadius=1.15f;
  [Range(.02f,.3f)] public float bounceSquash=.24f;
  public Color stickyColor=new(.25f,1,.66f,1),bounceColor=new(1,.48f,.12f,1),windColor=new(.72f,.88f,1,1);
 }
}
