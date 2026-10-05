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
 // Opt-in executable integration checks, excluded from the release APK.
 public sealed class BasketballGameplayReview:MonoBehaviour {
  string folder;bool failed;AppRoot app;BasketballBall ball;Athlete actor;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-basketballGameplayReview");if(i<0||i+1>=args.Length)return;
   new GameObject("Basketball gameplay review").AddComponent<BasketballGameplayReview>().folder=args[i+1];
  }
  void Check(bool ok,string message){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+message+"\n");}
  IEnumerator WaitGreen(){
   float end=Time.time+4;while(Time.time<end){float p=app.view.ShotPower;if(app.view.ShotCharging&&Mathf.Abs(p-BasketballBall.SweetSpot)<=BasketballBall.SweetWindow)yield break;yield return null;}
   Check(false,"live needle reaches a green window");
  }
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   float end=Time.realtimeSinceStartup+80;
   while((!(app=AppRoot.Instance)||!app.Exploring||app.SelectedSport!=SportId.Basketball)&&Time.realtimeSinceStartup<end)yield return null;
   ball=BasketballBall.Active;actor=app?app.LocalAthlete:null;
   if(!ball||!actor){Check(false,"basketball and player initialized");Finish();yield break;}
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.5f);
   Check(FindObjectsByType<BasketballShootButton>(FindObjectsSortMode.None).FirstOrDefault(b=>!b.pass),"minimal Shoot control present");
   Check(Mathf.Abs(ball.arcPerMetre-.18f)<.0001f,"larger ball shot arc imported");
   if(app.rooms.Connected){if(app.rooms.Host)yield return Host();else yield return Guest();}else yield return Offline();
   Finish();
  }
  void Finish(){File.AppendAllText(Path.Combine(folder,"results.txt"),"BASKETBALL_GAMEPLAY_COMPLETE success="+!failed+"\n");}
  void PlaceActor(Athlete value,Vector3 position,float yaw=0){
   value.capsule.enabled=false;value.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));
   var net=value.GetComponent<NetworkTransform>();if(net&&net.IsSpawned)net.Teleport(position,value.transform.rotation,Vector3.one);
   value.capsule.enabled=true;value.ResetLocomotion();Physics.SyncTransforms();
  }
  IEnumerator WaitHeld(Athlete value){float end=Time.time+3;while(ball.Holder!=value&&Time.time<end)yield return null;Check(ball.Holder==value,"auto pickup for "+value.name);}
  IEnumerator Offline(){
   ball.ResetHome();PlaceActor(actor,new Vector3(0,.07f,-4));
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up};yield return WaitHeld(actor);DevelopmentProbe.TurnCommand=default;
   Check(ball.Body.isKinematic&&!ball.Body.detectCollisions,"held ball cannot collide with holder");
   var start=ball.transform.position;DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.right,sprint=true};yield return new WaitForSeconds(.6f);DevelopmentProbe.TurnCommand=default;
   yield return new WaitForEndOfFrame();Check(Vector3.Distance(start,ball.transform.position)>2&&Vector3.Distance(ball.transform.position,ball.CarryPosition(actor))<.04f,"ball follows moving and turning player");
   Check(!ball.TryShoot(actor,float.NaN)&&ball.Held,"nonfinite shot rejected without losing possession");
   app.view.yaw=actor.transform.eulerAngles.y;
   for(int mode=0;mode<3;mode++){app.view.mode=mode;yield return new WaitForSeconds(.12f);Capture("carry-camera"+mode);if(mode==1)Capture("carry-controls",true);}
   app.view.mode=1;
   foreach(var trial in new[]{("Hoop_North",2f),("Hoop_North",5f),("Hoop_North",10f),("Hoop_North",18f),("Hoop_North",24f),("Hoop_South",5f)}){
    var hoop=app.stadium.GetComponentsInChildren<Transform>().Single(t=>t.name==trial.Item1);
    var inward=Vector3.ProjectOnPlane(-hoop.position,Vector3.up).normalized;var point=hoop.position+inward*trial.Item2;point.y=.07f;
    float yaw=Quaternion.LookRotation(-inward).eulerAngles.y;PlaceActor(actor,point,yaw);
    ball.Place(point-inward*.65f+Vector3.up*.06f,Quaternion.identity,Vector3.zero,Vector3.zero);yield return WaitHeld(actor);
    DevelopmentProbe.TurnCommand=new PlayerCommand{heading=yaw};app.view.yaw=yaw;
    Check(ball.SelectHoop(ball.CarryPosition(actor),yaw)==hoop,"camera chooses "+trial.Item1+" at "+trial.Item2+" m");
    uint shots=ball.ShotCount;
    // Exercise the same pointer-down path used on Android, through ReadCommand.
    var control=FindObjectsByType<BasketballShootButton>(FindObjectsSortMode.None).FirstOrDefault(b=>!b.pass);yield return null;
    var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=1};
    control.OnPointerDown(pointer);yield return WaitGreen();control.OnPointerUp(pointer);
    yield return null;yield return new WaitForFixedUpdate();
    Check(ball.Held&&(ball.ActionQueued||actor.BasketballMotion.BeforeRelease),"shot gathers before release");
    float releaseDeadline=Time.time+1.4f;while(ball.ShotCount==shots&&Time.time<releaseDeadline)yield return new WaitForFixedUpdate();
    Check(!ball.Held&&ball.ShotCount==shots+1,"touch edge launches once at "+trial.Item2+" m");
    Check(ball.Body.linearVelocity.y>3&&ball.Body.linearVelocity.magnitude<ball.maxShotSpeed&&ball.Body.angularVelocity.magnitude>10,"bounded shot speed and backspin");
    Check(!ball.TryShoot(actor,yaw),"duplicate shot without possession rejected");
    var goal=hoop.TransformPoint(new Vector3(0,3.048f,0));float error=100;bool crossed=false,grace=true;float elapsed=0;var previous=ball.Body.position;
    string csv=Path.Combine(folder,trial.Item1+"-"+trial.Item2+"m.csv");File.WriteAllText(csv,"time,x,y,z,vx,vy,vz,held\n");
    while(elapsed<3){
     yield return new WaitForFixedUpdate();elapsed+=Time.fixedDeltaTime;var current=ball.Body.position;
     if(elapsed<.28f)grace&=!ball.Held;
     var velocity=ball.Body.linearVelocity;File.AppendAllText(csv,$"{elapsed:R},{current.x:R},{current.y:R},{current.z:R},{velocity.x:R},{velocity.y:R},{velocity.z:R},{ball.Held}\n");
     if(!crossed&&previous.y>=goal.y&&current.y<goal.y&&ball.Body.linearVelocity.y<0){var crossing=Vector3.Lerp(previous,current,(previous.y-goal.y)/(previous.y-current.y));error=Vector3.Distance(crossing,goal);crossed=true;}
     if(elapsed>.5f&&elapsed<.5f+Time.fixedDeltaTime*1.5f&&trial.Item2==5)Capture(trial.Item1+"-shot");
     previous=current;
    }
    Check(grace,"no immediate self pickup after release");
    Check(crossed&&error<.11f,"descending ball crosses rim opening "+trial+" error="+error.ToString("F4"));
   }
   // Retrieve the actual rebound without placing or resetting the ball.
   float catchUntil=Time.time+5;while(!ball.Held&&Time.time<catchUntil){var p=ball.Body.position;p.y=.07f;PlaceActor(actor,p+Vector3.left*.5f);yield return new WaitForSeconds(.1f);}
   Check(ball.Holder==actor,"player retrieves real rebound for another shot");
   // Physical obstruction must prevent pickup through a wall.
   PlaceActor(actor,new Vector3(0,.07f,0));ball.Place(new Vector3(0,.122f,.8f),Quaternion.identity,Vector3.zero,Vector3.zero);
   var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=8;wall.transform.position=new Vector3(0,.8f,.4f);wall.transform.localScale=new Vector3(2,1.6f,.08f);Physics.SyncTransforms();yield return new WaitForSeconds(.6f);
   Check(!ball.Held,"wall blocks proximity pickup");Destroy(wall);yield return null;yield return WaitHeld(actor);
   yield return CourtBoundaries();
   // A deliberate teleport outside still recovers possession; walking is fenced.
   PlaceActor(actor,new Vector3(11,.07f,0));yield return new WaitForSeconds(.2f);Check(!ball.Held&&Mathf.Abs(ball.transform.position.x)<1,"holder teleported outside releases and resets ball");
   ball.Place(new Vector3(11,1,0),Quaternion.identity,Vector3.zero,Vector3.zero);yield return new WaitForSeconds(1);Check(Mathf.Abs(ball.transform.position.x)<1,"loose ball outside court returns");
   ball.Place(new Vector3(0,-4,0),Quaternion.identity,Vector3.zero,Vector3.zero);yield return new WaitForSeconds(.1f);Check(ball.transform.position.y>.1f,"fallen ball returns");
   ball.Body.useGravity=false;ball.Place(new Vector3(0,2,0),Quaternion.identity,Vector3.zero,Vector3.zero);yield return new WaitForSeconds(12.2f);Check(ball.transform.position.y<.2f,"stranded unreachable ball times out");ball.Body.useGravity=true;
   PlaceActor(actor,new Vector3(0,.07f,-2));yield return WaitHeld(actor);
   app.SendMessage("Return");yield return new WaitForSeconds(.15f);Check(!ball.Held&&ball.Body.isKinematic&&!ball.TryShoot(actor,0),"leaving exploration clears and freezes ball");
   app.EnterOffline();yield return new WaitForSeconds(.5f);Check(!ball.Held&&!ball.Body.isKinematic&&!actor.BasketballFreeRoam,"new offline session starts with loose ball and court boundary");
   app.SelectSport(SportId.Football);yield return new WaitForSeconds(.3f);var stream=app.environments.GetComponent<SkySailStreaming>();while(stream.Busy)yield return null;app.Show("stadium");
   Check(!BasketballBall.Active&&!FindObjectsByType<BasketballShootButton>(FindObjectsSortMode.None).FirstOrDefault(b=>!b.pass),"leaving island removes ball and Shoot control");
   yield return stream.Prepare(SportId.Basketball);app.SelectSport(SportId.Basketball);app.EnterOffline();yield return new WaitForSeconds(.4f);ball=BasketballBall.Active;
   Check(FindObjectsByType<BasketballBall>(FindObjectsSortMode.None).Length==1&&!ball.Held,"returning island creates one fresh ball");
   Check(!BasketballBall.SolveShot(Vector3.zero,Vector3.up*3,1,1,out _)&&!BasketballBall.SolveShot(Vector3.zero,new Vector3(float.NaN,0,0),1,24,out _),"trajectory rejects impossible speed and invalid target");
  }
  IEnumerator CourtBoundaries(){
   var arena=ball.transform.parent;var limits=BasketballBall.PlayerCourtLimits;
   var wood=app.stadium.GetComponentsInChildren<MeshFilter>().Single(f=>f.name=="Court__Maple").GetComponent<Renderer>().bounds;
   Check(Mathf.Abs(wood.size.x-2*limits.x)<.01f&&Mathf.Abs(wood.size.z-2*limits.y)<.01f,"player boundaries match the authored court");
   float margin=actor.capsule.radius+actor.capsule.skinWidth;
   bool Contained(){var p=arena.InverseTransformPoint(actor.transform.TransformPoint(actor.capsule.center));return Mathf.Abs(p.x)+margin<=limits.x+.015f&&Mathf.Abs(p.z)+margin<=limits.y+.015f;}
   foreach(var direction in new[]{Vector2.right,Vector2.left,Vector2.up,Vector2.down}){
    var point=Mathf.Abs(direction.x)>0?new Vector3(direction.x*(limits.x-1.5f),.07f,0):new Vector3(3,.07f,direction.y*(limits.y-1.5f));
    PlaceActor(actor,arena.TransformPoint(point),Quaternion.LookRotation(new Vector3(direction.x,0,direction.y)).eulerAngles.y);
    ball.Place(point+actor.transform.forward*.6f+Vector3.up*.15f,Quaternion.identity,Vector3.zero,Vector3.zero);yield return WaitHeld(actor);
    DevelopmentProbe.TurnCommand=new PlayerCommand{move=direction,sprint=true};yield return new WaitForSeconds(.7f);DevelopmentProbe.TurnCommand=default;
    Check(Contained()&&ball.Holder==actor,"sprint dribble stops at court edge "+direction);
   }
   ball.autoPickup=false;ball.ResetHome();
   PlaceActor(actor,arena.TransformPoint(new Vector3(limits.x-1,.07f,-3)));
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.one,sprint=true};yield return new WaitForSeconds(.7f);DevelopmentProbe.TurnCommand=default;
   var sliding=arena.InverseTransformPoint(actor.transform.position);
   Check(Contained()&&sliding.z>-.8f,"diagonal movement slides along sideline; end="+sliding.ToString("F3"));
   foreach(float x in new[]{-1f,1f})foreach(float z in new[]{-1f,1f}){
    PlaceActor(actor,arena.TransformPoint(new Vector3(x*(limits.x-1),.07f,z*(limits.y-1))));
    DevelopmentProbe.TurnCommand=new PlayerCommand{move=new Vector2(x,z),sprint=true};yield return new WaitForSeconds(.5f);DevelopmentProbe.TurnCommand=default;
    Check(Contained(),"diagonal sprint cannot escape corner "+x+","+z);
   }
   PlaceActor(actor,arena.TransformPoint(new Vector3(limits.x-margin-.1f,.07f,2)));app.view.RequestJump();
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.right,sprint=true};float end=Time.time+1.5f,peak=0;bool bounded=true;
   while(Time.time<end){yield return null;bounded&=Contained();peak=Mathf.Max(peak,arena.InverseTransformPoint(actor.transform.position).y);}
   DevelopmentProbe.TurnCommand=default;Check(bounded&&peak>.35f&&!actor.Airborne,"jump stays inside wall and lands normally");
   actor.Simulate(new PlayerCommand{move=Vector2.right,sprint=true},.25f);Check(Contained(),"large input timestep cannot tunnel through player boundary");
   actor.inTransit=true;Check(ball.ConstrainPlayerMotion(actor,Vector3.right)==Vector3.right,"transit bypasses court boundary");actor.inTransit=false;
   PlaceActor(actor,arena.TransformPoint(new Vector3(0,.07f,-4)));
   foreach(var direction in new[]{Vector2.right,Vector2.left,Vector2.up,Vector2.down}){
    var point=new Vector3(Mathf.Abs(direction.x)>0?direction.x*(limits.x-.25f):3,.45f,direction.y*(limits.y-.25f));
    ball.ResetHome();
    ball.Place(point,Quaternion.identity,arena.TransformDirection(new Vector3(direction.x,0,direction.y)*5),Vector3.zero);yield return new WaitForSeconds(.22f);
    var p=arena.InverseTransformPoint(ball.Body.position);
    Check(Mathf.Abs(p.x)>limits.x+.35f||Mathf.Abs(p.z)>limits.y+.35f,"loose ball crosses player wall "+direction);
   }
   ball.ResetHome();ball.Body.useGravity=false;ball.Place(new Vector3(limits.x+.9f,3,0),Quaternion.identity,Vector3.zero,Vector3.zero);
   yield return new WaitForSeconds(.9f);Check(ball.transform.localPosition.x>limits.x,"airborne ball outside painted line is not prematurely recovered");ball.Body.useGravity=true;
   ball.ResetHome();ball.Place(new Vector3(limits.x+.9f,.4f,0),Quaternion.identity,Vector3.zero,Vector3.zero);
   yield return new WaitForSeconds(.25f);Check(ball.transform.localPosition.x>limits.x,"out-of-court rebound remains visible before recovery");
   yield return new WaitForSeconds(.8f);Check(Mathf.Abs(ball.transform.localPosition.x)<1,"unreachable ball on apron returns inside the player boundary");
   PlaceActor(actor,arena.TransformPoint(new Vector3(limits.x-1,.07f,0)));ball.autoPickup=true;
   ball.Place(arena.InverseTransformPoint(actor.transform.position)+Vector3.forward*.6f+Vector3.up*.15f,Quaternion.identity,Vector3.zero,Vector3.zero);yield return WaitHeld(actor);
   FindFirstObjectByType<BasketballHUD>().GetComponentInChildren<UnityEngine.UI.Button>().onClick.Invoke();yield return new WaitForSeconds(.15f);
   Check(actor.BasketballFreeRoam&&!ball.Held&&!ball.CanShoot(actor),"Free roam control releases possession and disables pickup/actions");
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.right,sprint=true};yield return new WaitForSeconds(.5f);DevelopmentProbe.TurnCommand=default;
   Check(arena.InverseTransformPoint(actor.transform.position).x>limits.x+.4f,"deliberate free roam can leave the court");Capture("free-roam-controls",true);
   FindFirstObjectByType<BasketballHUD>().GetComponentInChildren<UnityEngine.UI.Button>().onClick.Invoke();yield return null;
   Check(!actor.BasketballFreeRoam,"Play basketball control restores play mode");
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.left,sprint=true};yield return new WaitForSeconds(.6f);
   DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.right,sprint=true};yield return new WaitForSeconds(.6f);DevelopmentProbe.TurnCommand=default;
   Check(Contained(),"outside player can enter then is fenced again");Capture("dribble-at-sideline",true);
   PlaceActor(actor,SkySailMap.Port(SportId.Basketball));
   Check(SkySailWorld.Instance.CanTravel(SkySailMap.Neighbor(SportId.Basketball,1),out _),"station arrivals remain outside court boundary and can travel");
   PlaceActor(actor,new Vector3(0,.07f,-2));ball.ResetHome();yield return WaitHeld(actor);
  }
  IEnumerator Host(){
   var players=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).OrderBy(p=>p.OwnerClientId).ToArray();
   Check(players.Length==2,"two connected players");if(players.Length!=2)yield break;
   var guest=players[1].GetComponent<Athlete>();actor=players[0].GetComponent<Athlete>();
   PlaceActor(actor,new Vector3(-.5f,.07f,-1.5f));PlaceActor(guest,new Vector3(.5f,.07f,-1.5f));ball.ResetHome();yield return WaitHeld(actor);
   Check(ball.HolderId==players[0].OwnerClientId&&!ball.TryShoot(guest,0),"simultaneous pickup selects one owner; nonholder shot rejected");
   yield return new WaitForSeconds(1.2f);PlaceActor(actor,new Vector3(0,.07f,5));yield return new WaitForSeconds(.7f);
   app.view.BeginShot();yield return new WaitForSeconds(2.5f);Check(ball.IsCharging(actor)&&actor.BasketballMotion.Charging&&ball.Held,"host holds visible aiming pose through multiple meter cycles");app.view.CancelShot();yield return new WaitForSeconds(.35f);Check(!ball.IsCharging(actor)&&!actor.BasketballMotion.Charging&&ball.Held,"host cancel restores dribble without firing");app.view.BeginShot();yield return WaitGreen();app.view.EndShot();yield return new WaitForSeconds(1.1f);Check(ball.ShotCount==1&&!ball.Held,"host input launches authoritative shot");yield return new WaitForSeconds(3);
   Check(ball.Score.made==1&&ball.Score.points==3,"host three-pointer counted once");
   PlaceActor(actor,new Vector3(-5,.07f,-5));PlaceActor(guest,new Vector3(0,.07f,-2));ball.ResetHome();yield return WaitHeld(guest);
   float deadline=Time.time+7;while(ball.ShotCount<2&&Time.time<deadline)yield return null;
   Check(ball.ShotCount==2&&!ball.Held,"guest reliable owner RPC launches one host-simulated shot");
   yield return new WaitForSeconds(3);
   Check(ball.Score.made==2&&ball.Score.missed==0,"guest basket counted by host");
   PlaceActor(actor,new Vector3(0,.07f,-2));PlaceActor(guest,new Vector3(0,.07f,3),180);ball.ResetHome();yield return WaitHeld(actor);
   Check(ball.TryPass(actor,0)&&!ball.TryPass(actor,0),"host pass accepts one edge and rejects a duplicate");
   deadline=Time.time+5;while(ball.Holder!=guest&&Time.time<deadline)yield return null;
   Check(ball.PassCount==1&&ball.Holder==guest,"host chest pass is caught by guest");
   deadline=Time.time+6;while((ball.PassCount<2||ball.Holder!=actor)&&Time.time<deadline)yield return null;
   Check(ball.PassCount==2&&ball.Holder==actor,"guest reliable pass returns to host");
   PlaceActor(actor,new Vector3(0,.07f,-4));PlaceActor(guest,new Vector3(0,.07f,7),180);
   ball.Place(actor.transform.position+Vector3.forward*.6f+Vector3.up*.1f,Quaternion.identity,Vector3.zero,Vector3.zero);yield return WaitHeld(actor);
   Check(ball.TryPass(actor,0),"long chest pass starts");deadline=Time.time+3;
   while(ball.Holder!=guest&&Time.time<deadline)yield return null;
   Check(ball.PassCount==3&&ball.Holder==guest,"11 metre pass arrives and is caught before recovery timeout");
   // The guest drives into a sideline, explicitly leaves play, then disconnects.
   PlaceActor(guest,new Vector3(BasketballBall.PlayerCourtLimits.x-1.5f,.07f,0));
   bool touchedBoundary=false,sawFreeRoam=false,bounded=true;deadline=Time.time+8;
   while(NetworkManager.Singleton.ConnectedClients.Count>1&&Time.time<deadline){
    if(guest){float x=ball.transform.parent.InverseTransformPoint(guest.transform.position).x;
     if(!guest.BasketballFreeRoam){bounded&=x+guest.capsule.radius+guest.capsule.skinWidth<=BasketballBall.PlayerCourtLimits.x+.015f;touchedBoundary|=x>BasketballBall.PlayerCourtLimits.x-.8f;}
     else sawFreeRoam=true;
    }
    yield return null;
   }
   Check(touchedBoundary&&bounded,"host constrains guest movement at sideline");
   Check(sawFreeRoam,"host accepts guest's owner-only Free roam control");
   yield return new WaitForSeconds(.5f);Check(!ball.Held&&!ball.Body.isKinematic,"holder disconnect recovers loose authoritative ball");
   var task=app.rooms.SetExploring(false);while(!task.IsCompleted)yield return null;yield return new WaitForSeconds(.3f);
   Check(!ball.Held&&!ball.Simulating&&ball.Body.isKinematic,"room return freezes ball and clears possession");
   task=app.rooms.SetExploring(true);while(!task.IsCompleted)yield return null;yield return new WaitForSeconds(.4f);
   Check(!ball.Held&&ball.Simulating,"room restart resumes one loose ball");
  }
  IEnumerator Guest(){
   Check(!ball.Authority&&ball.Body.isKinematic&&!ball.Body.detectCollisions,"guest keeps kinematic replica");
   Check(!ball.TryShoot(actor,0)&&!ball.Place(Vector3.up,Quaternion.identity,Vector3.one,Vector3.one)&&!ball.SetFreeRoam(actor,true),"guest cannot directly mutate simulation or court mode");
   bool sawHost=false,sawGuest=false,sawFlight=false,sent=false,sentPass=false,sawWindup=false,sawAim=false,sawCharge=false;float maxCarryError=0;double heldSince=0;float end=Time.time+40;
   while(Time.time<end){
    if(ball.Held){
     var holder=ball.Holder;if(holder){maxCarryError=Mathf.Max(maxCarryError,Vector3.Distance(ball.transform.position,ball.CarryPosition(holder)));}
     if(ball.HolderId==0){sawAim|=ball.Holder&&ball.Holder.BasketballMotion.Charging;sawCharge|=ball.Charge.active;sawHost=true;CheckOnceCapture();}
     if(ball.CanShoot(actor)&&ball.ShotCount==1&&!sent){sawGuest=true;yield return new WaitForSeconds(.6f);app.view.BeginShot();yield return WaitGreen();app.view.EndShot();sent=true;}
     if(ball.CanShoot(actor)&&ball.PassCount==1&&!sentPass){app.view.yaw=180;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=180};app.view.RequestPass();sentPass=true;}
     foreach(var player in FindObjectsByType<Athlete>(FindObjectsSortMode.None))sawWindup|=player.BasketballMotion&&player.BasketballMotion.Busy;
     if(ball.CanShoot(actor)&&ball.ShotCount==2&&ball.PassCount==3){
      if(heldSince==0)heldSince=Time.time;
      if(Time.time-heldSince>.4f){
       DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.right,sprint=true};yield return new WaitForSeconds(1.5f);DevelopmentProbe.TurnCommand=default;
       float x=ball.transform.parent.InverseTransformPoint(actor.transform.position).x,limit=BasketballBall.PlayerCourtLimits.x;
       Check(x>limit-.8f&&x+actor.capsule.radius+actor.capsule.skinWidth<=limit+.04f&&ball.CanShoot(actor),"guest sees dribbling player stop at authoritative sideline");
       FindFirstObjectByType<BasketballHUD>().GetComponentInChildren<UnityEngine.UI.Button>().onClick.Invoke();yield return new WaitForSeconds(.25f);
       Check(actor.BasketballFreeRoam&&!ball.Held,"guest receives Free roam state and possession release");
       DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.right,sprint=true};yield return new WaitForSeconds(.6f);DevelopmentProbe.TurnCommand=default;
       Check(ball.transform.parent.InverseTransformPoint(actor.transform.position).x>limit+.3f,"guest deliberately walks out in Free roam");break;
      }
     }
    }else if(ball.ShotCount>0)sawFlight=true;
    yield return new WaitForEndOfFrame();
   }
   Check(sawAim&&sawCharge,"opponent sees replicated aiming pose and looping charge state");Check(sawHost&&sawGuest,"replicated possession identifies both holders");Check(sent&&ball.ShotCount==2&&sawFlight,"guest receives host and guest shot flights");
   Check(maxCarryError<.15f,"carried replica follows presented athlete; max error="+maxCarryError.ToString("F3"));
   Check(ball.Body.isKinematic,"guest physics stays kinematic during shots");
   Check(sentPass&&ball.PassCount==3&&sawWindup,"replicated action timelines and owner pass RPC");
   Check(ball.Score.made==2&&ball.Score.points==6&&ball.Score.missed==0,"guest receives authoritative scoring totals");
   Check(app.stadium.GetComponentsInChildren<BasketballHoop>().Any(h=>h.Hit.kind!=BasketballNetKind.None),"guest receives authoritative net contact event");
   var leaving=app.rooms.Leave();while(!leaving.IsCompleted)yield return null;
  }
  bool captured;
  void CheckOnceCapture(){if(captured)return;captured=true;Capture("guest-sees-host-carry");}
  void Capture(string name,bool includeUI=false){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
   var camera=Camera.main;var rt=RenderTexture.GetTemporary(1280,720,24);var active=RenderTexture.active;var previous=camera.targetTexture;
   var canvas=includeUI?FindFirstObjectByType<BasketballHUD>().GetComponentInParent<Canvas>():null;
   if(canvas){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();}
   camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
   image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());
   Destroy(image);camera.targetTexture=previous;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);if(canvas){canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;}
  }
 }
}
#endif
