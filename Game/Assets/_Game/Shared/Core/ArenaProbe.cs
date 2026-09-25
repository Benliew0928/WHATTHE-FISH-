#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace SportsPrototype {
 public sealed partial class DevelopmentProbe {
  void Capture(string path){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
   var camera=Camera.main;var canvas=FindObjectsByType<Canvas>(FindObjectsSortMode.None).FirstOrDefault(c=>c.renderMode==RenderMode.ScreenSpaceOverlay);
   var target=new RenderTexture(1600,900,24);var previous=RenderTexture.active;camera.targetTexture=target;
   if(canvas){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=.5f;}
   Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
   var texture=new Texture2D(1600,900,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1600,900),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());
   camera.targetTexture=null;RenderTexture.active=previous;if(canvas){canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;}Destroy(target);Destroy(texture);
  }
  void Check(bool condition,string label){Record((condition?"PASS ":"FAIL ")+label);}
  IEnumerator Persistence(){
   yield return new WaitForSeconds(2);var app=AppRoot.Instance;
   if(args.Contains("-saveLogo")){var a=app.CurrentAppearance;a.logo=int.Parse(Value("-saveLogo","0"));app.SaveStadium(a);Record("SAVED_LOGO="+a.logo);}
   if(args.Contains("-expectLogo"))Check(app.CurrentAppearance.logo==int.Parse(Value("-expectLogo","0")),"LOGO_PERSISTENCE actual="+app.CurrentAppearance.logo);
  }
  IEnumerator ArenaAudit(){
   var app=AppRoot.Instance;var def=app.environments.Current;var saved=app.CurrentAppearance;
   Check(app.environments.roots.Count(r=>r.activeSelf)==1,"ONE_ACTIVE_ENVIRONMENT");
   Check(def.spawnPositions.Distinct().Count()==10,"TEN_DISTINCT_SPAWNS");
   foreach(var spawn in def.spawnPositions)Check(Physics.Raycast(spawn,Vector3.down,2,1<<8),"SPAWN_HAS_FLOOR "+spawn);
   foreach(var direction in new[]{Vector3.forward,Vector3.back,Vector3.left,Vector3.right})Check(Physics.Raycast(new Vector3(0,1,0),direction,40,1<<8),"ARENA_BOUNDARY "+direction);
   Check(Physics.Raycast(new Vector3(0,10,0),Vector3.up,10,1<<8),"ENCLOSED_ROOF");
   var rally=(BasketballArenaView)app.stadium;
   Check(rally.modularRally&&rally.transform.Find("Court")&&rally.transform.Find("Stadium"),"RALLY_MODULAR_ENVIRONMENT");
   Check(rally.GetComponentsInChildren<LODGroup>(true).Length==48,"RALLY_48_SEATING_LODS");
   app.Show("custom");
   for(int i=0;i<4;i++){
    var a=saved;a.logo=i;app.SaveStadium(a);app.Show("custom");yield return new WaitForSeconds(.7f);
    var arena=(BasketballArenaView)app.stadium;Check(arena.logoCatalog.Count(g=>g.activeSelf)==1&&arena.logoCatalog[i].activeSelf,"LOGO_SELECTION_"+i);
    Capture(Path.ChangeExtension(output,"logo"+i+".png"));
   }
   var padding=rally.GetComponentsInChildren<Renderer>(true).First(r=>r.name=="Hoop__Padding");
   var block=new MaterialPropertyBlock();var original=padding.sharedMaterial.GetColor("_BaseColor");
   for(int i=0;i<4;i++){
    var a=saved;a.palette=i;app.SaveStadium(a);padding.GetPropertyBlock(block,0);
    Check(Vector4.Distance(block.GetColor("_BaseColor"),i==0?original:LocalProfile.Teams[i])<.001f,"RALLY_PALETTE_"+i);
   }
   app.SaveStadium(saved);app.Show("sports");yield return new WaitForSeconds(.7f);Capture(Path.ChangeExtension(output,"sports.png"));
   app.SelectSport(SportId.Football);Check(app.SelectedSport==SportId.Football&&app.environments.roots.Count(r=>r.activeSelf)==1,"SWITCH_TO_FOOTBALL");
   app.SelectSport(SportId.Basketball);Check(app.SelectedSport==SportId.Basketball&&app.CurrentAppearance.logo==saved.logo,"SWITCH_BACK_TO_BASKETBALL");
  }
  bool guestChecked,leaveRequested;
  IEnumerator ArenaCameraAudit(){
   var app=AppRoot.Instance;app.view.mode=0;app.view.pitch=-25;app.view.yaw=35;
   yield return new WaitForSeconds(1);Capture(Path.ChangeExtension(output,"roof.png"));
   app.LocalAthlete.capsule.enabled=false;app.LocalAthlete.transform.position=new Vector3(11.15f,.03f,0);app.LocalAthlete.capsule.enabled=true;
   app.view.mode=1;app.view.yaw=-90;app.view.pitch=16;
   yield return new WaitForSeconds(1);Check(app.view.transform.position.x<11.75f,"THIRD_PERSON_WALL_CLEARANCE");Capture(Path.ChangeExtension(output,"wall-camera.png"));
   app.EnterOffline();app.view.yaw=0;app.view.pitch=16;
   if(args.Contains("-apronAudit"))yield return ApronRotationAudit();
  }
  IEnumerator ApronRotationAudit(){
   var app=AppRoot.Instance;
   var renderers=app.stadium.GetComponentsInChildren<Renderer>(true);
   var apron=renderers.Single(r=>r.name=="Court__Apron").bounds;
   var foundation=renderers.Single(r=>r.name=="Court__Foundation").bounds;
   Check(foundation.max.y<=apron.min.y+.0001f&&apron.max.y-foundation.max.y>=.10f,"APRON_FOUNDATION_SEPARATED");
   var positions=new[]{new Vector3(-9,.08f,0),new Vector3(9,.08f,0),new Vector3(4,.08f,15.5f),new Vector3(-4,.08f,-15.5f)};
   for(int side=0;side<positions.Length;side++){
    app.LocalAthlete.capsule.enabled=false;app.LocalAthlete.transform.position=positions[side];app.LocalAthlete.capsule.enabled=true;
    app.view.mode=1;app.view.pitch=45;
    // A full moving-camera orbit at each margin, capturing opposing views.
    for(int step=0;step<72;step++){
     app.view.yaw=step*5;yield return new WaitForEndOfFrame();
     if(step%18==0)Capture(Path.ChangeExtension(output,"apron-"+side+"-"+step+".png"));
    }
   }
   Record("APRON_ROTATION_COMPLETE sides=4 orbits=4 captures=16");
   app.EnterOffline();app.view.yaw=0;app.view.pitch=16;
  }
  void RoomChecks(){
   var app=AppRoot.Instance;
   if(app.rooms.Connected&&!app.rooms.Host&&NetworkAthlete.HostPlayer&&elapsed>10&&!guestChecked){
    guestChecked=true;var selected=app.SelectedSport;var before=app.CurrentAppearance;var world=NetworkAthlete.HostPlayer.WorldAppearance.Value;
    app.SelectSport(selected==SportId.Basketball?SportId.Football:SportId.Basketball);Check(app.SelectedSport==selected,"GUEST_CANNOT_CHANGE_SPORT");
    var attempt=before;attempt.logo=(before.logo+1)%4;app.SaveStadium(attempt);
    Check(app.CurrentAppearance.logo==before.logo&&NetworkAthlete.HostPlayer.WorldAppearance.Value==world,"GUEST_CANNOT_CHANGE_LOGO");
   }
   if(args.Contains("-leaveAfter")&&!leaveRequested&&elapsed>float.Parse(Value("-leaveAfter","118"))){leaveRequested=true;_=app.rooms.Leave();}
  }
 }
}
#endif
