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
    if(Voice.Room!=room){permissionVersion++;PermissionDenied=NeedsDeviceSettings=false;Notice="";}
    Voice.SetRoom(room,PlayerIdentity.DisplayName);
   }
   if(Voice.MicEnabled&&!RoomVoicePermissions.MicrophoneAllowed){Voice.SetMicrophone(false);PermissionDenied=true;}
   Voice.Tick();
   if(Time.unscaledTime>=nextMembership){
    nextMembership=Time.unscaledTime+.25f;members.Clear();
    if(review){foreach(var peer in Voice.Peers)members.Add(peer.Id);}
    else if(rooms&&rooms.Session!=null){foreach(var member in rooms.Session.Players)members.Add(member.Id);}
    Voice.ApplyMembership(members);
   }
  }
  public async Task Join(){
   if(!Available||AskingPermission)return;
   int version=permissionVersion;AskingPermission=true;PermissionDenied=false;Notice="";
   try{
    bool allowed=await RoomVoicePermissions.Request();
    if(!this||version!=permissionVersion)return;
    PermissionDenied=!allowed;
    if(allowed&&!paused)Voice.Join();
   }catch(Exception){if(this)Notice="Could not request microphone access. Check device settings and try again.";}
   finally{if(this)AskingPermission=false;}
  }
  public void Leave(){permissionVersion++;Voice.Leave();Notice="";}
  public void ToggleMicrophone(){
   if(!Voice.Ready)return;
   if(!RoomVoicePermissions.MicrophoneAllowed){PermissionDenied=true;Voice.SetMicrophone(false);return;}
   if(!review&&!transport.HasMicrophone&&!Voice.MicEnabled){NeedsDeviceSettings=true;Notice="No microphone found. Connect one or check device microphone settings.";return;}
   NeedsDeviceSettings=false;Notice="";Voice.SetMicrophone(!Voice.MicEnabled);
  }
  public void ToggleSpeakers(){Voice.SetSpeakers(!Voice.SpeakersEnabled);if(!review){PlayerPrefs.SetInt("RoomVoice.Speakers",Voice.SpeakersEnabled?1:0);PlayerPrefs.Save();}}
  public void SetVolume(float volume){Voice.SetVolume(volume);if(!review)PlayerPrefs.SetFloat("RoomVoice.Volume",Voice.Volume);}
  public string NameOf(RoomVoicePeer peer){
   if(peer.IsSelf)return "You";
   var player=RoomTeleport.Players().FirstOrDefault(p=>p.MemberId.Value.ToString()==peer.Id);
   string value=player?player.Nickname.Value.ToString():peer.Name;
   return string.IsNullOrWhiteSpace(value)?"Crew member":value;
  }
  void OnApplicationPause(bool value){paused=value;Voice?.Suspend(value);if(value)PlayerPrefs.Save();}
  void OnApplicationFocus(bool focused){
   if(Application.isMobilePlatform&&!focused&&Voice!=null){Voice.SetMicrophone(false);}
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
