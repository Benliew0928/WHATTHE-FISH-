#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Linq;
using Unity.Netcode.Components;
using UnityEngine;
namespace WhatTheFish {
 public sealed partial class DevelopmentProbe {
  IEnumerator NetworkBallAudit(){
   TurnCommandActive=true;TurnCommand=default;
   while(!AppRoot.Instance||!AppRoot.Instance.Exploring||!FootballBall.Instance)yield return null;
   var app=AppRoot.Instance;var ball=FootballBall.Instance;float start=elapsed;bool arranged=false,hostKick=false,clientKick=false,clientCharging=false,hostCharging=false,hostReleased=false;bool serverChargeSeen=false,clientControlSeen=false;float chargeMovePeak=0;float clientChargeStart=0,clientPeak=0;Vector3 centre=ball.Body.position;float clientTravel=0,hostTravel=0;
   while(elapsed<start+13){
    var players=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None);var snapshot=NetworkAthlete.HostPlayer.Ball.Value;
    if(app.rooms.Host&&!arranged&&elapsed>start+1){
     ball.ResetBall();centre=ball.Body.position;
     foreach(var p in players){var pos=centre+new Vector3(p.OwnerClientId==0?-4:0,-.22f,p.OwnerClientId==0?-4:-.9f);PlaceAthlete(p.GetComponent<Athlete>(),pos,0);p.GetComponent<NetworkTransform>().Teleport(pos,Quaternion.identity,Vector3.one);}
     arranged=true;
    }
    if(!app.rooms.Host&&!clientKick&&!clientCharging&&snapshot.valid&&snapshot.sequence>=2&&app.LocalAthlete.KickReady){centre=snapshot.position;clientCharging=app.view.BeginKick();clientChargeStart=elapsed;}
    if(clientCharging&&!clientKick){TurnCommand=new PlayerCommand{move=Vector2.up*.2f,sprint=true};clientControlSeen|=ball.CurrentController==app.LocalAthlete;}
    if(app.rooms.Host)foreach(var p in players)if(p.OwnerClientId!=0){var a=p.GetComponent<Athlete>();if(a.Charging){serverChargeSeen=true;chargeMovePeak=Mathf.Max(chargeMovePeak,a.Motor.Velocity.magnitude);}}
    if(clientCharging&&!clientKick&&elapsed>=clientChargeStart+ball.maximumChargeTime*.5f){app.view.EndKick();clientKick=true;TurnCommand=default;Check(clientControlSeen,"CLIENT_CONTROLLER_ID_REPLICATES_WHILE_CHARGING");}
    if(snapshot.valid&&snapshot.sequence==2){clientTravel=Mathf.Max(clientTravel,Vector3.Distance(ball.Body.position,centre));if(app.rooms.Host)clientPeak=Mathf.Max(clientPeak,ball.Body.linearVelocity.magnitude);}
    if(app.rooms.Host&&!hostKick&&elapsed>start+6){
     Check(serverChargeSeen&&chargeMovePeak>.4f&&chargeMovePeak<=.49f,"SERVER_CHARGING_MOVEMENT_PENALTY_NO_SPRINT");Record("MEASURE network chargeMovePeak="+chargeMovePeak);Check(clientTravel>2,"CLIENT_RPC_MOVES_AUTHORITATIVE_BALL");Check(clientPeak>ball.minimumKickSpeed+1&&clientPeak<ball.kickSpeed-1,"CLIENT_HALF_CHARGE_REACHES_SERVER");ball.ResetBall();centre=ball.Body.position;
     foreach(var p in players){var pos=centre+new Vector3(p.OwnerClientId==0?0:4,-.22f,p.OwnerClientId==0?-.9f:-4);PlaceAthlete(p.GetComponent<Athlete>(),pos,0);p.GetComponent<NetworkTransform>().Teleport(pos,Quaternion.identity,Vector3.one);}
     hostKick=true;
    }
    if(app.rooms.Host&&hostKick&&!hostCharging&&elapsed>start+6.5f)hostCharging=app.view.BeginKick();
    if(hostCharging&&!hostReleased&&elapsed>start+6.5f+ball.maximumChargeTime){app.view.EndKick();hostReleased=true;}
    if(snapshot.valid&&snapshot.sequence>=3)hostTravel=Mathf.Max(hostTravel,Vector3.Distance(ball.Body.position,centre));
    yield return null;
   }
   Check(clientTravel>2&&hostTravel>2,"BOTH_PLAYERS_KICK_ONE_SHARED_BALL");
   Check(app.rooms.Host?!ball.Body.isKinematic:ball.Body.isKinematic&&!ball.Body.detectCollisions,"ONLY_HOST_SIMULATES_PHYSICS");
   Check(Vector3.Distance(ball.Body.position,NetworkAthlete.HostPlayer.Ball.Value.position)<1,"SNAPSHOT_PRESENTATION_WITHIN_ONE_METRE");
   if(app.rooms.Host){
    ball.ResetBall();centre=ball.Body.position;
    foreach(var p in FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None)){var pos=centre+new Vector3(p.OwnerClientId==0?-4:0,-.22f,p.OwnerClientId==0?-4:2);PlaceAthlete(p.GetComponent<Athlete>(),pos,0);p.GetComponent<NetworkTransform>().Teleport(pos,Quaternion.identity,Vector3.one);}
    ball.Body.WakeUp();ball.Body.linearVelocity=Vector3.forward*9;
   }
   yield return new WaitForSeconds(1.2f);
   if(app.rooms.Host)Check(ball.Body.linearVelocity.magnitude<.08f&&ball.Body.position.z<centre.z+1.6f,"SERVER_BALL_STOPS_AT_REMOTE_PLAYER");
   var stopped=ball.Body.position;yield return new WaitForSeconds(.3f);
   Check(Vector3.Distance(stopped,ball.Body.position)<.08f&&Vector3.Distance(ball.Body.position,NetworkAthlete.HostPlayer.Ball.Value.position)<.15f,"STOPPED_BALL_REPLICATES_TO_BOTH_PLAYERS");
   if(app.rooms.Host){
    ball.ResetBall();centre=ball.Body.position;
    foreach(var p in FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None)){var pos=centre+new Vector3(p.OwnerClientId==0?-4:0,-.22f,p.OwnerClientId==0?-4:-.7f);PlaceAthlete(p.GetComponent<Athlete>(),pos,0);p.GetComponent<NetworkTransform>().Teleport(pos,Quaternion.identity,Vector3.one);}
   }
   yield return new WaitForSeconds(.3f);
   if(!app.rooms.Host)TurnCommand=new PlayerCommand{move=Vector2.up*.2f};
   yield return new WaitForSeconds(2);TurnCommand=default;yield return new WaitForSeconds(.3f);
   var guest=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).First(p=>p.OwnerClientId!=0).GetComponent<Athlete>();
   if(app.rooms.Host)Check(guest.Grounded&&ball.Body.position.z>centre.z+.3f,"SERVER_REMOTE_PLAYER_PUSHES_WITHOUT_CLIMBING");
   Check(guest.transform.position.y<ball.Body.position.y-.1f,"REMOTE_GROUNDED_HEIGHT_REPLICATES");
   if(app.rooms.Host){
    ball.ResetBall();var edge=ball.Pitch.InverseTransformPoint(ball.Body.position);edge.x=ball.PitchBounds.max.x-.5f;
    ball.Body.position=ball.Pitch.TransformPoint(edge);ball.Body.WakeUp();ball.Body.linearVelocity=ball.Pitch.TransformDirection(Vector3.right)*18;
   }
   if(app.rooms.Host)yield return new WaitForSeconds(1.5f);
   else {float until=Time.realtimeSinceStartup+5;while(Time.realtimeSinceStartup<until){var point=ball.Pitch.InverseTransformPoint(ball.Body.position);if(point.x>ball.PitchBounds.max.x-.5f&&Vector3.Distance(ball.Body.position,NetworkAthlete.HostPlayer.Ball.Value.position)<.15f)break;yield return null;}}
   var bounded=ball.Pitch.InverseTransformPoint(ball.Body.position);
   Check(bounded.x>ball.PitchBounds.max.x-.5f&&bounded.x<=ball.PitchBounds.max.x-.218f&&Vector3.Distance(ball.Body.position,NetworkAthlete.HostPlayer.Ball.Value.position)<.15f,"BALL_BOUNDARY_REPLICATES_TO_BOTH_PLAYERS");
   for(int i=0;i<ball.GoalCount;i++){
    var goal=ball.GoalBounds(i);int sign=goal.center.z>ball.PitchBounds.center.z?1:-1;float line=sign>0?ball.PitchBounds.max.z:ball.PitchBounds.min.z;
    if(app.rooms.Host){ball.ResetBall();var shot=ball.Pitch.InverseTransformPoint(ball.Body.position);shot.x=goal.center.x;shot.z=line-sign*.5f;ball.Body.position=ball.Pitch.TransformPoint(shot);ball.Body.WakeUp();ball.Body.linearVelocity=ball.Pitch.TransformDirection(Vector3.forward*sign)*9;}
    if(app.rooms.Host)yield return new WaitForSeconds(1.2f);
    else {float until=Time.realtimeSinceStartup+5;while(Time.realtimeSinceStartup<until){var point=ball.Pitch.InverseTransformPoint(ball.Body.position);if((point.z-line)*sign>.22f&&Vector3.Distance(ball.Body.position,NetworkAthlete.HostPlayer.Ball.Value.position)<.15f)break;yield return null;}}
    var inside=ball.Pitch.InverseTransformPoint(ball.Body.position);
    Check((inside.z-line)*sign>.22f&&inside.z>=goal.min.z+.218f&&inside.z<=goal.max.z-.218f&&Vector3.Distance(ball.Body.position,NetworkAthlete.HostPlayer.Ball.Value.position)<.15f,"GOAL_ENTRY_REPLICATES_TO_BOTH_PLAYERS_"+i);
   }
   TurnCommandActive=false;Record("NETWORK_FOOTBALL_BALL_COMPLETE");
  }
 }
}
#endif
