using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 // Pointer-down matches Space; holding a finger cannot queue repeated jumps.
 public sealed class JumpButton:MonoBehaviour,IPointerDownHandler {
  public Button button;public Text label;
  void Update(){var view=PlayerView.Instance;button.interactable=view&&view.active&&view.target&&view.target.CanRequestJump;label.text=Application.isMobilePlatform?"Jump":"Jump [Space]";}
  public void OnPointerDown(PointerEventData data){if(data.button==PointerEventData.InputButton.Left&&button.interactable)PlayerView.Instance.RequestJump();}
 }
}
