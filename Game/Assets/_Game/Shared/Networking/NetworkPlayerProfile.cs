using System;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public sealed partial class NetworkAthlete {
  public NetworkVariable<FixedString128Bytes> ProfileId=new();
  public NetworkVariable<FixedString32Bytes> ProfileCode=new();
  public NetworkVariable<int> ProfileAvatarVersion=new(),ProfileAvatarIndex=new(),ProfileHasPhoto=new();
  string profileChallenge="";bool profileChecking,profileRefreshQueued;float profileRefreshAt;
  public PublicProfile PublicProfile=>new(){id=ProfileId.Value.ToString(),code=ProfileCode.Value.ToString(),name=Nickname.Value.ToString(),avatarVersion=ProfileAvatarVersion.Value,avatar=ProfileAvatarIndex.Value,hasPhoto=ProfileHasPhoto.Value};
  async void StartPublicProfile(){
   if(!IsPlayerObject)return;
   if(IsOwner)PlayerProfiles.Changed+=ProfileSaved;
   if(!IsServer||AppRoot.Instance.rooms.LocalTest)return;
   ProfileId.Value=default;profileChallenge=Guid.NewGuid().ToString("N");
   float end=Time.realtimeSinceStartup+50;
   while(this&&IsSpawned&&AppRoot.Instance.rooms.Session==null&&Time.realtimeSinceStartup<end)await Task.Delay(100);
   if(!this||!IsSpawned||AppRoot.Instance.rooms.Session==null)return;
   if(IsOwner){try{ApplyPublicProfile(await PlayerProfiles.EnsureCloud());}catch(Exception){ }return;}
   RequestProfileProofRpc(profileChallenge,AppRoot.Instance.rooms.Session.Id);
   await Task.Delay(30000);
   if(this&&IsSpawned&&ProfileId.Value.Length==0)NetworkManager.DisconnectClient(OwnerClientId,"Could not verify your player profile. Please reconnect.");
  }
  void StopPublicProfile(){PlayerProfiles.Changed-=ProfileSaved;profileChallenge="";}
  async void ProfileSaved(){
   if(!this||!IsSpawned||!IsOwner||AppRoot.Instance.rooms.LocalTest||ProfileId.Value.Length==0||profileRefreshQueued)return;
   profileRefreshQueued=true;await Task.Delay(2200);profileRefreshQueued=false;
   if(this&&IsSpawned&&!PlayerProfiles.PendingSync)RefreshPublicProfileRpc();
  }
  void ApplyPublicProfile(PublicProfile p){
   if(!this||!IsSpawned||!IsServer||p.id!=MemberId.Value.ToString())return;
   ProfileId.Value=p.id;ProfileCode.Value=p.code;Nickname.Value=p.name;ProfileAvatarIndex.Value=p.avatar;ProfileAvatarVersion.Value=p.avatarVersion;ProfileHasPhoto.Value=p.hasPhoto;
  }
  [Rpc(SendTo.Owner,InvokePermission=RpcInvokePermission.Server)]
  void RequestProfileProofRpc(string challenge,string room)=>RequestProfileProof(challenge,room);
  async void RequestProfileProof(string challenge,string room){
   if(!IsOwner||!IsPlayerObject)return;
   try{
    float end=Time.realtimeSinceStartup+15;
    while(this&&IsSpawned&&AppRoot.Instance.rooms.Session==null&&Time.realtimeSinceStartup<end)await Task.Delay(100);
    if(!this||!IsSpawned||AppRoot.Instance.rooms.Session?.Id!=room)return;
    var result=await PlayerProfiles.Json<PlayerProfiles.ProofResponse>("/v1/room-proof","POST",new PlayerProfiles.ProofRequest{room=room,challenge=challenge});
    if(this&&IsSpawned)SubmitProfileProofRpc(result.proof);
   }catch(Exception){if(AppRoot.Instance)AppRoot.Instance.rooms.SeatMessage="Profile verification failed. Please reconnect.";}
  }
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]
  void SubmitProfileProofRpc(string proof)=>SubmitProfileProof(proof);
  async void SubmitProfileProof(string proof){
   if(!IsPlayerObject||profileChecking||ProfileId.Value.Length>0||proof==null||proof.Length!=64||string.IsNullOrEmpty(profileChallenge))return;
   profileChecking=true;
   try{
    var p=await PlayerProfiles.Json<PublicProfile>("/v1/room-proof/verify","POST",new PlayerProfiles.ProofRequest{room=AppRoot.Instance.rooms.Session.Id,challenge=profileChallenge,proof=proof});
    if(!this||!IsSpawned)return;
    if(p.id!=MemberId.Value.ToString()){NetworkManager.DisconnectClient(OwnerClientId,"Player identity did not match this room connection.");return;}
    ApplyPublicProfile(p);
   }catch(Exception){if(this&&IsSpawned)NetworkManager.DisconnectClient(OwnerClientId,"Could not verify player profile. Please reconnect.");}
   finally{profileChecking=false;}
  }
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)]
  void RefreshPublicProfileRpc()=>RefreshPublicProfile();
  async void RefreshPublicProfile(){
   if(ProfileId.Value.Length==0||profileChecking||Time.unscaledTime<profileRefreshAt)return;
   profileRefreshAt=Time.unscaledTime+2;profileChecking=true;string id=ProfileId.Value.ToString();
   try{var p=await PlayerProfiles.Get(id);if(this&&IsSpawned&&ProfileId.Value.ToString()==id)ApplyPublicProfile(p);}
   catch(Exception){ }
   finally{profileChecking=false;}
  }
 }
}
