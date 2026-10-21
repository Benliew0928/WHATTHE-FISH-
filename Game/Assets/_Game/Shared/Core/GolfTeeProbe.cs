#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace WhatTheFish {
 public sealed partial class GolfPolishProbe {
  void TeePlace(Athlete actor,Vector3 at){bool enabled=actor.capsule.enabled;actor.capsule.enabled=false;actor.transform.position=at+Vector3.up*.05f;actor.ResetLocomotion();actor.capsule.enabled=enabled;}
  IEnumerator TeePresentation(){
   var app=AppRoot.Instance;var local=app.LocalAthlete;
   if(args.Contains("-teeNetwork")){
    if(match.Authority){yield return StartMatch();yield return new WaitForSeconds(1);var roster=match.State.Players.ToArray();Check(roster.Length==2,"NETWORK_TWO_PLAYERS");Check(roster.Select(p=>match.AssignedTee(1,p.PlayerId)).Distinct().Count()==2,"HOST_DISTINCT_T1");foreach(var p in roster){var ball=match.Ball(p.PlayerId);ball.Place(match.Course.holes[0].cup.position+Vector3.up*GolfBall.Radius,true);match.EnterHole(ball,1);}yield return new WaitForSeconds(.5f);Check(roster.Select(p=>match.AssignedTee(2,p.PlayerId)).Distinct().Count()==2,"HOST_DISTINCT_T2");Check(roster.Any(p=>match.AssignedTee(2,p.PlayerId)==0),"HOST_USES_CENTRE_T2");float until=Time.time+25;string signal=Path.Combine(Path.GetDirectoryName(report),"tee-client-done");while(!File.Exists(signal)&&Time.time<until)yield return null;Check(File.Exists(signal),"CLIENT_ACKNOWLEDGED_ASSIGNMENTS");}
    else{float until=Time.time+30;while((match.State.Players.Count!=2||match.State.Players.Any(p=>p.CurrentHole!=2))&&Time.time<until)yield return null;Check(match.State.Players.Count==2&&match.State.Players.All(p=>p.CurrentHole==2),"CLIENT_RECEIVED_HOLE_TWO");var nets=RoomTeleport.Players().Where(n=>match.State.Player(n.StableId)!=null).ToArray();float snapshotUntil=Time.time+10;while(nets.Any(n=>n.GolfBall.Value.teeHole!=2)&&Time.time<snapshotUntil)yield return null;yield return new WaitForSeconds(.15f);Check(nets.Length==2&&nets.All(n=>n.GolfBall.Value.teeHole==2),"CLIENT_RECEIVED_TEE_HOLE");Check(nets.Select(n=>n.GolfBall.Value.teeSlot).Distinct().Count()==2,"CLIENT_NONOVERLAPPING_RESERVATIONS");Check(nets.Any(n=>n.GolfBall.Value.teeSlot==0),"CLIENT_CENTRE_FIRST");foreach(var n in nets){var site=match.Course.teeSites.First(s=>s.hole==2);var expected=match.Course.transform.TransformPoint(site.Pad(n.GolfBall.Value.teeSlot));Check(Vector3.ProjectOnPlane(match.Ball(n.StableId).Body.position-expected,Vector3.up).magnitude<.1f,"CLIENT_BALL_AT_ASSIGNED_PAD");}File.WriteAllText(Path.Combine(Path.GetDirectoryName(report),"tee-client-done"),"done");}
    yield break;
   }
   yield return FiveTeePresentation();
  }
 }
}
#endif
