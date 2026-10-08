using System;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WhatTheFish {
 public sealed class AppRoot:MonoBehaviour {
  public static AppRoot Instance; public SportEnvironmentController environments;public StadiumView stadium=>environments.View;public SportId SelectedSport=>environments.Selected;public StadiumAppearance CurrentAppearance=>LocalProfile.ForSport(SelectedSport);public PlayerView view;public RoomService rooms;
  public Athlete LocalAthlete;public bool Exploring {get;private set;}
  public StorybookMenu Menu {get;private set;}
  public SportId? PendingSelection=>pendingSelection;
  bool SupportsCustomization=>SelectedSport==SportId.Football||SelectedSport==SportId.Basketball;
  public GameObject athletePrefab; Athlete offline; Canvas canvas;RectTransform safe,page;Font font;Sprite rounded;Text status,roster,fps,controlHint;float rosterTimer;string screen="home",appliedWorld="";bool lastExploring; InputField code;Button cameraControl;
  readonly Color ink=LocalProfile.Hex("173834"),mint=LocalProfile.Hex("BFEBCB"),cream=LocalProfile.Hex("FFF9E9");
  void Awake(){Instance=this;Application.targetFrameRate=Application.isMobilePlatform?30:60;Screen.sleepTimeout=SleepTimeout.NeverSleep;}
  SportId? pendingSelection; uint selectionVersion;
  IEnumerator Start(){
   font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");rounded=MakeRound();
   canvas=new GameObject("Mobile UI",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
   var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
   safe=new GameObject("Safe area",typeof(RectTransform)).GetComponent<RectTransform>();safe.SetParent(canvas.transform,false);ApplySafeArea();
   if(!FindFirstObjectByType<EventSystem>())new GameObject("Input",typeof(EventSystem),typeof(StandaloneInputModule));
   rooms.SetupNetwork();rooms.Changed+=OnRoomChanged;
   offline=Instantiate(athletePrefab,new Vector3(0,.07f,-8),Quaternion.identity).GetComponent<Athlete>();offline.name="Offline athlete";offline.Setup();offline.Appearance(LocalProfile.Character);LocalAthlete=offline;
   var streaming=environments.GetComponent<SkySailStreaming>();if(streaming&&streaming.enabledForWorld)yield return streaming.Prepare(SportId.Football);
   environments.Activate(SportId.Football);stadium?.Apply(CurrentAppearance);Show("home");
   var args=Environment.GetCommandLineArgs();int sportArg=Array.IndexOf(args,"-sport");if(sportArg>=0&&sportArg+1<args.Length&&Enum.TryParse<SportId>(args[sportArg+1],true,out var requested)&&environments.Definition(requested)&&environments.Definition(requested).available){if(streaming&&streaming.enabledForWorld)yield return streaming.Prepare(requested);SelectSport(requested);}if(args.Contains("-offline"))EnterOffline();
   if(args.Contains("-localHost")||args.Contains("-localClient")){ushort port=7777;int ix=Array.IndexOf(args,"-port");if(ix>=0&&ix+1<args.Length)ushort.TryParse(args[ix+1],out port);offline.gameObject.SetActive(false);rooms.StartLocal(args.Contains("-localHost"),port);Show("room");}
  }
  void ApplySafeArea(){var a=Screen.safeArea;safe.anchorMin=new Vector2(a.x/Screen.width,a.y/Screen.height);safe.anchorMax=new Vector2(a.xMax/Screen.width,a.yMax/Screen.height);safe.offsetMin=safe.offsetMax=Vector2.zero;}
  void OnRoomChanged(){if(!rooms.Connected){appliedWorld="";stadium?.Apply(CurrentAppearance);}if(rooms.Connected){offline.gameObject.SetActive(false);if(screen=="home"||screen=="sport"||screen=="sports"||screen=="create"||screen=="join")Show("room");}else if(screen=="room"||screen=="stadium"&&lastExploring){Exploring=false;lastExploring=false;LocalAthlete=offline;Show("sport");}UpdateStatus();}
  void Update(){
   if(safe)ApplySafeArea();if(cameraControl)cameraControl.interactable=!view.GolfAimRequested;
   if(!Exploring){if(screen=="character"){view.transform.position=offline.transform.position+new Vector3(1.4f,1.25f,3.7f);view.transform.LookAt(offline.transform.position+new Vector3(1.4f,1.1f,0));}else {view.transform.position=environments.Current.menuCamera;view.transform.LookAt(environments.Current.menuFocus);}}
   if(!rooms.Connected&&lastExploring){Exploring=false;lastExploring=false;LocalAthlete=offline;Show("sport");}
   if(rooms.Connected&&NetworkAthlete.HostPlayer){
    var world=NetworkAthlete.HostPlayer;if(SelectedSport!=world.WorldSport.Value&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling)){SelectSport(world.WorldSport.Value,true);if(screen=="room")Show("room");}string json=world.WorldAppearance.Value.ToString();if(json.Length>0&&json!=appliedWorld){appliedWorld=json;stadium?.Apply(JsonUtility.FromJson<StadiumAppearance>(json));}
    if(world.Exploring.Value!=lastExploring){lastExploring=world.Exploring.Value;Exploring=lastExploring;Show(Exploring?"stadium":"room");}
   }
   view.target=LocalAthlete;view.active=Exploring&&(!Menu||!Menu.IsTransitioning);
   if(controlHint)controlHint.text=view.GolfAimRequested?"Drag right to aim • Hold, then release • Cancel to walk":GolfCartWorld.Allowed&&GolfCartWorld.Driving(LocalAthlete)?"Left stick: drive & steer • Drag right to look":view.mode==2&&!(LocalAthlete&&LocalAthlete.inTransit)&&(SelectedSport==SportId.Football||SelectedSport==SportId.Basketball)?SelectedSport==SportId.Basketball?"Drag right to aim • Push stick fully to run":"Broadcast view • Push stick fully to run":"Drag right to look • Push stick fully to run";
   if(Exploring&&!rooms.Connected&&LocalAthlete&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling))LocalAthlete.Simulate(view.ReadCommand(),Time.deltaTime);
   if(!Exploring&&LocalAthlete)LocalAthlete.HideHead(false);
   if((rosterTimer-=Time.deltaTime)<0){rosterTimer=.5f;RefreshRoster();if(fps)fps.text=$"{Mathf.RoundToInt(1/Mathf.Max(Time.smoothDeltaTime,.001f))} FPS   •   {(rooms.Connected?"ONLINE":"OFFLINE")}";}
   if(Input.GetKeyDown(KeyCode.Escape)){if(view.GolfAimRequested)view.CancelGolfAim();else if(Exploring)Return();else if(Menu)Menu.Back();else Show(rooms.Connected?"room":"home");}
  }
  void UpdateStatus(){if(status)status.text=rooms.busy?"Connecting…":rooms.Error??"";}
  public void SelectSport(SportId sport,bool fromHost=false){if(rooms.Connected&&!fromHost)return;var streaming=environments.GetComponent<SkySailStreaming>();if(streaming&&streaming.enabledForWorld&&!streaming.Loaded(sport)){if(pendingSelection!=sport){pendingSelection=sport;Show(screen);StartCoroutine(SelectLoaded(sport,fromHost,++selectionVersion));}return;}pendingSelection=null;selectionVersion++;if(sport!=SelectedSport)GolfCartWorld.ParkDrivers(false);environments.Activate(sport);appliedWorld="";stadium?.Apply(CurrentAppearance);if(offline&&!rooms.Connected)ResetOfflineSpawn();view.yaw=0;view.pitch=16;PlayerView.LookDelta=Vector2.zero;}
  IEnumerator SelectLoaded(SportId sport,bool fromHost,uint version){yield return environments.GetComponent<SkySailStreaming>().Prepare(sport);if(version!=selectionVersion)yield break;pendingSelection=null;if(!environments.GetComponent<SkySailStreaming>().Loaded(sport)){Debug.LogError("Could not load "+sport);yield break;}SelectSport(sport,fromHost);Show(screen);}
  IEnumerator EnterWhenReady(){while(pendingSelection.HasValue)yield return null;EnterOffline();}
  void ResetOfflineSpawn(){offline.capsule.enabled=false;offline.transform.SetPositionAndRotation(environments.Current.Spawn(0),Quaternion.identity);offline.capsule.enabled=true;offline.ResetLocomotion();}
  public void EnterOffline(){if(pendingSelection.HasValue){StartCoroutine(EnterWhenReady());return;}Exploring=true;lastExploring=false;offline.gameObject.SetActive(true);LocalAthlete=offline;ResetOfflineSpawn();stadium?.Apply(CurrentAppearance);Show("stadium");}
  async void Return(){if(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling)return;if(rooms.Connected){if(rooms.Host)await rooms.SetExploring(false);else {await rooms.Leave();Exploring=false;Show("sport");}}else{Exploring=false;Show("sport");}}
  void Clear(){PlayerView.LookDelta=Vector2.zero;if(page)Destroy(page.gameObject);page=new GameObject("Page",typeof(RectTransform)).GetComponent<RectTransform>();page.SetParent(safe,false);page.anchorMin=Vector2.zero;page.anchorMax=Vector2.one;page.offsetMin=page.offsetMax=Vector2.zero;roster=null;fps=null;status=null;}
  public void Show(string which){
   screen=which;
   if(which=="stadium"&&!pendingSelection.HasValue){if(Menu)Menu.Hide();Clear();HUD();
    GameButtonStyle.Apply(page);
    if(Menu)Menu.ArriveInWorld(page);
    return;
   }
   if(page){page.gameObject.SetActive(false);Destroy(page.gameObject);page=null;}roster=null;fps=null;status=null;
   if(!Menu){Menu=gameObject.AddComponent<StorybookMenu>();Menu.Initialize(this,safe);}
   Menu.Show(pendingSelection.HasValue?"loading":which);
  }
  NetworkAthlete LocalNetwork()=>FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).FirstOrDefault(p=>p.IsOwner);
  void RefreshRoster(){if(!roster)return;var players=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).OrderBy(p=>p.OwnerClientId).ToArray();roster.text=string.Join("\n",players.Select(p=>{
   return $"{(p.OwnerClientId==0?"HOST":"PLAYER "+p.OwnerClientId)}{(p.IsOwner?" (you)":"")}  ·  Rainbow Sprinter  ·  {(p.Ready.Value?"READY":"choosing…")}";}));}
  public void SaveStadium(StadiumAppearance a){if(!SupportsCustomization||rooms.Connected&&!rooms.Host)return;a.Clamp();LocalProfile.SaveSport(SelectedSport,a);stadium?.Apply(a);if(rooms.Host&&NetworkAthlete.HostPlayer)NetworkAthlete.HostPlayer.WorldAppearance.Value=JsonUtility.ToJson(a);}
  void Heading(string title,string caption){Label(page,title,0,new Vector2(82,714),new Vector2(510,80),44,ink);Label(page,caption,0,new Vector2(82,643),new Vector2(490,64),22,ink);}
  void Back(string target){Button("← Back",new Vector2(165,91),()=>Show(target),Color.white,new Vector2(165,54));}
  void HUD(){
   var look=Panel(page,new Vector2(1170,480),new Vector2(820,720),new Color(1,1,1,.001f));look.gameObject.AddComponent<TouchPad>().look=true;
   var bg=Panel(page,new Vector2(190,178),new Vector2(176,176),new Color(1,1,1,.25f));var pad=bg.gameObject.AddComponent<TouchPad>();var knob=Panel(bg,Vector2.zero,new Vector2(82,82),cream);knob.anchorMin=knob.anchorMax=new Vector2(.5f,.5f);var circle=MakeCircle();foreach(var r in new[]{bg,knob}){r.GetComponent<Image>().sprite=circle;r.GetComponent<Image>().type=Image.Type.Simple;}pad.knob=knob;view.stick=pad;
   cameraControl=Button("Camera",new Vector2(1470, 70),view.Switch,mint,new Vector2(160,60));Button(rooms.Host?"Return to room":SelectedSport switch{SportId.Basketball=>"Leave court",SportId.Golf=>"Leave island",SportId.Fishing=>"Leave lagoon",_=>"Leave pitch"},new Vector2(SelectedSport==SportId.Golf?1210:1418,SelectedSport==SportId.Golf?55:818),Return,cream,new Vector2(250,62));
   var jumpRect=Panel(page,new Vector2(1470,405),new Vector2(132,132),mint);jumpRect.name="Jump button";
   var jump=jumpRect.gameObject.AddComponent<JumpButton>();jump.button=jumpRect.gameObject.AddComponent<Button>();
   jump.label=Label(jumpRect,"Jump [Space]",0,Vector2.zero,new Vector2(210,90),23,ink);
   jump.label.alignment=TextAnchor.MiddleCenter;jump.label.rectTransform.anchorMin=jump.label.rectTransform.anchorMax=jump.label.rectTransform.pivot=new Vector2(.5f,.5f);
   if(SelectedSport==SportId.Golf){
    GolfLeaderboardUI.Create(page,font);
    var swingRect=Panel(page,new Vector2(1470,225),new Vector2(160,160),mint);swingRect.name="Golf swing button";
    var swing=swingRect.gameObject.AddComponent<GolfSwingButton>();swing.button=swingRect.gameObject.AddComponent<Button>();
    swing.label=Label(swingRect,"Swing [F]",0,Vector2.zero,new Vector2(210,90),23,ink);
    swing.label.alignment=TextAnchor.MiddleCenter;swing.label.rectTransform.anchorMin=swing.label.rectTransform.anchorMax=swing.label.rectTransform.pivot=new Vector2(.5f,.5f);
    var aimRect=Panel(page,new Vector2(1130,225),new Vector2(132,132),mint);aimRect.name="Golf aim button";
    swing.aimButton=aimRect.gameObject.AddComponent<Button>();swing.aimButton.onClick.AddListener(view.ToggleGolfAim);
    swing.aimLabel=Label(aimRect,"Aim",0,Vector2.zero,new Vector2(210,80),23,ink);
    swing.aimLabel.alignment=TextAnchor.MiddleCenter;swing.aimLabel.rectTransform.anchorMin=swing.aimLabel.rectTransform.anchorMax=swing.aimLabel.rectTransform.pivot=new Vector2(.5f,.5f);
    var startRect=Panel(page,new Vector2(800,813),new Vector2(265,64),mint);startRect.name="Start golf match";
    swing.startButton=startRect.gameObject.AddComponent<Button>();swing.startButton.onClick.AddListener(()=>GolfMatchManager.Instance?.StartMatch());
    swing.startLabel=Label(startRect,"Start golf match",0,Vector2.zero,new Vector2(250,60),23,ink);
    swing.startLabel.alignment=TextAnchor.MiddleCenter;swing.startLabel.rectTransform.anchorMin=swing.startLabel.rectTransform.anchorMax=swing.startLabel.rectTransform.pivot=new Vector2(.5f,.5f);
    foreach(bool summon in new[]{true,false}){
     var rect=Panel(page,new Vector2(1300,summon?340:165),new Vector2(138,138),summon?mint:LocalProfile.Hex("F0B956"));rect.name=summon?"Golf cart summon button":"Golf cart drive button";
     var control=rect.gameObject.AddComponent<GolfCartButton>();control.summon=summon;control.button=rect.gameObject.AddComponent<Button>();
     control.label=Label(rect,summon?"召唤":"驾驶",0,Vector2.zero,new Vector2(210,90),27,ink);control.label.font=Resources.Load<Font>("GolfCartLabels");
     control.label.alignment=TextAnchor.MiddleCenter;control.label.rectTransform.anchorMin=control.label.rectTransform.anchorMax=control.label.rectTransform.pivot=new Vector2(.5f,.5f);
    }
   }
   if(SelectedSport==SportId.Football){
    FootballMatchHUD.Create(page,font);FootballEffortHUD.Create(this,page,font);
    var rect=Panel(page,new Vector2(1300,165),new Vector2(138,138),LocalProfile.Hex("F0B956"));rect.name="Tackle button";
    var control=rect.gameObject.AddComponent<TackleButton>();control.button=rect.gameObject.AddComponent<Button>();
    control.label=Label(rect,"Tackle [E]",0,Vector2.zero,new Vector2(210,90),23,ink);
    control.label.alignment=TextAnchor.MiddleCenter;control.label.rectTransform.anchorMin=control.label.rectTransform.anchorMax=control.label.rectTransform.pivot=new Vector2(.5f,.5f);
    var kickRect=Panel(page,new Vector2(1470,225),new Vector2(160,160),mint);kickRect.name="Kick button";
    var kick=kickRect.gameObject.AddComponent<KickButton>();kick.button=kickRect.gameObject.AddComponent<Button>();
    var kickLabel=Label(kickRect,Application.isMobilePlatform?"Kick":"Kick [F]",0,Vector2.zero,new Vector2(210,90),23,ink);
    kick.label=kickLabel;
    kickLabel.alignment=TextAnchor.MiddleCenter;kickLabel.rectTransform.anchorMin=kickLabel.rectTransform.anchorMax=kickLabel.rectTransform.pivot=new Vector2(.5f,.5f);
    var cancelRect=Panel(page,new Vector2(1300,165),new Vector2(138,74),cream);cancelRect.name="Kick cancel area";
    kick.cancelArea=cancelRect;kick.cancelLabel=Label(cancelRect,"Cancel",0,Vector2.zero,new Vector2(130,64),23,ink);
    kick.cancelLabel.alignment=TextAnchor.MiddleCenter;kick.cancelLabel.rectTransform.anchorMin=kick.cancelLabel.rectTransform.anchorMax=kick.cancelLabel.rectTransform.pivot=new Vector2(.5f,.5f);
    cancelRect.gameObject.SetActive(false);
   }
   if(SelectedSport==SportId.Basketball){
    BasketballHUD.Create(page,font);
    var rect=Panel(page,new Vector2(1470,225),new Vector2(160,160),LocalProfile.Hex("F0B956"));rect.name="Shoot button";
    var control=rect.gameObject.AddComponent<BasketballShootButton>();control.button=rect.gameObject.AddComponent<Button>();
    control.label=Label(rect,"Shoot [E]",0,Vector2.zero,new Vector2(210,90),20,ink);
    control.label.alignment=TextAnchor.MiddleCenter;control.label.rectTransform.anchorMin=control.label.rectTransform.anchorMax=control.label.rectTransform.pivot=new Vector2(.5f,.5f);
    var cancel=Panel(page,new Vector2(1300,340),new Vector2(138,74),cream);cancel.name="Basketball cancel area";
    control.cancelArea=cancel;control.cancelLabel=Label(cancel,"Cancel",0,Vector2.zero,new Vector2(130,64),23,ink);
    control.cancelLabel.alignment=TextAnchor.MiddleCenter;control.cancelLabel.rectTransform.anchorMin=control.cancelLabel.rectTransform.anchorMax=control.cancelLabel.rectTransform.pivot=new Vector2(.5f,.5f);
    cancel.gameObject.AddComponent<Button>().onClick.AddListener(control.CancelGesture);cancel.gameObject.SetActive(false);
   }
   if(SelectedSport==SportId.Basketball){
    var rect=Panel(page,new Vector2(1300,165),new Vector2(138,138),LocalProfile.Hex("54D7C4"));rect.name="Pass button";
    var control=rect.gameObject.AddComponent<BasketballShootButton>();control.pass=true;control.button=rect.gameObject.AddComponent<Button>();
    control.label=Label(rect,"Pass [Q]",0,Vector2.zero,new Vector2(190,90),23,ink);
    control.label.alignment=TextAnchor.MiddleCenter;control.label.rectTransform.anchorMin=control.label.rectTransform.anchorMax=control.label.rectTransform.pivot=new Vector2(.5f,.5f);
    var cancel=Panel(page,new Vector2(1130,165),new Vector2(138,74),cream);cancel.name="Basketball pass cancel area";
    control.cancelArea=cancel;control.cancelLabel=Label(cancel,"Cancel",0,Vector2.zero,new Vector2(130,64),23,ink);
    control.cancelLabel.alignment=TextAnchor.MiddleCenter;control.cancelLabel.rectTransform.anchorMin=control.cancelLabel.rectTransform.anchorMax=control.cancelLabel.rectTransform.pivot=new Vector2(.5f,.5f);
    cancel.gameObject.AddComponent<Button>().onClick.AddListener(control.CancelGesture);cancel.gameObject.SetActive(false);
   }
   if(SelectedSport==SportId.Basketball){
    var rect=Panel(page,new Vector2(1300,340),new Vector2(138,138),mint);rect.name="Steal button";
    var control=rect.gameObject.AddComponent<BasketballStealButton>();control.button=rect.gameObject.AddComponent<Button>();
    control.label=Label(rect,"Hold F to steal",0,Vector2.zero,new Vector2(190,90),23,ink);
    control.label.alignment=TextAnchor.MiddleCenter;control.label.rectTransform.anchorMin=control.label.rectTransform.anchorMax=control.label.rectTransform.pivot=new Vector2(.5f,.5f);
   }
   if(SelectedSport==SportId.Basketball)BasketballActionSlot.Attach(page);
   if(SelectedSport==SportId.Football)FootballActionSlot.Attach(page);
   if(SelectedSport==SportId.Football||SelectedSport==SportId.Basketball)SportsMiniMap.Create(page,SelectedSport);
   var stadiumName=rooms.Connected&&NetworkAthlete.HostPlayer&&NetworkAthlete.HostPlayer.WorldAppearance.Value.Length>0?JsonUtility.FromJson<StadiumAppearance>(NetworkAthlete.HostPlayer.WorldAppearance.Value.ToString()).title:CurrentAppearance.title;
   Pill(stadiumName.ToUpperInvariant(),new Vector2(252,818),new Vector2(400,62));fps=Label(page,"",0,new Vector2(66,742),new Vector2(380,44),18,Color.white);
   controlHint=Label(page,"Drag right to look • Push stick fully to run",0,new Vector2(530,72),new Vector2(570,40),21,Color.white);
   if(SelectedSport==SportId.Football||SelectedSport==SportId.Basketball){controlHint.rectTransform.anchorMin=controlHint.rectTransform.anchorMax=new Vector2(.5f,0);controlHint.rectTransform.anchoredPosition=new Vector2(-285,SelectedSport==SportId.Football?193:148);controlHint.alignment=TextAnchor.MiddleCenter;controlHint.fontSize=18;}
  }
  void Pill(string s,Vector2 p,Vector2 size){var r=Panel(page,p,size,cream);var t=Label(r,s,0,Vector2.zero,size,23,ink);t.alignment=TextAnchor.MiddleCenter;var rt=t.rectTransform;rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.pivot=new Vector2(.5f,.5f);}
  public RectTransform Panel(Transform parent,Vector2 p,Vector2 size,Color c){var o=new GameObject("Card",typeof(RectTransform),typeof(Image));var r=o.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=Vector2.zero;r.sizeDelta=size;r.anchoredPosition=p;AnchorHud(r,p);var im=o.GetComponent<Image>();im.sprite=rounded;im.type=Image.Type.Sliced;im.color=c;return r;}
  void AnchorHud(RectTransform rect,Vector2 point){
   if(rect.parent!=page)return;
   // Keep the original 1600x900 touch layout, anchored to the safe edges on
   // wider phones and taller tablets. Centre overlays own their own anchors.
   var anchor=Vector2.zero;
   if(point.x>=900){anchor.x=1;point.x-=1600;}
   else if(Mathf.Approximately(point.x,800)){anchor.x=.5f;point.x-=800;}
   if(point.y>=700){anchor.y=1;point.y-=900;}
   rect.anchorMin=rect.anchorMax=anchor;rect.anchoredPosition=point;
  }
  Text Label(Transform parent,string text,int unused,Vector2 p,Vector2 size,int points,Color c){var o=new GameObject("Text",typeof(RectTransform),typeof(Text));var t=o.GetComponent<Text>();t.transform.SetParent(parent,false);var r=t.rectTransform;r.anchorMin=r.anchorMax=Vector2.zero;r.pivot=new Vector2(0,.5f);r.anchoredPosition=p;r.sizeDelta=size;t.font=font;t.fontSize=points;t.color=c;t.text=text;t.supportRichText=false;t.raycastTarget=false;t.verticalOverflow=VerticalWrapMode.Overflow;AnchorHud(r,p);return t;}
  Button Button(string text,Vector2 p,Action action,Color c,Vector2 size=default){if(size==default)size=new Vector2(495,80);var r=Panel(page,p,size,c);var b=r.gameObject.AddComponent<Button>();b.interactable=action!=null;b.onClick.AddListener(()=>{if(!rooms.busy)action?.Invoke();});var t=Label(r,text,0,Vector2.zero,size-new Vector2(18,0),25,ink);t.alignment=TextAnchor.MiddleCenter;t.rectTransform.anchorMin=t.rectTransform.anchorMax=new Vector2(.5f,.5f);t.rectTransform.pivot=new Vector2(.5f,.5f);return b;}
  InputField Field(string placeholder,Vector2 p,Vector2 size,string value,int limit){var r=Panel(page,p,size,Color.white);var input=r.gameObject.AddComponent<InputField>();var t=Label(r,value,0,Vector2.zero,size-new Vector2(32,0),24,ink);t.rectTransform.anchorMin=t.rectTransform.anchorMax=new Vector2(.5f,.5f);t.rectTransform.pivot=new Vector2(.5f,.5f);input.textComponent=t;input.text=value;input.characterLimit=limit;var ph=Label(r,placeholder,0,Vector2.zero,size-new Vector2(32,0),23,new Color(.35f,.45f,.4f));ph.rectTransform.anchorMin=ph.rectTransform.anchorMax=new Vector2(.5f,.5f);ph.rectTransform.pivot=new Vector2(.5f,.5f);input.placeholder=ph;t.alignment=TextAnchor.MiddleLeft;ph.alignment=TextAnchor.MiddleLeft;ph.gameObject.SetActive(string.IsNullOrEmpty(value));return input;}
  static Sprite MakeCircle(){var t=new Texture2D(64,64,TextureFormat.RGBA32,false);for(int y=0;y<64;y++)for(int x=0;x<64;x++)t.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(32-Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f)))));t.Apply();return Sprite.Create(t,new Rect(0,0,64,64),new Vector2(.5f,.5f));}
  static Sprite MakeRound(){var t=new Texture2D(64,64,TextureFormat.RGBA32,false);for(int y=0;y<64;y++)for(int x=0;x<64;x++){float dx=Mathf.Max(18-x,x-45),dy=Mathf.Max(18-y,y-45);float d=new Vector2(Mathf.Max(0,dx),Mathf.Max(0,dy)).magnitude;t.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(19-d)));}t.Apply();return Sprite.Create(t,new Rect(0,0,64,64),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(20,20,20,20));}
 }
}
