using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 // Pointer-down matches Space; holding a finger cannot queue repeated jumps.
 public sealed class JumpButton:MonoBehaviour,IPointerDownHandler {
  public Button button;public Text label;
  void Update(){var view=PlayerView.Instance;button.interactable=view&&view.active&&!view.GolfAimRequested&&view.target&&view.target.CanRequestJump;bool defense=view&&view.BasketballRole==BasketballRole.Defense;if(defense)button.interactable=button.interactable&&BasketballBall.Active.CanBlock(view.target);label.text=FishingGame.Instance&&FishingGame.Instance.Context?"JUMP":defense?(Application.isMobilePlatform?"Jump Block":"Jump block\nSpace"):(Application.isMobilePlatform?"Jump":"Jump\nSpace");}
  public void OnPointerDown(PointerEventData data){if(data.button==PointerEventData.InputButton.Left&&button.interactable)PlayerView.Instance.RequestJump();}
 }
}
