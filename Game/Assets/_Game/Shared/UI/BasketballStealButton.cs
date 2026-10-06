using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class BasketballStealButton:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler {
  public Button button;public Text label;int pointer=int.MinValue;bool pressed;
  CanvasGroup group;
  void Awake(){group=gameObject.AddComponent<CanvasGroup>();}
  void Update(){
   var view=PlayerView.Instance;var ball=BasketballBall.Active;
   bool visible=view&&view.BasketballRole==BasketballRole.Defense;group.alpha=visible?1:0;group.blocksRaycasts=visible;
   button.interactable=visible&&view.active&&ball&&ball.CanSteal(view.target);if(!visible&&pressed)Release(true);
   var motion=view&&view.target?view.target.BasketballMotion:null;
   label.text=view&&view.target&&view.target.BasketballFreeRoam?"Free roam":motion&&motion.Action==BasketballAction.Stripped?"Recovering":motion&&motion.Action==BasketballAction.Steal?"Swiping…":Application.isMobilePlatform?"Hold to steal":"Hold F to steal";
  }
  public void OnPointerDown(PointerEventData data){if(data.button!=PointerEventData.InputButton.Left||pressed||!button.interactable)return;if(PlayerView.Instance.BeginSteal(data.pointerId)){pointer=data.pointerId;pressed=true;}}
  public void OnPointerUp(PointerEventData data){if(pressed&&data.pointerId==pointer)Release(false);}
  public void OnPointerExit(PointerEventData data){if(pressed&&data.pointerId==pointer)Release(true);}
  void Release(bool cancel){if(PlayerView.Instance)PlayerView.Instance.EndSteal(pointer,cancel);pressed=false;}
  void OnDisable(){Release(true);}
 }
}
