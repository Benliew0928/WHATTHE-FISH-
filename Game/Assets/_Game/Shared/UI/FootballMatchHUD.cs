using System;
using UnityEngine;
using UnityEngine.UI;
namespace WhatTheFish {
 // Presentation only; authority still owns the score, roster and deadlines.
 public sealed class FootballMatchHUD:MonoBehaviour {
  Text startLabel,selectionHint,chooseALabel,chooseBLabel,botLabel;Button startButton,chooseA,chooseB,botsButton;RectTransform selectionPanel;SportsScoreboard presentation;
  RectTransform botSizes;readonly Button[] sizeButtons=new Button[4];
  Text selectionSubtitle;Button confirmTeam;
  public static FootballMatchHUD Create(Transform parent,Font unused){var r=CoveUI.Rect(parent,"Football match HUD",0,0,304,56);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-16);var hud=r.gameObject.AddComponent<FootballMatchHUD>();hud.Build();return hud;}
  void Build(){
   presentation=SportsScoreboard.Create(transform,SportId.Football);
   startButton=FootballUIStyle.Button(transform,"Start match","Start match",42,81,220,34,()=>FootballMatch.Instance?.StartMatch(),CoveUI.Cream,13);startLabel=startButton.GetComponentInChildren<Text>();
   botSizes=CoveUI.Rect(transform,"Bot team sizes",42,123,220,28);
   for(int i=0;i<4;i++){int size=i+2;sizeButtons[i]=FootballUIStyle.Button(botSizes,"Bot team size "+size,size+"v"+size,i*56,0,52,28,()=>FootballBots.PreferredTeamSize=size,CoveUI.Cream,12);}
   botsButton=FootballUIStyle.Button(transform,"Room bots","Fill with bots",42,159,220,34,()=>{var bots=FootballBots.Instance;if(!bots)return;if(bots.Count>0)bots.Clear();else bots.FillRoom();},CoveUI.Sky,13);botLabel=botsButton.GetComponentInChildren<Text>();
   selectionPanel=BasketballUIStyle.Plate(transform,"Team selection",-98,105,500,246,CoveUI.Cream,22,2,true).rectTransform;
   BasketballUIStyle.Text(selectionPanel,"Choose your team",20,12,460,36,24,true,TextAnchor.MiddleCenter);selectionSubtitle=BasketballUIStyle.Text(selectionPanel,"Unselected players are assigned when time expires.",20,48,460,23,11,false,TextAnchor.MiddleCenter);
   chooseA=FootballUIStyle.Button(selectionPanel,"Team A choice","Blue",18,80,224,116,()=>FootballMatch.Instance?.RequestTeam(FootballTeam.A),CoveUI.Sky,17);chooseALabel=chooseA.GetComponentInChildren<Text>();
   chooseB=FootballUIStyle.Button(selectionPanel,"Team B choice","Red",258,80,224,116,()=>FootballMatch.Instance?.RequestTeam(FootballTeam.B),FootballUIStyle.Cancel,17);chooseBLabel=chooseB.GetComponentInChildren<Text>();
   selectionHint=BasketballUIStyle.Text(selectionPanel,"",20,209,460,23,12,false,TextAnchor.MiddleCenter);selectionPanel.gameObject.SetActive(false);
   MenuDesigns.Dialog(selectionPanel.GetComponent<BasketballPlate>());MenuDesigns.TeamCard(chooseA,true);MenuDesigns.TeamCard(chooseB,false);
   confirmTeam=FootballUIStyle.Button(selectionPanel,"Start bot match","Start match",140,249,220,42,()=>FootballMatch.Instance?.ConfirmSoloTeam(),CoveUI.Mint,17);
  }
  void Update(){var match=FootballMatch.Instance;if(!match){startButton.gameObject.SetActive(false);botsButton.gameObject.SetActive(false);botSizes.gameObject.SetActive(false);return;}Present(match.Snapshot,match.Remaining,match.TeamOf(AppRoot.Instance.LocalAthlete),match.Authority&&match.MatchMode,Time.unscaledDeltaTime);if(startButton.gameObject.activeSelf){startButton.interactable=match.CanStart;if(!match.CanStart)startLabel.text="Even teams · 2–10 players";}var bots=FootballBots.Instance;bool offer=!presentation.DetailsExpanded&&bots&&AppRoot.Instance.rooms.Host&&!match.Active&&(FootballBots.Roster.Length<10||bots.Count>0);botsButton.gameObject.SetActive(offer);botSizes.gameObject.SetActive(offer);botsButton.interactable=bots&&(bots.Count>0||bots.CanFill);botLabel.text=bots&&bots.Count>0?"Remove bots":$"Fill {FootballBots.PreferredTeamSize}v{FootballBots.PreferredTeamSize} with bots";
   for(int i=0;i<sizeButtons.Length;i++){MenuDesigns.Selected(sizeButtons[i],FootballBots.PreferredTeamSize==i+2);sizeButtons[i].interactable=bots&&bots.CanFillSize(i+2);var plate=sizeButtons[i].GetComponent<BasketballPlate>();plate.color=FootballBots.PreferredTeamSize==i+2?CoveUI.Sky:CoveUI.Cream;}
  }
  void Present(FootballMatchSnapshot s,double remaining,FootballTeam team,bool authority,float dt){
   if(s.phase==FootballMatchPhase.Finished&&FootballMatch.Instance&&FootballMatch.Instance.GoalSequence)s.phase=FootballMatchPhase.GoalReset;
   startButton.gameObject.SetActive(authority&&!presentation.DetailsExpanded&&(s.phase==FootballMatchPhase.Idle||s.phase==FootballMatchPhase.Finished||s.phase==FootballMatchPhase.Cancelled));startLabel.text=s.phase==FootballMatchPhase.Idle?"Start match":"New match";
   bool selecting=s.phase==FootballMatchPhase.TeamSelection,solo=FootballMatch.Instance&&FootballMatch.Instance.SoloTeamSelection;selectionPanel.gameObject.SetActive(selecting);
   selectionPanel.sizeDelta=new Vector2(500,solo?310:246);selectionSubtitle.text=solo?"Pick a side, then start when you're ready.":"Unselected players are assigned when time expires.";
   confirmTeam.gameObject.SetActive(selecting&&solo);confirmTeam.interactable=FootballMatch.Instance&&FootballMatch.Instance.CanConfirmSoloTeam;
   chooseALabel.text=solo?(team==FootballTeam.A?"Blue  ✓":"Blue"):"Blue\n"+s.teamA+" / "+s.teamCapacity+(s.teamA>=s.teamCapacity?" · Full":"");chooseBLabel.text=solo?(team==FootballTeam.B?"Red  ✓":"Red"):"Red\n"+s.teamB+" / "+s.teamCapacity+(s.teamB>=s.teamCapacity?" · Full":"");
   MenuDesigns.Selected(chooseA,team==FootballTeam.A);MenuDesigns.Selected(chooseB,team==FootballTeam.B);
   chooseA.interactable=selecting&&(team==FootballTeam.A||s.teamA<s.teamCapacity);chooseB.interactable=selecting&&(team==FootballTeam.B||s.teamB<s.teamCapacity);
   selectionHint.text=team==FootballTeam.None?(solo?"Choose Blue or Red to continue.":"Auto-assignment in "+Math.Ceiling(Math.Max(0,remaining))+"s"):"You: "+CharacterIdentity.TeamName(team)+(solo?" · Ready to start":" · Confirmed when time expires");
  }
#if UNITY_EDITOR
  public void Preview(FootballMatchSnapshot snapshot,double seconds,FootballTeam team,float elapsed=0){presentation.Live=false;presentation.Present(SportsScoreboardFrame.Football(snapshot,seconds),elapsed);Present(snapshot,seconds,team,true,elapsed);}
#endif
 }
}
