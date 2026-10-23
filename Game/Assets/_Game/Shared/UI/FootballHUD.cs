using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 [DefaultExecutionOrder(80)]
 public sealed partial class FootballHUD:MonoBehaviour {
  public static FootballHUD Instance {get;private set;}
  public bool MenuOpen=>overlay&&overlay.gameObject.activeSelf;
  public KickButton Kick=>kick;
  CanvasGroup replayVisibility;
  public void SetReplayHidden(bool hidden){if(!replayVisibility)replayVisibility=gameObject.AddComponent<CanvasGroup>();replayVisibility.alpha=hidden?0:1;replayVisibility.blocksRaycasts=replayVisibility.interactable=!hidden;}
  RectTransform root,parent;Vector2 parentSize;Rect lastSafe;CanvasGroup controls;
  KickButton kick;FootballEffortButton dash;FootballPassButton jump;TackleButton tackle;FootballActionSlot slot;
  Image primaryArt;Text primaryHint,dashHint,jumpHint,tackleHint;BasketballPlate staminaFill;CanvasGroup dashGroup;
  public static void Create(RectTransform parent,Font unused){var r=CoveUI.Rect(parent,"Football touchline UI",0,0,800,450);var hud=r.gameObject.AddComponent<FootballHUD>();hud.root=r;hud.parent=parent;Instance=hud;hud.Build();hud.Layout();hud.CreateControlLayout();}
  void Build(){
   var layer=CoveUI.Rect(root,"Pitch controls",0,0,800,450);CoveUI.Stretch(layer);controls=layer.gameObject.AddComponent<CanvasGroup>();
   var look=BasketballUIStyle.Plate(layer,"Look and aim",0,0,1,1,new Color(1,1,1,.001f),0,0);look.raycastTarget=true;look.rectTransform.anchorMin=new Vector2(.34f,0);look.rectTransform.anchorMax=Vector2.one;look.rectTransform.offsetMin=look.rectTransform.offsetMax=Vector2.zero;look.gameObject.AddComponent<TouchPad>().look=true;
   var joy=BasketballUIStyle.Center(layer,"Movement joystick",ControlDesigns.Size(SportId.Football,"move"),new Vector2(82,84),Vector2.zero);var stickDesign=ControlDesigns.Build(joy,SportId.Football,"move");stickDesign.Face.raycastTarget=true;
   var pad=joy.gameObject.AddComponent<TouchPad>();pad.knob=stickDesign.Thumb;pad.designRadius=joy.rect.width*.5f;AppRoot.Instance.view.stick=pad;
   var primaryDesign=Action(layer,"Kick button",new Vector2(-170,220.5f),"shoot");var primary=ControlDesigns.BindButton(primaryDesign);primaryArt=primaryDesign.Art;
   kick=primary.gameObject.AddComponent<KickButton>();kick.button=primary;kick.illustrated=true;kick.label=primaryDesign.Label;primaryHint=primaryDesign.Hint;
   var guide=BasketballUIStyle.Center(layer,"Kick drag guide",Vector2.one,new Vector2(-170,220.5f),new Vector2(1,0));kick.guide=guide.gameObject.AddComponent<FootballKickGuide>();kick.guide.Build(kick);guide.SetSiblingIndex(primary.transform.GetSiblingIndex());
   var dashDesign=Action(layer,"Football dash button",new Vector2(-85,97),"dash");var db=ControlDesigns.BindButton(dashDesign);dashGroup=db.gameObject.AddComponent<CanvasGroup>();dash=db.gameObject.AddComponent<FootballEffortButton>();dash.button=db;dash.illustrated=true;dash.shootTarget=kick;dash.label=dashDesign.Label;staminaFill=dashDesign.Meter;dashHint=dashDesign.Hint;
   var passDesign=Action(layer,"Football pass button",new Vector2(-53,220),"pass");var jb=ControlDesigns.BindButton(passDesign);jump=jb.gameObject.AddComponent<FootballPassButton>();jump.button=jb;jump.label=passDesign.Label;jumpHint=passDesign.Hint;
   var marker=CoveUI.Rect(layer,"Pass receiver lock",0,0,800,450);CoveUI.Stretch(marker);marker.gameObject.AddComponent<FootballPassMarker>().raycastTarget=false;
   var tackleDesign=Action(layer,"Tackle button",new Vector2(-199,87),"tackle");var tb=ControlDesigns.BindButton(tackleDesign);tackle=tb.gameObject.AddComponent<TackleButton>();tackle.button=tb;tackle.illustrated=true;tackle.label=tackleDesign.Label;tackleHint=tackleDesign.Hint;
   FootballActionSwipe.Attach(db.gameObject,primary.gameObject,jb.gameObject,tb.gameObject);
   FootballActionSlot.Attach(layer);slot=primary.GetComponent<FootballActionSlot>();var pressure=primary.GetComponent<FootballEffortButton>();pressure.dashTarget=(RectTransform)db.transform;pressure.jumpTarget=(RectTransform)jb.transform;pressure.tackleTarget=(RectTransform)tb.transform;FootballMatchHUD.Create(layer,SportsUIStyle.DisplayFont);FootballPropHUD.Create(layer);
   var map=SportsMiniMap.Create(layer,SportId.Football);map.IllustratedFootball=true;var mr=map.rectTransform;mr.anchorMin=mr.anchorMax=new Vector2(0,1);mr.pivot=Vector2.one*.5f;mr.anchoredPosition=new Vector2(80.5f,-53.5f);mr.sizeDelta=new Vector2(125,75);
   var gear=FootballUIStyle.Button(root,"Pitch settings","",0,0,48,48,ToggleSettings);var gr=(RectTransform)gear.transform;gr.anchorMin=gr.anchorMax=gr.pivot=Vector2.one;gr.anchoredPosition=new Vector2(-18,-16);gear.GetComponent<BasketballPlate>().radius=Vector2.one*17;BasketballUIStyle.Art(gear.transform,BasketballIcon.Gear,9,9,30,30);
  }
  ControlDesigns.View Action(Transform p,string name,Vector2 point,string id)=>ControlDesigns.Build(BasketballUIStyle.Center(p,name,ControlDesigns.Size(SportId.Football,id),point,new Vector2(1,0)),SportId.Football,id);
  void Layout(){parentSize=parent.rect.size;lastSafe=Screen.safeArea;float scale=Mathf.Min(2,parentSize.x/800,parentSize.y/450);root.localScale=Vector3.one*scale;root.sizeDelta=parentSize/scale;ClearInput();BuildMenus();}
  void LateUpdate(){
   if(parent.rect.size!=parentSize||lastSafe!=Screen.safeArea)Layout();var view=PlayerView.Instance;if(!view||!view.target)return;
   bool charge=view.Charging,attack=slot.Attacking;primaryArt.sprite=ControlArt.Get(attack?"Football":"Pressure");
   ControlDesigns.Caption(kick.label,charge?Mathf.RoundToInt(view.Charge*100)+"%":attack?"Shoot":"Pressure",charge||attack?1:26f/34);
   primaryHint.text=charge?"POWER":attack?(kick.button.interactable?"HOLD":"NO BALL"):view.PressureHeld?"HOLDING":FootballPressure.CarrierFor(view.target)?"HOLD":"NO CARRIER";
   ControlDesigns.CourtState(kick.button,!attack);ControlDesigns.CourtState(dash.button);ControlDesigns.CourtState(jump.button);ControlDesigns.CourtState(tackle.button);
   var effort=view.target.FootballEffort;ControlDesigns.Caption(dash.label,effort.Exhausted?"Rest":"Dash");dashHint.text=effort.Exhausted?"RECOVERING":view.DashHeld?(charge?"DASH + SHOT":view.FootballPassAiming?"DASH + PASS":effort.Dashing?"SWIPE ACTIONS":"MOVE TO DASH"):"HOLD / SWIPE ACTIONS";
   staminaFill.rectTransform.sizeDelta=new Vector2(69*Mathf.Clamp01(effort.Stamina/100),4);staminaFill.color=effort.Exhausted?CoveUI.Coral:BasketballUIStyle.Teal;
   jumpHint.text=view.FootballPassTarget?"TAP TO PASS":"TAP";float cd=view.target.TackleCooldown;tackleHint.text=cd>.05f?$"{cd:F1}s":view.PressureHeld?"SWIPE LEFT":"TAP";
   ControlDesigns.Caption(tackle.label,cd>.05f?$"{cd:F1}s":"TACKLE");
   Show(dashGroup,true);Show(jump.gameObject,true);Show(tackle.gameObject,!charge&&!attack);
   if(charge){if(!kick.guide.gameObject.activeSelf)kick.guide.Begin();kick.guide.Refresh();}else kick.guide.End();
   if(MenuOpen)RefreshMenu();
  }

  static void Show(GameObject go,bool visible){var g=go.GetComponent<CanvasGroup>();if(!g)g=go.AddComponent<CanvasGroup>();Show(g,visible);}
  static void Show(CanvasGroup g,bool visible){g.alpha=visible?1:0;g.blocksRaycasts=g.interactable=visible;}
  void ClearInput(){if(controls)foreach(var swipe in controls.GetComponentsInChildren<FootballActionSwipe>())swipe.OnCancel(null);FootballPropHUD.Instance?.Cancel();if(kick)kick.CancelGesture();if(controls)foreach(var b in controls.GetComponentsInChildren<FootballEffortButton>())b.Clear();if(PlayerView.Instance){PlayerView.Instance.ClearMatchInput();PlayerView.LookDelta=Vector2.zero;if(controls)foreach(var pad in controls.GetComponentsInChildren<TouchPad>())pad.ResetInput();}}
  void OnDisable(){if(PlayerView.Instance)PlayerView.Instance.InputBlocked=false;if(Instance==this)Instance=null;}
 }
}
