#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
namespace SportsPrototype {
 // Explicit command-line opt-in. These checks do not run in normal play.
 public sealed class DevelopmentProbe:MonoBehaviour {
  public static bool Driving;public static float Heading;
  string[] args;string output;bool ready;float elapsed,next;int expected;bool started,returned,preset;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){if(Environment.GetCommandLineArgs().Contains("-probe"))new GameObject("Development probe").AddComponent<DevelopmentProbe>();}
  void Start(){args=Environment.GetCommandLineArgs();output=Value("-report",Application.persistentDataPath+"/probe.jsonl");int.TryParse(Value("-expected","1"),out expected);if(args.Contains("-smoke"))StartCoroutine(Smoke());}
  string Value(string key,string fallback){int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:fallback;}
  void Record(string text){File.AppendAllText(output,text+Environment.NewLine);Debug.Log("PROBE "+text);}
  IEnumerator Smoke(){yield return new WaitForSeconds(3);var app=AppRoot.Instance;app.Show("home");yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.ChangeExtension(output,"home.png"));
   app.Show("character");yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.ChangeExtension(output,"character.png"));
   app.EnterOffline();for(int i=0;i<3;i++){app.view.mode=i;yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.ChangeExtension(output,"camera"+i+".png"));Record("camera="+i+" position="+app.view.transform.position+" actor="+app.LocalAthlete.transform.position);}
   var start=app.LocalAthlete.transform.position;for(int i=0;i<60;i++){app.LocalAthlete.Simulate(new PlayerCommand{move=Vector2.up,heading=0},.02f);yield return null;}Record("movement distance="+Vector3.Distance(start,app.LocalAthlete.transform.position));
   Record("renderers="+FindObjectsByType<Renderer>(FindObjectsSortMode.None).Length+" animator="+app.LocalAthlete.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).fullPathHash);
   foreach(var t in app.stadium.GetComponentsInChildren<UnityEngine.UI.Text>())Record("sign="+t.text+" verts="+t.cachedTextGenerator.vertexCount+" world="+t.transform.position+" enabled="+t.enabled+" scale="+t.transform.lossyScale);
   Record("SMOKE_COMPLETE");}
  void Update(){if(args==null)return;elapsed+=Time.deltaTime;var app=AppRoot.Instance;if(!app)return;
   if(args.Contains("-viewScreen")){app.view.yaw=180;app.view.pitch=-6;app.view.mode=1;}
   var players=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None);var local=players.FirstOrDefault(p=>p.IsOwner);
   if(local&&!ready){ready=true;local.ReadyRpc(true);local.AppearanceRpc(JsonUtility.ToJson(new CharacterAppearance{outfit=(int)local.OwnerClientId%4,hairstyle=(int)local.OwnerClientId%2}));}
   Driving=local&&app.Exploring&&elapsed<38;Heading=local?(float)local.OwnerClientId*30:0;
   if(app.rooms.Host&&local&&!preset&&args.Contains("-presetProbe")){preset=true;var a=StadiumAppearance.Default;a.title="TEST "+Value("-port","7777");a.palette=2;a.design=2;a.screen=1;a.flags=false;local.WorldAppearance.Value=JsonUtility.ToJson(a);}
   if(app.rooms.Host&&elapsed>25&&!started&&players.Length==expected&&players.All(p=>p.Ready.Value)){started=true;_=app.rooms.SetExploring(true);}
   if(app.rooms.Host&&elapsed>48&&!returned){returned=true;_=app.rooms.SetExploring(false);}
   if(elapsed>next){next=elapsed+3;Record("time="+elapsed.ToString("F1")+" count="+players.Length+" connected="+app.rooms.Connected+" exploring="+app.Exploring+" error="+app.rooms.Error+" world="+(NetworkAthlete.HostPlayer?NetworkAthlete.HostPlayer.WorldAppearance.Value.ToString():"none")+" players="+string.Join(";",players.OrderBy(p=>p.OwnerClientId).Select(p=>p.OwnerClientId+":"+p.Ready.Value+":"+p.AppearanceData.Value+":"+p.transform.position)));}
   if(args.Contains("-exitAfter")&&float.TryParse(Value("-exitAfter","60"),out float limit)&&elapsed>limit){Record("PROBE_EXIT");Application.Quit();}
  }
 }
}
#endif
