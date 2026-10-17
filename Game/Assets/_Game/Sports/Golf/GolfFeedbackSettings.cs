using UnityEngine;

namespace WhatTheFish {
 [CreateAssetMenu(menuName="WHATTHE FISH/Golf Feedback")]
 public sealed class GolfFeedbackSettings:ScriptableObject {
  [Header("Local charging")]
  [Range(0,1)]public float chargeVolume=.18f,puttChargeVolume=.09f,readyVolume=.38f;
  [Range(.8f,1)]public float readyThreshold=.98f;
  [Range(0,.3f)]public float edgeOpacity=.3f;
  [Range(0,.2f)]public float chargeCameraPush=.06f;
  [Header("Contact and landing")]
  [Range(0,1)]public float hitVolume=.7f,landingVolume=.48f,cupVolume=.7f;
  [Min(.01f)]public float minimumLandingSpeed=.5f,landingCooldown=.12f;
  [Header("Swing camera kick (also disabled by Reduced Motion)")]
  public bool cameraShake=true;
  [Range(.02f,.2f)]public float shakeSeconds=.1f;
  [Range(0,.04f)]public float shakeDistance=.012f;
  [Header("Presentation")]
  [Range(0,24)]public int swingSparks=12,puttSparks=3,landingParticles=7;
  [Range(0,64)]public int cupParticles=24;
  [Header("Footsteps")]
  [Range(0,1)]public float footstepVolume=.48f;
  [Min(.2f)]public float walkingStepDistance=1.8f,runningStepDistance=2.6f;
  [Range(0,.49f)]public float footContactPhase=.05f;
  [Min(.001f)]public float footLiftHeight=.028f,footPlantHeight=.02f;
  [Min(.1f)]public float minimumFootstepInterval=.24f;
  [Header("Starting countdown")]
  [Range(0,1)]public float countdownVolume=.6f,whistleVolume=.65f;
  [Header("Cart contact effects")]
  [Min(.5f)]public float wheelTrackLifetime=3f;
  [Range(0,.8f)]public float wheelTrackOpacity=.48f;
  [Range(0,16)]public int wheelParticles=8;
  [Header("Ball camera inset")]
  public Vector2 followWindowSize=new(360,203);
  [Min(0)]public float followRestHold=3f;
  [Min(.1f)]public float followFadeSeconds=.65f;
 }
}
