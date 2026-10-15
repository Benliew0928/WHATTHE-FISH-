#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
public static class FootballTrailBuild {
 public static void BuildReview(){
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(SoccerBallBuilder.PrefabPath);
  if(Mathf.Abs(prefab.GetComponent<SphereCollider>().radius-.264f)>.00001f)throw new Exception("Football radius must be .264 metres.");
  var instance=UnityEngine.Object.Instantiate(prefab);
  try {var renderers=instance.GetComponentsInChildren<MeshRenderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);if(Mathf.Abs(bounds.size.x-.528f)>.001f||Mathf.Abs(bounds.size.y-.528f)>.001f||Mathf.Abs(bounds.size.z-.528f)>.001f)throw new Exception("Football visual diameter must be .528 metres.");}
  finally {UnityEngine.Object.DestroyImmediate(instance);}
  ProjectBuilder.BuildWindowsPlayer("../Builds/FootballTrailQA/Player/WhatTheFish.exe");
 }
}
#endif
