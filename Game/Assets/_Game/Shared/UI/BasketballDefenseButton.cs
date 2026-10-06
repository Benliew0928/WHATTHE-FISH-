using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class BasketballDefenseButton:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,IInitializePotentialDragHandler,IBeginDragHandler,IDragHandler {
  public Button button;public Text label;public bool guard;
  bool pressed,jumping;int pointer;Vector2 origin;
  void Update(){
   var view=PlayerView.Instance;var ball=BasketballBall.Active;
   button.interactable=view&&view.active&&ball&&ball.Role(view.target)==BasketballRole.Defense&&(guard||ball.CanBlock(view.target));
   if(!button.interactable&&pressed)Clear();
   label.fontSize=20;
   label.text=guard?(view&&view.GuardHeld?"Guarding":"Hold Defend"+(Application.isMobilePlatform?"":" [Q]")):jumping?"Release: jump block":button.interactable?(Application.isMobilePlatform?"Block\nUP Jump Block":"Block [E]\nUP Jump Block"):"Recovering";
  }
  public void OnPointerDown(PointerEventData data){
   if(data.button!=PointerEventData.InputButton.Left||pressed||!button.interactable)return;
   if(guard&&!PlayerView.Instance.BeginGuard(data.pointerId))return;
   pressed=true;pointer=data.pointerId;origin=data.position;jumping=false;
  }
  public void OnPointerUp(PointerEventData data){
   if(!pressed||data.pointerId!=pointer)return;Track(data);
   if(guard)PlayerView.Instance?.EndGuard(pointer);else PlayerView.Instance?.RequestBlock(jumping);pressed=false;
  }
  public void OnPointerExit(PointerEventData data){if(guard&&pressed&&data.pointerId==pointer)Clear();else Track(data);}
  public void OnInitializePotentialDrag(PointerEventData data){data.useDragThreshold=false;}
  public void OnBeginDrag(PointerEventData data){Track(data);}
  public void OnDrag(PointerEventData data){Track(data);}
  void Track(PointerEventData data){
   if(guard||!pressed||data.pointerId!=pointer)return;var canvas=GetComponentInParent<Canvas>();float scale=canvas?canvas.scaleFactor:1;
   var delta=(data.position-origin)/Mathf.Max(.01f,scale);
   if(delta.y>BasketballShootButton.SelectDistance&&delta.y>Mathf.Abs(delta.x)*.7f)jumping=true;
   else if(Mathf.Abs(delta.y)<BasketballShootButton.ReturnDistance)jumping=false;
  }
  void Clear(){if(guard&&pressed)PlayerView.Instance?.EndGuard(pointer);pressed=jumping=false;}
  void OnDisable(){Clear();}
  void OnApplicationFocus(bool focused){if(!focused)Clear();}
  void OnApplicationPause(bool paused){if(paused)Clear();}
 }
 // Keep the slots fixed. Disabling the old handler cancels its captured finger
 // before its new role can accept a fresh pointer-down.
 public sealed class BasketballActionSlot:MonoBehaviour {
  BasketballShootButton attack;BasketballDefenseButton defense;CanvasGroup group;
  public static void Attach(Transform parent){
   foreach(var attack in parent.GetComponentsInChildren<BasketballShootButton>(true)){
    var slot=attack.gameObject.AddComponent<BasketballActionSlot>();slot.attack=attack;slot.group=attack.gameObject.AddComponent<CanvasGroup>();
    slot.defense=attack.gameObject.AddComponent<BasketballDefenseButton>();slot.defense.button=attack.button;slot.defense.label=attack.label;slot.defense.guard=attack.pass;slot.defense.enabled=false;
   }
  }
  void Update(){
   var view=PlayerView.Instance;var role=view?view.BasketballRole:BasketballRole.Inactive;
   bool offense=role==BasketballRole.Attack,defending=role==BasketballRole.Defense;
   attack.enabled=offense;defense.enabled=defending;group.alpha=offense||defending?1:0;group.blocksRaycasts=offense||defending;
  }
 }
}
