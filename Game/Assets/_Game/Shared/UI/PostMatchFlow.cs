using System;
using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 [DefaultExecutionOrder(500)]
 public sealed class PostMatchFlow:MonoBehaviour {
  public static PostMatchFlow Instance {get;private set;}
  public bool Visible=>panel&&panel.gameObject.activeSelf;
  public static bool MatchActive=>FootballMatch.Instance&&FootballMatch.Instance.Active||BasketballMatch.Instance&&BasketballMatch.Instance.Active||GolfMatchManager.Instance&&GolfMatchManager.Instance.State.Running||FishingGame.Instance&&(FishingGame.Instance.State.Match.phase is FishingRoundPhase.Playing or FishingRoundPhase.Countdown);
  RectTransform panel,content;GameObject hostControls,guestControls;Button reopen,again,teleport,station,game,ready;Button[] cards=new Button[4];CovePlate[] surfaces=new CovePlate[4];Text subtitle,status,party,againLabel,travelLabel,readyLabel;Text[] capacities=new Text[4];
  AppRoot app;SportId selected;TeleportDestination destination;string shown="",current="";bool wasBlocked;string actionMessage="";
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){if(!Instance)new GameObject("Next adventure",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).AddComponent<PostMatchFlow>();}
  void Awake(){Instance=this;var canvas=GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=950;var scaler=GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;Build();}
  void Build(){
   panel=CoveUI.Rect(transform,"Next adventure overlay",0,0,1600,900);CoveUI.Stretch(panel);panel.gameObject.AddComponent<Image>().color=new Color(.02f,.10f,.16f,.66f);
   content=CoveUI.Rect(panel,"Safe adventure card",0,0,1440,720);content.anchorMin=content.anchorMax=content.pivot=Vector2.one*.5f;content.anchoredPosition=Vector2.zero;
   CoveUI.Plate(content,"Warm adventure card",0,0,1440,720,CoveUI.Cream,36,0,true);
   CoveUI.Text(content,"ROUND COMPLETE",42,26,650,34,24,true,CoveUI.Muted);
   CoveUI.Text(content,"Where to next?",38,54,800,88,50,true);
   subtitle=CoveUI.Text(content,"",42,141,1350,42,28,false,CoveUI.Muted);
   hostControls=CoveUI.Rect(content,"Host choices",0,0,1440,720).gameObject;
   again=CoveUI.Button(hostControls.transform,"Play again","Play again",1050,46,348,86,CoveUI.Gold,()=>PlayAgain(),size:32);againLabel=again.GetComponentInChildren<Text>();
   for(int i=0;i<4;i++){
    var sport=(SportId)i;float x=42+i*345;var b=CoveUI.Button(hostControls.transform,"Choose "+sport,"",x,205,321,230,CoveUI.Sky,()=>{selected=sport;actionMessage="";},size:30);cards[i]=b;surfaces[i]=b.GetComponent<CovePlate>();
    CoveUI.Image(b.transform,sport+" island",CoveUI.Island(sport),42,3,237,125);CoveUI.Text(b.transform,sport.ToString(),12,124,297,65,32,true,null,TextAnchor.MiddleCenter);capacities[i]=CoveUI.Text(b.transform,"",12,184,297,38,24,false,CoveUI.Muted,TextAnchor.MiddleCenter);
   }
   station=CoveUI.Button(hostControls.transform,"Choose station","Skyrail station",42,465,462,88,CoveUI.Cream,()=>destination=TeleportDestination.Station,size:32);
   game=CoveUI.Button(hostControls.transform,"Choose game location","Game area",522,465,462,88,CoveUI.Cream,()=>destination=TeleportDestination.Game,size:32);
   teleport=CoveUI.Button(hostControls.transform,"Teleport everyone","Teleport everyone",1002,465,396,88,CoveUI.Sky,Travel,size:30);travelLabel=teleport.GetComponentInChildren<Text>();
   status=CoveUI.Text(hostControls.transform,"",42,570,1356,46,26,false,CoveUI.Muted);
   guestControls=CoveUI.Rect(content,"Guest waiting",42,214,1356,330).gameObject;
   CoveUI.Plate(guestControls.transform,"Together card",0,0,1356,308,CoveUI.Sky,28,0);
   CoveUI.Text(guestControls.transform,"Your host is choosing the next adventure",35,35,1286,70,41,true,null,TextAnchor.MiddleCenter);
   CoveUI.Text(guestControls.transform,"Stay in the room. Everyone will travel together.",35,115,1286,50,29,false,CoveUI.Muted,TextAnchor.MiddleCenter);
   ready=CoveUI.Button(guestControls.transform,"Ready for fishing","Ready for next round",435,193,486,84,CoveUI.Mint,ReadyFishing,size:30);readyLabel=ready.GetComponentInChildren<Text>();
   party=CoveUI.Text(content,"",42,640,730,42,25,true,CoveUI.Muted);
   CoveUI.Button(content,"Keep exploring","Keep exploring",1020,628,378,66,CoveUI.Cream,Close,size:27);
   reopen=CoveUI.Button(transform,"Open next adventure","Next adventure",0,0,315,76,CoveUI.Sky,Open,size:27);var rr=(RectTransform)reopen.transform;rr.anchorMin=rr.anchorMax=new Vector2(.5f,1);rr.anchoredPosition=new Vector2(0,-65);
   panel.gameObject.SetActive(false);reopen.gameObject.SetActive(false);
  }
  public static void CloseResults(){FishingCeremony.Instance?.Cancel();FootballMVP.Instance?.Dismiss();BasketballMVP.Instance?.Dismiss();FishingHUD.Instance?.Results?.Close();}
  string FinishedKey(){
   if(!app||!app.Exploring)return "";
   return app.SelectedSport switch {
    SportId.Football when FootballMatch.Instance&&FootballMatch.Instance.Snapshot.phase==FootballMatchPhase.Finished&&!FootballMatch.Instance.GoalSequence=>"Football:"+FootballMatch.Instance.Snapshot.revision,
    SportId.Basketball when BasketballMatch.Instance&&BasketballMatch.Instance.Snapshot.phase==BasketballMatchPhase.Finished=>"Basketball:"+BasketballMatch.Instance.Snapshot.revision,
    SportId.Golf when GolfMatchManager.Instance&&GolfMatchManager.Instance.State.Phase==GolfMatchPhase.Ended=>"Golf:"+GolfMatchManager.Instance.Round,
    SportId.Fishing when FishingGame.Instance&&FishingGame.Instance.State.Match.phase==FishingRoundPhase.Ended=>"Fishing:"+FishingGame.Instance.State.Match.round,
    _=>""
   };
  }
  public void Open(){
   app=AppRoot.Instance;if(!app||!app.Exploring||TeleportSequence.Busy||RoomTeleport.Busy||MatchActive)return;
   CloseResults();selected=app.SelectedSport;destination=TeleportDestination.Station;actionMessage="";
   if(!Visible){wasBlocked=app.view.InputBlocked;app.view.ClearMatchInput();app.view.InputBlocked=true;}panel.gameObject.SetActive(true);reopen.gameObject.SetActive(false);Refresh();
  }
  public void Close(){if(!Visible)return;panel.gameObject.SetActive(false);if(app&&app.view)app.view.InputBlocked=wasBlocked;}
  public bool PlayAgain(){
   app=AppRoot.Instance;if(!app||app.rooms.Connected&&!app.rooms.Host||RoomTeleport.Busy||TeleportSequence.Busy||MatchActive)return false;
   CloseResults();Close();bool started=false;
   switch(app.SelectedSport){
    case SportId.Football: if(app.rooms.Connected)started=FootballMatch.Instance&&FootballMatch.Instance.StartMatch();else{app.PlayAgainstAI();started=true;}break;
    case SportId.Basketball:if(app.rooms.Connected)started=BasketballMatch.Instance&&BasketballMatch.Instance.StartMatch();else{app.PlayAgainstAI();started=true;}break;
    case SportId.Golf:started=GolfMatchManager.Instance&&GolfMatchManager.Instance.StartMatch();break;
    case SportId.Fishing:ReadyFishing();started=FishingGame.Instance&&FishingGame.Instance.StartMatch(FishingGame.Instance.State.Match.mode);break;
   }
   if(!started){Open();actionMessage=app.SelectedSport==SportId.Fishing?"Every player needs to be ready before the next round.":"The next match needs an even team roster. Use the match settings to add players or bots.";}return started;
  }
  void ReadyFishing(){var f=FishingGame.Instance;var a=app?app.LocalAthlete:null;var p=f&&a?f.Player(a):null;if(!p.HasValue||p.Value.ready)return;var n=a.GetComponent<NetworkAthlete>();if(n&&n.IsSpawned&&!n.IsServer)n.FishingRpc(FishingAction.Ready,f.State.Match.round,p.Value.sequence);else f.Execute(a,FishingAction.Ready,f.State.Match.round,p.Value.sequence);}
  void Travel(){if(RoomTeleport.Instance.Request(selected,destination))Close();else actionMessage=RoomTeleport.Instance.Message;}
  void Update(){
   app=AppRoot.Instance;if(!app)return;current=FinishedKey();bool busy=TeleportSequence.Busy||RoomTeleport.Busy||SkySailWorld.Instance&&SkySailWorld.Instance.Travelling;
   if(!app.Exploring||busy||MatchActive){Close();reopen.gameObject.SetActive(false);if(MatchActive||!app.Exploring)shown="";return;}
   bool result=FishingCeremony.Presenting||FootballMVP.Presenting||BasketballMVP.Presenting||FishingHUD.Instance&&FishingHUD.Instance.Results&&FishingHUD.Instance.Results.Visible;
   if(current.Length>0&&shown!=current&&!result){shown=current;Open();}
   reopen.gameObject.SetActive(!Visible&&current.Length>0&&!result);
   if(Visible)Refresh();
  }
  void Refresh(){
   bool host=!app.rooms.Connected||app.rooms.Host;int count=app.rooms.Connected?RoomSeats.PlayingCount:1;hostControls.SetActive(host);guestControls.SetActive(!host);
   subtitle.text=host?"Choose another island, or play "+app.SelectedSport.ToString().ToLowerInvariant()+" again.":"The round is over. Your place in the room is kept.";
   party.text=app.rooms.Connected?count+" PLAYERS  /  SAME ROOM":"SOLO ADVENTURE";
   againLabel.text=app.SelectedSport==SportId.Fishing&&FishingGame.Instance&&FishingGame.Instance.State.Match.mode==FishingMode.Cup&&!FishingGame.Instance.State.Match.cupFinished?"Next cup round":"Play again";
   for(int i=0;i<4;i++){bool fits=count<=TeleportDestinations.Capacity((SportId)i);cards[i].interactable=fits;cards[i].GetComponent<CoveFeedback>().SetTint((SportId)i==selected?CoveUI.Mint:fits?CoveUI.Sky:new Color(.85f,.86f,.83f));capacities[i].text=fits?"Up to "+TeleportDestinations.Capacity((SportId)i)+" players":"Your group exceeds 5";}
   station.GetComponent<CoveFeedback>().SetTint(destination==TeleportDestination.Station?CoveUI.Mint:CoveUI.Cream);game.GetComponent<CoveFeedback>().SetTint(destination==TeleportDestination.Game?CoveUI.Mint:CoveUI.Cream);
   bool allowed=RoomTeleport.Instance&&RoomTeleport.Instance.CanRequest(selected,destination,out _);teleport.interactable=allowed;travelLabel.text=app.rooms.Connected?"Teleport everyone":"Teleport";
   status.text=actionMessage.Length>0?actionMessage:selected+" · "+(destination==TeleportDestination.Station?"Beside the Skyrail station":"At the game area")+"  /  "+count+" reserved arrival "+(count==1?"space":"spaces");
   ready.gameObject.SetActive(app.SelectedSport==SportId.Fishing);if(FishingGame.Instance){var p=FishingGame.Instance.Player(app.LocalAthlete);readyLabel.text=p.HasValue&&p.Value.ready?"Ready for the next round":"Ready for next round";ready.interactable=p.HasValue&&!p.Value.ready;}
   var safe=Screen.safeArea;var root=(RectTransform)transform;content.localScale=Vector3.one*Mathf.Min(1,safe.width/Screen.width*root.rect.width/1480,safe.height/Screen.height*root.rect.height/760);content.anchoredPosition=new Vector2((safe.center.x/Screen.width-.5f)*root.rect.width,(safe.center.y/Screen.height-.5f)*root.rect.height);
  }
  void OnDestroy(){Close();if(Instance==this)Instance=null;}
 }
}
