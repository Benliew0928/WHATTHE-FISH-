using System.Collections.Generic;
using UnityEngine;
namespace WhatTheFish {
 public sealed partial class FootballBall {
  readonly double[] audioAt=new double[54];bool audioContactsReady;
  public bool AudioActive=>isActiveAndEnabled&&FootballMatch.Instance&&FootballMatch.Instance.Context;
  public void SoundContact(FootballSound cue,Vector3 point,float speed,float interval=.075f){
   if(!HasAuthority||!AudioActive||!float.IsFinite(speed)||speed<.4f)return;
   if(!audioContactsReady){for(int i=0;i<audioAt.Length;i++)audioAt[i]=-100;audioContactsReady=true;}
   int iCue=(int)cue;double now=FootballMotion.Clock;if(now-audioAt[iCue]<interval)return;audioAt[iCue]=now;
   FootballAudio.Emit(cue,point,Mathf.Lerp(.22f,1.15f,Mathf.InverseLerp(.4f,18,speed)));
  }
  List<Collider> audioFrame;
  void AudioBoundary(Vector3 point,Vector3 incoming){
   if(audioFrame==null){audioFrame=new List<Collider>();foreach(var c in Pitch.GetComponentsInChildren<BoxCollider>(true))if(c.name=="Collision_Post"||c.name=="Collision_Crossbar")audioFrame.Add(c);}
   // The pitch limit is tangent to the front of the goal frame. Its analytic
   // clamp can precede PhysX's contact callback. Check actual sphere/frame
   // overlap at the clamped position, never infer metal from goal proximity.
   foreach(var c in audioFrame){if(!c||!c.enabled)continue;var contact=c.ClosestPoint(point);var delta=contact-point;if(delta.sqrMagnitude<=(WorldRadius+.025f)*(WorldRadius+.025f)&&Vector3.Dot(incoming,delta)>0){SoundContact(FootballSound.Post,contact,incoming.magnitude);return;}}
   SoundContact(FootballSound.Boundary,point,incoming.magnitude,.18f);
  }
  public bool IsAbilityWall(Collider collider)=>collider&&(collider==wallColliders[0]||collider==wallColliders[1]);
  void AudioCollision(Collision collision){
   if(!HasAuthority||!AudioActive||PortalBusy||CurrentController||collision.contactCount==0)return;
   var collider=collision.collider;var contact=collision.GetContact(0);float speed=0;
   for(int i=0;i<collision.contactCount;i++)speed=Mathf.Max(speed,Mathf.Abs(Vector3.Dot(collision.relativeVelocity,collision.GetContact(i).normal)));
   if(collider.GetComponentInParent<Athlete>())SoundContact(FootballSound.Deflection,contact.point,speed);
   else if(IsAbilityWall(collider))SoundContact(FootballSound.WallHit,contact.point,speed,.10f);
   else if(collider.name=="Collision_Post"||collider.name=="Collision_Crossbar")SoundContact(FootballSound.Post,contact.point,speed);
   else if(collider.GetComponentInParent<FootballGoalNet>())return; // Net.Contact shares its own fallback gate.
   else if(contact.normal.y>.55f)SoundContact(FootballSound.Bounce,contact.point,speed);
  }
 }
}
