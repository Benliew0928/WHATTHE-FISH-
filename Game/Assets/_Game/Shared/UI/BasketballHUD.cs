using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 [DefaultExecutionOrder(80)]
 public sealed partial class BasketballHUD:MonoBehaviour {
  public static BasketballHUD Instance {get;private set;}
  public bool MenuOpen=>overlay&&overlay.gameObject.activeSelf;
  public BasketballGestureGuide ShotGuide=>shoot.guide;
  public BasketballGestureGuide PassGuide=>pass.guide;
  RectTransform root,parent;CanvasGroup controls;Text notice;BasketballPlate noticePlate;GameObject practiceScore;SportsScoreboard practiceBoard;
  BasketballShootButton shoot,pass;JumpButton jump;BasketballStealButton steal;BasketballPropHUD props;
  BasketballPlate passMarker;Athlete previewReceiver;
  Image shootArt,passArt;Text hold;Graphic shootPlate,passPlate;Vector2 parentSize;Rect lastSafe;
  public static void Create(Transform parent,Font unused){var r=CoveUI.Rect(parent,"Basketball courtside UI",0,0,800,450);var hud=r.gameObject.AddComponent<BasketballHUD>();hud.root=r;hud.parent=(RectTransform)parent;Instance=hud;hud.Build();hud.Layout();hud.CreateControlLayout();}
  void Build(){
   var layer=CoveUI.Rect(root,"Court controls",0,0,800,450);CoveUI.Stretch(layer);controls=layer.gameObject.AddComponent<CanvasGroup>();
   var look=BasketballUIStyle.Plate(layer,"Look and aim",0,0,1,1,new Color(1,1,1,.001f),0,0);look.raycastTarget=true;look.rectTransform.anchorMin=new Vector2(.34f,0);look.rectTransform.anchorMax=Vector2.one;look.rectTransform.offsetMin=look.rectTransform.offsetMax=Vector2.zero;look.gameObject.AddComponent<TouchPad>().look=true;
   var joystick=BasketballUIStyle.Center(layer,"Movement joystick",ControlDesigns.Size(SportId.Basketball,"move"),new Vector2(82,85),Vector2.zero);
   var stickDesign=ControlDesigns.Build(joystick,SportId.Basketball,"move");stickDesign.Face.raycastTarget=true;
   var pad=joystick.gameObject.AddComponent<TouchPad>();pad.knob=stickDesign.Thumb;pad.designRadius=joystick.rect.width*.5f;AppRoot.Instance.view.stick=pad;
   shoot=Action(layer,false,new Vector2(-85,100),out shootArt,out shootPlate);
   pass=Action(layer,true,new Vector2(-194,70),out passArt,out passPlate);
   hold=shoot.GetComponent<ControlDesignSource>().View.Hint;
   var jumpDesign=ActionFrame(layer,"Jump button",new Vector2(-55,206),"jump");var jumpButton=ControlDesigns.BindButton(jumpDesign);
   jump=jumpButton.gameObject.AddComponent<JumpButton>();jump.button=jumpButton;jump.illustrated=true;jump.label=jumpDesign.Label;
   BasketballActionSlot.Attach(layer);steal=pass.GetComponent<BasketballStealButton>();props=BasketballPropHUD.Create(layer);BuildScore(layer);BasketballMatchHUD.Create(layer);
   var map=SportsMiniMap.Create(layer,SportId.Basketball);var mr=map.rectTransform;mr.anchorMin=mr.anchorMax=new Vector2(0,1);mr.pivot=Vector2.one*.5f;mr.anchoredPosition=new Vector2(69,-47);mr.sizeDelta=new Vector2(204,124);mr.localScale=Vector3.one*.5f;
   var gear=BasketballUIStyle.Button(root,"Court settings","",0,0,48,48,ToggleSettings);var gr=(RectTransform)gear.transform;gr.anchorMin=gr.anchorMax=gr.pivot=new Vector2(1,1);gr.anchoredPosition=new Vector2(-18,-15);gear.GetComponent<BasketballPlate>().radius=Vector2.one*17;BasketballUIStyle.Art(gear.transform,BasketballIcon.Gear,9.5f,9.5f,29,29);
   noticePlate=BasketballUIStyle.Plate(root,"Court feedback",0,0,260,29,CoveUI.Ink,12,0);var nr=noticePlate.rectTransform;nr.anchorMin=nr.anchorMax=nr.pivot=new Vector2(.5f,1);nr.anchoredPosition=new Vector2(0,-73);notice=BasketballUIStyle.Text(nr,"",6,0,248,29,13,true,TextAnchor.MiddleCenter,CoveUI.Cream);noticePlate.gameObject.SetActive(false);
   passMarker=BasketballUIStyle.Plate(root,"Selected pass teammate",0,0,112,36,CoveUI.Mint,15,2);passMarker.raycastTarget=false;passMarker.rectTransform.anchorMin=passMarker.rectTransform.anchorMax=passMarker.rectTransform.pivot=Vector2.one*.5f;
   BasketballUIStyle.Text(passMarker.transform,"▼ PASS TO",3,1,106,34,15,true,TextAnchor.MiddleCenter);passMarker.gameObject.SetActive(false);
   BuildMenus();
  }
  ControlDesigns.View ActionFrame(Transform p,string name,Vector2 point,string id)=>ControlDesigns.Build(BasketballUIStyle.Center(p,name,ControlDesigns.Size(SportId.Basketball,id),point,new Vector2(1,0)),SportId.Basketball,id);
  BasketballShootButton Action(Transform p,bool isPass,Vector2 point,out Image art,out Graphic plate){
   string text=isPass?"Pass":"Shoot";var design=ActionFrame(p,text+" button",point,isPass?"pass":"shoot");var b=ControlDesigns.BindButton(design);plate=design.Face;art=design.Art;
   var control=b.gameObject.AddComponent<BasketballShootButton>();control.pass=isPass;control.button=b;control.illustrated=true;control.label=design.Label;
   if(!isPass){var r=BasketballUIStyle.Center(p,text+" drag guide",Vector2.one,point,new Vector2(1,0));var guide=r.gameObject.AddComponent<BasketballGestureGuide>();control.guide=guide;guide.Build(control);r.SetSiblingIndex(b.transform.GetSiblingIndex());}return control;
  }
  void BuildScore(Transform p){practiceBoard=SportsScoreboard.Create(p,SportId.Basketball);practiceScore=practiceBoard.gameObject;var r=(RectTransform)practiceScore.transform;r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-16);}
  void Layout(){if(!parent)return;parentSize=parent.rect.size;lastSafe=Screen.safeArea;float scale=Mathf.Min(2,parentSize.x/700,parentSize.y/395);root.localScale=Vector3.one*scale;root.sizeDelta=parentSize/scale;if(shoot){shoot.CancelGesture();pass.CancelGesture();}if(overlay)LayoutMenus();}
  void LateUpdate(){
   var honours=root.GetComponent<CanvasGroup>();if(!honours)honours=root.gameObject.AddComponent<CanvasGroup>();honours.alpha=BasketballMVP.Presenting?0:1;honours.blocksRaycasts=!BasketballMVP.Presenting;
   if(!parent)return;if(parent.rect.size!=parentSize||Screen.safeArea!=lastSafe)Layout();
   var view=PlayerView.Instance;var ball=BasketballBall.Active;if(!view||!ball)return;
   bool room=AppRoot.Instance.rooms.Connected||BasketballMatch.Instance&&BasketballMatch.Instance.Competitive;practiceScore.SetActive(!room);
   bool defense=!SportsPossession.Attacking(view.target,SportId.Basketball)&&view.BasketballRole!=BasketballRole.Inactive;bool charging=view.ShotCharging||view.PassCharging;
   shootArt.sprite=ControlArt.Get(defense?"Guard":"Basketball");passArt.sprite=ControlArt.Get(defense?"Steal":"BasketPass");
   bool blockReady=defense&&ball.GuardBlockReady(view.target);
   ControlDesigns.CourtState(shoot.button,defense,blockReady);ControlDesigns.CourtState(pass.button,defense);
   shootArt.color=shoot.button.interactable?Color.white:new Color(1,1,1,.76f);passArt.color=pass.button.interactable?Color.white:new Color(1,1,1,.76f);hold.gameObject.SetActive(!defense&&shoot.button.interactable);
   TintSupport(jump.button,ControlDesigns.FaceColor(jump.button));
   bool slots=view.BasketballRole!=BasketballRole.Inactive;SetShown(shoot.gameObject,slots&&!view.PassCharging);SetShown(pass.gameObject,slots&&!view.ShotCharging);SetShown(jump.gameObject,!charging);props.GestureHidden=charging;
   SetShown(jump.gameObject,!charging&&!view.GuardHeld);
   if(view.ShotCharging){if(!shoot.guide.gameObject.activeSelf)shoot.guide.Begin();shoot.guide.Refresh();}else shoot.guide.End();
   UpdatePassMarker(view,ball);
   var score=ball.Score;string message="";bool fresh=BasketballMotion.Clock-score.changed<2.5;
   if(!MenuOpen&&ball.IsInbound(view.target))message="Out of bounds Â· pass to re-enter";
   else if(!charging&&!MenuOpen){if(ball.Defense.blocks>0&&BasketballMotion.Clock-ball.Defense.time<1.4)message="Blocked Â· ball live";else if(fresh&&score.result==BasketballResult.Missed)message="Missed Â· get the rebound";else if(view.target&&view.target.BasketballFreeRoam)message="Free roam Â· play from Settings";}
   noticePlate.rectTransform.anchoredPosition=new Vector2(0,practiceBoard.Animating?-150:-100);
   notice.text=message;noticePlate.gameObject.SetActive(message.Length>0&&!MenuOpen&&!room);if(MenuOpen)RefreshMenu();
  }
  void UpdatePassMarker(PlayerView view,BasketballBall ball){
   bool show=!MenuOpen&&ball.Holder==view.target&&!view.ShotCharging&&!ball.ActionQueued;
   previewReceiver=show?(view.PassCharging?ball.PassReceiver:ball.SelectPassReceiver(view.target,view.yaw,previewReceiver)):null;
   var camera=Camera.main;var screen=previewReceiver&&camera?camera.WorldToScreenPoint(previewReceiver.transform.position+Vector3.up*2.1f):Vector3.zero;
   show=previewReceiver&&screen.z>0&&screen.x>0&&screen.x<Screen.width&&screen.y>0&&screen.y<Screen.height;
   passMarker.gameObject.SetActive(show);if(show&&RectTransformUtility.ScreenPointToLocalPointInRectangle(root,screen,null,out var local))passMarker.rectTransform.anchoredPosition=local-root.rect.center;
  }
  static void SetShown(GameObject go,bool shown){var g=go.GetComponent<CanvasGroup>();if(!g)g=go.AddComponent<CanvasGroup>();g.alpha=shown?1:0;g.blocksRaycasts=shown;}
  static void TintSupport(Button button,Color face)=>ControlDesigns.Availability(button,button.interactable);
  public void ToggleCourt(){var app=AppRoot.Instance;var ball=BasketballBall.Active;var actor=app?app.LocalAthlete:null;if(!ball||!ball.Playing||!actor||actor.inTransit)return;app.view.ClearMatchInput();var net=actor.GetComponent<NetworkAthlete>();if(net&&net.IsSpawned)net.BasketballFreeRoamRpc(!actor.BasketballFreeRoam);else ball.SetFreeRoam(actor,!actor.BasketballFreeRoam);CloseMenu();}
  void OnDisable(){if(PlayerView.Instance)PlayerView.Instance.InputBlocked=false;if(Instance==this)Instance=null;}
 }
 public sealed class BasketballJoystickMark:MaskableGraphic {protected override void OnPopulateMesh(VertexHelper v){v.Clear();var c=rectTransform.rect.center;BasketballUIMesh.Line(v,c+new Vector2(-7,-2),c+new Vector2(0,5),CoveUI.Cream,3);BasketballUIMesh.Line(v,c+new Vector2(0,5),c+new Vector2(7,-2),CoveUI.Cream,3);}}
}
