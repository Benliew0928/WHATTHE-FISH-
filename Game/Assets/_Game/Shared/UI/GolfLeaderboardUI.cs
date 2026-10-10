using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Read-only presentation. Rules, hole detection and ranking live in gameplay.
 public sealed class GolfLeaderboardUI:MonoBehaviour {
  Text title,footer,timer,notice;RectTransform page,board,columnBand,countdownCard,noticeCard,flag;Text[][] rows;Text[] headers;
  RectTransform[] rowRects,dividers;FishingStickerGraphic[] rowPanels,avatars;GolfHUDLayout layout;Font fallbackFont;
  readonly float[] columnShares={.332f,.14f,.14f,.176f,.212f};
  static readonly string[] ColumnNames={"Player","Progress","Strokes","Status","Finish time"};
  int visibleRows;float renderedRowHeight;
  public RectTransform BoardRect=>board;
  public RectTransform CountdownRect=>countdownCard;
  public RectTransform NoticeRect=>noticeCard;
  public bool CountdownVisible=>timer&&timer.gameObject.activeInHierarchy;
  public string CountdownText=>timer?timer.text:"";
  public static void Create(RectTransform page,Font font){var ui=Rect("Golf HUD",page,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero).gameObject.AddComponent<GolfLeaderboardUI>();ui.Build(page,font);page.gameObject.AddComponent<GolfTargetIndicator>().Build(page,font);}
  public static RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=position;r.sizeDelta=size;return r;}
  // Kept for existing world-marker callers. The scorecard uses native Cove lettering.
  public static Text Text(string name,Transform parent,Font font,Vector2 anchor,Vector2 position,Vector2 size,int points){var r=Rect(name,parent,anchor,position,size);var t=r.gameObject.AddComponent<Text>();t.font=font;t.fontSize=points;t.color=Color.white;t.alignment=TextAnchor.MiddleLeft;t.raycastTarget=false;t.supportRichText=false;var shadow=t.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.65f);shadow.effectDistance=new Vector2(1,-1);return t;}
  Text Label(string name,Transform parent,int points,TextAnchor alignment=TextAnchor.MiddleCenter,Color? color=null){
   var label=GolfHUDStyle.Text(name,parent,"",Vector2.zero,points,alignment,color);if(!label.font)label.font=fallbackFont;
   label.fontStyle=FontStyle.Normal;label.horizontalOverflow=HorizontalWrapMode.Overflow;label.verticalOverflow=VerticalWrapMode.Truncate;return label;
  }
  static void Place(RectTransform rect,Vector2 anchor,Vector2 position,Vector2 size){rect.anchorMin=rect.anchorMax=anchor;rect.anchoredPosition=position;rect.sizeDelta=size;}
  void Build(RectTransform parent,Font font){
   page=parent;fallbackFont=font;layout=Resources.Load<GolfHUDLayout>("GolfUILayout");if(!layout)layout=ScriptableObject.CreateInstance<GolfHUDLayout>();
   board=Rect("Golf leaderboard",page,Vector2.one,Vector2.zero,Vector2.zero);board.pivot=Vector2.one;GolfHUDStyle.Panel(board,GolfHUDStyle.Cream).raycastTarget=false;
   flag=Rect("Golf leaderboard flag",board,new Vector2(0,1),Vector2.zero,new Vector2(35,35));var icon=flag.gameObject.AddComponent<CoveIcon>();icon.symbol=CoveSymbol.Flag;icon.color=GolfHUDStyle.Ink;icon.raycastTarget=false;
   title=Label("Leaderboard title",board,30);title.text="LEADERBOARD";
   columnBand=Rect("Golf column band",board,new Vector2(0,1),Vector2.zero,Vector2.zero);var band=GolfHUDStyle.Panel(columnBand,LocalProfile.Hex("D2F2E5"),true);band.raycastTarget=false;band.cornerRadius=13;
   var names=ColumnNames;headers=new Text[names.Length];
   for(int j=0;j<names.Length;j++){headers[j]=Label("Column "+names[j],board,layout.tableHeaderFontSize);headers[j].text=names[j];}
   rows=new Text[10][];rowRects=new RectTransform[10];rowPanels=new FishingStickerGraphic[10];avatars=new FishingStickerGraphic[10];
   for(int i=0;i<rows.Length;i++){
    rowRects[i]=Rect("Golf score row "+i,board,new Vector2(0,1),Vector2.zero,Vector2.zero);rowPanels[i]=GolfHUDStyle.Panel(rowRects[i],LocalProfile.Hex("DFF5F0"),true);rowPanels[i].raycastTarget=false;rowPanels[i].cornerRadius=20;
    var avatarRect=Rect("Golf player avatar "+i,rowRects[i],new Vector2(0,.5f),Vector2.zero,Vector2.one*36);avatars[i]=avatarRect.gameObject.AddComponent<FishingStickerGraphic>();avatars[i].kind=FishingStickerGraphic.Kind.Avatar;avatars[i].tint=GolfHUDStyle.Teal;avatars[i].accent=GolfHUDStyle.Gold;avatars[i].raycastTarget=false;avatars[i].Decorate();
    rows[i]=new Text[5];for(int j=0;j<5;j++){rows[i][j]=Label("Player "+i+" "+names[j],rowRects[i],j==0?layout.tableNameFontSize:layout.tableFontSize,j==0?TextAnchor.MiddleLeft:TextAnchor.MiddleCenter);if(j==0||j==2)rows[i][j].font=CoveUI.DisplayFont??rows[i][j].font;}rowRects[i].gameObject.SetActive(false);
   }
   dividers=new RectTransform[4];for(int i=0;i<dividers.Length;i++){dividers[i]=Rect("Golf column divider "+i,board,new Vector2(0,1),Vector2.zero,Vector2.zero);var line=dividers[i].gameObject.AddComponent<Image>();line.color=new Color(GolfHUDStyle.Ink.r,GolfHUDStyle.Ink.g,GolfHUDStyle.Ink.b,.31f);line.raycastTarget=false;}
   footer=Label("Your progress",board,18);footer.horizontalOverflow=HorizontalWrapMode.Wrap;
   countdownCard=Rect("Golf final countdown card",page,new Vector2(.5f,1),layout.countdownPosition,layout.countdownSize);GolfHUDStyle.Panel(countdownCard,GolfHUDStyle.Ink).raycastTarget=false;
   GolfHUDStyle.Art(countdownCard,"Golf countdown clock","FishingUI/ClockBadge",new Vector2(66,72),new Vector2(-layout.countdownSize.x*.31f,0));
   timer=Label("Final 30 seconds",countdownCard,layout.countdownFontSize,TextAnchor.MiddleCenter,Color.white);Place(timer.rectTransform,new Vector2(.5f,.5f),new Vector2(32,0),new Vector2(layout.countdownSize.x-110,layout.countdownSize.y-12));
   noticeCard=Rect("Golf final countdown notice",page,new Vector2(.5f,1),layout.countdownPosition-new Vector2(0,layout.countdownSize.y*.5f+24),new Vector2(330,44));GolfHUDStyle.Panel(noticeCard,GolfHUDStyle.Cream).raycastTarget=false;
   notice=Label("First finisher notice",noticeCard,20);Place(notice.rectTransform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(302,34));
   timer.gameObject.SetActive(false);notice.gameObject.SetActive(false);countdownCard.gameObject.SetActive(false);noticeCard.gameObject.SetActive(false);Layout(1);
  }
  void Layout(int count){
   float padding=layout.boardPadding,margin=layout.margin;
   // Leave the middle clear for the cup arrow/chip on narrower safe areas.
   float width=Mathf.Min(layout.boardWidth,Mathf.Max(480,page.rect.width*.5f-margin-200));
   bool compact=count>6||page.rect.width/Mathf.Max(1,page.rect.height)<layout.compactAspect;
   renderedRowHeight=compact?layout.boardCompactRowHeight:layout.boardRowHeight;
   float rowTop=padding+layout.boardHeaderHeight+layout.boardColumnHeight;
   float height=padding*2+layout.boardHeaderHeight+layout.boardColumnHeight+count*renderedRowHeight+layout.boardFooterHeight;
   board.anchoredPosition=layout.boardPosition;board.sizeDelta=new Vector2(width,height);
   float inner=width-padding*2;
   Place(flag,new Vector2(0,1),new Vector2(padding+23,-padding-layout.boardHeaderHeight*.5f),new Vector2(35,35));
   Place(title.rectTransform,new Vector2(0,1),new Vector2(width*.5f+15,-padding-layout.boardHeaderHeight*.5f),new Vector2(inner-66,layout.boardHeaderHeight-6));title.fontSize=compact?27:30;
   Place(columnBand,new Vector2(0,1),new Vector2(width*.5f,-padding-layout.boardHeaderHeight-layout.boardColumnHeight*.5f),new Vector2(inner,layout.boardColumnHeight-4));
   float offset=0;for(int j=0;j<5;j++){
    float cell=inner*columnShares[j];Place(headers[j].rectTransform,new Vector2(0,1),new Vector2(padding+offset+cell*.5f,-padding-layout.boardHeaderHeight-layout.boardColumnHeight*.5f),new Vector2(cell-6,layout.boardColumnHeight-4));FitLine(headers[j],ColumnNames[j],layout.tableHeaderFontSize,15);offset+=cell;
   }
   for(int i=0;i<rowRects.Length;i++){
    Place(rowRects[i],new Vector2(0,1),new Vector2(width*.5f,-rowTop-renderedRowHeight*(i+.5f)),new Vector2(inner,renderedRowHeight-4));rowPanels[i].cornerRadius=Mathf.Min(20,(renderedRowHeight-4)*.5f);
    float avatarSize=Mathf.Min(36,renderedRowHeight-8);Place(avatars[i].rectTransform,new Vector2(0,.5f),new Vector2(avatarSize*.5f+7,0),Vector2.one*avatarSize);
    offset=0;for(int j=0;j<5;j++){
     float cell=inner*columnShares[j],left=j==0?avatarSize+15:4;Place(rows[i][j].rectTransform,new Vector2(0,.5f),new Vector2(offset+left+(cell-left-5)*.5f,0),new Vector2(cell-left-5,renderedRowHeight-5));rows[i][j].fontSize=compact?18:j==0?layout.tableNameFontSize:layout.tableFontSize;offset+=cell;
    }
   }
   offset=0;for(int i=0;i<dividers.Length;i++){offset+=inner*columnShares[i];float lineHeight=layout.boardColumnHeight+count*renderedRowHeight-6;Place(dividers[i],new Vector2(0,1),new Vector2(padding+offset,-padding-layout.boardHeaderHeight-lineHeight*.5f-3),new Vector2(layout.rowSeparatorWidth,lineHeight));}
   Place(footer.rectTransform,new Vector2(0,1),new Vector2(width*.5f,-rowTop-count*renderedRowHeight-layout.boardFooterHeight*.5f),new Vector2(inner-10,layout.boardFooterHeight-6));
   countdownCard.anchoredPosition=layout.countdownPosition;countdownCard.sizeDelta=layout.countdownSize;noticeCard.anchoredPosition=layout.countdownPosition-new Vector2(0,layout.countdownSize.y*.5f+24);
  }
  static void FitLine(Text label,string value,int points,int minimum){
   label.text=value;var settings=label.GetGenerationSettings(label.rectTransform.rect.size);settings.fontSize=points;settings.resizeTextForBestFit=false;
   float available=label.rectTransform.rect.width-2,preferred=label.cachedTextGeneratorForLayout.GetPreferredWidth(value,settings)/label.pixelsPerUnit;
   label.fontSize=Mathf.Clamp(Mathf.FloorToInt(points*available/Mathf.Max(available,preferred)),minimum,points);
   // Preserve status columns when a player has a long display name.
   if(preferred*label.fontSize/points>available&&value.Length>1){string shortened=value;settings.fontSize=label.fontSize;while(shortened.Length>1&&label.cachedTextGeneratorForLayout.GetPreferredWidth(shortened+"…",settings)/label.pixelsPerUnit>available)shortened=shortened.Substring(0,shortened.Length-1);label.text=shortened+"…";}
  }
  public static string FinishTime(double seconds){var total=(long)Math.Max(0,Math.Floor(seconds));return $"{total/60:00}:{total%60:00}";}
  void Update(){
   var match=GolfMatchManager.Instance;if(!match||!match.Context)return;
   var state=match.State;var ranked=state.Results();var local=match.Player(AppRoot.Instance.LocalAthlete);visibleRows=Math.Min(rows.Length,ranked.Length);Layout(Math.Max(1,visibleRows));
   title.text=state.Phase==GolfMatchPhase.Ended?"LEADERBOARD  •  RESULTS":"LEADERBOARD";FitLine(title,title.text,title.fontSize,24);
   for(int i=0;i<rows.Length;i++){
    bool visible=i<visibleRows;rowRects[i].gameObject.SetActive(visible);foreach(var cell in rows[i])cell.gameObject.SetActive(visible);if(!visible)continue;
    var result=ranked[i];var p=result.player;bool isLocal=p.PlayerId==local?.PlayerId;string prefix=state.Phase==GolfMatchPhase.Ended&&p.IsFinished?result.rank+(result.tie?"= ":". "):"";
    rowPanels[i].Refresh(1,isLocal?GolfHUDStyle.LocalBlue:i%2==0?LocalProfile.Hex("DFF5F0"):LocalProfile.Hex("FFF9E9"));avatars[i].accent=isLocal?GolfHUDStyle.Gold:GolfHUDStyle.Teal;
    bool namedYou=string.Equals(p.PlayerName,"You",StringComparison.OrdinalIgnoreCase);FitLine(rows[i][0],prefix+p.PlayerName+(isLocal&&!namedYou?" • you":""),rows[i][0].fontSize,17);rows[i][1].text=$"{p.CompletedHoleCount} / 5";rows[i][2].text=p.TotalStroke.ToString();FitLine(rows[i][3],p.Status,rows[i][3].fontSize,16);rows[i][4].text=p.IsFinished?FinishTime(p.FinishTime):"-";
   }
   footer.text=local==null?(match.Authority?"Start a match to play all five holes.":"Waiting for the host to start."):local.IsDNF?"Did not finish • Progress and strokes saved":local.IsFinished?"Finished • Ranking: strokes, then finish time":$"You: Hole {local.CurrentHole}  •  This hole: {local.CurrentHoleStroke}  •  Total: {local.TotalStroke}";
   bool countdown=state.CountdownStarted&&state.Phase==GolfMatchPhase.FinalCountdown;
   bool expired=state.Phase==GolfMatchPhase.Ended&&state.Players.Any(p=>p.IsDNF);
   timer.gameObject.SetActive(countdown||expired);notice.gameObject.SetActive(countdown||expired);countdownCard.gameObject.SetActive(countdown||expired);noticeCard.gameObject.SetActive(countdown||expired);
   timer.text=expired?"00:00":FinishTime(Math.Ceiling(match.Remaining));notice.text=expired?"Match finished":"A player has finished!";
  }
  public bool FitsSafeFrame(){
   if(!board||!page)return false;var corners=new Vector3[4];board.GetWorldCorners(corners);var frame=page.rect;
   return corners.All(c=>{var p=page.InverseTransformPoint(c);return p.x>=frame.xMin-.5f&&p.x<=frame.xMax+.5f&&p.y>=frame.yMin-.5f&&p.y<=frame.yMax+.5f;});
  }
  public bool RowsReadable(){
   for(int i=0;i<visibleRows;i++)foreach(var label in rows[i])if(label.fontSize<16||label.cachedTextGenerator.vertexCount==0)return false;return true;
  }
  public bool DividersVisible()=>dividers!=null&&dividers.All(d=>d.gameObject.activeInHierarchy&&d.rect.width>=1&&d.rect.height>0);
 }
}
