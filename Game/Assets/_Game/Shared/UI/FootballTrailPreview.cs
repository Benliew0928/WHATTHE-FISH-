using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 // Lightweight animated swatch using the same shader as the in-world trail.
 public sealed class FootballTrailPreview:MaskableGraphic {
  public int Style;public bool Basketball;
  protected override void OnEnable(){base.OnEnable();raycastTarget=false;}
  void Update(){var next=Basketball?BasketballTrailStyle.Material(Style):FootballTrailStyle.Material(Style);if(material!=next)material=next;}
  protected override void OnPopulateMesh(VertexHelper vh){
   vh.Clear();var r=rectTransform.rect;
   for(int i=0;i<=32;i++){
    float u=i/32f,centre=r.center.y+Mathf.Sin(u*5)*r.height*.13f;
    float width=r.height*.37f*Mathf.Lerp(.15f,1,Mathf.Sqrt(u));
    var c=new Color(1,1,1,Mathf.SmoothStep(0,1,u));
    vh.AddVert(new Vector3(r.xMin+u*r.width,centre-width),c,new Vector2(1-u,0));
    vh.AddVert(new Vector3(r.xMin+u*r.width,centre+width),c,new Vector2(1-u,1));
    if(i>0){int v=i*2;vh.AddTriangle(v-2,v-1,v);vh.AddTriangle(v-1,v+1,v);}
   }
  }
 }
}
