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
 public sealed class BasketballStealReview:MonoBehaviour {
  string folder,shared;bool failed;AppRoot app;BasketballBall ball;Athlete actor;int frame;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
   var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-basketballStealReview");if(i<0||i+1>=args.Length)return;
   new GameObject("Basketball steal review").AddComponent<BasketballStealReview>().folder=args[i+1];
  }
  void Check(bool ok,string text){failed|=!ok;File.AppendAllText(Path.Combine(folder,"results.txt"),(ok?"PASS ":"FAIL ")+text+"\n");}
  void Signal(string name)=>File.WriteAllText(Path.Combine(shared,name),"ready");
  bool Signalled(string name)=>File.Exists(Path.Combine(shared,name));
  IEnumerator Await(string name,float seconds=12){float end=Time.realtimeSinceStartup+seconds;while(!Signalled(name)&&Time.realtimeSinceStartup<end)yield return null;Check(Signalled(name),"peer checkpoint "+name);}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);shared=Directory.GetParent(folder).FullName;File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   float end=Time.realtimeSinceStartup+80;while((!(app=AppRoot.Instance)||!app.Exploring||app.SelectedSport!=SportId.Basketball)&&Time.realtimeSinceStartup<end)yield return null;
   ball=BasketballBall.Active;actor=app?app.LocalAthlete:null;
   if(!ball||!actor){Check(false,"initialized court");Finish();yield break;}
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;yield return new WaitForSeconds(.6f);
   Check(FindFirstObjectByType<BasketballStealButton>(),"dedicated Steal button present");
   if(app.rooms.Connected){if(app.rooms.Host)yield return Host();else yield return Guest();}else{Rules();yield return Offline();}
   Finish();
  }
  void Rules(){
   var victim=new Vector3(0,.07f,0);var attacker=new Vector3(.90f,.07f,.32f);var point=new Vector3(.34f,.38f,.32f);var facing=Quaternion.Euler(0,270,0);
   bool Contact(Vector3 a,Quaternion r,Vector3 p,float phase,bool secure=false)=>BasketballStealRules.Contact(a,r,victim,p,false,phase,secure,out _);
   Check(Contact(attacker,facing,point,.4f),"exposed ball-side swipe contacts at close range");
   Check(!Contact(attacker+Vector3.right*1.5f,facing,point,.4f),"out-of-range swipe misses");
   Check(!Contact(attacker,Quaternion.Euler(0,90,0),point,.4f),"facing away cannot steal");
   Check(!Contact(new Vector3(-.9f,.07f,.32f),Quaternion.Euler(0,90,0),point,.4f),"carrier shields far-side ball");
   Check(!Contact(attacker,facing,point+Vector3.up*1.8f,.4f),"ball above hand reach is protected");
   Check(!Contact(attacker,facing,point,.4f,true),"secure two-handed grip requires better contact");
   Check(!Contact(attacker,facing,point,0),"same distance at secure dribble phase misses");
   Check(!Contact(attacker,facing,new Vector3(float.NaN,0,0),.4f),"nonfinite geometry rejected");
   var shift=new Vector3(15,0,-30);var turn=Quaternion.Euler(0,73,0);
   Check(BasketballStealRules.Contact(turn*attacker+shift,turn*facing,turn*victim+shift,turn*point+shift,false,.4f,false,out _),"contact invariant under court rotation and translation");
   Vector3 Flight(Vector3 v,Vector3 d,float phase,bool left)=>BasketballStealRules.Deflection(attacker,point,v,d,phase,1.65f,false,left,.8f,.31f);
   var still=Flight(Vector3.zero,Vector3.zero,.3f,false);var moving=Flight(Vector3.forward*4,Vector3.zero,.3f,false);
   Check((Vector3.ProjectOnPlane(moving-still,Vector3.up)-Vector3.forward*2.4f).magnitude<.001f,"carrier momentum contributes to outgoing ball");
   Check(Vector3.Distance(still,Flight(Vector3.zero,Vector3.forward*4,.3f,false))>.9f,"defender momentum changes deflection");
   Check(Mathf.Sign(still.z)!=Mathf.Sign(Flight(Vector3.zero,Vector3.zero,.3f,true).z),"opposite swipe hands deflect across opposite sides");
   Check(Flight(Vector3.zero,Vector3.zero,.7f,false).y>still.y,"rising dribble lifts farther than downward dribble");
   Check(Flight(Vector3.one*100,Vector3.one*100,.5f,false).magnitude<8,"deflection speed is bounded");
   Check(default(BasketballMotionState).Equals(default),"default replicated motion equals itself");
  }
  void Place(Athlete who,Vector3 point,float heading=0){
   who.capsule.enabled=false;who.transform.SetPositionAndRotation(point,Quaternion.Euler(0,heading,0));var net=who.GetComponent<NetworkTransform>();
   if(net&&net.IsSpawned)net.Teleport(point,who.transform.rotation,Vector3.one);
   who.capsule.enabled=true;who.ResetLocomotion();Physics.SyncTransforms();
  }
  IEnumerator Offline(){
   ball.autoPickup=false;Place(actor,new Vector3(3,.07f,-3),270);DevelopmentProbe.TurnCommand=new PlayerCommand{heading=270};
   var control=FindFirstObjectByType<BasketballStealButton>();yield return null;
   var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,pointerId=41};
   uint before=actor.BasketballMotion.State.sequence;control.OnPointerDown(pointer);yield return new WaitForSeconds(1.8f);
   Check(actor.BasketballMotion.State.sequence>=before+3&&actor.BasketballMotion.State.sequence<=before+4,"hold repeats swipes at bounded cadence");
   Check(!ball.Held&&ball.StealCount==0,"empty swipes do not create possession or steals");
   control.OnPointerUp(new PointerEventData(EventSystem.current){pointerId=42});Check(app.view.StealHeld,"other finger cannot release steal hold");
   control.OnPointerUp(pointer);before=actor.BasketballMotion.State.sequence;yield return new WaitForSeconds(.85f);
   Check(!app.view.StealHeld&&actor.BasketballMotion.State.sequence==before,"release stops repeating");
   control.OnPointerDown(pointer);yield return null;control.OnPointerExit(pointer);yield return new WaitForSeconds(.8f);
   before=actor.BasketballMotion.State.sequence;yield return new WaitForSeconds(.8f);Check(!app.view.StealHeld&&actor.BasketballMotion.State.sequence==before,"drag off cancels repeat");
   Check(ball.TrySteal(actor,270),"idle grounded swipe accepted");int accepted=0;for(int i=0;i<100;i++)if(ball.TrySteal(actor,270))accepted++;
   Check(accepted==0,"100 duplicate requests cannot bypass recovery");Check(!actor.CanRequestJump,"swipe contact cannot be cancelled into a jump");
   yield return new WaitForSeconds(.8f);Check(!actor.BasketballMotion.Busy&&actor.CanRequestJump,"swipe recovers to locomotion and jump");
   Check(!ball.TrySteal(actor,float.NaN)&&!ball.TrySteal(actor,float.PositiveInfinity),"invalid headings rejected");
   actor.RequestJump();yield return new WaitForFixedUpdate();yield return null;Check(!ball.TrySteal(actor,270),"jump preparation rejects steal");yield return new WaitForSeconds(1.5f);
   ball.SetFreeRoam(actor,true);Check(!ball.TrySteal(actor,270)&&!app.view.BeginSteal(),"free roam disables stealing");ball.SetFreeRoam(actor,false);
   app.view.BeginSteal();app.view.SendMessage("OnApplicationFocus",false);Check(!app.view.StealHeld,"focus loss clears held input");
   control.OnPointerDown(pointer);control.enabled=false;Check(!app.view.StealHeld,"disabled control cancels touch hold");control.enabled=true;
   app.view.BeginSteal();app.SendMessage("Return");yield return new WaitForSeconds(.2f);Check(!app.view.StealHeld&&!ball.TrySteal(actor,270),"room return cancels input and attempts");
  }
  IEnumerator Possess(Athlete victim,Athlete defender,Vector3 defenderPoint,float heading){
   app.view.ClearMatchInput();Place(defender,new Vector3(4,.07f,-4));Place(victim,new Vector3(0,.07f,0));
   ball.autoPickup=true;ball.Place(new Vector3(0,.23f,.6f),Quaternion.identity,Vector3.zero,Vector3.zero);
   float end=Time.time+3;while(ball.Holder!=victim&&Time.time<end)yield return null;
   Check(ball.Holder==victim,"fixture obtains real proximity possession");Place(defender,defenderPoint,heading);yield return new WaitForSeconds(.15f);
  }
  IEnumerator Swipes(float seconds,float heading){
   DevelopmentProbe.TurnCommand=new PlayerCommand{heading=heading};app.view.BeginSteal();float end=Time.time+seconds;
   while(Time.time<end){yield return new WaitForEndOfFrame();Capture();}
   app.view.EndSteal();DevelopmentProbe.TurnCommand=default;
  }
  IEnumerator CaptureWindow(float seconds){float end=Time.time+seconds;while(Time.time<end){yield return new WaitForEndOfFrame();Capture();}}
  IEnumerator Host(){
   var players=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).OrderBy(p=>p.OwnerClientId).ToArray();Check(players.Length==2,"two network athletes");if(players.Length!=2)yield break;
   var guest=players[1].GetComponent<Athlete>();actor=players[0].GetComponent<Athlete>();yield return Await("guest-ready");
   yield return Possess(guest,actor,new Vector3(2.8f,.07f,.32f),270);uint before=ball.StealCount;
   yield return Swipes(1.7f,270);Check(ball.Holder==guest&&ball.StealCount==before,"distant repeated swipes leave possession");
   yield return Possess(guest,actor,new Vector3(-.9f,.07f,0),90);yield return Swipes(1.7f,90);
   Check(ball.Holder==guest&&ball.StealCount==before,"body shields far-side dribble through repeated attempts");
   yield return Possess(guest,actor,new Vector3(.9f,.07f,.32f),270);
   var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=8;wall.transform.position=new Vector3(.62f,.7f,.32f);wall.transform.localScale=new Vector3(.04f,1.4f,1.5f);Physics.SyncTransforms();
   yield return Swipes(1.7f,270);Check(ball.Holder==guest&&ball.StealCount==before,"physical wall blocks reachable swipe");Destroy(wall);yield return null;
   yield return Possess(guest,actor,new Vector3(.9f,.07f,.32f),270);
   Check(ball.TrySteal(actor,270),"swipe begins against current carrier");Place(guest,new Vector3(-2,.07f,0));yield return new WaitForSeconds(.5f);
   Check(ball.Holder==guest&&ball.StealCount==before,"carrier can move out of range during windup");
   yield return Possess(guest,actor,new Vector3(.9f,.07f,.32f),270);
   Check(ball.TrySteal(actor,270),"windup begins before possession reset");ball.ResetHome();yield return new WaitForSeconds(.5f);
   Check(ball.StealCount==before&&actor.BasketballMotion.Action!=BasketballAction.Steal,"possession reset cancels pending swipe");
   yield return Possess(guest,actor,new Vector3(.9f,.07f,.32f),270);
   Check(!ball.TrySteal(guest,0),"holder cannot steal own ball");Check(ball.BeginShotCharge(guest),"victim begins charging before strip");
   DevelopmentProbe.TurnCommand=new PlayerCommand{heading=270};app.view.BeginSteal();float end=Time.time+5;bool sawSwipe=false;
   while(ball.StealCount==before&&Time.time<end){sawSwipe|=actor.BasketballMotion.Action==BasketballAction.Steal;yield return new WaitForEndOfFrame();Capture();}
   app.view.EndSteal();
   Check(sawSwipe&&ball.StealCount==before+1,"host input succeeds from exposed side");
   Check(!ball.Held&&!ball.Body.isKinematic&&ball.Body.detectCollisions,"success releases a dynamic unowned ball");
   Check(guest.BasketballMotion.Action==BasketballAction.Stripped&&actor.BasketballMotion.Action==BasketballAction.Steal,"both players animate success");
   Check(ball.Body.linearVelocity.magnitude>1&&ball.Body.linearVelocity.magnitude<8&&ball.Body.angularVelocity.magnitude>1,"physical deflection and spin instead of ownership transfer");
   var sphere=ball.GetComponent<SphereCollider>();Check(!Physics.GetIgnoreCollision(sphere,guest.capsule)&&!Physics.GetIgnoreCollision(sphere,actor.capsule),"separated bodies retain real deflection collisions");
   Check(!ball.ReleaseShotCharge(guest,0)&&!ball.ActionQueued,"successful strip cancels charge and queued shot");
   uint shots=ball.ShotCount;yield return CaptureWindow(.20f);Check(!ball.Held&&ball.ShotCount==shots&&ball.Score.attempts==0,"pickup grace and scoring preserved");
   yield return Swipes(.7f,270);Check(ball.StealCount==before+1,"one possession cannot be stripped twice");
   Place(actor,new Vector3(4,.07f,-4));Place(guest,new Vector3(-4,.07f,-4));yield return new WaitForSeconds(.8f);
   var rebound=ball.Body.position;rebound.y=.07f;Place(guest,rebound+Vector3.left*.3f);end=Time.time+3;while(ball.Holder!=guest&&Time.time<end)yield return null;
   Check(ball.Holder==guest,"stripped player can win loose ball back by proximity");
   yield return Possess(actor,guest,new Vector3(.9f,.07f,.32f),270);Signal("guest-go");before=ball.StealCount;end=Time.time+8;
   while(ball.StealCount==before&&Time.time<end){yield return new WaitForEndOfFrame();Capture();}
   Check(ball.StealCount==before+1&&!ball.Held,"guest owner input produces authoritative loose ball");
   Check(actor.BasketballMotion.Action==BasketballAction.Stripped,"host receives victim reaction to guest swipe");
   yield return CaptureWindow(.8f);
   yield return Await("guest-done");
   app.view.ClearMatchInput();var task=app.rooms.SetExploring(false);while(!task.IsCompleted)yield return null;yield return new WaitForSeconds(.4f);
   Check(!ball.Held&&!ball.Simulating&&!ball.TrySteal(actor,0),"session exit clears authoritative attempts");
  }
  IEnumerator Guest(){
   Check(!ball.Authority&&!ball.TrySteal(actor,270),"guest cannot directly mutate authoritative steals");Signal("guest-ready");
   bool sawSwipe=false,sawReaction=false,sawLoose=false;float end=Time.time+35;
   while(!Signalled("guest-go")&&Time.time<end){
    foreach(var player in Athlete.Active){sawSwipe|=player.BasketballMotion.Action==BasketballAction.Steal;sawReaction|=player.BasketballMotion.Action==BasketballAction.Stripped;}
    sawLoose|=ball.StealCount>0&&!ball.Held;yield return null;
   }
   Check(Signalled("guest-go")&&sawSwipe&&sawReaction&&sawLoose,"guest observes host swipes, victim reaction and loose-ball replication");
   uint before=ball.StealCount;DevelopmentProbe.TurnCommand=new PlayerCommand{heading=270};app.view.BeginSteal();end=Time.time+7;
   while(ball.StealCount==before&&Time.time<end)yield return null;app.view.EndSteal();
   Check(ball.StealCount==before+1&&!ball.Held,"guest button repeats through owner RPC until success");
   Check(ball.Body.isKinematic&&!ball.Body.detectCollisions,"guest remains a noncolliding kinematic replica");
   yield return new WaitForSeconds(.8f);Check(!actor.BasketballMotion.Busy,"replicated swipe returns to idle");Signal("guest-done");
  }
  void Capture(){
   if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;frame++;
   var camera=Camera.main;var position=camera.transform.position;var rotation=camera.transform.rotation;var fov=camera.fieldOfView;
   camera.transform.position=new Vector3(3.2f,2.05f,4.25f);camera.transform.LookAt(new Vector3(.4f,.83f,.25f));camera.fieldOfView=35;
   var rt=RenderTexture.GetTemporary(960,720,24);var old=RenderTexture.active;var oldTarget=camera.targetTexture;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
   var image=new Texture2D(960,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,960,720),0,0);image.Apply();
   var frames=Path.Combine(folder,"frames");Directory.CreateDirectory(frames);File.WriteAllBytes(Path.Combine(frames,frame.ToString("D4")+".png"),image.EncodeToPNG());Destroy(image);
   camera.targetTexture=oldTarget;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);camera.transform.SetPositionAndRotation(position,rotation);camera.fieldOfView=fov;
   var motion=actor.BasketballMotion;var palm=motion.State.leftHand?motion.LeftPalm:motion.RightPalm;
   File.AppendAllText(Path.Combine(folder,"frames.csv"),$"{frame},{ball.StealCount},{ball.Held},{motion.Action},{motion.Elapsed:F3},{motion.MaximumReachError:F3},{Vector3.Distance(palm,motion.State.contact):F3},{ball.transform.position.y:F3}\n");
  }
  void Finish(){DevelopmentProbe.TurnCommand=default;app?.view.ClearMatchInput();File.AppendAllText(Path.Combine(folder,"results.txt"),"BASKETBALL_STEAL_COMPLETE success="+!failed+"\n");}
 }
}
#endif
