using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class GolfWarningGradient:BaseMeshEffect {
  public override void ModifyMesh(VertexHelper vh){if(!IsActive()||vh.currentVertCount==0)return;float lo=float.MaxValue,hi=float.MinValue;var v=new UIVertex();for(int i=0;i<vh.currentVertCount;i++){vh.PopulateUIVertex(ref v,i);lo=Mathf.Min(lo,v.position.y);hi=Mathf.Max(hi,v.position.y);}for(int i=vh.currentVertCount*4/5;i<vh.currentVertCount;i++){vh.PopulateUIVertex(ref v,i);if(v.color.r<245||v.color.g<245||v.color.b<245)continue;var c=Color.Lerp(new Color(1,.08f,0),new Color(1,.91f,.05f),Mathf.InverseLerp(lo,hi,v.position.y));c.a=v.color.a/255f;v.color=c;vh.SetUIVertex(v,i);}}
 }
}
