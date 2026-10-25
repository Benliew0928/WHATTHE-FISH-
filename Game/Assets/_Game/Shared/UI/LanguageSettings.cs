using System;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // The picker lives inside the settings panel: closing that panel also closes
 // its input surface, without changing the match or its input-blocking owner.
 public sealed class LanguageSettings:MonoBehaviour {
  static LanguageSettings active;
  public static bool DismissTop(){if(!active||!active.picker)return false;active.Dismiss();return true;}
  GameObject picker;Action changed;
  public static Button Add(RectTransform parent,float x,float y,float width,float height,int points=16,Action changed=null){
   var b=BasketballUIStyle.Button(parent,"Language settings","Language",x,y,width,height,null,CoveUI.Sky,points);
   Attach(b,parent,changed);return b;
  }
  public static void Attach(Button button,RectTransform parent,Action changed=null){var owner=button.gameObject.AddComponent<LanguageSettings>();owner.changed=changed;button.onClick.AddListener(()=>owner.Open(parent));}
  void Open(RectTransform parent){
   if(picker)return;active=this;
   var shade=BasketballUIStyle.Plate(parent,"Language selection",0,0,parent.rect.width,parent.rect.height,new Color(.04f,.15f,.19f,.92f),24,0);CoveUI.Stretch(shade.rectTransform);shade.raycastTarget=true;picker=shade.gameObject;
   float scale=Mathf.Min(parent.rect.width/650,parent.rect.height/422);
   var card=BasketballUIStyle.Plate(shade.transform,"Language choices",0,0,610,390,CoveUI.Cream,24,2,true);MenuDesigns.Dialog(card);card.raycastTarget=true;
   var r=card.rectTransform;r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=Vector2.zero;r.localScale=Vector3.one*scale;
   BasketballUIStyle.Text(r,"Language",20,14,570,40,25,true,TextAnchor.MiddleCenter);
   BasketballUIStyle.Text(r,"Choose your language. Changes apply throughout the game.",30,58,550,40,15,false,TextAnchor.MiddleCenter);
   string[] names={"English","简体中文","Bahasa Melayu"};
   for(int i=0;i<3;i++){
    var language=(GameLanguage)i;
    var b=BasketballUIStyle.Button(r,"Settings language option "+i,names[i],40,112+i*68,530,56,()=>{GameLocalization.Select(language);Dismiss();changed?.Invoke();},GameLocalization.Current==language?CoveUI.Mint:CoveUI.Cream,22);
    GameLocalization.UserContent(b.GetComponentInChildren<Text>());MenuDesigns.Selected(b,GameLocalization.Current==language);
   }
   BasketballUIStyle.Button(r,"Close settings language","Back",180,326,250,46,Dismiss,CoveUI.Gold,20).Select();
  }
  void Dismiss(){if(active==this)active=null;if(!picker)return;picker.SetActive(false);Destroy(picker);picker=null;}
  void OnDisable()=>Dismiss();
 }
}
