using System;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 // Presentation only. The match authority owns scores, phases and deadlines.
 public sealed class FootballMatchHUD:MonoBehaviour {
  static readonly Color Coral=new Color(1,.53f,.40f),Aqua=new Color(.30f,.88f,.91f),Gold=new Color(1,.80f,.37f);
  Text scoreA,scoreB,timer,phase,teamLabel,startLabel;Image rim,flashA,flashB,teamDot;Button startButton;RectTransform teamChip;
  RectTransform selectionPanel;Button chooseA,chooseB;Text chooseALabel,chooseBLabel,selectionHint;
  Sprite glass,round,outline;Texture2D glassTexture,roundTexture,outlineTexture;int previousA=-1,previousB=-1;float pulseA,pulseB;
  public static FootballMatchHUD Create(Transform parent,Font font){
   var root=new GameObject("Football match HUD",typeof(RectTransform)).GetComponent<RectTransform>();root.SetParent(parent,false);
   root.anchorMin=root.anchorMax=root.pivot=new Vector2(.5f,1);root.anchoredPosition=new Vector2(0,-8);root.sizeDelta=new Vector2(540,140);
   var hud=root.gameObject.AddComponent<FootballMatchHUD>();hud.Build(font);return hud;
  }
  void Build(Font font){
   glass=MakePanel(true,out glassTexture);round=MakePanel(false,out roundTexture);outline=MakePanel(false,out outlineTexture,true);
   var shadow=Panel("Soft shadow",transform,new Vector2(0,-51),new Vector2(548,98),round,new Color(0,.04f,.06f,.22f));
   shadow.gameObject.AddComponent<Shadow>().effectDistance=new Vector2(0,-2);
   rim=Panel("Glass rim",transform,new Vector2(0,-46),new Vector2(542,94),outline,new Color(.58f,.89f,.93f,.25f));
   var bar=Panel("Ocean glass",transform,new Vector2(0,-46),new Vector2(540,92),glass,Color.white);
   Panel("Top sheen",bar.transform,new Vector2(0,-2),new Vector2(454,1),round,new Color(.72f,.96f,1,.25f));
   Panel("Team A accent",bar.transform,new Vector2(-235,-46),new Vector2(4,38),round,Coral);
   Panel("Team B accent",bar.transform,new Vector2(235,-46),new Vector2(4,38),round,Aqua);
   flashA=Panel("Team A goal flash",bar.transform,new Vector2(-135,-46),new Vector2(182,78),round,Color.clear);
   flashB=Panel("Team B goal flash",bar.transform,new Vector2(135,-46),new Vector2(182,78),round,Color.clear);
   var teamA=Rect("Team A",bar.transform,new Vector2(-187,-46),new Vector2(90,36)).gameObject.AddComponent<FootballTeamAWordmark>();teamA.raycastTarget=false;
   Label("Team B",bar.transform,"TEAM B",new Vector2(190,-46),new Vector2(70,28),15,Aqua,font);
   scoreA=Label("Score A",bar.transform,"0",new Vector2(-110,-45),new Vector2(90,66),52,Color.white,font);
   scoreB=Label("Score B",bar.transform,"0",new Vector2(110,-45),new Vector2(90,66),52,Color.white,font);
   Panel("Left divider",bar.transform,new Vector2(-53,-46),new Vector2(1,40),round,new Color(1,1,1,.14f));
   Panel("Right divider",bar.transform,new Vector2(53,-46),new Vector2(1,40),round,new Color(1,1,1,.14f));
   timer=Label("Match timer",bar.transform,"03:00",new Vector2(0,-38),new Vector2(102,38),28,Color.white,font);
   phase=Label("Match phase",bar.transform,"FOOTBALL",new Vector2(0,-65),new Vector2(104,20),11,new Color(.75f,.91f,.93f),font);
   teamChip=Panel("Your team chip",transform,new Vector2(0,-113),new Vector2(172,30),round,new Color(.025f,.14f,.20f,.88f)).rectTransform;
   teamDot=Panel("Team indicator",teamChip,new Vector2(-66,-15),new Vector2(6,6),round,Coral);
   teamLabel=Label("Your team",teamChip,"You: Team A",new Vector2(5,-15),new Vector2(146,26),14,Color.white,font);
   var start=Panel("Start match",transform,new Vector2(0,-114),new Vector2(174,34),round,new Color(.08f,.33f,.39f,.94f));start.raycastTarget=true;
   startButton=start.gameObject.AddComponent<Button>();startButton.targetGraphic=start;
   var colors=startButton.colors;colors.highlightedColor=new Color(.7f,1,1);colors.pressedColor=new Color(.55f,.85f,.90f);startButton.colors=colors;
   startLabel=Label("Start label",start.transform,"Start match",new Vector2(0,-17),new Vector2(166,30),15,Color.white,font);
   startButton.onClick.AddListener(()=>FootballMatch.Instance?.StartMatch());
   selectionPanel=Panel("Team selection",transform,new Vector2(0,-246),new Vector2(440,148),glass,Color.white).rectTransform;
   Label("Selection title",selectionPanel,"CHOOSE YOUR TEAM",new Vector2(0,-24),new Vector2(400,30),19,Color.white,font);
   chooseA=TeamButton(selectionPanel,"Team A choice",-106,Coral,font,out chooseALabel);chooseB=TeamButton(selectionPanel,"Team B choice",106,Aqua,font,out chooseBLabel);
   chooseA.onClick.AddListener(()=>FootballMatch.Instance?.RequestTeam(FootballTeam.A));chooseB.onClick.AddListener(()=>FootballMatch.Instance?.RequestTeam(FootballTeam.B));
   selectionHint=Label("Selection hint",selectionPanel,"You: Not selected",new Vector2(0,-125),new Vector2(408,28),14,Color.white,font);
   selectionPanel.gameObject.SetActive(false);
  }
    Button TeamButton(Transform parent,string name,float x,Color accent,Font font,out Text label){
   var image=Panel(name,parent,new Vector2(x,-76),new Vector2(194,58),round,new Color(accent.r,accent.g,accent.b,.22f));image.raycastTarget=true;
   var button=image.gameObject.AddComponent<Button>();button.targetGraphic=image;label=Label(name+" label",image.transform,"",new Vector2(0,-29),new Vector2(186,52),17,Color.white,font);return button;
  }
  static RectTransform Rect(string name,Transform parent,Vector2 position,Vector2 size){var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);rect.anchorMin=rect.anchorMax=new Vector2(.5f,1);rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=position;rect.sizeDelta=size;return rect;}
  static Image Panel(string name,Transform parent,Vector2 position,Vector2 size,Sprite sprite,Color color){var image=Rect(name,parent,position,size).gameObject.AddComponent<Image>();image.sprite=sprite;image.type=Image.Type.Sliced;image.color=color;image.raycastTarget=false;return image;}
  static Text Label(string name,Transform parent,string value,Vector2 position,Vector2 size,int fontSize,Color color,Font font){var text=Rect(name,parent,position,size).gameObject.AddComponent<Text>();text.font=font;text.fontSize=fontSize;text.fontStyle=FontStyle.Bold;text.color=color;text.alignment=TextAnchor.MiddleCenter;text.text=value;text.raycastTarget=false;text.supportRichText=false;text.horizontalOverflow=HorizontalWrapMode.Overflow;var shadow=text.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,.03f,.06f,.65f);shadow.effectDistance=new Vector2(0,-1);return text;}
  // Tiny generated UI textures: no imported art, blur pass or shader dependency.
  static Sprite MakePanel(bool gradient,out Texture2D texture,bool border=false){
   const int width=128,height=64;const float radius=18;
   texture=new Texture2D(width,height,TextureFormat.RGBA32,false){name=gradient?"Scoreboard ocean glass":"Scoreboard rounded mask",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
   for(int y=0;y<height;y++)for(int x=0;x<width;x++){
    float dx=Mathf.Max(radius-x-.5f,x+.5f-(width-radius)),dy=Mathf.Max(radius-y-.5f,y+.5f-(height-radius));
    float distance=Mathf.Min(Mathf.Max(dx,dy),0)+Mathf.Sqrt(Mathf.Max(0,dx)*Mathf.Max(0,dx)+Mathf.Max(0,dy)*Mathf.Max(0,dy))-radius;
    float coverage=Mathf.Clamp01(-distance);if(border)coverage*=Mathf.Clamp01(distance+2);
    var color=gradient?Color.Lerp(new Color(.018f,.095f,.16f,.91f),new Color(.05f,.25f,.30f,.85f),y/(float)(height-1)):Color.white;color.a*=coverage;texture.SetPixel(x,y,color);
   }
   texture.Apply(false,true);return Sprite.Create(texture,new Rect(0,0,width,height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(radius,radius,radius,radius));
  }
  void Update(){
   var match=FootballMatch.Instance;if(!match){startButton.gameObject.SetActive(false);teamChip.gameObject.SetActive(false);return;}
   Present(match.Snapshot,match.Remaining,match.TeamOf(AppRoot.Instance.LocalAthlete),match.Authority,Time.unscaledDeltaTime);
   if(startButton.gameObject.activeSelf){startButton.interactable=match.CanStart;if(!match.CanStart)startLabel.text="Need even players (2+)";}
  }
  void Present(FootballMatchSnapshot s,double remaining,FootballTeam team,bool authority,float dt){
   if(previousA>=0&&s.scoreA>previousA)pulseA=.65f;if(previousB>=0&&s.scoreB>previousB)pulseB=.65f;
   if(s.scoreA<previousA||s.scoreB<previousB)pulseA=pulseB=0;previousA=s.scoreA;previousB=s.scoreB;
   pulseA=Mathf.Max(0,pulseA-dt);pulseB=Mathf.Max(0,pulseB-dt);
   Flash(flashA,scoreA,Coral,pulseA);Flash(flashB,scoreB,Aqua,pulseB);
   scoreA.text=s.scoreA.ToString();scoreB.text=s.scoreB.ToString();
   bool extra=s.phase==FootballMatchPhase.Overtime;
   rim.color=extra?new Color(Gold.r,Gold.g,Gold.b,.8f):new Color(.58f,.89f,.93f,.25f);timer.color=extra?Gold:Color.white;phase.color=extra?Gold:new Color(.75f,.91f,.93f);
   timer.fontSize=28;timer.text=TimeSpan.FromSeconds(Math.Max(0,Math.Ceiling(remaining))).ToString(@"mm\:ss");
   phase.text=s.phase switch {FootballMatchPhase.Idle=>"READY TO PLAY",FootballMatchPhase.Kickoff=>"KICKOFF",FootballMatchPhase.Regulation=>"REGULATION",FootballMatchPhase.TeamSelection=>"CHOOSE TEAMS",FootballMatchPhase.Overtime=>"GOLDEN GOAL",FootballMatchPhase.Finished=>s.result==FootballMatchResult.Draw?"DRAW":s.result==FootballMatchResult.TeamA?"TEAM A WINS":"TEAM B WINS",_=>"GOAL"};
   if(s.phase==FootballMatchPhase.Idle)timer.text="FOOTBALL";
   if(s.phase==FootballMatchPhase.Kickoff||s.phase==FootballMatchPhase.TeamSelection)timer.text=Math.Ceiling(remaining).ToString();
   if(s.phase==FootballMatchPhase.Finished)timer.text="FULL TIME";
   if(s.phase==FootballMatchPhase.Idle||s.phase==FootballMatchPhase.Finished)timer.fontSize=18;
   bool action=authority&&(s.phase==FootballMatchPhase.Idle||s.phase==FootballMatchPhase.Finished);
   startButton.gameObject.SetActive(action);startLabel.text=s.phase==FootballMatchPhase.Finished?"New match":"Start match";
   bool selecting=s.phase==FootballMatchPhase.TeamSelection;
   selectionPanel.gameObject.SetActive(selecting);
   chooseALabel.text="Team A   "+s.teamA+" / "+s.teamCapacity+(s.teamA>=s.teamCapacity?"\nFULL":"");chooseBLabel.text="Team B   "+s.teamB+" / "+s.teamCapacity+(s.teamB>=s.teamCapacity?"\nFULL":"");
   chooseA.interactable=selecting&&(team==FootballTeam.A||s.teamA<s.teamCapacity);chooseB.interactable=selecting&&(team==FootballTeam.B||s.teamB<s.teamCapacity);
   selectionHint.text=team==FootballTeam.None?"You: Not selected":"You: Team "+team+"   (confirmed when time expires)";
   teamChip.gameObject.SetActive(team!=FootballTeam.None||selecting);teamChip.anchoredPosition=new Vector2(action?-94:0,-113);
   startButton.GetComponent<RectTransform>().anchoredPosition=new Vector2(team!=FootballTeam.None?94:0,-114);
   teamLabel.text=team==FootballTeam.None?"You: Not selected":"You: Team "+team;teamDot.color=team==FootballTeam.A?Coral:Aqua;
  }
  static void Flash(Image image,Text score,Color accent,float pulse){float value=pulse/.65f;image.color=new Color(accent.r,accent.g,accent.b,value*.24f);score.color=Color.Lerp(Color.white,accent,value*.65f);score.rectTransform.localScale=Vector3.one*(1+.07f*Mathf.Sin(value*Mathf.PI));}
  void OnDestroy(){Release(glass);Release(round);Release(outline);Release(outlineTexture);Release(glassTexture);Release(roundTexture);}
  static void Release(UnityEngine.Object item){
   if(!item)return;
#if UNITY_EDITOR
   if(!Application.isPlaying){DestroyImmediate(item);return;}
#endif
   Destroy(item);
  }
#if UNITY_EDITOR
  public void Preview(FootballMatchSnapshot snapshot,double seconds,FootballTeam team,float elapsed=0){Present(snapshot,seconds,team,true,elapsed);}
#endif
 }
}


