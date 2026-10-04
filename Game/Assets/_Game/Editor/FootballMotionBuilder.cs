using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using WhatTheFish;

public sealed class FootballMotionBuilder:IPreprocessBuildWithReport {
 public int callbackOrder=>-100;
 public void OnPreprocessBuild(BuildReport report){Prepare();}
 // Explicit authoring command only: ordinary Prepare/build preserves edits.
 [MenuItem("WHATTHE FISH?/Football/Reset motion library to generator poses")]
 public static void ResetLibraryToDefaults(){const string path="Assets/_Game/Resources/FootballMotionLibrary.asset";var library=AssetDatabase.LoadAssetAtPath<FootballMotionLibrary>(path);if(!library){library=ScriptableObject.CreateInstance<FootballMotionLibrary>();AssetDatabase.CreateAsset(library,path);}library.takes=FootballMotionLibrary.Defaults();EditorUtility.SetDirty(library);AssetDatabase.SaveAssets();Prepare();}
 public static void BuildInitialReview(){ResetLibraryToDefaults();SkySailBuilder.RebuildPlayer();}
 [MenuItem("WHATTHE FISH?/Football/Prepare complete motion library")]
 public static void Prepare(){
  const string path="Assets/_Game/Resources/FootballMotionLibrary.asset";
  var library=AssetDatabase.LoadAssetAtPath<FootballMotionLibrary>(path);
  if(!library){library=ScriptableObject.CreateInstance<FootballMotionLibrary>();library.takes=FootballMotionLibrary.Defaults();AssetDatabase.CreateAsset(library,path);AssetDatabase.SaveAssets();}
  var names=new System.Collections.Generic.HashSet<string>();
  foreach(var take in library.takes){if(!names.Add(take.name)||take.duration<=0||take.keys.Length<2)throw new Exception("Invalid football take "+take.name);float previous=-1;foreach(var k in take.keys){if(k.time<previous||k.time<0||k.time>1)throw new Exception("Invalid timeline "+take.name);previous=k.time;if(!float.IsFinite(k.pose.pelvis.sqrMagnitude+k.pose.body.sqrMagnitude+k.pose.leftFoot.sqrMagnitude+k.pose.rightFoot.sqrMagnitude+k.pose.leftHand.sqrMagnitude+k.pose.rightHand.sqrMagnitude))throw new Exception("Non-finite pose "+take.name);}}
  foreach(var required in FootballMotionLibrary.Defaults())if(!names.Contains(required.name))throw new Exception("Missing football take "+required.name);
  Debug.Log("FOOTBALL_MOTION_LIBRARY_OK takes="+names.Count+"; saved artist edits preserved");
 }
}
