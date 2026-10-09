using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Local presentation only: the authoritative Bite deadline always controls Hook.
 public sealed class FishingHookCue:MaskableGraphic {
  readonly struct CueKey:IEquatable<CueKey> {
   readonly ulong owner;readonly uint round,sequence;
   public CueKey(FishingPlayerRecord p,uint r){owner=p.owner;round=r;sequence=p.sequence;}
   public bool Equals(CueKey other)=>owner==other.owner&&round==other.round&&sequence==other.sequence;
   public override bool Equals(object other)=>other is CueKey key&&Equals(key);
   public override int GetHashCode(){unchecked{return (owner.GetHashCode()*397^(int)round)*397^(int)sequence;}}
  }
  static readonly Color Ink=LocalProfile.Hex("143B50"),Gold=LocalProfile.Hex("FFCB53"),Cream=LocalProfile.Hex("FFF3D7");
  readonly HashSet<CueKey> seen=new();readonly List<RectTransform> protectedWidgets=new();readonly List<Rect> obstacles=new();
  readonly Vector3[] corners=new Vector3[4];
  RectTransform frame,action;FishingHUDLayout settings;FishingStickerGraphic actionGraphic;CanvasGroup group;
  CueKey activeKey;bool hasKey,ripplesVisible,geometryFits=true;Rect safe,markBounds,rippleBounds;Vector2 buttonCentre,worldCentre,markCentre;
  float scale=1,markSize,rippleSize,padding,buttonRadius,age;
  bool ReducedMotion=>settings.hookCueReducedMotion||MenuPreferences.ReducedMotion;
  public bool Visible {get;private set;}
  public bool WorldCueVisible {get;private set;}
  public int CueStarts {get;private set;}
  public float Remaining01 {get;private set;}
  public bool DecorationsIgnoreRaycast {
   get {if(raycastTarget||!group||group.blocksRaycasts||group.interactable)return false;foreach(var g in GetComponentsInChildren<Graphic>(true))if(g.raycastTarget)return false;return true;}
  }
  public static FishingHookCue Create(RectTransform frame,RectTransform action,FishingHUDLayout settings){
   var node=new GameObject("Fishing hook alert",typeof(RectTransform),typeof(CanvasGroup));var rect=(RectTransform)node.transform;rect.SetParent(frame,false);
   rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;rect.SetAsFirstSibling();
   var cue=node.AddComponent<FishingHookCue>();cue.frame=frame;cue.action=action;cue.settings=settings;cue.raycastTarget=false;
   cue.actionGraphic=action.GetComponent<FishingStickerGraphic>();cue.group=node.GetComponent<CanvasGroup>();cue.group.blocksRaycasts=false;cue.group.interactable=false;
   // Cache layout groups, then inspect their live bounds after each responsive layout.
   foreach(Transform child in frame){var widget=child as RectTransform;if(!widget||widget==rect)continue;
    foreach(var graphic in widget.GetComponentsInChildren<Graphic>(true))if(graphic.raycastTarget){cue.protectedWidgets.Add(widget);break;}
   }
   cue.Clear();return cue;
  }
  public void Tick(FishingPlayerRecord? player,uint round,double now,bool eligible,Camera camera,Vector3 worldPoint){
   if(!isActiveAndEnabled||!frame||!action||!settings||!eligible||!player.HasValue||!player.Value.connected||player.Value.phase!=FishingPhase.Bite||!double.IsFinite(now)||!double.IsFinite(player.Value.hookUntil)||now>=player.Value.hookUntil){Clear();return;}
   var p=player.Value;var key=new CueKey(p,round);
   if(!hasKey||!activeKey.Equals(key)){
    Clear();if(!seen.Add(key))return;activeKey=key;hasKey=true;Visible=true;CueStarts++;
   }else if(!Visible)return; // Hook, focus loss or an ended cue cannot replay from an old snapshot.
   Remaining01=Mathf.Clamp01((float)((p.hookUntil-now)/FishingState.HookSeconds));
   age=(float)Math.Max(0,now-(p.hookUntil-FishingState.HookSeconds));
   safe=rectTransform.rect;var button=Bounds(action);
   scale=Mathf.Min(button.width/Mathf.Max(1,action.rect.width),button.height/Mathf.Max(1,action.rect.height));
   padding=Mathf.Max(0,settings.hookCuePadding)*scale;rippleSize=Mathf.Max(1,settings.hookCueRippleSize)*scale;
   float pop=ReducedMotion?1:Mathf.Lerp(.84f,1,Mathf.SmoothStep(0,1,Mathf.Clamp01(age/Mathf.Max(.05f,settings.hookCuePopDuration))));
   markSize=Mathf.Max(1,settings.hookCueMarkSize)*scale*pop;
   buttonCentre=button.center;
   float edgeRoom=Mathf.Min(Mathf.Min(buttonCentre.x-safe.xMin,safe.xMax-buttonCentre.x),Mathf.Min(buttonCentre.y-safe.yMin,safe.yMax-buttonCentre.y));
   buttonRadius=Mathf.Max(0,Mathf.Min(Mathf.Min(button.width,button.height)*.5f+padding,edgeRoom-4*scale));
   // Only the illustrated Image child breathes; button bounds, label and pointer target remain fixed.
   float breath=ReducedMotion?1:1-Mathf.Clamp(settings.hookCuePulse,0,.15f)*(.5f-.5f*Mathf.Cos(age*16));
   if(actionGraphic)actionGraphic.PulseIllustration(breath);
   ReadObstacles();WorldCueVisible=ProjectFloat(camera,worldPoint,out worldCentre);
   float bounce=ReducedMotion?0:Mathf.Sin(age*14)*markSize*.045f;
   markCentre=worldCentre+Vector2.up*(Mathf.Max(1,settings.hookCueMarkSize)*scale*.72f+padding+bounce);
   markBounds=Box(markCentre,new Vector2(markSize*.5f,markSize));
   WorldCueVisible&=Fits(markBounds)&&!Blocked(markBounds);
   rippleBounds=Box(worldCentre,new Vector2(rippleSize+5*scale,rippleSize*.42f+5*scale));
   ripplesVisible=WorldCueVisible&&Fits(rippleBounds)&&!Blocked(rippleBounds);
   group.alpha=1;geometryFits=true;SetVerticesDirty();
  }
  public void Clear(){
   Visible=WorldCueVisible=ripplesVisible=false;Remaining01=0;geometryFits=true;
   if(group)group.alpha=0;if(actionGraphic)actionGraphic.PulseIllustration(1);SetVerticesDirty();
  }
  // Host rooms, actors and uint token wrap may legitimately reuse a round or cast identity.
  public void ResetHistory(){Clear();seen.Clear();hasKey=false;}
  protected override void OnDisable(){Clear();base.OnDisable();}
  protected override void OnDestroy(){Clear();base.OnDestroy();}
  void OnApplicationFocus(bool focused){if(!focused)Clear();}
  void OnApplicationPause(bool paused){if(paused)Clear();}
  public bool FitsSafeFrame(){
   if(!Visible)return true;
   return geometryFits&&Fits(Box(buttonCentre,Vector2.one*(buttonRadius+4*scale)*2))&&(!WorldCueVisible||Fits(markBounds))&&(!ripplesVisible||Fits(rippleBounds));
  }
  Rect Bounds(RectTransform widget){widget.GetWorldCorners(corners);Vector2 min=new(float.MaxValue,float.MaxValue),max=new(float.MinValue,float.MinValue);
   foreach(var corner in corners){var p=(Vector2)rectTransform.InverseTransformPoint(corner);min=Vector2.Min(min,p);max=Vector2.Max(max,p);}return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
  }
  void ReadObstacles(){
   obstacles.Clear();foreach(var widget in protectedWidgets){if(!widget||!widget.gameObject.activeInHierarchy)continue;var bounds=Bounds(widget);
    // The transparent Look pad is an input surface, not visible control artwork.
    if(bounds.width>=safe.width*.95f&&bounds.height>=safe.height*.95f)continue;
    bounds.xMin-=padding;bounds.xMax+=padding;bounds.yMin-=padding;bounds.yMax+=padding;obstacles.Add(bounds);
   }
  }
  bool ProjectFloat(Camera camera,Vector3 worldPoint,out Vector2 position){
   position=Vector2.zero;if(!camera||!float.IsFinite(worldPoint.x)||!float.IsFinite(worldPoint.y)||!float.IsFinite(worldPoint.z))return false;
   var viewport=camera.WorldToViewportPoint(worldPoint);if(viewport.z<=camera.nearClipPlane||viewport.x<0||viewport.x>1||viewport.y<0||viewport.y>1)return false;
   // Convert projection immediately to local canvas units; keep no screen-pixel UI positions.
   var point=camera.WorldToScreenPoint(worldPoint);var uiCamera=canvas&&canvas.renderMode!=RenderMode.ScreenSpaceOverlay?canvas.worldCamera:null;
   return RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,point,uiCamera,out position);
  }
  static Rect Box(Vector2 centre,Vector2 size)=>new(centre-size*.5f,size);
  bool Fits(Rect bounds)=>bounds.xMin>=safe.xMin-.1f&&bounds.xMax<=safe.xMax+.1f&&bounds.yMin>=safe.yMin-.1f&&bounds.yMax<=safe.yMax+.1f;
  bool Blocked(Rect bounds){foreach(var obstacle in obstacles)if(obstacle.Overlaps(bounds))return true;return false;}
  protected override void OnPopulateMesh(VertexHelper vh){
   vh.Clear();if(!Visible)return;
   Arc(vh,buttonCentre,Vector2.one*buttonRadius,Remaining01,7*scale,Ink);
   Arc(vh,buttonCentre,Vector2.one*buttonRadius,Remaining01,4.5f*scale,Gold);
   Arc(vh,buttonCentre,Vector2.one*(buttonRadius+2*scale),Remaining01,1.2f*scale,Cream);
   if(!WorldCueVisible)return;
   if(ripplesVisible)for(int i=0;i<2;i++){
    float phase=ReducedMotion?.35f:Mathf.Repeat(age*1.8f+i*.5f,1);
    float radius=rippleSize*(.27f+.23f*phase);var radii=new Vector2(radius,radius*.42f);float alpha=(1-phase)*.8f;
    var ink=Ink;ink.a=alpha;var gold=Gold;gold.a=alpha;var cream=Cream;cream.a=alpha;
    Arc(vh,worldCentre,radii,1,5*scale,ink);Arc(vh,worldCentre,radii,1,2.8f*scale,gold);Arc(vh,worldCentre,radii+Vector2.one*1.2f*scale,1,.9f*scale,cream);
   }
   // A crisp illustrated exclamation mark needs no font weight, texture or asset import.
   var stem=markCentre+Vector2.up*markSize*.16f;var dot=markCentre-Vector2.up*markSize*.35f;
   Capsule(vh,stem,new Vector2(markSize*.25f,markSize*.57f),Ink);
   Capsule(vh,stem,new Vector2(markSize*.21f,markSize*.53f),Cream);
   Capsule(vh,stem,new Vector2(markSize*.15f,markSize*.48f),Gold);
   Capsule(vh,stem+new Vector2(-.035f,.08f)*markSize,new Vector2(markSize*.035f,markSize*.25f),Cream);
   Disc(vh,dot,markSize*.115f,Ink);Disc(vh,dot,markSize*.091f,Cream);Disc(vh,dot,markSize*.075f,Gold);
  }
  void Arc(VertexHelper vh,Vector2 centre,Vector2 radii,float fraction,float width,Color tint){
   if(fraction<=0||radii.x<=0||radii.y<=0)return;const int segments=64;int count=Mathf.Max(1,Mathf.CeilToInt(segments*fraction));
   float half=width*.5f;int first=vh.currentVertCount;
   for(int i=0;i<=count;i++){float angle=(90-360*fraction*i/count)*Mathf.Deg2Rad;var direction=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
    Add(vh,centre+Vector2.Scale(direction,radii+Vector2.one*half),tint);Add(vh,centre+Vector2.Scale(direction,Vector2.Max(Vector2.zero,radii-Vector2.one*half)),tint);
    if(i>0){int a=first+(i-1)*2;vh.AddTriangle(a,a+2,a+1);vh.AddTriangle(a+1,a+2,a+3);}
   }
  }
  void Capsule(VertexHelper vh,Vector2 centre,Vector2 size,Color tint){
   var r=Box(centre,size);float radius=Mathf.Min(size.x,size.y)*.5f;int first=vh.currentVertCount;Add(vh,centre,tint);const int segments=6;
   for(int corner=0;corner<4;corner++)for(int i=0;i<=segments;i++){
    float angle=(corner*90+i*90f/segments)*Mathf.Deg2Rad;var c=new Vector2(corner==0||corner==3?r.xMax-radius:r.xMin+radius,corner<2?r.yMax-radius:r.yMin+radius);
    Add(vh,c+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius,tint);
   }
   const int count=4*(segments+1);for(int i=0;i<count;i++)vh.AddTriangle(first,first+1+i,first+1+(i+1)%count);
  }
  void Disc(VertexHelper vh,Vector2 centre,float radius,Color tint){
   const int count=24;int first=vh.currentVertCount;Add(vh,centre,tint);
   for(int i=0;i<count;i++){float angle=i*Mathf.PI*2/count;Add(vh,centre+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius,tint);}
   for(int i=0;i<count;i++)vh.AddTriangle(first,first+1+i,first+1+(i+1)%count);
  }
  void Add(VertexHelper vh,Vector2 point,Color tint){geometryFits&=point.x>=safe.xMin-.1f&&point.x<=safe.xMax+.1f&&point.y>=safe.yMin-.1f&&point.y<=safe.yMax+.1f;vh.AddVert(point,tint,Vector2.zero);}
 }
}
