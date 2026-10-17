using System;
using UnityEngine;
using UnityEngine.UI;
using static WhatTheFish.CoveUI;

namespace WhatTheFish {
 public sealed partial class StorybookMenu {
  // Menu-only theme: in-world controls and the welcome screen retain their own styles.
  static readonly Color Ink=Hex("F3F8FA"),Muted=Hex("ADC2CE"),Cream=Hex("153449"),
   Coral=Hex("BBEF6E"),Mint=Hex("BBEF6E"),Sky=Hex("1D485C"),Gold=Hex("53DED7"),Line=Hex("29495D");
  static readonly Color Background=Hex("0C2434"),Raised=Hex("193D52"),DarkInk=Hex("102B3B");
  static readonly SportId[] HomeSports={SportId.Fishing,SportId.Basketball,SportId.Football,SportId.Golf};
  static Font sportsFont;static Sprite[] sportArt;
  SportId guideSport;bool guideFromHub;
  static Font SportsFont=>sportsFont?sportsFont:sportsFont=Resources.Load<Font>("SportyMenu/BarlowDisplay");
  static Color Surface(Color color)=>color==Color.white?Raised:color;
  static Color Foreground(Color color)=>(color.r*.299f+color.g*.587f+color.b*.114f)>.6f?DarkInk:Ink;
  static Color Accent(SportId sport)=>sport switch{SportId.Fishing=>Gold,SportId.Basketball=>Hex("FFB17D"),SportId.Football=>Mint,_=>Hex("D1BAFF")};
  static string ShortCaption(SportId sport)=>sport switch{SportId.Fishing=>"Cast & catch",SportId.Basketball=>"Shoot & score",SportId.Football=>"Pass & score",_=>"Aim & putt"};
  static bool HasBots(SportId sport)=>sport==SportId.Football||sport==SportId.Basketball;
  static Sprite SportArt(SportId sport){
   if(sportArt==null){var atlas=Resources.Load<Texture2D>("SportyMenu/SportCards");sportArt=new Sprite[4];
    if(atlas)for(int i=0;i<HomeSports.Length;i++)sportArt[(int)HomeSports[i]]=Sprite.Create(atlas,new Rect(i%2*atlas.width/2f,(1-i/2)*atlas.height/2f,atlas.width/2f,atlas.height/2f),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
   }
   return sportArt[(int)sport];
  }
  static Text Text(Transform parent,string value,float x,float y,float w,float h,int size=25,bool bold=false,Color? color=null,TextAnchor align=TextAnchor.MiddleLeft){
   var t=CoveUI.Text(parent,value,x,y,w,h,size,bold,color??Ink,align);t.font=bold?SportsFont:CoveUI.TextFont;return t;
  }
  static CovePlate Plate(Transform parent,string name,float x,float y,float w,float h,Color color,float radius=24,float border=2,bool shadow=false){
   var p=CoveUI.Plate(parent,name,x,y,w,h,Surface(color),Mathf.Min(radius,16),border>0?1.5f:0,false);p.borderColor=Line;return p;
  }
  static CoveButton Button(Transform parent,string id,string label,float x,float y,float w,float h,Color color,Action action,CoveSymbol icon=CoveSymbol.None,int size=28){
   color=Surface(color);var b=CoveUI.Button(parent,id,label,x,y,w,h,color,action,icon,size);
   var p=b.GetComponent<CovePlate>();p.radius=13;p.border=1.5f;p.shadow=false;p.borderColor=Line;
   // Selectable initially enables with ColorTint before CoveUI sets Transition.None.
   // A menu created behind the welcome gate must not retain that disabled renderer tint.
   p.canvasRenderer.SetColor(Color.white);
   var feedback=b.GetComponent<CoveFeedback>();feedback.activeBorder=Line;feedback.disabledBorder=Line;feedback.disabledTint=Background;feedback.pressedTint=DarkInk;feedback.releaseBounce=.012f;
   foreach(var t in b.GetComponentsInChildren<Text>()){t.font=SportsFont;t.color=Foreground(color);}
   foreach(var i in b.GetComponentsInChildren<CoveIcon>())i.color=Foreground(color);
   return b;
  }
  static CoveIcon Icon(Transform parent,CoveSymbol symbol,float x,float y,float size,Color? color=null)=>CoveUI.Icon(parent,symbol,x,y,size,color??Ink);
  static InputField Field(Transform parent,string id,string placeholder,string value,float x,float y,float w,int limit=24){
   var f=CoveUI.Field(parent,id,placeholder,value,x,y,w,limit);var p=f.GetComponent<CovePlate>();p.color=Background;p.radius=12;p.borderColor=Line;
   f.textComponent.font=CoveUI.TextFont;f.textComponent.color=Ink;((Text)f.placeholder).color=Muted;f.caretColor=Ink;return f;
  }
  void Home(){
   history.Clear();guideFromHub=false;
   Image(content,"Official logo",Resources.Load<Sprite>("Menu/WhatTheFishLogo"),64,0,205,151);
   Button(content,"Player profile","Your profile",1085,48,248,68,Raised,()=>Go("profile"),CoveSymbol.People,27);
   Button(content,"Settings","Settings",1353,48,183,68,Raised,()=>Go("settings"),CoveSymbol.Settings,26);
   Plate(content,"Header divider",64,151,1472,2,Line,0,0);
   Text(content,"/",61,178,27,73,58,true,Mint);
   Text(content,"CHOOSE YOUR SPORT",96,179,1050,73,55,true);
   Text(content,"Pick a game. Let's play.",97,250,1050,38,24,false,Muted);
   Text(content,"BRING\nYOUR\nGAME.",1241,180,134,111,29,true,Gold);
   Image(content,"Sports mascot",Resources.Load<Sprite>("SportyMenu/Mascot"),1385,164,151,140);
   for(int i=0;i<HomeSports.Length;i++){var sport=HomeSports[i];SportCard(sport,64+i*374,322,350,350,()=>ChooseSport(sport,"sport"));}
   Utility("Customize venue","CUSTOMIZE VENUE","Manage your venues",64,708,724,Mint,CoveSymbol.Paint,()=>Go("venues"));
   Utility("How to play","HOW TO PLAY","Controls and rules",812,708,724,Cream,CoveSymbol.Book,()=>Go("guides"));
   Text(content,"Four sports. One place to play.",64,852,900,28,20,false,Muted);
   if(!string.IsNullOrEmpty(app.rooms.Error))NoticeLater(app.rooms.Error);
  }
  void SportCard(SportId sport,float x,float y,float w,float h,Action action,string id=null,string caption=null,bool enabled=true){
   var b=Button(content,id??"Island - "+sport,"",x,y,w,h,Cream,action);b.interactable=enabled;
   if(enabled)busyButtons.Add(b);
   float artHeight=w*2/3f;
   // The original approved atlas is sliced at exact quadrant boundaries, without recompression.
   var mask=Plate(b.transform,"Artwork window",2,2,w-4,artHeight,Color.white,12,0);mask.gameObject.AddComponent<Mask>().showMaskGraphic=false;
   var art=Image(mask.transform,sport+" illustration",SportArt(sport),0,0,w-4,artHeight);art.preserveAspect=false;
   Plate(b.transform,"Sport accent",2,artHeight, w-4,4,Accent(sport),0,0);
   Text(b.transform,sport.ToString().ToUpperInvariant(),24,artHeight+20,w-104,44,34,true);
   Text(b.transform,caption??ShortCaption(sport),24,artHeight+69,w-48,32,21,false,Muted);
   var arrow=Plate(b.transform,"Open sport",w-64,artHeight+27,40,40,enabled?Accent(sport):Raised,9,0);
   Icon(arrow.transform,enabled?CoveSymbol.Next:CoveSymbol.Flag,10,10,20,enabled?DarkInk:Muted);
  }
  void Utility(string id,string title,string caption,float x,float y,float width,Color color,CoveSymbol icon,Action action){
   var b=Button(content,id,"",x,y,width,124,color,action);var ink=Foreground(color);
   var tile=Plate(b.transform,"Icon tile",28,34,56,56,Color.Lerp(color,DarkInk,.13f),12,0);Icon(tile.transform,icon,14,14,28,ink);
   Text(b.transform,title,106,21,width-180,48,32,true,ink);Text(b.transform,caption,106,72,width-180,30,21,false,ink==Ink?Muted:DarkInk);
   Icon(b.transform,CoveSymbol.Next,width-55,49,26,ink);
  }
  void SportHome(){
   guideFromHub=false;Heading(app.SelectedSport.ToString().ToUpperInvariant(),ShortCaption(app.SelectedSport)+". Choose how you want to play.");
   var art=Plate(content,"Sport hero",64,190,565,574,Cream,16,1);
   Image(art.transform,"Sport artwork",SportArt(app.SelectedSport),2,2,561,374);
   Text(art.transform,"LET'S PLAY.",30,402,505,70,51,true,Accent(app.SelectedSport));
   Text(art.transform,SportCaption(app.SelectedSport),32,482,500,55,25,false,Muted);
   Mode("Create room","CREATE ROOM","Host a private game with friends",CoveSymbol.People,665,190,()=>Go("create"),Mint);
   Mode("Join room","JOIN ROOM","Enter your friend's room code",CoveSymbol.Join,1116,190,()=>Go("join"),Cream);
   Mode("Explore offline","EXPLORE OFFLINE","Jump in and explore on your own",CoveSymbol.Map,665,489,app.EnterOffline,Cream);
   bool bots=HasBots(app.SelectedSport);
   Mode("Play vs AI","VERSUS BOTS",bots?"Team up against computer players":"Not available for this sport yet",CoveSymbol.People,1116,489,()=>Go("ai"),Cream,bots);
   Button(content,"Sport help","How to play",64,807,300,65,Raised,()=>Go("help"),CoveSymbol.Book,27);
   Text(content,"Return to the main menu to choose another sport.",575,815,960,40,23,false,Muted,TextAnchor.MiddleRight);
  }
  void Mode(string id,string title,string caption,CoveSymbol icon,float x,float y,Action action,Color color,bool enabled=true){
   var b=Button(content,id,"",x,y,420,275,color,action);b.interactable=enabled;if(enabled)busyButtons.Add(b);var ink=Foreground(color);
   Icon(b.transform,icon,30,30,44,enabled?ink:Muted);
   Text(b.transform,title,30,110,360,59,37,true,enabled?ink:Muted);
   Text(b.transform,caption,30,185,360,62,23,false,ink==Ink?Muted:DarkInk);
   if(enabled)Icon(b.transform,CoveSymbol.Next,358,40,28,ink);
  }
  void VenueHub(){
   Heading("CUSTOMIZE VENUE","Choose a venue to make your own. Your saved looks are used when you host.");
   for(int i=0;i<HomeSports.Length;i++){var sport=HomeSports[i];bool supported=HasBots(sport);bool allowed=supported&&CanEdit&&(!app.rooms.Connected||app.SelectedSport==sport);
    SportCard(sport,64+i*374,254,350,350,()=>{history.Push("venues");ChooseSport(sport,"custom");},"Venue - "+sport,supported?"Edit venue":"Customization coming soon",allowed);
   }
   var note=Plate(content,"Venue hub information",64,652,1472,117,Cream,16,1);
   Text(note.transform,"YOUR HOME ADVANTAGE",28,16,1416,42,31,true,Mint);
   Text(note.transform,"Football: name, screen & flags. Basketball: name, colours & centre-court logo.",28,64,1416,35,24,false,Muted);
   Footer("Fishing and Golf venue editing will arrive in a future update.");
  }
  void GuideHub(){
   Heading("HOW TO PLAY","Pick a sport to see its controls and rules.");
   for(int i=0;i<HomeSports.Length;i++){var sport=HomeSports[i];SportCard(sport,64+i*374,254,350,350,()=>{guideSport=sport;guideFromHub=true;Go("help");},"Guide - "+sport,"Controls & rules");}
   Footer("Read a guide, then return to the main menu when you're ready to play.");
  }
 }
 public sealed class SportsMenuBackground:MaskableGraphic {
  protected override void OnPopulateMesh(VertexHelper vh){
   vh.Clear();var r=rectTransform.rect;CovePlate.Round(vh,r,0,CoveUI.Hex("0C2434"));
   int n=vh.currentVertCount;var c=CoveUI.Hex("123041");
   vh.AddVert(new Vector3(r.xMin+r.width*.39f,r.yMin),c,Vector2.zero);vh.AddVert(new Vector3(r.xMin+r.width*.66f,r.yMin),c,Vector2.zero);
   vh.AddVert(new Vector3(r.xMax,r.yMax),c,Vector2.zero);vh.AddVert(new Vector3(r.xMin+r.width*.74f,r.yMax),c,Vector2.zero);
   vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
  }
 }
}
