using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace WhatTheFish {
 public interface IRoomService { Task Create(SportId sport=SportId.Football); Task Join(string code); Task Leave(); }
 public sealed partial class RoomService:MonoBehaviour,IRoomService {
  // Combined recovery-clock and Golf hazard schemas require a new wire version.
  public const ushort ProtocolVersion=87;
  public ISession Session {get;private set;} public bool busy; public bool LocalTest;public SportId Sport {get;private set;}=SportId.Football; public string Error {get;private set;} public event Action Changed;
  public bool Connected=>!leaving&&!cancelConnection&&(Recovering||GuestRecovering||NetworkManager.Singleton&&NetworkManager.Singleton.IsListening);
  public bool Host=>Connected&&!Recovering&&NetworkManager.Singleton.IsHost;
  public string Code=>Session?.Code??(LocalTest?"LOCAL TEST":"—");
  public int Capacity=>AppRoot.Instance.environments.Definition(Sport).maxPlayers;
  public string SeatMessage {get;set;}="";
  readonly Dictionary<ulong,byte> approvedSeats=new();
  public IEnumerable<byte> PendingSeats=>approvedSeats.Values.Concat(missingGuests.Values.Select(p=>p.seat));
  public byte ClaimSeat(ulong client){if(approvedSeats.Remove(client,out byte seat))return seat;return RoomSeats.Unassigned;}
  byte ReserveSeat(ulong client){
   var used=RoomSeats.Occupants.Select(n=>n.RoomSeat.Value).Concat(approvedSeats.Values).ToArray();
   for(byte seat=0;seat<20;seat++){if(seat>=Capacity&&seat<10||seat>=10&&!RoomSeats.Allowed||used.Contains(seat))continue;approvedSeats[client]=seat;return seat;}
   return RoomSeats.Unassigned;
  }
  bool leaving,connectionOperation,cancelConnection;
  public string Operation {get;private set;}="";
  public bool CanCancelConnection=>busy&&connectionOperation&&!cancelConnection;
  public void CancelConnection(){if(!CanCancelConnection)return;cancelConnection=true;Operation="Cancelling · closing the room connection…";Changed?.Invoke();}
  void CheckConnection(){if(cancelConnection)throw new OperationCanceledException();}
  public void DismissError(){Error=null;Changed?.Invoke();}
  public void ReportError(string value){Error=value;Changed?.Invoke();}
  void Stage(string value){CheckConnection();Operation=value;Changed?.Invoke();}
  async Task ConnectOperation(Func<Task> action){if(busy||Connected)return;connectionOperation=true;cancelConnection=false;Operation="Signing in to online services…";try{await Run(action);}finally{connectionOperation=false;cancelConnection=false;Operation="";Changed?.Invoke();}}

  public void SetTravelSport(SportId sport){if(Host)Sport=sport;}
  public bool TrySetLobbySport(SportId sport,out string reason){
   reason="";var world=NetworkAthlete.HostPlayer;var app=AppRoot.Instance;
   if(!Host||!world){reason="Only the host can change the room's spot.";return false;}
   if(busy||Paused||world.Exploring.Value||RoomTeleport.Busy||TeleportSequence.Busy||app.PendingSelection.HasValue){reason="Wait until the room is ready before changing spots.";return false;}
   if(!Enum.IsDefined(typeof(SportId),sport)||!app.environments.Definition(sport)||!app.environments.Definition(sport).available){reason="This spot is unavailable.";return false;}
   if(sport==world.WorldSport.Value)return true;
   if(approvedSeats.Count>0){reason="Someone is joining. Try changing spots again in a moment.";return false;}
   if(!RoomSeats.Fits(sport)){reason=sport+" has "+TeleportDestinations.Capacity(sport)+" player places. Ask extra players to choose Spectators first.";return false;}
   // Publish the shared destination together; existing clients load it through AppRoot.
   // Keep the same session, invitation and roster, and require guests to acknowledge it.
   Sport=sport;RoomSeats.Normalize(sport);
   foreach(var player in RoomTeleport.Players())player.Ready.Value=false;
   world.WorldAppearance.Value=JsonUtility.ToJson(LocalProfile.ForSport(sport));
   world.WorldSport.Value=sport;SeatMessage="";Error=null;Changed?.Invoke();return true;
  }
  public static bool LobbyReady=>NetworkAthlete.HostPlayer&&RoomTeleport.Players().All(p=>p==NetworkAthlete.HostPlayer||p.Ready.Value);
  async Task Initialize(){await PlayerAccount.EnsureOnline();await PlayerProfiles.EnsureCloud();}
  public async Task Create(SportId sport=SportId.Football){if(Connected||busy)return;AppRoot.Instance.SelectSport(sport);Sport=sport;await ConnectOperation(async()=>{
   await Initialize();Stage("Creating your private room…");PrepareIdentity(AuthenticationService.Instance.PlayerId);
   Session=await MultiplayerService.Instance.CreateSessionAsync(new SessionOptions{MaxPlayers=20,IsPrivate=true,Name=Sport+" • "+LocalProfile.ForSport(Sport).title}.WithRelayNetwork().WithNetworkHandler(new RoomNetworkHandler(this)));
   CheckConnection();Bind();
  });}
  public async Task Join(string code){if(Connected||busy)return;await ConnectOperation(async()=>{
   code=StorybookMenu.NormalizeCode(code);if(!StorybookMenu.ValidCode(code))throw new InvalidOperationException("Enter the room code shared by your host.");
   await Initialize();Stage("Finding the room…");PrepareIdentity(AuthenticationService.Instance.PlayerId);Session=await MultiplayerService.Instance.JoinSessionByCodeAsync(code,new JoinSessionOptions().WithNetworkHandler(new RoomNetworkHandler(this)));CheckConnection();Bind();
  });}
  void Bind(){Session.Deleted+=Closed;Session.RemovedFromSession+=Closed;Session.SessionHostChanged+=HostChanged;Changed?.Invoke();}
  void HostChanged(string id){
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   Debug.Log("ROOM_HOST_CHANGED time="+Time.realtimeSinceStartup+" service="+id+" game="+(NetworkAthlete.HostPlayer?NetworkAthlete.HostPlayer.MemberId.Value.ToString():"none")+" recovering="+Recovering);
#endif
   // Session events can arrive after the replacement game connection is ready.
   if(!leaving&&!(NetworkManager.Singleton.IsConnectedClient&&NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.MemberId.Value.ToString()==id)){BeginRecovery();_=ElectOnlineHost();}
  }
  void Closed(){if(!leaving)_=ExitWithMessage("This room is no longer available.");}
  public async Task SetExploring(bool value){await Run(async()=>{
   if(Paused)throw new InvalidOperationException("Wait for your crew to reconnect first.");
   if(!Host)throw new InvalidOperationException("Only the host can start exploration.");
   if(value&&!LobbyReady)throw new InvalidOperationException("Wait until the other players and spectators are ready.");
   if(value&&(!RoomSeats.Fits(Sport)||RoomSeats.PlayingCount==0))throw new InvalidOperationException("Choose at least one player place within this island capacity.");
   if(RoomTeleport.Busy||TeleportSequence.Busy)throw new InvalidOperationException("Wait for everyone to arrive first.");
   if(Session!=null){Session.AsHost().IsLocked=value;await Session.AsHost().SavePropertiesAsync();}
   var world=NetworkAthlete.HostPlayer;if(world){
    if(value){if(!LobbyReady||!RoomSeats.Fits(Sport)||RoomSeats.PlayingCount==0){if(Session!=null){Session.AsHost().IsLocked=false;await Session.AsHost().SavePropertiesAsync();}throw new InvalidOperationException("The crew changed. Check player places and wait until everyone else is ready.");}world.Exploring.Value=true;RoomTeleport.Instance.BeginEntry();}
    else{if(RoomTeleport.Busy||TeleportSequence.Busy)throw new InvalidOperationException("Wait for everyone to arrive first.");world.Exploring.Value=false;}
   }
  });}
  public void SetupNetwork(){RoomChatService.Ensure(this);RoomInvitations.Ensure(this);RoomVoiceService.Ensure(this);
   var n=NetworkManager.Singleton;
   NetworkResponsiveness.Configure(n.NetworkConfig.PlayerPrefab.GetComponent<Unity.Netcode.Components.NetworkTransform>());
   // Twenty peers can publish a full roster in one tick; the old 128-packet queue dropped admission messages.
   n.GetComponent<UnityTransport>().MaxPacketQueueSize=1024;
   n.NetworkConfig.ProtocolVersion=ProtocolVersion;n.NetworkConfig.ConnectionApproval=true;
   n.ConnectionApprovalCallback=(request,response)=>{
    if(ApproveReturning(request,response)){if(!response.Approved)approvedMembers.Remove(request.ClientNetworkId);return;}
    bool running=Recovering||NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.Exploring.Value;bool full=!running&&ReserveSeat(request.ClientNetworkId)==RoomSeats.Unassigned;
    response.Approved=!full&&!running;response.CreatePlayerObject=response.Approved;
    response.Reason=full?"This room has no open places ("+Capacity+" players and up to 10 spectators).":running?"Exploration has started. Ask the host to return to the waiting room.":"";
    if(!response.Approved)approvedMembers.Remove(request.ClientNetworkId);
    response.Position=AppRoot.Instance.environments.Definition(Sport).Spawn(n.ConnectedClients.Count);response.Rotation=Quaternion.identity;
   };
   n.OnClientDisconnectCallback+=id=>{
    MemberDisconnected(id);
    approvedSeats.Remove(id);
    approvedMembers.Remove(id);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    Debug.Log("ROOM_DISCONNECT client="+id+" local="+n.LocalClientId+" server="+n.IsServer+" event="+n.DisconnectEvent+" reason="+n.DisconnectReason);
#endif
    if(!leaving&&!cancelConnection&&(!n.IsServer||id==n.LocalClientId)){if(checkpoint!=null){if(!BeginGuestRetry()&&!Recovering)BeginRecovery();}else if(!Recovering){string reason=n.DisconnectReason;if(string.IsNullOrEmpty(reason)||reason.StartsWith("[Disconnect")||reason.StartsWith("Client-")&&reason.Contains("disconnected by server"))reason="Could not connect. Check the room code and connection, and use the same game build as your host (network "+ProtocolVersion+").";if(connectionOperation)Error=reason;else _=ExitWithMessage(reason);}}
   };
  }
  public void StartLocal(bool host,ushort port=7777){
   Sport=AppRoot.Instance.SelectedSport;LocalTest=true;localPort=port;PrepareIdentity(Guid.NewGuid().ToString("N"));NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1",port,"127.0.0.1");
   if(host)NetworkManager.Singleton.StartHost();else NetworkManager.Singleton.StartClient();Changed?.Invoke();
  }
  internal bool LeaveAcknowledged;
  public async Task Leave(){if(leaving)return;GetComponent<RoomVoiceService>()?.Leave();bool wasHost=Session?.IsHost??Host;leaving=true;
   var owner=RoomTeleport.Players().FirstOrDefault(p=>p.IsOwner);
   if(!wasHost&&owner&&NetworkManager.Singleton.IsConnectedClient){LeaveAcknowledged=false;owner.LeavingRoomRpc();float end=Time.realtimeSinceStartup+1.5f;while(!LeaveAcknowledged&&Time.realtimeSinceStartup<end&&NetworkManager.Singleton.IsConnectedClient)await Task.Yield();}
   ClearGuestRecovery();rejoinSecret="";var session=Session;Session=null;EndRecovery();Changed?.Invoke();
   if(session!=null){session.Deleted-=Closed;session.RemovedFromSession-=Closed;session.SessionHostChanged-=HostChanged;try{if(wasHost&&session.Players.Count>1){var next=Successor(session.Players.Select(p=>p.Id),memberId);if(next!=null){session.AsHost().Host=next.member;await session.AsHost().SavePropertiesAsync();}}if(wasHost&&session.Players.Count<=1)await session.AsHost().DeleteAsync();else await session.LeaveAsync();}catch(Exception e){Debug.LogWarning(e.Message);}}
   if(NetworkManager.Singleton){NetworkManager.Singleton.Shutdown();while(NetworkManager.Singleton&&NetworkManager.Singleton.IsListening)await Task.Yield();}approvedSeats.Clear();approvedMembers.Clear();checkpoint=null;GetComponent<RoomChatService>()?.ResetRoom();LocalTest=false;leaving=false;Changed?.Invoke();
  }
  async Task ExitWithMessage(string message){await Leave();Error=message;Changed?.Invoke();}
  public async Task Run(Func<Task> action){
   if(busy)return;busy=true;Error=null;Changed?.Invoke();
   try{await action();}catch(OperationCanceledException){if(connectionOperation)await Leave();Error=null;}
   catch(Exception e){Debug.LogWarning("Room operation: "+e.Message);if(!cancelConnection&&string.IsNullOrEmpty(Error))Error=e.Message;if(connectionOperation||!Connected&&Session!=null)await Leave();}
   finally{if(connectionOperation&&cancelConnection){await Leave();Error=null;}busy=false;Changed?.Invoke();}
  }
 }
}
