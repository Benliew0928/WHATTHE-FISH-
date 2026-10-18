using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 // Includes dynamically created drawers/results and keyboard button activation.
 public sealed class GolfFishingButtonSound:MonoBehaviour,IPointerDownHandler,IPointerUpHandler {
  Button button;float pressed=-10;
  void Awake(){button=GetComponent<Button>();if(button)button.onClick.AddListener(Click);}
  void Click(){if(Time.unscaledTime-pressed<.7f)return;GolfFishingAudio.PlayMenu(GolfFishingAudio.ButtonCue(name));}
  public void OnPointerDown(PointerEventData e){if(e.button!=PointerEventData.InputButton.Left||!button||!button.IsInteractable()||!GolfFishingAudio.Current)return;pressed=Time.unscaledTime;GolfFishingAudio.PlayMenu(GolfFishingAudio.ButtonCue(name));}
  public void OnPointerUp(PointerEventData e){if(e.button==PointerEventData.InputButton.Left&&Time.unscaledTime-pressed<5)GolfFishingAudio.PointerReleased();}
  void OnDestroy(){if(button)button.onClick.RemoveListener(Click);}
 }
}
