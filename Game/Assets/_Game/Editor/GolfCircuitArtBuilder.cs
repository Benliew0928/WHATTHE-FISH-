using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WhatTheFish;
using Object=UnityEngine.Object;

// Reuses the existing island kit. Course art is regenerated from FiveHoleCourse.json.
public static class GolfCircuitArtBuilder {
 [Serializable] public sealed class Obstacle {public string name,module;public Vector3 position,size;public float yaw;}
 const string Folder=GolfCourseBuilder.Art;
 static Material Surface(string name,string source,Color tint){
  string path=Folder+"Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(!m){m=new Material(Shader.Find("WhatTheFish/GolfCourseSurface"));AssetDatabase.CreateAsset(m,path);}
  var original=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/RefinedIslands/Shared/Materials/RI_"+source+".mat");
  m.SetTexture("_BaseMap",original.GetTexture("_BaseMap"));m.SetColor("_Tint",tint);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;
 }
 static Mesh Save(Mesh m,string name){
  string path=Folder+"Meshes/Shared/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
  if(old){old.Clear(false);EditorUtility.CopySerialized(m,old);old.UploadMeshData(false);EditorUtility.SetDirty(old);Object.DestroyImmediate(m);return old;}
  AssetDatabase.CreateAsset(m,path);return m;
 }
 static Vector3 Ground(GameObject island,MeshCollider[] terrain,float x,float z){
  var ray=new Ray(island.transform.TransformPoint(new Vector3(x,100,z)),Vector3.down);
  foreach(var c in terrain)if(c.Raycast(ray,out var hit,200))return island.transform.InverseTransformPoint(hit.point);
  foreach(float offset in new[]{-.02f,.02f})foreach(var c in terrain)if(c.Raycast(new Ray(ray.origin+Vector3.right*offset,Vector3.down),out var hit,200))return new Vector3(x,island.transform.InverseTransformPoint(hit.point).y,z);
  throw new Exception("Circuit art without terrain "+x+","+z);
 }
 static void Patch(GameObject island,GolfCourse course,string name,Vector2 min,Vector2 max,Func<float,float,float> mask,Material material,MeshCollider[] terrain){
  var p=new List<Vector3>();var uv=new List<Vector2>();var colors=new List<Color>();var triangles=new List<int>();const float step=1f;
  int nx=Mathf.CeilToInt(max.x-min.x),nz=Mathf.CeilToInt(max.y-min.y);
  for(int iz=0;iz<nz;iz++)for(int ix=0;ix<nx;ix++){
   float x=min.x+ix*step,z=min.y+iz*step;
   var samples=new[]{new Vector2(x,z),new Vector2(x,z+step),new Vector2(x+step,z+step),new Vector2(x+step,z)};
   if(samples.All(v=>mask(v.x,v.y)<=0))continue;int n=p.Count;
   foreach(var v in samples){p.Add(Ground(island,terrain,v.x,v.y)+Vector3.up*.025f);uv.Add(v/4);colors.Add(new Color(1,1,1,Mathf.Clamp01(mask(v.x,v.y))));}
   triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
  }
  // Adjacent cells share all attributes: weld once before serializing mobile art.
  var used=new Dictionary<Vector3,int>();var wp=new List<Vector3>();var wu=new List<Vector2>();var wc=new List<Color>();var remap=new int[p.Count];
  for(int i=0;i<p.Count;i++){if(!used.TryGetValue(p[i],out int index)){index=wp.Count;used.Add(p[i],index);wp.Add(p[i]);wu.Add(uv[i]);wc.Add(colors[i]);}remap[i]=index;}
  var mesh=new Mesh{name=name,indexFormat=wp.Count>65535?UnityEngine.Rendering.IndexFormat.UInt32:UnityEngine.Rendering.IndexFormat.UInt16};mesh.SetVertices(wp);mesh.SetTriangles(triangles.Select(i=>remap[i]).ToArray(),0);mesh.SetUVs(0,wu);mesh.SetColors(wc);mesh.RecalculateNormals();mesh.RecalculateBounds();
  var node=new GameObject(name);node.layer=10;node.transform.SetParent(course.transform,false);node.AddComponent<MeshFilter>().sharedMesh=Save(mesh,name);var renderer=node.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
 }
 public static void Attach(GameObject island,GolfCourse course,Obstacle[] obstacles){
  var terrain=island.GetComponentsInChildren<MeshCollider>(true).Where(c=>c.name.StartsWith("Terrain__")).ToArray();
  var turf=Surface("Circuit mown fairway","Turf",new Color(.79f,.99f,.62f,.63f));var sand=Surface("Circuit sand","Sand",new Color(1,.96f,.82f,1));
  foreach(var f in course.fairways){
   float Distance(float x,float z,Vector3[] points){var p=new Vector2(x,z);float d=float.MaxValue;for(int i=1;i<points.Length;i++){var a=new Vector2(points[i-1].x,points[i-1].z);var b=new Vector2(points[i].x,points[i].z);var delta=b-a;float t=Mathf.Clamp01(Vector2.Dot(p-a,delta)/delta.sqrMagnitude);d=Mathf.Min(d,Vector2.Distance(p,a+delta*t));}return d;}
   float Mask(float x,float z){if(MobileIslandGrass.GolfSand(x,z)>.1f||course.sandPatches.Any(s=>s.Contains(x,z)))return 0;float edge=1;foreach(var green in course.greens)edge=Mathf.Min(edge,Mathf.Clamp01((Vector2.Distance(new Vector2(x,z),new Vector2(green.centre.x,green.centre.z))-green.radius)/2));float alpha=edge*Mathf.Clamp01((f.width*.5f-Distance(x,z,f.points))/2);if(f.alternative)foreach(var main in course.fairways.Where(p=>!p.alternative))alpha*=1-Mathf.Clamp01((main.width*.5f-Distance(x,z,main.points))/2);return alpha;}
   float r=f.width*.5f+1;
   Patch(island,course,"Fairway "+f.hole+(f.alternative?" bypass":""),new Vector2(f.points.Min(p=>p.x)-r,f.points.Min(p=>p.z)-r),new Vector2(f.points.Max(p=>p.x)+r,f.points.Max(p=>p.z)+r),Mask,turf,terrain);
  }
  foreach(var s in course.sandPatches){
   float Mask(float x,float z){float a=s.yaw*Mathf.Deg2Rad,dx=x-s.centre.x,dz=z-s.centre.z;float u=(dx*Mathf.Cos(a)+dz*Mathf.Sin(a))/s.radii.x,v=(-dx*Mathf.Sin(a)+dz*Mathf.Cos(a))/s.radii.y;return Mathf.Clamp01((1-Mathf.Sqrt(u*u+v*v))*10);}
   float radius=Mathf.Max(s.radii.x,s.radii.y)+2;Patch(island,course,s.name,new Vector2(s.centre.x-radius,s.centre.z-radius),new Vector2(s.centre.x+radius,s.centre.z+radius),Mask,sand,terrain);
  }
  foreach(var hole in course.holes){
   var at=hole.tee.position-hole.tee.forward*3+hole.tee.right*3;
   var plaque=new GameObject("Tee sign "+hole.number,typeof(RectTransform),typeof(Canvas));plaque.layer=10;plaque.transform.SetParent(course.transform,false);plaque.transform.SetPositionAndRotation(at+Vector3.up*1.2f,hole.tee.rotation);plaque.transform.localScale=Vector3.one*.006f;
   var canvas=plaque.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;plaque.GetComponent<RectTransform>().sizeDelta=new Vector2(420,160);
   var panel=new GameObject("Teal panel",typeof(RectTransform),typeof(UnityEngine.UI.Image));panel.transform.SetParent(plaque.transform,false);var rect=panel.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(420,160);panel.GetComponent<UnityEngine.UI.Image>().color=CoveUI.Ink;panel.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
   var label=new GameObject("Hole and par",typeof(RectTransform),typeof(UnityEngine.UI.Text));label.transform.SetParent(plaque.transform,false);var text=label.GetComponent<UnityEngine.UI.Text>();text.rectTransform.sizeDelta=new Vector2(400,150);text.font=CoveUI.DisplayFont;text.fontSize=44;text.alignment=TextAnchor.MiddleCenter;text.color=CoveUI.Cream;text.text="HOLE "+hole.number+"   PAR "+hole.par+"\n<size=26>"+hole.title+"</size>";text.raycastTarget=false;
   // Existing timber marker mesh/material; no extra image, font or collider.
   var timber=GameObject.CreatePrimitive(PrimitiveType.Cylinder);timber.name="Tee sign post";Object.DestroyImmediate(timber.GetComponent<Collider>());timber.layer=10;timber.transform.SetParent(course.transform,false);timber.transform.position=at+Vector3.up*.34f;timber.transform.localScale=new Vector3(.10f,.36f,.10f);timber.GetComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Art/RefinedIslands/Shared/Materials/RI_Timber.mat");
  }
  var group=new GameObject("Circuit obstacles");group.transform.SetParent(course.transform,false);
  foreach(var item in obstacles??Array.Empty<Obstacle>())AddObstacle(island,group.transform,item);
  foreach(var rough in course.roughPatches)AddSurfacePatch(island,course,rough,true);
  AssetDatabase.SaveAssets();Physics.SyncTransforms();
 }
 public static void AddObstacle(GameObject island,Transform group,Obstacle item){
  var terrain=island.GetComponentsInChildren<MeshCollider>(true).Where(c=>c.name.StartsWith("Terrain__")).ToArray();
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/RefinedIslands/"+item.module+".prefab");if(!prefab)throw new Exception("Missing circuit module "+item.module);
   var node=(GameObject)PrefabUtility.InstantiatePrefab(prefab);node.name=item.name;node.transform.SetParent(group,false);
   if(item.module.StartsWith("Rock")){
    var filter=node.GetComponentsInChildren<MeshFilter>(true).First(f=>f.name.StartsWith("LOD1"));var sourceBounds=filter.sharedMesh.bounds;
    // FBX child transforms include the import's metre conversion. Measure in
    // prefab-root space; raw mesh bounds alone would produce 100x colliders.
    var matrix=node.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;var bounds=new Bounds(matrix.MultiplyPoint3x4(sourceBounds.min),Vector3.zero);
    for(int bits=0;bits<8;bits++)bounds.Encapsulate(matrix.MultiplyPoint3x4(new Vector3((bits&1)==0?sourceBounds.min.x:sourceBounds.max.x,(bits&2)==0?sourceBounds.min.y:sourceBounds.max.y,(bits&4)==0?sourceBounds.min.z:sourceBounds.max.z)));
    node.transform.localScale=new Vector3(item.size.x/bounds.size.x,item.size.y/bounds.size.y,item.size.z/bounds.size.z);
    // Keep collision and every LOD from the same kit prefab. Embed its base.
    var at=Ground(island,terrain,item.position.x,item.position.z);var centreOffset=Quaternion.Euler(0,item.yaw,0)*Vector3.Scale(new Vector3(bounds.center.x,0,bounds.center.z),node.transform.localScale);
    node.transform.localPosition=at-centreOffset+Vector3.up*(item.position.y-bounds.min.y*node.transform.localScale.y-.2f);
   }else{node.transform.localScale=Vector3.one*1.1f;node.transform.localPosition=Ground(island,terrain,item.position.x,item.position.z);}
   node.transform.localRotation=Quaternion.Euler(0,item.yaw,0);
 }
 public static void AddSurfacePatch(GameObject island,GolfCourse course,GolfSandPatch patch,bool rough=false){
  var terrain=island.GetComponentsInChildren<MeshCollider>(true).Where(c=>c.name.StartsWith("Terrain__")).ToArray();
  var material=rough?Surface("Circuit obstacle rough","Turf",new Color(.53f,.72f,.40f,.65f)):AssetDatabase.LoadAssetAtPath<Material>(Folder+"Materials/Circuit sand.mat");
  float Mask(float x,float z){if(rough&&(MobileIslandGrass.GolfSand(x,z)>.1f||course.sandPatches.Any(s=>s.Contains(x,z))))return 0;float a=patch.yaw*Mathf.Deg2Rad,dx=x-patch.centre.x,dz=z-patch.centre.z;float u=(dx*Mathf.Cos(a)+dz*Mathf.Sin(a))/patch.radii.x,v=(-dx*Mathf.Sin(a)+dz*Mathf.Cos(a))/patch.radii.y;return Mathf.Clamp01((1-Mathf.Sqrt(u*u+v*v))*10);}
  float r=Mathf.Max(patch.radii.x,patch.radii.y)+2;
  Patch(island,course,patch.name,new Vector2(patch.centre.x-r,patch.centre.z-r),new Vector2(patch.centre.x+r,patch.centre.z+r),Mask,material,terrain);
 }

}
