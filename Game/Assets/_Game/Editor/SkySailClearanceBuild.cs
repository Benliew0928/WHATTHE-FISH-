using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using WhatTheFish;

// Fail before an expensive player build if route edits intersect delivered art.
public static class SkySailClearanceBuild {
 public static void Validate(){
  var world=UnityEngine.Object.FindFirstObjectByType<SkySailWorld>();var probes=new List<GameObject>();var results=new List<string>();bool failed=false;
  void Check(bool ok,string message){failed|=!ok;results.Add((ok?"PASS ":"FAIL ")+message);}
  try{
   foreach(var root in world.stations.Select(s=>s.transform).Concat(world.scenery.Cast<Transform>().Where(t=>t.name.EndsWith(" · Tower"))))
    foreach(var renderer in root.GetComponent<LODGroup>().GetLODs()[0].renderers){var mesh=renderer.GetComponent<MeshFilter>();if(!mesh)continue;var g=new GameObject(root.name+" / "+renderer.name){layer=30};g.transform.SetParent(renderer.transform,false);g.AddComponent<MeshCollider>().sharedMesh=mesh.sharedMesh;probes.Add(g);}
   Physics.SyncTransforms();
   foreach(var a in SkySailMap.Circuit){
    Check(Vector3.Distance(SkySailMap.Tower(a,SkySailMap.Neighbor(a,1)),SkySailMap.Tower(a,SkySailMap.Neighbor(a,-1)))>30,a+" has separate branch towers");
    foreach(int direction in new[]{-1,1}){
     var b=SkySailMap.Neighbor(a,direction);string collision=null;float turn=0;var last=SkySailMap.CabinRotation(a,b,0);
     for(int i=0;i<=2000;i++){
      float t=i/2000f;var p=world.scenery.TransformPoint(SkySailMap.Path(a,b,t));var q=SkySailMap.CabinRotation(a,b,t);turn=Mathf.Max(turn,Quaternion.Angle(q,last));last=q;
      var hits=Physics.OverlapBox(p+Vector3.up*1.65f,new Vector3(3.14f,2.08f,3.85f),q,1<<30);
      if(hits.Length==0)hits=Physics.OverlapBox(p+Vector3.up*5.05f,new Vector3(.36f,1.3f,1.4f),SkySailMap.HangerRotation(a,b,t),1<<30);
      if(hits.Length>0&&collision==null)collision="t="+t+" "+hits[0].name;
     }
     Check(collision==null,a+" → "+b+" body/hanger clearance "+collision);Check(turn<3,a+" → "+b+" max heading step "+turn);
    }
   }
  }finally{foreach(var g in probes)UnityEngine.Object.DestroyImmediate(g);}
  string folder=ProjectBuilder.ResolveBuildOutputPath("../Builds/SkyRailQA");Directory.CreateDirectory(folder);File.WriteAllLines(Path.Combine(folder,"editor-clearance.txt"),results);
  if(failed)throw new InvalidOperationException("Sky Rail clearance failed; inspect Builds/SkyRailQA/editor-clearance.txt");
  Debug.Log("SKY_RAIL_CLEARANCE_PASS "+results.Count+" assertions");
 }
}
