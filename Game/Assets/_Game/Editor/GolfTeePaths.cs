using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WhatTheFish;
using Object=UnityEngine.Object;
public static partial class GolfMultiplayerTeeBuild {
 public static void FinishPaths(GameObject island){
  var course=island.GetComponentInChildren<GolfCourse>(true);if(!course||course.teeSites.Length==0)return;
  ConfigureTeeTurf();
  foreach(var site in course.teeSites.Where(s=>s.hole!=1)){var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"terrace-"+site.hole+".asset");if(mesh){mesh.colors=mesh.vertices.Select(v=>new Color(1,1,1,site.GroundWeight(v.x,v.z))).ToArray();EditorUtility.SetDirty(mesh);}}
  var root=course.transform.Find("Multiplayer tee grounds");
  var first=course.teeSites.First(t=>t.hole==1);var supports=island.GetComponentsInChildren<MeshCollider>(true).Where(c=>c.name.StartsWith("Terrain__")&&!c.transform.IsChildOf(root)).ToArray();int moved=0;
  foreach(Transform item in island.transform){string assetPath=PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(item.gameObject).ToLowerInvariant();if(!(assetPath.Contains("/refinedislands/")&&(assetPath.Contains("palm")||assetPath.Contains("rock")))&&!item.name.StartsWith("Palm")&&!item.name.StartsWith("Rock"))continue;var at=island.transform.InverseTransformPoint(item.position);bool rear=first.Distance(at.x,at.z)>71&&first.Distance(at.x,at.z)<80&&Mathf.Abs(first.AngleDelta(at.x,at.z))<.38f;var site=course.teeSites.FirstOrDefault(t=>t.Clears(at.x,at.z)||t.Opening(at.x,at.z));if(site.count==0&&!rear)continue;if(rear)site=first;float a=site.Angle+(++moved%2==0?1:-1)*(site.HalfWidth+12/site.radius);float r=Mathf.Min(site.radius,site.Distance(at.x,at.z));var target=site.cup+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*r;if(course.SafeDryPosition(island.transform.TransformPoint(target),3))item.position=island.transform.TransformPoint(Floor(island,supports,target));}
  PolishT1(island,course,root,supports);
  RestoreOriginalEntryPath(island);
  Physics.SyncTransforms();AssetDatabase.SaveAssets();
 }
}
