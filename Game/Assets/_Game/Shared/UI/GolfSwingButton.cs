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
   label.text=view&&view.GolfCharging?$"{shot}\n{Mathf.RoundToInt(view.GolfDisplayedCharge*100)}%":view&&view.GolfAimRequested?GameButtonStyle.Caption(shot,"F"):"Aim first";
   if(holding&&(!button.interactable||!view.GolfCharging))Cancel();
   if(aimButton){aimButton.gameObject.SetActive(match&&match.Context&&match.State.Running);aimButton.interactable=view&&(view.GolfAimRequested||view.CanAimGolf);aimLabel.text=view&&view.GolfAimRequested?(Application.isMobilePlatform?"Cancel":"Cancel\nX"):view&&view.CanAimGolf?(Application.isMobilePlatform?"Aim":"Aim\nG"):"Near ball\nto aim";}
   if(startButton){startButton.gameObject.SetActive(match&&match.Authority&&match.Context&&!match.State.Running);startButton.interactable=match&&match.CanStart;startLabel.text=match&&match.State.Phase==GolfMatchPhase.Ended?"New golf match":"Start golf match";}
  }
  public void OnPointerDown(PointerEventData e){if(holding||e.button!=PointerEventData.InputButton.Left||!button.interactable||!PlayerView.Instance)return;pointer=e.pointerId;holding=PlayerView.Instance.BeginGolfSwing(pointer);}
  public void OnPointerUp(PointerEventData e){if(!holding||e.pointerId!=pointer)return;PlayerView.Instance?.EndGolfSwing(pointer);holding=false;}
  public void OnPointerExit(PointerEventData e){if(holding&&e.pointerId==pointer)Cancel();}
  void Cancel(){if(holding)PlayerView.Instance?.CancelGolfSwing(pointer);holding=false;}
  void OnDisable(){Cancel();}
 }
}
