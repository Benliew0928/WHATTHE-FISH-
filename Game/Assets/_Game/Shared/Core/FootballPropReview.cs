#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class FootballPropReview:MonoBehaviour {
  string folder,shared;bool failed;AppRoot app;FootballBall ball;FootballMatch match;Athlete actor;Camera reviewCamera;int originalCamera;Athlete[] peers;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-footballPropReview");if(i>=0&&i+1<a.Length)new GameObject("Football ability verification").AddComponent<FootballPropReview>().folder=a[i+1];}
  void Check(bool ok,string name){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+name+"\n");}
  void Signal(string name)=>File.WriteAllText(Path.Combine(shared,name),"ready");
  IEnumerator Wait(string name,float seconds=30){float end=Time.realtimeSinceStartup+seconds;while(!File.Exists(Path.Combine(shared,name))&&Time.realtimeSinceStartup<end)yield return null;Check(File.Exists(Path.Combine(shared,name)),"signal "+name);}
  Vector3 Point(float x,float z,float y=0)=>ball.Pitch.TransformPoint(new Vector3(ball.PitchBounds.center.x+x,ball.PitchBounds.max.y+y,ball.PitchBounds.center.z+z));
  float Heading(float offset=0)=>ball.Pitch.eulerAngles.y+offset;
  void Place(Athlete a,float x,float z,float heading=0){var p=Point(x,z,.02f);a.capsule.enabled=false;a.transform.SetPositionAndRotation(p,Quaternion.Euler(0,Heading(heading),0));var net=a.GetComponent<NetworkObject>();if(net&&net.IsSpawned)a.GetComponent<NetworkTransform>().Teleport(p,a.transform.rotation,Vector3.one);a.capsule.enabled=ball.HasAuthority;a.ResetLocomotion();Physics.SyncTransforms();}
  void PlaceAt(Athlete a,Vector3 p){a.capsule.enabled=false;a.transform.position=p;var n=a.GetComponent<NetworkObject>();if(n&&n.IsSpawned)a.GetComponent<NetworkTransform>().Teleport(p,a.transform.rotation,Vector3.one);a.capsule.enabled=ball.HasAuthority;a.ResetLocomotion();Physics.SyncTransforms();}
  bool Use(SportsProp kind){ball.ReviewGiveFootballProp(actor,kind);return ball.TryUseProp(actor,ball.PropSlot(actor).revision,match.Snapshot.revision,actor.transform.eulerAngles.y);}
  void FreeBall(float x,float z,float y=.18f,Vector3 velocity=default){ball.ReviewFootballBall(Point(x,z,y),ball.Pitch.TransformDirection(velocity));Physics.SyncTransforms();}
  IEnumerator Acquire(Athlete who){ball.ReviewFootballBall(ball.FootPosition(who),Vector3.zero);Physics.SyncTransforms();ball.RefreshControl(who);yield return null;Check(ball.CurrentController==who,"fixture attaches football");}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);shared=Directory.GetParent(folder).FullName;File.WriteAllText(Path.Combine(folder,"results.txt"),"");float end=Time.realtimeSinceStartup+95;
   while((!(app=AppRoot.Instance)||!app.Exploring||!FootballBall.Instance)&&Time.realtimeSinceStartup<end)yield return null;
   ball=FootballBall.Instance;actor=app?app.LocalAthlete:null;match=FootballMatch.Instance;if(!ball||!actor||!match){Check(false,"football initialized");Finish();yield break;}
   originalCamera=app.view.mode;app.view.mode=2;DevelopmentProbe.Driving=false;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.6f);
   if(!ball.HasAuthority){yield return Guest();Finish();yield break;}
   peers=Athlete.Active.Where(a=>a.GetComponent<NetworkObject>()&&a.GetComponent<NetworkObject>().IsSpawned).OrderBy(a=>a.GetComponent<NetworkObject>().OwnerClientId).ToArray();
   if(Environment.GetCommandLineArgs().Contains("-explorePropsOnly")){yield return ExploreChecks();Finish();yield break;}
   Check(peers.Length==0?ball.OfflinePropPractice&&ball.Props.live:!ball.Props.live&&ball.Props.pickup.kind==SportsProp.None,"solo Explore enables props; multiplayer practice waits for teams");Check(NetworkManager.Singleton.NetworkConfig.ProtocolVersion==RoomService.ProtocolVersion,"matching ability protocol");
   bool started=peers.Length==0?match.StartTestMatch():match.StartMatch();Check(started,"formal match starts");
   var roster=peers.Length==0?new[]{actor}:peers;foreach(var a in roster){int i=Array.IndexOf(roster,a);Check(match.ChooseTeam(a,i%2==0?FootballTeam.A:FootballTeam.B,match.Snapshot.revision),"choose team "+i);}
   match.Selection.Complete(new System.Random(7));match.State.TeamsConfirmed(match.Now-4);match.State.Advance(match.Now);yield return new WaitForFixedUpdate();yield return null;
   Check(match.Running&&ball.Props.live,"abilities run only in live match");for(int i=0;i<roster.Length;i++)Place(roster[i],i==0?0:8+i, i%2==0?-8:8,i%2==0?0:180);yield return new WaitForSeconds(.2f);
   Signal("live");RuleChecks();yield return SpawnChecks();yield return TurboChecks();yield return PortalChecks();yield return WallChecks();yield return Gallery();
   if(peers.Length==4)yield return NetworkChecks();yield return GoalChecks();
   ball.ResetFootballProps();Check(ball.PropSlot(actor).kind==SportsProp.None&&!ball.PortalBusy&&!ball.Props.wallA.active&&!ball.Props.wallB.active&&ball.TurboRemaining(actor)==0,"reset clears inventory and every ability effect");
   match.ClearMatch();yield return new WaitForSeconds(.2f);Check((peers.Length==0?ball.OfflinePropPractice&&ball.Props.pickup.respawn>FootballBall.PropClock&&!ball.Props.pickup.countingDown:!ball.Props.live)&&!ball.PortalBusy,"match exit clears effects and solo Explore resumes fresh countdown");Signal("host-done");if(peers.Length==4){yield return Wait("guest-done");yield return Wait("teammate-done");yield return Wait("rival-done");}Finish();
  }
  IEnumerator ExploreChecks(){
   Check(peers.Length==0&&match.Snapshot.phase==FootballMatchPhase.Idle&&!match.Running,"ordinary offline Explore without a test match");
   Check(ball.OfflinePropPractice&&ball.PropsLive&&ball.Props.live,"Explore automatically enables prop simulation and visuals");Check(ball.PropTeam(actor)==FootballTeam.A&&match.TeamOf(actor)==FootballTeam.None,"solo practice gets ability team without changing match roster");
   var spawn=ball.Props.pickup;var visual=ball.GetComponent<FootballPropVisual>();int seen=0;bool early=false,stable=true;double end=spawn.respawn;float timeout=Time.realtimeSinceStartup+12;
   while(ball.Props.pickup.kind==SportsProp.None&&Time.realtimeSinceStartup<timeout){yield return new WaitForEndOfFrame();if(!ball.Props.pickup.countingDown)continue;int number=visual.CountdownNumber;seen|=1<<number;early|=FootballBall.PropClock<end&&visual.PickupModel;stable&=ball.Props.pickup.anchor==spawn.anchor&&ball.Props.pickup.respawn==end;}
   Check(seen==62&&stable&&!early,"automatic first countdown visibly steps 5 4 3 2 1 at its fixed location");Check(ball.Props.pickup.kind!=SportsProp.None&&FootballBall.PropClock>=end,"first Explore pickup appears at its ten-second deadline");
   yield return null;Check(visual.PickupModel&&visual.PickupModel.gameObject.activeInHierarchy&&visual.PickupModel.GetComponent<Renderer>().bounds.size.magnitude>.6f,"actual Explore prop mesh visible above pitch");
   var anchor=ball.PropAnchor(ball.Props.pickup.anchor);Capture("explore-first-spawn",anchor+ball.Pitch.TransformDirection(new Vector3(anchor==ball.PropAnchor(0)?7:-7,6,-9)),anchor+Vector3.up*3.8f);CaptureHud("explore-spawn-stadium");
   var kind=ball.Props.pickup.kind;PlaceAt(actor,anchor);yield return new WaitForSeconds(.15f);Check(ball.PropSlot(actor).kind==kind&&ball.Props.pickup.kind==SportsProp.None,"walk into Explore pickup grants usable held slot");CaptureHud("explore-held-prop");
   Place(actor,0,-8);var held=ball.PropSlot(actor);app.view.yaw=Heading();FootballPropHUD.Instance.Activate();if(kind==SportsProp.Wall){yield return null;FootballPropHUD.Instance.Activate();}yield return new WaitForSeconds(.85f);Check(ball.PropSlot(actor).kind==SportsProp.None&&ball.PropSlot(actor).revision!=held.revision,"Explore prop activates through actual HUD button");
   ball.ReviewExpireFootballEffects();yield return new WaitForFixedUpdate();RuleChecks();yield return SpawnChecks();yield return TurboChecks();yield return PortalChecks();yield return WallChecks();
   Check(match.Snapshot.phase==FootballMatchPhase.Idle&&match.State.ScoreA==0&&match.State.ScoreB==0,"all three abilities work without starting match or enabling scoring");
   Place(actor,0,-8);FreeBall(8,8);ball.ReviewGiveFootballProp(actor,SportsProp.Wall);var stale=ball.PropSlot(actor);uint oldRound=match.Snapshot.revision;Check(match.StartTestMatch(),"formal match may start after solo ability practice");yield return new WaitForFixedUpdate();Check(!ball.PropsLive&&!ball.Props.live&&ball.PropSlot(actor).kind==SportsProp.None,"team selection clears practice props and effects");Check(!ball.TryUseProp(actor,stale.revision,oldRound,Heading()),"practice request cannot leak into formal match");
   match.ClearMatch();yield return new WaitForFixedUpdate();yield return null;Check(ball.OfflinePropPractice&&ball.Props.pickup.respawn>FootballBall.PropClock&&!ball.Props.pickup.countingDown&&ball.PropSlot(actor).kind==SportsProp.None,"returning to solo Explore restarts a clean countdown");
   ball.ReviewGiveFootballProp(actor,SportsProp.Portal);FreeBall(8,8);Place(actor,0,-8);Check(ball.RequestProp(actor,Heading()),"practice portal starts before exit");app.LeaveCourt();yield return new WaitForSeconds(.2f);Check(!app.Exploring&&!ball.PropsLive&&!ball.Props.live&&!ball.PortalBusy&&ball.PropSlot(actor).kind==SportsProp.None,"leaving Explore clears pickup inventory and pending transfer");
  }
  void RuleChecks(){
   var cycle=SportsAbilityRules.SchedulePickup(100,1);Check(cycle.respawn==110&&!cycle.countingDown&&cycle.kind==SportsProp.None,"shared cycle schedules exactly ten seconds");Check(!SportsAbilityRules.WarnPickup(ref cycle,104.999)&&SportsAbilityRules.WarnPickup(ref cycle,105)&&cycle.CountdownNumber(105)==5,"shared warning starts at five seconds exactly");Check(!SportsAbilityRules.WarnPickup(ref cycle,109)&&cycle.respawn==110&&cycle.anchor==1,"late updates never extend deadline or change location");
   foreach(var previous in new[]{SportsProp.Turbo,SportsProp.Portal,SportsProp.Wall}){var a=FootballPropRules.NextKind(previous,0);var b=FootballPropRules.NextKind(previous,1);Check(a!=previous&&b!=previous&&a!=b&&FootballPropRules.Kind(a)&&FootballPropRules.Kind(b),"both spawn alternatives exclude "+previous);}
   var wall=new FootballWallState{active=true,position=Vector3.zero,heading=0,until=20};
   Check(FootballPropRules.Sweep(wall,new Vector3(0,1,-100),new Vector3(0,1,100),.11f,out var f,out var n)&&f>.49f&&f<.51f&&n.z<-.9f,"continuous high-speed wall crossing");
   Check(!FootballPropRules.Sweep(wall,new Vector3(0,6,-3),new Vector3(0,6,3),.11f,out _,out _),"lob above top clears wall");
   Check(!FootballPropRules.Sweep(wall,new Vector3(3.2f,1,-3),new Vector3(3.2f,1,3),.11f,out _,out _),"ball clears wall side");
   Check(FootballPropRules.Sweep(wall,new Vector3(3.05f,1,-3),new Vector3(3.05f,1,3),.11f,out _,out _),"ball radius clips wall edge");
   Check(!FootballPropRules.Sweep(wall,new Vector3(0,1,-.26f),new Vector3(0,1,-2),.11f,out _,out _),"touching ball may leave barrier");
   Check(!FootballPropRules.Sweep(wall,new Vector3(float.NaN,0,0),Vector3.zero,.11f,out _,out _),"non-finite sweep rejected");
   var crossing=new FootballWallState{position=new Vector3(0,0,1),heading=90};Check(FootballPropRules.Overlap(wall,crossing),"crossed walls overlap");crossing.position=Vector3.forward*8;Check(!FootballPropRules.Overlap(wall,crossing),"separated walls do not overlap");
   var snapshot=new FootballBallSnapshot{valid=true,controllerId=7,sequence=13,rotation=Quaternion.identity,props=new FootballPropsState{live=true,wallA=wall,pickup=new SportsPropSpawn{countingDown=true,respawn=9,anchor=1},transferring=true,receiver=4,portalSequence=37,source=Vector3.up,destination=Vector3.right}};
   using var writer=new FastBufferWriter(1024,Allocator.Temp);writer.WriteNetworkSerializable(snapshot);using var reader=new FastBufferReader(writer,Allocator.Temp);reader.ReadNetworkSerializable(out FootballBallSnapshot result);Check(snapshot.Equals(result),"ball, walls, portal and countdown serialize together");
   var visual=ball.GetComponent<FootballPropVisual>();Check(visual&&FootballPropHUD.Instance,"football visual and HUD components attached");
   foreach(var name in new[]{"Turbo","Portal","Wall","DeployedWall"}){var model=Resources.Load<GameObject>("FootballProps/"+name);Check(model&&model.GetComponent<MeshFilter>().sharedMesh.vertexCount>0,"generated model loaded "+name);if(model){Check(!model.GetComponent<Collider>(),"prop model cannot block players "+name);if(SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null)Check(model.GetComponent<Renderer>().sharedMaterials.All(m=>m&&m.shader.isSupported),"all model shaders supported "+name);}}
   foreach(var kind in new[]{SportsProp.Turbo,SportsProp.Portal,SportsProp.Wall})Check(FootballPropHUD.Icon(kind),"distinct icon exists "+kind);
  }
  IEnumerator SpawnChecks(){
   ball.ResetFootballProps();yield return new WaitForFixedUpdate();yield return null;Check(!ball.Props.pickup.countingDown&&ball.Props.pickup.respawn-FootballBall.PropClock>9.8,"first pickup scheduled ten seconds ahead with no early warning");ball.ReviewAdvanceFootballSpawn(5);yield return new WaitForFixedUpdate();yield return null;var visual=ball.GetComponent<FootballPropVisual>();
   {
    var h=visual.Hologram;Check(h&&h.sharedMaterial,"hologram renderer is loaded");Check(Vector3.Distance(h.transform.lossyScale,Vector3.one*4.6f)<.001f,"countdown is four times original size");var m=h.sharedMaterial;var c=m.GetColor("_BaseColor");Check(m.GetFloat("_Surface")==0&&m.GetFloat("_ZWrite")==1&&c.a==1&&c.b>c.g*4&&c.g<.2f,"countdown is opaque dark blue with depth writing");
    var point=ball.PropAnchor(ball.Props.pickup.anchor);Capture("countdown",point+new Vector3(7,6,-10),point+Vector3.up*4);CaptureHud("countdown-stadium");
   }
   // Capture/readback can take over a second with four rendered players. Start
   // a fresh real-time countdown after capture instead of subtracting seconds
   // from a deadline that is also advancing during GPU readback.
   ball.ResetFootballProps();yield return new WaitForFixedUpdate();yield return null;
   int digits=0;double countdownEnd=ball.Props.pickup.respawn;float timeout=Time.realtimeSinceStartup+12;
   while(ball.Props.pickup.kind==SportsProp.None&&Time.realtimeSinceStartup<timeout){
    yield return new WaitForEndOfFrame();if(ball.Props.pickup.countingDown)digits|=1<<visual.CountdownNumber;
   }
   for(int n=5;n>=1;n--)Check((digits&(1<<n))!=0,"blue hologram digit "+n);
   Check(FootballBall.PropClock>=countdownEnd,"first spawn waits its full ten seconds");
   Check(ball.Props.pickup.kind!=SportsProp.None&&!ball.Props.pickup.countingDown,"prop materializes only after countdown");
   var first=ball.Props.pickup;yield return new WaitForSeconds(.2f);Check(ball.Props.pickup.Equals(first),"uncollected pickup remains the only world prop");var seen=ball.Props.pickup.kind;yield return null;Check(visual.PickupModel&&visual.PickupModel.GetComponent<Renderer>().bounds.size.magnitude>.6f,"floating prop has visible world dimensions");
   for(int i=0;i<8;i++){
    var spawn=ball.Props.pickup;var local=ball.Pitch.InverseTransformPoint(ball.PropAnchor(spawn.anchor));Check(Mathf.Abs(local.z-ball.PitchBounds.center.z)<.01f,"pickup on midfield line "+i);
    yield return new WaitForEndOfFrame();Check(visual.PickupModel&&Vector3.Distance(visual.PickupModel.lossyScale,Vector3.one*4.48f)<.001f&&visual.PickupModel.GetComponent<Renderer>().bounds.min.y>ball.PropAnchor(spawn.anchor).y,"enlarged pickup floats wholly above floor "+spawn.kind);
    var point=ball.PropAnchor(spawn.anchor);if(i<3)Capture("large-pickup-"+spawn.kind,point+new Vector3(7,6,-10),point+Vector3.up*3.8f);
    PlaceAt(actor,point+ball.Pitch.TransformDirection(Vector3.forward*2));yield return new WaitForSeconds(.08f);Check(ball.Props.pickup.kind==spawn.kind,"display scale does not enlarge collection radius "+i);
    PlaceAt(actor,ball.PropAnchor(spawn.anchor));yield return new WaitForSeconds(.12f);Check(ball.PropSlot(actor).kind==spawn.kind&&ball.Props.pickup.kind==SportsProp.None,"walking into prop collects once "+i);
    Place(actor,0,-8);ball.ReviewAdvanceFootballSpawn(5);yield return new WaitForFixedUpdate();yield return null;Check(ball.Props.pickup.countingDown,"next cycle announces location "+i);ball.ReviewAdvanceFootballSpawn(5);yield return new WaitForFixedUpdate();yield return null;Check(ball.Props.pickup.kind!=seen&&ball.Props.pickup.kind!=SportsProp.None,"consecutive prop differs "+i);seen=ball.Props.pickup.kind;
   }
   var old=ball.PropSlot(actor);ball.ReviewGiveFootballProp(actor,SportsProp.Turbo);Check(!ball.TryUseProp(actor,old.revision,match.Snapshot.revision,Heading())&&ball.PropSlot(actor).kind==SportsProp.Turbo,"stale inventory request cannot spend replacement");
   Check(!ball.TryUseProp(actor,ball.PropSlot(actor).revision,match.Snapshot.revision-1,Heading()),"old round activation rejected");
  }
  IEnumerator TurboChecks(){
   Place(actor,0,-10);FreeBall(8,10);Check(Use(SportsProp.Turbo),"Turbo accepted");Check(ball.TurboRemaining(actor)>4.8f&&ball.TurboRemaining(actor)<=5,"Turbo duration is five seconds");
   MeasureSpeed(new PlayerCommand{move=Vector2.up,heading=Heading()},6.3f,"boosted walk");
   MeasureSpeed(new PlayerCommand{move=Vector2.up,heading=Heading(),sprint=true},11.025f,"boosted sprint");
   MeasureSpeed(new PlayerCommand{move=Vector2.up,heading=Heading(),dash=true},15.98625f,"boosted dash with existing stamina drain");Check(actor.FootballEffort.Stamina<95,"Turbo does not bypass dash stamina");
   Place(actor,0,-8);yield return Acquire(actor);MeasureSpeed(new PlayerCommand{move=Vector2.up,heading=Heading()},4.725f,"boosted dribble penalty");
   Place(actor,0,-8);yield return Acquire(actor);MeasureSpeed(new PlayerCommand{move=Vector2.up,heading=Heading(),charging=true},3.78f,"boosted charge penalty");
   actor.ResetLocomotion();ball.ReviewGiveFootballProp(actor,SportsProp.Turbo);Check(!ball.TryUseProp(actor,ball.PropSlot(actor).revision,match.Snapshot.revision,Heading())&&ball.PropSlot(actor).kind==SportsProp.Turbo,"active Turbo refuses stacking and keeps prop");
   if(peers.Length==4)Check(ball.TurboRemaining(peers[2])>0&&ball.TurboRemaining(peers[1])==0,"Turbo affects own team only");
   yield return new WaitForSeconds(5.1f);FreeBall(8,10);MeasureSpeed(new PlayerCommand{move=Vector2.up,heading=Heading()},4.2f,"normal pace restored after actual deadline");Check(ball.TurboRemaining(actor)==0,"Turbo expires");
  }
  void MeasureSpeed(PlayerCommand c,float expected,string name){Place(actor,0,-12);for(int i=0;i<35;i++)actor.Simulate(c,.02f);Check(Mathf.Abs(actor.speed-expected)<.5f,name+" actual="+actor.speed.ToString("F2")+" expected="+expected);}
  IEnumerator PortalChecks(){
   FreeBall(4,8);Place(actor,0,8);ball.ReviewGiveFootballProp(actor,SportsProp.Portal);Check(!ball.TryUseProp(actor,ball.PropSlot(actor).revision,match.Snapshot.revision,Heading()),"opposing half rejects Portal");Place(actor,0,0);Check(!ball.TryUseProp(actor,ball.PropSlot(actor).revision,match.Snapshot.revision,Heading()),"centre-line safety margin rejects Portal");
   Place(actor,0,-8);FreeBall(4,8,3,new Vector3(1,0,0));var revision=ball.PropSlot(actor).revision;Check(ball.TryUseProp(actor,revision,match.Snapshot.revision,Heading()),"own-half Portal takes airborne ball");Check(!ball.TryUseProp(actor,revision,match.Snapshot.revision,Heading()),"duplicate portal request rejected");Check(ball.PortalBusy&&!ball.CurrentController&&ball.Body.isKinematic&&!ball.Body.detectCollisions,"portal exclusively reserves football");
   Check(!actor.CanRequestJump&&!actor.TryTackle()&&!ball.TryKick(actor),"receiver cannot jump, tackle or kick during transit");var planted=actor.transform.position;actor.Simulate(new PlayerCommand{move=Vector2.up,dash=true},.1f);Check(Vector3.Distance(planted,actor.transform.position)<.03f,"receiver stays planted during transfer");
   yield return new WaitForSeconds(.2f);Capture("portal-source",ball.Props.source+(Camera.main.transform.position-ball.Props.source).normalized*3.2f,ball.Props.source);CaptureHud("portal-in-play");yield return new WaitForSeconds(.65f);Check(ball.CurrentController==actor&&!ball.PortalBusy&&Vector3.Distance(ball.Body.position,ball.FootPosition(actor))<.03f,"portal completes at feet with one controller");
   ball.ReviewGiveFootballProp(actor,SportsProp.Portal);Check(!ball.TryUseProp(actor,ball.PropSlot(actor).revision,match.Snapshot.revision,Heading())&&ball.PropSlot(actor).kind==SportsProp.Portal,"self-steal rejected without spending");
   FreeBall(4,8);Check(Use(SportsProp.Portal),"interruptible transfer starts");actor.inTransit=true;yield return new WaitForSeconds(.1f);actor.inTransit=false;Check(match.State.Phase==FootballMatchPhase.Cancelled&&!ball.PortalBusy&&!ball.CurrentController&&ball.Body.isKinematic&&ball.Body.detectCollisions,"departing receiver cancels room match and clears reserved ball");Check(ball.PropSlot(actor).kind==SportsProp.None,"interrupted accepted transfer remains consumed");
   match.ClearMatch();Check(peers.Length==0?match.StartTestMatch():match.StartMatch(),"fresh fixture match after departure cancellation");
   var roster=peers.Length==0?new[]{actor}:peers;for(int i=0;i<roster.Length;i++)match.ChooseTeam(roster[i],i%2==0?FootballTeam.A:FootballTeam.B,match.Snapshot.revision);
   match.Selection.Complete(new System.Random(7));match.State.TeamsConfirmed(match.Now-4);match.State.Advance(match.Now);yield return new WaitForFixedUpdate();yield return null;Check(match.Running,"ability fixture resumes in fresh live match");
   Place(actor,0,-8);FreeBall(4,8);Check(Use(SportsProp.Portal),"tackle interruption transfer starts");actor.GetComponent<FootballTackle>().ReceiveHit(Vector3.right);yield return new WaitForSeconds(.1f);Check(!ball.PortalBusy&&!ball.CurrentController,"hit during portal cannot attach ball to fallen receiver");actor.ResetLocomotion();
  }
  IEnumerator WallChecks(){
   Place(actor,0,-8);FreeBall(8,8);ball.ReviewGiveFootballProp(actor,SportsProp.Wall);var hud=FootballPropHUD.Instance;app.view.yaw=Heading();hud.Activate();yield return null;Check(hud.Placing&&hud.PlacementValid&&ball.PropSlot(actor).kind==SportsProp.Wall,"first tap previews without spending");CaptureHud("wall-preview");hud.Cancel();Check(!hud.Placing&&ball.PropSlot(actor).kind==SportsProp.Wall,"cancel retains wall");
   Check(!ball.TryWallPlacement(actor,float.NaN,out _,out _),"NaN wall heading rejected");PlaceAt(actor,ball.PropAnchor(1));Check(!ball.TryWallPlacement(actor,Heading(),out _,out _),"wall footprint beyond touchline rejected");
   Place(actor,0,-8);FreeBall(0,-6);Check(!ball.TryWallPlacement(actor,Heading(),out _,out _),"wall overlapping football rejected");FreeBall(8,8);var stamp=FootballBall.PropClock;Check(Use(SportsProp.Wall),"wall placement accepted");Check(Math.Abs(ball.Props.wallA.until-stamp-15)<.02,"wall lifetime exactly fifteen seconds");
   ball.ReviewGiveFootballProp(actor,SportsProp.Wall);Check(!ball.TryUseProp(actor,ball.PropSlot(actor).revision,match.Snapshot.revision,Heading())&&ball.PropSlot(actor).kind==SportsProp.Wall,"one wall per team without consuming rejected second");
   Place(actor,0,-6.45f);FreeBall(0,-5.8f);ball.RefreshControl(actor);Check(!ball.CurrentController&&!ball.InKickRange(actor)&&!ball.TryKick(actor),"cannot control or kick through a wall");ball.ReviewGiveFootballProp(actor,SportsProp.Portal);Check(!ball.TryUseProp(actor,ball.PropSlot(actor).revision,match.Snapshot.revision,Heading()),"portal cannot deliver through receiver-side wall");
   Place(actor,8,-8);FreeBall(0,-9,.2f,Vector3.forward*25);yield return new WaitForSeconds(.2f);Check(ball.Pitch.InverseTransformDirection(ball.Body.linearVelocity).z<0,"free low kick rebounds");Capture("wall-deployed",Point(7,-14,6),Point(0,-6,2.5f));
   FreeBall(0,-9,7,Vector3.forward*25);yield return new WaitForSeconds(.3f);Check(ball.Pitch.InverseTransformPoint(ball.Body.position).z>ball.PitchBounds.center.z-6,"high lob clears wall");
   FreeBall(4,-9,.2f,Vector3.forward*25);yield return new WaitForSeconds(.3f);Check(ball.Pitch.InverseTransformPoint(ball.Body.position).z>ball.PitchBounds.center.z-6,"side pass clears wall");
   Place(actor,0,-8);yield return Acquire(actor);for(int i=0;i<45;i++)actor.Simulate(new PlayerCommand{move=Vector2.up,heading=Heading()},.02f);Check(ball.CurrentController!=actor&&ball.Pitch.InverseTransformPoint(ball.Body.position).z<ball.PitchBounds.center.z-6,"dribbling cannot carry ball through wall");
   FreeBall(8,8);var physical=ball.ReviewWallCollider(FootballTeam.A);Check(physical&&physical.enabled&&!physical.isTrigger&&Vector3.Distance(physical.size,FootballPropRules.WallHalfExtents*2)<.001f,"deployed wall has solid collider at original dimensions");
   foreach(var mode in new[]{"walk","dash","tackle","jump","hitch"}){
    Place(actor,2,-8);var command=new PlayerCommand{move=Vector2.up,heading=Heading(),dash=mode=="dash",tackle=mode=="tackle",jump=mode=="jump"};bool airborne=false;
    for(int i=0;i<(mode=="hitch"?8:65);i++){actor.Simulate(command,mode=="hitch"?.25f:.02f);command.tackle=command.jump=false;airborne|=actor.Airborne;}
    float z=ball.Pitch.InverseTransformPoint(actor.transform.position).z-ball.PitchBounds.center.z;Check(z< -6.35f&&z> -7,"solid wall stops "+mode+" z="+z.ToString("F3"));if(mode=="jump")Check(airborne,"jump collision exercised airborne capsule");
   }
   Place(actor,0,-4,180);for(int i=0;i<55;i++)actor.Simulate(new PlayerCommand{move=Vector2.up,heading=Heading(180),dash=true},.02f);Check(ball.Pitch.InverseTransformPoint(actor.transform.position).z>ball.PitchBounds.center.z-5.65f,"wall blocks reverse approach");
   Place(actor,4,-8);for(int i=0;i<55;i++)actor.Simulate(new PlayerCommand{move=Vector2.up,heading=Heading()},.02f);Check(ball.Pitch.InverseTransformPoint(actor.transform.position).z>ball.PitchBounds.center.z-5,"players can walk around wall end");
   while(FootballBall.PropClock<ball.Props.wallA.until+.03)yield return null;Check(!ball.WallActive(ball.Props.wallA),"wall disappears after actual fifteen-second lifetime");
   Place(actor,0,-8);for(int i=0;i<55;i++)actor.Simulate(new PlayerCommand{move=Vector2.up,heading=Heading()},.02f);Check(!physical.enabled&&ball.Pitch.InverseTransformPoint(actor.transform.position).z>ball.PitchBounds.center.z-5,"expired wall leaves no invisible player collision");
   ball.ReviewFootballWall(FootballTeam.A,Point(0,-3),Heading(),-.005f);Check(ball.SweepAbilityWall(Point(0,-4,1),Point(0,3,1),out _,out _,FootballBall.PropClock-.02),"contact before expiry still resolves at end of physics step");Check(!ball.SweepAbilityWall(Point(0,-10,1),Point(0,-2,1),out _,out _,FootballBall.PropClock-.02),"contact after expiry passes through");ball.ReviewExpireFootballEffects();yield return null;
   Place(actor,8,-8);ball.ReviewFootballWall(FootballTeam.A,Point(0,-3),Heading(45));FreeBall(0,-7,.4f,Vector3.forward*50);yield return new WaitForSeconds(.12f);var v=ball.Pitch.InverseTransformDirection(ball.Body.linearVelocity);Check(v.x< -10&&v.z<25,"rotated wall reflects using its surface normal");ball.ReviewExpireFootballEffects();yield return null;
   int north=Enumerable.Range(0,ball.GoalCount).First(i=>ball.GoalSign(i)>0);float goal=ball.GoalFront(north)-ball.PitchBounds.center.z;ball.ReviewFootballWall(FootballTeam.A,Point(0,goal-6.5f),Heading());FreeBall(0,goal-8,.6f,Vector3.forward*2000);match.ResetAbilityGoalTracking();int score=match.State.ScoreA;yield return new WaitForSeconds(.08f);Check(match.State.ScoreA==score&&ball.Pitch.InverseTransformPoint(ball.Body.position).z<ball.PitchBounds.center.z+goal-6,"wall contact resolves before a same-step goal");
   ball.ReviewFootballWall(FootballTeam.A,Point(0,-3),Heading(45));PlaceAt(actor,Point(0,-3)+Quaternion.Euler(0,Heading(45),0)*Vector3.back*2);actor.ResetLocomotion();for(int i=0;i<60;i++)actor.Simulate(new PlayerCommand{move=Vector2.up,heading=Heading(45),dash=true},.02f);var rotated=ball.Props.wallA;Check((Quaternion.Inverse(rotated.Rotation)*(actor.transform.position-rotated.position)).z<-.35f,"rotated wall blocks player dash");
   ball.ResetFootballProps();Check(!physical.enabled,"reset removes physical wall immediately");FreeBall(8,8);Place(actor,0,-8);yield return null;
  }
  IEnumerator NetworkChecks(){
   var guest=peers[1];var teammate=peers[2];for(int i=0;i<peers.Length;i++)Place(peers[i],i==0?0:i==1?3:i==2?-5:8,i%2==0?-9:9,i%2==0?0:180);ball.ReviewExpireFootballEffects();yield return Acquire(actor);
   ball.ReviewGiveFootballProp(guest,SportsProp.Portal);yield return new WaitForSeconds(.35f);Signal("guest-portal");yield return Wait("guest-received");Check(ball.CurrentController==guest&&ball.PropSlot(guest).kind==SportsProp.None,"guest owner RPC steals host-held ball once");
   ball.ReviewGiveFootballProp(teammate,SportsProp.Turbo);yield return new WaitForSeconds(.3f);Signal("teammate-turbo");yield return Wait("teammate-boosted");Check(ball.TurboRemaining(actor)>0&&ball.TurboRemaining(teammate)>0&&ball.TurboRemaining(guest)==0,"guest-triggered Turbo follows assigned teams");Signal("turbo-check");yield return Wait("guest-turbo-checked");yield return Wait("rival-turbo-checked");
   FreeBall(10,0);Place(guest,0,8,180);ball.ReviewGiveFootballProp(guest,SportsProp.Wall);yield return new WaitForSeconds(.3f);Signal("guest-wall");yield return Wait("guest-wall-placed");Check(ball.WallActive(ball.Props.wallB)&&ball.PropSlot(guest).kind==SportsProp.None,"guest places host-validated team B wall");
   ball.ReviewGiveFootballProp(peers[3],SportsProp.Wall);yield return new WaitForSeconds(.3f);Signal("rival-wall-reject");yield return Wait("rival-wall-rejected");Check(ball.PropSlot(peers[3]).kind==SportsProp.Wall,"teammate cannot stack the team wall");
   Place(actor,0,-8);FreeBall(10,0);Check(Use(SportsProp.Wall),"opposing team may have its own wall");Signal("walls-live");yield return Wait("guest-walls-seen");yield return Wait("teammate-walls-seen");yield return Wait("rival-walls-seen");
   Signal("guest-wall-move");yield return Wait("guest-wall-blocked");Check(ball.Pitch.InverseTransformPoint(guest.transform.position).z>ball.PitchBounds.center.z+6.35f,"host collision stops guest owner's network dash at own wall");
   Place(actor,0,4);for(int i=0;i<60;i++)actor.Simulate(new PlayerCommand{move=Vector2.up,heading=Heading(),dash=true},.02f);Check(ball.Pitch.InverseTransformPoint(actor.transform.position).z<ball.PitchBounds.center.z+5.65f,"opponent wall blocks host player too");
   ball.ReviewGiveFootballProp(guest,SportsProp.Portal);ball.ReviewGiveFootballProp(actor,SportsProp.Portal);Place(guest,7,10,180);Place(actor,-7,-10);FreeBall(0,0,3);yield return new WaitForSeconds(.4f);
   File.WriteAllText(Path.Combine(shared,"race-at"),(FootballBall.PropClock+1).ToString("R",System.Globalization.CultureInfo.InvariantCulture));Signal("portal-race");double at=double.Parse(File.ReadAllText(Path.Combine(shared,"race-at")),System.Globalization.CultureInfo.InvariantCulture);while(FootballBall.PropClock<at)yield return null;ball.RequestProp(actor,Heading());yield return new WaitForSeconds(.95f);int spent=(ball.PropSlot(actor).kind==SportsProp.None?1:0)+(ball.PropSlot(guest).kind==SportsProp.None?1:0);Check(spent==1&&(ball.CurrentController==actor||ball.CurrentController==guest),"simultaneous portals reserve one ball and consume one slot");yield return Wait("guest-race-done");
  }
  IEnumerator GoalChecks(){
   ball.ResetFootballProps();yield return new WaitForFixedUpdate();yield return null;var pending=ball.Props.pickup;ball.ResetBall(preserveInventory:true);Check(ball.Props.pickup.Equals(pending),"ball reset preserves pending spawn deadline and location");ball.ReviewAdvanceFootballSpawn(10);yield return new WaitForFixedUpdate();yield return null;var retainedPickup=ball.Props.pickup;Check(retainedPickup.kind!=SportsProp.None,"world pickup exists before goal");ball.ReviewExpireFootballEffects();yield return new WaitForFixedUpdate();Place(actor,-7,-10);FreeBall(8,8);ball.ReviewGiveFootballProp(actor,SportsProp.Turbo);Check(ball.RequestProp(actor,Heading()),"Turbo before goal accepted");yield return new WaitForSeconds(.1f);
   ball.ReviewFootballWall(FootballTeam.A,Point(0,-3),Heading());ball.ReviewGiveFootballProp(actor,SportsProp.Portal);var slot=ball.PropSlot(actor);uint round=match.Snapshot.revision;
   var owners=peers.Length==4?peers:new[]{actor};foreach(var owner in owners)if(owner!=actor)ball.ReviewGiveFootballProp(owner,SportsProp.Portal);var held=owners.Select(ball.PropSlot).ToArray();
   int north=Enumerable.Range(0,ball.GoalCount).First(i=>ball.GoalSign(i)>0);float goal=ball.GoalFront(north)-ball.PitchBounds.center.z;
   FreeBall(0,goal-2,.35f);match.ResetAbilityGoalTracking();yield return new WaitForFixedUpdate();ball.Body.position=Point(0,goal+1,.35f);Check(!ball.TryUseProp(actor,slot.revision,round,Heading()),"already crossed goal wins over portal activation");
   Check(match.State.ScoreA>0&&!match.Running&&!ball.PortalBusy&&!ball.Props.wallA.active&&owners.Select((owner,i)=>ball.PropSlot(owner).Equals(held[i])).All(same=>same)&&ball.TurboRemaining(actor)==0,"goal retains every held slot while clearing wall and Turbo");
   yield return null;yield return new WaitForEndOfFrame();var propHud=FootballPropHUD.Instance;Check(propHud&&propHud.GetComponent<CanvasGroup>().alpha>.99f&&!propHud.GetComponent<Button>().interactable,"goal keeps the held prop visible and unavailable until kickoff");CaptureHud("goal-retained-prop");Check(ball.Props.pickup.Equals(retainedPickup)&&ball.GetComponent<FootballPropVisual>().PickupModel&&ball.GetComponent<FootballPropVisual>().PickupModel.gameObject.activeInHierarchy,"same world pickup stays visible throughout goal pause");
   if(peers.Length==4){Signal("goal-held");foreach(var role in new[]{"guest","teammate","rival"})yield return Wait(role+"-goal-held");}
   float end=Time.realtimeSinceStartup+8;while(!match.Running&&Time.realtimeSinceStartup<end)yield return null;Check(match.Running&&ball.Props.pickup.Equals(retainedPickup),"kickoff preserves the same uncollected world prop");yield return new WaitForSeconds(.2f);
   Check(owners.Select((owner,i)=>ball.PropSlot(owner).Equals(held[i])).All(same=>same),"held slots and revisions survive kickoff and live prop initialization");
   yield return new WaitForEndOfFrame();Check(propHud&&propHud.GetComponent<CanvasGroup>().alpha>.99f,"held prop stays visible after kickoff");CaptureHud("kickoff-retained-prop");
   if(peers.Length==4){Signal("goal-resumed");foreach(var role in new[]{"guest","teammate","rival"})yield return Wait(role+"-goal-resumed");}
   ball.ReviewFootballBall(ball.KickoffPosition-Vector3.up*100,Vector3.zero);yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();Check(owners.Select((owner,i)=>ball.PropSlot(owner).Equals(held[i])).All(same=>same),"stranded ball recovery retains every held slot");
   Check(!ball.TryUseProp(actor,slot.revision,round,Heading())&&ball.PropSlot(actor).Equals(slot),"pre-goal round request cannot spend retained slot");
   Place(actor,0,-8);FreeBall(8,8);Check(ball.TryUseProp(actor,slot.revision,match.Snapshot.revision,Heading())&&ball.PropSlot(actor).kind==SportsProp.None,"retained prop is usable once with the current round");yield return new WaitForSeconds(.8f);
   ball.ReviewGiveFootballProp(actor,SportsProp.Wall);match.State.Advance(match.State.Deadline);yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();Check(match.State.Phase==FootballMatchPhase.Finished&&owners.All(owner=>ball.PropSlot(owner).kind==SportsProp.None),"match end clears every held inventory slot");
  }
  IEnumerator Guest(){
   yield return Wait("live",60);var n=actor.GetComponent<NetworkAthlete>();string role=n.OwnerClientId==1?"guest":n.OwnerClientId==2?"teammate":"rival";
   if(role=="guest"){
    yield return Wait("guest-portal",160);Check(ball.PropSlot(actor).kind==SportsProp.Portal,"guest receives held portal slot");Check(ball.InOwnHalf(actor),"guest is in assigned own half");Check(ball.RequestProp(actor,actor.transform.eulerAngles.y),"guest sends owner activation RPC");yield return new WaitForSeconds(1);Check(ball.CurrentController==actor&&!ball.PortalBusy&&ball.PropSlot(actor).kind==SportsProp.None,"guest observes foot possession and consumed slot");Signal("guest-received");
    yield return Wait("turbo-check");Check(ball.TurboRemaining(actor)==0,"opponent is not boosted");Signal("guest-turbo-checked");yield return Wait("guest-wall");Check(ball.RequestProp(actor,actor.transform.eulerAngles.y),"guest sends wall confirmation");yield return new WaitForSeconds(.5f);Check(ball.WallActive(ball.Props.wallB),"guest observes team wall");Signal("guest-wall-placed");
   }else if(role=="teammate"){
    yield return Wait("teammate-turbo",180);Check(ball.RequestProp(actor,actor.transform.eulerAngles.y),"teammate sends Turbo RPC");yield return new WaitForSeconds(.4f);Check(ball.TurboRemaining(actor)>0,"teammate observes boost");Signal("teammate-boosted");
   }else{yield return Wait("turbo-check",190);Check(ball.TurboRemaining(actor)==0,"other opponent is not boosted");Signal("rival-turbo-checked");yield return Wait("rival-wall-reject");ball.RequestProp(actor,actor.transform.eulerAngles.y);yield return new WaitForSeconds(.4f);Check(ball.PropSlot(actor).kind==SportsProp.Wall,"rejected network wall retains slot");Signal("rival-wall-rejected");}
   yield return Wait("walls-live");yield return new WaitForSeconds(.3f);Check(ball.WallActive(ball.Props.wallA)&&ball.WallActive(ball.Props.wallB),"both team walls replicate");CaptureHud(role+"-walls");Signal(role+"-walls-seen");
   Check(ball.ReviewWallCollider(FootballTeam.A)&&ball.ReviewWallCollider(FootballTeam.A).enabled&&ball.ReviewWallCollider(FootballTeam.B)&&ball.ReviewWallCollider(FootballTeam.B).enabled,"both replicated walls have physical collision");
   if(role=="guest"){
    yield return Wait("guest-wall-move");DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,heading=Heading(180),dash=true};yield return new WaitForSeconds(.9f);DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.25f);float z=ball.Pitch.InverseTransformPoint(actor.transform.position).z-ball.PitchBounds.center.z;Check(z>6.35f&&z<7,"guest observes dash blocked by wall without crossing z="+z.ToString("F3"));Signal("guest-wall-blocked");
    yield return Wait("portal-race");double at=double.Parse(File.ReadAllText(Path.Combine(shared,"race-at")),System.Globalization.CultureInfo.InvariantCulture);while(FootballBall.PropClock<at)yield return null;ball.RequestProp(actor,actor.transform.eulerAngles.y);yield return new WaitForSeconds(1.1f);Check(!ball.PortalBusy&&ball.CurrentController,"one controller after contested portal");Signal("guest-race-done");
   }
   yield return Wait("goal-held",60);yield return new WaitForSeconds(.2f);var heldSlot=ball.PropSlot(actor);Check(heldSlot.kind==SportsProp.Portal,"goal retains replicated held prop");Signal(role+"-goal-held");
   yield return Wait("goal-resumed");yield return new WaitForSeconds(.2f);Check(ball.PropSlot(actor).Equals(heldSlot)&&match.Running,"kickoff retains replicated slot and revision");Signal(role+"-goal-resumed");
   yield return Wait("host-done",60);yield return new WaitForSeconds(.3f);Check(!ball.Props.live&&!ball.PortalBusy&&!ball.Props.wallA.active&&!ball.Props.wallB.active&&ball.PropSlot(actor).kind==SportsProp.None,"match reset replicates all cleanup");Signal(role+"-done");
  }
  IEnumerator Gallery(){
   Place(actor,0,-8);var models=new GameObject[3];int i=0;foreach(var name in new[]{"Turbo","Portal","Wall"}){models[i]=Instantiate(Resources.Load<GameObject>("FootballProps/"+name));models[i].transform.SetPositionAndRotation(Point(-2+i*2,-4,.95f),Quaternion.Euler(0,Heading(15),0));i++;}
   yield return new WaitForEndOfFrame();Capture("prop-gallery",Point(1,-10,3.4f),Point(0,-4,.8f));ball.ReviewGiveFootballProp(actor,SportsProp.Wall);yield return null;CaptureHud("stadium-hud");foreach(var m in models)Destroy(m);FootballHUD.Instance.OpenGuide();FootballHUD.Instance.SelectGuide(3);yield return null;CaptureHud("ability-guide");FootballHUD.Instance.CloseMenu();
  }
  void Capture(string name,Vector3 position,Vector3 target){if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;if(!reviewCamera){reviewCamera=new GameObject("Football prop review camera").AddComponent<Camera>();reviewCamera.CopyFrom(Camera.main);reviewCamera.enabled=false;}reviewCamera.transform.position=position;reviewCamera.transform.LookAt(target);reviewCamera.fieldOfView=48;SaveCamera(reviewCamera,name,1280,720);}
  void SaveCamera(Camera camera,string name,int width,int height){var rt=RenderTexture.GetTemporary(width,height,24);var previous=RenderTexture.active;var target=camera.targetTexture;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);camera.targetTexture=target;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);}
  void CaptureHud(string name){if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;var c=FootballHUD.Instance.GetComponentInParent<Canvas>();var mode=c.renderMode;var camera=c.worldCamera;float distance=c.planeDistance;c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=Camera.main;c.planeDistance=1;Canvas.ForceUpdateCanvases();SaveCamera(Camera.main,name,Screen.width,Screen.height);c.renderMode=mode;c.worldCamera=camera;c.planeDistance=distance;Canvas.ForceUpdateCanvases();}
  void Finish(){DevelopmentProbe.TurnCommandActive=false;DevelopmentProbe.TurnCommand=default;if(app){app.view.mode=originalCamera;PlayerPrefs.SetInt("camera",originalCamera);PlayerPrefs.Save();}File.AppendAllText(Path.Combine(folder,"results.txt"),"FOOTBALL_PROPS_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
