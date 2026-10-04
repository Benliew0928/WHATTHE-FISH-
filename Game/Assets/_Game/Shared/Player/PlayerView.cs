using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace WhatTheFish {
 [DefaultExecutionOrder(-100)]
 public sealed class PlayerView:MonoBehaviour,IPlayerCommandSource {
  public static Vector2 LookDelta; public static PlayerView Instance; public Athlete target; public TouchPad stick; public int mode; public float yaw=0,pitch=16; public bool active; Camera cam;bool pendingTackle,pendingJump,pendingShoot,pendingPass;
  void Awake(){Instance=this;cam=GetComponent<Camera>();mode=Mathf.Clamp(PlayerPrefs.GetInt("camera",1),0,2);}
  public void RequestTackle(){if(active&&target&&target.TackleReady)pendingTackle=true;}
  bool pendingKick;float pendingCharge,chargeStarted;int chargeSource;
  GolfCartAction pendingCart;ulong pendingCartOwner;
  bool pendingGolf;ulong pendingGolfOwner;uint pendingGolfRound;float pendingGolfCharge,golfStarted;ulong chargingGolfOwner;int golfSource;
  public bool GolfCharging {get;private set;}
  public ulong GolfBallOwner=>chargingGolfOwner;
  public float GolfCharge=>GolfCharging?Mathf.Clamp01((Time.time-golfStarted)/1.3f):0;
  public bool BeginGolfSwing(int source=int.MinValue){var match=GolfMatchManager.Instance;var ball=match?match.Strikeable(target):null;if(GolfCharging||!active||!ball)return false;chargingGolfOwner=ball.Owner;target.GolfClubMotion.Address(ball.Body.position);GolfCharging=true;golfSource=source;golfStarted=Time.time;return true;}
  public void EndGolfSwing(int source=int.MinValue){if(!GolfCharging||source!=golfSource)return;float charge=GolfCharge;GolfCharging=false;QueueGolfSwing(charge,chargingGolfOwner);}
  public void CancelGolfSwing(int source){if(GolfCharging&&source==golfSource)GolfCharging=false;}
  public void RequestGolfSwing(float charge){var match=GolfMatchManager.Instance;var ball=match?match.Strikeable(target):null;if(ball)QueueGolfSwing(charge,ball.Owner);}
  void QueueGolfSwing(float charge,ulong owner){var match=GolfMatchManager.Instance;if(!active||!match||!match.CanStrike(target,match.Ball(owner)))return;pendingGolf=true;pendingGolfOwner=owner;pendingGolfRound=match.Round;pendingGolfCharge=Mathf.Clamp01(charge);}
  [Header("Golf cart camera")]
  [Min(.01f)] public float cartReverseTurnTime=.45f;
  [Min(1)] public float cartReverseTurnSpeed=180;
  GolfCart followedCart;Vector2 cartMove;float previousCartHeading,cartReverseAngle,cartReverseVelocity;bool cartReversing;
  [Header("Golf match third-person framing")]
  [Min(0)] public float golfShoulderOffset=1.8f;
  [Min(1)] public float golfViewDistance=3.5f;
  float golfFraming;
  public bool GolfAiming {get;private set;}
  public bool CanAimGolf=>active&&target&&mode==1&&!target.inTransit&&GolfMatchManager.Instance&&GolfMatchManager.Instance.Context&&GolfMatchManager.Instance.State.Running&&!GolfCartWorld.Driving(target);
  public void ToggleGolfAim(){if(CanAimGolf)GolfAiming=!GolfAiming;}
  public bool Charging {get;private set;}
  MeshRenderer kickAim;Material kickAimMaterial;Mesh kickAimMesh;
  public float Charge=>Charging&&FootballBall.Instance?FootballBall.Instance.ChargeFraction(Time.time-chargeStarted):0;
  public void RequestKick(float charge=1){if(active&&target&&target.KickReady){pendingKick=true;pendingCharge=Mathf.Clamp01(charge);}}
  public bool BeginKick(int source=int.MinValue){if(Charging||!active||!target||!target.KickReady)return false;Charging=true;chargeSource=source;chargeStarted=Time.time;return true;}
  public void EndKick(int source=int.MinValue){if(!Charging||source!=chargeSource)return;float charge=Charge;Charging=false;HideKickAim();RequestKick(charge);}
  public void CancelKick(int source){if(Charging&&source==chargeSource){Charging=false;HideKickAim();}}
  public void ClearMatchInput(){CancelInput();GolfAiming=false;golfFraming=0;}
  void CancelInput(){Charging=GolfCharging=false;HideKickAim();pendingKick=pendingGolf=false;pendingTackle=pendingJump=pendingShoot=pendingPass=false;pendingCart=GolfCartAction.None;cartMove=Vector2.zero;}
  void HideKickAim(){if(kickAim)kickAim.enabled=false;}
  void OnDestroy(){if(kickAimMaterial)Destroy(kickAimMaterial);if(kickAimMesh)Destroy(kickAimMesh);}
  static Mesh CreateKickAimMesh(){
   const float rounding=.09f,headLengthMultiplier=.8f;
   var corners=new[]{new Vector3(-.22f,0,0),new Vector3(.22f,0,0),new Vector3(.22f,0,.56f),new Vector3(.5f,0,.56f),new Vector3(0,0,1),new Vector3(-.5f,0,.56f),new Vector3(-.22f,0,.56f)};
   var outline=new List<Vector3>();
   // Round every corner, including the tip and the inward shaft/head joins.
   for(int i=0;i<corners.Length;i++){
    var corner=corners[i];var previous=corners[(i+corners.Length-1)%corners.Length];var next=corners[(i+1)%corners.Length];
    var entry=corner+(previous-corner).normalized*rounding;var exit=corner+(next-corner).normalized*rounding;
    for(int step=0;step<=6;step++){float t=step/6f;outline.Add(entry*((1-t)*(1-t))+corner*(2*(1-t)*t)+exit*(t*t));}
   }
   // Normalize the rounded shape, then shorten only the head beyond its shoulder.
   var bounds=new Bounds(outline[0],Vector3.zero);foreach(var point in outline)bounds.Encapsulate(point);
   var vertices=new Vector3[outline.Count+1];var normals=new Vector3[vertices.Length];
   vertices[0]=new Vector3(0,0,(.56f-bounds.min.z)/bounds.size.z);
   for(int i=0;i<outline.Count;i++){
    var p=outline[i];float z=(p.z-bounds.min.z)/bounds.size.z;
    if(z>vertices[0].z)z=vertices[0].z+(z-vertices[0].z)*headLengthMultiplier;
    vertices[i+1]=new Vector3((p.x-bounds.center.x)/bounds.size.x,0,z);
   }
   for(int i=0;i<normals.Length;i++)normals[i]=Vector3.up;
   var triangles=new List<int>();
   // The shoulder centre sees the entire outline; one fan avoids alpha overlap.
   for(int i=0;i<outline.Count;i++){
    int current=i+1,next=(i+1)%outline.Count+1;
    if(Vector3.Cross(vertices[next]-vertices[0],vertices[current]-vertices[0]).y<=.0000001f)continue;
    triangles.Add(0);triangles.Add(next);triangles.Add(current);
   }
   var mesh=new Mesh{name="Rounded solid kick arrow"};mesh.vertices=vertices;mesh.triangles=triangles.ToArray();mesh.normals=normals;
   mesh.RecalculateBounds();return mesh;
  }
  void UpdateKickAim(){
   var ball=FootballBall.Instance;
   if(!Charging||!active||!target||!target.KickReady||!ball||!FootballBall.Allowed){HideKickAim();return;}
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
   kickAimMaterial.SetColor("_BaseColor",charge>=1?new Color(1,0,0,.5f):new Color(1,1,1,.5f));
   kickAim.transform.SetPositionAndRotation(ball.transform.position+Vector3.up*.06f,Quaternion.LookRotation(direction));
   kickAim.transform.localScale=new Vector3(.85f*.8f,1,Mathf.Lerp(1.6f,1.6f*1.8f,charge));
   kickAim.enabled=true;
  }
  void OnDisable(){ClearMatchInput();ResetCartCamera();if(Instance==this)Instance=null;}
  void OnEnable(){Instance=this;}
  void OnApplicationFocus(bool focus){if(!focus)CancelInput();}
  void OnApplicationPause(bool paused){if(paused)CancelInput();}
  public void RequestJump(){if(active&&target&&target.CanRequestJump)pendingJump=true;}
  public void RequestShoot(){if(active&&target&&BasketballBall.Active&&BasketballBall.Active.CanShoot(target))pendingShoot=true;}
  public void RequestPass(){if(active&&target&&BasketballBall.Active&&BasketballBall.Active.CanShoot(target))pendingPass=true;}
  public void RequestCartToggle(){
   if(!active||!target||target.inTransit||!GolfCartWorld.Allowed)return;
   pendingCart=GolfCartWorld.HasCart(target)?GolfCartAction.Recall:GolfCartAction.Summon;pendingCartOwner=GolfCartWorld.Key(target);
  }
  public void RequestCartUse(){
   if(!active||!target||target.inTransit||!GolfCartWorld.Allowed)return;
   var driving=GolfCartWorld.Driving(target);var cart=driving?driving:GolfCartWorld.Nearest(target);if(!cart)return;
   pendingCart=driving?GolfCartAction.Leave:GolfCartAction.Drive;pendingCartOwner=cart.Owner;
  }
  void Update(){
   if(!active||!target||target.inTransit||FootballMatch.BlocksActions){CancelInput();return;}
   if(!FootballBall.Allowed){Charging=false;pendingKick=false;}
   if(Charging&&!target.KickReady)Charging=false;
   if(GolfCharging&&(!GolfMatchManager.Instance||!GolfMatchManager.Instance.CanStrike(target,GolfMatchManager.Instance.Ball(chargingGolfOwner))))GolfCharging=false;
   if(Input.GetKeyDown(KeyCode.Space)&&target.CanRequestJump){Charging=false;RequestJump();}
   if(Input.GetKeyDown(KeyCode.R))RequestCartToggle();
   if(Input.GetKeyDown(KeyCode.E)){Charging=false;if(GolfCartWorld.Allowed)RequestCartUse();else {RequestTackle();RequestShoot();}}
   if(Input.GetKeyDown(KeyCode.Q))RequestPass();
   if(Input.GetKeyDown(KeyCode.F)){if(AppRoot.Instance.SelectedSport==SportId.Golf)BeginGolfSwing();else BeginKick();}if(Input.GetKeyUp(KeyCode.F)){EndGolfSwing();EndKick();}
  }
  public void Switch(){mode=(mode+1)%3;GolfAiming=false;PlayerPrefs.SetInt("camera",mode);PlayerPrefs.Save();}
  public PlayerCommand ReadCommand(){
   var command=ReadInput();cartMove=command.move;return command;
  }
  PlayerCommand ReadInput(){
   if(FootballMatch.BlocksMovement){CancelInput();return default;}
   if(FootballMatch.BlocksActions)CancelInput();
   bool tackle=pendingTackle,jump=pendingJump,shoot=pendingShoot,pass=pendingPass,kick=pendingKick;float charge=pendingCharge;pendingTackle=pendingJump=pendingShoot=pendingPass=false;pendingKick=false;
   var cartAction=pendingCart;var cartOwner=pendingCartOwner;pendingCart=GolfCartAction.None;
   bool golfSwing=pendingGolf;var golfOwner=GolfCharging?chargingGolfOwner:pendingGolfOwner;var golfRound=golfSwing?pendingGolfRound:GolfMatchManager.Instance?GolfMatchManager.Instance.Round:0;float golfCharge=pendingGolfCharge;pendingGolf=false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
   if(DevelopmentProbe.TurnCommandActive){var command=DevelopmentProbe.TurnCommand;command.tackle|=tackle;command.jump|=jump;command.shoot|=shoot;command.pass|=pass;command.charging|=Charging;command.cartAction=cartAction;command.cartOwner=cartOwner;command.golfCharging=GolfCharging;command.golfRound=golfRound;command.golfBallOwner=golfOwner;if(kick){command.kick=true;command.kickCharge=charge;}if(golfSwing){command.golfSwing=true;command.golfBallOwner=golfOwner;command.golfCharge=golfCharge;command.golfRound=golfRound;}return command;}
   if(DevelopmentProbe.IslandDriving)return new PlayerCommand{move=Vector2.up,heading=DevelopmentProbe.IslandHeading,sprint=true};
   if(DevelopmentProbe.Driving)return new PlayerCommand{move=Vector2.up,heading=DevelopmentProbe.Heading,sprint=false};
#endif
   var touch=stick?stick.value:Vector2.zero;var v=touch+new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));return new PlayerCommand{move=Vector2.ClampMagnitude(v,1),heading=yaw,tackle=tackle,jump=jump,shoot=shoot,pass=pass,kick=kick,kickCharge=charge,charging=Charging,sprint=Input.GetKey(KeyCode.LeftShift)||touch.magnitude>.8f,cartAction=cartAction,cartOwner=cartOwner,golfSwing=golfSwing,golfCharging=GolfCharging,golfBallOwner=golfOwner,golfCharge=golfCharge,golfRound=golfRound};}
  void LateUpdate(){
   if(!CanAimGolf)GolfAiming=false;
   if(!active||!target){ResetCartCamera();HideKickAim();return;}
   if(Input.GetKeyDown(KeyCode.C))Switch();
   if(Input.GetMouseButton(1)){LookDelta+=new Vector2(Input.GetAxis("Mouse X")*14,Input.GetAxis("Mouse Y")*14);}
   yaw+=LookDelta.x*.13f;pitch=Mathf.Clamp(pitch-LookDelta.y*.10f,-25,65);LookDelta=Vector2.zero;
   if(SkySailWorld.Instance&&SkySailWorld.Instance.RideCamera(cam,yaw,pitch,mode)){ResetCartCamera();HideKickAim();return;}
   var cart=GolfCartWorld.Driving(target);if(cart&&GolfCartWorld.Allowed){HideKickAim();return;}
   ResetCartCamera();
   bool golf=GolfAiming&&CanAimGolf;
   golfFraming=golf?Mathf.MoveTowards(golfFraming,1,4*Time.deltaTime):0;
   Vector3 focus=target.transform.position+Vector3.up*(mode==0?1.57f:Mathf.Lerp(1.35f,1,golfFraming));
   var rotation=Quaternion.Euler(mode==2?58:pitch,yaw,0);float distance=mode==0?0:mode==1?Mathf.Lerp(5,golfViewDistance,golfFraming):AppRoot.Instance.environments.Current.elevatedDistance;
   Vector3 desired=focus-rotation*Vector3.forward*distance+Quaternion.Euler(0,yaw,0)*Vector3.right*(golfShoulderOffset*golfFraming);
   var cameraPath=desired-focus;
   if(distance>0&&Physics.SphereCast(focus,.2f,cameraPath.normalized,out var hit,cameraPath.magnitude,1<<8,QueryTriggerInteraction.Ignore))desired=focus+cameraPath.normalized*Mathf.Max(.35f,hit.distance-.15f);
   target.HideHead(mode==0||Vector3.Distance(desired,focus)<1.15f);
   cam.nearClipPlane=mode==0?.06f:.15f;transform.SetPositionAndRotation(desired,rotation);
   UpdateKickAim();
  }
  void ResetCartCamera(){followedCart=null;cartReverseAngle=cartReverseVelocity=0;cartReversing=false;}
  // Called after the cart interpolates its displayed pose, so camera and seat
  // follow the same turn on both the host and the driving client.
  public void UpdateCartCamera(GolfCart cart){
   if(!isActiveAndEnabled||!active||!target||target.inTransit||!GolfCartWorld.Allowed||GolfCartWorld.Driving(target)!=cart)return;
   float heading=GolfCartMotor.Heading(cart.transform.rotation);
   if(followedCart!=cart){ResetCartCamera();followedCart=cart;previousCartHeading=heading;}
   yaw+=Mathf.DeltaAngle(previousCartHeading,heading);
   previousCartHeading=heading;
   // Input chooses the view immediately when changing direction; actual speed
   // is the fallback while coasting. Neutral input keeps the last travel view.
   if(cartMove.y>.08f)cartReversing=false;
   else if(cartMove.y<-.08f)cartReversing=true;
   else if(cart.State.speed>.15f)cartReversing=false;
   else if(cart.State.speed<-.15f)cartReversing=true;
   if(mode==1)cartReverseAngle=Mathf.SmoothDamp(cartReverseAngle,cartReversing?180:0,ref cartReverseVelocity,Mathf.Max(.01f,cartReverseTurnTime),Mathf.Max(1,cartReverseTurnSpeed),Time.deltaTime);
   else cartReverseAngle=cartReverseVelocity=0;
   target.HideHead(mode==0);cart.Camera(cam,yaw+(mode==1?cartReverseAngle:0),pitch,mode);
  }
 }
}
