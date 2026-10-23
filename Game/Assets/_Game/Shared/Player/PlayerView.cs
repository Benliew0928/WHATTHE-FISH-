using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 [DefaultExecutionOrder(-100)]
 public sealed partial class PlayerView:MonoBehaviour,IPlayerCommandSource {
  public static Vector2 LookDelta; public static PlayerView Instance; public Athlete target; public TouchPad stick; public int mode; public float yaw=0,pitch=16; public bool active; Camera cam;bool pendingTackle,pendingJump,pendingShoot,pendingPass;
  void Awake(){Instance=this;cam=GetComponent<Camera>();mode=Mathf.Clamp(PlayerPrefs.GetInt("camera",1),0,2);gameObject.AddComponent<StadiumCameraPresenter>();}
  public void RequestTackle(){if(!InputBlocked&&active&&target&&target.TackleReady&&!SportsPossession.Attacking(target,SportId.Football))pendingTackle=true;}
  bool pendingKick;float pendingCharge,chargeStarted;int chargeSource;
  GolfCartAction pendingCart;ulong pendingCartOwner;
  bool pendingGolf;ulong pendingGolfOwner;uint pendingGolfRound;float pendingGolfCharge,selectedGolfPower;ulong chargingGolfOwner;int golfSource;
  public bool GolfCharging {get;private set;}
  public ulong GolfBallOwner=>chargingGolfOwner;
  public float GolfCharge=>GolfCharging?selectedGolfPower:0;
  public bool BeginGolfSwing(int source=int.MinValue){if(!TutorialSession.Allows("power"))return false;var match=GolfMatchManager.Instance;var ball=GolfAiming?match.Ball(aimOwner):null;if(InputBlocked||GolfCharging||golfReleased||!active||!ball||!match.CanStrike(target,ball))return false;chargingGolfOwner=ball.Owner;target.GolfClubMotion.Address(ball.Body.position);GolfCharging=true;chargePresented=false;golfSource=source;selectedGolfPower=0;golfPowerDragged=false;return true;}
  public void SetGolfPower(int source,float power){if(GolfCharging&&source==golfSource&&float.IsFinite(power)){selectedGolfPower=Mathf.Clamp01(power);golfPowerDragged=true;chargePresented=false;}}
  public void EndGolfSwing(int source=int.MinValue){if(!GolfCharging||source!=golfSource)return;float charge=GolfCharge;UpdateGolfPreview();float heading=chargePresented?presentedHeading:yaw;GolfCharging=false;if(charge<=.01f)return;QueueGolfSwing(charge,chargingGolfOwner);pendingGolfHeading=heading;}
  public void CancelGolfSwing(int source){if(GolfCharging&&source==golfSource)GolfCharging=false;}
  public void RequestGolfSwing(float charge){if(!GolfAimRequested)BeginGolfAim();var match=GolfMatchManager.Instance;if(GolfAiming)QueueGolfSwing(charge,aimOwner);}
  void QueueGolfSwing(float charge,ulong owner){var match=GolfMatchManager.Instance;if(InputBlocked||!active||golfReleased||!GolfAiming||!match||!match.CanStrike(target,match.Ball(owner)))return;WatchGolfShot(match.Ball(owner));pendingGolfHeading=yaw;pendingGolf=true;golfReleased=true;pendingGolfOwner=owner;pendingGolfRound=match.Round;pendingGolfCharge=Mathf.Clamp01(charge);}
  [Header("Golf match third-person framing")]
  [Min(0)] public float golfShoulderOffset=1.8f;
  [Min(1)] public float golfViewDistance=3.5f;
  float golfFraming;
  public bool Charging {get;private set;}
  MeshRenderer kickAim;Material kickAimMaterial;Mesh kickAimMesh;
  public float Charge=>Charging&&FootballBall.Instance?FootballBall.Instance.ChargeFraction(Time.time-chargeStarted):0;
  public void RequestKick(float charge=1){if(!InputBlocked&&active&&target&&target.KickReady&&SportsPossession.Attacking(target,SportId.Football)){pendingKick=true;pendingCharge=Mathf.Clamp01(charge);}}
  public bool BeginKick(int source=int.MinValue){if(!TutorialSession.Allows("shoot"))return false;if(InputBlocked||pendingTargetPass||Charging||FootballPassAiming||!active||!target||!target.KickReady||!SportsPossession.Attacking(target,SportId.Football))return false;Charging=true;chargeSource=source;chargeStarted=Time.time;if(source==int.MinValue)FootballAudio.PlayMenu();return true;}
  public void EndKick(int source=int.MinValue){if(!Charging||source!=chargeSource)return;float charge=Charge;Charging=false;HideKickAim();RequestKick(charge);}
  public void CancelKick(int source){if(Charging&&source==chargeSource){FootballAudio.PlayMenu(FootballSound.UIBack);Charging=false;HideKickAim();}}
  bool inputBlocked;
  public bool InputBlocked {get=>inputBlocked||RoomChatUI.Typing||SocialPanel.Visible||AccountPanel.Visible||TutorialSession.Active&&(!TutorialSession.Active.Ready||TutorialSession.Active.Paused||TutorialSession.Active.Finished);set=>inputBlocked=value;}
  public void ClearMatchInput(){CancelInput();golfFraming=0;ReturnFromGolfShot();}
  void CancelInput(){ClearTargetPass();if(FootballHUD.Instance)foreach(var swipe in FootballHUD.Instance.GetComponentsInChildren<FootballActionSwipe>())swipe.OnCancel(null);CancelFootballPass();pendingFootballPass=false;CancelFishing();CancelGolfAim();if(PassCharging||pendingPass||pendingPassBegin)pendingPassCancel=true;ClearFootballInput();ClearDefenseInput();CancelShot();CancelPass();pendingShotBegin=pendingPassBegin=false;StealHeld=pendingSteal=false;Charging=GolfCharging=false;HideKickAim();pendingKick=pendingGolf=false;pendingTackle=pendingJump=pendingShoot=pendingPass=false;pendingCart=GolfCartAction.None;}
  void HideKickAim(){if(kickAim)kickAim.enabled=false;}
  void OnDestroy(){if(golfPreview)Destroy(golfPreview.gameObject);if(kickAimMaterial)Destroy(kickAimMaterial);if(kickAimMesh)Destroy(kickAimMesh);}
  static Mesh CreateKickAimMesh(){
   // One reusable canvas for analytic rails, arrowhead, glow and flowing colour.
   // UVs are in arrow space; padding keeps the soft glow inside the quad.
   var mesh=new Mesh{name="Twin-rail energy kick arrow"};
   mesh.vertices=new[]{new Vector3(-.67f,0,-.12f),new Vector3(.67f,0,-.12f),new Vector3(.67f,0,1.12f),new Vector3(-.67f,0,1.12f)};
   mesh.uv=new[]{new Vector2(-.67f,-.12f),new Vector2(.67f,-.12f),new Vector2(.67f,1.12f),new Vector2(-.67f,1.12f)};
   mesh.triangles=new[]{0,2,1,0,3,2};mesh.RecalculateBounds();return mesh;
  }
  void UpdateKickAim(){
   var ball=FootballBall.Instance;
   if(!Charging||!active||!target||!target.KickReady||!ball||!FootballBall.Allowed&&!FootballMatch.CanAimKickoff(target)){HideKickAim();return;}
   if(!kickAim){
    var source=Resources.Load<Material>("FootballKickAim");if(!source)return;
    // An authored transparent material keeps its shader variant in player builds.
    kickAimMaterial=new Material(source){name="Kick aim"};
    var go=new GameObject("Kick aim");go.layer=2;go.transform.SetParent(transform,false);
    kickAimMesh=CreateKickAimMesh();go.AddComponent<MeshFilter>().sharedMesh=kickAimMesh;
    kickAim=go.AddComponent<MeshRenderer>();kickAim.sharedMaterial=kickAimMaterial;
    kickAim.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;kickAim.receiveShadows=false;
   }
   var direction=FootballBall.KickDirection(target);
   float charge=Charge;
   var cool=new Color(.08f,.85f,1,.9f);var gold=new Color(1,.8f,.16f,.9f);var hot=new Color(1,.2f,.32f,.9f);
   kickAimMaterial.SetColor("_BaseColor",charge<.55f?Color.Lerp(cool,gold,charge/.55f):Color.Lerp(gold,hot,(charge-.55f)/.45f));
   kickAimMaterial.SetFloat("_Charge",charge);
   kickAim.transform.SetPositionAndRotation(ball.transform.position+Vector3.up*.06f,Quaternion.LookRotation(direction));
   kickAim.transform.localScale=new Vector3(Mathf.Lerp(.85f,1.25f,charge),1,Mathf.Lerp(2.2f,4.4f,charge));
   kickAim.enabled=true;
  }
  void OnDisable(){ClearMatchInput();ResetGolfShotCamera();ResetFishingFightCamera();ResetStadiumCamera();ResetActivityStadiumCamera();if(Instance==this)Instance=null;}
  void OnEnable(){Instance=this;}
  void OnApplicationFocus(bool focus){if(!focus)CancelInput();}
  void OnApplicationPause(bool paused){if(paused)CancelInput();}
  public void RequestJump(){if(!TutorialSession.Allows("jump"))return;if(BasketballRole==WhatTheFish.BasketballRole.Defense){RequestBlock(true);return;}if(active&&!GolfAimRequested&&target&&target.CanRequestJump)pendingJump=true;}
  bool pendingShotBegin,pendingShotCancel;int shotSource;double shotPhase,shotSampled,pendingShotReleasedAt;Transform shotHoop;
  public bool ShotCharging {get;private set;}
  public Transform SelectedShotHoop=>shotHoop;
  public BasketballFinish ShotFinish {get;private set;}
  BasketballFinish pendingFinish;
  public void SelectFinish(BasketballFinish kind,int source=int.MinValue){if(ShotCharging&&source==shotSource&&kind>=BasketballFinish.Shot&&kind<=BasketballFinish.Dunk)ShotFinish=kind;}
  public BasketballFinishReason FinishAvailability=>BasketballBall.Active?BasketballBall.Active.FinishAvailability(target,ShotFinish,shotHoop):BasketballFinishReason.Possession;
  public bool ShotNeedsTiming=>ShotCharging&&(ShotFinish==BasketballFinish.Shot||FinishAvailability!=BasketballFinishReason.Ready);
  public float ShotWindow=>BasketballDefenseRules.Window(BasketballBall.Active&&BasketballBall.Active.IsCharging(target)?BasketballBall.Active.Charge.pressure:BasketballBall.Active?BasketballBall.Active.ShotPressure(target,shotHoop):0);
  public float ShotDistance=>shotHoop&&target?Vector3.ProjectOnPlane(shotHoop.position-target.transform.position,Vector3.up).magnitude:4;
  public float ShotSweepSeconds=>BasketballBall.ChargePeriod(ShotDistance);
  public float ShotPower {get{if(!ShotCharging)return 0;var ball=BasketballBall.Active;return BasketballBall.PowerAtPhase(ball&&ball.IsCharging(target)?ball.PresentedChargePhase:shotPhase+(BasketballMotion.Clock-shotSampled)/ShotSweepSeconds);}}
  public bool BeginShot(int source=int.MinValue){if(!TutorialSession.Allows("shoot"))return false;
   if(InputBlocked||pendingTargetPass||ShotCharging||PassCharging||pendingPass||pendingPassBegin||!active||!target||!BasketballBall.Active||!BasketballBall.Active.CanShoot(target))return false;
   shotHoop=BasketballBall.Active.SelectHoop(target.transform.position,yaw,target);if(!shotHoop)return false;
   ShotCharging=true;ShotFinish=BasketballFinish.Shot;shotPhase=0;shotSampled=BasketballMotion.Clock;shotSource=source;pendingShotBegin=true;return true;
  }
  public void EndShot(int source=int.MinValue){if(!ShotCharging||source!=shotSource)return;pendingFinish=ShotFinish;var ball=BasketballBall.Active;pendingShotReleasedAt=ball&&ball.IsCharging(target)?ball.PresentedChargeTime:BasketballMotion.Clock;ShotCharging=false;pendingShoot=true;}
  public void CancelShot(int source=int.MinValue){if(ShotCharging&&(source==int.MinValue||source==shotSource)){ShotCharging=false;pendingShotCancel=true;}}
  public void RequestPass(){if(BeginPass())EndPass();}
  bool pendingSteal;int stealSource;float repeatStealAt;
  public bool StealHeld {get;private set;}
  public bool BeginSteal(int source=int.MinValue){
   if(StealHeld||AutoBlockHeld||InputBlocked||!active||!target||!BasketballBall.Active||!BasketballBall.Active.CanSteal(target))return false;
   StealHeld=true;stealSource=source;pendingSteal=true;repeatStealAt=Time.unscaledTime+BasketballStealRules.Repeat;return true;
  }
  public void EndSteal(int source=int.MinValue,bool cancel=false){if(!StealHeld||source!=stealSource)return;StealHeld=false;if(cancel)pendingSteal=false;}
  public void RequestCartToggle(){if(!TutorialSession.Allows("buggy"))return;
   if(InputBlocked||!active||GolfAimRequested||!target||target.inTransit||!GolfCartWorld.Allowed)return;
   pendingCart=GolfCartWorld.HasCart(target)?GolfCartAction.Recall:GolfCartAction.Summon;pendingCartOwner=GolfCartWorld.Key(target);
  }
  public void RequestCartUse(){if(!TutorialSession.Allows("drive"))return;
   if(InputBlocked||!active||GolfAimRequested||!target||target.inTransit||!GolfCartWorld.Allowed)return;
   var driving=GolfCartWorld.Driving(target);var cart=driving?driving:GolfCartWorld.Nearest(target);if(!cart)return;
   pendingCart=driving?GolfCartAction.Leave:GolfCartAction.Drive;pendingCartOwner=cart.Owner;
  }
  void Update(){
   if(UpdateGolfReplayInput())return;
   if(!active||InputBlocked||!target||target.inTransit||FootballMatch.BlocksActions&&!FootballMatch.CanAimKickoff(target)||GolfMatchManager.BlocksMovement||FishingHUD.Instance&&FishingHUD.Instance.BlocksGameplay){CancelInput();return;}
   if(EventSystem.current&&EventSystem.current.currentSelectedGameObject){var field=EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.InputField>();if(field&&field.isFocused){CancelInput();return;}}
   UpdateTargetPassInput();UpdateFishingTargetInput();UpdateCountersteerKeyboard();
   UpdateFootballInput();UpdateDefenseInput();UpdateGolfAim();
   if(Input.GetKeyDown(KeyCode.G))ToggleGolfAim();
   if(!FootballBall.Allowed&&!FootballMatch.CanAimKickoff(target)){Charging=false;pendingKick=false;}
   if(Charging&&(!target.KickReady||!SportsPossession.Attacking(target,SportId.Football)))Charging=false;
   if(GolfCharging&&(!GolfMatchManager.Instance||!GolfMatchManager.Instance.CanStrike(target,GolfMatchManager.Instance.Ball(chargingGolfOwner))))GolfCharging=false;
   if(FootballPassAiming&&(!FootballBall.Instance||!FootballBall.Instance.CanPass(target)))CancelFootballPass();
   if(AppRoot.Instance.SelectedSport==SportId.Football){if(Input.GetKeyDown(KeyCode.Space))BeginFootballPass();if(Input.GetKeyUp(KeyCode.Space))EndFootballPass();}
   if(AppRoot.Instance.SelectedSport!=SportId.Football&&Input.GetKeyDown(KeyCode.Space)&&target.CanRequestJump){FootballAudio.PlayMenu();Charging=false;RequestJump();}
   if(ShotCharging&&(!BasketballBall.Active||!BasketballBall.Active.CanShoot(target)))CancelShot();
   if(ShotCharging){double now=BasketballMotion.Clock;shotPhase+=System.Math.Max(0,now-shotSampled)/ShotSweepSeconds;shotSampled=now;}
   if(PassCharging&&(!BasketballBall.Active||!BasketballBall.Active.CanPass(target)||passPlay!=BasketballBall.Active.Defense.play))CancelPass();
   if(Input.GetKeyDown(KeyCode.X)){CancelFootballPass();CancelShot();CancelPass();CancelFishing();CancelGolfAim();}
   if(StealHeld&&(!BasketballBall.Active||!BasketballBall.Active.CanSteal(target)))EndSteal(stealSource,true);
   if(Input.GetKeyDown(KeyCode.R))RequestCartToggle();
   if(Input.GetKeyDown(KeyCode.E)){if(target.TackleReady)FootballAudio.PlayMenu();Charging=false;if(GolfCartWorld.Allowed)RequestCartUse();else if(BasketballRole==WhatTheFish.BasketballRole.Defense)RequestBlock();else {RequestTackle();BeginShot();}}
   if(ShotCharging){if(Input.GetKeyDown(KeyCode.UpArrow))SelectFinish(BasketballFinish.Dunk);if(Input.GetKeyDown(KeyCode.DownArrow))SelectFinish(BasketballFinish.Layup);if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.RightArrow))SelectFinish(BasketballFinish.Shot);}
   if(Input.GetKeyUp(KeyCode.E))EndShot();
   if(PassCharging){if(Input.GetKeyDown(KeyCode.UpArrow))SelectPass(1);if(Input.GetKeyDown(KeyCode.DownArrow))SelectPass(-1);if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.RightArrow))SelectPass(0);}
   if(Input.GetKeyDown(KeyCode.Q)){if(BasketballRole==WhatTheFish.BasketballRole.Defense)BeginGuard();else BeginPass();}if(Input.GetKeyUp(KeyCode.Q)){EndGuard();EndPass();}
   if(GuardHeld&&guardSource==int.MinValue&&Input.GetKey(KeyCode.Q))SetGuardBlock(Input.GetKey(KeyCode.UpArrow));
   if(Input.GetKeyDown(KeyCode.F)){if(AppRoot.Instance.SelectedSport==SportId.Basketball)BeginSteal();else if(AppRoot.Instance.SelectedSport==SportId.Golf){}else if(AppRoot.Instance.SelectedSport==SportId.Fishing)BeginFishing();else BeginKick();}if(Input.GetKeyUp(KeyCode.F)){EndSteal();EndGolfSwing();EndKick();EndFishing();}
  }
  public void Switch(){if(GolfAimRequested||FishingGame.Instance&&FishingGame.Instance.Busy(target))return;if(!GolfFishingAudio.PlayMenu("ui-select"))FootballAudio.PlayMenu(FootballSound.UISelect);bool fishing=FishingGame.Instance&&FishingGame.Instance.Context;mode=(mode+1)%3;if(!fishing){PlayerPrefs.SetInt("camera",mode);PlayerPrefs.Save();}}
  public PlayerCommand ReadCommand(){
   if(target&&target.IsSpectator){
    if(!active||InputBlocked||target.inTransit)return default;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    if(DevelopmentProbe.TurnCommandActive){var probe=DevelopmentProbe.TurnCommand;return new PlayerCommand{move=probe.move,heading=probe.heading,sprint=probe.sprint};}
#endif
    var touch=stick?stick.value:Vector2.zero;return new PlayerCommand{move=Vector2.ClampMagnitude(touch+new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")),1),heading=yaw,sprint=Input.GetKey(KeyCode.LeftShift)||touch.magnitude>.8f};
   }
   var command=ReadInput();AddTargetPassInput(ref command);if(GolfAimRequested||GolfMatchManager.BlocksMovement){command.move=Vector2.zero;command.jump=command.sprint=false;command.cartAction=GolfCartAction.None;}if(command.golfSwing)command.heading=pendingGolfHeading;return TutorialSession.Filter(command);
  }
  PlayerCommand ReadInput(){
   if(InputBlocked){CancelInput();var canceled=new PlayerCommand{heading=yaw,shotCancel=pendingShotCancel,passCancel=pendingPassCancel};pendingShotCancel=pendingPassCancel=false;AddFishingInput(ref canceled);return canceled;}
   if(FootballMatch.CanAimKickoff(target)){bool released=pendingKick;pendingKick=false;return new PlayerCommand{heading=yaw,kick=released,kickCharge=pendingCharge,charging=Charging};}
   if(FootballMatch.BlocksMovement||GolfMatchManager.BlocksMovement||FishingHUD.Instance&&FishingHUD.Instance.BlocksGameplay){CancelInput();var canceled=new PlayerCommand();AddFishingInput(ref canceled);return canceled;}
   if(FootballMatch.BlocksActions)CancelInput();
   if(StealHeld&&Time.unscaledTime>=repeatStealAt){pendingSteal=true;repeatStealAt=Time.unscaledTime+BasketballStealRules.Repeat;}
   bool steal=pendingSteal;pendingSteal=false;
   bool tackle=pendingTackle,jump=pendingJump,shoot=pendingShoot,pass=pendingPass,kick=pendingKick,shotBegin=pendingShotBegin,shotCancel=pendingShotCancel;float charge=pendingCharge;pendingTackle=pendingJump=pendingShoot=pendingPass=false;pendingKick=pendingShotBegin=pendingShotCancel=false;
   var cartAction=pendingCart;var cartOwner=pendingCartOwner;pendingCart=GolfCartAction.None;
   bool golfSwing=pendingGolf;var golfOwner=GolfCharging?chargingGolfOwner:pendingGolfOwner;var golfRound=golfSwing?pendingGolfRound:GolfMatchManager.Instance?GolfMatchManager.Instance.Round:0;float golfCharge=pendingGolfCharge;pendingGolf=false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(DevelopmentProbe.TurnCommandActive){var command=DevelopmentProbe.TurnCommand;command.tackle|=tackle;command.jump|=jump;command.shoot|=shoot;command.shotReleasedAt=pendingShotReleasedAt;command.finish=shoot?pendingFinish:command.finish;command.shotBegin|=shotBegin;command.shotCancel|=shotCancel;command.pass|=pass;command.steal|=steal;command.charging|=Charging;command.cartAction=cartAction;command.cartOwner=cartOwner;command.golfCharging=GolfCharging;command.golfRound=golfRound;command.golfBallOwner=golfOwner;if(kick){command.kick=true;command.kickCharge=charge;}if(golfSwing){command.golfSwing=true;command.golfBallOwner=golfOwner;command.golfCharge=golfCharge;command.golfRound=golfRound;}AddFootballInput(ref command);AddDefenseInput(ref command);AddPassInput(ref command);AddFishingInput(ref command);return command;}
   if(DevelopmentProbe.IslandDriving)return new PlayerCommand{move=Vector2.up,heading=DevelopmentProbe.IslandHeading,sprint=true};
   if(DevelopmentProbe.Driving)return new PlayerCommand{move=Vector2.up,heading=DevelopmentProbe.Heading,sprint=false};
#endif
   var touch=stick?stick.value:Vector2.zero;var v=touch+new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));var result=new PlayerCommand{move=Vector2.ClampMagnitude(StadiumMovement(v),1),heading=yaw,tackle=tackle,jump=jump,shoot=shoot,finish=pendingFinish,shotReleasedAt=pendingShotReleasedAt,shotBegin=shotBegin,shotCancel=shotCancel,pass=pass,steal=steal,kick=kick,kickCharge=charge,charging=Charging,sprint=Input.GetKey(KeyCode.LeftShift)||touch.magnitude>.8f,cartAction=cartAction,cartOwner=cartOwner,golfSwing=golfSwing,golfCharging=GolfCharging,golfBallOwner=golfOwner,golfCharge=golfCharge,golfRound=golfRound};AddFootballInput(ref result);AddDefenseInput(ref result);AddPassInput(ref result);AddFishingInput(ref result);return result;}
  void LateUpdate(){
   ObserveFishingArrival();
   if(TeleportSequence.OwnsCamera){LookDelta=Vector2.zero;HideKickAim();return;}
   if(FootballReplay.Presenting||FootballMVP.Presenting||BasketballMVP.Presenting||FishingCeremony.Presenting){LookDelta=Vector2.zero;HideKickAim();return;}
   UpdateGolfAim();
   if(!active||!target){ResetGolfShotCamera();ResetFishingFightCamera();ResetStadiumCamera();ResetActivityStadiumCamera();HideKickAim();return;}
   bool fishingModal=FishingHUD.Instance&&FishingHUD.Instance.BlocksGameplay;
   if(!InputBlocked&&!fishingModal&&Input.GetKeyDown(KeyCode.C))Switch();
   if(!InputBlocked&&!fishingModal&&Input.GetMouseButton(1)){LookDelta+=new Vector2(Input.GetAxis("Mouse X")*14,Input.GetAxis("Mouse Y")*14);}
   if(fishingModal||FishingFightLocksLook||mode==2&&FishingGame.Instance&&FishingGame.Instance.Busy(target)||!TutorialSession.Allows("look"))LookDelta=Vector2.zero;
   yaw+=LookDelta.x*.13f;
   if(GolfAiming&&!InputBlocked){float turn=(stick?stick.value.x:0)+Input.GetAxisRaw("Horizontal");yaw+=Mathf.Clamp(turn,-1,1)*65*Time.deltaTime;}
   pitch=Mathf.Clamp(pitch-LookDelta.y*.10f,-25,65);LookDelta=Vector2.zero;
   if(!StadiumField(out _,out _,out _))ResetStadiumCamera();
   if(!ActivityStadiumContext)ResetActivityStadiumCamera();
   if(SkySailWorld.Instance&&SkySailWorld.Instance.RideCamera(cam,yaw,pitch,mode)){ResetGolfShotCamera();HideKickAim();return;}
   var cart=GolfCartWorld.Driving(target);if(cart&&GolfCartWorld.Allowed){HideKickAim();return;}
   if(StadiumField(out _,out _,out _))return;
   if(ActivityStadiumContext)return;
   bool golf=GolfAiming;
   golfFraming=golf?Mathf.MoveTowards(golfFraming,1,4*Time.deltaTime):0;
   int viewMode=golf?1:mode;
   Vector3 focus=target.transform.position+Vector3.up*(viewMode==0?1.57f:Mathf.Lerp(1.35f,1,golfFraming));
   if(golf){var ball=GolfMatchManager.Instance.Ball(aimOwner);if(ball)focus=Vector3.Lerp(focus,ball.Body.position+Vector3.up*.35f,golfFraming);}
   FishingCameraFocus(ref focus);
   bool fishingWide=mode==2&&FishingGame.Instance&&FishingGame.Instance.Context;var fishingLayout=FishingHUD.Instance?FishingHUD.Instance.Settings:null;
   float widePitch=fishingLayout?Mathf.Clamp(pitch,fishingLayout.islandPitchRange.x,fishingLayout.islandPitchRange.y):Mathf.Clamp(pitch,0,35);
   var rotation=Quaternion.Euler(golf?Mathf.Clamp(pitch,20,55):fishingWide?widePitch:mode==2?58:pitch,yaw,0);float distance=viewMode==0?0:viewMode==1?Mathf.Lerp(5,6,golfFraming):fishingWide?(fishingLayout?fishingLayout.islandViewDistance:7):AppRoot.Instance.environments.Current.elevatedDistance;
   Vector3 desired=focus-rotation*Vector3.forward*distance+Quaternion.Euler(0,yaw,0)*Vector3.right*(golfShoulderOffset*golfFraming);
   var cameraPath=desired-focus;
   if(distance>0&&Physics.SphereCast(focus,.2f,cameraPath.normalized,out var hit,cameraPath.magnitude,1<<8,QueryTriggerInteraction.Ignore))desired=focus+cameraPath.normalized*Mathf.Max(.35f,hit.distance-.15f);
   target.HideHead(viewMode==0||Vector3.Distance(desired,focus)<1.15f);
   PresentGolfShotCamera(ref desired,ref rotation);
   PresentFishingFightCamera(ref desired,ref rotation);
   if(GolfShotCameraActive)target.HideHead(Vector3.Distance(desired,target.transform.position+Vector3.up*1.57f)<.85f);
   if(FishingFightCameraActive)target.HideHead(Vector3.Distance(desired,target.transform.position+Vector3.up*1.57f)<.85f);
   cam.nearClipPlane=viewMode==0&&!GolfShotCameraActive&&!FishingFightCameraActive?.06f:.15f;transform.SetPositionAndRotation(desired,rotation);
   UpdateKickAim();UpdateGolfPreview();UpdateFishingAim();
  }
  // Follow the displayed seat position, while only player look input rotates
  // the view. Steering, coasting and reversing never change the chosen angle.
  public void UpdateCartCamera(GolfCart cart){
   if(!isActiveAndEnabled||!active||!target||target.inTransit||!GolfCartWorld.Allowed||GolfCartWorld.Driving(target)!=cart)return;
   ResetGolfShotCamera();target.HideHead(mode==0);cart.Camera(cam,yaw,pitch,mode);
  }
 }
}
