#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace WhatTheFish {
 public sealed class SkySailReview:MonoBehaviour {
  string folder;bool failed,network,captureOnly,walkOnly;AppRoot app;SkySailWorld world;Camera camera;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Initialize(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-skyReview");if(i<0)return;var r=new GameObject("Sky-Sail acceptance review").AddComponent<SkySailReview>();r.folder=a[i+1];r.network=a.Contains("-skyNetworkReview");r.captureOnly=a.Contains("-skyCaptureOnly");r.walkOnly=a.Contains("-skyWalkReview");}
  void Check(bool ok,string message){failed|=!ok;File.AppendAllText(Path.Combine(folder,"review.txt"),(ok?"PASS ":"FAIL ")+message+"\n");Debug.Log("SKY_REVIEW "+(ok?"PASS ":"FAIL ")+message);}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"review.txt"),"");float until=Time.realtimeSinceStartup+80;
   while((!AppRoot.Instance||!AppRoot.Instance.environments.View||!SkySailWorld.Instance)&&Time.realtimeSinceStartup<until)yield return null;
   app=AppRoot.Instance;world=SkySailWorld.Instance;camera=Camera.main;yield return new WaitForSeconds(2);
   if(!app||!app.environments.View||!world){Check(false,"World initialization");Finish();yield break;}
   if(network){yield return NetworkReview();Finish();yield break;}
   app.EnterOffline();Check(world.proxies.Length==4&&world.stations.Length==4,"Four islands and modular stations");Check(app.environments.roots.Count(r=>r&&r.activeSelf)==1,"One active detailed island at rest");
   foreach(var a in SkySailMap.Circuit)foreach(int direction in new[]{-1,1}){
    var b=SkySailMap.Neighbor(a,direction);Check(Vector3.Distance(SkySailMap.Path(a,b,0),SkySailMap.Berth(a))<.001f&&Vector3.Distance(SkySailMap.Path(a,b,1),SkySailMap.Berth(b))<.001f,a+" to "+b+" exact station endpoints");
    Check(Enumerable.Range(0,101).All(i=>SkySailMap.Path(a,b,i/100f).y>2),a+" to "+b+" sea clearance");
   }
   var streaming=app.environments.GetComponent<SkySailStreaming>();
   world.reviewCamera=true;app.view.enabled=false;
   if(walkOnly){foreach(var sport in SkySailMap.Circuit){yield return streaming.Prepare(sport);app.SelectSport(sport);yield return null;yield return WalkApproach(sport);}Finish();yield break;}
   camera.transform.position=new Vector3(0,810,-1100)-world.Origin;camera.transform.LookAt(new Vector3(0,0,70)-world.Origin);camera.fieldOfView=62;yield return null;Capture("01_World_Overview");
   for(int leg=0;leg<(captureOnly?1:4);leg++){
    var source=app.SelectedSport;var target=SkySailMap.Neighbor(source,1);Place(SkySailMap.Exit(source,0));Physics.SyncTransforms();
    Check(Physics.Raycast(app.LocalAthlete.transform.position+Vector3.up*.3f,Vector3.down,1,1<<8),source+" station arrival is supported");
    var station=world.stations[(int)source].transform;
    camera.transform.position=station.TransformPoint(new Vector3(14,10,20));camera.transform.LookAt(station.TransformPoint(new Vector3(0,4,-2)));camera.fieldOfView=60;yield return null;Capture(source+"_Station");
    camera.transform.position=world.cabin.TransformPoint(new Vector3(0,1.7f,-2.9f));camera.transform.rotation=world.cabin.rotation*Quaternion.Euler(0,16,0);yield return null;Capture(source+"_Cabin_Interior");
    if(captureOnly)break;
    Check(world.CanTravel(target,out _),source+" group can board for "+target);world.RequestTravel(target);
    bool riding=false,docking=false,sourceReleased=false;until=Time.realtimeSinceStartup+100;int shot=0;
    while(Time.realtimeSinceStartup<until){
     if(world.Journey.phase==SkySailPhase.Riding){riding=true;if(!streaming.Loaded(source))sourceReleased=true;
      float t=world.Progress;world.reviewCamera=false;app.view.enabled=true;app.view.mode=shot==1?0:1;app.view.yaw=world.cabin.eulerAngles.y+(shot==1?65:135);app.view.pitch=shot==1?4:13;
      if(shot==0&&t>.22f||shot==1&&t>.51f||shot==2&&t>.78f){yield return null;Capture(source+"_Journey_"+(shot+1));shot++;}
      CheckOnceSeat();
     }
     if(world.Journey.phase==SkySailPhase.Docking)docking=true;
     if(riding&&!world.Travelling)break;yield return null;
    }
    Check(riding&&docking&&!world.Travelling,source+" → "+target+" completes actual journey");Check(sourceReleased,source+" scene released during journey");Check(app.SelectedSport==target,"Arrived on "+target);Check(app.LocalAthlete.capsule.enabled,"Walking restored at "+target);
    yield return new WaitForSeconds(.4f);Check(Physics.Raycast(app.LocalAthlete.transform.position+Vector3.up*.3f,Vector3.down,1,1<<8),target+" disembark collision");
    Check(app.environments.roots.Count(r=>r&&r.activeSelf)==1,"Only destination detail active after docking");world.reviewCamera=true;app.view.enabled=false;checkedSeat=false;
    if(failed)break;
   }
   Finish();
  }
  bool checkedSeat;
  IEnumerator WalkApproach(SportId sport){
   var approach=app.environments.roots[(int)sport].transform.Find("Sky-Sail shore approach");
   if(!approach){Check(false,sport+" approach exists");yield break;}
   var start=approach.Find("Inland arrival").position;var end=approach.Find("Platform arrival").position;
   Place(start+Vector3.up*.1f);Physics.SyncTransforms();
   yield return WalkTo(end,sport+" shore to gangway");yield return WalkTo(SkySailMap.Exit(sport,0),sport+" gangway to boarding deck");
   camera.transform.position=world.stations[(int)sport].transform.TransformPoint(new Vector3(14,10,20));camera.transform.LookAt(world.stations[(int)sport].transform.TransformPoint(new Vector3(0,4,-2)));camera.fieldOfView=60;yield return null;Capture(sport+"_Station");
   yield return WalkTo(end,sport+" deck to gangway");yield return WalkTo(start,sport+" gangway to shore");
  }
  IEnumerator WalkTo(Vector3 goal,string label){
   var athlete=app.LocalAthlete;int budget=1800;float stalled=0;var last=athlete.transform.position;
   while(budget>0){
    for(int i=0;i<12;i++){var d=goal-athlete.transform.position;d.y=0;if(d.magnitude<.2f){budget=0;break;}athlete.Simulate(new PlayerCommand{move=Vector2.up,heading=Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg},1f/60);budget--;}
    yield return null;if(Vector3.Distance(last,athlete.transform.position)<.008f)stalled+=.2f;else stalled=0;last=athlete.transform.position;if(stalled>1.6f)break;
   }
   for(int i=0;i<20;i++)athlete.Simulate(default,1f/60);
   var delta=goal-athlete.transform.position;Check(new Vector2(delta.x,delta.z).magnitude<.5f&&Mathf.Abs(delta.y)<.65f,label+" controller traversal ("+athlete.transform.position+")");
  }
  void CheckOnceSeat(){if(checkedSeat)return;checkedSeat=true;Check(!app.LocalAthlete.capsule.enabled&&Vector3.Distance(app.LocalAthlete.transform.position,world.cabin.position)<5,"Passenger anchored inside cabin during ride");}
  IEnumerator NetworkReview(){
   float limit=Time.realtimeSinceStartup+70;while((!app.rooms.Connected||!NetworkAthlete.HostPlayer||FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).Length<2)&&Time.realtimeSinceStartup<limit)yield return null;
   var local=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).FirstOrDefault(p=>p.IsOwner);if(!local){Check(false,"Network player ready");yield break;}local.ReadyRpc(true);
   if(app.rooms.Host){while(FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).Any(p=>!p.Ready.Value)&&Time.realtimeSinceStartup<limit)yield return null;var task=app.rooms.SetExploring(true);while(!task.IsCompleted)yield return null;}
   while(!app.Exploring&&Time.realtimeSinceStartup<limit)yield return null;
   if(app.rooms.Host){int i=0;foreach(var p in FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None)){var a=p.GetComponent<Athlete>();a.capsule.enabled=false;var pos=SkySailMap.Exit(app.SelectedSport,i++);p.GetComponent<NetworkTransform>().Teleport(pos,Quaternion.identity,Vector3.one);a.capsule.enabled=true;}}
   foreach(var target in new[]{SportId.Golf,SportId.Football}){
    yield return new WaitForSeconds(2);if(app.rooms.Host)world.RequestTravel(target);else Check(!world.CanTravel(target,out _),"Guest cannot choose the room destination");
    bool ride=false;limit=Time.realtimeSinceStartup+110;
    while(Time.realtimeSinceStartup<limit){if(world.Journey.phase==SkySailPhase.Riding)ride=true;if(ride&&!world.Travelling)break;yield return null;}
    Check(ride&&!world.Travelling,"Replicated group ride completed to "+target);Check(NetworkAthlete.HostPlayer&&app.SelectedSport==target&&NetworkAthlete.HostPlayer.WorldSport.Value==target,"Shared destination agrees with host");
    Check(FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).Length==2&&FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).All(p=>Vector3.Distance(p.transform.position,SkySailMap.Port(target))<12),"Every player disembarks together");
    Check(FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).All(p=>p.GetComponent<NetworkTransform>().enabled),"Network transforms restored");
   }
   yield return new WaitForSeconds(2);
  }
  void Place(Vector3 p){app.LocalAthlete.capsule.enabled=false;app.LocalAthlete.transform.position=p;app.LocalAthlete.capsule.enabled=true;app.LocalAthlete.ResetLocomotion();}
  void Capture(string name){var rt=RenderTexture.GetTemporary(1920,1080,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Default,4);var old=camera.targetTexture;camera.targetTexture=rt;camera.Render();var active=RenderTexture.active;RenderTexture.active=rt;var pixels=new Texture2D(1920,1080,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1920,1080),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());UnityEngine.Object.Destroy(pixels);RenderTexture.active=active;camera.targetTexture=old;RenderTexture.ReleaseTemporary(rt);}
  void Finish(){File.AppendAllText(Path.Combine(folder,"review.txt"),"SKY_SAIL_REVIEW_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
