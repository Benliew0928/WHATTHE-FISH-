using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 // Input only; the match authority validates and counts every released swing.
 public sealed class GolfSwingButton:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler {
  public Button button,startButton,aimButton;public Text label,startLabel,aimLabel;
  bool holding;int pointer;
  void Update(){
   var view=PlayerView.Instance;var match=GolfMatchManager.Instance;
   button.interactable=view&&view.GolfAiming&&match&&match.CanStrike(view.target,match.Ball(view.AimedGolfOwner));
   string shot=view&&view.GolfMode==GolfShotMode.Putt?"Putt":"Swing";
   label.text=match&&match.Starting?$"Wait\n{Mathf.CeilToInt((float)match.StartRemaining)}s":view&&view.GolfCharging?$"{shot}\n{Mathf.RoundToInt(view.GolfDisplayedCharge*100)}%":view&&view.GolfAimRequested?shot:"Aim first";
   if(holding&&(!button.interactable||!view.GolfCharging))Cancel();
   if(aimButton){aimButton.gameObject.SetActive(match&&match.Context&&match.State.Running);aimButton.interactable=view&&match&&!match.Starting&&(view.GolfAimRequested||view.CanAimGolf);if(aimLabel)aimLabel.text=match&&match.Starting?$"{Mathf.CeilToInt((float)match.StartRemaining)}s":view&&view.GolfAimRequested?"Cancel":view&&view.CanAimGolf?"Aim":"Near ball\nto aim";}
   if(startButton){startButton.gameObject.SetActive(match&&match.Authority&&match.Context&&!match.State.Running);startButton.interactable=match&&match.CanStart;if(startLabel)startLabel.text="Start game";}
  }
  public void OnPointerDown(PointerEventData e){if(holding||e.button!=PointerEventData.InputButton.Left||!button.interactable||!PlayerView.Instance)return;pointer=e.pointerId;holding=PlayerView.Instance.BeginGolfSwing(pointer);}
  public void OnPointerUp(PointerEventData e){if(!holding||e.pointerId!=pointer)return;PlayerView.Instance?.EndGolfSwing(pointer);holding=false;}
  public void OnPointerExit(PointerEventData e){if(holding&&e.pointerId==pointer)Cancel();}
  void Cancel(){if(holding)PlayerView.Instance?.CancelGolfSwing(pointer);holding=false;}
  void OnDisable(){Cancel();}
 }
}
