using System;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Menu previews contain presentation rectangles only; no match, input or network objects.
 public static class ControlLayoutPreview {
  public static ControlLayoutEditor Open(RectTransform parent,SportId sport,Sprite art,Func<GameObject> menu){
   var frame=new GameObject(sport+" layout preview",typeof(RectTransform)).GetComponent<RectTransform>();
   frame.SetParent(parent,false);CoveUI.Stretch(frame);
   var canvas=frame.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=299;
   var background=frame.gameObject.AddComponent<Image>();background.color=CoveUI.Hex("0C2434");background.raycastTarget=false;
   if(art){
    var picture=new GameObject("Sport preview",typeof(RectTransform),typeof(Image)).GetComponent<Image>();picture.transform.SetParent(frame,false);
    CoveUI.Stretch(picture.rectTransform);picture.sprite=art;picture.preserveAspect=true;picture.color=new Color(1,1,1,.28f);picture.raycastTarget=false;
   }
   var editor=ControlLayoutEditor.Create(frame,sport,true);
   float unit=frame.rect.height/450;
   void Add(string id,string label,float x,float y,float w,float h,Sprite icon){
    var r=new GameObject("Preview "+id,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(frame,false);
    r.anchorMin=r.anchorMax=new Vector2(x/800,y/450);r.anchoredPosition=Vector2.zero;r.sizeDelta=new Vector2(w,h)*unit;
    ControlLayoutVisuals.Preview(r,sport,id);
    editor.Register(id,label,r,icon);
   }
   Sprite Icon(BasketballIcon icon)=>BasketballUIStyle.Icon(icon);
   if(sport==SportId.Basketball){
    Add("move","Move",82,85,108,108,Icon(BasketballIcon.Move));
    Add("shoot","Shoot / Guard",715,100,106,106,Icon(BasketballIcon.Shoot));
    Add("pass","Pass / Steal",606,70,80,80,Icon(BasketballIcon.Pass));
    Add("jump","Jump",745,206,64,64,Icon(BasketballIcon.Jump));
    Add("ability","Ability",623,213,80,80,SportsUIStyle.PropIcon(SportsProp.Turbo));
   }else if(sport==SportId.Football){
    Add("move","Move",82,84,108,108,Icon(BasketballIcon.Move));
    Add("shoot","Shoot / Pressure",630,220.5f,80,80,FootballUIStyle.Icon(FootballIcon.Ball));
    Add("dash","Dash",715,97,108,108,FootballUIStyle.Icon(FootballIcon.Dash));
    Add("pass","Pass",747,220,66,72,FootballUIStyle.Icon(FootballIcon.Target));
    Add("tackle","Tackle",601,87,76,79,FootballUIStyle.Icon(FootballIcon.Tackle));
    Add("ability","Ability",508,87,80,80,FootballPropHUD.Icon(SportsProp.Wall));
   }else if(sport==SportId.Golf){
    var config=Resources.Load<GolfHUDLayout>("GolfUILayout");
    float m=config.margin*.5f,gap=config.gap*.5f,side=m+(config.actionSize.x+2)*.5f;
    Add("move","Move",m+config.joystickSize.x*.25f,m+config.joystickSize.y*.25f,config.joystickSize.x*.5f,config.joystickSize.y*.5f,Icon(BasketballIcon.Move));
    Add("jump","Jump",800-m-52.5f,m+175+gap+10+config.jumpSize.y*.25f,config.jumpSize.x*.5f,config.jumpSize.y*.5f,Resources.Load<Sprite>("GolfUI/Badges/Jump"));
    Add("power","Shot power",737,102,105,175,Resources.Load<Sprite>("GolfUI/PowerSlider/Knob"));
    Add("aim","Aim / Cancel",800-side-config.aimSize.x*.25f,m+config.returnSize.y*.5f+gap+config.aimSize.y*.25f,config.aimSize.x*.5f,config.aimSize.y*.5f,Icon(BasketballIcon.Eye));
    Add("start","Start game",400,450-m-config.startSize.y*.25f,config.startSize.x*.5f,config.startSize.y*.5f,Resources.Load<Sprite>("GolfUI/StartButton/StartGame"));
    float cartX=800-side-config.cartSize.x*.25f,cartY=m+config.returnSize.y*.5f+gap+config.aimSize.y*.5f+gap+config.cartSize.y*.25f;
    Add("buggy","Buggy / Recall",cartX,cartY,config.cartSize.x*.5f,config.cartSize.y*.5f,Resources.Load<Sprite>("GolfUI/CartButtons/Buggy"));
    Add("drive","Drive / Exit",cartX,cartY+config.cartSize.y*.5f+gap,config.cartSize.x*.5f,config.cartSize.y*.5f,Resources.Load<Sprite>("GolfUI/CartButtons/Drive"));
   }else{
    Add("move","Move",54,54,88,88,Icon(BasketballIcon.Move));
    Add("jump","Jump",731,170,64,64,Icon(BasketballIcon.Jump));
    Add("cast","Cast / Reel right",731,69,118,118,Resources.Load<Sprite>("FishingUI/ReelButton"));
    Add("left","Reel left",65,65,109,109,Icon(BasketballIcon.Move));
    Add("cancel","Cancel cast",626,29,75,38,Icon(BasketballIcon.Close));
    Add("ability","Round skill",622,69,100,100,FishingGearArt.Icon(0));
   }
   editor.Open(menu);return editor;
  }
 }
}
