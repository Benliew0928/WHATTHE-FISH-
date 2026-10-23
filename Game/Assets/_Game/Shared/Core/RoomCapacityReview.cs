#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using UnityEngine;
namespace WhatTheFish {
 // Development-only multi-process capacity and abrupt-disconnect probe.
 public sealed class RoomCapacityReview:MonoBehaviour {
  string folder,previous,lastCommand;bool ready;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-roomCapacityProbe");if(i>=0&&i+1<a.Length)new GameObject("Room capacity probe").AddComponent<RoomCapacityReview>().folder=a[i+1];}
  IEnumerator Start(){Directory.CreateDirectory(folder);
   if(!default(BasketballSnapshot).Equals(default)||!default(GolfBallSnapshot).Equals(default))throw new InvalidOperationException("Default ball snapshots must compare equal to themselves; otherwise every idle avatar floods reliable replication.");
   while(!AppRoot.Instance||!AppRoot.Instance.rooms)yield return null;float deadline=Time.realtimeSinceStartup+1800;
   Unity.Netcode.NetworkManager.Singleton.OnClientDisconnectCallback+=id=>File.AppendAllText(Path.Combine(folder,"history.txt"),Time.realtimeSinceStartup+" disconnected "+id+"\n");
   while(Time.realtimeSinceStartup<deadline){var app=AppRoot.Instance;var people=RoomTeleport.Players();
    var own=people.FirstOrDefault(n=>n.IsOwner);
    if(own&&!ready&&!app.rooms.Recovering){own.ReadyRpc(true);ready=true;app.view.mode=1;}
    if(File.Exists(Path.Combine(folder,"leave"))&&app.rooms.Connected){var leaving=app.rooms.Leave();while(!leaving.IsCompleted)yield return null;Application.Quit();yield break;}
    string commandPath=Path.Combine(folder,"command.txt");
    string command=null;
    // File replacement can briefly hide the destination from Mono's reader.
    // A missed command read must not stop the probe or its live heartbeat.
    try{if(own&&File.Exists(commandPath))command=File.ReadAllText(commandPath).Trim();}catch(IOException){}
    if(own&&command!=null){
     if(command!=lastCommand){lastCommand=command;
      if(command.StartsWith("nick:"))own.NicknameRpc(command.Substring(5));
      else if(command.StartsWith("seat:")){own.ChooseRoomSeatRpc(byte.Parse(command.Split(':')[1]));}
      else if(command.StartsWith("ready:")){own.ReadyRpc(true);}
      else if(command.StartsWith("unready:")){own.ReadyRpc(false);}
      else if(command.StartsWith("spot:")){bool changed=app.rooms.TrySetLobbySport(Enum.Parse<SportId>(command.Split(':')[1]),out var reason);File.WriteAllText(Path.Combine(folder,"spot-result.txt"),"changed="+changed+" reason="+reason);}
      else if(command.StartsWith("match:")&&app.rooms.Host){switch(app.SelectedSport){case SportId.Football:FootballMatch.Instance.StartMatch();break;case SportId.Basketball:BasketballMatch.Instance.StartMatch();break;case SportId.Golf:GolfMatchManager.Instance.StartMatch();break;case SportId.Fishing:foreach(var actor in people.Where(n=>!n.IsSpectator))FishingGame.Instance.Execute(actor.GetComponent<Athlete>(),FishingAction.Ready,FishingGame.Instance.State.Match.round,FishingGame.Instance.Player(actor.GetComponent<Athlete>()).Value.sequence);FishingGame.Instance.StartMatch();break;}}
      else if(app.rooms.Host&&command.StartsWith("explore:")){var task=app.rooms.SetExploring(command.Split(':')[1]=="true");while(!task.IsCompleted)yield return null;}
     }
    }
    string state="count="+people.Length+" players="+people.Count(n=>!n.IsSpectator)+" spectators="+people.Count(n=>n.IsSpectator)+" unique="+(people.Select(n=>n.RoomSeat.Value).Distinct().Count()==people.Length)+" connected="+app.rooms.Connected+" error="+app.rooms.Error+"\n"+string.Join("\n",people.Select(n=>n.OwnerClientId+" seat="+n.RoomSeat.Value+" model="+n.IdentityNumber.Value));
    if(state!=previous){previous=state;File.AppendAllText(Path.Combine(folder,"history.txt"),Time.realtimeSinceStartup+" "+state+"\n");}
    string live=state+"\nlocal="+Unity.Netcode.NetworkManager.Singleton.LocalClientId+" localSeat="+(own?own.RoomSeat.Value:255)+" exploring="+app.Exploring+" travel="+(RoomTeleport.Instance?RoomTeleport.Instance.State.phase:RoomTeleportPhase.Idle)+" transit="+(own&&own.GetComponent<Athlete>().inTransit)+" serverTime="+RoomTeleport.Clock.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+" frame="+Time.frameCount+"\n"+RoomSeats.Signature()+"\nhost="+app.rooms.Host+" recovering="+app.rooms.Recovering+" hostSeat="+(NetworkAthlete.HostPlayer?NetworkAthlete.HostPlayer.RoomSeat.Value:255)+" code="+app.rooms.Code+"\n"+string.Join("\n",people.Select(n=>"member="+n.MemberId.Value+" stable="+n.StableId+" seat="+n.RoomSeat.Value+" name="+n.Nickname.Value+" position="+n.transform.position.ToString("F2")))+"\nmatch="+(NetworkAthlete.HostPlayer?app.SelectedSport switch{SportId.Football=>NetworkAthlete.HostPlayer.Match.Value.phase.ToString(),SportId.Basketball=>NetworkAthlete.HostPlayer.BasketballMatch.Value.phase.ToString(),SportId.Golf=>NetworkAthlete.HostPlayer.GolfMatch.Value.phase.ToString(),_=>NetworkAthlete.HostPlayer.FishingMatch.Value.phase.ToString()}:"None");
    live+="\nsport="+app.SelectedSport+" lobbyReady="+RoomService.LobbyReady+" readyGuests="+people.Count(n=>n!=NetworkAthlete.HostPlayer&&n.Ready.Value);
    var manager=Unity.Netcode.NetworkManager.Singleton;
    live+="\nheartbeat="+(NetworkAthlete.HostPlayer?NetworkAthlete.HostPlayer.RoomHeartbeat.Value:0)+" acknowledged="+(own&&own.RecoveryAcknowledged.Value);
    if(manager.IsServer)live+="\nconnections="+string.Join(",",manager.ConnectedClientsIds)+"\nvisible="+string.Join(";",people.Select(n=>n.OwnerClientId+":"+string.Join(",",manager.ConnectedClientsIds.Where(id=>n.NetworkObject.IsNetworkVisibleTo(id)))));
    live+=TransportDiagnostics(manager.GetComponent<UnityTransport>());
    if(manager.IsServer)live+="\ndirty="+string.Join(";",people.Select(n=>n.OwnerClientId+":"+string.Join(",",typeof(NetworkAthlete).GetFields().Where(f=>f.GetValue(n) is Unity.Netcode.NetworkVariableBase variable&&variable.IsDirty()).Select(f=>f.Name))));
    string target=Path.Combine(folder,"roster.txt"),temporary=target+".tmp";
    // Windows readers can temporarily deny deletion. Retry publication next tick;
    // a status-file viewer must never terminate the probe coroutine.
    try{File.WriteAllText(temporary,live);if(File.Exists(target))File.Replace(temporary,target,null);else File.Move(temporary,target);}catch(IOException){}
    yield return new WaitForSecondsRealtime(.5f);
   }Application.Quit();
  }
  static string TransportDiagnostics(UnityTransport transport){
   var driver=transport.GetNetworkDriver();if(!driver.IsCreated)return "";
   var flags=BindingFlags.Instance|BindingFlags.NonPublic;var type=typeof(UnityTransport);
   var queues=(IDictionary)type.GetField("m_SendQueue",flags).GetValue(transport);
   var reliable=(NetworkPipeline)type.GetField("m_ReliableSequencedPipeline",flags).GetValue(transport);
   var parse=type.GetMethod("ParseClientId",BindingFlags.Static|BindingFlags.NonPublic,null,new[]{typeof(ulong)},null);
   string result="\ntransport=";
   foreach(DictionaryEntry entry in queues){
    var target=entry.Key;var targetType=target.GetType();var pipeline=(NetworkPipeline)targetType.GetField("NetworkPipeline").GetValue(target);if(!pipeline.Equals(reliable))continue;
    var id=(ulong)targetType.GetField("ClientId").GetValue(target);var connection=(NetworkConnection)parse.Invoke(null,new object[]{id});if(driver.GetConnectionState(connection)!=NetworkConnection.State.Connected)continue;
    driver.GetPipelineBuffers(pipeline,NetworkPipelineStageId.Get<ReliableSequencedPipelineStage>(),connection,out _,out _,out var shared);
    var context=shared.GetSubArray(0,UnsafeUtility.SizeOf<ReliableUtility.SharedContext>()).Reinterpret<ReliableUtility.SharedContext>(1)[0];
    result+=id+":queued="+entry.Value.GetType().GetProperty("Length").GetValue(entry.Value)+",rtt="+context.RttInfo.LastRtt+",sent="+context.stats.PacketsSent+",received="+context.stats.PacketsReceived+",resent="+context.stats.PacketsResent+",dropped="+context.stats.PacketsDropped+";";
   }return result;
  }
 }
}
#endif
