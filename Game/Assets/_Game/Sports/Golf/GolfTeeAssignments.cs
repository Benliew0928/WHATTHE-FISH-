using System;
using System.Linq;
using UnityEngine;

namespace WhatTheFish {
 public sealed partial class GolfMatchManager {
  GolfTeeReservations teeReservations;
  bool teeBatch;double assignAfter;
  GolfTeeSite Site(int hole)=>Course.teeSites.First(s=>s.hole==hole);
  public bool WaitingForTee(Athlete actor){var p=Player(actor);return p!=null&&(Authority?teeReservations!=null&&teeReservations.Waiting(p.PlayerId):Ball(p.PlayerId)&&!Ball(p.PlayerId).Live&&State.Running&&!p.IsFinished&&!p.IsDNF);}
  public int AssignedTee(int hole,ulong owner)=>teeReservations!=null&&teeReservations.TryGet(hole,owner,out var i)?i:-1;
  internal void TeeSnapshot(ulong owner,out int hole,out int slot,out int queue){hole=0;slot=-1;queue=-1;if(teeReservations==null)return;queue=teeReservations.QueueIndex(owner);foreach(var r in teeReservations.Occupied)if(r.owner==owner){hole=r.hole;slot=r.slot;break;}}
  internal void RestoreTeeAssignments(){
   teeReservations=new GolfTeeReservations(unchecked((int)DateTime.UtcNow.Ticks));
   foreach(var net in RoomTeleport.Players()){var s=net.GolfBall.Value;if(s.valid&&s.teeHole>0&&s.teeSlot>=0)teeReservations.Restore(s.teeHole,net.StableId,s.teeSlot);}
   foreach(var net in RoomTeleport.Players().Where(n=>n.GolfBall.Value.waiting).OrderBy(n=>n.GolfBall.Value.teeQueue)){var p=State.Player(net.StableId);if(p!=null)teeReservations.Request(p.CurrentHole,net.StableId,net.GolfBall.Value.teeQueue);}
  }
  void RequestTee(ulong owner,int hole){
   teeReservations.Request(hole,owner,(long)Math.Round(Now/Time.fixedDeltaTime));assignAfter=Now+.025;
   var ball=Ball(owner);if(ball){ball.SetLive(false);ball.SetTeeWaiting(true);}
  }
  bool PadClear(int hole,int slot){
   if(teeBatch)return true;var at=Course.transform.TransformPoint(Site(hole).Pad(slot));
   return !balls.Values.Any(b=>b&&b.Live&&Vector3.ProjectOnPlane(b.Body.position-at,Vector3.up).sqrMagnitude<9)&&!Athlete.Active.Any(a=>a&&a.isActiveAndEnabled&&Vector3.ProjectOnPlane(a.transform.position-at,Vector3.up).sqrMagnitude<6.25f);
  }
  void AssignTees(){
   if(teeReservations==null||!State.Running)return;
   foreach(var p in State.Players)if(teeReservations.Waiting(p.PlayerId)&&(!actors.TryGetValue(p.PlayerId,out var a)||!a||!a.isActiveAndEnabled||p.IsFinished||p.IsDNF))teeReservations.Remove(p.PlayerId);
   foreach(var r in teeReservations.Occupied){
    var player=State.Player(r.owner);var ball=Ball(r.owner);actors.TryGetValue(r.owner,out var actor);
    if(player==null||player.IsDNF||player.IsFinished||!actor){teeReservations.Remove(r.owner);continue;}
    var at=Course.transform.TransformPoint(Site(r.hole).Pad(r.slot));
    bool Away(Vector3 p)=>Vector3.ProjectOnPlane(p-at,Vector3.up).sqrMagnitude>25;
    // Preserve both recovery anchors until a later shot has a safe origin.
    if(player.CurrentHole!=r.hole||ball&&Away(ball.Body.position)&&Away(ball.LastShotPosition)&&Away(ball.LastDryPosition)&&Away(actor.transform.position)&&!actor.GolfClubMotion.Busy)teeReservations.Release(r.hole,r.owner);
   }
   if(!teeBatch&&Now<assignAfter)return;
   var assigned=teeReservations.Assign(h=>Site(h).count,(h,i)=>Site(h).Offset(i),PadClear);
   foreach(var r in assigned){var ball=Ball(r.owner);if(!ball)continue;ball.SetTeeWaiting(false);ball.SetLive(true);ball.Place(TeePosition(r.hole,r.owner),true);if(r.hole==1){var support=Tee(r.owner);if(support)support.transform.position=TeeGroundPosition(1,r.owner);}}
  }
 }
}
