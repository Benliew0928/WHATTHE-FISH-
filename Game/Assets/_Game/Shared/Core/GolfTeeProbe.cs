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
   var helpers=Enumerable.Range(0,3).Select(i=>{var a=Instantiate(app.athletePrefab).GetComponent<Athlete>();a.name="Tee fixture "+i;a.Setup();return a;}).ToArray();
   var actors=new[]{local}.Concat(helpers).ToArray();ulong localId=GolfCartWorld.Key(local);var ids=new[]{localId,101UL,102UL,103UL};
   var fixtureRoster=actors.Select((a,i)=>(ids[i],a,"Tee player "+i)).ToArray();bool ready=false;float readyUntil=Time.time+18;while(!ready&&Time.time<readyUntil){ready=match.StartTestMatch(fixtureRoster);if(!ready)yield return null;}Check(ready,"FOUR_PLAYER_MATCH_READY_AFTER_ARRIVAL");yield return new WaitForSeconds(.7f);
   Check(ids.Select(id=>match.AssignedTee(1,id)).Distinct().Count()==4,"FOUR_DISTINCT_T1_PADS");Check(ids.Select(id=>Mathf.Abs(match.Course.teeSites[0].Offset(match.AssignedTee(1,id)))).OrderBy(i=>i).SequenceEqual(new[]{0,1,1,2}),"T1_CENTRE_THEN_OUTWARD");
   foreach(var id in ids){var ball=match.Ball(id);Check(ball.Live&&ball.ReadyToAim,"T1_BALL_SUPPORTED_AND_READY");Check(Vector3.Distance(match.Tee(id).transform.position,ball.Body.position-Vector3.up*(GolfBall.Radius+.002f+GolfTee.SeatHeight))<.035f,"OWN_WOODEN_TEE_ALIGNED");}
   Check(app.view.BeginGolfAim(),"LOCAL_AIM_ON_NEW_TEE");yield return new WaitForSeconds(.2f);Check(match.IsAiming(local),"LOCAL_AIM_CONFIRMED");match.ReleaseAim(local);app.view.ClearMatchInput();
   for(int i=0;i<3;i++){var ball=match.Ball(ids[i]);ball.Place(match.Course.holes[0].cup.position+Vector3.up*GolfBall.Radius,true);Check(match.EnterHole(ball,1)==GolfHoleResult.Completed,"SIMULTANEOUS_H1_COMPLETE");}
   yield return new WaitForSeconds(.15f);
   Check(ids.Take(3).Select(id=>match.AssignedTee(2,id)).OrderBy(i=>i).SequenceEqual(new[]{0,1,2}),"THREE_FINISHERS_GET_THREE_DIFFERENT_PADS");
   var fourth=match.Ball(ids[3]);fourth.Place(match.Course.holes[0].cup.position+Vector3.up*GolfBall.Radius,true);match.EnterHole(fourth,1);yield return new WaitForSeconds(.15f);
   Check(match.WaitingForTee(actors[3])&&!fourth.Live,"FOURTH_WAITS_WHEN_FULL");Check(fourth.GetComponentsInChildren<MeshRenderer>().All(r=>!r.enabled),"WAITING_BALL_NOT_DRAWN_IN_PREVIOUS_CUP");
   var centreOwner=ids.Take(3).First(id=>match.AssignedTee(2,id)==0);int centreIndex=System.Array.IndexOf(ids,centreOwner);var centreBall=match.Ball(centreOwner);var forward=Vector3.ProjectOnPlane(match.Course.holes[1].cup.position-centreBall.Body.position,Vector3.up).normalized;
   var away=centreBall.Body.position+forward*15;centreBall.Place(away,true);TeePlace(actors[centreIndex],away+Vector3.left*3);yield return new WaitForSeconds(.15f);
   Check(match.AssignedTee(2,ids[3])==0&&fourth.Live&&!match.WaitingForTee(actors[3]),"WAITING_PLAYER_RECEIVES_FREED_CENTRE");
   var pad=fourth.Body.position;fourth.Place(pad+Vector3.up*8,false);TeePlace(actors[3],pad+Vector3.right*12);yield return new WaitForSeconds(.08f);Check(match.AssignedTee(2,ids[3])==0,"RECOVERY_ANCHORS_KEEP_RESERVATION");
   var water=match.Course.waterPatches[0];fourth.Place(match.Course.transform.TransformPoint(water.centre)+Vector3.up*.02f,false);uint recovery=fourth.WaterRecoverySequence;yield return new WaitForSeconds(.15f);Check(fourth.WaterRecoverySequence>recovery&&Vector3.Distance(fourth.Body.position,pad)<.15f,"WATER_RETURNS_TO_RESERVED_PAD");
   Check(!match.CanStrike(local,match.Ball(ids[3])),"FOREIGN_BALL_STILL_REJECTED");
   foreach(int hole in new[]{2,3,4}){foreach(var id in ids.Take(3)){var ball=match.Ball(id);ball.Place(match.Course.holes[hole-1].cup.position+Vector3.up*GolfBall.Radius,true);match.EnterHole(ball,hole);}yield return new WaitForSeconds(.15f);Check(ids.Take(3).Select(id=>match.AssignedTee(hole+1,id)).OrderBy(i=>i).SequenceEqual(new[]{0,1,2}),"THREE_UNIQUE_T"+(hole+1));}
   match.ClearMatch();foreach(var helper in helpers)Destroy(helper.gameObject);Check(match.State.Phase==GolfMatchPhase.Idle,"CLEAR_MATCH_RESETS_ALLOCATIONS");
  }
 }
}
#endif
