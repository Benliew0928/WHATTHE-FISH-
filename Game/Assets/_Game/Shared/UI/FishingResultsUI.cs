using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 // Read-only completed-round presentation. The host starts the next round;
 // opening this card never awards or persists a score.
 public sealed class FishingResultsUI:MonoBehaviour {
  sealed class Standing {
   public RectTransform rect,localMark;public FishingStickerGraphic panel;public Text rank,name,score,cupPoints,cupCatch,status;public PlayerReportAction report;
  }
  RectTransform page,modal,card,reopen,crewStrip,summary,notes,footer;
  FishingResultsLayout layout;Font fallback;
  Text title,subtitle,scoreHeading,cupHeading,cupCatchHeading,summaryTitle,notesLine,detailLine,crewScore,crewNeed;
  Text[] statLabels,crewSizeLabels;CoveIcon[] crewChecks;FishingStickerGraphic[] crewFish;
  RectTransform nextAdventure;
  Button closeButton,walkingButton,reopenButton;Standing[] standings;
  FishingGame boundGame;Athlete actor;uint observedRound;bool observed,hasResult;FishingCeremony ceremony;
  FishingMatchSnapshot frozenMatch;FishingPlayerRecord[] frozenPlayers=Array.Empty<FishingPlayerRecord>();string[] frozenNames=Array.Empty<string>();ulong frozenLocal;
  public bool Visible=>modal&&modal.gameObject.activeSelf;
  public bool CrewVisible=>crewStrip&&crewStrip.gameObject.activeSelf;
  public float CrewBottomPadding=>layout?Mathf.Max(0,layout.padding):26;
  public float CrewExtraHeight=>layout?Mathf.Max(layout.crewExtraHeight,(crewStrip?crewStrip.sizeDelta.y:layout.crewSize.y)+CrewBottomPadding):88;
  public RectTransform CrewRect=>crewStrip;
  public RectTransform CardRect=>card;
  public RectTransform ReopenRect=>reopen;
  public Button CloseButton=>closeButton;
  public Button ReopenButton=>reopenButton;
  public int ShownRounds {get;private set;}
  public uint ResultRound=>hasResult?frozenMatch.round:0;
  public string Title=>title?title.text:"";
  public string Conditions=>notesLine?notesLine.text:"";
  public string CrewNeed=>crewNeed?crewNeed.text:"";
  public string[] StatTexts=>statLabels==null?Array.Empty<string>():statLabels.Select(t=>t.text).ToArray();
  public bool ModalBlocksRaycasts=>Visible&&modal.GetComponent<Image>().raycastTarget;
  public static FishingResultsUI Create(RectTransform parent,Font font){
   var node=Rect("Fishing completed round UI",parent,Vector2.zero,Vector2.zero);Stretch(node);var ui=node.gameObject.AddComponent<FishingResultsUI>();ui.page=parent;ui.fallback=font;
   ui.layout=Resources.Load<FishingResultsLayout>("FishingResultsLayout");if(!ui.layout)ui.layout=ScriptableObject.CreateInstance<FishingResultsLayout>();ui.Build();return ui;
  }
  static RectTransform Rect(string name,Transform parent,Vector2 position,Vector2 size)=>GolfHUDStyle.Rect(name,parent,new Vector2(.5f,.5f),position,size);
  static void Stretch(RectTransform node){node.anchorMin=Vector2.zero;node.anchorMax=Vector2.one;node.offsetMin=node.offsetMax=Vector2.zero;}
  Text Label(string name,Transform parent,string value,int size,TextAnchor alignment=TextAnchor.MiddleCenter,Color? color=null){
   var t=GolfHUDStyle.Text(name,parent,value,Vector2.zero,size,alignment,color);if(!t.font)t.font=fallback;t.fontStyle=FontStyle.Normal;t.resizeTextForBestFit=false;return t;
  }
  static void Place(RectTransform node,float x,float y,float width,float height){node.anchorMin=node.anchorMax=new Vector2(0,1);node.pivot=new Vector2(0,1);node.anchoredPosition=new Vector2(x,-y);node.sizeDelta=new Vector2(width,height);}
  static void FitSingleLine(Text label,string value,int maximum){
   label.text=value;
   if(label.name.StartsWith("Fishing final status",StringComparison.Ordinal)){((LocalizedText)label).ManualFit=true;label.horizontalOverflow=HorizontalWrapMode.Wrap;label.resizeTextForBestFit=true;label.resizeTextMinSize=12;label.resizeTextMaxSize=maximum;return;}
   label.horizontalOverflow=HorizontalWrapMode.Overflow;
   var settings=label.GetGenerationSettings(label.rectTransform.rect.size);settings.fontSize=maximum;settings.resizeTextForBestFit=false;
   float available=Mathf.Max(1,label.rectTransform.rect.width-4),preferred=label.cachedTextGeneratorForLayout.GetPreferredWidth(value,settings)/Mathf.Max(.01f,label.pixelsPerUnit);
   label.fontSize=Mathf.Clamp(Mathf.FloorToInt(maximum*available/Mathf.Max(available,preferred)),16,maximum);
  }
  static float LineHeight(Text label,int points){
   // Cove's ascent plus descent is taller than its point size. Reserve the
   // generated line height, including a little rounding room at scaled canvases.
   var settings=label.GetGenerationSettings(Vector2.one*10000);settings.fontSize=points;settings.resizeTextForBestFit=false;settings.horizontalOverflow=HorizontalWrapMode.Overflow;settings.verticalOverflow=VerticalWrapMode.Overflow;
   float preferred=label.cachedTextGeneratorForLayout.GetPreferredHeight("Ag",settings)/Mathf.Max(.01f,label.pixelsPerUnit);return Mathf.Ceil(Mathf.Max(points,preferred))+4;
  }
  Button MakeButton(string name,Transform parent,string value,Color color,Action clicked,out Text caption){
   var rect=Rect(name,parent,Vector2.zero,layout.readySize);var plate=GolfHUDStyle.Panel(rect,color,true);plate.raycastTarget=true;plate.cornerRadius=28;
   var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=plate;button.transition=Selectable.Transition.None;caption=Label(name+" label",rect,value,layout.rowFontSize);Stretch(caption.rectTransform);caption.rectTransform.offsetMin=new Vector2(12,4);caption.rectTransform.offsetMax=new Vector2(-12,-4);
   button.onClick.AddListener(()=>clicked());button.gameObject.AddComponent<GolfFishingButtonSound>();MenuDesigns.Apply(button);return button;
  }
  void Build(){
   reopenButton=MakeButton("Fishing reopen results",transform,"RESULTS",GolfHUDStyle.Cream,Open,out _);reopen=(RectTransform)reopenButton.transform;reopen.gameObject.SetActive(false);
   modal=Rect("Fishing results modal",transform,Vector2.zero,Vector2.zero);Stretch(modal);var modalCanvas=modal.gameObject.AddComponent<Canvas>();modalCanvas.overrideSorting=true;modalCanvas.sortingOrder=300;modal.gameObject.AddComponent<GraphicRaycaster>();var shade=modal.gameObject.AddComponent<Image>();shade.color=new Color(.025f,.11f,.15f,.62f);shade.raycastTarget=true;
   nextAdventure=PostMatchFlow.EndingButton(modal,"Fishing results next adventure");
   card=Rect("Fishing results card",modal,Vector2.zero,Vector2.zero);MenuDesigns.Dialog(GolfHUDStyle.Panel(card,GolfHUDStyle.Cream)).raycastTarget=true;
   var badge=Rect("Fishing results fish badge",card,Vector2.zero,new Vector2(62,62));var fish=badge.gameObject.AddComponent<FishingStickerGraphic>();fish.kind=FishingStickerGraphic.Kind.Fish;fish.tint=GolfHUDStyle.Teal;fish.raycastTarget=false;fish.Decorate();
   title=Label("Fishing result title",card,"ROUND RESULTS",layout.titleFontSize,TextAnchor.MiddleLeft);
   subtitle=Label("Fishing result subtitle",card,"",layout.labelFontSize,TextAnchor.MiddleLeft);
   closeButton=MakeButton("Fishing close results",card,"",GolfHUDStyle.Cream,Close,out var closeLabel);closeLabel.enabled=false;var closeIcon=Rect("Fishing result close icon",closeButton.transform,Vector2.zero,Vector2.one*25).gameObject.AddComponent<CoveIcon>();closeIcon.symbol=CoveSymbol.Close;closeIcon.color=GolfHUDStyle.Ink;closeIcon.raycastTarget=false;
   standings=new Standing[5];
   Label("Fishing results name heading",card,"PLAYER",layout.labelFontSize,TextAnchor.MiddleLeft);
   scoreHeading=Label("Fishing results score heading",card,"ROUND PTS",layout.labelFontSize);
   cupHeading=Label("Fishing results cup heading",card,"CUP PTS",layout.labelFontSize);
   cupCatchHeading=Label("Fishing results cup catch heading",card,"CUP CATCH",layout.labelFontSize);
   Label("Fishing results status heading",card,"STATUS",layout.labelFontSize);
   for(int i=0;i<5;i++){
    var row=new Standing{rect=Rect("Fishing final standing "+i,card,Vector2.zero,Vector2.zero)};row.panel=GolfHUDStyle.Panel(row.rect,LocalProfile.Hex("DFF5F0"),true);row.panel.cornerRadius=16;
    row.localMark=Rect("Local player marker",row.rect,Vector2.zero,new Vector2(4,28));var localMark=row.localMark.gameObject.AddComponent<Image>();localMark.color=GolfHUDStyle.Gold;localMark.raycastTarget=false;
    row.rank=Label("Fishing final rank "+i,row.rect,"",layout.rowFontSize);
    row.name=Label("Fishing final name "+i,row.rect,"",layout.rowFontSize,TextAnchor.MiddleLeft);
    row.score=Label("Fishing final score "+i,row.rect,"",layout.rowFontSize);
    row.cupPoints=Label("Fishing final cup points "+i,row.rect,"",layout.rowFontSize);
    row.cupCatch=Label("Fishing final cup catch "+i,row.rect,"",layout.rowFontSize);
    row.status=Label("Fishing final status "+i,row.rect,"",layout.labelFontSize);row.report=PlayerReportAction.Create(row.rect);standings[i]=row;
   }
   summary=Rect("Fishing local round summary",card,Vector2.zero,Vector2.zero);GolfHUDStyle.Panel(summary,LocalProfile.Hex("D2F2E5"),true).cornerRadius=18;
   summaryTitle=Label("Fishing local round heading",summary,"YOUR ROUND",layout.rowFontSize,TextAnchor.MiddleLeft);
   statLabels=new Text[9];string[] statNames={"Small catch total","Medium catch total","Large catch total","Wanted bonus total","Missed bite total","Snapped line total","Cancelled cast total","Timed out total","Local personal best"};
   for(int i=0;i<9;i++)statLabels[i]=Label("Fishing result "+statNames[i],summary,"",layout.statFontSize,i%3==0?TextAnchor.MiddleLeft:TextAnchor.MiddleCenter);
   notes=Rect("Fishing result conditions",card,Vector2.zero,Vector2.zero);notesLine=Label("Fishing result conditions line",notes,"",layout.labelFontSize,TextAnchor.MiddleLeft);detailLine=Label("Fishing result detail line",notes,"",layout.labelFontSize,TextAnchor.MiddleLeft);
   footer=Rect("Fishing results controls",card,Vector2.zero,Vector2.zero);walkingButton=MakeButton("Fishing result keep walking",footer,"CLOSE",GolfHUDStyle.Teal,Close,out _);
   CardNavigation(walkingButton,walkingButton,walkingButton,closeButton,closeButton);CardNavigation(closeButton,walkingButton,walkingButton,walkingButton,walkingButton);
   modal.gameObject.SetActive(false);Layout();
  }
  static void CardNavigation(Button button,Button left,Button right,Button up,Button down){button.navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnLeft=left,selectOnRight=right,selectOnUp=up,selectOnDown=down};}
  public void AttachCrewStrip(RectTransform clock){
   if(crewStrip)return;crewStrip=Rect("Fishing crew checklist",clock,Vector2.zero,layout.crewSize);crewStrip.gameObject.SetActive(false);
   crewScore=Label("Fishing crew basket total",crewStrip,"",layout.crewScoreFontSize,color:GolfHUDStyle.Gold);crewNeed=Label("Fishing crew contribution need",crewStrip,"",layout.crewFontSize,color:GolfHUDStyle.Cream);
   crewSizeLabels=new Text[3];crewChecks=new CoveIcon[3];crewFish=new FishingStickerGraphic[3];
   for(int i=0;i<3;i++){
    var icon=Rect("Crew required fish "+i,crewStrip,Vector2.zero,Vector2.one*30);crewFish[i]=icon.gameObject.AddComponent<FishingStickerGraphic>();crewFish[i].kind=FishingStickerGraphic.Kind.Fish;crewFish[i].tint=GolfHUDStyle.Teal;crewFish[i].raycastTarget=false;crewFish[i].Decorate();
    crewSizeLabels[i]=Label("Crew fish size "+i,crewStrip,new[]{"SMALL","MED","LARGE"}[i],layout.crewFontSize);
    var check=Rect("Crew fish size check "+i,crewStrip,Vector2.zero,Vector2.one*17);crewChecks[i]=check.gameObject.AddComponent<CoveIcon>();crewChecks[i].symbol=CoveSymbol.Check;crewChecks[i].color=GolfHUDStyle.Teal;crewChecks[i].raycastTarget=false;
   }
   LayoutCrew();
  }
  void LayoutCrew(){
   if(!crewStrip)return;float width=layout.crewSize.x,scoreHeight=LineHeight(crewScore,layout.crewScoreFontSize),sizeHeight=Mathf.Max(30,crewSizeLabels.Max(t=>LineHeight(t,layout.crewFontSize))),needHeight=LineHeight(crewNeed,layout.crewFontSize);
   float height=Mathf.Max(layout.crewSize.y,scoreHeight+sizeHeight+needHeight+4);crewStrip.sizeDelta=new Vector2(width,height);
   Place(crewScore.rectTransform,0,0,width,scoreHeight);Place(crewNeed.rectTransform,0,height-needHeight,width,needHeight);
   for(int i=0;i<3;i++){float x=width*i/3;Place(crewFish[i].rectTransform,x+2,scoreHeight+2,30,30);Place(crewSizeLabels[i].rectTransform,x+33,scoreHeight+2,width/3-51,sizeHeight);FitSingleLine(crewSizeLabels[i],crewSizeLabels[i].text,layout.crewFontSize);Place(crewChecks[i].rectTransform,x+width/3-17,scoreHeight+2+(sizeHeight-17)*.5f,17,17);}
  }
  void TickCrew(FishingGame game){
   if(!crewStrip)return;var m=game.State.Match;bool visible=m.mode==FishingMode.Crew&&(m.phase==FishingRoundPhase.Countdown||m.phase==FishingRoundPhase.Drafting||m.phase==FishingRoundPhase.Playing);crewStrip.gameObject.SetActive(visible);if(!visible)return;
   FitSingleLine(crewScore,"BASKET "+m.crewScore+" / "+m.crewTarget,layout.crewScoreFontSize);
   var needing=game.State.Players.Where(p=>p.connected&&p.catches==0).OrderBy(p=>p.owner==FishingGame.Key(actor)?0:1).ThenBy(p=>p.owner).ToArray();
   string next=needing.Length==0?"":needing[0].owner==FishingGame.Key(actor)?"YOU":game.PlayerName(needing[0].owner);
   FitSingleLine(crewNeed,needing.Length==0?"EVERYONE HAS CAUGHT":"NEEDS A CATCH: "+next+(needing.Length>1?" +"+(needing.Length-1):""),layout.crewFontSize);
   for(int i=0;i<3;i++){bool complete=(m.crewMask&(1<<i))!=0;crewChecks[i].gameObject.SetActive(complete);crewSizeLabels[i].color=complete?GolfHUDStyle.Teal:Color.white;crewFish[i].rectTransform.localScale=Vector3.one*(.8f+i*.1f);}
  }
  bool CanPresent(FishingGame game)=>isActiveAndEnabled&&SceneLifetime.CanPresent(gameObject)&&game&&SceneLifetime.CanPresent(game.gameObject);
  public void Tick(FishingGame game,Athlete local,bool eligible){
   if(!CanPresent(game)){ClearContext();return;}
   if(!game||!game.Context||!local){ClearContext();return;}
   if(boundGame!=game||actor!=local){ClearContext();boundGame=game;actor=local;}
   TickCrew(game);var match=game.State.Match;
   if(match.phase!=FishingRoundPhase.Ended){ceremony?.Cancel();ClearCardFocus();modal.gameObject.SetActive(false);reopen.gameObject.SetActive(false);hasResult=false;return;}
   if(!eligible){ClearCardFocus();modal.gameObject.SetActive(false);reopen.gameObject.SetActive(false);return;}
   if(!observed||observedRound!=match.round){
    var current=game.Player(local);if(!current.HasValue)return;observed=true;observedRound=match.round;
    // A delayed local result can arrive after the shared next-adventure card.
    // The completed-round card must own pointer and controller input first.
    PostMatchFlow.Instance?.Close();
    FishingHUD.Instance?.CloseRoundOverlays();
    Capture(game,current.Value.owner);hasResult=true;ShownRounds++;
    if(!ceremony)ceremony=gameObject.AddComponent<FishingCeremony>();
    bool animate=true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // Deterministic frozen-card fixtures exercise layout without a real round.
    animate=!game.ReviewSnapshot;
#endif
    if(!animate||!ceremony.Begin(game,frozenPlayers,Open))Open();
   }
   if(!hasResult)return;reopen.gameObject.SetActive(!Visible&&!FishingCeremony.Presenting);Layout();
  }
  void Capture(FishingGame game,ulong local){
   frozenMatch=game.State.Match;frozenLocal=local;
   var players=frozenMatch.mode==FishingMode.Crew?game.State.Players.OrderBy(p=>p.owner):game.State.Players.OrderByDescending(p=>frozenMatch.cupFinished?p.cupPoints:p.score).ThenByDescending(p=>frozenMatch.cupFinished?p.cupScore:0).ThenBy(p=>p.owner);
   frozenPlayers=players.Take(5).ToArray();frozenNames=frozenPlayers.Select(p=>game.PlayerName(p.owner)).ToArray();
   title.text=frozenMatch.outcome==FishingOutcome.Abandoned?"ROUND STOPPED":frozenMatch.mode==FishingMode.Crew?(frozenMatch.outcome==FishingOutcome.Success?"CREW COMPLETE":"CREW RESULTS"):frozenMatch.cupFinished?"CUP RESULTS":"ROUND RESULTS";
   subtitle.text=frozenMatch.mode==FishingMode.Cup?"CUP ROUND "+frozenMatch.cupRound+" / 3"+(frozenMatch.cupValid?"":" · CUP INVALID"):"";
   subtitle.gameObject.SetActive(!string.IsNullOrEmpty(subtitle.text));
   bool cup=frozenMatch.mode==FishingMode.Cup;cupHeading.gameObject.SetActive(cup);cupCatchHeading.gameObject.SetActive(cup);
   for(int i=0;i<standings.Length;i++){
    var row=standings[i];bool show=i<frozenPlayers.Length;row.rect.gameObject.SetActive(show);if(!show)continue;var p=frozenPlayers[i];bool mine=p.owner==local;row.report.Bind(p.owner);
    row.panel.Refresh(1,mine?GolfHUDStyle.LocalBlue:LocalProfile.Hex("DFF5F0"));row.localMark.gameObject.SetActive(mine);GameLocalization.UserContent(row.name);
    row.rank.text=frozenMatch.mode==FishingMode.Crew?"":(1+frozenPlayers.Count(q=>frozenMatch.cupFinished?q.cupPoints>p.cupPoints||q.cupPoints==p.cupPoints&&q.cupScore>p.cupScore:q.score>p.score)).ToString();
    row.score.text=p.score.ToString();row.cupPoints.gameObject.SetActive(cup);row.cupCatch.gameObject.SetActive(cup);row.cupPoints.text=p.cupPoints.ToString();row.cupCatch.text=p.cupScore.ToString();row.status.text=!p.connected?"LEFT":frozenMatch.outcome==FishingOutcome.Abandoned?"STOPPED":frozenMatch.mode==FishingMode.Crew?(p.catches>0?"CAUGHT":"NO CATCH"):"FINISHED";
   }
   var me=frozenPlayers.First(p=>p.owner==local);summaryTitle.text="YOUR ROUND · "+me.score+" PTS";
   statLabels[0].text="SMALL × "+me.smallCatches;statLabels[1].text="MEDIUM × "+me.mediumCatches;statLabels[2].text="LARGE × "+me.largeCatches;
   statLabels[3].text="WANTED BONUS +"+me.bonusTotal;statLabels[4].text="MISSED "+me.missedBites;statLabels[5].text="SNAPPED "+me.snappedLines;statLabels[6].text="CANCELLED "+me.cancels;statLabels[7].text="TIMED OUT "+me.timeouts;statLabels[8].text=PersonalBest(me);
   if(frozenMatch.outcome==FishingOutcome.Abandoned){notesLine.text="ROSTER CHANGED · READY TO TRY AGAIN";detailLine.text="This round does not update your personal best.";}
   else if(frozenMatch.mode==FishingMode.Crew){
    string sizes=string.Join(" / ",Enumerable.Range(0,3).Where(i=>(frozenMatch.crewMask&(1<<i))==0).Select(i=>new[]{"SMALL","MEDIUM","LARGE"}[i]));
    notesLine.text="BASKET "+frozenMatch.crewScore+" / "+frozenMatch.crewTarget+" · "+(sizes.Length==0?"ALL SIZES COLLECTED":"MISSING "+sizes);
    string needs=string.Join(", ",frozenPlayers.Select((p,i)=>new{p,name=frozenNames[i]}).Where(p=>p.p.catches==0).Select(p=>p.name));
    detailLine.text=needs.Length==0?(frozenMatch.outcome==FishingOutcome.Success?new[]{"","BRONZE","SILVER","GOLD"}[Mathf.Clamp(frozenMatch.medal,0,3)]+" · EVERYONE CONTRIBUTED":"EVERYONE CAUGHT · CREW GOALS INCOMPLETE"):"NEEDS A CATCH: "+needs;
   }else {notesLine.text=cup&&!frozenMatch.cupValid?"CUP INVALID · ROSTER CHANGED":"";detailLine.text="";}
   notes.gameObject.SetActive(!string.IsNullOrEmpty(notesLine.text)||!string.IsNullOrEmpty(detailLine.text));
   Layout();
  }
  string PersonalBest(FishingPlayerRecord local){
   string key="fishing.v2.best."+frozenMatch.mode+"."+frozenPlayers.Length;
   if(frozenMatch.mode!=FishingMode.Crew){int best=PlayerPrefs.GetInt(key,0);if(frozenMatch.outcome!=FishingOutcome.Abandoned)best=Math.Max(best,local.score);return "LOCAL BEST "+best+" PTS";}
   float elapsed=PlayerPrefs.GetFloat(key,float.MaxValue);if(frozenMatch.outcome==FishingOutcome.Success)elapsed=Mathf.Min(elapsed,(float)(frozenMatch.ended-frozenMatch.started));return elapsed==float.MaxValue?"LOCAL BEST —":"LOCAL BEST "+GolfLeaderboardUI.FinishTime(elapsed);
  }
  void FocusCard(){if(EventSystem.current)EventSystem.current.SetSelectedGameObject(walkingButton.gameObject);}
  void ClearCardFocus(){var events=EventSystem.current;if(events&&events.currentSelectedGameObject&&events.currentSelectedGameObject.transform.IsChildOf(transform))events.SetSelectedGameObject(null);}
  public void Close()=>HideCard(true);
  void HideCard(bool allowReopen){ClearCardFocus();if(modal)modal.gameObject.SetActive(false);if(reopen)reopen.gameObject.SetActive(allowReopen&&hasResult&&CanPresent(boundGame)&&SceneLifetime.CanPresent(reopen.gameObject));}
  public void Open(){if(!hasResult||!CanPresent(boundGame)||!boundGame.Context||boundGame.State.Match.phase!=FishingRoundPhase.Ended||boundGame.State.Match.round!=frozenMatch.round)return;PostMatchFlow.Instance?.Close();modal.gameObject.SetActive(true);reopen.gameObject.SetActive(false);FocusCard();Layout();}
  public void ClearContext(){ceremony?.Cancel();ClearCardFocus();if(modal)modal.gameObject.SetActive(false);if(reopen)reopen.gameObject.SetActive(false);if(crewStrip)crewStrip.gameObject.SetActive(false);boundGame=null;actor=null;hasResult=observed=false;frozenPlayers=Array.Empty<FishingPlayerRecord>();frozenNames=Array.Empty<string>();}
  void Layout(){
   PostMatchFlow.LayoutEndingButton(nextAdventure);nextAdventure.SetAsLastSibling();
   if(!page||!card)return;float width=Mathf.Max(400,layout.cardWidth),pad=layout.padding,gap=layout.gap,inner=width-pad*2;
   float titleHeight=LineHeight(title,layout.titleFontSize),subtitleHeight=subtitle.gameObject.activeSelf?LineHeight(subtitle,layout.labelFontSize):0,headerHeight=subtitleHeight>0?Mathf.Max(layout.headerHeight,titleHeight+subtitleHeight):Mathf.Max(66,titleHeight);
   float columnHeight=Mathf.Max(layout.columnHeight,LineHeight(scoreHeading,layout.labelFontSize)),rowHeight=Mathf.Max(layout.rowHeight,LineHeight(standings[0].name,layout.rowFontSize)+5);
   if(frozenMatch.mode==FishingMode.Cup){var settings=cupCatchHeading.GetGenerationSettings(new Vector2(inner*.14f,1000));settings.fontSize=layout.labelFontSize;settings.resizeTextForBestFit=false;settings.horizontalOverflow=HorizontalWrapMode.Wrap;settings.verticalOverflow=VerticalWrapMode.Overflow;columnHeight=Mathf.Max(columnHeight,cupCatchHeading.cachedTextGeneratorForLayout.GetPreferredHeight(cupCatchHeading.text,settings)/cupCatchHeading.pixelsPerUnit+4);}
   float summaryHeadingHeight=Mathf.Max(38,LineHeight(summaryTitle,layout.rowFontSize)),statHeight=statLabels.Max(t=>LineHeight(t,layout.statFontSize)),summaryHeight=Mathf.Max(layout.summaryHeight,summaryHeadingHeight+4+statHeight*3);
   float notesLineHeight=Mathf.Max(LineHeight(notesLine,layout.labelFontSize),LineHeight(detailLine,layout.labelFontSize)),notesHeight=notes.gameObject.activeSelf?Mathf.Max(layout.notesHeight,notesLineHeight*2):0;
   int count=Math.Max(1,frozenPlayers.Length);float y=pad,height=pad*2+headerHeight+columnHeight+count*rowHeight+summaryHeight+notesHeight+layout.footerHeight+gap*4;
   card.sizeDelta=new Vector2(width,height);card.localScale=Vector3.one*Mathf.Min(1,Mathf.Max(.05f,Mathf.Min((page.rect.width-layout.margin*2)/width,(page.rect.height-layout.margin*2)/height)));
   Place(card.Find("Fishing results fish badge") as RectTransform,pad,y+4,62,62);Place(title.rectTransform,pad+76,y,inner-layout.closeSize.x-92,titleHeight);Place(subtitle.rectTransform,pad+76,y+titleHeight,inner-76,subtitleHeight);Place((RectTransform)closeButton.transform,width-pad-layout.closeSize.x,y,layout.closeSize.x,layout.closeSize.y);y+=headerHeight;
   bool cup=frozenMatch.mode==FishingMode.Cup;float nameX=inner*.067f,nameWidth=inner*(cup?.30f:.47f),scoreX=inner*(cup?.39f:.58f),scoreWidth=inner*(cup?.14f:.17f),statusX=inner*.84f,statusWidth=inner*.16f;
   Place(card.Find("Fishing results name heading") as RectTransform,pad+nameX,y,nameWidth,columnHeight);Place(scoreHeading.rectTransform,pad+scoreX,y,scoreWidth,columnHeight);Place(cupHeading.rectTransform,pad+inner*.54f,y,inner*.14f,columnHeight);Place(cupCatchHeading.rectTransform,pad+inner*.69f,y,inner*.14f,columnHeight);Place(card.Find("Fishing results status heading") as RectTransform,pad+statusX,y,statusWidth,columnHeight);y+=columnHeight;
   for(int i=0;i<standings.Length;i++){
    var row=standings[i];Place(row.rect,pad,y+i*rowHeight,inner,rowHeight-5);Place(row.localMark,4,9,4,rowHeight-23);Place(row.rank.rectTransform,10,0,inner*.06f-6,rowHeight-5);Place(row.name.rectTransform,nameX,0,nameWidth-48,rowHeight-5);row.report.Place(nameX+nameWidth-46,(rowHeight-49)*.5f,44,44);Place(row.score.rectTransform,scoreX,0,scoreWidth,rowHeight-5);Place(row.cupPoints.rectTransform,inner*.54f,0,inner*.14f,rowHeight-5);Place(row.cupCatch.rectTransform,inner*.69f,0,inner*.14f,rowHeight-5);Place(row.status.rectTransform,statusX,0,statusWidth,rowHeight-5);FitSingleLine(row.status,row.status.text,layout.labelFontSize);
    if(i<frozenNames.Length){bool mine=frozenPlayers[i].owner==frozenLocal;string name=frozenNames[i];row.name.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,nameWidth-(row.report.gameObject.activeSelf?48:0));IslandResultName.Fit(row.name,name,"",mine&&!string.Equals(name,"YOU",StringComparison.OrdinalIgnoreCase)?GameLocalization.Translate(" · YOU"):"",layout.rowFontSize,16);}
   }
   y+=count*rowHeight+gap;Place(summary,pad,y,inner,summaryHeight);Place(summaryTitle.rectTransform,16,0,inner-32,summaryHeadingHeight);
   float cell=(inner-32)/3,line=(summaryHeight-summaryHeadingHeight-4)/3;
   for(int i=0;i<9;i++){Place(statLabels[i].rectTransform,16+(i%3)*cell,summaryHeadingHeight+4+(i/3)*line,cell-10,line);FitSingleLine(statLabels[i],statLabels[i].text,layout.statFontSize);}y+=summaryHeight+gap;
   Place(notes,pad,y,inner,notesHeight);Place(notesLine.rectTransform,0,0,inner,notesHeight*.5f);Place(detailLine.rectTransform,0,notesHeight*.5f,inner,notesHeight*.5f);FitSingleLine(notesLine,notesLine.text,layout.labelFontSize);FitSingleLine(detailLine,detailLine.text,layout.labelFontSize);y+=notesHeight+gap;
   Place(footer,pad,y,inner,layout.footerHeight);Place((RectTransform)walkingButton.transform,(inner-layout.readySize.x)*.5f,4,layout.readySize.x,layout.readySize.y);
   reopen.anchorMin=reopen.anchorMax=new Vector2(.5f,1);reopen.pivot=new Vector2(.5f,.5f);reopen.sizeDelta=layout.reopenSize;reopen.anchoredPosition=new Vector2(Mathf.Clamp(layout.reopenPosition.x,-page.rect.width*.5f+layout.margin+layout.reopenSize.x*.5f,page.rect.width*.5f-layout.margin-layout.reopenSize.x*.5f),Mathf.Clamp(layout.reopenPosition.y,-page.rect.height+layout.margin+layout.reopenSize.y*.5f,-layout.margin-layout.reopenSize.y*.5f));LayoutCrew();
  }
  void LateUpdate(){Layout();}
  bool InsidePage(RectTransform rect){var corners=new Vector3[4];rect.GetWorldCorners(corners);var bounds=page.rect;return corners.All(c=>{var p=page.InverseTransformPoint(c);return p.x>=bounds.xMin-1&&p.x<=bounds.xMax+1&&p.y>=bounds.yMin-1&&p.y<=bounds.yMax+1;});}
  public bool FitsSafeFrame()=>page&&(!Visible||InsidePage(card))&&(!reopen.gameObject.activeInHierarchy||InsidePage(reopen))&&(!CrewVisible||InsidePage(crewStrip));
  static bool LabelReadable(Text t)=>t.fontSize>=16&&t.fontStyle==FontStyle.Normal&&t.cachedTextGenerator.vertexCount>0&&!t.GetComponents<Shadow>().Any(s=>s.enabled);
  static bool LabelsReadable(Transform region)=>region.GetComponentsInChildren<Text>().Where(t=>t.isActiveAndEnabled&&!string.IsNullOrEmpty(t.text)).All(LabelReadable);
  public bool Readable()=>(!Visible||LabelsReadable(card))&&(!CrewVisible||LabelsReadable(crewStrip));
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public string ReadabilityProblems(){
   var regions=new[]{Visible?card:null,CrewVisible?crewStrip:null};return string.Join("\n",regions.Where(r=>r).SelectMany(r=>r.GetComponentsInChildren<Text>()).Where(t=>t.isActiveAndEnabled&&!string.IsNullOrEmpty(t.text)&&!LabelReadable(t)).Select(t=>"RESULT_TEXT_FAILURE name="+t.name+" text="+t.text+" font="+(t.font?t.font.name:"none")+" points="+t.fontSize+" rect="+t.rectTransform.rect.size+" line_required="+LineHeight(t,t.fontSize)+" vertices="+t.cachedTextGenerator.vertexCount+" style="+t.fontStyle+" enabled_shadows="+t.GetComponents<Shadow>().Count(s=>s.enabled)));
  }
#endif
  void OnDisable(){HideCard(false);if(crewStrip)crewStrip.gameObject.SetActive(false);}
 }
}
