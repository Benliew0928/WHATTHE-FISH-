using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 public sealed partial class FootballHUD {
  RectTransform customizePage;Button customizeNav;
  void BuildCustomization(float left,float width){
   customizeNav=FootballUIStyle.Button(settingsPanel,"Customize navigation","Customize",20,255,left-54,53,()=>SelectMenu(3),BasketballUIStyle.Paper,13);
   customizeNav.navigation=new Navigation{mode=Navigation.Mode.Automatic};
   customizePage=CoveUI.Rect(settingsPanel,"Customize page",left,82,width,230);
   BasketballUIStyle.Text(customizePage,"Your football trail",0,0,width,27,18,true);
   BasketballUIStyle.Text(customizePage,"Only you see this choice, whoever kicks. Saved automatically.",0,29,width,30,11);
   var cards=CoveUI.Rect(customizePage,"Trail choices",0,65,width,164);
   FootballTrailPicker.Create(cards,width,164,true);
  }
 }
}
