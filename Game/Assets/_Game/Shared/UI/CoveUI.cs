using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public static class CoveUI {
  public static readonly Color Ink=Hex("123F50"), Muted=Hex("50757B"), Cream=Hex("FFF9E9"),
   Coral=Hex("FF9273"), Mint=Hex("A7E8CC"), Sky=Hex("BAEDF4"), Gold=Hex("FFD47D"), Line=Hex("C4DAD5");
  static Font textFont,displayFont; static Sprite[] islands;
  public static bool KeyboardNavigation;
  public static Color Hex(string value)=>LocalProfile.Hex(value);
  public static Font TextFont=>textFont?textFont:textFont=Resources.Load<Font>("Menu/CoveText");
  public static Font DisplayFont=>displayFont?displayFont:displayFont=Resources.Load<Font>("Menu/CoveDisplay");
  // Coordinates use a 1600 x 900 design canvas with a top-left origin.
  public static RectTransform Rect(Transform parent,string name,float x,float y,float w,float h) {
   var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
   r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
  }
  public static void Stretch(RectTransform r) {r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
  public static Text Text(Transform parent,string value,float x,float y,float w,float h,int size=25,bool bold=false,Color? color=null,TextAnchor align=TextAnchor.MiddleLeft) {
   var t=Rect(parent,value.Length>28?value.Substring(0,28):value,x,y,w,h).gameObject.AddComponent<Text>();
   t.font=bold?DisplayFont:TextFont;t.fontSize=size;t.color=color??Ink;t.text=value;t.alignment=align;
   t.supportRichText=false;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;
  }
  public static CovePlate Plate(Transform parent,string name,float x,float y,float w,float h,Color color,float radius=24,float border=2,bool shadow=false) {
   var p=Rect(parent,name,x,y,w,h).gameObject.AddComponent<CovePlate>();p.color=color;p.radius=radius;p.border=border;p.shadow=shadow;p.raycastTarget=false;return p;
  }
  public static CoveButton Button(Transform parent,string id,string label,float x,float y,float w,float h,Color color,Action action,CoveSymbol icon=CoveSymbol.None,int size=28) {
   var plate=Plate(parent,id,x,y,w,h,color,22,2,true);plate.raycastTarget=true;plate.rectTransform.pivot=new Vector2(.5f,.5f);plate.rectTransform.anchoredPosition=new Vector2(x+w/2,-y-h/2);
   var b=plate.gameObject.AddComponent<CoveButton>();b.targetGraphic=plate;b.transition=Selectable.Transition.None;
   b.onClick.AddListener(()=>action?.Invoke());
   var feedback=b.gameObject.AddComponent<CoveFeedback>();feedback.plate=plate;
   float left=icon==CoveSymbol.None?12:65;
   Text(plate.transform,label,left,2,w-left-12,h-4,size,true,null,icon==CoveSymbol.None?TextAnchor.MiddleCenter:TextAnchor.MiddleLeft);
   if(icon!=CoveSymbol.None)Icon(plate.transform,icon,20,(h-34)/2,34);
   return b;
  }
  public static CoveIcon Icon(Transform parent,CoveSymbol symbol,float x,float y,float size,Color? color=null) {
   var i=Rect(parent,symbol.ToString(),x,y,size,size).gameObject.AddComponent<CoveIcon>();i.symbol=symbol;i.color=color??Ink;i.raycastTarget=false;return i;
  }
  public static Image Image(Transform parent,string name,Sprite sprite,float x,float y,float w,float h) {
   var i=Rect(parent,name,x,y,w,h).gameObject.AddComponent<Image>();i.rectTransform.pivot=new Vector2(.5f,.5f);i.rectTransform.anchoredPosition=new Vector2(x+w/2,-y-h/2);i.sprite=sprite;i.preserveAspect=true;i.raycastTarget=false;return i;
  }
  public static Sprite Island(SportId sport) {
   if(islands==null){
    var t=Resources.Load<Texture2D>("Menu/CoveIslands");islands=new Sprite[4];
    if(t)for(int i=0;i<4;i++)islands[i]=Sprite.Create(t,new Rect(i%2*t.width/2f,(1-i/2)*t.height/2f,t.width/2f,t.height/2f),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
   }
   return islands[(int)sport];
  }
  public static InputField Field(Transform parent,string id,string placeholder,string value,float x,float y,float w,int limit=24) {
   var plate=Plate(parent,id,x,y,w,70,Color.white,18,2);plate.raycastTarget=true;
   var field=plate.gameObject.AddComponent<InputField>();field.targetGraphic=plate;field.transition=Selectable.Transition.None;
   field.textComponent=Text(plate.transform,"",20,4,w-40,62,28,true);field.textComponent.supportRichText=false;
   field.placeholder=Text(plate.transform,placeholder,20,4,w-40,62,25,false,Muted);
   field.characterLimit=limit;field.lineType=InputField.LineType.SingleLine;field.text=value;
   field.selectionColor=new Color(.2f,.7f,.65f,.25f);field.caretColor=Ink;field.customCaretColor=true;
   plate.gameObject.AddComponent<CoveFieldFeedback>().plate=plate;return field;
  }
 }

 public sealed class CovePlate : MaskableGraphic {
  public float radius=24,border=2,pressure; public bool shadow,focus;public Color borderColor=CoveUI.Ink;
  protected override void OnPopulateMesh(VertexHelper vh) {
   vh.Clear();var r=rectTransform.rect;
   if(shadow){var s=r;s.y-=5*(1-pressure);Round(vh,s,radius,new Color(.03f,.24f,.28f,.18f));}
   if(focus){var s=r;s.xMin-=5;s.xMax+=5;s.yMin-=5;s.yMax+=5;Round(vh,s,radius+5,CoveUI.Gold);}
   if(border>0)Round(vh,r,radius,borderColor);
   var inner=r;inner.xMin+=border;inner.xMax-=border;inner.yMin+=border;inner.yMax-=border;Round(vh,inner,Mathf.Max(0,radius-border),color);
  }
  internal static void Round(VertexHelper vh,Rect r,float radius,Color c) {
   if(r.width<=0||r.height<=0)return;radius=Mathf.Min(radius,r.width/2,r.height/2);
   int start=vh.currentVertCount;vh.AddVert(r.center,c,Vector2.zero);
   for(int corner=0;corner<4;corner++){
    var center=new Vector2(corner==0||corner==3?r.xMax-radius:r.xMin+radius,corner<2?r.yMax-radius:r.yMin+radius);
    for(int s=0;s<=8;s++){float a=(corner*90+s*90f/8)*Mathf.Deg2Rad;vh.AddVert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,c,Vector2.zero);}
   }
   for(int i=0;i<36;i++)vh.AddTriangle(start,start+1+i,start+1+(i+1)%36);
  }
 }

 // Button owns click eligibility. Visual feedback never repeats or invokes actions.
 public sealed class CoveButton : Button {
  int pointer=int.MinValue;bool armed,released;float lastSubmit=-10;
  public int Activations {get;private set;}
  protected override void Awake(){base.Awake();onClick.AddListener(()=>{Activations++;CoveSound.Play();});}
  public override void OnPointerDown(PointerEventData e) {
   if(e.button!=PointerEventData.InputButton.Left||pointer!=int.MinValue||!IsInteractable())return;
   CoveUI.KeyboardNavigation=false;pointer=e.pointerId;armed=true;released=false;base.OnPointerDown(e);
  }
  public override void OnPointerUp(PointerEventData e) {
   if(e.pointerId!=pointer)return;
   released=armed&&IsInteractable()&&RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform,e.position,e.pressEventCamera);
   armed=false;pointer=int.MinValue;base.OnPointerUp(e);
  }
  public override void OnPointerExit(PointerEventData e){if(e.pointerId==pointer)armed=false;base.OnPointerExit(e);}
  public override void OnPointerClick(PointerEventData e) {
   if(!released||!IsInteractable())return;released=false;base.OnPointerClick(e);
  }
  public override void OnSubmit(BaseEventData e) {
   if(!IsInteractable()||Time.unscaledTime-lastSubmit<.22f)return;lastSubmit=Time.unscaledTime;
   CoveUI.KeyboardNavigation=true;GetComponent<CoveFeedback>()?.Pulse();onClick.Invoke();
  }
  public void CancelPress(){armed=released=false;pointer=int.MinValue;}
  protected override void OnDisable(){CancelPress();base.OnDisable();}
  void OnApplicationFocus(bool focused){if(!focused)CancelPress();}
  void OnApplicationPause(bool paused){if(paused)CancelPress();}
 }

 public sealed class CoveFeedback : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerEnterHandler,IPointerExitHandler,ISelectHandler,IDeselectHandler,ICancelHandler {
  public float pressScale=.966f,releaseBounce=.026f;
  public CovePlate plate;Selectable control;Transform visual;bool hover,focused,held;int pointer=int.MinValue;Vector3 rest;Color tint;float pulse,bounce,scale=1;
  void Awake(){control=GetComponent<Selectable>();visual=control is Slider slider&&slider.handleRect?slider.handleRect:transform;rest=visual.localScale;if(plate)tint=plate.color;}
  void Start(){if(plate)tint=plate.color;}
  public void UseVisual(Transform target){visual=target;rest=target.localScale;}
  public void Pulse(){pulse=.07f;bounce=.24f;}
  public void SetTint(Color value){tint=value;if(plate)plate.color=control&&control.IsInteractable()?tint:Color.Lerp(tint,CoveUI.Cream,.6f);}
  public void OnPointerDown(PointerEventData e){if(e.button!=PointerEventData.InputButton.Left||!control||!control.IsInteractable()||pointer!=int.MinValue)return;pointer=e.pointerId;held=true;}
  public void OnPointerUp(PointerEventData e){if(e.pointerId!=pointer)return;if(held)bounce=.18f;held=false;pointer=int.MinValue;}
  public void OnPointerEnter(PointerEventData e){hover=true;}
  public void OnPointerExit(PointerEventData e){hover=false;if(e.pointerId==pointer)held=false;}
  public void OnSelect(BaseEventData e){focused=true;}
  public void OnDeselect(BaseEventData e){focused=false;}
  public void OnCancel(BaseEventData e){ResetState();GetComponent<CoveButton>()?.CancelPress();}
  void ResetState(){held=hover=focused=false;pointer=int.MinValue;pulse=bounce=0;scale=1;if(visual)visual.localScale=rest;}
  void OnDisable(){ResetState();}
  void OnApplicationFocus(bool value){if(!value)ResetState();}
  void OnApplicationPause(bool value){if(value)ResetState();}
  void Update(){
   if(!control)return;bool active=control.IsInteractable();if(!active){held=false;pointer=int.MinValue;}
   pulse=Mathf.Max(0,pulse-Time.unscaledDeltaTime);bounce=Mathf.Max(0,bounce-Time.unscaledDeltaTime);
   float target=MenuPreferences.ReducedMotion?1:active?(held||pulse>0?pressScale:bounce>0?1+releaseBounce*Mathf.Sin(Mathf.Clamp01(bounce/.18f)*Mathf.PI):hover?1.018f:1):1;
   scale=Mathf.Lerp(scale,target,1-Mathf.Exp(-24*Time.unscaledDeltaTime));if(visual)visual.localScale=rest*scale;
   if(plate){bool newFocus=active&&focused;float pressure=held?1:0;bool changed=plate.focus!=newFocus||plate.pressure!=pressure;plate.focus=newFocus;plate.pressure=pressure;
    plate.color=!active?Color.Lerp(tint,CoveUI.Cream,.6f):held?Color.Lerp(tint,CoveUI.Ink,.12f):hover?Color.Lerp(tint,Color.white,.13f):tint;
    var border=active?CoveUI.Ink:CoveUI.Line;changed|=border!=plate.borderColor;plate.borderColor=border;if(changed)plate.SetVerticesDirty();}
  }
 }

 public sealed class CoveFieldFeedback : MonoBehaviour,ISelectHandler,IDeselectHandler {
  public CovePlate plate;Vector2 rest;float error;bool focused;
  void Start(){rest=((RectTransform)transform).anchoredPosition;}
  public void OnSelect(BaseEventData e){focused=true;}
  public void OnDeselect(BaseEventData e){focused=false;}
  public void Invalid(){error=.4f;}
  void Update(){
   error=Mathf.Max(0,error-Time.unscaledDeltaTime);plate.focus=focused;
   plate.borderColor=error>0?CoveUI.Hex("C84F42"):focused?CoveUI.Hex("189C99"):CoveUI.Line;plate.SetVerticesDirty();
   ((RectTransform)transform).anchoredPosition=rest+(MenuPreferences.ReducedMotion?Vector2.zero:Vector2.right*Mathf.Sin(error*60)*error*12);
  }
 }

 public enum CoveSymbol {None,People,Join,Map,Paint,Book,Settings,Back,Next,Check,Copy,Close,Sound,Flag,Boat,Star}
 public sealed class CoveIcon : MaskableGraphic {
  public CoveSymbol symbol;
  Vector2 P(float x,float y){var r=rectTransform.rect;return new Vector2(r.xMin+x*r.width,r.yMax-y*r.height);}
  void Line(VertexHelper vh,float x,float y,float xx,float yy,float width=.08f){var a=P(x,y);var b=P(xx,yy);var n=new Vector2(-(b-a).y,(b-a).x).normalized*rectTransform.rect.width*width*.5f;int i=vh.currentVertCount;vh.AddVert(a+n,color,Vector2.zero);vh.AddVert(b+n,color,Vector2.zero);vh.AddVert(b-n,color,Vector2.zero);vh.AddVert(a-n,color,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);}
  void Circle(VertexHelper vh,float x,float y,float radius){var p=P(x,y);CovePlate.Round(vh,new Rect(p.x-radius*rectTransform.rect.width,p.y-radius*rectTransform.rect.height,2*radius*rectTransform.rect.width,2*radius*rectTransform.rect.height),radius*rectTransform.rect.width,color);}
  void Box(VertexHelper vh,float x,float y,float w,float h){Line(vh,x,y,x+w,y);Line(vh,x+w,y,x+w,y+h);Line(vh,x+w,y+h,x,y+h);Line(vh,x,y+h,x,y);}
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();switch(symbol){
    case CoveSymbol.Back:Line(v,.7f,.15f,.3f,.5f);Line(v,.3f,.5f,.7f,.85f);break;
    case CoveSymbol.Next:Line(v,.3f,.15f,.7f,.5f);Line(v,.7f,.5f,.3f,.85f);break;
    case CoveSymbol.Check:Line(v,.12f,.5f,.4f,.78f,.13f);Line(v,.4f,.78f,.88f,.2f,.13f);break;
    case CoveSymbol.Close:Line(v,.2f,.2f,.8f,.8f);Line(v,.2f,.8f,.8f,.2f);break;
    case CoveSymbol.People:Circle(v,.36f,.25f,.18f);Circle(v,.78f,.28f,.14f);Line(v,.13f,.73f,.58f,.73f,.32f);Line(v,.66f,.73f,.9f,.73f,.25f);break;
    case CoveSymbol.Join:Box(v,.45f,.14f,.4f,.72f);Line(v,.05f,.5f,.65f,.5f);Line(v,.43f,.3f,.65f,.5f);Line(v,.43f,.7f,.65f,.5f);break;
    case CoveSymbol.Copy:Box(v,.32f,.25f,.5f,.61f);Line(v,.65f,.12f,.17f,.12f);Line(v,.17f,.12f,.17f,.68f);break;
    case CoveSymbol.Map:case CoveSymbol.Book:
     Line(v,.08f,.22f,.33f,.14f);Line(v,.33f,.14f,.65f,.26f);Line(v,.65f,.26f,.92f,.17f);Line(v,.08f,.22f,.08f,.85f);Line(v,.92f,.17f,.92f,.8f);Line(v,.08f,.85f,.34f,.76f);Line(v,.34f,.76f,.65f,.88f);Line(v,.65f,.88f,.92f,.8f);Line(v,.34f,.14f,.34f,.76f,.04f);Line(v,.65f,.26f,.65f,.88f,.04f);break;
    case CoveSymbol.Paint:Line(v,.4f,.63f,.8f,.15f,.21f);Line(v,.17f,.87f,.38f,.62f,.15f);Circle(v,.2f,.81f,.12f);break;
    case CoveSymbol.Settings:for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Line(v,.5f+Mathf.Cos(a)*.29f,.5f+Mathf.Sin(a)*.29f,.5f+Mathf.Cos(a)*.43f,.5f+Mathf.Sin(a)*.43f,.18f);}for(int i=0;i<20;i++){float a=i*Mathf.PI/10,b=(i+1)*Mathf.PI/10;Line(v,.5f+Mathf.Cos(a)*.25f,.5f+Mathf.Sin(a)*.25f,.5f+Mathf.Cos(b)*.25f,.5f+Mathf.Sin(b)*.25f,.13f);}break;
    case CoveSymbol.Flag:Line(v,.27f,.12f,.27f,.9f);Line(v,.3f,.18f,.82f,.31f,.15f);Line(v,.82f,.31f,.3f,.45f,.15f);break;
    case CoveSymbol.Boat:Box(v,.16f,.33f,.68f,.52f);Line(v,.5f,.1f,.5f,.33f);Line(v,.16f,.56f,.84f,.56f);Line(v,.5f,.33f,.5f,.56f,.04f);break;
    case CoveSymbol.Sound:Box(v,.12f,.35f,.22f,.3f);Line(v,.34f,.35f,.56f,.16f);Line(v,.56f,.16f,.56f,.84f);Line(v,.56f,.84f,.34f,.65f);Line(v,.78f,.3f,.89f,.5f);Line(v,.89f,.5f,.78f,.7f);break;
    case CoveSymbol.Star:for(int i=0;i<10;i++){float a=(-90+i*36)*Mathf.Deg2Rad,b=(-90+(i+1)*36)*Mathf.Deg2Rad;float r=i%2==0?.43f:.2f,rr=i%2==0?.2f:.43f;Line(v,.5f+Mathf.Cos(a)*r,.5f+Mathf.Sin(a)*r,.5f+Mathf.Cos(b)*rr,.5f+Mathf.Sin(b)*rr,.1f);}break;
   }
  }
 }

 // Reuses a few hundred UI vertices instead of shipping a full-screen painting.
 public sealed class CoveSea : MaskableGraphic {
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();var r=rectTransform.rect;int i=v.currentVertCount;
   v.AddVert(new Vector2(r.xMin,r.yMin),CoveUI.Hex("27B6CD"),Vector2.zero);v.AddVert(new Vector2(r.xMax,r.yMin),CoveUI.Hex("27B6CD"),Vector2.zero);
   v.AddVert(new Vector2(r.xMax,r.yMax),CoveUI.Hex("9AE4EC"),Vector2.zero);v.AddVert(new Vector2(r.xMin,r.yMax),CoveUI.Hex("9AE4EC"),Vector2.zero);v.AddTriangle(i,i+1,i+2);v.AddTriangle(i,i+2,i+3);
   for(int row=0;row<12;row++)for(int col=0;col<11;col++){
    float x=r.xMin+(col+.4f+(row%2)*.4f)*r.width/11,y=r.yMin+(row+.3f)*r.height/13;
    CovePlate.Round(v,new Rect(x,y,24+(col*17+row*13)%50,3),2,new Color(1,1,1,.17f));
   }
   for(int cloud=0;cloud<4;cloud++){
    float x=r.xMin+(.05f+cloud*.29f)*r.width,y=r.yMax-(.06f+(cloud%2)*.08f)*r.height;
    CovePlate.Round(v,new Rect(x,y-32,145,34),17,new Color(1,1,1,.57f));
    CovePlate.Round(v,new Rect(x+20,y-23,64,62),31,new Color(1,1,1,.57f));
    CovePlate.Round(v,new Rect(x+72,y-24,47,43),21,new Color(1,1,1,.57f));
   }
  }
 }

 public sealed class CoveFloat : MonoBehaviour {
  public float amplitude=3,speed=.8f,phase;Vector3 origin;
  void Start(){origin=transform.localPosition;}
  void Update(){transform.localPosition=origin+(MenuPreferences.ReducedMotion?Vector3.zero:Vector3.up*Mathf.Sin(Time.unscaledTime*speed+phase)*amplitude);}
 }

 public sealed class CoveSound : MonoBehaviour {
  static CoveSound instance;AudioSource source;AudioClip tap;
  public static void Play(){if(MenuPreferences.UIVolume<.001f)return;if(!instance)instance=new GameObject("Menu sound").AddComponent<CoveSound>();instance.source.PlayOneShot(instance.tap,MenuPreferences.UIVolume*.26f);}
  void Awake(){instance=this;source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;source.ignoreListenerPause=true;
   const int rate=22050;var samples=new float[1323];for(int i=0;i<samples.Length;i++){float t=(float)i/rate;float envelope=Mathf.Sin(Mathf.PI*i/samples.Length)*Mathf.Exp(-t*50);samples[i]=Mathf.Sin(2*Mathf.PI*(620*t-900*t*t))*envelope;}
   tap=AudioClip.Create("Soft UI tap",samples.Length,1,rate,false);tap.SetData(samples,0);
  }
  void OnDestroy(){if(tap)Destroy(tap);if(instance==this)instance=null;}
 }
}
