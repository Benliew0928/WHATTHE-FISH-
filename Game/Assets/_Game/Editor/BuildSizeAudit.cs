using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

// Serialized asset bytes are uncompressed; use the APK itself for the submission budget.
public static class BuildSizeAudit {
 public const long TargetBytes=75000000;
 public const long LimitBytes=100000000;
 public const string Output="../Builds/SizeAudit/";
 public static void AuditLatest(){Write(BuildReport.GetLatestReport(),"before");}
 public static void Write(BuildReport report,string label){
  string dir=Output+label+"/";Directory.CreateDirectory(dir);
  if(report==null)throw new Exception("No Unity build report is available.");
  var totals=report.packedAssets.SelectMany(a=>a.contents)
   .GroupBy(a=>a.sourceAssetPath??"<built-in>")
   .Select(g=>new{path=g.Key,bytes=g.Sum(a=>(long)a.packedSize)})
   .OrderByDescending(a=>a.bytes).ToArray();
  File.WriteAllLines(dir+"packed-assets.csv",new[]{"asset,uncompressed_serialized_bytes"}.Concat(totals.Select(a=>Csv(a.path)+","+a.bytes)));
  File.WriteAllLines(dir+"packed-layout.csv",new[]{"file,offset,bytes,asset"}.Concat(report.packedAssets.SelectMany(a=>a.contents.Select(c=>Csv(a.shortPath)+","+c.offset+","+c.packedSize+","+Csv(c.sourceAssetPath??"<built-in>")))));
  var all=AssetDatabase.GetAllAssetPaths().Where(p=>p.StartsWith("Assets/")&&!AssetDatabase.IsValidFolder(p)).ToArray();
  var resources=all.Where(p=>p.Contains("/Resources/")&&!p.Contains("/Editor/")).ToArray();
  var roots=SkySailBuilder.BuildScenes().Concat(new[]{AssetDatabase.GetAssetPath(GraphicsSettings.defaultRenderPipeline),AssetDatabase.GetAssetPath(QualitySettings.renderPipeline)})
   .Concat(PlayerSettings.GetPreloadedAssets().Where(a=>a).Select(AssetDatabase.GetAssetPath)).Where(p=>!string.IsNullOrEmpty(p)).ToArray();
  var sceneDeps=new HashSet<string>(AssetDatabase.GetDependencies(roots,true));
  var resourceDeps=new HashSet<string>(AssetDatabase.GetDependencies(resources,true));
  File.WriteAllLines(dir+"resource-only-dependencies.csv",new[]{"asset,uncompressed_serialized_bytes"}.Concat(totals.Where(a=>resourceDeps.Contains(a.path)&&!sceneDeps.Contains(a.path)).Select(a=>Csv(a.path)+","+a.bytes)));
  File.WriteAllLines(dir+"resources.txt",resources.OrderBy(p=>p));
  var packed=new HashSet<string>(totals.Select(a=>a.path));
  // Absence is not proof an asset is obsolete: scenes/prefabs can be flattened,
  // and original FBXs may be retained to regenerate derived mesh assets.
  File.WriteAllLines(dir+"assets-without-direct-packed-entry.txt",all.Where(p=>!p.Contains("/Editor/")&&!p.EndsWith(".cs")&&!packed.Contains(p)).OrderBy(p=>p));
  var meshes=new List<string>{"asset,mesh,vertices,triangles,compression,bounds_model_space"};
  foreach(var path in totals.Select(a=>a.path).Where(p=>p.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase))){
   var importer=AssetImporter.GetAtPath(path) as ModelImporter;
   foreach(var m in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Mesh>()){
    long indices=0;for(int i=0;i<m.subMeshCount;i++)indices+=m.GetIndexCount(i);
    meshes.Add(Csv(path)+","+Csv(m.name)+","+m.vertexCount+","+(indices/3)+","+importer.meshCompression+","+Csv(m.bounds.size.ToString("F3")));
   }
  }
  File.WriteAllLines(dir+"meshes.csv",meshes);
  if(report.summary.platform==BuildTarget.Android&&File.Exists(report.summary.outputPath)){
   using var archive=ZipFile.OpenRead(report.summary.outputPath);
   File.WriteAllLines(dir+"apk-entries.csv",new[]{"entry,compressed_bytes,uncompressed_bytes"}.Concat(archive.Entries.OrderByDescending(e=>e.CompressedLength).Select(e=>Csv(e.FullName)+","+e.CompressedLength+","+e.Length)));
  }
  File.WriteAllText(dir+"summary.txt","Report target: "+report.summary.platform+"\nResult: "+report.summary.result+"\nOutput: "+report.summary.outputPath+"\nSerialized assets: "+totals.Sum(a=>a.bytes)+" bytes (NOT compressed APK size)\n");
  Debug.Log("SIZE_AUDIT_COMPLETE "+Path.GetFullPath(dir));
 }
 public static void CheckApk(string path,bool enforce){
  long bytes=new FileInfo(path).Length;
  string result=bytes>=LimitBytes?"OVER_LIMIT":bytes>TargetBytes?"ABOVE_WORKING_TARGET":"WITHIN_TARGET";
  Directory.CreateDirectory(Output);
  File.WriteAllText(Output+"latest-apk.txt",$"APK: {Path.GetFullPath(path)}\nUTC: {DateTime.UtcNow:O}\nBytes: {bytes}\nDecimal MB: {(bytes/1000000d).ToString("F2",CultureInfo.InvariantCulture)}\nTarget bytes: {TargetBytes}\nMust be below: {LimitBytes}\nBudget: {result}\n");
  if(bytes>TargetBytes)Debug.LogWarning("APK_BUDGET "+result+" bytes="+bytes+" target="+TargetBytes+" limit="+LimitBytes);
  else Debug.Log("APK_BUDGET "+result+" bytes="+bytes);
  if(enforce&&bytes>=LimitBytes)throw new UnityEditor.Build.BuildFailedException("APK exceeds the hackathon limit: "+bytes+" bytes; must be below "+LimitBytes);
 }
 static string Csv(string value)=>"\""+value.Replace("\"","\"\"")+"\"";
}
