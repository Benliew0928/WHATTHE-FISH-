using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public struct GolfAimState:INetworkSerializable,IEquatable<GolfAimState> {
  public bool active;public ulong owner;public uint round,reset;public Vector3 ballPosition;public GolfShotMode mode;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref active);s.SerializeValue(ref owner);s.SerializeValue(ref round);s.SerializeValue(ref reset);s.SerializeValue(ref ballPosition);s.SerializeValue(ref mode);}
  public bool Equals(GolfAimState b)=>active==b.active&&owner==b.owner&&round==b.round&&reset==b.reset&&ballPosition==b.ballPosition&&mode==b.mode;
 }
 public sealed partial class GolfMatchManager {
  public const float AimReach=3f;
  readonly Dictionary<Athlete,GolfAimState> aims=new();
  readonly List<Athlete> invalidAims=new();
  public GolfAimState Aim(Athlete actor){if(!actor)return default;var net=actor.GetComponent<NetworkAthlete>();return !Authority&&net?net.GolfAim.Value:aims.TryGetValue(actor,out var value)?value:default;}
  public bool IsAiming(Athlete actor)=>Aim(actor).active;
  public GolfBall AimCandidate(Athlete actor){
   if(!CanSwing(actor))return null;GolfBall nearest=null;float distance=AimReach*AimReach;
   foreach(var ball in balls.Values){if(!AimEligible(actor,ball))continue;float d=(ball.Body.position-actor.transform.position).sqrMagnitude;if(d<distance&&TryStance(actor,ball,out _,PlayerView.Instance&&PlayerView.Instance.target==actor?PlayerView.Instance.yaw:actor.transform.eulerAngles.y)){nearest=ball;distance=d;}}
   return nearest;
  }
  bool AimEligible(Athlete actor,GolfBall ball){var p=ball?State.Player(ball.Owner):null;return ball&&ball.ReadyToAim&&p!=null&&!p.IsFinished&&!p.IsDNF&&Vector3.Distance(actor.transform.position,ball.Body.position)<=AimReach&&VisibleBall(actor,ball);}
  bool VisibleBall(Athlete actor,GolfBall ball)=>!Physics.Linecast(actor.transform.position+Vector3.up*.5f,ball.Body.position+Vector3.up*.04f,out var hit,1<<8,QueryTriggerInteraction.Ignore)||hit.distance>Vector3.Distance(actor.transform.position+Vector3.up*.5f,ball.Body.position)-.07f;
  public bool TryStance(Athlete actor,GolfBall ball,out Vector3 position,float heading=0){
   // Take a golfer's side-on stance once at entry. Later aim rotation never
   // drags the feet around the ball. Fall back toward the approach if blocked.
   var side=Quaternion.Euler(0,heading,0)*Vector3.left;
   if(StanceAt(actor,ball,side,out position))return true;
   var offset=Vector3.ProjectOnPlane(actor.transform.position-ball.Body.position,Vector3.up);
   if(offset.sqrMagnitude<.001f)offset=-actor.transform.forward;
   return StanceAt(actor,ball,offset.normalized,out position);
  }
  bool StanceAt(Athlete actor,GolfBall ball,Vector3 offset,out Vector3 position){
   position=actor.transform.position;var desired=ball.Body.position+offset*.85f;
   if(!Physics.Raycast(desired+Vector3.up*1.2f,Vector3.down,out var ground,2.5f,1<<8,QueryTriggerInteraction.Ignore)||ground.normal.y<.8f||Mathf.Abs(ground.point.y-actor.transform.position.y)>.6f)return false;
   desired=ground.point+Vector3.up*.035f;
   foreach(var other in Athlete.Active)if(other&&other!=actor&&Vector3.Distance(other.transform.position,desired)<.6f)return false;
   var capsule=actor.capsule;float radius=capsule.radius*.9f;float height=capsule.height;
   var lower=desired+Vector3.up*(radius+.09f);var upper=desired+Vector3.up*(height-radius);
   if(Physics.CheckCapsule(lower,upper,radius,(1<<8)|(1<<9),QueryTriggerInteraction.Ignore))return false;
   var delta=desired-position;var a=position+Vector3.up*(radius+.16f);var b=position+Vector3.up*(height-radius);
   if(delta.magnitude>.01f&&Physics.CapsuleCast(a,b,radius,delta.normalized,delta.magnitude,(1<<8)|(1<<9),QueryTriggerInteraction.Ignore))return false;
   if(Vector3.Distance(desired,ball.Body.position)>GolfClubMotion.SwingReach)return false;
   position=desired;return true;
  }
  public bool SetAim(Athlete actor,bool enabled,ulong owner,uint round,float heading=0){
   if(!Authority||!actor||round!=Round||!float.IsFinite(heading))return false;
   if(!enabled){ReleaseAim(actor);return true;}
   var existing=Aim(actor);if(existing.active)return existing.owner==owner;
   var ball=Ball(owner);
   if(!CanSwing(actor)||!AimEligible(actor,ball)||!TryStance(actor,ball,out var feet,heading))return false;
   bool controller=actor.capsule.enabled;actor.capsule.enabled=false;actor.transform.position=feet;actor.ResetLocomotion();actor.capsule.enabled=controller;
   actor.GolfClubMotion.Equip(Round);actor.GolfClubMotion.Address(ball.Body.position);Physics.SyncTransforms();
   var state=new GolfAimState{active=true,owner=owner,round=Round,reset=ball.ResetSequence,ballPosition=ball.Body.position,mode=ShotMode(ball)};aims[actor]=state;
   var net=actor.GetComponent<NetworkAthlete>();if(net&&net.IsSpawned){net.GolfAim.Value=state;net.GetComponent<Unity.Netcode.Components.NetworkTransform>().Teleport(feet,actor.transform.rotation,actor.transform.localScale);}
   return true;
  }
  public void ReleaseAim(Athlete actor){
   if(ReferenceEquals(actor,null))return;aims.Remove(actor);if(!actor)return;var net=actor.GetComponent<NetworkAthlete>();if(Authority&&net&&net.IsSpawned)net.GolfAim.Value=default;
   if(actor.GolfClubMotion)actor.GolfClubMotion.Charge(false,0,Vector3.zero,Round);
  }
  void ClearAims(){invalidAims.Clear();invalidAims.AddRange(aims.Keys);foreach(var actor in invalidAims)ReleaseAim(actor);aims.Clear();}
  void ValidateAims(){
   invalidAims.Clear();foreach(var item in aims){var actor=item.Key;var aim=item.Value;var ball=Ball(aim.owner);
    if(!actor||!CanSwing(actor)||!ball||!ball.ReadyToAim||ball.ResetSequence!=aim.reset||(ball.Body.position-aim.ballPosition).sqrMagnitude>.01f||!InRange(actor,ball))invalidAims.Add(actor);
   }foreach(var actor in invalidAims)ReleaseAim(actor);
  }
 }
}
