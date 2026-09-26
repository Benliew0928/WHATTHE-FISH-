#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public sealed class RefinedIslandReview:MonoBehaviour {
  string folder;bool failed,record,quick;int frame;Camera camera;AppRoot app;IslandLayout layout;
  RenderTexture target;Texture2D pixels;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-islandReview");if(i<0)return;
   var r=new GameObject("G2 L2 environment validation").AddComponent<RefinedIslandReview>();r.folder=a[i+1];r.record=a.Contains("-recordRoutes");r.quick=a.Contains("-captureOnly");
  }
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(folder,"review.txt"),(ok?"PASS ":"FAIL ")+label+"\n");Debug.Log("ISLAND_REVIEW "+(ok?"PASS ":"FAIL ")+label);}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"review.txt"),"");yield return new WaitForSeconds(3);
   QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;app=AppRoot.Instance;camera=Camera.main;
   // This diagnostic owns fixed-step controller simulation. The normal UI loop
   // would otherwise apply a second idle step during every captured frame.
   app.enabled=false;
   float stadiumBias=app.environments.mainLight.shadowBias,stadiumNormalBias=app.environments.mainLight.shadowNormalBias;
   foreach(var sport in new[]{SportId.Golf,SportId.Fishing}){
    app.SelectSport(sport);app.EnterOffline();app.view.target=app.LocalAthlete;app.view.active=true;app.view.enabled=false;var env=app.stadium.GetComponent<RefinedIslandEnvironment>();layout=env.layout;Physics.SyncTransforms();
    Check(layout.concept==(sport==SportId.Golf?"G2":"L2"),sport+" selected layout");
    Check(layout.spawns.Length==layout.capacity&&layout.spawns.Distinct().Count()==layout.capacity,sport+" capacity and distinct spawns");
    foreach(var p in layout.spawns)Check(Physics.Raycast(p+Vector3.up*.1f,Vector3.down,1,1<<8),sport+" supported spawn "+p);
    Check(app.stadium.GetComponentsInChildren<MeshRenderer>(true).All(r=>r.sharedMaterials.All(m=>m&&m.shader&&m.shader.isSupported)),sport+" supported materials");
    Check(app.stadium.GetComponentsInChildren<LODGroup>().Length>100,sport+" near mid far vegetation and rock LODs");
    Check(env.lighting.sky==RenderSettings.skybox&&camera.farClipPlane==env.lighting.farClip,sport+" lighting profile applied");
    Check(Mathf.Approximately(app.environments.mainLight.shadowBias,env.lighting.shadowBias),sport+" close-detail shadow bias applied");
    if(sport==SportId.Golf)Check(layout.bunkers.Length==5,"Golf five navigable bunkers");
    else {
     var v=(FishingLagoonView)app.stadium;Check(v.playerStands.Length==5&&v.playerStands.Select(s=>s.slotId).Distinct().Count()==5,"Fishing five stable independent station slots");
     foreach(var stand in v.playerStands){
      var r=stand.GetComponentsInChildren<MeshRenderer>().First(x=>x.sharedMaterials.Any(m=>m.name=="LG_Accent"));int index=Array.FindIndex(r.sharedMaterials,m=>m.name=="LG_Accent");var block=new MaterialPropertyBlock();r.GetPropertyBlock(block,index);
      Check(Vector4.Distance(block.GetColor("_BaseColor"),stand.accent)<.001f,"Fishing station accent "+stand.slotId);
     }
    }
    foreach(var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))if(canvas.renderMode!=RenderMode.WorldSpace)canvas.gameObject.SetActive(false);
    app.LocalAthlete.capsule.enabled=false;app.LocalAthlete.gameObject.SetActive(false);
    foreach(var view in layout.views){
     camera.transform.position=view.position;camera.transform.LookAt(view.target);camera.fieldOfView=view.fov;
     yield return new WaitForSeconds(.25f);Capture(Path.Combine(folder,sport+"_"+view.name+".png"));
    }
    // A downward 1–2m material inspection supplements the authored landscape views.
    var ground=layout.spawns[0]+(sport==SportId.Golf?Vector3.forward*6:Vector3.zero);
    if(Physics.Raycast(ground+Vector3.up*20,Vector3.down,out var groundHit,30,1<<8))ground=groundHit.point;
    camera.transform.position=ground+Vector3.up*1.65f;camera.transform.LookAt(ground+Vector3.forward*1.1f);camera.fieldOfView=60;
    yield return new WaitForSeconds(.25f);Capture(Path.Combine(folder,sport+"_13_Ground_Material_Close.png"));
    if(sport==SportId.Golf){var spring=layout.views.Single(v=>v.name=="09_Cascade");camera.transform.position=spring.position+Vector3.up*30;camera.transform.LookAt(spring.target);camera.fieldOfView=55;yield return new WaitForSeconds(.25f);Capture(Path.Combine(folder,"Golf_14_Spring_Terraces.png"));}
    app.LocalAthlete.gameObject.SetActive(true);app.LocalAthlete.capsule.enabled=true;app.LocalAthlete.HideHead(true);
    if(!quick){
     foreach(var route in layout.routes){
      Place(route.points[0]);bool traversed=true;
      foreach(var point in route.points){yield return Walk(point,false);traversed&=Reached(point);}
      Check(traversed,sport+" "+route.name+" outbound");traversed=true;
      foreach(var point in route.points.Reverse()){yield return Walk(point,false);traversed&=Reached(point);}
      Check(traversed,sport+" "+route.name+" return; actual="+app.LocalAthlete.transform.position);
     }
     // Exercise a representative sample around both shores with the real controller.
     for(int i=0;i<layout.boundaries.Length;i+=Mathf.Max(1,layout.boundaries.Length/32)){
      var edge=layout.boundaries[i];var middle=(edge.a+edge.b)*.5f;var along=edge.b-edge.a;along.y=0;var normal=Vector3.Cross(along.normalized,Vector3.up);
      Vector3 start=default;bool supported=false;
      foreach(float sign in new[]{1f,-1f}){var p=middle+normal*sign*1.3f;if(Physics.Raycast(p+Vector3.up*20,Vector3.down,out var hit,35,1<<8)&&hit.point.y>layout.sea_level+.1f){start=hit.point+Vector3.up*.12f;supported=true;break;}}
      if(!supported)continue;Place(start);var d=middle-start;d.y=0;float heading=Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg;
      for(int step=0;step<100;step++)app.LocalAthlete.Simulate(new PlayerCommand{move=Vector2.up,heading=heading,sprint=true},.02f);
      Check(app.LocalAthlete.transform.position.y>layout.sea_level-.25f&&Vector3.Distance(app.LocalAthlete.transform.position,layout.safe_return)>1,sport+" shoreline containment "+i);
     }
     Place(new Vector3(0,layout.sea_level-3,0));app.LocalAthlete.Simulate(default,.02f);Check(Vector3.Distance(app.LocalAthlete.transform.position,layout.safe_return)<.2f,sport+" environment-specific fall recovery");
     app.view.enabled=true;
     foreach(var location in layout.spawns.Take(2).Concat(layout.routes.Where(r=>r.name.Contains("approach")||r.name.Contains("bridge")).Select(r=>r.points.Last()))){
      Place(location);for(int mode=0;mode<3;mode++){app.view.mode=mode;app.view.yaw=35;app.view.pitch=16;yield return new WaitForSeconds(.2f);Check(!Physics.CheckSphere(camera.transform.position,.04f,1<<8,QueryTriggerInteraction.Ignore),sport+" camera "+mode+" clearance at "+location);}
     }
     app.view.enabled=false;
    }
    if(record){
     app.LocalAthlete.HideHead(true);
     frame=0;Directory.CreateDirectory(Path.Combine(folder,sport+"_WalkthroughFrames"));var points=new List<Vector3>();
     if(sport==SportId.Golf){points.AddRange(layout.routes.Single(r=>r.name=="Pavilion approach").points);points.AddRange(layout.routes.Single(r=>r.name=="Main fairway").points);}
     else{
      var circuit=layout.routes.Single(r=>r.name=="Lagoon circuit").points;points.AddRange(circuit);
      points.AddRange(circuit.Take(circuit.Length/2+1));points.AddRange(layout.routes.Single(r=>r.name=="Station 1 approach").points);
     }
     Place(points[0]);bool complete=true;foreach(var p in points){yield return Walk(p,true);bool reached=Reached(p);complete&=reached;if(!reached)Check(false,sport+" recorded waypoint target="+p+" actual="+app.LocalAthlete.transform.position);}Check(complete,sport+" continuous recorded route with controller; frames="+frame);
    }
    app.view.enabled=true;
   }
   var football=JsonUtility.ToJson(LocalProfile.Stadium);var basketball=JsonUtility.ToJson(LocalProfile.Basketball);
   foreach(var sport in new[]{SportId.Football,SportId.Golf,SportId.Basketball,SportId.Fishing,SportId.Football}){
    app.SelectSport(sport);Check(app.environments.roots.Count(r=>r.activeSelf)==1,"Environment switching "+sport);
    bool refined=sport==SportId.Golf||sport==SportId.Fishing;Check((RefinedIslandEnvironment.Active!=null)==refined,"Active island recovery profile "+sport);
    Check(camera.farClipPlane==3000&&RenderSettings.skybox,"Sky and distance restored "+sport);
    if(!refined)Check(Mathf.Approximately(app.environments.mainLight.shadowBias,stadiumBias)&&Mathf.Approximately(app.environments.mainLight.shadowNormalBias,stadiumNormalBias),sport+" original shadow bias restored");
   }
   Check(football==JsonUtility.ToJson(LocalProfile.Stadium)&&basketball==JsonUtility.ToJson(LocalProfile.Basketball),"Stadium appearance profiles preserved");
   File.AppendAllText(Path.Combine(folder,"review.txt"),"ISLAND_REVIEW_COMPLETE success="+(!failed)+"\n");Application.Quit(failed?1:0);
  }
  void Place(Vector3 p){var a=app.LocalAthlete;a.capsule.enabled=false;a.transform.position=p;a.capsule.enabled=true;a.ResetLocomotion();Physics.SyncTransforms();}
  bool Reached(Vector3 p){var d=p-app.LocalAthlete.transform.position;return new Vector2(d.x,d.z).magnitude<.5f&&Mathf.Abs(d.y)<.65f;}
  IEnumerator Walk(Vector3 p,bool film){
   var a=app.LocalAthlete;int limit=Mathf.CeilToInt((Vector3.Distance(a.transform.position,p)/4+5)*60),tick=0;float stalled=0;var last=a.transform.position;
   while(tick<limit){
    var d=p-a.transform.position;d.y=0;if(d.magnitude<.20f)break;float heading=Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg;
    for(int j=0;j<(film?2:12);j++){d=p-a.transform.position;d.y=0;if(d.magnitude<.18f)break;heading=Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg;a.Simulate(new PlayerCommand{move=Vector2.up,heading=heading},1f/60);tick++;}
    camera.transform.position=a.transform.position+Vector3.up*1.65f;camera.transform.rotation=Quaternion.Euler(-2,heading,0);camera.fieldOfView=70;yield return null;
    if(film)Capture(Path.Combine(folder,layout.sport+"_WalkthroughFrames",(frame++).ToString("D05")+".jpg"),true);
    if(Vector3.Distance(last,a.transform.position)<.008f)stalled+=(film?2f:12f)/60;else stalled=0;last=a.transform.position;if(stalled>1.5f){Debug.LogWarning("ROUTE_STALL target="+p+" actual="+a.transform.position);break;}
   }
   if(!film)for(int settle=0;settle<8;settle++)a.Simulate(default,1f/60);
  }
  void Capture(string path,bool video=false){
   int w=video?1280:1920,h=video?720:1080;if(!target||target.width!=w){if(target)Destroy(target);if(pixels)Destroy(pixels);target=new RenderTexture(w,h,24){antiAliasing=4};pixels=new Texture2D(w,h,TextureFormat.RGB24,false);}
   var old=RenderTexture.active;camera.targetTexture=target;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,w,h),0,0);pixels.Apply();File.WriteAllBytes(path,video?pixels.EncodeToJPG(87):pixels.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=old;
  }
 }
}
#endif
