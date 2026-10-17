using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Every peer keeps the last replicated checkpoint. Lobby ownership fences the
 // online election; the elected owner creates a new Relay allocation for NGO.
 public sealed partial class RoomService {
  public bool Recovering {get;private set;}
  public static bool Paused=>AppRoot.Instance&&AppRoot.Instance.rooms&&AppRoot.Instance.rooms.Recovering;
  public string RecoveryMessage {get;private set;}
  public string MemberId=>memberId;
  string memberId;ushort localPort;float nextCheckpoint,recoveryStarted,nextRefresh,previousTimeScale=1;
  double observedHeartbeat=-1;bool electing,localRecovery;uint recoveryVersion;
  Canvas recoveryCanvas;Text recoveryText;
  readonly Dictionary<ulong,string> approvedMembers=new();
  readonly Dictionary<ulong,string> connectedMembers=new();
  readonly HashSet<string> departedMembers=new();
  bool pruning;float nextPrune;
  readonly HashSet<string> excludedMembers=new();
  RoomCheckpoint checkpoint;
  internal sealed class RoomCheckpoint {
   public string host;public double clock;public SportId sport;public byte[] world;
   public readonly Dictionary<string,MemberCheckpoint> members=new();
  }
  internal sealed class MemberCheckpoint {
   public string member;public byte seat;public ulong id;public Vector3 position;public Quaternion rotation;public byte[] fields;
  }
  void PrepareIdentity(string id){memberId=id;checkpoint=null;excludedMembers.Clear();departedMembers.Clear();connectedMembers.Clear();observedHeartbeat=-1;SetPayload();}
  void SetPayload(){NetworkManager.Singleton.NetworkConfig.ConnectionData=Encoding.UTF8.GetBytes(memberId??"");}
  bool ApproveReturning(NetworkManager.ConnectionApprovalRequest request,NetworkManager.ConnectionApprovalResponse response){
   string member=Encoding.UTF8.GetString(request.Payload??Array.Empty<byte>());
   if(string.IsNullOrEmpty(member)||member.Length>100||RoomTeleport.Players().Any(p=>p.MemberId.Value.ToString()==member)){
    response.Approved=false;response.Reason="Invalid or duplicate room identity.";return true;
   }
   approvedMembers[request.ClientNetworkId]=member;
   if(!Recovering)return false;
   if(checkpoint!=null&&checkpoint.members.TryGetValue(member,out var saved)&&!excludedMembers.Contains(member)){
    approvedSeats[request.ClientNetworkId]=saved.seat;response.Approved=true;response.CreatePlayerObject=true;response.Position=saved.position;response.Rotation=saved.rotation;
   }else{response.Approved=false;response.Reason="The room is reconnecting. Please try again shortly.";}
   return true;
  }
  internal bool RestoreMember(NetworkAthlete actor){
   string id=approvedMembers.Remove(actor.OwnerClientId,out var approved)?approved:memberId;
   connectedMembers[actor.OwnerClientId]=id;
   actor.MemberId.Value=id;
   if(Recovering&&checkpoint!=null&&checkpoint.members.TryGetValue(id,out var saved)){
    actor.RestoreFields(saved.fields,false);actor.RestorePosition(saved.position,saved.rotation);
    if(actor.IsOwner){actor.RestoreFields(checkpoint.world,true);actor.MigrationReady.Value=false;SetServerClock(checkpoint.clock);}
    return true;
   }
   // Preserve the high-water mark even if the highest numbered participant left
   // before this host migration. Their statistics may still reference that ID.
   actor.ParticipantId.Value=NetworkAthlete.HostPlayer.NextParticipantId.Value++;return false;
  }
  void LateUpdate(){
   var n=NetworkManager.Singleton;var host=NetworkAthlete.HostPlayer;
   if(leaving||!n)return;
   if(!Recovering&&n.IsServer&&Session?.IsHost==true&&departedMembers.Count>0&&!pruning&&Time.realtimeSinceStartup>=nextPrune)_=PruneDepartedMembers();
   if(Recovering){
    if(recoveryText)recoveryText.text="Keeping your room together\n\n"+RecoveryMessage;
    if(n.IsServer&&host){
     SetServerClock(checkpoint.clock);
     var connected=RoomTeleport.Players().Where(p=>p.RecoveryAcknowledged.Value).Select(p=>p.MemberId.Value.ToString()).ToHashSet();
     var expected=checkpoint.members.Keys.Where(id=>!excludedMembers.Contains(id));
     if(expected.All(connected.Contains)||Time.realtimeSinceStartup-recoveryStarted>65){
      RestoreGames();host.MigrationReady.Value=true;host.RoomHeartbeat.Value=checkpoint.clock+.001;EndRecovery();
     }
    }else if(host&&host.IsSpawned&&RoomTeleport.Players().Any(p=>p.IsOwner)&&host.MigrationReady.Value&&host.MemberId.Value.ToString()!=checkpoint?.host){EndRecovery();}
    if(Recovering&&!LocalTest&&Session!=null&&Time.realtimeSinceStartup>nextRefresh){nextRefresh=Time.realtimeSinceStartup+4;_=RefreshElection();}
    if(Recovering&&Time.realtimeSinceStartup-recoveryStarted>150)_=ExitWithMessage("The room could not reconnect. Please check your connection and rejoin.");
    return;
   }
   if(!host||!host.IsSpawned||RoomTeleport.Busy||TeleportSequence.Busy||SkySailWorld.Instance&&SkySailWorld.Instance.Travelling||Time.realtimeSinceStartup<nextCheckpoint)return;
   nextCheckpoint=Time.realtimeSinceStartup+.5f;
   if(n.IsServer)host.RoomHeartbeat.Value=n.ServerTime.Time;
   if(host.RoomHeartbeat.Value==observedHeartbeat)return;
   observedHeartbeat=host.RoomHeartbeat.Value;
   var saved=new RoomCheckpoint{host=host.MemberId.Value.ToString(),clock=observedHeartbeat,sport=host.WorldSport.Value,world=host.CaptureFields(true)};
   foreach(var actor in RoomTeleport.Players())if(actor.MemberId.Value.Length>0)saved.members[actor.MemberId.Value.ToString()]=new MemberCheckpoint{member=actor.MemberId.Value.ToString(),seat=actor.RoomSeat.Value,id=actor.StableId,position=actor.transform.position,rotation=actor.transform.rotation,fields=actor.CaptureFields(false)};
   if(saved.members.ContainsKey(memberId))checkpoint=saved;
  }
  internal void HostDespawned(){if(!leaving&&!Recovering&&checkpoint!=null)BeginRecovery();}
  void MemberDisconnected(ulong client){
   if(connectedMembers.Remove(client,out var id)&&!leaving&&!Recovering&&NetworkManager.Singleton.IsServer&&id!=memberId)departedMembers.Add(id);
  }
  async Task PruneDepartedMembers(){
   pruning=true;var session=Session;
   try{
    foreach(var id in departedMembers.ToArray()){
     if(leaving||Session!=session||!session.IsHost)break;
     if(session.Players.Any(p=>p.Id==id))await session.AsHost().RemovePlayerAsync(id);
     departedMembers.Remove(id);
    }
   }catch(Exception e){Debug.LogWarning("Room membership cleanup will retry: "+e.Message);}
   finally{pruning=false;nextPrune=Time.realtimeSinceStartup+4;}
  }
  void BeginRecovery(){
   if(leaving||Recovering||checkpoint==null)return;
   Recovering=true;recoveryStarted=Time.realtimeSinceStartup;recoveryVersion++;excludedMembers.Clear();excludedMembers.Add(checkpoint.host);
   Sport=checkpoint.sport;previousTimeScale=Time.timeScale;Time.timeScale=0;
   if(TeleportSequence.Busy)TeleportSequence.Instance.Cancel();
   SkySailWorld.Instance?.AbortForRecovery();
   PlayerView.Instance?.ClearMatchInput();ShowRecovery();Changed?.Invoke();
   if(LocalTest)_=RecoverLocal(recoveryVersion);
  }
  MemberCheckpoint Successor(IEnumerable<string> present,string departing=null){
   if(checkpoint==null)return null;var available=present.ToHashSet();
   return checkpoint.members.Values.Where(p=>p.member!=departing&&!excludedMembers.Contains(p.member)&&available.Contains(p.member)).OrderBy(p=>p.seat).ThenBy(p=>p.member,StringComparer.Ordinal).FirstOrDefault();
  }
  void Describe(MemberCheckpoint next){RecoveryMessage=next==null?"Finding the next host…":(next.seat<10?"Player ":"Spectator ")+(next.seat%10+1).ToString("00")+" is becoming host…";}
  async Task RefreshElection(){try{if(Session!=null){await Session.RefreshAsync();await ElectOnlineHost();}}catch(Exception e){Debug.LogWarning("Room recovery: "+e.Message);}}
  async Task ElectOnlineHost(){
   if(electing||leaving||!Recovering||Session==null)return;electing=true;
   try{
    var session=Session;var next=Successor(session.Players.Select(p=>p.Id));Describe(next);
    if(next==null)return;
    // Unity may initially elect any survivor. That coordinator transfers the
    // lobby to the lowest occupied seat before any new game host is started.
    if(session.IsHost){
     if(next.member!=memberId){session.AsHost().Host=next.member;await session.AsHost().SavePropertiesAsync();}
     else if(!NetworkManager.Singleton.IsHost){var network=session.AsHost().Network;if(network.State==NetworkState.Started)await network.StopNetworkAsync();if(network.State==NetworkState.Stopped)await network.StartRelayNetworkAsync(new RelayNetworkOptions());}
    }
   }catch(Exception e){Debug.LogWarning("Host election will retry: "+e.Message);}finally{electing=false;}
  }
  async Task RecoverLocal(uint version){
   if(localRecovery)return;localRecovery=true;
   try{
    var next=Successor(checkpoint.members.Keys);Describe(next);await StopTransport();
    if(next==null||leaving||version!=recoveryVersion)return;
    SetPayload();var n=NetworkManager.Singleton;n.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1",localPort,"127.0.0.1");
    // Let the departing host release its socket, including voluntary leave.
    await Task.Delay(700);
    if(leaving||version!=recoveryVersion)return;
    if(next.member==memberId){if(!n.StartHost())throw new InvalidOperationException("Could not start the replacement host.");}
    else{
     while(Recovering&&!leaving&&version==recoveryVersion){
      if(!n.IsListening)n.StartClient();
      await Task.Delay(1000);
      if(n.IsConnectedClient)break;
     }
    }
   }catch(Exception e){Debug.LogException(e);}finally{localRecovery=false;}
  }
  async Task StopTransport(){var n=NetworkManager.Singleton;if(!n)return;if(n.IsListening)n.Shutdown();while(n&&(n.IsListening||n.ShutdownInProgress))await Task.Yield();await Task.Yield();approvedSeats.Clear();approvedMembers.Clear();connectedMembers.Clear();if(Recovering){var prepared=new TaskCompletionSource<bool>();StartCoroutine(PrepareIsland(prepared));await prepared.Task;}}
  System.Collections.IEnumerator PrepareIsland(TaskCompletionSource<bool> done){
   var app=AppRoot.Instance;var streaming=app.environments.GetComponent<SkySailStreaming>();
   if(streaming&&streaming.enabledForWorld)yield return streaming.Prepare(Sport);
   if(!leaving){if(app.SelectedSport!=Sport)app.SelectSport(Sport,true);SkySailWorld.Instance?.SetIsland(Sport);}done.TrySetResult(true);
  }
  static void SetServerClock(double clock){
   var n=NetworkManager.Singleton;if(!n||!n.IsServer||n.NetworkTimeSystem==null)return;
   var time=n.NetworkTimeSystem;double threshold=time.HardResetThresholdSec;time.HardResetThresholdSec=0;time.Reset(clock,0);time.HardResetThresholdSec=threshold;n.NetworkTickSystem.Reset(clock,clock);
  }
  void EndRecovery(){
   if(Recovering&&!leaving&&NetworkManager.Singleton.IsServer&&checkpoint!=null&&checkpoint.host!=memberId)departedMembers.Add(checkpoint.host);
   if(Recovering)Time.timeScale=previousTimeScale;Recovering=false;recoveryVersion++;
   if(recoveryCanvas)Destroy(recoveryCanvas.gameObject);recoveryCanvas=null;recoveryText=null;observedHeartbeat=-1;nextCheckpoint=0;
   if(!leaving){Changed?.Invoke();if(AppRoot.Instance&&AppRoot.Instance.Exploring)AppRoot.Instance.Show("stadium");}
  }
  void ShowRecovery(){
   RecoveryMessage="Finding the next host…";
   recoveryCanvas=new GameObject("Room reconnecting",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).GetComponent<Canvas>();recoveryCanvas.renderMode=RenderMode.ScreenSpaceOverlay;recoveryCanvas.sortingOrder=32000;
   var scaler=recoveryCanvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);
   var panel=new GameObject("Shade",typeof(Image)).GetComponent<Image>();panel.transform.SetParent(recoveryCanvas.transform,false);panel.color=new Color(.03f,.12f,.15f,.85f);panel.rectTransform.anchorMin=Vector2.zero;panel.rectTransform.anchorMax=Vector2.one;panel.rectTransform.offsetMin=panel.rectTransform.offsetMax=Vector2.zero;
   recoveryText=new GameObject("Message",typeof(Text)).GetComponent<Text>();recoveryText.transform.SetParent(panel.transform,false);recoveryText.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");recoveryText.fontSize=32;recoveryText.color=new Color(.95f,.98f,.86f);recoveryText.alignment=TextAnchor.MiddleCenter;recoveryText.rectTransform.sizeDelta=new Vector2(1000,250);
  }
  void RestoreGames(){RoomGameRecovery.Restore();}
  sealed class RoomNetworkHandler:INetworkHandler {
   readonly RoomService room;readonly SemaphoreSlim gate=new(1,1);
   public RoomNetworkHandler(RoomService room){this.room=room;}
   public async Task StopAsync(){
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    Debug.Log("ROOM_NETWORK_STOP time="+Time.realtimeSinceStartup+" recovering="+room.Recovering+" caller="+Environment.StackTrace);
#endif
    await gate.WaitAsync();try{await room.StopTransport();}finally{gate.Release();}}
   public async Task StartAsync(NetworkConfiguration configuration){
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    Debug.Log("ROOM_NETWORK_START time="+Time.realtimeSinceStartup+" role="+configuration.Role+" recovering="+room.Recovering);
#endif
    await gate.WaitAsync();try{
     if(room.leaving)return;
     await room.StopTransport();room.SetPayload();var n=NetworkManager.Singleton;var transport=n.GetComponent<UnityTransport>();
     transport.SetRelayServerData(configuration.RelayServerData);
     bool host=configuration.Role!=NetworkRole.Client;bool started=host?n.StartHost():n.StartClient();
     if(!started)throw new InvalidOperationException("Could not start the room connection.");
     float deadline=Time.realtimeSinceStartup+45;
     while(!n.IsConnectedClient&&n.IsListening&&Time.realtimeSinceStartup<deadline)await Task.Yield();
     if(!n.IsConnectedClient)throw new InvalidOperationException("Room connection timed out.");
    }finally{gate.Release();}
   }
  }
 }
}
