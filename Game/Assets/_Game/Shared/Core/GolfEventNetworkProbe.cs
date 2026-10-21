#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace WhatTheFish {
 public sealed partial class GolfPolishProbe {
  IEnumerator EventNetwork(){
   string folder=Path.GetDirectoryName(report);float until;bool host=match.Authority;
   if(host)yield return StartMatch();else {until=Time.time+25;while(match.State.Players.Count!=2&&Time.time<until)yield return null;}yield return new WaitForSeconds(1);
   var nets=RoomTeleport.Players().Where(n=>match.State.Player(n.StableId)!=null).OrderBy(n=>n.StableId).ToArray();Check(nets.Length==2,"EVENT_NETWORK_TWO_PLAYERS");
   string warningAck=Path.Combine(folder,"warning-client-ok");
   if(host){var state=match.Events.Snapshot;state.kind=GolfEventKind.StickyGreen;state.start=match.Now+10;state.end=state.start+GolfEventSchedule.Duration(state.kind);state.nextCycle=state.start+90;match.Events.Receive(state);until=Time.time+15;while(!File.Exists(warningAck)&&Time.time<until)yield return null;Check(File.Exists(warningAck),"CLIENT_ACKNOWLEDGES_TEN_SECOND_WARNING");}
   else {until=Time.time+15;while(!match.Events.WarningActive(match.Now)&&Time.time<until)yield return null;Check(match.Events.WarningActive(match.Now)&&match.Events.Snapshot.start-match.Now>8,"CLIENT_RECEIVES_TEN_SECOND_WARNING");yield return new WaitForSeconds(.15f);Check(GolfEventHUD.Instance.Card.GetComponentsInChildren<GolfEventBadgeGraphic>().Any(a=>a.Badge==5),"CLIENT_SHOWS_SUPPLIED_WARNING_BADGE");yield return new WaitForEndOfFrame();CaptureUI("network-warning",null,0,0,0,false);File.WriteAllText(warningAck,"ok");}
   var positions=nets.Select(n=>n.transform.position).ToArray();var balls=nets.Select(n=>match.Ball(n.StableId).Body.position).ToArray();
   for(int i=0;i<GolfEventSchedule.RandomKinds.Count;i++){
    var kind=GolfEventSchedule.RandomKinds[i];string ack=Path.Combine(folder,"event-"+i+"-client-ok");
    if(host){Check(match.ReviewEventHotkey((KeyCode)((int)KeyCode.F1+(int)kind-1)),"HOST_DISPATCH_"+kind);until=Time.time+20;while(!File.Exists(ack)&&Time.time<until)yield return null;Check(File.Exists(ack),"CLIENT_ACKNOWLEDGES_"+kind);}
    else {until=Time.time+25;while(match.Events.Active(match.Now)!=kind&&Time.time<until)yield return null;Check(match.Events.Active(match.Now)==kind,"CLIENT_RECEIVES_"+kind);Check(!match.ReviewEventHotkey(KeyCode.F1),"CLIENT_CANNOT_TRIGGER_"+kind);
     var swing=match.ShotConditions(GolfShotMode.Swing);var putt=match.ShotConditions(GolfShotMode.Putt);
     if(i==0){Check(swing.wind==Vector3.right*2.6f&&putt.wind==Vector3.zero,"CLIENT_STRONG_WIND_AIR_ONLY");yield return new WaitForSeconds(.6f);Check(match.GetComponent<GolfWindWorld>().Strength>.99f,"CLIENT_WIND_SCENERY_ACTIVE");}
     if(kind==GolfEventKind.StickyGreen||kind==GolfEventKind.BounceParty)Check(putt.kind==GolfEventKind.None,"CLIENT_PUTT_EXCLUDED_"+kind);
     File.WriteAllText(ack,"ok");}
   }
   Check(nets.Select((n,i)=>Vector3.ProjectOnPlane(n.transform.position-positions[i],Vector3.up).magnitude<.25f&&Vector3.Distance(match.Ball(n.StableId).Body.position,balls[i])<.15f).All(v=>v)&&match.State.Players.All(p=>p.TotalStroke==0&&p.CurrentHole==1),"NO_POSITION_SWAP_OR_SCORE_CHANGES");
   if(host){Check(!match.ReviewEventHotkey(KeyCode.F5),"HOST_F5_REMOVED");var ball=match.Ball(nets[0].StableId);for(int h=1;h<=5;h++){match.EnterHole(ball,h);yield return new WaitForSeconds(.15f);}}
   until=Time.time+25;while(!match.Events.RushActive(match.Now)&&Time.time<until)yield return null;
   Check(match.Events.RushActive(match.Now)&&match.EventMoveMultiplier(AppRoot.Instance.LocalAthlete)==2.1f&&match.EventCartMultiplier==1.9f,"NETWORK_NEW_RUSH_MULTIPLIERS");Check(match.Events.Active(match.Now)==GolfEventKind.BounceParty&&match.Events.Snapshot.nextCycle==0,"NETWORK_RUNNING_EVENT_CONTINUES_RANDOM_CYCLE_STOPS");yield return new WaitForEndOfFrame();CaptureUI("network-final",null,0,0,0,false);
   string rushAck=Path.Combine(folder,"rush-client-ok");if(!host)File.WriteAllText(rushAck,"ok");else {until=Time.time+20;while(!File.Exists(rushAck)&&Time.time<until)yield return null;Check(File.Exists(rushAck),"CLIENT_ACKNOWLEDGES_RUSH");Check(match.ReviewEventHotkey(KeyCode.F7),"HOST_CLEAR_EVENTS");}
   if(!host){until=Time.time+15;while(match.Events.RushActive(match.Now)&&Time.time<until)yield return null;Check(!match.Events.RushActive(match.Now)&&match.Events.Active(match.Now)==GolfEventKind.None,"CLIENT_RECEIVES_CLEAR");}
  }
 }
}
#endif
