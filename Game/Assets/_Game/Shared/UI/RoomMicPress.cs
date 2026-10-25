using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 // A hold is consumed before the button can emit its release click.
 public sealed class RoomMicPress:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,ICancelHandler {
  public const float HoldSeconds=.5f;
  public Action Tap,Hold;CoveButton button;int pointer=int.MinValue;float began;bool consumed;
  void Awake()=>button=GetComponent<CoveButton>();
  void OnEnable(){consumed=false;pointer=int.MinValue;}
  public void Click(){if(consumed){consumed=false;return;}Tap?.Invoke();}
  public void OnPointerDown(PointerEventData e){
   if(!button.IsInteractable()||pointer!=int.MinValue)return;
   if(e.button==PointerEventData.InputButton.Right){consumed=true;button.CancelPress();Hold?.Invoke();return;}
   if(e.button!=PointerEventData.InputButton.Left)return;
   consumed=false;pointer=e.pointerId;began=Time.unscaledTime;
  }
  void Update(){if(pointer!=int.MinValue&&Time.unscaledTime-began>=HoldSeconds){pointer=int.MinValue;consumed=true;button.CancelPress();Hold?.Invoke();}}
  public void OnPointerUp(PointerEventData e){if(pointer==e.pointerId)pointer=int.MinValue;}
  public void OnPointerExit(PointerEventData e){if(pointer==e.pointerId)Cancel();}
  public void OnCancel(BaseEventData e)=>Cancel();
  void Cancel(){pointer=int.MinValue;consumed=true;if(button)button.CancelPress();}
  void OnDisable()=>Cancel();
  void OnApplicationFocus(bool focused){if(!focused)Cancel();}
  void OnApplicationPause(bool paused){if(paused)Cancel();}
 }
}
