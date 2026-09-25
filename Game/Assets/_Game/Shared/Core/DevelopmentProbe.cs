#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
namespace SportsPrototype {
 // Explicit command-line opt-in. These checks do not run in normal play.
 public sealed partial class DevelopmentProbe:MonoBehaviour {
  public static bool Driving;public static float Heading;
  string[] args;string output;bool ready;float elapsed,next;int expected;bool started,returned,preset;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){if(Environment.GetCommandLineArgs().Contains("-probe"))new GameObject("Development probe").AddComponent<DevelopmentProbe>();}
  void Start(){args=Environment.GetCommandLineArgs();output=Value("-report",Application.persistentDataPath+"/probe.jsonl");int.TryParse(Value("-expected","1"),out expected);if(args.Contains("-tackleAudit"))StartCoroutine(TackleAudit());if(args.Contains("-networkTackleAudit"))StartCoroutine(NetworkTackleAudit());if(args.Contains("-turnAudit"))StartCoroutine(TurnAudit());if(args.Contains("-networkTurnAudit"))StartCoroutine(NetworkTurnAudit());if(args.Contains("-smoke"))StartCoroutine(Smoke());if(args.Contains("-rainbowAudit"))StartCoroutine(RainbowAudit());if(args.Contains("-idleAudit"))StartCoroutine(IdleAudit());if(args.Contains("-networkIdleAudit"))StartCoroutine(NetworkIdleAudit());if(args.Contains("-golfAudit"))StartCoroutine(IslandAudit());if(args.Contains("-saveLogo")||args.Contains("-expectLogo"))StartCoroutine(Persistence());if(args.Contains("-cloudHost")||args.Contains("-cloudCode"))StartCoroutine(Cloud());}
  IEnumerator RainbowAudit(){
   yield return new WaitForSeconds(2);var app=AppRoot.Instance;app.Show("character");yield return new WaitForSeconds(1);
   var athlete=app.LocalAthlete;var animator=athlete.GetComponentInChildren<Animator>();
   Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),"RAINBOW_STILL_IDLE");
   Check(animator.GetCurrentAnimatorClipInfo(0).Any(c=>c.clip.name=="Idle_Playful"),"RAINBOW_AUTHORED_IDLE_ASSIGNED");
   var hip=athlete.GetComponentsInChildren<Transform>(true).First(t=>t.name=="mixamorig:Hips");var initialHip=hip.localPosition;
   yield return new WaitForSeconds(1.5f);Check(Vector3.Distance(initialHip,hip.localPosition)>.00001f,"RAINBOW_IDLE_ACTUALLY_MOVES");Capture(Path.ChangeExtension(output,"idle.png"));
   app.EnterOffline();app.view.mode=1;IslandDriving=true;IslandHeading=0;yield return new WaitForSeconds(1.2f);
   Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Run"),"RAINBOW_RUN_ACTIVE");Check(athlete.speed>6,"RAINBOW_SPRINT_SPEED");Capture(Path.ChangeExtension(output,"run.png"));
   IslandDriving=false;yield return new WaitForSeconds(.8f);Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),"RAINBOW_RETURN_TO_IDLE");Capture(Path.ChangeExtension(output,"still.png"));
  }
  IEnumerator Cloud(){yield return null;var room=AppRoot.Instance.rooms;var task=args.Contains("-cloudHost")?room.Create(AppRoot.Instance.SelectedSport):room.Join(Value("-cloudCode",""));while(!task.IsCompleted)yield return null;Record("CLOUD_ROOM code="+room.Code+" connected="+room.Connected+" error="+room.Error);if(room.Connected&&args.Contains("-codeFile"))File.WriteAllText(Value("-codeFile","room-code.txt"),room.Code);}
  string Value(string key,string fallback){int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:fallback;}
  void Record(string text){File.AppendAllText(output,text+Environment.NewLine);Debug.Log("PROBE "+text);}
  IEnumerator Smoke(){yield return new WaitForSeconds(3);var app=AppRoot.Instance;app.Show("home");yield return new WaitForSeconds(1);Capture(Path.ChangeExtension(output,"home.png"));
   app.Show("character");yield return new WaitForSeconds(1);Capture(Path.ChangeExtension(output,"character.png"));
   if(app.SelectedSport==SportId.Basketball)yield return ArenaAudit();app.EnterOffline();if(app.SelectedSport==SportId.Golf)yield return IslandVisuals();if(app.SelectedSport==SportId.Basketball)yield return ArenaCameraAudit();for(int i=0;i<3;i++){app.view.mode=i;yield return new WaitForSeconds(1);Capture(Path.ChangeExtension(output,"camera"+i+".png"));Record("camera="+i+" position="+app.view.transform.position+" actor="+app.LocalAthlete.transform.position);}
   var start=app.LocalAthlete.transform.position;for(int i=0;i<60;i++)app.LocalAthlete.Simulate(new PlayerCommand{move=Vector2.up,heading=0},.02f);float travelled=Vector3.Distance(start,app.LocalAthlete.transform.position);Record("movement distance="+travelled);Check(travelled>4,"OFFLINE_MOVEMENT");
   Record("renderers="+FindObjectsByType<Renderer>(FindObjectsSortMode.None).Length+" animator="+app.LocalAthlete.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).fullPathHash);
   foreach(var t in app.stadium.GetComponentsInChildren<UnityEngine.UI.Text>())Record("sign="+t.text+" verts="+t.cachedTextGenerator.vertexCount+" sport="+app.SelectedSport+" activeEnvironments="+app.environments.roots.Count(r=>r.activeSelf)+" world="+t.transform.position+" enabled="+t.enabled+" scale="+t.transform.lossyScale);
   Record("SMOKE_COMPLETE");}
  void Update(){if(args==null)return;elapsed+=Time.deltaTime;var app=AppRoot.Instance;if(!app)return;RoomChecks();
   var players=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None);var local=players.FirstOrDefault(p=>p.IsOwner);
   if(local&&!ready){ready=true;local.ReadyRpc(true);local.AppearanceRpc(JsonUtility.ToJson(new CharacterAppearance{outfit=(int)local.OwnerClientId%4,hairstyle=(int)local.OwnerClientId%2}));}
   Driving=local&&app.Exploring&&elapsed<float.Parse(Value("-startAt","25"))+13;Heading=local?(float)local.OwnerClientId*30:0;
   if(args.Contains("-networkTackleAudit"))Driving=false;
   if(args.Contains("-networkTurnAudit")){float since=elapsed-float.Parse(Value("-startAt","25"));if(since>3&&since<7)Heading+=180;else if(since>=10)Heading-=150;if(since>=11)Heading=(local?(float)local.OwnerClientId*30:0)+(Mathf.FloorToInt((since-11)*10)%2==0?180:0);}
   if(app.rooms.Host&&local&&!preset&&args.Contains("-presetProbe")){preset=true;var a=StadiumAppearance.Default;a.title="TEST "+Value("-port","7777");a.palette=2;a.logo=2;a.design=2;a.screen=1;a.flags=false;local.WorldAppearance.Value=JsonUtility.ToJson(a);}
   if(app.rooms.Host&&elapsed>float.Parse(Value("-startAt","25"))&&!started&&players.Length==expected&&players.All(p=>p.Ready.Value)){started=true;_=app.rooms.SetExploring(true);}
   if(app.rooms.Host&&elapsed>float.Parse(Value("-returnAt","48"))&&!returned){returned=true;_=app.rooms.SetExploring(false);}
   if(elapsed>next){next=elapsed+3;Record("time="+elapsed.ToString("F1")+" count="+players.Length+" connected="+app.rooms.Connected+" exploring="+app.Exploring+" error="+app.rooms.Error+" sport="+app.SelectedSport+" activeEnvironments="+app.environments.roots.Count(r=>r.activeSelf)+" world="+(NetworkAthlete.HostPlayer?NetworkAthlete.HostPlayer.WorldAppearance.Value.ToString():"none")+" players="+string.Join(";",players.OrderBy(p=>p.OwnerClientId).Select(p=>p.OwnerClientId+":"+p.Ready.Value+":"+p.AppearanceData.Value+":"+p.transform.position)));}
   if(args.Contains("-exitAfter")&&float.TryParse(Value("-exitAfter","60"),out float limit)&&elapsed>limit){Record("PROBE_EXIT");Application.Quit();}
  }
 }
}
#endif
