using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Small, editable vector lettering. Uses the Canvas UI material; no font/texture asset.
 [RequireComponent(typeof(CanvasRenderer))]
 public sealed class FootballTeamAWordmark : MaskableGraphic {
  sealed class Letter {
   public readonly float x;
   public readonly Vector2[] edge, counter;
   public readonly int[] triangles;
   public readonly Vector2[][] marks;
   public Letter(float x, Vector2[] edge, Vector2[][] marks, Vector2[] counter = null, int[] triangles = null) {
    this.x=x; this.edge=edge; this.counter=counter; this.marks=marks;
    this.triangles=triangles??Triangulate(edge);
   }
  }
  static Vector2[] Points(params float[] xy) {
   var result=new Vector2[xy.Length/2];
   for(int i=0;i<result.Length;i++)result[i]=new Vector2(xy[i*2],xy[i*2+1]);
   return result;
  }
  static Letter A(float x) => new Letter(x,
   Points(0,0, 27,92, 49,94, 78,0, 55,0, 48,25, 27,25, 20,0),
   new[]{Points(13,28, 17,40, 21,43, 17,28), Points(24,64, 28,78, 31,80, 28,65),
    Points(57,24, 54,37, 58,40, 61,24), Points(31,30, 38,34, 40,31, 34,28)},
   Points(31,42, 44,42, 38,66),
   new[]{0,1,8, 0,8,6, 0,6,7, 1,2,10, 1,10,8, 2,3,9, 2,9,10,
    3,4,5, 3,5,9, 5,6,8, 5,8,9});
  static readonly Letter[] Letters={
   new Letter(0,Points(0,98, 86,96, 80,74, 54,76, 38,0, 14,0, 29,76, 0,78),
    new[]{Points(13,82, 20,94, 25,94, 18,81), Points(33,82, 39,89, 44,89, 38,81),
     Points(60,83, 66,93, 70,92, 65,83), Points(29,32, 38,66, 42,68, 34,32),
     Points(24,13, 27,21, 31,23, 29,14)}),
   new Letter(47,Points(0,0, 16,89, 73,89, 68,69, 35,70, 32,52, 60,53, 56,34, 28,34, 25,19, 62,18, 58,0),
    new[]{Points(27,75, 31,83, 36,84, 33,75), Points(46,76, 50,83, 54,83, 51,76),
     Points(15,29, 19,44, 23,46, 19,29), Points(35,41, 40,48, 44,47, 40,41),
     Points(33,6, 38,13, 43,13, 38,6)}),
   A(97),
   new Letter(160,Points(0,0, 2,93, 25,93, 44,49, 60,94, 84,96, 86,0, 63,0, 63,54, 47,23, 35,23, 22,53, 23,0),
    new[]{Points(8,18, 9,38, 13,42, 13,20), Points(10,65, 11,83, 15,85, 15,68),
     Points(34,52, 29,68, 33,69, 38,52), Points(54,54, 60,71, 64,73, 59,56),
     Points(71,16, 70,33, 74,37, 75,18), Points(70,72, 70,85, 74,86, 74,74)}),
   A(277)
  };
  static readonly Color Edge=new Color32(242,139,108,255),Ink=new Color32(195,37,16,255),
   Low=new Color32(239,105,6,255),High=new Color32(255,150,27,255),Shade=new Color32(93,31,28,225);
  Vector2 origin;
  float unit;

  protected override void OnPopulateMesh(VertexHelper mesh) {
   mesh.Clear();
   var rect=GetPixelAdjustedRect();unit=Mathf.Min(rect.width/363f,rect.height/108f);
   origin=rect.center-new Vector2(180,48)*unit;
   // Canvas triangles paint in order: the left letter is always in front of its neighbour.
   for(int i=Letters.Length-1;i>=0;i--) {
    var letter=Letters[i];
    Face(mesh,letter,Shade,new Vector2(1.8f,-2.2f),false);
    Border(mesh,letter,letter.edge,7,Shade,new Vector2(1.8f,-2.2f));
    if(letter.counter!=null)Border(mesh,letter,letter.counter,7,Shade,new Vector2(1.8f,-2.2f));
    Border(mesh,letter,letter.edge,4,Edge,Vector2.zero);
    if(letter.counter!=null)Border(mesh,letter,letter.counter,4,Edge,Vector2.zero);
    Face(mesh,letter,Color.white,Vector2.zero,true);
    foreach(var mark in letter.marks) {
     int start=mesh.currentVertCount;
     foreach(var point in mark)Vertex(mesh,letter,point,Ink,Vector2.zero);
     mesh.AddTriangle(start,start+1,start+2);mesh.AddTriangle(start,start+2,start+3);
    }
   }
  }
  void Face(VertexHelper mesh,Letter letter,Color tint,Vector2 offset,bool gradient) {
   int start=mesh.currentVertCount;
   foreach(var point in letter.edge)Vertex(mesh,letter,point,gradient?Color.Lerp(Low,High,point.y/98):tint,offset);
   if(letter.counter!=null)foreach(var point in letter.counter)Vertex(mesh,letter,point,gradient?Color.Lerp(Low,High,point.y/98):tint,offset);
   for(int i=0;i<letter.triangles.Length;i+=3)mesh.AddTriangle(start+letter.triangles[i],start+letter.triangles[i+1],start+letter.triangles[i+2]);
  }
  void Border(VertexHelper mesh,Letter letter,Vector2[] path,float width,Color tint,Vector2 offset) {
   int start=mesh.currentVertCount;
   for(int i=0;i<path.Length;i++) {
    var before=(path[i]-path[(i+path.Length-1)%path.Length]).normalized;
    var after=(path[(i+1)%path.Length]-path[i]).normalized;
    var normal=new Vector2(-after.y,after.x);
    var miter=(new Vector2(-before.y,before.x)+normal).normalized;
    var spread=miter*Mathf.Min(width*2,width*.5f/Mathf.Max(.25f,Vector2.Dot(miter,normal)));
    Vertex(mesh,letter,path[i]+spread,tint,offset);Vertex(mesh,letter,path[i]-spread,tint,offset);
   }
   for(int i=0;i<path.Length;i++) {
    int a=start+i*2,b=start+((i+1)%path.Length)*2;
    mesh.AddTriangle(a,b,a+1);mesh.AddTriangle(a+1,b,b+1);
   }
  }
  void Vertex(VertexHelper mesh,Letter letter,Vector2 point,Color tint,Vector2 offset) {
   // Shared forward lean preserves the deliberately unequal, hand-drawn letter widths.
   var p=origin+(new Vector2(letter.x+point.x+point.y*.18f,point.y)+offset)*unit;
   mesh.AddVert(p,tint*color,Vector2.zero);
  }
  static float Cross(Vector2 a,Vector2 b) => a.x*b.y-a.y*b.x;
  static int[] Triangulate(Vector2[] path) {
   var remaining=new List<int>();var triangles=new List<int>();
   float area=0;for(int i=0;i<path.Length;i++)area+=Cross(path[i],path[(i+1)%path.Length]);
   for(int i=0;i<path.Length;i++)remaining.Add(area>0?i:path.Length-1-i);
   while(remaining.Count>2) {
    bool clipped=false;
    for(int i=0;i<remaining.Count;i++) {
     int a=remaining[(i+remaining.Count-1)%remaining.Count],b=remaining[i],c=remaining[(i+1)%remaining.Count];
     if(Cross(path[b]-path[a],path[c]-path[b])<=0)continue;
     bool occupied=false;
     foreach(int p in remaining) {
      if(p==a||p==b||p==c)continue;
      if(Cross(path[b]-path[a],path[p]-path[a])>=0&&Cross(path[c]-path[b],path[p]-path[b])>=0&&Cross(path[a]-path[c],path[p]-path[c])>=0){occupied=true;break;}
     }
     if(occupied)continue;
     triangles.Add(a);triangles.Add(b);triangles.Add(c);remaining.RemoveAt(i);clipped=true;break;
    }
    if(!clipped)throw new System.InvalidOperationException("Team A lettering has an invalid contour.");
   }
   return triangles.ToArray();
  }
 }
}
