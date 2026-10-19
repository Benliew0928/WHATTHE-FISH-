using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // A loose-to-taut line pictogram, using the world rope's colour scale.
 public sealed class FishingTensionVisual:MaskableGraphic {
  RectTransform page;FishingRodMotion rod;Athlete owner;float tension;
  public static Color ColorFor(float value){value=Mathf.Clamp01(value);return value<.55f?Color.Lerp(CoveUI.Mint,CoveUI.Gold,Mathf.InverseLerp(.2f,.55f,value)):Color.Lerp(CoveUI.Gold,CoveUI.Hex("FF554C"),Mathf.InverseLerp(.55f,.9f,value));}
  public static FishingTensionVisual Create(RectTransform page){
   var r=BasketballUIStyle.Center(page,"Rope strain cue",new Vector2(112,78),Vector2.zero,Vector2.one*.5f);var visual=r.gameObject.AddComponent<FishingTensionVisual>();visual.page=page;visual.raycastTarget=false;
   r.gameObject.SetActive(false);return visual;
  }
  public bool Visible=>gameObject.activeSelf;
  public bool FitsSafeFrame(){if(!Visible)return true;var corners=new Vector3[4];rectTransform.GetWorldCorners(corners);foreach(var corner in corners){var p=page.InverseTransformPoint(corner);if(p.x<page.rect.xMin-1||p.x>page.rect.xMax+1||p.y<page.rect.yMin-1||p.y>page.rect.yMax+1)return false;}return true;}
  public void Tick(){
   var game=FishingGame.Instance;var view=PlayerView.Instance;var p=game&&view?game.Player(view.target):null;
   bool show=game&&game.Context&&view&&view.active&&p.HasValue&&p.Value.phase==FishingPhase.Reeling&&!(FishingHUD.Instance&&FishingHUD.Instance.BlocksGameplay);
   if(owner!=(view?view.target:null)||!rod){owner=view?view.target:null;rod=owner?owner.GetComponent<FishingRodMotion>():null;}
   var camera=view?view.FishingCamera:null;show&=rod&&rod.LineVisible&&camera;
   Vector3 screen=show?camera.WorldToScreenPoint(rod.LineGaugeAnchor):Vector3.zero;show&=screen.z>0;
   gameObject.SetActive(show);if(!show)return;
   tension=Mathf.Clamp01(p.Value.tension);
   RectTransformUtility.ScreenPointToLocalPointInRectangle(page,screen,null,out var at);
   float scale=FishingHUD.Instance?FishingHUD.Instance.HudScale:1;rectTransform.localScale=Vector3.one*scale;
   at+=Vector2.right*74*scale;var bounds=page.rect;var half=rectTransform.sizeDelta*.5f*scale;
   at.x=Mathf.Clamp(at.x,bounds.xMin+half.x+12,bounds.xMax-half.x-12);
   var layout=FishingHUD.Instance?FishingHUD.Instance.Settings:null;float bottom=layout?(layout.gaugeSize.y+layout.margin)*scale:12,top=layout?(layout.clockSize.y+layout.margin)*scale:12;
   at.y=Mathf.Clamp(at.y,bounds.yMin+half.y+bottom,bounds.yMax-half.y-top);
   rectTransform.anchoredPosition=at;SetVerticesDirty();
  }
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();var centre=rectTransform.rect.center;float halfSpan=Mathf.Lerp(23,40,tension),sag=18*(1-Mathf.SmoothStep(0,1,tension));
   float danger=Mathf.InverseLerp(.72f,1,tension),vibration=MenuPreferences.ReducedMotion?0:Mathf.Sin(Time.unscaledTime*22)*danger*1.4f;
   var ink=CoveUI.Ink;var tint=ColorFor(tension);
   Stroke(v,centre,halfSpan,sag,vibration,CoveUI.Cream,9);Stroke(v,centre,halfSpan,sag,vibration,ink,7);Stroke(v,centre,halfSpan,sag,vibration,tint,4);
   for(int side=-1;side<=1;side+=2){var pin=centre+Vector2.right*halfSpan*side;Disc(v,pin,8,CoveUI.Cream);Disc(v,pin,6,ink);Disc(v,pin,3.5f,tint);}
   if(tension>=.72f){
    // A steady warning survives reduced motion; only the taut line trembles.
    var warning=centre+Vector2.up*17;BasketballUIMesh.Line(v,warning,warning+Vector2.up*11,CoveUI.Cream,8);BasketballUIMesh.Line(v,warning,warning+Vector2.up*11,tint,4);
    Disc(v,centre+Vector2.up*9,4.5f,CoveUI.Cream);Disc(v,centre+Vector2.up*9,2.5f,tint);
   }
  }
  static void Stroke(VertexHelper v,Vector2 centre,float halfSpan,float sag,float vibration,Color tint,float width){
   var previous=centre+new Vector2(-halfSpan,0);
   for(int i=1;i<=24;i++){float t=i/24f;var point=centre+new Vector2(Mathf.Lerp(-halfSpan,halfSpan,t),-4*t*(1-t)*sag+Mathf.Sin(t*Mathf.PI)*vibration);BasketballUIMesh.Line(v,previous,point,tint,width);previous=point;}
  }
  static void Disc(VertexHelper v,Vector2 centre,float radius,Color tint)=>CovePlate.Round(v,new Rect(centre-Vector2.one*radius,Vector2.one*radius*2),radius,tint);
 }
}
