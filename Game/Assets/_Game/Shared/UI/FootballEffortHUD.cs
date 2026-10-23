using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class FootballEffortButton:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,IInitializePotentialDragHandler,IBeginDragHandler,IDragHandler,IEndDragHandler,ICancelHandler {
  public bool pressure,illustrated;public Button button;public Text label;
  public RectTransform dashTarget,jumpTarget,tackleTarget;
  public KickButton shootTarget;
  bool pressed,swipeShot;int pointer,choice;FootballPressureGuide guide;
  public bool Tracking=>pressed;
  void Update()=>Refresh();
  public void Refresh(){
   var view=PlayerView.Instance;bool eligible=view&&(pressure?view.FootballPressureReady&&FootballPressure.CarrierFor(view.target):view.FootballInputReady);
   if(!eligible||pressed&&pressure&&!view.PressureHeld)Clear();button.interactable=eligible&&(pressure||pressed||!view.target.FootballEffort.Exhausted);
   bool held=view&&(pressure?view.PressureHeld:view.DashHeld);
   if(illustrated)return;
   label.text=pressure?(held?"Pressuring":GameButtonStyle.Caption("Pressure")):
    view&&view.target&&view.target.FootballEffort.Exhausted?"Recovering":held?"Dashing":GameButtonStyle.Caption("Dash");
  }
  public void OnPointerDown(PointerEventData e){
   if(e.button!=PointerEventData.InputButton.Left||pressed||!button.interactable)return;var v=PlayerView.Instance;
   if(pressure?v.BeginPressure(e.pointerId):v.BeginDash(e.pointerId)){
    pressed=true;pointer=e.pointerId;choice=0;
    GetComponent<FootballActionSwipe>()?.Capture(e);
    if(pressure&&!GetComponent<FootballActionSwipe>()&&dashTarget&&jumpTarget&&tackleTarget){if(!guide)guide=FootballPressureGuide.Create((RectTransform)transform,dashTarget,jumpTarget,tackleTarget);guide.gameObject.SetActive(true);guide.Select(0);}
   }
  }
  public void OnPointerUp(PointerEventData e){if(pressed&&pointer==e.pointerId){if(swipeShot&&shootTarget){shootTarget.OnPointerUp(e);swipeShot=false;}Clear();}}
  public void OnPointerExit(PointerEventData e){if(GetComponent<FootballActionSwipe>())return;if(pressure||shootTarget)Track(e);else if(pressed&&pointer==e.pointerId)Clear();}
  public void OnInitializePotentialDrag(PointerEventData e){e.useDragThreshold=false;}
  public void OnBeginDrag(PointerEventData e)=>Track(e);
  public void OnDrag(PointerEventData e)=>Track(e);
  void Track(PointerEventData e){
   var swipe=GetComponent<FootballActionSwipe>();if(swipe){swipe.Track(e);return;}
   if(!pressure&&pressed&&e.pointerId==pointer&&shootTarget){
    if(!swipeShot&&shootTarget.enabled&&RectTransformUtility.RectangleContainsScreenPoint((RectTransform)shootTarget.transform,e.position,e.pressEventCamera)){shootTarget.OnPointerDown(e);swipeShot=shootTarget.Tracking;}
    else if(swipeShot){
     // Returning the same finger to Dash releases the shot while Dash stays held.
     if(RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform,e.position,e.pressEventCamera)){shootTarget.OnPointerUp(e);swipeShot=false;}
     else shootTarget.OnDrag(e);
    }return;
   }
   if(!pressure||!pressed||e.pointerId!=pointer||!dashTarget||!RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,e.position,e.pressEventCamera,out var delta))return;
   int next=0;
   if(delta.magnitude>((RectTransform)transform).rect.width*.5f+6){
    var targets=new[]{dashTarget,jumpTarget,tackleTarget};
    for(int i=0;i<targets.Length;i++){
     var toward=(Vector2)transform.InverseTransformPoint(targets[i].TransformPoint(targets[i].rect.center));
     if((delta-toward).magnitude<48||delta.magnitude<toward.magnitude+40&&Vector2.Dot(delta.normalized,toward.normalized)>.95f){next=i+1;break;}
    }
   }
   if(next==choice)return;
   var view=PlayerView.Instance;if(choice==1)view.EndDash(pointer);choice=next;
   if(next==1)view.BeginDash(pointer);else if(next==2){if(view.BeginFootballPass(pointer))view.EndFootballPass(pointer);}else if(next==3)view.RequestTackle();
   if(guide)guide.Select(next);
  }
  public void Clear(){if(swipeShot&&shootTarget)shootTarget.CancelGesture();swipeShot=false;if(pressed){if(pressure){PlayerView.Instance?.EndPressure(pointer);PlayerView.Instance?.EndDash(pointer);}else PlayerView.Instance?.EndDash(pointer);}pressed=false;choice=0;if(guide)guide.gameObject.SetActive(false);}
  public void OnCancel(BaseEventData e)=>Clear();
  public void OnEndDrag(PointerEventData e){if(pressed&&e.pointerId==pointer)Clear();}
  void OnDisable()=>Clear();
  void OnApplicationFocus(bool focused){if(!focused)Clear();}
  void OnApplicationPause(bool paused){if(paused)Clear();}
 }
 public sealed class FootballPressureGuide:MaskableGraphic {
  readonly Vector2[] ends=new Vector2[3];int selected;
  public static FootballPressureGuide Create(RectTransform owner,params RectTransform[] targets){
   var r=BasketballUIStyle.Center(owner.parent,"Pressure swipe paths",Vector2.one,owner.anchoredPosition,new Vector2(1,0));r.SetSiblingIndex(owner.GetSiblingIndex());
   var guide=r.gameObject.AddComponent<FootballPressureGuide>();guide.raycastTarget=false;
   for(int i=0;i<3;i++)guide.ends[i]=owner.InverseTransformPoint(targets[i].TransformPoint(targets[i].rect.center));return guide;
  }
  public void Select(int choice){selected=choice;SetVerticesDirty();}
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();for(int i=0;i<3;i++){var direction=ends[i].normalized;var start=direction*60;var end=ends[i]-direction*40;bool active=selected==i+1;BasketballUIMesh.Line(v,start,end,active?CoveUI.Mint:new Color(1,.98f,.91f,.55f),active?15:10);var tip=Vector2.Lerp(start,end,.7f);var side=new Vector2(-direction.y,direction.x)*4;BasketballUIMesh.Line(v,tip-direction*6+side,tip,CoveUI.Ink,2);BasketballUIMesh.Line(v,tip-direction*6-side,tip,CoveUI.Ink,2);}
  }
 }
 [DefaultExecutionOrder(10020)]
 public sealed class FootballEffortHUD:MonoBehaviour {
  sealed class Bar {public RectTransform root;public Image fill;public Text state;public Transform head;}
  readonly List<Vector2> placed=new();
  readonly Dictionary<Athlete,Bar> bars=new();readonly List<Athlete> removed=new();
  RectTransform parent;Font font;Bar local;Canvas canvas;
  public int VisibleBars {get;private set;}
  public static void Create(AppRoot app,RectTransform page,Font font,bool overheadOnly=false){
   var hud=page.gameObject.AddComponent<FootballEffortHUD>();hud.parent=page;hud.font=font;hud.canvas=page.GetComponentInParent<Canvas>();
   if(overheadOnly)return;
   hud.local=hud.MakeBar("Your football stamina",new Vector2(210,12));hud.local.state.rectTransform.sizeDelta=new Vector2(370,22);hud.local.root.anchorMin=hud.local.root.anchorMax=new Vector2(.5f,0);hud.local.root.anchoredPosition=new Vector2(0,161);
   foreach(bool pressure in new[]{false}){
    var r=app.Panel(page,new Vector2(1300,340),new Vector2(138,138),pressure?new Color(.94f,.73f,.34f):new Color(.33f,.84f,.77f));r.name=pressure?"Football pressure button":"Football dash button";
    var b=r.gameObject.AddComponent<FootballEffortButton>();b.pressure=pressure;b.button=r.gameObject.AddComponent<Button>();
    b.label=Text(r,font,22,new Vector2(210,90));
   }
  }
  static Text Text(Transform p,Font f,int size,Vector2 bounds){var t=new GameObject("Label",typeof(RectTransform),typeof(Text)).GetComponent<Text>();t.transform.SetParent(p,false);t.rectTransform.sizeDelta=bounds;t.font=f;t.fontSize=size;t.alignment=TextAnchor.MiddleCenter;t.color=new Color(.04f,.16f,.19f);t.raycastTarget=false;return t;}
  Bar MakeBar(string name,Vector2 size){
   var r=new GameObject(name,typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();r.SetParent(parent,false);r.sizeDelta=size;
   var bg=r.GetComponent<Image>();bg.color=new Color(.02f,.08f,.12f,.9f);bg.raycastTarget=false;
   var f=new GameObject("Stamina fill",typeof(RectTransform),typeof(Image)).GetComponent<Image>();f.transform.SetParent(r,false);f.raycastTarget=false;f.rectTransform.anchorMin=Vector2.zero;f.rectTransform.anchorMax=Vector2.one;f.rectTransform.offsetMin=new Vector2(2,2);f.rectTransform.offsetMax=new Vector2(-2,-2);
   var t=Text(r,font,14,new Vector2(Mathf.Max(130,size.x),22));t.color=Color.white;t.rectTransform.anchoredPosition=new Vector2(0,-18);
   return new Bar{root=r,fill=f,state=t};
  }
  void Paint(Bar bar,Athlete actor,bool own){
   var s=actor.FootballEffort;bar.fill.enabled=s.Stamina>1;bar.fill.rectTransform.anchorMax=new Vector2(s.Stamina/100,1);
   bar.fill.color=s.Exhausted?new Color(1,.34f,.25f):s.Dashing?new Color(1,.76f,.25f):s.Pressuring?new Color(.4f,.75f,1):new Color(.28f,.94f,.72f);
   bar.state.text=own?"STAMINA "+Mathf.RoundToInt(s.Stamina)+(s.Exhausted?" Â· RELEASE DASH":s.Dashing?" Â· DASH":s.Pressuring?" Â· PRESSURE":""):s.Pressuring?"PRESSURE":s.Dashing?"DASH":"";
   if(!own&&FootballBots.IsBot(actor)){var n=actor.GetComponent<NetworkAthlete>();int slot=n&&n.IsSpawned?n.BotSlot.Value:actor.GetComponent<FootballBot>().Slot;bar.state.text="BOT "+slot+(s.Pressuring?" Â· PRESSURE":s.Dashing?" Â· DASH":"");}
  }
  void LateUpdate(){
   var view=PlayerView.Instance;var camera=Camera.main;bool active=view&&view.active&&!view.InputBlocked&&FootballTackle.EnvironmentAllowed&&!FootballMatch.BlocksActions;
   VisibleBars=0;placed.Clear();if(local!=null){local.root.gameObject.SetActive(active&&view.target);if(active&&view.target)Paint(local,view.target,true);}
   foreach(var actor in Athlete.Active){
    if(!bars.TryGetValue(actor,out var bar)){
     bar=MakeBar("Football stamina above player",new Vector2(86,9));
     foreach(var bone in actor.GetComponentsInChildren<Transform>())if(bone.name=="mixamorig:HeadTop_End_end"){bar.head=bone;break;}
     bars.Add(actor,bar);
    }
    // The local Dash control already carries stamina. Keep Open type clean
    // at rest; a peer's meter appears only when effort is being spent/recovered.
    bool visible=active&&camera&&!actor.inTransit&&actor!=view.target&&(actor.FootballEffort.Stamina<99||actor.FootballEffort.Dashing||actor.FootballEffort.Pressuring);
    var world=bar.head?bar.head.position+Vector3.up*.2f:actor.transform.position+Vector3.up*2.15f;
    var screen=camera?camera.WorldToScreenPoint(world):Vector3.zero;
    bool identified=actor.GetComponent<CharacterIdentity>();Rect badge=default;
    if(identified){visible&=CharacterIdentityBadges.TryGetScreenRect(actor,out badge);screen=new Vector3(badge.center.x,Screen.height-badge.yMax-4,screen.z);}
    visible&=screen.z>0&&screen.x>0&&screen.x<Screen.width&&screen.y>0&&screen.y<Screen.height;
    if(visible)visible=!Physics.Linecast(camera.transform.position,world,1<<8,QueryTriggerInteraction.Ignore);
    bar.root.gameObject.SetActive(visible);if(!visible)continue;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,screen,canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,out var point);
    // Separate nearby overhead meters in broadcast view without hiding any player.
    if(!identified)for(int pass=0;pass<placed.Count;pass++)foreach(var previous in placed)if(Mathf.Abs(previous.x-point.x)<88&&Mathf.Abs(previous.y-point.y)<30)point.y=previous.y+30;
    placed.Add(point);bar.root.anchoredPosition=point;bar.state.rectTransform.anchoredPosition=new Vector2(0,16);Paint(bar,actor,false);VisibleBars++;
    if(identified){bar.state.text="";bar.root.sizeDelta=new Vector2(Mathf.Min(badge.width,50*Screen.height/720f)/canvas.scaleFactor,5/canvas.scaleFactor);}
   }
   removed.Clear();foreach(var pair in bars)if(!pair.Key||!Athlete.Active.Contains(pair.Key)){Destroy(pair.Value.root.gameObject);removed.Add(pair.Key);}foreach(var actor in removed)bars.Remove(actor);
  }
 }
}
