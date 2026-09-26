#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace WhatTheFish {
 public sealed class CoastalStadiumReview:MonoBehaviour {
  string folder;bool failed,record;int frame;Camera camera;AppRoot app;CoastalVenueRoutes routes;
  RenderTexture target;Texture2D pixels;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-stadiumReview");if(i<0)return;
   var r=new GameObject("Coastal stadium validation").AddComponent<CoastalStadiumReview>();r.folder=args[i+1];r.record=args.Contains("-recordRoutes");
  }
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(folder,"review.txt"),(ok?"PASS ":"FAIL ")+label+"\n");Debug.Log("STADIUM_REVIEW "+(ok?"PASS ":"FAIL ")+label);}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"review.txt"),"");yield return new WaitForSeconds(3);
   QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;app=AppRoot.Instance;camera=Camera.main;
   foreach(var sport in new[]{SportId.Football,SportId.Basketball}){
    app.SelectSport(sport);app.EnterOffline();app.view.enabled=false;routes=app.stadium.GetComponent<CoastalVenueRoutes>();
    Check(routes&&routes.entries.Length==4,sport+" four connected public entrances");
    foreach(var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))if(canvas.renderMode!=RenderMode.WorldSpace)canvas.gameObject.SetActive(false);
    Check(!app.stadium.transform.Find("Collision_RunoffBoundary"),sport+" old continuous boundary removed");
    if(sport==SportId.Basketball){Check(!app.stadium.transform.Find("Ceiling"),"Basketball ceiling removed");Check(!Physics.Raycast(new Vector3(0,3,0),Vector3.up,100,1<<8),"Basketball unobstructed central sky");}
    bool football=sport==SportId.Football;int main=Enumerable.Range(0,4).OrderBy(i=>routes.entries[i].z).First();
    var entry=routes.entries[main];var inside=routes.inside[main];var inward=(inside-entry).normalized;var side=Vector3.Cross(Vector3.up,inward);
    var top=routes.concourse[main];var low=routes.stairBottom[main];
    Vector3[] positions={football?new Vector3(140,86,-174):new Vector3(72,43,-86),football?new Vector3(-140,63,172):new Vector3(-67,38,82),entry-inward*4+Vector3.up*1.6f,entry+side*3+inward*5+Vector3.up*1.7f,top+Vector3.up*1.6f,Vector3.Lerp(low,routes.stairTop[main],.72f)+Vector3.up*1.7f,inside-inward*2+Vector3.up*1.65f,football?new Vector3(12,1.7f,-20):new Vector3(5,1.65f,-9)};
    Vector3[] looks={new Vector3(0,football?7:4,0),new Vector3(0,4,0),entry+inward*9+Vector3.up*3,entry+side*4+inward*8+Vector3.up*2.2f,top+inward*14+Vector3.up*1.0f,top+inward*6,inside+inward*14+Vector3.up*3,new Vector3(0,football?12:8,football?45:20)};
    if(!football)looks[4]=new Vector3(0,4,0);
    positions[5]=routes.aisles[0].points[football?1:3]+Vector3.up*1.6f;looks[5]=routes.aisles[0].points.Last()+Vector3.up;
    positions[6]=entry+inward*10+Vector3.up*1.65f;
    string[] names={"01_Exterior_Front","02_Exterior_Rear","03_Entrance","04_Material_Close","05_Concourse","06_Spectator_Aisle","07_Playing_Entrance","08_Open_Sky"};
    app.LocalAthlete.capsule.enabled=false;app.LocalAthlete.gameObject.SetActive(false);
    for(int i=0;i<names.Length;i++){
     camera.transform.position=positions[i];camera.transform.LookAt(looks[i]);camera.fieldOfView=i<2?48:68;
     yield return new WaitForSeconds(.35f);yield return new WaitForEndOfFrame();Capture(Path.Combine(folder,sport+"_"+names[i]+".png"));
    }
    // Actual controller movement, with no teleports between route waypoints.
    app.LocalAthlete.gameObject.SetActive(true);app.LocalAthlete.capsule.enabled=true;app.LocalAthlete.HideHead(true);
    for(int i=0;i<4;i++){
     Place(routes.entries[i]);yield return Walk(routes.inside[i],sport+" entry "+i,false);yield return Walk(routes.entries[i],sport+" exit "+i,false);
     Place(routes.stairBottom[i]);yield return Stairs(i,true,false);yield return Walk(routes.concourse[i],sport+" concourse landing "+i,false);
     yield return Walk(routes.stairTop[i],sport+" landing return "+i,false);yield return Stairs(i,false,false);
    }
    // Arrival route in one continuous recording, plus the main spectator stair.
    foreach(var aisle in routes.aisles){
     Place(aisle.points[0]);foreach(var point in aisle.points)yield return Walk(point,sport+" spectator aisle",false);
     foreach(var point in aisle.points.Reverse())yield return Walk(point,sport+" spectator aisle return",false);
    }
    if(record){
     frame=0;var frames=Path.Combine(folder,sport+"_WalkthroughFrames");Directory.CreateDirectory(frames);
     float dockZ=football?-165:-94;Place(new Vector3(0,-.48f,dockZ));
     yield return Walk(new Vector3(0,.15f,football?-139:-69),sport+" dock approach",true);
     yield return Walk(entry,sport+" arrival to main entrance",true);yield return Walk(inside,sport+" main entrance to playing area",true);
     yield return Walk(entry,sport+" playing area to island",true);
     yield return Walk(new Vector3(0,.15f,football?-139:-69),sport+" entrance to dock approach",true);
     yield return Walk(new Vector3(0,-.48f,dockZ),sport+" dock return",true);
     yield return Walk(new Vector3(0,.15f,football?-139:-69),sport+" dock departure",true);
     yield return Walk(entry,sport+" return to main entrance",true);yield return Walk(low,sport+" entrance to spectator stair",true);
     yield return Stairs(main,true,true);yield return Walk(top,sport+" recorded concourse",true);
    }
    float bridgeX=(football?132:70)*.47f,bridgeZ=-(football?160:83)*.66f;
    var bridgeWest=SurfacePoint(new Vector3(bridgeX-9.4f,0,bridgeZ));var bridgeEast=SurfacePoint(new Vector3(bridgeX+9.4f,0,bridgeZ));
    Place(bridgeWest);yield return Walk(bridgeEast,sport+" garden bridge crossing",false);
    yield return Walk(bridgeWest,sport+" garden bridge return",false);
    Place(new Vector3(0,-3,0));app.LocalAthlete.Simulate(default,.02f);Check(Vector3.Distance(app.LocalAthlete.transform.position,routes.safeReturn)<.2f,sport+" safe return after sea fall");
    // Measure rendering separately from readback, screenshot encoding and route stepping.
    app.LocalAthlete.capsule.enabled=false;app.LocalAthlete.gameObject.SetActive(false);camera.transform.position=positions[0];camera.transform.LookAt(looks[0]);
    // Batch-mode players skip automatic rendering. Explicit render plus a one-pixel
    // readback fences the GPU; this is a conservative offscreen rendering benchmark,
    // not the empty batch update loop's misleading Time.deltaTime FPS.
    var bench=new RenderTexture(1920,1080,24){antiAliasing=4};var fence=new Texture2D(1,1,TextureFormat.RGB24,false);
    var times=new List<float>();
    for(int i=0;i<210;i++){
     var clock=System.Diagnostics.Stopwatch.StartNew();var old=RenderTexture.active;camera.targetTexture=bench;camera.Render();RenderTexture.active=bench;fence.ReadPixels(new Rect(0,0,1,1),0,0);camera.targetTexture=null;RenderTexture.active=old;clock.Stop();
     if(i>=30)times.Add((float)clock.Elapsed.TotalMilliseconds);yield return null;
    }
    Destroy(bench);Destroy(fence);
    times.Sort();string perf=$"{sport}: 1920x1080 explicit render + GPU synchronization; GPU={SystemInfo.graphicsDeviceName}; average_ms={times.Average():F2}; p95_ms={times[(int)(times.Count*.95)]:F2}; equivalent_render_fps={1000/times.Average():F1}; Unity_reserved_MB={Profiler.GetTotalReservedMemoryLong()/1048576}; managed_MB={GC.GetTotalMemory(false)/1048576}; graphics_MB={Profiler.GetAllocatedMemoryForGraphicsDriver()/1048576}\n";
    File.AppendAllText(Path.Combine(folder,"performance.txt"),perf);Debug.Log(perf);
    app.LocalAthlete.gameObject.SetActive(true);app.LocalAthlete.capsule.enabled=true;
    app.view.enabled=true;
    foreach(var location in new[]{inside,entry,top}){
     Place(location);
     for(int mode=0;mode<3;mode++){app.view.mode=mode;app.view.yaw=0;app.view.pitch=16;yield return new WaitForSeconds(.2f);Check(!Physics.CheckSphere(camera.transform.position,.04f,1<<8,QueryTriggerInteraction.Ignore),sport+" camera mode "+mode+" clearance at "+location);}
    }
   }
   foreach(var sport in new[]{SportId.Golf,SportId.Fishing,SportId.Football,SportId.Basketball}){app.SelectSport(sport);Check(app.environments.roots.Count(r=>r.activeSelf)==1,"Environment switch "+sport);}
   File.AppendAllText(Path.Combine(folder,"review.txt"),"STADIUM_REVIEW_COMPLETE\n");Application.Quit(failed?1:0);
  }
  Vector3 SurfacePoint(Vector3 p){if(Physics.Raycast(p+Vector3.up*5,Vector3.down,out var hit,10,1<<8))return hit.point+Vector3.up*.08f;throw new InvalidOperationException("Missing route floor at "+p);}
  void Place(Vector3 p){var a=app.LocalAthlete;a.capsule.enabled=false;a.transform.position=p;a.capsule.enabled=true;a.ResetLocomotion();Physics.SyncTransforms();}
  IEnumerator Stairs(int index,bool up,bool filming){
   var points=up?routes.stairs[index].points:routes.stairs[index].points.Reverse().ToArray();
   foreach(var point in points)yield return Walk(point,routes.sport+" stair waypoint",filming,false);
   var destination=up?routes.stairTop[index]:routes.stairBottom[index];Check(Vector3.Distance(app.LocalAthlete.transform.position,destination)<.5f,routes.sport+" stair "+(up?"up ":"down ")+index+" reached="+app.LocalAthlete.transform.position);
  }
  IEnumerator Walk(Vector3 destination,string label,bool filming,bool report=true){
   var a=app.LocalAthlete;float travel=Vector3.Distance(a.transform.position,destination);int limit=Mathf.CeilToInt((travel/4+6)*60);int tick=0;float stalled=0;Vector3 last=a.transform.position;
   while(tick<limit){
    Vector3 delta=destination-a.transform.position;delta.y=0;if(delta.magnitude<.20f)break;
    float heading=Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg;
    for(int step=0;step<(filming?2:8);step++){
     var remaining=destination-a.transform.position;remaining.y=0;if(remaining.magnitude<.18f)break;
     heading=Mathf.Atan2(remaining.x,remaining.z)*Mathf.Rad2Deg;a.Simulate(new PlayerCommand{move=Vector2.up,heading=heading},1f/60);tick++;
    }
    camera.transform.position=a.transform.position+Vector3.up*1.65f;camera.transform.rotation=Quaternion.Euler(-3,heading,0);camera.fieldOfView=70;
    yield return null;
    if(filming){yield return new WaitForEndOfFrame();Capture(Path.Combine(folder,routes.sport+"_WalkthroughFrames",(frame++).ToString("D05")+".jpg"),true);}
    if(Vector3.Distance(last,a.transform.position)<.008f)stalled+=filming?2f/60:8f/60;else stalled=0;last=a.transform.position;if(stalled>2)break;
   }
   if(!filming)for(int settle=0;settle<12;settle++)a.Simulate(default,1f/60);
   var error=destination-a.transform.position;if(report)Check(new Vector2(error.x,error.z).magnitude<.45f&&Mathf.Abs(error.y)<.5f,label+" reached="+a.transform.position+" target="+destination);
  }
  void Capture(string path,bool video=false){
   int w=video?1280:1920,h=video?720:1080;
   if(!target||target.width!=w){if(target)Destroy(target);if(pixels)Destroy(pixels);target=new RenderTexture(w,h,24){antiAliasing=4};pixels=new Texture2D(w,h,TextureFormat.RGB24,false);}
   var old=RenderTexture.active;var prior=camera.targetTexture;camera.targetTexture=target;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,w,h),0,0);pixels.Apply();
   File.WriteAllBytes(path,video?pixels.EncodeToJPG(88):pixels.EncodeToPNG());camera.targetTexture=prior;RenderTexture.active=old;
  }
 }
}
#endif
