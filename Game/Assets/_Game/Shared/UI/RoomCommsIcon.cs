using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public enum RoomCommsSymbol {Chat,Microphone,Speaker}
 // Resolution-independent icons keep the phone controls crisp without texture assets.
 public sealed class RoomCommsIcon:MaskableGraphic {
  public RoomCommsSymbol Symbol;public bool Slashed;
  public void Set(RoomCommsSymbol symbol,bool slashed,Color tint){if(Symbol==symbol&&Slashed==slashed&&color==tint)return;Symbol=symbol;Slashed=slashed;color=tint;SetVerticesDirty();}
  Vector2 P(float x,float y){var r=rectTransform.rect;return new Vector2(r.xMin+x*r.width,r.yMax-y*r.height);}
  void Line(VertexHelper v,float x,float y,float xx,float yy,float weight=.065f){BasketballUIMesh.Line(v,P(x,y),P(xx,yy),color,rectTransform.rect.width*weight);}
  void Arc(VertexHelper v,float x,float y,float rx,float ry,float from,float to){for(int i=0;i<20;i++){float a=Mathf.Lerp(from,to,i/20f)*Mathf.Deg2Rad,b=Mathf.Lerp(from,to,(i+1)/20f)*Mathf.Deg2Rad;Line(v,x+Mathf.Cos(a)*rx,y+Mathf.Sin(a)*ry,x+Mathf.Cos(b)*rx,y+Mathf.Sin(b)*ry);}}
  void Polygon(VertexHelper v,params Vector2[] points){int start=v.currentVertCount;foreach(var p in points)v.AddVert(P(p.x,p.y),color,Vector2.zero);for(int i=1;i<points.Length-1;i++)v.AddTriangle(start,start+i,start+i+1);}
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();
   if(Symbol==RoomCommsSymbol.Chat){
    Line(v,.23f,.16f,.77f,.16f);Arc(v,.77f,.29f,.13f,.13f,-90,0);Line(v,.9f,.29f,.9f,.61f);Arc(v,.77f,.61f,.13f,.13f,0,90);
    Line(v,.77f,.74f,.43f,.74f);Line(v,.43f,.74f,.21f,.9f);Line(v,.21f,.9f,.24f,.74f);Arc(v,.23f,.61f,.13f,.13f,90,180);Line(v,.1f,.61f,.1f,.29f);Arc(v,.23f,.29f,.13f,.13f,180,270);
    foreach(float x in new[]{.31f,.5f,.69f})BasketballUIMesh.Dot(v,P(x,.45f),rectTransform.rect.width*.035f,color);
   }else if(Symbol==RoomCommsSymbol.Microphone){
    var a=P(.36f,.12f);var b=P(.64f,.6f);CovePlate.Round(v,new Rect(a.x,b.y,b.x-a.x,a.y-b.y),rectTransform.rect.width*.14f,color);
    Line(v,.22f,.4f,.22f,.47f);Line(v,.78f,.4f,.78f,.47f);Arc(v,.5f,.47f,.28f,.27f,0,180);Line(v,.5f,.74f,.5f,.89f);Line(v,.33f,.89f,.67f,.89f);
   }else{
    Polygon(v,new Vector2(.12f,.35f),new Vector2(.31f,.35f),new Vector2(.57f,.15f),new Vector2(.57f,.85f),new Vector2(.31f,.65f),new Vector2(.12f,.65f));
    if(!Slashed){Arc(v,.55f,.5f,.22f,.26f,-55,55);Arc(v,.55f,.5f,.37f,.4f,-55,55);}
   }
   if(Slashed){BasketballUIMesh.Line(v,P(.14f,.12f),P(.88f,.88f),CoveUI.Hex("102B38"),rectTransform.rect.width*.15f);Line(v,.14f,.12f,.88f,.88f,.075f);}
  }
 }
}
