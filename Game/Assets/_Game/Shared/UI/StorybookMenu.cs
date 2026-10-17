using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static WhatTheFish.CoveUI;

namespace WhatTheFish {
 public sealed partial class StorybookMenu : MonoBehaviour {
  AppRoot app;RectTransform shell,design,content;CanvasGroup group,entryGate;Coroutine transition;
  readonly Stack<string> history=new();readonly List<Button> busyButtons=new();
  readonly List<(NetworkAthlete player,Text state,CovePlate dot)> playerRows=new();
  Text message,readyLabel,startHint,roomCount,toast,headingCaption;CoveButton readyButton,startButton,joinButton;
  string restoreFocus,seatSignature;
  GameObject modalRoot;
  InputField roomCode,venueName;StadiumAppearance draft;bool hasDraft,leaving;
  int draftMinutes;string returnFromRules="create",lastError="";float refreshAt,toastUntil,busySince;
  RawImage venuePreview;Text previewTitle;RenderTexture previewTexture;Camera previewCamera;bool detailPreview;
  public string Page {get;private set;}="home";
  public bool IsTransitioning {get;private set;}
  public RectTransform Content=>content;
  public bool CanCustomize=>app.SelectedSport==SportId.Football||app.SelectedSport==SportId.Basketball;
  public bool CanEdit=>!app.rooms.Connected||app.rooms.Host;
  public void Initialize(AppRoot root,RectTransform parent) {
   app=root;MenuPreferences.Load();shell=Rect(parent,"Storybook Cove",0,0,1600,900);Stretch(shell);
   entryGate=shell.gameObject.AddComponent<CanvasGroup>();entryGate.interactable=entryGate.blocksRaycasts=!StartupWelcome.Blocking;
   var sea=Rect(shell,"Sports background",0,0,1600,900).gameObject.AddComponent<SportsMenuBackground>();Stretch(sea.rectTransform);sea.raycastTarget=true;
   design=Rect(shell,"Safe design canvas",0,0,1600,900);design.anchorMin=design.anchorMax=design.pivot=new Vector2(.5f,.5f);design.anchoredPosition=Vector2.zero;
  }
  public void Show(string page) {
   if(page=="sports")page="home";
   if(page=="custom"&&(!CanCustomize||!CanEdit))page=app.rooms.Connected?"room":"home";
   if(page=="room"&&!app.rooms.Connected)page="home";
   if(page=="ai"&&((app.SelectedSport!=SportId.Football&&app.SelectedSport!=SportId.Basketball)||app.rooms.Connected))page="home";
   if(hasDraft&&page!="custom"){app.stadium?.Apply(app.CurrentAppearance);hasDraft=false;}
   if(Page=="custom"&&page!="custom")ReleasePreview();
   restoreFocus=page==Page&&EventSystem.current&&EventSystem.current.currentSelectedGameObject?EventSystem.current.currentSelectedGameObject.name:null;
   shell.gameObject.SetActive(true);Page=page;modalRoot=null;
   if(transition!=null){StopCoroutine(transition);transition=null;}
   foreach(Transform child in design)if(child.name=="Retiring page")Destroy(child.gameObject);
   var old=content;var oldGroup=group;
   if(old){old.name="Retiring page";oldGroup.interactable=oldGroup.blocksRaycasts=false;}
   busyButtons.Clear();playerRows.Clear();message=readyLabel=startHint=roomCount=headingCaption=null;roomCode=venueName=null;
   readyButton=startButton=joinButton=null;venuePreview=null;lastError="";
   content=Rect(design,"Page - "+page,0,0,1600,900);group=content.gameObject.AddComponent<CanvasGroup>();
   switch(page){
    case "sport":SportHome();break;case "venues":VenueHub();break;case "guides":GuideHub();break;
    case "create":CreateRoom();break;case "join":JoinRoom();break;case "room":Lobby();break;
    case "rules":Rules();break;case "custom":Customize();break;case "settings":Settings();break;
    case "ai":AISetup();break;case "customize":CustomizeFootball();break;
    case "help":Help();break;case "profile":GuestProfile();break;case "character":AthleteCard();break;case "loading":Loading();break;
    default:Home();break;
   }
   var toastPlate=Plate(content,"Notice",470,814,660,56,Ink,20,0);toastPlate.gameObject.SetActive(false);
   toast=Text(toastPlate.transform,"",16,0,628,56,22,true,Cream,TextAnchor.MiddleCenter);toastUntil=0;
   EventSystem.current?.SetSelectedGameObject(null);
   group.alpha=MenuPreferences.ReducedMotion?1:0;group.interactable=group.blocksRaycasts=false;
   IsTransitioning=true;transition=StartCoroutine(Arrive(old,oldGroup));Refresh();
  }
  IEnumerator Arrive(RectTransform old,CanvasGroup previous) {
   float duration=MenuPreferences.ReducedMotion?.06f:.24f;
   for(float t=0;t<duration;t+=Time.unscaledDeltaTime){float p=Mathf.Clamp01(t/duration);float eased=1-Mathf.Pow(1-p,3);
    group.alpha=eased;content.anchoredPosition=new Vector2(MenuPreferences.ReducedMotion?0:20*(1-eased),0);
    if(previous)previous.alpha=1-p;yield return null;
   }
   if(old)Destroy(old.gameObject);group.alpha=1;content.anchoredPosition=Vector2.zero;group.interactable=group.blocksRaycasts=true;
   IsTransitioning=false;transition=null;
   if(CoveUI.KeyboardNavigation){var controls=content.GetComponentsInChildren<Selectable>();var target=controls.FirstOrDefault(s=>s.name==restoreFocus&&s.IsInteractable())??controls.FirstOrDefault(s=>s.IsInteractable());if(target)target.Select();}
  }
  public void Hide() {
   if(transition!=null)StopCoroutine(transition);transition=null;IsTransitioning=false;
   if(hasDraft){app.stadium?.Apply(app.CurrentAppearance);hasDraft=false;}ReleasePreview();
   shell.gameObject.SetActive(false);history.Clear();MenuPreferences.Save();
  }
  public void ArriveInWorld(RectTransform parent) {
   var curtain=Plate(parent,"Island arrival",0,0,1600,900,Cream,0,0);Stretch(curtain.rectTransform);curtain.raycastTarget=true;
   IsTransitioning=true;transition=StartCoroutine(RevealWorld(curtain));
  }
  IEnumerator RevealWorld(CovePlate curtain) {
   var fade=curtain.gameObject.AddComponent<CanvasGroup>();float duration=MenuPreferences.ReducedMotion?.08f:.32f;
   for(float t=0;t<duration;t+=Time.unscaledDeltaTime){fade.alpha=1-Mathf.SmoothStep(0,1,t/duration);yield return null;}
   Destroy(curtain.gameObject);IsTransitioning=false;transition=null;
  }
  void OnDestroy(){ReleasePreview();if(shell)Destroy(shell.gameObject);}
  void Update() {
   if(entryGate)entryGate.interactable=entryGate.blocksRaycasts=!StartupWelcome.Blocking;
   if(StartupWelcome.Blocking)return;
   if(!shell||!shell.gameObject.activeSelf)return;
   float scale=Mathf.Min(shell.rect.width/1600,shell.rect.height/900);design.localScale=Vector3.one*Mathf.Max(.1f,scale);
   if(Time.unscaledTime>=refreshAt){refreshAt=Time.unscaledTime+.15f;Refresh();}
   if(toast&&toast.transform.parent.gameObject.activeSelf&&Time.unscaledTime>toastUntil)toast.transform.parent.gameObject.SetActive(false);
   if(EventSystem.current&&!EventSystem.current.currentSelectedGameObject&&(Input.GetKeyDown(KeyCode.Tab)||Input.GetKeyDown(KeyCode.DownArrow))){
    CoveUI.KeyboardNavigation=true;var first=content.GetComponentsInChildren<Selectable>().FirstOrDefault(s=>s.IsInteractable());if(first)first.Select();
   }
  }
  public void Back() {
   if(app.rooms.busy||leaving)return;
   if(modalRoot){Destroy(modalRoot);modalRoot=null;group.interactable=true;return;}
   if(Page=="custom")ReadDraftName();
   if(Page=="room"){LeaveDialog();return;}
   if(Page=="custom"&&hasDraft&&!JsonUtility.ToJson(draft).Equals(JsonUtility.ToJson(app.CurrentAppearance))){DiscardDialog();return;}
   MenuPreferences.Save();GoBack();
  }
  void GoBack(){if(Page=="sport"){history.Clear();app.Show("home");return;}app.Show(history.Count>0?history.Pop():app.rooms.Connected?"room":"home");}
  void Go(string page) {if(app.rooms.busy||leaving||IsTransitioning)return;history.Push(Page);app.Show(page);}
  void Rebuild()=>app.Show(Page);
  void ChooseSport(SportId sport,string page) {if(app.rooms.Connected||app.PendingSelection.HasValue)return;if(page=="sport"){history.Clear();history.Push("home");}app.SelectSport(sport);app.Show(page);}
  void Notice(string value) {if(!toast)return;toast.text=value;toastUntil=Time.unscaledTime+3.2f;toast.transform.parent.gameObject.SetActive(true);}
  public static string NormalizeCode(string value)=>(value??"").Trim().ToUpperInvariant();
  public static bool ValidCode(string value) {value=NormalizeCode(value);return value.Length>=4&&value.Length<=12&&value.All(c=>c>='A'&&c<='Z'||c>='0'&&c<='9');}
  CoveButton ActionButton(string id,string label,float x,float y,float w,float h,Color color,Action action,CoveSymbol icon=CoveSymbol.None,int size=28) {
   var b=Button(content,id,label,x,y,w,h,color,action,icon,size);busyButtons.Add(b);return b;
  }
  void Heading(string title,string subtitle) {
   Image(content,"Official logo",Resources.Load<Sprite>("Menu/WhatTheFishLogo"),35,20,144,126);
   Text(content,title,206,25,1000,72,49,true);headingCaption=Text(content,subtitle,209,99,1050,40,24,false,Ink);
   ActionButton("Back","Back",1372,50,170,66,Cream,Back,CoveSymbol.Back,25);
  }
  void Footer(string value) {Text(content,value,150,830,1300,40,22,false,Ink,TextAnchor.MiddleCenter);}
  void CardTitle(Transform parent,string title,string caption,float y=28) {Text(parent,title,36,y,1100,49,32,true);if(!string.IsNullOrEmpty(caption))Text(parent,caption,36,y+51,1100,58,23,false,Muted);}
  void Badge(Transform parent,string text,float x,float y,float w,Color? color=null) {var p=Plate(parent,"Badge",x,y,w,39,color??Sky,18,0);Text(p.transform,text,10,0,w-20,39,19,true,Foreground(color??Sky),TextAnchor.MiddleCenter);}
  static string SportCaption(SportId sport)=>sport switch {SportId.Football=>"Team matches & kickabouts",SportId.Basketball=>"Dribble, pass & shoot",SportId.Golf=>"Five holes. Fewest strokes.",_=>"Cast, catch & compete with friends"};
  void NoticeLater(string value){StartCoroutine(DeferredNotice(value));}
  void AISetup(){
   int size=app.BotTeamSize;
   Heading(app.SelectedSport+" vs bots",MenuMatchRules.CurrentMinutes+"-minute match");
   Text(content,"Team size",204,210,1192,55,34,true,null,TextAnchor.MiddleCenter);
   for(int value=2;value<=5;value++){int selected=value;busyButtons.Add(Button(content,"AI team size "+value,value+"v"+value,204+(value-2)*304,294,280,116,size==value?Gold:Color.white,()=>{app.BotTeamSize=selected;Rebuild();},CoveSymbol.None,34));}
   Text(content,$"You + {size-1} bot teammates vs {size} bot opponents",204,440,1192,60,29,false,Ink,TextAnchor.MiddleCenter);
   busyButtons.Add(Button(content,"Start AI match","Choose team",558,554,484,104,Mint,app.PlayAgainstAI,CoveSymbol.People,33));
   ActionButton("AI match conditions","Match length",600,701,400,76,Cream,()=>{returnFromRules="ai";draftMinutes=MenuMatchRules.CurrentMinutes;Go("rules");},CoveSymbol.Flag,26);
  }
  IEnumerator DeferredNotice(string value){yield return null;Notice(value);}
  void CreateRoom() {
   Heading("CREATE A ROOM","Invite friends with a private code. Everyone travels together.");
   var p=Plate(content,"Create details",62,175,886,614,Cream,32,2,true);
   Text(p.transform,"Your sport",34,30,805,52,33,true);
   Text(p.transform,app.SelectedSport.ToString().ToUpperInvariant(),37,105,805,65,45,true,Accent(app.SelectedSport));
   Text(p.transform,"Invite-only room",37,221,550,42,29,true);Text(p.transform,"A code is created when your room is ready.",37,270,780,42,24,false,Muted);
   Badge(p.transform,"Up to "+app.environments.Current.maxPlayers+" players",37,339,241,Mint);
   Text(p.transform,"Starting conditions",37,418,700,40,28,true);
   Text(p.transform,RuleSummary(),37,464,680,52,24,false,Muted);
   ActionButton("Edit conditions","Conditions",674,584,231,65,Color.white,()=>{returnFromRules="create";draftMinutes=MenuMatchRules.CurrentMinutes;Go("rules");},CoveSymbol.Flag,23);
   var side=Plate(content,"Your island",983,175,557,614,Cream,32,2,true);
   Image(side.transform,"Island preview",SportArt(app.SelectedSport),43,15,470,324);
   Text(side.transform,app.SelectedSport.ToString(),30,322,497,60,39,true,null,TextAnchor.MiddleCenter);
   Text(side.transform,SportCaption(app.SelectedSport),27,384,503,45,24,false,Muted,TextAnchor.MiddleCenter);
   ActionButton("Create private room","Create room",1021,679,481,76,Coral,()=>{_=Connect(false);},CoveSymbol.People,30);
   message=Text(content,"",100,704,803,60,22,false,Muted);
   Footer("Your device hosts the room. Keep the game open while your friends play.");
  }
  string RuleSummary()=>app.SelectedSport switch {SportId.Football=>MenuMatchRules.CurrentMinutes+" min match  •  1 min overtime  •  2–10 players",SportId.Basketball=>MenuMatchRules.CurrentMinutes+" min match  •  20-second shot clock  •  2–10 players",SportId.Golf=>"5 holes  •  30-second finish countdown",_=>"3-minute rounds  •  Cup or Crew Catch  •  1–5 players"};
  void JoinRoom() {
   Heading("JOIN A ROOM","Ask your host for the room code, then hop aboard.");
   var p=Plate(content,"Join invitation",105,184,1390,600,Cream,35,2,true);
   Image(p.transform,"Island illustration",SportArt(app.SelectedSport),17,54,520,426);
   Text(p.transform,"Your invitation to play",585,35,745,60,40,true);
   Text(p.transform,"The host chooses the island and conditions.\nEnter their code below — letters aren't case-sensitive.",590,113,734,88,25,false,Muted);
   Text(p.transform,"ROOM CODE",590,223,720,35,20,true,Muted);
   roomCode=Field(p.transform,"Room code","e.g. WAVE24","",588,278,532,12);roomCode.contentType=InputField.ContentType.Alphanumeric;
   roomCode.onValidateInput=(text,index,c)=>c>='a'&&c<='z'?(char)(c-32):c>='A'&&c<='Z'||c>='0'&&c<='9'?c:'\0';
   roomCode.onValueChanged.AddListener(_=>{if(message)message.text="";Refresh();});
   roomCode.onEndEdit.AddListener(value=>{if(Input.GetKeyDown(KeyCode.Return)&&ValidCode(roomCode.text))_=Connect(true);});
   Button(p.transform,"Paste code","Paste",1143,278,198,70,Sky,()=>{var code=NormalizeCode(GUIUtility.systemCopyBuffer);if(!ValidCode(code)){roomCode.GetComponent<CoveFieldFeedback>().Invalid();message.text="Copy just the 4–12 letter or number code from your host.";}else{roomCode.text=code;roomCode.ActivateInputField();}},CoveSymbol.Copy,25);
   message=Text(p.transform,"",590,366,735,66,22,false,Muted);
   joinButton=Button(p.transform,"Join with code","Join room",590,463,750,84,Mint,()=>{_=Connect(true);},CoveSymbol.Join,31);busyButtons.Add(joinButton);
   Footer("Already in a game? Ask the host to return to the waiting room before you join.");
  }
  async Task Connect(bool joining) {
   if(app.rooms.busy||app.PendingSelection.HasValue)return;
   if(joining&&(!roomCode||!ValidCode(roomCode.text))){roomCode?.GetComponent<CoveFieldFeedback>().Invalid();if(message)message.text="Use the 4–12 letter or number code from your host.";return;}
   busySince=Time.unscaledTime;string value=roomCode?NormalizeCode(roomCode.text):"";
   if(joining)await app.rooms.Join(value);else await app.rooms.Create(app.SelectedSport);
   if(this)Refresh();
  }
  void Lobby() {
   Heading("Your crew is gathering",app.SelectedSport+"  •  "+RuleSummary());
   var p=Plate(content,"Player board",62,182,930,604,Cream,32,2,true);
   roomCount=Text(p.transform,"Your crew",32,23,650,52,28,true);
   Badge(p.transform,app.rooms.Host?"You're the host":"Guest",693,30,201,Gold);
   var players=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).Where(n=>n.IsSpawned&&n.IsPlayerObject).OrderBy(n=>n.OwnerClientId).ToArray();
   seatSignature=RoomSeats.Signature();
   Text(p.transform,"PLAYERS  ·  choose an open numbered place",30,92,860,36,24,true);
   DrawSeats(p.transform,players,false,app.environments.Current.maxPlayers,140);
   Text(p.transform,"SPECTATORS  ·  watch from the stands",30,330,860,36,24,true);
   DrawSeats(p.transform,players,true,10,378);
   var permission=Button(p.transform,"Allow spectators",RoomSeats.Allowed?"Spectators: allowed":"Spectators: closed",30,543,390,42,RoomSeats.Allowed?Mint:Sky,()=>NetworkAthlete.HostPlayer.AllowSpectatorsRpc(!RoomSeats.Allowed),size:21);
   permission.interactable=app.rooms.Host;
   Text(p.transform,"Click an empty place to move. Then ready up.",440,543,460,42,20,false,Muted);
   var side=Plate(content,"Room invitation",1020,182,521,604,Cream,32,2,true);
   Text(side.transform,"ROOM CODE",30,25,457,36,20,true,Muted,TextAnchor.MiddleCenter);
   Text(side.transform,app.rooms.Code,26,71,467,69,48,true,null,TextAnchor.MiddleCenter);
   Button(side.transform,"Copy invitation","Copy code",91,157,340,59,Sky,()=>StartCoroutine(CoveClipboard.Copy(app.rooms.Code,ok=>Notice(ok?"Room code copied. Share it with your crew.":"Copy isn't available. Share the code shown above."))),CoveSymbol.Copy,23);
   Button(side.transform,"Room conditions","Conditions",27,245,224,59,Color.white,()=>{returnFromRules="room";draftMinutes=MenuMatchRules.CurrentMinutes;Go("rules");},CoveSymbol.Flag,23);
   var venue=Button(side.transform,"Room venue",app.SelectedSport==SportId.Football?"Customize":"Venue",270,245,224,59,Color.white,()=>Go(app.SelectedSport==SportId.Football?"customize":"custom"),CoveSymbol.Paint,23);venue.interactable=CanCustomize&&(CanEdit||app.SelectedSport==SportId.Football);
   readyButton=Button(side.transform,"Ready toggle","I'm ready",27,327,467,74,Mint,ToggleReady,CoveSymbol.Check,28);readyLabel=readyButton.GetComponentInChildren<Text>();
   startHint=Text(side.transform,"",27,415,467,54,21,false,Muted,TextAnchor.MiddleCenter);
   startButton=Button(side.transform,"Travel together",app.rooms.Host?"Travel together":"Waiting for host",27,493,467,76,Coral,()=>{_=app.rooms.SetExploring(true);},CoveSymbol.Boat,28);
   ActionButton("Leave room","Leave room",64,809,214,61,Cream,LeaveDialog,CoveSymbol.Back,23);
   message=Text(content,"",318,807,991,57,21,false,Ink);
   Button(content,"Room help","How to play",1322,809,218,61,Cream,()=>Go("help"),CoveSymbol.Book,22);
  }
  void DrawSeats(Transform parent,NetworkAthlete[] players,bool spectator,int count,float top){
   for(int i=0;i<count;i++){
    byte seat=(byte)((spectator?10:0)+i);var player=players.FirstOrDefault(n=>n.RoomSeat.Value==seat);
    string number=(i+1).ToString("00");string title=player?player.Nickname.Value.ToString():"Open";
    if(title.Length>12)title=title.Substring(0,11)+"…";
    var button=Button(parent,(spectator?"Spectator seat ":"Player seat ")+number,number+"  "+title,30+(i%5)*175,top+(i/5)*77,165,66,player?(player.IsOwner?Gold:Mint):Color.white,()=>{
     var own=RoomTeleport.Players().FirstOrDefault(n=>n.IsOwner);if(own){app.rooms.SeatMessage="";own.ChooseRoomSeatRpc(seat);}
    },size:20);
    button.interactable=!player&&(!spectator||RoomSeats.Allowed);
    if(player){var caption=button.GetComponentInChildren<Text>();caption.rectTransform.sizeDelta=new Vector2(141,40);var state=Text(parent,"",38+(i%5)*175,top+(i/5)*77+44,149,20,14,false,Muted,TextAnchor.MiddleCenter);playerRows.Add((player,state,null));}
   }
  }
  void ToggleReady(){if(app.rooms.busy)return;var own=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).FirstOrDefault(p=>p.IsSpawned&&p.IsPlayerObject&&p.IsOwner);if(own)own.ReadyRpc(!own.Ready.Value);}
  void Rules() {
   Heading("Set the day up",CanEdit?"Choose your conditions before everyone readies up.":"The host chooses the conditions. Here is your room's setup.");
   var p=Plate(content,"Conditions",98,181,1404,605,Cream,34,2,true);
   Image(p.transform,"Sport preview",SportArt(app.SelectedSport),886,76,481,415);
   Text(p.transform,app.SelectedSport+" conditions",38,31,832,59,38,true);
   if(app.SelectedSport==SportId.Football||app.SelectedSport==SportId.Basketball){
    Text(p.transform,"Match length",40,122,804,40,27,true);
    var options=new[]{3,5,10};for(int i=0;i<3;i++){int value=options[i];var b=Button(p.transform,"Duration "+value,value+" minutes",40+i*258,181,238,75,draftMinutes==value?Gold:Color.white,()=>{draftMinutes=value;Rebuild();},CoveSymbol.None,26);b.interactable=CanEdit;}
    Detail(p.transform,"Teams","2–10 players in even teams, up to 5 v 5",40,300,788);
    Detail(p.transform,"If it's a tie","60 seconds: next score wins, then a draw. No shootouts.",40,382,788);
    Detail(p.transform,app.SelectedSport==SportId.Basketball?"Possession":"Before kickoff",app.SelectedSport==SportId.Basketball?"20-second shot clock; 2-second restarts; no free throws.":"10 seconds to pick teams; 3-second kickoff. No penalties.",40,464,788);
   }else{
    var rows=app.SelectedSport==SportId.Basketball?new[]{("Play style","Shared practice. Dribble, shoot and pass."),("Points","2 inside the arc; 3 outside the arc."),("Shooting","Hold, aim for the sweet spot, then release."),("Free roam","Use Free roam to explore without the ball.")}:app.SelectedSport==SportId.Golf?new[]{("Course","Five holes; each player progresses independently."),("Finish","First finisher starts a shared 30-second countdown."),("Ranking","Fewest strokes, then the fastest finish time."),("Ball play","You can strike any active ball; only the hitter gains a stroke.")}:new[]{("Round & Cup","Highest score after 3 minutes. Cup combines three rounds."),("Fish & bonuses","Small / medium / large: 2 / 5 / 10 points. Wanted catch: +3 once per minute."),("Crew Catch","20 points per player, all three sizes and a catch from everyone."),("Ready together","Pick 2 skills, then Ready. Host starts; shore bags and treasure offer gear.")};
    for(int i=0;i<rows.Length;i++)Detail(p.transform,rows[i].Item1,rows[i].Item2,40,126+i*97,790);
   }
   if(CanEdit&&(app.SelectedSport==SportId.Football||app.SelectedSport==SportId.Basketball)){
    ActionButton("Reset conditions","Reset",99,808,184,65,Cream,()=>{draftMinutes=3;Rebuild();});
    ActionButton("Save conditions","Save conditions",1166,808,335,65,Mint,()=>{MenuMatchRules.SetMinutes(draftMinutes);if(history.Count>0)history.Pop();app.Show(returnFromRules);NoticeLater("Conditions saved. Everyone can ready up.");},CoveSymbol.Check,26);
   }else ActionButton("Done conditions","Done",1265,808,237,65,Mint,Back,CoveSymbol.Check);
  }
  void Detail(Transform parent,string title,string value,float x,float y,float w) {Text(parent,title,x,y,w,35,25,true);Text(parent,value,x,y+35,w,49,23,false,Muted);}
  void CustomizeFootball() {
   Heading("Make the ball your own","Your trail, all game. Only you see your choice, whoever kicks the ball.");
   var panel=Plate(content,"Personal football style",63,178,1476,609,Cream,32,2,true);
   Text(panel.transform,"Choose a trail",30,16,1000,46,32,true);
   Text(panel.transform,"Saved automatically on this device",30,62,1000,35,23,false,Muted);
   var cards=Rect(panel.transform,"Trail choices",30,112,1416,467);FootballTrailPicker.Create(cards,1416,467,false);
   if(CanEdit)ActionButton("Customize shared venue","Venue design",63,809,310,65,Cream,()=>Go("custom"),CoveSymbol.Paint,26);
   ActionButton("Done customization","Done",1260,809,279,65,Mint,Back,CoveSymbol.Check,27);
  }
  void Customize() {
   if(!hasDraft){draft=app.CurrentAppearance;hasDraft=true;detailPreview=false;}
   Heading("Make it your home ground","Try a look. Preview your venue. Save when it feels right.");
   var p=Plate(content,"Venue controls",63,178,704,609,Cream,32,2,true);
   Text(p.transform,"Venue name",31,23,625,34,25,true);venueName=Field(p.transform,"Venue name","Name your venue",draft.title,30,72,642,24);
   venueName.onEndEdit.AddListener(v=>{draft.title=v;draft.Clamp();if(venueName)venueName.SetTextWithoutNotify(draft.title);PreviewDraft();});
   Text(p.transform,app.SelectedSport==SportId.Football?"Flags & screen accent":"Seats & trim",31,166,622,37,25,true);
   string[] palettes={"Mint","Rose","Lilac","Gold"};for(int i=0;i<4;i++){int choice=i;Button(p.transform,"Palette "+palettes[i],(draft.palette==i?"• ":"")+palettes[i],30+i*163,218,150,61,LocalProfile.Teams[i],()=>{ReadDraftName();draft.palette=choice;PreviewDraft();Rebuild();},CoveSymbol.None,23);}
   if(app.SelectedSport==SportId.Basketball){
    Text(p.transform,"Centre-court logo",31,306,640,36,25,true);
    string[] logos={"Rally crest","Star","Lightning","Shield"};for(int i=0;i<4;i++){int choice=i;Button(p.transform,"Logo "+logos[i],logos[i],30+(i%2)*326,363+(i/2)*83,310,65,draft.logo==i?Gold:Color.white,()=>{ReadDraftName();draft.logo=choice;PreviewDraft();Rebuild();},CoveSymbol.None,25);}
   }else{
    var messageButton=Button(p.transform,"Board design","Message: "+new[]{"Sunny days","Good energy","Move & play"}[draft.design],30,321,642,63,Color.white,()=>{ReadDraftName();draft.design=(draft.design+1)%3;PreviewDraft();Rebuild();},CoveSymbol.Next,25);messageButton.interactable=draft.screen==0;
    Button(p.transform,"Screen design","Screen: "+(draft.screen==0?"Venue title":"Team welcome"),30,407,642,63,Color.white,()=>{ReadDraftName();draft.screen=1-draft.screen;PreviewDraft();Rebuild();},CoveSymbol.Next,25);
    Button(p.transform,"Decorative flags","Flags: "+(draft.flags?"On":"Off"),30,493,642,63,draft.flags?Mint:Color.white,()=>{ReadDraftName();draft.flags=!draft.flags;PreviewDraft();Rebuild();},draft.flags?CoveSymbol.Check:CoveSymbol.Flag,25);
   }
   var side=Plate(content,"Venue preview",802,178,737,609,Cream,32,2,true);
   Text(side.transform,"Your "+app.SelectedSport.ToString().ToLowerInvariant()+" venue",31,24,680,42,30,true);
   Button(side.transform,"Preview overview","Overview",26,82,328,49,detailPreview?Color.white:Gold,()=>{ReadDraftName();detailPreview=false;Rebuild();},CoveSymbol.None,22);
   Button(side.transform,"Preview detail",app.SelectedSport==SportId.Football?"Big screen":"Centre court",374,82,337,49,detailPreview?Gold:Color.white,()=>{ReadDraftName();detailPreview=true;Rebuild();},CoveSymbol.None,22);
   var frame=Plate(side.transform,"Preview frame",26,141,685,388,Sky,8,2);venuePreview=Rect(frame.transform,"Live venue preview",4,4,677,380).gameObject.AddComponent<RawImage>();venuePreview.raycastTarget=false;
   previewTitle=Text(side.transform,draft.title,30,532,680,43,29,true,null,TextAnchor.MiddleCenter);
   Text(side.transform,app.rooms.Connected?"Saved changes are shared with every guest.":"Saved on this device and used when you host.",30,576,680,29,20,false,Muted,TextAnchor.MiddleCenter);
   ActionButton("Reset venue","Reset look",63,810,238,64,Cream,()=>{draft=new StadiumAppearance{title=app.SelectedSport==SportId.Basketball?"COURTSIDE CLUB":"SUNNY PARK",flags=true};PreviewDraft();Rebuild();},CoveSymbol.Paint,24);
   ActionButton("Save venue","Save venue",1204,809,335,66,Mint,()=>{ReadDraftName();app.SaveStadium(draft);hasDraft=false;GoBack();NoticeLater("Your venue is saved. Looking good!");},CoveSymbol.Check,27);
   PreviewDraft();
  }
  void ReadDraftName(){if(venueName)draft.title=venueName.text;draft.Clamp();}
  void PreviewDraft(){app.stadium?.Apply(draft);if(previewTitle)previewTitle.text=draft.title;StartCoroutine(CaptureVenue());}
  IEnumerator CaptureVenue() {
   yield return new WaitForEndOfFrame();if(Page!="custom"||!venuePreview||!Camera.main)yield break;
   if(!previewTexture){previewTexture=new RenderTexture(768,432,24){name="Venue preview"};previewTexture.Create();}
   if(!previewCamera){previewCamera=new GameObject("Venue preview camera").AddComponent<Camera>();previewCamera.enabled=false;}
   previewCamera.CopyFrom(Camera.main);previewCamera.enabled=false;previewCamera.transform.SetPositionAndRotation(Camera.main.transform.position,Camera.main.transform.rotation);
   if(detailPreview&&app.stadium){
    if(app.SelectedSport==SportId.Football&&app.stadium.title){var sign=app.stadium.title.transform;previewCamera.transform.position=sign.position-sign.forward*29;previewCamera.transform.LookAt(sign.position);previewCamera.fieldOfView=35;}
    else if(app.stadium is BasketballArenaView arena&&arena.logoCatalog.Length>draft.logo&&arena.logoCatalog[draft.logo]){var focus=arena.logoCatalog[draft.logo].transform.position;previewCamera.transform.position=focus+new Vector3(0,14,9);previewCamera.transform.LookAt(focus);previewCamera.fieldOfView=56;}
   }
   previewCamera.aspect=16f/9;previewCamera.targetTexture=previewTexture;previewCamera.Render();venuePreview.texture=previewTexture;
  }
  void ReleasePreview(){if(previewCamera)Destroy(previewCamera.gameObject);if(previewTexture){previewTexture.Release();Destroy(previewTexture);}previewCamera=null;previewTexture=null;}
  void GuestProfile() {
   Heading("Your player profile","One name for all four islands.");
   var p=Plate(content,"Guest profile controls",265,183,1070,605,Cream,34,2,true);
   Text(p.transform,"Guest on this device",44,28,982,50,34,true);
   Text(p.transform,"Player name",44,106,982,40,26,true);
   var field=Field(p.transform,"Profile name","Your name",PlayerIdentity.SavedName,44,163,982,PlayerIdentity.NameLimit);
   var feedback=Text(p.transform,"",44,248,982,65,24,false,Muted);
   Text(p.transform,"Your name is visible to other players. Guest profiles stay on this device and cannot be recovered after app data is cleared.\n\nHuawei account linking is coming soon.",44,326,982,150,26,false,Muted);
   var save=Button(p.transform,"Save player name","Save name",650,501,376,70,Mint,()=>{if(PlayerIdentity.SaveName(field.text,out var error)){field.SetTextWithoutNotify(PlayerIdentity.SavedName);feedback.text="Name saved.";}else feedback.text=error;},CoveSymbol.Check,27);
   void Validate(string value){save.interactable=PlayerIdentity.ValidateName(value,out _,out var error);feedback.text=error.Length>0?error:"Up to 16 characters: letters, numbers, spaces, - or _.";}
   field.onValueChanged.AddListener(Validate);Validate(field.text);
   if(app.SelectedSport==SportId.Football)ActionButton("Football ball style","Football ball style",265,810,380,65,Cream,()=>Go("customize"),CoveSymbol.Paint,25);
   ActionButton("Done profile","Done",1104,810,230,65,Mint,Back,CoveSymbol.Check);
  }
  void Settings() {
   Heading("SETTINGS","A little tuning for your kind of play.");
   var p=Plate(content,"Settings controls",97,183,1406,605,Cream,34,2,true);
   Text(p.transform,"Sound",35,25,1280,48,34,true);
   Volume(p.transform,"Game volume",MenuPreferences.MasterVolume,35,111,630,MenuPreferences.SetMaster);
   Volume(p.transform,"Menu sounds",MenuPreferences.UIVolume,737,111,630,MenuPreferences.SetUI);
   Text(p.transform,"Default camera",35,253,1280,38,28,true);
   string[] cameras={"First person","Third person","Elevated"};for(int i=0;i<3;i++){int mode=i;Button(p.transform,"Camera "+cameras[i],cameras[i],35+i*449,313,428,66,app.view.mode==i?Gold:Color.white,()=>{app.view.mode=mode;PlayerPrefs.SetInt("camera",mode);MenuPreferences.Save();Rebuild();},CoveSymbol.None,27);}
   Text(p.transform,"Gentler motion",35,421,930,39,29,true);Text(p.transform,"Keeps clear press feedback; reduces bouncing and page movement.",35,469,1006,59,24,false,Muted);
   Button(p.transform,"Reduced motion","Reduced motion: "+(MenuPreferences.ReducedMotion?"On":"Off"),1058,433,312,71,MenuPreferences.ReducedMotion?Mint:Color.white,()=>{MenuPreferences.SetMotion(!MenuPreferences.ReducedMotion);MenuPreferences.Save();Rebuild();},CoveSymbol.None,24);
   Text(p.transform,"Fishing vibration uses phone touch feedback. Reduced motion keeps it off.",35,549,1006,37,23,false,Muted);
   Button(p.transform,"Fishing vibration","Fishing vibration: "+(MenuPreferences.FishingVibration?"On":"Off"),1058,519,312,71,MenuPreferences.FishingVibration?Mint:Color.white,()=>{MenuPreferences.SetFishingVibration(!MenuPreferences.FishingVibration);MenuPreferences.Save();Rebuild();},CoveSymbol.None,23);
   ActionButton("Reset settings","Reset settings",99,809,269,63,Cream,()=>{MenuPreferences.SetMaster(.8f);MenuPreferences.SetUI(.65f);MenuPreferences.SetMotion(false);MenuPreferences.SetFishingVibration(false);app.view.mode=1;PlayerPrefs.SetInt("camera",1);MenuPreferences.Save();Rebuild();},CoveSymbol.Settings,23);
   ActionButton("Done settings","Done",1260,808,242,65,Mint,Back,CoveSymbol.Check,27);
  }
  void Volume(Transform parent,string label,float value,float x,float y,float width,Action<float> save) {
   var caption=Text(parent,label+"  "+Mathf.RoundToInt(value*100)+"%",x,y,width,43,26,true);
   var track=Plate(parent,label,x,y+66,width,50,Sky,24,1);track.raycastTarget=true;
   var slider=track.gameObject.AddComponent<Slider>();slider.direction=Slider.Direction.LeftToRight;
   var area=Rect(track.transform,"Fill area",21,18,width-42,14);var fill=Plate(area,"Fill",0,0,width-42,14,Ink,7,0);Stretch(fill.rectTransform);slider.fillRect=fill.rectTransform;
   var handleArea=Rect(track.transform,"Handle area",21,0,width-42,50);var handle=Plate(handleArea,"Handle",0,0,46,-4,Gold,23,2,true);handle.rectTransform.pivot=new Vector2(.5f,.5f);handle.rectTransform.anchoredPosition=Vector2.zero;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.value=value;
   slider.navigation=new Navigation{mode=Navigation.Mode.Automatic};slider.gameObject.AddComponent<CoveFeedback>().plate=handle;
   slider.onValueChanged.AddListener(v=>{save(v);caption.text=label+"  "+Mathf.RoundToInt(v*100)+"%";});
  }
  void Help() {
   var sport=guideFromHub?guideSport:app.SelectedSport;
   Heading(sport.ToString().ToUpperInvariant()+" · HOW TO PLAY","Controls, rules and a few tips to get started.");
   if(sport==SportId.Fishing){FishingTutorialUI.Create(content,true,Back);return;}
   var left=Plate(content,"Getting around",102,267,663,523,Cream,31,2,true);CardTitle(left.transform,"Getting around","");
   Detail(left.transform,"Move & run","Left stick / WASD. Push fully or hold Shift to run.",34,116,594);
   Detail(left.transform,"Look around","Drag on the right / hold right mouse and move.",34,242,594);
   Detail(left.transform,"Jump & camera","Jump / Space. Camera / C cycles your view.",34,368,594);
   var right=Plate(content,"Sport instructions",795,267,704,523,Cream,31,2,true);CardTitle(right.transform,sport.ToString(),"");
   var rows=sport==SportId.Football?new[]{("Kick & tackle","Hold Kick / F, then release. Drag to Cancel to stop. Tackle / E challenges for the ball."),("Play a match","In a room, travel to the pitch. Host starts with 2–10 players in even teams."),("Choose your team","Pick a team before the 10-second countdown ends.")}:sport==SportId.Basketball?new[]{("Shoot","Hold Shoot / E. Release in green; the needle loops faster farther away. Drag to Cancel / X."),("Pass & steal","Q passes. Hold Steal / F to swipe repeatedly; chase the loose ball."),("Explore freely","Switch Free roam on to walk without picking up the ball.")}:sport==SportId.Golf?new[]{("Start & swing","Start golf match. Walk near a ball and tap Aim / G. Hold F as power sweeps up and down; release when ready."),("Aim & ride","Drag right to aim along the path. Cancel / X lets you walk. R summons a cart; E drives or leaves it."),("Finish the course","Complete five holes. Fewest strokes wins; the first finisher starts a 30-second countdown.")}:Array.Empty<(string,string)>();
   for(int i=0;i<rows.Length;i++){Text(right.transform,rows[i].Item1,34,111+i*130,635,35,26,true);Text(right.transform,rows[i].Item2,34,149+i*130,635,77,23,false,Muted);}
   if(!app.rooms.Connected&&!guideFromHub)ActionButton("Try offline","Try it offline",1180,808,320,65,Mint,app.EnterOffline,CoveSymbol.Map,26);
   else if(app.rooms.Connected)Footer("Everyone stays on the same island. Ask the host when you're ready to travel.");
  }
  void AthleteCard() {
   Heading("Your Rainbow Sprinter","One bright little athlete, ready for all four islands.");
   var p=Plate(content,"Athlete information",265,221,1070,477,Cream,36,2,true);Image(p.transform,"Island postcard",SportArt(app.SelectedSport),22,20,407,380);
   Text(p.transform,"Ready for a day out",456,57,556,63,37,true);
   Text(p.transform,"Every crew member gets a distinct hairstyle and number.\n\nFootball and basketball use matching Blue or Red uniforms. Your identity stays with you across all four islands.",457,150,556,188,27,false,Muted);
   ActionButton("Done athlete","Done",1104,729,230,65,Mint,Back,CoveSymbol.Check);
  }
  void Loading() {
   var p=Plate(content,"Preparing island",420,243,760,385,Cream,38,2,true);
   var icon=Icon(p.transform,CoveSymbol.Boat,329,42,100);icon.gameObject.AddComponent<CoveFloat>().amplitude=7;
   Text(p.transform,"Preparing your island…",35,179,690,65,41,true,null,TextAnchor.MiddleCenter);
   Text(p.transform,"A little adventure is on its way.",35,271,690,53,25,false,Muted,TextAnchor.MiddleCenter);
  }
  void LeaveDialog() {if(app.rooms.busy||leaving)return;Confirm("Leave the room?",app.rooms.Host?"Your crew will disconnect when you leave.":"You can rejoin with the code while the room is open.","Leave room",async()=>{leaving=true;await app.rooms.Leave();leaving=false;history.Clear();app.Show("home");});}
  void DiscardDialog(){ReadDraftName();Confirm("Leave these changes?","Your saved venue will stay as it was.","Discard changes",()=>{hasDraft=false;app.stadium?.Apply(app.CurrentAppearance);GoBack();});}
  void Confirm(string title,string caption,string action,Action accept) {
   var overlay=Plate(content,"Confirmation",0,0,1600,900,new Color(.04f,.18f,.23f,.57f),0,0);overlay.raycastTarget=true;modalRoot=overlay.gameObject;group.interactable=false;
   var modal=Plate(overlay.transform,"Confirm card",425,279,750,331,Cream,32,2,true);Text(modal.transform,title,34,29,682,58,38,true);
   Text(modal.transform,caption,34,109,682,78,25,false,Muted);
   var modalGroup=modal.gameObject.AddComponent<CanvasGroup>();modalGroup.ignoreParentGroups=true;
   var stay=Button(modal.transform,"Keep playing","Stay",32,235,295,67,Color.white,()=>{Destroy(overlay.gameObject);modalRoot=null;group.interactable=true;},CoveSymbol.Back,26);
   Button(modal.transform,action,action,350,235,366,67,Coral,()=>{Destroy(overlay.gameObject);modalRoot=null;group.interactable=true;accept();},CoveSymbol.None,25);stay.Select();
  }
  void Refresh() {
   if(!app||!content)return;bool busy=app.rooms.busy||leaving||app.PendingSelection.HasValue;
   group.interactable=!busy&&!IsTransitioning&&!modalRoot;
   if(Page=="room"&&headingCaption)headingCaption.text=app.SelectedSport+"  •  "+RuleSummary();
   if(Page=="rules"&&!CanEdit&&draftMinutes!=MenuMatchRules.CurrentMinutes&&!IsTransitioning){draftMinutes=MenuMatchRules.CurrentMinutes;Rebuild();return;}
   if(joinButton)joinButton.interactable=!busy&&roomCode&&ValidCode(roomCode.text);
   if(readyButton||startButton){
    var players=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).Where(p=>p.IsSpawned&&p.IsPlayerObject).ToArray();
    if((players.Length!=playerRows.Count||seatSignature!=RoomSeats.Signature())&&Page=="room"&&!IsTransitioning){Rebuild();return;}
    var own=players.FirstOrDefault(p=>p.IsOwner);bool all=players.Length>0&&players.All(p=>p.Ready.Value);
    if(readyButton)readyButton.interactable=!busy&&own;if(readyLabel)readyLabel.text=own&&own.Ready.Value?"Ready!  Tap to undo":"I'm ready";
    if(startButton)startButton.interactable=!busy&&app.rooms.Host&&all&&RoomSeats.PlayingCount>0;
    if(startHint)startHint.text=busy?"Getting your crew moving…":!all?"Waiting for everyone to ready up":app.rooms.Host?"All set. Let's go together!":"All ready. Your host will start.";
    if(roomCount)roomCount.text="Players "+RoomSeats.PlayingCount+" / "+app.environments.Current.maxPlayers+"   ·   Watching "+players.Count(n=>n.IsSpectator)+" / 10";
    foreach(var row in playerRows)if(row.player&&row.state)row.state.text=(row.player.IsOwner?"You · ":row.player==NetworkAthlete.HostPlayer?"Host · ":"")+(row.player.Ready.Value?"Ready":"Choosing");
   }
   if(message){
    if(Page=="room"&&app.rooms.SeatMessage.Length>0)message.text=app.rooms.SeatMessage;
    else if(busy)message.text=Time.unscaledTime-busySince>15?"Still connecting… this is taking a little longer.":"Connecting… please wait a moment.";
    else if(!string.IsNullOrEmpty(app.rooms.Error)&&lastError!=app.rooms.Error){lastError=app.rooms.Error;message.text=FriendlyError(lastError);if(roomCode)roomCode.GetComponent<CoveFieldFeedback>().Invalid();}
    else if(lastError==""&&message.text.StartsWith("Connecting"))message.text="";
   }
  }
  static string FriendlyError(string error) {
   string lower=error.ToLowerInvariant();if(lower.Contains("full"))return "That room is full. Ask your host for another room.";
   if(lower.Contains("not found")||lower.Contains("invalid")||lower.Contains("code"))return "We couldn't find that room. Check the code and try again.";
   if(lower.Contains("started")||lower.Contains("locked"))return "Your crew is already playing. Ask the host to return to the room.";
   if(lower.Contains("host left"))return "The room could not choose another host. Please try joining again.";
   return "We couldn't connect. Check your connection, then try again. Offline exploration is available.";
  }
 }
}
