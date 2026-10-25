using System;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class BasketballMatchHUD:MonoBehaviour {
  Text startText,chooseAText,chooseBText,hint;SportsScoreboard presentation;
  Button start,chooseA,chooseB,confirmTeam;GameObject board,selection;
  RectTransform botSizes;Button botsButton;Text botsText;readonly Button[] sizeButtons=new Button[4];
  GameObject movementNotice;Text movementText;
  public static void Create(Transform parent){var r=CoveUI.Rect(parent,"Basketball match HUD",0,0,304,56);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-16);r.gameObject.AddComponent<BasketballMatchHUD>().Build();}
  void Build(){
   presentation=SportsScoreboard.Create(transform,SportId.Basketball);board=presentation.gameObject;
   movementNotice=BasketballUIStyle.Plate(board.transform,"Automatic basketball movement",0,86,304,42,CoveUI.Cream,16,1.5f).gameObject;
   CharacterIdentityBadges.ReserveHUD((RectTransform)movementNotice.transform);
   movementText=BasketballUIStyle.Text(movementNotice.transform,"",8,3,288,36,11,true,TextAnchor.MiddleCenter);
   start=BasketballUIStyle.Button(board.transform,"Start basketball match","Start match",42,112,220,34,()=>BasketballMatch.Instance?.StartMatch(),CoveUI.Cream,13);startText=start.GetComponentInChildren<Text>();
   botSizes=CoveUI.Rect(board.transform,"Basketball bot team sizes",42,154,220,28);
   for(int i=0;i<4;i++){int size=i+2;sizeButtons[i]=BasketballUIStyle.Button(botSizes,"Basketball bot size "+size,size+"v"+size,i*56,0,52,28,()=>BasketballBots.PreferredTeamSize=size,CoveUI.Cream,12);}
   botsButton=BasketballUIStyle.Button(board.transform,"Basketball room bots","Fill with bots",42,190,220,34,()=>{var bots=BasketballBots.Instance;if(!bots)return;if(bots.Count>0)bots.Clear();else bots.FillRoom(BasketballBots.PreferredTeamSize);},CoveUI.Sky,13);botsText=botsButton.GetComponentInChildren<Text>();
   selection=BasketballUIStyle.Plate(board.transform,"Choose basketball teams",-98,100,500,246,CoveUI.Cream,20,2,true).gameObject;
   BasketballUIStyle.Text(selection.transform,"Choose your team",20,14,460,36,24,true,TextAnchor.MiddleCenter);
   chooseA=BasketballUIStyle.Button(selection.transform,"Basketball Team A","Blue",18,75,224,116,()=>BasketballMatch.Instance?.RequestTeam(FootballTeam.A),CoveUI.Sky,16);chooseAText=chooseA.GetComponentInChildren<Text>();
   chooseB=BasketballUIStyle.Button(selection.transform,"Basketball Team B","Red",258,75,224,116,()=>BasketballMatch.Instance?.RequestTeam(FootballTeam.B),CoveUI.Coral,16);chooseBText=chooseB.GetComponentInChildren<Text>();
   hint=BasketballUIStyle.Text(selection.transform,"",20,204,460,27,12,false,TextAnchor.MiddleCenter);
   MenuDesigns.Dialog(selection.GetComponent<BasketballPlate>());MenuDesigns.TeamCard(chooseA,true);MenuDesigns.TeamCard(chooseB,false);
   confirmTeam=BasketballUIStyle.Button(selection.transform,"Start bot match","Start match",140,245,220,42,()=>BasketballMatch.Instance?.ConfirmSoloTeam(),CoveUI.Mint,17);
  }
  void Update(){
   var m=BasketballMatch.Instance;bool show=m&&(m.Room||m.Competitive);board.SetActive(show);if(!show)return;
   var s=m.Snapshot;var own=m.TeamOf(AppRoot.Instance.LocalAthlete);
   movementText.text=m.MovementHint(AppRoot.Instance.LocalAthlete);movementNotice.SetActive(movementText.text.Length>0&&!presentation.DetailsExpanded);
   ((RectTransform)movementNotice.transform).anchoredPosition=new Vector2(0,presentation.Animating?-152:-100);
   bool canOffer=m.Authority&&m.MatchMode&&!m.Active&&!presentation.DetailsExpanded;start.gameObject.SetActive(canOffer);start.interactable=m.CanStart;startText.text=!m.CanStart?"Even teams: 4-10 players":s.phase==BasketballMatchPhase.Idle?"Start match":"New match";
   float celebrationOffset=presentation.Animating?40:0;((RectTransform)start.transform).anchoredPosition=new Vector2(42,-112-celebrationOffset);botSizes.anchoredPosition=new Vector2(42,-154-celebrationOffset);((RectTransform)botsButton.transform).anchoredPosition=new Vector2(42,-190-celebrationOffset);
   var bots=BasketballBots.Instance;bool offer=canOffer&&m.Room&&bots;botSizes.gameObject.SetActive(offer);botsButton.gameObject.SetActive(offer);
   botsButton.interactable=bots&&(bots.Count>0||bots.CanFillSize(BasketballBots.PreferredTeamSize));botsText.text=bots&&bots.Count>0?"Remove bots":$"Fill {BasketballBots.PreferredTeamSize}v{BasketballBots.PreferredTeamSize} with bots";
   for(int i=0;i<4;i++){MenuDesigns.Selected(sizeButtons[i],BasketballBots.PreferredTeamSize==i+2);sizeButtons[i].interactable=bots&&bots.CanFillSize(i+2);sizeButtons[i].GetComponent<BasketballPlate>().color=BasketballBots.PreferredTeamSize==i+2?CoveUI.Sky:CoveUI.Cream;}
   bool selecting=s.phase==BasketballMatchPhase.TeamSelection;selection.SetActive(selecting);
   bool solo=m.SoloTeamSelection;((RectTransform)selection.transform).sizeDelta=new Vector2(500,solo?306:246);confirmTeam.gameObject.SetActive(selecting&&solo);confirmTeam.interactable=m.CanConfirmSoloTeam;
   chooseAText.text=solo?(own==FootballTeam.A?"Blue  ✓":"Blue"):"Blue\n"+s.teamA+" / "+s.capacity+(s.teamA>=s.capacity?" · Full":"");chooseBText.text=solo?(own==FootballTeam.B?"Red  ✓":"Red"):"Red\n"+s.teamB+" / "+s.capacity+(s.teamB>=s.capacity?" · Full":"");
   MenuDesigns.Selected(chooseA,own==FootballTeam.A);MenuDesigns.Selected(chooseB,own==FootballTeam.B);
   chooseA.interactable=selecting&&(own==FootballTeam.A||s.teamA<s.capacity);chooseB.interactable=selecting&&(own==FootballTeam.B||s.teamB<s.capacity);
   hint.text=own==FootballTeam.None?(solo?"Pick a side, then start when you're ready.":"Auto-assignment in "+Math.Ceiling(Math.Max(0,s.deadline-m.Now))+"s"):CharacterIdentity.TeamName(own)+" selected"+(solo?"":" · Starts in "+Math.Ceiling(Math.Max(0,s.deadline-m.Now))+"s");
  }
 }
}
