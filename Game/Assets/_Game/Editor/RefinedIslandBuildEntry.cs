using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WhatTheFish;
public static class RefinedIslandBuildEntry {
 // Iteration entry point deliberately reuses the approved stadium scene objects.
 public static void BuildWindows(){
  EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");
  var env=Object.FindFirstObjectByType<SportEnvironmentController>();
  foreach(var sport in new[]{SportId.Golf,SportId.Fishing}){
   int index=System.Array.FindIndex(env.definitions,d=>d.id==sport);Object.DestroyImmediate(env.roots[index]);
   var root=RefinedIslandBuilder.Build(sport.ToString());env.roots[index]=root;var layout=root.GetComponent<RefinedIslandEnvironment>().layout;var def=env.definitions[index];
   def.spawnPositions=layout.spawns;def.maxPlayers=layout.capacity;def.menuCamera=layout.views[0].position;def.menuFocus=layout.views[0].target;
   def.environmentPrefab=PrefabUtility.SaveAsPrefabAsset(root,MobilePackageCleanup.EnvironmentFolder+sport+"Environment.prefab");EditorUtility.SetDirty(def);
  }
  env.Activate(SportId.Football);EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
  ProjectBuilder.BuildCurrentWindows();
  RefinedIslandDependencyAudit.Audit();
 }
}
