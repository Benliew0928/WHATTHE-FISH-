using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class GolfChargeGraphic:Graphic {
  public bool vignette,meterFrame;float amount,opacity;
  public void Show(float progress,float alpha){if(Mathf.Abs(progress-amount)<.0001f&&Mathf.Abs(alpha-opacity)<.0001f)return;amount=progress;opacity=alpha;SetVerticesDirty();}
  protected override void OnPopulateMesh(VertexHelper mesh){
   mesh.Clear();if(opacity<=0)return;var r=rectTransform.rect;
   if(vignette){
    var outer=new[]{new Vector2(r.xMin,r.yMin),new Vector2(r.xMax,r.yMin),new Vector2(r.xMax,r.yMax),new Vector2(r.xMin,r.yMax)};
    var inner=new[]{new Vector2(r.xMin+r.width*.13f,r.yMin+r.height*.16f),new Vector2(r.xMax-r.width*.13f,r.yMin+r.height*.16f),new Vector2(r.xMax-r.width*.13f,r.yMax-r.height*.16f),new Vector2(r.xMin+r.width*.13f,r.yMax-r.height*.16f)};
    for(int i=0;i<4;i++){mesh.AddVert(outer[i],new Color(1,.35f,.4f,opacity),Vector2.zero);mesh.AddVert(inner[i],new Color(1,.45f,.5f,0),Vector2.zero);}
    for(int i=0;i<4;i++){int a=i*2,b=((i+1)%4)*2;mesh.AddTriangle(a,b,a+1);mesh.AddTriangle(a+1,b,b+1);}return;
   }
   if(meterFrame){Rounded(mesh,r,10,new Color(.12f,.68f,.77f,.95f));var inset=new Rect(r.xMin+2,r.yMin+2,r.width-4,r.height-4);Rounded(mesh,inset,8,new Color(.035f,.14f,.2f,.9f));return;}
   var colour=PowerColor(amount);colour.a=opacity;
   float height=r.height*amount;if(height<.1f)return;var filled=new Rect(r.xMin,r.yMin,r.width,height);Rounded(mesh,filled,7,colour);
   Rounded(mesh,new Rect(r.xMin+2,r.yMin+3,Mathf.Min(3,r.width*.2f),Mathf.Max(0,height-6)),1.5f,new Color(1,1,1,.28f));
  }
  static void Rounded(VertexHelper mesh,Rect r,float radius,Color colour){
   if(r.width<=0||r.height<=0)return;radius=Mathf.Min(radius,Mathf.Min(r.width,r.height)*.5f);int start=mesh.currentVertCount;mesh.AddVert(r.center,colour,Vector2.zero);
   for(int c=0;c<4;c++){var center=new Vector2(c<2?r.xMax-radius:r.xMin+radius,c==0||c==3?r.yMin+radius:r.yMax-radius);for(int j=0;j<=6;j++){float a=(-90+c*90+j*15)*Mathf.Deg2Rad;mesh.AddVert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,colour,Vector2.zero);}}
   int count=28;for(int i=0;i<count;i++)mesh.AddTriangle(start,start+1+(i+1)%count,start+1+i);
  }
  public static Color PowerColor(float power)=>Color.Lerp(new Color(1,.86f,0),Color.red,Mathf.InverseLerp(.75f,1,power));
 }
}
