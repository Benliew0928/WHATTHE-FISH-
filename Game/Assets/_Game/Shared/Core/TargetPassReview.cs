#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 public sealed class TargetPassReview:MonoBehaviour {
  string folder,shared;bool failed,basket;AppRoot app;Athlete actor,mate,other,enemy;Athlete[] roster;BasketballBall bb;FootballBall fb;Transform field;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-targetPassReview");if(i>=0&&i+1<args.Length)new GameObject("Target pass review").AddComponent<TargetPassReview>().folder=args[i+1];}
  void Check(bool value,string label){failed|=!value;File.AppendAllText(Path.Combine(folder,"results.txt"),(value?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Until(Func<bool> condition,string label,float seconds=12){float end=Time.realtimeSinceStartup+seconds;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Check(condition(),label);}
  void Signal(string name)=>File.WriteAllText(Path.Combine(shared,name),"ready");
  bool Has(string name)=>File.Exists(Path.Combine(shared,name));
  bool Authority=>basket?bb.Authority:fb.HasAuthority;
  Athlete Holder=>basket?bb.Holder:fb.CurrentController;
  uint Possession=>basket?bb.Defense.play:fb.PassPossession;
  bool PassTo(Athlete from,Athlete to,uint token)=>basket?bb.TryPassTo(from,to,token):fb.TryPassTo(from,to,token);
  FootballTeam Team(Athlete a)=>basket?BasketballMatch.Instance.TeamOf(a):FootballMatch.Instance.TeamOf(a);
  void Place(Athlete a,Vector3 local){var p=field.TransformPoint(local);bool enabled=a.capsule.enabled;a.capsule.enabled=false;a.transform.SetPositionAndRotation(p,field.rotation);a.ResetLocomotion();a.capsule.enabled=enabled;var n=a.GetComponent<NetworkTransform>();if(n&&n.IsSpawned&&Authority)n.Teleport(p,a.transform.rotation,a.transform.localScale);Physics.SyncTransforms();}
  void Fixture(Athlete from=null){
   from=from?from:actor;app.view.ClearMatchInput();DevelopmentProbe.TurnCommand=default;if(basket)bb.ResetHome();else fb.ResetBall(true);
   for(int i=0;i<roster.Length;i++)Place(roster[i],new Vector3(5,.07f,-8+i*2));
   Place(from,new Vector3(0,.07f,-5));Place(from==actor?mate:actor,new Vector3(2.8f,.07f,3));Place(other,new Vector3(-2,.07f,1));Place(enemy,new Vector3(-4,.07f,5));
   if(basket){bb.autoPickup=true;bb.GiveMatchPossession(from);BasketballMatch.Instance.Control(from);}else {fb.ReviewFootballBall(from.transform.position+from.transform.forward*.8f+Vector3.up*fb.WorldRadius,Vector3.zero);fb.RefreshControl(from);}
   app.view.yaw=field.eulerAngles.y;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=app.view.yaw};
  }
  Vector2 ScreenPoint(Athlete a){var cam=Camera.main;cam.transform.position=field.TransformPoint(new Vector3(0,9,-11));cam.transform.LookAt(field.TransformPoint(new Vector3(0,0,1)));return cam.WorldToScreenPoint(a.transform.position+Vector3.up*.9f);}
  bool Tap(Athlete a,int pointer=12){var point=ScreenPoint(a);app.view.TargetPassPointerDown(pointer,point);return app.view.TargetPassPointerUp(pointer,point);}
  void InspectPick(Athlete a){
   var p=ScreenPoint(a);var ray=Camera.main.ScreenPointToRay(p);var text=$"screen={p} pick={app.view.TeammateAtScreen(p)} actor={actor.transform.position} receiver={a.transform.position} teams={Team(actor)}/{Team(a)} receiverRoam={a.BasketballFreeRoam} valid={(basket?bb.ValidDirectReceiver(actor,a):fb.ValidPassReceiver(actor,a))}\n";
   foreach(var who in Athlete.Active)if(who&&who.visual)foreach(var r in who.visual.GetComponentsInChildren<SkinnedMeshRenderer>())text+=$"{who.name} id={TeammatePassing.Id(who)} bounds={r.bounds} hit={r.bounds.IntersectRay(ray,out float d)} depth={d}\n";
   var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=p},hits);foreach(var h in hits)text+="UI "+h.gameObject.name+"\n";File.WriteAllText(Path.Combine(folder,"pick.txt"),text);
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);shared=Directory.GetParent(folder).FullName;File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   yield return Until(()=>DevelopmentProbe.ReadyToReviewSport,"sport ready",85);app=AppRoot.Instance;actor=app.LocalAthlete;basket=app.SelectedSport==SportId.Basketball;bb=BasketballBall.Active;fb=FootballBall.Instance;field=basket?bb.transform.parent:fb.Pitch;
   DevelopmentProbe.Driving=false;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   yield return Until(()=>!RoomMatchStart.Travelling&&!app.PendingSelection.HasValue&&!app.environments.GetComponent<SkySailStreaming>().Busy&&!app.Menu.IsTransitioning&&!app.view.InputBlocked,"world ready for match start",30);
   if(!Authority){yield return Guest();Finish();yield break;}
   bool online=app.rooms.Connected;
   if(basket){
    if(online){Check(BasketballBots.Instance.FillRoom(3),"fill basketball 3v3");yield return new WaitForSeconds(.5f);roster=BasketballBots.Roster;Check(BasketballMatch.Instance.StartMatch(),"start basketball");}
    else {roster=new Athlete[6];roster[0]=actor;for(int i=1;i<6;i++){roster[i]=Instantiate(app.athletePrefab).GetComponent<Athlete>();roster[i].Setup();}Check(BasketballMatch.Instance.ReviewStart(roster,600,1),"start offline basketball");}
   }else {
    if(online)Check(FootballBots.Instance.FillRoom(3),"fill football 3v3");else Check(FootballBots.Instance.StartSolo(3),"solo football 3v3");
    yield return Until(()=>app.view.active&&!app.view.InputBlocked&&!actor.inTransit&&!app.Menu.IsTransitioning,"arrival complete");roster=FootballBots.Roster;
    var rules=FootballMatch.Instance.rules;FootballMatch.Instance.rules=Instantiate(rules);FootballMatch.Instance.rules.teamSelectionSeconds=online?2:.3f;FootballMatch.Instance.rules.kickoffSeconds=.25f;Check(FootballMatch.Instance.StartMatch(),"start football");
   }
   roster=roster.OrderBy(a=>a==actor?0:a.GetComponent<NetworkAthlete>() is NetworkAthlete n&&n.BotSlot.Value==0?1:2).ThenBy(TeammatePassing.Id).ToArray();
   foreach(var a in roster){var b=a.GetComponent<BasketballBot>();if(b)b.enabled=false;var f=a.GetComponent<FootballBot>();if(f)f.enabled=false;}
   for(int i=0;i<roster.Length;i++){bool chosen=basket?BasketballMatch.Instance.ChooseTeam(roster[i],i<3?FootballTeam.A:FootballTeam.B,BasketballMatch.Instance.Snapshot.revision):FootballMatch.Instance.ChooseTeam(roster[i],i<3?FootballTeam.A:FootballTeam.B,FootballMatch.Instance.Snapshot.revision);Check(chosen,"team assigned "+i);}
   if(!basket){if(!online)Check(FootballMatch.Instance.ConfirmSoloTeam(),"solo teams confirmed");yield return Until(()=>FootballMatch.Instance.KickoffReady,"kickoff ready");Check(FootballMatch.Instance.TryKickoff(FootballMatch.Instance.KickoffActor,90,.3f,FootballMatch.Instance.Snapshot.revision),"kickoff accepted");}
   yield return Until(()=>basket?BasketballMatch.Instance.Running:FootballMatch.Instance.Running,"regulation",20);mate=roster[1];other=roster[2];enemy=roster[3];
   Fixture();yield return new WaitForSeconds(.3f);Check(Holder==actor,"fixture possession");
   Check(!PassTo(actor,enemy,Possession)&&!PassTo(actor,actor,Possession)&&!PassTo(actor,null,Possession)&&!PassTo(actor,mate,Possession+1),"opponent self missing and stale rejected");
   var original=mate.transform.position;Place(mate,new Vector3(0,.07f,40));Check(!PassTo(actor,mate,Possession),"out of range rejected");Place(mate,field.InverseTransformPoint(original));
   yield return GestureChecks();
   Fixture();yield return new WaitForSeconds(.2f);Check(!Tap(mate),"first tap selects without throwing");Check(Tap(mate),"second tap queues exact teammate");
   Check(!app.view.BeginFootballPass()&&!app.view.BeginPass()&&!app.view.RequestTargetPass(other),"queued pass cannot race another pass");
   yield return Until(()=>Holder==mate,"double tap physically reaches selected teammate",5);yield return Capture("received");
   Fixture();yield return new WaitForSeconds(.25f);Place(other,new Vector3(0,.07f,-1));Place(mate,new Vector3(.6f,.07f,4));
   Check((basket?bb.SelectPassReceiver(actor,field.eulerAngles.y):fb.SelectPassReceiver(actor,field.eulerAngles.y))==other,"button aim still selects nearer lane teammate");
   Check(app.view.RequestTargetPass(mate),"direct target bypasses nearer lane choice");if(basket){yield return Until(()=>bb.PassAim.phase==2,"gather committed");Check(bb.PassReceiver==mate,"authority preserves exact identity");}else yield return null;
   if(!online){
    // Reach a steady running speed before release. A receiver who accelerates
    // after release is a different prediction problem from a moving receiver.
    Fixture();yield return new WaitForSeconds(.3f);for(int i=0;i<roster.Length;i++)if(roster[i]!=actor&&roster[i]!=mate)Place(roster[i],new Vector3(-5,.07f,-9+i*.7f));Place(mate,new Vector3(-3,.07f,3));
    var running=new PlayerCommand{move=Vector2.right,heading=field.eulerAngles.y};float end=Time.time+.35f;
    while(Time.time<end){mate.Simulate(running,Time.deltaTime);yield return null;}
    Check(PassTo(actor,mate,Possession),"moving receiver pass accepted speed="+mate.Motor.Velocity.magnitude);end=Time.time+3;
    while(Holder!=mate&&Time.time<end){mate.Simulate(running,Time.deltaTime);var body=basket?bb.Body:fb.Body;File.AppendAllText(Path.Combine(folder,"moving.csv"),$"{Time.time:F3},{mate.transform.position:F3},{mate.Motor.Velocity:F3},{body.position:F3},{body.linearVelocity:F3}\n");yield return null;}
    Check(Holder==mate,"lead pass caught by running teammate");
   }
   if(basket){Fixture();yield return new WaitForSeconds(.3f);uint count=bb.PassCount;Check(PassTo(actor,mate,Possession),"direct gather starts");mate.BasketballFreeRoam=true;yield return new WaitForSeconds(.7f);Check(Holder==actor&&bb.PassCount==count,"invalid receiver during gather keeps possession");mate.BasketballFreeRoam=false;}
   if(online){Fixture(mate);yield return new WaitForSeconds(.4f);Signal("guest-turn");yield return Until(()=>Has("guest-done"),"guest completed",20);Check(Holder==actor,"guest targeted pass reaches host");Signal("host-done");yield return new WaitForSeconds(.5f);}
   Finish();
  }
  IEnumerator GestureChecks(){
   Check(app.view.TeammateAtScreen(ScreenPoint(mate))==mate,"model hit selects teammate");Check(app.view.TeammateAtScreen(ScreenPoint(enemy))==null,"opponent model excluded");
   Check(!Tap(mate)&&!app.view.ReadCommand().targetedPass,"single tap does not pass");Check(!Tap(other)&&!app.view.ReadCommand().targetedPass,"different teammate does not complete double tap");app.view.ClearMatchInput();
   var p=ScreenPoint(mate);app.view.TargetPassPointerDown(12,p);app.view.TargetPassPointerMove(12,p+Vector2.right*100);Check(!app.view.TargetPassPointerUp(12,p),"drag away and return is rejected");Check(!Tap(mate),"drag cannot prime next tap");app.view.ClearMatchInput();
   Tap(mate);p=ScreenPoint(mate);app.view.TargetPassPointerDown(12,p);Check(!app.view.TargetPassPointerUp(13,p),"unrelated finger cannot release");app.view.ClearMatchInput();
   Tap(mate);yield return new WaitForSeconds(PlayerView.TeammateDoubleTapSeconds+.08f);Check(!Tap(mate),"expired double tap rejected");app.view.ClearMatchInput();
   p=ScreenPoint(mate);app.view.TargetPassPointerDown(12,p);yield return new WaitForSeconds(PlayerView.TeammateTapSeconds+.08f);p=ScreenPoint(mate);Check(!app.view.TargetPassPointerUp(12,p),"long press rejected");
   Tap(mate);app.view.InputBlocked=true;Check(!Tap(mate)&&!app.view.RequestTargetPass(mate),"modal blocks gesture and command");app.view.InputBlocked=false;app.view.ClearMatchInput();
   Tap(mate);uint before=Possession;Fixture();yield return new WaitForSeconds(.2f);Check(Possession!=before&&!Tap(mate),"new possession cannot complete old gesture");app.view.ClearMatchInput();
   var control=basket?(Component)BasketballHUD.Instance.GetComponentsInChildren<BasketballShootButton>().First(b=>b.pass):FootballHUD.Instance.GetComponentInChildren<FootballPassButton>();
   p=RectTransformUtility.WorldToScreenPoint(null,control.transform.position);app.view.TargetPassPointerDown(12,p);Check(!app.view.TargetPassPointerUp(12,p)&&!app.view.ReadCommand().targetedPass,"HUD touch cannot become model pass");
   var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=8;wall.transform.position=Vector3.Lerp(Camera.main.transform.position,mate.transform.position+Vector3.up, .8f);wall.transform.localScale=Vector3.one*2;Physics.SyncTransforms();Check(app.view.TeammateAtScreen(ScreenPoint(mate))==null,"architecture occludes model tap");Destroy(wall);yield return null;
   // Disabled remote colliders are intentional; picking still uses visual bounds.
   bool enabled=mate.capsule.enabled;mate.capsule.enabled=false;Check(app.view.TeammateAtScreen(ScreenPoint(mate))==mate,"picking works without receiver collider");mate.capsule.enabled=enabled;
   yield return Capture("target-view");
   if(basket){BasketballHUD.Instance.SelectGuide(1);BasketballHUD.Instance.OpenGuide();}else FootballHUD.Instance.OpenGuide();yield return Capture("controls-guide");if(basket)BasketballHUD.Instance.CloseMenu();else FootballHUD.Instance.CloseMenu();
  }
  IEnumerator Guest(){
   yield return Until(()=>Has("guest-turn"),"host fixture ready",110);mate=Athlete.Active.First(a=>a&&a.GetComponent<NetworkAthlete>() is NetworkAthlete n&&n.IsSpawned&&n.IsPlayerObject&&!n.IsOwner);yield return Until(()=>Holder==actor&&!app.view.InputBlocked,"guest possession snapshot");
   Check(!PassTo(actor,mate,Possession),"replica cannot author physics");var net=actor.GetComponent<NetworkAthlete>();uint round=basket?BasketballMatch.Instance.Snapshot.revision:FootballMatch.Instance.Snapshot.revision;
   net.TargetPassRpc(TeammatePassing.Id(mate),Possession+1,round,fb?fb.PropBallRevision:0);net.TargetPassRpc(TeammatePassing.Id(mate),Possession,round+1,fb?fb.PropBallRevision:0);net.TargetPassRpc(ulong.MaxValue,Possession,round,fb?fb.PropBallRevision:0);yield return new WaitForSeconds(.4f);Check(Holder==actor,"host rejects stale possession stale round and unknown receiver RPCs");
   InspectPick(mate);Check(app.view.TeammateAtScreen(ScreenPoint(mate))==mate,"guest can pick host model");Check(!Tap(mate)&&Tap(mate),"guest double tap queues receiver ID");
   yield return Until(()=>Holder==mate,"guest sees authoritative receipt",6);Signal("guest-done");yield return Until(()=>Has("host-done"),"host verifies receipt");
  }
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();ScreenPoint(mate);var camera=Camera.main;
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}Canvas.ForceUpdateCanvases();
   var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var old=RenderTexture.active;var target=camera.targetTexture;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);camera.targetTexture=target;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}
  }
  void Finish(){app?.view.ClearMatchInput();File.AppendAllText(Path.Combine(folder,"results.txt"),"TARGET_PASS_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
