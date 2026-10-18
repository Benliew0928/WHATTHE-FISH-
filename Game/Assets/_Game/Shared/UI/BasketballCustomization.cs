using UnityEngine;
namespace WhatTheFish {
 public sealed partial class BasketballHUD {
  RectTransform customizePanel;
  void BuildCustomization(){
   customizePanel=MenuPanel("Your basketball effects",650,390,BasketballIcon.Shoot,CloseMenu);
   BasketballUIStyle.Text(customizePanel,"Your choice, whoever has the ball. Saved on this device.",20,70,610,25,12);
   var cards=CoveUI.Rect(customizePanel,"Basketball trail choices",20,106,610,214);BasketballTrailPicker.Create(cards,610,214,true);
   Footer(customizePanel,334,650,50);BasketballUIStyle.Button(customizePanel,"Back to settings","‹ Settings",20,345,125,32,ShowSettings,null,13);
   BasketballUIStyle.Button(customizePanel,"Back to court","Back to court",485,344,145,34,CloseMenu,CoveUI.Gold);
  }
  public void ShowCustomization()=>OpenMenu(customizePanel);
 }
}
