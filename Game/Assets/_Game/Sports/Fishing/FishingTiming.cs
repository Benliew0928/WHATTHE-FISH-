using System;
using UnityEngine;

namespace WhatTheFish {
 public enum FishingTapGrade:sbyte {Miss=-1,None=0,Good=1,Perfect=2}
 public sealed partial class FishingState {
  public const double TimingPeriod=1.65;
  // Retain the current rod-pump presentation; holds no longer use tap grading.
  public static double StrokeSeconds(FishingTapGrade grade)=>grade==FishingTapGrade.Perfect?1.12:1.02;
  public static int TimingBeat(FishingPlayerRecord p,double now)=>Math.Max(0,(int)Math.Floor(Math.Max(0,now-p.phaseAt)/TimingPeriod));
  public static bool DirectionalBeat(FishingPlayerRecord p,double now)=>TimingBeat(p,now)%3==2;
  public static int CounterSide(FishingPlayerRecord p,double now)=>(TimingBeat(p,now)/3+p.fish)%2==0?1:-1;
  public static FishingAction CounterAction(FishingPlayerRecord p,double now)=>CounterSide(p,now)<0?FishingAction.ReelLeft:FishingAction.ReelRight;
  // Preserve the current fish and camera sway cadence. These clocks no longer
  // grade input: reeling uses renewable directional holds.
  public static float Pull(FishingPlayerRecord p,double now)=>(p.fish%2==0?-1:1)*Mathf.Sin((float)(Math.Max(0,now-p.phaseAt)*Math.PI/(TimingPeriod*3)));
 }
}
