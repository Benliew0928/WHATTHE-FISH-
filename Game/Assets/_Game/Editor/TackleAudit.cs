using System;
using System.IO;
using System.Linq;
using WhatTheFish;
using UnityEditor;
using UnityEngine;

public static class TackleAudit {
 public static void Run(){
  foreach(int fps in new[]{30,60,120})foreach(var action in new[]{FootballAction.Slide,FootballAction.Hit}){
   float distance=0;for(int i=0;i<fps;i++)distance+=FootballTackle.TravelAt(action,(i+1f)/fps)-FootballTackle.TravelAt(action,i/(float)fps);
   float expected=action==FootballAction.Slide?4.62f:.72f;
   if(Mathf.Abs(distance-expected)>.001f)throw new Exception("Frame-rate-dependent tackle travel");
  }
  foreach(var name in new[]{"Slide_Tackle","Tackle_Hit"}){
   var path="Assets/_Game/Art/RainbowSprinter"+name+".fbx";
   var clips=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
   if(clips.Length!=1||clips[0].name!=name||clips[0].isLooping)throw new Exception("Incorrect football animation import");
   float duration=name=="Slide_Tackle"?FootballTackle.SlideDuration:FootballTackle.HitDuration;
   float playback=name=="Slide_Tackle"?FootballTackle.SlidePlayback:1;
   if(Mathf.Abs(clips[0].length/playback-duration)>.002f)throw new Exception("Clip playback and gameplay duration differ");
  }
  foreach(var name in new[]{"OfflineAthlete","NetworkAthlete"}){
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Resources/"+name+".prefab");
   if(!prefab.GetComponent<FootballTackle>())throw new Exception("Missing football simulation component");
   var animator=prefab.GetComponentInChildren<Animator>();if(animator.applyRootMotion||!(animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController).layers.Any(l=>l.name=="Football action"))throw new Exception("Football Animator is not configured");
  }
  Directory.CreateDirectory("../Builds/TackleQA");File.WriteAllText("../Builds/TackleQA/editor-audit.txt","PASS slide/reaction distances at 30/60/120 fps; two non-looping imported clips with matching durations; both prefabs; root motion off.");
  Debug.Log("TACKLE_EDITOR_AUDIT_COMPLETE");
 }
}
