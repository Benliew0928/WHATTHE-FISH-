using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Independent cosmetic clock, synchronized to the unchanged host deadline.
 public sealed class GolfFinalCountdown:MonoBehaviour {
  Sprite blank,reference;
  RectTransform page,root;Image plate;Image[] digits=new Image[3];Sprite[] glyphs=new Sprite[11];GolfCountdownFace clock;
  double deadline=double.NaN;int previous=-1;float tickAt=-100,introAge,introAt;
  bool wasVisible;public int Seconds {get;private set;}
  public const float CornerScale=.54f,CornerDigitSize=50;
  public int DigitRefreshes {get;private set;}
  public RectTransform Card=>root;
  public bool Visible=>root&&root.gameObject.activeInHierarchy;
  public float ClockAngle=>clock?clock.Angle:0;
  public static GolfFinalCountdown Create(RectTransform parent){
   var node=new GameObject("Golf final countdown animation");node.transform.SetParent(parent,false);var value=node.AddComponent<GolfFinalCountdown>();value.Build(parent);return value;
  }
  void Build(RectTransform parent){
   page=parent;root=GolfHUDStyle.Rect("Golf finish in badge",parent,new Vector2(.5f,.5f),Vector2.zero,new Vector2(760,323));
   blank=Resources.Load<Sprite>("GolfUI/FinalCountdown/Plate");reference=Resources.Load<Sprite>("GolfUI/FinalCountdown/Reference");plate=root.gameObject.AddComponent<Image>();plate.sprite=blank;plate.raycastTarget=false;plate.preserveAspect=true;
   for(int i=0;i<glyphs.Length;i++)glyphs[i]=Resources.Load<Sprite>("GolfUI/FinalCountdown/Glyph"+i);
   for(int i=0;i<3;i++){var r=GolfHUDStyle.Rect("Golf remaining digit "+i,root,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);digits[i]=r.gameObject.AddComponent<Image>();digits[i].raycastTarget=false;digits[i].preserveAspect=true;}
   // The original clock face pixels sit over the identically positioned face
   // of the blank plate. Only this dial wiggles; plate and heading stay steady.
   var face=GolfHUDStyle.Rect("Golf ticking clock face",root,new Vector2(.5f,.5f),new Vector2(-185, -27),new Vector2(156,156));
   clock=face.gameObject.AddComponent<GolfCountdownFace>();clock.Source=reference;clock.raycastTarget=false;
   // Keep transform animation on its own small UI batch. Reordering the badge
   // against the leaderboard each frame rebuilt the entire shared HUD canvas.
   var canvas=root.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=100;
   root.gameObject.SetActive(false);
  }
  public void Show(bool visible,double remaining,double end,double now){
   if(root.gameObject.activeSelf!=visible)root.gameObject.SetActive(visible);
   if(!visible){wasVisible=false;deadline=double.NaN;previous=-1;return;}
   if(!wasVisible||deadline!=end){
    wasVisible=true;deadline=end;previous=-1;
    // Initialize against the host's deadline, then animate on the render clock.
    // Physics/server timestamps may advance in steps, especially on guests.
    introAge=Mathf.Max(0,(float)(now-(end-GolfMatchState.FinalCountdownSeconds)));introAt=Time.unscaledTime;
   }
   Seconds=Mathf.Clamp(Mathf.CeilToInt((float)remaining),0,30);
   if(Seconds!=previous){previous=Seconds;tickAt=Time.unscaledTime;RefreshDigits();}
   float age=introAge+Time.unscaledTime-introAt;
   // Only presentation uses the continuous clock. The host still owns expiry.
   float t=Mathf.SmoothStep(0,1,Mathf.Clamp01((age-.5f)/.8f));
   float initial=Mathf.Min(1,(page.rect.width-40)/760),small=Mathf.Min(CornerScale,(page.rect.width*.43f-24)/760);
   float events=GolfEventHUD.Instance?GolfEventHUD.Instance.StackHeight:0;
   var destination=new Vector2(page.rect.xMin+20+760*small*.5f,page.rect.yMax-20-events-323*small*.5f);
   root.anchoredPosition=Vector2.Lerp(Vector2.zero,destination,t);root.localScale=Vector3.one*Mathf.Lerp(initial,small,t);
   float tick=Time.unscaledTime-tickAt;
   float angle=MenuPreferences.ReducedMotion?0:tick<.32f?Mathf.Sin(tick*55)*6*(1-tick/.32f):0;
   if(Mathf.Abs(clock.Angle-angle)>.001f){clock.Angle=angle;clock.SetVerticesDirty();}
  }
  void RefreshDigits(){
   DigitRefreshes++;plate.sprite=blank;bool two=Seconds>=10;
   digits[0].gameObject.SetActive(two);digits[0].sprite=glyphs[Seconds/10];digits[1].sprite=glyphs[Seconds%10];digits[2].sprite=glyphs[10];
   // At the normal .54 corner scale each numeral occupies exactly 50 x 50
   // reference UI units; shrink s by the same factor as the numerals.
   float digit=CornerDigitSize/CornerScale,factor=digit/142;
   digits[0].rectTransform.anchoredPosition=new Vector2(61,-58);digits[0].rectTransform.sizeDelta=Vector2.one*digit;
   digits[1].rectTransform.anchoredPosition=new Vector2(two?156:110,-58);digits[1].rectTransform.sizeDelta=Vector2.one*digit;
   digits[2].rectTransform.anchoredPosition=new Vector2(two?240:202,-77);digits[2].rectTransform.sizeDelta=new Vector2(91,100)*factor;
  }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public void ReviewAge(float age){introAge=age;introAt=Time.unscaledTime;}
  public Vector2 RenderedDigitSize=>digits[1].rectTransform.sizeDelta*root.localScale.x;
  public Vector2 RenderedSecondsSize=>digits[2].rectTransform.sizeDelta*root.localScale.x;
#endif
  void OnDestroy(){if(root)Destroy(root.gameObject);}
 }
 public sealed class GolfCountdownFace:MaskableGraphic {
  public Sprite Source;public float Angle;
  public override Texture mainTexture=>Source?Source.texture:Texture2D.whiteTexture;
  protected override void OnPopulateMesh(VertexHelper vh){
   vh.Clear();if(!Source)return;var rect=rectTransform.rect;var centre=rect.center;
   Vector2 sample=new(492f/1923,1-476f/817);Vector2 radius=new(194f/1923,194f/817);
   var v=UIVertex.simpleVert;v.color=Color.white;v.position=centre;v.uv0=sample;vh.AddVert(v);
   for(int i=0;i<=64;i++){
    float a=i*Mathf.PI*2/64,source=a-Angle*Mathf.Deg2Rad;var d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
    v.position=centre+d*rect.size*.5f;v.uv0=sample+new Vector2(Mathf.Cos(source),Mathf.Sin(source))*radius;vh.AddVert(v);if(i>0)vh.AddTriangle(0,i,i+1);
   }
  }
 }
}
