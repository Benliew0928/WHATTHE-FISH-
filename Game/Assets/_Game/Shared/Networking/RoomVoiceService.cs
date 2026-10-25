using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace WhatTheFish {
 // This component belongs to RoomService, so sport scenes and avatar respawns do
 // not own the voice connection. Microphone capture is handled by the native SDK.
 public sealed class RoomVoiceService:MonoBehaviour {
  public RoomVoiceSession Voice {get;private set;}
  public bool AskingPermission {get;private set;}
  public bool PermissionDenied {get;private set;}
  public bool NeedsDeviceSettings {get;private set;}
  public string Notice {get;private set;}="";
  public bool SetupMissing=>Voice!=null&&Voice.Room.Length>0&&!review&&!VivoxRoomVoiceTransport.Configured;
  public bool Available=>Voice!=null&&Voice.Room.Length>0&&!SetupMissing;
  public string Status=>AskingPermission?"Allow microphone access on your device…":SetupMissing?"Room voice is unavailable in this build.":!Available?"Voice is available in online rooms.":PermissionDenied?"Microphone access is off. Allow it in device settings, then try again.":Voice.Suspended?"Voice paused while the app is in the background.":Voice.Ready?(Voice.MicEnabled?"Your microphone is on":"Listening · your microphone is muted"):Voice.Busy?"Connecting room voice…":Voice.Error.Length>0?Voice.Error:Voice.Enabled?"Reconnecting room voice…":"Join your crew's voice channel with your mic muted.";
  RoomService rooms;VivoxRoomVoiceTransport transport;
  readonly HashSet<string> members=new(StringComparer.Ordinal);
  readonly Dictionary<string,bool> roles=new(StringComparer.Ordinal);
  public bool PendingMicrophone {get;private set;}
  float nextMembership;
  int permissionVersion;
  bool review,paused;
  public static RoomVoiceService Ensure(RoomService room){
   var voice=room.GetComponent<RoomVoiceService>();if(!voice)voice=room.gameObject.AddComponent<RoomVoiceService>();voice.rooms=room;return voice;
  }
  void Awake(){
   transport=new VivoxRoomVoiceTransport();transport.AudioDeviceChanged+=VoiceDeviceChanged;Voice=new RoomVoiceSession(transport,()=>Time.realtimeSinceStartupAsDouble);
   Voice.SetVolume(PlayerPrefs.GetFloat("RoomVoice.Volume",.8f));
   Voice.SetSpeakers(PlayerPrefs.GetInt("RoomVoice.Speakers",1)!=0);
   AudioSettings.OnAudioConfigurationChanged+=AudioChanged;
  }
  void Update(){
   if(!review){
    string room=rooms&&rooms.Connected&&!rooms.LocalTest?rooms.Session?.Id??"":"";
    if(Voice.Room!=room){permissionVersion++;PendingMicrophone=false;PermissionDenied=NeedsDeviceSettings=false;Notice="";}
    Voice.SetRoom(room,PlayerIdentity.DisplayName);
   }
   if(Voice.MicEnabled&&!RoomVoicePermissions.MicrophoneAllowed){Voice.SetMicrophone(false);PermissionDenied=true;}
   Voice.Tick();
   if(PendingMicrophone&&Voice.Ready){PendingMicrophone=false;if(!paused)ToggleMicrophone();}
   if(Voice.Exhausted)PendingMicrophone=false;
   if(Time.unscaledTime>=nextMembership){
    nextMembership=Time.unscaledTime+.25f;members.Clear();roles.Clear();
    if(review){foreach(var peer in Voice.Peers){members.Add(peer.Id);roles[peer.Id]=peer.Spectator;}}
    else if(rooms&&rooms.Session!=null){foreach(var member in rooms.Session.Players)members.Add(member.Id);}
    if(!review)foreach(var actor in RoomTeleport.Players()){string id=actor.MemberId.Value.ToString();if(members.Contains(id))roles[id]=actor.IsSpectator;}
    Voice.ApplyMembership(members,roles);
   }
  }
  public async Task Join(bool speakWhenReady=false){
   if(!Available||AskingPermission)return;
   int version=permissionVersion;AskingPermission=true;PermissionDenied=false;Notice="";
   try{
    bool allowed=await RoomVoicePermissions.Request();
    if(!this||version!=permissionVersion)return;
    PermissionDenied=!allowed;
    if(allowed&&!paused){Voice.Join();PendingMicrophone=speakWhenReady;}
   }catch(Exception){if(this)Notice="Could not request microphone access. Check device settings and try again.";}
   finally{if(this)AskingPermission=false;}
  }
  public void Leave(){permissionVersion++;PendingMicrophone=false;Voice.Leave();Notice="";}
  public void TapMicrophone(){
   if(PendingMicrophone){PendingMicrophone=false;Voice.SetMicrophone(false);return;}
   if(Voice.Ready){ToggleMicrophone();return;}
   if(!AskingPermission&&!Voice.Busy)_=Join(true);
  }
  public void ToggleMicrophone(){
   if(!Voice.Ready)return;
   if(!RoomVoicePermissions.MicrophoneAllowed){PermissionDenied=true;Voice.SetMicrophone(false);return;}
   if(!review&&!transport.HasMicrophone&&!Voice.MicEnabled){NeedsDeviceSettings=true;Notice="No microphone found. Connect one or check device microphone settings.";return;}
   NeedsDeviceSettings=false;Notice="";Voice.SetMicrophone(!Voice.MicEnabled);
  }
  public void ToggleSpeakers(){Voice.SetSpeakers(!Voice.SpeakersEnabled);if(!review){PlayerPrefs.SetInt("RoomVoice.Speakers",Voice.SpeakersEnabled?1:0);PlayerPrefs.Save();}}
  public void SetVolume(float volume){Voice.SetVolume(volume);if(!review)PlayerPrefs.SetFloat("RoomVoice.Volume",Voice.Volume);}
  public string NameOf(RoomVoicePeer peer){
   string value=peer.Name;
   if(string.IsNullOrWhiteSpace(value)&&peer.IsSelf&&!review)value=PlayerIdentity.DisplayName;
   return string.IsNullOrWhiteSpace(value)?"Joining…":value;
  }
  public IReadOnlyList<RoomVoicePeer> Roster {
   get {
    if(review)return Voice.Peers;
    var connected=Voice.Peers.ToDictionary(p=>p.Id,p=>p,StringComparer.Ordinal);var result=new List<RoomVoicePeer>();
    foreach(var actor in RoomTeleport.Players()){
     string id=actor.MemberId.Value.ToString();if(!Voice.IsRoomMember(id))continue;
     if(!connected.TryGetValue(id,out var peer))peer=new RoomVoicePeer{Id=id,Connected=false};
     peer.Name=actor.Nickname.Value.ToString();peer.Spectator=actor.IsSpectator;peer.IsSelf=id==rooms.MemberId;result.Add(peer);
    }
    return result;
   }
  }
  void OnApplicationPause(bool value){paused=value;if(value){permissionVersion++;PendingMicrophone=false;}Voice?.Suspend(value);if(value)PlayerPrefs.Save();}
  void OnApplicationFocus(bool focused){
   if(Application.isMobilePlatform&&!focused&&Voice!=null){PendingMicrophone=false;Voice.SetMicrophone(false);}
  }
  void AudioChanged(bool deviceChanged){if(deviceChanged&&Voice!=null&&Voice.MicEnabled){Voice.SetMicrophone(false);Notice="Audio device changed. Turn your mic on when ready.";}}
  void VoiceDeviceChanged()=>AudioChanged(true);
  void OnDestroy(){permissionVersion++;AudioSettings.OnAudioConfigurationChanged-=AudioChanged;if(transport!=null)transport.AudioDeviceChanged-=VoiceDeviceChanged;Voice?.Dispose();}
  void OnApplicationQuit(){Voice?.Leave();PlayerPrefs.Save();}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  internal void ReviewUseSession(RoomVoiceSession session){Voice.Dispose();Voice=session;review=true;}
#endif
 }
}
