using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 [DefaultExecutionOrder(80)]
 public sealed partial class FootballHUD:MonoBehaviour {
  public static FootballHUD Instance {get;private set;}
  public bool MenuOpen=>overlay&&overlay.gameObject.activeSelf;
  public KickButton Kick=>kick;
  RectTransform root,parent;Vector2 parentSize;Rect lastSafe;CanvasGroup controls;
  KickButton kick;FootballEffortButton dash;JumpButton jump;TackleButton tackle;FootballActionSlot slot;
  Image primaryArt;Text primaryHint,dashHint,jumpHint,tackleHint;BasketballPlate staminaFill;CanvasGroup dashGroup;
  public static void Create(RectTransform parent,Font unused){var r=CoveUI.Rect(parent,"Football touchline UI",0,0,800,450);var hud=r.gameObject.AddComponent<FootballHUD>();hud.root=r;hud.parent=parent;Instance=hud;hud.Build();hud.Layout();}
  void Build(){
   var layer=CoveUI.Rect(root,"Pitch controls",0,0,800,450);CoveUI.Stretch(layer);controls=layer.gameObject.AddComponent<CanvasGroup>();
   var look=BasketballUIStyle.Plate(layer,"Look and aim",0,0,1,1,new Color(1,1,1,.001f),0,0);look.raycastTarget=true;look.rectTransform.anchorMin=new Vector2(.34f,0);look.rectTransform.anchorMax=Vector2.one;look.rectTransform.offsetMin=look.rectTransform.offsetMax=Vector2.zero;look.gameObject.AddComponent<TouchPad>().look=true;
   var joy=BasketballUIStyle.Center(layer,"Movement joystick",new Vector2(108,108),new Vector2(82,84),Vector2.zero);joy.gameObject.AddComponent<FootballJoystickFace>().raycastTarget=true;
   var knob=BasketballUIStyle.Plate(joy,"Joystick thumb",29.5f,29.5f,49,49,CoveUI.Cream,24.5f,2,true);knob.rectTransform.anchorMin=knob.rectTransform.anchorMax=knob.rectTransform.pivot=Vector2.one*.5f;knob.rectTransform.anchoredPosition=Vector2.zero;
   var pad=joy.gameObject.AddComponent<TouchPad>();pad.knob=knob.rectTransform;pad.designRadius=54;AppRoot.Instance.view.stick=pad;CoveUI.Rect(joy,"Run direction",45,6,18,12).gameObject.AddComponent<BasketballJoystickMark>().raycastTarget=false;
   var primary=Action(layer,"Kick button",new Vector2(-85,97),new Vector2(108,108),54,BasketballUIStyle.Shoot);primaryArt=FootballUIStyle.Art(primary.transform,FootballIcon.Ball,31,13,46,46);
   kick=primary.gameObject.AddComponent<KickButton>();kick.button=primary;kick.illustrated=true;kick.label=BasketballUIStyle.Text(primary.transform,"Kick",4,56,100,30,23,true,TextAnchor.MiddleCenter);primaryHint=BasketballUIStyle.Text(primary.transform,"HOLD",3,85,102,16,10,false,TextAnchor.MiddleCenter);
   var guide=BasketballUIStyle.Center(layer,"Kick drag guide",Vector2.one,new Vector2(-85,97),new Vector2(1,0));kick.guide=guide.gameObject.AddComponent<FootballKickGuide>();kick.guide.Build(kick);guide.SetSiblingIndex(primary.transform.GetSiblingIndex());
   var dr=BasketballUIStyle.Center(layer,"Dash and stamina",new Vector2(80,80),new Vector2(-170,220.5f),new Vector2(1,0));dashGroup=dr.gameObject.AddComponent<CanvasGroup>();
   var db=FootballUIStyle.Button(dr,"Football dash button","",0,0,80,80,null,CoveUI.Mint);Frame(db,29);FootballUIStyle.Art(db.transform,FootballIcon.Dash,18,1,44,44);dash=db.gameObject.AddComponent<FootballEffortButton>();dash.button=db;dash.illustrated=true;dash.label=BasketballUIStyle.Text(db.transform,"Dash",4,45,72,29,18,true,TextAnchor.MiddleCenter);
   var bar=BasketballUIStyle.Plate(dr,"Your football stamina",4,89,72,7,CoveUI.Ink,3.5f,0);staminaFill=BasketballUIStyle.Plate(bar.transform,"Stamina fill",1.5f,1.5f,69,4,BasketballUIStyle.Teal,2,0);dashHint=BasketballUIStyle.Text(dr,"HOLD TO DASH",-10,99,100,16,9,true,TextAnchor.MiddleCenter,CoveUI.Cream);
   var jb=Action(layer,"Jump button",new Vector2(-53,205),new Vector2(66,72),25,CoveUI.Cream);BasketballUIStyle.Art(jb.transform,BasketballIcon.Jump,16,2,34,31);jump=jb.gameObject.AddComponent<JumpButton>();jump.button=jb;jump.illustrated=true;jump.label=BasketballUIStyle.Text(jb.transform,"Jump",3,34,60,23,13,true,TextAnchor.MiddleCenter);jumpHint=BasketballUIStyle.Text(jb.transform,"OFF BALL",2,54,62,15,8,false,TextAnchor.MiddleCenter);
   var tb=Action(layer,"Tackle button",new Vector2(-199,87),new Vector2(76,79),27,CoveUI.Sky);FootballUIStyle.Art(tb.transform,FootballIcon.Tackle,10,3,56,35);tackle=tb.gameObject.AddComponent<TackleButton>();tackle.button=tb;tackle.illustrated=true;tackle.label=BasketballUIStyle.Text(tb.transform,"Tackle",3,38,70,24,15,true,TextAnchor.MiddleCenter);tackleHint=BasketballUIStyle.Text(tb.transform,"TAP",2,60,72,15,9,false,TextAnchor.MiddleCenter);
   FootballActionSlot.Attach(layer);slot=primary.GetComponent<FootballActionSlot>();var pressure=primary.GetComponent<FootballEffortButton>();pressure.dashTarget=(RectTransform)db.transform;pressure.jumpTarget=(RectTransform)jb.transform;pressure.tackleTarget=(RectTransform)tb.transform;FootballMatchHUD.Create(layer,SportsUIStyle.DisplayFont);FootballPropHUD.Create(layer);
   var map=SportsMiniMap.Create(layer,SportId.Football);map.IllustratedFootball=true;var mr=map.rectTransform;mr.anchorMin=mr.anchorMax=new Vector2(0,1);mr.pivot=Vector2.one*.5f;mr.anchoredPosition=new Vector2(80.5f,-53.5f);mr.sizeDelta=new Vector2(125,75);
   var gear=FootballUIStyle.Button(root,"Pitch settings","",0,0,48,48,ToggleSettings);var gr=(RectTransform)gear.transform;gr.anchorMin=gr.anchorMax=gr.pivot=Vector2.one;gr.anchoredPosition=new Vector2(-18,-16);gear.GetComponent<BasketballPlate>().radius=Vector2.one*17;BasketballUIStyle.Art(gear.transform,BasketballIcon.Gear,9,9,30,30);
  }
  Button Action(Transform p,string name,Vector2 point,Vector2 size,float radius,Color color){var r=BasketballUIStyle.Center(p,name,size,point,new Vector2(1,0));var plate=r.gameObject.AddComponent<BasketballPlate>();plate.color=color;plate.raycastTarget=true;var b=r.gameObject.AddComponent<Button>();b.targetGraphic=plate;b.transition=Selectable.Transition.None;b.navigation=new Navigation{mode=Navigation.Mode.None};Frame(b,radius);b.gameObject.AddComponent<BasketballButtonFeedback>();return b;}
  static void Frame(Button b,float radius){var p=b.GetComponent<BasketballPlate>();p.radius=Vector2.one*radius;p.border=2;p.inset=3;p.shadow=5;}
  void Layout(){parentSize=parent.rect.size;lastSafe=Screen.safeArea;float scale=Mathf.Min(2,parentSize.x/800,parentSize.y/450);root.localScale=Vector3.one*scale;root.sizeDelta=parentSize/scale;ClearInput();BuildMenus();}
  void LateUpdate(){
   if(parent.rect.size!=parentSize||lastSafe!=Screen.safeArea)Layout();var view=PlayerView.Instance;if(!view||!view.target)return;
   bool charge=view.Charging,attack=slot.Attacking;primaryArt.sprite=FootballUIStyle.Icon(attack?FootballIcon.Ball:FootballIcon.Pressure);
   kick.label.fontSize=charge||attack?46:32;kick.label.text=charge?Mathf.RoundToInt(view.Charge*100)+"%":attack?"Kick":"Pressure";
   primaryHint.text=charge?"POWER":attack?(kick.button.interactable?"HOLD":"NO BALL"):view.PressureHeld?"HOLDING":FootballPressure.CarrierFor(view.target)?"HOLD":"NO CARRIER";
   Tint(kick.button,attack?BasketballUIStyle.Shoot:CoveUI.Sky);Tint(dash.button,CoveUI.Mint);Tint(jump.button,CoveUI.Cream);Tint(tackle.button,CoveUI.Sky);
   var effort=view.target.FootballEffort;dash.label.text=effort.Exhausted?"Rest":"Dash";dashHint.text=view.PressureHeld?"SWIPE TO DASH":effort.Exhausted?"RECOVERING":view.DashHeld?(effort.Dashing?"DASHING":"MOVE TO DASH"):"HOLD TO DASH";
   staminaFill.rectTransform.sizeDelta=new Vector2(69*Mathf.Clamp01(effort.Stamina/100),4);staminaFill.color=effort.Exhausted?CoveUI.Coral:BasketballUIStyle.Teal;
   jumpHint.text=view.PressureHeld?"SWIPE UP":FootballBall.Instance&&FootballBall.Instance.CurrentController==view.target?"OFF BALL":"TAP";float cd=view.target.TackleCooldown;tackleHint.text=cd>.05f?$"{cd:F1}s":view.PressureHeld?"SWIPE LEFT":"TAP";
   Show(dashGroup,!charge);Show(jump.gameObject,!charge);Show(tackle.gameObject,!charge&&!attack);
   if(charge){if(!kick.guide.gameObject.activeSelf)kick.guide.Begin();kick.guide.Refresh();}else kick.guide.End();
   if(MenuOpen)RefreshMenu();
  }
  static void Tint(Button b,Color face){b.GetComponent<BasketballPlate>().color=b.interactable?face:FootballUIStyle.Disabled;foreach(var art in b.GetComponentsInChildren<Image>())art.color=b.interactable?Color.white:new Color(1,1,1,.76f);}
  static void Show(GameObject go,bool visible){var g=go.GetComponent<CanvasGroup>();if(!g)g=go.AddComponent<CanvasGroup>();Show(g,visible);}
  static void Show(CanvasGroup g,bool visible){g.alpha=visible?1:0;g.blocksRaycasts=g.interactable=visible;}
  void ClearInput(){FootballPropHUD.Instance?.Cancel();if(kick)kick.CancelGesture();if(controls)foreach(var b in controls.GetComponentsInChildren<FootballEffortButton>())b.Clear();if(PlayerView.Instance){PlayerView.Instance.ClearMatchInput();PlayerView.LookDelta=Vector2.zero;if(controls)foreach(var pad in controls.GetComponentsInChildren<TouchPad>())pad.ResetInput();}}
  void OnDisable(){if(PlayerView.Instance)PlayerView.Instance.InputBlocked=false;if(Instance==this)Instance=null;}
 }
}
