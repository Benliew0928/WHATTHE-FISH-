using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;

namespace WhatTheFish {
 public sealed class VivoxRoomVoiceTransport:IRoomVoiceTransport {
  IVivoxService sdk;
  bool initialized,subscribed;
  readonly List<RoomVoicePeer> peers=new();
  public event Action AudioDeviceChanged;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  internal bool ReviewSyntheticAudio;
#endif
  public static bool Configured=>RoomVoiceConfiguration.Configured;
  public string Channel {get;private set;}="";
  public bool Connected=>initialized&&sdk.IsLoggedIn&&Channel.Length>0&&sdk.ActiveChannels.ContainsKey(Channel);
  public bool HasMicrophone=>initialized&&sdk.AvailableInputDevices.Count>0;
  public IReadOnlyList<RoomVoicePeer> Peers {
   get {
    peers.Clear();
    if(Connected)foreach(var p in sdk.ActiveChannels[Channel])peers.Add(new RoomVoicePeer{Id=p.PlayerId,Name=p.DisplayName,IsSelf=p.IsSelf,Speaking=p.SpeechDetected,MicrophoneMuted=p.IsMuted,AudioEnergy=(float)p.AudioEnergy});
    return peers;
   }
  }
  public async Task Connect(string channel,string displayName){
   Channel=channel;
   try{
    if(UnityServices.State!=ServicesInitializationState.Initialized||!AuthenticationService.Instance.IsAuthorized)
     throw new RoomVoiceUnavailableException("Reconnect to your online room before joining voice.");
    if(!Configured)throw new RoomVoiceUnavailableException("Room voice is unavailable in this build.");
    sdk=VivoxService.Instance;
    if(!initialized){
     var configuration=new VivoxConfigurationOptions();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
     configuration.ForceCaptureSilence=ReviewSyntheticAudio;
#endif
     await sdk.InitializeAsync(configuration);initialized=true;
    }
    // Set capture privacy before login and before any channel can transmit.
    Microphone(false);Output(false,1);
    var audio=sdk.VivoxGlobalAudioSettings;
    audio.NoiseSuppressionEnabled=true;
    audio.VivoxAcousticEchoCancellationEnabled=true;
    audio.AutomaticGainControlEnabled=true;
    if(sdk.IsLoggedIn&&sdk.SignedInPlayerId!=AuthenticationService.Instance.PlayerId)await sdk.LogoutAsync();
    var login=new LoginOptions{DisplayName=displayName,EnableTTS=false};
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    if(ReviewSyntheticAudio)login.ParticipantUpdateFrequency=ParticipantPropertyUpdateFrequency.FivePerSecond;
#endif
    if(!sdk.IsLoggedIn)await sdk.LoginAsync(login);
    Microphone(false);Output(false,1);Subscribe();
    await sdk.JoinGroupChannelAsync(channel,ChatCapability.AudioOnly,new ChannelOptions{MakeActiveChannelUponJoining=true});
    Microphone(false);
   }catch(Exception e){
    Debug.LogWarning("ROOM_VOICE_CONNECT_FAILED type="+e.GetType().Name+(e is RequestFailedException request?" code="+request.ErrorCode:""));
    throw;
   }
  }
  void Subscribe(){
   if(subscribed)return;subscribed=true;sdk.ParticipantAddedToChannel+=ParticipantAdded;
   sdk.EffectiveInputDeviceChanged+=DeviceChanged;sdk.EffectiveOutputDeviceChanged+=DeviceChanged;
  }
  void Unsubscribe(){
   if(!subscribed)return;subscribed=false;sdk.ParticipantAddedToChannel-=ParticipantAdded;
   sdk.EffectiveInputDeviceChanged-=DeviceChanged;sdk.EffectiveOutputDeviceChanged-=DeviceChanged;
  }
  void DeviceChanged()=>AudioDeviceChanged?.Invoke();
  void ParticipantAdded(VivoxParticipant participant){
   if(participant.IsSelf||participant.ChannelName!=Channel)return;
   // Only the room roster may enable a remote participant's playback.
   participant.MutePlayerLocally();
  }
  public async Task Disconnect(){
   if(!initialized){Channel="";return;}
   Microphone(false);Output(false,1);
   try{
    try{if(sdk.ActiveChannels.Count>0)await sdk.LeaveAllChannelsAsync();}
    finally{if(sdk.IsLoggedIn)await sdk.LogoutAsync();}
    Channel="";peers.Clear();
   }finally{Unsubscribe();}
  }
  public void Microphone(bool enabled){
   if(!initialized)return;
   if(enabled&&Connected){if(sdk.IsInputDeviceMuted)sdk.UnmuteInputDevice();}
   else if(!sdk.IsInputDeviceMuted)sdk.MuteInputDevice();
  }
  public void Output(bool enabled,float volume){
   if(!initialized)return;
   bool audible=enabled&&volume>0&&Connected;
   if(audible){
    int db=Mathf.Clamp(Mathf.RoundToInt(20*Mathf.Log10(Mathf.Max(.0032f,volume))),-50,0);
    if(sdk.OutputDeviceVolume!=db)sdk.SetOutputDeviceVolume(db);
    if(sdk.IsOutputDeviceMuted)sdk.UnmuteOutputDevice();
   }else if(!sdk.IsOutputDeviceMuted)sdk.MuteOutputDevice();
  }
  public void MutePeer(string id,bool muted){
   if(!Connected)return;
   var peer=sdk.ActiveChannels[Channel].FirstOrDefault(p=>p.PlayerId==id&&!p.IsSelf);if(peer==null)return;
   // Vivox acknowledges local mute changes asynchronously. Reconcile against
   // its actual state each membership tick: a cached request can miss a late
   // mute acknowledgement after a newly admitted room member was unmuted.
   // The SDK itself skips native requests when its state already matches.
   if(muted)peer.MutePlayerLocally();else peer.UnmutePlayerLocally();
  }
 }
}
