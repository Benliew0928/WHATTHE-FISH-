using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class KickButton:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,IInitializePotentialDragHandler,IBeginDragHandler,IDragHandler {
  public Button button;
  public Text label,cancelLabel;
  public RectTransform cancelArea;
  bool tracking,holding;int pointer;
  void Update(){var actor=AppRoot.Instance?AppRoot.Instance.LocalAthlete:null;button.interactable=actor&&actor.KickReady;var view=PlayerView.Instance;if(tracking&&(!button.interactable||!view))ResetGesture();else if(holding&&!view.Charging)CancelShot();if(label)label.text=view&&view.Charging?$"Kick {Mathf.RoundToInt(view.Charge*100)}%":(Application.isMobilePlatform?"Kick":"Kick [F]")+"\nHold to charge";}
  public void OnPointerDown(PointerEventData data){if(tracking||data.button!=PointerEventData.InputButton.Left||!button.interactable||!PlayerView.Instance)return;pointer=data.pointerId;if(!PlayerView.Instance.BeginKick(pointer))return;tracking=holding=true;SetCancelLabel(false);if(cancelArea)cancelArea.gameObject.SetActive(true);}
  public void OnInitializePotentialDrag(PointerEventData data){data.useDragThreshold=false;}
  // Keeping drag and press on this object preserves the original finger's release event.
  public void OnBeginDrag(PointerEventData data){CheckCancel(data);}
  public void OnDrag(PointerEventData data){CheckCancel(data);}
  public void OnPointerExit(PointerEventData data){CheckCancel(data);}
  public void OnPointerUp(PointerEventData data){if(!tracking||data.pointerId!=pointer)return;CheckCancel(data);if(holding&&PlayerView.Instance)PlayerView.Instance.EndKick(pointer);holding=tracking=false;if(cancelArea)cancelArea.gameObject.SetActive(false);}
  void CheckCancel(PointerEventData data){if(tracking&&holding&&data.pointerId==pointer&&cancelArea&&RectTransformUtility.RectangleContainsScreenPoint(cancelArea,data.position,data.pressEventCamera))CancelShot();}
  // Cancel only this charge. Never release/reacquire the ball or queue a kick.
  void CancelShot(){if(holding&&PlayerView.Instance)PlayerView.Instance.CancelKick(pointer);holding=false;SetCancelLabel(true);}
  void SetCancelLabel(bool cancelled){if(!cancelLabel)return;bool chinese=cancelLabel.font&&cancelLabel.font.HasCharacter('取')&&cancelLabel.font.HasCharacter('消')&&cancelLabel.font.HasCharacter('已');cancelLabel.text=chinese?(cancelled?"已取消":"× 取消"):(cancelled?"Cancelled":"× Cancel");}
  void ResetGesture(){CancelShot();tracking=false;if(cancelArea)cancelArea.gameObject.SetActive(false);}
  void OnDisable(){ResetGesture();}
  void OnApplicationFocus(bool focused){if(!focused)ResetGesture();}
  void OnApplicationPause(bool paused){if(paused)ResetGesture();}
 }
}
