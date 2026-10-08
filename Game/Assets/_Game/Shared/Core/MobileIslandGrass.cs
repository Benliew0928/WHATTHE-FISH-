using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhatTheFish {
 // Prepared while the destination is inactive. Pure geometry math runs on a
 // worker; Unity mesh upload is spread over frames before the ready signal.
 public sealed class MobileIslandGrass : MonoBehaviour {
  public MobileGrassLibrary library;
  public Material leaf,lightLeaf;
  public string Error {get;private set;}
  public bool Ready {get;private set;}
  readonly List<Mesh> owned=new();
  CancellationTokenSource cancellation;
  GameObject generatedRoot;
  sealed class Geometry {
   public readonly List<Vector3> vertices=new();
   public readonly List<Vector2> uv=new();
   public readonly List<int>[] triangles={new(),new()};
   public void Add(Vector3[] points,bool light){
    int start=vertices.Count;vertices.AddRange(points);
    // Metre-scaled grain follows the vertical blade plane.
    Vector3 normal=Vector3.Cross(points[1]-points[0],points[2]-points[0]);
    foreach(var p in points)uv.Add(new Vector2(Math.Abs(normal.x)>Math.Abs(normal.z)?p.z:p.x,p.y)*.25f);
    var t=triangles[light?1:0];t.Add(start);t.Add(start+2);t.Add(start+1);t.Add(start);t.Add(start+3);t.Add(start+2);t.Add(start+3);t.Add(start+4);t.Add(start+2);
   }
  }
  sealed class Cell {public string name;public Geometry[] lod={new(),new(),new()};}
  public IEnumerator Prepare(){
   if(Ready)yield break;Error=null;
   var operation=PrepareCore();
   while(true){
    bool next=false;
    try{next=operation.MoveNext();}catch(Exception e){Error=e.Message;Debug.LogException(e);}
    if(!next)break;
    yield return operation.Current;
   }
   if(!Ready){
    cancellation?.Cancel();cancellation?.Dispose();cancellation=null;
    foreach(var mesh in owned)if(mesh)Destroy(mesh);owned.Clear();
    if(generatedRoot)Destroy(generatedRoot);
   }
  }
  IEnumerator PrepareCore(){
   var environment=GetComponent<RefinedIslandEnvironment>();
   var coast=GetComponent<CoastalEnvironment>();
   if((!environment&&!coast)||!library||library.patches.Length!=3){Error="Missing mobile grass source";yield break;}
   bool golf=environment&&environment.layout.sport=="Golf";
   string sport=environment?environment.layout.sport:coast.basketball?"Basketball":"Football";
   var placements=environment?environment.layout.instances.Where(p=>p.module.StartsWith("Grass_")).ToArray():Array.Empty<IslandPlacement>();
   var course=golf?GetComponentInChildren<GolfCourse>(true):null;
   // Pure island-local data; never access scene objects from the worker.
   var greens=course?(GolfGreen[])course.greens.Clone():Array.Empty<GolfGreen>();
   var coastal=coast?library.coastal.Where(p=>p.sport==sport).ToArray():null;
   var patches=library.patches;cancellation=new CancellationTokenSource();var token=cancellation.Token;
   var clock=System.Diagnostics.Stopwatch.StartNew();
   var task=Task.Run(()=>coastal!=null?GenerateCoast(coastal,token):Generate(placements,patches,golf,greens,token),token);
   while(!task.IsCompleted)yield return null;
   if(task.IsCanceled){Error="Grass preparation cancelled";yield break;}
   if(task.IsFaulted){Error=task.Exception.GetBaseException().Message;yield break;}
   var root=generatedRoot=new GameObject("Shared-patch grass · generated for active island");root.transform.SetParent(transform,false);
   int count=0;
   foreach(var cell in task.Result){
    var node=new GameObject(cell.name);node.transform.SetParent(root.transform,false);
    var lods=new LOD[3];float[] thresholds={.30f,.16f,.08f};
    for(int level=0;level<3;level++){
     var data=cell.lod[level];var go=new GameObject("LOD"+level);go.transform.SetParent(node.transform,false);go.layer=10;
     var mesh=new Mesh{name="Runtime grass "+cell.name+" LOD"+level,indexFormat=data.vertices.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};
     mesh.SetVertices(data.vertices);mesh.SetUVs(0,data.uv);mesh.subMeshCount=2;
     for(int sub=0;sub<2;sub++)mesh.SetTriangles(data.triangles[sub],sub,false);
     mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();mesh.UploadMeshData(true);owned.Add(mesh);
     go.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterials=new[]{leaf,lightLeaf};renderer.shadowCastingMode=ShadowCastingMode.Off;
     lods[level]=new LOD(thresholds[level],new Renderer[]{renderer});cell.lod[level]=null;
     // Never upload all cells in a single arrival frame.
     yield return null;
    }
    var group=node.AddComponent<LODGroup>();group.SetLODs(lods);group.RecalculateBounds();count++;
   }
   cancellation.Dispose();cancellation=null;Ready=true;
   Debug.Log($"MOBILE_GRASS_READY {sport} cells={count} meshes={owned.Count} preparationMs={clock.ElapsedMilliseconds}");
  }
  void OnDestroy(){cancellation?.Cancel();cancellation?.Dispose();foreach(var mesh in owned)if(mesh)Destroy(mesh);}
  static List<Cell> GenerateCoast(MobileGrassLibrary.CoastalPatch[] patches,CancellationToken token){
   var cells=new Dictionary<string,Cell>();var points=new Vector3[5];int blade=0;
   foreach(var patch in patches){
    token.ThrowIfCancellationRequested();if(!cells.TryGetValue(patch.cell,out var cell)){cell=new Cell{name=patch.cell};cells.Add(patch.cell,cell);}
    for(int k=0;k<7;k++){
     float angle=k*2.4f,h=patch.heights[k];var d=new Vector3((float)Math.Cos(angle),0,(float)Math.Sin(angle));var side=new Vector3(-d.z,0,d.x);var p=patch.position;
     points[0]=p-side*.035f;points[1]=p+side*.035f;points[2]=p+d*h*.23f+Vector3.up*(h*.55f)+side*.026f;points[3]=p+d*h*.23f+Vector3.up*(h*.55f)-side*.026f;points[4]=p+d*h*.56f+Vector3.up*h;
     cell.lod[0].Add(points,k%3==0);if(blade%3==0)cell.lod[1].Add(points,k%3==0);if(blade%8==0)cell.lod[2].Add(points,k%3==0);blade++;
    }
   }
   return cells.Values.ToList();
  }
  static List<Cell> Generate(IslandPlacement[] placements,MobileGrassLibrary.Patch[] patches,bool golf,GolfGreen[] greens,CancellationToken token){
   var cells=new Dictionary<(int,int),Cell>();int bladeNumber=0;float extent=golf?220:70;var points=new Vector3[5];
   foreach(var placement in placements){
    token.ThrowIfCancellationRequested();
    double angle=-placement.yaw*Math.PI/180;float cos=(float)Math.Cos(angle),sin=(float)Math.Sin(angle);
    var cellKey=((int)((placement.position.x+extent)/40),(int)((placement.position.z+extent)/40));
    if(!cells.TryGetValue(cellKey,out var cell)){cell=new Cell{name=$"Cell_{cellKey.Item1:00}_{cellKey.Item2:00}"};cells.Add(cellKey,cell);}
    int variant=placement.module[placement.module.Length-1]-'0';
    foreach(var blade in patches[variant].blades){
     points[0]=blade.a;points[1]=blade.b;points[2]=blade.c;points[3]=blade.d;points[4]=blade.e;bool allowed=true;
     for(int i=0;i<points.Length;i++){
      var p=points[i];float x=placement.position.x+placement.scale*(p.x*cos-p.z*sin),z=placement.position.z+placement.scale*(p.x*sin+p.z*cos);
      if(golf)foreach(var bunker in Bunkers)if(BunkerDistance(x,z,bunker)<=1.02){allowed=false;break;}
      float ground=(float)Height(x,z,golf);
      foreach(var green in greens){if(green.ClearsGrass(x,z))allowed=false;ground=green.Height(x,z,ground);}
      points[i]=new Vector3(x,ground+p.y*placement.scale-.01f,z);
     }
     if(!allowed)continue;
     cell.lod[0].Add(points,blade.light);
     if(bladeNumber%3==0)cell.lod[1].Add(points,blade.light);
     if(bladeNumber%8==0)cell.lod[2].Add(points,blade.light);
     bladeNumber++;
    }
   }
   return cells.Values.Where(c=>c.lod[0].vertices.Count>0).ToList();
  }
  static readonly double[][] Bunkers={new[]{-42d,-112,18,11,.3},new[]{43d,-56,24,14,-.3},new[]{-38d,16,21,13,.2},new[]{38d,74,18,12,.5},new[]{-37d,127,15,10,-.5}};
  static double Square(double a)=>a*a;
  static double Smooth(double a,double b,double x){double t=Math.Max(0,Math.Min(1,(x-a)/(b-a)));return t*t*(3-2*t);}
  static double BunkerDistance(double x,double y,double[] b){double u=(x-b[0])*Math.Cos(b[4])+(y-b[1])*Math.Sin(b[4]),v=-(x-b[0])*Math.Sin(b[4])+(y-b[1])*Math.Cos(b[4]);return Math.Sqrt(Square(u/b[2])+Square(v/b[3]))*(1+.08*Math.Sin(Math.Atan2(v/b[3],u/b[2])*3+.6));}
  public static float GolfSand(float x,float z){double distance=double.MaxValue;foreach(var b in Bunkers)distance=Math.Min(distance,BunkerDistance(x,z,b));return (float)(1-Smooth(.9,1.08,distance));}
  // Same analytic surfaces as build_refined_islands.py; no extra height textures.
  public static double Height(double x,double y,bool golf){
   double r=Math.Sqrt(x*x+y*y),a=Math.Atan2(golf?y:x,golf?x:y);
   if(golf){
    double radius=192+12*Math.Sin(3*a+.5)+7*Math.Sin(5*a-1)+4*Math.Cos(9*a)-9*Math.Exp(-Square(Math.Atan2(Math.Sin(a-.45),Math.Cos(a-.45))/.28));
    double q=r/radius,baseHeight=3.6+1.4*Math.Sin(y/70)+.75*Math.Cos(x/42)*Math.Sin(y/36);
    baseHeight+=6.8*Math.Exp(-Square((x-4)/57)-Square((y-131)/49))+2*Math.Exp(-Square((x+24)/50)-Square((y+144)/38));
    baseHeight+=1.9*Math.Sin(x/26+.3)*Math.Sin(y/31)*Smooth(35,70,Math.Abs(x-(24*Math.Sin((y+135)/55)-5)));
    foreach(var b in Bunkers)baseHeight-=1.15*(1-Smooth(.55,1.18,BunkerDistance(x,y,b)));
    double d=Math.Sqrt(Square((x+65)/10)+Square((y+145)/8));baseHeight=baseHeight*Smooth(1,2,d)+4.2*(1-Smooth(1,2,d));
    return baseHeight*(1-Smooth(.82,.98,q))+.18*Smooth(.82,.98,q)-.8*Smooth(.98,1.035,q)+6.5*Math.Exp(-Square((x-151)/20)-Square((y-73)/16))*(1-Smooth(.94,1.01,q));
   }
   double edge=55+2.4*Math.Sin(3*a+.4)+1.5*Math.Cos(7*a);
   double rise=1.45+1.8*Math.Exp(-Square((r-46)/5))*(.65+.35*Square(Math.Sin(a*5+.8)));
   double z=1.2+(rise-1.2)*Smooth(38,41,r);
   z=z*(1-Smooth(edge-5,edge+.7,r))-.6*Smooth(edge-1,edge+.7,r);
   double blend=1-Smooth(.9,1.4,Math.Sqrt(Square((x-41)/6.2)+Square((y+14)/6.3)));z=z*(1-blend)+1.2*blend;
   return z*Smooth(26,28,r)-.6*(1-Smooth(26,28,r));
  }
 }
}
