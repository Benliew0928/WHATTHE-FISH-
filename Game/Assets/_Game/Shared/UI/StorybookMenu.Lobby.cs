using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static WhatTheFish.CoveUI;

namespace WhatTheFish {
 public sealed partial class StorybookMenu {
  sealed class RoomSeatView {
   public byte seat;public CoveButton button;public Text label,state,number;public ProfileAvatar avatar;
   public GameObject portrait,empty;public CovePlate status;
  }
  bool showSpectators;
  RectTransform playerPlaces,spectatorPlaces;
  CoveButton playersTab,spectatorsTab,spotButton;Text playersTabLabel,spectatorsTabLabel;
  Text hostNotice,hostInstruction;
  void Lobby(){
   Heading("YOUR ROOM",app.SelectedSport+"  •  "+RuleSummary());
   var board=Plate(content,"Player board",62,182,930,604,Cream,32,2,true);
   spotButton=Button(board.transform,"Change room spot","",24,20,882,94,Background,ChooseRoomSpot);
   Image(spotButton.transform,"Chosen spot art",SportArt(app.SelectedSport),12,2,106,88);
   Text(spotButton.transform,"CHOSEN SPOT",135,8,490,26,19,true,Accent(app.SelectedSport));
   Text(spotButton.transform,app.SelectedSport.ToString().ToUpperInvariant(),133,37,490,44,32,true);
   Text(spotButton.transform,app.rooms.Host?"Change spot":"Host chooses",646,29,190,36,22,true,Muted,TextAnchor.MiddleRight);
   Icon(spotButton.transform,CoveSymbol.Next,844,32,28,Muted);
   playersTab=Button(board.transform,"Show players","",24,132,426,52,Gold,()=>ShowRoster(false),size:24);playersTabLabel=playersTab.GetComponentInChildren<Text>();
   spectatorsTab=Button(board.transform,"Show spectators","",468,132,438,52,Raised,()=>ShowRoster(true),size:24);spectatorsTabLabel=spectatorsTab.GetComponentInChildren<Text>();
   playerPlaces=Rect(board.transform,"Player places",24,204,882,332);
   spectatorPlaces=Rect(board.transform,"Spectator places",24,204,882,332);
   DrawSeats(playerPlaces,false,app.environments.Current.maxPlayers);
   DrawSeats(spectatorPlaces,true,RoomSeats.Spectators);ShowRoster(showSpectators);
   spectatorPermission=Button(board.transform,"Allow spectators","",24,551,338,34,Sky,()=>{if(app.rooms.Host&&NetworkAthlete.HostPlayer)NetworkAthlete.HostPlayer.AllowSpectatorsRpc(!RoomSeats.Allowed);},size:19);
   spectatorPermissionLabel=spectatorPermission.GetComponentInChildren<Text>();
   Text(board.transform,"Tap an open place to join. Tap a player for their profile.",380,545,520,46,19,false,Muted);
   var invite=Plate(content,"Room invitation",1020,182,521,265,Cream);
   Text(invite.transform,"INVITE YOUR CREW",24,17,470,35,27,true,Gold);
   Text(invite.transform,"ROOM CODE",24,69,275,26,18,true,Muted);
   Text(invite.transform,app.rooms.Code,24,99,275,56,38,true);
   Button(invite.transform,"Copy invitation","Copy code",24,172,270,60,Raised,()=>StartCoroutine(CoveClipboard.Copy(app.rooms.Code,ok=>Notice(ok?"Room code copied. Share it with your crew.":"Copy isn't available. Share the code shown above."))),CoveSymbol.Copy,23);
   BuildRoomQr(invite.transform);
   var side=Plate(content,"Room departure",1020,465,521,321,Cream);
   Button(side.transform,"Room conditions","Conditions",24,20,226,54,Raised,()=>{returnFromRules="room";draftMinutes=MenuMatchRules.CurrentMinutes;Go("rules");},CoveSymbol.Flag,23);
   Button(side.transform,"Room venue","Customize",270,20,226,54,Raised,()=>Go("venues"),CoveSymbol.Paint,23);
   readyButton=Button(side.transform,"Ready toggle","I'm ready",24,92,472,64,Mint,ToggleReady,CoveSymbol.Check,27);readyLabel=readyButton.GetComponentInChildren<Text>();
   hostNotice=Text(side.transform,"YOU'RE THE HOST",24,95,472,32,25,true,Gold,TextAnchor.MiddleCenter);
   hostInstruction=Text(side.transform,"Start when the rest of your crew is ready.",24,130,472,28,20,false,Muted,TextAnchor.MiddleCenter);
   startHint=Text(side.transform,"",24,167,472,45,20,false,Muted,TextAnchor.MiddleCenter);
   startButton=Button(side.transform,"Travel together","Travel together",24,223,472,70,Gold,()=>{_=app.rooms.SetExploring(true);},CoveSymbol.Boat,28);
   ActionButton("Leave room","Leave room",64,809,214,61,Cream,LeaveDialog,CoveSymbol.Back,23);
   message=Text(content,"",318,807,991,57,21,false,Ink);
   Button(content,"Room help","How to play",1322,809,218,61,Cream,()=>Go("help"),CoveSymbol.Book,22);
  }
  void ShowRoster(bool spectators){
   showSpectators=spectators;playerPlaces.gameObject.SetActive(!spectators);spectatorPlaces.gameObject.SetActive(spectators);
   playersTab.GetComponent<CoveFeedback>().SetTint(spectators?Raised:Gold);playersTabLabel.color=spectators?Ink:DarkInk;
   spectatorsTab.GetComponent<CoveFeedback>().SetTint(spectators?Gold:Raised);spectatorsTabLabel.color=spectators?DarkInk:Ink;
  }
  void DrawSeats(Transform parent,bool spectator,int count){
   for(int i=0;i<count;i++){
    byte seat=(byte)((spectator?10:0)+i);string number=(i+1).ToString("00");
    var button=Button(parent,(spectator?"Spectator seat ":"Player seat ")+number,"",i%5*179,i/5*174,166,158,Background,()=>{
     var occupant=RoomTeleport.Players().FirstOrDefault(n=>n.RoomSeat.Value==seat);
     if(occupant){SocialPanel.OpenPlayer(occupant.ProfileId.Value.ToString());return;}
     var own=RoomTeleport.Players().FirstOrDefault(n=>n.IsOwner);if(own){app.rooms.SeatMessage="";own.ChooseRoomSeatRpc(seat);}
    });
    button.GetComponent<CovePlate>().radius=22;
    var label=button.GetComponentInChildren<Text>();label.rectTransform.anchoredPosition=new Vector2(8,-94);label.rectTransform.sizeDelta=new Vector2(150,30);label.fontSize=23;label.resizeTextForBestFit=true;label.resizeTextMinSize=15;label.resizeTextMaxSize=23;
    var portrait=CoveUI.Plate(button.transform,"Portrait frame",47,12,72,72,Raised,36,2);portrait.borderColor=Line;
    var mask=CoveUI.Plate(portrait.transform,"Portrait crop",4,4,64,64,Raised,32,0);mask.gameObject.AddComponent<Mask>().showMaskGraphic=true;
    var avatar=ProfileAvatar.Add(mask.transform,new PublicProfile(),0,0,64);
    var empty=Icon(button.transform,CoveSymbol.People,63,32,40,Muted);
    var num=Text(button.transform,number,10,8,30,26,18,true,Muted);
    var status=Plate(button.transform,"Place status",15,128,136,23,Raised,12,0);
    var state=Text(status.transform,"",0,0,136,23,16,true,Muted,TextAnchor.MiddleCenter);
    seatViews.Add(new RoomSeatView{seat=seat,button=button,label=label,state=state,avatar=avatar,portrait=portrait.gameObject,empty=empty.gameObject,number=num,status=status});
   }
  }
  void RefreshSeats(NetworkAthlete[] players,bool busy){
   foreach(var cell in seatViews){
    var player=players.FirstOrDefault(n=>n.RoomSeat.Value==cell.seat);bool host=player&&player==NetworkAthlete.HostPlayer;
    var accent=host?Gold:player&&player.Ready.Value?Mint:Muted;
    cell.label.text=player?player.Nickname.Value.ToString():"Open place";cell.label.color=player?Ink:Muted;
    cell.portrait.SetActive(player);cell.empty.SetActive(!player);if(player)cell.avatar.Set(player.PublicProfile);
    cell.number.text=player&&player.IsOwner?"YOU":(cell.seat%10+1).ToString("00");cell.number.fontSize=player&&player.IsOwner?15:18;cell.number.color=player&&player.IsOwner?Gold:Muted;
    cell.state.text=player?(host?"HOST":player.Ready.Value?"READY":"NOT READY"):RoomSeats.Watching(cell.seat)&&!RoomSeats.Allowed?"CLOSED":"JOIN +";
    cell.state.color=accent;cell.status.color=player?Raised:Background;
    cell.button.interactable=!busy&&(player?player.ProfileId.Value.Length>0:!RoomSeats.Watching(cell.seat)||RoomSeats.Allowed);
    var feedback=cell.button.GetComponent<CoveFeedback>();feedback.activeBorder=player&&player.IsOwner?Gold:Line;feedback.disabledBorder=feedback.activeBorder;feedback.disabledTint=Background;feedback.SetTint(Background);
   }
   if(playersTabLabel)playersTabLabel.text="PLAYERS  "+players.Count(n=>!n.IsSpectator)+" / "+app.environments.Current.maxPlayers;
   if(spectatorsTabLabel)spectatorsTabLabel.text="SPECTATORS  "+players.Count(n=>n.IsSpectator)+" / 10";
   if(spotButton)spotButton.interactable=!busy&&app.rooms.Host;
   if(hostNotice)hostNotice.gameObject.SetActive(app.rooms.Host);if(hostInstruction)hostInstruction.gameObject.SetActive(app.rooms.Host);
   if(spectatorPermission){spectatorPermission.interactable=!busy&&app.rooms.Host;spectatorPermissionLabel.text=RoomSeats.Allowed?"Spectators: open":"Spectators: closed";spectatorPermission.GetComponent<CoveFeedback>().disabledTint=Sky;spectatorPermission.GetComponent<CoveFeedback>().SetTint(Sky);}
  }
  void ToggleReady(){if(app.rooms.busy||app.rooms.Host)return;var own=RoomTeleport.Players().FirstOrDefault(p=>p.IsOwner);if(own)own.ReadyRpc(!own.Ready.Value);}
  void ChooseRoomSpot(){
   if(!app.rooms.Host||app.rooms.busy||modalRoot)return;
   var overlay=Plate(content,"Choose room spot",0,0,1600,900,new Color(.04f,.10f,.17f,.9f),0,0);overlay.raycastTarget=true;modalRoot=overlay.gameObject;group.interactable=false;
   var card=Plate(overlay.transform,"Spot picker",170,166,1260,568,Cream);card.gameObject.AddComponent<CanvasGroup>().ignoreParentGroups=true;
   Text(card.transform,"CHOOSE YOUR NEXT SPOT",32,22,1040,58,38,true);
   Text(card.transform,"Same room, same crew. Guests ready up again after a change.",34,87,1178,36,24,false,Muted);
   foreach(var sport in HomeSports){
    int i=System.Array.IndexOf(HomeSports,sport);bool current=sport==app.SelectedSport;bool fits=RoomSeats.Fits(sport);
    var choice=Button(card.transform,"Room spot "+sport,"",32+i*304,147,284,302,Background,()=>{
     if(!app.rooms.TrySetLobbySport(sport,out var reason)){Notice(reason);return;}
     Destroy(overlay.gameObject);modalRoot=null;group.interactable=true;
    });
    Image(choice.transform,"Spot art",SportArt(sport),17,7,250,185);
    Text(choice.transform,sport.ToString().ToUpperInvariant(),12,192,260,42,29,true,null,TextAnchor.MiddleCenter);
    Text(choice.transform,current?"CURRENT SPOT":fits?TeleportDestinations.Capacity(sport)+" player places":"Needs "+TeleportDestinations.Capacity(sport)+" or fewer players",10,246,264,42,21,true,current?Gold:fits?Muted:Hex("FF9CA9"),TextAnchor.MiddleCenter);
    choice.interactable=!current&&fits;
   }
   Button(card.transform,"Cancel spot change","Back to room",441,484,378,57,Raised,()=>{Destroy(overlay.gameObject);modalRoot=null;group.interactable=true;},CoveSymbol.Back,25).Select();
  }
 }
}
