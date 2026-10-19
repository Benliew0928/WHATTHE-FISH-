using Unity.Netcode.Components;
using UnityEngine;

namespace WhatTheFish {
 // Server-authoritative, latest-received pose presentation for the local sports
 // player. Other players retain NGO's timestamp buffer. This is not prediction:
 // no client physics, ball side effects or client-authored positions are sent.
 public sealed class ResponsiveNetworkTransform:NetworkTransform {
  Vector3 latestPosition,presentedPosition,latestAngles;
  Quaternion presentedRotation;
  bool received,primed,snap;
  Athlete actor;
  public bool PresentingLatest {get;private set;}
  public override void OnNetworkSpawn(){base.OnNetworkSpawn();actor=GetComponent<Athlete>();ResetPresentation();}
  void ResetPresentation(){latestPosition=presentedPosition=transform.position;latestAngles=transform.eulerAngles;presentedRotation=transform.rotation;received=primed=snap=false;PresentingLatest=false;}
  protected override void OnNetworkTransformStateUpdated(ref NetworkTransformState oldState,ref NetworkTransformState state){
   base.OnNetworkTransformStateUpdated(ref oldState,ref state);
   // This prefab uses full precision world-space positions and Euler rotation.
   // If its encoding changes, use the stock presentation until adapted/tested.
   if(InLocalSpace||UseHalfFloatPrecision||UseQuaternionSynchronization)return;
   var p=state.GetPosition();var angles=state.GetRotation().eulerAngles;
   if(state.HasPositionX)latestPosition.x=p.x;if(state.HasPositionY)latestPosition.y=p.y;if(state.HasPositionZ)latestPosition.z=p.z;
   if(state.HasRotAngleX)latestAngles.x=angles.x;if(state.HasRotAngleY)latestAngles.y=angles.y;if(state.HasRotAngleZ)latestAngles.z=angles.z;
   received=true;snap|=state.IsTeleportingNextFrame;
  }
  public override void OnUpdate(){
   base.OnUpdate();
   var host=NetworkAthlete.HostPlayer;
   bool eligible=IsSpawned&&!IsServer&&IsOwner&&!NetworkResponsiveness.Baseline&&actor&&!actor.IsSpectator&&!actor.inTransit&&!RoomService.Paused&&
    host&&host.Exploring.Value&&(host.WorldSport.Value==SportId.Football||host.WorldSport.Value==SportId.Basketball)&&
    !InLocalSpace&&!UseHalfFloatPrecision&&!UseQuaternionSynchronization;
   if(!eligible){ResetPresentation();return;}
   if(!received)return;
   var rotation=Quaternion.Euler(latestAngles);
   if(!primed||snap||Vector3.Distance(presentedPosition,latestPosition)>3){presentedPosition=latestPosition;presentedRotation=rotation;}
   else {
    float weight=1-Mathf.Exp(-Time.unscaledDeltaTime/.025f);
    presentedPosition=Vector3.Lerp(presentedPosition,latestPosition,weight);presentedRotation=Quaternion.Slerp(presentedRotation,rotation,weight);
   }
   primed=true;snap=false;PresentingLatest=true;transform.SetPositionAndRotation(presentedPosition,presentedRotation);
  }
 }
}
