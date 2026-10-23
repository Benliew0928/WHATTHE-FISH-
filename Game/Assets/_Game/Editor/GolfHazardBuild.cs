using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Netcode;
using WhatTheFish;
public static class GolfHazardBuild {
 const string Art="Assets/_Game/Art/Golf/Events",Resource="Assets/_Game/Resources/GolfEvents";
 public static void Prepare(){
  AssetDatabase.Refresh();
  foreach(string name in new[]{"TornadoBadge","CrabBadge","StormPuff"}){var image=(TextureImporter)AssetImporter.GetAtPath("Assets/_Game/Resources/GolfUI/Events/"+name+".png");image.textureType=TextureImporterType.Default;image.alphaSource=TextureImporterAlphaSource.FromInput;image.alphaIsTransparency=true;image.mipmapEnabled=name=="StormPuff";image.isReadable=false;image.npotScale=TextureImporterNPOTScale.None;image.maxTextureSize=1024;image.textureCompression=TextureImporterCompression.CompressedHQ;image.SaveAndReimport();}
  Directory.CreateDirectory(Path.Combine(Application.dataPath,"..",Resource));AssetDatabase.Refresh();
  foreach(string name in new[]{"StormVolume","StormParticle"}){string path=Resource+"/"+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);bool create=!material;if(create)material=new Material(Shader.Find("WhatTheFish/Golf"+name));if(name=="StormParticle")material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Resources/GolfUI/Events/StormPuff.png"));if(create)AssetDatabase.CreateAsset(material,path);else EditorUtility.SetDirty(material);}
  var importer=(ModelImporter)AssetImporter.GetAtPath(Art+"/CrossingCrab.fbx");if(!importer)throw new Exception("Crab FBX missing");
  importer.animationType=ModelImporterAnimationType.Generic;importer.importAnimation=false;importer.optimizeGameObjects=false;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.materialLocation=ModelImporterMaterialLocation.InPrefab;importer.SaveAndReimport();
  Directory.CreateDirectory(Path.Combine(Application.dataPath,"..",Art,"Materials"));Directory.CreateDirectory(Path.Combine(Application.dataPath,"..",Resource));AssetDatabase.Refresh();
  var model=AssetDatabase.LoadAssetAtPath<GameObject>(Art+"/CrossingCrab.fbx");var instance=(GameObject)PrefabUtility.InstantiatePrefab(model);instance.name="CrossingCrab";
  try{
   foreach(var animator in instance.GetComponentsInChildren<Animator>())UnityEngine.Object.DestroyImmediate(animator);
   foreach(var renderer in instance.GetComponentsInChildren<SkinnedMeshRenderer>()){
    renderer.sharedMaterials=renderer.sharedMaterials.Select(source=>{
     string name=source.name.Replace(' ','-');string path=Art+"/Materials/"+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);bool create=!material;if(create)material=new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));
     material.name=name;material.SetColor("_BaseColor",source.color);material.enableInstancing=true;if(material.HasProperty("_Smoothness"))material.SetFloat("_Smoothness",.22f);if(create)AssetDatabase.CreateAsset(material,path);else EditorUtility.SetDirty(material);return material;
    }).ToArray();renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=true;renderer.updateWhenOffscreen=false;
   }
   var high=instance.GetComponentsInChildren<Renderer>().Where(r=>r.name.Contains("LOD0")).ToArray();var low=instance.GetComponentsInChildren<Renderer>().Where(r=>r.name.Contains("LOD1")).ToArray();
   if(high.Length!=1||low.Length!=1)throw new Exception("Expected two skinned crab LODs");var lod=instance.GetComponent<LODGroup>();if(!lod)lod=instance.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.15f,high),new LOD(.035f,low)});lod.RecalculateBounds();
   PrefabUtility.SaveAsPrefabAsset(instance,Resource+"/CrossingCrab.prefab");AssetDatabase.SaveAssets();
  }finally{UnityEngine.Object.DestroyImmediate(instance);}
  int checks=0;void Check(bool ok,string name){if(!ok)throw new Exception("Hazard check: "+name);checks++;Debug.Log("GOLF_HAZARD_CHECK PASS "+name);}
  Check(GolfEventSchedule.RandomKinds.Count==5&&GolfEventSchedule.IsRandom(GolfEventKind.Tornado)&&GolfEventSchedule.IsRandom(GolfEventKind.CrabCrossing)&&!GolfEventSchedule.IsRandom((GolfEventKind)2),"five events with retired id reserved");
  var state=new GolfMatchState();state.Start(new[]{new GolfEntrant(0,"Host"),new GolfEntrant(1,"Friend")},0);var schedule=new GolfEventSchedule();schedule.Begin(0,23);schedule.Advance(state,49.99,2);Check(!schedule.WarningActive(49.99),"no warning before fifty seconds");schedule.Advance(state,50,2);Check(schedule.WarningActive(50)&&schedule.Snapshot.start==60,"warning at fifty and event at sixty");schedule.Advance(state,60,2);Check(schedule.Active(60)!=GolfEventKind.None&&schedule.Snapshot.nextCycle==120,"sixty-second repeating cycle");
  var shared=schedule.Snapshot;shared.kind=GolfEventKind.Tornado;var a=GolfHazardRules.Offset(shared.kind,shared,3,80,Vector3.forward);var peer=new GolfEventSchedule();peer.Receive(shared);Check(a==GolfHazardRules.Offset(peer.Snapshot.kind,peer.Snapshot,3,80,Vector3.forward),"peers reconstruct same hazard lane");
  Vector3 from=new(2,3,4),to=new(10,0,5);float flight=1.35f;var velocity=GolfHazardRules.TossVelocity(from,to,flight);Check(Vector3.Distance(from+velocity*flight+Physics.gravity*(.5f*flight*flight),to)<.001f,"physical toss reaches selected landing");
  Check(GolfHazardRules.TossDistance(shared,0)>=14&&GolfHazardRules.TossDistance(shared,1)<=20&&GolfHazardRules.Stamp(1,2)!=GolfHazardRules.Stamp(2,2),"bounded toss and round-scoped immunity");
  var snapshot=new GolfBallSnapshot{valid=true,active=true,round=2,hazardHeld=true,tornadoStamp=GolfHazardRules.Stamp(2,3),rotation=Quaternion.identity};
  using(var writer=new FastBufferWriter(1024,Allocator.Temp)){writer.WriteNetworkSerializable(snapshot);using(var reader=new FastBufferReader(writer,Allocator.Temp)){reader.ReadNetworkSerializable(out GolfBallSnapshot decoded);Check(snapshot.Equals(decoded)&&decoded.hazardHeld&&decoded.tornadoStamp==snapshot.tornadoStamp,"hold lock and immunity survive network round trip");}}
  var fishing=new FishingState();fishing.Begin(new[]{0ul,1ul},1,0,false);Check(fishing.Players.All(p=>!p.ready)&&fishing.Start(new[]{0ul,1ul},2,0,FishingMode.Round),"host starts without any readiness flags");
  Check(!fishing.Start(new[]{0ul,1ul},3,0,FishingMode.Round),"duplicate start cannot restart active draft");
  var prefab=Resources.Load<GameObject>("GolfEvents/CrossingCrab");Check(prefab&&prefab.GetComponent<LODGroup>()&&prefab.GetComponentsInChildren<SkinnedMeshRenderer>().Length==2,"complete animated two-LOD crab resource");
  Check(prefab.GetComponentsInChildren<Transform>().Count(t=>t.name.StartsWith("Leg_"))==8,"eight editable leg bones");
  Check(AssetDatabase.GetDependencies(Resource+"/CrossingCrab.prefab",true).All(p=>p.StartsWith("Assets/")||p.StartsWith("Packages/")||p.StartsWith("Library/")),"crab dependencies stay inside project");
  GolfRoundPolishChecks.Run();
  Debug.Log("GOLF_HAZARD_PREPARE_OK checks="+checks);
 }
 public static void Windows(){Prepare();var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-hazardOutput");ProjectBuilder.BuildWindowsPlayer(at>=0&&at+1<args.Length?args[at+1]:"../Builds/GolfHazards/Player/WhatTheFish.exe");}
 // Used only after a full build when code changes leave serialized content intact.
 public static void RefreshWindows(){var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-hazardOutput");if(at<0||at+1>=args.Length)throw new Exception("Pass -hazardOutput for the existing full player");var report=PackagedContentBuild.BuildScripts(BuildTarget.StandaloneWindows64,ProjectBuilder.ResolveBuildOutputPath(args[at+1]),BuildOptions.Development|BuildOptions.CompressWithLz4HC);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Hazard player code refresh failed");}
}
