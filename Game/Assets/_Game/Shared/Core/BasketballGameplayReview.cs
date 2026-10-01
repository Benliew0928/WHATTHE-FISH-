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
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   float end=Time.realtimeSinceStartup+80;
   while((!(app=AppRoot.Instance)||!app.Exploring||app.SelectedSport!=SportId.Basketball)&&Time.realtimeSinceStartup<end)yield return null;
   ball=BasketballBall.Active;actor=app?app.LocalAthlete:null;
   if(!ball||!actor){Check(false,"basketball and player initialized");Finish();yield break;}
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.5f);
   Check(FindFirstObjectByType<BasketballShootButton>(),"minimal Shoot control present");
   Check(Mathf.Abs(ball.arcPerMetre-.12f)<.0001f,"authored prefab shot arc imported");
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
    var control=FindFirstObjectByType<BasketballShootButton>();yield return null;
    control.OnPointerDown(new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left});
    yield return null;yield return new WaitForFixedUpdate();
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
   // Walking outside the playable court returns the ball, while the player can explore.
   PlaceActor(actor,new Vector3(11,.07f,0));yield return new WaitForSeconds(.2f);Check(!ball.Held&&Mathf.Abs(ball.transform.position.x)<1,"holder leaving court releases and resets ball");
   ball.Place(new Vector3(11,1,0),Quaternion.identity,Vector3.zero,Vector3.zero);yield return new WaitForSeconds(1);Check(Mathf.Abs(ball.transform.position.x)<1,"loose ball outside court returns");
   ball.Place(new Vector3(0,-4,0),Quaternion.identity,Vector3.zero,Vector3.zero);yield return new WaitForSeconds(.1f);Check(ball.transform.position.y>.1f,"fallen ball returns");
   ball.Body.useGravity=false;ball.Place(new Vector3(0,2,0),Quaternion.identity,Vector3.zero,Vector3.zero);yield return new WaitForSeconds(12.2f);Check(ball.transform.position.y<.2f,"stranded unreachable ball times out");ball.Body.useGravity=true;
   PlaceActor(actor,new Vector3(0,.07f,-2));yield return WaitHeld(actor);
   app.SendMessage("Return");yield return new WaitForSeconds(.15f);Check(!ball.Held&&ball.Body.isKinematic&&!ball.TryShoot(actor,0),"leaving exploration clears and freezes ball");
   app.EnterOffline();yield return new WaitForSeconds(.5f);Check(!ball.Held&&!ball.Body.isKinematic,"new offline session starts with loose ball");
   app.SelectSport(SportId.Football);yield return new WaitForSeconds(.3f);var stream=app.environments.GetComponent<SkySailStreaming>();while(stream.Busy)yield return null;app.Show("stadium");
   Check(!BasketballBall.Active&&!FindFirstObjectByType<BasketballShootButton>(),"leaving island removes ball and Shoot control");
   yield return stream.Prepare(SportId.Basketball);app.SelectSport(SportId.Basketball);app.EnterOffline();yield return new WaitForSeconds(.4f);ball=BasketballBall.Active;
   Check(FindObjectsByType<BasketballBall>(FindObjectsSortMode.None).Length==1&&!ball.Held,"returning island creates one fresh ball");
   Check(!BasketballBall.SolveShot(Vector3.zero,Vector3.up*3,1,1,out _)&&!BasketballBall.SolveShot(Vector3.zero,new Vector3(float.NaN,0,0),1,24,out _),"trajectory rejects impossible speed and invalid target");
  }
  IEnumerator Host(){
   var players=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).OrderBy(p=>p.OwnerClientId).ToArray();
   Check(players.Length==2,"two connected players");if(players.Length!=2)yield break;
   var guest=players[1].GetComponent<Athlete>();actor=players[0].GetComponent<Athlete>();
   PlaceActor(actor,new Vector3(-.5f,.07f,-1.5f));PlaceActor(guest,new Vector3(.5f,.07f,-1.5f));ball.ResetHome();yield return WaitHeld(actor);
   Check(ball.HolderId==players[0].OwnerClientId&&!ball.TryShoot(guest,0),"simultaneous pickup selects one owner; nonholder shot rejected");
   yield return new WaitForSeconds(1.2f);PlaceActor(actor,new Vector3(0,.07f,5));yield return new WaitForSeconds(.7f);
   app.view.RequestShoot();yield return new WaitForSeconds(.2f);Check(ball.ShotCount==1&&!ball.Held,"host input launches authoritative shot");yield return new WaitForSeconds(3);
   PlaceActor(actor,new Vector3(-5,.07f,-5));PlaceActor(guest,new Vector3(0,.07f,-2));ball.ResetHome();yield return WaitHeld(guest);
   float deadline=Time.time+7;while(ball.ShotCount<2&&Time.time<deadline)yield return null;
   Check(ball.ShotCount==2&&!ball.Held,"guest reliable owner RPC launches one host-simulated shot");
   yield return new WaitForSeconds(3);PlaceActor(guest,new Vector3(0,.07f,-2));ball.ResetHome();yield return WaitHeld(guest);
   // Guest deliberately disconnects while holding; no stale kinematic possession.
   deadline=Time.time+8;while(NetworkManager.Singleton.ConnectedClients.Count>1&&Time.time<deadline)yield return null;
   yield return new WaitForSeconds(.5f);Check(!ball.Held&&!ball.Body.isKinematic,"holder disconnect recovers loose authoritative ball");
   var task=app.rooms.SetExploring(false);while(!task.IsCompleted)yield return null;yield return new WaitForSeconds(.3f);
   Check(!ball.Held&&!ball.Simulating&&ball.Body.isKinematic,"room return freezes ball and clears possession");
   task=app.rooms.SetExploring(true);while(!task.IsCompleted)yield return null;yield return new WaitForSeconds(.4f);
   Check(!ball.Held&&ball.Simulating,"room restart resumes one loose ball");
  }
  IEnumerator Guest(){
   Check(!ball.Authority&&ball.Body.isKinematic&&!ball.Body.detectCollisions,"guest keeps kinematic replica");
   Check(!ball.TryShoot(actor,0)&&!ball.Place(Vector3.up,Quaternion.identity,Vector3.one,Vector3.one),"guest cannot directly mutate simulation");
   bool sawHost=false,sawGuest=false,sawFlight=false,sent=false;float maxCarryError=0;double heldSince=0;float end=Time.time+30;
   while(Time.time<end){
    if(ball.Held){
     var holder=ball.Holder;if(holder){maxCarryError=Mathf.Max(maxCarryError,Vector3.Distance(ball.transform.position,ball.CarryPosition(holder)));}
     if(ball.HolderId==0){sawHost=true;CheckOnceCapture();}
     if(ball.CanShoot(actor)&&ball.ShotCount==1&&!sent){sawGuest=true;yield return new WaitForSeconds(.6f);app.view.RequestShoot();sent=true;}
     if(ball.CanShoot(actor)&&ball.ShotCount==2){if(heldSince==0)heldSince=Time.time;if(Time.time-heldSince>.8f)break;}
    }else if(ball.ShotCount>0)sawFlight=true;
    yield return new WaitForEndOfFrame();
   }
   Check(sawHost&&sawGuest,"replicated possession identifies both holders");Check(sent&&ball.ShotCount==2&&sawFlight,"guest receives host and guest shot flights");
   Check(maxCarryError<.15f,"carried replica follows presented athlete; max error="+maxCarryError.ToString("F3"));
   Check(ball.Body.isKinematic,"guest physics stays kinematic during shots");
   Check(ball.CanShoot(actor),"guest holds ball before disconnect");var leaving=app.rooms.Leave();while(!leaving.IsCompleted)yield return null;
  }
  bool captured;
  void CheckOnceCapture(){if(captured)return;captured=true;Capture("guest-sees-host-carry");}
  void Capture(string name,bool includeUI=false){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
   var camera=Camera.main;var rt=RenderTexture.GetTemporary(1280,720,24);var active=RenderTexture.active;var previous=camera.targetTexture;
   var canvas=includeUI?FindFirstObjectByType<Canvas>():null;
   if(canvas){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();}
   camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
   image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());
   Destroy(image);camera.targetTexture=previous;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);if(canvas){canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;}
  }
 }
}
#endif
