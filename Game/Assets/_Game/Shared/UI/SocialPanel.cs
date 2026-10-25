using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static WhatTheFish.PlayerHubStyle;

namespace WhatTheFish {
 public sealed partial class SocialPanel:MonoBehaviour {
  static SocialPanel instance;
  static int closedFrame=-1;
  RectTransform design,body,list,rail;Text feedback,title,subtitle,heroName,heroCode,portraitKind;
  InputField nameField,search;CoveButton[] avatarButtons;
  RawImage preview;Texture2D draftTexture;
  string photo="",page="",query="",selectedId="",liveKey="";
  int avatar,generation;bool working,edited;float searchAt,pollAt,liveAt;bool searching;
  public static bool Visible=>instance;
  public static bool BlocksBack=>Visible||Time.frameCount==closedFrame;
  public static void OpenOwn()=>Open("edit");
  public static void OpenFriends()=>Open("friends");
  public static void OpenInvitations()=>Open("invites");
  public static void OpenRoom()=>Open("room");
  public static void OpenPlayer(string id){if(string.IsNullOrEmpty(id))return;Open(id==PlayerProfiles.Current.id?"edit":"player",id);}
  static void Open(string page,string id=""){
   if(instance){if(instance.working)return;instance.Show(page,id);return;}
   instance=new GameObject("Player profiles and friends").AddComponent<SocialPanel>();instance.Create();instance.Show(page,id);
  }
  void Create(){
   var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=30000;
   gameObject.AddComponent<GraphicRaycaster>();gameObject.AddComponent<Image>().color=Background;
   var backdrop=CoveUI.Rect(transform,"Sports menu backdrop",0,0,1600,900);CoveUI.Stretch(backdrop);backdrop.gameObject.AddComponent<SportsMenuBackground>().raycastTarget=false;
   design=CoveUI.Rect(transform,"Social safe area",0,0,1600,900);
   CoveUI.Image(design,"Official logo",Resources.Load<Sprite>("Menu/WhatTheFishLogo"),48,12,185,123);
   title=Label(design,"PLAYER HUB",310,26,1000,65,52,true);
   subtitle=Label(design,"Your identity. Your crew. Your next game.",312,94,1000,35,23,false,Muted);
   Action(design,"Close social panel","Close",1380,48,180,64,Raised,Close,CoveSymbol.Back,26);
   Card(design,"Header divider",40,145,1520,2,Line,0);
   feedback=Label(design,"",310,827,1250,58,21,false,Muted);Layout();
  }
  void Navigation(){
   if(rail){rail.gameObject.SetActive(false);Destroy(rail.gameObject);}
   rail=CoveUI.Rect(design,"Player hub navigation",40,170,240,630);
   Label(rail,"YOUR CREW",16,4,220,35,22,true,Cyan);
   Nav("My profile tab","My profile","edit",60,CoveSymbol.People);
   Nav("Friends tab","Friends","friends",150,CoveSymbol.People);
   Nav("Room invitations tab","Invitations","invites",240,CoveSymbol.Join);
   Nav("Find players tab","Find players","search",330,CoveSymbol.Join);
   Nav("Room players tab","In this room","room",420,CoveSymbol.People);
   Card(rail,"Navigation divider",16,520,208,2,Line,0);
   Label(rail,"YOUR NEXT ADVENTURE",16,547,220,60,21,true,Cyan);
  }
  void Nav(string id,string label,string destination,float y,CoveSymbol icon){
   bool active=page==destination||destination=="friends"&&(page=="incoming"||page=="outgoing"||page=="blocks"||page=="player");
   var button=Action(rail,id,label,0,y,240,72,active?Lime:Panel,()=>Show(destination),icon,26);
   if(destination=="friends")FriendRequestBadge.Add(button.transform,190,18,includeInvitations:false);
   if(destination=="invites")InvitationBadge.Add(button.transform,190,18);
  }
  void Show(string next,string id=""){
   if(working)return;DismissInvitationConfirmation();generation++;page=next;selectedId=id;searching=false;pollAt=Time.unscaledTime+25;feedback.text="";
   if(body){body.gameObject.SetActive(false);Destroy(body.gameObject);}body=CoveUI.Rect(design,"Social "+page,310,170,1250,630);
   if(draftTexture){Destroy(draftTexture);draftTexture=null;}avatarButtons=null;Navigation();
   title.text=page=="edit"?"YOUR PROFILE":page=="player"?"PLAYER PROFILE":page=="room"?"IN THIS ROOM":page=="search"?"FIND YOUR CREW":"YOUR FRIENDS";
   subtitle.text=page=="edit"?"Make it yours. Every game starts with you.":page=="search"?"Search a name, meet a teammate.":page=="room"?"The players sharing your adventure.":"";
   subtitle.gameObject.SetActive(!string.IsNullOrEmpty(subtitle.text));
   if(page=="edit"){Edit();return;}
   if(page=="invites"){Invitations();return;}
   if(page=="player"){liveKey=LiveKey(id);_ = ViewPlayer(id,generation);return;}
   if(page=="search"){
    search=Entry(body,"Search player names","Name or #player code",query,0,0,1010,64);
    Action(body,"Search players","Search",1030,0,220,70,Lime,()=>StartSearch(0),CoveSymbol.Join,27);
    Label(body,"Exact names first. Use a player code to find one specific person.",2,81,1246,35,21,false,Muted);
    search.onValueChanged.AddListener(value=>{query=value;searchAt=Time.unscaledTime+.4f;searching=true;generation++;});
    ListArea(134,496);if(query.Length>0)StartSearch(0);else Empty("YOUR NEXT TEAMMATE","Type a name or paste a player code to find your crew.");return;
   }
   if(page=="room"){ListArea(0,630);liveKey=RoomKey();Room();return;}
   Action(body,"Friends list","Friends",0,0,295,64,page=="friends"?Lime:Panel,()=>Show("friends"),size:25);
   var requests=Action(body,"Incoming requests","Requests",315,0,295,64,page=="incoming"?Lime:Panel,()=>Show("incoming"),size:25);
   FriendRequestBadge.Add(requests.transform,231,14,includeInvitations:false);
   Action(body,"Sent requests","Sent",630,0,295,64,page=="outgoing"?Lime:Panel,()=>Show("outgoing"),size:25);
   Action(body,"Blocked players","Blocked",945,0,305,64,page=="blocks"?Lime:Panel,()=>Show("blocks"),size:25);
   ListArea(88,542);_ = LoadList(generation);
  }
  void ListArea(float y,float height){
   var viewport=CoveUI.Rect(body,"Player list viewport",0,y,1250,height);viewport.gameObject.AddComponent<RectMask2D>();
   var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=45;
   list=CoveUI.Rect(viewport,"Player list",0,0,1250,0);scroll.viewport=viewport;scroll.content=list;
   var ray=viewport.gameObject.AddComponent<Image>();ray.color=Color.clear;
  }
  void ClearList(){foreach(Transform child in list){child.gameObject.SetActive(false);Destroy(child.gameObject);}list.sizeDelta=new Vector2(1250,0);list.anchoredPosition=Vector2.zero;}
  void Empty(string heading,string message){
   var c=Card(list,"Empty player list",0,0,1250,400);
   ProfileAvatar.Add(c.transform,new PublicProfile{avatar=8},32,17,234,true);
   Label(c.transform,heading,310,84,890,58,38,true);
   Label(c.transform,message,312,155,850,76,24,false,Muted);
   if(page!="search")Action(c.transform,"Find more players","Find players",312,265,290,68,Lime,()=>Show("search"),CoveSymbol.Join,27);
   list.sizeDelta=new Vector2(1250,400);
  }
  void Row(PublicProfile p,int index,bool blocked=false){
   float y=index*118;var row=Action(list,"View player "+p.code,"",0,y,1250,104,Panel,()=>OpenPlayer(p.id));
   Card(row.transform,"Avatar frame",14,10,84,84,Raised,10);ProfileAvatar.Add(row.transform,p,16,12,80);
   GameLocalization.UserContent(Label(row.transform,p.name,122,10,650,44,32,true));
   Label(row.transform,"#"+p.code,124,57,650,31,20,false,Muted);
   string state=blocked?"Blocked":p.relationship=="incoming"?"Request received":p.relationship=="outgoing"?"Request sent":p.relationship=="friends"?"Friend":p.match=="similar"?"Similar name":"View profile";
   if(blocked)Action(row.transform,"Unblock "+p.code,"Unblock",997,22,230,60,Raised,()=>Run("Removing block…",async()=>{await PlayerProfiles.Unblock(p.id);ShowAfter("blocks");}),size:24);
   else if(p.relationship=="friends"){
    var invite=Action(row.transform,"Invite friend "+p.code,"Invite",997,22,230,60,Lime,()=>SendInvitation(p),CoveSymbol.Join,24);
    invite.interactable=RoomInvitations.CanInvite(out _);Label(row.transform,"Friend",790,29,175,45,23,false,Cyan,TextAnchor.MiddleRight);
   }else{Label(row.transform,state,813,29,340,45,23,false,Muted,TextAnchor.MiddleRight);CoveUI.Icon(row.transform,CoveSymbol.Join,1182,36,30,Cyan);}
   list.sizeDelta=new Vector2(1250,(index+1)*118);
  }
  async Task LoadList(int version){
   feedback.text="Loading your crew…";
   try{
    await PlayerProfiles.EnsureCloud();PublicProfile[] rows;
    if(page=="blocks")rows=(await PlayerProfiles.Blocks()).players;
    else{var result=await PlayerProfiles.Friends();rows=page=="incoming"?result.incoming:page=="outgoing"?result.outgoing:result.friends;}
    if(!this||version!=generation)return;ClearList();for(int i=0;i<rows.Length;i++)Row(rows[i],i,page=="blocks");
    if(rows.Length==0)Empty(page=="incoming"?"ALL CAUGHT UP":page=="outgoing"?"NO REQUESTS SENT":page=="blocks"?"NO BLOCKED PLAYERS":"BUILD YOUR CREW",page=="incoming"?"New friend requests will appear here.":page=="blocks"?"Players you block will appear here.":"Find a player by name, or add someone from your room.");
    feedback.text=rows.Length==0?"":page=="friends"&&!RoomInvitations.CanInvite(out var inviteReason)?inviteReason:rows.Length+" player"+(rows.Length==1?"":"s")+" · updates every 25 seconds";
   }catch(Exception e){if(this&&version==generation)feedback.text=PlayerProfiles.Explain(e);}
  }
  async void StartSearch(int start){
   if(working||page!="search")return;searching=false;query=search.text;int version=++generation;feedback.text="Searching…";
   if(string.IsNullOrWhiteSpace(query)){ClearList();Empty("YOUR NEXT TEAMMATE","Type a name or paste a player code to find your crew.");feedback.text="";return;}
   try{
    await PlayerProfiles.EnsureCloud();var result=await PlayerProfiles.Search(query,start);
    if(!this||version!=generation)return;ClearList();for(int i=0;i<result.players.Length;i++)Row(result.players[i],i);
    int count=result.players.Length;float y=count*118;
    if(start>0)Action(list,"Previous search page","Previous",0,y,300,64,Raised,()=>StartSearch(Math.Max(0,start-20)),size:25);
    if(result.more)Action(list,"Next search page","Next results",930,y,320,64,Lime,()=>StartSearch(result.offset),size:25);
    if(start>0||result.more)list.sizeDelta=new Vector2(1250,y+80);
    if(count==0)Empty("NO MATCHES YET","Try a shorter name, or ask your friend for their player code.");
    feedback.text=count==0?"":"Exact names first · tap a player to see their profile and send a request.";
   }catch(Exception e){if(this&&version==generation)feedback.text=PlayerProfiles.Explain(e);}
  }
  void Room(){
   var players=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).Where(n=>n.IsSpawned&&n.IsPlayerObject).OrderBy(n=>n.RoomSeat.Value).ToArray();
   ClearList();int i=0;foreach(var n in players){var p=n.PublicProfile;if(!string.IsNullOrEmpty(p.id))Row(p,i++);}
   if(players.Length==0)Empty("YOUR ROOM, YOUR CREW","Join an online room to see the players travelling with you.");
   feedback.text=players.Length==0?"":i==0?"Player profiles are being verified…":"Tap a player to view their profile. Names and pictures update when saved.";
  }
  void Edit(){
   var p=PlayerProfiles.Current;avatar=p.avatar;photo=PlayerProfiles.CurrentPhoto;edited=false;
   var hero=Card(body,"Your character card",0,0,330,630);
   portraitKind=Label(hero.transform,"YOUR CHARACTER",20,14,290,34,22,true,Cyan,TextAnchor.MiddleCenter);
   preview=CoveUI.Rect(hero.transform,"Profile picture preview",20,48,290,435).gameObject.AddComponent<RawImage>();preview.raycastTarget=false;
   heroName=Label(hero.transform,p.name,20,483,290,48,33,true,null,TextAnchor.MiddleCenter);GameLocalization.UserContent(heroName);heroName.resizeTextForBestFit=true;heroName.resizeTextMinSize=22;heroName.resizeTextMaxSize=33;
   heroCode=Label(hero.transform,"",16,533,298,32,19,false,Muted,TextAnchor.MiddleCenter);
   Action(hero.transform,"Copy player code","Copy code",65,576,200,40,Raised,()=>{GUIUtility.systemCopyBuffer=PlayerProfiles.Current.code;feedback.text=string.IsNullOrEmpty(PlayerProfiles.Current.code)?"Connect to sync your player code.":"Player code copied.";},size:21);
   Card(body,"Edit profile card",354,0,896,548);
   Label(body,"DISPLAY NAME",378,12,840,35,23,true,Cyan);
   nameField=Entry(body,"Profile name","Your player name",p.name,378,54,848,PlayerIdentity.NameLimit);
   nameField.onValueChanged.AddListener(value=>{Dirty();heroName.text=value;});
   Action(body,"Camera profile picture","Take photo",378,141,270,58,Raised,()=>ChoosePhoto(true),size:24);
   Action(body,"Gallery profile picture","Choose picture",666,141,272,58,Raised,()=>ChoosePhoto(false),size:24);
   Action(body,"Remove profile picture","Use character",956,141,270,58,Raised,()=>{photo="";Dirty();Preview();},size:24);
   Label(body,"CHOOSE YOUR AVATAR",378,214,840,35,23,true);
   avatarButtons=new CoveButton[PlayerProfiles.AvatarCount];
   for(int i=0;i<PlayerProfiles.AvatarCount;i++){
    int value=i;float x=378+i%5*172,y=259+i/5*139;
    var b=Action(body,"Character avatar "+i,"",x,y,160,128,Background,()=>{avatar=value;photo="";Dirty();Preview();});avatarButtons[i]=b;
    ProfileAvatar.Add(b.transform,new PublicProfile{avatar=i},34,3,92);
    Label(b.transform,(i+1).ToString("00"),12,95,136,28,20,true,Muted,TextAnchor.MiddleRight);
   }
   for(int i=0;i<avatarButtons.Length;i++){int value=i;CosmeticLock.Bind(avatarButtons[i],"profileAvatar",i,()=>{avatar=value;photo="";Dirty();Preview();});}
   Action(body,"Save player profile","Save profile",354,568,310,62,Lime,()=>Run("Saving your profile…",async()=>{string message=await PlayerProfiles.Save(nameField.text,avatar,photo);edited=false;HeroCode();feedback.text=message;}),CoveSymbol.Check,28);
   Action(body,"Reload player profile","Reload cloud",682,568,260,62,Raised,()=>Run("Loading your cloud profile…",async()=>{await PlayerProfiles.EnsureCloud(true);ShowAfter("edit");}),size:25);
   Action(body,"Manage account","Sign-in options",960,568,290,62,Raised,()=>AccountPanel.Open(()=>{Show("edit");}),size:25);
   Preview();feedback.text=PlayerProfiles.Status+(PlayerIdentity.AccountId.Length==0?" · Link a sign-in account to recover this guest on another device.":"");_ = LoadEdit(generation);
  }
  void HeroCode(){if(heroCode)heroCode.text=string.IsNullOrEmpty(PlayerProfiles.Current.code)?"Player code syncs online":"#"+PlayerProfiles.Current.code;}
  void Dirty(){edited=true;feedback.text="Unsaved changes · tap Save profile when you're ready.";}
  async Task LoadEdit(int version){
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(PlayerEconomy.ReviewTransport!=null)return;
#endif
   try{await PlayerProfiles.EnsureCloud();if(!this||generation!=version||edited||working)return;
    var p=PlayerProfiles.Current;nameField.SetTextWithoutNotify(p.name);heroName.text=p.name;avatar=p.avatar;photo=PlayerProfiles.CurrentPhoto;Preview();feedback.text=PlayerProfiles.Status;
   }catch(Exception e){if(this&&version==generation&&!edited)feedback.text="Device profile ready. "+PlayerProfiles.Explain(e);}
  }
  void Preview(){
   if(draftTexture)Destroy(draftTexture);draftTexture=null;
   try{if(photo.Length>0)draftTexture=PlayerProfiles.DecodePhoto(photo);}catch{photo="";}
   preview.texture=draftTexture?draftTexture:PlayerProfiles.DefaultAvatar(avatar);preview.uvRect=new Rect(0,0,1,1);
   preview.rectTransform.sizeDelta=new Vector2(290,draftTexture?290:435);preview.rectTransform.anchoredPosition=new Vector2(20,-(48+(draftTexture?72.5f:0)));
   portraitKind.text=draftTexture?"YOUR PICTURE":"CHARACTER "+(avatar+1).ToString("00");HeroCode();
   if(avatarButtons==null)return;
   for(int i=0;i<avatarButtons.Length;i++){
    bool chosen=photo.Length==0&&i==avatar;var b=avatarButtons[i];var p=b.GetComponent<CovePlate>();
    p.border=chosen?3:1.5f;p.borderColor=chosen?Lime:Line;p.SetVerticesDirty();
    b.GetComponent<CoveFeedback>().activeBorder=chosen?Lime:Line;b.GetComponent<CoveFeedback>().SetTint(chosen?Raised:Background);
    var number=b.GetComponentsInChildren<Text>().Last();number.color=chosen?Lime:Muted;number.text=chosen?"SELECTED  "+(i+1).ToString("00"):(i+1).ToString("00");
   }
  }
  async void ChoosePhoto(bool camera){if(working)return;await RunTask(camera?"Opening camera…":"Opening your pictures…",async()=>{photo=await ProfilePhoto.Choose(camera);edited=true;Preview();feedback.text="Picture ready. Tap Save profile to share it with other players.";});}
  async Task ViewPlayer(string id,int version){
   feedback.text="Loading player…";
   try{
    var p=await PlayerProfiles.Get(id);if(!this||generation!=version)return;
    var hero=Card(body,"Player character card",0,0,330,630);Label(hero.transform,"PLAYER CARD",20,14,290,34,22,true,Cyan,TextAnchor.MiddleCenter);
    ProfileAvatar.Add(hero.transform,p,20,48,290,true);
    Label(hero.transform,p.hasPhoto>0?"CUSTOM PICTURE":"CHARACTER "+(p.avatar+1).ToString("00"),20,536,290,40,23,true,Muted,TextAnchor.MiddleCenter);
    Card(body,"Player details",354,0,896,630);
    Label(body,p.relationship=="friends"?"IN YOUR CREW":p.relationship=="incoming"?"FRIEND REQUEST":p.relationship=="outgoing"?"REQUEST SENT":"MEET THE PLAYER",388,27,824,38,24,true,Cyan);
    var name=Label(body,p.name,386,83,822,80,57,true);GameLocalization.UserContent(name);name.resizeTextForBestFit=true;name.resizeTextMinSize=30;name.resizeTextMaxSize=57;
    Label(body,"#"+p.code,390,176,824,35,24,false,Muted);
    Action(body,"Copy viewed player code","Copy player code",388,234,310,54,Raised,()=>{GUIUtility.systemCopyBuffer=p.code;feedback.text="Player code copied.";},CoveSymbol.Copy,23);
    Card(body,"Player details divider",388,317,828,2,Line,0);
    string description=p.relationship=="friends"?"You're on the same team. Ready for the next game?":p.relationship=="outgoing"?"Your request is on its way. This player can accept it from their Friends page.":p.relationship=="incoming"?"This player wants to join your crew.":"Send a friend request and make this player part of your crew.";
    Label(body,description,388,337,824,69,24,false,Muted);
    if(p.relationship=="incoming"){
     Action(body,"Accept friend request","Accept request",388,427,480,68,Lime,()=>FriendAction(id,"accept"),CoveSymbol.Check,28);
     Action(body,"Decline friend request","Decline",888,427,328,68,Raised,()=>FriendAction(id,"decline"),size:27);
    }else if(p.relationship=="friends"){
     var invite=Action(body,"Invite viewed friend","Invite to room",388,427,440,68,Lime,()=>SendInvitation(p),CoveSymbol.Join,28);invite.interactable=RoomInvitations.CanInvite(out _);
     Action(body,"Player friendship action","Remove friend",848,427,368,68,Raised,()=>FriendAction(id,"remove"),CoveSymbol.People,26);
    }else Action(body,"Player friendship action",p.relationship=="outgoing"?"Cancel request":"Add friend",388,427,828,68,p.relationship=="outgoing"?Raised:Lime,()=>FriendAction(id,p.relationship=="outgoing"?"cancel":"request"),CoveSymbol.People,28);
    Action(body,"Block player","Block player",388,545,260,56,Background,()=>ConfirmBlock(id),size:23);
    Action(body,"Report player name","Report name",670,545,260,56,Background,()=>Report(id,"name"),size:23);
    Action(body,"Report player photo","Report picture",952,545,264,56,Background,()=>Report(id,"photo"),size:23);
    feedback.text="A player code stays the same, even when a name changes.";
   }catch(Exception e){if(this&&generation==version)feedback.text=PlayerProfiles.Explain(e);}
  }
  void FriendAction(string id,string action)=>Run(action=="request"?"Sending friend request…":"Updating friendship…",async()=>{await PlayerProfiles.Friend(id,action);ShowAfter("player",id);});
  void Report(string id,string reason)=>Run("Sending your report…",async()=>{await PlayerProfiles.Report(id,reason);feedback.text="Report saved for the game team to review. You can also block this player.";});
  void ConfirmBlock(string id){
   feedback.text="Block this player? This removes friendship and hides each profile from the other.";
   if(body.Find("Confirm block player"))return;
   Action(body,"Confirm block player","Confirm block",388,545,828,56,Cyan,()=>Run("Blocking player…",async()=>{await PlayerProfiles.Block(id);ShowAfter("friends");}),size:26);
  }
  void ShowAfter(string next,string id=""){working=false;Show(next,id);}
  async void Run(string operation,Func<Task> action)=>await RunTask(operation,action);
  async Task RunTask(string operation,Func<Task> action){
   if(working)return;working=true;var states=design.GetComponentsInChildren<Selectable>().Select(s=>(control:s,enabled:s.interactable)).ToArray();foreach(var item in states)item.control.interactable=false;feedback.text=operation;
   try{await action();}catch(Exception e){if(this)feedback.text=PlayerProfiles.Explain(e);}
   finally{if(this){working=false;foreach(var item in states)if(item.control)item.control.interactable=item.enabled;}}
  }
  void Layout(){var safe=Screen.safeArea;float scale=Mathf.Min(safe.width/1600,safe.height/900)*.96f;design.localScale=Vector3.one*scale;design.anchoredPosition=new Vector2(safe.center.x-800*scale,-(Screen.height-safe.center.y-450*scale));}
  static string LiveKey(string id){var p=RoomTeleport.Players().FirstOrDefault(n=>n.ProfileId.Value.ToString()==id);return p?p.Nickname.Value+":"+p.ProfileAvatarVersion.Value:"";}
  static string RoomKey()=>string.Join("|",RoomTeleport.Players().OrderBy(p=>p.OwnerClientId).Select(p=>p.ProfileId.Value+":"+p.Nickname.Value+":"+p.ProfileAvatarVersion.Value));
  void Update(){
   Layout();if(AccountPanel.Visible)return;
   if(Input.GetKeyDown(KeyCode.Escape)&&!InvitationBack()&&!working)Close();
   if(searching&&Time.unscaledTime>=searchAt)StartSearch(0);
   if(!working&&page=="player"&&Time.unscaledTime>=liveAt){liveAt=Time.unscaledTime+.4f;if(liveKey!=LiveKey(selectedId))Show("player",selectedId);}
   if(!working&&page=="room"&&Time.unscaledTime>=liveAt){liveAt=Time.unscaledTime+.4f;string key=RoomKey();if(liveKey!=key){liveKey=key;Room();}}
   if(!working&&Time.unscaledTime>=pollAt){pollAt=Time.unscaledTime+25;if(page=="room")Room();else if(page=="friends"||page=="incoming"||page=="outgoing")_ = LoadList(generation);}
   UpdateInvitations();
  }
  void Close(){if(working)return;closedFrame=Time.frameCount;Destroy(gameObject);}
  void OnDestroy(){generation++;if(draftTexture)Destroy(draftTexture);if(instance==this)instance=null;}
 }
 public sealed class ProfileAvatar:MonoBehaviour {
  RawImage image;string key="";PublicProfile pending;bool requested,portrait;int layoutFrame;float retryAt,x,y,size;
  public static ProfileAvatar Add(Transform parent,PublicProfile profile,float x,float y,float size,bool fullPortrait=false){
   var r=CoveUI.Rect(parent,"Player avatar",x,y,size,size);var v=r.gameObject.AddComponent<ProfileAvatar>();v.image=r.gameObject.AddComponent<RawImage>();v.image.raycastTarget=false;
   v.x=x;v.y=y;v.size=size;v.portrait=fullPortrait;v.Set(profile);return v;
  }
  void Display(Texture2D texture,bool photo){
   image.texture=texture;image.uvRect=photo||portrait?new Rect(0,0,1,1):PlayerProfiles.AvatarCropFor(pending.avatar);
   image.rectTransform.sizeDelta=new Vector2(size,portrait&&!photo?size*1.5f:size);
   image.rectTransform.anchoredPosition=new Vector2(x,-(y+(portrait&&photo?size*.25f:0)));
  }
  public void Set(PublicProfile p){
   string next=p.id+":"+p.avatarVersion+":"+p.avatar+":"+p.hasPhoto;if(key==next&&image.texture)return;
   key=next;pending=p;requested=false;retryAt=0;layoutFrame=Time.frameCount+2;Display(PlayerProfiles.DefaultAvatar(p.avatar),false);
  }
  void Update(){
   if(pending!=null&&pending.hasPhoto>0&&!image.texture){requested=false;Display(PlayerProfiles.DefaultAvatar(pending.avatar),false);}
   if(pending==null||pending.hasPhoto==0||requested||Time.frameCount<layoutFrame||Time.unscaledTime<retryAt||image.canvasRenderer.cull)return;
   requested=true;Fetch(pending,key);
  }
  async void Fetch(PublicProfile p,string expected){
   var texture=await PlayerProfiles.Picture(p);if(!this||key!=expected)return;
   bool fallback=texture==PlayerProfiles.DefaultAvatar(p.avatar);Display(texture,!fallback);
   if(fallback){requested=false;retryAt=Time.unscaledTime+30;}
  }
 }
}
