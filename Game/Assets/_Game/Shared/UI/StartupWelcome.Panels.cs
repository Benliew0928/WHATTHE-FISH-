using System;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed partial class StartupWelcome {
  static string WelcomeCopy(string en,string zh,string ms)=>GameLocalization.Current==GameLanguage.SimplifiedChinese?zh:GameLocalization.Current==GameLanguage.Malay?ms:en;
  void OnApplicationPause(bool paused){if(paused)MenuPreferences.Save();}

  RectTransform UtilityPanel(string name,string title,string subtitle,Button source,float width,float height,WelcomeSymbol symbol){
   if(ModalOpen||leaving)return null;
   returnFocus=source;CreateModal();SetBaseInteractable(false);
   var rim=Plate(modal,name,(Width-width)/2,(Height-height)/2,width,height,"8CE9DA","26A99F","176B70",38,true);
   Plate(rim.transform,"Cream inset",7,7,width-14,height-14,"FFFCED","F5E9CC","BCE4CC",32,false);
   var badge=Plate(rim.transform,"Header badge",30,28,76,76,"FFF0A5","FFC969","E3AD4D",27,true);
   var icon=Rect(badge.transform,"Header icon",20,20,36,36).gameObject.AddComponent<WelcomeIcon>();icon.symbol=symbol;icon.color=Ink;icon.raycastTarget=false;
   Label(rim.transform,title,126,23,width-236,52,38,true,Ink);
   Label(rim.transform,subtitle,128,79,width-245,37,23,false,Ink);
   Plate(rim.transform,"Header divider",32,128,width-64,2,"CFE0CB","CFE0CB","CFE0CB",0,false);
   var close=ActionButton(rim.transform,"Close "+name,"",width-86,27,54,54,"FFFFFF","E2F5DE","89BEB0",CloseModal,WelcomeSymbol.None);
   CoveUI.Icon(close.transform,CoveSymbol.Close,17,17,20,Ink);close.Select();
   return rim.rectTransform;
  }

  void OpenWelcomeSettings(){
   var card=UtilityPanel("Welcome settings","Settings",WelcomeCopy("Make yourself comfortable.","调整到你喜欢的状态。","Sesuaikan mengikut keselesaan anda."),SettingsButton,860,712,WelcomeSymbol.Gear);
   if(!card)return;
   WelcomeVolume(card,"Master",WelcomeCopy("Master volume","总音量","Kelantangan utama"),MenuPreferences.MasterVolume,148,MenuPreferences.SetMaster);
   WelcomeVolume(card,"Game sounds",WelcomeCopy("Game sounds","游戏音效","Bunyi permainan"),MenuPreferences.SFXVolume,238,MenuPreferences.SetSFX);
   WelcomeVolume(card,"Ambience",WelcomeCopy("Ambience","环境音","Bunyi persekitaran"),MenuPreferences.AmbienceVolume,328,MenuPreferences.SetAmbience);
   WelcomeVolume(card,"Buttons",WelcomeCopy("Button sounds","按钮音效","Bunyi butang"),MenuPreferences.UIVolume,418,MenuPreferences.SetUI);
   var motion=Plate(card,"Motion preference",32,522,796,94,"F0F8E3","E1EED7","BCD9C2",22,false);
   Label(motion.transform,WelcomeCopy("Reduced motion","减少动态效果","Kurangkan gerakan"),22,7,514,44,26,true,Ink);
   Label(motion.transform,WelcomeCopy("Gentler menu movement and feedback.","减轻菜单动画和动态反馈。","Animasi menu yang lebih lembut."),22,54,514,28,20,false,Ink);
   Button toggle=null;
   void Refresh(){
    toggle.GetComponentInChildren<Text>().text=MenuPreferences.ReducedMotion?WelcomeCopy("On","开","Aktif"):WelcomeCopy("Off","关","Tidak aktif");
    var p=toggle.GetComponent<WelcomePlate>();p.top=LocalProfile.Hex(MenuPreferences.ReducedMotion?"A4F1DC":"FFFFFF");p.bottom=LocalProfile.Hex(MenuPreferences.ReducedMotion?"67CDB5":"E6EDDC");p.SetVerticesDirty();
   }
   toggle=ActionButton(motion.transform,"Welcome reduced motion","",570,17,202,60,"FFFFFF","E6EDDC","82B7A4",()=>{MenuPreferences.SetMotion(!MenuPreferences.ReducedMotion);MenuPreferences.Save();Refresh();},WelcomeSymbol.None,26);Refresh();
   Label(card,WelcomeCopy("Saved automatically · shared with your game","自动保存 · 与游戏内设置同步","Disimpan automatik · digunakan dalam permainan"),34,641,560,42,20,false,Ink);
   ActionButton(card,"Done welcome settings","Done",626,638,202,52,"C9FFF0","82DDC4","57A996",CloseModal,WelcomeSymbol.None,26);
  }

  void WelcomeVolume(Transform parent,string id,string caption,float value,float y,Action<float> save){
   var row=Plate(parent,id+" preference",32,y,796,80,"FFFFFF","FAF3DD","D8E1CB",21,false);
   Label(row.transform,caption,22,18,266,42,26,true,Ink);
   var amount=Label(row.transform,"",690,21,84,36,25,true,Ink,TextAnchor.MiddleRight);
   var touch=Rect(row.transform,id,310,9,350,62);var hit=touch.gameObject.AddComponent<Image>();hit.color=Color.clear;
   var track=Plate(touch,"Recessed track",0,17,350,28,"CDDFD0","E1E7D3","AEC6B5",14,false);
   var slider=touch.gameObject.AddComponent<Slider>();slider.direction=Slider.Direction.LeftToRight;
   // Fill the rail up to its inner border; only the thumb needs travel padding.
   var fillArea=Rect(touch,"Fill area",2,19,346,24);var fill=Plate(fillArea,"Teal fill",0,0,346,24,"76E6D3","37BCAF","31988F",12,false);CoveUI.Stretch(fill.rectTransform);slider.fillRect=fill.rectTransform;
   var handleArea=Rect(touch,"Handle area",16,31,318,0);var handle=Plate(handleArea,"Thumb",0,0,44,44,"FFFFFF","FFF0BD","4EAB9B",22,true);handle.rectTransform.pivot=Vector2.one*.5f;
   slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.navigation=new Navigation{mode=Navigation.Mode.Automatic};
   slider.SetValueWithoutNotify(value);amount.text=Mathf.RoundToInt(value*100)+"%";
   slider.onValueChanged.AddListener(v=>{save(v);amount.text=Mathf.RoundToInt(v*100)+"%";});
  }

 }
}
