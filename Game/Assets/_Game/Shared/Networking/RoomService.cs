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
  public const ushort ProtocolVersion=76;
  public ISession Session {get;private set;} public bool busy; public bool LocalTest;public SportId Sport {get;private set;}=SportId.Football; public string Error {get;private set;} public event Action Changed;
  public bool Connected=>!leaving&&(Recovering||NetworkManager.Singleton&&NetworkManager.Singleton.IsListening);
  public bool Host=>Connected&&!Recovering&&NetworkManager.Singleton.IsHost;
  public string Code=>Session?.Code??(LocalTest?"LOCAL TEST":"—");
  public int Capacity=>AppRoot.Instance.environments.Definition(Sport).maxPlayers;
  public string SeatMessage {get;set;}="";
  readonly Dictionary<ulong,byte> approvedSeats=new();
  public IEnumerable<byte> PendingSeats=>approvedSeats.Values;
  public byte ClaimSeat(ulong client){if(approvedSeats.Remove(client,out byte seat))return seat;return RoomSeats.Unassigned;}
  byte ReserveSeat(ulong client){
   var used=RoomSeats.Occupants.Select(n=>n.RoomSeat.Value).Concat(approvedSeats.Values).ToArray();
   for(byte seat=0;seat<20;seat++){if(seat>=Capacity&&seat<10||seat>=10&&!RoomSeats.Allowed||used.Contains(seat))continue;approvedSeats[client]=seat;return seat;}
   return RoomSeats.Unassigned;
  }
  bool leaving;
  public void SetTravelSport(SportId sport){if(Host)Sport=sport;}
  async Task Initialize(){await PlayerAccount.EnsureOnline();await PlayerProfiles.EnsureCloud();}
  public async Task Create(SportId sport=SportId.Football){if(Connected)return;AppRoot.Instance.SelectSport(sport);Sport=sport;await Run(async()=>{
   await Initialize();PrepareIdentity(AuthenticationService.Instance.PlayerId);
   Session=await MultiplayerService.Instance.CreateSessionAsync(new SessionOptions{MaxPlayers=20,IsPrivate=true,Name=Sport+" • "+LocalProfile.ForSport(Sport).title}.WithRelayNetwork().WithNetworkHandler(new RoomNetworkHandler(this)));
   Bind();
  });}
  public async Task Join(string code){if(Connected)return;await Run(async()=>{
   code=StorybookMenu.NormalizeCode(code);if(!StorybookMenu.ValidCode(code))throw new InvalidOperationException("Enter the room code shared by your host.");
   await Initialize();PrepareIdentity(AuthenticationService.Instance.PlayerId);Session=await MultiplayerService.Instance.JoinSessionByCodeAsync(code,new JoinSessionOptions().WithNetworkHandler(new RoomNetworkHandler(this)));Bind();
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
   if(!Host)throw new InvalidOperationException("Only the host can start exploration.");
   if(value&&FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).Any(p=>p.IsPlayerObject&&!p.Ready.Value))throw new InvalidOperationException("Wait until every player is ready.");
   if(value&&(!RoomSeats.Fits(Sport)||RoomSeats.PlayingCount==0))throw new InvalidOperationException("Choose at least one player place within this island capacity.");
   if(RoomTeleport.Busy||TeleportSequence.Busy)throw new InvalidOperationException("Wait for everyone to arrive first.");
   if(Session!=null){Session.AsHost().IsLocked=value;await Session.AsHost().SavePropertiesAsync();}
   var world=NetworkAthlete.HostPlayer;if(world){
    if(value){if(!RoomSeats.Fits(Sport)||RoomSeats.PlayingCount==0)throw new InvalidOperationException("Choose at least one player place within this island's capacity.");world.Exploring.Value=true;RoomTeleport.Instance.BeginEntry();}
    else{if(RoomTeleport.Busy||TeleportSequence.Busy)throw new InvalidOperationException("Wait for everyone to arrive first.");world.Exploring.Value=false;}
   }
  });}
  public void SetupNetwork(){
   var n=NetworkManager.Singleton;
   NetworkResponsiveness.Configure(n.NetworkConfig.PlayerPrefab.GetComponent<Unity.Netcode.Components.NetworkTransform>());
   // Twenty peers can publish a full roster in one tick; the old 128-packet queue dropped admission messages.
   n.GetComponent<UnityTransport>().MaxPacketQueueSize=1024;
   n.NetworkConfig.ProtocolVersion=ProtocolVersion;n.NetworkConfig.ConnectionApproval=true;
   n.ConnectionApprovalCallback=(request,response)=>{
    if(ApproveReturning(request,response))return;
    bool running=Recovering||NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.Exploring.Value;bool full=!running&&ReserveSeat(request.ClientNetworkId)==RoomSeats.Unassigned;
    response.Approved=!full&&!running;response.CreatePlayerObject=response.Approved;
    response.Reason=full?"This room has no open places ("+Capacity+" players and up to 10 spectators).":running?"Exploration has started. Ask the host to return to the waiting room.":"";
    response.Position=AppRoot.Instance.environments.Definition(Sport).Spawn(n.ConnectedClients.Count);response.Rotation=Quaternion.identity;
   };
   n.OnClientDisconnectCallback+=id=>{
    MemberDisconnected(id);
    approvedSeats.Remove(id);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    Debug.Log("ROOM_DISCONNECT client="+id+" local="+n.LocalClientId+" server="+n.IsServer+" event="+n.DisconnectEvent+" reason="+n.DisconnectReason);
#endif
    if(!leaving&&(!n.IsServer||id==n.LocalClientId)){if(checkpoint!=null){BeginRecovery();}else if(!Recovering){string reason=n.DisconnectReason;if(string.IsNullOrEmpty(reason)||reason.StartsWith("[Disconnect")||reason.StartsWith("Client-")&&reason.Contains("disconnected by server"))reason="Could not connect. Check the room code and connection, and use the same game build as your host (network "+ProtocolVersion+").";_=ExitWithMessage(reason);}}
   };
  }
  public void StartLocal(bool host,ushort port=7777){
   Sport=AppRoot.Instance.SelectedSport;LocalTest=true;localPort=port;PrepareIdentity(Guid.NewGuid().ToString("N"));NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1",port,"127.0.0.1");
   if(host)NetworkManager.Singleton.StartHost();else NetworkManager.Singleton.StartClient();Changed?.Invoke();
  }
  public async Task Leave(){if(leaving)return;bool wasHost=Host;leaving=true;var session=Session;Session=null;EndRecovery();Changed?.Invoke();
   if(session!=null){session.Deleted-=Closed;session.RemovedFromSession-=Closed;session.SessionHostChanged-=HostChanged;try{if(wasHost&&session.Players.Count>1){var next=Successor(session.Players.Select(p=>p.Id),memberId);if(next!=null){session.AsHost().Host=next.member;await session.AsHost().SavePropertiesAsync();}}if(wasHost&&session.Players.Count<=1)await session.AsHost().DeleteAsync();else await session.LeaveAsync();}catch(Exception e){Debug.LogWarning(e.Message);}}
   if(NetworkManager.Singleton){NetworkManager.Singleton.Shutdown();while(NetworkManager.Singleton&&NetworkManager.Singleton.IsListening)await Task.Yield();}approvedSeats.Clear();approvedMembers.Clear();checkpoint=null;LocalTest=false;leaving=false;Changed?.Invoke();
  }
  async Task ExitWithMessage(string message){await Leave();Error=message;Changed?.Invoke();}
  public async Task Run(Func<Task> action){if(busy)return;busy=true;Error=null;Changed?.Invoke();try{await action();}catch(Exception e){Debug.LogException(e);Error=e.Message;if(!Connected&&Session!=null)await Leave();}finally{busy=false;Changed?.Invoke();}}
 }
}
