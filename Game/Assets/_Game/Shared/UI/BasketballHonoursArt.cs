using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 // Lightweight basketball seams and court markings behind the shared character art.
 public sealed class BasketballHonoursArt:MaskableGraphic {
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();var c=rectTransform.rect.center;float r=rectTransform.rect.width*.46f;var orange=new Color(1,.47f,.14f,.88f);var paper=new Color(1,.97f,.86f,.80f);
   BasketballUIMesh.Dot(v,c,r,orange);
   void Line(Vector2 a,Vector2 b,float w=5)=>BasketballUIMesh.Line(v,c+a,c+b,paper,w);
   Line(new Vector2(-r,0),new Vector2(r,0));Line(new Vector2(0,-r),new Vector2(0,r));
   for(int i=0;i<64;i++){float a=-Mathf.PI*.5f+Mathf.PI*i/64,b=-Mathf.PI*.5f+Mathf.PI*(i+1)/64;foreach(float sign in new[]{-1f,1f})Line(new Vector2(sign*r*(.62f-.34f*Mathf.Cos(a)),r*Mathf.Sin(a)),new Vector2(sign*r*(.62f-.34f*Mathf.Cos(b)),r*Mathf.Sin(b)),4);}
   for(int i=0;i<6;i++){float y=-r+20+i*24;Line(new Vector2(-r-5,y),new Vector2(-r+65,y+24),3);}
  }
 }
}
