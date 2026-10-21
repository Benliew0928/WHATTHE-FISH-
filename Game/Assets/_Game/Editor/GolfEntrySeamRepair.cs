using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WhatTheFish;

public static partial class GolfMultiplayerTeeBuild {
 static void RepairEntryPathSeams(GameObject island){
  var terrain=island.GetComponentsInChildren<MeshCollider>(true).Where(c=>c.name.StartsWith("Terrain__")&&!c.transform.IsChildOf(island.GetComponentInChildren<GolfCourse>(true).transform)).ToArray();
  var report=new List<string>();
  foreach(var f in island.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name.Contains("CoastalPath_")&&AssetDatabase.GetAssetPath(f.sharedMesh).StartsWith(Folder+"cleared-"))){
   var mesh=UnityEngine.Object.Instantiate(f.sharedMesh);var matrix=island.transform.worldToLocalMatrix*f.transform.localToWorldMatrix;var inverse=matrix.inverse;var v=mesh.vertices;var p=v.Select(matrix.MultiplyPoint3x4).ToArray();int duplicates=0,raised=0;
   string Key(Vector3 at)=>Mathf.RoundToInt(at.x*1000)+","+Mathf.RoundToInt(at.z*1000);
   // UV splits keep their separate indices, but must share exactly one height.
   var heights=new Dictionary<string,float>();
   for(int i=0;i<p.Length;i++)if(OriginalEntryCorridor(p[i].x,p[i].z)>.001f){var key=Key(p[i]);float y=Mathf.Max(p[i].y,Floor(island,terrain,p[i]).y+.045f);if(!heights.TryGetValue(key,out float old)||y>old)heights[key]=y;}
   for(int i=0;i<p.Length;i++)if(heights.TryGetValue(Key(p[i]),out float y)){if(y>p[i].y+.0001f)raised++;p[i].y=y;v[i]=inverse.MultiplyPoint3x4(p[i]);}
   for(int sub=0;sub<mesh.subMeshCount;sub++){
    var source=mesh.GetTriangles(sub);var output=new List<int>();var seen=new HashSet<string>();
    for(int i=0;i<source.Length;i+=3){var a=source[i];var b=source[i+1];var c=source[i+2];var centre=(p[a]+p[b]+p[c])/3;
     if(OriginalEntryCorridor(centre.x,centre.z)>.001f){var keys=new[]{Key(p[a]),Key(p[b]),Key(p[c])};Array.Sort(keys,StringComparer.Ordinal);if(!seen.Add(string.Join(";",keys))){duplicates++;continue;}}
     output.Add(a);output.Add(b);output.Add(c);
    }
    mesh.SetTriangles(output,sub);
   }
   mesh.vertices=v;mesh.RecalculateBounds();f.sharedMesh=Save(mesh,"cleared-"+f.name);foreach(var c in f.GetComponents<MeshCollider>())c.sharedMesh=f.sharedMesh;
   report.Add(f.name+" duplicate projected faces removed="+duplicates+" supported vertices="+raised);
  }
  string evidence=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Builds/GolfEventBadgeQA"));Directory.CreateDirectory(evidence);File.WriteAllLines(Path.Combine(evidence,"entry-seams.txt"),report);
 }
 public static void BuildEventBadges(){
  string path="Assets/_Game/Resources/GolfUI/Events/BadgeSheet.png";var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
  CaptureTeeColours("badge-seams-before");
  var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/SkySail_Golf.unity");var island=scene.GetRootGameObjects().Single(r=>r.GetComponent<SkySailIslandScene>());island.SetActive(true);Physics.SyncTransforms();RepairEntryPathSeams(island);AssetDatabase.SaveAssets();CaptureTeeColours("badge-seams-after");
  ProjectBuilder.BuildWindowsPlayer("../Builds/GolfEventBadgeQA/Player/WhatTheFish.exe");
 }
 public static void BuildBadgeDelivery(){AssetDatabase.ImportAsset("Assets/_Game/Resources/GolfUI/Events/BadgeSheet.png",ImportAssetOptions.ForceUpdate);ProjectBuilder.BuildWindowsPlayer("../Builds/GolfEventBadgeQA/Player/WhatTheFish.exe");}
 public static void RefreshBadgeCode(){var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath("../Builds/GolfEventBadgeQA/Player/WhatTheFish.exe"),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Badge refresh failed");}
}
