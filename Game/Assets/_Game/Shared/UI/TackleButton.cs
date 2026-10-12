using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 // Trigger on pointer down, not release; a held finger never repeats a tackle.
 public sealed class TackleButton:MonoBehaviour,IPointerDownHandler {
  public bool illustrated;public Text label;public Button button;
  void Update()=>Refresh();
  public void Refresh(){
   var app=AppRoot.Instance;var athlete=app?app.LocalAthlete:null;
   bool ready=PlayerView.Instance&&!PlayerView.Instance.InputBlocked&&athlete&&athlete.TackleReady;button.interactable=ready;
   float cooldown=athlete?athlete.TackleCooldown:0;
   if(illustrated)return;
   label.text=cooldown>.05f?$"Tackle\n{cooldown:F1}s":Application.isMobilePlatform?"Tackle":"Tackle\nE";
  }
  public void OnPointerDown(PointerEventData data){if(data.button==PointerEventData.InputButton.Left&&button.interactable)PlayerView.Instance.RequestTackle();}
 }
}
