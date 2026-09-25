#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SportsPrototype {
 public sealed partial class DevelopmentProbe {
  public static bool IslandDriving;public static float IslandHeading;
  void IslandTeleport(Vector3 position){var player=AppRoot.Instance.LocalAthlete;player.capsule.enabled=false;player.transform.position=position;player.capsule.enabled=true;player.ResetLocomotion();Physics.SyncTransforms();}
  Vector3 GolfFloor(float x,float z){
   if(Physics.Raycast(new Vector3(x,100,z),Vector3.down,out var hit,150,1<<8))return hit.point+Vector3.up*.12f;
   return new Vector3(x,-20,z);
  }
  IEnumerator IslandVisuals(){
   var app=AppRoot.Instance;app.EnterOffline();app.view.yaw=0;app.view.pitch=12;
   for(int mode=0;mode<3;mode++){app.view.mode=mode;yield return new WaitForSeconds(.6f);Capture(Path.ChangeExtension(output,"tee-camera"+mode+".png"));}
   IslandTeleport(GolfFloor(166,53));app.view.yaw=8;app.view.pitch=-12;app.view.mode=2;yield return new WaitForSeconds(.6f);Capture(Path.ChangeExtension(output,"cascade.png"));
   IslandTeleport(GolfFloor(4,128));app.view.yaw=180;app.view.pitch=16;app.view.mode=2;yield return new WaitForSeconds(.6f);Capture(Path.ChangeExtension(output,"green.png"));
   // Neutral art review from the running player, followed by restored player controls.
   app.view.enabled=false;var cam=Camera.main;cam.transform.position=new Vector3(290,260,-350);cam.transform.LookAt(new Vector3(0,3,5));
   yield return new WaitForSeconds(.6f);Capture(Path.ChangeExtension(output,"overview.png"));
   app.view.enabled=true;app.EnterOffline();app.view.yaw=0;app.view.pitch=16;app.view.mode=1;
  }
  IEnumerator IslandAudit(){
   yield return new WaitForSeconds(2);var app=AppRoot.Instance;app.SelectSport(SportId.Golf);Physics.SyncTransforms();
   Check(app.environments.roots.Count(r=>r.activeSelf)==1,"GOLF_ONE_ACTIVE_ENVIRONMENT");
   Check(app.environments.Current.spawnPositions.Distinct().Count()==10,"GOLF_TEN_DISTINCT_SPAWNS");
   foreach(var spawn in app.environments.Current.spawnPositions)Check(Physics.Raycast(spawn,Vector3.down,2,1<<8),"GOLF_SPAWN_FLOOR "+spawn);
   Check(Camera.main.farClipPlane==1400&&RenderSettings.fogStartDistance==700&&RenderSettings.fogEndDistance==1300,"GOLF_VIEW_DISTANCE");
   var renderers=app.stadium.GetComponentsInChildren<MeshRenderer>();
   Check(renderers.Length>100&&renderers.All(r=>r.sharedMaterials.All(m=>m&&m.shader.name=="Universal Render Pipeline/Lit")),"TIDEBLOOM_MATERIALS_AND_MESHES");
   Check(app.stadium.GetComponentsInChildren<MeshCollider>().Count(c=>c.name.StartsWith("Terrain__"))==16,"TIDEBLOOM_TERRAIN_COLLISION");
   var football=JsonUtility.ToJson(LocalProfile.Stadium);var basketball=JsonUtility.ToJson(LocalProfile.Basketball);
   var change=app.CurrentAppearance;change.title="SHOULD NOT SAVE";app.SaveStadium(change);LocalProfile.SaveSport(SportId.Golf,change);
   Check(app.CurrentAppearance.title=="ISLAND GREENS"&&football==JsonUtility.ToJson(LocalProfile.Stadium)&&basketball==JsonUtility.ToJson(LocalProfile.Basketball),"GOLF_PREFERENCES_ISOLATED");
   foreach(var sport in new[]{SportId.Football,SportId.Basketball,SportId.Golf}){
    app.SelectSport(sport);Check(app.environments.roots.Count(r=>r.activeSelf)==1,"SWITCH_ENVIRONMENT "+sport);
    Check(Camera.main.farClipPlane==(sport==SportId.Golf?1400:450),"RESTORE_VIEW_DISTANCE "+sport);
   }
   app.EnterOffline();yield return new WaitForSeconds(.5f);
   // Test containment against the exported coast, independent of the old square footprint.
   for(int n=0;n<48;n++){
    float heading=n*7.5f;var direction=Quaternion.Euler(0,heading,0)*Vector3.forward;
    bool found=Physics.Raycast(new Vector3(0,2,0),direction,out var hit,400,1<<9);
    Check(found,"SHORE_RAY_"+n);if(!found)continue;
    var start=hit.point-direction*2;IslandTeleport(GolfFloor(start.x,start.z));
    for(int step=0;step<180;step++)app.LocalAthlete.Simulate(new PlayerCommand{move=Vector2.up,heading=heading,sprint=true},.02f);
    var p=app.LocalAthlete.transform.position;
    // Sliding along a curved coast can pass the original ray distance while
    // remaining inside. Measure the boundary at the final heading instead.
    var radial=new Vector3(p.x,0,p.z);bool finalRay=Physics.Raycast(new Vector3(0,2,0),radial.normalized,out var finalShore,400,1<<9);
    Check(finalRay&&radial.magnitude<finalShore.distance-.1f&&p.y>-.8f,"SHORE_SPRINT_CONTAINMENT_"+n+" clearance="+(finalShore.distance-radial.magnitude));
   }
   // Follow the fairway using the real controller across its changes in elevation.
   IslandTeleport(GolfFloor(-13,-146));bool routeOK=true;float lowestClearance=100;
   for(int segment=0;segment<29;segment++){
    float z=-140+segment*10;float x=24*Mathf.Sin((z+135)/55)-5;var target=new Vector3(x,0,z);
    for(int step=0;step<180;step++){
     var delta=target-app.LocalAthlete.transform.position;delta.y=0;if(delta.magnitude<.45f)break;
     app.LocalAthlete.Simulate(new PlayerCommand{move=Vector2.up,heading=Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg,sprint=true},.02f);
    }
    var p=app.LocalAthlete.transform.position;var flat=p-target;flat.y=0;
    routeOK&=flat.magnitude<1;lowestClearance=Mathf.Min(lowestClearance,p.y-GolfFloor(p.x,p.z).y);
   }
   Check(routeOK&&lowestClearance>-.35f,"FAIRWAY_FULL_TRAVERSAL clearance="+lowestClearance);
   // Sand bowls must remain navigable in and out, not decorative holes in collision.
   foreach(var point in new[]{new Vector2(-42,-112),new Vector2(43,-56),new Vector2(-38,16),new Vector2(38,74),new Vector2(-37,127)}){
    IslandTeleport(GolfFloor(point.x,point.y));var before=app.LocalAthlete.transform.position;
    for(int step=0;step<240;step++)app.LocalAthlete.Simulate(new PlayerCommand{move=Vector2.up,heading=90,sprint=true},.02f);
    Check(app.LocalAthlete.transform.position.x-before.x>25&&app.LocalAthlete.transform.position.y>0,"BUNKER_EXIT "+point);
   }
   if(Physics.Raycast(new Vector3(0,2,0),Vector3.back,out var southShore,400,1<<9)){
    IslandTeleport(GolfFloor(0,southShore.point.z+1));app.view.mode=1;app.view.yaw=0;app.view.pitch=12;yield return new WaitForSeconds(.6f);
    Check(app.view.transform.position.z<southShore.point.z,"SHORE_BOUNDARY_EXCLUDED_FROM_CAMERA");
   }
   yield return IslandVisuals();Check(!app.rooms.Connected,"GOLF_OFFLINE_NO_ROOM");Record("GOLF_AUDIT_COMPLETE");
  }
 }
}
#endif
