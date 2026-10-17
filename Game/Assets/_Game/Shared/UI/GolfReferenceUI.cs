using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Original approved UI artwork, with native live text above it.
 public static class GolfReferenceUI {
  public static Image Plate(RectTransform parent,string name,bool sliced=true){
   var rect=GolfHUDStyle.Rect("Reference "+name,parent,new Vector2(.5f,.5f),Vector2.zero,parent.sizeDelta);rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
   var image=rect.gameObject.AddComponent<Image>();image.sprite=Resources.Load<Sprite>("GolfUI/Reference/"+name);image.type=sliced?Image.Type.Sliced:Image.Type.Simple;image.raycastTarget=false;return image;
  }
  public static Image Icon(Transform parent,string name,Vector2 size,Vector2 at){return GolfHUDStyle.Art(parent,"Reference "+name,"GolfUI/Reference/"+name,size,at);}
  public static Image Caption(RectTransform parent,string name,Vector2 sourceSize,Vector2 patchSize,Vector2 offset){
   var image=Icon(parent,name+"Caption",patchSize,offset);image.preserveAspect=false;var placement=image.gameObject.AddComponent<GolfReferenceCaption>();placement.parent=parent;placement.sourceSize=sourceSize;placement.patchSize=patchSize;placement.offset=offset;return image;
  }
 }
 [DefaultExecutionOrder(500)]
 public sealed class GolfReferenceCaption:MonoBehaviour {
  public RectTransform parent;public Vector2 sourceSize,patchSize,offset;Vector2 lastSize;
  void LateUpdate(){if(!parent||parent.rect.size==lastSize)return;lastSize=parent.rect.size;var scale=new Vector2(lastSize.x/sourceSize.x,lastSize.y/sourceSize.y);var rect=(RectTransform)transform;rect.sizeDelta=Vector2.Scale(patchSize,scale);rect.anchoredPosition=Vector2.Scale(offset,scale);}
 }
}
