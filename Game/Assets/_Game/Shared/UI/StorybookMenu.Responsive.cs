using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed partial class StorybookMenu {
  float layoutWidth=1600,layoutHeight=900;Vector2 appliedLayout;
  readonly Dictionary<RectTransform,Rect> authoredRects=new();
  public RectTransform DesignFrame=>design;
  public bool CompactLayout=>layoutWidth/layoutHeight<1.6f;
  void LayoutScreen(){
   if(!design)return;
   var area=ResponsiveScreen.ContentArea;bool adaptive=Page=="home"||Page=="sport";
   bool compact=area.width/area.height<1.6f;
   float scale=Mathf.Min(area.width/(adaptive&&compact?1200:1600),area.height/(adaptive&&compact?1100:900));
   layoutWidth=adaptive?area.width/scale:1600;layoutHeight=adaptive?area.height/scale:900;
   float canvasScale=design.GetComponentInParent<Canvas>().scaleFactor;
   design.sizeDelta=new Vector2(layoutWidth,layoutHeight);design.localScale=Vector3.one*(scale/canvasScale);
   design.anchoredPosition=(area.center-new Vector2(Screen.width,Screen.height)*.5f)/canvasScale;
   if(firstVisitDesign){firstVisitDesign.sizeDelta=design.sizeDelta;firstVisitDesign.localScale=design.localScale;firstVisitDesign.anchoredPosition=design.anchoredPosition;}
  }
  static Rect AuthoredRect(RectTransform r)=>new Rect(r.anchoredPosition.x-r.pivot.x*r.rect.width,-r.anchoredPosition.y-(1-r.pivot.y)*r.rect.height,r.rect.width,r.rect.height);
  static void Box(RectTransform r,float x,float y,float w,float h){
   if(!r)return;r.sizeDelta=new Vector2(w,h);r.anchoredPosition=new Vector2(x+r.pivot.x*w,-y-(1-r.pivot.y)*h);
  }
  static void Box(Transform parent,string name,float x,float y,float w,float h)=>Box(parent.Find(name) as RectTransform,x,y,w,h);
  void RememberLayout(){
   authoredRects.Clear();foreach(var r in content.GetComponentsInChildren<RectTransform>(true))authoredRects[r]=AuthoredRect(r);appliedLayout=Vector2.zero;
  }
  void ApplyPageLayout(){
   if(!content)return;var size=new Vector2(layoutWidth,layoutHeight);if(appliedLayout==size)return;appliedLayout=size;
   content.sizeDelta=size;
   if(Page=="home")LayoutHome();else if(Page=="sport")LayoutSport();
   if(toast)Box(toast.transform.parent as RectTransform,(layoutWidth-660)/2,layoutHeight-86,660,56);
   if(firstVisitDesign)DrawFirstVisit();
  }
  void Place(string name,float x,float y,float w,float h){
   var r=content.Find(name) as RectTransform;if(!r)return;Box(r,x,y,w,h);
   if(!r.GetComponent<Button>())return;
   // Native labels grow with their button; icons/corner treatment keep their size.
   if(authoredRects.TryGetValue(r,out var original))foreach(Transform child in r){
    var t=child.GetComponent<Text>();var c=child as RectTransform;
    if(t&&authoredRects.TryGetValue(c,out var b))Box(c,b.x,b.y,Mathf.Max(1,b.width+w-original.width),Mathf.Max(1,b.height+h-original.height));
   }
  }
  void LayoutHome(){
   float left=Mathf.Max(48,(layoutWidth-2200)/2),right=layoutWidth-left,inner=right-left;
   bool compact=CompactLayout;float divider=compact?230:151;
   Place("Official logo",left,0,205,151);
   if(compact){
    Place("Coin balance",right-520,35,224,68);Place("Rewards",right-276,35,276,68);
    Place("Friends icon",right-656,140,185,68);Place("Player profile",right-451,140,248,68);Place("Settings",right-183,140,183,68);
   }else{
    Place("Coin balance",left+250,48,224,68);Place("Rewards",left+494,48,276,68);
    Place("Friends icon",right-656,48,185,68);Place("Player profile",right-451,48,248,68);Place("Settings",right-183,48,183,68);
   }
   Place("Header divider",left,divider,inner,2);
   float bannerY=divider+25,bannerH=compact?176:128;
   Place("Play together",left,bannerY,inner,bannerH);
   var banner=content.Find("Play together");Box(banner,"ONE ROOM. FOUR SPORTS.",28,compact?14:40,compact?inner-56:inner-710,48);
   float roomY=bannerY+(compact?82:29);
   Place("Create room",right-653,roomY,300,70);Place("Join room",right-333,roomY,305,70);
   float titleY=bannerY+bannerH+18,cardY=titleY+68,utilityY=layoutHeight-170;
   Place("CHOOSE YOUR SPORT",left,titleY,inner,48);
   int columns=layoutWidth/layoutHeight<.8f?1:compact?2:4,rows=4/columns;float gap=24,cardW=(inner-gap*(columns-1))/columns,cardH=(utilityY-30-cardY-gap*(rows-1))/rows;
   for(int i=0;i<HomeSports.Length;i++)LayoutCard(HomeSports[i],left+i%columns*(cardW+gap),cardY+i/columns*(cardH+gap),cardW,cardH);
   float utilityW=(inner-24)/2;
   LayoutUtility("Customize venue",left,utilityY,utilityW);LayoutUtility("Tutorials",left+utilityW+24,utilityY,utilityW);
  }
  void LayoutCard(SportId sport,float x,float y,float w,float h){
   var card=content.Find("Island - "+sport) as RectTransform;if(!card)return;Box(card,x,y,w,h);
   if(h<w*.55f){
    float imageW=(h-4)*1.5f;
    Box(card,"Artwork window",2,2,imageW,h-4);Box(card.Find("Artwork window"),sport+" illustration",0,0,imageW,h-4);
    Box(card,"Sport accent",imageW+2,2,4,h-4);Box(card,sport.ToString().ToUpperInvariant(),imageW+24,20,w-imageW-42,h-80);Box(card,"Open sport",w-64,h-56,40,40);
    return;
   }
   float artH=Mathf.Min(w*2/3,h-76),imageH=(w-4)*2/3;
   Box(card,"Artwork window",2,2,w-4,artH);Box(card.Find("Artwork window"),sport+" illustration",0,(artH-imageH)/2,w-4,imageH);
   Box(card,"Sport accent",2,artH,w-4,4);Box(card,sport.ToString().ToUpperInvariant(),24,artH+14,w-104,44);Box(card,"Open sport",w-64,artH+27,40,40);
  }
  void LayoutUtility(string name,float x,float y,float w){
   Place(name,x,y,w,124);var r=content.Find(name);if(!r)return;
   foreach(Transform child in r){var icon=child.GetComponent<CoveIcon>();if(icon&&icon.symbol==CoveSymbol.Next)Box(child as RectTransform,w-55,49,26,26);}
  }
  void LayoutSport(){
   float left=Mathf.Max(48,(layoutWidth-2200)/2),inner=layoutWidth-2*left;bool stacked=layoutWidth/layoutHeight<1.2f;
   Place("Official logo",left,20,144,126);Place(app.SelectedSport.ToString().ToUpperInvariant(),left+171,49,inner-400,72);
   Place("Back",layoutWidth-left-170,50,170,66);
   float top=190,bottom=layoutHeight-140,heroW=stacked?inner:inner*.385f,heroH=stacked?Mathf.Min(550,(bottom-top)*.42f):bottom-top;
   var hero=content.Find("Sport hero") as RectTransform;Box(hero,left,top,heroW,heroH);
   float imageH=heroH-150;Box(hero,"Sport artwork",2,2,heroW-4,imageH);Box(hero,"LET'S PLAY.",30,imageH+38,heroW-60,70);
   float modeX=stacked?left:left+heroW+36,modeY=stacked?top+heroH+24:top,modeW=stacked?inner:inner-heroW-36,modeH=(bottom-modeY-24)/2;
   LayoutMode("Explore offline",modeX,modeY,modeW,modeH);LayoutMode("Play vs AI",modeX,modeY+modeH+24,modeW,modeH);
   Place("Sport help",left,layoutHeight-93,300,65);
  }
  void LayoutMode(string name,float x,float y,float w,float h){
   Place(name,x,y,w,h);var r=content.Find(name);if(!r)return;
   foreach(Transform child in r){
    var icon=child.GetComponent<CoveIcon>();if(icon&&icon.symbol==CoveSymbol.Next)Box(child as RectTransform,w-62,40,28,28);
    var t=child.GetComponent<Text>();if(t&&!string.IsNullOrEmpty(t.text))Box(t.rectTransform,30,Mathf.Max(80,(h-59)/2),w-60,59);
   }
  }
 }
}
