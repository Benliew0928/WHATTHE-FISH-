using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using WhatTheFish;

public static class JumpAnimationBuilder {
 const string Path="Assets/_Game/Art/RainbowSprinterJump.fbx";
 [MenuItem("WHATTHE FISH?/Prepare jump animation")]
 public static void Prepare(){
  var importer=AssetImporter.GetAtPath(Path) as ModelImporter;
  if(!importer)throw new Exception("Export jump_rainbow_sprinter.py before building.");
  var sourceTakes=importer.defaultClipAnimations;
  if(sourceTakes.Length!=1)throw new Exception("Jump must contain one animation-only take.");
  if(!importer.preserveHierarchy||importer.animationCompression!=ModelImporterAnimationCompression.KeyframeReduction||!importer.clipAnimations.Any(c=>c.name=="Jump"&&c.firstFrame==sourceTakes[0].firstFrame&&c.lastFrame==sourceTakes[0].lastFrame)){
   importer.globalScale=1;importer.useFileScale=true;importer.preserveHierarchy=true;
   importer.animationType=ModelImporterAnimationType.Generic;importer.importAnimation=true;
   importer.importCameras=false;importer.importLights=false;importer.materialImportMode=ModelImporterMaterialImportMode.None;
   importer.animationCompression=ModelImporterAnimationCompression.KeyframeReduction;
   importer.animationRotationError=.3f;importer.animationPositionError=.1f;importer.animationScaleError=.1f;
   var clips=importer.defaultClipAnimations;if(clips.Length!=1)throw new Exception("Jump must contain one animation-only take.");
   clips[0].name="Jump";clips[0].loopTime=false;clips[0].loopPose=false;importer.clipAnimations=clips;importer.SaveAndReimport();
  }
  var clip=AssetDatabase.LoadAllAssetsAtPath(Path).OfType<AnimationClip>().Single(c=>c.name=="Jump");
  var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/_Game/Settings/Athlete.controller");
  if(!controller)throw new Exception("Build the athlete controller first.");
  if(!controller.parameters.Any(p=>p.name=="JumpTime"))controller.AddParameter("JumpTime",AnimatorControllerParameterType.Float);
  if(!controller.layers.Any(l=>l.name=="Jump"))controller.AddLayer("Jump");
  var layers=controller.layers;var layer=layers.Single(l=>l.name=="Jump");layer.defaultWeight=0;controller.layers=layers;
  var machine=layer.stateMachine;var state=machine.states.Select(s=>s.state).FirstOrDefault(s=>s.name=="Jump");
  if(!state)state=machine.AddState("Jump");state.motion=clip;state.timeParameter="JumpTime";state.timeParameterActive=true;machine.defaultState=state;
  EditorUtility.SetDirty(controller);AssetDatabase.SaveAssets();Audit();
 }
 public static void Audit(){
  foreach(int fps in new[]{20,30,60,120}){
   var jump=new JumpMotor();jump.Reset();jump.Request();float y=0,maximum=0;bool landed=false,tookOff=false;
   for(int i=0;i<fps*2;i++){
    y+=jump.Step(!jump.Airborne,true,1f/fps);maximum=Mathf.Max(maximum,y);
    tookOff|=jump.Airborne&&y>0;
    if(y<=0){y=0;jump.Collide(CollisionFlags.Below);landed|=tookOff;}
   }
   if(!landed||Mathf.Abs(maximum-JumpMotor.Height)>.01f)throw new Exception("Jump height or landing varies at "+fps+" FPS: "+maximum);
  }
  var motor=new JumpMotor();motor.Reset();motor.Request();motor.Step(true,true,.01f);
  if(!motor.Preparing||motor.Airborne||motor.PoseTime<=0)throw new Exception("Grounded jump did not visibly load immediately.");
  for(int i=0;i<8;i++)motor.Step(true,true,.01f);uint first=motor.Sequence;
  motor.Request();for(int i=0;i<20;i++)motor.Step(false,true,.01f);
  if(first!=motor.Sequence)throw new Exception("Airborne jump request restarted flight.");
  motor.Collide(CollisionFlags.Above);if(motor.Velocity>0)throw new Exception("Ceiling did not cancel ascent.");
  motor.Collide(CollisionFlags.Below);motor.Step(true,true,.01f);if(motor.Airborne)throw new Exception("Expired press repeated after landing.");
  motor.Reset();motor.Step(true,true,.01f);motor.Step(false,true,.04f);motor.Request();motor.Step(false,true,.01f);if(motor.Velocity<7)throw new Exception("Coyote jump failed.");
  motor.Reset();motor.Step(false,true,.2f);motor.Request();motor.Step(false,true,.01f);motor.Collide(CollisionFlags.Below);motor.Step(true,true,.01f);if(motor.Velocity<7)throw new Exception("Buffered landing jump failed.");
  motor.Reset();motor.Request();motor.Step(true,false,.01f);motor.Step(true,true,.01f);if(motor.Airborne||motor.Preparing)throw new Exception("Blocked input leaked into gameplay.");
  motor.Reset();motor.Request();motor.Step(true,true,.01f);motor.Step(true,false,.01f);if(motor.Preparing||motor.Airborne)throw new Exception("Tackle did not cancel the grounded load.");
  foreach(string name in new[]{"OfflineAthlete","NetworkAthlete"}){
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Characters/"+name+".prefab");
   if(!prefab||prefab.GetComponentInChildren<Animator>().applyRootMotion)throw new Exception("Jump requires the existing in-place athlete prefabs.");
  }
  Directory.CreateDirectory("../Builds/JumpQA");File.WriteAllText("../Builds/JumpQA/editor-audit.txt","PASS 20/30/60/120 FPS height; landing; no double jump; ceiling; coyote time; landing buffer; blocked input; root motion disabled.");
 }
}
