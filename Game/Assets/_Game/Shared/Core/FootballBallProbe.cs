#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
namespace WhatTheFish {
 // Opt-in integration checks against the actual pitch, controller and PhysX scene.
 public sealed class FootballBallProbe:MonoBehaviour {
  public string ReportPath;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();if(args.Contains("-footballBallAudit")){var probe=new GameObject("Football physics checks").AddComponent<FootballBallProbe>();int i=Array.IndexOf(args,"-report");probe.ReportPath=i>=0?args[i+1]:Path.Combine(Application.persistentDataPath,"football-ball.txt");}}
  void Check(bool ok,string name){File.AppendAllText(ReportPath,(ok?"PASS ":"FAIL ")+name+Environment.NewLine);if(!ok)throw new Exception("Football check failed: "+name);}
  void Place(Athlete actor,Vector3 position,float yaw=0){actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));actor.ResetLocomotion();actor.capsule.enabled=true;Physics.SyncTransforms();}
  IEnumerator CaptureControl(Athlete actor,string name){
   File.AppendAllText(ReportPath,$"CAPTURE {name} batch={Application.isBatchMode} graphics={SystemInfo.graphicsDeviceType}\n");
   if(Application.isBatchMode)yield break;
   var view=PlayerView.Instance;int mode=view.mode;float yaw=view.yaw,pitch=view.pitch;
   view.mode=1;view.yaw=actor.transform.eulerAngles.y+145;view.pitch=14;
   yield return new WaitForEndOfFrame();var path=Path.Combine(Path.GetDirectoryName(ReportPath),"control-"+name+".png");var probe=FindFirstObjectByType<DevelopmentProbe>();if(probe)probe.CaptureFrame(path);else ScreenCapture.CaptureScreenshot(path);
   yield return null;view.mode=mode;view.yaw=yaw;view.pitch=pitch;
  }
  IEnumerator Start(){
   Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));File.WriteAllText(ReportPath,"Football prototype integration checks\n");
   while(!AppRoot.Instance||!FootballBall.Instance||!AppRoot.Instance.LocalAthlete)yield return null;
   var app=AppRoot.Instance;app.EnterOffline();DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   yield return new WaitForSeconds(.5f);var ball=FootballBall.Instance;var actor=app.LocalAthlete;var origin=ball.Body.position;
   Check(!ball.Body.isKinematic&&ball.Body.useGravity&&ball.Body.collisionDetectionMode==CollisionDetectionMode.ContinuousDynamic,"DYNAMIC_GRAVITY_CCD");
   Place(actor,origin+new Vector3(0,-.22f,-4));Check(!actor.TryKick(),"OUT_OF_RANGE_REJECTED");
   Place(actor,origin+new Vector3(0,-.22f,-.9f),180);Check(!actor.TryKick(),"BACK_TO_BALL_REJECTED");
   Place(actor,origin+new Vector3(0,2,-.9f));Check(!actor.TryKick(),"AIRBORNE_REJECTED");
   Place(actor,origin+new Vector3(0,-.22f,-.9f));var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=8;wall.transform.position=origin+new Vector3(0,.6f,-.45f);wall.transform.localScale=new Vector3(3,2,.08f);Physics.SyncTransforms();
   Check(!actor.TryKick(),"WALL_BLOCKS_KICK");Destroy(wall);yield return null;
   yield return null;var button=FindFirstObjectByType<KickButton>();Check(button,"TOUCH_KICK_CONTROL");
   File.AppendAllText(ReportPath,$"Kick setup ball={ball.Body.position} actor={actor.transform.position} grounded={actor.Grounded} range={ball.InKickRange(actor)} enabled={button.button.interactable}\n");
   var kickPointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=7};
   button.OnPointerDown(kickPointer);yield return new WaitForSeconds(ball.maximumChargeTime+.1f);
   Check(ball.Body.linearVelocity.magnitude<.08f&&PlayerView.Instance.Charging&&PlayerView.Instance.Charge==1,"HOLD_CHARGES_WITHOUT_KICKING_OR_AUTO_RELEASE");
   button.OnPointerUp(new PointerEventData(EventSystem.current){pointerId=8});Check(PlayerView.Instance.Charging,"OTHER_POINTER_CANNOT_RELEASE_KICK");
   button.OnPointerUp(kickPointer);yield return new WaitForSeconds(.15f);
   File.AppendAllText(ReportPath,$"After kick velocity={ball.Body.linearVelocity} position={ball.Body.position} sleeping={ball.Body.IsSleeping()}\n");
   Check(ball.Body.linearVelocity.magnitude>6,"TOUCH_KICK_FAST_START");Check(!actor.TryKick(),"NO_REPEAT_DURING_COOLDOWN");
   float fast=ball.Body.linearVelocity.magnitude;var firstRotation=ball.Body.rotation;
   float rollingAngle=0,rollingUntil=Time.time+2;
   while(Time.time<rollingUntil){yield return null;rollingAngle+=Quaternion.Angle(firstRotation,ball.Body.rotation);firstRotation=ball.Body.rotation;}
   File.AppendAllText(ReportPath,$"Rolling sampled angle={rollingAngle} spin={ball.Body.angularVelocity.magnitude}\n");
   Check(ball.Body.linearVelocity.magnitude<fast-1&&ball.Body.linearVelocity.magnitude>1,"GRADUAL_DECELERATION");Check(rollingAngle>45&&ball.Body.angularVelocity.magnitude>1,"ACTUAL_ROLLING_ROTATION");
   yield return new WaitForSeconds(12);Check(ball.Body.IsSleeping()&&ball.Body.linearVelocity.magnitude<.08f,"NATURAL_STOP");Check(ball.Body.position.y>origin.y-.05f&&ball.Body.position.y<origin.y+.05f,"STAYS_ON_PITCH");Check(Vector3.Distance(origin,ball.Body.position)>8,"USEFUL_KICK_DISTANCE");
   ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,-.9f));yield return new WaitForSeconds(.5f);
   wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=8;wall.transform.position=origin+new Vector3(0,.6f,3);wall.transform.localScale=new Vector3(3,2,.05f);Physics.SyncTransforms();Check(actor.TryKick(),"SECOND_KICK_ACCEPTED");
   yield return new WaitForSeconds(1);Check(ball.Body.position.z<origin.z+2.85f,"THIN_WALL_CCD_NO_TUNNEL");Check(ball.Body.position.y<origin.y+.3f,"WALL_NO_EXPLOSIVE_BOUNCE");Destroy(wall);
   ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,-.9f));var rival=Instantiate(app.athletePrefab).GetComponent<Athlete>();rival.Setup();Place(rival,origin+new Vector3(0,-.22f,2));yield return new WaitForSeconds(.5f);Check(actor.TryKick(),"PLAYER_COLLISION_KICK");
   yield return new WaitForSeconds(.8f);Check(ball.Body.position.z<origin.z+1.6f,"BALL_COLLIDES_WITH_PLAYER");
   File.AppendAllText(ReportPath,$"Player interception ball={ball.Body.position} velocity={ball.Body.linearVelocity} angular={ball.Body.angularVelocity} sleeping={ball.Body.IsSleeping()}\n");
   Check(ball.Body.linearVelocity.magnitude<.08f,"PLAYER_INTERCEPTION_STOPS_BALL");Destroy(rival.gameObject);
   ball.ResetBall();Place(actor,origin+new Vector3(-4,-.22f,-4));rival=Instantiate(app.athletePrefab).GetComponent<Athlete>();rival.Setup();Place(rival,origin+new Vector3(.35f,-.22f,2));
   ball.Body.WakeUp();ball.Body.linearVelocity=Vector3.forward*18;yield return new WaitForSeconds(.3f);
   Check(ball.Body.linearVelocity.magnitude<.08f&&ball.Body.angularVelocity.magnitude<.5f&&ball.Body.position.z<origin.z+2,"FAST_GLANCING_PLAYER_INTERCEPTION");Destroy(rival.gameObject);
   ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,-.9f));yield return new WaitForSeconds(.4f);Check(actor.TryKick(0),"KICK_RELEASES_ATTACHED_BALL");
   Check(!ball.CurrentController&&!ball.Body.isKinematic,"KICK_RESTORES_FREE_PHYSICS");
   Place(actor,origin+new Vector3(4,-.22f,-4));yield return new WaitForSeconds(.2f);
   float deadline;
   ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,-1.2f));DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up};yield return new WaitForSeconds(.8f);DevelopmentProbe.TurnCommand=default;
   Check(ball.Body.position.z>origin.z+.3f&&ball.Body.position.y<origin.y+.3f,"ATTACHED_BALL_FOLLOWS_WALK_WITHOUT_LAUNCH");
   ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,-.55f));yield return new WaitForSeconds(.3f);
   var contactOrigin=ball.Body.position;DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up};yield return new WaitForSeconds(.8f);DevelopmentProbe.TurnCommand=default;
   File.AppendAllText(ReportPath,$"Rest contact ball={ball.Body.position} actor={actor.transform.position} speed={actor.speed} motor={actor.Motor.Velocity} radius={actor.capsule.radius} centre={actor.capsule.center} step={actor.capsule.stepOffset}\n");
   Check(ball.Body.position.z>contactOrigin.z+.3f,"ATTACHMENT_FOLLOWS_MOVEMENT_FROM_REST");
   ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,-.7f));yield return new WaitForSeconds(.3f);
   float feet=actor.transform.position.y,maxFeet=feet;
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up*.2f};deadline=Time.time+2;
   while(Time.time<deadline){maxFeet=Mathf.Max(maxFeet,actor.transform.position.y);yield return null;}DevelopmentProbe.TurnCommand=default;
   File.AppendAllText(ReportPath,$"Slow contact ball={ball.Body.position} actor={actor.transform.position} grounded={actor.Grounded} range={ball.InKickRange(actor)} speed={actor.speed}\n");
   Check(ball.Body.position.z>origin.z+.3f,"SLOW_ATTACHED_MOVEMENT_FOLLOWS");
   Check(maxFeet<feet+.08f&&actor.Grounded,"SLOW_CONTACT_CANNOT_CLIMB_BALL");
   Check(ball.InKickRange(actor),"KICK_ELIGIBLE_AFTER_GROUNDED_CONTACT");
   foreach(var sprint in new[]{false,true}){
    ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,-.7f));yield return new WaitForSeconds(.2f);feet=actor.transform.position.y;maxFeet=feet;
    DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,sprint=sprint};deadline=Time.time+1;
    while(Time.time<deadline){maxFeet=Mathf.Max(maxFeet,actor.transform.position.y);yield return null;}DevelopmentProbe.TurnCommand=default;
    Check(maxFeet<feet+.08f&&actor.Grounded,sprint?"SPRINT_CANNOT_CLIMB_BALL":"WALK_CANNOT_CLIMB_BALL");
   }
   ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,0));yield return new WaitForSeconds(.4f);
   Check(actor.Grounded&&Vector3.ProjectOnPlane(actor.transform.position-ball.Body.position,Vector3.up).magnitude>.5f,"OVERLAP_RECOVERS_BESIDE_BALL");
   Check(ball.Pitch&&Mathf.Abs(ball.PitchBounds.size.x*Mathf.Abs(ball.Pitch.lossyScale.x)-68)<.02f&&Mathf.Abs(ball.PitchBounds.size.z*Mathf.Abs(ball.Pitch.lossyScale.z)-105)<.02f,"AUTHORED_WHITE_LINE_PITCH_68_BY_105");
   Place(actor,origin+new Vector3(-4,-.22f,-4));
   var fieldCentre=ball.Pitch.InverseTransformPoint(origin);var limits=ball.PitchBounds;float radius=.22f;
   var directions=new[]{Vector3.right,Vector3.left,Vector3.forward,Vector3.back};int edge=0;
   foreach(var direction in directions){
    ball.ResetBall();var near=fieldCentre;
    if(direction.x!=0)near.x=direction.x>0?limits.max.x-.5f:limits.min.x+.5f;else {near.x=5;near.z=direction.z>0?limits.max.z-.5f:limits.min.z+.5f;}
    ball.Body.position=ball.Pitch.TransformPoint(near);ball.Body.WakeUp();ball.Body.linearVelocity=ball.Pitch.TransformDirection(direction)*18;
    yield return new WaitForSeconds(.3f);var inside=ball.Pitch.InverseTransformPoint(ball.Body.position);
    Check(inside.x>=limits.min.x+radius-.002f&&inside.x<=limits.max.x-radius+.002f&&inside.z>=limits.min.z+radius-.002f&&inside.z<=limits.max.z-radius+.002f,"BALL_FULLY_INSIDE_EDGE_"+edge);
    Check(Mathf.Abs(Vector3.Dot(ball.Body.linearVelocity,ball.Pitch.TransformDirection(direction)))<.08f,"OUTWARD_SPEED_STOPS_AT_EDGE_"+edge);
    ball.ResetBall();var playerNear=ball.Pitch.TransformPoint(near);playerNear.y=origin.y-.22f;
    var heading=ball.Pitch.TransformDirection(direction);Place(actor,playerNear,Mathf.Atan2(heading.x,heading.z)*Mathf.Rad2Deg);
    DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,sprint=true,heading=Mathf.Atan2(heading.x,heading.z)*Mathf.Rad2Deg};yield return new WaitForSeconds(.6f);DevelopmentProbe.TurnCommand=default;
    var outside=ball.Pitch.InverseTransformPoint(actor.transform.position);
    Check(Vector3.Dot(outside-near,direction)>.7f,"PLAYER_CAN_CROSS_EDGE_"+edge++);
   }
   ball.ResetBall();Place(actor,origin+new Vector3(-4,-.22f,-4));var corner=fieldCentre;corner.x=limits.max.x-.4f;corner.z=limits.max.z-.4f;
   ball.Body.position=ball.Pitch.TransformPoint(corner);ball.Body.WakeUp();ball.Body.linearVelocity=ball.Pitch.TransformDirection(new Vector3(1,0,1).normalized)*18;
   yield return new WaitForSeconds(.3f);var cornerStop=ball.Pitch.InverseTransformPoint(ball.Body.position);
   Check(cornerStop.x<=limits.max.x-radius+.002f&&cornerStop.z<=limits.max.z-radius+.002f&&ball.Body.linearVelocity.magnitude<.08f,"CORNER_STOPS_BOTH_OUTWARD_AXES");
   ball.ResetBall();var along=fieldCentre;along.x=limits.max.x+.1f;ball.Body.position=ball.Pitch.TransformPoint(along);ball.Body.WakeUp();ball.Body.linearVelocity=ball.Pitch.TransformDirection(Vector3.forward)*4;
   yield return new WaitForSeconds(.3f);var slide=ball.Pitch.InverseTransformPoint(ball.Body.position);
   Check(slide.x<=limits.max.x-radius+.002f&&slide.z>fieldCentre.z+.4f,"BOUNDARY_RETAINS_ALONG_LINE_ROLLING");
   Check(ball.GoalCount==2,"BOTH_AUTHORED_GOAL_VOLUMES_FOUND");
   for(int goalIndex=0;goalIndex<ball.GoalCount;goalIndex++){
    var goal=ball.GoalBounds(goalIndex);int sign=goal.center.z>fieldCentre.z?1:-1;float line=sign>0?limits.max.z:limits.min.z;
    ball.ResetBall();var shot=fieldCentre;shot.x=goal.center.x;shot.z=line-sign*.9f;ball.Body.position=ball.Pitch.TransformPoint(shot);
    var shotDirection=ball.Pitch.TransformDirection(Vector3.forward*sign);var kicker=ball.Body.position-shotDirection*.9f; kicker.y=origin.y-.22f;
    Place(actor,kicker,Mathf.Atan2(shotDirection.x,shotDirection.z)*Mathf.Rad2Deg);yield return new WaitForSeconds(.4f);
    Check(actor.TryKick(),"KICK_INTO_GOAL_"+goalIndex);yield return new WaitForSeconds(.35f);
    var scored=ball.Pitch.InverseTransformPoint(ball.Body.position);
    Check((scored.z-line)*sign>radius&&scored.x>=goal.min.x+radius-.002f&&scored.x<=goal.max.x-radius+.002f,"BALL_ENTERS_GOAL_"+goalIndex);
    yield return new WaitForSeconds(.4f);scored=ball.Pitch.InverseTransformPoint(ball.Body.position);
    Check(scored.z>=goal.min.z+radius-.002f&&scored.z<=goal.max.z-radius+.002f&&ball.Body.linearVelocity.magnitude<.08f,"GOAL_BACK_RETAINS_BALL_"+goalIndex);
    Place(actor,origin+new Vector3(-4,-.22f,-4));ball.Body.WakeUp();ball.Body.linearVelocity=ball.Pitch.TransformDirection(Vector3.right)*18;yield return new WaitForSeconds(.3f);
    scored=ball.Pitch.InverseTransformPoint(ball.Body.position);Check(scored.x<=goal.max.x-radius+.002f&&(scored.z-line)*sign>radius,"GOAL_SIDE_RETAINS_BALL_"+goalIndex);
    scored.x=goal.center.x;ball.Body.position=ball.Pitch.TransformPoint(scored);ball.Body.WakeUp();ball.Body.linearVelocity=-shotDirection*9;yield return new WaitForSeconds(.5f);
    Check((ball.Pitch.InverseTransformPoint(ball.Body.position).z-line)*sign<-.3f,"BALL_CAN_RETURN_FROM_GOAL_"+goalIndex);
    ball.ResetBall();shot.x=goal.center.x;shot.z=line-sign*.5f;shot.y+=3;ball.Body.position=ball.Pitch.TransformPoint(shot);ball.Body.WakeUp();ball.Body.linearVelocity=shotDirection*18;yield return new WaitForSeconds(.2f);
    Check((ball.Pitch.InverseTransformPoint(ball.Body.position).z-line)*sign<=-radius+.002f,"ABOVE_CROSSBAR_STILL_BOUNDED_"+goalIndex);
   }
   yield return GoalNetAudit(ball,actor,origin);
   ball.Body.position=origin+Vector3.down*30;yield return new WaitForFixedUpdate();Check(Vector3.Distance(ball.Body.position,origin)<.1f,"FALL_RECOVERY_TO_CENTRE");
   var parent=ball.transform.parent;var previous=parent.position;parent.position+=Vector3.right*100;ball.ResetBall();Check(Vector3.Distance(ball.Body.position,origin+Vector3.right*100)<.1f,"CENTRE_FOLLOWS_STREAMED_ISLAND_OFFSET");parent.position=previous;Physics.SyncTransforms();ball.ResetBall();
   Check(Vector3.Distance(ball.Body.position,origin)<.1f,"CENTRE_RESTORED_AFTER_ISLAND_REBASE");
   previous=parent.position;parent.position+=Vector3.right*100;ball.Body.position=ball.Pitch.TransformPoint(new Vector3(limits.max.x+1,fieldCentre.y,fieldCentre.z));yield return null;yield return null;
   Check(ball.Pitch.InverseTransformPoint(ball.Body.position).x<=limits.max.x-radius+.002f,"BOUNDARY_FOLLOWS_STREAMED_ISLAND_OFFSET");parent.position=previous;Physics.SyncTransforms();ball.ResetBall();
   Place(actor,origin+new Vector3(0,-.22f,-.9f));
   yield return new WaitForSeconds(.4f);ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,-.9f));yield return new WaitForSeconds(.2f);
   button.OnPointerDown(kickPointer);button.OnPointerUp(kickPointer);yield return new WaitForSeconds(.08f);
   float weak=Vector3.ProjectOnPlane(ball.Body.linearVelocity,Vector3.up).magnitude;
   Check(Mathf.Abs(weak-ball.minimumKickSpeed*FootballEffort.BallPace)<.6f,"SHORT_TAP_MINIMUM_KICK");
   yield return new WaitForSeconds(.4f);ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,-.9f));yield return new WaitForSeconds(.2f);
   Check(PlayerView.Instance.BeginKick(),"KEYBOARD_CHARGE_BEGINS");yield return new WaitForSeconds(ball.maximumChargeTime*.5f);float half=PlayerView.Instance.Charge;PlayerView.Instance.EndKick();yield return new WaitForSeconds(.08f);
   Check(Vector3.ProjectOnPlane(ball.Body.linearVelocity,Vector3.up).magnitude>weak+1&&Mathf.Abs(Vector3.ProjectOnPlane(ball.Body.linearVelocity,Vector3.up).magnitude-Mathf.Lerp(ball.minimumKickSpeed,ball.kickSpeed,half)*FootballEffort.BallPace)<.6f,"HALF_CHARGE_INTERPOLATES_FORCE");
   yield return new WaitForSeconds(.4f);ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,-.9f));yield return new WaitForSeconds(.2f);
   button.OnPointerDown(kickPointer);yield return new WaitForSeconds(.2f);kickPointer.position=RectTransformUtility.WorldToScreenPoint(null,button.cancelArea.position);button.OnDrag(kickPointer);button.OnPointerUp(kickPointer);yield return new WaitForSeconds(.1f);
   Check(!PlayerView.Instance.Charging&&ball.CurrentController==actor&&ball.Body.isKinematic,"CANCEL_AREA_DRAG_RETAINS_BALL_WITHOUT_KICK");
   kickPointer.position=RectTransformUtility.WorldToScreenPoint(null,button.GetComponent<RectTransform>().position);
   Check(PlayerView.Instance.BeginKick(),"CHARGE_FOR_CHANGED_FACING");yield return new WaitForSeconds(.2f);Place(actor,origin+new Vector3(-.9f,-.22f,0),90);yield return new WaitForSeconds(.1f);PlayerView.Instance.EndKick();yield return new WaitForSeconds(.08f);
   Check(ball.Body.linearVelocity.x>2&&Mathf.Abs(ball.Body.linearVelocity.z)<.15f,"RELEASE_USES_CURRENT_FACING");
   yield return new WaitForSeconds(.4f);ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,-.9f));yield return new WaitForSeconds(.2f);PlayerView.Instance.BeginKick();ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,-4));yield return new WaitForSeconds(.1f);PlayerView.Instance.EndKick();yield return new WaitForSeconds(.08f);
   Check(!PlayerView.Instance.Charging&&ball.Body.linearVelocity.magnitude<.08f,"OUT_OF_RANGE_CHARGE_CANCELS");
   Check(!ball.TryKick(actor,float.NaN)&&!ball.TryKick(actor,float.PositiveInfinity),"NONFINITE_CHARGE_REJECTED");
   ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,-.9f));yield return new WaitForSeconds(.2f);Check(ball.TryKick(actor,5),"OVERSIZED_CHARGE_CLAMPED");yield return new WaitForSeconds(.08f);
   Check(Vector3.ProjectOnPlane(ball.Body.linearVelocity,Vector3.up).magnitude<=ball.kickSpeed*FootballEffort.BallPace+.1f,"CHARGE_CANNOT_EXCEED_MAXIMUM_SPEED");
   yield return BalanceAudit(ball,actor,origin);
   ball.ResetBall();DevelopmentProbe.TurnCommandActive=false;File.AppendAllText(ReportPath,"FOOTBALL_PHYSICS_COMPLETE\n");Debug.Log("FOOTBALL_PHYSICS_COMPLETE "+ReportPath);
  }

  IEnumerator GoalNetAudit(FootballBall ball,Athlete actor,Vector3 origin){
   DevelopmentProbe.TurnCommand=default;ball.ResetBall();
   var nets=ball.Pitch.GetComponentsInChildren<FootballGoalNet>(true);
   Check(nets.Length==2,"BOTH_GOALS_HAVE_CONTINUOUS_NET_COLLISION");
   foreach(var net in nets)Check(net.GetComponentsInChildren<MeshCollider>().Length==4&&net.GetComponentsInChildren<MeshCollider>().All(c=>c.convex&&!c.isTrigger&&c.gameObject.layer==8),"NET_FOUR_SOLID_WORLD_PANELS_"+net.transform.parent.name);
   for(int i=0;i<ball.GoalCount;i++){
    var bounds=ball.GoalBounds(i);float front=ball.GoalFront(i);int sign=ball.GoalSign(i);float back=sign>0?bounds.max.z:bounds.min.z;
    File.AppendAllText(ReportPath,$"NET geometry goal={i} bounds={bounds} front={front:F3} back={back:F3} roofBack={FootballGoalNet.RoofHeight(bounds,front,sign,back):F3}\n");
    foreach(int face in new[]{0,1,2})foreach(bool outside in new[]{false,true})foreach(int mode in new[]{0,1,2}){
     Vector3 normal=face==0?Vector3.left:face==1?Vector3.right:Vector3.forward*sign;
     var surface=new Vector3(face==0?bounds.min.x:face==1?bounds.max.x:bounds.center.x,bounds.min.y,face==2?back:(front+back)*.5f);
     var position=ball.Pitch.TransformPoint(surface+normal*(outside?.8f:-.8f));var direction=ball.Pitch.TransformDirection(normal*(outside?-1:1));float yaw=Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg;
     Place(actor,position,yaw);var command=new PlayerCommand{move=Vector2.up,heading=yaw,sprint=mode==1};
     if(mode==2){Check(actor.TryTackle(),$"NET_SLIDE_START_{i}_{face}_{outside}");command.move=Vector2.zero;}
     for(int step=0;step<12;step++)actor.Simulate(command,.05f);
     float distance=Vector3.Dot(ball.Pitch.InverseTransformPoint(actor.transform.position)-surface,normal);
     Check(outside?distance>.25f:distance<-.25f,$"NET_BLOCKS_{(mode==0?"WALK":mode==1?"SPRINT":"SLIDE")}_{i}_{face}_{(outside?"OUTSIDE":"INSIDE")}");
    }
    var mouth=new Vector3(bounds.center.x,bounds.min.y,front-sign*.8f);var heading=ball.Pitch.TransformDirection(Vector3.forward*sign);float mouthYaw=Mathf.Atan2(heading.x,heading.z)*Mathf.Rad2Deg;
    Place(actor,ball.Pitch.TransformPoint(mouth),mouthYaw);for(int step=0;step<12;step++)actor.Simulate(new PlayerCommand{move=Vector2.up,heading=mouthYaw},.05f);
    Check((ball.Pitch.InverseTransformPoint(actor.transform.position).z-front)*sign>.4f,"PLAYER_CAN_ENTER_OPEN_GOAL_MOUTH_"+i);
    Place(actor,actor.transform.position,mouthYaw+180);for(int step=0;step<12;step++)actor.Simulate(new PlayerCommand{move=Vector2.up,heading=mouthYaw+180},.05f);
    Check((ball.Pitch.InverseTransformPoint(actor.transform.position).z-front)*sign<-.4f,"PLAYER_CAN_EXIT_OPEN_GOAL_MOUTH_"+i);
    var middle=new Vector3(bounds.center.x,bounds.min.y,(front+back)*.5f);Place(actor,ball.Pitch.TransformPoint(middle));actor.RequestJump();float highest=actor.capsule.bounds.max.y;
    for(int step=0;step<40;step++){actor.Simulate(default,.05f);highest=Mathf.Max(highest,actor.capsule.bounds.max.y);}
    float ceiling=ball.Pitch.TransformPoint(new Vector3(middle.x,FootballGoalNet.RoofHeight(bounds,front,sign,middle.z),middle.z)).y;
    Check(highest<=ceiling+.04f,"JUMP_CANNOT_PASS_NET_ROOF_"+i);
   }
   Place(actor,origin+new Vector3(-4,-.22f,-4));ball.ResetBall();yield return null;
  }

  IEnumerator BalanceAudit(FootballBall ball,Athlete actor,Vector3 origin){
   var run=new PlayerCommand{move=Vector2.up,sprint=true};
   DevelopmentProbe.TurnCommand=default;ball.ResetBall();Place(actor,origin+new Vector3(5,-.22f,-5));
   DevelopmentProbe.TurnCommand=run;yield return new WaitForSeconds(.6f);float sprint=actor.speed;
   Check(Mathf.Abs(sprint-10.5f)<.1f,"CONTROL_UNLADEN_SPRINT_150_PERCENT");
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up};yield return new WaitForSeconds(.4f);float walk=actor.speed;
   Check(Mathf.Abs(walk-6)<.1f,"CONTROL_UNLADEN_WALK_150_PERCENT");
   DevelopmentProbe.TurnCommand=default;ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,-.9f));yield return new WaitForSeconds(.2f);
   Check(ball.CurrentController==actor&&ball.Body.isKinematic,"CONTROL_ACQUIRE_IS_ATTACHED_STATE");
   yield return CaptureControl(actor,"acquired");
   Check(Mathf.Abs(ball.MovementSpeed(actor,false,false)-4.5f)<.01f&&Mathf.Abs(ball.MovementSpeed(actor,true,false)-7.875f)<.01f,"CONTROL_BASE_SPEED_TIMES_POINT75_ONCE");
   for(int i=0;i<30;i++)ball.MovementSpeed(actor,true,false);
   Check(Mathf.Abs(ball.MovementSpeed(actor,true,false)-7.875f)<.01f,"CONTROL_REPEATED_QUERIES_NO_COMPOUND_SLOWDOWN");
   Check(!actor.CanRequestJump&&!actor.TackleReady&&!actor.TryTackle()&&!actor.GetComponent<FootballTackle>().TryStart(true),"CONTROL_JUMP_TACKLE_AND_DIRECT_SLIDE_BLOCKED");
   actor.RequestJump();Check(!actor.LoadingJump&&!actor.Airborne,"CONTROL_DIRECT_JUMP_REQUEST_BLOCKED");
   var rival=Instantiate(AppRoot.Instance.athletePrefab).GetComponent<Athlete>();rival.Setup();Place(rival,actor.transform.position+new Vector3(.1f,0,0));ball.RefreshControl(rival);
   Check(ball.CurrentController==actor&&ball.MovementSpeed(rival,true,false)==10.5f&&!ball.TryKick(rival,0),"CONTROL_PROXIMITY_CONTACT_AND_OTHER_KICK_CANNOT_STEAL");
   Place(rival,origin+new Vector3(4,-.22f,-4));
   foreach(float yaw in new[]{180f,90f,270f,0f}){
    actor.transform.rotation=Quaternion.Euler(0,yaw,0);actor.ResetLocomotion();ball.RefreshControl();yield return null;
    Check(ball.CurrentController==actor&&Vector3.Distance(ball.Body.position,ball.FootPosition(actor))<.002f&&Vector3.Distance(ball.transform.position,ball.FootPosition(actor))<.002f,"CONTROL_STATIONARY_TURN_FOLLOWS_"+yaw);
    if(yaw==180)yield return CaptureControl(actor,"turn180");
   }
   float maxGap=0,dribble=0;
   foreach(bool running in new[]{false,true}){
    DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,sprint=running};float until=Time.time+.7f;
    while(Time.time<until){yield return null;maxGap=Mathf.Max(maxGap,Vector3.Distance(ball.transform.position,ball.FootPosition(actor)));dribble=actor.speed;}
    Check(ball.CurrentController==actor&&maxGap<.12f&&Mathf.Abs(dribble-(running?7.875f:4.5f))<.1f,running?"CONTROL_SPRINT_STAYS_ATTACHED_AT_7_POINT875":"CONTROL_WALK_STAYS_ATTACHED_AT_4_POINT5");
   }
   DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.2f);
   Check(PlayerView.Instance.BeginKick(),"CONTROL_CHARGE_START");DevelopmentProbe.TurnCommand=run;yield return new WaitForSeconds(.2f);
   Check(actor.Motor.Velocity.magnitude<=3.61f&&ball.CurrentController==actor&&Vector3.Distance(ball.Body.position,ball.FootPosition(actor))<.002f,"CONTROL_CHARGE_RETAINS_ATTACHMENT_SEPARATE_SPEED");
   var aim=PlayerView.Instance.GetComponentsInChildren<MeshRenderer>(true).FirstOrDefault(r=>r.name=="Kick aim");
   Check(aim&&aim.enabled&&Vector3.Dot(aim.transform.forward,FootballBall.KickDirection(actor))>.999f,"CONTROL_CHARGE_AIM_MATCHES_KICK_DIRECTION");
   float AimLength()=>aim.GetComponent<MeshFilter>().sharedMesh.bounds.size.z*aim.transform.lossyScale.z;
   Check(aim.GetComponent<MeshFilter>().sharedMesh.vertexCount==4&&aim.sharedMaterial.shader.name=="WhatTheFish/FootballKickCharge","CONTROL_CHARGE_TWIN_RAIL_PROCEDURAL_AIM");
   var aimColor=aim.sharedMaterial.GetColor("_BaseColor");float earlyAimLength=AimLength();
   File.AppendAllText(ReportPath,$"MEASURE aim color={aimColor} surface={aim.sharedMaterial.GetFloat("_Surface")} zwrite={aim.sharedMaterial.GetFloat("_ZWrite")} queue={aim.sharedMaterial.renderQueue} charge={PlayerView.Instance.Charge} graphics={SystemInfo.graphicsDeviceType}\n");
   Check(aimColor.a==.9f&&aim.sharedMaterial.GetFloat("_Charge")>0&&aim.sharedMaterial.GetFloat("_Surface")==1&&aim.sharedMaterial.GetFloat("_ZWrite")==0&&aim.sharedMaterial.renderQueue==3000,"CONTROL_CHARGE_AIM_COLOR_FLOW_TRANSPARENT");
   yield return new WaitForSeconds(ball.maximumChargeTime+.1f);yield return null;yield return null;
   Check(PlayerView.Instance.Charge==1&&aim.sharedMaterial.GetColor("_BaseColor")==new Color(1,.2f,.32f,.9f)&&Mathf.Abs(aim.transform.localScale.z-4.4f)<.001f&&Mathf.Abs(aim.transform.localScale.x-1.25f)<.001f&&AimLength()>5.4f&&AimLength()<5.5f&&earlyAimLength<AimLength(),"CONTROL_FULL_CHARGE_RAINBOW_RAILS_DOUBLE_LENGTH_AND_WIDER");
   yield return CaptureControl(actor,"charge-aim");
   PlayerView.Instance.CancelKick(int.MinValue);DevelopmentProbe.TurnCommand=default;
   Check(aim&&!aim.enabled,"CONTROL_CANCEL_HIDES_KICK_AIM");
   Check(PlayerView.Instance.BeginKick(),"CONTROL_AIM_RECHARGE");yield return null;yield return null;
   Check(aim.enabled,"CONTROL_RECHARGE_REUSES_KICK_AIM");PlayerView.Instance.EndKick();
   Check(!aim.enabled,"CONTROL_RELEASE_HIDES_KICK_AIM");PlayerView.Instance.ClearMatchInput();
   Check(PlayerView.Instance.BeginKick(),"CONTROL_AIM_BEFORE_TRAVEL");actor.inTransit=true;yield return null;yield return null;
   Check(!aim.enabled&&!PlayerView.Instance.Charging,"CONTROL_TRAVEL_CANCELS_KICK_AIM");actor.inTransit=false;
   yield return null;yield return null;
   var kickButton=FindFirstObjectByType<KickButton>();var view=PlayerView.Instance;
   Check(kickButton&&kickButton.cancelArea,"FAKE_SHOT_CANCEL_AREA_EXISTS");
   var kickPoint=RectTransformUtility.WorldToScreenPoint(null,kickButton.GetComponent<RectTransform>().position);
   var cancelPoint=RectTransformUtility.WorldToScreenPoint(null,kickButton.cancelArea.position);
   var finger=new PointerEventData(EventSystem.current){pointerId=42,button=PointerEventData.InputButton.Left,position=kickPoint};
   kickButton.OnInitializePotentialDrag(finger);kickButton.OnPointerDown(finger);
   Check(view.Charging&&!finger.useDragThreshold&&kickButton.cancelArea.gameObject.activeSelf,"FAKE_SHOT_CHARGE_SHOWS_CANCEL_AREA");
   yield return new WaitForSeconds(.2f);
   yield return CaptureControl(actor,"fake-shot-charge");
   // The previously inactive cancel rect is laid out when the charge shows it.
   // Drag to its displayed position, not its pre-activation layout coordinates.
   Canvas.ForceUpdateCanvases();cancelPoint=RectTransformUtility.WorldToScreenPoint(null,kickButton.cancelArea.position);
   var otherFinger=new PointerEventData(EventSystem.current){pointerId=43,button=PointerEventData.InputButton.Left,position=cancelPoint};
   kickButton.OnDrag(otherFinger);kickButton.OnPointerUp(otherFinger);
   Check(view.Charging,"FAKE_SHOT_OTHER_FINGER_CANNOT_CANCEL_OR_RELEASE");
   finger.position=cancelPoint;kickButton.OnBeginDrag(finger);
   File.AppendAllText(ReportPath,$"CANCEL state charging={view.Charging} aim={aim.enabled} owner={ball.CurrentController==actor} kinematic={ball.Body.isKinematic} point={cancelPoint} contains={RectTransformUtility.RectangleContainsScreenPoint(kickButton.cancelArea,cancelPoint,null)}\n");
   Check(!view.Charging&&!view.ReadCommand().kick&&!aim.enabled&&ball.CurrentController==actor&&ball.Body.isKinematic,"FAKE_SHOT_CANCEL_NEVER_RELEASES_BALL");
   finger.position=kickPoint;kickButton.OnDrag(finger);kickButton.OnPointerDown(finger);
   Check(!view.Charging,"FAKE_SHOT_RETURN_TO_KICK_CANNOT_RESTART_SAME_GESTURE");
   // Real second-finger joystick input stays independent of the cancelled Kick gesture.
   DevelopmentProbe.TurnCommandActive=false;var stick=view.stick;
   otherFinger.position=RectTransformUtility.WorldToScreenPoint(null,stick.transform.position);stick.OnPointerDown(otherFinger);
   otherFinger.position+=Vector2.right*(70*stick.transform.lossyScale.x);stick.OnDrag(otherFinger);
   float beforeTurn=actor.transform.eulerAngles.y;yield return new WaitForSeconds(.3f);
   Check(actor.speed>0&&Mathf.Abs(Mathf.DeltaAngle(beforeTurn,actor.transform.eulerAngles.y))>10&&ball.CurrentController==actor&&ball.Body.isKinematic,"FAKE_SHOT_LEFT_JOYSTICK_TURNS_AND_DRIBBLES_WITHOUT_RELEASE");
   stick.OnPointerUp(otherFinger);DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   kickButton.OnPointerUp(finger);yield return null;
   Check(!view.Charging&&!view.ReadCommand().kick&&!kickButton.cancelArea.gameObject.activeSelf&&ball.CurrentController==actor,"FAKE_SHOT_CANCELLED_RELEASE_HAS_NO_DELAYED_KICK");
   kickButton.OnPointerDown(finger);Check(view.Charging,"FAKE_SHOT_FRESH_PRESS_RESTARTS_CHARGE");kickButton.OnPointerUp(finger);
   var lightCommand=view.ReadCommand();Check(lightCommand.kick&&lightCommand.kickCharge<.01f,"FAKE_SHOT_NORMAL_TAP_STILL_QUEUES_LIGHT_KICK");
   kickButton.OnPointerDown(finger);yield return new WaitForSeconds(ball.maximumChargeTime+.1f);kickButton.OnPointerUp(finger);
   var fullCommand=view.ReadCommand();Check(fullCommand.kick&&fullCommand.kickCharge==1,"FAKE_SHOT_NORMAL_HOLD_STILL_QUEUES_FULL_KICK");
   kickButton.OnPointerDown(finger);finger.position=cancelPoint;kickButton.OnPointerUp(finger);
   Check(!view.Charging&&!view.ReadCommand().kick&&ball.CurrentController==actor&&ball.Body.isKinematic,"FAKE_SHOT_RELEASE_IN_CANCEL_AREA_WITHOUT_DRAG_STILL_CANCELS");
   ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,-.9f));yield return new WaitForSeconds(.2f);
   Check(actor.TryKick(0),"CONTROL_LIGHT_KICK_RELEASE");float light=ball.Body.linearVelocity.magnitude;
   Check(!ball.CurrentController&&!ball.Body.isKinematic&&ball.MovementSpeed(actor,true,false)==10.5f,"CONTROL_KICK_CLEAR_OWNER_RESTORE_DYNAMIC_AND_FULL_SPEED");
   // Check the immediate exclusion before rendering can advance the real ball out of range.
   ball.Body.position=ball.FootPosition(actor);ball.Body.linearVelocity=Vector3.zero;ball.Body.angularVelocity=Vector3.zero;Physics.SyncTransforms();ball.RefreshControl(actor);
   Check(!ball.CurrentController,"CONTROL_RELEASE_REQUIRES_FRESH_APPROACH_NO_TIMER");
   Place(rival,actor.transform.position+Vector3.right*.15f);ball.RefreshControl(rival);
   Check(ball.CurrentController==rival,"CONTROL_OTHER_PLAYER_CAN_RECEIVE_IMMEDIATELY");
   ball.ResetBall();Place(rival,origin+new Vector3(4,-.22f,-4));Place(actor,origin+new Vector3(0,-.22f,-.9f));yield return new WaitForSeconds(.2f);
   Check(actor.TryKick(0),"CONTROL_LIGHT_KICK_PRESENTATION");yield return new WaitForFixedUpdate();
   Check(ball.Body.linearVelocity.magnitude>sprint,"CONTROL_LIGHT_FIRST_PHYSICS_FASTER_THAN_RUN");yield return CaptureControl(actor,"kicked");
   ball.ResetBall();Place(rival,origin+new Vector3(4,-.22f,-4));Place(actor,origin+new Vector3(0,-.22f,-.9f));yield return new WaitForSeconds(.4f);Check(actor.TryKick(1),"CONTROL_FULL_KICK_RELEASE");float full=ball.Body.linearVelocity.magnitude;
   Check(full>light*1.5f,"CONTROL_FULL_CLEARLY_STRONGER_THAN_LIGHT");
   ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,-.9f));yield return new WaitForSeconds(.4f);
   // A slide touching only the held ball, or missing both, cannot eject possession.
   Place(rival,ball.Body.position+new Vector3(1.4f,-.22f,0),270);Check(rival.TryTackle(),"CONTROL_OPPONENT_SLIDE_START");
   rival.Simulate(default,.1f);ball.Intercept(rival);
   Check(ball.CurrentController==actor,"CONTROL_MISSED_TACKLE_AND_BALL_ONLY_CONTACT_KEEP_OWNER");
   var hit=actor.GetComponent<FootballTackle>();int beforeHit=hit.HitsReceived;
   Place(rival,actor.transform.position+Vector3.left*1.4f,90);Check(rival.TryTackle(),"CONTROL_ACTUAL_PLAYER_SLIDE_START");rival.Simulate(default,.1f);
   Check(hit.HitsReceived==beforeHit+1&&actor.Action==FootballAction.Hit&&!ball.CurrentController&&!ball.Body.isKinematic,"CONTROL_VALID_PLAYER_SLIDE_HIT_RELEASES_FREE_BALL");
   float slide=Vector3.ProjectOnPlane(ball.Body.linearVelocity,Vector3.up).magnitude;
   Check(Mathf.Abs(ball.Body.linearVelocity.y-ball.tackleBallLift*FootballEffort.BallPace)<.01f,"CONTROL_TACKLE_LAUNCHES_BALL_UPWARD");
   for(int i=0;i<20;i++)ball.Intercept(rival);
   Check(slide<=ball.tackleBallSpeed*FootballEffort.BallPace+.01f&&slide<light&&Vector3.ProjectOnPlane(ball.Body.linearVelocity,Vector3.up).magnitude<=slide+.01f&&ball.Body.linearVelocity.y<=ball.tackleBallLift*FootballEffort.BallPace+.01f,"CONTROL_SINGLE_SLIDE_NO_DUPLICATE_ACCELERATION");
   ball.RefreshControl(actor);ball.RefreshControl(rival);Check(!ball.CurrentController,"CONTROL_TACKLER_AND_VICTIM_DO_NOT_AUTO_RECEIVE");
   float launchHeight=ball.Body.position.y;yield return new WaitForSeconds(.12f);
   Check(ball.Body.position.y>launchHeight+.2f,"CONTROL_TACKLE_BALL_ACTUALLY_RISES");
   yield return new WaitForSeconds(1);
   rival.ResetLocomotion();Place(rival,origin+new Vector3(8,-.22f,-8));actor.ResetLocomotion();ball.RefreshControl(actor);
   var free=ball.Body.position;Place(actor,free+new Vector3(0,-.22f,-3));ball.RefreshControl(actor);Place(actor,free+new Vector3(0,-.22f,-.9f));ball.Body.linearVelocity=Vector3.zero;ball.RefreshControl(actor);
   Check(ball.CurrentController==actor&&ball.Body.isKinematic,"CONTROL_FRESH_APPROACH_CAN_REGAIN_AFTER_TACKLE");
   ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,-.9f));ball.Body.linearVelocity=Vector3.forward*15;ball.RefreshControl(actor);
   Check(!ball.CurrentController,"CONTROL_FAST_PASS_NOT_AUTO_ACQUIRED");
   ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,-.9f),180);ball.RefreshControl(actor);Check(!ball.CurrentController,"CONTROL_BACK_TO_BALL_NOT_ACQUIRED");
   ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,-.9f));Check(actor.TryTackle(),"CONTROL_FREE_PLAYER_CAN_SLIDE");ball.RefreshControl(actor);Check(!ball.CurrentController,"CONTROL_SLIDING_PLAYER_CANNOT_ACQUIRE");
   Check(ball.Intercept(actor)&&Mathf.Abs(ball.Body.linearVelocity.y-ball.tackleBallLift*FootballEffort.BallPace)<.01f,"CONTROL_FREE_BALL_SLIDE_LAUNCHES_UPWARD");
   var freeLaunch=ball.Body.linearVelocity;ball.Intercept(actor);
   Check(Vector3.Distance(ball.Body.linearVelocity,freeLaunch)<.001f,"CONTROL_FREE_BALL_SLIDE_SINGLE_LAUNCH");ball.ResetBall();
   actor.ResetLocomotion();Place(actor,origin+new Vector3(0,-.22f,-.9f));ball.RefreshControl(actor);Check(ball.CurrentController==actor,"CONTROL_NORMAL_PLAYER_REACQUIRES");
   ball.ResetBall();Check(!ball.CurrentController&&!Physics.GetIgnoreCollision(ball.GetComponent<SphereCollider>(),actor.capsule),"CONTROL_RESET_RESTORES_COLLISION_AND_CLEARS_OWNER");
   Place(actor,origin+new Vector3(0,-.22f,-.9f));ball.RefreshControl(actor);actor.gameObject.SetActive(false);Check(!ball.CurrentController&&!ball.Body.isKinematic,"CONTROL_DESPAWN_RELEASES_PHYSICS");actor.gameObject.SetActive(true);actor.ResetLocomotion();
   ball.ResetBall();Place(actor,origin+new Vector3(0,-.22f,-.9f));ball.RefreshControl(actor);actor.inTransit=true;ball.RefreshControl();Check(!ball.CurrentController,"CONTROL_TRAVEL_RELEASES_OWNER");actor.inTransit=false;
   foreach(var outward in new[]{Vector3.right,Vector3.left,Vector3.forward,Vector3.back}){
    ball.ResetBall();var local=ball.Pitch.InverseTransformPoint(origin);local.x=outward.x==0?5:outward.x>0?ball.PitchBounds.max.x-.4f:ball.PitchBounds.min.x+.4f;local.z=outward.z==0?0:outward.z>0?ball.PitchBounds.max.z-.4f:ball.PitchBounds.min.z+.4f;
    ball.Body.position=ball.Pitch.TransformPoint(local);var forward=ball.Pitch.TransformDirection(outward);var feet=ball.Body.position-forward*.9f;feet.y=origin.y-.22f;float yaw=Mathf.Atan2(forward.x,forward.z)*Mathf.Rad2Deg;Place(actor,feet,yaw);ball.RefreshControl(actor);
    for(int i=0;i<5;i++)actor.Simulate(new PlayerCommand{move=Vector2.up,sprint=true,heading=yaw},.1f);
    var at=ball.Pitch.InverseTransformPoint(ball.Body.position);Check(ball.CurrentController==actor&&Vector3.Distance(ball.Body.position,ball.FootPosition(actor))<.002f&&at.x<=ball.PitchBounds.max.x-.218f&&at.x>=ball.PitchBounds.min.x+.218f&&at.z<=ball.PitchBounds.max.z-.218f&&at.z>=ball.PitchBounds.min.z+.218f,"CONTROL_ATTACHED_FIELD_EDGE_"+outward);
   }
   Destroy(rival.gameObject);DevelopmentProbe.TurnCommand=default;
   File.AppendAllText(ReportPath,$"MEASURE attached walk={walk:F3} sprint={sprint:F3} dribbleSprint={dribble:F3} maxPresentationGap={maxGap:F4}m light={light:F3} full={full:F3} tackle={slide:F3}m/s\n");
   yield return null;
  }
 }
}
#endif
