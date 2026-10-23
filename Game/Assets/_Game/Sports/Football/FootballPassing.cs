using UnityEngine;
namespace WhatTheFish {
 public sealed partial class FootballBall {
  public const float PassLaneHalfWidth=1.8f,PassRange=30;
  readonly PredictivePass assistedPass=new();
  public PassFlightSnapshot PassFlight=>HasAuthority?assistedPass.Snapshot(Body):NetworkAthlete.HostPlayer?NetworkAthlete.HostPlayer.Ball.Value.passFlight:default;
  public bool PassFollowing=>assistedPass.Following;
  public Vector3 OriginalPassDestination=>assistedPass.OriginalDestination;
  void StepAssistedPass(){
   if(!assistedPass.Active)return;
   assistedPass.Step(Body,false);
   var receiver=assistedPass.FirstReceiver(Body,controlDistance-.15f,.75f,a=>a.isActiveAndEnabled&&!a.IsSpectator&&!a.inTransit&&a.Grounded&&a.Action==FootballAction.None&&a.capsule&&a.capsule.enabled&&!mustApproach.Contains(a),out float fraction);
   if(receiver){Body.position+=Body.linearVelocity*(Time.fixedDeltaTime*fraction);Body.linearVelocity=Vector3.zero;Body.angularVelocity=Vector3.zero;RefreshControl(receiver);}
  }
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
   return TeammatePassing.ClosestAngle(passer,heading,candidate=>ValidPassReceiver(passer,candidate));
  }
  public bool TryPass(Athlete actor,float heading){
   if(!HasAuthority||!CanPass(actor)||!float.IsFinite(heading))return false;
   return ReleasePass(actor,SelectPassReceiver(actor,heading),heading);
  }
  bool ReleasePass(Athlete actor,Athlete target,float heading){
   var contact=Body.position;
   var direction=Quaternion.Euler(0,heading,0)*Vector3.forward;
   float distance=12,travel=1;Vector3 destination=Body.position+direction*distance;if(target){
    var centre=target.transform.position;destination=centre;
    for(int i=0;i<12;i++){
     distance=Vector3.ProjectOnPlane(destination-Body.position,Vector3.up).magnitude;
     travel=distance/PassSpeed(distance);
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
   // A grounded physical pass. Only small corrections are allowed after release.
   var toDestination=Vector3.ProjectOnPlane(destination-Body.position,Vector3.up);distance=toDestination.magnitude;direction=toDestination.normalized;
   float speed=PassSpeed(distance);travel=distance/speed;destination.y=Body.position.y;
   ReleaseControl(actor);impulsePending=true;Body.WakeUp();Body.linearVelocity=direction*speed;Body.angularVelocity=Vector3.Cross(Vector3.up,direction)*speed/WorldRadius;
   assistedPass.Begin(actor,target,destination,Body.linearVelocity,travel);
   FootballMatch.Instance?.RecordPass(actor);FootballAudio.Emit(FootballSound.KickLight,Body.position,.8f);actor.FootballMotion?.Kick(.25f,contact);return true;
  }
  float PassSpeed(float distance)=>Mathf.Clamp(12+distance*.25f,12,23);
 }
}
