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
  MomentumChecks();
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
 static void MomentumChecks(){
  var lines=new System.Collections.Generic.List<string>();
  foreach(int fps in new[]{20,30,60,120})foreach(float heading in Enumerable.Range(0,16).Select(i=>i*22.5f)){
   float dt=1f/fps;var direction=Quaternion.Euler(0,heading,0)*Vector3.forward;
   var motor=new LocomotionMotor();motor.Reset(heading);motor.Step(direction,7,true,.25f);
   var travel=Vector3.zero;
   for(int i=0;i<fps*2/5;i++)travel+=motor.Step(Vector3.zero,4,false,dt);
   if(travel.magnitude<2.1f||travel.magnitude>2.3f||motor.Velocity.magnitude<4||Vector3.Cross(travel,direction).magnitude>.001f)throw new Exception("Air release lost momentum or changed heading "+fps+" / "+heading);
   for(int i=0;i<Mathf.CeilToInt(fps*.3f);i++){
    var before=motor.Velocity;motor.Step(-direction,7,false,dt);
    if((motor.Velocity-before).magnitude>LocomotionMotor.AirAcceleration*dt+.001f)throw new Exception("Air steering snapped velocity");
    if(motor.Velocity.magnitude>7.001f)throw new Exception("Air steering gained excess speed");
   }
   if(Vector3.Dot(motor.Velocity,-direction)<6.9f)throw new Exception("Air reversal too slow");
   for(int i=0;i<Mathf.CeilToInt(fps*.3f);i++)motor.Step(Vector3.zero,4,true,dt);
   if(motor.Velocity.sqrMagnitude>.00001f||motor.Phase!=LocomotionPhase.Idle)throw new Exception("Landing retained unwanted sliding");
   motor.Step(direction,7,true,.25f);motor.Step(direction*.35f,4,false,.3f);
   if(Mathf.Abs(motor.Velocity.magnitude-1.4f)>.001f)throw new Exception("Analog air input did not settle to requested speed");
   motor.Reset(heading);if(motor.Step(Vector3.zero,4,false,.2f)!=Vector3.zero)throw new Exception("Reset retained air momentum");
   lines.Add($"PASS momentum fps={fps} heading={heading} coast400ms={travel.magnitude:F4}m; bounded reversal; landing stop; analog; reset");
  }
  var wall=new LocomotionMotor();wall.Reset(45);wall.Step(new Vector3(1,0,1).normalized,7,true,.25f);wall.Constrain(Vector3.back);
  if(Mathf.Abs(wall.Velocity.z)>.001f||wall.Velocity.x<4.9f)throw new Exception("Wall did not preserve only tangential momentum");
  var regular=new LocomotionMotor();var hitch=new LocomotionMotor();regular.Reset(0);hitch.Reset(0);regular.Step(Vector3.forward,7,true,.25f);hitch.Step(Vector3.forward,7,true,.25f);
  var regularTravel=Vector3.zero;for(int i=0;i<24;i++)regularTravel+=regular.Step(Vector3.zero,4,false,1f/60);
  var hitchTravel=hitch.Step(Vector3.zero,4,false,.013f)+hitch.Step(Vector3.zero,4,false,.2f)+hitch.Step(Vector3.zero,4,false,.187f);
  if(Vector3.Distance(regularTravel,hitchTravel)>.001f)throw new Exception("Hitch changed coasting distance");
  lines.Add("PASS wall tangent preservation and irregular-frame coast distance");
  Directory.CreateDirectory("../Builds/MomentumQA");File.WriteAllLines("../Builds/MomentumQA/editor-audit.txt",lines);
 }
}
