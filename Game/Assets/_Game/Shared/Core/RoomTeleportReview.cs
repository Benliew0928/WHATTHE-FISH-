#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class RoomTeleportReview:MonoBehaviour {
  string folder,shared,role;bool failed;int checks;AppRoot app;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-roomTeleportReview");if(i>=0&&i+1<a.Length)new GameObject("Room teleport review").AddComponent<RoomTeleportReview>().folder=a[i+1];}
  void Check(bool ok,string text){failed|=!ok;checks++;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+text+"\n");}
  IEnumerator Until(Func<bool> condition,string label,float seconds=35){float end=Time.realtimeSinceStartup+seconds;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Check(condition(),label);}
  IEnumerator Idle(){yield return Until(()=>!RoomTeleport.Busy&&!TeleportSequence.Busy&&!app.environments.GetComponent<SkySailStreaming>().Busy,"previous streaming work released",100);}
  IEnumerator Sync(string stage){File.WriteAllText(Path.Combine(shared,role+"-"+stage),"ready");yield return Until(()=>File.Exists(Path.Combine(shared,"Host-"+stage))&&File.Exists(Path.Combine(shared,"Guest-"+stage)),"both peers ready: "+stage,90);}
  void Click(string name){var b=PostMatchFlow.Instance.GetComponentsInChildren<Button>(true).First(x=>x.name==name);Check(b.gameObject.activeInHierarchy&&b.interactable,"UI action available: "+name);if(b.gameObject.activeInHierarchy&&b.interactable)b.onClick.Invoke();}
  IEnumerator Capture(string name,bool includeUI=true){
   yield return null;yield return new WaitForEndOfFrame();var camera=Camera.main;var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Select(c=>(canvas:c,mode:c.renderMode,camera:c.worldCamera,plane:c.planeDistance,enabled:c.enabled)).ToArray();
   foreach(var c in canvases){if(!includeUI)c.canvas.enabled=false;else if(c.enabled&&c.mode==RenderMode.ScreenSpaceOverlay){c.canvas.renderMode=RenderMode.ScreenSpaceCamera;c.canvas.worldCamera=camera;c.canvas.planeDistance=1;}}Canvas.ForceUpdateCanvases();
   var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var old=RenderTexture.active;var target=camera.targetTexture;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
   var t=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);t.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),t.EncodeToPNG());Destroy(t);camera.targetTexture=target;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);
   if(includeUI&&PostMatchFlow.Instance.Visible)Check(PostMatchFlow.Instance.GetComponentsInChildren<Text>().Where(t=>t.gameObject.activeInHierarchy&&t.text.Length>0).All(t=>t.cachedTextGenerator.vertexCount>=4),"all visible postmatch labels render at "+Screen.width+"x"+Screen.height);
   foreach(var c in canvases){c.canvas.enabled=c.enabled;c.canvas.renderMode=c.mode;c.canvas.worldCamera=c.camera;c.canvas.planeDistance=c.plane;}Canvas.ForceUpdateCanvases();
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");shared=Path.Combine(Directory.GetParent(folder).FullName,"Sync");Directory.CreateDirectory(shared);
   yield return Until(()=>AppRoot.Instance&&AppRoot.Instance.LocalAthlete&&AppRoot.Instance.rooms.Connected&&RoomTeleport.Players().Length==2,"two connected players",100);app=AppRoot.Instance;if(!app||!app.rooms.Connected||RoomTeleport.Players().Length!=2){Finish();yield break;}
   role=app.rooms.Host?"Host":"Guest";DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;RoomTeleport.Players().First(n=>n.IsOwner).ReadyRpc(true);
   if(app.rooms.Host){yield return Until(()=>RoomTeleport.Players().All(n=>n.Ready.Value),"room ready");var task=app.rooms.SetExploring(true);while(!task.IsCompleted)yield return null;}
   yield return Until(()=>app.Exploring,"enter shared world");yield return new WaitForSeconds(1);app.view.mode=1;
   foreach(var sport in new[]{SportId.Football,SportId.Basketball,SportId.Golf,SportId.Fishing}){
    yield return Travel(sport,TeleportDestination.Game);
    if(failed){Finish();yield break;}
    yield return ResultsAndRematch(sport);
    if(failed){Finish();yield break;}
    yield return Travel(sport,TeleportDestination.Station);
    yield return Cabin(sport);
   }
   yield return Sync("blocked-landing");GameObject obstacle=null;uint blockedSeq=RoomTeleport.Instance.State.sequence;var blockedSource=app.SelectedSport;var blockedPositions=RoomTeleport.Players().Select(n=>n.transform.position).ToArray();
   if(app.rooms.Host){obstacle=GameObject.CreatePrimitive(PrimitiveType.Cube);obstacle.name="Room teleport obstruction fixture";obstacle.transform.position=TeleportDestinations.Nominal(SportId.Fishing,TeleportDestination.Game,0).position+Vector3.up;obstacle.transform.localScale=new Vector3(1.4f,2,1.4f);Physics.SyncTransforms();Check(RoomTeleport.Instance.Request(SportId.Fishing,TeleportDestination.Game),"blocked landing begins preparation");}
   yield return Until(()=>RoomTeleport.Instance.State.sequence>blockedSeq&&RoomTeleport.Instance.State.phase==RoomTeleportPhase.Cancelled&&!TeleportSequence.Busy,"blocked landing cancels entire room");Check(Restored()&&app.SelectedSport==blockedSource&&RoomTeleport.Players().Select((n,i)=>Vector3.Distance(n.transform.position,blockedPositions[i])<.35f).All(x=>x),"blocked landing preserves every source pose");if(obstacle)Destroy(obstacle);yield return null;
   // Rollback at each meaningful boundary keeps the shared room and all actors.
   foreach(var phase in new[]{TeleportPhase.Preparing,TeleportPhase.Absorbing,TeleportPhase.Arriving}){
    yield return Idle();yield return Sync("cancel-"+phase);var before=RoomTeleport.Players().Select(n=>n.transform.position).ToArray();var source=app.SelectedSport;uint seq=RoomTeleport.Instance.State.sequence;
    if(app.rooms.Host)Check(RoomTeleport.Instance.Request(SportId.Football,TeleportDestination.Station),"cancel fixture starts "+phase);
    yield return Until(()=>RoomTeleport.Instance.State.sequence>seq,"cancel sequence replicated");
    if(app.rooms.Host){yield return Until(()=>TeleportSequence.Instance&&TeleportSequence.Instance.Phase==phase,"reached cancel phase "+phase);RoomTeleport.Instance.Cancel();}
    yield return Until(()=>RoomTeleport.Instance.State.phase==RoomTeleportPhase.Cancelled&&!TeleportSequence.Busy,"cancel restored "+phase,95);yield return new WaitForSeconds(.6f);
    Check(app.SelectedSport==source&&RoomTeleport.Players().Select((n,i)=>Vector3.Distance(n.transform.position,before[i])<.35f).All(x=>x),"cancel restores every source position "+phase);Check(Restored(),"cancel restores renderers, controls and network transforms "+phase);
   }
   yield return Idle();yield return Sync("disconnect");
   if(app.rooms.Host){Check(RoomTeleport.Instance.Request(SportId.Golf,TeleportDestination.Game),"disconnect fixture starts");yield return Until(()=>RoomTeleport.Instance.State.phase==RoomTeleportPhase.Cancelled&&!TeleportSequence.Busy,"departure cancels shared teleport",95);Check(app.rooms.Connected&&RoomTeleport.Players().Length==1&&Restored(),"remaining host stays in room and visible");}
   else{yield return Until(()=>RoomTeleport.Busy,"guest sees disconnect fixture");var task=app.rooms.Leave();while(!task.IsCompleted)yield return null;yield return Until(()=>!TeleportSequence.Busy,"disconnected guest releases teleport state");}
   Finish();
  }
  bool Restored()=>!RoomTeleport.Busy&&!TeleportSequence.Busy&&RoomTeleport.Players().All(n=>!n.GetComponent<Athlete>().inTransit&&n.GetComponent<NetworkTransform>().enabled&&n.GetComponent<Athlete>().visual.GetComponentsInChildren<Renderer>(true).All(r=>!r.forceRenderingOff&&r.sharedMaterials.All(m=>!m||m.shader.name!="WhatTheFish/TeleportCharacter")));
  IEnumerator Travel(SportId sport,TeleportDestination destination){
   string tag=sport+"-"+destination;yield return Idle();yield return Sync(tag);PostMatchFlow.Instance.Open();uint seq=RoomTeleport.Instance.State.sequence;
   if(app.rooms.Host){Click("Choose "+sport);Click(destination==TeleportDestination.Game?"Choose game location":"Choose station");yield return null;yield return Capture(tag+"-choice");Click("Teleport everyone");}
   else {Check(!RoomTeleport.Instance.Request(sport,destination),"guest cannot choose destination");Check(!PostMatchFlow.Instance.PlayAgain(),"guest cannot start match");}
   yield return Until(()=>RoomTeleport.Instance.State.sequence>seq,"shared teleport published");
   bool charge=false,absorb=false,travel=false,arrival=false,hitch=false;float deadline=Time.realtimeSinceStartup+100;
   while((RoomTeleport.Busy||TeleportSequence.Busy)&&Time.realtimeSinceStartup<deadline){
    var s=TeleportSequence.Instance;if(s){
     if(s.Phase==TeleportPhase.Charging&&!charge){charge=true;Check(RoomTeleport.Players().All(n=>n.GetComponent<Athlete>().inTransit&&!n.GetComponent<NetworkTransform>().enabled),"all actors locked before disappearing");if(app.rooms.Host){Check(!RoomTeleport.Instance.Request(sport,destination)&&!PostMatchFlow.Instance.PlayAgain(),"overlapping travel and match restart rejected");var n=RoomTeleport.Players().First(n=>n.IsOwner);var ack=n.TeleportReady.Value;n.TeleportReadyRpc(seq,3,false);Check(n.TeleportReady.Value.Equals(ack),"stale acknowledgement rejected");n.TeleportReadyRpc(RoomTeleport.Instance.State.sequence,3,false);Check(n.TeleportReady.Value.Equals(ack),"arrival acknowledgement rejected before arrival phase");}}
     absorb|=s.Phase==TeleportPhase.Absorbing;if(!app.rooms.Host&&sport==SportId.Golf&&destination==TeleportDestination.Game&&s.Phase==TeleportPhase.Travelling&&s.PhaseProgress>.2f&&!hitch){hitch=true;System.Threading.Thread.Sleep(650);}
     if(s.Phase==TeleportPhase.Travelling&&!travel&&s.PhaseProgress>.4f){travel=true;Check(RoomTeleport.Players().All(n=>n.GetComponent<Athlete>().visual.GetComponentsInChildren<Renderer>().All(r=>r.forceRenderingOff)),"all characters absorbed during visible world flight");yield return Capture(tag+"-flight");}
     arrival|=s.Phase==TeleportPhase.Arriving;
    }yield return null;
   }
   Check(RoomTeleport.Instance.State.phase==RoomTeleportPhase.Complete&&Restored(),tag+" completes and restores actors");Check(charge&&absorb&&travel&&arrival,"every animation phase observed on "+role);yield return new WaitForSeconds(.7f);
   Check(app.rooms.Connected&&app.Exploring&&app.SelectedSport==sport&&NetworkAthlete.HostPlayer.WorldSport.Value==sport,"same room agrees on destination "+tag);
   var people=RoomTeleport.Players();var plan=TeleportSequence.Instance.Landings;Check(people.Length==2&&plan.Length==2,"two assigned human landing spaces");
   for(int i=0;i<people.Length&&i<plan.Length;i++)Check(Vector3.Distance(people[i].transform.position,plan[i].position)<.4f,"network player at assigned destination "+i+" actual="+people[i].transform.position+" planned="+plan[i].position);
   Check(people.Length==2&&Vector3.Distance(people[0].transform.position,people[1].transform.position)>1.3f,"no overlapping arrivals");Check(TeleportSequence.Instance.LargestCameraStep<60,"continuous camera flight max step="+TeleportSequence.Instance.LargestCameraStep);
   Check(app.environments.roots.Count(r=>r&&r.activeSelf)==1,"only destination scene remains");Check(!app.view.InputBlocked,"movement input restored");yield return Capture(tag+"-arrival");
  }
  static void Publish(object match,string method="Publish")=>match.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(match,null);
  IEnumerator ResultsAndRematch(SportId sport){
   yield return Sync(sport+"-finish");
   if(app.rooms.Host){
    switch(sport){
     case SportId.Football:var f=FootballMatch.Instance;Check(f.StartMatch(),"start football fixture");f.State.TeamsConfirmed(f.Now);f.State.Advance(f.Now+10);f.State.Advance(f.Now+10000);Publish(f);break;
     case SportId.Basketball:var b=BasketballMatch.Instance;Check(BasketballBots.Instance.FillRoom(2),"fill basketball room to four");Check(b.StartMatch(),"start basketball fixture");b.State.Confirm(b.Now,0);b.State.Advance(b.Now);b.State.Advance(b.Now+10000);b.State.Advance(b.Now+20000);Publish(b,"Synchronize");break;
     case SportId.Golf:var g=GolfMatchManager.Instance;Check(g.StartMatch(),"start golf fixture");foreach(var p in g.State.Players)for(int hole=1;hole<=5;hole++){g.State.RecordSwing(p.PlayerId,g.Now);g.State.EnterHole(p.PlayerId,hole,g.Now);}Publish(g);break;
     case SportId.Fishing:var fish=FishingGame.Instance;foreach(var n in RoomTeleport.Players()){var a=n.GetComponent<Athlete>();var p=fish.Player(a).Value;fish.Execute(a,FishingAction.Ready,fish.State.Match.round,p.sequence);}Check(fish.StartMatch(),"start fishing fixture");fish.State.Advance(fish.State.Match.deadline+1,0);Publish(fish);break;
    }
   }
   yield return new WaitForSeconds(2);PostMatchFlow.CloseResults();yield return Until(()=>PostMatchFlow.Instance.Visible,sport+" postmatch panel opens");Check(app.rooms.Connected&&app.Exploring,"results keep player in shared world");yield return Capture(sport+"-postmatch");
   if(sport==SportId.Football){Screen.SetResolution(960,540,false);yield return new WaitForSeconds(.7f);yield return Capture("postmatch-compact");Screen.SetResolution(1280,720,false);yield return new WaitForSeconds(.7f);}
   Click("Keep exploring");Check(!PostMatchFlow.Instance.Visible&&!app.view.InputBlocked,"keep exploring closes only panel");yield return null;Click("Open next adventure");Check(PostMatchFlow.Instance.Visible,"next adventure can reopen");
   if(!app.rooms.Host&&sport==SportId.Fishing)Click("Ready for fishing");yield return Sync(sport+"-rematch");yield return new WaitForSeconds(.5f);
   if(app.rooms.Host)Click("Play again");yield return Until(()=>PostMatchFlow.MatchActive,sport+" rematch starts");Check(!PostMatchFlow.Instance.Visible,"new match dismisses choices");if(app.rooms.Host)Check(!RoomTeleport.Instance.CanRequest(sport,TeleportDestination.Station,out _),"active match blocks travel");yield return Sync(sport+"-clear");
   if(app.rooms.Host){switch(sport){case SportId.Football:FootballMatch.Instance.ClearMatch();break;case SportId.Basketball:BasketballMatch.Instance.Clear();break;case SportId.Golf:GolfMatchManager.Instance.ClearMatch();break;case SportId.Fishing:FishingGame.Instance.State.Abandon(FishingGame.Instance.Now);FishingGame.Instance.ReturnToPractice();break;}}
   yield return Until(()=>!PostMatchFlow.MatchActive,"fixture cleanup");yield return new WaitForSeconds(.5f);PostMatchFlow.CloseResults();PostMatchFlow.Instance.Close();
  }
  IEnumerator Cabin(SportId sport){
   var world=SkySailWorld.Instance;var camera=Camera.main;bool view=app.view.enabled;world.reviewCamera=true;app.view.enabled=false;
   foreach(var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))c.enabled=false;
   camera.transform.SetPositionAndRotation(world.cabin.TransformPoint(new Vector3(0,1.7f,-2.9f)),world.cabin.rotation*Quaternion.Euler(0,16,0));camera.fieldOfView=60;yield return Capture(sport+"-cabin-interior",false);
   var lighting=world.cabin.GetComponent<SkySailCabinLighting>();Check(lighting&&lighting.InteriorView(camera),"interior shading recognizes cabin view");
   var lamps=world.cabin.GetComponentsInChildren<Renderer>().Where(r=>r.sharedMaterials.Any(m=>m&&m.name=="SS_Lamp")).ToArray();Check(lamps.Length>0&&lamps.All(r=>r.sharedMaterials.All(m=>m.shader.name=="Universal Render Pipeline/Unlit"&&m.renderQueue>2500&&m.IsKeywordEnabled("_SURFACE_TYPE_TRANSPARENT"))&&!r.receiveShadows),"bulbs glow after the opaque ambient-shading pass");
   if(lamps.Length>0){var center=lamps[0].bounds.center+world.cabin.right*2.2f;camera.transform.position=center+world.cabin.TransformDirection(new Vector3(0,-.25f,-1.4f));camera.transform.LookAt(center);camera.fieldOfView=55;yield return Capture(sport+"-cabin-bulbs",false);}
   camera.transform.position=world.cabin.TransformPoint(new Vector3(12,9,-19));camera.transform.LookAt(world.cabin.position+Vector3.up*2);Check(!lighting.InteriorView(camera),"outside views retain contact shading");yield return Capture(sport+"-cabin-exterior",false);
   foreach(var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))c.enabled=true;app.view.enabled=view;world.reviewCamera=false;
  }
  void Finish(){DevelopmentProbe.TurnCommandActive=false;File.AppendAllText(Path.Combine(folder,"results.txt"),"ROOM_TELEPORT_REVIEW_COMPLETE success="+!failed+" checks="+checks+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
