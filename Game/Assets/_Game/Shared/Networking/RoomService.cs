using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace SportsPrototype {
 public interface IRoomService { Task Create(); Task Join(string code); Task Leave(); }
 public sealed class RoomService:MonoBehaviour,IRoomService {
  public ISession Session {get;private set;} public bool busy; public bool LocalTest; public string Error {get;private set;} public event Action Changed;
  public bool Connected=>!leaving&&NetworkManager.Singleton&&NetworkManager.Singleton.IsListening;
  public bool Host=>Connected&&NetworkManager.Singleton.IsHost;
  public string Code=>Session?.Code??(LocalTest?"LOCAL TEST":"—");
  bool leaving;
  async Task Initialize(){
   if(string.IsNullOrEmpty(Application.cloudProjectId))throw new InvalidOperationException("Online rooms need a linked Unity cloud project. Offline Explore is ready to use. See Docs/SETUP.md.");
   if(UnityServices.State!=ServicesInitializationState.Initialized)await UnityServices.InitializeAsync();
   if(!AuthenticationService.Instance.IsSignedIn)await AuthenticationService.Instance.SignInAnonymouslyAsync();
  }
  public async Task Create(){await Run(async()=>{
   await Initialize();
   Session=await MultiplayerService.Instance.CreateSessionAsync(new SessionOptions{MaxPlayers=10,IsPrivate=true,Name="Football • "+LocalProfile.Stadium.title}.WithRelayNetwork());
   Bind();
  });}
  public async Task Join(string code){await Run(async()=>{
   code=code.Trim().ToUpperInvariant();if(code.Length<4||code.Length>12||!code.All(char.IsLetterOrDigit))throw new InvalidOperationException("Enter the room code shared by your host.");
   await Initialize();Session=await MultiplayerService.Instance.JoinSessionByCodeAsync(code);Bind();
  });}
  void Bind(){Session.Deleted+=Closed;Session.RemovedFromSession+=Closed;Session.SessionHostChanged+=HostChanged;Changed?.Invoke();}
  void HostChanged(string id){Closed();}
  void Closed(){if(!leaving)_=ExitWithMessage("The host left. This room is now closed.");}
  public async Task SetExploring(bool value){await Run(async()=>{
   if(!Host)throw new InvalidOperationException("Only the host can start exploration.");
   if(value&&FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).Any(p=>!p.Ready.Value))throw new InvalidOperationException("Wait until every player is ready.");
   if(Session!=null){Session.AsHost().IsLocked=value;await Session.AsHost().SavePropertiesAsync();}
   var world=NetworkAthlete.HostPlayer;if(world)world.Exploring.Value=value;
  });}
  public void SetupNetwork(){
   var n=NetworkManager.Singleton;n.NetworkConfig.ConnectionApproval=true;
   n.ConnectionApprovalCallback=(request,response)=>{
    bool full=n.ConnectedClients.Count>=10;bool running=NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.Exploring.Value;
    response.Approved=!full&&!running;response.CreatePlayerObject=response.Approved;
    response.Reason=full?"This room is full (10 players).":running?"Exploration has started. Ask the host to return to the waiting room.":"";
    response.Position=new Vector3((n.ConnectedClients.Count%5-2)*2,1,-8-(n.ConnectedClients.Count/5)*3);response.Rotation=Quaternion.identity;
   };
   n.OnClientDisconnectCallback+=id=>{if(!leaving&&(!n.IsServer||id==n.LocalClientId)){string reason=n.DisconnectReason;if(string.IsNullOrEmpty(reason)||reason.StartsWith("[Disconnect"))reason="The connection closed or the host left. You can create a new room or explore offline.";_=ExitWithMessage(reason);}};
  }
  public void StartLocal(bool host,ushort port=7777){
   LocalTest=true;NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1",port,"127.0.0.1");
   if(host)NetworkManager.Singleton.StartHost();else NetworkManager.Singleton.StartClient();Changed?.Invoke();
  }
  public async Task Leave(){if(leaving)return;bool wasHost=Host;leaving=true;var session=Session;Session=null;Changed?.Invoke();
   if(session!=null){session.Deleted-=Closed;session.RemovedFromSession-=Closed;session.SessionHostChanged-=HostChanged;try{if(wasHost)await session.AsHost().DeleteAsync();else await session.LeaveAsync();}catch(Exception e){Debug.LogWarning(e.Message);}}
   if(NetworkManager.Singleton){NetworkManager.Singleton.Shutdown();while(NetworkManager.Singleton&&NetworkManager.Singleton.IsListening)await Task.Yield();}LocalTest=false;leaving=false;Changed?.Invoke();
  }
  async Task ExitWithMessage(string message){await Leave();Error=message;Changed?.Invoke();}
  public async Task Run(Func<Task> action){if(busy)return;busy=true;Error=null;Changed?.Invoke();try{await action();}catch(Exception e){Debug.LogException(e);Error=e.Message;if(!Connected&&Session!=null)await Leave();}finally{busy=false;Changed?.Invoke();}}
 }
}
