using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public static class LegacyAssetAudit {
 public static void Prepare(){
  const string variants="Assets/_Game/Art/Basketball/Rally/LogoVariants/";
  var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Resources/BasketballEnvironment.prefab");
  if(!source)throw new Exception("Current basketball prefab is required.");
  for(int i=1;i<4;i++){
   var original=source.GetComponentsInChildren<MeshFilter>(true).Single(f=>f.name=="Logo"+i);
   if(!AssetDatabase.GetAssetPath(original.sharedMesh).StartsWith(variants))throw new Exception("Logo mesh still depends on placeholder");
   var go=new GameObject("Logo"+i);go.layer=8;go.AddComponent<MeshFilter>().sharedMesh=original.sharedMesh;
   var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterials=original.GetComponent<Renderer>().sharedMaterials;
   renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
   PrefabUtility.SaveAsPrefabAsset(go,variants+"Logo"+i+".prefab");UnityEngine.Object.DestroyImmediate(go);
  }
  AssetDatabase.SaveAssets();Audit();
 }
 public static void Audit(){
  var roots=AssetDatabase.FindAssets("t:Scene",new[]{"Assets/_Game"}).Select(AssetDatabase.GUIDToAssetPath)
   .Concat(AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/_Game/Resources","Assets/_Game/Prefabs","Assets/_Game/Art/Basketball/Rally/LogoVariants"}).Select(AssetDatabase.GUIDToAssetPath)).ToArray();
  var used=new HashSet<string>(AssetDatabase.GetDependencies(roots,true));
  var candidates=new[]{"Assets/_Game/Art/Stadium.fbx","Assets/_Game/Art/BasketballArena.fbx"}
   .Concat(AssetDatabase.FindAssets("t:Material",new[]{"Assets/_Game/Materials"}).Select(AssetDatabase.GUIDToAssetPath)).ToArray();
  File.WriteAllLines("../Builds/legacy-asset-candidates.txt",candidates.Select(p=>(used.Contains(p)?"KEEP ":"UNUSED ")+p));
  foreach(var p in candidates.Where(p=>p.EndsWith(".fbx")))if(used.Contains(p))throw new Exception("Legacy FBX still referenced: "+p);
  Debug.Log("LEGACY_DEPENDENCY_AUDIT_COMPLETE");
 }
}
