using System;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 [DefaultExecutionOrder(500)]
 public sealed class PostMatchFlow:MonoBehaviour {
  public static PostMatchFlow Instance {get;private set;}
  public bool Visible=>panel&&panel.gameObject.activeSelf;
  public bool WaitingVisible=>dock&&dock.gameObject.activeSelf;
  public static bool MatchActive=>FootballMatch.Instance&&FootballMatch.Instance.Active||BasketballMatch.Instance&&BasketballMatch.Instance.Active||GolfMatchManager.Instance&&GolfMatchManager.Instance.State.Running||FishingGame.Instance&&(FishingGame.Instance.State.Match.phase is FishingRoundPhase.Playing or FishingRoundPhase.Countdown or FishingRoundPhase.Drafting);
  RectTransform panel,content,dock;GameObject hostControls,guestControls;Button reopen,again,teleport,station,game,ready;Button[] cards=new Button[4];Text title,subtitle,status,party,againLabel,travelLabel,readyLabel,dockTitle,dockHint;Text[] capacities=new Text[4];
  AppRoot app;SportId selected;TeleportDestination destination;string shown="",current="";bool wasBlocked;string actionMessage="";
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){if(!Instance)new GameObject("Next adventure",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).AddComponent<PostMatchFlow>();}
  void Awake(){Instance=this;WorldUI.Canvas(gameObject,950);Build();}
  void Build(){
   panel=CoveUI.Rect(transform,"Next adventure overlay",0,0,1600,900);CoveUI.Stretch(panel);panel.gameObject.AddComponent<Image>().color=new Color(.025f,.045f,.04f,.34f);
   content=CoveUI.Rect(panel,"Safe adventure card",0,0,1180,626);WorldUI.Card(content,"Adventure card",0,0,1180,626);
   title=WorldUI.Label(content,"NEXT ADVENTURE",30,22,700,32,21,true,WorldUI.Muted);
   WorldUI.Label(content,"Where to next?",28,56,750,64,44,true);
   subtitle=WorldUI.Label(content,"",30,126,1120,42,23,false,WorldUI.Muted);
   hostControls=CoveUI.Rect(content,"Host choices",0,0,1180,626).gameObject;
   again=WorldUI.Button(hostControls.transform,"Play again","Play again",868,49,282,64,()=>PlayAgain());againLabel=again.GetComponentInChildren<Text>();
   for(int i=0;i<4;i++){
    var sport=(SportId)i;var b=WorldUI.Button(hostControls.transform,"Choose "+sport,"",30+i*284,188,268,208,()=>{selected=sport;actionMessage="";});cards[i]=b;
    CoveUI.Image(b.transform,sport+" island",CoveUI.Island(sport),30,0,208,119);
    WorldUI.Label(b.transform,sport.ToString(),10,116,248,42,29,true,null,TextAnchor.MiddleCenter);
    capacities[i]=WorldUI.Label(b.transform,"",10,162,248,32,21,false,WorldUI.Muted,TextAnchor.MiddleCenter);
   }
   WorldUI.Label(hostControls.transform,"ARRIVE AT",30,416,500,25,18,true,WorldUI.Muted);
   station=WorldUI.Button(hostControls.transform,"Choose station","Skyrail station",30,452,302,64,()=>destination=TeleportDestination.Station);
   game=WorldUI.Button(hostControls.transform,"Choose game location","Game area",346,452,242,64,()=>destination=TeleportDestination.Game);
   teleport=WorldUI.Button(hostControls.transform,"Teleport everyone","Take everyone",610,452,540,64,Travel,CoveUI.Mint,28);travelLabel=teleport.GetComponentInChildren<Text>();
   status=WorldUI.Label(hostControls.transform,"",30,526,1120,38,21,false,WorldUI.Muted);
   guestControls=CoveUI.Rect(content,"Guest waiting",30,184,1120,345).gameObject;
   WorldUI.Label(guestControls.transform,"Stay for the next adventure",24,25,1072,62,36,true);
   WorldUI.Label(guestControls.transform,"Your host chooses the next island. You can keep exploring until departure.\nEveryone in the room travels together, including spectators.",24,105,1072,100,25,false,WorldUI.Muted);
   ready=WorldUI.Button(guestControls.transform,"Ready for fishing","Ready for next round",24,230,480,68,ReadyFishing,CoveUI.Mint);readyLabel=ready.GetComponentInChildren<Text>();
   party=WorldUI.Label(content,"",30,581,700,28,20,true,WorldUI.Muted);
   WorldUI.Button(content,"Keep exploring","Keep exploring",892,560,258,56,Close,size:22);
   dock=CoveUI.Rect(transform,"Waiting for next adventure",0,0,800,100);WorldUI.Card(dock,"Waiting surface",0,0,800,100);
   dockTitle=WorldUI.Label(dock,"",22,12,525,34,25,true);dockHint=WorldUI.Label(dock,"",22,51,525,32,19,false,WorldUI.Muted);
   reopen=WorldUI.Button(dock,"Open next adventure","Details",568,20,210,60,Open);
   panel.gameObject.SetActive(false);dock.gameObject.SetActive(false);
  }
  public static void CloseResults(){FishingCeremony.Instance?.Cancel();FootballMVP.Instance?.Dismiss();BasketballMVP.Instance?.Dismiss();FishingHUD.Instance?.Results?.Close();FindFirstObjectByType<GolfLeaderboardUI>()?.CloseResults();}
  bool Abandoned=>app&&app.SelectedSport==SportId.Fishing&&FishingGame.Instance&&FishingGame.Instance.State.Match.outcome==FishingOutcome.Abandoned;
  bool Voided=>app&&(app.SelectedSport==SportId.Football&&FootballMatch.Instance&&FootballMatch.Instance.Snapshot.phase==FootballMatchPhase.Cancelled||app.SelectedSport==SportId.Basketball&&BasketballMatch.Instance&&BasketballMatch.Instance.Snapshot.phase==BasketballMatchPhase.Cancelled);
  string FinishedKey(){
   if(!app||!app.Exploring)return "";
   return app.SelectedSport switch {
    SportId.Football when FootballMatch.Instance&&(FootballMatch.Instance.Snapshot.phase is FootballMatchPhase.Finished or FootballMatchPhase.Cancelled)&&!FootballMatch.Instance.GoalSequence=>"Football:"+FootballMatch.Instance.Snapshot.revision,
    SportId.Basketball when BasketballMatch.Instance&&(BasketballMatch.Instance.Snapshot.phase is BasketballMatchPhase.Finished or BasketballMatchPhase.Cancelled)=>"Basketball:"+BasketballMatch.Instance.Snapshot.revision,
    SportId.Golf when GolfMatchManager.Instance&&GolfMatchManager.Instance.State.Phase==GolfMatchPhase.Ended=>"Golf:"+GolfMatchManager.Instance.Round,
    SportId.Fishing when FishingGame.Instance&&FishingGame.Instance.State.Match.phase==FishingRoundPhase.Ended=>"Fishing:"+FishingGame.Instance.State.Match.round,
    _=>""
   };
  }
  public void Open(){
   app=AppRoot.Instance;if(!app||!app.Exploring||RoomService.Paused||RoomMatchStart.Travelling||MatchActive)return;
   CloseResults();if(FootballHUD.Instance&&FootballHUD.Instance.MenuOpen)FootballHUD.Instance.CloseMenu();if(BasketballHUD.Instance&&BasketballHUD.Instance.MenuOpen)BasketballHUD.Instance.CloseMenu();GolfHUD.Instance?.IslandMenu?.Close();FishingHUD.Instance?.IslandMenu?.Close();selected=app.SelectedSport;destination=TeleportDestination.Station;actionMessage="";
   if(!Visible){wasBlocked=app.view.ExplicitInputBlocked;app.view.ClearMatchInput();app.view.InputBlocked=true;}panel.gameObject.SetActive(true);dock.gameObject.SetActive(false);Refresh();
  }
  public void Close(){if(!Visible)return;panel.gameObject.SetActive(false);if(app&&app.view)app.view.InputBlocked=wasBlocked;}
  public bool PlayAgain(){
   app=AppRoot.Instance;if(!app||app.rooms.Connected&&!app.rooms.Host||RoomMatchStart.Travelling||MatchActive)return false;
   CloseResults();Close();bool started=false;
   switch(app.SelectedSport){
    case SportId.Football:if(app.rooms.Connected)started=FootballMatch.Instance&&FootballMatch.Instance.StartMatch();else{app.PlayAgainstAI();started=true;}break;
    case SportId.Basketball:if(app.rooms.Connected)started=BasketballMatch.Instance&&BasketballMatch.Instance.StartMatch();else{app.PlayAgainstAI();started=true;}break;
    case SportId.Golf:started=GolfMatchManager.Instance&&GolfMatchManager.Instance.StartMatch();break;
    case SportId.Fishing:ReadyFishing();started=FishingGame.Instance&&FishingGame.Instance.StartMatch(FishingGame.Instance.State.Match.mode);break;
   }
   if(!started){Open();actionMessage=RoomMatchStart.Hint(app.SelectedSport);}return started;
  }
  void ReadyFishing(){var f=FishingGame.Instance;var a=app?app.LocalAthlete:null;var p=f&&a?f.Player(a):null;if(!p.HasValue||p.Value.ready)return;var n=a.GetComponent<NetworkAthlete>();if(n&&n.IsSpawned&&!n.IsServer)n.FishingRpc(FishingAction.Ready,f.State.Match.round,p.Value.sequence);else f.Execute(a,FishingAction.Ready,f.State.Match.round,p.Value.sequence);}
  void Travel(){if(RoomTeleport.Instance.Request(selected,destination))Close();else actionMessage=RoomTeleport.Instance.Message;}
  void Update(){
   app=AppRoot.Instance;if(!app)return;current=FinishedKey();
   if(!app.Exploring||RoomService.Paused||RoomMatchStart.Travelling||MatchActive){Close();dock.gameObject.SetActive(false);if(MatchActive||!app.Exploring)shown="";return;}
   bool result=(app.SelectedSport==SportId.Golf&&FindFirstObjectByType<GolfLeaderboardUI>() is GolfLeaderboardUI ranking&&ranking.Expanded)||FishingCeremony.Presenting||FootballMVP.Presenting||BasketballMVP.Presenting||FishingHUD.Instance&&FishingHUD.Instance.Results&&FishingHUD.Instance.Results.Visible;
   bool host=!app.rooms.Connected||app.rooms.Host;
   if(current.Length>0&&shown!=current&&!result&&!app.view.InputBlocked&&!SocialPanel.Visible&&!AccountPanel.Visible){shown=current;if(host)Open();}
   bool other=app.view.InputBlocked||SocialPanel.Visible||AccountPanel.Visible;
   dock.gameObject.SetActive(!Visible&&current.Length>0&&!result&&!other);
   if(dock.gameObject.activeSelf){dockTitle.text=Abandoned?"Round abandoned · ready again":Voided?"Match ended · no winner":host?"Ready for another adventure?":"Waiting for "+WorldUI.HostName;dockHint.text=host?"Choose an island or play again.":"Keep exploring · the host will take everyone together";reopen.GetComponentInChildren<Text>().text=host?"Choose next":"Details";WorldUI.Fit(dock,(RectTransform)transform,800,100,true,142);}
   if(Visible)Refresh();
  }
  void Refresh(){
   bool host=!app.rooms.Connected||app.rooms.Host;int count=app.rooms.Connected?RoomSeats.PlayingCount:1;hostControls.SetActive(host);guestControls.SetActive(!host);
   title.text=Abandoned?"ROUND ABANDONED":Voided?"MATCH VOID · A PLAYER LEFT":"NEXT ADVENTURE";
   subtitle.text=Abandoned?"The crew changed during this round. Ready up again, or choose another island.":Voided?"No winner was awarded. Your crew can explore or start a new match.":host?"Play "+app.SelectedSport.ToString().ToLowerInvariant()+" again, or take the crew to another island.":WorldUI.HostName+" is choosing what happens next.";
   party.text=app.rooms.Connected?count+" players · "+(RoomTeleport.Players().Length-count)+" spectators · one crew":IslandBots.Instance&&IslandBots.Instance.Solo?"You + "+IslandBots.Instance.Count+" bot opponents":"Solo adventure";
   againLabel.text=app.SelectedSport==SportId.Fishing&&FishingGame.Instance&&FishingGame.Instance.State.Match.mode==FishingMode.Cup&&!FishingGame.Instance.State.Match.cupFinished?"Next cup round":"Play again";
   for(int i=0;i<4;i++){bool fits=count<=TeleportDestinations.Capacity((SportId)i);cards[i].interactable=fits;var p=cards[i].GetComponent<CovePlate>();p.borderColor=(SportId)i==selected?WorldUI.Accent(selected):WorldUI.Line;p.border=(SportId)i==selected?3:1.5f;cards[i].GetComponent<CoveFeedback>().activeBorder=p.borderColor;cards[i].GetComponent<CoveFeedback>().SetTint(WorldUI.Raised);capacities[i].text=fits?"Up to "+TeleportDestinations.Capacity((SportId)i)+" players":"Room has too many players";}
   Mark(station,destination==TeleportDestination.Station);Mark(game,destination==TeleportDestination.Game);
   string why="";bool allowed=RoomTeleport.Instance&&RoomTeleport.Instance.CanRequest(selected,destination,out why);teleport.interactable=allowed;travelLabel.text=(app.rooms.Connected?"Take everyone to ":"Visit ")+selected;
   status.text=actionMessage.Length>0?actionMessage:!allowed?why:selected+" · "+(destination==TeleportDestination.Station?"Explore from the Skyrail station":"Arrive beside the game area");
   ready.gameObject.SetActive(false);
   WorldUI.Fit(content,(RectTransform)transform,1180,626);
  }
  static void Mark(Button button,bool selected){var f=button.GetComponent<CoveFeedback>();f.activeBorder=selected?CoveUI.Mint:WorldUI.Line;button.GetComponent<CovePlate>().border=selected?3:1.5f;}
  void OnDestroy(){Close();if(Instance==this)Instance=null;}
 }
}
