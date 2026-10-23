using System;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using WhatTheFish;
public static class GolfRoundPolishChecks {
 public static void Run(){
  int checks=0;void Check(bool ok,string label){if(!ok)throw new Exception("Round polish: "+label);checks++;Debug.Log("ROUND_POLISH_CHECK PASS "+label);}
  bool coverage=true,boundaries=true,routes=true,restored=true;int varied=0;
  for(uint seed=0;seed<100;seed++){
   var events=Enumerable.Range(0,20).Select(i=>GolfEventSchedule.ScheduledKind(seed,(uint)i)).ToArray();
   for(int bag=0;bag<4;bag++)coverage&=events.Skip(bag*5).Take(5).Distinct().Count()==5;
   boundaries&=Enumerable.Range(1,19).All(i=>events[i]!=events[i-1]);
   var state=new GolfMatchState();state.Start(new[]{new GolfEntrant(0,"A"),new GolfEntrant(1,"B")},0);state.ShuffleRoute(seed);
   var order=Enumerable.Range(0,5).Select(i=>GolfMatchState.RouteHole(state.RouteCode,i)).ToArray();routes&=order[0]==1&&order.Distinct().Count()==5;if(state.RouteCode!=12345)varied++;
   var p=state.Player(0);state.EnterHole(0,p.CurrentHole,1);
   var snapshot=new GolfMatchSnapshot{phase=state.Phase,round=9,routeCode=state.RouteCode,started=state.StartedAt};
   var peer=new GolfMatchState();typeof(GolfMatchState).GetMethod("Receive",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(peer,new object[]{snapshot,state.Players.Select(x=>(x.PlayerId,GolfPlayerSnapshot.Capture(x,9))).ToArray()});
   restored&=peer.RouteCode==state.RouteCode&&peer.Player(0).CurrentHole==p.CurrentHole&&peer.Player(1).CurrentHole==1;
   while(!p.IsFinished)state.EnterHole(0,p.CurrentHole,2+p.CompletedHoleCount);
   routes&=p.CompletedHoleCount==5&&p.IsFinished;
  }
  Check(coverage,"each seeded bag covers all five events");Check(boundaries,"no immediate repeat across bag boundaries");
  Check(routes&&varied>80,"shared five-hole route starts at one with no duplicate destinations");Check(restored,"route and individual progress survive snapshot restore");
  var match=new GolfMatchState();match.Start(new[]{new GolfEntrant(0,"A")},0);var schedule=new GolfEventSchedule();schedule.Begin(0,91);
  var seen=new System.Collections.Generic.HashSet<GolfEventKind>();for(int i=0;i<5;i++){schedule.Advance(match,50+i*60,1);Check(schedule.WarningActive(50+i*60)&&schedule.Snapshot.start==60+i*60,"scheduled warning "+i);seen.Add(schedule.Active(60+i*60));}
  Check(seen.Count==5,"live schedule covers five different events at sixty-second intervals");
  var source=new GolfMatchSnapshot{round=3,routeCode=15324,events=schedule.Snapshot};
  using(var writer=new FastBufferWriter(2048,Allocator.Temp)){writer.WriteNetworkSerializable(source);using(var reader=new FastBufferReader(writer,Allocator.Temp)){reader.ReadNetworkSerializable(out GolfMatchSnapshot copy);Check(source.Equals(copy),"random course and event bag serialize exactly");}}
  var ball=new GolfBallSnapshot{valid=true,hazardLaunch=13,hazardHeld=true,tornadoStamp=7,rotation=Quaternion.identity};
  using(var writer=new FastBufferWriter(2048,Allocator.Temp)){writer.WriteNetworkSerializable(ball);using(var reader=new FastBufferReader(writer,Allocator.Temp)){reader.ReadNetworkSerializable(out GolfBallSnapshot copy);Check(ball.Equals(copy)&&copy.hazardLaunch==13,"hazard flight cue reaches other devices");}}
  Check(FishingAbilityRules.Duration(FishingAbility.FishingBuddy)==8&&FishingAbilityRules.Cooldown(FishingAbility.FishingBuddy)==22&&FishingAbilityRules.Duration(FishingAbility.SchoolCall)==15,"buffed Fishing skill windows and cooldowns");
  Check(BasketballCelebrationPoses.Defaults().Length>=4,"shared team and winner celebration takes exist");
  Debug.Log("ROUND_POLISH_CHECKS_OK checks="+checks);
 }
}
