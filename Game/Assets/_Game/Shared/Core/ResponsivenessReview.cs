#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace WhatTheFish {
 // Real local peers; the Python harness adds delay/loss to actual UDP datagrams.
 public sealed class ResponsivenessReview:MonoBehaviour {
  string folder;bool failed;AppRoot app;Athlete actor;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-responsivenessReview");if(i>=0&&i+1<a.Length)new GameObject("Responsiveness review").AddComponent<ResponsivenessReview>().folder=a[i+1];}
  void Check(bool value,string label){failed|=!value;File.AppendAllText(Path.Combine(folder,"results.txt"),(value?"PASS ":"FAIL ")+label+"\n");}
  IEnumerator Start(){
   Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"results.txt"),"");
   // Override the generic probe's opening walk, including during room entry.
   DevelopmentProbe.TurnCommandActive=true;DevelopmentProbe.TurnCommand=default;
   float deadline=Time.realtimeSinceStartup+130;
   while(!DevelopmentProbe.ReadyToReviewSport&&Time.realtimeSinceStartup<deadline)yield return null;
   Check(DevelopmentProbe.ReadyToReviewSport,"room entry completed");if(!DevelopmentProbe.ReadyToReviewSport){Finish();yield break;}
   app=AppRoot.Instance;actor=app.LocalAthlete;var network=actor.GetComponent<NetworkAthlete>();
   if(Array.IndexOf(Environment.GetCommandLineArgs(),"-capacityLatency")>=0)Application.targetFrameRate=30;
   Check(network&&network.IsSpawned&&network.IsOwner,"measuring owned network athlete");
   Check(NetworkResponsiveness.Newer(1,uint.MaxValue)&&!NetworkResponsiveness.Newer(4,5)&&!NetworkResponsiveness.Newer(5,5),"reordered and duplicate inputs rejected across wrap");
   if(network.IsServer){
    var ball=FootballBall.Instance;int lane=0;
    foreach(var player in RoomTeleport.Players().Where(n=>!n.IsSpectator).OrderBy(n=>n.OwnerClientId)){
     var a=player.GetComponent<Athlete>();var p=ball.Pitch.TransformPoint(new Vector3(-11+lane++*2.2f,ball.PitchBounds.max.y+.03f,-16));
     a.capsule.enabled=false;player.GetComponent<NetworkTransform>().Teleport(p,ball.Pitch.rotation,Vector3.one);a.capsule.enabled=true;a.ResetLocomotion();player.ClearMatchInput();
    }
    ball.ReviewFootballBall(ball.Pitch.TransformPoint(new Vector3(14,ball.PitchBounds.max.y+ball.WorldRadius,10)),Vector3.zero);
   }
   yield return new WaitForSecondsRealtime(3);
   Check(network.GetComponent<NetworkTransform>().PositionLerpSmoothing==NetworkResponsiveness.Baseline,"interpolation policy applied");
   var responsive=network.GetComponent<ResponsiveNetworkTransform>();
   Check(responsive&&responsive.PresentingLatest==(!network.IsServer&&!network.IsSpectator&&!NetworkResponsiveness.Baseline),"latest pose used only by eligible local guest");
   var samples=new List<string>{"trial,client,first_3cm_ms,first_25cm_ms,travel_m,stop_settle_ms"};
   // Spectators stay on their viewing deck; the ten playing places move at once.
   if(!network.IsSpectator)for(int trial=0;trial<6;trial++){
    var origin=actor.transform.position;float heading=FootballBall.Instance.Pitch.eulerAngles.y+(trial%2==0?0:180);
    float start=Time.realtimeSinceStartup,first=-1,quarter=-1;
    DevelopmentProbe.TurnCommand=new PlayerCommand{move=Vector2.up,heading=heading,sprint=true};
    while(Time.realtimeSinceStartup-start<1.4f){
     yield return null;float distance=Vector3.ProjectOnPlane(actor.transform.position-origin,Vector3.up).magnitude;
     if(first<0&&distance>=.03f)first=(Time.realtimeSinceStartup-start)*1000;
     if(quarter<0&&distance>=.25f)quarter=(Time.realtimeSinceStartup-start)*1000;
    }
    DevelopmentProbe.TurnCommand=default;float release=Time.realtimeSinceStartup,settled=release;var previous=actor.transform.position;
    while(Time.realtimeSinceStartup-release<1.4f){yield return null;if(Vector3.Distance(previous,actor.transform.position)>.003f)settled=Time.realtimeSinceStartup;previous=actor.transform.position;}
    float travel=Vector3.ProjectOnPlane(actor.transform.position-origin,Vector3.up).magnitude;
    Check(first>=0&&first<800&&quarter>=first&&travel>5&&travel<13,"movement responds and remains bounded trial "+trial);
    samples.Add(FormattableString.Invariant($"{trial},{network.OwnerClientId},{first:F2},{quarter:F2},{travel:F3},{(settled-release)*1000:F2}"));
   }
   else yield return new WaitForSecondsRealtime(17);
   File.WriteAllLines(Path.Combine(folder,"latency.csv"),samples);
   string shared=Directory.GetParent(folder).FullName;
   File.WriteAllText(Path.Combine(shared,"walk-done-"+network.OwnerClientId),"ready");
   while(RoomTeleport.Players().Any(n=>!File.Exists(Path.Combine(shared,"walk-done-"+n.OwnerClientId))))yield return null;
   yield return Shots(network,shared);
   Check(network.IsSpawned&&app.rooms.Connected&&!app.rooms.Recovering,"peer remains connected after concurrent motion");
   Check(!actor.inTransit&&!RoomTeleport.Busy,"travel state remains unlocked");
   File.WriteAllText(Path.Combine(folder,"roster.txt"),RoomSeats.Signature());
   // Allow the slowest peer to finish before the host disconnects.
   File.WriteAllText(Path.Combine(folder,"done"),"ready");
   while(!File.Exists(Path.Combine(Directory.GetParent(folder).FullName,"finish")))yield return null;
   Finish();
  }
  IEnumerator Shots(NetworkAthlete own,string shared){
   var ball=FootballBall.Instance;var peers=RoomTeleport.Players().OrderBy(n=>n.OwnerClientId).ToArray();
   var rows=new List<string>{"client,shot_visible_ms,animation_ms"};
   foreach(var shooter in peers.Where(n=>!n.IsSpectator).Take(3)){
    string ready=Path.Combine(shared,"shot-ready-"+shooter.OwnerClientId),done=Path.Combine(shared,"shot-done-"+shooter.OwnerClientId);
    if(own.IsServer){
     int i=0;foreach(var peer in peers.Where(n=>!n.IsSpectator)){
      var a=peer.GetComponent<Athlete>();var point=ball.Pitch.TransformPoint(new Vector3(peer==shooter?0:12,ball.PitchBounds.max.y+.025f,peer==shooter?-4:-12+i++*2));
      a.capsule.enabled=false;peer.GetComponent<NetworkTransform>().Teleport(point,ball.Pitch.rotation,Vector3.one);a.capsule.enabled=true;a.ResetLocomotion();peer.ClearMatchInput();
     }
     var target=shooter.GetComponent<Athlete>();ball.ReviewFootballBall(ball.FootPosition(target),Vector3.zero);ball.RefreshControl(target);
     yield return new WaitForSecondsRealtime(1.2f);Check(ball.CurrentController==target,"shot fixture possession "+shooter.OwnerClientId);File.WriteAllText(ready,"ready");
    }
    while(!File.Exists(ready))yield return null;
    if(own.OwnerClientId==shooter.OwnerClientId){
     Check(actor.KickReady&&ball.CurrentController==actor,"local shot permitted by replicated possession");
     var start=ball.transform.position;float began=Time.realtimeSinceStartup,visible=-1,animation=-1;uint gesture=actor.FootballMotion.State.sequence;
     app.view.RequestKick(1);
     while(Time.realtimeSinceStartup-began<1.2f){yield return null;if(visible<0&&Vector3.Distance(ball.transform.position,start)>.15f)visible=(Time.realtimeSinceStartup-began)*1000;if(animation<0&&actor.FootballMotion.State.sequence!=gesture)animation=(Time.realtimeSinceStartup-began)*1000;}
     Check(visible>=0&&visible<800,"accepted kick visible within latency budget");
     Check(animation>=0&&animation<800,"kick animation received");rows.Add(FormattableString.Invariant($"{own.OwnerClientId},{visible:F2},{animation:F2}"));File.WriteAllText(done,"done");
    }
    while(!File.Exists(done))yield return null;
   }
   File.WriteAllLines(Path.Combine(folder,"shots.csv"),rows);
  }
  void Finish(){DevelopmentProbe.TurnCommand=default;DevelopmentProbe.TurnCommandActive=false;File.AppendAllText(Path.Combine(folder,"results.txt"),"RESPONSIVENESS_COMPLETE success="+!failed+"\n");Application.Quit(failed?1:0);}
 }
}
#endif
