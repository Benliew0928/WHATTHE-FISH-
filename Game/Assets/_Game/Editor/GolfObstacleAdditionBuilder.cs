using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WhatTheFish;
using Object=UnityEngine.Object;

// Applies the approved extra obstacles to saved content without regenerating
// terrain, holes, flags, fairways, signs, or any unrelated scene object.
public static class GolfObstacleAdditionBuilder {
 const string Config="Assets/_Game/Sports/Golf/FiveHoleCourse.json";
 const string Scene="Assets/_Game/Scenes/SkySail_Golf.unity";
 const string Prefab="Assets/_Game/Prefabs/Environments/GolfEnvironment.prefab";
 static string Folder=>Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/GolfWaterV5QA"));
 [Serializable] sealed class Layout {public GolfCircuitArtBuilder.Obstacle[] obstacles;public GolfSandPatch[] sandPatches,roughPatches;public GolfWaterPatch[] waterPatches;}
 [Serializable] sealed class Retained {public GolfHole[] holes;public GolfGreen[] greens;public GolfFairway[] fairways;}
 static readonly List<string> report=new();
 static void Check(bool pass,string label){report.Add((pass?"PASS ":"FAIL ")+label);File.WriteAllLines(Path.Combine(Folder,"editor.txt"),report);if(!pass)throw new Exception(label);}
 static string RetainedData(GolfCourse c)=>JsonUtility.ToJson(new Retained{holes=c.holes,greens=c.greens,fairways=c.fairways});
 sealed class Guard {
  readonly Dictionary<Transform,(Vector3,Quaternion,Vector3)> transforms;
  readonly Dictionary<Component,string> components;
  readonly string courseData;
  public Guard(GameObject island,GolfCourse course){
   transforms=island.GetComponentsInChildren<Transform>(true).ToDictionary(t=>t,t=>(t.localPosition,t.localRotation,t.localScale));
   components=island.GetComponentsInChildren<Component>(true).Where(c=>c&&!(c is Transform)&&!(c is GolfCourse)&&!(c is MeshFilter f&&f.name.Contains("Cell_"))).ToDictionary(c=>c,c=>EditorJsonUtility.ToJson(c));
   courseData=RetainedData(course);
  }
  public void Audit(GolfCourse course,string scope){
   Check(transforms.All(p=>p.Key&&p.Key.localPosition==p.Value.Item1&&p.Key.localRotation==p.Value.Item2&&p.Key.localScale==p.Value.Item3),scope+" all existing object positions, rotations and scales unchanged");
   Check(components.All(p=>p.Key&&EditorJsonUtility.ToJson(p.Key)==p.Value),scope+" all existing non-grass components unchanged, including terrain colliders, flags and materials");
   Check(RetainedData(course)==courseData,scope+" exact retained hole, tee, green, route, bypass and par data");
  }
 }
 static void Apply(GameObject island,Layout layout,string scope){
  bool active=island.activeSelf;island.SetActive(true);Physics.SyncTransforms();
  try{
   var course=island.GetComponentInChildren<GolfCourse>(true);GolfPondArtBuilder.RefreshCreatedWater(island,course);var guard=new Guard(island,course);
   var group=course.transform.Find("Circuit obstacles");Check(group,scope+" original obstacle group exists");
   var addedSand=layout.sandPatches.Where(s=>s.name.StartsWith("V2 ")).ToArray();
   foreach(var item in layout.obstacles.Where(o=>o.name.StartsWith("V2 ")||o.name.StartsWith("V5 ")))if(!group.Find(item.name))GolfCircuitArtBuilder.AddObstacle(island,group,item);
   foreach(var patch in addedSand)if(!course.transform.Find(patch.name))GolfCircuitArtBuilder.AddSurfacePatch(island,course,patch);
   foreach(var patch in layout.roughPatches)if(!course.transform.Find(patch.name))GolfCircuitArtBuilder.AddSurfacePatch(island,course,patch,true);
   course.sandPatches=layout.sandPatches;course.roughPatches=layout.roughPatches;
   course.waterPatches=layout.waterPatches.Select(w=>course.waterPatches.Any(old=>old.name==w.name)?course.waterPatches.First(old=>old.name==w.name):w).ToArray();EditorUtility.SetDirty(course);
   GolfCourseBuilder.ClearAddedSandGrass(island,addedSand);GolfPondArtBuilder.Attach(island,course);Physics.SyncTransforms();guard.Audit(course,scope);
   Audit(island,layout,scope);
  }finally{island.SetActive(active);}
 }
 static Vector3 Floor(GameObject island,Vector3 local){
  var ray=new Ray(island.transform.TransformPoint(local+Vector3.up*100),Vector3.down);
  foreach(var collider in island.GetComponentsInChildren<MeshCollider>(true).Where(c=>c.name.StartsWith("Terrain__")))if(collider.Raycast(ray,out var hit,200))return hit.point;
  foreach(var h in island.GetComponentInChildren<GolfCourse>(true).holes)if(Vector2.Distance(new Vector2(local.x,local.z),new Vector2(h.cup.localPosition.x,h.cup.localPosition.z))<=h.cupRadius)return h.cup.position;
  throw new Exception("No obstacle validation terrain at "+local);
 }
 static bool AddedCollider(Collider collider,Transform group){var t=collider.transform;while(t&&t.parent!=group)t=t.parent;return t&&(t.name.StartsWith("V2 ")||t.name.StartsWith("V5 "));}
 static void Audit(GameObject island,Layout layout,string scope){
  var course=island.GetComponentInChildren<GolfCourse>(true);var group=course.transform.Find("Circuit obstacles");
  Check(group.childCount==layout.obstacles.Length&&group.childCount==225,scope+" 84 existing + 141 additional physical props; approved V5 additions");
  Check(group.GetComponentsInChildren<Collider>(true).All(c=>c.enabled&&!c.isTrigger&&c.gameObject.layer==8),scope+" solid obstacles use existing physical collision layer");
  Check(course.sandPatches.Length==20&&course.roughPatches.Length==20&&course.waterPatches.Length==6,scope+" 20 retained sand, 20 rough zones, six water patches");
  foreach(var item in layout.obstacles.Where(o=>o.name.StartsWith("V2 ")||o.name.StartsWith("V5 "))){
   var node=group.Find(item.name);var colliders=node.GetComponentsInChildren<Collider>(true);
   Check(colliders.Length>0&&colliders.All(c=>c.bounds.size.magnitude<35),scope+" grounded kit collision "+item.name);
  }
  var settings=AssetDatabase.LoadAssetAtPath<GolfBallPhysicsSettings>("Assets/_Game/Resources/GolfBallPhysics.asset");
  foreach(var patch in course.sandPatches.Where(p=>p.name.StartsWith("V2 ")))Check(Mathf.Abs(course.RollingResistance(Floor(island,patch.centre),settings)-settings.sandResistance)<.001f,scope+" existing sand resistance "+patch.name);
  foreach(var patch in course.roughPatches){
   bool found=false;foreach(float dx in new[]{-.6f,0,.6f})foreach(float dz in new[]{-.6f,0,.6f}){
    var p=patch.centre+new Vector3(dx*patch.radii.x,0,dz*patch.radii.y);
    if(MobileIslandGrass.GolfSand(p.x,p.z)>0||course.sandPatches.Any(s=>s.Contains(p.x,p.z)))continue;
    found|=Mathf.Abs(course.RollingResistance(Floor(island,p),settings)-settings.roughResistance)<.001f;
   }
   Check(found,scope+" existing rough resistance outside retained sand "+patch.name);
  }
  foreach(var route in course.fairways){
   bool clear=true,wide=true;float closest=float.PositiveInfinity;
   for(int i=1;i<route.points.Length;i++){
    int steps=Mathf.CeilToInt(Vector3.Distance(route.points[i-1],route.points[i]));
    for(int j=0;j<=steps;j++){
     var p=Floor(island,Vector3.Lerp(route.points[i-1],route.points[i],j/(float)steps))+Vector3.up*.5f;
     foreach(var collider in group.GetComponentsInChildren<Collider>(true).Where(c=>c.transform.IsChildOf(group)&&c.transform.parent!=null)){
      float d=Vector3.Distance(p,collider.bounds.ClosestPoint(p));closest=Mathf.Min(closest,d);

     }
     Check(!course.TouchesWater(p,p,GolfBall.Radius),scope+" water-free route sample "+route.hole+" "+p);
     var hits=Physics.OverlapSphere(p,.35f,1<<8,QueryTriggerInteraction.Ignore);clear&=!hits.Any(c=>c.transform.IsChildOf(group));
     wide&=!Physics.OverlapSphere(p,4,1<<8,QueryTriggerInteraction.Ignore).Any(c=>c.transform.IsChildOf(group)&&AddedCollider(c,group));
    }
   }
   Check(wide,scope+" added solids retain 8m route corridor "+route.hole+(route.alternative?" bypass":""));
   report.Add($"MEASURE {scope} route={route.hole} bypass={route.alternative} closest solid distance={closest:F3}m");
   Check(clear,scope+" safe route ball clearance "+route.hole+(route.alternative?" bypass":""));
  }
  foreach(var water in course.waterPatches){
   Check(course.transform.Find(water.name),scope+" water art exists "+water.name);
   var visual=course.transform.Find(water.name+"/Pond water");
   Check(visual.localPosition==water.centre&&visual.GetComponent<MeshFilter>().sharedMesh.vertices[0]==Vector3.zero,scope+" water mesh local origin aligned "+water.name);
   var at=course.transform.TransformPoint(water.centre);Check(course.TouchesWater(at,at,GolfBall.Radius),scope+" water surface contact "+water.name);
   Check(!course.TouchesWater(at+Vector3.up,at+Vector3.up,GolfBall.Radius),scope+" air above water safe "+water.name);
   report.Add($"MEASURE {scope} {water.name} water level={water.centre.y:F3} bank rise above centre={water.centre.y-GolfPondArtBuilder.Ground(island,water.centre.x,water.centre.z).y:F3}m");
  }
  foreach(var h in course.holes){
   var ground=Floor(island,island.transform.InverseTransformPoint(h.cup.position))+Vector3.up*.3f;
   Check(!Physics.OverlapSphere(ground,5,1<<8,QueryTriggerInteraction.Ignore).Any(c=>c.transform.IsChildOf(group)),scope+" hole "+h.number+" 5m solid-free putting area");
   ground=Floor(island,island.transform.InverseTransformPoint(h.tee.position))+Vector3.up*.5f;
   Check(!Physics.OverlapSphere(ground,3.5f,1<<8,QueryTriggerInteraction.Ignore).Any(c=>c.transform.IsChildOf(group)),scope+" tee "+h.number+" unchanged clear stance");
  }
 }
 [MenuItem("WHATTHE FISH?/Golf/Add approved V5 obstacles and ponds")]
 public static void ApplyAndReview(){
  Directory.CreateDirectory(Folder);report.Clear();var layout=JsonUtility.FromJson<Layout>(AssetDatabase.LoadAssetAtPath<TextAsset>(Config).text);
  var scene=EditorSceneManager.OpenScene(Scene);var island=scene.GetRootGameObjects().Single(r=>r.GetComponent<SkySailIslandScene>());
  Apply(island,layout,"scene");Apply(island,layout,"scene repeat");EditorSceneManager.SaveScene(scene);
  var prefab=PrefabUtility.LoadPrefabContents(Prefab);try{Apply(prefab,layout,"prefab");PrefabUtility.SaveAsPrefabAsset(prefab,Prefab);}finally{PrefabUtility.UnloadPrefabContents(prefab);}
  AssetDatabase.SaveAssets();GolfCircuitReview.CaptureSavedViews();Debug.Log("GOLF_WATER_V5_ONLY_OK");
 }
 public static void BuildCandidate(){GolfCircuitReview.CaptureSavedViews();ProjectBuilder.BuildWindowsPlayer("../Builds/GolfWaterV5QA/Player/WhatTheFish.exe");}
}
