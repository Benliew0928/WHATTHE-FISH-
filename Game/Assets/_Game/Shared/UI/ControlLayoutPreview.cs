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
   void Add(string id,string label,float x,float y){
    var r=new GameObject("Preview "+id,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(frame,false);
    r.anchorMin=r.anchorMax=new Vector2(x/800,y/450);r.anchoredPosition=Vector2.zero;
    r.sizeDelta=ControlDesigns.Size(sport,id)*unit*(sport==SportId.Basketball||sport==SportId.Football?1:.5f);
    ControlLayoutVisuals.Preview(r,sport,id);editor.Register(id,label,r);
   }
   if(sport==SportId.Basketball){
    Add("move","Move",82,85);Add("shoot","Shoot / Guard",715,100);Add("pass","Pass / Steal",606,70);Add("jump","Jump",745,206);Add("ability","Ability",623,213);
   }else if(sport==SportId.Football){
    Add("move","Move",82,84);Add("shoot","Shoot / Pressure",630,220.5f);Add("dash","Dash",715,97);Add("pass","Pass",747,220);Add("tackle","Tackle",601,87);Add("ability","Ability",508,87);
   }else if(sport==SportId.Golf){
    var c=ControlDesigns.GolfLayout;float m=c.margin*.5f,gap=c.gap*.5f,side=m+(c.actionSize.x+2)*.5f;
    Add("move","Move",m+c.joystickSize.x*.25f,m+c.joystickSize.y*.25f);
    Add("jump","Jump",800-m-ControlDesigns.Size(sport,"power").x*.25f,m+ControlDesigns.Size(sport,"power").y*.5f+gap+10+c.jumpSize.y*.25f);
    Add("power","Shot power",737,102);Add("aim","Aim / Cancel",800-side-c.aimSize.x*.25f,m+c.returnSize.y*.5f+gap+c.aimSize.y*.25f);
    Add("start","Start game",400,450-m-c.startSize.y*.25f);
    float cartX=800-side-c.cartSize.x*.25f,cartY=m+c.returnSize.y*.5f+gap+c.aimSize.y*.5f+gap+c.cartSize.y*.25f;
    Add("buggy","Buggy / Recall",cartX,cartY);Add("drive","Drive / Exit",cartX,cartY+c.cartSize.y*.5f+gap);
   }else{
    Add("move","Move",54,54);Add("jump","Jump",731,170);Add("cast","Cast / Reel right",731,69);Add("left","Reel left",65,65);Add("cancel","Cancel cast",626,29);
    var c=ControlDesigns.FishingLayout;Add("ability","Round skill",731-c.actionSize.x*.25f-c.skillButtonSize.x*.25f-c.skillButtonGap.x*.5f,69+c.actionSize.y*.25f-c.skillButtonSize.y*.25f);
   }
   editor.Open(menu);return editor;
  }
 }
}
