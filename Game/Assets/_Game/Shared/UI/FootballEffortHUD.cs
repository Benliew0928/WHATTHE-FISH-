using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class FootballEffortButton:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler {
  public bool pressure;public Button button;public Text label;
  bool pressed;int pointer;
  void Update()=>Refresh();
  public void Refresh(){
   var view=PlayerView.Instance;bool eligible=view&&view.FootballInputReady&&(pressure?FootballPressure.CarrierFor(view.target):!view.Charging);
   if(!eligible)Clear();button.interactable=eligible&&(pressure||pressed||!view.target.FootballEffort.Exhausted);
   bool held=view&&(pressure?view.PressureHeld:view.DashHeld);
   label.text=pressure?(held?"Pressuring":GameButtonStyle.Caption("Pressure","Q")):
    view&&view.target&&view.target.FootballEffort.Exhausted?"Recovering":held?"Dashing":GameButtonStyle.Caption("Dash","Ctrl");
  }
  public void OnPointerDown(PointerEventData e){if(e.button!=PointerEventData.InputButton.Left||pressed||!button.interactable)return;var v=PlayerView.Instance;if(pressure?v.BeginPressure(e.pointerId):v.BeginDash(e.pointerId)){pressed=true;pointer=e.pointerId;}}
  public void OnPointerUp(PointerEventData e){if(pressed&&pointer==e.pointerId)Clear();}
  public void OnPointerExit(PointerEventData e){if(pressed&&pointer==e.pointerId)Clear();}
  void Clear(){if(pressed){if(pressure)PlayerView.Instance?.EndPressure(pointer);else PlayerView.Instance?.EndDash(pointer);}pressed=false;}
  void OnDisable(){Clear();}
  void OnApplicationFocus(bool focused){if(!focused)Clear();}
  void OnApplicationPause(bool paused){if(paused)Clear();}
 }
 [DefaultExecutionOrder(200)]
 public sealed class FootballEffortHUD:MonoBehaviour {
  sealed class Bar {public RectTransform root;public Image fill;public Text state;public Transform head;}
  readonly List<Vector2> placed=new();
  readonly Dictionary<Athlete,Bar> bars=new();readonly List<Athlete> removed=new();
  RectTransform parent;Font font;Bar local;Canvas canvas;
  public int VisibleBars {get;private set;}
  public static void Create(AppRoot app,RectTransform page,Font font){
   var hud=page.gameObject.AddComponent<FootballEffortHUD>();hud.parent=page;hud.font=font;hud.canvas=page.GetComponentInParent<Canvas>();
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
   bar.state.text=own?"STAMINA "+Mathf.RoundToInt(s.Stamina)+(s.Exhausted?" · RELEASE DASH":s.Dashing?" · DASH":s.Pressuring?" · PRESSURE":""):s.Pressuring?"PRESSURE":s.Dashing?"DASH":"";
  }
  void LateUpdate(){
   var view=PlayerView.Instance;var camera=Camera.main;bool active=view&&view.active&&FootballTackle.EnvironmentAllowed&&!FootballMatch.BlocksActions;
   VisibleBars=0;placed.Clear();local.root.gameObject.SetActive(active&&view.target);if(active&&view.target)Paint(local,view.target,true);
   foreach(var actor in Athlete.Active){
    if(!bars.TryGetValue(actor,out var bar)){
     bar=MakeBar("Football stamina above player",new Vector2(86,9));
     foreach(var bone in actor.GetComponentsInChildren<Transform>())if(bone.name=="mixamorig:HeadTop_End_end"){bar.head=bone;break;}
     bars.Add(actor,bar);
    }
    bool visible=active&&camera&&!actor.inTransit&&!(actor==view.target&&view.mode==0);
    var world=bar.head?bar.head.position+Vector3.up*.2f:actor.transform.position+Vector3.up*2.15f;
    var screen=camera?camera.WorldToScreenPoint(world):Vector3.zero;
    visible&=screen.z>0&&screen.x>0&&screen.x<Screen.width&&screen.y>0&&screen.y<Screen.height;
    if(visible)visible=!Physics.Linecast(camera.transform.position,world,1<<8,QueryTriggerInteraction.Ignore);
    bar.root.gameObject.SetActive(visible);if(!visible)continue;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,screen,canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,out var point);
    // Separate nearby overhead meters in broadcast view without hiding any player.
    for(int pass=0;pass<placed.Count;pass++)foreach(var previous in placed)if(Mathf.Abs(previous.x-point.x)<88&&Mathf.Abs(previous.y-point.y)<30)point.y=previous.y+30;
    placed.Add(point);bar.root.anchoredPosition=point;bar.state.rectTransform.anchoredPosition=new Vector2(0,16);Paint(bar,actor,false);VisibleBars++;
   }
   removed.Clear();foreach(var pair in bars)if(!pair.Key||!Athlete.Active.Contains(pair.Key)){Destroy(pair.Value.root.gameObject);removed.Add(pair.Key);}foreach(var actor in removed)bars.Remove(actor);
  }
 }
}
