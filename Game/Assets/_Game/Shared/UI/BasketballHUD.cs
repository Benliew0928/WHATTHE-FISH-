using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 [DefaultExecutionOrder(80)]
 public sealed partial class BasketballHUD:MonoBehaviour {
  public static BasketballHUD Instance {get;private set;}
  public bool MenuOpen=>overlay&&overlay.gameObject.activeSelf;
  public BasketballGestureGuide ShotGuide=>shoot.guide;
  public BasketballGestureGuide PassGuide=>pass.guide;
  RectTransform root,parent;CanvasGroup controls;Text points,baskets,notice;BasketballPlate noticePlate;
  BasketballShootButton shoot,pass;JumpButton jump;BasketballStealButton steal;BasketballPropHUD props;
  Image shootArt,passArt;Text hold;BasketballPlate shootPlate,passPlate;Vector2 parentSize;Rect lastSafe;
  public static void Create(Transform parent,Font unused){var r=CoveUI.Rect(parent,"Basketball courtside UI",0,0,800,450);var hud=r.gameObject.AddComponent<BasketballHUD>();hud.root=r;hud.parent=(RectTransform)parent;Instance=hud;hud.Build();hud.Layout();}
  void Build(){
   var layer=CoveUI.Rect(root,"Court controls",0,0,800,450);CoveUI.Stretch(layer);controls=layer.gameObject.AddComponent<CanvasGroup>();
   var look=BasketballUIStyle.Plate(layer,"Look and aim",0,0,1,1,new Color(1,1,1,.001f),0,0);look.raycastTarget=true;look.rectTransform.anchorMin=new Vector2(.34f,0);look.rectTransform.anchorMax=Vector2.one;look.rectTransform.offsetMin=look.rectTransform.offsetMax=Vector2.zero;look.gameObject.AddComponent<TouchPad>().look=true;
   var joystick=BasketballUIStyle.Center(layer,"Movement joystick",new Vector2(108,108),new Vector2(82,85),Vector2.zero);
   var ring=joystick.gameObject.AddComponent<BasketballPlate>();ring.radius=Vector2.one*54;ring.border=3;ring.edge=new Color(1,.976f,.914f,.6f);ring.color=new Color(1,.976f,.914f,.16f);ring.raycastTarget=true;
   var knob=BasketballUIStyle.Plate(joystick,"Joystick thumb",29.5f,29.5f,49,49,CoveUI.Cream,24.5f,2,true);knob.rectTransform.anchorMin=knob.rectTransform.anchorMax=knob.rectTransform.pivot=Vector2.one*.5f;knob.rectTransform.anchoredPosition=Vector2.zero;
   var pad=joystick.gameObject.AddComponent<TouchPad>();pad.knob=knob.rectTransform;pad.designRadius=54;AppRoot.Instance.view.stick=pad;
   var chevron=CoveUI.Rect(joystick,"Run direction",45,6,18,12).gameObject.AddComponent<BasketballJoystickMark>();chevron.raycastTarget=false;
   shoot=Action(layer,false,new Vector2(-85,100),106,BasketballUIStyle.Shoot,BasketballIcon.Shoot,"Shoot",out shootArt,out shootPlate);
   pass=Action(layer,true,new Vector2(-194,70),80,CoveUI.Sky,BasketballIcon.Pass,"Pass",out passArt,out passPlate);
   hold=BasketballUIStyle.Text(shoot.transform,"HOLD",6,81,94,17,11,false,TextAnchor.MiddleCenter);
   var jumpButton=ActionFrame(layer,"Jump button",new Vector2(-55,206),64,CoveUI.Cream,24);
   BasketballUIStyle.Art(jumpButton.transform,BasketballIcon.Jump,16,8,32,32);jump=jumpButton.gameObject.AddComponent<JumpButton>();jump.button=jumpButton;jumpButton.gameObject.AddComponent<BasketballButtonFeedback>();jump.illustrated=true;jump.label=BasketballUIStyle.Text(jumpButton.transform,"Jump",4,33,56,28,13,true,TextAnchor.MiddleCenter);
   var stealButton=ActionFrame(layer,"Steal button",new Vector2(-275,152),68,CoveUI.Sky,26);BasketballUIStyle.Art(stealButton.transform,BasketballIcon.Steal,15,6,38,38);steal=stealButton.gameObject.AddComponent<BasketballStealButton>();steal.button=stealButton;steal.illustrated=true;steal.label=BasketballUIStyle.Text(stealButton.transform,"Steal",4,43,60,20,14,true,TextAnchor.MiddleCenter);
   stealButton.gameObject.AddComponent<BasketballButtonFeedback>();BasketballActionSlot.Attach(layer);props=BasketballPropHUD.Create(layer);BuildScore(layer);
   var map=SportsMiniMap.Create(layer,SportId.Basketball);var mr=map.rectTransform;mr.anchorMin=mr.anchorMax=new Vector2(0,1);mr.pivot=Vector2.one*.5f;mr.anchoredPosition=new Vector2(69,-47);mr.sizeDelta=new Vector2(204,124);mr.localScale=Vector3.one*.5f;
   var gear=BasketballUIStyle.Button(root,"Court settings","",0,0,48,48,ToggleSettings);var gr=(RectTransform)gear.transform;gr.anchorMin=gr.anchorMax=gr.pivot=new Vector2(1,1);gr.anchoredPosition=new Vector2(-18,-15);gear.GetComponent<BasketballPlate>().radius=Vector2.one*17;BasketballUIStyle.Art(gear.transform,BasketballIcon.Gear,9.5f,9.5f,29,29);
   noticePlate=BasketballUIStyle.Plate(root,"Court feedback",0,0,260,29,CoveUI.Ink,12,0);var nr=noticePlate.rectTransform;nr.anchorMin=nr.anchorMax=nr.pivot=new Vector2(.5f,1);nr.anchoredPosition=new Vector2(0,-73);notice=BasketballUIStyle.Text(nr,"",6,0,248,29,13,true,TextAnchor.MiddleCenter,CoveUI.Cream);noticePlate.gameObject.SetActive(false);
   BuildMenus();
  }
  Button ActionFrame(Transform p,string name,Vector2 point,float size,Color face,float radius){var r=BasketballUIStyle.Center(p,name,Vector2.one*size,point,new Vector2(1,0));var plate=r.gameObject.AddComponent<BasketballPlate>();plate.radius=Vector2.one*radius;plate.border=2.5f;plate.inset=3;plate.shadow=5;plate.color=face;plate.raycastTarget=true;var b=r.gameObject.AddComponent<Button>();b.targetGraphic=plate;b.transition=Selectable.Transition.None;b.navigation=new Navigation{mode=Navigation.Mode.None};return b;}
  BasketballShootButton Action(Transform p,bool isPass,Vector2 point,float size,Color face,BasketballIcon icon,string text,out Image art,out BasketballPlate plate){
   var b=ActionFrame(p,text+" button",point,size,face,isPass?size*.38f:size*.5f);plate=b.GetComponent<BasketballPlate>();if(!isPass)plate.inset=4;
   art=BasketballUIStyle.Art(b.transform,icon,isPass?19:25.5f,isPass?9:6,isPass?42:55,isPass?42:55);
   var control=b.gameObject.AddComponent<BasketballShootButton>();control.pass=isPass;control.button=b;control.illustrated=true;control.label=BasketballUIStyle.Text(b.transform,text,4,isPass?48:56,size-8,isPass?25:31,isPass?17:21,true,TextAnchor.MiddleCenter);b.gameObject.AddComponent<BasketballButtonFeedback>();
   var r=BasketballUIStyle.Center(p,text+" drag guide",Vector2.one,point,new Vector2(1,0));var guide=r.gameObject.AddComponent<BasketballGestureGuide>();control.guide=guide;guide.Build(control);r.SetSiblingIndex(b.transform.GetSiblingIndex());return control;
  }
  void BuildScore(Transform p){var score=BasketballUIStyle.Plate(p,"Shared practice score",0,0,230,46,CoveUI.Cream,20,2,true);var r=score.rectTransform;r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-16);BasketballUIStyle.Art(r,BasketballIcon.Ball,9,8,30,30);points=BasketballUIStyle.Text(r,"0",48,0,48,46,28,true,TextAnchor.MiddleCenter);BasketballUIStyle.Text(r,"SHARED\nPOINTS",99,6,54,34,11);BasketballUIStyle.Plate(r,"Score divider",156,9,1,28,CoveUI.Hex("B6C2B7"),0,0);baskets=BasketballUIStyle.Text(r,"0 / 0",169,3,53,22,15,true);BasketballUIStyle.Text(r,"baskets",169,24,54,18,12);}
  void Layout(){if(!parent)return;parentSize=parent.rect.size;lastSafe=Screen.safeArea;float scale=Mathf.Min(2,parentSize.x/700,parentSize.y/395);root.localScale=Vector3.one*scale;root.sizeDelta=parentSize/scale;if(shoot){shoot.CancelGesture();pass.CancelGesture();}if(overlay)LayoutMenus();}
  void LateUpdate(){
   if(!parent)return;if(parent.rect.size!=parentSize||Screen.safeArea!=lastSafe)Layout();
   var view=PlayerView.Instance;var ball=BasketballBall.Active;if(!view||!ball)return;
   bool defense=!SportsPossession.Attacking(view.target,SportId.Basketball)&&view.BasketballRole!=BasketballRole.Inactive;bool charging=view.ShotCharging||view.PassCharging;
   shootArt.sprite=BasketballUIStyle.Icon(defense?BasketballIcon.Block:BasketballIcon.Shoot);passArt.sprite=BasketballUIStyle.Icon(defense?BasketballIcon.Guard:BasketballIcon.Pass);
   shootPlate.color=shoot.button.interactable?(defense?CoveUI.Sky:BasketballUIStyle.Shoot):CoveUI.Hex("D5DBCE");passPlate.color=pass.button.interactable?(defense?CoveUI.Mint:CoveUI.Sky):CoveUI.Hex("D5DBCE");
   shootArt.color=shoot.button.interactable?Color.white:new Color(.7f,.7f,.7f,.7f);passArt.color=pass.button.interactable?Color.white:new Color(.7f,.7f,.7f,.7f);hold.gameObject.SetActive(!defense&&shoot.button.interactable);
   TintSupport(jump.button,CoveUI.Cream);TintSupport(steal.button,CoveUI.Sky);
   bool slots=view.BasketballRole!=BasketballRole.Inactive;SetShown(shoot.gameObject,slots&&!view.PassCharging);SetShown(pass.gameObject,slots&&!view.ShotCharging);SetShown(jump.gameObject,!charging);SetShown(steal.gameObject,!charging&&defense);props.GestureHidden=charging;
   if(view.ShotCharging){if(!shoot.guide.gameObject.activeSelf)shoot.guide.Begin();shoot.guide.Refresh();}else shoot.guide.End();
   if(view.PassCharging){if(!pass.guide.gameObject.activeSelf)pass.guide.Begin();pass.guide.Refresh();}else pass.guide.End();
   var score=ball.Score;points.text=score.points.ToString();baskets.text=score.made+" / "+score.attempts;string message="";bool fresh=BasketballMotion.Clock-score.changed<2.5;
   if(!MenuOpen&&ball.IsInbound(view.target))message="Out of bounds · pass to re-enter";
   else if(!charging&&!MenuOpen){if(ball.Defense.blocks>0&&BasketballMotion.Clock-ball.Defense.time<1.4)message="Blocked · ball live";else if(fresh&&score.result==BasketballResult.Scored)message="Basket! +"+score.lastPoints;else if(fresh&&score.result==BasketballResult.Missed)message="Missed · get the rebound";else if(view.target&&view.target.BasketballFreeRoam)message="Free roam · play from Settings";}
   notice.text=message;noticePlate.gameObject.SetActive(message.Length>0&&!MenuOpen);if(MenuOpen)RefreshMenu();
  }
  static void SetShown(GameObject go,bool shown){var g=go.GetComponent<CanvasGroup>();if(!g)g=go.AddComponent<CanvasGroup>();g.alpha=shown?1:0;g.blocksRaycasts=shown;}
  static void TintSupport(Button button,Color face){bool available=button.interactable;button.GetComponent<BasketballPlate>().color=available?face:CoveUI.Hex("D5DBCE");button.GetComponentInChildren<Image>().color=available?Color.white:new Color(.7f,.7f,.7f,.7f);}
  public void ToggleCourt(){var app=AppRoot.Instance;var ball=BasketballBall.Active;var actor=app?app.LocalAthlete:null;if(!ball||!ball.Playing||!actor||actor.inTransit)return;app.view.ClearMatchInput();var net=actor.GetComponent<NetworkAthlete>();if(net&&net.IsSpawned)net.BasketballFreeRoamRpc(!actor.BasketballFreeRoam);else ball.SetFreeRoam(actor,!actor.BasketballFreeRoam);CloseMenu();}
  void OnDisable(){if(PlayerView.Instance)PlayerView.Instance.InputBlocked=false;if(Instance==this)Instance=null;}
 }
 public sealed class BasketballJoystickMark:MaskableGraphic {protected override void OnPopulateMesh(VertexHelper v){v.Clear();var c=rectTransform.rect.center;BasketballUIMesh.Line(v,c+new Vector2(-7,-2),c+new Vector2(0,5),CoveUI.Cream,3);BasketballUIMesh.Line(v,c+new Vector2(0,5),c+new Vector2(7,-2),CoveUI.Cream,3);}}
}
