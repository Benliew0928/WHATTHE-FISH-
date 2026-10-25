#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class CrewConnectionsReview:MonoBehaviour {
  string folder,lastCommand="",result="";bool busy;int capture;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-crewReview");if(i>=0&&i+1<args.Length)new GameObject("Crew connections review").AddComponent<CrewConnectionsReview>().folder=args[i+1];}
  [Serializable]sealed class Peer {public string member,name,profile;public ulong stable;public byte seat;public FootballTeam football,basketball;public Vector3 position;}
  [Serializable]sealed class Control {public string name;public bool enabled;}
  [Serializable]sealed class Report {public string code,member,error,result,match,sport,phase;public bool connected,host,paused,retrying,exploring,travelling,canStart;public float seconds,timeScale;public double clock;public Peer[] peers;public PublicProfile self;public string football,basketball,golf,fishing;public int invites;public string[] ui;public Control[] controls;}
  IEnumerator Start(){Directory.CreateDirectory(folder);while(!AppRoot.Instance||!AppRoot.Instance.rooms)yield return null;
   while(true){
    var app=AppRoot.Instance;var room=app.rooms;var own=RoomTeleport.Players().FirstOrDefault(p=>p.IsOwner);
    if(own&&!RoomService.Paused&&!own.Ready.Value)own.ReadyRpc(true);
    var host=NetworkAthlete.HostPlayer;var report=new Report{code=room.Code,member=room.MemberId,error=room.Error,result=result,connected=room.Connected,host=room.Host,paused=RoomService.Paused,retrying=room.RejoiningSelf,seconds=room.GuestSeconds,clock=RoomTeleport.Clock,timeScale=Time.timeScale,sport=app.SelectedSport.ToString(),invites=RoomInvitations.Instance.Count,self=PlayerProfiles.Current,exploring=app.Exploring,travelling=RoomMatchStart.Travelling,canStart=RoomMatchStart.CanStart(app.SelectedSport),phase=host?(app.SelectedSport switch{SportId.Football=>host.Match.Value.phase.ToString(),SportId.Basketball=>host.BasketballMatch.Value.phase.ToString(),SportId.Golf=>host.GolfMatch.Value.phase.ToString(),_=>host.FishingMatch.Value.phase.ToString()}):"",
     peers=RoomTeleport.Players().Select(p=>new Peer{member=p.MemberId.Value.ToString(),name=p.Nickname.Value.ToString(),profile=p.ProfileId.Value.ToString(),stable=p.StableId,seat=p.RoomSeat.Value,football=p.Team.Value,basketball=p.BasketballTeam.Value,position=p.transform.position}).ToArray(),
     football=host?JsonUtility.ToJson(host.Match.Value):"",basketball=host?JsonUtility.ToJson(host.BasketballMatch.Value):"",golf=host?JsonUtility.ToJson(host.GolfMatch.Value):"",fishing=host?JsonUtility.ToJson(host.FishingMatch.Value):"",
     ui=FindObjectsByType<Text>(FindObjectsSortMode.None).Where(t=>t.isActiveAndEnabled&&t.canvasRenderer.GetAlpha()>0).Select(t=>t.text).Where(t=>!string.IsNullOrEmpty(t)).ToArray(),controls=FindObjectsByType<Button>(FindObjectsSortMode.None).Where(b=>b.isActiveAndEnabled).Select(b=>new Control{name=b.name,enabled=b.IsInteractable()}).ToArray()};
    try{File.WriteAllText(Path.Combine(folder,"crew.json.tmp"),JsonUtility.ToJson(report,true));File.Copy(Path.Combine(folder,"crew.json.tmp"),Path.Combine(folder,"crew.json"),true);}catch(IOException){ }
    string command="";try{if(File.Exists(Path.Combine(folder,"crew-command.txt")))command=File.ReadAllText(Path.Combine(folder,"crew-command.txt")).Trim();}catch(IOException){ }
    if(!busy&&command.Length>0&&command!=lastCommand){lastCommand=command;_=Command(command);}
    if(room.GuestRecovering&&capture++==2)StartCoroutine(Capture("reconnecting"));
    yield return new WaitForSecondsRealtime(.2f);
   }
  }
  async Task Command(string command){busy=true;try{
   var app=AppRoot.Instance;var room=app.rooms;var own=RoomTeleport.Players().FirstOrDefault(p=>p.IsOwner);
   var split=command.Split(':');switch(split[0]){
    case "drop":room.ReviewRetryDelay=float.Parse(split[1],System.Globalization.CultureInfo.InvariantCulture);NetworkManager.Singleton.Shutdown();break;
    case "leave":await room.Leave();break;
    case "explore":await room.SetExploring(true);break;
    case "lobby":await room.SetExploring(false);break;
    case "start":switch(app.SelectedSport){case SportId.Football:FootballMatch.Instance.StartMatch();break;case SportId.Basketball:BasketballMatch.Instance.StartMatch();break;case SportId.Golf:GolfMatchManager.Instance.StartMatch();break;default:foreach(var actor in RoomTeleport.Players().Where(n=>!n.IsSpectator))FishingGame.Instance.Execute(actor.GetComponent<Athlete>(),FishingAction.Ready,FishingGame.Instance.State.Match.round,FishingGame.Instance.Player(actor.GetComponent<Athlete>()).Value.sequence);FishingGame.Instance.StartMatch();break;}break;
    case "kickoff":var match=FootballMatch.Instance;var kicker=RoomTeleport.Players().First(p=>p.StableId==match.Snapshot.kickoffPlayer);if(!match.TryKickoff(kicker.GetComponent<Athlete>(),0,.5f,match.Snapshot.revision))throw new InvalidOperationException("Kickoff was not ready.");break;
    case "host-online":await room.Create(app.SelectedSport);break;
    case "join-online":await room.Join(split[1]);break;
    case "cloud":await PlayerProfiles.EnsureCloud();break;
    case "click":var button=FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.name==split[1]&&b.isActiveAndEnabled&&b.IsInteractable());button.onClick.Invoke();break;
    case "resize":Screen.SetResolution(int.Parse(split[1]),int.Parse(split[2]),false);break;
    case "friend-request":await PlayerProfiles.Friend(split[1],"request");break;
    case "friend-accept":await PlayerProfiles.Friend(split[1],"accept");break;
    case "invite":await RoomInvitations.Instance.Send(split[1]);break;
    case "refresh":await RoomInvitations.Instance.Refresh();break;
    case "join-invite":await RoomInvitations.Instance.Join(RoomInvitations.Instance.Inbox.First(),false);break;
    case "inbox":SocialPanel.OpenInvitations();break;
    case "friends":SocialPanel.OpenFriends();break;
    case "capture":StartCoroutine(Capture("ui-"+split[1]));break;
    case "delete-profile":await PlayerProfiles.Request("/v1/me","DELETE",new DeleteRequest{confirm="delete-profile"});break;
    case "quit":Application.Quit();break;
   }
   result="OK "+command;
  }catch(Exception error){result="ERROR "+command+": "+error.Message;Debug.LogException(error);}finally{busy=false;}}
  [Serializable]sealed class DeleteRequest {public string confirm;}
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var camera=Camera.main;var previousTarget=camera.targetTexture;var previousActive=RenderTexture.active;var target=new RenderTexture(Screen.width,Screen.height,24);var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
   try{foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=.5f;}camera.targetTexture=target;for(int pass=0;pass<3;pass++){foreach(var t in FindObjectsByType<Text>(FindObjectsSortMode.None))t.SetAllDirty();Canvas.ForceUpdateCanvases();camera.Render();}RenderTexture.active=target;image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());}
   finally{camera.targetTexture=previousTarget;RenderTexture.active=previousActive;Destroy(target);Destroy(image);for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=RenderMode.ScreenSpaceOverlay;canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}Canvas.ForceUpdateCanvases();}
  }
 }
}
#endif
