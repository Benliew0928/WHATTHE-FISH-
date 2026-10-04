using UnityEngine;

namespace WhatTheFish {
 [CreateAssetMenu(menuName="WHATTHE FISH/Golf Ball Physics")]
 public sealed class GolfBallPhysicsSettings:ScriptableObject {
  [Header("Swing launch")]
  [Min(0)] public float minimumSwingSpeed=1.2f;
  [Tooltip("Full-charge horizontal launch speed, in metres/second.")]
  [Min(0)] public float maximumSwingSpeed=18f;
  [Min(0)] public float maximumSwingLift=1.8f;
  public Vector3 SwingVelocity(Vector3 direction,float charge){
   charge=Mathf.Clamp01(charge);
   return direction*Mathf.Lerp(minimumSwingSpeed,maximumSwingSpeed,charge)+Vector3.up*(charge>.25f?Mathf.Lerp(0,maximumSwingLift,(charge-.25f)/.75f):0);
  }
  [Header("Slow rolling and rest")]
  [Min(.01f)] public float slowRollingThreshold=2f;
  [Min(.001f)] public float stopSpeedThreshold=.25f;
  [Min(.02f)] public float stopDelay=.5f;
  [Min(.01f)] public float settlingMaxSpeed=1f;
  [Tooltip("Additional tangential deceleration in m/s², ramped in as the ball slows.")]
  [Min(.01f)] public float rollingResistanceStrength=1.8f;
  [Header("Existing normal roll")]
  [Min(0)] public float levelRollingResistance=1.35f;
  [Range(0,2)] public float levelGroundDegrees=.5f;
  [Header("New gameplay forces")]
  [Tooltip("Unexpected velocity change (m/s) that restarts settling. Ground gravity is excluded.")]
  [Min(.01f)] public float externalVelocityChange=.2f;
  [Min(0)] public float newForceGracePeriod=.12f;
  void OnValidate(){
   minimumSwingSpeed=Mathf.Max(0,minimumSwingSpeed);maximumSwingSpeed=Mathf.Max(minimumSwingSpeed,maximumSwingSpeed);maximumSwingLift=Mathf.Max(0,maximumSwingLift);
   slowRollingThreshold=Mathf.Max(.01f,slowRollingThreshold);
   stopSpeedThreshold=Mathf.Clamp(stopSpeedThreshold,.001f,slowRollingThreshold);
   settlingMaxSpeed=Mathf.Clamp(settlingMaxSpeed,stopSpeedThreshold,slowRollingThreshold);
   stopDelay=Mathf.Max(.02f,stopDelay);rollingResistanceStrength=Mathf.Max(.01f,rollingResistanceStrength);
  }
 }
}
