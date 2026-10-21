using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // The supplied atlas stays unmodified. Replace only its baked seconds with a
 // neighbouring empty gradient strip, then overlay a live countdown label.
 public sealed class GolfEventBadgeGraphic:MaskableGraphic {
  Texture2D sheet;int badge;
  // Canvas caches the bound texture separately from the vertex UVs. Changing
  // atlas must invalidate both, or event UVs sample the previous warning sheet.
  public Texture2D Sheet {get=>sheet;set{if(sheet==value)return;sheet=value;SetAllDirty();}}
  public int Badge {get=>badge;set{if(badge==value)return;badge=value;SetVerticesDirty();}}
  static readonly Rect[] Bounds={new(14,105,734,261),new(746,108,709,258),new(1454,125,701,245),new(297,375,823,303),new(1120,410,795,245),new(0,0,1983,793)};
  static readonly Rect[] Times={new(609,228,91,56),new(1313,229,91,56),new(2029,230,91,56),new(938,512,92,59),new(1781,526,91,56),new(930,278,670,295)};
  static readonly float[] Empty={610,1321,2032,935,1785,910};
  public static Rect Crop(int index)=>Bounds[index];
  public static Rect TimeBox(int index)=>Times[index];
  public override Texture mainTexture=>Sheet?Sheet:Texture2D.whiteTexture;
  protected override void OnPopulateMesh(VertexHelper vh){
   vh.Clear();if(!Sheet)return;var crop=Bounds[Badge];var time=Times[Badge];
   void Quad(float x0,float y0,float x1,float y1,float u0,float v0,float u1,float v1){
    if(x1<=x0||y1<=y0)return;int n=vh.currentVertCount;var r=rectTransform.rect;
    void V(float x,float y,float u,float v){var p=UIVertex.simpleVert;p.color=color;p.position=new Vector2(r.xMin+(x-crop.x)/crop.width*r.width,r.yMax-(y-crop.y)/crop.height*r.height);p.uv0=new Vector2(u/Sheet.width,1-v/Sheet.height);vh.AddVert(p);}
    V(x0,y0,u0,v0);V(x1,y0,u1,v0);V(x1,y1,u1,v1);V(x0,y1,u0,v1);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
   }
   if(Badge==5){
    const float split=1440,lower=368;
    Quad(0,0,crop.width,time.yMin,0,0,crop.width,time.yMin);
    Quad(0,time.yMax,crop.width,crop.height,0,time.yMax,crop.width,crop.height);
    Quad(0,time.yMin,time.xMin,time.yMax,0,time.yMin,time.xMin,time.yMax);
    Quad(time.xMax,time.yMin,crop.width,time.yMax,time.xMax,time.yMin,crop.width,time.yMax);
    Quad(time.xMin,time.yMin,split,time.yMax,910,time.yMin,910.1f,time.yMax);
    Quad(split,time.yMin,time.xMax,lower,split,time.yMin,time.xMax,lower);
    Quad(split,lower,time.xMax,time.yMax,910,lower,910.1f,time.yMax);return;
   }
   Quad(crop.xMin,crop.yMin,crop.xMax,time.yMin,crop.xMin,crop.yMin,crop.xMax,time.yMin);
   Quad(crop.xMin,time.yMax,crop.xMax,crop.yMax,crop.xMin,time.yMax,crop.xMax,crop.yMax);
   Quad(crop.xMin,time.yMin,time.xMin,time.yMax,crop.xMin,time.yMin,time.xMin,time.yMax);
   Quad(time.xMax,time.yMin,crop.xMax,time.yMax,time.xMax,time.yMin,crop.xMax,time.yMax);
   Quad(time.xMin,time.yMin,time.xMax,time.yMax,Empty[Badge],time.yMin,Empty[Badge]+.1f,time.yMax);
  }
 }
}
