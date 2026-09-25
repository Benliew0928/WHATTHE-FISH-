using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class RainbowImportAudit {
 public static void Run(){
  const string root="Assets/_Game/Art/";
  var model=AssetDatabase.LoadAssetAtPath<GameObject>(root+"RainbowSprinter.fbx");
  var run=AssetDatabase.LoadAllAssetsAtPath(root+"RainbowSprinterRun.fbx").OfType<AnimationClip>().Single(c=>c.name=="Running");
  var instance=UnityEngine.Object.Instantiate(model);
  var lines=new System.Collections.Generic.List<string>();
  var skinned=instance.GetComponentsInChildren<SkinnedMeshRenderer>(true).First(r=>r.name=="RainbowSprinter_LOD0");
  var rig=instance.GetComponentsInChildren<Transform>(true).First(t=>t.name=="RainbowSprinterRig");
  foreach(var time in new[]{-1f,.02f,.2f,.5f}){
   if(time>=0)run.SampleAnimation(instance,time);
   lines.Add($"time={time} rigScale={rig.localScale} rigPosition={rig.localPosition} bounds={skinned.bounds.center}/{skinned.bounds.size} hip={skinned.bones.First(b=>b.name.Contains("Hips")).position}");
   if(skinned.bounds.size.magnitude>4||skinned.bounds.center.magnitude>4)throw new System.Exception("The running take does not match the model's skeleton scale at "+time);
  }
  File.WriteAllLines("../Builds/rainbow-import-audit.txt",lines);
  UnityEngine.Object.DestroyImmediate(instance);
  Debug.Log("RAINBOW_IMPORT_AUDIT_COMPLETE "+lines.Count);
 }
}
