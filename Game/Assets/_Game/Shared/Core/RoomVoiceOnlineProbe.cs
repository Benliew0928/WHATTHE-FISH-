#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;

namespace WhatTheFish {
 // Isolated developer probe: unique test channel and guest auth profile only.
 // Synthetic file injection exercises the media transport with the mic muted.
 public sealed class RoomVoiceOnlineProbe:MonoBehaviour {
  string folder,testRoom,injection;RoomVoiceSession voice;VivoxRoomVoiceTransport transport;float energy;bool injecting,done;int nativeControls;string outcome="starting";
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-voiceOnlineProbe");if(i<0||i+1>=args.Length)return;
   var p=new GameObject("Online voice probe").AddComponent<RoomVoiceOnlineProbe>();p.folder=args[i+1];i=Array.IndexOf(args,"-voiceTestRoom");p.testRoom=i>=0&&i+1<args.Length?args[i+1]:"";i=Array.IndexOf(args,"-voiceInject");p.injection=i>=0&&i+1<args.Length?args[i+1]:"";
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);if(string.IsNullOrWhiteSpace(testRoom)){Finish("FAIL missing isolated test room");yield break;}
   while(!AppRoot.Instance||!RoomChatUI.Instance)yield return null;
   var login=PlayerAccount.EnsureOnline();while(!login.IsCompleted)yield return null;
   if(login.IsFaulted){Finish("FAIL authentication "+login.Exception.GetBaseException().GetType().Name);yield break;}
   if(!VivoxRoomVoiceTransport.Configured){Finish("FAIL Vivox project settings are missing. Enable Vivox, fetch settings in Unity Services, then rebuild.");yield break;}
   transport=new VivoxRoomVoiceTransport{ReviewSyntheticAudio=true};voice=new RoomVoiceSession(transport,()=>Time.realtimeSinceStartupAsDouble);voice.SetRoom(testRoom,"Voice QA");voice.Join();
   float deadline=Time.realtimeSinceStartup+120;
   while(!voice.Ready&&Time.realtimeSinceStartup<deadline){voice.Tick();Save();yield return null;}
   if(!voice.Ready){Finish("FAIL native voice connection "+voice.Error);yield break;}
   outcome="connected";Save();deadline=Time.realtimeSinceStartup+100;
   while(Time.realtimeSinceStartup<deadline&&!File.Exists(Path.Combine(folder,"stop.txt"))){
    voice.Tick();var peers=voice.Peers.ToArray();voice.ApplyMembership(new HashSet<string>(peers.Select(p=>p.Id)),peers.ToDictionary(p=>p.Id,p=>!p.IsSelf));
    var remote=VivoxService.Instance.ActiveChannels.TryGetValue(voice.Channel,out var participants)?participants.FirstOrDefault(p=>!p.IsSelf):null;
    if(remote!=null){
     if(nativeControls==0){voice.SetPeerVolume(remote.PlayerId,.25f);nativeControls=1;}
     else if(nativeControls==1&&remote.LocalVolume==-12){voice.MuteSpectators(true);nativeControls=2;}
     else if(nativeControls==2&&remote.IsMuted){voice.MuteSpectators(false);voice.SetPeerVolume(remote.PlayerId,1);nativeControls=3;}
     else if(nativeControls==3&&!remote.IsMuted&&remote.LocalVolume==0)nativeControls=4;
    }
    foreach(var peer in peers)if(!peer.IsSelf)energy=Mathf.Max(energy,peer.AudioEnergy);
    if(!injecting&&injection.Length>0&&peers.Length>=2&&nativeControls==4){
     if(!File.Exists(injection)){Finish("FAIL injection file missing");yield break;}
     VivoxService.Instance.StartAudioInjection(injection);injecting=true;outcome="injecting";
    }
    Save();yield return new WaitForSecondsRealtime(.1f);
   }
   if(injecting)VivoxService.Instance.StopAudioInjection();
   bool micMuted=VivoxService.Instance.IsInputDeviceMuted;voice.Leave();voice.Tick();while(voice.Busy)yield return null;
   Finish(micMuted?(energy>0&&nativeControls==4?"VOICE_ONLINE_PASS":"FAIL remote audio or native participant controls"):"FAIL microphone unexpectedly enabled");
  }
  void Save(){
   var peers=voice?.Peers.ToArray()??Array.Empty<RoomVoicePeer>();bool connected=transport!=null&&transport.Connected;
   var snapshot=new State{status=outcome,connected=voice?.Ready??false,channel=voice?.Channel??"",peers=peers.Length,maxRemoteEnergy=energy,micEnabled=voice?.MicEnabled??false,error=voice?.Error??"",participantControls=nativeControls==4,remoteMuted=peers.Count(p=>!p.IsSelf&&p.MicrophoneMuted),nativeInjection=connected&&VivoxService.Instance.IsInjectingAudio,inputMuted=!connected||VivoxService.Instance.IsInputDeviceMuted,outputMuted=!connected||VivoxService.Instance.IsOutputDeviceMuted,transmittingChannels=connected?VivoxService.Instance.TransmittingChannels.Count:0};
   string temp=Path.Combine(folder,"state.pending"),target=Path.Combine(folder,"state.json");File.WriteAllText(temp,JsonUtility.ToJson(snapshot,true));try{if(File.Exists(target))File.Replace(temp,target,null);else File.Move(temp,target);}catch(IOException){}
  }
  [Serializable]sealed class State{public string status,channel,error;public bool connected,micEnabled,nativeInjection,inputMuted,outputMuted,participantControls;public int peers,remoteMuted,transmittingChannels;public float maxRemoteEnergy;}
  void Finish(string result){outcome=result;done=true;Save();File.WriteAllText(Path.Combine(folder,"results.txt"),result+"\n");voice?.Dispose();Application.Quit(result.StartsWith("FAIL")?1:0);}
  void OnDestroy(){if(!done)voice?.Dispose();}
 }
}
#endif
