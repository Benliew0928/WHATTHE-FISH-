#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
namespace WhatTheFish {
 [DefaultExecutionOrder(810)] public sealed class GolfEntrySignReview:MonoBehaviour {
  string folder;AppRoot app;int checks;bool failed;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-golfEntrySignReview");if(i>=0)new GameObject("Golf entry sign check").AddComponent<GolfEntrySignReview>().folder=args[i+1];}
  void Check(bool pass,string label){checks++;failed|=!pass;File.AppendAllText(Path.Combine(folder,"results.txt"),(pass?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Until(Func<bool> test,string label,float timeout=90){float until=Time.realtimeSinceStartup+timeout;while(!test()&&Time.realtimeSinceStartup<until)yield return null;Check(test(),label);}
  void Update(){DevelopmentProbe.Driving=false;DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   yield return Until(()=>AppRoot.Instance&&AppRoot.Instance.LocalAthlete&&AppRoot.Instance.rooms.Connected&&RoomTeleport.Players().Length==2,"TWO_GOLF_ROOM_PLAYERS");app=AppRoot.Instance;if(failed)yield break;
   if(app.rooms.Host){yield return Until(()=>RoomTeleport.Players().All(n=>n.Ready.Value),"ROOM_READY");var task=app.rooms.SetExploring(true);while(!task.IsCompleted)yield return null;}
   yield return Until(()=>TeleportSequence.Busy&&TeleportSequence.Instance.Phase==TeleportPhase.Travelling,"REAL_ROOM_ENTRY_FLIGHT");if(failed)yield break;
   Check(RoomTeleport.Instance.State.entry&&RoomTeleport.Instance.State.to==SportId.Golf,"ENTRY_FROM_PREPARATION_ROOM_TO_GOLF");
   var root=app.environments.roots[(int)SportId.Golf];var course=root.GetComponentInChildren<GolfCourse>(true);var signs=course.GetComponentsInChildren<Canvas>(true).Where(c=>c.name.StartsWith("Tee sign ")).ToArray();var first=signs.Single(c=>c.name=="Tee sign 1");
   Check(signs.Length==5,"FIVE_AUTHORED_COURSE_SIGNS");Check(first.GetComponentInChildren<Text>().text.Contains("HOLE 1")&&first.GetComponentInChildren<Text>().text.Contains("PAR 3"),"ORIGINAL_HOLE_ONE_PAR_THREE_TEXT");
   bool flight=false,arrival=false,settle=false;float until=Time.realtimeSinceStartup+35;
   while(TeleportSequence.Busy&&Time.realtimeSinceStartup<until){
    var service=TeleportSequence.Instance;
    if(service.Phase==TeleportPhase.Travelling&&!flight&&service.PhaseProgress>.3f){flight=true;Check(signs.All(c=>c.enabled&&!TeleportSequence.HidesCanvas(c)),"SIGNS_VISIBLE_DURING_FLIGHT");var hud=GameObject.Find("Mobile UI");Check(hud&&!hud.GetComponent<Canvas>().enabled,"SCREEN_HUD_STILL_HIDDEN_DURING_FLIGHT");yield return new WaitForEndOfFrame();Capture(first,"flight-sign");}
    if(service.Phase==TeleportPhase.Arriving&&!arrival){arrival=true;Check(signs.All(c=>c.enabled),"SIGNS_VISIBLE_DURING_ARRIVAL");yield return new WaitForEndOfFrame();Capture(first,"arrival-sign");}
    if(service.Phase==TeleportPhase.Settling&&!settle){settle=true;Check(signs.All(c=>c.enabled),"SIGNS_VISIBLE_DURING_SETTLING");}
    yield return null;
   }
   Check(flight&&arrival,"FLIGHT_AND_ARRIVAL_OBSERVED");Check(!TeleportSequence.Busy&&app.Exploring&&app.SelectedSport==SportId.Golf,"GOLF_ENTRY_COMPLETES");Check(signs.All(c=>c.enabled)&&!app.view.InputBlocked,"SIGNS_AND_CONTROLS_RETAINED_AFTER_ENTRY");yield return new WaitForEndOfFrame();Capture(first,"entered-sign");
   var settings=Resources.Load<GolfBallPhysicsSettings>("GolfBallPhysics");Check(GolfEventSchedule.RandomKinds.Count==3&&!Enum.GetNames(typeof(GolfEventKind)).Contains("MoonGolf"),"MOON_REMOVED_FROM_POOL_AND_ENUM");
   bool valid=true,noRepeat=true;for(uint seed=0;seed<300;seed++){var state=new GolfMatchState();state.Start(new[]{new GolfEntrant(1,"One"),new GolfEntrant(2,"Two")},0);var schedule=new GolfEventSchedule();schedule.Begin(0,seed);var previous=GolfEventKind.None;for(int cycle=0;cycle<6;cycle++){schedule.Advance(state,80+cycle*90,2);var kind=schedule.Snapshot.kind;valid&=GolfEventSchedule.IsRandom(kind)&&(byte)kind!=2;noRepeat&=kind!=previous;previous=kind;}}
   Check(valid&&noRepeat,"1800_DRAWS_ONLY_THREE_EVENTS_NO_IMMEDIATE_REPEAT");Check(new GolfShotConditions{kind=(GolfEventKind)2}.GravityScale==1&&GolfEventSchedule.Duration((GolfEventKind)2)==0,"REMOVED_ID_HAS_NO_GRAVITY_OR_DURATION");
   var stale=new GolfEventSchedule();stale.Receive(new GolfEventSnapshot{kind=(GolfEventKind)2,start=10,end=30});Check(!stale.WarningActive(5)&&stale.Active(15)==GolfEventKind.None,"REMOVED_ID_CANNOT_SHOW_WARNING_OR_EVENT");
   File.AppendAllText(Path.Combine(folder,"results.txt"),(failed?"FAILED":"COMPLETE")+" checks="+checks+"\n");
  }
  void Capture(Canvas sign,string name){
   var go=new GameObject("Golf sign evidence camera");var cam=go.AddComponent<Camera>();cam.CopyFrom(Camera.main);cam.enabled=false;go.AddComponent<UniversalAdditionalCameraData>();cam.fieldOfView=38;cam.transform.position=sign.transform.position-sign.transform.forward*5+Vector3.up*.25f;cam.transform.LookAt(sign.transform.position);Canvas.ForceUpdateCanvases();
   var rt=new RenderTexture(1100,700,24);var tex=new Texture2D(1100,700,TextureFormat.RGB24,false);var old=RenderTexture.active;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1100,700),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),tex.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=old;Destroy(rt);Destroy(tex);Destroy(go);
  }
 }
}
#endif
