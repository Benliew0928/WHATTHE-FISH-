using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SportsPrototype {
 public sealed class AppRoot:MonoBehaviour {
  public static AppRoot Instance; public SportEnvironmentController environments;public StadiumView stadium=>environments.View;public SportId SelectedSport=>environments.Selected;public StadiumAppearance CurrentAppearance=>LocalProfile.ForSport(SelectedSport);public PlayerView view;public RoomService rooms;
  public Athlete LocalAthlete;public bool Exploring {get;private set;}
  public GameObject athletePrefab; Athlete offline; Canvas canvas;RectTransform safe,page;Font font;Sprite rounded;Text status,roster,fps;float rosterTimer;string screen="home",appliedWorld="";bool lastExploring; InputField code;
  readonly Color ink=LocalProfile.Hex("173834"),mint=LocalProfile.Hex("BFEBCB"),cream=LocalProfile.Hex("FFF9E9");
  void Awake(){Instance=this;Application.targetFrameRate=Application.isMobilePlatform?30:60;Screen.sleepTimeout=SleepTimeout.NeverSleep;}
  void Start(){
   font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");rounded=MakeRound();
   canvas=new GameObject("Mobile UI",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
   var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
   safe=new GameObject("Safe area",typeof(RectTransform)).GetComponent<RectTransform>();safe.SetParent(canvas.transform,false);ApplySafeArea();
   if(!FindFirstObjectByType<EventSystem>())new GameObject("Input",typeof(EventSystem),typeof(StandaloneInputModule));
   rooms.SetupNetwork();rooms.Changed+=OnRoomChanged;
   offline=Instantiate(athletePrefab,new Vector3(0,.07f,-8),Quaternion.identity).GetComponent<Athlete>();offline.name="Offline athlete";offline.Setup();offline.Appearance(LocalProfile.Character);LocalAthlete=offline;
   environments.Activate(SportId.Football);stadium.Apply(CurrentAppearance);Show("home");
   var args=Environment.GetCommandLineArgs();int sportArg=Array.IndexOf(args,"-sport");if(sportArg>=0&&sportArg+1<args.Length&&Enum.TryParse<SportId>(args[sportArg+1],true,out var requested)&&environments.Definition(requested)&&environments.Definition(requested).available)SelectSport(requested);if(args.Contains("-offline"))EnterOffline();
   if(args.Contains("-localHost")||args.Contains("-localClient")){ushort port=7777;int ix=Array.IndexOf(args,"-port");if(ix>=0&&ix+1<args.Length)ushort.TryParse(args[ix+1],out port);offline.gameObject.SetActive(false);rooms.StartLocal(args.Contains("-localHost"),port);Show("room");}
  }
  void ApplySafeArea(){var a=Screen.safeArea;safe.anchorMin=new Vector2(a.x/Screen.width,a.y/Screen.height);safe.anchorMax=new Vector2(a.xMax/Screen.width,a.yMax/Screen.height);safe.offsetMin=safe.offsetMax=Vector2.zero;}
  void OnRoomChanged(){if(!rooms.Connected){appliedWorld="";stadium.Apply(CurrentAppearance);}if(rooms.Connected){offline.gameObject.SetActive(false);if(screen!="stadium"&&screen!="character")Show("room");}else if(screen=="room"||screen=="stadium"&&lastExploring){Exploring=false;lastExploring=false;LocalAthlete=offline;Show("sport");}UpdateStatus();}
  void Update(){
   if(!Exploring){if(screen=="character"){view.transform.position=offline.transform.position+new Vector3(1.4f,1.25f,3.7f);view.transform.LookAt(offline.transform.position+new Vector3(1.4f,1.1f,0));}else {view.transform.position=environments.Current.menuCamera;view.transform.LookAt(environments.Current.menuFocus);}}
   if(!rooms.Connected&&lastExploring){Exploring=false;lastExploring=false;LocalAthlete=offline;Show("sport");}
   if(rooms.Connected&&NetworkAthlete.HostPlayer){
    var world=NetworkAthlete.HostPlayer;if(SelectedSport!=world.WorldSport.Value){SelectSport(world.WorldSport.Value,true);if(screen=="room")Show("room");}string json=world.WorldAppearance.Value.ToString();if(json.Length>0&&json!=appliedWorld){appliedWorld=json;stadium.Apply(JsonUtility.FromJson<StadiumAppearance>(json));}
    if(world.Exploring.Value!=lastExploring){lastExploring=world.Exploring.Value;Exploring=lastExploring;Show(Exploring?"stadium":"room");}
   }
   view.target=LocalAthlete;view.active=Exploring;
   if(Exploring&&!rooms.Connected&&LocalAthlete)LocalAthlete.Simulate(view.ReadCommand(),Time.deltaTime);
   if(!Exploring&&LocalAthlete)LocalAthlete.HideHead(false);
   if((rosterTimer-=Time.deltaTime)<0){rosterTimer=.5f;RefreshRoster();if(fps)fps.text=$"{Mathf.RoundToInt(1/Mathf.Max(Time.smoothDeltaTime,.001f))} FPS   •   {(rooms.Connected?"ONLINE":"OFFLINE")}";}
   if(Input.GetKeyDown(KeyCode.Escape)){if(Exploring)Return();else Show(rooms.Connected?"room":"home");}
  }
  void UpdateStatus(){if(status)status.text=rooms.busy?"Connecting…":rooms.Error??"";}
  public void SelectSport(SportId sport,bool fromHost=false){if(rooms.Connected&&!fromHost)return;environments.Activate(sport);appliedWorld="";stadium.Apply(CurrentAppearance);if(offline&&!rooms.Connected)ResetOfflineSpawn();view.yaw=0;view.pitch=16;PlayerView.LookDelta=Vector2.zero;}
  void ResetOfflineSpawn(){offline.capsule.enabled=false;offline.transform.SetPositionAndRotation(environments.Current.Spawn(0),Quaternion.identity);offline.capsule.enabled=true;offline.ResetLocomotion();}
  public void EnterOffline(){Exploring=true;lastExploring=false;offline.gameObject.SetActive(true);LocalAthlete=offline;ResetOfflineSpawn();stadium.Apply(CurrentAppearance);Show("stadium");}
  async void Return(){if(rooms.Connected){if(rooms.Host)await rooms.SetExploring(false);else {await rooms.Leave();Exploring=false;Show("sport");}}else{Exploring=false;Show("sport");}}
  void Clear(){PlayerView.LookDelta=Vector2.zero;if(page)Destroy(page.gameObject);page=new GameObject("Page",typeof(RectTransform)).GetComponent<RectTransform>();page.SetParent(safe,false);page.anchorMin=Vector2.zero;page.anchorMax=Vector2.one;page.offsetMin=page.offsetMax=Vector2.zero;roster=null;fps=null;status=null;}
  public void Show(string which){if(which=="custom"&&SelectedSport==SportId.Golf)which=rooms.Connected?"room":"sport";if(which=="custom"&&rooms.Connected&&!rooms.Host)which="room";screen=which;Clear();if(which=="stadium"){HUD();return;}
   Panel(page,new Vector2(330,450),new Vector2(590,824),new Color(1,.98f,.93f,.96f));
   Label(page,"SPORTS CLUB  /  PROTOTYPE",58,new Vector2(82,822),new Vector2(510,35),16,ink);
   if(which=="home"){
    Label(page,"Good days.\nGreat games.",0,new Vector2(82,694),new Vector2(510,160),62,ink);
    Label(page,"Your little escape to play and explore.",0,new Vector2(82,575),new Vector2(475,70),25,ink);
    Button("Let's play",new Vector2(330,454),()=>Show("sports"),mint);
    Button("Your athlete",new Vector2(330,358),()=>Show("character"),Color.white);
    Button("Settings & controls",new Vector2(330,262),()=>Show("settings"),Color.white);
    Label(page,"THREE HOME GROUNDS\nA shared place to move, meet and explore.",0,new Vector2(82,128),new Vector2(485,90),21,ink);
    Pill(CurrentAppearance.title.ToUpperInvariant(),new Vector2(1300,104),new Vector2(380,65));
   } else if(which=="sports"){
    Heading("Pick your sport","Three places. Plenty of possibilities.");
    Button("Football  /  Explore stadium",new Vector2(330,542),()=>{SelectSport(SportId.Football);Show("sport");},mint);
    Button("Basketball  /  Explore arena",new Vector2(330,440),()=>{SelectSport(SportId.Basketball);Show("sport");},mint);
    Button("Golf  /  Explore island",new Vector2(330,338),()=>{SelectSport(SportId.Golf);Show("sport");},mint);Back("home");
   } else if(which=="sport"){
    Heading(SelectedSport.ToString(),SelectedSport switch{SportId.Basketball=>"Your indoor home court. Take a look around.",SportId.Golf=>"An open island. Sea on every side.",_=>"The pitch is yours. Take a look around."});
    Button("Explore offline",new Vector2(330,568),EnterOffline,mint);
    Button("Create internet room",new Vector2(330,468),async()=>{await rooms.Create(SelectedSport);UpdateStatus();},Color.white);
    code=Field("Room code",new Vector2(235,367),new Vector2(305,76),"",12);
    Button("Join",new Vector2(490,367),async()=>{await rooms.Join(code.text);UpdateStatus();},mint,new Vector2(160,76));
    if(SelectedSport!=SportId.Golf)Button("Stadium customisation",new Vector2(330,267),()=>Show("custom"),Color.white);else Label(page,"420 m of open land.\nAbout one minute to sprint across.",0,new Vector2(82,265),new Vector2(490,100),23,ink);
    status=Label(page,"",0,new Vector2(82,160),new Vector2(500,100),19,ink);UpdateStatus();Back("sports");
   } else if(which=="character"){
    offline.gameObject.SetActive(true);Heading("Rainbow Sprinter","Our shared athlete for this art phase.");
    Label(page,"One bright cartoon look across all three sports.\nOutfit and appearance choices return when the\ncharacter has separate, swappable parts.",0,new Vector2(82,434),new Vector2(510,168),25,ink);
    Button("Done",new Vector2(330,138),()=>{offline.gameObject.SetActive(!rooms.Connected);Show(rooms.Connected?"room":"home");},Color.white);
   } else if(which=="settings"){
    Heading("Make it comfy","Landscape play · 30 FPS target");
    Label(page,"MOVE\nLeft thumb stick / WASD\n\nLOOK\nDrag on the right / Hold right mouse\n\nCAMERA\nTap Camera / Press C\n\nSPRINT\nPush stick fully / Hold Shift",0,new Vector2(82,390),new Vector2(490,365),25,ink);
    Button("Default camera: "+new[]{"First person","Third person","Elevated"}[view.mode],new Vector2(330,177),()=>{view.Switch();Show("settings");},mint);Back("home");
   } else if(which=="custom"){
    Heading("Your home ground","Saved on this device. Shared by the host.");var a=CurrentAppearance;
    var name=Field("Stadium name",new Vector2(330,583),new Vector2(490,70),a.title,24);name.onEndEdit.AddListener(v=>{var s=CurrentAppearance;s.title=v;SaveStadium(s);});
    Button("Seats & trim: "+new[]{"Mint","Rose","Lilac","Gold"}[a.palette],new Vector2(330,492),()=>{var s=CurrentAppearance;s.palette=(s.palette+1)%4;SaveStadium(s);Show("custom");},LocalProfile.Teams[a.palette]);
    if(SelectedSport==SportId.Basketball){
     Button("Centre logo: "+BasketballArenaView.LogoNames[a.logo],new Vector2(330,401),()=>{var s=CurrentAppearance;s.logo=(s.logo+1)%4;SaveStadium(s);Show("custom");},Color.white);
     Button("Reset logo & colour",new Vector2(330,310),()=>{var s=CurrentAppearance;s.logo=0;s.palette=0;SaveStadium(s);Show("custom");},Color.white);
     Label(page,"Preview on the court →\nLogo and accent colour are shared with guests.",0,new Vector2(82,230),new Vector2(490,90),22,ink);
    }else{
    Button("Boards: "+new[]{"Sunny days","Good energy","Move & play"}[a.design],new Vector2(330,401),()=>{var s=CurrentAppearance;s.design=(s.design+1)%3;SaveStadium(s);Show("custom");},Color.white);
    Button("Big screen: "+(a.screen==0?"Stadium title":"Team welcome"),new Vector2(330,310),()=>{var s=CurrentAppearance;s.screen=1-s.screen;SaveStadium(s);Show("custom");},Color.white);
    Button("Decorative flags: "+(a.flags?"On":"Off"),new Vector2(330,219),()=>{var s=CurrentAppearance;s.flags=!s.flags;SaveStadium(s);Show("custom");},Color.white);}Back(rooms.Connected?"room":"sport");
   } else if(which=="room"){
    Heading(SelectedSport+" room","Code: "+rooms.Code+"   /   10 places");
    roster=Label(page,"Waiting for players…",0,new Vector2(82,469),new Vector2(505,330),22,ink);
    Button("Ready / not ready",new Vector2(330,248),()=>{var p=LocalNetwork();if(p)p.ReadyRpc(!p.Ready.Value);},mint);
    if(rooms.Host){if(SelectedSport==SportId.Golf)Button("Start",new Vector2(330,158),async()=>await rooms.SetExploring(true),mint);else{Button("Start",new Vector2(454,158),async()=>await rooms.SetExploring(true),mint,new Vector2(240,72));Button("Stadium",new Vector2(207,158),()=>Show("custom"),Color.white,new Vector2(240,72));}}
    else Button("Your athlete",new Vector2(330,158),()=>Show("character"),Color.white);
    Button("Leave",new Vector2(143,73),async()=>{await rooms.Leave();Exploring=false;Show("sport");},Color.white,new Vector2(130,48));
    status=Label(page,"",0,new Vector2(292,72),new Vector2(295,80),17,ink);RefreshRoster();UpdateStatus();
   }
  }
  NetworkAthlete LocalNetwork()=>FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).FirstOrDefault(p=>p.IsOwner);
  void RefreshRoster(){if(!roster)return;var players=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).OrderBy(p=>p.OwnerClientId).ToArray();roster.text=string.Join("\n",players.Select(p=>{
   return $"{(p.OwnerClientId==0?"HOST":"PLAYER "+p.OwnerClientId)}{(p.IsOwner?" (you)":"")}  ·  Rainbow Sprinter  ·  {(p.Ready.Value?"READY":"choosing…")}";}));}
  public void SaveStadium(StadiumAppearance a){if(SelectedSport==SportId.Golf||rooms.Connected&&!rooms.Host)return;LocalProfile.SaveSport(SelectedSport,a);stadium.Apply(a);if(rooms.Host&&NetworkAthlete.HostPlayer)NetworkAthlete.HostPlayer.WorldAppearance.Value=JsonUtility.ToJson(a);}
  void Heading(string title,string caption){Label(page,title,0,new Vector2(82,714),new Vector2(510,80),44,ink);Label(page,caption,0,new Vector2(82,643),new Vector2(490,64),22,ink);}
  void Back(string target){Button("← Back",new Vector2(165,91),()=>Show(target),Color.white,new Vector2(165,54));}
  void HUD(){
   var look=Panel(page,new Vector2(1170,480),new Vector2(820,720),new Color(1,1,1,.001f));look.gameObject.AddComponent<TouchPad>().look=true;
   var bg=Panel(page,new Vector2(190,178),new Vector2(176,176),new Color(1,1,1,.25f));var pad=bg.gameObject.AddComponent<TouchPad>();var knob=Panel(bg,Vector2.zero,new Vector2(82,82),cream);knob.anchorMin=knob.anchorMax=new Vector2(.5f,.5f);var circle=MakeCircle();foreach(var r in new[]{bg,knob}){r.GetComponent<Image>().sprite=circle;r.GetComponent<Image>().type=Image.Type.Simple;}pad.knob=knob;view.stick=pad;
   Button("Camera",new Vector2(1425,155),view.Switch,mint,new Vector2(220,90));Button(rooms.Host?"Return to room":SelectedSport switch{SportId.Basketball=>"Leave court",SportId.Golf=>"Leave island",_=>"Leave pitch"},new Vector2(1418,818),Return,cream,new Vector2(250,62));
   if(SelectedSport==SportId.Football){
    var rect=Panel(page,new Vector2(1425,285),new Vector2(220,105),LocalProfile.Hex("F0B956"));rect.name="Tackle button";
    var control=rect.gameObject.AddComponent<TackleButton>();control.button=rect.gameObject.AddComponent<Button>();
    control.label=Label(rect,"Tackle [Space]",0,Vector2.zero,new Vector2(210,90),23,ink);
    control.label.alignment=TextAnchor.MiddleCenter;control.label.rectTransform.anchorMin=control.label.rectTransform.anchorMax=control.label.rectTransform.pivot=new Vector2(.5f,.5f);
   }
   var stadiumName=rooms.Connected&&NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.WorldAppearance.Value.Length>0?JsonUtility.FromJson<StadiumAppearance>(NetworkAthlete.HostPlayer.WorldAppearance.Value.ToString()).title:CurrentAppearance.title;
   Pill(stadiumName.ToUpperInvariant(),new Vector2(252,818),new Vector2(400,62));fps=Label(page,"",0,new Vector2(66,742),new Vector2(380,44),18,Color.white);
   Label(page,"Drag right to look • Push stick fully to run",0,new Vector2(530,72),new Vector2(570,40),21,Color.white);
  }
  void Pill(string s,Vector2 p,Vector2 size){var r=Panel(page,p,size,cream);var t=Label(r,s,0,Vector2.zero,size,23,ink);t.alignment=TextAnchor.MiddleCenter;var rt=t.rectTransform;rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.pivot=new Vector2(.5f,.5f);}
  public RectTransform Panel(Transform parent,Vector2 p,Vector2 size,Color c){var o=new GameObject("Card",typeof(RectTransform),typeof(Image));var r=o.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=Vector2.zero;r.sizeDelta=size;r.anchoredPosition=p;var im=o.GetComponent<Image>();im.sprite=rounded;im.type=Image.Type.Sliced;im.color=c;return r;}
  Text Label(Transform parent,string text,int unused,Vector2 p,Vector2 size,int points,Color c){var o=new GameObject("Text",typeof(RectTransform),typeof(Text));var t=o.GetComponent<Text>();t.transform.SetParent(parent,false);var r=t.rectTransform;r.anchorMin=r.anchorMax=Vector2.zero;r.pivot=new Vector2(0,.5f);r.anchoredPosition=p;r.sizeDelta=size;t.font=font;t.fontSize=points;t.color=c;t.text=text;t.supportRichText=false;t.raycastTarget=false;t.verticalOverflow=VerticalWrapMode.Overflow;return t;}
  void Button(string text,Vector2 p,Action action,Color c,Vector2 size=default){if(size==default)size=new Vector2(495,80);var r=Panel(page,p,size,c);var b=r.gameObject.AddComponent<Button>();b.interactable=action!=null;b.onClick.AddListener(()=>{if(!rooms.busy)action?.Invoke();});var t=Label(r,text,0,Vector2.zero,size-new Vector2(18,0),25,ink);t.alignment=TextAnchor.MiddleCenter;t.rectTransform.anchorMin=t.rectTransform.anchorMax=new Vector2(.5f,.5f);t.rectTransform.pivot=new Vector2(.5f,.5f);}
  InputField Field(string placeholder,Vector2 p,Vector2 size,string value,int limit){var r=Panel(page,p,size,Color.white);var input=r.gameObject.AddComponent<InputField>();var t=Label(r,value,0,Vector2.zero,size-new Vector2(32,0),24,ink);t.rectTransform.anchorMin=t.rectTransform.anchorMax=new Vector2(.5f,.5f);t.rectTransform.pivot=new Vector2(.5f,.5f);input.textComponent=t;input.text=value;input.characterLimit=limit;var ph=Label(r,placeholder,0,Vector2.zero,size-new Vector2(32,0),23,new Color(.35f,.45f,.4f));ph.rectTransform.anchorMin=ph.rectTransform.anchorMax=new Vector2(.5f,.5f);ph.rectTransform.pivot=new Vector2(.5f,.5f);input.placeholder=ph;t.alignment=TextAnchor.MiddleLeft;ph.alignment=TextAnchor.MiddleLeft;ph.gameObject.SetActive(string.IsNullOrEmpty(value));return input;}
  static Sprite MakeCircle(){var t=new Texture2D(64,64,TextureFormat.RGBA32,false);for(int y=0;y<64;y++)for(int x=0;x<64;x++)t.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(32-Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f)))));t.Apply();return Sprite.Create(t,new Rect(0,0,64,64),new Vector2(.5f,.5f));}
  static Sprite MakeRound(){var t=new Texture2D(64,64,TextureFormat.RGBA32,false);for(int y=0;y<64;y++)for(int x=0;x<64;x++){float dx=Mathf.Max(18-x,x-45),dy=Mathf.Max(18-y,y-45);float d=new Vector2(Mathf.Max(0,dx),Mathf.Max(0,dy)).magnitude;t.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(19-d)));}t.Apply();return Sprite.Create(t,new Rect(0,0,64,64),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(20,20,20,20));}
 }
}
