using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 // Presentation only: Results() remains the sole source of ordering and scores.
 public sealed class GolfLeaderboardUI:MonoBehaviour {
  sealed class ScoreRow {
   public RectTransform rect,avatar;public Image art,portrait,status;public Image[] cells=new Image[5];public Text[] text=new Text[5];
  }
  Text footer,timer,notice;Image titleArt,frameArt,columnArt;RectTransform page,board,frame,columnBand,viewport,content,countdownCard,noticeCard;
  Button toggle,backdrop;ScrollRect scroll;Scrollbar scrollbar;RectTransform scrollTrack;GolfHUDLayout layout;Font fallbackFont;
  readonly List<ScoreRow> rows=new();readonly List<int> selected=new();readonly List<ulong> displayedIds=new();
  readonly Dictionary<string,Sprite> sprites=new();
  readonly float[] columnShares={.29f,.175f,.143f,.19f,.202f};
  static readonly string[] ColumnNames={"Player","Progress","Strokes","Status","Finish time"};
  bool expanded,aimSummary,pointerInside;double lastRound=double.NaN;int visibleRows;float renderedRowHeight,hoverScale=1;
  public RectTransform BoardRect=>board;
  public float RenderedHeight=>board?board.rect.height*board.localScale.y:0;
  public RectTransform CountdownRect=>countdownCard;
  public RectTransform NoticeRect=>noticeCard;
  public RectTransform RowViewport=>viewport;
  public Button ToggleButton=>toggle;
  public Button BackdropButton=>backdrop;
  public ScrollRect RankingScroll=>scroll;
  public bool Expanded=>expanded;
  public bool AimSummary=>aimSummary;
  public float HoverScaleFactor=>hoverScale;
  internal void SetHovered(bool value){pointerInside=value;}
  public int VisiblePlayerCount=>visibleRows;
  public IReadOnlyList<ulong> DisplayedPlayerIds=>displayedIds;
  public bool CountdownVisible=>timer&&timer.gameObject.activeInHierarchy;
  public string CountdownText=>timer?timer.text:"";
  public static void Create(RectTransform page,Font font){var ui=Rect("Golf HUD",page,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero).gameObject.AddComponent<GolfLeaderboardUI>();ui.Build(page,font);page.gameObject.AddComponent<GolfTargetIndicator>().Build(page,font);}
  public static RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=position;r.sizeDelta=size;return r;}
  public static Text Text(string name,Transform parent,Font font,Vector2 anchor,Vector2 position,Vector2 size,int points){var r=Rect(name,parent,anchor,position,size);var t=r.gameObject.AddComponent<Text>();t.font=font;t.fontSize=points;t.color=Color.white;t.alignment=TextAnchor.MiddleLeft;t.raycastTarget=false;t.supportRichText=false;var shadow=t.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.65f);shadow.effectDistance=new Vector2(1,-1);return t;}
  Sprite Sprite(string name){if(!sprites.TryGetValue(name,out var value))sprites[name]=value=Resources.Load<Sprite>("GolfUI/Leaderboard/"+name);return value;}
  Text Label(string name,Transform parent,int points,TextAnchor alignment=TextAnchor.MiddleCenter,Color? color=null){
   var label=GolfHUDStyle.Text(name,parent,"",Vector2.zero,points,alignment,color);label.font=CoveUI.DisplayFont??fallbackFont;label.fontStyle=FontStyle.Normal;label.horizontalOverflow=HorizontalWrapMode.Overflow;label.verticalOverflow=VerticalWrapMode.Truncate;
   var outline=label.gameObject.AddComponent<Outline>();outline.effectColor=Color.white;outline.effectDistance=new Vector2(1.1f,-1.1f);return label;
  }
  static void Place(RectTransform rect,Vector2 anchor,Vector2 position,Vector2 size){rect.anchorMin=rect.anchorMax=anchor;rect.anchoredPosition=position;rect.sizeDelta=size;}
  static void Stretch(RectTransform rect){rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;}
  Image Art(RectTransform parent,string name,bool sliced=false){var rect=Rect("Leaderboard art "+name,parent,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);Stretch(rect);var image=rect.gameObject.AddComponent<Image>();image.sprite=Sprite(name);image.type=sliced?Image.Type.Sliced:Image.Type.Simple;image.raycastTarget=false;return image;}
  Button Touch(RectTransform rect){var image=rect.gameObject.AddComponent<Image>();image.color=Color.clear;var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=image;button.transition=Selectable.Transition.None;rect.gameObject.AddComponent<CoveFeedback>().enabled=false;return button;}
  void Build(RectTransform parent,Font font){
   page=parent;fallbackFont=font;layout=Resources.Load<GolfHUDLayout>("GolfUILayout");if(!layout)layout=ScriptableObject.CreateInstance<GolfHUDLayout>();
   var shade=Rect("Golf ranking backdrop",page,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);Stretch(shade);backdrop=Touch(shade);shade.GetComponent<Image>().color=new Color(.025f,.16f,.2f,.54f);backdrop.onClick.AddListener(()=>SetExpanded(false));shade.gameObject.SetActive(false);
   board=Rect("Golf leaderboard",page,Vector2.one,Vector2.zero,Vector2.zero);board.pivot=Vector2.one;
   board.gameObject.AddComponent<GolfRankingHover>().owner=this;
   frame=Rect("Golf leaderboard frame",board,new Vector2(0,1),Vector2.zero,Vector2.zero);frame.pivot=new Vector2(0,1);frameArt=Art(frame,"Frame",true);frameArt.raycastTarget=true;
   titleArt=Art(Rect("Golf leaderboard title",board,new Vector2(.5f,1),Vector2.zero,Vector2.zero),"Title");titleArt.preserveAspect=true;
   columnBand=Rect("Golf column band",board,new Vector2(0,1),Vector2.zero,Vector2.zero);columnArt=Art(columnBand,"Columns");
   viewport=Rect("Golf ranking viewport",board,new Vector2(0,1),Vector2.zero,Vector2.zero);viewport.pivot=new Vector2(0,1);var input=viewport.gameObject.AddComponent<Image>();input.color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();
   content=Rect("Golf ranking content",viewport,new Vector2(0,1),Vector2.zero,Vector2.zero);content.pivot=new Vector2(0,1);
   scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.vertical=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=32;
   scrollTrack=Rect("Golf ranking scrollbar",board,new Vector2(0,1),Vector2.zero,Vector2.zero);var track=scrollTrack.gameObject.AddComponent<Image>();track.color=new Color(.88f,.77f,.48f,.5f);
   var slide=Rect("Golf ranking scrollbar slide",scrollTrack,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);Stretch(slide);
   var thumb=Rect("Golf ranking scrollbar handle",slide,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero);Stretch(thumb);var thumbArt=Art(thumb,"LocalCell",true);thumbArt.raycastTarget=true;
   scrollbar=scrollTrack.gameObject.AddComponent<Scrollbar>();scrollbar.handleRect=thumb;scrollbar.targetGraphic=thumbArt;scrollbar.direction=Scrollbar.Direction.BottomToTop;scrollbar.transition=Selectable.Transition.None;scroll.verticalScrollbar=scrollbar;
   var toggleRect=Rect("Golf ranking toggle",board,new Vector2(0,1),Vector2.zero,Vector2.zero);toggle=Touch(toggleRect);toggle.onClick.AddListener(()=>SetExpanded(!expanded));
   footer=Label("Your progress",toggleRect,18);Stretch(footer.rectTransform);
   // A large title hit target also opens the full list without tiny touch controls.
   var titleTouch=Touch((RectTransform)titleArt.transform.parent);titleTouch.onClick.AddListener(()=>SetExpanded(!expanded));
   countdownCard=Rect("Golf final countdown card",page,new Vector2(.5f,1),layout.countdownPosition,layout.countdownSize);GolfHUDStyle.Panel(countdownCard,GolfHUDStyle.Ink).raycastTarget=false;
   GolfHUDStyle.Art(countdownCard,"Golf countdown clock","FishingUI/ClockBadge",new Vector2(66,72),new Vector2(-layout.countdownSize.x*.31f,0));
   timer=Label("Final 30 seconds",countdownCard,layout.countdownFontSize,TextAnchor.MiddleCenter,Color.white);Place(timer.rectTransform,new Vector2(.5f,.5f),new Vector2(32,0),new Vector2(layout.countdownSize.x-110,layout.countdownSize.y-12));timer.GetComponent<Outline>().enabled=false;
   noticeCard=Rect("Golf final countdown notice",page,new Vector2(.5f,1),layout.countdownPosition-new Vector2(0,layout.countdownSize.y*.5f+24),new Vector2(330,44));GolfHUDStyle.Panel(noticeCard,GolfHUDStyle.Cream).raycastTarget=false;
   notice=Label("First finisher notice",noticeCard,20);Place(notice.rectTransform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(302,34));
   timer.gameObject.SetActive(false);notice.gameObject.SetActive(false);countdownCard.gameObject.SetActive(false);noticeCard.gameObject.SetActive(false);EnsureRows(10);Layout(0);
  }
  void EnsureRows(int count){
   while(rows.Count<count){int i=rows.Count;var row=new ScoreRow();row.rect=Rect("Golf score row "+i,content,new Vector2(0,1),Vector2.zero,Vector2.zero);row.art=Art(row.rect,"Row",true);
    row.avatar=Rect("Golf player avatar "+i,row.rect,new Vector2(0,.5f),Vector2.zero,Vector2.one*36);row.portrait=Art(row.avatar,"Avatar0");row.portrait.preserveAspect=true;
    for(int j=0;j<5;j++){
     if(j>0){var cell=Rect("Golf value plate "+j,row.rect,new Vector2(0,.5f),Vector2.zero,Vector2.zero);row.cells[j]=Art(cell,j==3?"Pending":"Cell",true);}
     row.text[j]=Label("Player "+i+" "+ColumnNames[j],row.rect,20,j==0?TextAnchor.MiddleLeft:TextAnchor.MiddleCenter);
    }
    row.status=row.cells[3];row.rect.gameObject.SetActive(false);rows.Add(row);
   }
  }
  void SetExpanded(bool value){
   if(value&&aimSummary)return;expanded=value;if(!value)hoverScale=1;scroll.StopMovement();content.anchoredPosition=Vector2.zero;
   if(value){backdrop.transform.SetAsLastSibling();board.SetAsLastSibling();countdownCard.SetAsLastSibling();noticeCard.SetAsLastSibling();}
   backdrop.gameObject.SetActive(value);
  }
  void Layout(int count){
   float margin=layout.margin;
   float width=expanded?Mathf.Min(layout.boardExpandedWidth,page.rect.width-margin*2):Mathf.Min(layout.boardWidth,Mathf.Max(480,page.rect.width*.5f-margin-200));
   float padding=layout.boardPadding,inner=width-padding*2-(expanded?12:0);
   float titleWidth=expanded?width*layout.boardExpandedTitleFraction:Mathf.Min(layout.boardCompactTitleWidth,width);
   float header=titleWidth*Sprite("Title").rect.height/Sprite("Title").rect.width;
   float columns=inner*Sprite("Columns").rect.height/Sprite("Columns").rect.width;
   float toggleHeight=layout.boardToggleHeight;
   float maxHeight=page.rect.height*(expanded?layout.boardExpandedHeightFraction:layout.boardCompactHeightFraction);
   renderedRowHeight=expanded?layout.boardFullRowHeight:layout.boardPreviewRowHeight;
   float available=maxHeight-header-columns-toggleHeight-padding*2;
   float body=count==0?36:expanded?Mathf.Min(Mathf.Max(renderedRowHeight,count*renderedRowHeight),available):count*renderedRowHeight;
   float rowTop=header+columns+4,height=rowTop+body+toggleHeight+padding;
   if(aimSummary){width=Mathf.Min(layout.boardWidth,490);inner=width-padding*2;height=layout.boardAimSummaryHeight;}
   board.anchoredPosition=layout.boardPosition;board.sizeDelta=new Vector2(width,height);
   // Preserve row, badge and header proportions. A crowded preview scales as
   // one card instead of squeezing only its rows/column artwork vertically.
   float hoverTarget=expanded&&pointerInside&&!MenuPreferences.ReducedMotion?layout.boardHoverScale:1;
   hoverScale=Mathf.Lerp(hoverScale,hoverTarget,1-Mathf.Exp(-20*Time.unscaledDeltaTime));
   board.localScale=Vector3.one*(!expanded&&!aimSummary?Mathf.Min(1,maxHeight/height):1)*hoverScale;
   float frameTop=aimSummary?0:header*layout.boardHeaderJoin;
   Place(frame,new Vector2(0,1),new Vector2(0,-frameTop),new Vector2(width,height-frameTop));
   frameArt.sprite=Sprite(aimSummary?"LocalRow":"Frame");
   Place((RectTransform)titleArt.transform.parent,new Vector2(0,1),new Vector2(width*.5f,-header*.5f),new Vector2(titleWidth,header));
   titleArt.transform.parent.gameObject.SetActive(!aimSummary);
   Place(columnBand,new Vector2(0,1),new Vector2(padding+inner*.5f,-header-columns*.5f),new Vector2(inner,columns));columnBand.gameObject.SetActive(!aimSummary);
   Place(viewport,new Vector2(0,1),new Vector2(padding,-rowTop),new Vector2(inner,Mathf.Max(0,body)));viewport.gameObject.SetActive(!aimSummary&&count>0);
   content.sizeDelta=new Vector2(inner,count*renderedRowHeight);scroll.vertical=expanded&&content.rect.height>viewport.rect.height+.5f;
   Place(scrollTrack,new Vector2(0,1),new Vector2(width-padding-3,-rowTop-body*.5f),new Vector2(7,body-6));scrollTrack.gameObject.SetActive(scroll.vertical&&!aimSummary);
   for(int i=0;i<rows.Count;i++){
    var row=rows[i];Place(row.rect,new Vector2(0,1),new Vector2(inner*.5f,-renderedRowHeight*(i+.5f)),new Vector2(inner,renderedRowHeight-4));
    float avatarSize=renderedRowHeight-9;Place(row.avatar,new Vector2(0,.5f),new Vector2(avatarSize*.5f+7,0),Vector2.one*avatarSize);
    float offset=0;for(int j=0;j<5;j++){
     float cell=inner*columnShares[j],left=j==0?avatarSize+15:4;
     var position=new Vector2(offset+left+(cell-left-5)*.5f,0);var cellSize=new Vector2(cell-left-5,renderedRowHeight*(expanded?.64f:.76f));
     if(j>0){Place((RectTransform)row.cells[j].transform.parent,new Vector2(0,.5f),position,cellSize);}
     Place(row.text[j].rectTransform,new Vector2(0,.5f),position,new Vector2(cell-left-5,renderedRowHeight-5));row.text[j].fontSize=expanded?j==0?25:27:j==0?20:22;offset+=cell;
    }
   }
   Place((RectTransform)toggle.transform,new Vector2(0,1),new Vector2(width*.5f,aimSummary?-height*.5f:-rowTop-body-toggleHeight*.5f),new Vector2(inner,aimSummary?height-10:toggleHeight));
   footer.fontSize=aimSummary?20:18;toggle.interactable=!aimSummary;
   countdownCard.anchoredPosition=layout.countdownPosition;countdownCard.sizeDelta=layout.countdownSize;noticeCard.anchoredPosition=layout.countdownPosition-new Vector2(0,layout.countdownSize.y*.5f+24);
  }
  static void FitLine(Text label,string value,int points,int minimum){
   label.text=value;var settings=label.GetGenerationSettings(label.rectTransform.rect.size);settings.fontSize=points;settings.resizeTextForBestFit=false;
   float available=label.rectTransform.rect.width-2,preferred=label.cachedTextGeneratorForLayout.GetPreferredWidth(value,settings)/label.pixelsPerUnit;
   label.fontSize=Mathf.Clamp(Mathf.FloorToInt(points*available/Mathf.Max(available,preferred)),minimum,points);
   if(preferred*label.fontSize/points>available&&value.Length>1){string shortened=value;settings.fontSize=label.fontSize;while(shortened.Length>1&&label.cachedTextGeneratorForLayout.GetPreferredWidth(shortened+"…",settings)/label.pixelsPerUnit>available)shortened=shortened.Substring(0,shortened.Length-1);label.text=shortened+"…";}
  }
  public static string FinishTime(double seconds){var total=(long)Math.Max(0,Math.Floor(seconds));return $"{total/60:00}:{total%60:00}";}
  void Update(){
   var match=GolfMatchManager.Instance;if(!match||!match.Context)return;
   var state=match.State;var ranked=state.Results();var local=match.Player(AppRoot.Instance.LocalAthlete);
   // The shared look pad is created later than this card; interactive ranking
   // controls must remain above that pad in both compact and expanded modes.
   board.SetAsLastSibling();if(countdownCard.gameObject.activeSelf)countdownCard.SetAsLastSibling();if(noticeCard.gameObject.activeSelf)noticeCard.SetAsLastSibling();
   bool aiming=PlayerView.Instance&&PlayerView.Instance.GolfAimRequested;
   board.gameObject.SetActive(!aiming);
   if(lastRound!=state.StartedAt){lastRound=state.StartedAt;SetExpanded(false);}
   aimSummary=aiming;if(aiming&&expanded)SetExpanded(false);
   EnsureRows(ranked.Length);selected.Clear();displayedIds.Clear();
   int localIndex=Array.FindIndex(ranked,r=>r.player.PlayerId==local?.PlayerId);
   if(!aimSummary){for(int i=0;i<(expanded?ranked.Length:Math.Min(3,ranked.Length));i++)selected.Add(i);if(!expanded&&localIndex>=3)selected.Add(localIndex);}
   visibleRows=selected.Count;Layout(visibleRows);
   for(int i=0;i<rows.Count;i++){
    bool visible=i<visibleRows;var row=rows[i];row.rect.gameObject.SetActive(visible);if(!visible)continue;
    int orderedIndex=selected[i];var result=ranked[orderedIndex];var p=result.player;bool isLocal=p.PlayerId==local?.PlayerId;displayedIds.Add(p.PlayerId);
    string prefix=p.IsFinished?result.rank+(result.tie?"= ":". "):p.IsDNF?"":(orderedIndex+1)+". ";
    row.art.sprite=Sprite(isLocal?"LocalRow":"Row");row.portrait.sprite=Sprite("Avatar"+(isLocal?0:1+(int)(p.PlayerId%3)));
    for(int j=1;j<5;j++)if(j!=3)row.cells[j].sprite=Sprite(isLocal?"LocalCell":"Cell");
    bool namedYou=string.Equals(p.PlayerName,"You",StringComparison.OrdinalIgnoreCase);
    FitLine(row.text[0],prefix+(isLocal&&namedYou?"You":p.PlayerName)+(isLocal&&!namedYou?" (you)":""),row.text[0].fontSize,17);
    row.text[0].color=isLocal?Color.white:GolfHUDStyle.Ink;row.text[0].GetComponent<Outline>().effectColor=isLocal?GolfHUDStyle.Ink:Color.white;row.text[0].GetComponent<Outline>().effectDistance=Vector2.one*(isLocal?1.7f:1.1f);
    row.text[1].text=$"{p.CompletedHoleCount} / 5";row.text[2].text=p.TotalStroke.ToString();row.text[4].text=p.IsFinished?FinishTime(p.FinishTime):"--:--";
    row.text[3].text=p.IsFinished?"Finished":p.IsDNF?"DNF":"Playing";
    bool baked=!p.IsDNF;row.status.sprite=Sprite(baked?p.IsFinished?"Finished":"Playing":"Pending");row.status.type=Image.Type.Simple;row.status.preserveAspect=true;row.text[3].enabled=!baked;
    row.text[3].color=Color.white;row.text[3].GetComponent<Outline>().effectColor=new Color(.55f,.29f,.03f);FitLine(row.text[3],row.text[3].text,row.text[3].fontSize,16);
   }
   if(aimSummary)footer.text="";
   else if(ranked.Length==0)footer.text=match.Authority?"Start a match to play all five holes.":"Waiting for the host to start.";
   else footer.text=expanded?$"Close ranking   ×   {ranked.Length} players":$"View all   +   {ranked.Length} players";
   bool countdown=state.CountdownStarted&&state.Phase==GolfMatchPhase.FinalCountdown;
   bool expired=state.Phase==GolfMatchPhase.Ended&&state.Players.Any(p=>p.IsDNF);
   timer.gameObject.SetActive(countdown||expired);notice.gameObject.SetActive(countdown||expired);countdownCard.gameObject.SetActive(countdown||expired);noticeCard.gameObject.SetActive(countdown||expired);
   timer.text=expired?"00:00":FinishTime(Math.Ceiling(match.Remaining));notice.text=expired?"Match finished":"A player has finished!";
  }
  public bool FitsSafeFrame(){
   if(!board||!page)return false;var corners=new Vector3[4];board.GetWorldCorners(corners);var area=page.rect;
   return corners.All(c=>{var p=page.InverseTransformPoint(c);return p.x>=area.xMin-.5f&&p.x<=area.xMax+.5f&&p.y>=area.yMin-.5f&&p.y<=area.yMax+.5f;});
  }
  public bool RowsReadable(){
   for(int i=0;i<visibleRows;i++)foreach(var label in rows[i].text)if(label.enabled&&(label.fontSize<16||label.cachedTextGenerator.vertexCount==0))return false;return true;
  }
  public bool DividersVisible()=>columnArt&&columnArt.sprite&&columnBand.gameObject.activeInHierarchy&&columnBand.rect.width>0;
 }
 // Pointer events bubble from all of the panel's controls. Animate the common
 // root so title, frame, rows, labels and scrollbar always share one scale.
 public sealed class GolfRankingHover:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler {
  internal GolfLeaderboardUI owner;
  public void OnPointerEnter(PointerEventData data){if(owner)owner.SetHovered(true);}
  public void OnPointerExit(PointerEventData data){if(owner)owner.SetHovered(false);}
  void OnDisable(){if(owner)owner.SetHovered(false);}
  void OnApplicationFocus(bool focused){if(!focused&&owner)owner.SetHovered(false);}
 }
}
