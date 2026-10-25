using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Presentation reads accepted, clocked snapshots. It never pauses a round or
 // changes an ability, shot, inventory, or the player's gameplay heading.
 [DefaultExecutionOrder(180)]
 public sealed class ActivityCinematic:MonoBehaviour {
  public const float GolfDuration=3.5f,FishingDuration=2.6f;
  float Duration=>context==SportId.Golf?GolfDuration:FishingDuration;
  public static bool WatchesGolfEvent(GolfEventKind kind)=>GolfEventSchedule.IsRandom(kind);
  public static ActivityCinematic Instance {get;private set;}
  readonly GolfEventShowcase showcase=new();readonly GolfRushShowcase rushShowcase=new();
  public bool RushDemonstrationVisible=>rushShowcase.Visible;
  public int RushRunnerCount=>rushShowcase.RunnerCount;
  public bool DemonstrationVisible=>showcase.Visible;
  PlayerView view;Canvas canvas;RectTransform spotlight;
  SportId context;Athlete owner;uint round,seen;double stamp,rushSeen,started;bool showing,rush,cameraCancelled;int cameraMode;
  FishingAbility skill;GolfEventKind weather;readonly Vector3[] corners=new Vector3[4];
  public bool Visible=>showing;
  public bool AnnouncementVisible=>false;
  public bool CameraActive {get;private set;}
  public bool BuddySpotlight=>Visible&&skill==FishingAbility.FishingBuddy&&spotlight.gameObject.activeSelf;
  public string Title=>context==SportId.Fishing?FishingAbilityRules.Name(skill):rush?"FINAL RUSH":GolfEventHUD.Name(weather);
  public float Age {get;private set;}
  public FishingAbility Skill=>skill;
  void Awake(){
   Instance=this;view=GetComponent<PlayerView>();
   canvas=new GameObject("Activity cinematic overlay",typeof(RectTransform)).AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=4;
   spotlight=CoveUI.Rect(canvas.transform,"Active skill spotlight",0,0,100,100);spotlight.pivot=Vector2.zero;var focus=spotlight.gameObject.AddComponent<ActivityFocusOutline>();focus.raycastTarget=false;
   gameObject.AddComponent<GolfCinematicHUD>();canvas.enabled=false;spotlight.gameObject.SetActive(false);
  }
  void ResetContext(){showcase.Dispose();rushShowcase.Dispose();showing=false;CameraActive=false;seen=0;stamp=rushSeen=0;round=0;owner=null;if(canvas)canvas.enabled=false;}
  bool Allowed(){var app=AppRoot.Instance;return app&&app.Exploring&&view&&view.isActiveAndEnabled&&view.active&&view.target&&!view.target.inTransit&&!TeleportSequence.OwnsCamera&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling);}
  void Begin(double at,FishingAbility fishing,GolfEventKind golf,bool finalRush=false){showcase.Dispose();rushShowcase.Dispose();started=at;skill=fishing;weather=golf;rush=finalRush;showing=true;cameraCancelled=context==SportId.Golf&&(!finalRush&&!WatchesGolfEvent(golf));cameraMode=view.mode;}
  void LateUpdate(){
   CameraActive=false;canvas.enabled=false;spotlight.gameObject.SetActive(false);
   if(!Allowed()){ResetContext();return;}
   var sport=AppRoot.Instance.SelectedSport;if(sport!=SportId.Fishing&&sport!=SportId.Golf){ResetContext();return;}
   if(context!=sport||owner!=view.target){ResetContext();context=sport;owner=view.target;}
   double now;
   if(sport==SportId.Fishing){
    var game=FishingGame.Instance;var record=game?game.Player(owner):null;
    if(!game||!game.Context||!game.State.Running||!record.HasValue||!record.Value.connected||owner.IsSpectator){ResetContext();return;}
    now=game.Now;var p=record.Value;
    if(round!=game.State.Match.round){showing=false;round=game.State.Match.round;seen=0;stamp=0;}
    // A clock-corrected snapshot can arrive just before its activation time.
    // Consume it only when due so that a future packet cannot lose its cue.
    if(now>=p.activeAt&&(seen!=p.itemSequence||stamp!=p.activeAt)){seen=p.itemSequence;stamp=p.activeAt;if(FishingAbilityRules.Valid(p.active)&&now-p.activeAt<Duration)Begin(p.activeAt,p.active,GolfEventKind.None);}
    // Ending a fight or cancelling a skill must not leave a camera detour.
    if(showing&&(p.active!=skill||now>=p.activeUntil))showing=false;
   }else{
    var game=GolfMatchManager.Instance;if(!game||!game.Context||!game.State.Running){ResetContext();return;}now=game.Now;var e=game.Events.Snapshot;
    if(round!=game.Round){showing=false;round=game.Round;seen=0;stamp=rushSeen=0;}
    // Observe at activation, not during the ten-second warning. A changed
    // sequence also distinguishes repeated event types and late snapshots.
    if(game.Events.Active(now)!=GolfEventKind.None&&(seen!=e.sequence||stamp!=e.start)){seen=e.sequence;stamp=e.start;if(now-e.start<Duration)Begin(e.start,FishingAbility.None,e.kind);}
    if(game.Events.RushActive(now)&&rushSeen!=e.rushAt){rushSeen=e.rushAt;if(now-e.rushAt<Duration)Begin(e.rushAt,FishingAbility.None,GolfEventKind.None,true);}
    if(showing&&(rush?!game.Events.RushActive(now):game.Events.Active(now)!=weather))showing=false;
   }
   Age=Mathf.Max(0,(float)(now-started));if(!showing||Age>=Duration){showing=false;showcase.Dispose();rushShowcase.Dispose();return;}
   if(view.InputBlocked||SocialPanel.Visible||AccountPanel.Visible||IslandTutorialUI.ModalOpen||sport==SportId.Fishing&&FishingHUD.Instance&&FishingHUD.Instance.BlocksGameplay){showing=false;showcase.Dispose();rushShowcase.Dispose();return;}
   if(view.mode!=cameraMode)cameraCancelled=true;
   float enter=Mathf.SmoothStep(0,1,Age/.38f),leave=Mathf.SmoothStep(0,1,Mathf.InverseLerp(Duration-.6f,Duration,Age));
   if(skill==FishingAbility.FishingBuddy&&sport==SportId.Fishing){var hud=FishingHUD.Instance?FishingHUD.Instance.GetComponent<FishingAbilityHUD>():null;var button=hud?hud.SkillButton:null;if(button&&button.gameObject.activeInHierarchy){canvas.enabled=true;((RectTransform)button.transform).GetWorldCorners(corners);float pad=7;spotlight.gameObject.SetActive(true);spotlight.anchoredPosition=new Vector2(corners[0].x-pad,corners[0].y-Screen.height-pad);spotlight.sizeDelta=new Vector2(corners[2].x-corners[0].x+2*pad,corners[2].y-corners[0].y+2*pad);spotlight.GetComponent<ActivityFocusOutline>().color=new Color(.4f,.88f,1,enter*(1-leave));}}
   CameraActive=!cameraCancelled&&!MenuPreferences.ReducedMotion&&!owner.IsSpectator&&!(sport==SportId.Golf&&(view.GolfAiming||view.GolfCharging||view.GolfShotCameraActive||GolfCartWorld.Driving(owner)));
   if(CameraActive&&sport==SportId.Golf&&rush){
    showcase.Dispose();if(!rushShowcase.Present(Age,owner)){CameraActive=false;cameraCancelled=true;}
   }else if(CameraActive&&sport==SportId.Golf&&GolfEventShowcase.Supports(weather)){
    rushShowcase.Dispose();if(!showcase.Present(weather,Age,owner,GolfMatchManager.Instance)){CameraActive=false;cameraCancelled=true;}
   }else {showcase.Dispose();rushShowcase.Dispose();}
  }
  public bool CameraPose(out Vector3 focus,out Vector3 position,out float weight){
   focus=position=default;weight=0;if(!CameraActive||!owner)return false;
   weight=Mathf.SmoothStep(0,1,Age/.5f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(Duration-.75f,Duration,Age)));
   var origin=owner.transform.position;var forward=owner.transform.forward;var right=owner.transform.right;
   if(context==SportId.Fishing){
    var game=FishingGame.Instance;var p=game.Player(owner);if(!p.HasValue)return false;
    var hook=p.Value.fish>=0?game.Presentation.HookPoint(p.Value):origin+forward*7;
    var direction=Vector3.ProjectOnPlane(hook-origin,Vector3.up);if(direction.sqrMagnitude>.1f){forward=direction.normalized;right=Vector3.Cross(Vector3.up,forward);}
    if(skill==FishingAbility.FishingBuddy){focus=origin+right*.7f+Vector3.up*1.2f;position=focus-forward*5+right*2.4f+Vector3.up*1.8f;}
    else if(skill==FishingAbility.Sonar){focus=origin+forward*6;position=origin-forward*3+Vector3.up*8+right*(.6f*Mathf.Sin(Age));}
    else if(skill==FishingAbility.SchoolCall){focus=p.Value.schoolPoint+Vector3.up*.6f;position=origin-forward*2+right*1.5f+Vector3.up*5;}
    else {float mix=skill==FishingAbility.BubbleGuard?.8f:skill==FishingAbility.SteelWire?.48f:.24f;focus=Vector3.Lerp(origin+Vector3.up*1.5f,hook+Vector3.up*.35f,mix);position=origin-forward*3.5f+right*(skill==FishingAbility.PowerReel?2:3)+Vector3.up*3;}
   }else if(rushShowcase.Visible){rushShowcase.Camera(out focus,out position);
   }else if(showcase.Visible){showcase.Camera(Age,out focus,out position);
   }else{
    var game=GolfMatchManager.Instance;focus=origin+forward*9+Vector3.up;
    var world=game.GetComponent<GolfHazardWorld>();if(GolfHazardRules.IsHazard(weather)&&world){GolfHazardWorld.Lane nearest=null;float distance=float.MaxValue;foreach(var lane in world.Lanes){float d=(lane.centre-origin).sqrMagnitude;if(d<distance){distance=d;nearest=lane;}}if(nearest!=null&&distance<2500)focus=world.Position(nearest,game.Events.Snapshot,game.Now)+Vector3.up*(weather==GolfEventKind.Tornado?2:1);}
    var direction=Vector3.ProjectOnPlane(focus-origin,Vector3.up);if(direction.sqrMagnitude>.1f)forward=direction.normalized;
    position=origin-forward*6+Vector3.up*(rush?5:7);
   }
   var path=position-focus;if(path.sqrMagnitude>.01f&&Physics.SphereCast(focus,.2f,path.normalized,out var hit,path.magnitude,1<<8,QueryTriggerInteraction.Ignore))position=focus+path.normalized*Mathf.Max(.4f,hit.distance-.15f);
   return true;
  }
  void OnDisable(){ResetContext();if(Instance==this)Instance=null;}
  void OnEnable()=>Instance=this;
  void OnDestroy(){showcase.Dispose();rushShowcase.Dispose();if(canvas)Destroy(canvas.gameObject);if(Instance==this)Instance=null;}
 }
 public sealed class ActivityFocusOutline:MaskableGraphic {
  protected override void OnPopulateMesh(VertexHelper v){v.Clear();var r=rectTransform.rect;float length=Mathf.Min(r.width,r.height)*.22f;foreach(int x in new[]{-1,1})foreach(int y in new[]{-1,1}){var p=r.center+new Vector2(x*r.width,y*r.height)*.5f;BasketballUIMesh.Line(v,p,p-new Vector2(x*length,0),color,3);BasketballUIMesh.Line(v,p,p-new Vector2(0,y*length),color,3);}}
 }
}
