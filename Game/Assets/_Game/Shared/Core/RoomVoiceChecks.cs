#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WhatTheFish {
 public static class RoomVoiceChecks {
  public sealed class Fake:IRoomVoiceTransport {
   public string Channel {get;set;}="";public bool Connected {get;set;}
   public bool Mic,Sound;public float Volume;public int Joins,Leaves,Failures;public TaskCompletionSource<bool> Gate;
   public readonly List<RoomVoicePeer> People=new();public readonly Dictionary<string,bool> Muted=new();
   public IReadOnlyList<RoomVoicePeer> Peers=>People;
   public async Task Connect(string channel,string name){
    if(Connected)throw new Exception("Two channels active");Joins++;Channel=channel;
    if(Gate!=null){var gate=Gate;Gate=null;await gate.Task;}
    if(Failures-->0)throw new Exception("Simulated network failure");Connected=true;
   }
   public Task Disconnect(){Leaves++;Mic=false;Sound=false;Connected=false;Channel="";return Task.CompletedTask;}
   public void Microphone(bool enabled){Mic=enabled;}
   public void Output(bool enabled,float volume){Sound=enabled;Volume=volume;}
   public void MutePeer(string id,bool muted){Muted[id]=muted;}
  }
  public static async Task<string[]> Run(){
   var results=new List<string>();void Check(bool value,string name){results.Add((value?"PASS ":"FAIL ")+name);if(!value)throw new Exception(name);}
   double now=0;var fake=new Fake();var voice=new RoomVoiceSession(fake,()=>now);
   Check(RoomVoiceSession.ChannelFor("session-A")==RoomVoiceSession.ChannelFor("session-A"),"same room shares one deterministic channel");
   Check(RoomVoiceSession.ChannelFor("session-A")!=RoomVoiceSession.ChannelFor("session-B"),"different rooms have different channels");
   Check(RoomVoiceSession.ChannelFor("")=="","offline has no channel");
   voice.Join();voice.Tick();Check(fake.Joins==0,"cannot join without an online room");
   voice.SetRoom("session-A","Alice");voice.SetMicrophone(true);Check(!fake.Mic&&!voice.MicEnabled,"disconnected mic cannot activate");
   voice.Join();voice.Tick();Check(voice.Ready&&fake.Connected,"room voice joins successfully");
   Check(!voice.MicEnabled&&!fake.Mic,"joins with microphone muted");
   Check(fake.Sound,"connected output is enabled");
   voice.SetMicrophone(true);Check(fake.Mic&&voice.MicEnabled,"explicit mic action enables capture");
   int joins=fake.Joins;for(int i=0;i<30;i++){voice.SetRoom("session-A","New display name");voice.Tick();}
   Check(fake.Joins==joins&&fake.Mic,"sport/name/host updates keep the same channel and mic state");
   voice.SetVolume(.35f);Check(Math.Abs(fake.Volume-.35)<.001,"volume reaches transport");
   voice.SetVolume(float.NaN);voice.SetVolume(float.PositiveInfinity);Check(Math.Abs(voice.Volume-.35)<.001,"invalid volume cannot poison audio state");
   voice.SetVolume(5);Check(voice.Volume==1,"volume is bounded");voice.SetVolume(-1);Check(voice.Volume==0,"volume lower bound");
   voice.SetVolume(.8f);voice.SetSpeakers(false);Check(!fake.Sound&&fake.Mic,"speaker mute does not change microphone choice");voice.SetSpeakers(true);
   fake.People.Add(new RoomVoicePeer{Id="friend"});fake.People.Add(new RoomVoicePeer{Id="outsider"});
   voice.ApplyMembership(new HashSet<string>{"friend"});Check(!fake.Muted["friend"]&&fake.Muted["outsider"],"only current room members are audible");
   voice.ToggleMute("outsider");voice.ToggleMute("outsider");Check(fake.Muted["outsider"],"UI mute actions cannot unmute a nonmember");
   voice.ToggleMute("friend");Check(fake.Muted["friend"],"individual mute applies");voice.ApplyMembership(new HashSet<string>{"friend"});Check(fake.Muted["friend"],"membership refresh preserves individual mute");
   voice.Suspend(true);Check(!fake.Mic&&!fake.Sound&&!voice.MicEnabled,"background immediately silences capture and playback");voice.Tick();Check(!fake.Connected,"background leaves native channel");
   voice.Suspend(false);voice.Tick();Check(voice.Ready&&!fake.Mic&&voice.IsMuted("friend"),"foreground reconnects muted and preserves room mute choices");
   voice.SetMicrophone(true);voice.Tick();fake.Connected=false;voice.Tick();Check(voice.Ready&&!fake.Mic,"network recovery never reactivates the microphone");
   voice.SetRoom("session-B","Alice");Check(!fake.Mic&&!fake.Sound&&!voice.Enabled&&!voice.IsMuted("friend"),"new room resets voice consent and per-room mutes immediately");voice.Tick();Check(!fake.Connected,"new room disconnects old channel");
   var gate=new TaskCompletionSource<bool>();fake.Gate=gate;voice.Join();voice.Tick();Check(voice.Busy,"pending join is serialized");
   voice.SetRoom("session-C","Alice");voice.Join();voice.Tick();Check(voice.Busy,"new room waits for previous SDK operation");gate.SetResult(true);
   await Idle(voice);voice.Tick();Check(voice.Ready&&fake.Channel==RoomVoiceSession.ChannelFor("session-C")&&!fake.Mic,"late join is cleaned up before connecting new room");
   voice.Leave();Check(!fake.Mic&&!fake.Sound,"leave silences audio synchronously");voice.Tick();Check(!fake.Connected,"leave disconnects channel");
   gate=new TaskCompletionSource<bool>();fake.Gate=gate;voice.Join();voice.Tick();voice.Leave();gate.SetResult(true);await Idle(voice);
   Check(!fake.Connected&&!fake.Mic,"leaving during join cleans up delayed completion");
   fake.Failures=10;voice.Join();voice.Tick();joins=fake.Joins;voice.Tick();Check(fake.Joins==joins&&voice.Error.Length>0,"failed join backs off and reports recoverable error");
   for(int i=0;i<4;i++){now+=40;voice.Tick();}Check(voice.Exhausted,"automatic retries are bounded");joins=fake.Joins;now+=100;voice.Tick();Check(fake.Joins==joins,"exhausted retries do not hammer service");
   fake.Failures=0;voice.Join();voice.Tick();Check(voice.Ready&&!voice.Exhausted&&voice.Error.Length==0,"manual retry recovers service");
   voice.Dispose();Check(!fake.Mic&&!fake.Sound&&!fake.Connected,"destroying owner releases channel and microphone");
   voice.Join();voice.Tick();Check(!fake.Connected,"disposed session cannot reconnect");
   fake=new Fake{Failures=1};voice=new RoomVoiceSession(fake,()=>now);voice.SetRoom("failing-room","Alice");voice.Join();voice.Tick();voice.Dispose();Check(fake.Channel.Length==0,"disposal cleans partial connection even during retry backoff");
   return results.ToArray();
  }
  static async Task Idle(RoomVoiceSession voice){for(int i=0;i<2000&&voice.Busy;i++)await Task.Delay(1);if(voice.Busy)throw new Exception("Voice operation did not settle");}
 }
}
#endif
