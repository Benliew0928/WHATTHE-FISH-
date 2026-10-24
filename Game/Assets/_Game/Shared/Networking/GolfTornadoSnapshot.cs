using System;
using Unity.Netcode;
using UnityEngine;
namespace WhatTheFish {
 // Host-owned motion and a 6.4-second/10 Hz sand source history. Clients never
 // select waypoints or decide whether a ball touches the tornado.
 public struct GolfTornadoSnapshot:INetworkSerializable,IEquatable<GolfTornadoSnapshot> {
  public ulong owner,sandTrail;public uint round,sequence,revision,random,ballReset;public int hole;
  public Vector3 position,velocity,goal;public double time,steerAt,appearAt;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter {
   s.SerializeValue(ref owner);s.SerializeValue(ref sandTrail);s.SerializeValue(ref round);s.SerializeValue(ref sequence);s.SerializeValue(ref revision);s.SerializeValue(ref random);s.SerializeValue(ref ballReset);s.SerializeValue(ref hole);s.SerializeValue(ref position);s.SerializeValue(ref velocity);s.SerializeValue(ref goal);s.SerializeValue(ref time);s.SerializeValue(ref steerAt);s.SerializeValue(ref appearAt);
  }
  public bool Equals(GolfTornadoSnapshot b)=>owner==b.owner&&sandTrail==b.sandTrail&&round==b.round&&sequence==b.sequence&&revision==b.revision&&random==b.random&&ballReset==b.ballReset&&hole==b.hole&&position==b.position&&velocity==b.velocity&&goal==b.goal&&time==b.time&&steerAt==b.steerAt&&appearAt==b.appearAt;
 }
}
