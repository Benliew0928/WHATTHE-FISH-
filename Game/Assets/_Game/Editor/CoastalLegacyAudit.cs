using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// A read-only dependency inventory. Archiving is performed separately, after EXE QA.
public static class CoastalLegacyAudit {
 public static void Audit(){
  var all=AssetDatabase.GetAllAssetPaths().Where(p=>p.StartsWith("Assets/")&&!AssetDatabase.IsValidFolder(p)).ToArray();
  var retired=new HashSet<string>(new[]{"Ceiling","LightingRig","Scoreboard"}.Select(n=>"Assets/_Game/Prefabs/Basketball/Rally/"+n+".prefab"));
  var roots=all.Where(p=>p.EndsWith(".unity")||(p.Contains("/Resources/")&&!p.EndsWith(".cs"))||(p.EndsWith(".prefab")&&!retired.Contains(p))).ToArray();
  var used=new HashSet<string>(AssetDatabase.GetDependencies(roots,true));
  foreach(var p in roots)used.Add(p);
  var output=new List<string>{"Dependency roots include all scenes, active prefabs and every Resources asset. Unused does not by itself authorize archival of authoring inputs."};
  foreach(var p in all.OrderBy(p=>p))output.Add((used.Contains(p)?"ACTIVE ":"UNROOTED ")+p);
  Directory.CreateDirectory("../Builds/CoastalStadiumAudit");File.WriteAllLines("../Builds/CoastalStadiumAudit/unity-dependencies.txt",output);
  var candidates=retired.Concat(new[]{"Ceiling","LightingRig","Scoreboard"}.Select(n=>"Assets/_Game/Art/Basketball/Rally/"+n+".fbx"))
   .Concat(all.Where(p=>p.StartsWith("Assets/_Game/Art/Basketball/Rally/Materials/Rally_Ceiling_")||p.StartsWith("Assets/_Game/Art/Basketball/Rally/Materials/Rally_LightingRig_")||p.StartsWith("Assets/_Game/Art/Basketball/Rally/Materials/Rally_Scoreboard_"))).Where(File.Exists).Distinct().ToArray();
  foreach(var p in candidates)if(used.Contains(p))throw new Exception("Retired arena asset is still referenced: "+p);
  File.WriteAllLines("../Builds/CoastalStadiumAudit/retired-unity-assets.txt",candidates);
  var invalid=all.Where(p=>p.EndsWith(".prefab")||p.EndsWith(".unity")||p.EndsWith(".mat")).SelectMany(p=>AssetDatabase.GetDependencies(p,true).Select(d=>new{p,d})).Where(x=>x.d.Contains("/Legacy/"));
  if(invalid.Any())throw new Exception("Active Unity data references Legacy");
  foreach(var sport in new[]{"Football","Basketball"}){
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(MobilePackageCleanup.EnvironmentFolder+sport+"Environment.prefab");
   if(!prefab)throw new Exception("Missing environment "+sport);
   foreach(var transform in prefab.GetComponentsInChildren<Transform>(true))
    if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject)>0)throw new Exception("Missing script on "+transform.name);
   foreach(var renderer in prefab.GetComponentsInChildren<Renderer>(true))
    if(renderer.sharedMaterials.Any(m=>!m))throw new Exception("Missing material on "+renderer.name);
  }
  Debug.Log("COASTAL_LEGACY_AUDIT_COMPLETE assets="+all.Length+" roots="+roots.Length+" retired="+candidates.Length);
 }
}
