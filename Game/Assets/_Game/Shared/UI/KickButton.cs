using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class KickButton:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,IInitializePotentialDragHandler,IBeginDragHandler,IDragHandler,IEndDragHandler,ICancelHandler {
  public Button button;public Text label,cancelLabel;public RectTransform cancelArea;public bool illustrated;public FootballKickGuide guide;
  bool tracking;int pointer=int.MinValue;
  public bool Tracking=>tracking;
  public bool CancelArmed {get;private set;}
  void Update()=>Refresh();
  public void Refresh(){var actor=AppRoot.Instance?AppRoot.Instance.LocalAthlete:null;var view=PlayerView.Instance;button.interactable=view&&view.active&&!view.InputBlocked&&actor&&actor.KickReady&&SportsPossession.Attacking(actor,SportId.Football);if(tracking&&(!button.interactable||!view.Charging))CancelGesture();if(!illustrated&&label)label.text=view&&view.Charging?$"Kick\n{Mathf.RoundToInt(view.Charge*100)}%":"Kick";}
  public void OnPointerDown(PointerEventData data){if(tracking||data.button!=PointerEventData.InputButton.Left||!button.interactable||!PlayerView.Instance||!PlayerView.Instance.BeginKick(data.pointerId))return;pointer=data.pointerId;tracking=true;CancelArmed=false;if(guide)guide.Begin();else if(cancelArea)cancelArea.gameObject.SetActive(true);}
  public void OnInitializePotentialDrag(PointerEventData data){data.useDragThreshold=false;}
  public void OnBeginDrag(PointerEventData data)=>Track(data);
  public void OnDrag(PointerEventData data)=>Track(data);
  public void OnPointerExit(PointerEventData data)=>Track(data);
  void Track(PointerEventData data){if(!tracking||data.pointerId!=pointer)return;CancelArmed=false;if(cancelArea&&RectTransformUtility.ScreenPointToLocalPointInRectangle(cancelArea,data.position,data.pressEventCamera,out var p)){var r=cancelArea.rect;r.xMin-=8;r.xMax+=8;r.yMin-=8;r.yMax+=8;CancelArmed=r.Contains(p);}if(guide)guide.Refresh();}
  public void OnPointerUp(PointerEventData data){
   if(!tracking||data.pointerId!=pointer)return;
   Track(data);if(CancelArmed||CanceledTouch(pointer)){CancelGesture();return;}
   if(PlayerView.Instance)PlayerView.Instance.EndKick(pointer);Finish();
  }
  static bool CanceledTouch(int id){var module=EventSystem.current?EventSystem.current.currentInputModule as StandaloneInputModule:null;var input=module?module.inputOverride:null;int count=input?input.touchCount:Input.touchCount;for(int i=0;i<count;i++){var touch=input?input.GetTouch(i):Input.GetTouch(i);if(touch.fingerId==id)return touch.phase==TouchPhase.Canceled;}return false;}
  public void CancelGesture(){if(tracking&&PlayerView.Instance)PlayerView.Instance.CancelKick(pointer);Finish();}
  void Finish(){tracking=CancelArmed=false;pointer=int.MinValue;if(guide)guide.End();else if(cancelArea)cancelArea.gameObject.SetActive(false);}
  public void OnEndDrag(PointerEventData data){if(tracking&&data.pointerId==pointer)CancelGesture();}
  public void OnCancel(BaseEventData data)=>CancelGesture();
  void OnDisable()=>CancelGesture();
  void OnApplicationFocus(bool focused){if(!focused)CancelGesture();}
  void OnApplicationPause(bool paused){if(paused)CancelGesture();}
 }
}
