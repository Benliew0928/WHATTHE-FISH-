using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Read-only presentation. Rules, hole detection and ranking live in gameplay.
 public sealed class GolfLeaderboardUI:MonoBehaviour {
  Text title,footer,timer,notice;RectTransform board;Text[][] rows;
  public bool CountdownVisible=>timer&&timer.gameObject.activeSelf;
  public string CountdownText=>timer?timer.text:"";
  public static void Create(RectTransform page,Font font){var ui=Rect("Golf HUD",page,new Vector2(.5f,.5f),Vector2.zero,Vector2.zero).gameObject.AddComponent<GolfLeaderboardUI>();ui.Build(page,font);page.gameObject.AddComponent<GolfTargetIndicator>().Build(page,font);}
  public static RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=position;r.sizeDelta=size;return r;}
  public static Text Text(string name,Transform parent,Font font,Vector2 anchor,Vector2 position,Vector2 size,int points){var r=Rect(name,parent,anchor,position,size);var t=r.gameObject.AddComponent<Text>();t.font=font;t.fontSize=points;t.color=Color.white;t.alignment=TextAnchor.MiddleLeft;t.raycastTarget=false;t.supportRichText=false;var shadow=t.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.65f);shadow.effectDistance=new Vector2(1,-1);return t;}
  void Build(RectTransform page,Font font){
   board=Rect("Golf leaderboard",page,Vector2.one,new Vector2(-28,-22),new Vector2(570,160));board.pivot=Vector2.one;
   var panel=board.gameObject.AddComponent<Image>();panel.color=new Color(.025f,.14f,.18f,.88f);panel.raycastTarget=false;
   title=Text("Leaderboard title",board,font,new Vector2(0,1),new Vector2(282,-25),new Vector2(532,32),23);title.fontStyle=FontStyle.Bold;
   float[] x={87,199,273,364,488};float[] width={138,78,66,108,112};string[] names={"Player","Progress","Strokes","Status","Finish time"};
   for(int j=0;j<names.Length;j++){var header=Text("Column "+names[j],board,font,new Vector2(0,1),new Vector2(x[j],-59),new Vector2(width[j],25),16);header.text=names[j];header.color=LocalProfile.Teams[0];}
   rows=new Text[10][];
   for(int i=0;i<rows.Length;i++){rows[i]=new Text[5];for(int j=0;j<5;j++)rows[i][j]=Text("Player "+i+" "+names[j],board,font,new Vector2(0,1),new Vector2(x[j],-87-i*25),new Vector2(width[j],25),17);}
   footer=Text("Your progress",board,font,new Vector2(0,1),new Vector2(282,-130),new Vector2(532,36),16);
   timer=Text("Final 30 seconds",page,font,new Vector2(.5f,1),new Vector2(0,-179),new Vector2(270,58),44);timer.alignment=TextAnchor.MiddleCenter;timer.color=LocalProfile.Hex("F0B956");timer.fontStyle=FontStyle.Bold;
   notice=Text("First finisher notice",page,font,new Vector2(.5f,1),new Vector2(0,-219),new Vector2(500,32),20);notice.alignment=TextAnchor.MiddleCenter;
   timer.gameObject.SetActive(false);notice.gameObject.SetActive(false);
  }
  public static string FinishTime(double seconds){var total=(long)Math.Max(0,Math.Floor(seconds));return $"{total/60:00}:{total%60:00}";}
  void Update(){
   var match=GolfMatchManager.Instance;if(!match||!match.Context)return;
   var state=match.State;var ranked=state.Results();var local=match.Player(AppRoot.Instance.LocalAthlete);
   title.text=state.Phase==GolfMatchPhase.Ended?"LEADERBOARD  •  RESULTS":"LEADERBOARD";
   for(int i=0;i<rows.Length;i++){
    bool visible=i<ranked.Length;foreach(var cell in rows[i])cell.gameObject.SetActive(visible);if(!visible)continue;
    var result=ranked[i];var p=result.player;string prefix=state.Phase==GolfMatchPhase.Ended&&p.IsFinished?result.rank+(result.tie?"= ":". "):"";
    rows[i][0].text=prefix+p.PlayerName+(p.PlayerId==local?.PlayerId?" • you":"");rows[i][1].text=$"{p.CompletedHoleCount} / 5";rows[i][2].text=p.TotalStroke.ToString();rows[i][3].text=p.Status;rows[i][4].text=p.IsFinished?FinishTime(p.FinishTime):"-";
   }
   int count=Math.Max(1,ranked.Length);board.sizeDelta=new Vector2(570,122+count*25);footer.rectTransform.anchoredPosition=new Vector2(282,-104-count*25);
   footer.text=local==null?(match.Authority?"Start a match to play all five holes.":"Waiting for the host to start."):local.IsDNF?"Did not finish • Progress and strokes saved":local.IsFinished?"Finished • Ranking: strokes, then finish time":$"You: Hole {local.CurrentHole}  •  This hole: {local.CurrentHoleStroke}  •  Total: {local.TotalStroke}";
   bool countdown=state.CountdownStarted&&state.Phase==GolfMatchPhase.FinalCountdown;
   bool expired=state.Phase==GolfMatchPhase.Ended&&state.Players.Any(p=>p.IsDNF);
   timer.gameObject.SetActive(countdown||expired);notice.gameObject.SetActive(countdown||expired);
   timer.text=expired?"00:00":FinishTime(Math.Ceiling(match.Remaining));notice.text=expired?"Match finished":"A player has finished!";
  }
 }
}
