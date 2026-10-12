using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Procedural, non-interactive marker: no texture, material or font dependency.
 [DefaultExecutionOrder(220)]
 public sealed class StadiumPlayerIndicator:MaskableGraphic {
  RectTransform page;Text label;Graphic[] obstacles;readonly Vector3[] corners=new Vector3[4];
  Vector2 direction;bool showing;int lastDistance=-1;
  public bool Showing=>showing;
  public Vector2 Direction=>direction;
  public static void Create(RectTransform page,Font font){
   var r=new GameObject("Your player edge marker",typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(page,false);
   r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.sizeDelta=new Vector2(104,76);
   var marker=r.gameObject.AddComponent<StadiumPlayerIndicator>();marker.page=page;marker.raycastTarget=false;
   var text=new GameObject("YOU and distance",typeof(RectTransform)).AddComponent<Text>();text.transform.SetParent(r,false);
   text.rectTransform.anchorMin=Vector2.zero;text.rectTransform.anchorMax=Vector2.one;text.rectTransform.offsetMin=text.rectTransform.offsetMax=Vector2.zero;
   text.font=font;text.fontSize=18;text.fontStyle=FontStyle.Normal;text.alignment=TextAnchor.MiddleCenter;text.color=Color.white;text.raycastTarget=false;
   marker.label=text;marker.SetShown(false);
  }
  void SetShown(bool value){showing=value;canvasRenderer.cull=!value;if(label)label.enabled=value;SetVerticesDirty();}
  // Camera-space bearing does not reverse twice for targets behind the lens.
  public static Vector2 Bearing(Camera camera,Vector3 world){
   var local=camera.transform.InverseTransformPoint(world);
   var bearing=new Vector2(local.x,local.y);
   return bearing.sqrMagnitude<.00001f?Vector2.down:bearing.normalized;
  }
  protected override void OnDisable(){base.OnDisable();SetShown(false);}
  void LateUpdate(){
   var view=PlayerView.Instance;
   if(!view||view.InputBlocked||!view.StadiumFollowing||!view.target){if(showing)SetShown(false);return;}
   // HUD skinning runs after Create; cache the finished graphics on first use.
   if(obstacles==null)obstacles=page.GetComponentsInChildren<Graphic>(true);
   var camera=view.GetComponent<Camera>();var world=view.target.transform.position+Vector3.up*.9f;
   var screen=camera.WorldToScreenPoint(world);var safe=Screen.safeArea;
   // Small re-entry margin prevents flicker while the player crosses an edge.
   var visible=safe;if(showing){visible.xMin+=24;visible.xMax-=24;visible.yMin+=24;visible.yMax-=24;}
   bool outside=screen.z<=camera.nearClipPlane||!visible.Contains(new Vector2(screen.x,screen.y));
   if(!outside){if(showing)SetShown(false);return;}
   if(!showing)SetShown(true);
   direction=Bearing(camera,world);
   var bounds=page.rect;bounds.xMin+=72;bounds.xMax-=72;bounds.yMin+=58;bounds.yMax-=58;
   var center=bounds.center;
   float scale=Mathf.Min(bounds.width*.5f/Mathf.Max(.0001f,Mathf.Abs(direction.x)),bounds.height*.5f/Mathf.Max(.0001f,Mathf.Abs(direction.y)));
   var point=center+direction*scale;point.x=Mathf.Clamp(point.x,bounds.xMin,bounds.xMax);point.y=Mathf.Clamp(point.y,bounds.yMin,bounds.yMax);
   bool vertical=Mathf.Abs(point.x-center.x)>=bounds.width*.5f-.1f;
   // Stay on the same edge and move only enough to avoid existing HUD cards.
   // Read current rectangles so tablet layouts and hidden controls also work.
   if(OverlapsHud(point))for(int i=1;i<=32;i++){
    float offset=((i+1)/2)*48*(i%2==1?1:-1);
    var candidate=point+(vertical?Vector2.up:Vector2.right)*offset;
    if(candidate.x<bounds.xMin||candidate.x>bounds.xMax||candidate.y<bounds.yMin||candidate.y>bounds.yMax)continue;
    if(!OverlapsHud(candidate)){point=candidate;break;}
   }
   rectTransform.anchoredPosition=point-page.rect.center;
   var ball=AppRoot.Instance.SelectedSport==SportId.Football?FootballBall.Instance.transform:BasketballBall.Active.transform;
   int metres=Mathf.RoundToInt(Vector3.ProjectOnPlane(view.target.transform.position-ball.position,Vector3.up).magnitude);
   if(metres!=lastDistance){label.text="YOU\n"+metres+" m";lastDistance=metres;}
   canvasRenderer.cull=false;SetVerticesDirty();
  }
  bool OverlapsHud(Vector2 point){
   var marker=new Rect(point-new Vector2(59,43),new Vector2(118,86));
   foreach(var graphic in obstacles){
    if(!graphic||graphic==this||graphic==label||graphic is Text||!graphic.isActiveAndEnabled||graphic.color.a<.1f)continue;
    var group=graphic.GetComponentInParent<CanvasGroup>();if(group&&group.alpha<.01f)continue;
    var r=graphic.rectTransform;if(r.IsChildOf(transform))continue;
    r.GetWorldCorners(corners);var min=(Vector2)page.InverseTransformPoint(corners[0]);var max=(Vector2)page.InverseTransformPoint(corners[2]);
    if(max.x-min.x>page.rect.width*.8f||max.y-min.y>page.rect.height*.8f)continue;
    if(marker.Overlaps(Rect.MinMaxRect(min.x,min.y,max.x,max.y)))return true;
   }
   return false;
  }
  protected override void OnPopulateMesh(VertexHelper mesh){
   mesh.Clear();if(!showing)return;
   CovePlate.Round(mesh,new Rect(-44,-28,88,56),14,new Color(.015f,.08f,.14f,.94f));
   var d=direction;var side=new Vector2(-d.y,d.x);
   float radius=1/Mathf.Max(Mathf.Abs(d.x)/47,Mathf.Abs(d.y)/32);
   var tip=d*(radius+14);var foot=d*radius;int n=mesh.currentVertCount;
   var cyan=new Color(.22f,.87f,1,1);
   mesh.AddVert(tip,cyan,Vector2.zero);mesh.AddVert(foot+side*9,cyan,Vector2.zero);mesh.AddVert(foot-side*9,cyan,Vector2.zero);mesh.AddTriangle(n,n+1,n+2);
  }
 }
}
