#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public sealed class StadiumCameraReview:MonoBehaviour {
  string folder;bool failed;AppRoot app;Camera cam;Athlete actor;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-stadiumCameraReview");
   if(i>=0&&i+1<args.Length)new GameObject("Stadium camera review").AddComponent<StadiumCameraReview>().folder=args[i+1];
  }
  void Check(bool ok,string label){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+label+"\n");}
  void Place(Transform field,Vector3 local){actor.capsule.enabled=false;actor.transform.position=field.TransformPoint(local);actor.ResetLocomotion();Physics.SyncTransforms();}
  IEnumerator Settle(){yield return new WaitForSeconds(2.5f);yield return new WaitForEndOfFrame();}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   float end=Time.realtimeSinceStartup+60;while((!(app=AppRoot.Instance)||!app.LocalAthlete||!app.stadium)&&Time.realtimeSinceStartup<end)yield return null;
   if(!app||!app.LocalAthlete){Check(false,"world initialized");Finish();yield break;}
   cam=Camera.main;actor=app.LocalAthlete;
   while(!app.Menu||app.Menu.IsTransitioning)yield return null;
   yield return null;
   foreach(var sport in new[]{SportId.Football,SportId.Basketball}){
    app.SelectSport(sport);while(app.PendingSelection.HasValue)yield return null;app.EnterOffline();yield return Settle();
    app.view.mode=2;app.view.yaw=0;app.view.pitch=16;
    Transform field;Bounds bounds;
    if(sport==SportId.Football){field=FootballBall.Instance.Pitch;bounds=FootballBall.Instance.PitchBounds;}
    else {field=BasketballBall.Active.transform.parent;var half=BasketballBall.PlayerCourtLimits;bounds=new Bounds(Vector3.zero,new Vector3(half.x*2,0,half.y*2));}
    var halfSize=bounds.extents;var center=bounds.center;center.y=bounds.max.y+.07f;
    var points=new[]{Vector2.zero,new Vector2(-1,0),new Vector2(1,0),new Vector2(0,-1),new Vector2(0,1),new Vector2(-1,-1),new Vector2(1,-1),new Vector2(-1,1),new Vector2(1,1)};
    float firstCoverage=0;
    for(int i=0;i<points.Length;i++){
     var p=center+new Vector3(points[i].x*(halfSize.x-.4f),0,points[i].y*(halfSize.z-.4f));Place(field,p);yield return Settle();
     Check(!cam.orthographic&&cam.fieldOfView<50,sport+" perspective broadcast lens "+i);
     var feet=cam.WorldToViewportPoint(actor.transform.position);var head=cam.WorldToViewportPoint(actor.transform.position+Vector3.up*1.75f);
     Check(feet.z>0&&feet.x>.015f&&feet.x<.985f&&feet.y>.015f&&head.y<.998f,sport+" player visible at field sample "+i+" feet="+feet.ToString("F3")+" head="+head.ToString("F3"));
     var a=cam.WorldToViewportPoint(field.TransformPoint(p+Vector3.forward*3));var b=cam.WorldToViewportPoint(field.TransformPoint(p-Vector3.forward*3));
     Check(b.x-a.x>.04f,sport+" consistent sideline direction through diagonal pan "+i);
     Footprint(field,bounds,out float coverage,out float lawn);if(i==0)firstCoverage=coverage;
     Check(lawn>(sport==SportId.Football?.50f:.40f),sport+" useful playing surface at sample "+i+" lawn="+lawn.ToString("F3"));
     Check(feet.x>=.14f&&feet.x<=.82f&&feet.y>.22f&&head.y<.84f,sport+" player clear of HUD at sample "+i);
     if(sport==SportId.Basketball)foreach(var hoop in field.GetComponentsInChildren<BasketballHoop>()){
      var rim=hoop.transform.TransformPoint(0,BasketballHoop.RimHeight,0);
      if(Vector3.ProjectOnPlane(rim-actor.transform.position,Vector3.up).magnitude<9){var v=cam.WorldToViewportPoint(rim);Check(v.x>.1f&&v.x<.82f&&v.y>.18f&&v.y<.82f,"nearby basket stays inside broadcast picture "+i);}
     }
     if(i>0)Check(Vector2.Distance(new Vector2(feet.x,feet.y),Vector2.one*.5f)>.1f,sport+" edge player is off centre "+i);
     File.AppendAllText(Path.Combine(folder,"framing.csv"),$"{sport},{i},{cam.aspect:F4},{coverage:F4},{lawn:F4},{cam.transform.position.y:F3},{feet.x:F3},{feet.y:F3}\n");
     Capture(Path.Combine(folder,sport+"-"+i+".png"));
    }
    Check(firstCoverage>(sport==SportId.Football?.32f:.35f)&&firstCoverage<(sport==SportId.Football?.50f:.75f),sport+" intended playing-area coverage "+firstCoverage.ToString("F3"));
    yield return Motion(field,center,halfSize,sport);
    Place(field,center);yield return Settle();var pose=cam.transform.position;var rotation=cam.transform.rotation;
    Place(field,center+Vector3.up*1.6f);app.view.yaw=132;app.view.pitch=-20;yield return Settle();
    Check(Vector3.Distance(pose,cam.transform.position)<.06f&&Quaternion.Angle(rotation,cam.transform.rotation)<.06f,sport+" jump and aim do not tilt or bob the field view");
    Place(field,center);yield return Settle();
    app.view.stick.value=Vector2.right;var command=app.view.ReadCommand();app.view.stick.value=Vector2.zero;
    var world=Quaternion.Euler(0,command.heading,0)*new Vector3(command.move.x,0,command.move.y);
    Check(Vector3.Dot(world.normalized,cam.transform.right)>.999f,sport+" right input moves screen right while aim remains independent");
    app.view.stick.value=Vector2.up;command=app.view.ReadCommand();app.view.stick.value=Vector2.zero;
    world=Quaternion.Euler(0,command.heading,0)*new Vector3(command.move.x,0,command.move.y);
    Check(Vector3.Dot(world.normalized,Vector3.ProjectOnPlane(cam.transform.forward,Vector3.up).normalized)>.999f,sport+" up input moves toward the far sideline");
    // Keep this movement check clear of the physical football at kickoff.
    Place(field,center+Vector3.forward*5);actor.capsule.enabled=true;var before=actor.transform.position;actor.Simulate(command,.2f);yield return null;
    Check(Vector3.Dot(actor.transform.position-before,world)>.01f,sport+" real motor accepts remapped stadium movement");
    Place(field,center);app.view.yaw=43;app.view.pitch=23;app.view.mode=1;yield return Settle();
    var expected=actor.transform.position+Vector3.up*1.35f-Quaternion.Euler(23,43,0)*Vector3.forward*5;
    Check(!cam.orthographic&&Mathf.Abs(cam.fieldOfView-60)<.01f&&Vector3.Distance(expected,cam.transform.position)<.01f,sport+" original third-person framing and lens restored");
    app.view.mode=0;yield return Settle();
    Check(!cam.orthographic&&Vector3.Distance(cam.transform.position,actor.transform.position+Vector3.up*1.57f)<.01f&&Mathf.Abs(cam.nearClipPlane-.06f)<.001f,sport+" original first-person framing restored");
    app.view.mode=2;Place(field,center+new Vector3(halfSize.x+25,0,0));yield return Settle();
    var outsideFeet=cam.WorldToViewportPoint(actor.transform.position);
    Check(outsideFeet.x>0&&outsideFeet.x<1&&outsideFeet.y>0&&outsideFeet.y<1,sport+" exploration beyond field remains visible");
   }
   app.view.mode=2;app.SelectSport(SportId.Golf);while(app.PendingSelection.HasValue)yield return null;app.EnterOffline();yield return Settle();
   Check(!cam.orthographic,"Golf elevated view retains its original perspective");
   app.SelectSport(SportId.Football);while(app.PendingSelection.HasValue)yield return null;app.EnterOffline();yield return Settle();Check(!cam.orthographic&&Mathf.Abs(cam.fieldOfView-37)<.01f,"return to football restores broadcast lens");
   app.view.active=false;app.view.enabled=false;Check(!cam.orthographic&&Mathf.Abs(cam.fieldOfView-60)<.01f,"disabling view restores projection and lens for menus and travel");
   Finish();
  }
  void Footprint(Transform frame,Bounds field,out float coverage,out float lawn){
   // Sample actual perspective rays and the field independently. A bounding
   // rectangle overcounts a rotated trapezoid and cannot measure screen area.
   var plane=new Plane(frame.up,frame.TransformPoint(new Vector3(0,field.max.y,0)));int onField=0,visible=0;
   const int n=64;
   for(int x=0;x<n;x++)for(int y=0;y<n;y++){
    float u=(x+.5f)/n,v=(y+.5f)/n;
    var ray=cam.ViewportPointToRay(new Vector2(u,v));
    if(plane.Raycast(ray,out float distance)){var p=frame.InverseTransformPoint(ray.GetPoint(distance));if(p.x>=field.min.x&&p.x<=field.max.x&&p.z>=field.min.z&&p.z<=field.max.z)onField++;}
    var screen=cam.WorldToViewportPoint(frame.TransformPoint(new Vector3(Mathf.Lerp(field.min.x,field.max.x,u),field.max.y,Mathf.Lerp(field.min.z,field.max.z,v))));
    if(screen.z>0&&screen.x>=0&&screen.x<=1&&screen.y>=0&&screen.y<=1)visible++;
   }
   coverage=(float)visible/(n*n);lawn=(float)onField/(n*n);
  }
  IEnumerator Motion(Transform field,Vector3 center,Vector3 half,SportId sport){
   bool football=sport==SportId.Football;
   var start=center+new Vector3(-half.x*.3f,0,-half.z*.65f);
   Place(field,start);yield return Settle();
   var firstRotation=cam.transform.rotation;var previous=cam.transform.position;var previousRotation=cam.transform.rotation;
   float minHeight=float.MaxValue,maxHeight=0,maxPanSpeed=0,maxTravelSpeed=0;bool visible=true;
   var frames=Path.Combine(folder,sport+"-motion");Directory.CreateDirectory(frames);
   const int count=240;float previousCapture=Time.captureDeltaTime;Time.captureDeltaTime=1f/30;
   for(int i=0;i<count;i++){
    float t=(float)i/(count-1);var local=Vector3.Lerp(start,center+new Vector3(half.x*.25f,0,half.z*.65f),t);
    Place(field,local);yield return new WaitForEndOfFrame();
    var feet=cam.WorldToViewportPoint(actor.transform.position);
    visible&=feet.z>0&&feet.x>.05f&&feet.x<.95f&&feet.y>.18f&&feet.y<.84f;
    float height=field.InverseTransformPoint(cam.transform.position).y;
    minHeight=Mathf.Min(minHeight,height);maxHeight=Mathf.Max(maxHeight,height);
    maxPanSpeed=Mathf.Max(maxPanSpeed,Mathf.Abs(Mathf.DeltaAngle(previousRotation.eulerAngles.y,cam.transform.eulerAngles.y))*30);
    maxTravelSpeed=Mathf.Max(maxTravelSpeed,Vector3.Distance(previous,cam.transform.position)*30);
    File.AppendAllText(Path.Combine(folder,sport+"-motion.csv"),$"{i},{local.x:F3},{local.z:F3},{height:F3},{cam.transform.eulerAngles.y:F3},{feet.x:F3},{feet.y:F3}\n");
    if(Screen.width==1280&&i%2==0)Capture(Path.Combine(frames,(i/2).ToString("D4")+".png"));
    previous=cam.transform.position;previousRotation=cam.transform.rotation;
   }
   Time.captureDeltaTime=previousCapture;
   Check(visible,sport+" moving player stays in useful viewport");
   Check(Quaternion.Angle(firstRotation,cam.transform.rotation)>(football?15:27),sport+" broadcast pans toward the opposite end");
   Check(maxPanSpeed<20&&maxTravelSpeed<(football?25:12),sport+" continuous tracking without camera cuts pan="+maxPanSpeed.ToString("F2")+" travel="+maxTravelSpeed.ToString("F2"));
   Check(maxHeight-minHeight>(football?.3f:.15f),sport+" lens distance eases with transition and end-court framing");
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
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"STADIUM_CAMERA_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
