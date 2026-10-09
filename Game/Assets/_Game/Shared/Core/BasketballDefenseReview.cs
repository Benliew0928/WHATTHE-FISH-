#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 [Serializable]public sealed class BasketballDefenseGeometry {
  [Serializable]public sealed class Mesh {public string mesh;public int[] headTriangles,armVertices;}
  public Mesh[] meshes;
 }
 public sealed class BasketballDefenseReview:MonoBehaviour {
  string folder,shared,stage="setup";bool failed,capture,passOnly;int rate=30,frame;AppRoot app;BasketballBall ball;Athlete actor;Camera review;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-basketballDefenseReview");if(i<0||i+1>=args.Length)return;
   var r=new GameObject("Basketball defense review").AddComponent<BasketballDefenseReview>();r.folder=args[i+1];r.capture=!args.Contains("-defenseNoCapture");
   r.passOnly=args.Contains("-defensePassOnly");
   i=Array.IndexOf(args,"-defenseRate");if(i>=0&&i+1<args.Length)int.TryParse(args[i+1],out r.rate);
  }
  void Check(bool ok,string text){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+text+"\n");}
  void Signal(string name,string value="ready"){
   var path=Path.Combine(shared,name);File.WriteAllText(path+".tmp",value);File.Move(path+".tmp",path);
  }
  bool Has(string name)=>File.Exists(Path.Combine(shared,name));
  IEnumerator Await(string name,float timeout=12){float end=Time.realtimeSinceStartup+timeout;while(!Has(name)&&Time.realtimeSinceStartup<end)yield return null;Check(Has(name),"peer checkpoint "+name);}
  void Place(Athlete who,Vector3 p,float yaw=0){who.capsule.enabled=false;who.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));who.capsule.enabled=ball.Authority;who.ResetLocomotion();var net=who.GetComponent<NetworkTransform>();if(net&&net.IsSpawned&&ball.Authority)net.Teleport(p,who.transform.rotation,Vector3.one);Physics.SyncTransforms();}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);Directory.CreateDirectory(Path.Combine(folder,"frames"));shared=Directory.GetParent(folder).FullName;File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   float end=Time.realtimeSinceStartup+80;while((!(app=AppRoot.Instance)||!app.Exploring||!app.LocalAthlete||!BasketballBall.Active)&&Time.realtimeSinceStartup<end)yield return null;
   actor=app?app.LocalAthlete:null;ball=BasketballBall.Active;if(!actor||!ball){Check(false,"court initialized");Finish();yield break;}
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.7f);
   review=new GameObject("Defense review camera").AddComponent<Camera>();review.CopyFrom(Camera.main);review.enabled=false;capture&=SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null;app.view.mode=1;actor.HideHead(false);
   ball.DefenseTrace=line=>File.AppendAllText(Path.Combine(folder,"contact.csv"),stage+","+line+"\n");
   Check(RoomService.ProtocolVersion==38,"matching basketball protocol 38");
   if(app.rooms.Connected){if(ball.Authority)yield return Host();else yield return Guest();}
   else {Rules();Time.captureFramerate=rate;if(passOnly)yield return PassMotion();else yield return Motion();}
   Finish();
  }
  void Rules(){
   bool Sweep(Vector3 h0,Vector3 h1,Vector3 b0,Vector3 b1)=>BasketballDefenseRules.Sweep(h0,h1,b0,b1,.27f,out _);
   Check(Sweep(Vector3.zero,Vector3.right,Vector3.right*.5f+Vector3.forward,Vector3.right*.5f-Vector3.forward),"swept hand and ball contact between frames");
   Check(!Sweep(Vector3.zero,Vector3.right,Vector3.up,Vector3.up+Vector3.right),"ball above actual reach misses");
   Check(!Sweep(Vector3.zero,Vector3.right,new Vector3(float.NaN,0,0),Vector3.zero),"nonfinite contact rejected");
   Check(Sweep(Vector3.zero,Vector3.zero,Vector3.right*.1f,Vector3.right*.1f),"stationary overlapping spheres contact");
   Check(!Sweep(Vector3.zero,Vector3.zero,Vector3.right,Vector3.right),"stationary separated spheres miss");
   foreach(float t in new[]{0f,.11f,.31f,.69f})Check(!BasketballDefenseRules.Active(BasketballAction.Block,t),"standing block inactive at "+t);
   Check(BasketballDefenseRules.Active(BasketballAction.Block,.2f)&&BasketballDefenseRules.Active(BasketballAction.JumpBlock,.6f),"distinct standing and airborne contact windows");
   var hoop=Vector3.forward*12;float Pressure(Vector3 p,Vector3 f)=>BasketballDefenseRules.Pressure(p,f,Vector3.zero,hoop);
   Check(Pressure(Vector3.forward,-Vector3.forward)>.7f,"close facing defender contests shot");
   Check(Pressure(Vector3.back,Vector3.forward)==0&&Pressure(Vector3.forward,Vector3.forward)==0,"behind and facing-away players do not contest");
   Check(Pressure(Vector3.forward*4,-Vector3.forward)==0,"distant guard does not contest");
   Check(BasketballDefenseRules.Window(0)==BasketballBall.SweetWindow&&Mathf.Abs(BasketballDefenseRules.Window(1)-.025f)<.0001f,"pressure has bounded visible tolerance");
   Check(BasketballShotOdds.Chance(4,.65f,.025f)<BasketballShotOdds.Chance(4,.65f),"pressure lowers perfect shot chance");
   Check(BasketballShotOdds.Chance(4,.69f,.025f)<BasketballShotOdds.Chance(4,.69f,.045f),"contested edge release lowers scoring odds");
   var normal=Vector3.back;var deflected=BasketballDefenseRules.Deflect(Vector3.forward*10,Vector3.zero,normal,normal);
   Check(deflected.z<0&&deflected.magnitude<=13,"direct block reflects incoming flight with bounded speed");
   Check(BasketballDefenseRules.ProtectedRim(Vector3.up*4,Vector3.up*3,new Vector3(0,3.3f,0)),"descending shot above rim protected");
   Check(!BasketballDefenseRules.ProtectedRim(Vector3.up*3,Vector3.up*4,new Vector3(0,3.3f,0)),"ascending shot may be blocked");
   var motor=new LocomotionMotor();motor.Reset(0);for(int i=0;i<60;i++)motor.Strafe(Vector3.right,3,true,0,1f/60);
   Check(Mathf.Abs(motor.Yaw)<.01f&&Mathf.Abs(motor.Velocity.x-3)<.01f,"guard strafes at three metres per second while facing forward");
  }
  IEnumerator Motion(){
   ball.autoPickup=false;ball.ResetHome();Place(actor,new Vector3(0,.07f,0));yield return new WaitForSeconds(.5f);
   Check(ball.Role(actor)==BasketballRole.Loose&&!app.view.BeginGuard()&&!app.view.RequestBlock()&&!app.view.BeginSteal(),"loose-ball controls reject possession-dependent actions");
   foreach(var action in new[]{BasketballAction.Block,BasketballAction.JumpBlock,BasketballAction.Blocked})foreach(bool left in new[]{false,true}){
    Place(actor,new Vector3(0,.07f,0));yield return new WaitForSeconds(.3f);
    stage=action+"-"+(left?"left":"right");actor.BasketballMotion.BeginDefense(action,0,actor.transform.position+new Vector3(left?-.3f:.3f,action==BasketballAction.Block?.8f:1.3f,.55f));
    yield return ObserveMotion(BasketballMotion.Duration(action)+.35f,action==BasketballAction.JumpBlock);
   }
   actor.visual.GetComponent<LODGroup>().ForceLOD(1);Place(actor,new Vector3(0,.07f,0));yield return new WaitForSeconds(.3f);
   stage="jump-block-lod1";actor.BasketballMotion.BeginDefense(BasketballAction.JumpBlock,0,actor.transform.position+new Vector3(.3f,1.3f,.55f));yield return ObserveMotion(1.65f,true);actor.visual.GetComponent<LODGroup>().ForceLOD(-1);
   foreach(bool left in new[]{false,true}){
    float sign=left?-1:1;Place(actor,new Vector3(0,.07f,0));yield return new WaitForSeconds(.3f);
    stage="guard-"+(left?"left":"right");actor.capsule.enabled=false;actor.BasketballMotion.Guard(0);yield return ObserveMotion(.7f,false,false);Check(actor.BasketballMotion.Guarding,"guard pose held for surface inspection");
    stage="guard-high-block-"+(left?"left":"right");actor.capsule.enabled=true;actor.BasketballMotion.BeginDefense(BasketballAction.Block,sign*55,actor.transform.position+new Vector3(sign*.15f,1.35f,.4f));yield return ObserveMotion(1.05f,false);
    actor.capsule.enabled=false;actor.BasketballMotion.Guard(0);yield return new WaitForSeconds(.35f);stage="guard-release-"+(left?"left":"right");actor.BasketballMotion.EndGuard();actor.capsule.enabled=true;yield return ObserveMotion(.5f,false);
    actor.BasketballMotion.Begin(BasketballAction.Charge,0,new Vector3(.1f,.9f,.35f));yield return new WaitForSeconds(.35f);
    stage="shot-interrupted-"+(left?"left":"right");actor.BasketballMotion.BeginDefense(BasketballAction.Blocked,0,actor.transform.position+new Vector3(sign*.3f,1.3f,.5f));yield return ObserveMotion(.93f,false);
    actor.capsule.enabled=false;actor.BasketballMotion.Guard(0);yield return new WaitForSeconds(.35f);actor.capsule.enabled=true;
    stage="guard-jump-"+(left?"left":"right");actor.BasketballMotion.BeginDefense(BasketballAction.JumpBlock,0,actor.transform.position+new Vector3(sign*.3f,1.3f,.5f));yield return ObserveMotion(1.65f,true);
    Place(actor,new Vector3(0,.07f,0));yield return new WaitForSeconds(.3f);
    var plan=new BasketballMotionState{action=BasketballAction.Dunk,heading=0,leftHand=left,finishOrigin=actor.transform.position,finishTarget=actor.transform.position+Vector3.forward*.7f,finishJump=Mathf.Sqrt(2*JumpMotor.Gravity*1.8f)};
    actor.BasketballMotion.BeginFinish(plan,new Vector3(sign*.2f,.8f,.3f));yield return new WaitForSeconds(.48f);
    stage="dunk-interrupted-"+(left?"left":"right");Check(actor.Airborne,"interruption begins in actual dunk flight");actor.BasketballMotion.BeginDefense(BasketballAction.Blocked,0,actor.transform.position+new Vector3(sign*.3f,1.3f,.5f));yield return ObserveMotion(1.2f,false);
   }
   Place(actor,new Vector3(0,.07f,0));DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.right};yield return new WaitForSeconds(.4f);
   stage="moving-jump-block";actor.BasketballMotion.BeginDefense(BasketballAction.JumpBlock,actor.transform.eulerAngles.y,actor.transform.position+actor.transform.forward*.5f+Vector3.up*1.3f);yield return ObserveMotion(1.65f,true);DevelopmentProbe.TurnCommand=default;
   yield return PassMotion();
  }
  IEnumerator PassMotion(){
   ball.autoPickup=false;ball.ResetHome();
   // Passes use the same calibrated rig. Keep its oversized stylized head in
   // the geometry sweep when tuning the chest gather and follow-through.
   foreach(bool lod1 in new[]{false,true}){
    Place(actor,new Vector3(0,.07f,0));yield return new WaitForSeconds(.3f);
    stage="pass-hold"+(lod1?"-lod1":"");actor.BasketballMotion.Begin(BasketballAction.PassCharge,0,new Vector3(.34f,.72f,.32f));yield return ObserveMotion(.8f,false,false);
    stage="pass-push"+(lod1?"-lod1":"");actor.BasketballMotion.Begin(BasketballAction.Pass,0,new Vector3(0,.78f,.36f));yield return ObserveMotion(1.0f,false);
    foreach(float bend in new[]{-1f,1f}){
     stage="pass-"+BasketballPassRules.Name(bend)+(lod1?"-lod1":"");actor.BasketballMotion.Begin(BasketballAction.PassCharge,0,new Vector3(.34f,.72f,.32f));yield return ObserveMotion(.6f,false,false);
     actor.BasketballMotion.Begin(BasketballAction.Pass,0,new Vector3(0,.78f,.36f));var pose=actor.BasketballMotion.State;pose.passBend=bend;actor.BasketballMotion.Receive(pose);yield return ObserveMotion(1.0f,false);
    }
    // Cancellation returns to dribbling, so it requires actual possession.
    // Cancelling a synthetic unheld pose instead returns to idle and tests an
    // unreachable transition rather than the player's control path.
    ball.autoPickup=true;var point=actor.transform.position+Vector3.forward*.6f+Vector3.up*.2f;
    ball.Place(ball.transform.parent.InverseTransformPoint(point),Quaternion.identity,Vector3.zero,Vector3.zero);float until=Time.time+3;while(ball.Holder!=actor&&Time.time<until)yield return null;Check(ball.Holder==actor,"pass cancel has real possession");
    yield return new WaitForSeconds(.5f);
    stage="pass-cancel"+(lod1?"-lod1":"");Check(ball.BeginPassCharge(actor,0,ball.Defense.play),"pass cancel begins through host rules");yield return ObserveMotion(.9f,false,false);ball.CancelPassCharge(actor,ball.Defense.play);yield return ObserveMotion(.6f,false);ball.autoPickup=false;ball.ResetHome();
   }
  }
  IEnumerator ObserveMotion(float seconds,bool jump,bool settles=true){
   var joints=actor.visual.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("mixamorig:")&&!t.name.Contains("_end")).ToArray();var old=joints.Select(t=>actor.transform.InverseTransformPoint(t.position)).ToArray();
   float end=Time.time+seconds,maxStep=0,armStep=0,contact=0,peak=actor.transform.position.y,minFoot=100;int samples=0,penetrations=0;string worst="";
   while(Time.time<end){
    yield return new WaitForEndOfFrame();var motion=actor.BasketballMotion;samples++;peak=Mathf.Max(peak,actor.transform.position.y);contact=Mathf.Max(contact,motion.ContactError);
    for(int i=0;i<joints.Length;i++){var p=actor.transform.InverseTransformPoint(joints[i].position);float step=Vector3.Distance(old[i],p);if(samples>1){if(step>maxStep){maxStep=step;worst=joints[i].name+" at "+motion.Elapsed.ToString("F3");}if(joints[i].name.Contains("Arm")||joints[i].name.Contains("Hand"))armStep=Mathf.Max(armStep,step);}old[i]=p;if(joints[i].name.EndsWith("Foot"))minFoot=Mathf.Min(minFoot,joints[i].position.y);}
    penetrations+=HeadIntersections();Capture(actor);
   }
   Check(contact<.085f,stage+" hand/contact agreement="+contact.ToString("F4"));
   // The running shoe tip travels faster than the hands. Keep the arm limit
   // separate so a natural stride cannot hide the original elbow flips.
   Check(armStep<6f/rate+.012f,stage+" continuous arms max-step="+armStep.ToString("F4"));
   Check(maxStep<8f/rate+.012f,stage+" continuous joints max-step="+maxStep.ToString("F4")+" "+worst);
   Check(minFoot>-.03f,stage+" feet remain above court="+minFoot.ToString("F4"));
   Check(penetrations==0,stage+" hand/forearm vertices inside head="+penetrations);
   if(settles)Check(!actor.BasketballMotion.Busy&&!actor.Airborne&&actor.Grounded,stage+" recovers and lands");
   if(jump)Check(peak>1.8f&&peak<2.3f,stage+" actual capsule jump peak="+peak.ToString("F3"));
  }
  BasketballDefenseGeometry geometry;UnityEngine.Mesh baked;SkinnedMeshRenderer skin;BasketballDefenseGeometry.Mesh surface;Transform headBone;Vector3 headMin,headMax;string cachedMesh;
  struct Triangle {public Vector3 a,b,c;}
  readonly System.Collections.Generic.List<Triangle>[] cells=new System.Collections.Generic.List<Triangle>[32*32];
  int HeadIntersections(){
   if(geometry==null){
    string path=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"defense-geometry.json");
    if(!File.Exists(path)){Check(false,"external head clearance fixture exists");return 1;}
    geometry=JsonUtility.FromJson<BasketballDefenseGeometry>(File.ReadAllText(path));baked=new UnityEngine.Mesh();
   }
   bool lod1=stage.Contains("lod1");skin=actor.visual.GetComponentsInChildren<SkinnedMeshRenderer>().First(s=>s.sharedMesh.name.Contains(lod1?"LOD1":"LOD0"));surface=geometry.meshes.First(m=>m.mesh==skin.sharedMesh.name);
   // The imported renderer carries a 100x scale. Compensate while baking so
   // localToWorldMatrix applies that scale once, including during movement.
   skin.BakeMesh(baked,true);var points=baked.vertices;
   if(!headBone)headBone=actor.visual.GetComponentsInChildren<Transform>().First(t=>t.name=="mixamorig:Head");
   var matrix=Matrix4x4.TRS(headBone.position,headBone.rotation,Vector3.one).inverse*skin.transform.localToWorldMatrix;
   int X(float x)=>Mathf.Clamp(Mathf.FloorToInt((x-headMin.x)/(headMax.x-headMin.x)*31),0,31);
   int Y(float y)=>Mathf.Clamp(Mathf.FloorToInt((y-headMin.y)/(headMax.y-headMin.y)*31),0,31);
   if(cachedMesh!=surface.mesh){
    Debug.Log($"DEFENSE_GEOMETRY {skin.name} scale={skin.transform.lossyScale} shared={skin.sharedMesh.bounds} baked={baked.bounds} world={skin.bounds} head={headBone.position}");
    cachedMesh=surface.mesh;for(int i=0;i<cells.Length;i++){if(cells[i]==null)cells[i]=new();else cells[i].Clear();}
    var ts=surface.headTriangles;var headPoints=points.Select(p=>matrix.MultiplyPoint3x4(p)).ToArray();headMin=Vector3.one*100;headMax=Vector3.one*-100;
    foreach(int index in ts){headMin=Vector3.Min(headMin,headPoints[index]);headMax=Vector3.Max(headMax,headPoints[index]);}
    Check((headMax-headMin).magnitude>.5f&&(headMax-headMin).magnitude<1.6f,"head surface measured in world metres: "+(headMax-headMin).magnitude.ToString("F3"));
    for(int i=0;i<ts.Length;i+=3){var t=new Triangle{a=headPoints[ts[i]],b=headPoints[ts[i+1]],c=headPoints[ts[i+2]]};
     for(int x=X(Mathf.Min(t.a.x,t.b.x,t.c.x));x<=X(Mathf.Max(t.a.x,t.b.x,t.c.x));x++)for(int y=Y(Mathf.Min(t.a.y,t.b.y,t.c.y));y<=Y(Mathf.Max(t.a.y,t.b.y,t.c.y));y++)cells[y*32+x].Add(t);
    }
   }
   int hits=0;float deepest=0;
   foreach(int index in surface.armVertices){var p=matrix.MultiplyPoint3x4(points[index]);if(p.x<headMin.x||p.x>headMax.x||p.y<headMin.y||p.y>headMax.y||p.z<headMin.z||p.z>headMax.z)continue;float near=float.PositiveInfinity,far=float.NegativeInfinity;
    foreach(var t in cells[Y(p.y)*32+X(p.x)]){
     float det=(t.b.y-t.c.y)*(t.a.x-t.c.x)+(t.c.x-t.b.x)*(t.a.y-t.c.y);if(Mathf.Abs(det)<.00000001f)continue;
     float u=((t.b.y-t.c.y)*(p.x-t.c.x)+(t.c.x-t.b.x)*(p.y-t.c.y))/det;
     float v=((t.c.y-t.a.y)*(p.x-t.c.x)+(t.a.x-t.c.x)*(p.y-t.c.y))/det;
     if(u<0||v<0||u+v>1)continue;float z=u*t.a.z+v*t.b.z+(1-u-v)*t.c.z;near=Mathf.Min(near,z);far=Mathf.Max(far,z);
    }
    float depth=Mathf.Min(p.z-near,far-p.z);if(depth>.003f){hits++;deepest=Mathf.Max(deepest,depth);}
   }
   File.AppendAllText(Path.Combine(folder,"head-clearance.csv"),$"{frame},{stage},{actor.BasketballMotion.Elapsed:F4},{hits},{deepest:F5}\n");return hits;
  }
  IEnumerator Possess(Athlete victim,Athlete defender,Vector3 victimPoint,Vector3 defenderPoint,float heading=270,float victimYaw=0){
   app.view.ClearMatchInput();DevelopmentProbe.TurnCommand=default;ball.ResetHome();Place(defender,new Vector3(5,.07f,-5));Place(victim,victimPoint,victimYaw);
   ball.autoPickup=true;var point=victimPoint+victim.transform.forward*.6f+Vector3.up*.2f;
   ball.Place(ball.transform.parent.InverseTransformPoint(point),Quaternion.identity,Vector3.zero,Vector3.zero);
   float end=Time.time+3;while(ball.Holder!=victim&&Time.time<end)yield return null;Check(ball.Holder==victim,"real proximity possession");
   Place(defender,defenderPoint,heading);app.view.yaw=actor.transform.eulerAngles.y;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=app.view.yaw};yield return new WaitForSeconds(.2f);
  }
  IEnumerator WaitCapture(float seconds,Athlete subject=null){float end=Time.time+seconds;while(Time.time<end){yield return new WaitForEndOfFrame();Capture(subject?subject:actor);}}
  IEnumerator Host(){
   yield return Await("guest-ready");var guest=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).First(p=>!p.IsOwner).GetComponent<Athlete>();
   yield return Possess(guest,actor,new Vector3(0,.07f,0),new Vector3(0,.07f,1.05f),180);
   Check(ball.Role(actor)==BasketballRole.Defense&&ball.Role(guest)==BasketballRole.Attack,"roles follow host possession");
   var guard=FindObjectsByType<BasketballDefenseButton>(FindObjectsSortMode.None).Single(p=>p.guard);var block=FindObjectsByType<BasketballDefenseButton>(FindObjectsSortMode.None).Single(p=>!p.guard);
   Check(guard.enabled&&block.enabled&&!FindObjectsByType<BasketballShootButton>(FindObjectsSortMode.None).Any(p=>p.enabled),"defender shows defense handlers only");
   var touch=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=41,position=new Vector2(900,300)};
   guard.OnPointerDown(touch);yield return new WaitForSeconds(.3f);stage="guard-stationary";yield return WaitCapture(.7f);
   Check(app.view.GuardHeld&&actor.BasketballMotion.Guarding,"hold enters replicated guard stance");CaptureControls("defender-controls");
   guard.OnPointerUp(new PointerEventData(EventSystem.current){pointerId=42});Check(app.view.GuardHeld,"other finger cannot release guard");
   Check(ball.BeginShotCharge(guest,0),"attacker charges against guard");yield return new WaitForSeconds(.25f);
   Check(ball.Charge.pressure>.5f&&BasketballDefenseRules.Window(ball.Charge.pressure)<.04f,"real frontal guarding narrows host shot window");
   var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=8;wall.transform.position=new Vector3(0,1,.5f);wall.transform.localScale=new Vector3(2,2,.05f);Physics.SyncTransforms();
   Check(ball.ShotPressure(guest,ball.ChargeHoop(guest,0))==0,"wall prevents pressure");Destroy(wall);yield return null;
   stage="guard-shuffle";var initial=actor.transform.position;DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.right,heading=180,sprint=true};yield return WaitCapture(.6f);
   Check(actor.speed>2.5f&&actor.speed<3.2f&&Vector3.Distance(initial,actor.transform.position)>1,"guard overrides sprint with lateral movement");
   Check(Vector3.Dot(actor.transform.forward,(guest.transform.position-actor.transform.position).normalized)>.8f,"shuffle keeps chest toward attacker");
   DevelopmentProbe.TurnCommand=new PlayerCommand{heading=180};guard.OnPointerUp(touch);yield return new WaitForSeconds(.3f);Check(!app.view.GuardHeld&&!actor.BasketballMotion.Guarding,"release exits guard");ball.CancelShotCharge(guest);
   yield return Possess(guest,actor,new Vector3(0,.07f,0),new Vector3(.83f,.07f,.32f));
   uint before=ball.Defense.blocks;Check(!ball.TryBlock(guest,0,false)&&!ball.TryBlock(actor,float.NaN,false),"holder and invalid heading cannot block");
   Check(!ball.TryBlock(actor,270,false,ball.Defense.play-1),"stale possession request rejected");
   stage="standing-dribble-block";float until=Time.time+5;
   while(ball.Defense.blocks==before&&Time.time<until){if(ball.CanBlock(actor))app.view.RequestBlock();yield return WaitCapture(.1f);}
   Check(ball.Defense.blocks==before+1&&!ball.Held,"standing contact knocks exposed dribble loose");
   Check(!ball.Body.isKinematic&&ball.Body.detectCollisions&&ball.Body.linearVelocity.magnitude>1,"block creates physical unowned rebound");
   Check(ball.Score.attempts==0,"blocked dribble does not add a shot attempt");yield return new WaitForSeconds(.8f);
   yield return Possess(guest,actor,new Vector3(0,.07f,0),new Vector3(2.8f,.07f,.32f));before=ball.Defense.blocks;Check(app.view.RequestBlock(),"distant block still spends animation");yield return WaitCapture(.8f);Check(ball.Defense.blocks==before&&ball.Holder==guest,"out-of-reach block misses");
   yield return Possess(guest,actor,new Vector3(0,.07f,0),new Vector3(.83f,.07f,.32f));
   wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=8;wall.transform.position=new Vector3(.60f,.75f,.32f);wall.transform.localScale=new Vector3(.04f,1.5f,1.5f);Physics.SyncTransforms();
   before=ball.Defense.blocks;ball.TryBlock(actor,270,false);yield return WaitCapture(.8f);Check(ball.Defense.blocks==before&&ball.Holder==guest,"solid wall blocks hand contact");Destroy(wall);yield return null;
   yield return Possess(guest,actor,new Vector3(0,.07f,0),new Vector3(-.83f,.07f,.32f),90);before=ball.Defense.blocks;
   ball.TryBlock(actor,90,false);yield return WaitCapture(.8f);Check(ball.Defense.blocks==before&&ball.Holder==guest,"carrier torso shields far-side block");
   yield return Possess(guest,actor,new Vector3(0,.07f,0),new Vector3(2.8f,.07f,.32f));
   // The same main button selects an airborne block using its owning finger.
   block.OnPointerDown(touch);var drag=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=41,position=touch.position+Vector2.up*90};block.OnDrag(drag);
   block.OnPointerUp(new PointerEventData(EventSystem.current){pointerId=42});yield return new WaitForSeconds(.08f);Check(!actor.BasketballMotion.JumpBlocking,"other finger cannot commit block gesture");
   block.OnPointerUp(drag);yield return new WaitForSeconds(.22f);Check(actor.BasketballMotion.JumpBlocking&&actor.Airborne,"upward block gesture launches real jump");yield return WaitCapture(1.25f);
   yield return Possess(guest,actor,new Vector3(0,.07f,0),new Vector3(.83f,.07f,.32f));
   Check(ball.TryBlock(actor,270,false),"standing block accepted");int accepted=0;for(int i=0;i<100;i++)if(ball.TryBlock(actor,270,true))accepted++;Check(accepted==0&&!actor.CanRequestJump,"spam and jump cannot cancel committed block");
   ball.ResetHome();before=ball.Defense.blocks;yield return new WaitForSeconds(.8f);Check(ball.Defense.blocks==before,"reset cancels pending contact");
   // Meet the shot with the outside hand; standing directly behind one's own
   // face is no longer a reachable hand lane on the revised short-arm rig.
   yield return Possess(guest,actor,new Vector3(0,.07f,0),new Vector3(.52f,.07f,-.06f));ball.BeginShotCharge(guest,0);
   until=Time.time+1.5f;while(!guest.BasketballMotion.Charging&&Time.time<until)yield return null;yield return new WaitForSeconds(.30f);
   before=ball.Defense.blocks;stage="standing-shot-block";app.view.RequestBlock();ball.TryShoot(guest,0);yield return WaitCapture(.9f);
   Check(ball.Defense.blocks==before+1,"standing block interrupts reachable shot gather or release");
   yield return Possess(guest,actor,new Vector3(0,.07f,0),new Vector3(5,.07f,-5));
   Check(ball.BeginPassCharge(guest,0,ball.Defense.play),"carrier charges a real chest pass");yield return new WaitForSeconds(.9f);Check(ball.ReleasePassCharge(guest,0,ball.Defense.play,ball.PassClock),"carrier releases a real chest pass");until=Time.time+1;while(ball.Held&&Time.time<until)yield return null;yield return new WaitForSeconds(.55f);
   // Intercept from the passing lane's side: a frontal torso collision is a body deflection, not a hand block.
   var travel=Vector3.ProjectOnPlane(ball.Body.linearVelocity,Vector3.up);var direction=travel.normalized;var lateral=Vector3.Cross(Vector3.up,direction);var intercept=ball.Body.position+travel*.18f+lateral*.58f;intercept.y=.07f;
   Place(actor,intercept,Quaternion.LookRotation(-lateral).eulerAngles.y);app.view.yaw=actor.transform.eulerAngles.y;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=app.view.yaw};before=ball.Defense.blocks;uint attempts=ball.Score.attempts;
   Check(ball.TryBlock(actor,app.view.yaw,false),"released pass accepts standing block request");stage="released-pass-block";yield return WaitCapture(.8f);
   Check(ball.Defense.blocks==before+1&&ball.Score.attempts==attempts,"released pass deflects without adding a shot attempt");
   yield return Possess(guest,actor,new Vector3(0,.07f,0),new Vector3(.83f,.07f,.32f));guard.OnPointerDown(touch);yield return new WaitForSeconds(.25f);
   ball.ResetHome();Place(guest,new Vector3(5,.07f,5));var pickup=actor.transform.position+Vector3.up*.2f+actor.transform.forward*.45f;ball.Place(ball.transform.parent.InverseTransformPoint(pickup),Quaternion.identity,Vector3.zero,Vector3.zero);
   until=Time.time+3;while(ball.Holder!=actor&&Time.time<until)yield return null;yield return new WaitForSeconds(.3f);uint passes=ball.PassCount;guard.OnPointerUp(touch);yield return new WaitForSeconds(.3f);
   Check(ball.Holder==actor&&!app.view.GuardHeld&&ball.PassCount==passes,"held defense finger cannot become pass after pickup");
   Check(!guard.enabled&&!block.enabled&&FindObjectsByType<BasketballShootButton>(FindObjectsSortMode.None).All(p=>p.enabled),"attacker shows offense handlers only");CaptureControls("attacker-controls");
   int number=0;
   foreach(string hoopName in new[]{"Hoop_North","Hoop_South"})foreach(var kind in new[]{BasketballFinish.Layup,BasketballFinish.Dunk}){
    var hoop=app.stadium.GetComponentsInChildren<Transform>().Single(t=>t.name==hoopName);float side=number%2==0?-.55f:.55f;
    var origin=hoop.TransformPoint(new Vector3(side,.07f,3));float yaw=Quaternion.LookRotation(Vector3.ProjectOnPlane(hoop.position-origin,Vector3.up)).eulerAngles.y;
    yield return Possess(actor,guest,origin,new Vector3(5,.07f,-5),0,yaw);app.view.yaw=yaw;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=yaw};
    app.view.BeginShot();yield return new WaitForSeconds(.3f);DevelopmentProbe.TurnCommand=new PlayerCommand{heading=yaw,move=Vector2.up};yield return new WaitForSeconds(.18f);
    app.view.SelectFinish(kind);app.view.EndShot();until=Time.time+1;while(!actor.BasketballMotion.Finishing&&Time.time<until)yield return null;
    Check(actor.BasketballMotion.Finishing,"committed "+hoopName+" "+kind);DevelopmentProbe.TurnCommand=new PlayerCommand{heading=yaw};
    var plan=actor.BasketballMotion.State;float contactTime=kind==BasketballFinish.Dunk?.61f:.50f;
    var target=BasketballMotion.FinishRoot(plan,contactTime)+Quaternion.Euler(0,plan.heading,0)*BasketballMotion.FinishBall(plan,contactTime);
    var sideVector=Quaternion.Euler(0,plan.heading,0)*Vector3.right;var defenderPoint=target+sideVector*(kind==BasketballFinish.Layup?.27f:.30f);defenderPoint.y=.07f;
    if(kind==BasketballFinish.Layup)yield return new WaitForSeconds(.20f);
    Place(guest,defenderPoint,Quaternion.LookRotation(-sideVector).eulerAngles.y);before=ball.Defense.blocks;uint shots=ball.ShotCount;
    stage=hoopName+"-"+kind+"-jump-block";Signal("guest-command-"+(++number),Quaternion.LookRotation(-sideVector).eulerAngles.y.ToString(System.Globalization.CultureInfo.InvariantCulture));yield return WaitCapture(1.7f,guest);
    Check(ball.Defense.blocks==before+1,"guest jump blocks "+hoopName+" "+kind);
    Check(!actor.Airborne&&!guest.Airborne&&!actor.BasketballMotion.Finishing,"both players land after "+kind+" interruption");
    Check(ball.ShotCount<=shots+1,"interrupted finish releases at most once");
   }
   Signal("host-done");yield return Await("guest-done");app.view.ClearMatchInput();var task=app.rooms.SetExploring(false);while(!task.IsCompleted)yield return null;yield return new WaitForSeconds(.3f);
   Check(!ball.Held&&!ball.Simulating&&!app.view.GuardHeld,"session exit clears defensive input and ball");
  }
  IEnumerator Guest(){
   Check(!ball.Authority&&!ball.TryBlock(actor,0,true),"guest cannot mutate authoritative ball");Signal("guest-ready");int command=0;bool sawGuard=false,sawBlock=false,sawAir=false;uint maximum=0;float end=Time.realtimeSinceStartup+130;
   while(!Has("host-done")&&Time.realtimeSinceStartup<end){
    foreach(var p in Athlete.Active){sawGuard|=p.BasketballMotion.Guarding;sawBlock|=p.BasketballMotion.Blocking;sawAir|=p.BasketballMotion.JumpBlocking&&p.Airborne;}
    maximum=Math.Max(maximum,ball.Defense.blocks);
    if(Has("guest-command-"+(command+1))&&ball.CanBlock(actor)){var next=File.ReadAllText(Path.Combine(shared,"guest-command-"+(++command)));app.view.yaw=float.Parse(next,System.Globalization.CultureInfo.InvariantCulture);DevelopmentProbe.TurnCommand=new PlayerCommand{heading=app.view.yaw};Check(app.view.RequestBlock(true),"guest requests jump block "+command);}
    yield return null;
   }
   Check(Has("host-done")&&sawGuard&&sawBlock&&sawAir,"guest observes guard, block and actual jump replication");
   Check(maximum>=5,"guest receives authoritative contact results");Check(ball.Body.isKinematic&&!ball.Body.detectCollisions,"guest remains noncolliding replica");Signal("guest-done");
  }
  void CaptureControls(string name){
   if(!capture)return;var view=Camera.main;var canvas=FindFirstObjectByType<BasketballHUD>().GetComponentInParent<Canvas>();
   var mode=canvas.renderMode;var oldCamera=canvas.worldCamera;float plane=canvas.planeDistance;
   canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=view;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();
   var rt=RenderTexture.GetTemporary(1280,720,24);var old=RenderTexture.active;var previous=view.targetTexture;view.targetTexture=rt;view.Render();RenderTexture.active=rt;
   var picture=new Texture2D(1280,720,TextureFormat.RGB24,false);picture.ReadPixels(new Rect(0,0,1280,720),0,0);picture.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),picture.EncodeToPNG());Destroy(picture);
   view.targetTexture=previous;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);canvas.renderMode=mode;canvas.worldCamera=oldCamera;canvas.planeDistance=plane;Canvas.ForceUpdateCanvases();
  }
  void Capture(Athlete subject){
   var motion=subject.BasketballMotion;var palm=motion.State.leftHand?motion.LeftPalm:motion.RightPalm;
   File.AppendAllText(Path.Combine(folder,"motion.csv"),$"{frame},{stage},{motion.Action},{motion.Elapsed:F4},{subject.transform.position.y:F4},{motion.ContactError:F4},{Vector3.Distance(palm,ball.transform.position):F4},{ball.Defense.blocks},{ball.Held},{palm.x:F4},{palm.y:F4},{palm.z:F4},{ball.transform.position.x:F4},{ball.transform.position.y:F4},{ball.transform.position.z:F4},{(ball.Holder?ball.Holder.BasketballMotion.Elapsed:0):F4}\n");
   if(capture&&frame%2==0){
    RenderFrame(subject,new Vector3(2.4f,.65f,3.6f),"frames");
    if(!app.rooms.Connected&&frame%4==0){RenderFrame(subject,new Vector3(0,.50f,4.3f),"front");RenderFrame(subject,new Vector3(4.3f,.50f,0),"side");}
   }
   frame++;
  }
  void RenderFrame(Athlete subject,Vector3 offset,string directory){
    Directory.CreateDirectory(Path.Combine(folder,directory));
    var focus=subject.transform.position+Vector3.up*.78f;review.transform.position=focus+subject.transform.rotation*offset;review.transform.LookAt(focus);review.fieldOfView=33;
    var rt=RenderTexture.GetTemporary(960,720,24);var old=RenderTexture.active;review.targetTexture=rt;review.Render();RenderTexture.active=rt;
    var image=new Texture2D(960,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,960,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,directory,frame.ToString("D5")+".png"),image.EncodeToPNG());Destroy(image);review.targetTexture=null;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);
  }
  void Finish(){Time.captureFramerate=0;DevelopmentProbe.TurnCommand=default;app?.view.ClearMatchInput();if(review)Destroy(review.gameObject);File.AppendAllText(Path.Combine(folder,"results.txt"),"BASKETBALL_DEFENSE_COMPLETE success="+!failed+"\n");}
 }
}
#endif
