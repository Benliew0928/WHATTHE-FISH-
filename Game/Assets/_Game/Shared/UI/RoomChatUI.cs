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
  public static bool BlocksBack=>Instance&&(Instance.open||Instance.quick)||Time.frameCount==closedFrame;
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
  void OnMessage(RoomChatMessage m,bool live){if(!live||chat.IsMuted(m.member))return;if(!open&&m.member!=app.rooms.MemberId)unread++;ShowBubble(m);}
  bool Allowed=>app&&app.rooms.Connected&&(app.ScreenName=="room"||app.ScreenName=="stadium")&&!StartupWelcome.Blocking&&!RoomService.Paused&&!RoomTeleport.Busy&&!TeleportSequence.Busy&&!app.PendingSelection.HasValue&&!SocialPanel.Visible&&!AccountPanel.Visible&&!TutorialSession.Active&&!ControlLayoutEditor.Active&&!FootballMVP.Presenting&&!BasketballMVP.Presenting&&!FootballReplay.Presenting&&!FishingCeremony.Presenting&&!(PostMatchFlow.Instance&&PostMatchFlow.Instance.Visible)&&!(FootballHUD.Instance&&FootballHUD.Instance.MenuOpen)&&!(BasketballHUD.Instance&&BasketballHUD.Instance.MenuOpen)&&!(GolfHUD.Instance&&GolfHUD.Instance.IslandMenu&&GolfHUD.Instance.IslandMenu.Visible)&&!(FishingHUD.Instance&&FishingHUD.Instance.BlocksGameplay);
  void Update(){
   if(!app||!root)return;
   bool allowed=Allowed&&!(app.Exploring&&app.SelectedSport==SportId.Golf&&golfBoard&&golfBoard.Expanded);
   if(!allowed&&root.gameObject.activeSelf){ReleaseInput();quick=false;}
   root.gameObject.SetActive(allowed);UpdateSafeArea();
   if(!app.rooms.Connected){if(open||unread>0){open=quick=options=false;draft="";unread=page=0;dirty=true;}ClearBubbles();}
   if(lobby!=!app.Exploring||previousSize!=safe.rect.size){lobby=!app.Exploring;previousSize=safe.rect.size;Rebuild();}
   if(!allowed)return;
   bool golfAim=app.Exploring&&app.SelectedSport==SportId.Golf&&GolfHUD.Instance&&GolfHUD.Instance.AimFocused;
   if(launcherOpacity){launcherOpacity.alpha=app.Exploring&&app.SelectedSport==SportId.Golf&&GolfHUD.Instance?GolfHUD.Instance.AuxiliaryOpacity:1;launcherOpacity.interactable=launcherOpacity.blocksRaycasts=!golfAim;}
   bool isFocused=input&&input.isFocused;
   if(isFocused&&!focused){app.view.ClearMatchInput();PlayerView.LookDelta=Vector2.zero;}focused=isFocused;
   if(Input.GetKeyDown(KeyCode.Escape)&&(open||quick)){Close();return;}
   if(!golfAim&&!isFocused&&!open&&!quick&&Input.GetKeyDown(KeyCode.Return)){SetOpen(true);input.ActivateInputField();}
   else if(isFocused&&(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.KeypadEnter))&&string.IsNullOrEmpty(UnityEngine.Input.compositionString))SendText();
   if(open&&dirty&&Time.unscaledTime>=nextRebuild){dirty=false;nextRebuild=Time.unscaledTime+.1f;RenderHistory();}
   if(unreadText)unreadText.text=unread>0?"Chat  "+(unread>99?"99+":unread.ToString()):"Chat";
   if(note){string next=chat.Cooldown>0?"Give your crew a moment…":chat.Status;if(status!=next){status=next;note.text=next;}}
   if(send)send.interactable=chat.Ready&&chat.Cooldown<=0&&!string.IsNullOrWhiteSpace(input.text);
   PlacePanel();
  }
  void UpdateSafeArea(){var a=Screen.safeArea;safe.anchorMin=new Vector2(a.x/Screen.width,a.y/Screen.height);safe.anchorMax=new Vector2(a.xMax/Screen.width,a.yMax/Screen.height);safe.offsetMin=safe.offsetMax=Vector2.zero;}
  public void SetOpen(bool value){open=value;quick=options=false;if(open){unread=page=0;}else ReleaseInput();Rebuild();}
  public void Close(){open=quick=options=false;ReleaseInput();closedFrame=Time.frameCount;Rebuild();}
  void ReleaseInput(){if(input){draft=input.text;input.DeactivateInputField();if(EventSystem.current&&EventSystem.current.currentSelectedGameObject==input.gameObject)EventSystem.current.SetSelectedGameObject(null);}focused=false;app?.view?.ClearMatchInput();}
  void SendText(){if(!input)return;if(chat.SendText(input.text)){draft="";input.text="";ReleaseInput();page=0;dirty=true;closedFrame=Time.frameCount;}}
  public void SendSticker(string id){if(chat.SendSticker(id)){quick=false;if(quickPanel)quickPanel.gameObject.SetActive(false);if(panel)panel.gameObject.SetActive(open);page=0;dirty=true;ReleaseInput();}}
  Button Button(Transform parent,string id,string label,float x,float y,float w,float h,Color color,Action action,int size=20){var b=PlayerHubStyle.Action(parent,id,label,x,y,w,h,color,action,size:size);foreach(var t in b.GetComponentsInChildren<Text>()){t.font=CoveUI.TextFont;t.fontStyle=FontStyle.Normal;t.color=color==Cream||color==Mint?Ink:Cream;}return b;}
  Text Label(Transform p,string text,float x,float y,float w,float h,int size,Color? color=null)=>CoveUI.Text(p,text,x,y,w,h,size,false,color??Cream);
  void Rebuild(){
   if(!root)return;if(input)draft=input.text;
   foreach(Transform child in root){child.gameObject.SetActive(false);Destroy(child.gameObject);}
   input=null;panel=null;optionsPanel=null;quickPanel=null;scroll=null;note=null;title=null;
   float w=lobby?520:380,h=lobby?600:332;
   launchers=CoveUI.Rect(root,"Chat buttons",0,0,210,58);launcherOpacity=launchers.gameObject.AddComponent<CanvasGroup>();
   var chatButton=Button(launchers,"Open room chat","Chat",0,0,130,58,Cream,()=>SetOpen(!open),23);unreadText=chatButton.GetComponentInChildren<Text>();
   var stickerButton=Button(launchers,"Quick stickers","",140,0,60,58,Cream,()=>{quick=!quick;options=false;ReleaseInput();Rebuild();});CoveUI.Image(stickerButton.transform,"Stickers",Sticker("laugh"),8,6,44,44);
   panel=CoveUI.Plate(root,"Room chat history",0,0,w,h,PanelColor,18,1).rectTransform;panel.GetComponent<CovePlate>().borderColor=CoveUI.Hex("52747C");panel.GetComponent<CovePlate>().raycastTarget=true;
   title=Label(panel,"ROOM CHAT",16,5,w-136,36,22,Mint);Label(panel,"One crew · every island",16,39,w-130,22,15,Muted);
   Button(panel,"Chat options","•••",w-116,7,48,48,PlayerHubStyle.Raised,()=>{options=!options;quick=false;Rebuild();});
   Button(panel,"Close chat","×",w-60,7,48,48,PlayerHubStyle.Raised,Close,28);
   viewport=CoveUI.Rect(panel,"Message viewport",12,69,w-24,h-173);viewport.gameObject.AddComponent<RectMask2D>();
   var hit=viewport.gameObject.AddComponent<Image>();hit.color=Color.clear;hit.raycastTarget=true;
   content=CoveUI.Rect(viewport,"Messages",0,0,w-24,0);scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=40;
   input=PlayerHubStyle.Entry(panel,"Message input","Say something…",draft,12,h-96,w-132,240);
   input.GetComponent<RectTransform>().sizeDelta=new Vector2(w-132,54);
   foreach(var t in input.GetComponentsInChildren<Text>()){t.rectTransform.anchoredPosition=new Vector2(10,-4);t.rectTransform.sizeDelta=new Vector2(w-152,46);t.fontSize=20;}
   input.lineType=InputField.LineType.SingleLine;input.onValueChanged.AddListener(v=>{if(string.IsNullOrEmpty(UnityEngine.Input.compositionString)&&(System.Globalization.StringInfo.ParseCombiningCharacters(v).Length>RoomChatService.TextLimit||System.Text.Encoding.UTF8.GetByteCount(v)>480))input.SetTextWithoutNotify(RoomChatService.CleanText(v));draft=input.text;});
   input.onEndEdit.AddListener(v=>{if(TouchScreenKeyboard.isSupported&&input.touchScreenKeyboard!=null&&input.touchScreenKeyboard.status==TouchScreenKeyboard.Status.Done)SendText();});
   var fish=Button(panel,"Compose sticker","",w-112,h-96,48,54,PlayerHubStyle.Raised,()=>{quick=!quick;options=false;ReleaseInput();Rebuild();});CoveUI.Image(fish.transform,"Fish",Sticker("ggwp"),4,6,40,40);
   send=Button(panel,"Send message","↑",w-58,h-96,46,54,Mint,SendText,30);
   note=Label(panel,chat.Status,14,h-35,w-28,25,13,Muted);status="";
   panel.gameObject.SetActive(open&&!quick&&!options);
   quickPanel=CoveUI.Plate(root,"Choose a free sticker",0,0,w,280,PanelColor,18,1).rectTransform;quickPanel.GetComponent<CovePlate>().raycastTarget=true;
   Label(quickPanel,"SEND A LITTLE FISH",16,11,w-80,30,19,Mint);Button(quickPanel,"Close stickers","×",w-60,6,48,48,PlayerHubStyle.Raised,()=>{quick=false;quickPanel.gameObject.SetActive(false);if(panel)panel.gameObject.SetActive(open);},28);
   float cell=(w-24)/4;
   for(int i=0;i<8;i++){string id=RoomChatService.Stickers[i];var choice=Button(quickPanel,"Send "+id,"",12+i%4*cell,60+i/4*95,cell-5,92,PlayerHubStyle.Raised,()=>SendSticker(id));CoveUI.Image(choice.transform,id,Sticker(id),(cell-5-66)/2,2,66,66);Label(choice.transform,RoomChatService.StickerNames[i],2,66,cell-9,24,14).alignment=TextAnchor.MiddleCenter;}
   Label(quickPanel,"All stickers are free",16,250,w-32,22,14,Muted).alignment=TextAnchor.MiddleCenter;quickPanel.gameObject.SetActive(quick);
   if(options){optionsPanel=CoveUI.Plate(root,"Chat preferences",0,0,w,242,PanelColor,18,1).rectTransform;optionsPanel.GetComponent<CovePlate>().raycastTarget=true;Label(optionsPanel,"CHAT PREFERENCES",16,12,w-32,30,20,Mint);
    Button(optionsPanel,"Bubble preference",chat.HideBubbles?"Show speech bubbles":"Hide speech bubbles",12,50,w-24,54,PlayerHubStyle.Raised,()=>{chat.HideBubbles=!chat.HideBubbles;Rebuild();});
    Label(optionsPanel,"Tap a sender's name in history to mute\nor unmute them. History stays in this room.",16,114,w-32,68,17,Muted);
    Button(optionsPanel,"Close preferences","Done",12,185,w-24,45,Mint,()=>{options=false;Rebuild();});}
   PlacePanel();dirty=true;if(open)RenderHistory();
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
    dock=fishingBoard?Bounds(fishingBoard).xMax+18:404;
    dock=Mathf.Clamp(dock,38,sw-panel.rect.width-12);
   }
   float px=lobby?Mathf.Max(12,sw-panel.rect.width-42):dock,py=lobby?Mathf.Min(184,Mathf.Max(12,sh-panel.rect.height-100)):launchTop+73;
   // The native keyboard supplies the actual occluded pixels. Input remains
   // reachable even on split-screen devices and phones with a large IME.
   float keyboard=TouchScreenKeyboard.visible?(TouchScreenKeyboard.area.height>0?TouchScreenKeyboard.area.height:Screen.height*.45f)/canvas.scaleFactor:0;
   py=Mathf.Clamp(py,8,Mathf.Max(8,sh-keyboard-panel.rect.height-8));
   panel.anchoredPosition=new Vector2(px,-py);
   launchers.anchoredPosition=lobby?new Vector2(Mathf.Min(650,sw-230),-Mathf.Max(8,sh-90-keyboard)):new Vector2(dock,-launchTop);
   if(quickPanel)quickPanel.anchoredPosition=open?new Vector2(px,-py):lobby?new Vector2(Mathf.Min(650,sw-quickPanel.rect.width-12),-Mathf.Max(8,sh-quickPanel.rect.height-110-keyboard)):new Vector2(dock,-Mathf.Min(launchTop+73,Mathf.Max(12,sh-quickPanel.rect.height-12)));
   if(optionsPanel)optionsPanel.anchoredPosition=new Vector2(px,-py);
  }
  void RenderHistory(){
   if(!content||!scroll)return;
   float old=scroll.verticalNormalizedPosition;bool atBottom=old<.05f;
   foreach(Transform child in content){child.gameObject.SetActive(false);Destroy(child.gameObject);}
   var history=chat.History;int end=Math.Max(0,history.Count-page*32),start=Math.Max(0,end-32);float width=content.rect.width,y=0;
   if(start>0){Button(content,"Older messages","↑ Earlier messages",0,y,width,46,PlayerHubStyle.Raised,()=>{page++;RenderHistory();scroll.verticalNormalizedPosition=1;},17);y+=54;}
   if(history.Count==0){Label(content,chat.Synced?"Your crew's conversation starts here.\nSay hello or send a sticker.":"Loading this room's conversation…",3,12,width-6,70,18,Muted);y=90;}
   for(int i=start;i<end;i++){
    var m=history[i];bool mute=chat.IsMuted(m.member);string member=m.member;
    var name=Button(content,"Sender "+i,(m.name.Length>20?m.name.Substring(0,20)+"…":m.name)+(mute?" · muted":""),0,y,width-95,36,PanelColor,()=>{chat.ToggleMute(member);RenderHistory();},16);name.GetComponentInChildren<Text>().alignment=TextAnchor.MiddleLeft;
    Label(content,DateTimeOffset.FromUnixTimeMilliseconds(m.time).ToLocalTime().ToString("HH:mm"),width-62,y,62,36,13,Muted).alignment=TextAnchor.MiddleRight;y+=35;
    Label(content,m.context,12,y,width-20,18,12,Muted);y+=23;
    if(mute){Label(content,"Message hidden",12,y,width-24,28,17,Muted);y+=36;}
    else if(!string.IsNullOrEmpty(m.sticker)){CoveUI.Image(content,m.sticker,Sticker(m.sticker),10,y,76,76);y+=82;}
    else{var text=Label(content,m.text,12,y,width-24,40,19);text.verticalOverflow=VerticalWrapMode.Overflow;float height=Mathf.Max(27,text.preferredHeight);text.rectTransform.sizeDelta=new Vector2(width-24,height);y+=height+10;}
    y+=8;
   }
   if(page>0){Button(content,"Newer messages","↓ Newer messages",0,y,width,46,Mint,()=>{page--;RenderHistory();scroll.verticalNormalizedPosition=1;},17);y+=54;}
   content.sizeDelta=new Vector2(width,Mathf.Max(viewport.rect.height,y));Canvas.ForceUpdateCanvases();scroll.verticalNormalizedPosition=atBottom&&page==0?0:old;
  }
 }
}
