#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WhatTheFish;
public static class SpectatorBuild {
 public static void ScanLandings(){
  string output=ProjectBuilder.ResolveBuildOutputPath("../Builds/SpectatorQA/landing-scan.txt");File.WriteAllText(output,"");
  var setup=EditorSceneManager.GetSceneManagerSetup();
  try{foreach(var sport in new[]{SportId.Football,SportId.Fishing}){
   var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_"+sport+".unity",OpenSceneMode.Single);foreach(var root in scene.GetRootGameObjects()){root.transform.position=Vector3.zero;root.SetActive(true);}Physics.SyncTransforms();
   for(int phase=0;phase<100;phase++){int clear=0;
    for(int slot=0;slot<10;slot++){
     Vector3 p;if(sport==SportId.Football){p=RoomSeats.SeatingPerimeter(42,61,17,10.85f,(slot+phase/100f)/10);p.y=8.25f;}else {float angle=(slot*36+phase*.36f)*Mathf.Deg2Rad;p=new Vector3(Mathf.Cos(angle)*30,3,Mathf.Sin(angle)*30);}
     if(!Physics.Raycast(p+Vector3.up*3,Vector3.down,out var floor,10,1<<8,QueryTriggerInteraction.Ignore)||floor.normal.y<.92f)continue;
     p=floor.point+Vector3.up*.035f;if(Physics.OverlapCapsule(p+Vector3.up*.36f,p+Vector3.up*1.45f,.34f,~0,QueryTriggerInteraction.Ignore).Length==0)clear++;
    }
    if(clear==10)File.AppendAllText(output,sport+" phase="+phase+" all ten clear\n");
   }
  }}finally{if(Array.Exists(setup,s=>s.isLoaded&&s.isActive))EditorSceneManager.RestoreSceneManagerSetup(setup);else EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");}
 }
 public static void CheckLandings(){
  string output=ProjectBuilder.ResolveBuildOutputPath("../Builds/SpectatorQA/landing-geometry.txt");Directory.CreateDirectory(Path.GetDirectoryName(output));File.WriteAllText(output,"");bool failed=false;
  var setup=EditorSceneManager.GetSceneManagerSetup();var controller=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Characters/NetworkAthlete.prefab").GetComponent<CharacterController>();
  try{foreach(SportId sport in Enum.GetValues(typeof(SportId))){
   EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity",OpenSceneMode.Single);
   var world=UnityEngine.Object.FindFirstObjectByType<SkySailWorld>();world.scenery.localPosition=-SkySailMap.Center(sport);
   for(int i=0;i<4;i++)foreach(var collider in world.stations[i].GetComponentsInChildren<Collider>(true))collider.enabled=i==(int)sport;
   world.cabin.localPosition=SkySailMap.Berth(sport);world.cabin.localRotation=SkySailMap.Facing(sport);
   var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_"+sport+".unity",OpenSceneMode.Additive);foreach(var root in scene.GetRootGameObjects()){root.transform.position=Vector3.zero;root.SetActive(true);}Physics.SyncTransforms();
   for(int slot=0;slot<30;slot++){
    var landing=slot<10?RoomSeats.SpectatorLanding(sport,slot):RoomSeats.StationLanding(sport,slot-10);bool ground=Physics.Raycast(landing.position+Vector3.up*3,Vector3.down,out var floor,10,1<<8,QueryTriggerInteraction.Ignore);string reason="";
    if(!ground||floor.normal.y<.92f)reason="ground "+floor.point+" normal="+floor.normal;
    else {var p=floor.point+Vector3.up*.035f;var hits=Physics.OverlapCapsule(p+Vector3.up*(controller.radius+.06f),p+Vector3.up*(controller.height-controller.radius),controller.radius+.04f,~0,QueryTriggerInteraction.Ignore);if(hits.Length>0)reason=string.Join(",",Array.ConvertAll(hits,h=>h.name));}
    bool valid=reason.Length==0;failed|=!valid;File.AppendAllText(output,(valid?"PASS ":"FAIL ")+sport+(slot<10?" spectator "+slot:" station "+(slot-10))+" at "+landing.position+" "+reason+"\n");
   }
  }}finally{if(Array.Exists(setup,s=>s.isLoaded&&s.isActive))EditorSceneManager.RestoreSceneManagerSetup(setup);else EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");}
  if(failed)throw new Exception("Spectator landing geometry failed. Inspect Builds/SpectatorQA/landing-geometry.txt");
 }
 public static void Build(){RoomTeleportBuild.Prepare();CheckLandings();ProjectBuilder.BuildWindowsPlayer("../Builds/SpectatorQA/Player/WhatTheFish.exe");}
}
#endif
