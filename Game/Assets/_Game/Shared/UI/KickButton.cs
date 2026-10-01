using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed class KickButton:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler {
  public Button button;
  public Text label;bool holding;int pointer;
  void Update(){var actor=AppRoot.Instance?AppRoot.Instance.LocalAthlete:null;button.interactable=actor&&actor.KickReady;var view=PlayerView.Instance;if(label)label.text=view&&view.Charging?$"Kick {Mathf.RoundToInt(view.Charge*100)}%":(Application.isMobilePlatform?"Kick":"Kick [F]")+"\nHold to charge";}
  public void OnPointerDown(PointerEventData data){if(holding||data.button!=PointerEventData.InputButton.Left||!button.interactable||!PlayerView.Instance)return;pointer=data.pointerId;holding=PlayerView.Instance.BeginKick(pointer);}
  public void OnPointerUp(PointerEventData data){if(!holding||data.pointerId!=pointer)return;holding=false;if(PlayerView.Instance)PlayerView.Instance.EndKick(pointer);}
  public void OnPointerExit(PointerEventData data){if(holding&&data.pointerId==pointer)Cancel();}
  void OnDisable(){Cancel();}
  void Cancel(){if(holding&&PlayerView.Instance)PlayerView.Instance.CancelKick(pointer);holding=false;}
 }
}
