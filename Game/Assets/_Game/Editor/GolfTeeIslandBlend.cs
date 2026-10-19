using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WhatTheFish;
using Object=UnityEngine.Object;

public static partial class GolfMultiplayerTeeBuild {
 // Regenerate from pre-tee course masters. Deform the island itself, without
 // stacking a second opaque floor on top of it.
 static void PolishT1(GameObject island,GolfCourse course,Transform root,MeshCollider[] support){
  var site=course.teeSites.First(s=>s.hole==1);
  var original=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/RefinedIslands/Golf/Golf_Terrain.mat");
  var material=AssetDatabase.LoadAssetAtPath<Material>(Folder+"T1 island blend.mat");
  if(!material){material=new Material(original);AssetDatabase.CreateAsset(material,Folder+"T1 island blend.mat");}
  material.CopyPropertiesFromMaterial(original);material.shader=Shader.Find("WhatTheFish/GolfTeeIsland");material.SetVector("_T1Data",new Vector4(site.cup.x,site.cup.z,site.Angle,site.radius));material.SetFloat("_IslandExtent",RefinedIslandBuilder.Read("Golf").extent);EditorUtility.SetDirty(material);
  var imported=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/RefinedIslands/Golf/Terrain.fbx").GetComponentsInChildren<MeshFilter>(true).ToDictionary(f=>f.name,f=>f.sharedMesh);
  var native=island.GetComponentsInChildren<MeshFilter>(true).Where(f=>imported.ContainsKey(f.name)&&!f.transform.IsChildOf(course.transform)).ToArray();
  foreach(var filter in native){filter.sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(GolfCourseBuilder.Art+"Meshes/Terrain/"+filter.name+".asset")??imported[filter.name];filter.GetComponent<MeshCollider>().sharedMesh=filter.sharedMesh;filter.GetComponent<MeshRenderer>().sharedMaterial=original;}
  Physics.SyncTransforms();
  foreach(var filter in native){
   var source=filter.sharedMesh;var v=source.vertices;var matrix=island.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;var inverse=matrix.inverse;var colors=new Color[v.Length];var surfaces=new Vector2[v.Length];var weights=new float[v.Length];bool changed=false;
   for(int i=0;i<v.Length;i++){var p=matrix.MultiplyPoint3x4(v[i]);float w=site.GroundWeight(p.x,p.z);T1Paint(site,p,out colors[i],out surfaces[i]);weights[i]=w;changed|=w>0||colors[i].a>0;p.y=Mathf.Lerp(p.y,site.height,w);v[i]=inverse.MultiplyPoint3x4(p);}
   if(!changed)continue;
   var mesh=Object.Instantiate(source);mesh.name="Joined T1 "+filter.name;mesh.vertices=v;mesh.colors=colors;mesh.uv2=surfaces;mesh.RecalculateNormals();var normals=mesh.normals;var originalNormals=source.normals;
   for(int i=0;i<normals.Length;i++)normals[i]=Vector3.Lerp(originalNormals[i],normals[i],weights[i]).normalized;
   mesh.normals=normals;mesh.RecalculateBounds();filter.sharedMesh=Save(mesh,"joined-"+filter.name);filter.GetComponent<MeshRenderer>().sharedMaterial=material;filter.GetComponent<MeshCollider>().sharedMesh=filter.sharedMesh;
  }
  foreach(string name in new[]{"Terrain__Tee terrace 1","Terrain__Rear tee path","Opening surface 1","Opening surface 2","Opening surface 3","Terrain__Reclaimed old tee spur"}){var node=root.Find(name);if(node)Object.DestroyImmediate(node.gameObject);}
  Physics.SyncTransforms();var ground=native.Select(f=>f.GetComponent<MeshCollider>()).ToArray();var drapeGround=new T1GroundIndex(island,ground);
  // Restore original dressing shapes, then refit only the tee's actual grading.
  foreach(var filter in course.GetComponentsInChildren<MeshFilter>(true)){
   if(filter.transform.IsChildOf(root)||!filter.sharedMesh)continue;
   var renderer=filter.GetComponent<MeshRenderer>();if(!renderer||!renderer.sharedMaterial||renderer.sharedMaterial.shader.name!="WhatTheFish/GolfCourseSurface")continue;
   var master=AssetDatabase.LoadAssetAtPath<Mesh>(GolfCourseBuilder.Art+"Meshes/Shared/"+filter.name+".asset");if(!master)continue; // Pond-bank meshes are authored by their own generator.
   filter.sharedMesh=master;var matrix=island.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;
   if(!master.vertices.Any(v=>T1GradingNeighborhood(site,matrix.MultiplyPoint3x4(v))))continue;
   var mesh=DrapeT1Course(master,matrix,site,drapeGround);filter.sharedMesh=Save(mesh,"draped-"+filter.name);
  }
  RestoreT1BunkerScenery(island,course);MoveT1Sign(island,course,ground);
  EditorUtility.SetDirty(course);Physics.SyncTransforms();
 }
 static float T1Smooth(float lo,float hi,float value)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(lo,hi,value));
 // Include the triangles interpolating across the analytic grading boundary.
 // Using only GroundWeight > 0 left older dressings intersecting those faces.
 static bool T1GradingNeighborhood(GolfTeeSite site,Vector3 p){float outside=Mathf.Max(Mathf.Abs(site.Distance(p.x,p.z)-(site.radius+.5f))-4.5f,(Mathf.Abs(site.AngleDelta(p.x,p.z))-site.HalfWidth)*site.radius);return outside<8;}
 static Color T1Hex(string value){ColorUtility.TryParseHtmlString("#"+value,out var color);return color.linear;}
 static void T1Paint(GolfTeeSite site,Vector3 p,out Color color,out Vector2 surface){
  float r=site.Distance(p.x,p.z),angle=site.AngleDelta(p.x,p.z),side=Mathf.Abs(angle)*r;
  float edge=1-T1Smooth(Mathf.Max(1,r*site.HalfWidth-3),r*site.HalfWidth+2,side);
  float opening=T1Smooth(5,10,r)*(1-T1Smooth(66,72,r))*edge;
  float reclaim=(1-T1Smooth(14,19,Mathf.Abs(p.x+27)))*(1-T1Smooth(3,7,Mathf.Abs(p.z+142)));
  float weight=Mathf.Max(opening,reclaim);
  float noise=.012f*Mathf.Sin(p.x*.24f)*Mathf.Sin(p.z*.31f)+.007f*Mathf.Cos(p.x*.77f+p.z*.19f);
  float stripe=Mathf.Sin((p.z+p.x*.18f)*.3f)>0?1:0;
  Color lawn=new Color(.5255f+noise+.015f*stripe,.698f+noise+.025f*stripe,.2863f+noise+.009f*stripe).linear;
  float bunker=T1Smooth(38,40,r)*(1-T1Smooth(43,45,r))*edge;
  float longer=T1Smooth(22,24,r)*(1-T1Smooth(26,28,r))*edge;
  Color tint=Color.Lerp(lawn,T1Hex("679544"),longer*.65f);tint=Color.Lerp(tint,T1Hex("E8CF9D"),bunker);
  float pathMask=(1-T1Smooth(1.1f,2.1f,Mathf.Abs(r-75.5f)))*(1-T1Smooth(.30f,.36f,Mathf.Abs(angle)));
  float pathZ=p.z-(p.z<-100?6*Mathf.Exp(-Mathf.Pow((p.x+64)/18,4)):0);
  float road=Mathf.Sqrt(p.x*p.x/(145*145)+pathZ*pathZ/(156*156));pathMask*=1-T1Smooth(1.005f,1.024f,road);
  tint=Color.Lerp(tint,T1Hex("D6C29A"),pathMask);weight=Mathf.Max(weight,pathMask);
  weight*=1-GolfTeeSite.RestoredBunker(p.x,p.z);
  color=new Color(tint.r,tint.g,tint.b,weight);surface=new Vector2(Mathf.Lerp(bunker,pathMask,pathMask),0);
 }
 static void MoveT1Sign(GameObject island,GolfCourse course,MeshCollider[] ground){
  var sign=course.transform.Find("Tee sign 1");if(!sign)throw new InvalidOperationException("Missing Hole 1 sign");
  var post=course.transform.Find("Tee sign post 1")??course.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Tee sign post").OrderBy(t=>Vector3.ProjectOnPlane(t.position-sign.position,Vector3.up).sqrMagnitude).First();
  var site=course.teeSites.First(s=>s.hole==1);var radial=Vector3.ProjectOnPlane(site.centre-site.cup,Vector3.up).normalized;var tangent=new Vector3(-radial.z,0,radial.x);
  var p=Floor(island,ground,site.centre+radial*6+tangent*3);var rotation=course.holes.First(h=>h.number==1).tee.rotation;
  var local=sign.parent.InverseTransformPoint(island.transform.TransformPoint(p+Vector3.up*1.2f));
  if(sign is RectTransform rect)rect.anchoredPosition3D=local;else sign.localPosition=local;
  sign.rotation=rotation;post.name="Tee sign post 1";post.position=island.transform.TransformPoint(p+Vector3.up*.34f);
 }
}
