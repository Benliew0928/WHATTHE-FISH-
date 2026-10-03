#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WhatTheFish {
 public sealed partial class DevelopmentProbe {
  IEnumerator JumpMotionReview(){
   yield return new WaitForSeconds(2);var app=AppRoot.Instance;app.EnterOffline();TurnCommandActive=true;TurnCommand=default;
   yield return new WaitForSeconds(.6f);var actor=app.LocalAthlete;var origin=actor.transform.position;
   string directory=Value("-jumpReview","JumpReview");
   bool momentum=args.Contains("-momentumReview");
   foreach(string view in momentum?new[]{"forward-coast","diagonal-coast","redirect"}:new[]{"side","front","steering"}){
    float heading=view=="diagonal-coast"?45:0;
    PlaceAthlete(actor,origin,heading);TurnCommand=default;app.view.mode=1;app.view.yaw=momentum?heading+90:view=="side"?90:155;app.view.pitch=8;
    yield return new WaitForSeconds(.4f);
    Time.captureFramerate=60;turnFrames=Path.Combine(directory,view);Directory.CreateDirectory(turnFrames);turnFrame=0;recordingTurn=true;
    for(int frame=0;frame<(momentum?180:120);frame++){
     if(momentum){
      if(frame==36)app.view.RequestJump();
      Vector2 move=frame>=12&&frame<43?Vector2.up:view=="redirect"&&frame>=60&&frame<82?Vector2.left:Vector2.zero;
      TurnCommand=new PlayerCommand{move=move,heading=heading,sprint=move.sqrMagnitude>0};
     }else{
      if(frame==20)app.view.RequestJump();
      if(view=="steering")TurnCommand=new PlayerCommand{move=frame<20?Vector2.zero:frame<43?Vector2.up:frame<69?Vector2.down:Vector2.zero,sprint=true};
     }
     yield return null;
    }
    recordingTurn=false;Time.captureFramerate=0;
   }
   TurnCommand=default;TurnCommandActive=false;Record("JUMP_MOTION_REVIEW_COMPLETE");
  }
  IEnumerator JumpAudit(){
   yield return new WaitForSeconds(2);var app=AppRoot.Instance;app.EnterOffline();app.view.mode=1;app.view.yaw=145;app.view.pitch=12;
   TurnCommandActive=true;TurnCommand=default;yield return new WaitForSeconds(.5f);
   var actor=app.LocalAthlete;var origin=actor.transform.position;
   var button=FindFirstObjectByType<JumpButton>();Check(button&&button.button.interactable,"JUMP_BUTTON_AVAILABLE");
   Check(FindFirstObjectByType<TackleButton>().label.text=="Tackle [E]","TACKLE_E_LABEL");
   foreach(int fps in new[]{20,30,60,120}){
    var clone=Instantiate(app.athletePrefab).GetComponent<Athlete>();clone.Setup();PlaceAthlete(clone,origin+Vector3.right*8,0);
    for(int i=0;i<10;i++)clone.Simulate(default,1f/fps);
    float floor=clone.transform.position.y,peak=floor;bool tookOff=false,landed=false,reverse=false;uint sequence=0;
    for(int i=0;i<fps*2;i++){
     var before=clone.transform.position;
     clone.Simulate(new PlayerCommand{jump=i==0||i==fps/5,move=i<fps/4?Vector2.up:Vector2.down,sprint=true},1f/fps);
     peak=Mathf.Max(peak,clone.transform.position.y);
     if(i==0){Check(clone.LoadingJump&&!clone.TryTackle(),"JUMP_IMMEDIATE_LOAD_REJECTS_TACKLE_"+fps);}
     if(!tookOff&&clone.Airborne){tookOff=clone.transform.position.y>floor;sequence=clone.Jump.Sequence;}
     if(i>=fps/4&&i<=fps/4+Mathf.CeilToInt(fps*.25f))reverse|=clone.transform.position.z<before.z-.005f;
     if(i==fps/3)Check(clone.Jump.Sequence==sequence,"JUMP_NO_DOUBLE_"+fps);
     landed|=tookOff&&!clone.Airborne;
    }
    Check(tookOff&&landed&&Mathf.Abs(peak-floor-JumpMotor.Height)<.04f,"JUMP_HEIGHT_AND_LANDING_"+fps+" height="+(peak-floor));
    Check(reverse&&clone.speed>6.8f,"JUMP_RESPONSIVE_AIR_REVERSAL_"+fps);
    clone.capsule.enabled=false;Destroy(clone.gameObject);
   }
   MomentumAudit(origin);
   // A low solid ceiling cancels ascent; a wall never permits phasing.
   var roof=GameObject.CreatePrimitive(PrimitiveType.Cube);roof.layer=8;roof.transform.position=origin+Vector3.up*2.25f;roof.transform.localScale=new Vector3(4,.2f,4);Physics.SyncTransforms();
   PlaceAthlete(actor,origin,0);app.view.RequestJump();float max=origin.y;bool ceiling=false;float until=Time.time+1;
   while(Time.time<until){max=Mathf.Max(max,actor.transform.position.y);ceiling|=actor.Airborne&&actor.Jump.Velocity<=0;yield return null;}
   Check(ceiling&&max-origin.y<.55f&&!actor.Airborne,"JUMP_CEILING_CANCEL_AND_LAND");Destroy(roof);yield return null;
   var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=8;wall.transform.position=origin+Vector3.forward*1.2f+Vector3.up;wall.transform.localScale=new Vector3(4,4,.15f);Physics.SyncTransforms();
   PlaceAthlete(actor,origin,0);TurnCommand=new PlayerCommand{move=Vector2.up,sprint=true};app.view.RequestJump();yield return new WaitForSeconds(1);
   Check(actor.transform.position.z<origin.z+.95f&&!actor.Airborne,"JUMP_WALL_BLOCKS_TRAVEL");TurnCommand=default;
   yield return new WaitForSeconds(.2f);Check(actor.Motor.Velocity.sqrMagnitude<.001f,"JUMP_WALL_CLEARS_MOMENTUM");Destroy(wall);yield return null;
   foreach(int lod in new[]{0,1}){
    PlaceAthlete(actor,origin,0);actor.GetComponentInChildren<LODGroup>().ForceLOD(lod);yield return new WaitForSeconds(.2f);
    var animator=actor.GetComponentInChildren<Animator>();int layer=animator.GetLayerIndex("Jump");
    button.OnPointerDown(new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left});
    yield return new WaitForSeconds(.20f);Check(actor.Airborne&&animator.GetLayerWeight(layer)>.95f&&animator.GetCurrentAnimatorClipInfo(layer).Any(c=>c.clip.name=="Jump"),"JUMP_POSE_LOD"+lod);
    Capture(Path.ChangeExtension(output,"jump-lod"+lod+".png"));
    TurnCommand=new PlayerCommand{move=Vector2.right,sprint=true};var before=actor.transform.position;yield return null;
    Check(actor.transform.position.x>before.x,"JUMP_STEER_LOD"+lod);
    yield return new WaitForSeconds(.55f);Capture(Path.ChangeExtension(output,"landing-lod"+lod+".png"));
    yield return new WaitForSeconds(.35f);Check(!actor.Airborne&&animator.GetLayerWeight(layer)<.01f,"JUMP_BLEND_TO_RUN_LOD"+lod);TurnCommand=default;
   }
   actor.GetComponentInChildren<LODGroup>().ForceLOD(-1);
   PlaceAthlete(actor,origin,0);actor.inTransit=true;app.view.RequestJump();actor.inTransit=false;yield return new WaitForSeconds(.2f);Check(!actor.Airborne,"JUMP_TRANSIT_INPUT_DISCARDED");
   actor.TryTackle();app.view.RequestJump();yield return new WaitForSeconds(.2f);Check(!actor.Airborne,"JUMP_TACKLE_INPUT_DISCARDED");yield return new WaitForSeconds(.6f);
   foreach(var sport in new[]{SportId.Basketball,SportId.Golf,SportId.Fishing}){
    app.SelectSport(sport);app.EnterOffline();float deadline=Time.time+35;
    while((app.SelectedSport!=sport||!app.view.active)&&Time.time<deadline)yield return null;
    yield return new WaitForSeconds(.5f);button=FindFirstObjectByType<JumpButton>();Check(button&&app.SelectedSport==sport,"JUMP_BUTTON_"+sport);
    app.view.RequestJump();yield return new WaitForSeconds(.2f);Check(actor.Airborne,"JUMP_ON_"+sport);yield return new WaitForSeconds(.8f);
   }
   TurnCommandActive=false;Record("JUMP_GAMEPLAY_COMPLETE");
  }
  void MomentumAudit(Vector3 origin){
   var clone=Instantiate(AppRoot.Instance.athletePrefab).GetComponent<Athlete>();clone.Setup();
   foreach(int fps in new[]{20,30,60,120})foreach(float heading in new[]{0f,45f,90f,135f,180f,225f,270f,315f})foreach(float release in new[]{0f,.2f,.5f}){
    PlaceAthlete(clone,origin+Vector3.right*8,heading);float dt=1f/fps;
    var run=new PlayerCommand{move=Vector2.up,heading=heading,sprint=true};
    for(int i=0;i<Mathf.CeilToInt(fps*.3f);i++)clone.Simulate(run,dt);
    var direction=Quaternion.Euler(0,heading,0)*Vector3.forward;Vector3 releasePosition=default,landingPosition=default;bool released=false,tookOff=false,landed=false;float launchSpeed=0;
    for(int i=0;i<fps*2;i++){
     var command=run;command.jump=i==0;
     if(i*dt>=release){if(!released){released=true;releasePosition=clone.transform.position;}command.move=Vector2.zero;command.sprint=false;command.heading=heading+137;}
     clone.Simulate(command,dt);
     if(!tookOff&&clone.Airborne){tookOff=true;launchSpeed=clone.Motor.Velocity.magnitude;}
     if(tookOff&&!clone.Airborne&&!landed){landed=true;landingPosition=clone.transform.position;}
    }
    var coast=landingPosition-releasePosition;coast.y=0;
    Check(tookOff&&landed&&launchSpeed>5.5f&&Vector3.Dot(coast,direction)>.7f&&Vector3.Cross(coast,direction).magnitude<.04f&&clone.Motor.Velocity.sqrMagnitude<.001f,
     $"MOMENTUM_{fps}FPS_{heading}DEG_RELEASE{release:F1} coast={Vector3.Dot(coast,direction):F3} launch={launchSpeed:F3}");
   }
   PlaceAthlete(clone,origin+Vector3.right*8,0);clone.Simulate(new PlayerCommand{jump=true},.1f);
   var still=clone.transform.position;for(int i=0;i<60;i++)clone.Simulate(default,1f/60);
   var drift=clone.transform.position-still;drift.y=0;Check(drift.sqrMagnitude<.0001f,"MOMENTUM_STANDING_JUMP_NO_DRIFT");
   clone.capsule.enabled=false;Destroy(clone.gameObject);
  }
  IEnumerator NetworkJumpAudit(){
   TurnCommandActive=true;TurnCommand=default;
   while(!AppRoot.Instance||!AppRoot.Instance.Exploring)yield return null;
   var app=AppRoot.Instance;float start=Time.time;bool sent=false;var airborne=new HashSet<ulong>();var landed=new HashSet<ulong>();var reversed=new HashSet<ulong>();var coasted=new HashSet<ulong>();var last=new Dictionary<ulong,Vector3>();
   while(Time.time-start<6){
    float time=Time.time-start;float launch=app.rooms.Host?1:3;
    if(time>launch&&!sent){app.view.RequestJump();sent=true;}
    bool coasting=time>=launch+.16f&&time<launch+.36f;
    TurnCommand=new PlayerCommand{move=time<launch?Vector2.zero:time<launch+.16f?Vector2.up:coasting?Vector2.zero:time<launch+.70f?Vector2.down:Vector2.zero,sprint=!coasting,heading=coasting?137:0};
    foreach(var player in FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None)){
     var a=player.GetComponent<Athlete>();var id=player.OwnerClientId;
     if(a.Airborne){airborne.Add(id);if(last.TryGetValue(id,out var p)){
      if(a.transform.position.z<p.z-.005f)reversed.Add(id);
      float since=time-(id==Unity.Netcode.NetworkManager.ServerClientId?1:3);
      if(since>.23f&&since<.35f&&a.transform.position.z>p.z+.005f)coasted.Add(id);
     }}
     else if(airborne.Contains(id))landed.Add(id);
     last[id]=a.transform.position;
    }
    yield return null;
   }
   Check(airborne.Count==expected&&landed.Count==expected,"NETWORK_BOTH_JUMP_AND_LAND");
   Check(reversed.Count==expected,"NETWORK_AIR_STEERING");
   Check(coasted.Count==expected,"NETWORK_RELEASE_CARRIES_MOMENTUM");
   TurnCommand=default;TurnCommandActive=false;Record("NETWORK_JUMP_COMPLETE");
  }
 }
}
#endif
