using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Linq;
using UnityEngine;
namespace WhatTheFish.Editor {
 public static class BasketballDefenseBuild {
  public static void Windows(){AssetDatabase.Refresh();EditorSceneManager.OpenScene("Assets/_Game/Scenes/Bootstrap.unity");ProjectBuilder.BuildWindowsPlayer("../Builds/BasketballDefenseQA/Player/WhatTheFish.exe");}
  // External development fixture only: do not enable mesh read/write or ship
  // another copy of the character just to inspect its posed surface in tests.
  public static void Geometry(string directory){
   var model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/RainbowSprinter.fbx");
   var data=new BasketballDefenseGeometry{meshes=model.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(s=>{
    var weights=s.sharedMesh.boneWeights;
    float Weight(BoneWeight w,string token)=>new[]{(w.boneIndex0,w.weight0),(w.boneIndex1,w.weight1),(w.boneIndex2,w.weight2),(w.boneIndex3,w.weight3)}.Where(p=>s.bones[p.Item1].name.Contains(token)).Sum(p=>p.Item2);
    var head=weights.Select(w=>Weight(w,"Head")>.9f).ToArray();var tris=s.sharedMesh.triangles;
    return new BasketballDefenseGeometry.Mesh{mesh=s.sharedMesh.name,headTriangles=Enumerable.Range(0,tris.Length/3).Where(i=>head[tris[i*3]]&&head[tris[i*3+1]]&&head[tris[i*3+2]]).SelectMany(i=>new[]{tris[i*3],tris[i*3+1],tris[i*3+2]}).ToArray(),armVertices=Enumerable.Range(0,weights.Length).Where(i=>Weight(weights[i],"Hand")+Weight(weights[i],"ForeArm")>.8f).ToArray()};
   }).ToArray()};
   Directory.CreateDirectory(directory);File.WriteAllText(Path.Combine(directory,"defense-geometry.json"),JsonUtility.ToJson(data));
  }
  public static void Scripts(){
   AssetDatabase.Refresh();
   var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=SkySailBuilder.BuildScenes(),locationPathName="../Builds/BasketballDefenseQA/Player/WhatTheFish.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development|BuildOptions.BuildScriptsOnly|BuildOptions.CompressWithLz4HC});
   if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Defense script build failed: "+report.summary.result);
  }
 }
}
