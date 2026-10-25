using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Rally Heat / Touchline Live: vector panels and existing sports fonts, shared by HUD and venue.
 [DefaultExecutionOrder(480)]
 public sealed class SportsScoreboard:MonoBehaviour {
  public static readonly Color Charcoal=CoveUI.Hex("141B22"),Orange=CoveUI.Hex("FF963E"),Ivory=CoveUI.Hex("FFF5E6"),Green=CoveUI.Hex("163C35"),Chalk=CoveUI.Hex("EEF2DC"),Lime=CoveUI.Hex("C7EF6A"),Blue=CoveUI.Hex("7AD0FF"),Red=CoveUI.Hex("FF9285");
  public SportId Sport {get;private set;}public bool Venue {get;private set;}
  public Text Title {get;private set;}public Text ScoreA {get;private set;}public Text ScoreB {get;private set;}public Text Clock {get;private set;}public Text Phase {get;private set;}
  public string Shot=>shot?shot.text:"";public int CelebrationCount {get;private set;}public int LastPoints {get;private set;}public bool Animating=>burst&&burst.Visible;
  public bool Live=true;public SportsScoreboardFrame Frame {get;private set;}
  public ScoreboardDetails Details {get;private set;}public bool DetailsExpanded=>Details&&Details.Expanded;
  Text nameA,nameB,detail,shot,you,detailsHint;ScoreboardBurst burst;RectTransform aMotion,bMotion;StadiumView venueView;float ageA=10,ageB=10;
  readonly ScoreboardScoreTracker tracker=new();

  public static SportsScoreboard Create(Transform parent,SportId sport,float width=304,bool venue=false){
   var r=CoveUI.Rect(parent,sport==SportId.Basketball?"Rally Heat scoreboard":"Touchline Live scoreboard",0,0,width,sport==SportId.Basketball?(venue?194:92):(venue?56:70));
   var view=r.gameObject.AddComponent<SportsScoreboard>();view.Sport=sport;view.Venue=venue;view.Build(width);return view;
  }
  public static Text Label(Transform parent,string name,string value,float x,float y,float w,float h,int size,Color color,bool display=true,TextAnchor align=TextAnchor.MiddleCenter){
   var t=CoveUI.Text(parent,value,x,y,w*2,h*2,size*2,display,color,align);t.name=name;t.font=display?SportsUIStyle.DisplayFont:SportsUIStyle.TextFont;t.rectTransform.localScale=Vector3.one*.5f;t.resizeTextForBestFit=true;t.resizeTextMinSize=Mathf.Max(12,size);t.resizeTextMaxSize=size*2;
   if(name=="Venue name")GameLocalization.UserContent(t);
   return t;
  }
  BasketballPlate Plate(Transform parent,string name,float x,float y,float w,float h,Color color,float radius=0)=>BasketballUIStyle.Plate(parent,name,x,y,w,h,Venue?color:ScoreboardDetails.Glass(color),radius,0);
  RectTransform ScoreHolder(string name,float x,float y,float w,float h,out Text label,int size,Color color){
   var r=CoveUI.Rect(transform,name,x,y,w,h);r.pivot=Vector2.one*.5f;r.anchoredPosition+=new Vector2(w*.5f,-h*.5f);
   label=Label(r,name+" digits","0",0,0,w,h,size,color);return r;
  }
  void Build(float w){
   bool basket=Sport==SportId.Basketball;float fullHeight=((RectTransform)transform).rect.height,h=Venue?fullHeight:fullHeight-14;
   var back=Plate(transform,"Sport score chassis",0,0,w,fullHeight,basket?Charcoal:Chalk,basket?2:1);back.shadow=Venue?3:0;back.edge=basket?CoveUI.Hex("071319"):Green;
   if(basket){
    Plate(transform,"Basketball orange edge",0,0,w,3,Orange);
    float top=Venue?31:5;float side=Venue?98:65;float numberY=Venue?59:19,numberHeight=Venue?72:35;int numberSize=Venue?64:31;
    if(Venue){Title=Label(transform,"Venue name","RALLY COURT",8,5,w-16,23,16,Orange);}
    nameA=Label(transform,"Blue label","BLUE",6,top,side,16,Venue?15:10,Blue);nameB=Label(transform,"Red label","RED",w-side-6,top,side,16,Venue?15:10,Red);
    aMotion=ScoreHolder("Blue score",6,numberY,side,numberHeight,out var a,numberSize,Venue?Orange:Ivory);ScoreA=a;
    bMotion=ScoreHolder("Red score",w-side-6,numberY,side,numberHeight,out var b,numberSize,Venue?Orange:Ivory);ScoreB=b;
    Clock=Label(transform,"Match clock","READY",side+9,Venue?69:5,w-2*side-18,Venue?43:29,Venue?31:23,Ivory);
    Phase=Label(transform,"Match phase","WARM-UP",side+5,Venue?113:35,w-2*side-10,Venue?23:17,Venue?13:9,CoveUI.Hex("BCD0D0"));
    float lower=Venue?157:57;Plate(transform,"Basketball status rail",0,lower,w,h-lower,CoveUI.Hex("26353C"));
    detail=Label(transform,"Possession","RALLY COURT",10,lower+2,w-(Venue?125:92),h-lower-4,Venue?13:9,Ivory,false,TextAnchor.MiddleLeft);
    float sw=Venue?103:70;Plate(transform,"Shot clock orange tile",w-sw-5,lower+3,sw,h-lower-6,Orange,1);
    Label(transform,"Shot clock label","SHOT",w-sw,lower+3,sw*.48f,h-lower-6,Venue?12:8,Charcoal);
    shot=Label(transform,"Shot clock","—",w-sw*.5f-4,lower+2,sw*.5f,h-lower-4,Venue?24:14,Charcoal);
   }else{
    float clockWidth=Venue?91:73;
    Plate(transform,"Touchline lime edge",0,0,4,h,Lime);Plate(transform,"Touchline clock block",4,0,clockWidth,h,Green);
    Clock=Label(transform,"Match clock","READY",8,7,clockWidth-8,26,Venue?24:21,Ivory);
    Phase=Label(transform,"Match phase","KICKABOUT",8,34,clockWidth-8,17,9,Chalk);
    float space=w-clockWidth-4,centre=clockWidth+4+space*.5f;
    if(Venue){venueView=GetComponentInParent<StadiumView>();Title=Label(transform,"Venue name","SUNVALE",clockWidth+7,0,space-6,10,7,Green);}
    float box=Venue?92:73;Plate(transform,"Touchline score inset",centre-box*.5f,8,box,40,Green,1);
    aMotion=ScoreHolder("Blue score",centre-box*.5f+2,9,box*.40f,38,out var a,29,Ivory);ScoreA=a;
    bMotion=ScoreHolder("Red score",centre+box*.10f-2,9,box*.40f,38,out var b,29,Ivory);ScoreB=b;
    Label(transform,"Score separator","–",centre-6,12,12,29,16,Lime);
    float side=(space-box)*.5f;nameA=Label(transform,"Blue label","BLUE",clockWidth+5,18,side,19,11,Green);nameB=Label(transform,"Red label","RED",centre+box*.5f,18,side,19,11,Green);
    Plate(transform,"Blue team identity",clockWidth+4+side*.5f-13,12,26,3,CoveUI.Hex("187DBB"));Plate(transform,"Red team identity",w-side*.5f-13,12,26,3,CoveUI.Hex("C95040"));
    you=Label(transform,"Your team","",clockWidth+5,37,side,13,8,Green,false);
   }
   burst=ScoreboardBurst.Create(transform,Sport,w,Venue? (basket?143:59):fullHeight+6,Venue&&basket,!Venue);
   if(!Venue){detailsHint=Label(transform,"Player stats toggle hint","PLAYER STATS  +",0,h,w,14,9,basket?Ivory:Green);Details=ScoreboardDetails.Attach(this,back);CharacterIdentityBadges.ReserveHUD((RectTransform)transform);CharacterIdentityBadges.ReserveHUD(burst.Rect);}
  }
  void OnEnable(){tracker.Clear();ageA=ageB=10;if(burst)burst.Hide();}
  void OnDisable(){tracker.Clear();if(burst)burst.Hide();}
  void LateUpdate(){if(Live)Present(SportsScoreboardFeed.Read(Sport),Time.unscaledDeltaTime);if(Details){detailsHint.text=DetailsExpanded?"PLAYER STATS  −":"PLAYER STATS  +";burst.SetTop(DetailsExpanded?Details.Top+4:((RectTransform)transform).rect.height+6);if(DetailsExpanded)burst.transform.SetAsLastSibling();}}
  public void Present(SportsScoreboardFrame frame,float dt){
   if(!ScoreA)return;bool rebased=frame.a<Frame.a||frame.b<Frame.b||frame.context!=Frame.context||frame.practice!=Frame.practice;Frame=frame;int points=tracker.Observe(frame,out var scoring);
   if(points>0){CelebrationCount++;LastPoints=points;burst.Play(points,scoring,frame.practice);if(scoring==FootballTeam.A)ageA=0;else ageB=0;}
   if(frame.reset||!frame.ready||rebased){ageA=ageB=10;burst.Hide();}
   ageA+=dt;ageB+=dt;AnimateNumber(aMotion,ageA);AnimateNumber(bMotion,ageB);
   if(!frame.ready)return;
   ScoreA.text=frame.a.ToString();ScoreB.text=frame.practice?frame.made+"/"+frame.attempts:frame.b.ToString();
   nameA.text=frame.practice?"POINTS":"BLUE";nameB.text=frame.practice?"BASKETS":"RED";
   if(!Venue&&!frame.practice&&Sport==SportId.Basketball&&BasketballMatch.Instance&&AppRoot.Instance){var own=BasketballMatch.Instance.TeamOf(AppRoot.Instance.LocalAthlete);if(own==FootballTeam.A)nameA.text="BLUE · YOU";if(own==FootballTeam.B)nameB.text="RED · YOU";}
   Clock.text=frame.clock;Phase.text=frame.phase;
   if(Venue&&Sport==SportId.Football&&venueView&&venueView.title)Title.text=venueView.title.text;
   if(detail)detail.text=frame.detail;if(shot)shot.text=frame.shot;
   if(you){var m=FootballMatch.Instance;var app=AppRoot.Instance;var own=m&&app?m.TeamOf(app.LocalAthlete):FootballTeam.None;you.text=Venue||own==FootballTeam.None?"":"YOU";float space=((RectTransform)transform).rect.width-(Venue?95:77),side=(space-(Venue?92:73))*.5f;you.rectTransform.anchoredPosition=new Vector2(own==FootballTeam.B?((RectTransform)transform).rect.width-side:Venue?96:78,-37);}
  }
  static void AnimateNumber(RectTransform r,float age){float t=Mathf.Clamp01(age/.52f),lift=Mathf.Sin(t*Mathf.PI)*Mathf.Exp(-t*2);r.localScale=Vector3.one*(1+.16f*lift);}
 }
}
