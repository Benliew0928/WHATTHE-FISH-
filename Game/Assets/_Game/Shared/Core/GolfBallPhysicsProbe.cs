#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 // Opt-in checks at the real tee, without moving the ball or forcing a LOD.
 [DefaultExecutionOrder(70)]
 public sealed class GolfBallPhysicsProbe:MonoBehaviour {
  AppRoot app;GolfMatchManager match;string report;int checks;
  GolfBall launchBall;Rigidbody launchReference;bool launchAfterSettling;
  GolfBall monitoredSwing;float horizontalLaunch,horizontalLaunchPeak;
  void FixedUpdate(){
   if(monitoredSwing){var horizontal=Vector3.ProjectOnPlane(monitoredSwing.Body.linearVelocity,Vector3.up).magnitude;
    // Read after the manager's contact but before that step's collision solver.
    // A later landing can transfer angular/vertical energy into horizontal motion.
    if(horizontalLaunch==0&&monitoredSwing.Motion==GolfBallMotion.Flying&&horizontal>1)horizontalLaunch=horizontal;
    horizontalLaunchPeak=Mathf.Max(horizontalLaunchPeak,horizontal);
   }
   if(!launchAfterSettling)return;
   var velocity=new Vector3(3,4,0);launchBall.Strike(velocity);launchReference.useGravity=true;launchReference.linearVelocity=velocity;launchAfterSettling=false;
  }
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){if(Environment.GetCommandLineArgs().Contains("-golfBallPhysicsAudit"))new GameObject("Golf ball physics checks").AddComponent<GolfBallPhysicsProbe>();}
  void Check(bool value,string name){File.AppendAllText(report,(value?"PASS ":"FAIL ")+name+"\n");if(!value)throw new Exception(name);checks++;}
  IEnumerator Frame(){if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)yield return null;else yield return new WaitForEndOfFrame();}
  void Capture(string name){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
   // Render every overlay, including the leaderboard and the ball marker.
   // The shared capture helper otherwise selects only one overlay Canvas.
   var canvases=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
   try {foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=Camera.main;c.planeDistance=.5f;}FindFirstObjectByType<DevelopmentProbe>()?.CaptureFrame(Path.ChangeExtension(report,name+".png"));}
   finally {for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=RenderMode.ScreenSpaceOverlay;canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}}
  }
  void CheckRenderedBall(GolfBall ball,string tag){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
   Capture("tee-third-person-"+tag);var renderers=ball.GetComponentsInChildren<Renderer>();var visible=renderers.Select(r=>r.enabled).ToArray();foreach(var r in renderers)r.enabled=false;Capture("tee-no-ball-"+tag);for(int i=0;i<renderers.Length;i++)renderers[i].enabled=visible[i];
   var image=new Texture2D(2,2);var background=new Texture2D(2,2);image.LoadImage(File.ReadAllBytes(Path.ChangeExtension(report,"tee-third-person-"+tag+".png")));background.LoadImage(File.ReadAllBytes(Path.ChangeExtension(report,"tee-no-ball-"+tag+".png")));
   var camera=Camera.main;var point=camera.WorldToViewportPoint(ball.Body.position);var edge=camera.WorldToViewportPoint(ball.Body.position+camera.transform.up*GolfBall.Radius);int x=Mathf.RoundToInt(point.x*image.width),y=Mathf.RoundToInt(point.y*image.height),radius=Mathf.Max(2,Mathf.CeilToInt((edge.y-point.y)*image.height));int pixels=0;
   for(int dx=-radius;dx<=radius;dx++)for(int dy=-radius;dy<=radius;dy++){if(dx*dx+dy*dy>radius*radius||x+dx<0||x+dx>=image.width||y+dy<0||y+dy>=image.height)continue;var a=image.GetPixel(x+dx,y+dy);var b=background.GetPixel(x+dx,y+dy);if(a.r+a.g+a.b>1.5f&&a.r+a.g+a.b-b.r-b.g-b.b>.4f)pixels++;}
   File.AppendAllText(report,$"TEE_RENDER pitch={tag} x={x} y={y} radiusPixels={radius} visibleBallPixels={pixels}\n");Destroy(image);Destroy(background);Check(pixels>=3,"THIRD_PERSON_WITH_UI_CONTAINS_ACTUAL_BALL_PIXELS_"+tag);
  }
  void PlaceActor(Athlete actor,Vector3 position){actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(position,Quaternion.identity);actor.ResetLocomotion();actor.capsule.enabled=true;Physics.SyncTransforms();}
  bool Supported(GolfBall ball)=>Physics.Raycast(ball.Body.position+Vector3.up*.01f,Vector3.down,out var ground,GolfBall.Radius+.03f,(1<<8)|(1<<GolfTee.Layer),QueryTriggerInteraction.Ignore)&&Vector3.Distance(ball.Body.position,ground.point)>=GolfBall.Radius*.8f;
  IEnumerator Start(){
   var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-report");report=index>=0?args[index+1]:Path.Combine(Application.persistentDataPath,"golf-ball-physics.txt");
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   float until=Time.realtimeSinceStartup+60;while((!AppRoot.Instance||!AppRoot.Instance.Exploring||!GolfMatchManager.Instance)&&Time.realtimeSinceStartup<until)yield return null;
   app=AppRoot.Instance;match=GolfMatchManager.Instance;Check(app&&match&&match.StartMatch(),"REAL_SINGLE_PLAYER_STARTS_AT_AUTHORED_TEE");
   var actor=app.LocalAthlete;var ball=match.Ball(actor);yield return new WaitForSeconds(.25f);
   Check(ball.PhysicsSettings==Resources.Load<GolfBallPhysicsSettings>("GolfBallPhysics"),"LIVE_BALL_USES_PERSISTENT_INSPECTOR_PHYSICS_SETTINGS");
   var lod=ball.GetComponent<LODGroup>();var surface=Physics.Raycast(ball.Body.position+Vector3.up,Vector3.down,out var hit,2,(1<<8)|(1<<GolfTee.Layer),QueryTriggerInteraction.Ignore);
   File.AppendAllText(report,$"TEE position={ball.Body.position} diameter={ball.GetComponentsInChildren<Renderer>().First().bounds.size} lodSize={lod.size:F4} cameraDistance={Vector3.Distance(Camera.main.transform.position,ball.Body.position):F2} grounded={surface} clearance={(surface?ball.Body.position.y-hit.point.y:-1):F4} collider={(surface?hit.collider.name:"none")} kinematic={ball.Body.isKinematic} velocity={ball.Body.linearVelocity}\n");
   Check(ball.Live&&!ball.Body.isKinematic&&ball.Body.detectCollisions&&ball.Body.useGravity,"LIVE_BALL_HAS_DYNAMIC_GRAVITY_AND_COLLISION");
   Check(Mathf.Abs(GolfBall.Radius-.0645f)<.000001f&&ball.GetComponentsInChildren<MeshFilter>().All(f=>Mathf.Abs(f.sharedMesh.bounds.size.x*f.transform.lossyScale.x-.129f)<.0003f)&&Mathf.Abs(ball.GetComponent<SphereCollider>().radius-GolfBall.Radius)<.000001f,"BALL_MODEL_AND_PHYSICS_ARE_THREE_TIMES_PREVIOUS_SIZE");
   var tee=match.Course.holes[0].tee;actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(tee.position-tee.forward*.9f+Vector3.up*.04f,tee.rotation);actor.ResetLocomotion();actor.capsule.enabled=true;Physics.SyncTransforms();
   Check(lod.GetLODs().Last().screenRelativeTransitionHeight==0,"LIVE_BALL_DISTANT_LOD_DOES_NOT_PREMATURELY_CULL");
   app.view.mode=1;app.view.yaw=tee.eulerAngles.y;
   app.view.pitch=16;yield return new WaitForSeconds(.35f);yield return Frame();
   var swing=FindFirstObjectByType<GolfSwingButton>();var right=Quaternion.Euler(0,app.view.yaw,0)*Vector3.right;
   Check(!app.view.GolfAiming&&Mathf.Abs(Vector3.Dot(Camera.main.transform.position-actor.transform.position,right))<.001f,"MATCH_START_KEEPS_CENTRED_THIRD_PERSON_CAMERA");Capture("tee-default-centred");
   Check(swing.aimButton.gameObject.activeSelf&&swing.aimButton.interactable&&swing.aimLabel.text=="瞄准"&&"瞄准取消".All(c=>swing.aimLabel.font.HasCharacter(c)),"AIM_BUTTON_HAS_SUPPORTED_CHINESE_LABEL_AND_IS_AVAILABLE");
   swing.aimButton.onClick.Invoke();yield return new WaitForSeconds(.35f);yield return Frame();
   Check(app.view.GolfAiming&&Mathf.Abs(Vector3.Dot(Camera.main.transform.position-actor.transform.position,right)-app.view.golfShoulderOffset)<.03f&&swing.aimLabel.text=="取消瞄准","AIM_BUTTON_ENTERS_EXISTING_LEFT_SHOULDER_FRAMING");
   foreach(float pitch in new[]{16f,25f}){
    app.view.pitch=pitch;yield return new WaitForSeconds(.35f);yield return Frame();
    Check(Physics.Linecast(Camera.main.transform.position,ball.Body.position,out var viewHit,(1<<0)|(1<<8),QueryTriggerInteraction.Ignore)&&viewHit.collider==ball.GetComponent<SphereCollider>(),"THIRD_PERSON_SEES_FOOT_BALL_WITHOUT_PLAYER_OCCLUSION_"+pitch);
    CheckRenderedBall(ball,pitch.ToString("F0"));
   }
   var mask=Camera.main.cullingMask;Camera.main.cullingMask=mask&~(1<<10);Capture("tee-without-grass");Camera.main.cullingMask=mask;
   swing.aimButton.onClick.Invoke();yield return new WaitForSeconds(.1f);yield return Frame();
   Check(!app.view.GolfAiming&&Mathf.Abs(Vector3.Dot(Camera.main.transform.position-actor.transform.position,right))<.001f,"SECOND_AIM_CLICK_RETURNS_TO_CENTRED_CAMERA");Capture("tee-aim-cancelled");
   swing.aimButton.onClick.Invoke();app.view.mode=0;app.view.pitch=48;yield return Frame();yield return null;
   Check(!app.view.GolfAiming&&!swing.aimButton.gameObject.activeSelf,"FIRST_PERSON_CLEARS_AIM_AND_KEEPS_EXISTING_CAMERA");Capture("tee-first-person");
   app.view.mode=2;yield return Frame();yield return null;
   Check(!app.view.GolfAiming&&!swing.aimButton.gameObject.activeSelf,"ELEVATED_VIEW_REMAINS_UNCHANGED");
   app.view.mode=1;yield return Frame();swing.aimButton.onClick.Invoke();app.view.ClearMatchInput();yield return Frame();
   Check(!app.view.GolfAiming&&Mathf.Abs(Vector3.Dot(Camera.main.transform.position-actor.transform.position,right))<.001f,"ROUND_RESET_CLEARS_LOCAL_AIM_STATE");
   app.view.enabled=false;Camera.main.transform.position=ball.Body.position+new Vector3(.13f,.10f,.17f);Camera.main.transform.LookAt(ball.Body.position);Camera.main.nearClipPlane=.01f;yield return Frame();Capture("tee-close");
   yield return new WaitForSeconds(.5f);Check(Supported(ball),"INITIAL_BALL_REMAINS_ON_PLAYING_SURFACE");
   yield return TeeChecks(ball,actor);
   yield return PhysicsChecks(ball,actor);
   app.view.enabled=true;DevelopmentProbe.TurnCommand=default;
   yield return CupChecks(ball,actor);
   File.AppendAllText(report,"GOLF_BALL_PHYSICS_COMPLETE checks="+checks+"\n");
  }
  IEnumerator CupChecks(GolfBall ball,Athlete actor){
   int strokes=match.Player(actor).TotalStroke;
   for(int number=1;number<=5;number++){
    var hole=match.Course.holes.First(h=>h.number==number);ball.Place(hole.cup.position+Vector3.up*.18f+Vector3.right*.035f,false);ball.Body.linearVelocity=Vector3.down*.7f;ball.Body.WakeUp();Physics.SyncTransforms();
    float until=Time.time+2;while(match.Player(actor).CompletedHoleCount<number&&Time.time<until)yield return new WaitForFixedUpdate();
    File.AppendAllText(report,$"LARGE_BALL_CUP hole={number} completed={match.Player(actor).CompletedHoleCount} strokes={match.Player(actor).TotalStroke} position={ball.Body.position}\n");
    Check(match.Player(actor).CompletedHoleCount==number&&match.Player(actor).TotalStroke==strokes,"LARGER_BALL_ENTERS_EXISTING_CUP_WITHOUT_CHANGING_STROKES_"+number);
    if(number<5)Check(Vector3.Distance(ball.Body.position,match.TeePosition(number+1,ball.Owner))<.04f&&Supported(ball),"NEXT_TEE_SUPPORTS_LARGER_BALL_"+number);
   }
   Check(match.Player(actor).IsFinished&&match.State.Phase==GolfMatchPhase.Ended,"FIVE_EXISTING_HOLES_STILL_FINISH_THE_MATCH");
   uint previousRound=match.Round;Check(match.StartMatch()&&match.Round!=previousRound&&match.Player(actor).TotalStroke==0&&!app.view.GolfAiming,"RESTART_RESETS_PROGRESS_AND_STARTS_WITHOUT_AIM");yield return new WaitForSeconds(.2f);
   Check(match.Tee(match.Ball(actor).Owner)&&FindObjectsByType<GolfTee>(FindObjectsSortMode.None).Length==1,"RESTART_REPLACES_OLD_WOODEN_SUPPORT_WITH_ONE_NEW_TEE");
  }
  IEnumerator TeeChecks(GolfBall ball,Athlete actor){
   var tee=match.Tee(ball.Owner);Check(tee&&tee.transform.parent==match.transform&&tee.GetComponent<Rigidbody>()==null&&tee.GetComponent<MeshCollider>().sharedMesh==tee.GetComponent<MeshFilter>().sharedMesh,"WOODEN_TEE_IS_SEPARATE_FIXED_CONCAVE_SUPPORT");
   Check(tee.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_BaseMap").width==64&&tee.GetComponent<MeshRenderer>().bounds.size.x>.047f&&tee.GetComponent<MeshRenderer>().bounds.size.x<.049f,"WOODEN_TEE_USES_SMALL_SHARED_GRAIN_AND_BALL_SIZED_CUP");
   Check(tee.gameObject.layer==GolfTee.Layer&&Physics.GetIgnoreCollision(tee.GetComponent<Collider>(),actor.capsule)&&Physics.Raycast(tee.transform.position+Vector3.up,Vector3.down,out var terrain,2,1<<8,QueryTriggerInteraction.Ignore)&&terrain.collider!=tee.GetComponent<Collider>(),"WOODEN_SUPPORT_EXCLUDED_FROM_CHARACTER_COLLISION_AND_NORMAL_GROUND_QUERIES");
   var origin=tee.transform.position;var seated=ball.Body.position;yield return new WaitForSeconds(2);
   Check(Vector3.Distance(seated,ball.Body.position)<.003f&&Mathf.Abs(ball.Body.position.y-origin.y-GolfTee.SeatHeight-GolfBall.Radius)<.004f&&Supported(ball),"EXISTING_DYNAMIC_BALL_STAYS_ON_WOODEN_CUP");
   if(SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null){
    var camera=Camera.main;camera.transform.position=origin+new Vector3(.20f,.19f,.33f);camera.transform.LookAt(origin+Vector3.up*.105f);camera.fieldOfView=43;camera.nearClipPlane=.003f;yield return Frame();Capture("wooden-opening-tee-close");
    var renderers=ball.GetComponentsInChildren<Renderer>();var enabled=renderers.Select(r=>r.enabled).ToArray();foreach(var r in renderers)r.enabled=false;yield return Frame();Capture("wooden-tee-without-ball");for(int i=0;i<renderers.Length;i++)renderers[i].enabled=enabled[i];
   }
   ball.Place(new Vector3(450,5,450),false);yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();yield return new WaitForSeconds(.2f);
   Check(Vector3.Distance(ball.Body.position,ball.LastShotPosition)<.004f&&match.Player(actor).TotalStroke==0,"OPENING_OUT_OF_BOUNDS_RECOVERY_RETURNS_BALL_TO_WOODEN_CUP_WITHOUT_PENALTY");
   var stance=origin+Vector3.back*.75f;
   Check(Physics.Raycast(stance+Vector3.up*2,Vector3.down,out var floor,4,1<<8,QueryTriggerInteraction.Ignore),"OPENING_SWING_STANCE_SAMPLED_FROM_ACTUAL_SLOPING_TERRAIN");
   PlaceActor(actor,floor.point+Vector3.up*.02f);DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.2f);
   File.AppendAllText(report,$"OPENING_SWING canSwing={match.CanSwing(actor)} grounded={actor.Grounded} airborne={actor.Airborne} busy={actor.GolfClubMotion.Busy} address={actor.GolfClubMotion.CanAddress(ball.Body.position)} selected={match.Strikeable(actor)?.Owner} actor={actor.transform.position} heading={actor.transform.eulerAngles.y:F2} ball={ball.Body.position}\n");
   Check(match.TrySwing(actor,ball.Owner,0,.03f,match.Round),"LOW_CHARGE_REAL_SWING_ACCEPTED_FROM_WOODEN_SUPPORT");yield return new WaitForSeconds(.9f);
   Check(Vector3.ProjectOnPlane(ball.Body.position-origin,Vector3.up).magnitude>.2f&&tee.transform.position==origin&&match.Player(actor).TotalStroke==1,"BALL_LEAVES_CUP_NORMALLY_TEE_STAYS_FIXED_AND_SWING_COUNTS_ONCE");
   Check(match.EnterHole(ball,2)==GolfHoleResult.Invalid&&match.Player(actor).CurrentHole==1&&match.Player(actor).TotalStroke==1,"WRONG_HOLE_AFTER_OPENING_SWING_PRESERVES_EXISTING_RULES");yield return new WaitForSeconds(.7f);
   Check(Vector3.Distance(ball.Body.position,ball.LastShotPosition)<.004f&&Supported(ball),"WRONG_HOLE_RECOVERY_IS_SUPPORTED_ON_ORIGINAL_WOODEN_TEE");
   DevelopmentProbe.TurnCommand=default;
  }
  IEnumerator PhysicsChecks(GolfBall ball,Athlete actor){
   Check(Physics.Raycast(new Vector3(-40,20,-100),Vector3.down,out var floor,40,1<<8,QueryTriggerInteraction.Ignore),"PHYSICS_FIXTURE_INSIDE_EXISTING_COURSE");
   var plate=new GameObject("Temporary ball test surface");plate.layer=8;plate.transform.position=floor.point+Vector3.up*6;plate.AddComponent<BoxCollider>().size=new Vector3(30,.2f,30);
   Vector3 Rest()=>plate.transform.TransformPoint(Vector3.up*.1f)+plate.transform.up*(GolfBall.Radius+.002f);
   ball.Place(Rest(),true);PlaceActor(actor,plate.transform.position+new Vector3(-1,.14f,0));yield return new WaitForSeconds(.2f);
   var still=ball.Body.position;var actorStart=actor.transform.position;
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.right,heading=0,sprint=true};yield return new WaitForSeconds(.7f);DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.15f);
   File.AppendAllText(report,$"BODY_PUSH actorDistance={Vector3.Distance(actorStart,actor.transform.position):F4} ballDistance={Vector3.Distance(still,ball.Body.position):F5} speed={ball.Body.linearVelocity.magnitude:F4}\n");
   Check(Vector3.Distance(actorStart,actor.transform.position)>2&&Vector3.Distance(still,ball.Body.position)<.025f,"RUNNING_OWNER_CANNOT_PUSH_BALL");
   var late=Instantiate(app.athletePrefab,new Vector3(-45,plate.transform.position.y+.15f,-100),Quaternion.identity).GetComponent<Athlete>();late.Setup();yield return new WaitForFixedUpdate();
   Check(Physics.GetIgnoreCollision(ball.GetComponent<SphereCollider>(),late.capsule),"LATE_PLAYER_COLLIDER_ALSO_EXCLUDED");
   PlaceActor(late,plate.transform.position+new Vector3(-1,.14f,0));yield return new WaitForFixedUpdate();late.capsule.Move(Vector3.right*2);yield return new WaitForFixedUpdate();
   Check(Vector3.Distance(still,ball.Body.position)<.025f,"OTHER_PLAYER_AND_REENABLED_CONTROLLER_CANNOT_PUSH_BALL");Destroy(late.gameObject);
   var cart=new GameObject("Temporary moving cart collider");cart.layer=GolfCartMotor.Layer;cart.AddComponent<BoxCollider>().size=Vector3.one;cart.transform.position=still+Vector3.left;
   for(int i=0;i<20;i++){cart.transform.position+=Vector3.right*.1f;Physics.SyncTransforms();yield return new WaitForFixedUpdate();}
   Check(Vector3.Distance(still,ball.Body.position)<.025f,"DRIVEN_CART_CANNOT_SUPPLY_BALL_IMPULSE");Destroy(cart);
   PlaceActor(actor,plate.transform.position+new Vector3(-5,.14f,-5));ball.Place(Rest(),true);ball.Strike(Vector3.right*3);float initial=ball.Body.linearVelocity.magnitude;
   yield return new WaitForSeconds(.6f);float slowing=ball.Body.linearVelocity.magnitude;yield return new WaitForSeconds(5);
   File.AppendAllText(report,$"LEVEL_ROLL initial={initial:F3} after0.6={slowing:F3} final={ball.Body.linearVelocity.magnitude:F3}\n");
   Check(slowing>0&&slowing<initial&&ball.Body.linearVelocity.magnitude<.065f,"FLAT_FREE_BALL_ROLLS_SLOWS_AND_STOPS");
   foreach(float angle in new[]{5f,15f}){
    plate.transform.rotation=Quaternion.Euler(0,0,angle);Physics.SyncTransforms();ball.Place(Rest(),true);
    var downhill=Vector3.ProjectOnPlane(Physics.gravity,plate.transform.up).normalized;
    ball.Strike(downhill*1.8f);yield return new WaitForSeconds(.16f);
    float first=ball.Body.linearVelocity.magnitude,maximum=first,previousSpeed=first;bool smooth=first>.8f,monotone=true,seenSlow=false;float until=Time.time+5;
    while(Time.time<until&&ball.Motion!=GolfBallMotion.Resting){
     yield return new WaitForFixedUpdate();float current=ball.Body.linearVelocity.magnitude;
     if(ball.Motion==GolfBallMotion.SlowRolling){seenSlow=true;maximum=Mathf.Max(maximum,current);if(current>previousSpeed+.06f)monotone=false;}
     previousSpeed=current;
    }
    File.AppendAllText(report,$"SETTLING angle={angle} entry={first:F4} maximum={maximum:F4} final={ball.Body.linearVelocity.magnitude:F4} state={ball.Motion} timer={ball.StopTimer:F3}\n");
    Check(smooth&&seenSlow&&monotone&&ball.Motion==GolfBallMotion.Resting,"SLOW_SLOPE_BALL_DECELERATES_SMOOTHLY_INTO_REST_"+angle);
    var anchored=ball.Body.position;yield return new WaitForSeconds(2);
    Check(Vector3.Distance(anchored,ball.Body.position)<.001f&&!ball.Body.useGravity&&!ball.Body.isKinematic&&ball.Body.detectCollisions&&ball.Body.linearVelocity==Vector3.zero&&ball.Body.angularVelocity==Vector3.zero,"RESTING_SLOPE_BALL_STAYS_PUT_WITH_DYNAMIC_COLLISION_RESPONSE_"+angle);
    ball.Strike(downhill*5);Check(ball.Motion==GolfBallMotion.Flying&&ball.Body.useGravity&&ball.StopTimer==0,"NEW_STRIKE_IMMEDIATELY_RELEASES_REST_AND_SPEED_LIMIT_"+angle);
    yield return new WaitForSeconds(.5f);
    Check(ball.Motion==GolfBallMotion.FastRolling&&ball.Body.linearVelocity.magnitude>4.5f,"FAST_DOWNHILL_BALL_REMAINS_FREE_"+angle);
   }
   plate.transform.rotation=Quaternion.identity;Physics.SyncTransforms();
   ball.Place(Rest(),true);ball.Strike(Vector3.right*.08f);
   float waitUntil=Time.time+3;while(ball.StopTimer<.35f&&Time.time<waitUntil)yield return new WaitForFixedUpdate();
   Check(ball.StopTimer>=.35f&&ball.Motion==GolfBallMotion.SlowRolling,"NEAR_COMPLETE_STOP_TIMER_FIXTURE");
   var gentleObject=new GameObject("Temporary gentle golf ball contact");var gentle=gentleObject.AddComponent<GolfBall>();gentle.Bind(match,999998);gentle.SetLive(true);gentle.Place(ball.Body.position+Vector3.right*(GolfBall.Radius*2+.002f),false);gentle.Strike(Vector3.left*.12f);
   float contactUntil=Time.time+.25f;bool gentleReset=false;while(Time.time<contactUntil){yield return new WaitForFixedUpdate();if(ball.StopTimer<.1f&&ball.Motion==GolfBallMotion.Flying){gentleReset=true;break;}}
   Check(gentleReset,"EVEN_GENTLE_NEW_BALL_CONTACT_RESETS_NEAR_COMPLETE_STOP_TIMER");Destroy(gentleObject);
   ball.Place(Rest(),true);ball.Strike(Vector3.right*.08f);waitUntil=Time.time+3;while(ball.StopTimer<.35f&&Time.time<waitUntil)yield return new WaitForFixedUpdate();
   ball.ApplyGameplayImpulse(Vector3.right*(ball.Body.mass*.08f));
   Check(ball.StopTimer==0&&ball.Motion==GolfBallMotion.Flying&&ball.Body.useGravity,"SMALL_GAMEPLAY_IMPULSE_RESETS_OLD_STOP_TIMER");
   yield return new WaitForSeconds(.25f);Check(ball.Motion!=GolfBallMotion.Resting,"OLD_STOP_TIMER_CANNOT_STOP_A_NEW_IMPULSE");
   yield return new WaitForSeconds(.8f);Check(ball.Motion==GolfBallMotion.Resting,"NUDGED_BALL_CAN_SETTLE_AGAIN");
   ball.Body.AddForce(Vector3.right*(ball.Body.mass*.05f),ForceMode.Impulse);yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
   Check(ball.Motion!=GolfBallMotion.Resting&&ball.Body.useGravity&&ball.StopTimer<.1f,"SMALL_DIRECT_RIGIDBODY_FORCE_ALSO_RELEASES_REST");yield return new WaitForSeconds(1);
   Check(ball.Motion==GolfBallMotion.Resting,"COLLISION_FIXTURE_RESTING_AGAIN");
   var otherObject=new GameObject("Temporary colliding golf ball");var other=otherObject.AddComponent<GolfBall>();other.Bind(match,999999);other.SetLive(true);other.Place(ball.Body.position+Vector3.right*.7f,false);other.Strike(Vector3.left*4);
   waitUntil=Time.time+2;bool collisionWoke=false;
   while(Time.time<waitUntil){yield return new WaitForFixedUpdate();if(ball.Body.linearVelocity.magnitude>.3f){collisionWoke=ball.Body.useGravity&&ball.StopTimer<.1f&&ball.Motion!=GolfBallMotion.Resting;break;}}
   Check(collisionWoke,"ANOTHER_GOLF_BALL_WAKES_REST_WITH_FRESH_TIMER_AND_NORMAL_IMPULSE");Destroy(otherObject);
   ball.Place(Rest()+Vector3.up*2,true);ball.Strike(new Vector3(3,4,0));
   var reference=new GameObject("Temporary normal flight reference");reference.transform.position=ball.Body.position+Vector3.forward*2;var referenceBody=reference.AddComponent<Rigidbody>();referenceBody.mass=ball.Body.mass;referenceBody.linearDamping=.05f;referenceBody.angularDamping=.1f;referenceBody.linearVelocity=ball.Body.linearVelocity;
   yield return new WaitForSeconds(.3f);
   Check(ball.Motion==GolfBallMotion.Flying&&ball.Body.useGravity&&Vector3.Distance(ball.Body.linearVelocity,referenceBody.linearVelocity)<.002f,"AIRBORNE_FLIGHT_MATCHES_UNMODIFIED_RIGIDBODY_VELOCITY");Destroy(reference);
   plate.transform.rotation=Quaternion.Euler(0,0,15);Physics.SyncTransforms();ball.Place(Rest(),true);ball.Strike(Vector3.ProjectOnPlane(Vector3.left,plate.transform.up).normalized*.5f);yield return new WaitForSeconds(.18f);
   Check(ball.Motion==GolfBallMotion.SlowRolling,"SAME_STEP_STRIKE_STARTS_DURING_SLOPE_SETTLING");
   var flightReference=new GameObject("Temporary same-step flight reference");launchReference=flightReference.AddComponent<Rigidbody>();launchReference.position=ball.Body.position+Vector3.forward*2;launchReference.linearDamping=.05f;launchReference.useGravity=false;launchBall=ball;launchAfterSettling=true;
   while(launchAfterSettling)yield return new WaitForFixedUpdate();yield return new WaitForSeconds(.15f);
   Check(ball.Motion==GolfBallMotion.Flying&&Vector3.Distance(ball.Body.linearVelocity,launchReference.linearVelocity)<.002f,"SAME_PHYSICS_STEP_STRIKE_HAS_NO_RESIDUAL_SETTLING_FORCE");Destroy(flightReference);launchBall=null;launchReference=null;
   plate.transform.rotation=Quaternion.identity;Physics.SyncTransforms();
   // Rest cannot suspend a ball after its support disappears.
   ball.Place(Rest(),true);yield return new WaitForSeconds(1);Check(ball.Motion==GolfBallMotion.Resting,"SUPPORT_REMOVAL_FIXTURE_RESTING");
   plate.SetActive(false);yield return new WaitForSeconds(.12f);Check(ball.Motion==GolfBallMotion.Flying&&ball.Body.useGravity&&ball.Body.linearVelocity.y<-.2f,"LOSS_OF_GROUND_RESTORES_NORMAL_FALLING");plate.SetActive(true);Physics.SyncTransforms();
   float puttSpeed=0;
   foreach(float charge in new[]{.08f,.8f}){
    ball.Place(Rest(),true);PlaceActor(actor,plate.transform.position+new Vector3(0,.14f,-.9f));yield return new WaitForSeconds(.2f);var count=match.Player(actor).TotalStroke;
    Check(match.TrySwing(actor,ball.Owner,0,charge,match.Round),"QUALIFIED_SWING_ACCEPTED_"+charge);var reset=ball.ResetSequence;float speed=0,peakY=ball.Body.position.y;var start=ball.Body.position;
    float until=Time.time+.6f;while(Time.time<until){speed=Mathf.Max(speed,ball.Body.linearVelocity.magnitude);peakY=Mathf.Max(peakY,ball.Body.position.y);yield return new WaitForFixedUpdate();}
    File.AppendAllText(report,$"SWING charge={charge:F2} peakSpeed={speed:F3} heightGain={peakY-start.y:F3} travel={Vector3.Distance(start,ball.Body.position):F3} strokes={match.Player(actor).TotalStroke}\n");
    Check(match.Player(actor).TotalStroke==count+1&&ball.ResetSequence==reset&&Vector3.Distance(start,ball.Body.position)>.1f,"ONLY_SWING_COUNTS_ONCE_AND_MOVES_FREE_BALL_"+charge);
    if(charge<.25f){puttSpeed=speed;Check(peakY-start.y<.1f&&speed>1,"LIGHT_SWING_PUTTS_ON_SURFACE");}else Check(speed>puttSpeed*2&&peakY-start.y>.05f,"SHORT_CHARGED_SWING_ACCELERATES_AND_LOFTS_BALL");
    yield return new WaitForSeconds(.3f);
   }
   plate.transform.rotation=Quaternion.identity;Physics.SyncTransforms();ball.Place(Rest(),true);PlaceActor(actor,plate.transform.position+new Vector3(0,.14f,-.9f));yield return new WaitForSeconds(.3f);
   var fullStart=ball.Body.position;var fullCount=match.Player(actor).TotalStroke;var fullReset=ball.ResetSequence;horizontalLaunch=horizontalLaunchPeak=0;monitoredSwing=ball;
   Check(match.TrySwing(actor,ball.Owner,0,1,match.Round),"FULL_CHARGE_EIGHTEEN_METRE_PER_SECOND_SWING_ACCEPTED");yield return new WaitForSeconds(.8f);monitoredSwing=null;
   float fullTravel=Vector3.ProjectOnPlane(ball.Body.position-fullStart,Vector3.up).magnitude;
   File.AppendAllText(report,$"FULL_CHARGE launchHorizontal={horizontalLaunch:F5} subsequentHorizontalPeak={horizontalLaunchPeak:F5} travel={fullTravel:F5} state={ball.Motion} strokes={match.Player(actor).TotalStroke}\n");
   Check(Mathf.Abs(horizontalLaunch-18f)<.001f,"FULL_CHARGE_CONTACT_LAUNCHES_AT_EIGHTEEN_METRES_PER_SECOND");
   Check(fullTravel>5.5f&&ball.Motion!=GolfBallMotion.Resting,"FULL_CHARGE_TRAVELS_BEYOND_PREVIOUS_FIVE_METRE_CAP");
   Check(match.Player(actor).TotalStroke==fullCount+1&&ball.ResetSequence==fullReset,"FULL_CHARGE_COUNTS_ONCE_WITHOUT_FORCED_RECOVERY");
   ball.Place(match.TeePosition(1,ball.Owner),true);Destroy(plate);yield return null;
  }
 }
}
#endif
