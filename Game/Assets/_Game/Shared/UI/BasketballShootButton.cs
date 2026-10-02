using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 // Same edge-triggered touch control as jump/tackle, with a possession hint.
 public sealed class BasketballShootButton:MonoBehaviour,IPointerDownHandler {
  public Button button;public Text label;public bool pass;
  void Update(){
   var view=PlayerView.Instance;var ball=BasketballBall.Active;
   button.interactable=view&&view.active&&ball&&ball.CanShoot(view.target);
   label.text=button.interactable?(pass?(Application.isMobilePlatform?"Pass":"Pass [Q]"):(Application.isMobilePlatform?"Shoot":"Shoot [E]")):ball&&ball.Held?"Ball held":"Walk to ball";
  }
  public void OnPointerDown(PointerEventData data){if(data.button==PointerEventData.InputButton.Left&&button.interactable){if(pass)PlayerView.Instance.RequestPass();else PlayerView.Instance.RequestShoot();}}
 }
}
