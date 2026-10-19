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
  SportId guideSport,venueSport=SportId.Football;bool guideFromHub;
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
   var room=Plate(content,"Play together",64,176,1472,128,Cream);
   Text(room.transform,"ONE ROOM. FOUR SPORTS.",28,17,710,48,35,true,Mint);
   Text(room.transform,"Invite your crew. Travel and play together.",28,72,710,32,23,false,Muted);
   ActionButton("Create room","Create room",883,205,300,70,Mint,()=>Go("create"),CoveSymbol.People,28);
   ActionButton("Join room","Join room",1203,205,305,70,Raised,()=>Go("join"),CoveSymbol.Join,28);
   Text(content,"CHOOSE YOUR SPORT",64,322,1000,48,35,true);
   Text(content,"Solo adventures & versus bots",1000,329,536,36,23,false,Muted,TextAnchor.MiddleRight);
   for(int i=0;i<HomeSports.Length;i++){var sport=HomeSports[i];SportCard(sport,64+i*374,390,350,310,()=>ChooseSport(sport,"sport"));}
   Utility("Customize venue","CUSTOMIZE VENUE","Make it your home ground",64,730,724,Cream,CoveSymbol.Paint,()=>Go("venues"));
   Utility("How to play","HOW TO PLAY","Controls, rules & tips",812,730,724,Cream,CoveSymbol.Book,()=>{guideSport=app.SelectedSport;Go("guides");});
   if(!string.IsNullOrEmpty(app.rooms.Error))NoticeLater(app.rooms.Error);
  }
  void SportCard(SportId sport,float x,float y,float w,float h,Action action,string id=null,string caption=null,bool enabled=true){
   var b=Button(content,id??"Island - "+sport,"",x,y,w,h,Cream,action);b.interactable=enabled;
   if(enabled)busyButtons.Add(b);
   float artHeight=Mathf.Min(w*2/3f,h-112);
   // The original approved atlas is sliced at exact quadrant boundaries, without recompression.
   var mask=Plate(b.transform,"Artwork window",2,2,w-4,artHeight,Color.white,12,0);mask.gameObject.AddComponent<Mask>().showMaskGraphic=false;
   float imageHeight=(w-4)*2/3f;
   var art=Image(mask.transform,sport+" illustration",SportArt(sport),0,(artHeight-imageHeight)/2,w-4,imageHeight);art.preserveAspect=false;
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
   Mode("Explore offline","EXPLORE OFFLINE","Your own pace. Explore the island and practise your skills.",CoveSymbol.Map,665,190,app.EnterOffline,Mint);
   bool bots=HasBots(app.SelectedSport);
   Mode("Play vs AI","VERSUS BOTS",bots?"Pick your team size and take on computer opponents.":"No bots for this sport yet. Play with friends from the main menu.",CoveSymbol.People,665,489,()=>Go("ai"),Cream,bots);
   Button(content,"Sport help","How to play",64,807,300,65,Raised,()=>Go("help"),CoveSymbol.Book,27);
   Text(content,"Playing with friends? Create or join a room on the main menu.",445,815,1090,40,23,false,Muted,TextAnchor.MiddleRight);
  }
  void Mode(string id,string title,string caption,CoveSymbol icon,float x,float y,Action action,Color color,bool enabled=true){
   var b=Button(content,id,"",x,y,870,275,color,action);b.interactable=enabled;if(enabled)busyButtons.Add(b);var ink=Foreground(color);
   Icon(b.transform,icon,30,30,44,enabled?ink:Muted);
   Text(b.transform,title,30,110,800,59,37,true,enabled?ink:Muted);
   Text(b.transform,caption,30,185,800,62,25,false,ink==Ink?Muted:DarkInk);
   if(enabled)Icon(b.transform,CoveSymbol.Next,808,40,28,ink);
  }
  void VenueHub(){
   Heading("CUSTOMIZE VENUE","Your home advantage. Saved looks are used when you host.");
   SportTabs("Venue",venueSport,s=>{venueSport=s;Rebuild();});
   var p=Plate(content,"Selected venue",64,294,1472,484,Cream);
   Image(p.transform,"Venue illustration",SportArt(venueSport),20,24,630,420);
   bool supported=HasBots(venueSport);
   Text(p.transform,venueSport.ToString().ToUpperInvariant(),705,42,710,70,51,true,Accent(venueSport));
   Text(p.transform,supported?"MAKE IT YOURS":"MORE TO COME",708,131,700,42,27,true);
   Text(p.transform,venueSport==SportId.Football?"Name your ground, choose an accent colour,\nand style the big screen and flags.":venueSport==SportId.Basketball?"Name your arena, recolour the seats and trim,\nand choose your centre-court logo.":"This island is ready to explore.\nVenue customization is coming in a future update.",708,196,700,106,27,false,Muted);
   var edit=Button(p.transform,"Edit selected venue",supported?"Edit venue":"Coming soon",708,347,380,76,supported?Mint:Raised,()=>{history.Push("venues");ChooseSport(venueSport,"custom");},CoveSymbol.Paint,28);
   edit.interactable=supported&&CanEdit&&!app.rooms.Connected;
   Footer("Choose a sport above. Your saved venue designs stay separate.");
  }
  void GuideHub(){
   guideFromHub=true;Help();
  }
  void SportTabs(string prefix,SportId selected,Action<SportId> select){
   var sports=new[]{SportId.Basketball,SportId.Football,SportId.Golf,SportId.Fishing};
   for(int i=0;i<sports.Length;i++){
    var sport=sports[i];bool active=sport==selected;
    var b=Button(content,prefix+" - "+sport,"",64+i*374,172,350,88,active?Raised:Background,()=>select(sport));
    var icon=Rect(b.transform,"Sport icon",22,16,56,56).gameObject.AddComponent<MenuSportIcon>();icon.sport=sport;icon.selected=active;icon.raycastTarget=false;
    Text(b.transform,sport.ToString().ToUpperInvariant(),98,20,232,44,28,true,active?Accent(sport):Hex("89969D"));
    if(active)Plate(b.transform,"Selected sport underline",22,82,306,4,Accent(sport),0,0);
   }
  }
 }
 // Small vector icons stay crisp at every menu scale and add no texture payload.
 public sealed class MenuSportIcon:MaskableGraphic {
  public SportId sport;public bool selected;
  Vector2 P(float x,float y)=>new(rectTransform.rect.xMin+x*rectTransform.rect.width,rectTransform.rect.yMin+y*rectTransform.rect.height);
  void Disc(VertexHelper v,float x,float y,float radius,Color c){var p=P(x,y);float r=rectTransform.rect.width*radius;CovePlate.Round(v,new Rect(p.x-r,p.y-r,2*r,2*r),r,c);}
  void Stroke(VertexHelper v,float x,float y,float xx,float yy,Color c,float width=.035f){var a=P(x,y);var b=P(xx,yy);var n=new Vector2(-(b-a).y,(b-a).x).normalized*rectTransform.rect.width*width/2;int i=v.currentVertCount;v.AddVert(a+n,c,Vector2.zero);v.AddVert(b+n,c,Vector2.zero);v.AddVert(b-n,c,Vector2.zero);v.AddVert(a-n,c,Vector2.zero);v.AddTriangle(i,i+1,i+2);v.AddTriangle(i,i+2,i+3);}
  void Triangle(VertexHelper v,Vector2 a,Vector2 b,Vector2 c,Color tint){int i=v.currentVertCount;v.AddVert(P(a.x,a.y),tint,Vector2.zero);v.AddVert(P(b.x,b.y),tint,Vector2.zero);v.AddVert(P(c.x,c.y),tint,Vector2.zero);v.AddTriangle(i,i+1,i+2);}
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();var gray=CoveUI.Hex("89969D");var dark=CoveUI.Hex("102B3B");
   Color fill=selected?sport switch{SportId.Basketball=>CoveUI.Hex("FFB17D"),SportId.Football=>CoveUI.Hex("BBEF6E"),SportId.Golf=>CoveUI.Hex("D1BAFF"),_=>CoveUI.Hex("53DED7")}:gray;
   if(sport==SportId.Fishing){
    Disc(v,.42f,.5f,.28f,fill);Triangle(v,new(.6f,.5f),new(.95f,.78f),new(.95f,.22f),fill);
    Triangle(v,new(.28f,.71f),new(.55f,.92f),new(.6f,.64f),fill);Disc(v,.3f,.55f,.044f,dark);
    if(selected){Stroke(v,.49f,.36f,.56f,.5f,dark);Stroke(v,.56f,.5f,.49f,.64f,dark);}return;
   }
   Disc(v,.5f,.5f,.44f,fill);
   if(sport==SportId.Basketball){Stroke(v,.06f,.5f,.94f,.5f,dark);Stroke(v,.5f,.06f,.5f,.94f,dark);for(int side=0;side<2;side++)for(int j=0;j<12;j++){float y=.1f+j*.8f/12,yy=.1f+(j+1)*.8f/12;float x=.17f+.15f*Mathf.Sin((y-.1f)/.8f*Mathf.PI),xx=.17f+.15f*Mathf.Sin((yy-.1f)/.8f*Mathf.PI);Stroke(v,side==0?x:1-x,y,side==0?xx:1-xx,yy,dark);}}
   else if(sport==SportId.Football){for(int i=0;i<5;i++){float a=(90+i*72)*Mathf.Deg2Rad,b=(90+(i+1)*72)*Mathf.Deg2Rad;var p=new Vector2(.5f+Mathf.Cos(a)*.18f,.5f+Mathf.Sin(a)*.18f);var q=new Vector2(.5f+Mathf.Cos(b)*.18f,.5f+Mathf.Sin(b)*.18f);Triangle(v,new(.5f,.5f),p,q,dark);Stroke(v,p.x,p.y,.5f+Mathf.Cos(a)*.43f,.5f+Mathf.Sin(a)*.43f,dark);}}
   else {for(int row=0;row<3;row++)for(int col=0;col<3;col++)Disc(v,.3f+col*.19f,.28f+row*.19f,.032f,dark);if(selected)Disc(v,.36f,.74f,.07f,Color.white);}
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
