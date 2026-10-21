using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace WhatTheFish {
 public sealed partial class NetworkAthlete {
  public NetworkVariable<FixedString128Bytes> MemberId=new();
  public NetworkVariable<ulong> ParticipantId=new(ulong.MaxValue);
  public NetworkVariable<ulong> NextParticipantId=new();
  public NetworkVariable<double> RoomHeartbeat=new();
  public NetworkVariable<bool> MigrationReady=new(true);
  public NetworkVariable<bool> RecoveryAcknowledged=new(false);
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] void RecoveryAcknowledgedRpc(){RecoveryAcknowledged.Value=true;}
  public ulong StableId=>ParticipantId.Value;
  NetworkVariableBase[] MigrationFields(bool world)=>world?new NetworkVariableBase[]{
   NextParticipantId,Exploring,FootballMinutes,BasketballMinutes,BasketballMatch,WorldSport,WorldTravel,WorldTeleport,WorldAppearance,AllowSpectators,Basketball,Ball,Match,FootballPlayers,BasketballPlayers,GolfMatch,GolfPlayers,FishingMatch,FishingPlayers,FishingFish
  }:new NetworkVariableBase[]{
   MemberId,ParticipantId,Ready,Nickname,ProfileId,ProfileCode,ProfileAvatarVersion,ProfileAvatarIndex,ProfileHasPhoto,RoomSeat,IdentityNumber,AppearanceData,BasketballTeam,Team,Motion,Jump,Football,Effort,BasketballProp,FootballProp,BasketballPose,BasketballFreeRoam,FootballPose,Cart,GolfBall,GolfPose,GolfAim
  };
  internal byte[] CaptureFields(bool world){
   using var writer=new FastBufferWriter(1024,Allocator.Temp,262144);
   foreach(var field in MigrationFields(world))field.WriteField(writer);
   return writer.ToArray();
  }
  internal void RestoreFields(byte[] bytes,bool world){
   using var reader=new FastBufferReader(bytes,Allocator.Temp);
   foreach(var field in MigrationFields(world)){field.ReadField(reader);field.SetDirty(true);}
   if(world){
    // A journey's roster uses connection object IDs. Settle at the current
    // island rather than replaying an old roster against newly spawned peers.
    var teleport=WorldTeleport.Value;teleport.phase=RoomTeleportPhase.Idle;WorldTeleport.Value=teleport;
    var sail=WorldTravel.Value;sail.phase=SkySailPhase.Idle;WorldTravel.Value=sail;
   }else{appearanceInitialized=true;athlete.capsule.excludeLayers=IsSpectator?~(1<<8):0;}
  }
  internal void RestorePosition(Vector3 position,Quaternion rotation){
   athlete.inTransit=false;athlete.capsule.enabled=false;transform.SetPositionAndRotation(position,rotation);athlete.ResetLocomotion();
   GetComponent<NetworkTransform>().Teleport(position,rotation,Vector3.one);athlete.capsule.enabled=IsServer;
  }
 }
}
