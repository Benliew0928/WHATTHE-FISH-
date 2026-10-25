using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public sealed partial class RoomService {
  public const float GuestGraceSeconds=30;
  readonly HashSet<string> voluntaryDepartures=new();
  readonly Dictionary<string,MemberCheckpoint> missingGuests=new();
  bool guestPaused,guestRetrying,guestHolding;float guestUntil,guestScale=1;uint guestAttempt;
  string rejoinSecret="";RoomCheckpoint guestCheckpoint;GuestRecoveryUI guestUI;double guestPausedClock;
  // Gameplay pauses independently of NGO's clock, so network ticks, countdown
  // replication and acknowledgements keep running for every connected peer.
  public static double GameClock(double networkTime){
   var room=AppRoot.Instance?AppRoot.Instance.rooms:null;if(room&&room.guestPaused)return room.guestPausedClock;
   var host=NetworkAthlete.HostPlayer;return host?host.GuestSeconds.Value>0?host.GuestClock.Value:networkTime-host.GameClockOffset.Value:networkTime;
  }
  public bool GuestRecovering=>guestPaused||guestRetrying||guestHolding;
  public float GuestSeconds=>guestHolding||guestRetrying?Mathf.Max(0,guestUntil-Time.realtimeSinceStartup):NetworkAthlete.HostPlayer?NetworkAthlete.HostPlayer.GuestSeconds.Value:0;
  public string GuestNames=>NetworkAthlete.HostPlayer?NetworkAthlete.HostPlayer.GuestNames.Value.ToString():"";
  public bool RejoiningSelf=>guestRetrying;
  internal static string RejoinHash(string secret){using var sha=SHA256.Create();return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(secret??""))).Replace("-","");}
  internal void IssueRejoinKey(NetworkAthlete actor){
   using var rng=RandomNumberGenerator.Create();var bytes=new byte[32];rng.GetBytes(bytes);
   string key=BitConverter.ToString(bytes).Replace("-","");actor.RejoinSecret.Value=key;actor.RejoinDigest.Value=RejoinHash(key);
  }
  void CacheRejoinKey(){var own=RoomTeleport.Players().FirstOrDefault(p=>p.IsOwner);if(own&&own.RejoinSecret.Value.Length==64)rejoinSecret=own.RejoinSecret.Value.ToString();}
  internal void VoluntaryDeparture(NetworkAthlete actor){if(actor&&actor.IsServer&&actor.IsPlayerObject)voluntaryDepartures.Add(actor.MemberId.Value.ToString());}
  internal void GuestDespawned(NetworkAthlete actor){
   if(!actor.IsServer||!actor.IsPlayerObject||actor==NetworkAthlete.HostPlayer||leaving||Recovering||!NetworkAthlete.HostPlayer)return;
   string id=actor.MemberId.Value.ToString();
   if(voluntaryDepartures.Remove(id)||string.IsNullOrEmpty(id)||actor.RejoinDigest.Value.Length!=64||!LocalTest&&actor.ProfileId.Value.Length==0)return;
   if(RoomTeleport.Busy||TeleportSequence.Busy||SkySailWorld.Instance&&SkySailWorld.Instance.Travelling){
    // A journey roster contains connection object IDs. Settle the crew in the
    // waiting room instead of continuing a half-completed journey with stale IDs.
    TeleportSequence.Instance?.Cancel();SkySailWorld.Instance?.AbortForRecovery();
    var world=NetworkAthlete.HostPlayer;var travel=world.WorldTravel.Value;travel.phase=SkySailPhase.Idle;world.WorldTravel.Value=travel;
    var teleport=world.WorldTeleport.Value;teleport.phase=RoomTeleportPhase.Cancelled;teleport.reason="A crew member lost connection. Ready up together after reconnecting.";world.WorldTeleport.Value=teleport;world.Exploring.Value=false;
    foreach(var p in RoomTeleport.Players())p.GetComponent<Athlete>().inTransit=false;
    if(Session?.IsHost==true)_=UnlockAfterInterruptedJourney();
   }
   // Capture before NGO destroys the player and before a sports FixedUpdate can void the roster.
   if(!guestHolding){
    var host=NetworkAthlete.HostPlayer;
    guestCheckpoint=new RoomCheckpoint{host=memberId,clock=NetworkManager.Singleton.ServerTime.Time,sport=Sport,world=host.CaptureFields(true)};
    foreach(var p in RoomTeleport.Players().Append(actor).Distinct())guestCheckpoint.members[p.MemberId.Value.ToString()]=SaveMember(p);
    host.GuestClock.Value=GameClock(guestCheckpoint.clock);
    checkpoint=guestCheckpoint;guestUntil=Time.realtimeSinceStartup+GuestGraceSeconds;guestHolding=true;
   }
   if(!guestCheckpoint.members.TryGetValue(id,out var saved)){saved=SaveMember(actor);guestCheckpoint.members[id]=saved;}
   missingGuests[id]=saved;SetGuestPause(true);PublishGuestWait();
  }
  MemberCheckpoint SaveMember(NetworkAthlete p)=>new(){member=p.MemberId.Value.ToString(),name=p.Nickname.Value.ToString(),seat=p.RoomSeat.Value,id=p.StableId,position=p.transform.position,rotation=p.transform.rotation,fields=p.CaptureFields(false),rejoinDigest=p.RejoinDigest.Value.ToString()};
  bool ApproveGuest(string id,string key,ulong client,NetworkManager.ConnectionApprovalResponse response){
   if(!guestHolding||!missingGuests.TryGetValue(id,out var saved))return false;
   response.Approved=Time.realtimeSinceStartup<guestUntil&&key.Length==64&&RejoinHash(key)==saved.rejoinDigest;
   response.CreatePlayerObject=response.Approved;response.Reason=response.Approved?"":"Your reserved place could not be verified. Rejoin from the waiting room.";
   if(response.Approved){approvedSeats[client]=saved.seat;response.Position=saved.position;response.Rotation=saved.rotation;}
   return true;
  }
  bool RestoreGuest(NetworkAthlete actor,string id){
   if(!guestHolding||!missingGuests.TryGetValue(id,out var saved))return false;
   // Keep the existing room key through repeated interruptions. Rotating it here
   // can strand a client that drops again before receiving the replacement key.
   actor.RestoreFields(saved.fields,false);actor.RestorePosition(saved.position,saved.rotation);return true;
  }
  void PublishGuestWait(){var host=NetworkAthlete.HostPlayer;if(!host)return;host.GuestSeconds.Value=GuestSeconds;host.GuestNames.Value=(missingGuests.Values.FirstOrDefault()?.name??"A crew member")+(missingGuests.Count>1?" + "+(missingGuests.Count-1)+" more":"");}
  bool TickGuestRecovery(){
   CacheRejoinKey();var host=NetworkAthlete.HostPlayer;
   if(guestHolding&&host){
    foreach(var id in missingGuests.Keys.ToArray()){
     var actor=RoomTeleport.Players().FirstOrDefault(p=>p.MemberId.Value.ToString()==id);
     if(actor&&actor.RecoveryAcknowledged.Value&&(LocalTest||actor.ProfileId.Value.ToString()==id))missingGuests.Remove(id);
    }
    if(missingGuests.Count==0||Time.realtimeSinceStartup>=guestUntil){
     bool expired=missingGuests.Count>0;
     foreach(var id in missingGuests.Keys)departedMembers.Add(id);
     // Rebind stable participant IDs to the new network objects. Match clocks stayed frozen.
     double offset=host.GameClockOffset.Value+NetworkManager.Singleton.ServerTime.Time-guestCheckpoint.clock;
     host.RestoreFields(guestCheckpoint.world,true);host.GameClockOffset.Value=offset;
     RestoreGames();if(expired)CancelIncompleteGames(guestCheckpoint);
     host.GuestSeconds.Value=0;host.GuestNames.Value=default;guestHolding=false;missingGuests.Clear();guestCheckpoint=null;
     SetGuestPause(false);observedHeartbeat=-1;nextCheckpoint=0;
     if(expired)SeatMessage="Reconnection time ended. Your crew can start a new round.";
     Changed?.Invoke();return false;
    }
    PublishGuestWait();return true;
   }
   if(!guestRetrying&&!Recovering)SetGuestPause(host&&host.GuestSeconds.Value>0);
   return guestPaused||guestRetrying;
  }
  static void CancelIncompleteGames(RoomCheckpoint saved,string ignored=null){
   var room=AppRoot.Instance.rooms;
   var present=RoomTeleport.Players().Where(p=>p.RecoveryAcknowledged.Value&&(room.LocalTest||p.ProfileId.Value.Length>0)).Select(p=>p.MemberId.Value.ToString()).ToHashSet();
   if(!saved.members.Values.Any(p=>p.seat<10&&p.member!=ignored&&!present.Contains(p.member)))return;
   FootballMatch.Instance?.CancelMatch();BasketballMatch.Instance?.Cancel();
   if(FishingGame.Instance)FishingGame.Instance.State.Abandon(RoomTeleport.Clock);
   // Golf's existing departure processing marks the absent player's round DNF.
  }
  void SetGuestPause(bool value){
   if(guestPaused==value)return;if(value)guestPausedClock=GameClock(NetworkManager.Singleton&&NetworkManager.Singleton.IsListening?NetworkManager.Singleton.ServerTime.Time:Time.unscaledTimeAsDouble);guestPaused=value;
   if(value){guestScale=Time.timeScale;Time.timeScale=0;PlayerView.Instance?.ClearMatchInput();guestUI=GuestRecoveryUI.Create(this);}
   else{Time.timeScale=guestScale;if(guestUI)Destroy(guestUI.gameObject);guestUI=null;}
  }
  bool BeginGuestRetry(){
   if(leaving||cancelConnection||connectionOperation||Recovering||checkpoint==null||rejoinSecret.Length!=64)return false;
   if(guestRetrying)return true;
   guestRetrying=true;guestUntil=Time.realtimeSinceStartup+GuestGraceSeconds;SetGuestPause(true);_=RetryGuest(++guestAttempt);return true;
  }
  async Task RetryGuest(uint attempt){
   try{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    if(ReviewRetryDelay>0)await Task.Delay(Mathf.CeilToInt(ReviewRetryDelay*1000));
#endif
    while(!leaving&&guestRetrying&&attempt==guestAttempt&&Time.realtimeSinceStartup<guestUntil){
     await StopTransport();if(leaving||!guestRetrying||attempt!=guestAttempt)return;
     SetPayload();var net=NetworkManager.Singleton;net.StartClient();float until=Time.realtimeSinceStartup+4;
     while(!leaving&&guestRetrying&&attempt==guestAttempt&&Time.realtimeSinceStartup<until&&!net.IsConnectedClient&&net.IsListening)await Task.Yield();
     if(net.IsConnectedClient){
      while(!leaving&&guestRetrying&&attempt==guestAttempt&&net.IsConnectedClient&&Time.realtimeSinceStartup<guestUntil){
       var own=RoomTeleport.Players().FirstOrDefault(p=>p.IsOwner);var host=NetworkAthlete.HostPlayer;
       if(own&&host&&host.MigrationReady.Value&&host.GuestSeconds.Value<=0&&(LocalTest||own.ProfileId.Value.Length>0)){guestRetrying=false;SetGuestPause(false);Changed?.Invoke();return;}
       await Task.Yield();
      }
     }
     await Task.Delay(700);
    }
    if(!leaving&&guestRetrying&&attempt==guestAttempt){
     guestRetrying=false;SetGuestPause(false);
     if(LocalTest){BeginRecovery();}else await ExitWithMessage("Your reconnection time ended. Ask your host to return to the waiting room, then join again.");
    }
   }catch(Exception){if(!leaving&&attempt==guestAttempt){guestRetrying=false;SetGuestPause(false);await ExitWithMessage("Could not reconnect to your crew. Check your connection and join again.");}}
  }
  void ClearGuestRecovery(){guestAttempt++;guestRetrying=guestHolding=false;missingGuests.Clear();guestCheckpoint=null;SetGuestPause(false);voluntaryDepartures.Clear();}
  async Task UnlockAfterInterruptedJourney(){try{var session=Session;if(session?.IsHost==true){session.AsHost().IsLocked=false;await session.AsHost().SavePropertiesAsync();}}catch(Exception){SeatMessage="The journey stopped. Return to the waiting room before inviting friends.";}}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  internal float ReviewRetryDelay;
#endif
 }
 public sealed partial class NetworkAthlete {
  public NetworkVariable<FixedString128Bytes> RejoinSecret=new(default,NetworkVariableReadPermission.Owner);
  public NetworkVariable<FixedString128Bytes> RejoinDigest=new();
  public NetworkVariable<float> GuestSeconds=new();
  public NetworkVariable<double> GuestClock=new();
  public NetworkVariable<double> GameClockOffset=new();
  public NetworkVariable<FixedString512Bytes> GuestNames=new();
  [Rpc(SendTo.Server,InvokePermission=RpcInvokePermission.Owner)] public void LeavingRoomRpc(){AppRoot.Instance.rooms.VoluntaryDeparture(this);LeavingAcknowledgedRpc();}
  [Rpc(SendTo.Owner,InvokePermission=RpcInvokePermission.Server)] void LeavingAcknowledgedRpc(){AppRoot.Instance.rooms.LeaveAcknowledged=true;}
 }
}
