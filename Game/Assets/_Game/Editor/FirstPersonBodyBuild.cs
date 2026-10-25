#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class FirstPersonBodyBuild {
 const string Output="../Builds/FirstPersonBodyQA/Player/WhatTheFish.exe";
 public static void Build(){Validate();ProjectBuilder.BuildWindowsPlayer(Output);}
 public static void Scripts(){var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath(Output),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("First-person review scripts build failed");}
 public static void Validate(){
  var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/RainbowSprinter.fbx");int count=0;
  foreach(var skin in source.GetComponentsInChildren<SkinnedMeshRenderer>(true)){
   var mesh=skin.sharedMesh;var uv=new List<Vector4>();mesh.GetUVs(2,uv);int hidden=0,visible=0;
   foreach(var value in uv){if(value.w>.99f)hidden++;else if(value.w<.01f)visible++;else throw new Exception("Head mask must contain binary rest-pose vertices");}
   if(mesh.GetVertexAttributeDimension(VertexAttribute.TexCoord2)!=4||hidden<100||visible<100||mesh.GetBlendShapeIndex("GolfRightGrip")<0)throw new Exception("First-person head mask or original grip missing: "+mesh.name);
   var weights=mesh.boneWeights;var hands=skin.bones.Select(b=>b.name.Contains("Hand")).ToArray();
   for(int i=0;i<weights.Length;i++){var w=weights[i];bool hand=w.weight0>.001f&&hands[w.boneIndex0]||w.weight1>.001f&&hands[w.boneIndex1]||w.weight2>.001f&&hands[w.boneIndex2]||w.weight3>.001f&&hands[w.boneIndex3];if(hand&&uv[i].w>.5f)throw new Exception("First-person mask must preserve hand/finger vertices: "+mesh.name);}
   Debug.Log($"FIRST_PERSON_MESH {mesh.name} hidden={hidden} body={visible} vertices={mesh.vertexCount} grip=retained");count++;
  }
  if(count!=2)throw new Exception("Expected both original character LODs");
 }
}
#endif
