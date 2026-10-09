using UnityEngine;

namespace WhatTheFish {
 [CreateAssetMenu(menuName="WHATTHE FISH/Golf Ball Physics")]
 public sealed class GolfBallPhysicsSettings:ScriptableObject {
  [Header("Swing launch")]
  [Min(0)] public float minimumSwingSpeed=3f;
  [Tooltip("Full-charge horizontal launch speed, in metres/second.")]
  [Min(0)] public float maximumSwingSpeed=17f;
  [Min(0)] public float minimumSwingLift=2.8f;
  [Min(0)] public float maximumSwingLift=8.5f;
  [Tooltip("Tangential speed retained when a lofted swing first meets the turf.")]
  [Range(0,1)] public float landingSpeedRetention=.78f;
  [Min(0)] public float minimumPuttSpeed=.35f;
  [Min(0)] public float maximumPuttSpeed=7.5f;
  public Vector3 SwingVelocity(Vector3 direction,float charge,GolfShotMode mode=GolfShotMode.Swing){
   charge=Mathf.Clamp01(charge);
   direction=Vector3.ProjectOnPlane(direction,Vector3.up).normalized;
   if(mode==GolfShotMode.Putt)return direction*Mathf.Lerp(minimumPuttSpeed,maximumPuttSpeed,charge);
   return direction*Mathf.Lerp(minimumSwingSpeed,maximumSwingSpeed,charge)+Vector3.up*Mathf.Lerp(minimumSwingLift,maximumSwingLift,charge);
  }
  [Header("Slow rolling and rest")]
  [Min(.01f)] public float slowRollingThreshold=2f;
  [Min(.001f)] public float stopSpeedThreshold=.25f;
  [Min(.02f)] public float stopDelay=.5f;
  [Min(.01f)] public float settlingMaxSpeed=1f;
  [Tooltip("Additional tangential deceleration in m/s², ramped in as the ball slows.")]
  [Min(.01f)] public float rollingResistanceStrength=1.8f;
  [Header("Ground resistance in metres per second squared")]
  [Min(.1f)] public float greenResistance=1.15f;
  [Min(.1f)] public float fairwayResistance=3.3f;
  [Min(.1f)] public float roughResistance=4.5f;
  [Min(.1f)] public float sandResistance=8f;
  [Header("New gameplay forces")]
  [Tooltip("Unexpected velocity change (m/s) that restarts settling. Ground gravity is excluded.")]
  [Min(.01f)] public float externalVelocityChange=.2f;
  [Min(0)] public float newForceGracePeriod=.12f;
  void OnValidate(){
   minimumSwingSpeed=Mathf.Max(0,minimumSwingSpeed);maximumSwingSpeed=Mathf.Max(minimumSwingSpeed,maximumSwingSpeed);maximumSwingLift=Mathf.Max(0,maximumSwingLift);
   minimumSwingLift=Mathf.Clamp(minimumSwingLift,0,maximumSwingLift);minimumPuttSpeed=Mathf.Max(0,minimumPuttSpeed);maximumPuttSpeed=Mathf.Clamp(maximumPuttSpeed,minimumPuttSpeed,maximumSwingSpeed);
   slowRollingThreshold=Mathf.Max(.01f,slowRollingThreshold);
   stopSpeedThreshold=Mathf.Clamp(stopSpeedThreshold,.001f,slowRollingThreshold);
   settlingMaxSpeed=Mathf.Clamp(settlingMaxSpeed,stopSpeedThreshold,slowRollingThreshold);
   stopDelay=Mathf.Max(.02f,stopDelay);rollingResistanceStrength=Mathf.Max(.01f,rollingResistanceStrength);
  }
 }
}
