using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WhatTheFish {
 public sealed class RoomVoicePeer {
  public string Id,Name;
  public bool Speaking,IsSelf,MicrophoneMuted;
  public float AudioEnergy;
 }

 // The transport is independent of NGO: game host recovery must not interrupt voice.
 public interface IRoomVoiceTransport {
  string Channel {get;}
  bool Connected {get;}
  IReadOnlyList<RoomVoicePeer> Peers {get;}
  Task Connect(string channel,string displayName);
  Task Disconnect();
  void Microphone(bool enabled);
  void Output(bool enabled,float volume);
  void MutePeer(string id,bool muted);
 }

 // Serializes SDK operations. A delayed join is never allowed to unmute a left
 // room, nor can its completion overwrite the next room's state.
 public sealed class RoomVoiceSession {
  readonly IRoomVoiceTransport transport;
  readonly HashSet<string> muted=new(StringComparer.Ordinal);
  readonly HashSet<string> allowedMembers=new(StringComparer.Ordinal);
  readonly Func<double> clock;
  string room="",displayName="Player";
  int generation,failures;
  bool busy,suspended,disposed,wasReady;
  double nextAttempt;
  public bool Enabled {get;private set;}
  public bool MicEnabled {get;private set;}
  public bool SpeakersEnabled {get;private set;}=true;
  public float Volume {get;private set;}=1;
  public string Error {get;private set;}="";
  public bool Busy=>busy;
  public bool Suspended=>suspended;
  public bool Exhausted=>failures>=5;
  public string Room=>room;
  public string Channel=>ChannelFor(room);
  public bool Ready=>!disposed&&!suspended&&Enabled&&room.Length>0&&!busy&&transport.Connected&&transport.Channel==Channel;
  public IReadOnlyList<RoomVoicePeer> Peers=>transport.Peers;
  public RoomVoiceSession(IRoomVoiceTransport transport,Func<double> clock){this.transport=transport;this.clock=clock;}
  public static string ChannelFor(string sessionId){
   if(string.IsNullOrEmpty(sessionId))return "";
   using var sha=SHA256.Create();
   return "wtf-room-"+BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(sessionId))).Replace("-","").ToLowerInvariant();
  }
  public void SetRoom(string id,string name){
   displayName=string.IsNullOrWhiteSpace(name)?"Player":name;
   id=id??"";if(room==id)return;
   generation++;room=id;Enabled=false;MicEnabled=false;muted.Clear();allowedMembers.Clear();failures=0;nextAttempt=0;Error="";
   transport.Microphone(false);transport.Output(false,Volume);
  }
  public void Join(){if(disposed||room.Length==0)return;Enabled=true;MicEnabled=false;failures=0;nextAttempt=0;Error="";}
  public void Leave(){generation++;Enabled=false;MicEnabled=false;failures=0;nextAttempt=0;Error="";transport.Microphone(false);transport.Output(false,Volume);}
  public void Suspend(bool value){if(suspended==value)return;suspended=value;generation++;nextAttempt=0;MicEnabled=false;transport.Microphone(false);if(value)transport.Output(false,Volume);}
  public void SetMicrophone(bool value){MicEnabled=value&&Ready;transport.Microphone(MicEnabled);}
  public void SetSpeakers(bool value){SpeakersEnabled=value;transport.Output(Ready&&value,Volume);}
  public void SetVolume(float value){if(float.IsNaN(value)||float.IsInfinity(value))return;Volume=Math.Max(0,Math.Min(1,value));transport.Output(Ready&&SpeakersEnabled,Volume);}
  public bool IsMuted(string id)=>id!=null&&muted.Contains(id);
  public bool IsRoomMember(string id)=>id!=null&&allowedMembers.Contains(id);
  public void ToggleMute(string id){if(string.IsNullOrEmpty(id)||!allowedMembers.Contains(id))return;if(!muted.Add(id))muted.Remove(id);transport.MutePeer(id,muted.Contains(id));}
  public void ApplyMembership(ISet<string> members){
   allowedMembers.Clear();allowedMembers.UnionWith(members);
   foreach(var peer in transport.Peers)if(!peer.IsSelf)transport.MutePeer(peer.Id,!members.Contains(peer.Id)||muted.Contains(peer.Id));
  }
  public void Tick(){
   bool ready=Ready;
   if(wasReady&&!ready){MicEnabled=false;transport.Microphone(false);}
   wasReady=ready;
   if(busy)return;
   string desired=!disposed&&!suspended&&Enabled?Channel:"";
   if(desired.Length==0&&transport.Channel.Length==0&&!transport.Connected)return;
   if(desired.Length>0&&transport.Connected&&transport.Channel==desired){
    transport.Microphone(MicEnabled);transport.Output(SpeakersEnabled,Volume);return;
   }
   if(clock()<nextAttempt||desired.Length>0&&Exhausted)return;
   busy=true;_ = Reconcile(desired,generation);
  }
  async Task Reconcile(string desired,int version){
   try{
    MicEnabled=false;transport.Microphone(false);transport.Output(false,Volume);
    await transport.Disconnect();
    if(version!=generation||disposed||suspended||!Enabled||desired.Length==0)return;
    await transport.Connect(desired,displayName);
    // Privacy controls are applied inside the adapter before channel connection,
    // and again here. Cleanup follows immediately if this join became stale.
    transport.Microphone(false);
    if(version!=generation||disposed||suspended||!Enabled){await transport.Disconnect();return;}
    if(!transport.Connected||transport.Channel!=desired)throw new InvalidOperationException("Voice channel did not connect.");
    failures=0;Error="";transport.Output(SpeakersEnabled,Volume);
   }catch(Exception e){
    transport.Microphone(false);
    if(disposed||version!=generation){try{await transport.Disconnect();}catch(Exception){ }return;}
    if(version==generation&&!disposed){
     failures++;nextAttempt=clock()+Math.Min(30,Math.Pow(2,failures));
     Error=e is RoomVoiceUnavailableException?e.Message:"Voice could not connect. Check your connection and try again.";
    }
   }finally{busy=false;}
  }
  public void Dispose(){disposed=true;Leave();Tick();}
 }
 public sealed class RoomVoiceUnavailableException:Exception {public RoomVoiceUnavailableException(string message):base(message){} }
}
