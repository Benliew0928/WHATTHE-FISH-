using System;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 // A short, bounded correction of a committed trajectory, never a homing ball.
 public struct PassFlightSnapshot:INetworkSerializable,IEquatable<PassFlightSnapshot> {
  public bool active;public Vector3 origin,destination,velocity;public float remaining;public double sampled;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref active);if(!active){origin=destination=velocity=default;remaining=0;sampled=0;return;}s.SerializeValue(ref origin);s.SerializeValue(ref destination);s.SerializeValue(ref velocity);s.SerializeValue(ref remaining);s.SerializeValue(ref sampled);}
  public bool Equals(PassFlightSnapshot b)=>active==b.active&&origin==b.origin&&destination==b.destination&&velocity==b.velocity&&remaining==b.remaining&&sampled==b.sampled;
 }
 public sealed class PredictivePass {
  public bool Active {get;private set;}
  public bool Following {get;private set;}
  public Vector3 OriginalDestination {get;private set;}
  public Vector3 Destination {get;private set;}
  public Athlete Receiver {get;private set;}
  public Athlete Passer {get;private set;}
  Vector3 initialRun,launchDirection;double arrives;float height;
  public float Remaining=>Mathf.Max(0,(float)(arrives-BasketballMotion.Clock));
  public void Clear(){Active=Following=false;Receiver=Passer=null;}
  public void Begin(Athlete passer,Athlete receiver,Vector3 destination,Vector3 velocity,float duration){
   Passer=passer;Receiver=receiver;OriginalDestination=Destination=destination;initialRun=TeammatePassing.Velocity(receiver);
   launchDirection=Vector3.ProjectOnPlane(velocity,Vector3.up).normalized;height=receiver?destination.y-receiver.transform.position.y:0;
   arrives=BasketballMotion.Clock+duration;Active=true;Following=receiver;
  }
  public PassFlightSnapshot Snapshot(Rigidbody body)=>!Active?default:new(){active=true,origin=body.position,destination=Destination,velocity=body.isKinematic?Vector3.zero:body.linearVelocity,remaining=Remaining,sampled=BasketballMotion.Clock};
  public void Step(Rigidbody body,bool airborne){
   if(!Active)return;float remaining=Remaining,dt=Time.fixedDeltaTime;
   if(remaining<dt*.5f){Clear();return;}
   if(!Receiver){Following=false;return;}
   if(!Receiver.isActiveAndEnabled||Receiver.inTransit||Receiver.IsSpectator){Following=false;return;}
   var run=TeammatePassing.Velocity(Receiver);
   // A rotation without a movement change is harmless. A reversal, sharp turn,
   // stop or teleport permanently abandons tracking for this pass.
   if(Following){
    var predicted=Receiver.transform.position+run*remaining+Vector3.up*height;
    bool changed=initialRun.sqrMagnitude>.25f&&(run.sqrMagnitude<initialRun.sqrMagnitude*.16f||Vector3.Dot(initialRun.normalized,run.normalized)<.82f);
    if(changed||Vector3.ProjectOnPlane(predicted-OriginalDestination,Vector3.up).sqrMagnitude>4){Following=false;Destination=OriginalDestination;}
    else Destination=Vector3.MoveTowards(Destination,predicted,dt*4);
   }
   var current=body.linearVelocity;var wanted=Vector3.ProjectOnPlane(Destination-body.position,Vector3.up)/Mathf.Max(dt,remaining);
   var flat=Vector3.ProjectOnPlane(current,Vector3.up);
   // Never turn back or make a last-moment hook to recover a missed receiver.
   if(wanted.sqrMagnitude<.01f||Vector3.Dot(wanted.normalized,launchDirection)<.91f||Vector3.Dot(wanted,flat)<=0){Following=false;Destination=OriginalDestination;return;}
   var corrected=Vector3.MoveTowards(flat,wanted,8*dt);
   body.linearVelocity=corrected+Vector3.up*current.y;
   if(!airborne)body.angularVelocity=Vector3.Cross(Vector3.up,corrected)*8;
  }
  // Find the first reachable player along this physics step. All teams use the
  // same receiving envelope; the intended receiver has no interception priority.
  public Athlete FirstReceiver(Rigidbody body,float radius,float maxHeight,Func<Athlete,bool> eligible,out float fraction){
   fraction=0;if(!Active)return null;var start=body.position;var delta=body.linearVelocity*Time.fixedDeltaTime;
   var flat=Vector3.ProjectOnPlane(delta,Vector3.up);float length=flat.sqrMagnitude,best=float.PositiveInfinity;Athlete found=null;
   foreach(var actor in Athlete.Active){
    if(!actor||actor==Passer||!eligible(actor))continue;
    var offset=Vector3.ProjectOnPlane(start-actor.transform.position,Vector3.up);float c=offset.sqrMagnitude-radius*radius,t=0;
    if(c>0){if(length<.000001f)continue;float b=Vector3.Dot(offset,flat),disc=b*b-length*c;if(b>=0||disc<0)continue;t=(-b-Mathf.Sqrt(disc))/length;if(t<0||t>1)continue;}
    var contact=start+delta*t;float y=contact.y-actor.transform.position.y;if(y<-.2f||y>maxHeight)continue;
    if(t>best+.0001f||Mathf.Abs(t-best)<.0001f&&found&&TeammatePassing.Id(actor)>=TeammatePassing.Id(found))continue;
    if(Physics.Linecast(start,contact,1<<8,QueryTriggerInteraction.Ignore)||Physics.Linecast(actor.transform.position+Vector3.up*(maxHeight*.5f),contact,1<<8,QueryTriggerInteraction.Ignore))continue;
    best=t;found=actor;
   }
   if(found)fraction=best;return found;
  }
 }
}
