using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class BasketballShootButton:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,IInitializePotentialDragHandler,IBeginDragHandler,IDragHandler {
  public Button button;public Text label,cancelLabel;public RectTransform cancelArea;public bool pass;
  int pointer=int.MinValue;bool pressed,tracking;Vector2 pressPosition;
  public const float SelectDistance=48,ReturnDistance=26;
  void Update()=>Refresh();
  public void Refresh(){
   var view=PlayerView.Instance;var ball=BasketballBall.Active;
   button.interactable=view&&view.active&&ball&&ball.CanShoot(view.target)&&(pass?!view.ShotCharging:!view.PassCharging);
   bool holding=view&&(pass?view.PassCharging:view.ShotCharging);
   if(tracking&&(!button.interactable||!view))ResetGesture();else if(pressed&&!holding)CancelGesture();
   if(cancelArea)cancelArea.gameObject.SetActive(tracking||holding);
   if(!tracking&&holding&&cancelLabel)cancelLabel.text=Application.isMobilePlatform?"Cancel":"Cancel [X]";
   label.fontSize=22;
   if(pass){label.text=holding?$"{BasketballPassRules.Name(view.PassBend)}\n{Mathf.RoundToInt(view.PassPower*100)}%":ball&&ball.ActionQueued?"Passing...":GameButtonStyle.Caption("Pass","Q");return;}
   label.text=view&&view.target&&view.target.BasketballFreeRoam?"Free roam":holding?view.ShotFinish.ToString()+"\nRelease":ball&&ball.ActionQueued?"Gathering...":GameButtonStyle.Caption("Shoot","E");
  }
  public void OnPointerDown(PointerEventData data){if(data.button!=PointerEventData.InputButton.Left||!button.interactable||tracking||!PlayerView.Instance)return;if(pass?PlayerView.Instance.BeginPass(data.pointerId):PlayerView.Instance.BeginShot(data.pointerId)){pressed=tracking=true;pointer=data.pointerId;pressPosition=data.position;if(cancelLabel)cancelLabel.text="Cancel";if(cancelArea)cancelArea.gameObject.SetActive(true);}}
  public void OnInitializePotentialDrag(PointerEventData data){data.useDragThreshold=false;}
  public void OnBeginDrag(PointerEventData data){CheckCancel(data);}
  public void OnDrag(PointerEventData data){CheckCancel(data);}
  public void OnPointerExit(PointerEventData data){CheckCancel(data);}
  void CheckCancel(PointerEventData data){
   if(!tracking||!pressed||data.pointerId!=pointer)return;
   if(cancelArea&&RectTransformUtility.RectangleContainsScreenPoint(cancelArea,data.position,data.pressEventCamera)){CancelGesture();return;}
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
  public void OnPointerUp(PointerEventData data){if(!tracking||data.pointerId!=pointer)return;CheckCancel(data);if(pressed&&PlayerView.Instance){if(pass)PlayerView.Instance.EndPass(pointer);else PlayerView.Instance.EndShot(pointer);}pressed=tracking=false;if(cancelArea)cancelArea.gameObject.SetActive(false);}
  public void CancelGesture(){if(PlayerView.Instance){if(pass)PlayerView.Instance.CancelPass();else PlayerView.Instance.CancelShot();}pressed=false;if(cancelLabel)cancelLabel.text="Cancelled";}
  void ResetGesture(){if(pressed)CancelGesture();tracking=false;if(cancelArea)cancelArea.gameObject.SetActive(false);}
  void OnDisable(){ResetGesture();}
  void OnApplicationFocus(bool focused){if(!focused)ResetGesture();}
  void OnApplicationPause(bool paused){if(paused)ResetGesture();}
 }
}
