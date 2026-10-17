#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class SpectatorReview:MonoBehaviour {
  string folder,shared,role;AppRoot app;bool failed;int checks;NetworkAthlete own;
  readonly List<NetworkAthlete> fixtures=new();
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-spectatorReview");if(i>=0&&i+1<a.Length)new GameObject("Spectator review").AddComponent<SpectatorReview>().folder=a[i+1];}
  void Check(bool ok,string label){failed|=!ok;checks++;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Until(Func<bool> condition,string label,float seconds=80){float end=Time.realtimeSinceStartup+seconds;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Check(condition(),label);}
  IEnumerator Sync(string name){File.WriteAllText(Path.Combine(shared,role+"-"+name),"ready");yield return Until(()=>File.Exists(Path.Combine(shared,"Host-"+name))&&File.Exists(Path.Combine(shared,"Guest-"+name)),"peers: "+name);}
  IEnumerator Idle(){yield return Until(()=>!RoomTeleport.Busy&&!TeleportSequence.Busy&&!app.PendingSelection.HasValue&&!app.environments.GetComponent<SkySailStreaming>().Busy,"all loading and travel released");}
  IEnumerator Capture(string name){
   yield return null;yield return new WaitForEndOfFrame();var cam=Camera.main;
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Select(c=>(c,mode:c.renderMode,c.worldCamera,c.planeDistance)).ToArray();
   foreach(var c in canvases)if(c.c.enabled&&c.mode==RenderMode.ScreenSpaceOverlay){c.c.renderMode=RenderMode.ScreenSpaceCamera;c.c.worldCamera=cam;c.c.planeDistance=1;}
   Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var old=RenderTexture.active;var target=cam.targetTexture;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
   var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);cam.targetTexture=target;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);
   foreach(var c in canvases){c.c.renderMode=c.mode;c.c.worldCamera=c.worldCamera;c.c.planeDistance=c.planeDistance;}
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");shared=Path.Combine(Directory.GetParent(folder).FullName,"Sync");Directory.CreateDirectory(shared);
   yield return Until(()=>AppRoot.Instance&&AppRoot.Instance.rooms.Connected&&RoomTeleport.Players().Length==2,"two peers connected");app=AppRoot.Instance;if(!app||RoomTeleport.Players().Length!=2){Finish();yield break;}
   role=app.rooms.Host?"Host":"Guest";own=RoomTeleport.Players().First(n=>n.IsOwner);DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   if(app.rooms.Host){NetworkAthlete.HostPlayer.AllowSpectatorsRpc(false);Check(!RoomSeats.Choose(own,10,out _),"closed spectator seat rejected by server");NetworkAthlete.HostPlayer.AllowSpectatorsRpc(true);own.ChooseRoomSeatRpc(0);}
   yield return Sync("policy");if(!app.rooms.Host)own.ChooseRoomSeatRpc(10);
   yield return Until(()=>RoomTeleport.Players().Count(n=>n.IsSpectator)==1,"guest switches into spectator row");
   Check(RoomTeleport.Players().All(n=>n.IdentityNumber.Value==1),"player 01 and spectator 01 reuse the same model independently");
   if(app.rooms.Host){Check(!RoomSeats.Choose(own,10,out _),"occupied spectator slot cannot be stolen");NetworkAthlete.HostPlayer.AllowSpectatorsRpc(false);Check(RoomSeats.Allowed,"closing occupied row does not evict spectators");}
   else{own.AllowSpectatorsRpc(false);yield return new WaitForSeconds(.2f);Check(RoomSeats.Allowed,"guest cannot change host admission policy");own.ChooseRoomSeatRpc(19);}
   yield return Until(()=>RoomTeleport.Players().Any(n=>n.RoomSeat.Value==19&&n.IdentityNumber.Value==10),"spectator chooses number ten");
   if(!app.rooms.Host)own.ChooseRoomSeatRpc(10);
   yield return Until(()=>RoomTeleport.Players().Any(n=>n.RoomSeat.Value==10),"spectator returns to number one");
   if(!app.rooms.Host)own.NicknameRpc("WATCHER");
   yield return Until(()=>RoomSeats.Signature().Contains("WATCHER"),"delayed nickname refreshes numbered room places");
   app.rooms.SeatMessage="";yield return new WaitForSeconds(1);yield return Capture("Room-two-rows");
   Screen.SetResolution(844,390,false);yield return new WaitForSeconds(.6f);yield return Capture("Room-compact");Screen.SetResolution(1280,720,false);
   app.view.mode=1;own.ReadyRpc(true);yield return Sync("entry");
   uint sequence=RoomTeleport.Instance.State.sequence;
   if(app.rooms.Host){yield return Until(()=>RoomTeleport.Players().All(n=>n.Ready.Value),"everyone ready");var task=app.rooms.SetExploring(true);while(!task.IsCompleted)yield return null;}
   yield return Journey(sequence,true,"Football-entry");if(failed){Finish();yield break;}
   foreach(var sport in new[]{SportId.Football,SportId.Basketball,SportId.Golf,SportId.Fishing}){
    if(sport!=SportId.Football){yield return Idle();yield return Sync(sport+"-travel");sequence=RoomTeleport.Instance.State.sequence;if(app.rooms.Host)Check(RoomTeleport.Instance.Request(sport,TeleportDestination.Game),"host travels with spectator to "+sport);yield return Journey(sequence,false,sport+"-travel");}
    if(failed){Finish();yield break;}
    yield return FullCapacity(sport);if(failed){Finish();yield break;}
    yield return Movement(sport);if(failed){Finish();yield break;}
    if(sport!=SportId.Football){yield return Sync(sport+"-lobby");if(app.rooms.Host){var task=app.rooms.SetExploring(false);while(!task.IsCompleted)yield return null;}yield return Until(()=>!app.Exploring,"return to waiting room");yield return new WaitForSeconds(.7f);own.ReadyRpc(true);yield return Sync(sport+"-entry");sequence=RoomTeleport.Instance.State.sequence;if(app.rooms.Host){var task=app.rooms.SetExploring(true);while(!task.IsCompleted)yield return null;}yield return Journey(sequence,true,sport+"-entry");}
   }
   yield return Sync("host-watching");if(app.rooms.Host){var task=app.rooms.SetExploring(false);while(!task.IsCompleted)yield return null;}
   yield return Until(()=>!app.Exploring,"waiting room for role swap");own.ChooseRoomSeatRpc((byte)(app.rooms.Host?11:1));
   yield return Until(()=>NetworkAthlete.HostPlayer.IsSpectator&&RoomTeleport.Players().Any(n=>!n.IsSpectator&&n.OwnerClientId!=0),"host can spectate while guest plays");own.ReadyRpc(true);yield return Sync("cancel-entry");
   if(app.rooms.Host){yield return Until(()=>RoomTeleport.Players().All(n=>n.Ready.Value),"swapped roster ready");var task=app.rooms.SetExploring(true);while(!task.IsCompleted)yield return null;}
   yield return Until(()=>TeleportSequence.Instance&&TeleportSequence.Instance.Phase==TeleportPhase.Travelling,"swapped room entry flies");yield return Sync("cancel-entry-flight");if(app.rooms.Host)RoomTeleport.Instance.Cancel();
   yield return Until(()=>!app.Exploring&&!RoomTeleport.Busy&&!TeleportSequence.Busy,"cancelled entry restores waiting room");
   Check(!own.GetComponent<Athlete>().inTransit&&own.GetComponent<NetworkTransform>().enabled,"cancelled entry restores network and movement");
   yield return Sync("disconnect");if(!app.rooms.Host){var task=app.rooms.Leave();while(!task.IsCompleted)yield return null;}else yield return Until(()=>RoomTeleport.Players().Length==1&&RoomSeats.Free(app.SelectedSport,false)==0,"guest departure leaves host spectator connected");
   Finish();
  }
  IEnumerator Journey(uint previous,bool entry,string tag){
   yield return Until(()=>RoomTeleport.Instance.State.sequence>previous,"journey published "+tag);bool flight=false,arrival=false,hitch=false;float deadline=Time.realtimeSinceStartup+100;
   while((RoomTeleport.Busy||TeleportSequence.Busy)&&Time.realtimeSinceStartup<deadline){
    var s=TeleportSequence.Instance;if(s&&s.Phase==TeleportPhase.Travelling){
     if(!flight){flight=true;Check(own.GetComponent<Athlete>().inTransit&&!own.GetComponent<NetworkTransform>().enabled,"input and network transform held during flight");Check(!RoomSeats.Choose(own,1,out _),"slot switch blocked during entry/travel");yield return Capture(tag+"-cloud");if(entry)Check(Camera.main.transform.position.y>70,"entry starts in sky");}
     if(!app.rooms.Host&&!hitch&&s.PhaseProgress>.35f){hitch=true;System.Threading.Thread.Sleep(400);}
    }
    if(s&&s.Phase==TeleportPhase.Arriving&&!arrival){arrival=true;yield return Capture(tag+"-reveal");}
    yield return null;
   }
   Check(RoomTeleport.Instance.State.phase==RoomTeleportPhase.Complete,"journey completed "+tag+" "+RoomTeleport.Instance.Message+" "+(TeleportSequence.Instance?TeleportSequence.Instance.Error:""));
   Check(flight&&arrival,"flight and blue-light arrival observed "+tag);Check(!app.view.InputBlocked&&!own.GetComponent<Athlete>().inTransit&&own.GetComponent<NetworkTransform>().enabled,"input camera and network restored");
   yield return new WaitForSeconds(.5f);yield return Idle();
   var roster=RoomTeleport.Players();var plan=TeleportSequence.Instance.Landings;
   for(int i=0;i<roster.Length&&i<plan.Length;i++)Check(Vector3.Distance(roster[i].transform.position,plan[i].position)<.5f,"assigned destination replicated "+i);
   Check(app.rooms.Connected&&own.IsSpectator==!app.rooms.Host,"same room and chosen places retained");yield return Capture(tag+"-landed");
  }
  IEnumerator FullCapacity(SportId sport){
   yield return Sync(sport+"-capacity");
   if(app.rooms.Host){
    for(int i=1;i<10;i++)Spawn((byte)(10+i));for(int i=1;i<TeleportDestinations.Capacity(sport);i++)Spawn((byte)i);
    var roster=RoomSeats.Occupants.OrderBy(n=>n.RoomSeat.Value).Select(n=>n.GetComponent<Athlete>()).ToArray();
    Check(roster.Length==TeleportDestinations.Capacity(sport)+10,"full independent player and spectator capacities "+sport);
    Check(RoomSeats.Free(sport,true)==255&&RoomSeats.Free(sport,false)==255,"both rows reject overflow "+sport);
    foreach(var kind in new[]{TeleportDestination.Game,TeleportDestination.Station}){
     bool valid=TeleportDestinations.Resolve(sport,kind,roster,Vector3.zero,out var plan,out var error);Check(valid,"full-capacity safe landings "+sport+" "+kind+" "+error);
     if(valid&&kind==TeleportDestination.Game){for(int i=0;i<roster.Length;i++){var actor=roster[i];actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(plan[i].position,plan[i].rotation);actor.GetComponent<NetworkTransform>().Teleport(plan[i].position,plan[i].rotation,Vector3.one);actor.capsule.enabled=true;}Physics.SyncTransforms();}
    }
    Check(FootballBots.Roster.Count(a=>a.IsSpectator)==0&&BasketballBots.Roster.Count(a=>a.IsSpectator)==0,"spectators excluded from both bot fill rosters");
    yield return Capture(sport+"-full-capacity");
   }
   yield return Until(()=>RoomSeats.Occupants.Length==TeleportDestinations.Capacity(sport)+10,"full roster replicated to peer");
   yield return Until(()=>RoomSeats.Occupants.Where(n=>n.IsSpectator).Select(n=>n.IdentityNumber.Value).Distinct().Count()==10,"ten distinct spectator models replicated");
   yield return Sync(sport+"-full-roster");if(app.rooms.Host){foreach(var n in fixtures)n.NetworkObject.Despawn(true);fixtures.Clear();}
   yield return Until(()=>RoomSeats.Occupants.Length==2,"capacity fixtures removed");
  }
  void Spawn(byte seat){var go=Instantiate(NetworkManager.Singleton.NetworkConfig.PlayerPrefab,new Vector3(0,20,0),Quaternion.identity);var n=go.GetComponent<NetworkAthlete>();go.GetComponent<NetworkObject>().Spawn();n.SetRoomSeat(seat);fixtures.Add(n);}
  IEnumerator Movement(SportId sport){
   yield return Sync(sport+"-movement");var spectator=RoomTeleport.Players().First(n=>n.IsSpectator).GetComponent<Athlete>();
   if(app.rooms.Host){
    Check(!spectator.TryKick()&&!spectator.TryTackle()&&!spectator.CanRequestJump,"spectator cannot perform sporting actions");
    if(sport==SportId.Football||sport==SportId.Basketball){
     if(sport==SportId.Football){Check(FootballBots.Instance.FillRoom(2),"bot fill counts only playing places");Check(FootballMatch.Instance.StartMatch(),"football starts with spectator present");Check(!FootballMatch.Instance.ChooseTeam(spectator,FootballTeam.A,FootballMatch.Instance.Snapshot.revision),"spectator excluded from football teams");}
     else{Check(BasketballBots.Instance.FillRoom(2),"basketball bot fill counts only players");Check(BasketballMatch.Instance.StartMatch(),"basketball starts with spectator present");Check(!BasketballMatch.Instance.Contains(spectator),"spectator excluded from basketball teams");}
     Check(SpectatorMovement.Court(out var frame,out var bounds),"authored playing boundary found");
     foreach(float rate in new[]{20f,30f,60f,120f})foreach(int side in new[]{0,1,2,3}){
      var p=bounds.center;p.y=.05f;if(side<2)p.x=(side==0?bounds.min.x-1.2f:bounds.max.x+1.2f);else p.z=(side==2?bounds.min.z-1.2f:bounds.max.z+1.2f);
      Vector3 direction=side==0?Vector3.right:side==1?Vector3.left:side==2?Vector3.forward:Vector3.back;
      for(int i=0;i<rate*2;i++)p+=SpectatorMovement.Constrain(p,direction*7/rate,bounds,.95f);
      Check(Mathf.Abs(p.x-bounds.center.x)>bounds.extents.x+.8f||Mathf.Abs(p.z-bounds.center.z)>bounds.extents.z+.8f,"court exclusion side="+side+" rate="+rate);
     }
     // Actual authoritative capsule movement at the touchline, including a long frame.
     var start=new Vector3(bounds.max.x+1.2f,.08f,bounds.center.z);spectator.capsule.enabled=false;spectator.transform.position=frame.TransformPoint(start);spectator.capsule.enabled=true;spectator.ResetLocomotion();
     for(int i=0;i<180;i++)spectator.Simulate(new PlayerCommand{move=Vector2.up,heading=frame.eulerAngles.y-90,sprint=true,kick=true,tackle=true,jump=true,shoot=true},i==20?.25f:1/60f);
     Check(frame.InverseTransformPoint(spectator.transform.position).x>bounds.max.x+.8f,"real spectator capsule cannot enter court");
     Check(!FootballBall.Instance||FootballBall.Instance.CurrentController!=spectator,"spectator never gains football possession");Check(!BasketballBall.Active||!BasketballBall.Active.CanShoot(spectator),"spectator never gains basketball possession");
     // Restore the assigned view before testing guest input.
     var landing=RoomSeats.SpectatorLanding(sport,0);Physics.Raycast(landing.position+Vector3.up*3,Vector3.down,out var floor,10,1<<8);spectator.GetComponent<NetworkTransform>().Teleport(floor.point+Vector3.up*.035f,landing.rotation,Vector3.one);spectator.ResetLocomotion();
    }else if(sport==SportId.Fishing)Check(!FishingGame.Instance.Player(spectator).HasValue,"spectator does not occupy a fishing pier");
    else Check(!GolfCartWorld.Execute(spectator,GolfCartAction.Summon,0),"spectator cannot summon gameplay equipment");
   }
   yield return Sync(sport+"-walk");yield return new WaitForSeconds(.5f);Vector3 before=own.transform.position;
   if(!app.rooms.Host)DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,heading=own.transform.eulerAngles.y+90,sprint=true};
   yield return new WaitForSeconds(1);DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.5f);
   if(!app.rooms.Host)Check(Vector3.Distance(before,own.transform.position)>.4f,"guest spectator input moves through server "+sport);
   yield return Capture(sport+"-watching");yield return Sync(sport+"-clear-match");
   if(app.rooms.Host){FootballMatch.Instance?.ClearMatch();BasketballMatch.Instance?.Clear();FootballBots.Instance?.Clear();BasketballBots.Instance?.Clear();}
   yield return Until(()=>!PostMatchFlow.MatchActive,"match fixture cleared");
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"SPECTATOR_REVIEW_COMPLETE success="+!failed+" checks="+checks+"\n");Application.Quit(failed?2:0);}
 }
}
#endif
