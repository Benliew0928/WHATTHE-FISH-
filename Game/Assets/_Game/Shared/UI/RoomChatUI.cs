using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 [DefaultExecutionOrder(-50)]
 public sealed partial class RoomChatUI:MonoBehaviour {
  public static RoomChatUI Instance {get;private set;}
  static int closedFrame=-1;
  public static bool Typing=>Instance&&Instance.input&&Instance.input.isFocused||Time.frameCount==closedFrame;
  public static bool BlocksInput=>Typing||Instance&&Instance.voiceOpen;
  public static bool BlocksBack=>Instance&&(Instance.open||Instance.quick||Instance.voiceOpen)||Time.frameCount==closedFrame;
  public bool Open=>open;
  public InputField MessageInput=>input;
  public RectTransform Panel=>panel;
  public RectTransform Launchers=>launchers;
  public bool Visible=>root&&root.gameObject.activeInHierarchy;
  public static Sprite Sticker(string id){int index=Array.IndexOf(RoomChatService.Stickers,id);if(index<0)return null;if(!sprites[index])sprites[index]=Resources.Load<Sprite>("RoomChat/"+id);return sprites[index];}
  static readonly Sprite[] sprites=new Sprite[8];
  AppRoot app;RoomChatService chat;Canvas canvas;RectTransform safe,root,launchers,panel,quickPanel,content,viewport,optionsPanel,fishingBoard;
  InputField input;Text title,note,unreadText;Button send;ScrollRect scroll;CanvasGroup layer,launcherOpacity;
  bool open,quick,options,dirty=true,lobby,focused;int unread,page;float nextRebuild;string draft="",status="";Vector2 previousSize;
  bool chatSpectators,localSpectator;readonly int[] audienceUnread=new int[2];Button[] chatTabs;RoomCommsIcon chatIcon;
  public bool SpectatorChat=>chatSpectators;
  static readonly Color Cream=CoveUI.Hex("FFF8E8"),Ink=CoveUI.Hex("173C47"),Mint=CoveUI.Hex("9EE3D3"),PanelColor=CoveUI.Hex("102B38"),Muted=CoveUI.Hex("BDD0D7");
  public static void Create(AppRoot app){if(Instance)return;var ui=new GameObject("Room chat",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).AddComponent<RoomChatUI>();ui.app=app;ui.chat=RoomChatService.Ensure(app.rooms);ui.Build();}
  void Build(){
   Instance=this;canvas=GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=120;
   var scaler=GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
   safe=CoveUI.Rect(transform,"Chat safe area",0,0,1600,900);root=CoveUI.Rect(safe,"Chat controls",0,0,1600,900);CoveUI.Stretch(root);layer=root.gameObject.AddComponent<CanvasGroup>();
   chat.Received+=OnMessage;chat.Changed+=Changed;BuildBubbles();Rebuild();
  }
  void OnDestroy(){if(chat){chat.Received-=OnMessage;chat.Changed-=Changed;}if(Instance==this)Instance=null;}
  void Changed(){dirty=true;}
  void OnMessage(RoomChatMessage m,bool live){if(!live||chat.IsMuted(m.member))return;if((!open||chatSpectators!=m.spectator)&&m.member!=app.rooms.MemberId)audienceUnread[m.spectator?1:0]++;ShowBubble(m);}
  bool Allowed=>app&&app.rooms.Connected&&(app.ScreenName=="room"||app.ScreenName=="stadium")&&!StartupWelcome.Blocking&&!RoomService.Paused&&!RoomTeleport.Busy&&!TeleportSequence.Busy&&!app.PendingSelection.HasValue&&!SocialPanel.Visible&&!AccountPanel.Visible&&!CoinWalletUI.Visible&&!CosmeticPurchaseUI.Visible&&!IslandTutorialUI.ModalOpen&&!TutorialSession.Active&&!ControlLayoutEditor.Active&&!FootballMVP.Presenting&&!BasketballMVP.Presenting&&!FootballReplay.Presenting&&!FishingCeremony.Presenting&&!(PostMatchFlow.Instance&&PostMatchFlow.Instance.Visible)&&!(FootballHUD.Instance&&FootballHUD.Instance.MenuOpen)&&!(BasketballHUD.Instance&&BasketballHUD.Instance.MenuOpen)&&!(GolfHUD.Instance&&GolfHUD.Instance.IslandMenu&&GolfHUD.Instance.IslandMenu.Visible)&&!(FishingHUD.Instance&&FishingHUD.Instance.BlocksGameplay);
  void Update(){
   if(!app||!root)return;
   bool allowed=Allowed&&!(app.Exploring&&app.SelectedSport==SportId.Golf&&golfBoard&&golfBoard.Expanded);
   if(!allowed&&root.gameObject.activeSelf){ReleaseInput();quick=false;voiceOpen=false;if(voicePanel)voicePanel.gameObject.SetActive(false);}
   bool golfAim=app.Exploring&&app.SelectedSport==SportId.Golf&&GolfHUD.Instance&&GolfHUD.Instance.AimFocused;
   root.gameObject.SetActive(allowed);UpdateSafeArea();RefreshVoiceFallback(allowed&&!golfAim);
   if(!app.rooms.Connected){if(open||unread>0){open=quick=options=false;draft="";unread=page=0;dirty=true;}chatSpectators=voiceSpectators=false;selectedVoiceMember="";Array.Clear(audienceUnread,0,2);ClearBubbles();}
   if(localSpectator!=chat.LocalSpectator){localSpectator=chat.LocalSpectator;SetChatAudience(localSpectator);}
   if(lobby!=!app.Exploring||previousSize!=safe.rect.size){lobby=!app.Exploring;previousSize=safe.rect.size;Rebuild();}
   if(!allowed)return;
   if(launcherOpacity){launcherOpacity.alpha=app.Exploring&&app.SelectedSport==SportId.Golf&&GolfHUD.Instance?GolfHUD.Instance.AuxiliaryOpacity:1;launcherOpacity.interactable=launcherOpacity.blocksRaycasts=!golfAim;}
   bool isFocused=input&&input.isFocused;
   if(isFocused&&!focused){app.view.ClearMatchInput();PlayerView.LookDelta=Vector2.zero;}focused=isFocused;
   if(Input.GetKeyDown(KeyCode.Escape)&&!PlayerReportUI.BlocksBack&&(open||quick||voiceOpen)){Close();return;}
   if(!golfAim&&!isFocused&&!open&&!quick&&!voiceOpen&&Input.GetKeyDown(KeyCode.Return)){SetOpen(true);input.ActivateInputField();}
   else if(isFocused&&(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.KeypadEnter))&&string.IsNullOrEmpty(UnityEngine.Input.compositionString))SendText();
   if(open&&dirty&&Time.unscaledTime>=nextRebuild){dirty=false;nextRebuild=Time.unscaledTime+.1f;RenderHistory();}
   unread=audienceUnread[0]+audienceUnread[1];
   if(unreadText){unreadText.gameObject.SetActive(unread>0);unreadText.text=unread>99?"99+":unread.ToString();}
   if(chatTabs!=null)for(int i=0;i<2;i++)chatTabs[i].GetComponentInChildren<Text>().text=(i==0?"Players":"Spectators")+(audienceUnread[i]>0?" ("+audienceUnread[i]+")":"");
   if(note){string next=chat.Cooldown>0?"Please wait…":chat.Status;if(status!=next){status=next;note.text=next;}}
   if(input){input.interactable=chatSpectators==localSpectator;((Text)input.placeholder).text=input.interactable?"Say something…":"Switch to "+(localSpectator?"Spectators":"Players")+" to send";}
   if(send)send.interactable=chatSpectators==localSpectator&&chat.Ready&&chat.Cooldown<=0&&!string.IsNullOrWhiteSpace(input.text);
   PlacePanel();RefreshVoice();
  }
  void UpdateSafeArea(){var a=Screen.safeArea;safe.anchorMin=new Vector2(a.x/Screen.width,a.y/Screen.height);safe.anchorMax=new Vector2(a.xMax/Screen.width,a.yMax/Screen.height);safe.offsetMin=safe.offsetMax=Vector2.zero;}
  public void SetOpen(bool value){open=value;quick=options=voiceOpen=false;if(open){audienceUnread[chatSpectators?1:0]=0;page=0;}else ReleaseInput();Rebuild();}
  public void SetChatAudience(bool spectators){chatSpectators=spectators;page=0;if(open)audienceUnread[spectators?1:0]=0;ReleaseInput();Rebuild();}
  public void Close(){open=quick=options=voiceOpen=false;ReleaseInput();closedFrame=Time.frameCount;Rebuild();}
  void ReleaseInput(){if(input){draft=input.text;input.DeactivateInputField();if(EventSystem.current&&EventSystem.current.currentSelectedGameObject==input.gameObject)EventSystem.current.SetSelectedGameObject(null);}focused=false;app?.view?.ClearMatchInput();}
  void SendText(){if(!input||chatSpectators!=chat.LocalSpectator)return;if(chat.SendText(input.text)){draft="";input.text="";ReleaseInput();page=0;dirty=true;closedFrame=Time.frameCount;}}
  public void SendSticker(string id){if(chatSpectators!=chat.LocalSpectator)return;if(chat.SendSticker(id)){quick=false;if(quickPanel)quickPanel.gameObject.SetActive(false);if(panel)panel.gameObject.SetActive(open);page=0;dirty=true;ReleaseInput();}}
  Button Button(Transform parent,string id,string label,float x,float y,float w,float h,Color color,Action action,int size=20){var b=PlayerHubStyle.Action(parent,id,label,x,y,w,h,color,action,size:size);foreach(var t in b.GetComponentsInChildren<Text>()){t.font=CoveUI.TextFont;t.fontStyle=FontStyle.Normal;t.color=color==Cream||color==Mint?Ink:Cream;}return b;}
  Text Label(Transform p,string text,float x,float y,float w,float h,int size,Color? color=null)=>CoveUI.Text(p,text,x,y,w,h,size,false,color??Cream);
  void Rebuild(){
   if(!root)return;if(input)draft=input.text;
   foreach(Transform child in root){child.gameObject.SetActive(false);Destroy(child.gameObject);}
   input=null;panel=null;optionsPanel=null;quickPanel=null;scroll=null;note=null;title=null;
   float w=lobby?520:560,h=lobby?600:260;
   launchers=CoveUI.Rect(root,"Chat buttons",0,0,140,64);launcherOpacity=launchers.gameObject.AddComponent<CanvasGroup>();
   var chatButton=IconButton(launchers,"Open room chat",RoomCommsSymbol.Chat,0,0,64,64,CoveUI.Hex("207B91"),()=>SetOpen(!open),out chatIcon);
   unreadText=Label(chatButton.transform,"",40,-3,30,24,13,Cream);unreadText.alignment=TextAnchor.MiddleCenter;
   BuildVoicePanel(w);
   panel=CoveUI.Plate(root,"Room chat history",0,0,w,h,PanelColor,18,1).rectTransform;panel.GetComponent<CovePlate>().borderColor=CoveUI.Hex("52747C");panel.GetComponent<CovePlate>().raycastTarget=true;
   MenuDesigns.Dialog(panel.GetComponent<CovePlate>());title=Label(panel,"CHAT",16,8,lobby?w-136:68,36,22,Mint);
   Button(panel,"Chat options","•••",w-116,7,48,48,PlayerHubStyle.Raised,()=>{options=!options;quick=false;Rebuild();});
   Button(panel,"Close chat","×",w-60,7,48,48,PlayerHubStyle.Raised,Close,28);
   chatTabs=AudienceTabs(panel,w,lobby?60:8,chatSpectators,SetChatAudience,"Chat",lobby?12:88,lobby?12:120);
   float historyTop=lobby?116:62;
   viewport=CoveUI.Rect(panel,"Message viewport",12,historyTop,w-24,h-historyTop-94);viewport.gameObject.AddComponent<RectMask2D>();
   var hit=viewport.gameObject.AddComponent<Image>();hit.color=Color.clear;hit.raycastTarget=true;
   content=CoveUI.Rect(viewport,"Messages",0,0,w-24,0);scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=40;
   input=PlayerHubStyle.Entry(panel,"Message input","Say something…",draft,12,h-80,w-132,240);
   input.GetComponent<RectTransform>().sizeDelta=new Vector2(w-132,54);
   foreach(var t in input.GetComponentsInChildren<Text>()){t.rectTransform.anchoredPosition=new Vector2(10,-4);t.rectTransform.sizeDelta=new Vector2(w-152,46);t.fontSize=20;}
   input.lineType=InputField.LineType.SingleLine;input.onValueChanged.AddListener(v=>{if(string.IsNullOrEmpty(UnityEngine.Input.compositionString)&&(System.Globalization.StringInfo.ParseCombiningCharacters(v).Length>RoomChatService.TextLimit||System.Text.Encoding.UTF8.GetByteCount(v)>480))input.SetTextWithoutNotify(RoomChatService.CleanText(v));draft=input.text;});
   input.onEndEdit.AddListener(v=>{if(TouchScreenKeyboard.isSupported&&input.touchScreenKeyboard!=null&&input.touchScreenKeyboard.status==TouchScreenKeyboard.Status.Done)SendText();});
   var fish=Button(panel,"Compose sticker","",w-112,h-80,48,54,PlayerHubStyle.Raised,()=>{if(chatSpectators!=chat.LocalSpectator)return;quick=!quick;options=false;ReleaseInput();Rebuild();});CoveUI.Image(fish.transform,"Fish",Sticker("ggwp"),4,6,40,40);fish.interactable=chatSpectators==chat.LocalSpectator;
   send=Button(panel,"Send message","↑",w-58,h-80,46,54,Mint,SendText,30);
   note=Label(panel,chat.Status,14,h-25,w-28,22,12,Muted);status="";
   panel.gameObject.SetActive(open&&!quick&&!options);
   quickPanel=MenuDesigns.Dialog(CoveUI.Plate(root,"Choose a sticker",0,0,w,258,PanelColor,18,1)).rectTransform;quickPanel.GetComponent<CovePlate>().raycastTarget=true;
   Label(quickPanel,"STICKERS",16,11,w-80,30,20,Mint);Button(quickPanel,"Close stickers","×",w-60,6,48,48,PlayerHubStyle.Raised,()=>{quick=false;quickPanel.gameObject.SetActive(false);if(panel)panel.gameObject.SetActive(open);},28);
   float cell=(w-24)/4;
   for(int i=0;i<8;i++){string id=RoomChatService.Stickers[i];var choice=Button(quickPanel,"Send "+id,"",12+i%4*cell,60+i/4*95,cell-5,92,PlayerHubStyle.Raised,()=>SendSticker(id));CoveUI.Image(choice.transform,id,Sticker(id),(cell-5-66)/2,2,66,66);Label(choice.transform,RoomChatService.StickerNames[i],2,66,cell-9,24,14).alignment=TextAnchor.MiddleCenter;}
   quickPanel.gameObject.SetActive(quick);
   if(options){optionsPanel=CoveUI.Plate(root,"Chat preferences",0,0,w,178,PanelColor,18,1).rectTransform;optionsPanel.GetComponent<CovePlate>().raycastTarget=true;Label(optionsPanel,"CHAT PREFERENCES",16,12,w-32,30,20,Mint);
    Button(optionsPanel,"Bubble preference",chat.HideBubbles?"Show speech bubbles":"Hide speech bubbles",12,50,w-24,54,PlayerHubStyle.Raised,()=>{chat.HideBubbles=!chat.HideBubbles;Rebuild();});
    Button(optionsPanel,"Close preferences","Done",12,118,w-24,45,Mint,()=>{options=false;Rebuild();});}
   MenuDesigns.Buttons(panel);MenuDesigns.Buttons(quickPanel);PlacePanel();dirty=true;if(open)RenderHistory();
  }
  GolfLeaderboardUI golfBoard;
  void PlacePanel(){
   if(!panel)return;float sw=safe.rect.width,sh=safe.rect.height;
   float dock=38;
   float launchTop=214;
   if(!lobby&&app.SelectedSport==SportId.Golf){
    if(!golfBoard&&app.HudFrame)golfBoard=app.HudFrame.GetComponentInChildren<GolfLeaderboardUI>(true);
    if(golfBoard&&golfBoard.BoardRect.gameObject.activeInHierarchy){var board=Bounds(golfBoard.BoardRect);dock=Mathf.Max(12,board.xMin);launchTop=sh-board.yMin+12;}
    else {dock=24;launchTop=20;}
    launchTop=Mathf.Clamp(launchTop,12,Mathf.Max(12,sh-76));
    launchers.gameObject.SetActive(!golfBoard||!golfBoard.Expanded);
   }else launchers.gameObject.SetActive(true);
   if(!lobby&&app.SelectedSport==SportId.Fishing){
    if(!fishingBoard&&app.HudFrame)fishingBoard=app.HudFrame.GetComponentsInChildren<RectTransform>().FirstOrDefault(r=>r.name=="Lagoon sticker leaderboard");
    if(fishingBoard){var board=Bounds(fishingBoard);dock=Mathf.Max(12,board.xMin);launchTop=sh-board.yMin+12;}
    else {dock=24;launchTop=264;}
   }
   float keyboard=TouchScreenKeyboard.visible?(TouchScreenKeyboard.area.height>0?TouchScreenKeyboard.area.height:Screen.height*.45f)/canvas.scaleFactor:0;
   if(!lobby){
    // Every expanded view shares the shallow area beneath the two launchers.
    // Keep the complete group above a native keyboard when composing on a phone.
    dock=Mathf.Clamp(dock,12,Mathf.Max(12,sw-panel.rect.width-12));
    launchTop=Mathf.Min(launchTop,Mathf.Max(12,sh-keyboard-64-10-260-12));
    launchers.anchoredPosition=new Vector2(dock,-launchTop);
    var position=new Vector2(dock,-(launchTop+74));
    panel.anchoredPosition=position;if(quickPanel)quickPanel.anchoredPosition=position;
    if(optionsPanel)optionsPanel.anchoredPosition=position;if(voicePanel)voicePanel.anchoredPosition=position;
    return;
   }
   float px=Mathf.Max(12,sw-panel.rect.width-42),py=Mathf.Min(184,Mathf.Max(12,sh-panel.rect.height-100));
   py=Mathf.Clamp(py,8,Mathf.Max(8,sh-keyboard-panel.rect.height-8));
   panel.anchoredPosition=new Vector2(px,-py);
   launchers.anchoredPosition=new Vector2(Mathf.Min(650,sw-150),-Mathf.Max(8,sh-90-keyboard));
   if(quickPanel)quickPanel.anchoredPosition=new Vector2(px,-py);
   if(optionsPanel)optionsPanel.anchoredPosition=new Vector2(px,-py);
   if(voicePanel)voicePanel.anchoredPosition=new Vector2(px,-Mathf.Clamp(py,8,Mathf.Max(8,sh-voicePanel.rect.height-8)));
  }

  void RenderHistory(){
   if(!content||!scroll)return;
   float old=scroll.verticalNormalizedPosition;bool atBottom=old<.05f;
   foreach(Transform child in content){child.gameObject.SetActive(false);Destroy(child.gameObject);}
   var history=chat.HistoryFor(chatSpectators).ToList();int end=Math.Max(0,history.Count-page*32),start=Math.Max(0,end-32);float width=content.rect.width,y=0;
   if(start>0){Button(content,"Older messages","↑ Earlier messages",0,y,width,46,PlayerHubStyle.Raised,()=>{page++;RenderHistory();scroll.verticalNormalizedPosition=1;},17);y+=54;}
   if(history.Count==0){Label(content,chat.Synced?"No messages yet":"Loading messages…",3,12,width-6,46,18,Muted);y=64;}
   for(int i=start;i<end;i++){
    var m=history[i];bool mute=chat.IsMuted(m.member);string member=m.member;
    var name=Button(content,"Sender "+i,(m.name.Length>20?m.name.Substring(0,20)+"…":m.name)+(mute?GameLocalization.Translate(" · muted"):""),0,y,width-95,36,PanelColor,()=>{chat.ToggleMute(member);RenderHistory();},16);GameLocalization.UserContent(name.GetComponentInChildren<Text>());name.GetComponentInChildren<Text>().alignment=TextAnchor.MiddleLeft;
    Label(content,DateTimeOffset.FromUnixTimeMilliseconds(m.time).ToLocalTime().ToString("HH:mm"),width-62,y,62,36,13,Muted).alignment=TextAnchor.MiddleRight;y+=35;
    if(mute){Label(content,"Message hidden",12,y,width-24,28,17,Muted);y+=36;}
    else if(!string.IsNullOrEmpty(m.sticker)){CoveUI.Image(content,m.sticker,Sticker(m.sticker),10,y,76,76);y+=82;}
    else{var text=Label(content,m.text,12,y,width-24,40,19);GameLocalization.UserContent(text);text.verticalOverflow=VerticalWrapMode.Overflow;float height=Mathf.Max(27,text.preferredHeight);text.rectTransform.sizeDelta=new Vector2(width-24,height);y+=height+10;}
    y+=8;
   }
   if(page>0){Button(content,"Newer messages","↓ Newer messages",0,y,width,46,Mint,()=>{page--;RenderHistory();scroll.verticalNormalizedPosition=1;},17);y+=54;}
   content.sizeDelta=new Vector2(width,Mathf.Max(viewport.rect.height,y));Canvas.ForceUpdateCanvases();scroll.verticalNormalizedPosition=atBottom&&page==0?0:old;
  }
 }
}
