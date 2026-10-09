using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Reuse the menu's small procedural plates, typography and pointer feedback.
 // No imported textures, per-frame hierarchy scans or action listeners.
 public sealed class GameButtonStyle:MonoBehaviour {
  Button button;CovePlate plate;Text[] labels;bool previous=true;CoveFeedback feedback;FootballActionSlot football;BasketballShootButton basketball;bool lastAttack;
  public static string Caption(string action,string key)=>action+"\n"+(Application.isMobilePlatform?"Hold":key);
  public static void Apply(Transform parent){foreach(var b in parent.GetComponentsInChildren<Button>(true))Apply(b);}
  public static void Apply(Button b){
   if(b.GetComponent<GameButtonStyle>()||b is CoveButton)return;
   var style=b.gameObject.AddComponent<GameButtonStyle>();style.button=b;
   var rect=(RectTransform)b.transform;var image=b.GetComponent<Image>();
   var color=image?image.color:CoveUI.Mint;
   if(color.r+color.g+color.b<1.2f)color=CoveUI.Sky;
   else if(color.r>color.g*1.15f)color=CoveUI.Gold;
   else if(color.r>.85f&&color.g>.85f)color=CoveUI.Cream;
   else color=CoveUI.Mint;
   style.plate=CoveUI.Plate(rect,"Button face",0,0,rect.rect.width,rect.rect.height,color,Mathf.Min(rect.rect.width,rect.rect.height)/2,2,true);
   CoveUI.Stretch(style.plate.rectTransform);style.plate.transform.SetAsFirstSibling();style.plate.raycastTarget=true;
   // Keep the touch target fixed while only its contents animate.
   if(image){image.color=Color.clear;image.raycastTarget=true;}
   b.targetGraphic=style.plate;b.transition=Selectable.Transition.None;
   var feedback=b.GetComponent<CoveFeedback>();if(!feedback)feedback=b.gameObject.AddComponent<CoveFeedback>();feedback.plate=style.plate;feedback.pressScale=.91f;feedback.releaseBounce=.045f;style.feedback=feedback;
   style.football=b.GetComponent<FootballActionSlot>();style.basketball=b.GetComponent<BasketballShootButton>();style.lastAttack=!(style.football?style.football.Attacking:style.basketball&&style.basketball.enabled);
   style.labels=b.GetComponentsInChildren<Text>(true);
   foreach(var label in style.labels){
    // Keep the dedicated Chinese font used by the cart and aiming controls.
    if(label.font&&label.font.name=="LegacyRuntime"&&CoveUI.TextFont)label.font=CoveUI.TextFont;
    label.color=CoveUI.Ink;label.raycastTarget=false;
    if(rect.rect.height>=90){label.rectTransform.anchorMin=label.rectTransform.anchorMax=label.rectTransform.pivot=new Vector2(.5f,.5f);label.rectTransform.anchoredPosition=Vector2.zero;label.rectTransform.sizeDelta=new Vector2(rect.rect.width-26,rect.rect.height-32);label.fontSize=22;label.resizeTextForBestFit=true;label.resizeTextMinSize=17;label.resizeTextMaxSize=22;}
   }
   var visual=new GameObject("Button visual",typeof(RectTransform)).GetComponent<RectTransform>();visual.SetParent(rect,false);CoveUI.Stretch(visual);
   style.plate.transform.SetParent(visual,false);style.plate.raycastTarget=false;
   foreach(var label in style.labels)label.transform.SetParent(visual,false);
   feedback.UseVisual(visual);
  }
  void LateUpdate(){
   if(football||basketball){
    bool attack=football?football.Attacking:basketball.enabled;
    if(attack!=lastAttack||Time.frameCount<3){lastAttack=attack;feedback.OnCancel(null);feedback.SetTint(attack?CoveUI.Gold:CoveUI.Sky);}
   }
   bool enabled=button.IsInteractable();if(enabled==previous)return;previous=enabled;
   foreach(var label in labels)if(label)label.color=enabled?CoveUI.Ink:CoveUI.Muted;
  }
 }
}
