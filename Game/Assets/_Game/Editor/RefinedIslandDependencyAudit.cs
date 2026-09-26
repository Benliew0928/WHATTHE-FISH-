using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public static class RefinedIslandDependencyAudit {
 public static void Audit(){
  var all=AssetDatabase.GetAllAssetPaths().Where(p=>p.StartsWith("Assets/")&&!AssetDatabase.IsValidFolder(p)).ToArray();
  Func<string,bool> retired=p=>p.StartsWith("Assets/_Game/Art/Golf/Tidebloom/")||p=="Assets/_Game/Art/GolfIsland.fbx"||p.StartsWith("Assets/_Game/Art/Fishing/Lagoon/")||p.StartsWith("Assets/_Game/Prefabs/Fishing/Lagoon/");
  var roots=all.Where(p=>!retired(p)&&(p.EndsWith(".unity")||p.EndsWith(".prefab")||p.Contains("/Resources/"))).ToArray();
  var used=new HashSet<string>(AssetDatabase.GetDependencies(roots,true));var candidates=all.Where(retired).ToArray();
  string dir="../Builds/IslandRefinementAudit/";Directory.CreateDirectory(dir);
  File.WriteAllLines(dir+"unity-dependencies.txt",all.OrderBy(p=>p).Select(p=>(used.Contains(p)?"ACTIVE ":"UNROOTED ")+p));
  foreach(var p in candidates)if(used.Contains(p))throw new Exception("Superseded island asset is still referenced: "+p);
  File.WriteAllLines(dir+"retired-unity-assets.txt",candidates);
  foreach(var sport in new[]{"Football","Basketball","Golf","Fishing"}){
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Resources/"+sport+"Environment.prefab");if(!prefab)throw new Exception("Missing environment: "+sport);
   foreach(var t in prefab.GetComponentsInChildren<Transform>(true))if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0)throw new Exception("Missing script on "+t.name);
   foreach(var r in prefab.GetComponentsInChildren<Renderer>(true))if(r.sharedMaterials.Any(m=>!m))throw new Exception("Missing material on "+r.name);
   foreach(var f in prefab.GetComponentsInChildren<MeshFilter>(true))if(!f.sharedMesh)throw new Exception("Missing mesh on "+f.name);
  }
  File.WriteAllText(dir+"dependency-audit-result.txt","PASS all active scenes, Resources and non-retired prefabs; all four environments have meshes/materials/scripts; retired assets unreferenced.\nREFINED_DEPENDENCY_AUDIT_COMPLETE candidates="+candidates.Length+"\n");
 }
}
