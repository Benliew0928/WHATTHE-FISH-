using System;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class BasketballMatchHUD:MonoBehaviour {
  Text a,b,clock,phase,shot,startText,chooseAText,chooseBText,hint,team;
  Button start,chooseA,chooseB;GameObject board,selection;
  RectTransform botSizes;Button botsButton;Text botsText;readonly Button[] sizeButtons=new Button[4];
  public static void Create(Transform parent){var r=CoveUI.Rect(parent,"Basketball match HUD",0,0,304,56);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-16);r.gameObject.AddComponent<BasketballMatchHUD>().Build();}
  void Build(){
   board=BasketballUIStyle.Plate(transform,"Team scoreboard",0,0,304,78,CoveUI.Cream,20,2,true).gameObject;
   CharacterIdentityBadges.ReserveHUD((RectTransform)board.transform);
   BasketballUIStyle.Text(board.transform,"BLUE",7,4,64,18,11,true,TextAnchor.MiddleCenter);BasketballUIStyle.Text(board.transform,"RED",233,4,64,18,11,true,TextAnchor.MiddleCenter);
   a=BasketballUIStyle.Text(board.transform,"0",8,20,62,32,26,true,TextAnchor.MiddleCenter);b=BasketballUIStyle.Text(board.transform,"0",234,20,62,32,26,true,TextAnchor.MiddleCenter);
   clock=BasketballUIStyle.Text(board.transform,"03:00",78,2,148,30,23,true,TextAnchor.MiddleCenter);phase=BasketballUIStyle.Text(board.transform,"WARM-UP",72,34,160,18,10,true,TextAnchor.MiddleCenter);
   shot=BasketballUIStyle.Text(board.transform,"",105,55,192,18,11,true,TextAnchor.MiddleCenter);
   team=BasketballUIStyle.Text(board.transform,"",7,55,98,18,11,true,TextAnchor.MiddleCenter);
   start=BasketballUIStyle.Button(board.transform,"Start basketball match","Start match",42,112,220,34,()=>BasketballMatch.Instance?.StartMatch(),CoveUI.Cream,13);startText=start.GetComponentInChildren<Text>();
   botSizes=CoveUI.Rect(board.transform,"Basketball bot team sizes",42,154,220,28);
   for(int i=0;i<4;i++){int size=i+2;sizeButtons[i]=BasketballUIStyle.Button(botSizes,"Basketball bot size "+size,size+"v"+size,i*56,0,52,28,()=>BasketballBots.PreferredTeamSize=size,CoveUI.Cream,12);}
   botsButton=BasketballUIStyle.Button(board.transform,"Basketball room bots","Fill with bots",42,190,220,34,()=>{var bots=BasketballBots.Instance;if(!bots)return;if(bots.Count>0)bots.Clear();else bots.FillRoom(BasketballBots.PreferredTeamSize);},CoveUI.Sky,13);botsText=botsButton.GetComponentInChildren<Text>();
   selection=BasketballUIStyle.Plate(board.transform,"Choose basketball teams",-78,110,460,148,CoveUI.Cream,20,2,true).gameObject;
   BasketballUIStyle.Text(selection.transform,"Choose your team",12,8,436,30,20,true,TextAnchor.MiddleCenter);
   chooseA=BasketballUIStyle.Button(selection.transform,"Basketball Team A","Blue",14,48,207,48,()=>BasketballMatch.Instance?.RequestTeam(FootballTeam.A),CoveUI.Sky,16);chooseAText=chooseA.GetComponentInChildren<Text>();
   chooseB=BasketballUIStyle.Button(selection.transform,"Basketball Team B","Red",239,48,207,48,()=>BasketballMatch.Instance?.RequestTeam(FootballTeam.B),CoveUI.Coral,16);chooseBText=chooseB.GetComponentInChildren<Text>();
   hint=BasketballUIStyle.Text(selection.transform,"",12,107,436,27,12,false,TextAnchor.MiddleCenter);
  }
  void Update(){
   var m=BasketballMatch.Instance;bool show=m&&(m.Room||m.Competitive);board.SetActive(show);if(!show)return;
   var s=m.Snapshot;var own=m.TeamOf(AppRoot.Instance.LocalAthlete);a.text=s.scoreA.ToString();b.text=s.scoreB.ToString();
   clock.text=TimeSpan.FromSeconds(Math.Max(0,Math.Ceiling(m.Remaining))).ToString(@"mm\:ss");
   phase.text=s.phase switch{BasketballMatchPhase.Idle=>"WARM-UP",BasketballMatchPhase.TeamSelection=>"CHOOSE TEAMS",BasketballMatchPhase.Countdown=>"TIP-OFF",BasketballMatchPhase.Regulation=>"MATCH",BasketballMatchPhase.Overtime=>"GOLDEN BASKET",BasketballMatchPhase.Buzzer=>"LAST SHOT",BasketballMatchPhase.Restart=>s.restart==BasketballRestart.Basket?"BASKET":s.restart==BasketballRestart.ShotClock?"SHOT CLOCK":"OUT OF BOUNDS",BasketballMatchPhase.Cancelled=>"PLAYER LEFT - MATCH VOID",_=>s.result==FootballMatchResult.Draw?"DRAW":s.result==FootballMatchResult.TeamA?"BLUE WINS":"RED WINS"};
   if(s.phase==BasketballMatchPhase.Idle)clock.text="READY";
   if(s.phase==BasketballMatchPhase.TeamSelection||s.phase==BasketballMatchPhase.Countdown)clock.text=Math.Ceiling(Math.Max(0,s.deadline-m.Now)).ToString();
   if(s.phase==BasketballMatchPhase.Buzzer)clock.text="00:00";
   if(s.phase==BasketballMatchPhase.Cancelled)clock.text="VOID";
   shot.text=s.phase==BasketballMatchPhase.Restart?(m.RestartReady?CharacterIdentity.TeamName(s.possession).ToUpperInvariant()+" · PASS TO OPEN":CharacterIdentity.TeamName(s.possession).ToUpperInvariant()+" BALL  "+Math.Ceiling(Math.Max(0,s.deadline-m.Now))):m.Running&&s.possession!=FootballTeam.None?"SHOT CLOCK  "+Math.Ceiling(Math.Max(0,s.shotDeadline-m.Now)):"";
   team.text=own==FootballTeam.None?"":"YOU: "+CharacterIdentity.TeamName(own).ToUpperInvariant();
   bool canOffer=m.Authority&&m.MatchMode&&!m.Active;start.gameObject.SetActive(canOffer);start.interactable=m.CanStart;startText.text=!m.CanStart?"Even teams: 4-10 players":s.phase==BasketballMatchPhase.Idle?"Start match":"New match";
   var bots=BasketballBots.Instance;bool offer=canOffer&&m.Room&&bots;botSizes.gameObject.SetActive(offer);botsButton.gameObject.SetActive(offer);
   botsButton.interactable=bots&&(bots.Count>0||bots.CanFillSize(BasketballBots.PreferredTeamSize));botsText.text=bots&&bots.Count>0?"Remove bots":$"Fill {BasketballBots.PreferredTeamSize}v{BasketballBots.PreferredTeamSize} with bots";
   for(int i=0;i<4;i++){sizeButtons[i].interactable=bots&&bots.CanFillSize(i+2);sizeButtons[i].GetComponent<BasketballPlate>().color=BasketballBots.PreferredTeamSize==i+2?CoveUI.Sky:CoveUI.Cream;}
   bool selecting=s.phase==BasketballMatchPhase.TeamSelection;selection.SetActive(selecting);
   chooseAText.text="Blue  "+s.teamA+" / "+s.capacity;chooseBText.text="Red  "+s.teamB+" / "+s.capacity;
   chooseA.interactable=selecting&&(own==FootballTeam.A||s.teamA<s.capacity);chooseB.interactable=selecting&&(own==FootballTeam.B||s.teamB<s.capacity);
   hint.text=own==FootballTeam.None?"Unselected players are assigned when time expires.":CharacterIdentity.TeamName(own)+" selected";
  }
 }
}
