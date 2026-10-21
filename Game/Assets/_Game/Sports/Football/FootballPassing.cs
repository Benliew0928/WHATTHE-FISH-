using UnityEngine;
namespace WhatTheFish {
 public sealed partial class FootballBall {
  public const float PassLaneHalfWidth=1.8f,PassRange=30;
  public uint PassPossession=>HasAuthority?sequence:NetworkAthlete.HostPlayer?NetworkAthlete.HostPlayer.Ball.Value.sequence:0;
  public bool CanPass(Athlete actor)=>actor&&Allowed&&HasPassPossession(actor)&&actor.KickReady&&!PropLocked(actor);
  bool HasPassPossession(Athlete actor)=>CurrentController==actor;
  public bool ValidPassReceiver(Athlete passer,Athlete receiver){
   var match=FootballMatch.Instance;
   if(!passer||!receiver||receiver==passer||!receiver.isActiveAndEnabled||receiver.IsSpectator||receiver.inTransit||!match||match.TeamOf(passer)==FootballTeam.None||match.TeamOf(receiver)!=match.TeamOf(passer))return false;
   var delta=Vector3.ProjectOnPlane(receiver.transform.position-passer.transform.position,Vector3.up);
   return delta.sqrMagnitude>=.25f&&delta.sqrMagnitude<=PassRange*PassRange&&Mathf.Abs(receiver.transform.position.y-passer.transform.position.y)<.8f&&!Physics.Linecast(passer.transform.position+Vector3.up*.4f,receiver.transform.position+Vector3.up*.4f,1<<8,QueryTriggerInteraction.Ignore);
  }
  public bool TryPassTo(Athlete actor,Athlete receiver,uint possession){
   if(!HasAuthority||possession!=PassPossession||!CanPass(actor)||!ValidPassReceiver(actor,receiver))return false;
   return ReleasePass(actor,receiver,0);
  }
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
   return ReleasePass(actor,SelectPassReceiver(actor,heading),heading);
  }
  bool ReleasePass(Athlete actor,Athlete target,float heading){
   var contact=Body.position;
   var direction=Quaternion.Euler(0,heading,0)*Vector3.forward;
   float distance=12;if(target){
    var centre=target.transform.position;var destination=centre;
    for(int i=0;i<5;i++){
     distance=Vector3.ProjectOnPlane(destination-actor.transform.position,Vector3.up).magnitude;
     float launch=PassSpeed(distance),deceleration=Mathf.Max(.01f,rollingDeceleration);
     float travel=(launch-Mathf.Sqrt(Mathf.Max(0,launch*launch-2*deceleration*distance)))/deceleration;
     destination=centre+TeammatePassing.Lead(target,travel);
    }
    var local=Pitch.InverseTransformPoint(destination);local.x=Mathf.Clamp(local.x,PitchBounds.min.x+.5f,PitchBounds.max.x-.5f);local.z=Mathf.Clamp(local.z,PitchBounds.min.z+.5f,PitchBounds.max.z-.5f);destination=Pitch.TransformPoint(local);
    if(Physics.Linecast(centre+Vector3.up*.4f,destination+Vector3.up*.4f,1<<8,QueryTriggerInteraction.Ignore))destination=centre;
    var delta=Vector3.ProjectOnPlane(destination-actor.transform.position,Vector3.up);distance=delta.magnitude;direction=delta.normalized;
   }
   // Turn into the selected pass before releasing from the foot, including a
   // backwards camera aim. Otherwise the pass can collide with its own passer.
   float passHeading=Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg;
   actor.Motor.FaceGolf(passHeading);actor.transform.rotation=Quaternion.Euler(0,passHeading,0);FollowController(actor);if(controller!=actor)return false;
   // A grounded physical pass; opponents can block it. No homing or teleporting.
   float speed=PassSpeed(distance);
   ReleaseControl(actor);impulsePending=true;Body.WakeUp();Body.linearVelocity=direction*speed;Body.angularVelocity=Vector3.Cross(Vector3.up,direction)*speed/WorldRadius;
   FootballMatch.Instance?.RecordPass(actor);FootballAudio.Emit(FootballSound.KickLight,Body.position,.8f);actor.FootballMotion?.Kick(.25f,contact);return true;
  }
  float PassSpeed(float distance)=>Mathf.Clamp(Mathf.Sqrt(2*rollingDeceleration*distance)+4.5f,9,17);
 }
}
