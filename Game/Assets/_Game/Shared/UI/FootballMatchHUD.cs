using System;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 // Presentation only; authority still owns the score, roster and deadlines.
 public sealed class FootballMatchHUD:MonoBehaviour {
  Text scoreA,scoreB,timer,phase,startLabel,selectionHint,chooseALabel,chooseBLabel;Button startButton,chooseA,chooseB;RectTransform selectionPanel;GameObject youA,youB;BasketballPlate board;int previousA=-1,previousB=-1;float pulseA,pulseB;
  public static FootballMatchHUD Create(Transform parent,Font unused){var r=CoveUI.Rect(parent,"Football match HUD",0,0,304,56);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-16);var hud=r.gameObject.AddComponent<FootballMatchHUD>();hud.Build();return hud;}
  void Build(){
   board=BasketballUIStyle.Plate(transform,"Football score strip",0,0,304,56,CoveUI.Cream,21,2,true);board.shadow=4;
   Team(16,"TEAM A",CoveUI.Hex("7DC8E8"),out youA);Team(235,"TEAM B",CoveUI.Coral,out youB);
   scoreA=BasketballUIStyle.Text(board.transform,"0",65,3,33,45,31,true,TextAnchor.MiddleCenter);scoreB=BasketballUIStyle.Text(board.transform,"0",207,3,33,45,31,true,TextAnchor.MiddleCenter);
   BasketballUIStyle.Plate(board.transform,"Left score divider",105,10,1,36,CoveUI.Hex("D5D7C5"),0,0);BasketballUIStyle.Plate(board.transform,"Right score divider",199,10,1,36,CoveUI.Hex("D5D7C5"),0,0);
   timer=BasketballUIStyle.Text(board.transform,"03:00",110,3,84,31,23,true,TextAnchor.MiddleCenter);phase=BasketballUIStyle.Text(board.transform,"MATCH",109,34,86,17,8,false,TextAnchor.MiddleCenter);
   startButton=FootballUIStyle.Button(transform,"Start match","Start match",42,67,220,34,()=>FootballMatch.Instance?.StartMatch(),CoveUI.Cream,13);startLabel=startButton.GetComponentInChildren<Text>();
   selectionPanel=BasketballUIStyle.Plate(transform,"Team selection",-78,115,460,176,CoveUI.Cream,22,2,true).rectTransform;
   BasketballUIStyle.Text(selectionPanel,"Choose your team",20,9,420,32,21,true,TextAnchor.MiddleCenter);BasketballUIStyle.Text(selectionPanel,"Your choice locks when the countdown ends.",20,42,420,23,12,false,TextAnchor.MiddleCenter);
   chooseA=FootballUIStyle.Button(selectionPanel,"Team A choice","Team A",18,73,207,55,()=>FootballMatch.Instance?.RequestTeam(FootballTeam.A),CoveUI.Sky,17);chooseALabel=chooseA.GetComponentInChildren<Text>();
   chooseB=FootballUIStyle.Button(selectionPanel,"Team B choice","Team B",235,73,207,55,()=>FootballMatch.Instance?.RequestTeam(FootballTeam.B),FootballUIStyle.Cancel,17);chooseBLabel=chooseB.GetComponentInChildren<Text>();
   selectionHint=BasketballUIStyle.Text(selectionPanel,"",18,140,424,23,12,false,TextAnchor.MiddleCenter);selectionPanel.gameObject.SetActive(false);
  }
  void Team(float x,string name,Color accent,out GameObject you){BasketballUIStyle.Plate(board.transform,name+" accent",x,8,44,3,accent,0,0);BasketballUIStyle.Text(board.transform,name,x-4,14,52,18,11,true,TextAnchor.MiddleCenter);var chip=BasketballUIStyle.Plate(board.transform,name+" YOU",x,35,44,14,accent,4,0);BasketballUIStyle.Text(chip.transform,"YOU",0,0,44,14,8,false,TextAnchor.MiddleCenter);you=chip.gameObject;you.SetActive(false);}
  void Update(){var match=FootballMatch.Instance;if(!match){startButton.gameObject.SetActive(false);return;}Present(match.Snapshot,match.Remaining,match.TeamOf(AppRoot.Instance.LocalAthlete),match.Authority,Time.unscaledDeltaTime);if(startButton.gameObject.activeSelf){startButton.interactable=match.CanStart;if(!match.CanStart)startLabel.text="Even teams · 2–10 players";}}
  void Present(FootballMatchSnapshot s,double remaining,FootballTeam team,bool authority,float dt){
   if(previousA>=0&&s.scoreA>previousA)pulseA=.65f;if(previousB>=0&&s.scoreB>previousB)pulseB=.65f;previousA=s.scoreA;previousB=s.scoreB;pulseA=Mathf.Max(0,pulseA-dt);pulseB=Mathf.Max(0,pulseB-dt);
   scoreA.color=Color.Lerp(CoveUI.Ink,CoveUI.Hex("2788ED"),pulseA/.65f);scoreB.color=Color.Lerp(CoveUI.Ink,CoveUI.Hex("C6543D"),pulseB/.65f);scoreA.text=s.scoreA.ToString();scoreB.text=s.scoreB.ToString();
   board.color=s.phase==FootballMatchPhase.Overtime?CoveUI.Hex("FFF0C1"):CoveUI.Cream;timer.fontSize=46;timer.text=TimeSpan.FromSeconds(Math.Max(0,Math.Ceiling(remaining))).ToString(@"mm\:ss");
   phase.text=s.phase switch {FootballMatchPhase.Idle=>"KICKABOUT",FootballMatchPhase.Kickoff=>"KICKOFF",FootballMatchPhase.Regulation=>"MATCH",FootballMatchPhase.TeamSelection=>"CHOOSE TEAMS",FootballMatchPhase.Overtime=>"GOLDEN GOAL",FootballMatchPhase.Finished=>s.result==FootballMatchResult.Draw?"DRAW":s.result==FootballMatchResult.TeamA?"TEAM A WINS":"TEAM B WINS",_=>"GOAL"};
   if(s.phase==FootballMatchPhase.Idle){timer.text="PRACTICE";timer.fontSize=28;}
   if(s.phase==FootballMatchPhase.Kickoff||s.phase==FootballMatchPhase.TeamSelection)timer.text=Math.Ceiling(remaining).ToString();
   if(s.phase==FootballMatchPhase.Finished){timer.text="FULL TIME";timer.fontSize=26;}
   startButton.gameObject.SetActive(authority&&(s.phase==FootballMatchPhase.Idle||s.phase==FootballMatchPhase.Finished));startLabel.text=s.phase==FootballMatchPhase.Finished?"New match":"Start match";
   bool selecting=s.phase==FootballMatchPhase.TeamSelection;selectionPanel.gameObject.SetActive(selecting);chooseALabel.text="Team A  "+s.teamA+" / "+s.teamCapacity+(s.teamA>=s.teamCapacity?" · Full":"");chooseBLabel.text="Team B  "+s.teamB+" / "+s.teamCapacity+(s.teamB>=s.teamCapacity?" · Full":"");
   chooseA.interactable=selecting&&(team==FootballTeam.A||s.teamA<s.teamCapacity);chooseB.interactable=selecting&&(team==FootballTeam.B||s.teamB<s.teamCapacity);
   selectionHint.text=team==FootballTeam.None?"You haven't picked a team yet.":"You: Team "+team+" · Confirmed when time expires";youA.SetActive(team==FootballTeam.A);youB.SetActive(team==FootballTeam.B);
  }
#if UNITY_EDITOR
  public void Preview(FootballMatchSnapshot snapshot,double seconds,FootballTeam team,float elapsed=0){Present(snapshot,seconds,team,true,elapsed);}
#endif
 }
}
