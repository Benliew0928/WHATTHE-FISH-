#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class TravelMatchReview:MonoBehaviour {
  string folder,shared,role;AppRoot app;bool failed;int checks;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-travelMatchReview");if(i>=0)new GameObject("Travel and match review").AddComponent<TravelMatchReview>().folder=args[i+1];}
  void Check(bool ok,string label){failed|=!ok;checks++;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Until(Func<bool> condition,string label,float seconds=90){float end=Time.realtimeSinceStartup+seconds;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Check(condition(),label);}
  IEnumerator Sync(string tag){File.WriteAllText(Path.Combine(shared,role+"-"+tag),"ready");yield return Until(()=>new[]{"Host","Player","Spectator"}.All(r=>File.Exists(Path.Combine(shared,r+"-"+tag))),"three peers: "+tag);}
  bool Idle=>!RoomMatchStart.Travelling&&!app.PendingSelection.HasValue&&!app.environments.GetComponent<SkySailStreaming>().Busy;
  bool StartMatch(SportId sport)=>sport switch{SportId.Football=>FootballMatch.Instance.StartMatch(),SportId.Basketball=>BasketballMatch.Instance.StartMatch(),SportId.Golf=>GolfMatchManager.Instance.StartMatch(),_=>FishingGame.Instance.StartMatch()};
  void Clear(){FootballMatch.Instance?.ClearMatch();BasketballMatch.Instance?.Clear();FootballBots.Instance?.Clear();BasketballBots.Instance?.Clear();GolfMatchManager.Instance?.ClearMatch();if(FishingGame.Instance){FishingGame.Instance.State.Abandon(FishingGame.Instance.Now);FishingGame.Instance.ReturnToPractice();}}
  void Station(){int i=0;foreach(var n in RoomTeleport.Players()){var pose=RoomSeats.StationLanding(app.SelectedSport,i++);Move(n,pose.position,pose.rotation);}}
  void Move(NetworkAthlete n,Vector3 p,Quaternion q){var a=n.GetComponent<Athlete>();bool enabled=a.capsule.enabled;a.capsule.enabled=false;n.GetComponent<NetworkTransform>().Teleport(p,q,Vector3.one);a.ResetLocomotion();a.capsule.enabled=enabled;Physics.SyncTransforms();}
  IEnumerator Capture(string name){
   yield return null;yield return new WaitForEndOfFrame();var camera=Camera.main;var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Select(c=>(canvas:c,mode:c.renderMode,camera:c.worldCamera,plane:c.planeDistance)).ToArray();
   foreach(var c in canvases)if(c.canvas.enabled&&c.mode==RenderMode.ScreenSpaceOverlay){c.canvas.renderMode=RenderMode.ScreenSpaceCamera;c.canvas.worldCamera=camera;c.canvas.planeDistance=1;}Canvas.ForceUpdateCanvases();
   var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var old=RenderTexture.active;var target=camera.targetTexture;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);camera.targetTexture=target;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);
   foreach(var c in canvases){c.canvas.renderMode=c.mode;c.canvas.worldCamera=c.camera;c.canvas.planeDistance=c.plane;}Canvas.ForceUpdateCanvases();
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");role=Path.GetFileName(folder);shared=Path.Combine(Directory.GetParent(folder).FullName,"Sync");Directory.CreateDirectory(shared);
   yield return Until(()=>AppRoot.Instance&&AppRoot.Instance.rooms.Connected&&RoomTeleport.Players().Length==3,"three real clients connected");app=AppRoot.Instance;if(!app||RoomTeleport.Players().Length!=3){Finish();yield break;}
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;var own=RoomTeleport.Players().First(n=>n.IsOwner);
   if(role=="Spectator")own.ChooseRoomSeatRpc(10);
   yield return Until(()=>RoomSeats.PlayingCount==2&&RoomTeleport.Players().Count(n=>n.IsSpectator)==1,"two players and one spectator");own.ReadyRpc(true);yield return Sync("ready");
   if(app.rooms.Host){yield return Until(()=>RoomTeleport.Players().All(n=>n.Ready.Value),"all ready");var task=app.rooms.SetExploring(true);while(!task.IsCompleted)yield return null;}
   yield return Until(()=>app.Exploring&&Idle,"room entry completed");
   foreach(var sport in new[]{SportId.Football,SportId.Basketball,SportId.Golf,SportId.Fishing}){
    uint seq=RoomTeleport.Instance.State.sequence;yield return Sync(sport+"-travel");
    if(app.rooms.Host){Check(RoomTeleport.Instance.Request(sport,TeleportDestination.Station),"teleport room to "+sport+" station");Check(!StartMatch(app.SelectedSport),"same-frame teleport prevents a match start");}
    yield return Until(()=>RoomTeleport.Instance.State.sequence>seq&&Idle&&app.SelectedSport==sport,"station arrival "+sport);yield return new WaitForSeconds(.5f);yield return Sync(sport+"-station-ready");
    var watcher=RoomTeleport.Players().First(n=>n.IsSpectator);var players=RoomTeleport.Players().Where(n=>!n.IsSpectator).ToArray();
    if(app.rooms.Host){
     var world=SkySailWorld.Instance;var target=SkySailMap.Neighbor(sport,1);Check(world.CanTravel(target,out _),"idle room can ride from "+sport);
     var before=watcher.transform.position;Move(watcher,before+Vector3.right*40,watcher.transform.rotation);Check(!world.CanTravel(target,out var missing)&&missing.Contains("2/3"),"absent spectator blocks ride with attendance count");Move(watcher,before,watcher.transform.rotation);
     if(sport==SportId.Basketball)Check(BasketballBots.Instance.FillRoom(2),"fill 2v2 basketball with two bots");
     if(sport==SportId.Fishing)foreach(var n in players){var fish=FishingGame.Instance;var a=n.GetComponent<Athlete>();var p=fish.Player(a).Value;Check(fish.Execute(a,FishingAction.Ready,fish.State.Match.round,p.sequence),"ready fishing participant");}
     var blocked=GameObject.CreatePrimitive(PrimitiveType.Cube);blocked.name="Blocked viewing arrival fixture";blocked.transform.position=RoomSeats.SpectatorLanding(sport,watcher.RoomSeat.Value%10).position+Vector3.up*.5f;blocked.transform.localScale=new Vector3(2,3,2);Physics.SyncTransforms();
     var positions=RoomTeleport.Players().Select(n=>n.transform.position).ToArray();Check(!StartMatch(sport)&&!PostMatchFlow.MatchActive,"blocked viewing area rejects start "+sport);Check(RoomTeleport.Players().Select((n,i)=>Vector3.Distance(n.transform.position,positions[i])<.01f).All(v=>v),"rejected start moves nobody "+sport);yield return Capture(sport+"-blocked-start");Destroy(blocked);yield return null;Physics.SyncTransforms();
     if(sport==SportId.Fishing){var obstruction=GameObject.CreatePrimitive(PrimitiveType.Cube);obstruction.name="Blocked pier fixture";obstruction.transform.position=FishingGame.Instance.Pier(players[0].RoomSeat.Value)+Vector3.up*.5f;obstruction.transform.localScale=new Vector3(2,3,2);Physics.SyncTransforms();Check(!StartMatch(sport)&&FishingGame.Instance.State.Match.phase==FishingRoundPhase.Practice,"blocked pier prevents countdown");Check(RoomTeleport.Players().All(n=>Vector3.Distance(n.transform.position,SkySailMap.Port(sport))<12),"blocked pier leaves entire room at station");Destroy(obstruction);yield return null;Physics.SyncTransforms();}
     Check(StartMatch(sport),"start "+sport+" from station: "+RoomMatchStart.Message);
    }
    yield return Until(()=>PostMatchFlow.MatchActive,"match state replicated "+sport);yield return new WaitForSeconds(.6f);
    var expected=RoomSeats.SpectatorLanding(sport,watcher.RoomSeat.Value%10).position;Check(Vector2.Distance(new Vector2(expected.x,expected.z),new Vector2(watcher.transform.position.x,watcher.transform.position.z))<.5f,"spectator reaches viewing area "+sport);Check(watcher.IsSpectator&&watcher.RoomSeat.Value==10,"spectator role and number retained "+sport);
    if(sport==SportId.Fishing){var piers=players.Select(n=>FishingGame.Instance.NearestPier(n.GetComponent<Athlete>())).ToArray();Check(piers.All(i=>i>=0)&&piers.Distinct().Count()==2,"players occupy distinct fishing piers before play");Check(FishingGame.Instance.State.Match.phase==FishingRoundPhase.Countdown,"fishing starts with countdown after placement");}
    Check(!app.view.InputBlocked&&own.GetComponent<NetworkTransform>().enabled&&!own.GetComponent<Athlete>().inTransit,"controls and transform remain usable "+sport);yield return Capture(sport+"-started");yield return Sync(sport+"-placed");
    if(app.rooms.Host){Station();var world=SkySailWorld.Instance;uint journey=world.Journey.sequence;var target=SkySailMap.Neighbor(sport,1);Check(!world.CanTravel(target,out var reason)&&reason.Contains("Finish"),"active "+sport+" blocks cable car even with everyone at station");world.RequestTravel(target);Check(!world.Travelling&&world.Journey.sequence==journey,"ride request preserves active round "+sport);Check(!RoomTeleport.Instance.CanRequest(target,TeleportDestination.Game,out _),"active "+sport+" blocks group teleport");}
    yield return new WaitForSeconds(.3f);Check(PostMatchFlow.MatchActive,"round survives rejected travel "+sport);if(sport==SportId.Fishing){yield return Until(()=>FishingGame.Instance.State.Match.phase==FishingRoundPhase.Playing,"Fishing advances to live play");if(app.rooms.Host)Check(!SkySailWorld.Instance.CanTravel(SportId.Football,out var liveReason)&&liveReason.Contains("Finish"),"live Fishing also blocks departure");}yield return Sync(sport+"-clear");if(app.rooms.Host)Clear();yield return Until(()=>!PostMatchFlow.MatchActive,"round cleared "+sport);PostMatchFlow.CloseResults();PostMatchFlow.Instance.Close();
   }
   yield return Sync("ride");if(app.rooms.Host){Station();SkySailWorld.Instance.RequestTravel(SportId.Football);Check(SkySailWorld.Instance.Travelling,"ride locks immediately in request frame");Check(!StartMatch(SportId.Fishing),"same-frame ride rejects Fishing start");Check(!RoomTeleport.Instance.Request(SportId.Football,TeleportDestination.Game),"ride rejects overlapping teleport");}
   yield return Until(()=>SkySailWorld.Instance.Travelling,"shared ride observed");Check(!RoomMatchStart.CanStart(app.SelectedSport),"start disabled during journey");yield return Until(()=>Idle&&app.SelectedSport==SportId.Football,"three-person ride completes",120);Check(RoomTeleport.Players().All(n=>Vector3.Distance(n.transform.position,SkySailMap.Port(SportId.Football))<12),"players and spectator disembark together");Check(RoomTeleport.Players().All(n=>n.GetComponent<NetworkTransform>().enabled&&!n.GetComponent<Athlete>().inTransit),"every passenger restored before journey unlocks");
   yield return Sync("host-spectator");if(app.rooms.Host){var task=app.rooms.SetExploring(false);while(!task.IsCompleted)yield return null;}
   yield return Until(()=>!app.Exploring,"waiting room for role swap");if(role=="Host")own.ChooseRoomSeatRpc(11);yield return Until(()=>NetworkAthlete.HostPlayer.IsSpectator,"host releases playing place");if(role=="Spectator")own.ChooseRoomSeatRpc((byte)Enumerable.Range(0,10).First(i=>RoomTeleport.Players().All(n=>n.RoomSeat.Value!=i)));yield return Until(()=>NetworkAthlete.HostPlayer.IsSpectator&&RoomSeats.PlayingCount==2,"host spectator with two players");own.ReadyRpc(true);yield return Sync("swap-ready");
   if(app.rooms.Host){yield return Until(()=>RoomTeleport.Players().All(n=>n.Ready.Value),"swapped room ready");var task=app.rooms.SetExploring(true);while(!task.IsCompleted)yield return null;}yield return Until(()=>app.Exploring&&Idle,"swapped entry completes");yield return new WaitForSeconds(.3f);
   if(app.rooms.Host){var hud=FindFirstObjectByType<SpectatorHUD>();Check(hud&&hud.startButton&&hud.startButton.interactable,"host spectator gets eligible start control");yield return Capture("Host-spectator-start-guidance");hud.startButton.onClick.Invoke();yield return null;Check(PostMatchFlow.MatchActive&&!hud.startButton.interactable,"host spectator starts match and button disables");}
   yield return Sync("finish");Finish();
  }
  void Finish(){DevelopmentProbe.TurnCommandActive=false;File.AppendAllText(Path.Combine(folder,"results.txt"),"TRAVEL_MATCH_REVIEW_COMPLETE success="+!failed+" checks="+checks+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
