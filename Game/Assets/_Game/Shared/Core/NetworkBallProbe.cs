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
   var animatedKicks=new System.Collections.Generic.HashSet<ulong>();var animatedCharges=new System.Collections.Generic.HashSet<ulong>();
   while(elapsed<start+13){
    var players=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None);var snapshot=NetworkAthlete.HostPlayer.Ball.Value;
    foreach(var p in players){var m=p.GetComponent<Athlete>().FootballMotion;if(m&&m.State.charging)animatedCharges.Add(p.OwnerClientId);if(m&&m.State.gesture==FootballGesture.Kick)animatedKicks.Add(p.OwnerClientId);}
    if(app.rooms.Host&&!arranged&&elapsed>start+1){
     ball.ResetBall();centre=ball.Body.position;
     foreach(var p in players){var pos=centre+new Vector3(p.OwnerClientId==0?-4:0,-.22f,p.OwnerClientId==0?-4:-.9f);PlaceAthlete(p.GetComponent<Athlete>(),pos,0);p.GetComponent<NetworkTransform>().Teleport(pos,Quaternion.identity,Vector3.one);}
     arranged=true;
    }
    if(!app.rooms.Host&&!clientKick&&!clientCharging&&snapshot.valid&&snapshot.sequence>=2&&app.LocalAthlete.KickReady){centre=snapshot.position;clientCharging=app.view.BeginKick();clientChargeStart=elapsed;}
    if(clientCharging&&!clientKick){TurnCommand=new PlayerCommand{move=Vector2.up*.2f,sprint=true};clientControlSeen|=ball.CurrentController==app.LocalAthlete;}
    if(app.rooms.Host)foreach(var p in players)if(p.OwnerClientId!=0){var a=p.GetComponent<Athlete>();if(a.Charging){serverChargeSeen=true;chargeMovePeak=Mathf.Max(chargeMovePeak,a.Motor.Velocity.magnitude);}}
    if(clientCharging&&!clientKick&&elapsed>=clientChargeStart+ball.maximumChargeTime*.5f){app.view.EndKick();clientKick=true;TurnCommand=default;Check(clientControlSeen,"CLIENT_CONTROLLER_ID_REPLICATES_WHILE_CHARGING");}
    if(snapshot.valid&&(app.rooms.Host?arranged&&!hostKick:clientCharging||clientKick)){clientTravel=Mathf.Max(clientTravel,Vector3.Distance(ball.Body.position,centre));if(app.rooms.Host&&!ball.Body.isKinematic)clientPeak=Mathf.Max(clientPeak,ball.Body.linearVelocity.magnitude);}
    if(app.rooms.Host&&!hostKick&&elapsed>start+6){
     Check(serverChargeSeen&&chargeMovePeak>.6f&&chargeMovePeak<=.735f,"SERVER_CHARGING_MOVEMENT_PENALTY_NO_SPRINT");Record("MEASURE network chargeMovePeak="+chargeMovePeak);Check(clientTravel>2,"CLIENT_RPC_MOVES_AUTHORITATIVE_BALL");Check(clientPeak>ball.minimumKickSpeed*FootballEffort.BallPace+1&&clientPeak<ball.kickSpeed*FootballEffort.BallPace-1,"CLIENT_HALF_CHARGE_REACHES_SERVER");ball.ResetBall();centre=ball.Body.position;
     foreach(var p in players){var pos=centre+new Vector3(p.OwnerClientId==0?0:4,-.22f,p.OwnerClientId==0?-.9f:-4);PlaceAthlete(p.GetComponent<Athlete>(),pos,0);p.GetComponent<NetworkTransform>().Teleport(pos,Quaternion.identity,Vector3.one);}
     hostKick=true;
    }
    if(app.rooms.Host&&hostKick&&!hostCharging&&elapsed>start+6.5f)hostCharging=app.view.BeginKick();
    if(hostCharging&&!hostReleased&&elapsed>start+6.5f+ball.maximumChargeTime){app.view.EndKick();hostReleased=true;}
    if(snapshot.valid&&snapshot.sequence>=3)hostTravel=Mathf.Max(hostTravel,Vector3.Distance(ball.Body.position,centre));
    yield return null;
   }
   Check(clientTravel>2&&hostTravel>2,"BOTH_PLAYERS_KICK_ONE_SHARED_BALL");
   Check(animatedKicks.Count==expected&&animatedCharges.Count==expected,"BOTH_PLAYERS_KICK_AND_CHARGE_POSES_REPLICATE");
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
   // Keep server fixtures alive long enough for clients whose scene initialization finishes later.
   if(app.rooms.Host)yield return new WaitForSeconds(.8f);
   if(app.rooms.Host){
    ball.ResetBall();centre=ball.Body.position;
    foreach(var p in FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None)){var pos=centre+new Vector3(p.OwnerClientId==0?-4:0,-.22f,p.OwnerClientId==0?-4:-.7f);PlaceAthlete(p.GetComponent<Athlete>(),pos,0);p.GetComponent<NetworkTransform>().Teleport(pos,Quaternion.identity,Vector3.one);}
   }
   yield return new WaitForSeconds(.3f);
   if(!app.rooms.Host)TurnCommand=new PlayerCommand{move=Vector2.up*.2f};
   yield return new WaitForSeconds(2);TurnCommand=default;yield return new WaitForSeconds(.3f);
   var guest=FindObjectsByType<NetworkAthlete>(FindObjectsSortMode.None).First(p=>p.OwnerClientId!=0).GetComponent<Athlete>();
   if(app.rooms.Host)Check(guest.Grounded&&ball.Body.position.z>centre.z+.3f,"SERVER_REMOTE_PLAYER_ATTACHED_MOVEMENT_WITHOUT_CLIMBING");
   Check(guest.transform.position.y<ball.Body.position.y-.1f,"REMOTE_GROUNDED_HEIGHT_REPLICATES");
   Check(ball.CurrentController==guest&&ball.Body.isKinematic&&Vector3.Distance(ball.transform.position,ball.FootPosition(guest))<.12f,"REMOTE_BALL_ATTACHED_TO_REPLICATED_CONTROLLER");
   if(app.rooms.Host){var pos=guest.transform.position;PlaceAthlete(guest,pos,180);guest.GetComponent<NetworkTransform>().Teleport(pos,Quaternion.Euler(0,180,0),Vector3.one);}
   if(app.rooms.Host)yield return new WaitForSeconds(.6f);
   else {float until=Time.realtimeSinceStartup+3;while(Time.realtimeSinceStartup<until&&(Vector3.Dot(guest.transform.forward,Vector3.back)<.95f||ball.CurrentController!=guest||Vector3.Distance(ball.transform.position,ball.FootPosition(guest))>=.12f))yield return null;}
   Record($"MEASURE network turn owner={ball.CurrentController==guest} yaw={guest.transform.eulerAngles.y:F2} footGap={Vector3.Distance(ball.transform.position,ball.FootPosition(guest)):F4}");
   Check(ball.CurrentController==guest&&Vector3.Dot(guest.transform.forward,Vector3.back)>.95f&&Vector3.Distance(ball.transform.position,ball.FootPosition(guest))<.12f,"REMOTE_STATIONARY_180_TURN_KEEPS_FEET_ATTACHMENT");
   if(app.rooms.Host){
    yield return new WaitForSeconds(.8f);
    var tackler=app.LocalAthlete;var pos=guest.transform.position+Vector3.left*1.4f;PlaceAthlete(tackler,pos,90);tackler.GetComponent<NetworkTransform>().Teleport(pos,Quaternion.Euler(0,90,0),Vector3.one);
    Check(tackler.TryTackle(),"HOST_FREE_PLAYER_CAN_TACKLE_REMOTE_CONTROLLER");
   }
   if(app.rooms.Host)yield return new WaitForSeconds(.15f);
   else {float until=Time.realtimeSinceStartup+3;while(Time.realtimeSinceStartup<until&&(ball.CurrentController||guest.Action!=FootballAction.Hit))yield return null;}
   Check(!ball.CurrentController&&guest.Action==FootballAction.Hit,"SUCCESSFUL_REMOTE_PLAYER_TACKLE_RELEASE_REPLICATES");
   if(app.rooms.Host)yield return new WaitForSeconds(.6f);
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
   Check(ball.Pitch.GetComponentsInChildren<FootballGoalNet>().Length==2,"BOTH_PEERS_HAVE_GOAL_NET_WORLD_COLLISION");
   if(app.rooms.Host){
    yield return new WaitForSeconds(1.5f);ball.ResetBall();
    for(int i=0;i<ball.GoalCount;i++){
     var netBounds=ball.GoalBounds(i);float front=ball.GoalFront(i);int sign=ball.GoalSign(i);float back=sign>0?netBounds.max.z:netBounds.min.z;
     foreach(int face in new[]{0,1,2}){
      var normal=face==0?Vector3.left:face==1?Vector3.right:Vector3.forward*sign;
      var surface=new Vector3(face==0?netBounds.min.x:face==1?netBounds.max.x:netBounds.center.x,netBounds.min.y,face==2?back:(front+back)*.5f);
      var direction=ball.Pitch.TransformDirection(normal);float yaw=Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg;
      PlaceAthlete(guest,ball.Pitch.TransformPoint(surface-normal*.8f),yaw);
      for(int step=0;step<12;step++)guest.Simulate(new PlayerCommand{move=Vector2.up,sprint=true,heading=yaw},.05f);
      Check(Vector3.Dot(ball.Pitch.InverseTransformPoint(guest.transform.position)-surface,normal)<-.25f,"SERVER_REMOTE_PLAYER_BLOCKED_BY_NET_"+i+"_"+face);
     }
    }
    guest.GetComponent<NetworkTransform>().Teleport(guest.transform.position,guest.transform.rotation,Vector3.one);yield return new WaitForSeconds(1);
   }else{
    var last=ball.GoalBounds(ball.GoalCount-1);int sign=ball.GoalSign(ball.GoalCount-1);float back=sign>0?last.max.z:last.min.z,until=Time.realtimeSinceStartup+5;
    while(Time.realtimeSinceStartup<until&&Mathf.Abs(ball.Pitch.InverseTransformPoint(guest.transform.position).z-back)>1)yield return null;
   }
   var lastBounds=ball.GoalBounds(ball.GoalCount-1);int lastSign=ball.GoalSign(ball.GoalCount-1);float lastBack=lastSign>0?lastBounds.max.z:lastBounds.min.z;
   float netDistance=(ball.Pitch.InverseTransformPoint(guest.transform.position).z-lastBack)*lastSign;
   Check(netDistance<-.25f&&netDistance>-1,"REMOTE_NET_STOP_POSE_REPLICATES_TO_BOTH_PEERS");
   TurnCommandActive=false;Record("NETWORK_FOOTBALL_BALL_COMPLETE");
  }
 }
}
#endif
