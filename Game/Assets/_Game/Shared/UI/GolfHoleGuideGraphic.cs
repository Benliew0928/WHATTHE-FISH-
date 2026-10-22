using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 // Upright number with a separate directional tip. Screen-size geometry stays
 // legible at a distant cup, without another bitmap or a touch target.
 public sealed class GolfHoleGuideGraphic:Graphic {
  public float direction;
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();var r=rectTransform.rect;float radius=Mathf.Min(r.width*.46f,r.height*.36f);var centre=new Vector2(0,8);
   Disc(v,centre+Vector2.down*3,radius+2,new Color(.03f,.11f,.16f,.4f));
   Disc(v,centre,radius,GolfHUDStyle.Ink);Disc(v,centre,radius-3,Color.white);Disc(v,centre,radius-6,GolfHUDStyle.Gold);
   var q=Quaternion.Euler(0,0,direction);Vector2 a=q*new Vector3(-11,-radius+7),b=q*new Vector3(11,-radius+7),c=q*new Vector3(0,-radius-13);
   Triangle(v,centre+a,centre+b,centre+c,GolfHUDStyle.Ink);Triangle(v,centre+a*.78f,centre+b*.78f,centre+c*.85f,GolfHUDStyle.Gold);
  }
  static void Triangle(VertexHelper v,Vector2 a,Vector2 b,Vector2 c,Color tint){int at=v.currentVertCount;v.AddVert(a,tint,Vector2.zero);v.AddVert(b,tint,Vector2.zero);v.AddVert(c,tint,Vector2.zero);v.AddTriangle(at,at+1,at+2);}
  static void Disc(VertexHelper v,Vector2 p,float radius,Color tint){int at=v.currentVertCount;v.AddVert(p,tint,Vector2.zero);for(int i=0;i<=32;i++){float angle=i*Mathf.PI/16;v.AddVert(p+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius,tint,Vector2.zero);if(i>0)v.AddTriangle(at,at+i,at+i+1);}}
  public void Point(float degrees){if(Mathf.Abs(Mathf.DeltaAngle(direction,degrees))<.1f)return;direction=degrees;SetVerticesDirty();}
 }
}
