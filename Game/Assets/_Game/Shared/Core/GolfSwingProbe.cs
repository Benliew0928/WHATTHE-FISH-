#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 // Opt-in interaction regression: actual UI, club rig and authoritative ball.
 public sealed class GolfSwingProbe:MonoBehaviour {
  string report;int checks;AppRoot app;GolfMatchManager match;GameObject floor;Athlete helper;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){if(Environment.GetCommandLineArgs().Contains("-golfSwingAudit"))new GameObject("Golf swing interaction checks").AddComponent<GolfSwingProbe>();}
  void Check(bool value,string name){File.AppendAllText(report,(value?"PASS ":"FAIL ")+name+"\n");if(!value)throw new Exception(name);checks++;}
  static IEnumerator Frame(){if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)yield return null;else yield return new WaitForEndOfFrame();}
  void Place(Athlete actor,Vector3 position,float yaw=0){actor.capsule.enabled=false;actor.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));actor.ResetLocomotion();actor.capsule.enabled=true;Physics.SyncTransforms();}
  void Capture(string name){if(!Environment.GetCommandLineArgs().Contains("-golfSwingCapture"))return;var camera=Camera.main;var p=camera.transform.position;var q=camera.transform.rotation;float fov=camera.fieldOfView;camera.transform.position=app.LocalAthlete.transform.position+new Vector3(1.8f,1.5f,1.3f);camera.transform.LookAt(app.LocalAthlete.transform.position+Vector3.up*.65f);camera.fieldOfView=45;FindFirstObjectByType<DevelopmentProbe>()?.CaptureFrame(Path.ChangeExtension(report,name+".png"));camera.transform.SetPositionAndRotation(p,q);camera.fieldOfView=fov;}
  IEnumerator Start(){
   var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-report");report=index>=0?args[index+1]:Path.Combine(Application.persistentDataPath,"golf-swing.txt");
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   float until=Time.time+60;while((!AppRoot.Instance||!AppRoot.Instance.Exploring||AppRoot.Instance.SelectedSport!=SportId.Golf||!GolfMatchManager.Instance)&&Time.time<until)yield return null;
   app=AppRoot.Instance;match=GolfMatchManager.Instance;Check(app&&match&&match.Context,"SWING_CONTEXT");
   var actor=app.LocalAthlete;helper=Instantiate(app.athletePrefab).GetComponent<Athlete>();helper.Setup();
   Check(match.StartTestMatch(new[]{(100UL,actor,"Hitter"),(200UL,helper,"Ball owner")}),"TWO_BALL_MATCH");
   Check(Physics.Raycast(new Vector3(-40,15,-100),Vector3.down,out var ground,40,1<<8,QueryTriggerInteraction.Ignore),"SUPPORTED_TEST_SITE");var site=ground.point+Vector3.up*3;
   floor=new GameObject("Temporary swing access floor");floor.layer=8;floor.transform.position=site-Vector3.up*.1f;floor.AddComponent<BoxCollider>().size=new Vector3(30,.2f,30);Physics.SyncTransforms();
   var ball=match.Ball(200);var other=match.Ball(100);var rest=site+Vector3.up*(GolfBall.Radius+.002f);ball.Place(rest,true);other.Place(rest+Vector3.right*6,true);Place(helper,site+Vector3.right*8+Vector3.up*.04f);
   yield return new WaitForSeconds(.6f);var button=FindFirstObjectByType<GolfSwingButton>();Check(button&&button.button,"SWING_UI_EXISTS");
   foreach(float distance in new[]{.05f,.25f,.5f,.75f,1f,1.08f})for(int angle=0;angle<360;angle+=30){
    Place(actor,site+Quaternion.Euler(0,angle,0)*Vector3.forward*distance+Vector3.up*.04f);yield return new WaitForFixedUpdate();yield return null;
    Check(match.Strikeable(actor)==ball&&button.aimButton.interactable&&!button.button.interactable,$"NEAR_BALL_READY_distance={distance:F2}_angle={angle}");
   }
   Place(actor,site+Vector3.right*.5f+Vector3.up*.04f);app.view.yaw=0;yield return new WaitForFixedUpdate();yield return null;
   var pointer=new PointerEventData(EventSystem.current){pointerId=31,button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,button.button.transform.position)};
   var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);Check(hits.Count>0&&hits[0].gameObject.transform.IsChildOf(button.transform),"SWING_CENTRE_RECEIVES_UI_RAYCAST");
   Check(app.view.BeginGolfAim(),"SIDE_BALL_ENTERS_AIM");yield return null;var feet=actor.transform.position;button.OnPointerDown(pointer);yield return new WaitForSeconds(.35f);yield return Frame();
   Check(app.view.GolfCharging&&app.view.GolfBallOwner==200,"SIDE_BALL_TOUCH_STARTS_CHARGE");
   Check(Vector3.Angle(actor.transform.forward,Vector3.ProjectOnPlane(ball.Body.position-actor.transform.position,Vector3.up))<1&&Vector3.ProjectOnPlane(actor.transform.position-feet,Vector3.up).magnitude<.002f,"AUTO_ADDRESS_TURNS_BODY_WITHOUT_MOVING_FEET");
   Check(actor.GolfClubMotion.TwoHanded&&actor.GolfClubMotion.GripError<.025f,"SIDE_BALL_CHARGE_KEEPS_BOTH_HANDS_ATTACHED");Capture("side-charge");
   other.Place(actor.transform.position+Vector3.forward*.2f+Vector3.up*(GolfBall.Radius-.02f),true);yield return new WaitForFixedUpdate();
   Check(match.Strikeable(actor)==other&&app.view.GolfBallOwner==200,"CHARGE_LOCKS_ORIGINAL_BALL_WHEN_ANOTHER_COMES_CLOSER");
   var otherStart=other.Body.position;button.OnPointerUp(pointer);yield return new WaitForSeconds(.25f);
   Check(match.Player(actor).TotalStroke==1&&match.Player(helper).TotalStroke==0,"TOUCH_RELEASE_COUNTS_ONCE_FOR_HITTER");
   Check(ball.Velocity.z>1&&Mathf.Abs(ball.Velocity.x)<.03f&&Vector3.Distance(other.Body.position,otherStart)<.02f,"RELEASE_HITS_LOCKED_BALL_IN_CAMERA_DIRECTION");yield return new WaitForSeconds(.6f);
   ball.Place(rest,true);yield return new WaitForSeconds(.6f);Check(app.view.BeginGolfAim(),"LOST_BALL_TEST_AIMS");yield return null;button.OnPointerDown(pointer);yield return null;ball.Place(rest+Vector3.right*4,true);yield return new WaitForFixedUpdate();yield return null;button.OnPointerUp(pointer);
   Check(!app.view.GolfCharging&&match.Player(actor).TotalStroke==1,"LOST_LOCKED_BALL_CANCELS_WITHOUT_HITTING_NEARBY_BALL");other.Place(rest+Vector3.right*6,true);ball.Place(rest,true);
   Place(actor,site+Vector3.back*1.3f+Vector3.up*.04f);yield return new WaitForFixedUpdate();yield return null;Check(!match.Strikeable(actor)&&!button.button.interactable,"UNLOCKED_SWING_REQUIRES_AIM");
   Place(actor,site+Vector3.back*.75f+Vector3.up*.04f);yield return new WaitForFixedUpdate();var wall=new GameObject("Temporary swing obstruction");wall.layer=8;wall.transform.position=site+new Vector3(0,.4f,-.38f);wall.AddComponent<BoxCollider>().size=new Vector3(2,1,.08f);Physics.SyncTransforms();yield return null;Check(!match.Strikeable(actor),"WALL_STILL_BLOCKS_SWING");Destroy(wall);yield return null;
   actor.inTransit=true;Check(!match.Strikeable(actor),"TRANSIT_STILL_BLOCKS_SWING");actor.inTransit=false;
   actor.RequestJump();actor.Simulate(default,.02f);Check(actor.LoadingJump&&!match.Strikeable(actor),"JUMP_PREPARATION_STILL_BLOCKS_SWING");Place(actor,site+Vector3.back*.75f+Vector3.up*.04f);yield return new WaitForFixedUpdate();
   foreach(float distance in new[]{.05f,.25f,.5f,.9f,1.08f}){
    ball.Place(rest,true);Place(actor,site+Vector3.right*distance+Vector3.up*.04f,180);yield return new WaitForFixedUpdate();yield return null;
    Check(app.view.BeginGolfAim()&&app.view.BeginGolfSwing(44),$"CLOSE_SIDE_CHARGE_{distance:F2}");yield return new WaitForSeconds(.3f);app.view.EndGolfSwing(44);
    until=Time.time+1;while(actor.GolfClubMotion.State.action!=GolfClubAction.Swing&&Time.time<until)yield return null;
    while(actor.GolfClubMotion.Elapsed<GolfClubMotion.ContactTime)yield return null;yield return Frame();var club=actor.GolfClubMotion;
    File.AppendAllText(report,$"CONTACT distance={distance:F2} elapsed={club.Elapsed:F3} grip={club.GripError:F4} headError={Vector3.Distance(club.Club.head.position,club.State.target):F4}\n");
    Check(club.TwoHanded&&club.GripError<.025f,$"CLOSE_SIDE_CONTACT_BOTH_GRIPS_{distance:F2}");
    Check(club.Club.head.position.y>=club.State.target.y-.02f,$"CLOSE_SIDE_CONTACT_ABOVE_GROUND_{distance:F2}");Capture("contact-"+distance.ToString("F2"));yield return new WaitForSeconds(.8f);
   }
   Check(match.Player(actor).TotalStroke==6,"FIVE_MORE_CLOSE_SWINGS_COUNT_EXACTLY_ONCE");match.ClearMatch();Destroy(helper.gameObject);Destroy(floor);File.AppendAllText(report,"GOLF_SWING_COMPLETE checks="+checks+"\n");
  }
  void OnDestroy(){if(helper)Destroy(helper.gameObject);if(floor)Destroy(floor);}
 }
}
#endif
