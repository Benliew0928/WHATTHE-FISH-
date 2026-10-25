using UnityEngine;
using UnityEngine.UI;

namespace WhatTheFish {
 // Presentation reads accepted, clocked snapshots. It never pauses a round or
 // changes an ability, shot, inventory, or the player's gameplay heading.
 [DefaultExecutionOrder(180)]
 public sealed class ActivityCinematic:MonoBehaviour {
  public const float Duration=2.6f;
  public static ActivityCinematic Instance {get;private set;}
  PlayerView view;Canvas canvas;CanvasGroup fade;RectTransform card,spotlight;Image icon,bar;Text title,detail,kicker;ActivityCueGraphic motion;
  SportId context;Athlete owner;uint round,seen;double stamp,rushSeen,started;bool showing,rush,cameraCancelled;int cameraMode;
  FishingAbility skill;GolfEventKind weather;readonly Vector3[] corners=new Vector3[4];
  public bool Visible=>showing&&canvas&&canvas.enabled;
  public bool CameraActive {get;private set;}
  public bool BuddySpotlight=>Visible&&skill==FishingAbility.FishingBuddy&&spotlight.gameObject.activeSelf;
  public string Title=>title?title.text:"";
  public float Age {get;private set;}
  public FishingAbility Skill=>skill;
  void Awake(){
   Instance=this;view=GetComponent<PlayerView>();
   canvas=new GameObject("Activity cinematic overlay",typeof(RectTransform)).AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=4;
   var plate=BasketballUIStyle.Plate(canvas.transform,"Activity announcement",0,0,366,98,new Color(.025f,.09f,.14f,.95f),14,2);plate.raycastTarget=false;card=plate.rectTransform;card.pivot=new Vector2(.5f,0);
   fade=card.gameObject.AddComponent<CanvasGroup>();fade.blocksRaycasts=fade.interactable=false;
   var art=CoveUI.Rect(card,"Animated event emblem",8,10,76,76);motion=art.gameObject.AddComponent<ActivityCueGraphic>();motion.raycastTarget=false;
   icon=CoveUI.Rect(card,"Skill emblem",15,17,62,62).gameObject.AddComponent<Image>();icon.preserveAspect=true;icon.raycastTarget=false;
   kicker=CoveUI.Text(card,"",94,9,258,17,11,true,new Color(.6f,.86f,.92f));
   title=CoveUI.Text(card,"",94,26,258,31,22,true,Color.white);title.font=SportsUIStyle.DisplayFont;
   detail=CoveUI.Text(card,"",94,59,258,27,13,false,new Color(.84f,.92f,.96f));detail.font=SportsUIStyle.TextFont;
   bar=CoveUI.Rect(card,"Activation progress",14,92,338,3).gameObject.AddComponent<Image>();bar.raycastTarget=false;
   spotlight=CoveUI.Rect(canvas.transform,"Active skill spotlight",0,0,100,100);spotlight.pivot=Vector2.zero;var focus=spotlight.gameObject.AddComponent<ActivityFocusOutline>();focus.raycastTarget=false;
   CharacterIdentityBadges.ReserveHUD(card);canvas.enabled=false;spotlight.gameObject.SetActive(false);
  }
  void ResetContext(){showing=false;CameraActive=false;seen=0;stamp=rushSeen=0;round=0;owner=null;if(canvas)canvas.enabled=false;}
  bool Allowed(){var app=AppRoot.Instance;return app&&app.Exploring&&view&&view.isActiveAndEnabled&&view.active&&view.target&&!view.target.inTransit&&!TeleportSequence.OwnsCamera&&!(SkySailWorld.Instance&&SkySailWorld.Instance.Travelling);}
  void Begin(double at,FishingAbility fishing,GolfEventKind golf,bool finalRush=false){started=at;skill=fishing;weather=golf;rush=finalRush;showing=true;cameraCancelled=false;cameraMode=view.mode;}
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
   Age=Mathf.Max(0,(float)(now-started));if(!showing||Age>=Duration){showing=false;return;}
   if(view.InputBlocked||SocialPanel.Visible||AccountPanel.Visible||IslandTutorialUI.ModalOpen||sport==SportId.Fishing&&FishingHUD.Instance&&FishingHUD.Instance.BlocksGameplay){showing=false;return;}
   if(view.mode!=cameraMode)cameraCancelled=true;
   float enter=Mathf.SmoothStep(0,1,Age/.38f),leave=Mathf.SmoothStep(0,1,Mathf.InverseLerp(2,Duration,Age));
   var tint=sport==SportId.Fishing?FishingAbilityRules.Color(skill):EventColor(weather,rush);
   float scale=Mathf.Min(Mathf.Clamp(Screen.height/720f,.65f,2),(Screen.safeArea.width-24)/366);
   canvas.enabled=true;fade.alpha=enter*(1-leave);card.localScale=Vector3.one*scale;
   card.anchoredPosition=new Vector2(Screen.safeArea.center.x+(sport==SportId.Golf?-24:0)*scale+(MenuPreferences.ReducedMotion?0:22*(1-enter)-12*leave)*scale,-(Screen.height-Screen.safeArea.yMax+244*scale));
   kicker.text=sport==SportId.Fishing?(skill==FishingAbility.FishingBuddy?"BUDDY JOINED":"SKILL ACTIVATED"):"COURSE EVENT · LIVE";
   title.text=sport==SportId.Fishing?FishingAbilityRules.Name(skill):rush?"FINAL RUSH":GolfEventHUD.Name(weather);
   detail.text=sport==SportId.Fishing?SkillDetail(skill):EventDetail(weather,rush);
   icon.gameObject.SetActive(sport==SportId.Fishing);if(icon.gameObject.activeSelf){icon.sprite=ControlArt.Skill(skill);icon.rectTransform.localScale=Vector3.one*(MenuPreferences.ReducedMotion?1:.88f+.12f*enter);}
   motion.weather=weather;motion.fishing=sport==SportId.Fishing;motion.rush=rush;motion.age=MenuPreferences.ReducedMotion?1:Age;motion.color=tint;motion.SetVerticesDirty();
   bar.color=tint;bar.rectTransform.sizeDelta=new Vector2(338*(1-Age/Duration),3);
   if(skill==FishingAbility.FishingBuddy&&sport==SportId.Fishing){var hud=FishingHUD.Instance?FishingHUD.Instance.GetComponent<FishingAbilityHUD>():null;var button=hud?hud.SkillButton:null;if(button&&button.gameObject.activeInHierarchy){((RectTransform)button.transform).GetWorldCorners(corners);float pad=(7+(MenuPreferences.ReducedMotion?0:3*Mathf.Sin(Age*5)))*scale;spotlight.gameObject.SetActive(true);spotlight.anchoredPosition=new Vector2(corners[0].x-pad,corners[0].y-Screen.height-pad);spotlight.sizeDelta=new Vector2(corners[2].x-corners[0].x+2*pad,corners[2].y-corners[0].y+2*pad);spotlight.localScale=Vector3.one;spotlight.GetComponent<ActivityFocusOutline>().color=new Color(tint.r,tint.g,tint.b,fade.alpha);}}
   CameraActive=!cameraCancelled&&!MenuPreferences.ReducedMotion&&!owner.IsSpectator&&!(sport==SportId.Golf&&(view.GolfAiming||view.GolfCharging||view.GolfShotCameraActive||GolfCartWorld.Driving(owner)));
  }
  public bool CameraPose(out Vector3 focus,out Vector3 position,out float weight){
   focus=position=default;weight=0;if(!CameraActive||!owner)return false;
   weight=Mathf.SmoothStep(0,1,Age/.5f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.85f,Duration,Age)));
   var origin=owner.transform.position;var forward=owner.transform.forward;var right=owner.transform.right;
   if(context==SportId.Fishing){
    var game=FishingGame.Instance;var p=game.Player(owner);if(!p.HasValue)return false;
    var hook=p.Value.fish>=0?game.Presentation.HookPoint(p.Value):origin+forward*7;
    var direction=Vector3.ProjectOnPlane(hook-origin,Vector3.up);if(direction.sqrMagnitude>.1f){forward=direction.normalized;right=Vector3.Cross(Vector3.up,forward);}
    if(skill==FishingAbility.FishingBuddy){focus=origin+right*.7f+Vector3.up*1.2f;position=focus-forward*5+right*2.4f+Vector3.up*1.8f;}
    else if(skill==FishingAbility.Sonar){focus=origin+forward*6;position=origin-forward*3+Vector3.up*8+right*(.6f*Mathf.Sin(Age));}
    else if(skill==FishingAbility.SchoolCall){focus=p.Value.schoolPoint+Vector3.up*.6f;position=origin-forward*2+right*1.5f+Vector3.up*5;}
    else {float mix=skill==FishingAbility.BubbleGuard?.8f:skill==FishingAbility.SteelWire?.48f:.24f;focus=Vector3.Lerp(origin+Vector3.up*1.5f,hook+Vector3.up*.35f,mix);position=origin-forward*3.5f+right*(skill==FishingAbility.PowerReel?2:3)+Vector3.up*3;}
   }else{
    var game=GolfMatchManager.Instance;focus=origin+forward*9+Vector3.up;
    var world=game.GetComponent<GolfHazardWorld>();if(GolfHazardRules.IsHazard(weather)&&world){GolfHazardWorld.Lane nearest=null;float distance=float.MaxValue;foreach(var lane in world.Lanes){float d=(lane.centre-origin).sqrMagnitude;if(d<distance){distance=d;nearest=lane;}}if(nearest!=null&&distance<2500)focus=world.Position(nearest,game.Events.Snapshot,game.Now)+Vector3.up*(weather==GolfEventKind.Tornado?2:1);}
    var direction=Vector3.ProjectOnPlane(focus-origin,Vector3.up);if(direction.sqrMagnitude>.1f)forward=direction.normalized;
    position=origin-forward*6+Vector3.up*(rush?5:7);
   }
   var path=position-focus;if(path.sqrMagnitude>.01f&&Physics.SphereCast(focus,.2f,path.normalized,out var hit,path.magnitude,1<<8,QueryTriggerInteraction.Ignore))position=focus+path.normalized*Mathf.Max(.4f,hit.distance-.15f);
   return true;
  }
  static Color EventColor(GolfEventKind kind,bool rush)=>rush?CoveUI.Gold:kind==GolfEventKind.StickyGreen?new Color(.38f,1,.52f):kind==GolfEventKind.BounceParty?new Color(1,.65f,.24f):kind==GolfEventKind.Tornado?new Color(.62f,.7f,1):kind==GolfEventKind.CrabCrossing?new Color(1,.45f,.34f):new Color(.4f,.88f,1);
  static string SkillDetail(FishingAbility kind)=>kind switch{FishingAbility.SteelWire=>"Reinforced line · tension protected",FishingAbility.PowerReel=>"Power up · reel 55% faster",FishingAbility.FishingBuddy=>"Your helper is here · reel together",FishingAbility.BubbleGuard=>"One strong surge absorbed",FishingAbility.Sonar=>"Wanted fish revealed",_=>"Five fish are swimming closer"};
  static string EventDetail(GolfEventKind kind,bool rush)=>rush?"Move faster · finish your route!":kind switch{GolfEventKind.TropicalWind=>"Wind now pushes airborne shots",GolfEventKind.StickyGreen=>"Swing shots stop on landing",GolfEventKind.BounceParty=>"Swing shots bounce higher",GolfEventKind.Tornado=>"Watch for twisters on the course",_=>"Crabs are crossing the fairways"};
  void OnDisable(){ResetContext();if(Instance==this)Instance=null;}
  void OnEnable()=>Instance=this;
  void OnDestroy(){if(canvas)Destroy(canvas.gameObject);if(Instance==this)Instance=null;}
 }
 public sealed class ActivityFocusOutline:MaskableGraphic {
  protected override void OnPopulateMesh(VertexHelper v){v.Clear();var r=rectTransform.rect;float length=Mathf.Min(r.width,r.height)*.22f;foreach(int x in new[]{-1,1})foreach(int y in new[]{-1,1}){var p=r.center+new Vector2(x*r.width,y*r.height)*.5f;BasketballUIMesh.Line(v,p,p-new Vector2(x*length,0),color,3);BasketballUIMesh.Line(v,p,p-new Vector2(0,y*length),color,3);}}
 }
 public sealed class ActivityCueGraphic:MaskableGraphic {
  public GolfEventKind weather;public bool fishing,rush;public float age;
  protected override void OnPopulateMesh(VertexHelper v){
   v.Clear();var centre=rectTransform.rect.center;float scale=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)/76;
   void Line(Vector2 a,Vector2 b,float width=3)=>BasketballUIMesh.Line(v,centre+a*scale,centre+b*scale,color,width*scale);
   void Dot(Vector2 p,float radius)=>BasketballUIMesh.Dot(v,centre+p*scale,radius*scale,color);
   void Ring(float radius,float start=0,float arc=1){for(int i=0;i<48;i++){float a=start+i/48f*arc*Mathf.PI*2,b=start+(i+1)/48f*arc*Mathf.PI*2;Line(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,2);}}
   if(fishing){Ring(35,age*1.5f,.28f);Ring(35,age*1.5f+Mathf.PI,.28f);return;}
   if(rush){for(int i=0;i<3;i++){float x=-24+i*20;Line(new Vector2(x-8,-16),new Vector2(x+6,0),5);Line(new Vector2(x+6,0),new Vector2(x-8,16),5);}return;}
   if(weather==GolfEventKind.TropicalWind){for(int i=0;i<4;i++){float y=-22+i*14;Line(new Vector2(-30+5*Mathf.Sin(age*3+i),y),new Vector2(25,y),3);Dot(new Vector2(26,y+3),3);}return;}
   if(weather==GolfEventKind.StickyGreen){Line(new Vector2(-31,-23),new Vector2(31,-23),5);Dot(new Vector2(0,Mathf.Lerp(22,-15,Mathf.SmoothStep(0,1,Mathf.Repeat(age*.6f,1)))),8);for(int i=0;i<3;i++)Dot(new Vector2((i-1)*20,-25),5);return;}
   if(weather==GolfEventKind.BounceParty){Line(new Vector2(-31,-25),new Vector2(31,-25),2);float t=Mathf.Repeat(age*.7f,1);Dot(new Vector2(Mathf.Lerp(-26,26,t),-14+40*Mathf.Sin(t*Mathf.PI)),8);return;}
   if(weather==GolfEventKind.Tornado){for(int i=0;i<6;i++){float y=-27+i*10,x=Mathf.Sin(age*3+i*.4f)*4,w=5+i*4;Line(new Vector2(x-w,y),new Vector2(x+w,y),4);}return;}
   Dot(Vector2.zero,16);for(int side=-1;side<=1;side+=2){for(int i=0;i<3;i++){float y=-12+i*9;Line(new Vector2(side*12,y),new Vector2(side*28,y-7+Mathf.Sin(age*8+i)*3));}Line(new Vector2(side*12,10),new Vector2(side*27,24),4);Dot(new Vector2(side*28,24),6);}Dot(new Vector2(-7,17),4);Dot(new Vector2(7,17),4);
  }
 }
}
