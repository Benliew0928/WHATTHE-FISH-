using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Read-only inventory: an absent shader property is only a candidate, not
// permission to remove an authoring asset or a runtime shader-swap dependency.
public static class HudResourceAudit {
 public static void Run(){
  const string output="../Builds/SimpleHudQA/";Directory.CreateDirectory(output);
  var lines=new List<string>{"material\tshader\tproperty\ttexture\tpresentInShader"};
  foreach(var path in AssetDatabase.FindAssets("t:Material",new[]{"Assets/_Game"}).Select(AssetDatabase.GUIDToAssetPath)){
   var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(!material||!material.shader)continue;
   var serialized=new SerializedObject(material);var values=serialized.FindProperty("m_SavedProperties.m_TexEnvs");
   for(int i=0;i<values.arraySize;i++){
    var entry=values.GetArrayElementAtIndex(i);var name=entry.FindPropertyRelative("first").stringValue;var texture=entry.FindPropertyRelative("second.m_Texture").objectReferenceValue;
    if(texture)lines.Add(path+"\t"+material.shader.name+"\t"+name+"\t"+AssetDatabase.GetAssetPath(texture)+"\t"+material.HasProperty(name));
   }
  }
  File.WriteAllLines(output+"material-textures.tsv",lines);
  var graphics=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
  var shaders=graphics.FindProperty("m_AlwaysIncludedShaders");var shaderLines=new List<string>();
  for(int i=0;i<shaders.arraySize;i++){var shader=shaders.GetArrayElementAtIndex(i).objectReferenceValue;shaderLines.Add(shader?shader.name:"MISSING");}
  File.WriteAllLines(output+"forced-shaders.txt",shaderLines);
  var roots=SkySailBuilder.BuildScenes().Concat(AssetDatabase.GetAllAssetPaths().Where(p=>p.StartsWith("Assets/")&&p.Contains("/Resources/")&&!AssetDatabase.IsValidFolder(p))).ToArray();
  var dependencies=new HashSet<string>(AssetDatabase.GetDependencies(roots,true));
  File.WriteAllLines(output+"scene-resource-dependencies.txt",dependencies.OrderBy(p=>p));
  File.WriteAllLines(output+"unreferenced-art-candidates.txt",AssetDatabase.GetAllAssetPaths().Where(p=>p.StartsWith("Assets/_Game/Art/")&&!AssetDatabase.IsValidFolder(p)&&!dependencies.Contains(p)).OrderBy(p=>p));
  Debug.Log("HUD_RESOURCE_AUDIT_COMPLETE");
 }
 public static void Windows(){Run();ProjectBuilder.BuildWindowsPlayer("../Builds/SimpleHudQA/Player/WhatTheFish.exe");}
 public static void Scripts(){AssetDatabase.Refresh();var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=SkySailBuilder.BuildScenes(),locationPathName="../Builds/SimpleHudQA/Player/WhatTheFish.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development|BuildOptions.BuildScriptsOnly|BuildOptions.CompressWithLz4HC});if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("HUD build failed: "+report.summary.result);}
}
