using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WhatTheFish;

public static class GolfDecorativeFlagReview {
 public static void RemoveAndBuild(){
  string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/GolfFlagRemovalQA"));Directory.CreateDirectory(folder);
  var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_Golf.unity");
  var island=scene.GetRootGameObjects().Single(g=>g.GetComponent<SkySailIslandScene>());
  var course=island.GetComponentInChildren<GolfCourse>(true);var flags=course.holes.Select(h=>h.flag).ToArray();
  int removed=GolfCourseBuilder.RemoveDecorativeFlags(island);
  if(flags.Length!=5||!flags.SequenceEqual(course.holes.Select(h=>h.flag)))throw new Exception("Retained hole flags incorrect");
  EditorSceneManager.SaveScene(scene);
  const string path="Assets/_Game/Prefabs/Environments/GolfEnvironment.prefab";
  var contents=PrefabUtility.LoadPrefabContents(path);int prefabRemoved;
  try{prefabRemoved=GolfCourseBuilder.RemoveDecorativeFlags(contents);if(contents.GetComponentInChildren<GolfCourse>(true).holes.Count(h=>h.flag)!=5)throw new Exception("Prefab hole flags missing");PrefabUtility.SaveAsPrefabAsset(contents,path);}
  finally{PrefabUtility.UnloadPrefabContents(contents);}
  File.WriteAllText(Path.Combine(folder,"removal.txt"),$"sceneRemovedTriangles={removed} prefabRemovedTriangles={prefabRemoved} numberedHoleFlagsRetained=5\n");
  GolfCourseReview.CaptureSavedMap();
  ProjectBuilder.BuildWindowsPlayer(Environment.GetEnvironmentVariable("WTF_GOLF_FLAG_PLAYER")??"../Builds/GolfFlagRemovalQA/Player/WhatTheFish.exe");
 }
 public static void Inspect(){
  var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_Golf.unity");
  var island=scene.GetRootGameObjects().Single(g=>g.GetComponent<SkySailIslandScene>());
  var lines=island.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name.Contains("CourseProps")).Select(f=>{
   var points=f.sharedMesh.vertices.Select(p=>island.transform.InverseTransformPoint(f.transform.TransformPoint(p))).ToArray();
   var bounds=new Bounds(points[0],Vector3.zero);foreach(var p in points)bounds.Encapsulate(p);
   return $"{f.name} asset={AssetDatabase.GetAssetPath(f.sharedMesh)} bounds={bounds} vertices={points.Length}";
  }).ToArray();
  string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/GolfFlagRemovalQA"));Directory.CreateDirectory(folder);File.WriteAllLines(Path.Combine(folder,"inventory.txt"),lines);
  Debug.Log("GOLF_DECORATIVE_FLAG_INVENTORY "+string.Join("; ",lines));
 }
}
