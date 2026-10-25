#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WhatTheFish {
 // Explicit opt-in, exercises the shipped scene colliders and the actual player motor.
 public sealed class VenueCollisionReview:MonoBehaviour {
  [Serializable] public class Sample {public string path;public Vector3 point,normal;}
  [Serializable] public class Samples {public Sample[] items;}
  VenueCollisionContacts contacts;string folder;int checks;bool failed;AppRoot app;Athlete actor;
  static string ObjectPath(Transform t)=>t.parent?ObjectPath(t.parent)+"/"+t.name:t.name;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-venueCollisionReview");if(i>=0&&i+1<args.Length)new GameObject("Venue collision review").AddComponent<VenueCollisionReview>().folder=args[i+1];}
  void Check(bool ok,string label){checks++;failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  void Place(Vector3 point){actor.capsule.enabled=false;actor.transform.position=point;actor.ResetLocomotion();actor.capsule.enabled=true;Physics.SyncTransforms();}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");float until=Time.realtimeSinceStartup+120;
   while(!(DevelopmentProbe.ReadyToReviewSport&&PackagedContentBoot.Ready)&&Time.realtimeSinceStartup<until)yield return null;
   Check(DevelopmentProbe.ReadyToReviewSport,"packaged sport ready");if(!DevelopmentProbe.ReadyToReviewSport){Finish();yield break;}
   yield return new WaitForSeconds(.5f);Physics.SyncTransforms();app=AppRoot.Instance;actor=app.LocalAthlete;contacts=actor.gameObject.AddComponent<VenueCollisionContacts>();DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;while(IslandTutorialUI.DismissTop()){}
   app.view.enabled=false;QualitySettings.vSyncCount=0;Application.targetFrameRate=60;
   Check(GraphicsSettings.currentRenderPipeline&&GraphicsSettings.currentRenderPipeline.name.Contains("Desktop"),"Windows desktop pipeline preserved");
   Check(IslandContactShading.Resolve(Camera.main)!=null,"Windows contact shading retained");
   var all=FindObjectsByType<MeshCollider>(FindObjectsSortMode.None).Where(c=>c.name=="Venue solid collision"&&c.enabled).ToDictionary(c=>ObjectPath(c.transform));
   Check(all.Count>20,"active venue solid proxies="+all.Count);
   var loaded=new List<Sample>();
   foreach(var scene in new[]{"Bootstrap","SkySail_"+app.SelectedSport})loaded.AddRange(JsonUtility.FromJson<Samples>(File.ReadAllText(Path.Combine(folder,"..",scene+"-samples.json"))).items);
   int surfaces=0,movement=0;var moved=new HashSet<string>();var categories=new HashSet<string>();
   foreach(var sample in loaded){
    if(!all.TryGetValue(sample.path,out var collider)||!collider.enabled||!collider.gameObject.activeInHierarchy)continue;
    if(collider.bounds.center.magnitude>700)continue;
    var p=collider.transform.TransformPoint(sample.point);var n=collider.transform.TransformDirection(sample.normal).normalized;
    Check(collider.Raycast(new Ray(p+n*.16f,-n),out var surface,.36f),"surface "+sample.path);surfaces++;
    if(movement>=45||moved.Contains(sample.path)||Mathf.Abs(n.y)>.1f)continue;
    var outward=Vector3.ProjectOnPlane(n,Vector3.up).normalized;var start=p+outward*1.1f;
    if(!Physics.Raycast(start+Vector3.up*.25f,Vector3.down,out var ground,3.5f,1<<8,QueryTriggerInteraction.Ignore))continue;
    float height=p.y-ground.point.y;if(height<.45f||height>1.55f)continue;
    start.y=ground.point.y+.04f;
    if(Physics.CheckCapsule(start+Vector3.up*.37f,start+Vector3.up*1.45f,.29f,1<<8,QueryTriggerInteraction.Ignore))continue;
    // A camera sweep must begin in free space. Raw facade triangle samples
    // can start inside another slat of the same concave mesh; PhysX intentionally
    // does not report a sweep from an already overlapping shape.
    Check(Physics.SphereCast(p+outward*1.1f,.12f,-outward,out _,1.35f,1<<8,QueryTriggerInteraction.Ignore),"camera from clear player approach "+sample.path);
    Place(start);contacts.target=collider;contacts.touched=false;var heading=Quaternion.LookRotation(-outward).eulerAngles.y;
    for(int step=0;step<100;step++)actor.Simulate(new PlayerCommand{move=Vector2.up,heading=heading,sprint=true},1f/60);
    float distance=Vector3.Dot(actor.transform.position-p,outward);
    // Low seats/rocks may legitimately be stepped onto. They must not swallow the body.
    Check(contacts.touched||distance>-.12f||actor.transform.position.y>p.y+.05f,"sprint solid "+sample.path+" signedDistance="+distance.ToString("F3"));
    if(movement<10&&collider.bounds.max.y>start.y+3){
     foreach(bool jump in new[]{false,true}){
      Place(start);contacts.touched=false;
      for(int tick=0;tick<100;tick++)actor.Simulate(new PlayerCommand{move=Vector2.up,heading=heading,sprint=true,jump=jump&&tick==5,tackle=!jump&&tick==5&&app.SelectedSport==SportId.Football},1f/60);
      float clearance=Vector3.Dot(actor.transform.position-p,outward);Check(contacts.touched||clearance>-.12f,(jump?"jump":"slide/run")+" solid "+sample.path);
     }
    }
    if(movement<5){Camera.main.transform.position=start+outward*3+Vector3.up*2;Camera.main.transform.LookAt(p);Capture("solid-"+movement);}
    movement++;moved.Add(sample.path);
    categories.Add(sample.path.Contains("Seats")?"seats":sample.path.Contains("Palm")?"trunks":sample.path.Contains("Coastal stadium")?"facade":sample.path.Contains("Sail Station")?"station":"other solids");
    if(movement%5==0)yield return null;
   }
   Check(surfaces>100,"surface samples="+surfaces);Check(movement>=8,"actual sprint contacts="+movement+" categories="+string.Join(",",categories));
   // LOD changes must never toggle physical solids.
   foreach(var group in FindObjectsByType<LODGroup>(FindObjectsSortMode.None).Where(g=>g.GetComponentInChildren<MeshCollider>())){group.ForceLOD(group.lodCount-1);}
   yield return null;Check(all.Values.All(c=>c.enabled&&c.gameObject.activeInHierarchy),"collisions survive distant LOD");
   foreach(var group in FindObjectsByType<LODGroup>(FindObjectsSortMode.None))group.ForceLOD(-1);
   var routes=CoastalVenueRoutes.Active;
   if(routes){
    for(int i=0;i<routes.entries.Length;i++){
     Place(routes.entries[i]);yield return Walk(routes.inside[i],"entry "+i);yield return Walk(routes.entries[i],"exit "+i);
     Place(routes.stairBottom[i]);foreach(var p in routes.stairs[i].points)yield return Walk(p,"stair up "+i,false);
     Check(Vector3.Distance(actor.transform.position,routes.stairTop[i])<.6f,"stair top "+i);yield return Walk(routes.concourse[i],"concourse "+i);yield return Walk(routes.stairTop[i],"landing return "+i);
     foreach(var p in routes.stairs[i].points.Reverse())yield return Walk(p,"stair down "+i,false);
     Check(Vector3.Distance(actor.transform.position,routes.stairBottom[i])<.6f,"stair bottom "+i);
    }
    foreach(var aisle in routes.aisles){Place(aisle.points[0]);foreach(var p in aisle.points)yield return Walk(p,"spectator aisle");foreach(var p in aisle.points.Reverse())yield return Walk(p,"spectator aisle return");}
    float x=(app.SelectedSport==SportId.Football?132:70)*.47f,z=-(app.SelectedSport==SportId.Football?160:83)*.66f;
    Vector3 west=Floor(new Vector3(x-9.4f,0,z)),east=Floor(new Vector3(x+9.4f,0,z));Place(west);yield return Walk(east,"garden bridge");yield return Walk(west,"garden bridge return");
   }else{
    var env=RefinedIslandEnvironment.Active;
    foreach(var route in env.layout.routes){Place(NavigationPoint(route.points[0]));foreach(var p in route.points){
     // Legacy preview routes predate five-hole rocks and the station gangway.
     // Walk around those existing solids instead of treating them as a clear line.
     var destination=NavigationPoint(p);var from=actor.transform.position;
     if(!ClearSegment(from,destination)){
      var side=Vector3.Cross(Vector3.up,destination-from).normalized;bool found=false;
      for(int offset=1;offset<=10&&!found;offset++)foreach(float sign in new[]{1f,-1f}){
       Vector3 a=Floor(from+side*offset*sign),b=Floor(destination+side*offset*sign);
       if(ClearSegment(from,a)&&ClearSegment(a,b)&&ClearSegment(b,destination)){yield return Walk(a,route.name+" detour out");yield return Walk(b,route.name+" detour around");found=true;break;}
      }
     }
     yield return Walk(destination,route.name);
    }}
   }
   // Shared station apron/berth was previously traversable; structural collision
   // must leave its walking platform and cabin doorway clear.
   var station=SkySailWorld.Instance.stations[(int)app.SelectedSport].transform;
   Vector3 left=station.TransformPoint(new Vector3(-4,.82f,-5)),right=station.TransformPoint(new Vector3(4,.82f,-5));
   Place(left);yield return Walk(right,"station boarding apron");yield return Walk(left,"station boarding apron return");
   Finish();
  }
  Vector3 Floor(Vector3 p){return Physics.Raycast(p+Vector3.up*1.2f,Vector3.down,out var hit,6,1<<8)?hit.point+Vector3.up*.08f:p;}
  Vector3 NavigationPoint(Vector3 raw){
   bool Free(Vector3 p)=>!Physics.CheckCapsule(p+Vector3.up*.4f,p+Vector3.up*1.4f,.3f,1<<8|1<<9,QueryTriggerInteraction.Ignore);
   var point=Floor(raw);if(Free(point))return point;
   for(float radius=.5f;radius<=5;radius+=.5f)for(int i=0;i<16;i++){
    float angle=i*Mathf.PI/8;var next=Floor(raw+new Vector3(Mathf.Cos(angle)*radius,0,Mathf.Sin(angle)*radius));if(Free(next))return next;
   }return point;
  }
  bool ClearSegment(Vector3 start,Vector3 end){
   int steps=Mathf.Max(1,Mathf.CeilToInt(Vector3.Distance(start,end)/.4f));var previous=Floor(start);
   for(int i=1;i<=steps;i++){
    var p=Floor(Vector3.Lerp(start,end,i/(float)steps));if(Mathf.Abs(p.y-previous.y)>.32f)return false;
    var delta=p-previous;
    if(Physics.CapsuleCast(previous+Vector3.up*.4f,previous+Vector3.up*1.4f,.29f,delta.normalized,delta.magnitude,1<<8|1<<9,QueryTriggerInteraction.Ignore))return false;
    previous=p;
   }return true;
  }
  IEnumerator Walk(Vector3 destination,string label,bool report=true){
   float travel=Vector3.Distance(actor.transform.position,destination);int limit=Mathf.CeilToInt((travel/3+5)*60);int ticks=0;Vector3 last=actor.transform.position;int stalled=0;
   while(ticks<limit){
    var delta=Vector3.ProjectOnPlane(destination-actor.transform.position,Vector3.up);if(delta.magnitude<.18f)break;
    for(int k=0;k<12;k++){
     delta=Vector3.ProjectOnPlane(destination-actor.transform.position,Vector3.up);if(delta.magnitude<.18f)break;
     actor.Simulate(new PlayerCommand{move=Vector2.up*Mathf.Clamp01(delta.magnitude/.9f),heading=Quaternion.LookRotation(delta).eulerAngles.y},1f/60);ticks++;
    }
    if(Vector3.Distance(last,actor.transform.position)<.005f)stalled++;else stalled=0;last=actor.transform.position;if(stalled>10)break;yield return null;
   }
   for(int k=0;k<12;k++)actor.Simulate(default,1f/60);
   var error=destination-actor.transform.position;if(report){bool passed=new Vector2(error.x,error.z).magnitude<.45f&&Mathf.Abs(error.y)<.65f;
    if(!passed){Camera.main.transform.position=actor.transform.position+Vector3.up*3-actor.transform.forward*4;Camera.main.transform.LookAt(actor.transform.position+Vector3.up);Capture("blocked-"+checks);}
    Check(passed,label+" actual="+actor.transform.position+" target="+destination);}
  }
  void Capture(string name){
   var rt=RenderTexture.GetTemporary(1280,720,24);var old=RenderTexture.active;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);
   RenderPipeline.SubmitRenderRequest(Camera.main,new UniversalRenderPipeline.SingleCameraRequest{destination=rt});RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),tex.EncodeToPNG());RenderTexture.active=old;Destroy(tex);RenderTexture.ReleaseTemporary(rt);
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"VENUE_COLLISION_COMPLETE success="+!failed+" checks="+checks+"\n");Application.Quit(failed?1:0);}
 }
 public sealed class VenueCollisionContacts:MonoBehaviour {public Collider target;public bool touched;void OnControllerColliderHit(ControllerColliderHit hit){if(hit.collider==target)touched=true;}}
}
#endif
