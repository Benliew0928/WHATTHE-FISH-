#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 public sealed class BasketballFinishReview:MonoBehaviour {
  string folder,stage;bool failed,noCapture;int rate=30,frame;AppRoot app;Athlete actor;BasketballBall ball;Transform hoop;Camera camera;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-basketballFinishReview");if(i<0||i+1>=args.Length)return;
   var review=new GameObject("Basketball finish review").AddComponent<BasketballFinishReview>();review.folder=args[i+1];review.noCapture=args.Contains("-finishNoCapture");
   i=Array.IndexOf(args,"-finishRate");if(i>=0&&i+1<args.Length)int.TryParse(args[i+1],out review.rate);
  }
  void Check(bool value,string label){failed|=!value;File.AppendAllText(Path.Combine(folder,"results.txt"),(value?"PASS ":"FAIL ")+label+"\n");}
  void SeedLayup(bool make){for(int seed=0;seed<10000;seed++){UnityEngine.Random.InitState(seed);if((UnityEngine.Random.value<BasketballFinishRules.LayupMakeChance)==make){UnityEngine.Random.InitState(seed);return;}}throw new InvalidOperationException("No layup seed");}
  void Place(Athlete player,Vector3 p,float yaw){player.capsule.enabled=false;player.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));player.capsule.enabled=ball.Authority;player.ResetLocomotion();var net=player.GetComponent<Unity.Netcode.Components.NetworkTransform>();if(net&&net.IsSpawned&&ball.Authority)net.Teleport(p,Quaternion.Euler(0,yaw,0),Vector3.one);Physics.SyncTransforms();}
  IEnumerator Pickup(float distance,float side=0,Athlete player=null){
   if(!player)player=actor;app.view.ClearMatchInput();DevelopmentProbe.TurnCommand=default;ball.ResetHome();
   var p=hoop.TransformPoint(new Vector3(side,.07f,distance));float yaw=Quaternion.LookRotation(Vector3.ProjectOnPlane(hoop.position-p,Vector3.up)).eulerAngles.y;Place(player,p,yaw);app.view.yaw=yaw;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=yaw};
   ball.autoPickup=true;ball.Place(ball.transform.parent.InverseTransformPoint(p+player.transform.forward*.6f+Vector3.up*.2f),Quaternion.identity,Vector3.zero,Vector3.zero);
   float end=Time.time+2;while(ball.Holder!=player&&Time.time<end)yield return null;yield return new WaitForSeconds(.12f);Check(ball.Holder==player,"pickup "+distance+" / "+side);
  }
  IEnumerator DriveCharge(float distance,float side=0){
   yield return Pickup(distance,side);float yaw=actor.transform.eulerAngles.y;app.view.BeginShot();
   yield return new WaitForSeconds(.30f);DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,heading=yaw};
   float end=Time.time+1.5f;while(Time.time<end&&Mathf.Abs(app.view.ShotPower-BasketballBall.SweetSpot)>.025f)yield return null;
  }
  IEnumerator Observe(string label,bool expectMake,bool capture=true){
   stage=label;uint shots=ball.ShotCount,attempts=ball.Score.attempts;float end=Time.time+2.2f;float reach=0,contact=0,maxStep=0,maxY=actor.transform.position.y;var previous=actor.transform.position;bool airborne=false,active=false;int overlaps=0;Vector3 release=Vector3.zero;bool released=false;
   while(Time.time<end){
    yield return new WaitForEndOfFrame();var motion=actor.BasketballMotion;active|=motion.Finishing;airborne|=actor.Airborne;maxY=Mathf.Max(maxY,actor.transform.position.y);
    maxStep=Mathf.Max(maxStep,Vector3.Distance(previous,actor.transform.position));previous=actor.transform.position;
    if(motion.Finishing&&ball.Held&&motion.Elapsed>.14f&&Physics.CheckSphere(ball.CarryPosition(actor),BasketballBall.Radius-.005f,1<<8,QueryTriggerInteraction.Ignore))overlaps++;
    if(motion.Finishing){reach=Mathf.Max(reach,motion.MaximumReachError);contact=Mathf.Max(contact,motion.ContactError);}
    if(ball.ShotCount>shots&&!released){released=true;release=ball.Body.position;}
    if(capture)Capture();
   }
   Check(active&&airborne&&maxY>1,"actual capsule leaves ground: "+label+" maxY="+maxY.ToString("F3"));
   Check(ball.ShotCount==shots+1&&ball.Score.attempts==attempts+1,"one physical release / attempt: "+label+" notice="+ball.FinishNotice.reason);
   Check(!actor.Airborne&&actor.Grounded&&!actor.BasketballMotion.Busy,"grounded recovery completes: "+label);
   Check(overlaps==0,"held ball clears architecture: "+label+" overlaps="+overlaps);
   Check(reach<.075f&&contact<.045f,"reachable hand contact: "+label+" reach="+reach.ToString("F3")+" palm="+contact.ToString("F3"));
   Check(maxStep<14f/rate+.04f,"bounded capsule movement: "+label+" step="+maxStep.ToString("F3"));
   end=Time.time+3;while(ball.Score.result==BasketballResult.Flying&&Time.time<end)yield return null;
   Check(expectMake?ball.Score.result==BasketballResult.Scored:ball.Score.result==BasketballResult.Missed,(expectMake?"physical basket: ":"rolled miss rebounds: ")+label+" release="+release.ToString("F3"));
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   float end=Time.realtimeSinceStartup+80;while((!(app=AppRoot.Instance)||!app.Exploring||!app.LocalAthlete||!BasketballBall.Active)&&Time.realtimeSinceStartup<end)yield return null;
   actor=app?app.LocalAthlete:null;ball=BasketballBall.Active;if(!actor||!ball){Check(false,"initialized");Finish();yield break;}
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.5f);
   hoop=app.stadium.GetComponentsInChildren<Transform>().Single(t=>t.name=="Hoop_North");
   camera=new GameObject("Finish review camera").AddComponent<Camera>();camera.CopyFrom(Camera.main);camera.enabled=false;app.view.mode=1;actor.HideHead(false);
   if(Environment.GetCommandLineArgs().Contains("-finishGestureOnly")){Time.captureFramerate=rate;yield return InputGestures();Finish();yield break;}
   if(NetworkManager.Singleton&&NetworkManager.Singleton.IsListening){if(ball.Authority)yield return Host();else yield return Guest();Finish();yield break;}
   Time.captureFramerate=rate;
   foreach(var name in new[]{"Hoop_North","Hoop_South"}){
    hoop=app.stadium.GetComponentsInChildren<Transform>().Single(t=>t.name==name);
    foreach(var kind in new[]{BasketballFinish.Layup,BasketballFinish.Dunk})foreach(float side in new[]{-.75f,.75f}){
     yield return DriveCharge(3,side);var reason=ball.FinishAvailability(actor,kind,hoop,true);Check(reason==BasketballFinishReason.Ready,"eligible "+name+" "+kind+" "+side+": "+reason);
     SeedLayup(true);app.view.SelectFinish(kind);app.view.EndShot();yield return Observe(name+"-"+kind+"-"+(side<0?"left":"right"),true);
    }
   }
   var lod=actor.visual.GetComponent<LODGroup>();lod.ForceLOD(1);
   yield return DriveCharge(3,.5f);app.view.SelectFinish(BasketballFinish.Dunk);app.view.EndShot();yield return Observe("lod1-dunk",true);lod.ForceLOD(-1);
   yield return Pickup(1.9f);SeedLayup(true);app.view.BeginShot();app.view.SelectFinish(BasketballFinish.Layup);app.view.EndShot();yield return Observe("standing-layup-no-charge",true);
   yield return Rejections();yield return Gesture();
   yield return OddsAndGather();
   // An off-green dunk succeeds: eligible finishes never use normal-shot timing.
   yield return Pickup(2.5f);ball.BeginShotCharge(actor,actor.transform.eulerAngles.y);yield return new WaitForSeconds(.75f);DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,heading=actor.transform.eulerAngles.y};yield return new WaitForSeconds(.15f);
   var randomBefore=UnityEngine.Random.state;Check(ball.ReleaseShotCharge(actor,actor.transform.eulerAngles.y,double.NaN,BasketballFinish.Dunk),"off-green dunk accepts valid approach");Check(UnityEngine.Random.state.Equals(randomBefore),"dunk does not roll random odds");yield return Observe("untimed-dunk",true);
   Finish();
  }
  IEnumerator Rejections(){
   foreach(var entry in new[]{(6f,0f,BasketballFinish.Layup,BasketballFinishReason.Closer),(.4f,0f,BasketballFinish.Layup,BasketballFinishReason.TooClose),(1.8f,0f,BasketballFinish.Dunk,BasketballFinishReason.Drive),(-1.5f,0f,BasketballFinish.Layup,BasketballFinishReason.Front)}){
    yield return Pickup(entry.Item1,entry.Item2);uint shots=ball.ShotCount;ball.BeginShotCharge(actor,actor.transform.eulerAngles.y);yield return new WaitForSeconds(.4f);
    Check(ball.ReleaseShotCharge(actor,actor.transform.eulerAngles.y,double.NaN,entry.Item3)&&ball.FinishNotice.reason==entry.Item4,"normal-shot fallback: "+entry.Item4);
    yield return new WaitForSeconds(1);Check(ball.ShotCount==shots+1&&!actor.BasketballMotion.Finishing,"fallback releases once without an ineligible finish: "+entry.Item4);
   }
   yield return Pickup(2.5f);DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,heading=actor.transform.eulerAngles.y};yield return new WaitForSeconds(.15f);
   uint queuedShots=ball.ShotCount;ball.BeginShotCharge(actor,actor.transform.eulerAngles.y);ball.ReleaseShotCharge(actor,actor.transform.eulerAngles.y,double.NaN,BasketballFinish.Dunk);yield return new WaitForSeconds(2.4f);
   Check(!ball.ActionQueued&&!actor.BasketballMotion.Busy&&!actor.Airborne&&(ball.ShotCount==queuedShots+1||ball.Holder==actor&&ball.FinishNotice.reason!=BasketballFinishReason.Ready),"quick dribble request revalidates or recovers without a stranded action");
   yield return Pickup(1.4f);ball.BeginShotCharge(actor);actor.RequestJump();yield return new WaitForSeconds(.20f);
   Check(ball.ReleaseShotCharge(actor,actor.transform.eulerAngles.y,double.NaN,BasketballFinish.Layup)&&ball.FinishNotice.reason==BasketballFinishReason.Grounded&&!actor.BasketballMotion.Finishing,"airborne finish request falls back without adding a second jump");yield return new WaitForSeconds(1);
   yield return Pickup(1.4f);ball.BeginShotCharge(actor);yield return new WaitForSeconds(.4f);
   var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=8;wall.transform.position=actor.transform.position+actor.transform.forward*.5f+Vector3.up*1.5f;wall.transform.localScale=new Vector3(1,2,.12f);Physics.SyncTransforms();
   Check(ball.ReleaseShotCharge(actor,actor.transform.eulerAngles.y,double.NaN,BasketballFinish.Layup)&&ball.FinishNotice.reason==BasketballFinishReason.Lane&&!actor.BasketballMotion.Finishing,"capsule sweep rejects blocked finish and selects ordinary shot");Destroy(wall);yield return null;
   yield return DriveCharge(3);app.view.SelectFinish(BasketballFinish.Dunk);app.view.EndShot();yield return new WaitForSeconds(.34f);
   uint interruptedAttempts=ball.Score.attempts;var plan=actor.BasketballMotion.State;
   wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=8;wall.transform.position=BasketballMotion.FinishRoot(plan,.52f)+Vector3.up*1.5f;wall.transform.localScale=new Vector3(1,3,.12f);Physics.SyncTransforms();
   yield return new WaitForSeconds(.9f);Check(!ball.ActionQueued&&ball.Holder==actor&&ball.Score.attempts==interruptedAttempts,"new obstacle interrupts flight without a phantom attempt");Destroy(wall);yield return new WaitForSeconds(.6f);Check(!actor.Airborne&&actor.Grounded,"interrupted jump lands using gravity");
   yield return DriveCharge(3);app.view.SelectFinish(BasketballFinish.Dunk);app.view.EndShot();yield return new WaitForSeconds(.12f);
   uint attempts=ball.Score.attempts;Check(!actor.CanRequestJump&&!ball.TryPass(actor,0)&&!ball.BeginShotCharge(actor,0),"jump pass and shot cannot cancel committed finish");
   actor.ResetLocomotion();yield return new WaitForSeconds(1);Check(!ball.ActionQueued&&ball.Score.attempts==attempts,"reset cancels finish before release exactly once");
  }
  IEnumerator Gesture(){
   yield return Pickup(1.4f);var button=FindObjectsByType<BasketballShootButton>(FindObjectsSortMode.None).First(b=>!b.pass);yield return null;
   Vector2 origin=RectTransformUtility.WorldToScreenPoint(null,button.transform.position);float scale=button.GetComponentInParent<Canvas>().scaleFactor;
   var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=51,position=origin};button.OnPointerDown(pointer);yield return new WaitForSeconds(.4f);
   pointer.position=origin+Vector2.up*65*scale;button.OnDrag(pointer);Check(app.view.ShotFinish==BasketballFinish.Dunk,"up selects dunk on shoot button");
   button.OnDrag(new PointerEventData(EventSystem.current){pointerId=52,position=origin-Vector2.up*70*scale});Check(app.view.ShotFinish==BasketballFinish.Dunk,"foreign finger cannot change finish");
   pointer.position=origin+Vector2.up*35*scale;button.OnDrag(pointer);Check(app.view.ShotFinish==BasketballFinish.Dunk,"hysteresis retains choice near threshold");
   pointer.position=origin;button.OnDrag(pointer);Check(app.view.ShotFinish==BasketballFinish.Shot,"return to centre restores regular shot");
   pointer.position=origin-Vector2.up*65*scale;button.OnDrag(pointer);Check(app.view.ShotFinish==BasketballFinish.Layup,"down selects layup");
   if(!noCapture){yield return null;yield return new WaitForEndOfFrame();CaptureControls();yield return null;}
   uint shots=ball.ShotCount;pointer.position=RectTransformUtility.WorldToScreenPoint(null,button.cancelArea.position);button.OnDrag(pointer);button.OnPointerUp(pointer);yield return new WaitForSeconds(.35f);
   Check(!app.view.ShotCharging&&ball.Held&&ball.ShotCount==shots,"cancel after selection cannot fire on lift");
  }
  IEnumerator OddsAndGather(){
   foreach(var name in new[]{"Hoop_North","Hoop_South"})foreach(float side in new[]{-.75f,.75f}){
    hoop=app.stadium.GetComponentsInChildren<Transform>().Single(t=>t.name==name);
    yield return DriveCharge(4.25f,side);SeedLayup(false);
    var before=UnityEngine.Random.state;_ = UnityEngine.Random.value;var afterOne=UnityEngine.Random.state;UnityEngine.Random.state=before;
    Check(ball.ReleaseShotCharge(actor,actor.transform.eulerAngles.y,double.NaN,BasketballFinish.Layup),"wide-range layup commits: "+name+" / "+side);
    Check(UnityEngine.Random.state.Equals(afterOne),"one host roll at layup commit");
    Check(!ball.ReleaseShotCharge(actor,actor.transform.eulerAngles.y,double.NaN,BasketballFinish.Layup)&&UnityEngine.Random.state.Equals(afterOne),"duplicate request cannot reroll layup");
    yield return Observe(name+"-layup-15-percent-miss-"+side,false);
   }
   yield return DriveCharge(4.25f);DevelopmentProbe.TurnCommand=new PlayerCommand{heading=actor.transform.eulerAngles.y};yield return new WaitForSeconds(.075f);
   Check(actor.Motor.Velocity.magnitude<.1f&&ball.FinishAvailability(actor,BasketballFinish.Dunk,hoop,true)==BasketballFinishReason.Ready,"brief planted gather retains actual run-up");
   app.view.SelectFinish(BasketballFinish.Dunk);app.view.EndShot();yield return Observe("wide-dunk-after-plant",true);
   yield return DriveCharge(3.5f);DevelopmentProbe.TurnCommand=new PlayerCommand{heading=actor.transform.eulerAngles.y};yield return new WaitForSeconds(.35f);
   Check(ball.FinishAvailability(actor,BasketballFinish.Dunk,hoop)==BasketballFinishReason.Drive,"expired run-up cannot grant a standing dunk");app.view.CancelShot();
  }
  // Feed the actual UI input module, including raycasts, pointer capture,
  // exit/drag/up dispatch and its touch path. Direct OnDrag calls miss these.
  IEnumerator InputGestures(){
   var system=EventSystem.current;var module=system.GetComponent<StandaloneInputModule>();
   var input=system.gameObject.AddComponent<BasketballGestureReviewInput>();var previous=module.inputOverride;module.inputOverride=input;
   var button=FindObjectsByType<BasketballShootButton>(FindObjectsSortMode.None).First(b=>!b.pass);
   foreach(bool touch in new[]{false,true})foreach(var kind in new[]{BasketballFinish.Dunk,BasketballFinish.Layup}){
    yield return Pickup(4.1f);DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,heading=actor.transform.eulerAngles.y};yield return new WaitForSeconds(.08f);Canvas.ForceUpdateCanvases();
    Vector2 origin=RectTransformUtility.WorldToScreenPoint(null,button.transform.position);float scale=button.GetComponentInParent<Canvas>().scaleFactor;
    string name=(touch?"touch":"mouse")+"-"+kind;float yaw=actor.transform.eulerAngles.y;
    input.Set(origin,true,false,touch);Pump();Check(app.view.ShotCharging,name+" input-module press begins charge");
    yield return new WaitForSeconds(.04f);
    var position=origin+Vector2.up*(kind==BasketballFinish.Dunk?110:-110)*scale;
    input.Set(position,false,false,touch);Pump();yield return null;
    Check(app.view.ShotCharging&&app.view.ShotFinish==kind,name+" drag outside button keeps selected charge");
    Check(ball.FinishAvailability(actor,kind,hoop,true)==BasketballFinishReason.Ready,name+" eligible at input release");
   Check(!app.view.ShotNeedsTiming&&app.view.ShotPower<.4f,name+" ready finish needs no charge or timing");
    Check(!FindFirstObjectByType<BasketballHUD>().transform.Find("Shot power").gameObject.activeSelf,name+" ready finish hides power meter");
    SeedLayup(true);uint shots=ball.ShotCount;input.Set(position,false,true,touch);Pump();
    yield return Observe("input-"+name,true,!noCapture);
    Check(ball.ShotCount==shots+1,name+" full gesture produces one release");
   }
   foreach(bool touch in new[]{false,true})foreach(var kind in new[]{BasketballFinish.Dunk,BasketballFinish.Layup}){
    yield return Pickup(6);yield return null;Canvas.ForceUpdateCanvases();
    Vector2 origin=RectTransformUtility.WorldToScreenPoint(null,button.transform.position);float scale=button.GetComponentInParent<Canvas>().scaleFactor;
    string name=(touch?"touch":"mouse")+"-far-"+kind;
    input.Set(origin,true,false,touch);Pump();yield return new WaitForSeconds(.25f);
    Vector2 point=origin+Vector2.up*(kind==BasketballFinish.Dunk?100:-100)*scale;input.Set(point,false,false,touch);Pump();yield return null;
    Check(app.view.ShotFinish==kind&&app.view.ShotNeedsTiming&&app.view.ShotWindow==BasketballBall.SweetWindow,name+" retains normal charge timing");
    Check(button.label.text.Contains("normal shot"),name+" button explains fallback");
    if(!noCapture){yield return new WaitForEndOfFrame();CaptureControls(name);}
    uint shots=ball.ShotCount;float power=app.view.ShotPower;input.Set(point,false,true,touch);Pump();yield return new WaitForSeconds(1);
    Check(ball.ShotCount==shots+1&&!actor.BasketballMotion.Finishing,name+" gesture releases one ordinary shot");
    Check(Mathf.Abs(ball.LastReleasePower-power)<.02f,name+" preserves displayed ordinary-shot power");
   }
   module.inputOverride=previous;Destroy(input);
   void Pump(){system.SendMessage("OnApplicationFocus",true,SendMessageOptions.DontRequireReceiver);module.UpdateModule();module.Process();input.Settle();}
  }
  string SignalPath(string name)=>Path.Combine(Directory.GetParent(folder).FullName,name+".signal");
  void Signal(string name)=>File.WriteAllText(SignalPath(name),"ready");
  bool Signalled(string name)=>File.Exists(SignalPath(name));
  IEnumerator Await(string name){float end=Time.time+25;while(!Signalled(name)&&Time.time<end)yield return null;Check(Signalled(name),"network signal "+name);}
  IEnumerator Host(){
   yield return Await("guest-ready");var guest=Athlete.Active.Single(a=>a!=actor&&a.GetComponent<NetworkAthlete>());Place(guest,hoop.TransformPoint(new Vector3(4,.07f,5)),0);
   yield return DriveCharge(3);app.view.SelectFinish(BasketballFinish.Dunk);app.view.EndShot();yield return Observe("host-dunk",true,false);
   yield return Pickup(3,0,guest);SeedLayup(true);Signal("guest-go");yield return Await("guest-done");Check(ball.Score.made>=2,"host counts guest layup from physical flight");
   yield return Pickup(1.9f,0,guest);SeedLayup(false);Signal("guest-miss-go");yield return Await("guest-miss-done");Check(ball.Score.made==2&&ball.Score.attempts==3,"host preserves missed guest layup as a physical missed attempt");
  }
  IEnumerator Guest(){
   Check(!ball.Authority&&!ball.BeginShotCharge(actor),"guest cannot directly simulate host actions");Signal("guest-ready");bool saw=false;float end=Time.time+25;
   while(!Signalled("guest-go")&&Time.time<end){saw|=Athlete.Active.Any(a=>a!=actor&&a.BasketballMotion.Action==BasketballAction.Dunk);yield return null;}
   Check(saw,"guest observes replicated dunk pose");yield return new WaitForSeconds(.3f);app.view.yaw=actor.transform.eulerAngles.y;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=app.view.yaw};
   Check(ball.Holder==actor&&app.view.BeginShot(),"guest owns ball and begins charge");yield return new WaitForSeconds(.30f);DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,heading=app.view.yaw};
   app.view.SelectFinish(BasketballFinish.Layup);end=Time.time+.6f;while(app.view.ShotNeedsTiming&&Time.time<end)yield return null;SeedLayup(false);
   uint made=ball.Score.made;Check(!app.view.ShotNeedsTiming,"guest eligible layup hides timing: "+app.view.FinishAvailability);app.view.EndShot();end=Time.time+5;saw=false;
   while(Time.time<end){saw|=actor.BasketballMotion.Action==BasketballAction.Layup;yield return null;}
   Check(saw&&ball.Score.made==made+1,"host make roll wins despite guest-local miss seed");Check(ball.Body.isKinematic&&!ball.Body.detectCollisions,"guest basketball stays nonauthoritative");DevelopmentProbe.TurnCommand=new PlayerCommand{heading=app.view.yaw};Signal("guest-done");
   yield return Await("guest-miss-go");yield return new WaitForSeconds(.3f);DevelopmentProbe.TurnCommand=new PlayerCommand{heading=actor.transform.eulerAngles.y};app.view.yaw=actor.transform.eulerAngles.y;
   made=ball.Score.made;uint attempts=ball.Score.attempts;SeedLayup(true);Check(app.view.BeginShot(),"guest begins immediate close layup");app.view.SelectFinish(BasketballFinish.Layup);app.view.EndShot();
   end=Time.time+5;saw=false;while(Time.time<end){saw|=actor.BasketballMotion.Action==BasketballAction.Layup;yield return null;}
   Check(saw&&ball.Score.made==made&&ball.Score.attempts==attempts+1&&ball.Score.result==BasketballResult.Missed,"host miss roll wins despite guest-local make seed: pose="+saw+" result="+ball.Score.result+" notice="+ball.FinishNotice.reason);Signal("guest-miss-done");
  }
  void CaptureControls(string name="finish-controls"){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
   var view=Camera.main;var canvas=FindFirstObjectByType<BasketballHUD>().GetComponentInParent<Canvas>();
   canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=view;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();
   var rt=RenderTexture.GetTemporary(1280,720,24);var old=RenderTexture.active;var previous=view.targetTexture;view.targetTexture=rt;view.Render();RenderTexture.active=rt;
   var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);view.targetTexture=previous;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);
   canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;Canvas.ForceUpdateCanvases();
  }
  void Capture(){
   if(noCapture||SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
   camera.transform.position=hoop.TransformPoint(new Vector3(4.3f,3.1f,5.4f));camera.transform.LookAt(hoop.TransformPoint(new Vector3(0,2.1f,.6f)));camera.fieldOfView=49;
   var rt=RenderTexture.GetTemporary(960,720,24);var old=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(960,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,960,720),0,0);image.Apply();
   var path=Path.Combine(folder,"frames");Directory.CreateDirectory(path);File.WriteAllBytes(Path.Combine(path,(++frame).ToString("D5")+".png"),image.EncodeToPNG());Destroy(image);camera.targetTexture=null;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);
   var motion=actor.BasketballMotion;File.AppendAllText(Path.Combine(folder,"frames.csv"),$"{frame},{stage},{motion.Elapsed:F3},{motion.Action},{actor.transform.position.y:F4},{ball.transform.position.y:F4},{motion.MaximumReachError:F4},{motion.ContactError:F4},{ball.Held}\n");
  }
  void Finish(){Time.captureFramerate=0;DevelopmentProbe.TurnCommand=default;app?.view.ClearMatchInput();File.AppendAllText(Path.Combine(folder,"results.txt"),"BASKETBALL_FINISH_COMPLETE success="+!failed+"\n");}
 }
 public sealed class BasketballGestureReviewInput:BaseInput {
  Vector2 position;bool down,up,held,touch;TouchPhase phase;
  public void Set(Vector2 point,bool press,bool release,bool asTouch){position=point;down=press;up=release;held=!release;touch=asTouch;phase=press?TouchPhase.Began:release?TouchPhase.Ended:TouchPhase.Moved;}
  public void Settle(){down=false;up=false;phase=TouchPhase.Stationary;}
  public override bool mousePresent=>!touch;
  public override Vector2 mousePosition=>position;
  public override Vector2 mouseScrollDelta=>Vector2.zero;
  public override bool GetMouseButtonDown(int button)=>button==0&&down;
  public override bool GetMouseButtonUp(int button)=>button==0&&up;
  public override bool GetMouseButton(int button)=>button==0&&held;
  public override bool touchSupported=>touch;
  public override int touchCount=>touch&&(held||up)?1:0;
  public override Touch GetTouch(int index)=>new Touch{fingerId=71,position=position,phase=phase,type=TouchType.Direct};
  public override float GetAxisRaw(string axis)=>0;
  public override bool GetButtonDown(string name)=>false;
 }
}
#endif
