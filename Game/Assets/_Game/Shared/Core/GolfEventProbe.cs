#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace WhatTheFish {
 public sealed partial class GolfPolishProbe {
  void EventRules(){
   var matchState=new GolfMatchState();matchState.Start(new[]{new GolfEntrant(1,"One"),new GolfEntrant(2,"Two")},0);var events=new GolfEventSchedule();events.Begin(0,152);
   events.Advance(matchState,79.99,2);Check(events.Active(79.99)==GolfEventKind.None&&!events.WarningActive(79.99),"NO_EVENT_BEFORE_WARNING");events.Advance(matchState,80,2);Check(events.WarningActive(80)&&events.Snapshot.start==90&&events.Active(90)!=GolfEventKind.None,"TEN_SECOND_WARNING_THEN_90_SECOND_EVENT");
   bool pool=true;for(uint seed=0;seed<100;seed++){var e=new GolfEventSchedule();e.Begin(0,seed);e.Advance(matchState,80,10);pool&=GolfEventSchedule.IsRandom(e.Snapshot.kind);}Check(pool&&Enum.GetValues(typeof(GolfEventKind)).Length==4,"ONLY_THREE_RANDOM_EVENTS_NO_MOON_OR_SWAP");
   var settings=Resources.Load<GolfBallPhysicsSettings>("GolfBallPhysics");
   Check(new GolfShotConditions{kind=(GolfEventKind)2}.GravityScale==1&&!GolfEventSchedule.IsRandom((GolfEventKind)2),"REMOVED_EVENT_HAS_NO_GRAVITY_MODIFIER");
   var bounce=new GolfShotConditions{kind=GolfEventKind.BounceParty};var v=bounce.Landing(settings,new Vector3(10,-10,0),Vector3.up,true);Check(Mathf.Abs(v.x-7.5f)<.001f&&Mathf.Abs(v.y-8)<.001f,"BOUNCE_20_VERTICAL_25_HORIZONTAL_PERCENT_LOSS");
   Check(new GolfShotConditions{kind=GolfEventKind.StickyGreen}.Landing(settings,new Vector3(8,-5,2),Vector3.up,true)==Vector3.zero,"STICKY_ZEROES_TOUCHDOWN_VELOCITY");
   foreach(var kind in new[]{GolfEventKind.StickyGreen,GolfEventKind.BounceParty}){events.ReviewEvent(kind,1);Check(events.Conditions(2,GolfShotMode.Putt).kind==GolfEventKind.None,"PUTT_UNAFFECTED_"+kind);}
   events.ReviewEvent(GolfEventKind.TropicalWind,1,Vector3.right*GolfEventSchedule.WindAcceleration);Check(events.Conditions(2,GolfShotMode.Swing).wind.magnitude==2.6f&&events.Conditions(2,GolfShotMode.Putt).wind==Vector3.zero,"FOUR_TIMES_WIND_AIR_ONLY");
   for(int h=1;h<=5;h++)matchState.EnterHole(1,h,5+h);
   events.Advance(matchState,10,2);Check(events.Active(11)==GolfEventKind.TropicalWind&&events.Snapshot.nextCycle==0,"RUNNING_WEATHER_CONTINUES_FINISH_STOPS_RANDOM_CYCLE");Check(events.RushActive(11)&&events.RushActive(40.99)&&!events.RushActive(41),"RUSH_ONE_SECOND_DELAY_30_SECONDS");events.Advance(matchState,180,2);Check(events.Active(180)==GolfEventKind.None&&events.Snapshot.nextCycle==0,"NO_NEW_EVENT_AFTER_FIRST_FINISHER");
   var warned=new GolfEventSchedule();warned.Begin(0,15);var fresh=new GolfMatchState();fresh.Start(new[]{new GolfEntrant(1,"A"),new GolfEntrant(2,"B")},0);warned.Advance(fresh,80,2);for(int h=1;h<=5;h++)fresh.EnterHole(1,h,80.5);warned.Advance(fresh,80.5,2);Check(!warned.WarningActive(88)&&warned.Active(90)==GolfEventKind.None,"WARNED_NOT_STARTED_EVENT_CANCELLED_ON_FINISH");
   var snapshot=new GolfMatchSnapshot{round=8,events=events.Snapshot};using var writer=new FastBufferWriter(1024,Allocator.Temp);writer.WriteNetworkSerializable(snapshot);using var reader=new FastBufferReader(writer,Allocator.Temp);reader.ReadNetworkSerializable(out GolfMatchSnapshot copy);Check(snapshot.Equals(copy),"EVENT_STATE_NETWORK_ROUNDTRIP");
  }
  IEnumerator EventPresentation(){if(args.Contains("-eventHudFix")){yield return EventHudFixPresentation();yield break;}EventRules();if(args.Contains("-eventNetwork"))yield return EventNetwork();else if(args.Contains("-rushFiveAudit"))yield return RushFivePresentation();else if(args.Contains("-eventBadges"))yield return EventBadgePresentation();else yield return HotkeyPresentation();}
 }
}
#endif
