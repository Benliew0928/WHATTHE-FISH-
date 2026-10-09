using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Keep the authored FBXs. Remove redundant sampled keys in their delivery
// imports, validating every bone at twice the authored 60 Hz sample rate.
public static class AnimationDeliveryOptimizer {
 static readonly string[] Names={"Idle","Turn_Left_180","Turn_Left_90","Turn_Right_90","Turn_Right_180","Slide_Tackle","Tackle_Hit"};
 public static void Configure(ModelImporter importer){
  importer.animationCompression=ModelImporterAnimationCompression.KeyframeReduction;
  importer.animationRotationError=.001f;importer.animationPositionError=.001f;importer.animationScaleError=.001f;
 }
 sealed class Pose {public Vector3[] positions,scales;public Quaternion[] rotations;}
 static Pose Sample(GameObject model,Transform[] bones,AnimationClip clip,float time){
  clip.SampleAnimation(model,time);return new Pose{positions=bones.Select(b=>b.position).ToArray(),rotations=bones.Select(b=>b.rotation).ToArray(),scales=bones.Select(b=>b.lossyScale).ToArray()};
 }
 public static void Prepare(){
  const string output="../Builds/SimpleHudQA/";Directory.CreateDirectory(output);
  var lines=new List<string>{"clip,samples,bones,keys_before,keys_after,max_position_metres,max_rotation_degrees,max_scale_error"};
  foreach(var name in Names){
   string path="Assets/_Game/Art/RainbowSprinter"+name+".fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(path);
   // A repeatable comparison against the unchanged full sampled take.
   importer.animationCompression=ModelImporterAnimationCompression.Off;importer.SaveAndReimport();
   var before=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Single(c=>!c.name.StartsWith("__preview__"));
   var model=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/RainbowSprinter.fbx"));
   bool valid=false;
   try{
    var bones=model.GetComponentsInChildren<Transform>(true);float duration=before.length;bool looping=before.isLooping;int steps=Mathf.CeilToInt(duration*120);
    var samples=Enumerable.Range(0,steps+1).Select(i=>Sample(model,bones,before,duration*i/steps)).ToArray();
    int oldKeys=AnimationUtility.GetCurveBindings(before).Sum(b=>AnimationUtility.GetEditorCurve(before,b).length);
    Configure(importer);importer.SaveAndReimport();
    var after=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Single(c=>!c.name.StartsWith("__preview__"));
    if(Mathf.Abs(duration-after.length)>.00001f||after.isLooping!=looping)throw new Exception("Animation timing changed: "+name);
    float position=0,angle=0,scale=0;
    for(int i=0;i<=steps;i++){
     var pose=Sample(model,bones,after,duration*i/steps);var reference=samples[i];
     for(int j=0;j<bones.Length;j++){position=Mathf.Max(position,Vector3.Distance(pose.positions[j],reference.positions[j]));angle=Mathf.Max(angle,Quaternion.Angle(pose.rotations[j],reference.rotations[j]));scale=Mathf.Max(scale,Vector3.Distance(pose.scales[j],reference.scales[j]));}
    }
    int newKeys=AnimationUtility.GetCurveBindings(after).Sum(b=>AnimationUtility.GetEditorCurve(after,b).length);
    if(position>.001f||angle>.1f||scale>.001f||newKeys>oldKeys)throw new Exception($"Animation exceeds delivery tolerance {name}: position={position} rotation={angle} scale={scale}");
    valid=true;lines.Add(FormattableString.Invariant($"{name},{steps+1},{bones.Length},{oldKeys},{newKeys},{position},{angle},{scale}"));
   }finally{UnityEngine.Object.DestroyImmediate(model);if(!valid){importer.animationCompression=ModelImporterAnimationCompression.Off;importer.SaveAndReimport();}}
  }
  File.WriteAllLines(output+"animation-key-validation.csv",lines);Debug.Log("ANIMATION_DELIVERY_VALIDATED "+string.Join("; ",lines));
 }
}
