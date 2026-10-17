using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class BasketballDefenseButton:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,IInitializePotentialDragHandler,IBeginDragHandler,IDragHandler,IEndDragHandler,ICancelHandler {
  public Button button;public Text label;public bool guard,illustrated;
  bool pressed,blockSelected,stealArmed;int pointer;BasketballDefenseGuide guide;
  public bool Tracking=>pressed;
  void Update()=>Refresh();
  public void Refresh(){
   var view=PlayerView.Instance;var ball=BasketballBall.Active;
   button.interactable=view&&view.active&&!view.InputBlocked&&ball&&ball.Role(view.target)==BasketballRole.Defense;
   if(pressed&&(!button.interactable||!view.GuardHeld))Clear();
   label.fontSize=illustrated?42:22;label.text="Guard";
   if(guide)guide.gameObject.SetActive(pressed);
  }
  public void OnPointerDown(PointerEventData data){
   if(data.button!=PointerEventData.InputButton.Left||pressed||!button.interactable||!PlayerView.Instance||PlayerView.Instance.InputBlocked||!PlayerView.Instance.BeginGuard(data.pointerId))return;
   pressed=true;pointer=data.pointerId;blockSelected=false;stealArmed=true;
   if(!guide)guide=BasketballDefenseGuide.Create((RectTransform)transform);guide.gameObject.SetActive(true);guide.Select(false);
  }
  public void OnPointerUp(PointerEventData data){
   if(!pressed||data.pointerId!=pointer)return;
   if(BasketballShootButton.CanceledTouch(pointer)){Clear();return;}
   Track(data);if(blockSelected)PlayerView.Instance?.RequestBlock();Clear();
  }
  public void OnPointerExit(PointerEventData data)=>Track(data);
  public void OnInitializePotentialDrag(PointerEventData data){data.useDragThreshold=false;}
  public void OnBeginDrag(PointerEventData data)=>Track(data);
  public void OnDrag(PointerEventData data)=>Track(data);
  void Track(PointerEventData data){
   if(!pressed||data.pointerId!=pointer||!RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,data.position,data.pressEventCamera,out var delta))return;
   if(delta.magnitude<((RectTransform)transform).rect.width*.5f+6){stealArmed=true;blockSelected=false;}
   else if(delta.x<-70&&delta.y<35&&delta.y>-85){blockSelected=false;if(stealArmed){PlayerView.Instance?.RequestGuardSteal(pointer);stealArmed=false;}}
   else blockSelected=delta.y>75&&Mathf.Abs(delta.x)<60;
   if(guide)guide.Select(blockSelected);
  }
  void Clear(){if(pressed)PlayerView.Instance?.EndGuard(pointer);pressed=blockSelected=false;if(guide)guide.gameObject.SetActive(false);}
  public void OnCancel(BaseEventData data)=>Clear();
  public void OnEndDrag(PointerEventData data){if(pressed&&data.pointerId==pointer)Clear();}
  void OnDisable()=>Clear();
  void OnApplicationFocus(bool focused){if(!focused)Clear();}
  void OnApplicationPause(bool paused){if(paused)Clear();}
 }
 // Reuse the attack's fixed thumb slots, artwork and gesture language.
 public sealed class BasketballDefenseGuide:MaskableGraphic {
  BasketballPlate block;Text label;
  public static BasketballDefenseGuide Create(RectTransform owner){
   var r=BasketballUIStyle.Center(owner.parent,"Guard drag guide",Vector2.one,owner.anchoredPosition,new Vector2(1,0));
   r.SetSiblingIndex(owner.GetSiblingIndex());var g=r.gameObject.AddComponent<BasketballDefenseGuide>();g.raycastTarget=false;
   g.block=BasketballUIStyle.Plate(r,"Swipe up block",0,0,92,49,CoveUI.Cream,24,1.5f);g.block.rectTransform.anchorMin=g.block.rectTransform.anchorMax=g.block.rectTransform.pivot=Vector2.one*.5f;g.block.rectTransform.anchoredPosition=new Vector2(0,130);g.block.raycastTarget=false;
   g.label=BasketballUIStyle.Text(g.block.transform,"Block ↑",3,4,86,41,16,true,TextAnchor.MiddleCenter);
   var hint=BasketballUIStyle.Plate(r,"Guard instructions",0,0,234,30,CoveUI.Cream,12,0);hint.rectTransform.anchorMin=hint.rectTransform.anchorMax=hint.rectTransform.pivot=Vector2.one*.5f;hint.rectTransform.anchoredPosition=new Vector2(-64,178);hint.raycastTarget=false;
   BasketballUIStyle.Text(hint.transform,"Hold guard · slide left to steal",5,1,224,28,12,false,TextAnchor.MiddleCenter);return g;
  }
  public void Select(bool selected){block.color=selected?CoveUI.Mint:CoveUI.Cream;block.dashed=!selected;block.Refresh();label.text=selected?"Release\nBlock":"Block ↑";}
  protected override void OnPopulateMesh(VertexHelper v){v.Clear();BasketballUIMesh.Line(v,new Vector2(0,58),new Vector2(0,102),CoveUI.Mint,18);BasketballUIMesh.Line(v,new Vector2(-58,-16),new Vector2(-76,-21),CoveUI.Mint,18);}
 }
 [DefaultExecutionOrder(-40)]
 public sealed class BasketballActionSlot:MonoBehaviour {
  BasketballShootButton attack;BasketballDefenseButton defense;BasketballStealButton steal;CanvasGroup group;
  public static void Attach(Transform parent){
   foreach(var attack in parent.GetComponentsInChildren<BasketballShootButton>(true)){
    var slot=attack.gameObject.AddComponent<BasketballActionSlot>();slot.attack=attack;slot.group=attack.gameObject.AddComponent<CanvasGroup>();
    if(attack.pass){slot.steal=attack.gameObject.AddComponent<BasketballStealButton>();slot.steal.button=attack.button;slot.steal.label=attack.label;slot.steal.illustrated=attack.illustrated;slot.steal.enabled=false;}
    else {slot.defense=attack.gameObject.AddComponent<BasketballDefenseButton>();slot.defense.button=attack.button;slot.defense.label=attack.label;slot.defense.guard=true;slot.defense.illustrated=attack.illustrated;slot.defense.enabled=false;}
   }
  }
  void Update(){
   var view=PlayerView.Instance;var role=view?view.BasketballRole:BasketballRole.Inactive;
   bool offense=view&&SportsPossession.Attacking(view.target,SportId.Basketball),defending=!offense&&role!=BasketballRole.Inactive;
   attack.enabled=offense;if(defense)defense.enabled=defending;if(steal)steal.enabled=defending;
   if(offense)attack.Refresh();else if(defending){if(defense)defense.Refresh();if(steal)steal.Refresh();}
   group.alpha=offense||defending?1:0;group.blocksRaycasts=offense||defending;
  }
 }
}
