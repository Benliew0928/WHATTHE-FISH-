#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class ProgrammedBotsReview:MonoBehaviour {
  string folder;bool failed;AppRoot app;bool basketball;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-programmedBotsReview");if(i>=0&&i+1<a.Length)new GameObject("Programmed bots review").AddComponent<ProgrammedBotsReview>().folder=a[i+1];}
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Until(Func<bool> predicate,string label,float seconds=25){float end=Time.realtimeSinceStartup+seconds;while(!predicate()&&Time.realtimeSinceStartup<end){var m=FootballMatch.Instance;if(!basketball&&m&&m.Authority&&m.KickoffReady&&m.KickoffActor)m.TryKickoff(m.KickoffActor,m.BotKickoffHeading(m.KickoffActor),.3f,m.Snapshot.revision);yield return null;}Check(predicate(),label);}
  Athlete[] Roster=>basketball?BasketballBots.Roster:FootballBots.Roster;
  int Count=>basketball?BasketballBots.Instance.Count:FootballBots.Instance.Count;
  bool Running=>basketball?BasketballMatch.Instance.Running:FootballMatch.Instance.Running;
  FootballTeam Team(Athlete a)=>basketball?BasketballMatch.Instance.TeamOf(a):FootballMatch.Instance.TeamOf(a);
  ulong Id(Athlete a)=>basketball?BasketballMatch.PlayerId(a):FootballBots.Id(a);
  bool StartMatch()=>basketball?BasketballMatch.Instance.StartMatch():FootballMatch.Instance.StartMatch();
  void ClearMatch(){if(basketball)BasketballMatch.Instance.Clear();else FootballMatch.Instance.ClearMatch();}
  void ClearBots(){if(basketball)BasketballBots.Instance.Clear();else FootballBots.Instance.Clear();}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   yield return Until(()=>AppRoot.Instance&&AppRoot.Instance.LocalAthlete&&AppRoot.Instance.Exploring,"entered selected sport",80);
   app=AppRoot.Instance;if(!app||!app.Exploring){Finish();yield break;}basketball=app.SelectedSport==SportId.Basketball;
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;app.view.mode=2;
   if(app.rooms.Connected){yield return Room();Finish();yield break;}
   Check(Count==0&&!StartMatch(),"Explore Offline has no bots or competitive start");
   app.Show("home");yield return Until(()=>!app.Menu.IsTransitioning,"menu ready");
   var button=app.Menu.Content.GetComponentsInChildren<Button>().FirstOrDefault(b=>b.name=="Play vs AI");Check(button,"Play vs bots entry exists");if(!button){Finish();yield break;}button.onClick.Invoke();
   yield return Until(()=>app.Menu.Page=="ai"&&!app.Menu.IsTransitioning,"bot setup opens");
   Check(app.Menu.Content.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("AI team size "))==4,"2v2 through 5v5 options");yield return Capture("setup");
   Time.timeScale=3;
   for(int size=2;size<=5;size++){
    bool started=basketball?BasketballBots.Instance.StartSolo(size):FootballBots.Instance.StartSolo(size);Check(started,"solo setup "+size);
    yield return null;Check(Count==size*2-1&&Roster.Length==size*2,"human plus correct bot roster "+size);
    Check(Roster.Select(Id).Distinct().Count()==size*2,"unique identities "+size);
    Check(!(basketball?BasketballBots.Instance.StartSolo(6):FootballBots.Instance.StartSolo(6))&&Roster.Length==size*2,"invalid size leaves roster intact "+size);
    Check(StartMatch(),"match starts "+size);yield return Until(()=>Running,"regulation starts "+size);
    Check(Roster.Count(a=>Team(a)==FootballTeam.A)==size&&Roster.Count(a=>Team(a)==FootballTeam.B)==size&&Team(app.LocalAthlete)==FootballTeam.A,"balanced teams and local A "+size);
    if(basketball)foreach(var a in Roster)foreach(float heading in new[]{0f,180f})Check(BasketballBall.Active.SelectHoop(a.transform.position,heading,a)==BasketballMatch.Instance.AttackingHoop(a),"assigned hoop "+Id(a)+" facing "+heading);
    yield return new WaitForSeconds(8);
    Check(Roster.Where(a=>a!=app.LocalAthlete).All(a=>basketball?a.GetComponent<BasketballBot>().Decisions>5:a.GetComponent<FootballBot>().Decisions>5),"all bots make delayed decisions "+size);
    Check(Roster.All(a=>float.IsFinite(a.transform.position.x)&&float.IsFinite(a.transform.position.y)&&float.IsFinite(a.transform.position.z)),"finite athlete positions "+size);
    if(size==5)yield return Capture("5v5");
    app.EnterOffline();yield return null;Check(Count==0&&!StartMatch(),"Explore resets match and removes bots "+size);
   }
   if(basketball)yield return BasketballActions();else yield return FootballActions();
   yield return FullMatch();
   Time.timeScale=1;app.EnterOffline();yield return null;
   Check(Count==0&&!Running&&app.LocalAthlete&&app.LocalAthlete.isActiveAndEnabled,"return to clean exploration keeps human");Finish();
  }
  IEnumerator BasketballActions(){
   BasketballBots.Instance.StartSolo(2);yield return null;StartMatch();yield return Until(()=>Running,"action fixture running");
   var m=BasketballMatch.Instance;var ball=BasketballBall.Active;var shooter=Roster.First(a=>a!=app.LocalAthlete&&Team(a)==FootballTeam.A);var policy=shooter.GetComponent<BasketballBot>();
   foreach(var a in Roster)if(a.GetComponent<BasketballBot>())a.GetComponent<BasketballBot>().enabled=false;
   var hoop=m.AttackingHoop(shooter);var court=ball.transform.parent;
   foreach(var a in Roster)Place(a,court.TransformPoint(new Vector3(5,.05f,-8+Array.IndexOf(Roster,a)*2)));
   Place(shooter,hoop.TransformPoint(new Vector3(0,.05f,4.8f)));ball.GiveMatchPossession(shooter);m.Control(shooter);
   policy.Initialize(policy.Slot);policy.enabled=true;uint shots=ball.ShotCount;
   yield return Until(()=>ball.ShotCount>shots,"bot physically releases a charged shot",12);
   Check(ball.LastReleasePower>.5f&&ball.LastReleasePower<.85f,"casual shot timing uses bounded imperfect power");
   yield return Until(()=>!ball.Held&&!ball.ActionQueued,"shot is a free physics ball",5);yield return Capture("shot");
   yield return new WaitForSeconds(4);
   // This fixture disables the receiving bots. Complete their manual inbound
   // through the normal pass APIs before testing the next attacking action.
   yield return Until(()=>Running||m.RestartReady,"post-shot play or inbound ready");
   if(m.RestartReady&&ball.InboundPending){
    var inbounder=ball.Holder;var receiver=Roster.First(a=>a!=inbounder&&Team(a)==Team(inbounder));
    var direction=receiver.transform.position-inbounder.transform.position;float heading=Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg;
    Check(ball.BeginPassCharge(inbounder,heading,ball.Defense.play),"disabled fixture receiver begins manual inbound");
    yield return new WaitForSeconds(.3f);
    Check(ball.ReleasePassCharge(inbounder,heading,ball.Defense.play,BasketballMotion.Clock),"disabled fixture receiver releases manual inbound");
   }
   yield return Until(()=>Running,"post-shot restart resumes");
   policy.enabled=false;
   // Keep the receiver ahead after the carrier's normal reaction/dribble delay.
   float sign=court.InverseTransformPoint(hoop.position).z>0?1:-1;
   Place(shooter,court.TransformPoint(new Vector3(0,.05f,-sign*6)));
   Place(app.LocalAthlete,court.TransformPoint(new Vector3(4,.05f,sign*4)));
   var enemies=Roster.Where(a=>Team(a)!=Team(shooter)).ToArray();Place(enemies[0],court.TransformPoint(new Vector3(-2,.05f,-sign*6)));Place(enemies[1],court.TransformPoint(new Vector3(-5,.05f,-sign*10)));
   ball.GiveMatchPossession(shooter);m.Control(shooter);policy.Initialize(policy.Slot);policy.enabled=true;uint passes=ball.PassCount;
   float until=Time.realtimeSinceStartup+8;
   while(ball.PassCount<=passes&&Time.realtimeSinceStartup<until){
    File.AppendAllText(Path.Combine(folder,"pass-trace.txt"),$"time={Time.time:F2} phase={m.Snapshot.phase} holder={(ball.Holder?Id(ball.Holder):0)} actor={Id(shooter)} pos={court.InverseTransformPoint(shooter.transform.position)} intent={policy.Intent} action={shooter.BasketballMotion.Action} aim={ball.PassAim.phase} canPass={ball.CanPass(shooter)} passes={ball.PassCount} shots={ball.ShotCount} locked={ball.PropLocked(shooter)}\n");
    yield return new WaitForSeconds(.15f);
   }
   Check(ball.PassCount>passes,"bot passes to an open advanced teammate");
   yield return Until(()=>ball.PassCount>passes&&ball.Holder==app.LocalAthlete,"human teammate physically receives bot pass",6);
   Check(ball.PropActor(BasketballMatch.PlayerId(shooter))==shooter,"offline ability identity resolves bot rather than human");
   policy.enabled=false;var defender=enemies[0];var defense=defender.GetComponent<BasketballBot>();
   var ownHoop=m.Hoop(Team(defender));var toHome=Vector3.ProjectOnPlane(ownHoop.position-app.LocalAthlete.transform.position,Vector3.up).normalized;
   Place(defender,app.LocalAthlete.transform.position+toHome*2);ball.GiveMatchPossession(app.LocalAthlete);m.Control(app.LocalAthlete);defense.Initialize(defense.Slot);defense.enabled=true;
   yield return Until(()=>defender.BasketballMotion.Guarding,"defender physically guards opposing carrier",5);
   app.EnterOffline();yield return null;
  }
  IEnumerator FootballActions(){
   FootballBots.Instance.StartSolo(2);yield return null;StartMatch();yield return Until(()=>Running,"football shot fixtures running");
   var ball=FootballBall.Instance;var match=FootballMatch.Instance;
   foreach(var team in new[]{FootballTeam.A,FootballTeam.B}){
    foreach(var a in Roster)if(a.GetComponent<FootballBot>())a.GetComponent<FootballBot>().enabled=false;
    for(int i=0;i<Roster.Length;i++)Place(Roster[i],ball.Pitch.TransformPoint(new Vector3(ball.PitchBounds.center.x-18+i*4,ball.PitchBounds.max.y+.02f,ball.PitchBounds.center.z)));
    var shooter=Roster.First(a=>a!=app.LocalAthlete&&Team(a)==team);var policy=shooter.GetComponent<FootballBot>();float sign=team==FootballTeam.A?1:-1;
    int goal=Enumerable.Range(0,ball.GoalCount).First(i=>ball.GoalSign(i)==sign);
    Place(shooter,ball.Pitch.TransformPoint(new Vector3(ball.GoalBounds(goal).center.x,ball.PitchBounds.max.y+.02f,ball.GoalFront(goal)-sign*12)));
    shooter.transform.rotation=Quaternion.Euler(0,ball.Pitch.eulerAngles.y+(sign>0?0:180),0);shooter.ResetLocomotion();
    ball.ForgetPlayer(shooter);ball.ReviewFootballBall(ball.FootPosition(shooter),Vector3.zero);Physics.SyncTransforms();ball.RefreshControl(shooter);
    Check(ball.CurrentController==shooter,"football shot fixture acquires through normal pickup "+team);
    int before=team==FootballTeam.A?match.State.ScoreA:match.State.ScoreB;policy.Initialize(policy.Slot,42);policy.enabled=true;
    yield return Until(()=>(team==FootballTeam.A?match.State.ScoreA:match.State.ScoreB)>before,"bot physically scores at assigned football goal "+team,8);
    Check(policy.Kicks>0,"goal uses the normal kick action "+team);policy.enabled=false;
    yield return Until(()=>Running,"football score restart resumes "+team);
   }
   app.EnterOffline();yield return null;
  }
  IEnumerator FullMatch(){
   if(basketball)BasketballBots.Instance.StartSolo(2);else FootballBots.Instance.StartSolo(2);yield return null;
   // Only the development fixture replaces local input to exercise an unattended
   // full match. Normal solo play always retains the human's controls.
   MonoBehaviour driver;
   if(basketball){var bot=app.LocalAthlete.gameObject.AddComponent<BasketballBot>();bot.Initialize(0);driver=bot;}
   else {var bot=app.LocalAthlete.gameObject.AddComponent<FootballBot>();bot.Initialize(0,1729);driver=bot;}
   bool started=basketball?BasketballMatch.Instance.ReviewStart(Roster,60,1):FootballMatch.Instance.StartBotReviewMatch(1729,60);Check(started,"full match fixture starts");
   yield return Until(()=>Running,"full match regulation");
   float end=Time.realtimeSinceStartup+100;bool restart=false;uint shots=0,passes=0;int kicks=0,score=0;
   while(Time.realtimeSinceStartup<end){
    if(basketball){var m=BasketballMatch.Instance;restart|=m.Snapshot.phase==BasketballMatchPhase.Restart;shots=Math.Max(shots,BasketballBall.Active.ShotCount);passes=Math.Max(passes,BasketballBall.Active.PassCount);score=m.State.ScoreA+m.State.ScoreB;if(m.Snapshot.phase==BasketballMatchPhase.Finished)break;}
    else {var m=FootballMatch.Instance;restart|=m.Snapshot.phase==FootballMatchPhase.GoalReset;score=m.State.ScoreA+m.State.ScoreB;kicks=Roster.Sum(a=>a.GetComponent<FootballBot>().Kicks);if(m.Snapshot.phase==FootballMatchPhase.Finished)break;}
    yield return null;
   }
   Check(basketball?BasketballMatch.Instance.Snapshot.phase==BasketballMatchPhase.Finished:FootballMatch.Instance.Snapshot.phase==FootballMatchPhase.Finished,"complete match reaches result without intervention");
   Check(basketball?shots>0:kicks>0,"bots produce physical attacking actions");if(basketball)Check(score>0&&restart,"bots score and resume after score");
   File.AppendAllText(Path.Combine(folder,"results.txt"),$"METRICS score={score} shots={shots} passes={passes} kicks={kicks}\n");yield return Capture("result");
   Destroy(driver);yield return null;ClearMatch();ClearBots();
  }
  IEnumerator Room(){
   var host=NetworkAthlete.HostPlayer;var local=app.LocalAthlete;
   if(app.rooms.Host){
    Check(basketball?BasketballBots.Instance.FillRoom(2):FootballBots.Instance.FillRoom(2),"host fills room to 2v2");yield return new WaitForSeconds(2);
    Check(Count==2&&Roster.Length==4&&Roster.Select(Id).Distinct().Count()==4,"room bot roster has distinct identities");
    Check(NetworkAthlete.HostPlayer==host&&app.LocalAthlete==local,"bots do not overwrite host or local human");Check(StartMatch(),"host starts filled match");
    foreach(var a in Roster.Where(a=>a.GetComponent<NetworkAthlete>().BotSlot.Value==0)){if(basketball)BasketballMatch.Instance.ChooseTeam(a,FootballTeam.A,BasketballMatch.Instance.Snapshot.revision);else FootballMatch.Instance.ChooseTeam(a,FootballTeam.A,FootballMatch.Instance.Snapshot.revision);}
    yield return Until(()=>Running,"filled room enters regulation");
    Check(!(basketball?BasketballBots.Instance.FillRoom(3):FootballBots.Instance.FillRoom(3)),"active match rejects roster filling");
    if(basketball){var bot=Roster.First(a=>a.GetComponent<BasketballBot>());BasketballBall.Active.GiveMatchPossession(bot);BasketballMatch.Instance.Control(bot);}
    yield return new WaitForSeconds(30);
    Check(Count==2&&Roster.Where(a=>a.GetComponent<NetworkAthlete>().BotSlot.Value>0).All(a=>basketball?a.GetComponent<BasketballBot>().Decisions>10:a.GetComponent<FootballBot>().Decisions>10),"host simulates both bots through live play");yield return Capture("room-live");
    if(basketball)Check(BasketballBall.Active.ShotCount+BasketballBall.Active.PassCount>0,"room basketball bots release shots or passes");
    ClearMatch();ClearBots();yield return new WaitForSeconds(3);Check(Count==0&&NetworkAthlete.HostPlayer==host&&local,"remove bots retains room humans");
   }else {
    Check(!(basketball?BasketballBots.Instance.FillRoom(2):FootballBots.Instance.FillRoom(2)),"guest cannot create bots");
    yield return Until(()=>Count==2,"guest receives both bots");Check(Roster.Select(Id).Distinct().Count()==4,"guest has unique participant identities");
    Check(Roster.All(a=>!a.GetComponent<BasketballBot>()&&!a.GetComponent<FootballBot>()),"guest never runs bot decisions");
    yield return Until(()=>Running,"guest sees filled match running");bool seenCarrier=!basketball;float end=Time.realtimeSinceStartup+45;
    while(Count>0&&Time.realtimeSinceStartup<end){if(basketball&&BasketballBall.Active.Held&&BasketballBall.Active.HolderId>=(1UL<<63)){var holder=BasketballBall.Active.Holder;seenCarrier|=holder&&holder!=local&&BasketballMatch.PlayerId(holder)==BasketballBall.Active.HolderId;}yield return null;}
    Check(seenCarrier,"guest resolves bot ball carrier correctly");Check(Count==0&&NetworkAthlete.HostPlayer==host&&app.LocalAthlete==local,"guest receives clean removal without losing humans");
   }
  }
  static void Place(Athlete a,Vector3 p){bool enabled=a.capsule.enabled;a.capsule.enabled=false;a.transform.position=p;a.capsule.enabled=enabled;a.ResetLocomotion();Physics.SyncTransforms();}
  IEnumerator Capture(string name){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)yield break;
   yield return null;yield return new WaitForEndOfFrame();var camera=Camera.main;var canvas=FindObjectsByType<Canvas>(FindObjectsSortMode.None).First(c=>c.name=="Mobile UI");var mode=canvas.renderMode;var oldCamera=canvas.worldCamera;float plane=canvas.planeDistance;
   canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var old=RenderTexture.active;var target=camera.targetTexture;
   camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var texture=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);texture.Apply();
   Check(texture.GetPixels32().Any(p=>p.r>30||p.g>30||p.b>30),"nonblank capture "+name);File.WriteAllBytes(Path.Combine(folder,name+".png"),texture.EncodeToPNG());Destroy(texture);
   camera.targetTexture=target;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);canvas.renderMode=mode;canvas.worldCamera=oldCamera;canvas.planeDistance=plane;Canvas.ForceUpdateCanvases();
  }
  void Finish(){Time.timeScale=1;File.AppendAllText(Path.Combine(folder,"results.txt"),"PROGRAMMED_BOTS_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
