#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace WhatTheFish {
 public sealed class BasketballPropReview:MonoBehaviour {
  string folder,shared;bool failed;BasketballBall ball;AppRoot app;Athlete actor;Camera reviewCamera;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-basketballPropReview");if(i>=0&&i+1<args.Length)new GameObject("Basketball prop review").AddComponent<BasketballPropReview>().folder=args[i+1];}
  void Check(bool ok,string message){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+message+"\n");}
  void Signal(string name)=>File.WriteAllText(Path.Combine(shared,name),"ready");
  IEnumerator Wait(string name,float seconds=30){float end=Time.realtimeSinceStartup+seconds;while(!File.Exists(Path.Combine(shared,name))&&Time.realtimeSinceStartup<end)yield return null;Check(File.Exists(Path.Combine(shared,name)),"signal "+name);}
  void Place(Athlete a,Vector3 local,float yaw=0){var p=ball.transform.parent.TransformPoint(local);var net=a.GetComponent<NetworkObject>();a.capsule.enabled=false;a.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));if(net&&net.IsSpawned)a.GetComponent<NetworkTransform>().Teleport(p,a.transform.rotation,Vector3.one);a.capsule.enabled=true;a.ResetLocomotion();Physics.SyncTransforms();}
  IEnumerator Pickup(Vector3 position){
   app.view.ClearMatchInput();DevelopmentProbe.TurnCommand=default;Place(actor,position);ball.ResetHome();ball.autoPickup=true;
   ball.Place(position+new Vector3(0,.18f,.6f),Quaternion.identity,Vector3.zero,Vector3.zero);
   float end=Time.time+2;while(ball.Holder!=actor&&Time.time<end)yield return null;yield return new WaitForSeconds(.15f);Check(ball.Holder==actor,"ball pickup "+position);
  }
  bool Use(SportsProp kind){ball.ReviewGiveProp(actor,kind);return ball.TryUseProp(actor,ball.PropSlot(actor).revision);}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);shared=Directory.GetParent(folder).FullName;File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   float end=Time.realtimeSinceStartup+80;while((!(app=AppRoot.Instance)||!app.Exploring||app.SelectedSport!=SportId.Basketball)&&Time.realtimeSinceStartup<end)yield return null;
   ball=BasketballBall.Active;actor=app?app.LocalAthlete:null;if(!ball||!actor){Check(false,"initialized");Finish();yield break;}
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.6f);
   if(!ball.Authority){yield return Guest();Finish();yield break;}
   var peers=Athlete.Active.Where(a=>a.GetComponent<NetworkObject>()&&a.GetComponent<NetworkObject>().IsSpawned).OrderBy(a=>a.GetComponent<NetworkObject>().OwnerClientId).ToArray();
   foreach(var peer in peers)Place(peer,new Vector3(peer==actor?0:4,.08f,peer==actor?-8:6));
   Check(ball.Props.pickup.countingDown&&ball.Props.pickup.kind==SportsProp.None,"session starts with countdown, not an immediate pickup");
   yield return SpawnVisuals();
   yield return SpawnSequence();
   var old=ball.PropSlot(actor);ball.ReviewGiveProp(actor,SportsProp.Turbo);Check(!ball.TryUseProp(actor,old.revision)&&ball.PropSlot(actor).kind==SportsProp.Turbo,"stale request cannot spend replacement inventory");
   Place(actor,new Vector3(BasketballBall.PaintedCourtLimits.x+.6f,.08f,0));
   Check(!ball.TryUseProp(actor,ball.PropSlot(actor).revision),"apron cannot activate ability");
   Place(actor,new Vector3(0,.08f,-9));if(peers.Length>=3){Place(peers[1],new Vector3(3,.08f,-9));Place(peers[2],new Vector3(-3,.08f,-9));}yield return new WaitForSeconds(.2f);
   Check(Use(SportsProp.Turbo),"turbo activation accepted");var spent=ball.PropSlot(actor);Check(spent.kind==SportsProp.None&&!ball.TryUseProp(actor,spent.revision),"spent turbo cannot activate twice");
   Check(ball.TurboRemaining(actor)>4.8f&&ball.TurboRemaining(actor)<=5,"turbo lasts five seconds");
   Signal("turbo-start");yield return MeasureMovement(1.5f);
   ball.ReviewGiveProp(actor,SportsProp.Turbo);Check(!ball.TryUseProp(actor,ball.PropSlot(actor).revision)&&ball.PropSlot(actor).kind==SportsProp.Turbo,"active team buff refuses stacking without consumption");
   if(peers.Length>=3){Check(ball.TurboRemaining(peers[2])>0&&ball.TurboRemaining(peers[1])==0,"boost applies to teammate only, not opponent");}
   end=Time.time+6;while(ball.TurboRemaining(actor)>0&&Time.time<end)yield return null;Check(ball.TurboRemaining(actor)==0,"turbo expires");
   if(peers.Length>=3){yield return Wait("turbo-guest-done",5);yield return Wait("turbo-teammate-done",5);}
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up};yield return new WaitForSeconds(.6f);Check(actor.speed>3.8f&&actor.speed<4.2f,"normal movement restored");DevelopmentProbe.TurnCommand=default;
   yield return Pickup(new Vector3(0,.08f,-8));ball.ReviewGiveProp(actor,SportsProp.LockOn);var hud=FindFirstObjectByType<BasketballPropHUD>();Check(hud,"ability HUD exists");uint shots=ball.ShotCount,rolls=ball.ShotRolls;var position=actor.transform.position;double started=BasketballMotion.Clock;hud.Activate();
   Check(ball.Props.aiming&&ball.PropSlot(actor).kind==SportsProp.None,"one click starts forced aim and spends immediately");
   Check(ball.transform.parent.InverseTransformPoint(ball.ChargeHoop(actor,0).position).z>0,"team A lock-on targets the opposing positive-Z hoop");
   Check(!ball.BeginShotCharge(actor)&&!ball.TryPass(actor,0)&&!actor.CanRequestJump,"aim prevents normal shot, pass and jump");
   Check(Math.Abs(ball.Props.aimUntil-started-2)<.01,"authoritative aim deadline is two seconds");
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,jump=true,sprint=true};yield return new WaitForSeconds(1.4f);DevelopmentProbe.TurnCommand=default;
   Check(ball.Holder==actor&&ball.ShotCount==shots&&Vector3.Distance(position,actor.transform.position)<.025f,"player stays planted before two seconds");
   Capture("lock-on-aim",new Vector3(2,2.4f,-4),new Vector3(0,1,-8));
   end=Time.time+2;while(ball.ShotCount==shots&&Time.time<end)yield return null;
   Check(ball.ShotCount==shots+1&&BasketballMotion.Clock-started>=1.98&&BasketballMotion.Clock-started<2.5,"automatic release after two seconds: "+(BasketballMotion.Clock-started));
   Check(ball.LastShotChance==1&&ball.ShotRolls==rolls,"powered shot bypasses random miss algorithm");
   end=Time.time+6;while(ball.Score.result==BasketballResult.Flying&&Time.time<end)yield return null;Check(ball.Score.result==BasketballResult.Scored,"full-range ability physically scores");
   // Cover corners, near backboard and both extreme long-range approaches.
   foreach(var point in new[]{new Vector3(-6.7f,.08f,-13),new Vector3(6.7f,.08f,-13),new Vector3(-6,.08f,8),new Vector3(0,.08f,10),new Vector3(0,.08f,13.7f)}){
    yield return Pickup(point);Check(Use(SportsProp.LockOn),"grid ability starts "+point);shots=ball.ShotCount;end=Time.time+9;while((ball.ShotCount==shots||ball.Score.result==BasketballResult.Flying)&&Time.time<end)yield return null;Check(ball.Score.result==BasketballResult.Scored,"guaranteed unblocked grid shot "+point);
   }
   yield return Pickup(new Vector3(0,.08f,-6));shots=ball.ShotCount;Check(Use(SportsProp.LockOn),"interruptible aim starts");yield return new WaitForSeconds(.5f);ball.ResetHome();yield return new WaitForSeconds(3.3f);
   Check(!ball.Props.aiming&&!ball.PropLocked(actor)&&ball.PropSlot(actor).kind==SportsProp.None&&ball.ShotCount==shots,"interrupted aim stays consumed, unlocks, never fires later");
   ball.autoPickup=false;Place(actor,new Vector3(0,.08f,6));yield return new WaitForSeconds(.15f);ball.ReviewGiveProp(actor,SportsProp.Portal);Check(!ball.TryUseProp(actor,ball.PropSlot(actor).revision)&&ball.PropSlot(actor).kind==SportsProp.Portal,"enemy-half portal rejected without consumption");
   Place(actor,new Vector3(0,.08f,0));yield return new WaitForSeconds(.15f);Check(!ball.TryUseProp(actor,ball.PropSlot(actor).revision),"centre line excluded from home half");
   Place(actor,new Vector3(0,.08f,-8));ball.Place(new Vector3(3,1,8),Quaternion.identity,Vector3.right*3,Vector3.zero);yield return new WaitForSeconds(.15f);
   var portalSlot=ball.PropSlot(actor);Check(ball.TryUseProp(actor,portalSlot.revision),"own-half portal accepts airborne loose ball");Check(!ball.TryUseProp(actor,portalSlot.revision),"duplicate transfer rejected");
   yield return new WaitForSeconds(.2f);Check(actor.BasketballMotion.Charging,"receiver holds a receiving pose");Check(ball.PortalBusy&&!ball.Held&&ball.Body.isKinematic&&!ball.Body.detectCollisions,"ball reserved exclusively during transfer");Capture("portal-open",new Vector3(5,3.3f,11),new Vector3(3,1,8));
   yield return new WaitForSeconds(.36f);yield return new WaitForEndOfFrame();
   Check(Vector3.Distance(actor.BasketballMotion.RightPalm,ball.Props.destination)<.24f,"portal destination stays within receiving hand reach");
   Capture("portal-catch",new Vector3(2,2,-5),new Vector3(0,1,-8));yield return new WaitForSeconds(.3f);
   Check(ball.Holder==actor&&!ball.PortalBusy&&!ball.PropLocked(actor),"portal ends with one owner and unlocked controls");
   ball.ReviewGiveProp(actor,SportsProp.Portal);Check(!ball.TryUseProp(actor,ball.PropSlot(actor).revision)&&ball.PropSlot(actor).kind==SportsProp.Portal,"self-steal is disabled without spending");
   ball.ResetHome();Place(actor,new Vector3(0,.08f,-8));yield return new WaitForSeconds(.1f);Check(Use(SportsProp.Portal),"transfer interruption setup");actor.inTransit=true;yield return new WaitForSeconds(.15f);actor.inTransit=false;
   Check(!ball.PortalBusy&&!ball.Held&&!ball.Body.isKinematic&&ball.PropSlot(actor).kind==SportsProp.None,"departing receiver safely releases reserved ball");
   if(peers.Length>=3)yield return NetworkCases(peers);
   ball.ReviewResetProps();Check(ball.PropSlot(actor).kind==SportsProp.None&&!ball.Props.aiming&&!ball.PortalBusy&&ball.TurboRemaining(actor)==0,"session reset clears inventory, locks and effects");
   yield return Gallery();Signal("host-done");Finish();
  }
  IEnumerator NetworkCases(Athlete[] peers){
   var opponent=peers[1];var teammate=peers[2];Place(opponent,new Vector3(1,.08f,-5),-90);Place(teammate,new Vector3(4,.08f,-8));yield return Pickup(new Vector3(0,.08f,-5));Check(Use(SportsProp.LockOn),"multiplayer aiming setup");yield return new WaitForSeconds(.5f);uint shots=ball.ShotCount;
   Check(ball.TryBlock(opponent,-90,false),"opponent ordinary block can interrupt prop aim");Check(!ball.Props.aiming&&!ball.Held&&ball.PropSlot(actor).kind==SportsProp.None,"easy block cancels aim and knocks ball loose");yield return new WaitForSeconds(.8f);
   Place(opponent,new Vector3(3,.08f,7));yield return Pickup(new Vector3(0,.08f,-8));ball.ReviewGiveProp(opponent,SportsProp.Portal);yield return new WaitForSeconds(.25f);Signal("guest-portal");yield return Wait("guest-requested",5);yield return Wait("guest-observed",5);
   Check(ball.Holder==opponent&&ball.PropSlot(opponent).kind==SportsProp.None,"guest owner RPC transfers host-held ball once");
   ball.ReviewGiveProp(actor,SportsProp.Portal);yield return new WaitForSeconds(.15f);Check(ball.TryUseProp(actor,ball.PropSlot(actor).revision),"host portal can reclaim opponent-held ball");ball.ReviewGiveProp(teammate,SportsProp.Portal);
   Check(!ball.TryUseProp(teammate,ball.PropSlot(teammate).revision)&&ball.PropSlot(teammate).kind==SportsProp.Portal,"simultaneous second portal retains its charge");yield return new WaitForSeconds(.85f);
   Check(ball.TryUseProp(teammate,ball.PropSlot(teammate).revision),"portal may take teammate-held ball");yield return new WaitForSeconds(.85f);Check(ball.Holder==teammate,"teammate transfer completes");
   yield return Pickup(new Vector3(0,.08f,-8));Check(Use(SportsProp.LockOn),"powered-flight interception setup");shots=ball.ShotCount;float end=Time.time+4;while(ball.ShotCount==shots&&Time.time<end)yield return null;
   ball.ReviewGiveProp(opponent,SportsProp.Portal);Check(ball.TryUseProp(opponent,ball.PropSlot(opponent).revision),"portal intercepts powered shot in flight");yield return new WaitForSeconds(.85f);Check(ball.Holder==opponent&&ball.Score.result==BasketballResult.Missed,"interception cancels scoring, ball arrives once");
   yield return new WaitForSeconds(.5f);ball.ReviewGiveProp(opponent,SportsProp.LockOn);
   Check(ball.TryUseProp(opponent,ball.PropSlot(opponent).revision),"team B lock-on activates with received ball");
   Check(ball.transform.parent.InverseTransformPoint(ball.ChargeHoop(opponent,0).position).z<0,"team B lock-on targets the opposing negative-Z hoop");
   shots=ball.ShotCount;end=Time.time+9;while((ball.ShotCount==shots||ball.Score.result==BasketballResult.Flying)&&Time.time<end)yield return null;
   Check(ball.Score.result==BasketballResult.Scored,"team B assisted shot physically scores at opponent end");
   Signal("network-checked");
  }
  IEnumerator Guest(){
   ulong id=actor.GetComponent<NetworkObject>().OwnerClientId;
   yield return Wait("countdown-start",40);yield return ObserveCountdown(false);
   yield return Wait("countdown-materialized",5);yield return new WaitForSeconds(.1f);
   var expected=File.ReadAllText(Path.Combine(shared,"spawn-location"));Check(ball.Props.pickup.anchor.ToString()==expected,"client sees the same announced spawn side");
   CheckPickup(ball.Props.pickup);Signal("countdown-observed-"+id);
   yield return Wait("turbo-start",80);yield return new WaitForSeconds(.2f);
   yield return MeasureMovement(id==1?1:1.5f);Signal(id==1?"turbo-guest-done":"turbo-teammate-done");
   if(id==1){yield return Wait("guest-portal",140);float end=Time.time+3;while(ball.PropSlot(actor).kind!=SportsProp.Portal&&Time.time<end)yield return null;Check(ball.PropSlot(actor).kind==SportsProp.Portal,"guest receives replicated inventory");var slot=ball.PropSlot(actor);actor.GetComponent<NetworkAthlete>().UseBasketballPropRpc(slot.revision);actor.GetComponent<NetworkAthlete>().UseBasketballPropRpc(slot.revision);Signal("guest-requested");end=Time.time+3;while((ball.Holder!=actor||ball.PropSlot(actor).kind!=SportsProp.None)&&Time.time<end)yield return null;Check(ball.Holder==actor&&ball.PropSlot(actor).kind==SportsProp.None,"guest sees atomic transfer and spent inventory");Signal("guest-observed");}
   yield return Wait("host-done",180);Check(!ball.PortalBusy,"no dangling replicated transfer at completion");
  }
  IEnumerator MeasureMovement(float multiplier){
   foreach(bool sprint in new[]{false,true}){
    float expected=(sprint?7:4)*multiplier;DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,sprint=sprint};yield return new WaitForSeconds(.6f);
    Check(Mathf.Abs(actor.speed-expected)<.25f,(sprint?"sprint":"walk")+" actual controller speed "+actor.speed+" expected "+expected);
    var start=actor.transform.position;float at=Time.time;yield return new WaitForSeconds(.3f);float measured=Vector3.ProjectOnPlane(actor.transform.position-start,Vector3.up).magnitude/(Time.time-at);
    Check(Mathf.Abs(measured-expected)<expected*.12f,(sprint?"sprint":"walk")+" measured travel speed "+measured+" expected "+expected);
   }
   DevelopmentProbe.TurnCommand=default;
  }
  void CheckPickup(BasketballPropSpawn spawn){
   Check(spawn.kind!=SportsProp.None,"materialized prop exists");if(spawn.kind==SportsProp.None)return;var visual=ball.GetComponent<BasketballPropVisual>();var pickup=visual?visual.ReviewPickup():null;
   Check(pickup&&pickup.gameObject.activeInHierarchy,"actual spawned model active "+spawn.kind+" side "+spawn.anchor);if(!pickup)return;
   var renderers=pickup.GetComponentsInChildren<MeshRenderer>();Check(renderers.Length>0&&renderers.All(r=>r.enabled&&r.sharedMaterials.All(m=>m&&m.shader.isSupported)),"spawned model has supported visible materials");if(renderers.Length==0)return;
   var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
   float extent=Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z),floor=ball.transform.parent.TransformPoint(BasketballPropRules.Anchor(spawn.anchor)).y;
   Check(extent>.4f&&extent<1.6f,"spawned model readable metre size "+spawn.kind+" bounds "+bounds.size+" root scale "+pickup.localScale);
   Check(bounds.min.y>floor+.10f&&bounds.max.y<floor+1.4f,"spawned model floats fully above floor");
  }
  IEnumerator SpawnVisuals(){
   foreach(var kind in new[]{SportsProp.LockOn,SportsProp.Turbo,SportsProp.Portal})for(int side=0;side<2;side++){
    ball.ReviewSpawnProps(kind,side);yield return new WaitForEndOfFrame();CheckPickup(ball.Props.pickup);
    var p=BasketballPropRules.Anchor(side);Capture("spawned-"+kind+"-"+side,p+new Vector3(side==0?3:-3,1.5f,-3),p+Vector3.up*.65f);
   }
  }
  IEnumerator ObserveCountdown(bool capture){
   float timeout=Time.time+8;while(!ball.Props.pickup.countingDown&&Time.time<timeout)yield return null;
   var initial=ball.Props.pickup;Check(initial.countingDown,"countdown snapshot received");int seen=0;bool stable=true,noPickup=true,noEarlyGrant=true;var slot=ball.PropSlot(actor);var visual=ball.GetComponent<BasketballPropVisual>();
   while(ball.Props.pickup.countingDown&&Time.time<timeout){
    yield return new WaitForEndOfFrame();var state=ball.Props.pickup;if(!state.countingDown)break;
    int n=state.CountdownNumber(BasketballMotion.Clock);stable&=initial.anchor==state.anchor&&initial.respawn==state.respawn;noPickup&=state.kind==SportsProp.None&&!visual.ReviewPickup();noEarlyGrant&=ball.PropSlot(actor).Equals(slot);
    if((seen&(1<<n))==0){
     seen|=1<<n;Check(visual.ReviewCountdownNumber==n&&visual.ReviewHologram.enabled,"visible hologram digit "+n);
     var mesh=visual.ReviewHologram.GetComponent<MeshFilter>().sharedMesh;Check(mesh.bounds.size.z>.1f&&mesh.bounds.size.y>.7f,"countdown digit has real three-dimensional depth");
     Check(Mathf.Abs(mesh.bounds.center.x)<.001f,"countdown digit is centred over its projection pad");
     if(capture){var p=BasketballPropRules.Anchor(state.anchor)+Vector3.up*1.1f;var toward=ball.transform.parent.InverseTransformPoint(Camera.main.transform.position)-p;Capture("countdown-"+n,p+toward.normalized*4,p);if(n==5)Capture("countdown-court",p+toward.normalized*12,p);}
    }
   }
   Check(seen==62,"countdown visibly steps 5, 4, 3, 2, 1");Check(stable,"announced side and deadline stay fixed");Check(noPickup&&noEarlyGrant,"countdown cannot render or award a pickup early");
   yield return new WaitForEndOfFrame();Check(!ball.Props.pickup.countingDown&&!visual.ReviewHologram.enabled,"hologram disappears at materialization");
  }
  IEnumerator SpawnSequence(){
   for(int side=0;side<2;side++){
    var p=BasketballPropRules.Anchor(side);Check(p.z==0&&Mathf.Abs(p.x)<BasketballBall.PaintedCourtLimits.x-.4f&&Mathf.Abs(p.x)>BasketballBall.PaintedCourtLimits.x-1,"reachable centre-line sideline location "+side);
    Check(!Physics.CheckSphere(ball.transform.parent.TransformPoint(p)+Vector3.up*.5f,.4f,1<<8,QueryTriggerInteraction.Ignore),"centre-line location clears architecture "+side);
   }
   foreach(var previous in new[]{SportsProp.LockOn,SportsProp.Turbo,SportsProp.Portal}){
    var a=BasketballPropRules.NextKind(previous,0);var b=BasketballPropRules.NextKind(previous,1);Check(a!=previous&&b!=previous&&a!=b&&a!=SportsProp.None&&b!=SportsProp.None,"both random choices exclude previous "+previous);
   }
   ball.ReviewResetProps();double started=BasketballMotion.Clock;Signal("countdown-start");yield return ObserveCountdown(true);
   Check(BasketballMotion.Clock-started>=5&&BasketballMotion.Clock-started<5.3,"initial spawn waits five full seconds");CheckPickup(ball.Props.pickup);
   var first=ball.Props.pickup;File.WriteAllText(Path.Combine(shared,"spawn-location"),first.anchor.ToString());Signal("countdown-materialized");
   if(Athlete.Active.Count>=3){yield return Wait("countdown-observed-1",5);yield return Wait("countdown-observed-2",5);}
   Place(actor,BasketballPropRules.Anchor(first.anchor));yield return new WaitForSeconds(.2f);
   Check(ball.PropSlot(actor).kind==first.kind&&ball.Props.pickup.kind==SportsProp.None,"walk-through collects one shared prop");double collectedUntil=ball.Props.pickup.respawn;
   Check(collectedUntil-BasketballMotion.Clock>19.7&&!ball.Props.pickup.countingDown,"collection starts twenty-second respawn interval");
   Place(actor,new Vector3(0,.08f,-8));ball.ReviewGiveProp(actor,SportsProp.None);yield return new WaitForSeconds(14);
   Check(!ball.Props.pickup.countingDown&&ball.Props.pickup.kind==SportsProp.None,"cooldown remains empty before final five seconds");
   float timeout=Time.time+2;while(!ball.Props.pickup.countingDown&&Time.time<timeout)yield return null;
   Check(Math.Abs(ball.Props.pickup.respawn-collectedUntil)<.08,"five-second warning is included in twenty-second respawn");
   Place(actor,BasketballPropRules.Anchor(ball.Props.pickup.anchor));yield return ObserveCountdown(false);yield return new WaitForSeconds(.15f);
   Check(ball.PropSlot(actor).kind!=SportsProp.None&&ball.PropSlot(actor).kind!=first.kind,"waiting player receives a different prop only after countdown");
   var last=ball.PropSlot(actor).kind;
   // Drive many real host transitions without spending twenty seconds per sample.
   for(int i=0;i<16;i++){
    Place(actor,new Vector3(0,.08f,-8));ball.ReviewAdvanceSpawn(15);yield return new WaitForFixedUpdate();yield return null;
    Check(ball.Props.pickup.countingDown&&ball.Props.pickup.anchor<=1&&ball.Props.pickup.CountdownNumber(BasketballMotion.Clock)==5,"next cycle announces one fixed side "+i);
    ball.ReviewAdvanceSpawn(5);yield return new WaitForFixedUpdate();yield return null;
    var next=ball.Props.pickup;Check(next.kind!=SportsProp.None&&next.kind!=last,"consecutive host spawn differs "+i);last=next.kind;
    Place(actor,BasketballPropRules.Anchor(next.anchor));yield return new WaitForSeconds(.1f);
   }
   Place(actor,new Vector3(0,.08f,-8));ball.ReviewResetProps();yield return new WaitForEndOfFrame();ball.enabled=false;yield return new WaitForEndOfFrame();
   Check(!ball.Props.pickup.countingDown&&ball.Props.pickup.kind==SportsProp.None&&!ball.GetComponent<BasketballPropVisual>().ReviewHologram.enabled,"disabling arena clears pending countdown and its hologram");
   ball.enabled=true;yield return new WaitForFixedUpdate();yield return null;Check(ball.Props.pickup.countingDown&&ball.Props.pickup.CountdownNumber(BasketballMotion.Clock)==5,"re-entering starts a fresh five-second countdown");
   ball.ReviewAdvanceSpawn(5);yield return new WaitForFixedUpdate();yield return null;Place(actor,BasketballPropRules.Anchor(ball.Props.pickup.anchor));yield return new WaitForSeconds(.2f);
  }
  IEnumerator Gallery(){
   ball.autoPickup=false;Place(actor,new Vector3(0,.08f,-5));
   var created=new GameObject[3];int i=0;foreach(string name in new[]{"LockOn","Turbo","Portal"}){var asset=Resources.Load<GameObject>("BasketballProps/"+name);Check(asset,"model loaded "+name);if(asset){created[i]=Instantiate(asset,ball.transform.parent);created[i].transform.localPosition=new Vector3(-8.6f,.7f,1+i*1.4f);created[i].transform.localRotation=Quaternion.Euler(0,70,0)*asset.transform.localRotation;}i++;}
   yield return new WaitForEndOfFrame();Capture("prop-gallery",new Vector3(-5.6f,2.2f,5.5f),new Vector3(-8.6f,.65f,2.3f));
   ball.ReviewGiveProp(actor,SportsProp.Turbo);yield return new WaitForEndOfFrame();CaptureHud();yield return new WaitForSeconds(.3f);foreach(var go in created)if(go)Destroy(go);
  }
  void Capture(string name,Vector3 position,Vector3 target){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
   if(!reviewCamera){reviewCamera=new GameObject("Prop review camera").AddComponent<Camera>();reviewCamera.CopyFrom(Camera.main);reviewCamera.enabled=false;}
   reviewCamera.transform.position=ball.transform.parent.TransformPoint(position);reviewCamera.transform.LookAt(ball.transform.parent.TransformPoint(target));reviewCamera.fieldOfView=48;
   var rt=RenderTexture.GetTemporary(1280,720,24);var prior=RenderTexture.active;reviewCamera.targetTexture=rt;reviewCamera.Render();RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());Destroy(image);reviewCamera.targetTexture=null;RenderTexture.active=prior;RenderTexture.ReleaseTemporary(rt);
  }
  void CaptureHud(){
   var cam=Camera.main;var target=new RenderTexture(Screen.width,Screen.height,24);var previous=RenderTexture.active;var cameraTarget=cam.targetTexture;
   var overlays=FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();var cameras=overlays.Select(c=>c.worldCamera).ToArray();var distances=overlays.Select(c=>c.planeDistance).ToArray();
   try{cam.targetTexture=target;for(int i=0;i<overlays.Length;i++){overlays[i].renderMode=RenderMode.ScreenSpaceCamera;overlays[i].worldCamera=cam;overlays[i].planeDistance=1;}Canvas.ForceUpdateCanvases();cam.Render();RenderTexture.active=target;var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,"ability-hud.png"),image.EncodeToPNG());Destroy(image);}
   finally{for(int i=0;i<overlays.Length;i++){overlays[i].renderMode=RenderMode.ScreenSpaceOverlay;overlays[i].worldCamera=cameras[i];overlays[i].planeDistance=distances[i];}cam.targetTexture=cameraTarget;RenderTexture.active=previous;target.Release();Destroy(target);Canvas.ForceUpdateCanvases();}
  }
  void Finish(){DevelopmentProbe.TurnCommand=default;File.AppendAllText(Path.Combine(folder,"results.txt"),"BASKETBALL_PROPS_COMPLETE success="+!failed+"\n");}
 }
}
#endif
