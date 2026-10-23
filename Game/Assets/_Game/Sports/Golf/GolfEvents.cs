using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;

namespace WhatTheFish {
 public enum GolfEventKind:byte { None=0,TropicalWind=1,StickyGreen=3,BounceParty=4,Tornado=5,CrabCrossing=6 }
 // Conditions are captured per shot; preview and authority share this response.
 public struct GolfShotConditions:INetworkSerializable,IEquatable<GolfShotConditions> {
  public GolfEventKind kind;public Vector3 wind;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref kind);s.SerializeValue(ref wind);}
  public float GravityScale=>1;
  public float RollScale=>1;
  public bool StopsOnGround=>kind==GolfEventKind.StickyGreen;
  public Vector3 AirAcceleration=>Physics.gravity*GravityScale+wind;
  public Vector3 Landing(GolfBallPhysicsSettings settings,Vector3 velocity,Vector3 normal,bool first){
   if(StopsOnGround)return Vector3.zero;
   if(kind!=GolfEventKind.BounceParty)return settings.LandingVelocity(velocity,normal,first);
   float incoming=Mathf.Max(0,-Vector3.Dot(velocity,normal));
   return Vector3.ProjectOnPlane(velocity,normal)*.75f+normal*(incoming>settings.minimumBounceSpeed?incoming*.8f:0);
  }
  public bool Equals(GolfShotConditions other)=>kind==other.kind&&wind==other.wind;
 }
 public struct GolfEventSnapshot:INetworkSerializable,IEquatable<GolfEventSnapshot> {
  public GolfEventKind kind,last;public uint sequence,seed;public double start,end,nextCycle,rushAt;public Vector3 wind;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {s.SerializeValue(ref kind);s.SerializeValue(ref last);s.SerializeValue(ref sequence);s.SerializeValue(ref seed);s.SerializeValue(ref start);s.SerializeValue(ref end);s.SerializeValue(ref nextCycle);s.SerializeValue(ref rushAt);s.SerializeValue(ref wind);}
  public bool Equals(GolfEventSnapshot b)=>kind==b.kind&&last==b.last&&sequence==b.sequence&&seed==b.seed&&start==b.start&&end==b.end&&nextCycle==b.nextCycle&&rushAt==b.rushAt&&wind==b.wind;
 }
 public sealed class GolfEventSchedule {
  public const double Cycle=60,Warning=10,RushDelay=1,RushDuration=30;
  public const double WeatherDuration=45;
  public const float WindAcceleration=2.6f;
  public static IReadOnlyList<GolfEventKind> RandomKinds {get;}=Array.AsReadOnly(new[]{GolfEventKind.TropicalWind,GolfEventKind.StickyGreen,GolfEventKind.BounceParty,GolfEventKind.Tornado,GolfEventKind.CrabCrossing});
  public static bool IsRandom(GolfEventKind kind)=>kind is GolfEventKind.TropicalWind or GolfEventKind.StickyGreen or GolfEventKind.BounceParty or GolfEventKind.Tornado or GolfEventKind.CrabCrossing;
  public GolfEventSnapshot Snapshot {get;private set;}
  public static double Duration(GolfEventKind kind)=>IsRandom(kind)?WeatherDuration:0;
  // Only the authority starts a schedule. Use independent OS entropy per match;
  // keep the explicit-seed overload for reproducible development fixtures.
  public void Begin(double started){
   var bytes=new byte[4];using(var random=RandomNumberGenerator.Create())random.GetBytes(bytes);
   Begin(started,BitConverter.ToUInt32(bytes,0));
  }
  public void Begin(double started,uint seed){Snapshot=new GolfEventSnapshot{nextCycle=started+Cycle,seed=seed};}
  public void Receive(GolfEventSnapshot value){Snapshot=value;}
  public void Clear(){Snapshot=default;}
  // A seeded bag covers every event once. Its boundary cannot repeat the
  // preceding bag's last item; sequence + seed also survive host migration.
  static GolfEventKind[] Bag(uint seed,uint bag){
   var result=new GolfEventKind[RandomKinds.Count];for(int i=0;i<result.Length;i++)result[i]=RandomKinds[i];
   var rng=new System.Random(unchecked((int)(seed+bag*104729)));
   for(int i=result.Length-1;i>0;i--){int j=rng.Next(i+1);(result[i],result[j])=(result[j],result[i]);}return result;
  }
  public static GolfEventKind ScheduledKind(uint seed,uint sequence){
   uint bag=sequence/(uint)RandomKinds.Count;int slot=(int)(sequence%(uint)RandomKinds.Count);var order=Bag(seed,bag);
   if(bag>0&&order[0]==Bag(seed,bag-1)[^1])(order[0],order[1])=(order[1],order[0]);return order[slot];
  }
  public GolfEventKind Active(double now)=>IsRandom(Snapshot.kind)&&now>=Snapshot.start&&now<Snapshot.end?Snapshot.kind:GolfEventKind.None;
  public bool WarningActive(double now)=>IsRandom(Snapshot.kind)&&now<Snapshot.start&&now>=Snapshot.start-Warning;
  public bool RushActive(double now)=>Snapshot.rushAt>0&&now>=Snapshot.rushAt&&now<Snapshot.rushAt+RushDuration;
  public GolfShotConditions Conditions(double now,GolfShotMode mode){
   var kind=Active(now);if(mode==GolfShotMode.Putt&&(kind==GolfEventKind.StickyGreen||kind==GolfEventKind.BounceParty))kind=GolfEventKind.None;
   return new GolfShotConditions{kind=kind,wind=kind==GolfEventKind.TropicalWind&&mode==GolfShotMode.Swing?Snapshot.wind:Vector3.zero};
  }
  public void Advance(GolfMatchState match,double now,int availablePlayers){
   var state=Snapshot;
   if(match.CountdownStarted&&state.rushAt==0)state.rushAt=match.CountdownDeadline-GolfMatchState.FinalCountdownSeconds+RushDelay;
   if(match.Phase!=GolfMatchPhase.Playing){
    // Preserve a running event to its original expiry, cancel only future/warned ones.
    if(now<state.start){state.kind=GolfEventKind.None;state.start=state.end=0;}
    state.nextCycle=0;Snapshot=state;return;
   }
   if(state.nextCycle<=0||now<state.nextCycle-Warning||state.start==state.nextCycle){Snapshot=state;return;}
   var rng=new System.Random(unchecked((int)(state.seed+state.sequence*7919)));
   var kind=ScheduledKind(state.seed,state.sequence);
   state.sequence++;state.kind=state.last=kind;state.start=state.nextCycle;state.end=state.start+Duration(kind);
   float angle=(float)(rng.NextDouble()*Math.PI*2);state.wind=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*WindAcceleration;
   state.nextCycle+=Cycle;while(state.nextCycle<=now)state.nextCycle+=Cycle;Snapshot=state;
  }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public void ReviewEvent(GolfEventKind kind,double now,Vector3 wind=default){if(!IsRandom(kind))return;var state=Snapshot;state.kind=kind;state.start=now;state.end=now+Duration(kind);state.wind=wind;state.sequence++;Snapshot=state;}
#endif
 }
}
