using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class IdleImportAudit {
 [Serializable] public class BonePose { public string name; public Vector3 position; }
 [Serializable] public class PoseSample { public float time; public BonePose[] bones; }
 [Serializable] public class Reference { public PoseSample[] samples; }
 static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
 public static void Run(){
  const string root="Assets/_Game/Art/";
  var model=AssetDatabase.LoadAssetAtPath<GameObject>(root+"RainbowSprinter.fbx");
  var run=AssetDatabase.LoadAllAssetsAtPath(root+"RainbowSprinterRun.fbx").OfType<AnimationClip>().Single(c=>c.name=="Running");
  var idleClips=AssetDatabase.LoadAllAssetsAtPath(root+"RainbowSprinterIdle.fbx").OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
  Require(idleClips.Length==1,"Idle FBX must contain exactly one clip");
  var idle=idleClips[0];Require(idle.name=="Idle_Playful"&&Mathf.Abs(idle.length-8)<.02f&&idle.isLooping,"Idle name, duration or loop flag is incorrect");
  var instance=UnityEngine.Object.Instantiate(model);
  try {
   var lines=new List<string>();
   var bones=instance.GetComponentsInChildren<Transform>(true).ToDictionary(t=>AnimationUtility.CalculateTransformPath(t,instance.transform));
   foreach(var binding in AnimationUtility.GetCurveBindings(idle))Require(bones.ContainsKey(binding.path),"Unbound idle curve: "+binding.path);
   var rig=bones.Values.Single(t=>t.name=="RainbowSprinterRig");var originalScale=rig.localScale;var originalPosition=rig.localPosition;
   var named=bones.Values.GroupBy(t=>t.name).ToDictionary(g=>g.Key,g=>g.First());
   var renderers=instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
   var reference=JsonUtility.FromJson<Reference>(File.ReadAllText("../Builds/IdleQA/blender-reference.json"));
   float maximumError=0,minimumY=float.PositiveInfinity;
   foreach(var sample in reference.samples){
    idle.SampleAnimation(instance,sample.time);
    Require(Vector3.Distance(rig.localScale,originalScale)<.001f&&Vector3.Distance(rig.localPosition,originalPosition)<.0001f,"Idle changes the rig root transform");
    foreach(var bone in sample.bones){
     Require(named.ContainsKey(bone.name),"Missing bone "+bone.name);
     maximumError=Mathf.Max(maximumError,Vector3.Distance(named[bone.name].position,bone.position));
     if(sample.time==0)lines.Add($"REFERENCE {bone.name} unity={named[bone.name].position:F6} blender={bone.position:F6}");
    }
    foreach(var renderer in renderers){
     // Compensate FBX's 100x renderer transform before TransformPoint applies it.
     var mesh=new Mesh();renderer.BakeMesh(mesh,true);
     float localMinimum=float.PositiveInfinity;
     foreach(var vertex in mesh.vertices)localMinimum=Mathf.Min(localMinimum,renderer.transform.TransformPoint(vertex).y);
     minimumY=Mathf.Min(minimumY,localMinimum);
     lines.Add($"MESH time={sample.time} name={renderer.name} minimumY={localMinimum} localScale={renderer.transform.localScale} worldScale={renderer.transform.lossyScale} bounds={mesh.bounds}");
     Require(mesh.bounds.size.magnitude<4,"Idle mesh bounds exploded");UnityEngine.Object.DestroyImmediate(mesh);
    }
   }
   File.WriteAllLines("../Builds/IdleQA/unity-positions.txt",lines);
   Require(maximumError<.003f,"Unity differs from Blender by "+maximumError+" m");
   Require(minimumY>-.005f,"Idle shoe penetrates ground: "+minimumY);
   idle.SampleAnimation(instance,0);var initial=bones.Values.Select(t=>(t,t.localPosition,t.localRotation)).ToArray();
   idle.SampleAnimation(instance,idle.length);
   foreach(var pose in initial)Require(Vector3.Distance(pose.t.localPosition,pose.localPosition)<.0005f&&Quaternion.Angle(pose.t.localRotation,pose.localRotation)<.1f,"Idle endpoint discontinuity at "+pose.t.name);
   idle.SampleAnimation(instance,0);var hip=named["mixamorig:Hips"].position;
   idle.SampleAnimation(instance,2);Require(Vector3.Distance(hip,named["mixamorig:Hips"].position)>.005f,"Idle clip has no body motion");
   foreach(var time in new[]{.02f,.2f,.5f}){
    run.SampleAnimation(instance,time);
    Require(Vector3.Distance(rig.localScale,originalScale)<.001f,"Run scale changed");
    foreach(var renderer in renderers)Require(renderer.bounds.size.magnitude<4,"Run bounds exploded");
   }
   lines.Add($"PASS imported idle: duration={idle.length:F3}s bindings={AnimationUtility.GetCurveBindings(idle).Length} BlenderMaximumError={maximumError:F7}m lowestVertex={minimumY:F7}m");
   foreach(var prefabName in new[]{"OfflineAthlete","NetworkAthlete"}){
    var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Resources/"+prefabName+".prefab");
    var animator=prefab.GetComponentInChildren<Animator>();Require(!animator.applyRootMotion,"Root motion enabled");
    var controller=(AnimatorController)animator.runtimeAnimatorController;
    var states=controller.layers[0].stateMachine;var idleState=states.states.Single(s=>s.state.name=="Idle").state;var runState=states.states.Single(s=>s.state.name=="Run").state;
    Require(idleState.motion==idle&&runState.motion==run&&states.defaultState==idleState,"Prefab motion assignment mismatch");
    foreach(var state in new[]{idleState,runState}){
     var transition=state.transitions.Single(t=>t.destinationState==(state==idleState?runState:idleState));
     Require(!transition.hasExitTime&&transition.hasFixedDuration&&transition.interruptionSource==TransitionInterruptionSource.Destination,"Incorrect transition responsiveness");
     Require(Mathf.Abs(transition.duration-(state==idleState?.15f:.25f))<.001f,"Incorrect blend duration");
    }
    lines.Add("PASS "+prefabName+" controller, clips and root motion");
   }
   Directory.CreateDirectory("../Builds/IdleQA");File.WriteAllLines("../Builds/IdleQA/unity-import-audit.txt",lines);
   Debug.Log("IDLE_IMPORT_AUDIT_COMPLETE "+string.Join("; ",lines));
  } finally {UnityEngine.Object.DestroyImmediate(instance);}
 }
}
