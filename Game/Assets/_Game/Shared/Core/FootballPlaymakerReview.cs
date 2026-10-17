#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class FootballPlaymakerReview:MonoBehaviour {
  string folder;bool failed;int checks;AppRoot app;FootballMatch match;FootballBall ball;Athlete passer,mate,enemy;Vector3 forward;float heading;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-footballPlaymakerReview");if(i>=0&&i+1<a.Length)new GameObject("Playmaker checks").AddComponent<FootballPlaymakerReview>().folder=a[i+1];}
  void Check(bool value,string name){checks++;failed|=!value;File.AppendAllText(Path.Combine(folder,"results.txt"),(value?"PASS ":"FAIL ")+name+"\n");}
  IEnumerator Until(Func<bool> test,string name,float seconds=12){float end=Time.realtimeSinceStartup+seconds;while(!test()&&Time.realtimeSinceStartup<end)yield return null;Check(test(),name);}
  void BotsOff(){foreach(var a in FootballBots.Roster){var b=a.GetComponent<FootballBot>();if(b)b.enabled=false;}}
  FootballPlayerRecord Record(Athlete a)=>match.PlayerRecords.First(p=>p.id==FootballBots.Id(a));
  void Place(Athlete a,Vector3 p,float yaw){bool enabled=a.capsule.enabled;a.capsule.enabled=false;a.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));a.ResetLocomotion();var n=a.GetComponent<NetworkAthlete>();if(n&&n.IsSpawned&&n.IsServer)a.GetComponent<NetworkTransform>().Teleport(p,a.transform.rotation,a.transform.localScale);a.capsule.enabled=enabled;Physics.SyncTransforms();}
  void Fixture(){ball.ResetBall(true);var centre=ball.KickoffPosition;centre.y=passer.transform.position.y;int i=0;foreach(var a in FootballBots.Roster)Place(a,centre+ball.Pitch.right*15+forward*(-10-i++*2),heading);Place(passer,centre-forward*4,heading);Place(mate,centre+forward*5,heading+180);ball.ReviewFootballBall(passer.transform.position+forward*.8f+Vector3.up*ball.WorldRadius,Vector3.zero);ball.RefreshControl(passer);match.ResetAbilityGoalTracking();app.view.yaw=heading;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=heading};}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");yield return Until(()=>AppRoot.Instance&&AppRoot.Instance.Exploring&&FootballMatch.Instance&&AppRoot.Instance.LocalAthlete,"football ready",90);
   app=AppRoot.Instance;match=FootballMatch.Instance;ball=FootballBall.Instance;if(!app||!match||!ball){Finish();yield break;}DevelopmentProbe.Driving=false;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   if(!match.Authority){yield return Guest();Finish();yield break;}
   bool online=app.rooms.Connected;var old=match.rules;match.rules=Instantiate(old);match.rules.teamSelectionSeconds=online?2:.25f;match.rules.kickoffSeconds=.25f;
   if(online)yield return Until(()=>FootballBots.Roster.Length==2&&!app.view.InputBlocked,"both humans arrive before room fill");
   for(int size=2;size<=(online?2:5);size++){
    if(online)Check(FootballBots.Instance.FillRoom(2),"fill room to 2v2");else Check(FootballBots.Instance.StartSolo(size),"VS AI "+size+"v"+size);
    yield return Until(()=>app.view.active&&!app.view.InputBlocked&&!app.LocalAthlete.inTransit&&!app.Menu.IsTransitioning,"arrival releases player input "+size);Check(match.StartMatch(),"start match "+size);BotsOff();
    if(online)foreach(var a in FootballBots.Roster)match.ChooseTeam(a,FootballBots.IsBot(a)?FootballTeam.B:FootballTeam.A,match.Snapshot.revision);
    else {match.RequestTeam(FootballTeam.A);Check(match.ConfirmSoloTeam(),"explicit solo start");}
    yield return Until(()=>match.KickoffReady,"kickoff ready");Check(match.TryKickoff(match.KickoffActor,90,.3f,match.Snapshot.revision),"kickoff");
    foreach(var participant in FootballBots.Roster)MatchBoundaryChecks.Verify(participant,false,true,Check);
    passer=app.LocalAthlete;mate=FootballBots.Roster.First(a=>a!=passer&&match.TeamOf(a)==match.TeamOf(passer));enemy=FootballBots.Roster.First(a=>match.TeamOf(a)!=match.TeamOf(passer));forward=ball.Pitch.TransformDirection(Vector3.forward).normalized;heading=Mathf.Atan2(forward.x,forward.z)*Mathf.Rad2Deg;
    Fixture();yield return null;Check(ball.CurrentController==passer&&ball.CanPass(passer),"pass possession ready "+size);
    Check(match.PlayerRecords.Count()==size*2,"all players have stat records "+size);
    Check(ball.SelectPassReceiver(passer,heading)==mate,"nearest teammate in lane "+size);Check(ball.SelectPassReceiver(passer,heading+90)==null,"outside aim width excluded "+size);
    var original=mate.transform.position;Place(mate,original+ball.Pitch.right*2.5f,heading);Check(ball.SelectPassReceiver(passer,heading)==null,"fixed lane width bounded "+size);Place(mate,original,heading);
    if(!online&&size==2){
     // Drive the same pointer across actual hitboxes and back; independent press too.
     var dash=FootballHUD.Instance.GetComponentsInChildren<FootballEffortButton>().First(b=>!b.pressure);var kick=FootballHUD.Instance.Kick;
     Vector2 Centre(Component c)=>RectTransformUtility.WorldToScreenPoint(null,((RectTransform)c.transform).TransformPoint(((RectTransform)c.transform).rect.center));
     var touch=new PointerEventData(EventSystem.current){pointerId=23,button=PointerEventData.InputButton.Left,position=Centre(dash)};
     dash.Refresh();dash.OnPointerDown(touch);Check(app.view.DashHeld,"primary pointer begins dash");touch.position=Centre(kick);dash.OnDrag(touch);Check(app.view.DashHeld&&app.view.Charging,"diagonal swipe charges without ending dash");
     passer.Simulate(new PlayerCommand{dash=true,charging=true,move=Vector2.up,heading=heading},.06f);Check(passer.FootballEffort.Dashing&&passer.Charging,"authoritative dash continues while charging");
     touch.position=Centre(dash);dash.OnDrag(touch);Check(app.view.DashHeld&&!app.view.Charging&&app.view.ReadCommand().kick,"return to dash fires shot while retaining dash");dash.OnPointerUp(touch);dash.GetComponent<FootballActionSwipe>().OnPointerUp(touch);Check(!app.view.DashHeld,"lift releases dash");
     Fixture();yield return null;
     var pass=FootballHUD.Instance.GetComponentInChildren<FootballPassButton>();var swipe=dash.GetComponent<FootballActionSwipe>();
     touch.position=Centre(dash);dash.Refresh();dash.OnPointerDown(touch);touch.position=Centre(pass);swipe.OnDrag(touch);Check(app.view.DashHeld&&app.view.FootballPassAiming&&app.view.FootballPassTarget==mate,"dash swipe holds pass with receiver lock");
     swipe.OnPointerUp(new PointerEventData(EventSystem.current){pointerId=24});Check(app.view.DashHeld&&app.view.FootballPassAiming,"unrelated finger cannot release routed pass");
     swipe.OnCancel(touch);Check(!app.view.DashHeld&&!app.view.FootballPassAiming,"swipe cancellation clears all owned holds");
     touch.position=Centre(kick);kick.Refresh();kick.OnPointerDown(touch);touch.position=Centre(pass);kick.GetComponent<FootballActionSwipe>().OnDrag(touch);Check(!app.view.Charging&&app.view.FootballPassAiming&&!app.view.ReadCommand().kick,"shoot swipe to pass cancels shot without firing");kick.GetComponent<FootballActionSwipe>().OnCancel(touch);
     touch.position=Centre(pass);pass.OnPointerDown(touch);touch.position=Centre(dash);pass.GetComponent<FootballActionSwipe>().OnDrag(touch);Check(app.view.DashHeld&&!app.view.FootballPassAiming&&app.view.ReadCommand().footballPass,"pass swipe to dash releases pass and begins dash");pass.GetComponent<FootballActionSwipe>().OnPointerUp(touch);Fixture();yield return null;
     Check(app.view.BeginDash(50)&&app.view.BeginKick(51),"separate shoot finger accepted during dash");app.view.CancelKick(51);Check(app.view.DashHeld,"cancelling shoot preserves independent dash");app.view.EndDash(50);Fixture();yield return null;
     Check(!FootballHUD.Instance.GetComponentInChildren<JumpButton>(),"pass replaces football jump button");yield return Capture("controls");
     int localSteps=0,foreignSteps=0;void Hear(FootballSound cue,Vector3 p,float g,string reason,uint id){if((cue==FootballSound.Walk||cue==FootballSound.Run||cue==FootballSound.Cut)&&reason!="replay"){if(Vector3.Distance(p,passer.transform.position)<2)localSteps++;else foreignSteps++;}}
     FootballAudio.Trace+=Hear;float end=Time.time+1.6f;while(Time.time<end){passer.Simulate(new PlayerCommand{move=Vector2.up,heading=heading},Time.deltaTime);enemy.Simulate(new PlayerCommand{move=Vector2.up,heading=heading},Time.deltaTime);yield return null;}FootballAudio.Trace-=Hear;
     Check(localSteps>0&&foreignSteps==0,"moving local and opponent produce only local footsteps");Fixture();yield return null;
    }
    if(!online&&size==2){yield return SwipeDefense();Fixture();yield return null;}
    Check(app.view.BeginFootballPass(),"pass hold begins");Check(app.view.FootballPassTarget==mate,"receiver highlight locks teammate");yield return Capture("pass-lock-"+size);
    if(!online&&size==2){foreach(int mode in new[]{0,1,2}){app.view.mode=mode;yield return new WaitForSeconds(.2f);yield return Capture("pass-lock-camera-"+mode);var marker=FootballHUD.Instance.GetComponentInChildren<FootballPassMarker>();Check(marker.PathVisible&&marker.LockBlend>.9f&&Vector3.ProjectOnPlane(marker.DisplayedEndpoint-mate.transform.position,Vector3.up).magnitude<.15f,"route and halo track receiver in camera "+mode);Check(!FootballHUD.Instance.GetComponentsInChildren<Text>().Any(t=>t.text.StartsWith("LOCKED  #")),"no floating lock badge in camera "+mode);}yield return PassTransitions();app.view.mode=1;}
    app.view.EndFootballPass();
    // The normal offline/network driver consumes this edge on the next frame.
    yield return Until(()=>ball.CurrentController==mate,"physical pass received "+size,5);Check(Record(passer).passes==1,"one successful new Pass action "+size);
    if(online){yield return Until(()=>ball.CurrentController==passer&&Record(mate).passes==1,"guest reliable pass returns to host",8);}
    if(size==2){
     var scorer=online?passer:mate;match.State.Advance(match.State.Deadline);int index=Enumerable.Range(0,ball.GoalCount).First(i=>ball.GoalSign(i)>0);var g=ball.GoalBounds(index);var local=new Vector3(g.center.x,ball.PitchBounds.max.y+.02f,ball.GoalFront(index)-5);Place(scorer,ball.Pitch.TransformPoint(local),heading);ball.FollowController(scorer);match.ResetAbilityGoalTracking();
     Check(scorer.TryKick(.5f),"receiver shoots at goal");yield return Until(()=>match.GoalSequence,"physical scored goal",4);Check(Record(scorer).goals==1&&Record(online?mate:passer).assists==1,"direct pass within five seconds earns assist");
     yield return Until(()=>FootballMVP.Instance&&FootballMVP.Instance.Visible,"MVP after final replay",15);Check(FootballMVP.Instance&&FootballMVP.Instance.GetComponentsInChildren<Image>().Count(i=>i.name=="MVP player illustration"&&i.sprite)==1,"MVP shows one 2D character illustration");Check(match.MVP.id==FootballBots.Id(scorer),"goal scorer is MVP");yield return new WaitForSeconds(.5f);yield return PortraitCheck();yield return Capture("mvp");
     var statsButton=FootballMVP.Instance.GetComponentsInChildren<Button>().First(b=>b.name=="Toggle match stats");statsButton.onClick.Invoke();yield return Capture("match-stats");FootballMVP.Instance.GetComponentsInChildren<Button>().First(b=>b.name=="Close MVP").onClick.Invoke();foreach(var participant in FootballBots.Roster)MatchBoundaryChecks.Verify(participant,false,false,Check);Check(!app.view.InputBlocked&&!FootballMVP.Presenting,"closing MVP restores input and camera ownership");
    }else{
     Fixture();yield return null;Place(enemy,passer.transform.position+forward*4,heading+180);int intercepted=Record(enemy).interceptions;Check(ball.TryPass(passer,heading),"blocked pass released");yield return Until(()=>Record(enemy).interceptions==intercepted+1,"enemy pass block counted",4);yield return new WaitForSeconds(.3f);Check(Record(enemy).interceptions==intercepted+1&&Record(passer).passes==1,"interception exactly once and no false completed pass");
     Fixture();yield return null;Check(passer.TryKick(.05f),"normal shot released");yield return Until(()=>ball.CurrentController==mate,"teammate receives shot",4);Check(Record(passer).passes==1,"received shots never add passes "+size);
    }
    if(online)yield return new WaitForSeconds(2);match.ClearMatch();yield return null;
   }
   match.rules=old;Finish();
  }
  IEnumerator PassTransitions(){
   var marker=FootballHUD.Instance.GetComponentInChildren<FootballPassMarker>();
   int rate=Application.targetFrameRate;
   foreach(int fps in new[]{20,60,120}){
    Application.targetFrameRate=fps;app.view.yaw=heading;yield return new WaitForSeconds(.6f);
    Check(marker.PathVisible&&marker.LockBlend>.98f,"energy path locks at "+fps+" fps cap");
    var locked=marker.DisplayedEndpoint;app.view.yaw=heading+90;yield return null;yield return new WaitForEndOfFrame();
    var free=passer.transform.position+Quaternion.Euler(0,app.view.yaw,0)*Vector3.forward*12;free.y=locked.y;
    Check(!app.view.FootballPassTarget&&Vector3.Distance(marker.DisplayedEndpoint,free)>.1f&&marker.LockBlend>0,"unlock eases rather than snapping "+fps);
    yield return new WaitForSeconds(.6f);Check(Vector3.Distance(marker.DisplayedEndpoint,free)<.15f&&marker.LockBlend<.02f,"free aim settles "+fps);yield return Capture("pass-free-path-"+fps);
    app.view.yaw=heading;yield return null;yield return new WaitForEndOfFrame();Check(marker.LockBlend<.99f&&Vector3.Distance(marker.DisplayedEndpoint,locked)>.1f,"acquisition eases rather than snapping "+fps);
    yield return new WaitForSeconds(.6f);Check(marker.LockBlend>.98f&&Vector3.Distance(marker.DisplayedEndpoint,locked)<.15f,"receiver lock settles "+fps);yield return Capture("pass-locked-path-"+fps);
   }
   Application.targetFrameRate=rate;app.view.CancelFootballPass();yield return null;yield return new WaitForEndOfFrame();Check(!marker.PathVisible,"cancel removes pass ribbon");
   app.view.BeginFootballPass();yield return new WaitForSeconds(.2f);ball.ResetBall(true);yield return null;yield return new WaitForEndOfFrame();Check(!marker.PathVisible,"possession loss removes pass ribbon");Fixture();yield return null;app.view.BeginFootballPass();yield return new WaitForSeconds(.3f);
  }
  IEnumerator SwipeDefense(){
   Fixture();ball.ResetBall(true);Place(enemy,passer.transform.position+forward*1.8f,heading+180);ball.ReviewFootballBall(ball.FootPosition(enemy),Vector3.zero);ball.RefreshControl(enemy);yield return null;
   Vector2 Centre(Component c)=>RectTransformUtility.WorldToScreenPoint(null,((RectTransform)c.transform).TransformPoint(((RectTransform)c.transform).rect.center));
   var dash=FootballHUD.Instance.GetComponentsInChildren<FootballEffortButton>().First(b=>!b.pressure);var pressure=FootballHUD.Instance.GetComponentsInChildren<FootballEffortButton>().First(b=>b.pressure);var tackle=FootballHUD.Instance.GetComponentInChildren<TackleButton>();
   var touch=new PointerEventData(EventSystem.current){pointerId=33,button=PointerEventData.InputButton.Left,position=Centre(dash)};
   dash.Refresh();dash.OnPointerDown(touch);touch.position=Centre(pressure);dash.GetComponent<FootballActionSwipe>().OnDrag(touch);Check(app.view.DashHeld&&app.view.PressureHeld,"dash swipe begins pressure while keeping dash");
   touch.position=Centre(tackle);dash.GetComponent<FootballActionSwipe>().OnDrag(touch);Check(app.view.DashHeld&&app.view.PressureHeld&&app.view.ReadCommand().tackle,"dash through pressure to tackle keeps both continuous holds");dash.GetComponent<FootballActionSwipe>().OnPointerUp(touch);Check(!app.view.DashHeld&&!app.view.PressureHeld,"lift clears routed defensive holds");
   touch.position=Centre(tackle);tackle.Refresh();tackle.OnPointerDown(touch);app.view.ReadCommand();touch.position=Centre(dash);tackle.GetComponent<FootballActionSwipe>().OnDrag(touch);touch.position=Centre(pressure);tackle.GetComponent<FootballActionSwipe>().OnDrag(touch);Check(app.view.DashHeld&&app.view.PressureHeld,"tackle origin can swipe through dash to pressure");tackle.GetComponent<FootballActionSwipe>().OnCancel(touch);
   touch.position=Centre(pressure);pressure.Refresh();pressure.OnPointerDown(touch);touch.position=Centre(dash);pressure.GetComponent<FootballActionSwipe>().OnDrag(touch);Check(app.view.PressureHeld&&app.view.DashHeld,"pressure origin can swipe to dash");pressure.GetComponent<FootballActionSwipe>().OnPointerUp(touch);
   var pass=FootballHUD.Instance.GetComponentInChildren<FootballPassButton>();touch.position=Centre(pass);ExecuteEvents.Execute(pass.gameObject,touch,ExecuteEvents.pointerDownHandler);touch.position=Centre(dash);ExecuteEvents.Execute(pass.gameObject,touch,ExecuteEvents.dragHandler);Check(app.view.DashHeld,"disabled pass action still accepts captured swipe to dash through EventSystem");ExecuteEvents.Execute(pass.gameObject,touch,ExecuteEvents.pointerUpHandler);Check(!app.view.DashHeld,"EventSystem pointer up releases visited dash");
  }
  IEnumerator Guest(){
   yield return Until(()=>match.Running,"guest match live",30);yield return Until(()=>ball.CurrentController==app.LocalAthlete,"guest receives host pass",15);
   var local=app.LocalAthlete;var host=FootballBots.Roster.First(a=>a!=local&&!FootballBots.IsBot(a));var d=host.transform.position-local.transform.position;app.view.yaw=Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=app.view.yaw};
   yield return null;Check(app.view.BeginFootballPass(),"guest pass button begins");app.view.EndFootballPass();yield return Until(()=>Record(local).passes==1,"host publishes guest completed pass",8);
   yield return Until(()=>FootballMVP.Instance&&FootballMVP.Instance.Visible,"guest MVP after replay",20);Check(match.PlayerRecords.Count()==4&&match.PlayerRecords.Sum(p=>p.goals)==1&&match.PlayerRecords.Sum(p=>p.assists)==1,"host authored stats replicated for everyone");yield return Capture("guest-mvp");FootballMVP.Instance.GetComponentsInChildren<Button>().First(b=>b.name=="Close MVP").onClick.Invoke();Check(match.BoundaryReleased&&!app.view.InputBlocked,"guest Back to pitch releases boundary and input");yield return new WaitForSeconds(1);
  }
  IEnumerator PortraitCheck(){
   yield return new WaitForEndOfFrame();var art=FootballMVP.Instance.GetComponentsInChildren<Image>().First(i=>i.name=="MVP player illustration");Check(art.sprite&&art.sprite.texture.width>=300,"MVP illustration imported with mobile detail");
   for(int n=1;n<=10;n++)Check(Resources.Load<Sprite>("Football/MVP/mvp-"+n.ToString("00")),"MVP art available for character "+n);
   var camera=Camera.main;var local=ball.Pitch.InverseTransformPoint(camera.transform.position);Check(!camera.orthographic&&Mathf.Abs(local.y-ball.PitchBounds.max.y-1.55f)<.05f,"MVP camera at pitch eye level");
   Check(Athlete.Active.Where(a=>a&&a.visual).SelectMany(a=>a.visual.GetComponentsInChildren<Renderer>()).All(r=>r.forceRenderingOff),"MVP hides live characters behind illustration");
   foreach(var text in FootballMVP.Instance.GetComponentsInChildren<Text>())if(text.text=="MVP"||text.text=="1")Check(text.cachedTextGenerator.vertexCount>=4,"MVP text renders "+text.text);
  }
  IEnumerator Capture(string name){
   yield return new WaitForEndOfFrame();var camera=Camera.main;var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var old=RenderTexture.active;var target=camera.targetTexture;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);camera.targetTexture=target;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"FOOTBALL_PLAYMAKER_COMPLETE success="+!failed+" checks="+checks+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
