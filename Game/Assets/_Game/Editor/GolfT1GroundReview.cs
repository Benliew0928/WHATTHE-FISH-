using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WhatTheFish;

public static partial class GolfMultiplayerTeeBuild {
 static void ValidateT1Ground(GameObject island,GolfCourse course,Action<bool,string> check){
  var imported=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/RefinedIslands/Golf/Terrain.fbx").GetComponentsInChildren<MeshFilter>(true).ToDictionary(f=>f.name,f=>f.sharedMesh);
  var site=course.teeSites.First(s=>s.hole==1);int restored=0,joined=0;var seam=new Dictionary<Vector2Int,List<float>>();
  foreach(var filter in island.GetComponentsInChildren<MeshFilter>(true).Where(f=>imported.ContainsKey(f.name)&&!f.transform.IsChildOf(course.transform))){
   var master=AssetDatabase.LoadAssetAtPath<Mesh>(GolfCourseBuilder.Art+"Meshes/Terrain/"+filter.name+".asset")??imported[filter.name];
   var mesh=filter.sharedMesh;var old=master.vertices;var v=mesh.vertices;var matrix=island.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;bool shape=true,blue=true;
   check(v.Length==old.Length&&mesh.triangles.SequenceEqual(master.triangles),"original terrain topology preserved "+filter.name);
   bool painted=filter.GetComponent<MeshRenderer>().sharedMaterial.shader.name=="WhatTheFish/GolfTeeIsland";if(painted)joined++;
   var colors=mesh.colors;
   for(int i=0;i<v.Length;i++){
    var original=matrix.MultiplyPoint3x4(old[i]);var actual=matrix.MultiplyPoint3x4(v[i]);
    if(site.GroundWeight(original.x,original.z)==0)shape&=Vector3.Distance(original,actual)<.001f;
    if(GolfTeeSite.RestoredBunker(original.x,original.z)>.999f){restored++;blue&=Vector3.Distance(original,actual)<.001f&&(!painted||colors[i].a<.0001f);}
    var key=new Vector2Int(Mathf.RoundToInt(actual.x*1000),Mathf.RoundToInt(actual.z*1000));if(!seam.TryGetValue(key,out var values)){values=new List<float>();seam[key]=values;}values.Add(actual.y);
   }
   check(shape,"terrain outside T1 grading retains its original position "+filter.name);check(blue,"blue-circle native ground and colour restored "+filter.name);
  }
  check(joined>0&&restored>50,"joined ground and restored bunker vertices exist");
  check(seam.Values.Where(v=>v.Count>1).All(v=>v.Max()-v.Min()<.002f),"shared terrain vertices have no height steps");
  check(site.Surface(-42,-112)==0,"restored bunker uses original surface rules");
  var ground=island.GetComponentsInChildren<MeshCollider>(true).Where(c=>imported.ContainsKey(c.name)&&!c.transform.IsChildOf(course.transform)).ToArray();
  foreach(var filter in course.GetComponentsInChildren<MeshFilter>(true).Where(f=>AssetDatabase.GetAssetPath(f.sharedMesh).StartsWith(Folder+"draped-"))){
   var mesh=filter.sharedMesh;var v=mesh.vertices;var colors=mesh.colors;var t=mesh.triangles;var matrix=island.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;float minimum=float.MaxValue;int samples=0;
   for(int i=0;i<t.Length;i+=3){int a=t[i],b=t[i+1],c=t[i+2];if((colors[a].a+colors[b].a+colors[c].a)/3<.02f)continue;var p=matrix.MultiplyPoint3x4((v[a]+v[b]+v[c])/3);if(!T1GradingNeighborhood(site,p))continue;float clearance=p.y-Floor(island,ground,p).y;minimum=Mathf.Min(minimum,clearance);samples++;}
   check(samples==0||minimum>.005f,"course dressing clears actual graded terrain "+filter.name+(samples==0?" (only transparent edge touches T1)":" min="+minimum.ToString("F4")+" m samples="+samples));
  }
 }
}
