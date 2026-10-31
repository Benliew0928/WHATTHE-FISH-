using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 // A click belongs to the finger that pressed it. OS cancellation is never a tap.
 public class TouchButton:Button,ICancelHandler {
  int pointer=int.MinValue,releasedPointer=int.MinValue;bool armed;
  public override void OnPointerDown(PointerEventData e){
   if(e.button!=PointerEventData.InputButton.Left||pointer!=int.MinValue||!IsActive()||!IsInteractable())return;
   pointer=e.pointerId;armed=true;releasedPointer=int.MinValue;base.OnPointerDown(e);
  }
  public override void OnPointerUp(PointerEventData e){
   if(e.pointerId!=pointer)return;
   releasedPointer=armed&&IsActive()&&IsInteractable()&&!TouchGesture.Canceled(pointer)&&RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform,e.position,e.pressEventCamera)?pointer:int.MinValue;
   pointer=int.MinValue;armed=false;base.OnPointerUp(e);
  }
  public override void OnPointerExit(PointerEventData e){if(e.pointerId==pointer)armed=false;base.OnPointerExit(e);}
  public override void OnPointerClick(PointerEventData e){if(e.pointerId!=releasedPointer||e.button!=PointerEventData.InputButton.Left||TouchGesture.Canceled(e.pointerId))return;releasedPointer=int.MinValue;base.OnPointerClick(e);}
  public void CancelPress(){pointer=releasedPointer=int.MinValue;armed=false;if(EventSystem.current)base.OnPointerUp(new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left});if(IsActive())DoStateTransition(IsInteractable()?SelectionState.Normal:SelectionState.Disabled,true);}
  public void OnCancel(BaseEventData e)=>CancelPress();
  protected override void OnDisable(){CancelPress();base.OnDisable();}
  protected virtual void OnApplicationFocus(bool focused){if(!focused)CancelPress();}
  protected virtual void OnApplicationPause(bool paused){if(paused)CancelPress();}
 }
 internal static class TouchGesture {
  internal static bool Canceled(int id){
   var module=EventSystem.current?EventSystem.current.currentInputModule as StandaloneInputModule:null;
   var input=module?module.inputOverride:null;int count=input?input.touchCount:Input.touchCount;
   for(int i=0;i<count;i++){var touch=input?input.GetTouch(i):Input.GetTouch(i);if(touch.fingerId==id)return touch.phase==TouchPhase.Canceled;}
   return false;
  }
 }
}
