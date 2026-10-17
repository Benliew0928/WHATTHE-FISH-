using UnityEngine;
namespace WhatTheFish {
 public sealed partial class FootballBall {
  public const float PassLaneHalfWidth=1.8f,PassRange=30;
  public bool CanPass(Athlete actor)=>actor&&Allowed&&HasPassPossession(actor)&&actor.KickReady&&!PropLocked(actor);
  bool HasPassPossession(Athlete actor)=>CurrentController==actor;
  public Athlete SelectPassReceiver(Athlete passer,float heading){
   if(!passer||!float.IsFinite(heading))return null;var match=FootballMatch.Instance;if(!match||match.TeamOf(passer)==FootballTeam.None)return null;
   var forward=Quaternion.Euler(0,heading,0)*Vector3.forward;Athlete best=null;float nearest=float.PositiveInfinity;
   foreach(var candidate in Athlete.Active){
    if(!candidate||candidate==passer||!candidate.isActiveAndEnabled||candidate.inTransit||match.TeamOf(candidate)!=match.TeamOf(passer))continue;
    var delta=Vector3.ProjectOnPlane(candidate.transform.position-passer.transform.position,Vector3.up);float along=Vector3.Dot(delta,forward),distance=delta.sqrMagnitude;
    if(along<.75f||distance>PassRange*PassRange||(delta-forward*along).sqrMagnitude>PassLaneHalfWidth*PassLaneHalfWidth||Mathf.Abs(candidate.transform.position.y-passer.transform.position.y)>.8f||Physics.Linecast(passer.transform.position+Vector3.up*.4f,candidate.transform.position+Vector3.up*.4f,1<<8,QueryTriggerInteraction.Ignore))continue;
    if(distance<nearest-.001f||Mathf.Abs(distance-nearest)<.001f&&best&&FootballBots.Id(candidate)<FootballBots.Id(best)){best=candidate;nearest=distance;}
   }return best;
  }
  public bool TryPass(Athlete actor,float heading){
   if(!HasAuthority||!CanPass(actor)||!float.IsFinite(heading))return false;
   var target=SelectPassReceiver(actor,heading);var contact=Body.position;
   var direction=Quaternion.Euler(0,heading,0)*Vector3.forward;
   float distance=12;if(target){var delta=Vector3.ProjectOnPlane(target.transform.position-actor.transform.position,Vector3.up);distance=delta.magnitude;direction=delta.normalized;}
   // Turn into the selected pass before releasing from the foot, including a
   // backwards camera aim. Otherwise the pass can collide with its own passer.
   float passHeading=Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg;
   actor.Motor.FaceGolf(passHeading);actor.transform.rotation=Quaternion.Euler(0,passHeading,0);FollowController(actor);if(controller!=actor)return false;
   // A grounded physical pass; opponents can block it. No homing or teleporting.
   float speed=Mathf.Clamp(Mathf.Sqrt(2*rollingDeceleration*distance)+4.5f,9,17);
   ReleaseControl(actor);impulsePending=true;Body.WakeUp();Body.linearVelocity=direction*speed;Body.angularVelocity=Vector3.Cross(Vector3.up,direction)*speed/WorldRadius;
   FootballMatch.Instance?.RecordPass(actor);FootballAudio.Emit(FootballSound.KickLight,Body.position,.8f);actor.FootballMotion?.Kick(.25f,contact);return true;
  }
 }
}
