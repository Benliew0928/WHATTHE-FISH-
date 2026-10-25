using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public struct ScoreboardPlayerData {
  public ulong id;public FootballTeam team;public string name;public byte number;public int score,assists,passes,interceptions;
 }

 // A local view over the existing authoritative/replicated statistics. Opening it
 // never pauses the match, changes a score, or installs a full-screen input shield.
 [DefaultExecutionOrder(-60)]
 public sealed class ScoreboardDetails:MonoBehaviour {
  public const float BackgroundOpacity=.8f;
  static ScoreboardDetails current;static int closedFrame=-1;
  public static bool BlocksBack=>current&&current.Expanded||closedFrame==Time.frameCount;
  public bool Expanded {get;private set;}
  public RectTransform Panel {get;private set;}
  public CoveButton ToggleButton {get;private set;}
  public ulong SelectedPlayer {get;private set;}=ulong.MaxValue;
  public IReadOnlyList<ScoreboardPlayerData> Players=>players;
  public float Top {get;private set;}
  public ScrollRect BlueScroll=>teams[0].scroll;public ScrollRect RedScroll=>teams[1].scroll;
  public Text SelectedName=>selectedName;
  SportsScoreboard owner;CanvasGroup opacity;Text heading,kicker,empty,note,selectedName,selectedNumber,selectedTeam;
  readonly Text[] selectedStats=new Text[4];RectTransform focus;BasketballPlate background;CoveButton close;
  readonly List<ScoreboardPlayerData> players=new();readonly TeamView[] teams=new TeamView[2];
  readonly Vector3[] corners=new Vector3[4];float motion,nextRefresh,width,height,rosterHeight,rowHeight;int context;bool practice;Vector2 previousSize;
  sealed class TeamView {public RectTransform root,viewport,content;public Text label,total,columns;public ScrollRect scroll;public readonly List<Row> rows=new();public Text more;}
  sealed class Row {public RectTransform root;public Text number,name;public Text[] stats;public BasketballPlate selected;public CoveButton button;public ulong id;}
  Color Ink=>owner.Sport==SportId.Basketball?SportsScoreboard.Charcoal:SportsScoreboard.Green;
  Color Accent=>owner.Sport==SportId.Basketball?SportsScoreboard.Orange:SportsScoreboard.Lime;
  bool Football=>owner.Sport==SportId.Football;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void Reset(){current=null;closedFrame=-1;}
  public static Color Glass(Color color,float alpha=BackgroundOpacity){color.a=alpha;return color;}
  public static string ShortName(string value,int length){if(string.IsNullOrWhiteSpace(value))return "Player";var s=new StringInfo(value);return s.LengthInTextElements<=length?value:s.SubstringByTextElements(0,length-1)+"…";}
  public static ScoreboardDetails Attach(SportsScoreboard owner,BasketballPlate chassis){
   var view=owner.gameObject.AddComponent<ScoreboardDetails>();view.owner=owner;
   chassis.raycastTarget=true;view.ToggleButton=chassis.gameObject.AddComponent<CoveButton>();view.ToggleButton.targetGraphic=chassis;view.ToggleButton.transition=Selectable.Transition.None;view.ToggleButton.navigation=new Navigation{mode=Navigation.Mode.None};view.ToggleButton.onClick.AddListener(view.Toggle);
   view.Build();return view;
  }
  Text Label(Transform parent,string name,string value,float x,float y,float w,float h,int size,Color color,TextAnchor align=TextAnchor.MiddleLeft){
   var t=SportsScoreboard.Label(parent,name,value,x,y,w,h,size,color,false,align);t.resizeTextMinSize=Mathf.Max(12,size);t.resizeTextMaxSize=size*2;
   var shadow=t.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.8f);shadow.effectDistance=new Vector2(1,-1);shadow.useGraphicAlpha=true;return t;
  }
  BasketballPlate Plate(Transform parent,string name,float x,float y,float w,float h,Color color,float radius=0){return BasketballUIStyle.Plate(parent,name,x,y,w,h,color,radius,0);}
  CoveButton Button(Transform parent,string name,string caption,float x,float y,float w,float h,Action action){
   var face=Plate(parent,name,x,y,w,h,Glass(Ink,.24f),3);face.raycastTarget=true;var b=face.gameObject.AddComponent<CoveButton>();b.targetGraphic=face;b.transition=Selectable.Transition.None;b.navigation=new Navigation{mode=Navigation.Mode.None};b.onClick.AddListener(()=>action());
   if(caption.Length>0)Label(face.transform,"Caption",caption,4,0,w-8,h,12,SportsScoreboard.Ivory,TextAnchor.MiddleCenter);return b;
  }
  void Build(){
   Panel=CoveUI.Rect(transform,Football?"Club Roster details":"Rally Ledger details",0,0,560,250);
   opacity=Panel.gameObject.AddComponent<CanvasGroup>();opacity.alpha=0;opacity.blocksRaycasts=opacity.interactable=false;
   background=Plate(Panel,"Translucent details background",0,0,560,250,Glass(Ink),4);background.raycastTarget=true;
   kicker=Label(Panel,"Details sport",Football?"SUNVALE / MATCH CENTRE":"RALLY COURT / MATCH CENTRE",12,5,400,15,10,Accent);
   heading=Label(Panel,"Details heading",Football?"ON THE PITCH":"LIVE BOX SCORE",12,20,400,25,18,SportsScoreboard.Ivory);
   if(Football){
    focus=CoveUI.Rect(Panel,"Selected football player",12,51,536,58);
    var jersey=Plate(focus,"Selected shirt",0,4,36,42,Glass(Accent,.78f),4);
    selectedNumber=Label(jersey.transform,"Selected number","",0,0,36,42,23,Ink,TextAnchor.MiddleCenter);
    selectedTeam=Label(focus,"Selected team","",46,0,195,17,10,Accent);
    selectedName=Label(focus,"Selected player name","",46,17,195,35,15,SportsScoreboard.Ivory);GameLocalization.UserContent(selectedName);
    string[] names={"GOALS","ASSISTS","PASSES","INTERCEPTS"};
    for(int i=0;i<4;i++){selectedStats[i]=Label(focus,"Selected "+names[i],"—",250+i*66,8,62,25,20,SportsScoreboard.Ivory,TextAnchor.MiddleCenter);Label(focus,names[i],names[i],250+i*66,34,62,18,9,SportsScoreboard.Ivory,TextAnchor.MiddleCenter);}
   }
   for(int i=0;i<2;i++){
    var team=teams[i]=new TeamView();team.root=CoveUI.Rect(Panel,i==0?"Blue player list":"Red player list",12,Football?114:53,260,160);
    team.label=Label(team.root,"Team label",i==0?"BLUE TEAM":"RED TEAM",0,0,190,20,11,i==0?SportsScoreboard.Blue:SportsScoreboard.Red);
    team.total=Label(team.root,"Team score","0",210,-4,50,28,17,SportsScoreboard.Ivory,TextAnchor.MiddleRight);
    Plate(team.root,"Team identity",0,23,260,2,i==0?SportsScoreboard.Blue:SportsScoreboard.Red);
    team.columns=Label(team.root,"Column names","",0,27,260,16,9,SportsScoreboard.Ivory);
    team.viewport=CoveUI.Rect(team.root,"Roster viewport",0,46,260,100);var hit=team.viewport.gameObject.AddComponent<Image>();hit.color=Color.clear;hit.raycastTarget=true;team.viewport.gameObject.AddComponent<RectMask2D>();
    team.content=CoveUI.Rect(team.viewport,"Player rows",0,0,260,100);
    team.scroll=team.viewport.gameObject.AddComponent<ScrollRect>();team.scroll.content=team.content;team.scroll.viewport=team.viewport;team.scroll.horizontal=false;team.scroll.vertical=true;team.scroll.movementType=ScrollRect.MovementType.Clamped;team.scroll.scrollSensitivity=24;team.scroll.inertia=false;
    team.more=Label(team.root,"Scroll hint","",0,147,260,14,9,SportsScoreboard.Ivory,TextAnchor.MiddleRight);
   }
   empty=Label(Panel,"No player stats","Player statistics appear when a match starts.",20,90,520,65,14,SportsScoreboard.Ivory,TextAnchor.MiddleCenter);
   note=Label(Panel,"Live details note","Match continues · Tap score to close",12,222,350,26,10,SportsScoreboard.Ivory);
   close=Button(Panel,"Close player statistics",Football?"Back to pitch":"Back to court",418,220,130,28,()=>SetExpanded(false));
   CharacterIdentityBadges.ReserveHUD(Panel);Panel.gameObject.SetActive(false);
  }
  public void Toggle()=>SetExpanded(!Expanded);
  public void SetExpanded(bool value){
   if(value&&(!owner||!owner.gameObject.activeInHierarchy||!owner.Frame.ready||owner.Frame.phase=="CHOOSE TEAMS"))return;
   if(value&&current&&current!=this)current.SetExpanded(false);
   Expanded=value;if(value){current=this;Panel.gameObject.SetActive(true);Panel.SetAsLastSibling();Refresh();Layout();}
   else {if(current==this)current=null;closedFrame=Time.frameCount;}
   opacity.blocksRaycasts=opacity.interactable=value;
   // Cancel an already held action, without blocking subsequent movement or controls.
   if(PlayerView.Instance)PlayerView.Instance.ClearMatchInput();
   if(EventSystem.current&&EventSystem.current.currentSelectedGameObject&&EventSystem.current.currentSelectedGameObject.transform.IsChildOf(transform))EventSystem.current.SetSelectedGameObject(null);
  }
  public bool SelectPlayer(ulong id){if(!players.Exists(p=>p.id==id))return false;SelectedPlayer=id;PresentSelection();return true;}
  public void HandleBack(){if(Expanded)SetExpanded(false);}
  void Update(){
   if(!owner)return;
   if(Expanded&&(owner.Frame.context!=context||owner.Frame.practice!=practice||owner.Frame.phase=="CHOOSE TEAMS"||!owner.Frame.ready||PlayerView.Instance&&PlayerView.Instance.InputBlocked))SetExpanded(false);
   if(Input.GetKeyDown(KeyCode.Escape)&&Expanded&&!RoomChatUI.BlocksBack){HandleBack();return;}
   if(Input.GetKeyDown(KeyCode.Tab)&&owner.Live&&!RoomChatUI.BlocksInput&&AppRoot.Instance&&AppRoot.Instance.Exploring&&PlayerView.Instance&&!PlayerView.Instance.InputBlocked)Toggle();
   motion=MenuPreferences.ReducedMotion?(Expanded?1:0):Mathf.MoveTowards(motion,Expanded?1:0,Time.unscaledDeltaTime/.24f);
   if(Panel){float eased=1-Mathf.Pow(1-motion,3);opacity.alpha=eased;Panel.localScale=new Vector3(1,.965f+.035f*eased,1);if(!Expanded&&motion<=0)Panel.gameObject.SetActive(false);}
  }
  void LateUpdate(){if(!owner||!Panel||!Panel.gameObject.activeSelf)return;Layout();if(Expanded&&Time.unscaledTime>=nextRefresh)Refresh();heading.enabled=kicker.enabled=!owner.Animating;}
  void OnDisable(){Expanded=false;motion=0;if(current==this)current=null;if(Panel)Panel.gameObject.SetActive(false);}
  void OnDestroy(){if(current==this)current=null;}
  void OnApplicationFocus(bool focused){if(!focused&&Expanded)SetExpanded(false);}
  void OnApplicationPause(bool paused){if(paused&&Expanded)SetExpanded(false);}
  void Refresh(){
   nextRefresh=Time.unscaledTime+.1f;context=owner.Frame.context;practice=owner.Frame.practice;players.Clear();
   if(!practice){
    if(Football&&FootballMatch.Instance)foreach(var p in FootballMatch.Instance.PlayerRecords)players.Add(new ScoreboardPlayerData{id=p.id,team=p.team,name=p.name.ToString(),number=p.number,score=p.goals,assists=p.assists,passes=p.passes,interceptions=p.interceptions});
    else if(!Football&&BasketballMatch.Instance)foreach(var p in BasketballMatch.Instance.PlayerRecords)players.Add(new ScoreboardPlayerData{id=p.id,team=p.team,name=p.name.ToString(),number=p.number,score=p.points,assists=p.assists,passes=p.passes,interceptions=p.interceptions});
   }
   players.RemoveAll(p=>p.team==FootballTeam.None);players.Sort((a,b)=>{int n=a.team.CompareTo(b.team);if(n==0)n=a.number.CompareTo(b.number);return n==0?a.id.CompareTo(b.id):n;});
   if(!players.Exists(p=>p.id==SelectedPlayer)){var app=AppRoot.Instance;ulong own=app&&app.LocalAthlete?(Football?FootballBots.Id(app.LocalAthlete):BasketballMatch.PlayerId(app.LocalAthlete)):ulong.MaxValue;SelectedPlayer=players.Exists(p=>p.id==own)?own:players.Count>0?players[0].id:ulong.MaxValue;}
   for(int i=0;i<2;i++){var team=teams[i];int row=0;foreach(var player in players)if(player.team==(i==0?FootballTeam.A:FootballTeam.B)){if(row>=team.rows.Count)team.rows.Add(BuildRow(team));PresentRow(team.rows[row++],player);}
    for(int j=row;j<team.rows.Count;j++)team.rows[j].root.gameObject.SetActive(false);team.total.text=(i==0?owner.Frame.a:owner.Frame.b).ToString();
   }
   empty.gameObject.SetActive(players.Count==0);empty.text=practice?"SHARED PRACTICE\n"+owner.Frame.a+" points · "+owner.Frame.made+" / "+owner.Frame.attempts+" baskets\nIndividual match stats start with a team match.":"Player statistics appear when a match starts.";
   foreach(var team in teams)team.root.gameObject.SetActive(players.Count>0);if(focus)focus.gameObject.SetActive(players.Count>0);
   note.text=practice?"Shared court totals · Tap score to close":"Match continues · Tap score to close";PresentSelection();Layout();
  }
  Row BuildRow(TeamView team){
   var row=new Row();row.root=CoveUI.Rect(team.content,"Player",0,0,260,28);row.selected=Plate(row.root,"Player highlight",0,0,260,28,Color.clear,2);
   if(Football){row.selected.raycastTarget=true;row.button=row.selected.gameObject.AddComponent<CoveButton>();row.button.targetGraphic=row.selected;row.button.transition=Selectable.Transition.None;row.button.navigation=new Navigation{mode=Navigation.Mode.None};row.button.onClick.AddListener(()=>SelectPlayer(row.id));}
   row.number=Label(row.root,"Shirt number","",2,0,23,28,11,SportsScoreboard.Ivory);row.name=Label(row.root,"Player name","",29,0,120,28,12,SportsScoreboard.Ivory);GameLocalization.UserContent(row.name);
   row.stats=new Text[Football?2:4];for(int i=0;i<row.stats.Length;i++)row.stats[i]=Label(row.root,new[]{Football?"Goals":"Points","Assists","Passes","Interceptions"}[i],"0",150+i*27,0,27,28,Football?13:12,SportsScoreboard.Ivory,TextAnchor.MiddleCenter);return row;
  }
  void PresentRow(Row row,ScoreboardPlayerData p){
   row.id=p.id;row.root.name="Player "+p.id;row.root.gameObject.SetActive(true);row.number.text=p.number.ToString();row.number.color=p.team==FootballTeam.A?SportsScoreboard.Blue:SportsScoreboard.Red;
   var app=AppRoot.Instance;bool own=app&&app.LocalAthlete&&p.id==(Football?FootballBots.Id(app.LocalAthlete):BasketballMatch.PlayerId(app.LocalAthlete));
   row.name.text=ShortName(p.name,own?(Football?12:8):(Football?17:12))+(own?" · "+GameLocalization.Translate("YOU"):"");row.stats[0].text=p.score.ToString();row.stats[1].text=p.assists.ToString();if(!Football){row.stats[2].text=p.passes.ToString();row.stats[3].text=p.interceptions.ToString();}
   row.selected.color=Glass(Accent,Football&&p.id==SelectedPlayer?.16f:own?.08f:0);
  }
  void PresentSelection(){
   if(!Football)return;var index=players.FindIndex(p=>p.id==SelectedPlayer);if(index<0){selectedName.text="";return;}var p=players[index];
   selectedName.text=ShortName(p.name,26);selectedNumber.text=p.number.ToString();selectedTeam.text=(p.team==FootballTeam.A?"BLUE":"RED")+" TEAM";
   selectedStats[0].text=p.score.ToString();selectedStats[1].text=p.assists.ToString();selectedStats[2].text=p.passes.ToString();selectedStats[3].text=p.interceptions.ToString();
   foreach(var team in teams)foreach(var row in team.rows)row.selected.color=Glass(Accent,row.id==p.id?.16f:0);
  }
  void Layout(){
   if(!Panel||!owner)return;var r=(RectTransform)transform;var app=AppRoot.Instance;var safe=app?app.SafeFrame:null;if(!safe)return;
   safe.GetWorldCorners(corners);var lower=r.InverseTransformPoint(corners[0]);var upper=r.InverseTransformPoint(corners[2]);float visibleWidth=upper.x-lower.x,pixelsPerUnit=Screen.safeArea.width/Mathf.Max(1,visibleWidth);
   Top=r.rect.height+8;width=Football?Mathf.Clamp(visibleWidth-394,406,600):Mathf.Clamp(visibleWidth-280,470,600);width=Mathf.Min(width,visibleWidth-24);
   float available=-lower.y-Top-Mathf.Min(100,visibleWidth*.12f);height=Mathf.Clamp(available,180,Football?302:224);
   rowHeight=Football?Mathf.Max(24,44/Mathf.Max(.5f,pixelsPerUnit)):18;
   float rosterTop=Football?108:47;rosterHeight=Mathf.Max(35,height-rosterTop-85);if(Football)rosterHeight=Mathf.Max(rowHeight,Mathf.Floor(rosterHeight/rowHeight)*rowHeight);
   var size=new Vector2(width,height);bool resize=size!=previousSize;previousSize=size;
   float x=Football?lower.x+142+Mathf.Max(0,visibleWidth-394-width)*.5f:(r.rect.width-width)*.5f;x=Mathf.Clamp(x,lower.x+12,upper.x-width-12);
   Panel.anchoredPosition=new Vector2(x,-Top-(1-motion)*8);Panel.sizeDelta=size;background.rectTransform.sizeDelta=size;
   close.GetComponent<RectTransform>().anchoredPosition=new Vector2(width-134,-(height-35));note.rectTransform.anchoredPosition=new Vector2(12,-(height-33));note.rectTransform.sizeDelta=new Vector2((width-160)*2,52);
   empty.rectTransform.anchoredPosition=new Vector2(20,-62);empty.rectTransform.sizeDelta=new Vector2((width-40)*2,(height-100)*2);
   if(focus){focus.sizeDelta=new Vector2(width-24,58);float nameWidth=Mathf.Max(100,width*.28f);selectedName.rectTransform.sizeDelta=new Vector2(nameWidth*2,70);selectedTeam.rectTransform.sizeDelta=new Vector2(nameWidth*2,34);float start=55+nameWidth,sw=(width-24-start)/4;for(int i=0;i<4;i++){var t=selectedStats[i];t.rectTransform.anchoredPosition=new Vector2(start+i*sw,0);t.rectTransform.sizeDelta=new Vector2(sw*2,70);var cap=focus.Find(new[]{"GOALS","ASSISTS","PASSES","INTERCEPTS"}[i]) as RectTransform;cap.anchoredPosition=new Vector2(start+i*sw,-34);cap.sizeDelta=new Vector2(sw*2,36);cap.GetComponent<Text>().text=i==3&&sw<65?"INT":new[]{"GOALS","ASSISTS","PASSES","INTERCEPTS"}[i];}}
   float column=(width-36)/2;for(int i=0;i<2;i++){
    var team=teams[i];team.root.anchoredPosition=new Vector2(12+i*(column+12),-rosterTop);team.root.sizeDelta=new Vector2(column,46+rosterHeight+14);team.total.rectTransform.anchoredPosition=new Vector2(column-50,4);
    ((RectTransform)team.root.Find("Team identity")).sizeDelta=new Vector2(column,2);team.viewport.sizeDelta=new Vector2(column,rosterHeight);
    int count=0;foreach(var row in team.rows)if(row.root.gameObject.activeSelf){row.root.anchoredPosition=new Vector2(0,-count++*rowHeight);row.root.sizeDelta=new Vector2(column,rowHeight);row.selected.rectTransform.sizeDelta=row.root.sizeDelta;row.number.rectTransform.sizeDelta=new Vector2(46,rowHeight*2);
     float statsWidth=Football?72:120,nameWidth=column-29-statsWidth;row.name.rectTransform.sizeDelta=new Vector2(nameWidth*2,rowHeight*2);for(int j=0;j<row.stats.Length;j++){float sw=statsWidth/row.stats.Length;row.stats[j].rectTransform.anchoredPosition=new Vector2(column-statsWidth+j*sw,0);row.stats[j].rectTransform.sizeDelta=new Vector2(sw*2,rowHeight*2);}}
    team.content.sizeDelta=new Vector2(column,count*rowHeight);team.scroll.vertical=count*rowHeight>rosterHeight;
    team.more.text="";
    team.columns.text="PLAYER";team.columns.rectTransform.sizeDelta=new Vector2(column*2,32);
    var headers=team.root.Find("Stat columns");if(!headers){var group=CoveUI.Rect(team.root,"Stat columns",0,27,column,16);foreach(string caption in Football?new[]{"G","AST"}:new[]{"PTS","AST","PASS","INT"})Label(group,caption,caption,0,0,30,16,9,SportsScoreboard.Ivory,TextAnchor.MiddleCenter);headers=group;}
    float headerWidth=Football?72:120;for(int j=0;j<headers.childCount;j++){var t=(RectTransform)headers.GetChild(j);float sw=headerWidth/headers.childCount;t.anchoredPosition=new Vector2(column-headerWidth+j*sw,0);t.sizeDelta=new Vector2(sw*2,32);}
    if(resize)team.scroll.verticalNormalizedPosition=1;
   }
   if(!practice&&(teams[0].scroll.vertical||teams[1].scroll.vertical))note.text="Match continues · Scroll player lists";
  }
 }
}
