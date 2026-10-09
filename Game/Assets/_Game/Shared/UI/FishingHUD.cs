using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class FishingHUD:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler {
  public static FishingHUD Instance {get;private set;}
  sealed class Row {
   public ulong id;public RectTransform rect,popRect;public FishingStickerGraphic panel,avatar,flame,crown;
   public Text name,points,rank,pop,arrow;public uint award;public int score,order;public float displayed,pulse,popAge=3,milestoneAge=3,cueAge=3;public bool initialized,milestone;
  }
  static readonly Color Ink=LocalProfile.Hex("143B50"),Cream=LocalProfile.Hex("FFF3D7"),Teal=LocalProfile.Hex("4CDED6"),Gold=LocalProfile.Hex("FFCB53"),Coral=LocalProfile.Hex("FF7659");
  readonly Dictionary<ulong,Row> rows=new();
  RectTransform page,board,drawer,gauges,clockCard,moveControl,lookControl,cameraControl,jumpControl,returnControl;FishingHUDLayout layout;readonly Dictionary<RectTransform,RectTransform> frames=new();Font font,legacyFont;Button action,start,cancel,ready,practice;Button[] modes;
  Text actionLabel,startLabel,readyLabel,clock,wanted,bonus,footer,crew;InputField nickname;RectTransform clockBadge,wantedBadge;FishingStickerGraphic timerPlate,wantedPlate,bonusPlate;FishingStickerGraphic[] clockShines;
  FishingStickerGraphic catchGauge,tensionGauge,reelIcon,targetIcon;FishingStickerGraphic[] flames,sparkles;
  FishingStickerGraphic aim,aimTarget;
  public Vector2 AimScreenPoint=>RectTransformUtility.WorldToScreenPoint(null,aim.rectTransform.position);
  public float AimAcquireDegrees=>layout.aimAcquireDegrees;
  public float AimReleaseDegrees=>Mathf.Max(layout.aimAcquireDegrees,layout.aimReleaseDegrees);
  public bool AimLocked=>aim&&aim.gameObject.activeInHierarchy&&aim.tint==Teal;
  bool holding;int pointer;uint round;FishingMode chosenMode;float shownProgress,shownTension,selectionPulse;uint selectionRevision;
  public string StatusText=>actionLabel?actionLabel.text:"";
  public string WantedText=>wanted?wanted.text:"";
  public string WantedBonusText=>bonus?bonus.text:"";
  public float ShownTension=>shownTension;
  public int ScorePulseCount {get;private set;}
  public bool DangerVisible=>flames!=null&&flames.Any(f=>f.gameObject.activeInHierarchy);
  public bool RowsMoving=>rows.Values.Any(r=>Mathf.Abs(r.rect.anchoredPosition.y-(-87-r.order*58))>.8f);
  public int VisibleScoreCount=>rows.Values.Count(r=>r.rect.gameObject.activeInHierarchy&&r.points.cachedTextGenerator.vertexCount>0);
  public RectTransform ActionRect=>(RectTransform)transform;
  public static void Create(RectTransform page,Font font,RectTransform movement,RectTransform look,RectTransform camera,RectTransform jump,RectTransform back){
   var frame=GolfLeaderboardUI.Rect("Fishing HUD frame",page,Vector2.zero,Vector2.zero,Vector2.zero);frame.anchorMax=Vector2.one;frame.offsetMin=frame.offsetMax=Vector2.zero;
   var node=GolfLeaderboardUI.Rect("Fishing action",frame,Vector2.one,Vector2.zero,new Vector2(235,235));var ui=node.gameObject.AddComponent<FishingHUD>();Instance=ui;ui.page=frame;ui.layout=Resources.Load<FishingHUDLayout>("FishingUILayout");if(!ui.layout)ui.layout=ScriptableObject.CreateInstance<FishingHUDLayout>();
   ui.moveControl=movement;ui.lookControl=look;ui.cameraControl=camera;ui.jumpControl=jump;ui.returnControl=back;foreach(var control in new[]{movement,look,camera,jump,back})control.SetParent(frame,false);look.SetAsFirstSibling();look.anchorMin=Vector2.zero;look.anchorMax=Vector2.one;look.offsetMin=look.offsetMax=Vector2.zero;
   ui.legacyFont=font;ui.font=Resources.Load<Font>("Menu/CoveDisplay")??font;ui.Build();ui.Layout();
  }
  RectTransform Rect(string name,Transform parent,Vector2 pos,Vector2 size,Vector2? anchor=null)=>GolfLeaderboardUI.Rect(name,parent,anchor??new Vector2(.5f,.5f),pos,size);
  FishingStickerGraphic Graphic(RectTransform node,FishingStickerGraphic.Kind kind,Color tint){var g=node.gameObject.AddComponent<FishingStickerGraphic>();g.kind=kind;g.tint=tint;g.raycastTarget=kind==FishingStickerGraphic.Kind.Panel||kind==FishingStickerGraphic.Kind.Gauge;g.Decorate();return g;}
  Text Text(Transform parent,string value,Vector2 pos,Vector2 size,int points,Color? color=null){
   // Both Cove faces contain their intended weight; a second bold style and
   // overlapping shadow/outline copies close the counters in small labels.
   var t=Rect(value,parent,pos,size).gameObject.AddComponent<Text>();t.font=points<=23?CoveUI.TextFont??font:font;t.fontSize=points;t.text=value;t.fontStyle=FontStyle.Normal;t.color=color??Ink;t.alignment=TextAnchor.MiddleCenter;
   t.raycastTarget=false;t.supportRichText=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;
   if(t.color==Color.white)InkOutline(t);return t;
  }
  void InkOutline(Text text){var outline=text.GetComponent<Outline>()??text.gameObject.AddComponent<Outline>();outline.effectColor=Ink;outline.effectDistance=new Vector2(1,-1);}
  Button Button(RectTransform node,string value,Color tint,out Text label){
   var g=Graphic(node,FishingStickerGraphic.Kind.Panel,tint);g.raycastTarget=true;var b=node.gameObject.AddComponent<Button>();b.targetGraphic=g;
   b.transition=Selectable.Transition.None;label=Text(node,value,Vector2.zero,node.sizeDelta-new Vector2(16,6),25);
   var feedback=node.gameObject.AddComponent<CoveFeedback>();return b;
  }
  void Build(){
   aim=Graphic(Rect("Fishing aim reticle",page,Vector2.zero,layout.aimSize,layout.aimAnchor),FishingStickerGraphic.Kind.Aim,Cream);aim.raycastTarget=false;
   aimTarget=Graphic(Rect("Selected fish brackets",page,Vector2.zero,layout.aimSize*.85f),FishingStickerGraphic.Kind.Aim,Teal);aimTarget.value=0;aimTarget.raycastTarget=false;
   action=Button((RectTransform)transform,"AIM AT A FISH",Gold,out actionLabel);actionLabel.rectTransform.anchoredPosition=new Vector2(0,-58);actionLabel.rectTransform.sizeDelta=new Vector2(180,68);actionLabel.fontSize=28;actionLabel.color=Color.white;InkOutline(actionLabel);action.GetComponent<FishingStickerGraphic>().UseIllustration("ReelButton");
   reelIcon=Graphic(Rect("Reel badge",transform,new Vector2(0,29),new Vector2(100,72)),FishingStickerGraphic.Kind.Reel,Teal);reelIcon.gameObject.SetActive(false);
   actionLabel.resizeTextForBestFit=true;actionLabel.resizeTextMinSize=18;actionLabel.resizeTextMaxSize=28;
   cancel=Button(Rect("Fishing cancel",page,Vector2.zero,layout.cancelSize),"CANCEL",Coral,out var cancelText);cancelText.fontSize=23;cancel.onClick.AddListener(()=>PlayerView.Instance?.CancelFishing());
   board=Rect("Lagoon sticker leaderboard",page,new Vector2(20,-20),new Vector2(365,370),new Vector2(0,1));board.pivot=new Vector2(0,1);Graphic(board,FishingStickerGraphic.Kind.Panel,Cream);
   var headerPlate=Rect("Lagoon header sticker",board,new Vector2(0,-31),new Vector2(351,57),new Vector2(.5f,1));Graphic(headerPlate,FishingStickerGraphic.Kind.Panel,Ink);
   var header=Text(board,"LAGOON CUP",new Vector2(13,0),new Vector2(295,45),30,Color.white);header.rectTransform.anchorMin=header.rectTransform.anchorMax=new Vector2(.5f,1);header.rectTransform.anchoredPosition=new Vector2(14,-31);
   Graphic(Rect("Header fish",board,new Vector2(30,-32),new Vector2(49,42),new Vector2(0,1)),FishingStickerGraphic.Kind.Fish,Teal);
   footer=Text(board,"",Vector2.zero,new Vector2(343,60),21);footer.rectTransform.anchorMin=footer.rectTransform.anchorMax=new Vector2(.5f,1);
   drawer=Rect("Fishing match setup",board,Vector2.zero,new Vector2(343,160),new Vector2(.5f,1));
   modes=new Button[3];string[] labels={"ROUND","CUP x3","CREW"};
   for(int i=0;i<3;i++){int mode=i;modes[i]=Button(Rect("Mode "+labels[i],drawer,new Vector2((i-1)*113,57),new Vector2(107,44)),labels[i],i==0?Teal:Cream,out var text);text.fontSize=20;text.font=CoveUI.TextFont??font;modes[i].onClick.AddListener(()=>chosenMode=(FishingMode)mode);}
   ready=Button(Rect("Fishing ready",drawer,new Vector2(-85,5),new Vector2(164,48)),"READY",Teal,out readyLabel);ready.onClick.AddListener(()=>PlayerView.Instance?.ReadyFishing());
   start=Button(Rect("Start fishing round",drawer,new Vector2(85,5),new Vector2(164,48)),"START",Gold,out startLabel);start.onClick.AddListener(()=>FishingGame.Instance?.StartMatch(chosenMode));
   var nr=Rect("Fishing nickname",drawer,new Vector2(-43,-51),new Vector2(250,44));var ng=Graphic(nr,FishingStickerGraphic.Kind.Panel,Cream);ng.raycastTarget=true;
   nickname=nr.gameObject.AddComponent<InputField>();nickname.targetGraphic=ng;nickname.characterLimit=16;nickname.lineType=InputField.LineType.SingleLine;
   var nt=Text(nr,PlayerPrefs.GetString("fishing.nickname",""),Vector2.zero,new Vector2(226,35),20);nickname.textComponent=nt;var placeholder=Text(nr,"YOUR NAME",Vector2.zero,new Vector2(226,35),20,new Color(.18f,.38f,.42f,.85f));nickname.placeholder=placeholder;nickname.text=PlayerPrefs.GetString("fishing.nickname","");
   nickname.onEndEdit.AddListener(value=>{string name=new string(value.Where(c=>char.IsLetterOrDigit(c)||c==' '||c=='_'||c=='-').Take(16).ToArray()).Trim();PlayerPrefs.SetString("fishing.nickname",name);PlayerPrefs.Save();var actor=PlayerView.Instance?PlayerView.Instance.target:null;var net=actor?actor.GetComponent<NetworkAthlete>():null;if(net&&net.IsOwner&&net.IsSpawned)net.NicknameRpc(name);});
   practice=Button(Rect("Return to free fishing",drawer,new Vector2(133,-51),new Vector2(67,44)),"FREE",Cream,out var pt);pt.fontSize=17;pt.font=CoveUI.TextFont??font;practice.onClick.AddListener(()=>FishingGame.Instance?.ReturnToPractice());
   var timeCard=clockCard=Rect("Fishing clock card",page,Vector2.zero,layout.clockSize);timerPlate=Graphic(timeCard,FishingStickerGraphic.Kind.Panel,Ink);
   clockBadge=Rect("Stopwatch sticker",timeCard,new Vector2(-139,39),new Vector2(76,83));Graphic(clockBadge,FishingStickerGraphic.Kind.Fish,Gold).UseIllustration("ClockBadge");
   clock=Text(timeCard,"FREE FISHING",new Vector2(34,39),new Vector2(240,65),44,Color.white);clock.resizeTextForBestFit=true;clock.resizeTextMinSize=22;clock.resizeTextMaxSize=44;
   var wantedCard=Rect("Wanted fish sticker",timeCard,new Vector2(0,-42),new Vector2(374,68));wantedPlate=Graphic(wantedCard,FishingStickerGraphic.Kind.Panel,Cream);
   wantedBadge=Rect("Wanted fish illustration",wantedCard,new Vector2(-146,0),new Vector2(58,48));Graphic(wantedBadge,FishingStickerGraphic.Kind.Fish,Teal);
   wanted=Text(wantedCard,"READY TO START",new Vector2(-17,0),new Vector2(190,57),23);wanted.resizeTextForBestFit=true;wanted.resizeTextMinSize=18;wanted.resizeTextMaxSize=23;
   var reward=Rect("Wanted bonus badge",wantedCard,new Vector2(138,0),new Vector2(60,47));bonusPlate=Graphic(reward,FishingStickerGraphic.Kind.Panel,Gold);bonus=Text(reward,"+3",Vector2.zero,new Vector2(52,40),23);
   clockShines=new FishingStickerGraphic[2];for(int i=0;i<2;i++)clockShines[i]=Graphic(Rect("Timer glint "+i,timeCard,new Vector2(i==0?-172:164,65),new Vector2(15,18)),FishingStickerGraphic.Kind.Shine,Gold);
   crew=Text(timeCard,"",new Vector2(0,-95),new Vector2(360,38),23,Gold);crew.gameObject.SetActive(false);
   gauges=Rect("Fishing catch and tension",page,Vector2.zero,layout.gaugeSize);
   catchGauge=Graphic(Rect("Catch gauge",gauges,new Vector2(0,31),new Vector2(574,55)),FishingStickerGraphic.Kind.Gauge,Teal);
   Text(catchGauge.transform,"CATCH 0%",Vector2.zero,new Vector2(480,40),25,Color.white);
   tensionGauge=Graphic(Rect("Tension gauge",gauges,new Vector2(0,-32),new Vector2(574,61)),FishingStickerGraphic.Kind.Gauge,Gold);
   Text(tensionGauge.transform,"TENSION 0%",Vector2.zero,new Vector2(475,44),26,Color.white);
   Graphic(Rect("Catch fish badge",catchGauge.transform,new Vector2(-260,0),new Vector2(49,40)),FishingStickerGraphic.Kind.Fish,Teal);
   targetIcon=Graphic(Rect("Danger warning",tensionGauge.transform,new Vector2(255,0),new Vector2(38,38)),FishingStickerGraphic.Kind.Warning,Gold);
   flames=new FishingStickerGraphic[4];sparkles=new FishingStickerGraphic[2];
   for(int i=0;i<flames.Length;i++)flames[i]=Graphic(Rect("Line danger flame "+i,gauges,new Vector2(205+i*23,-27),new Vector2(28,45)),FishingStickerGraphic.Kind.Flame,Coral);
   for(int i=0;i<2;i++)sparkles[i]=Graphic(Rect("Catch shine "+i,gauges,new Vector2(i==0?-150:175,31),new Vector2(20,24)),FishingStickerGraphic.Kind.Shine,Color.white);
   targetIcon.rectTransform.SetParent(gauges,false);targetIcon.rectTransform.anchoredPosition=new Vector2(255,-32);targetIcon.transform.SetAsLastSibling();
   gauges.gameObject.SetActive(false);
   // Apply the same sticker outlines to the existing movement/camera/jump controls.
   foreach(var b in page.GetComponentsInChildren<Button>()){
    if(b==action||b==cancel||b==ready||b==start||b==practice||modes.Contains(b))continue;
     var image=b.GetComponent<Image>();if(!image)continue;image.enabled=false;var node=Rect("Sticker button skin",b.transform,Vector2.zero,((RectTransform)b.transform).sizeDelta);node.SetAsFirstSibling();var graphic=Graphic(node,FishingStickerGraphic.Kind.Panel,Cream);graphic.raycastTarget=true;b.targetGraphic=graphic;b.transition=Selectable.Transition.None;foreach(var label in b.GetComponentsInChildren<Text>()){label.font=font;label.fontStyle=FontStyle.Normal;label.color=Ink;foreach(var shadow in label.GetComponents<Shadow>())shadow.enabled=false;}
   }
  }
  Row MakeRow(ulong id,int order){
   var rect=Rect("Catch row "+id,board,new Vector2(182,-87-order*58),new Vector2(341,52),new Vector2(0,1));
   var row=new Row{id=id,rect=rect,order=order};row.panel=Graphic(rect,FishingStickerGraphic.Kind.Panel,LocalProfile.Hex("A7EFF0"));
   row.rank=Text(rect,"",new Vector2(-148,0),new Vector2(33,42),29);row.rank.font=legacyFont;
   row.avatar=Graphic(Rect("Fisher avatar",rect,new Vector2(-109,0),new Vector2(49,49)),FishingStickerGraphic.Kind.Avatar,LocalProfile.Teams[(int)(id%4)]);row.avatar.accent=Gold;
   row.name=Text(rect,"",new Vector2(-29,0),new Vector2(95,35),24);row.name.font=CoveUI.TextFont??font;row.name.resizeTextForBestFit=true;row.name.resizeTextMinSize=18;row.name.resizeTextMaxSize=24;
   row.points=Text(rect,"0",new Vector2(119,0),new Vector2(64,52),32,Color.white);
   row.popRect=Rect("Catch reward pop",rect,new Vector2(60,6),new Vector2(60,37));Graphic(row.popRect,FishingStickerGraphic.Kind.Panel,Coral);row.pop=Text(row.popRect,"",Vector2.zero,new Vector2(56,33),23,Color.white);
   row.arrow=Text(rect,"",new Vector2(152,0),new Vector2(22,35),21);row.arrow.font=legacyFont;
   row.flame=Graphic(Rect("Twenty point flame",rect,new Vector2(160,11),new Vector2(29,49)),FishingStickerGraphic.Kind.Flame,Coral);row.flame.transform.SetAsFirstSibling();
   row.crown=Graphic(Rect("Winner crown",rect,new Vector2(-151,24),new Vector2(30,29)),FishingStickerGraphic.Kind.Crown,Gold);
   rows[id]=row;return row;
  }
  void UpdateRows(FishingGame game,PlayerView view){
   var match=game.State.Match;bool changed=round!=match.round;if(changed){round=match.round;shownProgress=shownTension=0;}
   bool team=match.mode==FishingMode.Crew;var ranked=team?game.State.Players.OrderBy(p=>p.owner).ToArray():game.State.Players.OrderByDescending(p=>match.cupFinished?p.cupPoints:p.score).ThenByDescending(p=>match.cupFinished?p.cupScore:0).ThenBy(p=>p.owner).ToArray();
   for(int i=0;i<ranked.Length;i++){
    var p=ranked[i];if(!rows.TryGetValue(p.owner,out var row))row=MakeRow(p.owner,i);row.rect.gameObject.SetActive(true);
    if(changed||!row.initialized){row.displayed=p.score;row.award=p.awardSequence;row.score=p.score;row.popAge=row.milestoneAge=row.cueAge=3;row.pulse=0;row.initialized=true;row.rect.anchoredPosition=new Vector2(182,-87-i*58);row.milestone=p.score>=20;}
    if(row.award!=p.awardSequence){row.award=p.awardSequence;row.pulse=1;row.popAge=0;ScorePulseCount++;if(p.score>=20&&!row.milestone){row.milestone=true;row.milestoneAge=0;}}
    row.cueAge+=Time.unscaledDeltaTime;row.milestoneAge+=Time.unscaledDeltaTime;
    if(row.order!=i){row.arrow.text=i<row.order?"▲":"▼";row.arrow.color=i<row.order?LocalProfile.Hex("329F56"):Coral;row.cueAge=0;row.pulse=Mathf.Max(row.pulse,.7f);}else if(row.cueAge>1.2f)row.arrow.text="";row.order=i;
    if(match.phase==FishingRoundPhase.Practice||match.phase==FishingRoundPhase.Ended){row.arrow.text=p.ready?"✓":"";row.arrow.color=Ink;}
    var pos=row.rect.anchoredPosition;float destination=-87-i*58;pos.y=Mathf.Lerp(pos.y,destination,1-Mathf.Exp(-12*Time.unscaledDeltaTime));row.rect.anchoredPosition=pos;
    row.pulse=Mathf.MoveTowards(row.pulse,0,Time.unscaledDeltaTime*1.7f);row.rect.localScale=Vector3.one*(1+.055f*Mathf.Sin(row.pulse*Mathf.PI));row.popAge+=Time.unscaledDeltaTime;row.popRect.gameObject.SetActive(row.popAge<1.4f);row.popRect.anchoredPosition=new Vector2(54,4+row.popAge*11);row.popRect.localScale=Vector3.one*(1+.15f*Mathf.Exp(-row.popAge*5));
    row.pop.text="+"+(p.lastBonus>0&&row.popAge>.65f?p.lastBonus:p.lastAward-p.lastBonus);var popGroup=row.popRect.GetComponent<CanvasGroup>()??row.popRect.gameObject.AddComponent<CanvasGroup>();popGroup.alpha=1-Mathf.InverseLerp(1,1.4f,row.popAge);
    row.displayed=Mathf.MoveTowards(row.displayed,p.score,Time.unscaledDeltaTime*Mathf.Max(12,Mathf.Abs(p.score-row.displayed)*10));row.points.text=match.cupFinished?p.cupPoints.ToString():Mathf.RoundToInt(row.displayed).ToString();
    bool local=p.owner==FishingGame.Key(view.target);string name=game.PlayerName(p.owner);if(local&&(name=="HOST"||name.StartsWith("FRIEND")))name="YOU";row.name.text=name.Length>10?name.Substring(0,9)+"…":name;
    row.rank.text=team?(p.catches>0?"✓":"•"):(match.cupFinished?game.State.CupRank(p.owner):game.State.Rank(p.owner)).ToString();
    bool heat=p.score>=18&&p.score<20||row.milestoneAge<1.4f;row.flame.gameObject.SetActive(heat);row.flame.rectTransform.localScale=Vector3.one*(1+.12f*Mathf.Sin(Time.unscaledTime*9));
    row.crown.gameObject.SetActive(!team&&row.rank.text=="1");row.panel.Refresh(1,heat?Gold:local?LocalProfile.Hex("9DECD4"):LocalProfile.Hex("C0EFF1"));var cg=row.rect.GetComponent<CanvasGroup>()??row.rect.gameObject.AddComponent<CanvasGroup>();cg.alpha=p.connected?1:.45f;row.score=p.score;
   }
   foreach(var pair in rows)if(!ranked.Any(p=>p.owner==pair.Key))pair.Value.rect.gameObject.SetActive(false);
   int count=Math.Max(1,ranked.Length);bool setup=match.phase==FishingRoundPhase.Practice||match.phase==FishingRoundPhase.Ended;
   float footerY=-62-count*58;footer.rectTransform.anchoredPosition=new Vector2(0,footerY-16);drawer.anchoredPosition=new Vector2(0,footerY-117);drawer.gameObject.SetActive(setup);
   board.sizeDelta=new Vector2(365,62+count*58+50+(setup?162:0));
   if(match.phase==FishingRoundPhase.Ended){
    if(match.outcome==FishingOutcome.Abandoned)footer.text="ROUND ABANDONED · READY AGAIN";
    else if(team)footer.text=match.outcome==FishingOutcome.Success?new[]{"","BRONZE CREW!","SILVER CREW!","GOLD CREW!"}[match.medal]:"TRY AGAIN · BASKET INCOMPLETE";
    else if(match.cupFinished)footer.text=game.State.CupRank(FishingGame.Key(view.target))==1?"CUP CHAMPION!":"CUP COMPLETE · PLAY AGAIN";
    else footer.text=game.State.Rank(FishingGame.Key(view.target))==1?"YOU WIN! · READY TO REMATCH":"ROUND OVER · READY TO REMATCH";
   }else footer.text=team?"SHARED BASKET · EVERYONE CATCHES":match.mode==FishingMode.Cup?"CUP "+match.cupRound+"/3 · "+(game.Player(view.target)?.cupPoints??0)+" CUP PTS":game.ParticipantCount==1?(match.phase==FishingRoundPhase.Practice?"SOLO PRACTICE":"SOLO ROUND") :"CATCH POINTS · 2 / 5 / 10";
  }
  void Update(){
   var game=FishingGame.Instance;var view=PlayerView.Instance;if(!game||!game.Context||!view)return;var p=game.Player(view.target);var match=game.State.Match;if(!game.Authority)chosenMode=match.mode;UpdateRows(game,view);
   bool setup=match.phase==FishingRoundPhase.Practice||match.phase==FishingRoundPhase.Ended;bool countdown=match.phase==FishingRoundPhase.Countdown;
   start.gameObject.SetActive(game.Authority);start.interactable=game.CanStart(chosenMode);startLabel.text=chosenMode==FishingMode.Cup&&match.mode==FishingMode.Cup&&match.cupValid&&!match.cupFinished?"NEXT ROUND":"START";
   ready.interactable=setup&&p.HasValue&&!p.Value.Busy&&PlayerView.Instance&&PlayerView.Instance.active;readyLabel.text=p.HasValue&&p.Value.ready?"READY!":"READY";
   nickname.interactable=setup;practice.gameObject.SetActive(game.Authority&&match.phase==FishingRoundPhase.Ended);
   for(int i=0;i<modes.Length;i++){modes[i].interactable=game.Authority&&setup;var g=modes[i].GetComponent<FishingStickerGraphic>();g.Refresh(1,i==(int)chosenMode?Teal:Cream);}
   bool running=game.State.Running;var phase=p.HasValue?p.Value.phase:FishingPhase.Ready;bool fighting=p.HasValue&&phase==FishingPhase.Reeling;
   action.interactable=view.active&&running&&p.HasValue&&(phase==FishingPhase.Ready&&!view.FishingCastPending&&game.CanCast(view.target,view.SelectedFishingFish)&&game.VisibleTarget(view.target,view.FishingCamera,view.SelectedFishingFish)||phase==FishingPhase.Bite||fighting);
   cancel.interactable=p.HasValue&&p.Value.Busy;
   if(holding&&(!running||!fighting||!view.FishingHeld)){view.EndFishing(pointer);holding=false;}
   if(!p.HasValue)actionLabel.text=game.ParticipantCount>5?"MAX 5 PLAYERS":"NEXT ROUND";
   else if(countdown)actionLabel.text="GET READY!";
   else if(!running)actionLabel.text="ROUND OVER";
   else if(phase==FishingPhase.Bite)actionLabel.text="HOOK!";
   else if(fighting)actionLabel.text=p.Value.held&&p.Value.tension>.72f?"RELEASE!":"REEL";
   else if(view.FishingCastPending)actionLabel.text="CASTING…";
   else if(phase==FishingPhase.Casting)actionLabel.text="CASTING…";
   else if(phase==FishingPhase.Waiting)actionLabel.text="WATCH FLOAT";
   else if(phase==FishingPhase.Caught)actionLabel.text="LANDED! +"+p.Value.lastAward;
   else if(phase==FishingPhase.Escaped)actionLabel.text=p.Value.result==FishingResult.Snapped?"LINE SNAPPED":p.Value.result==FishingResult.Missed?"MISSED BITE":"TRY AGAIN";
   else if(game.NearestPier(view.target)<0)actionLabel.text="FIND A PIER";
   else if(view.SelectedFishingFish<0)actionLabel.text="AIM AT\nA FISH";
   else actionLabel.text=game.Available(view.target,view.SelectedFishingFish)?"CAST\n"+FishingState.SizeName(view.SelectedFishingFish)+" +"+FishingState.Points(view.SelectedFishingFish):"FISH RESTING";
   var actionGraphic=action.GetComponent<FishingStickerGraphic>();actionGraphic.Refresh(1,phase==FishingPhase.Bite?Teal:fighting&&p.Value.tension>.72f?Coral:Gold);
   var group=action.GetComponent<CanvasGroup>()??action.gameObject.AddComponent<CanvasGroup>();group.alpha=action.interactable?1:.76f;
   if(view.SelectionRevision!=selectionRevision){selectionRevision=view.SelectionRevision;selectionPulse=1;}selectionPulse=Mathf.MoveTowards(selectionPulse,0,Time.unscaledDeltaTime*2);
   reelIcon.rectTransform.localScale=Vector3.one*(1+.07f*Mathf.Sin(Time.unscaledTime*(phase==FishingPhase.Bite?16:3))+selectionPulse*.1f);
   actionGraphic.rectTransform.localRotation=Quaternion.Euler(0,0,fighting&&p.Value.held?Mathf.Sin(Time.unscaledTime*10)*1.5f:0);
   double remaining=Math.Max(0,match.deadline-game.Now);bool live=match.phase==FishingRoundPhase.Playing,urgent=live&&remaining<10;
   clock.text=countdown?Math.Max(1,Math.Ceiling(match.started-game.Now)).ToString():live?GolfLeaderboardUI.FinishTime(Math.Ceiling(remaining)):match.phase==FishingRoundPhase.Ended?"RESULTS":"FREE FISHING";
   int window=game.State.Window(game.Now),wantedSize=game.State.Wanted(game.Now);bool claimed=p.HasValue&&(p.Value.wantedMask&(1<<window))!=0,next=live&&window<2&&60-(game.Now-match.started)%60<8;
   wanted.text=live||countdown?(next?"NEXT UP\n":"WANTED\n")+FishingState.SizeName(next?(wantedSize+1)%3:wantedSize):game.State.AllReady?"EVERYONE\nREADY!":match.phase==FishingRoundPhase.Ended?"READY TO\nREMATCH":"READY TO\nSTART";
   bonus.text=live||countdown?claimed&&!next?"OK":"+3":"GO";bonusPlate.Refresh(1,claimed&&!next?Teal:Gold);wantedPlate.Refresh(1,next?LocalProfile.Hex("FFF0BC"):Cream);timerPlate.Refresh(1,urgent?LocalProfile.Hex("703F40"):Ink);
   float beat=Mathf.Exp(-Mathf.Repeat(Time.unscaledTime,1)*7);clockBadge.localScale=Vector3.one*(1+(countdown?.08f:urgent?.065f:.018f)*beat);clockBadge.localRotation=Quaternion.Euler(0,0,urgent?Mathf.Sin(Time.unscaledTime*12)*4:0);wantedBadge.localScale=Vector3.one*((.86f+wantedSize*.07f)+(next?.04f*Mathf.Sin(Time.unscaledTime*10):0));
   for(int i=0;i<clockShines.Length;i++)clockShines[i].rectTransform.localScale=Vector3.one*(.7f+.25f*Mathf.Sin(Time.unscaledTime*3+i));clock.color=urgent?LocalProfile.Hex("FFD65C"):Color.white;
   bool team=match.mode==FishingMode.Crew&&!setup;crew.gameObject.SetActive(team);if(team)crew.text="CREW "+match.crewScore+" / "+match.crewTarget+" · "+((match.crewMask&1)!=0?"S+":"S-")+" "+((match.crewMask&2)!=0?"M+":"M-")+" "+((match.crewMask&4)!=0?"L+":"L-");clockCard.sizeDelta=layout.clockSize+new Vector2(0,team?56:0);float topShift=team?28:0;clockBadge.anchoredPosition=new Vector2(-139,39+topShift);clock.rectTransform.anchoredPosition=new Vector2(34,39+topShift);((RectTransform)wanted.transform.parent).anchoredPosition=new Vector2(0,-42+topShift);crew.rectTransform.anchoredPosition=new Vector2(0,-85);
   gauges.gameObject.SetActive(fighting);
   if(fighting){
    float dt=Time.unscaledDeltaTime;shownProgress=Mathf.Lerp(shownProgress,p.Value.progress,1-Mathf.Exp(-12*dt));shownTension=Mathf.Lerp(shownTension,p.Value.tension,1-Mathf.Exp(-25*dt));
    catchGauge.Refresh(shownProgress,Teal);tensionGauge.Refresh(shownTension,Color.Lerp(Teal,p.Value.tension<.65f?Gold:Coral,Mathf.InverseLerp(.2f,p.Value.tension<.65f?.65f:.9f,p.Value.tension)),p.Value.tension>.72f);
    catchGauge.GetComponentInChildren<Text>().text="CATCH "+Mathf.RoundToInt(p.Value.progress*100)+"%";tensionGauge.GetComponentInChildren<Text>().text="TENSION "+Mathf.RoundToInt(p.Value.tension*100)+"%";targetIcon.gameObject.SetActive(p.Value.tension>.72f);
    for(int i=0;i<flames.Length;i++){bool show=p.Value.tension>.72f;flames[i].gameObject.SetActive(show);if(show){flames[i].rectTransform.anchoredPosition=new Vector2(203+i*24,-32+Mathf.Sin(Time.unscaledTime*10+i*1.9f)*7);flames[i].rectTransform.localScale=Vector3.one*(.75f+.22f*Mathf.Sin(Time.unscaledTime*12+i));}}
    for(int i=0;i<sparkles.Length;i++){sparkles[i].rectTransform.anchoredPosition=new Vector2(Mathf.Lerp(-235,230,Mathf.Repeat(Time.unscaledTime*.33f+i*.5f,1)),31);sparkles[i].rectTransform.localScale=Vector3.one*(.7f+.25f*Mathf.Sin(Time.unscaledTime*6+i));}
   }
  }
  void LateUpdate(){if(page){Layout();UpdateAim();}}
  void UpdateAim(){
   var game=FishingGame.Instance;var view=PlayerView.Instance;var p=game&&view?game.Player(view.target):null;
   bool visible=game&&game.Context&&view&&view.active&&game.State.Running&&p.HasValue&&p.Value.phase==FishingPhase.Ready&&game.NearestPier(view.target)>=0;
   aim.gameObject.SetActive(visible);int fish=view?view.SelectedFishingFish:-1;bool locked=visible&&game.VisibleTarget(view.target,view.FishingCamera,fish);
   aim.Refresh(1,locked?Teal:Cream);aimTarget.gameObject.SetActive(locked);
   if(locked){var screen=view.FishingCamera.WorldToScreenPoint(game.Presentation.TargetPosition(fish));RectTransformUtility.ScreenPointToLocalPointInRectangle(page,screen,null,out var point);aimTarget.rectTransform.anchoredPosition=point;}
  }
  void PlaceFrame(RectTransform widget,Vector2 anchor,Vector2 pivot,Vector2 position,float scale){
   if(!frames.TryGetValue(widget,out var frame)){frame=Rect("Layout "+widget.name,page,Vector2.zero,widget.sizeDelta);frames[widget]=frame;widget.SetParent(frame,false);widget.anchorMin=widget.anchorMax=widget.pivot=new Vector2(.5f,.5f);widget.anchoredPosition=Vector2.zero;}
   frame.anchorMin=frame.anchorMax=anchor;frame.pivot=pivot;frame.anchoredPosition=position;frame.sizeDelta=widget.sizeDelta;frame.localScale=Vector3.one*scale;
  }
  void Layout(){
   float width=page.rect.width,height=page.rect.height;if(width<=0||height<=0)return;float m=layout.margin,gap=layout.gap;bool compact=width/height<layout.compactAspect;
   aim.rectTransform.anchorMin=aim.rectTransform.anchorMax=layout.aimAnchor;aim.rectTransform.sizeDelta=layout.aimSize;
   float scale=Mathf.Min(width/height<1?layout.portraitScale:1,(width-2*m)/(board.sizeDelta.x+layout.clockSize.x+gap));scale=Mathf.Clamp(scale,.1f,2);
   ((RectTransform)transform).sizeDelta=layout.actionSize;cameraControl.sizeDelta=layout.cameraSize;jumpControl.sizeDelta=layout.jumpSize;returnControl.sizeDelta=layout.returnSize;moveControl.sizeDelta=layout.joystickSize;
   PlaceFrame(board,new Vector2(0,1),new Vector2(0,1),new Vector2(m,-m),scale);
   PlaceFrame(returnControl,Vector2.one,Vector2.one,new Vector2(-m,-m),scale);
   PlaceFrame(clockCard,compact?Vector2.one:new Vector2(.5f,1),compact?Vector2.one:new Vector2(.5f,1),new Vector2(compact?-m:0,-m-(compact?(layout.returnSize.y+gap)*scale:0)),scale);
   PlaceFrame((RectTransform)transform,new Vector2(1,0),new Vector2(1,0),new Vector2(-m,m),scale);
   PlaceFrame((RectTransform)cancel.transform,new Vector2(1,0),new Vector2(1,0),new Vector2(-m-(layout.actionSize.x+gap)*scale,m),scale);
   PlaceFrame(jumpControl,new Vector2(1,0),new Vector2(.5f,0),new Vector2(-m-layout.actionSize.x*scale*.5f,m+(layout.actionSize.y+gap)*scale),scale);
   PlaceFrame(cameraControl,Vector2.zero,Vector2.zero,new Vector2(m,m),scale);
   PlaceFrame(moveControl,Vector2.zero,new Vector2(.5f,0),new Vector2(m+layout.joystickSize.x*scale*.5f,m+(layout.cameraSize.y+gap)*scale),scale);
   float bottom=compact?m+(layout.actionSize.y+gap+layout.jumpSize.y+gap)*scale:m;
   PlaceFrame(gauges,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,bottom),scale);
  }
  public bool FitsSafeFrame(){
   var corners=new Vector3[4];var bounds=page.rect;foreach(var pair in frames){if(!pair.Key.gameObject.activeInHierarchy)continue;pair.Value.GetWorldCorners(corners);foreach(var c in corners){var p=page.InverseTransformPoint(c);if(p.x<bounds.xMin-1||p.x>bounds.xMax+1||p.y<bounds.yMin-1||p.y>bounds.yMax+1)return false;}}return true;
  }
  Rect FrameBounds(RectTransform widget){var c=new Vector3[4];frames[widget].GetWorldCorners(c);var min=page.InverseTransformPoint(c[0]);var max=page.InverseTransformPoint(c[2]);return UnityEngine.Rect.MinMaxRect(min.x,min.y,max.x,max.y);}
  public bool RegionsSeparate(){
   RectTransform[][] regions={new[]{board,clockCard,returnControl},new[]{(RectTransform)transform,(RectTransform)cancel.transform,jumpControl,cameraControl,moveControl,gauges}};
   foreach(var group in regions)for(int i=0;i<group.Length;i++)for(int j=i+1;j<group.Length;j++){if(!group[i].gameObject.activeInHierarchy||!group[j].gameObject.activeInHierarchy)continue;if(FrameBounds(group[i]).Overlaps(FrameBounds(group[j])))return false;}return true;
  }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public void ReviewFrameInsets(Vector2 min,Vector2 max){page.offsetMin=min;page.offsetMax=-max;}
  public bool TypographyClean()=>page.GetComponentsInChildren<Text>(true).All(t=>t.fontStyle==FontStyle.Normal&&t.GetComponents<Shadow>().All(s=>!s.enabled||s is Outline&&Mathf.Abs(s.effectDistance.x)<=1&&Mathf.Abs(s.effectDistance.y)<=1));
#endif
  public void OnPointerDown(PointerEventData e){if(holding||e.button!=PointerEventData.InputButton.Left||!action.interactable)return;pointer=e.pointerId;holding=PlayerView.Instance&&PlayerView.Instance.BeginFishing(pointer);}
  public void OnPointerUp(PointerEventData e){if(!holding||e.pointerId!=pointer)return;PlayerView.Instance?.EndFishing(pointer);holding=false;}
  public void OnPointerExit(PointerEventData e){if(holding&&e.pointerId==pointer){PlayerView.Instance?.EndFishing(pointer);holding=false;}}
  void OnDisable(){if(holding)PlayerView.Instance?.EndFishing(pointer);holding=false;}
  void OnDestroy(){if(Instance==this)Instance=null;if(board)Destroy(board.gameObject);if(gauges)Destroy(gauges.gameObject);}
 }
}
