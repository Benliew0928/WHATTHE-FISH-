using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class BasketballShootButton:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,IInitializePotentialDragHandler,IBeginDragHandler,IDragHandler,ICancelHandler,IEndDragHandler {
  public Button button;public Text label,cancelLabel;public RectTransform cancelArea;public bool pass,illustrated;public BasketballGestureGuide guide;
  int pointer=int.MinValue;bool pressed,tracking;Vector2 pressPosition;uint play;
  public bool Tracking=>tracking;
  public const float SelectDistance=48,ReturnDistance=26;
  void Update()=>Refresh();
  public void Refresh(){
   var view=PlayerView.Instance;var ball=BasketballBall.Active;
   button.interactable=view&&view.active&&!view.InputBlocked&&ball&&(pass?ball.CanPass(view.target):ball.CanShoot(view.target))&&(pass?!view.ShotCharging:!view.PassCharging);
   bool holding=view&&(pass?view.PassCharging:view.ShotCharging);
   if(tracking&&(!button.interactable||!view||ball.Defense.play!=play))ResetGesture();else if(pressed&&!holding)CancelGesture();
   if(illustrated){label.fontSize=pass?34:42;label.text=pass?"Pass":"Shoot";return;}
   if(cancelArea)cancelArea.gameObject.SetActive(tracking||holding);
   if(!tracking&&holding&&cancelLabel)cancelLabel.text=Application.isMobilePlatform?"Cancel":"Cancel [X]";
   label.fontSize=22;
   if(pass){label.text=holding?$"{BasketballPassRules.Name(view.PassBend)}\n{Mathf.RoundToInt(view.PassPower*100)}%":ball&&ball.ActionQueued?"Passing...":GameButtonStyle.Caption("Pass","Q");return;}
   label.text=view&&view.target&&view.target.BasketballFreeRoam?"Free roam":holding?view.ShotFinish.ToString()+"\nRelease":ball&&ball.ActionQueued?"Gathering...":GameButtonStyle.Caption("Shoot","E");
  }
  public void OnPointerDown(PointerEventData data){if(data.button!=PointerEventData.InputButton.Left||!button.interactable||tracking||!PlayerView.Instance||PlayerView.Instance.InputBlocked)return;if(pass?PlayerView.Instance.BeginPass(data.pointerId):PlayerView.Instance.BeginShot(data.pointerId)){pressed=tracking=true;pointer=data.pointerId;pressPosition=data.position;play=BasketballBall.Active.Defense.play;if(guide)guide.Begin();else {if(cancelLabel)cancelLabel.text="Cancel";if(cancelArea)cancelArea.gameObject.SetActive(true);}}}
  public void OnInitializePotentialDrag(PointerEventData data){data.useDragThreshold=false;}
  public void OnBeginDrag(PointerEventData data){CheckCancel(data);}
  public void OnDrag(PointerEventData data){CheckCancel(data);}
  public void OnPointerExit(PointerEventData data){CheckCancel(data);}
  void CheckCancel(PointerEventData data){
   if(!tracking||!pressed||data.pointerId!=pointer)return;
   if(guide){
    if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(guide.rectTransform,data.position,data.pressEventCamera,out var local))return;
    var choice=guide.Track(local);if(choice==BasketballDragChoice.Cancel){guide.Refresh();return;}
    if(pass)PlayerView.Instance.SelectPass(choice==BasketballDragChoice.Upper?1:choice==BasketballDragChoice.Diagonal?-1:0,pointer);
    else PlayerView.Instance.SelectFinish(choice==BasketballDragChoice.Upper?BasketballFinish.Dunk:choice==BasketballDragChoice.Diagonal?BasketballFinish.Layup:BasketballFinish.Shot,pointer);
    guide.Refresh();return;
   }
   if(cancelArea&&RectTransformUtility.RectangleContainsScreenPoint(cancelArea,data.position,data.pressEventCamera))return;
   var canvas=GetComponentInParent<Canvas>();float scale=canvas?canvas.scaleFactor:1;
   Vector2 delta=(data.position-pressPosition)/Mathf.Max(.01f,scale);
   if(pass){
    if(Mathf.Abs(delta.y)<ReturnDistance)PlayerView.Instance.SelectPass(0,pointer);
    else if(Mathf.Abs(delta.y)>=SelectDistance&&Mathf.Abs(delta.y)>Mathf.Abs(delta.x)*.7f)PlayerView.Instance.SelectPass(Mathf.Sign(delta.y)*Mathf.Lerp(.35f,1,Mathf.InverseLerp(SelectDistance,150,Mathf.Abs(delta.y))),pointer);
    return;
   }
   if(Mathf.Abs(delta.y)<ReturnDistance)PlayerView.Instance.SelectFinish(BasketballFinish.Shot,pointer);
   else if(Mathf.Abs(delta.y)>=SelectDistance&&Mathf.Abs(delta.y)>Mathf.Abs(delta.x)*.7f)PlayerView.Instance.SelectFinish(delta.y>0?BasketballFinish.Dunk:BasketballFinish.Layup,pointer);
  }
  public void OnPointerUp(PointerEventData data){if(!tracking||data.pointerId!=pointer)return;if(CanceledTouch(pointer)){CancelGesture();return;}CheckCancel(data);if(guide&&(guide.Choice==BasketballDragChoice.Outside||guide.Choice==BasketballDragChoice.Cancel)||!guide&&cancelArea&&RectTransformUtility.RectangleContainsScreenPoint(cancelArea,data.position,data.pressEventCamera)){CancelGesture();return;}if(pressed&&PlayerView.Instance){if(pass)PlayerView.Instance.EndPass(pointer);else PlayerView.Instance.EndShot(pointer);}pressed=tracking=false;pointer=int.MinValue;if(guide)guide.End();else if(cancelArea)cancelArea.gameObject.SetActive(false);}
  static bool CanceledTouch(int id){var module=EventSystem.current?EventSystem.current.currentInputModule as StandaloneInputModule:null;var input=module?module.inputOverride:null;int count=input?input.touchCount:Input.touchCount;for(int i=0;i<count;i++){var touch=input?input.GetTouch(i):Input.GetTouch(i);if(touch.fingerId==id)return touch.phase==TouchPhase.Canceled;}return false;}
  public void CancelGesture(){if(pressed&&PlayerView.Instance){if(pass)PlayerView.Instance.CancelPass(pointer);else PlayerView.Instance.CancelShot(pointer);}pressed=tracking=false;pointer=int.MinValue;if(guide)guide.End();else if(cancelArea)cancelArea.gameObject.SetActive(false);}
  void ResetGesture()=>CancelGesture();
  public void OnCancel(BaseEventData data)=>ResetGesture();
  public void OnEndDrag(PointerEventData data){if(tracking&&data.pointerId==pointer)ResetGesture();}
  void OnDisable(){ResetGesture();}
  void OnApplicationFocus(bool focused){if(!focused)ResetGesture();}
  void OnApplicationPause(bool paused){if(paused)ResetGesture();}
 }
}
