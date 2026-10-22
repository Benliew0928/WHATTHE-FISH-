using UnityEngine;

namespace WhatTheFish {
 [CreateAssetMenu(menuName="WHATTHE FISH/Golf Ball Physics")]
 public sealed class GolfBallPhysicsSettings:ScriptableObject {
  [Header("Swing launch")]
  [Min(0)] public float minimumSwingSpeed=3f;
  [Tooltip("Full-charge horizontal launch speed, in metres/second.")]
  [Min(0)] public float maximumSwingSpeed=34f;
  [Min(0)] public float minimumSwingLift=2.8f;
  [Min(0)] public float maximumSwingLift=17f;
  [Tooltip("Tangential speed retained when a lofted swing first meets the turf.")]
  [Range(0,1)] public float landingSpeedRetention=.72f;
  [Header("Turf landing")]
  [Tooltip("Normal speed retained by each turf bounce; energy falls by its square.")]
  [Range(0,1)] public float turfRestitution=.46f;
  [Tooltip("Impacts below this normal speed settle into rolling instead of tiny jittering hops.")]
  [Min(0)] public float minimumBounceSpeed=.55f;
  [Range(0,1)] public float subsequentLandingRetention=.92f;
  public Vector3 LandingVelocity(Vector3 velocity,Vector3 normal,bool firstLanding){
   float incoming=Mathf.Max(0,-Vector3.Dot(velocity,normal));
   var tangent=Vector3.ProjectOnPlane(velocity,normal)*(firstLanding?landingSpeedRetention:subsequentLandingRetention);
   return tangent+normal*(incoming>minimumBounceSpeed?incoming*turfRestitution:0);
  }
  [Min(0)] public float minimumPuttSpeed=.35f;
  [Min(0)] public float maximumPuttSpeed=15f;
  public Vector3 SwingVelocity(Vector3 direction,float charge,GolfShotMode mode=GolfShotMode.Swing){
   charge=Mathf.Clamp01(charge);
   direction=Vector3.ProjectOnPlane(direction,Vector3.up).normalized;
   if(mode==GolfShotMode.Putt){
    // Remap charge through sqrt so rolling distance grows roughly linearly with
    // the displayed charge value. Because distance proportional to v^2, making
    // v proportional to sqrt(charge) gives distance proportional to charge.
    // Min/max putt speeds are preserved exactly. Ball physics is unchanged.
    float linearCharge=Mathf.Sqrt(charge);
    return direction*Mathf.Lerp(minimumPuttSpeed,maximumPuttSpeed,linearCharge);
   }
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
  [Header("Cup crossing assistance")]
  [Tooltip("Ground-level crossings up to this speed drop in; harder shots can roll across.")]
  [Min(.1f)] public float cupCaptureSpeed=6f;
  [Tooltip("Inside-rim allowance as a fraction of the ball radius. One accepts the ball centre anywhere inside the opening.")]
  [Range(0,1)] public float cupRimForgiveness=1f;
  [Tooltip("Ball-centre height at which a gentle entry can begin sinking.")]
  [Range(.5f,1.5f)] public float cupEntryHeightRatio=1.3f;
  [Range(.12f,.8f)] public float cupSinkSeconds=.32f;
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
   turfRestitution=Mathf.Clamp01(turfRestitution);minimumBounceSpeed=Mathf.Max(0,minimumBounceSpeed);subsequentLandingRetention=Mathf.Clamp01(subsequentLandingRetention);
   cupCaptureSpeed=Mathf.Max(.1f,cupCaptureSpeed);cupRimForgiveness=Mathf.Clamp01(cupRimForgiveness);cupEntryHeightRatio=Mathf.Clamp(cupEntryHeightRatio,.5f,1.5f);cupSinkSeconds=Mathf.Clamp(cupSinkSeconds,.12f,.8f);
  }
 }
}
