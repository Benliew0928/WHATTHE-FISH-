using Unity.Netcode.Components;
using UnityEngine;

namespace WhatTheFish {
 // Keep the one-tick interpolation buffer, but remove the second 100 ms low-pass
 // filter. Transient poses may be dropped; NGO still reliably synchronizes teleports
 // and periodically repairs lost axes. Authority and action RPCs stay on the host.
 public static class NetworkResponsiveness {
  public const float Interval=1f/30;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public static readonly bool Baseline=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-latencyBaseline")>=0;
#else
  public const bool Baseline=false;
#endif
  public static void Configure(NetworkTransform pose){
   if(!pose)return;
   pose.Interpolate=true;pose.UseUnreliableDeltas=!Baseline;
   pose.PositionInterpolationType=pose.RotationInterpolationType=NetworkTransform.InterpolationTypes.LegacyLerp;
   pose.PositionLerpSmoothing=pose.RotationLerpSmoothing=Baseline;
   pose.PositionThreshold=Baseline?.001f:.01f;pose.RotAngleThreshold=Baseline?.01f:.25f;
  }
  public static bool Newer(uint next,uint previous)=>unchecked((int)(next-previous))>0;
  public static bool InputChanged(PlayerCommand a,PlayerCommand b)=>
   (a.move-b.move).sqrMagnitude>.04f||Mathf.Abs(Mathf.DeltaAngle(a.heading,b.heading))>8||
   a.sprint!=b.sprint||a.dash!=b.dash||a.pressure!=b.pressure||a.charging!=b.charging||
   a.guard!=b.guard||a.guardBlock!=b.guardBlock||a.defensePlay!=b.defensePlay||
   (a.move.sqrMagnitude<.0225f)!=(b.move.sqrMagnitude<.0225f);
 }
}
