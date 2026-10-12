#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 [DefaultExecutionOrder(150)]
 public sealed class StadiumCameraReview:MonoBehaviour {
  string folder;bool failed,manual;AppRoot app;Camera cam;Athlete actor;Rigidbody body;Vector3 ballPosition;
  SimulationMode previousSimulation;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-stadiumCameraReview");
   if(i>=0&&i+1<args.Length)new GameObject("Stadium camera review").AddComponent<StadiumCameraReview>().folder=args[i+1];
  }
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  void Place(Transform field,Vector3 local){actor.capsule.enabled=false;actor.transform.position=field.TransformPoint(local);actor.ResetLocomotion();Physics.SyncTransforms();}
  IEnumerator Settle(){yield return new WaitForSeconds(2);yield return new WaitForEndOfFrame();}
  void LateUpdate(){if(manual&&body){body.isKinematic=true;body.position=ballPosition;body.transform.position=ballPosition;}}
  bool Visible(Vector3 world,Rect rect){var p=cam.WorldToViewportPoint(world);return p.z>0&&rect.Contains(new Vector2(p.x,p.y));}
  readonly Rect ballSafe=new Rect(.115f,.245f,.72f,.54f);
  readonly Rect picture=new Rect(.03f,.03f,.94f,.94f);
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   float deadline=Time.realtimeSinceStartup+60;while((!(app=AppRoot.Instance)||!app.LocalAthlete||!app.stadium)&&Time.realtimeSinceStartup<deadline)yield return null;
   if(!app||!app.LocalAthlete){Check(false,"world initialized");Finish();yield break;}
   cam=Camera.main;actor=app.LocalAthlete;previousSimulation=Physics.simulationMode;
   while(!app.Menu||app.Menu.IsTransitioning)yield return null;
   yield return null;
   foreach(var sport in new[]{SportId.Football,SportId.Basketball}){
    manual=false;Physics.simulationMode=previousSimulation;
    app.SelectSport(sport);while(app.PendingSelection.HasValue)yield return null;app.EnterOffline();yield return Settle();
    app.view.mode=2;app.view.yaw=0;app.view.pitch=16;
    bool football=sport==SportId.Football;Transform field;Bounds bounds;
    if(football){field=FootballBall.Instance.Pitch;bounds=FootballBall.Instance.PitchBounds;body=FootballBall.Instance.Body;}
    else {field=BasketballBall.Active.transform.parent;var half=BasketballBall.PlayerCourtLimits;bounds=new Bounds(Vector3.zero,new Vector3(half.x*2,0,half.y*2));BasketballBall.Active.autoPickup=false;body=BasketballBall.Active.Body;}
    var halfSize=bounds.extents;var center=bounds.center;center.y=bounds.max.y+.08f;
    Physics.simulationMode=SimulationMode.Script;manual=true;
    var points=new[]{Vector2.zero,new Vector2(-1,0),new Vector2(1,0),new Vector2(0,-1),new Vector2(0,1),new Vector2(-1,-1),new Vector2(1,-1),new Vector2(-1,1),new Vector2(1,1)};
    for(int i=0;i<points.Length;i++){
     var p=center+new Vector3(points[i].x*(halfSize.x-1),0,points[i].y*(halfSize.z-1));
     ballPosition=field.TransformPoint(p+Vector3.up*.7f);Place(field,p+new Vector3(1,0,-1));yield return Settle();
     Check(!cam.orthographic&&cam.fieldOfView<50,sport+" broadcast perspective "+i);
     Check(Visible(ballPosition,ballSafe),sport+" ball inside useful viewport at field sample "+i);
     Check(Visible(actor.transform.position+Vector3.up*.9f,picture),sport+" nearby player remains visible "+i);
     if(i==0||i==7)Capture(Path.Combine(folder,sport+"-near-"+i+".png"));
    }
    ballPosition=field.TransformPoint(center+new Vector3(0,.5f,halfSize.z*.65f));Place(field,center-new Vector3(0,0,halfSize.z*.9f));yield return Settle();
    var marker=FindFirstObjectByType<StadiumPlayerIndicator>();
    Check(Visible(ballPosition,ballSafe),sport+" distant local athlete cannot pull focus off the ball");
    Check(marker&&marker.Showing,sport+" offscreen local player shows edge arrow");
    Check(marker&&!marker.raycastTarget&&marker.GetComponentsInChildren<UnityEngine.UI.Graphic>().All(g=>!g.raycastTarget),sport+" marker never intercepts touch");
    Check(marker&&!MarkerOverlapsButtons(marker),sport+" marker remains clear of actual styled button rectangles");
    if(marker)Check(Vector2.Dot(marker.Direction,StadiumPlayerIndicator.Bearing(cam,actor.transform.position+Vector3.up*.9f))>.99f,sport+" arrow points toward local character");
    Capture(Path.Combine(folder,sport+"-far-player.png"));
    yield return Settle();var pose=cam.transform.position;var rotation=cam.transform.rotation;
    Place(field,center-new Vector3(halfSize.x*.7f,0,halfSize.z*.8f));yield return Settle();
    Check(Vector3.Distance(pose,cam.transform.position)<.15f&&Quaternion.Angle(rotation,cam.transform.rotation)<.15f,sport+" moving a distant player does not move the camera, displacement="+Vector3.Distance(pose,cam.transform.position));
    Place(field,field.InverseTransformPoint(ballPosition)-Vector3.up*.5f+Vector3.right);yield return Settle();
    Check(marker&&!marker.Showing,sport+" arrow hides when character returns");
    pose=cam.transform.position;rotation=cam.transform.rotation;
    actor.transform.position+=Vector3.up*1.6f;app.view.yaw=132;app.view.pitch=-20;yield return Settle();
    Check(Vector3.Distance(pose,cam.transform.position)<.06f&&Quaternion.Angle(rotation,cam.transform.rotation)<.06f,sport+" jump and aim do not bob or rotate broadcast");
    foreach(int sign in new[]{1,-1})yield return Flight(field,center,halfSize,sport,sign);
    // Abrupt portal/reset at the opposite end must not become a giant velocity lead.
    ballPosition=field.TransformPoint(center+new Vector3(0,1,-halfSize.z*.8f));yield return new WaitForEndOfFrame();
    Check(Visible(ballPosition,ballSafe),sport+" teleport keeps the new ball visible immediately");yield return Settle();
    Check(Vector3.Distance(cam.transform.position,ballPosition)<(football?90:40),sport+" teleport settles without runaway zoom");
    Place(field,center);app.view.stick.value=Vector2.right;var command=app.view.ReadCommand();app.view.stick.value=Vector2.zero;
    var world=Quaternion.Euler(0,command.heading,0)*new Vector3(command.move.x,0,command.move.y);
    Check(Vector3.Dot(world.normalized,cam.transform.right)>.999f,sport+" right input follows presented camera while aim remains independent");
    app.view.stick.value=Vector2.up;command=app.view.ReadCommand();app.view.stick.value=Vector2.zero;
    world=Quaternion.Euler(0,command.heading,0)*new Vector3(command.move.x,0,command.move.y);
    Check(Vector3.Dot(world.normalized,Vector3.ProjectOnPlane(cam.transform.forward,Vector3.up).normalized)>.999f,sport+" up input moves toward far sideline");
    actor.capsule.enabled=true;var before=actor.transform.position;actor.Simulate(command,.2f);yield return null;
    Check(Vector3.Dot(actor.transform.position-before,world)>.01f,sport+" real motor accepts stadium movement");
    app.view.yaw=43;app.view.pitch=23;app.view.mode=1;yield return Settle();
    Check(!marker.Showing&&!cam.orthographic&&Mathf.Abs(cam.fieldOfView-60)<.01f,sport+" third person restores lens and hides marker");
    app.view.mode=0;yield return Settle();
    Check(Vector3.Distance(cam.transform.position,actor.transform.position+Vector3.up*1.57f)<.01f&&Mathf.Abs(cam.nearClipPlane-.06f)<.001f,sport+" original first person restored");
    app.view.mode=2;yield return Settle();
    // Test bearing independently for all four screen quadrants and behind camera.
    foreach(var d in new[]{new Vector3(2,1,4),new Vector3(-2,1,4),new Vector3(2,-1,-4),new Vector3(-2,-1,-4),new Vector3(0,0,-4)}){
     var bearing=StadiumPlayerIndicator.Bearing(cam,cam.transform.TransformPoint(d));
     Check(float.IsFinite(bearing.x)&&float.IsFinite(bearing.y)&&bearing.sqrMagnitude>.99f&&(d.x==0||Mathf.Sign(bearing.x)==Mathf.Sign(d.x)),sport+" robust edge bearing "+d);
    }
    if(!football){manual=false;Physics.simulationMode=previousSimulation;yield return PhysicalShot(field);}
   }
   manual=false;Physics.simulationMode=previousSimulation;actor.capsule.enabled=true;
   app.SelectSport(SportId.Golf);while(app.PendingSelection.HasValue)yield return null;app.EnterOffline();yield return Settle();
   Check(!app.view.StadiumFollowing&&!cam.orthographic,"Golf retains its own elevated camera");
   app.SelectSport(SportId.Football);while(app.PendingSelection.HasValue)yield return null;app.EnterOffline();yield return Settle();
   Check(Mathf.Abs(cam.fieldOfView-37)<.01f,"sport re-entry restores ball broadcast");
   app.view.enabled=false;yield return new WaitForEndOfFrame();
   Check(!app.view.StadiumFollowing&&Mathf.Abs(cam.fieldOfView-60)<.01f&&!FindFirstObjectByType<StadiumPlayerIndicator>().Showing,"disable clears camera and marker");Finish();
  }
  bool MarkerOverlapsButtons(StadiumPlayerIndicator marker){
   var corners=new Vector3[4];marker.rectTransform.GetWorldCorners(corners);var bounds=Rect.MinMaxRect(corners[0].x,corners[0].y,corners[2].x,corners[2].y);
   foreach(var button in FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None)){
    ((RectTransform)button.transform).GetWorldCorners(corners);
    if(bounds.Overlaps(Rect.MinMaxRect(corners[0].x,corners[0].y,corners[2].x,corners[2].y)))return true;
   }
   return false;
  }
  IEnumerator Flight(Transform field,Vector3 center,Vector3 half,SportId sport,int sign){
   bool football=sport==SportId.Football;var start=center+new Vector3(-2,1,-half.z*.72f*sign);var finish=center+new Vector3(2,1,half.z*.72f*sign);
   Place(field,center-new Vector3(0,0,half.z*.85f*sign));ballPosition=field.TransformPoint(start);yield return Settle();
   bool visible=true,groundVisible=true;float maxTravel=0,maxPan=0;var previous=cam.transform.position;var previousRotation=cam.transform.rotation;
   float capture=Time.captureDeltaTime;Time.captureDeltaTime=1f/30;
   for(int i=0;i<=150;i++){
    float t=i/150f;var point=Vector3.Lerp(start,finish,t);point.y+=(football?12:9)*Mathf.Sin(t*Mathf.PI);ballPosition=field.TransformPoint(point);
    yield return new WaitForEndOfFrame();visible&=Visible(ballPosition,ballSafe);
    groundVisible&=Visible(field.TransformPoint(new Vector3(point.x,center.y,point.z)),picture);
    if(i>3){maxTravel=Mathf.Max(maxTravel,Vector3.Distance(previous,cam.transform.position)*30);maxPan=Mathf.Max(maxPan,Quaternion.Angle(previousRotation,cam.transform.rotation)*30);}
    previous=cam.transform.position;previousRotation=cam.transform.rotation;
    if(i==25||i==75||i==140)Capture(Path.Combine(folder,sport+"-flight-"+sign+"-"+i+".png"));
   }
   Time.captureDeltaTime=capture;
   Check(visible,sport+" complete flight stays visible in direction "+sign);
   Check(groundVisible,sport+" flight retains court underneath "+sign);
   Check(maxTravel<(football?65:35)&&maxPan<35,sport+" continuous ball tracking travel="+maxTravel.ToString("F2")+" pan="+maxPan.ToString("F2"));
  }
  IEnumerator PhysicalShot(Transform field){
   var ball=BasketballBall.Active;Place(field,new Vector3(0,.08f,-8));actor.capsule.enabled=true;ball.autoPickup=true;ball.ResetHome();
   ball.Place(new Vector3(0,.25f,-7.4f),Quaternion.identity,Vector3.zero,Vector3.zero);
   float deadline=Time.time+3;while(ball.Holder!=actor&&Time.time<deadline)yield return null;
   Check(ball.Holder==actor,"real held-ball setup");yield return Settle();
   var pose=cam.transform.position;float bob=0;
   for(int i=0;i<60;i++){yield return new WaitForEndOfFrame();bob=Mathf.Max(bob,Vector3.Distance(cam.transform.position,pose));}
   Check(bob<.3f,"held dribble does not pump camera zoom or height: "+bob);
   ball.ReviewGiveProp(actor,SportsProp.LockOn);Check(ball.TryUseProp(actor,ball.PropSlot(actor).revision),"real long-range shot starts");
   uint shots=ball.ShotCount;deadline=Time.time+4;while(ball.ShotCount==shots&&Time.time<deadline)yield return null;
   bool visible=true;int frames=0;float apex=0;deadline=Time.time+8;
   while(ball.Score.result==BasketballResult.Flying&&Time.time<deadline){yield return new WaitForEndOfFrame();visible&=Visible(ball.transform.position,ballSafe);apex=Mathf.Max(apex,ball.transform.localPosition.y);frames++;if(frames==35)Capture(Path.Combine(folder,"Basketball-real-lock-on.png"));}
   Check(frames>10&&visible&&apex>4,"actual Rigidbody long shot tracked throughout arc, frames="+frames+" apex="+apex);
   Check(ball.Score.result==BasketballResult.Scored,"camera changes preserve physical assisted scoring");
  }
  void Capture(string path){
   var target=new RenderTexture(Screen.width,Screen.height,24);var previous=RenderTexture.active;var cameraTarget=cam.targetTexture;
   var overlays=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
   var cameras=overlays.Select(c=>c.worldCamera).ToArray();var distances=overlays.Select(c=>c.planeDistance).ToArray();
   try {
    cam.targetTexture=target;for(int i=0;i<overlays.Length;i++){overlays[i].renderMode=RenderMode.ScreenSpaceCamera;overlays[i].worldCamera=cam;overlays[i].planeDistance=1;}
    Canvas.ForceUpdateCanvases();cam.Render();RenderTexture.active=target;
    var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());Destroy(image);
   }finally{for(int i=0;i<overlays.Length;i++){overlays[i].renderMode=RenderMode.ScreenSpaceOverlay;overlays[i].worldCamera=cameras[i];overlays[i].planeDistance=distances[i];}cam.targetTexture=cameraTarget;RenderTexture.active=previous;target.Release();Destroy(target);Canvas.ForceUpdateCanvases();}
  }
  void Finish(){manual=false;Physics.simulationMode=previousSimulation;File.AppendAllText(Path.Combine(folder,"results.txt"),"STADIUM_CAMERA_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
