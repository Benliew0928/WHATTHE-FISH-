#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using WhatTheFish;

public static class GolfSwingReview {
 public static void BuildVerification(){MeasureReach();ProjectBuilder.BuildWindowsPlayer(Environment.GetEnvironmentVariable("WTF_GOLF_SWING_PLAYER")??"../Builds/GolfSwingPlayer/WhatTheFish.exe");}
 public static void MeasureReach(){
  var root=Path.GetFullPath(Path.Combine(Application.dataPath,"../.."));
  var folder=Path.Combine(root,"Builds/GolfSwingQA");Directory.CreateDirectory(folder);
  var actor=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<Athlete>("Assets/_Game/Prefabs/Characters/NetworkAthlete.prefab"));
  try {
   actor.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);actor.Setup();actor.GolfClubMotion.Equip(1);
   var club=actor.GolfClubMotion;var field=typeof(GolfClubMotion).GetField("left",BindingFlags.Instance|BindingFlags.NonPublic);var limb=field.GetValue(club);var type=limb.GetType();
   var shoulder=(Vector3)type.GetField("shoulderOffset").GetValue(limb);var arm=(float)type.GetField("reach").GetValue(limb);
   var report=Path.Combine(folder,"reach-matrix.txt");
   File.WriteAllText(report,$"rig={club.RigReady} shoulder={shoulder:F4} arm={arm:F4} club={club.Club.transform.InverseTransformPoint(club.Club.head.position).magnitude:F4}\n");
   foreach(float distance in new[]{.05f,.25f,.5f,.75f,1f,1.25f,1.5f,1.75f,2f}){
    int count=0;
    for(int angle=0;angle<360;angle+=30){var target=Quaternion.Euler(0,angle,0)*Vector3.forward*distance+Vector3.up*(GolfBall.Radius+.002f);bool ready=club.CanAddress(target);if(ready)count++;File.AppendAllText(report,$"distance={distance:F2} angle={angle} ready={ready}\n");}
    Debug.Log($"SWING_REACH distance={distance:F2} allowedAngles={count}/12");
   }
   Debug.Log("SWING_REACH_MATRIX_COMPLETE");
  }finally{UnityEngine.Object.DestroyImmediate(actor.gameObject);}
 }
}
#endif
