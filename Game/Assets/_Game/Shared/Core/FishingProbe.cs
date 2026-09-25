#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed partial class DevelopmentProbe {
  bool FishingWalk(Vector3 target,int maxSteps=400){
   var athlete=AppRoot.Instance.LocalAthlete;
   for(int i=0;i<maxSteps;i++){
    var delta=target-athlete.transform.position;delta.y=0;if(delta.magnitude<.3f)return athlete.transform.position.y>.4f;
    athlete.Simulate(new PlayerCommand{move=Vector2.up,heading=Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg,sprint=true},.02f);
   }
   return false;
  }
  IEnumerator FishingAudit(){
   yield return new WaitForSeconds(2);var app=AppRoot.Instance;
   app.SelectSport(SportId.Fishing);app.Show("sports");yield return null;
   Check(FindObjectsByType<Text>(FindObjectsSortMode.None).Any(t=>t.text=="Fishing  /  Explore lagoon"),"FISHING_MENU_ENTRY");
   Capture(Path.ChangeExtension(output,"sports.png"));
   app.EnterOffline();yield return new WaitForSeconds(.4f);Physics.SyncTransforms();
   var lagoon=(FishingLagoonView)app.stadium;var def=app.environments.Current;
   Check(app.environments.roots.Count(r=>r.activeSelf)==1,"FISHING_ONE_ENVIRONMENT");
   Check(def.maxPlayers==5&&def.spawnPositions.Distinct().Count()==5,"FIVE_SPAWNS_CAPACITY");
   Check(lagoon.playerStands.Length==5&&lagoon.playerStands.All(s=>s.moduleType=="PlayerStand"),"FIVE_INDEPENDENT_STANDS");
   var standMeshes=lagoon.playerStands.Select(s=>s.GetComponentInChildren<MeshFilter>().sharedMesh).ToArray();
   Check(standMeshes.All(m=>m==standMeshes[0]),"STANDS_SHARE_REPLACEABLE_MESH");
   Check(lagoon.GetComponentsInChildren<FishingModule>().Select(m=>m.moduleType).Distinct().Count()==16,"SIXTEEN_MODULE_TYPES");
   Check(lagoon.GetComponentsInChildren<MeshRenderer>().All(r=>r.sharedMaterials.All(m=>m&&m.shader&&m.shader.isSupported)),"MATERIALS_SUPPORTED");
   for(int i=0;i<5;i++){
    var spawn=def.Spawn(i);var stand=lagoon.standingPositions[i];
    Check(Physics.Raycast(spawn+Vector3.up,Vector3.down,out var floor,3,1<<8)&&Mathf.Abs(floor.point.y-1.2f)<.1f,"SPAWN_FLOOR_"+i);
    IslandTeleport(spawn);Check(FishingWalk(stand),"DECK_ENTRY_"+i+" position="+app.LocalAthlete.transform.position);
    var inward=-new Vector3(stand.x,0,stand.z).normalized;
    for(int k=0;k<120;k++)app.LocalAthlete.Simulate(new PlayerCommand{move=Vector2.up,heading=Mathf.Atan2(inward.x,inward.z)*Mathf.Rad2Deg,sprint=true},.02f);
    var p=app.LocalAthlete.transform.position;
    Check(new Vector2(p.x,p.z).magnitude>20.8f&&p.y>.9f,"DECK_WATER_CONTAINMENT_"+i+" position="+p);
    Check(FishingWalk(spawn),"DECK_EXIT_"+i);
    foreach(int side in new[]{-1,1}){
     IslandTeleport(stand);float bearing=Mathf.Atan2(stand.x,stand.z)*Mathf.Rad2Deg;
     for(int k=0;k<100;k++)app.LocalAthlete.Simulate(new PlayerCommand{move=Vector2.up,heading=bearing+side*90,sprint=true},.02f);
     var local=lagoon.playerStands[i].transform.InverseTransformPoint(app.LocalAthlete.transform.position);
     Check(Mathf.Abs(local.x)<3&&app.LocalAthlete.transform.position.y>.9f,"DECK_SIDE_CONTAINMENT_"+i+"_"+side+" local="+local);
    }
   }
   // Continuous circuit includes a straight crossing of the northern inlet.
   IslandTeleport(new Vector3(0,1.35f,-36));bool loop=true;
   for(int degree=185;degree<=535;degree+=5){
    float a=degree*Mathf.Deg2Rad;var point=new Vector3(36*Mathf.Sin(a),1.2f,36*Mathf.Cos(a));
    if(point.z>34&&Mathf.Abs(point.x)<7)point.z=36;
    if(!FishingWalk(point,140)){Record("LOOP_STUCK angle="+degree+" actual="+app.LocalAthlete.transform.position+" target="+point);loop=false;break;}
   }
   Check(loop,"FULL_COASTAL_LOOP_AND_BRIDGE");
   for(int i=0;i<5;i++){
    float a=(216+i*72)*Mathf.Deg2Rad;var start=new Vector3(31*Mathf.Sin(a),1.35f,31*Mathf.Cos(a));
    if(i==2)start=new Vector3(0,1.35f,36); // The north gap is water; start on its bridge.
    IslandTeleport(start);
    for(int k=0;k<100;k++)app.LocalAthlete.Simulate(new PlayerCommand{move=Vector2.up,heading=216+i*72+180,sprint=true},.02f);
    var p=app.LocalAthlete.transform.position;Check(new Vector2(p.x,p.z).magnitude>27.8f&&p.y>.3f,"INNER_SHORE_CONTAINMENT_"+i+" position="+p);
   }
   foreach(var stand in lagoon.playerStands){
    var renderer=stand.GetComponentsInChildren<MeshRenderer>().First(r=>r.sharedMaterials.Any(m=>m.name=="LG_Accent"));
    int index=System.Array.FindIndex(renderer.sharedMaterials,m=>m.name=="LG_Accent");var block=new MaterialPropertyBlock();renderer.GetPropertyBlock(block,index);
    Check(Vector4.Distance(block.GetColor("_BaseColor"),stand.accent)<.001f,"STATION_ACCENT_"+stand.slotId);
   }
   foreach(var sport in new[]{SportId.Football,SportId.Basketball,SportId.Golf,SportId.Fishing}){
    app.SelectSport(sport);Check(app.environments.roots.Count(r=>r.activeSelf)==1,"SPORT_SWITCH_"+sport);
   }
   app.EnterOffline();IslandTeleport(lagoon.standingPositions[0]);app.view.yaw=0;app.view.pitch=12;
   for(int mode=0;mode<3;mode++){app.view.mode=mode;yield return new WaitForSeconds(.3f);Capture(Path.ChangeExtension(output,"camera"+mode+".png"));}
   app.view.enabled=false;var camera=Camera.main;
   camera.transform.position=new Vector3(0,98,-112);camera.transform.LookAt(Vector3.zero);float oldFov=camera.fieldOfView;camera.fieldOfView=42;
   var ui=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
   foreach(var c in ui)c.gameObject.SetActive(false);yield return null;Capture(Path.ChangeExtension(output,"overview.png"));
   camera.fieldOfView=oldFov;camera.transform.position=new Vector3(3,5.9f,-31);camera.transform.LookAt(new Vector3(0,3,6));yield return null;Capture(Path.ChangeExtension(output,"deck.png"));
   camera.transform.position=new Vector3(15,11,25);camera.transform.LookAt(new Vector3(0,1.2f,36));yield return null;Capture(Path.ChangeExtension(output,"bridge.png"));
   foreach(var c in ui)c.gameObject.SetActive(true);app.view.enabled=true;app.view.mode=1;app.EnterOffline();
   Check(!app.rooms.Connected,"FISHING_OFFLINE");Record("FISHING_AUDIT_COMPLETE");
  }
 }
}
#endif
