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
   var fillArea=Rect(touch,"Fill area",16,22,318,18);var fill=Plate(fillArea,"Teal fill",0,0,318,18,"76E6D3","37BCAF","31988F",9,false);CoveUI.Stretch(fill.rectTransform);slider.fillRect=fill.rectTransform;
   var handleArea=Rect(touch,"Handle area",16,31,318,0);var handle=Plate(handleArea,"Thumb",0,0,44,44,"FFFFFF","FFF0BD","4EAB9B",22,true);handle.rectTransform.pivot=Vector2.one*.5f;
   slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.navigation=new Navigation{mode=Navigation.Mode.Automatic};
   slider.SetValueWithoutNotify(value);amount.text=Mathf.RoundToInt(value*100)+"%";
   slider.onValueChanged.AddListener(v=>{save(v);amount.text=Mathf.RoundToInt(v*100)+"%";});
  }

  void OpenWelcomeInfo(){
   var card=UtilityPanel("Welcome info",WelcomeCopy("Your island getaway","欢迎来到运动岛","Percutian pulau anda"),WelcomeCopy("Four islands. One shared adventure.","四座岛屿，一起开启冒险。","Empat pulau. Satu pengembaraan bersama."),InfoButton,1040,760,WelcomeSymbol.Info);
   if(!card)return;
   InfoSport(card,SportId.Football,32,154,"D9F1CA",WelcomeCopy("Pass, tackle and score goals with your team.","传球、抢断，和队友一起进球。","Hantar, takel dan jaringkan gol bersama pasukan."));
   InfoSport(card,SportId.Basketball,536,154,"FFE4C6",WelcomeCopy("Pass, defend and shoot your way to victory.","配合传球、防守投篮，争取胜利。","Hantar, bertahan dan menjaring untuk menang."));
   InfoSport(card,SportId.Golf,32,320,"F0DFBA",WelcomeCopy("Aim, swing and finish five holes in fewer strokes.","瞄准挥杆，用更少杆数完成五个球洞。","Bidik, hayun dan tamatkan lima lubang dengan kurang pukulan."));
   InfoSport(card,SportId.Fishing,536,320,"BFDDF4",WelcomeCopy("Cast your line, reel in fish and collect points.","抛竿收线，钓起鱼儿赢取积分。","Lontar pancing, tarik ikan dan kumpulkan mata."));
   var together=Plate(card,"Play together overview",32,490,976,116,"DCF5E8","CBEADB","A8CDB9",24,false);
   CoveUI.Icon(together.transform,CoveSymbol.People,23,25,48,Ink);
   Label(together.transform,WelcomeCopy("Play solo or bring your crew","独自游玩，或约上朋友","Main sendiri atau bersama rakan"),92,5,858,44,28,true,Ink);
   Label(together.transform,WelcomeCopy("Create a room or join with a code. Travel together and enjoy one mini game at a time.","创建房间或输入房间码加入。大家一起前往岛屿，每次进行一个小游戏。","Cipta bilik atau sertai dengan kod. Pergi bersama dan main satu permainan mini pada satu masa."),92,49,858,61,22,false,Ink);
   Label(card,WelcomeCopy("Step-by-step tutorials await in the main menu after sign-in.","登录后，可在主菜单查看分步骤教程。","Tutorial langkah demi langkah tersedia di menu utama selepas log masuk."),36,623,968,50,22,false,Ink,TextAnchor.MiddleCenter);
   var version=Label(card,"Version "+Application.version,34,695,398,30,20,false,Ink);version.name="Game version";
   ActionButton(card,"Done welcome info","Done",810,684,198,52,"C9FFF0","82DDC4","57A996",CloseModal,WelcomeSymbol.None,26);
  }

  void InfoSport(Transform parent,SportId sport,float x,float y,string tint,string description){
   var tile=Plate(parent,"About "+sport,x,y,472,148,"FFFFFF",tint,"BDD5C4",24,false);
   var badge=Plate(tile.transform,"Sport badge",14,28,90,90,"FFFFFF",tint,"C2D6C6",27,false);
   var icon=Rect(badge.transform,"Sport icon",3,3,84,84).gameObject.AddComponent<RawImage>();
   icon.texture=Resources.Load<Texture2D>("WelcomeSportIcons/"+sport);icon.raycastTarget=false;
   Label(tile.transform,sport.ToString(),117,8,333,46,30,true,Ink);
   Label(tile.transform,description,117,62,333,72,23,false,Ink);
  }

 }
}
