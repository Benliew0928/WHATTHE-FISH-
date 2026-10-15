using System;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public enum RoomTeleportPhase:byte { Idle, Preparing, Departing, Arriving, Complete, Cancelled }
 public struct RoomTeleportState:INetworkSerializable,IEquatable<RoomTeleportState> {
  public bool entry;public uint sequence;public RoomTeleportPhase phase;public SportId from,to;public TeleportDestination destination;public double started;public FixedString512Bytes roster;public FixedString128Bytes reason;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref entry);s.SerializeValue(ref sequence);s.SerializeValue(ref phase);s.SerializeValue(ref from);s.SerializeValue(ref to);s.SerializeValue(ref destination);s.SerializeValue(ref started);s.SerializeValue(ref roster);s.SerializeValue(ref reason);}
  public bool Equals(RoomTeleportState b)=>entry==b.entry&&sequence==b.sequence&&phase==b.phase&&from==b.from&&to==b.to&&destination==b.destination&&started==b.started&&roster.Equals(b.roster)&&reason.Equals(b.reason);
 }
 public struct RoomTeleportAck:INetworkSerializable,IEquatable<RoomTeleportAck> {
  public uint sequence;public byte stage;public bool failed;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref sequence);s.SerializeValue(ref stage);s.SerializeValue(ref failed);}
  public bool Equals(RoomTeleportAck b)=>sequence==b.sequence&&stage==b.stage&&failed==b.failed;
 }
 [DefaultExecutionOrder(650)]
 public sealed class RoomTeleport:MonoBehaviour {
  public static RoomTeleport Instance {get;private set;}
  public static bool Busy=>Instance&&Instance.Active;
  public bool Active=>State.phase is RoomTeleportPhase.Preparing or RoomTeleportPhase.Departing or RoomTeleportPhase.Arriving;
  public RoomTeleportState State=>NetworkAthlete.HostPlayer?NetworkAthlete.HostPlayer.WorldTeleport.Value:default;
  public string Message {get;private set;}="";
  public static double Clock=>NetworkManager.Singleton&&NetworkManager.Singleton.IsListening?NetworkManager.Singleton.ServerTime.Time:Time.unscaledTimeAsDouble;
  uint seen;byte ack;bool launched;RoomTeleportPhase observed;AppRoot app;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){if(!Instance)new GameObject("Shared room teleport").AddComponent<RoomTeleport>();}
  void Awake(){Instance=this;}
  public static NetworkAthlete[] Players()=>FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).Where(n=>n.IsSpawned&&n.IsPlayerObject).OrderBy(n=>n.OwnerClientId).ToArray();
  public static string Roster()=>string.Join(",",Players().Select(n=>n.NetworkObjectId.ToString(System.Globalization.CultureInfo.InvariantCulture)));
  public bool CanRequest(SportId island,TeleportDestination destination,out string reason){
   app=AppRoot.Instance;reason="";
   if(!app||!app.Exploring){reason="Enter an island first.";return false;}
   if(app.rooms.Connected&&!app.rooms.Host){reason="Your host chooses where everyone goes.";return false;}
   if(!Enum.IsDefined(typeof(SportId),island)||!Enum.IsDefined(typeof(TeleportDestination),destination)){reason="Choose a destination.";return false;}
   if(Busy||TeleportSequence.Busy||SkySailWorld.Instance&&SkySailWorld.Instance.Travelling||app.environments.GetComponent<SkySailStreaming>().Busy){reason="Wait for the current journey to finish.";return false;}
   if(PostMatchFlow.MatchActive){reason="Finish the current match first.";return false;}
   int count=app.rooms.Connected?RoomSeats.PlayingCount:1;
   if(count<1||count>TeleportDestinations.Capacity(island)){reason=island+" holds "+TeleportDestinations.Capacity(island)+" players. Your "+count+" players stay together.";return false;}
   return true;
  }
  public void BeginEntry(){
   app=AppRoot.Instance;if(!app||!app.rooms.Host||!NetworkAthlete.HostPlayer)return;
   NetworkAthlete.HostPlayer.WorldTeleport.Value=new RoomTeleportState{entry=true,sequence=State.sequence+1,from=app.SelectedSport,to=app.rooms.Sport,destination=TeleportDestination.Game,phase=RoomTeleportPhase.Preparing,started=Clock,roster=new FixedString512Bytes(Roster())};
  }
  public bool Request(SportId island,TeleportDestination destination){
   if(!CanRequest(island,destination,out var why)){Message=why;return false;}
   PostMatchFlow.CloseResults();PostMatchFlow.Instance?.Close();FootballBots.Instance?.Clear();BasketballBots.Instance?.Clear();Message="";
   if(!app.rooms.Connected)return TeleportSequence.Ensure().StartPreview(island,destination,new[]{app.LocalAthlete});
   var s=new RoomTeleportState{sequence=State.sequence+1,from=app.SelectedSport,to=island,destination=destination,phase=RoomTeleportPhase.Preparing,started=Clock,roster=new FixedString512Bytes(Roster())};
   NetworkAthlete.HostPlayer.WorldTeleport.Value=s;return true;
  }
  public void Cancel(){if(app&&app.rooms.Host&&Active)Advance(RoomTeleportPhase.Cancelled,"The host cancelled. Everyone stays together.");}
  void Advance(RoomTeleportPhase phase,string reason="") {var s=State;s.phase=phase;s.started=Clock+(phase==RoomTeleportPhase.Departing?.4:0);s.reason=new FixedString128Bytes(reason);NetworkAthlete.HostPlayer.WorldTeleport.Value=s;}
  public bool AllowsCharge=>State.phase>=RoomTeleportPhase.Departing&&State.phase<=RoomTeleportPhase.Complete&&Clock>=State.started;
  public bool AllowsArrival=>State.phase is RoomTeleportPhase.Arriving or RoomTeleportPhase.Complete;
  public void Report(byte stage,bool failed=false){
   var local=Players().FirstOrDefault(n=>n.IsOwner);if(!local||ack>=stage&&!failed)return;
   ack=stage;local.TeleportReadyRpc(State.sequence,stage,failed);
  }
  void Update(){if(RoomService.Paused)return;
   app=AppRoot.Instance;if(!app)return;
   if(!app.rooms.Connected||!NetworkAthlete.HostPlayer){if(launched){TeleportSequence.Instance?.Cancel("The room connection closed.");launched=false;}seen=0;observed=RoomTeleportPhase.Idle;return;}
   var s=State;
   if(s.sequence!=seen&&Active){seen=s.sequence;ack=0;launched=false;Message="";}
   if(Active&&!launched&&s.sequence==seen&&app.Exploring&&!app.PendingSelection.HasValue&&!app.environments.GetComponent<SkySailStreaming>().Busy){
    if(Roster()==s.roster.ToString()){
     PostMatchFlow.CloseResults();PostMatchFlow.Instance?.Close();
     launched=TeleportSequence.Ensure().StartRoom(s.to,s.destination,Players().Select(n=>n.GetComponent<Athlete>()).ToArray(),s.entry,s.sequence);
     if(!launched)Report(1,true);
    }
   }
   if(app.rooms.Host&&Active){
    var people=Players();
    if(!NetworkAthlete.HostPlayer.Exploring.Value||Roster()!=s.roster.ToString())Advance(RoomTeleportPhase.Cancelled,"A player left. The group was restored.");
    else if(people.Any(n=>n.TeleportReady.Value.sequence==s.sequence&&n.TeleportReady.Value.failed))Advance(RoomTeleportPhase.Cancelled,"A destination could not be prepared. Please try again.");
    else if(Clock-s.started>90)Advance(RoomTeleportPhase.Cancelled,"A player took too long to prepare. Please try again.");
    else if(s.phase==RoomTeleportPhase.Preparing&&people.All(n=>n.TeleportReady.Value.sequence==s.sequence&&n.TeleportReady.Value.stage>=1))Advance(RoomTeleportPhase.Departing);
    else if(s.phase==RoomTeleportPhase.Departing&&people.All(n=>n.TeleportReady.Value.sequence==s.sequence&&n.TeleportReady.Value.stage>=2))Advance(RoomTeleportPhase.Arriving);
    else if(s.phase==RoomTeleportPhase.Arriving&&people.All(n=>n.TeleportReady.Value.sequence==s.sequence&&n.TeleportReady.Value.stage>=3)){
     RoomSeats.Normalize(s.to);NetworkAthlete.HostPlayer.WorldSport.Value=s.to;NetworkAthlete.HostPlayer.WorldAppearance.Value=JsonUtility.ToJson(LocalProfile.ForSport(s.to));app.rooms.SetTravelSport(s.to);Advance(RoomTeleportPhase.Complete);
    }
   }
   s=State;
   if(s.phase!=observed){observed=s.phase;
    if(s.phase==RoomTeleportPhase.Cancelled){Message=s.reason.ToString();if(launched)TeleportSequence.Instance?.Cancel(Message);launched=false;if(s.entry&&app.rooms.Host)_=app.rooms.SetExploring(false);}
    else if(s.phase==RoomTeleportPhase.Complete){if(launched)TeleportSequence.Instance?.FinishRoom();launched=false;Message="";}
   }
  }
  void OnDestroy(){if(Instance==this)Instance=null;}
 }
}
