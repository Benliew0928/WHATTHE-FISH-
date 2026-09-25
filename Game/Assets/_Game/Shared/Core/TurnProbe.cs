#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public sealed partial class DevelopmentProbe {
  public static bool TurnCommandActive;public static PlayerCommand TurnCommand;
  bool recordingTurn;int turnFrame;string turnFrames;
  void LateUpdate(){if(recordingTurn)Capture(Path.Combine(turnFrames,$"frame-{turnFrame++:D4}.png"));}
  IEnumerator TurnAudit(){
   yield return new WaitForSeconds(2);
   Application.targetFrameRate=60;QualitySettings.vSyncCount=0;
   var app=AppRoot.Instance;TurnCommandActive=true;TurnCommand=default;
   foreach(SportId sport in Enum.GetValues(typeof(SportId))){
    app.SelectSport(sport);app.EnterOffline();app.view.mode=1;app.view.yaw=-30;
    var athlete=app.LocalAthlete;var animator=athlete.GetComponentInChildren<Animator>();
    animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
    var lod=athlete.GetComponentInChildren<LODGroup>();
    yield return new WaitForSeconds(.5f);
    if(sport==SportId.Basketball&&args.Contains("-turnVideo")){
     Time.captureFramerate=30;turnFrames=Path.Combine(Path.GetDirectoryName(output),"frames");Directory.CreateDirectory(turnFrames);recordingTurn=true;
    }
    foreach(int level in new[]{0,1}){
     lod.ForceLOD(level);
     // First command turns from standing; following commands reverse a running athlete.
     foreach(float heading in new[]{180f,0f,-150f,30f}){
      TurnCommand=new PlayerCommand{move=Vector2.up,heading=heading,sprint=true};
      float until=Time.time+1.0f,turnTravel=0;bool sawTurn=false,sawRun=false,validClips=true;
      var previous=athlete.transform.position;var phase=athlete.Phase;
      while(Time.time<until){
       yield return null;
       bool turning=athlete.Phase==LocomotionPhase.Turning;sawTurn|=turning;
       if(turning&&phase==LocomotionPhase.Turning){var delta=athlete.transform.position-previous;delta.y=0;turnTravel+=delta.magnitude;}
       sawRun|=sawTurn&&animator.GetCurrentAnimatorStateInfo(0).IsName("Run")&&athlete.speed>6;
       validClips&=animator.GetCurrentAnimatorClipInfo(0).Length>0;
       previous=athlete.transform.position;phase=athlete.Phase;
      }
      Check(sawTurn&&sawRun&&validClips&&turnTravel>.05f,$"TURN_{sport}_LOD{level}_heading{heading} movingTurnTravel={turnTravel:F4}");
     }
     float path=0;int stalled=0,wrongDirection=0;bool running=true;
     for(int i=0;i<90;i++){
      var desired=i%2==0?Vector3.forward:Vector3.back;
      TurnCommand=new PlayerCommand{move=i%2==0?Vector2.up:Vector2.down,heading=0,sprint=true};
      var position=athlete.transform.position;yield return null;
      var delta=athlete.transform.position-position;delta.y=0;path+=delta.magnitude;
      if(delta.magnitude<.00001f)stalled++;
      if(Vector3.Dot(delta,desired)<=0)wrongDirection++;
      running&=animator.GetCurrentAnimatorStateInfo(0).IsName("Run");
     }
     Check(stalled==0&&wrongDirection==0&&path>2&&running,$"RAPID_STICK_{sport}_LOD{level} stalled={stalled} wrongDirection={wrongDirection} path={path:F3}");
     TurnCommand=default;yield return new WaitForSeconds(.55f);
     Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),$"TURN_STOP_{sport}_LOD{level}");
    }
    recordingTurn=false;Time.captureFramerate=0;lod.ForceLOD(-1);
    app.view.mode=0;yield return new WaitForSeconds(.2f);
    Check(athlete.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).All(r=>!r.enabled),"TURN_FIRST_PERSON_"+sport);
   }
   TurnCommandActive=false;Record("TURN_GAMEPLAY_COMPLETE");
  }
  IEnumerator NetworkTurnAudit(){
   while(!AppRoot.Instance||!AppRoot.Instance.rooms.Connected)yield return null;
   var counts=new Dictionary<ulong,int>();var previous=new Dictionary<ulong,LocomotionPhase>();
   var travelled=new Dictionary<ulong,float>();var positions=new Dictionary<ulong,Vector3>();bool validClips=true;
   while(elapsed<float.Parse(Value("-returnAt","48"))){
    foreach(var network in FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None)){
     var athlete=network.GetComponent<Athlete>();var animator=athlete.GetComponentInChildren<Animator>();animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
     var id=network.OwnerClientId;
     if(athlete.Phase==LocomotionPhase.Turning){
      if(!previous.TryGetValue(id,out var before)||before!=LocomotionPhase.Turning)counts[id]=counts.GetValueOrDefault(id)+1;
      if(positions.TryGetValue(id,out var position)){var delta=athlete.transform.position-position;delta.y=0;travelled[id]=travelled.GetValueOrDefault(id)+delta.magnitude;}
      validClips&=animator.GetCurrentAnimatorClipInfo(0).Length>0;
     }
     previous[id]=athlete.Phase;positions[id]=athlete.transform.position;
    }
    yield return null;
   }
   Check(counts.Count==expected&&counts.All(c=>c.Value>=3),"NETWORK_TURNS_PER_ACTOR "+string.Join(",",counts.Select(c=>c.Key+":"+c.Value)));
   Check(travelled.Count==expected&&travelled.All(c=>c.Value>.3f)&&validClips,"NETWORK_MOVES_DURING_TURNS "+string.Join(",",travelled.Select(c=>c.Key+":"+c.Value)));
   Record("NETWORK_TURN_AUDIT_COMPLETE");
  }
 }
}
#endif
