using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WhatTheFish;
using Object=UnityEngine.Object;

// Small contained ponds with horizontal water and terrain-conforming banks.
// Existing terrain meshes, cup openings and every outside surface stay intact.
public static class GolfPondArtBuilder {
 const string Folder=GolfCourseBuilder.Art;
 static Mesh Save(Mesh mesh,string name){
  string path=Folder+"Meshes/Shared/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
  if(old){old.Clear(false);EditorUtility.CopySerialized(mesh,old);old.UploadMeshData(false);EditorUtility.SetDirty(old);Object.DestroyImmediate(mesh);return old;}
  AssetDatabase.CreateAsset(mesh,path);return mesh;
 }
 public static Vector3 Ground(GameObject island,float x,float z){
  var ray=new Ray(island.transform.TransformPoint(new Vector3(x,100,z)),Vector3.down);
  foreach(var c in island.GetComponentsInChildren<MeshCollider>(true).Where(c=>c.name.StartsWith("Terrain__")))if(c.Raycast(ray,out var hit,200))return island.transform.InverseTransformPoint(hit.point);
  throw new Exception("Pond without original terrain at "+x+","+z);
 }
 static GameObject Node(Transform parent,string name,Mesh mesh,Material material,bool collision){
  var node=new GameObject(name);node.layer=collision?8:10;node.transform.SetParent(parent,false);
  node.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=node.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
  if(collision)node.AddComponent<MeshCollider>().sharedMesh=mesh;return node;
 }
 public static void RefreshCreatedWater(GameObject island,GolfCourse course){
  var material=AssetDatabase.LoadAssetAtPath<Material>(Folder+"Materials/Pond water.mat");if(material){material.renderQueue=3000;EditorUtility.SetDirty(material);}
  foreach(var patch in course.waterPatches){
   var node=course.transform.Find(patch.name+"/Pond water");if(!node)continue;var filter=node.GetComponent<MeshFilter>();
   const int sides=64,rings=6;var points=new List<Vector3>{Vector3.zero};var uv=new List<Vector2>{Vector2.zero};var triangles=new List<int>();
   for(int r=1;r<=rings;r++)for(int i=0;i<sides;i++){float a=-i*Mathf.PI*2/sides,t=r/(float)rings;points.Add(new Vector3(Mathf.Cos(a)*patch.radii.x*t,0,Mathf.Sin(a)*patch.radii.y*t));uv.Add(new Vector2(Mathf.Cos(a)*t,Mathf.Sin(a)*t));}
   for(int i=0;i<sides;i++)triangles.AddRange(new[]{0,1+i,1+(i+1)%sides});
   for(int r=0;r<rings-1;r++)for(int i=0;i<sides;i++){int a=1+r*sides+i,b=1+r*sides+(i+1)%sides,c=a+sides,d=b+sides;triangles.AddRange(new[]{a,c,b,b,c,d});}
   var mesh=new Mesh{name=patch.name+" water"};mesh.SetVertices(points);mesh.SetTriangles(triangles,0);mesh.SetUVs(0,uv);mesh.RecalculateNormals();mesh.RecalculateBounds();filter.sharedMesh=Save(mesh,patch.name+" water");
   node.localPosition=patch.centre;
  }
 }
 public static void Attach(GameObject island,GolfCourse course){
  if(course.waterPatches.Length==0)return;
  var material=AssetDatabase.LoadAssetAtPath<Material>(Folder+"Materials/Pond water.mat");
  if(!material){material=new Material(Shader.Find("WhatTheFish/LagoonWater"));material.SetColor("_ShallowColor",new Color(.03f,.36f,.95f));material.SetColor("_DeepColor",new Color(.015f,.25f,.8f));material.SetFloat("_Opacity",.28f);material.SetFloat("_WaveSpeed",1);material.SetFloat("_ReflectionReady",0);material.enableInstancing=true;AssetDatabase.CreateAsset(material,Folder+"Materials/Pond water.mat");}
  var sand=AssetDatabase.LoadAssetAtPath<Material>(Folder+"Materials/Circuit sand.mat");
  material.renderQueue=3000;EditorUtility.SetDirty(material);
  for(int index=0;index<course.waterPatches.Length;index++){
   var patch=course.waterPatches[index];if(course.transform.Find(patch.name))continue;
   float highest=float.NegativeInfinity;
   // Contained banks allow a flat pool on the existing slope without moving
   // terrain or lowering surrounding fairway/green/tee geometry.
   for(float x=-patch.radii.x*patch.bankScale;x<=patch.radii.x*patch.bankScale;x+=.5f)
    for(float z=-patch.radii.y*patch.bankScale;z<=patch.radii.y*patch.bankScale;z+=.5f)
     if(x*x/(patch.radii.x*patch.radii.x)+z*z/(patch.radii.y*patch.radii.y)<=patch.bankScale*patch.bankScale)highest=Mathf.Max(highest,Ground(island,patch.centre.x+x,patch.centre.z+z).y);
   patch.centre.y=highest+.08f;course.waterPatches[index]=patch;
   var parent=new GameObject(patch.name);parent.transform.SetParent(course.transform,false);
   const int sides=64;var points=new List<Vector3>();var uv=new List<Vector2>();var colors=new List<Color>();var triangles=new List<int>();
   float[] rings={.99f,1.08f,1.25f,patch.bankScale};
   for(int r=0;r<rings.Length;r++)for(int i=0;i<sides;i++){
    float a=-i*Mathf.PI*2/sides,x=patch.centre.x+Mathf.Cos(a)*patch.radii.x*rings[r],z=patch.centre.z+Mathf.Sin(a)*patch.radii.y*rings[r];
    var ground=Ground(island,x,z);float y=r==0?patch.centre.y-.04f:r==1?patch.centre.y+.10f:r==2?Mathf.Lerp(patch.centre.y+.1f,ground.y+.012f,.6f):ground.y+.008f;
    points.Add(new Vector3(x,y,z));uv.Add(new Vector2(x,z)/4);colors.Add(new Color(1,1,1,r==3?0:1));
   }
   for(int r=0;r<rings.Length-1;r++)for(int i=0;i<sides;i++){int a=r*sides+i,b=r*sides+(i+1)%sides,c=a+sides,d=b+sides;triangles.AddRange(new[]{a,c,b,b,c,d});}
   var bank=new Mesh{name=patch.name+" bank"};bank.SetVertices(points);bank.SetTriangles(triangles,0);bank.SetUVs(0,uv);bank.SetColors(colors);bank.RecalculateNormals();bank.RecalculateBounds();
   Node(parent.transform,"Pond bank",Save(bank,patch.name+" bank"),sand,true);
   points.Clear();uv.Clear();triangles.Clear();points.Add(Vector3.zero);uv.Add(Vector2.zero);
   // A few shared radial rings retain subtle existing shader ripples; no map
   // texture, reflection camera, rigidbody or per-frame CPU mesh work is added.
   const int waterRings=6;
   for(int r=1;r<=waterRings;r++)for(int i=0;i<sides;i++){float a=-i*Mathf.PI*2/sides,t=r/(float)waterRings;points.Add(new Vector3(Mathf.Cos(a)*patch.radii.x*t,0,Mathf.Sin(a)*patch.radii.y*t));uv.Add(new Vector2(Mathf.Cos(a)*t,Mathf.Sin(a)*t));}
   for(int i=0;i<sides;i++)triangles.AddRange(new[]{0,1+i,1+(i+1)%sides});
   for(int r=0;r<waterRings-1;r++)for(int i=0;i<sides;i++){int a=1+r*sides+i,b=1+r*sides+(i+1)%sides,c=a+sides,d=b+sides;triangles.AddRange(new[]{a,c,b,b,c,d});}
   var water=new Mesh{name=patch.name+" water"};water.SetVertices(points);water.SetTriangles(triangles,0);water.SetUVs(0,uv);water.RecalculateNormals();water.RecalculateBounds();
   Node(parent.transform,"Pond water",Save(water,patch.name+" water"),material,false).transform.localPosition=patch.centre;
  }
  GolfCourseBuilder.ClearAddedSandGrass(island,course.waterPatches.Select(w=>new GolfSandPatch{centre=w.centre,radii=w.radii*w.bankScale}).ToArray());
  EditorUtility.SetDirty(course);AssetDatabase.SaveAssets();Physics.SyncTransforms();
 }
}
