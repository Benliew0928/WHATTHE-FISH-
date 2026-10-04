using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Small native UI geometry; no extra bitmap or shader is shipped.
 public sealed class GolfArrowGraphic:Graphic {
  protected override void OnPopulateMesh(VertexHelper mesh){
   mesh.Clear();var r=rectTransform.rect;
   var points=new[]{new Vector2(.38f,0),new Vector2(.62f,0),new Vector2(.62f,.55f),new Vector2(1,.55f),new Vector2(.5f,1),new Vector2(0,.55f),new Vector2(.38f,.55f),new Vector2(.5f,.55f)};
   foreach(var p in points)mesh.AddVert(new Vector3(r.xMin+p.x*r.width,r.yMin+p.y*r.height),color,Vector2.zero);
   for(int i=0;i<7;i++)mesh.AddTriangle(7,i,(i+1)%7);
  }
 }
}
