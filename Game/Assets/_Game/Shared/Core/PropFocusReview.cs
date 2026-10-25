#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace WhatTheFish {
 public sealed class PropFocusReview:MonoBehaviour {
  string folder,shared,role;bool failed,network,basket;AppRoot app;Athlete local;PlayerView view;Camera cam;
  BasketballBall bb;FootballBall fb;Athlete[] players;int stage;bool authority;
  static string Arg(string key){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:"";}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var path=Arg("-propFocusReview");if(path.Length>0)new GameObject("Prop focus review").AddComponent<PropFocusReview>().folder=path;}
  void Check(bool ok,string text){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+text+"\n");}
  IEnumerator Until(Func<bool> condition,string label,float seconds=70){float end=Time.realtimeSinceStartup+seconds;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Check(condition(),label);}
  IEnumerator Sync(string tag){if(!network)yield break;File.WriteAllText(Path.Combine(shared,role+tag),"ready");yield return Until(()=>new[]{"Host","Player","Spectator"}.All(r=>File.Exists(Path.Combine(shared,r+tag))),"sync "+tag);}
  IEnumerator Settle(float time=.2f){yield return new WaitForSecondsRealtime(time);yield return new WaitForEndOfFrame();}
  SportsPropSlot Slot(Athlete a)=>basket?bb.PropSlot(a):fb.PropSlot(a);
  void Give(Athlete a,SportsProp prop){if(basket)bb.ReviewGiveProp(a,prop);else fb.ReviewGiveFootballProp(a,prop);}
  double Now=>basket?BasketballMotion.Clock:FootballBall.PropClock;
  SportsPropActivation Event=>basket?bb.Props.activation:fb.Props.activation;
  Vector3 Point(float x,float z){if(basket)return bb.transform.parent.TransformPoint(x,.08f,z);var b=fb.PitchBounds;return fb.Pitch.TransformPoint(b.center.x+x,b.max.y+.03f,b.center.z+z);}
  void Place(Athlete a,float x,float z){var p=Point(x,z);var q=Quaternion.Euler(0,basket?0:fb.Pitch.eulerAngles.y,0);bool enabled=a.capsule.enabled;a.capsule.enabled=false;a.transform.SetPositionAndRotation(p,q);var n=a.GetComponent<NetworkAthlete>();if(n&&n.IsSpawned)a.GetComponent<NetworkTransform>().Teleport(p,q,Vector3.one);a.ResetLocomotion();a.capsule.enabled=enabled;Physics.SyncTransforms();}
  void ResetWorld(){
   if(basket){bb.ReviewResetProps();bb.autoPickup=false;bb.Place(new Vector3(5,.35f,5),Quaternion.identity,Vector3.zero,Vector3.zero);}
   else {fb.ResetFootballProps();fb.ReviewFootballBall(Point(8,8)+Vector3.up*.2f,Vector3.zero);}
   for(int i=0;i<players.Length;i++){var team=basket?bb.PropTeam(players[i]):fb.PropTeam(players[i]);Place(players[i],i==0?-3:3,team==FootballTeam.B?8:-8);}
   if(basket)foreach(var bot in FindObjectsByType<BasketballBot>(FindObjectsSortMode.None)){bot.enabled=false;Place(bot.GetComponent<Athlete>(),6,bot.Slot*2-3);}
  }
  void Request(Athlete a){if(basket)bb.RequestProp(a);else fb.RequestProp(a,a.transform.eulerAngles.y);}
  bool IsOwner(Athlete a)=>!network||a==local;
  SportsPropPresentation Presentation=>basket?bb.GetComponent<SportsPropPresentation>():fb.GetComponent<SportsPropPresentation>();
  void Capture(string label)=>FishingResultsChecks.Capture(Path.Combine(folder,"view.txt"),label,0);
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");role=Path.GetFileName(folder);shared=Path.Combine(Directory.GetParent(folder).FullName,"Sync");Directory.CreateDirectory(shared);network=role!="Offline";
   if(network){
    yield return Until(()=>AppRoot.Instance&&AppRoot.Instance.rooms.Connected&&RoomTeleport.Players().Length==3,"three peers connected");app=AppRoot.Instance;if(!app||RoomTeleport.Players().Length!=3){Finish();yield break;}
    var own=RoomTeleport.Players().First(n=>n.IsOwner);bool hostSpectator=Arg("-spectatorHost")=="yes";
    if(role==(hostSpectator?"Host":"Spectator"))own.ChooseRoomSeatRpc(10);
    yield return Until(()=>RoomSeats.PlayingCount==2&&RoomTeleport.Players().Count(n=>n.IsSpectator)==1,"two players and spectator");own.ReadyRpc(true);yield return Sync("ready");
    if(app.rooms.Host){yield return Until(()=>RoomTeleport.Players().All(n=>n.Ready.Value),"host observes every ready request");var task=app.rooms.SetExploring(true);while(!task.IsCompleted)yield return null;}
   }
   yield return Until(()=>DevelopmentProbe.ReadyToReviewSport,"sport ready");if(!DevelopmentProbe.ReadyToReviewSport){Finish();yield break;}app=AppRoot.Instance;local=app.LocalAthlete;view=app.view;cam=Camera.main;basket=app.SelectedSport==SportId.Basketball;bb=BasketballBall.Active;fb=FootballBall.Instance;authority=!network||app.rooms.Host;
   while(IslandTutorialUI.DismissTop()){}yield return Settle();
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   players=network?RoomTeleport.Players().Where(n=>!n.IsSpectator).OrderBy(n=>n.OwnerClientId).Select(n=>n.GetComponent<Athlete>()).ToArray():new[]{local};
   if(network&&!basket){if(authority)Check(FootballMatch.Instance.StartMatch(),"football match starts");yield return Until(()=>FootballMatch.Instance.KickoffReady||FootballMatch.Instance.Running,"football kickoff ready",35);if(authority&&FootballMatch.Instance.KickoffReady)FootballMatch.Instance.TryKickoff(FootballMatch.Instance.KickoffActor,FootballMatch.Instance.BotKickoffHeading(FootballMatch.Instance.KickoffActor),.3f,FootballMatch.Instance.Snapshot.revision);yield return Until(()=>FootballMatch.Instance.Running,"football live");}
   if(network&&basket){if(authority){Check(BasketballBots.Instance.FillRoom(2),"basketball fills 2v2");foreach(var bot in FindObjectsByType<BasketballBot>(FindObjectsSortMode.None))bot.enabled=false;Check(BasketballMatch.Instance.StartMatch(),"basketball match starts");}yield return Until(()=>BasketballMatch.Instance.Running,"basketball live",35);}
   yield return Settle(.5f);
   var sample=new SportsPropActivation{kind=SportsProp.Turbo,actor=19,sequence=7,started=123.5};using(var writer=new FastBufferWriter(1024,Allocator.Temp)){writer.WriteNetworkSerializable(sample);using var reader=new FastBufferReader(writer,Allocator.Temp);reader.ReadNetworkSerializable(out SportsPropActivation copy);Check(copy.Equals(sample),"activation identity and time serialize");}
   foreach(int mode in new[]{0,1,2})yield return Activation(SportsProp.Turbo,mode,players[mode%players.Length]);
   yield return Activation(SportsProp.Portal,1,players[players.Length-1]);
   if(!basket)yield return Activation(SportsProp.Wall,2,players[0]);
   yield return IconChecks();
   if(basket)yield return LockOn();
   if(authority)ResetWorld();yield return Sync("cleanup");yield return Settle();Check(!view.PropFocusActive&&!view.PropHeadingHeld,"reset clears focus and held heading");Check(SportsPropPresentation.Held(local)==SportsProp.None,"reset clears prop icon source");
   yield return Sync("finished");Finish();
  }
  IEnumerator Activation(SportsProp prop,int mode,Athlete user){
   string tag=(stage++).ToString();DevelopmentProbe.TurnCommand=default;
   if(authority){ResetWorld();Give(user,prop);}yield return Sync(tag+"setup");
   view.mode=mode;view.yaw=basket?0:fb.Pitch.eulerAngles.y;view.pitch=16;yield return Settle(.35f);
   yield return Until(()=>Slot(user).kind==prop,"held inventory replicated "+prop);
   Check(SportsPropPresentation.Held(user)==prop,"held icon source matches "+prop);
   uint before=Event.sequence;float heading=view.yaw;var start=local.transform.position;double at=Now;
   yield return Sync(tag+"activate");if(IsOwner(user))Request(user);
   yield return Until(()=>Event.sequence!=before&&Event.kind==prop,"accepted activation replicated "+prop,4);
   yield return Settle(.45f);
   Check(local.IsSpectator?!view.PropFocusActive:view.PropFocusActive,"camera participation mode="+mode+" spectator="+local.IsSpectator);
   Check(!view.InputBlocked&&Time.timeScale==1,"cinematic does not block input or pause simulation");
   Check(Mathf.Abs(Mathf.DeltaAngle(heading,view.yaw))<.01f,"forced view preserves gameplay heading");
   if(!local.IsSpectator){
    DevelopmentProbe.TurnCommandActive=false;view.stick.value=new Vector2(.3f,.6f);var command=view.ReadCommand();view.stick.value=Vector2.zero;DevelopmentProbe.TurnCommandActive=true;
    Check(command.move.sqrMagnitude>.1f&&Mathf.Abs(Mathf.DeltaAngle(command.heading,heading))<.01f,"real touch input remains available during focus");
    PlayerView.LookDelta=new Vector2(10,0);yield return Settle(.03f);Check(Mathf.Abs(Mathf.DeltaAngle(view.yaw,heading))>1&&view.PropFocusActive,"manual aim changes during forced view");view.yaw=heading;
   }
   Check(Slot(user).kind==SportsProp.None&&SportsPropPresentation.Held(user)==SportsProp.None,"spent prop removes held icon immediately");
   Check(Presentation.ActivationVisible,"world activation effects visible including spectators");
   if(!local.IsSpectator&&user!=local){var p=cam.WorldToViewportPoint(user.transform.position+Vector3.up*1.25f);Check(p.z>0&&Mathf.Abs(p.x-.5f)<.035f&&Mathf.Abs(p.y-.5f)<.035f,"camera points to remote prop user");}
   if(mode==1)Capture(prop+"-activation");
   if(!local.IsSpectator&&!(prop==SportsProp.Portal&&user==local)){
    DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.right,heading=heading};yield return Settle(.3f);DevelopmentProbe.TurnCommand=default;
    Check(Vector3.Distance(start,local.transform.position)>.1f,"player moves while activation is presenting");
   }
   yield return Until(()=>Now-Event.started>=2.75,"activation duration elapsed",5);yield return Settle(.1f);
   Check(!view.PropFocusActive&&(local.IsSpectator?!view.PropHeadingHeld:view.PropHeadingHeld),"ends with view still facing user; spectator untouched");
   Check(Now-at>2.7,"sport clock advances through activation");Check(!Presentation.ActivationVisible,"activation effect cleans up on deadline");
   if(!local.IsSpectator&&user!=local){var p=cam.WorldToViewportPoint(user.transform.position+Vector3.up*1.25f);Check(p.z>0&&Mathf.Abs(p.x-.5f)<.035f,"post-animation heading stays toward user");}
   if(!local.IsSpectator){PlayerView.LookDelta=new Vector2(18,0);yield return Settle(.05f);Check(!view.PropHeadingHeld,"manual look releases held heading");}
   yield return Sync(tag+"done");
  }
  IEnumerator IconChecks(){
   float lineup=basket?0:-8;
   if(authority){ResetWorld();for(int i=0;i<players.Length;i++)Place(players[i],i==0?-2:2,lineup);}yield return Sync("icons-reset");view.mode=1;view.enabled=false;
   cam.transform.position=Point(0,lineup-8)+Vector3.up*4;cam.transform.LookAt(Point(0,lineup)+Vector3.up*1.4f);cam.fieldOfView=52;
   foreach(var prop in basket?new[]{SportsProp.Turbo,SportsProp.Portal,SportsProp.LockOn}:new[]{SportsProp.Turbo,SportsProp.Portal,SportsProp.Wall}){
    if(authority)foreach(var p in players)Give(p,prop);yield return Sync("icons-"+prop);yield return Until(()=>players.All(p=>Slot(p).kind==prop),"all held icons replicated "+prop);yield return Settle(.25f);
    foreach(var p in players){bool present=CharacterIdentityBadges.TryGetHeldProp(p,out var kind,out var icon);bool label=CharacterIdentityBadges.TryGetScreenRect(p,out var bounds);Check(present&&kind==prop&&label&&bounds.Contains(icon.center),"rendered prop icon is inside owner's label "+prop+" "+CharacterIdentityBadges.PlayerName(p));}
    if(!network||role=="Player")Capture("inventory-"+prop);
    // Keep this inventory until every peer has inspected/captured its labels.
    // A faster host must not clear the final slot while a guest is rendering.
    yield return Sync("icons-checked-"+prop);
   }
   if(authority)foreach(var p in players)Give(p,SportsProp.None);yield return Sync("icons-empty");yield return Settle(.3f);Check(players.All(p=>!CharacterIdentityBadges.TryGetHeldProp(p,out _,out _)),"empty inventory removes every rendered icon");view.enabled=true;yield return Settle();
  }
  IEnumerator LockOn(){
   var user=players[0];if(authority){ResetWorld();bb.autoPickup=true;bb.Place(bb.transform.parent.InverseTransformPoint(user.transform.position)+new Vector3(0,.18f,.6f),Quaternion.identity,Vector3.zero,Vector3.zero);}
   yield return Until(()=>bb.Holder==user,"Lock-on user has ball",4);if(authority)Give(user,SportsProp.LockOn);yield return Sync("lock-held");yield return Until(()=>Slot(user).kind==SportsProp.LockOn,"Lock-on inventory replicated");
   view.mode=1;view.yaw=basket?0:fb.Pitch.eulerAngles.y;yield return Settle(.3f);
   if(local==user)Check(CharacterIdentityBadges.TryGetHeldProp(user,out var held,out _)&&held==SportsProp.LockOn,"held prop icon rendered beside name");
   Capture("held-lock-on");yield return Sync("lock-use");if(IsOwner(user))Request(user);
   yield return Until(()=>bb.Props.aiming,"Lock-on starts",3);yield return Settle(.12f);
   Check(!view.PropFocusActive&&!view.PropHeadingHeld,"Lock-on never takes camera");Check(Presentation.WarningVisible,"Lock-on golden warning visible");
   Check(!CharacterIdentityBadges.TryGetHeldProp(user,out _,out _),"spent Lock-on has no inventory icon");Capture("lock-on-warning");
   yield return Until(()=>!bb.Props.aiming,"Lock-on aim ends",3);yield return Settle(.1f);Check(!Presentation.WarningVisible,"Lock-on warning clears on release or interruption");yield return Sync("lock-done");
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"PROP_FOCUS_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
