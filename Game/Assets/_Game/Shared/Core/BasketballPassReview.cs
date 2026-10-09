#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.EventSystems;
namespace WhatTheFish {
 public sealed partial class BasketballPassReview:MonoBehaviour {
  string folder,shared;bool failed,capture,trajectory;int frame,rate=30;AppRoot app;Athlete actor;BasketballBall ball;Camera review;BasketballPassVisual lane;
  Transform[] armJoints;Vector3[] armPrevious;float armTime,armExcess;string armWorst;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-basketballPassReview");if(i<0||i+1>=args.Length)return;
   var r=new GameObject("Basketball pass review").AddComponent<BasketballPassReview>();r.folder=args[i+1];r.capture=!args.Contains("-passNoCapture");r.trajectory=args.Contains("-passTrajectory");i=Array.IndexOf(args,"-passRate");if(i>=0&&i+1<args.Length&&int.TryParse(args[i+1],out int fps))r.rate=Mathf.Clamp(fps,20,120);
  }
  void Check(bool ok,string message){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+message+"\n");}
  void Signal(string name){string path=Path.Combine(shared,name);File.WriteAllText(path+".tmp","ready");File.Move(path+".tmp",path);}
  bool Has(string name)=>File.Exists(Path.Combine(shared,name));
  IEnumerator Await(string name,float timeout=12){float end=Time.realtimeSinceStartup+timeout;while(!Has(name)&&Time.realtimeSinceStartup<end)yield return null;Check(Has(name),"peer checkpoint "+name);}
  void Place(Athlete who,Vector3 p,float yaw=0){who.capsule.enabled=false;who.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));who.capsule.enabled=ball.Authority;who.ResetLocomotion();var net=who.GetComponent<NetworkTransform>();if(net&&net.IsSpawned&&ball.Authority)net.Teleport(p,who.transform.rotation,Vector3.one);Physics.SyncTransforms();}
  IEnumerator Possess(Athlete who,Vector3 p,float yaw=0){
   armTime=0;
   app.view.ClearMatchInput();ball.ResetHome();Place(who,p,yaw);ball.autoPickup=true;
   ball.Place(ball.transform.parent.InverseTransformPoint(p+who.transform.forward*.6f+Vector3.up*.2f),Quaternion.identity,Vector3.zero,Vector3.zero);
   float end=Time.time+3;while(ball.Holder!=who&&Time.time<end)yield return null;Check(ball.Holder==who,"proximity pickup");yield return new WaitForSeconds(.3f);
   app.view.yaw=yaw;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=yaw};
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);shared=Directory.GetParent(folder).FullName;File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   float end=Time.realtimeSinceStartup+80;while((!(app=AppRoot.Instance)||!app.Exploring||!app.LocalAthlete||!BasketballBall.Active)&&Time.realtimeSinceStartup<end)yield return null;
   actor=app?app.LocalAthlete:null;ball=BasketballBall.Active;if(!actor||!ball){Check(false,"court initialized");Finish();yield break;}
   lane=ball.GetComponent<BasketballPassVisual>();DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.6f);
   review=new GameObject("Pass review camera").AddComponent<Camera>();review.CopyFrom(Camera.main);review.enabled=false;capture&=SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null;
   Check(RoomService.ProtocolVersion==38,"matching pass protocol 38");if(capture)Check(Resources.Load<Material>("BasketballPassAim").shader.isSupported,"pass shader supported by rendered player");
   if(app.rooms.Connected){if(ball.Authority)yield return Host();else yield return Guest();}else yield return Offline();Finish();
  }
  IEnumerator Observe(float seconds,string stage){
   float end=Time.time+seconds;while(Time.time<end){yield return new WaitForEndOfFrame();var aim=ball.PassAim;
    if(!app.rooms.Connected){
     if(armJoints==null){armJoints=actor.visual.GetComponentsInChildren<Transform>().Where(t=>t.name.Contains("Hand")||t.name.Contains("Arm")).ToArray();armPrevious=new Vector3[armJoints.Length];}
     float dt=Time.time-armTime;
     for(int i=0;i<armJoints.Length;i++){var p=actor.transform.InverseTransformPoint(armJoints[i].position);if(armTime>0&&dt<.1f){float excess=Vector3.Distance(p,armPrevious[i])-(6*dt+.012f);if(excess>armExcess){armExcess=excess;armWorst=stage+" "+armJoints[i].name+" "+actor.BasketballMotion.Elapsed.ToString("F3");}}armPrevious[i]=p;}armTime=Time.time;
    }
    File.AppendAllText(Path.Combine(folder,"samples.csv"),$"{frame},{stage},{aim.phase},{aim.bend:F3},{aim.heading:F2},{ball.PassPower:F3},{lane.Visible},{lane.Length:F3},{actor.BasketballMotion.Action},{actor.BasketballMotion.MaximumReachError:F4}\n");
    if(capture&&frame%2==0){Capture(stage,"lane",new Vector3(-5,7,-6),4);Capture(stage,"front",new Vector3(0,1.4f,4.4f),.9f);Capture(stage,"side",new Vector3(4.4f,1.4f,0),.9f);}frame++;
   }
  }
  IEnumerator Release(float expectedPower,float heading){
   uint before=ball.PassCount;app.view.EndPass();float end=Time.time+2;
   while(ball.PassCount==before&&Time.time<end)yield return Observe(.001f,"push");
   Check(ball.PassCount==before+1,"one release produces one pass");Check(Mathf.Abs(ball.PassAim.power-expectedPower)<.12f,"host bounds charge to held duration");
   var flat=Vector3.ProjectOnPlane(ball.LastLaunchVelocity,Vector3.up).normalized;var forward=Quaternion.Euler(0,heading,0)*Vector3.forward;
   Check(Vector3.Dot(flat,forward)>.999f,"launch follows displayed heading without hidden receiver redirect");
   yield return Observe(.5f,"release");Check(!lane.Visible,"release pulse fades");
  }
  IEnumerator Offline(){
   Time.captureFramerate=rate;
   yield return Possess(actor,new Vector3(0,.07f,-5));uint count=ball.PassCount;uint play=ball.Defense.play;
   Check(!ball.BeginPassCharge(actor,float.NaN,play)&&!ball.BeginPassCharge(actor,0,play+1),"invalid aim and stale possession rejected");
   Check(app.view.BeginPass(),"hold begins pass");yield return Observe(.42f,"half-charge");
   Check(ball.IsPassAiming(actor)&&actor.BasketballMotion.PassCharging&&lane.Visible,"actual gather and shared lane become visible");
   Check(!app.view.BeginShot()&&!ball.BeginShotCharge(actor,0),"pass and shot cannot charge together");
   Check(!ball.ReleasePassCharge(actor,0,play+1,ball.PassClock)&&!ball.ReleasePassCharge(actor,float.NaN,play,ball.PassClock),"stale and nonfinite releases rejected");
   app.view.CancelPass();yield return Observe(.45f,"cancel");Check(ball.Holder==actor&&ball.PassCount==count&&!lane.Visible,"cancel keeps possession and hides lane");
   var control=FindObjectsByType<BasketballShootButton>(FindObjectsSortMode.None).First(b=>b.pass);
   var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=12,position=RectTransformUtility.WorldToScreenPoint(null,control.transform.position)};
   control.OnPointerDown(pointer);yield return new WaitForSeconds(.2f);control.OnPointerUp(new PointerEventData(EventSystem.current){pointerId=13});Check(app.view.PassCharging,"second finger cannot release pass");
   pointer.position=RectTransformUtility.WorldToScreenPoint(null,control.cancelArea.position);control.OnDrag(pointer);control.OnPointerUp(pointer);yield return new WaitForSeconds(.4f);Check(!app.view.PassCharging&&ball.PassCount==count&&ball.Held,"drag cancel does not throw");
   app.view.RequestPass();float end=Time.time+2;while(ball.PassCount==count&&Time.time<end)yield return null;Check(ball.PassCount==count+1&&ball.PassAim.power<.05f,"quick tap releases a four metre chest pass");yield return Observe(.6f,"quick-pass");
   yield return Possess(actor,new Vector3(0,.07f,-5));Check(app.view.BeginPass(),"full pass begins");yield return Observe(.95f,"full-charge");
   Check(ball.PassPower>.99f&&ball.Held,"full charge caps without automatic throw");
   app.view.yaw=45;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=45};yield return Observe(.5f,"aim-turn");Check(Mathf.Abs(Mathf.DeltaAngle(ball.PassAim.heading,45))<.1f,"held aim follows camera input");
   yield return Release(1,45);
   yield return Possess(actor,new Vector3(0,.07f,-5));app.view.BeginPass();yield return new WaitForSeconds(.3f);app.view.ClearMatchInput();yield return new WaitForSeconds(.4f);Check(!ball.IsPassAiming(actor)&&!lane.Visible&&ball.Held,"input cancellation clears host aim without throwing");
   count=ball.PassCount;Check(app.view.BeginPass(),"begin pass for release/cancel edge");yield return new WaitForSeconds(.3f);app.view.EndPass();Check(!app.view.BeginShot(),"queued pass cannot race a shot in the same input frame");app.view.ClearMatchInput();yield return new WaitForSeconds(.4f);Check(ball.Held&&ball.PassCount==count&&!ball.IsPassAiming(actor),"cancel after queued release clears host aim and keeps ball");
   Check(app.view.BeginShot(),"shot remains available after cancellation");app.view.EndShot();Check(!app.view.BeginPass(),"queued shot cannot race a pass in the same input frame");app.view.ClearMatchInput();yield return new WaitForSeconds(.1f);ball.CancelShotCharge(actor);yield return new WaitForSeconds(.4f);
   Check(app.view.BeginPass(),"pass available for possession-loss check");yield return new WaitForSeconds(.3f);Check(ball.IsPassAiming(actor),"host aiming before possession loss");ball.ResetHome();yield return new WaitForSeconds(.3f);Check(!app.view.PassCharging&&!lane.Visible&&ball.PassAim.phase==0,"possession loss clears charge and cue");
   if(trajectory){Odds();yield return Arcs();yield return ShotTrials();}
  }
  IEnumerator Host(){
   yield return Await("guest-ready");var guest=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).First(n=>!n.IsOwner).GetComponent<Athlete>();
   Place(guest,new Vector3(2,.07f,1));yield return Possess(actor,new Vector3(0,.07f,-5));Check(app.view.BeginPass(),"host pass starts");app.view.SelectPass(1);yield return Observe(.7f,"host-charge");
   app.view.yaw=25;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=25};yield return Observe(.5f,"host-turn");yield return Await("saw-host-lane");yield return Release(1,25);
   yield return Await("saw-host-release");Place(actor,new Vector3(-5,.07f,0));yield return Possess(guest,new Vector3(0,.07f,-5));Signal("guest-turn");
   float end=Time.time+10;while(!ball.IsPassAiming(guest)&&Time.time<end)yield return null;Check(ball.IsPassAiming(guest),"reliable guest begin accepted by host");yield return Observe(.55f,"guest-charge");Check(lane.Visible&&Mathf.Abs(Mathf.DeltaAngle(ball.PassAim.heading,70))<.1f,"host sees guest steering direction");Check(ball.PassAim.bend<-.95f&&lane.Path.Bounces,"host sees guest bounce trajectory");
   var receive=BasketballPassPath.Create(lane.Path.start,70,1,-1,BasketballBall.PassFloor(lane.Path.start));var receiver=receive.Point(receive.duration);receiver.x=Mathf.Clamp(receiver.x,-6.9f,6.9f);receiver.y=.07f;Place(actor,receiver,250);Signal("host-saw-guest");
   yield return Await("guest-released");yield return Observe(.8f,"guest-release");Check(ball.PassCount==2,"guest release processed once by host");Check(ball.Holder==actor&&ball.PassBounces==1,"receiver catches guest bounce after real floor rebound");Check(Vector3.Dot(Vector3.ProjectOnPlane(ball.LastLaunchVelocity,Vector3.up).normalized,Quaternion.Euler(0,70,0)*Vector3.forward)>.999f,"guest pass follows its shared lane");Place(actor,new Vector3(-5,.07f,0));
   if(trajectory)yield return HostOdds(guest);Signal("host-done");yield return Await("guest-done");
  }
  IEnumerator Guest(){
   Check(!ball.Authority&&!ball.BeginPassCharge(actor,0,ball.Defense.play),"guest cannot author ball simulation");Signal("guest-ready");
   float end=Time.time+12;while((ball.PassAim.phase!=1||Mathf.Abs(Mathf.DeltaAngle(ball.PassAim.heading,25))>.1f||!lane.Visible||ball.PassPower<=.75f)&&Time.time<end)yield return null;
   Check(lane.Visible&&ball.PassPower>.75f,"observer sees host direction and charge");Check(ball.PassAim.bend>.95f&&lane.Path.Point(lane.Path.duration*.5f).y-lane.Path.start.y>1,"observer sees host loft arc");Signal("saw-host-lane");yield return Observe(.2f,"remote-host-lane");
   end=Time.time+8;while(ball.PassCount<1&&Time.time<end)yield return null;Check(ball.PassCount==1,"guest receives host pass result");Signal("saw-host-release");yield return Await("guest-turn");
   end=Time.time+4;while(ball.Holder!=actor&&Time.time<end)yield return null;yield return new WaitForSeconds(.2f);
   app.view.yaw=70;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=70};Check(app.view.BeginPass(),"guest starts through normal input");app.view.SelectPass(-1);yield return Await("host-saw-guest");yield return Observe(.35f,"guest-local-lane");Check(lane.Visible&&ball.PassPower>.95f,"guest receives authoritative full charge");app.view.EndPass();
   end=Time.time+4;while(ball.PassCount<2&&Time.time<end)yield return null;Check(ball.PassCount==2,"guest release yields exactly one pass");Signal("guest-released");if(trajectory)yield return GuestOdds();yield return Await("host-done");Check(ball.Body.isKinematic&&!ball.Body.detectCollisions,"guest remains a noncolliding replica");Signal("guest-done");
  }
  void Capture(string stage,string view,Vector3 offset,float focus){
   string directory=Path.Combine(folder,view);Directory.CreateDirectory(directory);var subject=ball.Holder?ball.Holder:actor;var rotation=Quaternion.Euler(0,ball.PassAim.heading,0);
   review.transform.position=subject.transform.position+rotation*offset;review.transform.LookAt(subject.transform.position+(view=="lane"?rotation*Vector3.forward*focus:Vector3.up*focus));review.fieldOfView=view=="lane"?60:35;
   var rt=RenderTexture.GetTemporary(960,720,24);var old=RenderTexture.active;review.targetTexture=rt;review.Render();RenderTexture.active=rt;var image=new Texture2D(960,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,960,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(directory,frame.ToString("D5")+"-"+stage+".png"),image.EncodeToPNG());Destroy(image);review.targetTexture=null;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);
  }
  void Finish(){if(armJoints!=null)Check(armExcess<=0,"actual held-pass arm continuity; excess="+armExcess.ToString("F4")+" "+armWorst);Time.captureFramerate=0;DevelopmentProbe.TurnCommand=default;app?.view.ClearMatchInput();if(review)Destroy(review.gameObject);File.AppendAllText(Path.Combine(folder,"results.txt"),"BASKETBALL_PASS_COMPLETE success="+!failed+"\n");}
 }
}
#endif
