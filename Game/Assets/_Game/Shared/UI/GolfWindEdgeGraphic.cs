using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 // Only the outer screen border is tinted; the playing surface stays clear.
 public sealed class GolfWindEdgeGraphic:Graphic {
  public void Strength(float alpha)=>Strength(alpha,new Color(.58f,.68f,.77f));
  public void Strength(float alpha,Color tint){tint.a=alpha;if(color!=tint)color=tint;}
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();var r=rectTransform.rect;float width=Mathf.Min(r.width,r.height)*.16f;
   var a=new Vector2(r.xMin,r.yMin);var b=new Vector2(r.xMax,r.yMin);var c=new Vector2(r.xMax,r.yMax);var d=new Vector2(r.xMin,r.yMax);
   var ia=a+Vector2.one*width;var ib=b+new Vector2(-width,width);var ic=c-Vector2.one*width;var id=d+new Vector2(width,-width);
   Quad(v,a,b,ib,ia);Quad(v,b,c,ic,ib);Quad(v,c,d,id,ic);Quad(v,d,a,ia,id);
  }
  void Quad(VertexHelper v,Vector2 a,Vector2 b,Vector2 c,Vector2 d){int at=v.currentVertCount;var clear=color;clear.a=0;v.AddVert(a,color,Vector2.zero);v.AddVert(b,color,Vector2.zero);v.AddVert(c,clear,Vector2.zero);v.AddVert(d,clear,Vector2.zero);v.AddTriangle(at,at+1,at+2);v.AddTriangle(at,at+2,at+3);}
 }
}
